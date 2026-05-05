using System;
using System.Collections;
using Extensions;
using UnityEngine;
using UnityEngine.Networking;

// Token: 0x02000015 RID: 21
public class GameSettingsAPIManager : NetworkSingleton<GameSettingsAPIManager>
{
	// Token: 0x06000042 RID: 66 RVA: 0x00003F86 File Offset: 0x00002186
	public override void OnStartServer()
	{
		base.OnStartServer();
		base.StartCoroutine(this.FetchAndApplyMainSettingsCoroutine());
	}

	// Token: 0x06000043 RID: 67 RVA: 0x00003F9B File Offset: 0x0000219B
	public void ReloadMainSettings()
	{
		if (!base.isServer)
		{
			Debug.LogWarning("[GameSettingsAPIManager] Not on server - cannot reload settings");
			return;
		}
		base.StartCoroutine(this.FetchAndApplyMainSettingsCoroutine());
	}

	// Token: 0x06000044 RID: 68 RVA: 0x00003FBD File Offset: 0x000021BD
	public IEnumerator FetchAndApplyMainSettingsCoroutine()
	{
		this.gameSettings = Resources.Load<GameSettings>("GameSettings");
		long num = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
		string uri = string.Format("{0}/{1}/main-settings?t={2}", this.apiBaseUrl, this.gameId, num);
		using (UnityWebRequest request = UnityWebRequest.Get(uri))
		{
			request.SetRequestHeader("Content-Type", "application/json");
			request.SetRequestHeader("Cache-Control", "no-cache, no-store, must-revalidate");
			request.SetRequestHeader("Pragma", "no-cache");
			request.SetRequestHeader("Expires", "0");
			yield return request.SendWebRequest();
			if (request.result != UnityWebRequest.Result.Success)
			{
				Debug.LogError("[GameSettingsAPIManager] Failed to fetch main settings: " + request.error);
				if (request.downloadHandler != null)
				{
					Debug.LogError("[GameSettingsAPIManager] Response: " + request.downloadHandler.text);
				}
				yield break;
			}
			try
			{
				GameSettingsAPIManager.MainSettingsResponse mainSettingsResponse = JsonUtility.FromJson<GameSettingsAPIManager.MainSettingsResponse>(request.downloadHandler.text);
				if (((mainSettingsResponse != null) ? mainSettingsResponse.settings : null) != null)
				{
					DynamicSettingsApplier.ApplySettings(mainSettingsResponse.settings);
				}
				else
				{
					Debug.LogWarning("[GameSettingsAPIManager] ⚠️ Response did not contain settings array");
				}
			}
			catch (Exception ex)
			{
				Debug.LogError("[GameSettingsAPIManager] ❌ Error parsing main settings: " + ex.Message + "\n" + ex.StackTrace);
			}
		}
		UnityWebRequest request = null;
		yield break;
		yield break;
	}

	// Token: 0x06000046 RID: 70 RVA: 0x00002321 File Offset: 0x00000521
	public override bool Weaved()
	{
		return true;
	}

	// Token: 0x0400005A RID: 90
	[Header("API Settings")]
	[SerializeField]
	private string gameId = "gamblelite";

	// Token: 0x0400005B RID: 91
	[SerializeField]
	private string apiBaseUrl = "https://api.diabolical.studio/rest-api/gameSettings";

	// Token: 0x0400005C RID: 92
	[SerializeField]
	private GameSettings gameSettings;

	// Token: 0x02000016 RID: 22
	[Serializable]
	private class MainSettingsResponse
	{
		// Token: 0x0400005D RID: 93
		public string game_id;

		// Token: 0x0400005E RID: 94
		public string game_name;

		// Token: 0x0400005F RID: 95
		public SettingResponse[] settings;
	}
}
