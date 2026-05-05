using System;
using System.Collections.Generic;
using UnityEngine;

// Token: 0x02000308 RID: 776
[CreateAssetMenu(fileName = "WordBank", menuName = "Gamble With Your Friends/Word Bank", order = 0)]
public class WordBankSO : ScriptableObject
{
	// Token: 0x06001A7A RID: 6778 RVA: 0x0006FA2C File Offset: 0x0006DC2C
	public List<string> GetPool(SlotType slot)
	{
		if (this._map == null)
		{
			this._map = new Dictionary<SlotType, List<string>>();
			foreach (Pool pool in this.pools)
			{
				if (!this._map.ContainsKey(pool.slot))
				{
					this._map[pool.slot] = new List<string>();
				}
				this._map[pool.slot].AddRange(pool.items);
			}
		}
		List<string> result;
		if (!this._map.TryGetValue(slot, out result))
		{
			return WordBankSO._empty;
		}
		return result;
	}

	// Token: 0x04001127 RID: 4391
	[Header("Pools (each item counts as ONE slot no matter how many words)")]
	public List<Pool> pools = new List<Pool>();

	// Token: 0x04001128 RID: 4392
	private Dictionary<SlotType, List<string>> _map;

	// Token: 0x04001129 RID: 4393
	private static readonly List<string> _empty = new List<string>();
}
