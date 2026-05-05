using System;
using System.Collections.Generic;
using System.Linq;
using Mirror;
using UnityEngine;

// Token: 0x0200026B RID: 619
public class UIManager : MonoBehaviour
{
	// Token: 0x170001FF RID: 511
	// (get) Token: 0x060015F8 RID: 5624 RVA: 0x0005ECCB File Offset: 0x0005CECB
	// (set) Token: 0x060015F9 RID: 5625 RVA: 0x0005ECD2 File Offset: 0x0005CED2
	public static UIManager Instance { get; private set; }

	// Token: 0x060015FA RID: 5626 RVA: 0x0005ECDA File Offset: 0x0005CEDA
	private void Awake()
	{
		if (UIManager.Instance == null)
		{
			UIManager.Instance = this;
			Object.DontDestroyOnLoad(base.gameObject);
			return;
		}
		Object.Destroy(base.gameObject);
	}

	// Token: 0x060015FB RID: 5627 RVA: 0x0005ED06 File Offset: 0x0005CF06
	private void OnEnable()
	{
		InputEvents.OnEscapeMenuEvent = (Action)Delegate.Combine(InputEvents.OnEscapeMenuEvent, new Action(this.HandleEscapeKey));
	}

	// Token: 0x060015FC RID: 5628 RVA: 0x0005ED28 File Offset: 0x0005CF28
	private void OnDisable()
	{
		InputEvents.OnEscapeMenuEvent = (Action)Delegate.Remove(InputEvents.OnEscapeMenuEvent, new Action(this.HandleEscapeKey));
	}

	// Token: 0x060015FD RID: 5629 RVA: 0x0005ED4C File Offset: 0x0005CF4C
	public void RegisterUI(IUIManager ui)
	{
		if (ui != null && !this.registeredUIs.Contains(ui))
		{
			this.registeredUIs.Add(ui);
			this.registeredUIs.Sort((IUIManager a, IUIManager b) => b.Priority.CompareTo(a.Priority));
		}
	}

	// Token: 0x060015FE RID: 5630 RVA: 0x0005EDA0 File Offset: 0x0005CFA0
	public void UnregisterUI(IUIManager ui)
	{
		if (ui != null)
		{
			this.registeredUIs.Remove(ui);
			if (this.currentlyActiveUI == ui)
			{
				this.currentlyActiveUI = null;
			}
		}
	}

	// Token: 0x060015FF RID: 5631 RVA: 0x0005EDC4 File Offset: 0x0005CFC4
	private void HandleEscapeKey()
	{
		IUIManager iuimanager = this.registeredUIs.FirstOrDefault((IUIManager ui) => ui.IsActive);
		if (iuimanager != null)
		{
			iuimanager.CloseUI();
			this.currentlyActiveUI = null;
			Cursor.lockState = CursorLockMode.Locked;
			UICursorSimple instance = UICursorSimple.Instance;
			if (instance != null)
			{
				instance.HideCursor();
			}
			NetworkIdentity localPlayer = NetworkClient.localPlayer;
			if (localPlayer != null)
			{
				PlayerController component = localPlayer.GetComponent<PlayerController>();
				if (component != null && component.head != null)
				{
					component.head.isLocked = false;
				}
			}
		}
	}

	// Token: 0x06001600 RID: 5632 RVA: 0x0005EE5B File Offset: 0x0005D05B
	public void SetActiveUI(IUIManager ui)
	{
		if (this.currentlyActiveUI != null && this.currentlyActiveUI != ui)
		{
			this.currentlyActiveUI.CloseUI();
		}
		this.currentlyActiveUI = ui;
	}

	// Token: 0x06001601 RID: 5633 RVA: 0x0005EE80 File Offset: 0x0005D080
	public void ClearActiveUI(IUIManager ui)
	{
		if (this.currentlyActiveUI == ui)
		{
			this.currentlyActiveUI = null;
		}
	}

	// Token: 0x04000E68 RID: 3688
	[Header("UI References")]
	private readonly List<IUIManager> registeredUIs = new List<IUIManager>();

	// Token: 0x04000E69 RID: 3689
	private IUIManager currentlyActiveUI;
}
