using System;
using System.Collections;
using UnityEngine;

namespace EpicToonFX
{
	// Token: 0x02000374 RID: 884
	public class ETFXLoopScript : MonoBehaviour
	{
		// Token: 0x06001D25 RID: 7461 RVA: 0x0007E219 File Offset: 0x0007C419
		private void Start()
		{
			this.PlayEffect();
		}

		// Token: 0x06001D26 RID: 7462 RVA: 0x0007E221 File Offset: 0x0007C421
		public void PlayEffect()
		{
			base.StartCoroutine("EffectLoop");
		}

		// Token: 0x06001D27 RID: 7463 RVA: 0x0007E22F File Offset: 0x0007C42F
		private IEnumerator EffectLoop()
		{
			GameObject effectPlayer = Object.Instantiate<GameObject>(this.chosenEffect, base.transform.position, base.transform.rotation);
			effectPlayer.transform.localScale = new Vector3(this.spawnScale, this.spawnScale, this.spawnScale);
			if (this.disableLights && effectPlayer.GetComponent<Light>())
			{
				effectPlayer.GetComponent<Light>().enabled = false;
			}
			if (this.disableSound && effectPlayer.GetComponent<AudioSource>())
			{
				effectPlayer.GetComponent<AudioSource>().enabled = false;
			}
			yield return new WaitForSeconds(this.loopTimeLimit);
			Object.Destroy(effectPlayer);
			this.PlayEffect();
			yield break;
		}

		// Token: 0x040013B4 RID: 5044
		public GameObject chosenEffect;

		// Token: 0x040013B5 RID: 5045
		public float loopTimeLimit = 2f;

		// Token: 0x040013B6 RID: 5046
		[Header("Spawn options")]
		public bool disableLights = true;

		// Token: 0x040013B7 RID: 5047
		public bool disableSound = true;

		// Token: 0x040013B8 RID: 5048
		public float spawnScale = 1f;
	}
}
