using System;
using System.Collections.Generic;

// Token: 0x02000170 RID: 368
[Serializable]
public class PlayerResultData
{
	// Token: 0x17000141 RID: 321
	// (get) Token: 0x06000E17 RID: 3607 RVA: 0x0003A784 File Offset: 0x00038984
	public long NetProfit
	{
		get
		{
			return this.totalPayouts - this.totalBets;
		}
	}

	// Token: 0x040008EB RID: 2283
	public long totalBets;

	// Token: 0x040008EC RID: 2284
	public long totalPayouts;

	// Token: 0x040008ED RID: 2285
	public Dictionary<CasinoGameType, GameResultBreakdown> ByGameType = new Dictionary<CasinoGameType, GameResultBreakdown>();
}
