using System;
using System.Collections.Generic;
using Extensions;
using UnityEngine;

// Token: 0x02000317 RID: 791
[CreateAssetMenu(menuName = "Game Settings/Game Settings", fileName = "GameSettings")]
public class GameSettings : ScriptableObject
{
	// Token: 0x06001ABA RID: 6842 RVA: 0x0007146C File Offset: 0x0006F66C
	public long GetQuota(int index, long previousQuota, long currentMoney)
	{
		if (index == 0)
		{
			return this.startingQuota;
		}
		if (index >= this.quotas.Length)
		{
			double num = (double)((float)currentMoney);
			float[] array = this.quotas;
			return (long)Math.Round(num * (double)array[array.Length - 1]);
		}
		float num2 = (float)(currentMoney - previousQuota) * this.catchUpFactor;
		return FathF.RoundByFirstNDigits((long)Math.Round((double)(((float)previousQuota + num2) * this.quotas[index])), 2);
	}

	// Token: 0x06001ABB RID: 6843 RVA: 0x000714CC File Offset: 0x0006F6CC
	public int GetQuotaExcessReward(int floor, long quota, long money)
	{
		if (quota <= 0L)
		{
			return 0;
		}
		float num = (float)((double)money / (double)quota);
		int result = 0;
		foreach (GameSettings.QuotaExcess quotaExcess in this.floorData[floor].quotaExcessRewards)
		{
			if (quotaExcess.requirement > num)
			{
				break;
			}
			result = quotaExcess.reward;
		}
		return result;
	}

	// Token: 0x06001ABC RID: 6844 RVA: 0x00071544 File Offset: 0x0006F744
	public long GetAuxiliaryMoney(int daysLeft, long quota)
	{
		int num = this.daysBeforeQuota - daysLeft;
		if (num >= this.auxiliaryMoneyPercentage.Length)
		{
			return 0L;
		}
		return (long)Math.Round((double)(this.auxiliaryMoneyPercentage[num] * (float)quota));
	}

	// Token: 0x06001ABD RID: 6845 RVA: 0x0007157C File Offset: 0x0006F77C
	public GameSettings.CasinoFloorData GetCurrentFloorData()
	{
		if (this.floorData == null || this.floorData.Count == 0)
		{
			Debug.LogWarning("GameSettings.GetCurrentFloorData: floorData is null or empty. Returning null.");
			return null;
		}
		int num = (NetworkSingleton<GameManager>.Instance != null) ? NetworkSingleton<GameManager>.Instance.currentFloor : 0;
		if (num < 0)
		{
			Debug.LogWarning(string.Format("GameSettings.GetCurrentFloorData: floor index {0} is negative. Using floor 0 instead.", num));
			num = 0;
		}
		else if (num >= this.floorData.Count)
		{
			Debug.LogWarning(string.Format("GameSettings.GetCurrentFloorData: floor index {0} is out of range (max: {1}). Using last available floor.", num, this.floorData.Count - 1));
			num = this.floorData.Count - 1;
		}
		return this.floorData[num];
	}

	// Token: 0x06001ABE RID: 6846 RVA: 0x00071634 File Offset: 0x0006F834
	public int GetTicketReward(int day)
	{
		if (this.floorData == null || this.floorData.Count == 0)
		{
			Debug.LogWarning("GameSettings.GetTicketReward: floorData is null or empty. Returning default reward of 0.");
			return 0;
		}
		if (day < 0)
		{
			Debug.LogWarning(string.Format("GameSettings.GetTicketReward: floor index {0} is negative. Using floor 0 instead.", day));
			day = 0;
		}
		return this.floorData[this.DayToFloor(day)].ticketReward;
	}

	// Token: 0x06001ABF RID: 6847 RVA: 0x00071698 File Offset: 0x0006F898
	public int DayToFloor(int day)
	{
		int result = 0;
		int num = 0;
		while (num < this.floorData.Count && this.floorData[num].requiredQuotaToAccess <= (long)day)
		{
			result = num;
			num++;
		}
		return result;
	}

	// Token: 0x14000022 RID: 34
	// (add) Token: 0x06001AC0 RID: 6848 RVA: 0x000716D8 File Offset: 0x0006F8D8
	// (remove) Token: 0x06001AC1 RID: 6849 RVA: 0x0007170C File Offset: 0x0006F90C
	public static event Action<GameSettings> SettingsChanged;

	// Token: 0x06001AC2 RID: 6850 RVA: 0x0007173F File Offset: 0x0006F93F
	public void NotifyChanged()
	{
		Action<GameSettings> settingsChanged = GameSettings.SettingsChanged;
		if (settingsChanged == null)
		{
			return;
		}
		settingsChanged(this);
	}

	// Token: 0x06001AC3 RID: 6851 RVA: 0x00071751 File Offset: 0x0006F951
	public void SetTimeScale(float newTimeScale)
	{
		this.timeScale = Mathf.Clamp(newTimeScale, 0.5f, 100f);
		Time.timeScale = this.timeScale;
		this.NotifyChanged();
	}

	// Token: 0x04001173 RID: 4467
	[Header("Game State")]
	public bool gameHasStarted;

	// Token: 0x04001174 RID: 4468
	[Header("Game Speed")]
	[Tooltip("Global time scale for the game. 1.0 = normal speed, 0.5 = half speed, 2.0 = double speed")]
	[Range(0.5f, 100f)]
	public float timeScale = 1f;

	// Token: 0x04001175 RID: 4469
	[Header("Settings")]
	[Tooltip("If false, API updates from GameSettingsAPIManager will be ignored")]
	public bool useAPIUpdates = true;

	// Token: 0x04001176 RID: 4470
	public bool apiDebugMode;

	// Token: 0x04001177 RID: 4471
	public int daysBeforeQuota = 3;

	// Token: 0x04001178 RID: 4472
	public float dayDuration = 300f;

	// Token: 0x04001179 RID: 4473
	public float bossMonologueDelay = 60f;

	// Token: 0x0400117A RID: 4474
	public int dailyTicketReward = 1;

	// Token: 0x0400117B RID: 4475
	[Range(0.5f, 1f)]
	public float catchUpFactor = 0.75f;

	// Token: 0x0400117C RID: 4476
	[Header("Starting Values")]
	public long startingQuota = 100L;

	// Token: 0x0400117D RID: 4477
	public long startingTicket = 3L;

	// Token: 0x0400117E RID: 4478
	public long startingMoney = 75L;

	// Token: 0x0400117F RID: 4479
	[Header("NPC Settings")]
	public int npcCount = 10;

	// Token: 0x04001180 RID: 4480
	public float[] auxiliaryMoneyPercentage = new float[3];

	// Token: 0x04001181 RID: 4481
	public float[] quotas = new float[12];

	// Token: 0x04001182 RID: 4482
	public List<GameSettings.CasinoFloorData> floorData = new List<GameSettings.CasinoFloorData>();

	// Token: 0x02000318 RID: 792
	[Serializable]
	public class CasinoFloorData
	{
		// Token: 0x04001184 RID: 4484
		[Min(0.1f)]
		public float estimatedValueMultiplier = 1f;

		// Token: 0x04001185 RID: 4485
		[Min(0f)]
		public long requiredQuotaToAccess = 3L;

		// Token: 0x04001186 RID: 4486
		[Min(0f)]
		public int ticketReward = 5;

		// Token: 0x04001187 RID: 4487
		[Min(1f)]
		public float shreddingEyePrice = 7f;

		// Token: 0x04001188 RID: 4488
		[Min(1f)]
		public float shreddingMouthPrice = 5f;

		// Token: 0x04001189 RID: 4489
		[Min(1f)]
		public float shreddingBodyPrice = 10f;

		// Token: 0x0400118A RID: 4490
		public List<GameSettings.QuotaExcess> quotaExcessRewards = new List<GameSettings.QuotaExcess>();

		// Token: 0x0400118B RID: 4491
		public int rerollCost = 2;
	}

	// Token: 0x02000319 RID: 793
	[Serializable]
	public class QuotaExcess
	{
		// Token: 0x0400118C RID: 4492
		public float requirement;

		// Token: 0x0400118D RID: 4493
		public int reward;
	}
}
