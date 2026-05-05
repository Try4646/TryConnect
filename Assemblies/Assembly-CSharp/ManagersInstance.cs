using System;
using Extensions;
using UnityEngine;
using UnityEngine.SceneManagement;

// Token: 0x0200017E RID: 382
public class ManagersInstance : NetworkSingleton<ManagersInstance>
{
	// Token: 0x06000E56 RID: 3670 RVA: 0x0003B670 File Offset: 0x00039870
	protected override void OnAwake()
	{
		Object.DontDestroyOnLoad(base.gameObject);
	}

	// Token: 0x06000E57 RID: 3671 RVA: 0x0003B67D File Offset: 0x0003987D
	private void OnEnable()
	{
		SceneManager.sceneLoaded += this.OnSceneLoaded;
	}

	// Token: 0x06000E58 RID: 3672 RVA: 0x0003B690 File Offset: 0x00039890
	private void OnDisable()
	{
		SceneManager.sceneLoaded -= this.OnSceneLoaded;
	}

	// Token: 0x06000E59 RID: 3673 RVA: 0x0003B6A3 File Offset: 0x000398A3
	private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
	{
		if (scene.name == "NetworkSetupScene")
		{
			Object.Destroy(base.gameObject);
		}
	}

	// Token: 0x06000E5B RID: 3675 RVA: 0x00002321 File Offset: 0x00000521
	public override bool Weaved()
	{
		return true;
	}
}
