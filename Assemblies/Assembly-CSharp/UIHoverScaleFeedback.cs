using System;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using UnityEngine;
using UnityEngine.EventSystems;

// Token: 0x020002FF RID: 767
[RequireComponent(typeof(RectTransform))]
public class UIHoverScaleFeedback : MonoBehaviour, IPointerEnterHandler, IEventSystemHandler, IPointerExitHandler
{
	// Token: 0x06001A54 RID: 6740 RVA: 0x0006F42C File Offset: 0x0006D62C
	private void Awake()
	{
		this.rectTransform = base.GetComponent<RectTransform>();
		this.originalScale = this.rectTransform.localScale;
	}

	// Token: 0x06001A55 RID: 6741 RVA: 0x0006F44B File Offset: 0x0006D64B
	public void SetHasClicked(bool value)
	{
		this._hasClicked = value;
	}

	// Token: 0x06001A56 RID: 6742 RVA: 0x0006F454 File Offset: 0x0006D654
	public void OnPointerEnter(PointerEventData eventData)
	{
		if (this._hasClicked)
		{
			return;
		}
		this.currentTween = this.rectTransform.DOScale(this.hoverScale, this.tweenDuration).SetEase(this.tweenEase);
	}

	// Token: 0x06001A57 RID: 6743 RVA: 0x0006F487 File Offset: 0x0006D687
	public void OnPointerExit(PointerEventData eventData)
	{
		if (this._hasClicked)
		{
			return;
		}
		this.currentTween = this.rectTransform.DOScale(this.originalScale, this.tweenDuration).SetEase(this.tweenEase);
	}

	// Token: 0x06001A58 RID: 6744 RVA: 0x0006F4BA File Offset: 0x0006D6BA
	private void OnDisable()
	{
		this.rectTransform.localScale = this.originalScale;
	}

	// Token: 0x04001108 RID: 4360
	[Header("Scale Settings")]
	[Tooltip("Target scale when hovered")]
	public Vector3 hoverScale = new Vector3(1.1f, 1.1f, 1f);

	// Token: 0x04001109 RID: 4361
	[Tooltip("Duration of the scale tween")]
	public float tweenDuration = 0.2f;

	// Token: 0x0400110A RID: 4362
	[Tooltip("Ease type used for scaling")]
	public Ease tweenEase = Ease.OutBack;

	// Token: 0x0400110B RID: 4363
	private bool _hasClicked;

	// Token: 0x0400110C RID: 4364
	private RectTransform rectTransform;

	// Token: 0x0400110D RID: 4365
	private Vector3 originalScale;

	// Token: 0x0400110E RID: 4366
	private Tween currentTween;
}
