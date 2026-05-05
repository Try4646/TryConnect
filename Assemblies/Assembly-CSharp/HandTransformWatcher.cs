using System;
using Mirror;
using UnityEngine;

// Token: 0x020001ED RID: 493
public class HandTransformWatcher : NetworkBehaviour
{
	// Token: 0x060011B1 RID: 4529 RVA: 0x0004C799 File Offset: 0x0004A999
	private void OnTransformChildrenChanged()
	{
		Action onChildrenChanged = this.OnChildrenChanged;
		if (onChildrenChanged == null)
		{
			return;
		}
		onChildrenChanged();
	}

	// Token: 0x060011B3 RID: 4531 RVA: 0x00002321 File Offset: 0x00000521
	public override bool Weaved()
	{
		return true;
	}

	// Token: 0x04000B66 RID: 2918
	[Header("Event Callbacks")]
	public Action OnChildrenChanged;
}
