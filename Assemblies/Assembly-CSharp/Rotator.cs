using System;
using UnityEngine;

// Token: 0x020002F3 RID: 755
public class Rotator : MonoBehaviour
{
	// Token: 0x06001A27 RID: 6695 RVA: 0x0006E300 File Offset: 0x0006C500
	private void Start()
	{
		this._axisVector = ((this.axis == Rotator.Axis.X) ? Vector3.right : ((this.axis == Rotator.Axis.Y) ? Vector3.up : Vector3.forward));
	}

	// Token: 0x06001A28 RID: 6696 RVA: 0x0006E32C File Offset: 0x0006C52C
	private void Update()
	{
		base.transform.Rotate(this._axisVector * (this.speed * Time.deltaTime), Space.Self);
	}

	// Token: 0x040010BB RID: 4283
	public float speed = 50f;

	// Token: 0x040010BC RID: 4284
	public Rotator.Axis axis = Rotator.Axis.Y;

	// Token: 0x040010BD RID: 4285
	private Vector3 _axisVector;

	// Token: 0x020002F4 RID: 756
	public enum Axis
	{
		// Token: 0x040010BF RID: 4287
		X,
		// Token: 0x040010C0 RID: 4288
		Y,
		// Token: 0x040010C1 RID: 4289
		Z
	}
}
