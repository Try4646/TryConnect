using System;
using UnityEngine;

// Token: 0x0200031C RID: 796
[Serializable]
public class PlayerInfo
{
	// Token: 0x06001ACC RID: 6860 RVA: 0x00071976 File Offset: 0x0006FB76
	public PlayerInfo(string name, ulong steamId, Color color)
	{
		this.playerName = name;
		this.steamId = steamId;
		this.playerColor = color;
	}

	// Token: 0x0400119B RID: 4507
	public string playerName;

	// Token: 0x0400119C RID: 4508
	public ulong steamId;

	// Token: 0x0400119D RID: 4509
	public Color playerColor;
}
