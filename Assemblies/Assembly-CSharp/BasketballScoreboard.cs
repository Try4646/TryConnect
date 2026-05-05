using System;
using System.Runtime.InteropServices;
using Mirror;
using MoreMountains.Feedbacks;
using TMPro;
using UnityEngine;

// Token: 0x020000C4 RID: 196
public class BasketballScoreboard : NetworkBehaviour
{
	// Token: 0x0600074E RID: 1870 RVA: 0x0001E903 File Offset: 0x0001CB03
	public override void OnStartServer()
	{
		base.OnStartServer();
		this.basketball = Object.Instantiate<Basketball>(this.basketballPrefab, this.initialSpawnPos.position, this.initialSpawnPos.rotation);
		NetworkServer.Spawn(this.basketball.gameObject, null);
	}

	// Token: 0x0600074F RID: 1871 RVA: 0x0001E943 File Offset: 0x0001CB43
	private void OnScoreAChanged(int oldScore, int newScore)
	{
		this.scoreTextA.text = newScore.ToString();
	}

	// Token: 0x06000750 RID: 1872 RVA: 0x0001E957 File Offset: 0x0001CB57
	private void OnScoreBChanged(int oldScore, int newScore)
	{
		this.scoreTextB.text = newScore.ToString();
	}

	// Token: 0x06000751 RID: 1873 RVA: 0x0001E96C File Offset: 0x0001CB6C
	[Server]
	public void RetrieveBasketball()
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void BasketballScoreboard::RetrieveBasketball()' called when server was not active");
			return;
		}
		this.basketball.DestroyItem();
		this.basketball = Object.Instantiate<Basketball>(this.basketballPrefab, this.spawnPos.position, this.spawnPos.rotation);
		NetworkServer.Spawn(this.basketball.gameObject, null);
	}

	// Token: 0x06000752 RID: 1874 RVA: 0x0001E9D4 File Offset: 0x0001CBD4
	[Server]
	public void IncreaseScore(bool isTeamA)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void BasketballScoreboard::IncreaseScore(System.Boolean)' called when server was not active");
			return;
		}
		if (isTeamA)
		{
			this.Network_scoreA = this._scoreA + 1;
			this.teamAScoreFb.PlayFeedbacks();
			return;
		}
		this.Network_scoreB = this._scoreB + 1;
		this.teamBScoreFb.PlayFeedbacks();
	}

	// Token: 0x06000753 RID: 1875 RVA: 0x0001EA2C File Offset: 0x0001CC2C
	[Server]
	public void ResetScores()
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void BasketballScoreboard::ResetScores()' called when server was not active");
			return;
		}
		this.Network_scoreA = 0;
		this.Network_scoreB = 0;
	}

	// Token: 0x06000754 RID: 1876 RVA: 0x0001EA51 File Offset: 0x0001CC51
	public BasketballScoreboard()
	{
		this._Mirror_SyncVarHookDelegate__scoreA = new Action<int, int>(this.OnScoreAChanged);
		this._Mirror_SyncVarHookDelegate__scoreB = new Action<int, int>(this.OnScoreBChanged);
	}

	// Token: 0x06000755 RID: 1877 RVA: 0x00002321 File Offset: 0x00000521
	public override bool Weaved()
	{
		return true;
	}

	// Token: 0x170000A1 RID: 161
	// (get) Token: 0x06000756 RID: 1878 RVA: 0x0001EA80 File Offset: 0x0001CC80
	// (set) Token: 0x06000757 RID: 1879 RVA: 0x0001EA93 File Offset: 0x0001CC93
	public int Network_scoreA
	{
		get
		{
			return this._scoreA;
		}
		[param: In]
		set
		{
			base.GeneratedSyncVarSetter<int>(value, ref this._scoreA, 1UL, this._Mirror_SyncVarHookDelegate__scoreA);
		}
	}

	// Token: 0x170000A2 RID: 162
	// (get) Token: 0x06000758 RID: 1880 RVA: 0x0001EAB4 File Offset: 0x0001CCB4
	// (set) Token: 0x06000759 RID: 1881 RVA: 0x0001EAC7 File Offset: 0x0001CCC7
	public int Network_scoreB
	{
		get
		{
			return this._scoreB;
		}
		[param: In]
		set
		{
			base.GeneratedSyncVarSetter<int>(value, ref this._scoreB, 2UL, this._Mirror_SyncVarHookDelegate__scoreB);
		}
	}

	// Token: 0x0600075A RID: 1882 RVA: 0x0001EAE8 File Offset: 0x0001CCE8
	public override void SerializeSyncVars(NetworkWriter writer, bool forceAll)
	{
		base.SerializeSyncVars(writer, forceAll);
		if (forceAll)
		{
			writer.WriteVarInt(this._scoreA);
			writer.WriteVarInt(this._scoreB);
			return;
		}
		writer.WriteVarULong(this.syncVarDirtyBits);
		if ((this.syncVarDirtyBits & 1UL) != 0UL)
		{
			writer.WriteVarInt(this._scoreA);
		}
		if ((this.syncVarDirtyBits & 2UL) != 0UL)
		{
			writer.WriteVarInt(this._scoreB);
		}
	}

	// Token: 0x0600075B RID: 1883 RVA: 0x0001EB70 File Offset: 0x0001CD70
	public override void DeserializeSyncVars(NetworkReader reader, bool initialState)
	{
		base.DeserializeSyncVars(reader, initialState);
		if (initialState)
		{
			base.GeneratedSyncVarDeserialize<int>(ref this._scoreA, this._Mirror_SyncVarHookDelegate__scoreA, reader.ReadVarInt());
			base.GeneratedSyncVarDeserialize<int>(ref this._scoreB, this._Mirror_SyncVarHookDelegate__scoreB, reader.ReadVarInt());
			return;
		}
		long num = (long)reader.ReadVarULong();
		if ((num & 1L) != 0L)
		{
			base.GeneratedSyncVarDeserialize<int>(ref this._scoreA, this._Mirror_SyncVarHookDelegate__scoreA, reader.ReadVarInt());
		}
		if ((num & 2L) != 0L)
		{
			base.GeneratedSyncVarDeserialize<int>(ref this._scoreB, this._Mirror_SyncVarHookDelegate__scoreB, reader.ReadVarInt());
		}
	}

	// Token: 0x040004E4 RID: 1252
	[Header("References")]
	[SerializeField]
	private TextMeshPro scoreTextA;

	// Token: 0x040004E5 RID: 1253
	[SerializeField]
	private TextMeshPro scoreTextB;

	// Token: 0x040004E6 RID: 1254
	[SerializeField]
	private MMF_Player teamAScoreFb;

	// Token: 0x040004E7 RID: 1255
	[SerializeField]
	private MMF_Player teamBScoreFb;

	// Token: 0x040004E8 RID: 1256
	[Header("Ball")]
	[SerializeField]
	private Basketball basketballPrefab;

	// Token: 0x040004E9 RID: 1257
	[SerializeField]
	private Basketball basketball;

	// Token: 0x040004EA RID: 1258
	[SerializeField]
	private Transform spawnPos;

	// Token: 0x040004EB RID: 1259
	[SerializeField]
	private Transform initialSpawnPos;

	// Token: 0x040004EC RID: 1260
	[SyncVar(hook = "OnScoreAChanged")]
	private int _scoreA;

	// Token: 0x040004ED RID: 1261
	[SyncVar(hook = "OnScoreBChanged")]
	private int _scoreB;

	// Token: 0x040004EE RID: 1262
	public Action<int, int> _Mirror_SyncVarHookDelegate__scoreA;

	// Token: 0x040004EF RID: 1263
	public Action<int, int> _Mirror_SyncVarHookDelegate__scoreB;
}
