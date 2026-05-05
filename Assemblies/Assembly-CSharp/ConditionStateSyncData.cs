using System;

// Token: 0x02000131 RID: 305
[Serializable]
public struct ConditionStateSyncData
{
	// Token: 0x0400079D RID: 1949
	public int currentWinCount;

	// Token: 0x0400079E RID: 1950
	public int consecutiveWinCount;

	// Token: 0x0400079F RID: 1951
	public int currentLossCount;

	// Token: 0x040007A0 RID: 1952
	public int consecutiveLossCount;

	// Token: 0x040007A1 RID: 1953
	public long totalBetAmount;

	// Token: 0x040007A2 RID: 1954
	public long totalPayoutAmount;

	// Token: 0x040007A3 RID: 1955
	public long totalProfit;

	// Token: 0x040007A4 RID: 1956
	public float elapsedSinceStart;

	// Token: 0x040007A5 RID: 1957
	public float elapsedSinceLastGame;
}
