using System;
using System.Collections.Generic;
using Mirror;
using Steamworks;
using UnityEngine;

// Token: 0x02000340 RID: 832
[CreateAssetMenu(menuName = "Game Settings/Color Settings", fileName = "ColorSettings")]
public class UIColorPalette : ScriptableObject
{
	// Token: 0x17000282 RID: 642
	// (get) Token: 0x06001B64 RID: 7012 RVA: 0x00075148 File Offset: 0x00073348
	public Color NPCColor
	{
		get
		{
			return this.playerColors[Random.Range(0, this.playerColors.Length)];
		}
	}

	// Token: 0x1400002A RID: 42
	// (add) Token: 0x06001B65 RID: 7013 RVA: 0x00075164 File Offset: 0x00073364
	// (remove) Token: 0x06001B66 RID: 7014 RVA: 0x00075198 File Offset: 0x00073398
	public static event Action<UIColorPalette> PaletteChanged;

	// Token: 0x06001B67 RID: 7015 RVA: 0x000751CB File Offset: 0x000733CB
	public void NotifyChanged()
	{
		Action<UIColorPalette> paletteChanged = UIColorPalette.PaletteChanged;
		if (paletteChanged == null)
		{
			return;
		}
		paletteChanged(this);
	}

	// Token: 0x06001B68 RID: 7016 RVA: 0x000751E0 File Offset: 0x000733E0
	public Color GetPlayerColor(NetworkIdentity playerId)
	{
		Color result;
		if (this.playerColorMap.TryGetValue(playerId, out result))
		{
			return result;
		}
		return this.playerColor;
	}

	// Token: 0x06001B69 RID: 7017 RVA: 0x00075205 File Offset: 0x00073405
	public void SetPlayerColor(NetworkIdentity playerId, Color color)
	{
		this.playerColorMap[playerId] = color;
		this.NotifyChanged();
	}

	// Token: 0x06001B6A RID: 7018 RVA: 0x0007521A File Offset: 0x0007341A
	public void RemovePlayerColor(NetworkIdentity playerId)
	{
		if (this.playerColorMap.Remove(playerId))
		{
			this.NotifyChanged();
		}
	}

	// Token: 0x06001B6B RID: 7019 RVA: 0x00075230 File Offset: 0x00073430
	public void UpdateLocalPlayerColor(LobbySettings lobbySettings)
	{
		if (lobbySettings == null)
		{
			return;
		}
		ulong steamID = SteamUser.GetSteamID().m_SteamID;
		if (lobbySettings.GetPlayerBySteamId(steamID) != null)
		{
			this.NotifyChanged();
		}
	}

	// Token: 0x0400123C RID: 4668
	public Color profitGreen = Color.green;

	// Token: 0x0400123D RID: 4669
	public Color lossRed = Color.red;

	// Token: 0x0400123E RID: 4670
	public Color ticketYellow = Color.yellow;

	// Token: 0x0400123F RID: 4671
	public Color white = Color.white;

	// Token: 0x04001240 RID: 4672
	public Color black = Color.black;

	// Token: 0x04001241 RID: 4673
	public Color playerColor = Color.blue;

	// Token: 0x04001242 RID: 4674
	public Color gwyfMainColor = Color.gray;

	// Token: 0x04001243 RID: 4675
	public Color gwyfSecondaryColor = Color.gray;

	// Token: 0x04001244 RID: 4676
	[Header("Player Colors")]
	[Tooltip("List of colors that will be assigned to players")]
	public Color[] playerColors = new Color[]
	{
		new Color(0.282f, 0.784f, 0.424f),
		new Color(0.694f, 0.282f, 0.784f),
		new Color(0.282f, 0.541f, 0.784f),
		new Color(0.784f, 0.282f, 0.282f)
	};

	// Token: 0x04001245 RID: 4677
	private Dictionary<NetworkIdentity, Color> playerColorMap = new Dictionary<NetworkIdentity, Color>();
}
