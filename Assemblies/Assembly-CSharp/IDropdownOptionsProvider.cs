using System;
using System.Collections.Generic;

// Token: 0x02000314 RID: 788
public interface IDropdownOptionsProvider
{
	// Token: 0x06001AB2 RID: 6834
	List<string> GetOptions();

	// Token: 0x06001AB3 RID: 6835
	int GetDefaultIndex(List<string> options);
}
