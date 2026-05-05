using System;
using System.Collections.Generic;
using UnityEngine;

// Token: 0x0200028F RID: 655
public class BossGame : MonoBehaviour
{
	// Token: 0x06001759 RID: 5977 RVA: 0x00062DD0 File Offset: 0x00060FD0
	private void Start()
	{
		this.UpdateWinIndicators();
	}

	// Token: 0x0600175A RID: 5978 RVA: 0x00062DD8 File Offset: 0x00060FD8
	public void AddToConsecutiveWins()
	{
		this.currentConsecutiveWins++;
		if (this.currentConsecutiveWins >= this.requiredConsecutiveWins)
		{
			Debug.Log("Player has won the Boss Game!");
		}
		this.UpdateWinIndicators();
	}

	// Token: 0x0600175B RID: 5979 RVA: 0x000048A7 File Offset: 0x00002AA7
	public void ResetConsecutiveWins()
	{
	}

	// Token: 0x0600175C RID: 5980 RVA: 0x00062E08 File Offset: 0x00061008
	private void UpdateWinIndicators()
	{
		for (int i = 0; i < this.winIndicators.Count; i++)
		{
			if (i < this.currentConsecutiveWins)
			{
				this.winIndicators[i].material.color = Color.green;
			}
			else
			{
				this.winIndicators[i].material.color = Color.red;
			}
		}
	}

	// Token: 0x04000F27 RID: 3879
	public int requiredConsecutiveWins = 3;

	// Token: 0x04000F28 RID: 3880
	public int currentConsecutiveWins;

	// Token: 0x04000F29 RID: 3881
	public List<Renderer> winIndicators;
}
