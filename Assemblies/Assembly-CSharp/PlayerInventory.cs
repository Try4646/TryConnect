using System;
using System.Collections;
using System.Runtime.InteropServices;
using Extensions;
using JetBrains.Annotations;
using Mirror;
using Mirror.RemoteCalls;
using SkyBrave_Toolkit.Scripts.Scriptable_Game_Events;
using UnityEngine;

// Token: 0x02000203 RID: 515
public class PlayerInventory : NetworkBehaviour
{
	// Token: 0x14000014 RID: 20
	// (add) Token: 0x060012DB RID: 4827 RVA: 0x00051C54 File Offset: 0x0004FE54
	// (remove) Token: 0x060012DC RID: 4828 RVA: 0x00051C8C File Offset: 0x0004FE8C
	public event Action<Item> OnClientItemPickup;

	// Token: 0x14000015 RID: 21
	// (add) Token: 0x060012DD RID: 4829 RVA: 0x00051CC4 File Offset: 0x0004FEC4
	// (remove) Token: 0x060012DE RID: 4830 RVA: 0x00051CFC File Offset: 0x0004FEFC
	public event Action<float, Item> OnClientItemThrown;

	// Token: 0x14000016 RID: 22
	// (add) Token: 0x060012DF RID: 4831 RVA: 0x00051D34 File Offset: 0x0004FF34
	// (remove) Token: 0x060012E0 RID: 4832 RVA: 0x00051D6C File Offset: 0x0004FF6C
	public event Action ServerOnItemStash;

	// Token: 0x14000017 RID: 23
	// (add) Token: 0x060012E1 RID: 4833 RVA: 0x00051DA4 File Offset: 0x0004FFA4
	// (remove) Token: 0x060012E2 RID: 4834 RVA: 0x00051DDC File Offset: 0x0004FFDC
	public event Action<float> OnThrowChargeChanged;

	// Token: 0x14000018 RID: 24
	// (add) Token: 0x060012E3 RID: 4835 RVA: 0x00051E14 File Offset: 0x00050014
	// (remove) Token: 0x060012E4 RID: 4836 RVA: 0x00051E4C File Offset: 0x0005004C
	public event Action OnLocalInventoryUpdated;

	// Token: 0x060012E5 RID: 4837 RVA: 0x00051E84 File Offset: 0x00050084
	public override void OnStartServer()
	{
		base.OnStartServer();
		int num = 0;
		while ((long)num < (long)((ulong)this.inventorySlotCount))
		{
			this.Pockets.Add(null);
			num++;
		}
	}

	// Token: 0x060012E6 RID: 4838 RVA: 0x00051EB8 File Offset: 0x000500B8
	public override void OnStartClient()
	{
		base.OnStartClient();
		SyncList<Item> pockets = this.Pockets;
		pockets.Callback = (Action<SyncList<Item>.Operation, int, Item, Item>)Delegate.Combine(pockets.Callback, new Action<SyncList<Item>.Operation, int, Item, Item>(this.OnPocketsChanged));
		if (!base.isLocalPlayer)
		{
			base.enabled = false;
			return;
		}
		MonoSingleton<LocalManager>.Instance.interactionUIPanel.SetPlayerInventory(this);
		MonoSingleton<LocalManager>.Instance.heldItemActionPanel.SetPlayerInventory(this);
	}

	// Token: 0x060012E7 RID: 4839 RVA: 0x00051F22 File Offset: 0x00050122
	private void Awake()
	{
		this._ps = Resources.Load<PlayerSettings>("PlayerSettings");
		this._rigidbody = base.GetComponent<Rigidbody>();
		this._pc = base.GetComponent<PlayerController>();
	}

	// Token: 0x060012E8 RID: 4840 RVA: 0x00051F4C File Offset: 0x0005014C
	private void OnEnable()
	{
		InputEvents.OnThrowItemEvent = (Action<bool>)Delegate.Combine(InputEvents.OnThrowItemEvent, new Action<bool>(this.OnThrowItemEvent));
		InputEvents.OnItemSelectEvent = (Action<int>)Delegate.Combine(InputEvents.OnItemSelectEvent, new Action<int>(this.OnItemSelectEvent));
		InputEvents.OnZoomEvent = (Action<bool>)Delegate.Combine(InputEvents.OnZoomEvent, new Action<bool>(this.OnZoomEvent));
	}

	// Token: 0x060012E9 RID: 4841 RVA: 0x00051FBC File Offset: 0x000501BC
	private void OnDisable()
	{
		InputEvents.OnThrowItemEvent = (Action<bool>)Delegate.Remove(InputEvents.OnThrowItemEvent, new Action<bool>(this.OnThrowItemEvent));
		InputEvents.OnItemSelectEvent = (Action<int>)Delegate.Remove(InputEvents.OnItemSelectEvent, new Action<int>(this.OnItemSelectEvent));
		InputEvents.OnZoomEvent = (Action<bool>)Delegate.Remove(InputEvents.OnZoomEvent, new Action<bool>(this.OnZoomEvent));
	}

	// Token: 0x060012EA RID: 4842 RVA: 0x00052029 File Offset: 0x00050229
	public void SetPlayerHandsVisible(bool visible)
	{
		if (this.playerHands != null)
		{
			this.playerHands.SetActive(visible);
		}
	}

	// Token: 0x060012EB RID: 4843 RVA: 0x00052048 File Offset: 0x00050248
	[Tooltip("Only call this from Item script!!")]
	[Server]
	public void ServerAddItem(Item item)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void PlayerInventory::ServerAddItem(Item)' called when server was not active");
			return;
		}
		if (this.NetworkholdingItem == item)
		{
			return;
		}
		if (this.NetworkholdingItem)
		{
			bool flag = false;
			for (int i = 0; i < this.Pockets.Count; i++)
			{
				if (this.Pockets[i] == null)
				{
					this.Pockets[i] = this.NetworkholdingItem;
					this.NetworkholdingItem = null;
					this.Pockets[i].ServerHandExit(this);
					flag = true;
					break;
				}
			}
			if (!flag)
			{
				this.NetworkholdingItem.ServerDrop();
			}
		}
		this.NetworkholdingItem = item;
	}

	// Token: 0x060012EC RID: 4844 RVA: 0x000520F6 File Offset: 0x000502F6
	[Tooltip("Only call this from Item script!!")]
	[Server]
	public void ServerRemoveItem(Item item)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void PlayerInventory::ServerRemoveItem(Item)' called when server was not active");
			return;
		}
		if (!this.NetworkholdingItem)
		{
			return;
		}
		if (this.NetworkholdingItem != item)
		{
			return;
		}
		this.NetworkholdingItem = null;
	}

	// Token: 0x060012ED RID: 4845 RVA: 0x00052134 File Offset: 0x00050334
	[Server]
	[CanBeNull]
	public Item ServerDropHoldingItem()
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'Item PlayerInventory::ServerDropHoldingItem()' called when server was not active");
			return null;
		}
		if (this.NetworkholdingItem)
		{
			this.NetworkholdingItem.ServerDrop();
			return this.NetworkholdingItem;
		}
		return null;
	}

	// Token: 0x060012EE RID: 4846 RVA: 0x00052184 File Offset: 0x00050384
	[Server]
	public void ServerThrowItemRandomly()
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void PlayerInventory::ServerThrowItemRandomly()' called when server was not active");
			return;
		}
		this.ServerThrowItem(this._pc.serverVelocity, (Random.insideUnitSphere + Vector3.up).normalized, this._ps.maxItemThrowForce, this._ps.maxItemThrowTorque);
	}

	// Token: 0x060012EF RID: 4847 RVA: 0x000521E4 File Offset: 0x000503E4
	[Server]
	public void ServerRemoveItemFromPocket(Item item)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void PlayerInventory::ServerRemoveItemFromPocket(Item)' called when server was not active");
			return;
		}
		for (int i = 0; i < this.Pockets.Count; i++)
		{
			if (this.Pockets[i] == item)
			{
				item.ServerDrop();
				this.Pockets[i] = null;
			}
		}
	}

	// Token: 0x060012F0 RID: 4848 RVA: 0x00052243 File Offset: 0x00050443
	private void OnHoldingItemChanged([CanBeNull] Item oldItem, [CanBeNull] Item newItem)
	{
		if (newItem)
		{
			Action<Item> onClientItemPickup = this.OnClientItemPickup;
			if (onClientItemPickup != null)
			{
				onClientItemPickup(newItem);
			}
		}
		this.OnInventoryUpdate(oldItem, newItem);
	}

	// Token: 0x060012F1 RID: 4849 RVA: 0x00052267 File Offset: 0x00050467
	private void OnPocketsChanged(SyncList<Item>.Operation op, int index, Item oldItem, Item newItem)
	{
		this.OnInventoryUpdate(oldItem, newItem);
	}

	// Token: 0x060012F2 RID: 4850 RVA: 0x00052274 File Offset: 0x00050474
	private void OnInventoryUpdate([CanBeNull] Item oldItem, [CanBeNull] Item newItem)
	{
		if (base.isLocalPlayer)
		{
			if (newItem)
			{
				this._localAlreadyThrown = false;
			}
			GameEvent gameEvent = this.localOnInventoryUpdate;
			if (gameEvent != null)
			{
				gameEvent.Raise();
			}
			Action onLocalInventoryUpdated = this.OnLocalInventoryUpdated;
			if (onLocalInventoryUpdated != null)
			{
				onLocalInventoryUpdated();
			}
			this.StopThrowRoutine();
			if (newItem && InputEvents.IsThrowItemPressed)
			{
				this._throwRoutine = base.StartCoroutine(this.ThrowRoutine());
			}
		}
	}

	// Token: 0x060012F3 RID: 4851 RVA: 0x000522E4 File Offset: 0x000504E4
	private void OnItemSelectEvent(int index)
	{
		if (index > this.Pockets.Count || index <= 0)
		{
			return;
		}
		int slot = index - 1;
		this.CmdSelectSlot(slot);
	}

	// Token: 0x060012F4 RID: 4852 RVA: 0x00052310 File Offset: 0x00050510
	[Command]
	private void CmdSelectSlot(int slot)
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		writer.WriteVarInt(slot);
		base.SendCommandInternal("System.Void PlayerInventory::CmdSelectSlot(System.Int32)", -1745405221, writer, 0, true);
		NetworkWriterPool.Return(writer);
	}

	// Token: 0x060012F5 RID: 4853 RVA: 0x0005234C File Offset: 0x0005054C
	[ClientRpc]
	private void RpcSetItemParent(Item item, bool active)
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		writer.WriteNetworkBehaviour(item);
		writer.WriteBool(active);
		this.SendRPCInternal("System.Void PlayerInventory::RpcSetItemParent(Item,System.Boolean)", -1603755222, writer, 0, true);
		NetworkWriterPool.Return(writer);
	}

	// Token: 0x060012F6 RID: 4854 RVA: 0x00052390 File Offset: 0x00050590
	private void OnZoomEvent(bool isPressed)
	{
		if (!isPressed)
		{
			return;
		}
		if (this._throwRoutine != null)
		{
			this._isThrowCancelled = true;
		}
		this.StopThrowRoutine();
	}

	// Token: 0x060012F7 RID: 4855 RVA: 0x000523AC File Offset: 0x000505AC
	private void OnThrowItemEvent(bool isPressed)
	{
		if (!this.NetworkholdingItem)
		{
			return;
		}
		if (this._localAlreadyThrown)
		{
			return;
		}
		if (isPressed)
		{
			if (this._throwRoutine != null)
			{
				base.StopCoroutine(this._throwRoutine);
			}
			this._throwRoutine = base.StartCoroutine(this.ThrowRoutine());
			return;
		}
		if (this._isThrowCancelled)
		{
			this._isThrowCancelled = false;
			return;
		}
		this._localAlreadyThrown = true;
		float force = Mathf.Lerp(this._ps.minItemThrowForce, this._ps.maxItemThrowForce, this._currentThrowPercentage);
		float torque = Mathf.Lerp(this._ps.minItemThrowTorque, this._ps.maxItemThrowTorque, this._currentThrowPercentage);
		this.StopThrowRoutine();
		this.NetworkholdingItem.OnLocalDrop();
		this.CmdThrowItem(this._rigidbody.linearVelocity, this.throwPosition.forward, force, torque);
		this.OnItemThrown(force, this.NetworkholdingItem);
		this.CmdOnItemThrown(force, this.NetworkholdingItem);
	}

	// Token: 0x060012F8 RID: 4856 RVA: 0x000524A4 File Offset: 0x000506A4
	private void StopThrowRoutine()
	{
		if (this._throwRoutine != null)
		{
			base.StopCoroutine(this._throwRoutine);
		}
		this._throwRoutine = null;
		this._currentThrowPercentage = 0f;
		this.throwSfxLoopComponent.LoopSFX(false);
		Action<float> onThrowChargeChanged = this.OnThrowChargeChanged;
		if (onThrowChargeChanged == null)
		{
			return;
		}
		onThrowChargeChanged(0f);
	}

	// Token: 0x060012F9 RID: 4857 RVA: 0x000524F8 File Offset: 0x000506F8
	[Command]
	private void CmdOnItemThrown(float force, Item item)
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		writer.WriteFloat(force);
		writer.WriteNetworkBehaviour(item);
		base.SendCommandInternal("System.Void PlayerInventory::CmdOnItemThrown(System.Single,Item)", -862694014, writer, 0, true);
		NetworkWriterPool.Return(writer);
	}

	// Token: 0x060012FA RID: 4858 RVA: 0x0005253C File Offset: 0x0005073C
	[ClientRpc]
	private void RpcOnItemThrown(float force, Item item)
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		writer.WriteFloat(force);
		writer.WriteNetworkBehaviour(item);
		this.SendRPCInternal("System.Void PlayerInventory::RpcOnItemThrown(System.Single,Item)", -781555935, writer, 0, true);
		NetworkWriterPool.Return(writer);
	}

	// Token: 0x060012FB RID: 4859 RVA: 0x00052580 File Offset: 0x00050780
	private void OnItemThrown(float force, Item item)
	{
		Action<float, Item> onClientItemThrown = this.OnClientItemThrown;
		if (onClientItemThrown == null)
		{
			return;
		}
		onClientItemThrown(force, item);
	}

	// Token: 0x060012FC RID: 4860 RVA: 0x00052594 File Offset: 0x00050794
	private IEnumerator ThrowRoutine()
	{
		this.throwSfxLoopComponent.LoopSFX(true);
		this._currentThrowPercentage = 0f;
		Action<float> onThrowChargeChanged = this.OnThrowChargeChanged;
		if (onThrowChargeChanged != null)
		{
			onThrowChargeChanged(0f);
		}
		float percentage = 0f;
		while (percentage < 1f)
		{
			percentage = Mathf.Clamp01(percentage + Time.deltaTime / this._ps.itemThrowDuration);
			this._currentThrowPercentage = Mathf.InverseLerp(this._ps.throwThreshold / this._ps.itemThrowDuration, 1f, percentage);
			if (this._currentThrowPercentage > 0f)
			{
				Action<float> onThrowChargeChanged2 = this.OnThrowChargeChanged;
				if (onThrowChargeChanged2 != null)
				{
					onThrowChargeChanged2(this._currentThrowPercentage);
				}
			}
			yield return new WaitForEndOfFrame();
		}
		yield break;
	}

	// Token: 0x060012FD RID: 4861 RVA: 0x000525A4 File Offset: 0x000507A4
	[Command]
	private void CmdThrowItem(Vector3 velocity, Vector3 direction, float force, float torque)
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		writer.WriteVector3(velocity);
		writer.WriteVector3(direction);
		writer.WriteFloat(force);
		writer.WriteFloat(torque);
		base.SendCommandInternal("System.Void PlayerInventory::CmdThrowItem(UnityEngine.Vector3,UnityEngine.Vector3,System.Single,System.Single)", 1818901107, writer, 0, true);
		NetworkWriterPool.Return(writer);
	}

	// Token: 0x060012FE RID: 4862 RVA: 0x000525FC File Offset: 0x000507FC
	[Server]
	private void ServerThrowItem(Vector3 velocity, Vector3 direction, float force, float torque)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void PlayerInventory::ServerThrowItem(UnityEngine.Vector3,UnityEngine.Vector3,System.Single,System.Single)' called when server was not active");
			return;
		}
		if (!this.NetworkholdingItem)
		{
			return;
		}
		Item networkholdingItem = this.NetworkholdingItem;
		Vector3 velocity2 = velocity + direction * force / (networkholdingItem.Mass + this._ps.constantMass);
		Vector3 angularVelocity = Random.insideUnitSphere * torque / (networkholdingItem.Mass + this._ps.constantMass);
		networkholdingItem.ServerThrow(this.throwPosition.position, this.throwPosition.rotation, velocity2, angularVelocity);
	}

	// Token: 0x060012FF RID: 4863 RVA: 0x0005269A File Offset: 0x0005089A
	public PlayerInventory()
	{
		base.InitSyncObject(this.Pockets);
		this._Mirror_SyncVarHookDelegate_holdingItem = new Action<Item, Item>(this.OnHoldingItemChanged);
	}

	// Token: 0x06001300 RID: 4864 RVA: 0x00002321 File Offset: 0x00000521
	public override bool Weaved()
	{
		return true;
	}

	// Token: 0x170001B2 RID: 434
	// (get) Token: 0x06001301 RID: 4865 RVA: 0x000526CC File Offset: 0x000508CC
	// (set) Token: 0x06001302 RID: 4866 RVA: 0x000526EB File Offset: 0x000508EB
	public Item NetworkholdingItem
	{
		get
		{
			return base.GetSyncVarNetworkBehaviour<Item>(this.___holdingItemNetId, ref this.holdingItem);
		}
		[param: In]
		set
		{
			base.GeneratedSyncVarSetter_NetworkBehaviour<Item>(value, ref this.holdingItem, 1UL, this._Mirror_SyncVarHookDelegate_holdingItem, ref this.___holdingItemNetId);
		}
	}

	// Token: 0x06001303 RID: 4867 RVA: 0x00052710 File Offset: 0x00050910
	protected void UserCode_CmdSelectSlot__Int32(int slot)
	{
		if (!(this.NetworkholdingItem != null))
		{
			if (this.Pockets[slot] != null)
			{
				Item networkholdingItem = this.Pockets[slot];
				this.Pockets[slot] = null;
				this.NetworkholdingItem = networkholdingItem;
				this.NetworkholdingItem.ServerHandEnter(this);
			}
			return;
		}
		Action serverOnItemStash = this.ServerOnItemStash;
		if (serverOnItemStash != null)
		{
			serverOnItemStash();
		}
		if (this.Pockets[slot] == null)
		{
			this.Pockets[slot] = this.NetworkholdingItem;
			this.NetworkholdingItem = null;
			this.Pockets[slot].ServerHandExit(this);
			return;
		}
		SyncList<Item> pockets = this.Pockets;
		Item networkholdingItem2 = this.NetworkholdingItem;
		Item networkholdingItem3 = this.Pockets[slot];
		pockets[slot] = networkholdingItem2;
		this.NetworkholdingItem = networkholdingItem3;
		this.Pockets[slot].ServerHandExit(this);
		this.NetworkholdingItem.ServerHandEnter(this);
	}

	// Token: 0x06001304 RID: 4868 RVA: 0x0005280D File Offset: 0x00050A0D
	protected static void InvokeUserCode_CmdSelectSlot__Int32(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdSelectSlot called on client.");
			return;
		}
		((PlayerInventory)obj).UserCode_CmdSelectSlot__Int32(reader.ReadVarInt());
	}

	// Token: 0x06001305 RID: 4869 RVA: 0x00052836 File Offset: 0x00050A36
	protected void UserCode_RpcSetItemParent__Item__Boolean(Item item, bool active)
	{
		if (active)
		{
			item.transform.SetParent(this.handTransform);
			item.gameObject.SetActive(true);
			return;
		}
		item.gameObject.SetActive(false);
		item.transform.SetParent(this.pocketTransform);
	}

	// Token: 0x06001306 RID: 4870 RVA: 0x00052876 File Offset: 0x00050A76
	protected static void InvokeUserCode_RpcSetItemParent__Item__Boolean(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcSetItemParent called on server.");
			return;
		}
		((PlayerInventory)obj).UserCode_RpcSetItemParent__Item__Boolean(reader.ReadNetworkBehaviour<Item>(), reader.ReadBool());
	}

	// Token: 0x06001307 RID: 4871 RVA: 0x000528A5 File Offset: 0x00050AA5
	protected void UserCode_CmdOnItemThrown__Single__Item(float force, Item item)
	{
		this.RpcOnItemThrown(force, item);
	}

	// Token: 0x06001308 RID: 4872 RVA: 0x000528AF File Offset: 0x00050AAF
	protected static void InvokeUserCode_CmdOnItemThrown__Single__Item(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdOnItemThrown called on client.");
			return;
		}
		((PlayerInventory)obj).UserCode_CmdOnItemThrown__Single__Item(reader.ReadFloat(), reader.ReadNetworkBehaviour<Item>());
	}

	// Token: 0x06001309 RID: 4873 RVA: 0x000528DF File Offset: 0x00050ADF
	protected void UserCode_RpcOnItemThrown__Single__Item(float force, Item item)
	{
		if (base.isLocalPlayer)
		{
			return;
		}
		this.OnItemThrown(force, item);
	}

	// Token: 0x0600130A RID: 4874 RVA: 0x000528F2 File Offset: 0x00050AF2
	protected static void InvokeUserCode_RpcOnItemThrown__Single__Item(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcOnItemThrown called on server.");
			return;
		}
		((PlayerInventory)obj).UserCode_RpcOnItemThrown__Single__Item(reader.ReadFloat(), reader.ReadNetworkBehaviour<Item>());
	}

	// Token: 0x0600130B RID: 4875 RVA: 0x00052922 File Offset: 0x00050B22
	protected void UserCode_CmdThrowItem__Vector3__Vector3__Single__Single(Vector3 velocity, Vector3 direction, float force, float torque)
	{
		this.ServerThrowItem(velocity, direction, force, torque);
	}

	// Token: 0x0600130C RID: 4876 RVA: 0x0005292F File Offset: 0x00050B2F
	protected static void InvokeUserCode_CmdThrowItem__Vector3__Vector3__Single__Single(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdThrowItem called on client.");
			return;
		}
		((PlayerInventory)obj).UserCode_CmdThrowItem__Vector3__Vector3__Single__Single(reader.ReadVector3(), reader.ReadVector3(), reader.ReadFloat(), reader.ReadFloat());
	}

	// Token: 0x0600130D RID: 4877 RVA: 0x0005296C File Offset: 0x00050B6C
	static PlayerInventory()
	{
		RemoteProcedureCalls.RegisterCommand(typeof(PlayerInventory), "System.Void PlayerInventory::CmdSelectSlot(System.Int32)", new RemoteCallDelegate(PlayerInventory.InvokeUserCode_CmdSelectSlot__Int32), true);
		RemoteProcedureCalls.RegisterCommand(typeof(PlayerInventory), "System.Void PlayerInventory::CmdOnItemThrown(System.Single,Item)", new RemoteCallDelegate(PlayerInventory.InvokeUserCode_CmdOnItemThrown__Single__Item), true);
		RemoteProcedureCalls.RegisterCommand(typeof(PlayerInventory), "System.Void PlayerInventory::CmdThrowItem(UnityEngine.Vector3,UnityEngine.Vector3,System.Single,System.Single)", new RemoteCallDelegate(PlayerInventory.InvokeUserCode_CmdThrowItem__Vector3__Vector3__Single__Single), true);
		RemoteProcedureCalls.RegisterRpc(typeof(PlayerInventory), "System.Void PlayerInventory::RpcSetItemParent(Item,System.Boolean)", new RemoteCallDelegate(PlayerInventory.InvokeUserCode_RpcSetItemParent__Item__Boolean));
		RemoteProcedureCalls.RegisterRpc(typeof(PlayerInventory), "System.Void PlayerInventory::RpcOnItemThrown(System.Single,Item)", new RemoteCallDelegate(PlayerInventory.InvokeUserCode_RpcOnItemThrown__Single__Item));
	}

	// Token: 0x0600130E RID: 4878 RVA: 0x00052A1C File Offset: 0x00050C1C
	public override void SerializeSyncVars(NetworkWriter writer, bool forceAll)
	{
		base.SerializeSyncVars(writer, forceAll);
		if (forceAll)
		{
			writer.WriteNetworkBehaviour(this.NetworkholdingItem);
			return;
		}
		writer.WriteVarULong(this.syncVarDirtyBits);
		if ((this.syncVarDirtyBits & 1UL) != 0UL)
		{
			writer.WriteNetworkBehaviour(this.NetworkholdingItem);
		}
	}

	// Token: 0x0600130F RID: 4879 RVA: 0x00052A74 File Offset: 0x00050C74
	public override void DeserializeSyncVars(NetworkReader reader, bool initialState)
	{
		base.DeserializeSyncVars(reader, initialState);
		if (initialState)
		{
			base.GeneratedSyncVarDeserialize_NetworkBehaviour<Item>(ref this.holdingItem, this._Mirror_SyncVarHookDelegate_holdingItem, reader, ref this.___holdingItemNetId);
			return;
		}
		long num = (long)reader.ReadVarULong();
		if ((num & 1L) != 0L)
		{
			base.GeneratedSyncVarDeserialize_NetworkBehaviour<Item>(ref this.holdingItem, this._Mirror_SyncVarHookDelegate_holdingItem, reader, ref this.___holdingItemNetId);
		}
	}

	// Token: 0x04000C0E RID: 3086
	[Header("References")]
	public Transform handTransform;

	// Token: 0x04000C0F RID: 3087
	public Transform pocketTransform;

	// Token: 0x04000C10 RID: 3088
	public Transform throwPosition;

	// Token: 0x04000C11 RID: 3089
	[Tooltip("Optional. Player hand rig to hide when holding an item that has its own itemHandRig.")]
	[SerializeField]
	private GameObject playerHands;

	// Token: 0x04000C12 RID: 3090
	[SyncVar(hook = "OnHoldingItemChanged")]
	[CanBeNull]
	public Item holdingItem;

	// Token: 0x04000C13 RID: 3091
	[ItemCanBeNull]
	public readonly SyncList<Item> Pockets = new SyncList<Item>();

	// Token: 0x04000C14 RID: 3092
	[Header("Settings")]
	public uint inventorySlotCount;

	// Token: 0x04000C15 RID: 3093
	private PlayerSettings _ps;

	// Token: 0x04000C16 RID: 3094
	private PlayerController _pc;

	// Token: 0x04000C17 RID: 3095
	private Rigidbody _rigidbody;

	// Token: 0x04000C18 RID: 3096
	private Coroutine _throwRoutine;

	// Token: 0x04000C19 RID: 3097
	private float _currentThrowPercentage;

	// Token: 0x04000C1A RID: 3098
	private bool _isThrowCancelled;

	// Token: 0x04000C1B RID: 3099
	private bool _localAlreadyThrown;

	// Token: 0x04000C21 RID: 3105
	public GameEvent localOnInventoryUpdate;

	// Token: 0x04000C22 RID: 3106
	[SerializeField]
	private SFXLoopComponent throwSfxLoopComponent;

	// Token: 0x04000C23 RID: 3107
	protected NetworkBehaviourSyncVar ___holdingItemNetId;

	// Token: 0x04000C24 RID: 3108
	public Action<Item, Item> _Mirror_SyncVarHookDelegate_holdingItem;
}
