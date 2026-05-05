using System;
using Mirror;
using Mirror.RemoteCalls;
using Smooth;
using UnityEngine;

// Token: 0x0200034F RID: 847
public class ServerAuthorityExamplePlayerController : NetworkBehaviour
{
	// Token: 0x06001BCF RID: 7119 RVA: 0x000777B6 File Offset: 0x000759B6
	private void Awake()
	{
		this.rb = base.GetComponent<Rigidbody>();
		this.smoothSync = base.GetComponent<SmoothSyncMirror>();
	}

	// Token: 0x06001BD0 RID: 7120 RVA: 0x000777D0 File Offset: 0x000759D0
	public override void OnStartServer()
	{
		this.rb.isKinematic = false;
		base.OnStartServer();
	}

	// Token: 0x06001BD1 RID: 7121 RVA: 0x000777E4 File Offset: 0x000759E4
	private void Update()
	{
		if (base.isOwned)
		{
			if (Input.GetKeyUp(KeyCode.DownArrow))
			{
				this.CmdMove(KeyCode.DownArrow);
			}
			if (Input.GetKeyUp(KeyCode.UpArrow))
			{
				this.CmdMove(KeyCode.UpArrow);
			}
			if (Input.GetKeyUp(KeyCode.LeftArrow))
			{
				this.CmdMove(KeyCode.LeftArrow);
			}
			if (Input.GetKeyUp(KeyCode.RightArrow))
			{
				this.CmdMove(KeyCode.RightArrow);
			}
			if (Input.GetKeyUp(KeyCode.T))
			{
				this.CmdTeleport();
			}
		}
	}

	// Token: 0x06001BD2 RID: 7122 RVA: 0x00077864 File Offset: 0x00075A64
	[Command]
	private void CmdTeleport()
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		base.SendCommandInternal("System.Void ServerAuthorityExamplePlayerController::CmdTeleport()", 55251365, writer, 0, true);
		NetworkWriterPool.Return(writer);
	}

	// Token: 0x06001BD3 RID: 7123 RVA: 0x00077894 File Offset: 0x00075A94
	[Command]
	private void CmdMove(KeyCode keyCode)
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		Mirror.GeneratedNetworkCode._Write_UnityEngine.KeyCode(writer, keyCode);
		base.SendCommandInternal("System.Void ServerAuthorityExamplePlayerController::CmdMove(UnityEngine.KeyCode)", -953675450, writer, 0, true);
		NetworkWriterPool.Return(writer);
	}

	// Token: 0x06001BD5 RID: 7125 RVA: 0x00002321 File Offset: 0x00000521
	public override bool Weaved()
	{
		return true;
	}

	// Token: 0x06001BD6 RID: 7126 RVA: 0x000778EC File Offset: 0x00075AEC
	protected void UserCode_CmdTeleport()
	{
		this.smoothSync.teleportAnyObjectFromServer(base.transform.position + Vector3.right * 5f, base.transform.rotation, base.transform.localScale);
	}

	// Token: 0x06001BD7 RID: 7127 RVA: 0x00077939 File Offset: 0x00075B39
	protected static void InvokeUserCode_CmdTeleport(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdTeleport called on client.");
			return;
		}
		((ServerAuthorityExamplePlayerController)obj).UserCode_CmdTeleport();
	}

	// Token: 0x06001BD8 RID: 7128 RVA: 0x0007795C File Offset: 0x00075B5C
	protected void UserCode_CmdMove__KeyCode(KeyCode keyCode)
	{
		switch (keyCode)
		{
		case KeyCode.UpArrow:
			this.rb.AddForce(new Vector3(0f, 1.5f, 1f) * this.rigidbodyMovementForce);
			return;
		case KeyCode.DownArrow:
			this.rb.AddForce(new Vector3(0f, -1.5f, -1f) * this.rigidbodyMovementForce);
			return;
		case KeyCode.RightArrow:
			this.rb.AddForce(new Vector3(1f, 0f, 0f) * this.rigidbodyMovementForce);
			return;
		case KeyCode.LeftArrow:
			this.rb.AddForce(new Vector3(-1f, 0f, 0f) * this.rigidbodyMovementForce);
			return;
		default:
			return;
		}
	}

	// Token: 0x06001BD9 RID: 7129 RVA: 0x00077A31 File Offset: 0x00075C31
	protected static void InvokeUserCode_CmdMove__KeyCode(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdMove called on client.");
			return;
		}
		((ServerAuthorityExamplePlayerController)obj).UserCode_CmdMove__KeyCode(Mirror.GeneratedNetworkCode._Read_UnityEngine.KeyCode(reader));
	}

	// Token: 0x06001BDA RID: 7130 RVA: 0x00077A5C File Offset: 0x00075C5C
	static ServerAuthorityExamplePlayerController()
	{
		RemoteProcedureCalls.RegisterCommand(typeof(ServerAuthorityExamplePlayerController), "System.Void ServerAuthorityExamplePlayerController::CmdTeleport()", new RemoteCallDelegate(ServerAuthorityExamplePlayerController.InvokeUserCode_CmdTeleport), true);
		RemoteProcedureCalls.RegisterCommand(typeof(ServerAuthorityExamplePlayerController), "System.Void ServerAuthorityExamplePlayerController::CmdMove(UnityEngine.KeyCode)", new RemoteCallDelegate(ServerAuthorityExamplePlayerController.InvokeUserCode_CmdMove__KeyCode), true);
	}

	// Token: 0x040012A1 RID: 4769
	private Rigidbody rb;

	// Token: 0x040012A2 RID: 4770
	public float transformMovementSpeed = 30f;

	// Token: 0x040012A3 RID: 4771
	public float rigidbodyMovementForce = 500f;

	// Token: 0x040012A4 RID: 4772
	private SmoothSyncMirror smoothSync;
}
