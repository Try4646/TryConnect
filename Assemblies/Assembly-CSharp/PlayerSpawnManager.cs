using System;
using System.Collections;
using System.Collections.Generic;
using Extensions;
using Mirror;
using Mirror.RemoteCalls;
using UnityEngine;

// Token: 0x0200019F RID: 415
public class PlayerSpawnManager : NetworkSingleton<PlayerSpawnManager>
{
	// Token: 0x1400000E RID: 14
	// (add) Token: 0x06000F34 RID: 3892 RVA: 0x00040444 File Offset: 0x0003E644
	// (remove) Token: 0x06000F35 RID: 3893 RVA: 0x0004047C File Offset: 0x0003E67C
	public event Action OnAllPlayersSpawned;

	// Token: 0x1400000F RID: 15
	// (add) Token: 0x06000F36 RID: 3894 RVA: 0x000404B4 File Offset: 0x0003E6B4
	// (remove) Token: 0x06000F37 RID: 3895 RVA: 0x000404EC File Offset: 0x0003E6EC
	public event Action OnPlayerLateJoined;

	// Token: 0x1700014E RID: 334
	// (get) Token: 0x06000F38 RID: 3896 RVA: 0x00040521 File Offset: 0x0003E721
	public int RegisteredCount
	{
		get
		{
			return this.registered.Count;
		}
	}

	// Token: 0x06000F39 RID: 3897 RVA: 0x0004052E File Offset: 0x0003E72E
	protected override void OnAwake()
	{
		base.OnAwake();
		this.lobbySettings = Resources.Load<LobbySettings>("LobbySettings");
		if (this.emptyPlayerPrefab == null)
		{
			this.emptyPlayerPrefab = Resources.Load<GameObject>("EmptyPlayer");
		}
	}

	// Token: 0x06000F3A RID: 3898 RVA: 0x00040564 File Offset: 0x0003E764
	[Server]
	public void ServerOnSceneChanged(string sceneName)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void PlayerSpawnManager::ServerOnSceneChanged(System.String)' called when server was not active");
			return;
		}
		this.sceneEpoch++;
		this.sceneAcceptingSpawns = false;
		this.spawningEnabled = false;
		this.initialSpawnSequenceStarted = false;
		this.initializedEpoch = -1;
		this.ready.Clear();
		this.spawning.Clear();
		this.usedSpawnPoints.Clear();
		this.spawnIndexByConnId.Clear();
		foreach (KeyValuePair<int, NetworkConnectionToClient> keyValuePair in NetworkServer.connections)
		{
			NetworkConnectionToClient value = keyValuePair.Value;
			if (value != null)
			{
				int connectionId = value.connectionId;
				this.registered.Add(connectionId);
				if (value.identity != null)
				{
					NetworkServer.RemovePlayerForConnection(value, RemovePlayerOptions.Destroy);
				}
			}
		}
		this.sceneAcceptingSpawns = true;
		if (this.timeoutCoroutine != null)
		{
			base.StopCoroutine(this.timeoutCoroutine);
		}
		this.timeoutCoroutine = base.StartCoroutine(this.SpawnTimeoutRoutine());
		foreach (KeyValuePair<int, NetworkConnectionToClient> keyValuePair2 in NetworkServer.connections)
		{
			NetworkConnectionToClient value2 = keyValuePair2.Value;
			if (value2 != null && value2.isReady)
			{
				this.TrySpawnForConnection(value2);
			}
		}
	}

	// Token: 0x06000F3B RID: 3899 RVA: 0x000406D4 File Offset: 0x0003E8D4
	[Server]
	public void RegisterConnection(NetworkConnectionToClient conn)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void PlayerSpawnManager::RegisterConnection(Mirror.NetworkConnectionToClient)' called when server was not active");
			return;
		}
		if (conn == null)
		{
			return;
		}
		int connectionId = conn.connectionId;
		if (this.registered.Add(connectionId))
		{
			Debug.Log(string.Format("Registered connection {0}. Total registered: {1}", connectionId, this.registered.Count));
		}
		this.CheckSpawningReady();
	}

	// Token: 0x06000F3C RID: 3900 RVA: 0x0004073C File Offset: 0x0003E93C
	[Server]
	public void OnClientSceneReady(NetworkConnectionToClient conn)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void PlayerSpawnManager::OnClientSceneReady(Mirror.NetworkConnectionToClient)' called when server was not active");
			return;
		}
		if (conn == null)
		{
			return;
		}
		int connectionId = conn.connectionId;
		if (this.ready.Add(connectionId))
		{
			Debug.Log(string.Format("Connection {0} is ready. Total ready: {1}", connectionId, this.ready.Count));
		}
		this.CheckSpawningReady();
		if (!this.sceneAcceptingSpawns)
		{
			Debug.LogWarning(string.Format("Connection {0} ready but scene not accepting spawns yet", connectionId));
			return;
		}
		this.TrySpawnForConnection(conn);
	}

	// Token: 0x06000F3D RID: 3901 RVA: 0x000407C8 File Offset: 0x0003E9C8
	[Server]
	public void ServerOnDisconnected(NetworkConnectionToClient conn)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void PlayerSpawnManager::ServerOnDisconnected(Mirror.NetworkConnectionToClient)' called when server was not active");
			return;
		}
		if (conn == null)
		{
			return;
		}
		int connectionId = conn.connectionId;
		this.registered.Remove(connectionId);
		this.ready.Remove(connectionId);
		this.spawning.Remove(connectionId);
		int item;
		if (this.spawnIndexByConnId.TryGetValue(connectionId, out item))
		{
			this.usedSpawnPoints.Remove(item);
		}
		this.spawnIndexByConnId.Remove(connectionId);
	}

	// Token: 0x06000F3E RID: 3902 RVA: 0x00040848 File Offset: 0x0003EA48
	[Server]
	private void TryInitializeIfComplete()
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void PlayerSpawnManager::TryInitializeIfComplete()' called when server was not active");
			return;
		}
		if (!this.sceneAcceptingSpawns)
		{
			return;
		}
		int num = Mathf.Max(1, this.registered.Count);
		if (this.registered.Count < num)
		{
			return;
		}
		if (this.ready.Count < num)
		{
			return;
		}
		int num2 = 0;
		foreach (KeyValuePair<int, NetworkConnectionToClient> keyValuePair in NetworkServer.connections)
		{
			NetworkConnectionToClient value = keyValuePair.Value;
			if (value != null)
			{
				int connectionId = value.connectionId;
				if (this.registered.Contains(connectionId) && this.ready.Contains(connectionId) && value.identity != null)
				{
					num2++;
				}
			}
		}
		if (num2 < num)
		{
			return;
		}
		if (this.initializedEpoch == this.sceneEpoch)
		{
			return;
		}
		this.initializedEpoch = this.sceneEpoch;
		Action onAllPlayersSpawned = this.OnAllPlayersSpawned;
		if (onAllPlayersSpawned == null)
		{
			return;
		}
		onAllPlayersSpawned();
	}

	// Token: 0x06000F3F RID: 3903 RVA: 0x0004095C File Offset: 0x0003EB5C
	[Server]
	private void CheckSpawningReady()
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void PlayerSpawnManager::CheckSpawningReady()' called when server was not active");
			return;
		}
		if (this.spawningEnabled)
		{
			return;
		}
		if (!this.sceneAcceptingSpawns)
		{
			return;
		}
		int num = Mathf.Max(1, this.registered.Count);
		if (this.registered.Count < num)
		{
			return;
		}
		if (this.ready.Count < num)
		{
			return;
		}
		this.spawningEnabled = true;
		if (this.timeoutCoroutine != null)
		{
			base.StopCoroutine(this.timeoutCoroutine);
			this.timeoutCoroutine = null;
		}
		this.StartInitialSpawnSequence();
	}

	// Token: 0x06000F40 RID: 3904 RVA: 0x000409E8 File Offset: 0x0003EBE8
	[Server]
	private void TrySpawnForConnection(NetworkConnectionToClient conn)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void PlayerSpawnManager::TrySpawnForConnection(Mirror.NetworkConnectionToClient)' called when server was not active");
			return;
		}
		if (conn == null)
		{
			return;
		}
		if (!conn.isReady)
		{
			return;
		}
		if (!this.spawningEnabled)
		{
			Debug.Log(string.Format("TrySpawnForConnection: Spawning not enabled yet for connection {0}", conn.connectionId));
			return;
		}
		if (!this.initialSpawnSequenceStarted)
		{
			Debug.Log(string.Format("TrySpawnForConnection: Initial spawn sequence not started for connection {0}", conn.connectionId));
			return;
		}
		int connectionId = conn.connectionId;
		if (!this.registered.Contains(connectionId))
		{
			Debug.LogWarning(string.Format("TrySpawnForConnection: Connection {0} not registered", connectionId));
			return;
		}
		if (!this.ready.Contains(connectionId))
		{
			Debug.LogWarning(string.Format("TrySpawnForConnection: Connection {0} not ready", connectionId));
			return;
		}
		if (this.spawning.Contains(connectionId))
		{
			Debug.LogWarning(string.Format("TrySpawnForConnection: Connection {0} already spawning", connectionId));
			return;
		}
		if (conn.identity != null)
		{
			Debug.LogWarning(string.Format("TrySpawnForConnection: Connection {0} already has identity", connectionId));
			return;
		}
		List<Transform> startPositions = NetworkManager.startPositions;
		if (startPositions == null || startPositions.Count == 0)
		{
			Debug.LogError(string.Format("TrySpawnForConnection: No spawn points available! Connection {0} cannot spawn.", connectionId));
			return;
		}
		Debug.Log(string.Format("Attempting to spawn player for connection {0}", connectionId));
		this.spawning.Add(connectionId);
		base.StartCoroutine(this.SpawnRoutine(conn, startPositions));
	}

	// Token: 0x06000F41 RID: 3905 RVA: 0x00040B4C File Offset: 0x0003ED4C
	[Server]
	private IEnumerator SpawnRoutine(NetworkConnectionToClient conn, List<Transform> starts)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Collections.IEnumerator PlayerSpawnManager::SpawnRoutine(Mirror.NetworkConnectionToClient,System.Collections.Generic.List`1<UnityEngine.Transform>)' called when server was not active");
			return null;
		}
		PlayerSpawnManager.<SpawnRoutine>d__30 <SpawnRoutine>d__ = new PlayerSpawnManager.<SpawnRoutine>d__30(0);
		<SpawnRoutine>d__.<>4__this = this;
		<SpawnRoutine>d__.conn = conn;
		<SpawnRoutine>d__.starts = starts;
		return <SpawnRoutine>d__;
	}

	// Token: 0x06000F42 RID: 3906 RVA: 0x00040B98 File Offset: 0x0003ED98
	[TargetRpc]
	private void TargetLockPlayerInputs(NetworkConnection conn)
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		this.SendTargetRPCInternal(conn, "System.Void PlayerSpawnManager::TargetLockPlayerInputs(Mirror.NetworkConnection)", 851145063, writer, 0);
		NetworkWriterPool.Return(writer);
	}

	// Token: 0x06000F43 RID: 3907 RVA: 0x00040BC8 File Offset: 0x0003EDC8
	[Server]
	private void StartInitialSpawnSequence()
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void PlayerSpawnManager::StartInitialSpawnSequence()' called when server was not active");
			return;
		}
		if (this.initialSpawnSequenceStarted)
		{
			return;
		}
		this.initialSpawnSequenceStarted = true;
		base.StartCoroutine(this.InitialSpawnSequenceRoutine());
	}

	// Token: 0x06000F44 RID: 3908 RVA: 0x00040BFC File Offset: 0x0003EDFC
	[Server]
	private IEnumerator InitialSpawnSequenceRoutine()
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Collections.IEnumerator PlayerSpawnManager::InitialSpawnSequenceRoutine()' called when server was not active");
			return null;
		}
		PlayerSpawnManager.<InitialSpawnSequenceRoutine>d__33 <InitialSpawnSequenceRoutine>d__ = new PlayerSpawnManager.<InitialSpawnSequenceRoutine>d__33(0);
		<InitialSpawnSequenceRoutine>d__.<>4__this = this;
		return <InitialSpawnSequenceRoutine>d__;
	}

	// Token: 0x06000F45 RID: 3909 RVA: 0x00040C38 File Offset: 0x0003EE38
	[TargetRpc]
	private void TargetSceneTransition(NetworkConnection conn)
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		this.SendTargetRPCInternal(conn, "System.Void PlayerSpawnManager::TargetSceneTransition(Mirror.NetworkConnection)", -2064852653, writer, 0);
		NetworkWriterPool.Return(writer);
	}

	// Token: 0x06000F46 RID: 3910 RVA: 0x00040C68 File Offset: 0x0003EE68
	[Server]
	private IEnumerator SpawnTimeoutRoutine()
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Collections.IEnumerator PlayerSpawnManager::SpawnTimeoutRoutine()' called when server was not active");
			return null;
		}
		PlayerSpawnManager.<SpawnTimeoutRoutine>d__35 <SpawnTimeoutRoutine>d__ = new PlayerSpawnManager.<SpawnTimeoutRoutine>d__35(0);
		<SpawnTimeoutRoutine>d__.<>4__this = this;
		return <SpawnTimeoutRoutine>d__;
	}

	// Token: 0x06000F47 RID: 3911 RVA: 0x00040CA4 File Offset: 0x0003EEA4
	[Server]
	private int GetSpawnIndexFor(int connId, int total)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Int32 PlayerSpawnManager::GetSpawnIndexFor(System.Int32,System.Int32)' called when server was not active");
			return 0;
		}
		int result;
		if (this.spawnIndexByConnId.TryGetValue(connId, out result))
		{
			return result;
		}
		List<int> list = new List<int>();
		for (int i = 0; i < total; i++)
		{
			if (!this.usedSpawnPoints.Contains(i))
			{
				list.Add(i);
			}
		}
		int num;
		if (list.Count > 0)
		{
			num = list[Random.Range(0, list.Count)];
		}
		else
		{
			num = Random.Range(0, total);
		}
		this.usedSpawnPoints.Add(num);
		this.spawnIndexByConnId[connId] = num;
		return num;
	}

	// Token: 0x06000F49 RID: 3913 RVA: 0x00002321 File Offset: 0x00000521
	public override bool Weaved()
	{
		return true;
	}

	// Token: 0x06000F4A RID: 3914 RVA: 0x00040D9D File Offset: 0x0003EF9D
	protected void UserCode_TargetLockPlayerInputs__NetworkConnection(NetworkConnection conn)
	{
		InputEvents.ActiveLayer = InputLayer.SpawnBox;
	}

	// Token: 0x06000F4B RID: 3915 RVA: 0x00040DA5 File Offset: 0x0003EFA5
	protected static void InvokeUserCode_TargetLockPlayerInputs__NetworkConnection(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("TargetRPC TargetLockPlayerInputs called on server.");
			return;
		}
		((PlayerSpawnManager)obj).UserCode_TargetLockPlayerInputs__NetworkConnection(null);
	}

	// Token: 0x06000F4C RID: 3916 RVA: 0x00040DC9 File Offset: 0x0003EFC9
	protected void UserCode_TargetSceneTransition__NetworkConnection(NetworkConnection conn)
	{
		MonoSingleton<SceneTransitioner>.Instance.SetLoadingScreen(false, 0.5f, true);
	}

	// Token: 0x06000F4D RID: 3917 RVA: 0x00040DDC File Offset: 0x0003EFDC
	protected static void InvokeUserCode_TargetSceneTransition__NetworkConnection(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("TargetRPC TargetSceneTransition called on server.");
			return;
		}
		((PlayerSpawnManager)obj).UserCode_TargetSceneTransition__NetworkConnection(null);
	}

	// Token: 0x06000F4E RID: 3918 RVA: 0x00040E00 File Offset: 0x0003F000
	static PlayerSpawnManager()
	{
		RemoteProcedureCalls.RegisterRpc(typeof(PlayerSpawnManager), "System.Void PlayerSpawnManager::TargetLockPlayerInputs(Mirror.NetworkConnection)", new RemoteCallDelegate(PlayerSpawnManager.InvokeUserCode_TargetLockPlayerInputs__NetworkConnection));
		RemoteProcedureCalls.RegisterRpc(typeof(PlayerSpawnManager), "System.Void PlayerSpawnManager::TargetSceneTransition(Mirror.NetworkConnection)", new RemoteCallDelegate(PlayerSpawnManager.InvokeUserCode_TargetSceneTransition__NetworkConnection));
	}

	// Token: 0x040009D5 RID: 2517
	[SerializeField]
	private GameObject playerPrefab;

	// Token: 0x040009D6 RID: 2518
	[SerializeField]
	private GameObject emptyPlayerPrefab;

	// Token: 0x040009D7 RID: 2519
	private LobbySettings lobbySettings;

	// Token: 0x040009D8 RID: 2520
	private readonly HashSet<int> registered = new HashSet<int>();

	// Token: 0x040009D9 RID: 2521
	private readonly HashSet<int> ready = new HashSet<int>();

	// Token: 0x040009DA RID: 2522
	private readonly HashSet<int> spawning = new HashSet<int>();

	// Token: 0x040009DB RID: 2523
	private readonly Dictionary<int, int> spawnIndexByConnId = new Dictionary<int, int>();

	// Token: 0x040009DC RID: 2524
	private readonly HashSet<int> usedSpawnPoints = new HashSet<int>();

	// Token: 0x040009DD RID: 2525
	private int sceneEpoch;

	// Token: 0x040009DE RID: 2526
	private bool sceneAcceptingSpawns;

	// Token: 0x040009DF RID: 2527
	private bool spawningEnabled;

	// Token: 0x040009E0 RID: 2528
	private bool initialSpawnSequenceStarted;

	// Token: 0x040009E1 RID: 2529
	private int initializedEpoch = -1;

	// Token: 0x040009E2 RID: 2530
	private Coroutine timeoutCoroutine;
}
