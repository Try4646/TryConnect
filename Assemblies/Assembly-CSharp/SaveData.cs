using System;
using System.Collections.Generic;

// Token: 0x020001A6 RID: 422
[Serializable]
public class SaveData
{
	// Token: 0x04000A03 RID: 2563
	public string saveName;

	// Token: 0x04000A04 RID: 2564
	public long saveTime;

	// Token: 0x04000A05 RID: 2565
	public int successfulQuota;

	// Token: 0x04000A06 RID: 2566
	public int daysLeft;

	// Token: 0x04000A07 RID: 2567
	public int daysPassed;

	// Token: 0x04000A08 RID: 2568
	public long currentQuota;

	// Token: 0x04000A09 RID: 2569
	public int currentFloor;

	// Token: 0x04000A0A RID: 2570
	public long requiredQuotaToNextFloor;

	// Token: 0x04000A0B RID: 2571
	public long money;

	// Token: 0x04000A0C RID: 2572
	public long tickets;

	// Token: 0x04000A0D RID: 2573
	public List<int> itemIds = new List<int>();

	// Token: 0x04000A0E RID: 2574
	public List<int> challengeIds = new List<int>();

	// Token: 0x04000A0F RID: 2575
	public List<ChallengeProgressSaveData> challengeProgress = new List<ChallengeProgressSaveData>();

	// Token: 0x04000A10 RID: 2576
	public List<PlayerOrganSaveData> playerOrganStates = new List<PlayerOrganSaveData>();

	// Token: 0x04000A11 RID: 2577
	public List<PlayerUpgradeSaveData> playerUpgradeStates = new List<PlayerUpgradeSaveData>();

	// Token: 0x04000A12 RID: 2578
	public List<ProfitHistorySaveData> profitHistory = new List<ProfitHistorySaveData>();

	// Token: 0x04000A13 RID: 2579
	public long payoutTotalWins;

	// Token: 0x04000A14 RID: 2580
	public long payoutTotalLosses;

	// Token: 0x04000A15 RID: 2581
	public int seed;
}
