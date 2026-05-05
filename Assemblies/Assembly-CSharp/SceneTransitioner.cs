using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using Extensions;
using UnityEngine;
using UnityEngine.UI;

// Token: 0x0200016A RID: 362
public class SceneTransitioner : MonoSingleton<SceneTransitioner>
{
	// Token: 0x06000DF9 RID: 3577 RVA: 0x0003A00C File Offset: 0x0003820C
	protected override void OnAwake()
	{
		base.OnAwake();
		Object.DontDestroyOnLoad(base.gameObject);
		if (this.canvasGroup)
		{
			this.canvasGroup.alpha = 0f;
			this.canvasGroup.blocksRaycasts = false;
		}
		if (this.loadingEffects)
		{
			this.loadingEffects.gameObject.SetActive(false);
		}
	}

	// Token: 0x06000DFA RID: 3578 RVA: 0x0003A074 File Offset: 0x00038274
	public void SetLoadingScreen(bool isEnabled, float duration, bool loadingScreen = true)
	{
		if (this._requestQueue.Count > 0 && this._requestQueue.Peek().enable == isEnabled)
		{
			return;
		}
		this._requestQueue.Enqueue(new SceneTransitioner.FadeRequest
		{
			enable = isEnabled,
			duration = duration,
			loadingScreen = loadingScreen
		});
		if (!this._isProcessing)
		{
			base.StartCoroutine(this.ProcessQueue());
		}
	}

	// Token: 0x06000DFB RID: 3579 RVA: 0x0003A0E4 File Offset: 0x000382E4
	private IEnumerator ProcessQueue()
	{
		this._isProcessing = true;
		while (this._requestQueue.Count > 0)
		{
			SceneTransitioner.FadeRequest request = this._requestQueue.Dequeue();
			yield return this.PlayFade(request);
		}
		this._isProcessing = false;
		yield break;
	}

	// Token: 0x06000DFC RID: 3580 RVA: 0x0003A0F3 File Offset: 0x000382F3
	private IEnumerator PlayFade(SceneTransitioner.FadeRequest request)
	{
		float endValue = request.enable ? 1f : 0f;
		if (this.canvasGroup)
		{
			this.canvasGroup.blocksRaycasts = request.enable;
		}
		bool flag = request.enable && request.loadingScreen;
		if (this.loadingEffects)
		{
			this.loadingEffects.gameObject.SetActive(flag);
			if (flag && this.loadingQuotes)
			{
				this.loadingQuotes.ShowRandomQuote();
			}
		}
		Sequence fadeSequence = this._fadeSequence;
		if (fadeSequence != null)
		{
			fadeSequence.Kill(false);
		}
		this._fadeSequence = DOTween.Sequence().SetUpdate(true);
		if (this.canvasGroup)
		{
			this._fadeSequence.Append(this.canvasGroup.DOFade(endValue, request.duration).SetEase(Ease.Linear));
		}
		yield return this._fadeSequence.WaitForCompletion();
		yield break;
	}

	// Token: 0x06000DFD RID: 3581 RVA: 0x0003A10C File Offset: 0x0003830C
	public void ForceSet(bool isEnabled)
	{
		this._requestQueue.Clear();
		Sequence fadeSequence = this._fadeSequence;
		if (fadeSequence != null)
		{
			fadeSequence.Kill(false);
		}
		float alpha = isEnabled ? 1f : 0f;
		if (this.canvasGroup)
		{
			this.canvasGroup.alpha = alpha;
			this.canvasGroup.blocksRaycasts = isEnabled;
		}
		if (this.loadingEffects)
		{
			this.loadingEffects.gameObject.SetActive(isEnabled);
			if (isEnabled && this.loadingQuotes)
			{
				this.loadingQuotes.ShowRandomQuote();
			}
		}
	}

	// Token: 0x040008D5 RID: 2261
	[SerializeField]
	private CanvasGroup canvasGroup;

	// Token: 0x040008D6 RID: 2262
	[SerializeField]
	private Image fadeImage;

	// Token: 0x040008D7 RID: 2263
	[SerializeField]
	private GameObject loadingEffects;

	// Token: 0x040008D8 RID: 2264
	[SerializeField]
	private LoadingQuotes loadingQuotes;

	// Token: 0x040008D9 RID: 2265
	private readonly Queue<SceneTransitioner.FadeRequest> _requestQueue = new Queue<SceneTransitioner.FadeRequest>();

	// Token: 0x040008DA RID: 2266
	private bool _isProcessing;

	// Token: 0x040008DB RID: 2267
	private Sequence _fadeSequence;

	// Token: 0x0200016B RID: 363
	private struct FadeRequest
	{
		// Token: 0x040008DC RID: 2268
		public bool enable;

		// Token: 0x040008DD RID: 2269
		public float duration;

		// Token: 0x040008DE RID: 2270
		public bool loadingScreen;
	}
}
