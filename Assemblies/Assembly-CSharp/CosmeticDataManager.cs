using System;
using System.Collections.Generic;
using UnityEngine;

// Token: 0x020000AE RID: 174
public static class CosmeticDataManager
{
	// Token: 0x060006A1 RID: 1697 RVA: 0x0001C4EC File Offset: 0x0001A6EC
	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
	private static void AutoInitialize()
	{
		CosmeticDataManager.Initialize();
	}

	// Token: 0x060006A2 RID: 1698 RVA: 0x0001C4F4 File Offset: 0x0001A6F4
	public static void Initialize()
	{
		if (CosmeticDataManager._isInitialized)
		{
			return;
		}
		CosmeticDataManager._cosmeticCache = new Dictionary<int, CosmeticData>();
		CosmeticData[] array = Resources.LoadAll<CosmeticData>("Cosmetics");
		int num = 0;
		foreach (CosmeticData cosmeticData in array)
		{
			if (cosmeticData == null)
			{
				num++;
			}
			else if (cosmeticData.cosmeticId > 0)
			{
				if (CosmeticDataManager._cosmeticCache.ContainsKey(cosmeticData.cosmeticId))
				{
					Debug.LogWarning(string.Format("[CosmeticDataManager] Duplicate cosmeticId found: {0} ({1}). Skipping duplicate.", cosmeticData.cosmeticId, cosmeticData.cosmeticName));
				}
				else
				{
					CosmeticDataManager._cosmeticCache[cosmeticData.cosmeticId] = cosmeticData;
				}
			}
		}
		if (num > 0)
		{
			Debug.LogWarning(string.Format("[CosmeticDataManager] Skipped {0} null/broken cosmetic assets during initialization.", num));
		}
		CosmeticDataManager.RebuildSortedCosmeticIds();
		CosmeticDataManager._isInitialized = true;
	}

	// Token: 0x060006A3 RID: 1699 RVA: 0x0001C5B4 File Offset: 0x0001A7B4
	public static CosmeticData GetCosmeticById(int cosmeticId)
	{
		if (!CosmeticDataManager._isInitialized)
		{
			Debug.LogWarning("[CosmeticDataManager] Not initialized. Calling Initialize() now...");
			CosmeticDataManager.Initialize();
		}
		CosmeticData result;
		if (CosmeticDataManager._cosmeticCache == null || !CosmeticDataManager._cosmeticCache.TryGetValue(cosmeticId, out result))
		{
			Debug.LogWarning(string.Format("[CosmeticDataManager] CosmeticData with ID {0} not found!", cosmeticId));
			return null;
		}
		return result;
	}

	// Token: 0x060006A4 RID: 1700 RVA: 0x0001C605 File Offset: 0x0001A805
	public static bool HasCosmetic(int cosmeticId)
	{
		if (!CosmeticDataManager._isInitialized)
		{
			CosmeticDataManager.Initialize();
		}
		return CosmeticDataManager._cosmeticCache != null && CosmeticDataManager._cosmeticCache.ContainsKey(cosmeticId);
	}

	// Token: 0x060006A5 RID: 1701 RVA: 0x0001C627 File Offset: 0x0001A827
	public static IEnumerable<CosmeticData> GetAllCosmetics()
	{
		if (!CosmeticDataManager._isInitialized)
		{
			CosmeticDataManager.Initialize();
		}
		if (CosmeticDataManager._cosmeticCache == null)
		{
			return new List<CosmeticData>();
		}
		return CosmeticDataManager._cosmeticCache.Values;
	}

	// Token: 0x060006A6 RID: 1702 RVA: 0x0001C64C File Offset: 0x0001A84C
	private static void RebuildSortedCosmeticIds()
	{
		if (CosmeticDataManager._cosmeticCache == null || CosmeticDataManager._cosmeticCache.Count == 0)
		{
			CosmeticDataManager._sortedValidCosmeticIds = Array.Empty<int>();
			return;
		}
		List<int> list = new List<int>(CosmeticDataManager._cosmeticCache.Keys);
		list.Sort();
		CosmeticDataManager._sortedValidCosmeticIds = list.ToArray();
	}

	// Token: 0x060006A7 RID: 1703 RVA: 0x0001C68B File Offset: 0x0001A88B
	public static int GetValidCosmeticCount()
	{
		if (!CosmeticDataManager._isInitialized)
		{
			CosmeticDataManager.Initialize();
		}
		if (CosmeticDataManager._cosmeticCache == null)
		{
			return 0;
		}
		return CosmeticDataManager._cosmeticCache.Count;
	}

	// Token: 0x060006A8 RID: 1704 RVA: 0x0001C6AC File Offset: 0x0001A8AC
	public static int[] GetValidCosmeticIdsSorted()
	{
		if (!CosmeticDataManager._isInitialized)
		{
			CosmeticDataManager.Initialize();
		}
		return CosmeticDataManager._sortedValidCosmeticIds ?? Array.Empty<int>();
	}

	// Token: 0x04000474 RID: 1140
	private static Dictionary<int, CosmeticData> _cosmeticCache;

	// Token: 0x04000475 RID: 1141
	private static int[] _sortedValidCosmeticIds = Array.Empty<int>();

	// Token: 0x04000476 RID: 1142
	private static bool _isInitialized = false;
}
