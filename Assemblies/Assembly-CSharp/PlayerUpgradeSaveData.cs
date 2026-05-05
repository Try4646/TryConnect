using System;
using System.Collections.Generic;

// Token: 0x020001A9 RID: 425
[Serializable]
public class PlayerUpgradeSaveData
{
	// Token: 0x04000A25 RID: 2597
	public string steamId;

	// Token: 0x04000A26 RID: 2598
	public List<PlayerUpgradeValue> upgrades = new List<PlayerUpgradeValue>();
}
