using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

// Token: 0x02000232 RID: 562
public static class TooltipKeyParser
{
	// Token: 0x06001476 RID: 5238 RVA: 0x00057D68 File Offset: 0x00055F68
	public static List<TooltipElement> ParseTooltip(string tooltipText)
	{
		List<TooltipElement> list = new List<TooltipElement>();
		if (string.IsNullOrEmpty(tooltipText))
		{
			Debug.Log("TooltipKeyParser: Tooltip text is null or empty");
			return list;
		}
		int num = 0;
		foreach (object obj in Regex.Matches(tooltipText, TooltipKeyParser.KeyBracketPattern, RegexOptions.IgnoreCase))
		{
			Match match = (Match)obj;
			if (match.Index > num)
			{
				string text = tooltipText.Substring(num, match.Index - num);
				if (!string.IsNullOrEmpty(text))
				{
					list.Add(new TooltipElement
					{
						Type = TooltipElementType.Text,
						Content = text
					});
				}
			}
			string text2 = match.Groups[1].Value;
			text2 = TooltipKeyParser.ResolveDynamicKey(text2);
			list.Add(new TooltipElement
			{
				Type = TooltipElementType.Key,
				Content = text2
			});
			num = match.Index + match.Length;
		}
		if (num < tooltipText.Length)
		{
			string text3 = tooltipText.Substring(num);
			if (!string.IsNullOrEmpty(text3))
			{
				list.Add(new TooltipElement
				{
					Type = TooltipElementType.Text,
					Content = text3
				});
			}
		}
		if (list.Count == 0)
		{
			list.Add(new TooltipElement
			{
				Type = TooltipElementType.Text,
				Content = tooltipText
			});
		}
		return list;
	}

	// Token: 0x06001477 RID: 5239 RVA: 0x00057EDC File Offset: 0x000560DC
	public static bool HasKeys(string tooltipText)
	{
		return !string.IsNullOrEmpty(tooltipText) && Regex.IsMatch(tooltipText, TooltipKeyParser.KeyBracketPattern, RegexOptions.IgnoreCase);
	}

	// Token: 0x06001478 RID: 5240 RVA: 0x00057EF4 File Offset: 0x000560F4
	private static string ResolveDynamicKey(string keyText)
	{
		if (string.IsNullOrWhiteSpace(keyText))
		{
			return keyText;
		}
		if (!string.Equals(keyText.Trim(), "E", StringComparison.OrdinalIgnoreCase))
		{
			return keyText;
		}
		if (InputReader.Instance == null)
		{
			return "E";
		}
		string bindingDisplayName = InputReader.Instance.GetBindingDisplayName("Interact", 0);
		if (!string.IsNullOrWhiteSpace(bindingDisplayName))
		{
			return bindingDisplayName;
		}
		return "E";
	}

	// Token: 0x04000CEA RID: 3306
	private static readonly string KeyBracketPattern = "\\[([^\\]]+)\\]";

	// Token: 0x04000CEB RID: 3307
	private const string InteractFallbackKey = "E";
}
