using System;
using Extensions;
using Steamworks;
using UnityEngine;
using UnityEngine.UI;

// Token: 0x0200023F RID: 575
public class DisableIfNotLobbyOwner : MonoBehaviour
{
	// Token: 0x060014C4 RID: 5316 RVA: 0x0005942C File Offset: 0x0005762C
	private void Awake()
	{
		this.button = base.GetComponent<Button>();
		if (this.button == null)
		{
			Debug.LogWarning("[DisableIfNotLobbyOwner] No Button component found on " + base.gameObject.name);
		}
		this.canvasGroup = base.GetComponent<CanvasGroup>();
		if (this.canvasGroup == null)
		{
			Debug.LogWarning("[DisableIfNotLobbyOwner] No CanvasGroup component found on " + base.gameObject.name);
		}
	}

	// Token: 0x060014C5 RID: 5317 RVA: 0x000594A4 File Offset: 0x000576A4
	private void OnEnable()
	{
		LobbyManager.OnLobbyOwnerStatusChanged += this.OnLobbyOwnerStatusChanged;
		LobbyManager.OnLobbyEnteredEvent += this.OnLobbyEntered;
		LobbyManager.OnLobbyLeftEvent += this.OnLobbyLeft;
		VersionMismatchManager.OnVersionMismatchChanged += this.OnVersionMismatchChanged;
		if (MonoSingleton<VersionMismatchManager>.Instance != null)
		{
			this.hasVersionMismatch = MonoSingleton<VersionMismatchManager>.Instance.HasVersionMismatch();
		}
	}

	// Token: 0x060014C6 RID: 5318 RVA: 0x00059514 File Offset: 0x00057714
	private void OnDisable()
	{
		LobbyManager.OnLobbyOwnerStatusChanged -= this.OnLobbyOwnerStatusChanged;
		LobbyManager.OnLobbyEnteredEvent -= this.OnLobbyEntered;
		LobbyManager.OnLobbyLeftEvent -= this.OnLobbyLeft;
		VersionMismatchManager.OnVersionMismatchChanged -= this.OnVersionMismatchChanged;
	}

	// Token: 0x060014C7 RID: 5319 RVA: 0x00059565 File Offset: 0x00057765
	private void Start()
	{
		this.CheckLobbyOwnerStatus();
		this.ApplyDisableState();
	}

	// Token: 0x060014C8 RID: 5320 RVA: 0x00059573 File Offset: 0x00057773
	private void OnLobbyOwnerStatusChanged(bool isOwner)
	{
		this.isLobbyOwner = isOwner;
		this.ApplyDisableState();
	}

	// Token: 0x060014C9 RID: 5321 RVA: 0x00059565 File Offset: 0x00057765
	private void OnLobbyEntered()
	{
		this.CheckLobbyOwnerStatus();
		this.ApplyDisableState();
	}

	// Token: 0x060014CA RID: 5322 RVA: 0x00059582 File Offset: 0x00057782
	private void OnLobbyLeft()
	{
		this.isLobbyOwner = false;
		this.hasVersionMismatch = false;
		this.ApplyDisableState();
	}

	// Token: 0x060014CB RID: 5323 RVA: 0x00059598 File Offset: 0x00057798
	private void OnVersionMismatchChanged(bool hasMismatch)
	{
		this.hasVersionMismatch = hasMismatch;
		this.ApplyDisableState();
	}

	// Token: 0x060014CC RID: 5324 RVA: 0x000595A8 File Offset: 0x000577A8
	private void CheckLobbyOwnerStatus()
	{
		if (!SteamManager.Initialized)
		{
			this.isLobbyOwner = false;
			return;
		}
		LobbySettings lobbySettings = Resources.Load<LobbySettings>("LobbySettings");
		if (lobbySettings == null || !lobbySettings.inALobby || lobbySettings.steamLobbyID == CSteamID.Nil)
		{
			this.isLobbyOwner = false;
			return;
		}
		try
		{
			CSteamID lobbyOwner = SteamMatchmaking.GetLobbyOwner(lobbySettings.steamLobbyID);
			this.isLobbyOwner = (lobbyOwner == SteamUser.GetSteamID());
		}
		catch (Exception ex)
		{
			Debug.LogWarning("[DisableIfNotLobbyOwner] Failed to check lobby owner status: " + ex.Message);
			this.isLobbyOwner = false;
		}
	}

	// Token: 0x060014CD RID: 5325 RVA: 0x0005964C File Offset: 0x0005784C
	private void ApplyDisableState()
	{
		if (!this.isLobbyOwner || this.hasVersionMismatch)
		{
			if (this.canvasGroup != null)
			{
				this.canvasGroup.alpha = this.disabledAlpha;
				this.canvasGroup.blocksRaycasts = false;
				this.canvasGroup.interactable = false;
			}
			if (this.button != null)
			{
				this.button.interactable = false;
				return;
			}
		}
		else
		{
			if (this.canvasGroup != null)
			{
				this.canvasGroup.alpha = 1f;
				this.canvasGroup.blocksRaycasts = true;
				this.canvasGroup.interactable = true;
			}
			if (this.button != null)
			{
				this.button.interactable = true;
			}
		}
	}

	// Token: 0x04000D3F RID: 3391
	[Header("Settings")]
	[Tooltip("Alpha value to set when player is not the lobby owner (0-1)")]
	[SerializeField]
	private float disabledAlpha = 0.5f;

	// Token: 0x04000D40 RID: 3392
	private Button button;

	// Token: 0x04000D41 RID: 3393
	private CanvasGroup canvasGroup;

	// Token: 0x04000D42 RID: 3394
	private bool isLobbyOwner;

	// Token: 0x04000D43 RID: 3395
	private bool hasVersionMismatch;
}
