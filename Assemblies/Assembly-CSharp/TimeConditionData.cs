using System;
using Extensions;
using UnityEngine;

// Token: 0x02000140 RID: 320
[Serializable]
public class TimeConditionData : ChallengeConditionData
{
	// Token: 0x06000C7E RID: 3198 RVA: 0x00033FF0 File Offset: 0x000321F0
	public override bool Evaluate(ChallengeContext context)
	{
		ChallengeManager instance = NetworkSingleton<ChallengeManager>.Instance;
		ConditionState conditionState = (instance != null) ? instance.GetConditionState(this) : null;
		return conditionState != null && (this.checkSinceStart ? (Time.time - conditionState.startTime) : (Time.time - conditionState.lastGameTime)) <= this.timeLimit;
	}

	// Token: 0x06000C7F RID: 3199 RVA: 0x00034044 File Offset: 0x00032244
	public override float GetProgress(ChallengeContext context)
	{
		ChallengeManager instance = NetworkSingleton<ChallengeManager>.Instance;
		ConditionState conditionState = (instance != null) ? instance.GetConditionState(this) : null;
		if (conditionState == null)
		{
			return 0f;
		}
		float num = this.checkSinceStart ? (Time.time - conditionState.startTime) : (Time.time - conditionState.lastGameTime);
		return Mathf.Clamp01(1f - num / this.timeLimit);
	}

	// Token: 0x06000C80 RID: 3200 RVA: 0x000340A4 File Offset: 0x000322A4
	public override string GetProgressText(ChallengeContext context)
	{
		ChallengeManager instance = NetworkSingleton<ChallengeManager>.Instance;
		ConditionState conditionState = (instance != null) ? instance.GetConditionState(this) : null;
		if (conditionState == null)
		{
			return "Time limit expired";
		}
		float num = this.checkSinceStart ? (Time.time - conditionState.startTime) : (Time.time - conditionState.lastGameTime);
		float num2 = Mathf.Max(0f, this.timeLimit - num);
		return string.Format("{0:F1}s remaining", num2);
	}

	// Token: 0x06000C81 RID: 3201 RVA: 0x00034114 File Offset: 0x00032314
	public override void OnGameResult(long bet, long payout, CasinoGameType gameType, Vector3 position)
	{
		if (!this.checkSinceStart)
		{
			ChallengeManager instance = NetworkSingleton<ChallengeManager>.Instance;
			ConditionState conditionState = (instance != null) ? instance.GetConditionState(this) : null;
			if (conditionState != null)
			{
				conditionState.lastGameTime = Time.time;
			}
		}
	}

	// Token: 0x06000C82 RID: 3202 RVA: 0x0003414C File Offset: 0x0003234C
	public override void ResetCondition()
	{
		ChallengeManager instance = NetworkSingleton<ChallengeManager>.Instance;
		ConditionState conditionState = (instance != null) ? instance.GetConditionState(this) : null;
		if (conditionState != null)
		{
			conditionState.startTime = Time.time;
			conditionState.lastGameTime = Time.time;
		}
	}

	// Token: 0x040007D7 RID: 2007
	[Header("Time Settings")]
	[Tooltip("The time limit in seconds")]
	public float timeLimit = 60f;

	// Token: 0x040007D8 RID: 2008
	[Tooltip("Whether to check time since challenge started or time since last game")]
	public bool checkSinceStart = true;
}
