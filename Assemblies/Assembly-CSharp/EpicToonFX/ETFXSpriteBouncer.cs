using System;
using UnityEngine;

namespace EpicToonFX
{
	// Token: 0x0200037A RID: 890
	public class ETFXSpriteBouncer : MonoBehaviour
	{
		// Token: 0x06001D64 RID: 7524 RVA: 0x0007EEAC File Offset: 0x0007D0AC
		private void Start()
		{
			this.startScale = base.transform.localScale;
			if (this.startScale.y != 1f)
			{
				float y = this.startScale.y / this.scaleAmount;
				this.startScale = new Vector3(this.startScale.x, y, this.startScale.z);
			}
		}

		// Token: 0x06001D65 RID: 7525 RVA: 0x0007EF14 File Offset: 0x0007D114
		private void Update()
		{
			this.scaleTimer += Time.deltaTime;
			float t = Mathf.Clamp01(this.scaleTimer / this.scaleDuration);
			float y = Mathf.Lerp(this.startScale.y, this.startScale.y * this.scaleAmount, t) + Mathf.PingPong(this.scaleTimer / this.scaleDuration, 0.1f);
			Vector3 localScale = new Vector3(this.startScale.x, y, this.startScale.z);
			base.transform.localScale = localScale;
		}

		// Token: 0x040013E2 RID: 5090
		public float scaleAmount = 1.1f;

		// Token: 0x040013E3 RID: 5091
		public float scaleDuration = 1f;

		// Token: 0x040013E4 RID: 5092
		private Vector3 startScale;

		// Token: 0x040013E5 RID: 5093
		private float scaleTimer;
	}
}
