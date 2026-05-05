using System;
using Mirror;
using UnityEngine;
using UnityEngine.AI;

// Token: 0x020001D8 RID: 472
[CreateAssetMenu(menuName = "NPC Socket Actions/Slots Action")]
public class SlotsSocketAction : NPCSocketAction
{
	// Token: 0x060010C8 RID: 4296 RVA: 0x000480D4 File Offset: 0x000462D4
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

	// Token: 0x060010C9 RID: 4297 RVA: 0x00048178 File Offset: 0x00046378
	public override void OnUpdate(NPC npc, NPCSocket socket)
	{
		Quaternion b = Quaternion.LookRotation(socket.Forward);
		npc.Transform.rotation = Quaternion.Slerp(npc.Transform.rotation, b, Time.deltaTime * 2f);
	}

	// Token: 0x060010CA RID: 4298 RVA: 0x000481B8 File Offset: 0x000463B8
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
