using System;
using Extensions;
using Mirror;
using Mirror.RemoteCalls;
using Unity.AI.Navigation;
using UnityEngine;

// Token: 0x0200018B RID: 395
public class NavMeshManager : NetworkSingleton<NavMeshManager>
{
	// Token: 0x06000ECA RID: 3786 RVA: 0x0003D5C6 File Offset: 0x0003B7C6
	[Server]
	public void InitializeNavMesh()
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void NavMeshManager::InitializeNavMesh()' called when server was not active");
			return;
		}
		this.RpcInitializeNavMesh();
	}

	// Token: 0x06000ECB RID: 3787 RVA: 0x0003D5E4 File Offset: 0x0003B7E4
	[ClientRpc]
	public void RpcInitializeNavMesh()
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		this.SendRPCInternal("System.Void NavMeshManager::RpcInitializeNavMesh()", -1113796349, writer, 0, true);
		NetworkWriterPool.Return(writer);
	}

	// Token: 0x06000ECC RID: 3788 RVA: 0x0003D614 File Offset: 0x0003B814
	[Server]
	public void ClearNavMesh()
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void NavMeshManager::ClearNavMesh()' called when server was not active");
			return;
		}
		this.RpcClearNavMesh();
	}

	// Token: 0x06000ECD RID: 3789 RVA: 0x0003D634 File Offset: 0x0003B834
	[ClientRpc]
	public void RpcClearNavMesh()
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		this.SendRPCInternal("System.Void NavMeshManager::RpcClearNavMesh()", -110519022, writer, 0, true);
		NetworkWriterPool.Return(writer);
	}

	// Token: 0x06000ECF RID: 3791 RVA: 0x00002321 File Offset: 0x00000521
	public override bool Weaved()
	{
		return true;
	}

	// Token: 0x06000ED0 RID: 3792 RVA: 0x0003D66C File Offset: 0x0003B86C
	protected void UserCode_RpcInitializeNavMesh()
	{
		NavMeshSurface component = base.GetComponent<NavMeshSurface>();
		component.RemoveData();
		component.BuildNavMesh();
	}

	// Token: 0x06000ED1 RID: 3793 RVA: 0x0003D67F File Offset: 0x0003B87F
	protected static void InvokeUserCode_RpcInitializeNavMesh(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcInitializeNavMesh called on server.");
			return;
		}
		((NavMeshManager)obj).UserCode_RpcInitializeNavMesh();
	}

	// Token: 0x06000ED2 RID: 3794 RVA: 0x0003D6A4 File Offset: 0x0003B8A4
	protected void UserCode_RpcClearNavMesh()
	{
		NavMeshSurface component = base.GetComponent<NavMeshSurface>();
		if (component.navMeshData)
		{
			component.RemoveData();
		}
	}

	// Token: 0x06000ED3 RID: 3795 RVA: 0x0003D6CB File Offset: 0x0003B8CB
	protected static void InvokeUserCode_RpcClearNavMesh(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcClearNavMesh called on server.");
			return;
		}
		((NavMeshManager)obj).UserCode_RpcClearNavMesh();
	}

	// Token: 0x06000ED4 RID: 3796 RVA: 0x0003D6F0 File Offset: 0x0003B8F0
	static NavMeshManager()
	{
		RemoteProcedureCalls.RegisterRpc(typeof(NavMeshManager), "System.Void NavMeshManager::RpcInitializeNavMesh()", new RemoteCallDelegate(NavMeshManager.InvokeUserCode_RpcInitializeNavMesh));
		RemoteProcedureCalls.RegisterRpc(typeof(NavMeshManager), "System.Void NavMeshManager::RpcClearNavMesh()", new RemoteCallDelegate(NavMeshManager.InvokeUserCode_RpcClearNavMesh));
	}
}
