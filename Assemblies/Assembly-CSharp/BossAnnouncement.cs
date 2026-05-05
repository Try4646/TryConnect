using System;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using Mirror;
using Mirror.RemoteCalls;
using UnityEngine;

// Token: 0x02000022 RID: 34
public class BossAnnouncement : NetworkBehaviour
{
	// Token: 0x06000078 RID: 120 RVA: 0x00004BEF File Offset: 0x00002DEF
	private void Awake()
	{
		this._startPosition = base.transform.localPosition;
		this._targetPosition = this._startPosition - Vector3.up * this.slideDownDistance;
	}

	// Token: 0x06000079 RID: 121 RVA: 0x00004C23 File Offset: 0x00002E23
	public void StartSlideIn()
	{
		if (base.isServer)
		{
			this.RpcStartSlideIn();
		}
	}

	// Token: 0x0600007A RID: 122 RVA: 0x00004C34 File Offset: 0x00002E34
	[ClientRpc]
	private void RpcStartSlideIn()
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		this.SendRPCInternal("System.Void BossAnnouncement::RpcStartSlideIn()", 106223176, writer, 0, true);
		NetworkWriterPool.Return(writer);
	}

	// Token: 0x0600007B RID: 123 RVA: 0x00004C64 File Offset: 0x00002E64
	private void SlideIn()
	{
		if (this._isAnimating)
		{
			return;
		}
		this._isAnimating = true;
		base.transform.localPosition = this._startPosition;
		base.transform.DOLocalMove(this._targetPosition, this.slideDuration, false).SetEase(this.slideEase).OnComplete(delegate
		{
			this._isAnimating = false;
			if (base.isServer)
			{
				this.RpcStartSpeech();
			}
		});
	}

	// Token: 0x0600007C RID: 124 RVA: 0x00004CC7 File Offset: 0x00002EC7
	public void StartSlideOut()
	{
		if (base.isServer)
		{
			this.RpcStartSlideOut();
		}
	}

	// Token: 0x0600007D RID: 125 RVA: 0x00004CD8 File Offset: 0x00002ED8
	[ClientRpc]
	private void RpcStartSlideOut()
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		this.SendRPCInternal("System.Void BossAnnouncement::RpcStartSlideOut()", 1113800275, writer, 0, true);
		NetworkWriterPool.Return(writer);
	}

	// Token: 0x0600007E RID: 126 RVA: 0x00004D08 File Offset: 0x00002F08
	private void SlideOut()
	{
		if (this._isAnimating)
		{
			return;
		}
		this._isAnimating = true;
		base.transform.DOLocalMove(this._startPosition, this.slideDuration, false).SetEase(this.slideEase).OnComplete(delegate
		{
			this._isAnimating = false;
		});
	}

	// Token: 0x0600007F RID: 127 RVA: 0x00004D5C File Offset: 0x00002F5C
	[ClientRpc]
	private void RpcStartSpeech()
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		this.SendRPCInternal("System.Void BossAnnouncement::RpcStartSpeech()", -1381309830, writer, 0, true);
		NetworkWriterPool.Return(writer);
	}

	// Token: 0x06000080 RID: 128 RVA: 0x00004D8C File Offset: 0x00002F8C
	private void InitializeSpeech()
	{
		Debug.Log("BossAnnouncement: Starting speech");
	}

	// Token: 0x06000084 RID: 132 RVA: 0x00002321 File Offset: 0x00000521
	public override bool Weaved()
	{
		return true;
	}

	// Token: 0x06000085 RID: 133 RVA: 0x00004DDD File Offset: 0x00002FDD
	protected void UserCode_RpcStartSlideIn()
	{
		this.SlideIn();
	}

	// Token: 0x06000086 RID: 134 RVA: 0x00004DE5 File Offset: 0x00002FE5
	protected static void InvokeUserCode_RpcStartSlideIn(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcStartSlideIn called on server.");
			return;
		}
		((BossAnnouncement)obj).UserCode_RpcStartSlideIn();
	}

	// Token: 0x06000087 RID: 135 RVA: 0x00004E08 File Offset: 0x00003008
	protected void UserCode_RpcStartSlideOut()
	{
		this.SlideOut();
	}

	// Token: 0x06000088 RID: 136 RVA: 0x00004E10 File Offset: 0x00003010
	protected static void InvokeUserCode_RpcStartSlideOut(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcStartSlideOut called on server.");
			return;
		}
		((BossAnnouncement)obj).UserCode_RpcStartSlideOut();
	}

	// Token: 0x06000089 RID: 137 RVA: 0x00004E33 File Offset: 0x00003033
	protected void UserCode_RpcStartSpeech()
	{
		this.InitializeSpeech();
	}

	// Token: 0x0600008A RID: 138 RVA: 0x00004E3B File Offset: 0x0000303B
	protected static void InvokeUserCode_RpcStartSpeech(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcStartSpeech called on server.");
			return;
		}
		((BossAnnouncement)obj).UserCode_RpcStartSpeech();
	}

	// Token: 0x0600008B RID: 139 RVA: 0x00004E60 File Offset: 0x00003060
	static BossAnnouncement()
	{
		RemoteProcedureCalls.RegisterRpc(typeof(BossAnnouncement), "System.Void BossAnnouncement::RpcStartSlideIn()", new RemoteCallDelegate(BossAnnouncement.InvokeUserCode_RpcStartSlideIn));
		RemoteProcedureCalls.RegisterRpc(typeof(BossAnnouncement), "System.Void BossAnnouncement::RpcStartSlideOut()", new RemoteCallDelegate(BossAnnouncement.InvokeUserCode_RpcStartSlideOut));
		RemoteProcedureCalls.RegisterRpc(typeof(BossAnnouncement), "System.Void BossAnnouncement::RpcStartSpeech()", new RemoteCallDelegate(BossAnnouncement.InvokeUserCode_RpcStartSpeech));
	}

	// Token: 0x04000080 RID: 128
	[Header("Settings")]
	[SerializeField]
	private float slideDownDistance = 5f;

	// Token: 0x04000081 RID: 129
	[SerializeField]
	private float slideDuration = 1f;

	// Token: 0x04000082 RID: 130
	[SerializeField]
	private Ease slideEase = Ease.OutQuad;

	// Token: 0x04000083 RID: 131
	private Vector3 _startPosition;

	// Token: 0x04000084 RID: 132
	private Vector3 _targetPosition;

	// Token: 0x04000085 RID: 133
	private bool _isAnimating;
}
