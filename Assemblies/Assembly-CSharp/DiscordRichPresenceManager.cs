using System;
using System.Reflection;
using Discord.Sdk;
using Extensions;
using Steamworks;
using UnityEngine;
using UnityEngine.SceneManagement;

// Token: 0x02000296 RID: 662
public class DiscordRichPresenceManager : MonoSingleton<DiscordRichPresenceManager>
{
	// Token: 0x17000218 RID: 536
	// (get) Token: 0x0600177F RID: 6015 RVA: 0x00063517 File Offset: 0x00061717
	public bool IsConnected
	{
		get
		{
			return this.isDiscordReady && this.discordClient != null;
		}
	}

	// Token: 0x1400001F RID: 31
	// (add) Token: 0x06001780 RID: 6016 RVA: 0x0006352C File Offset: 0x0006172C
	// (remove) Token: 0x06001781 RID: 6017 RVA: 0x00063564 File Offset: 0x00061764
	public event Action<bool> ConnectionStateChanged;

	// Token: 0x06001782 RID: 6018 RVA: 0x00063599 File Offset: 0x00061799
	protected override void OnAwake()
	{
		base.OnAwake();
		this.lobbySettings = Resources.Load<LobbySettings>("LobbySettings");
		SceneManager.sceneLoaded += this.OnSceneLoaded;
	}

	// Token: 0x06001783 RID: 6019 RVA: 0x000635C2 File Offset: 0x000617C2
	public void ConnectDiscord(bool allowOAuthPrompt = true)
	{
		if (!SteamManager.Initialized || this.isDiscordReady)
		{
			return;
		}
		if (this.discordClient == null)
		{
			this.InitializeDiscord(allowOAuthPrompt);
			return;
		}
		if (allowOAuthPrompt && string.IsNullOrEmpty(this.codeVerifier))
		{
			this.StartOAuthFlow();
		}
	}

	// Token: 0x06001784 RID: 6020 RVA: 0x000635FA File Offset: 0x000617FA
	public void DisconnectDiscord()
	{
		PlayerPrefs.DeleteKey("Discord_AccessToken");
		PlayerPrefs.DeleteKey("Discord_TokenExpiresAt");
		PlayerPrefs.Save();
		if (this.discordClient == null)
		{
			return;
		}
		this.userRequestedDisconnect = true;
		this.ShutdownDiscord();
	}

	// Token: 0x06001785 RID: 6021 RVA: 0x0006362B File Offset: 0x0006182B
	public static bool HasUserAcceptedDiscordToaster()
	{
		return PlayerPrefs.GetInt("Discord_ToasterAccepted", 0) == 1;
	}

	// Token: 0x06001786 RID: 6022 RVA: 0x0006363B File Offset: 0x0006183B
	public static void SetUserAcceptedDiscordToaster()
	{
		PlayerPrefs.SetInt("Discord_ToasterAccepted", 1);
		PlayerPrefs.Save();
	}

	// Token: 0x06001787 RID: 6023 RVA: 0x0006364D File Offset: 0x0006184D
	public static void ResetDiscordToasterAccepted()
	{
		PlayerPrefs.DeleteKey("Discord_ToasterAccepted");
		PlayerPrefs.Save();
	}

	// Token: 0x06001788 RID: 6024 RVA: 0x0006365E File Offset: 0x0006185E
	[ContextMenu("Reset Discord Toaster Accepted")]
	private void ResetDiscordToasterAcceptedContextMenu()
	{
		DiscordRichPresenceManager.ResetDiscordToasterAccepted();
	}

	// Token: 0x06001789 RID: 6025 RVA: 0x00063665 File Offset: 0x00061865
	public void Update()
	{
		if (this.discordClient != null)
		{
			NativeMethods.Discord_RunCallbacks();
		}
	}

	// Token: 0x0600178A RID: 6026 RVA: 0x00063674 File Offset: 0x00061874
	private void OnDestroy()
	{
		SceneManager.sceneLoaded -= this.OnSceneLoaded;
		this.ShutdownDiscord();
	}

	// Token: 0x0600178B RID: 6027 RVA: 0x00063690 File Offset: 0x00061890
	private void InitializeDiscord(bool allowOAuthPrompt)
	{
		if (this.discordClientId == 0UL)
		{
			Debug.LogWarning("[Discord] Client ID not set! Discord integration disabled.");
			return;
		}
		try
		{
			this.discordClient = new Client();
			this.discordClient.SetApplicationId(this.discordClientId);
			this.discordClient.AddLogCallback(new Client.LogCallback(this.OnDiscordLog), LoggingSeverity.Error);
			this.discordClient.SetStatusChangedCallback(new Client.OnStatusChanged(this.OnDiscordStatusChanged));
			this.discordClient.SetActivityJoinCallback(new Client.ActivityJoinCallback(this.OnDiscordJoin));
			string @string = PlayerPrefs.GetString("Discord_AccessToken", "");
			if (!string.IsNullOrEmpty(@string))
			{
				this.OnReceivedToken(@string);
			}
			else if (allowOAuthPrompt)
			{
				this.StartOAuthFlow();
			}
		}
		catch (Exception ex)
		{
			Debug.LogError("[Discord] Failed to initialize: " + ex.Message);
		}
	}

	// Token: 0x0600178C RID: 6028 RVA: 0x00063768 File Offset: 0x00061968
	private void OnDiscordLog(string message, LoggingSeverity severity)
	{
		if (severity == LoggingSeverity.Error)
		{
			Debug.LogError("[Discord] " + message);
		}
	}

	// Token: 0x0600178D RID: 6029 RVA: 0x00063780 File Offset: 0x00061980
	private void OnDiscordStatusChanged(Client.Status status, Client.Error error, int errorCode)
	{
		if (error != Client.Error.None)
		{
			Debug.LogError(string.Format("[Discord] Error: {0}, code: {1}", error, errorCode));
		}
		if (status == Client.Status.Ready)
		{
			this.isDiscordReady = true;
			Action<bool> connectionStateChanged = this.ConnectionStateChanged;
			if (connectionStateChanged != null)
			{
				connectionStateChanged(true);
			}
			this.UpdatePresenceForCurrentScene();
			return;
		}
		if (status == Client.Status.Disconnected)
		{
			bool flag = this.userRequestedDisconnect;
			this.userRequestedDisconnect = false;
			this.isDiscordReady = false;
			Action<bool> connectionStateChanged2 = this.ConnectionStateChanged;
			if (connectionStateChanged2 != null)
			{
				connectionStateChanged2(false);
			}
			if (!flag && error != Client.Error.None)
			{
				PlayerPrefs.DeleteKey("Discord_AccessToken");
				PlayerPrefs.DeleteKey("Discord_TokenExpiresAt");
				PlayerPrefs.Save();
				if (string.IsNullOrEmpty(this.codeVerifier))
				{
					this.StartOAuthFlow();
				}
			}
		}
	}

	// Token: 0x0600178E RID: 6030 RVA: 0x0006382C File Offset: 0x00061A2C
	private void StartOAuthFlow()
	{
		if (this.discordClient == null)
		{
			return;
		}
		try
		{
			AuthorizationCodeVerifier authorizationCodeVerifier = this.discordClient.CreateAuthorizationCodeVerifier();
			this.codeVerifier = authorizationCodeVerifier.Verifier();
			AuthorizationArgs authorizationArgs = new AuthorizationArgs();
			authorizationArgs.SetClientId(this.discordClientId);
			authorizationArgs.SetScopes(Client.GetDefaultPresenceScopes());
			authorizationArgs.SetCodeChallenge(authorizationCodeVerifier.Challenge());
			this.discordClient.Authorize(authorizationArgs, new Client.AuthorizationCallback(this.OnAuthorizeResult));
		}
		catch (Exception ex)
		{
			Debug.LogError("[Discord] Failed to start OAuth flow: " + ex.Message);
		}
	}

	// Token: 0x0600178F RID: 6031 RVA: 0x000638C8 File Offset: 0x00061AC8
	private void OnAuthorizeResult(ClientResult result, string code, string redirectUri)
	{
		if (!result.Successful())
		{
			Debug.LogError("[Discord] Authorization failed: " + result.Error());
			return;
		}
		string text = this.codeVerifier;
		this.codeVerifier = null;
		if (string.IsNullOrEmpty(text))
		{
			Debug.LogError("[Discord] Missing OAuth code verifier; aborting token exchange.");
			return;
		}
		this.GetTokenFromCode(code, redirectUri, text);
	}

	// Token: 0x06001790 RID: 6032 RVA: 0x0006391D File Offset: 0x00061B1D
	private void GetTokenFromCode(string code, string redirectUri, string verifier)
	{
		this.discordClient.GetToken(this.discordClientId, code, verifier, redirectUri, delegate(ClientResult result, string token, string refreshToken, AuthorizationTokenType tokenType, int expiresIn, string scope)
		{
			this.codeVerifier = null;
			if (token != "")
			{
				this.OnReceivedToken(token);
				return;
			}
			Debug.LogError("[Discord] Failed to retrieve token");
		});
	}

	// Token: 0x06001791 RID: 6033 RVA: 0x00063940 File Offset: 0x00061B40
	private void OnReceivedToken(string token)
	{
		PlayerPrefs.SetString("Discord_AccessToken", token);
		PlayerPrefs.SetString("Discord_TokenExpiresAt", DateTime.Now.AddDays(7.0).ToBinary().ToString());
		PlayerPrefs.Save();
		this.discordClient.UpdateToken(AuthorizationTokenType.Bearer, token, delegate(ClientResult result)
		{
			if (result.Successful())
			{
				this.discordClient.Connect();
				return;
			}
			Debug.LogError("[Discord] Failed to update token: " + result.Error());
		});
	}

	// Token: 0x06001792 RID: 6034 RVA: 0x000639A8 File Offset: 0x00061BA8
	private void OnDiscordJoin(string joinSecret)
	{
		if (string.IsNullOrEmpty(joinSecret))
		{
			return;
		}
		ulong ulSteamID = 0UL;
		if (joinSecret.StartsWith("steam_lobby:"))
		{
			string text = joinSecret.Substring("steam_lobby:".Length);
			if (!ulong.TryParse(text, out ulSteamID))
			{
				Debug.LogError("[Discord] Failed to parse lobby ID: " + text);
				return;
			}
		}
		else if (!ulong.TryParse(joinSecret, out ulSteamID))
		{
			Debug.LogError("[Discord] Invalid join secret format: " + joinSecret);
			return;
		}
		this.JoinSteamLobby(new CSteamID(ulSteamID));
	}

	// Token: 0x06001793 RID: 6035 RVA: 0x00063A24 File Offset: 0x00061C24
	private void JoinSteamLobby(CSteamID lobbyId)
	{
		if (!SteamManager.Initialized)
		{
			return;
		}
		if (NetworkSingleton<GameManager>.Instance != null && NetworkSingleton<GameManager>.Instance.state == GameState.Game && SceneManager.GetActiveScene().name == "CasinoScene")
		{
			return;
		}
		if (MonoSingleton<LobbyManager>.Instance != null)
		{
			MethodInfo method = MonoSingleton<LobbyManager>.Instance.GetType().GetMethod("CleanupCurrentLobby", BindingFlags.Instance | BindingFlags.NonPublic);
			if (method != null)
			{
				method.Invoke(MonoSingleton<LobbyManager>.Instance, null);
			}
		}
		SteamMatchmaking.JoinLobby(lobbyId);
	}

	// Token: 0x06001794 RID: 6036 RVA: 0x00063AA9 File Offset: 0x00061CA9
	private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
	{
		if (scene.name == "MainMenuScene" && DiscordRichPresenceManager.HasUserAcceptedDiscordToaster() && this.discordClient == null)
		{
			this.ConnectDiscord(false);
		}
		if (this.isDiscordReady)
		{
			this.UpdatePresenceForCurrentScene();
		}
	}

	// Token: 0x06001795 RID: 6037 RVA: 0x00063AE2 File Offset: 0x00061CE2
	public void RefreshPresence()
	{
		if (!this.isDiscordReady)
		{
			return;
		}
		this.UpdatePresenceForCurrentScene();
	}

	// Token: 0x06001796 RID: 6038 RVA: 0x00063AF4 File Offset: 0x00061CF4
	private void UpdatePresenceForCurrentScene()
	{
		if (SceneManager.GetActiveScene().name == "MainMenuScene")
		{
			this.SetMainMenuPresence();
			return;
		}
		if (Object.FindFirstObjectByType<GameManager>() != null)
		{
			if (NetworkSingleton<GameManager>.Instance.state == GameState.Lobby)
			{
				this.SetInHomePresence();
				return;
			}
			if (NetworkSingleton<GameManager>.Instance.state == GameState.Game)
			{
				this.SetInGamePresence();
			}
		}
	}

	// Token: 0x06001797 RID: 6039 RVA: 0x00063B54 File Offset: 0x00061D54
	public void SetMainMenuPresence()
	{
		if (!this.isDiscordReady || this.discordClient == null)
		{
			return;
		}
		int partySize = 0;
		int partyMax = 0;
		string joinSecret = null;
		if (this.lobbySettings != null && this.lobbySettings.steamLobbyID != CSteamID.Nil)
		{
			partySize = SteamMatchmaking.GetNumLobbyMembers(this.lobbySettings.steamLobbyID);
			partyMax = SteamMatchmaking.GetLobbyMemberLimit(this.lobbySettings.steamLobbyID);
			if (DiscordRichPresenceManager.DiscordJoinSecretAllowedForLobby(this.lobbySettings.steamLobbyID))
			{
				joinSecret = string.Format("steam_lobby:{0}", this.lobbySettings.steamLobbyID.m_SteamID);
			}
		}
		else if (this.lobbySettings != null)
		{
			partySize = this.lobbySettings.currentPlayerCount;
			partyMax = this.lobbySettings.maxPlayers;
		}
		this.UpdateRichPresence("In Main Menu", "Getting the gang together", partySize, partyMax, joinSecret);
	}

	// Token: 0x06001798 RID: 6040 RVA: 0x00063C2C File Offset: 0x00061E2C
	public void SetInHomePresence()
	{
		if (!this.isDiscordReady || this.discordClient == null)
		{
			return;
		}
		string arg = MoneyFormatter.FormatWithDollar((NetworkSingleton<MoneyManager>.Instance != null) ? NetworkSingleton<MoneyManager>.Instance.balance : 0L);
		int num = (NetworkSingleton<GameManager>.Instance != null) ? (NetworkSingleton<GameManager>.Instance.daysPassed + 1) : 1;
		int num2 = 0;
		string joinSecret = null;
		if (this.lobbySettings != null && this.lobbySettings.steamLobbyID != CSteamID.Nil)
		{
			num2 = SteamMatchmaking.GetNumLobbyMembers(this.lobbySettings.steamLobbyID);
			if (DiscordRichPresenceManager.DiscordJoinSecretAllowedForLobby(this.lobbySettings.steamLobbyID))
			{
				joinSecret = string.Format("steam_lobby:{0}", this.lobbySettings.steamLobbyID.m_SteamID);
			}
		}
		string details = "Playing Gamble with Friends";
		string state = string.Format("Day {0} - Balance: {1}", num, arg);
		int partySize = num2;
		LobbySettings lobbySettings = this.lobbySettings;
		this.UpdateRichPresence(details, state, partySize, (lobbySettings != null) ? lobbySettings.maxPlayers : 0, joinSecret);
	}

	// Token: 0x06001799 RID: 6041 RVA: 0x00063D24 File Offset: 0x00061F24
	public void SetInGamePresence()
	{
		if (!this.isDiscordReady || this.discordClient == null)
		{
			return;
		}
		if (Object.FindFirstObjectByType<GameManager>() == null)
		{
			return;
		}
		long num = (NetworkSingleton<MoneyManager>.Instance != null) ? NetworkSingleton<MoneyManager>.Instance.balance : 0L;
		long currentQuota = NetworkSingleton<GameManager>.Instance.currentQuota;
		string arg = MoneyFormatter.FormatWithDollar(num);
		string arg2 = MoneyFormatter.FormatWithDollar(currentQuota);
		int num2 = Mathf.RoundToInt((currentQuota > 0L) ? (Mathf.Clamp01((float)num / (float)currentQuota) * 100f) : 0f);
		this.UpdateRichPresence("Playing Gamble with Friends", string.Format("Letting it ride - {0}/{1} ({2}%)", arg, arg2, num2), 0, 0, null);
	}

	// Token: 0x0600179A RID: 6042 RVA: 0x00063DCC File Offset: 0x00061FCC
	public void UpdatePlayerCount()
	{
		if (!this.isDiscordReady)
		{
			return;
		}
		if (NetworkSingleton<GameManager>.Instance != null)
		{
			if (NetworkSingleton<GameManager>.Instance.state == GameState.Lobby)
			{
				this.SetInHomePresence();
				return;
			}
			if (SceneManager.GetActiveScene().name == "MainMenuScene")
			{
				this.SetMainMenuPresence();
			}
		}
	}

	// Token: 0x0600179B RID: 6043 RVA: 0x00063E21 File Offset: 0x00062021
	private static bool DiscordJoinSecretAllowedForLobby(CSteamID lobbyId)
	{
		return lobbyId != CSteamID.Nil && SteamManager.Initialized;
	}

	// Token: 0x0600179C RID: 6044 RVA: 0x00063E38 File Offset: 0x00062038
	private void UpdateRichPresence(string details, string state, int partySize, int partyMax, string joinSecret)
	{
		if (!this.isDiscordReady || this.discordClient == null)
		{
			return;
		}
		try
		{
			Activity activity = this.currentActivity;
			if (activity != null)
			{
				activity.Dispose();
			}
			this.currentActivity = new Activity();
			this.currentActivity.SetType(ActivityTypes.Playing);
			this.currentActivity.SetDetails(details);
			this.currentActivity.SetState(state);
			if (partySize > 0 && partyMax > 0 && !string.IsNullOrEmpty(joinSecret))
			{
				ActivityParty activityParty = new ActivityParty();
				ActivityParty activityParty2 = activityParty;
				LobbySettings lobbySettings = this.lobbySettings;
				activityParty2.SetId(((lobbySettings != null) ? lobbySettings.steamLobbyID.m_SteamID.ToString() : null) ?? "0");
				activityParty.SetCurrentSize(partySize);
				activityParty.SetMaxSize(partyMax);
				activityParty.SetPrivacy(ActivityPartyPrivacy.Private);
				this.currentActivity.SetParty(activityParty);
				ActivitySecrets activitySecrets = new ActivitySecrets();
				activitySecrets.SetJoin(joinSecret);
				this.currentActivity.SetSecrets(activitySecrets);
			}
			this.discordClient.UpdateRichPresence(this.currentActivity, delegate(ClientResult result)
			{
				if (!result.Successful())
				{
					Debug.LogError("[Discord] Failed to update presence: " + result.Error());
				}
			});
		}
		catch (Exception ex)
		{
			Debug.LogError("[Discord] Error updating Rich Presence: " + ex.Message + "\n" + ex.StackTrace);
		}
	}

	// Token: 0x0600179D RID: 6045 RVA: 0x00063F7C File Offset: 0x0006217C
	public void ClearRichPresence()
	{
		if (!this.isDiscordReady || this.discordClient == null)
		{
			return;
		}
		try
		{
			Activity activity = new Activity();
			this.discordClient.UpdateRichPresence(activity, delegate(ClientResult result)
			{
			});
			activity.Dispose();
		}
		catch (Exception ex)
		{
			Debug.LogWarning("[Discord] Failed to clear presence: " + ex.Message);
		}
	}

	// Token: 0x0600179E RID: 6046 RVA: 0x00063FFC File Offset: 0x000621FC
	private void ShutdownDiscord()
	{
		if (this.discordClient == null)
		{
			return;
		}
		try
		{
			Activity activity = this.currentActivity;
			if (activity != null)
			{
				activity.Dispose();
			}
			this.currentActivity = null;
			this.discordClient.Disconnect();
			this.discordClient.Dispose();
			this.discordClient = null;
			this.isDiscordReady = false;
			this.codeVerifier = null;
			Action<bool> connectionStateChanged = this.ConnectionStateChanged;
			if (connectionStateChanged != null)
			{
				connectionStateChanged(false);
			}
		}
		catch (Exception ex)
		{
			Debug.LogError("[Discord] Error during shutdown: " + ex.Message);
		}
	}

	// Token: 0x04000F3B RID: 3899
	[Header("Discord Settings")]
	[SerializeField]
	private ulong discordClientId = 1466437432097636467UL;

	// Token: 0x04000F3C RID: 3900
	private LobbySettings lobbySettings;

	// Token: 0x04000F3D RID: 3901
	private Client discordClient;

	// Token: 0x04000F3E RID: 3902
	private bool isDiscordReady;

	// Token: 0x04000F3F RID: 3903
	private bool userRequestedDisconnect;

	// Token: 0x04000F40 RID: 3904
	private Activity currentActivity;

	// Token: 0x04000F41 RID: 3905
	private string codeVerifier;

	// Token: 0x04000F43 RID: 3907
	private const string PREFS_ACCESS_TOKEN = "Discord_AccessToken";

	// Token: 0x04000F44 RID: 3908
	private const string PREFS_TOKEN_EXPIRES_AT = "Discord_TokenExpiresAt";
}
