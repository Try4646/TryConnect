using System;
using UnityEngine;

// Token: 0x020000D4 RID: 212
[Serializable]
public class ItemAction
{
	// Token: 0x04000553 RID: 1363
	[Tooltip("Display name for the action (e.g., 'Use', 'Throw', 'Consume')")]
	public string actionName;

	// Token: 0x04000554 RID: 1364
	[Tooltip("Key to display for this action (e.g., 'E', 'F', 'G', 'Left Click')")]
	public string key;

	// Token: 0x04000555 RID: 1365
	[Tooltip("If true, shows 'Hold' text. If false, no hold text is shown.")]
	public bool isHold;
}
