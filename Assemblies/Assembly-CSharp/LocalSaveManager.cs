using System;
using System.Collections.Generic;
using System.IO;
using Extensions;
using UnityEngine;

// Token: 0x020001A5 RID: 421
public class LocalSaveManager : MonoSingleton<LocalSaveManager>
{
	// Token: 0x1700015B RID: 347
	// (get) Token: 0x06000F83 RID: 3971 RVA: 0x00041914 File Offset: 0x0003FB14
	private string SaveDirectoryPath
	{
		get
		{
			return Path.Combine(Application.persistentDataPath, "Saves");
		}
	}

	// Token: 0x1700015C RID: 348
	// (get) Token: 0x06000F84 RID: 3972 RVA: 0x00041925 File Offset: 0x0003FB25
	// (set) Token: 0x06000F85 RID: 3973 RVA: 0x0004192D File Offset: 0x0003FB2D
	public string SelectedSaveName { get; private set; }

	// Token: 0x06000F86 RID: 3974 RVA: 0x00041936 File Offset: 0x0003FB36
	protected override void OnAwake()
	{
		base.OnAwake();
		if (!Directory.Exists(this.SaveDirectoryPath))
		{
			Directory.CreateDirectory(this.SaveDirectoryPath);
		}
		this.SelectedSaveName = PlayerPrefs.GetString("SelectedSaveName", "");
	}

	// Token: 0x06000F87 RID: 3975 RVA: 0x0004196C File Offset: 0x0003FB6C
	public List<string> GetAvailableSaves()
	{
		List<string> list = new List<string>();
		if (!Directory.Exists(this.SaveDirectoryPath))
		{
			return list;
		}
		foreach (string path in Directory.GetFiles(this.SaveDirectoryPath, "*.json"))
		{
			list.Add(Path.GetFileNameWithoutExtension(path));
		}
		return list;
	}

	// Token: 0x06000F88 RID: 3976 RVA: 0x000419C0 File Offset: 0x0003FBC0
	public void CreateNewSave(string saveName)
	{
		if (string.IsNullOrEmpty(saveName))
		{
			Debug.LogError("[LocalSaveManager] Cannot create save with empty name");
			return;
		}
		this.SelectedSaveName = saveName;
		PlayerPrefs.SetString("SelectedSaveName", saveName);
		PlayerPrefs.Save();
		GameSettings gameSettings = Resources.Load<GameSettings>("GameSettings");
		SaveData obj = new SaveData
		{
			saveName = saveName,
			saveTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
			successfulQuota = 0,
			daysLeft = gameSettings.daysBeforeQuota,
			daysPassed = 0,
			currentQuota = gameSettings.startingQuota,
			currentFloor = 0,
			requiredQuotaToNextFloor = gameSettings.floorData[1].requiredQuotaToAccess,
			money = gameSettings.startingMoney,
			tickets = gameSettings.startingTicket,
			seed = Random.Range(int.MinValue, int.MaxValue)
		};
		string path = Path.Combine(this.SaveDirectoryPath, saveName + ".json");
		try
		{
			string contents = JsonUtility.ToJson(obj, true);
			File.WriteAllText(path, contents);
			PlayerPrefs.SetString("SelectedSaveData", JsonUtility.ToJson(obj));
			PlayerPrefs.Save();
		}
		catch (Exception ex)
		{
			Debug.LogError("[LocalSaveManager] Failed to create save: " + ex.Message);
		}
	}

	// Token: 0x06000F89 RID: 3977 RVA: 0x00041AFC File Offset: 0x0003FCFC
	public void SelectSave(string saveName)
	{
		if (string.IsNullOrEmpty(saveName))
		{
			Debug.LogError("[LocalSaveManager] Cannot select save with empty name");
			return;
		}
		string text = Path.Combine(this.SaveDirectoryPath, saveName + ".json");
		if (!File.Exists(text))
		{
			Debug.LogError("[LocalSaveManager] Save file not found: " + text);
			return;
		}
		this.SelectedSaveName = saveName;
		PlayerPrefs.SetString("SelectedSaveName", saveName);
		SaveData saveData = this.LoadSaveData(saveName);
		if (saveData != null)
		{
			string value = JsonUtility.ToJson(saveData);
			PlayerPrefs.SetString("SelectedSaveData", value);
		}
		PlayerPrefs.Save();
		Debug.Log("[LocalSaveManager] Active save: " + saveName);
	}

	// Token: 0x06000F8A RID: 3978 RVA: 0x00041B90 File Offset: 0x0003FD90
	public SaveData LoadSaveData(string saveName)
	{
		if (string.IsNullOrEmpty(saveName))
		{
			Debug.LogError("[LocalSaveManager] Cannot load save with empty name");
			return null;
		}
		string text = Path.Combine(this.SaveDirectoryPath, saveName + ".json");
		if (!File.Exists(text))
		{
			Debug.LogError("[LocalSaveManager] Save file not found: " + text);
			return null;
		}
		SaveData result;
		try
		{
			result = JsonUtility.FromJson<SaveData>(File.ReadAllText(text));
		}
		catch (Exception ex)
		{
			Debug.LogError("[LocalSaveManager] Failed to load save: " + ex.Message);
			result = null;
		}
		return result;
	}

	// Token: 0x06000F8B RID: 3979 RVA: 0x00041C1C File Offset: 0x0003FE1C
	public void SaveGameData(SaveData saveData)
	{
		if (saveData == null || string.IsNullOrEmpty(saveData.saveName))
		{
			Debug.LogWarning("[LocalSaveManager] No save data to save");
			return;
		}
		string path = Path.Combine(this.SaveDirectoryPath, saveData.saveName + ".json");
		try
		{
			saveData.saveTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
			string contents = JsonUtility.ToJson(saveData, true);
			File.WriteAllText(path, contents);
			Debug.Log("[LocalSaveManager] Saved game to: " + saveData.saveName);
		}
		catch (Exception ex)
		{
			Debug.LogError("[LocalSaveManager] Failed to save game: " + ex.Message);
		}
	}

	// Token: 0x06000F8C RID: 3980 RVA: 0x00041CC4 File Offset: 0x0003FEC4
	public void DeleteSave(string saveName)
	{
		if (string.IsNullOrEmpty(saveName))
		{
			Debug.LogError("[LocalSaveManager] Cannot delete save with empty name");
			return;
		}
		string path = Path.Combine(this.SaveDirectoryPath, saveName + ".json");
		if (File.Exists(path))
		{
			try
			{
				File.Delete(path);
				Debug.Log("[LocalSaveManager] Deleted save: " + saveName);
				if (this.SelectedSaveName == saveName)
				{
					this.SelectedSaveName = "";
					PlayerPrefs.DeleteKey("SelectedSaveName");
					PlayerPrefs.Save();
				}
			}
			catch (Exception ex)
			{
				Debug.LogError("[LocalSaveManager] Failed to delete save: " + ex.Message);
			}
		}
	}

	// Token: 0x040009FF RID: 2559
	private const string SAVE_DIRECTORY = "Saves";

	// Token: 0x04000A00 RID: 2560
	private const string SELECTED_SAVE_KEY = "SelectedSaveName";

	// Token: 0x04000A01 RID: 2561
	private const string SAVE_DATA_KEY = "SelectedSaveData";
}
