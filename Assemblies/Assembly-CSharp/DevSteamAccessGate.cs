using System;
using System.Collections.Generic;
using SRDebugger.Services;
using Steamworks;
using UnityEngine;

// Token: 0x02000150 RID: 336
[DisallowMultipleComponent]
public class DevSteamAccessGate : MonoBehaviour
{
	// Token: 0x06000CD1 RID: 3281 RVA: 0x0003609C File Offset: 0x0003429C
	private void Start()
	{
		bool flag = this.IsCurrentUserDeveloper();
		InputEvents.SetDevMode(flag);
		DevSteamAccessGate.ApplySrDebuggerAccess(flag);
	}

	// Token: 0x06000CD2 RID: 3282 RVA: 0x000360B0 File Offset: 0x000342B0
	private bool IsCurrentUserDeveloper()
	{
		if (!SteamManager.Initialized)
		{
			return false;
		}
		ulong steamID = SteamUser.GetSteamID().m_SteamID;
		for (int i = 0; i < this.devAccounts.Count; i++)
		{
			if (this.devAccounts[i].steamId == steamID)
			{
				return true;
			}
		}
		return false;
	}

	// Token: 0x06000CD3 RID: 3283 RVA: 0x00036100 File Offset: 0x00034300
	private static void ApplySrDebuggerAccess(bool isDev)
	{
		if (!SRDebug.IsInitialized)
		{
			SRDebug.Init();
		}
		IDebugService instance = SRDebug.Instance;
		instance.IsTriggerEnabled = isDev;
		instance.IsTriggerErrorNotificationEnabled = isDev;
		if (!isDev)
		{
			instance.DestroyDebugPanel();
		}
	}

	// Token: 0x04000845 RID: 2117
	[SerializeField]
	private List<DevSteamAccessGate.DevAccount> devAccounts = new List<DevSteamAccessGate.DevAccount>();

	// Token: 0x02000151 RID: 337
	[Serializable]
	public struct DevAccount
	{
		// Token: 0x04000846 RID: 2118
		public string username;

		// Token: 0x04000847 RID: 2119
		public ulong steamId;
	}
}
