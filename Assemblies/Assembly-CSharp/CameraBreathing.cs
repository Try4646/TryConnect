using System;
using UnityEngine;

// Token: 0x02000179 RID: 377
public class CameraBreathing : MonoBehaviour
{
	// Token: 0x06000E47 RID: 3655 RVA: 0x0003B1EF File Offset: 0x000393EF
	private void Awake()
	{
		this._baseLocalPos = base.transform.localPosition;
		this._baseLocalRot = base.transform.localRotation;
	}

	// Token: 0x06000E48 RID: 3656 RVA: 0x0003B214 File Offset: 0x00039414
	private void LateUpdate()
	{
		this._time += Time.deltaTime * this.frequency;
		Vector3 vector = new Vector3(this.x ? Mathf.Sin(this._time * 1.1f) : 0f, this.y ? Mathf.Sin(this._time * 1.3f + 1f) : 0f, this.z ? Mathf.Sin(this._time * 0.9f + 2f) : 0f) * this.amplitude;
		Vector3 euler = vector * this.rotationMultiplier;
		switch (this.transformTarget)
		{
		case TransformTarget.Position:
			base.transform.localPosition = this._baseLocalPos + vector;
			return;
		case TransformTarget.Rotation:
			base.transform.localRotation = this._baseLocalRot * Quaternion.Euler(euler);
			return;
		case TransformTarget.Both:
			base.transform.localPosition = this._baseLocalPos + vector;
			base.transform.localRotation = this._baseLocalRot * Quaternion.Euler(euler);
			return;
		default:
			return;
		}
	}

	// Token: 0x0400090D RID: 2317
	[Header("Settings")]
	public TransformTarget transformTarget;

	// Token: 0x0400090E RID: 2318
	public float frequency = 1f;

	// Token: 0x0400090F RID: 2319
	public float amplitude = 0.02f;

	// Token: 0x04000910 RID: 2320
	public float rotationMultiplier = 1f;

	// Token: 0x04000911 RID: 2321
	[Header("Axis")]
	public bool x = true;

	// Token: 0x04000912 RID: 2322
	public bool y = true;

	// Token: 0x04000913 RID: 2323
	public bool z = true;

	// Token: 0x04000914 RID: 2324
	private Vector3 _baseLocalPos;

	// Token: 0x04000915 RID: 2325
	private Quaternion _baseLocalRot;

	// Token: 0x04000916 RID: 2326
	private float _time;
}
