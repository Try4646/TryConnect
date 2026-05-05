using System;
using System.Collections.Generic;

// Token: 0x020001A7 RID: 423
[Serializable]
public class ChallengeProgressSaveData
{
	// Token: 0x04000A16 RID: 2582
	public int challengeID;

	// Token: 0x04000A17 RID: 2583
	public float progress;

	// Token: 0x04000A18 RID: 2584
	public bool isCompleted;

	// Token: 0x04000A19 RID: 2585
	public bool isClaimed;

	// Token: 0x04000A1A RID: 2586
	public int completionCount;

	// Token: 0x04000A1B RID: 2587
	public long lastBet;

	// Token: 0x04000A1C RID: 2588
	public long lastPayout;

	// Token: 0x04000A1D RID: 2589
	public CasinoGameType lastGameType;

	// Token: 0x04000A1E RID: 2590
	public long quotaAtActivation;

	// Token: 0x04000A1F RID: 2591
	public List<ConditionStateSyncData> conditionStates = new List<ConditionStateSyncData>();
}
