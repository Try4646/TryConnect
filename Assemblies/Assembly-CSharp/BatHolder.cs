using System;
using System.Runtime.InteropServices;
using Mirror;
using UnityEngine;

// Token: 0x02000021 RID: 33
public class BatHolder : NetworkBehaviour
{
	// Token: 0x06000070 RID: 112 RVA: 0x00004A5B File Offset: 0x00002C5B
	private void OnBatSet(Bat oldBat, Bat newBat)
	{
		newBat.GetComponent<Rigidbody>().isKinematic = true;
		newBat.transform.SetParent(this.batSpawnPoint);
		newBat.LocalSetBatSpawnPoint(this.batSpawnPoint);
	}

	// Token: 0x06000071 RID: 113 RVA: 0x00004A88 File Offset: 0x00002C88
	public override void OnStartServer()
	{
		base.OnStartServer();
		Bat bat = Object.Instantiate<Bat>(this.batPrefab, this.batSpawnPoint.position, this.batSpawnPoint.rotation);
		NetworkServer.Spawn(bat.gameObject, null);
		this.Network_bat = bat;
	}

	// Token: 0x06000072 RID: 114 RVA: 0x00004AD0 File Offset: 0x00002CD0
	public BatHolder()
	{
		this._Mirror_SyncVarHookDelegate__bat = new Action<Bat, Bat>(this.OnBatSet);
	}

	// Token: 0x06000073 RID: 115 RVA: 0x00002321 File Offset: 0x00000521
	public override bool Weaved()
	{
		return true;
	}

	// Token: 0x1700000E RID: 14
	// (get) Token: 0x06000074 RID: 116 RVA: 0x00004AEC File Offset: 0x00002CEC
	// (set) Token: 0x06000075 RID: 117 RVA: 0x00004B0B File Offset: 0x00002D0B
	public Bat Network_bat
	{
		get
		{
			return base.GetSyncVarNetworkBehaviour<Bat>(this.____batNetId, ref this._bat);
		}
		[param: In]
		set
		{
			base.GeneratedSyncVarSetter_NetworkBehaviour<Bat>(value, ref this._bat, 1UL, this._Mirror_SyncVarHookDelegate__bat, ref this.____batNetId);
		}
	}

	// Token: 0x06000076 RID: 118 RVA: 0x00004B30 File Offset: 0x00002D30
	public override void SerializeSyncVars(NetworkWriter writer, bool forceAll)
	{
		base.SerializeSyncVars(writer, forceAll);
		if (forceAll)
		{
			writer.WriteNetworkBehaviour(this.Network_bat);
			return;
		}
		writer.WriteVarULong(this.syncVarDirtyBits);
		if ((this.syncVarDirtyBits & 1UL) != 0UL)
		{
			writer.WriteNetworkBehaviour(this.Network_bat);
		}
	}

	// Token: 0x06000077 RID: 119 RVA: 0x00004B88 File Offset: 0x00002D88
	public override void DeserializeSyncVars(NetworkReader reader, bool initialState)
	{
		base.DeserializeSyncVars(reader, initialState);
		if (initialState)
		{
			base.GeneratedSyncVarDeserialize_NetworkBehaviour<Bat>(ref this._bat, this._Mirror_SyncVarHookDelegate__bat, reader, ref this.____batNetId);
			return;
		}
		long num = (long)reader.ReadVarULong();
		if ((num & 1L) != 0L)
		{
			base.GeneratedSyncVarDeserialize_NetworkBehaviour<Bat>(ref this._bat, this._Mirror_SyncVarHookDelegate__bat, reader, ref this.____batNetId);
		}
	}

	// Token: 0x0400007B RID: 123
	[SerializeField]
	private Bat batPrefab;

	// Token: 0x0400007C RID: 124
	[SerializeField]
	private Transform batSpawnPoint;

	// Token: 0x0400007D RID: 125
	[SyncVar(hook = "OnBatSet")]
	private Bat _bat;

	// Token: 0x0400007E RID: 126
	protected NetworkBehaviourSyncVar ____batNetId;

	// Token: 0x0400007F RID: 127
	public Action<Bat, Bat> _Mirror_SyncVarHookDelegate__bat;
}
