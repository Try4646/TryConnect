using System;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using Mirror;
using Mirror.RemoteCalls;
using UnityEngine;

// Token: 0x020000D8 RID: 216
public class Basketball : ConsumableItem
{
	// Token: 0x0600086B RID: 2155 RVA: 0x00021FDB File Offset: 0x000201DB
	protected override void OnPickedUp(PlayerInventory playerInventory)
	{
		base.OnPickedUp(playerInventory);
		this.RpcOnPickedUp(playerInventory);
		this.RpcSetTrail(false);
	}

	// Token: 0x0600086C RID: 2156 RVA: 0x00021FF4 File Offset: 0x000201F4
	[ClientRpc]
	private void RpcOnPickedUp(PlayerInventory playerInventory)
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		writer.WriteNetworkBehaviour(playerInventory);
		this.SendRPCInternal("System.Void Basketball::RpcOnPickedUp(PlayerInventory)", 2025474788, writer, 0, true);
		NetworkWriterPool.Return(writer);
	}

	// Token: 0x0600086D RID: 2157 RVA: 0x0002202E File Offset: 0x0002022E
	protected override void OnDropped(PlayerInventory playerInventory)
	{
		base.OnDropped(playerInventory);
		this.RpcResetBallPosition();
		this.RpcSetTrail(true);
	}

	// Token: 0x0600086E RID: 2158 RVA: 0x00022044 File Offset: 0x00020244
	[ClientRpc]
	private void RpcResetBallPosition()
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		this.SendRPCInternal("System.Void Basketball::RpcResetBallPosition()", -33739656, writer, 0, true);
		NetworkWriterPool.Return(writer);
	}

	// Token: 0x0600086F RID: 2159 RVA: 0x00022074 File Offset: 0x00020274
	[ClientRpc]
	private void RpcSetTrail(bool isEnabled)
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		writer.WriteBool(isEnabled);
		this.SendRPCInternal("System.Void Basketball::RpcSetTrail(System.Boolean)", -1701249672, writer, 0, true);
		NetworkWriterPool.Return(writer);
	}

	// Token: 0x06000870 RID: 2160 RVA: 0x000220AE File Offset: 0x000202AE
	protected override void OnUseItem(bool isPressed)
	{
		if (!isPressed)
		{
			return;
		}
		this.Bounce();
	}

	// Token: 0x06000871 RID: 2161 RVA: 0x000220BC File Offset: 0x000202BC
	private void Bounce()
	{
		if (this._isBouncing)
		{
			return;
		}
		Vector3 a = Vector3.ProjectOnPlane(this._holderRb.linearVelocity, Vector3.up);
		Vector3 vector = base.transform.position + a * this.bounceHorizontalDisplacement;
		int num = Physics.RaycastNonAlloc(vector, Vector3.down, this._raycastHits, float.PositiveInfinity, this.rayMask, QueryTriggerInteraction.Ignore);
		if (num == 0)
		{
			return;
		}
		this._isBouncing = true;
		this.anim.SetTrigger("Bounce");
		RaycastHit raycastHit = this._raycastHits[0];
		for (int i = 1; i < num; i++)
		{
			if (this._raycastHits[i].distance < raycastHit.distance)
			{
				raycastHit = this._raycastHits[i];
			}
		}
		float duration = Mathf.Clamp(raycastHit.distance * 0.01f + this.minBounceTime, this.minBounceTime, this.maxBounceTime);
		Vector3 endValue = new Vector3(vector.x, raycastHit.point.y + (this.ballRadius * 2f - this.scaleAmount) / 2f, vector.z);
		Sequence sequence = DOTween.Sequence();
		sequence.Append(this.ballTransform.DOMove(endValue, duration, false).SetEase(Ease.OutQuad));
		sequence.Join(this.ballTransform.DOScaleY(this.ballRadius * 2f - this.scaleAmount, duration).SetEase(Ease.OutCubic));
		sequence.AppendCallback(delegate
		{
			this.bounceSfx.PlayOneShotOverrideParamsWithCustomPos(this.ballTransform.position);
		});
		sequence.Append(this.ballTransform.DOLocalMove(Vector3.zero, duration, false).SetEase(Ease.InOutQuart));
		sequence.Join(this.ballTransform.DOScaleY(1f, duration).SetEase(Ease.InCubic));
		sequence.OnUpdate(delegate
		{
			this.ballTransform.rotation = Quaternion.identity;
		});
		sequence.OnComplete(delegate
		{
			this._isBouncing = false;
		});
	}

	// Token: 0x06000876 RID: 2166 RVA: 0x00002321 File Offset: 0x00000521
	public override bool Weaved()
	{
		return true;
	}

	// Token: 0x06000877 RID: 2167 RVA: 0x0002233D File Offset: 0x0002053D
	protected void UserCode_RpcOnPickedUp__PlayerInventory(PlayerInventory playerInventory)
	{
		this._holderRb = playerInventory.GetComponent<Rigidbody>();
	}

	// Token: 0x06000878 RID: 2168 RVA: 0x0002234B File Offset: 0x0002054B
	protected static void InvokeUserCode_RpcOnPickedUp__PlayerInventory(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcOnPickedUp called on server.");
			return;
		}
		((Basketball)obj).UserCode_RpcOnPickedUp__PlayerInventory(reader.ReadNetworkBehaviour<PlayerInventory>());
	}

	// Token: 0x06000879 RID: 2169 RVA: 0x00022374 File Offset: 0x00020574
	protected void UserCode_RpcResetBallPosition()
	{
		this.ballTransform.DOKill(false);
		this.ballTransform.localPosition = Vector3.zero;
		this.ballTransform.localRotation = Quaternion.identity;
	}

	// Token: 0x0600087A RID: 2170 RVA: 0x000223A3 File Offset: 0x000205A3
	protected static void InvokeUserCode_RpcResetBallPosition(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcResetBallPosition called on server.");
			return;
		}
		((Basketball)obj).UserCode_RpcResetBallPosition();
	}

	// Token: 0x0600087B RID: 2171 RVA: 0x000223C6 File Offset: 0x000205C6
	protected void UserCode_RpcSetTrail__Boolean(bool isEnabled)
	{
		this.trail.emitting = isEnabled;
	}

	// Token: 0x0600087C RID: 2172 RVA: 0x000223D4 File Offset: 0x000205D4
	protected static void InvokeUserCode_RpcSetTrail__Boolean(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcSetTrail called on server.");
			return;
		}
		((Basketball)obj).UserCode_RpcSetTrail__Boolean(reader.ReadBool());
	}

	// Token: 0x0600087D RID: 2173 RVA: 0x00022400 File Offset: 0x00020600
	static Basketball()
	{
		RemoteProcedureCalls.RegisterRpc(typeof(Basketball), "System.Void Basketball::RpcOnPickedUp(PlayerInventory)", new RemoteCallDelegate(Basketball.InvokeUserCode_RpcOnPickedUp__PlayerInventory));
		RemoteProcedureCalls.RegisterRpc(typeof(Basketball), "System.Void Basketball::RpcResetBallPosition()", new RemoteCallDelegate(Basketball.InvokeUserCode_RpcResetBallPosition));
		RemoteProcedureCalls.RegisterRpc(typeof(Basketball), "System.Void Basketball::RpcSetTrail(System.Boolean)", new RemoteCallDelegate(Basketball.InvokeUserCode_RpcSetTrail__Boolean));
	}

	// Token: 0x04000565 RID: 1381
	[Header("Settings")]
	[SerializeField]
	private float minBounceTime = 0.2f;

	// Token: 0x04000566 RID: 1382
	[SerializeField]
	private float maxBounceTime = 0.3f;

	// Token: 0x04000567 RID: 1383
	[SerializeField]
	private float bounceHorizontalDisplacement = 0.1f;

	// Token: 0x04000568 RID: 1384
	[SerializeField]
	private float ballRadius = 0.5f;

	// Token: 0x04000569 RID: 1385
	[SerializeField]
	private float scaleAmount = 0.5f;

	// Token: 0x0400056A RID: 1386
	[Header("References")]
	[SerializeField]
	private Transform ballTransform;

	// Token: 0x0400056B RID: 1387
	[SerializeField]
	private LayerMask rayMask;

	// Token: 0x0400056C RID: 1388
	[SerializeField]
	private TrailRenderer trail;

	// Token: 0x0400056D RID: 1389
	[SerializeField]
	private Animator anim;

	// Token: 0x0400056E RID: 1390
	[Header("SFX")]
	[SerializeField]
	private SFXComponent bounceSfx;

	// Token: 0x0400056F RID: 1391
	private readonly RaycastHit[] _raycastHits = new RaycastHit[8];

	// Token: 0x04000570 RID: 1392
	private Rigidbody _holderRb;

	// Token: 0x04000571 RID: 1393
	private bool _isBouncing;
}
