using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using FMODUnity;
using Mirror;
using Mirror.RemoteCalls;
using TMPro;
using UnityEngine;

// Token: 0x02000064 RID: 100
public class Minesweeper : GameBase
{
	// Token: 0x0600035F RID: 863 RVA: 0x000104F1 File Offset: 0x0000E6F1
	private void OnMinesCountChanged(int oldValue, int newValue)
	{
		this.minesCountText.text = newValue.ToString() + " Mines";
	}

	// Token: 0x06000360 RID: 864 RVA: 0x00010510 File Offset: 0x0000E710
	[ClientRpc]
	private void RpcSetMultiplierText(double multiplier)
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		writer.WriteDouble(multiplier);
		this.SendRPCInternal("System.Void Minesweeper::RpcSetMultiplierText(System.Double)", 291016282, writer, 0, true);
		NetworkWriterPool.Return(writer);
	}

	// Token: 0x06000361 RID: 865 RVA: 0x0001054C File Offset: 0x0000E74C
	[ClientRpc]
	private void RpcSetPotentialWinningText(long winning)
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		writer.WriteVarLong(winning);
		this.SendRPCInternal("System.Void Minesweeper::RpcSetPotentialWinningText(System.Int64)", -530662363, writer, 0, true);
		NetworkWriterPool.Return(writer);
	}

	// Token: 0x06000362 RID: 866 RVA: 0x00010586 File Offset: 0x0000E786
	[Server]
	public void IncreaseMines()
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void Minesweeper::IncreaseMines()' called when server was not active");
			return;
		}
		if (this.isPlaying)
		{
			return;
		}
		this.Network_currentMineCount = Mathf.Clamp(this._currentMineCount + 1, this.minMines, this.maxMines);
	}

	// Token: 0x06000363 RID: 867 RVA: 0x000105C5 File Offset: 0x0000E7C5
	[Server]
	public void DecreaseMines()
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void Minesweeper::DecreaseMines()' called when server was not active");
			return;
		}
		if (this.isPlaying)
		{
			return;
		}
		this.Network_currentMineCount = Mathf.Clamp(this._currentMineCount - 1, this.minMines, this.maxMines);
	}

	// Token: 0x06000364 RID: 868 RVA: 0x00010604 File Offset: 0x0000E804
	[Server]
	protected override void StartGame()
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void Minesweeper::StartGame()' called when server was not active");
			return;
		}
		base.StartGame();
		this.SetMines();
		foreach (MinesweeperTile minesweeperTile in this.tiles)
		{
			minesweeperTile.ServerSetButtonColor(1);
		}
	}

	// Token: 0x06000365 RID: 869 RVA: 0x00010678 File Offset: 0x0000E878
	[Server]
	private void SetMines()
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void Minesweeper::SetMines()' called when server was not active");
			return;
		}
		int count = this.tiles.Count;
		this.Network_currentMineCount = Mathf.Clamp(this._currentMineCount, 1, count - 1);
		Random seededRandom = base.GetSeededRandom(0);
		int[] array = new int[count];
		for (int i = 0; i < count; i++)
		{
			array[i] = i;
		}
		this._mineIndexes.Clear();
		for (int j = 0; j < this._currentMineCount; j++)
		{
			int num = seededRandom.Next(j, count);
			ref int ptr = ref array[j];
			int[] array2 = array;
			int num2 = num;
			int num3 = array[num];
			int num4 = array[j];
			ptr = num3;
			array2[num2] = num4;
			this._mineIndexes.Add(array[j]);
		}
	}

	// Token: 0x06000366 RID: 870 RVA: 0x0001073C File Offset: 0x0000E93C
	[Server]
	public void RevealTile(MinesweeperTile tile)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void Minesweeper::RevealTile(MinesweeperTile)' called when server was not active");
			return;
		}
		if (!this.isPlaying || this._hasEnded)
		{
			return;
		}
		int item = this.tiles.IndexOf(tile);
		if (!this._revealedTiles.Add(item))
		{
			return;
		}
		if (this._mineIndexes.Contains(item))
		{
			tile.ServerSetMine(true);
			tile.ServerSetButtonColor(0);
			tile.ServerExplode();
			this.Lose();
			return;
		}
		tile.ServerSetButtonColor(2);
		double num = this.CalculateCurrentMultiplier();
		long winning = (long)Math.Round((double)this.currentBet * num);
		this.RpcPlayRevealGreenSFX(num, tile.transform.position);
		this.RpcSetMultiplierText(num);
		this.RpcSetPotentialWinningText(winning);
		if (this._revealedTiles.Count >= this.tiles.Count - this._currentMineCount)
		{
			this.CashOut();
		}
	}

	// Token: 0x06000367 RID: 871 RVA: 0x00010817 File Offset: 0x0000EA17
	[Server]
	private void Lose()
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void Minesweeper::Lose()' called when server was not active");
			return;
		}
		this.RevealAllMines();
		this.Payout(0.0, ChangeType.GameResult, null, -1L);
		base.StartCoroutine(this.ResetAfterDelay());
	}

	// Token: 0x06000368 RID: 872 RVA: 0x00010854 File Offset: 0x0000EA54
	[Server]
	public void CashOut()
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void Minesweeper::CashOut()' called when server was not active");
			return;
		}
		if (!this.isPlaying || this._hasEnded)
		{
			return;
		}
		double multiplier = this.CalculateCurrentMultiplier();
		this.Payout(multiplier, ChangeType.GameResult, null, -1L);
		this.RevealAllMines();
		base.StartCoroutine(this.ResetAfterDelay());
	}

	// Token: 0x06000369 RID: 873 RVA: 0x000108AC File Offset: 0x0000EAAC
	[Server]
	private IEnumerator ResetAfterDelay()
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Collections.IEnumerator Minesweeper::ResetAfterDelay()' called when server was not active");
			return null;
		}
		Minesweeper.<ResetAfterDelay>d__21 <ResetAfterDelay>d__ = new Minesweeper.<ResetAfterDelay>d__21(0);
		<ResetAfterDelay>d__.<>4__this = this;
		return <ResetAfterDelay>d__;
	}

	// Token: 0x0600036A RID: 874 RVA: 0x000108E8 File Offset: 0x0000EAE8
	[Server]
	protected override void ResetGame()
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void Minesweeper::ResetGame()' called when server was not active");
			return;
		}
		this._revealedTiles.Clear();
		this._mineIndexes.Clear();
		foreach (MinesweeperTile minesweeperTile in this.tiles)
		{
			minesweeperTile.ServerSetButtonColor(0);
			minesweeperTile.ServerSetMine(false);
		}
		this.RpcSetMultiplierText(1.0);
		this.RpcSetPotentialWinningText(0L);
		this._hasEnded = false;
		base.ResetGame();
	}

	// Token: 0x0600036B RID: 875 RVA: 0x00010990 File Offset: 0x0000EB90
	private void RevealAllMines()
	{
		for (int i = 0; i < this.tiles.Count; i++)
		{
			this.tiles[i].ServerSetMine(this._mineIndexes.Contains(i));
		}
	}

	// Token: 0x0600036C RID: 876 RVA: 0x000109D0 File Offset: 0x0000EBD0
	[ClientRpc]
	private void RpcPlayRevealGreenSFX(double currentMultiplier, Vector3 position)
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		writer.WriteDouble(currentMultiplier);
		writer.WriteVector3(position);
		this.SendRPCInternal("System.Void Minesweeper::RpcPlayRevealGreenSFX(System.Double,UnityEngine.Vector3)", -1358049268, writer, 0, true);
		NetworkWriterPool.Return(writer);
	}

	// Token: 0x0600036D RID: 877 RVA: 0x00010A14 File Offset: 0x0000EC14
	private double CalculateCurrentMultiplier()
	{
		if (this._revealedTiles.Count == 0)
		{
			return 1.0;
		}
		int count = this.tiles.Count;
		double num = 1.0;
		int num2 = count - this._currentMineCount;
		for (int i = 0; i < this._revealedTiles.Count; i++)
		{
			double num3 = (double)(num2 - i) / (double)(count - i);
			num *= 1.0 / num3;
		}
		return num * base.EstimatedValue;
	}

	// Token: 0x0600036E RID: 878 RVA: 0x00010A90 File Offset: 0x0000EC90
	public Minesweeper()
	{
		this._Mirror_SyncVarHookDelegate__currentMineCount = new Action<int, int>(this.OnMinesCountChanged);
	}

	// Token: 0x0600036F RID: 879 RVA: 0x00002321 File Offset: 0x00000521
	public override bool Weaved()
	{
		return true;
	}

	// Token: 0x1700005C RID: 92
	// (get) Token: 0x06000370 RID: 880 RVA: 0x00010AE4 File Offset: 0x0000ECE4
	// (set) Token: 0x06000371 RID: 881 RVA: 0x00010AF7 File Offset: 0x0000ECF7
	public int Network_currentMineCount
	{
		get
		{
			return this._currentMineCount;
		}
		[param: In]
		set
		{
			base.GeneratedSyncVarSetter<int>(value, ref this._currentMineCount, 8UL, this._Mirror_SyncVarHookDelegate__currentMineCount);
		}
	}

	// Token: 0x06000372 RID: 882 RVA: 0x00010B16 File Offset: 0x0000ED16
	protected void UserCode_RpcSetMultiplierText__Double(double multiplier)
	{
		this.multiplierText.text = multiplier.ToString("0.##") + "x";
	}

	// Token: 0x06000373 RID: 883 RVA: 0x00010B39 File Offset: 0x0000ED39
	protected static void InvokeUserCode_RpcSetMultiplierText__Double(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcSetMultiplierText called on server.");
			return;
		}
		((Minesweeper)obj).UserCode_RpcSetMultiplierText__Double(reader.ReadDouble());
	}

	// Token: 0x06000374 RID: 884 RVA: 0x00010B63 File Offset: 0x0000ED63
	protected void UserCode_RpcSetPotentialWinningText__Int64(long winning)
	{
		this.potentialWinningText.text = "$" + winning.ToString("0");
	}

	// Token: 0x06000375 RID: 885 RVA: 0x00010B86 File Offset: 0x0000ED86
	protected static void InvokeUserCode_RpcSetPotentialWinningText__Int64(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcSetPotentialWinningText called on server.");
			return;
		}
		((Minesweeper)obj).UserCode_RpcSetPotentialWinningText__Int64(reader.ReadVarLong());
	}

	// Token: 0x06000376 RID: 886 RVA: 0x00010BAF File Offset: 0x0000EDAF
	protected void UserCode_RpcPlayRevealGreenSFX__Double__Vector3(double currentMultiplier, Vector3 position)
	{
		SFXManager.SFXOneShotWithParameters(this.revealGreenTileSfx, null, position, 0.7f + (float)currentMultiplier * 0.1f);
	}

	// Token: 0x06000377 RID: 887 RVA: 0x00010BCC File Offset: 0x0000EDCC
	protected static void InvokeUserCode_RpcPlayRevealGreenSFX__Double__Vector3(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcPlayRevealGreenSFX called on server.");
			return;
		}
		((Minesweeper)obj).UserCode_RpcPlayRevealGreenSFX__Double__Vector3(reader.ReadDouble(), reader.ReadVector3());
	}

	// Token: 0x06000378 RID: 888 RVA: 0x00010BFC File Offset: 0x0000EDFC
	static Minesweeper()
	{
		RemoteProcedureCalls.RegisterRpc(typeof(Minesweeper), "System.Void Minesweeper::RpcSetMultiplierText(System.Double)", new RemoteCallDelegate(Minesweeper.InvokeUserCode_RpcSetMultiplierText__Double));
		RemoteProcedureCalls.RegisterRpc(typeof(Minesweeper), "System.Void Minesweeper::RpcSetPotentialWinningText(System.Int64)", new RemoteCallDelegate(Minesweeper.InvokeUserCode_RpcSetPotentialWinningText__Int64));
		RemoteProcedureCalls.RegisterRpc(typeof(Minesweeper), "System.Void Minesweeper::RpcPlayRevealGreenSFX(System.Double,UnityEngine.Vector3)", new RemoteCallDelegate(Minesweeper.InvokeUserCode_RpcPlayRevealGreenSFX__Double__Vector3));
	}

	// Token: 0x06000379 RID: 889 RVA: 0x00010C6C File Offset: 0x0000EE6C
	public override void SerializeSyncVars(NetworkWriter writer, bool forceAll)
	{
		base.SerializeSyncVars(writer, forceAll);
		if (forceAll)
		{
			writer.WriteVarInt(this._currentMineCount);
			return;
		}
		writer.WriteVarULong(this.syncVarDirtyBits);
		if ((this.syncVarDirtyBits & 8UL) != 0UL)
		{
			writer.WriteVarInt(this._currentMineCount);
		}
	}

	// Token: 0x0600037A RID: 890 RVA: 0x00010CC4 File Offset: 0x0000EEC4
	public override void DeserializeSyncVars(NetworkReader reader, bool initialState)
	{
		base.DeserializeSyncVars(reader, initialState);
		if (initialState)
		{
			base.GeneratedSyncVarDeserialize<int>(ref this._currentMineCount, this._Mirror_SyncVarHookDelegate__currentMineCount, reader.ReadVarInt());
			return;
		}
		long num = (long)reader.ReadVarULong();
		if ((num & 8L) != 0L)
		{
			base.GeneratedSyncVarDeserialize<int>(ref this._currentMineCount, this._Mirror_SyncVarHookDelegate__currentMineCount, reader.ReadVarInt());
		}
	}

	// Token: 0x0400027E RID: 638
	[Header("References")]
	[SerializeField]
	private TextMeshPro minesCountText;

	// Token: 0x0400027F RID: 639
	[SerializeField]
	private TextMeshPro multiplierText;

	// Token: 0x04000280 RID: 640
	[SerializeField]
	private TextMeshPro potentialWinningText;

	// Token: 0x04000281 RID: 641
	[SerializeField]
	private List<MinesweeperTile> tiles;

	// Token: 0x04000282 RID: 642
	[Header("Settings")]
	[SerializeField]
	private int minMines = 1;

	// Token: 0x04000283 RID: 643
	[SerializeField]
	private int maxMines = 24;

	// Token: 0x04000284 RID: 644
	[Header("SFX")]
	[SerializeField]
	private EventReference revealGreenTileSfx;

	// Token: 0x04000285 RID: 645
	[SyncVar(hook = "OnMinesCountChanged")]
	private int _currentMineCount = 3;

	// Token: 0x04000286 RID: 646
	private HashSet<int> _mineIndexes = new HashSet<int>();

	// Token: 0x04000287 RID: 647
	private HashSet<int> _revealedTiles = new HashSet<int>();

	// Token: 0x04000288 RID: 648
	private bool _hasEnded;

	// Token: 0x04000289 RID: 649
	public Action<int, int> _Mirror_SyncVarHookDelegate__currentMineCount;
}
