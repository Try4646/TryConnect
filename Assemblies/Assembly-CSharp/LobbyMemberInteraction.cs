using System;
using System.Collections;
using Extensions;
using Steamworks;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// Token: 0x020002C2 RID: 706
public class LobbyMemberInteraction : MonoBehaviour, IPointerEnterHandler, IEventSystemHandler, IPointerExitHandler, IPointerClickHandler
{
	// Token: 0x060018F9 RID: 6393 RVA: 0x000691F8 File Offset: 0x000673F8
	private void Awake()
	{
		this.steamIdComponent = base.GetComponent<SteamIdComponent>();
		if (this.leaveButton != null)
		{
			this.leaveButtonCanvasGroup = this.leaveButton.GetComponent<CanvasGroup>();
		}
		if (this.kickButton != null)
		{
			this.kickButtonCanvasGroup = this.kickButton.GetComponent<CanvasGroup>();
		}
		if (this.leaveButton != null)
		{
			this.leaveButton.SetActive(false);
		}
		if (this.kickButton != null)
		{
			this.kickButton.SetActive(false);
		}
	}

	// Token: 0x060018FA RID: 6394 RVA: 0x00069284 File Offset: 0x00067484
	private void Start()
	{
		if (this.steamIdComponent != null)
		{
			ulong steamID = SteamUser.GetSteamID().m_SteamID;
			this.isLocalPlayer = (this.steamIdComponent.SteamId == steamID);
			LobbySettings lobbySettings = Resources.Load<LobbySettings>("LobbySettings");
			if (lobbySettings != null && lobbySettings.inALobby && lobbySettings.steamLobbyID != CSteamID.Nil)
			{
				CSteamID lobbyOwner = SteamMatchmaking.GetLobbyOwner(lobbySettings.steamLobbyID);
				this.isLobbyOwner = (lobbyOwner == SteamUser.GetSteamID());
			}
		}
	}

	// Token: 0x060018FB RID: 6395 RVA: 0x0006930C File Offset: 0x0006750C
	private void Update()
	{
		if (this.isHovering)
		{
			if (this.isLocalPlayer && !this.isLeaveButtonVisible)
			{
				this.ShowLeaveButton();
				return;
			}
			if (!this.isLocalPlayer && this.isLobbyOwner && !this.isKickButtonVisible)
			{
				this.ShowKickButton();
				return;
			}
		}
		else
		{
			if (this.isLeaveButtonVisible)
			{
				this.HideLeaveButton();
			}
			if (this.isKickButtonVisible)
			{
				this.HideKickButton();
			}
		}
	}

	// Token: 0x060018FC RID: 6396 RVA: 0x00069374 File Offset: 0x00067574
	private void ShowLeaveButton()
	{
		if (this.leaveButton == null || this.leaveButtonCanvasGroup == null)
		{
			return;
		}
		this.leaveButton.SetActive(true);
		this.isLeaveButtonVisible = true;
		base.StartCoroutine(this.FadeCanvasGroup(this.leaveButtonCanvasGroup, 0f, this.targetAlpha, this.fadeDuration, null));
	}

	// Token: 0x060018FD RID: 6397 RVA: 0x000693D8 File Offset: 0x000675D8
	private void HideLeaveButton()
	{
		if (this.leaveButton == null || this.leaveButtonCanvasGroup == null)
		{
			return;
		}
		this.isLeaveButtonVisible = false;
		base.StartCoroutine(this.FadeCanvasGroup(this.leaveButtonCanvasGroup, this.targetAlpha, 0f, this.fadeDuration, delegate
		{
			if (this.leaveButton != null)
			{
				this.leaveButton.SetActive(false);
			}
		}));
	}

	// Token: 0x060018FE RID: 6398 RVA: 0x0006943C File Offset: 0x0006763C
	private void ShowKickButton()
	{
		if (this.kickButton == null || this.kickButtonCanvasGroup == null)
		{
			return;
		}
		this.kickButton.SetActive(true);
		this.isKickButtonVisible = true;
		base.StartCoroutine(this.FadeCanvasGroup(this.kickButtonCanvasGroup, 0f, this.targetAlpha, this.fadeDuration, null));
	}

	// Token: 0x060018FF RID: 6399 RVA: 0x000694A0 File Offset: 0x000676A0
	private void HideKickButton()
	{
		if (this.kickButton == null || this.kickButtonCanvasGroup == null)
		{
			return;
		}
		this.isKickButtonVisible = false;
		base.StartCoroutine(this.FadeCanvasGroup(this.kickButtonCanvasGroup, this.targetAlpha, 0f, this.fadeDuration, delegate
		{
			if (this.kickButton != null)
			{
				this.kickButton.SetActive(false);
			}
		}));
	}

	// Token: 0x06001900 RID: 6400 RVA: 0x00069501 File Offset: 0x00067701
	private IEnumerator FadeCanvasGroup(CanvasGroup canvasGroup, float startAlpha, float endAlpha, float duration, Action onComplete = null)
	{
		float elapsed = 0f;
		while (elapsed < duration)
		{
			elapsed += Time.deltaTime;
			float t = elapsed / duration;
			canvasGroup.alpha = Mathf.Lerp(startAlpha, endAlpha, t);
			yield return null;
		}
		canvasGroup.alpha = endAlpha;
		if (onComplete != null)
		{
			onComplete();
		}
		yield break;
	}

	// Token: 0x06001901 RID: 6401 RVA: 0x0006952E File Offset: 0x0006772E
	public void OnPointerEnter(PointerEventData eventData)
	{
		this.isHovering = true;
	}

	// Token: 0x06001902 RID: 6402 RVA: 0x00069537 File Offset: 0x00067737
	public void OnPointerExit(PointerEventData eventData)
	{
		this.isHovering = false;
	}

	// Token: 0x06001903 RID: 6403 RVA: 0x00069540 File Offset: 0x00067740
	public void OnPointerClick(PointerEventData eventData)
	{
		if (this.isLocalPlayer && this.isHovering && this.leaveButton != null && this.leaveButton.activeSelf && this.isLeaveButtonVisible)
		{
			this.LeaveLobby();
			return;
		}
		if (!this.isLocalPlayer && this.isLobbyOwner && this.isHovering && this.kickButton != null && this.kickButton.activeSelf && this.isKickButtonVisible)
		{
			this.KickPlayer();
		}
	}

	// Token: 0x06001904 RID: 6404 RVA: 0x000695C8 File Offset: 0x000677C8
	private void LeaveLobby()
	{
		Debug.Log("Local player clicked to leave lobby");
		if (MonoSingleton<LobbyManager>.Instance != null)
		{
			MonoSingleton<LobbyManager>.Instance.ClearRichPresence();
		}
		LobbySettings lobbySettings = Resources.Load<LobbySettings>("LobbySettings");
		if (lobbySettings != null && lobbySettings.inALobby && lobbySettings.steamLobbyID != CSteamID.Nil)
		{
			if (SteamMatchmaking.GetLobbyOwner(lobbySettings.steamLobbyID) == SteamUser.GetSteamID())
			{
				Debug.Log("Host is leaving - disbanding lobby...");
				if (MonoSingleton<LobbyManager>.Instance != null)
				{
					MonoSingleton<LobbyManager>.Instance.DisbandLobby();
				}
				return;
			}
			SteamMatchmaking.LeaveLobby(lobbySettings.steamLobbyID);
			lobbySettings.inALobby = false;
			lobbySettings.steamLobbyID = CSteamID.Nil;
		}
		if (MonoSingleton<LobbyManager>.Instance != null)
		{
			MonoSingleton<LobbyManager>.Instance.CreateNewLobby();
		}
	}

	// Token: 0x06001905 RID: 6405 RVA: 0x00069694 File Offset: 0x00067894
	private void KickPlayer()
	{
		if (this.steamIdComponent == null)
		{
			return;
		}
		Debug.Log(string.Format("Lobby owner clicked to kick player: {0}", this.steamIdComponent.SteamId));
		LobbySettings lobbySettings = Resources.Load<LobbySettings>("LobbySettings");
		if (lobbySettings != null && lobbySettings.inALobby && lobbySettings.steamLobbyID != CSteamID.Nil)
		{
			CSteamID csteamID = new CSteamID(this.steamIdComponent.SteamId);
			SteamMatchmaking.SetLobbyMemberData(lobbySettings.steamLobbyID, "Kicked", "1");
			SteamMatchmaking.SetLobbyMemberData(lobbySettings.steamLobbyID, "KickTarget", csteamID.ToString());
			Debug.Log(string.Format("Player {0} has been kicked from the lobby", this.steamIdComponent.SteamId));
			base.StartCoroutine(this.CleanupKickData());
		}
	}

	// Token: 0x06001906 RID: 6406 RVA: 0x00069773 File Offset: 0x00067973
	private IEnumerator CleanupKickData()
	{
		yield return new WaitForSeconds(1f);
		LobbySettings lobbySettings = Resources.Load<LobbySettings>("LobbySettings");
		if (lobbySettings != null && lobbySettings.inALobby && lobbySettings.steamLobbyID != CSteamID.Nil)
		{
			SteamMatchmaking.SetLobbyMemberData(lobbySettings.steamLobbyID, "Kicked", "");
			SteamMatchmaking.SetLobbyMemberData(lobbySettings.steamLobbyID, "KickTarget", "");
		}
		yield break;
	}

	// Token: 0x04001012 RID: 4114
	[Header("UI References")]
	[SerializeField]
	public GameObject leaveButton;

	// Token: 0x04001013 RID: 4115
	[SerializeField]
	public GameObject kickButton;

	// Token: 0x04001014 RID: 4116
	[SerializeField]
	public RawImage profileImage;

	// Token: 0x04001015 RID: 4117
	[Header("Animation Settings")]
	[SerializeField]
	private float fadeDuration = 0.2f;

	// Token: 0x04001016 RID: 4118
	[SerializeField]
	private float targetAlpha = 1f;

	// Token: 0x04001017 RID: 4119
	private SteamIdComponent steamIdComponent;

	// Token: 0x04001018 RID: 4120
	private CanvasGroup leaveButtonCanvasGroup;

	// Token: 0x04001019 RID: 4121
	private CanvasGroup kickButtonCanvasGroup;

	// Token: 0x0400101A RID: 4122
	private bool isLocalPlayer;

	// Token: 0x0400101B RID: 4123
	private bool isHovering;

	// Token: 0x0400101C RID: 4124
	private bool isLobbyOwner;

	// Token: 0x0400101D RID: 4125
	private bool isLeaveButtonVisible;

	// Token: 0x0400101E RID: 4126
	private bool isKickButtonVisible;
}
