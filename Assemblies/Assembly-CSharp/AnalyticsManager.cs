using System;
using System.Collections;
using System.Text;
using Extensions;
using Mirror;
using UnityEngine;
using UnityEngine.Networking;

// Token: 0x02000006 RID: 6
public class AnalyticsManager : NetworkSingleton<AnalyticsManager>
{
	// Token: 0x0600000B RID: 11 RVA: 0x000021A8 File Offset: 0x000003A8
	public override void OnStartServer()
	{
		this.sessionId = string.Format("session_{0}", Random.Range(100000, 999999));
		this.roundCounter = 0;
	}

	// Token: 0x0600000C RID: 12 RVA: 0x000021D8 File Offset: 0x000003D8
	[Server]
	public void SendAnalytics(GameBase game, string playerName, long betAmount, long payout)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void AnalyticsManager::SendAnalytics(GameBase,System.String,System.Int64,System.Int64)' called when server was not active");
			return;
		}
		if (string.IsNullOrEmpty(this.apiKey))
		{
			Debug.LogWarning("[AnalyticsManager] API key is not set. Skipping analytics.");
			return;
		}
		this.roundCounter++;
		string round_id = string.Format("round_{0}", this.roundCounter);
		string subGameName = this.GetSubGameName(game);
		float bet_multiplier = (betAmount > 0L) ? ((float)payout / (float)betAmount) : 0f;
		string bet_type = "standard";
		AnalyticsManager.AnalyticsData data = new AnalyticsManager.AnalyticsData
		{
			game_id = this.gameId,
			sub_game_name = subGameName,
			player_name = playerName,
			bet_amount = (float)betAmount,
			bet_multiplier = bet_multiplier,
			win_amount = (float)payout,
			round_id = round_id,
			session_id = this.sessionId,
			bet_type = bet_type
		};
		base.StartCoroutine(this.SendAnalyticsCoroutine(data));
	}

	// Token: 0x0600000D RID: 13 RVA: 0x000022BC File Offset: 0x000004BC
	private string GetSubGameName(GameBase game)
	{
		return game.GameType.ToString().ToLower();
	}

	// Token: 0x0600000E RID: 14 RVA: 0x000022E2 File Offset: 0x000004E2
	private IEnumerator SendAnalyticsCoroutine(AnalyticsManager.AnalyticsData data)
	{
		string s = JsonUtility.ToJson(data);
		byte[] bytes = Encoding.UTF8.GetBytes(s);
		using (UnityWebRequest request = new UnityWebRequest(this.apiEndpoint, "POST"))
		{
			request.uploadHandler = new UploadHandlerRaw(bytes);
			request.downloadHandler = new DownloadHandlerBuffer();
			request.SetRequestHeader("x-api-key", this.apiKey);
			request.SetRequestHeader("Content-Type", "application/json");
			yield return request.SendWebRequest();
			if (request.result != UnityWebRequest.Result.Success)
			{
				Debug.LogError("[AnalyticsManager] Failed to send analytics: " + request.error);
				string str = "[AnalyticsManager] Response: ";
				DownloadHandler downloadHandler = request.downloadHandler;
				Debug.LogError(str + ((downloadHandler != null) ? downloadHandler.text : null));
			}
			else
			{
				Debug.Log("[AnalyticsManager] Analytics sent successfully for round " + data.round_id);
			}
		}
		UnityWebRequest request = null;
		yield break;
		yield break;
	}

	// Token: 0x06000010 RID: 16 RVA: 0x00002321 File Offset: 0x00000521
	public override bool Weaved()
	{
		return true;
	}

	// Token: 0x04000005 RID: 5
	[Header("API Settings")]
	[SerializeField]
	private string apiKey = "";

	// Token: 0x04000006 RID: 6
	[SerializeField]
	private string apiEndpoint = "";

	// Token: 0x04000007 RID: 7
	[SerializeField]
	private string gameId = "";

	// Token: 0x04000008 RID: 8
	private string sessionId;

	// Token: 0x04000009 RID: 9
	private int roundCounter;

	// Token: 0x02000007 RID: 7
	[Serializable]
	private class AnalyticsData
	{
		// Token: 0x0400000A RID: 10
		public string game_id;

		// Token: 0x0400000B RID: 11
		public string sub_game_name;

		// Token: 0x0400000C RID: 12
		public string player_name;

		// Token: 0x0400000D RID: 13
		public float bet_amount;

		// Token: 0x0400000E RID: 14
		public float bet_multiplier;

		// Token: 0x0400000F RID: 15
		public float win_amount;

		// Token: 0x04000010 RID: 16
		public string round_id;

		// Token: 0x04000011 RID: 17
		public string session_id;

		// Token: 0x04000012 RID: 18
		public string bet_type;
	}
}
