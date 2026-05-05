using System;
using System.Collections.Generic;
using UnityEngine;

// Token: 0x020000D6 RID: 214
[CreateAssetMenu(menuName = "Game Settings/Item Price Settings", fileName = "ItemPriceSettings")]
public class ItemPriceSettings : ScriptableObject
{
	// Token: 0x06000862 RID: 2146 RVA: 0x00021DD8 File Offset: 0x0001FFD8
	public int GetBasePrice(SpawnableSO spawnableSO)
	{
		if (spawnableSO == null)
		{
			return this.defaultBasePrice;
		}
		this.BuildCacheIfNeeded();
		ItemPriceSettings.ItemPriceData itemPriceData;
		if (this._priceCache.TryGetValue(spawnableSO.spawnableID, out itemPriceData))
		{
			return itemPriceData.basePrice;
		}
		return this.defaultBasePrice;
	}

	// Token: 0x06000863 RID: 2147 RVA: 0x00021E20 File Offset: 0x00020020
	public int GetPriceIncreasePerFloor(SpawnableSO spawnableSO)
	{
		if (spawnableSO == null)
		{
			return this.defaultPriceIncreasePerFloor;
		}
		this.BuildCacheIfNeeded();
		ItemPriceSettings.ItemPriceData itemPriceData;
		if (this._priceCache.TryGetValue(spawnableSO.spawnableID, out itemPriceData))
		{
			return itemPriceData.priceIncreasePerFloor;
		}
		return this.defaultPriceIncreasePerFloor;
	}

	// Token: 0x06000864 RID: 2148 RVA: 0x00021E68 File Offset: 0x00020068
	public int CalculatePrice(SpawnableSO spawnableSO, int floorIndex)
	{
		if (spawnableSO == null)
		{
			return this.defaultBasePrice;
		}
		int basePrice = this.GetBasePrice(spawnableSO);
		int priceIncreasePerFloor = this.GetPriceIncreasePerFloor(spawnableSO);
		return basePrice + floorIndex * priceIncreasePerFloor;
	}

	// Token: 0x06000865 RID: 2149 RVA: 0x00021E98 File Offset: 0x00020098
	public int CalculateCosmeticPrice(CosmeticRarity rarity, int floorIndex)
	{
		int num = this.defaultBasePrice + floorIndex * this.defaultPriceIncreasePerFloor;
		int[] array = (this.cosmeticRarityTicketAdd != null && this.cosmeticRarityTicketAdd.Length != 0) ? this.cosmeticRarityTicketAdd : ItemPriceSettings.DefaultCosmeticRarityTicketAdd;
		if (rarity >= CosmeticRarity.Common && rarity < (CosmeticRarity)array.Length)
		{
			return num + array[(int)rarity];
		}
		return num;
	}

	// Token: 0x06000866 RID: 2150 RVA: 0x00021EE8 File Offset: 0x000200E8
	private void BuildCacheIfNeeded()
	{
		if (this._priceCache == null)
		{
			this._priceCache = new Dictionary<int, ItemPriceSettings.ItemPriceData>();
			foreach (ItemPriceSettings.ItemPriceData itemPriceData in this.itemPrices)
			{
				if (itemPriceData.spawnableSO != null)
				{
					this._priceCache[itemPriceData.spawnableSO.spawnableID] = itemPriceData;
				}
			}
		}
	}

	// Token: 0x06000867 RID: 2151 RVA: 0x00021F6C File Offset: 0x0002016C
	private void OnValidate()
	{
		this._priceCache = null;
	}

	// Token: 0x0400055C RID: 1372
	[Header("Item Prices")]
	[Tooltip("List of items and their base prices.")]
	[SerializeField]
	private List<ItemPriceSettings.ItemPriceData> itemPrices = new List<ItemPriceSettings.ItemPriceData>();

	// Token: 0x0400055D RID: 1373
	[Header("Global Settings")]
	[Tooltip("Default base price for items not in the list.")]
	[Min(0f)]
	[SerializeField]
	private int defaultBasePrice = 2;

	// Token: 0x0400055E RID: 1374
	[Tooltip("Default price increase per floor for items not in the list.")]
	[Min(0f)]
	[SerializeField]
	private int defaultPriceIncreasePerFloor = 1;

	// Token: 0x0400055F RID: 1375
	[Header("Cosmetics (rarity)")]
	[Tooltip("Extra tickets on top of the default floor-scaled base. Order: Common, Uncommon, Rare, Epic, Legendary.")]
	[SerializeField]
	private int[] cosmeticRarityTicketAdd = new int[]
	{
		0,
		1,
		3,
		6,
		10
	};

	// Token: 0x04000560 RID: 1376
	private static readonly int[] DefaultCosmeticRarityTicketAdd = new int[]
	{
		0,
		1,
		3,
		6,
		10
	};

	// Token: 0x04000561 RID: 1377
	private Dictionary<int, ItemPriceSettings.ItemPriceData> _priceCache;

	// Token: 0x020000D7 RID: 215
	[Serializable]
	public class ItemPriceData
	{
		// Token: 0x04000562 RID: 1378
		[Tooltip("The SpawnableSO this price applies to.")]
		public SpawnableSO spawnableSO;

		// Token: 0x04000563 RID: 1379
		[Tooltip("Base price in tickets for this item.")]
		[Min(0f)]
		public int basePrice = 2;

		// Token: 0x04000564 RID: 1380
		[Tooltip("Price increase per floor. Final price = basePrice + (floorIndex * priceIncreasePerFloor)")]
		[Min(0f)]
		public int priceIncreasePerFloor = 1;
	}
}
