using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Extensions;
using Mirror;
using MoreMountains.Tools;
using UnityEngine;

// Token: 0x020001DF RID: 479
public class ItemStampManager : NetworkSingleton<ItemStampManager>
{
	// Token: 0x06001101 RID: 4353 RVA: 0x00048FEC File Offset: 0x000471EC
	protected override void OnAwake()
	{
		base.OnAwake();
		this._spawnedItems.Clear();
		this._spawnedItemInstances.Clear();
		this._instanceToStamp.Clear();
		this._purchasedStamps.Clear();
		this._rerollCount = 0;
	}

	// Token: 0x06001102 RID: 4354 RVA: 0x00049028 File Offset: 0x00047228
	[Server]
	public GameObject GetUniqueLoot(MMLootTableGameObjectSO lootTable)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'UnityEngine.GameObject ItemStampManager::GetUniqueLoot(MoreMountains.Tools.MMLootTableGameObjectSO)' called when server was not active");
			return null;
		}
		return this.GetUniqueLoot(lootTable, Vector3.zero);
	}

	// Token: 0x06001103 RID: 4355 RVA: 0x00049064 File Offset: 0x00047264
	[Server]
	public GameObject GetUniqueLoot(MMLootTableGameObjectSO lootTable, Vector3 stampPosition)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'UnityEngine.GameObject ItemStampManager::GetUniqueLoot(MoreMountains.Tools.MMLootTableGameObjectSO,UnityEngine.Vector3)' called when server was not active");
			return null;
		}
		if (lootTable == null)
		{
			Debug.LogError("ItemStampManager: Loot table is null.");
			return null;
		}
		string positionKey = this.GetPositionKey(stampPosition, lootTable);
		GameObject result;
		if (this._preAssignedItems.TryGetValue(positionKey, out result))
		{
			return result;
		}
		Debug.LogWarning(string.Format("ItemStampManager: NO pre-assigned item for position ({0:F2}, {1:F2}, {2:F2}) with loot table {3}. Key: {4}. Using fallback selection.", new object[]
		{
			stampPosition.x,
			stampPosition.y,
			stampPosition.z,
			lootTable.name,
			positionKey
		}));
		return this.GetUniqueLootFallback(lootTable, stampPosition);
	}

	// Token: 0x06001104 RID: 4356 RVA: 0x00049118 File Offset: 0x00047318
	[Server]
	private GameObject GetUniqueLootFallback(MMLootTableGameObjectSO lootTable, Vector3 stampPosition)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'UnityEngine.GameObject ItemStampManager::GetUniqueLootFallback(MoreMountains.Tools.MMLootTableGameObjectSO,UnityEngine.Vector3)' called when server was not active");
			return null;
		}
		if (lootTable.LootTable == null || lootTable.LootTable.ObjectsToLoot == null || lootTable.LootTable.ObjectsToLoot.Count == 0)
		{
			Debug.LogError("ItemStampManager: Loot table has no items.");
			return null;
		}
		List<GameObject> list = (from x in lootTable.LootTable.ObjectsToLoot
		where x != null && x.Loot != null
		select x.Loot).ToList<GameObject>();
		if (list.Count == 0)
		{
			Debug.LogError("ItemStampManager: No valid prefabs in loot table.");
			return null;
		}
		int successfulQuota = NetworkSingleton<GameManager>.Instance.successfulQuota;
		Random random = new Random(this.GetDeterministicHash(stampPosition, NetworkSingleton<SeededRandomManager>.Instance.CurrentSeed, successfulQuota));
		List<GameObject> list2 = list.ToList<GameObject>();
		for (int i = list2.Count - 1; i > 0; i--)
		{
			int index = random.Next(0, i + 1);
			GameObject value = list2[i];
			list2[i] = list2[index];
			list2[index] = value;
		}
		GameObject gameObject = null;
		for (int j = 0; j < list2.Count; j++)
		{
			GameObject gameObject2 = list2[j];
			if (!this._spawnedItems.Contains(gameObject2))
			{
				gameObject = gameObject2;
				this._spawnedItems.Add(gameObject2);
				break;
			}
		}
		if (gameObject == null)
		{
			gameObject = list2[0];
		}
		return gameObject;
	}

	// Token: 0x06001105 RID: 4357 RVA: 0x000492B4 File Offset: 0x000474B4
	private string GetPositionKey(Vector3 position, MMLootTableGameObjectSO lootTable)
	{
		int num = Mathf.RoundToInt(position.x * 100f);
		int num2 = Mathf.RoundToInt(position.y * 100f);
		int num3 = Mathf.RoundToInt(position.z * 100f);
		int num4 = (lootTable != null) ? lootTable.GetInstanceID() : -1;
		return string.Format("{0},{1},{2},{3}", new object[]
		{
			num,
			num2,
			num3,
			num4
		});
	}

	// Token: 0x06001106 RID: 4358 RVA: 0x00049340 File Offset: 0x00047540
	private int GetDeterministicHash(Vector3 position, int seed, int quotaIndex)
	{
		int num = seed * 31 + quotaIndex;
		int num2 = Mathf.RoundToInt(position.x * 100f);
		int num3 = Mathf.RoundToInt(position.y * 100f);
		int num4 = Mathf.RoundToInt(position.z * 100f);
		return ((num * 31 + num2) * 31 + num3) * 31 + num4;
	}

	// Token: 0x06001107 RID: 4359 RVA: 0x00049398 File Offset: 0x00047598
	[Server]
	public void MarkItemAsSpawned(GameObject itemPrefab)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void ItemStampManager::MarkItemAsSpawned(UnityEngine.GameObject)' called when server was not active");
			return;
		}
		if (itemPrefab != null)
		{
			this._spawnedItems.Add(itemPrefab);
		}
	}

	// Token: 0x06001108 RID: 4360 RVA: 0x000493C5 File Offset: 0x000475C5
	[Server]
	public void UnmarkItemAsSpawned(GameObject itemPrefab)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void ItemStampManager::UnmarkItemAsSpawned(UnityEngine.GameObject)' called when server was not active");
			return;
		}
		if (itemPrefab != null)
		{
			this._spawnedItems.Remove(itemPrefab);
		}
	}

	// Token: 0x06001109 RID: 4361 RVA: 0x000493F4 File Offset: 0x000475F4
	[Server]
	public void ResetTracking()
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void ItemStampManager::ResetTracking()' called when server was not active");
			return;
		}
		this._spawnedItems.Clear();
		this._spawnedItemInstances.Clear();
		this._instanceToStamp.Clear();
		this._purchasedStamps.Clear();
		this._rerollCount = 0;
		Debug.Log("ItemStampManager: Spawned items tracking has been reset.");
	}

	// Token: 0x0600110A RID: 4362 RVA: 0x00049454 File Offset: 0x00047654
	[Server]
	public int GetSpawnedItemCount()
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Int32 ItemStampManager::GetSpawnedItemCount()' called when server was not active");
			return 0;
		}
		return this._spawnedItems.Count;
	}

	// Token: 0x0600110B RID: 4363 RVA: 0x00049490 File Offset: 0x00047690
	[Server]
	public bool HasItemBeenSpawned(GameObject itemPrefab)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Boolean ItemStampManager::HasItemBeenSpawned(UnityEngine.GameObject)' called when server was not active");
			return default(bool);
		}
		return itemPrefab != null && this._spawnedItems.Contains(itemPrefab);
	}

	// Token: 0x0600110C RID: 4364 RVA: 0x000494D8 File Offset: 0x000476D8
	[Server]
	public void RegisterSpawnedInstance(GameObject instance, ItemStamp sourceStamp)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void ItemStampManager::RegisterSpawnedInstance(UnityEngine.GameObject,ItemStamp)' called when server was not active");
			return;
		}
		if (instance == null || sourceStamp == null)
		{
			return;
		}
		if (!this._spawnedItemInstances.Contains(instance))
		{
			this._spawnedItemInstances.Add(instance);
		}
		this._instanceToStamp[instance] = sourceStamp;
	}

	// Token: 0x0600110D RID: 4365 RVA: 0x00049534 File Offset: 0x00047734
	[Server]
	public ItemStamp GetStampFromInstance(GameObject instance)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'ItemStamp ItemStampManager::GetStampFromInstance(UnityEngine.GameObject)' called when server was not active");
			return null;
		}
		if (instance == null)
		{
			return null;
		}
		ItemStamp result;
		if (this._instanceToStamp.TryGetValue(instance, out result))
		{
			return result;
		}
		return null;
	}

	// Token: 0x0600110E RID: 4366 RVA: 0x00049580 File Offset: 0x00047780
	[Server]
	public void UnregisterSpawnedInstance(GameObject instance)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void ItemStampManager::UnregisterSpawnedInstance(UnityEngine.GameObject)' called when server was not active");
			return;
		}
		if (instance == null)
		{
			return;
		}
		this._spawnedItemInstances.Remove(instance);
		this._instanceToStamp.Remove(instance);
	}

	// Token: 0x0600110F RID: 4367 RVA: 0x000495BC File Offset: 0x000477BC
	[Server]
	public void MarkInstancePurchased(GameObject instance)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void ItemStampManager::MarkInstancePurchased(UnityEngine.GameObject)' called when server was not active");
			return;
		}
		if (instance == null)
		{
			return;
		}
		ItemStamp itemStamp;
		if (this._instanceToStamp.TryGetValue(instance, out itemStamp) && itemStamp != null)
		{
			this._purchasedStamps.Add(itemStamp);
		}
		this.UnregisterSpawnedInstance(instance);
	}

	// Token: 0x06001110 RID: 4368 RVA: 0x00049618 File Offset: 0x00047818
	[Server]
	private static void ServerDestroySpawnedLootObject(GameObject go)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void ItemStampManager::ServerDestroySpawnedLootObject(UnityEngine.GameObject)' called when server was not active");
			return;
		}
		if (go == null)
		{
			return;
		}
		ConsumableItem consumableItem;
		if (go.TryGetComponent<ConsumableItem>(out consumableItem))
		{
			consumableItem.DestroyItem();
			return;
		}
		Item item;
		if (go.TryGetComponent<Item>(out item))
		{
			item.ServerDrop();
			NetworkServer.Destroy(go);
			return;
		}
		NetworkServer.Destroy(go);
	}

	// Token: 0x06001111 RID: 4369 RVA: 0x00049674 File Offset: 0x00047874
	[Server]
	private List<GameObject> CollectInstancesForStamp(ItemStamp stamp)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Collections.Generic.List`1<UnityEngine.GameObject> ItemStampManager::CollectInstancesForStamp(ItemStamp)' called when server was not active");
			return null;
		}
		List<GameObject> list = new List<GameObject>();
		if (stamp == null)
		{
			return list;
		}
		foreach (KeyValuePair<GameObject, ItemStamp> keyValuePair in this._instanceToStamp)
		{
			if (keyValuePair.Value == stamp && keyValuePair.Key != null)
			{
				list.Add(keyValuePair.Key);
			}
		}
		return list;
	}

	// Token: 0x06001112 RID: 4370 RVA: 0x00049720 File Offset: 0x00047920
	[Server]
	private void DestroySpawnedInstancesForStamp(ItemStamp stamp)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void ItemStampManager::DestroySpawnedInstancesForStamp(ItemStamp)' called when server was not active");
			return;
		}
		foreach (GameObject gameObject in this.CollectInstancesForStamp(stamp))
		{
			this.UnregisterSpawnedInstance(gameObject);
			ItemStampManager.ServerDestroySpawnedLootObject(gameObject);
		}
	}

	// Token: 0x06001113 RID: 4371 RVA: 0x00049790 File Offset: 0x00047990
	[Server]
	private void DestroyAllSpawnedInstances()
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void ItemStampManager::DestroyAllSpawnedInstances()' called when server was not active");
			return;
		}
		List<GameObject> list = new List<GameObject>(this._spawnedItemInstances);
		this._spawnedItemInstances.Clear();
		this._instanceToStamp.Clear();
		for (int i = 0; i < list.Count; i++)
		{
			GameObject gameObject = list[i];
			if (gameObject != null)
			{
				ItemStampManager.ServerDestroySpawnedLootObject(gameObject);
			}
		}
	}

	// Token: 0x06001114 RID: 4372 RVA: 0x000497FC File Offset: 0x000479FC
	[Server]
	public void RetrieveAndRespawnItemStamp(ItemStamp stamp)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void ItemStampManager::RetrieveAndRespawnItemStamp(ItemStamp)' called when server was not active");
			return;
		}
		if (!base.isServer || stamp == null || !stamp.gameObject.activeInHierarchy)
		{
			return;
		}
		if (this._purchasedStamps.Contains(stamp))
		{
			return;
		}
		base.StartCoroutine(this.CoRetrieveAndRespawnItemStamp(stamp));
	}

	// Token: 0x06001115 RID: 4373 RVA: 0x0004985A File Offset: 0x00047A5A
	[Server]
	public void RetrieveAndRespawnItemStampForInstance(GameObject spawnedInstance)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void ItemStampManager::RetrieveAndRespawnItemStampForInstance(UnityEngine.GameObject)' called when server was not active");
			return;
		}
		if (!base.isServer)
		{
			return;
		}
		this.RetrieveAndRespawnItemStamp(this.GetStampFromInstance(spawnedInstance));
	}

	// Token: 0x06001116 RID: 4374 RVA: 0x00049888 File Offset: 0x00047A88
	[Server]
	public void OnLobbyStampItemConsumed(GameObject instance)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void ItemStampManager::OnLobbyStampItemConsumed(UnityEngine.GameObject)' called when server was not active");
			return;
		}
		if (!base.isServer || instance == null)
		{
			return;
		}
		ItemStamp stampFromInstance = this.GetStampFromInstance(instance);
		if (stampFromInstance == null || this._purchasedStamps.Contains(stampFromInstance))
		{
			return;
		}
		this.UnregisterSpawnedInstance(instance);
		base.StartCoroutine(this.CoRespawnStampAfterDeferredDestroy(stampFromInstance));
	}

	// Token: 0x06001117 RID: 4375 RVA: 0x000498F4 File Offset: 0x00047AF4
	[Server]
	private IEnumerator CoRetrieveAndRespawnItemStamp(ItemStamp stamp)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Collections.IEnumerator ItemStampManager::CoRetrieveAndRespawnItemStamp(ItemStamp)' called when server was not active");
			return null;
		}
		ItemStampManager.<CoRetrieveAndRespawnItemStamp>d__32 <CoRetrieveAndRespawnItemStamp>d__ = new ItemStampManager.<CoRetrieveAndRespawnItemStamp>d__32(0);
		<CoRetrieveAndRespawnItemStamp>d__.<>4__this = this;
		<CoRetrieveAndRespawnItemStamp>d__.stamp = stamp;
		return <CoRetrieveAndRespawnItemStamp>d__;
	}

	// Token: 0x06001118 RID: 4376 RVA: 0x00049938 File Offset: 0x00047B38
	[Server]
	private IEnumerator CoRespawnStampAfterDeferredDestroy(ItemStamp stamp)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Collections.IEnumerator ItemStampManager::CoRespawnStampAfterDeferredDestroy(ItemStamp)' called when server was not active");
			return null;
		}
		ItemStampManager.<CoRespawnStampAfterDeferredDestroy>d__33 <CoRespawnStampAfterDeferredDestroy>d__ = new ItemStampManager.<CoRespawnStampAfterDeferredDestroy>d__33(0);
		<CoRespawnStampAfterDeferredDestroy>d__.<>4__this = this;
		<CoRespawnStampAfterDeferredDestroy>d__.stamp = stamp;
		return <CoRespawnStampAfterDeferredDestroy>d__;
	}

	// Token: 0x06001119 RID: 4377 RVA: 0x0004997C File Offset: 0x00047B7C
	[Server]
	public void RerollAllItemStamps()
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void ItemStampManager::RerollAllItemStamps()' called when server was not active");
			return;
		}
		if (!base.isServer)
		{
			return;
		}
		this.DestroyAllSpawnedInstances();
		int seed = NetworkSingleton<SeededRandomManager>.Instance.CurrentSeed + 1;
		NetworkSingleton<SeededRandomManager>.Instance.InitializeSeed(seed);
		this._spawnedItems.Clear();
		this._preAssignedItems.Clear();
		List<ItemStamp> itemStamps = (from s in Object.FindObjectsByType<ItemStamp>(FindObjectsSortMode.None).ToList<ItemStamp>()
		where s != null && !this._purchasedStamps.Contains(s)
		select s).ToList<ItemStamp>();
		this.PreAssignItemsToStamps(itemStamps);
		base.StartCoroutine(this.InitializeAllItemStampsCoroutine(itemStamps));
	}

	// Token: 0x0600111A RID: 4378 RVA: 0x00049A14 File Offset: 0x00047C14
	[Server]
	public void TryRerollAllItemStampsWithCost()
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void ItemStampManager::TryRerollAllItemStampsWithCost()' called when server was not active");
			return;
		}
		if (!base.isServer)
		{
			return;
		}
		GameSettings gameSettings = Resources.Load<GameSettings>("GameSettings");
		if (gameSettings == null)
		{
			Debug.LogWarning("[ItemStampManager] GameSettings not found in Resources. Cannot charge reroll cost.");
			return;
		}
		GameSettings.CasinoFloorData currentFloorData = gameSettings.GetCurrentFloorData();
		if (currentFloorData == null)
		{
			Debug.LogWarning("[ItemStampManager] Current floor data is null. Cannot charge reroll cost.");
			return;
		}
		int num = currentFloorData.rerollCost;
		if (num < 0)
		{
			num = 0;
		}
		int num2 = (this._rerollCount + 1) * num;
		if (num2 > 0)
		{
			if (NetworkSingleton<MoneyManager>.Instance == null)
			{
				Debug.LogWarning("[ItemStampManager] MoneyManager not found. Cannot charge reroll cost.");
				return;
			}
			if (!NetworkSingleton<MoneyManager>.Instance.TryChangeTicketBalance((long)(-(long)num2)))
			{
				Debug.Log(string.Format("[ItemStampManager] Not enough tickets to reroll items. Need {0}, have {1}.", num2, NetworkSingleton<MoneyManager>.Instance.ticketBalance));
				return;
			}
		}
		this.RerollAllItemStamps();
		this._rerollCount++;
	}

	// Token: 0x0600111B RID: 4379 RVA: 0x00049AF0 File Offset: 0x00047CF0
	[Server]
	public int GetCurrentRerollCost()
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Int32 ItemStampManager::GetCurrentRerollCost()' called when server was not active");
			return 0;
		}
		int num = 0;
		GameSettings gameSettings = Resources.Load<GameSettings>("GameSettings");
		if (gameSettings != null)
		{
			GameSettings.CasinoFloorData currentFloorData = gameSettings.GetCurrentFloorData();
			if (currentFloorData != null)
			{
				num = currentFloorData.rerollCost;
				if (num < 0)
				{
					num = 0;
				}
			}
		}
		return (this._rerollCount + 1) * num;
	}

	// Token: 0x0600111C RID: 4380 RVA: 0x00049B58 File Offset: 0x00047D58
	[Server]
	public void RetrieveAndRespawnAllItemStamps()
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void ItemStampManager::RetrieveAndRespawnAllItemStamps()' called when server was not active");
			return;
		}
		if (!base.isServer)
		{
			return;
		}
		if (Time.time - this._lastRetrieveTime < 1f)
		{
			return;
		}
		this._lastRetrieveTime = Time.time;
		this.DestroyAllSpawnedInstances();
		this._spawnedItems.Clear();
		this._preAssignedItems.Clear();
		List<ItemStamp> itemStamps = (from s in Object.FindObjectsByType<ItemStamp>(FindObjectsSortMode.None).ToList<ItemStamp>()
		where s != null && !this._purchasedStamps.Contains(s)
		select s).ToList<ItemStamp>();
		this.PreAssignItemsToStamps(itemStamps);
		base.StartCoroutine(this.InitializeAllItemStampsCoroutine(itemStamps));
	}

	// Token: 0x0600111D RID: 4381 RVA: 0x00049BF8 File Offset: 0x00047DF8
	[Server]
	public IEnumerator InitializeManager()
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Collections.IEnumerator ItemStampManager::InitializeManager()' called when server was not active");
			return null;
		}
		ItemStampManager.<InitializeManager>d__38 <InitializeManager>d__ = new ItemStampManager.<InitializeManager>d__38(0);
		<InitializeManager>d__.<>4__this = this;
		return <InitializeManager>d__;
	}

	// Token: 0x0600111E RID: 4382 RVA: 0x00049C34 File Offset: 0x00047E34
	[Server]
	private void PreAssignItemsToStamps(List<ItemStamp> itemStamps)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void ItemStampManager::PreAssignItemsToStamps(System.Collections.Generic.List`1<ItemStamp>)' called when server was not active");
			return;
		}
		int successfulQuota = NetworkSingleton<GameManager>.Instance.successfulQuota;
		int currentSeed = NetworkSingleton<SeededRandomManager>.Instance.CurrentSeed;
		Dictionary<MMLootTableGameObjectSO, List<ItemStamp>> dictionary = new Dictionary<MMLootTableGameObjectSO, List<ItemStamp>>();
		foreach (ItemStamp itemStamp in itemStamps)
		{
			if (!(itemStamp == null))
			{
				MMLootTableGameObjectSO lootTableFromStamp = this.GetLootTableFromStamp(itemStamp);
				if (!(lootTableFromStamp == null))
				{
					if (!dictionary.ContainsKey(lootTableFromStamp))
					{
						dictionary[lootTableFromStamp] = new List<ItemStamp>();
					}
					dictionary[lootTableFromStamp].Add(itemStamp);
				}
			}
		}
		foreach (KeyValuePair<MMLootTableGameObjectSO, List<ItemStamp>> keyValuePair in from x in dictionary
		orderby x.Key.GetInstanceID()
		select x)
		{
			MMLootTableGameObjectSO key = keyValuePair.Key;
			List<ItemStamp> value = keyValuePair.Value;
			if (key.LootTable != null && key.LootTable.ObjectsToLoot != null)
			{
				List<GameObject> list = key.LootTable.ObjectsToLoot.Where((MMLootGameObject x) => x != null && x.Loot != null).Select((MMLootGameObject x) => x.Loot).ToList<GameObject>();
				if (list.Count != 0)
				{
					Random random = new Random(this.GetDeterministicHash(Vector3.zero, currentSeed, successfulQuota) * 31 + key.GetInstanceID());
					List<GameObject> list2 = list.ToList<GameObject>();
					for (int i = list2.Count - 1; i > 0; i--)
					{
						int index = random.Next(0, i + 1);
						GameObject value2 = list2[i];
						list2[i] = list2[index];
						list2[index] = value2;
					}
					List<ItemStamp> list3 = value.OrderBy((ItemStamp s) => s.transform.position.x).ThenBy((ItemStamp s) => s.transform.position.y).ThenBy((ItemStamp s) => s.transform.position.z).ToList<ItemStamp>();
					Dictionary<GameObject, int> dictionary2 = new Dictionary<GameObject, int>();
					foreach (ItemStamp itemStamp2 in list3)
					{
						Vector3 position = itemStamp2.transform.position;
						int num = new Random(this.GetDeterministicHash(position, currentSeed, successfulQuota) * 31 + key.GetInstanceID()).Next(0, list2.Count);
						GameObject gameObject = null;
						for (int j = 0; j < list2.Count; j++)
						{
							int index2 = (num + j) % list2.Count;
							GameObject gameObject2 = list2[index2];
							int num2;
							if (!dictionary2.TryGetValue(gameObject2, out num2) || num2 < this.maxAssignmentsPerItem)
							{
								gameObject = gameObject2;
								dictionary2[gameObject2] = num2 + 1;
								break;
							}
						}
						if (gameObject == null)
						{
							gameObject = list2[num];
						}
						string positionKey = this.GetPositionKey(position, key);
						this._preAssignedItems[positionKey] = gameObject;
					}
				}
			}
		}
	}

	// Token: 0x0600111F RID: 4383 RVA: 0x00049FF8 File Offset: 0x000481F8
	private MMLootTableGameObjectSO GetLootTableFromStamp(ItemStamp stamp)
	{
		if (stamp == null)
		{
			return null;
		}
		try
		{
			FieldInfo field = typeof(ItemStamp).GetField("lootTable", BindingFlags.Instance | BindingFlags.NonPublic);
			if (field != null)
			{
				return field.GetValue(stamp) as MMLootTableGameObjectSO;
			}
		}
		catch (Exception ex)
		{
			Debug.LogWarning("ItemStampManager: Failed to get loot table from ItemStamp " + stamp.gameObject.name + ": " + ex.Message);
		}
		return null;
	}

	// Token: 0x06001120 RID: 4384 RVA: 0x0004A080 File Offset: 0x00048280
	[Server]
	private IEnumerator InitializeAllItemStampsCoroutine(List<ItemStamp> itemStamps)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Collections.IEnumerator ItemStampManager::InitializeAllItemStampsCoroutine(System.Collections.Generic.List`1<ItemStamp>)' called when server was not active");
			return null;
		}
		ItemStampManager.<InitializeAllItemStampsCoroutine>d__41 <InitializeAllItemStampsCoroutine>d__ = new ItemStampManager.<InitializeAllItemStampsCoroutine>d__41(0);
		<InitializeAllItemStampsCoroutine>d__.<>4__this = this;
		<InitializeAllItemStampsCoroutine>d__.itemStamps = itemStamps;
		return <InitializeAllItemStampsCoroutine>d__;
	}

	// Token: 0x06001124 RID: 4388 RVA: 0x00002321 File Offset: 0x00000521
	public override bool Weaved()
	{
		return true;
	}

	// Token: 0x04000AF6 RID: 2806
	private readonly HashSet<GameObject> _spawnedItems = new HashSet<GameObject>();

	// Token: 0x04000AF7 RID: 2807
	private readonly List<GameObject> _spawnedItemInstances = new List<GameObject>();

	// Token: 0x04000AF8 RID: 2808
	private readonly Dictionary<GameObject, ItemStamp> _instanceToStamp = new Dictionary<GameObject, ItemStamp>();

	// Token: 0x04000AF9 RID: 2809
	private readonly HashSet<ItemStamp> _purchasedStamps = new HashSet<ItemStamp>();

	// Token: 0x04000AFA RID: 2810
	private int _rerollCount;

	// Token: 0x04000AFB RID: 2811
	private readonly Dictionary<string, GameObject> _preAssignedItems = new Dictionary<string, GameObject>();

	// Token: 0x04000AFC RID: 2812
	[Header("Performance Settings")]
	[Tooltip("Maximum number of ItemStamps to process per frame")]
	[SerializeField]
	private int maxStampsPerFrame = 5;

	// Token: 0x04000AFD RID: 2813
	[Tooltip("Maximum frame time in milliseconds before yielding")]
	[SerializeField]
	private float maxFrameTime = 16f;

	// Token: 0x04000AFE RID: 2814
	[Header("Uniqueness Settings")]
	[Tooltip("Maximum number of times each item prefab from a loot table can be assigned across its ItemStamps before duplicates are allowed again. 1 = each item at most once.")]
	[SerializeField]
	[Min(1f)]
	private int maxAssignmentsPerItem = 1;

	// Token: 0x04000AFF RID: 2815
	private float _lastRetrieveTime;
}
