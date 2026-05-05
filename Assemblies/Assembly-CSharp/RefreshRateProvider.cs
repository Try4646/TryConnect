using System;
using System.Collections.Generic;
using UnityEngine;

// Token: 0x02000323 RID: 803
[CreateAssetMenu(menuName = "Game Settings/Dropdown Provider/Refresh Rate", fileName = "RefreshRateProvider")]
public class RefreshRateProvider : ScriptableObject, IDropdownOptionsProvider
{
	// Token: 0x06001AF5 RID: 6901 RVA: 0x0007221C File Offset: 0x0007041C
	public List<string> GetOptions()
	{
		List<string> list = new List<string>();
		int maxAvailableRefreshRate = this.GetMaxAvailableRefreshRate();
		List<float> availableRefreshRates = this.GetAvailableRefreshRates();
		List<int> list3;
		if (this.commonRefreshRates == null || this.commonRefreshRates.Count <= 0)
		{
			List<int> list2 = new List<int>();
			list2.Add(60);
			list2.Add(75);
			list2.Add(120);
			list2.Add(144);
			list2.Add(165);
			list2.Add(240);
			list3 = list2;
			list2.Add(360);
		}
		else
		{
			list3 = this.commonRefreshRates;
		}
		List<int> list4 = new List<int>();
		foreach (int num in list3)
		{
			if (num <= maxAvailableRefreshRate)
			{
				bool flag = false;
				using (List<float>.Enumerator enumerator2 = availableRefreshRates.GetEnumerator())
				{
					while (enumerator2.MoveNext())
					{
						if (Mathf.Abs(enumerator2.Current - (float)num) <= 2f)
						{
							flag = true;
							break;
						}
					}
				}
				if (flag)
				{
					list4.Add(num);
				}
			}
		}
		if (list4.Count == 0)
		{
			foreach (float num2 in availableRefreshRates)
			{
				if (num2 <= (float)maxAvailableRefreshRate)
				{
					list4.Add(Mathf.RoundToInt(num2));
				}
			}
		}
		list4.Sort();
		for (int i = 0; i < list4.Count; i++)
		{
			list.Add(string.Format("{0} Hz", list4[i]));
		}
		if (list.Count == 0)
		{
			float f = Screen.currentResolution.refreshRateRatio.numerator / Screen.currentResolution.refreshRateRatio.denominator;
			list.Add(string.Format("{0} Hz", Mathf.RoundToInt(f)));
		}
		return list;
	}

	// Token: 0x06001AF6 RID: 6902 RVA: 0x00072424 File Offset: 0x00070624
	private int GetMaxAvailableRefreshRate()
	{
		Resolution[] resolutions = Screen.resolutions;
		if (resolutions == null || resolutions.Length == 0)
		{
			return Mathf.RoundToInt(Screen.currentResolution.refreshRateRatio.numerator / Screen.currentResolution.refreshRateRatio.denominator);
		}
		int num = 0;
		foreach (Resolution resolution in resolutions)
		{
			int num2 = Mathf.RoundToInt(resolution.refreshRateRatio.numerator / resolution.refreshRateRatio.denominator);
			if (num2 > num)
			{
				num = num2;
			}
		}
		if (num <= 0)
		{
			return 60;
		}
		return num;
	}

	// Token: 0x06001AF7 RID: 6903 RVA: 0x000724BC File Offset: 0x000706BC
	private List<float> GetAvailableRefreshRates()
	{
		List<float> list = new List<float>();
		Resolution[] resolutions = Screen.resolutions;
		if (resolutions == null || resolutions.Length == 0)
		{
			float item = Screen.currentResolution.refreshRateRatio.numerator / Screen.currentResolution.refreshRateRatio.denominator;
			list.Add(item);
			return list;
		}
		HashSet<int> hashSet = new HashSet<int>();
		foreach (Resolution resolution in resolutions)
		{
			float f = resolution.refreshRateRatio.numerator / resolution.refreshRateRatio.denominator;
			hashSet.Add(Mathf.RoundToInt(f));
		}
		foreach (int num in hashSet)
		{
			list.Add((float)num);
		}
		return list;
	}

	// Token: 0x06001AF8 RID: 6904 RVA: 0x000725A8 File Offset: 0x000707A8
	public int GetDefaultIndex(List<string> options)
	{
		if (options == null || options.Count == 0)
		{
			return 0;
		}
		RefreshRate refreshRateRatio = Screen.currentResolution.refreshRateRatio;
		int num = Mathf.RoundToInt(refreshRateRatio.numerator / refreshRateRatio.denominator);
		for (int i = 0; i < options.Count; i++)
		{
			int num2;
			if (int.TryParse(options[i].Trim().ToLowerInvariant().Replace("hz", "").Trim(), out num2) && num2 == num)
			{
				return i;
			}
		}
		return 0;
	}

	// Token: 0x040011D3 RID: 4563
	[Tooltip("Common refresh rate values to check for. These will be filtered to only show available rates up to the user's maximum.")]
	public List<int> commonRefreshRates = new List<int>
	{
		60,
		75,
		120,
		144,
		165,
		240,
		360
	};
}
