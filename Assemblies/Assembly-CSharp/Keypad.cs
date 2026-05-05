using System;
using System.Runtime.InteropServices;
using Extensions;
using Mirror;
using Mirror.RemoteCalls;
using MoreMountains.Feedbacks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Token: 0x02000096 RID: 150
public class Keypad : NetworkBehaviour
{
	// Token: 0x06000556 RID: 1366 RVA: 0x00017CDE File Offset: 0x00015EDE
	public override void OnStartClient()
	{
		this.UpdateDisplay();
	}

	// Token: 0x06000557 RID: 1367 RVA: 0x00017CE6 File Offset: 0x00015EE6
	private void OnEnable()
	{
		this.palette = Resources.Load<UIColorPalette>("ColorSettings");
		MoneyManager instance = NetworkSingleton<MoneyManager>.Instance;
		instance.OnBalanceChanged = (Action<BalanceChangeData>)Delegate.Combine(instance.OnBalanceChanged, new Action<BalanceChangeData>(this.OnBalanceChanged));
	}

	// Token: 0x06000558 RID: 1368 RVA: 0x00017D1E File Offset: 0x00015F1E
	private void OnDisable()
	{
		MoneyManager instance = NetworkSingleton<MoneyManager>.Instance;
		instance.OnBalanceChanged = (Action<BalanceChangeData>)Delegate.Remove(instance.OnBalanceChanged, new Action<BalanceChangeData>(this.OnBalanceChanged));
	}

	// Token: 0x06000559 RID: 1369 RVA: 0x00017D48 File Offset: 0x00015F48
	private void Start()
	{
		if (this.keypadCamera && this.keypadRenderTarget)
		{
			RenderTexture renderTexture = new RenderTexture(450, 600, 24);
			this.keypadCamera.targetTexture = renderTexture;
			this.keypadRenderTarget.texture = renderTexture;
			this.keypadCamera.enabled = false;
		}
		if (this.NetworkcasinoGame)
		{
			base.transform.SetParent(this.NetworkcasinoGame.keypadSpawnPoint, false);
			this.UpdateMinMaxBetText(this.NetworkcasinoGame.MinBet, this.NetworkcasinoGame.MaxBet);
		}
		if (this.displayText)
		{
			this.displayText.text = "$0";
		}
		if (this.errorText)
		{
			this.errorText.text = "";
		}
		this.RequestRender();
	}

	// Token: 0x0600055A RID: 1370 RVA: 0x00017E25 File Offset: 0x00016025
	private void LateUpdate()
	{
		if (!base.isClient || !this.keypadCamera || !this._needsRender)
		{
			return;
		}
		this._needsRender = false;
		this.keypadCamera.Render();
	}

	// Token: 0x0600055B RID: 1371 RVA: 0x00017E57 File Offset: 0x00016057
	public void SetCasinoGame(GameBase game)
	{
		this.NetworkcasinoGame = game;
	}

	// Token: 0x0600055C RID: 1372 RVA: 0x00017E60 File Offset: 0x00016060
	private void OnCasinoGameSet(GameBase oldGame, GameBase newGame)
	{
		if (newGame)
		{
			base.transform.SetParent(newGame.keypadSpawnPoint, false);
			this.UpdateMinMaxBetText(this.NetworkcasinoGame.MinBet, this.NetworkcasinoGame.MaxBet);
		}
	}

	// Token: 0x0600055D RID: 1373 RVA: 0x00017CDE File Offset: 0x00015EDE
	private void OnInputValueChanged(string oldValue, string newValue)
	{
		this.UpdateDisplay();
	}

	// Token: 0x0600055E RID: 1374 RVA: 0x00017E98 File Offset: 0x00016098
	private void OnErrorTextChanged(string oldValue, string newValue)
	{
		this.errorText.text = newValue;
		this.RequestRender();
	}

	// Token: 0x0600055F RID: 1375 RVA: 0x00017EAC File Offset: 0x000160AC
	private void OnBalanceChanged(BalanceChangeData changeData)
	{
		if (!base.isServer)
		{
			return;
		}
		if (this.isGoldenChipApplied)
		{
			return;
		}
		this.ApplyInput(this._currentInput, true);
	}

	// Token: 0x06000560 RID: 1376 RVA: 0x00017ED0 File Offset: 0x000160D0
	[Server]
	public void AppendDigit(string digit)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void Keypad::AppendDigit(System.String)' called when server was not active");
			return;
		}
		if (this.isGoldenChipApplied)
		{
			return;
		}
		long maxBet = this.NetworkcasinoGame.MaxBet;
		this.Network_errorMessage = "";
		int num = int.Parse(digit);
		if (this._currentInput == "0" && num == 0)
		{
			return;
		}
		string text;
		if (this._currentInput == "0" && num > 0)
		{
			text = digit;
		}
		else
		{
			text = this._currentInput + digit;
		}
		long num2;
		if (long.TryParse(text, out num2) && num2 > maxBet)
		{
			this.Network_errorMessage = "Max Bet: " + MoneyFormatter.FormatWithDollar(maxBet);
			return;
		}
		this.ApplyInput(text, true);
	}

	// Token: 0x06000561 RID: 1377 RVA: 0x00017F84 File Offset: 0x00016184
	[Server]
	public void SetBetMinimum()
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void Keypad::SetBetMinimum()' called when server was not active");
			return;
		}
		if (this.isGoldenChipApplied)
		{
			return;
		}
		this.ApplyInput(this.NetworkcasinoGame.MinBet.ToString(), true);
	}

	// Token: 0x06000562 RID: 1378 RVA: 0x00017FCC File Offset: 0x000161CC
	[Server]
	public void SetPercentageBet(float percentage)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void Keypad::SetPercentageBet(System.Single)' called when server was not active");
			return;
		}
		if (this.isGoldenChipApplied)
		{
			return;
		}
		string input = this.GetPercentageBet((double)percentage, true).ToString();
		this.ApplyInput(input, true);
	}

	// Token: 0x06000563 RID: 1379 RVA: 0x00018011 File Offset: 0x00016211
	[Server]
	public void ClearInput()
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void Keypad::ClearInput()' called when server was not active");
			return;
		}
		if (this.isGoldenChipApplied)
		{
			return;
		}
		this.ApplyInput("0", true);
	}

	// Token: 0x06000564 RID: 1380 RVA: 0x00018040 File Offset: 0x00016240
	[Server]
	public void SetGoldenChip(bool apply, float multiplier = 1f)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void Keypad::SetGoldenChip(System.Boolean,System.Single)' called when server was not active");
			return;
		}
		this.isGoldenChipApplied = apply;
		this.goldenChipSFX.RpcLoopSFX(apply);
		if (apply)
		{
			string input = this.GetPercentageBet((double)multiplier, false).ToString();
			this.ApplyInput(input, false);
			this.Network_errorMessage = "Golden Chip!";
			this.RpcSetDisplayColor(this.palette.ticketYellow);
			this.RpcSetGoldenChipParticles(true);
			return;
		}
		this.ClearInput();
		this.RpcSetGoldenChipParticles(false);
	}

	// Token: 0x06000565 RID: 1381 RVA: 0x000180C4 File Offset: 0x000162C4
	[Server]
	private void ApplyInput(string input, bool validate)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void Keypad::ApplyInput(System.String,System.Boolean)' called when server was not active");
			return;
		}
		this.Network_currentInput = input;
		long num = this.NetworkcasinoGame.MinBet;
		long maxBet = this.NetworkcasinoGame.MaxBet;
		if (this.NetworkcasinoGame is Roulette)
		{
			num = 1L;
		}
		long num2;
		if (!long.TryParse(this._currentInput, out num2))
		{
			this.Network_errorMessage = "Invalid amount";
			return;
		}
		if (!validate)
		{
			this.Network_errorMessage = "";
			this.RpcSetDisplayColor(this.palette.profitGreen);
			this.NetworkcasinoGame.ServerSetBet(num2);
			return;
		}
		if (num2 < num)
		{
			this.Network_errorMessage = "Min: " + MoneyFormatter.FormatWithDollar(num);
			this.RpcSetDisplayColor(this.palette.lossRed);
		}
		else if (num2 > maxBet)
		{
			this.Network_errorMessage = "Max: " + MoneyFormatter.FormatWithDollar(maxBet);
			this.RpcSetDisplayColor(this.palette.lossRed);
		}
		else if (num2 > NetworkSingleton<MoneyManager>.Instance.balance)
		{
			this.Network_errorMessage = "Not Enough Money";
			this.RpcSetDisplayColor(this.palette.lossRed);
		}
		else
		{
			this.Network_errorMessage = "";
			this.RpcSetDisplayColor(this.palette.profitGreen);
		}
		this.NetworkcasinoGame.ServerSetBet(num2);
	}

	// Token: 0x06000566 RID: 1382 RVA: 0x0001820C File Offset: 0x0001640C
	[Server]
	private long GetPercentageBet(double percentage, bool limitToBalance = true)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Int64 Keypad::GetPercentageBet(System.Double,System.Boolean)' called when server was not active");
			return 0L;
		}
		long balance = NetworkSingleton<MoneyManager>.Instance.balance;
		long num = this.NetworkcasinoGame.MaxBet;
		if (limitToBalance)
		{
			num = Math.Min(this.NetworkcasinoGame.MaxBet, balance);
		}
		return (long)Math.Round((double)num * percentage);
	}

	// Token: 0x06000567 RID: 1383 RVA: 0x00018270 File Offset: 0x00016470
	public long GetCurrentInput()
	{
		long result;
		if (!long.TryParse(this._currentInput, out result))
		{
			return 0L;
		}
		return result;
	}

	// Token: 0x06000568 RID: 1384 RVA: 0x00018290 File Offset: 0x00016490
	[Server]
	public void ServerUpdateMinMaxBetText(long minBet, long maxBet)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void Keypad::ServerUpdateMinMaxBetText(System.Int64,System.Int64)' called when server was not active");
			return;
		}
		this.RpcUpdateMinMaxBetText(minBet, maxBet);
	}

	// Token: 0x06000569 RID: 1385 RVA: 0x000182B0 File Offset: 0x000164B0
	[ClientRpc]
	private void RpcUpdateMinMaxBetText(long minBet, long maxBet)
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		writer.WriteVarLong(minBet);
		writer.WriteVarLong(maxBet);
		this.SendRPCInternal("System.Void Keypad::RpcUpdateMinMaxBetText(System.Int64,System.Int64)", -784711597, writer, 0, true);
		NetworkWriterPool.Return(writer);
	}

	// Token: 0x0600056A RID: 1386 RVA: 0x000182F4 File Offset: 0x000164F4
	private void UpdateMinMaxBetText(long minBet, long maxBet)
	{
		this.minMaxBetText.text = "Min: " + MoneyFormatter.FormatWithDollar(minBet) + " \nMax: " + MoneyFormatter.FormatWithDollar(maxBet);
	}

	// Token: 0x0600056B RID: 1387 RVA: 0x0001831C File Offset: 0x0001651C
	[ClientRpc]
	private void RpcSetDisplayColor(Color color)
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		writer.WriteColor(color);
		this.SendRPCInternal("System.Void Keypad::RpcSetDisplayColor(UnityEngine.Color)", 827791925, writer, 0, true);
		NetworkWriterPool.Return(writer);
	}

	// Token: 0x0600056C RID: 1388 RVA: 0x00018358 File Offset: 0x00016558
	[ClientRpc]
	private void RpcSetGoldenChipParticles(bool isEnabled)
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		writer.WriteBool(isEnabled);
		this.SendRPCInternal("System.Void Keypad::RpcSetGoldenChipParticles(System.Boolean)", -154841549, writer, 0, true);
		NetworkWriterPool.Return(writer);
	}

	// Token: 0x0600056D RID: 1389 RVA: 0x00018394 File Offset: 0x00016594
	[ClientRpc]
	public void RpcUpdateDisplay()
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		this.SendRPCInternal("System.Void Keypad::RpcUpdateDisplay()", 1846554495, writer, 0, true);
		NetworkWriterPool.Return(writer);
	}

	// Token: 0x0600056E RID: 1390 RVA: 0x000183C4 File Offset: 0x000165C4
	private void UpdateDisplay()
	{
		long num;
		if (string.IsNullOrEmpty(this._currentInput))
		{
			this.displayText.text = "$0";
		}
		else if (long.TryParse(this._currentInput, out num))
		{
			this.displayText.text = "$" + num.ToString("N0");
		}
		else
		{
			this.displayText.text = "$" + this._currentInput;
		}
		this.RequestRender();
	}

	// Token: 0x0600056F RID: 1391 RVA: 0x00018443 File Offset: 0x00016643
	[Server]
	public void ServerInvalidBetAmountFb()
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void Keypad::ServerInvalidBetAmountFb()' called when server was not active");
			return;
		}
		this.Network_errorMessage = "Invalid bet amount";
		this.RpcPlayInvalidBetAmountFb();
	}

	// Token: 0x06000570 RID: 1392 RVA: 0x0001846C File Offset: 0x0001666C
	[ClientRpc]
	private void RpcPlayInvalidBetAmountFb()
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		this.SendRPCInternal("System.Void Keypad::RpcPlayInvalidBetAmountFb()", -526309948, writer, 0, true);
		NetworkWriterPool.Return(writer);
	}

	// Token: 0x06000571 RID: 1393 RVA: 0x0001849C File Offset: 0x0001669C
	private void RequestRender()
	{
		if (!base.isClient || !this.keypadCamera)
		{
			return;
		}
		this._needsRender = true;
	}

	// Token: 0x06000572 RID: 1394 RVA: 0x000184BC File Offset: 0x000166BC
	public Keypad()
	{
		this._Mirror_SyncVarHookDelegate_casinoGame = new Action<GameBase, GameBase>(this.OnCasinoGameSet);
		this._Mirror_SyncVarHookDelegate__currentInput = new Action<string, string>(this.OnInputValueChanged);
		this._Mirror_SyncVarHookDelegate__errorMessage = new Action<string, string>(this.OnErrorTextChanged);
	}

	// Token: 0x06000573 RID: 1395 RVA: 0x00002321 File Offset: 0x00000521
	public override bool Weaved()
	{
		return true;
	}

	// Token: 0x17000089 RID: 137
	// (get) Token: 0x06000574 RID: 1396 RVA: 0x0001851C File Offset: 0x0001671C
	// (set) Token: 0x06000575 RID: 1397 RVA: 0x0001853B File Offset: 0x0001673B
	public GameBase NetworkcasinoGame
	{
		get
		{
			return base.GetSyncVarNetworkBehaviour<GameBase>(this.___casinoGameNetId, ref this.casinoGame);
		}
		[param: In]
		set
		{
			base.GeneratedSyncVarSetter_NetworkBehaviour<GameBase>(value, ref this.casinoGame, 1UL, this._Mirror_SyncVarHookDelegate_casinoGame, ref this.___casinoGameNetId);
		}
	}

	// Token: 0x1700008A RID: 138
	// (get) Token: 0x06000576 RID: 1398 RVA: 0x00018560 File Offset: 0x00016760
	// (set) Token: 0x06000577 RID: 1399 RVA: 0x00018573 File Offset: 0x00016773
	public string Network_currentInput
	{
		get
		{
			return this._currentInput;
		}
		[param: In]
		set
		{
			base.GeneratedSyncVarSetter<string>(value, ref this._currentInput, 2UL, this._Mirror_SyncVarHookDelegate__currentInput);
		}
	}

	// Token: 0x1700008B RID: 139
	// (get) Token: 0x06000578 RID: 1400 RVA: 0x00018594 File Offset: 0x00016794
	// (set) Token: 0x06000579 RID: 1401 RVA: 0x000185A7 File Offset: 0x000167A7
	public string Network_errorMessage
	{
		get
		{
			return this._errorMessage;
		}
		[param: In]
		set
		{
			base.GeneratedSyncVarSetter<string>(value, ref this._errorMessage, 4UL, this._Mirror_SyncVarHookDelegate__errorMessage);
		}
	}

	// Token: 0x0600057A RID: 1402 RVA: 0x000185C6 File Offset: 0x000167C6
	protected void UserCode_RpcUpdateMinMaxBetText__Int64__Int64(long minBet, long maxBet)
	{
		this.UpdateMinMaxBetText(minBet, maxBet);
		this.RequestRender();
	}

	// Token: 0x0600057B RID: 1403 RVA: 0x000185D6 File Offset: 0x000167D6
	protected static void InvokeUserCode_RpcUpdateMinMaxBetText__Int64__Int64(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcUpdateMinMaxBetText called on server.");
			return;
		}
		((Keypad)obj).UserCode_RpcUpdateMinMaxBetText__Int64__Int64(reader.ReadVarLong(), reader.ReadVarLong());
	}

	// Token: 0x0600057C RID: 1404 RVA: 0x00018605 File Offset: 0x00016805
	protected void UserCode_RpcSetDisplayColor__Color(Color color)
	{
		this.displayText.color = color;
		this.RequestRender();
	}

	// Token: 0x0600057D RID: 1405 RVA: 0x00018619 File Offset: 0x00016819
	protected static void InvokeUserCode_RpcSetDisplayColor__Color(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcSetDisplayColor called on server.");
			return;
		}
		((Keypad)obj).UserCode_RpcSetDisplayColor__Color(reader.ReadColor());
	}

	// Token: 0x0600057E RID: 1406 RVA: 0x00018642 File Offset: 0x00016842
	protected void UserCode_RpcSetGoldenChipParticles__Boolean(bool isEnabled)
	{
		if (isEnabled)
		{
			this.goldenChipParticles.Play();
			return;
		}
		this.goldenChipParticles.Stop();
	}

	// Token: 0x0600057F RID: 1407 RVA: 0x0001865E File Offset: 0x0001685E
	protected static void InvokeUserCode_RpcSetGoldenChipParticles__Boolean(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcSetGoldenChipParticles called on server.");
			return;
		}
		((Keypad)obj).UserCode_RpcSetGoldenChipParticles__Boolean(reader.ReadBool());
	}

	// Token: 0x06000580 RID: 1408 RVA: 0x00017CDE File Offset: 0x00015EDE
	protected void UserCode_RpcUpdateDisplay()
	{
		this.UpdateDisplay();
	}

	// Token: 0x06000581 RID: 1409 RVA: 0x00018687 File Offset: 0x00016887
	protected static void InvokeUserCode_RpcUpdateDisplay(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcUpdateDisplay called on server.");
			return;
		}
		((Keypad)obj).UserCode_RpcUpdateDisplay();
	}

	// Token: 0x06000582 RID: 1410 RVA: 0x000186AA File Offset: 0x000168AA
	protected void UserCode_RpcPlayInvalidBetAmountFb()
	{
		this.invalidBetAmountFb.PlayFeedbacks();
		this.RequestRender();
	}

	// Token: 0x06000583 RID: 1411 RVA: 0x000186BD File Offset: 0x000168BD
	protected static void InvokeUserCode_RpcPlayInvalidBetAmountFb(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcPlayInvalidBetAmountFb called on server.");
			return;
		}
		((Keypad)obj).UserCode_RpcPlayInvalidBetAmountFb();
	}

	// Token: 0x06000584 RID: 1412 RVA: 0x000186E0 File Offset: 0x000168E0
	static Keypad()
	{
		RemoteProcedureCalls.RegisterRpc(typeof(Keypad), "System.Void Keypad::RpcUpdateMinMaxBetText(System.Int64,System.Int64)", new RemoteCallDelegate(Keypad.InvokeUserCode_RpcUpdateMinMaxBetText__Int64__Int64));
		RemoteProcedureCalls.RegisterRpc(typeof(Keypad), "System.Void Keypad::RpcSetDisplayColor(UnityEngine.Color)", new RemoteCallDelegate(Keypad.InvokeUserCode_RpcSetDisplayColor__Color));
		RemoteProcedureCalls.RegisterRpc(typeof(Keypad), "System.Void Keypad::RpcSetGoldenChipParticles(System.Boolean)", new RemoteCallDelegate(Keypad.InvokeUserCode_RpcSetGoldenChipParticles__Boolean));
		RemoteProcedureCalls.RegisterRpc(typeof(Keypad), "System.Void Keypad::RpcUpdateDisplay()", new RemoteCallDelegate(Keypad.InvokeUserCode_RpcUpdateDisplay));
		RemoteProcedureCalls.RegisterRpc(typeof(Keypad), "System.Void Keypad::RpcPlayInvalidBetAmountFb()", new RemoteCallDelegate(Keypad.InvokeUserCode_RpcPlayInvalidBetAmountFb));
	}

	// Token: 0x06000585 RID: 1413 RVA: 0x00018790 File Offset: 0x00016990
	public override void SerializeSyncVars(NetworkWriter writer, bool forceAll)
	{
		base.SerializeSyncVars(writer, forceAll);
		if (forceAll)
		{
			writer.WriteNetworkBehaviour(this.NetworkcasinoGame);
			writer.WriteString(this._currentInput);
			writer.WriteString(this._errorMessage);
			return;
		}
		writer.WriteVarULong(this.syncVarDirtyBits);
		if ((this.syncVarDirtyBits & 1UL) != 0UL)
		{
			writer.WriteNetworkBehaviour(this.NetworkcasinoGame);
		}
		if ((this.syncVarDirtyBits & 2UL) != 0UL)
		{
			writer.WriteString(this._currentInput);
		}
		if ((this.syncVarDirtyBits & 4UL) != 0UL)
		{
			writer.WriteString(this._errorMessage);
		}
	}

	// Token: 0x06000586 RID: 1414 RVA: 0x00018844 File Offset: 0x00016A44
	public override void DeserializeSyncVars(NetworkReader reader, bool initialState)
	{
		base.DeserializeSyncVars(reader, initialState);
		if (initialState)
		{
			base.GeneratedSyncVarDeserialize_NetworkBehaviour<GameBase>(ref this.casinoGame, this._Mirror_SyncVarHookDelegate_casinoGame, reader, ref this.___casinoGameNetId);
			base.GeneratedSyncVarDeserialize<string>(ref this._currentInput, this._Mirror_SyncVarHookDelegate__currentInput, reader.ReadString());
			base.GeneratedSyncVarDeserialize<string>(ref this._errorMessage, this._Mirror_SyncVarHookDelegate__errorMessage, reader.ReadString());
			return;
		}
		long num = (long)reader.ReadVarULong();
		if ((num & 1L) != 0L)
		{
			base.GeneratedSyncVarDeserialize_NetworkBehaviour<GameBase>(ref this.casinoGame, this._Mirror_SyncVarHookDelegate_casinoGame, reader, ref this.___casinoGameNetId);
		}
		if ((num & 2L) != 0L)
		{
			base.GeneratedSyncVarDeserialize<string>(ref this._currentInput, this._Mirror_SyncVarHookDelegate__currentInput, reader.ReadString());
		}
		if ((num & 4L) != 0L)
		{
			base.GeneratedSyncVarDeserialize<string>(ref this._errorMessage, this._Mirror_SyncVarHookDelegate__errorMessage, reader.ReadString());
		}
	}

	// Token: 0x040003C7 RID: 967
	[SerializeField]
	private UIColorPalette palette;

	// Token: 0x040003C8 RID: 968
	[Header("References")]
	[SerializeField]
	private TextMeshPro displayText;

	// Token: 0x040003C9 RID: 969
	[SerializeField]
	private TextMeshPro errorText;

	// Token: 0x040003CA RID: 970
	[SerializeField]
	private TextMeshPro minMaxBetText;

	// Token: 0x040003CB RID: 971
	[SerializeField]
	private ParticleSystem goldenChipParticles;

	// Token: 0x040003CC RID: 972
	[SerializeField]
	private MMF_Player invalidBetAmountFb;

	// Token: 0x040003CD RID: 973
	[SerializeField]
	private RawImage keypadRenderTarget;

	// Token: 0x040003CE RID: 974
	public Camera keypadCamera;

	// Token: 0x040003CF RID: 975
	private bool _needsRender;

	// Token: 0x040003D0 RID: 976
	[SyncVar(hook = "OnCasinoGameSet")]
	public GameBase casinoGame;

	// Token: 0x040003D1 RID: 977
	[SyncVar(hook = "OnInputValueChanged")]
	private string _currentInput = "";

	// Token: 0x040003D2 RID: 978
	[SyncVar(hook = "OnErrorTextChanged")]
	private string _errorMessage = "";

	// Token: 0x040003D3 RID: 979
	[Header("SFX")]
	[SerializeField]
	private SFXLoopComponent goldenChipSFX;

	// Token: 0x040003D4 RID: 980
	public bool isGoldenChipApplied;

	// Token: 0x040003D5 RID: 981
	protected NetworkBehaviourSyncVar ___casinoGameNetId;

	// Token: 0x040003D6 RID: 982
	public Action<GameBase, GameBase> _Mirror_SyncVarHookDelegate_casinoGame;

	// Token: 0x040003D7 RID: 983
	public Action<string, string> _Mirror_SyncVarHookDelegate__currentInput;

	// Token: 0x040003D8 RID: 984
	public Action<string, string> _Mirror_SyncVarHookDelegate__errorMessage;
}
