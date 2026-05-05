using System;
using System.Collections;
using System.Runtime.InteropServices;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using Extensions;
using Mirror;
using Mirror.RemoteCalls;
using UnityEngine;
using UnityEngine.AI;

// Token: 0x020001BE RID: 446
public class NPC : NetworkBehaviour
{
	// Token: 0x17000171 RID: 369
	// (get) Token: 0x06001026 RID: 4134 RVA: 0x00045DDE File Offset: 0x00043FDE
	public NavMeshAgent Agent
	{
		get
		{
			return this.agent;
		}
	}

	// Token: 0x17000172 RID: 370
	// (get) Token: 0x06001027 RID: 4135 RVA: 0x00045DE6 File Offset: 0x00043FE6
	public Transform Transform
	{
		get
		{
			return this.npcTransform;
		}
	}

	// Token: 0x17000173 RID: 371
	// (get) Token: 0x06001028 RID: 4136 RVA: 0x00045DEE File Offset: 0x00043FEE
	public NPCBehavior Behavior
	{
		get
		{
			return this.behavior;
		}
	}

	// Token: 0x06001029 RID: 4137 RVA: 0x00045DF6 File Offset: 0x00043FF6
	public void SetDebugState(string stateLabel)
	{
		this._debugState = stateLabel;
	}

	// Token: 0x0600102A RID: 4138 RVA: 0x00045E00 File Offset: 0x00044000
	[Server]
	public void SetDestination(Vector3 position)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void NPC::SetDestination(UnityEngine.Vector3)' called when server was not active");
			return;
		}
		if (this.agent == null || !this.agent.isOnNavMesh)
		{
			return;
		}
		this.agent.SetDestination(position);
		this.SetDestinationRpc(position);
	}

	// Token: 0x0600102B RID: 4139 RVA: 0x00045E54 File Offset: 0x00044054
	[ClientRpc]
	private void SetDestinationRpc(Vector3 position)
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		writer.WriteVector3(position);
		this.SendRPCInternal("System.Void NPC::SetDestinationRpc(UnityEngine.Vector3)", 375149186, writer, 0, true);
		NetworkWriterPool.Return(writer);
	}

	// Token: 0x0600102C RID: 4140 RVA: 0x00045E8E File Offset: 0x0004408E
	[Server]
	public void Warp(Vector3 position)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void NPC::Warp(UnityEngine.Vector3)' called when server was not active");
			return;
		}
		if (this.agent == null)
		{
			return;
		}
		this.agent.Warp(position);
		this.WarpRpc(position);
	}

	// Token: 0x0600102D RID: 4141 RVA: 0x00045EC8 File Offset: 0x000440C8
	[ClientRpc]
	private void WarpRpc(Vector3 position)
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		writer.WriteVector3(position);
		this.SendRPCInternal("System.Void NPC::WarpRpc(UnityEngine.Vector3)", -1260865492, writer, 0, true);
		NetworkWriterPool.Return(writer);
	}

	// Token: 0x17000174 RID: 372
	// (get) Token: 0x0600102E RID: 4142 RVA: 0x00045F02 File Offset: 0x00044102
	// (set) Token: 0x0600102F RID: 4143 RVA: 0x00045F0C File Offset: 0x0004410C
	public NPC.NPCState State
	{
		get
		{
			return this._state;
		}
		set
		{
			if (this._ragdollRoutine != null)
			{
				base.StopCoroutine(this._ragdollRoutine);
			}
			if (value == NPC.NPCState.Ragdoll)
			{
				this._ragdollRoutine = base.StartCoroutine(this.DelayedDisableRagdoll());
			}
			if (value == this._state)
			{
				return;
			}
			this.Network_state = value;
			this.SetNPCState(value);
		}
	}

	// Token: 0x06001030 RID: 4144 RVA: 0x00045F5B File Offset: 0x0004415B
	private void OnStateChanged(NPC.NPCState oldState, NPC.NPCState newState)
	{
		this.SetNPCState(newState);
	}

	// Token: 0x06001031 RID: 4145 RVA: 0x00045F64 File Offset: 0x00044164
	private void Awake()
	{
		if (this.agent == null)
		{
			this.agent = base.GetComponent<NavMeshAgent>();
		}
		if (this.npcTransform == null)
		{
			this.npcTransform = base.transform;
		}
		this._rb = base.GetComponent<Rigidbody>();
		this._ps = Resources.Load<PlayerSettings>("PlayerSettings");
		if (this._rb != null)
		{
			this._rb.freezeRotation = true;
			this._rb.isKinematic = false;
		}
	}

	// Token: 0x06001032 RID: 4146 RVA: 0x00045FE7 File Offset: 0x000441E7
	public override void OnStartServer()
	{
		base.OnStartServer();
		base.StartCoroutine(this.RegisterWithController());
	}

	// Token: 0x06001033 RID: 4147 RVA: 0x00045FFC File Offset: 0x000441FC
	private IEnumerator RegisterWithController()
	{
		while (NetworkSingleton<NPCController>.Instance == null)
		{
			yield return null;
		}
		NetworkSingleton<NPCController>.Instance.RegisterNPC(this);
		yield break;
	}

	// Token: 0x06001034 RID: 4148 RVA: 0x0004600C File Offset: 0x0004420C
	private void OnDisable()
	{
		if (!base.isServer)
		{
			return;
		}
		NPCController instance = NetworkSingleton<NPCController>.Instance;
		NPCBehaviorState npcbehaviorState = (instance != null) ? instance.GetNPCState(this) : null;
		if (npcbehaviorState != null && npcbehaviorState.currentState == NPCBehaviorState.State.UsingSocket && npcbehaviorState.currentSocket != null)
		{
			if (npcbehaviorState.currentSocket.Action != null && npcbehaviorState.hasEnteredSocket)
			{
				npcbehaviorState.currentSocket.Action.OnExit(this, npcbehaviorState.currentSocket);
			}
			npcbehaviorState.currentSocket.Release();
			npcbehaviorState.currentSocket = null;
			npcbehaviorState.hasEnteredSocket = false;
			npcbehaviorState.currentState = NPCBehaviorState.State.Idle;
		}
	}

	// Token: 0x06001035 RID: 4149 RVA: 0x000460A1 File Offset: 0x000442A1
	private void OnEnable()
	{
		if (!base.isServer)
		{
			return;
		}
		base.StartCoroutine(this.ResetAnimator());
	}

	// Token: 0x06001036 RID: 4150 RVA: 0x000460B9 File Offset: 0x000442B9
	private IEnumerator ResetAnimator()
	{
		if (!base.gameObject.activeSelf)
		{
			yield break;
		}
		yield return new WaitForSeconds(1f);
		Animator component = base.GetComponent<Animator>();
		if (component != null && component.isInitialized)
		{
			component.Play("Default", 0, 0f);
			component.Update(0f);
			NetworkAnimator component2 = base.GetComponent<NetworkAnimator>();
			if (component2 != null && base.isServer)
			{
				component2.SetTrigger("npc_done");
			}
		}
		yield break;
	}

	// Token: 0x06001037 RID: 4151 RVA: 0x000460C8 File Offset: 0x000442C8
	private void OnDestroy()
	{
		if (Application.isPlaying && NetworkSingleton<NPCController>.Instance != null)
		{
			NetworkSingleton<NPCController>.Instance.UnregisterNPC(this);
		}
	}

	// Token: 0x06001038 RID: 4152 RVA: 0x000460EC File Offset: 0x000442EC
	public void Initialize(float walkSpeed, float stoppingDistance)
	{
		if (this.agent != null)
		{
			this.agent.speed = walkSpeed;
			this.agent.stoppingDistance = stoppingDistance;
			this.agent.acceleration = 8f;
			this.agent.angularSpeed = 120f;
			this.agent.updatePosition = true;
			this.agent.updateRotation = true;
			this.agent.updateUpAxis = true;
		}
	}

	// Token: 0x06001039 RID: 4153 RVA: 0x00046164 File Offset: 0x00044364
	private void SetNPCState(NPC.NPCState newState)
	{
		if (this._rb == null)
		{
			return;
		}
		if (!this._rb.isKinematic)
		{
			this._rb.linearVelocity = Vector3.zero;
		}
		this._rb.constraints = ((newState == NPC.NPCState.Ragdoll) ? RigidbodyConstraints.None : RigidbodyConstraints.FreezeRotation);
		if (this.agent != null)
		{
			if (newState == NPC.NPCState.Free)
			{
				this.agent.enabled = false;
				this._rb.DORotate(Vector3.zero, 0.5f, RotateMode.Fast).SetEase(Ease.OutCubic).OnComplete(delegate
				{
					if (this.agent != null)
					{
						this.agent.enabled = true;
						if (base.isServer)
						{
							this.Warp(base.transform.position);
						}
					}
				});
				return;
			}
			this.agent.enabled = false;
		}
	}

	// Token: 0x0600103A RID: 4154 RVA: 0x0004620A File Offset: 0x0004440A
	private IEnumerator DelayedDisableRagdoll()
	{
		yield return new WaitForSeconds(this._ps.ragdollDuration);
		if (this._state == NPC.NPCState.Ragdoll)
		{
			this.State = NPC.NPCState.Free;
		}
		yield break;
	}

	// Token: 0x0600103B RID: 4155 RVA: 0x00046219 File Offset: 0x00044419
	[Server]
	public void ServerKnockback(Vector3 force, Vector3 torque)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void NPC::ServerKnockback(UnityEngine.Vector3,UnityEngine.Vector3)' called when server was not active");
			return;
		}
		if (this._rb == null)
		{
			return;
		}
		this.State = NPC.NPCState.Ragdoll;
		this.ApplyKnockbackRpc(force, torque);
	}

	// Token: 0x0600103C RID: 4156 RVA: 0x00046250 File Offset: 0x00044450
	[ClientRpc]
	private void ApplyKnockbackRpc(Vector3 force, Vector3 torque)
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		writer.WriteVector3(force);
		writer.WriteVector3(torque);
		this.SendRPCInternal("System.Void NPC::ApplyKnockbackRpc(UnityEngine.Vector3,UnityEngine.Vector3)", 1947053242, writer, 0, true);
		NetworkWriterPool.Return(writer);
	}

	// Token: 0x0600103D RID: 4157 RVA: 0x00046294 File Offset: 0x00044494
	public NPC()
	{
		this._Mirror_SyncVarHookDelegate__state = new Action<NPC.NPCState, NPC.NPCState>(this.OnStateChanged);
	}

	// Token: 0x0600103F RID: 4159 RVA: 0x00002321 File Offset: 0x00000521
	public override bool Weaved()
	{
		return true;
	}

	// Token: 0x17000175 RID: 373
	// (get) Token: 0x06001040 RID: 4160 RVA: 0x000462E4 File Offset: 0x000444E4
	// (set) Token: 0x06001041 RID: 4161 RVA: 0x000462F7 File Offset: 0x000444F7
	public NPC.NPCState Network_state
	{
		get
		{
			return this._state;
		}
		[param: In]
		set
		{
			base.GeneratedSyncVarSetter<NPC.NPCState>(value, ref this._state, 1UL, this._Mirror_SyncVarHookDelegate__state);
		}
	}

	// Token: 0x06001042 RID: 4162 RVA: 0x00046316 File Offset: 0x00044516
	protected void UserCode_SetDestinationRpc__Vector3(Vector3 position)
	{
		if (base.isServer)
		{
			return;
		}
		if (this.agent != null && this.agent.isOnNavMesh)
		{
			this.agent.SetDestination(position);
		}
	}

	// Token: 0x06001043 RID: 4163 RVA: 0x00046349 File Offset: 0x00044549
	protected static void InvokeUserCode_SetDestinationRpc__Vector3(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC SetDestinationRpc called on server.");
			return;
		}
		((NPC)obj).UserCode_SetDestinationRpc__Vector3(reader.ReadVector3());
	}

	// Token: 0x06001044 RID: 4164 RVA: 0x00046372 File Offset: 0x00044572
	protected void UserCode_WarpRpc__Vector3(Vector3 position)
	{
		if (base.isServer)
		{
			return;
		}
		if (this.agent != null)
		{
			this.agent.Warp(position);
		}
	}

	// Token: 0x06001045 RID: 4165 RVA: 0x00046398 File Offset: 0x00044598
	protected static void InvokeUserCode_WarpRpc__Vector3(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC WarpRpc called on server.");
			return;
		}
		((NPC)obj).UserCode_WarpRpc__Vector3(reader.ReadVector3());
	}

	// Token: 0x06001046 RID: 4166 RVA: 0x000463C1 File Offset: 0x000445C1
	protected void UserCode_ApplyKnockbackRpc__Vector3__Vector3(Vector3 force, Vector3 torque)
	{
		if (this._rb != null)
		{
			this._rb.AddForce(force, ForceMode.VelocityChange);
			this._rb.AddTorque(torque, ForceMode.VelocityChange);
		}
	}

	// Token: 0x06001047 RID: 4167 RVA: 0x000463EB File Offset: 0x000445EB
	protected static void InvokeUserCode_ApplyKnockbackRpc__Vector3__Vector3(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC ApplyKnockbackRpc called on server.");
			return;
		}
		((NPC)obj).UserCode_ApplyKnockbackRpc__Vector3__Vector3(reader.ReadVector3(), reader.ReadVector3());
	}

	// Token: 0x06001048 RID: 4168 RVA: 0x0004641C File Offset: 0x0004461C
	static NPC()
	{
		RemoteProcedureCalls.RegisterRpc(typeof(NPC), "System.Void NPC::SetDestinationRpc(UnityEngine.Vector3)", new RemoteCallDelegate(NPC.InvokeUserCode_SetDestinationRpc__Vector3));
		RemoteProcedureCalls.RegisterRpc(typeof(NPC), "System.Void NPC::WarpRpc(UnityEngine.Vector3)", new RemoteCallDelegate(NPC.InvokeUserCode_WarpRpc__Vector3));
		RemoteProcedureCalls.RegisterRpc(typeof(NPC), "System.Void NPC::ApplyKnockbackRpc(UnityEngine.Vector3,UnityEngine.Vector3)", new RemoteCallDelegate(NPC.InvokeUserCode_ApplyKnockbackRpc__Vector3__Vector3));
	}

	// Token: 0x06001049 RID: 4169 RVA: 0x0004648C File Offset: 0x0004468C
	public override void SerializeSyncVars(NetworkWriter writer, bool forceAll)
	{
		base.SerializeSyncVars(writer, forceAll);
		if (forceAll)
		{
			Mirror.GeneratedNetworkCode._Write_NPC/NPCState(writer, this._state);
			return;
		}
		writer.WriteVarULong(this.syncVarDirtyBits);
		if ((this.syncVarDirtyBits & 1UL) != 0UL)
		{
			Mirror.GeneratedNetworkCode._Write_NPC/NPCState(writer, this._state);
		}
	}

	// Token: 0x0600104A RID: 4170 RVA: 0x000464E4 File Offset: 0x000446E4
	public override void DeserializeSyncVars(NetworkReader reader, bool initialState)
	{
		base.DeserializeSyncVars(reader, initialState);
		if (initialState)
		{
			base.GeneratedSyncVarDeserialize<NPC.NPCState>(ref this._state, this._Mirror_SyncVarHookDelegate__state, Mirror.GeneratedNetworkCode._Read_NPC/NPCState(reader));
			return;
		}
		long num = (long)reader.ReadVarULong();
		if ((num & 1L) != 0L)
		{
			base.GeneratedSyncVarDeserialize<NPC.NPCState>(ref this._state, this._Mirror_SyncVarHookDelegate__state, Mirror.GeneratedNetworkCode._Read_NPC/NPCState(reader));
		}
	}

	// Token: 0x04000A6B RID: 2667
	[SyncVar(hook = "OnStateChanged")]
	private NPC.NPCState _state;

	// Token: 0x04000A6C RID: 2668
	[Header("References")]
	[SerializeField]
	private NavMeshAgent agent;

	// Token: 0x04000A6D RID: 2669
	[SerializeField]
	private Transform npcTransform;

	// Token: 0x04000A6E RID: 2670
	[SerializeField]
	private NPCEyes npcEyes;

	// Token: 0x04000A6F RID: 2671
	[Header("Behavior")]
	[SerializeField]
	private NPCBehavior behavior;

	// Token: 0x04000A70 RID: 2672
	[Header("Debug")]
	[SerializeField]
	[ReadOnly]
	private string _debugState;

	// Token: 0x04000A71 RID: 2673
	public RandomNPCSFX npcSfx;

	// Token: 0x04000A72 RID: 2674
	private Rigidbody _rb;

	// Token: 0x04000A73 RID: 2675
	private PlayerSettings _ps;

	// Token: 0x04000A74 RID: 2676
	private Coroutine _ragdollRoutine;

	// Token: 0x04000A75 RID: 2677
	public Action<NPC.NPCState, NPC.NPCState> _Mirror_SyncVarHookDelegate__state;

	// Token: 0x020001BF RID: 447
	public enum NPCState
	{
		// Token: 0x04000A77 RID: 2679
		Free,
		// Token: 0x04000A78 RID: 2680
		Ragdoll
	}
}
