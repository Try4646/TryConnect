using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Extensions;
using Mirror;
using Mirror.RemoteCalls;
using UnityEngine;

// Token: 0x02000128 RID: 296
public class ChallengeManager : NetworkSingleton<ChallengeManager>
{
	// Token: 0x14000009 RID: 9
	// (add) Token: 0x06000BE5 RID: 3045 RVA: 0x00030B68 File Offset: 0x0002ED68
	// (remove) Token: 0x06000BE6 RID: 3046 RVA: 0x00030BA0 File Offset: 0x0002EDA0
	public event Action<Challenge> OnChallengeCompleted;

	// Token: 0x1400000A RID: 10
	// (add) Token: 0x06000BE7 RID: 3047 RVA: 0x00030BD8 File Offset: 0x0002EDD8
	// (remove) Token: 0x06000BE8 RID: 3048 RVA: 0x00030C10 File Offset: 0x0002EE10
	public event Action<Challenge> OnChallengeProgressUpdated;

	// Token: 0x06000BE9 RID: 3049 RVA: 0x00030C45 File Offset: 0x0002EE45
	protected override void OnAwake()
	{
		base.OnAwake();
		this._challengeSettings = Resources.Load<ChallengeSettings>("ChallengeSettings");
		this.LoadChallengesFromSettings();
	}

	// Token: 0x06000BEA RID: 3050 RVA: 0x00030C63 File Offset: 0x0002EE63
	public override void OnStartServer()
	{
		base.OnStartServer();
		this.SubscribeToGameResults();
	}

	// Token: 0x06000BEB RID: 3051 RVA: 0x00030C71 File Offset: 0x0002EE71
	public override void OnStartClient()
	{
		base.OnStartClient();
		if (base.isServer)
		{
			this.UpdateActiveChallengesDisplay();
			return;
		}
		this.CmdRequestChallengesSync();
	}

	// Token: 0x06000BEC RID: 3052 RVA: 0x00030C8E File Offset: 0x0002EE8E
	private void LoadChallengesFromSettings()
	{
		this.allChallenges = new List<Challenge>(this._challengeSettings.challenges);
		Debug.Log(string.Format("[ChallengeManager] Loaded {0} challenges from ChallengeSettings", this.allChallenges.Count));
	}

	// Token: 0x06000BED RID: 3053 RVA: 0x00030CC5 File Offset: 0x0002EEC5
	private void OnEnable()
	{
		this.SubscribeToGameResults();
	}

	// Token: 0x06000BEE RID: 3054 RVA: 0x00030CCD File Offset: 0x0002EECD
	private void SubscribeToGameResults()
	{
		if (NetworkSingleton<GameResultsManager>.Instance != null)
		{
			NetworkSingleton<GameResultsManager>.Instance.OnResultRegistered -= this.OnGameResultRegistered;
			NetworkSingleton<GameResultsManager>.Instance.OnResultRegistered += this.OnGameResultRegistered;
		}
	}

	// Token: 0x06000BEF RID: 3055 RVA: 0x00030D08 File Offset: 0x0002EF08
	private void OnDisable()
	{
		if (NetworkSingleton<GameResultsManager>.Instance != null)
		{
			NetworkSingleton<GameResultsManager>.Instance.OnResultRegistered -= this.OnGameResultRegistered;
		}
		if (this._challengeSettings != null)
		{
			this._challengeSettings.activeChallenges.Clear();
		}
	}

	// Token: 0x06000BF0 RID: 3056 RVA: 0x00030D56 File Offset: 0x0002EF56
	public List<ChallengeProgress> GetActiveChallenges()
	{
		return this.activeChallenges.Values.ToList<ChallengeProgress>();
	}

	// Token: 0x06000BF1 RID: 3057 RVA: 0x00030D68 File Offset: 0x0002EF68
	public ChallengeProgress GetChallengeProgress(Challenge challenge)
	{
		if (challenge == null)
		{
			return null;
		}
		if (!this.activeChallenges.ContainsKey(challenge))
		{
			return null;
		}
		return this.activeChallenges[challenge];
	}

	// Token: 0x06000BF2 RID: 3058 RVA: 0x00030D91 File Offset: 0x0002EF91
	public bool IsChallengeActive(Challenge challenge)
	{
		return challenge != null && this.activeChallenges.ContainsKey(challenge);
	}

	// Token: 0x06000BF3 RID: 3059 RVA: 0x00030DAC File Offset: 0x0002EFAC
	[Server]
	public void ClaimChallengeReward(Challenge challenge)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void ChallengeManager::ClaimChallengeReward(Challenge)' called when server was not active");
			return;
		}
		if (challenge == null)
		{
			return;
		}
		if (!this.activeChallenges.ContainsKey(challenge))
		{
			return;
		}
		ChallengeProgress challengeProgress = this.activeChallenges[challenge];
		if (!challengeProgress.isCompleted || challengeProgress.isClaimed)
		{
			return;
		}
		challengeProgress.isClaimed = true;
		int ticketReward = challenge.GetTicketReward();
		if (ticketReward > 0)
		{
			NetworkSingleton<MoneyManager>.Instance.TryChangeTicketBalance((long)ticketReward);
			Debug.Log(string.Format("[ChallengeManager] Automatically received {0} tickets for completing challenge: {1}", ticketReward, challenge.challengeName));
			this.RpcNotifyChallengeRewardAwarded(challenge.challengeName, ticketReward);
		}
		challenge.ResetChallenge();
		if (challenge.conditions != null)
		{
			foreach (ChallengeConditionData challengeConditionData in challenge.conditions)
			{
				if (challengeConditionData != null)
				{
					challengeConditionData.ResetCondition();
					ConditionState orCreateState = this.conditionStateTracker.GetOrCreateState(challengeConditionData);
					if (orCreateState != null)
					{
						orCreateState.Reset();
					}
				}
			}
		}
		this.activeChallenges.Remove(challenge);
		Debug.Log("[ChallengeManager] Challenge " + challenge.challengeName + " reset and removed. Can be purchased again.");
		this.UpdateChallenges();
	}

	// Token: 0x06000BF4 RID: 3060 RVA: 0x00030EE8 File Offset: 0x0002F0E8
	[ClientRpc]
	private void RpcNotifyChallengeRewardAwarded(string challengeName, int ticketReward)
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		writer.WriteString(challengeName);
		writer.WriteVarInt(ticketReward);
		this.SendRPCInternal("System.Void ChallengeManager::RpcNotifyChallengeRewardAwarded(System.String,System.Int32)", -1363418638, writer, 0, true);
		NetworkWriterPool.Return(writer);
	}

	// Token: 0x06000BF5 RID: 3061 RVA: 0x00030F2C File Offset: 0x0002F12C
	[Server]
	private void OnGameResultRegistered(long bet, long payout, PlayerProfile playerProfile, CasinoGameType gameType, Vector3 position, bool hadTipsyFortune, bool hadInspiringMelody, bool hadImmunity, Dictionary<string, object> gameSpecificData)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void ChallengeManager::OnGameResultRegistered(System.Int64,System.Int64,PlayerProfile,CasinoGameType,UnityEngine.Vector3,System.Boolean,System.Boolean,System.Boolean,System.Collections.Generic.Dictionary`2<System.String,System.Object>)' called when server was not active");
			return;
		}
		if (!this.challengesEnabled)
		{
			return;
		}
		ChallengeContext challengeContext = new ChallengeContext
		{
			bet = bet,
			payout = payout,
			gameType = gameType,
			gamePosition = position,
			hadTipsyFortuneBuff = hadTipsyFortune,
			hadInspiringMelodyBuff = hadInspiringMelody,
			hadImmunityBuff = hadImmunity,
			gameSpecificData = (gameSpecificData ?? new Dictionary<string, object>())
		};
		foreach (KeyValuePair<Challenge, ChallengeProgress> keyValuePair in new List<KeyValuePair<Challenge, ChallengeProgress>>(this.activeChallenges))
		{
			Challenge key = keyValuePair.Key;
			ChallengeProgress value = keyValuePair.Value;
			if (!(key == null) && this.activeChallenges.ContainsKey(key))
			{
				challengeContext.quotaAtActivation = value.quotaAtActivation;
				key.OnGameResult(bet, payout, gameType, position, challengeContext);
				bool isCompleted = value.isCompleted;
				value.UpdateProgress(challengeContext);
				if (value.isCompleted && !isCompleted && this.challengesCanComplete)
				{
					Action<Challenge> onChallengeCompleted = this.OnChallengeCompleted;
					if (onChallengeCompleted != null)
					{
						onChallengeCompleted(key);
					}
					this.RpcNotifyChallengeCompleted(key.challengeName, value.progress, value.progressText);
					this.ClaimChallengeReward(key);
				}
				if (this.activeChallenges.ContainsKey(key))
				{
					Action<Challenge> onChallengeProgressUpdated = this.OnChallengeProgressUpdated;
					if (onChallengeProgressUpdated != null)
					{
						onChallengeProgressUpdated(key);
					}
				}
			}
		}
		this.UpdateChallenges();
	}

	// Token: 0x06000BF6 RID: 3062 RVA: 0x000310B8 File Offset: 0x0002F2B8
	[ClientRpc]
	private void RpcNotifyChallengeCompleted(string challengeName, float progress01, string progressText)
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		writer.WriteString(challengeName);
		writer.WriteFloat(progress01);
		writer.WriteString(progressText);
		this.SendRPCInternal("System.Void ChallengeManager::RpcNotifyChallengeCompleted(System.String,System.Single,System.String)", -878636952, writer, 0, true);
		NetworkWriterPool.Return(writer);
	}

	// Token: 0x06000BF7 RID: 3063 RVA: 0x00031108 File Offset: 0x0002F308
	[Server]
	public void UpdateChallengeProgress()
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void ChallengeManager::UpdateChallengeProgress()' called when server was not active");
			return;
		}
		if (!this.challengesEnabled)
		{
			return;
		}
		ChallengeContext context = new ChallengeContext
		{
			bet = 0L,
			payout = 0L,
			gameType = CasinoGameType.Blackjack,
			gamePosition = Vector3.zero,
			hadTipsyFortuneBuff = false,
			hadInspiringMelodyBuff = false,
			hadImmunityBuff = false
		};
		PlayerProfile[] array = Object.FindObjectsByType<PlayerProfile>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
		if (array.Length != 0)
		{
			PlayerProfile playerProfile = array[0];
		}
		foreach (KeyValuePair<Challenge, ChallengeProgress> keyValuePair in this.activeChallenges)
		{
			Challenge key = keyValuePair.Key;
			ChallengeProgress value = keyValuePair.Value;
			if (!(key == null))
			{
				bool isCompleted = value.isCompleted;
				value.UpdateProgress(context);
				if (value.isCompleted && !isCompleted && this.challengesCanComplete)
				{
					Action<Challenge> onChallengeCompleted = this.OnChallengeCompleted;
					if (onChallengeCompleted != null)
					{
						onChallengeCompleted(key);
					}
					this.RpcNotifyChallengeCompleted(key.challengeName, value.progress, value.progressText);
				}
				Action<Challenge> onChallengeProgressUpdated = this.OnChallengeProgressUpdated;
				if (onChallengeProgressUpdated != null)
				{
					onChallengeProgressUpdated(key);
				}
			}
		}
		this.UpdateChallenges();
	}

	// Token: 0x06000BF8 RID: 3064 RVA: 0x00031250 File Offset: 0x0002F450
	[Server]
	public void ResetAllChallenges()
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void ChallengeManager::ResetAllChallenges()' called when server was not active");
			return;
		}
		foreach (KeyValuePair<Challenge, ChallengeProgress> keyValuePair in this.activeChallenges)
		{
			Challenge key = keyValuePair.Key;
			ChallengeProgress value = keyValuePair.Value;
			if (!(key == null))
			{
				key.ResetChallenge();
				value.Reset();
			}
		}
		this.conditionStateTracker.ResetAll();
	}

	// Token: 0x06000BF9 RID: 3065 RVA: 0x000312E4 File Offset: 0x0002F4E4
	[Server]
	public void ServerResetAllChallenges()
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void ChallengeManager::ServerResetAllChallenges()' called when server was not active");
			return;
		}
		this.ResetAllChallenges();
		this.activeChallenges.Clear();
		this.UpdateChallenges();
	}

	// Token: 0x06000BFA RID: 3066 RVA: 0x00031314 File Offset: 0x0002F514
	[Server]
	public bool ServerActivateChallengeById(int challengeId)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Boolean ChallengeManager::ServerActivateChallengeById(System.Int32)' called when server was not active");
			return default(bool);
		}
		Challenge challenge = this.allChallenges.FirstOrDefault((Challenge x) => x != null && x.challengeID == challengeId);
		return !(challenge == null) && this.ActivateChallenge(challenge);
	}

	// Token: 0x06000BFB RID: 3067 RVA: 0x0003137C File Offset: 0x0002F57C
	[Server]
	public void ServerCompleteAllActiveChallenges()
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void ChallengeManager::ServerCompleteAllActiveChallenges()' called when server was not active");
			return;
		}
		foreach (ChallengeProgress challengeProgress in this.GetActiveChallenges().ToList<ChallengeProgress>())
		{
			if (!(((challengeProgress != null) ? challengeProgress.challenge : null) == null) && !challengeProgress.isClaimed)
			{
				Challenge challenge = challengeProgress.challenge;
				challengeProgress.isCompleted = true;
				challengeProgress.progress = 1f;
				ChallengeContext context = new ChallengeContext
				{
					bet = challengeProgress.lastBet,
					payout = challengeProgress.lastPayout,
					gameType = challengeProgress.lastGameType,
					gamePosition = Vector3.zero,
					quotaAtActivation = challengeProgress.quotaAtActivation,
					hadTipsyFortuneBuff = false,
					hadInspiringMelodyBuff = false,
					hadImmunityBuff = false
				};
				challengeProgress.progressText = challenge.GetProgressText(context);
				this.RpcNotifyChallengeCompleted(challenge.challengeName, 1f, challengeProgress.progressText);
				this.ClaimChallengeReward(challenge);
			}
		}
	}

	// Token: 0x06000BFC RID: 3068 RVA: 0x000314A4 File Offset: 0x0002F6A4
	public ConditionState GetConditionState(ChallengeConditionData condition)
	{
		return this.conditionStateTracker.GetOrCreateState(condition);
	}

	// Token: 0x06000BFD RID: 3069 RVA: 0x000314B2 File Offset: 0x0002F6B2
	private void UpdateActiveChallengesDisplay()
	{
		if (this._challengeSettings != null)
		{
			this._challengeSettings.UpdateActiveChallenges();
		}
		this.UpdateActiveChallengesUI();
	}

	// Token: 0x06000BFE RID: 3070 RVA: 0x000314D4 File Offset: 0x0002F6D4
	private void UpdateActiveChallengesUI()
	{
		if (this.activeChallengeListParent == null || this.activeChallengeEntryPrefab == null)
		{
			return;
		}
		HashSet<int> hashSet = new HashSet<int>();
		foreach (ChallengeProgress challengeProgress in this.GetActiveChallenges())
		{
			if (challengeProgress != null && !(challengeProgress.challenge == null) && !challengeProgress.isClaimed)
			{
				int challengeID = challengeProgress.challenge.challengeID;
				hashSet.Add(challengeID);
				ChallengeEntryUI challengeEntryUI;
				if (this.activeChallengeEntries.TryGetValue(challengeID, out challengeEntryUI))
				{
					challengeEntryUI.SetData(challengeProgress);
				}
				else
				{
					challengeEntryUI = Object.Instantiate<ChallengeEntryUI>(this.activeChallengeEntryPrefab, this.activeChallengeListParent);
					challengeEntryUI.SetData(challengeProgress);
					this.activeChallengeEntries[challengeID] = challengeEntryUI;
				}
			}
		}
		List<int> list = new List<int>();
		foreach (KeyValuePair<int, ChallengeEntryUI> keyValuePair in this.activeChallengeEntries)
		{
			if (!hashSet.Contains(keyValuePair.Key) && !this.completedChallengeEntryIds.Contains(keyValuePair.Key))
			{
				if (keyValuePair.Value != null)
				{
					Object.Destroy(keyValuePair.Value.gameObject);
				}
				list.Add(keyValuePair.Key);
			}
		}
		foreach (int key in list)
		{
			this.activeChallengeEntries.Remove(key);
		}
	}

	// Token: 0x06000BFF RID: 3071 RVA: 0x00031694 File Offset: 0x0002F894
	public void AddChallenge(Challenge challenge)
	{
		if (challenge == null || this.allChallenges.Contains(challenge))
		{
			return;
		}
		this.allChallenges.Add(challenge);
	}

	// Token: 0x06000C00 RID: 3072 RVA: 0x000316BA File Offset: 0x0002F8BA
	public void RemoveChallenge(Challenge challenge)
	{
		if (challenge == null)
		{
			return;
		}
		this.allChallenges.Remove(challenge);
	}

	// Token: 0x06000C01 RID: 3073 RVA: 0x000316D4 File Offset: 0x0002F8D4
	[Server]
	public List<Challenge> GetChallengesByFloorIndex(int floorIndex)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Collections.Generic.List`1<Challenge> ChallengeManager::GetChallengesByFloorIndex(System.Int32)' called when server was not active");
			return null;
		}
		if (this._challengeSettings != null)
		{
			List<Challenge> challengesByFloorIndex = this._challengeSettings.GetChallengesByFloorIndex(floorIndex);
			if (challengesByFloorIndex != null && challengesByFloorIndex.Count > 0)
			{
				return challengesByFloorIndex;
			}
		}
		return (from c in this.allChallenges
		where c != null && c.floorIndex == floorIndex
		select c).ToList<Challenge>();
	}

	// Token: 0x06000C02 RID: 3074 RVA: 0x0003175C File Offset: 0x0002F95C
	[Server]
	public Challenge GetRandomChallenge(int floorIndex)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'Challenge ChallengeManager::GetRandomChallenge(System.Int32)' called when server was not active");
			return null;
		}
		if (this._challengeSettings != null)
		{
			Challenge randomChallenge = this._challengeSettings.GetRandomChallenge(floorIndex);
			if (randomChallenge != null)
			{
				return randomChallenge;
			}
		}
		List<Challenge> list = (from c in this.allChallenges
		where c != null && c.floorIndex == floorIndex
		select c).ToList<Challenge>();
		if (list.Count == 0)
		{
			return null;
		}
		return list.GetRandomElement<Challenge>();
	}

	// Token: 0x06000C03 RID: 3075 RVA: 0x000317F0 File Offset: 0x0002F9F0
	[Server]
	public bool ActivateChallenge(Challenge challenge)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Boolean ChallengeManager::ActivateChallenge(Challenge)' called when server was not active");
			return default(bool);
		}
		if (challenge == null)
		{
			return false;
		}
		ChallengeProgress challengeProgress = new ChallengeProgress(challenge);
		challengeProgress.quotaAtActivation = ((NetworkSingleton<GameManager>.Instance != null) ? NetworkSingleton<GameManager>.Instance.currentQuota : 0L);
		if (challenge.conditions != null)
		{
			foreach (ChallengeConditionData challengeConditionData in challenge.conditions)
			{
				if (challengeConditionData != null)
				{
					this.conditionStateTracker.GetOrCreateState(challengeConditionData);
				}
			}
		}
		challenge.ResetChallenge();
		this.activeChallenges[challenge] = challengeProgress;
		Debug.Log("[ChallengeManager] Activated challenge: " + challenge.challengeName);
		this.newChallengeSfx.RpcPlayOneShotWith3DPos();
		this.UpdateChallenges();
		return true;
	}

	// Token: 0x06000C04 RID: 3076 RVA: 0x000318E4 File Offset: 0x0002FAE4
	private IEnumerator LerpCanvasGroupAlpha(CanvasGroup canvasGroup, float targetAlpha, float duration)
	{
		if (canvasGroup == null || duration <= 0f)
		{
			if (canvasGroup != null)
			{
				canvasGroup.alpha = targetAlpha;
			}
			yield break;
		}
		float startAlpha = canvasGroup.alpha;
		float time = 0f;
		while (time < duration && canvasGroup != null)
		{
			time += Time.deltaTime;
			float t = Mathf.Clamp01(time / duration);
			canvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, t);
			yield return null;
		}
		if (canvasGroup != null)
		{
			canvasGroup.alpha = targetAlpha;
		}
		yield break;
	}

	// Token: 0x06000C05 RID: 3077 RVA: 0x00031901 File Offset: 0x0002FB01
	[Server]
	public void NotifyNewDailyChallengesGiven()
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void ChallengeManager::NotifyNewDailyChallengesGiven()' called when server was not active");
			return;
		}
		this.RpcClearCompletedChallengeEntries();
	}

	// Token: 0x06000C06 RID: 3078 RVA: 0x00031920 File Offset: 0x0002FB20
	[ClientRpc]
	private void RpcClearCompletedChallengeEntries()
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		this.SendRPCInternal("System.Void ChallengeManager::RpcClearCompletedChallengeEntries()", -838261859, writer, 0, true);
		NetworkWriterPool.Return(writer);
	}

	// Token: 0x06000C07 RID: 3079 RVA: 0x00031950 File Offset: 0x0002FB50
	[Server]
	public bool DeactivateChallenge(Challenge challenge)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Boolean ChallengeManager::DeactivateChallenge(Challenge)' called when server was not active");
			return default(bool);
		}
		if (challenge == null)
		{
			return false;
		}
		if (!this.activeChallenges.ContainsKey(challenge))
		{
			return false;
		}
		challenge.ResetChallenge();
		this.activeChallenges.Remove(challenge);
		Debug.Log("[ChallengeManager] Deactivated challenge: " + challenge.challengeName);
		this.UpdateChallenges();
		return true;
	}

	// Token: 0x06000C08 RID: 3080 RVA: 0x000319C8 File Offset: 0x0002FBC8
	[Server]
	private void UpdateChallenges()
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void ChallengeManager::UpdateChallenges()' called when server was not active");
			return;
		}
		List<ChallengeSyncData> list = new List<ChallengeSyncData>();
		foreach (KeyValuePair<Challenge, ChallengeProgress> keyValuePair in this.activeChallenges)
		{
			ConditionStateSyncData[] conditionStates = this.BuildConditionStateSyncData(keyValuePair.Key);
			list.Add(new ChallengeSyncData
			{
				challengeID = keyValuePair.Key.challengeID,
				progress = keyValuePair.Value.progress,
				isCompleted = keyValuePair.Value.isCompleted,
				isClaimed = keyValuePair.Value.isClaimed,
				completionCount = keyValuePair.Value.completionCount,
				lastBet = keyValuePair.Value.lastBet,
				lastPayout = keyValuePair.Value.lastPayout,
				lastGameType = keyValuePair.Value.lastGameType,
				conditionStates = conditionStates
			});
		}
		this.RpcSyncChallenges(list.ToArray());
	}

	// Token: 0x06000C09 RID: 3081 RVA: 0x00031B00 File Offset: 0x0002FD00
	[Command(requiresAuthority = false)]
	private void CmdRequestChallengesSync()
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		base.SendCommandInternal("System.Void ChallengeManager::CmdRequestChallengesSync()", 54202643, writer, 0, false);
		NetworkWriterPool.Return(writer);
	}

	// Token: 0x06000C0A RID: 3082 RVA: 0x00031B30 File Offset: 0x0002FD30
	private ConditionStateSyncData[] BuildConditionStateSyncData(Challenge challenge)
	{
		if (challenge == null || challenge.conditions == null)
		{
			return Array.Empty<ConditionStateSyncData>();
		}
		ConditionStateSyncData[] array = new ConditionStateSyncData[challenge.conditions.Count];
		for (int i = 0; i < challenge.conditions.Count; i++)
		{
			ChallengeConditionData condition = challenge.conditions[i];
			ConditionState orCreateState = this.conditionStateTracker.GetOrCreateState(condition);
			if (orCreateState == null)
			{
				array[i] = default(ConditionStateSyncData);
			}
			else
			{
				array[i] = new ConditionStateSyncData
				{
					currentWinCount = orCreateState.currentWinCount,
					consecutiveWinCount = orCreateState.consecutiveWinCount,
					currentLossCount = orCreateState.currentLossCount,
					consecutiveLossCount = orCreateState.consecutiveLossCount,
					totalBetAmount = orCreateState.totalBetAmount,
					totalPayoutAmount = orCreateState.totalPayoutAmount,
					totalProfit = orCreateState.totalProfit,
					elapsedSinceStart = Time.time - orCreateState.startTime,
					elapsedSinceLastGame = Time.time - orCreateState.lastGameTime
				};
			}
		}
		return array;
	}

	// Token: 0x06000C0B RID: 3083 RVA: 0x00031C43 File Offset: 0x0002FE43
	public ConditionStateSyncData[] GetConditionStateSnapshot(Challenge challenge)
	{
		return this.BuildConditionStateSyncData(challenge);
	}

	// Token: 0x06000C0C RID: 3084 RVA: 0x00031C4C File Offset: 0x0002FE4C
	[ClientRpc]
	private void RpcSyncChallenges(ChallengeSyncData[] challengeSyncData)
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		Mirror.GeneratedNetworkCode._Write_ChallengeSyncData[](writer, challengeSyncData);
		this.SendRPCInternal("System.Void ChallengeManager::RpcSyncChallenges(ChallengeSyncData[])", -304260989, writer, 0, true);
		NetworkWriterPool.Return(writer);
	}

	// Token: 0x06000C0D RID: 3085 RVA: 0x00031C88 File Offset: 0x0002FE88
	private void ApplyConditionStates(Challenge challenge, ConditionStateSyncData[] conditionStates)
	{
		if (challenge == null || challenge.conditions == null || conditionStates == null)
		{
			return;
		}
		int num = Mathf.Min(challenge.conditions.Count, conditionStates.Length);
		for (int i = 0; i < num; i++)
		{
			ChallengeConditionData condition = challenge.conditions[i];
			ConditionState orCreateState = this.conditionStateTracker.GetOrCreateState(condition);
			if (orCreateState != null)
			{
				ConditionStateSyncData conditionStateSyncData = conditionStates[i];
				orCreateState.currentWinCount = conditionStateSyncData.currentWinCount;
				orCreateState.consecutiveWinCount = conditionStateSyncData.consecutiveWinCount;
				orCreateState.currentLossCount = conditionStateSyncData.currentLossCount;
				orCreateState.consecutiveLossCount = conditionStateSyncData.consecutiveLossCount;
				orCreateState.totalBetAmount = conditionStateSyncData.totalBetAmount;
				orCreateState.totalPayoutAmount = conditionStateSyncData.totalPayoutAmount;
				orCreateState.totalProfit = conditionStateSyncData.totalProfit;
				orCreateState.startTime = Time.time - conditionStateSyncData.elapsedSinceStart;
				orCreateState.lastGameTime = Time.time - conditionStateSyncData.elapsedSinceLastGame;
			}
		}
	}

	// Token: 0x06000C0E RID: 3086 RVA: 0x00031D7C File Offset: 0x0002FF7C
	[Server]
	public void RestoreChallenge(Challenge challenge, bool isCompleted, bool isClaimed, float progress, int completionCount, long lastBet, long lastPayout, CasinoGameType lastGameType, long quotaAtActivation, ConditionStateSyncData[] conditionStates)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void ChallengeManager::RestoreChallenge(Challenge,System.Boolean,System.Boolean,System.Single,System.Int32,System.Int64,System.Int64,CasinoGameType,System.Int64,ConditionStateSyncData[])' called when server was not active");
			return;
		}
		if (challenge == null)
		{
			return;
		}
		if (!this.activeChallenges.ContainsKey(challenge))
		{
			ChallengeProgress value = new ChallengeProgress(challenge);
			this.activeChallenges[challenge] = value;
			if (challenge.conditions != null)
			{
				foreach (ChallengeConditionData challengeConditionData in challenge.conditions)
				{
					if (challengeConditionData != null)
					{
						this.conditionStateTracker.GetOrCreateState(challengeConditionData);
					}
				}
			}
		}
		ChallengeProgress challengeProgress = this.activeChallenges[challenge];
		challengeProgress.isCompleted = isCompleted;
		challengeProgress.isClaimed = isClaimed;
		challengeProgress.progress = progress;
		challengeProgress.completionCount = completionCount;
		challengeProgress.lastBet = lastBet;
		challengeProgress.lastPayout = lastPayout;
		challengeProgress.lastGameType = lastGameType;
		challengeProgress.quotaAtActivation = quotaAtActivation;
		this.ApplyConditionStates(challenge, conditionStates);
		ChallengeContext context = new ChallengeContext
		{
			bet = challengeProgress.lastBet,
			payout = challengeProgress.lastPayout,
			gameType = challengeProgress.lastGameType,
			gamePosition = Vector3.zero,
			quotaAtActivation = challengeProgress.quotaAtActivation,
			hadTipsyFortuneBuff = false,
			hadInspiringMelodyBuff = false,
			hadImmunityBuff = false
		};
		challengeProgress.progressText = challenge.GetProgressText(context);
		this.UpdateChallenges();
	}

	// Token: 0x06000C0F RID: 3087 RVA: 0x00031EE4 File Offset: 0x000300E4
	[Server]
	public void InitializeChallenges(bool enabled, bool canComplete)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void ChallengeManager::InitializeChallenges(System.Boolean,System.Boolean)' called when server was not active");
			return;
		}
		this.challengesEnabled = enabled;
		this.challengesCanComplete = canComplete;
		Debug.Log(string.Format("[ChallengeManager] Initialized - Enabled: {0}, Can Complete: {1}", enabled, canComplete));
	}

	// Token: 0x06000C10 RID: 3088 RVA: 0x00031F24 File Offset: 0x00030124
	[Server]
	public void ServerClearChallengesUI()
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void ChallengeManager::ServerClearChallengesUI()' called when server was not active");
			return;
		}
		this.RpcClearCompletedChallengeEntries();
	}

	// Token: 0x06000C11 RID: 3089 RVA: 0x00031F41 File Offset: 0x00030141
	[Server]
	public void SetChallengesEnabled(bool enabled)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void ChallengeManager::SetChallengesEnabled(System.Boolean)' called when server was not active");
			return;
		}
		this.challengesEnabled = enabled;
		Debug.Log(string.Format("[ChallengeManager] Challenges enabled: {0}", enabled));
	}

	// Token: 0x06000C12 RID: 3090 RVA: 0x00031F74 File Offset: 0x00030174
	[Server]
	public void SetChallengesCanComplete(bool canComplete)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void ChallengeManager::SetChallengesCanComplete(System.Boolean)' called when server was not active");
			return;
		}
		this.challengesCanComplete = canComplete;
		Debug.Log(string.Format("[ChallengeManager] Challenges can complete: {0}", canComplete));
	}

	// Token: 0x06000C14 RID: 3092 RVA: 0x00002321 File Offset: 0x00000521
	public override bool Weaved()
	{
		return true;
	}

	// Token: 0x06000C15 RID: 3093 RVA: 0x00032000 File Offset: 0x00030200
	protected void UserCode_RpcNotifyChallengeRewardAwarded__String__Int32(string challengeName, int ticketReward)
	{
		Debug.Log(string.Format("[ChallengeManager] Challenge completed! Received {0} tickets for: {1}", ticketReward, challengeName));
		Challenge challenge = this.allChallenges.FirstOrDefault((Challenge c) => c != null && c.challengeName == challengeName);
		if (challenge != null)
		{
			this.completedChallengeEntryIds.Add(challenge.challengeID);
			if (challenge.linkedAchievement)
			{
				challenge.linkedAchievement.UnlockAchievement();
			}
		}
		if (MonoSingleton<DaySummaryRuntime>.Instance != null)
		{
			MonoSingleton<DaySummaryRuntime>.Instance.Add(challengeName, ticketReward);
		}
	}

	// Token: 0x06000C16 RID: 3094 RVA: 0x0003209E File Offset: 0x0003029E
	protected static void InvokeUserCode_RpcNotifyChallengeRewardAwarded__String__Int32(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcNotifyChallengeRewardAwarded called on server.");
			return;
		}
		((ChallengeManager)obj).UserCode_RpcNotifyChallengeRewardAwarded__String__Int32(reader.ReadString(), reader.ReadVarInt());
	}

	// Token: 0x06000C17 RID: 3095 RVA: 0x000320D0 File Offset: 0x000302D0
	protected void UserCode_RpcNotifyChallengeCompleted__String__Single__String(string challengeName, float progress01, string progressText)
	{
		Debug.Log("[ChallengeManager] Challenge completed: " + challengeName);
		this.challengeCompleteSfx.PlayOneShotWith3DPos();
		Challenge challenge = this.allChallenges.FirstOrDefault((Challenge c) => c != null && c.challengeName == challengeName);
		if (challenge == null)
		{
			return;
		}
		this.completedChallengeEntryIds.Add(challenge.challengeID);
		ChallengeEntryUI challengeEntryUI;
		if (this.activeChallengeEntries.TryGetValue(challenge.challengeID, out challengeEntryUI) && challengeEntryUI != null)
		{
			float num = Mathf.Clamp01(progress01);
			if (num < 1f)
			{
				num = 1f;
			}
			ChallengeProgress data = new ChallengeProgress(challenge)
			{
				isCompleted = true,
				isClaimed = false,
				progress = num,
				progressText = (progressText ?? string.Empty)
			};
			challengeEntryUI.SetData(data);
			CanvasGroup componentInChildren = challengeEntryUI.GetComponentInChildren<CanvasGroup>();
			if (componentInChildren != null)
			{
				base.StartCoroutine(this.LerpCanvasGroupAlpha(componentInChildren, 1f, 0.25f));
			}
		}
	}

	// Token: 0x06000C18 RID: 3096 RVA: 0x000321D1 File Offset: 0x000303D1
	protected static void InvokeUserCode_RpcNotifyChallengeCompleted__String__Single__String(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcNotifyChallengeCompleted called on server.");
			return;
		}
		((ChallengeManager)obj).UserCode_RpcNotifyChallengeCompleted__String__Single__String(reader.ReadString(), reader.ReadFloat(), reader.ReadString());
	}

	// Token: 0x06000C19 RID: 3097 RVA: 0x00032207 File Offset: 0x00030407
	protected void UserCode_RpcClearCompletedChallengeEntries()
	{
		this.completedChallengeEntryIds.Clear();
		this.UpdateActiveChallengesDisplay();
	}

	// Token: 0x06000C1A RID: 3098 RVA: 0x0003221A File Offset: 0x0003041A
	protected static void InvokeUserCode_RpcClearCompletedChallengeEntries(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcClearCompletedChallengeEntries called on server.");
			return;
		}
		((ChallengeManager)obj).UserCode_RpcClearCompletedChallengeEntries();
	}

	// Token: 0x06000C1B RID: 3099 RVA: 0x0003223D File Offset: 0x0003043D
	protected void UserCode_CmdRequestChallengesSync()
	{
		if (!base.isServer)
		{
			return;
		}
		this.UpdateChallenges();
	}

	// Token: 0x06000C1C RID: 3100 RVA: 0x0003224E File Offset: 0x0003044E
	protected static void InvokeUserCode_CmdRequestChallengesSync(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdRequestChallengesSync called on client.");
			return;
		}
		((ChallengeManager)obj).UserCode_CmdRequestChallengesSync();
	}

	// Token: 0x06000C1D RID: 3101 RVA: 0x00032274 File Offset: 0x00030474
	protected void UserCode_RpcSyncChallenges__ChallengeSyncData[](ChallengeSyncData[] challengeSyncData)
	{
		HashSet<int> hashSet = new HashSet<int>();
		if (challengeSyncData != null)
		{
			foreach (ChallengeSyncData challengeSyncData2 in challengeSyncData)
			{
				hashSet.Add(challengeSyncData2.challengeID);
			}
		}
		if (hashSet.Count == 0)
		{
			this.activeChallenges.Clear();
			this.UpdateActiveChallengesDisplay();
			return;
		}
		List<Challenge> list = new List<Challenge>();
		foreach (Challenge challenge in this.activeChallenges.Keys)
		{
			if (challenge == null || !hashSet.Contains(challenge.challengeID))
			{
				list.Add(challenge);
			}
		}
		foreach (Challenge key in list)
		{
			this.activeChallenges.Remove(key);
		}
		for (int i = 0; i < challengeSyncData.Length; i++)
		{
			ChallengeSyncData syncData = challengeSyncData[i];
			Challenge challenge2 = this.allChallenges.FirstOrDefault((Challenge c) => c != null && c.challengeID == syncData.challengeID);
			if (!(challenge2 == null))
			{
				ChallengeProgress challengeProgress = new ChallengeProgress(challenge2);
				challengeProgress.progress = syncData.progress;
				challengeProgress.isCompleted = syncData.isCompleted;
				challengeProgress.isClaimed = syncData.isClaimed;
				challengeProgress.completionCount = syncData.completionCount;
				challengeProgress.lastBet = syncData.lastBet;
				challengeProgress.lastPayout = syncData.lastPayout;
				challengeProgress.lastGameType = syncData.lastGameType;
				this.ApplyConditionStates(challenge2, syncData.conditionStates);
				ChallengeContext context = new ChallengeContext
				{
					bet = challengeProgress.lastBet,
					payout = challengeProgress.lastPayout,
					gameType = challengeProgress.lastGameType,
					gamePosition = Vector3.zero,
					quotaAtActivation = challengeProgress.quotaAtActivation,
					hadTipsyFortuneBuff = false,
					hadInspiringMelodyBuff = false,
					hadImmunityBuff = false
				};
				challengeProgress.progressText = challenge2.GetProgressText(context);
				this.activeChallenges[challenge2] = challengeProgress;
			}
		}
		this.UpdateActiveChallengesDisplay();
	}

	// Token: 0x06000C1E RID: 3102 RVA: 0x000324F4 File Offset: 0x000306F4
	protected static void InvokeUserCode_RpcSyncChallenges__ChallengeSyncData[](NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcSyncChallenges called on server.");
			return;
		}
		((ChallengeManager)obj).UserCode_RpcSyncChallenges__ChallengeSyncData[](Mirror.GeneratedNetworkCode._Read_ChallengeSyncData[](reader));
	}

	// Token: 0x06000C1F RID: 3103 RVA: 0x00032520 File Offset: 0x00030720
	static ChallengeManager()
	{
		RemoteProcedureCalls.RegisterCommand(typeof(ChallengeManager), "System.Void ChallengeManager::CmdRequestChallengesSync()", new RemoteCallDelegate(ChallengeManager.InvokeUserCode_CmdRequestChallengesSync), false);
		RemoteProcedureCalls.RegisterRpc(typeof(ChallengeManager), "System.Void ChallengeManager::RpcNotifyChallengeRewardAwarded(System.String,System.Int32)", new RemoteCallDelegate(ChallengeManager.InvokeUserCode_RpcNotifyChallengeRewardAwarded__String__Int32));
		RemoteProcedureCalls.RegisterRpc(typeof(ChallengeManager), "System.Void ChallengeManager::RpcNotifyChallengeCompleted(System.String,System.Single,System.String)", new RemoteCallDelegate(ChallengeManager.InvokeUserCode_RpcNotifyChallengeCompleted__String__Single__String));
		RemoteProcedureCalls.RegisterRpc(typeof(ChallengeManager), "System.Void ChallengeManager::RpcClearCompletedChallengeEntries()", new RemoteCallDelegate(ChallengeManager.InvokeUserCode_RpcClearCompletedChallengeEntries));
		RemoteProcedureCalls.RegisterRpc(typeof(ChallengeManager), "System.Void ChallengeManager::RpcSyncChallenges(ChallengeSyncData[])", new RemoteCallDelegate(ChallengeManager.InvokeUserCode_RpcSyncChallenges__ChallengeSyncData[]));
	}

	// Token: 0x04000779 RID: 1913
	[Header("Challenge Settings")]
	[Tooltip("List of all available challenges (auto-populated from ChallengeSettings)")]
	[SerializeField]
	private List<Challenge> allChallenges = new List<Challenge>();

	// Token: 0x0400077A RID: 1914
	[Tooltip("Whether challenges are enabled")]
	[SerializeField]
	private bool challengesEnabled = true;

	// Token: 0x0400077B RID: 1915
	[Tooltip("Whether challenges can be completed (progress tracking still works even if false)")]
	[SerializeField]
	private bool challengesCanComplete = true;

	// Token: 0x0400077C RID: 1916
	private Dictionary<Challenge, ChallengeProgress> activeChallenges = new Dictionary<Challenge, ChallengeProgress>();

	// Token: 0x0400077D RID: 1917
	private ConditionStateTracker conditionStateTracker = new ConditionStateTracker();

	// Token: 0x04000780 RID: 1920
	private ChallengeSettings _challengeSettings;

	// Token: 0x04000781 RID: 1921
	[Header("Challenge UI")]
	[Tooltip("Parent transform that receives spawned challenge UI entries")]
	[SerializeField]
	private Transform activeChallengeListParent;

	// Token: 0x04000782 RID: 1922
	[Tooltip("Prefab with a ChallengeEntryUI component")]
	[SerializeField]
	private ChallengeEntryUI activeChallengeEntryPrefab;

	// Token: 0x04000783 RID: 1923
	[Header("SFX")]
	[SerializeField]
	private SFXComponent newChallengeSfx;

	// Token: 0x04000784 RID: 1924
	[SerializeField]
	private SFXComponent challengeCompleteSfx;

	// Token: 0x04000785 RID: 1925
	private readonly Dictionary<int, ChallengeEntryUI> activeChallengeEntries = new Dictionary<int, ChallengeEntryUI>();

	// Token: 0x04000786 RID: 1926
	private readonly HashSet<int> completedChallengeEntryIds = new HashSet<int>();
}
