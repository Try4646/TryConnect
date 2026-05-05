using System;
using FMODUnity;
using Mirror;
using Mirror.RemoteCalls;
using UnityEngine;

// Token: 0x02000277 RID: 631
public class RandomNPCSFX : NetworkBehaviour
{
	// Token: 0x06001673 RID: 5747 RVA: 0x000602AF File Offset: 0x0005E4AF
	public override void OnStartServer()
	{
		this.assignedPitch = Random.Range(-4f, 4f);
		this.assignedVoice = Random.Range(0, this.amt_of_voices + 1);
		this.SetNextVoiceWaitTime();
	}

	// Token: 0x06001674 RID: 5748 RVA: 0x000602E0 File Offset: 0x0005E4E0
	private void SetNextVoiceWaitTime()
	{
		this.waitTime = Time.time + Random.Range(this.minWaitTime, this.maxWaitTime);
	}

	// Token: 0x06001675 RID: 5749 RVA: 0x000602FF File Offset: 0x0005E4FF
	public void ManagedUpdate()
	{
		if (!base.gameObject.activeInHierarchy)
		{
			return;
		}
		if (Time.time >= this.waitTime)
		{
			this.PlayRandomVoiceLine();
			this.SetNextVoiceWaitTime();
		}
	}

	// Token: 0x06001676 RID: 5750 RVA: 0x00060328 File Offset: 0x0005E528
	private void PlayRandomVoiceLine()
	{
		if (!base.isServer)
		{
			return;
		}
		SFXParams[] sFXParams = new SFXParams[]
		{
			new SFXParams("NPCVoice", (float)this.assignedVoice),
			new SFXParams("NPCPitch", this.assignedPitch),
			new SFXParams("NPCVoiceLine", (float)Random.Range(0, this.amt_of_lines + 1))
		};
		this.RpcPlayRandomVoiceLine(sFXParams);
	}

	// Token: 0x06001677 RID: 5751 RVA: 0x0006039C File Offset: 0x0005E59C
	[ClientRpc]
	private void RpcPlayRandomVoiceLine(SFXParams[] sFXParams)
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		Mirror.GeneratedNetworkCode._Write_SFXParams[](writer, sFXParams);
		this.SendRPCInternal("System.Void RandomNPCSFX::RpcPlayRandomVoiceLine(SFXParams[])", -1511883347, writer, 0, true);
		NetworkWriterPool.Return(writer);
	}

	// Token: 0x06001678 RID: 5752 RVA: 0x000603D8 File Offset: 0x0005E5D8
	[Server]
	public void CmdInteractPlayRandomVoiceLine()
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void RandomNPCSFX::CmdInteractPlayRandomVoiceLine()' called when server was not active");
			return;
		}
		if (!base.gameObject.activeInHierarchy || Time.time <= this.interactCooldownTimer)
		{
			return;
		}
		SFXParams[] sFXParams = new SFXParams[]
		{
			new SFXParams("NPCVoice", (float)this.assignedVoice),
			new SFXParams("NPCPitch", this.assignedPitch),
			new SFXParams("NPCVoiceLine", (float)Random.Range(0, this.amt_of_lines + 1))
		};
		this.RpcPlayRandomVoiceLine(sFXParams);
		this.SetNextVoiceWaitTime();
		this.interactCooldownTimer = Time.time + this.interactCooldownTime;
	}

	// Token: 0x0600167A RID: 5754 RVA: 0x00002321 File Offset: 0x00000521
	public override bool Weaved()
	{
		return true;
	}

	// Token: 0x0600167B RID: 5755 RVA: 0x000604C4 File Offset: 0x0005E6C4
	protected void UserCode_RpcPlayRandomVoiceLine__SFXParams[](SFXParams[] sFXParams)
	{
		if (!base.gameObject.activeInHierarchy)
		{
			return;
		}
		if (this.head != null)
		{
			SFXManager.SFXOneShot3DAttachedWithParameters(this.eventReference, sFXParams, this.head, false);
			return;
		}
		SFXManager.SFXOneShot3DAttachedWithParameters(this.eventReference, sFXParams, base.gameObject, false);
	}

	// Token: 0x0600167C RID: 5756 RVA: 0x00060514 File Offset: 0x0005E714
	protected static void InvokeUserCode_RpcPlayRandomVoiceLine__SFXParams[](NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcPlayRandomVoiceLine called on server.");
			return;
		}
		((RandomNPCSFX)obj).UserCode_RpcPlayRandomVoiceLine__SFXParams[](Mirror.GeneratedNetworkCode._Read_SFXParams[](reader));
	}

	// Token: 0x0600167D RID: 5757 RVA: 0x0006053D File Offset: 0x0005E73D
	static RandomNPCSFX()
	{
		RemoteProcedureCalls.RegisterRpc(typeof(RandomNPCSFX), "System.Void RandomNPCSFX::RpcPlayRandomVoiceLine(SFXParams[])", new RemoteCallDelegate(RandomNPCSFX.InvokeUserCode_RpcPlayRandomVoiceLine__SFXParams[]));
	}

	// Token: 0x04000EA5 RID: 3749
	[SerializeField]
	private EventReference eventReference;

	// Token: 0x04000EA6 RID: 3750
	[SerializeField]
	private GameObject head;

	// Token: 0x04000EA7 RID: 3751
	private int assignedVoice;

	// Token: 0x04000EA8 RID: 3752
	private float assignedPitch;

	// Token: 0x04000EA9 RID: 3753
	[SerializeField]
	private float minWaitTime = 7f;

	// Token: 0x04000EAA RID: 3754
	[SerializeField]
	private float maxWaitTime = 45f;

	// Token: 0x04000EAB RID: 3755
	private float waitTime;

	// Token: 0x04000EAC RID: 3756
	[SerializeField]
	private float interactCooldownTime = 2.5f;

	// Token: 0x04000EAD RID: 3757
	private float interactCooldownTimer;

	// Token: 0x04000EAE RID: 3758
	private int amt_of_voices = 3;

	// Token: 0x04000EAF RID: 3759
	private int amt_of_lines = 10;
}
