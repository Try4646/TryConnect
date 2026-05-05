using System;
using UnityEngine;

// Token: 0x02000300 RID: 768
[RequireComponent(typeof(Rigidbody))]
public class UprightStabilizer : MonoBehaviour
{
	// Token: 0x06001A5A RID: 6746 RVA: 0x0006F502 File Offset: 0x0006D702
	private void Awake()
	{
		this._rb = base.GetComponent<Rigidbody>();
	}

	// Token: 0x06001A5B RID: 6747 RVA: 0x0006F510 File Offset: 0x0006D710
	private void FixedUpdate()
	{
		if (this._rb.isKinematic)
		{
			return;
		}
		Vector3 up = base.transform.up;
		Vector3 up2 = Vector3.up;
		Vector3 vector = Vector3.Cross(up, up2);
		if (vector.sqrMagnitude < 0.001f)
		{
			return;
		}
		vector.Normalize();
		float num = Vector3.Angle(up, up2) * 0.017453292f;
		this._rb.AddTorque(vector * (num * this.strength));
		float num2 = Vector3.Dot(this._rb.angularVelocity, vector);
		this._rb.angularVelocity -= vector * (num2 * this.angularDamping * Time.fixedDeltaTime);
	}

	// Token: 0x0400110F RID: 4367
	[SerializeField]
	private float strength = 5f;

	// Token: 0x04001110 RID: 4368
	[SerializeField]
	private float angularDamping = 2f;

	// Token: 0x04001111 RID: 4369
	private Rigidbody _rb;
}
