using System;

// Token: 0x02000130 RID: 304
[Serializable]
public struct ChallengeSyncData
{
	// Token: 0x04000794 RID: 1940
	public int challengeID;

	// Token: 0x04000795 RID: 1941
	public float progress;

	// Token: 0x04000796 RID: 1942
	public bool isCompleted;

	// Token: 0x04000797 RID: 1943
	public bool isClaimed;

	// Token: 0x04000798 RID: 1944
	public int completionCount;

	// Token: 0x04000799 RID: 1945
	public long lastBet;

	// Token: 0x0400079A RID: 1946
	public long lastPayout;

	// Token: 0x0400079B RID: 1947
	public CasinoGameType lastGameType;

	// Token: 0x0400079C RID: 1948
	public ConditionStateSyncData[] conditionStates;
}
