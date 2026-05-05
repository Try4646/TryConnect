using System;
using TMPro;
using UnityEngine;

// Token: 0x02000304 RID: 772
public class VersionMismatchDisplay : MonoBehaviour
{
	// Token: 0x06001A73 RID: 6771 RVA: 0x0006F8F7 File Offset: 0x0006DAF7
	private void Awake()
	{
		if (this.versionMismatchPanel != null)
		{
			this.versionMismatchPanel.SetActive(false);
		}
	}

	// Token: 0x06001A74 RID: 6772 RVA: 0x0006F914 File Offset: 0x0006DB14
	public void ShowVersionMismatch(string playerVersion)
	{
		if (this.versionMismatchPanel != null)
		{
			this.versionMismatchPanel.SetActive(true);
			if (this.versionText != null && !string.IsNullOrEmpty(playerVersion))
			{
				this.versionText.text = "Version: " + playerVersion;
			}
		}
	}

	// Token: 0x06001A75 RID: 6773 RVA: 0x0006F8F7 File Offset: 0x0006DAF7
	public void HideVersionMismatch()
	{
		if (this.versionMismatchPanel != null)
		{
			this.versionMismatchPanel.SetActive(false);
		}
	}

	// Token: 0x0400111B RID: 4379
	[Header("UI References")]
	[SerializeField]
	private GameObject versionMismatchPanel;

	// Token: 0x0400111C RID: 4380
	[SerializeField]
	private TextMeshProUGUI versionText;
}
