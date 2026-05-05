using System;
using System.Collections;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using FMODUnity;
using Mirror;
using Mirror.RemoteCalls;
using UnityEngine;

// Token: 0x02000052 RID: 82
public class DuckRaceDuck : NetworkBehaviour
{
	// Token: 0x06000259 RID: 601 RVA: 0x0000CC60 File Offset: 0x0000AE60
	[Server]
	public void ServerStartRace(Random rng)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void DuckRaceDuck::ServerStartRace(System.Random)' called when server was not active");
			return;
		}
		base.StartCoroutine(this.DuckRaceRoutine(rng));
		base.StartCoroutine(this.DuckQuackRoutine(rng));
	}

	// Token: 0x0600025A RID: 602 RVA: 0x0000CC93 File Offset: 0x0000AE93
	private IEnumerator DuckRaceRoutine(Random rng)
	{
		this.RpcSetRunningAnimation(true);
		while (!this.duckRace.hasEnded)
		{
			float num = Mathf.Lerp(this.minStepDistance, this.maxStepDistance, (float)rng.NextDouble());
			float targetZ = Mathf.Min(base.transform.localPosition.z + num, this.duckRace.endPoint.localPosition.z);
			this.RpcStep(targetZ);
			yield return new WaitForSeconds(this.stepTweenDuration);
			if (Mathf.Approximately(targetZ, this.duckRace.endPoint.localPosition.z))
			{
				if (this.duckRace.OnDuckFinish(this))
				{
					this.RpcWinFeedback();
				}
				this.RpcSetRunningAnimation(false);
				yield break;
			}
			float seconds = Mathf.Lerp(this.minStepDelay, this.maxStepDelay, (float)rng.NextDouble());
			yield return new WaitForSeconds(seconds);
		}
		this.RpcSetRunningAnimation(false);
		yield break;
	}

	// Token: 0x0600025B RID: 603 RVA: 0x0000CCAC File Offset: 0x0000AEAC
	[ClientRpc]
	private void RpcSetRunningAnimation(bool isRunning)
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		writer.WriteBool(isRunning);
		this.SendRPCInternal("System.Void DuckRaceDuck::RpcSetRunningAnimation(System.Boolean)", -684728799, writer, 0, true);
		NetworkWriterPool.Return(writer);
	}

	// Token: 0x0600025C RID: 604 RVA: 0x0000CCE6 File Offset: 0x0000AEE6
	private IEnumerator DuckQuackRoutine(Random rng)
	{
		while (!this.duckRace.hasEnded)
		{
			float seconds = Mathf.Lerp(0.3f, 1f, (float)rng.NextDouble());
			this.RpcDuckQuack();
			yield return new WaitForSeconds(seconds);
		}
		yield break;
	}

	// Token: 0x0600025D RID: 605 RVA: 0x0000CCFC File Offset: 0x0000AEFC
	[ClientRpc]
	private void RpcDuckQuack()
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		this.SendRPCInternal("System.Void DuckRaceDuck::RpcDuckQuack()", 1749117279, writer, 0, true);
		NetworkWriterPool.Return(writer);
	}

	// Token: 0x0600025E RID: 606 RVA: 0x0000CD2C File Offset: 0x0000AF2C
	[ClientRpc]
	private void RpcStep(float targetZ)
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		writer.WriteFloat(targetZ);
		this.SendRPCInternal("System.Void DuckRaceDuck::RpcStep(System.Single)", 1439858130, writer, 0, true);
		NetworkWriterPool.Return(writer);
	}

	// Token: 0x0600025F RID: 607 RVA: 0x0000CD68 File Offset: 0x0000AF68
	[ClientRpc]
	private void RpcWinFeedback()
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		this.SendRPCInternal("System.Void DuckRaceDuck::RpcWinFeedback()", 1853282982, writer, 0, true);
		NetworkWriterPool.Return(writer);
	}

	// Token: 0x06000260 RID: 608 RVA: 0x0000CD98 File Offset: 0x0000AF98
	[Server]
	public void ServerReturn()
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void DuckRaceDuck::ServerReturn()' called when server was not active");
			return;
		}
		this.RpcReturn();
	}

	// Token: 0x06000261 RID: 609 RVA: 0x0000CDB8 File Offset: 0x0000AFB8
	[ClientRpc]
	private void RpcReturn()
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		this.SendRPCInternal("System.Void DuckRaceDuck::RpcReturn()", -1424841539, writer, 0, true);
		NetworkWriterPool.Return(writer);
	}

	// Token: 0x06000262 RID: 610 RVA: 0x0000CDE8 File Offset: 0x0000AFE8
	[Server]
	public void ResetDuck()
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void DuckRaceDuck::ResetDuck()' called when server was not active");
			return;
		}
		this.RpcResetDuck();
		this.RpcSetRunningAnimation(false);
	}

	// Token: 0x06000263 RID: 611 RVA: 0x0000CE0C File Offset: 0x0000B00C
	[ClientRpc]
	private void RpcResetDuck()
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		this.SendRPCInternal("System.Void DuckRaceDuck::RpcResetDuck()", -931685591, writer, 0, true);
		NetworkWriterPool.Return(writer);
	}

	// Token: 0x06000265 RID: 613 RVA: 0x00002321 File Offset: 0x00000521
	public override bool Weaved()
	{
		return true;
	}

	// Token: 0x06000266 RID: 614 RVA: 0x0000CE7B File Offset: 0x0000B07B
	protected void UserCode_RpcSetRunningAnimation__Boolean(bool isRunning)
	{
		this.animator.SetBool("isRunning", isRunning);
		if (isRunning)
		{
			this.dustVfx.Play();
			return;
		}
		this.dustVfx.Stop();
	}

	// Token: 0x06000267 RID: 615 RVA: 0x0000CEA8 File Offset: 0x0000B0A8
	protected static void InvokeUserCode_RpcSetRunningAnimation__Boolean(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcSetRunningAnimation called on server.");
			return;
		}
		((DuckRaceDuck)obj).UserCode_RpcSetRunningAnimation__Boolean(reader.ReadBool());
	}

	// Token: 0x06000268 RID: 616 RVA: 0x0000CED1 File Offset: 0x0000B0D1
	protected void UserCode_RpcDuckQuack()
	{
		SFXManager.SFXOneShot(this.duckQuackSfx, base.transform.position);
	}

	// Token: 0x06000269 RID: 617 RVA: 0x0000CEE9 File Offset: 0x0000B0E9
	protected static void InvokeUserCode_RpcDuckQuack(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcDuckQuack called on server.");
			return;
		}
		((DuckRaceDuck)obj).UserCode_RpcDuckQuack();
	}

	// Token: 0x0600026A RID: 618 RVA: 0x0000CF0C File Offset: 0x0000B10C
	protected void UserCode_RpcStep__Single(float targetZ)
	{
		base.transform.DOLocalMoveZ(targetZ, this.stepTweenDuration, false).SetEase(Ease.Linear).WaitForCompletion();
	}

	// Token: 0x0600026B RID: 619 RVA: 0x0000CF2D File Offset: 0x0000B12D
	protected static void InvokeUserCode_RpcStep__Single(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcStep called on server.");
			return;
		}
		((DuckRaceDuck)obj).UserCode_RpcStep__Single(reader.ReadFloat());
	}

	// Token: 0x0600026C RID: 620 RVA: 0x0000CF58 File Offset: 0x0000B158
	protected void UserCode_RpcWinFeedback()
	{
		base.transform.DOLocalJump(base.transform.localPosition, 0.25f, 3, 1f, false).SetEase(Ease.Linear);
		SFXManager.SFXOneShot(this.duckWinSfx, base.transform.position);
	}

	// Token: 0x0600026D RID: 621 RVA: 0x0000CFA4 File Offset: 0x0000B1A4
	protected static void InvokeUserCode_RpcWinFeedback(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcWinFeedback called on server.");
			return;
		}
		((DuckRaceDuck)obj).UserCode_RpcWinFeedback();
	}

	// Token: 0x0600026E RID: 622 RVA: 0x0000CFC7 File Offset: 0x0000B1C7
	protected void UserCode_RpcReturn()
	{
		base.transform.DOLocalMoveZ(this.duckRace.startPoint.localPosition.z, 1f, false).SetEase(Ease.OutQuad);
	}

	// Token: 0x0600026F RID: 623 RVA: 0x0000CFF6 File Offset: 0x0000B1F6
	protected static void InvokeUserCode_RpcReturn(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcReturn called on server.");
			return;
		}
		((DuckRaceDuck)obj).UserCode_RpcReturn();
	}

	// Token: 0x06000270 RID: 624 RVA: 0x0000D01C File Offset: 0x0000B21C
	protected void UserCode_RpcResetDuck()
	{
		base.transform.DOKill(false);
		base.transform.localPosition = new Vector3(base.transform.localPosition.x, base.transform.localPosition.y, this.duckRace.startPoint.localPosition.z);
	}

	// Token: 0x06000271 RID: 625 RVA: 0x0000D07B File Offset: 0x0000B27B
	protected static void InvokeUserCode_RpcResetDuck(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcResetDuck called on server.");
			return;
		}
		((DuckRaceDuck)obj).UserCode_RpcResetDuck();
	}

	// Token: 0x06000272 RID: 626 RVA: 0x0000D0A0 File Offset: 0x0000B2A0
	static DuckRaceDuck()
	{
		RemoteProcedureCalls.RegisterRpc(typeof(DuckRaceDuck), "System.Void DuckRaceDuck::RpcSetRunningAnimation(System.Boolean)", new RemoteCallDelegate(DuckRaceDuck.InvokeUserCode_RpcSetRunningAnimation__Boolean));
		RemoteProcedureCalls.RegisterRpc(typeof(DuckRaceDuck), "System.Void DuckRaceDuck::RpcDuckQuack()", new RemoteCallDelegate(DuckRaceDuck.InvokeUserCode_RpcDuckQuack));
		RemoteProcedureCalls.RegisterRpc(typeof(DuckRaceDuck), "System.Void DuckRaceDuck::RpcStep(System.Single)", new RemoteCallDelegate(DuckRaceDuck.InvokeUserCode_RpcStep__Single));
		RemoteProcedureCalls.RegisterRpc(typeof(DuckRaceDuck), "System.Void DuckRaceDuck::RpcWinFeedback()", new RemoteCallDelegate(DuckRaceDuck.InvokeUserCode_RpcWinFeedback));
		RemoteProcedureCalls.RegisterRpc(typeof(DuckRaceDuck), "System.Void DuckRaceDuck::RpcReturn()", new RemoteCallDelegate(DuckRaceDuck.InvokeUserCode_RpcReturn));
		RemoteProcedureCalls.RegisterRpc(typeof(DuckRaceDuck), "System.Void DuckRaceDuck::RpcResetDuck()", new RemoteCallDelegate(DuckRaceDuck.InvokeUserCode_RpcResetDuck));
	}

	// Token: 0x040001DE RID: 478
	[SerializeField]
	private float minStepDelay = 0.5f;

	// Token: 0x040001DF RID: 479
	[SerializeField]
	private float maxStepDelay = 1f;

	// Token: 0x040001E0 RID: 480
	[SerializeField]
	private float minStepDistance = 0.25f;

	// Token: 0x040001E1 RID: 481
	[SerializeField]
	private float maxStepDistance = 1f;

	// Token: 0x040001E2 RID: 482
	[SerializeField]
	private float stepTweenDuration = 0.4f;

	// Token: 0x040001E3 RID: 483
	[SerializeField]
	private DuckRace duckRace;

	// Token: 0x040001E4 RID: 484
	[SerializeField]
	private Animator animator;

	// Token: 0x040001E5 RID: 485
	[SerializeField]
	private ParticleSystem dustVfx;

	// Token: 0x040001E6 RID: 486
	[SerializeField]
	private EventReference duckQuackSfx;

	// Token: 0x040001E7 RID: 487
	[SerializeField]
	private EventReference duckWinSfx;
}
