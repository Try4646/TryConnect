using System;
using Mirror;
using Mirror.RemoteCalls;
using UnityEngine;

// Token: 0x02000292 RID: 658
public class CasinoBuilding : NetworkBehaviour
{
	// Token: 0x06001765 RID: 5989 RVA: 0x00062F94 File Offset: 0x00061194
	[ClientRpc]
	public void RpcCloseTheGate()
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		this.SendRPCInternal("System.Void CasinoBuilding::RpcCloseTheGate()", -891921159, writer, 0, true);
		NetworkWriterPool.Return(writer);
	}

	// Token: 0x06001766 RID: 5990 RVA: 0x00062FC4 File Offset: 0x000611C4
	[Server]
	public void ServerSpawnGoToHomeVehicle()
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void CasinoBuilding::ServerSpawnGoToHomeVehicle()' called when server was not active");
			return;
		}
		if (!this.vehicle)
		{
			this.vehicle = Object.FindFirstObjectByType<VehicleDoors>(FindObjectsInactive.Include);
		}
		this.vehicle.ServerOpenDoors();
	}

	// Token: 0x06001768 RID: 5992 RVA: 0x00002321 File Offset: 0x00000521
	public override bool Weaved()
	{
		return true;
	}

	// Token: 0x06001769 RID: 5993 RVA: 0x000048A7 File Offset: 0x00002AA7
	protected void UserCode_RpcCloseTheGate()
	{
	}

	// Token: 0x0600176A RID: 5994 RVA: 0x00062FFF File Offset: 0x000611FF
	protected static void InvokeUserCode_RpcCloseTheGate(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcCloseTheGate called on server.");
			return;
		}
		((CasinoBuilding)obj).UserCode_RpcCloseTheGate();
	}

	// Token: 0x0600176B RID: 5995 RVA: 0x00063022 File Offset: 0x00061222
	static CasinoBuilding()
	{
		RemoteProcedureCalls.RegisterRpc(typeof(CasinoBuilding), "System.Void CasinoBuilding::RpcCloseTheGate()", new RemoteCallDelegate(CasinoBuilding.InvokeUserCode_RpcCloseTheGate));
	}

	// Token: 0x04000F2D RID: 3885
	[SerializeField]
	private VehicleDoors vehicle;
}
