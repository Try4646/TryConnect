using System;
using UnityEngine;

// Token: 0x020002AF RID: 687
public class BezierCurve
{
	// Token: 0x06001837 RID: 6199 RVA: 0x0006682B File Offset: 0x00064A2B
	public BezierCurve(Vector3 startPoint, Vector3 endPoint, Vector3 bendOffset)
	{
		this.StartPoint = startPoint;
		this.EndPoint = endPoint;
		this.BendOffset = bendOffset;
	}

	// Token: 0x06001838 RID: 6200 RVA: 0x00066848 File Offset: 0x00064A48
	public Vector3 GetPoint(float t)
	{
		Vector3 vector = Vector3.Lerp(this.StartPoint, this.EndPoint, 0.5f) + this.BendOffset;
		Vector3 a = Vector3.Lerp(this.StartPoint, vector, t);
		Vector3 b = Vector3.Lerp(vector, this.EndPoint, t);
		return Vector3.Lerp(a, b, t);
	}

	// Token: 0x06001839 RID: 6201 RVA: 0x0006689C File Offset: 0x00064A9C
	public Vector3 GetDirection(float t)
	{
		Vector3 vector = Vector3.Lerp(this.StartPoint, this.EndPoint, 0.5f) + this.BendOffset;
		Vector3 startPoint = this.StartPoint;
		Vector3 vector2 = vector;
		Vector3 endPoint = this.EndPoint;
		return (2f * (1f - t) * (vector2 - startPoint) + 2f * t * (endPoint - vector2)).normalized;
	}

	// Token: 0x04000F96 RID: 3990
	public Vector3 StartPoint;

	// Token: 0x04000F97 RID: 3991
	public Vector3 EndPoint;

	// Token: 0x04000F98 RID: 3992
	public Vector3 BendOffset;
}
