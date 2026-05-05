using System;
using Extensions;
using Mirror;
using UnityEngine;

// Token: 0x020001F5 RID: 501
public class PlayerCameraController : NetworkBehaviour
{
	// Token: 0x060011E1 RID: 4577 RVA: 0x0004D37C File Offset: 0x0004B57C
	private void Awake()
	{
		this._cs = Resources.Load<CameraSettings>("CameraSettings");
		this._cam = MonoSingleton<LocalManager>.Instance.mainCamera;
	}

	// Token: 0x060011E2 RID: 4578 RVA: 0x0004D3A0 File Offset: 0x0004B5A0
	public override void OnStartClient()
	{
		base.OnStartClient();
		if (!base.isLocalPlayer)
		{
			base.enabled = false;
			return;
		}
		this._cam.transform.SetParent(base.transform);
		this._cam.transform.localPosition = Vector3.zero;
		this._cam.transform.localRotation = Quaternion.identity;
	}

	// Token: 0x060011E3 RID: 4579 RVA: 0x0004D404 File Offset: 0x0004B604
	private void Start()
	{
		this._baseFOV = this._cs.baseFOV.value;
		this._cam.fieldOfView = this._baseFOV * this._runFOVMultiplier;
		this._previousHeadPosition = this.playerHead.transform.position;
	}

	// Token: 0x060011E4 RID: 4580 RVA: 0x0004D455 File Offset: 0x0004B655
	private void LateUpdate()
	{
		this.SetBaseFOV();
		this.SetRunFOVMultiplier();
		this.HeadSway();
		this.HeadBob();
		this.SetFinalCameraProperties();
	}

	// Token: 0x060011E5 RID: 4581 RVA: 0x0004D475 File Offset: 0x0004B675
	private void SetFinalCameraProperties()
	{
		this._cam.fieldOfView = this._baseFOV * this._runFOVMultiplier;
		base.transform.localPosition = this._finalHeadBob;
		base.transform.localRotation = this._finalHeadSway;
	}

	// Token: 0x060011E6 RID: 4582 RVA: 0x0004D4B4 File Offset: 0x0004B6B4
	private void SetBaseFOV()
	{
		float b = InputEvents.IsZoomPressed ? this._cs.zoomFOV : this._cs.baseFOV.value;
		this._baseFOV = Mathf.Lerp(this._baseFOV, b, this._cs.zoomLerpSpeed * Time.deltaTime);
	}

	// Token: 0x060011E7 RID: 4583 RVA: 0x0004D50C File Offset: 0x0004B70C
	private void SetRunFOVMultiplier()
	{
		float b = 1f;
		if (InputEvents.IsSprintPressed && Vector3.ProjectOnPlane(this.rigidBody.linearVelocity, Vector3.up).magnitude > this._cs.runFOVThreshold)
		{
			b = this._cs.runFOVMultiplier;
		}
		this._runFOVMultiplier = Mathf.Lerp(this._runFOVMultiplier, b, this._cs.runLerpSpeed * Time.deltaTime);
	}

	// Token: 0x060011E8 RID: 4584 RVA: 0x0004D580 File Offset: 0x0004B780
	private void HeadSway()
	{
		Vector3 direction = (this.playerHead.transform.position - this._previousHeadPosition) / Mathf.Max(Time.deltaTime, 0.0001f);
		this._previousHeadPosition = this.playerHead.transform.position;
		Vector3 vector = this.playerHead.transform.InverseTransformDirection(direction);
		float target = vector.z * this._cs.swayAmountXAxis;
		float target2 = -vector.x * this._cs.swayAmountZAxis;
		this._currentTiltX = Mathf.SmoothDampAngle(this._currentTiltX, target, ref this._tiltXVel, this._cs.swayDamping);
		this._currentTiltZ = Mathf.SmoothDampAngle(this._currentTiltZ, target2, ref this._tiltZVel, this._cs.swayDamping);
		this._finalHeadSway = Quaternion.Euler(this._currentTiltX, 0f, this._currentTiltZ);
	}

	// Token: 0x060011E9 RID: 4585 RVA: 0x0004D66C File Offset: 0x0004B86C
	private void HeadBob()
	{
		if (!this._cs.bobbingEnabled)
		{
			this._finalHeadBob = Vector3.zero;
			return;
		}
		Vector3 vector = Vector3.ProjectOnPlane(this.rigidBody.linearVelocity, Vector3.up);
		bool flag = vector.magnitude > 0.1f;
		if (this.playerController.isGrounded && flag)
		{
			this._hasReset = false;
			this._xScroll += Time.deltaTime * this._cs.xFrequency * vector.magnitude;
			this._yScroll += Time.deltaTime * this._cs.yFrequency * vector.magnitude;
			float num = this._cs.xCurve.Evaluate(this._xScroll);
			float num2 = this._cs.yCurve.Evaluate(this._yScroll);
			this._finalOffset.x = num * this._cs.xAmplitude * vector.magnitude / 1000f;
			this._finalOffset.y = num2 * this._cs.yAmplitude * vector.magnitude / 1000f;
			this._finalHeadBob = Vector3.Lerp(this._finalHeadBob, this._finalOffset, Time.deltaTime * this._cs.headBobLerpSpeed);
			return;
		}
		if (!this._hasReset)
		{
			this._hasReset = true;
			this._xScroll = 0f;
			this._yScroll = 0f;
			this._finalOffset = Vector3.zero;
		}
		this._finalHeadBob = Vector3.Lerp(this._finalHeadBob, Vector3.zero, Time.deltaTime * this._cs.headBobResetLerpSpeed);
	}

	// Token: 0x060011EB RID: 4587 RVA: 0x00002321 File Offset: 0x00000521
	public override bool Weaved()
	{
		return true;
	}

	// Token: 0x04000B92 RID: 2962
	private Camera _cam;

	// Token: 0x04000B93 RID: 2963
	private CameraSettings _cs;

	// Token: 0x04000B94 RID: 2964
	[SerializeField]
	private Rigidbody rigidBody;

	// Token: 0x04000B95 RID: 2965
	[SerializeField]
	private PlayerController playerController;

	// Token: 0x04000B96 RID: 2966
	[SerializeField]
	private PlayerHead playerHead;

	// Token: 0x04000B97 RID: 2967
	private float _baseFOV;

	// Token: 0x04000B98 RID: 2968
	private float _runFOVMultiplier = 1f;

	// Token: 0x04000B99 RID: 2969
	private Vector3 _previousHeadPosition;

	// Token: 0x04000B9A RID: 2970
	private float _currentTiltX;

	// Token: 0x04000B9B RID: 2971
	private float _currentTiltZ;

	// Token: 0x04000B9C RID: 2972
	private float _tiltXVel;

	// Token: 0x04000B9D RID: 2973
	private float _tiltZVel;

	// Token: 0x04000B9E RID: 2974
	private Quaternion _finalHeadSway;

	// Token: 0x04000B9F RID: 2975
	private float _xScroll;

	// Token: 0x04000BA0 RID: 2976
	private float _yScroll;

	// Token: 0x04000BA1 RID: 2977
	private Vector3 _finalOffset;

	// Token: 0x04000BA2 RID: 2978
	private bool _hasReset;

	// Token: 0x04000BA3 RID: 2979
	private Vector3 _finalHeadBob;
}
