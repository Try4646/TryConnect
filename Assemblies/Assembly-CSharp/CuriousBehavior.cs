using System;
using System.Collections.Generic;
using Extensions;
using UnityEngine;
using UnityEngine.AI;

// Token: 0x020001BB RID: 443
[CreateAssetMenu(menuName = "NPC Behaviors/Curious Behavior")]
public class CuriousBehavior : NPCBehavior
{
	// Token: 0x06001009 RID: 4105 RVA: 0x000443E0 File Offset: 0x000425E0
	public override void UpdateBehavior(NPC npc, NPCBehaviorState state)
	{
		NavMeshAgent agent = npc.Agent;
		if (state.currentState == NPCBehaviorState.State.UsingSocket)
		{
			this.UpdateSocketUsage(npc, state);
			return;
		}
		if (agent == null || !agent.isOnNavMesh)
		{
			return;
		}
		switch (state.currentState)
		{
		case NPCBehaviorState.State.Idle:
			agent.isStopped = true;
			if (base.ShouldCheckSockets())
			{
				NPCSocket npcsocket = base.FindAvailableSocket(npc.Transform.position, null);
				if (npcsocket != null && !this.IsSocketOnCooldown(state, npcsocket))
				{
					this.StartUsingSocket(npc, state, npcsocket);
					return;
				}
			}
			if (Time.time >= state.stateUntil)
			{
				state.currentState = NPCBehaviorState.State.Roaming;
				state.stateUntil = Time.time + base.GetRandomRoamTime();
				this.StartRoaming(npc, state);
				return;
			}
			break;
		case NPCBehaviorState.State.Roaming:
			agent.isStopped = false;
			if (agent.pathPending)
			{
				return;
			}
			if (agent.remainingDistance <= agent.stoppingDistance + 0.1f || Time.time >= state.stateUntil)
			{
				state.currentState = NPCBehaviorState.State.Idle;
				state.stateUntil = Time.time + base.GetRandomIdleTime();
				agent.isStopped = true;
				return;
			}
			break;
		case NPCBehaviorState.State.Watching:
			agent.isStopped = false;
			if (agent.pathPending)
			{
				return;
			}
			if (agent.remainingDistance <= this.approachDistance)
			{
				agent.isStopped = true;
				Vector3 normalized = (state.watchPosition - npc.Transform.position).normalized;
				if (normalized != Vector3.zero)
				{
					Quaternion b = Quaternion.LookRotation(normalized);
					npc.Transform.rotation = Quaternion.Slerp(npc.Transform.rotation, b, Time.deltaTime * 2f);
				}
			}
			if (Time.time >= state.stateUntil)
			{
				state.currentState = NPCBehaviorState.State.Idle;
				state.stateUntil = Time.time + base.GetRandomIdleTime();
			}
			break;
		default:
			return;
		}
	}

	// Token: 0x0600100A RID: 4106 RVA: 0x000445A0 File Offset: 0x000427A0
	private void StartUsingSocket(NPC npc, NPCBehaviorState state, NPCSocket socket)
	{
		state.currentState = NPCBehaviorState.State.UsingSocket;
		state.currentSocket = socket;
		state.hasEnteredSocket = false;
		state.socketStartTime = 0f;
		state.socketAttemptStartTime = Time.time;
		state.stateUntil = 0f;
		socket.Reserve();
		NavMeshAgent agent = npc.Agent;
		NavMeshHit navMeshHit;
		if (agent != null && agent.isOnNavMesh && NavMesh.SamplePosition(socket.Position, out navMeshHit, 2f, -1))
		{
			npc.SetDestination(navMeshHit.position);
		}
	}

	// Token: 0x0600100B RID: 4107 RVA: 0x00044624 File Offset: 0x00042824
	private void UpdateSocketUsage(NPC npc, NPCBehaviorState state)
	{
		NavMeshAgent agent = npc.Agent;
		if (state.currentSocket == null)
		{
			state.currentState = NPCBehaviorState.State.Idle;
			state.stateUntil = Time.time + base.GetRandomIdleTime();
			return;
		}
		if (Vector3.Distance(npc.Transform.position, state.currentSocket.Position) <= state.currentSocket.UseRadius)
		{
			if (!state.hasEnteredSocket)
			{
				state.hasEnteredSocket = true;
				state.socketStartTime = Time.time;
				float num = (state.currentSocket.Action != null) ? state.currentSocket.Action.GetRandomDuration() : 10f;
				state.stateUntil = Time.time + num;
				if (state.currentSocket.Action != null)
				{
					state.currentSocket.Action.OnEnter(npc, state.currentSocket);
				}
			}
			if (state.currentSocket.Action != null)
			{
				state.currentSocket.Action.OnUpdate(npc, state.currentSocket);
			}
		}
		else
		{
			if (agent != null && agent.enabled)
			{
				agent.isStopped = false;
			}
			if (!state.hasEnteredSocket && (Time.time - state.socketAttemptStartTime > 15f || (agent != null && agent.enabled && !agent.pathPending && agent.pathStatus == NavMeshPathStatus.PathInvalid)))
			{
				state.currentSocket.Release();
				state.currentSocket = null;
				state.currentState = NPCBehaviorState.State.Idle;
				state.stateUntil = Time.time + base.GetRandomIdleTime();
				return;
			}
		}
		if (state.hasEnteredSocket && Time.time >= state.stateUntil)
		{
			if (state.currentSocket.Action != null)
			{
				state.currentSocket.Action.OnExit(npc, state.currentSocket);
			}
			NPCSocket currentSocket = state.currentSocket;
			state.currentSocket.Release();
			state.currentSocket = null;
			state.hasEnteredSocket = false;
			state.socketStartTime = 0f;
			if (currentSocket != null)
			{
				state.socketCooldowns[currentSocket] = Time.time + 30f;
			}
			state.currentState = NPCBehaviorState.State.Idle;
			state.stateUntil = Time.time + base.GetRandomIdleTime();
		}
	}

	// Token: 0x0600100C RID: 4108 RVA: 0x00044864 File Offset: 0x00042A64
	public override void OnPayoutRecorded(NPC npc, PayoutRecord record, Vector3 npcPosition)
	{
		if (NetworkSingleton<GameManager>.Instance == null)
		{
			return;
		}
		long num = (long)((float)NetworkSingleton<GameManager>.Instance.currentQuota * this.payoutThreshold);
		if (record.payout < num)
		{
			return;
		}
		if (Vector3.Distance(npcPosition, record.gamePosition) > this.watchRadius)
		{
			return;
		}
		NPCController instance = NetworkSingleton<NPCController>.Instance;
		NPCBehaviorState npcbehaviorState = (instance != null) ? instance.GetNPCState(npc) : null;
		if (npcbehaviorState == null)
		{
			return;
		}
		if (npcbehaviorState.currentState == NPCBehaviorState.State.UsingSocket && npcbehaviorState.currentSocket != null)
		{
			float num2 = Time.time - npcbehaviorState.socketStartTime;
			float num3 = (npcbehaviorState.currentSocket.Action != null) ? npcbehaviorState.currentSocket.Action.MinDuration : 5f;
			if (num2 < num3)
			{
				return;
			}
		}
		npcbehaviorState.currentState = NPCBehaviorState.State.Watching;
		npcbehaviorState.watchPosition = record.gamePosition;
		npcbehaviorState.stateUntil = Time.time + this.watchDuration;
		NavMeshAgent agent = npc.Agent;
		if (agent != null && agent.isOnNavMesh)
		{
			Vector3 vector = Random.insideUnitCircle * 2f;
			NavMeshHit navMeshHit;
			if (NavMesh.SamplePosition(record.gamePosition + new Vector3(vector.x, 0f, vector.y), out navMeshHit, 5f, -1))
			{
				npc.SetDestination(navMeshHit.position);
			}
		}
	}

	// Token: 0x0600100D RID: 4109 RVA: 0x000449B0 File Offset: 0x00042BB0
	public override Vector3 ChooseRoamTarget(Vector3 currentPosition, float radius)
	{
		Vector2 vector = Random.insideUnitCircle * radius;
		NavMeshHit navMeshHit;
		if (NavMesh.SamplePosition(currentPosition + new Vector3(vector.x, 0f, vector.y), out navMeshHit, radius, -1))
		{
			return navMeshHit.position;
		}
		return currentPosition;
	}

	// Token: 0x0600100E RID: 4110 RVA: 0x000449FC File Offset: 0x00042BFC
	private void StartRoaming(NPC npc, NPCBehaviorState state)
	{
		NavMeshAgent agent = npc.Agent;
		if (agent != null && agent.isOnNavMesh)
		{
			Vector3 vector = this.ChooseRoamTarget(npc.Transform.position, this.roamRadius);
			state.targetPosition = vector;
			NavMeshHit navMeshHit;
			if (NavMesh.SamplePosition(vector, out navMeshHit, 2f, -1))
			{
				npc.SetDestination(navMeshHit.position);
			}
		}
	}

	// Token: 0x0600100F RID: 4111 RVA: 0x00044A5D File Offset: 0x00042C5D
	private bool IsSocketOnCooldown(NPCBehaviorState state, NPCSocket socket)
	{
		this.CleanExpiredCooldowns(state);
		return state.socketCooldowns.ContainsKey(socket) && Time.time < state.socketCooldowns[socket];
	}

	// Token: 0x06001010 RID: 4112 RVA: 0x00044A8C File Offset: 0x00042C8C
	private void CleanExpiredCooldowns(NPCBehaviorState state)
	{
		List<NPCSocket> list = new List<NPCSocket>();
		foreach (KeyValuePair<NPCSocket, float> keyValuePair in state.socketCooldowns)
		{
			if (Time.time >= keyValuePair.Value)
			{
				list.Add(keyValuePair.Key);
			}
		}
		foreach (NPCSocket key in list)
		{
			state.socketCooldowns.Remove(key);
		}
	}

	// Token: 0x04000A5F RID: 2655
	[Header("Curious Settings")]
	[SerializeField]
	[Range(0f, 1f)]
	private float payoutThreshold = 0.1f;

	// Token: 0x04000A60 RID: 2656
	[SerializeField]
	private float watchRadius = 25f;

	// Token: 0x04000A61 RID: 2657
	[SerializeField]
	private float watchDuration = 15f;

	// Token: 0x04000A62 RID: 2658
	[SerializeField]
	private float approachDistance = 3f;
}
