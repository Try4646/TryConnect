using System;
using UnityEngine;

namespace EpicToonFX
{
	// Token: 0x02000378 RID: 888
	public class ETFXProjectileScript : MonoBehaviour
	{
		// Token: 0x06001D3E RID: 7486 RVA: 0x0007E7CC File Offset: 0x0007C9CC
		private void Start()
		{
			this.rb = base.GetComponent<Rigidbody>();
			this.myTransform = base.transform;
			this.sphereCollider = base.GetComponent<SphereCollider>();
			this.projectileParticle = Object.Instantiate<GameObject>(this.projectileParticle, this.myTransform.position, this.myTransform.rotation);
			this.projectileParticle.transform.parent = this.myTransform;
			if (this.muzzleParticle)
			{
				this.muzzleParticle = Object.Instantiate<GameObject>(this.muzzleParticle, this.myTransform.position, this.myTransform.rotation);
				Object.Destroy(this.muzzleParticle, 1.5f);
			}
			this.RotateTowardsDirection(true);
		}

		// Token: 0x06001D3F RID: 7487 RVA: 0x0007E888 File Offset: 0x0007CA88
		private void FixedUpdate()
		{
			if (this.destroyed)
			{
				return;
			}
			float radius = this.sphereCollider ? this.sphereCollider.radius : this.colliderRadius;
			Vector3 vector = this.rb.linearVelocity;
			float maxDistance = vector.magnitude * Time.deltaTime;
			if (this.rb.useGravity)
			{
				vector += Physics.gravity * Time.deltaTime;
				maxDistance = vector.magnitude * Time.deltaTime;
			}
			RaycastHit raycastHit;
			if (Physics.SphereCast(this.myTransform.position, radius, vector, out raycastHit, maxDistance))
			{
				this.myTransform.position = raycastHit.point + raycastHit.normal * this.collideOffset;
				GameObject obj = Object.Instantiate<GameObject>(this.impactParticle, this.myTransform.position, Quaternion.FromToRotation(Vector3.up, raycastHit.normal));
				if (raycastHit.transform.tag == "Target")
				{
					ETFXTarget component = raycastHit.transform.GetComponent<ETFXTarget>();
					if (component != null)
					{
						component.OnHit();
					}
				}
				foreach (GameObject gameObject in this.trailParticles)
				{
					GameObject gameObject2 = this.myTransform.Find(this.projectileParticle.name + "/" + gameObject.name).gameObject;
					gameObject2.transform.parent = null;
					Object.Destroy(gameObject2, 3f);
				}
				Object.Destroy(this.projectileParticle, 3f);
				Object.Destroy(obj, 5f);
				this.DestroyMissile();
			}
			else
			{
				this.destroyTimer += Time.deltaTime;
				if (this.destroyTimer >= 5f)
				{
					this.DestroyMissile();
				}
			}
			this.RotateTowardsDirection(false);
		}

		// Token: 0x06001D40 RID: 7488 RVA: 0x0007EA64 File Offset: 0x0007CC64
		private void DestroyMissile()
		{
			this.destroyed = true;
			foreach (GameObject gameObject in this.trailParticles)
			{
				GameObject gameObject2 = this.myTransform.Find(this.projectileParticle.name + "/" + gameObject.name).gameObject;
				gameObject2.transform.parent = null;
				Object.Destroy(gameObject2, 3f);
			}
			Object.Destroy(this.projectileParticle, 3f);
			Object.Destroy(base.gameObject);
			ParticleSystem[] componentsInChildren = base.GetComponentsInChildren<ParticleSystem>();
			for (int j = 1; j < componentsInChildren.Length; j++)
			{
				ParticleSystem particleSystem = componentsInChildren[j];
				if (particleSystem.gameObject.name.Contains("Trail"))
				{
					particleSystem.transform.SetParent(null);
					Object.Destroy(particleSystem.gameObject, 2f);
				}
			}
		}

		// Token: 0x06001D41 RID: 7489 RVA: 0x0007EB44 File Offset: 0x0007CD44
		private void RotateTowardsDirection(bool immediate = false)
		{
			if (this.rb.linearVelocity != Vector3.zero)
			{
				Quaternion quaternion = Quaternion.LookRotation(this.rb.linearVelocity.normalized, Vector3.up);
				if (immediate)
				{
					this.myTransform.rotation = quaternion;
					return;
				}
				float t = Vector3.Angle(this.myTransform.forward, this.rb.linearVelocity.normalized) * Time.deltaTime;
				this.myTransform.rotation = Quaternion.Slerp(this.myTransform.rotation, quaternion, t);
			}
		}

		// Token: 0x040013D3 RID: 5075
		public GameObject impactParticle;

		// Token: 0x040013D4 RID: 5076
		public GameObject projectileParticle;

		// Token: 0x040013D5 RID: 5077
		public GameObject muzzleParticle;

		// Token: 0x040013D6 RID: 5078
		public GameObject[] trailParticles;

		// Token: 0x040013D7 RID: 5079
		[Header("Adjust if not using Sphere Collider")]
		public float colliderRadius = 1f;

		// Token: 0x040013D8 RID: 5080
		[Range(0f, 1f)]
		public float collideOffset = 0.15f;

		// Token: 0x040013D9 RID: 5081
		private Rigidbody rb;

		// Token: 0x040013DA RID: 5082
		private Transform myTransform;

		// Token: 0x040013DB RID: 5083
		private SphereCollider sphereCollider;

		// Token: 0x040013DC RID: 5084
		private float destroyTimer;

		// Token: 0x040013DD RID: 5085
		private bool destroyed;
	}
}
