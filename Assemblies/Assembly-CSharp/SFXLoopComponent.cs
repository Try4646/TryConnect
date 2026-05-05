using System;
using FMOD.Studio;
using FMODUnity;
using Mirror;
using Mirror.RemoteCalls;
using UnityEngine;

// Token: 0x0200027F RID: 639
public class SFXLoopComponent : NetworkBehaviour
{
	// Token: 0x060016BB RID: 5819 RVA: 0x00061040 File Offset: 0x0005F240
	[Command(requiresAuthority = false)]
	public void CmdLoopSFX(bool play)
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		writer.WriteBool(play);
		base.SendCommandInternal("System.Void SFXLoopComponent::CmdLoopSFX(System.Boolean)", 1121932687, writer, 0, false);
		NetworkWriterPool.Return(writer);
	}

	// Token: 0x060016BC RID: 5820 RVA: 0x0006107C File Offset: 0x0005F27C
	[ClientRpc]
	public void RpcLoopSFX(bool play)
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		writer.WriteBool(play);
		this.SendRPCInternal("System.Void SFXLoopComponent::RpcLoopSFX(System.Boolean)", 824686118, writer, 0, true);
		NetworkWriterPool.Return(writer);
	}

	// Token: 0x060016BD RID: 5821 RVA: 0x000610B8 File Offset: 0x0005F2B8
	public void LoopSFX(bool play)
	{
		if (this.eventReference.IsNull)
		{
			return;
		}
		if (play)
		{
			if (this.loopInstance.isValid())
			{
				PLAYBACK_STATE playback_STATE;
				this.loopInstance.getPlaybackState(out playback_STATE);
				if (playback_STATE == PLAYBACK_STATE.PLAYING)
				{
					return;
				}
			}
			this.loopInstance = RuntimeManager.CreateInstance(this.eventReference);
			this.loopInstance.set3DAttributes(base.transform.position.To3DAttributes());
			RuntimeManager.AttachInstanceToGameObject(this.loopInstance, base.gameObject, true);
			this.loopInstance.start();
			return;
		}
		if (!this.loopInstance.isValid())
		{
			return;
		}
		this.loopInstance.stop(this.allowFadeout ? FMOD.Studio.STOP_MODE.ALLOWFADEOUT : FMOD.Studio.STOP_MODE.IMMEDIATE);
		this.loopInstance.release();
	}

	// Token: 0x060016BE RID: 5822 RVA: 0x00061173 File Offset: 0x0005F373
	private void OnDisable()
	{
		this.LoopSFX(false);
	}

	// Token: 0x060016BF RID: 5823 RVA: 0x0006117C File Offset: 0x0005F37C
	public void ModulatePitch(float pitch)
	{
		this.loopInstance.setPitch(pitch);
	}

	// Token: 0x060016C1 RID: 5825 RVA: 0x00002321 File Offset: 0x00000521
	public override bool Weaved()
	{
		return true;
	}

	// Token: 0x060016C2 RID: 5826 RVA: 0x0006119A File Offset: 0x0005F39A
	protected void UserCode_CmdLoopSFX__Boolean(bool play)
	{
		this.RpcLoopSFX(play);
	}

	// Token: 0x060016C3 RID: 5827 RVA: 0x000611A3 File Offset: 0x0005F3A3
	protected static void InvokeUserCode_CmdLoopSFX__Boolean(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdLoopSFX called on client.");
			return;
		}
		((SFXLoopComponent)obj).UserCode_CmdLoopSFX__Boolean(reader.ReadBool());
	}

	// Token: 0x060016C4 RID: 5828 RVA: 0x000611CC File Offset: 0x0005F3CC
	protected void UserCode_RpcLoopSFX__Boolean(bool play)
	{
		this.LoopSFX(play);
	}

	// Token: 0x060016C5 RID: 5829 RVA: 0x000611D5 File Offset: 0x0005F3D5
	protected static void InvokeUserCode_RpcLoopSFX__Boolean(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcLoopSFX called on server.");
			return;
		}
		((SFXLoopComponent)obj).UserCode_RpcLoopSFX__Boolean(reader.ReadBool());
	}

	// Token: 0x060016C6 RID: 5830 RVA: 0x00061200 File Offset: 0x0005F400
	static SFXLoopComponent()
	{
		RemoteProcedureCalls.RegisterCommand(typeof(SFXLoopComponent), "System.Void SFXLoopComponent::CmdLoopSFX(System.Boolean)", new RemoteCallDelegate(SFXLoopComponent.InvokeUserCode_CmdLoopSFX__Boolean), false);
		RemoteProcedureCalls.RegisterRpc(typeof(SFXLoopComponent), "System.Void SFXLoopComponent::RpcLoopSFX(System.Boolean)", new RemoteCallDelegate(SFXLoopComponent.InvokeUserCode_RpcLoopSFX__Boolean));
	}

	// Token: 0x04000ED2 RID: 3794
	[SerializeField]
	private EventReference eventReference;

	// Token: 0x04000ED3 RID: 3795
	[SerializeField]
	private bool allowFadeout = true;

	// Token: 0x04000ED4 RID: 3796
	public EventInstance loopInstance;
}
