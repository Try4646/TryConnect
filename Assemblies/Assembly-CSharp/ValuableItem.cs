using System;
using System.Runtime.InteropServices;
using Mirror;
using UnityEngine;

// Token: 0x02000301 RID: 769
public class ValuableItem : NetworkBehaviour
{
	// Token: 0x06001A5D RID: 6749 RVA: 0x0006F5DE File Offset: 0x0006D7DE
	private void Awake()
	{
		this._item = base.GetComponent<Item>();
	}

	// Token: 0x06001A5E RID: 6750 RVA: 0x0006F5EC File Offset: 0x0006D7EC
	private void Start()
	{
		bool isServer = base.isServer;
	}

	// Token: 0x06001A5F RID: 6751 RVA: 0x0006F5F5 File Offset: 0x0006D7F5
	private void DestroySelf()
	{
		this._item.ServerDrop();
		NetworkServer.Destroy(base.gameObject);
	}

	// Token: 0x06001A60 RID: 6752 RVA: 0x0006F60D File Offset: 0x0006D80D
	[Server]
	public void ServerChangeValue(int targetValue)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void ValuableItem::ServerChangeValue(System.Int32)' called when server was not active");
			return;
		}
		this.Networkvalue = targetValue;
	}

	// Token: 0x06001A61 RID: 6753 RVA: 0x0006F62B File Offset: 0x0006D82B
	private void OnValueChanged(int oldValue, int newValue)
	{
		if (newValue > oldValue)
		{
			this.ChangeMatColor(Color.green);
		}
	}

	// Token: 0x06001A62 RID: 6754 RVA: 0x0006F63C File Offset: 0x0006D83C
	private void ChangeMatColor(Color targetColor)
	{
		this.renderer.material.color = targetColor;
	}

	// Token: 0x06001A63 RID: 6755 RVA: 0x0006F64F File Offset: 0x0006D84F
	public ValuableItem()
	{
		this._Mirror_SyncVarHookDelegate_value = new Action<int, int>(this.OnValueChanged);
	}

	// Token: 0x06001A64 RID: 6756 RVA: 0x00002321 File Offset: 0x00000521
	public override bool Weaved()
	{
		return true;
	}

	// Token: 0x1700026D RID: 621
	// (get) Token: 0x06001A65 RID: 6757 RVA: 0x0006F674 File Offset: 0x0006D874
	// (set) Token: 0x06001A66 RID: 6758 RVA: 0x0006F687 File Offset: 0x0006D887
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

	// Token: 0x06001A67 RID: 6759 RVA: 0x0006F6A8 File Offset: 0x0006D8A8
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

	// Token: 0x06001A68 RID: 6760 RVA: 0x0006F700 File Offset: 0x0006D900
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

	// Token: 0x04001112 RID: 4370
	[SerializeField]
	private Renderer renderer;

	// Token: 0x04001113 RID: 4371
	[SyncVar(hook = "OnValueChanged")]
	public int value = 10;

	// Token: 0x04001114 RID: 4372
	private Item _item;

	// Token: 0x04001115 RID: 4373
	public Action<int, int> _Mirror_SyncVarHookDelegate_value;
}
