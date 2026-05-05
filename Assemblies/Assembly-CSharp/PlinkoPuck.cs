using System;
using Mirror;
using Mirror.RemoteCalls;
using MoreMountains.Feedbacks;
using UnityEngine;

// Token: 0x0200006D RID: 109
public class PlinkoPuck : NetworkBehaviour
{
	// Token: 0x060003C6 RID: 966 RVA: 0x0001190A File Offset: 0x0000FB0A
	[Server]
	public void Initialize(long bet)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void PlinkoPuck::Initialize(System.Int64)' called when server was not active");
			return;
		}
		this.betAmount = bet;
	}

	// Token: 0x060003C7 RID: 967 RVA: 0x00011928 File Offset: 0x0000FB28
	public override void OnStartServer()
	{
		base.OnStartServer();
		base.Invoke("ServerDestroyPuck", this.lifetime);
	}

	// Token: 0x060003C8 RID: 968 RVA: 0x00011944 File Offset: 0x0000FB44
	private void OnCollisionEnter(Collision other)
	{
		this.ClampHorizontalVelocity();
		float num = Mathf.Max(1f, this.minVelocityForSound);
		if (other.GetContact(0).impulse.sqrMagnitude > num * num)
		{
			this.RpcPlayFeedbacks();
		}
	}

	// Token: 0x060003C9 RID: 969 RVA: 0x0001198C File Offset: 0x0000FB8C
	[ClientRpc]
	private void RpcPlayFeedbacks()
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		this.SendRPCInternal("System.Void PlinkoPuck::RpcPlayFeedbacks()", -410348092, writer, 0, true);
		NetworkWriterPool.Return(writer);
	}

	// Token: 0x060003CA RID: 970 RVA: 0x000119BC File Offset: 0x0000FBBC
	private void ClampHorizontalVelocity()
	{
		Vector3 linearVelocity = this.rb.linearVelocity;
		Vector3 vector = new Vector3(linearVelocity.x, 0f, linearVelocity.z);
		float magnitude = vector.magnitude;
		if (magnitude < this.minHorizontalSpeedOnCollide && magnitude > 0.001f)
		{
			Vector3 vector2 = vector.normalized * this.minHorizontalSpeedOnCollide;
			this.rb.linearVelocity = new Vector3(vector2.x, linearVelocity.y, vector2.z);
		}
	}

	// Token: 0x060003CB RID: 971 RVA: 0x00011A3B File Offset: 0x0000FC3B
	[Server]
	private void ServerDestroyPuck()
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void PlinkoPuck::ServerDestroyPuck()' called when server was not active");
			return;
		}
		NetworkServer.Destroy(base.gameObject);
	}

	// Token: 0x060003CD RID: 973 RVA: 0x00002321 File Offset: 0x00000521
	public override bool Weaved()
	{
		return true;
	}

	// Token: 0x060003CE RID: 974 RVA: 0x00011A86 File Offset: 0x0000FC86
	protected void UserCode_RpcPlayFeedbacks()
	{
		this.onHitFb.PlayFeedbacks();
	}

	// Token: 0x060003CF RID: 975 RVA: 0x00011A93 File Offset: 0x0000FC93
	protected static void InvokeUserCode_RpcPlayFeedbacks(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcPlayFeedbacks called on server.");
			return;
		}
		((PlinkoPuck)obj).UserCode_RpcPlayFeedbacks();
	}

	// Token: 0x060003D0 RID: 976 RVA: 0x00011AB6 File Offset: 0x0000FCB6
	static PlinkoPuck()
	{
		RemoteProcedureCalls.RegisterRpc(typeof(PlinkoPuck), "System.Void PlinkoPuck::RpcPlayFeedbacks()", new RemoteCallDelegate(PlinkoPuck.InvokeUserCode_RpcPlayFeedbacks));
	}

	// Token: 0x040002B0 RID: 688
	[Header("References")]
	[SerializeField]
	private MMF_Player onHitFb;

	// Token: 0x040002B1 RID: 689
	[SerializeField]
	private Rigidbody rb;

	// Token: 0x040002B2 RID: 690
	[Header("Puck Settings")]
	[SerializeField]
	private float lifetime = 20f;

	// Token: 0x040002B3 RID: 691
	[SerializeField]
	private float minHorizontalSpeedOnCollide = 0.5f;

	// Token: 0x040002B4 RID: 692
	[SerializeField]
	private float minVelocityForSound = 0.5f;

	// Token: 0x040002B5 RID: 693
	[Header("Debug")]
	public long betAmount;

	// Token: 0x040002B6 RID: 694
	public bool hasTouched;
}
