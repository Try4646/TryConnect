using System;
using System.Collections.Generic;
using UnityEngine;

// Token: 0x0200007E RID: 126
[CreateAssetMenu(fileName = "Slot Pattern", menuName = "Slot Pattern")]
public class SlotPattern : ScriptableObject
{
	// Token: 0x0600048D RID: 1165 RVA: 0x000148E4 File Offset: 0x00012AE4
	public List<int> GetPatternIndexes()
	{
		List<int> list = new List<int>();
		for (int i = 0; i < this.grid.values.Length; i++)
		{
			if (this.grid.values[i])
			{
				list.Add(i);
			}
		}
		return list;
	}

	// Token: 0x0600048E RID: 1166 RVA: 0x00014928 File Offset: 0x00012B28
	public void Debug()
	{
		foreach (int num in this.GetPatternIndexes())
		{
			UnityEngine.Debug.LogWarning(num);
		}
	}

	// Token: 0x0400031E RID: 798
	public float multiplier = 1f;

	// Token: 0x0400031F RID: 799
	public BoolGrid3x5 grid;
}
