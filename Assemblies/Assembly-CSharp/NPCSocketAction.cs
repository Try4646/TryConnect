using System;
using UnityEngine;

// Token: 0x020001CC RID: 460
public abstract class NPCSocketAction : ScriptableObject
{
	// Token: 0x0600108B RID: 4235
	public abstract void OnEnter(NPC npc, NPCSocket socket);

	// Token: 0x0600108C RID: 4236
	public abstract void OnUpdate(NPC npc, NPCSocket socket);

	// Token: 0x0600108D RID: 4237
	public abstract void OnExit(NPC npc, NPCSocket socket);

	// Token: 0x0600108E RID: 4238 RVA: 0x0004711E File Offset: 0x0004531E
	public float GetRandomDuration()
	{
		return Random.Range(this.minDuration, this.maxDuration);
	}

	// Token: 0x17000182 RID: 386
	// (get) Token: 0x0600108F RID: 4239 RVA: 0x00047131 File Offset: 0x00045331
	public float MinDuration
	{
		get
		{
			return this.minDuration;
		}
	}

	// Token: 0x06001090 RID: 4240 RVA: 0x00047139 File Offset: 0x00045339
	protected bool ShouldInterested()
	{
		return Random.value < this.interestChance;
	}

	// Token: 0x04000AB7 RID: 2743
	[Header("Action Settings")]
	[SerializeField]
	protected string animationTrigger;

	// Token: 0x04000AB8 RID: 2744
	[SerializeField]
	protected string actionDoneTrigger;

	// Token: 0x04000AB9 RID: 2745
	[SerializeField]
	protected float minDuration = 5f;

	// Token: 0x04000ABA RID: 2746
	[SerializeField]
	protected float maxDuration = 15f;

	// Token: 0x04000ABB RID: 2747
	[SerializeField]
	protected float interestChance = 0.3f;
}
