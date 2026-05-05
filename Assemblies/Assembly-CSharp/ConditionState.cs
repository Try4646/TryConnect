using System;
using System.Collections.Generic;
using UnityEngine;

// Token: 0x02000134 RID: 308
[Serializable]
public class ConditionState
{
	// Token: 0x06000C39 RID: 3129 RVA: 0x000329E1 File Offset: 0x00030BE1
	public ConditionState()
	{
		this.startTime = Time.time;
		this.lastGameTime = Time.time;
	}

	// Token: 0x06000C3A RID: 3130 RVA: 0x00032A0C File Offset: 0x00030C0C
	public void Reset()
	{
		this.currentWinCount = 0;
		this.consecutiveWinCount = 0;
		this.currentLossCount = 0;
		this.consecutiveLossCount = 0;
		this.totalBetAmount = 0L;
		this.totalPayoutAmount = 0L;
		this.totalProfit = 0L;
		this.startTime = Time.time;
		this.lastGameTime = Time.time;
		this.customData.Clear();
	}

	// Token: 0x06000C3B RID: 3131 RVA: 0x00032A70 File Offset: 0x00030C70
	public T GetCustom<T>(string key, T defaultValue = default(T))
	{
		object obj;
		if (this.customData == null || !this.customData.TryGetValue(key, out obj))
		{
			return defaultValue;
		}
		if (obj is T)
		{
			return (T)((object)obj);
		}
		return defaultValue;
	}

	// Token: 0x06000C3C RID: 3132 RVA: 0x00032AA9 File Offset: 0x00030CA9
	public void SetCustom<T>(string key, T value)
	{
		if (this.customData == null)
		{
			this.customData = new Dictionary<string, object>();
		}
		this.customData[key] = value;
	}

	// Token: 0x06000C3D RID: 3133 RVA: 0x00032AD0 File Offset: 0x00030CD0
	public int GetCustomInt(string key, int defaultValue = 0)
	{
		return this.GetCustom<int>(key, defaultValue);
	}

	// Token: 0x06000C3E RID: 3134 RVA: 0x00032ADA File Offset: 0x00030CDA
	public void SetCustomInt(string key, int value)
	{
		this.SetCustom<int>(key, value);
	}

	// Token: 0x040007B2 RID: 1970
	public int currentWinCount;

	// Token: 0x040007B3 RID: 1971
	public int consecutiveWinCount;

	// Token: 0x040007B4 RID: 1972
	public int currentLossCount;

	// Token: 0x040007B5 RID: 1973
	public int consecutiveLossCount;

	// Token: 0x040007B6 RID: 1974
	public long totalBetAmount;

	// Token: 0x040007B7 RID: 1975
	public long totalPayoutAmount;

	// Token: 0x040007B8 RID: 1976
	public long totalProfit;

	// Token: 0x040007B9 RID: 1977
	public float startTime;

	// Token: 0x040007BA RID: 1978
	public float lastGameTime;

	// Token: 0x040007BB RID: 1979
	private Dictionary<string, object> customData = new Dictionary<string, object>();
}
