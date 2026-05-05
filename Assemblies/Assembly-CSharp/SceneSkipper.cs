using System;
using System.Collections;
using Extensions;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

// Token: 0x0200020F RID: 527
public class SceneSkipper : MonoBehaviour
{
	// Token: 0x0600136E RID: 4974 RVA: 0x00053BF1 File Offset: 0x00051DF1
	private void OnEnable()
	{
		SceneManager.sceneLoaded += this.OnSceneLoaded;
	}

	// Token: 0x0600136F RID: 4975 RVA: 0x00053C04 File Offset: 0x00051E04
	private void OnDisable()
	{
		SceneManager.sceneLoaded -= this.OnSceneLoaded;
	}

	// Token: 0x06001370 RID: 4976 RVA: 0x00053C17 File Offset: 0x00051E17
	private void Start()
	{
		base.StartCoroutine(this.SkipSceneRoutine());
	}

	// Token: 0x06001371 RID: 4977 RVA: 0x00053C28 File Offset: 0x00051E28
	private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
	{
		this._isSkipping = false;
		if (MonoSingleton<SceneTransitioner>.Instance != null)
		{
			MonoSingleton<SceneTransitioner>.Instance.SetLoadingScreen(false, this.transitionDuration, false);
		}
		if (!string.IsNullOrEmpty(this.mainMenuSceneName) && scene.name == this.mainMenuSceneName)
		{
			Object.Destroy(base.gameObject);
		}
		base.StartCoroutine(this.SkipSceneRoutine());
	}

	// Token: 0x06001372 RID: 4978 RVA: 0x00053C94 File Offset: 0x00051E94
	private IEnumerator SkipSceneRoutine()
	{
		yield return new WaitForSeconds(3f);
		if (this.ignoreInput)
		{
			yield break;
		}
		this.SkipScene();
		yield break;
	}

	// Token: 0x06001373 RID: 4979 RVA: 0x00053CA3 File Offset: 0x00051EA3
	private void Update()
	{
		if (!this.ignoreInput && SceneSkipper.IsAnyInputPressedThisFrame())
		{
			this.SkipScene();
		}
	}

	// Token: 0x06001374 RID: 4980 RVA: 0x00053CBC File Offset: 0x00051EBC
	private static bool IsAnyInputPressedThisFrame()
	{
		return (Keyboard.current != null && Keyboard.current.anyKey.wasPressedThisFrame) || (Mouse.current != null && (Mouse.current.leftButton.wasPressedThisFrame || Mouse.current.rightButton.wasPressedThisFrame || Mouse.current.middleButton.wasPressedThisFrame)) || (Gamepad.current != null && (Gamepad.current.buttonSouth.wasPressedThisFrame || Gamepad.current.buttonNorth.wasPressedThisFrame || Gamepad.current.buttonEast.wasPressedThisFrame || Gamepad.current.buttonWest.wasPressedThisFrame || Gamepad.current.startButton.wasPressedThisFrame));
	}

	// Token: 0x06001375 RID: 4981 RVA: 0x00053D81 File Offset: 0x00051F81
	public void SkipSceneFromInspector()
	{
		this.SkipScene();
	}

	// Token: 0x06001376 RID: 4982 RVA: 0x00053D8C File Offset: 0x00051F8C
	private void SkipScene()
	{
		if (this._isSkipping)
		{
			return;
		}
		if (string.IsNullOrEmpty(this.mainMenuSceneName) || !(SceneManager.GetActiveScene().name != this.mainMenuSceneName))
		{
			Object.Destroy(base.gameObject);
			return;
		}
		int num = SceneManager.GetActiveScene().buildIndex + 1;
		if (num < SceneManager.sceneCountInBuildSettings)
		{
			this._isSkipping = true;
			base.StartCoroutine(this.TransitionAndLoadRoutine(num));
			return;
		}
		Debug.LogWarning("No more scenes to load. Check your Build Settings.");
	}

	// Token: 0x06001377 RID: 4983 RVA: 0x00053E0D File Offset: 0x0005200D
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

	// Token: 0x04000C62 RID: 3170
	[SerializeField]
	private string mainMenuSceneName = "MainMenuScene";

	// Token: 0x04000C63 RID: 3171
	[SerializeField]
	private float transitionDuration = 0.5f;

	// Token: 0x04000C64 RID: 3172
	public bool ignoreInput;

	// Token: 0x04000C65 RID: 3173
	private bool _isSkipping;
}
