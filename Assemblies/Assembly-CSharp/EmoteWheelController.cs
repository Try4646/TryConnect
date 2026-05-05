using System;
using System.Collections.Generic;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using Gilzoide.UpdateManager;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// Token: 0x0200029C RID: 668
public class EmoteWheelController : MonoBehaviour, IUpdatable, IManagedObject
{
	// Token: 0x060017BD RID: 6077 RVA: 0x00064708 File Offset: 0x00062908
	private void Awake()
	{
		this.canvasGroup = base.GetComponent<CanvasGroup>();
		if (this.canvasGroup == null)
		{
			this.canvasGroup = base.gameObject.AddComponent<CanvasGroup>();
		}
		if (this.radialMenu == null)
		{
			this.radialMenu = base.GetComponent<RMF_RadialMenu>();
		}
		this.canvasGroup.alpha = 0f;
		this.canvasGroup.interactable = false;
		this.canvasGroup.blocksRaycasts = false;
	}

	// Token: 0x060017BE RID: 6078 RVA: 0x00064782 File Offset: 0x00062982
	private void OnEnable()
	{
		InputEvents.OnEmoteWheelEvent = (Action<bool>)Delegate.Combine(InputEvents.OnEmoteWheelEvent, new Action<bool>(this.OnEmoteWheelPressed));
	}

	// Token: 0x060017BF RID: 6079 RVA: 0x000647A4 File Offset: 0x000629A4
	private void OnDisable()
	{
		InputEvents.OnEmoteWheelEvent = (Action<bool>)Delegate.Remove(InputEvents.OnEmoteWheelEvent, new Action<bool>(this.OnEmoteWheelPressed));
	}

	// Token: 0x060017C0 RID: 6080 RVA: 0x000048A7 File Offset: 0x00002AA7
	public void ManagedUpdate()
	{
	}

	// Token: 0x060017C1 RID: 6081 RVA: 0x000647C6 File Offset: 0x000629C6
	private void OnEmoteWheelPressed(bool isPressed)
	{
		if (isPressed)
		{
			this.ShowEmoteWheel();
			return;
		}
		this.HideEmoteWheel();
	}

	// Token: 0x060017C2 RID: 6082 RVA: 0x000647D8 File Offset: 0x000629D8
	private void ShowEmoteWheel()
	{
		if (this.isEmoteWheelActive)
		{
			return;
		}
		this.isEmoteWheelActive = true;
		if (this.radialMenu != null)
		{
			if (!this.radialMenu.useDeltaSelection)
			{
				this.hoveredIndexOnOpen = this.GetHoveredElementIndex();
				if (this.hoveredIndexOnOpen < 0)
				{
					this.hoveredIndexOnOpen = this.radialMenu.index;
				}
			}
			else
			{
				this.hoveredIndexOnOpen = this.radialMenu.index;
			}
		}
		else
		{
			this.hoveredIndexOnOpen = -1;
		}
		if (this.radialMenu != null && this.radialMenu.useDeltaSelection)
		{
			this.radialMenu.SetDeltaModeActive(true);
			UICursorSimple instance = UICursorSimple.Instance;
			if (instance != null)
			{
				instance.HideCursor();
			}
		}
		else if (this.unlockCursorOnActive)
		{
			this.previousCursorLockMode = Cursor.lockState;
			this.previousCursorVisible = Cursor.visible;
			UICursorSimple instance2 = UICursorSimple.Instance;
			if (instance2 != null)
			{
				instance2.ShowCursor();
			}
		}
		if (this.lockCameraOnActive && this.playerHead != null)
		{
			this.previousCameraLocked = this.playerHead.isLocked;
			this.playerHead.isLocked = true;
		}
		this.canvasGroup.interactable = true;
		this.canvasGroup.blocksRaycasts = true;
		this.canvasGroup.DOFade(1f, this.animationDuration).SetEase(this.animationEase).OnComplete(delegate
		{
			this.canvasGroup.alpha = 1f;
		});
	}

	// Token: 0x060017C3 RID: 6083 RVA: 0x00064938 File Offset: 0x00062B38
	private void HideEmoteWheel()
	{
		if (!this.isEmoteWheelActive)
		{
			return;
		}
		if (this.radialMenu != null)
		{
			if (!this.radialMenu.useDeltaSelection)
			{
				int hoveredElementIndex = this.GetHoveredElementIndex();
				if (hoveredElementIndex >= 0)
				{
					this.capturedIndex = hoveredElementIndex;
				}
				else
				{
					this.capturedIndex = this.radialMenu.index;
				}
				if (this.hoveredIndexOnOpen >= 0 && this.capturedIndex == this.hoveredIndexOnOpen)
				{
					int lastSelectedIndex = this.radialMenu.GetLastSelectedIndex();
					if (lastSelectedIndex >= 0 && lastSelectedIndex < this.radialMenu.elements.Count)
					{
						this.capturedIndex = lastSelectedIndex;
					}
				}
				Cursor.lockState = CursorLockMode.Locked;
				UICursorSimple instance = UICursorSimple.Instance;
				if (instance != null)
				{
					instance.HideCursor();
				}
			}
			else
			{
				this.capturedIndex = this.radialMenu.index;
				if (this.hoveredIndexOnOpen >= 0 && this.capturedIndex == this.hoveredIndexOnOpen)
				{
					int lastSelectedIndex2 = this.radialMenu.GetLastSelectedIndex();
					if (lastSelectedIndex2 >= 0 && lastSelectedIndex2 < this.radialMenu.elements.Count)
					{
						this.capturedIndex = lastSelectedIndex2;
					}
				}
			}
		}
		this.isEmoteWheelActive = false;
		this.canvasGroup.DOKill(false);
		if (this.radialMenu != null && this.radialMenu.useDeltaSelection)
		{
			this.radialMenu.SetDeltaModeActive(false);
			UICursorSimple instance2 = UICursorSimple.Instance;
			if (instance2 != null)
			{
				instance2.HideCursor();
			}
		}
		if (this.lockCameraOnActive && this.playerHead != null)
		{
			this.playerHead.isLocked = this.previousCameraLocked;
		}
		this.canvasGroup.interactable = false;
		this.canvasGroup.blocksRaycasts = false;
		this.canvasGroup.alpha = 0f;
		this.ExecuteCapturedEmote();
	}

	// Token: 0x060017C4 RID: 6084 RVA: 0x00064ADC File Offset: 0x00062CDC
	private void ExecuteCapturedEmote()
	{
		if (this.radialMenu != null && this.radialMenu.elements != null && this.capturedIndex >= 0 && this.capturedIndex < this.radialMenu.elements.Count)
		{
			RMF_RadialMenuElement rmf_RadialMenuElement = this.radialMenu.elements[this.capturedIndex];
			if (rmf_RadialMenuElement != null && rmf_RadialMenuElement.button != null)
			{
				Button.ButtonClickedEvent onClick = rmf_RadialMenuElement.button.onClick;
				if (onClick != null)
				{
					onClick.Invoke();
				}
				this.radialMenu.SetLastSelectedIndex(this.capturedIndex);
			}
		}
	}

	// Token: 0x060017C5 RID: 6085 RVA: 0x000647C6 File Offset: 0x000629C6
	public void SetEmoteWheelActive(bool active)
	{
		if (active)
		{
			this.ShowEmoteWheel();
			return;
		}
		this.HideEmoteWheel();
	}

	// Token: 0x1700021D RID: 541
	// (get) Token: 0x060017C6 RID: 6086 RVA: 0x00064B7D File Offset: 0x00062D7D
	public bool IsEmoteWheelActive
	{
		get
		{
			return this.isEmoteWheelActive;
		}
	}

	// Token: 0x1700021E RID: 542
	// (get) Token: 0x060017C7 RID: 6087 RVA: 0x00064B85 File Offset: 0x00062D85
	public int CapturedIndex
	{
		get
		{
			return this.capturedIndex;
		}
	}

	// Token: 0x060017C8 RID: 6088 RVA: 0x00064B8D File Offset: 0x00062D8D
	public void SetCameraLocked(bool locked)
	{
		if (this.playerHead != null)
		{
			this.playerHead.isLocked = locked;
		}
	}

	// Token: 0x1700021F RID: 543
	// (get) Token: 0x060017C9 RID: 6089 RVA: 0x00064BA9 File Offset: 0x00062DA9
	public bool IsCameraLocked
	{
		get
		{
			return this.playerHead != null && this.playerHead.isLocked;
		}
	}

	// Token: 0x060017CA RID: 6090 RVA: 0x00064BC8 File Offset: 0x00062DC8
	private int GetHoveredElementIndex()
	{
		if (this.radialMenu == null || this.radialMenu.elements == null)
		{
			return -1;
		}
		if (EventSystem.current != null)
		{
			PointerEventData pointerEventData = new PointerEventData(EventSystem.current);
			pointerEventData.position = CursorPointerInput.ScreenPosition;
			List<RaycastResult> list = new List<RaycastResult>();
			EventSystem.current.RaycastAll(pointerEventData, list);
			foreach (RaycastResult raycastResult in list)
			{
				RMF_RadialMenuElement componentInParent = raycastResult.gameObject.GetComponentInParent<RMF_RadialMenuElement>();
				if (componentInParent != null && this.radialMenu.elements.Contains(componentInParent))
				{
					return componentInParent.assignedIndex;
				}
			}
			return -1;
		}
		return -1;
	}

	// Token: 0x04000F5A RID: 3930
	[Header("Radial Menu Reference")]
	[Tooltip("Reference to the RMF_RadialMenu component")]
	public RMF_RadialMenu radialMenu;

	// Token: 0x04000F5B RID: 3931
	[Header("Animation Settings")]
	[Tooltip("Duration for the fade in/out animation")]
	public float animationDuration = 0.2f;

	// Token: 0x04000F5C RID: 3932
	[Tooltip("Easing type for the animation")]
	public Ease animationEase = Ease.OutCubic;

	// Token: 0x04000F5D RID: 3933
	[Header("Cursor Settings")]
	[Tooltip("Should the cursor be unlocked when emote wheel is active?")]
	public bool unlockCursorOnActive = true;

	// Token: 0x04000F5E RID: 3934
	[Header("Camera Settings")]
	[Tooltip("Should the camera be locked when emote wheel is active?")]
	public bool lockCameraOnActive = true;

	// Token: 0x04000F5F RID: 3935
	[Tooltip("Reference to the PlayerHead component for camera control (must be manually assigned)")]
	public PlayerHead playerHead;

	// Token: 0x04000F60 RID: 3936
	private CanvasGroup canvasGroup;

	// Token: 0x04000F61 RID: 3937
	private bool isEmoteWheelActive;

	// Token: 0x04000F62 RID: 3938
	private int capturedIndex;

	// Token: 0x04000F63 RID: 3939
	private int hoveredIndexOnOpen = -1;

	// Token: 0x04000F64 RID: 3940
	private CursorLockMode previousCursorLockMode;

	// Token: 0x04000F65 RID: 3941
	private bool previousCursorVisible;

	// Token: 0x04000F66 RID: 3942
	private bool previousCameraLocked;
}
