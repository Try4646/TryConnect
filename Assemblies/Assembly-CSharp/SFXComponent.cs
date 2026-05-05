using System;
using FMODUnity;
using Mirror;
using Mirror.RemoteCalls;
using UnityEngine;

// Token: 0x02000279 RID: 633
public class SFXComponent : NetworkBehaviour
{
	// Token: 0x06001682 RID: 5762 RVA: 0x000605A7 File Offset: 0x0005E7A7
	public void PlayOneShotWith3DPos()
	{
		if (this.eventReference.IsNull)
		{
			return;
		}
		SFXManager.SFXOneShot(this.eventReference, base.gameObject.transform.position);
	}

	// Token: 0x06001683 RID: 5763 RVA: 0x000605D2 File Offset: 0x0005E7D2
	public void PlayOneShotWithCustom3DPos(Vector3 pos)
	{
		if (this.eventReference.IsNull)
		{
			return;
		}
		SFXManager.SFXOneShot(this.eventReference, pos);
	}

	// Token: 0x06001684 RID: 5764 RVA: 0x000605EE File Offset: 0x0005E7EE
	public void PlayOneShotOverrideParams(float _value = 0f)
	{
		if (this.eventReference.IsNull)
		{
			return;
		}
		SFXManager.SFXOneShotWithParameters(this.eventReference, this.fmodParams, base.gameObject.transform.position, 1f);
	}

	// Token: 0x06001685 RID: 5765 RVA: 0x00060624 File Offset: 0x0005E824
	public void PlayOneShotOverrideParamsWithCustomPos(Vector3 pos)
	{
		if (this.eventReference.IsNull)
		{
			return;
		}
		SFXManager.SFXOneShotWithParameters(this.eventReference, this.fmodParams, pos, 1f);
	}

	// Token: 0x06001686 RID: 5766 RVA: 0x0006064B File Offset: 0x0005E84B
	public void PlayOneShotAttached()
	{
		if (this.eventReference.IsNull)
		{
			return;
		}
		SFXManager.SFXOneShot3DAttached(this.eventReference, base.gameObject, false);
	}

	// Token: 0x06001687 RID: 5767 RVA: 0x0006066D File Offset: 0x0005E86D
	public void PlayOneShotAttachedOverrideParameters()
	{
		if (this.eventReference.IsNull)
		{
			return;
		}
		SFXManager.SFXOneShot3DAttachedWithParameters(this.eventReference, this.fmodParams, base.gameObject, false);
	}

	// Token: 0x06001688 RID: 5768 RVA: 0x00060698 File Offset: 0x0005E898
	[ClientRpc]
	public void RpcPlayOneShotAttached()
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		this.SendRPCInternal("System.Void SFXComponent::RpcPlayOneShotAttached()", -1900197242, writer, 0, true);
		NetworkWriterPool.Return(writer);
	}

	// Token: 0x06001689 RID: 5769 RVA: 0x000606C8 File Offset: 0x0005E8C8
	[ClientRpc]
	public void RpcPlayOneShotWith3DPos()
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		this.SendRPCInternal("System.Void SFXComponent::RpcPlayOneShotWith3DPos()", -1364297797, writer, 0, true);
		NetworkWriterPool.Return(writer);
	}

	// Token: 0x0600168A RID: 5770 RVA: 0x000606F8 File Offset: 0x0005E8F8
	[ClientRpc]
	public void RpcPlayOneShotWithCustom3DPos(Vector3 pos)
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		writer.WriteVector3(pos);
		this.SendRPCInternal("System.Void SFXComponent::RpcPlayOneShotWithCustom3DPos(UnityEngine.Vector3)", -510311345, writer, 0, true);
		NetworkWriterPool.Return(writer);
	}

	// Token: 0x0600168B RID: 5771 RVA: 0x00060734 File Offset: 0x0005E934
	[ClientRpc]
	public void RpcPlayerInteractOneShotOnlyOnClient(PlayerInteract playerInteract)
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		writer.WriteNetworkBehaviour(playerInteract);
		this.SendRPCInternal("System.Void SFXComponent::RpcPlayerInteractOneShotOnlyOnClient(PlayerInteract)", 653552140, writer, 0, true);
		NetworkWriterPool.Return(writer);
	}

	// Token: 0x0600168D RID: 5773 RVA: 0x00002321 File Offset: 0x00000521
	public override bool Weaved()
	{
		return true;
	}

	// Token: 0x0600168E RID: 5774 RVA: 0x0006076E File Offset: 0x0005E96E
	protected void UserCode_RpcPlayOneShotAttached()
	{
		this.PlayOneShotAttached();
	}

	// Token: 0x0600168F RID: 5775 RVA: 0x00060776 File Offset: 0x0005E976
	protected static void InvokeUserCode_RpcPlayOneShotAttached(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcPlayOneShotAttached called on server.");
			return;
		}
		((SFXComponent)obj).UserCode_RpcPlayOneShotAttached();
	}

	// Token: 0x06001690 RID: 5776 RVA: 0x00060799 File Offset: 0x0005E999
	protected void UserCode_RpcPlayOneShotWith3DPos()
	{
		this.PlayOneShotWith3DPos();
	}

	// Token: 0x06001691 RID: 5777 RVA: 0x000607A1 File Offset: 0x0005E9A1
	protected static void InvokeUserCode_RpcPlayOneShotWith3DPos(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcPlayOneShotWith3DPos called on server.");
			return;
		}
		((SFXComponent)obj).UserCode_RpcPlayOneShotWith3DPos();
	}

	// Token: 0x06001692 RID: 5778 RVA: 0x000607C4 File Offset: 0x0005E9C4
	protected void UserCode_RpcPlayOneShotWithCustom3DPos__Vector3(Vector3 pos)
	{
		this.PlayOneShotWithCustom3DPos(pos);
	}

	// Token: 0x06001693 RID: 5779 RVA: 0x000607CD File Offset: 0x0005E9CD
	protected static void InvokeUserCode_RpcPlayOneShotWithCustom3DPos__Vector3(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcPlayOneShotWithCustom3DPos called on server.");
			return;
		}
		((SFXComponent)obj).UserCode_RpcPlayOneShotWithCustom3DPos__Vector3(reader.ReadVector3());
	}

	// Token: 0x06001694 RID: 5780 RVA: 0x000607F6 File Offset: 0x0005E9F6
	protected void UserCode_RpcPlayerInteractOneShotOnlyOnClient__PlayerInteract(PlayerInteract playerInteract)
	{
		if (playerInteract == null)
		{
			return;
		}
		if (playerInteract.isLocalPlayer)
		{
			this.PlayOneShotWith3DPos();
		}
	}

	// Token: 0x06001695 RID: 5781 RVA: 0x00060810 File Offset: 0x0005EA10
	protected static void InvokeUserCode_RpcPlayerInteractOneShotOnlyOnClient__PlayerInteract(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcPlayerInteractOneShotOnlyOnClient called on server.");
			return;
		}
		((SFXComponent)obj).UserCode_RpcPlayerInteractOneShotOnlyOnClient__PlayerInteract(reader.ReadNetworkBehaviour<PlayerInteract>());
	}

	// Token: 0x06001696 RID: 5782 RVA: 0x0006083C File Offset: 0x0005EA3C
	static SFXComponent()
	{
		RemoteProcedureCalls.RegisterRpc(typeof(SFXComponent), "System.Void SFXComponent::RpcPlayOneShotAttached()", new RemoteCallDelegate(SFXComponent.InvokeUserCode_RpcPlayOneShotAttached));
		RemoteProcedureCalls.RegisterRpc(typeof(SFXComponent), "System.Void SFXComponent::RpcPlayOneShotWith3DPos()", new RemoteCallDelegate(SFXComponent.InvokeUserCode_RpcPlayOneShotWith3DPos));
		RemoteProcedureCalls.RegisterRpc(typeof(SFXComponent), "System.Void SFXComponent::RpcPlayOneShotWithCustom3DPos(UnityEngine.Vector3)", new RemoteCallDelegate(SFXComponent.InvokeUserCode_RpcPlayOneShotWithCustom3DPos__Vector3));
		RemoteProcedureCalls.RegisterRpc(typeof(SFXComponent), "System.Void SFXComponent::RpcPlayerInteractOneShotOnlyOnClient(PlayerInteract)", new RemoteCallDelegate(SFXComponent.InvokeUserCode_RpcPlayerInteractOneShotOnlyOnClient__PlayerInteract));
	}

	// Token: 0x04000EB1 RID: 3761
	[SerializeField]
	private EventReference eventReference;

	// Token: 0x04000EB2 RID: 3762
	public SFXParams[] fmodParams;
}
