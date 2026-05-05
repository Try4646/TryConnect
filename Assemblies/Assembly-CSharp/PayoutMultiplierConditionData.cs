using System;
using UnityEngine;

// Token: 0x0200013C RID: 316
[Serializable]
public class PayoutMultiplierConditionData : ChallengeConditionData
{
	// Token: 0x06000C66 RID: 3174 RVA: 0x00033844 File Offset: 0x00031A44
	public override bool Evaluate(ChallengeContext context)
	{
		if (context.bet <= 0L)
		{
			return false;
		}
		float num = (float)context.payout / (float)context.bet;
		bool flag = num >= this.minPayoutMultiplier;
		bool flag2 = this.maxPayoutMultiplier == 0f || num <= this.maxPayoutMultiplier;
		return flag && flag2;
	}

	// Token: 0x06000C67 RID: 3175 RVA: 0x00033898 File Offset: 0x00031A98
	public override float GetProgress(ChallengeContext context)
	{
		if (context.bet <= 0L)
		{
			return 0f;
		}
		return Mathf.Clamp01((float)context.payout / (float)context.bet / this.minPayoutMultiplier);
	}

	// Token: 0x06000C68 RID: 3176 RVA: 0x000338C8 File Offset: 0x00031AC8
	public override string GetProgressText(ChallengeContext context)
	{
		if (context.bet <= 0L)
		{
			return string.Format("{0:F1}x multiplier", this.minPayoutMultiplier);
		}
		float num = (float)context.payout / (float)context.bet;
		if (num >= this.minPayoutMultiplier && (this.maxPayoutMultiplier == 0f || num <= this.maxPayoutMultiplier))
		{
			return string.Format("{0:F1}x (✓)", num);
		}
		return string.Format("{0:F1}x / {1:F1}x", num, this.minPayoutMultiplier);
	}

	// Token: 0x06000C69 RID: 3177 RVA: 0x000048A7 File Offset: 0x00002AA7
	public override void OnGameResult(long bet, long payout, CasinoGameType gameType, Vector3 position)
	{
	}

	// Token: 0x06000C6A RID: 3178 RVA: 0x000048A7 File Offset: 0x00002AA7
	public override void ResetCondition()
	{
	}

	// Token: 0x040007CE RID: 1998
	[Header("Payout Multiplier Settings")]
	[Tooltip("The minimum payout multiplier required")]
	public float minPayoutMultiplier = 1f;

	// Token: 0x040007CF RID: 1999
	[Tooltip("The maximum payout multiplier (0 = no maximum)")]
	public float maxPayoutMultiplier;
}
