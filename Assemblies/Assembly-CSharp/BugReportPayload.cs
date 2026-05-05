using System;

// Token: 0x0200000E RID: 14
[Serializable]
public class BugReportPayload
{
	// Token: 0x04000028 RID: 40
	public string title;

	// Token: 0x04000029 RID: 41
	public string version;

	// Token: 0x0400002A RID: 42
	public string severity;

	// Token: 0x0400002B RID: 43
	public string category;

	// Token: 0x0400002C RID: 44
	public string whatHappened;

	// Token: 0x0400002D RID: 45
	public string expected;

	// Token: 0x0400002E RID: 46
	public string[] reproSteps;

	// Token: 0x0400002F RID: 47
	public string frequency;

	// Token: 0x04000030 RID: 48
	public string priority;

	// Token: 0x04000031 RID: 49
	public bool canReproduceNow;

	// Token: 0x04000032 RID: 50
	public string build;

	// Token: 0x04000033 RID: 51
	public string branch;

	// Token: 0x04000034 RID: 52
	public string channel;

	// Token: 0x04000035 RID: 53
	public string platform;

	// Token: 0x04000036 RID: 54
	public string os;

	// Token: 0x04000037 RID: 55
	public string gpu;

	// Token: 0x04000038 RID: 56
	public string cpu;

	// Token: 0x04000039 RID: 57
	public int ram;

	// Token: 0x0400003A RID: 58
	public string driverVersion;

	// Token: 0x0400003B RID: 59
	public string scene;

	// Token: 0x0400003C RID: 60
	public string level;

	// Token: 0x0400003D RID: 61
	public string seed;

	// Token: 0x0400003E RID: 62
	public string role;

	// Token: 0x0400003F RID: 63
	public string lobby;

	// Token: 0x04000040 RID: 64
	public string region;

	// Token: 0x04000041 RID: 65
	public int ping;

	// Token: 0x04000042 RID: 66
	public float jitter;

	// Token: 0x04000043 RID: 67
	public float packetLoss;

	// Token: 0x04000044 RID: 68
	public string matchId;

	// Token: 0x04000045 RID: 69
	public string sessionId;

	// Token: 0x04000046 RID: 70
	public int playerCount;

	// Token: 0x04000047 RID: 71
	public string networkBackend;

	// Token: 0x04000048 RID: 72
	public string timeSinceMatchStart;

	// Token: 0x04000049 RID: 73
	public string timestampUtc;

	// Token: 0x0400004A RID: 74
	public string gameId;
}
