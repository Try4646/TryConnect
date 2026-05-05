using System;
using Mirror;
using UnityEngine;

// Token: 0x020001FC RID: 508
public class PlayerExpressionComponent : NetworkBehaviour
{
	// Token: 0x06001292 RID: 4754 RVA: 0x000503C5 File Offset: 0x0004E5C5
	public override void OnStartClient()
	{
		if (!base.isLocalPlayer)
		{
			base.enabled = false;
			return;
		}
		this.networkAnimator = base.GetComponent<NetworkAnimator>();
	}

	// Token: 0x06001293 RID: 4755 RVA: 0x000503E3 File Offset: 0x0004E5E3
	public void SetHandAnimationTrigger(string triggerName)
	{
		this.networkAnimator.SetTrigger(triggerName);
	}

	// Token: 0x06001295 RID: 4757 RVA: 0x00002321 File Offset: 0x00000521
	public override bool Weaved()
	{
		return true;
	}

	// Token: 0x04000BD3 RID: 3027
	[SerializeField]
	private NetworkAnimator networkAnimator;
}
