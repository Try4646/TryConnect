using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Extensions;
using Mirror;
using Mirror.RemoteCalls;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

// Token: 0x0200008D RID: 141
public class GameBase : NetworkBehaviour
{
	// Token: 0x17000078 RID: 120
	// (get) Token: 0x060004EB RID: 1259 RVA: 0x0001637E File Offset: 0x0001457E
	public string GameName
	{
		get
		{
			return this.gameName;
		}
	}

	// Token: 0x17000079 RID: 121
	// (get) Token: 0x060004EC RID: 1260 RVA: 0x00016386 File Offset: 0x00014586
	public CasinoGameType GameType
	{
		get
		{
			return this.gameType;
		}
	}

	// Token: 0x1700007A RID: 122
	// (get) Token: 0x060004ED RID: 1261 RVA: 0x0001638E File Offset: 0x0001458E
	// (set) Token: 0x060004EE RID: 1262 RVA: 0x00016396 File Offset: 0x00014596
	public long BaseMinBet
	{
		get
		{
			return this.baseMinBet;
		}
		set
		{
			this.baseMinBet = value;
			if (base.isServer && this.Keypad)
			{
				this.Keypad.ServerUpdateMinMaxBetText(this.MinBet, this.MaxBet);
			}
		}
	}

	// Token: 0x1700007B RID: 123
	// (get) Token: 0x060004EF RID: 1263 RVA: 0x000163CB File Offset: 0x000145CB
	// (set) Token: 0x060004F0 RID: 1264 RVA: 0x000163D3 File Offset: 0x000145D3
	public long BaseMaxBet
	{
		get
		{
			return this.baseMaxBet;
		}
		set
		{
			this.baseMaxBet = value;
			if (base.isServer && this.Keypad)
			{
				this.Keypad.ServerUpdateMinMaxBetText(this.MinBet, this.MaxBet);
			}
		}
	}

	// Token: 0x1700007C RID: 124
	// (get) Token: 0x060004F1 RID: 1265 RVA: 0x00016408 File Offset: 0x00014608
	protected double EstimatedValue
	{
		get
		{
			return (double)((this._gs && this._gs.floorData != null && this.casinoLevel >= 0 && this.casinoLevel < this._gs.floorData.Count && this._gs.floorData[this.casinoLevel] != null) ? (this.estimatedValue * this._gs.floorData[this.casinoLevel].estimatedValueMultiplier) : this.estimatedValue);
		}
	}

	// Token: 0x1700007D RID: 125
	// (get) Token: 0x060004F2 RID: 1266 RVA: 0x00016494 File Offset: 0x00014694
	public long MinBet
	{
		get
		{
			if (!this._gs || !NetworkSingleton<GameManager>.Instance)
			{
				return this.baseMinBet;
			}
			return Math.Max(1L, (long)Math.Round(FathF.RoundByFirstNDigits((double)this.baseMinBet * (double)NetworkSingleton<GameManager>.Instance.currentQuota * 0.001 * Math.Pow(2.0, (double)(this.casinoLevel - NetworkSingleton<GameManager>.Instance.currentFloor - 1)), 2), MidpointRounding.AwayFromZero));
		}
	}

	// Token: 0x1700007E RID: 126
	// (get) Token: 0x060004F3 RID: 1267 RVA: 0x00016518 File Offset: 0x00014718
	public long MaxBet
	{
		get
		{
			if (!this._gs || !NetworkSingleton<GameManager>.Instance)
			{
				return this.baseMaxBet;
			}
			return Math.Max(5L, (long)Math.Round(FathF.RoundByFirstNDigits((double)this.baseMaxBet * (double)NetworkSingleton<GameManager>.Instance.currentQuota * 0.001 * Math.Pow(2.0, (double)(this.casinoLevel - NetworkSingleton<GameManager>.Instance.currentFloor - 1)) * this.MaxBetOverrideMultiplier, 2), MidpointRounding.AwayFromZero));
		}
	}

	// Token: 0x1700007F RID: 127
	// (get) Token: 0x060004F4 RID: 1268 RVA: 0x000165A0 File Offset: 0x000147A0
	public Keypad Keypad
	{
		get
		{
			return this.keypad;
		}
	}

	// Token: 0x17000080 RID: 128
	// (get) Token: 0x060004F5 RID: 1269 RVA: 0x000165A8 File Offset: 0x000147A8
	// (set) Token: 0x060004F6 RID: 1270 RVA: 0x000165B0 File Offset: 0x000147B0
	public double MaxBetOverrideMultiplier
	{
		get
		{
			return this.maxBetOverrideMultiplier;
		}
		set
		{
			this.maxBetOverrideMultiplier = value;
			if (this.Keypad)
			{
				this.Keypad.ServerUpdateMinMaxBetText(this.MinBet, this.MaxBet);
			}
		}
	}

	// Token: 0x17000081 RID: 129
	// (get) Token: 0x060004F7 RID: 1271 RVA: 0x000165DD File Offset: 0x000147DD
	protected bool IsCasinoHelperEnabled
	{
		get
		{
			return this.casinoHelper != null && this.casinoHelper.value;
		}
	}

	// Token: 0x060004F8 RID: 1272 RVA: 0x000165FA File Offset: 0x000147FA
	protected void SetCasinoHelperText(int index, string text)
	{
		if (this.casinoHelperTexts == null || index < 0 || index >= this.casinoHelperTexts.Length)
		{
			return;
		}
		if (this.casinoHelperTexts[index] == null)
		{
			return;
		}
		this.casinoHelperTexts[index].text = text;
	}

	// Token: 0x060004F9 RID: 1273 RVA: 0x00016634 File Offset: 0x00014834
	protected void ClearCasinoHelperTexts()
	{
		if (this.casinoHelperTexts == null)
		{
			return;
		}
		for (int i = 0; i < this.casinoHelperTexts.Length; i++)
		{
			if (this.casinoHelperTexts[i] != null)
			{
				this.casinoHelperTexts[i].text = "";
			}
		}
	}

	// Token: 0x060004FA RID: 1274 RVA: 0x0001667F File Offset: 0x0001487F
	private void Awake()
	{
		this._gs = Resources.Load<GameSettings>("GameSettings");
		this.NetworkcurrentBet = 0L;
		this.isPlaying = false;
		this.canBet = true;
		this.OnAwake();
	}

	// Token: 0x060004FB RID: 1275 RVA: 0x000048A7 File Offset: 0x00002AA7
	protected virtual void OnAwake()
	{
	}

	// Token: 0x060004FC RID: 1276 RVA: 0x000166AD File Offset: 0x000148AD
	protected virtual void OnDisable()
	{
		if (this.isPlaying)
		{
			NetworkSingleton<MoneyManager>.Instance.TryChangeBalance(this.currentBet, null, ChangeType.Bet);
			this.ResetGame();
		}
	}

	// Token: 0x060004FD RID: 1277 RVA: 0x000166D0 File Offset: 0x000148D0
	public override void OnStartServer()
	{
		base.OnStartServer();
		this.keypad = Object.Instantiate<Keypad>(Resources.Load<Keypad>("Keypad"), this.keypadSpawnPoint.position, this.keypadSpawnPoint.rotation, this.keypadSpawnPoint);
		this.keypad.SetCasinoGame(this);
		NetworkServer.Spawn(this.keypad.gameObject, null);
		if (this.gameFeedbacks)
		{
			this.gameFeedbacks = base.GetComponent<CasinoGameFeedbacks>();
		}
	}

	// Token: 0x060004FE RID: 1278 RVA: 0x0001674A File Offset: 0x0001494A
	public override void OnStartClient()
	{
		base.OnStartClient();
		if (this.keypad)
		{
			this.keypad.transform.SetParent(this.keypadSpawnPoint);
		}
	}

	// Token: 0x060004FF RID: 1279 RVA: 0x00016775 File Offset: 0x00014975
	[Server]
	public void ServerSetBet(long betAmount)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void GameBase::ServerSetBet(System.Int64)' called when server was not active");
			return;
		}
		if (!this.canBet)
		{
			return;
		}
		this.NetworkcurrentBet = betAmount;
		this.keypad.RpcUpdateDisplay();
		this.OnBetSet();
	}

	// Token: 0x06000500 RID: 1280 RVA: 0x000048A7 File Offset: 0x00002AA7
	protected virtual void OnBetSet()
	{
	}

	// Token: 0x06000501 RID: 1281 RVA: 0x000167AD File Offset: 0x000149AD
	[Server]
	public void ApplyGoldenChip(float multiplier)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void GameBase::ApplyGoldenChip(System.Single)' called when server was not active");
			return;
		}
		this.SetGoldenChip(true, multiplier);
	}

	// Token: 0x06000502 RID: 1282 RVA: 0x000167CC File Offset: 0x000149CC
	protected virtual void SetGoldenChip(bool apply, float multiplier = 1f)
	{
		if (this.isGoldenChipApplied == apply)
		{
			return;
		}
		this.NetworkisGoldenChipApplied = apply;
		if (!apply)
		{
			this.isGoldenBet = false;
		}
		this.keypad.SetGoldenChip(apply, multiplier);
	}

	// Token: 0x06000503 RID: 1283 RVA: 0x000167F8 File Offset: 0x000149F8
	[Server]
	public virtual void TryStartGame(PlayerInteract playerInteract)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void GameBase::TryStartGame(PlayerInteract)' called when server was not active");
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
		PlayerProfile playerProfile;
		if (playerInteract.TryGetComponent<PlayerProfile>(out playerProfile))
		{
			this.interactingPlayer = playerProfile;
		}
		if (!this.isGoldenChipApplied)
		{
			if (this.gameType != CasinoGameType.CoinFlip && (this.currentBet < this.MinBet || this.currentBet > this.MaxBet || !NetworkSingleton<MoneyManager>.Instance.TryChangeBalance(-this.currentBet, this.interactingPlayer, ChangeType.Bet)))
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

	// Token: 0x06000504 RID: 1284 RVA: 0x000168AC File Offset: 0x00014AAC
	[Server]
	protected virtual void StartGame()
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void GameBase::StartGame()' called when server was not active");
			return;
		}
	}

	// Token: 0x06000505 RID: 1285 RVA: 0x000168C4 File Offset: 0x00014AC4
	protected virtual void ResetGame()
	{
		this.isPlaying = false;
		this.canBet = true;
		if (this.isGoldenBet)
		{
			this.SetGoldenChip(false, 1f);
		}
		this.NetworkcurrentBet = this.keypad.GetCurrentInput();
		this.gameTurn++;
	}

	// Token: 0x06000506 RID: 1286 RVA: 0x00016914 File Offset: 0x00014B14
	protected Random GetSeededRandom(int additionalContext = 0)
	{
		if (NetworkSingleton<SeededRandomManager>.Instance == null || NetworkSingleton<GameManager>.Instance == null)
		{
			return new Random(Random.Range(int.MinValue, int.MaxValue));
		}
		long currentSeed = (long)NetworkSingleton<SeededRandomManager>.Instance.CurrentSeed;
		int daysPassed = NetworkSingleton<GameManager>.Instance.daysPassed;
		Vector3 position = base.transform.position;
		long num = ((currentSeed * (long)((ulong)-1640531535) + (long)daysPassed ^ (long)this.gameTurn * (long)((ulong)-2048144777)) * (long)((ulong)-1640531535) + (long)((int)(position.x * 1000f)) ^ (long)((int)(position.y * 1000f)) * (long)((ulong)-2048144777)) * (long)((ulong)-1640531535) + (long)((int)(position.z * 1000f)) ^ (long)additionalContext * (long)((ulong)-2048144777);
		long num2 = (num ^ num >> 32) * (long)((ulong)-2048144789);
		long num3 = (num2 ^ num2 >> 16) * (long)((ulong)-1028477379);
		return new Random((int)(num3 ^ num3 >> 13));
	}

	// Token: 0x06000507 RID: 1287 RVA: 0x000169FC File Offset: 0x00014BFC
	[Server]
	protected virtual void Payout(double multiplier, ChangeType changeType = ChangeType.GameResult, Dictionary<string, object> gameSpecificData = null, long bet = -1L)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void GameBase::Payout(System.Double,ChangeType,System.Collections.Generic.Dictionary`2<System.String,System.Object>,System.Int64)' called when server was not active");
			return;
		}
		PlayerBuff playerBuff;
		if (!this.interactingPlayer.TryGetComponent<PlayerBuff>(out playerBuff))
		{
			return;
		}
		if (bet < 0L)
		{
			bet = this.currentBet;
		}
		double num = (double)playerBuff.GetValue(PlayerBuffType.TipsyFortune);
		double num2 = (double)playerBuff.GetValue(PlayerBuffType.InspiringMelody);
		double num3 = (double)playerBuff.GetValue(PlayerBuffType.Immunity);
		long num4 = (long)Math.Round(multiplier * (double)bet);
		long num5 = num4 - bet;
		if (num5 > 0L)
		{
			float upgradeData = NetworkSingleton<UpgradeManager>.Instance.GetUpgradeData(this.interactingPlayer.steamId, PlayerUpgradeType.GamblersConfidence);
			num4 = bet + (long)Math.Round((double)num5 * num * (1.0 + num2) * (double)upgradeData);
			float upgradeData2 = NetworkSingleton<UpgradeManager>.Instance.GetUpgradeData(this.interactingPlayer.steamId, PlayerUpgradeType.BonusDraw);
			if (this.GetSeededRandom(0).NextDouble() <= (double)upgradeData2)
			{
				NetworkSingleton<MoneyManager>.Instance.TryChangeTicketBalance(1L);
			}
		}
		else if (num5 < 0L)
		{
			if (num3 > 0.0)
			{
				num4 = bet;
			}
			else
			{
				long num6 = -num5;
				long num7 = bet - num6;
				double num8 = (double)Mathf.Clamp01(NetworkSingleton<UpgradeManager>.Instance.GetUpgradeData(this.interactingPlayer.steamId, PlayerUpgradeType.Insurance) + playerBuff.GetValue(PlayerBuffType.InspiringMelody));
				num4 = num7 + (long)Math.Round((double)num6 * num8);
			}
		}
		else
		{
			num4 = bet;
		}
		if (num4 > 0L)
		{
			NetworkSingleton<MoneyManager>.Instance.TryChangeBalance(num4, this.interactingPlayer, changeType);
		}
		NetworkSingleton<GameResultsManager>.Instance.RegisterResult(bet, num4, this.interactingPlayer, this.gameType, base.transform.position, num > 1.0, num2 > 0.0, num3 > 0.0, gameSpecificData);
		if (NetworkSingleton<AnalyticsManager>.Instance && this.interactingPlayer && !Application.isEditor)
		{
			NetworkSingleton<AnalyticsManager>.Instance.SendAnalytics(this, this.interactingPlayer.playerName, bet, (long)Math.Round((double)bet * multiplier));
		}
		this.RpcEndGameFeedBacks(multiplier);
	}

	// Token: 0x06000508 RID: 1288 RVA: 0x00016BEC File Offset: 0x00014DEC
	[ClientRpc]
	private void RpcEndGameFeedBacks(double multiplier)
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		writer.WriteDouble(multiplier);
		this.SendRPCInternal("System.Void GameBase::RpcEndGameFeedBacks(System.Double)", 673425954, writer, 0, true);
		NetworkWriterPool.Return(writer);
	}

	// Token: 0x06000509 RID: 1289 RVA: 0x00002321 File Offset: 0x00000521
	protected virtual bool CanGameStart()
	{
		return true;
	}

	// Token: 0x0600050A RID: 1290 RVA: 0x00016C26 File Offset: 0x00014E26
	public void SetEstimatedValue(float value)
	{
		this.estimatedValue = value;
	}

	// Token: 0x0600050B RID: 1291 RVA: 0x00016C30 File Offset: 0x00014E30
	[Server]
	public void ServerSimulatePayout(PlayerProfile player, bool isWin)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void GameBase::ServerSimulatePayout(PlayerProfile,System.Boolean)' called when server was not active");
			return;
		}
		if (player == null)
		{
			Debug.LogWarning("[GameBase] Cannot simulate payout: PlayerProfile is null");
			return;
		}
		this.interactingPlayer = player;
		if (this.currentBet <= 0L)
		{
			this.NetworkcurrentBet = (long)Math.Round((double)(this.MinBet + this.MaxBet) / 2.0);
			if (this.currentBet < this.MinBet)
			{
				this.NetworkcurrentBet = this.MinBet;
			}
		}
		if (!NetworkSingleton<MoneyManager>.Instance.TryChangeBalance(-this.currentBet, player, ChangeType.Bet))
		{
			Debug.LogWarning(string.Format("[GameBase] Cannot simulate payout: Failed to deduct bet of {0}", this.currentBet));
			return;
		}
		this.Payout((double)(isWin ? 2 : 0), ChangeType.GameResult, null, -1L);
	}

	// Token: 0x0600050D RID: 1293 RVA: 0x00002321 File Offset: 0x00000521
	public override bool Weaved()
	{
		return true;
	}

	// Token: 0x17000082 RID: 130
	// (get) Token: 0x0600050E RID: 1294 RVA: 0x00016D48 File Offset: 0x00014F48
	// (set) Token: 0x0600050F RID: 1295 RVA: 0x00016D5B File Offset: 0x00014F5B
	public int NetworkcasinoLevel
	{
		get
		{
			return this.casinoLevel;
		}
		[param: In]
		set
		{
			base.GeneratedSyncVarSetter<int>(value, ref this.casinoLevel, 1UL, null);
		}
	}

	// Token: 0x17000083 RID: 131
	// (get) Token: 0x06000510 RID: 1296 RVA: 0x00016D78 File Offset: 0x00014F78
	// (set) Token: 0x06000511 RID: 1297 RVA: 0x00016D8B File Offset: 0x00014F8B
	public long NetworkcurrentBet
	{
		get
		{
			return this.currentBet;
		}
		[param: In]
		set
		{
			base.GeneratedSyncVarSetter<long>(value, ref this.currentBet, 2UL, null);
		}
	}

	// Token: 0x17000084 RID: 132
	// (get) Token: 0x06000512 RID: 1298 RVA: 0x00016DA8 File Offset: 0x00014FA8
	// (set) Token: 0x06000513 RID: 1299 RVA: 0x00016DBB File Offset: 0x00014FBB
	public bool NetworkisGoldenChipApplied
	{
		get
		{
			return this.isGoldenChipApplied;
		}
		[param: In]
		set
		{
			base.GeneratedSyncVarSetter<bool>(value, ref this.isGoldenChipApplied, 4UL, null);
		}
	}

	// Token: 0x06000514 RID: 1300 RVA: 0x00016DD8 File Offset: 0x00014FD8
	protected void UserCode_RpcEndGameFeedBacks__Double(double multiplier)
	{
		this.gameFeedbacks.PlayGameResultFeedback(multiplier);
		if (multiplier > 1.0)
		{
			UnityEvent unityEvent = this.onProfitEvent;
			if (unityEvent == null)
			{
				return;
			}
			unityEvent.Invoke();
			return;
		}
		else if (multiplier < 1.0)
		{
			UnityEvent unityEvent2 = this.onLossEvent;
			if (unityEvent2 == null)
			{
				return;
			}
			unityEvent2.Invoke();
			return;
		}
		else
		{
			UnityEvent unityEvent3 = this.onTieEvent;
			if (unityEvent3 == null)
			{
				return;
			}
			unityEvent3.Invoke();
			return;
		}
	}

	// Token: 0x06000515 RID: 1301 RVA: 0x00016E3B File Offset: 0x0001503B
	protected static void InvokeUserCode_RpcEndGameFeedBacks__Double(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcEndGameFeedBacks called on server.");
			return;
		}
		((GameBase)obj).UserCode_RpcEndGameFeedBacks__Double(reader.ReadDouble());
	}

	// Token: 0x06000516 RID: 1302 RVA: 0x00016E65 File Offset: 0x00015065
	static GameBase()
	{
		RemoteProcedureCalls.RegisterRpc(typeof(GameBase), "System.Void GameBase::RpcEndGameFeedBacks(System.Double)", new RemoteCallDelegate(GameBase.InvokeUserCode_RpcEndGameFeedBacks__Double));
	}

	// Token: 0x06000517 RID: 1303 RVA: 0x00016E88 File Offset: 0x00015088
	public override void SerializeSyncVars(NetworkWriter writer, bool forceAll)
	{
		base.SerializeSyncVars(writer, forceAll);
		if (forceAll)
		{
			writer.WriteVarInt(this.casinoLevel);
			writer.WriteVarLong(this.currentBet);
			writer.WriteBool(this.isGoldenChipApplied);
			return;
		}
		writer.WriteVarULong(this.syncVarDirtyBits);
		if ((this.syncVarDirtyBits & 1UL) != 0UL)
		{
			writer.WriteVarInt(this.casinoLevel);
		}
		if ((this.syncVarDirtyBits & 2UL) != 0UL)
		{
			writer.WriteVarLong(this.currentBet);
		}
		if ((this.syncVarDirtyBits & 4UL) != 0UL)
		{
			writer.WriteBool(this.isGoldenChipApplied);
		}
	}

	// Token: 0x06000518 RID: 1304 RVA: 0x00016F3C File Offset: 0x0001513C
	public override void DeserializeSyncVars(NetworkReader reader, bool initialState)
	{
		base.DeserializeSyncVars(reader, initialState);
		if (initialState)
		{
			base.GeneratedSyncVarDeserialize<int>(ref this.casinoLevel, null, reader.ReadVarInt());
			base.GeneratedSyncVarDeserialize<long>(ref this.currentBet, null, reader.ReadVarLong());
			base.GeneratedSyncVarDeserialize<bool>(ref this.isGoldenChipApplied, null, reader.ReadBool());
			return;
		}
		long num = (long)reader.ReadVarULong();
		if ((num & 1L) != 0L)
		{
			base.GeneratedSyncVarDeserialize<int>(ref this.casinoLevel, null, reader.ReadVarInt());
		}
		if ((num & 2L) != 0L)
		{
			base.GeneratedSyncVarDeserialize<long>(ref this.currentBet, null, reader.ReadVarLong());
		}
		if ((num & 4L) != 0L)
		{
			base.GeneratedSyncVarDeserialize<bool>(ref this.isGoldenChipApplied, null, reader.ReadBool());
		}
	}

	// Token: 0x0400037E RID: 894
	[Header("References")]
	public Transform keypadSpawnPoint;

	// Token: 0x0400037F RID: 895
	[SerializeField]
	protected CasinoGameFeedbacks gameFeedbacks;

	// Token: 0x04000380 RID: 896
	[SyncVar]
	public int casinoLevel;

	// Token: 0x04000381 RID: 897
	[SerializeField]
	protected UnityEvent onProfitEvent;

	// Token: 0x04000382 RID: 898
	[SerializeField]
	protected UnityEvent onTieEvent;

	// Token: 0x04000383 RID: 899
	[SerializeField]
	protected UnityEvent onLossEvent;

	// Token: 0x04000384 RID: 900
	[Header("Settings")]
	[SerializeField]
	protected string gameName = "";

	// Token: 0x04000385 RID: 901
	[SerializeField]
	protected CasinoGameType gameType;

	// Token: 0x04000386 RID: 902
	[SerializeField]
	private float estimatedValue = 1f;

	// Token: 0x04000387 RID: 903
	[SerializeField]
	private long baseMinBet = 1L;

	// Token: 0x04000388 RID: 904
	[SerializeField]
	private long baseMaxBet = 100L;

	// Token: 0x04000389 RID: 905
	[Header("Debug")]
	[SyncVar]
	[ReadOnly]
	public long currentBet;

	// Token: 0x0400038A RID: 906
	[SerializeField]
	[ReadOnly]
	protected bool canBet = true;

	// Token: 0x0400038B RID: 907
	[SerializeField]
	[ReadOnly]
	private double maxBetOverrideMultiplier = 1.0;

	// Token: 0x0400038C RID: 908
	[SerializeField]
	[ReadOnly]
	public bool isPlaying;

	// Token: 0x0400038D RID: 909
	[SerializeField]
	[ReadOnly]
	protected PlayerProfile interactingPlayer;

	// Token: 0x0400038E RID: 910
	[SerializeField]
	[ReadOnly]
	protected Keypad keypad;

	// Token: 0x0400038F RID: 911
	[SyncVar]
	[ReadOnly]
	public bool isGoldenChipApplied;

	// Token: 0x04000390 RID: 912
	[ReadOnly]
	public bool isGoldenBet;

	// Token: 0x04000391 RID: 913
	[SerializeField]
	protected int gameTurn;

	// Token: 0x04000392 RID: 914
	[Header("ACCESSIBILITY SETTINGS")]
	[SerializeField]
	private ToggleSettingItem casinoHelper;

	// Token: 0x04000393 RID: 915
	[SerializeField]
	private TextMeshPro[] casinoHelperTexts;

	// Token: 0x04000394 RID: 916
	private GameSettings _gs;
}
