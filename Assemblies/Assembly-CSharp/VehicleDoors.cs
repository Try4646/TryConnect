using System;
using System.Runtime.InteropServices;
using FMODUnity;
using Mirror;
using UnityEngine;

// Token: 0x02000302 RID: 770
public class VehicleDoors : NetworkBehaviour
{
	// Token: 0x06001A69 RID: 6761 RVA: 0x0006F765 File Offset: 0x0006D965
	[Server]
	public void ServerOpenDoors()
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void VehicleDoors::ServerOpenDoors()' called when server was not active");
			return;
		}
		if (this._doorsOpen)
		{
			return;
		}
		this.Network_doorsOpen = true;
		this.triggerZone.IsActive = true;
	}

	// Token: 0x06001A6A RID: 6762 RVA: 0x0006F798 File Offset: 0x0006D998
	private void OnDoorsOpenChanged(bool oldValue, bool newValue)
	{
		if (newValue)
		{
			this.animator.SetTrigger("openDoors");
			StudioParameterTrigger component = base.GetComponent<StudioParameterTrigger>();
			if (component != null)
			{
				component.TriggerParameters();
			}
		}
	}

	// Token: 0x06001A6B RID: 6763 RVA: 0x0006F7CE File Offset: 0x0006D9CE
	public VehicleDoors()
	{
		this._Mirror_SyncVarHookDelegate__doorsOpen = new Action<bool, bool>(this.OnDoorsOpenChanged);
	}

	// Token: 0x06001A6C RID: 6764 RVA: 0x00002321 File Offset: 0x00000521
	public override bool Weaved()
	{
		return true;
	}

	// Token: 0x1700026E RID: 622
	// (get) Token: 0x06001A6D RID: 6765 RVA: 0x0006F7E8 File Offset: 0x0006D9E8
	// (set) Token: 0x06001A6E RID: 6766 RVA: 0x0006F7FB File Offset: 0x0006D9FB
	public bool Network_doorsOpen
	{
		get
		{
			return this._doorsOpen;
		}
		[param: In]
		set
		{
			base.GeneratedSyncVarSetter<bool>(value, ref this._doorsOpen, 1UL, this._Mirror_SyncVarHookDelegate__doorsOpen);
		}
	}

	// Token: 0x06001A6F RID: 6767 RVA: 0x0006F81C File Offset: 0x0006DA1C
	public override void SerializeSyncVars(NetworkWriter writer, bool forceAll)
	{
		base.SerializeSyncVars(writer, forceAll);
		if (forceAll)
		{
			writer.WriteBool(this._doorsOpen);
			return;
		}
		writer.WriteVarULong(this.syncVarDirtyBits);
		if ((this.syncVarDirtyBits & 1UL) != 0UL)
		{
			writer.WriteBool(this._doorsOpen);
		}
	}

	// Token: 0x06001A70 RID: 6768 RVA: 0x0006F874 File Offset: 0x0006DA74
	public override void DeserializeSyncVars(NetworkReader reader, bool initialState)
	{
		base.DeserializeSyncVars(reader, initialState);
		if (initialState)
		{
			base.GeneratedSyncVarDeserialize<bool>(ref this._doorsOpen, this._Mirror_SyncVarHookDelegate__doorsOpen, reader.ReadBool());
			return;
		}
		long num = (long)reader.ReadVarULong();
		if ((num & 1L) != 0L)
		{
			base.GeneratedSyncVarDeserialize<bool>(ref this._doorsOpen, this._Mirror_SyncVarHookDelegate__doorsOpen, reader.ReadBool());
		}
	}

	// Token: 0x04001116 RID: 4374
	[SerializeField]
	private Animator animator;

	// Token: 0x04001117 RID: 4375
	[SerializeField]
	private AllPlayersTriggerZone triggerZone;

	// Token: 0x04001118 RID: 4376
	[SyncVar(hook = "OnDoorsOpenChanged")]
	private bool _doorsOpen;

	// Token: 0x04001119 RID: 4377
	public Action<bool, bool> _Mirror_SyncVarHookDelegate__doorsOpen;
}
