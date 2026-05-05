using System;
using Extensions;
using UnityEngine;

// Token: 0x02000132 RID: 306
[Serializable]
public class ChallengeProgress
{
	// Token: 0x06000C32 RID: 3122 RVA: 0x000327AC File Offset: 0x000309AC
	public ChallengeProgress(Challenge challenge)
	{
		this.challenge = challenge;
		this.isCompleted = false;
		this.isClaimed = false;
		this.progress = 0f;
		this.progressText = "";
		this.startTime = Time.time;
		this.completionCount = 0;
		this.lastBet = 0L;
		this.lastPayout = 0L;
		this.lastGameType = CasinoGameType.Blackjack;
		this.quotaAtActivation = ((NetworkSingleton<GameManager>.Instance != null) ? NetworkSingleton<GameManager>.Instance.currentQuota : 0L);
	}

	// Token: 0x06000C33 RID: 3123 RVA: 0x00032834 File Offset: 0x00030A34
	public void UpdateProgress(ChallengeContext context)
	{
		if (this.challenge == null)
		{
			return;
		}
		if (this.isCompleted && !this.challenge.repeatable)
		{
			return;
		}
		this.lastBet = context.bet;
		this.lastPayout = context.payout;
		this.lastGameType = context.gameType;
		context.quotaAtActivation = this.quotaAtActivation;
		this.progress = this.challenge.GetProgress(context);
		this.progressText = this.challenge.GetProgressText(context);
		if (this.challenge.IsCompleted(context) && !this.isCompleted)
		{
			this.isCompleted = true;
			this.completionCount++;
		}
	}

	// Token: 0x06000C34 RID: 3124 RVA: 0x000328E5 File Offset: 0x00030AE5
	public void Reset()
	{
		this.isCompleted = false;
		this.isClaimed = false;
		this.progress = 0f;
		this.progressText = "";
		this.startTime = Time.time;
	}

	// Token: 0x040007A6 RID: 1958
	public Challenge challenge;

	// Token: 0x040007A7 RID: 1959
	public bool isCompleted;

	// Token: 0x040007A8 RID: 1960
	public bool isClaimed;

	// Token: 0x040007A9 RID: 1961
	public float progress;

	// Token: 0x040007AA RID: 1962
	public string progressText;

	// Token: 0x040007AB RID: 1963
	public float startTime;

	// Token: 0x040007AC RID: 1964
	public int completionCount;

	// Token: 0x040007AD RID: 1965
	public long lastBet;

	// Token: 0x040007AE RID: 1966
	public long lastPayout;

	// Token: 0x040007AF RID: 1967
	public CasinoGameType lastGameType;

	// Token: 0x040007B0 RID: 1968
	public long quotaAtActivation;
}
