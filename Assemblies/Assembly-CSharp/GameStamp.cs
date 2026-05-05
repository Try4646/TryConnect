using System;
using System.Collections;
using Mirror;
using Mirror.RemoteCalls;
using UnityEngine;

// Token: 0x020001DA RID: 474
public class GameStamp : NetworkBehaviour
{
	// Token: 0x060010D0 RID: 4304 RVA: 0x000483DA File Offset: 0x000465DA
	private void OnGamePrefabChanged()
	{
		if (!Application.isPlaying && this.previewEnabled)
		{
			this.UpdatePreview();
		}
	}

	// Token: 0x060010D1 RID: 4305 RVA: 0x000483DA File Offset: 0x000465DA
	private void OnFloorChanged()
	{
		if (!Application.isPlaying && this.previewEnabled)
		{
			this.UpdatePreview();
		}
	}

	// Token: 0x17000189 RID: 393
	// (get) Token: 0x060010D2 RID: 4306 RVA: 0x000483F1 File Offset: 0x000465F1
	private string PreviewStatus
	{
		get
		{
			if (!this.previewEnabled)
			{
				return "Preview Disabled";
			}
			if (!(this.currentPreview != null))
			{
				return "Preview Disabled";
			}
			return "Preview Active";
		}
	}

	// Token: 0x060010D3 RID: 4307 RVA: 0x0004841C File Offset: 0x0004661C
	public override void OnStartServer()
	{
		foreach (object obj in base.transform)
		{
			NetworkServer.Destroy(((Transform)obj).gameObject);
		}
		if (this.spawnOnServerStart)
		{
			this.Initialize();
		}
	}

	// Token: 0x060010D4 RID: 4308 RVA: 0x00048488 File Offset: 0x00046688
	public void Initialize()
	{
		if (!base.isServer)
		{
			return;
		}
		if (this.gamePrefab == null)
		{
			Debug.LogWarning("GameStamp on " + base.gameObject.name + ": Game prefab is not assigned. Skipping initialization.");
			NetworkServer.Destroy(base.gameObject);
			return;
		}
		if (this.gamePrefab.GetComponentInChildren<GameBase>() == null)
		{
			Debug.LogWarning(string.Concat(new string[]
			{
				"GameStamp on ",
				base.gameObject.name,
				": Prefab ",
				this.gamePrefab.name,
				" does not have a GameBase component. Skipping initialization."
			}));
			NetworkServer.Destroy(base.gameObject);
			return;
		}
		this.SpawnGame(this.gamePrefab, base.transform.position, base.transform.rotation, base.transform.localScale);
	}

	// Token: 0x060010D5 RID: 4309 RVA: 0x00048568 File Offset: 0x00046768
	[Server]
	private void SpawnGame(GameObject prefab, Vector3 position, Quaternion rotation, Vector3 scale)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void GameStamp::SpawnGame(UnityEngine.GameObject,UnityEngine.Vector3,UnityEngine.Quaternion,UnityEngine.Vector3)' called when server was not active");
			return;
		}
		if (prefab == null)
		{
			Debug.LogWarning("GameStamp: Tried to spawn null prefab on " + base.gameObject.name);
			NetworkServer.Destroy(base.gameObject);
			return;
		}
		GameObject gameObject = Object.Instantiate<GameObject>(prefab, position, rotation);
		gameObject.transform.localScale = scale;
		NetworkServer.Spawn(gameObject, null);
		if (this.floor != null)
		{
			GameBase componentInChildren = gameObject.GetComponentInChildren<GameBase>();
			if (componentInChildren != null)
			{
				componentInChildren.NetworkcasinoLevel = this.floor.floorIndex;
			}
		}
		base.StartCoroutine(this.SetParentHierarchyAfterDelay(gameObject));
	}

	// Token: 0x060010D6 RID: 4310 RVA: 0x00048613 File Offset: 0x00046813
	private IEnumerator SetParentHierarchyAfterDelay(GameObject spawnedGame)
	{
		yield return new WaitForSeconds(0.2f);
		this.RpcSetParentHierarchy(spawnedGame);
		yield break;
	}

	// Token: 0x060010D7 RID: 4311 RVA: 0x0004862C File Offset: 0x0004682C
	[ClientRpc]
	private void RpcSetParentHierarchy(GameObject spawnedGame)
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		writer.WriteGameObject(spawnedGame);
		this.SendRPCInternal("System.Void GameStamp::RpcSetParentHierarchy(UnityEngine.GameObject)", -745019018, writer, 0, true);
		NetworkWriterPool.Return(writer);
	}

	// Token: 0x060010D8 RID: 4312 RVA: 0x00048668 File Offset: 0x00046868
	public void UpdatePreview()
	{
		if (Application.isPlaying || !this.previewEnabled)
		{
			return;
		}
		this.ClearPreview();
		if (this.gamePrefab == null)
		{
			return;
		}
		this.currentPreview = Object.Instantiate<GameObject>(this.gamePrefab, base.transform.position, base.transform.rotation);
		this.currentPreview.transform.SetParent(base.transform);
		this.SetPreviewMode(this.currentPreview, true);
	}

	// Token: 0x060010D9 RID: 4313 RVA: 0x000486E4 File Offset: 0x000468E4
	private void SetPreviewMode(GameObject target, bool isPreview)
	{
		if (isPreview)
		{
			Collider[] componentsInChildren = target.GetComponentsInChildren<Collider>();
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				componentsInChildren[i].enabled = false;
			}
			foreach (Rigidbody rigidbody in target.GetComponentsInChildren<Rigidbody>())
			{
				rigidbody.isKinematic = true;
				rigidbody.detectCollisions = false;
			}
			GameBase componentInChildren = target.GetComponentInChildren<GameBase>();
			if (componentInChildren != null)
			{
				componentInChildren.enabled = false;
			}
		}
	}

	// Token: 0x060010DA RID: 4314 RVA: 0x00048750 File Offset: 0x00046950
	public void ClearPreview()
	{
		if (Application.isPlaying)
		{
			return;
		}
		while (base.transform.childCount > 0)
		{
			Transform child = base.transform.GetChild(0);
			if (child != null && child.gameObject != null)
			{
				Object.DestroyImmediate(child.gameObject);
			}
		}
		this.currentPreview = null;
	}

	// Token: 0x060010DB RID: 4315 RVA: 0x000487AC File Offset: 0x000469AC
	private void OnDrawGizmosSelected()
	{
		if (!Application.isPlaying && this.currentPreview != null && this.currentPreview.transform != null)
		{
			this.currentPreview.transform.position = base.transform.position;
			this.currentPreview.transform.rotation = base.transform.rotation;
		}
	}

	// Token: 0x060010DD RID: 4317 RVA: 0x00002321 File Offset: 0x00000521
	public override bool Weaved()
	{
		return true;
	}

	// Token: 0x060010DE RID: 4318 RVA: 0x00048830 File Offset: 0x00046A30
	protected void UserCode_RpcSetParentHierarchy__GameObject(GameObject spawnedGame)
	{
		if (spawnedGame != null && this != null && base.transform != null && base.transform.parent != null)
		{
			spawnedGame.transform.SetParent(base.transform.parent);
		}
		if (this != null)
		{
			Object.Destroy(base.gameObject);
		}
	}

	// Token: 0x060010DF RID: 4319 RVA: 0x0004889A File Offset: 0x00046A9A
	protected static void InvokeUserCode_RpcSetParentHierarchy__GameObject(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcSetParentHierarchy called on server.");
			return;
		}
		((GameStamp)obj).UserCode_RpcSetParentHierarchy__GameObject(reader.ReadGameObject());
	}

	// Token: 0x060010E0 RID: 4320 RVA: 0x000488C3 File Offset: 0x00046AC3
	static GameStamp()
	{
		RemoteProcedureCalls.RegisterRpc(typeof(GameStamp), "System.Void GameStamp::RpcSetParentHierarchy(UnityEngine.GameObject)", new RemoteCallDelegate(GameStamp.InvokeUserCode_RpcSetParentHierarchy__GameObject));
	}

	// Token: 0x04000AE2 RID: 2786
	[Header("Game Prefab")]
	[Tooltip("The casino game prefab to spawn. Must have a GameBase component.")]
	[SerializeField]
	private GameObject gamePrefab;

	// Token: 0x04000AE3 RID: 2787
	[Header("Floor Reference")]
	[Tooltip("The casino floor this game belongs to. Used to set the casinoLevel on spawned games.")]
	[SerializeField]
	public CasinoFloor floor;

	// Token: 0x04000AE4 RID: 2788
	[Header("Spawn Settings")]
	[Tooltip("If true, spawns immediately on server start. If false, waits for Initialize() to be called.")]
	[SerializeField]
	private bool spawnOnServerStart = true;

	// Token: 0x04000AE5 RID: 2789
	[Header("Preview")]
	[SerializeField]
	private GameObject currentPreview;

	// Token: 0x04000AE6 RID: 2790
	[SerializeField]
	public bool previewEnabled = true;
}
