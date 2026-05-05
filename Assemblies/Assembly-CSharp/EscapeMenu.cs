using System;
using Mirror;
using UnityEngine;

// Token: 0x0200029E RID: 670
public class EscapeMenu : MonoBehaviour
{
	// Token: 0x060017D0 RID: 6096 RVA: 0x00064E4C File Offset: 0x0006304C
	private void Awake()
	{
		this._canvasGroup = base.GetComponentInChildren<CanvasGroup>();
		if (this.voiceList == null)
		{
			this.voiceList = base.GetComponentInChildren<EscapeMenuVoiceList>();
		}
		this._canvasGroup.alpha = 0f;
		this._canvasGroup.interactable = false;
		this._canvasGroup.blocksRaycasts = false;
	}

	// Token: 0x060017D1 RID: 6097 RVA: 0x00064EA8 File Offset: 0x000630A8
	public void OnStuckClicked()
	{
		if (InputEvents.ActiveLayer != InputLayer.Default)
		{
			return;
		}
		NetworkIdentity localPlayer = NetworkClient.localPlayer;
		PlayerController playerController = (localPlayer != null) ? localPlayer.GetComponent<PlayerController>() : null;
		if (playerController != null)
		{
			GameObject gameObject = GameObject.Find("StuckTeleport");
			Transform transform = (gameObject != null) ? gameObject.transform : null;
			playerController.LocalTeleport((transform != null) ? transform.position : Vector3.zero);
			playerController.LocalRotate(Vector2.zero);
			this.ToggleEscapeMenu();
		}
	}

	// Token: 0x060017D2 RID: 6098 RVA: 0x00064F16 File Offset: 0x00063116
	private void OnEnable()
	{
		InputEvents.OnEscapeMenuEvent = (Action)Delegate.Combine(InputEvents.OnEscapeMenuEvent, new Action(this.ToggleEscapeMenu));
	}

	// Token: 0x060017D3 RID: 6099 RVA: 0x00064F38 File Offset: 0x00063138
	private void OnDisable()
	{
		InputEvents.OnEscapeMenuEvent = (Action)Delegate.Remove(InputEvents.OnEscapeMenuEvent, new Action(this.ToggleEscapeMenu));
	}

	// Token: 0x060017D4 RID: 6100 RVA: 0x00064F5C File Offset: 0x0006315C
	private void ToggleEscapeMenu()
	{
		if (this._isOpen)
		{
			EscapeMenuSettingsPanel escapeMenuSettingsPanel = Object.FindFirstObjectByType<EscapeMenuSettingsPanel>();
			if (escapeMenuSettingsPanel != null && escapeMenuSettingsPanel.gameObject.activeSelf)
			{
				return;
			}
			BugReportPanel bugReportPanel = Object.FindFirstObjectByType<BugReportPanel>();
			if (bugReportPanel != null && bugReportPanel.gameObject.activeSelf)
			{
				return;
			}
		}
		this._isOpen = !this._isOpen;
		PlayerController component = NetworkClient.localPlayer.GetComponent<PlayerController>();
		component.IsLocked = this._isOpen;
		component.head.isLocked = this._isOpen;
		this._canvasGroup.alpha = (float)(this._isOpen ? 1 : 0);
		this._canvasGroup.interactable = this._isOpen;
		this._canvasGroup.blocksRaycasts = this._isOpen;
		if (this._isOpen)
		{
			UICursorSimple instance = UICursorSimple.Instance;
			if (instance != null)
			{
				instance.ShowCursor();
			}
			EscapeMenuVoiceList escapeMenuVoiceList = this.voiceList;
			if (escapeMenuVoiceList == null)
			{
				return;
			}
			escapeMenuVoiceList.RefreshList();
			return;
		}
		else
		{
			UICursorSimple instance2 = UICursorSimple.Instance;
			if (instance2 == null)
			{
				return;
			}
			instance2.HideCursor();
			return;
		}
	}

	// Token: 0x04000F68 RID: 3944
	private CanvasGroup _canvasGroup;

	// Token: 0x04000F69 RID: 3945
	private bool _isOpen;

	// Token: 0x04000F6A RID: 3946
	[SerializeField]
	private EscapeMenuVoiceList voiceList;
}
