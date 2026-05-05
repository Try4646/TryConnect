using System;
using Extensions;
using FMODUnity;
using Mirror;
using Mirror.RemoteCalls;
using TMPro;
using UnityEngine;

// Token: 0x02000168 RID: 360
public class GameUI : NetworkSingleton<GameUI>
{
	// Token: 0x06000DD3 RID: 3539 RVA: 0x00039A22 File Offset: 0x00037C22
	private void Start()
	{
		this.SetFloorText(NetworkSingleton<GameManager>.Instance.currentFloor);
	}

	// Token: 0x06000DD4 RID: 3540 RVA: 0x00039A34 File Offset: 0x00037C34
	[Server]
	public void ServerSetLoadingScreen(bool isEnabled, float duration, bool loadingScreen = true)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void GameUI::ServerSetLoadingScreen(System.Boolean,System.Single,System.Boolean)' called when server was not active");
			return;
		}
		this.RpcSetLoadingScreen(isEnabled, duration, loadingScreen);
	}

	// Token: 0x06000DD5 RID: 3541 RVA: 0x00039A54 File Offset: 0x00037C54
	[ClientRpc]
	private void RpcSetLoadingScreen(bool isEnabled, float duration, bool loadingScreen)
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		writer.WriteBool(isEnabled);
		writer.WriteFloat(duration);
		writer.WriteBool(loadingScreen);
		this.SendRPCInternal("System.Void GameUI::RpcSetLoadingScreen(System.Boolean,System.Single,System.Boolean)", -1385870491, writer, 0, true);
		NetworkWriterPool.Return(writer);
	}

	// Token: 0x06000DD6 RID: 3542 RVA: 0x00039AA2 File Offset: 0x00037CA2
	public void SetDaysText(int days)
	{
		this.dayText.text = string.Format("DAY {0}", days);
	}

	// Token: 0x06000DD7 RID: 3543 RVA: 0x00039ABF File Offset: 0x00037CBF
	public void SetFloorText(int floor)
	{
		this.floorText.text = string.Format("FLOOR {0}", floor + 1);
	}

	// Token: 0x06000DD8 RID: 3544 RVA: 0x00039AE0 File Offset: 0x00037CE0
	public void SetTimerText(float time)
	{
		int num = Mathf.FloorToInt(time / 60f);
		int num2 = Mathf.FloorToInt(time % 60f);
		this.timerText.text = string.Format("{0:#0}:{1:00}", num, num2);
		this.timerText.color = ((time > 30f) ? Color.white : new Color(0.925f, 0.18f, 0.247f));
		RuntimeManager.StudioSystem.setParameterByName("StressMode", (float)((time > 30f) ? 0 : 1), false);
	}

	// Token: 0x06000DD9 RID: 3545 RVA: 0x00039B77 File Offset: 0x00037D77
	[Server]
	public void ServerToggleTimer(bool isEnabled)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void GameUI::ServerToggleTimer(System.Boolean)' called when server was not active");
			return;
		}
		this.RpcToggleTimer(isEnabled);
	}

	// Token: 0x06000DDA RID: 3546 RVA: 0x00039B98 File Offset: 0x00037D98
	[ClientRpc]
	private void RpcToggleTimer(bool isEnabled)
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		writer.WriteBool(isEnabled);
		this.SendRPCInternal("System.Void GameUI::RpcToggleTimer(System.Boolean)", -1414466594, writer, 0, true);
		NetworkWriterPool.Return(writer);
	}

	// Token: 0x06000DDB RID: 3547 RVA: 0x00039BD2 File Offset: 0x00037DD2
	private void ToggleTimer(bool isEnabled)
	{
		this.timerHolder.gameObject.SetActive(isEnabled);
	}

	// Token: 0x06000DDC RID: 3548 RVA: 0x00039BE5 File Offset: 0x00037DE5
	[Server]
	public void ServerToggleMoneyUI(bool isEnabled, bool showCont = true)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void GameUI::ServerToggleMoneyUI(System.Boolean,System.Boolean)' called when server was not active");
			return;
		}
		this.RpcToggleMoneyUI(isEnabled, showCont);
	}

	// Token: 0x06000DDD RID: 3549 RVA: 0x00039C04 File Offset: 0x00037E04
	[ClientRpc]
	private void RpcToggleMoneyUI(bool isEnabled, bool showCont)
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		writer.WriteBool(isEnabled);
		writer.WriteBool(showCont);
		this.SendRPCInternal("System.Void GameUI::RpcToggleMoneyUI(System.Boolean,System.Boolean)", -1857566996, writer, 0, true);
		NetworkWriterPool.Return(writer);
	}

	// Token: 0x06000DDE RID: 3550 RVA: 0x00039C48 File Offset: 0x00037E48
	private void ToggleMoneyUI(bool isEnabled, bool showCont)
	{
		this.moneyHolder.SetActive(isEnabled);
		this.contributionsHolder.SetActive(showCont);
	}

	// Token: 0x06000DDF RID: 3551 RVA: 0x00039C62 File Offset: 0x00037E62
	[Server]
	public void ServerToggleStatusUI(bool isEnabled)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void GameUI::ServerToggleStatusUI(System.Boolean)' called when server was not active");
			return;
		}
		this.RpcToggleStatusUI(isEnabled);
	}

	// Token: 0x06000DE0 RID: 3552 RVA: 0x00039C80 File Offset: 0x00037E80
	[ClientRpc]
	private void RpcToggleStatusUI(bool isEnabled)
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		writer.WriteBool(isEnabled);
		this.SendRPCInternal("System.Void GameUI::RpcToggleStatusUI(System.Boolean)", 1917469469, writer, 0, true);
		NetworkWriterPool.Return(writer);
	}

	// Token: 0x06000DE1 RID: 3553 RVA: 0x00039CBA File Offset: 0x00037EBA
	private void ToggleStatusUI(bool isEnabled)
	{
		this.statusHolder.SetActive(isEnabled);
	}

	// Token: 0x06000DE2 RID: 3554 RVA: 0x00039CC8 File Offset: 0x00037EC8
	[Server]
	public void ServerToggleCrosshair(bool isEnabled)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void GameUI::ServerToggleCrosshair(System.Boolean)' called when server was not active");
			return;
		}
		this.RpcToggleCrosshair(isEnabled);
	}

	// Token: 0x06000DE3 RID: 3555 RVA: 0x00039CE8 File Offset: 0x00037EE8
	[ClientRpc]
	private void RpcToggleCrosshair(bool isEnabled)
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		writer.WriteBool(isEnabled);
		this.SendRPCInternal("System.Void GameUI::RpcToggleCrosshair(System.Boolean)", -1365095413, writer, 0, true);
		NetworkWriterPool.Return(writer);
	}

	// Token: 0x06000DE4 RID: 3556 RVA: 0x00039D22 File Offset: 0x00037F22
	private void ToggleCrosshair(bool isEnabled)
	{
		MonoSingleton<LocalManager>.Instance.SetCrosshair(isEnabled);
	}

	// Token: 0x06000DE5 RID: 3557 RVA: 0x00039D2F File Offset: 0x00037F2F
	[Server]
	public void ServerToggleItemInputsUI(bool isEnabled)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void GameUI::ServerToggleItemInputsUI(System.Boolean)' called when server was not active");
			return;
		}
		this.RpcToggleItemInputsUI(isEnabled);
	}

	// Token: 0x06000DE6 RID: 3558 RVA: 0x00039D50 File Offset: 0x00037F50
	[ClientRpc]
	private void RpcToggleItemInputsUI(bool isEnabled)
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		writer.WriteBool(isEnabled);
		this.SendRPCInternal("System.Void GameUI::RpcToggleItemInputsUI(System.Boolean)", -1451531007, writer, 0, true);
		NetworkWriterPool.Return(writer);
	}

	// Token: 0x06000DE7 RID: 3559 RVA: 0x00039D8A File Offset: 0x00037F8A
	private void ToggleItemInputsUI(bool isEnabled)
	{
		MonoSingleton<LocalManager>.Instance.itemInputsUI.SetActive(isEnabled);
	}

	// Token: 0x06000DE9 RID: 3561 RVA: 0x00002321 File Offset: 0x00000521
	public override bool Weaved()
	{
		return true;
	}

	// Token: 0x06000DEA RID: 3562 RVA: 0x00039DA4 File Offset: 0x00037FA4
	protected void UserCode_RpcSetLoadingScreen__Boolean__Single__Boolean(bool isEnabled, float duration, bool loadingScreen)
	{
		MonoSingleton<SceneTransitioner>.Instance.SetLoadingScreen(isEnabled, duration, loadingScreen);
	}

	// Token: 0x06000DEB RID: 3563 RVA: 0x00039DB3 File Offset: 0x00037FB3
	protected static void InvokeUserCode_RpcSetLoadingScreen__Boolean__Single__Boolean(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcSetLoadingScreen called on server.");
			return;
		}
		((GameUI)obj).UserCode_RpcSetLoadingScreen__Boolean__Single__Boolean(reader.ReadBool(), reader.ReadFloat(), reader.ReadBool());
	}

	// Token: 0x06000DEC RID: 3564 RVA: 0x00039DE9 File Offset: 0x00037FE9
	protected void UserCode_RpcToggleTimer__Boolean(bool isEnabled)
	{
		this.ToggleTimer(isEnabled);
	}

	// Token: 0x06000DED RID: 3565 RVA: 0x00039DF2 File Offset: 0x00037FF2
	protected static void InvokeUserCode_RpcToggleTimer__Boolean(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcToggleTimer called on server.");
			return;
		}
		((GameUI)obj).UserCode_RpcToggleTimer__Boolean(reader.ReadBool());
	}

	// Token: 0x06000DEE RID: 3566 RVA: 0x00039E1B File Offset: 0x0003801B
	protected void UserCode_RpcToggleMoneyUI__Boolean__Boolean(bool isEnabled, bool showCont)
	{
		this.ToggleMoneyUI(isEnabled, showCont);
	}

	// Token: 0x06000DEF RID: 3567 RVA: 0x00039E25 File Offset: 0x00038025
	protected static void InvokeUserCode_RpcToggleMoneyUI__Boolean__Boolean(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcToggleMoneyUI called on server.");
			return;
		}
		((GameUI)obj).UserCode_RpcToggleMoneyUI__Boolean__Boolean(reader.ReadBool(), reader.ReadBool());
	}

	// Token: 0x06000DF0 RID: 3568 RVA: 0x00039E54 File Offset: 0x00038054
	protected void UserCode_RpcToggleStatusUI__Boolean(bool isEnabled)
	{
		this.ToggleStatusUI(isEnabled);
	}

	// Token: 0x06000DF1 RID: 3569 RVA: 0x00039E5D File Offset: 0x0003805D
	protected static void InvokeUserCode_RpcToggleStatusUI__Boolean(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcToggleStatusUI called on server.");
			return;
		}
		((GameUI)obj).UserCode_RpcToggleStatusUI__Boolean(reader.ReadBool());
	}

	// Token: 0x06000DF2 RID: 3570 RVA: 0x00039E86 File Offset: 0x00038086
	protected void UserCode_RpcToggleCrosshair__Boolean(bool isEnabled)
	{
		this.ToggleCrosshair(isEnabled);
	}

	// Token: 0x06000DF3 RID: 3571 RVA: 0x00039E8F File Offset: 0x0003808F
	protected static void InvokeUserCode_RpcToggleCrosshair__Boolean(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcToggleCrosshair called on server.");
			return;
		}
		((GameUI)obj).UserCode_RpcToggleCrosshair__Boolean(reader.ReadBool());
	}

	// Token: 0x06000DF4 RID: 3572 RVA: 0x00039EB8 File Offset: 0x000380B8
	protected void UserCode_RpcToggleItemInputsUI__Boolean(bool isEnabled)
	{
		this.ToggleItemInputsUI(isEnabled);
	}

	// Token: 0x06000DF5 RID: 3573 RVA: 0x00039EC1 File Offset: 0x000380C1
	protected static void InvokeUserCode_RpcToggleItemInputsUI__Boolean(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcToggleItemInputsUI called on server.");
			return;
		}
		((GameUI)obj).UserCode_RpcToggleItemInputsUI__Boolean(reader.ReadBool());
	}

	// Token: 0x06000DF6 RID: 3574 RVA: 0x00039EEC File Offset: 0x000380EC
	static GameUI()
	{
		RemoteProcedureCalls.RegisterRpc(typeof(GameUI), "System.Void GameUI::RpcSetLoadingScreen(System.Boolean,System.Single,System.Boolean)", new RemoteCallDelegate(GameUI.InvokeUserCode_RpcSetLoadingScreen__Boolean__Single__Boolean));
		RemoteProcedureCalls.RegisterRpc(typeof(GameUI), "System.Void GameUI::RpcToggleTimer(System.Boolean)", new RemoteCallDelegate(GameUI.InvokeUserCode_RpcToggleTimer__Boolean));
		RemoteProcedureCalls.RegisterRpc(typeof(GameUI), "System.Void GameUI::RpcToggleMoneyUI(System.Boolean,System.Boolean)", new RemoteCallDelegate(GameUI.InvokeUserCode_RpcToggleMoneyUI__Boolean__Boolean));
		RemoteProcedureCalls.RegisterRpc(typeof(GameUI), "System.Void GameUI::RpcToggleStatusUI(System.Boolean)", new RemoteCallDelegate(GameUI.InvokeUserCode_RpcToggleStatusUI__Boolean));
		RemoteProcedureCalls.RegisterRpc(typeof(GameUI), "System.Void GameUI::RpcToggleCrosshair(System.Boolean)", new RemoteCallDelegate(GameUI.InvokeUserCode_RpcToggleCrosshair__Boolean));
		RemoteProcedureCalls.RegisterRpc(typeof(GameUI), "System.Void GameUI::RpcToggleItemInputsUI(System.Boolean)", new RemoteCallDelegate(GameUI.InvokeUserCode_RpcToggleItemInputsUI__Boolean));
	}

	// Token: 0x040008CC RID: 2252
	[Header("References")]
	[SerializeField]
	private GameObject statusHolder;

	// Token: 0x040008CD RID: 2253
	[SerializeField]
	private GameObject timerHolder;

	// Token: 0x040008CE RID: 2254
	[SerializeField]
	private GameObject moneyHolder;

	// Token: 0x040008CF RID: 2255
	[SerializeField]
	private GameObject contributionsHolder;

	// Token: 0x040008D0 RID: 2256
	[SerializeField]
	private TextMeshProUGUI dayText;

	// Token: 0x040008D1 RID: 2257
	[SerializeField]
	private TextMeshProUGUI floorText;

	// Token: 0x040008D2 RID: 2258
	[SerializeField]
	private TextMeshProUGUI timerText;
}
