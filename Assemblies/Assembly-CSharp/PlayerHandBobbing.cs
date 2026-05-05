using System;
using Mirror;
using UnityEngine;

// Token: 0x020001FE RID: 510
public class PlayerHandBobbing : NetworkBehaviour
{
	// Token: 0x060012A1 RID: 4769 RVA: 0x0005087D File Offset: 0x0004EA7D
	private void Awake()
	{
		this._cs = Resources.Load<CameraSettings>("CameraSettings");
	}

	// Token: 0x060012A2 RID: 4770 RVA: 0x0005088F File Offset: 0x0004EA8F
	public override void OnStartClient()
	{
		base.OnStartClient();
		if (base.isLocalPlayer)
		{
			base.enabled = false;
			return;
		}
	}

	// Token: 0x060012A3 RID: 4771 RVA: 0x000508A7 File Offset: 0x0004EAA7
	private void LateUpdate()
	{
		this.HeadBob();
	}

	// Token: 0x060012A4 RID: 4772 RVA: 0x000508B0 File Offset: 0x0004EAB0
	private void HeadBob()
	{
		if (!this.playerController.hasBody)
		{
			this._finalHeadBob = Vector3.zero;
			base.transform.localPosition = Vector3.zero;
			return;
		}
		Vector3 vector = Vector3.ProjectOnPlane(this.playerController.serverVelocity, Vector3.up);
		bool flag = vector.magnitude > 0.1f;
		if (this.playerController.isGrounded && flag)
		{
			this._hasReset = false;
			this._xScroll += Time.deltaTime * this._cs.xFrequency * vector.magnitude;
			this._yScroll += Time.deltaTime * this._cs.yFrequency * vector.magnitude;
			float num = this._cs.xCurve.Evaluate(this._xScroll);
			float num2 = this._cs.yCurve.Evaluate(this._yScroll);
			this._finalOffset.x = num * this._cs.xAmplitude * this.bobbingXMultiplier * vector.magnitude / 1000f;
			this._finalOffset.y = num2 * this._cs.yAmplitude * this.bobbingYMultiplier * vector.magnitude / 1000f;
			this._finalHeadBob = Vector3.Lerp(this._finalHeadBob, this._finalOffset, Time.deltaTime * this._cs.headBobLerpSpeed);
		}
		else
		{
			if (!this._hasReset)
			{
				this._hasReset = true;
				this._xScroll = 0f;
				this._yScroll = 0f;
				this._finalOffset = Vector3.zero;
			}
			this._finalHeadBob = Vector3.Lerp(this._finalHeadBob, Vector3.zero, Time.deltaTime * this._cs.headBobResetLerpSpeed);
		}
		base.transform.localPosition = this._finalHeadBob;
	}

	// Token: 0x060012A6 RID: 4774 RVA: 0x00002321 File Offset: 0x00000521
	public override bool Weaved()
	{
		return true;
	}

	// Token: 0x04000BE0 RID: 3040
	[SerializeField]
	private PlayerController playerController;

	// Token: 0x04000BE1 RID: 3041
	[SerializeField]
	private float bobbingXMultiplier;

	// Token: 0x04000BE2 RID: 3042
	[SerializeField]
	private float bobbingYMultiplier;

	// Token: 0x04000BE3 RID: 3043
	private CameraSettings _cs;

	// Token: 0x04000BE4 RID: 3044
	private float _xScroll;

	// Token: 0x04000BE5 RID: 3045
	private float _yScroll;

	// Token: 0x04000BE6 RID: 3046
	private Vector3 _finalOffset;

	// Token: 0x04000BE7 RID: 3047
	private bool _hasReset;

	// Token: 0x04000BE8 RID: 3048
	private Vector3 _finalHeadBob;
}
