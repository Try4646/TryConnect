using System;
using Extensions;
using Mirror;
using UnityEngine;

// Token: 0x020002E3 RID: 739
public class SceneSwitcher : NetworkBehaviour
{
	// Token: 0x060019C2 RID: 6594 RVA: 0x0006BDF4 File Offset: 0x00069FF4
	[Server]
	public void ServerProgressSceneWithoutInteraction()
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void SceneSwitcher::ServerProgressSceneWithoutInteraction()' called when server was not active");
			return;
		}
		if (!this.isInteractable)
		{
			return;
		}
		this.isInteractable = false;
		if (NetworkSingleton<GameManager>.Instance.state == GameState.Game)
		{
			NetworkSingleton<GameManager>.Instance.ShowDayStats();
			return;
		}
		if (NetworkSingleton<GameManager>.Instance.state == GameState.Lose)
		{
			NetworkSingleton<GameManager>.Instance.ShowGameOverStats();
			return;
		}
		NetworkSingleton<GameManager>.Instance.ProgressGame();
	}

	// Token: 0x060019C4 RID: 6596 RVA: 0x00002321 File Offset: 0x00000521
	public override bool Weaved()
	{
		return true;
	}

	// Token: 0x0400108B RID: 4235
	[SerializeField]
	private GameState target;

	// Token: 0x0400108C RID: 4236
	[SerializeField]
	private bool isInteractable = true;
}
