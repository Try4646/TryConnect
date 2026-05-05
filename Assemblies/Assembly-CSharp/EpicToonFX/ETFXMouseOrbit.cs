using System;
using System.Collections;
using UnityEngine;

namespace EpicToonFX
{
	// Token: 0x02000376 RID: 886
	public class ETFXMouseOrbit : MonoBehaviour
	{
		// Token: 0x06001D2F RID: 7471 RVA: 0x0007E38C File Offset: 0x0007C58C
		private void Start()
		{
			Vector3 eulerAngles = base.transform.eulerAngles;
			this.rotationYAxis = eulerAngles.y;
			this.rotationXAxis = eulerAngles.x;
			if (base.GetComponent<Rigidbody>())
			{
				base.GetComponent<Rigidbody>().freezeRotation = true;
			}
		}

		// Token: 0x06001D30 RID: 7472 RVA: 0x0007E3D8 File Offset: 0x0007C5D8
		private void Update()
		{
			if (this.target)
			{
				if (Input.GetMouseButton(1))
				{
					this.velocityX += this.xSpeed * Input.GetAxis("Mouse X") * this.distance * 0.02f;
					this.velocityY += this.ySpeed * Input.GetAxis("Mouse Y") * 0.02f;
					if (this.isAutoRotating)
					{
						this.StopAutoRotation();
					}
				}
				this.distance = Mathf.Clamp(this.distance - Input.GetAxis("Mouse ScrollWheel") * 15f, this.distanceMin, this.distanceMax);
			}
		}

		// Token: 0x06001D31 RID: 7473 RVA: 0x0007E488 File Offset: 0x0007C688
		private void FixedUpdate()
		{
			if (this.target)
			{
				this.rotationYAxis += this.velocityX;
				this.rotationXAxis -= this.velocityY;
				this.rotationXAxis = ETFXMouseOrbit.ClampAngle(this.rotationXAxis, this.yMinLimit, this.yMaxLimit);
				Quaternion rotation = Quaternion.Euler(this.rotationXAxis, this.rotationYAxis, 0f);
				RaycastHit raycastHit;
				if (Physics.Linecast(this.target.position, base.transform.position, out raycastHit))
				{
					this.distance -= raycastHit.distance;
				}
				Vector3 point = new Vector3(0f, 0f, -this.distance);
				Vector3 position = Vector3.Lerp(base.transform.position, rotation * point + this.target.position, 0.6f);
				base.transform.rotation = rotation;
				base.transform.position = position;
				this.velocityX = Mathf.Lerp(this.velocityX, 0f, Time.deltaTime * this.smoothTime);
				this.velocityY = Mathf.Lerp(this.velocityY, 0f, Time.deltaTime * this.smoothTime);
			}
		}

		// Token: 0x06001D32 RID: 7474 RVA: 0x0007E5D3 File Offset: 0x0007C7D3
		public static float ClampAngle(float angle, float min, float max)
		{
			if (angle < -360f)
			{
				angle += 360f;
			}
			if (angle > 360f)
			{
				angle -= 360f;
			}
			return Mathf.Clamp(angle, min, max);
		}

		// Token: 0x06001D33 RID: 7475 RVA: 0x0007E5FF File Offset: 0x0007C7FF
		public void InitializeAutoRotation()
		{
			this.isAutoRotating = true;
			base.StartCoroutine(this.AutoRotate());
		}

		// Token: 0x06001D34 RID: 7476 RVA: 0x0007E615 File Offset: 0x0007C815
		public void SetAutoRotationSpeed(float rotationSpeed)
		{
			this.maxVelocityX = rotationSpeed;
		}

		// Token: 0x06001D35 RID: 7477 RVA: 0x0007E620 File Offset: 0x0007C820
		private void StopAutoRotation()
		{
			if (this.etfxEffectController != null)
			{
				this.etfxEffectController.autoRotation = false;
			}
			if (this.etfxEffectControllerPooled != null)
			{
				this.etfxEffectControllerPooled.autoRotation = false;
			}
			this.isAutoRotating = false;
			base.StopAllCoroutines();
		}

		// Token: 0x06001D36 RID: 7478 RVA: 0x0007E66E File Offset: 0x0007C86E
		private IEnumerator AutoRotate()
		{
			int lerpSteps = 0;
			while (lerpSteps < 30)
			{
				this.velocityX = Mathf.Lerp(this.velocityX, this.maxVelocityX, this.autoRotationSmoothing);
				yield return new WaitForFixedUpdate();
			}
			while (this.isAutoRotating)
			{
				this.velocityX = this.maxVelocityX;
				yield return new WaitForFixedUpdate();
			}
			yield break;
		}

		// Token: 0x040013BD RID: 5053
		public Transform target;

		// Token: 0x040013BE RID: 5054
		public float distance = 12f;

		// Token: 0x040013BF RID: 5055
		public float xSpeed = 120f;

		// Token: 0x040013C0 RID: 5056
		public float ySpeed = 120f;

		// Token: 0x040013C1 RID: 5057
		public float yMinLimit = -20f;

		// Token: 0x040013C2 RID: 5058
		public float yMaxLimit = 80f;

		// Token: 0x040013C3 RID: 5059
		public float distanceMin = 8f;

		// Token: 0x040013C4 RID: 5060
		public float distanceMax = 15f;

		// Token: 0x040013C5 RID: 5061
		public float smoothTime = 2f;

		// Token: 0x040013C6 RID: 5062
		private float rotationYAxis;

		// Token: 0x040013C7 RID: 5063
		private float rotationXAxis;

		// Token: 0x040013C8 RID: 5064
		private float velocityX;

		// Token: 0x040013C9 RID: 5065
		private float maxVelocityX = 0.1f;

		// Token: 0x040013CA RID: 5066
		private float velocityY;

		// Token: 0x040013CB RID: 5067
		private readonly float autoRotationSmoothing = 0.02f;

		// Token: 0x040013CC RID: 5068
		[HideInInspector]
		public bool isAutoRotating;

		// Token: 0x040013CD RID: 5069
		[HideInInspector]
		public ETFXEffectController etfxEffectController;

		// Token: 0x040013CE RID: 5070
		[HideInInspector]
		public ETFXEffectControllerPooled etfxEffectControllerPooled;
	}
}
