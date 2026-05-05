using System;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using UnityEngine;

// Token: 0x02000244 RID: 580
public class DiscordToasterUI : MonoBehaviour
{
	// Token: 0x060014E5 RID: 5349 RVA: 0x00059A9C File Offset: 0x00057C9C
	private void Awake()
	{
		if (this.toasterRoot)
		{
			Vector2 anchoredPosition = this.toasterRoot.anchoredPosition;
			anchoredPosition.y = this.hiddenAnchorY;
			this.toasterRoot.anchoredPosition = anchoredPosition;
		}
	}

	// Token: 0x060014E6 RID: 5350 RVA: 0x00059ADB File Offset: 0x00057CDB
	private void OnDisable()
	{
		Tween activeTween = this._activeTween;
		if (activeTween == null)
		{
			return;
		}
		activeTween.Kill(false);
	}

	// Token: 0x060014E7 RID: 5351 RVA: 0x00059AF0 File Offset: 0x00057CF0
	public void SlideIn()
	{
		if (!this.toasterRoot)
		{
			return;
		}
		Tween activeTween = this._activeTween;
		if (activeTween != null)
		{
			activeTween.Kill(false);
		}
		base.gameObject.SetActive(true);
		this.toasterRoot.anchoredPosition = new Vector2(this.toasterRoot.anchoredPosition.x, this.hiddenAnchorY);
		this._activeTween = this.toasterRoot.DOAnchorPosY(this.visibleAnchorY, this.animateInDuration, false).SetEase(this.easeIn).SetTarget(this.toasterRoot);
	}

	// Token: 0x060014E8 RID: 5352 RVA: 0x00059B84 File Offset: 0x00057D84
	public void SlideOut()
	{
		if (!this.toasterRoot)
		{
			return;
		}
		Tween activeTween = this._activeTween;
		if (activeTween != null)
		{
			activeTween.Kill(false);
		}
		this._activeTween = this.toasterRoot.DOAnchorPosY(this.hiddenAnchorY, this.animateOutDuration, false).SetEase(this.easeOut).SetTarget(this.toasterRoot).OnComplete(delegate
		{
			base.gameObject.SetActive(false);
		});
	}

	// Token: 0x04000D4E RID: 3406
	[Header("Toaster")]
	[SerializeField]
	private RectTransform toasterRoot;

	// Token: 0x04000D4F RID: 3407
	[SerializeField]
	private float visibleAnchorY = 80f;

	// Token: 0x04000D50 RID: 3408
	[SerializeField]
	private float hiddenAnchorY = -400f;

	// Token: 0x04000D51 RID: 3409
	[SerializeField]
	private float animateInDuration = 0.4f;

	// Token: 0x04000D52 RID: 3410
	[SerializeField]
	private float animateOutDuration = 0.3f;

	// Token: 0x04000D53 RID: 3411
	[SerializeField]
	private Ease easeIn = Ease.OutBack;

	// Token: 0x04000D54 RID: 3412
	[SerializeField]
	private Ease easeOut = Ease.InBack;

	// Token: 0x04000D55 RID: 3413
	private Tween _activeTween;
}
