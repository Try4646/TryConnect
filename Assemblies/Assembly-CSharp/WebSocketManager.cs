using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Security;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using Extensions;
using Mirror;
using SkyBrave_Toolkit.Scripts.Scriptable_Game_Events;
using Steamworks;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using WebSocketSharp;

// Token: 0x02000114 RID: 276
public class WebSocketManager : MonoSingleton<WebSocketManager>
{
	// Token: 0x170000FA RID: 250
	// (get) Token: 0x06000B74 RID: 2932 RVA: 0x0002EBE0 File Offset: 0x0002CDE0
	public bool WebSocketFeaturesEnabled
	{
		get
		{
			return this.webSocketFeaturesEnabled;
		}
	}

	// Token: 0x06000B75 RID: 2933 RVA: 0x0002EBE8 File Offset: 0x0002CDE8
	private void RouteMessage(string message)
	{
		foreach (WebSocketManager.IMessageHandler messageHandler in this.messageHandlers.Values)
		{
			if (messageHandler.CanHandle(message))
			{
				messageHandler.Handle(message);
				WebSocketManager.BaseMessageHandler baseMessageHandler = messageHandler as WebSocketManager.BaseMessageHandler;
				if (baseMessageHandler != null)
				{
					baseMessageHandler.ShowMessage(messageHandler.GetDisplayText(message), -1f);
				}
				break;
			}
		}
	}

	// Token: 0x06000B76 RID: 2934 RVA: 0x0002EC68 File Offset: 0x0002CE68
	public void RegisterMessageHandler(string key, WebSocketManager.IMessageHandler handler)
	{
		if (this.messageHandlers.ContainsKey(key))
		{
			this.messageHandlers[key] = handler;
			return;
		}
		this.messageHandlers.Add(key, handler);
	}

	// Token: 0x06000B77 RID: 2935 RVA: 0x0002EC94 File Offset: 0x0002CE94
	protected override void OnAwake()
	{
		base.OnAwake();
		this.mainThreadCtx = SynchronizationContext.Current;
		this.gameSettings = Resources.Load<GameSettings>("GameSettings");
		this.lobbySettings = Resources.Load<LobbySettings>("LobbySettings");
		this.InitializeVoicePlayer();
		this.InitializeMessageHandlers();
		if (!this.webSocketFeaturesEnabled)
		{
			this.Disconnect();
		}
	}

	// Token: 0x06000B78 RID: 2936 RVA: 0x0002ECEC File Offset: 0x0002CEEC
	private void OnEnable()
	{
		SceneManager.sceneLoaded += this.OnSceneLoaded;
		if (SteamManager.Initialized)
		{
			this.lobbyEnterCallback = Callback<LobbyEnter_t>.Create(new Callback<LobbyEnter_t>.DispatchDelegate(this.OnLobbyEntered));
			this.lobbyDataUpdateCallback = Callback<LobbyDataUpdate_t>.Create(new Callback<LobbyDataUpdate_t>.DispatchDelegate(this.OnLobbyDataUpdate));
		}
	}

	// Token: 0x06000B79 RID: 2937 RVA: 0x0002ED3F File Offset: 0x0002CF3F
	private void OnDisable()
	{
		SceneManager.sceneLoaded -= this.OnSceneLoaded;
		Callback<LobbyEnter_t> callback = this.lobbyEnterCallback;
		if (callback != null)
		{
			callback.Dispose();
		}
		Callback<LobbyDataUpdate_t> callback2 = this.lobbyDataUpdateCallback;
		if (callback2 == null)
		{
			return;
		}
		callback2.Dispose();
	}

	// Token: 0x06000B7A RID: 2938 RVA: 0x0002ED74 File Offset: 0x0002CF74
	private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
	{
		if (scene.name == "MainMenuScene")
		{
			this.sessionCode = this.GenerateSessionCode();
			this.UpdateSessionCodeFromLobby();
			return;
		}
		if (scene.name == "NetworkSetupScene")
		{
			this.Disconnect();
		}
	}

	// Token: 0x06000B7B RID: 2939 RVA: 0x0002EDC0 File Offset: 0x0002CFC0
	private void OnLobbyEntered(LobbyEnter_t cb)
	{
		if (this.lobbySettings != null)
		{
			this.lobbySettings.steamLobbyID = new CSteamID(cb.m_ulSteamIDLobby);
			this.lobbySettings.inALobby = true;
			this.UpdateSessionCodeFromLobby();
		}
	}

	// Token: 0x06000B7C RID: 2940 RVA: 0x0002EDF8 File Offset: 0x0002CFF8
	private void OnLobbyDataUpdate(LobbyDataUpdate_t cb)
	{
		if (this.lobbySettings == null || this.lobbySettings.steamLobbyID == CSteamID.Nil)
		{
			return;
		}
		if ((CSteamID)cb.m_ulSteamIDLobby != this.lobbySettings.steamLobbyID)
		{
			return;
		}
		if (cb.m_ulSteamIDMember == cb.m_ulSteamIDLobby)
		{
			string lobbyData = SteamMatchmaking.GetLobbyData(this.lobbySettings.steamLobbyID, "LobbyCode");
			if (!string.IsNullOrEmpty(lobbyData) && lobbyData != this.sessionCode && !this.IsLobbyOwner())
			{
				this.sessionCode = lobbyData;
				this.ownerNickname = this.GetLobbyOwnerNickname();
				this.UpdateUI();
			}
		}
	}

	// Token: 0x06000B7D RID: 2941 RVA: 0x0002EEA4 File Offset: 0x0002D0A4
	private void UpdateSessionCodeFromLobby()
	{
		if (this.IsLobbyOwner())
		{
			SteamMatchmaking.SetLobbyData(this.lobbySettings.steamLobbyID, "LobbyCode", this.sessionCode);
			this.lobbySettings.lobbyCode = this.sessionCode;
		}
		else
		{
			string lobbyData = SteamMatchmaking.GetLobbyData(this.lobbySettings.steamLobbyID, "LobbyCode");
			if (!string.IsNullOrEmpty(lobbyData))
			{
				this.sessionCode = lobbyData;
			}
		}
		this.ownerNickname = this.GetLobbyOwnerNickname();
		this.UpdateUI();
	}

	// Token: 0x06000B7E RID: 2942 RVA: 0x0002EF20 File Offset: 0x0002D120
	private bool IsLobbyOwner()
	{
		if (!SteamManager.Initialized || this.lobbySettings == null || this.lobbySettings.steamLobbyID == CSteamID.Nil)
		{
			return false;
		}
		bool result;
		try
		{
			result = (SteamMatchmaking.GetLobbyOwner(this.lobbySettings.steamLobbyID) == SteamUser.GetSteamID());
		}
		catch
		{
			result = false;
		}
		return result;
	}

	// Token: 0x06000B7F RID: 2943 RVA: 0x0002EF90 File Offset: 0x0002D190
	private void InitializeVoicePlayer()
	{
		this.voicePlayer = Object.FindFirstObjectByType<VoiceAnnouncementPlayer>();
		if (this.voicePlayer == null)
		{
			GameObject gameObject = new GameObject("VoiceAnnouncementPlayer");
			this.voicePlayer = gameObject.AddComponent<VoiceAnnouncementPlayer>();
			Object.DontDestroyOnLoad(gameObject);
		}
	}

	// Token: 0x06000B80 RID: 2944 RVA: 0x0002EFD3 File Offset: 0x0002D1D3
	private void InitializeMessageHandlers()
	{
		this.RegisterMessageHandler("speed", new WebSocketManager.SpeedMessageHandler(this.messagePrefab, this.messageParent, this.gameSettings));
		this.RegisterMessageHandler("default", new WebSocketManager.DefaultMessageHandler(this.messagePrefab, this.messageParent));
	}

	// Token: 0x06000B81 RID: 2945 RVA: 0x0002F014 File Offset: 0x0002D214
	private string GenerateSessionCode()
	{
		if (this.lobbySettings == null)
		{
			return "";
		}
		int num = (this.lobbySettings.codeLength > 0) ? this.lobbySettings.codeLength : 6;
		StringBuilder stringBuilder = new StringBuilder(num);
		Random random = new Random();
		for (int i = 0; i < num; i++)
		{
			stringBuilder.Append("1234567890"[random.Next("1234567890".Length)]);
		}
		return stringBuilder.ToString();
	}

	// Token: 0x06000B82 RID: 2946 RVA: 0x000048A7 File Offset: 0x00002AA7
	public void Initialize()
	{
	}

	// Token: 0x06000B83 RID: 2947 RVA: 0x0002F094 File Offset: 0x0002D294
	private void Disconnect()
	{
		this.shouldPing = false;
		object obj = this.wsLock;
		lock (obj)
		{
			if (this.ws != null)
			{
				if (this.ws.IsAlive)
				{
					this.ws.Close();
				}
				this.ws = null;
			}
		}
		this.isConnecting = false;
	}

	// Token: 0x06000B84 RID: 2948 RVA: 0x0002F104 File Offset: 0x0002D304
	private void UpdateUI()
	{
		if (this.sessionCodeLabel != null)
		{
			if (!string.IsNullOrEmpty(this.ownerNickname))
			{
				this.sessionCodeLabel.text = this.sessionCode + " | " + this.ownerNickname + "'s Lobby";
				return;
			}
			this.sessionCodeLabel.text = this.sessionCode;
		}
	}

	// Token: 0x06000B85 RID: 2949 RVA: 0x0002F164 File Offset: 0x0002D364
	private string GetLobbyOwnerNickname()
	{
		if (!SteamManager.Initialized || this.lobbySettings == null || this.lobbySettings.steamLobbyID == CSteamID.Nil)
		{
			return string.Empty;
		}
		try
		{
			CSteamID lobbyOwner = SteamMatchmaking.GetLobbyOwner(this.lobbySettings.steamLobbyID);
			if (lobbyOwner != CSteamID.Nil)
			{
				string friendPersonaName = SteamFriends.GetFriendPersonaName(lobbyOwner);
				return string.IsNullOrEmpty(friendPersonaName) ? SteamFriends.GetPersonaName() : friendPersonaName;
			}
		}
		catch
		{
		}
		return string.Empty;
	}

	// Token: 0x06000B86 RID: 2950 RVA: 0x0002F1F8 File Offset: 0x0002D3F8
	private void StartWebSocketThread()
	{
		if (!this.WebSocketFeaturesEnabled || this.isConnecting)
		{
			return;
		}
		this.shouldPing = true;
		this.isConnecting = true;
		Debug.Log("[WebSocket] Connection attempt started. sessionCode=" + this.sessionCode + ", ownerNickname=" + this.ownerNickname);
		this.wsThread = new Thread(delegate()
		{
			try
			{
				this.Connect();
			}
			catch (Exception ex)
			{
				Debug.LogError("WebSocket thread error: " + ex.Message);
				object obj = this.wsLock;
				lock (obj)
				{
					this.isConnecting = false;
				}
			}
		});
		this.wsThread.IsBackground = true;
		this.wsThread.Start();
	}

	// Token: 0x06000B87 RID: 2951 RVA: 0x0002F274 File Offset: 0x0002D474
	private void Connect()
	{
		object obj = this.wsLock;
		lock (obj)
		{
			if (!this.WebSocketFeaturesEnabled)
			{
				this.isConnecting = false;
			}
			else
			{
				try
				{
					string url = this.BuildWebSocketUrl();
					this.ws = new WebSocket(url, Array.Empty<string>());
					this.ws.SslConfiguration.EnabledSslProtocols = SslProtocols.Tls12;
					this.ws.SslConfiguration.ServerCertificateValidationCallback = ((object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors) => true);
					this.ws.WaitTime = TimeSpan.FromHours(24.0);
					this.ws.OnOpen += delegate(object s, EventArgs e)
					{
						Debug.Log("[WebSocket] Connected.");
						UnityMainThreadDispatcher.Enqueue(delegate
						{
							this.webSocketEvent.Raise();
						});
						new Thread(delegate()
						{
							while (this.shouldPing && this.ws != null && this.ws.IsAlive)
							{
								this.SendPing();
								Thread.Sleep(30000);
							}
						})
						{
							IsBackground = true
						}.Start();
					};
					this.ws.OnMessage += delegate(object s, MessageEventArgs e)
					{
						string rawData = e.Data;
						string messageType = "text";
						try
						{
							WebSocketManager.VoiceMessage voiceMsg = JsonUtility.FromJson<WebSocketManager.VoiceMessage>(rawData);
							if (!string.IsNullOrEmpty(voiceMsg.type) && (voiceMsg.type == "voice_start" || voiceMsg.type == "voice_audio" || voiceMsg.type == "voice_stop"))
							{
								messageType = "voice";
								UnityMainThreadDispatcher.Enqueue(delegate
								{
									this.RelayMessageToClients(rawData, messageType);
									this.HandleVoiceMessage(voiceMsg);
								});
								return;
							}
						}
						catch
						{
						}
						try
						{
							WebSocketManager.GameSettingsChangedMessage settingsChangedMsg = JsonUtility.FromJson<WebSocketManager.GameSettingsChangedMessage>(rawData);
							if ((!string.IsNullOrEmpty(settingsChangedMsg.type) && (settingsChangedMsg.type == "gameSettingsChanged" || settingsChangedMsg.type == "mainGameSettingsChanged")) || settingsChangedMsg.isMainGameSettings)
							{
								messageType = "settings";
								UnityMainThreadDispatcher.Enqueue(delegate
								{
									this.RelayMessageToClients(rawData, messageType);
									this.HandleGameSettingsChanged(settingsChangedMsg);
								});
								return;
							}
						}
						catch
						{
						}
						UnityMainThreadDispatcher.Enqueue(delegate
						{
							this.RelayMessageToClients(rawData, messageType);
							try
							{
								WebSocketManager.ServerMessage serverMessage = JsonUtility.FromJson<WebSocketManager.ServerMessage>(rawData);
								this.RouteMessage(((serverMessage != null) ? serverMessage.message : null) ?? rawData);
							}
							catch
							{
								this.RouteMessage(rawData);
							}
						});
					};
					this.ws.OnClose += delegate(object s, CloseEventArgs e)
					{
						Debug.Log(string.Format("[WebSocket] Disconnected. code={0}, reason={1}, clean={2}", e.Code, e.Reason, e.WasClean));
						object obj3 = this.wsLock;
						lock (obj3)
						{
							this.isConnecting = false;
						}
					};
					this.ws.OnError += delegate(object s, ErrorEventArgs e)
					{
						Debug.LogError("[WebSocket] Error: " + e.Message);
						object obj3 = this.wsLock;
						lock (obj3)
						{
							this.isConnecting = false;
						}
					};
					Debug.Log("[WebSocket] Connecting...");
					this.ws.Connect();
				}
				catch (Exception ex)
				{
					Debug.LogError("[WebSocket] Failed to establish connection: " + ex.Message);
					object obj2 = this.wsLock;
					lock (obj2)
					{
						this.isConnecting = false;
					}
				}
			}
		}
	}

	// Token: 0x06000B88 RID: 2952 RVA: 0x0002F428 File Offset: 0x0002D628
	private string BuildWebSocketUrl()
	{
		string str = Uri.EscapeDataString(this.sessionCode ?? string.Empty);
		string str2 = Uri.EscapeDataString(this.ownerNickname ?? string.Empty);
		return "wss://api.diabolical.studio/socket/?sessionCode=" + str + "&ownerNickname=" + str2;
	}

	// Token: 0x06000B89 RID: 2953 RVA: 0x0002F470 File Offset: 0x0002D670
	private void SendPing()
	{
		object obj = this.wsLock;
		lock (obj)
		{
			if (this.ws != null && this.ws.IsAlive)
			{
				try
				{
					this.ws.Ping();
				}
				catch (Exception ex)
				{
					Debug.LogError("Ping failed: " + ex.Message);
				}
			}
		}
	}

	// Token: 0x06000B8A RID: 2954 RVA: 0x0002F4F4 File Offset: 0x0002D6F4
	private void HandleVoiceMessage(WebSocketManager.VoiceMessage voiceMsg)
	{
		if (this.voicePlayer == null)
		{
			this.InitializeVoicePlayer();
			if (this.voicePlayer == null)
			{
				return;
			}
		}
		string type = voiceMsg.type;
		BossAnnouncement[] array;
		if (!(type == "voice_start"))
		{
			if (!(type == "voice_audio"))
			{
				if (!(type == "voice_stop"))
				{
					return;
				}
				this.voicePlayer.OnVoiceStop();
				array = Object.FindObjectsByType<BossAnnouncement>(FindObjectsInactive.Include, FindObjectsSortMode.None);
				for (int i = 0; i < array.Length; i++)
				{
					array[i].StartSlideOut();
				}
			}
			else if (!string.IsNullOrEmpty(voiceMsg.data))
			{
				this.voicePlayer.OnVoiceAudio(voiceMsg.data);
				return;
			}
			return;
		}
		this.voicePlayer.OnVoiceStart();
		array = Object.FindObjectsByType<BossAnnouncement>(FindObjectsInactive.Include, FindObjectsSortMode.None);
		for (int i = 0; i < array.Length; i++)
		{
			array[i].StartSlideIn();
		}
	}

	// Token: 0x06000B8B RID: 2955 RVA: 0x0002F5C8 File Offset: 0x0002D7C8
	private void HandleGameSettingsChanged(WebSocketManager.GameSettingsChangedMessage settingsMsg)
	{
		if (settingsMsg.isMainGameSettings || (!string.IsNullOrEmpty(settingsMsg.type) && settingsMsg.type == "mainGameSettingsChanged") || string.IsNullOrEmpty(settingsMsg.subGameName))
		{
			if (NetworkSingleton<GameSettingsAPIManager>.Instance != null)
			{
				NetworkSingleton<GameSettingsAPIManager>.Instance.ReloadMainSettings();
				return;
			}
		}
		else if (NetworkSingleton<RequestSettingsFromApi>.Instance != null)
		{
			NetworkSingleton<RequestSettingsFromApi>.Instance.ReloadSettings();
		}
	}

	// Token: 0x06000B8C RID: 2956 RVA: 0x0002F63B File Offset: 0x0002D83B
	private void Start()
	{
		base.StartCoroutine(this.CheckForGameStart());
		base.StartCoroutine(this.MonitorAndRegisterClientHandler());
	}

	// Token: 0x06000B8D RID: 2957 RVA: 0x0002F657 File Offset: 0x0002D857
	private IEnumerator CheckForGameStart()
	{
		bool wasServerActive = false;
		for (;;)
		{
			bool active = NetworkServer.active;
			if (!this.WebSocketFeaturesEnabled)
			{
				this.Disconnect();
				this.nextConnectAttemptTime = 0f;
				wasServerActive = active;
				yield return new WaitForSeconds(0.5f);
			}
			else
			{
				if (wasServerActive && !active)
				{
					this.Disconnect();
					this.nextConnectAttemptTime = 0f;
				}
				if (this.WebSocketFeaturesEnabled && active && !this.isConnecting && (this.ws == null || !this.ws.IsAlive) && !string.IsNullOrEmpty(this.sessionCode) && this.IsLobbyOwner() && Time.unscaledTime >= this.nextConnectAttemptTime)
				{
					this.StartWebSocketThread();
					this.nextConnectAttemptTime = Time.unscaledTime + 300f;
				}
				wasServerActive = active;
				yield return new WaitForSeconds(0.5f);
			}
		}
		yield break;
	}

	// Token: 0x06000B8E RID: 2958 RVA: 0x0002F666 File Offset: 0x0002D866
	private IEnumerator MonitorAndRegisterClientHandler()
	{
		bool wasClientActive = false;
		for (;;)
		{
			bool active = NetworkClient.active;
			if (active && !wasClientActive)
			{
				NetworkClient.ReplaceHandler<WebSocketRelayMessage>(new Action<WebSocketRelayMessage>(this.OnWebSocketRelayMessage), true);
			}
			wasClientActive = active;
			yield return new WaitForSeconds(0.5f);
		}
		yield break;
	}

	// Token: 0x06000B8F RID: 2959 RVA: 0x0002F675 File Offset: 0x0002D875
	private bool IsNetworkHost()
	{
		return NetworkServer.active;
	}

	// Token: 0x06000B90 RID: 2960 RVA: 0x0002F67C File Offset: 0x0002D87C
	private void RelayMessageToClients(string rawMessage, string messageType = "text")
	{
		if (!this.IsNetworkHost() || !NetworkServer.active)
		{
			return;
		}
		NetworkServer.SendToAll<WebSocketRelayMessage>(new WebSocketRelayMessage
		{
			rawData = rawMessage,
			messageType = messageType
		}, 0, false);
	}

	// Token: 0x06000B91 RID: 2961 RVA: 0x0002F6BC File Offset: 0x0002D8BC
	private void ProcessRelayedMessage(WebSocketRelayMessage message)
	{
		if (this.IsNetworkHost())
		{
			return;
		}
		string rawData = message.rawData;
		try
		{
			WebSocketManager.VoiceMessage voiceMessage = JsonUtility.FromJson<WebSocketManager.VoiceMessage>(rawData);
			if (!string.IsNullOrEmpty(voiceMessage.type) && (voiceMessage.type == "voice_start" || voiceMessage.type == "voice_audio" || voiceMessage.type == "voice_stop"))
			{
				this.HandleVoiceMessage(voiceMessage);
				return;
			}
		}
		catch
		{
		}
		try
		{
			WebSocketManager.GameSettingsChangedMessage gameSettingsChangedMessage = JsonUtility.FromJson<WebSocketManager.GameSettingsChangedMessage>(rawData);
			if ((!string.IsNullOrEmpty(gameSettingsChangedMessage.type) && (gameSettingsChangedMessage.type == "gameSettingsChanged" || gameSettingsChangedMessage.type == "mainGameSettingsChanged")) || gameSettingsChangedMessage.isMainGameSettings)
			{
				this.HandleGameSettingsChanged(gameSettingsChangedMessage);
				return;
			}
		}
		catch
		{
		}
		try
		{
			WebSocketManager.ServerMessage serverMessage = JsonUtility.FromJson<WebSocketManager.ServerMessage>(rawData);
			this.RouteMessage(((serverMessage != null) ? serverMessage.message : null) ?? rawData);
		}
		catch
		{
			this.RouteMessage(rawData);
		}
	}

	// Token: 0x06000B92 RID: 2962 RVA: 0x0002F7DC File Offset: 0x0002D9DC
	private void OnWebSocketRelayMessage(WebSocketRelayMessage message)
	{
		this.ProcessRelayedMessage(message);
	}

	// Token: 0x06000B93 RID: 2963 RVA: 0x0002F7E5 File Offset: 0x0002D9E5
	private void OnApplicationQuit()
	{
		this.shouldPing = false;
		this.Disconnect();
		if (this.wsThread != null && this.wsThread.IsAlive)
		{
			this.wsThread.Join(1000);
		}
	}

	// Token: 0x06000B94 RID: 2964 RVA: 0x0002F81C File Offset: 0x0002DA1C
	private void OnDestroy()
	{
		SceneManager.sceneLoaded -= this.OnSceneLoaded;
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
		if (NetworkClient.active)
		{
			NetworkClient.UnregisterHandler<WebSocketRelayMessage>();
		}
		this.Disconnect();
	}

	// Token: 0x0400071E RID: 1822
	[SerializeField]
	private GameEvent webSocketEvent;

	// Token: 0x0400071F RID: 1823
	[SerializeField]
	private TextMeshProUGUI sessionCodeLabel;

	// Token: 0x04000720 RID: 1824
	[Tooltip("When off, no outbound WebSocket connection, ping, or reconnect logic runs.")]
	[SerializeField]
	private bool webSocketFeaturesEnabled = true;

	// Token: 0x04000721 RID: 1825
	private WebSocket ws;

	// Token: 0x04000722 RID: 1826
	private string sessionCode = "";

	// Token: 0x04000723 RID: 1827
	private string ownerNickname = "";

	// Token: 0x04000724 RID: 1828
	private SynchronizationContext mainThreadCtx;

	// Token: 0x04000725 RID: 1829
	private Thread wsThread;

	// Token: 0x04000726 RID: 1830
	private bool isConnecting;

	// Token: 0x04000727 RID: 1831
	private readonly object wsLock = new object();

	// Token: 0x04000728 RID: 1832
	private const float PING_INTERVAL = 30f;

	// Token: 0x04000729 RID: 1833
	private const float RECONNECT_INTERVAL = 300f;

	// Token: 0x0400072A RID: 1834
	private bool shouldPing = true;

	// Token: 0x0400072B RID: 1835
	private float nextConnectAttemptTime;

	// Token: 0x0400072C RID: 1836
	public GameObject messagePrefab;

	// Token: 0x0400072D RID: 1837
	public Transform messageParent;

	// Token: 0x0400072E RID: 1838
	private GameSettings gameSettings;

	// Token: 0x0400072F RID: 1839
	private LobbySettings lobbySettings;

	// Token: 0x04000730 RID: 1840
	private VoiceAnnouncementPlayer voicePlayer;

	// Token: 0x04000731 RID: 1841
	private Dictionary<string, WebSocketManager.IMessageHandler> messageHandlers = new Dictionary<string, WebSocketManager.IMessageHandler>();

	// Token: 0x04000732 RID: 1842
	private Callback<LobbyEnter_t> lobbyEnterCallback;

	// Token: 0x04000733 RID: 1843
	private Callback<LobbyDataUpdate_t> lobbyDataUpdateCallback;

	// Token: 0x02000115 RID: 277
	[Serializable]
	public class ServerMessage
	{
		// Token: 0x04000734 RID: 1844
		public string message;
	}

	// Token: 0x02000116 RID: 278
	[Serializable]
	public class VoiceMessage
	{
		// Token: 0x04000735 RID: 1845
		public string type;

		// Token: 0x04000736 RID: 1846
		public string message;

		// Token: 0x04000737 RID: 1847
		public string data;

		// Token: 0x04000738 RID: 1848
		public int sampleRate;

		// Token: 0x04000739 RID: 1849
		public int channels;

		// Token: 0x0400073A RID: 1850
		public string format;
	}

	// Token: 0x02000117 RID: 279
	[Serializable]
	public class GameSettingsChangedMessage
	{
		// Token: 0x0400073B RID: 1851
		public string type;

		// Token: 0x0400073C RID: 1852
		public string gameId;

		// Token: 0x0400073D RID: 1853
		public string subGameName;

		// Token: 0x0400073E RID: 1854
		public string message;

		// Token: 0x0400073F RID: 1855
		public bool isMainGameSettings;
	}

	// Token: 0x02000118 RID: 280
	public interface IMessageHandler
	{
		// Token: 0x06000BA0 RID: 2976
		bool CanHandle(string message);

		// Token: 0x06000BA1 RID: 2977
		void Handle(string message);

		// Token: 0x06000BA2 RID: 2978
		string GetDisplayText(string message);
	}

	// Token: 0x02000119 RID: 281
	public abstract class BaseMessageHandler : WebSocketManager.IMessageHandler
	{
		// Token: 0x06000BA3 RID: 2979 RVA: 0x0002FC0C File Offset: 0x0002DE0C
		public BaseMessageHandler(GameObject prefab, Transform parent, float duration = 2f)
		{
			this.messagePrefab = prefab;
			this.messageParent = parent;
			this.displayDuration = duration;
		}

		// Token: 0x06000BA4 RID: 2980
		public abstract bool CanHandle(string message);

		// Token: 0x06000BA5 RID: 2981
		public abstract void Handle(string message);

		// Token: 0x06000BA6 RID: 2982
		public abstract string GetDisplayText(string message);

		// Token: 0x06000BA7 RID: 2983 RVA: 0x0002FC34 File Offset: 0x0002DE34
		public void ShowMessage(string text, float duration = -1f)
		{
			if (!this.messagePrefab || !this.messageParent)
			{
				return;
			}
			GameObject gameObject = Object.Instantiate<GameObject>(this.messagePrefab, this.messageParent);
			gameObject.GetComponentInChildren<TextMeshProUGUI>().text = text;
			Object.Destroy(gameObject, (duration > 0f) ? duration : this.displayDuration);
		}

		// Token: 0x04000740 RID: 1856
		protected GameObject messagePrefab;

		// Token: 0x04000741 RID: 1857
		protected Transform messageParent;

		// Token: 0x04000742 RID: 1858
		protected float displayDuration = 2f;
	}

	// Token: 0x0200011A RID: 282
	public class SpeedMessageHandler : WebSocketManager.BaseMessageHandler
	{
		// Token: 0x06000BA8 RID: 2984 RVA: 0x0002FC8F File Offset: 0x0002DE8F
		public SpeedMessageHandler(GameObject prefab, Transform parent, GameSettings settings) : base(prefab, parent, 2f)
		{
			this.gameSettings = settings;
		}

		// Token: 0x06000BA9 RID: 2985 RVA: 0x0002FCA5 File Offset: 0x0002DEA5
		public override bool CanHandle(string message)
		{
			return message.ToLower().Contains("speed");
		}

		// Token: 0x06000BAA RID: 2986 RVA: 0x0002FCB8 File Offset: 0x0002DEB8
		public override void Handle(string message)
		{
			Match match = Regex.Match(message, "-?\\d*\\.?\\d+");
			float timeScale;
			if (match.Success && float.TryParse(match.Value, out timeScale))
			{
				this.gameSettings.SetTimeScale(timeScale);
			}
		}

		// Token: 0x06000BAB RID: 2987 RVA: 0x00013703 File Offset: 0x00011903
		public override string GetDisplayText(string message)
		{
			return message;
		}

		// Token: 0x04000743 RID: 1859
		private GameSettings gameSettings;
	}

	// Token: 0x0200011B RID: 283
	public class DefaultMessageHandler : WebSocketManager.BaseMessageHandler
	{
		// Token: 0x06000BAC RID: 2988 RVA: 0x0002FCF4 File Offset: 0x0002DEF4
		public DefaultMessageHandler(GameObject prefab, Transform parent) : base(prefab, parent, 5f)
		{
		}

		// Token: 0x06000BAD RID: 2989 RVA: 0x00002321 File Offset: 0x00000521
		public override bool CanHandle(string message)
		{
			return true;
		}

		// Token: 0x06000BAE RID: 2990 RVA: 0x000048A7 File Offset: 0x00002AA7
		public override void Handle(string message)
		{
		}

		// Token: 0x06000BAF RID: 2991 RVA: 0x00013703 File Offset: 0x00011903
		public override string GetDisplayText(string message)
		{
			return message;
		}
	}
}
