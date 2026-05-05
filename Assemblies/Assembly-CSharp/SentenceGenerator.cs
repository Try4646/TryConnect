using System;
using System.Collections.Generic;
using UnityEngine;

// Token: 0x020002E6 RID: 742
public static class SentenceGenerator
{
	// Token: 0x060019C8 RID: 6600 RVA: 0x0006BEDC File Offset: 0x0006A0DC
	public static string BuildLine(WordBankSO bank, SentencePattern pattern, int maxSlots = 3)
	{
		List<string> list = new List<string>();
		int num = 0;
		for (int i = 0; i < pattern.tokens.Count; i++)
		{
			PatternToken patternToken = pattern.tokens[i];
			if (!patternToken.isLiteral)
			{
				if (num >= maxSlots)
				{
					break;
				}
				List<string> pool = bank.GetPool(patternToken.slot);
				if (pool.Count != 0)
				{
					string item = pool[Random.Range(0, pool.Count)];
					list.Add(item);
					num++;
				}
			}
			else if (num < maxSlots)
			{
				bool flag = false;
				for (int j = i + 1; j < pattern.tokens.Count; j++)
				{
					PatternToken patternToken2 = pattern.tokens[j];
					if (!patternToken2.isLiteral && bank.GetPool(patternToken2.slot).Count > 0)
					{
						flag = true;
						break;
					}
				}
				if (flag)
				{
					list.Add(patternToken.literal);
				}
			}
		}
		string text = string.Join(" ", list).Replace(" ,", ",").Replace(" .", ".").Replace("  ", " ");
		if (string.IsNullOrWhiteSpace(text))
		{
			text = "we messed up.";
		}
		return text + ".";
	}
}
