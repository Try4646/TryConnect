using System;
using System.Collections.Generic;
using Extensions;
using UnityEngine;
using UnityEngine.AI;

// Token: 0x020001BD RID: 445
[CreateAssetMenu(menuName = "NPC Behaviors/Social Behavior")]
public class SocialBehavior : NPCBehavior
{
	// Token: 0x0600101C RID: 4124 RVA: 0x00045498 File Offset: 0x00043698
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
			this.CheckForNearbyPeople(npc, state);
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
			this.CheckForNearbyPeople(npc, state);
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
			break;
		case NPCBehaviorState.State.Socializing:
			agent.isStopped = false;
			if (agent.pathPending)
			{
				return;
			}
			if (agent.remainingDistance <= this.approachDistance)
			{
				agent.isStopped = true;
				if (state.socialTarget != null)
				{
					Vector3 normalized = (state.socialTarget.position - npc.Transform.position).normalized;
					if (normalized != Vector3.zero)
					{
						Quaternion b = Quaternion.LookRotation(normalized);
						npc.Transform.rotation = Quaternion.Slerp(npc.Transform.rotation, b, Time.deltaTime * 2f);
					}
				}
			}
			if (Time.time >= state.stateUntil || state.socialTarget == null)
			{
				state.currentState = NPCBehaviorState.State.Idle;
				state.stateUntil = Time.time + base.GetRandomIdleTime();
				state.socialTarget = null;
			}
			break;
		default:
			return;
		}
	}

	// Token: 0x0600101D RID: 4125 RVA: 0x00045694 File Offset: 0x00043894
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

	// Token: 0x0600101E RID: 4126 RVA: 0x00045718 File Offset: 0x00043918
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

	// Token: 0x0600101F RID: 4127 RVA: 0x00045957 File Offset: 0x00043B57
	private bool IsSocketOnCooldown(NPCBehaviorState state, NPCSocket socket)
	{
		this.CleanExpiredCooldowns(state);
		return state.socketCooldowns.ContainsKey(socket) && Time.time < state.socketCooldowns[socket];
	}

	// Token: 0x06001020 RID: 4128 RVA: 0x00045984 File Offset: 0x00043B84
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

	// Token: 0x06001021 RID: 4129 RVA: 0x00045A38 File Offset: 0x00043C38
	private void CheckForNearbyPeople(NPC npc, NPCBehaviorState state)
	{
		if (state.currentState == NPCBehaviorState.State.Socializing)
		{
			return;
		}
		if (state.currentState == NPCBehaviorState.State.UsingSocket && state.currentSocket != null)
		{
			float num = Time.time - state.socketStartTime;
			float num2 = (state.currentSocket.Action != null) ? state.currentSocket.Action.MinDuration : 5f;
			if (num < num2)
			{
				return;
			}
		}
		PlayerProfile[] array = Object.FindObjectsByType<PlayerProfile>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
		Transform transform = null;
		float num3 = float.MaxValue;
		foreach (PlayerProfile playerProfile in array)
		{
			if (!(playerProfile == null) && !(playerProfile.transform == null))
			{
				float num4 = Vector3.Distance(npc.Transform.position, playerProfile.transform.position);
				if (num4 < this.socialRadius && num4 < num3)
				{
					transform = playerProfile.transform;
					num3 = num4;
				}
			}
		}
		if (transform != null && Random.value < 0.3f)
		{
			state.currentState = NPCBehaviorState.State.Socializing;
			state.socialTarget = transform;
			state.stateUntil = Time.time + this.socialDuration;
			NavMeshAgent agent = npc.Agent;
			if (agent != null && agent.isOnNavMesh)
			{
				Vector3 vector = Random.insideUnitCircle * 1.5f;
				NavMeshHit navMeshHit;
				if (NavMesh.SamplePosition(transform.position + new Vector3(vector.x, 0f, vector.y), out navMeshHit, 3f, -1))
				{
					npc.SetDestination(navMeshHit.position);
				}
			}
		}
	}

	// Token: 0x06001022 RID: 4130 RVA: 0x00045BC8 File Offset: 0x00043DC8
	public override void OnPayoutRecorded(NPC npc, PayoutRecord record, Vector3 npcPosition)
	{
		if (record.payout < 1000L || Random.value > 0.2f)
		{
			return;
		}
		if (Vector3.Distance(npcPosition, record.gamePosition) > this.socialRadius)
		{
			return;
		}
		NPCController instance = NetworkSingleton<NPCController>.Instance;
		NPCBehaviorState npcbehaviorState = (instance != null) ? instance.GetNPCState(npc) : null;
		if (npcbehaviorState == null || npcbehaviorState.currentState == NPCBehaviorState.State.Socializing)
		{
			return;
		}
		if (npcbehaviorState.currentState == NPCBehaviorState.State.UsingSocket && npcbehaviorState.currentSocket != null)
		{
			float num = Time.time - npcbehaviorState.socketStartTime;
			float num2 = (npcbehaviorState.currentSocket.Action != null) ? npcbehaviorState.currentSocket.Action.MinDuration : 5f;
			if (num < num2)
			{
				return;
			}
		}
		npcbehaviorState.currentState = NPCBehaviorState.State.Watching;
		npcbehaviorState.watchPosition = record.gamePosition;
		npcbehaviorState.stateUntil = Time.time + 8f;
		NavMeshAgent agent = npc.Agent;
		if (agent != null && agent.isOnNavMesh)
		{
			Vector3 vector = Random.insideUnitCircle * 3f;
			NavMeshHit navMeshHit;
			if (NavMesh.SamplePosition(record.gamePosition + new Vector3(vector.x, 0f, vector.y), out navMeshHit, 5f, -1))
			{
				npc.SetDestination(navMeshHit.position);
			}
		}
	}

	// Token: 0x06001023 RID: 4131 RVA: 0x00045D08 File Offset: 0x00043F08
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

	// Token: 0x06001024 RID: 4132 RVA: 0x00045D54 File Offset: 0x00043F54
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

	// Token: 0x04000A68 RID: 2664
	[Header("Social Settings")]
	[SerializeField]
	private float socialRadius = 20f;

	// Token: 0x04000A69 RID: 2665
	[SerializeField]
	private float approachDistance = 2f;

	// Token: 0x04000A6A RID: 2666
	[SerializeField]
	private float socialDuration = 10f;
}
