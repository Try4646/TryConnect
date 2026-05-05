using System;
using System.Collections;
using Extensions;
using UnityEngine;
using UnityEngine.SceneManagement;

// Token: 0x02000212 RID: 530
public class TransitionAnimator : MonoSingleton<TransitionAnimator>
{
	// Token: 0x06001385 RID: 4997 RVA: 0x00053F43 File Offset: 0x00052143
	private void Start()
	{
		this.fadeImage.SetActive(true);
	}

	// Token: 0x06001386 RID: 4998 RVA: 0x00053F51 File Offset: 0x00052151
	public void LoadGame(int levelIndex)
	{
		if (this.isTransitioning)
		{
			return;
		}
		this.isTransitioning = true;
		base.StartCoroutine(this.LoadLevel(levelIndex));
	}

	// Token: 0x06001387 RID: 4999 RVA: 0x00053F71 File Offset: 0x00052171
	private IEnumerator LoadLevel(int levelIndex)
	{
		this.transition.SetTrigger("START");
		yield return new WaitForSeconds(1f);
		AsyncOperation operation = SceneManager.LoadSceneAsync(levelIndex);
		while (!operation.isDone)
		{
			Mathf.Clamp01(operation.progress / 0.9f);
			yield return null;
		}
		this.isTransitioning = false;
		this.FadeIn();
		yield break;
	}

	// Token: 0x06001388 RID: 5000 RVA: 0x00053F87 File Offset: 0x00052187
	public void FadeOut()
	{
		this.transition.SetTrigger("START");
		this.isBlackScreen = true;
	}

	// Token: 0x06001389 RID: 5001 RVA: 0x00053FA0 File Offset: 0x000521A0
	public void FadeIn()
	{
		this.transition.SetTrigger("END");
		this.isBlackScreen = false;
	}

	// Token: 0x04000C6D RID: 3181
	[SerializeField]
	private GameObject fadeImage;

	// Token: 0x04000C6E RID: 3182
	public Animator transition;

	// Token: 0x04000C6F RID: 3183
	public bool isTransitioning;

	// Token: 0x04000C70 RID: 3184
	public bool isBlackScreen;
}
