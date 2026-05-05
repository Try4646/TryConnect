using System;

namespace SettingsSystem
{
	// Token: 0x02000393 RID: 915
	public interface ISettingsApplier
	{
		// Token: 0x06001E05 RID: 7685
		void Apply(SettingItemBase entry);

		// Token: 0x06001E06 RID: 7686
		void ApplyAll(SettingsLayout layout);
	}
}
