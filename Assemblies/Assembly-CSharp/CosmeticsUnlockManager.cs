using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Extensions;
using UnityEngine;

// Token: 0x020000B7 RID: 183
public class CosmeticsUnlockManager : MonoSingleton<CosmeticsUnlockManager>
{
	// Token: 0x14000003 RID: 3
	// (add) Token: 0x060006F0 RID: 1776 RVA: 0x0001D73C File Offset: 0x0001B93C
	// (remove) Token: 0x060006F1 RID: 1777 RVA: 0x0001D770 File Offset: 0x0001B970
	public static event Action<int> OnCosmeticUnlocked;

	// Token: 0x14000004 RID: 4
	// (add) Token: 0x060006F2 RID: 1778 RVA: 0x0001D7A4 File Offset: 0x0001B9A4
	// (remove) Token: 0x060006F3 RID: 1779 RVA: 0x0001D7D8 File Offset: 0x0001B9D8
	public static event Action OnUnlocksLoaded;

	// Token: 0x060006F4 RID: 1780 RVA: 0x0001D80B File Offset: 0x0001BA0B
	protected override void OnAwake()
	{
		this.LoadFromFile();
		this.EnsureDefaultClothingUnlocked(false);
		if (this.debugMode)
		{
			Debug.Log(string.Format("[CosmeticsUnlockManager] Initialized with {0} unlocked cosmetics", this.unlockedCosmetics.Count));
		}
	}

	// Token: 0x060006F5 RID: 1781 RVA: 0x0001D844 File Offset: 0x0001BA44
	public bool UnlockCosmetic(int cosmeticId)
	{
		if (this.unlockedCosmetics.Contains(cosmeticId))
		{
			if (this.debugMode)
			{
				Debug.Log(string.Format("[CosmeticsUnlockManager] Cosmetic {0} already unlocked", cosmeticId));
			}
			return false;
		}
		this.unlockedCosmetics.Add(cosmeticId);
		this.SaveToFile();
		Action<int> onCosmeticUnlocked = CosmeticsUnlockManager.OnCosmeticUnlocked;
		if (onCosmeticUnlocked != null)
		{
			onCosmeticUnlocked(cosmeticId);
		}
		if (this.debugMode)
		{
			Debug.Log(string.Format("[CosmeticsUnlockManager] Unlocked cosmetic {0}", cosmeticId));
		}
		return true;
	}

	// Token: 0x060006F6 RID: 1782 RVA: 0x0001D8C0 File Offset: 0x0001BAC0
	public int UnlockRandomCosmetic()
	{
		this.EnsureDefaultClothingUnlocked(true);
		CosmeticData[] array = (from c in Resources.LoadAll<CosmeticData>("Cosmetics")
		where !this.unlockedCosmetics.Contains(c.cosmeticId)
		select c).ToArray<CosmeticData>();
		if (array.Length == 0)
		{
			if (this.debugMode)
			{
				Debug.Log("[CosmeticsUnlockManager] All cosmetics unlocked");
			}
			return -1;
		}
		CosmeticData cosmeticData = array[Random.Range(0, array.Length)];
		this.UnlockCosmetic(cosmeticData.cosmeticId);
		return cosmeticData.cosmeticId;
	}

	// Token: 0x060006F7 RID: 1783 RVA: 0x0001D92C File Offset: 0x0001BB2C
	public bool IsCosmeticUnlocked(int cosmeticId)
	{
		return this.unlockedCosmetics.Contains(cosmeticId);
	}

	// Token: 0x060006F8 RID: 1784 RVA: 0x0001D93A File Offset: 0x0001BB3A
	public int[] GetUnlockedCosmetics()
	{
		return this.unlockedCosmetics.ToArray<int>();
	}

	// Token: 0x060006F9 RID: 1785 RVA: 0x0001D947 File Offset: 0x0001BB47
	public int GetUnlockedCount()
	{
		return this.unlockedCosmetics.Count;
	}

	// Token: 0x060006FA RID: 1786 RVA: 0x0001D954 File Offset: 0x0001BB54
	public int GetTotalCosmeticsCount()
	{
		return CosmeticDataManager.GetValidCosmeticCount();
	}

	// Token: 0x060006FB RID: 1787 RVA: 0x0001D95B File Offset: 0x0001BB5B
	public int GetDefaultClothingCosmeticId()
	{
		if (!(this.defaultClothingCosmetic != null))
		{
			return -1;
		}
		return this.defaultClothingCosmetic.cosmeticId;
	}

	// Token: 0x060006FC RID: 1788 RVA: 0x0001D978 File Offset: 0x0001BB78
	public void ResetAllUnlocks()
	{
		this.unlockedCosmetics.Clear();
		this.EnsureDefaultClothingUnlocked(true);
		this.SaveToFile();
		if (this.debugMode)
		{
			Debug.Log("[CosmeticsUnlockManager] All unlocks reset");
		}
	}

	// Token: 0x060006FD RID: 1789 RVA: 0x0001D9A4 File Offset: 0x0001BBA4
	public Dictionary<CosmeticType, int> GetEquippedCosmetics()
	{
		return new Dictionary<CosmeticType, int>(this.equippedCosmetics);
	}

	// Token: 0x060006FE RID: 1790 RVA: 0x0001D9B1 File Offset: 0x0001BBB1
	public void SetEquippedCosmetics(Dictionary<CosmeticType, int> equipped, bool skipSave = false)
	{
		this.equippedCosmetics = new Dictionary<CosmeticType, int>(equipped);
		if (!skipSave)
		{
			this.SaveToFile();
		}
	}

	// Token: 0x060006FF RID: 1791 RVA: 0x0001D9C8 File Offset: 0x0001BBC8
	public void SetPlayerColor(Color color)
	{
		this.savedPlayerColor = new Color?(color);
		this.SaveToFile();
		if (this.debugMode)
		{
			Debug.Log(string.Format("[CosmeticsUnlockManager] Saved player color: {0}", color));
		}
	}

	// Token: 0x06000700 RID: 1792 RVA: 0x0001D9F9 File Offset: 0x0001BBF9
	public Color? GetPlayerColor()
	{
		return this.savedPlayerColor;
	}

	// Token: 0x06000701 RID: 1793 RVA: 0x0001DA04 File Offset: 0x0001BC04
	public void LoadFromFile()
	{
		try
		{
			string saveFilePath = this.GetSaveFilePath();
			if (!File.Exists(saveFilePath))
			{
				if (this.debugMode)
				{
					Debug.Log("[CosmeticsUnlockManager] No save file found");
				}
			}
			else
			{
				CosmeticsUnlockManager.CosmeticsSaveData cosmeticsSaveData = JsonUtility.FromJson<CosmeticsUnlockManager.CosmeticsSaveData>(File.ReadAllText(saveFilePath));
				if (cosmeticsSaveData == null)
				{
					if (this.debugMode)
					{
						Debug.LogWarning("[CosmeticsUnlockManager] Failed to parse save file");
					}
				}
				else
				{
					if (cosmeticsSaveData.unlockedCosmetics != null)
					{
						this.unlockedCosmetics = new HashSet<int>(cosmeticsSaveData.unlockedCosmetics);
					}
					this.equippedCosmetics.Clear();
					if (cosmeticsSaveData.equippedCosmetics != null)
					{
						foreach (CosmeticsUnlockManager.EquippedCosmeticData equippedCosmeticData in cosmeticsSaveData.equippedCosmetics)
						{
							this.equippedCosmetics[equippedCosmeticData.cosmeticType] = equippedCosmeticData.cosmeticId;
						}
					}
					this.savedPlayerColor = ((!string.IsNullOrEmpty(cosmeticsSaveData.playerColorHex)) ? new Color?(ColorHexUtility.HexToColor(cosmeticsSaveData.playerColorHex)) : null);
					if (this.debugMode)
					{
						Debug.Log(string.Format("[CosmeticsUnlockManager] Loaded {0} unlocks, ", this.unlockedCosmetics.Count) + string.Format("{0} equipped cosmetics", this.equippedCosmetics.Count));
					}
					Action onUnlocksLoaded = CosmeticsUnlockManager.OnUnlocksLoaded;
					if (onUnlocksLoaded != null)
					{
						onUnlocksLoaded();
					}
					this.EnsureDefaultClothingUnlocked(false);
				}
			}
		}
		catch (Exception ex)
		{
			Debug.LogError("[CosmeticsUnlockManager] Failed to load save file: " + ex.Message);
			this.ResetToDefaults();
			this.EnsureDefaultClothingUnlocked(false);
		}
	}

	// Token: 0x06000702 RID: 1794 RVA: 0x0001DB90 File Offset: 0x0001BD90
	private void SaveToFile()
	{
		try
		{
			CosmeticsUnlockManager.CosmeticsSaveData cosmeticsSaveData = new CosmeticsUnlockManager.CosmeticsSaveData();
			cosmeticsSaveData.unlockedCosmetics = this.unlockedCosmetics.ToArray<int>();
			cosmeticsSaveData.equippedCosmetics = (from kvp in this.equippedCosmetics
			select new CosmeticsUnlockManager.EquippedCosmeticData
			{
				cosmeticType = kvp.Key,
				cosmeticId = kvp.Value
			}).ToArray<CosmeticsUnlockManager.EquippedCosmeticData>();
			cosmeticsSaveData.playerColorHex = ((this.savedPlayerColor != null) ? ColorHexUtility.ColorToHex(this.savedPlayerColor.Value) : "");
			cosmeticsSaveData.timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
			cosmeticsSaveData.version = "1.0";
			string contents = JsonUtility.ToJson(cosmeticsSaveData, true);
			File.WriteAllText(this.GetSaveFilePath(), contents);
			if (this.debugMode)
			{
				Debug.Log("[CosmeticsUnlockManager] Saved to " + this.GetSaveFilePath());
			}
		}
		catch (Exception ex)
		{
			Debug.LogError("[CosmeticsUnlockManager] Failed to save: " + ex.Message);
		}
	}

	// Token: 0x06000703 RID: 1795 RVA: 0x0001DC8C File Offset: 0x0001BE8C
	private void ResetToDefaults()
	{
		this.unlockedCosmetics = new HashSet<int>();
		this.equippedCosmetics.Clear();
		this.savedPlayerColor = null;
	}

	// Token: 0x06000704 RID: 1796 RVA: 0x0001DCB0 File Offset: 0x0001BEB0
	private void EnsureDefaultClothingUnlocked(bool skipSave)
	{
		int defaultClothingCosmeticId = this.GetDefaultClothingCosmeticId();
		if (defaultClothingCosmeticId <= 0)
		{
			return;
		}
		if (this.unlockedCosmetics.Contains(defaultClothingCosmeticId))
		{
			return;
		}
		this.unlockedCosmetics.Add(defaultClothingCosmeticId);
		if (!skipSave)
		{
			this.SaveToFile();
		}
	}

	// Token: 0x06000705 RID: 1797 RVA: 0x0001DCEE File Offset: 0x0001BEEE
	private string GetSaveFilePath()
	{
		return Path.Combine(Application.persistentDataPath, this.saveFileName);
	}

	// Token: 0x040004A3 RID: 1187
	[Header("Settings")]
	[SerializeField]
	private string saveFileName = "cosmetics_unlocks.json";

	// Token: 0x040004A4 RID: 1188
	[Header("Defaults")]
	[SerializeField]
	private CosmeticData defaultClothingCosmetic;

	// Token: 0x040004A5 RID: 1189
	[Header("Debug")]
	[SerializeField]
	private bool debugMode = true;

	// Token: 0x040004A6 RID: 1190
	private HashSet<int> unlockedCosmetics = new HashSet<int>();

	// Token: 0x040004A7 RID: 1191
	private Dictionary<CosmeticType, int> equippedCosmetics = new Dictionary<CosmeticType, int>();

	// Token: 0x040004A8 RID: 1192
	private Color? savedPlayerColor;

	// Token: 0x020000B8 RID: 184
	[Serializable]
	private class EquippedCosmeticData
	{
		// Token: 0x040004AB RID: 1195
		public CosmeticType cosmeticType;

		// Token: 0x040004AC RID: 1196
		public int cosmeticId;
	}

	// Token: 0x020000B9 RID: 185
	[Serializable]
	private class CosmeticsSaveData
	{
		// Token: 0x040004AD RID: 1197
		public int[] unlockedCosmetics;

		// Token: 0x040004AE RID: 1198
		public CosmeticsUnlockManager.EquippedCosmeticData[] equippedCosmetics;

		// Token: 0x040004AF RID: 1199
		public string playerColorHex = "";

		// Token: 0x040004B0 RID: 1200
		public long timestamp;

		// Token: 0x040004B1 RID: 1201
		public string version = "1.0";
	}
}
