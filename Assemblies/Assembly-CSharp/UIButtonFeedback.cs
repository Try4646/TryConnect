using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Token: 0x020002FD RID: 765
[DisallowMultipleComponent]
public class UIButtonFeedback : MonoBehaviour
{
	// Token: 0x06001A44 RID: 6724 RVA: 0x0006ECCB File Offset: 0x0006CECB
	private void Awake()
	{
		this._baseScale = base.transform.localScale;
		this.CacheChildren();
		this.CacheBaseValues();
	}

	// Token: 0x06001A45 RID: 6725 RVA: 0x0006ECEA File Offset: 0x0006CEEA
	public void PlayFeedback()
	{
		this.StartLerp(true);
	}

	// Token: 0x06001A46 RID: 6726 RVA: 0x0006ECF3 File Offset: 0x0006CEF3
	public void RevertFeedback()
	{
		this.StartLerp(false);
	}

	// Token: 0x06001A47 RID: 6727 RVA: 0x0006ECFC File Offset: 0x0006CEFC
	private void OnDisable()
	{
		if (this._activeLerp != null)
		{
			base.StopCoroutine(this._activeLerp);
			this._activeLerp = null;
		}
		this.ApplyInstantBaseValues();
	}

	// Token: 0x06001A48 RID: 6728 RVA: 0x0006ED20 File Offset: 0x0006CF20
	private void CacheChildren()
	{
		List<Image> list = new List<Image>();
		Image[] componentsInChildren = base.GetComponentsInChildren<Image>(true);
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			if (componentsInChildren[i].transform != base.transform)
			{
				list.Add(componentsInChildren[i]);
			}
		}
		this._childImages = list.ToArray();
		List<TMP_Text> list2 = new List<TMP_Text>();
		TMP_Text[] componentsInChildren2 = base.GetComponentsInChildren<TMP_Text>(true);
		for (int j = 0; j < componentsInChildren2.Length; j++)
		{
			if (componentsInChildren2[j].transform != base.transform)
			{
				list2.Add(componentsInChildren2[j]);
			}
		}
		this._childTmpTexts = list2.ToArray();
		List<Text> list3 = new List<Text>();
		Text[] componentsInChildren3 = base.GetComponentsInChildren<Text>(true);
		for (int k = 0; k < componentsInChildren3.Length; k++)
		{
			if (componentsInChildren3[k].transform != base.transform)
			{
				list3.Add(componentsInChildren3[k]);
			}
		}
		this._childLegacyTexts = list3.ToArray();
	}

	// Token: 0x06001A49 RID: 6729 RVA: 0x0006EE18 File Offset: 0x0006D018
	private void CacheBaseValues()
	{
		this._baseImageColors = new Color[this._childImages.Length];
		for (int i = 0; i < this._childImages.Length; i++)
		{
			this._baseImageColors[i] = this._childImages[i].color;
		}
		this._baseTmpTextColors = new Color[this._childTmpTexts.Length];
		for (int j = 0; j < this._childTmpTexts.Length; j++)
		{
			this._baseTmpTextColors[j] = this._childTmpTexts[j].color;
		}
		this._baseLegacyTextColors = new Color[this._childLegacyTexts.Length];
		for (int k = 0; k < this._childLegacyTexts.Length; k++)
		{
			this._baseLegacyTextColors[k] = this._childLegacyTexts[k].color;
		}
	}

	// Token: 0x06001A4A RID: 6730 RVA: 0x0006EEE2 File Offset: 0x0006D0E2
	private void StartLerp(bool toFeedback)
	{
		if (this._activeLerp != null)
		{
			base.StopCoroutine(this._activeLerp);
		}
		this._activeLerp = base.StartCoroutine(this.LerpRoutine(toFeedback));
	}

	// Token: 0x06001A4B RID: 6731 RVA: 0x0006EF0B File Offset: 0x0006D10B
	private IEnumerator LerpRoutine(bool toFeedback)
	{
		Vector3 fromScale = base.transform.localScale;
		Vector3 toScale = toFeedback ? this.feedbackScale : this._baseScale;
		Color[] fromImageColors = new Color[this._childImages.Length];
		Color[] toImageColors = new Color[this._childImages.Length];
		for (int i = 0; i < this._childImages.Length; i++)
		{
			fromImageColors[i] = this._childImages[i].color;
			Color color = this._baseImageColors[i];
			toImageColors[i] = (toFeedback ? new Color(color.r, color.g, color.b, 1f) : color);
		}
		Color[] fromTmpColors = new Color[this._childTmpTexts.Length];
		Color[] toTmpColors = new Color[this._childTmpTexts.Length];
		for (int j = 0; j < this._childTmpTexts.Length; j++)
		{
			fromTmpColors[j] = this._childTmpTexts[j].color;
			Color color2 = this._baseTmpTextColors[j];
			toTmpColors[j] = (toFeedback ? new Color(0f, 0f, 0f, color2.a) : color2);
		}
		Color[] fromLegacyColors = new Color[this._childLegacyTexts.Length];
		Color[] toLegacyColors = new Color[this._childLegacyTexts.Length];
		for (int k = 0; k < this._childLegacyTexts.Length; k++)
		{
			fromLegacyColors[k] = this._childLegacyTexts[k].color;
			Color color3 = this._baseLegacyTextColors[k];
			toLegacyColors[k] = (toFeedback ? new Color(0f, 0f, 0f, color3.a) : color3);
		}
		float elapsed = 0f;
		while (elapsed < this.lerpDuration)
		{
			elapsed += Time.unscaledDeltaTime;
			float t = Mathf.Clamp01(elapsed / this.lerpDuration);
			base.transform.localScale = Vector3.Lerp(fromScale, toScale, t);
			for (int l = 0; l < this._childImages.Length; l++)
			{
				this._childImages[l].color = Color.Lerp(fromImageColors[l], toImageColors[l], t);
			}
			for (int m = 0; m < this._childTmpTexts.Length; m++)
			{
				this._childTmpTexts[m].color = Color.Lerp(fromTmpColors[m], toTmpColors[m], t);
			}
			for (int n = 0; n < this._childLegacyTexts.Length; n++)
			{
				this._childLegacyTexts[n].color = Color.Lerp(fromLegacyColors[n], toLegacyColors[n], t);
			}
			yield return null;
		}
		base.transform.localScale = toScale;
		for (int num = 0; num < this._childImages.Length; num++)
		{
			this._childImages[num].color = toImageColors[num];
		}
		for (int num2 = 0; num2 < this._childTmpTexts.Length; num2++)
		{
			this._childTmpTexts[num2].color = toTmpColors[num2];
		}
		for (int num3 = 0; num3 < this._childLegacyTexts.Length; num3++)
		{
			this._childLegacyTexts[num3].color = toLegacyColors[num3];
		}
		this._activeLerp = null;
		yield break;
	}

	// Token: 0x06001A4C RID: 6732 RVA: 0x0006EF24 File Offset: 0x0006D124
	private void ApplyInstantBaseValues()
	{
		base.transform.localScale = this._baseScale;
		for (int i = 0; i < this._childImages.Length; i++)
		{
			this._childImages[i].color = this._baseImageColors[i];
		}
		for (int j = 0; j < this._childTmpTexts.Length; j++)
		{
			this._childTmpTexts[j].color = this._baseTmpTextColors[j];
		}
		for (int k = 0; k < this._childLegacyTexts.Length; k++)
		{
			this._childLegacyTexts[k].color = this._baseLegacyTextColors[k];
		}
	}

	// Token: 0x040010F1 RID: 4337
	[SerializeField]
	private float lerpDuration = 0.12f;

	// Token: 0x040010F2 RID: 4338
	[SerializeField]
	private Vector3 feedbackScale = new Vector3(1.05f, 1.05f, 1f);

	// Token: 0x040010F3 RID: 4339
	private Image[] _childImages;

	// Token: 0x040010F4 RID: 4340
	private TMP_Text[] _childTmpTexts;

	// Token: 0x040010F5 RID: 4341
	private Text[] _childLegacyTexts;

	// Token: 0x040010F6 RID: 4342
	private Color[] _baseImageColors;

	// Token: 0x040010F7 RID: 4343
	private Color[] _baseTmpTextColors;

	// Token: 0x040010F8 RID: 4344
	private Color[] _baseLegacyTextColors;

	// Token: 0x040010F9 RID: 4345
	private Vector3 _baseScale;

	// Token: 0x040010FA RID: 4346
	private Coroutine _activeLerp;
}
