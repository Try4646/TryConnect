using System;
using System.Collections;
using Extensions;
using Mirror;
using UnityEngine;
using UnityEngine.SceneManagement;

// Token: 0x02000214 RID: 532
public class CustomNetworkManager : NetworkManager
{
	// Token: 0x06001391 RID: 5009 RVA: 0x0005409C File Offset: 0x0005229C
	public override void OnStartServer()
	{
		base.OnStartServer();
		NetworkServer.ReplaceHandler<JoinGameMessage>(new Action<NetworkConnectionToClient, JoinGameMessage>(this.OnJoinGameMessage), true);
		NetworkServer.ReplaceHandler<SceneReadyMessage>(new Action<NetworkConnectionToClient, SceneReadyMessage>(this.OnSceneReadyMessage), true);
		NetworkServer.ReplaceHandler<ClientScenePlayReadyMessage>(new Action<NetworkConnectionToClient, ClientScenePlayReadyMessage>(this.OnClientScenePlayReadyMessage), true);
		this.SpawnManagersPrefab();
		if (NetworkSingleton<PlayerSpawnManager>.Instance != null)
		{
			NetworkSingleton<PlayerSpawnManager>.Instance.OnAllPlayersSpawned -= this.OnAllPlayersSpawned;
			NetworkSingleton<PlayerSpawnManager>.Instance.OnAllPlayersSpawned += this.OnAllPlayersSpawned;
			NetworkSingleton<PlayerSpawnManager>.Instance.ServerOnSceneChanged(SceneManager.GetActiveScene().name);
		}
	}

	// Token: 0x06001392 RID: 5010 RVA: 0x0005413B File Offset: 0x0005233B
	public override void OnStopServer()
	{
		base.OnStopServer();
		NetworkServer.UnregisterHandler<JoinGameMessage>();
		NetworkServer.UnregisterHandler<SceneReadyMessage>();
		NetworkServer.UnregisterHandler<ClientScenePlayReadyMessage>();
		if (NetworkSingleton<PlayerSpawnManager>.Instance != null)
		{
			NetworkSingleton<PlayerSpawnManager>.Instance.OnAllPlayersSpawned -= this.OnAllPlayersSpawned;
		}
	}

	// Token: 0x06001393 RID: 5011 RVA: 0x00054178 File Offset: 0x00052378
	public override void OnClientConnect()
	{
		base.OnClientConnect();
		if (!NetworkClient.ready)
		{
			NetworkClient.Ready();
		}
		if (!this.sentJoin)
		{
			this.sentJoin = true;
			NetworkClient.Send<JoinGameMessage>(default(JoinGameMessage), 0);
		}
		string name = SceneManager.GetActiveScene().name;
		if (!string.IsNullOrEmpty(name) && name != this.lastSceneReadySent)
		{
			this.lastSceneReadySent = name;
			base.StartCoroutine(this.SendSceneReadyNextFrame());
		}
	}

	// Token: 0x06001394 RID: 5012 RVA: 0x000541EE File Offset: 0x000523EE
	public override void OnStopClient()
	{
		base.OnStopClient();
		this.sentJoin = false;
		this.lastSceneReadySent = null;
	}

	// Token: 0x06001395 RID: 5013 RVA: 0x00054204 File Offset: 0x00052404
	public override void OnClientSceneChanged()
	{
		base.OnClientSceneChanged();
		if (!NetworkClient.isConnected)
		{
			return;
		}
		if (!NetworkClient.ready)
		{
			NetworkClient.Ready();
		}
		string name = SceneManager.GetActiveScene().name;
		if (string.IsNullOrEmpty(name))
		{
			return;
		}
		if (name == this.lastSceneReadySent)
		{
			return;
		}
		this.lastSceneReadySent = name;
		base.StartCoroutine(this.SendSceneReadyNextFrame());
	}

	// Token: 0x06001396 RID: 5014 RVA: 0x00054266 File Offset: 0x00052466
	private IEnumerator SendSceneReadyNextFrame()
	{
		yield return null;
		if (NetworkClient.isConnected)
		{
			NetworkClient.Send<SceneReadyMessage>(default(SceneReadyMessage), 0);
		}
		yield break;
	}

	// Token: 0x06001397 RID: 5015 RVA: 0x00054270 File Offset: 0x00052470
	public override void OnServerSceneChanged(string sceneName)
	{
		base.OnServerSceneChanged(sceneName);
		if (NetworkSingleton<PlayerSpawnManager>.Instance != null)
		{
			NetworkSingleton<PlayerSpawnManager>.Instance.OnAllPlayersSpawned -= this.OnAllPlayersSpawned;
			NetworkSingleton<PlayerSpawnManager>.Instance.OnAllPlayersSpawned += this.OnAllPlayersSpawned;
			NetworkSingleton<PlayerSpawnManager>.Instance.ServerOnSceneChanged(sceneName);
		}
	}

	// Token: 0x06001398 RID: 5016 RVA: 0x000542C8 File Offset: 0x000524C8
	private void OnAllPlayersSpawned()
	{
		if (!NetworkServer.active)
		{
			return;
		}
		if (NetworkSingleton<GameManager>.Instance == null)
		{
			return;
		}
		string name = SceneManager.GetActiveScene().name;
		NetworkSingleton<GameManager>.Instance.InitializeScene(name);
	}

	// Token: 0x06001399 RID: 5017 RVA: 0x00054304 File Offset: 0x00052504
	private void OnJoinGameMessage(NetworkConnectionToClient conn, JoinGameMessage message)
	{
		PlayerSpawnManager instance = NetworkSingleton<PlayerSpawnManager>.Instance;
		if (instance == null)
		{
			return;
		}
		instance.RegisterConnection(conn);
	}

	// Token: 0x0600139A RID: 5018 RVA: 0x00054316 File Offset: 0x00052516
	private void OnSceneReadyMessage(NetworkConnectionToClient conn, SceneReadyMessage message)
	{
		PlayerSpawnManager instance = NetworkSingleton<PlayerSpawnManager>.Instance;
		if (instance == null)
		{
			return;
		}
		instance.OnClientSceneReady(conn);
	}

	// Token: 0x0600139B RID: 5019 RVA: 0x00054328 File Offset: 0x00052528
	private void OnClientScenePlayReadyMessage(NetworkConnectionToClient conn, ClientScenePlayReadyMessage message)
	{
		GameManager instance = NetworkSingleton<GameManager>.Instance;
		if (instance == null)
		{
			return;
		}
		instance.ServerOnClientScenePlayReady(conn, message.epoch);
	}

	// Token: 0x0600139C RID: 5020 RVA: 0x00054340 File Offset: 0x00052540
	public override void OnServerDisconnect(NetworkConnectionToClient conn)
	{
		if (((conn != null) ? conn.identity : null) != null)
		{
			PlayerCarry playerCarry;
			PlayerInventory playerInventory;
			if (conn.identity.TryGetComponent<PlayerCarry>(out playerCarry) && playerCarry.TryGetHolderInventory(out playerInventory))
			{
				playerInventory.ServerDropHoldingItem();
			}
			PlayerInventory playerInventory2;
			if (conn.identity.TryGetComponent<PlayerInventory>(out playerInventory2))
			{
				playerInventory2.ServerDropHoldingItem();
			}
		}
		PlayerSpawnManager instance = NetworkSingleton<PlayerSpawnManager>.Instance;
		if (instance != null)
		{
			instance.ServerOnDisconnected(conn);
		}
		SkipUI[] array = Object.FindObjectsByType<SkipUI>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
		for (int i = 0; i < array.Length; i++)
		{
			array[i].ServerRegisterSkipFromConnection(conn);
		}
		base.OnServerDisconnect(conn);
	}

	// Token: 0x0600139D RID: 5021 RVA: 0x000048A7 File Offset: 0x00002AA7
	public override void OnServerAddPlayer(NetworkConnectionToClient conn)
	{
	}

	// Token: 0x0600139E RID: 5022 RVA: 0x000543D3 File Offset: 0x000525D3
	[Server]
	private void SpawnManagersPrefab()
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void CustomNetworkManager::SpawnManagersPrefab()' called when server was not active");
			return;
		}
		NetworkServer.Spawn(Object.Instantiate<GameObject>(this.managersPrefab), null);
	}

	// Token: 0x04000C76 RID: 3190
	private string lastSceneReadySent;

	// Token: 0x04000C77 RID: 3191
	private bool sentJoin;

	// Token: 0x04000C78 RID: 3192
	[SerializeField]
	private GameObject managersPrefab;
}
