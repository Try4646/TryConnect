using System;
using FMODUnity;
using UnityEngine;

// Token: 0x0200027E RID: 638
public class SFXLocalPlayer : MonoBehaviour
{
	// Token: 0x060016B6 RID: 5814 RVA: 0x00060F98 File Offset: 0x0005F198
	public void PlayOneShotWith3DPos()
	{
		if (this.eventReference.IsNull)
		{
			return;
		}
		SFXManager.SFXOneShot(this.eventReference, base.gameObject.transform.position);
	}

	// Token: 0x060016B7 RID: 5815 RVA: 0x00060FC3 File Offset: 0x0005F1C3
	public void PlayOneShotWithCustom3DPos(Vector3 pos)
	{
		if (this.eventReference.IsNull)
		{
			return;
		}
		SFXManager.SFXOneShot(this.eventReference, pos);
	}

	// Token: 0x060016B8 RID: 5816 RVA: 0x00060FDF File Offset: 0x0005F1DF
	public void PlayOneShotOverrideParams()
	{
		if (this.eventReference.IsNull)
		{
			return;
		}
		SFXManager.SFXOneShotWithParameters(this.eventReference, this.fmodParams, base.gameObject.transform.position, 1f);
	}

	// Token: 0x060016B9 RID: 5817 RVA: 0x00061015 File Offset: 0x0005F215
	public void PlayOneShotWithPitchMod(float pitch = 1f)
	{
		if (this.eventReference.IsNull)
		{
			return;
		}
		SFXManager.SFXOneShotWithParameters(this.eventReference, null, base.transform.position, pitch);
	}

	// Token: 0x04000ED0 RID: 3792
	[SerializeField]
	private EventReference eventReference;

	// Token: 0x04000ED1 RID: 3793
	public SFXParams[] fmodParams;
}
