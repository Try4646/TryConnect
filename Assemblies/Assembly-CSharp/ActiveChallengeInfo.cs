using System;
using UnityEngine;

// Token: 0x02000310 RID: 784
[Serializable]
public class ActiveChallengeInfo
{
	// Token: 0x06001AA4 RID: 6820 RVA: 0x00070EAF File Offset: 0x0006F0AF
	public ActiveChallengeInfo(Challenge challenge, string playerName, float progress, string progressText, bool isCompleted, bool isClaimed)
	{
		this.challenge = challenge;
		this.playerName = playerName;
		this.progress = progress;
		this.progressText = progressText;
		this.isCompleted = isCompleted;
		this.isClaimed = isClaimed;
	}

	// Token: 0x0400115A RID: 4442
	[Tooltip("The challenge being worked on")]
	public Challenge challenge;

	// Token: 0x0400115B RID: 4443
	[Tooltip("Player name working on this challenge")]
	public string playerName;

	// Token: 0x0400115C RID: 4444
	[Tooltip("Current progress (0-1)")]
	public float progress;

	// Token: 0x0400115D RID: 4445
	[Tooltip("Progress text description")]
	public string progressText;

	// Token: 0x0400115E RID: 4446
	[Tooltip("Whether the challenge is completed")]
	public bool isCompleted;

	// Token: 0x0400115F RID: 4447
	[Tooltip("Whether the reward has been claimed")]
	public bool isClaimed;
}
