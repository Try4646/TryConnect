using System;
using TMPro;
using UnityEngine;

// Token: 0x020001B2 RID: 434
public class UpgradeEntryUI : MonoBehaviour
{
	// Token: 0x06000FC2 RID: 4034 RVA: 0x000432F0 File Offset: 0x000414F0
	public void SetUpgradeEntry(PlayerUpgradeType type, float value, float change)
	{
		string text = "";
		string text2 = "";
		switch (type)
		{
		case PlayerUpgradeType.GamblersConfidence:
			text = "Gambler's Confidence";
			text2 = "Increases profit";
			value -= 1f;
			break;
		case PlayerUpgradeType.Insurance:
			text = "Insurance";
			text2 = "Reduces loss";
			break;
		case PlayerUpgradeType.Stakeholder:
			text = "Stakeholder";
			text2 = "Empowers held items";
			value -= 1f;
			break;
		case PlayerUpgradeType.BonusDraw:
			text = "Bonus Draw";
			text2 = "Gives a chance to earn ticket on win";
			break;
		}
		this.labelText.text = text;
		this.descriptionText.text = text2;
		this.valueText.text = (value * 100f).ToString("0.#") + "%";
	}

	// Token: 0x04000A3B RID: 2619
	[SerializeField]
	private TextMeshProUGUI labelText;

	// Token: 0x04000A3C RID: 2620
	[SerializeField]
	private TextMeshProUGUI descriptionText;

	// Token: 0x04000A3D RID: 2621
	[SerializeField]
	private TextMeshProUGUI valueText;
}
