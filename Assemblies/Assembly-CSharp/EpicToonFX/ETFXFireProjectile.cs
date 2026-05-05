using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace EpicToonFX
{
	// Token: 0x02000372 RID: 882
	public class ETFXFireProjectile : MonoBehaviour
	{
		// Token: 0x06001D14 RID: 7444 RVA: 0x0007DBA4 File Offset: 0x0007BDA4
		private void Start()
		{
			if (this.gunPrefab != null)
			{
				this.instantiatedGun = Object.Instantiate<GameObject>(this.gunPrefab, Vector3.zero, Quaternion.identity);
				this.instantiatedGun.transform.SetParent(base.transform);
				this.instantiatedGun.transform.localPosition = Vector3.zero;
			}
			if (this.speedSlider != null)
			{
				this.speedSlider.onValueChanged.AddListener(new UnityAction<float>(this.OnSpeedSliderChanged));
				this.speed = this.speedSlider.value;
			}
			GameObject gameObject = GameObject.Find("ToggleAuto");
			if (gameObject != null)
			{
				this.fullAutoButton = gameObject.GetComponent<Toggle>();
			}
			this.UpdateDisplayName();
		}

		// Token: 0x06001D15 RID: 7445 RVA: 0x0007DC68 File Offset: 0x0007BE68
		private void Update()
		{
			if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D))
			{
				this.nextEffect();
			}
			else if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow))
			{
				this.previousEffect();
			}
			if (this.fullAutoButton != null)
			{
				this.isFullAuto = this.fullAutoButton.isOn;
			}
			if (this.instantiatedGun != null)
			{
				this.UpdateGunPositionAndRotation();
			}
			if (this.isFullAuto)
			{
				if (this.canShoot && Input.GetKey(KeyCode.Mouse0) && !EventSystem.current.IsPointerOverGameObject())
				{
					base.StartCoroutine(this.Shoot());
				}
			}
			else if (Input.GetKeyDown(KeyCode.Mouse0) && !EventSystem.current.IsPointerOverGameObject())
			{
				this.ShootProjectile();
			}
			if (this.speedSlider != null)
			{
				this.speedSlider.onValueChanged.AddListener(new UnityAction<float>(this.OnSpeedSliderChanged));
				this.speed = this.speedSlider.value;
			}
			Debug.DrawRay(Camera.main.ScreenPointToRay(Input.mousePosition).origin, Camera.main.ScreenPointToRay(Input.mousePosition).direction * 100f, Color.yellow);
		}

		// Token: 0x06001D16 RID: 7446 RVA: 0x0007DDB2 File Offset: 0x0007BFB2
		private IEnumerator Shoot()
		{
			this.canShoot = false;
			this.ShootProjectile();
			yield return new WaitForSeconds(this.fireRate);
			this.canShoot = true;
			yield break;
		}

		// Token: 0x06001D17 RID: 7447 RVA: 0x0007DDC4 File Offset: 0x0007BFC4
		private void ShootProjectile()
		{
			Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
			RaycastHit raycastHit;
			Vector3 normalized;
			if (Physics.Raycast(ray, out raycastHit, 100f))
			{
				normalized = (raycastHit.point - this.spawnPosition.position).normalized;
			}
			else
			{
				normalized = ray.direction.normalized;
			}
			Vector3 position = this.spawnPosition.position + normalized * this.spawnOffset;
			Quaternion rotation = Quaternion.LookRotation(normalized);
			Object.Instantiate<GameObject>(this.projectiles[this.currentProjectile], position, rotation).GetComponent<Rigidbody>().AddForce(normalized * this.speed);
		}

		// Token: 0x06001D18 RID: 7448 RVA: 0x0007DE74 File Offset: 0x0007C074
		private void UpdateGunPositionAndRotation()
		{
			Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
			RaycastHit raycastHit;
			Vector3 a;
			if (Physics.Raycast(ray, out raycastHit))
			{
				a = raycastHit.point;
			}
			else
			{
				a = ray.origin + ray.direction * 100f;
			}
			Vector3 normalized = (a - base.transform.position).normalized;
			float y = Mathf.Atan2(normalized.x, normalized.z) * 57.29578f;
			Quaternion b = Quaternion.Euler(-Mathf.Asin(normalized.y / normalized.magnitude) * 57.29578f, y, 0f);
			if (this.instantiatedGun != null)
			{
				this.instantiatedGun.transform.rotation = Quaternion.Slerp(this.instantiatedGun.transform.rotation, b, Time.deltaTime * 10f);
				this.instantiatedGun.transform.position = this.spawnPosition.position - this.instantiatedGun.transform.forward * this.gunOffset;
			}
		}

		// Token: 0x06001D19 RID: 7449 RVA: 0x0007DF9A File Offset: 0x0007C19A
		public void nextEffect()
		{
			if (this.currentProjectile < this.projectiles.Length - 1)
			{
				this.currentProjectile++;
			}
			else
			{
				this.currentProjectile = 0;
			}
			this.UpdateDisplayName();
		}

		// Token: 0x06001D1A RID: 7450 RVA: 0x0007DFCB File Offset: 0x0007C1CB
		public void previousEffect()
		{
			if (this.currentProjectile > 0)
			{
				this.currentProjectile--;
			}
			else
			{
				this.currentProjectile = this.projectiles.Length - 1;
			}
			this.UpdateDisplayName();
		}

		// Token: 0x06001D1B RID: 7451 RVA: 0x0007DFFC File Offset: 0x0007C1FC
		private void UpdateDisplayName()
		{
			Text text = (this.missileNameText != null) ? this.missileNameText : base.GetComponentInChildren<Text>();
			if (text != null)
			{
				string text2 = this.projectiles[this.currentProjectile].GetComponent<ETFXProjectileScript>().projectileParticle.name;
				if (this.cleanUpMissileName)
				{
					text2 = this.CleanUpMissileName(text2);
				}
				text.text = string.Format("{0} ({1}/{2})", text2, this.currentProjectile + 1, this.projectiles.Length);
			}
		}

		// Token: 0x06001D1C RID: 7452 RVA: 0x0007E088 File Offset: 0x0007C288
		private string CleanUpMissileName(string name)
		{
			name = name.Replace("Missile", "");
			name = name.Replace("Blue", " Blue");
			name = name.Replace("Red", " Red");
			name = name.Replace("Yellow", " Yellow");
			name = name.Replace("Green", " Green");
			name = name.Replace("Purple", " Purple");
			name = name.Replace("White", " White");
			name = name.Replace("Black", " Black");
			name = name.Replace("Pink", " Pink");
			name = name.Replace("Orange", " Orange");
			return name;
		}

		// Token: 0x06001D1D RID: 7453 RVA: 0x0007E14A File Offset: 0x0007C34A
		private void OnSpeedSliderChanged(float value)
		{
			this.speed = value;
		}

		// Token: 0x040013A2 RID: 5026
		public GameObject[] projectiles;

		// Token: 0x040013A3 RID: 5027
		[Header("GUI Links")]
		public Text missileNameText;

		// Token: 0x040013A4 RID: 5028
		public Toggle fullAutoButton;

		// Token: 0x040013A5 RID: 5029
		public Slider speedSlider;

		// Token: 0x040013A6 RID: 5030
		public bool cleanUpMissileName;

		// Token: 0x040013A7 RID: 5031
		[Header("Projectile Settings")]
		public Transform spawnPosition;

		// Token: 0x040013A8 RID: 5032
		[HideInInspector]
		public int currentProjectile;

		// Token: 0x040013A9 RID: 5033
		public float speed = 1000f;

		// Token: 0x040013AA RID: 5034
		public float spawnOffset = 0.3f;

		// Token: 0x040013AB RID: 5035
		[Header("Firing Settings")]
		public float fireRate = 0.13f;

		// Token: 0x040013AC RID: 5036
		public bool isFullAuto = true;

		// Token: 0x040013AD RID: 5037
		[Header("Gun Settings")]
		public GameObject gunPrefab;

		// Token: 0x040013AE RID: 5038
		public float gunOffset = 0.5f;

		// Token: 0x040013AF RID: 5039
		private bool canShoot = true;

		// Token: 0x040013B0 RID: 5040
		private GameObject instantiatedGun;
	}
}
