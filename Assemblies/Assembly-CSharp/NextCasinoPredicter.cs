using System;
using System.Collections.Generic;
using System.Linq;
using Extensions;
using MoreMountains.Tools;
using UnityEngine;

// Token: 0x0200018C RID: 396
public class NextCasinoPredicter
{
	// Token: 0x06000ED5 RID: 3797 RVA: 0x0003D740 File Offset: 0x0003B940
	public static void PredictFloorGames(MMLootTableGameObjectSO lootTable, int floorIndex, int predictionCount)
	{
		if (NetworkSingleton<GameManager>.Instance == null || NetworkSingleton<SeededRandomManager>.Instance == null)
		{
			Debug.LogWarning("[NextCasinoPredicter] Required managers not available");
			return;
		}
		if (predictionCount <= 0)
		{
			return;
		}
		if (lootTable == null)
		{
			Debug.LogWarning(string.Format("[NextCasinoPredicter] Loot table is null for floor {0}", floorIndex));
			return;
		}
		if (lootTable.LootTable == null)
		{
			Debug.LogWarning(string.Format("[NextCasinoPredicter] Floor {0} loot table has no LootTable data", floorIndex));
			return;
		}
		int successfulQuota = NetworkSingleton<GameManager>.Instance.successfulQuota;
		int currentSeed = NetworkSingleton<SeededRandomManager>.Instance.CurrentSeed;
		List<GameObject> list = (from x in lootTable.LootTable.ObjectsToLoot
		where x != null && x.Loot != null
		select x.Loot).ToList<GameObject>();
		if (list.Count == 0)
		{
			Debug.LogWarning(string.Format("[NextCasinoPredicter] Floor {0} has no valid prefabs", floorIndex));
			return;
		}
		Random random = new Random(NextCasinoPredicter.GetDeterministicHash(Vector3.zero, currentSeed, successfulQuota) * 31 + floorIndex);
		List<GameObject> list2 = list.ToList<GameObject>();
		for (int i = list2.Count - 1; i > 0; i--)
		{
			int index = random.Next(0, i + 1);
			GameObject value = list2[i];
			list2[i] = list2[index];
			list2[index] = value;
		}
		List<ValueTuple<int, string>> list3 = new List<ValueTuple<int, string>>();
		HashSet<CasinoGameType> hashSet = new HashSet<CasinoGameType>();
		NextCasinoPredicter._cachedAvailableGameTypes[floorIndex] = hashSet;
		for (int j = 0; j < predictionCount; j++)
		{
			int index2 = j % list2.Count;
			GameObject gameObject = list2[index2];
			list3.Add(new ValueTuple<int, string>(j, gameObject.name));
			CasinoGameType? casinoGameType = NextCasinoPredicter.ExtractGameTypeFromPrefabName(gameObject.name);
			if (casinoGameType != null)
			{
				hashSet.Add(casinoGameType.Value);
			}
		}
		for (int k = 0; k < list3.Count; k++)
		{
			ValueTuple<int, string> valueTuple = list3[k];
		}
	}

	// Token: 0x06000ED6 RID: 3798 RVA: 0x0003D94C File Offset: 0x0003BB4C
	public static void PredictFloorGames(int floorIndex, int predictionCount)
	{
		string text = string.Format("FloorLootTables/Floor {0}", floorIndex);
		MMLootTableGameObjectSO mmlootTableGameObjectSO = Resources.Load<MMLootTableGameObjectSO>(text);
		if (mmlootTableGameObjectSO == null)
		{
			Debug.LogWarning(string.Format("[NextCasinoPredicter] Could not load loot table for floor {0} (tried path: {1})", floorIndex, text));
			return;
		}
		NextCasinoPredicter.PredictFloorGames(mmlootTableGameObjectSO, floorIndex, predictionCount);
	}

	// Token: 0x06000ED7 RID: 3799 RVA: 0x0003D99C File Offset: 0x0003BB9C
	public static HashSet<CasinoGameType> GetAvailableGameTypesForFloor(int floorIndex)
	{
		HashSet<CasinoGameType> hashSet;
		if (NextCasinoPredicter._cachedAvailableGameTypes.TryGetValue(floorIndex, out hashSet) && hashSet != null && hashSet.Count > 0)
		{
			return new HashSet<CasinoGameType>(hashSet);
		}
		HashSet<CasinoGameType> hashSet2 = new HashSet<CasinoGameType>();
		string text = string.Format("FloorLootTables/Floor {0}", floorIndex);
		MMLootTableGameObjectSO mmlootTableGameObjectSO = Resources.Load<MMLootTableGameObjectSO>(text);
		if (mmlootTableGameObjectSO == null || mmlootTableGameObjectSO.LootTable == null || mmlootTableGameObjectSO.LootTable.ObjectsToLoot == null)
		{
			Debug.LogWarning(string.Format("[NextCasinoPredicter] Could not load loot table for floor {0} to compute available game types (path: {1})", floorIndex, text));
			return hashSet2;
		}
		foreach (GameObject gameObject in (from x in mmlootTableGameObjectSO.LootTable.ObjectsToLoot
		where x != null && x.Loot != null
		select x.Loot).ToList<GameObject>())
		{
			if (!(gameObject == null))
			{
				GameBase componentInChildren = gameObject.GetComponentInChildren<GameBase>(true);
				if (componentInChildren != null)
				{
					hashSet2.Add(componentInChildren.GameType);
				}
			}
		}
		return hashSet2;
	}

	// Token: 0x06000ED8 RID: 3800 RVA: 0x0003DAE0 File Offset: 0x0003BCE0
	private static CasinoGameType? ExtractGameTypeFromPrefabName(string prefabName)
	{
		if (string.IsNullOrEmpty(prefabName))
		{
			return null;
		}
		string text = prefabName;
		if (text.EndsWith("_Cluster"))
		{
			text = text.Substring(0, text.Length - "_Cluster".Length);
		}
		CasinoGameType value;
		if (Enum.TryParse<CasinoGameType>(text, true, out value))
		{
			return new CasinoGameType?(value);
		}
		if (text.Equals("SlotMachine", StringComparison.OrdinalIgnoreCase) || text.Equals("Slots", StringComparison.OrdinalIgnoreCase))
		{
			return new CasinoGameType?(CasinoGameType.SlotMachine);
		}
		if (text.Equals("WheelOfFortune", StringComparison.OrdinalIgnoreCase) || text.Equals("Wheel of Fortune", StringComparison.OrdinalIgnoreCase))
		{
			return new CasinoGameType?(CasinoGameType.WheelOfFortune);
		}
		if (text.Equals("DuckRace", StringComparison.OrdinalIgnoreCase) || text.Equals("Duck Race", StringComparison.OrdinalIgnoreCase))
		{
			return new CasinoGameType?(CasinoGameType.DuckRace);
		}
		if (text.Equals("CrossyRoad", StringComparison.OrdinalIgnoreCase) || text.Equals("Crossy Road", StringComparison.OrdinalIgnoreCase))
		{
			return new CasinoGameType?(CasinoGameType.CrossyRoad);
		}
		if (text.Equals("DragonTower", StringComparison.OrdinalIgnoreCase) || text.Equals("Dragon Tower", StringComparison.OrdinalIgnoreCase))
		{
			return new CasinoGameType?(CasinoGameType.DragonTower);
		}
		if (text.Equals("Minesweeper", StringComparison.OrdinalIgnoreCase) || text.Equals("MineSweeper", StringComparison.OrdinalIgnoreCase) || text.Equals("Mine Sweeper", StringComparison.OrdinalIgnoreCase))
		{
			return new CasinoGameType?(CasinoGameType.Minesweeper);
		}
		if (text.Equals("MoneyWheel", StringComparison.OrdinalIgnoreCase) || text.Equals("Money Wheel", StringComparison.OrdinalIgnoreCase))
		{
			return new CasinoGameType?(CasinoGameType.MoneyWheel);
		}
		Debug.LogWarning(string.Concat(new string[]
		{
			"[NextCasinoPredicter] Could not extract game type from prefab name: '",
			prefabName,
			"' (cleaned: '",
			text,
			"')"
		}));
		return null;
	}

	// Token: 0x06000ED9 RID: 3801 RVA: 0x0003DC78 File Offset: 0x0003BE78
	private static int GetDeterministicHash(Vector3 position, int seed, int quotaIndex)
	{
		int num = seed * 31 + quotaIndex;
		int num2 = Mathf.RoundToInt(position.x * 100f);
		int num3 = Mathf.RoundToInt(position.y * 100f);
		int num4 = Mathf.RoundToInt(position.z * 100f);
		return ((num * 31 + num2) * 31 + num3) * 31 + num4;
	}

	// Token: 0x04000978 RID: 2424
	private static readonly Dictionary<int, HashSet<CasinoGameType>> _cachedAvailableGameTypes = new Dictionary<int, HashSet<CasinoGameType>>();
}
