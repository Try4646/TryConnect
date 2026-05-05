using System;
using System.Collections;
using DG.Tweening;
using FMODUnity;
using TMPro;
using UnityEngine;

// Token: 0x02000264 RID: 612
public class GameLostContributionEntry : MonoBehaviour
{
	// Token: 0x060015BF RID: 5567 RVA: 0x0005DB3F File Offset: 0x0005BD3F
	public void Setup(string playerName, long contribution)
	{
		this._playerName = playerName;
		this._contribution = contribution;
		this._colorPalette = Resources.Load<UIColorPalette>("ColorSettings");
	}

	// Token: 0x060015C0 RID: 5568 RVA: 0x0005DB5F File Offset: 0x0005BD5F
	public IEnumerator Animate(float duration)
	{
		this.playerNameText.text = this._playerName;
		yield return new WaitForSeconds(duration);
		this.contributionText.color = ((this._contribution >= 0L) ? this._colorPalette.profitGreen : this._colorPalette.lossRed);
		DOVirtual.Float(0f, 1f, duration, delegate(float t)
		{
			double a = (double)((float)this._contribution * t);
			this.contributionText.text = (MoneyFormatter.FormatWithDollar((long)Math.Round(a)) ?? "");
			if (this._contribution != 0L)
			{
				SFXManager.SFXOneShot(this.textChangeSfx, default(Vector3));
			}
		}).SetEase(Ease.OutCubic).OnComplete(delegate
		{
			this.contributionText.transform.DOPunchScale(this.contributionText.transform.localScale * 0.2f, 0.5f, 1, 1f);
		});
		yield return new WaitForSeconds(duration + 0.5f);
		yield break;
	}

	// Token: 0x060015C1 RID: 5569 RVA: 0x0005DB78 File Offset: 0x0005BD78
	public void SetImmediate()
	{
		this.playerNameText.text = this._playerName;
		this.contributionText.color = ((this._contribution >= 0L) ? this._colorPalette.profitGreen : this._colorPalette.lossRed);
		this.contributionText.text = (MoneyFormatter.FormatWithDollar(this._contribution) ?? "");
	}

	// Token: 0x04000E24 RID: 3620
	[SerializeField]
	private TextMeshProUGUI playerNameText;

	// Token: 0x04000E25 RID: 3621
	[SerializeField]
	private TextMeshProUGUI contributionText;

	// Token: 0x04000E26 RID: 3622
	[SerializeField]
	private EventReference textChangeSfx;

	// Token: 0x04000E27 RID: 3623
	private string _playerName;

	// Token: 0x04000E28 RID: 3624
	private long _contribution;

	// Token: 0x04000E29 RID: 3625
	private UIColorPalette _colorPalette;
}
