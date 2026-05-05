using System;
using UnityEngine;

// Token: 0x0200013F RID: 319
[Serializable]
public class StatusConditionData : ChallengeConditionData
{
	// Token: 0x06000C79 RID: 3193 RVA: 0x00033ED4 File Offset: 0x000320D4
	public override bool Evaluate(ChallengeContext context)
	{
		bool flag = context.HadBuff(this.requiredBuff);
		if (!this.requireBuffActive)
		{
			return !flag;
		}
		return flag;
	}

	// Token: 0x06000C7A RID: 3194 RVA: 0x00033EFC File Offset: 0x000320FC
	public override float GetProgress(ChallengeContext context)
	{
		bool flag = context.HadBuff(this.requiredBuff);
		if (!(this.requireBuffActive ? flag : (!flag)))
		{
			return 0f;
		}
		return 1f;
	}

	// Token: 0x06000C7B RID: 3195 RVA: 0x00033F34 File Offset: 0x00032134
	public override string GetProgressText(ChallengeContext context)
	{
		bool flag = context.HadBuff(this.requiredBuff);
		string buffName = this.GetBuffName(this.requiredBuff);
		if (this.requireBuffActive)
		{
			if (flag)
			{
				return buffName + " active (✓)";
			}
			return buffName + " required";
		}
		else
		{
			if (!flag)
			{
				return "No " + buffName + " (✓)";
			}
			return buffName + " not allowed";
		}
	}

	// Token: 0x06000C7C RID: 3196 RVA: 0x00033FA0 File Offset: 0x000321A0
	private string GetBuffName(PlayerBuffType buffType)
	{
		string result;
		switch (buffType)
		{
		case PlayerBuffType.TipsyFortune:
			result = "Drink buff";
			break;
		case PlayerBuffType.InspiringMelody:
			result = "Melody buff";
			break;
		case PlayerBuffType.Immunity:
			result = "Immunity buff";
			break;
		default:
			result = "Buff";
			break;
		}
		return result;
	}

	// Token: 0x040007D5 RID: 2005
	[Header("Status Settings")]
	[Tooltip("The buff type to check for")]
	public PlayerBuffType requiredBuff;

	// Token: 0x040007D6 RID: 2006
	[Tooltip("Whether the buff must be active (true) or must not be active (false)")]
	public bool requireBuffActive = true;
}
