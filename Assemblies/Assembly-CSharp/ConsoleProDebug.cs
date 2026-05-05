using System;
using UnityEngine;

// Token: 0x0200028B RID: 651
public static class ConsoleProDebug
{
	// Token: 0x06001732 RID: 5938 RVA: 0x000048A7 File Offset: 0x00002AA7
	public static void Clear()
	{
	}

	// Token: 0x06001733 RID: 5939 RVA: 0x0006278C File Offset: 0x0006098C
	public static void LogToFilter(string inLog, string inFilterName, Object inContext = null)
	{
		Debug.Log(inLog + "\nCPAPI:{\"cmd\":\"Filter\", \"name\":\"" + inFilterName + "\"}", inContext);
	}

	// Token: 0x06001734 RID: 5940 RVA: 0x000627A5 File Offset: 0x000609A5
	public static void LogAsType(string inLog, string inTypeName, Object inContext = null)
	{
		Debug.Log(inLog + "\nCPAPI:{\"cmd\":\"LogType\", \"name\":\"" + inTypeName + "\"}", inContext);
	}

	// Token: 0x06001735 RID: 5941 RVA: 0x000627BE File Offset: 0x000609BE
	public static void Watch(string inName, string inValue)
	{
		Debug.Log(string.Concat(new string[]
		{
			inName,
			" : ",
			inValue,
			"\nCPAPI:{\"cmd\":\"Watch\", \"name\":\"",
			inName,
			"\"}"
		}));
	}

	// Token: 0x06001736 RID: 5942 RVA: 0x000627F4 File Offset: 0x000609F4
	public static void Search(string inText)
	{
		Debug.Log("\nCPAPI:{\"cmd\":\"Search\", \"text\":\"" + inText + "\"}");
	}
}
