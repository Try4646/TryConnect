using System;
using UnityEngine;

namespace EpicToonFX
{
	// Token: 0x02000382 RID: 898
	public class ETFXRotation : MonoBehaviour
	{
		// Token: 0x06001D80 RID: 7552 RVA: 0x000048A7 File Offset: 0x00002AA7
		private void Start()
		{
		}

		// Token: 0x06001D81 RID: 7553 RVA: 0x0007F670 File Offset: 0x0007D870
		private void Update()
		{
			if (this.rotateSpace == ETFXRotation.spaceEnum.Local)
			{
				base.transform.Rotate(this.rotateVector * Time.deltaTime);
			}
			if (this.rotateSpace == ETFXRotation.spaceEnum.World)
			{
				base.transform.Rotate(this.rotateVector * Time.deltaTime, Space.World);
			}
		}

		// Token: 0x04001409 RID: 5129
		[Header("Rotate axises by degrees per second")]
		public Vector3 rotateVector = Vector3.zero;

		// Token: 0x0400140A RID: 5130
		public ETFXRotation.spaceEnum rotateSpace;

		// Token: 0x02000383 RID: 899
		public enum spaceEnum
		{
			// Token: 0x0400140C RID: 5132
			Local,
			// Token: 0x0400140D RID: 5133
			World
		}
	}
}
