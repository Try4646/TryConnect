using System;
using UnityEngine;

// Token: 0x02000088 RID: 136
public class WheelOfFortuneLocal : MonoBehaviour
{
	// Token: 0x060004DD RID: 1245 RVA: 0x00015C66 File Offset: 0x00013E66
	public void Play()
	{
		if (this.wheel != null)
		{
			this.wheel.SpinTheWheel();
		}
	}

	// Token: 0x0400034E RID: 846
	[Header("References")]
	[SerializeField]
	private WheelLocal wheel;

	// Token: 0x0400034F RID: 847
	[SerializeField]
	private CasinoGameFeedbacks gameFeedbacks;
}
