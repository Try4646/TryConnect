using System;
using UnityEngine;

// Token: 0x02000290 RID: 656
public class CameraClipping : MonoBehaviour
{
	// Token: 0x0600175E RID: 5982 RVA: 0x00062E7C File Offset: 0x0006107C
	private void ClipCameraMatrix()
	{
		this._plane.normal = this.ConvertEulerAnglesToVector3(this.clippingPlane.rotation.eulerAngles, Vector3.down);
		Plane plane = new Plane(this._plane.normal, this.cam.transform.position);
		this._plane.distance = -plane.GetDistanceToPoint(this.clippingPlane.position + this.cam.transform.position);
		Vector4 vector = new Vector4(this._plane.normal.x, this._plane.normal.y, this._plane.normal.z, this._plane.distance);
		Vector4 clipPlane = Matrix4x4.Transpose(Matrix4x4.Inverse(this.cam.worldToCameraMatrix)) * vector;
		this.cam.projectionMatrix = this.cam.CalculateObliqueMatrix(clipPlane);
	}

	// Token: 0x0600175F RID: 5983 RVA: 0x00062F7C File Offset: 0x0006117C
	private Vector3 ConvertEulerAnglesToVector3(Vector3 euler, Vector3 upVector)
	{
		return Quaternion.Euler(euler) * upVector;
	}

	// Token: 0x06001760 RID: 5984 RVA: 0x00062F8A File Offset: 0x0006118A
	public void LateUpdate()
	{
		this.ClipCameraMatrix();
	}

	// Token: 0x04000F2A RID: 3882
	[SerializeField]
	private Transform clippingPlane;

	// Token: 0x04000F2B RID: 3883
	[SerializeField]
	private Camera cam;

	// Token: 0x04000F2C RID: 3884
	private Plane _plane;
}
