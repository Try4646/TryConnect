using System;
using System.Collections.Generic;
using Extensions;
using JetBrains.Annotations;
using Mirror;
using UnityEngine;

// Token: 0x02000201 RID: 513
public class PlayerInteract : NetworkBehaviour
{
	// Token: 0x14000013 RID: 19
	// (add) Token: 0x060012C4 RID: 4804 RVA: 0x000513A4 File Offset: 0x0004F5A4
	// (remove) Token: 0x060012C5 RID: 4805 RVA: 0x000513DC File Offset: 0x0004F5DC
	public event Action<IInteractable> OnTargetInteractableChanged;

	// Token: 0x170001B1 RID: 433
	// (get) Token: 0x060012C6 RID: 4806 RVA: 0x00051414 File Offset: 0x0004F614
	private bool IsLocked
	{
		get
		{
			bool flag = this._pi.NetworkholdingItem || (this._pc.State == PlayerController.PlayerState.Ragdoll && this._pc.hasBody);
			if (this._isLocked != flag)
			{
				this._isLocked = flag;
				if (this._isLocked)
				{
					this.SetTargetInteractable(null);
					if (MonoSingleton<CursorManager>.Instance != null)
					{
						MonoSingleton<CursorManager>.Instance.SetCursorType(CursorManager.CursorType.Default);
					}
				}
			}
			return this._isLocked;
		}
	}

	// Token: 0x060012C7 RID: 4807 RVA: 0x00051490 File Offset: 0x0004F690
	private void Awake()
	{
		this._cam = MonoSingleton<LocalManager>.Instance.mainCamera;
		this._pc = base.GetComponent<PlayerController>();
		this._pi = base.GetComponent<PlayerInventory>();
		this._playerHandButtonAnimation = base.GetComponent<PlayerHandButtonAnimation>();
		this._priceDisplayManager = Object.FindFirstObjectByType<ItemPriceDisplayManager>();
		this._descriptionDisplayManager = Object.FindFirstObjectByType<ItemDescriptionDisplayManager>();
	}

	// Token: 0x060012C8 RID: 4808 RVA: 0x000514E7 File Offset: 0x0004F6E7
	private void OnEnable()
	{
		InputEvents.OnInteractEvent = (Action<bool>)Delegate.Combine(InputEvents.OnInteractEvent, new Action<bool>(this.OnInteractEvent));
		this.OnTargetInteractableChanged += this.OnTargetInteractableChangedHandler;
	}

	// Token: 0x060012C9 RID: 4809 RVA: 0x0005151B File Offset: 0x0004F71B
	private void OnDisable()
	{
		InputEvents.OnInteractEvent = (Action<bool>)Delegate.Remove(InputEvents.OnInteractEvent, new Action<bool>(this.OnInteractEvent));
		this.OnTargetInteractableChanged -= this.OnTargetInteractableChangedHandler;
	}

	// Token: 0x060012CA RID: 4810 RVA: 0x0005154F File Offset: 0x0004F74F
	public override void OnStartClient()
	{
		base.OnStartClient();
		if (!base.isLocalPlayer)
		{
			base.enabled = false;
			return;
		}
		if (MonoSingleton<LocalManager>.Instance != null)
		{
			this._interactionUIPanel = MonoSingleton<LocalManager>.Instance.interactionUIPanel;
		}
		MonoSingleton<CursorManager>.Instance.SetCursorType(CursorManager.CursorType.Default);
	}

	// Token: 0x060012CB RID: 4811 RVA: 0x0005158F File Offset: 0x0004F78F
	private void Update()
	{
		if (this.IsLocked)
		{
			return;
		}
		this.RaycastInteractable();
		this.HoldInteract();
		this.UpdateInteractionUI();
	}

	// Token: 0x060012CC RID: 4812 RVA: 0x000515AC File Offset: 0x0004F7AC
	private void RaycastInteractable()
	{
		int num = Physics.SphereCastNonAlloc(new Ray(this._cam.transform.position, this._cam.transform.forward), this.raycastRadius, this._hits, this.raycastDistance, this.raycastLayer);
		Array.Sort<RaycastHit>(this._hits, 0, num, Comparer<RaycastHit>.Create((RaycastHit a, RaycastHit b) => a.distance.CompareTo(b.distance)));
		IInteractable targetInteractable = null;
		float num2 = float.MaxValue;
		float num3 = this.raycastDistance + 1f;
		for (int i = 0; i < num; i++)
		{
			RaycastHit raycastHit = this._hits[i];
			if (!(raycastHit.transform == base.transform))
			{
				if (raycastHit.distance > num3 + this.raycastBlockThreshold)
				{
					break;
				}
				IInteractable interactable;
				if (raycastHit.transform.TryGetComponent<IInteractable>(out interactable))
				{
					Vector3 to = raycastHit.transform.position - this._cam.transform.position;
					float num4 = Vector3.Angle(this._cam.transform.forward, to);
					if (num4 < num2)
					{
						num2 = num4;
						targetInteractable = interactable;
					}
				}
				if (!raycastHit.collider.isTrigger && num3 >= this.raycastDistance)
				{
					num3 = raycastHit.distance;
				}
			}
		}
		this.SetTargetInteractable(targetInteractable);
	}

	// Token: 0x060012CD RID: 4813 RVA: 0x00051714 File Offset: 0x0004F914
	private void SetTargetInteractable([CanBeNull] IInteractable interactable)
	{
		if (this.TargetInteractable == interactable)
		{
			return;
		}
		if (this._currentlyHoveredItem != null)
		{
			if (this._priceDisplayManager != null)
			{
				this._priceDisplayManager.HidePriceForItem(this._currentlyHoveredItem);
			}
			if (this._descriptionDisplayManager != null)
			{
				this._descriptionDisplayManager.HideDescriptionForItem(this._currentlyHoveredItem);
			}
			this._currentlyHoveredItem = null;
		}
		IInteractable targetInteractable = this.TargetInteractable;
		if (targetInteractable != null)
		{
			targetInteractable.OnHoverExit(this);
		}
		if (this.TargetInteractable != null)
		{
			this.TargetInteractable.OnInteractableChanged -= this.OnTargetInteractableDataChangedHandler;
		}
		this.TargetInteractable = interactable;
		if (this.TargetInteractable != null)
		{
			this.TargetInteractable.OnInteractableChanged += this.OnTargetInteractableDataChangedHandler;
		}
		IInteractable targetInteractable2 = this.TargetInteractable;
		if (targetInteractable2 != null)
		{
			targetInteractable2.OnHover(this);
		}
		Item item = interactable as Item;
		if (item != null && (item.spawnableSo != null || item.GetComponent<GachaSphere>() != null))
		{
			this._currentlyHoveredItem = item;
			if (this._priceDisplayManager != null)
			{
				this._priceDisplayManager.ShowPriceForItem(item);
			}
			if (this._descriptionDisplayManager != null && item.ShouldShowHoverDescription)
			{
				this._descriptionDisplayManager.ShowDescriptionForItem(item);
			}
		}
		this.ResetInteract();
		Action<IInteractable> onTargetInteractableChanged = this.OnTargetInteractableChanged;
		if (onTargetInteractableChanged == null)
		{
			return;
		}
		onTargetInteractableChanged(this.TargetInteractable);
	}

	// Token: 0x060012CE RID: 4814 RVA: 0x00051872 File Offset: 0x0004FA72
	private void OnTargetInteractableDataChangedHandler(IInteractable interactable)
	{
		IInteractable targetInteractable = this.TargetInteractable;
		if (targetInteractable != null)
		{
			targetInteractable.OnHoverExit(this);
		}
		IInteractable targetInteractable2 = this.TargetInteractable;
		if (targetInteractable2 != null)
		{
			targetInteractable2.OnHover(this);
		}
		Action<IInteractable> onTargetInteractableChanged = this.OnTargetInteractableChanged;
		if (onTargetInteractableChanged == null)
		{
			return;
		}
		onTargetInteractableChanged(interactable);
	}

	// Token: 0x060012CF RID: 4815 RVA: 0x000518AC File Offset: 0x0004FAAC
	private void HoldInteract()
	{
		if (!InputEvents.IsInteractPressed)
		{
			return;
		}
		if (this.TargetInteractable == null)
		{
			return;
		}
		if (!this.TargetInteractable.IsInteractable)
		{
			return;
		}
		if (!this.TargetInteractable.MeetRequirements)
		{
			return;
		}
		if (!this.TargetInteractable.HoldInteract)
		{
			return;
		}
		if (this._hasHold)
		{
			return;
		}
		if (this._holdInteractTimer < this.TargetInteractable.HoldDuration)
		{
			this._holdInteractTimer += Time.deltaTime;
			IInteractable targetInteractable = this.TargetInteractable;
			if (targetInteractable != null)
			{
				targetInteractable.OnHold(this);
			}
			if (this.TargetInteractable != null)
			{
				this.TargetInteractable.HoldProgress = Mathf.Clamp01(this._holdInteractTimer / this.TargetInteractable.HoldDuration);
				return;
			}
		}
		else
		{
			this._hasHold = true;
			this._holdInteractTimer = 0f;
			IInteractable targetInteractable2 = this.TargetInteractable;
			if (targetInteractable2 != null)
			{
				targetInteractable2.OnHoldExit(this);
			}
			IInteractable targetInteractable3 = this.TargetInteractable;
			if (targetInteractable3 == null)
			{
				return;
			}
			targetInteractable3.OnInteract(this);
		}
	}

	// Token: 0x060012D0 RID: 4816 RVA: 0x00051994 File Offset: 0x0004FB94
	private void UpdateInteractionUI()
	{
		if (this._interactionUIPanel == null || this.TargetInteractable == null)
		{
			return;
		}
		if (this.TargetInteractable.IsBeingHold && this.TargetInteractable.HoldInteract)
		{
			this._interactionUIPanel.UpdateProgressBar(this.TargetInteractable.HoldProgress);
			return;
		}
		this._interactionUIPanel.UpdateProgressBar(0f);
	}

	// Token: 0x060012D1 RID: 4817 RVA: 0x000519F9 File Offset: 0x0004FBF9
	private void OnTargetInteractableChangedHandler(IInteractable interactable)
	{
		this.UpdateInteractionUI(interactable);
		this.UpdateCursor(interactable);
	}

	// Token: 0x060012D2 RID: 4818 RVA: 0x00051A0C File Offset: 0x0004FC0C
	private void UpdateInteractionUI(IInteractable interactable)
	{
		if (this._interactionUIPanel == null)
		{
			return;
		}
		if (interactable != null && interactable.IsInteractable && interactable.MeetRequirements)
		{
			this._interactionUIPanel.SetItemNameText(interactable.InteractableName);
			this._interactionUIPanel.SetTooltip(interactable.TooltipMessage);
			this._interactionUIPanel.UpdateProgressBar(0f);
			return;
		}
		this._interactionUIPanel.ResetUI();
	}

	// Token: 0x060012D3 RID: 4819 RVA: 0x00051A7C File Offset: 0x0004FC7C
	private void UpdateCursor(IInteractable interactable)
	{
		if (MonoSingleton<CursorManager>.Instance == null)
		{
			return;
		}
		if (interactable == null || !interactable.IsInteractable || !interactable.MeetRequirements)
		{
			MonoSingleton<CursorManager>.Instance.SetCursorType(CursorManager.CursorType.Default);
			return;
		}
		CursorManager.CursorType cursorType = interactable.CursorType;
		MonoSingleton<CursorManager>.Instance.SetCursorType(cursorType);
	}

	// Token: 0x060012D4 RID: 4820 RVA: 0x00051AC8 File Offset: 0x0004FCC8
	private void OnInteractEvent(bool performed)
	{
		if (!performed)
		{
			this.ResetInteract();
			return;
		}
		if (this.TargetInteractable == null)
		{
			return;
		}
		if (!this.TargetInteractable.IsInteractable)
		{
			return;
		}
		if (!this.TargetInteractable.MeetRequirements)
		{
			return;
		}
		if (this.TargetInteractable.HoldInteract)
		{
			return;
		}
		if (this._playerHandButtonAnimation != null && !this._pi.NetworkholdingItem && !(this.TargetInteractable is Item))
		{
			MonoBehaviour monoBehaviour = this.TargetInteractable as MonoBehaviour;
			if (monoBehaviour != null)
			{
				this._playerHandButtonAnimation.PressButton(monoBehaviour.transform);
			}
		}
		IInteractable targetInteractable = this.TargetInteractable;
		if (targetInteractable == null)
		{
			return;
		}
		targetInteractable.OnInteract(this);
	}

	// Token: 0x060012D5 RID: 4821 RVA: 0x00051B7C File Offset: 0x0004FD7C
	private void ResetInteract()
	{
		this._holdInteractTimer = 0f;
		this._hasHold = false;
		IInteractable targetInteractable = this.TargetInteractable;
		if (targetInteractable != null)
		{
			targetInteractable.OnHoldExit(this);
		}
		if (this._interactionUIPanel != null && this.TargetInteractable != null)
		{
			this._interactionUIPanel.UpdateProgressBar(0f);
		}
	}

	// Token: 0x060012D7 RID: 4823 RVA: 0x00002321 File Offset: 0x00000521
	public override bool Weaved()
	{
		return true;
	}

	// Token: 0x04000BFA RID: 3066
	public float raycastDistance = 3f;

	// Token: 0x04000BFB RID: 3067
	public float raycastRadius = 0.1f;

	// Token: 0x04000BFC RID: 3068
	public float raycastBlockThreshold = 0.5f;

	// Token: 0x04000BFD RID: 3069
	public LayerMask raycastLayer = -1;

	// Token: 0x04000BFE RID: 3070
	public IInteractable TargetInteractable;

	// Token: 0x04000BFF RID: 3071
	private readonly RaycastHit[] _hits = new RaycastHit[16];

	// Token: 0x04000C00 RID: 3072
	private float _holdInteractTimer;

	// Token: 0x04000C01 RID: 3073
	private bool _hasHold;

	// Token: 0x04000C02 RID: 3074
	private PlayerInventory _pi;

	// Token: 0x04000C03 RID: 3075
	private PlayerController _pc;

	// Token: 0x04000C04 RID: 3076
	private Camera _cam;

	// Token: 0x04000C06 RID: 3078
	private bool _isLocked;

	// Token: 0x04000C07 RID: 3079
	private InteractionUIPanel _interactionUIPanel;

	// Token: 0x04000C08 RID: 3080
	private ItemPriceDisplayManager _priceDisplayManager;

	// Token: 0x04000C09 RID: 3081
	private ItemDescriptionDisplayManager _descriptionDisplayManager;

	// Token: 0x04000C0A RID: 3082
	private Item _currentlyHoveredItem;

	// Token: 0x04000C0B RID: 3083
	private PlayerHandButtonAnimation _playerHandButtonAnimation;
}
