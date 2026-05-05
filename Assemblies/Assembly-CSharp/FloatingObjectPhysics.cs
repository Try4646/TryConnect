using System;
using UnityEngine;

// Token: 0x02000149 RID: 329
[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Collider))]
public class FloatingObjectPhysics : MonoBehaviour
{
	// Token: 0x06000CAD RID: 3245 RVA: 0x00034EAC File Offset: 0x000330AC
	private void Awake()
	{
		this.rb = base.GetComponent<Rigidbody>();
		this.col = base.GetComponent<Collider>();
		this.rb.useGravity = false;
		this.rb.linearDamping = this.drag;
		this.rb.angularDamping = this.angularDrag;
		this.rb.isKinematic = false;
		this.originalPosition = base.transform.position;
		this.originalRotation = base.transform.rotation;
		this.hoverTimeOffset = Random.Range(0f, 6.2831855f);
		if (this.hoverRandomOffset > 0f)
		{
			this.hoverTimeOffset += Random.Range(-this.hoverRandomOffset, this.hoverRandomOffset);
		}
	}

	// Token: 0x06000CAE RID: 3246 RVA: 0x00034F6E File Offset: 0x0003316E
	private void Start()
	{
		if (this.mainCamera == null)
		{
			this.mainCamera = Camera.main;
		}
	}

	// Token: 0x06000CAF RID: 3247 RVA: 0x00034F89 File Offset: 0x00033189
	private void Update()
	{
		if (this.enableHover)
		{
			this.ApplyHoverMovement();
		}
		this.ReturnToOriginal();
	}

	// Token: 0x06000CB0 RID: 3248 RVA: 0x00034FA0 File Offset: 0x000331A0
	public void HandleClick(RaycastHit hit, Camera camera)
	{
		if (camera == null)
		{
			return;
		}
		Vector3 point = hit.point;
		Vector3 forward = camera.transform.forward;
		this.rb.AddForceAtPosition(forward * this.forceMultiplier, point, ForceMode.Impulse);
		Vector3 right = camera.transform.right;
		Vector3 up = camera.transform.up;
		Vector3 torque = (right + up * 0.5f) * this.forceMultiplier * 0.1f;
		this.rb.AddTorque(torque, ForceMode.Impulse);
		this.PlayParticleAtPoint(point, hit.normal);
		this.isReturning = false;
	}

	// Token: 0x06000CB1 RID: 3249 RVA: 0x00035044 File Offset: 0x00033244
	private void PlayParticleAtPoint(Vector3 position, Vector3 normal)
	{
		if (this.clickParticle == null)
		{
			if (this.debugParticles)
			{
				Debug.LogWarning("[FloatingObjectPhysics] No particle system assigned on " + base.gameObject.name);
			}
			return;
		}
		if (!this.instantiateParticles)
		{
			if (!this.clickParticle.gameObject.activeInHierarchy)
			{
				this.clickParticle.gameObject.SetActive(true);
			}
			if (this.clickParticle.isPlaying)
			{
				this.clickParticle.Stop();
				this.clickParticle.Clear();
			}
			this.clickParticle.transform.position = position;
			this.clickParticle.transform.rotation = Quaternion.LookRotation(normal);
			this.clickParticle.Play();
			if (this.debugParticles)
			{
				Debug.Log(string.Format("[FloatingObjectPhysics] Played particle at {0}", position));
			}
			return;
		}
		ParticleSystem particleSystem = Object.Instantiate<ParticleSystem>(this.clickParticle, position, Quaternion.LookRotation(normal));
		particleSystem.gameObject.SetActive(true);
		ParticleSystem.MainModule main = particleSystem.main;
		main.playOnAwake = false;
		particleSystem.Play();
		if (this.debugParticles)
		{
			Debug.Log(string.Format("[FloatingObjectPhysics] Instantiated and played particle at {0}", position));
		}
		if (main.duration > 0f)
		{
			float num = (main.startLifetime.constantMax > 0f) ? main.startLifetime.constantMax : main.startLifetime.constant;
			Object.Destroy(particleSystem.gameObject, main.duration + num + 1f);
			return;
		}
		Object.Destroy(particleSystem.gameObject, 5f);
	}

	// Token: 0x06000CB2 RID: 3250 RVA: 0x000351E4 File Offset: 0x000333E4
	private void ApplyHoverMovement()
	{
		if (Vector3.Distance(base.transform.position, this.originalPosition) > 0.5f || this.rb.linearVelocity.magnitude > this.velocityThreshold * 2f)
		{
			return;
		}
		float d = Mathf.Sin(Time.time * this.hoverSpeed + this.hoverTimeOffset) * this.hoverAmplitude;
		Vector3 force = (this.originalPosition + this.hoverDirection.normalized * d - base.transform.position) * this.returnLerpSpeed * 0.5f;
		this.rb.AddForce(force, ForceMode.Force);
	}

	// Token: 0x06000CB3 RID: 3251 RVA: 0x000352A0 File Offset: 0x000334A0
	private void ReturnToOriginal()
	{
		if (!this.isReturning && this.rb.linearVelocity.magnitude < this.velocityThreshold)
		{
			this.isReturning = true;
		}
		Vector3 a = this.originalPosition;
		if (this.enableHover && Vector3.Distance(base.transform.position, this.originalPosition) <= 0.5f && this.rb.linearVelocity.magnitude <= this.velocityThreshold * 2f)
		{
			float d = Mathf.Sin(Time.time * this.hoverSpeed + this.hoverTimeOffset) * this.hoverAmplitude;
			a = this.originalPosition + this.hoverDirection.normalized * d;
		}
		if (this.isReturning || Vector3.Distance(base.transform.position, this.originalPosition) < 1f)
		{
			Vector3 vector = a - base.transform.position;
			float magnitude = vector.magnitude;
			if (magnitude > this.positionThreshold)
			{
				Vector3 force = vector.normalized * this.returnLerpSpeed * magnitude;
				this.rb.AddForce(force, ForceMode.Force);
			}
			else
			{
				Vector3 b = (a - base.transform.position) / Time.deltaTime;
				this.rb.linearVelocity = Vector3.Lerp(this.rb.linearVelocity, b, Time.deltaTime * this.returnLerpSpeed);
			}
			float num;
			Vector3 a2;
			(this.originalRotation * Quaternion.Inverse(base.transform.rotation)).ToAngleAxis(out num, out a2);
			if (num > this.rotationThreshold)
			{
				if (num > 180f)
				{
					num -= 360f;
				}
				Vector3 torque = a2 * (num * 0.017453292f * this.rotationLerpSpeed);
				this.rb.AddTorque(torque, ForceMode.Force);
				return;
			}
			base.transform.rotation = Quaternion.Lerp(base.transform.rotation, this.originalRotation, Time.deltaTime * this.rotationLerpSpeed);
			this.rb.angularVelocity = Vector3.Lerp(this.rb.angularVelocity, Vector3.zero, Time.deltaTime * this.rotationLerpSpeed);
		}
	}

	// Token: 0x06000CB4 RID: 3252 RVA: 0x000354E4 File Offset: 0x000336E4
	private void OnDrawGizmosSelected()
	{
		if (Application.isPlaying)
		{
			Gizmos.color = Color.green;
			Gizmos.DrawWireSphere(this.originalPosition, 0.1f);
			Gizmos.DrawLine(base.transform.position, this.originalPosition);
		}
	}

	// Token: 0x06000CB5 RID: 3253 RVA: 0x0003551D File Offset: 0x0003371D
	public void ResetOriginalTransform()
	{
		this.originalPosition = base.transform.position;
		this.originalRotation = base.transform.rotation;
	}

	// Token: 0x0400080F RID: 2063
	[Header("Physics Settings")]
	[SerializeField]
	private float forceMultiplier = 10f;

	// Token: 0x04000810 RID: 2064
	[SerializeField]
	private float returnLerpSpeed = 2f;

	// Token: 0x04000811 RID: 2065
	[SerializeField]
	private float rotationLerpSpeed = 2f;

	// Token: 0x04000812 RID: 2066
	[SerializeField]
	private float drag = 2f;

	// Token: 0x04000813 RID: 2067
	[SerializeField]
	private float angularDrag = 2f;

	// Token: 0x04000814 RID: 2068
	[Header("Return Threshold")]
	[SerializeField]
	private float positionThreshold = 0.01f;

	// Token: 0x04000815 RID: 2069
	[SerializeField]
	private float rotationThreshold = 0.1f;

	// Token: 0x04000816 RID: 2070
	[SerializeField]
	private float velocityThreshold = 0.1f;

	// Token: 0x04000817 RID: 2071
	[Header("Hover Movement")]
	[SerializeField]
	private bool enableHover = true;

	// Token: 0x04000818 RID: 2072
	[SerializeField]
	private float hoverAmplitude = 0.1f;

	// Token: 0x04000819 RID: 2073
	[SerializeField]
	private float hoverSpeed = 1f;

	// Token: 0x0400081A RID: 2074
	[SerializeField]
	private Vector3 hoverDirection = Vector3.up;

	// Token: 0x0400081B RID: 2075
	[Tooltip("Additional random offset per object for variation")]
	[SerializeField]
	private float hoverRandomOffset;

	// Token: 0x0400081C RID: 2076
	[Header("Visual Effects")]
	[SerializeField]
	private ParticleSystem clickParticle;

	// Token: 0x0400081D RID: 2077
	[Tooltip("If true, will instantiate a new particle system for each click. Otherwise, moves the existing one.")]
	[SerializeField]
	private bool instantiateParticles = true;

	// Token: 0x0400081E RID: 2078
	[SerializeField]
	private bool debugParticles;

	// Token: 0x0400081F RID: 2079
	private Rigidbody rb;

	// Token: 0x04000820 RID: 2080
	private Collider col;

	// Token: 0x04000821 RID: 2081
	private Camera mainCamera;

	// Token: 0x04000822 RID: 2082
	private Vector3 originalPosition;

	// Token: 0x04000823 RID: 2083
	private Quaternion originalRotation;

	// Token: 0x04000824 RID: 2084
	private bool isReturning;

	// Token: 0x04000825 RID: 2085
	private float hoverTimeOffset;
}
