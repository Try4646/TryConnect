using System;
using System.Globalization;

// Token: 0x02000187 RID: 391
public static class MoneyFormatter
{
	// Token: 0x06000E9F RID: 3743 RVA: 0x0003C9C0 File Offset: 0x0003ABC0
	public static string Format(long amount)
	{
		if (amount == 0L)
		{
			return "0";
		}
		bool flag = amount < 0L;
		long num = Math.Abs(amount);
		if (num >= 1000L)
		{
			double num2;
			string str;
			if (num < 1000000L)
			{
				num2 = (double)num / 1000.0;
				str = "K";
			}
			else if (num < 1000000000L)
			{
				num2 = (double)num / 1000000.0;
				str = "M";
			}
			else if (num < 1000000000000L)
			{
				num2 = (double)num / 1000000000.0;
				str = "B";
			}
			else
			{
				num2 = (double)num / 1000000000000.0;
				str = "T";
			}
			string str2 = num2.ToString("F2", CultureInfo.InvariantCulture).TrimEnd('0').TrimEnd('.');
			return (flag ? "-" : "") + str2 + str;
		}
		string text = num.ToString("N0", MoneyFormatter.DotSeparatorFormat);
		if (!flag)
		{
			return text;
		}
		return "-" + text;
	}

	// Token: 0x06000EA0 RID: 3744 RVA: 0x0003CABA File Offset: 0x0003ACBA
	public static string FormatWithDollar(long amount)
	{
		if (amount < 0L)
		{
			return "-$" + MoneyFormatter.Format(Math.Abs(amount));
		}
		return "$" + MoneyFormatter.Format(amount);
	}

	// Token: 0x06000EA1 RID: 3745 RVA: 0x0003CAE8 File Offset: 0x0003ACE8
	public static string GetSuffix(long amount)
	{
		long num = Math.Abs(amount);
		if (num < 1000L)
		{
			return "";
		}
		string result;
		if (num < 1000000000L)
		{
			if (num >= 1000000L)
			{
				result = "M";
			}
			else
			{
				result = "K";
			}
		}
		else if (num >= 1000000000000L)
		{
			result = "T";
		}
		else
		{
			result = "B";
		}
		return result;
	}

	// Token: 0x06000EA2 RID: 3746 RVA: 0x0003CB4C File Offset: 0x0003AD4C
	public static float FormatFloat(long amount)
	{
		if (amount == 0L)
		{
			return 0f;
		}
		bool flag = amount < 0L;
		long num = Math.Abs(amount);
		if (num < 1000L)
		{
			return (float)amount;
		}
		double num2;
		if (num < 1000000000L)
		{
			if (num >= 1000000L)
			{
				num2 = (double)num / 1000000.0;
			}
			else
			{
				num2 = (double)num / 1000.0;
			}
		}
		else if (num >= 1000000000000L)
		{
			num2 = (double)num / 1000000000000.0;
		}
		else
		{
			num2 = (double)num / 1000000000.0;
		}
		double num3 = num2;
		double num4 = Math.Pow(10.0, Math.Floor(Math.Log10(num3)) - 2.0);
		double num5 = Math.Floor(num3 / num4) * num4;
		return (float)(flag ? (-(float)num5) : num5);
	}

	// Token: 0x04000968 RID: 2408
	private static readonly NumberFormatInfo DotSeparatorFormat = new NumberFormatInfo
	{
		NumberGroupSeparator = ".",
		NumberDecimalSeparator = "."
	};
}
