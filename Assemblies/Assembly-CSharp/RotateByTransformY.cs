using System;
using UnityEngine;

// Token: 0x020002F2 RID: 754
public class RotateByTransformY : MonoBehaviour
{
	// Token: 0x06001A25 RID: 6693 RVA: 0x0006E288 File Offset: 0x0006C488
	private void Update()
	{
		Quaternion b = Quaternion.Euler(base.transform.eulerAngles.x, this.target.eulerAngles.y, base.transform.eulerAngles.z);
		base.transform.rotation = Quaternion.Slerp(base.transform.rotation, b, this.lerp);
	}

	// Token: 0x040010B9 RID: 4281
	[SerializeField]
	private Transform target;

	// Token: 0x040010BA RID: 4282
	[SerializeField]
	[Range(0f, 1f)]
	private float lerp = 0.1f;
}
