using System;
using System.Collections.Generic;

// Token: 0x020001B4 RID: 436
[Serializable]
public class PlayerUpgradeData
{
	// Token: 0x06000FD6 RID: 4054 RVA: 0x000438A4 File Offset: 0x00041AA4
	public PlayerUpgradeData()
	{
		this.Upgrades[PlayerUpgradeType.GamblersConfidence] = 1f;
		this.Upgrades[PlayerUpgradeType.Insurance] = 0f;
		this.Upgrades[PlayerUpgradeType.Stakeholder] = 1f;
		this.Upgrades[PlayerUpgradeType.BonusDraw] = 0f;
	}

	// Token: 0x04000A3F RID: 2623
	public Dictionary<PlayerUpgradeType, float> Upgrades = new Dictionary<PlayerUpgradeType, float>();
}
