using System;
using UnityEngine;

// Token: 0x020002FC RID: 764
public class TransformSnapper : MonoBehaviour
{
	// Token: 0x06001A41 RID: 6721 RVA: 0x0006EB20 File Offset: 0x0006CD20
	public void ForceSnap()
	{
		Vector3 position = base.transform.position;
		Vector3 eulerAngles = base.transform.eulerAngles;
		Vector3 lossyScale = base.transform.lossyScale;
		if (this.positionSnap > 0f)
		{
			base.transform.position = this.SnapVector3(position, this.positionSnap);
		}
		if (this.rotationSnap > 0f)
		{
			base.transform.eulerAngles = this.SnapVector3(eulerAngles, this.rotationSnap);
		}
		if (this.scaleSnap > 0f)
		{
			Vector3 vector = this.SnapVector3(lossyScale, this.scaleSnap);
			if (base.transform.parent != null)
			{
				Vector3 lossyScale2 = base.transform.parent.lossyScale;
				Vector3 localScale = new Vector3((lossyScale2.x != 0f) ? (vector.x / lossyScale2.x) : vector.x, (lossyScale2.y != 0f) ? (vector.y / lossyScale2.y) : vector.y, (lossyScale2.z != 0f) ? (vector.z / lossyScale2.z) : vector.z);
				base.transform.localScale = localScale;
				return;
			}
			base.transform.localScale = vector;
		}
	}

	// Token: 0x06001A42 RID: 6722 RVA: 0x0006EC6E File Offset: 0x0006CE6E
	private Vector3 SnapVector3(Vector3 value, float snap)
	{
		return new Vector3(Mathf.Round(value.x / snap) * snap, Mathf.Round(value.y / snap) * snap, Mathf.Round(value.z / snap) * snap);
	}

	// Token: 0x040010EE RID: 4334
	[Header("Snap Settings")]
	[Tooltip("World position snap step (ex: 0.05)")]
	public float positionSnap = 0.05f;

	// Token: 0x040010EF RID: 4335
	[Tooltip("World rotation snap step (ex: 15)")]
	public float rotationSnap = 15f;

	// Token: 0x040010F0 RID: 4336
	[Tooltip("World scale snap step (ex: 0.1)")]
	public float scaleSnap = 0.1f;
}
