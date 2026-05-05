using System;
using System.Collections.Generic;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using FMODUnity;
using MoreMountains.Feedbacks;
using UnityEngine;

// Token: 0x02000089 RID: 137
public class CasinoGameFeedbacks : MonoBehaviour
{
	// Token: 0x060004DF RID: 1247 RVA: 0x00015C84 File Offset: 0x00013E84
	private void Awake()
	{
		if (this.lightRenderers.Length != 0)
		{
			this._mpbsPerRenderer = new MaterialPropertyBlock[this.lightRenderers.Length][];
			this._defaultColorsPerRenderer = new Color[this.lightRenderers.Length][];
			this._startColorsPerRenderer = new Color[this.lightRenderers.Length][];
			for (int i = 0; i < this.lightRenderers.Length; i++)
			{
				Renderer renderer = this.lightRenderers[i];
				int num = renderer.sharedMaterials.Length;
				this._mpbsPerRenderer[i] = new MaterialPropertyBlock[num];
				this._defaultColorsPerRenderer[i] = new Color[num];
				this._startColorsPerRenderer[i] = new Color[num];
				for (int j = 0; j < num; j++)
				{
					this._mpbsPerRenderer[i][j] = new MaterialPropertyBlock();
					this._defaultColorsPerRenderer[i][j] = renderer.sharedMaterials[j].GetColor("_EmissionColor");
				}
			}
		}
		if (this.lights.Length != 0)
		{
			this._defaultLightColors = new Color[this.lights.Length];
			this._startLightColors = new Color[this.lights.Length];
			for (int k = 0; k < this.lights.Length; k++)
			{
				this._defaultLightColors[k] = this.lights[k].color;
			}
		}
	}

	// Token: 0x060004E0 RID: 1248 RVA: 0x00015DC4 File Offset: 0x00013FC4
	public void PlayGameResultFeedback(double multiplier)
	{
		this.LightFeedbacks(multiplier);
		this.SfxFeedback(multiplier);
		this.GameFeedback(multiplier);
	}

	// Token: 0x060004E1 RID: 1249 RVA: 0x00015DDC File Offset: 0x00013FDC
	private void LightFeedbacks(double multiplier)
	{
		if (this.lightRenderers.Length == 0 && this.lights.Length == 0)
		{
			return;
		}
		if (this.lightFeedbacks.Count <= 0)
		{
			return;
		}
		CasinoGameFeedbacks.LightFeedbackData data = this.lightFeedbacks[0];
		foreach (CasinoGameFeedbacks.LightFeedbackData lightFeedbackData in this.lightFeedbacks)
		{
			if ((double)lightFeedbackData.threshold > multiplier)
			{
				break;
			}
			data = lightFeedbackData;
		}
		if (this._lightTween != null && this._lightTween.IsActive())
		{
			this._lightTween.Kill(false);
		}
		this.StartLightTween(data);
	}

	// Token: 0x060004E2 RID: 1250 RVA: 0x00015E8C File Offset: 0x0001408C
	private void StartLightTween(CasinoGameFeedbacks.LightFeedbackData data)
	{
		Tween lightTween = this._lightTween;
		if (lightTween != null)
		{
			lightTween.Kill(false);
		}
		for (int i = 0; i < this.lightRenderers.Length; i++)
		{
			Renderer renderer = this.lightRenderers[i];
			int num = renderer.sharedMaterials.Length;
			for (int j = 0; j < num; j++)
			{
				MaterialPropertyBlock materialPropertyBlock = this._mpbsPerRenderer[i][j];
				renderer.GetPropertyBlock(materialPropertyBlock);
				this._startColorsPerRenderer[i][j] = materialPropertyBlock.GetColor("_EmissionColor");
			}
		}
		for (int k = 0; k < this.lights.Length; k++)
		{
			this._startLightColors[k] = this.lights[k].color;
		}
		float t = 0f;
		Color target = data.color * data.intensity;
		this._lightTween = DOTween.Sequence().Append(DOTween.To(() => t, delegate(float x)
		{
			t = x;
			for (int l = 0; l < this.lightRenderers.Length; l++)
			{
				Renderer renderer2 = this.lightRenderers[l];
				int num2 = renderer2.sharedMaterials.Length;
				for (int m = 0; m < num2; m++)
				{
					MaterialPropertyBlock materialPropertyBlock2 = this._mpbsPerRenderer[l][m];
					Color value = Color.Lerp(this._startColorsPerRenderer[l][m], target, t);
					materialPropertyBlock2.SetColor("_EmissionColor", value);
					renderer2.SetPropertyBlock(materialPropertyBlock2, m);
				}
			}
			for (int n = 0; n < this.lights.Length; n++)
			{
				this.lights[n].color = Color.Lerp(this._startLightColors[n], target, t);
			}
		}, 1f, this.lightTweenDuration).SetEase(Ease.OutQuad)).AppendInterval(data.duration).Append(DOTween.To(() => t, delegate(float x)
		{
			t = x;
			for (int l = 0; l < this.lightRenderers.Length; l++)
			{
				Renderer renderer2 = this.lightRenderers[l];
				int num2 = renderer2.sharedMaterials.Length;
				for (int m = 0; m < num2; m++)
				{
					MaterialPropertyBlock materialPropertyBlock2 = this._mpbsPerRenderer[l][m];
					Color value = Color.Lerp(this._defaultColorsPerRenderer[l][m], target, t);
					materialPropertyBlock2.SetColor("_EmissionColor", value);
					renderer2.SetPropertyBlock(materialPropertyBlock2, m);
				}
			}
			for (int n = 0; n < this.lights.Length; n++)
			{
				this.lights[n].color = Color.Lerp(this._defaultLightColors[n], target, t);
			}
		}, 0f, this.lightTweenDuration).SetEase(Ease.InQuad));
	}

	// Token: 0x060004E3 RID: 1251 RVA: 0x00015FF0 File Offset: 0x000141F0
	private void GameFeedback(double multiplier)
	{
		ParticleSystem[] array = this.resultParticles;
		if (array != null && array.Length > 0 && this.lightFeedbacks.Count > 0)
		{
			CasinoGameFeedbacks.LightFeedbackData lightFeedbackData = this.lightFeedbacks[0];
			foreach (CasinoGameFeedbacks.LightFeedbackData lightFeedbackData2 in this.lightFeedbacks)
			{
				if ((double)lightFeedbackData2.threshold > multiplier)
				{
					break;
				}
				lightFeedbackData = lightFeedbackData2;
			}
			array = this.resultParticles;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].main.startColor = lightFeedbackData.color;
			}
		}
		this.onGameResultFeedback.PlayFeedbacks();
		if (multiplier > 1.0)
		{
			this.onGameWinFeedback.PlayFeedbacks();
			return;
		}
		if (multiplier < 1.0)
		{
			this.onGameLoseFeedback.PlayFeedbacks();
			return;
		}
		this.onGameTieFeedback.PlayFeedbacks();
	}

	// Token: 0x060004E4 RID: 1252 RVA: 0x000160F8 File Offset: 0x000142F8
	private void SfxFeedback(double multiplier)
	{
		if (multiplier > 1.0)
		{
			SFXParams[] sFXParams = new SFXParams[]
			{
				new SFXParams("Multiplier", Mathf.Clamp((float)multiplier, 0f, 1000f))
			};
			SFXManager.SFXOneShotWithParameters(this.winSFX, sFXParams, base.transform.position, 1f);
			return;
		}
		if (multiplier < 1.0)
		{
			SFXManager.SFXOneShot(this.loseSFX, base.transform.position);
			return;
		}
		SFXManager.SFXOneShot(this.tieSFX, base.transform.position);
	}

	// Token: 0x04000350 RID: 848
	[Header("References")]
	[SerializeField]
	private Renderer[] lightRenderers;

	// Token: 0x04000351 RID: 849
	[SerializeField]
	private Light[] lights;

	// Token: 0x04000352 RID: 850
	[SerializeField]
	private List<CasinoGameFeedbacks.LightFeedbackData> lightFeedbacks;

	// Token: 0x04000353 RID: 851
	[SerializeField]
	private MMF_Player onGameResultFeedback;

	// Token: 0x04000354 RID: 852
	[SerializeField]
	private MMF_Player onGameWinFeedback;

	// Token: 0x04000355 RID: 853
	[SerializeField]
	private MMF_Player onGameLoseFeedback;

	// Token: 0x04000356 RID: 854
	[SerializeField]
	private MMF_Player onGameTieFeedback;

	// Token: 0x04000357 RID: 855
	[SerializeField]
	private ParticleSystem[] resultParticles;

	// Token: 0x04000358 RID: 856
	[Header("Settings")]
	[SerializeField]
	private float lightTweenDuration;

	// Token: 0x04000359 RID: 857
	[SerializeField]
	private Gradient onWinColor;

	// Token: 0x0400035A RID: 858
	[SerializeField]
	private Gradient onLoseColor;

	// Token: 0x0400035B RID: 859
	[SerializeField]
	private Gradient onTieColor;

	// Token: 0x0400035C RID: 860
	[Header("SFX")]
	[SerializeField]
	private EventReference winSFX;

	// Token: 0x0400035D RID: 861
	[SerializeField]
	private EventReference loseSFX;

	// Token: 0x0400035E RID: 862
	[SerializeField]
	private EventReference tieSFX;

	// Token: 0x0400035F RID: 863
	private MaterialPropertyBlock[][] _mpbsPerRenderer;

	// Token: 0x04000360 RID: 864
	private Color[][] _defaultColorsPerRenderer;

	// Token: 0x04000361 RID: 865
	private Color[][] _startColorsPerRenderer;

	// Token: 0x04000362 RID: 866
	private Color[] _defaultLightColors;

	// Token: 0x04000363 RID: 867
	private Color[] _startLightColors;

	// Token: 0x04000364 RID: 868
	private Tween _lightTween;

	// Token: 0x0200008A RID: 138
	[Serializable]
	public struct LightFeedbackData
	{
		// Token: 0x04000365 RID: 869
		public float threshold;

		// Token: 0x04000366 RID: 870
		public Color color;

		// Token: 0x04000367 RID: 871
		public float intensity;

		// Token: 0x04000368 RID: 872
		public float duration;
	}
}
