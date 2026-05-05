using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using FMODUnity;
using Mirror;
using Mirror.RemoteCalls;
using TMPro;
using UnityEngine;

// Token: 0x0200005E RID: 94
public class Keno : GameBase
{
	// Token: 0x17000056 RID: 86
	// (get) Token: 0x06000320 RID: 800 RVA: 0x0000F897 File Offset: 0x0000DA97
	private KenoButton[] Buttons
	{
		get
		{
			if (this._buttons == null || this._buttons.Length == 0)
			{
				this._buttons = this.buttonsParent.GetComponentsInChildren<KenoButton>();
			}
			return this._buttons;
		}
	}

	// Token: 0x06000321 RID: 801 RVA: 0x0000F8C4 File Offset: 0x0000DAC4
	private void OnMultiplierChanged(double oldMult, double newMult)
	{
		this.multiplierText.text = newMult.ToString("0.##") + "x";
		this.potentialWinningText.text = "$" + Math.Round((double)this.currentBet * newMult).ToString("N0");
	}

	// Token: 0x06000322 RID: 802 RVA: 0x0000F924 File Offset: 0x0000DB24
	protected override bool CanGameStart()
	{
		if (this._selectedButtons.Count <= 0)
		{
			KenoButton[] buttons = this.Buttons;
			for (int i = 0; i < buttons.Length; i++)
			{
				buttons[i].ServerWarningFeedback();
			}
			this.RpcWarningFeedback();
			return false;
		}
		return true;
	}

	// Token: 0x06000323 RID: 803 RVA: 0x0000F965 File Offset: 0x0000DB65
	[Server]
	protected override void StartGame()
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void Keno::StartGame()' called when server was not active");
			return;
		}
		base.StartGame();
		base.StartCoroutine(this.GameRoutine());
	}

	// Token: 0x06000324 RID: 804 RVA: 0x0000F98F File Offset: 0x0000DB8F
	private IEnumerator GameRoutine()
	{
		List<int> selectedNumbers = FathF.GetUniqueRandomNumbers(this.diamondCount, 0, this.Buttons.Length - 1, true);
		int hitCount = 0;
		int num;
		for (int i = 0; i < selectedNumbers.Count; i = num + 1)
		{
			KenoButton kenoButton = this.Buttons[selectedNumbers[i]];
			kenoButton.ServerRevealDiamond(true);
			if (this._selectedButtons.Contains(kenoButton))
			{
				num = hitCount;
				hitCount = num + 1;
				this.Network_currentMultiplier = this.GetMultiplier(hitCount);
				this.RpcDiamondRevealFeedback(true, kenoButton.transform.position, i);
			}
			else
			{
				this.RpcDiamondRevealFeedback(false, kenoButton.transform.position, i);
			}
			yield return new WaitForSeconds(this.revealDelay);
			num = i;
		}
		this.EndGame();
		yield break;
	}

	// Token: 0x06000325 RID: 805 RVA: 0x0000F99E File Offset: 0x0000DB9E
	private void EndGame()
	{
		this.Payout(this._currentMultiplier, ChangeType.GameResult, null, -1L);
		base.StartCoroutine(this.ResetGameRoutine());
	}

	// Token: 0x06000326 RID: 806 RVA: 0x0000F9BD File Offset: 0x0000DBBD
	private IEnumerator ResetGameRoutine()
	{
		yield return new WaitForSeconds(1f);
		this.ResetGame();
		yield break;
	}

	// Token: 0x06000327 RID: 807 RVA: 0x0000F9CC File Offset: 0x0000DBCC
	protected override void ResetGame()
	{
		base.ResetGame();
		KenoButton[] buttons = this.Buttons;
		for (int i = 0; i < buttons.Length; i++)
		{
			buttons[i].ServerRevealDiamond(false);
		}
		this.Network_currentMultiplier = 0.0;
	}

	// Token: 0x06000328 RID: 808 RVA: 0x0000FA0C File Offset: 0x0000DC0C
	private double GetMultiplier(int hitCount)
	{
		int n = this.Buttons.Length;
		int count = this._selectedButtons.Count;
		int m = this.diamondCount;
		return this.GetBalancedMultipliers(n, count, m)[hitCount] * base.EstimatedValue;
	}

	// Token: 0x06000329 RID: 809 RVA: 0x0000FA4C File Offset: 0x0000DC4C
	private Dictionary<int, double> GetBalancedMultipliers(int N, int k, int m)
	{
		Dictionary<int, double> dictionary = new Dictionary<int, double>();
		double num = 0.0;
		for (int i = 0; i <= Math.Min(k, m); i++)
		{
			double num2 = this.Hypergeometric(N, m, k, i);
			dictionary[i] = num2;
			num += num2;
		}
		Dictionary<int, float> rawMultipliers = dictionary.ToDictionary((KeyValuePair<int, double> kv) => kv.Key, (KeyValuePair<int, double> kv) => (float)Math.Pow((double)kv.Key, (double)this.riskRewardRatio));
		rawMultipliers[0] = 0f;
		double num3 = dictionary.Sum((KeyValuePair<int, double> kv) => kv.Value * (double)rawMultipliers[kv.Key]);
		double scale = 1.0 / num3;
		return rawMultipliers.ToDictionary((KeyValuePair<int, float> kv) => kv.Key, (KeyValuePair<int, float> kv) => (double)kv.Value * scale);
	}

	// Token: 0x0600032A RID: 810 RVA: 0x0000FB4A File Offset: 0x0000DD4A
	private double Hypergeometric(int N, int m, int k, int x)
	{
		return this.Binomial(m, x) * this.Binomial(N - m, k - x) / this.Binomial(N, k);
	}

	// Token: 0x0600032B RID: 811 RVA: 0x0000FB6C File Offset: 0x0000DD6C
	private double Binomial(int n, int k)
	{
		if (k < 0 || k > n)
		{
			return 0.0;
		}
		if (k == 0 || k == n)
		{
			return 1.0;
		}
		double num = 1.0;
		for (int i = 1; i <= k; i++)
		{
			num *= (double)(n - (k - i)) / (double)i;
		}
		return num;
	}

	// Token: 0x0600032C RID: 812 RVA: 0x0000FBC0 File Offset: 0x0000DDC0
	public bool SelectButton(KenoButton button)
	{
		if (this._selectedButtons.Contains(button))
		{
			this._selectedButtons.Remove(button);
			return false;
		}
		if (this._selectedButtons.Count >= this.maxSelectionCount)
		{
			return false;
		}
		this._selectedButtons.Add(button);
		return true;
	}

	// Token: 0x0600032D RID: 813 RVA: 0x0000FC0C File Offset: 0x0000DE0C
	[ClientRpc]
	private void RpcDiamondRevealFeedback(bool isHit, Vector3 position, int idx)
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		writer.WriteBool(isHit);
		writer.WriteVector3(position);
		writer.WriteVarInt(idx);
		this.SendRPCInternal("System.Void Keno::RpcDiamondRevealFeedback(System.Boolean,UnityEngine.Vector3,System.Int32)", -94606144, writer, 0, true);
		NetworkWriterPool.Return(writer);
	}

	// Token: 0x0600032E RID: 814 RVA: 0x0000FC5C File Offset: 0x0000DE5C
	[ClientRpc]
	private void RpcWarningFeedback()
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		this.SendRPCInternal("System.Void Keno::RpcWarningFeedback()", 189043358, writer, 0, true);
		NetworkWriterPool.Return(writer);
	}

	// Token: 0x0600032F RID: 815 RVA: 0x0000FC8C File Offset: 0x0000DE8C
	public Keno()
	{
		this._Mirror_SyncVarHookDelegate__currentMultiplier = new Action<double, double>(this.OnMultiplierChanged);
	}

	// Token: 0x06000330 RID: 816 RVA: 0x00002321 File Offset: 0x00000521
	public override bool Weaved()
	{
		return true;
	}

	// Token: 0x17000057 RID: 87
	// (get) Token: 0x06000331 RID: 817 RVA: 0x0000FCE4 File Offset: 0x0000DEE4
	// (set) Token: 0x06000332 RID: 818 RVA: 0x0000FCF7 File Offset: 0x0000DEF7
	public double Network_currentMultiplier
	{
		get
		{
			return this._currentMultiplier;
		}
		[param: In]
		set
		{
			base.GeneratedSyncVarSetter<double>(value, ref this._currentMultiplier, 8UL, this._Mirror_SyncVarHookDelegate__currentMultiplier);
		}
	}

	// Token: 0x06000333 RID: 819 RVA: 0x0000FD18 File Offset: 0x0000DF18
	protected void UserCode_RpcDiamondRevealFeedback__Boolean__Vector3__Int32(bool isHit, Vector3 position, int idx)
	{
		float num = (float)idx / (float)this.Buttons.Length * 3f;
		if (isHit)
		{
			SFXManager.SFXOneShotWithParameters(this.sfxOnHit, null, position, 0.7f + num);
			return;
		}
		SFXManager.SFXOneShotWithParameters(this.sfxOnMiss, null, position, 0.7f + num);
	}

	// Token: 0x06000334 RID: 820 RVA: 0x0000FD64 File Offset: 0x0000DF64
	protected static void InvokeUserCode_RpcDiamondRevealFeedback__Boolean__Vector3__Int32(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcDiamondRevealFeedback called on server.");
			return;
		}
		((Keno)obj).UserCode_RpcDiamondRevealFeedback__Boolean__Vector3__Int32(reader.ReadBool(), reader.ReadVector3(), reader.ReadVarInt());
	}

	// Token: 0x06000335 RID: 821 RVA: 0x0000FD99 File Offset: 0x0000DF99
	protected void UserCode_RpcWarningFeedback()
	{
		SFXManager.SFXOneShot(this.sfxWarning, base.transform.position);
	}

	// Token: 0x06000336 RID: 822 RVA: 0x0000FDB1 File Offset: 0x0000DFB1
	protected static void InvokeUserCode_RpcWarningFeedback(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcWarningFeedback called on server.");
			return;
		}
		((Keno)obj).UserCode_RpcWarningFeedback();
	}

	// Token: 0x06000337 RID: 823 RVA: 0x0000FDD4 File Offset: 0x0000DFD4
	static Keno()
	{
		RemoteProcedureCalls.RegisterRpc(typeof(Keno), "System.Void Keno::RpcDiamondRevealFeedback(System.Boolean,UnityEngine.Vector3,System.Int32)", new RemoteCallDelegate(Keno.InvokeUserCode_RpcDiamondRevealFeedback__Boolean__Vector3__Int32));
		RemoteProcedureCalls.RegisterRpc(typeof(Keno), "System.Void Keno::RpcWarningFeedback()", new RemoteCallDelegate(Keno.InvokeUserCode_RpcWarningFeedback));
	}

	// Token: 0x06000338 RID: 824 RVA: 0x0000FE24 File Offset: 0x0000E024
	public override void SerializeSyncVars(NetworkWriter writer, bool forceAll)
	{
		base.SerializeSyncVars(writer, forceAll);
		if (forceAll)
		{
			writer.WriteDouble(this._currentMultiplier);
			return;
		}
		writer.WriteVarULong(this.syncVarDirtyBits);
		if ((this.syncVarDirtyBits & 8UL) != 0UL)
		{
			writer.WriteDouble(this._currentMultiplier);
		}
	}

	// Token: 0x06000339 RID: 825 RVA: 0x0000FE7C File Offset: 0x0000E07C
	public override void DeserializeSyncVars(NetworkReader reader, bool initialState)
	{
		base.DeserializeSyncVars(reader, initialState);
		if (initialState)
		{
			base.GeneratedSyncVarDeserialize<double>(ref this._currentMultiplier, this._Mirror_SyncVarHookDelegate__currentMultiplier, reader.ReadDouble());
			return;
		}
		long num = (long)reader.ReadVarULong();
		if ((num & 8L) != 0L)
		{
			base.GeneratedSyncVarDeserialize<double>(ref this._currentMultiplier, this._Mirror_SyncVarHookDelegate__currentMultiplier, reader.ReadDouble());
		}
	}

	// Token: 0x04000254 RID: 596
	[Header("Game Settings")]
	[SerializeField]
	private float revealDelay = 0.1f;

	// Token: 0x04000255 RID: 597
	[SerializeField]
	private int diamondCount = 10;

	// Token: 0x04000256 RID: 598
	[SerializeField]
	private int maxSelectionCount = 10;

	// Token: 0x04000257 RID: 599
	[SerializeField]
	private float riskRewardRatio = 2f;

	// Token: 0x04000258 RID: 600
	[Header("References")]
	[SerializeField]
	private TextMeshPro multiplierText;

	// Token: 0x04000259 RID: 601
	[SerializeField]
	private TextMeshPro potentialWinningText;

	// Token: 0x0400025A RID: 602
	[SerializeField]
	private Transform buttonsParent;

	// Token: 0x0400025B RID: 603
	private KenoButton[] _buttons;

	// Token: 0x0400025C RID: 604
	[Header("SFX")]
	[SerializeField]
	private EventReference sfxWarning;

	// Token: 0x0400025D RID: 605
	[SerializeField]
	private EventReference sfxOnHit;

	// Token: 0x0400025E RID: 606
	[SerializeField]
	private EventReference sfxOnMiss;

	// Token: 0x0400025F RID: 607
	[SyncVar(hook = "OnMultiplierChanged")]
	private double _currentMultiplier;

	// Token: 0x04000260 RID: 608
	private string _playerName;

	// Token: 0x04000261 RID: 609
	private List<KenoButton> _selectedButtons = new List<KenoButton>();

	// Token: 0x04000262 RID: 610
	public Action<double, double> _Mirror_SyncVarHookDelegate__currentMultiplier;
}
