using System;
using System.Collections.Generic;
using UnityEngine;

// Token: 0x020001C4 RID: 452
public class NPCBehaviorState
{
	// Token: 0x04000A89 RID: 2697
	public NPCBehaviorState.State currentState;

	// Token: 0x04000A8A RID: 2698
	public float stateUntil;

	// Token: 0x04000A8B RID: 2699
	public Vector3 targetPosition;

	// Token: 0x04000A8C RID: 2700
	public Vector3 watchPosition;

	// Token: 0x04000A8D RID: 2701
	public Transform socialTarget;

	// Token: 0x04000A8E RID: 2702
	public NPCSocket currentSocket;

	// Token: 0x04000A8F RID: 2703
	public bool hasEnteredSocket;

	// Token: 0x04000A90 RID: 2704
	public float socketStartTime;

	// Token: 0x04000A91 RID: 2705
	public float socketAttemptStartTime;

	// Token: 0x04000A92 RID: 2706
	public Dictionary<NPCSocket, float> socketCooldowns = new Dictionary<NPCSocket, float>();

	// Token: 0x020001C5 RID: 453
	public enum State
	{
		// Token: 0x04000A94 RID: 2708
		Idle,
		// Token: 0x04000A95 RID: 2709
		Roaming,
		// Token: 0x04000A96 RID: 2710
		Watching,
		// Token: 0x04000A97 RID: 2711
		Socializing,
		// Token: 0x04000A98 RID: 2712
		UsingSocket
	}
}
