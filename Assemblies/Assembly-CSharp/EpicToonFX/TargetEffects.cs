using System;
using System.Collections.Generic;
using UnityEngine;

namespace EpicToonFX
{
	// Token: 0x0200037B RID: 891
	[Serializable]
	public class TargetEffects
	{
		// Token: 0x040013E6 RID: 5094
		public GameObject hitParticle;

		// Token: 0x040013E7 RID: 5095
		public GameObject respawnParticle;

		// Token: 0x040013E8 RID: 5096
		public List<GameObject> deathParticles = new List<GameObject>();

		// Token: 0x040013E9 RID: 5097
		public AudioClip destroySound;

		// Token: 0x040013EA RID: 5098
		public AudioClip respawnSound;
	}
}
