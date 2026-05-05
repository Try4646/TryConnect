using System;

// Token: 0x02000280 RID: 640
[Serializable]
public struct SFXParams
{
	// Token: 0x060016C7 RID: 5831 RVA: 0x0006124E File Offset: 0x0005F44E
	public SFXParams(string n, float v)
	{
		this.name = n;
		this.value = v;
	}

	// Token: 0x04000ED5 RID: 3797
	public string name;

	// Token: 0x04000ED6 RID: 3798
	public float value;
}
