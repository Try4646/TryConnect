using System;
using Extensions;
using UnityEngine;

// Token: 0x02000242 RID: 578
public class InviteFriendsButton : MonoBehaviour
{
	// Token: 0x060014D9 RID: 5337 RVA: 0x000598BE File Offset: 0x00057ABE
	public void OnInviteFriendsClick()
	{
		if (MonoSingleton<LobbyManager>.Instance == null)
		{
			Debug.LogError("[InviteFriendsButton] LobbyManager.Instance is null!");
			return;
		}
		MonoSingleton<LobbyManager>.Instance.InviteFriend();
	}
}
