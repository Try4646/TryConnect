using System;
using System.Collections.Generic;
using UnityEngine;

// Token: 0x02000316 RID: 790
[CreateAssetMenu(menuName = "Game Settings/Dropdown Provider/Framerate", fileName = "FramerateProvider")]
public class FramerateProvider : ScriptableObject, IDropdownOptionsProvider
{
	// Token: 0x06001AB7 RID: 6839 RVA: 0x000711F4 File Offset: 0x0006F3F4
	public List<string> GetOptions()
	{
		List<string> list = new List<string>();
		foreach (int num in this.framerateOptions)
		{
			if (num == -1)
			{
				list.Add("Unlimited");
			}
			else if (num > 0)
			{
				list.Add(num.ToString());
			}
		}
		if (list.Count == 0)
		{
			list.AddRange(new string[]
			{
				"30",
				"60",
				"120",
				"144",
				"240",
				"Unlimited"
			});
		}
		return list;
	}

	// Token: 0x06001AB8 RID: 6840 RVA: 0x000712B0 File Offset: 0x0006F4B0
	public int GetDefaultIndex(List<string> options)
	{
		if (options == null || options.Count == 0)
		{
			return 0;
		}
		int targetFrameRate = Application.targetFrameRate;
		if (targetFrameRate == -1)
		{
			for (int i = 0; i < options.Count; i++)
			{
				string a = options[i].Trim().ToLowerInvariant();
				if (a == "unlimited" || a == "uncapped" || a == "off")
				{
					return i;
				}
			}
			return 0;
		}
		for (int j = 0; j < options.Count; j++)
		{
			string text = options[j];
			string a2 = text.Trim().ToLowerInvariant();
			int num;
			if (!(a2 == "unlimited") && !(a2 == "uncapped") && !(a2 == "off") && int.TryParse(text, out num) && num == targetFrameRate)
			{
				return j;
			}
		}
		int result = 0;
		int num2 = int.MaxValue;
		for (int k = 0; k < options.Count; k++)
		{
			string text2 = options[k];
			string a3 = text2.Trim().ToLowerInvariant();
			int num3;
			if (!(a3 == "unlimited") && !(a3 == "uncapped") && !(a3 == "off") && int.TryParse(text2, out num3) && num3 > 0)
			{
				int num4 = Mathf.Abs(num3 - targetFrameRate);
				if (num4 < num2)
				{
					num2 = num4;
					result = k;
				}
			}
		}
		return result;
	}

	// Token: 0x04001172 RID: 4466
	[Tooltip("Common framerate options to include. Set to -1 for unlimited.")]
	public List<int> framerateOptions = new List<int>
	{
		30,
		60,
		120,
		144,
		240,
		-1
	};
}
