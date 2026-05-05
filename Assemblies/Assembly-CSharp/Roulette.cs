using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Extensions;
using Mirror;
using Mirror.RemoteCalls;
using TMPro;
using UnityEngine;

// Token: 0x02000079 RID: 121
public class Roulette : GameBase
{
	// Token: 0x0600045D RID: 1117 RVA: 0x00013C6E File Offset: 0x00011E6E
	private void OnEnable()
	{
		this.wheel.OnWheelStopped += this.HandleWheelStopped;
	}

	// Token: 0x0600045E RID: 1118 RVA: 0x00013C87 File Offset: 0x00011E87
	protected override void OnDisable()
	{
		base.OnDisable();
		this.wheel.OnWheelStopped -= this.HandleWheelStopped;
	}

	// Token: 0x0600045F RID: 1119 RVA: 0x00013CA8 File Offset: 0x00011EA8
	[Server]
	public override void TryStartGame(PlayerInteract playerInteract)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void Roulette::TryStartGame(PlayerInteract)' called when server was not active");
			return;
		}
		if (!this.CanGameStart())
		{
			return;
		}
		if (this.isPlaying)
		{
			return;
		}
		PlayerProfile interactingPlayer;
		if (playerInteract.TryGetComponent<PlayerProfile>(out interactingPlayer))
		{
			this.interactingPlayer = interactingPlayer;
		}
		long totalBet = this.GetTotalBet();
		if (totalBet < base.MinBet)
		{
			RouletteButton[] array = this.buttons;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].ServerWarningFeedback();
			}
			return;
		}
		if (!this.isGoldenChipApplied)
		{
			if (!NetworkSingleton<MoneyManager>.Instance.TryChangeBalance(-totalBet, this.interactingPlayer, ChangeType.Bet))
			{
				this.keypad.ServerInvalidBetAmountFb();
				return;
			}
		}
		else
		{
			this.isGoldenBet = true;
		}
		this.isPlaying = true;
		this.canBet = false;
		this.StartGame();
	}

	// Token: 0x06000460 RID: 1120 RVA: 0x00013D5F File Offset: 0x00011F5F
	[Server]
	protected override void StartGame()
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void Roulette::StartGame()' called when server was not active");
			return;
		}
		base.StartGame();
		this.wheel.SpinTheWheel(base.GetSeededRandom(0));
	}

	// Token: 0x06000461 RID: 1121 RVA: 0x00013D90 File Offset: 0x00011F90
	[Server]
	public void ResetBets()
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void Roulette::ResetBets()' called when server was not active");
			return;
		}
		if (this.isPlaying)
		{
			return;
		}
		this._bets.Clear();
		this._goldenBetOption = null;
		RouletteButton[] array = this.buttons;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].ServerSetBets(base.MaxBet, 0L);
		}
		this.RpcSetTotalBetText("$0");
	}

	// Token: 0x06000462 RID: 1122 RVA: 0x00013E00 File Offset: 0x00012000
	[ClientRpc]
	private void RpcSetTotalBetText(string text)
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		writer.WriteString(text);
		this.SendRPCInternal("System.Void Roulette::RpcSetTotalBetText(System.String)", -1387118594, writer, 0, true);
		NetworkWriterPool.Return(writer);
	}

	// Token: 0x06000463 RID: 1123 RVA: 0x00013E3C File Offset: 0x0001203C
	[Server]
	public void SelectBettingOption(string option, RouletteButton button)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void Roulette::SelectBettingOption(System.String,RouletteButton)' called when server was not active");
			return;
		}
		if (this.isPlaying)
		{
			return;
		}
		long num = this.keypad.GetCurrentInput();
		if (num <= 0L)
		{
			return;
		}
		if (this.isGoldenChipApplied)
		{
			if (string.IsNullOrEmpty(this._goldenBetOption))
			{
				this._goldenBetOption = option;
			}
			else if (this._goldenBetOption != option)
			{
				button.ServerWarningFeedback();
				return;
			}
		}
		long balance = NetworkSingleton<MoneyManager>.Instance.balance;
		long num2 = Math.Min(base.MaxBet, balance);
		long num3 = this._bets.ContainsKey(option) ? this._bets[option] : 0L;
		long num4;
		if (!this.isGoldenChipApplied)
		{
			num4 = num3 + num;
			if (num4 > num2)
			{
				num4 = num2;
				num = num4 - num3;
				if (num <= 0L)
				{
					return;
				}
			}
		}
		else
		{
			this.ResetBets();
			num4 = num;
		}
		this._bets[option] = num4;
		long totalBet = this.GetTotalBet();
		base.ServerSetBet(totalBet);
		this.RpcSetTotalBetText(MoneyFormatter.FormatWithDollar(totalBet));
		button.ServerSetBets(base.MaxBet, num4);
	}

	// Token: 0x06000464 RID: 1124 RVA: 0x00013F48 File Offset: 0x00012148
	private long GetTotalBet()
	{
		long num = 0L;
		foreach (long num2 in this._bets.Values)
		{
			num += num2;
		}
		return num;
	}

	// Token: 0x06000465 RID: 1125 RVA: 0x00013FA4 File Offset: 0x000121A4
	protected override void SetGoldenChip(bool apply, float multiplier = 1f)
	{
		if (this.isGoldenChipApplied == apply)
		{
			return;
		}
		base.NetworkisGoldenChipApplied = apply;
		if (!apply)
		{
			this.isGoldenBet = false;
		}
		if (!apply)
		{
			this._goldenBetOption = null;
		}
		this.ResetBets();
		this.keypad.SetGoldenChip(apply, multiplier);
	}

	// Token: 0x06000466 RID: 1126 RVA: 0x00013FE0 File Offset: 0x000121E0
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
		int resultNumber;
		if (!int.TryParse(result, out resultNumber))
		{
			this.EndGame(0.0);
			return;
		}
		double num = 0.0;
		foreach (KeyValuePair<string, long> keyValuePair in this._bets)
		{
			string key = keyValuePair.Key;
			long value = keyValuePair.Value;
			if (value > 0L)
			{
				double num2 = (double)this.CheckBetWin(key, resultNumber);
				if (num2 > 0.0)
				{
					num += (double)value * num2;
				}
			}
		}
		long totalBet = this.GetTotalBet();
		double multiplier = (totalBet > 0L) ? (num / (double)totalBet) : 0.0;
		this.EndGame(multiplier);
	}

	// Token: 0x06000467 RID: 1127 RVA: 0x000140C4 File Offset: 0x000122C4
	private float CheckBetWin(string betOption, int resultNumber)
	{
		if (string.IsNullOrEmpty(betOption))
		{
			return 0f;
		}
		int num;
		if (int.TryParse(betOption, out num))
		{
			if (resultNumber != num)
			{
				return 0f;
			}
			return 36f;
		}
		else if (betOption == "Red")
		{
			if (!Roulette.RedNumbers.Contains(resultNumber))
			{
				return 0f;
			}
			return 2f;
		}
		else if (betOption == "Black")
		{
			if (!Roulette.BlackNumbers.Contains(resultNumber))
			{
				return 0f;
			}
			return 2f;
		}
		else if (betOption == "Odd")
		{
			if (resultNumber == 0 || resultNumber % 2 != 1)
			{
				return 0f;
			}
			return 2f;
		}
		else if (betOption == "Even")
		{
			if (resultNumber == 0 || resultNumber % 2 != 0)
			{
				return 0f;
			}
			return 2f;
		}
		else if (betOption == "Low")
		{
			if (resultNumber < 1 || resultNumber > 18)
			{
				return 0f;
			}
			return 2f;
		}
		else if (betOption == "High")
		{
			if (resultNumber < 19 || resultNumber > 36)
			{
				return 0f;
			}
			return 2f;
		}
		else if (betOption == "Dozen1")
		{
			if (resultNumber < 1 || resultNumber > 12)
			{
				return 0f;
			}
			return 3f;
		}
		else if (betOption == "Dozen2")
		{
			if (resultNumber < 13 || resultNumber > 24)
			{
				return 0f;
			}
			return 3f;
		}
		else if (betOption == "Dozen3")
		{
			if (resultNumber < 25 || resultNumber > 36)
			{
				return 0f;
			}
			return 3f;
		}
		else if (betOption == "Column1")
		{
			if (!Roulette.Column1.Contains(resultNumber))
			{
				return 0f;
			}
			return 3f;
		}
		else if (betOption == "Column2")
		{
			if (!Roulette.Column2.Contains(resultNumber))
			{
				return 0f;
			}
			return 3f;
		}
		else
		{
			if (!(betOption == "Column3"))
			{
				return 0f;
			}
			if (!Roulette.Column3.Contains(resultNumber))
			{
				return 0f;
			}
			return 3f;
		}
	}

	// Token: 0x06000468 RID: 1128 RVA: 0x000142AC File Offset: 0x000124AC
	private void EndGame(double multiplier)
	{
		this.Payout(multiplier, ChangeType.GameResult, null, this.GetTotalBet());
		base.StartCoroutine(this.ResetGameRoutine());
	}

	// Token: 0x06000469 RID: 1129 RVA: 0x000142CA File Offset: 0x000124CA
	private IEnumerator ResetGameRoutine()
	{
		yield return new WaitForSeconds(1f);
		this.ResetGame();
		yield break;
	}

	// Token: 0x0600046A RID: 1130 RVA: 0x000142D9 File Offset: 0x000124D9
	protected override void ResetGame()
	{
		base.ResetGame();
		this._goldenBetOption = null;
		this.wheel.ResetWheel();
	}

	// Token: 0x0600046C RID: 1132 RVA: 0x00014308 File Offset: 0x00012508
	static Roulette()
	{
		RemoteProcedureCalls.RegisterRpc(typeof(Roulette), "System.Void Roulette::RpcSetTotalBetText(System.String)", new RemoteCallDelegate(Roulette.InvokeUserCode_RpcSetTotalBetText__String));
	}

	// Token: 0x0600046D RID: 1133 RVA: 0x00002321 File Offset: 0x00000521
	public override bool Weaved()
	{
		return true;
	}

	// Token: 0x0600046E RID: 1134 RVA: 0x000143A8 File Offset: 0x000125A8
	protected void UserCode_RpcSetTotalBetText__String(string text)
	{
		this.totalBetText.text = text;
	}

	// Token: 0x0600046F RID: 1135 RVA: 0x000143B6 File Offset: 0x000125B6
	protected static void InvokeUserCode_RpcSetTotalBetText__String(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcSetTotalBetText called on server.");
			return;
		}
		((Roulette)obj).UserCode_RpcSetTotalBetText__String(reader.ReadString());
	}

	// Token: 0x04000306 RID: 774
	[Header("References")]
	[SerializeField]
	private Wheel wheel;

	// Token: 0x04000307 RID: 775
	[SerializeField]
	private RouletteButton[] buttons;

	// Token: 0x04000308 RID: 776
	[SerializeField]
	private TextMeshPro totalBetText;

	// Token: 0x04000309 RID: 777
	private Dictionary<string, long> _bets = new Dictionary<string, long>();

	// Token: 0x0400030A RID: 778
	private string _goldenBetOption;

	// Token: 0x0400030B RID: 779
	private static readonly int[] RedNumbers = new int[]
	{
		1,
		3,
		5,
		7,
		9,
		12,
		14,
		16,
		18,
		19,
		21,
		23,
		25,
		27,
		30,
		32,
		34,
		36
	};

	// Token: 0x0400030C RID: 780
	private static readonly int[] BlackNumbers = new int[]
	{
		2,
		4,
		6,
		8,
		10,
		11,
		13,
		15,
		17,
		20,
		22,
		24,
		26,
		28,
		29,
		31,
		33,
		35
	};

	// Token: 0x0400030D RID: 781
	private static readonly int[] Column1 = new int[]
	{
		1,
		4,
		7,
		10,
		13,
		16,
		19,
		22,
		25,
		28,
		31,
		34
	};

	// Token: 0x0400030E RID: 782
	private static readonly int[] Column2 = new int[]
	{
		2,
		5,
		8,
		11,
		14,
		17,
		20,
		23,
		26,
		29,
		32,
		35
	};

	// Token: 0x0400030F RID: 783
	private static readonly int[] Column3 = new int[]
	{
		3,
		6,
		9,
		12,
		15,
		18,
		21,
		24,
		27,
		30,
		33,
		36
	};
}
