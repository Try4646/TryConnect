using System;
using System.Collections;
using System.Runtime.InteropServices;
using FMODUnity;
using Mirror;
using Mirror.RemoteCalls;
using UnityEngine;

// Token: 0x020001A3 RID: 419
public class SpawnBoxPlayerRagdollTrigger : InteractableBase
{
	// Token: 0x17000155 RID: 341
	// (get) Token: 0x06000F61 RID: 3937 RVA: 0x000413A4 File Offset: 0x0003F5A4
	public bool IsBoxInteractable
	{
		get
		{
			return this._isInteractable;
		}
	}

	// Token: 0x17000156 RID: 342
	// (get) Token: 0x06000F62 RID: 3938 RVA: 0x000413AC File Offset: 0x0003F5AC
	public bool AreLidsOpen
	{
		get
		{
			return this._areLidsOpen;
		}
	}

	// Token: 0x06000F63 RID: 3939 RVA: 0x000413B4 File Offset: 0x0003F5B4
	private void OnBoxInteractableChanged(bool oldValue, bool newValue)
	{
		this.IsInteractable = newValue;
	}

	// Token: 0x06000F64 RID: 3940 RVA: 0x000413BD File Offset: 0x0003F5BD
	private void OnLidsToggle(bool oldValue, bool newValue)
	{
		this.animator.SetBool("OpenLids", newValue);
	}

	// Token: 0x06000F65 RID: 3941 RVA: 0x000413D0 File Offset: 0x0003F5D0
	[Server]
	public void AssignPlayer(PlayerController player)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void SpawnBoxPlayerRagdollTrigger::AssignPlayer(PlayerController)' called when server was not active");
			return;
		}
		this.Network_areLidsOpen = false;
		base.StartCoroutine(this.AssignPlayerRoutine(player));
	}

	// Token: 0x06000F66 RID: 3942 RVA: 0x000413FC File Offset: 0x0003F5FC
	private IEnumerator AssignPlayerRoutine(PlayerController player)
	{
		yield return new WaitForSeconds(this.setInteractableDelay);
		this.TargetDisableLids(player.connectionToClient);
		yield return new WaitForSeconds(0.5f);
		this.assignedPlayer = player;
		this.Network_isInteractable = true;
		yield break;
	}

	// Token: 0x06000F67 RID: 3943 RVA: 0x00041412 File Offset: 0x0003F612
	public override void ServerOnInteract(PlayerInteract playerInteract)
	{
		base.ServerOnInteract(playerInteract);
		this.WakePlayerUp();
	}

	// Token: 0x06000F68 RID: 3944 RVA: 0x00041424 File Offset: 0x0003F624
	[Server]
	private void WakePlayerUp()
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void SpawnBoxPlayerRagdollTrigger::WakePlayerUp()' called when server was not active");
			return;
		}
		if (!this.assignedPlayer)
		{
			return;
		}
		PlayerController playerController = this.assignedPlayer;
		this.assignedPlayer = null;
		this.Network_isInteractable = false;
		this.Network_areLidsOpen = true;
		this.RpcOnWakeUp();
		playerController.ServerWakeUp();
		this.PlayMusicStinger(playerController.connectionToClient);
	}

	// Token: 0x06000F69 RID: 3945 RVA: 0x00041488 File Offset: 0x0003F688
	[TargetRpc]
	private void TargetDisableLids(NetworkConnection conn)
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		this.SendTargetRPCInternal(conn, "System.Void SpawnBoxPlayerRagdollTrigger::TargetDisableLids(Mirror.NetworkConnection)", -1416889657, writer, 0);
		NetworkWriterPool.Return(writer);
	}

	// Token: 0x06000F6A RID: 3946 RVA: 0x000414B8 File Offset: 0x0003F6B8
	private void DisableLidColliders()
	{
		Collider[] array = this.lidColliders;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].isTrigger = true;
		}
	}

	// Token: 0x06000F6B RID: 3947 RVA: 0x000414E4 File Offset: 0x0003F6E4
	private void EnableLidColliders()
	{
		Collider[] array = this.lidColliders;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].isTrigger = false;
		}
	}

	// Token: 0x06000F6C RID: 3948 RVA: 0x00041510 File Offset: 0x0003F710
	[ClientRpc]
	private void RpcOnWakeUp()
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		this.SendRPCInternal("System.Void SpawnBoxPlayerRagdollTrigger::RpcOnWakeUp()", -587603264, writer, 0, true);
		NetworkWriterPool.Return(writer);
	}

	// Token: 0x06000F6D RID: 3949 RVA: 0x00041540 File Offset: 0x0003F740
	[TargetRpc]
	private void PlayMusicStinger(NetworkConnection conn)
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		this.SendTargetRPCInternal(conn, "System.Void SpawnBoxPlayerRagdollTrigger::PlayMusicStinger(Mirror.NetworkConnection)", 1929011759, writer, 0);
		NetworkWriterPool.Return(writer);
	}

	// Token: 0x06000F6E RID: 3950 RVA: 0x00041570 File Offset: 0x0003F770
	public SpawnBoxPlayerRagdollTrigger()
	{
		this._Mirror_SyncVarHookDelegate__isInteractable = new Action<bool, bool>(this.OnBoxInteractableChanged);
		this._Mirror_SyncVarHookDelegate__areLidsOpen = new Action<bool, bool>(this.OnLidsToggle);
	}

	// Token: 0x06000F6F RID: 3951 RVA: 0x00002321 File Offset: 0x00000521
	public override bool Weaved()
	{
		return true;
	}

	// Token: 0x17000157 RID: 343
	// (get) Token: 0x06000F70 RID: 3952 RVA: 0x000415A8 File Offset: 0x0003F7A8
	// (set) Token: 0x06000F71 RID: 3953 RVA: 0x000415BB File Offset: 0x0003F7BB
	public bool Network_isInteractable
	{
		get
		{
			return this._isInteractable;
		}
		[param: In]
		set
		{
			base.GeneratedSyncVarSetter<bool>(value, ref this._isInteractable, 1UL, this._Mirror_SyncVarHookDelegate__isInteractable);
		}
	}

	// Token: 0x17000158 RID: 344
	// (get) Token: 0x06000F72 RID: 3954 RVA: 0x000415DC File Offset: 0x0003F7DC
	// (set) Token: 0x06000F73 RID: 3955 RVA: 0x000415EF File Offset: 0x0003F7EF
	public bool Network_areLidsOpen
	{
		get
		{
			return this._areLidsOpen;
		}
		[param: In]
		set
		{
			base.GeneratedSyncVarSetter<bool>(value, ref this._areLidsOpen, 2UL, this._Mirror_SyncVarHookDelegate__areLidsOpen);
		}
	}

	// Token: 0x06000F74 RID: 3956 RVA: 0x0004160E File Offset: 0x0003F80E
	protected void UserCode_TargetDisableLids__NetworkConnection(NetworkConnection conn)
	{
		this.DisableLidColliders();
	}

	// Token: 0x06000F75 RID: 3957 RVA: 0x00041616 File Offset: 0x0003F816
	protected static void InvokeUserCode_TargetDisableLids__NetworkConnection(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("TargetRPC TargetDisableLids called on server.");
			return;
		}
		((SpawnBoxPlayerRagdollTrigger)obj).UserCode_TargetDisableLids__NetworkConnection(null);
	}

	// Token: 0x06000F76 RID: 3958 RVA: 0x0004163A File Offset: 0x0003F83A
	protected void UserCode_RpcOnWakeUp()
	{
		SFXManager.SFXOneShot(this.wakeUpSfx, base.transform.position);
	}

	// Token: 0x06000F77 RID: 3959 RVA: 0x00041652 File Offset: 0x0003F852
	protected static void InvokeUserCode_RpcOnWakeUp(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcOnWakeUp called on server.");
			return;
		}
		((SpawnBoxPlayerRagdollTrigger)obj).UserCode_RpcOnWakeUp();
	}

	// Token: 0x06000F78 RID: 3960 RVA: 0x00041678 File Offset: 0x0003F878
	protected void UserCode_PlayMusicStinger__NetworkConnection(NetworkConnection conn)
	{
		SFXManager.SFXOneShot(this.musicStinger, default(Vector3));
	}

	// Token: 0x06000F79 RID: 3961 RVA: 0x00041699 File Offset: 0x0003F899
	protected static void InvokeUserCode_PlayMusicStinger__NetworkConnection(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("TargetRPC PlayMusicStinger called on server.");
			return;
		}
		((SpawnBoxPlayerRagdollTrigger)obj).UserCode_PlayMusicStinger__NetworkConnection(null);
	}

	// Token: 0x06000F7A RID: 3962 RVA: 0x000416C0 File Offset: 0x0003F8C0
	static SpawnBoxPlayerRagdollTrigger()
	{
		RemoteProcedureCalls.RegisterRpc(typeof(SpawnBoxPlayerRagdollTrigger), "System.Void SpawnBoxPlayerRagdollTrigger::RpcOnWakeUp()", new RemoteCallDelegate(SpawnBoxPlayerRagdollTrigger.InvokeUserCode_RpcOnWakeUp));
		RemoteProcedureCalls.RegisterRpc(typeof(SpawnBoxPlayerRagdollTrigger), "System.Void SpawnBoxPlayerRagdollTrigger::TargetDisableLids(Mirror.NetworkConnection)", new RemoteCallDelegate(SpawnBoxPlayerRagdollTrigger.InvokeUserCode_TargetDisableLids__NetworkConnection));
		RemoteProcedureCalls.RegisterRpc(typeof(SpawnBoxPlayerRagdollTrigger), "System.Void SpawnBoxPlayerRagdollTrigger::PlayMusicStinger(Mirror.NetworkConnection)", new RemoteCallDelegate(SpawnBoxPlayerRagdollTrigger.InvokeUserCode_PlayMusicStinger__NetworkConnection));
	}

	// Token: 0x06000F7B RID: 3963 RVA: 0x00041730 File Offset: 0x0003F930
	public override void SerializeSyncVars(NetworkWriter writer, bool forceAll)
	{
		base.SerializeSyncVars(writer, forceAll);
		if (forceAll)
		{
			writer.WriteBool(this._isInteractable);
			writer.WriteBool(this._areLidsOpen);
			return;
		}
		writer.WriteVarULong(this.syncVarDirtyBits);
		if ((this.syncVarDirtyBits & 1UL) != 0UL)
		{
			writer.WriteBool(this._isInteractable);
		}
		if ((this.syncVarDirtyBits & 2UL) != 0UL)
		{
			writer.WriteBool(this._areLidsOpen);
		}
	}

	// Token: 0x06000F7C RID: 3964 RVA: 0x000417B8 File Offset: 0x0003F9B8
	public override void DeserializeSyncVars(NetworkReader reader, bool initialState)
	{
		base.DeserializeSyncVars(reader, initialState);
		if (initialState)
		{
			base.GeneratedSyncVarDeserialize<bool>(ref this._isInteractable, this._Mirror_SyncVarHookDelegate__isInteractable, reader.ReadBool());
			base.GeneratedSyncVarDeserialize<bool>(ref this._areLidsOpen, this._Mirror_SyncVarHookDelegate__areLidsOpen, reader.ReadBool());
			return;
		}
		long num = (long)reader.ReadVarULong();
		if ((num & 1L) != 0L)
		{
			base.GeneratedSyncVarDeserialize<bool>(ref this._isInteractable, this._Mirror_SyncVarHookDelegate__isInteractable, reader.ReadBool());
		}
		if ((num & 2L) != 0L)
		{
			base.GeneratedSyncVarDeserialize<bool>(ref this._areLidsOpen, this._Mirror_SyncVarHookDelegate__areLidsOpen, reader.ReadBool());
		}
	}

	// Token: 0x040009F1 RID: 2545
	[SerializeField]
	private float setInteractableDelay = 1f;

	// Token: 0x040009F2 RID: 2546
	[SerializeField]
	private Animator animator;

	// Token: 0x040009F3 RID: 2547
	[SerializeField]
	private EventReference wakeUpSfx;

	// Token: 0x040009F4 RID: 2548
	[SerializeField]
	private EventReference musicStinger;

	// Token: 0x040009F5 RID: 2549
	[SerializeField]
	private Collider[] lidColliders;

	// Token: 0x040009F6 RID: 2550
	[SerializeField]
	private PlayerController assignedPlayer;

	// Token: 0x040009F7 RID: 2551
	[SyncVar(hook = "OnBoxInteractableChanged")]
	private bool _isInteractable;

	// Token: 0x040009F8 RID: 2552
	[SyncVar(hook = "OnLidsToggle")]
	private bool _areLidsOpen;

	// Token: 0x040009F9 RID: 2553
	public Action<bool, bool> _Mirror_SyncVarHookDelegate__isInteractable;

	// Token: 0x040009FA RID: 2554
	public Action<bool, bool> _Mirror_SyncVarHookDelegate__areLidsOpen;
}
