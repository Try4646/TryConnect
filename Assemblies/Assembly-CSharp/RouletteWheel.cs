using System;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using FMODUnity;
using Mirror;
using Mirror.RemoteCalls;
using UnityEngine;

// Token: 0x0200007C RID: 124
public class RouletteWheel : Wheel
{
	// Token: 0x06000484 RID: 1156 RVA: 0x000146B8 File Offset: 0x000128B8
	[Server]
	public override void SpinTheWheel(Random rng)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void RouletteWheel::SpinTheWheel(System.Random)' called when server was not active");
			return;
		}
		if (this._isSpinning)
		{
			return;
		}
		this._isSpinning = true;
		float num = 9.72973f;
		float num2 = (float)rng.Next(0, 37) * num;
		float num3 = (float)rng.Next(0, 37) * num;
		float num4 = (float)this.minTurnAmount * 360f + num2;
		float angle = (float)this.minTurnAmount * 360f + num3;
		if (this.spinDirection)
		{
			num4 *= -1f;
		}
		base.RpcSpinWheel(num4, this.spinDuration);
		this.RpcSpinBallWheel(angle, this.spinDuration);
		base.StartCoroutine(base.WaitAndStop());
	}

	// Token: 0x06000485 RID: 1157 RVA: 0x00014764 File Offset: 0x00012964
	[ClientRpc]
	private void RpcSpinBallWheel(float angle, float duration)
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		writer.WriteFloat(angle);
		writer.WriteFloat(duration);
		this.SendRPCInternal("System.Void RouletteWheel::RpcSpinBallWheel(System.Single,System.Single)", 2086220949, writer, 0, true);
		NetworkWriterPool.Return(writer);
	}

	// Token: 0x06000488 RID: 1160 RVA: 0x00002321 File Offset: 0x00000521
	public override bool Weaved()
	{
		return true;
	}

	// Token: 0x06000489 RID: 1161 RVA: 0x00014804 File Offset: 0x00012A04
	protected void UserCode_RpcSpinBallWheel__Single__Single(float angle, float duration)
	{
		this.ball.DOLocalMove(new Vector3(0f, 1.25f, 0.1f), 0.2f, false);
		this.ballWheel.DOLocalRotate(new Vector3(0f, 0f, -angle), duration - this.ballDropDuration, RotateMode.FastBeyond360).SetEase(this.easing).OnComplete(delegate
		{
			this.ball.DOLocalMove(new Vector3(0f, 0.9f, 0f), this.ballDropDuration, false).SetEase(Ease.OutBounce);
			SFXManager.SFXOneShot(this.ballLandSfx, base.transform.position);
		});
	}

	// Token: 0x0600048A RID: 1162 RVA: 0x00014879 File Offset: 0x00012A79
	protected static void InvokeUserCode_RpcSpinBallWheel__Single__Single(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcSpinBallWheel called on server.");
			return;
		}
		((RouletteWheel)obj).UserCode_RpcSpinBallWheel__Single__Single(reader.ReadFloat(), reader.ReadFloat());
	}

	// Token: 0x0600048B RID: 1163 RVA: 0x000148AA File Offset: 0x00012AAA
	static RouletteWheel()
	{
		RemoteProcedureCalls.RegisterRpc(typeof(RouletteWheel), "System.Void RouletteWheel::RpcSpinBallWheel(System.Single,System.Single)", new RemoteCallDelegate(RouletteWheel.InvokeUserCode_RpcSpinBallWheel__Single__Single));
	}

	// Token: 0x04000319 RID: 793
	[SerializeField]
	private Transform ballWheel;

	// Token: 0x0400031A RID: 794
	[SerializeField]
	private Transform ball;

	// Token: 0x0400031B RID: 795
	[SerializeField]
	private float ballDropDuration;

	// Token: 0x0400031C RID: 796
	[SerializeField]
	private EventReference ballLandSfx;
}
