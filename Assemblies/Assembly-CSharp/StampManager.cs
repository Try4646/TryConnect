using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Extensions;
using Mirror;
using MoreMountains.Tools;
using UnityEngine;

// Token: 0x020001E6 RID: 486
public class StampManager : NetworkSingleton<StampManager>
{
	// Token: 0x06001158 RID: 4440 RVA: 0x0004ABC8 File Offset: 0x00048DC8
	protected override void OnAwake()
	{
		base.OnAwake();
		this.RebuildFloorLookup();
	}

	// Token: 0x06001159 RID: 4441 RVA: 0x0004ABD6 File Offset: 0x00048DD6
	protected override void OnValidate()
	{
		base.OnValidate();
		this.RebuildFloorLookup();
	}

	// Token: 0x0600115A RID: 4442 RVA: 0x0004ABE4 File Offset: 0x00048DE4
	private void RebuildFloorLookup()
	{
		this._lootTableByFloor.Clear();
		if (this.floorLootTables == null)
		{
			return;
		}
		foreach (StampManager.FloorLootTable floorLootTable in this.floorLootTables)
		{
			if (floorLootTable != null && !(floorLootTable.floor == null) && !(floorLootTable.lootTable == null))
			{
				this._lootTableByFloor[floorLootTable.floor] = floorLootTable.lootTable;
			}
		}
	}

	// Token: 0x0600115B RID: 4443 RVA: 0x0004AC7C File Offset: 0x00048E7C
	[Server]
	public IEnumerator InitializeManager()
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Collections.IEnumerator StampManager::InitializeManager()' called when server was not active");
			return null;
		}
		StampManager.<InitializeManager>d__14 <InitializeManager>d__ = new StampManager.<InitializeManager>d__14(0);
		<InitializeManager>d__.<>4__this = this;
		return <InitializeManager>d__;
	}

	// Token: 0x0600115C RID: 4444 RVA: 0x0004ACB8 File Offset: 0x00048EB8
	private string GetPositionKey(Vector3 position, CasinoFloor floor)
	{
		int num = Mathf.RoundToInt(position.x * 100f);
		int num2 = Mathf.RoundToInt(position.y * 100f);
		int num3 = Mathf.RoundToInt(position.z * 100f);
		int num4 = (floor != null) ? floor.floorIndex : -1;
		return string.Format("{0},{1},{2},{3}", new object[]
		{
			num,
			num2,
			num3,
			num4
		});
	}

	// Token: 0x0600115D RID: 4445 RVA: 0x0004AD44 File Offset: 0x00048F44
	[Server]
	private void PreAssignGamesToStamps()
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void StampManager::PreAssignGamesToStamps()' called when server was not active");
			return;
		}
		int successfulQuota = NetworkSingleton<GameManager>.Instance.successfulQuota;
		int currentSeed = NetworkSingleton<SeededRandomManager>.Instance.CurrentSeed;
		Dictionary<CasinoFloor, List<ObjectStamp>> dictionary = new Dictionary<CasinoFloor, List<ObjectStamp>>();
		foreach (ObjectStamp objectStamp in this.allStamps)
		{
			if (!(objectStamp == null) && !(objectStamp.Floor == null))
			{
				if (!dictionary.ContainsKey(objectStamp.Floor))
				{
					dictionary[objectStamp.Floor] = new List<ObjectStamp>();
				}
				dictionary[objectStamp.Floor].Add(objectStamp);
			}
		}
		foreach (KeyValuePair<CasinoFloor, List<ObjectStamp>> keyValuePair in (from kvp in dictionary
		orderby kvp.Key.floorIndex
		select kvp).ToList<KeyValuePair<CasinoFloor, List<ObjectStamp>>>())
		{
			CasinoFloor key = keyValuePair.Key;
			List<ObjectStamp> value = keyValuePair.Value;
			MMLootTableGameObjectSO lootTableForFloor = this.GetLootTableForFloor(key);
			if (lootTableForFloor == null || lootTableForFloor.LootTable == null)
			{
				Debug.LogWarning("[CasinoGame] Floor " + key.name + " has no loot table, skipping");
			}
			else
			{
				List<GameObject> list = (from x in lootTableForFloor.LootTable.ObjectsToLoot
				where x != null && x.Loot != null
				select x.Loot).ToList<GameObject>();
				if (list.Count == 0)
				{
					Debug.LogWarning("[CasinoGame] Floor " + key.name + " has no valid prefabs, skipping");
				}
				else
				{
					Random random = new Random(this.GetDeterministicHash(Vector3.zero, currentSeed, successfulQuota) * 31 + key.floorIndex);
					List<GameObject> list2 = list.ToList<GameObject>();
					for (int i = list2.Count - 1; i > 0; i--)
					{
						int index = random.Next(0, i + 1);
						GameObject value2 = list2[i];
						list2[i] = list2[index];
						list2[index] = value2;
					}
					List<ObjectStamp> list3 = (from s in value
					orderby s.transform.position.x, s.transform.position.y, s.transform.position.z
					select s).ToList<ObjectStamp>();
					for (int j = 0; j < list3.Count; j++)
					{
						Vector3 position = list3[j].transform.position;
						int index2 = j % list2.Count;
						GameObject gameObject = list2[index2];
						string positionKey = this.GetPositionKey(position, key);
						this._preAssignedGames[positionKey] = gameObject;
						if (this.logStampAssignments)
						{
							Debug.Log(string.Format("[StampManager] Floor {0} stamp at {1} -> {2} (key: {3}, index: {4})", new object[]
							{
								key.floorIndex,
								position,
								gameObject.name,
								positionKey,
								j
							}));
						}
					}
				}
			}
		}
	}

	// Token: 0x0600115E RID: 4446 RVA: 0x0004B100 File Offset: 0x00049300
	public MMLootTableGameObjectSO GetLootTableForFloor(CasinoFloor floor)
	{
		if (floor == null)
		{
			return null;
		}
		if (this._lootTableByFloor.Count == 0)
		{
			this.RebuildFloorLookup();
		}
		MMLootTableGameObjectSO result;
		this._lootTableByFloor.TryGetValue(floor, out result);
		return result;
	}

	// Token: 0x0600115F RID: 4447 RVA: 0x0004B13C File Offset: 0x0004933C
	public GameObject GetRandomPreviewPrefabForFloor(CasinoFloor floor)
	{
		MMLootTableGameObjectSO lootTableForFloor = this.GetLootTableForFloor(floor);
		bool flag;
		if (lootTableForFloor == null)
		{
			flag = (null != null);
		}
		else
		{
			MMLootTableGameObject lootTable = lootTableForFloor.LootTable;
			flag = (((lootTable != null) ? lootTable.ObjectsToLoot : null) != null);
		}
		if (!flag || lootTableForFloor.LootTable.ObjectsToLoot.Count == 0)
		{
			return null;
		}
		List<GameObject> list = (from x in lootTableForFloor.LootTable.ObjectsToLoot
		where x != null && x.Loot != null
		select x.Loot).ToList<GameObject>();
		if (list.Count == 0)
		{
			return null;
		}
		return list[NetworkSingleton<SeededRandomManager>.Instance.Range(0, list.Count)];
	}

	// Token: 0x06001160 RID: 4448 RVA: 0x0004B1FC File Offset: 0x000493FC
	[Server]
	public GameObject GetCappedLoot(MMLootTableGameObjectSO lootFilter)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'UnityEngine.GameObject StampManager::GetCappedLoot(MoreMountains.Tools.MMLootTableGameObjectSO)' called when server was not active");
			return null;
		}
		return this.GetCappedLoot(lootFilter, Vector3.zero);
	}

	// Token: 0x06001161 RID: 4449 RVA: 0x0004B238 File Offset: 0x00049438
	[Server]
	public GameObject GetCappedLoot(MMLootTableGameObjectSO lootFilter, Vector3 stampPosition)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'UnityEngine.GameObject StampManager::GetCappedLoot(MoreMountains.Tools.MMLootTableGameObjectSO,UnityEngine.Vector3)' called when server was not active");
			return null;
		}
		if (lootFilter == null)
		{
			Debug.LogError("StampManager: Loot filter is null.");
			return null;
		}
		if (lootFilter.LootTable == null || lootFilter.LootTable.ObjectsToLoot == null || lootFilter.LootTable.ObjectsToLoot.Count == 0)
		{
			Debug.LogError("StampManager: Loot table has no items.");
			return null;
		}
		List<GameObject> list = (from x in lootFilter.LootTable.ObjectsToLoot
		where x != null && x.Loot != null
		select x.Loot).ToList<GameObject>();
		if (list.Count == 0)
		{
			Debug.LogError("StampManager: No valid prefabs in loot table.");
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
		return list2[0];
	}

	// Token: 0x06001162 RID: 4450 RVA: 0x0004B394 File Offset: 0x00049594
	private int GetDeterministicHash(Vector3 position, int seed, int quotaIndex)
	{
		int num = seed * 31 + quotaIndex;
		int num2 = Mathf.RoundToInt(position.x * 100f);
		int num3 = Mathf.RoundToInt(position.y * 100f);
		int num4 = Mathf.RoundToInt(position.z * 100f);
		return ((num * 31 + num2) * 31 + num3) * 31 + num4;
	}

	// Token: 0x06001163 RID: 4451 RVA: 0x0004B3EC File Offset: 0x000495EC
	[Server]
	public GameObject GetCappedLootForFloor(CasinoFloor floor)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'UnityEngine.GameObject StampManager::GetCappedLootForFloor(CasinoFloor)' called when server was not active");
			return null;
		}
		return this.GetCappedLootForFloor(floor, Vector3.zero);
	}

	// Token: 0x06001164 RID: 4452 RVA: 0x0004B428 File Offset: 0x00049628
	[Server]
	public GameObject GetCappedLootForFloor(CasinoFloor floor, Vector3 stampPosition)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'UnityEngine.GameObject StampManager::GetCappedLootForFloor(CasinoFloor,UnityEngine.Vector3)' called when server was not active");
			return null;
		}
		if (floor == null)
		{
			Debug.LogError("[CasinoGame] StampManager: Floor is null. Cannot get loot.");
			return null;
		}
		string positionKey = this.GetPositionKey(stampPosition, floor);
		GameObject result;
		if (this._preAssignedGames.TryGetValue(positionKey, out result))
		{
			return result;
		}
		Debug.LogWarning(string.Format("[CasinoGame] NO pre-assigned game for position ({0:F2}, {1:F2}, {2:F2}) on floor {3}. Key: {4}. Total assignments: {5}. Using fallback selection.", new object[]
		{
			stampPosition.x,
			stampPosition.y,
			stampPosition.z,
			floor.name,
			positionKey,
			this._preAssignedGames.Count
		}));
		MMLootTableGameObjectSO lootTableForFloor = this.GetLootTableForFloor(floor);
		return this.GetCappedLoot(lootTableForFloor, stampPosition);
	}

	// Token: 0x06001165 RID: 4453 RVA: 0x0004B4F8 File Offset: 0x000496F8
	[Server]
	private IEnumerator InitializeAllStampsCoroutine()
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Collections.IEnumerator StampManager::InitializeAllStampsCoroutine()' called when server was not active");
			return null;
		}
		StampManager.<InitializeAllStampsCoroutine>d__24 <InitializeAllStampsCoroutine>d__ = new StampManager.<InitializeAllStampsCoroutine>d__24(0);
		<InitializeAllStampsCoroutine>d__.<>4__this = this;
		return <InitializeAllStampsCoroutine>d__;
	}

	// Token: 0x06001166 RID: 4454 RVA: 0x0004B534 File Offset: 0x00049734
	[Server]
	public void ResetStampManager()
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void StampManager::ResetStampManager()' called when server was not active");
			return;
		}
		this.allStamps.Clear();
		this._spawnCountsByTable.Clear();
		this._preAssignedGames.Clear();
		this._lootTableByFloor.Clear();
	}

	// Token: 0x06001168 RID: 4456 RVA: 0x00002321 File Offset: 0x00000521
	public override bool Weaved()
	{
		return true;
	}

	// Token: 0x04000B23 RID: 2851
	[Header("Performance Settings")]
	[SerializeField]
	private float maxFrameTime = 16f;

	// Token: 0x04000B24 RID: 2852
	[SerializeField]
	private int maxStampsPerFrame = 5;

	// Token: 0x04000B25 RID: 2853
	[Header("Floor Loot Tables")]
	[Tooltip("Assign as many floors as you want and their corresponding loot tables.")]
	[SerializeField]
	private List<StampManager.FloorLootTable> floorLootTables = new List<StampManager.FloorLootTable>();

	// Token: 0x04000B26 RID: 2854
	[Header("Loot Spawn Limits (Server)")]
	[Tooltip("How many times the same prefab can be spawned from a single loot table (MMLootTableGameObjectSO) during this scene. Set to 0 or less for unlimited.")]
	[SerializeField]
	private int defaultMaxSpawnsPerLootPrefab = 2;

	// Token: 0x04000B27 RID: 2855
	[Tooltip("How many times we'll re-roll the loot table to try to find an item that hasn't hit its cap yet.")]
	[SerializeField]
	private int maxLootRollAttempts = 25;

	// Token: 0x04000B28 RID: 2856
	[SerializeField]
	private List<ObjectStamp> allStamps = new List<ObjectStamp>();

	// Token: 0x04000B29 RID: 2857
	[Header("Debug")]
	[SerializeField]
	private bool logStampAssignments;

	// Token: 0x04000B2A RID: 2858
	private readonly Dictionary<MMLootTableGameObjectSO, Dictionary<GameObject, int>> _spawnCountsByTable = new Dictionary<MMLootTableGameObjectSO, Dictionary<GameObject, int>>();

	// Token: 0x04000B2B RID: 2859
	private readonly Dictionary<string, GameObject> _preAssignedGames = new Dictionary<string, GameObject>();

	// Token: 0x04000B2C RID: 2860
	private readonly Dictionary<CasinoFloor, MMLootTableGameObjectSO> _lootTableByFloor = new Dictionary<CasinoFloor, MMLootTableGameObjectSO>();

	// Token: 0x020001E7 RID: 487
	[Serializable]
	private class FloorLootTable
	{
		// Token: 0x04000B2D RID: 2861
		[SerializeField]
		public CasinoFloor floor;

		// Token: 0x04000B2E RID: 2862
		[SerializeField]
		public MMLootTableGameObjectSO lootTable;
	}
}
