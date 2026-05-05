using System;
using System.Collections.Generic;
using Extensions;
using UnityEngine;

// Token: 0x020001B6 RID: 438
public class UpgradeUI : MonoSingleton<UpgradeUI>
{
	// Token: 0x06000FD7 RID: 4055 RVA: 0x00043908 File Offset: 0x00041B08
	public void UpdateUpgradeUI(PlayerUpgradeType type, float value, float change)
	{
		float num = new PlayerUpgradeData().Upgrades[type];
		UpgradeEntryUI upgradeEntryUI;
		if (this._upgradeEntries.TryGetValue(type, out upgradeEntryUI))
		{
			if (value == num)
			{
				Object.Destroy(upgradeEntryUI.gameObject);
				this._upgradeEntries.Remove(type);
				return;
			}
			upgradeEntryUI.SetUpgradeEntry(type, value, change);
			return;
		}
		else
		{
			if (value == num)
			{
				return;
			}
			UpgradeEntryUI upgradeEntryUI2 = Object.Instantiate<UpgradeEntryUI>(this.upgradeEntryUI, this.entryParent);
			this._upgradeEntries.Add(type, upgradeEntryUI2);
			upgradeEntryUI2.SetUpgradeEntry(type, value, change);
			return;
		}
	}

	// Token: 0x06000FD8 RID: 4056 RVA: 0x0004398C File Offset: 0x00041B8C
	public void ClearUpgradeUI()
	{
		foreach (KeyValuePair<PlayerUpgradeType, UpgradeEntryUI> keyValuePair in this._upgradeEntries)
		{
			Object.Destroy(keyValuePair.Value.gameObject);
		}
		this._upgradeEntries.Clear();
	}

	// Token: 0x04000A45 RID: 2629
	[SerializeField]
	private Transform entryParent;

	// Token: 0x04000A46 RID: 2630
	[SerializeField]
	private UpgradeEntryUI upgradeEntryUI;

	// Token: 0x04000A47 RID: 2631
	private Dictionary<PlayerUpgradeType, UpgradeEntryUI> _upgradeEntries = new Dictionary<PlayerUpgradeType, UpgradeEntryUI>();
}
