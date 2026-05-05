using System;
using System.Collections.Generic;
using Extensions;
using UnityEngine;

// Token: 0x020002ED RID: 749
public class SteamAchievementsManager : NetworkSingleton<SteamAchievementsManager>
{
	// Token: 0x060019FB RID: 6651 RVA: 0x0006D324 File Offset: 0x0006B524
	private void OnEnable()
	{
		MoneyManager instance = NetworkSingleton<MoneyManager>.Instance;
		instance.OnBalanceChanged = (Action<BalanceChangeData>)Delegate.Combine(instance.OnBalanceChanged, new Action<BalanceChangeData>(this.CheckBalanceChangeAchievements));
	}

	// Token: 0x060019FC RID: 6652 RVA: 0x0006D34C File Offset: 0x0006B54C
	private void OnDisable()
	{
		MoneyManager instance = NetworkSingleton<MoneyManager>.Instance;
		instance.OnBalanceChanged = (Action<BalanceChangeData>)Delegate.Remove(instance.OnBalanceChanged, new Action<BalanceChangeData>(this.CheckBalanceChangeAchievements));
	}

	// Token: 0x060019FD RID: 6653 RVA: 0x0006D374 File Offset: 0x0006B574
	private void Start()
	{
		if (this.unlockStatus.Count != this.moneyAchievements.Count)
		{
			this.unlockStatus = new List<bool>(new bool[this.moneyAchievements.Count]);
		}
		this.CheckAndUpdateUnlockStatus();
	}

	// Token: 0x060019FE RID: 6654 RVA: 0x0006D3B0 File Offset: 0x0006B5B0
	private void CheckAndUpdateUnlockStatus()
	{
		foreach (SteamAchievement_SteamworksNET steamAchievement_SteamworksNET in this.moneyAchievements)
		{
			if (steamAchievement_SteamworksNET.CheckAchievementState())
			{
				int num = this.moneyAchievements.IndexOf(steamAchievement_SteamworksNET);
				if (num >= 0 && num < this.unlockStatus.Count)
				{
					this.unlockStatus[num] = true;
				}
			}
		}
	}

	// Token: 0x060019FF RID: 6655 RVA: 0x0006D430 File Offset: 0x0006B630
	private void CheckBalanceChangeAchievements(BalanceChangeData data)
	{
		if (data.changeType == ChangeType.Save)
		{
			return;
		}
		foreach (SteamAchievement_SteamworksNET steamAchievement_SteamworksNET in this.moneyAchievements)
		{
			if (steamAchievement_SteamworksNET.Value == "ACH_MONEY_10M" && NetworkSingleton<MoneyManager>.Instance.balance >= 10000000L && !this.unlockStatus[this.moneyAchievements.IndexOf(steamAchievement_SteamworksNET)])
			{
				steamAchievement_SteamworksNET.UnlockAchievement();
				this.unlockStatus[this.moneyAchievements.IndexOf(steamAchievement_SteamworksNET)] = true;
			}
			else if (steamAchievement_SteamworksNET.Value == "ACH_MONEY_100M" && NetworkSingleton<MoneyManager>.Instance.balance >= 100000000L && !this.unlockStatus[this.moneyAchievements.IndexOf(steamAchievement_SteamworksNET)])
			{
				steamAchievement_SteamworksNET.UnlockAchievement();
				this.unlockStatus[this.moneyAchievements.IndexOf(steamAchievement_SteamworksNET)] = true;
			}
			else if (steamAchievement_SteamworksNET.Value == "ACH_MONEY_1B" && NetworkSingleton<MoneyManager>.Instance.balance >= 1000000000L && !this.unlockStatus[this.moneyAchievements.IndexOf(steamAchievement_SteamworksNET)])
			{
				steamAchievement_SteamworksNET.UnlockAchievement();
				this.unlockStatus[this.moneyAchievements.IndexOf(steamAchievement_SteamworksNET)] = true;
			}
			else if (steamAchievement_SteamworksNET.Value == "ACH_MONEY_10B" && NetworkSingleton<MoneyManager>.Instance.balance >= 10000000000L && !this.unlockStatus[this.moneyAchievements.IndexOf(steamAchievement_SteamworksNET)])
			{
				steamAchievement_SteamworksNET.UnlockAchievement();
				this.unlockStatus[this.moneyAchievements.IndexOf(steamAchievement_SteamworksNET)] = true;
			}
			else if (steamAchievement_SteamworksNET.Value == "ACH_MONEY_100B" && NetworkSingleton<MoneyManager>.Instance.balance >= 100000000000L && !this.unlockStatus[this.moneyAchievements.IndexOf(steamAchievement_SteamworksNET)])
			{
				steamAchievement_SteamworksNET.UnlockAchievement();
				this.unlockStatus[this.moneyAchievements.IndexOf(steamAchievement_SteamworksNET)] = true;
			}
		}
	}

	// Token: 0x06001A01 RID: 6657 RVA: 0x00002321 File Offset: 0x00000521
	public override bool Weaved()
	{
		return true;
	}

	// Token: 0x040010A9 RID: 4265
	[Header("Money-Related Achievements")]
	public List<SteamAchievement_SteamworksNET> moneyAchievements;

	// Token: 0x040010AA RID: 4266
	public List<bool> unlockStatus;
}
