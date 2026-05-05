using System;
using System.Collections.Generic;
using Extensions;
using JetBrains.Annotations;
using UnityEngine;

// Token: 0x020001FD RID: 509
public class PlayerEyes : MonoBehaviour
{
	// Token: 0x170001AE RID: 430
	// (get) Token: 0x06001296 RID: 4758 RVA: 0x000503F1 File Offset: 0x0004E5F1
	public Transform EyeLeft
	{
		get
		{
			return this.eyeLeft;
		}
	}

	// Token: 0x170001AF RID: 431
	// (get) Token: 0x06001297 RID: 4759 RVA: 0x000503F9 File Offset: 0x0004E5F9
	public Transform EyeRight
	{
		get
		{
			return this.eyeRight;
		}
	}

	// Token: 0x06001298 RID: 4760 RVA: 0x00050401 File Offset: 0x0004E601
	private void Awake()
	{
		this._eyeRotationState = this.eyeLeft.rotation;
	}

	// Token: 0x06001299 RID: 4761 RVA: 0x00050414 File Offset: 0x0004E614
	private void OnEnable()
	{
		LocalManager instance = MonoSingleton<LocalManager>.Instance;
		instance.OnNewPlayerRegistered = (Action<PlayerReferences>)Delegate.Combine(instance.OnNewPlayerRegistered, new Action<PlayerReferences>(this.OnPlayerRegistered));
	}

	// Token: 0x0600129A RID: 4762 RVA: 0x0005043C File Offset: 0x0004E63C
	private void OnDisable()
	{
		if (MonoSingleton<LocalManager>.Instance != null)
		{
			LocalManager instance = MonoSingleton<LocalManager>.Instance;
			instance.OnNewPlayerRegistered = (Action<PlayerReferences>)Delegate.Remove(instance.OnNewPlayerRegistered, new Action<PlayerReferences>(this.OnPlayerRegistered));
		}
	}

	// Token: 0x0600129B RID: 4763 RVA: 0x00050471 File Offset: 0x0004E671
	private void OnPlayerRegistered(PlayerReferences references)
	{
		if (this._mouths.Contains(references.mouth))
		{
			return;
		}
		if (references.transform.GetComponentInChildren<PlayerEyes>() == this)
		{
			return;
		}
		this._mouths.Add(references.mouth);
	}

	// Token: 0x0600129C RID: 4764 RVA: 0x000504AC File Offset: 0x0004E6AC
	private void Start()
	{
		foreach (PlayerReferences playerReferences in MonoSingleton<LocalManager>.Instance.players)
		{
			if (!this._mouths.Contains(playerReferences.mouth) && !(playerReferences.transform.GetComponentInChildren<PlayerEyes>() == this))
			{
				this._mouths.Add(playerReferences.mouth);
			}
		}
	}

	// Token: 0x0600129D RID: 4765 RVA: 0x00050534 File Offset: 0x0004E734
	private void LateUpdate()
	{
		this.SelectTargetLookAt();
		if (this._targetLookAt && Vector3.Angle(this._targetLookAt.position - base.transform.position, base.transform.forward) > this.eyeRotationClampInDegrees)
		{
			this._targetLookAt = null;
		}
		this.SmoothRotateEyes();
	}

	// Token: 0x0600129E RID: 4766 RVA: 0x00050594 File Offset: 0x0004E794
	private void SelectTargetLookAt()
	{
		if (Time.time - this._lastTargetSetTime < this.changeTargetCd)
		{
			return;
		}
		if (this._mouths == null || this._mouths.Count == 0)
		{
			return;
		}
		Transform transform = null;
		float num = float.MinValue;
		foreach (PlayerMouth playerMouth in this._mouths)
		{
			if (playerMouth)
			{
				Vector3 from = playerMouth.headTransform.position - base.transform.position;
				float num2 = Vector3.Angle(from, base.transform.forward);
				if (num2 <= this.eyeRotationClampInDegrees)
				{
					float num3 = Mathf.Clamp(from.sqrMagnitude, 25f, 2500f);
					float currentAmplitude = playerMouth.currentAmplitude;
					float num4 = 0f;
					float num5 = num2 / this.eyeRotationClampInDegrees;
					if (currentAmplitude > 0f)
					{
						num4 = currentAmplitude / num3 * (1f - num5);
					}
					if (num4 > num)
					{
						num = num4;
						transform = playerMouth.headTransform;
					}
				}
			}
		}
		if (this._targetLookAt != transform)
		{
			this._targetLookAt = transform;
			this._lastTargetSetTime = Time.time;
		}
	}

	// Token: 0x0600129F RID: 4767 RVA: 0x000506DC File Offset: 0x0004E8DC
	private void SmoothRotateEyes()
	{
		Vector3 forward;
		if (this._targetLookAt)
		{
			forward = (this._targetLookAt.position - base.transform.position).normalized;
		}
		else
		{
			forward = base.transform.forward;
		}
		if (forward.sqrMagnitude < 0.0001f)
		{
			return;
		}
		float num;
		Vector3 a;
		(Quaternion.LookRotation(forward, base.transform.up) * Quaternion.Inverse(this._eyeRotationState)).ToAngleAxis(out num, out a);
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
		this.eyeLeft.rotation = this._eyeRotationState;
		this.eyeRight.rotation = this._eyeRotationState;
	}

	// Token: 0x04000BD4 RID: 3028
	[Header("Settings")]
	[SerializeField]
	private float springStrength = 300f;

	// Token: 0x04000BD5 RID: 3029
	[SerializeField]
	private float damping = 12f;

	// Token: 0x04000BD6 RID: 3030
	[SerializeField]
	private float maxSpeed = 720f;

	// Token: 0x04000BD7 RID: 3031
	[SerializeField]
	private float changeTargetCd = 2f;

	// Token: 0x04000BD8 RID: 3032
	[SerializeField]
	private float eyeRotationClampInDegrees = 60f;

	// Token: 0x04000BD9 RID: 3033
	[Header("References")]
	[SerializeField]
	private Transform eyeLeft;

	// Token: 0x04000BDA RID: 3034
	[SerializeField]
	private Transform eyeRight;

	// Token: 0x04000BDB RID: 3035
	[SerializeField]
	private List<PlayerMouth> _mouths = new List<PlayerMouth>();

	// Token: 0x04000BDC RID: 3036
	[SerializeField]
	[CanBeNull]
	private Transform _targetLookAt;

	// Token: 0x04000BDD RID: 3037
	private float _lastTargetSetTime;

	// Token: 0x04000BDE RID: 3038
	private Quaternion _eyeRotationState;

	// Token: 0x04000BDF RID: 3039
	private Vector3 _eyeAngularVelocity;
}
