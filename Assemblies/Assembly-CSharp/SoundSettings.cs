using System;
using UnityEngine;

// Token: 0x02000337 RID: 823
[CreateAssetMenu(menuName = "Game Settings/Sound Settings", fileName = "SoundSettings")]
public class SoundSettings : ScriptableObject
{
	// Token: 0x14000028 RID: 40
	// (add) Token: 0x06001B4A RID: 6986 RVA: 0x00074640 File Offset: 0x00072840
	// (remove) Token: 0x06001B4B RID: 6987 RVA: 0x00074674 File Offset: 0x00072874
	public static event Action<SoundSettings> SettingsChanged;

	// Token: 0x06001B4C RID: 6988 RVA: 0x000746A7 File Offset: 0x000728A7
	private void NotifyChanged()
	{
		Action<SoundSettings> settingsChanged = SoundSettings.SettingsChanged;
		if (settingsChanged == null)
		{
			return;
		}
		settingsChanged(this);
	}

	// Token: 0x06001B4D RID: 6989 RVA: 0x000746B9 File Offset: 0x000728B9
	public void TriggerSettingsChanged()
	{
		this.NotifyChanged();
	}

	// Token: 0x04001212 RID: 4626
	public SliderSettingItem masterVol;

	// Token: 0x04001213 RID: 4627
	public SliderSettingItem musicVol;

	// Token: 0x04001214 RID: 4628
	public SliderSettingItem sFXVol;

	// Token: 0x04001215 RID: 4629
	public SliderSettingItem proxChatVol;
}
