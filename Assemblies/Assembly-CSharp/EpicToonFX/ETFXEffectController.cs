using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace EpicToonFX
{
	// Token: 0x0200036C RID: 876
	public class ETFXEffectController : MonoBehaviour
	{
		// Token: 0x06001CEA RID: 7402 RVA: 0x0007D278 File Offset: 0x0007B478
		private void Awake()
		{
			this.effectNameText = GameObject.Find("EffectName").GetComponent<Text>();
			this.effectIndexText = GameObject.Find("EffectIndex").GetComponent<Text>();
			this.etfxMouseOrbit = Camera.main.GetComponent<ETFXMouseOrbit>();
			this.etfxMouseOrbit.etfxEffectController = this;
		}

		// Token: 0x06001CEB RID: 7403 RVA: 0x0007D2CB File Offset: 0x0007B4CB
		private void Start()
		{
			this.etfxMouseOrbit = Camera.main.GetComponent<ETFXMouseOrbit>();
			this.etfxMouseOrbit.etfxEffectController = this;
			base.Invoke("InitializeLoop", this.startDelay);
		}

		// Token: 0x06001CEC RID: 7404 RVA: 0x0007D2FA File Offset: 0x0007B4FA
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

		// Token: 0x06001CED RID: 7405 RVA: 0x0007D332 File Offset: 0x0007B532
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

		// Token: 0x06001CEE RID: 7406 RVA: 0x0007D365 File Offset: 0x0007B565
		public void InitializeLoop()
		{
			base.StartCoroutine(this.EffectLoop());
		}

		// Token: 0x06001CEF RID: 7407 RVA: 0x0007D374 File Offset: 0x0007B574
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

		// Token: 0x06001CF0 RID: 7408 RVA: 0x0007D3A5 File Offset: 0x0007B5A5
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

		// Token: 0x06001CF1 RID: 7409 RVA: 0x0007D3D6 File Offset: 0x0007B5D6
		private void CleanCurrentEffect()
		{
			base.StopAllCoroutines();
			if (this.currentEffect != null)
			{
				Object.Destroy(this.currentEffect);
			}
			base.StartCoroutine(this.EffectLoop());
		}

		// Token: 0x06001CF2 RID: 7410 RVA: 0x0007D404 File Offset: 0x0007B604
		private IEnumerator EffectLoop()
		{
			GameObject gameObject = Object.Instantiate<GameObject>(this.effects[this.effectIndex], base.transform.position, Quaternion.identity);
			this.currentEffect = gameObject;
			if (this.disableLights && gameObject.GetComponent<Light>())
			{
				gameObject.GetComponent<Light>().enabled = false;
			}
			if (this.disableSound && gameObject.GetComponent<AudioSource>())
			{
				gameObject.GetComponent<AudioSource>().enabled = false;
			}
			this.effectNameText.text = this.effects[this.effectIndex].name;
			this.effectIndexText.text = (this.effectIndex + 1).ToString() + " of " + this.effects.Length.ToString();
			ParticleSystem particleSystem = gameObject.GetComponent<ParticleSystem>();
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

		// Token: 0x04001375 RID: 4981
		public GameObject[] effects;

		// Token: 0x04001376 RID: 4982
		private int effectIndex;

		// Token: 0x04001377 RID: 4983
		[Space(10f)]
		[Header("Spawn Settings")]
		public bool disableLights = true;

		// Token: 0x04001378 RID: 4984
		public bool disableSound = true;

		// Token: 0x04001379 RID: 4985
		public float startDelay = 0.2f;

		// Token: 0x0400137A RID: 4986
		public float respawnDelay = 0.5f;

		// Token: 0x0400137B RID: 4987
		public bool slideshowMode;

		// Token: 0x0400137C RID: 4988
		public bool autoRotation;

		// Token: 0x0400137D RID: 4989
		[Range(0.001f, 0.5f)]
		public float autoRotationSpeed = 0.1f;

		// Token: 0x0400137E RID: 4990
		private GameObject currentEffect;

		// Token: 0x0400137F RID: 4991
		private Text effectNameText;

		// Token: 0x04001380 RID: 4992
		private Text effectIndexText;

		// Token: 0x04001381 RID: 4993
		private ETFXMouseOrbit etfxMouseOrbit;
	}
}
