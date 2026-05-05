using System;
using System.Collections;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using Mirror;
using Mirror.RemoteCalls;
using TMPro;
using UnityEngine;

// Token: 0x02000055 RID: 85
public class HiLoGame : GameBase
{
	// Token: 0x0600027F RID: 639 RVA: 0x0000D364 File Offset: 0x0000B564
	private void OnEnable()
	{
		HiLoSlider hiLoSlider = this.hiLoSlider;
		hiLoSlider.OnValueChangedAction = (Action<float>)Delegate.Combine(hiLoSlider.OnValueChangedAction, new Action<float>(this.HandleValueChanged));
	}

	// Token: 0x06000280 RID: 640 RVA: 0x0000D38D File Offset: 0x0000B58D
	protected override void OnDisable()
	{
		base.OnDisable();
		HiLoSlider hiLoSlider = this.hiLoSlider;
		hiLoSlider.OnValueChangedAction = (Action<float>)Delegate.Remove(hiLoSlider.OnValueChangedAction, new Action<float>(this.HandleValueChanged));
	}

	// Token: 0x06000281 RID: 641 RVA: 0x0000D3BC File Offset: 0x0000B5BC
	private void HandleValueChanged(float value)
	{
		if (!base.isServer)
		{
			return;
		}
		this.RpcSetIsOver(value, this._isOver);
	}

	// Token: 0x06000282 RID: 642 RVA: 0x0000D3D4 File Offset: 0x0000B5D4
	protected override void OnBetSet()
	{
		this.RpcSetIsOver(this.hiLoSlider.currentValue, this._isOver);
	}

	// Token: 0x06000283 RID: 643 RVA: 0x0000D3F0 File Offset: 0x0000B5F0
	[ClientRpc]
	private void RpcSetIsOver(float value, bool isOver)
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		writer.WriteFloat(value);
		writer.WriteBool(isOver);
		this.SendRPCInternal("System.Void HiLoGame::RpcSetIsOver(System.Single,System.Boolean)", 1068155300, writer, 0, true);
		NetworkWriterPool.Return(writer);
	}

	// Token: 0x06000284 RID: 644 RVA: 0x0000D434 File Offset: 0x0000B634
	[Server]
	public void SetOver(bool isOver)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void HiLoGame::SetOver(System.Boolean)' called when server was not active");
			return;
		}
		if (this.isPlaying)
		{
			return;
		}
		this._isOver = isOver;
		this.RpcSetIsOver(this.hiLoSlider.currentValue, this._isOver);
	}

	// Token: 0x06000285 RID: 645 RVA: 0x0000D472 File Offset: 0x0000B672
	protected override void StartGame()
	{
		base.StartGame();
		this.hiLoSlider.LockSlider(true);
		this.RollDice();
	}

	// Token: 0x06000286 RID: 646 RVA: 0x0000D48C File Offset: 0x0000B68C
	private void RollDice()
	{
		float roll = (float)base.GetSeededRandom(0).NextDouble();
		this.RpcRollDice(roll, this._isOver);
	}

	// Token: 0x06000287 RID: 647 RVA: 0x0000D4B4 File Offset: 0x0000B6B4
	[ClientRpc]
	private void RpcRollDice(float roll, bool isOver)
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		writer.WriteFloat(roll);
		writer.WriteBool(isOver);
		this.SendRPCInternal("System.Void HiLoGame::RpcRollDice(System.Single,System.Boolean)", -1523939456, writer, 0, true);
		NetworkWriterPool.Return(writer);
	}

	// Token: 0x06000288 RID: 648 RVA: 0x0000D4F8 File Offset: 0x0000B6F8
	private void RollDiceFeedbacks(float roll, bool isOver)
	{
		this._sfxLoopComponent.LoopSFX(true);
		this.rollResultText.text = "";
		float startX = this.rollStartPoint.localPosition.x;
		float endX = this.rollEndPoint.localPosition.x;
		float endValue = Mathf.Lerp(startX, endX, roll);
		Sequence sequence = DOTween.Sequence();
		float endValue2 = isOver ? startX : endX;
		float duration = (isOver ? roll : (1f - roll)) * 5f;
		sequence.Append(this.rollResultIndicator.DOLocalMoveX(endValue2, 0.5f, false).SetEase(Ease.InOutQuad));
		sequence.Append(this.rollResultIndicator.DOLocalMoveX(endValue, duration, false).SetEase(Ease.Linear));
		sequence.Join(this.rollResultDice.DOShakeRotation(duration, 45f, 0, 15f, true, ShakeRandomnessMode.Harmonic));
		sequence.Join(this.rollResultDice.parent.DOLocalRotate(Random.insideUnitSphere * 1080f, duration, RotateMode.FastBeyond360).SetEase(Ease.OutSine));
		sequence.Join(this.rollResultDice.DOShakeScale(duration, 1f, 5, 15f, false, ShakeRandomnessMode.Full));
		sequence.OnUpdate(delegate
		{
			this.rollResultText.text = (Mathf.InverseLerp(startX, endX, this.rollResultIndicator.localPosition.x) * 100f).ToString("0.0") + "%";
			this._sfxLoopComponent.ModulatePitch(Mathf.InverseLerp(startX, endX, this.rollResultIndicator.localPosition.x) + 0.63f);
		});
		if (base.isServer)
		{
			sequence.OnComplete(delegate
			{
				this.rollResultDice.parent.DOPunchScale(Vector3.one, 0.5f, 1, 0.5f).SetEase(Ease.InOutSine);
				this._sfxLoopComponent.LoopSFX(false);
				this.EndGame(roll);
			});
			return;
		}
		sequence.OnComplete(delegate
		{
			this.rollResultDice.parent.DOPunchScale(Vector3.one, 0.5f, 1, 0.5f).SetEase(Ease.InOutSine);
			this._sfxLoopComponent.LoopSFX(false);
		});
	}

	// Token: 0x06000289 RID: 649 RVA: 0x0000D6A0 File Offset: 0x0000B8A0
	private void EndGame(float roll)
	{
		if (this._isOver)
		{
			if (roll >= this.hiLoSlider.currentValue)
			{
				this.Win();
			}
			else
			{
				this.Lose();
			}
		}
		else if (roll <= this.hiLoSlider.currentValue)
		{
			this.Win();
		}
		else
		{
			this.Lose();
		}
		base.StartCoroutine(this.ResetGameRoutine());
	}

	// Token: 0x0600028A RID: 650 RVA: 0x0000D6FC File Offset: 0x0000B8FC
	private IEnumerator ResetGameRoutine()
	{
		yield return new WaitForSeconds(1f);
		this.ResetGame();
		yield break;
	}

	// Token: 0x0600028B RID: 651 RVA: 0x0000D70B File Offset: 0x0000B90B
	protected override void ResetGame()
	{
		base.ResetGame();
		this.hiLoSlider.LockSlider(false);
	}

	// Token: 0x0600028C RID: 652 RVA: 0x0000D720 File Offset: 0x0000B920
	private void Win()
	{
		double num = this._isOver ? (1.0 - (double)this.hiLoSlider.currentValue) : ((double)this.hiLoSlider.currentValue);
		double multiplier = base.EstimatedValue / num;
		this.Payout(multiplier, ChangeType.GameResult, null, -1L);
	}

	// Token: 0x0600028D RID: 653 RVA: 0x0000D76E File Offset: 0x0000B96E
	private void Lose()
	{
		this.Payout(0.0, ChangeType.GameResult, null, -1L);
	}

	// Token: 0x0600028F RID: 655 RVA: 0x00002321 File Offset: 0x00000521
	public override bool Weaved()
	{
		return true;
	}

	// Token: 0x06000290 RID: 656 RVA: 0x0000D784 File Offset: 0x0000B984
	protected void UserCode_RpcSetIsOver__Single__Boolean(float value, bool isOver)
	{
		double num = (double)(isOver ? (1f - value) : value);
		this.multiplierText.text = (base.EstimatedValue / num).ToString("0.##") + "x";
		this.potentialWinningText.text = "$" + ((long)Math.Round((double)this.currentBet * base.EstimatedValue / num)).ToString();
		this.underIndicator.material = (isOver ? this.inactiveIndicator : this.activeIndicator);
		this.overIndicator.material = (isOver ? this.activeIndicator : this.inactiveIndicator);
	}

	// Token: 0x06000291 RID: 657 RVA: 0x0000D83F File Offset: 0x0000BA3F
	protected static void InvokeUserCode_RpcSetIsOver__Single__Boolean(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcSetIsOver called on server.");
			return;
		}
		((HiLoGame)obj).UserCode_RpcSetIsOver__Single__Boolean(reader.ReadFloat(), reader.ReadBool());
	}

	// Token: 0x06000292 RID: 658 RVA: 0x0000D86F File Offset: 0x0000BA6F
	protected void UserCode_RpcRollDice__Single__Boolean(float roll, bool isOver)
	{
		this.RollDiceFeedbacks(roll, isOver);
	}

	// Token: 0x06000293 RID: 659 RVA: 0x0000D879 File Offset: 0x0000BA79
	protected static void InvokeUserCode_RpcRollDice__Single__Boolean(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcRollDice called on server.");
			return;
		}
		((HiLoGame)obj).UserCode_RpcRollDice__Single__Boolean(reader.ReadFloat(), reader.ReadBool());
	}

	// Token: 0x06000294 RID: 660 RVA: 0x0000D8AC File Offset: 0x0000BAAC
	static HiLoGame()
	{
		RemoteProcedureCalls.RegisterRpc(typeof(HiLoGame), "System.Void HiLoGame::RpcSetIsOver(System.Single,System.Boolean)", new RemoteCallDelegate(HiLoGame.InvokeUserCode_RpcSetIsOver__Single__Boolean));
		RemoteProcedureCalls.RegisterRpc(typeof(HiLoGame), "System.Void HiLoGame::RpcRollDice(System.Single,System.Boolean)", new RemoteCallDelegate(HiLoGame.InvokeUserCode_RpcRollDice__Single__Boolean));
	}

	// Token: 0x040001F1 RID: 497
	[Header("References")]
	[SerializeField]
	private HiLoSlider hiLoSlider;

	// Token: 0x040001F2 RID: 498
	[SerializeField]
	private Transform rollResultIndicator;

	// Token: 0x040001F3 RID: 499
	[SerializeField]
	private Transform rollResultDice;

	// Token: 0x040001F4 RID: 500
	[SerializeField]
	private TextMeshPro rollResultText;

	// Token: 0x040001F5 RID: 501
	[SerializeField]
	private Transform rollStartPoint;

	// Token: 0x040001F6 RID: 502
	[SerializeField]
	private Transform rollEndPoint;

	// Token: 0x040001F7 RID: 503
	[SerializeField]
	private TextMeshPro multiplierText;

	// Token: 0x040001F8 RID: 504
	[SerializeField]
	private TextMeshPro potentialWinningText;

	// Token: 0x040001F9 RID: 505
	[SerializeField]
	private MeshRenderer underIndicator;

	// Token: 0x040001FA RID: 506
	[SerializeField]
	private MeshRenderer overIndicator;

	// Token: 0x040001FB RID: 507
	[SerializeField]
	private Material activeIndicator;

	// Token: 0x040001FC RID: 508
	[SerializeField]
	private Material inactiveIndicator;

	// Token: 0x040001FD RID: 509
	[Header("SFX")]
	[SerializeField]
	private SFXLoopComponent _sfxLoopComponent;

	// Token: 0x040001FE RID: 510
	private bool _isOver;
}
