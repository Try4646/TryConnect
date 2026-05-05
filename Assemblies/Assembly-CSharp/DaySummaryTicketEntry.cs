using System;
using System.Collections;
using DG.Tweening;
using FMODUnity;
using TMPro;
using UnityEngine;

// Token: 0x0200025C RID: 604
public class DaySummaryTicketEntry : MonoBehaviour
{
	// Token: 0x06001583 RID: 5507 RVA: 0x0005C5C5 File Offset: 0x0005A7C5
	public void Setup(string source, int tickets)
	{
		this._source = source;
		this._tickets = "+" + tickets.ToString();
	}

	// Token: 0x06001584 RID: 5508 RVA: 0x0005C5E5 File Offset: 0x0005A7E5
	public IEnumerator Animate(float duration)
	{
		this.sourceText.text = this._source;
		SFXManager.SFXOneShot(this.smallTextSlideSfx, default(Vector3));
		yield return new WaitForSeconds(duration);
		this.ticketsText.text = this._tickets;
		this.ticketsText.transform.DOPunchScale(this.ticketsText.transform.localScale * 0.2f, 0.5f, 1, 1f);
		SFXManager.SFXOneShot(this.ticketTextChangeSfx, default(Vector3));
		yield return new WaitForSeconds(0.5f);
		yield break;
	}

	// Token: 0x06001585 RID: 5509 RVA: 0x0005C5FB File Offset: 0x0005A7FB
	public void SetImmediate()
	{
		this.sourceText.text = this._source;
		this.ticketsText.text = this._tickets;
	}

	// Token: 0x04000DD0 RID: 3536
	[SerializeField]
	private TextMeshProUGUI sourceText;

	// Token: 0x04000DD1 RID: 3537
	[SerializeField]
	private TextMeshProUGUI ticketsText;

	// Token: 0x04000DD2 RID: 3538
	[SerializeField]
	private EventReference ticketTextChangeSfx;

	// Token: 0x04000DD3 RID: 3539
	[SerializeField]
	private EventReference smallTextSlideSfx;

	// Token: 0x04000DD4 RID: 3540
	private string _source;

	// Token: 0x04000DD5 RID: 3541
	private string _tickets;
}
