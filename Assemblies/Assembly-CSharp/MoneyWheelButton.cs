using System;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using Mirror;
using Mirror.RemoteCalls;
using MoreMountains.Feedbacks;
using UnityEngine;

// Token: 0x02000069 RID: 105
public class MoneyWheelButton : InteractableBase
{
	// Token: 0x060003A6 RID: 934 RVA: 0x00011446 File Offset: 0x0000F646
	protected override void OnAwake()
	{
		base.OnAwake();
		this._localScaleZ = this.modelTransform.localScale.z;
	}

	// Token: 0x060003A7 RID: 935 RVA: 0x00011464 File Offset: 0x0000F664
	public override void ServerOnInteract(PlayerInteract playerInteract)
	{
		base.ServerOnInteract(playerInteract);
		this.moneyWheel.SelectBettingOption(this.betOption);
	}

	// Token: 0x060003A8 RID: 936 RVA: 0x0001147E File Offset: 0x0000F67E
	public void SelectFeedBack(bool isSelected)
	{
		this.RpcSelectFeedBack(isSelected);
	}

	// Token: 0x060003A9 RID: 937 RVA: 0x00011488 File Offset: 0x0000F688
	[ClientRpc]
	private void RpcSelectFeedBack(bool isSelected)
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		writer.WriteBool(isSelected);
		this.SendRPCInternal("System.Void MoneyWheelButton::RpcSelectFeedBack(System.Boolean)", -1914425721, writer, 0, true);
		NetworkWriterPool.Return(writer);
	}

	// Token: 0x060003AA RID: 938 RVA: 0x000114C2 File Offset: 0x0000F6C2
	public override void RpcOnInteract(PlayerInteract playerInteract)
	{
		base.RpcOnInteract(playerInteract);
		this.pressFb.PlayFeedbacks();
	}

	// Token: 0x060003AC RID: 940 RVA: 0x00002321 File Offset: 0x00000521
	public override bool Weaved()
	{
		return true;
	}

	// Token: 0x060003AD RID: 941 RVA: 0x000114D8 File Offset: 0x0000F6D8
	protected void UserCode_RpcSelectFeedBack__Boolean(bool isSelected)
	{
		float num = this._localScaleZ;
		if (isSelected)
		{
			num -= 0.5f;
		}
		this.modelTransform.DOScaleZ(num, 0.3f).SetEase(isSelected ? Ease.OutBack : Ease.InBack);
	}

	// Token: 0x060003AE RID: 942 RVA: 0x00011517 File Offset: 0x0000F717
	protected static void InvokeUserCode_RpcSelectFeedBack__Boolean(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcSelectFeedBack called on server.");
			return;
		}
		((MoneyWheelButton)obj).UserCode_RpcSelectFeedBack__Boolean(reader.ReadBool());
	}

	// Token: 0x060003AF RID: 943 RVA: 0x00011540 File Offset: 0x0000F740
	static MoneyWheelButton()
	{
		RemoteProcedureCalls.RegisterRpc(typeof(MoneyWheelButton), "System.Void MoneyWheelButton::RpcSelectFeedBack(System.Boolean)", new RemoteCallDelegate(MoneyWheelButton.InvokeUserCode_RpcSelectFeedBack__Boolean));
	}

	// Token: 0x0400029F RID: 671
	[SerializeField]
	private MoneyWheel moneyWheel;

	// Token: 0x040002A0 RID: 672
	[SerializeField]
	private Transform modelTransform;

	// Token: 0x040002A1 RID: 673
	[SerializeField]
	private MMF_Player pressFb;

	// Token: 0x040002A2 RID: 674
	public string betOption;

	// Token: 0x040002A3 RID: 675
	private float _localScaleZ;
}
