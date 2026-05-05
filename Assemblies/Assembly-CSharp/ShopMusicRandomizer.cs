using System;
using System.Runtime.InteropServices;
using Mirror;
using UnityEngine;

// Token: 0x02000284 RID: 644
public class ShopMusicRandomizer : NetworkBehaviour
{
	// Token: 0x060016F9 RID: 5881 RVA: 0x00061D30 File Offset: 0x0005FF30
	public override void OnStartClient()
	{
		if (base.isServer)
		{
			this.Networksongidx = Random.Range(0, 2);
		}
		this.musicLoop.LoopSFX(true);
		this.musicLoop.loopInstance.setParameterByName("ShopSong", (float)this.songidx, false);
	}

	// Token: 0x060016FB RID: 5883 RVA: 0x00002321 File Offset: 0x00000521
	public override bool Weaved()
	{
		return true;
	}

	// Token: 0x17000209 RID: 521
	// (get) Token: 0x060016FC RID: 5884 RVA: 0x00061D7C File Offset: 0x0005FF7C
	// (set) Token: 0x060016FD RID: 5885 RVA: 0x00061D8F File Offset: 0x0005FF8F
	public int Networksongidx
	{
		get
		{
			return this.songidx;
		}
		[param: In]
		set
		{
			base.GeneratedSyncVarSetter<int>(value, ref this.songidx, 1UL, null);
		}
	}

	// Token: 0x060016FE RID: 5886 RVA: 0x00061DAC File Offset: 0x0005FFAC
	public override void SerializeSyncVars(NetworkWriter writer, bool forceAll)
	{
		base.SerializeSyncVars(writer, forceAll);
		if (forceAll)
		{
			writer.WriteVarInt(this.songidx);
			return;
		}
		writer.WriteVarULong(this.syncVarDirtyBits);
		if ((this.syncVarDirtyBits & 1UL) != 0UL)
		{
			writer.WriteVarInt(this.songidx);
		}
	}

	// Token: 0x060016FF RID: 5887 RVA: 0x00061E04 File Offset: 0x00060004
	public override void DeserializeSyncVars(NetworkReader reader, bool initialState)
	{
		base.DeserializeSyncVars(reader, initialState);
		if (initialState)
		{
			base.GeneratedSyncVarDeserialize<int>(ref this.songidx, null, reader.ReadVarInt());
			return;
		}
		long num = (long)reader.ReadVarULong();
		if ((num & 1L) != 0L)
		{
			base.GeneratedSyncVarDeserialize<int>(ref this.songidx, null, reader.ReadVarInt());
		}
	}

	// Token: 0x04000EF5 RID: 3829
	[SerializeField]
	private SFXLoopComponent musicLoop;

	// Token: 0x04000EF6 RID: 3830
	[SyncVar]
	private int songidx;
}
