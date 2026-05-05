using System;
using Mirror;
using Mirror.RemoteCalls;
using MoreMountains.Feedbacks;
using UnityEngine;

// Token: 0x0200006C RID: 108
public class PlinkoPocket : NetworkBehaviour
{
	// Token: 0x060003BF RID: 959 RVA: 0x00011818 File Offset: 0x0000FA18
	private void OnTriggerEnter(Collider other)
	{
		if (!base.isServer)
		{
			return;
		}
		if (!other.attachedRigidbody)
		{
			return;
		}
		PlinkoPuck plinkoPuck;
		if (!other.attachedRigidbody.TryGetComponent<PlinkoPuck>(out plinkoPuck))
		{
			return;
		}
		if (plinkoPuck.hasTouched)
		{
			return;
		}
		plinkoPuck.hasTouched = true;
		this.plinkoGame.OnPuckEnteredPocket(this.slotIndex, plinkoPuck);
		NetworkServer.Destroy(plinkoPuck.gameObject);
		this.RpcPlayFeedbacks();
	}

	// Token: 0x060003C0 RID: 960 RVA: 0x00011880 File Offset: 0x0000FA80
	[ClientRpc]
	private void RpcPlayFeedbacks()
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		this.SendRPCInternal("System.Void PlinkoPocket::RpcPlayFeedbacks()", -2139126725, writer, 0, true);
		NetworkWriterPool.Return(writer);
	}

	// Token: 0x060003C2 RID: 962 RVA: 0x00002321 File Offset: 0x00000521
	public override bool Weaved()
	{
		return true;
	}

	// Token: 0x060003C3 RID: 963 RVA: 0x000118B8 File Offset: 0x0000FAB8
	protected void UserCode_RpcPlayFeedbacks()
	{
		this.onEnterFb.PlayFeedbacks();
	}

	// Token: 0x060003C4 RID: 964 RVA: 0x000118C5 File Offset: 0x0000FAC5
	protected static void InvokeUserCode_RpcPlayFeedbacks(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcPlayFeedbacks called on server.");
			return;
		}
		((PlinkoPocket)obj).UserCode_RpcPlayFeedbacks();
	}

	// Token: 0x060003C5 RID: 965 RVA: 0x000118E8 File Offset: 0x0000FAE8
	static PlinkoPocket()
	{
		RemoteProcedureCalls.RegisterRpc(typeof(PlinkoPocket), "System.Void PlinkoPocket::RpcPlayFeedbacks()", new RemoteCallDelegate(PlinkoPocket.InvokeUserCode_RpcPlayFeedbacks));
	}

	// Token: 0x040002AD RID: 685
	[Header("Pocket Settings")]
	[SerializeField]
	private int slotIndex;

	// Token: 0x040002AE RID: 686
	[SerializeField]
	private Plinko plinkoGame;

	// Token: 0x040002AF RID: 687
	[SerializeField]
	private MMF_Player onEnterFb;
}
