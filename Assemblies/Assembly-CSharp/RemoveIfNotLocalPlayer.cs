using System;
using Mirror;
using UnityEngine;

// Token: 0x020002D9 RID: 729
public class RemoveIfNotLocalPlayer : NetworkBehaviour
{
	// Token: 0x06001990 RID: 6544 RVA: 0x0006B3E4 File Offset: 0x000695E4
	public override void OnStartClient()
	{
		if (!base.isLocalPlayer)
		{
			Object.Destroy(base.gameObject);
		}
	}

	// Token: 0x06001992 RID: 6546 RVA: 0x00002321 File Offset: 0x00000521
	public override bool Weaved()
	{
		return true;
	}
}
