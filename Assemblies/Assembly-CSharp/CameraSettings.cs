using System;
using UnityEngine;

// Token: 0x0200030F RID: 783
[CreateAssetMenu(menuName = "Game Settings/Camera Settings", fileName = "CameraSettings")]
public class CameraSettings : ScriptableObject
{
	// Token: 0x17000270 RID: 624
	// (get) Token: 0x06001A9C RID: 6812 RVA: 0x00070DC3 File Offset: 0x0006EFC3
	public float cameraLerp
	{
		get
		{
			if (!this.cameraLerpToggle.value)
			{
				return 100f;
			}
			return this.cameraLerpSlider.value;
		}
	}

	// Token: 0x17000271 RID: 625
	// (get) Token: 0x06001A9D RID: 6813 RVA: 0x00070DE3 File Offset: 0x0006EFE3
	public float zoomSensitivity
	{
		get
		{
			return this.sensitivity.value * 0.6f;
		}
	}

	// Token: 0x17000272 RID: 626
	// (get) Token: 0x06001A9E RID: 6814 RVA: 0x00070DF6 File Offset: 0x0006EFF6
	public float zoomFOV
	{
		get
		{
			return this.baseFOV.value * 0.7f;
		}
	}

	// Token: 0x17000273 RID: 627
	// (get) Token: 0x06001A9F RID: 6815 RVA: 0x00070E09 File Offset: 0x0006F009
	public bool bobbingEnabled
	{
		get
		{
			return this.CameraHeadBobToggle.value;
		}
	}

	// Token: 0x14000020 RID: 32
	// (add) Token: 0x06001AA0 RID: 6816 RVA: 0x00070E18 File Offset: 0x0006F018
	// (remove) Token: 0x06001AA1 RID: 6817 RVA: 0x00070E4C File Offset: 0x0006F04C
	public static event Action<CameraSettings> SettingsChanged;

	// Token: 0x06001AA2 RID: 6818 RVA: 0x00070E7F File Offset: 0x0006F07F
	private void NotifyChanged()
	{
		Action<CameraSettings> settingsChanged = CameraSettings.SettingsChanged;
		if (settingsChanged == null)
		{
			return;
		}
		settingsChanged(this);
	}

	// Token: 0x04001144 RID: 4420
	[Header("Camera Settings")]
	public SliderSettingItem cameraLerpSlider;

	// Token: 0x04001145 RID: 4421
	public ToggleSettingItem cameraLerpToggle;

	// Token: 0x04001146 RID: 4422
	public SliderSettingItem sensitivity;

	// Token: 0x04001147 RID: 4423
	public SliderSettingItem controllerSensitivity;

	// Token: 0x04001148 RID: 4424
	[Header("FOV Settings")]
	public SliderSettingItem baseFOV;

	// Token: 0x04001149 RID: 4425
	public float zoomLerpSpeed;

	// Token: 0x0400114A RID: 4426
	public float runFOVMultiplier;

	// Token: 0x0400114B RID: 4427
	public float runFOVThreshold;

	// Token: 0x0400114C RID: 4428
	public float runLerpSpeed;

	// Token: 0x0400114D RID: 4429
	[Header("Head Sway Settings")]
	public float swayAmountZAxis;

	// Token: 0x0400114E RID: 4430
	public float swayAmountXAxis;

	// Token: 0x0400114F RID: 4431
	public float swayDamping;

	// Token: 0x04001150 RID: 4432
	[Header("HeadBob Settings")]
	public AnimationCurve xCurve;

	// Token: 0x04001151 RID: 4433
	public AnimationCurve yCurve;

	// Token: 0x04001152 RID: 4434
	public ToggleSettingItem CameraHeadBobToggle;

	// Token: 0x04001153 RID: 4435
	public float xAmplitude = 4f;

	// Token: 0x04001154 RID: 4436
	public float yAmplitude = 8f;

	// Token: 0x04001155 RID: 4437
	public float xFrequency;

	// Token: 0x04001156 RID: 4438
	public float yFrequency;

	// Token: 0x04001157 RID: 4439
	public float headBobLerpSpeed;

	// Token: 0x04001158 RID: 4440
	public float headBobResetLerpSpeed;
}
