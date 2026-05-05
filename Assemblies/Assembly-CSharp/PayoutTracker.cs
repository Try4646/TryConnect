using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using Extensions;
using Mirror;
using UnityEngine;
using UnityEngine.SceneManagement;

// Token: 0x02000192 RID: 402
public class PayoutTracker : NetworkSingleton<PayoutTracker>
{
	// Token: 0x1400000D RID: 13
	// (add) Token: 0x06000EF0 RID: 3824 RVA: 0x0003E330 File Offset: 0x0003C530
	// (remove) Token: 0x06000EF1 RID: 3825 RVA: 0x0003E368 File Offset: 0x0003C568
	public event Action<PayoutRecord> OnPayoutRecorded;

	// Token: 0x06000EF2 RID: 3826 RVA: 0x0003E39D File Offset: 0x0003C59D
	protected override void OnAwake()
	{
		base.OnAwake();
		SyncList<PayoutRecord> syncList = this.payoutHistory;
		syncList.OnAdd = (Action<int>)Delegate.Combine(syncList.OnAdd, new Action<int>(this.OnPayoutAdded));
	}

	// Token: 0x06000EF3 RID: 3827 RVA: 0x0003E3CC File Offset: 0x0003C5CC
	private void OnEnable()
	{
		NetworkSingleton<GameResultsManager>.Instance.OnResultRegistered += this.OnGameResultRegistered;
		if (NetworkSingleton<MoneyManager>.Instance != null)
		{
			MoneyManager instance = NetworkSingleton<MoneyManager>.Instance;
			instance.OnBalanceChanged = (Action<BalanceChangeData>)Delegate.Combine(instance.OnBalanceChanged, new Action<BalanceChangeData>(this.OnBalanceChanged));
		}
	}

	// Token: 0x06000EF4 RID: 3828 RVA: 0x0003E424 File Offset: 0x0003C624
	private void OnDisable()
	{
		if (NetworkSingleton<GameResultsManager>.Instance != null)
		{
			NetworkSingleton<GameResultsManager>.Instance.OnResultRegistered -= this.OnGameResultRegistered;
		}
		if (NetworkSingleton<MoneyManager>.Instance != null)
		{
			MoneyManager instance = NetworkSingleton<MoneyManager>.Instance;
			instance.OnBalanceChanged = (Action<BalanceChangeData>)Delegate.Remove(instance.OnBalanceChanged, new Action<BalanceChangeData>(this.OnBalanceChanged));
		}
	}

	// Token: 0x06000EF5 RID: 3829 RVA: 0x0003E488 File Offset: 0x0003C688
	[Server]
	private void OnBalanceChanged(BalanceChangeData data)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void PayoutTracker::OnBalanceChanged(BalanceChangeData)' called when server was not active");
			return;
		}
		if (SceneManager.GetActiveScene().name != "CasinoScene")
		{
			return;
		}
		if (!this.enableTracking || data == null || data.changeType == ChangeType.Save || data.changeAmount == 0L)
		{
			return;
		}
		if (data.changeAmount > 0L)
		{
			this.NetworklifetimeTotalWins = this.lifetimeTotalWins + data.changeAmount;
			return;
		}
		this.NetworklifetimeTotalLosses = this.lifetimeTotalLosses + Math.Abs(data.changeAmount);
	}

	// Token: 0x06000EF6 RID: 3830 RVA: 0x0003E518 File Offset: 0x0003C718
	[Server]
	public void InitializeStartingPoints()
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void PayoutTracker::InitializeStartingPoints()' called when server was not active");
			return;
		}
		if (!this.enableTracking)
		{
			return;
		}
		List<PlayerProfile> list = (from p in Object.FindObjectsByType<PlayerProfile>(FindObjectsSortMode.None)
		where p != null && p.hasSynced && !string.IsNullOrEmpty(p.playerName)
		select p).ToList<PlayerProfile>();
		float time = Time.time;
		using (List<PlayerProfile>.Enumerator enumerator = list.GetEnumerator())
		{
			while (enumerator.MoveNext())
			{
				PlayerProfile player = enumerator.Current;
				if (!this.payoutHistory.Any((PayoutRecord r) => r.playerProfile == player))
				{
					PayoutRecord item = new PayoutRecord
					{
						timestamp = time,
						playerName = player.playerName,
						playerProfile = player,
						bet = 0L,
						payout = 0L,
						profit = 0L,
						isWin = false,
						isLoss = false,
						gameType = CasinoGameType.Blackjack,
						gamePosition = Vector3.zero
					};
					this.payoutHistory.Add(item);
				}
			}
		}
	}

	// Token: 0x06000EF7 RID: 3831 RVA: 0x0003E64C File Offset: 0x0003C84C
	[Server]
	private void OnGameResultRegistered(long bet, long payout, PlayerProfile playerProfile, CasinoGameType gameType, Vector3 gamePosition, bool hadTipsyFortune, bool hadInspiringMelody, bool hadImmunity, Dictionary<string, object> gameSpecificData)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void PayoutTracker::OnGameResultRegistered(System.Int64,System.Int64,PlayerProfile,CasinoGameType,UnityEngine.Vector3,System.Boolean,System.Boolean,System.Boolean,System.Collections.Generic.Dictionary`2<System.String,System.Object>)' called when server was not active");
			return;
		}
		if (!this.enableTracking || playerProfile == null)
		{
			return;
		}
		if (!this.payoutHistory.Any((PayoutRecord r) => r.playerProfile == playerProfile))
		{
			PayoutRecord item = new PayoutRecord
			{
				timestamp = Time.time,
				playerName = playerProfile.playerName,
				playerProfile = playerProfile,
				bet = 0L,
				payout = 0L,
				profit = 0L,
				isWin = false,
				isLoss = false,
				gameType = CasinoGameType.Blackjack,
				gamePosition = Vector3.zero
			};
			this.payoutHistory.Add(item);
		}
		long num = payout - bet;
		bool isWin = num > 0L;
		bool isLoss = num < 0L;
		PayoutRecord item2 = new PayoutRecord
		{
			timestamp = Time.time,
			playerName = playerProfile.playerName,
			playerProfile = playerProfile,
			bet = bet,
			payout = payout,
			profit = num,
			isWin = isWin,
			isLoss = isLoss,
			gameType = gameType,
			gamePosition = gamePosition
		};
		this.payoutHistory.Add(item2);
		if (this.payoutHistory.Count > this.maxHistoryRecords)
		{
			int num2 = this.payoutHistory.Count - this.maxHistoryRecords;
			for (int i = 0; i < num2; i++)
			{
				this.payoutHistory.RemoveAt(0);
			}
		}
		Debug.Log(string.Format("[PayoutTracker] Recorded: {0} - Bet: {1}, Payout: {2}, Profit: {3} ({4})", new object[]
		{
			playerProfile.playerName,
			bet,
			payout,
			num,
			gameType
		}));
	}

	// Token: 0x06000EF8 RID: 3832 RVA: 0x0003E82A File Offset: 0x0003CA2A
	private void OnPayoutAdded(int index)
	{
		if (index >= 0 && index < this.payoutHistory.Count)
		{
			Action<PayoutRecord> onPayoutRecorded = this.OnPayoutRecorded;
			if (onPayoutRecorded == null)
			{
				return;
			}
			onPayoutRecorded(this.payoutHistory[index]);
		}
	}

	// Token: 0x06000EF9 RID: 3833 RVA: 0x0003E85C File Offset: 0x0003CA5C
	public List<PayoutRecord> GetPlayerRecords(PlayerProfile playerProfile)
	{
		if (playerProfile == null)
		{
			return new List<PayoutRecord>();
		}
		return (from r in this.payoutHistory
		where r.playerProfile == playerProfile
		select r).ToList<PayoutRecord>();
	}

	// Token: 0x06000EFA RID: 3834 RVA: 0x0003E8A8 File Offset: 0x0003CAA8
	public List<PayoutRecord> GetPlayerRecords(string playerName)
	{
		if (string.IsNullOrEmpty(playerName))
		{
			return new List<PayoutRecord>();
		}
		return (from r in this.payoutHistory
		where r.playerName == playerName
		select r).ToList<PayoutRecord>();
	}

	// Token: 0x06000EFB RID: 3835 RVA: 0x0003E8F1 File Offset: 0x0003CAF1
	public List<PayoutRecord> GetAllRecords()
	{
		return new List<PayoutRecord>(this.payoutHistory);
	}

	// Token: 0x06000EFC RID: 3836 RVA: 0x0003E900 File Offset: 0x0003CB00
	public List<PayoutRecord> GetRecordsInTimeRange(float startTime, float endTime)
	{
		return (from r in this.payoutHistory
		where r.timestamp >= startTime && r.timestamp <= endTime
		select r).ToList<PayoutRecord>();
	}

	// Token: 0x06000EFD RID: 3837 RVA: 0x0003E940 File Offset: 0x0003CB40
	public List<ProfitDataPoint> GetPlayerProfitOverTime(PlayerProfile playerProfile)
	{
		List<PayoutRecord> list = (from r in this.GetPlayerRecords(playerProfile)
		orderby r.timestamp
		select r).ToList<PayoutRecord>();
		List<ProfitDataPoint> list2 = new List<ProfitDataPoint>();
		long num = 0L;
		foreach (PayoutRecord payoutRecord in list)
		{
			num += payoutRecord.profit;
			list2.Add(new ProfitDataPoint
			{
				time = payoutRecord.timestamp,
				cumulativeProfit = num
			});
		}
		return list2;
	}

	// Token: 0x06000EFE RID: 3838 RVA: 0x0003E9E8 File Offset: 0x0003CBE8
	public List<ProfitDataPoint> GetPlayerProfitByMinute(PlayerProfile playerProfile)
	{
		List<PayoutRecord> list = (from r in this.GetPlayerRecords(playerProfile)
		orderby r.timestamp
		select r).ToList<PayoutRecord>();
		if (list.Count == 0)
		{
			return new List<ProfitDataPoint>();
		}
		Dictionary<int, long> dictionary = new Dictionary<int, long>();
		foreach (PayoutRecord payoutRecord in list)
		{
			int num = Mathf.FloorToInt(payoutRecord.timestamp / 60f);
			if (!dictionary.ContainsKey(num))
			{
				dictionary[num] = 0L;
			}
			Dictionary<int, long> dictionary2 = dictionary;
			int key = num;
			dictionary2[key] += payoutRecord.profit;
		}
		List<ProfitDataPoint> list2 = new List<ProfitDataPoint>();
		long num2 = 0L;
		foreach (int num3 in (from m in dictionary.Keys
		orderby m
		select m).ToList<int>())
		{
			num2 += dictionary[num3];
			list2.Add(new ProfitDataPoint
			{
				time = (float)num3 * 60f,
				cumulativeProfit = num2
			});
		}
		return list2;
	}

	// Token: 0x06000EFF RID: 3839 RVA: 0x0003EB5C File Offset: 0x0003CD5C
	[Server]
	public void ClearHistory()
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void PayoutTracker::ClearHistory()' called when server was not active");
			return;
		}
		this.payoutHistory.Clear();
		Debug.Log("[PayoutTracker] History cleared");
	}

	// Token: 0x06000F00 RID: 3840 RVA: 0x0003EB88 File Offset: 0x0003CD88
	[Server]
	public List<PayoutRecord> RollbackLastSeconds(float seconds)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Collections.Generic.List`1<PayoutRecord> PayoutTracker::RollbackLastSeconds(System.Single)' called when server was not active");
			return null;
		}
		if (!this.enableTracking || seconds <= 0f || this.payoutHistory.Count == 0)
		{
			return new List<PayoutRecord>();
		}
		float cutoff = Time.time - seconds;
		List<PayoutRecord> list = (from r in this.payoutHistory
		where r.timestamp >= cutoff
		orderby r.timestamp
		select r).ToList<PayoutRecord>();
		if (list.Count == 0)
		{
			return list;
		}
		foreach (PayoutRecord item in list)
		{
			this.payoutHistory.Remove(item);
		}
		Debug.Log(string.Format("[PayoutTracker] Rolled back {0} records from last {1} seconds", list.Count, seconds));
		return list;
	}

	// Token: 0x06000F01 RID: 3841 RVA: 0x0003ECA0 File Offset: 0x0003CEA0
	public int GetRecordCount()
	{
		return this.payoutHistory.Count;
	}

	// Token: 0x06000F02 RID: 3842 RVA: 0x0003ECAD File Offset: 0x0003CEAD
	public long GetLifetimeTotalWins()
	{
		return this.lifetimeTotalWins;
	}

	// Token: 0x06000F03 RID: 3843 RVA: 0x0003ECB5 File Offset: 0x0003CEB5
	public long GetLifetimeTotalLosses()
	{
		return this.lifetimeTotalLosses;
	}

	// Token: 0x06000F04 RID: 3844 RVA: 0x0003ECBD File Offset: 0x0003CEBD
	public long GetLifetimeNetTotal()
	{
		return this.lifetimeTotalWins - this.lifetimeTotalLosses;
	}

	// Token: 0x06000F05 RID: 3845 RVA: 0x0003ECCC File Offset: 0x0003CECC
	[Server]
	public void SetLifetimeTotals(long totalWins, long totalLosses)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void PayoutTracker::SetLifetimeTotals(System.Int64,System.Int64)' called when server was not active");
			return;
		}
		this.NetworklifetimeTotalWins = Math.Max(0L, totalWins);
		this.NetworklifetimeTotalLosses = Math.Max(0L, totalLosses);
	}

	// Token: 0x06000F06 RID: 3846 RVA: 0x0003ECFF File Offset: 0x0003CEFF
	public PayoutTracker()
	{
		base.InitSyncObject(this.payoutHistory);
	}

	// Token: 0x06000F07 RID: 3847 RVA: 0x00002321 File Offset: 0x00000521
	public override bool Weaved()
	{
		return true;
	}

	// Token: 0x1700014C RID: 332
	// (get) Token: 0x06000F08 RID: 3848 RVA: 0x0003ED30 File Offset: 0x0003CF30
	// (set) Token: 0x06000F09 RID: 3849 RVA: 0x0003ED43 File Offset: 0x0003CF43
	public long NetworklifetimeTotalWins
	{
		get
		{
			return this.lifetimeTotalWins;
		}
		[param: In]
		set
		{
			base.GeneratedSyncVarSetter<long>(value, ref this.lifetimeTotalWins, 1UL, null);
		}
	}

	// Token: 0x1700014D RID: 333
	// (get) Token: 0x06000F0A RID: 3850 RVA: 0x0003ED60 File Offset: 0x0003CF60
	// (set) Token: 0x06000F0B RID: 3851 RVA: 0x0003ED73 File Offset: 0x0003CF73
	public long NetworklifetimeTotalLosses
	{
		get
		{
			return this.lifetimeTotalLosses;
		}
		[param: In]
		set
		{
			base.GeneratedSyncVarSetter<long>(value, ref this.lifetimeTotalLosses, 2UL, null);
		}
	}

	// Token: 0x06000F0C RID: 3852 RVA: 0x0003ED90 File Offset: 0x0003CF90
	public override void SerializeSyncVars(NetworkWriter writer, bool forceAll)
	{
		base.SerializeSyncVars(writer, forceAll);
		if (forceAll)
		{
			writer.WriteVarLong(this.lifetimeTotalWins);
			writer.WriteVarLong(this.lifetimeTotalLosses);
			return;
		}
		writer.WriteVarULong(this.syncVarDirtyBits);
		if ((this.syncVarDirtyBits & 1UL) != 0UL)
		{
			writer.WriteVarLong(this.lifetimeTotalWins);
		}
		if ((this.syncVarDirtyBits & 2UL) != 0UL)
		{
			writer.WriteVarLong(this.lifetimeTotalLosses);
		}
	}

	// Token: 0x06000F0D RID: 3853 RVA: 0x0003EE18 File Offset: 0x0003D018
	public override void DeserializeSyncVars(NetworkReader reader, bool initialState)
	{
		base.DeserializeSyncVars(reader, initialState);
		if (initialState)
		{
			base.GeneratedSyncVarDeserialize<long>(ref this.lifetimeTotalWins, null, reader.ReadVarLong());
			base.GeneratedSyncVarDeserialize<long>(ref this.lifetimeTotalLosses, null, reader.ReadVarLong());
			return;
		}
		long num = (long)reader.ReadVarULong();
		if ((num & 1L) != 0L)
		{
			base.GeneratedSyncVarDeserialize<long>(ref this.lifetimeTotalWins, null, reader.ReadVarLong());
		}
		if ((num & 2L) != 0L)
		{
			base.GeneratedSyncVarDeserialize<long>(ref this.lifetimeTotalLosses, null, reader.ReadVarLong());
		}
	}

	// Token: 0x0400098B RID: 2443
	[Header("Settings")]
	[SerializeField]
	private bool enableTracking = true;

	// Token: 0x0400098C RID: 2444
	[SerializeField]
	private int maxHistoryRecords = 10000;

	// Token: 0x0400098D RID: 2445
	[SerializeField]
	private readonly SyncList<PayoutRecord> payoutHistory = new SyncList<PayoutRecord>();

	// Token: 0x0400098E RID: 2446
	[SyncVar]
	[SerializeField]
	private long lifetimeTotalWins;

	// Token: 0x0400098F RID: 2447
	[SyncVar]
	[SerializeField]
	private long lifetimeTotalLosses;
}
