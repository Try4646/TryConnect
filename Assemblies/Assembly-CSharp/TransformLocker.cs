using System;
using UnityEngine;

// Token: 0x020002FB RID: 763
[ExecuteInEditMode]
public class TransformLocker : MonoBehaviour
{
	// Token: 0x06001A3F RID: 6719 RVA: 0x0006EAA4 File Offset: 0x0006CCA4
	private void OnDrawGizmos()
	{
		if (this.LockPosition)
		{
			Vector3 localPosition = base.transform.localPosition;
			if (this.LockPositionX)
			{
				localPosition.x = this.LockedPositionX;
			}
			if (this.LockPositionY)
			{
				localPosition.y = this.LockedPositionY;
			}
			if (this.LockPositionZ)
			{
				localPosition.z = this.LockedPositionZ;
			}
			base.transform.localPosition = localPosition;
		}
	}

	// Token: 0x040010E7 RID: 4327
	public bool LockPosition = true;

	// Token: 0x040010E8 RID: 4328
	public bool LockPositionX;

	// Token: 0x040010E9 RID: 4329
	public float LockedPositionX;

	// Token: 0x040010EA RID: 4330
	public bool LockPositionY;

	// Token: 0x040010EB RID: 4331
	public float LockedPositionY;

	// Token: 0x040010EC RID: 4332
	public bool LockPositionZ;

	// Token: 0x040010ED RID: 4333
	public float LockedPositionZ;
}
