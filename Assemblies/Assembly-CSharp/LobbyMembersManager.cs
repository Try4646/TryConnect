using System;
using System.Collections;
using System.Collections.Generic;
using Extensions;
using Steamworks;
using UnityEngine;

// Token: 0x0200010E RID: 270
public class LobbyMembersManager : MonoSingleton<LobbyMembersManager>
{
	// Token: 0x06000B45 RID: 2885 RVA: 0x0002D89C File Offset: 0x0002BA9C
	protected override void OnAwake()
	{
		base.OnAwake();
		this.lobbySettings = Resources.Load<LobbySettings>("LobbySettings");
	}

	// Token: 0x06000B46 RID: 2886 RVA: 0x0002D8B4 File Offset: 0x0002BAB4
	private void OnEnable()
	{
		this.lobbyEnterCallback = Callback<LobbyEnter_t>.Create(new Callback<LobbyEnter_t>.DispatchDelegate(this.OnLobbyEntered));
		this.lobbyChatUpdateCallback = Callback<LobbyChatUpdate_t>.Create(new Callback<LobbyChatUpdate_t>.DispatchDelegate(this.OnLobbyChatUpdate));
		this.lobbyDataUpdateCallback = Callback<LobbyDataUpdate_t>.Create(new Callback<LobbyDataUpdate_t>.DispatchDelegate(this.OnLobbyDataUpdate));
		LobbyManager.OnLobbyEnteredEvent += this.OnLobbyEnteredEvent;
		if (this.lobbySettings != null && this.lobbySettings.steamLobbyID != CSteamID.Nil && SteamManager.Initialized)
		{
			this.EnsureLocalPlayerVersionSet();
			this.RefreshPlayersList();
		}
	}

	// Token: 0x06000B47 RID: 2887 RVA: 0x0002D950 File Offset: 0x0002BB50
	private void OnDisable()
	{
		Callback<LobbyEnter_t> callback = this.lobbyEnterCallback;
		if (callback != null)
		{
			callback.Dispose();
		}
		Callback<LobbyChatUpdate_t> callback2 = this.lobbyChatUpdateCallback;
		if (callback2 != null)
		{
			callback2.Dispose();
		}
		Callback<LobbyDataUpdate_t> callback3 = this.lobbyDataUpdateCallback;
		if (callback3 != null)
		{
			callback3.Dispose();
		}
		LobbyManager.OnLobbyEnteredEvent -= this.OnLobbyEnteredEvent;
	}

	// Token: 0x06000B48 RID: 2888 RVA: 0x0002D9A4 File Offset: 0x0002BBA4
	private void OnLobbyEntered(LobbyEnter_t cb)
	{
		if (this.lastProcessedLobbyID == cb.m_ulSteamIDLobby)
		{
			return;
		}
		this.lastProcessedLobbyID = cb.m_ulSteamIDLobby;
		if (this.lobbySettings != null)
		{
			this.lobbySettings.steamLobbyID = new CSteamID(cb.m_ulSteamIDLobby);
		}
		this.EnsureLocalPlayerVersionSet();
		base.StartCoroutine(this.DelayedRefreshPlayersList());
	}

	// Token: 0x06000B49 RID: 2889 RVA: 0x0002DA03 File Offset: 0x0002BC03
	private void OnLobbyEnteredEvent()
	{
		this.EnsureLocalPlayerVersionSet();
		base.StartCoroutine(this.DelayedRefreshPlayersList());
	}

	// Token: 0x06000B4A RID: 2890 RVA: 0x0002DA18 File Offset: 0x0002BC18
	private void OnLobbyChatUpdate(LobbyChatUpdate_t cb)
	{
		if (this.lobbySettings == null || this.lobbySettings.steamLobbyID.m_SteamID != cb.m_ulSteamIDLobby)
		{
			return;
		}
		uint rgfChatMemberStateChange = cb.m_rgfChatMemberStateChange;
		bool flag = (rgfChatMemberStateChange & 1U) > 0U;
		bool flag2 = (rgfChatMemberStateChange & 2U) > 0U;
		bool flag3 = (rgfChatMemberStateChange & 4U) > 0U;
		bool flag4 = (rgfChatMemberStateChange & 8U) > 0U;
		if (flag || flag2 || flag3 || flag4)
		{
			this.EnsureLocalPlayerVersionSet();
			if (flag)
			{
				base.StartCoroutine(this.DelayedRefreshPlayersList());
				return;
			}
			this.RefreshPlayersList();
		}
	}

	// Token: 0x06000B4B RID: 2891 RVA: 0x0002DA94 File Offset: 0x0002BC94
	private void EnsureLocalPlayerVersionSet()
	{
		if (!SteamManager.Initialized || this.lobbySettings == null || this.lobbySettings.steamLobbyID == CSteamID.Nil)
		{
			return;
		}
		CSteamID steamID = SteamUser.GetSteamID();
		CSteamID steamLobbyID = this.lobbySettings.steamLobbyID;
		if (string.IsNullOrEmpty(SteamMatchmaking.GetLobbyMemberData(steamLobbyID, steamID, "GameVersion")))
		{
			SteamMatchmaking.SetLobbyMemberData(steamLobbyID, "GameVersion", Application.version);
		}
	}

	// Token: 0x06000B4C RID: 2892 RVA: 0x0002DB04 File Offset: 0x0002BD04
	private void RefreshPlayersList()
	{
		if (this.lobbySettings == null || this.lobbySettings.steamLobbyID == CSteamID.Nil || !SteamManager.Initialized)
		{
			return;
		}
		this.EnsureLocalPlayerVersionSet();
		CSteamID steamLobbyID = this.lobbySettings.steamLobbyID;
		int numLobbyMembers = SteamMatchmaking.GetNumLobbyMembers(steamLobbyID);
		CSteamID lobbyOwner = SteamMatchmaking.GetLobbyOwner(steamLobbyID);
		SteamMatchmaking.GetLobbyMemberData(steamLobbyID, lobbyOwner, "GameVersion");
		List<PlayerInfo> list = new List<PlayerInfo>();
		for (int i = 0; i < numLobbyMembers; i++)
		{
			CSteamID lobbyMemberByIndex = SteamMatchmaking.GetLobbyMemberByIndex(steamLobbyID, i);
			if (!(lobbyMemberByIndex == CSteamID.Nil))
			{
				string lobbyMemberData = SteamMatchmaking.GetLobbyMemberData(steamLobbyID, lobbyMemberByIndex, "PlayerColor");
				string lobbyMemberData2 = SteamMatchmaking.GetLobbyMemberData(steamLobbyID, lobbyMemberByIndex, "GameVersion");
				if (!string.IsNullOrEmpty(lobbyMemberData) && !string.IsNullOrEmpty(lobbyMemberData2))
				{
					string text = SteamFriends.GetFriendPersonaName(lobbyMemberByIndex);
					if (string.IsNullOrEmpty(text))
					{
						text = "Player " + lobbyMemberByIndex.m_SteamID.ToString();
					}
					Color color = ColorHexUtility.HexToColor(lobbyMemberData);
					list.Add(new PlayerInfo(text, lobbyMemberByIndex.m_SteamID, color));
				}
			}
		}
		if (MonoSingleton<VersionMismatchManager>.Instance != null)
		{
			MonoSingleton<VersionMismatchManager>.Instance.CheckVersionMismatches();
		}
		this.lobbySettings.UpdatePlayers(list);
	}

	// Token: 0x06000B4D RID: 2893 RVA: 0x0002DC3B File Offset: 0x0002BE3B
	private IEnumerator DelayedRefreshPlayersList()
	{
		yield return new WaitForSeconds(0.5f);
		this.EnsureLocalPlayerVersionSet();
		this.RefreshPlayersList();
		yield break;
	}

	// Token: 0x06000B4E RID: 2894 RVA: 0x0002DC4C File Offset: 0x0002BE4C
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
		if ((CSteamID)cb.m_ulSteamIDLobby != this.lobbySettings.steamLobbyID)
		{
			return;
		}
		CSteamID steamIDUser = new CSteamID(cb.m_ulSteamIDMember);
		CSteamID steamLobbyID = this.lobbySettings.steamLobbyID;
		string lobbyMemberData = SteamMatchmaking.GetLobbyMemberData(steamLobbyID, steamIDUser, "PlayerColor");
		string lobbyMemberData2 = SteamMatchmaking.GetLobbyMemberData(steamLobbyID, steamIDUser, "GameVersion");
		if (!string.IsNullOrEmpty(lobbyMemberData) && !string.IsNullOrEmpty(lobbyMemberData2))
		{
			this.RefreshPlayersList();
		}
	}

	// Token: 0x040006FE RID: 1790
	private LobbySettings lobbySettings;

	// Token: 0x040006FF RID: 1791
	private ulong lastProcessedLobbyID;

	// Token: 0x04000700 RID: 1792
	private Callback<LobbyEnter_t> lobbyEnterCallback;

	// Token: 0x04000701 RID: 1793
	private Callback<LobbyChatUpdate_t> lobbyChatUpdateCallback;

	// Token: 0x04000702 RID: 1794
	private Callback<LobbyDataUpdate_t> lobbyDataUpdateCallback;
}
