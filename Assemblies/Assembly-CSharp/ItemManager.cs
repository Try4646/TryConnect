using System;
using System.Collections.Generic;
using Extensions;
using Mirror;
using UnityEngine;

// Token: 0x02000178 RID: 376
public class ItemManager : NetworkSingleton<ItemManager>
{
	// Token: 0x06000E40 RID: 3648 RVA: 0x0003B033 File Offset: 0x00039233
	public void ServerAddItem(SpawnableSO spawnableSo)
	{
		this.currentItems.Add(spawnableSo);
	}

	// Token: 0x06000E41 RID: 3649 RVA: 0x0003B044 File Offset: 0x00039244
	[Server]
	public void ServerRemoveItem(SpawnableSO spawnableSo, ConsumableItem itemInstance)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void ItemManager::ServerRemoveItem(SpawnableSO,ConsumableItem)' called when server was not active");
			return;
		}
		if (this.currentItems.Contains(spawnableSo))
		{
			this.currentItems.Remove(spawnableSo);
		}
		if (this.spawnedItemInstances.Contains(itemInstance))
		{
			this.spawnedItemInstances.Remove(itemInstance);
		}
	}

	// Token: 0x06000E42 RID: 3650 RVA: 0x0003B09C File Offset: 0x0003929C
	[Server]
	public List<int> GetCurrentItemIds()
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Collections.Generic.List`1<System.Int32> ItemManager::GetCurrentItemIds()' called when server was not active");
			return null;
		}
		List<int> list = new List<int>();
		foreach (SpawnableSO spawnableSO in this.currentItems)
		{
			if (spawnableSO)
			{
				list.Add(spawnableSO.spawnableID);
			}
		}
		return list;
	}

	// Token: 0x06000E43 RID: 3651 RVA: 0x0003B124 File Offset: 0x00039324
	[Server]
	public void SetCurrentItems(List<int> itemIds)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void ItemManager::SetCurrentItems(System.Collections.Generic.List`1<System.Int32>)' called when server was not active");
			return;
		}
		this.currentItems.Clear();
		foreach (int id in itemIds)
		{
			SpawnableSO spawnableSoById = SpawnableSettings.GetSpawnableSoById(id);
			if (spawnableSoById)
			{
				this.currentItems.Add(spawnableSoById);
			}
		}
	}

	// Token: 0x06000E44 RID: 3652 RVA: 0x0003B1A4 File Offset: 0x000393A4
	[Server]
	public void ServerResetItems()
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void ItemManager::ServerResetItems()' called when server was not active");
			return;
		}
		this.currentItems.Clear();
		this.spawnedItemInstances.Clear();
	}

	// Token: 0x06000E46 RID: 3654 RVA: 0x00002321 File Offset: 0x00000521
	public override bool Weaved()
	{
		return true;
	}

	// Token: 0x0400090B RID: 2315
	public List<SpawnableSO> currentItems = new List<SpawnableSO>();

	// Token: 0x0400090C RID: 2316
	public List<ConsumableItem> spawnedItemInstances = new List<ConsumableItem>();
}
