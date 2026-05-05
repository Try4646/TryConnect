using System;
using System.Collections;
using Extensions;
using Mirror;
using Mirror.RemoteCalls;
using UnityEngine;

// Token: 0x020001E5 RID: 485
public class ObjectStamp : NetworkBehaviour
{
	// Token: 0x17000197 RID: 407
	// (get) Token: 0x06001147 RID: 4423 RVA: 0x0004A5EB File Offset: 0x000487EB
	public CasinoFloor Floor
	{
		get
		{
			return this.floor;
		}
	}

	// Token: 0x06001148 RID: 4424 RVA: 0x0004A5F3 File Offset: 0x000487F3
	private void OnFloorChanged()
	{
		if (!Application.isPlaying && this.previewEnabled)
		{
			this.UpdatePreview();
		}
	}

	// Token: 0x06001149 RID: 4425 RVA: 0x0004A60C File Offset: 0x0004880C
	public void UpdatePreview()
	{
		if (Application.isPlaying || !this.previewEnabled)
		{
			return;
		}
		this.ClearPreview();
		if (this.floor == null)
		{
			return;
		}
		GameObject previewPrefab = this.GetPreviewPrefab();
		if (previewPrefab != null)
		{
			this.currentPreview = Object.Instantiate<GameObject>(previewPrefab, base.transform.position, base.transform.rotation);
			this.currentPreview.transform.SetParent(base.transform);
			this.SetPreviewMode(this.currentPreview, true);
			return;
		}
		Debug.LogWarning("No preview prefab found for this floor (check StampManager floor loot tables).");
	}

	// Token: 0x0600114A RID: 4426 RVA: 0x0004A6A0 File Offset: 0x000488A0
	private GameObject GetPreviewPrefab()
	{
		if (this.floor == null)
		{
			return null;
		}
		StampManager stampManager = Object.FindAnyObjectByType<StampManager>();
		if (stampManager == null)
		{
			return null;
		}
		return stampManager.GetRandomPreviewPrefabForFloor(this.floor);
	}

	// Token: 0x17000198 RID: 408
	// (get) Token: 0x0600114B RID: 4427 RVA: 0x0004A6DA File Offset: 0x000488DA
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

	// Token: 0x0600114C RID: 4428 RVA: 0x0004A704 File Offset: 0x00048904
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

	// Token: 0x0600114D RID: 4429 RVA: 0x0004A770 File Offset: 0x00048970
	public void Initialize()
	{
		if (!base.isServer)
		{
			return;
		}
		GameObject gameObject = null;
		bool flag = this.requireFloorData && !this.spawnOnServerStart;
		if (this.directSpawnPrefab != null)
		{
			gameObject = this.directSpawnPrefab;
		}
		else if (this.floor != null && NetworkSingleton<StampManager>.Instance != null)
		{
			gameObject = NetworkSingleton<StampManager>.Instance.GetCappedLootForFloor(this.floor, base.transform.position);
		}
		else
		{
			if (flag && this.floor == null)
			{
				Debug.LogWarning("ObjectStamp on " + base.gameObject.name + ": Floor data is required but not set. Skipping initialization.");
				NetworkServer.Destroy(base.gameObject);
				return;
			}
			if (this.spawnOnServerStart && this.directSpawnPrefab == null)
			{
				Debug.Log("ObjectStamp on " + base.gameObject.name + ": spawnOnServerStart is enabled but no directSpawnPrefab is set. Stamp will remain inactive.");
				return;
			}
		}
		if (gameObject == null)
		{
			if (!this.spawnOnServerStart)
			{
				NetworkServer.Destroy(base.gameObject);
			}
			return;
		}
		this.SpawnObject(gameObject, base.transform.position, base.transform.rotation, base.transform.localScale);
	}

	// Token: 0x0600114E RID: 4430 RVA: 0x0004A8A8 File Offset: 0x00048AA8
	[Server]
	private void SpawnObject(GameObject prefab, Vector3 position, Quaternion rotation, Vector3 scale)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void ObjectStamp::SpawnObject(UnityEngine.GameObject,UnityEngine.Vector3,UnityEngine.Quaternion,UnityEngine.Vector3)' called when server was not active");
			return;
		}
		if (prefab == null)
		{
			Debug.LogWarning("ObjectStamp: Tried to spawn null prefab on " + base.gameObject.name);
			NetworkServer.Destroy(base.gameObject);
			return;
		}
		GameObject gameObject = Object.Instantiate<GameObject>(prefab, position, rotation);
		gameObject.transform.localScale = scale;
		NetworkServer.Spawn(gameObject, null);
		foreach (object obj in gameObject.transform)
		{
			using (IEnumerator enumerator2 = ((Transform)obj).GetEnumerator())
			{
				while (enumerator2.MoveNext())
				{
					GameStamp gameStamp;
					if (((Transform)enumerator2.Current).TryGetComponent<GameStamp>(out gameStamp))
					{
						gameStamp.floor = this.floor;
						gameStamp.Initialize();
					}
				}
			}
		}
		this.RpcSetParentHierarchy(gameObject);
	}

	// Token: 0x0600114F RID: 4431 RVA: 0x0004A9BC File Offset: 0x00048BBC
	[ClientRpc]
	private void RpcSetParentHierarchy(GameObject spawnedObject)
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		writer.WriteGameObject(spawnedObject);
		this.SendRPCInternal("System.Void ObjectStamp::RpcSetParentHierarchy(UnityEngine.GameObject)", -2142009037, writer, 0, true);
		NetworkWriterPool.Return(writer);
	}

	// Token: 0x06001150 RID: 4432 RVA: 0x0004A9F8 File Offset: 0x00048BF8
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
			foreach (MonoBehaviour monoBehaviour in target.GetComponentsInChildren<MonoBehaviour>())
			{
				if (monoBehaviour is GameBase)
				{
					monoBehaviour.enabled = false;
				}
			}
		}
	}

	// Token: 0x06001151 RID: 4433 RVA: 0x0004AA78 File Offset: 0x00048C78
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

	// Token: 0x06001152 RID: 4434 RVA: 0x0004AAD4 File Offset: 0x00048CD4
	private void OnDrawGizmosSelected()
	{
		if (!Application.isPlaying && this.currentPreview != null && this.currentPreview.transform != null)
		{
			this.currentPreview.transform.position = base.transform.position;
			this.currentPreview.transform.rotation = base.transform.rotation;
		}
	}

	// Token: 0x06001154 RID: 4436 RVA: 0x00002321 File Offset: 0x00000521
	public override bool Weaved()
	{
		return true;
	}

	// Token: 0x06001155 RID: 4437 RVA: 0x0004AB55 File Offset: 0x00048D55
	protected void UserCode_RpcSetParentHierarchy__GameObject(GameObject spawnedObject)
	{
		spawnedObject.transform.SetParent(base.gameObject.transform.parent);
		Object.Destroy(base.gameObject);
	}

	// Token: 0x06001156 RID: 4438 RVA: 0x0004AB7D File Offset: 0x00048D7D
	protected static void InvokeUserCode_RpcSetParentHierarchy__GameObject(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcSetParentHierarchy called on server.");
			return;
		}
		((ObjectStamp)obj).UserCode_RpcSetParentHierarchy__GameObject(reader.ReadGameObject());
	}

	// Token: 0x06001157 RID: 4439 RVA: 0x0004ABA6 File Offset: 0x00048DA6
	static ObjectStamp()
	{
		RemoteProcedureCalls.RegisterRpc(typeof(ObjectStamp), "System.Void ObjectStamp::RpcSetParentHierarchy(UnityEngine.GameObject)", new RemoteCallDelegate(ObjectStamp.InvokeUserCode_RpcSetParentHierarchy__GameObject));
	}

	// Token: 0x04000B1D RID: 2845
	[Header("Floor Reference")]
	[SerializeField]
	private CasinoFloor floor;

	// Token: 0x04000B1E RID: 2846
	[SerializeField]
	private bool requireFloorData = true;

	// Token: 0x04000B1F RID: 2847
	[Header("Preview")]
	[SerializeField]
	private GameObject currentPreview;

	// Token: 0x04000B20 RID: 2848
	[SerializeField]
	public bool previewEnabled = true;

	// Token: 0x04000B21 RID: 2849
	[Header("Spawn Settings")]
	[Tooltip("If true, spawns immediately on server start instead of waiting for StampManager initialization. Also automatically disables floor data requirement.")]
	[SerializeField]
	private bool spawnOnServerStart;

	// Token: 0x04000B22 RID: 2850
	[Header("Direct Spawn (Server Start Mode)")]
	[Tooltip("Prefab to spawn directly when spawnOnServerStart is enabled. If not set, will use floor-based loot system.")]
	[SerializeField]
	private GameObject directSpawnPrefab;
}
