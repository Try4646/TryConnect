using System;
using System.Collections;
using Extensions;
using Mirror;
using Mirror.RemoteCalls;
using UnityEngine;

// Token: 0x02000209 RID: 521
public class PlayerPingManager : NetworkBehaviour
{
	// Token: 0x06001347 RID: 4935 RVA: 0x0005356D File Offset: 0x0005176D
	private void Awake()
	{
		this._cam = MonoSingleton<LocalManager>.Instance.mainCamera;
	}

	// Token: 0x06001348 RID: 4936 RVA: 0x0005357F File Offset: 0x0005177F
	private void OnEnable()
	{
		InputEvents.OnPingEvent = (Action)Delegate.Combine(InputEvents.OnPingEvent, new Action(this.OnPing));
	}

	// Token: 0x06001349 RID: 4937 RVA: 0x000535A1 File Offset: 0x000517A1
	private void OnDisable()
	{
		InputEvents.OnPingEvent = (Action)Delegate.Remove(InputEvents.OnPingEvent, new Action(this.OnPing));
	}

	// Token: 0x0600134A RID: 4938 RVA: 0x0004CAE8 File Offset: 0x0004ACE8
	public override void OnStartClient()
	{
		base.OnStartClient();
		if (!base.isLocalPlayer)
		{
			base.enabled = false;
			return;
		}
	}

	// Token: 0x0600134B RID: 4939 RVA: 0x000535C4 File Offset: 0x000517C4
	private void OnPing()
	{
		int num = Physics.RaycastNonAlloc(this._cam.transform.position, this._cam.transform.forward, this._raycastHits, 100f, this.rayMask, QueryTriggerInteraction.Ignore);
		if (num == 0)
		{
			return;
		}
		RaycastHit raycastHit = this._raycastHits[0];
		for (int i = 1; i < num; i++)
		{
			if (this._raycastHits[i].distance < raycastHit.distance)
			{
				raycastHit = this._raycastHits[i];
			}
		}
		Vector3 position = raycastHit.point + raycastHit.normal * this.pingHeightOffset;
		Quaternion rotation = Quaternion.FromToRotation(Vector3.up, raycastHit.normal);
		ulong steamId = base.GetComponent<PlayerProfile>().steamId;
		this.CmdSpawnPing(position, rotation, steamId);
	}

	// Token: 0x0600134C RID: 4940 RVA: 0x000536A0 File Offset: 0x000518A0
	[Command]
	private void CmdSpawnPing(Vector3 position, Quaternion rotation, ulong steamId)
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		writer.WriteVector3(position);
		writer.WriteQuaternion(rotation);
		writer.WriteVarULong(steamId);
		base.SendCommandInternal("System.Void PlayerPingManager::CmdSpawnPing(UnityEngine.Vector3,UnityEngine.Quaternion,System.UInt64)", 1491304033, writer, 0, true);
		NetworkWriterPool.Return(writer);
	}

	// Token: 0x0600134D RID: 4941 RVA: 0x000536F0 File Offset: 0x000518F0
	[Server]
	private IEnumerator DestroyPingAfterDelay(GameObject pingObject, float delay)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Collections.IEnumerator PlayerPingManager::DestroyPingAfterDelay(UnityEngine.GameObject,System.Single)' called when server was not active");
			return null;
		}
		PlayerPingManager.<DestroyPingAfterDelay>d__12 <DestroyPingAfterDelay>d__ = new PlayerPingManager.<DestroyPingAfterDelay>d__12(0);
		<DestroyPingAfterDelay>d__.<>4__this = this;
		<DestroyPingAfterDelay>d__.pingObject = pingObject;
		<DestroyPingAfterDelay>d__.delay = delay;
		return <DestroyPingAfterDelay>d__;
	}

	// Token: 0x0600134E RID: 4942 RVA: 0x0005373C File Offset: 0x0005193C
	[ClientRpc]
	private void RpcSetPingSteamId(uint pingNetId, ulong steamId)
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		writer.WriteVarUInt(pingNetId);
		writer.WriteVarULong(steamId);
		this.SendRPCInternal("System.Void PlayerPingManager::RpcSetPingSteamId(System.UInt32,System.UInt64)", -1572483224, writer, 0, true);
		NetworkWriterPool.Return(writer);
	}

	// Token: 0x06001350 RID: 4944 RVA: 0x00002321 File Offset: 0x00000521
	public override bool Weaved()
	{
		return true;
	}

	// Token: 0x06001351 RID: 4945 RVA: 0x000537A0 File Offset: 0x000519A0
	protected void UserCode_CmdSpawnPing__Vector3__Quaternion__UInt64(Vector3 position, Quaternion rotation, ulong steamId)
	{
		NetworkIdentity networkIdentity;
		if (this._lastPingNetId != 0U && NetworkServer.spawned.TryGetValue(this._lastPingNetId, out networkIdentity))
		{
			NetworkServer.Destroy(networkIdentity.gameObject);
		}
		GameObject gameObject = Object.Instantiate<GameObject>(this.pingPrefab, position, rotation);
		NetworkServer.Spawn(gameObject, null);
		this._lastPingNetId = gameObject.GetComponent<NetworkIdentity>().netId;
		this.RpcSetPingSteamId(this._lastPingNetId, steamId);
		base.StartCoroutine(this.DestroyPingAfterDelay(gameObject, 3f));
	}

	// Token: 0x06001352 RID: 4946 RVA: 0x0005381A File Offset: 0x00051A1A
	protected static void InvokeUserCode_CmdSpawnPing__Vector3__Quaternion__UInt64(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdSpawnPing called on client.");
			return;
		}
		((PlayerPingManager)obj).UserCode_CmdSpawnPing__Vector3__Quaternion__UInt64(reader.ReadVector3(), reader.ReadQuaternion(), reader.ReadVarULong());
	}

	// Token: 0x06001353 RID: 4947 RVA: 0x00053850 File Offset: 0x00051A50
	protected void UserCode_RpcSetPingSteamId__UInt32__UInt64(uint pingNetId, ulong steamId)
	{
		NetworkIdentity networkIdentity;
		if (NetworkClient.spawned.TryGetValue(pingNetId, out networkIdentity))
		{
			SteamIdComponent component = networkIdentity.GetComponent<SteamIdComponent>();
			if (component != null)
			{
				component.SetSteamID(steamId);
			}
		}
	}

	// Token: 0x06001354 RID: 4948 RVA: 0x00053884 File Offset: 0x00051A84
	protected static void InvokeUserCode_RpcSetPingSteamId__UInt32__UInt64(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcSetPingSteamId called on server.");
			return;
		}
		((PlayerPingManager)obj).UserCode_RpcSetPingSteamId__UInt32__UInt64(reader.ReadVarUInt(), reader.ReadVarULong());
	}

	// Token: 0x06001355 RID: 4949 RVA: 0x000538B4 File Offset: 0x00051AB4
	static PlayerPingManager()
	{
		RemoteProcedureCalls.RegisterCommand(typeof(PlayerPingManager), "System.Void PlayerPingManager::CmdSpawnPing(UnityEngine.Vector3,UnityEngine.Quaternion,System.UInt64)", new RemoteCallDelegate(PlayerPingManager.InvokeUserCode_CmdSpawnPing__Vector3__Quaternion__UInt64), true);
		RemoteProcedureCalls.RegisterRpc(typeof(PlayerPingManager), "System.Void PlayerPingManager::RpcSetPingSteamId(System.UInt32,System.UInt64)", new RemoteCallDelegate(PlayerPingManager.InvokeUserCode_RpcSetPingSteamId__UInt32__UInt64));
	}

	// Token: 0x04000C4B RID: 3147
	[Header("Ping Settings")]
	[SerializeField]
	private GameObject pingPrefab;

	// Token: 0x04000C4C RID: 3148
	[SerializeField]
	private LayerMask rayMask;

	// Token: 0x04000C4D RID: 3149
	[SerializeField]
	private float pingHeightOffset = 0.1f;

	// Token: 0x04000C4E RID: 3150
	private readonly RaycastHit[] _raycastHits = new RaycastHit[8];

	// Token: 0x04000C4F RID: 3151
	private Camera _cam;

	// Token: 0x04000C50 RID: 3152
	private uint _lastPingNetId;
}
