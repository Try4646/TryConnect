using System;
using System.Collections;
using Dissonance;
using Mirror;
using Mirror.RemoteCalls;
using UnityEngine;

// Token: 0x02000271 RID: 625
public class PlayerVoiceFX : NetworkBehaviour
{
	// Token: 0x06001628 RID: 5672 RVA: 0x0005F5E6 File Offset: 0x0005D7E6
	public override void OnStartClient()
	{
		base.OnStartClient();
		this.InitializeReferences();
	}

	// Token: 0x06001629 RID: 5673 RVA: 0x0005F5F4 File Offset: 0x0005D7F4
	private void InitializeReferences()
	{
		if (this._comms == null)
		{
			this._comms = Object.FindAnyObjectByType<DissonanceComms>();
		}
		if (this._manager == null)
		{
			this._manager = Object.FindAnyObjectByType<VoipManipulationManager>();
		}
		if (this._dissonancePlayer == null)
		{
			this._dissonancePlayer = base.GetComponent<IDissonancePlayer>();
		}
	}

	// Token: 0x0600162A RID: 5674 RVA: 0x0005F648 File Offset: 0x0005D848
	[Command(requiresAuthority = false)]
	public void CmdStartVoiceFX(VoipManipulationManager.VoipFX voipFX)
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		Mirror.GeneratedNetworkCode._Write_VoipManipulationManager/VoipFX(writer, voipFX);
		base.SendCommandInternal("System.Void PlayerVoiceFX::CmdStartVoiceFX(VoipManipulationManager/VoipFX)", -681699849, writer, 0, false);
		NetworkWriterPool.Return(writer);
	}

	// Token: 0x0600162B RID: 5675 RVA: 0x0005F684 File Offset: 0x0005D884
	[ClientRpc]
	public void RpcStartVoiceFX(VoipManipulationManager.VoipFX voipFX)
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		Mirror.GeneratedNetworkCode._Write_VoipManipulationManager/VoipFX(writer, voipFX);
		this.SendRPCInternal("System.Void PlayerVoiceFX::RpcStartVoiceFX(VoipManipulationManager/VoipFX)", 87756608, writer, 0, true);
		NetworkWriterPool.Return(writer);
	}

	// Token: 0x0600162C RID: 5676 RVA: 0x0005F6BE File Offset: 0x0005D8BE
	public void StartVoiceFX(VoipManipulationManager.VoipFX voipFX)
	{
		if (this.AllowedToChangeFX())
		{
			this._manager.AssignPlayerVoipFX(this._dissonancePlayer.PlayerId, voipFX);
		}
	}

	// Token: 0x0600162D RID: 5677 RVA: 0x0005F6DF File Offset: 0x0005D8DF
	public void ResetVoiceFX()
	{
		if (this.AllowedToChangeFX())
		{
			this._manager.AssignPlayerVoipFX(this._dissonancePlayer.PlayerId, VoipManipulationManager.VoipFX.Default);
		}
	}

	// Token: 0x0600162E RID: 5678 RVA: 0x0005F700 File Offset: 0x0005D900
	[Command(requiresAuthority = false)]
	public void CmdResetVoiceFX()
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		base.SendCommandInternal("System.Void PlayerVoiceFX::CmdResetVoiceFX()", 661324567, writer, 0, false);
		NetworkWriterPool.Return(writer);
	}

	// Token: 0x0600162F RID: 5679 RVA: 0x0005F730 File Offset: 0x0005D930
	[ClientRpc]
	public void RpcResetVoiceFX()
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		this.SendRPCInternal("System.Void PlayerVoiceFX::RpcResetVoiceFX()", -457294148, writer, 0, true);
		NetworkWriterPool.Return(writer);
	}

	// Token: 0x06001630 RID: 5680 RVA: 0x0005F760 File Offset: 0x0005D960
	[Command(requiresAuthority = false)]
	public void CmdStartTimedVoiceFX(VoipManipulationManager.VoipFX voipFX, float duration, bool overridable)
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		Mirror.GeneratedNetworkCode._Write_VoipManipulationManager/VoipFX(writer, voipFX);
		writer.WriteFloat(duration);
		writer.WriteBool(overridable);
		base.SendCommandInternal("System.Void PlayerVoiceFX::CmdStartTimedVoiceFX(VoipManipulationManager/VoipFX,System.Single,System.Boolean)", 1023355318, writer, 0, false);
		NetworkWriterPool.Return(writer);
	}

	// Token: 0x06001631 RID: 5681 RVA: 0x0005F7B0 File Offset: 0x0005D9B0
	[ClientRpc]
	public void RpcStartTimedVoiceFX(VoipManipulationManager.VoipFX voipFX, float duration, bool overridable)
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		Mirror.GeneratedNetworkCode._Write_VoipManipulationManager/VoipFX(writer, voipFX);
		writer.WriteFloat(duration);
		writer.WriteBool(overridable);
		this.SendRPCInternal("System.Void PlayerVoiceFX::RpcStartTimedVoiceFX(VoipManipulationManager/VoipFX,System.Single,System.Boolean)", -364832183, writer, 0, true);
		NetworkWriterPool.Return(writer);
	}

	// Token: 0x06001632 RID: 5682 RVA: 0x0005F7FE File Offset: 0x0005D9FE
	private IEnumerator DelayedCall(Action action, float delay, bool overridable)
	{
		this.inOverridableCoroutine = overridable;
		yield return new WaitForSeconds(delay);
		this.inOverridableCoroutine = false;
		this.coroutine = null;
		action();
		yield break;
	}

	// Token: 0x06001633 RID: 5683 RVA: 0x0005F824 File Offset: 0x0005DA24
	public bool AllowedToChangeFX()
	{
		if (this.coroutine == null)
		{
			return true;
		}
		if (this.inOverridableCoroutine && this.coroutine != null)
		{
			base.StopCoroutine(this.coroutine);
			this.coroutine = null;
			this.inOverridableCoroutine = false;
			return true;
		}
		if (!this.inOverridableCoroutine && this.coroutine != null)
		{
			Debug.Log("Voice FX override denied");
			return false;
		}
		return true;
	}

	// Token: 0x06001634 RID: 5684 RVA: 0x0005F884 File Offset: 0x0005DA84
	[Command(requiresAuthority = false)]
	public void CmdSetNoMouthFX(bool active)
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		writer.WriteBool(active);
		base.SendCommandInternal("System.Void PlayerVoiceFX::CmdSetNoMouthFX(System.Boolean)", 58745215, writer, 0, false);
		NetworkWriterPool.Return(writer);
	}

	// Token: 0x06001635 RID: 5685 RVA: 0x0005F8C0 File Offset: 0x0005DAC0
	[ClientRpc]
	public void RpcSetNoMouthFX(bool active)
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		writer.WriteBool(active);
		this.SendRPCInternal("System.Void PlayerVoiceFX::RpcSetNoMouthFX(System.Boolean)", 526642084, writer, 0, true);
		NetworkWriterPool.Return(writer);
	}

	// Token: 0x06001636 RID: 5686 RVA: 0x0005F8FA File Offset: 0x0005DAFA
	private IEnumerator SetNoMouthFXRoutine(bool active)
	{
		while (this._dissonancePlayer == null || this._dissonancePlayer.PlayerId == null)
		{
			yield return new WaitForSeconds(0.2f);
		}
		bool setFX = this._manager.SetPlayerNoMouthFX(this._dissonancePlayer.PlayerId, active);
		while (!setFX)
		{
			setFX = this._manager.SetPlayerNoMouthFX(this._dissonancePlayer.PlayerId, active);
			yield return new WaitForSeconds(0.2f);
		}
		this.isMuffled = active;
		yield break;
	}

	// Token: 0x06001639 RID: 5689 RVA: 0x00002321 File Offset: 0x00000521
	public override bool Weaved()
	{
		return true;
	}

	// Token: 0x0600163A RID: 5690 RVA: 0x0005F918 File Offset: 0x0005DB18
	protected void UserCode_CmdStartVoiceFX__VoipFX(VoipManipulationManager.VoipFX voipFX)
	{
		this.RpcStartVoiceFX(voipFX);
	}

	// Token: 0x0600163B RID: 5691 RVA: 0x0005F921 File Offset: 0x0005DB21
	protected static void InvokeUserCode_CmdStartVoiceFX__VoipFX(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdStartVoiceFX called on client.");
			return;
		}
		((PlayerVoiceFX)obj).UserCode_CmdStartVoiceFX__VoipFX(Mirror.GeneratedNetworkCode._Read_VoipManipulationManager/VoipFX(reader));
	}

	// Token: 0x0600163C RID: 5692 RVA: 0x0005F94A File Offset: 0x0005DB4A
	protected void UserCode_RpcStartVoiceFX__VoipFX(VoipManipulationManager.VoipFX voipFX)
	{
		this.StartVoiceFX(voipFX);
	}

	// Token: 0x0600163D RID: 5693 RVA: 0x0005F953 File Offset: 0x0005DB53
	protected static void InvokeUserCode_RpcStartVoiceFX__VoipFX(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcStartVoiceFX called on server.");
			return;
		}
		((PlayerVoiceFX)obj).UserCode_RpcStartVoiceFX__VoipFX(Mirror.GeneratedNetworkCode._Read_VoipManipulationManager/VoipFX(reader));
	}

	// Token: 0x0600163E RID: 5694 RVA: 0x0005F97C File Offset: 0x0005DB7C
	protected void UserCode_CmdResetVoiceFX()
	{
		this.RpcResetVoiceFX();
	}

	// Token: 0x0600163F RID: 5695 RVA: 0x0005F984 File Offset: 0x0005DB84
	protected static void InvokeUserCode_CmdResetVoiceFX(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdResetVoiceFX called on client.");
			return;
		}
		((PlayerVoiceFX)obj).UserCode_CmdResetVoiceFX();
	}

	// Token: 0x06001640 RID: 5696 RVA: 0x0005F910 File Offset: 0x0005DB10
	protected void UserCode_RpcResetVoiceFX()
	{
		this.ResetVoiceFX();
	}

	// Token: 0x06001641 RID: 5697 RVA: 0x0005F9A7 File Offset: 0x0005DBA7
	protected static void InvokeUserCode_RpcResetVoiceFX(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcResetVoiceFX called on server.");
			return;
		}
		((PlayerVoiceFX)obj).UserCode_RpcResetVoiceFX();
	}

	// Token: 0x06001642 RID: 5698 RVA: 0x0005F9CA File Offset: 0x0005DBCA
	protected void UserCode_CmdStartTimedVoiceFX__VoipFX__Single__Boolean(VoipManipulationManager.VoipFX voipFX, float duration, bool overridable)
	{
		this.RpcStartTimedVoiceFX(voipFX, duration, overridable);
	}

	// Token: 0x06001643 RID: 5699 RVA: 0x0005F9D5 File Offset: 0x0005DBD5
	protected static void InvokeUserCode_CmdStartTimedVoiceFX__VoipFX__Single__Boolean(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdStartTimedVoiceFX called on client.");
			return;
		}
		((PlayerVoiceFX)obj).UserCode_CmdStartTimedVoiceFX__VoipFX__Single__Boolean(Mirror.GeneratedNetworkCode._Read_VoipManipulationManager/VoipFX(reader), reader.ReadFloat(), reader.ReadBool());
	}

	// Token: 0x06001644 RID: 5700 RVA: 0x0005FA0B File Offset: 0x0005DC0B
	protected void UserCode_RpcStartTimedVoiceFX__VoipFX__Single__Boolean(VoipManipulationManager.VoipFX voipFX, float duration, bool overridable)
	{
		this.StartVoiceFX(voipFX);
		this.coroutine = base.StartCoroutine(this.DelayedCall(delegate
		{
			this.ResetVoiceFX();
		}, duration, overridable));
	}

	// Token: 0x06001645 RID: 5701 RVA: 0x0005FA34 File Offset: 0x0005DC34
	protected static void InvokeUserCode_RpcStartTimedVoiceFX__VoipFX__Single__Boolean(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcStartTimedVoiceFX called on server.");
			return;
		}
		((PlayerVoiceFX)obj).UserCode_RpcStartTimedVoiceFX__VoipFX__Single__Boolean(Mirror.GeneratedNetworkCode._Read_VoipManipulationManager/VoipFX(reader), reader.ReadFloat(), reader.ReadBool());
	}

	// Token: 0x06001646 RID: 5702 RVA: 0x0005FA6A File Offset: 0x0005DC6A
	protected void UserCode_CmdSetNoMouthFX__Boolean(bool active)
	{
		this.RpcSetNoMouthFX(active);
	}

	// Token: 0x06001647 RID: 5703 RVA: 0x0005FA73 File Offset: 0x0005DC73
	protected static void InvokeUserCode_CmdSetNoMouthFX__Boolean(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdSetNoMouthFX called on client.");
			return;
		}
		((PlayerVoiceFX)obj).UserCode_CmdSetNoMouthFX__Boolean(reader.ReadBool());
	}

	// Token: 0x06001648 RID: 5704 RVA: 0x0005FA9C File Offset: 0x0005DC9C
	protected void UserCode_RpcSetNoMouthFX__Boolean(bool active)
	{
		base.StartCoroutine(this.SetNoMouthFXRoutine(active));
	}

	// Token: 0x06001649 RID: 5705 RVA: 0x0005FAAC File Offset: 0x0005DCAC
	protected static void InvokeUserCode_RpcSetNoMouthFX__Boolean(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcSetNoMouthFX called on server.");
			return;
		}
		((PlayerVoiceFX)obj).UserCode_RpcSetNoMouthFX__Boolean(reader.ReadBool());
	}

	// Token: 0x0600164A RID: 5706 RVA: 0x0005FAD8 File Offset: 0x0005DCD8
	static PlayerVoiceFX()
	{
		RemoteProcedureCalls.RegisterCommand(typeof(PlayerVoiceFX), "System.Void PlayerVoiceFX::CmdStartVoiceFX(VoipManipulationManager/VoipFX)", new RemoteCallDelegate(PlayerVoiceFX.InvokeUserCode_CmdStartVoiceFX__VoipFX), false);
		RemoteProcedureCalls.RegisterCommand(typeof(PlayerVoiceFX), "System.Void PlayerVoiceFX::CmdResetVoiceFX()", new RemoteCallDelegate(PlayerVoiceFX.InvokeUserCode_CmdResetVoiceFX), false);
		RemoteProcedureCalls.RegisterCommand(typeof(PlayerVoiceFX), "System.Void PlayerVoiceFX::CmdStartTimedVoiceFX(VoipManipulationManager/VoipFX,System.Single,System.Boolean)", new RemoteCallDelegate(PlayerVoiceFX.InvokeUserCode_CmdStartTimedVoiceFX__VoipFX__Single__Boolean), false);
		RemoteProcedureCalls.RegisterCommand(typeof(PlayerVoiceFX), "System.Void PlayerVoiceFX::CmdSetNoMouthFX(System.Boolean)", new RemoteCallDelegate(PlayerVoiceFX.InvokeUserCode_CmdSetNoMouthFX__Boolean), false);
		RemoteProcedureCalls.RegisterRpc(typeof(PlayerVoiceFX), "System.Void PlayerVoiceFX::RpcStartVoiceFX(VoipManipulationManager/VoipFX)", new RemoteCallDelegate(PlayerVoiceFX.InvokeUserCode_RpcStartVoiceFX__VoipFX));
		RemoteProcedureCalls.RegisterRpc(typeof(PlayerVoiceFX), "System.Void PlayerVoiceFX::RpcResetVoiceFX()", new RemoteCallDelegate(PlayerVoiceFX.InvokeUserCode_RpcResetVoiceFX));
		RemoteProcedureCalls.RegisterRpc(typeof(PlayerVoiceFX), "System.Void PlayerVoiceFX::RpcStartTimedVoiceFX(VoipManipulationManager/VoipFX,System.Single,System.Boolean)", new RemoteCallDelegate(PlayerVoiceFX.InvokeUserCode_RpcStartTimedVoiceFX__VoipFX__Single__Boolean));
		RemoteProcedureCalls.RegisterRpc(typeof(PlayerVoiceFX), "System.Void PlayerVoiceFX::RpcSetNoMouthFX(System.Boolean)", new RemoteCallDelegate(PlayerVoiceFX.InvokeUserCode_RpcSetNoMouthFX__Boolean));
	}

	// Token: 0x04000E84 RID: 3716
	private DissonanceComms _comms;

	// Token: 0x04000E85 RID: 3717
	private VoipManipulationManager _manager;

	// Token: 0x04000E86 RID: 3718
	private IDissonancePlayer _dissonancePlayer;

	// Token: 0x04000E87 RID: 3719
	private Coroutine coroutine;

	// Token: 0x04000E88 RID: 3720
	private bool inOverridableCoroutine;

	// Token: 0x04000E89 RID: 3721
	public bool isMuffled;
}
