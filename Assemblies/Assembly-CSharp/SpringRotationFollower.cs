using System;
using UnityEngine;

// Token: 0x020002F8 RID: 760
public class SpringRotationFollower : MonoBehaviour
{
	// Token: 0x06001A34 RID: 6708 RVA: 0x0006E6EF File Offset: 0x0006C8EF
	private void Awake()
	{
		this._rotationState = this.target.rotation;
	}

	// Token: 0x06001A35 RID: 6709 RVA: 0x0006E702 File Offset: 0x0006C902
	private void LateUpdate()
	{
		this.SmoothRotate();
	}

	// Token: 0x06001A36 RID: 6710 RVA: 0x0006E70C File Offset: 0x0006C90C
	private void SmoothRotate()
	{
		Vector3 forward = this.target.forward;
		if (forward.sqrMagnitude < 0.0001f)
		{
			return;
		}
		float num;
		Vector3 a;
		(Quaternion.LookRotation(forward, this.target.up) * Quaternion.Inverse(this._rotationState)).ToAngleAxis(out num, out a);
		if (num > 180f)
		{
			num -= 360f;
		}
		Vector3 a2 = a * (num * this.springStrength);
		this._angularVelocity += a2 * Time.deltaTime;
		this._angularVelocity *= Mathf.Exp(-this.damping * Time.deltaTime);
		this._angularVelocity = Vector3.ClampMagnitude(this._angularVelocity, this.maxSpeed);
		Quaternion lhs = Quaternion.Euler(this._angularVelocity * Time.deltaTime);
		this._rotationState = lhs * this._rotationState;
		base.transform.rotation = this._rotationState;
	}

	// Token: 0x040010D5 RID: 4309
	[Header("Settings")]
	[SerializeField]
	private float springStrength = 300f;

	// Token: 0x040010D6 RID: 4310
	[SerializeField]
	private float damping = 12f;

	// Token: 0x040010D7 RID: 4311
	[SerializeField]
	private float maxSpeed = 720f;

	// Token: 0x040010D8 RID: 4312
	[Header("References")]
	[SerializeField]
	private Transform target;

	// Token: 0x040010D9 RID: 4313
	private Quaternion _rotationState;

	// Token: 0x040010DA RID: 4314
	private Vector3 _angularVelocity;
}
