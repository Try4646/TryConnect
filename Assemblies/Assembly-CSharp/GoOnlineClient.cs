using System;
using System.Collections;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using Extensions;
using FMODUnity;
using Steamworks;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// Token: 0x02000219 RID: 537
public class GoOnlineClient : MonoBehaviour
{
	// Token: 0x060013A6 RID: 5030 RVA: 0x0005446E File Offset: 0x0005266E
	private void Start()
	{
		base.StartCoroutine(this.InitializeWithRetry());
	}

	// Token: 0x060013A7 RID: 5031 RVA: 0x00054480 File Offset: 0x00052680
	private void OnDestroy()
	{
		CallResult<LobbyCreated_t> callResult = this.lobbyCreated;
		if (callResult != null)
		{
			callResult.Dispose();
		}
		Callback<LobbyEnter_t> callback = this.lobbyEnterCallback;
		if (callback != null)
		{
			callback.Dispose();
		}
		if (this.chipImages != null)
		{
			foreach (Image image in this.chipImages)
			{
				if (image != null)
				{
					image.DOKill(false);
				}
			}
		}
	}

	// Token: 0x060013A8 RID: 5032 RVA: 0x000544E1 File Offset: 0x000526E1
	private IEnumerator InitializeWithRetry()
	{
		yield return base.StartCoroutine(this.SteamInitialization());
		if (!SteamManager.Initialized)
		{
			Debug.LogError("Initialization failed - Steam not initialized or authentication failed. Cannot proceed.");
			yield break;
		}
		this.LoadLobbySettings();
		this.LeaveExistingLobbyIfAny();
		this.InitializeUI();
		this.SetupLobbyCallbacks();
		this.StartProgressSequence();
		yield break;
	}

	// Token: 0x060013A9 RID: 5033 RVA: 0x000544F0 File Offset: 0x000526F0
	private IEnumerator SteamInitialization()
	{
		int maxRetries = 3;
		float retryDelay = 1f;
		int retryCount = 0;
		while (retryCount < maxRetries && !SteamManager.Initialized)
		{
			if (retryCount > 0)
			{
				this.UpdateRetryUI(retryCount, maxRetries);
				yield return new WaitForSeconds(retryDelay);
				retryDelay = Mathf.Min(retryDelay * 1.5f, 5f);
			}
			yield return new WaitForSeconds(0.5f);
			int num = retryCount;
			retryCount = num + 1;
		}
		if (!SteamManager.Initialized)
		{
			this.HandleSteamInitFailure();
			yield break;
		}
		yield return base.StartCoroutine(this.VerifyGameOwnership());
		yield break;
	}

	// Token: 0x060013AA RID: 5034 RVA: 0x000544FF File Offset: 0x000526FF
	private IEnumerator VerifyGameOwnership()
	{
		if (!SteamApps.BIsSubscribedApp(new AppId_t(3892270U)))
		{
			this.HandleGameOwnershipFailure();
			yield break;
		}
		Debug.Log("[Verified] Steam Initialization - Game ownership - [SUCCESSFUL]");
		yield break;
	}

	// Token: 0x060013AB RID: 5035 RVA: 0x0005450E File Offset: 0x0005270E
	private void HandleGameOwnershipFailure()
	{
		Debug.LogError("Steam Initialization - Game ownership verification failed! You do not own this game.");
		if (this.statusText != null)
		{
			this.statusText.text = "Access Denied: You do not own this game.";
		}
		base.enabled = false;
	}

	// Token: 0x060013AC RID: 5036 RVA: 0x00054540 File Offset: 0x00052740
	private void UpdateRetryUI(int retryCount, int maxRetries)
	{
		Debug.LogWarning(string.Format("Steam initialization attempt {0}/{1} failed. Retrying...", retryCount + 1, maxRetries));
		if (this.statusText != null)
		{
			this.statusText.text = string.Format("Steam initialization failed. Retrying... ({0}/{1})", retryCount + 1, maxRetries);
		}
	}

	// Token: 0x060013AD RID: 5037 RVA: 0x0005459C File Offset: 0x0005279C
	private void HandleSteamInitFailure()
	{
		Debug.LogError("Steam failed to initialize after multiple attempts!");
		if (this.statusText != null)
		{
			this.statusText.text = "Steam initialization failed! Please ensure Steam is running.";
		}
		if (this.chipImages != null)
		{
			foreach (Image image in this.chipImages)
			{
				if (image != null)
				{
					image.color = Color.red;
					image.gameObject.SetActive(true);
				}
			}
		}
		base.enabled = false;
	}

	// Token: 0x060013AE RID: 5038 RVA: 0x00054619 File Offset: 0x00052819
	private void LoadLobbySettings()
	{
		this.lobbySettings = Resources.Load<LobbySettings>("LobbySettings");
		if (this.lobbySettings == null)
		{
			Debug.LogError("Failed to load LobbySettings!");
		}
	}

	// Token: 0x060013AF RID: 5039 RVA: 0x00054644 File Offset: 0x00052844
	private void LeaveExistingLobbyIfAny()
	{
		if (this.lobbySettings == null || !SteamManager.Initialized)
		{
			return;
		}
		if (this.lobbySettings.steamLobbyID != CSteamID.Nil)
		{
			Debug.Log(string.Format("Leaving existing Steam lobby before creating a new one: {0}", this.lobbySettings.steamLobbyID));
			SteamMatchmaking.LeaveLobby(this.lobbySettings.steamLobbyID);
		}
		this.lobbySettings.steamLobbyID = CSteamID.Nil;
		this.lobbySettings.inALobby = false;
		this.lobbySettings.lobbyCode = "";
		this.lobbySettings.currentPlayerCount = 0;
		this.lobbySettings.NotifyChanged();
	}

	// Token: 0x060013B0 RID: 5040 RVA: 0x000546F0 File Offset: 0x000528F0
	private void SetupLobbyCallbacks()
	{
		this.lobbyCreated = CallResult<LobbyCreated_t>.Create(new CallResult<LobbyCreated_t>.APIDispatchDelegate(this.OnLobbyCreated));
		Callback<LobbyEnter_t> callback = this.lobbyEnterCallback;
		if (callback != null)
		{
			callback.Dispose();
		}
		this.lobbyEnterCallback = Callback<LobbyEnter_t>.Create(new Callback<LobbyEnter_t>.DispatchDelegate(this.OnLobbyEntered));
		this.hasProcessedLobbyEnter = false;
	}

	// Token: 0x060013B1 RID: 5041 RVA: 0x00054744 File Offset: 0x00052944
	private void InitializeUI()
	{
		if (this.chipImages != null)
		{
			foreach (Image image in this.chipImages)
			{
				if (image != null)
				{
					image.gameObject.SetActive(false);
					image.color = Color.white;
				}
			}
		}
		if (this.statusText != null)
		{
			this.statusText.text = this.statusMessages[0];
		}
		this.lastShownChipIndex = -1;
	}

	// Token: 0x060013B2 RID: 5042 RVA: 0x000547BA File Offset: 0x000529BA
	private void StartProgressSequence()
	{
		base.StartCoroutine(this.ProgressSequenceWithDelays());
	}

	// Token: 0x060013B3 RID: 5043 RVA: 0x000547C9 File Offset: 0x000529C9
	private IEnumerator ProgressSequenceWithDelays()
	{
		int num;
		for (int i = 0; i < 5; i = num + 1)
		{
			if (this.statusText != null && i < this.statusMessages.Length)
			{
				this.statusText.text = this.statusMessages[i];
			}
			this.ShowChipForProgress(i);
			SFXParams[] sFXParams = new SFXParams[]
			{
				new SFXParams("LoadingPercent", ((float)i + 1f) / 5f)
			};
			SFXManager.SFXOneShotWithParameters(this.loadingSFX, sFXParams, default(Vector3), 1f);
			yield return new WaitForSeconds(this.statusDisplayDelay);
			num = i;
		}
		this.CreateLobby();
		yield break;
	}

	// Token: 0x060013B4 RID: 5044 RVA: 0x000547D8 File Offset: 0x000529D8
	private void ShowChipForProgress(int statusIndex)
	{
		if (statusIndex <= this.lastShownChipIndex)
		{
			return;
		}
		int num = Mathf.Clamp(statusIndex, 0, this.chipImages.Length - 1);
		if (this.chipImages != null && num < this.chipImages.Length && this.chipImages[num] != null)
		{
			Image image = this.chipImages[num];
			this.ApplyRandomChipMaterial(image);
			image.gameObject.SetActive(true);
			image.transform.localScale = Vector3.zero;
			image.transform.DOScale(Vector3.one, this.chipAppearDuration).SetEase(Ease.OutBack);
			this.lastShownChipIndex = statusIndex;
		}
	}

	// Token: 0x060013B5 RID: 5045 RVA: 0x000048A7 File Offset: 0x00002AA7
	private void ApplyRandomChipMaterial(Image chipImage)
	{
	}

	// Token: 0x060013B6 RID: 5046 RVA: 0x00054878 File Offset: 0x00052A78
	public void CreateLobby()
	{
		SteamAPICall_t hAPICall = SteamMatchmaking.CreateLobby(ELobbyType.k_ELobbyTypePublic, this.lobbySettings.maxPlayers);
		this.lobbyCreated.Set(hAPICall, null);
	}

	// Token: 0x060013B7 RID: 5047 RVA: 0x000548A4 File Offset: 0x00052AA4
	private void OnLobbyCreated(LobbyCreated_t res, bool failure)
	{
		if (res.m_eResult != EResult.k_EResultOK)
		{
			Debug.LogError(string.Format("Lobby creation failed: {0}", res.m_eResult));
			if (this.statusText != null && 2 < this.statusMessages.Length)
			{
				this.statusText.text = this.statusMessages[2];
			}
			return;
		}
		base.StartCoroutine(this.ContinueAfterLobbyCreation(res));
	}

	// Token: 0x060013B8 RID: 5048 RVA: 0x0005490F File Offset: 0x00052B0F
	private IEnumerator ContinueAfterLobbyCreation(LobbyCreated_t res)
	{
		this.SetupLobbyData(res);
		this.InitializeWebSocket();
		if (SteamManager.Initialized)
		{
			base.StartCoroutine(this.JoinLobbyWithDelay());
		}
		else
		{
			Debug.LogError("Steam is not initialized! Cannot join lobby.");
		}
		yield break;
	}

	// Token: 0x060013B9 RID: 5049 RVA: 0x00054928 File Offset: 0x00052B28
	private void SetupLobbyData(LobbyCreated_t res)
	{
		this.lobbySettings.steamLobbyID = new CSteamID(res.m_ulSteamIDLobby);
		string pchValue = SteamUser.GetSteamID().ToString();
		SteamMatchmaking.SetLobbyData(this.lobbySettings.steamLobbyID, "name", SteamFriends.GetPersonaName() + "'s Lobby");
		SteamMatchmaking.SetLobbyData(this.lobbySettings.steamLobbyID, "HostAddress", pchValue);
		SteamMatchmaking.SetLobbyData(this.lobbySettings.steamLobbyID, "SelectedScene", "Gameplay");
		SteamMatchmaking.SetLobbyData(this.lobbySettings.steamLobbyID, "GameStarted", "0");
		SteamMatchmaking.SetLobbyData(this.lobbySettings.steamLobbyID, "LobbyCode", this.lobbySettings.lobbyCode);
		string version = Application.version;
		SteamMatchmaking.SetLobbyMemberData(this.lobbySettings.steamLobbyID, "GameVersion", version);
		Debug.Log("Set host game version on lobby creation: " + version);
	}

	// Token: 0x060013BA RID: 5050 RVA: 0x00054A20 File Offset: 0x00052C20
	private void InitializeWebSocket()
	{
		WebSocketManager webSocketManager = Object.FindFirstObjectByType<WebSocketManager>();
		if (!(webSocketManager != null))
		{
			Debug.LogError("WebSocketManager not found! Cannot initialize WebSocket connection.");
			return;
		}
		if (!webSocketManager.WebSocketFeaturesEnabled)
		{
			return;
		}
		webSocketManager.Initialize();
	}

	// Token: 0x060013BB RID: 5051 RVA: 0x00054A56 File Offset: 0x00052C56
	private IEnumerator JoinLobbyWithDelay()
	{
		yield return new WaitForSeconds(0.1f);
		if (SteamManager.Initialized)
		{
			SteamMatchmaking.JoinLobby(this.lobbySettings.steamLobbyID);
		}
		else
		{
			Debug.LogError("Steam is not initialized! Cannot join lobby.");
		}
		yield break;
	}

	// Token: 0x060013BC RID: 5052 RVA: 0x00054A68 File Offset: 0x00052C68
	private void OnLobbyEntered(LobbyEnter_t cb)
	{
		if (this.hasProcessedLobbyEnter)
		{
			return;
		}
		CSteamID csteamID = new CSteamID(cb.m_ulSteamIDLobby);
		if (csteamID != this.lobbySettings.steamLobbyID)
		{
			return;
		}
		this.hasProcessedLobbyEnter = true;
		this.lobbySettings.inALobby = true;
		Debug.Log(string.Format("Successfully joined own lobby {0}, LobbyCode: {1}, ensuring lobby is fully ready...", csteamID, this.lobbySettings.lobbyCode));
		base.StartCoroutine(this.WaitForLobbyReady());
		Callback<LobbyEnter_t> callback = this.lobbyEnterCallback;
		if (callback != null)
		{
			callback.Dispose();
		}
		this.lobbyEnterCallback = null;
	}

	// Token: 0x060013BD RID: 5053 RVA: 0x00054AF7 File Offset: 0x00052CF7
	private IEnumerator WaitForLobbyReady()
	{
		yield return null;
		int maxAttempts = 10;
		int num;
		for (int attempts = 0; attempts < maxAttempts; attempts = num + 1)
		{
			if (this.IsLobbyReady())
			{
				yield return new WaitForSeconds(0.5f);
				base.StartCoroutine(this.LoadMainMenuSceneRoutine());
				yield break;
			}
			yield return new WaitForSeconds(0.1f);
			num = attempts;
		}
		Debug.LogWarning("Lobby ready timeout reached, loading MainMenuScene anyway.");
		base.StartCoroutine(this.LoadMainMenuSceneRoutine());
		yield break;
	}

	// Token: 0x060013BE RID: 5054 RVA: 0x00054B08 File Offset: 0x00052D08
	private bool IsLobbyReady()
	{
		return !(this.lobbySettings.steamLobbyID == CSteamID.Nil) && SteamMatchmaking.GetNumLobbyMembers(this.lobbySettings.steamLobbyID) > 0 && SteamMatchmaking.GetLobbyMemberByIndex(this.lobbySettings.steamLobbyID, 0) != CSteamID.Nil;
	}

	// Token: 0x060013BF RID: 5055 RVA: 0x00054B5E File Offset: 0x00052D5E
	private IEnumerator LoadMainMenuSceneRoutine()
	{
		int nextSceneIndex = SceneManager.GetActiveScene().buildIndex + 1;
		if (nextSceneIndex < SceneManager.sceneCountInBuildSettings)
		{
			MonoSingleton<SceneTransitioner>.Instance.SetLoadingScreen(true, 0.3f, false);
			MonoSingleton<SceneTransitioner>.Instance.SetLoadingScreen(false, 0.3f, true);
			yield return new WaitForSeconds(0.3f);
			SceneManager.LoadScene(nextSceneIndex);
		}
		else
		{
			Debug.LogWarning("No more scenes to load. Check your Build Settings.");
		}
		yield break;
	}

	// Token: 0x04000C7C RID: 3196
	[Header("UI Progress Elements")]
	[SerializeField]
	private Image[] chipImages = new Image[5];

	// Token: 0x04000C7D RID: 3197
	[SerializeField]
	private TextMeshProUGUI statusText;

	// Token: 0x04000C7E RID: 3198
	[Header("Progress Settings")]
	[SerializeField]
	private float statusDisplayDelay = 1f;

	// Token: 0x04000C7F RID: 3199
	[SerializeField]
	private float chipAppearDuration = 0.5f;

	// Token: 0x04000C80 RID: 3200
	[Header("SFX")]
	[SerializeField]
	private EventReference loadingSFX;

	// Token: 0x04000C81 RID: 3201
	private readonly string[] statusMessages = new string[]
	{
		"Rolling the dice...",
		"Shuffling the deck...",
		"Dealing the cards...",
		"Stacking the chips...",
		"All bets are in!"
	};

	// Token: 0x04000C82 RID: 3202
	private LobbySettings lobbySettings;

	// Token: 0x04000C83 RID: 3203
	private CallResult<LobbyCreated_t> lobbyCreated;

	// Token: 0x04000C84 RID: 3204
	private Callback<LobbyEnter_t> lobbyEnterCallback;

	// Token: 0x04000C85 RID: 3205
	private bool hasProcessedLobbyEnter;

	// Token: 0x04000C86 RID: 3206
	private int lastShownChipIndex = -1;
}
