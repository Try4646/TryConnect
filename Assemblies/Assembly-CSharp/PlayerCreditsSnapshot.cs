using System;
using System.Collections.Generic;

// Token: 0x0200022B RID: 555
[Serializable]
public class PlayerCreditsSnapshot
{
	// Token: 0x04000CD2 RID: 3282
	public ulong steamId;

	// Token: 0x04000CD3 RID: 3283
	public string displayName;

	// Token: 0x04000CD4 RID: 3284
	public List<PlayerCreditsSnapshot.CosmeticEntry> cosmetics = new List<PlayerCreditsSnapshot.CosmeticEntry>();

	// Token: 0x0200022C RID: 556
	[Serializable]
	public struct CosmeticEntry
	{
		// Token: 0x04000CD5 RID: 3285
		public CosmeticType type;

		// Token: 0x04000CD6 RID: 3286
		public int cosmeticId;
	}
}
