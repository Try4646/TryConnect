using System;
using System.Collections;
using System.Text;
using Extensions;
using Mirror;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

// Token: 0x02000009 RID: 9
public class BugReportAPIClient : MonoSingleton<BugReportAPIClient>
{
	// Token: 0x17000005 RID: 5
	// (get) Token: 0x06000019 RID: 25 RVA: 0x00002520 File Offset: 0x00000720
	public static string SessionId
	{
		get
		{
			if (string.IsNullOrEmpty(BugReportAPIClient._sessionId))
			{
				BugReportAPIClient._sessionId = Guid.NewGuid().ToString();
			}
			return BugReportAPIClient._sessionId;
		}
	}

	// Token: 0x0600001A RID: 26 RVA: 0x00002558 File Offset: 0x00000758
	public void FillContext(BugReportPayload p)
	{
		p.version = Application.version;
		p.build = Application.version;
		p.platform = Application.platform.ToString();
		p.os = SystemInfo.operatingSystem;
		p.gpu = SystemInfo.graphicsDeviceName;
		p.cpu = SystemInfo.processorType;
		p.ram = SystemInfo.systemMemorySize;
		p.driverVersion = SystemInfo.graphicsDeviceVersion;
		p.scene = SceneManager.GetActiveScene().name;
		p.timestampUtc = DateTime.UtcNow.ToString("o");
		p.gameId = this.gameId;
		p.sessionId = BugReportAPIClient.SessionId;
		p.branch = "";
		if (NetworkClient.active)
		{
			p.role = (NetworkServer.active ? "Host" : "Client");
			p.networkBackend = "Mirror";
			if (NetworkManager.singleton != null && NetworkManager.singleton.numPlayers > 0)
			{
				p.playerCount = NetworkManager.singleton.numPlayers;
			}
		}
	}

	// Token: 0x0600001B RID: 27 RVA: 0x0000266D File Offset: 0x0000086D
	public void SendReport(BugReportPayload payload, Action<bool, string, string> onComplete)
	{
		if (string.IsNullOrWhiteSpace(this.discordWebhookUrl))
		{
			if (onComplete != null)
			{
				onComplete(false, "Discord webhook URL not set.", null);
			}
			return;
		}
		base.StartCoroutine(this.SendReportCoroutine(payload, onComplete));
	}

	// Token: 0x0600001C RID: 28 RVA: 0x0000269C File Offset: 0x0000089C
	private IEnumerator SendReportCoroutine(BugReportPayload payload, Action<bool, string, string> onComplete)
	{
		string s = BugReportAPIClient.BuildDiscordWebhookJson(payload);
		byte[] bytes = Encoding.UTF8.GetBytes(s);
		using (UnityWebRequest request = new UnityWebRequest(this.discordWebhookUrl, "POST"))
		{
			request.uploadHandler = new UploadHandlerRaw(bytes);
			request.downloadHandler = new DownloadHandlerBuffer();
			request.SetRequestHeader("Content-Type", "application/json");
			yield return request.SendWebRequest();
			bool flag = request.result == UnityWebRequest.Result.Success;
			string text = null;
			if (!flag)
			{
				text = ((request.downloadHandler != null && request.downloadHandler.data != null && request.downloadHandler.data.Length != 0) ? request.downloadHandler.text : request.error);
				if (string.IsNullOrEmpty(text))
				{
					text = "Request failed.";
				}
			}
			if (onComplete != null)
			{
				onComplete(flag, text ?? "", null);
			}
		}
		UnityWebRequest request = null;
		yield break;
		yield break;
	}

	// Token: 0x0600001D RID: 29 RVA: 0x000026BC File Offset: 0x000008BC
	private static string BuildDiscordWebhookJson(BugReportPayload p)
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.AppendLine(string.Concat(new string[]
		{
			"**Severity:** ",
			p.severity,
			"  **Category:** ",
			p.category,
			"  **Frequency:** ",
			p.frequency
		}));
		stringBuilder.AppendLine(string.Format("**Can reproduce now:** {0}", p.canReproduceNow));
		if (!string.IsNullOrEmpty(p.whatHappened))
		{
			stringBuilder.AppendLine().AppendLine("**What happened**").AppendLine(p.whatHappened);
		}
		if (!string.IsNullOrEmpty(p.expected))
		{
			stringBuilder.AppendLine().AppendLine("**Expected**").AppendLine(p.expected);
		}
		if (p.reproSteps != null && p.reproSteps.Length != 0)
		{
			stringBuilder.AppendLine().AppendLine("**Repro**");
			for (int i = 0; i < p.reproSteps.Length; i++)
			{
				stringBuilder.AppendLine(string.Format("{0}. {1}", i + 1, p.reproSteps[i]));
			}
		}
		stringBuilder.AppendLine().AppendLine("**Context**");
		stringBuilder.AppendLine(string.Concat(new string[]
		{
			"Game: `",
			p.gameId,
			"`  Version: `",
			p.version,
			"`  Scene: `",
			p.scene,
			"`"
		}));
		stringBuilder.AppendLine(string.Concat(new string[]
		{
			"Platform: `",
			p.platform,
			"`  OS: `",
			BugReportAPIClient.Trunc(p.os, 120),
			"`"
		}));
		stringBuilder.AppendLine(string.Format("GPU: `{0}`  CPU: `{1}`  RAM MB: `{2}`", BugReportAPIClient.Trunc(p.gpu, 80), BugReportAPIClient.Trunc(p.cpu, 80), p.ram));
		if (!string.IsNullOrEmpty(p.role))
		{
			stringBuilder.AppendLine(string.Format("Network: `{0}`  Role: `{1}`  Players: `{2}`", p.networkBackend, p.role, p.playerCount));
		}
		stringBuilder.AppendLine(string.Concat(new string[]
		{
			"Session: `",
			p.sessionId,
			"`  UTC: `",
			p.timestampUtc,
			"`"
		}));
		string description = BugReportAPIClient.Trunc(stringBuilder.ToString(), 3900);
		string title = BugReportAPIClient.Trunc(string.IsNullOrEmpty(p.title) ? "Bug report" : p.title, 250);
		return JsonUtility.ToJson(new BugReportAPIClient.DiscordWebhookBody
		{
			username = "Bug report",
			embeds = new BugReportAPIClient.DiscordEmbed[]
			{
				new BugReportAPIClient.DiscordEmbed
				{
					title = title,
					description = description,
					color = 15158332,
					footer = new BugReportAPIClient.DiscordEmbedFooter
					{
						text = p.gameId + " · " + p.sessionId
					}
				}
			}
		});
	}

	// Token: 0x0600001E RID: 30 RVA: 0x000029C5 File Offset: 0x00000BC5
	private static string Trunc(string s, int max)
	{
		if (string.IsNullOrEmpty(s))
		{
			return "";
		}
		if (s.Length <= max)
		{
			return s;
		}
		return s.Substring(0, max - 1) + "…";
	}

	// Token: 0x04000018 RID: 24
	[Header("Discord webhook")]
	[Tooltip("Full URL: https://discord.com/api/webhooks/...")]
	[SerializeField]
	private string discordWebhookUrl = "";

	// Token: 0x04000019 RID: 25
	[SerializeField]
	private string gameId = "gamblelite";

	// Token: 0x0400001A RID: 26
	private static string _sessionId;

	// Token: 0x0200000A RID: 10
	[Serializable]
	private class DiscordWebhookBody
	{
		// Token: 0x0400001B RID: 27
		public string username;

		// Token: 0x0400001C RID: 28
		public BugReportAPIClient.DiscordEmbed[] embeds;
	}

	// Token: 0x0200000B RID: 11
	[Serializable]
	private class DiscordEmbed
	{
		// Token: 0x0400001D RID: 29
		public string title;

		// Token: 0x0400001E RID: 30
		public string description;

		// Token: 0x0400001F RID: 31
		public int color;

		// Token: 0x04000020 RID: 32
		public BugReportAPIClient.DiscordEmbedFooter footer;
	}

	// Token: 0x0200000C RID: 12
	[Serializable]
	private class DiscordEmbedFooter
	{
		// Token: 0x04000021 RID: 33
		public string text;
	}
}
