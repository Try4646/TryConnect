using System;
using System.Collections;
using Extensions;
using Mirror;
using Mirror.RemoteCalls;
using UnityEngine;

// Token: 0x0200003B RID: 59
public class CoinFlip : NetworkBehaviour
{
	// Token: 0x06000141 RID: 321 RVA: 0x00008185 File Offset: 0x00006385
	[Server]
	public void ServerPlayCoinFlip()
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void CoinFlip::ServerPlayCoinFlip()' called when server was not active");
			return;
		}
		this.FlipCoin();
		base.StartCoroutine(this.CheckCoinStoppedRoutine());
	}

	// Token: 0x06000142 RID: 322 RVA: 0x000081B0 File Offset: 0x000063B0
	[Server]
	private void FlipCoin()
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void CoinFlip::FlipCoin()' called when server was not active");
			return;
		}
		Random seededRandom = this.GetSeededRandom(0);
		float d = Mathf.Lerp(this.minFlipForce, this.maxFlipForce, (float)seededRandom.NextDouble());
		this.coin.isKinematic = false;
		this.coin.AddForce(Vector3.up * d, ForceMode.VelocityChange);
		float f = (float)(seededRandom.NextDouble() * 3.141592653589793 * 2.0);
		Vector3 normalized = new Vector3(Mathf.Cos(f), 0f, Mathf.Sin(f)).normalized;
		float d2 = Mathf.Lerp(this.minFlipTorque, this.maxFlipTorque, (float)seededRandom.NextDouble());
		this.coin.AddTorque(normalized * d2, ForceMode.VelocityChange);
		this.loopComponent.RpcLoopSFX(true);
	}

	// Token: 0x06000143 RID: 323 RVA: 0x0000828C File Offset: 0x0000648C
	[Server]
	private IEnumerator CheckCoinStoppedRoutine()
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Collections.IEnumerator CoinFlip::CheckCoinStoppedRoutine()' called when server was not active");
			return null;
		}
		CoinFlip.<CheckCoinStoppedRoutine>d__17 <CheckCoinStoppedRoutine>d__ = new CoinFlip.<CheckCoinStoppedRoutine>d__17(0);
		<CheckCoinStoppedRoutine>d__.<>4__this = this;
		return <CheckCoinStoppedRoutine>d__;
	}

	// Token: 0x06000144 RID: 324 RVA: 0x000082C8 File Offset: 0x000064C8
	[Server]
	private void DecideWinner()
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void CoinFlip::DecideWinner()' called when server was not active");
			return;
		}
		if (Vector3.Angle(this.coin.transform.up, Vector3.up) > 90f)
		{
			NetworkSingleton<WinSceneManager>.Instance.ServerConcludeCoinFlip(true);
			this.RpcPlayJeffVoiceLine(true);
			return;
		}
		NetworkSingleton<WinSceneManager>.Instance.ServerConcludeCoinFlip(false);
		this.RpcPlayJeffVoiceLine(false);
	}

	// Token: 0x06000145 RID: 325 RVA: 0x00008330 File Offset: 0x00006530
	private Random GetSeededRandom(int additionalContext = 0)
	{
		if (NetworkSingleton<SeededRandomManager>.Instance == null || NetworkSingleton<GameManager>.Instance == null)
		{
			return new Random(Random.Range(int.MinValue, int.MaxValue));
		}
		long num = (long)NetworkSingleton<SeededRandomManager>.Instance.CurrentSeed ^ (long)additionalContext * (long)((ulong)-2048144777);
		long num2 = (num ^ num >> 32) * (long)((ulong)-2048144789);
		long num3 = (num2 ^ num2 >> 16) * (long)((ulong)-1028477379);
		return new Random((int)(num3 ^ num3 >> 13));
	}

	// Token: 0x06000146 RID: 326 RVA: 0x000083A4 File Offset: 0x000065A4
	[Server]
	public void TestCoinFlip()
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void CoinFlip::TestCoinFlip()' called when server was not active");
			return;
		}
		Random seededRandom = this.GetSeededRandom(Random.Range(int.MinValue, int.MaxValue));
		float d = Mathf.Lerp(this.minFlipForce, this.maxFlipForce, (float)seededRandom.NextDouble());
		this.coin.AddForce(Vector3.up * d, ForceMode.VelocityChange);
		float f = (float)(seededRandom.NextDouble() * 3.141592653589793 * 2.0);
		Vector3 normalized = new Vector3(Mathf.Cos(f), 0f, Mathf.Sin(f)).normalized;
		float d2 = Mathf.Lerp(this.minFlipTorque, this.maxFlipTorque, (float)seededRandom.NextDouble());
		this.coin.AddTorque(normalized * d2, ForceMode.VelocityChange);
		base.StartCoroutine(this.CheckCoinStoppedRoutine());
	}

	// Token: 0x06000147 RID: 327 RVA: 0x00008484 File Offset: 0x00006684
	[ClientRpc]
	private void RpcPlayJeffVoiceLine(bool isWin)
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		writer.WriteBool(isWin);
		this.SendRPCInternal("System.Void CoinFlip::RpcPlayJeffVoiceLine(System.Boolean)", 750501188, writer, 0, true);
		NetworkWriterPool.Return(writer);
	}

	// Token: 0x06000149 RID: 329 RVA: 0x00002321 File Offset: 0x00000521
	public override bool Weaved()
	{
		return true;
	}

	// Token: 0x0600014A RID: 330 RVA: 0x00008541 File Offset: 0x00006741
	protected void UserCode_RpcPlayJeffVoiceLine__Boolean(bool isWin)
	{
		if (this.winVoiceLineSfx == null || this.loseVoiceLineSfx == null)
		{
			return;
		}
		if (isWin)
		{
			this.winVoiceLineSfx.PlayOneShotWith3DPos();
			return;
		}
		this.loseVoiceLineSfx.PlayOneShotWith3DPos();
	}

	// Token: 0x0600014B RID: 331 RVA: 0x0000857A File Offset: 0x0000677A
	protected static void InvokeUserCode_RpcPlayJeffVoiceLine__Boolean(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcPlayJeffVoiceLine called on server.");
			return;
		}
		((CoinFlip)obj).UserCode_RpcPlayJeffVoiceLine__Boolean(reader.ReadBool());
	}

	// Token: 0x0600014C RID: 332 RVA: 0x000085A3 File Offset: 0x000067A3
	static CoinFlip()
	{
		RemoteProcedureCalls.RegisterRpc(typeof(CoinFlip), "System.Void CoinFlip::RpcPlayJeffVoiceLine(System.Boolean)", new RemoteCallDelegate(CoinFlip.InvokeUserCode_RpcPlayJeffVoiceLine__Boolean));
	}

	// Token: 0x04000108 RID: 264
	[Header("References")]
	[SerializeField]
	private Rigidbody coin;

	// Token: 0x04000109 RID: 265
	[Header("Throw Settings")]
	[SerializeField]
	private float minFlipForce = 10f;

	// Token: 0x0400010A RID: 266
	[SerializeField]
	private float maxFlipForce = 20f;

	// Token: 0x0400010B RID: 267
	[SerializeField]
	private float minFlipTorque = 2f;

	// Token: 0x0400010C RID: 268
	[SerializeField]
	private float maxFlipTorque = 4f;

	// Token: 0x0400010D RID: 269
	[Header("Stop Check Settings")]
	[SerializeField]
	private float firstCheckDelay = 1f;

	// Token: 0x0400010E RID: 270
	[SerializeField]
	private float checkInterval = 0.1f;

	// Token: 0x0400010F RID: 271
	[SerializeField]
	private float maxDuration = 10f;

	// Token: 0x04000110 RID: 272
	[SerializeField]
	private float stopVelocityThreshold = 0.1f;

	// Token: 0x04000111 RID: 273
	[SerializeField]
	private float stopAngularVelocityThreshold = 0.1f;

	// Token: 0x04000112 RID: 274
	[SerializeField]
	private float coinHeightThreshold = 1.25f;

	// Token: 0x04000113 RID: 275
	[Header("SFX")]
	[SerializeField]
	private SFXLoopComponent loopComponent;

	// Token: 0x04000114 RID: 276
	[SerializeField]
	private SFXLocalPlayer winVoiceLineSfx;

	// Token: 0x04000115 RID: 277
	[SerializeField]
	private SFXLocalPlayer loseVoiceLineSfx;

	// Token: 0x04000116 RID: 278
	private Coroutine _checkStopCoroutine;
}
