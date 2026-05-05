using System;
using System.Collections.Generic;
using UnityEngine;

// Token: 0x02000126 RID: 294
public class ChallengeContext
{
	// Token: 0x170000FF RID: 255
	// (get) Token: 0x06000BDC RID: 3036 RVA: 0x0003085B File Offset: 0x0002EA5B
	public bool isWin
	{
		get
		{
			return this.payout > this.bet;
		}
	}

	// Token: 0x17000100 RID: 256
	// (get) Token: 0x06000BDD RID: 3037 RVA: 0x0003086B File Offset: 0x0002EA6B
	public bool isLoss
	{
		get
		{
			return this.payout < this.bet;
		}
	}

	// Token: 0x17000101 RID: 257
	// (get) Token: 0x06000BDE RID: 3038 RVA: 0x0003087B File Offset: 0x0002EA7B
	public long profit
	{
		get
		{
			return this.payout - this.bet;
		}
	}

	// Token: 0x06000BDF RID: 3039 RVA: 0x0003088C File Offset: 0x0002EA8C
	public T GetGameData<T>(string key, T defaultValue = default(T))
	{
		object obj;
		if (this.gameSpecificData == null || !this.gameSpecificData.TryGetValue(key, out obj))
		{
			return defaultValue;
		}
		if (obj is T)
		{
			return (T)((object)obj);
		}
		return defaultValue;
	}

	// Token: 0x06000BE0 RID: 3040 RVA: 0x000308C8 File Offset: 0x0002EAC8
	public bool HadBuff(PlayerBuffType buffType)
	{
		bool result;
		switch (buffType)
		{
		case PlayerBuffType.TipsyFortune:
			result = this.hadTipsyFortuneBuff;
			break;
		case PlayerBuffType.InspiringMelody:
			result = this.hadInspiringMelodyBuff;
			break;
		case PlayerBuffType.Immunity:
			result = this.hadImmunityBuff;
			break;
		default:
			result = false;
			break;
		}
		return result;
	}

	// Token: 0x04000767 RID: 1895
	public long bet;

	// Token: 0x04000768 RID: 1896
	public long payout;

	// Token: 0x04000769 RID: 1897
	public CasinoGameType gameType;

	// Token: 0x0400076A RID: 1898
	public Vector3 gamePosition;

	// Token: 0x0400076B RID: 1899
	public long quotaAtActivation;

	// Token: 0x0400076C RID: 1900
	public bool hadTipsyFortuneBuff;

	// Token: 0x0400076D RID: 1901
	public bool hadInspiringMelodyBuff;

	// Token: 0x0400076E RID: 1902
	public bool hadImmunityBuff;

	// Token: 0x0400076F RID: 1903
	public Dictionary<string, object> gameSpecificData = new Dictionary<string, object>();
}
