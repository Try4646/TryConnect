using System;
using System.Collections;
using Extensions;
using Mirror;
using Mirror.RemoteCalls;
using Steamworks;
using UnityEngine;

// Token: 0x020002DB RID: 731
public class ReturnToMainMenu : NetworkBehaviour
{
	// Token: 0x0600199C RID: 6556 RVA: 0x0006B55D File Offset: 0x0006975D
	public void ClientReturnToMainMenu()
	{
		if (base.isClient && !base.isServer && NetworkClient.isConnected)
		{
			this.CmdDropHeldItemBeforeExit();
		}
		this.LeaveSteamLobbyIfInLobby();
		if (base.isServer)
		{
			this.RpcReturnToMainMenu();
		}
		base.StartCoroutine(this.ReturnToMainMenuCoroutine());
	}

	// Token: 0x0600199D RID: 6557 RVA: 0x0006B59D File Offset: 0x0006979D
	private IEnumerator ReturnToMainMenuCoroutine()
	{
		MonoSingleton<SceneTransitioner>.Instance.SetLoadingScreen(true, 0.5f, false);
		yield return new WaitForSeconds(1f);
		LobbySettings lobbySettings = Resources.Load<LobbySettings>("LobbySettings");
		if (base.isServer && SteamManager.Initialized && lobbySettings != null && lobbySettings.steamLobbyID != CSteamID.Nil)
		{
			CSteamID steamLobbyID = lobbySettings.steamLobbyID;
			if (SteamMatchmaking.GetLobbyOwner(steamLobbyID) == SteamUser.GetSteamID())
			{
				Debug.Log("Host quitting - setting GameStarted to false in Steam lobby");
				SteamMatchmaking.SetLobbyData(steamLobbyID, "GameStarted", "0");
			}
		}
		if (base.isServer)
		{
			NetworkManager.singleton.StopHost();
		}
		if (base.isClient)
		{
			NetworkManager.singleton.StopClient();
		}
		yield break;
	}

	// Token: 0x0600199E RID: 6558 RVA: 0x0006B5AC File Offset: 0x000697AC
	[ClientRpc]
	public void RpcReturnToMainMenu()
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		this.SendRPCInternal("System.Void ReturnToMainMenu::RpcReturnToMainMenu()", 1385993414, writer, 0, true);
		NetworkWriterPool.Return(writer);
	}

	// Token: 0x0600199F RID: 6559 RVA: 0x0006B5DC File Offset: 0x000697DC
	[Command]
	private void CmdDropHeldItemBeforeExit()
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		base.SendCommandInternal("System.Void ReturnToMainMenu::CmdDropHeldItemBeforeExit()", 455386482, writer, 0, true);
		NetworkWriterPool.Return(writer);
	}

	// Token: 0x060019A0 RID: 6560 RVA: 0x0006B60C File Offset: 0x0006980C
	private void LeaveSteamLobbyIfInLobby()
	{
		if (MonoSingleton<LobbyManager>.Instance == null)
		{
			return;
		}
		LobbySettings lobbySettings = Resources.Load<LobbySettings>("LobbySettings");
		if (lobbySettings != null && lobbySettings.inALobby && lobbySettings.steamLobbyID != CSteamID.Nil)
		{
			Debug.Log("Client quitting - leaving Steam lobby...");
			MonoSingleton<LobbyManager>.Instance.LeaveLobby();
		}
	}

	// Token: 0x060019A2 RID: 6562 RVA: 0x00002321 File Offset: 0x00000521
	public override bool Weaved()
	{
		return true;
	}

	// Token: 0x060019A3 RID: 6563 RVA: 0x0006B669 File Offset: 0x00069869
	protected void UserCode_RpcReturnToMainMenu()
	{
		this.LeaveSteamLobbyIfInLobby();
		base.StartCoroutine(this.ReturnToMainMenuCoroutine());
	}

	// Token: 0x060019A4 RID: 6564 RVA: 0x0006B67E File Offset: 0x0006987E
	protected static void InvokeUserCode_RpcReturnToMainMenu(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcReturnToMainMenu called on server.");
			return;
		}
		((ReturnToMainMenu)obj).UserCode_RpcReturnToMainMenu();
	}

	// Token: 0x060019A5 RID: 6565 RVA: 0x0006B6A4 File Offset: 0x000698A4
	protected void UserCode_CmdDropHeldItemBeforeExit()
	{
		NetworkConnectionToClient connectionToClient = base.connectionToClient;
		if (((connectionToClient != null) ? connectionToClient.identity : null) == null)
		{
			return;
		}
		PlayerCarry playerCarry;
		PlayerInventory playerInventory;
		if (base.connectionToClient.identity.TryGetComponent<PlayerCarry>(out playerCarry) && playerCarry.TryGetHolderInventory(out playerInventory))
		{
			playerInventory.ServerDropHoldingItem();
		}
		PlayerInventory playerInventory2;
		if (base.connectionToClient.identity.TryGetComponent<PlayerInventory>(out playerInventory2))
		{
			playerInventory2.ServerDropHoldingItem();
		}
	}

	// Token: 0x060019A6 RID: 6566 RVA: 0x0006B70C File Offset: 0x0006990C
	protected static void InvokeUserCode_CmdDropHeldItemBeforeExit(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdDropHeldItemBeforeExit called on client.");
			return;
		}
		((ReturnToMainMenu)obj).UserCode_CmdDropHeldItemBeforeExit();
	}

	// Token: 0x060019A7 RID: 6567 RVA: 0x0006B730 File Offset: 0x00069930
	static ReturnToMainMenu()
	{
		RemoteProcedureCalls.RegisterCommand(typeof(ReturnToMainMenu), "System.Void ReturnToMainMenu::CmdDropHeldItemBeforeExit()", new RemoteCallDelegate(ReturnToMainMenu.InvokeUserCode_CmdDropHeldItemBeforeExit), true);
		RemoteProcedureCalls.RegisterRpc(typeof(ReturnToMainMenu), "System.Void ReturnToMainMenu::RpcReturnToMainMenu()", new RemoteCallDelegate(ReturnToMainMenu.InvokeUserCode_RpcReturnToMainMenu));
	}
}
