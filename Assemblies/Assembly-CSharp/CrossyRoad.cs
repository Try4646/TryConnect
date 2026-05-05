using System;
using System.Collections;
using System.Runtime.InteropServices;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using FMODUnity;
using Mirror;
using Mirror.RemoteCalls;
using TMPro;
using UnityEngine;

// Token: 0x02000048 RID: 72
public class CrossyRoad : GameBase
{
	// Token: 0x060001E1 RID: 481 RVA: 0x0000AF48 File Offset: 0x00009148
	private void OnMultiplierChanged(double oldMultiplier, double newMultiplier)
	{
		if (this.multiplierText)
		{
			this.multiplierText.text = string.Format("{0:F1}x", newMultiplier);
		}
		if (this.potentialWinningsText)
		{
			long num = (long)Math.Round((double)this.currentBet * newMultiplier);
			this.potentialWinningsText.text = string.Format("${0:N0}", num);
		}
	}

	// Token: 0x060001E2 RID: 482 RVA: 0x0000AFB8 File Offset: 0x000091B8
	private void Start()
	{
		Animator[] array = this.sidePenguinAnims;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].speed = Random.Range(0.8f, 1.2f);
		}
	}

	// Token: 0x060001E3 RID: 483 RVA: 0x0000AFF4 File Offset: 0x000091F4
	[Server]
	protected override void StartGame()
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void CrossyRoad::StartGame()' called when server was not active");
			return;
		}
		this._currentStep = 0;
		this.NetworkcurrentMultiplier = this.stepMultipliers[this._currentStep];
		this._currentCrashChance = this.GetCrashChanceForStep(this._currentStep);
	}

	// Token: 0x060001E4 RID: 484 RVA: 0x0000B044 File Offset: 0x00009244
	[Server]
	public void TakeStep()
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void CrossyRoad::TakeStep()' called when server was not active");
			return;
		}
		if (!this.isPlaying)
		{
			return;
		}
		if (this._hasEnded)
		{
			return;
		}
		if (this._currentStep >= this.maxSteps)
		{
			return;
		}
		if (Time.time - this._lastStepTime < this.stepTweenDuration + 0.1f)
		{
			return;
		}
		this._lastStepTime = Time.time;
		base.StartCoroutine(this.StepRoutine());
	}

	// Token: 0x060001E5 RID: 485 RVA: 0x0000B0BA File Offset: 0x000092BA
	private IEnumerator StepRoutine()
	{
		float num = (float)base.GetSeededRandom(this._currentStep * 9999).NextDouble();
		this._currentStep++;
		if (num < this._currentCrashChance)
		{
			this.RpcPenguinStep(this._currentStep, (float)this.stepMultipliers[this._currentStep]);
			this._hasEnded = true;
			yield return new WaitForSeconds(this.stepTweenDuration);
			this.Crash();
			yield break;
		}
		this.NetworkcurrentMultiplier = this.stepMultipliers[this._currentStep];
		this._currentCrashChance = this.GetCrashChanceForStep(this._currentStep);
		this.RpcPenguinStep(this._currentStep, (float)this.currentMultiplier);
		if (this._currentStep >= this.maxSteps)
		{
			yield return new WaitForSeconds(this.stepTweenDuration);
			this.CashOut();
		}
		yield break;
	}

	// Token: 0x060001E6 RID: 486 RVA: 0x0000B0CC File Offset: 0x000092CC
	[Server]
	private void Crash()
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void CrossyRoad::Crash()' called when server was not active");
			return;
		}
		this.RpcCrashFeedback(this._currentStep, (float)this.currentMultiplier);
		this.Payout(0.0, ChangeType.GameResult, null, -1L);
		base.StartCoroutine(this.ResetAfterDelay());
	}

	// Token: 0x060001E7 RID: 487 RVA: 0x0000B124 File Offset: 0x00009324
	[Server]
	public void CashOut()
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void CrossyRoad::CashOut()' called when server was not active");
			return;
		}
		if (!this.isPlaying)
		{
			return;
		}
		if (this._hasEnded)
		{
			return;
		}
		if (this._currentStep <= 0)
		{
			return;
		}
		this._hasEnded = true;
		this.Payout(this.currentMultiplier, ChangeType.GameResult, null, -1L);
		this.RpcWinFeedback((float)this.currentMultiplier);
		base.StartCoroutine(this.ResetAfterDelay());
	}

	// Token: 0x060001E8 RID: 488 RVA: 0x0000B194 File Offset: 0x00009394
	[Server]
	private IEnumerator ResetAfterDelay()
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Collections.IEnumerator CrossyRoad::ResetAfterDelay()' called when server was not active");
			return null;
		}
		CrossyRoad.<ResetAfterDelay>d__30 <ResetAfterDelay>d__ = new CrossyRoad.<ResetAfterDelay>d__30(0);
		<ResetAfterDelay>d__.<>4__this = this;
		return <ResetAfterDelay>d__;
	}

	// Token: 0x060001E9 RID: 489 RVA: 0x0000B1D0 File Offset: 0x000093D0
	[Server]
	protected override void ResetGame()
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void CrossyRoad::ResetGame()' called when server was not active");
			return;
		}
		this._hasEnded = false;
		this._currentStep = 0;
		this.NetworkcurrentMultiplier = this.stepMultipliers[this._currentStep];
		this._currentCrashChance = this.GetCrashChanceForStep(this._currentStep);
		base.ResetGame();
	}

	// Token: 0x060001EA RID: 490 RVA: 0x0000B22C File Offset: 0x0000942C
	private float GetCrashChanceForStep(int step)
	{
		if (step < 0)
		{
			return 0f;
		}
		if (step >= this.maxSteps)
		{
			return 1f;
		}
		double num = this.stepMultipliers[step];
		double num2 = this.stepMultipliers[step + 1];
		double num3 = base.EstimatedValue / num;
		double num4 = base.EstimatedValue / num2 / num3;
		return Mathf.Clamp01((float)(1.0 - num4));
	}

	// Token: 0x060001EB RID: 491 RVA: 0x0000B28C File Offset: 0x0000948C
	[ClientRpc]
	private void RpcPenguinStep(int step, float mult)
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		writer.WriteVarInt(step);
		writer.WriteFloat(mult);
		this.SendRPCInternal("System.Void CrossyRoad::RpcPenguinStep(System.Int32,System.Single)", -2082101845, writer, 0, true);
		NetworkWriterPool.Return(writer);
	}

	// Token: 0x060001EC RID: 492 RVA: 0x0000B2D0 File Offset: 0x000094D0
	[ClientRpc]
	private void RpcResetDuckPosition()
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		this.SendRPCInternal("System.Void CrossyRoad::RpcResetDuckPosition()", 614803394, writer, 0, true);
		NetworkWriterPool.Return(writer);
	}

	// Token: 0x060001ED RID: 493 RVA: 0x0000B300 File Offset: 0x00009500
	[ClientRpc]
	private void RpcWinFeedback(float mult)
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		writer.WriteFloat(mult);
		this.SendRPCInternal("System.Void CrossyRoad::RpcWinFeedback(System.Single)", 1987450247, writer, 0, true);
		NetworkWriterPool.Return(writer);
	}

	// Token: 0x060001EE RID: 494 RVA: 0x0000B33C File Offset: 0x0000953C
	[ClientRpc]
	private void RpcCrashFeedback(int step, float mult)
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		writer.WriteVarInt(step);
		writer.WriteFloat(mult);
		this.SendRPCInternal("System.Void CrossyRoad::RpcCrashFeedback(System.Int32,System.Single)", 2077413361, writer, 0, true);
		NetworkWriterPool.Return(writer);
	}

	// Token: 0x060001EF RID: 495 RVA: 0x0000B380 File Offset: 0x00009580
	public CrossyRoad()
	{
		this._Mirror_SyncVarHookDelegate_currentMultiplier = new Action<double, double>(this.OnMultiplierChanged);
	}

	// Token: 0x060001F1 RID: 497 RVA: 0x00002321 File Offset: 0x00000521
	public override bool Weaved()
	{
		return true;
	}

	// Token: 0x1700003A RID: 58
	// (get) Token: 0x060001F2 RID: 498 RVA: 0x0000B3DC File Offset: 0x000095DC
	// (set) Token: 0x060001F3 RID: 499 RVA: 0x0000B3EF File Offset: 0x000095EF
	public double NetworkcurrentMultiplier
	{
		get
		{
			return this.currentMultiplier;
		}
		[param: In]
		set
		{
			base.GeneratedSyncVarSetter<double>(value, ref this.currentMultiplier, 8UL, this._Mirror_SyncVarHookDelegate_currentMultiplier);
		}
	}

	// Token: 0x060001F4 RID: 500 RVA: 0x0000B410 File Offset: 0x00009610
	protected void UserCode_RpcPenguinStep__Int32__Single(int step, float mult)
	{
		Vector3 endValue = this.startPoint.position + (this.endPoint.position - this.startPoint.position).normalized * (this.stepDistance * (float)step);
		this.penguin.DOJump(endValue, this.jumpHeight, 1, this.stepTweenDuration, false).SetEase(Ease.Linear);
		this.penguinAnim.SetTrigger("Jump");
		SFXManager.SFXOneShotWithParameters(this.duckSFX, null, this.penguin.transform.position, 0.8f + mult * 0.04f);
	}

	// Token: 0x060001F5 RID: 501 RVA: 0x0000B4B9 File Offset: 0x000096B9
	protected static void InvokeUserCode_RpcPenguinStep__Int32__Single(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcPenguinStep called on server.");
			return;
		}
		((CrossyRoad)obj).UserCode_RpcPenguinStep__Int32__Single(reader.ReadVarInt(), reader.ReadFloat());
	}

	// Token: 0x060001F6 RID: 502 RVA: 0x0000B4E9 File Offset: 0x000096E9
	protected void UserCode_RpcResetDuckPosition()
	{
		this.penguin.DOMove(this.startPoint.position, this.stepTweenDuration, false).SetEase(Ease.OutQuad);
	}

	// Token: 0x060001F7 RID: 503 RVA: 0x0000B50F File Offset: 0x0000970F
	protected static void InvokeUserCode_RpcResetDuckPosition(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcResetDuckPosition called on server.");
			return;
		}
		((CrossyRoad)obj).UserCode_RpcResetDuckPosition();
	}

	// Token: 0x060001F8 RID: 504 RVA: 0x0000B534 File Offset: 0x00009734
	protected void UserCode_RpcWinFeedback__Single(float mult)
	{
		this.penguinAnim.SetTrigger("Win");
		Animator[] array = this.sidePenguinAnims;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].SetTrigger("Win");
		}
		SFXManager.SFXOneShotWithParameters(this.crossyWinSFX, null, this.penguin.transform.position, 0.8f + mult * 0.04f);
	}

	// Token: 0x060001F9 RID: 505 RVA: 0x0000B59C File Offset: 0x0000979C
	protected static void InvokeUserCode_RpcWinFeedback__Single(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcWinFeedback called on server.");
			return;
		}
		((CrossyRoad)obj).UserCode_RpcWinFeedback__Single(reader.ReadFloat());
	}

	// Token: 0x060001FA RID: 506 RVA: 0x0000B5C8 File Offset: 0x000097C8
	protected void UserCode_RpcCrashFeedback__Int32__Single(int step, float mult)
	{
		this.waterVfx.transform.position = this.iceCubes[step].transform.position;
		Transform target = this.iceCubes[step];
		Sequence s = DOTween.Sequence();
		s.Append(target.DOShakePosition(0.2f, 0.1f, 10, 90f, false, true, ShakeRandomnessMode.Full).SetEase(Ease.OutCirc));
		s.Append(target.DOLocalMoveY(-0.2f, 0.8f, false).SetEase(Ease.InCirc));
		s.AppendInterval(0.5f);
		s.AppendCallback(delegate
		{
			this.waterVfx.Play();
		});
		s.AppendInterval(0.5f);
		s.Append(target.DOLocalMoveY(0f, 0.5f, false).SetEase(Ease.OutBack));
		this.penguinAnim.SetTrigger("Crash");
		SFXManager.SFXOneShotWithParameters(this.crossyDuckLoseSFX, null, this.penguin.transform.position, 0.8f + mult * 0.04f);
		SFXManager.SFXOneShotWithParameters(this.crossyLoseSFX, null, base.transform.position, 1f);
	}

	// Token: 0x060001FB RID: 507 RVA: 0x0000B6EA File Offset: 0x000098EA
	protected static void InvokeUserCode_RpcCrashFeedback__Int32__Single(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcCrashFeedback called on server.");
			return;
		}
		((CrossyRoad)obj).UserCode_RpcCrashFeedback__Int32__Single(reader.ReadVarInt(), reader.ReadFloat());
	}

	// Token: 0x060001FC RID: 508 RVA: 0x0000B71C File Offset: 0x0000991C
	static CrossyRoad()
	{
		RemoteProcedureCalls.RegisterRpc(typeof(CrossyRoad), "System.Void CrossyRoad::RpcPenguinStep(System.Int32,System.Single)", new RemoteCallDelegate(CrossyRoad.InvokeUserCode_RpcPenguinStep__Int32__Single));
		RemoteProcedureCalls.RegisterRpc(typeof(CrossyRoad), "System.Void CrossyRoad::RpcResetDuckPosition()", new RemoteCallDelegate(CrossyRoad.InvokeUserCode_RpcResetDuckPosition));
		RemoteProcedureCalls.RegisterRpc(typeof(CrossyRoad), "System.Void CrossyRoad::RpcWinFeedback(System.Single)", new RemoteCallDelegate(CrossyRoad.InvokeUserCode_RpcWinFeedback__Single));
		RemoteProcedureCalls.RegisterRpc(typeof(CrossyRoad), "System.Void CrossyRoad::RpcCrashFeedback(System.Int32,System.Single)", new RemoteCallDelegate(CrossyRoad.InvokeUserCode_RpcCrashFeedback__Int32__Single));
	}

	// Token: 0x060001FD RID: 509 RVA: 0x0000B7AC File Offset: 0x000099AC
	public override void SerializeSyncVars(NetworkWriter writer, bool forceAll)
	{
		base.SerializeSyncVars(writer, forceAll);
		if (forceAll)
		{
			writer.WriteDouble(this.currentMultiplier);
			return;
		}
		writer.WriteVarULong(this.syncVarDirtyBits);
		if ((this.syncVarDirtyBits & 8UL) != 0UL)
		{
			writer.WriteDouble(this.currentMultiplier);
		}
	}

	// Token: 0x060001FE RID: 510 RVA: 0x0000B804 File Offset: 0x00009A04
	public override void DeserializeSyncVars(NetworkReader reader, bool initialState)
	{
		base.DeserializeSyncVars(reader, initialState);
		if (initialState)
		{
			base.GeneratedSyncVarDeserialize<double>(ref this.currentMultiplier, this._Mirror_SyncVarHookDelegate_currentMultiplier, reader.ReadDouble());
			return;
		}
		long num = (long)reader.ReadVarULong();
		if ((num & 8L) != 0L)
		{
			base.GeneratedSyncVarDeserialize<double>(ref this.currentMultiplier, this._Mirror_SyncVarHookDelegate_currentMultiplier, reader.ReadDouble());
		}
	}

	// Token: 0x04000193 RID: 403
	[Header("References")]
	[SerializeField]
	private TextMeshPro multiplierText;

	// Token: 0x04000194 RID: 404
	[Header("References")]
	[SerializeField]
	private TextMeshPro potentialWinningsText;

	// Token: 0x04000195 RID: 405
	[SerializeField]
	private Transform startPoint;

	// Token: 0x04000196 RID: 406
	[SerializeField]
	private Transform endPoint;

	// Token: 0x04000197 RID: 407
	[SerializeField]
	private Transform penguin;

	// Token: 0x04000198 RID: 408
	[SerializeField]
	private Animator penguinAnim;

	// Token: 0x04000199 RID: 409
	[SerializeField]
	private Animator[] sidePenguinAnims;

	// Token: 0x0400019A RID: 410
	[SerializeField]
	private Transform[] iceCubes;

	// Token: 0x0400019B RID: 411
	[SerializeField]
	private ParticleSystem waterVfx;

	// Token: 0x0400019C RID: 412
	[Header("Settings")]
	[SerializeField]
	private float stepDistance = 0.2f;

	// Token: 0x0400019D RID: 413
	[SerializeField]
	private float jumpHeight = 0.1f;

	// Token: 0x0400019E RID: 414
	[SerializeField]
	private float stepTweenDuration = 0.4f;

	// Token: 0x0400019F RID: 415
	[SerializeField]
	private int maxSteps = 10;

	// Token: 0x040001A0 RID: 416
	[SerializeField]
	private double[] stepMultipliers;

	// Token: 0x040001A1 RID: 417
	[SerializeField]
	[SyncVar(hook = "OnMultiplierChanged")]
	private double currentMultiplier;

	// Token: 0x040001A2 RID: 418
	[Header("SFX")]
	[SerializeField]
	private EventReference duckSFX;

	// Token: 0x040001A3 RID: 419
	[SerializeField]
	private EventReference crossyWinSFX;

	// Token: 0x040001A4 RID: 420
	[SerializeField]
	private EventReference crossyLoseSFX;

	// Token: 0x040001A5 RID: 421
	[SerializeField]
	private EventReference crossyDuckLoseSFX;

	// Token: 0x040001A6 RID: 422
	private int _currentStep;

	// Token: 0x040001A7 RID: 423
	private float _currentCrashChance;

	// Token: 0x040001A8 RID: 424
	private float _lastStepTime;

	// Token: 0x040001A9 RID: 425
	private bool _hasEnded;

	// Token: 0x040001AA RID: 426
	public Action<double, double> _Mirror_SyncVarHookDelegate_currentMultiplier;
}
