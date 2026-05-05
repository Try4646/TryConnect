using System;
using TMPro;
using UnityEngine;

// Token: 0x02000169 RID: 361
public class LoadingQuotes : MonoBehaviour
{
	// Token: 0x06000DF7 RID: 3575 RVA: 0x00039FBC File Offset: 0x000381BC
	public void ShowRandomQuote()
	{
		if (this.targetText == null || this.quotes == null || this.quotes.Length == 0)
		{
			return;
		}
		int num = Random.Range(0, this.quotes.Length);
		this.targetText.text = this.quotes[num];
	}

	// Token: 0x040008D3 RID: 2259
	[SerializeField]
	private TextMeshProUGUI targetText;

	// Token: 0x040008D4 RID: 2260
	[SerializeField]
	private string[] quotes;
}
