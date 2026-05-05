using System;
using System.Runtime.InteropServices;
using Extensions;
using Mirror;
using TMPro;
using UnityEngine;

// Token: 0x020002DA RID: 730
public class RerollButton : InteractableEventTrigger
{
	// Token: 0x06001993 RID: 6547 RVA: 0x0006B3F9 File Offset: 0x000695F9
	private void OnRerollCostChanged(int oldCost, int newCost)
	{
		this.rerollCostText.text = string.Format("{0} Tickets", newCost);
	}

	// Token: 0x06001994 RID: 6548 RVA: 0x0006B416 File Offset: 0x00069616
	public override void OnStartServer()
	{
		base.OnStartServer();
		this.Network_rerollCost = NetworkSingleton<ItemStampManager>.Instance.GetCurrentRerollCost();
	}

	// Token: 0x06001995 RID: 6549 RVA: 0x0006B42E File Offset: 0x0006962E
	public override void ServerOnInteract(PlayerInteract playerInteract)
	{
		base.ServerOnInteract(playerInteract);
		NetworkSingleton<ItemStampManager>.Instance.TryRerollAllItemStampsWithCost();
		this.Network_rerollCost = NetworkSingleton<ItemStampManager>.Instance.GetCurrentRerollCost();
	}

	// Token: 0x06001996 RID: 6550 RVA: 0x0006B451 File Offset: 0x00069651
	public RerollButton()
	{
		this._Mirror_SyncVarHookDelegate__rerollCost = new Action<int, int>(this.OnRerollCostChanged);
	}

	// Token: 0x06001997 RID: 6551 RVA: 0x00002321 File Offset: 0x00000521
	public override bool Weaved()
	{
		return true;
	}

	// Token: 0x17000264 RID: 612
	// (get) Token: 0x06001998 RID: 6552 RVA: 0x0006B46C File Offset: 0x0006966C
	// (set) Token: 0x06001999 RID: 6553 RVA: 0x0006B47F File Offset: 0x0006967F
	public int Network_rerollCost
	{
		get
		{
			return this._rerollCost;
		}
		[param: In]
		set
		{
			base.GeneratedSyncVarSetter<int>(value, ref this._rerollCost, 1UL, this._Mirror_SyncVarHookDelegate__rerollCost);
		}
	}

	// Token: 0x0600199A RID: 6554 RVA: 0x0006B4A0 File Offset: 0x000696A0
	public override void SerializeSyncVars(NetworkWriter writer, bool forceAll)
	{
		base.SerializeSyncVars(writer, forceAll);
		if (forceAll)
		{
			writer.WriteVarInt(this._rerollCost);
			return;
		}
		writer.WriteVarULong(this.syncVarDirtyBits);
		if ((this.syncVarDirtyBits & 1UL) != 0UL)
		{
			writer.WriteVarInt(this._rerollCost);
		}
	}

	// Token: 0x0600199B RID: 6555 RVA: 0x0006B4F8 File Offset: 0x000696F8
	public override void DeserializeSyncVars(NetworkReader reader, bool initialState)
	{
		base.DeserializeSyncVars(reader, initialState);
		if (initialState)
		{
			base.GeneratedSyncVarDeserialize<int>(ref this._rerollCost, this._Mirror_SyncVarHookDelegate__rerollCost, reader.ReadVarInt());
			return;
		}
		long num = (long)reader.ReadVarULong();
		if ((num & 1L) != 0L)
		{
			base.GeneratedSyncVarDeserialize<int>(ref this._rerollCost, this._Mirror_SyncVarHookDelegate__rerollCost, reader.ReadVarInt());
		}
	}

	// Token: 0x0400106A RID: 4202
	[SerializeField]
	private TextMeshPro rerollCostText;

	// Token: 0x0400106B RID: 4203
	[SyncVar(hook = "OnRerollCostChanged")]
	private int _rerollCost;

	// Token: 0x0400106C RID: 4204
	public Action<int, int> _Mirror_SyncVarHookDelegate__rerollCost;
}
