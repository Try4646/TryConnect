using System;
using System.Collections;
using System.Globalization;
using Mirror;
using UnityEngine;

// Token: 0x02000086 RID: 134
public class WheelOfFortune : GameBase
{
	// Token: 0x060004CE RID: 1230 RVA: 0x00015AD5 File Offset: 0x00013CD5
	private void OnEnable()
	{
		this.wheel.OnWheelStopped += this.HandleWheelStopped;
	}

	// Token: 0x060004CF RID: 1231 RVA: 0x00015AEE File Offset: 0x00013CEE
	protected override void OnDisable()
	{
		base.OnDisable();
		this.wheel.OnWheelStopped -= this.HandleWheelStopped;
	}

	// Token: 0x060004D0 RID: 1232 RVA: 0x00015B0D File Offset: 0x00013D0D
	[Server]
	protected override void StartGame()
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void WheelOfFortune::StartGame()' called when server was not active");
			return;
		}
		base.StartGame();
		this.wheel.SpinTheWheel(base.GetSeededRandom(0));
	}

	// Token: 0x060004D1 RID: 1233 RVA: 0x00015B3C File Offset: 0x00013D3C
	private void HandleWheelStopped(string result)
	{
		if (!base.isServer)
		{
			return;
		}
		if (!this.isPlaying)
		{
			return;
		}
		if (result == "Spin")
		{
			this.gameTurn++;
			this.wheel.SpinTheWheel(base.GetSeededRandom(0));
			return;
		}
		decimal value;
		if (decimal.TryParse(result, NumberStyles.Float, CultureInfo.InvariantCulture, out value))
		{
			this.EndGame((double)value);
			return;
		}
		this.EndGame(0.0);
	}

	// Token: 0x060004D2 RID: 1234 RVA: 0x00015BBA File Offset: 0x00013DBA
	private void EndGame(double multiplier)
	{
		this.Payout(multiplier * base.EstimatedValue, ChangeType.GameResult, null, -1L);
		base.StartCoroutine(this.ResetGameRoutine());
	}

	// Token: 0x060004D3 RID: 1235 RVA: 0x00015BDB File Offset: 0x00013DDB
	private IEnumerator ResetGameRoutine()
	{
		yield return new WaitForSeconds(1f);
		this.ResetGame();
		yield break;
	}

	// Token: 0x060004D4 RID: 1236 RVA: 0x00015BEA File Offset: 0x00013DEA
	protected override void ResetGame()
	{
		this.wheel.ResetWheel();
		base.ResetGame();
	}

	// Token: 0x060004D6 RID: 1238 RVA: 0x00002321 File Offset: 0x00000521
	public override bool Weaved()
	{
		return true;
	}

	// Token: 0x0400034A RID: 842
	[Header("References")]
	[SerializeField]
	private Wheel wheel;
}
