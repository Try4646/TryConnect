using System;
using System.Collections.Generic;

// Token: 0x020002E5 RID: 741
[Serializable]
public class SentencePattern
{
	// Token: 0x060019C7 RID: 6599 RVA: 0x0006BEBE File Offset: 0x0006A0BE
	public SentencePattern(string name, IEnumerable<PatternToken> toks)
	{
		this.name = name;
		this.tokens = new List<PatternToken>(toks);
	}

	// Token: 0x04001090 RID: 4240
	public string name;

	// Token: 0x04001091 RID: 4241
	public List<PatternToken> tokens;
}
