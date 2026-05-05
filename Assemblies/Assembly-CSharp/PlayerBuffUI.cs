using System;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;

// Token: 0x02000237 RID: 567
public class PlayerBuffUI : MonoBehaviour
{
	// Token: 0x06001488 RID: 5256 RVA: 0x000584FF File Offset: 0x000566FF
	private void Start()
	{
		this.drunkVolume.profile.TryGet<LensDistortion>(out this._lensDistortion);
		this.immunityVolume.profile.TryGet<Bloom>(out this._bloom);
	}

	// Token: 0x06001489 RID: 5257 RVA: 0x0005852F File Offset: 0x0005672F
	private void OnEnable()
	{
		SceneManager.sceneLoaded += this.OnSceneChanged;
	}

	// Token: 0x0600148A RID: 5258 RVA: 0x00058542 File Offset: 0x00056742
	private void OnDisable()
	{
		SceneManager.sceneLoaded -= this.OnSceneChanged;
		this.ResetAllEffects();
	}

	// Token: 0x0600148B RID: 5259 RVA: 0x0005855B File Offset: 0x0005675B
	private void OnSceneChanged(Scene scene, LoadSceneMode mode)
	{
		this.ResetAllEffects();
	}

	// Token: 0x0600148C RID: 5260 RVA: 0x00058563 File Offset: 0x00056763
	private void ResetAllEffects()
	{
		this.OnTipsyFortuneChanged(1f);
		this.OnInspiringMelodyChanged(0f);
		this.OnImmunityChanged(0f);
	}

	// Token: 0x0600148D RID: 5261 RVA: 0x00058586 File Offset: 0x00056786
	public void OnChanged(PlayerBuffType type, float value)
	{
		switch (type)
		{
		case PlayerBuffType.TipsyFortune:
			this.OnTipsyFortuneChanged(value);
			return;
		case PlayerBuffType.InspiringMelody:
			this.OnInspiringMelodyChanged(value);
			return;
		case PlayerBuffType.Immunity:
			this.OnImmunityChanged(value);
			return;
		default:
			return;
		}
	}

	// Token: 0x0600148E RID: 5262 RVA: 0x000585B4 File Offset: 0x000567B4
	private void OnTipsyFortuneChanged(float value)
	{
		bool flag = value > 1f;
		this.drunkSfx.LoopSFX(flag);
		if (this._lensDistortion == null)
		{
			return;
		}
		if (flag)
		{
			this.StartPingPong();
			return;
		}
		this.StopAndResetToZero();
	}

	// Token: 0x0600148F RID: 5263 RVA: 0x000585F8 File Offset: 0x000567F8
	private void StartPingPong()
	{
		Tween resetTween = this._resetTween;
		if (resetTween != null)
		{
			resetTween.Kill(false);
		}
		Tween loopTweenX = this._loopTweenX;
		if (loopTweenX != null)
		{
			loopTweenX.Kill(false);
		}
		this.drunkVolume.weight = 1f;
		this._lensDistortion.xMultiplier.value = 0f;
		this._lensDistortion.yMultiplier.value = 1f;
		float t = 0f;
		this._loopTweenX = DOTween.To(() => t, delegate(float v)
		{
			t = v;
			this._lensDistortion.xMultiplier.value = t;
		}, 0.8f, 2f).SetEase(Ease.InOutSine).SetLoops(-1, LoopType.Yoyo);
		this._loopTweenY = DOTween.To(() => t, delegate(float v)
		{
			t = v;
			this._lensDistortion.yMultiplier.value = 1f - t;
		}, 0.8f, 3f).SetEase(Ease.InOutSine).SetLoops(-1, LoopType.Yoyo);
	}

	// Token: 0x06001490 RID: 5264 RVA: 0x000586F0 File Offset: 0x000568F0
	private void StopAndResetToZero()
	{
		Tween loopTweenX = this._loopTweenX;
		if (loopTweenX != null)
		{
			loopTweenX.Kill(false);
		}
		Tween loopTweenY = this._loopTweenY;
		if (loopTweenY != null)
		{
			loopTweenY.Kill(false);
		}
		Tween resetTween = this._resetTween;
		if (resetTween != null)
		{
			resetTween.Kill(false);
		}
		this._resetTween = DOTween.To(() => new Vector2(this._lensDistortion.xMultiplier.value, this._lensDistortion.yMultiplier.value), delegate(Vector2 v)
		{
			this._lensDistortion.xMultiplier.value = v.x;
			this._lensDistortion.yMultiplier.value = v.y;
			this.drunkVolume.weight = Mathf.Clamp01(v.x);
		}, Vector2.zero, 0.25f).SetEase(Ease.OutCubic);
	}

	// Token: 0x06001491 RID: 5265 RVA: 0x00058767 File Offset: 0x00056967
	private void OnInspiringMelodyChanged(float value)
	{
		this.melodyEffectUI.gameObject.SetActive(value > 0f);
	}

	// Token: 0x06001492 RID: 5266 RVA: 0x00058784 File Offset: 0x00056984
	private void OnImmunityChanged(float value)
	{
		if (!this._bloom)
		{
			return;
		}
		this.immunityVolume.weight = ((value > 0f) ? 1f : 0f);
		Tween immunityTween = this._immunityTween;
		if (immunityTween != null)
		{
			immunityTween.Kill(false);
		}
		this._immunityTween = DOTween.To(() => this._bloom.intensity.value, delegate(float v)
		{
			this.immunityVolume.priority = (float)((v > 0f) ? 1 : 0);
			this._bloom.intensity.value = v;
		}, (value > 0f) ? 5f : 0f, 0.5f).SetEase(Ease.OutCubic);
	}

	// Token: 0x04000D0C RID: 3340
	public Transform melodyEffectUI;

	// Token: 0x04000D0D RID: 3341
	public Volume drunkVolume;

	// Token: 0x04000D0E RID: 3342
	private LensDistortion _lensDistortion;

	// Token: 0x04000D0F RID: 3343
	private Tween _loopTweenX;

	// Token: 0x04000D10 RID: 3344
	private Tween _loopTweenY;

	// Token: 0x04000D11 RID: 3345
	private Tween _resetTween;

	// Token: 0x04000D12 RID: 3346
	public Volume immunityVolume;

	// Token: 0x04000D13 RID: 3347
	private Bloom _bloom;

	// Token: 0x04000D14 RID: 3348
	private Tween _immunityTween;

	// Token: 0x04000D15 RID: 3349
	[Header("SFX")]
	[SerializeField]
	private SFXLocalLoopComponent drunkSfx;
}
