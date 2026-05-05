using System;
using UnityEngine;

namespace EpicToonFX
{
	// Token: 0x02000381 RID: 897
	public class ETFXPitchRandomizer : MonoBehaviour
	{
		// Token: 0x06001D7E RID: 7550 RVA: 0x0007F61E File Offset: 0x0007D81E
		private void Start()
		{
			base.transform.GetComponent<AudioSource>().pitch *= 1f + Random.Range(-this.randomPercent / 100f, this.randomPercent / 100f);
		}

		// Token: 0x04001408 RID: 5128
		public float randomPercent = 10f;
	}
}
