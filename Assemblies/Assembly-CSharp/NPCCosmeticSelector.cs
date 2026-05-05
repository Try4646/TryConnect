using System;
using System.Collections.Generic;
using UnityEngine;

// Token: 0x020001C7 RID: 455
public class NPCCosmeticSelector : MonoBehaviour
{
	// Token: 0x06001071 RID: 4209 RVA: 0x00046AD0 File Offset: 0x00044CD0
	private void Awake()
	{
		if (this.applyOnAwake)
		{
			this.ApplySelectedPreset();
		}
	}

	// Token: 0x06001072 RID: 4210 RVA: 0x00046AE0 File Offset: 0x00044CE0
	private void OnValidate()
	{
		this.ApplySelectedPreset();
	}

	// Token: 0x06001073 RID: 4211 RVA: 0x00046AE8 File Offset: 0x00044CE8
	[ContextMenu("Apply Selected Preset")]
	public void ApplySelectedPreset()
	{
		if (this.presets == null || this.presets.Length == 0)
		{
			return;
		}
		this.selectedPresetIndex = Mathf.Clamp(this.selectedPresetIndex, 0, this.presets.Length - 1);
		NPCCosmeticSelector.CosmeticPreset cosmeticPreset = this.presets[this.selectedPresetIndex];
		if (cosmeticPreset == null || cosmeticPreset.enabledCosmetics == null)
		{
			return;
		}
		HashSet<GameObject> hashSet = new HashSet<GameObject>(cosmeticPreset.enabledCosmetics);
		for (int i = 0; i < this.presets.Length; i++)
		{
			NPCCosmeticSelector.CosmeticPreset cosmeticPreset2 = this.presets[i];
			if (cosmeticPreset2 != null && cosmeticPreset2.enabledCosmetics != null)
			{
				for (int j = 0; j < cosmeticPreset2.enabledCosmetics.Length; j++)
				{
					GameObject gameObject = cosmeticPreset2.enabledCosmetics[j];
					if (!(gameObject == null))
					{
						gameObject.SetActive(hashSet.Contains(gameObject));
					}
				}
			}
		}
	}

	// Token: 0x06001074 RID: 4212 RVA: 0x00046BAB File Offset: 0x00044DAB
	public void SetSelectedPresetIndex(int presetIndex)
	{
		this.selectedPresetIndex = presetIndex;
		this.ApplySelectedPreset();
	}

	// Token: 0x04000A9D RID: 2717
	[Header("Preset Selection")]
	[SerializeField]
	private NPCCosmeticSelector.CosmeticPreset[] presets;

	// Token: 0x04000A9E RID: 2718
	[SerializeField]
	private int selectedPresetIndex;

	// Token: 0x04000A9F RID: 2719
	[SerializeField]
	private bool applyOnAwake = true;

	// Token: 0x020001C8 RID: 456
	[Serializable]
	public class CosmeticPreset
	{
		// Token: 0x04000AA0 RID: 2720
		public string presetName;

		// Token: 0x04000AA1 RID: 2721
		public GameObject[] enabledCosmetics;
	}
}
