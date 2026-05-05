using System;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

// Token: 0x0200009D RID: 157
public class ConsoleButton : MonoBehaviour
{
	// Token: 0x060005B5 RID: 1461 RVA: 0x00019160 File Offset: 0x00017360
	private void Awake()
	{
		if (this.button == null)
		{
			this.button = base.GetComponent<Button>();
		}
		if (this.buttonText == null)
		{
			this.buttonText = base.GetComponentInChildren<TextMeshProUGUI>();
		}
		if (this.buttonImage == null)
		{
			this.buttonImage = base.GetComponent<Image>();
		}
	}

	// Token: 0x060005B6 RID: 1462 RVA: 0x000191BB File Offset: 0x000173BB
	public void SetText(string text)
	{
		if (this.buttonText != null)
		{
			this.buttonText.text = text;
		}
	}

	// Token: 0x060005B7 RID: 1463 RVA: 0x000191D7 File Offset: 0x000173D7
	public void SetOnClick(UnityAction action)
	{
		if (this.button != null)
		{
			this.button.onClick.RemoveAllListeners();
			this.button.onClick.AddListener(action);
		}
	}

	// Token: 0x060005B8 RID: 1464 RVA: 0x00019208 File Offset: 0x00017408
	public void SetInteractable(bool interactable)
	{
		if (this.button != null)
		{
			this.button.interactable = interactable;
		}
	}

	// Token: 0x040003FC RID: 1020
	[Header("UI Components")]
	[SerializeField]
	private Button button;

	// Token: 0x040003FD RID: 1021
	[SerializeField]
	private TextMeshProUGUI buttonText;

	// Token: 0x040003FE RID: 1022
	[SerializeField]
	private Image buttonImage;
}
