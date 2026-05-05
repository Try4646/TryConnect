using System;
using Extensions;
using Steamworks;
using UnityEngine;

// Token: 0x02000111 RID: 273
public class VersionMismatchManager : MonoSingleton<VersionMismatchManager>
{
	// Token: 0x14000008 RID: 8
	// (add) Token: 0x06000B5A RID: 2906 RVA: 0x0002DDF8 File Offset: 0x0002BFF8
	// (remove) Token: 0x06000B5B RID: 2907 RVA: 0x0002DE2C File Offset: 0x0002C02C
	public static event Action<bool> OnVersionMismatchChanged;

	// Token: 0x06000B5C RID: 2908 RVA: 0x0002DE5F File Offset: 0x0002C05F
	protected override void OnAwake()
	{
		base.OnAwake();
		this.lobbySettings = Resources.Load<LobbySettings>("LobbySettings");
	}

	// Token: 0x06000B5D RID: 2909 RVA: 0x0002DE78 File Offset: 0x0002C078
	private void OnEnable()
	{
		LobbyManager.OnLobbyEnteredEvent += this.OnLobbyEntered;
		LobbyManager.OnLobbyLeftEvent += this.OnLobbyLeft;
		if (SteamManager.Initialized)
		{
			this.lobbyDataUpdateCallback = Callback<LobbyDataUpdate_t>.Create(new Callback<LobbyDataUpdate_t>.DispatchDelegate(this.OnLobbyDataUpdate));
		}
	}

	// Token: 0x06000B5E RID: 2910 RVA: 0x0002DEC5 File Offset: 0x0002C0C5
	private void OnDisable()
	{
		LobbyManager.OnLobbyEnteredEvent -= this.OnLobbyEntered;
		LobbyManager.OnLobbyLeftEvent -= this.OnLobbyLeft;
		Callback<LobbyDataUpdate_t> callback = this.lobbyDataUpdateCallback;
		if (callback == null)
		{
			return;
		}
		callback.Dispose();
	}

	// Token: 0x06000B5F RID: 2911 RVA: 0x0002DEFC File Offset: 0x0002C0FC
	private void OnLobbyDataUpdate(LobbyDataUpdate_t cb)
	{
		if (cb.m_ulSteamIDMember == 0UL)
		{
			return;
		}
		if (this.lobbySettings == null || this.lobbySettings.steamLobbyID == CSteamID.Nil)
		{
			return;
		}
		CSteamID y = new CSteamID(cb.m_ulSteamIDMember);
		CSteamID steamLobbyID = this.lobbySettings.steamLobbyID;
		int numLobbyMembers = SteamMatchmaking.GetNumLobbyMembers(steamLobbyID);
		bool flag = false;
		for (int i = 0; i < numLobbyMembers; i++)
		{
			if (SteamMatchmaking.GetLobbyMemberByIndex(steamLobbyID, i) == y)
			{
				flag = true;
				break;
			}
		}
		if (!flag)
		{
			return;
		}
		this.CheckVersionMismatches();
	}

	// Token: 0x06000B60 RID: 2912 RVA: 0x0002DF88 File Offset: 0x0002C188
	private void OnLobbyEntered()
	{
		if (this.hasVersionMismatch)
		{
			this.hasVersionMismatch = false;
			Action<bool> onVersionMismatchChanged = VersionMismatchManager.OnVersionMismatchChanged;
			if (onVersionMismatchChanged == null)
			{
				return;
			}
			onVersionMismatchChanged(false);
		}
	}

	// Token: 0x06000B61 RID: 2913 RVA: 0x0002DF88 File Offset: 0x0002C188
	private void OnLobbyLeft()
	{
		if (this.hasVersionMismatch)
		{
			this.hasVersionMismatch = false;
			Action<bool> onVersionMismatchChanged = VersionMismatchManager.OnVersionMismatchChanged;
			if (onVersionMismatchChanged == null)
			{
				return;
			}
			onVersionMismatchChanged(false);
		}
	}

	// Token: 0x06000B62 RID: 2914 RVA: 0x0002DFAC File Offset: 0x0002C1AC
	public void CheckVersionMismatches()
	{
		if (this.lobbySettings == null || this.lobbySettings.steamLobbyID == CSteamID.Nil || !SteamManager.Initialized)
		{
			return;
		}
		CSteamID steamLobbyID = this.lobbySettings.steamLobbyID;
		int numLobbyMembers = SteamMatchmaking.GetNumLobbyMembers(steamLobbyID);
		if (numLobbyMembers == 0)
		{
			return;
		}
		CSteamID lobbyOwner = SteamMatchmaking.GetLobbyOwner(steamLobbyID);
		if (lobbyOwner == CSteamID.Nil)
		{
			return;
		}
		string lobbyMemberData = SteamMatchmaking.GetLobbyMemberData(steamLobbyID, lobbyOwner, "GameVersion");
		if (string.IsNullOrEmpty(lobbyMemberData))
		{
			return;
		}
		bool flag = false;
		LobbyMemberList lobbyMemberList = Object.FindFirstObjectByType<LobbyMemberList>();
		for (int i = 0; i < numLobbyMembers; i++)
		{
			CSteamID lobbyMemberByIndex = SteamMatchmaking.GetLobbyMemberByIndex(steamLobbyID, i);
			if (lobbyMemberByIndex != CSteamID.Nil && !(lobbyMemberByIndex == lobbyOwner))
			{
				string lobbyMemberData2 = SteamMatchmaking.GetLobbyMemberData(steamLobbyID, lobbyMemberByIndex, "GameVersion");
				if (string.IsNullOrEmpty(lobbyMemberData2) || lobbyMemberData2 != lobbyMemberData)
				{
					Debug.Log(string.Format("[VersionMismatchManager] Version mismatch detected - Host: {0}, Player: {1} (SteamID: {2})", lobbyMemberData, lobbyMemberData2 ?? "empty", lobbyMemberByIndex.m_SteamID));
					flag = true;
					if (lobbyMemberList != null)
					{
						lobbyMemberList.UpdatePlayerVersionMismatch(lobbyMemberByIndex, lobbyMemberData2 ?? "unknown", true);
					}
				}
				else if (lobbyMemberList != null)
				{
					lobbyMemberList.UpdatePlayerVersionMismatch(lobbyMemberByIndex, lobbyMemberData2, false);
				}
			}
		}
		if (this.hasVersionMismatch != flag)
		{
			this.hasVersionMismatch = flag;
			Action<bool> onVersionMismatchChanged = VersionMismatchManager.OnVersionMismatchChanged;
			if (onVersionMismatchChanged != null)
			{
				onVersionMismatchChanged(this.hasVersionMismatch);
			}
			Debug.Log(string.Format("Version mismatch status changed: {0}", this.hasVersionMismatch));
		}
	}

	// Token: 0x06000B63 RID: 2915 RVA: 0x0002E138 File Offset: 0x0002C338
	public bool HasVersionMismatch()
	{
		return this.hasVersionMismatch;
	}

	// Token: 0x04000708 RID: 1800
	private LobbySettings lobbySettings;

	// Token: 0x04000709 RID: 1801
	private bool hasVersionMismatch;

	// Token: 0x0400070A RID: 1802
	private Callback<LobbyDataUpdate_t> lobbyDataUpdateCallback;
}
