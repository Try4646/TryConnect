using System;
using System.Collections.Generic;
using UnityEngine;

// Token: 0x02000307 RID: 775
[Serializable]
public class Pool
{
	// Token: 0x04001125 RID: 4389
	public SlotType slot;

	// Token: 0x04001126 RID: 4390
	[TextArea(2, 8)]
	public List<string> items = new List<string>();
}
