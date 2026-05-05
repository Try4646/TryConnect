using System;
using System.Collections;
using Extensions;
using UnityEngine;

// Token: 0x0200024C RID: 588
public class QuitGameComponent : MonoBehaviour
{
	// Token: 0x06001520 RID: 5408 RVA: 0x000089A8 File Offset: 0x00006BA8
	public void QuitGame()
	{
		Application.Quit();
	}

	// Token: 0x06001521 RID: 5409 RVA: 0x0005AC8C File Offset: 0x00058E8C
	public void QuitGameWithConfirmation()
	{
		if (MonoSingleton<ConfirmationDialogManager>.Instance != null)
		{
			MonoSingleton<ConfirmationDialogManager>.Instance.ShowConfirmation("Are you sure you want to quit?", delegate
			{
				base.StartCoroutine(this.QuitGameCoroutine());
			}, delegate
			{
				Debug.Log("[QuitGameComponent] User cancelled quitting game.");
			}, "Yes, quit", "No, stay");
			return;
		}
		Debug.LogWarning("[QuitGameComponent] ConfirmationDialogManager not found. Quitting without confirmation.");
		this.QuitGame();
	}

	// Token: 0x06001522 RID: 5410 RVA: 0x0005ACFB File Offset: 0x00058EFB
	private IEnumerator QuitGameCoroutine()
	{
		MonoSingleton<SceneTransitioner>.Instance.SetLoadingScreen(true, 0.5f, false);
		yield return new WaitForSeconds(1f);
		this.QuitGame();
		yield break;
	}
}
