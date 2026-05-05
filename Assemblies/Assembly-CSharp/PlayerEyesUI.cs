using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

// Token: 0x0200024B RID: 587
public class PlayerEyesUI : MonoBehaviour
{
	// Token: 0x0600151C RID: 5404 RVA: 0x0005AB4D File Offset: 0x00058D4D
	private void Awake()
	{
		if (this.blindnessVolume != null && this.blindnessVolume.profile != null)
		{
			this.blindnessVolume.profile.TryGet<Vignette>(out this._vignette);
		}
	}

	// Token: 0x0600151D RID: 5405 RVA: 0x0005AB87 File Offset: 0x00058D87
	public void ToggleEye(bool isRightEye, bool isEnabled)
	{
		if (!isRightEye)
		{
			this._leftEyeEnabled = isEnabled;
		}
		else
		{
			this._rightEyeEnabled = isEnabled;
		}
		this.UpdateVignette();
	}

	// Token: 0x0600151E RID: 5406 RVA: 0x0005ABA4 File Offset: 0x00058DA4
	private void UpdateVignette()
	{
		if (this._vignette == null)
		{
			return;
		}
		bool flag = !this._leftEyeEnabled;
		bool flag2 = !this._rightEyeEnabled;
		Vector2 x;
		if (flag && flag2)
		{
			this.blindnessVolume.weight = 1f;
			x = new Vector2(0.5f, 2f);
		}
		else if (flag)
		{
			this.blindnessVolume.weight = 1f;
			x = new Vector2(0.75f, 0.5f);
		}
		else
		{
			if (!flag2)
			{
				this.blindnessVolume.weight = 0f;
				return;
			}
			this.blindnessVolume.weight = 1f;
			x = new Vector2(0.25f, 0.5f);
		}
		this._vignette.active = true;
		this._vignette.center.Override(x);
	}

	// Token: 0x04000D7C RID: 3452
	[SerializeField]
	private Volume blindnessVolume;

	// Token: 0x04000D7D RID: 3453
	private Vignette _vignette;

	// Token: 0x04000D7E RID: 3454
	private bool _leftEyeEnabled = true;

	// Token: 0x04000D7F RID: 3455
	private bool _rightEyeEnabled = true;
}
