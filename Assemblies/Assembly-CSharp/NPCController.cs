using System;
using System.Collections.Generic;
using Extensions;
using Gilzoide.UpdateManager;
using Mirror;
using UnityEngine;

// Token: 0x020001C6 RID: 454
public class NPCController : NetworkSingleton<NPCController>, IUpdatable, IManagedObject
{
	// Token: 0x06001066 RID: 4198 RVA: 0x000467CF File Offset: 0x000449CF
	protected override void OnAwake()
	{
		base.OnAwake();
	}

	// Token: 0x06001067 RID: 4199 RVA: 0x000467D7 File Offset: 0x000449D7
	private void OnEnable()
	{
		this.RegisterInManager();
		if (NetworkSingleton<PayoutTracker>.Instance != null)
		{
			NetworkSingleton<PayoutTracker>.Instance.OnPayoutRecorded += this.OnPayoutRecorded;
		}
	}

	// Token: 0x06001068 RID: 4200 RVA: 0x00046802 File Offset: 0x00044A02
	private void OnDisable()
	{
		this.UnregisterInManager();
		if (NetworkSingleton<PayoutTracker>.Instance != null)
		{
			NetworkSingleton<PayoutTracker>.Instance.OnPayoutRecorded -= this.OnPayoutRecorded;
		}
	}

	// Token: 0x06001069 RID: 4201 RVA: 0x0004682D File Offset: 0x00044A2D
	public void ManagedUpdate()
	{
		if (!base.isServer)
		{
			return;
		}
		this.UpdateNPCs();
	}

	// Token: 0x0600106A RID: 4202 RVA: 0x00046840 File Offset: 0x00044A40
	[Server]
	public void RegisterNPC(NPC npc)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void NPCController::RegisterNPC(NPC)' called when server was not active");
			return;
		}
		if (npc == null || this.allNPCs.Contains(npc))
		{
			return;
		}
		this.allNPCs.Add(npc);
		npc.Initialize(this.walkSpeed, this.stoppingDistance);
		NPCBehaviorState npcbehaviorState = new NPCBehaviorState();
		npcbehaviorState.currentState = NPCBehaviorState.State.Idle;
		npcbehaviorState.stateUntil = Time.time + 2f;
		this.npcStates[npc] = npcbehaviorState;
	}

	// Token: 0x0600106B RID: 4203 RVA: 0x000468C3 File Offset: 0x00044AC3
	[Server]
	public void UnregisterNPC(NPC npc)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void NPCController::UnregisterNPC(NPC)' called when server was not active");
			return;
		}
		if (npc == null)
		{
			return;
		}
		this.allNPCs.Remove(npc);
		this.npcStates.Remove(npc);
	}

	// Token: 0x0600106C RID: 4204 RVA: 0x000468FE File Offset: 0x00044AFE
	public NPCBehaviorState GetNPCState(NPC npc)
	{
		if (!this.npcStates.ContainsKey(npc))
		{
			return null;
		}
		return this.npcStates[npc];
	}

	// Token: 0x0600106D RID: 4205 RVA: 0x0004691C File Offset: 0x00044B1C
	private void UpdateNPCs()
	{
		foreach (NPC npc in this.allNPCs)
		{
			if (!(npc == null) && this.npcStates.ContainsKey(npc) && npc.State != NPC.NPCState.Ragdoll && npc.gameObject.activeSelf)
			{
				NPCBehaviorState npcbehaviorState = this.npcStates[npc];
				if (npc.Behavior != null)
				{
					npc.Behavior.UpdateBehavior(npc, npcbehaviorState);
				}
				npc.SetDebugState(npcbehaviorState.currentState.ToString());
				if (npc.npcSfx != null)
				{
					npc.npcSfx.ManagedUpdate();
				}
			}
		}
	}

	// Token: 0x0600106E RID: 4206 RVA: 0x000469F4 File Offset: 0x00044BF4
	[Server]
	private void OnPayoutRecorded(PayoutRecord record)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void NPCController::OnPayoutRecorded(PayoutRecord)' called when server was not active");
			return;
		}
		if (!base.isServer)
		{
			return;
		}
		foreach (NPC npc in this.allNPCs)
		{
			if (!(npc == null) && this.npcStates.ContainsKey(npc) && !(npc.Behavior == null))
			{
				npc.Behavior.OnPayoutRecorded(npc, record, npc.Transform.position);
			}
		}
	}

	// Token: 0x06001070 RID: 4208 RVA: 0x00002321 File Offset: 0x00000521
	public override bool Weaved()
	{
		return true;
	}

	// Token: 0x04000A99 RID: 2713
	[Header("NPC Settings")]
	[SerializeField]
	private float walkSpeed = 3.5f;

	// Token: 0x04000A9A RID: 2714
	[SerializeField]
	private float stoppingDistance = 0.5f;

	// Token: 0x04000A9B RID: 2715
	private List<NPC> allNPCs = new List<NPC>();

	// Token: 0x04000A9C RID: 2716
	private Dictionary<NPC, NPCBehaviorState> npcStates = new Dictionary<NPC, NPCBehaviorState>();
}
