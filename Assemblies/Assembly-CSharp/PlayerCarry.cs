using System;
using Mirror;
using Mirror.RemoteCalls;
using UnityEngine;

// Token: 0x020001F6 RID: 502
public class PlayerCarry : Item
{
	// Token: 0x060011EC RID: 4588 RVA: 0x0004D829 File Offset: 0x0004BA29
	protected override void OnAwake()
	{
		base.OnAwake();
		this._pc = base.GetComponent<PlayerController>();
	}

	// Token: 0x060011ED RID: 4589 RVA: 0x0004D83D File Offset: 0x0004BA3D
	private void Start()
	{
		this.IsInteractable = (!this._pc.hasBody && !base.NetworkHolder);
	}

	// Token: 0x060011EE RID: 4590 RVA: 0x0004D863 File Offset: 0x0004BA63
	protected override void OnPickedUp(PlayerInventory playerInventory)
	{
		base.OnPickedUp(playerInventory);
		this.RpcOnPickedUp(playerInventory.GetComponent<PlayerCarry>());
	}

	// Token: 0x060011EF RID: 4591 RVA: 0x0004D878 File Offset: 0x0004BA78
	[ClientRpc]
	private void RpcOnPickedUp(PlayerCarry holderCarry)
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		writer.WriteNetworkBehaviour(holderCarry);
		this.SendRPCInternal("System.Void PlayerCarry::RpcOnPickedUp(PlayerCarry)", -1743165390, writer, 0, true);
		NetworkWriterPool.Return(writer);
	}

	// Token: 0x060011F0 RID: 4592 RVA: 0x0004D8B2 File Offset: 0x0004BAB2
	protected override void OnDropped(PlayerInventory playerInventory)
	{
		base.OnDropped(playerInventory);
		this.TargetOnDropped(base.connectionToClient, playerInventory.GetComponent<PlayerCarry>());
	}

	// Token: 0x060011F1 RID: 4593 RVA: 0x0004D8D0 File Offset: 0x0004BAD0
	[TargetRpc]
	private void TargetOnDropped(NetworkConnection conn, PlayerCarry holderCarry)
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		writer.WriteNetworkBehaviour(holderCarry);
		this.SendTargetRPCInternal(conn, "System.Void PlayerCarry::TargetOnDropped(Mirror.NetworkConnection,PlayerCarry)", -2013112336, writer, 0);
		NetworkWriterPool.Return(writer);
	}

	// Token: 0x060011F2 RID: 4594 RVA: 0x0004D90A File Offset: 0x0004BB0A
	public override void ServerThrow(Vector3 position, Quaternion rotation, Vector3 force, Vector3 torque)
	{
		if (base.NetworkHolder)
		{
			base.ServerDrop();
		}
		this.TargetOnThrow(base.connectionToClient, force, torque);
	}

	// Token: 0x060011F3 RID: 4595 RVA: 0x0004D930 File Offset: 0x0004BB30
	[TargetRpc]
	private void TargetOnThrow(NetworkConnection conn, Vector3 force, Vector3 torque)
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		writer.WriteVector3(force);
		writer.WriteVector3(torque);
		this.SendTargetRPCInternal(conn, "System.Void PlayerCarry::TargetOnThrow(Mirror.NetworkConnection,UnityEngine.Vector3,UnityEngine.Vector3)", 1675301902, writer, 0);
		NetworkWriterPool.Return(writer);
	}

	// Token: 0x060011F4 RID: 4596 RVA: 0x0004D974 File Offset: 0x0004BB74
	public void LocalSetInteractable(bool isInteractable)
	{
		this.CmdSetInteractable(isInteractable);
	}

	// Token: 0x060011F5 RID: 4597 RVA: 0x0004D97D File Offset: 0x0004BB7D
	public bool TryGetHolderInventory(out PlayerInventory holderInventory)
	{
		holderInventory = base.NetworkHolder;
		return holderInventory;
	}

	// Token: 0x060011F6 RID: 4598 RVA: 0x0004D990 File Offset: 0x0004BB90
	[Command]
	private void CmdSetInteractable(bool isInteractable)
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		writer.WriteBool(isInteractable);
		base.SendCommandInternal("System.Void PlayerCarry::CmdSetInteractable(System.Boolean)", -737844972, writer, 0, true);
		NetworkWriterPool.Return(writer);
	}

	// Token: 0x060011F7 RID: 4599 RVA: 0x0004D9CC File Offset: 0x0004BBCC
	[ClientRpc]
	private void RpcSetInteractable(bool isInteractable)
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		writer.WriteBool(isInteractable);
		this.SendRPCInternal("System.Void PlayerCarry::RpcSetInteractable(System.Boolean)", -570291399, writer, 0, true);
		NetworkWriterPool.Return(writer);
	}

	// Token: 0x060011F9 RID: 4601 RVA: 0x00002321 File Offset: 0x00000521
	public override bool Weaved()
	{
		return true;
	}

	// Token: 0x060011FA RID: 4602 RVA: 0x0004DA06 File Offset: 0x0004BC06
	protected void UserCode_RpcOnPickedUp__PlayerCarry(PlayerCarry holderCarry)
	{
		this.phba.LocalResetHands();
		if (base.isLocalPlayer)
		{
			this._pc.State = PlayerController.PlayerState.Locked;
			holderCarry.MeetRequirements = false;
			holderCarry.GetComponentInChildren<SpringPositionFollower>().enabled = false;
			this.handSpring.enabled = false;
		}
	}

	// Token: 0x060011FB RID: 4603 RVA: 0x0004DA46 File Offset: 0x0004BC46
	protected static void InvokeUserCode_RpcOnPickedUp__PlayerCarry(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcOnPickedUp called on server.");
			return;
		}
		((PlayerCarry)obj).UserCode_RpcOnPickedUp__PlayerCarry(reader.ReadNetworkBehaviour<PlayerCarry>());
	}

	// Token: 0x060011FC RID: 4604 RVA: 0x0004DA6F File Offset: 0x0004BC6F
	protected void UserCode_TargetOnDropped__NetworkConnection__PlayerCarry(NetworkConnection conn, PlayerCarry holderCarry)
	{
		this._pc.State = PlayerController.PlayerState.Ragdoll;
		holderCarry.MeetRequirements = true;
		holderCarry.GetComponentInChildren<SpringPositionFollower>().enabled = true;
		this.handSpring.enabled = true;
	}

	// Token: 0x060011FD RID: 4605 RVA: 0x0004DA9C File Offset: 0x0004BC9C
	protected static void InvokeUserCode_TargetOnDropped__NetworkConnection__PlayerCarry(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("TargetRPC TargetOnDropped called on server.");
			return;
		}
		((PlayerCarry)obj).UserCode_TargetOnDropped__NetworkConnection__PlayerCarry(null, reader.ReadNetworkBehaviour<PlayerCarry>());
	}

	// Token: 0x060011FE RID: 4606 RVA: 0x0004DAC6 File Offset: 0x0004BCC6
	protected void UserCode_TargetOnThrow__NetworkConnection__Vector3__Vector3(NetworkConnection conn, Vector3 force, Vector3 torque)
	{
		this.Rb.AddForce(force, ForceMode.VelocityChange);
		this.Rb.AddTorque(torque, ForceMode.VelocityChange);
	}

	// Token: 0x060011FF RID: 4607 RVA: 0x0004DAE2 File Offset: 0x0004BCE2
	protected static void InvokeUserCode_TargetOnThrow__NetworkConnection__Vector3__Vector3(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("TargetRPC TargetOnThrow called on server.");
			return;
		}
		((PlayerCarry)obj).UserCode_TargetOnThrow__NetworkConnection__Vector3__Vector3(null, reader.ReadVector3(), reader.ReadVector3());
	}

	// Token: 0x06001200 RID: 4608 RVA: 0x0004DB12 File Offset: 0x0004BD12
	protected void UserCode_CmdSetInteractable__Boolean(bool isInteractable)
	{
		this.RpcSetInteractable(isInteractable);
	}

	// Token: 0x06001201 RID: 4609 RVA: 0x0004DB1B File Offset: 0x0004BD1B
	protected static void InvokeUserCode_CmdSetInteractable__Boolean(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdSetInteractable called on client.");
			return;
		}
		((PlayerCarry)obj).UserCode_CmdSetInteractable__Boolean(reader.ReadBool());
	}

	// Token: 0x06001202 RID: 4610 RVA: 0x0004DB44 File Offset: 0x0004BD44
	protected void UserCode_RpcSetInteractable__Boolean(bool isInteractable)
	{
		this.InteractableName = base.GetComponent<PlayerProfile>().playerName;
		this.IsInteractable = isInteractable;
		this.CursorType = ((isInteractable && this.MeetRequirements) ? CursorManager.CursorType.Interact : CursorManager.CursorType.Default);
	}

	// Token: 0x06001203 RID: 4611 RVA: 0x0004DB73 File Offset: 0x0004BD73
	protected static void InvokeUserCode_RpcSetInteractable__Boolean(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcSetInteractable called on server.");
			return;
		}
		((PlayerCarry)obj).UserCode_RpcSetInteractable__Boolean(reader.ReadBool());
	}

	// Token: 0x06001204 RID: 4612 RVA: 0x0004DB9C File Offset: 0x0004BD9C
	static PlayerCarry()
	{
		RemoteProcedureCalls.RegisterCommand(typeof(PlayerCarry), "System.Void PlayerCarry::CmdSetInteractable(System.Boolean)", new RemoteCallDelegate(PlayerCarry.InvokeUserCode_CmdSetInteractable__Boolean), true);
		RemoteProcedureCalls.RegisterRpc(typeof(PlayerCarry), "System.Void PlayerCarry::RpcOnPickedUp(PlayerCarry)", new RemoteCallDelegate(PlayerCarry.InvokeUserCode_RpcOnPickedUp__PlayerCarry));
		RemoteProcedureCalls.RegisterRpc(typeof(PlayerCarry), "System.Void PlayerCarry::RpcSetInteractable(System.Boolean)", new RemoteCallDelegate(PlayerCarry.InvokeUserCode_RpcSetInteractable__Boolean));
		RemoteProcedureCalls.RegisterRpc(typeof(PlayerCarry), "System.Void PlayerCarry::TargetOnDropped(Mirror.NetworkConnection,PlayerCarry)", new RemoteCallDelegate(PlayerCarry.InvokeUserCode_TargetOnDropped__NetworkConnection__PlayerCarry));
		RemoteProcedureCalls.RegisterRpc(typeof(PlayerCarry), "System.Void PlayerCarry::TargetOnThrow(Mirror.NetworkConnection,UnityEngine.Vector3,UnityEngine.Vector3)", new RemoteCallDelegate(PlayerCarry.InvokeUserCode_TargetOnThrow__NetworkConnection__Vector3__Vector3));
	}

	// Token: 0x04000BA4 RID: 2980
	private PlayerController _pc;

	// Token: 0x04000BA5 RID: 2981
	[SerializeField]
	private SpringPositionFollower handSpring;

	// Token: 0x04000BA6 RID: 2982
	[SerializeField]
	private PlayerHandButtonAnimation phba;
}
