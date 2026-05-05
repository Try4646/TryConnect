using System;
using System.Runtime.InteropServices;
using Mirror;
using UnityEngine;

// Token: 0x0200003A RID: 58
public class ClawMachineItem : NetworkBehaviour
{
	// Token: 0x06000137 RID: 311 RVA: 0x00007FE4 File Offset: 0x000061E4
	public override void OnStartServer()
	{
		base.OnStartServer();
		Rigidbody component = base.GetComponent<Rigidbody>();
		if (component != null)
		{
			component.isKinematic = false;
		}
	}

	// Token: 0x06000138 RID: 312 RVA: 0x0000800E File Offset: 0x0000620E
	[Server]
	public void ServerChangeValue(int targetValue)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void ClawMachineItem::ServerChangeValue(System.Int32)' called when server was not active");
			return;
		}
		this.Networkvalue = targetValue;
	}

	// Token: 0x06000139 RID: 313 RVA: 0x0000802C File Offset: 0x0000622C
	private void OnValueChanged(int oldValue, int newValue)
	{
		if (newValue > oldValue)
		{
			this.ChangeMatColor(Color.green);
		}
	}

	// Token: 0x0600013A RID: 314 RVA: 0x0000803D File Offset: 0x0000623D
	private void ChangeMatColor(Color targetColor)
	{
		if (this.itemRenderer != null && this.itemRenderer.material != null)
		{
			this.itemRenderer.material.color = targetColor;
		}
	}

	// Token: 0x0600013B RID: 315 RVA: 0x00008071 File Offset: 0x00006271
	public ClawMachineItem()
	{
		this._Mirror_SyncVarHookDelegate_value = new Action<int, int>(this.OnValueChanged);
	}

	// Token: 0x0600013C RID: 316 RVA: 0x00002321 File Offset: 0x00000521
	public override bool Weaved()
	{
		return true;
	}

	// Token: 0x17000024 RID: 36
	// (get) Token: 0x0600013D RID: 317 RVA: 0x00008094 File Offset: 0x00006294
	// (set) Token: 0x0600013E RID: 318 RVA: 0x000080A7 File Offset: 0x000062A7
	public int Networkvalue
	{
		get
		{
			return this.value;
		}
		[param: In]
		set
		{
			base.GeneratedSyncVarSetter<int>(value, ref this.value, 1UL, this._Mirror_SyncVarHookDelegate_value);
		}
	}

	// Token: 0x0600013F RID: 319 RVA: 0x000080C8 File Offset: 0x000062C8
	public override void SerializeSyncVars(NetworkWriter writer, bool forceAll)
	{
		base.SerializeSyncVars(writer, forceAll);
		if (forceAll)
		{
			writer.WriteVarInt(this.value);
			return;
		}
		writer.WriteVarULong(this.syncVarDirtyBits);
		if ((this.syncVarDirtyBits & 1UL) != 0UL)
		{
			writer.WriteVarInt(this.value);
		}
	}

	// Token: 0x06000140 RID: 320 RVA: 0x00008120 File Offset: 0x00006320
	public override void DeserializeSyncVars(NetworkReader reader, bool initialState)
	{
		base.DeserializeSyncVars(reader, initialState);
		if (initialState)
		{
			base.GeneratedSyncVarDeserialize<int>(ref this.value, this._Mirror_SyncVarHookDelegate_value, reader.ReadVarInt());
			return;
		}
		long num = (long)reader.ReadVarULong();
		if ((num & 1L) != 0L)
		{
			base.GeneratedSyncVarDeserialize<int>(ref this.value, this._Mirror_SyncVarHookDelegate_value, reader.ReadVarInt());
		}
	}

	// Token: 0x04000105 RID: 261
	[SerializeField]
	private Renderer itemRenderer;

	// Token: 0x04000106 RID: 262
	[SyncVar(hook = "OnValueChanged")]
	public int value = 10;

	// Token: 0x04000107 RID: 263
	public Action<int, int> _Mirror_SyncVarHookDelegate_value;
}
