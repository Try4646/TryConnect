using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace EpicToonFX
{
	// Token: 0x0200036E RID: 878
	public class ETFXEffectControllerPooled : MonoBehaviour
	{
		// Token: 0x06001CFA RID: 7418 RVA: 0x0007D614 File Offset: 0x0007B814
		private void Awake()
		{
			this.effectNameText = GameObject.Find("EffectName").GetComponent<Text>();
			this.effectIndexText = GameObject.Find("EffectIndex").GetComponent<Text>();
			this.etfxMouseOrbit = Camera.main.GetComponent<ETFXMouseOrbit>();
			this.etfxMouseOrbit.etfxEffectControllerPooled = this;
			this.effectsPool = new List<GameObject>();
			for (int i = 0; i < this.effects.Length; i++)
			{
				GameObject gameObject = Object.Instantiate<GameObject>(this.effects[i], base.transform.position, Quaternion.identity);
				gameObject.transform.parent = base.transform;
				this.effectsPool.Add(gameObject);
				gameObject.SetActive(false);
			}
		}

		// Token: 0x06001CFB RID: 7419 RVA: 0x0007D6C7 File Offset: 0x0007B8C7
		private void Start()
		{
			base.Invoke("InitializeLoop", this.startDelay);
		}

		// Token: 0x06001CFC RID: 7420 RVA: 0x0007D6DA File Offset: 0x0007B8DA
		private void Update()
		{
			if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D))
			{
				this.NextEffect();
			}
			if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow))
			{
				this.PreviousEffect();
			}
		}

		// Token: 0x06001CFD RID: 7421 RVA: 0x0007D712 File Offset: 0x0007B912
		private void FixedUpdate()
		{
			if (this.autoRotation)
			{
				this.etfxMouseOrbit.SetAutoRotationSpeed(this.autoRotationSpeed);
				if (!this.etfxMouseOrbit.isAutoRotating)
				{
					this.etfxMouseOrbit.InitializeAutoRotation();
				}
			}
		}

		// Token: 0x06001CFE RID: 7422 RVA: 0x0007D745 File Offset: 0x0007B945
		public void InitializeLoop()
		{
			base.StartCoroutine(this.EffectLoop());
		}

		// Token: 0x06001CFF RID: 7423 RVA: 0x0007D754 File Offset: 0x0007B954
		public void NextEffect()
		{
			if (this.effectIndex < this.effects.Length - 1)
			{
				this.effectIndex++;
			}
			else
			{
				this.effectIndex = 0;
			}
			this.CleanCurrentEffect();
		}

		// Token: 0x06001D00 RID: 7424 RVA: 0x0007D785 File Offset: 0x0007B985
		public void PreviousEffect()
		{
			if (this.effectIndex > 0)
			{
				this.effectIndex--;
			}
			else
			{
				this.effectIndex = this.effects.Length - 1;
			}
			this.CleanCurrentEffect();
		}

		// Token: 0x06001D01 RID: 7425 RVA: 0x0007D7B6 File Offset: 0x0007B9B6
		private void CleanCurrentEffect()
		{
			base.StopAllCoroutines();
			if (this.currentEffect != null)
			{
				this.currentEffect.SetActive(false);
			}
			base.StartCoroutine(this.EffectLoop());
		}

		// Token: 0x06001D02 RID: 7426 RVA: 0x0007D7E5 File Offset: 0x0007B9E5
		private IEnumerator EffectLoop()
		{
			this.currentEffect = this.effectsPool[this.effectIndex];
			this.currentEffect.SetActive(true);
			if (this.disableLights && this.currentEffect.GetComponent<Light>())
			{
				this.currentEffect.GetComponent<Light>().enabled = false;
			}
			if (this.disableSound && this.currentEffect.GetComponent<AudioSource>())
			{
				this.currentEffect.GetComponent<AudioSource>().enabled = false;
			}
			this.effectNameText.text = this.effects[this.effectIndex].name;
			this.effectIndexText.text = (this.effectIndex + 1).ToString() + " of " + this.effects.Length.ToString();
			ParticleSystem particleSystem = this.currentEffect.GetComponent<ParticleSystem>();
			for (;;)
			{
				yield return new WaitForSeconds(particleSystem.main.duration + this.respawnDelay);
				if (!this.slideshowMode)
				{
					if (!particleSystem.main.loop)
					{
						this.currentEffect.SetActive(false);
						this.currentEffect.SetActive(true);
					}
				}
				else
				{
					if (particleSystem.main.loop)
					{
						yield return new WaitForSeconds(this.respawnDelay);
					}
					this.NextEffect();
				}
			}
			yield break;
		}

		// Token: 0x04001386 RID: 4998
		public GameObject[] effects;

		// Token: 0x04001387 RID: 4999
		private List<GameObject> effectsPool;

		// Token: 0x04001388 RID: 5000
		private int effectIndex;

		// Token: 0x04001389 RID: 5001
		[Space(10f)]
		[Header("Spawn Settings")]
		public bool disableLights = true;

		// Token: 0x0400138A RID: 5002
		public bool disableSound = true;

		// Token: 0x0400138B RID: 5003
		public float startDelay = 0.2f;

		// Token: 0x0400138C RID: 5004
		public float respawnDelay = 0.5f;

		// Token: 0x0400138D RID: 5005
		public bool slideshowMode;

		// Token: 0x0400138E RID: 5006
		public bool autoRotation;

		// Token: 0x0400138F RID: 5007
		[Range(0.001f, 0.5f)]
		public float autoRotationSpeed = 0.1f;

		// Token: 0x04001390 RID: 5008
		private GameObject currentEffect;

		// Token: 0x04001391 RID: 5009
		private Text effectNameText;

		// Token: 0x04001392 RID: 5010
		private Text effectIndexText;

		// Token: 0x04001393 RID: 5011
		private ETFXMouseOrbit etfxMouseOrbit;
	}
}
