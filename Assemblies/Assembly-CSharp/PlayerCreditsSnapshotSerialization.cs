using System;
using System.Collections.Generic;
using Mirror;

// Token: 0x0200022D RID: 557
public static class PlayerCreditsSnapshotSerialization
{
	// Token: 0x06001450 RID: 5200 RVA: 0x000570D4 File Offset: 0x000552D4
	public static void WritePlayerCreditsSnapshot(this NetworkWriter writer, PlayerCreditsSnapshot snapshot)
	{
		writer.WriteULong(snapshot.steamId);
		writer.WriteString(snapshot.displayName ?? string.Empty);
		List<PlayerCreditsSnapshot.CosmeticEntry> cosmetics = snapshot.cosmetics;
		writer.WriteInt((cosmetics != null) ? cosmetics.Count : 0);
		if (snapshot.cosmetics != null)
		{
			foreach (PlayerCreditsSnapshot.CosmeticEntry cosmeticEntry in snapshot.cosmetics)
			{
				writer.WriteInt((int)cosmeticEntry.type);
				writer.WriteInt(cosmeticEntry.cosmeticId);
			}
		}
	}

	// Token: 0x06001451 RID: 5201 RVA: 0x0005717C File Offset: 0x0005537C
	public static PlayerCreditsSnapshot ReadPlayerCreditsSnapshot(this NetworkReader reader)
	{
		PlayerCreditsSnapshot playerCreditsSnapshot = new PlayerCreditsSnapshot
		{
			steamId = reader.ReadULong(),
			displayName = reader.ReadString()
		};
		int num = reader.ReadInt();
		playerCreditsSnapshot.cosmetics = new List<PlayerCreditsSnapshot.CosmeticEntry>(num);
		for (int i = 0; i < num; i++)
		{
			playerCreditsSnapshot.cosmetics.Add(new PlayerCreditsSnapshot.CosmeticEntry
			{
				type = (CosmeticType)reader.ReadInt(),
				cosmeticId = reader.ReadInt()
			});
		}
		return playerCreditsSnapshot;
	}
}
