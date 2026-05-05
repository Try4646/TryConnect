using System;
using System.Collections.Generic;
using Extensions;

// Token: 0x0200025A RID: 602
public class DaySummaryRuntime : MonoSingleton<DaySummaryRuntime>
{
	// Token: 0x170001F0 RID: 496
	// (get) Token: 0x0600157F RID: 5503 RVA: 0x0005C566 File Offset: 0x0005A766
	public IReadOnlyList<DaySummaryRuntime.ChallengeReward> CompletedChallenges
	{
		get
		{
			return this._completedChallenges;
		}
	}

	// Token: 0x06001580 RID: 5504 RVA: 0x0005C56E File Offset: 0x0005A76E
	public void Clear()
	{
		this._completedChallenges.Clear();
	}

	// Token: 0x06001581 RID: 5505 RVA: 0x0005C57C File Offset: 0x0005A77C
	public void Add(string challengeName, int tickets)
	{
		if (tickets <= 0)
		{
			return;
		}
		this._completedChallenges.Add(new DaySummaryRuntime.ChallengeReward
		{
			challengeName = challengeName,
			tickets = tickets
		});
	}

	// Token: 0x04000DCD RID: 3533
	private readonly List<DaySummaryRuntime.ChallengeReward> _completedChallenges = new List<DaySummaryRuntime.ChallengeReward>();

	// Token: 0x0200025B RID: 603
	[Serializable]
	public struct ChallengeReward
	{
		// Token: 0x04000DCE RID: 3534
		public string challengeName;

		// Token: 0x04000DCF RID: 3535
		public int tickets;
	}
}
