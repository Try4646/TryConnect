using System;
using System.Collections.Generic;
using Extensions;
using Mirror;
using UnityEngine;
using UnityEngine.SceneManagement;

// Token: 0x0200017B RID: 379
public class LocalManager : MonoSingleton<LocalManager>
{
	// Token: 0x06000E4A RID: 3658 RVA: 0x0003B384 File Offset: 0x00039584
	protected override void OnAwake()
	{
		base.OnAwake();
		Object.DontDestroyOnLoad(base.gameObject);
		if (!this.playerEyesUI)
		{
			this.playerEyesUI = base.GetComponentInChildren<PlayerEyesUI>();
		}
		if (!this.playerBuffUI)
		{
			this.playerBuffUI = base.GetComponentInChildren<PlayerBuffUI>();
		}
		if (!this.interactionUIPanel)
		{
			this.interactionUIPanel = base.GetComponentInChildren<InteractionUIPanel>();
		}
		if (!this.heldItemActionPanel)
		{
			this.heldItemActionPanel = base.GetComponentInChildren<HeldItemActionPanel>();
		}
	}

	// Token: 0x06000E4B RID: 3659 RVA: 0x0003B406 File Offset: 0x00039606
	private void OnEnable()
	{
		SceneManager.sceneLoaded += this.OnSceneLoaded;
	}

	// Token: 0x06000E4C RID: 3660 RVA: 0x0003B419 File Offset: 0x00039619
	private void OnDisable()
	{
		SceneManager.sceneLoaded -= this.OnSceneLoaded;
	}

	// Token: 0x06000E4D RID: 3661 RVA: 0x0003B42C File Offset: 0x0003962C
	private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
	{
		if (!this || !base.gameObject)
		{
			return;
		}
		string name = scene.name;
		if (name == "NetworkSetupScene" || name == "MainMenuScene")
		{
			GameSettings gameSettings = Resources.Load<GameSettings>("GameSettings");
			if (gameSettings != null)
			{
				gameSettings.gameHasStarted = false;
			}
			base.enabled = false;
			Object.Destroy(base.gameObject, 0.01f);
			return;
		}
		if (scene.name == "HomeScene")
		{
			UICursorSimple instance = UICursorSimple.Instance;
			if (instance != null)
			{
				instance.HideCursor();
			}
		}
		if (this.mainCamera)
		{
			Object.Destroy(this.mainCamera.gameObject);
		}
		this.mainCamera = Object.Instantiate<Camera>(Resources.Load<Camera>("Camera"), base.transform);
		this.players.Clear();
	}

	// Token: 0x06000E4E RID: 3662 RVA: 0x0003B50A File Offset: 0x0003970A
	private void Start()
	{
		if (this.mainCamera)
		{
			Object.Destroy(this.mainCamera.gameObject);
		}
		this.mainCamera = Object.Instantiate<Camera>(Resources.Load<Camera>("Camera"), base.transform);
	}

	// Token: 0x06000E4F RID: 3663 RVA: 0x0003B544 File Offset: 0x00039744
	public void SetCrosshair(bool isEnabled)
	{
		this.crosshair.SetActive(isEnabled);
	}

	// Token: 0x06000E50 RID: 3664 RVA: 0x0003B554 File Offset: 0x00039754
	public void RegisterPlayer(NetworkIdentity identity)
	{
		PlayerReferences playerReferences = new PlayerReferences(identity);
		this.players.Add(playerReferences);
		Action<PlayerReferences> onNewPlayerRegistered = this.OnNewPlayerRegistered;
		if (onNewPlayerRegistered == null)
		{
			return;
		}
		onNewPlayerRegistered(playerReferences);
	}

	// Token: 0x06000E51 RID: 3665 RVA: 0x0003B588 File Offset: 0x00039788
	public void UnregisterPlayer(NetworkIdentity identity)
	{
		this.players.Remove(this.players.Find((PlayerReferences player) => player.identity == identity));
	}

	// Token: 0x0400091B RID: 2331
	public Camera mainCamera;

	// Token: 0x0400091C RID: 2332
	public PlayerEyesUI playerEyesUI;

	// Token: 0x0400091D RID: 2333
	public PlayerBuffUI playerBuffUI;

	// Token: 0x0400091E RID: 2334
	public InteractionUIPanel interactionUIPanel;

	// Token: 0x0400091F RID: 2335
	public HeldItemActionPanel heldItemActionPanel;

	// Token: 0x04000920 RID: 2336
	public BaseCursor baseCursor;

	// Token: 0x04000921 RID: 2337
	public GameObject crosshair;

	// Token: 0x04000922 RID: 2338
	public GameObject itemInputsUI;

	// Token: 0x04000923 RID: 2339
	public List<PlayerReferences> players;

	// Token: 0x04000924 RID: 2340
	public Action<PlayerReferences> OnNewPlayerRegistered;
}
