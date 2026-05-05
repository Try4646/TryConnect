using System;
using Extensions;
using UnityEngine;

// Token: 0x020001C9 RID: 457
public class NPCEyes : MonoBehaviour
{
	// Token: 0x06001077 RID: 4215 RVA: 0x00046BCC File Offset: 0x00044DCC
	private void Awake()
	{
		if (this.eyeLeft != null)
		{
			this._eyeRotationState = this.eyeLeft.rotation;
		}
		if (this.headTransform == null)
		{
			this.headTransform = base.transform;
		}
		this._interestRadiusSqr = this.interestRadius * this.interestRadius;
	}

	// Token: 0x06001078 RID: 4216 RVA: 0x00046C28 File Offset: 0x00044E28
	private void LateUpdate()
	{
		this.SelectTargetLookAt();
		if (this._targetLookAt && Vector3.Angle(this._targetLookAt.position - this.headTransform.position, this.headTransform.forward) > this.eyeRotationClampInDegrees)
		{
			this._targetLookAt = null;
		}
		this.SmoothRotateEyes();
	}

	// Token: 0x06001079 RID: 4217 RVA: 0x00046C88 File Offset: 0x00044E88
	private void SelectTargetLookAt()
	{
		if (Time.time - this._lastTargetSetTime < this.changeTargetCd)
		{
			return;
		}
		Transform transform = null;
		Vector3 zero = Vector3.zero;
		bool flag = false;
		float num = float.MinValue;
		if (MonoSingleton<LocalManager>.Instance != null)
		{
			foreach (PlayerReferences playerReferences in MonoSingleton<LocalManager>.Instance.players)
			{
				if (!(((playerReferences != null) ? playerReferences.headTransform : null) == null))
				{
					Vector3 from = playerReferences.headTransform.position - this.headTransform.position;
					float sqrMagnitude = from.sqrMagnitude;
					if (sqrMagnitude <= this._interestRadiusSqr)
					{
						float num2 = Vector3.Angle(from, this.headTransform.forward);
						if (num2 <= this.eyeRotationClampInDegrees)
						{
							float num3 = 1f / (sqrMagnitude + 1f) * (1f - num2 / this.eyeRotationClampInDegrees);
							if (num3 > num)
							{
								num = num3;
								transform = playerReferences.headTransform;
								flag = false;
							}
						}
					}
				}
			}
		}
		if (flag)
		{
			this._targetLookAt = null;
			this._targetPosition = zero;
			this._hasPositionTarget = true;
			this._lastTargetSetTime = Time.time;
			return;
		}
		if (this._targetLookAt != transform)
		{
			this._targetLookAt = transform;
			this._hasPositionTarget = false;
			this._lastTargetSetTime = Time.time;
		}
	}

	// Token: 0x0600107A RID: 4218 RVA: 0x00046DFC File Offset: 0x00044FFC
	private void SmoothRotateEyes()
	{
		Vector3 forward;
		if (this._hasPositionTarget)
		{
			forward = (this._targetPosition - this.headTransform.position).normalized;
		}
		else if (this._targetLookAt)
		{
			forward = (this._targetLookAt.position - this.headTransform.position).normalized;
		}
		else
		{
			forward = this.headTransform.forward;
		}
		if (forward.sqrMagnitude < 0.0001f)
		{
			return;
		}
		float num;
		Vector3 a;
		(Quaternion.LookRotation(forward, Vector3.up) * Quaternion.Inverse(this._eyeRotationState)).ToAngleAxis(out num, out a);
		if (num > 180f)
		{
			num -= 360f;
		}
		Vector3 a2 = a * (num * this.springStrength);
		this._eyeAngularVelocity += a2 * Time.deltaTime;
		this._eyeAngularVelocity *= Mathf.Exp(-this.damping * Time.deltaTime);
		this._eyeAngularVelocity = Vector3.ClampMagnitude(this._eyeAngularVelocity, this.maxSpeed);
		Quaternion lhs = Quaternion.Euler(this._eyeAngularVelocity * Time.deltaTime);
		this._eyeRotationState = lhs * this._eyeRotationState;
		if (this.eyeLeft != null)
		{
			this.eyeLeft.rotation = this._eyeRotationState;
		}
		if (this.eyeRight != null)
		{
			this.eyeRight.rotation = this._eyeRotationState;
		}
	}

	// Token: 0x04000AA2 RID: 2722
	[Header("Settings")]
	[SerializeField]
	private float springStrength = 300f;

	// Token: 0x04000AA3 RID: 2723
	[SerializeField]
	private float damping = 12f;

	// Token: 0x04000AA4 RID: 2724
	[SerializeField]
	private float maxSpeed = 720f;

	// Token: 0x04000AA5 RID: 2725
	[SerializeField]
	private float changeTargetCd = 2f;

	// Token: 0x04000AA6 RID: 2726
	[SerializeField]
	private float eyeRotationClampInDegrees = 60f;

	// Token: 0x04000AA7 RID: 2727
	[SerializeField]
	private float interestRadius = 25f;

	// Token: 0x04000AA8 RID: 2728
	[Header("References")]
	[SerializeField]
	private Transform eyeLeft;

	// Token: 0x04000AA9 RID: 2729
	[SerializeField]
	private Transform eyeRight;

	// Token: 0x04000AAA RID: 2730
	[SerializeField]
	private Transform headTransform;

	// Token: 0x04000AAB RID: 2731
	private Transform _targetLookAt;

	// Token: 0x04000AAC RID: 2732
	private Vector3 _targetPosition;

	// Token: 0x04000AAD RID: 2733
	private bool _hasPositionTarget;

	// Token: 0x04000AAE RID: 2734
	private float _lastTargetSetTime;

	// Token: 0x04000AAF RID: 2735
	private Quaternion _eyeRotationState;

	// Token: 0x04000AB0 RID: 2736
	private Vector3 _eyeAngularVelocity;

	// Token: 0x04000AB1 RID: 2737
	private float _interestRadiusSqr;
}
