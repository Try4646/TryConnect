using System;
using DG.Tweening;
using UnityEngine;

// Token: 0x0200009C RID: 156
public class WheelResult : MonoBehaviour
{
	// Token: 0x060005B3 RID: 1459 RVA: 0x0001912F File Offset: 0x0001732F
	public void SelectedResultFeedback()
	{
		this.meshTransform.DOPunchScale(this.meshTransform.localScale * 1.2f, 0.5f, 1, 1f);
	}

	// Token: 0x040003FA RID: 1018
	public string result;

	// Token: 0x040003FB RID: 1019
	[SerializeField]
	private Transform meshTransform;
}
