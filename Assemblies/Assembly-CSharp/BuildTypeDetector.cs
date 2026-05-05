using System;
using UnityEngine;

// Token: 0x020002C8 RID: 712
public static class BuildTypeDetector
{
	// Token: 0x0600193E RID: 6462 RVA: 0x0006A586 File Offset: 0x00068786
	public static BuildTypeDetector.BuildType GetCurrentBuildType()
	{
		return BuildTypeDetector.BuildType.SteamBuild;
	}

	// Token: 0x0600193F RID: 6463 RVA: 0x0006A589 File Offset: 0x00068789
	public static bool IsLocalBuild()
	{
		return BuildTypeDetector.GetCurrentBuildType() == BuildTypeDetector.BuildType.LocalBuild;
	}

	// Token: 0x06001940 RID: 6464 RVA: 0x0006A593 File Offset: 0x00068793
	public static bool IsSteamBuild()
	{
		return BuildTypeDetector.GetCurrentBuildType() == BuildTypeDetector.BuildType.SteamBuild;
	}

	// Token: 0x06001941 RID: 6465 RVA: 0x0006A5A0 File Offset: 0x000687A0
	public static string GetBuildTypeString()
	{
		BuildTypeDetector.BuildType currentBuildType = BuildTypeDetector.GetCurrentBuildType();
		if (currentBuildType == BuildTypeDetector.BuildType.LocalBuild)
		{
			return "Local Build";
		}
		if (currentBuildType != BuildTypeDetector.BuildType.SteamBuild)
		{
			return "Unknown Build";
		}
		return "Steam Build";
	}

	// Token: 0x06001942 RID: 6466 RVA: 0x0006A5CE File Offset: 0x000687CE
	public static void LogBuildType()
	{
		Debug.Log("Current Build Type: " + BuildTypeDetector.GetBuildTypeString());
	}

	// Token: 0x020002C9 RID: 713
	public enum BuildType
	{
		// Token: 0x0400103B RID: 4155
		Unknown,
		// Token: 0x0400103C RID: 4156
		LocalBuild,
		// Token: 0x0400103D RID: 4157
		SteamBuild
	}
}
