using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Extensions;
using Mirror;
using UnityEngine;

// Token: 0x020001AC RID: 428
public class SaveManager : NetworkSingleton<SaveManager>
{
	// Token: 0x1700015D RID: 349
	// (get) Token: 0x06000F94 RID: 3988 RVA: 0x00041DEF File Offset: 0x0003FFEF
	// (set) Token: 0x06000F95 RID: 3989 RVA: 0x00041DF7 File Offset: 0x0003FFF7
	public string CurrentSaveName { get; private set; }

	// Token: 0x1700015E RID: 350
	// (get) Token: 0x06000F96 RID: 3990 RVA: 0x00041914 File Offset: 0x0003FB14
	private string SaveDirectoryPath
	{
		get
		{
			return Path.Combine(Application.persistentDataPath, "Saves");
		}
	}

	// Token: 0x06000F97 RID: 3991 RVA: 0x00041E00 File Offset: 0x00040000
	public IEnumerator LoadGameSaveCoroutine()
	{
		string @string = PlayerPrefs.GetString("SelectedSaveData", "");
		string string2 = PlayerPrefs.GetString("SelectedSaveName", "");
		if (!string.IsNullOrEmpty(@string))
		{
			try
			{
				this.currentSaveData = JsonUtility.FromJson<SaveData>(@string);
				if (this.currentSaveData != null)
				{
					this.CurrentSaveName = this.currentSaveData.saveName;
					Debug.Log(string.Format("[SaveManager] Loaded save data from PlayerPrefs: {0} (money: {1}, tickets: {2})", this.CurrentSaveName, this.currentSaveData.money, this.currentSaveData.tickets));
				}
			}
			catch (Exception ex)
			{
				Debug.LogError("[SaveManager] Failed to parse save data from PlayerPrefs: " + ex.Message);
			}
		}
		if (this.currentSaveData == null && !string.IsNullOrEmpty(string2))
		{
			this.currentSaveData = this.LoadSaveDataFromFile(string2);
			if (this.currentSaveData != null)
			{
				this.CurrentSaveName = this.currentSaveData.saveName;
				Debug.Log("[SaveManager] Loaded save data from file: " + this.CurrentSaveName);
			}
		}
		if (this.currentSaveData != null)
		{
			if (this.currentSaveData.seed == 0)
			{
				this.currentSaveData.seed = Random.Range(int.MinValue, int.MaxValue);
				Debug.Log(string.Format("[SaveManager] Generated new seed for old save: {0}", this.currentSaveData.seed));
			}
			if (NetworkSingleton<SeededRandomManager>.Instance != null)
			{
				NetworkSingleton<SeededRandomManager>.Instance.InitializeSeed(this.currentSaveData.seed);
			}
		}
		this.LoadGame();
		yield return null;
		yield break;
	}

	// Token: 0x06000F98 RID: 3992 RVA: 0x00041E10 File Offset: 0x00040010
	[Server]
	public void SaveGame()
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void SaveManager::SaveGame()' called when server was not active");
			return;
		}
		if (this.currentSaveData == null || string.IsNullOrEmpty(this.CurrentSaveName))
		{
			Debug.LogWarning("[SaveManager] No save data to save");
			return;
		}
		GameManager instance = NetworkSingleton<GameManager>.Instance;
		MoneyManager instance2 = NetworkSingleton<MoneyManager>.Instance;
		ItemManager instance3 = NetworkSingleton<ItemManager>.Instance;
		ChallengeManager instance4 = NetworkSingleton<ChallengeManager>.Instance;
		if (instance == null || instance2 == null || instance3 == null)
		{
			Debug.LogError("[SaveManager] Required managers not found");
			return;
		}
		this.currentSaveData.saveTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
		this.currentSaveData.successfulQuota = instance.successfulQuota;
		this.currentSaveData.daysLeft = instance.daysLeft;
		this.currentSaveData.daysPassed = instance.daysPassed;
		this.currentSaveData.currentQuota = instance.currentQuota;
		this.currentSaveData.currentFloor = instance.currentFloor;
		this.currentSaveData.requiredQuotaToNextFloor = instance.requiredQuotaToNextFloor;
		this.currentSaveData.money = instance2.balance;
		this.currentSaveData.tickets = instance2.ticketBalance;
		if (NetworkSingleton<PayoutTracker>.Instance != null)
		{
			this.currentSaveData.payoutTotalWins = NetworkSingleton<PayoutTracker>.Instance.GetLifetimeTotalWins();
			this.currentSaveData.payoutTotalLosses = NetworkSingleton<PayoutTracker>.Instance.GetLifetimeTotalLosses();
		}
		if (this.currentSaveData.seed == 0)
		{
			this.currentSaveData.seed = Random.Range(int.MinValue, int.MaxValue);
		}
		this.currentSaveData.itemIds = instance3.GetCurrentItemIds();
		this.currentSaveData.challengeIds.Clear();
		this.currentSaveData.challengeProgress.Clear();
		if (instance4 != null)
		{
			foreach (ChallengeProgress challengeProgress in instance4.GetActiveChallenges())
			{
				if (((challengeProgress != null) ? challengeProgress.challenge : null) != null)
				{
					this.currentSaveData.challengeIds.Add(challengeProgress.challenge.challengeID);
					ConditionStateSyncData[] conditionStateSnapshot = instance4.GetConditionStateSnapshot(challengeProgress.challenge);
					ChallengeProgressSaveData challengeProgressSaveData = new ChallengeProgressSaveData
					{
						challengeID = challengeProgress.challenge.challengeID,
						progress = challengeProgress.progress,
						isCompleted = challengeProgress.isCompleted,
						isClaimed = challengeProgress.isClaimed,
						completionCount = challengeProgress.completionCount,
						lastBet = challengeProgress.lastBet,
						lastPayout = challengeProgress.lastPayout,
						lastGameType = challengeProgress.lastGameType,
						quotaAtActivation = challengeProgress.quotaAtActivation
					};
					if (conditionStateSnapshot != null && conditionStateSnapshot.Length != 0)
					{
						challengeProgressSaveData.conditionStates.AddRange(conditionStateSnapshot);
					}
					this.currentSaveData.challengeProgress.Add(challengeProgressSaveData);
				}
			}
		}
		this.currentSaveData.playerOrganStates.Clear();
		OrganManager instance5 = NetworkSingleton<OrganManager>.Instance;
		if (instance5 != null)
		{
			foreach (KeyValuePair<ulong, PlayerOrganData> keyValuePair in instance5.GetAllOrganDataBySteamId())
			{
				PlayerOrganData value = keyValuePair.Value;
				this.currentSaveData.playerOrganStates.Add(new PlayerOrganSaveData
				{
					steamId = keyValuePair.Key.ToString(),
					leftEye = value.leftEye,
					rightEye = value.rightEye,
					body = value.body,
					mouth = value.mouth
				});
			}
			Debug.Log(string.Format("[SaveManager] Saved organ states for {0} players", this.currentSaveData.playerOrganStates.Count));
		}
		this.currentSaveData.profitHistory.Clear();
		MoneyDisplayAndFeedbacks moneyDisplayAndFeedbacks = Object.FindFirstObjectByType<MoneyDisplayAndFeedbacks>();
		if (moneyDisplayAndFeedbacks != null)
		{
			foreach (KeyValuePair<string, long> keyValuePair2 in moneyDisplayAndFeedbacks.GetProfitHistorySnapshot())
			{
				this.currentSaveData.profitHistory.Add(new ProfitHistorySaveData
				{
					playerName = keyValuePair2.Key,
					profitAmount = keyValuePair2.Value
				});
			}
		}
		this.currentSaveData.playerUpgradeStates.Clear();
		UpgradeManager instance6 = NetworkSingleton<UpgradeManager>.Instance;
		if (instance6 != null)
		{
			foreach (KeyValuePair<ulong, PlayerUpgradeData> keyValuePair3 in instance6.GetAllUpgradeDataBySteamId())
			{
				List<PlayerUpgradeSaveData> playerUpgradeStates = this.currentSaveData.playerUpgradeStates;
				PlayerUpgradeSaveData playerUpgradeSaveData = new PlayerUpgradeSaveData();
				playerUpgradeSaveData.steamId = keyValuePair3.Key.ToString();
				playerUpgradeSaveData.upgrades = (from kvp in keyValuePair3.Value.Upgrades
				select new PlayerUpgradeValue
				{
					type = kvp.Key,
					value = kvp.Value
				}).ToList<PlayerUpgradeValue>();
				playerUpgradeStates.Add(playerUpgradeSaveData);
			}
		}
		this.SaveGameDataToFile(this.currentSaveData);
	}

	// Token: 0x06000F99 RID: 3993 RVA: 0x00042380 File Offset: 0x00040580
	[Server]
	public void LoadGame()
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void SaveManager::LoadGame()' called when server was not active");
			return;
		}
		if (this.currentSaveData == null)
		{
			Debug.LogWarning("[SaveManager] No save data to load");
			return;
		}
		if (this.hasLoadedGame)
		{
			Debug.Log("[SaveManager] Save data already applied, skipping duplicate load");
			return;
		}
		GameManager instance = NetworkSingleton<GameManager>.Instance;
		MoneyManager instance2 = NetworkSingleton<MoneyManager>.Instance;
		ItemManager instance3 = NetworkSingleton<ItemManager>.Instance;
		ChallengeManager instance4 = NetworkSingleton<ChallengeManager>.Instance;
		if (instance == null || instance2 == null || instance3 == null)
		{
			Debug.LogError("[SaveManager] Required managers not found - will retry");
			return;
		}
		instance.NetworksuccessfulQuota = this.currentSaveData.successfulQuota;
		instance.NetworkdaysLeft = this.currentSaveData.daysLeft;
		instance.NetworkdaysPassed = this.currentSaveData.daysPassed;
		instance.NetworkcurrentQuota = this.currentSaveData.currentQuota;
		instance.NetworkcurrentFloor = this.currentSaveData.currentFloor;
		instance.NetworkrequiredQuotaToNextFloor = this.currentSaveData.requiredQuotaToNextFloor;
		instance2.SetBalance(this.currentSaveData.money, null, ChangeType.Save);
		instance2.TrySetTicketBalance(this.currentSaveData.tickets);
		if (NetworkSingleton<PayoutTracker>.Instance != null)
		{
			NetworkSingleton<PayoutTracker>.Instance.SetLifetimeTotals(this.currentSaveData.payoutTotalWins, this.currentSaveData.payoutTotalLosses);
		}
		instance3.SetCurrentItems(this.currentSaveData.itemIds);
		if (instance4 != null && this.currentSaveData != null)
		{
			ChallengeSettings challengeSettings = Resources.Load<ChallengeSettings>("ChallengeSettings");
			int num = 0;
			if (this.currentSaveData.challengeProgress != null && this.currentSaveData.challengeProgress.Count > 0)
			{
				using (List<ChallengeProgressSaveData>.Enumerator enumerator = this.currentSaveData.challengeProgress.GetEnumerator())
				{
					while (enumerator.MoveNext())
					{
						ChallengeProgressSaveData savedProgress = enumerator.Current;
						Challenge challenge = challengeSettings.challenges.FirstOrDefault((Challenge c) => c.challengeID == savedProgress.challengeID);
						if (!(challenge == null))
						{
							ChallengeManager challengeManager = instance4;
							Challenge challenge2 = challenge;
							bool isCompleted = savedProgress.isCompleted;
							bool isClaimed = savedProgress.isClaimed;
							float progress = savedProgress.progress;
							int completionCount = savedProgress.completionCount;
							long lastBet = savedProgress.lastBet;
							long lastPayout = savedProgress.lastPayout;
							CasinoGameType lastGameType = savedProgress.lastGameType;
							long quotaAtActivation = savedProgress.quotaAtActivation;
							List<ConditionStateSyncData> conditionStates = savedProgress.conditionStates;
							challengeManager.RestoreChallenge(challenge2, isCompleted, isClaimed, progress, completionCount, lastBet, lastPayout, lastGameType, quotaAtActivation, (conditionStates != null) ? conditionStates.ToArray() : null);
							num++;
						}
					}
				}
				Debug.Log(string.Format("[SaveManager] Restored {0}/{1} challenges with progress", num, this.currentSaveData.challengeProgress.Count));
			}
			else
			{
				List<int> challengeIds = this.currentSaveData.challengeIds;
				if (challengeIds != null && challengeIds.Count > 0)
				{
					using (List<int>.Enumerator enumerator2 = this.currentSaveData.challengeIds.GetEnumerator())
					{
						while (enumerator2.MoveNext())
						{
							int challengeId = enumerator2.Current;
							if (instance4.ActivateChallenge(challengeSettings.challenges.FirstOrDefault((Challenge c) => c.challengeID == challengeId)))
							{
								num++;
							}
						}
					}
					Debug.Log(string.Format("[SaveManager] Activated {0}/{1} challenges", num, this.currentSaveData.challengeIds.Count));
				}
			}
		}
		OrganManager instance5 = NetworkSingleton<OrganManager>.Instance;
		if (instance5 != null && this.currentSaveData.playerOrganStates != null && this.currentSaveData.playerOrganStates.Count > 0)
		{
			int num2 = 0;
			foreach (PlayerOrganSaveData playerOrganSaveData in this.currentSaveData.playerOrganStates)
			{
				ulong steamId;
				if (!string.IsNullOrEmpty(playerOrganSaveData.steamId) && ulong.TryParse(playerOrganSaveData.steamId, out steamId))
				{
					bool mouth = playerOrganSaveData.mouth;
					instance5.SetOrganDataBySteamId(steamId, playerOrganSaveData.leftEye, playerOrganSaveData.rightEye, playerOrganSaveData.body, mouth);
					num2++;
				}
			}
			Debug.Log(string.Format("[SaveManager] Loaded organ states for {0} players", num2));
			instance5.ServerApplyAllOrganSettings();
		}
		UpgradeManager instance6 = NetworkSingleton<UpgradeManager>.Instance;
		if (instance6 != null && this.currentSaveData.playerUpgradeStates != null && this.currentSaveData.playerUpgradeStates.Count > 0)
		{
			int num3 = 0;
			foreach (PlayerUpgradeSaveData playerUpgradeSaveData in this.currentSaveData.playerUpgradeStates)
			{
				ulong steamId2;
				if (!string.IsNullOrEmpty(playerUpgradeSaveData.steamId) && ulong.TryParse(playerUpgradeSaveData.steamId, out steamId2))
				{
					foreach (PlayerUpgradeValue playerUpgradeValue in playerUpgradeSaveData.upgrades)
					{
						instance6.SetUpgradeData(steamId2, playerUpgradeValue.type, playerUpgradeValue.value);
					}
				}
			}
			Debug.Log(string.Format("[SaveManager] Loaded upgrade states for {0} players", num3));
		}
		MoneyDisplayAndFeedbacks moneyDisplayAndFeedbacks = Object.FindFirstObjectByType<MoneyDisplayAndFeedbacks>();
		if (moneyDisplayAndFeedbacks != null && this.currentSaveData.profitHistory != null)
		{
			Dictionary<string, long> dictionary = new Dictionary<string, long>();
			foreach (ProfitHistorySaveData profitHistorySaveData in this.currentSaveData.profitHistory)
			{
				if (!string.IsNullOrWhiteSpace(profitHistorySaveData.playerName))
				{
					dictionary[profitHistorySaveData.playerName] = profitHistorySaveData.profitAmount;
				}
			}
			moneyDisplayAndFeedbacks.SetProfitHistory(dictionary);
		}
		this.hasLoadedGame = true;
		Debug.Log(string.Format("[SaveManager] Applied save data to game state: {0} (money: {1}, tickets: {2})", this.CurrentSaveName, instance2.balance, instance2.ticketBalance));
	}

	// Token: 0x06000F9A RID: 3994 RVA: 0x000429C8 File Offset: 0x00040BC8
	[Server]
	public void ResetCurrentSaveToDefaults()
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void SaveManager::ResetCurrentSaveToDefaults()' called when server was not active");
			return;
		}
		if (string.IsNullOrEmpty(this.CurrentSaveName))
		{
			Debug.LogWarning("[SaveManager] No current save name to reset");
			return;
		}
		GameSettings gameSettings = Resources.Load<GameSettings>("GameSettings");
		if (gameSettings == null)
		{
			Debug.LogError("[SaveManager] GameSettings not found - cannot reset save");
			return;
		}
		long requiredQuotaToNextFloor = 0L;
		if (gameSettings.floorData != null && gameSettings.floorData.Count > 0)
		{
			requiredQuotaToNextFloor = ((gameSettings.floorData.Count > 1) ? gameSettings.floorData[1].requiredQuotaToAccess : gameSettings.floorData[0].requiredQuotaToAccess);
		}
		this.currentSaveData = new SaveData
		{
			saveName = this.CurrentSaveName,
			saveTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
			successfulQuota = 0,
			daysLeft = gameSettings.daysBeforeQuota,
			daysPassed = 0,
			currentQuota = gameSettings.startingQuota,
			currentFloor = 0,
			requiredQuotaToNextFloor = requiredQuotaToNextFloor,
			money = gameSettings.startingMoney,
			tickets = gameSettings.startingTicket,
			itemIds = new List<int>(),
			challengeIds = new List<int>(),
			challengeProgress = new List<ChallengeProgressSaveData>(),
			playerOrganStates = new List<PlayerOrganSaveData>(),
			profitHistory = new List<ProfitHistorySaveData>(),
			payoutTotalWins = 0L,
			payoutTotalLosses = 0L,
			seed = Random.Range(int.MinValue, int.MaxValue)
		};
		if (NetworkSingleton<SeededRandomManager>.Instance != null)
		{
			NetworkSingleton<SeededRandomManager>.Instance.InitializeSeed(this.currentSaveData.seed);
		}
		this.SaveGameDataToFile(this.currentSaveData);
		OrganManager instance = NetworkSingleton<OrganManager>.Instance;
		if (instance != null)
		{
			instance.ServerResetAllOrgansToDefaults();
		}
		UpgradeManager instance2 = NetworkSingleton<UpgradeManager>.Instance;
		if (instance2 != null)
		{
			instance2.ServerResetAllUpgradesToDefaults();
		}
		ChallengeManager instance3 = NetworkSingleton<ChallengeManager>.Instance;
		if (instance3 != null)
		{
			instance3.ServerResetAllChallenges();
		}
		MoneyDisplayAndFeedbacks instance4 = NetworkSingleton<MoneyDisplayAndFeedbacks>.Instance;
		if (instance4 != null)
		{
			instance4.ServerResetProfitHistory();
		}
		this.hasLoadedGame = false;
		Debug.Log("[SaveManager] Reset save to defaults: " + this.CurrentSaveName);
	}

	// Token: 0x06000F9B RID: 3995 RVA: 0x00042BE4 File Offset: 0x00040DE4
	private SaveData LoadSaveDataFromFile(string saveName)
	{
		if (string.IsNullOrEmpty(saveName))
		{
			Debug.LogError("[SaveManager] Cannot load save with empty name");
			return null;
		}
		if (!Directory.Exists(this.SaveDirectoryPath))
		{
			Directory.CreateDirectory(this.SaveDirectoryPath);
		}
		string text = Path.Combine(this.SaveDirectoryPath, saveName + ".json");
		if (!File.Exists(text))
		{
			Debug.LogError("[SaveManager] Save file not found: " + text);
			return null;
		}
		SaveData result;
		try
		{
			SaveData saveData = JsonUtility.FromJson<SaveData>(File.ReadAllText(text));
			Debug.Log("[SaveManager] Loaded save data from file: " + saveName);
			result = saveData;
		}
		catch (Exception ex)
		{
			Debug.LogError("[SaveManager] Failed to load save: " + ex.Message);
			result = null;
		}
		return result;
	}

	// Token: 0x06000F9C RID: 3996 RVA: 0x00042C9C File Offset: 0x00040E9C
	private void SaveGameDataToFile(SaveData saveData)
	{
		if (saveData == null || string.IsNullOrEmpty(saveData.saveName))
		{
			Debug.LogWarning("[SaveManager] No save data to save");
			return;
		}
		if (!Directory.Exists(this.SaveDirectoryPath))
		{
			Directory.CreateDirectory(this.SaveDirectoryPath);
		}
		string path = Path.Combine(this.SaveDirectoryPath, saveData.saveName + ".json");
		try
		{
			saveData.saveTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
			string contents = JsonUtility.ToJson(saveData, true);
			File.WriteAllText(path, contents);
			Debug.Log("[SaveManager] Saved game to file: " + saveData.saveName);
		}
		catch (Exception ex)
		{
			Debug.LogError("[SaveManager] Failed to save game: " + ex.Message);
		}
	}

	// Token: 0x06000F9E RID: 3998 RVA: 0x00002321 File Offset: 0x00000521
	public override bool Weaved()
	{
		return true;
	}

	// Token: 0x04000A2B RID: 2603
	private const string SAVE_DIRECTORY = "Saves";

	// Token: 0x04000A2C RID: 2604
	private SaveData currentSaveData;

	// Token: 0x04000A2E RID: 2606
	private bool hasLoadedGame;
}
