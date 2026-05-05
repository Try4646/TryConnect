using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

// Token: 0x020002A4 RID: 676
public static class PlayerVoiceVolumePersistence
{
	// Token: 0x17000222 RID: 546
	// (get) Token: 0x060017ED RID: 6125 RVA: 0x0006568E File Offset: 0x0006388E
	private static string SavePath
	{
		get
		{
			return Path.Combine(Application.persistentDataPath, "playervoicevolumes.json");
		}
	}

	// Token: 0x060017EE RID: 6126 RVA: 0x000656A0 File Offset: 0x000638A0
	public static Dictionary<string, float> Load()
	{
		Dictionary<string, float> dictionary = new Dictionary<string, float>(StringComparer.OrdinalIgnoreCase);
		if (!File.Exists(PlayerVoiceVolumePersistence.SavePath))
		{
			return dictionary;
		}
		try
		{
			PlayerVoiceVolumePersistence.SaveData saveData = JsonUtility.FromJson<PlayerVoiceVolumePersistence.SaveData>(File.ReadAllText(PlayerVoiceVolumePersistence.SavePath));
			if (((saveData != null) ? saveData.volumes : null) == null)
			{
				return dictionary;
			}
			foreach (PlayerVoiceVolumePersistence.PlayerVolumeEntry playerVolumeEntry in saveData.volumes)
			{
				if (!string.IsNullOrEmpty(playerVolumeEntry.steamId))
				{
					dictionary[playerVolumeEntry.steamId] = Mathf.Clamp01(playerVolumeEntry.volumePercent);
				}
			}
		}
		catch (Exception ex)
		{
			Debug.LogWarning("[PlayerVoiceVolumePersistence] Load failed: " + ex.Message);
		}
		return dictionary;
	}

	// Token: 0x060017EF RID: 6127 RVA: 0x0006577C File Offset: 0x0006397C
	public static void Save(Dictionary<string, float> volumesBySteamId)
	{
		if (volumesBySteamId == null)
		{
			return;
		}
		try
		{
			PlayerVoiceVolumePersistence.SaveData saveData = new PlayerVoiceVolumePersistence.SaveData();
			foreach (KeyValuePair<string, float> keyValuePair in volumesBySteamId)
			{
				if (!string.IsNullOrEmpty(keyValuePair.Key))
				{
					saveData.volumes.Add(new PlayerVoiceVolumePersistence.PlayerVolumeEntry
					{
						steamId = keyValuePair.Key,
						volumePercent = Mathf.Clamp01(keyValuePair.Value)
					});
				}
			}
			File.WriteAllText(PlayerVoiceVolumePersistence.SavePath, JsonUtility.ToJson(saveData, true));
		}
		catch (Exception ex)
		{
			Debug.LogWarning("[PlayerVoiceVolumePersistence] Save failed: " + ex.Message);
		}
	}

	// Token: 0x04000F75 RID: 3957
	private const string FileName = "playervoicevolumes.json";

	// Token: 0x020002A5 RID: 677
	[Serializable]
	private class SaveData
	{
		// Token: 0x04000F76 RID: 3958
		public List<PlayerVoiceVolumePersistence.PlayerVolumeEntry> volumes = new List<PlayerVoiceVolumePersistence.PlayerVolumeEntry>();
	}

	// Token: 0x020002A6 RID: 678
	[Serializable]
	public class PlayerVolumeEntry
	{
		// Token: 0x04000F77 RID: 3959
		public string steamId;

		// Token: 0x04000F78 RID: 3960
		public float volumePercent;
	}
}
