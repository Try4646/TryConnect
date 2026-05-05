using System;
using System.Collections.Generic;
using Extensions;
using Mirror;
using UnityEngine;

// Token: 0x0200016E RID: 366
public class GameResultsManager : NetworkSingleton<GameResultsManager>
{
	// Token: 0x1400000C RID: 12
	// (add) Token: 0x06000E0B RID: 3595 RVA: 0x0003A398 File Offset: 0x00038598
	// (remove) Token: 0x06000E0C RID: 3596 RVA: 0x0003A3D0 File Offset: 0x000385D0
	public event Action<long, long, PlayerProfile, CasinoGameType, Vector3, bool, bool, bool, Dictionary<string, object>> OnResultRegistered;

	// Token: 0x06000E0D RID: 3597 RVA: 0x0003A408 File Offset: 0x00038608
	[Server]
	public void RegisterResult(long bet, long payout, PlayerProfile playerProfile, CasinoGameType gameType, Vector3 position, bool hadTipsyFortune = false, bool hadInspiringMelody = false, bool hadImmunity = false, Dictionary<string, object> gameSpecificData = null)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void GameResultsManager::RegisterResult(System.Int64,System.Int64,PlayerProfile,CasinoGameType,UnityEngine.Vector3,System.Boolean,System.Boolean,System.Boolean,System.Collections.Generic.Dictionary`2<System.String,System.Object>)' called when server was not active");
			return;
		}
		PlayerResultData playerResultData;
		if (!this.results.TryGetValue(playerProfile, out playerResultData))
		{
			playerResultData = new PlayerResultData();
			this.results[playerProfile] = playerResultData;
		}
		playerResultData.totalBets += bet;
		playerResultData.totalPayouts += payout;
		GameResultBreakdown gameResultBreakdown;
		if (!playerResultData.ByGameType.TryGetValue(gameType, out gameResultBreakdown))
		{
			gameResultBreakdown = new GameResultBreakdown();
			playerResultData.ByGameType[gameType] = gameResultBreakdown;
		}
		gameResultBreakdown.totalBet += bet;
		gameResultBreakdown.totalPayout += payout;
		this.SetAsLastResult(bet, payout, playerProfile);
		Action<long, long, PlayerProfile, CasinoGameType, Vector3, bool, bool, bool, Dictionary<string, object>> onResultRegistered = this.OnResultRegistered;
		if (onResultRegistered != null)
		{
			onResultRegistered(bet, payout, playerProfile, gameType, position, hadTipsyFortune, hadInspiringMelody, hadImmunity, gameSpecificData);
		}
		this.UpdateDebugList(playerProfile, playerResultData);
	}

	// Token: 0x06000E0E RID: 3598 RVA: 0x0003A4DC File Offset: 0x000386DC
	[Server]
	private void SetAsLastResult(long bet, long payout, PlayerProfile playerProfile)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void GameResultsManager::SetAsLastResult(System.Int64,System.Int64,PlayerProfile)' called when server was not active");
			return;
		}
		PlayerResultData playerResultData = new PlayerResultData();
		playerResultData.totalBets = bet;
		playerResultData.totalPayouts = payout;
		this.lastResults[playerProfile] = playerResultData;
	}

	// Token: 0x06000E0F RID: 3599 RVA: 0x0003A520 File Offset: 0x00038720
	[Server]
	public PlayerResultData GetPlayerResult(PlayerProfile profile)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'PlayerResultData GameResultsManager::GetPlayerResult(PlayerProfile)' called when server was not active");
			return null;
		}
		PlayerResultData result;
		this.results.TryGetValue(profile, out result);
		return result;
	}

	// Token: 0x06000E10 RID: 3600 RVA: 0x0003A560 File Offset: 0x00038760
	[Server]
	private void UpdateDebugList(PlayerProfile profile, PlayerResultData data)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void GameResultsManager::UpdateDebugList(PlayerProfile,PlayerResultData)' called when server was not active");
			return;
		}
		PlayerResultDebugEntry playerResultDebugEntry = this.debugEntries.Find((PlayerResultDebugEntry e) => e.player == profile);
		if (playerResultDebugEntry == null)
		{
			playerResultDebugEntry = new PlayerResultDebugEntry();
			this.debugEntries.Add(playerResultDebugEntry);
		}
		playerResultDebugEntry.player = profile;
		playerResultDebugEntry.totalBets = data.totalBets;
		playerResultDebugEntry.totalPayouts = data.totalPayouts;
		playerResultDebugEntry.netProfit = data.NetProfit;
	}

	// Token: 0x06000E11 RID: 3601 RVA: 0x0003A5EC File Offset: 0x000387EC
	[Server]
	public void ClearResults()
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void GameResultsManager::ClearResults()' called when server was not active");
			return;
		}
		this.results.Clear();
		this.lastResults.Clear();
		this.debugEntries.Clear();
		Debug.Log("[GameResultsManager] Results cleared");
	}

	// Token: 0x06000E12 RID: 3602 RVA: 0x0003A63C File Offset: 0x0003883C
	[Server]
	public void RollbackResults(IEnumerable<PayoutRecord> records)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void GameResultsManager::RollbackResults(System.Collections.Generic.IEnumerable`1<PayoutRecord>)' called when server was not active");
			return;
		}
		if (records == null)
		{
			return;
		}
		foreach (PayoutRecord payoutRecord in records)
		{
			if (payoutRecord != null && !(payoutRecord.playerProfile == null))
			{
				PlayerResultData playerResultData;
				if (this.results.TryGetValue(payoutRecord.playerProfile, out playerResultData))
				{
					playerResultData.totalBets -= payoutRecord.bet;
					playerResultData.totalPayouts -= payoutRecord.payout;
					GameResultBreakdown gameResultBreakdown;
					if (playerResultData.ByGameType.TryGetValue(payoutRecord.gameType, out gameResultBreakdown))
					{
						gameResultBreakdown.totalBet -= payoutRecord.bet;
						gameResultBreakdown.totalPayout -= payoutRecord.payout;
					}
					this.UpdateDebugList(payoutRecord.playerProfile, playerResultData);
				}
				this.lastResults.Remove(payoutRecord.playerProfile);
			}
		}
	}

	// Token: 0x06000E14 RID: 3604 RVA: 0x00002321 File Offset: 0x00000521
	public override bool Weaved()
	{
		return true;
	}

	// Token: 0x040008E6 RID: 2278
	private readonly Dictionary<PlayerProfile, PlayerResultData> results = new Dictionary<PlayerProfile, PlayerResultData>();

	// Token: 0x040008E7 RID: 2279
	public readonly Dictionary<PlayerProfile, PlayerResultData> lastResults = new Dictionary<PlayerProfile, PlayerResultData>();

	// Token: 0x040008E9 RID: 2281
	[SerializeField]
	private List<PlayerResultDebugEntry> debugEntries = new List<PlayerResultDebugEntry>();
}
