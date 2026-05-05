using System;
using UnityEngine;

// Token: 0x02000125 RID: 293
[Serializable]
public abstract class ChallengeConditionData
{
	// Token: 0x06000BD6 RID: 3030
	public abstract bool Evaluate(ChallengeContext context);

	// Token: 0x06000BD7 RID: 3031
	public abstract float GetProgress(ChallengeContext context);

	// Token: 0x06000BD8 RID: 3032
	public abstract string GetProgressText(ChallengeContext context);

	// Token: 0x06000BD9 RID: 3033 RVA: 0x000048A7 File Offset: 0x00002AA7
	public virtual void ResetCondition()
	{
	}

	// Token: 0x06000BDA RID: 3034 RVA: 0x000048A7 File Offset: 0x00002AA7
	public virtual void OnGameResult(long bet, long payout, CasinoGameType gameType, Vector3 position)
	{
	}

	// Token: 0x04000765 RID: 1893
	[Header("Condition Settings")]
	[Tooltip("Description of what this condition checks")]
	[TextArea(2, 4)]
	public string description;

	// Token: 0x04000766 RID: 1894
	[Tooltip("When enabled, this condition filters which results count toward other conditions.")]
	public bool useAsResultFilter;
}
