using System;
using Mirror;
using UnityEngine;
using UnityEngine.AI;

// Token: 0x020001D9 RID: 473
[CreateAssetMenu(menuName = "NPC Socket Actions/Smoking Action")]
public class SmokingSocketAction : NPCSocketAction
{
	// Token: 0x060010CC RID: 4300 RVA: 0x00048254 File Offset: 0x00046454
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

	// Token: 0x060010CD RID: 4301 RVA: 0x000482F8 File Offset: 0x000464F8
	public override void OnUpdate(NPC npc, NPCSocket socket)
	{
		Quaternion b = Quaternion.LookRotation(socket.Forward);
		npc.Transform.rotation = Quaternion.Slerp(npc.Transform.rotation, b, Time.deltaTime * 0.5f);
	}

	// Token: 0x060010CE RID: 4302 RVA: 0x00048338 File Offset: 0x00046538
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
			if (npc.isServer)
			{
				npc.Warp(npc.Transform.position);
			}
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
