using System;
using System.Collections.Generic;
using Dissonance;
using UnityEngine;

// Token: 0x0200031F RID: 799
[CreateAssetMenu(menuName = "Game Settings/Dropdown Provider/Microphone Devices", fileName = "MicrophoneDevicesProvider")]
public class MicrophoneDevicesProvider : ScriptableObject, IDropdownOptionsProvider
{
	// Token: 0x06001AD8 RID: 6872 RVA: 0x00071BFC File Offset: 0x0006FDFC
	public List<string> GetOptions()
	{
		List<string> list = new List<string>();
		List<string> list2 = new List<string>();
		DissonanceComms dissonanceComms = Object.FindFirstObjectByType<DissonanceComms>();
		if (dissonanceComms != null)
		{
			dissonanceComms.GetMicrophoneDevices(list2);
		}
		else
		{
			string[] devices = UnityEngine.Microphone.devices;
			if (devices != null && devices.Length != 0)
			{
				list2.AddRange(devices);
			}
		}
		if (this.includeDefaultOption)
		{
			list.Add(this.defaultLabel);
		}
		for (int i = 0; i < list2.Count; i++)
		{
			string text = list2[i];
			if (!string.IsNullOrWhiteSpace(text) && !string.Equals(text, this.defaultLabel, StringComparison.OrdinalIgnoreCase) && !list.Contains(text))
			{
				list.Add(text);
			}
		}
		return list;
	}

	// Token: 0x06001AD9 RID: 6873 RVA: 0x00071CA0 File Offset: 0x0006FEA0
	public int GetDefaultIndex(List<string> options)
	{
		if (options == null || options.Count == 0)
		{
			return 0;
		}
		if (this.includeDefaultOption)
		{
			int num = options.FindIndex((string option) => string.Equals(option, this.defaultLabel, StringComparison.OrdinalIgnoreCase));
			if (num >= 0)
			{
				return num;
			}
		}
		return 0;
	}

	// Token: 0x040011A8 RID: 4520
	public bool includeDefaultOption = true;

	// Token: 0x040011A9 RID: 4521
	public string defaultLabel = "System Default";
}
