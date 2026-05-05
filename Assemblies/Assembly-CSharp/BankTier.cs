using System;
using UnityEngine;

// Token: 0x0200005D RID: 93
[CreateAssetMenu(fileName = "BankTier", menuName = "Bank/Bank Tier")]
public class BankTier : ScriptableObject
{
	// Token: 0x0600031D RID: 797 RVA: 0x0000F7EC File Offset: 0x0000D9EC
	public float GetRandomModification()
	{
		float minInclusive = 1f - this.maxLossPercent;
		float maxInclusive = 1f + this.maxGainPercent;
		return Random.Range(minInclusive, maxInclusive);
	}

	// Token: 0x0600031E RID: 798 RVA: 0x0000F818 File Offset: 0x0000DA18
	public string GetFluctuationDisplay()
	{
		return string.Format("+{0:F0}% / -{1:F0}%", this.maxGainPercent * 100f, this.maxLossPercent * 100f);
	}

	// Token: 0x0400024E RID: 590
	[Header("Tier Settings")]
	[Tooltip("The tier number (1-4)")]
	public int tierNumber = 1;

	// Token: 0x0400024F RID: 591
	[Header("Market Fluctuation")]
	[Tooltip("Maximum positive percentage change (e.g., 0.25 = 25% gain, 0.90 = 90% gain)")]
	[Range(0f, 1f)]
	public float maxGainPercent = 0.25f;

	// Token: 0x04000250 RID: 592
	[Tooltip("Maximum negative percentage change (e.g., 0.25 = 25% loss, 1.0 = 100% loss)")]
	[Range(0f, 1f)]
	public float maxLossPercent = 0.25f;

	// Token: 0x04000251 RID: 593
	[Header("Time Settings")]
	[Tooltip("Time interval between market modifications in seconds (60 = 1 minute)")]
	public float modificationInterval = 60f;

	// Token: 0x04000252 RID: 594
	[Header("Deposit Limits")]
	[Tooltip("Minimum amount that can be deposited in this tier")]
	public long minDepositAmount = 1L;

	// Token: 0x04000253 RID: 595
	[Tooltip("Maximum amount that can be deposited in this tier")]
	public long maxDepositAmount = 10000L;
}
