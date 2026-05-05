using System;
using UnityEngine;

// Token: 0x0200019A RID: 410
[Serializable]
public class PayoutRecord
{
	// Token: 0x0400099E RID: 2462
	public float timestamp;

	// Token: 0x0400099F RID: 2463
	public string playerName;

	// Token: 0x040009A0 RID: 2464
	public PlayerProfile playerProfile;

	// Token: 0x040009A1 RID: 2465
	public long bet;

	// Token: 0x040009A2 RID: 2466
	public long payout;

	// Token: 0x040009A3 RID: 2467
	public long profit;

	// Token: 0x040009A4 RID: 2468
	public bool isWin;

	// Token: 0x040009A5 RID: 2469
	public bool isLoss;

	// Token: 0x040009A6 RID: 2470
	public CasinoGameType gameType;

	// Token: 0x040009A7 RID: 2471
	public Vector3 gamePosition;
}
