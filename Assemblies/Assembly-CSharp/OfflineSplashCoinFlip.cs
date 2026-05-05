using System;
using System.Collections;
using DG.Tweening;
using Extensions;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

// Token: 0x0200003D RID: 61
public class OfflineSplashCoinFlip : MonoBehaviour
{
	// Token: 0x06000153 RID: 339 RVA: 0x000086EF File Offset: 0x000068EF
	private void Awake()
	{
		if (this.sceneSkipperToDisable != null)
		{
			this.sceneSkipperToDisable.ignoreInput = true;
		}
	}

	// Token: 0x06000154 RID: 340 RVA: 0x0000870B File Offset: 0x0000690B
	public void OnChooseNo()
	{
		if (this._choiceLocked)
		{
			return;
		}
		this._choiceLocked = this.disableInputAfterChoice;
		SteamAchievement_SteamworksNET steamAchievement_SteamworksNET = this.onChooseNo;
		if (steamAchievement_SteamworksNET != null)
		{
			steamAchievement_SteamworksNET.UnlockAchievement();
		}
		this.LoadNextScene();
		this.FadeOutText();
	}

	// Token: 0x06000155 RID: 341 RVA: 0x00008740 File Offset: 0x00006940
	public void OnChooseYes()
	{
		if (this._choiceLocked)
		{
			return;
		}
		this._choiceLocked = this.disableInputAfterChoice;
		if (this.coin == null)
		{
			this.ResolveResult(Random.value > 0.5f);
			return;
		}
		if (this._checkStopCoroutine != null)
		{
			base.StopCoroutine(this._checkStopCoroutine);
		}
		this.FlipCoin();
		this._checkStopCoroutine = base.StartCoroutine(this.CheckCoinStoppedRoutine());
		this.FadeOutText();
	}

	// Token: 0x06000156 RID: 342 RVA: 0x000087B5 File Offset: 0x000069B5
	private void FadeOutText()
	{
		this.questionCanvasGroup.DOFade(0f, 0.5f);
	}

	// Token: 0x06000157 RID: 343 RVA: 0x000087D0 File Offset: 0x000069D0
	private void FlipCoin()
	{
		this.coinFlipSfx.LoopSFX(true);
		this.heartLoop.LoopSFX(true);
		this.coin.linearVelocity = Vector3.zero;
		this.coin.angularVelocity = Vector3.zero;
		float d = Random.Range(this.minFlipForce, this.maxFlipForce);
		this.coin.AddForce(Vector3.up * d, ForceMode.VelocityChange);
		float f = Random.value * 3.1415927f * 2f;
		Vector3 normalized = new Vector3(Mathf.Cos(f), 0f, Mathf.Sin(f)).normalized;
		float d2 = Random.Range(this.minFlipTorque, this.maxFlipTorque);
		this.coin.AddTorque(normalized * d2, ForceMode.VelocityChange);
	}

	// Token: 0x06000158 RID: 344 RVA: 0x00008895 File Offset: 0x00006A95
	private IEnumerator CheckCoinStoppedRoutine()
	{
		yield return new WaitForSeconds(this.firstCheckDelay);
		float startTime = Time.time;
		while (Time.time - startTime < this.maxDuration)
		{
			yield return new WaitForSeconds(this.checkInterval);
			if (this.coin.transform.localPosition.y <= this.coinHeightThreshold && this.coin.linearVelocity.sqrMagnitude <= this.stopVelocityThreshold * this.stopVelocityThreshold && this.coin.angularVelocity.sqrMagnitude <= this.stopAngularVelocityThreshold * this.stopAngularVelocityThreshold)
			{
				break;
			}
		}
		bool isHeads = Vector3.Angle(this.coin.transform.up, Vector3.up) <= 90f;
		this.ResolveResult(isHeads);
		this.coinFlipSfx.LoopSFX(false);
		this.heartLoop.LoopSFX(false);
		yield break;
	}

	// Token: 0x06000159 RID: 345 RVA: 0x000088A4 File Offset: 0x00006AA4
	private void ResolveResult(bool isHeads)
	{
		bool flag = isHeads == this.headsQuitsGame;
		string message = flag ? this.loseMessage : this.winMessage;
		if (flag)
		{
			SteamAchievement_SteamworksNET steamAchievement_SteamworksNET = this.onChooseYesLose;
			if (steamAchievement_SteamworksNET != null)
			{
				steamAchievement_SteamworksNET.UnlockAchievement();
			}
			this.gameLoseSfx.PlayOneShotWith3DPos();
			this.boomSfx.PlayOneShotWith3DPos();
		}
		else
		{
			SteamAchievement_SteamworksNET steamAchievement_SteamworksNET2 = this.onChooseYesWin;
			if (steamAchievement_SteamworksNET2 != null)
			{
				steamAchievement_SteamworksNET2.UnlockAchievement();
			}
			this.gameWinSfx.PlayOneShotOverrideParams();
		}
		base.StartCoroutine(this.ShowResultAndContinueRoutine(message, flag));
	}

	// Token: 0x0600015A RID: 346 RVA: 0x00008924 File Offset: 0x00006B24
	private IEnumerator ShowResultAndContinueRoutine(string message, bool shouldQuit)
	{
		if (this.resultText != null)
		{
			this.resultText.text = message;
		}
		if (this.resultCanvasGroup != null)
		{
			this.resultCanvasGroup.DOFade(1f, this.resultFadeDuration);
		}
		yield return new WaitForSeconds(this.resultDisplayDuration);
		if (shouldQuit)
		{
			this.QuitGame();
			yield break;
		}
		this.LoadNextScene();
		yield break;
	}

	// Token: 0x0600015B RID: 347 RVA: 0x00008944 File Offset: 0x00006B44
	private void LoadNextScene()
	{
		if (this._isTransitioning)
		{
			return;
		}
		int num = SceneManager.GetActiveScene().buildIndex + 1;
		if (num < SceneManager.sceneCountInBuildSettings)
		{
			this._isTransitioning = true;
			base.StartCoroutine(this.TransitionAndLoadRoutine(num));
			return;
		}
		Debug.LogWarning("No more scenes to load. Check your Build Settings.");
	}

	// Token: 0x0600015C RID: 348 RVA: 0x00008992 File Offset: 0x00006B92
	private IEnumerator TransitionAndLoadRoutine(int nextSceneIndex)
	{
		if (MonoSingleton<SceneTransitioner>.Instance != null)
		{
			MonoSingleton<SceneTransitioner>.Instance.SetLoadingScreen(true, this.transitionDuration, false);
			yield return new WaitForSeconds(this.transitionDuration);
		}
		SceneManager.LoadScene(nextSceneIndex);
		yield break;
	}

	// Token: 0x0600015D RID: 349 RVA: 0x000089A8 File Offset: 0x00006BA8
	private void QuitGame()
	{
		Application.Quit();
	}

	// Token: 0x0400011B RID: 283
	[Header("Optional References")]
	[SerializeField]
	private Rigidbody coin;

	// Token: 0x0400011C RID: 284
	[SerializeField]
	private SceneSkipper sceneSkipperToDisable;

	// Token: 0x0400011D RID: 285
	[SerializeField]
	private CanvasGroup questionCanvasGroup;

	// Token: 0x0400011E RID: 286
	[SerializeField]
	private CanvasGroup resultCanvasGroup;

	// Token: 0x0400011F RID: 287
	[SerializeField]
	private TMP_Text resultText;

	// Token: 0x04000120 RID: 288
	[Header("Throw Settings")]
	[SerializeField]
	private float minFlipForce = 10f;

	// Token: 0x04000121 RID: 289
	[SerializeField]
	private float maxFlipForce = 20f;

	// Token: 0x04000122 RID: 290
	[SerializeField]
	private float minFlipTorque = 2f;

	// Token: 0x04000123 RID: 291
	[SerializeField]
	private float maxFlipTorque = 4f;

	// Token: 0x04000124 RID: 292
	[Header("Stop Check Settings")]
	[SerializeField]
	private float firstCheckDelay = 1f;

	// Token: 0x04000125 RID: 293
	[SerializeField]
	private float checkInterval = 0.1f;

	// Token: 0x04000126 RID: 294
	[SerializeField]
	private float maxDuration = 10f;

	// Token: 0x04000127 RID: 295
	[SerializeField]
	private float stopVelocityThreshold = 0.1f;

	// Token: 0x04000128 RID: 296
	[SerializeField]
	private float stopAngularVelocityThreshold = 0.1f;

	// Token: 0x04000129 RID: 297
	[SerializeField]
	private float coinHeightThreshold = 1.25f;

	// Token: 0x0400012A RID: 298
	[Header("Result Settings")]
	[SerializeField]
	private bool headsQuitsGame = true;

	// Token: 0x0400012B RID: 299
	[SerializeField]
	private bool disableInputAfterChoice = true;

	// Token: 0x0400012C RID: 300
	[SerializeField]
	private float transitionDuration = 0.5f;

	// Token: 0x0400012D RID: 301
	[SerializeField]
	private float resultFadeDuration = 0.35f;

	// Token: 0x0400012E RID: 302
	[SerializeField]
	private float resultDisplayDuration = 1.2f;

	// Token: 0x0400012F RID: 303
	[SerializeField]
	private string loseMessage = "Uh oh, you lost sucka.";

	// Token: 0x04000130 RID: 304
	[SerializeField]
	private string winMessage = "You win this time.";

	// Token: 0x04000131 RID: 305
	[Header("Achievements")]
	public SteamAchievement_SteamworksNET onChooseNo;

	// Token: 0x04000132 RID: 306
	public SteamAchievement_SteamworksNET onChooseYesWin;

	// Token: 0x04000133 RID: 307
	public SteamAchievement_SteamworksNET onChooseYesLose;

	// Token: 0x04000134 RID: 308
	[Header("SFX")]
	[SerializeField]
	private SFXLocalLoopComponent coinFlipSfx;

	// Token: 0x04000135 RID: 309
	[SerializeField]
	private SFXLocalPlayer gameWinSfx;

	// Token: 0x04000136 RID: 310
	[SerializeField]
	private SFXLocalPlayer gameLoseSfx;

	// Token: 0x04000137 RID: 311
	[SerializeField]
	private SFXLocalPlayer boomSfx;

	// Token: 0x04000138 RID: 312
	[SerializeField]
	private SFXLocalLoopComponent heartLoop;

	// Token: 0x04000139 RID: 313
	private bool _choiceLocked;

	// Token: 0x0400013A RID: 314
	private bool _isTransitioning;

	// Token: 0x0400013B RID: 315
	private Coroutine _checkStopCoroutine;
}
