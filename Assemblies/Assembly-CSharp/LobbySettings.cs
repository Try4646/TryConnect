using System;
using System.Collections.Generic;
using Steamworks;
using UnityEngine;

// Token: 0x0200031D RID: 797
[CreateAssetMenu(menuName = "Game Settings/Lobby Settings", fileName = "LobbySettings")]
public class LobbySettings : ScriptableObject
{
	// Token: 0x14000024 RID: 36
	// (add) Token: 0x06001ACD RID: 6861 RVA: 0x00071994 File Offset: 0x0006FB94
	// (remove) Token: 0x06001ACE RID: 6862 RVA: 0x000719C8 File Offset: 0x0006FBC8
	public static event Action<LobbySettings> SettingsChanged;

	// Token: 0x06001ACF RID: 6863 RVA: 0x000719FB File Offset: 0x0006FBFB
	public void NotifyChanged()
	{
		Action<LobbySettings> settingsChanged = LobbySettings.SettingsChanged;
		if (settingsChanged == null)
		{
			return;
		}
		settingsChanged(this);
	}

	// Token: 0x06001AD0 RID: 6864 RVA: 0x00071A10 File Offset: 0x0006FC10
	public void UpdatePlayers(List<PlayerInfo> newPlayers)
	{
		if (newPlayers != null)
		{
			foreach (PlayerInfo playerInfo in newPlayers)
			{
				PlayerInfo playerBySteamId = this.GetPlayerBySteamId(playerInfo.steamId);
				if (playerBySteamId != null && playerInfo.playerColor == Color.white)
				{
					playerInfo.playerColor = playerBySteamId.playerColor;
				}
			}
			this.players.Clear();
			this.players.AddRange(newPlayers);
			UIColorPalette colorPalette = Resources.Load<UIColorPalette>("ColorSettings");
			this.SyncColorsToPalette(colorPalette);
		}
		else
		{
			this.players.Clear();
		}
		this.currentPlayerCount = this.players.Count;
		this.NotifyChanged();
	}

	// Token: 0x06001AD1 RID: 6865 RVA: 0x00071AD8 File Offset: 0x0006FCD8
	private void SyncColorsToPalette(UIColorPalette colorPalette)
	{
		if (colorPalette == null)
		{
			return;
		}
		colorPalette.UpdateLocalPlayerColor(this);
	}

	// Token: 0x06001AD2 RID: 6866 RVA: 0x00071AEC File Offset: 0x0006FCEC
	public void UpdatePlayerColor(ulong steamId, Color newColor)
	{
		PlayerInfo playerBySteamId = this.GetPlayerBySteamId(steamId);
		if (playerBySteamId != null)
		{
			playerBySteamId.playerColor = newColor;
			ulong steamID = SteamUser.GetSteamID().m_SteamID;
			if (steamId == steamID)
			{
				UIColorPalette uicolorPalette = Resources.Load<UIColorPalette>("ColorSettings");
				if (uicolorPalette != null)
				{
					uicolorPalette.UpdateLocalPlayerColor(this);
				}
			}
			this.NotifyChanged();
		}
	}

	// Token: 0x06001AD3 RID: 6867 RVA: 0x00071B3C File Offset: 0x0006FD3C
	public PlayerInfo GetPlayerBySteamId(ulong steamId)
	{
		return this.players.Find((PlayerInfo p) => p.steamId == steamId);
	}

	// Token: 0x06001AD4 RID: 6868 RVA: 0x00071B70 File Offset: 0x0006FD70
	public void RemovePlayerBySteamId(ulong steamId)
	{
		PlayerInfo playerBySteamId = this.GetPlayerBySteamId(steamId);
		if (playerBySteamId != null)
		{
			ulong steamID = SteamUser.GetSteamID().m_SteamID;
			if (steamId == steamID)
			{
				UIColorPalette uicolorPalette = Resources.Load<UIColorPalette>("ColorSettings");
				if (uicolorPalette != null)
				{
					uicolorPalette.playerColor = Color.blue;
					uicolorPalette.NotifyChanged();
				}
			}
			this.players.Remove(playerBySteamId);
			this.NotifyChanged();
		}
	}

	// Token: 0x0400119E RID: 4510
	public CSteamID steamLobbyID;

	// Token: 0x0400119F RID: 4511
	[Header("Lobby Settings")]
	public bool createLobbyOnStart = true;

	// Token: 0x040011A0 RID: 4512
	public bool inALobby;

	// Token: 0x040011A1 RID: 4513
	public int maxPlayers;

	// Token: 0x040011A2 RID: 4514
	public int currentPlayerCount;

	// Token: 0x040011A3 RID: 4515
	[Header("Player Database")]
	public List<PlayerInfo> players = new List<PlayerInfo>();

	// Token: 0x040011A4 RID: 4516
	[Header("Code Settings")]
	public int codeLength;

	// Token: 0x040011A5 RID: 4517
	public string lobbyCode;
}
