using System;
using System.Collections;
using System.Collections.Generic;
using Extensions;
using Mirror;
using Mirror.RemoteCalls;
using Unity.Cinemachine;
using UnityEngine;

// Token: 0x020001B7 RID: 439
public class WinSceneManager : NetworkSingleton<WinSceneManager>
{
	// Token: 0x06000FDA RID: 4058 RVA: 0x00043A08 File Offset: 0x00041C08
	[Server]
	public void ServerInitializePlayCoinFlip()
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void WinSceneManager::ServerInitializePlayCoinFlip()' called when server was not active");
			return;
		}
		if (this._isChoiceMade)
		{
			return;
		}
		this._isChoiceMade = true;
		NetworkSingleton<GameUI>.Instance.ServerToggleMoneyUI(false, true);
		NetworkSingleton<GameUI>.Instance.ServerToggleCrosshair(false);
		base.StartCoroutine(this.PlayCoinFlipRoutine());
	}

	// Token: 0x06000FDB RID: 4059 RVA: 0x00043A5E File Offset: 0x00041C5E
	private IEnumerator PlayCoinFlipRoutine()
	{
		this.RpcFadeCamera(true);
		yield return new WaitForSeconds(1f);
		NetworkSingleton<OrganManager>.Instance.ServerResetAllOrgansToDefaults();
		this.ServerLockDebtBags();
		yield return null;
		this.ServerTeleportDebtBags(true);
		this.RpcSetCameraLayer();
		this.RpcLockPlayerInputs();
		for (int i = 0; i < MonoSingleton<LocalManager>.Instance.players.Count; i++)
		{
			MonoSingleton<LocalManager>.Instance.players[i].controller.ServerLock(true);
			MonoSingleton<LocalManager>.Instance.players[i].controller.ServerLockHead(true);
			MonoSingleton<LocalManager>.Instance.players[i].controller.ServerTeleport(this.playerPositionsCoinFlip[i].position);
			MonoSingleton<LocalManager>.Instance.players[i].controller.ServerRotate(new Vector2(this.playerPositionsCoinFlip[i].eulerAngles.y, 0f));
		}
		this.RpcChangeCameraCoinFlip();
		this.RpcFadeCamera(false);
		yield return new WaitForSeconds(1f);
		this.startCoinflipSFX.RpcLoopSFX(true);
		this.coinFlip.ServerPlayCoinFlip();
		yield break;
	}

	// Token: 0x06000FDC RID: 4060 RVA: 0x00043A70 File Offset: 0x00041C70
	[ClientRpc]
	private void RpcChangeCameraCoinFlip()
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		this.SendRPCInternal("System.Void WinSceneManager::RpcChangeCameraCoinFlip()", 879864906, writer, 0, true);
		NetworkWriterPool.Return(writer);
	}

	// Token: 0x06000FDD RID: 4061 RVA: 0x00043AA0 File Offset: 0x00041CA0
	[Server]
	public void ServerConcludeCoinFlip(bool isWin)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void WinSceneManager::ServerConcludeCoinFlip(System.Boolean)' called when server was not active");
			return;
		}
		base.StartCoroutine(this.ConcludeCoinFlipRoutine(isWin));
		this.RpcSetLoopIsWin(isWin);
		this.startCoinflipSFX.RpcLoopSFX(false);
	}

	// Token: 0x06000FDE RID: 4062 RVA: 0x00043AD8 File Offset: 0x00041CD8
	private IEnumerator ConcludeCoinFlipRoutine(bool isWin)
	{
		if (!isWin)
		{
			this.moneyPipeCoinFlip.ServerStartSucking();
			yield return new WaitForSeconds(2f);
		}
		else
		{
			yield return new WaitForSeconds(1f);
		}
		this.RpcFadeCamera(false);
		yield return new WaitForSeconds(1f);
		NetworkSingleton<GameManager>.Instance.ServerSetCutscene(isWin ? 0 : 1);
		yield break;
	}

	// Token: 0x06000FDF RID: 4063 RVA: 0x00043AEE File Offset: 0x00041CEE
	[Server]
	public void ServerInitializePayDebt()
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void WinSceneManager::ServerInitializePayDebt()' called when server was not active");
			return;
		}
		if (this._isChoiceMade)
		{
			return;
		}
		this._isChoiceMade = true;
		base.StartCoroutine(this.PayDebtRoutine());
	}

	// Token: 0x06000FE0 RID: 4064 RVA: 0x00043B22 File Offset: 0x00041D22
	private IEnumerator PayDebtRoutine()
	{
		this.RpcFadeCamera(true);
		yield return new WaitForSeconds(1f);
		NetworkSingleton<OrganManager>.Instance.ServerResetAllOrgansToDefaults();
		this.ServerLockDebtBags();
		yield return null;
		this.ServerTeleportDebtBags(false);
		this.RpcSetCameraLayer();
		this.RpcLockPlayerInputs();
		for (int i = 0; i < MonoSingleton<LocalManager>.Instance.players.Count; i++)
		{
			MonoSingleton<LocalManager>.Instance.players[i].controller.ServerLock(true);
			MonoSingleton<LocalManager>.Instance.players[i].controller.ServerLockHead(true);
			MonoSingleton<LocalManager>.Instance.players[i].controller.ServerTeleport(this.playerPositionsPayDebt[i].position);
			MonoSingleton<LocalManager>.Instance.players[i].controller.ServerRotate(new Vector2(this.playerPositionsPayDebt[i].eulerAngles.y, 0f));
		}
		this.RpcChangeCameraPayDebt();
		this.RpcFadeCamera(false);
		yield return new WaitForSeconds(1f);
		this.startPayDebtSFX.RpcPlayOneShotWith3DPos();
		this.moneyPipePayDebt.ServerStartSucking();
		yield return new WaitForSeconds(2f);
		this.RpcFadeCamera(true);
		yield return new WaitForSeconds(1f);
		NetworkSingleton<GameManager>.Instance.ServerSetCutscene(2);
		yield break;
	}

	// Token: 0x06000FE1 RID: 4065 RVA: 0x00043B34 File Offset: 0x00041D34
	[ClientRpc]
	private void RpcChangeCameraPayDebt()
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		this.SendRPCInternal("System.Void WinSceneManager::RpcChangeCameraPayDebt()", 1449297785, writer, 0, true);
		NetworkWriterPool.Return(writer);
	}

	// Token: 0x06000FE2 RID: 4066 RVA: 0x00043B64 File Offset: 0x00041D64
	[Server]
	private void ServerLockDebtBags()
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void WinSceneManager::ServerLockDebtBags()' called when server was not active");
			return;
		}
		foreach (DebtBag debtBag in this.debtBags)
		{
			debtBag.ServerLock();
		}
	}

	// Token: 0x06000FE3 RID: 4067 RVA: 0x00043BCC File Offset: 0x00041DCC
	[Server]
	private void ServerTeleportDebtBags(bool isCoinFlip)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void WinSceneManager::ServerTeleportDebtBags(System.Boolean)' called when server was not active");
			return;
		}
		for (int i = 0; i < this.debtBags.Count; i++)
		{
			DebtBag debtBag = this.debtBags[i];
			Transform transform = isCoinFlip ? this.debtBagPositionsCoinFlip[i] : this.debtBagPositionsPayDebt[i];
			debtBag.ServerTeleport(transform.position);
			debtBag.ServerRotate(transform.rotation);
		}
	}

	// Token: 0x06000FE4 RID: 4068 RVA: 0x00043C40 File Offset: 0x00041E40
	[ClientRpc]
	private void RpcFadeCamera(bool isEnabled)
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		writer.WriteBool(isEnabled);
		this.SendRPCInternal("System.Void WinSceneManager::RpcFadeCamera(System.Boolean)", -1034923765, writer, 0, true);
		NetworkWriterPool.Return(writer);
	}

	// Token: 0x06000FE5 RID: 4069 RVA: 0x00043C7C File Offset: 0x00041E7C
	[ClientRpc]
	private void RpcLockPlayerInputs()
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		this.SendRPCInternal("System.Void WinSceneManager::RpcLockPlayerInputs()", 256121420, writer, 0, true);
		NetworkWriterPool.Return(writer);
	}

	// Token: 0x06000FE6 RID: 4070 RVA: 0x00043CAC File Offset: 0x00041EAC
	[ClientRpc]
	private void RpcSetCameraLayer()
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		this.SendRPCInternal("System.Void WinSceneManager::RpcSetCameraLayer()", 466922551, writer, 0, true);
		NetworkWriterPool.Return(writer);
	}

	// Token: 0x06000FE7 RID: 4071 RVA: 0x00043CDC File Offset: 0x00041EDC
	[ClientRpc]
	private void RpcSetLoopIsWin(bool isWin)
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		writer.WriteBool(isWin);
		this.SendRPCInternal("System.Void WinSceneManager::RpcSetLoopIsWin(System.Boolean)", -2093602558, writer, 0, true);
		NetworkWriterPool.Return(writer);
	}

	// Token: 0x06000FE9 RID: 4073 RVA: 0x00002321 File Offset: 0x00000521
	public override bool Weaved()
	{
		return true;
	}

	// Token: 0x06000FEA RID: 4074 RVA: 0x00043D29 File Offset: 0x00041F29
	protected void UserCode_RpcChangeCameraCoinFlip()
	{
		this.cameraParentCoinFlip.enabled = true;
	}

	// Token: 0x06000FEB RID: 4075 RVA: 0x00043D37 File Offset: 0x00041F37
	protected static void InvokeUserCode_RpcChangeCameraCoinFlip(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcChangeCameraCoinFlip called on server.");
			return;
		}
		((WinSceneManager)obj).UserCode_RpcChangeCameraCoinFlip();
	}

	// Token: 0x06000FEC RID: 4076 RVA: 0x00043D5A File Offset: 0x00041F5A
	protected void UserCode_RpcChangeCameraPayDebt()
	{
		this.cameraParentPayDebt.enabled = true;
	}

	// Token: 0x06000FED RID: 4077 RVA: 0x00043D68 File Offset: 0x00041F68
	protected static void InvokeUserCode_RpcChangeCameraPayDebt(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcChangeCameraPayDebt called on server.");
			return;
		}
		((WinSceneManager)obj).UserCode_RpcChangeCameraPayDebt();
	}

	// Token: 0x06000FEE RID: 4078 RVA: 0x00043D8B File Offset: 0x00041F8B
	protected void UserCode_RpcFadeCamera__Boolean(bool isEnabled)
	{
		MonoSingleton<SceneTransitioner>.Instance.SetLoadingScreen(isEnabled, 1f, false);
	}

	// Token: 0x06000FEF RID: 4079 RVA: 0x00043D9E File Offset: 0x00041F9E
	protected static void InvokeUserCode_RpcFadeCamera__Boolean(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcFadeCamera called on server.");
			return;
		}
		((WinSceneManager)obj).UserCode_RpcFadeCamera__Boolean(reader.ReadBool());
	}

	// Token: 0x06000FF0 RID: 4080 RVA: 0x00037D62 File Offset: 0x00035F62
	protected void UserCode_RpcLockPlayerInputs()
	{
		InputEvents.ActiveLayer = InputLayer.Cutscene;
	}

	// Token: 0x06000FF1 RID: 4081 RVA: 0x00043DC7 File Offset: 0x00041FC7
	protected static void InvokeUserCode_RpcLockPlayerInputs(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcLockPlayerInputs called on server.");
			return;
		}
		((WinSceneManager)obj).UserCode_RpcLockPlayerInputs();
	}

	// Token: 0x06000FF2 RID: 4082 RVA: 0x00043DEA File Offset: 0x00041FEA
	protected void UserCode_RpcSetCameraLayer()
	{
		MonoSingleton<LocalManager>.Instance.mainCamera.cullingMask |= 1 << LayerMask.NameToLayer("SelfMeshPlayer");
	}

	// Token: 0x06000FF3 RID: 4083 RVA: 0x00043E11 File Offset: 0x00042011
	protected static void InvokeUserCode_RpcSetCameraLayer(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcSetCameraLayer called on server.");
			return;
		}
		((WinSceneManager)obj).UserCode_RpcSetCameraLayer();
	}

	// Token: 0x06000FF4 RID: 4084 RVA: 0x00043E34 File Offset: 0x00042034
	protected void UserCode_RpcSetLoopIsWin__Boolean(bool isWin)
	{
		this.startCoinflipSFX.loopInstance.setParameterByName("IsWin", (float)(isWin ? 1 : 0), false);
	}

	// Token: 0x06000FF5 RID: 4085 RVA: 0x00043E55 File Offset: 0x00042055
	protected static void InvokeUserCode_RpcSetLoopIsWin__Boolean(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcSetLoopIsWin called on server.");
			return;
		}
		((WinSceneManager)obj).UserCode_RpcSetLoopIsWin__Boolean(reader.ReadBool());
	}

	// Token: 0x06000FF6 RID: 4086 RVA: 0x00043E80 File Offset: 0x00042080
	static WinSceneManager()
	{
		RemoteProcedureCalls.RegisterRpc(typeof(WinSceneManager), "System.Void WinSceneManager::RpcChangeCameraCoinFlip()", new RemoteCallDelegate(WinSceneManager.InvokeUserCode_RpcChangeCameraCoinFlip));
		RemoteProcedureCalls.RegisterRpc(typeof(WinSceneManager), "System.Void WinSceneManager::RpcChangeCameraPayDebt()", new RemoteCallDelegate(WinSceneManager.InvokeUserCode_RpcChangeCameraPayDebt));
		RemoteProcedureCalls.RegisterRpc(typeof(WinSceneManager), "System.Void WinSceneManager::RpcFadeCamera(System.Boolean)", new RemoteCallDelegate(WinSceneManager.InvokeUserCode_RpcFadeCamera__Boolean));
		RemoteProcedureCalls.RegisterRpc(typeof(WinSceneManager), "System.Void WinSceneManager::RpcLockPlayerInputs()", new RemoteCallDelegate(WinSceneManager.InvokeUserCode_RpcLockPlayerInputs));
		RemoteProcedureCalls.RegisterRpc(typeof(WinSceneManager), "System.Void WinSceneManager::RpcSetCameraLayer()", new RemoteCallDelegate(WinSceneManager.InvokeUserCode_RpcSetCameraLayer));
		RemoteProcedureCalls.RegisterRpc(typeof(WinSceneManager), "System.Void WinSceneManager::RpcSetLoopIsWin(System.Boolean)", new RemoteCallDelegate(WinSceneManager.InvokeUserCode_RpcSetLoopIsWin__Boolean));
	}

	// Token: 0x04000A48 RID: 2632
	public List<DebtBag> debtBags = new List<DebtBag>();

	// Token: 0x04000A49 RID: 2633
	[Header("CoinFlip")]
	[SerializeField]
	private CoinFlip coinFlip;

	// Token: 0x04000A4A RID: 2634
	[SerializeField]
	private Transform[] playerPositionsCoinFlip;

	// Token: 0x04000A4B RID: 2635
	[SerializeField]
	private Transform[] debtBagPositionsCoinFlip;

	// Token: 0x04000A4C RID: 2636
	[SerializeField]
	private CinemachineCamera cameraParentCoinFlip;

	// Token: 0x04000A4D RID: 2637
	[SerializeField]
	private MoneyPipe moneyPipeCoinFlip;

	// Token: 0x04000A4E RID: 2638
	[Header("PayDebt")]
	[SerializeField]
	private Transform[] playerPositionsPayDebt;

	// Token: 0x04000A4F RID: 2639
	[SerializeField]
	private Transform[] debtBagPositionsPayDebt;

	// Token: 0x04000A50 RID: 2640
	[SerializeField]
	private CinemachineCamera cameraParentPayDebt;

	// Token: 0x04000A51 RID: 2641
	[SerializeField]
	private MoneyPipe moneyPipePayDebt;

	// Token: 0x04000A52 RID: 2642
	[Header("SFX")]
	[SerializeField]
	private SFXLoopComponent startCoinflipSFX;

	// Token: 0x04000A53 RID: 2643
	[SerializeField]
	private SFXComponent startPayDebtSFX;

	// Token: 0x04000A54 RID: 2644
	private bool _isChoiceMade;
}
