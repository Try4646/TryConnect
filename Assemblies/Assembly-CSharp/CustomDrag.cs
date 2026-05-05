using System;
using UnityEngine;

// Token: 0x020002D1 RID: 721
[RequireComponent(typeof(Rigidbody))]
public class CustomDrag : MonoBehaviour
{
	// Token: 0x0600196A RID: 6506 RVA: 0x0006AB2F File Offset: 0x00068D2F
	private void Awake()
	{
		this.rb = base.GetComponent<Rigidbody>();
		this.rb.linearDamping = 0f;
		this.rb.angularDamping = 0f;
	}

	// Token: 0x0600196B RID: 6507 RVA: 0x0006AB5D File Offset: 0x00068D5D
	private void FixedUpdate()
	{
		if (this.rb.isKinematic)
		{
			return;
		}
		this.ApplyLocalLinearDrag();
		this.ApplyLocalAngularDrag();
	}

	// Token: 0x0600196C RID: 6508 RVA: 0x0006AB7C File Offset: 0x00068D7C
	private void ApplyLocalLinearDrag()
	{
		Vector3 direction = base.transform.InverseTransformDirection(this.rb.linearVelocity);
		direction.x *= 1f - this.linearDrag.x * Time.fixedDeltaTime;
		direction.y *= 1f - this.linearDrag.y * Time.fixedDeltaTime;
		direction.z *= 1f - this.linearDrag.z * Time.fixedDeltaTime;
		this.rb.linearVelocity = base.transform.TransformDirection(direction);
	}

	// Token: 0x0600196D RID: 6509 RVA: 0x0006AC20 File Offset: 0x00068E20
	private void ApplyLocalAngularDrag()
	{
		Vector3 direction = base.transform.InverseTransformDirection(this.rb.angularVelocity);
		direction.x *= 1f - this.angularDrag.x * Time.fixedDeltaTime;
		direction.y *= 1f - this.angularDrag.y * Time.fixedDeltaTime;
		direction.z *= 1f - this.angularDrag.z * Time.fixedDeltaTime;
		this.rb.angularVelocity = base.transform.TransformDirection(direction);
	}

	// Token: 0x04001056 RID: 4182
	public Vector3 linearDrag;

	// Token: 0x04001057 RID: 4183
	public Vector3 angularDrag;

	// Token: 0x04001058 RID: 4184
	private Rigidbody rb;
}
