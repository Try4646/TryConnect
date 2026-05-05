using System;
using FMODUnity;
using Mirror;
using Mirror.RemoteCalls;
using UnityEngine;
using UnityEngine.Events;

// Token: 0x0200027A RID: 634
public class SFXGenericButtonComponent : NetworkBehaviour
{
	// Token: 0x06001697 RID: 5783 RVA: 0x000608C9 File Offset: 0x0005EAC9
	private void Awake()
	{
		this.interactableEventTrigger = base.GetComponent<InteractableEventTrigger>();
	}

	// Token: 0x06001698 RID: 5784 RVA: 0x000608D7 File Offset: 0x0005EAD7
	private void OnEnable()
	{
		this.interactableEventTrigger.serverOnInteractEvent.AddListener(new UnityAction<PlayerInteract>(this.RpcPlayButtonSFX));
	}

	// Token: 0x06001699 RID: 5785 RVA: 0x000608F5 File Offset: 0x0005EAF5
	private void OnDisable()
	{
		this.interactableEventTrigger.serverOnInteractEvent.RemoveListener(new UnityAction<PlayerInteract>(this.RpcPlayButtonSFX));
	}

	// Token: 0x0600169A RID: 5786 RVA: 0x00060914 File Offset: 0x0005EB14
	[ClientRpc]
	private void RpcPlayButtonSFX(PlayerInteract _playerInteract)
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		writer.WriteNetworkBehaviour(_playerInteract);
		this.SendRPCInternal("System.Void SFXGenericButtonComponent::RpcPlayButtonSFX(PlayerInteract)", 1731324265, writer, 0, true);
		NetworkWriterPool.Return(writer);
	}

	// Token: 0x0600169C RID: 5788 RVA: 0x00002321 File Offset: 0x00000521
	public override bool Weaved()
	{
		return true;
	}

	// Token: 0x0600169D RID: 5789 RVA: 0x0006094E File Offset: 0x0005EB4E
	protected void UserCode_RpcPlayButtonSFX__PlayerInteract(PlayerInteract _playerInteract)
	{
		SFXManager.SFXOneShot(this.buttonSFX, base.transform.position);
	}

	// Token: 0x0600169E RID: 5790 RVA: 0x00060966 File Offset: 0x0005EB66
	protected static void InvokeUserCode_RpcPlayButtonSFX__PlayerInteract(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcPlayButtonSFX called on server.");
			return;
		}
		((SFXGenericButtonComponent)obj).UserCode_RpcPlayButtonSFX__PlayerInteract(reader.ReadNetworkBehaviour<PlayerInteract>());
	}

	// Token: 0x0600169F RID: 5791 RVA: 0x0006098F File Offset: 0x0005EB8F
	static SFXGenericButtonComponent()
	{
		RemoteProcedureCalls.RegisterRpc(typeof(SFXGenericButtonComponent), "System.Void SFXGenericButtonComponent::RpcPlayButtonSFX(PlayerInteract)", new RemoteCallDelegate(SFXGenericButtonComponent.InvokeUserCode_RpcPlayButtonSFX__PlayerInteract));
	}

	// Token: 0x04000EB3 RID: 3763
	[SerializeField]
	private EventReference buttonSFX;

	// Token: 0x04000EB4 RID: 3764
	private InteractableEventTrigger interactableEventTrigger;
}
