using System;
using UnityEngine;

// Token: 0x02000293 RID: 659
public class CasinoFloor : MonoBehaviour
{
	// Token: 0x0600176C RID: 5996 RVA: 0x00063044 File Offset: 0x00061244
	public void SetSfxTrigger(bool isEnabled)
	{
		BoxCollider[] array = this.sfxTriggerBox;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].enabled = isEnabled;
		}
	}

	// Token: 0x04000F2E RID: 3886
	public int floorIndex;

	// Token: 0x04000F2F RID: 3887
	[SerializeField]
	private BoxCollider[] sfxTriggerBox;
}
