using System;
using System.Collections;
using UnityEngine;

namespace EpicToonFX
{
	// Token: 0x0200037C RID: 892
	public class ETFXTarget : MonoBehaviour
	{
		// Token: 0x06001D68 RID: 7528 RVA: 0x0007EFDD File Offset: 0x0007D1DD
		private void Start()
		{
			this.targetRenderer = base.GetComponent<Renderer>();
			this.targetCollider = base.GetComponent<Collider>();
			this.audioSource = base.GetComponent<AudioSource>();
			this.originalScale = base.transform.localScale;
		}

		// Token: 0x06001D69 RID: 7529 RVA: 0x0007F014 File Offset: 0x0007D214
		private void SpawnTarget()
		{
			this.targetRenderer.enabled = true;
			this.targetCollider.enabled = true;
			if (this.effects.respawnParticle)
			{
				Object.Destroy(Object.Instantiate<GameObject>(this.effects.respawnParticle, base.transform.position, base.transform.rotation), 3.5f);
			}
			if (this.effects.respawnSound && this.audioSource)
			{
				this.audioSource.PlayOneShot(this.effects.respawnSound);
			}
			this.currentHits = 0;
			base.transform.localScale = this.originalScale;
		}

		// Token: 0x06001D6A RID: 7530 RVA: 0x0007F0C8 File Offset: 0x0007D2C8
		private IEnumerator Respawn()
		{
			yield return new WaitForSeconds(this.respawnTime);
			this.SpawnTarget();
			yield break;
		}

		// Token: 0x06001D6B RID: 7531 RVA: 0x0007F0D8 File Offset: 0x0007D2D8
		public void OnHit()
		{
			this.currentHits++;
			if (this.currentHits >= this.hitsToDestroy)
			{
				this.DestroyTarget();
				return;
			}
			if (this.effects.hitParticle)
			{
				Object.Destroy(Object.Instantiate<GameObject>(this.effects.hitParticle, base.transform.position, base.transform.rotation), 2f);
			}
			if (this.enableSquashAndStretch)
			{
				base.StartCoroutine(this.SquashAndStretch());
			}
		}

		// Token: 0x06001D6C RID: 7532 RVA: 0x0007F15F File Offset: 0x0007D35F
		private IEnumerator SquashAndStretch()
		{
			float timeElapsed = 0f;
			Vector3 startScale = this.originalScale;
			Vector3 endScale = Vector3.Scale(this.originalScale, this.squashScale);
			while (timeElapsed < this.duration)
			{
				base.transform.localScale = Vector3.Lerp(startScale, endScale, timeElapsed / this.duration);
				timeElapsed += Time.deltaTime;
				yield return null;
			}
			timeElapsed = 0f;
			startScale = endScale;
			endScale = Vector3.Scale(this.originalScale, this.stretchScale);
			while (timeElapsed < this.duration)
			{
				base.transform.localScale = Vector3.Lerp(startScale, endScale, timeElapsed / this.duration);
				timeElapsed += Time.deltaTime;
				yield return null;
			}
			timeElapsed = 0f;
			startScale = endScale;
			endScale = this.originalScale;
			while (timeElapsed < this.duration)
			{
				base.transform.localScale = Vector3.Lerp(startScale, endScale, timeElapsed / this.duration);
				timeElapsed += Time.deltaTime;
				yield return null;
			}
			yield break;
		}

		// Token: 0x06001D6D RID: 7533 RVA: 0x0007F170 File Offset: 0x0007D370
		private void DestroyTarget()
		{
			if (this.effects.deathParticles.Count > 0)
			{
				GameObject obj;
				if (this.effects.deathParticles.Count == 1)
				{
					obj = Object.Instantiate<GameObject>(this.effects.deathParticles[0], base.transform.position, base.transform.rotation);
				}
				else
				{
					int index = Random.Range(0, this.effects.deathParticles.Count);
					obj = Object.Instantiate<GameObject>(this.effects.deathParticles[index], base.transform.position, base.transform.rotation);
				}
				Object.Destroy(obj, 2f);
			}
			this.targetRenderer.enabled = false;
			this.targetCollider.enabled = false;
			if (this.effects.destroySound && this.audioSource)
			{
				this.audioSource.PlayOneShot(this.effects.destroySound);
			}
			base.StartCoroutine(this.Respawn());
		}

		// Token: 0x040013EB RID: 5099
		public TargetEffects effects;

		// Token: 0x040013EC RID: 5100
		[Header("General Settings")]
		public int hitsToDestroy = 5;

		// Token: 0x040013ED RID: 5101
		public float respawnTime = 3f;

		// Token: 0x040013EE RID: 5102
		[Header("Squash & Stretch")]
		public bool enableSquashAndStretch = true;

		// Token: 0x040013EF RID: 5103
		public float duration = 0.07f;

		// Token: 0x040013F0 RID: 5104
		public Vector3 squashScale = new Vector3(0.8f, 1.2f, 1f);

		// Token: 0x040013F1 RID: 5105
		public Vector3 stretchScale = new Vector3(1.2f, 0.8f, 1f);

		// Token: 0x040013F2 RID: 5106
		private Renderer targetRenderer;

		// Token: 0x040013F3 RID: 5107
		private Collider targetCollider;

		// Token: 0x040013F4 RID: 5108
		private AudioSource audioSource;

		// Token: 0x040013F5 RID: 5109
		private int currentHits;

		// Token: 0x040013F6 RID: 5110
		private Vector3 originalScale;
	}
}
