using System;
using System.Runtime.InteropServices;
using Mirror;
using UnityEngine;

// Token: 0x020001F0 RID: 496
public class PlayerBody : NetworkBehaviour
{
	// Token: 0x060011C9 RID: 4553 RVA: 0x00002321 File Offset: 0x00000521
	public override bool Weaved()
	{
		return true;
	}

	// Token: 0x170001A1 RID: 417
	// (get) Token: 0x060011CA RID: 4554 RVA: 0x0004CD48 File Offset: 0x0004AF48
	// (set) Token: 0x060011CB RID: 4555 RVA: 0x0004CD5B File Offset: 0x0004AF5B
	public bool Network_hasBody
	{
		get
		{
			return this._hasBody;
		}
		[param: In]
		set
		{
			base.GeneratedSyncVarSetter<bool>(value, ref this._hasBody, 1UL, null);
		}
	}

	// Token: 0x060011CC RID: 4556 RVA: 0x0004CD78 File Offset: 0x0004AF78
	public override void SerializeSyncVars(NetworkWriter writer, bool forceAll)
	{
		base.SerializeSyncVars(writer, forceAll);
		if (forceAll)
		{
			writer.WriteBool(this._hasBody);
			return;
		}
		writer.WriteVarULong(this.syncVarDirtyBits);
		if ((this.syncVarDirtyBits & 1UL) != 0UL)
		{
			writer.WriteBool(this._hasBody);
		}
	}

	// Token: 0x060011CD RID: 4557 RVA: 0x0004CDD0 File Offset: 0x0004AFD0
	public override void DeserializeSyncVars(NetworkReader reader, bool initialState)
	{
		base.DeserializeSyncVars(reader, initialState);
		if (initialState)
		{
			base.GeneratedSyncVarDeserialize<bool>(ref this._hasBody, null, reader.ReadBool());
			return;
		}
		long num = (long)reader.ReadVarULong();
		if ((num & 1L) != 0L)
		{
			base.GeneratedSyncVarDeserialize<bool>(ref this._hasBody, null, reader.ReadBool());
		}
	}

	// Token: 0x04000B7E RID: 2942
	private Rigidbody _rb;

	// Token: 0x04000B7F RID: 2943
	[SyncVar]
	private bool _hasBody = true;
}
