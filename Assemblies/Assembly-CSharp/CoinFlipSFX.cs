using System;
using UnityEngine;

// Token: 0x0200026D RID: 621
public class CoinFlipSFX : MonoBehaviour
{
	// Token: 0x06001607 RID: 5639 RVA: 0x0005EEDD File Offset: 0x0005D0DD
	private void Start()
	{
		if (this.loopComponent == null && this.localLoopComponent != null)
		{
			this.useLocalLoopComponent = true;
			return;
		}
		this.noLoopComponents = true;
	}

	// Token: 0x06001608 RID: 5640 RVA: 0x0005EF0C File Offset: 0x0005D10C
	private void Update()
	{
		if (this.coin.IsSleeping())
		{
			return;
		}
		if (this.noLoopComponents)
		{
			return;
		}
		if (!this.useLocalLoopComponent)
		{
			if (this.loopComponent.loopInstance.isValid())
			{
				float value = this.coin.angularVelocity.magnitude * 2f;
				this.loopComponent.loopInstance.setParameterByName("AngularVelocity", value, false);
				return;
			}
		}
		else if (this.localLoopComponent.loopInstance.isValid())
		{
			float value2 = this.coin.angularVelocity.magnitude * 2f;
			this.localLoopComponent.loopInstance.setParameterByName("AngularVelocity", value2, false);
		}
	}

	// Token: 0x04000E6D RID: 3693
	[SerializeField]
	private Rigidbody coin;

	// Token: 0x04000E6E RID: 3694
	[SerializeField]
	private SFXLoopComponent loopComponent;

	// Token: 0x04000E6F RID: 3695
	[SerializeField]
	private SFXLocalLoopComponent localLoopComponent;

	// Token: 0x04000E70 RID: 3696
	private bool useLocalLoopComponent;

	// Token: 0x04000E71 RID: 3697
	private bool noLoopComponents;
}
