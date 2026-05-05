using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using Mirror.RemoteCalls;
using MoreMountains.Feedbacks;
using TMPro;
using UnityEngine;

// Token: 0x02000041 RID: 65
public class Craps : GameBase
{
	// Token: 0x06000171 RID: 369 RVA: 0x00008D23 File Offset: 0x00006F23
	private void OnEnable()
	{
		this.diceManager.OnDiceRolled += this.HandleDiceRolled;
	}

	// Token: 0x06000172 RID: 370 RVA: 0x00008D3C File Offset: 0x00006F3C
	protected override void OnDisable()
	{
		base.OnDisable();
		this.diceManager.OnDiceRolled -= this.HandleDiceRolled;
	}

	// Token: 0x06000173 RID: 371 RVA: 0x00008D5B File Offset: 0x00006F5B
	[Server]
	protected override void StartGame()
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void Craps::StartGame()' called when server was not active");
			return;
		}
		this.diceManager.ResetRound();
		this.RpcSetMultiplierAndWinningText("2x", MoneyFormatter.FormatWithDollar(this.currentBet * 2L));
	}

	// Token: 0x06000174 RID: 372 RVA: 0x00008D98 File Offset: 0x00006F98
	private void HandleDiceRolled(int result)
	{
		if (!this.isPlaying)
		{
			return;
		}
		if (!base.isServer)
		{
			return;
		}
		this._lastRolledValue = result;
		Gradient gradient = new Gradient();
		GradientColorKey[] array = new GradientColorKey[2];
		GradientAlphaKey[] alphaKeys = new GradientAlphaKey[]
		{
			new GradientAlphaKey(1f, 0f),
			new GradientAlphaKey(1f, 1f)
		};
		if (!this._isPointPhase)
		{
			if (result == 7 || result == 11)
			{
				array[0] = new GradientColorKey(new Color(0.2f, 0.6f, 0.2f), 0f);
				array[1] = new GradientColorKey(new Color(0.4f, 0.8f, 0.2f), 1f);
				this.Win(2.0);
			}
			else if (result == 2 || result == 3 || result == 12)
			{
				array[0] = new GradientColorKey(new Color(0.7f, 0.2f, 0.1f), 0f);
				array[1] = new GradientColorKey(new Color(0.9f, 0.4f, 0.1f), 1f);
				this.Lose();
			}
			else
			{
				array[0] = new GradientColorKey(new Color(0.7f, 0.8f, 0.2f), 0f);
				array[1] = new GradientColorKey(new Color(0.9f, 0.8f, 0.2f), 1f);
				this._isPointPhase = true;
				this._pointValue = result;
				this._pointPhaseThrows = 0;
				this.diceManager.ResetRound();
				this.RpcSetWinConditionText(this._pointValue.ToString());
				this.RpcSetLoseConditionText("7");
				this.RpcSetMultiplierAndWinningText("4x", MoneyFormatter.FormatWithDollar(this.currentBet * 4L));
			}
		}
		else
		{
			this._pointPhaseThrows++;
			if (result == this._pointValue)
			{
				array[0] = new GradientColorKey(new Color(0.2f, 0.6f, 0.2f), 0f);
				array[1] = new GradientColorKey(new Color(0.4f, 0.8f, 0.2f), 1f);
				this.Win(4.0);
			}
			else if (result == 7)
			{
				array[0] = new GradientColorKey(new Color(0.7f, 0.2f, 0.1f), 0f);
				array[1] = new GradientColorKey(new Color(0.9f, 0.4f, 0.1f), 1f);
				this.Lose();
			}
			else if (this._pointPhaseThrows >= this.maxPointPhaseThrows)
			{
				array[0] = new GradientColorKey(new Color(0.7f, 0.8f, 0.2f), 0f);
				array[1] = new GradientColorKey(new Color(0.9f, 0.8f, 0.2f), 1f);
				this.RpcDiceRolledFeedback(gradient, "REFUND");
				this.PushRefund();
			}
			else
			{
				array[0] = new GradientColorKey(new Color(0.2f, 0.5f, 0.8f), 0f);
				array[1] = new GradientColorKey(new Color(0.2f, 0.7f, 0.9f), 1f);
				this.diceManager.ResetRound();
			}
		}
		gradient.SetKeys(array, alphaKeys);
		this.RpcDiceRolledFeedback(gradient, result.ToString());
	}

	// Token: 0x06000175 RID: 373 RVA: 0x0000911C File Offset: 0x0000731C
	[ClientRpc]
	private void RpcDiceRolledFeedback(Gradient gradient, string result)
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		Mirror.GeneratedNetworkCode._Write_UnityEngine.Gradient(writer, gradient);
		writer.WriteString(result);
		this.SendRPCInternal("System.Void Craps::RpcDiceRolledFeedback(UnityEngine.Gradient,System.String)", 868177736, writer, 0, true);
		NetworkWriterPool.Return(writer);
	}

	// Token: 0x06000176 RID: 374 RVA: 0x00009160 File Offset: 0x00007360
	private void Win(double multiplier)
	{
		Dictionary<string, object> gameSpecificData = new Dictionary<string, object>
		{
			{
				"lastDiceRoll",
				this._lastRolledValue
			},
			{
				"pointValue",
				this._pointValue
			}
		};
		this.Payout(multiplier * base.EstimatedValue, ChangeType.GameResult, gameSpecificData, -1L);
		base.StartCoroutine(this.ResetGameRoutine());
	}

	// Token: 0x06000177 RID: 375 RVA: 0x000091C0 File Offset: 0x000073C0
	private void Lose()
	{
		Dictionary<string, object> gameSpecificData = new Dictionary<string, object>
		{
			{
				"lastDiceRoll",
				this._lastRolledValue
			},
			{
				"pointValue",
				this._pointValue
			}
		};
		this.Payout(0.0, ChangeType.GameResult, gameSpecificData, -1L);
		base.StartCoroutine(this.ResetGameRoutine());
	}

	// Token: 0x06000178 RID: 376 RVA: 0x00009220 File Offset: 0x00007420
	private void PushRefund()
	{
		Dictionary<string, object> gameSpecificData = new Dictionary<string, object>
		{
			{
				"lastDiceRoll",
				this._lastRolledValue
			},
			{
				"pointValue",
				this._pointValue
			}
		};
		this.Payout(1.0, ChangeType.GameResult, gameSpecificData, -1L);
		base.StartCoroutine(this.ResetGameRoutine());
	}

	// Token: 0x06000179 RID: 377 RVA: 0x0000927F File Offset: 0x0000747F
	private IEnumerator ResetGameRoutine()
	{
		yield return new WaitForSeconds(1f);
		this.ResetGame();
		yield break;
	}

	// Token: 0x0600017A RID: 378 RVA: 0x00009290 File Offset: 0x00007490
	protected override void ResetGame()
	{
		this.diceManager.LockDices(true);
		this._isPointPhase = false;
		this._pointValue = 0;
		this._pointPhaseThrows = 0;
		this.RpcSetLoseConditionText("2 / 3 / 12");
		this.RpcSetWinConditionText("7 / 11");
		this.RpcSetMultiplierAndWinningText("", "");
		base.ResetGame();
	}

	// Token: 0x0600017B RID: 379 RVA: 0x000092EC File Offset: 0x000074EC
	[ClientRpc]
	private void RpcSetLoseConditionText(string text)
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		writer.WriteString(text);
		this.SendRPCInternal("System.Void Craps::RpcSetLoseConditionText(System.String)", 1778590780, writer, 0, true);
		NetworkWriterPool.Return(writer);
	}

	// Token: 0x0600017C RID: 380 RVA: 0x00009328 File Offset: 0x00007528
	[ClientRpc]
	private void RpcSetWinConditionText(string text)
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		writer.WriteString(text);
		this.SendRPCInternal("System.Void Craps::RpcSetWinConditionText(System.String)", -1225143097, writer, 0, true);
		NetworkWriterPool.Return(writer);
	}

	// Token: 0x0600017D RID: 381 RVA: 0x00009364 File Offset: 0x00007564
	[ClientRpc]
	private void RpcSetMultiplierAndWinningText(string multText, string winText)
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		writer.WriteString(multText);
		writer.WriteString(winText);
		this.SendRPCInternal("System.Void Craps::RpcSetMultiplierAndWinningText(System.String,System.String)", 1435110022, writer, 0, true);
		NetworkWriterPool.Return(writer);
	}

	// Token: 0x0600017F RID: 383 RVA: 0x00002321 File Offset: 0x00000521
	public override bool Weaved()
	{
		return true;
	}

	// Token: 0x06000180 RID: 384 RVA: 0x000093B7 File Offset: 0x000075B7
	protected void UserCode_RpcDiceRolledFeedback__Gradient__String(Gradient gradient, string result)
	{
		MMF_FloatingText feedbackOfType = this.diceRollFeedback.GetFeedbackOfType<MMF_FloatingText>();
		feedbackOfType.Value = result;
		feedbackOfType.AnimateColorGradient = gradient;
		this.diceRollFeedback.PlayFeedbacks();
	}

	// Token: 0x06000181 RID: 385 RVA: 0x000093DC File Offset: 0x000075DC
	protected static void InvokeUserCode_RpcDiceRolledFeedback__Gradient__String(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcDiceRolledFeedback called on server.");
			return;
		}
		((Craps)obj).UserCode_RpcDiceRolledFeedback__Gradient__String(Mirror.GeneratedNetworkCode._Read_UnityEngine.Gradient(reader), reader.ReadString());
	}

	// Token: 0x06000182 RID: 386 RVA: 0x0000940B File Offset: 0x0000760B
	protected void UserCode_RpcSetLoseConditionText__String(string text)
	{
		this.loseConditionText.text = text;
	}

	// Token: 0x06000183 RID: 387 RVA: 0x00009419 File Offset: 0x00007619
	protected static void InvokeUserCode_RpcSetLoseConditionText__String(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcSetLoseConditionText called on server.");
			return;
		}
		((Craps)obj).UserCode_RpcSetLoseConditionText__String(reader.ReadString());
	}

	// Token: 0x06000184 RID: 388 RVA: 0x00009442 File Offset: 0x00007642
	protected void UserCode_RpcSetWinConditionText__String(string text)
	{
		this.winConditionText.text = text;
	}

	// Token: 0x06000185 RID: 389 RVA: 0x00009450 File Offset: 0x00007650
	protected static void InvokeUserCode_RpcSetWinConditionText__String(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcSetWinConditionText called on server.");
			return;
		}
		((Craps)obj).UserCode_RpcSetWinConditionText__String(reader.ReadString());
	}

	// Token: 0x06000186 RID: 390 RVA: 0x00009479 File Offset: 0x00007679
	protected void UserCode_RpcSetMultiplierAndWinningText__String__String(string multText, string winText)
	{
		this.multiplierText.text = multText;
		this.potentialWinText.text = winText;
	}

	// Token: 0x06000187 RID: 391 RVA: 0x00009493 File Offset: 0x00007693
	protected static void InvokeUserCode_RpcSetMultiplierAndWinningText__String__String(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcSetMultiplierAndWinningText called on server.");
			return;
		}
		((Craps)obj).UserCode_RpcSetMultiplierAndWinningText__String__String(reader.ReadString(), reader.ReadString());
	}

	// Token: 0x06000188 RID: 392 RVA: 0x000094C4 File Offset: 0x000076C4
	static Craps()
	{
		RemoteProcedureCalls.RegisterRpc(typeof(Craps), "System.Void Craps::RpcDiceRolledFeedback(UnityEngine.Gradient,System.String)", new RemoteCallDelegate(Craps.InvokeUserCode_RpcDiceRolledFeedback__Gradient__String));
		RemoteProcedureCalls.RegisterRpc(typeof(Craps), "System.Void Craps::RpcSetLoseConditionText(System.String)", new RemoteCallDelegate(Craps.InvokeUserCode_RpcSetLoseConditionText__String));
		RemoteProcedureCalls.RegisterRpc(typeof(Craps), "System.Void Craps::RpcSetWinConditionText(System.String)", new RemoteCallDelegate(Craps.InvokeUserCode_RpcSetWinConditionText__String));
		RemoteProcedureCalls.RegisterRpc(typeof(Craps), "System.Void Craps::RpcSetMultiplierAndWinningText(System.String,System.String)", new RemoteCallDelegate(Craps.InvokeUserCode_RpcSetMultiplierAndWinningText__String__String));
	}

	// Token: 0x04000149 RID: 329
	[Header("References")]
	[SerializeField]
	private DiceManager diceManager;

	// Token: 0x0400014A RID: 330
	[SerializeField]
	private TextMeshPro winConditionText;

	// Token: 0x0400014B RID: 331
	[SerializeField]
	private TextMeshPro loseConditionText;

	// Token: 0x0400014C RID: 332
	[SerializeField]
	private TextMeshPro multiplierText;

	// Token: 0x0400014D RID: 333
	[SerializeField]
	private TextMeshPro potentialWinText;

	// Token: 0x0400014E RID: 334
	[SerializeField]
	private MMF_Player diceRollFeedback;

	// Token: 0x0400014F RID: 335
	[Header("Point Phase Rules")]
	[SerializeField]
	private int maxPointPhaseThrows = 5;

	// Token: 0x04000150 RID: 336
	private int _pointValue;

	// Token: 0x04000151 RID: 337
	private bool _isPointPhase;

	// Token: 0x04000152 RID: 338
	private int _lastRolledValue;

	// Token: 0x04000153 RID: 339
	private int _pointPhaseThrows;
}
