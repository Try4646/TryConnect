using System;
using System.Collections;
using Mirror;
using Mirror.RemoteCalls;
using UnityEngine;

// Token: 0x02000067 RID: 103
public class MoneyWheel : GameBase
{
	// Token: 0x06000391 RID: 913 RVA: 0x00011155 File Offset: 0x0000F355
	public override void OnStartServer()
	{
		base.OnStartServer();
		this.SelectBettingOption("Green");
	}

	// Token: 0x06000392 RID: 914 RVA: 0x00011168 File Offset: 0x0000F368
	private void OnEnable()
	{
		this.wheel.OnWheelStopped += this.HandleWheelStopped;
	}

	// Token: 0x06000393 RID: 915 RVA: 0x00011181 File Offset: 0x0000F381
	protected override void OnDisable()
	{
		base.OnDisable();
		this.wheel.OnWheelStopped -= this.HandleWheelStopped;
	}

	// Token: 0x06000394 RID: 916 RVA: 0x000111A0 File Offset: 0x0000F3A0
	[Server]
	protected override void StartGame()
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void MoneyWheel::StartGame()' called when server was not active");
			return;
		}
		base.StartGame();
		this.wheel.SpinTheWheel(base.GetSeededRandom(0));
	}

	// Token: 0x06000395 RID: 917 RVA: 0x000111D0 File Offset: 0x0000F3D0
	[Server]
	public void SelectBettingOption(string option)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void MoneyWheel::SelectBettingOption(System.String)' called when server was not active");
			return;
		}
		if (this.isPlaying)
		{
			return;
		}
		this._currentBettingOption = option;
		foreach (MoneyWheelButton moneyWheelButton in this.buttons)
		{
			moneyWheelButton.SelectFeedBack(option == moneyWheelButton.betOption);
			if (option == moneyWheelButton.betOption)
			{
				this.RpcSetBetIndicatorPosition(moneyWheelButton.transform.position);
			}
		}
	}

	// Token: 0x06000396 RID: 918 RVA: 0x0001124C File Offset: 0x0000F44C
	private void HandleWheelStopped(string result)
	{
		if (!base.isServer)
		{
			return;
		}
		if (!this.isPlaying)
		{
			return;
		}
		if (!(result == this._currentBettingOption))
		{
			this.EndGame(0f);
			return;
		}
		if (result == "Green")
		{
			this.EndGame(2f);
			return;
		}
		if (result == "Blue")
		{
			this.EndGame(3f);
			return;
		}
		if (result == "Red")
		{
			this.EndGame(5f);
			return;
		}
		if (!(result == "Orange"))
		{
			return;
		}
		this.EndGame(10f);
	}

	// Token: 0x06000397 RID: 919 RVA: 0x000112E9 File Offset: 0x0000F4E9
	private void EndGame(float multiplier)
	{
		this.Payout((double)multiplier * base.EstimatedValue, ChangeType.GameResult, null, -1L);
		base.StartCoroutine(this.ResetGameRoutine());
	}

	// Token: 0x06000398 RID: 920 RVA: 0x0001130B File Offset: 0x0000F50B
	private IEnumerator ResetGameRoutine()
	{
		yield return new WaitForSeconds(1f);
		this.ResetGame();
		yield break;
	}

	// Token: 0x06000399 RID: 921 RVA: 0x0001131A File Offset: 0x0000F51A
	protected override void ResetGame()
	{
		base.ResetGame();
		this.wheel.ResetWheel();
	}

	// Token: 0x0600039A RID: 922 RVA: 0x00011330 File Offset: 0x0000F530
	[ClientRpc]
	private void RpcSetBetIndicatorPosition(Vector3 position)
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		writer.WriteVector3(position);
		this.SendRPCInternal("System.Void MoneyWheel::RpcSetBetIndicatorPosition(UnityEngine.Vector3)", -1398401581, writer, 0, true);
		NetworkWriterPool.Return(writer);
	}

	// Token: 0x0600039C RID: 924 RVA: 0x00002321 File Offset: 0x00000521
	public override bool Weaved()
	{
		return true;
	}

	// Token: 0x0600039D RID: 925 RVA: 0x0001137D File Offset: 0x0000F57D
	protected void UserCode_RpcSetBetIndicatorPosition__Vector3(Vector3 position)
	{
		this.betIndicator.transform.position = position;
	}

	// Token: 0x0600039E RID: 926 RVA: 0x00011390 File Offset: 0x0000F590
	protected static void InvokeUserCode_RpcSetBetIndicatorPosition__Vector3(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcSetBetIndicatorPosition called on server.");
			return;
		}
		((MoneyWheel)obj).UserCode_RpcSetBetIndicatorPosition__Vector3(reader.ReadVector3());
	}

	// Token: 0x0600039F RID: 927 RVA: 0x000113B9 File Offset: 0x0000F5B9
	static MoneyWheel()
	{
		RemoteProcedureCalls.RegisterRpc(typeof(MoneyWheel), "System.Void MoneyWheel::RpcSetBetIndicatorPosition(UnityEngine.Vector3)", new RemoteCallDelegate(MoneyWheel.InvokeUserCode_RpcSetBetIndicatorPosition__Vector3));
	}

	// Token: 0x04000298 RID: 664
	[Header("References")]
	[SerializeField]
	private Wheel wheel;

	// Token: 0x04000299 RID: 665
	[SerializeField]
	private MoneyWheelButton[] buttons;

	// Token: 0x0400029A RID: 666
	[SerializeField]
	private Transform betIndicator;

	// Token: 0x0400029B RID: 667
	private string _currentBettingOption = "Green";
}
