using System;
using UnityEngine;

// Token: 0x02000139 RID: 313
[Serializable]
public class GameTypeConditionData : ChallengeConditionData
{
	// Token: 0x06000C56 RID: 3158 RVA: 0x000333E9 File Offset: 0x000315E9
	public override bool Evaluate(ChallengeContext context)
	{
		return context.gameType == this.requiredGameType;
	}

	// Token: 0x06000C57 RID: 3159 RVA: 0x000333F9 File Offset: 0x000315F9
	public override float GetProgress(ChallengeContext context)
	{
		if (!this.Evaluate(context))
		{
			return 0f;
		}
		return 1f;
	}

	// Token: 0x06000C58 RID: 3160 RVA: 0x00033410 File Offset: 0x00031610
	public override string GetProgressText(ChallengeContext context)
	{
		if (context.gameType != this.requiredGameType)
		{
			return "Not playing " + this.requiredGameType.ToString();
		}
		return "Playing " + this.requiredGameType.ToString();
	}

	// Token: 0x040007C6 RID: 1990
	[Header("Game Type Settings")]
	[Tooltip("The casino game type this condition checks for")]
	public CasinoGameType requiredGameType;
}
