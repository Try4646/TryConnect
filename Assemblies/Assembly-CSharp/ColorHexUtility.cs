using System;
using UnityEngine;

// Token: 0x020000AA RID: 170
public static class ColorHexUtility
{
	// Token: 0x0600069E RID: 1694 RVA: 0x0001C3A8 File Offset: 0x0001A5A8
	public static string ColorToHex(Color color)
	{
		int num = Mathf.RoundToInt(color.r * 255f);
		int num2 = Mathf.RoundToInt(color.g * 255f);
		int num3 = Mathf.RoundToInt(color.b * 255f);
		int num4 = Mathf.RoundToInt(color.a * 255f);
		return string.Format("{0:X2}{1:X2}{2:X2}{3:X2}", new object[]
		{
			num,
			num2,
			num3,
			num4
		});
	}

	// Token: 0x0600069F RID: 1695 RVA: 0x0001C434 File Offset: 0x0001A634
	public static Color HexToColor(string hex)
	{
		if (string.IsNullOrEmpty(hex))
		{
			return Color.white;
		}
		hex = hex.TrimStart('#');
		if (hex.Length != 8)
		{
			return Color.white;
		}
		Color result;
		try
		{
			float num = (float)Convert.ToInt32(hex.Substring(0, 2), 16);
			int num2 = Convert.ToInt32(hex.Substring(2, 2), 16);
			int num3 = Convert.ToInt32(hex.Substring(4, 2), 16);
			int num4 = Convert.ToInt32(hex.Substring(6, 2), 16);
			result = new Color(num / 255f, (float)num2 / 255f, (float)num3 / 255f, (float)num4 / 255f);
		}
		catch
		{
			result = Color.white;
		}
		return result;
	}
}
