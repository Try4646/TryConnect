using System;
using FMODUnity;
using Mirror;
using Mirror.RemoteCalls;
using UnityEngine;

// Token: 0x02000274 RID: 628
public class PlinkoPuckSFX : NetworkBehaviour
{
	// Token: 0x06001657 RID: 5719 RVA: 0x0005FD74 File Offset: 0x0005DF74
	private void OnCollisionEnter(Collision other)
	{
		if (this.eventRef.IsNull)
		{
			return;
		}
		if (this.hitCooldownTimer >= Time.time)
		{
			return;
		}
		Vector3 relativeVelocity = other.relativeVelocity;
		if (relativeVelocity.magnitude < this.SensitivityThreshold)
		{
			return;
		}
		float num = Mathf.Max(0f, relativeVelocity.magnitude - this.SensitivityThreshold);
		num = Mathf.Clamp01(num * 0.07f);
		this.HandleHit(num);
		this.pitchMod += Random.Range(0.02f, 0.15f);
	}

	// Token: 0x06001658 RID: 5720 RVA: 0x0005FDFD File Offset: 0x0005DFFD
	private void HandleHit(float magnitude)
	{
		this.CmdPlayHit(magnitude);
		this.hitCooldownTimer = Time.time + this.hitCooldownTime;
	}

	// Token: 0x06001659 RID: 5721 RVA: 0x0005FE18 File Offset: 0x0005E018
	[Server]
	private void CmdPlayHit(float magnitude)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void PlinkoPuckSFX::CmdPlayHit(System.Single)' called when server was not active");
			return;
		}
		if (this.eventRef.IsNull)
		{
			return;
		}
		this.RpcPlayHit(magnitude);
	}

	// Token: 0x0600165A RID: 5722 RVA: 0x0005FE44 File Offset: 0x0005E044
	[ClientRpc]
	private void RpcPlayHit(float magnitude)
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		writer.WriteFloat(magnitude);
		this.SendRPCInternal("System.Void PlinkoPuckSFX::RpcPlayHit(System.Single)", -1859393961, writer, 0, true);
		NetworkWriterPool.Return(writer);
	}

	// Token: 0x0600165C RID: 5724 RVA: 0x00002321 File Offset: 0x00000521
	public override bool Weaved()
	{
		return true;
	}

	// Token: 0x0600165D RID: 5725 RVA: 0x0005FEA8 File Offset: 0x0005E0A8
	protected void UserCode_RpcPlayHit__Single(float magnitude)
	{
		SFXParams[] sFXParams = new SFXParams[]
		{
			new SFXParams("Magnitude", magnitude)
		};
		SFXManager.SFXOneShotWithParameters(this.eventRef, sFXParams, base.gameObject.transform.position, this.pitchMod);
	}

	// Token: 0x0600165E RID: 5726 RVA: 0x0005FEF0 File Offset: 0x0005E0F0
	protected static void InvokeUserCode_RpcPlayHit__Single(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcPlayHit called on server.");
			return;
		}
		((PlinkoPuckSFX)obj).UserCode_RpcPlayHit__Single(reader.ReadFloat());
	}

	// Token: 0x0600165F RID: 5727 RVA: 0x0005FF1A File Offset: 0x0005E11A
	static PlinkoPuckSFX()
	{
		RemoteProcedureCalls.RegisterRpc(typeof(PlinkoPuckSFX), "System.Void PlinkoPuckSFX::RpcPlayHit(System.Single)", new RemoteCallDelegate(PlinkoPuckSFX.InvokeUserCode_RpcPlayHit__Single));
	}

	// Token: 0x04000E95 RID: 3733
	public LayerMask allowedLayers;

	// Token: 0x04000E96 RID: 3734
	[SerializeField]
	private EventReference eventRef;

	// Token: 0x04000E97 RID: 3735
	[SerializeField]
	private float SensitivityThreshold = 3f;

	// Token: 0x04000E98 RID: 3736
	[SerializeField]
	private float hitCooldownTime = 0.3f;

	// Token: 0x04000E99 RID: 3737
	private float hitCooldownTimer;

	// Token: 0x04000E9A RID: 3738
	[SerializeField]
	private float pitchMod = 1f;
}
