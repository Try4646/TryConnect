using System;
using UnityEngine;

// Token: 0x0200014C RID: 332
[Serializable]
public class CursorData
{
	// Token: 0x04000832 RID: 2098
	public CursorType type;

	// Token: 0x04000833 RID: 2099
	public Texture2D texture;

	// Token: 0x04000834 RID: 2100
	[Tooltip("Leave at 0,0 for top-left alignment. Set to texture dimensions/2 for center alignment")]
	public Vector2 hotspot = Vector2.zero;

	// Token: 0x04000835 RID: 2101
	[Tooltip("If true, hotspot will be set to center of texture")]
	public bool useCenterAlignment;
}
