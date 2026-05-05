using System;
using UnityEngine;

namespace EpicToonFX
{
	// Token: 0x0200037F RID: 895
	public class ETFXLightFade : MonoBehaviour
	{
		// Token: 0x06001D7B RID: 7547 RVA: 0x0007F550 File Offset: 0x0007D750
		private void Start()
		{
			this.li = base.GetComponent<Light>();
			if (this.li != null)
			{
				this.initIntensity = this.li.intensity;
			}
		}

		// Token: 0x06001D7C RID: 7548 RVA: 0x0007F580 File Offset: 0x0007D780
		private void Update()
		{
			if (this.li != null)
			{
				this.li.intensity -= this.initIntensity * (Time.deltaTime / this.life);
				if (this.li.intensity <= 0f)
				{
					switch (this.onLifeEnd)
					{
					case ETFXLightFade.OnLifeEnd.DoNothing:
						break;
					case ETFXLightFade.OnLifeEnd.Disable:
						this.li.enabled = false;
						return;
					case ETFXLightFade.OnLifeEnd.Destroy:
						Object.Destroy(this.li);
						break;
					default:
						return;
					}
				}
			}
		}

		// Token: 0x04001400 RID: 5120
		[Header("Seconds to dim the light")]
		public float life = 0.2f;

		// Token: 0x04001401 RID: 5121
		public ETFXLightFade.OnLifeEnd onLifeEnd = ETFXLightFade.OnLifeEnd.Destroy;

		// Token: 0x04001402 RID: 5122
		private Light li;

		// Token: 0x04001403 RID: 5123
		private float initIntensity;

		// Token: 0x02000380 RID: 896
		public enum OnLifeEnd
		{
			// Token: 0x04001405 RID: 5125
			DoNothing,
			// Token: 0x04001406 RID: 5126
			Disable,
			// Token: 0x04001407 RID: 5127
			Destroy
		}
	}
}
