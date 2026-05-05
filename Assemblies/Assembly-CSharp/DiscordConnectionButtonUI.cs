using System;
using Extensions;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Token: 0x02000243 RID: 579
public class DiscordConnectionButtonUI : MonoBehaviour
{
	// Token: 0x060014DB RID: 5339 RVA: 0x000598E4 File Offset: 0x00057AE4
	private void OnEnable()
	{
		DiscordRichPresenceManager instance = MonoSingleton<DiscordRichPresenceManager>.Instance;
		if (instance != null)
		{
			instance.ConnectionStateChanged += this.OnConnectionStateChanged;
		}
		this.RefreshConnectionState();
	}

	// Token: 0x060014DC RID: 5340 RVA: 0x00059918 File Offset: 0x00057B18
	private void OnDisable()
	{
		DiscordRichPresenceManager instance = MonoSingleton<DiscordRichPresenceManager>.Instance;
		if (instance != null)
		{
			instance.ConnectionStateChanged -= this.OnConnectionStateChanged;
		}
	}

	// Token: 0x060014DD RID: 5341 RVA: 0x00059946 File Offset: 0x00057B46
	public void OnApplicationFocus(bool hasFocus)
	{
		if (hasFocus)
		{
			this.RefreshConnectionState();
		}
	}

	// Token: 0x060014DE RID: 5342 RVA: 0x00059954 File Offset: 0x00057B54
	public void StartDiscordIntegration()
	{
		DiscordRichPresenceManager instance = MonoSingleton<DiscordRichPresenceManager>.Instance;
		if (instance == null)
		{
			return;
		}
		if (instance.IsConnected)
		{
			instance.DisconnectDiscord();
		}
		else
		{
			DiscordRichPresenceManager.SetUserAcceptedDiscordToaster();
			instance.ConnectDiscord(true);
		}
		this.RefreshConnectionState();
	}

	// Token: 0x060014DF RID: 5343 RVA: 0x00059994 File Offset: 0x00057B94
	public void RefreshConnectionState()
	{
		DiscordRichPresenceManager instance = MonoSingleton<DiscordRichPresenceManager>.Instance;
		bool isConnected = instance != null && instance.IsConnected;
		this.ApplyVisualState(isConnected);
	}

	// Token: 0x060014E0 RID: 5344 RVA: 0x000599C1 File Offset: 0x00057BC1
	public void SetConnectedVisual()
	{
		this.ApplyVisualState(true);
	}

	// Token: 0x060014E1 RID: 5345 RVA: 0x000599CA File Offset: 0x00057BCA
	public void SetDisconnectedVisual()
	{
		this.ApplyVisualState(false);
	}

	// Token: 0x060014E2 RID: 5346 RVA: 0x000599D4 File Offset: 0x00057BD4
	private void ApplyVisualState(bool isConnected)
	{
		if (this.statusText)
		{
			this.statusText.text = (isConnected ? this.connectedText : this.disconnectedText);
		}
		if (this.statusGraphic)
		{
			this.statusGraphic.color = (isConnected ? this.colorPalette.profitGreen : this.colorPalette.white);
		}
		this.button.interactable = true;
		this.button.GetComponent<Image>().color = (isConnected ? this.colorPalette.profitGreen : this.colorPalette.white);
	}

	// Token: 0x060014E3 RID: 5347 RVA: 0x00059A74 File Offset: 0x00057C74
	private void OnConnectionStateChanged(bool isConnected)
	{
		this.ApplyVisualState(isConnected);
	}

	// Token: 0x04000D48 RID: 3400
	[SerializeField]
	private UIColorPalette colorPalette;

	// Token: 0x04000D49 RID: 3401
	[Header("Optional UI")]
	[SerializeField]
	private Button button;

	// Token: 0x04000D4A RID: 3402
	[SerializeField]
	private TextMeshProUGUI statusText;

	// Token: 0x04000D4B RID: 3403
	[SerializeField]
	private Graphic statusGraphic;

	// Token: 0x04000D4C RID: 3404
	[SerializeField]
	private string connectedText = "Discord Connected";

	// Token: 0x04000D4D RID: 3405
	[SerializeField]
	private string disconnectedText = "Connect Discord";
}
