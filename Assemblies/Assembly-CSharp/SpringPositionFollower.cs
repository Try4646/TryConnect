using System;
using UnityEngine;

// Token: 0x020002F7 RID: 759
public class SpringPositionFollower : MonoBehaviour
{
	// Token: 0x06001A2E RID: 6702 RVA: 0x0006E593 File Offset: 0x0006C793
	private void Awake()
	{
		this._positionState = base.transform.position;
	}

	// Token: 0x06001A2F RID: 6703 RVA: 0x0006E5A6 File Offset: 0x0006C7A6
	private void OnDisable()
	{
		base.transform.localPosition = Vector3.zero;
		this._velocity = Vector3.zero;
	}

	// Token: 0x06001A30 RID: 6704 RVA: 0x0006E5C3 File Offset: 0x0006C7C3
	private void OnEnable()
	{
		this._positionState = this.target.position;
	}

	// Token: 0x06001A31 RID: 6705 RVA: 0x0006E5D6 File Offset: 0x0006C7D6
	private void LateUpdate()
	{
		this.SmoothMove();
	}

	// Token: 0x06001A32 RID: 6706 RVA: 0x0006E5E0 File Offset: 0x0006C7E0
	private void SmoothMove()
	{
		if (!this.target)
		{
			return;
		}
		Vector3 a = (this.target.position - this._positionState) * this.springStrength;
		this._velocity += a * Time.deltaTime;
		this._velocity *= Mathf.Exp(-this.damping * Time.deltaTime);
		this._velocity = Vector3.ClampMagnitude(this._velocity, this.maxSpeed);
		this._positionState += this._velocity * Time.deltaTime;
		base.transform.position = Vector3.Lerp(this.target.position, this._positionState, this.positionOffsetMultiplier);
	}

	// Token: 0x040010CE RID: 4302
	[Header("Settings")]
	[SerializeField]
	private float springStrength = 300f;

	// Token: 0x040010CF RID: 4303
	[SerializeField]
	private float damping = 12f;

	// Token: 0x040010D0 RID: 4304
	[SerializeField]
	private float maxSpeed = 20f;

	// Token: 0x040010D1 RID: 4305
	[SerializeField]
	private float positionOffsetMultiplier = 0.2f;

	// Token: 0x040010D2 RID: 4306
	[Header("References")]
	[SerializeField]
	private Transform target;

	// Token: 0x040010D3 RID: 4307
	private Vector3 _positionState;

	// Token: 0x040010D4 RID: 4308
	private Vector3 _velocity;
}
