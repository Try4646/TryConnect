using System;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using Extensions;
using UnityEngine;

// Token: 0x020002A9 RID: 681
public class EyeLookTarget : MonoBehaviour
{
	// Token: 0x17000224 RID: 548
	// (get) Token: 0x060017FF RID: 6143 RVA: 0x00065A8C File Offset: 0x00063C8C
	private Transform leftEye
	{
		get
		{
			PlayerOrgans playerOrgans = this.playerOrgans;
			if (playerOrgans == null)
			{
				return null;
			}
			return playerOrgans.LeftEye;
		}
	}

	// Token: 0x17000225 RID: 549
	// (get) Token: 0x06001800 RID: 6144 RVA: 0x00065A9F File Offset: 0x00063C9F
	private Transform rightEye
	{
		get
		{
			PlayerOrgans playerOrgans = this.playerOrgans;
			if (playerOrgans == null)
			{
				return null;
			}
			return playerOrgans.RightEye;
		}
	}

	// Token: 0x06001801 RID: 6145 RVA: 0x00065AB2 File Offset: 0x00063CB2
	private void Start()
	{
		this.playerOrgans = MonoSingleton<LocalManager>.Instance.players.Find((PlayerReferences player) => player.identity.isLocalPlayer).organs;
	}

	// Token: 0x06001802 RID: 6146 RVA: 0x00065AF0 File Offset: 0x00063CF0
	private void LateUpdate()
	{
		if (this.isDestroying)
		{
			return;
		}
		Vector3 position = base.transform.position;
		if (this.leftEye != null)
		{
			this.SmoothRotateEye(this.leftEye, ref this.leftEyeYawVelocity, ref this.leftEyePitchVelocity, this.leftEyeMinDegrees, this.leftEyeMaxDegrees, position);
		}
		if (this.rightEye != null)
		{
			this.SmoothRotateEye(this.rightEye, ref this.rightEyeYawVelocity, ref this.rightEyePitchVelocity, this.rightEyeMinDegrees, this.rightEyeMaxDegrees, position);
		}
	}

	// Token: 0x06001803 RID: 6147 RVA: 0x00065B78 File Offset: 0x00063D78
	private Quaternion ClampRotation(Transform eye, Quaternion targetRotation, float minDegrees, float maxDegrees)
	{
		if (eye.parent == null)
		{
			return targetRotation;
		}
		Vector3 forward = eye.parent.forward;
		Vector3 direction = targetRotation * Vector3.forward;
		Vector3 vector = eye.parent.InverseTransformDirection(direction);
		float num = Mathf.Atan2(vector.x, vector.z) * 57.29578f;
		float value = Mathf.Asin(vector.y) * 57.29578f;
		num = Mathf.Clamp(num, minDegrees, maxDegrees);
		float num2 = Mathf.Clamp(value, minDegrees, maxDegrees);
		float f = num * 0.017453292f;
		float f2 = num2 * 0.017453292f;
		Vector3 direction2 = new Vector3(Mathf.Sin(f) * Mathf.Cos(f2), Mathf.Sin(f2), Mathf.Cos(f) * Mathf.Cos(f2));
		return Quaternion.LookRotation(eye.parent.TransformDirection(direction2));
	}

	// Token: 0x06001804 RID: 6148 RVA: 0x00065C44 File Offset: 0x00063E44
	public void SmoothEyesToForwardAndDestroy()
	{
		if (this.isDestroying)
		{
			return;
		}
		this.isDestroying = true;
		if (this.playerOrgans == null)
		{
			Object.Destroy(base.gameObject);
			return;
		}
		Transform leftEye = this.leftEye;
		Transform transform;
		if ((transform = ((leftEye != null) ? leftEye.parent : null)) == null)
		{
			Transform rightEye = this.rightEye;
			transform = ((rightEye != null) ? rightEye.parent : null);
		}
		Transform transform2 = transform;
		if (transform2 == null)
		{
			Object.Destroy(base.gameObject);
			return;
		}
		Quaternion rotation = transform2.rotation;
		float duration = 0.3f;
		int completed = 0;
		int total = ((this.leftEye != null) ? 1 : 0) + ((this.rightEye != null) ? 1 : 0);
		if (total == 0)
		{
			Object.Destroy(base.gameObject);
			return;
		}
		TweenCallback action = delegate()
		{
			int completed = completed;
			completed++;
			if (completed >= total)
			{
				Object.Destroy(this.gameObject);
			}
		};
		if (this.leftEye != null)
		{
			this.leftEye.DORotateQuaternion(rotation, duration).OnComplete(action);
		}
		if (this.rightEye != null)
		{
			this.rightEye.DORotateQuaternion(rotation, duration).OnComplete(action);
		}
	}

	// Token: 0x06001805 RID: 6149 RVA: 0x00065D70 File Offset: 0x00063F70
	private void SmoothRotateEye(Transform eye, ref float yawVelocity, ref float pitchVelocity, float minDegrees, float maxDegrees, Vector3 targetPoint)
	{
		if (eye.parent == null)
		{
			return;
		}
		Vector3 lhs = targetPoint - eye.position;
		if (lhs == Vector3.zero)
		{
			return;
		}
		Vector3 vector = eye.parent.InverseTransformDirection(lhs.normalized);
		float num = Mathf.Atan2(vector.x, vector.z) * 57.29578f;
		float num2 = Mathf.Asin(vector.y) * 57.29578f;
		num = Mathf.Clamp(num, minDegrees, maxDegrees);
		num2 = Mathf.Clamp(num2, minDegrees, maxDegrees);
		Vector3 localEulerAngles = eye.localEulerAngles;
		float current = this.NormalizeAngle(localEulerAngles.y);
		float current2 = this.NormalizeAngle(localEulerAngles.x);
		float y = Mathf.SmoothDampAngle(current, num, ref yawVelocity, this.smoothTime);
		float x = Mathf.SmoothDampAngle(current2, num2, ref pitchVelocity, this.smoothTime);
		eye.localRotation = Quaternion.Euler(x, y, 0f);
	}

	// Token: 0x06001806 RID: 6150 RVA: 0x00065E53 File Offset: 0x00064053
	private float NormalizeAngle(float angle)
	{
		if (angle > 180f)
		{
			angle -= 360f;
		}
		return angle;
	}

	// Token: 0x04000F7F RID: 3967
	[Header("Smooth Rotation Settings")]
	[SerializeField]
	private float smoothTime = 0.12f;

	// Token: 0x04000F80 RID: 3968
	[Header("Left Eye Rotation Limits")]
	[SerializeField]
	private float leftEyeMinDegrees = -45f;

	// Token: 0x04000F81 RID: 3969
	[SerializeField]
	private float leftEyeMaxDegrees = 45f;

	// Token: 0x04000F82 RID: 3970
	[Header("Right Eye Rotation Limits")]
	[SerializeField]
	private float rightEyeMinDegrees = -45f;

	// Token: 0x04000F83 RID: 3971
	[SerializeField]
	private float rightEyeMaxDegrees = 45f;

	// Token: 0x04000F84 RID: 3972
	private PlayerOrgans playerOrgans;

	// Token: 0x04000F85 RID: 3973
	public bool isDestroying;

	// Token: 0x04000F86 RID: 3974
	private float leftEyeYawVelocity;

	// Token: 0x04000F87 RID: 3975
	private float leftEyePitchVelocity;

	// Token: 0x04000F88 RID: 3976
	private float rightEyeYawVelocity;

	// Token: 0x04000F89 RID: 3977
	private float rightEyePitchVelocity;
}
