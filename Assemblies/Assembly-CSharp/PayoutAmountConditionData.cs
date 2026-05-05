using System;
using Extensions;
using UnityEngine;

// Token: 0x0200013B RID: 315
[Serializable]
public class PayoutAmountConditionData : ChallengeConditionData
{
	// Token: 0x06000C60 RID: 3168 RVA: 0x0003363C File Offset: 0x0003183C
	public override bool Evaluate(ChallengeContext context)
	{
		if (this.checkTotalPayout)
		{
			ChallengeManager instance = NetworkSingleton<ChallengeManager>.Instance;
			ConditionState conditionState = (instance != null) ? instance.GetConditionState(this) : null;
			return conditionState != null && conditionState.totalPayoutAmount >= (long)this.minPayoutAmount && (this.maxPayoutAmount == 0 || conditionState.totalPayoutAmount <= (long)this.maxPayoutAmount);
		}
		return context.payout >= (long)this.minPayoutAmount && (this.maxPayoutAmount == 0 || context.payout <= (long)this.maxPayoutAmount);
	}

	// Token: 0x06000C61 RID: 3169 RVA: 0x000336C4 File Offset: 0x000318C4
	public override float GetProgress(ChallengeContext context)
	{
		if (!this.checkTotalPayout)
		{
			return Mathf.Clamp01((float)context.payout / (float)this.minPayoutAmount);
		}
		ChallengeManager instance = NetworkSingleton<ChallengeManager>.Instance;
		ConditionState conditionState = (instance != null) ? instance.GetConditionState(this) : null;
		if (conditionState == null)
		{
			return 0f;
		}
		return Mathf.Clamp01((float)conditionState.totalPayoutAmount / (float)this.minPayoutAmount);
	}

	// Token: 0x06000C62 RID: 3170 RVA: 0x00033720 File Offset: 0x00031920
	public override string GetProgressText(ChallengeContext context)
	{
		if (this.checkTotalPayout)
		{
			ChallengeManager instance = NetworkSingleton<ChallengeManager>.Instance;
			ConditionState conditionState = (instance != null) ? instance.GetConditionState(this) : null;
			if (conditionState == null)
			{
				return string.Format("Payout 0/{0} total", this.minPayoutAmount);
			}
			return string.Format("Payout {0}/{1} total", conditionState.totalPayoutAmount, this.minPayoutAmount);
		}
		else
		{
			if (context.payout >= (long)this.minPayoutAmount)
			{
				return string.Format("Payout {0} (✓)", context.payout);
			}
			return string.Format("Payout {0}/{1}", context.payout, this.minPayoutAmount);
		}
	}

	// Token: 0x06000C63 RID: 3171 RVA: 0x000337C8 File Offset: 0x000319C8
	public override void OnGameResult(long bet, long payout, CasinoGameType gameType, Vector3 position)
	{
		if (this.checkTotalPayout)
		{
			ChallengeManager instance = NetworkSingleton<ChallengeManager>.Instance;
			ConditionState conditionState = (instance != null) ? instance.GetConditionState(this) : null;
			if (conditionState != null)
			{
				conditionState.totalPayoutAmount += payout;
			}
		}
	}

	// Token: 0x06000C64 RID: 3172 RVA: 0x00033804 File Offset: 0x00031A04
	public override void ResetCondition()
	{
		ChallengeManager instance = NetworkSingleton<ChallengeManager>.Instance;
		ConditionState conditionState = (instance != null) ? instance.GetConditionState(this) : null;
		if (conditionState != null)
		{
			conditionState.totalPayoutAmount = 0L;
		}
	}

	// Token: 0x040007CB RID: 1995
	[Header("Payout Amount Settings")]
	[Tooltip("The minimum payout amount required")]
	public int minPayoutAmount = 1000;

	// Token: 0x040007CC RID: 1996
	[Tooltip("The maximum payout amount (0 = no maximum)")]
	public int maxPayoutAmount;

	// Token: 0x040007CD RID: 1997
	[Tooltip("Whether to check total payout amount or single payout")]
	public bool checkTotalPayout;
}
