using System;
using Steamworks;
using UnityEngine;

// Token: 0x02000020 RID: 32
[CreateAssetMenu(fileName = "ACH_", menuName = "Game Data/Steam/Achievement")]
public class SteamAchievement_SteamworksNET : StringVariable
{
	// Token: 0x0600006B RID: 107 RVA: 0x00004908 File Offset: 0x00002B08
	public void AssignValueByAssetName()
	{
		this.Value = base.name;
		Debug.Log("Assigned achievement ID '" + this.Value + "' from asset name.");
	}

	// Token: 0x0600006C RID: 108 RVA: 0x00004930 File Offset: 0x00002B30
	public bool CheckAchievementState()
	{
		if (!SteamManager.Initialized)
		{
			Debug.LogWarning("Steam not initialized.");
			return false;
		}
		string value = this.Value;
		bool flag = false;
		if (!SteamUserStats.GetAchievement(value, out flag))
		{
			Debug.LogError("Failed to get achievement '" + value + "'.");
			return false;
		}
		Debug.Log(string.Format("Achievement {0} status: {1}", value, flag));
		return flag;
	}

	// Token: 0x0600006D RID: 109 RVA: 0x00004994 File Offset: 0x00002B94
	public void UnlockAchievement()
	{
		if (!SteamManager.Initialized)
		{
			Debug.LogWarning("Steam not initialized.");
			return;
		}
		string value = this.Value;
		if (!SteamUserStats.SetAchievement(value))
		{
			Debug.LogError("Failed to set achievement '" + value + "'.");
			return;
		}
		SteamUserStats.StoreStats();
		Debug.Log("Achievement " + value + " unlocked.");
	}

	// Token: 0x0600006E RID: 110 RVA: 0x000049F4 File Offset: 0x00002BF4
	public void ClearAchievement()
	{
		if (!SteamManager.Initialized)
		{
			Debug.LogWarning("Steam not initialized.");
			return;
		}
		string value = this.Value;
		if (!SteamUserStats.ClearAchievement(value))
		{
			Debug.LogError("Failed to clear achievement '" + value + "'.");
			return;
		}
		SteamUserStats.StoreStats();
		Debug.Log("Achievement " + value + " cleared.");
	}
}
