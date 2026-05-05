using System;

// Token: 0x020002E4 RID: 740
public struct PatternToken
{
	// Token: 0x060019C5 RID: 6597 RVA: 0x0006BE70 File Offset: 0x0006A070
	public static PatternToken Lit(string text)
	{
		return new PatternToken
		{
			isLiteral = true,
			literal = text
		};
	}

	// Token: 0x060019C6 RID: 6598 RVA: 0x0006BE98 File Offset: 0x0006A098
	public static PatternToken Slot(SlotType s)
	{
		return new PatternToken
		{
			isLiteral = false,
			slot = s
		};
	}

	// Token: 0x0400108D RID: 4237
	public bool isLiteral;

	// Token: 0x0400108E RID: 4238
	public string literal;

	// Token: 0x0400108F RID: 4239
	public SlotType slot;
}
