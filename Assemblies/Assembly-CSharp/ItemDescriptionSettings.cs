using System;
using System.Collections.Generic;
using UnityEngine;

// Token: 0x020000CF RID: 207
[CreateAssetMenu(menuName = "Game Settings/Item Description Settings", fileName = "ItemDescriptionSettings")]
public class ItemDescriptionSettings : ScriptableObject
{
	// Token: 0x06000801 RID: 2049 RVA: 0x00020504 File Offset: 0x0001E704
	public string GetDescription(SpawnableSO spawnableSO)
	{
		if (spawnableSO == null)
		{
			return this.defaultDescription;
		}
		this.BuildCacheIfNeeded();
		ItemDescriptionSettings.ItemDescriptionData itemDescriptionData;
		if (!this._descriptionCache.TryGetValue(spawnableSO.spawnableID, out itemDescriptionData))
		{
			return this.defaultDescription;
		}
		if (!string.IsNullOrEmpty(itemDescriptionData.description))
		{
			return itemDescriptionData.description;
		}
		return this.defaultDescription;
	}

	// Token: 0x06000802 RID: 2050 RVA: 0x00020560 File Offset: 0x0001E760
	private void BuildCacheIfNeeded()
	{
		if (this._descriptionCache == null)
		{
			this._descriptionCache = new Dictionary<int, ItemDescriptionSettings.ItemDescriptionData>();
			foreach (ItemDescriptionSettings.ItemDescriptionData itemDescriptionData in this.itemDescriptions)
			{
				if (itemDescriptionData.spawnableSO != null)
				{
					this._descriptionCache[itemDescriptionData.spawnableSO.spawnableID] = itemDescriptionData;
				}
			}
		}
	}

	// Token: 0x06000803 RID: 2051 RVA: 0x000205E4 File Offset: 0x0001E7E4
	private void OnValidate()
	{
		this._descriptionCache = null;
	}

	// Token: 0x04000531 RID: 1329
	[Header("Item Descriptions")]
	[Tooltip("List of items and their descriptions.")]
	[SerializeField]
	private List<ItemDescriptionSettings.ItemDescriptionData> itemDescriptions = new List<ItemDescriptionSettings.ItemDescriptionData>();

	// Token: 0x04000532 RID: 1330
	[Header("Global Settings")]
	[Tooltip("Default description for items not in the list.")]
	[TextArea(2, 5)]
	[SerializeField]
	private string defaultDescription = "";

	// Token: 0x04000533 RID: 1331
	private Dictionary<int, ItemDescriptionSettings.ItemDescriptionData> _descriptionCache;

	// Token: 0x020000D0 RID: 208
	[Serializable]
	public class ItemDescriptionData
	{
		// Token: 0x04000534 RID: 1332
		[Tooltip("The SpawnableSO this description applies to.")]
		public SpawnableSO spawnableSO;

		// Token: 0x04000535 RID: 1333
		[Tooltip("Short description explaining how this item works.")]
		[TextArea(2, 5)]
		public string description = "";
	}
}
