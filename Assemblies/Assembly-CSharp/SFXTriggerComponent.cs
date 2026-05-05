using System;
using FMODUnity;
using Mirror;
using Mirror.RemoteCalls;
using UnityEngine;

// Token: 0x02000283 RID: 643
public class SFXTriggerComponent : NetworkBehaviour
{
	// Token: 0x060016E4 RID: 5860 RVA: 0x000618C1 File Offset: 0x0005FAC1
	private void Awake()
	{
		this._collider = base.GetComponent<Collider>();
	}

	// Token: 0x060016E5 RID: 5861 RVA: 0x000618D0 File Offset: 0x0005FAD0
	private void OnTriggerEnter(Collider other)
	{
		if (!base.isActiveAndEnabled)
		{
			return;
		}
		if (Time.time < this._saved_cooldown)
		{
			return;
		}
		if (!this.IsLayerAllowed(other.gameObject.layer))
		{
			return;
		}
		if (this.eventReference.IsNull)
		{
			return;
		}
		if (this.clientOnly || other.gameObject.layer != this.playerLayer)
		{
			if (other.gameObject.layer == this.playerLayer && other.attachedRigidbody.GetComponent<NetworkIdentity>().isLocalPlayer)
			{
				this.TriggerEnterEvent();
				if (this.disableColliderAfterTrigger)
				{
					this._collider.enabled = false;
				}
			}
			return;
		}
		if (!base.isServer)
		{
			return;
		}
		this.CmdTriggerServerEvent();
		if (this.disableColliderAfterTrigger)
		{
			this._collider.enabled = false;
		}
	}

	// Token: 0x060016E6 RID: 5862 RVA: 0x00061994 File Offset: 0x0005FB94
	private void OnTriggerExit(Collider other)
	{
		if (!base.isActiveAndEnabled)
		{
			return;
		}
		if (!this.useTriggerExit)
		{
			return;
		}
		if (Time.time < this._saved_cooldown)
		{
			return;
		}
		if (!this.IsLayerAllowed(other.gameObject.layer))
		{
			return;
		}
		if (this.exitEventReference.IsNull)
		{
			return;
		}
		if (this.clientOnly || other.gameObject.layer != this.playerLayer)
		{
			if (other.gameObject.layer == this.playerLayer)
			{
				if (!other.attachedRigidbody.GetComponent<NetworkIdentity>().isLocalPlayer)
				{
					return;
				}
				this.TriggerExitEvent();
				if (this.disableColliderAfterTrigger)
				{
					this._collider.enabled = false;
				}
			}
			return;
		}
		if (!base.isServer)
		{
			return;
		}
		this.CmdTriggerServerExitEvent();
		if (this.disableColliderAfterTrigger)
		{
			this._collider.enabled = false;
		}
	}

	// Token: 0x060016E7 RID: 5863 RVA: 0x00061A62 File Offset: 0x0005FC62
	private bool IsLayerAllowed(int layer)
	{
		return (this.allowedLayers & 1 << layer) != 0;
	}

	// Token: 0x060016E8 RID: 5864 RVA: 0x00061A7C File Offset: 0x0005FC7C
	[Command(requiresAuthority = false)]
	private void CmdTriggerServerEvent()
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		base.SendCommandInternal("System.Void SFXTriggerComponent::CmdTriggerServerEvent()", 657598474, writer, 0, false);
		NetworkWriterPool.Return(writer);
	}

	// Token: 0x060016E9 RID: 5865 RVA: 0x00061AAC File Offset: 0x0005FCAC
	[ClientRpc]
	private void RpcTriggerServerEvent()
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		this.SendRPCInternal("System.Void SFXTriggerComponent::RpcTriggerServerEvent()", -668821287, writer, 0, true);
		NetworkWriterPool.Return(writer);
	}

	// Token: 0x060016EA RID: 5866 RVA: 0x00061ADC File Offset: 0x0005FCDC
	private void TriggerEnterEvent()
	{
		Vector3 pos = (this.customPosition == null) ? base.transform.position : this.customPosition.position;
		SFXManager.SFXOneShot(this.eventReference, pos);
		this._saved_cooldown = Time.time + this.cooldownTime;
	}

	// Token: 0x060016EB RID: 5867 RVA: 0x00061B30 File Offset: 0x0005FD30
	[Command(requiresAuthority = false)]
	private void CmdTriggerServerExitEvent()
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		base.SendCommandInternal("System.Void SFXTriggerComponent::CmdTriggerServerExitEvent()", -1927226966, writer, 0, false);
		NetworkWriterPool.Return(writer);
	}

	// Token: 0x060016EC RID: 5868 RVA: 0x00061B60 File Offset: 0x0005FD60
	[ClientRpc]
	private void RpcTriggerServerExitEvent()
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		this.SendRPCInternal("System.Void SFXTriggerComponent::RpcTriggerServerExitEvent()", -731581903, writer, 0, true);
		NetworkWriterPool.Return(writer);
	}

	// Token: 0x060016ED RID: 5869 RVA: 0x00061B90 File Offset: 0x0005FD90
	private void TriggerExitEvent()
	{
		Vector3 pos = (this.customPosition == null) ? base.transform.position : this.customPosition.position;
		SFXManager.SFXOneShot(this.exitEventReference, pos);
	}

	// Token: 0x060016EF RID: 5871 RVA: 0x00002321 File Offset: 0x00000521
	public override bool Weaved()
	{
		return true;
	}

	// Token: 0x060016F0 RID: 5872 RVA: 0x00061BF1 File Offset: 0x0005FDF1
	protected void UserCode_CmdTriggerServerEvent()
	{
		this.RpcTriggerServerEvent();
	}

	// Token: 0x060016F1 RID: 5873 RVA: 0x00061BF9 File Offset: 0x0005FDF9
	protected static void InvokeUserCode_CmdTriggerServerEvent(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdTriggerServerEvent called on client.");
			return;
		}
		((SFXTriggerComponent)obj).UserCode_CmdTriggerServerEvent();
	}

	// Token: 0x060016F2 RID: 5874 RVA: 0x00061C1C File Offset: 0x0005FE1C
	protected void UserCode_RpcTriggerServerEvent()
	{
		this.TriggerEnterEvent();
	}

	// Token: 0x060016F3 RID: 5875 RVA: 0x00061C24 File Offset: 0x0005FE24
	protected static void InvokeUserCode_RpcTriggerServerEvent(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcTriggerServerEvent called on server.");
			return;
		}
		((SFXTriggerComponent)obj).UserCode_RpcTriggerServerEvent();
	}

	// Token: 0x060016F4 RID: 5876 RVA: 0x00061C47 File Offset: 0x0005FE47
	protected void UserCode_CmdTriggerServerExitEvent()
	{
		this.RpcTriggerServerExitEvent();
	}

	// Token: 0x060016F5 RID: 5877 RVA: 0x00061C4F File Offset: 0x0005FE4F
	protected static void InvokeUserCode_CmdTriggerServerExitEvent(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdTriggerServerExitEvent called on client.");
			return;
		}
		((SFXTriggerComponent)obj).UserCode_CmdTriggerServerExitEvent();
	}

	// Token: 0x060016F6 RID: 5878 RVA: 0x00061C72 File Offset: 0x0005FE72
	protected void UserCode_RpcTriggerServerExitEvent()
	{
		this.TriggerExitEvent();
	}

	// Token: 0x060016F7 RID: 5879 RVA: 0x00061C7A File Offset: 0x0005FE7A
	protected static void InvokeUserCode_RpcTriggerServerExitEvent(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcTriggerServerExitEvent called on server.");
			return;
		}
		((SFXTriggerComponent)obj).UserCode_RpcTriggerServerExitEvent();
	}

	// Token: 0x060016F8 RID: 5880 RVA: 0x00061CA0 File Offset: 0x0005FEA0
	static SFXTriggerComponent()
	{
		RemoteProcedureCalls.RegisterCommand(typeof(SFXTriggerComponent), "System.Void SFXTriggerComponent::CmdTriggerServerEvent()", new RemoteCallDelegate(SFXTriggerComponent.InvokeUserCode_CmdTriggerServerEvent), false);
		RemoteProcedureCalls.RegisterCommand(typeof(SFXTriggerComponent), "System.Void SFXTriggerComponent::CmdTriggerServerExitEvent()", new RemoteCallDelegate(SFXTriggerComponent.InvokeUserCode_CmdTriggerServerExitEvent), false);
		RemoteProcedureCalls.RegisterRpc(typeof(SFXTriggerComponent), "System.Void SFXTriggerComponent::RpcTriggerServerEvent()", new RemoteCallDelegate(SFXTriggerComponent.InvokeUserCode_RpcTriggerServerEvent));
		RemoteProcedureCalls.RegisterRpc(typeof(SFXTriggerComponent), "System.Void SFXTriggerComponent::RpcTriggerServerExitEvent()", new RemoteCallDelegate(SFXTriggerComponent.InvokeUserCode_RpcTriggerServerExitEvent));
	}

	// Token: 0x04000EEA RID: 3818
	[SerializeField]
	private Transform customPosition;

	// Token: 0x04000EEB RID: 3819
	[Header("Trigger Enter")]
	[SerializeField]
	private EventReference eventReference;

	// Token: 0x04000EEC RID: 3820
	[SerializeField]
	private LayerMask allowedLayers;

	// Token: 0x04000EED RID: 3821
	[SerializeField]
	private float cooldownTime = 1f;

	// Token: 0x04000EEE RID: 3822
	private float _saved_cooldown;

	// Token: 0x04000EEF RID: 3823
	[SerializeField]
	private bool clientOnly = true;

	// Token: 0x04000EF0 RID: 3824
	[SerializeField]
	private bool disableColliderAfterTrigger;

	// Token: 0x04000EF1 RID: 3825
	private int playerLayer = 6;

	// Token: 0x04000EF2 RID: 3826
	[Header("Trigger Exit")]
	[SerializeField]
	private bool useTriggerExit;

	// Token: 0x04000EF3 RID: 3827
	[SerializeField]
	private EventReference exitEventReference;

	// Token: 0x04000EF4 RID: 3828
	private Collider _collider;
}
