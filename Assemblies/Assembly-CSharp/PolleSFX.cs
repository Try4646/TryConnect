using System;
using System.Collections;
using FMOD.Studio;
using FMODUnity;
using Mirror;
using Mirror.RemoteCalls;
using UnityEngine;

// Token: 0x02000275 RID: 629
public class PolleSFX : NetworkBehaviour
{
	// Token: 0x06001660 RID: 5728 RVA: 0x0005FF3C File Offset: 0x0005E13C
	public void PlayPolleSays()
	{
		if (this.polleEvent.IsNull)
		{
			return;
		}
		if (this._isPlaying)
		{
			return;
		}
		this._isPlaying = true;
		int randomNumber = this.GetRandomNumber();
		this.PlayPolleSays(randomNumber);
		this.CmdPlayPolleSays(randomNumber);
	}

	// Token: 0x06001661 RID: 5729 RVA: 0x0005FF7C File Offset: 0x0005E17C
	[Command(requiresAuthority = false)]
	private void CmdPlayPolleSays(int voiceLineIndex)
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		writer.WriteVarInt(voiceLineIndex);
		base.SendCommandInternal("System.Void PolleSFX::CmdPlayPolleSays(System.Int32)", 862651091, writer, 0, false);
		NetworkWriterPool.Return(writer);
	}

	// Token: 0x06001662 RID: 5730 RVA: 0x0005FFB8 File Offset: 0x0005E1B8
	[ClientRpc]
	private void RpcPlayPolleSays(int voiceLineIndex)
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		writer.WriteVarInt(voiceLineIndex);
		this.SendRPCInternal("System.Void PolleSFX::RpcPlayPolleSays(System.Int32)", 201215486, writer, 0, true);
		NetworkWriterPool.Return(writer);
	}

	// Token: 0x06001663 RID: 5731 RVA: 0x0005FFF4 File Offset: 0x0005E1F4
	private void PlayPolleSays(int voiceLineIndex)
	{
		if (this._eventInstance.isValid())
		{
			PLAYBACK_STATE playback_STATE;
			this._eventInstance.getPlaybackState(out playback_STATE);
			if (playback_STATE == PLAYBACK_STATE.PLAYING)
			{
				return;
			}
		}
		this._eventInstance = RuntimeManager.CreateInstance(this.polleEvent);
		RuntimeManager.AttachInstanceToGameObject(this._eventInstance, base.gameObject, false);
		this._eventInstance.setParameterByName("PolleSays", (float)voiceLineIndex, false);
		base.StartCoroutine("PolleRoutine");
	}

	// Token: 0x06001664 RID: 5732 RVA: 0x00060063 File Offset: 0x0005E263
	public IEnumerator PolleRoutine()
	{
		this._eventInstance.start();
		yield return new WaitForSeconds(0.3f);
		PLAYBACK_STATE playbackState;
		this._eventInstance.getPlaybackState(out playbackState);
		while (playbackState == PLAYBACK_STATE.PLAYING)
		{
			this._eventInstance.getPlaybackState(out playbackState);
			yield return new WaitForSeconds(0.1f);
		}
		this._eventInstance.release();
		if (!this.exitEvent.IsNull)
		{
			SFXManager.SFXOneShot3DAttached(this.exitEvent, base.gameObject, false);
			yield return new WaitForSeconds(3f);
		}
		this._isPlaying = false;
		yield break;
	}

	// Token: 0x06001665 RID: 5733 RVA: 0x00060074 File Offset: 0x0005E274
	private int GetRandomNumber()
	{
		if (this.amtOfVoiceLines <= 1)
		{
			return 0;
		}
		int num = this.amtOfVoiceLines;
		if (this._lastLine >= 0)
		{
			num--;
		}
		int num2 = Random.Range(0, num);
		if (num2 == this._lastLine)
		{
			num2++;
		}
		this._lastLine = num2;
		return num2;
	}

	// Token: 0x06001667 RID: 5735 RVA: 0x00002321 File Offset: 0x00000521
	public override bool Weaved()
	{
		return true;
	}

	// Token: 0x06001668 RID: 5736 RVA: 0x000600CC File Offset: 0x0005E2CC
	protected void UserCode_CmdPlayPolleSays__Int32(int voiceLineIndex)
	{
		this.RpcPlayPolleSays(voiceLineIndex);
	}

	// Token: 0x06001669 RID: 5737 RVA: 0x000600D5 File Offset: 0x0005E2D5
	protected static void InvokeUserCode_CmdPlayPolleSays__Int32(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdPlayPolleSays called on client.");
			return;
		}
		((PolleSFX)obj).UserCode_CmdPlayPolleSays__Int32(reader.ReadVarInt());
	}

	// Token: 0x0600166A RID: 5738 RVA: 0x000600FE File Offset: 0x0005E2FE
	protected void UserCode_RpcPlayPolleSays__Int32(int voiceLineIndex)
	{
		if (this._isPlaying)
		{
			return;
		}
		this._isPlaying = true;
		this.PlayPolleSays(voiceLineIndex);
	}

	// Token: 0x0600166B RID: 5739 RVA: 0x00060117 File Offset: 0x0005E317
	protected static void InvokeUserCode_RpcPlayPolleSays__Int32(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcPlayPolleSays called on server.");
			return;
		}
		((PolleSFX)obj).UserCode_RpcPlayPolleSays__Int32(reader.ReadVarInt());
	}

	// Token: 0x0600166C RID: 5740 RVA: 0x00060140 File Offset: 0x0005E340
	static PolleSFX()
	{
		RemoteProcedureCalls.RegisterCommand(typeof(PolleSFX), "System.Void PolleSFX::CmdPlayPolleSays(System.Int32)", new RemoteCallDelegate(PolleSFX.InvokeUserCode_CmdPlayPolleSays__Int32), false);
		RemoteProcedureCalls.RegisterRpc(typeof(PolleSFX), "System.Void PolleSFX::RpcPlayPolleSays(System.Int32)", new RemoteCallDelegate(PolleSFX.InvokeUserCode_RpcPlayPolleSays__Int32));
	}

	// Token: 0x04000E9B RID: 3739
	[SerializeField]
	private EventReference polleEvent;

	// Token: 0x04000E9C RID: 3740
	[SerializeField]
	private EventReference exitEvent;

	// Token: 0x04000E9D RID: 3741
	[SerializeField]
	private int amtOfVoiceLines;

	// Token: 0x04000E9E RID: 3742
	private EventInstance _eventInstance;

	// Token: 0x04000E9F RID: 3743
	private int _lastLine = -1;

	// Token: 0x04000EA0 RID: 3744
	private bool _isPlaying;
}
