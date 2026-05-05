using System;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using Mirror;
using Mirror.RemoteCalls;
using UnityEngine;

// Token: 0x020002AC RID: 684
public class EyeZoomEffect : NetworkBehaviour
{
	// Token: 0x0600180D RID: 6157 RVA: 0x00065EFB File Offset: 0x000640FB
	private void Awake()
	{
		this._playerEyes = base.GetComponent<PlayerEyes>();
	}

	// Token: 0x0600180E RID: 6158 RVA: 0x0004CAE8 File Offset: 0x0004ACE8
	public override void OnStartClient()
	{
		base.OnStartClient();
		if (!base.isLocalPlayer)
		{
			base.enabled = false;
			return;
		}
	}

	// Token: 0x0600180F RID: 6159 RVA: 0x00065F09 File Offset: 0x00064109
	private void OnEnable()
	{
		InputEvents.OnZoomEvent = (Action<bool>)Delegate.Combine(InputEvents.OnZoomEvent, new Action<bool>(this.OnEyeZoom));
	}

	// Token: 0x06001810 RID: 6160 RVA: 0x00065F2B File Offset: 0x0006412B
	private void OnDisable()
	{
		InputEvents.OnZoomEvent = (Action<bool>)Delegate.Remove(InputEvents.OnZoomEvent, new Action<bool>(this.OnEyeZoom));
	}

	// Token: 0x06001811 RID: 6161 RVA: 0x00065F4D File Offset: 0x0006414D
	private void OnEyeZoom(bool isPressed)
	{
		this.LocalOnEyeZoom(isPressed);
		this.CmdOnEyeZoom(isPressed);
	}

	// Token: 0x06001812 RID: 6162 RVA: 0x00065F60 File Offset: 0x00064160
	[Command]
	private void CmdOnEyeZoom(bool isPressed)
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		writer.WriteBool(isPressed);
		base.SendCommandInternal("System.Void EyeZoomEffect::CmdOnEyeZoom(System.Boolean)", -2065909384, writer, 0, true);
		NetworkWriterPool.Return(writer);
	}

	// Token: 0x06001813 RID: 6163 RVA: 0x00065F9C File Offset: 0x0006419C
	[ClientRpc]
	private void RpcOnEyeZoom(bool isPressed)
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		writer.WriteBool(isPressed);
		this.SendRPCInternal("System.Void EyeZoomEffect::RpcOnEyeZoom(System.Boolean)", -321266763, writer, 0, true);
		NetworkWriterPool.Return(writer);
	}

	// Token: 0x06001814 RID: 6164 RVA: 0x00065FD8 File Offset: 0x000641D8
	private void LocalOnEyeZoom(bool isPressed)
	{
		if (isPressed)
		{
			this._playerEyes.EyeLeft.DOScaleZ(this.zoomedEyeScale, 0.2f).SetEase(Ease.OutQuad);
			this._playerEyes.EyeRight.DOScaleZ(this.zoomedEyeScale, 0.2f).SetEase(Ease.OutQuad);
			return;
		}
		this._playerEyes.EyeLeft.DOScaleZ(this.initialEyeScale, 0.2f).SetEase(Ease.OutQuad);
		this._playerEyes.EyeRight.DOScaleZ(this.initialEyeScale, 0.2f).SetEase(Ease.OutQuad);
	}

	// Token: 0x06001816 RID: 6166 RVA: 0x00002321 File Offset: 0x00000521
	public override bool Weaved()
	{
		return true;
	}

	// Token: 0x06001817 RID: 6167 RVA: 0x0006608F File Offset: 0x0006428F
	protected void UserCode_CmdOnEyeZoom__Boolean(bool isPressed)
	{
		this.RpcOnEyeZoom(isPressed);
	}

	// Token: 0x06001818 RID: 6168 RVA: 0x00066098 File Offset: 0x00064298
	protected static void InvokeUserCode_CmdOnEyeZoom__Boolean(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdOnEyeZoom called on client.");
			return;
		}
		((EyeZoomEffect)obj).UserCode_CmdOnEyeZoom__Boolean(reader.ReadBool());
	}

	// Token: 0x06001819 RID: 6169 RVA: 0x000660C1 File Offset: 0x000642C1
	protected void UserCode_RpcOnEyeZoom__Boolean(bool isPressed)
	{
		if (base.isLocalPlayer)
		{
			return;
		}
		this.LocalOnEyeZoom(isPressed);
	}

	// Token: 0x0600181A RID: 6170 RVA: 0x000660D3 File Offset: 0x000642D3
	protected static void InvokeUserCode_RpcOnEyeZoom__Boolean(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcOnEyeZoom called on server.");
			return;
		}
		((EyeZoomEffect)obj).UserCode_RpcOnEyeZoom__Boolean(reader.ReadBool());
	}

	// Token: 0x0600181B RID: 6171 RVA: 0x000660FC File Offset: 0x000642FC
	static EyeZoomEffect()
	{
		RemoteProcedureCalls.RegisterCommand(typeof(EyeZoomEffect), "System.Void EyeZoomEffect::CmdOnEyeZoom(System.Boolean)", new RemoteCallDelegate(EyeZoomEffect.InvokeUserCode_CmdOnEyeZoom__Boolean), true);
		RemoteProcedureCalls.RegisterRpc(typeof(EyeZoomEffect), "System.Void EyeZoomEffect::RpcOnEyeZoom(System.Boolean)", new RemoteCallDelegate(EyeZoomEffect.InvokeUserCode_RpcOnEyeZoom__Boolean));
	}

	// Token: 0x04000F8F RID: 3983
	[SerializeField]
	private float initialEyeScale = 0.4f;

	// Token: 0x04000F90 RID: 3984
	[SerializeField]
	private float zoomedEyeScale = 0.6f;

	// Token: 0x04000F91 RID: 3985
	private PlayerEyes _playerEyes;
}
