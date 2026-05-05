using System;
using System.Collections.Generic;

// Token: 0x02000133 RID: 307
public class ConditionStateTracker
{
	// Token: 0x06000C35 RID: 3125 RVA: 0x00032916 File Offset: 0x00030B16
	public ConditionState GetOrCreateState(ChallengeConditionData condition)
	{
		if (condition == null)
		{
			return null;
		}
		if (!this.conditionStates.ContainsKey(condition))
		{
			this.conditionStates[condition] = new ConditionState();
		}
		return this.conditionStates[condition];
	}

	// Token: 0x06000C36 RID: 3126 RVA: 0x00032948 File Offset: 0x00030B48
	public void ResetAll()
	{
		foreach (ConditionState conditionState in this.conditionStates.Values)
		{
			if (conditionState != null)
			{
				conditionState.Reset();
			}
		}
	}

	// Token: 0x06000C37 RID: 3127 RVA: 0x000329A4 File Offset: 0x00030BA4
	public void ResetCondition(ChallengeConditionData condition)
	{
		if (condition == null)
		{
			return;
		}
		if (this.conditionStates.ContainsKey(condition))
		{
			ConditionState conditionState = this.conditionStates[condition];
			if (conditionState == null)
			{
				return;
			}
			conditionState.Reset();
		}
	}

	// Token: 0x040007B1 RID: 1969
	private Dictionary<ChallengeConditionData, ConditionState> conditionStates = new Dictionary<ChallengeConditionData, ConditionState>();
}
