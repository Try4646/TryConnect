using System;
using UnityEngine;

// Token: 0x02000336 RID: 822
[CreateAssetMenu(menuName = "Game Settings/Setting Item/Slider", fileName = "SliderSetting")]
public class SliderSettingItem : SettingItemBase
{
	// Token: 0x1700027F RID: 639
	// (get) Token: 0x06001B48 RID: 6984 RVA: 0x00002321 File Offset: 0x00000521
	public override SettingKind Kind
	{
		get
		{
			return SettingKind.Slider;
		}
	}

	// Token: 0x0400120C RID: 4620
	public float min;

	// Token: 0x0400120D RID: 4621
	public float max = 1f;

	// Token: 0x0400120E RID: 4622
	public bool wholeNumbers;

	// Token: 0x0400120F RID: 4623
	public float value;

	// Token: 0x04001210 RID: 4624
	public float defaultValue;

	// Token: 0x04001211 RID: 4625
	[Tooltip("If true, this setting will be applied on every scene load from saved settings")]
	public bool loadOnSceneStart;
}
