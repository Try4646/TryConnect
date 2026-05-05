using System;

// Token: 0x02000189 RID: 393
[Serializable]
public class BalanceChangeData
{
	// Token: 0x06000EC9 RID: 3785 RVA: 0x0003D5A9 File Offset: 0x0003B7A9
	public BalanceChangeData(long changeAmount, PlayerProfile changer, ChangeType changeType)
	{
		this.changeAmount = changeAmount;
		this.changer = changer;
		this.changeType = changeType;
	}

	// Token: 0x0400096F RID: 2415
	public long changeAmount;

	// Token: 0x04000970 RID: 2416
	public PlayerProfile changer;

	// Token: 0x04000971 RID: 2417
	public ChangeType changeType;
}
