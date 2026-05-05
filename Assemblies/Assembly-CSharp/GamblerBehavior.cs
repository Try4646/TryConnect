using System;
using System.Collections.Generic;
using Extensions;
using UnityEngine;
using UnityEngine.AI;

// Token: 0x020001BC RID: 444
[CreateAssetMenu(menuName = "NPC Behaviors/Gambler Behavior")]
public class GamblerBehavior : NPCBehavior
{
	// Token: 0x06001012 RID: 4114 RVA: 0x00044B74 File Offset: 0x00042D74
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
			this.CheckForNearbyGames(npc, state);
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
			this.CheckForNearbyGames(npc, state);
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

	// Token: 0x06001013 RID: 4115 RVA: 0x00044D44 File Offset: 0x00042F44
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

	// Token: 0x06001014 RID: 4116 RVA: 0x00044DC8 File Offset: 0x00042FC8
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

	// Token: 0x06001015 RID: 4117 RVA: 0x00045007 File Offset: 0x00043207
	private bool IsSocketOnCooldown(NPCBehaviorState state, NPCSocket socket)
	{
		this.CleanExpiredCooldowns(state);
		return state.socketCooldowns.ContainsKey(socket) && Time.time < state.socketCooldowns[socket];
	}

	// Token: 0x06001016 RID: 4118 RVA: 0x00045034 File Offset: 0x00043234
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

	// Token: 0x06001017 RID: 4119 RVA: 0x000450E8 File Offset: 0x000432E8
	private void CheckForNearbyGames(NPC npc, NPCBehaviorState state)
	{
		if (state.currentState == NPCBehaviorState.State.Watching)
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
		if (NetworkSingleton<PayoutTracker>.Instance == null)
		{
			return;
		}
		foreach (PayoutRecord payoutRecord in NetworkSingleton<PayoutTracker>.Instance.GetRecordsInTimeRange(Time.time - 5f, Time.time))
		{
			if (payoutRecord.payout >= (long)this.payoutThreshold && Vector3.Distance(npc.Transform.position, payoutRecord.gamePosition) <= this.gameInterestRadius)
			{
				state.currentState = NPCBehaviorState.State.Watching;
				state.watchPosition = payoutRecord.gamePosition;
				state.stateUntil = Time.time + this.watchDuration;
				NavMeshAgent agent = npc.Agent;
				if (!(agent != null) || !agent.isOnNavMesh)
				{
					break;
				}
				Vector3 vector = Random.insideUnitCircle * 1.5f;
				NavMeshHit navMeshHit;
				if (NavMesh.SamplePosition(payoutRecord.gamePosition + new Vector3(vector.x, 0f, vector.y), out navMeshHit, 4f, -1))
				{
					npc.SetDestination(navMeshHit.position);
					break;
				}
				break;
			}
		}
	}

	// Token: 0x06001018 RID: 4120 RVA: 0x00045280 File Offset: 0x00043480
	public override void OnPayoutRecorded(NPC npc, PayoutRecord record, Vector3 npcPosition)
	{
		if (record.payout < (long)this.payoutThreshold)
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
			float num = Time.time - npcbehaviorState.socketStartTime;
			float num2 = (npcbehaviorState.currentSocket.Action != null) ? npcbehaviorState.currentSocket.Action.MinDuration : 5f;
			if (num < num2)
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
			Vector3 vector = Random.insideUnitCircle * 1.5f;
			NavMeshHit navMeshHit;
			if (NavMesh.SamplePosition(record.gamePosition + new Vector3(vector.x, 0f, vector.y), out navMeshHit, 4f, -1))
			{
				npc.SetDestination(navMeshHit.position);
			}
		}
	}

	// Token: 0x06001019 RID: 4121 RVA: 0x000453AC File Offset: 0x000435AC
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

	// Token: 0x0600101A RID: 4122 RVA: 0x000453F8 File Offset: 0x000435F8
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

	// Token: 0x04000A63 RID: 2659
	[Header("Gambler Settings")]
	[SerializeField]
	private int payoutThreshold = 200;

	// Token: 0x04000A64 RID: 2660
	[SerializeField]
	private float watchRadius = 30f;

	// Token: 0x04000A65 RID: 2661
	[SerializeField]
	private float watchDuration = 20f;

	// Token: 0x04000A66 RID: 2662
	[SerializeField]
	private float approachDistance = 2.5f;

	// Token: 0x04000A67 RID: 2663
	[SerializeField]
	private float gameInterestRadius = 15f;
}
