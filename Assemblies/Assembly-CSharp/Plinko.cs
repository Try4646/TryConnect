using System;
using System.Collections.Generic;
using Mirror;
using Mirror.RemoteCalls;
using UnityEngine;

// Token: 0x0200006A RID: 106
public class Plinko : GameBase
{
	// Token: 0x060003B0 RID: 944 RVA: 0x00011562 File Offset: 0x0000F762
	protected override void OnAwake()
	{
		base.OnAwake();
		this._rng = base.GetSeededRandom(0);
	}

	// Token: 0x060003B1 RID: 945 RVA: 0x00011577 File Offset: 0x0000F777
	protected override bool CanGameStart()
	{
		if (Time.time - this._lastSpawnTime < this.cooldown)
		{
			return false;
		}
		this._lastSpawnTime = Time.time;
		return true;
	}

	// Token: 0x060003B2 RID: 946 RVA: 0x0001159B File Offset: 0x0000F79B
	[Server]
	protected override void StartGame()
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void Plinko::StartGame()' called when server was not active");
			return;
		}
		base.StartGame();
		this.DropPuck();
	}

	// Token: 0x060003B3 RID: 947 RVA: 0x000115C0 File Offset: 0x0000F7C0
	[Server]
	private void DropPuck()
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void Plinko::DropPuck()' called when server was not active");
			return;
		}
		Vector3 position = this.spawnPosition.position;
		float num = (this._rng.NextDouble() < 0.5) ? (-0.1f + (float)this._rng.NextDouble() * 0.095f) : (0.005f + (float)this._rng.NextDouble() * 0.095f);
		position.x += num;
		position.z += -0.1f + (float)this._rng.NextDouble() * 0.2f;
		PlinkoPuck plinkoPuck = Object.Instantiate<PlinkoPuck>(this.puckPrefab, position, this.spawnPosition.rotation);
		plinkoPuck.Initialize(this.currentBet);
		NetworkServer.Spawn(plinkoPuck.gameObject, null);
		this.ResetGame();
	}

	// Token: 0x060003B4 RID: 948 RVA: 0x0001169C File Offset: 0x0000F89C
	[Server]
	public void OnPuckEnteredPocket(int slotIndex, PlinkoPuck puck)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void Plinko::OnPuckEnteredPocket(System.Int32,PlinkoPuck)' called when server was not active");
			return;
		}
		double num = 0.0;
		if (this.slotMultipliers != null && slotIndex >= 0 && slotIndex < this.slotMultipliers.Length)
		{
			num = this.slotMultipliers[slotIndex];
		}
		this.Payout(num * base.EstimatedValue, ChangeType.GameResult, null, puck.betAmount);
	}

	// Token: 0x060003B5 RID: 949 RVA: 0x00011700 File Offset: 0x0000F900
	public void ServerPlayPillarFeedbacks(PlinkoPillar pillar)
	{
		if (!base.isServer)
		{
			return;
		}
		int index = this.pillars.IndexOf(pillar);
		this.RpcPlayPillarFeedbacks(index);
	}

	// Token: 0x060003B6 RID: 950 RVA: 0x0001172C File Offset: 0x0000F92C
	[ClientRpc]
	private void RpcPlayPillarFeedbacks(int index)
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		writer.WriteVarInt(index);
		this.SendRPCInternal("System.Void Plinko::RpcPlayPillarFeedbacks(System.Int32)", 1965725964, writer, 0, true);
		NetworkWriterPool.Return(writer);
	}

	// Token: 0x060003B8 RID: 952 RVA: 0x00002321 File Offset: 0x00000521
	public override bool Weaved()
	{
		return true;
	}

	// Token: 0x060003B9 RID: 953 RVA: 0x00011786 File Offset: 0x0000F986
	protected void UserCode_RpcPlayPillarFeedbacks__Int32(int index)
	{
		this.pillars[index].PlayFeedbacks();
	}

	// Token: 0x060003BA RID: 954 RVA: 0x00011799 File Offset: 0x0000F999
	protected static void InvokeUserCode_RpcPlayPillarFeedbacks__Int32(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcPlayPillarFeedbacks called on server.");
			return;
		}
		((Plinko)obj).UserCode_RpcPlayPillarFeedbacks__Int32(reader.ReadVarInt());
	}

	// Token: 0x060003BB RID: 955 RVA: 0x000117C2 File Offset: 0x0000F9C2
	static Plinko()
	{
		RemoteProcedureCalls.RegisterRpc(typeof(Plinko), "System.Void Plinko::RpcPlayPillarFeedbacks(System.Int32)", new RemoteCallDelegate(Plinko.InvokeUserCode_RpcPlayPillarFeedbacks__Int32));
	}

	// Token: 0x040002A4 RID: 676
	[Header("References")]
	[SerializeField]
	private PlinkoPuck puckPrefab;

	// Token: 0x040002A5 RID: 677
	[SerializeField]
	private Transform spawnPosition;

	// Token: 0x040002A6 RID: 678
	[SerializeField]
	private List<PlinkoPillar> pillars;

	// Token: 0x040002A7 RID: 679
	[Header("Settings")]
	[SerializeField]
	private double[] slotMultipliers = new double[]
	{
		0.2,
		0.5,
		1.0,
		2.0,
		5.0,
		2.0,
		1.0,
		0.5,
		0.2
	};

	// Token: 0x040002A8 RID: 680
	[SerializeField]
	private float cooldown;

	// Token: 0x040002A9 RID: 681
	private float _lastSpawnTime;

	// Token: 0x040002AA RID: 682
	private Random _rng;
}
