using System;

// Token: 0x02000241 RID: 577
public interface IUIManager
{
	// Token: 0x170001E4 RID: 484
	// (get) Token: 0x060014D5 RID: 5333
	bool IsActive { get; }

	// Token: 0x060014D6 RID: 5334
	void CloseUI();

	// Token: 0x060014D7 RID: 5335
	void OpenUI();

	// Token: 0x170001E5 RID: 485
	// (get) Token: 0x060014D8 RID: 5336
	int Priority { get; }
}
