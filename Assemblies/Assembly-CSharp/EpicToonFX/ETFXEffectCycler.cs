using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace EpicToonFX
{
	// Token: 0x02000370 RID: 880
	public class ETFXEffectCycler : MonoBehaviour
	{
		// Token: 0x06001D0A RID: 7434 RVA: 0x0007DA02 File Offset: 0x0007BC02
		private void Start()
		{
			base.Invoke("PlayEffect", this.startDelay);
		}

		// Token: 0x06001D0B RID: 7435 RVA: 0x0007DA15 File Offset: 0x0007BC15
		public void PlayEffect()
		{
			base.StartCoroutine("EffectLoop");
			if (this.effectIndex < this.listOfEffects.Count - 1)
			{
				this.effectIndex++;
				return;
			}
			this.effectIndex = 0;
		}

		// Token: 0x06001D0C RID: 7436 RVA: 0x0007DA4E File Offset: 0x0007BC4E
		private IEnumerator EffectLoop()
		{
			GameObject instantiatedEffect = Object.Instantiate<GameObject>(this.listOfEffects[this.effectIndex], base.transform.position, base.transform.rotation * Quaternion.Euler(0f, 0f, 0f));
			if (this.disableLights && instantiatedEffect.GetComponent<Light>())
			{
				instantiatedEffect.GetComponent<Light>().enabled = false;
			}
			if (this.disableSound && instantiatedEffect.GetComponent<AudioSource>())
			{
				instantiatedEffect.GetComponent<AudioSource>().enabled = false;
			}
			yield return new WaitForSeconds(this.loopLength);
			Object.Destroy(instantiatedEffect);
			this.PlayEffect();
			yield break;
		}

		// Token: 0x04001398 RID: 5016
		public List<GameObject> listOfEffects;

		// Token: 0x04001399 RID: 5017
		private int effectIndex;

		// Token: 0x0400139A RID: 5018
		[Header("Spawn Settings")]
		[SerializeField]
		[Space(10f)]
		public float loopLength = 1f;

		// Token: 0x0400139B RID: 5019
		public float startDelay = 1f;

		// Token: 0x0400139C RID: 5020
		public bool disableLights = true;

		// Token: 0x0400139D RID: 5021
		public bool disableSound = true;
	}
}
