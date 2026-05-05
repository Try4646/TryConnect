using System;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using UnityEngine;

// Token: 0x020000F6 RID: 246
public class Plush : Item
{
	// Token: 0x060009FD RID: 2557 RVA: 0x00027FE4 File Offset: 0x000261E4
	protected override void OnUseItem(bool isPressed)
	{
		if (!isPressed)
		{
			return;
		}
		this.Squeeze();
		if (base.NetworkHolder && base.NetworkHolder.isLocalPlayer && this.polleSfx)
		{
			this.polleSfx.PlayPolleSays();
		}
	}

	// Token: 0x060009FE RID: 2558 RVA: 0x00028024 File Offset: 0x00026224
	private void Squeeze()
	{
		Tween scaleTween = this._scaleTween;
		if (scaleTween != null)
		{
			scaleTween.Kill(false);
		}
		Sequence sequence = DOTween.Sequence();
		sequence.Append(this.modelTransform.DOScale(new Vector3(1.1f, 0.9f, 0.9f), 0.05f).SetEase(Ease.OutQuad));
		sequence.Append(this.modelTransform.DOScale(new Vector3(0.7f, 1.1f, 1.1f), 0.1f).SetEase(Ease.OutQuad));
		sequence.Append(this.modelTransform.DOScale(Vector3.one, 0.4f).SetEase(Ease.OutElastic, 1.5f, 0.3f));
		this._scaleTween = sequence;
		if (this.squeezeSfx)
		{
			this.squeezeSfx.PlayOneShotAttached();
		}
	}

	// Token: 0x06000A00 RID: 2560 RVA: 0x00002321 File Offset: 0x00000521
	public override bool Weaved()
	{
		return true;
	}

	// Token: 0x04000657 RID: 1623
	[SerializeField]
	private SFXComponent squeezeSfx;

	// Token: 0x04000658 RID: 1624
	[SerializeField]
	private PolleSFX polleSfx;

	// Token: 0x04000659 RID: 1625
	private Tween _scaleTween;
}
