using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Extensions;
using Mirror;
using Mirror.RemoteCalls;
using MoreMountains.Tools;
using UnityEngine;

// Token: 0x020001DC RID: 476
public class ItemStamp : NetworkBehaviour
{
	// Token: 0x1700018C RID: 396
	// (get) Token: 0x060010E7 RID: 4327 RVA: 0x00048954 File Offset: 0x00046B54
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

	// Token: 0x060010E8 RID: 4328 RVA: 0x00048980 File Offset: 0x00046B80
	public override void OnStartServer()
	{
		foreach (object obj in base.transform)
		{
			NetworkServer.Destroy(((Transform)obj).gameObject);
		}
	}

	// Token: 0x060010E9 RID: 4329 RVA: 0x000489DC File Offset: 0x00046BDC
	public void Initialize()
	{
		if (!base.isServer)
		{
			return;
		}
		if (this.lootTable == null)
		{
			Debug.LogWarning("ItemStamp on " + base.gameObject.name + ": Loot table is not assigned. Skipping initialization.");
			NetworkServer.Destroy(base.gameObject);
			return;
		}
		if (NetworkSingleton<ItemStampManager>.Instance == null)
		{
			Debug.LogError("ItemStamp on " + base.gameObject.name + ": ItemStampManager.Instance is null! Cannot spawn item deterministically.");
			NetworkServer.Destroy(base.gameObject);
			return;
		}
		GameObject uniqueLoot = NetworkSingleton<ItemStampManager>.Instance.GetUniqueLoot(this.lootTable, base.transform.position);
		if (uniqueLoot == null)
		{
			Debug.LogWarning("ItemStamp on " + base.gameObject.name + ": Could not get item prefab. Skipping initialization.");
			NetworkServer.Destroy(base.gameObject);
			return;
		}
		if (uniqueLoot.GetComponent<Item>() == null)
		{
			Debug.LogWarning(string.Concat(new string[]
			{
				"ItemStamp on ",
				base.gameObject.name,
				": Prefab ",
				uniqueLoot.name,
				" does not have an Item component. Skipping initialization."
			}));
			NetworkServer.Destroy(base.gameObject);
			return;
		}
		this.SpawnItem(uniqueLoot, base.transform.position, base.transform.rotation, base.transform.localScale);
	}

	// Token: 0x060010EA RID: 4330 RVA: 0x00048B34 File Offset: 0x00046D34
	[Server]
	private void SpawnItem(GameObject prefab, Vector3 position, Quaternion rotation, Vector3 scale)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void ItemStamp::SpawnItem(UnityEngine.GameObject,UnityEngine.Vector3,UnityEngine.Quaternion,UnityEngine.Vector3)' called when server was not active");
			return;
		}
		if (prefab == null)
		{
			Debug.LogWarning("ItemStamp: Tried to spawn null prefab on " + base.gameObject.name);
			return;
		}
		GameObject gameObject = Object.Instantiate<GameObject>(prefab, position, rotation);
		gameObject.transform.localScale = scale;
		NetworkServer.Spawn(gameObject, null);
		if (NetworkSingleton<ItemStampManager>.Instance != null)
		{
			NetworkSingleton<ItemStampManager>.Instance.RegisterSpawnedInstance(gameObject, this);
		}
		base.StartCoroutine(this.SetParentHierarchyAfterDelay(gameObject));
	}

	// Token: 0x060010EB RID: 4331 RVA: 0x00048BBE File Offset: 0x00046DBE
	private IEnumerator SetParentHierarchyAfterDelay(GameObject spawnedItem)
	{
		yield return new WaitForSeconds(0.2f);
		this.RpcSetParentHierarchy(spawnedItem);
		yield break;
	}

	// Token: 0x060010EC RID: 4332 RVA: 0x00048BD4 File Offset: 0x00046DD4
	[ClientRpc]
	private void RpcSetParentHierarchy(GameObject spawnedItem)
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		writer.WriteGameObject(spawnedItem);
		this.SendRPCInternal("System.Void ItemStamp::RpcSetParentHierarchy(UnityEngine.GameObject)", 1388244683, writer, 0, true);
		NetworkWriterPool.Return(writer);
	}

	// Token: 0x060010ED RID: 4333 RVA: 0x00048C10 File Offset: 0x00046E10
	public void UpdatePreview()
	{
		if (Application.isPlaying || !this.previewEnabled)
		{
			return;
		}
		this.ClearPreview();
		if (this.lootTable == null)
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
		Debug.LogWarning("ItemStamp on " + base.gameObject.name + ": No preview prefab found from loot table.");
	}

	// Token: 0x060010EE RID: 4334 RVA: 0x00048CB8 File Offset: 0x00046EB8
	private GameObject GetPreviewPrefab()
	{
		if (this.lootTable == null || this.lootTable.LootTable == null || this.lootTable.LootTable.ObjectsToLoot == null)
		{
			return null;
		}
		List<MMLootGameObject> objectsToLoot = this.lootTable.LootTable.ObjectsToLoot;
		if (objectsToLoot == null || objectsToLoot.Count == 0)
		{
			return null;
		}
		List<GameObject> list = (from x in objectsToLoot
		where x != null && x.Loot != null
		select x.Loot).ToList<GameObject>();
		if (list.Count == 0)
		{
			return null;
		}
		return list[NetworkSingleton<SeededRandomManager>.Instance.Range(0, list.Count)];
	}

	// Token: 0x060010EF RID: 4335 RVA: 0x00048D84 File Offset: 0x00046F84
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
			Item component = target.GetComponent<Item>();
			if (component != null)
			{
				component.enabled = false;
			}
		}
	}

	// Token: 0x060010F0 RID: 4336 RVA: 0x00048DF0 File Offset: 0x00046FF0
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

	// Token: 0x060010F1 RID: 4337 RVA: 0x00048E4C File Offset: 0x0004704C
	private void OnDrawGizmosSelected()
	{
		if (!Application.isPlaying && this.currentPreview != null && this.currentPreview.transform != null)
		{
			this.currentPreview.transform.position = base.transform.position;
			this.currentPreview.transform.rotation = base.transform.rotation;
		}
	}

	// Token: 0x060010F3 RID: 4339 RVA: 0x00002321 File Offset: 0x00000521
	public override bool Weaved()
	{
		return true;
	}

	// Token: 0x060010F4 RID: 4340 RVA: 0x00048ED0 File Offset: 0x000470D0
	protected void UserCode_RpcSetParentHierarchy__GameObject(GameObject spawnedItem)
	{
		if (spawnedItem != null && this != null && base.transform != null && base.transform.parent != null)
		{
			spawnedItem.transform.SetParent(base.transform.parent);
		}
	}

	// Token: 0x060010F5 RID: 4341 RVA: 0x00048F26 File Offset: 0x00047126
	protected static void InvokeUserCode_RpcSetParentHierarchy__GameObject(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcSetParentHierarchy called on server.");
			return;
		}
		((ItemStamp)obj).UserCode_RpcSetParentHierarchy__GameObject(reader.ReadGameObject());
	}

	// Token: 0x060010F6 RID: 4342 RVA: 0x00048F4F File Offset: 0x0004714F
	static ItemStamp()
	{
		RemoteProcedureCalls.RegisterRpc(typeof(ItemStamp), "System.Void ItemStamp::RpcSetParentHierarchy(UnityEngine.GameObject)", new RemoteCallDelegate(ItemStamp.InvokeUserCode_RpcSetParentHierarchy__GameObject));
	}

	// Token: 0x04000AEB RID: 2795
	[Header("Loot Table")]
	[Tooltip("The loot table containing item prefabs to spawn. Items should be SpawnableSO prefabs.")]
	[SerializeField]
	private MMLootTableGameObjectSO lootTable;

	// Token: 0x04000AEC RID: 2796
	[Header("Spawn Settings")]
	[Tooltip("If true, spawns immediately on server start. If false, waits for Initialize() to be called.")]
	[SerializeField]
	private bool spawnOnServerStart = true;

	// Token: 0x04000AED RID: 2797
	[Header("Preview")]
	[SerializeField]
	private GameObject currentPreview;

	// Token: 0x04000AEE RID: 2798
	[SerializeField]
	public bool previewEnabled = true;
}
