using System;
using System.Collections.Generic;
using UnityEngine;

// Token: 0x020002F5 RID: 757
[ExecuteInEditMode]
public class SpiralPositioner : MonoBehaviour
{
	// Token: 0x06001A2A RID: 6698 RVA: 0x0006E36B File Offset: 0x0006C56B
	private void OnValidate()
	{
		if (!this.enableOnEditor)
		{
			return;
		}
		this.PositionChildren();
	}

	// Token: 0x06001A2B RID: 6699 RVA: 0x0006E37C File Offset: 0x0006C57C
	private void Update()
	{
		if (Application.isPlaying)
		{
			if (this.enableOnPlaymode)
			{
				this.PositionChildren();
				return;
			}
		}
		else if (this.enableOnEditor)
		{
			this.PositionChildren();
		}
	}

	// Token: 0x06001A2C RID: 6700 RVA: 0x0006E3A4 File Offset: 0x0006C5A4
	private void PositionChildren()
	{
		List<Transform> list = new List<Transform>();
		foreach (object obj in base.transform)
		{
			Transform transform = (Transform)obj;
			if (transform.gameObject.activeSelf)
			{
				list.Add(transform);
			}
		}
		int count = list.Count;
		if (count == 0)
		{
			return;
		}
		for (int i = 0; i < count; i++)
		{
			Transform transform2 = list[i];
			float f = 0.017453292f * ((float)i * 360f / (float)count + this.startOffset);
			Vector3 zero = Vector3.zero;
			switch (this.axis)
			{
			case SpiralPositioner.Axis.X:
				zero = new Vector3(this.heightStep, Mathf.Cos(f) * this.radius, Mathf.Sin(f) * this.radius);
				break;
			case SpiralPositioner.Axis.Y:
				zero = new Vector3(Mathf.Cos(f) * this.radius, this.heightStep, Mathf.Sin(f) * this.radius);
				break;
			case SpiralPositioner.Axis.Z:
				zero = new Vector3(Mathf.Cos(f) * this.radius, Mathf.Sin(f) * this.radius, this.heightStep);
				break;
			}
			transform2.localPosition = zero;
			if (this.setRotation)
			{
				Vector3 normalized = transform2.localPosition.normalized;
				if (normalized != Vector3.zero)
				{
					transform2.localRotation = Quaternion.LookRotation(Vector3.forward, normalized) * Quaternion.Euler(this.rotationOffset, 0f, 0f);
				}
			}
		}
	}

	// Token: 0x040010C2 RID: 4290
	public bool enableOnEditor = true;

	// Token: 0x040010C3 RID: 4291
	public bool enableOnPlaymode;

	// Token: 0x040010C4 RID: 4292
	[Header("Spiral Settings")]
	public SpiralPositioner.Axis axis = SpiralPositioner.Axis.Y;

	// Token: 0x040010C5 RID: 4293
	public float radius = 2f;

	// Token: 0x040010C6 RID: 4294
	public float heightStep = 0.5f;

	// Token: 0x040010C7 RID: 4295
	public float startOffset = 90f;

	// Token: 0x040010C8 RID: 4296
	public float rotationOffset;

	// Token: 0x040010C9 RID: 4297
	[Header("Rotation Settings")]
	public bool setRotation;

	// Token: 0x020002F6 RID: 758
	public enum Axis
	{
		// Token: 0x040010CB RID: 4299
		X,
		// Token: 0x040010CC RID: 4300
		Y,
		// Token: 0x040010CD RID: 4301
		Z
	}
}
