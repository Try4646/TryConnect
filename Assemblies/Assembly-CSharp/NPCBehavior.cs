using System;
using Extensions;
using UnityEngine;

// Token: 0x020001C3 RID: 451
public abstract class NPCBehavior : ScriptableObject
{
	// Token: 0x0600105D RID: 4189
	public abstract void UpdateBehavior(NPC npc, NPCBehaviorState state);

	// Token: 0x0600105E RID: 4190
	public abstract void OnPayoutRecorded(NPC npc, PayoutRecord record, Vector3 npcPosition);

	// Token: 0x0600105F RID: 4191
	public abstract Vector3 ChooseRoamTarget(Vector3 currentPosition, float radius);

	// Token: 0x06001060 RID: 4192 RVA: 0x00046702 File Offset: 0x00044902
	protected float GetRandomIdleTime()
	{
		return Random.Range(this.minIdleTime, this.maxIdleTime);
	}

	// Token: 0x06001061 RID: 4193 RVA: 0x00046715 File Offset: 0x00044915
	protected float GetRandomRoamTime()
	{
		return Random.Range(this.minRoamTime, this.maxRoamTime);
	}

	// Token: 0x06001062 RID: 4194 RVA: 0x00046728 File Offset: 0x00044928
	protected bool ShouldCheckSockets()
	{
		return Random.value < this.socketInterestChance;
	}

	// Token: 0x06001063 RID: 4195 RVA: 0x00046737 File Offset: 0x00044937
	protected NPCSocket FindAvailableSocket(Vector3 position, NPCSocketAction preferredAction = null)
	{
		if (MonoSingleton<NPCSocketManager>.Instance == null)
		{
			return null;
		}
		return MonoSingleton<NPCSocketManager>.Instance.FindAvailableSocket(position, this.socketSearchRadius, preferredAction);
	}

	// Token: 0x04000A82 RID: 2690
	[Header("General Settings")]
	[SerializeField]
	protected float minIdleTime = 2f;

	// Token: 0x04000A83 RID: 2691
	[SerializeField]
	protected float maxIdleTime = 6f;

	// Token: 0x04000A84 RID: 2692
	[SerializeField]
	protected float minRoamTime = 5f;

	// Token: 0x04000A85 RID: 2693
	[SerializeField]
	protected float maxRoamTime = 12f;

	// Token: 0x04000A86 RID: 2694
	[SerializeField]
	protected float roamRadius = 15f;

	// Token: 0x04000A87 RID: 2695
	[Header("Socket Settings")]
	[SerializeField]
	protected float socketInterestChance = 0.2f;

	// Token: 0x04000A88 RID: 2696
	[SerializeField]
	protected float socketSearchRadius = 30f;
}
