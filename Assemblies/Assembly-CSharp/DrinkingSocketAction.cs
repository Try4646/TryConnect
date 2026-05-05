using System;
using Mirror;
using UnityEngine;
using UnityEngine.AI;

// Token: 0x020001D7 RID: 471
[CreateAssetMenu(menuName = "NPC Socket Actions/Drinking Action")]
public class DrinkingSocketAction : NPCSocketAction
{
	// Token: 0x060010C4 RID: 4292 RVA: 0x00047F4C File Offset: 0x0004614C
	public override void OnEnter(NPC npc, NPCSocket socket)
	{
		NavMeshAgent agent = npc.Agent;
		if (agent != null)
		{
			agent.enabled = false;
		}
		Rigidbody component = npc.GetComponent<Rigidbody>();
		if (component != null)
		{
			component.isKinematic = true;
		}
		Quaternion rotation = Quaternion.LookRotation(socket.Forward);
		npc.Transform.rotation = rotation;
		if (!string.IsNullOrEmpty(this.animationTrigger) && npc.isServer)
		{
			NetworkAnimator component2 = npc.GetComponent<NetworkAnimator>();
			if (component2 != null)
			{
				component2.SetTrigger(this.animationTrigger);
				return;
			}
			Animator component3 = npc.GetComponent<Animator>();
			if (component3 != null)
			{
				component3.SetTrigger(this.animationTrigger);
			}
		}
	}

	// Token: 0x060010C5 RID: 4293 RVA: 0x00047FF0 File Offset: 0x000461F0
	public override void OnUpdate(NPC npc, NPCSocket socket)
	{
		Quaternion b = Quaternion.LookRotation(socket.Forward);
		npc.Transform.rotation = Quaternion.Slerp(npc.Transform.rotation, b, Time.deltaTime * 1f);
	}

	// Token: 0x060010C6 RID: 4294 RVA: 0x00048030 File Offset: 0x00046230
	public override void OnExit(NPC npc, NPCSocket socket)
	{
		Rigidbody component = npc.GetComponent<Rigidbody>();
		if (component != null)
		{
			component.isKinematic = false;
		}
		NavMeshAgent agent = npc.Agent;
		if (agent != null)
		{
			agent.enabled = true;
			npc.Warp(npc.Transform.position);
		}
		if (!string.IsNullOrEmpty(this.actionDoneTrigger) && npc.isServer)
		{
			NetworkAnimator component2 = npc.GetComponent<NetworkAnimator>();
			if (component2 != null)
			{
				component2.SetTrigger(this.actionDoneTrigger);
				return;
			}
			Animator component3 = npc.GetComponent<Animator>();
			if (component3 != null)
			{
				component3.SetTrigger(this.actionDoneTrigger);
			}
		}
	}
}
