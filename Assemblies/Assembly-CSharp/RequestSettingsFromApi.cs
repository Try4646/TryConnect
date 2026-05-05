using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using Extensions;
using UnityEngine;
using UnityEngine.Networking;

// Token: 0x02000018 RID: 24
public class RequestSettingsFromApi : NetworkSingleton<RequestSettingsFromApi>
{
	// Token: 0x0600004F RID: 79 RVA: 0x00004270 File Offset: 0x00002470
	public void ReloadSettings()
	{
		if (base.isServer)
		{
			base.StartCoroutine(this.LoadAndApplySettingsCoroutine());
		}
	}

	// Token: 0x06000050 RID: 80 RVA: 0x00004287 File Offset: 0x00002487
	private IEnumerator LoadAndApplySettingsCoroutine()
	{
		yield return base.StartCoroutine(this.FetchAllGameSettingsCoroutine());
		this.ApplySettingsToGames();
		yield break;
	}

	// Token: 0x06000051 RID: 81 RVA: 0x00004296 File Offset: 0x00002496
	private IEnumerator FetchAllGameSettingsCoroutine()
	{
		long num = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
		string uri = string.Format("{0}/{1}/all?t={2}", this.apiBaseUrl, this.gameId, num);
		using (UnityWebRequest request = UnityWebRequest.Get(uri))
		{
			request.SetRequestHeader("Cache-Control", "no-cache, no-store, must-revalidate");
			request.SetRequestHeader("Pragma", "no-cache");
			request.SetRequestHeader("Expires", "0");
			yield return request.SendWebRequest();
			if (request.result != UnityWebRequest.Result.Success)
			{
				Debug.LogError("Failed to fetch game settings: " + request.error);
				yield break;
			}
			try
			{
				RequestSettingsFromApi.GameDataResponse gameDataResponse = JsonUtility.FromJson<RequestSettingsFromApi.GameDataResponse>(request.downloadHandler.text);
				if (((gameDataResponse != null) ? gameDataResponse.sub_games : null) != null)
				{
					this.ProcessGameSettings(gameDataResponse);
				}
			}
			catch (Exception ex)
			{
				Debug.LogError("Error parsing game settings: " + ex.Message);
			}
		}
		UnityWebRequest request = null;
		yield break;
		yield break;
	}

	// Token: 0x06000052 RID: 82 RVA: 0x000042A8 File Offset: 0x000024A8
	private void ProcessGameSettings(RequestSettingsFromApi.GameDataResponse gameData)
	{
		this._gameSettings.Clear();
		foreach (RequestSettingsFromApi.SubGameResponse subGameResponse in gameData.sub_games)
		{
			if (subGameResponse.settings != null)
			{
				string sub_game_name = subGameResponse.sub_game_name;
				string text = ((sub_game_name != null) ? sub_game_name.ToLower() : null) ?? "";
				if (!string.IsNullOrEmpty(text))
				{
					RequestSettingsFromApi.GameSettings gameSettings = new RequestSettingsFromApi.GameSettings();
					foreach (RequestSettingsFromApi.SettingResponse settingResponse in subGameResponse.settings)
					{
						if (!(settingResponse.setting_type != "number"))
						{
							string setting_key = settingResponse.setting_key;
							float value3;
							if (!(setting_key == "estimatedValue"))
							{
								int value2;
								if (!(setting_key == "baseMinBet"))
								{
									if (setting_key == "baseMaxBet")
									{
										int value;
										if (int.TryParse(settingResponse.setting_value, out value))
										{
											gameSettings.baseMaxBet = new int?(value);
										}
									}
								}
								else if (int.TryParse(settingResponse.setting_value, out value2))
								{
									gameSettings.baseMinBet = new int?(value2);
								}
							}
							else if (float.TryParse(settingResponse.setting_value, NumberStyles.Float, CultureInfo.InvariantCulture, out value3))
							{
								gameSettings.estimatedValue = new float?(value3);
							}
						}
					}
					if (gameSettings.estimatedValue != null || gameSettings.baseMinBet != null || gameSettings.baseMaxBet != null)
					{
						this._gameSettings[text] = gameSettings;
					}
				}
			}
		}
	}

	// Token: 0x06000053 RID: 83 RVA: 0x0000442C File Offset: 0x0000262C
	private void ApplySettingsToGames()
	{
		GameBase[] array = Object.FindObjectsByType<GameBase>(FindObjectsInactive.Include, FindObjectsSortMode.None);
		List<GameObject> list = new List<GameObject>();
		foreach (GameBase gameBase in array)
		{
			string gameName = gameBase.GameName;
			string text = ((gameName != null) ? gameName.ToLower() : null) ?? "";
			string text2 = gameBase.GameType.ToString().ToLower();
			RequestSettingsFromApi.GameSettings settings = null;
			if (!string.IsNullOrEmpty(text) && this._gameSettings.TryGetValue(text, out settings))
			{
				this.ApplySettingsToGame(gameBase, settings);
			}
			else if (this._gameSettings.TryGetValue(text2, out settings))
			{
				this.ApplySettingsToGame(gameBase, settings);
			}
			else
			{
				bool flag = false;
				foreach (string text3 in new string[]
				{
					text.Replace(" ", ""),
					text.Replace(" ", "_"),
					(text2 == "slotmachine") ? "slots" : null
				})
				{
					if (!string.IsNullOrEmpty(text3) && this._gameSettings.TryGetValue(text3, out settings))
					{
						this.ApplySettingsToGame(gameBase, settings);
						flag = true;
						break;
					}
				}
				if (!flag)
				{
					list.Add(gameBase.gameObject);
				}
			}
		}
		if (list.Count > 0)
		{
			Debug.Log(string.Format("[RequestSettingsFromApi] No API settings applied for {0} GameObject(s): ", list.Count) + string.Join(", ", list.ConvertAll<string>((GameObject g) => g.name)));
		}
	}

	// Token: 0x06000054 RID: 84 RVA: 0x000045D8 File Offset: 0x000027D8
	private void ApplySettingsToGame(GameBase game, RequestSettingsFromApi.GameSettings settings)
	{
		if (settings.estimatedValue != null)
		{
			game.SetEstimatedValue(settings.estimatedValue.Value);
		}
		if (settings.baseMinBet != null)
		{
			game.BaseMinBet = (long)settings.baseMinBet.Value;
		}
		if (settings.baseMaxBet != null)
		{
			game.BaseMaxBet = (long)settings.baseMaxBet.Value;
		}
	}

	// Token: 0x06000056 RID: 86 RVA: 0x00002321 File Offset: 0x00000521
	public override bool Weaved()
	{
		return true;
	}

	// Token: 0x04000064 RID: 100
	[Header("API Settings")]
	[SerializeField]
	private string gameId = "";

	// Token: 0x04000065 RID: 101
	[SerializeField]
	private string apiBaseUrl = "";

	// Token: 0x04000066 RID: 102
	private Dictionary<string, RequestSettingsFromApi.GameSettings> _gameSettings = new Dictionary<string, RequestSettingsFromApi.GameSettings>();

	// Token: 0x02000019 RID: 25
	private class GameSettings
	{
		// Token: 0x04000067 RID: 103
		public float? estimatedValue;

		// Token: 0x04000068 RID: 104
		public int? baseMinBet;

		// Token: 0x04000069 RID: 105
		public int? baseMaxBet;
	}

	// Token: 0x0200001A RID: 26
	[Serializable]
	public class GameDataResponse
	{
		// Token: 0x0400006A RID: 106
		public string game_id;

		// Token: 0x0400006B RID: 107
		public string game_name;

		// Token: 0x0400006C RID: 108
		public RequestSettingsFromApi.SubGameResponse[] sub_games;
	}

	// Token: 0x0200001B RID: 27
	[Serializable]
	public class SubGameResponse
	{
		// Token: 0x0400006D RID: 109
		public string sub_game_name;

		// Token: 0x0400006E RID: 110
		public RequestSettingsFromApi.SettingResponse[] settings;
	}

	// Token: 0x0200001C RID: 28
	[Serializable]
	public class SettingResponse
	{
		// Token: 0x0400006F RID: 111
		public string setting_key;

		// Token: 0x04000070 RID: 112
		public string setting_value;

		// Token: 0x04000071 RID: 113
		public string setting_type;
	}
}
