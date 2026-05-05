using System;
using MoreMountains.Feedbacks;
using UnityEngine;

// Token: 0x0200006B RID: 107
public class PlinkoPillar : MonoBehaviour
{
	// Token: 0x060003BC RID: 956 RVA: 0x000117E4 File Offset: 0x0000F9E4
	private void OnCollisionEnter(Collision other)
	{
		if (other.gameObject.layer != LayerMask.NameToLayer("PlinkoPuck"))
		{
			return;
		}
		this.plinko.ServerPlayPillarFeedbacks(this);
	}

	// Token: 0x060003BD RID: 957 RVA: 0x0001180A File Offset: 0x0000FA0A
	public void PlayFeedbacks()
	{
		this.onHitFb.PlayFeedbacks();
	}

	// Token: 0x040002AB RID: 683
	[SerializeField]
	private Plinko plinko;

	// Token: 0x040002AC RID: 684
	[SerializeField]
	private MMF_Player onHitFb;
}
