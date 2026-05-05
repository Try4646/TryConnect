using System;
using UnityEngine;

// Token: 0x020002F9 RID: 761
public class SpringRotationFollowerY : MonoBehaviour
{
	// Token: 0x06001A38 RID: 6712 RVA: 0x0006E83C File Offset: 0x0006CA3C
	private void Awake()
	{
		this._currentYaw = base.transform.localEulerAngles.y;
	}

	// Token: 0x06001A39 RID: 6713 RVA: 0x0006E854 File Offset: 0x0006CA54
	private void LateUpdate()
	{
		this.SmoothRotateY();
	}

	// Token: 0x06001A3A RID: 6714 RVA: 0x0006E85C File Offset: 0x0006CA5C
	private void SmoothRotateY()
	{
		Vector3 vector = base.transform.parent ? base.transform.parent.InverseTransformDirection(this.target.forward) : this.target.forward;
		vector.y = 0f;
		if (vector.sqrMagnitude < 0.0001f)
		{
			return;
		}
		vector.Normalize();
		float num = Mathf.Atan2(vector.x, vector.z) * 57.29578f;
		float num2 = Mathf.DeltaAngle(this._currentYaw, num) * this.springStrength;
		this._angularVelocity += num2 * Time.deltaTime;
		this._angularVelocity *= Mathf.Exp(-this.damping * Time.deltaTime);
		this._angularVelocity = Mathf.Clamp(this._angularVelocity, -this.maxSpeed, this.maxSpeed);
		this._currentYaw += this._angularVelocity * Time.deltaTime;
		base.transform.localRotation = Quaternion.Euler(0f, this._currentYaw, 0f);
	}

	// Token: 0x040010DB RID: 4315
	[Header("Settings")]
	[SerializeField]
	private float springStrength = 300f;

	// Token: 0x040010DC RID: 4316
	[SerializeField]
	private float damping = 12f;

	// Token: 0x040010DD RID: 4317
	[SerializeField]
	private float maxSpeed = 720f;

	// Token: 0x040010DE RID: 4318
	[Header("References")]
	[SerializeField]
	private Transform target;

	// Token: 0x040010DF RID: 4319
	private float _currentYaw;

	// Token: 0x040010E0 RID: 4320
	private float _angularVelocity;
}
