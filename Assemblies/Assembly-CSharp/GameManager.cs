using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using Extensions;
using Mirror;
using Mirror.RemoteCalls;
using MoreMountains.Feedbacks;
using SkyBrave_Toolkit.Scripts.Scriptable_Game_Events;
using Steamworks;
using UnityEngine;

// Token: 0x02000158 RID: 344
public class GameManager : NetworkSingleton<GameManager>
{
	// Token: 0x1400000B RID: 11
	// (add) Token: 0x06000D08 RID: 3336 RVA: 0x0003705C File Offset: 0x0003525C
	// (remove) Token: 0x06000D09 RID: 3337 RVA: 0x00037094 File Offset: 0x00035294
	public event Action<long, long> OnQuotaChangedEvent;

	// Token: 0x17000114 RID: 276
	// (get) Token: 0x06000D0A RID: 3338 RVA: 0x000370C9 File Offset: 0x000352C9
	// (set) Token: 0x06000D0B RID: 3339 RVA: 0x000370D1 File Offset: 0x000352D1
	public bool HasDayStarted { get; private set; }

	// Token: 0x06000D0C RID: 3340 RVA: 0x000370DA File Offset: 0x000352DA
	protected override void OnAwake()
	{
		base.OnAwake();
		this._gs = Resources.Load<GameSettings>("GameSettings");
	}

	// Token: 0x06000D0D RID: 3341 RVA: 0x000370F2 File Offset: 0x000352F2
	public override void OnStartServer()
	{
		base.StartCoroutine(NetworkSingleton<SaveManager>.Instance.LoadGameSaveCoroutine());
	}

	// Token: 0x06000D0E RID: 3342 RVA: 0x00037105 File Offset: 0x00035305
	private void OnTimerChanged(float oldValue, float newValue)
	{
		NetworkSingleton<GameUI>.Instance.SetTimerText(this._gs.dayDuration - newValue);
	}

	// Token: 0x06000D0F RID: 3343 RVA: 0x000048A7 File Offset: 0x00002AA7
	private void OnDaysChanged(int oldValue, int newValue)
	{
	}

	// Token: 0x06000D10 RID: 3344 RVA: 0x0003711E File Offset: 0x0003531E
	private void OnDaysPassedChanged(int oldValue, int newValue)
	{
		NetworkSingleton<GameUI>.Instance.SetDaysText(newValue + 1);
	}

	// Token: 0x06000D11 RID: 3345 RVA: 0x0003712D File Offset: 0x0003532D
	private void OnQuotaChanged(long oldValue, long newValue)
	{
		Action<long, long> onQuotaChangedEvent = this.OnQuotaChangedEvent;
		if (onQuotaChangedEvent != null)
		{
			onQuotaChangedEvent(oldValue, newValue);
		}
		NetworkSingleton<GameUI>.Instance.SetFloorText(this.currentFloor);
	}

	// Token: 0x06000D12 RID: 3346 RVA: 0x00037152 File Offset: 0x00035352
	public void StartDay()
	{
		this.HasDayStarted = true;
	}

	// Token: 0x06000D13 RID: 3347 RVA: 0x0003715B File Offset: 0x0003535B
	private void Update()
	{
		if (!base.isServer)
		{
			return;
		}
		this.SetTimer();
	}

	// Token: 0x06000D14 RID: 3348 RVA: 0x0003716C File Offset: 0x0003536C
	private void SetTimer()
	{
		if (this.state != GameState.Game || this._isTransitioning)
		{
			return;
		}
		if (!this.HasDayStarted)
		{
			return;
		}
		if (this._isTimeOver)
		{
			return;
		}
		this.Network_timer = Mathf.Min(this._timer + Time.deltaTime, this._gs.dayDuration);
		if (this._timer >= this._gs.dayDuration)
		{
			base.StartCoroutine(this.OnTimerEnd());
		}
	}

	// Token: 0x06000D15 RID: 3349 RVA: 0x000371E0 File Offset: 0x000353E0
	[Server]
	public void ServerAdjustTimer(float deltaSeconds)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void GameManager::ServerAdjustTimer(System.Single)' called when server was not active");
			return;
		}
		if (Mathf.Approximately(deltaSeconds, 0f) || this._gs == null)
		{
			return;
		}
		float network_timer = Mathf.Clamp(this._timer + deltaSeconds, 0f, this._gs.dayDuration);
		this.Network_timer = network_timer;
	}

	// Token: 0x06000D16 RID: 3350 RVA: 0x00037243 File Offset: 0x00035443
	private IEnumerator OnTimerEnd()
	{
		this._isTimeOver = true;
		this.RpcPlayDayEndFeedback();
		GameEvent gameEvent = this.onDayEnded;
		if (gameEvent != null)
		{
			gameEvent.Raise();
		}
		yield return new WaitForSeconds(0.5f);
		foreach (ConsumableItem consumableItem in NetworkSingleton<ItemManager>.Instance.spawnedItemInstances.ToList<ConsumableItem>())
		{
			consumableItem.DestroyItem();
		}
		NetworkSingleton<ElevatorManager>.Instance.IsLocked = true;
		NetworkSingleton<ElevatorManager>.Instance.ServerForceTeleportPlayers(0);
		yield break;
	}

	// Token: 0x06000D17 RID: 3351 RVA: 0x00037252 File Offset: 0x00035452
	private void ServerPayDebt()
	{
		NetworkSingleton<MoneyManager>.Instance.TryChangeTicketBalance((long)this.currentTicketReward);
		this.ProgressNextQuota();
	}

	// Token: 0x06000D18 RID: 3352 RVA: 0x0003726C File Offset: 0x0003546C
	[Server]
	private void ProgressNextQuota()
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void GameManager::ProgressNextQuota()' called when server was not active");
			return;
		}
		this.NetworksuccessfulQuota = this.successfulQuota + 1;
		this.NetworkdaysLeft = this._gs.daysBeforeQuota;
		NetworkSingleton<MoneyManager>.Instance.TryChangeTicketBalance((long)this._gs.GetQuotaExcessReward(this.currentFloor, this.currentQuota, NetworkSingleton<MoneyManager>.Instance.balance));
		this.NetworkcurrentQuota = this._gs.GetQuota(this.successfulQuota, this.currentQuota, NetworkSingleton<MoneyManager>.Instance.balance);
		GameEvent gameEvent = this.onQuotaAchieved;
		if (gameEvent != null)
		{
			gameEvent.Raise();
		}
		if ((long)this.successfulQuota >= this.requiredQuotaToNextFloor)
		{
			this.ProgressFloor();
		}
	}

	// Token: 0x06000D19 RID: 3353 RVA: 0x00037327 File Offset: 0x00035527
	[Server]
	public void ServerGetAuxiliaryMoney()
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void GameManager::ServerGetAuxiliaryMoney()' called when server was not active");
			return;
		}
		NetworkSingleton<MoneyManager>.Instance.TryChangeBalance(this._gs.GetAuxiliaryMoney(this.daysLeft, this.currentQuota), null, ChangeType.Misc);
	}

	// Token: 0x06000D1A RID: 3354 RVA: 0x00037364 File Offset: 0x00035564
	[Server]
	private void ProgressFloor()
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void GameManager::ProgressFloor()' called when server was not active");
			return;
		}
		this.NetworkcurrentFloor = this.currentFloor + 1;
		if (this._gs.floorData != null && this.currentFloor + 1 < this._gs.floorData.Count)
		{
			this.NetworkrequiredQuotaToNextFloor = this._gs.floorData[this.currentFloor + 1].requiredQuotaToAccess;
		}
		else
		{
			this.NetworkrequiredQuotaToNextFloor = long.MaxValue;
		}
		GameEvent gameEvent = this.onFloorProgressed;
		if (gameEvent == null)
		{
			return;
		}
		gameEvent.Raise();
	}

	// Token: 0x06000D1B RID: 3355 RVA: 0x000373FF File Offset: 0x000355FF
	[Server]
	public void ServerSetScene(GameState newState)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void GameManager::ServerSetScene(GameState)' called when server was not active");
			return;
		}
		base.StartCoroutine(this.ServerSetSceneRoutine(newState));
	}

	// Token: 0x06000D1C RID: 3356 RVA: 0x00037424 File Offset: 0x00035624
	private IEnumerator ServerSetSceneRoutine(GameState newState)
	{
		if (this._isTransitioning)
		{
			yield break;
		}
		if (this.state == newState)
		{
			yield break;
		}
		this._isTransitioning = true;
		this.RpcResetSummaryAndGameOverUI();
		NetworkSingleton<GameUI>.Instance.ServerSetLoadingScreen(true, 0.5f, true);
		yield return new WaitForSeconds(0.6f);
		this.RpcToggleInputs(1);
		if (newState == GameState.Lobby)
		{
			NetworkManager.singleton.ServerChangeScene("HomeScene");
			yield break;
		}
		if (newState == GameState.Game)
		{
			NetworkManager.singleton.ServerChangeScene("CasinoScene");
			yield break;
		}
		if (newState == GameState.Lose)
		{
			NetworkManager.singleton.ServerChangeScene("LoseStateScene");
			yield break;
		}
		if (newState == GameState.Win)
		{
			NetworkManager.singleton.ServerChangeScene("WinStateScene");
			yield break;
		}
		if (newState == GameState.Test)
		{
			NetworkManager.singleton.ServerChangeScene("GameTest");
			yield break;
		}
		if (newState == GameState.Cutscene)
		{
			NetworkManager.singleton.ServerChangeScene("EndingCutscene_Coinflip_Won");
			yield break;
		}
		if (newState == GameState.Summary)
		{
			NetworkManager.singleton.ServerChangeScene("SummaryScene");
			yield break;
		}
		if (newState == GameState.FollowUs)
		{
			NetworkManager.singleton.ServerChangeScene("FollowUs");
			yield break;
		}
		yield break;
	}

	// Token: 0x06000D1D RID: 3357 RVA: 0x0003743A File Offset: 0x0003563A
	[Server]
	public void ShowDayStats()
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void GameManager::ShowDayStats()' called when server was not active");
			return;
		}
		this.RpcDayStatsFeedback();
		this.ServerLockPlayers();
		this.ServerLockPlayerHeads();
		this.RpcLockInputs();
	}

	// Token: 0x06000D1E RID: 3358 RVA: 0x00037469 File Offset: 0x00035669
	[Server]
	public void ShowGameOverStats()
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void GameManager::ShowGameOverStats()' called when server was not active");
			return;
		}
		this.RpcPlayGameOverFeedback();
		this.ServerLockPlayers();
		this.ServerLockPlayerHeads();
		this.RpcLockInputs();
	}

	// Token: 0x06000D1F RID: 3359 RVA: 0x00037498 File Offset: 0x00035698
	[Server]
	private void ServerLockPlayers()
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void GameManager::ServerLockPlayers()' called when server was not active");
			return;
		}
		foreach (PlayerReferences playerReferences in MonoSingleton<LocalManager>.Instance.players)
		{
			playerReferences.controller.ServerLock(true);
		}
	}

	// Token: 0x06000D20 RID: 3360 RVA: 0x00037508 File Offset: 0x00035708
	[Server]
	private void ServerLockPlayerHeads()
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void GameManager::ServerLockPlayerHeads()' called when server was not active");
			return;
		}
		foreach (PlayerReferences playerReferences in MonoSingleton<LocalManager>.Instance.players)
		{
			playerReferences.controller.ServerLockHead(true);
		}
	}

	// Token: 0x06000D21 RID: 3361 RVA: 0x00037578 File Offset: 0x00035778
	[ClientRpc]
	private void RpcLockInputs()
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		this.SendRPCInternal("System.Void GameManager::RpcLockInputs()", -1945655203, writer, 0, true);
		NetworkWriterPool.Return(writer);
	}

	// Token: 0x06000D22 RID: 3362 RVA: 0x000375A8 File Offset: 0x000357A8
	[Server]
	public void ProgressGame()
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void GameManager::ProgressGame()' called when server was not active");
			return;
		}
		base.StartCoroutine(this.ProgressGameRoutine());
	}

	// Token: 0x06000D23 RID: 3363 RVA: 0x000375CC File Offset: 0x000357CC
	private IEnumerator ProgressGameRoutine()
	{
		if (this._isTransitioning)
		{
			yield break;
		}
		this._isTransitioning = true;
		NetworkSingleton<GameUI>.Instance.ServerSetLoadingScreen(true, 0.5f, true);
		yield return new WaitForSeconds(1f);
		this.RpcToggleInputs(1);
		if (this.state == GameState.Lose || this.state == GameState.Summary)
		{
			NetworkSingleton<SaveManager>.Instance.ResetCurrentSaveToDefaults();
			NetworkSingleton<SaveManager>.Instance.LoadGame();
		}
		if (this.state == GameState.Lobby)
		{
			if (this.successfulQuota >= this._gs.quotas.Length)
			{
				NetworkManager.singleton.ServerChangeScene("WinStateScene");
				yield break;
			}
			NetworkManager.singleton.ServerChangeScene("CasinoScene");
			yield break;
		}
		else if (this.state == GameState.Game)
		{
			this.NetworkdaysLeft = this.daysLeft - 1;
			this.NetworkdaysPassed = this.daysPassed + 1;
			if (this.daysLeft <= 0 && NetworkSingleton<MoneyManager>.Instance.balance < this.currentQuota)
			{
				NetworkManager.singleton.ServerChangeScene("LoseStateScene");
				yield break;
			}
			this.ServerPayDebt();
			NetworkManager.singleton.ServerChangeScene("HomeScene");
			yield break;
		}
		else
		{
			if (this.state == GameState.Lose)
			{
				NetworkManager.singleton.ServerChangeScene("HomeScene");
				yield break;
			}
			if (this.state == GameState.Cutscene)
			{
				NetworkManager.singleton.ServerChangeScene("SummaryScene");
			}
			if (this.state == GameState.Summary)
			{
				NetworkManager.singleton.ServerChangeScene("FollowUs");
			}
			if (this.state == GameState.FollowUs)
			{
				NetworkManager.singleton.ServerChangeScene("HomeScene");
			}
			if (this.state == GameState.Test)
			{
				NetworkManager.singleton.ServerChangeScene("HomeScene");
				yield break;
			}
			yield break;
		}
	}

	// Token: 0x06000D24 RID: 3364 RVA: 0x000375DB File Offset: 0x000357DB
	[Server]
	public void ServerSetCutscene(int cutsceneIndex)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void GameManager::ServerSetCutscene(System.Int32)' called when server was not active");
			return;
		}
		base.StartCoroutine(this.SetCutsceneRoutine(cutsceneIndex));
	}

	// Token: 0x06000D25 RID: 3365 RVA: 0x00037600 File Offset: 0x00035800
	private IEnumerator SetCutsceneRoutine(int cutsceneIndex)
	{
		if (this._isTransitioning)
		{
			yield break;
		}
		this._isTransitioning = true;
		NetworkSingleton<GameUI>.Instance.ServerSetLoadingScreen(true, 0.1f, false);
		yield return new WaitForSeconds(0.2f);
		this.RpcToggleInputs(1);
		switch (cutsceneIndex)
		{
		case 0:
			NetworkManager.singleton.ServerChangeScene("EndingCutscene_Coinflip_Won");
			break;
		case 1:
			NetworkManager.singleton.ServerChangeScene("EndingCutscene_Coinflip_Lost");
			break;
		case 2:
			NetworkManager.singleton.ServerChangeScene("EndingCutscene_Debt_Paid");
			break;
		case 3:
			NetworkManager.singleton.ServerChangeScene("SummaryScene");
			break;
		}
		yield break;
	}

	// Token: 0x06000D26 RID: 3366 RVA: 0x00037618 File Offset: 0x00035818
	[Server]
	public void InitializeScene(string sceneName)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void GameManager::InitializeScene(System.String)' called when server was not active");
			return;
		}
		this._sceneEpoch++;
		this._scenePlayReady.Clear();
		this._expectedScenePlayers = 1;
		if (NetworkSingleton<PlayerSpawnManager>.Instance != null)
		{
			this._expectedScenePlayers = Mathf.Max(1, NetworkSingleton<PlayerSpawnManager>.Instance.RegisteredCount);
		}
		base.StartCoroutine(this.InitializeSceneRoutine(sceneName));
	}

	// Token: 0x06000D27 RID: 3367 RVA: 0x0003768C File Offset: 0x0003588C
	[Server]
	private IEnumerator InitializeSceneRoutine(string sceneName)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Collections.IEnumerator GameManager::InitializeSceneRoutine(System.String)' called when server was not active");
			return null;
		}
		GameManager.<InitializeSceneRoutine>d__59 <InitializeSceneRoutine>d__ = new GameManager.<InitializeSceneRoutine>d__59(0);
		<InitializeSceneRoutine>d__.<>4__this = this;
		<InitializeSceneRoutine>d__.sceneName = sceneName;
		return <InitializeSceneRoutine>d__;
	}

	// Token: 0x06000D28 RID: 3368 RVA: 0x000376CE File Offset: 0x000358CE
	private IEnumerator LobbyInitializeRoutine()
	{
		this.Networkstate = GameState.Lobby;
		SteamMatchmaking.SetLobbyJoinable(MonoSingleton<LobbyManager>.Instance.GetCurrentLobbyID(), true);
		this.PredictNextCasinoGames();
		yield return null;
		NetworkSingleton<SaveManager>.Instance.SaveGame();
		yield return base.StartCoroutine(NetworkSingleton<ItemStampManager>.Instance.InitializeManager());
		this.NetworkcurrentTicketReward = this._gs.GetTicketReward(this.daysPassed);
		NetworkSingleton<ChallengeManager>.Instance.InitializeChallenges(true, false);
		NetworkSingleton<MoneyManager>.Instance.TryChangeTicketBalance((long)this._gs.dailyTicketReward);
		NetworkSingleton<OrganManager>.Instance.ServerApplyAllOrganSettings();
		this.RpcLoadAllPlayerCosmetics();
		NetworkSingleton<UpgradeManager>.Instance.ServerResetAllUpgradesToDefaults();
		NetworkSingleton<ItemManager>.Instance.ServerResetItems();
		NetworkSingleton<ChallengeManager>.Instance.ServerResetAllChallenges();
		NetworkSingleton<ChallengeManager>.Instance.ServerClearChallengesUI();
		NetworkSingleton<NavMeshManager>.Instance.ClearNavMesh();
		NetworkSingleton<GameUI>.Instance.ServerToggleTimer(false);
		NetworkSingleton<GameUI>.Instance.ServerToggleStatusUI(true);
		NetworkSingleton<GameUI>.Instance.ServerToggleMoneyUI(true, true);
		NetworkSingleton<GameUI>.Instance.ServerToggleCrosshair(true);
		NetworkSingleton<GameUI>.Instance.ServerToggleItemInputsUI(true);
		this.RpcSetInLobbyPresence();
		yield return Resources.UnloadUnusedAssets();
		yield break;
	}

	// Token: 0x06000D29 RID: 3369 RVA: 0x000376DD File Offset: 0x000358DD
	private IEnumerator GameInitializeRoutine()
	{
		this.Networkstate = GameState.Game;
		SteamMatchmaking.SetLobbyJoinable(MonoSingleton<LobbyManager>.Instance.GetCurrentLobbyID(), false);
		yield return base.StartCoroutine(NetworkSingleton<StampManager>.Instance.InitializeManager());
		yield return new WaitForSeconds(3f);
		NetworkSingleton<RequestSettingsFromApi>.Instance.ReloadSettings();
		NetworkSingleton<PayoutTracker>.Instance.InitializeStartingPoints();
		NetworkSingleton<GameResultsManager>.Instance.ClearResults();
		this.RpcClearDaySummary();
		NetworkSingleton<ChallengeManager>.Instance.InitializeChallenges(true, true);
		NetworkSingleton<NavMeshManager>.Instance.ClearNavMesh();
		NetworkSingleton<ElevatorManager>.Instance.Initialize();
		NetworkSingleton<MoneyManager>.Instance.SetDayStartBalance();
		NetworkSingleton<OrganManager>.Instance.ServerApplyAllOrganSettings();
		this.RpcLoadAllPlayerCosmetics();
		this.HasDayStarted = false;
		this._isTimeOver = false;
		this.Network_timer = 0f;
		NetworkSingleton<GameUI>.Instance.ServerToggleTimer(true);
		NetworkSingleton<GameUI>.Instance.ServerToggleStatusUI(true);
		NetworkSingleton<GameUI>.Instance.ServerToggleMoneyUI(true, true);
		NetworkSingleton<GameUI>.Instance.ServerToggleCrosshair(true);
		NetworkSingleton<GameUI>.Instance.ServerToggleItemInputsUI(true);
		if (this.daysPassed == 0)
		{
			this.RpcPlayStartFeedback();
		}
		this.RpcSetInGamePresence();
		yield return new WaitForSeconds(1f);
		yield return Resources.UnloadUnusedAssets();
		yield break;
	}

	// Token: 0x06000D2A RID: 3370 RVA: 0x000376EC File Offset: 0x000358EC
	private IEnumerator LoseInitializeRoutine()
	{
		this.Networkstate = GameState.Lose;
		SteamMatchmaking.SetLobbyJoinable(MonoSingleton<LobbyManager>.Instance.GetCurrentLobbyID(), false);
		NetworkSingleton<ChallengeManager>.Instance.InitializeChallenges(false, false);
		NetworkSingleton<OrganManager>.Instance.ServerApplyAllOrganSettings();
		this.RpcLoadAllPlayerCosmetics();
		NetworkSingleton<GameUI>.Instance.ServerToggleTimer(false);
		NetworkSingleton<GameUI>.Instance.ServerToggleStatusUI(false);
		NetworkSingleton<GameUI>.Instance.ServerToggleMoneyUI(false, true);
		NetworkSingleton<GameUI>.Instance.ServerToggleCrosshair(true);
		NetworkSingleton<GameUI>.Instance.ServerToggleItemInputsUI(false);
		NetworkSingleton<NavMeshManager>.Instance.ClearNavMesh();
		yield return new WaitForSeconds(1f);
		yield return Resources.UnloadUnusedAssets();
		this.RpcPlayFailFeedback();
		yield break;
	}

	// Token: 0x06000D2B RID: 3371 RVA: 0x000376FB File Offset: 0x000358FB
	private IEnumerator WinInitializeRoutine()
	{
		this.Networkstate = GameState.Win;
		SteamMatchmaking.SetLobbyJoinable(MonoSingleton<LobbyManager>.Instance.GetCurrentLobbyID(), false);
		NetworkSingleton<ChallengeManager>.Instance.InitializeChallenges(false, false);
		NetworkSingleton<OrganManager>.Instance.ServerApplyAllOrganSettings();
		this.RpcLoadAllPlayerCosmetics();
		NetworkSingleton<NavMeshManager>.Instance.ClearNavMesh();
		NetworkSingleton<ElevatorManager>.Instance.Initialize();
		NetworkSingleton<GameUI>.Instance.ServerToggleTimer(false);
		NetworkSingleton<GameUI>.Instance.ServerToggleStatusUI(false);
		NetworkSingleton<GameUI>.Instance.ServerToggleMoneyUI(true, false);
		NetworkSingleton<GameUI>.Instance.ServerToggleCrosshair(true);
		NetworkSingleton<GameUI>.Instance.ServerToggleItemInputsUI(false);
		yield return new WaitForSeconds(1f);
		yield return Resources.UnloadUnusedAssets();
		yield break;
	}

	// Token: 0x06000D2C RID: 3372 RVA: 0x0003770A File Offset: 0x0003590A
	private IEnumerator CutsceneInitializeRoutine()
	{
		this.Networkstate = GameState.Cutscene;
		NetworkSingleton<OrganManager>.Instance.ServerApplyAllOrganSettings();
		this.RpcLoadAllPlayerCosmetics();
		NetworkSingleton<CreditsRollManager>.Instance.BeginCreditsFromScenePlayers();
		NetworkSingleton<GameUI>.Instance.ServerToggleTimer(false);
		NetworkSingleton<GameUI>.Instance.ServerToggleStatusUI(false);
		NetworkSingleton<GameUI>.Instance.ServerToggleMoneyUI(false, true);
		NetworkSingleton<GameUI>.Instance.ServerToggleCrosshair(false);
		NetworkSingleton<GameUI>.Instance.ServerToggleItemInputsUI(false);
		yield return new WaitForSeconds(1f);
		yield return Resources.UnloadUnusedAssets();
		NetworkSingleton<EndingSequenceManager>.Instance.ServerStartSequence();
		yield break;
	}

	// Token: 0x06000D2D RID: 3373 RVA: 0x00037719 File Offset: 0x00035919
	private IEnumerator SummaryInitializeRoutine()
	{
		this.Networkstate = GameState.Summary;
		NetworkSingleton<GameUI>.Instance.ServerToggleTimer(false);
		NetworkSingleton<GameUI>.Instance.ServerToggleStatusUI(false);
		NetworkSingleton<GameUI>.Instance.ServerToggleMoneyUI(false, true);
		NetworkSingleton<GameUI>.Instance.ServerToggleCrosshair(false);
		NetworkSingleton<GameUI>.Instance.ServerToggleItemInputsUI(false);
		this.ShowGameOverStats();
		yield return null;
		yield return Resources.UnloadUnusedAssets();
		yield break;
	}

	// Token: 0x06000D2E RID: 3374 RVA: 0x00037728 File Offset: 0x00035928
	private IEnumerator FollowUsInitializeRoutine()
	{
		this.Networkstate = GameState.FollowUs;
		NetworkSingleton<GameUI>.Instance.ServerToggleTimer(false);
		NetworkSingleton<GameUI>.Instance.ServerToggleStatusUI(false);
		NetworkSingleton<GameUI>.Instance.ServerToggleMoneyUI(false, true);
		NetworkSingleton<GameUI>.Instance.ServerToggleCrosshair(false);
		NetworkSingleton<GameUI>.Instance.ServerToggleItemInputsUI(false);
		yield return null;
		yield return Resources.UnloadUnusedAssets();
		yield break;
	}

	// Token: 0x06000D2F RID: 3375 RVA: 0x00037737 File Offset: 0x00035937
	private IEnumerator TestInitializeRoutine()
	{
		this.Networkstate = GameState.Test;
		SteamMatchmaking.SetLobbyJoinable(MonoSingleton<LobbyManager>.Instance.GetCurrentLobbyID(), true);
		NetworkSingleton<RequestSettingsFromApi>.Instance.ReloadSettings();
		NetworkSingleton<ChallengeManager>.Instance.InitializeChallenges(true, true);
		NetworkSingleton<NavMeshManager>.Instance.ClearNavMesh();
		NetworkSingleton<OrganManager>.Instance.ServerApplyAllOrganSettings();
		this.RpcLoadAllPlayerCosmetics();
		NetworkSingleton<GameUI>.Instance.ServerToggleTimer(false);
		yield return null;
		yield return Resources.UnloadUnusedAssets();
		yield break;
	}

	// Token: 0x06000D30 RID: 3376 RVA: 0x00037748 File Offset: 0x00035948
	[Server]
	public void ServerOnClientScenePlayReady(NetworkConnectionToClient conn, int epoch)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void GameManager::ServerOnClientScenePlayReady(Mirror.NetworkConnectionToClient,System.Int32)' called when server was not active");
			return;
		}
		if (epoch != this._sceneEpoch)
		{
			return;
		}
		if (conn == null)
		{
			return;
		}
		if (!this._scenePlayReady.Add(conn.connectionId))
		{
			return;
		}
		if (this._scenePlayReady.Count < this._expectedScenePlayers)
		{
			return;
		}
		NetworkSingleton<GameUI>.Instance.ServerSetLoadingScreen(false, 0.5f, true);
		if (this.state == GameState.Lobby)
		{
			this.RpcToggleInputs(2);
			this.ServerLockPlayers();
		}
		else
		{
			GameState gameState = this.state;
			if (gameState != GameState.Cutscene && gameState != GameState.Summary && gameState != GameState.FollowUs)
			{
				this.RpcToggleInputs(0);
			}
		}
		this._isTransitioning = false;
	}

	// Token: 0x06000D31 RID: 3377 RVA: 0x000377E9 File Offset: 0x000359E9
	[Server]
	private void PredictNextCasinoGames()
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void GameManager::PredictNextCasinoGames()' called when server was not active");
			return;
		}
		NextCasinoPredicter.PredictFloorGames(1, 5);
		NextCasinoPredicter.PredictFloorGames(2, 5);
		NextCasinoPredicter.PredictFloorGames(3, 5);
		NextCasinoPredicter.PredictFloorGames(4, 5);
	}

	// Token: 0x06000D32 RID: 3378 RVA: 0x0003781C File Offset: 0x00035A1C
	[ClientRpc]
	public void RpcLoadAllPlayerCosmetics()
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		this.SendRPCInternal("System.Void GameManager::RpcLoadAllPlayerCosmetics()", 1450340149, writer, 0, true);
		NetworkWriterPool.Return(writer);
	}

	// Token: 0x06000D33 RID: 3379 RVA: 0x0003784C File Offset: 0x00035A4C
	[ClientRpc]
	private void RpcSceneInitialized(int epoch)
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		writer.WriteVarInt(epoch);
		this.SendRPCInternal("System.Void GameManager::RpcSceneInitialized(System.Int32)", -1278576032, writer, 0, true);
		NetworkWriterPool.Return(writer);
	}

	// Token: 0x06000D34 RID: 3380 RVA: 0x00037888 File Offset: 0x00035A88
	[ClientRpc]
	private void RpcSetInLobbyPresence()
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		this.SendRPCInternal("System.Void GameManager::RpcSetInLobbyPresence()", -549038361, writer, 0, true);
		NetworkWriterPool.Return(writer);
	}

	// Token: 0x06000D35 RID: 3381 RVA: 0x000378B8 File Offset: 0x00035AB8
	[ClientRpc]
	private void RpcSetInGamePresence()
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		this.SendRPCInternal("System.Void GameManager::RpcSetInGamePresence()", -1924947969, writer, 0, true);
		NetworkWriterPool.Return(writer);
	}

	// Token: 0x06000D36 RID: 3382 RVA: 0x000378E8 File Offset: 0x00035AE8
	[ClientRpc]
	private void RpcToggleInputs(int inputLayerIndex)
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		writer.WriteVarInt(inputLayerIndex);
		this.SendRPCInternal("System.Void GameManager::RpcToggleInputs(System.Int32)", 329602083, writer, 0, true);
		NetworkWriterPool.Return(writer);
	}

	// Token: 0x06000D37 RID: 3383 RVA: 0x00037924 File Offset: 0x00035B24
	[ClientRpc]
	private void RpcDayStatsFeedback()
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		this.SendRPCInternal("System.Void GameManager::RpcDayStatsFeedback()", 1193006993, writer, 0, true);
		NetworkWriterPool.Return(writer);
	}

	// Token: 0x06000D38 RID: 3384 RVA: 0x00037954 File Offset: 0x00035B54
	[ClientRpc]
	private void RpcPlayGameOverFeedback()
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		this.SendRPCInternal("System.Void GameManager::RpcPlayGameOverFeedback()", 1573754606, writer, 0, true);
		NetworkWriterPool.Return(writer);
	}

	// Token: 0x06000D39 RID: 3385 RVA: 0x00037984 File Offset: 0x00035B84
	[ClientRpc]
	private void RpcPlayDayEndFeedback()
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		this.SendRPCInternal("System.Void GameManager::RpcPlayDayEndFeedback()", -1818792019, writer, 0, true);
		NetworkWriterPool.Return(writer);
	}

	// Token: 0x06000D3A RID: 3386 RVA: 0x000379B4 File Offset: 0x00035BB4
	[ClientRpc]
	private void RpcPlayStartFeedback()
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		this.SendRPCInternal("System.Void GameManager::RpcPlayStartFeedback()", 1628449086, writer, 0, true);
		NetworkWriterPool.Return(writer);
	}

	// Token: 0x06000D3B RID: 3387 RVA: 0x000379E4 File Offset: 0x00035BE4
	[ClientRpc]
	private void RpcPlayFailFeedback()
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		this.SendRPCInternal("System.Void GameManager::RpcPlayFailFeedback()", -1505895802, writer, 0, true);
		NetworkWriterPool.Return(writer);
	}

	// Token: 0x06000D3C RID: 3388 RVA: 0x00037A14 File Offset: 0x00035C14
	[ClientRpc]
	private void RpcClearDaySummary()
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		this.SendRPCInternal("System.Void GameManager::RpcClearDaySummary()", 1541698864, writer, 0, true);
		NetworkWriterPool.Return(writer);
	}

	// Token: 0x06000D3D RID: 3389 RVA: 0x00037A44 File Offset: 0x00035C44
	[ClientRpc]
	private void RpcResetSummaryAndGameOverUI()
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		this.SendRPCInternal("System.Void GameManager::RpcResetSummaryAndGameOverUI()", -1553746105, writer, 0, true);
		NetworkWriterPool.Return(writer);
	}

	// Token: 0x06000D3E RID: 3390 RVA: 0x00037A74 File Offset: 0x00035C74
	[ClientRpc]
	private void RpcInitializeCreditsRoll(PlayerCreditsSnapshot[] snapshots)
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		Mirror.GeneratedNetworkCode._Write_PlayerCreditsSnapshot[](writer, snapshots);
		this.SendRPCInternal("System.Void GameManager::RpcInitializeCreditsRoll(PlayerCreditsSnapshot[])", 574056377, writer, 0, true);
		NetworkWriterPool.Return(writer);
	}

	// Token: 0x06000D3F RID: 3391 RVA: 0x00037AAE File Offset: 0x00035CAE
	private IEnumerator InitializeCreditsRollCoroutine(PlayerCreditsSnapshot[] snapshots)
	{
		if (snapshots == null || snapshots.Length == 0)
		{
			Debug.LogError("[GameManager] Received empty or null snapshots array in RPC!");
			yield break;
		}
		CreditsRollManager creditsManager = null;
		int attempts = 0;
		while (creditsManager == null && attempts < 10)
		{
			creditsManager = Object.FindFirstObjectByType<CreditsRollManager>();
			if (creditsManager == null)
			{
				yield return null;
				int num = attempts;
				attempts = num + 1;
			}
		}
		if (creditsManager != null)
		{
			creditsManager.BeginCredits(snapshots);
		}
		else
		{
			Debug.LogError("[GameManager] CreditsRollManager not found after scene load!");
		}
		yield break;
	}

	// Token: 0x06000D40 RID: 3392 RVA: 0x00037AC0 File Offset: 0x00035CC0
	[ClientRpc]
	private void RpcInitializeCreditsTextScroller(PlayerCreditsSnapshot[] snapshots)
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		Mirror.GeneratedNetworkCode._Write_PlayerCreditsSnapshot[](writer, snapshots);
		this.SendRPCInternal("System.Void GameManager::RpcInitializeCreditsTextScroller(PlayerCreditsSnapshot[])", 319010527, writer, 0, true);
		NetworkWriterPool.Return(writer);
	}

	// Token: 0x06000D41 RID: 3393 RVA: 0x00037AFA File Offset: 0x00035CFA
	private IEnumerator InitializeCreditsTextScrollerCoroutine(PlayerCreditsSnapshot[] snapshots)
	{
		if (snapshots == null || snapshots.Length == 0)
		{
			Debug.LogError("[GameManager] Received empty or null snapshots array in RPC!");
			yield break;
		}
		CreditsRollManager creditsManager = null;
		int attempts = 0;
		while (creditsManager == null && attempts < 10)
		{
			creditsManager = Object.FindFirstObjectByType<CreditsRollManager>();
			if (creditsManager == null)
			{
				yield return null;
				int num = attempts;
				attempts = num + 1;
			}
		}
		if (creditsManager != null)
		{
			creditsManager.BeginCredits(snapshots);
		}
		else
		{
			Debug.LogWarning("[GameManager] CreditsRollManager not found after scene load!");
		}
		yield break;
	}

	// Token: 0x06000D42 RID: 3394 RVA: 0x00037B0C File Offset: 0x00035D0C
	public GameManager()
	{
		this._Mirror_SyncVarHookDelegate_daysLeft = new Action<int, int>(this.OnDaysChanged);
		this._Mirror_SyncVarHookDelegate_daysPassed = new Action<int, int>(this.OnDaysPassedChanged);
		this._Mirror_SyncVarHookDelegate_currentQuota = new Action<long, long>(this.OnQuotaChanged);
		this._Mirror_SyncVarHookDelegate__timer = new Action<float, float>(this.OnTimerChanged);
	}

	// Token: 0x06000D43 RID: 3395 RVA: 0x00002321 File Offset: 0x00000521
	public override bool Weaved()
	{
		return true;
	}

	// Token: 0x17000115 RID: 277
	// (get) Token: 0x06000D44 RID: 3396 RVA: 0x00037B74 File Offset: 0x00035D74
	// (set) Token: 0x06000D45 RID: 3397 RVA: 0x00037B87 File Offset: 0x00035D87
	public int NetworkdaysLeft
	{
		get
		{
			return this.daysLeft;
		}
		[param: In]
		set
		{
			base.GeneratedSyncVarSetter<int>(value, ref this.daysLeft, 1UL, this._Mirror_SyncVarHookDelegate_daysLeft);
		}
	}

	// Token: 0x17000116 RID: 278
	// (get) Token: 0x06000D46 RID: 3398 RVA: 0x00037BA8 File Offset: 0x00035DA8
	// (set) Token: 0x06000D47 RID: 3399 RVA: 0x00037BBB File Offset: 0x00035DBB
	public int NetworkdaysPassed
	{
		get
		{
			return this.daysPassed;
		}
		[param: In]
		set
		{
			base.GeneratedSyncVarSetter<int>(value, ref this.daysPassed, 2UL, this._Mirror_SyncVarHookDelegate_daysPassed);
		}
	}

	// Token: 0x17000117 RID: 279
	// (get) Token: 0x06000D48 RID: 3400 RVA: 0x00037BDC File Offset: 0x00035DDC
	// (set) Token: 0x06000D49 RID: 3401 RVA: 0x00037BEF File Offset: 0x00035DEF
	public int NetworksuccessfulQuota
	{
		get
		{
			return this.successfulQuota;
		}
		[param: In]
		set
		{
			base.GeneratedSyncVarSetter<int>(value, ref this.successfulQuota, 4UL, null);
		}
	}

	// Token: 0x17000118 RID: 280
	// (get) Token: 0x06000D4A RID: 3402 RVA: 0x00037C0C File Offset: 0x00035E0C
	// (set) Token: 0x06000D4B RID: 3403 RVA: 0x00037C1F File Offset: 0x00035E1F
	public long NetworkcurrentQuota
	{
		get
		{
			return this.currentQuota;
		}
		[param: In]
		set
		{
			base.GeneratedSyncVarSetter<long>(value, ref this.currentQuota, 8UL, this._Mirror_SyncVarHookDelegate_currentQuota);
		}
	}

	// Token: 0x17000119 RID: 281
	// (get) Token: 0x06000D4C RID: 3404 RVA: 0x00037C40 File Offset: 0x00035E40
	// (set) Token: 0x06000D4D RID: 3405 RVA: 0x00037C53 File Offset: 0x00035E53
	public int NetworkcurrentFloor
	{
		get
		{
			return this.currentFloor;
		}
		[param: In]
		set
		{
			base.GeneratedSyncVarSetter<int>(value, ref this.currentFloor, 16UL, null);
		}
	}

	// Token: 0x1700011A RID: 282
	// (get) Token: 0x06000D4E RID: 3406 RVA: 0x00037C70 File Offset: 0x00035E70
	// (set) Token: 0x06000D4F RID: 3407 RVA: 0x00037C83 File Offset: 0x00035E83
	public long NetworkrequiredQuotaToNextFloor
	{
		get
		{
			return this.requiredQuotaToNextFloor;
		}
		[param: In]
		set
		{
			base.GeneratedSyncVarSetter<long>(value, ref this.requiredQuotaToNextFloor, 32UL, null);
		}
	}

	// Token: 0x1700011B RID: 283
	// (get) Token: 0x06000D50 RID: 3408 RVA: 0x00037CA0 File Offset: 0x00035EA0
	// (set) Token: 0x06000D51 RID: 3409 RVA: 0x00037CB3 File Offset: 0x00035EB3
	public int NetworkcurrentTicketReward
	{
		get
		{
			return this.currentTicketReward;
		}
		[param: In]
		set
		{
			base.GeneratedSyncVarSetter<int>(value, ref this.currentTicketReward, 64UL, null);
		}
	}

	// Token: 0x1700011C RID: 284
	// (get) Token: 0x06000D52 RID: 3410 RVA: 0x00037CD0 File Offset: 0x00035ED0
	// (set) Token: 0x06000D53 RID: 3411 RVA: 0x00037CE3 File Offset: 0x00035EE3
	public bool NetworkisDebtPaid
	{
		get
		{
			return this.isDebtPaid;
		}
		[param: In]
		set
		{
			base.GeneratedSyncVarSetter<bool>(value, ref this.isDebtPaid, 128UL, null);
		}
	}

	// Token: 0x1700011D RID: 285
	// (get) Token: 0x06000D54 RID: 3412 RVA: 0x00037D00 File Offset: 0x00035F00
	// (set) Token: 0x06000D55 RID: 3413 RVA: 0x00037D13 File Offset: 0x00035F13
	public GameState Networkstate
	{
		get
		{
			return this.state;
		}
		[param: In]
		set
		{
			base.GeneratedSyncVarSetter<GameState>(value, ref this.state, 256UL, null);
		}
	}

	// Token: 0x1700011E RID: 286
	// (get) Token: 0x06000D56 RID: 3414 RVA: 0x00037D30 File Offset: 0x00035F30
	// (set) Token: 0x06000D57 RID: 3415 RVA: 0x00037D43 File Offset: 0x00035F43
	public float Network_timer
	{
		get
		{
			return this._timer;
		}
		[param: In]
		set
		{
			base.GeneratedSyncVarSetter<float>(value, ref this._timer, 512UL, this._Mirror_SyncVarHookDelegate__timer);
		}
	}

	// Token: 0x06000D58 RID: 3416 RVA: 0x00037D62 File Offset: 0x00035F62
	protected void UserCode_RpcLockInputs()
	{
		InputEvents.ActiveLayer = InputLayer.Cutscene;
	}

	// Token: 0x06000D59 RID: 3417 RVA: 0x00037D6A File Offset: 0x00035F6A
	protected static void InvokeUserCode_RpcLockInputs(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcLockInputs called on server.");
			return;
		}
		((GameManager)obj).UserCode_RpcLockInputs();
	}

	// Token: 0x06000D5A RID: 3418 RVA: 0x00037D90 File Offset: 0x00035F90
	protected void UserCode_RpcLoadAllPlayerCosmetics()
	{
		foreach (PlayerCustomization playerCustomization in Object.FindObjectsByType<PlayerCustomization>(FindObjectsSortMode.None))
		{
			if (playerCustomization.isLocalPlayer)
			{
				playerCustomization.LoadCosmetics();
				playerCustomization.LoadSavedPlayerColor();
			}
		}
	}

	// Token: 0x06000D5B RID: 3419 RVA: 0x00037DCA File Offset: 0x00035FCA
	protected static void InvokeUserCode_RpcLoadAllPlayerCosmetics(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcLoadAllPlayerCosmetics called on server.");
			return;
		}
		((GameManager)obj).UserCode_RpcLoadAllPlayerCosmetics();
	}

	// Token: 0x06000D5C RID: 3420 RVA: 0x00037DF0 File Offset: 0x00035FF0
	protected void UserCode_RpcSceneInitialized__Int32(int epoch)
	{
		if (!NetworkClient.isConnected)
		{
			return;
		}
		NetworkClient.Send<ClientScenePlayReadyMessage>(new ClientScenePlayReadyMessage
		{
			epoch = epoch
		}, 0);
	}

	// Token: 0x06000D5D RID: 3421 RVA: 0x00037E1C File Offset: 0x0003601C
	protected static void InvokeUserCode_RpcSceneInitialized__Int32(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcSceneInitialized called on server.");
			return;
		}
		((GameManager)obj).UserCode_RpcSceneInitialized__Int32(reader.ReadVarInt());
	}

	// Token: 0x06000D5E RID: 3422 RVA: 0x00037E45 File Offset: 0x00036045
	protected void UserCode_RpcSetInLobbyPresence()
	{
		MonoSingleton<SteamRichPresenceManager>.Instance.SetInHomePresence();
		if (MonoSingleton<DiscordRichPresenceManager>.Instance != null)
		{
			MonoSingleton<DiscordRichPresenceManager>.Instance.SetInHomePresence();
		}
	}

	// Token: 0x06000D5F RID: 3423 RVA: 0x00037E68 File Offset: 0x00036068
	protected static void InvokeUserCode_RpcSetInLobbyPresence(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcSetInLobbyPresence called on server.");
			return;
		}
		((GameManager)obj).UserCode_RpcSetInLobbyPresence();
	}

	// Token: 0x06000D60 RID: 3424 RVA: 0x00037E8B File Offset: 0x0003608B
	protected void UserCode_RpcSetInGamePresence()
	{
		MonoSingleton<SteamRichPresenceManager>.Instance.SetInGamePresence();
		if (MonoSingleton<DiscordRichPresenceManager>.Instance != null)
		{
			MonoSingleton<DiscordRichPresenceManager>.Instance.SetInGamePresence();
		}
	}

	// Token: 0x06000D61 RID: 3425 RVA: 0x00037EAE File Offset: 0x000360AE
	protected static void InvokeUserCode_RpcSetInGamePresence(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcSetInGamePresence called on server.");
			return;
		}
		((GameManager)obj).UserCode_RpcSetInGamePresence();
	}

	// Token: 0x06000D62 RID: 3426 RVA: 0x00037ED1 File Offset: 0x000360D1
	protected void UserCode_RpcToggleInputs__Int32(int inputLayerIndex)
	{
		switch (inputLayerIndex)
		{
		case 0:
			InputEvents.ActiveLayer = InputLayer.Default;
			return;
		case 1:
			InputEvents.ActiveLayer = InputLayer.Cutscene;
			return;
		case 2:
			InputEvents.ActiveLayer = InputLayer.SpawnBox;
			return;
		default:
			return;
		}
	}

	// Token: 0x06000D63 RID: 3427 RVA: 0x00037EFA File Offset: 0x000360FA
	protected static void InvokeUserCode_RpcToggleInputs__Int32(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcToggleInputs called on server.");
			return;
		}
		((GameManager)obj).UserCode_RpcToggleInputs__Int32(reader.ReadVarInt());
	}

	// Token: 0x06000D64 RID: 3428 RVA: 0x00037F23 File Offset: 0x00036123
	protected void UserCode_RpcDayStatsFeedback()
	{
		this.daySummaryUI.Show();
	}

	// Token: 0x06000D65 RID: 3429 RVA: 0x00037F30 File Offset: 0x00036130
	protected static void InvokeUserCode_RpcDayStatsFeedback(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcDayStatsFeedback called on server.");
			return;
		}
		((GameManager)obj).UserCode_RpcDayStatsFeedback();
	}

	// Token: 0x06000D66 RID: 3430 RVA: 0x00037F53 File Offset: 0x00036153
	protected void UserCode_RpcPlayGameOverFeedback()
	{
		this.gameOverUI.Show();
	}

	// Token: 0x06000D67 RID: 3431 RVA: 0x00037F60 File Offset: 0x00036160
	protected static void InvokeUserCode_RpcPlayGameOverFeedback(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcPlayGameOverFeedback called on server.");
			return;
		}
		((GameManager)obj).UserCode_RpcPlayGameOverFeedback();
	}

	// Token: 0x06000D68 RID: 3432 RVA: 0x00037F83 File Offset: 0x00036183
	protected void UserCode_RpcPlayDayEndFeedback()
	{
		this.dayEndFb.PlayFeedbacks();
	}

	// Token: 0x06000D69 RID: 3433 RVA: 0x00037F90 File Offset: 0x00036190
	protected static void InvokeUserCode_RpcPlayDayEndFeedback(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcPlayDayEndFeedback called on server.");
			return;
		}
		((GameManager)obj).UserCode_RpcPlayDayEndFeedback();
	}

	// Token: 0x06000D6A RID: 3434 RVA: 0x00037FB3 File Offset: 0x000361B3
	protected void UserCode_RpcPlayStartFeedback()
	{
		this.startFb.PlayFeedbacks();
	}

	// Token: 0x06000D6B RID: 3435 RVA: 0x00037FC0 File Offset: 0x000361C0
	protected static void InvokeUserCode_RpcPlayStartFeedback(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcPlayStartFeedback called on server.");
			return;
		}
		((GameManager)obj).UserCode_RpcPlayStartFeedback();
	}

	// Token: 0x06000D6C RID: 3436 RVA: 0x00037FE3 File Offset: 0x000361E3
	protected void UserCode_RpcPlayFailFeedback()
	{
		this.failFb.PlayFeedbacks();
	}

	// Token: 0x06000D6D RID: 3437 RVA: 0x00037FF0 File Offset: 0x000361F0
	protected static void InvokeUserCode_RpcPlayFailFeedback(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcPlayFailFeedback called on server.");
			return;
		}
		((GameManager)obj).UserCode_RpcPlayFailFeedback();
	}

	// Token: 0x06000D6E RID: 3438 RVA: 0x00038013 File Offset: 0x00036213
	protected void UserCode_RpcClearDaySummary()
	{
		if (MonoSingleton<DaySummaryRuntime>.Instance != null)
		{
			MonoSingleton<DaySummaryRuntime>.Instance.Clear();
		}
	}

	// Token: 0x06000D6F RID: 3439 RVA: 0x0003802C File Offset: 0x0003622C
	protected static void InvokeUserCode_RpcClearDaySummary(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcClearDaySummary called on server.");
			return;
		}
		((GameManager)obj).UserCode_RpcClearDaySummary();
	}

	// Token: 0x06000D70 RID: 3440 RVA: 0x0003804F File Offset: 0x0003624F
	protected void UserCode_RpcResetSummaryAndGameOverUI()
	{
		this.daySummaryUI.Reset();
		this.gameOverUI.Reset();
	}

	// Token: 0x06000D71 RID: 3441 RVA: 0x00038067 File Offset: 0x00036267
	protected static void InvokeUserCode_RpcResetSummaryAndGameOverUI(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcResetSummaryAndGameOverUI called on server.");
			return;
		}
		((GameManager)obj).UserCode_RpcResetSummaryAndGameOverUI();
	}

	// Token: 0x06000D72 RID: 3442 RVA: 0x0003808A File Offset: 0x0003628A
	protected void UserCode_RpcInitializeCreditsRoll__PlayerCreditsSnapshot[](PlayerCreditsSnapshot[] snapshots)
	{
		Debug.Log(string.Format("[GameManager] RpcInitializeCreditsRoll received {0} snapshots", (snapshots != null) ? snapshots.Length : 0));
		base.StartCoroutine(this.InitializeCreditsRollCoroutine(snapshots));
	}

	// Token: 0x06000D73 RID: 3443 RVA: 0x000380B7 File Offset: 0x000362B7
	protected static void InvokeUserCode_RpcInitializeCreditsRoll__PlayerCreditsSnapshot[](NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcInitializeCreditsRoll called on server.");
			return;
		}
		((GameManager)obj).UserCode_RpcInitializeCreditsRoll__PlayerCreditsSnapshot[](Mirror.GeneratedNetworkCode._Read_PlayerCreditsSnapshot[](reader));
	}

	// Token: 0x06000D74 RID: 3444 RVA: 0x000380E0 File Offset: 0x000362E0
	protected void UserCode_RpcInitializeCreditsTextScroller__PlayerCreditsSnapshot[](PlayerCreditsSnapshot[] snapshots)
	{
		Debug.Log(string.Format("[GameManager] RpcInitializeCreditsTextScroller received {0} snapshots", (snapshots != null) ? snapshots.Length : 0));
		base.StartCoroutine(this.InitializeCreditsRollCoroutine(snapshots));
	}

	// Token: 0x06000D75 RID: 3445 RVA: 0x0003810D File Offset: 0x0003630D
	protected static void InvokeUserCode_RpcInitializeCreditsTextScroller__PlayerCreditsSnapshot[](NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcInitializeCreditsTextScroller called on server.");
			return;
		}
		((GameManager)obj).UserCode_RpcInitializeCreditsTextScroller__PlayerCreditsSnapshot[](Mirror.GeneratedNetworkCode._Read_PlayerCreditsSnapshot[](reader));
	}

	// Token: 0x06000D76 RID: 3446 RVA: 0x00038138 File Offset: 0x00036338
	static GameManager()
	{
		RemoteProcedureCalls.RegisterRpc(typeof(GameManager), "System.Void GameManager::RpcLockInputs()", new RemoteCallDelegate(GameManager.InvokeUserCode_RpcLockInputs));
		RemoteProcedureCalls.RegisterRpc(typeof(GameManager), "System.Void GameManager::RpcLoadAllPlayerCosmetics()", new RemoteCallDelegate(GameManager.InvokeUserCode_RpcLoadAllPlayerCosmetics));
		RemoteProcedureCalls.RegisterRpc(typeof(GameManager), "System.Void GameManager::RpcSceneInitialized(System.Int32)", new RemoteCallDelegate(GameManager.InvokeUserCode_RpcSceneInitialized__Int32));
		RemoteProcedureCalls.RegisterRpc(typeof(GameManager), "System.Void GameManager::RpcSetInLobbyPresence()", new RemoteCallDelegate(GameManager.InvokeUserCode_RpcSetInLobbyPresence));
		RemoteProcedureCalls.RegisterRpc(typeof(GameManager), "System.Void GameManager::RpcSetInGamePresence()", new RemoteCallDelegate(GameManager.InvokeUserCode_RpcSetInGamePresence));
		RemoteProcedureCalls.RegisterRpc(typeof(GameManager), "System.Void GameManager::RpcToggleInputs(System.Int32)", new RemoteCallDelegate(GameManager.InvokeUserCode_RpcToggleInputs__Int32));
		RemoteProcedureCalls.RegisterRpc(typeof(GameManager), "System.Void GameManager::RpcDayStatsFeedback()", new RemoteCallDelegate(GameManager.InvokeUserCode_RpcDayStatsFeedback));
		RemoteProcedureCalls.RegisterRpc(typeof(GameManager), "System.Void GameManager::RpcPlayGameOverFeedback()", new RemoteCallDelegate(GameManager.InvokeUserCode_RpcPlayGameOverFeedback));
		RemoteProcedureCalls.RegisterRpc(typeof(GameManager), "System.Void GameManager::RpcPlayDayEndFeedback()", new RemoteCallDelegate(GameManager.InvokeUserCode_RpcPlayDayEndFeedback));
		RemoteProcedureCalls.RegisterRpc(typeof(GameManager), "System.Void GameManager::RpcPlayStartFeedback()", new RemoteCallDelegate(GameManager.InvokeUserCode_RpcPlayStartFeedback));
		RemoteProcedureCalls.RegisterRpc(typeof(GameManager), "System.Void GameManager::RpcPlayFailFeedback()", new RemoteCallDelegate(GameManager.InvokeUserCode_RpcPlayFailFeedback));
		RemoteProcedureCalls.RegisterRpc(typeof(GameManager), "System.Void GameManager::RpcClearDaySummary()", new RemoteCallDelegate(GameManager.InvokeUserCode_RpcClearDaySummary));
		RemoteProcedureCalls.RegisterRpc(typeof(GameManager), "System.Void GameManager::RpcResetSummaryAndGameOverUI()", new RemoteCallDelegate(GameManager.InvokeUserCode_RpcResetSummaryAndGameOverUI));
		RemoteProcedureCalls.RegisterRpc(typeof(GameManager), "System.Void GameManager::RpcInitializeCreditsRoll(PlayerCreditsSnapshot[])", new RemoteCallDelegate(GameManager.InvokeUserCode_RpcInitializeCreditsRoll__PlayerCreditsSnapshot[]));
		RemoteProcedureCalls.RegisterRpc(typeof(GameManager), "System.Void GameManager::RpcInitializeCreditsTextScroller(PlayerCreditsSnapshot[])", new RemoteCallDelegate(GameManager.InvokeUserCode_RpcInitializeCreditsTextScroller__PlayerCreditsSnapshot[]));
	}

	// Token: 0x06000D77 RID: 3447 RVA: 0x00038328 File Offset: 0x00036528
	public override void SerializeSyncVars(NetworkWriter writer, bool forceAll)
	{
		base.SerializeSyncVars(writer, forceAll);
		if (forceAll)
		{
			writer.WriteVarInt(this.daysLeft);
			writer.WriteVarInt(this.daysPassed);
			writer.WriteVarInt(this.successfulQuota);
			writer.WriteVarLong(this.currentQuota);
			writer.WriteVarInt(this.currentFloor);
			writer.WriteVarLong(this.requiredQuotaToNextFloor);
			writer.WriteVarInt(this.currentTicketReward);
			writer.WriteBool(this.isDebtPaid);
			Mirror.GeneratedNetworkCode._Write_GameState(writer, this.state);
			writer.WriteFloat(this._timer);
			return;
		}
		writer.WriteVarULong(this.syncVarDirtyBits);
		if ((this.syncVarDirtyBits & 1UL) != 0UL)
		{
			writer.WriteVarInt(this.daysLeft);
		}
		if ((this.syncVarDirtyBits & 2UL) != 0UL)
		{
			writer.WriteVarInt(this.daysPassed);
		}
		if ((this.syncVarDirtyBits & 4UL) != 0UL)
		{
			writer.WriteVarInt(this.successfulQuota);
		}
		if ((this.syncVarDirtyBits & 8UL) != 0UL)
		{
			writer.WriteVarLong(this.currentQuota);
		}
		if ((this.syncVarDirtyBits & 16UL) != 0UL)
		{
			writer.WriteVarInt(this.currentFloor);
		}
		if ((this.syncVarDirtyBits & 32UL) != 0UL)
		{
			writer.WriteVarLong(this.requiredQuotaToNextFloor);
		}
		if ((this.syncVarDirtyBits & 64UL) != 0UL)
		{
			writer.WriteVarInt(this.currentTicketReward);
		}
		if ((this.syncVarDirtyBits & 128UL) != 0UL)
		{
			writer.WriteBool(this.isDebtPaid);
		}
		if ((this.syncVarDirtyBits & 256UL) != 0UL)
		{
			Mirror.GeneratedNetworkCode._Write_GameState(writer, this.state);
		}
		if ((this.syncVarDirtyBits & 512UL) != 0UL)
		{
			writer.WriteFloat(this._timer);
		}
	}

	// Token: 0x06000D78 RID: 3448 RVA: 0x00038520 File Offset: 0x00036720
	public override void DeserializeSyncVars(NetworkReader reader, bool initialState)
	{
		base.DeserializeSyncVars(reader, initialState);
		if (initialState)
		{
			base.GeneratedSyncVarDeserialize<int>(ref this.daysLeft, this._Mirror_SyncVarHookDelegate_daysLeft, reader.ReadVarInt());
			base.GeneratedSyncVarDeserialize<int>(ref this.daysPassed, this._Mirror_SyncVarHookDelegate_daysPassed, reader.ReadVarInt());
			base.GeneratedSyncVarDeserialize<int>(ref this.successfulQuota, null, reader.ReadVarInt());
			base.GeneratedSyncVarDeserialize<long>(ref this.currentQuota, this._Mirror_SyncVarHookDelegate_currentQuota, reader.ReadVarLong());
			base.GeneratedSyncVarDeserialize<int>(ref this.currentFloor, null, reader.ReadVarInt());
			base.GeneratedSyncVarDeserialize<long>(ref this.requiredQuotaToNextFloor, null, reader.ReadVarLong());
			base.GeneratedSyncVarDeserialize<int>(ref this.currentTicketReward, null, reader.ReadVarInt());
			base.GeneratedSyncVarDeserialize<bool>(ref this.isDebtPaid, null, reader.ReadBool());
			base.GeneratedSyncVarDeserialize<GameState>(ref this.state, null, Mirror.GeneratedNetworkCode._Read_GameState(reader));
			base.GeneratedSyncVarDeserialize<float>(ref this._timer, this._Mirror_SyncVarHookDelegate__timer, reader.ReadFloat());
			return;
		}
		long num = (long)reader.ReadVarULong();
		if ((num & 1L) != 0L)
		{
			base.GeneratedSyncVarDeserialize<int>(ref this.daysLeft, this._Mirror_SyncVarHookDelegate_daysLeft, reader.ReadVarInt());
		}
		if ((num & 2L) != 0L)
		{
			base.GeneratedSyncVarDeserialize<int>(ref this.daysPassed, this._Mirror_SyncVarHookDelegate_daysPassed, reader.ReadVarInt());
		}
		if ((num & 4L) != 0L)
		{
			base.GeneratedSyncVarDeserialize<int>(ref this.successfulQuota, null, reader.ReadVarInt());
		}
		if ((num & 8L) != 0L)
		{
			base.GeneratedSyncVarDeserialize<long>(ref this.currentQuota, this._Mirror_SyncVarHookDelegate_currentQuota, reader.ReadVarLong());
		}
		if ((num & 16L) != 0L)
		{
			base.GeneratedSyncVarDeserialize<int>(ref this.currentFloor, null, reader.ReadVarInt());
		}
		if ((num & 32L) != 0L)
		{
			base.GeneratedSyncVarDeserialize<long>(ref this.requiredQuotaToNextFloor, null, reader.ReadVarLong());
		}
		if ((num & 64L) != 0L)
		{
			base.GeneratedSyncVarDeserialize<int>(ref this.currentTicketReward, null, reader.ReadVarInt());
		}
		if ((num & 128L) != 0L)
		{
			base.GeneratedSyncVarDeserialize<bool>(ref this.isDebtPaid, null, reader.ReadBool());
		}
		if ((num & 256L) != 0L)
		{
			base.GeneratedSyncVarDeserialize<GameState>(ref this.state, null, Mirror.GeneratedNetworkCode._Read_GameState(reader));
		}
		if ((num & 512L) != 0L)
		{
			base.GeneratedSyncVarDeserialize<float>(ref this._timer, this._Mirror_SyncVarHookDelegate__timer, reader.ReadFloat());
		}
	}

	// Token: 0x04000879 RID: 2169
	[Header("Feedbacks")]
	[SerializeField]
	private MMF_Player startFb;

	// Token: 0x0400087A RID: 2170
	[SerializeField]
	private MMF_Player failFb;

	// Token: 0x0400087B RID: 2171
	[SerializeField]
	private MMF_Player dayEndFb;

	// Token: 0x0400087C RID: 2172
	[SerializeField]
	private DaySummaryUI daySummaryUI;

	// Token: 0x0400087D RID: 2173
	[SerializeField]
	private GameOverUI gameOverUI;

	// Token: 0x0400087E RID: 2174
	[Header("Debug")]
	[SyncVar(hook = "OnDaysChanged")]
	[ReadOnly]
	public int daysLeft;

	// Token: 0x0400087F RID: 2175
	[SyncVar(hook = "OnDaysPassedChanged")]
	[ReadOnly]
	public int daysPassed;

	// Token: 0x04000880 RID: 2176
	[SyncVar]
	[ReadOnly]
	public int successfulQuota;

	// Token: 0x04000881 RID: 2177
	[SyncVar(hook = "OnQuotaChanged")]
	[ReadOnly]
	public long currentQuota;

	// Token: 0x04000882 RID: 2178
	[SyncVar]
	[ReadOnly]
	public int currentFloor;

	// Token: 0x04000883 RID: 2179
	[SyncVar]
	[ReadOnly]
	public long requiredQuotaToNextFloor;

	// Token: 0x04000884 RID: 2180
	[SyncVar]
	[ReadOnly]
	public int currentTicketReward;

	// Token: 0x04000885 RID: 2181
	[SyncVar]
	[ReadOnly]
	public bool isDebtPaid;

	// Token: 0x04000886 RID: 2182
	[SyncVar]
	[ReadOnly]
	public GameState state;

	// Token: 0x04000887 RID: 2183
	[SyncVar(hook = "OnTimerChanged")]
	private float _timer;

	// Token: 0x04000888 RID: 2184
	private GameSettings _gs;

	// Token: 0x04000889 RID: 2185
	public GameEvent onQuotaAchieved;

	// Token: 0x0400088A RID: 2186
	public GameEvent onFloorProgressed;

	// Token: 0x0400088B RID: 2187
	public GameEvent onDayEnded;

	// Token: 0x0400088C RID: 2188
	public GameEvent sceneInitCompleted;

	// Token: 0x0400088F RID: 2191
	private bool _isTimeOver;

	// Token: 0x04000890 RID: 2192
	private bool _isTransitioning;

	// Token: 0x04000891 RID: 2193
	private int _sceneEpoch;

	// Token: 0x04000892 RID: 2194
	private readonly HashSet<int> _scenePlayReady = new HashSet<int>();

	// Token: 0x04000893 RID: 2195
	private int _expectedScenePlayers;

	// Token: 0x04000894 RID: 2196
	public Action<int, int> _Mirror_SyncVarHookDelegate_daysLeft;

	// Token: 0x04000895 RID: 2197
	public Action<int, int> _Mirror_SyncVarHookDelegate_daysPassed;

	// Token: 0x04000896 RID: 2198
	public Action<long, long> _Mirror_SyncVarHookDelegate_currentQuota;

	// Token: 0x04000897 RID: 2199
	public Action<float, float> _Mirror_SyncVarHookDelegate__timer;
}
