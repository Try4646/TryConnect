using System;
using System.Collections;
using System.Text;
using Extensions;
using Mirror;
using Steamworks;
using UnityEngine;
using UnityEngine.SceneManagement;

// Token: 0x02000222 RID: 546
public class LobbyManager : MonoSingleton<LobbyManager>
{
	// Token: 0x14000019 RID: 25
	// (add) Token: 0x060013F1 RID: 5105 RVA: 0x000551F4 File Offset: 0x000533F4
	// (remove) Token: 0x060013F2 RID: 5106 RVA: 0x00055228 File Offset: 0x00053428
	public static event Action<bool> OnLobbyOwnerStatusChanged;

	// Token: 0x1400001A RID: 26
	// (add) Token: 0x060013F3 RID: 5107 RVA: 0x0005525C File Offset: 0x0005345C
	// (remove) Token: 0x060013F4 RID: 5108 RVA: 0x00055290 File Offset: 0x00053490
	public static event Action OnLobbyEnteredEvent;

	// Token: 0x1400001B RID: 27
	// (add) Token: 0x060013F5 RID: 5109 RVA: 0x000552C4 File Offset: 0x000534C4
	// (remove) Token: 0x060013F6 RID: 5110 RVA: 0x000552F8 File Offset: 0x000534F8
	public static event Action OnLobbyLeftEvent;

	// Token: 0x060013F7 RID: 5111 RVA: 0x0005532C File Offset: 0x0005352C
	protected override void OnAwake()
	{
		base.OnAwake();
		this.lobbySettings = Resources.Load<LobbySettings>("LobbySettings");
		this.gameSettings = Resources.Load<GameSettings>("GameSettings");
		if (!SteamManager.Initialized)
		{
			Debug.LogError("Steam is not initialized!");
			base.enabled = false;
			return;
		}
		this.lobbyEnterCallback = Callback<LobbyEnter_t>.Create(new Callback<LobbyEnter_t>.DispatchDelegate(this.OnLobbyEntered));
		this.lobbyDataUpdateCallback = Callback<LobbyDataUpdate_t>.Create(new Callback<LobbyDataUpdate_t>.DispatchDelegate(this.OnLobbyDataUpdated));
		this.joinRequestCallback = Callback<GameLobbyJoinRequested_t>.Create(new Callback<GameLobbyJoinRequested_t>.DispatchDelegate(this.OnJoinRequest));
		this.lobbyChatUpdateCallback = Callback<LobbyChatUpdate_t>.Create(new Callback<LobbyChatUpdate_t>.DispatchDelegate(this.OnLobbyChatUpdate));
		this.lobbyKickedCallback = Callback<LobbyKicked_t>.Create(new Callback<LobbyKicked_t>.DispatchDelegate(this.OnLobbyKicked));
		this.richPresenceJoinRequestedCallback = Callback<GameRichPresenceJoinRequested_t>.Create(new Callback<GameRichPresenceJoinRequested_t>.DispatchDelegate(this.OnRichPresenceJoinRequested));
	}

	// Token: 0x060013F8 RID: 5112 RVA: 0x00055402 File Offset: 0x00053602
	private void OnEnable()
	{
		SettingItemBase.SettingsChanged += this.OnSettingChanged;
		this.ApplyLobbyModeFromSetting();
		InputEvents.OnF3Event = (Action)Delegate.Combine(InputEvents.OnF3Event, new Action(this.DevStart));
	}

	// Token: 0x060013F9 RID: 5113 RVA: 0x0005543B File Offset: 0x0005363B
	private void OnDisable()
	{
		SettingItemBase.SettingsChanged -= this.OnSettingChanged;
		InputEvents.OnF3Event = (Action)Delegate.Remove(InputEvents.OnF3Event, new Action(this.DevStart));
	}

	// Token: 0x060013FA RID: 5114 RVA: 0x00055470 File Offset: 0x00053670
	private void OnDestroy()
	{
		Callback<LobbyEnter_t> callback = this.lobbyEnterCallback;
		if (callback != null)
		{
			callback.Dispose();
		}
		Callback<LobbyDataUpdate_t> callback2 = this.lobbyDataUpdateCallback;
		if (callback2 != null)
		{
			callback2.Dispose();
		}
		Callback<GameLobbyJoinRequested_t> callback3 = this.joinRequestCallback;
		if (callback3 != null)
		{
			callback3.Dispose();
		}
		Callback<LobbyChatUpdate_t> callback4 = this.lobbyChatUpdateCallback;
		if (callback4 != null)
		{
			callback4.Dispose();
		}
		Callback<LobbyKicked_t> callback5 = this.lobbyKickedCallback;
		if (callback5 != null)
		{
			callback5.Dispose();
		}
		Callback<GameRichPresenceJoinRequested_t> callback6 = this.richPresenceJoinRequestedCallback;
		if (callback6 == null)
		{
			return;
		}
		callback6.Dispose();
	}

	// Token: 0x060013FB RID: 5115 RVA: 0x000554E4 File Offset: 0x000536E4
	public void StartGame()
	{
		if (this.gameSettings.gameHasStarted)
		{
			return;
		}
		if (this.lobbySettings.steamLobbyID == CSteamID.Nil)
		{
			Debug.LogWarning("Not in a lobby.");
			return;
		}
		if (SteamMatchmaking.GetLobbyOwner(this.lobbySettings.steamLobbyID) != SteamUser.GetSteamID())
		{
			Debug.LogWarning("Only the lobby owner can start the game.");
			return;
		}
		Debug.Log("Starting game as Host…");
		if (MonoSingleton<SteamRichPresenceManager>.Instance != null)
		{
			MonoSingleton<SteamRichPresenceManager>.Instance.SetInGamePresence();
		}
		if (MonoSingleton<DiscordRichPresenceManager>.Instance != null)
		{
			MonoSingleton<DiscordRichPresenceManager>.Instance.SetInGamePresence();
		}
		MonoSingleton<SceneTransitioner>.Instance.SetLoadingScreen(true, 0.5f, true);
		base.StartCoroutine(this.StartHostAfterFade());
		this.gameSettings.gameHasStarted = true;
		SteamMatchmaking.SetLobbyData(this.lobbySettings.steamLobbyID, "GameStarted", "1");
	}

	// Token: 0x060013FC RID: 5116 RVA: 0x000555C5 File Offset: 0x000537C5
	public void JoinLobby()
	{
		if (!SteamManager.Initialized)
		{
			Debug.LogError("Cannot open Steam overlay - Steam is not initialized!");
			return;
		}
		Debug.Log("Opening Steam overlay for friend lobby join...");
		SteamFriends.ActivateGameOverlay("Friends");
	}

	// Token: 0x060013FD RID: 5117 RVA: 0x000555F0 File Offset: 0x000537F0
	private void OnRichPresenceJoinRequested(GameRichPresenceJoinRequested_t cb)
	{
		if (NetworkSingleton<GameManager>.Instance != null && NetworkSingleton<GameManager>.Instance.state == GameState.Game && SceneManager.GetActiveScene().name == "CasinoScene")
		{
			Debug.Log("Join request rejected: Game is in progress");
			return;
		}
		this.CleanupCurrentLobby();
		string[] array = cb.m_rgchConnect.Split(' ', StringSplitOptions.None);
		SteamMatchmaking.JoinLobby(new CSteamID(ulong.Parse((array.Length > 1) ? array[1] : array[0])));
	}

	// Token: 0x060013FE RID: 5118 RVA: 0x00055670 File Offset: 0x00053870
	public void InviteFriend()
	{
		if (!SteamManager.Initialized)
		{
			Debug.LogError("Cannot open Steam overlay - Steam is not initialized!");
			return;
		}
		if (this.lobbySettings.steamLobbyID == CSteamID.Nil)
		{
			Debug.LogWarning("Not in a lobby - cannot invite friends.");
			return;
		}
		Debug.Log(string.Format("Opening Steam overlay for game lobby invite to lobby: {0}", this.lobbySettings.steamLobbyID));
		SteamFriends.ActivateGameOverlayInviteDialog(this.lobbySettings.steamLobbyID);
	}

	// Token: 0x060013FF RID: 5119 RVA: 0x000556E0 File Offset: 0x000538E0
	private void OnLobbyEntered(LobbyEnter_t cb)
	{
		this.lobbySettings.steamLobbyID = new CSteamID(cb.m_ulSteamIDLobby);
		this.lobbySettings.inALobby = true;
		if (MonoSingleton<SteamRichPresenceManager>.Instance != null)
		{
			MonoSingleton<SteamRichPresenceManager>.Instance.SetMainMenuPresence();
		}
		if (MonoSingleton<DiscordRichPresenceManager>.Instance != null)
		{
			MonoSingleton<DiscordRichPresenceManager>.Instance.SetMainMenuPresence();
		}
		string lobbyData = SteamMatchmaking.GetLobbyData(this.lobbySettings.steamLobbyID, "LobbyCode");
		if (!string.IsNullOrEmpty(lobbyData))
		{
			this.lobbySettings.lobbyCode = lobbyData;
			this.lobbySettings.NotifyChanged();
			WebSocketManager webSocketManager = Object.FindFirstObjectByType<WebSocketManager>();
			if (webSocketManager != null)
			{
				if (webSocketManager.WebSocketFeaturesEnabled)
				{
					webSocketManager.Initialize();
					Debug.Log("WebSocket initialized with new lobby code: " + lobbyData);
				}
			}
			else
			{
				Debug.LogWarning("WebSocketManager not found! Cannot initialize WebSocket connection.");
			}
		}
		this.UpdatePlayerCount();
		this.CheckWhetherGameAlreadyStarted();
		this.SyncSavedPlayerColorToSteamLobby();
		if (SteamMatchmaking.GetLobbyOwner(this.lobbySettings.steamLobbyID) == SteamUser.GetSteamID())
		{
			this.SyncJoinVisibilityLobbyData();
		}
		this.NotifyLobbyOwnerStatus();
		Action onLobbyEnteredEvent = LobbyManager.OnLobbyEnteredEvent;
		if (onLobbyEnteredEvent == null)
		{
			return;
		}
		onLobbyEnteredEvent();
	}

	// Token: 0x06001400 RID: 5120 RVA: 0x000557F4 File Offset: 0x000539F4
	private void OnLobbyDataUpdated(LobbyDataUpdate_t cb)
	{
		if ((CSteamID)cb.m_ulSteamIDLobby != this.lobbySettings.steamLobbyID || cb.m_bSuccess == 0)
		{
			return;
		}
		if (cb.m_ulSteamIDMember != cb.m_ulSteamIDLobby)
		{
			CSteamID steamIDUser = new CSteamID(cb.m_ulSteamIDMember);
			string lobbyMemberData = SteamMatchmaking.GetLobbyMemberData(this.lobbySettings.steamLobbyID, steamIDUser, "Kicked");
			string lobbyMemberData2 = SteamMatchmaking.GetLobbyMemberData(this.lobbySettings.steamLobbyID, steamIDUser, "KickTarget");
			if (lobbyMemberData == "1" && !string.IsNullOrEmpty(lobbyMemberData2))
			{
				CSteamID csteamID = new CSteamID(ulong.Parse(lobbyMemberData2));
				if (csteamID == SteamUser.GetSteamID())
				{
					Debug.Log("You have been kicked from the lobby!");
					this.ClearRichPresence();
					SteamMatchmaking.LeaveLobby(this.lobbySettings.steamLobbyID);
					this.lobbySettings.inALobby = false;
					this.lobbySettings.steamLobbyID = CSteamID.Nil;
					if (this.gameSettings != null)
					{
						this.gameSettings.gameHasStarted = false;
					}
					this.CreateNewLobby();
					return;
				}
				Debug.Log(string.Format("Player {0} has been kicked from the lobby", csteamID));
				this.UpdatePlayerCount();
				if (MonoSingleton<SteamRichPresenceManager>.Instance != null)
				{
					MonoSingleton<SteamRichPresenceManager>.Instance.UpdatePlayerCount();
				}
				if (MonoSingleton<DiscordRichPresenceManager>.Instance != null)
				{
					MonoSingleton<DiscordRichPresenceManager>.Instance.UpdatePlayerCount();
				}
			}
		}
		if (SteamMatchmaking.GetLobbyData(this.lobbySettings.steamLobbyID, "Disbanded") == "1")
		{
			Debug.Log("Lobby has been disbanded by the host - leaving lobby...");
			this.ClearRichPresence();
			SteamMatchmaking.LeaveLobby(this.lobbySettings.steamLobbyID);
			this.lobbySettings.inALobby = false;
			this.lobbySettings.steamLobbyID = CSteamID.Nil;
			this.lobbySettings.lobbyCode = "";
			if (this.gameSettings != null)
			{
				this.gameSettings.gameHasStarted = false;
			}
			this.CreateNewLobby();
			return;
		}
		string lobbyData = SteamMatchmaking.GetLobbyData(this.lobbySettings.steamLobbyID, "LobbyCode");
		if (!string.IsNullOrEmpty(lobbyData) && lobbyData != this.lobbySettings.lobbyCode)
		{
			this.lobbySettings.lobbyCode = lobbyData;
			this.lobbySettings.NotifyChanged();
			WebSocketManager webSocketManager = Object.FindFirstObjectByType<WebSocketManager>();
			if (webSocketManager != null && webSocketManager.WebSocketFeaturesEnabled)
			{
				webSocketManager.Initialize();
			}
		}
		if (cb.m_ulSteamIDMember == cb.m_ulSteamIDLobby && !string.IsNullOrEmpty(this.lobbySettings.lobbyCode))
		{
			this.UpdateLobbyNameDisplay();
		}
		this.CheckWhetherGameAlreadyStarted();
		if (cb.m_ulSteamIDMember == cb.m_ulSteamIDLobby && MonoSingleton<DiscordRichPresenceManager>.Instance != null)
		{
			MonoSingleton<DiscordRichPresenceManager>.Instance.RefreshPresence();
		}
		this.NotifyLobbyOwnerStatus();
	}

	// Token: 0x06001401 RID: 5121 RVA: 0x00055A9C File Offset: 0x00053C9C
	private void OnLobbyChatUpdate(LobbyChatUpdate_t cb)
	{
		if ((CSteamID)cb.m_ulSteamIDLobby != this.lobbySettings.steamLobbyID)
		{
			return;
		}
		this.UpdatePlayerCount();
		if (MonoSingleton<SteamRichPresenceManager>.Instance != null)
		{
			MonoSingleton<SteamRichPresenceManager>.Instance.UpdatePlayerCount();
		}
		if (MonoSingleton<DiscordRichPresenceManager>.Instance != null)
		{
			MonoSingleton<DiscordRichPresenceManager>.Instance.UpdatePlayerCount();
		}
		this.NotifyLobbyOwnerStatus();
	}

	// Token: 0x06001402 RID: 5122 RVA: 0x00055B04 File Offset: 0x00053D04
	private void UpdatePlayerCount()
	{
		if (this.lobbySettings.steamLobbyID == CSteamID.Nil)
		{
			return;
		}
		int numLobbyMembers = SteamMatchmaking.GetNumLobbyMembers(this.lobbySettings.steamLobbyID);
		this.lobbySettings.currentPlayerCount = numLobbyMembers;
	}

	// Token: 0x06001403 RID: 5123 RVA: 0x00055B48 File Offset: 0x00053D48
	private void SyncSavedPlayerColorToSteamLobby()
	{
		if (!SteamManager.Initialized)
		{
			return;
		}
		if (this.lobbySettings == null || this.lobbySettings.steamLobbyID == CSteamID.Nil)
		{
			return;
		}
		if (MonoSingleton<CosmeticsUnlockManager>.Instance == null)
		{
			base.StartCoroutine(this.RetrySyncPlayerColor());
			return;
		}
		Color? playerColor = MonoSingleton<CosmeticsUnlockManager>.Instance.GetPlayerColor();
		if (playerColor != null)
		{
			string pchValue = ColorHexUtility.ColorToHex(playerColor.Value);
			SteamMatchmaking.SetLobbyMemberData(this.lobbySettings.steamLobbyID, "PlayerColor", pchValue);
		}
		SteamMatchmaking.SetLobbyMemberData(this.lobbySettings.steamLobbyID, "GameVersion", Application.version);
	}

	// Token: 0x06001404 RID: 5124 RVA: 0x00055BEF File Offset: 0x00053DEF
	private IEnumerator RetrySyncPlayerColor()
	{
		yield return new WaitForSeconds(0.5f);
		if (MonoSingleton<CosmeticsUnlockManager>.Instance != null)
		{
			this.SyncSavedPlayerColorToSteamLobby();
		}
		yield break;
	}

	// Token: 0x06001405 RID: 5125 RVA: 0x00055C00 File Offset: 0x00053E00
	private void CheckWhetherGameAlreadyStarted()
	{
		if (SceneManager.GetActiveScene().name != "MainMenuScene")
		{
			return;
		}
		if (SteamMatchmaking.GetLobbyData(this.lobbySettings.steamLobbyID, "GameStarted") != "1")
		{
			return;
		}
		string lobbyData = SteamMatchmaking.GetLobbyData(this.lobbySettings.steamLobbyID, "HostAddress");
		if (SteamUser.GetSteamID().ToString() == lobbyData)
		{
			return;
		}
		if (NetworkClient.active || this.isStartingClient)
		{
			return;
		}
		Debug.Log("Game has started → connecting to host " + lobbyData);
		if (MonoSingleton<SteamRichPresenceManager>.Instance != null)
		{
			MonoSingleton<SteamRichPresenceManager>.Instance.SetInGamePresence();
		}
		MonoSingleton<SceneTransitioner>.Instance.SetLoadingScreen(true, 0.5f, true);
		this.isStartingClient = true;
		base.StartCoroutine(this.StartClientAfterFade(lobbyData));
		this.gameSettings.gameHasStarted = true;
	}

	// Token: 0x06001406 RID: 5126 RVA: 0x00055CE4 File Offset: 0x00053EE4
	private void OnJoinRequest(GameLobbyJoinRequested_t req)
	{
		this.CleanupCurrentLobby();
		Debug.Log("Friend invite received – joining lobby…");
		SteamMatchmaking.JoinLobby(req.m_steamIDLobby);
	}

	// Token: 0x06001407 RID: 5127 RVA: 0x00055D04 File Offset: 0x00053F04
	private void OnLobbyKicked(LobbyKicked_t cb)
	{
		Debug.Log("Player kicked from lobby.");
		this.UpdatePlayerCount();
		if (this.gameSettings != null)
		{
			this.gameSettings.gameHasStarted = false;
		}
		if (MonoSingleton<SteamRichPresenceManager>.Instance != null)
		{
			MonoSingleton<SteamRichPresenceManager>.Instance.UpdatePlayerCount();
		}
		if (MonoSingleton<DiscordRichPresenceManager>.Instance != null)
		{
			MonoSingleton<DiscordRichPresenceManager>.Instance.UpdatePlayerCount();
		}
	}

	// Token: 0x06001408 RID: 5128 RVA: 0x00055D6C File Offset: 0x00053F6C
	public void CreateNewLobby()
	{
		if (this.lobbySettings == null)
		{
			return;
		}
		this.CleanupCurrentLobby();
		this.lobbySettings.lobbyCode = this.GenerateCode(this.lobbySettings.codeLength);
		SteamAPICall_t hAPICall = SteamMatchmaking.CreateLobby(ELobbyType.k_ELobbyTypePublic, this.lobbySettings.maxPlayers);
		CallResult<LobbyCreated_t>.Create(new CallResult<LobbyCreated_t>.APIDispatchDelegate(this.OnNewLobbyCreated)).Set(hAPICall, null);
	}

	// Token: 0x06001409 RID: 5129 RVA: 0x00055DD4 File Offset: 0x00053FD4
	private void OnNewLobbyCreated(LobbyCreated_t result, bool failure)
	{
		if (failure || result.m_eResult != EResult.k_EResultOK)
		{
			Debug.LogError(string.Format("Failed to create new lobby: {0}", result.m_eResult));
			SceneManager.LoadSceneAsync("NetworkSetupScene");
			return;
		}
		if (this.lobbySettings != null)
		{
			this.lobbySettings.steamLobbyID = new CSteamID(result.m_ulSteamIDLobby);
			this.lobbySettings.inALobby = true;
			SteamMatchmaking.SetLobbyData(this.lobbySettings.steamLobbyID, "name", SteamFriends.GetPersonaName() + "'s Lobby");
			SteamMatchmaking.SetLobbyData(this.lobbySettings.steamLobbyID, "HostAddress", SteamUser.GetSteamID().ToString());
			SteamMatchmaking.SetLobbyData(this.lobbySettings.steamLobbyID, "SelectedScene", "Gameplay");
			SteamMatchmaking.SetLobbyData(this.lobbySettings.steamLobbyID, "GameStarted", "0");
			SteamMatchmaking.SetLobbyData(this.lobbySettings.steamLobbyID, "LobbyCode", this.lobbySettings.lobbyCode);
			this.SyncJoinVisibilityLobbyData();
			string version = Application.version;
			SteamMatchmaking.SetLobbyMemberData(this.lobbySettings.steamLobbyID, "GameVersion", version);
			Debug.Log("Set host game version on lobby creation: " + version);
			Debug.Log(string.Format("New lobby created: {0} with code: {1}", this.lobbySettings.steamLobbyID, this.lobbySettings.lobbyCode));
			SteamMatchmaking.JoinLobby(this.lobbySettings.steamLobbyID);
		}
	}

	// Token: 0x0600140A RID: 5130 RVA: 0x00055F58 File Offset: 0x00054158
	private string GenerateCode(int length)
	{
		StringBuilder stringBuilder = new StringBuilder(length);
		Random random = new Random();
		for (int i = 0; i < length; i++)
		{
			stringBuilder.Append("1234567890"[random.Next("1234567890".Length)]);
		}
		return stringBuilder.ToString();
	}

	// Token: 0x0600140B RID: 5131 RVA: 0x00055FA5 File Offset: 0x000541A5
	public CSteamID GetCurrentLobbyID()
	{
		return this.lobbySettings.steamLobbyID;
	}

	// Token: 0x0600140C RID: 5132 RVA: 0x00055FB2 File Offset: 0x000541B2
	public void ClearRichPresence()
	{
		if (MonoSingleton<SteamRichPresenceManager>.Instance != null)
		{
			MonoSingleton<SteamRichPresenceManager>.Instance.ClearRichPresence();
		}
		if (MonoSingleton<DiscordRichPresenceManager>.Instance != null)
		{
			MonoSingleton<DiscordRichPresenceManager>.Instance.ClearRichPresence();
		}
	}

	// Token: 0x0600140D RID: 5133 RVA: 0x00055FE4 File Offset: 0x000541E4
	public void CleanupCurrentLobby()
	{
		if (!SteamManager.Initialized || this.lobbySettings == null || this.lobbySettings.steamLobbyID == CSteamID.Nil)
		{
			return;
		}
		if (SteamMatchmaking.GetLobbyOwner(this.lobbySettings.steamLobbyID) == SteamUser.GetSteamID())
		{
			Debug.Log(string.Format("Cleaning up current lobby {0} before joining new one", this.lobbySettings.steamLobbyID));
			SteamMatchmaking.SetLobbyData(this.lobbySettings.steamLobbyID, "GameStarted", "0");
			SteamMatchmaking.LeaveLobby(this.lobbySettings.steamLobbyID);
			this.lobbySettings.steamLobbyID = CSteamID.Nil;
			this.lobbySettings.inALobby = false;
			this.lobbySettings.lobbyCode = "";
			if (this.gameSettings != null)
			{
				this.gameSettings.gameHasStarted = false;
			}
		}
	}

	// Token: 0x0600140E RID: 5134 RVA: 0x000560D0 File Offset: 0x000542D0
	private void UpdateLobbyNameDisplay()
	{
		WebSocketManager webSocketManager = Object.FindFirstObjectByType<WebSocketManager>();
		if (webSocketManager != null && webSocketManager.WebSocketFeaturesEnabled && this.lobbySettings != null && this.lobbySettings.steamLobbyID != CSteamID.Nil && !string.IsNullOrEmpty(this.lobbySettings.lobbyCode))
		{
			webSocketManager.Initialize();
		}
	}

	// Token: 0x0600140F RID: 5135 RVA: 0x00056134 File Offset: 0x00054334
	public void SetLobbyPrivacy(bool isPrivate)
	{
		if (!SteamManager.Initialized || this.lobbySettings == null || this.lobbySettings.steamLobbyID == CSteamID.Nil)
		{
			return;
		}
		if (SteamMatchmaking.GetLobbyOwner(this.lobbySettings.steamLobbyID) != SteamUser.GetSteamID())
		{
			return;
		}
		ELobbyType eLobbyType = isPrivate ? ELobbyType.k_ELobbyTypePrivate : ELobbyType.k_ELobbyTypeFriendsOnly;
		if (SteamMatchmaking.SetLobbyType(this.lobbySettings.steamLobbyID, eLobbyType))
		{
			SteamMatchmaking.SetLobbyData(this.lobbySettings.steamLobbyID, "JoinVisibility", isPrivate ? "invite" : "friends");
			Debug.Log("Lobby privacy set to " + (isPrivate ? "Private" : "Public"));
			return;
		}
		Debug.LogWarning("Failed to set lobby privacy to " + (isPrivate ? "Private" : "Public"));
	}

	// Token: 0x06001410 RID: 5136 RVA: 0x00056208 File Offset: 0x00054408
	private void OnSettingChanged(SettingItemBase entry)
	{
		if (entry == null || !this.IsLobbyModeEntry(entry))
		{
			return;
		}
		this.ApplyLobbyModeFromSetting();
	}

	// Token: 0x06001411 RID: 5137 RVA: 0x00056223 File Offset: 0x00054423
	private bool IsLobbyModeEntry(SettingItemBase entry)
	{
		return !(entry == null) && !string.IsNullOrWhiteSpace(entry.key) && string.Equals(entry.key.Trim(), "lobbymode", StringComparison.OrdinalIgnoreCase);
	}

	// Token: 0x06001412 RID: 5138 RVA: 0x00056254 File Offset: 0x00054454
	private void ApplyLobbyModeFromSetting()
	{
		if (!SteamManager.Initialized || this.lobbySettings == null || this.lobbySettings.steamLobbyID == CSteamID.Nil)
		{
			return;
		}
		if (this.lobbyModeSetting == null)
		{
			return;
		}
		if (SteamMatchmaking.GetLobbyOwner(this.lobbySettings.steamLobbyID) != SteamUser.GetSteamID())
		{
			return;
		}
		ELobbyType elobbyType = (this.lobbyModeSetting.index == 1) ? ELobbyType.k_ELobbyTypePrivate : ELobbyType.k_ELobbyTypeFriendsOnly;
		if (SteamMatchmaking.SetLobbyType(this.lobbySettings.steamLobbyID, elobbyType))
		{
			this.SyncJoinVisibilityLobbyData();
			if (MonoSingleton<DiscordRichPresenceManager>.Instance != null)
			{
				MonoSingleton<DiscordRichPresenceManager>.Instance.RefreshPresence();
			}
			Debug.Log(string.Format("Lobby visibility set to {0}", elobbyType));
			return;
		}
		Debug.LogWarning(string.Format("Failed to set lobby visibility to {0}", elobbyType));
	}

	// Token: 0x06001413 RID: 5139 RVA: 0x0005632C File Offset: 0x0005452C
	private void SyncJoinVisibilityLobbyData()
	{
		if (!SteamManager.Initialized || this.lobbySettings == null || this.lobbySettings.steamLobbyID == CSteamID.Nil)
		{
			return;
		}
		if (SteamMatchmaking.GetLobbyOwner(this.lobbySettings.steamLobbyID) != SteamUser.GetSteamID())
		{
			return;
		}
		if (this.lobbyModeSetting == null)
		{
			return;
		}
		string pchValue = (this.lobbyModeSetting.index == 1) ? "invite" : "friends";
		SteamMatchmaking.SetLobbyData(this.lobbySettings.steamLobbyID, "JoinVisibility", pchValue);
	}

	// Token: 0x06001414 RID: 5140 RVA: 0x000563C4 File Offset: 0x000545C4
	private IEnumerator StartHostAfterFade()
	{
		yield return new WaitForSeconds(1f);
		NetworkManager.singleton.StartHost();
		yield break;
	}

	// Token: 0x06001415 RID: 5141 RVA: 0x000563CC File Offset: 0x000545CC
	private IEnumerator StartClientAfterFade(string hostAddress)
	{
		yield return new WaitForSeconds(1f);
		if (!NetworkClient.active)
		{
			NetworkManager.singleton.networkAddress = hostAddress;
			NetworkManager.singleton.StartClient();
		}
		this.isStartingClient = false;
		yield break;
	}

	// Token: 0x06001416 RID: 5142 RVA: 0x000563E4 File Offset: 0x000545E4
	public void LeaveLobby()
	{
		if (!SteamManager.Initialized || this.lobbySettings == null || this.lobbySettings.steamLobbyID == CSteamID.Nil)
		{
			return;
		}
		SteamMatchmaking.LeaveLobby(this.lobbySettings.steamLobbyID);
		this.lobbySettings.inALobby = false;
		this.lobbySettings.steamLobbyID = CSteamID.Nil;
		this.lobbySettings.lobbyCode = "";
		if (this.gameSettings != null)
		{
			this.gameSettings.gameHasStarted = false;
		}
		if (MonoSingleton<SteamRichPresenceManager>.Instance != null)
		{
			MonoSingleton<SteamRichPresenceManager>.Instance.SetMainMenuPresence();
		}
		if (MonoSingleton<DiscordRichPresenceManager>.Instance != null)
		{
			MonoSingleton<DiscordRichPresenceManager>.Instance.SetMainMenuPresence();
		}
		Action onLobbyLeftEvent = LobbyManager.OnLobbyLeftEvent;
		if (onLobbyLeftEvent != null)
		{
			onLobbyLeftEvent();
		}
		this.NotifyLobbyOwnerStatus();
	}

	// Token: 0x06001417 RID: 5143 RVA: 0x000564B8 File Offset: 0x000546B8
	public void DisbandLobby()
	{
		if (!SteamManager.Initialized || this.lobbySettings == null || this.lobbySettings.steamLobbyID == CSteamID.Nil)
		{
			return;
		}
		if (SteamMatchmaking.GetLobbyOwner(this.lobbySettings.steamLobbyID) != SteamUser.GetSteamID())
		{
			Debug.LogWarning("Only the lobby owner can disband the lobby.");
			return;
		}
		Debug.Log("Disbanding lobby - kicking all players...");
		int numLobbyMembers = SteamMatchmaking.GetNumLobbyMembers(this.lobbySettings.steamLobbyID);
		CSteamID steamID = SteamUser.GetSteamID();
		for (int i = 0; i < numLobbyMembers; i++)
		{
			CSteamID lobbyMemberByIndex = SteamMatchmaking.GetLobbyMemberByIndex(this.lobbySettings.steamLobbyID, i);
			if (!(lobbyMemberByIndex == steamID))
			{
				try
				{
					SteamMatchmaking.SetLobbyMemberData(this.lobbySettings.steamLobbyID, "Kicked", "1");
					SteamMatchmaking.SetLobbyMemberData(this.lobbySettings.steamLobbyID, "KickTarget", lobbyMemberByIndex.ToString());
					Debug.Log(string.Format("Kicked player {0} during lobby disband", lobbyMemberByIndex));
				}
				catch (Exception ex)
				{
					Debug.LogWarning(string.Format("Failed to kick player {0}: {1}", lobbyMemberByIndex, ex.Message));
				}
			}
		}
		SteamMatchmaking.SetLobbyData(this.lobbySettings.steamLobbyID, "GameStarted", "0");
		if (this.gameSettings != null)
		{
			this.gameSettings.gameHasStarted = false;
		}
		SteamMatchmaking.SetLobbyData(this.lobbySettings.steamLobbyID, "Disbanded", "1");
		base.StartCoroutine(this.LeaveLobbyAfterDisband());
	}

	// Token: 0x06001418 RID: 5144 RVA: 0x00056648 File Offset: 0x00054848
	private IEnumerator LeaveLobbyAfterDisband()
	{
		yield return new WaitForSeconds(0.5f);
		if (this.lobbySettings != null && this.lobbySettings.steamLobbyID != CSteamID.Nil)
		{
			SteamMatchmaking.LeaveLobby(this.lobbySettings.steamLobbyID);
			this.lobbySettings.inALobby = false;
			this.lobbySettings.steamLobbyID = CSteamID.Nil;
			this.lobbySettings.lobbyCode = "";
			Action onLobbyLeftEvent = LobbyManager.OnLobbyLeftEvent;
			if (onLobbyLeftEvent != null)
			{
				onLobbyLeftEvent();
			}
			this.NotifyLobbyOwnerStatus();
			this.CreateNewLobby();
		}
		yield break;
	}

	// Token: 0x06001419 RID: 5145 RVA: 0x00056658 File Offset: 0x00054858
	private void NotifyLobbyOwnerStatus()
	{
		if (SteamManager.Initialized && !(this.lobbySettings == null) && !(this.lobbySettings.steamLobbyID == CSteamID.Nil))
		{
			try
			{
				bool obj = SteamMatchmaking.GetLobbyOwner(this.lobbySettings.steamLobbyID) == SteamUser.GetSteamID();
				Action<bool> onLobbyOwnerStatusChanged = LobbyManager.OnLobbyOwnerStatusChanged;
				if (onLobbyOwnerStatusChanged != null)
				{
					onLobbyOwnerStatusChanged(obj);
				}
			}
			catch (Exception ex)
			{
				Debug.LogWarning("[LobbyManager] Failed to check lobby owner status: " + ex.Message);
				Action<bool> onLobbyOwnerStatusChanged2 = LobbyManager.OnLobbyOwnerStatusChanged;
				if (onLobbyOwnerStatusChanged2 != null)
				{
					onLobbyOwnerStatusChanged2(false);
				}
			}
			return;
		}
		Action<bool> onLobbyOwnerStatusChanged3 = LobbyManager.OnLobbyOwnerStatusChanged;
		if (onLobbyOwnerStatusChanged3 == null)
		{
			return;
		}
		onLobbyOwnerStatusChanged3(false);
	}

	// Token: 0x0600141A RID: 5146 RVA: 0x0005670C File Offset: 0x0005490C
	private void DevStart()
	{
		if (this.gameSettings.gameHasStarted)
		{
			return;
		}
		this.gameSettings.gameHasStarted = true;
		NetworkManager.singleton.StartHost();
	}

	// Token: 0x04000CA6 RID: 3238
	public const string JoinVisibilityLobbyKey = "JoinVisibility";

	// Token: 0x04000CA7 RID: 3239
	[Header("Settings")]
	[SerializeField]
	private DropdownSettingItem lobbyModeSetting;

	// Token: 0x04000CA8 RID: 3240
	private LobbySettings lobbySettings;

	// Token: 0x04000CA9 RID: 3241
	private GameSettings gameSettings;

	// Token: 0x04000CAA RID: 3242
	private bool isStartingClient;

	// Token: 0x04000CAE RID: 3246
	private Callback<LobbyEnter_t> lobbyEnterCallback;

	// Token: 0x04000CAF RID: 3247
	private Callback<LobbyDataUpdate_t> lobbyDataUpdateCallback;

	// Token: 0x04000CB0 RID: 3248
	private Callback<GameLobbyJoinRequested_t> joinRequestCallback;

	// Token: 0x04000CB1 RID: 3249
	private Callback<LobbyChatUpdate_t> lobbyChatUpdateCallback;

	// Token: 0x04000CB2 RID: 3250
	private Callback<LobbyKicked_t> lobbyKickedCallback;

	// Token: 0x04000CB3 RID: 3251
	private Callback<GameRichPresenceJoinRequested_t> richPresenceJoinRequestedCallback;
}
