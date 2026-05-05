using System;
using UnityEngine;

// Token: 0x020002FA RID: 762
public class TransformLerper : MonoBehaviour
{
	// Token: 0x06001A3C RID: 6716 RVA: 0x0006E9A5 File Offset: 0x0006CBA5
	private void Awake()
	{
		this._worldPosition = base.transform.position;
	}

	// Token: 0x06001A3D RID: 6717 RVA: 0x0006E9B8 File Offset: 0x0006CBB8
	private void LateUpdate()
	{
		if (!this.target)
		{
			return;
		}
		Vector3 position = this.target.position;
		Vector3 vector = position - this._worldPosition;
		float magnitude = vector.magnitude;
		if (magnitude <= this.stopDistance)
		{
			this._worldPosition = position;
			base.transform.position = this._worldPosition;
			return;
		}
		float time = Mathf.Clamp01(magnitude / this.maxDistance);
		float num = this.speedByDistance.Evaluate(time) * this.maxSpeed * Time.deltaTime;
		num = Mathf.Min(num, magnitude);
		this._worldPosition += vector.normalized * num;
		base.transform.position = this._worldPosition;
	}

	// Token: 0x040010E1 RID: 4321
	[Header("References")]
	[SerializeField]
	private Transform target;

	// Token: 0x040010E2 RID: 4322
	[Header("Speed Settings")]
	[SerializeField]
	private float maxSpeed = 1f;

	// Token: 0x040010E3 RID: 4323
	[SerializeField]
	private float maxDistance = 1f;

	// Token: 0x040010E4 RID: 4324
	[SerializeField]
	private AnimationCurve speedByDistance;

	// Token: 0x040010E5 RID: 4325
	[Header("Stopping")]
	[SerializeField]
	private float stopDistance = 0.01f;

	// Token: 0x040010E6 RID: 4326
	private Vector3 _worldPosition;
}
