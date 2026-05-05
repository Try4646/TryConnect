using System;
using TMPro;
using UnityEngine;

// Token: 0x02000303 RID: 771
public class VersionDisplay : MonoBehaviour
{
	// Token: 0x06001A71 RID: 6769 RVA: 0x0006F8D9 File Offset: 0x0006DAD9
	private void Start()
	{
		this.versionText = base.GetComponent<TextMeshProUGUI>();
		this.versionText.text = Application.version;
	}

	// Token: 0x0400111A RID: 4378
	private TextMeshProUGUI versionText;
}
