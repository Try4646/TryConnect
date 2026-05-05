using System;
using System.Collections.Generic;
using UnityEngine;

// Token: 0x02000325 RID: 805
[CreateAssetMenu(menuName = "Game Settings/Dropdown Provider/Resolutions By Aspect", fileName = "ResolutionByAspectProvider")]
public class ResolutionByAspectProvider : ScriptableObject, IDropdownOptionsProvider
{
	// Token: 0x06001AFC RID: 6908 RVA: 0x00072698 File Offset: 0x00070898
	public List<string> GetOptions()
	{
		List<string> list = new List<string>();
		Resolution[] resolutions = Screen.resolutions;
		if (resolutions == null || resolutions.Length == 0)
		{
			list.Add(string.Format("{0}x{1}", Screen.currentResolution.width, Screen.currentResolution.height));
			return list;
		}
		float num = (Screen.width > 0 && Screen.height > 0) ? ((float)Screen.width / (float)Screen.height) : ((float)Screen.currentResolution.width / (float)Screen.currentResolution.height);
		HashSet<string> hashSet = new HashSet<string>();
		List<Resolution> list2 = new List<Resolution>();
		foreach (Resolution item in resolutions)
		{
			if (Mathf.Abs((float)item.width / (float)item.height - num) <= this.aspectTolerance)
			{
				string item2 = string.Format("{0}x{1}", item.width, item.height);
				if (hashSet.Add(item2))
				{
					list2.Add(item);
				}
			}
		}
		if (list2.Count == 0)
		{
			list.Add(string.Format("{0}x{1}", Screen.currentResolution.width, Screen.currentResolution.height));
			return list;
		}
		list2.Sort(delegate(Resolution a, Resolution b)
		{
			int value = a.width * a.height;
			return (b.width * b.height).CompareTo(value);
		});
		for (int j = 0; j < list2.Count; j++)
		{
			list.Add(string.Format("{0}x{1}", list2[j].width, list2[j].height));
		}
		return list;
	}

	// Token: 0x06001AFD RID: 6909 RVA: 0x0007286C File Offset: 0x00070A6C
	public int GetDefaultIndex(List<string> options)
	{
		if (options == null || options.Count == 0)
		{
			return 0;
		}
		string b = string.Format("{0}x{1}", Screen.currentResolution.width, Screen.currentResolution.height);
		for (int i = 0; i < options.Count; i++)
		{
			if (options[i] == b)
			{
				return i;
			}
		}
		return 0;
	}

	// Token: 0x040011D4 RID: 4564
	[Range(0f, 0.1f)]
	public float aspectTolerance = 0.01f;
}
