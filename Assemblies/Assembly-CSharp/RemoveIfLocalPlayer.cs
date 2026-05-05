using System;
using Mirror;
using UnityEngine;

// Token: 0x020002D8 RID: 728
public class RemoveIfLocalPlayer : NetworkBehaviour
{
	// Token: 0x0600198D RID: 6541 RVA: 0x0006B3CF File Offset: 0x000695CF
	public override void OnStartClient()
	{
		if (base.isLocalPlayer)
		{
			Object.Destroy(base.gameObject);
		}
	}

	// Token: 0x0600198F RID: 6543 RVA: 0x00002321 File Offset: 0x00000521
	public override bool Weaved()
	{
		return true;
	}
}
