using System;
using System.Collections.Generic;
using UnityEngine;

// Token: 0x02000315 RID: 789
[CreateAssetMenu(menuName = "Game Settings/Setting Item/Dropdown", fileName = "DropdownSetting")]
public class DropdownSettingItem : SettingItemBase
{
	// Token: 0x17000274 RID: 628
	// (get) Token: 0x06001AB4 RID: 6836 RVA: 0x000711A1 File Offset: 0x0006F3A1
	public string CurrentOption
	{
		get
		{
			if (this.options == null || this.index < 0 || this.index >= this.options.Count)
			{
				return string.Empty;
			}
			return this.options[this.index];
		}
	}

	// Token: 0x17000275 RID: 629
	// (get) Token: 0x06001AB5 RID: 6837 RVA: 0x0006A586 File Offset: 0x00068786
	public override SettingKind Kind
	{
		get
		{
			return SettingKind.Dropdown;
		}
	}

	// Token: 0x0400116D RID: 4461
	public List<string> options = new List<string>();

	// Token: 0x0400116E RID: 4462
	public int index;

	// Token: 0x0400116F RID: 4463
	public bool useDynamicOptions;

	// Token: 0x04001170 RID: 4464
	public ScriptableObject optionsProvider;

	// Token: 0x04001171 RID: 4465
	[Tooltip("If true, this setting will be applied on every scene load from saved settings")]
	public bool loadOnSceneStart;
}
