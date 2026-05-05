using System;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using FMODUnity;
using Mirror;
using Mirror.RemoteCalls;
using MoreMountains.Feedbacks;
using TMPro;
using UnityEngine;

// Token: 0x0200007B RID: 123
public class RouletteButton : InteractableBase
{
	// Token: 0x06000476 RID: 1142 RVA: 0x0001444A File Offset: 0x0001264A
	public override void ServerOnInteract(PlayerInteract playerInteract)
	{
		base.ServerOnInteract(playerInteract);
		this.roulette.SelectBettingOption(base.name, this);
	}

	// Token: 0x06000477 RID: 1143 RVA: 0x00014465 File Offset: 0x00012665
	public override void RpcOnInteract(PlayerInteract playerInteract)
	{
		base.RpcOnInteract(playerInteract);
		this.pressFb.PlayFeedbacks();
	}

	// Token: 0x06000478 RID: 1144 RVA: 0x00014479 File Offset: 0x00012679
	[Server]
	public void ServerWarningFeedback()
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void RouletteButton::ServerWarningFeedback()' called when server was not active");
			return;
		}
		this.RpcWarningFeedback();
	}

	// Token: 0x06000479 RID: 1145 RVA: 0x00014498 File Offset: 0x00012698
	[ClientRpc]
	private void RpcWarningFeedback()
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		this.SendRPCInternal("System.Void RouletteButton::RpcWarningFeedback()", 651345437, writer, 0, true);
		NetworkWriterPool.Return(writer);
	}

	// Token: 0x0600047A RID: 1146 RVA: 0x000144C8 File Offset: 0x000126C8
	[Server]
	public void ServerSetBets(long max, long bet)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void RouletteButton::ServerSetBets(System.Int64,System.Int64)' called when server was not active");
			return;
		}
		this.RpcSetBets(max, bet);
	}

	// Token: 0x0600047B RID: 1147 RVA: 0x000144E8 File Offset: 0x000126E8
	[ClientRpc]
	private void RpcSetBets(long max, long bet)
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		writer.WriteVarLong(max);
		writer.WriteVarLong(bet);
		this.SendRPCInternal("System.Void RouletteButton::RpcSetBets(System.Int64,System.Int64)", -137259232, writer, 0, true);
		NetworkWriterPool.Return(writer);
	}

	// Token: 0x0600047E RID: 1150 RVA: 0x00002321 File Offset: 0x00000521
	public override bool Weaved()
	{
		return true;
	}

	// Token: 0x0600047F RID: 1151 RVA: 0x0001453F File Offset: 0x0001273F
	protected void UserCode_RpcWarningFeedback()
	{
		this.warningFb.PlayFeedbacks();
	}

	// Token: 0x06000480 RID: 1152 RVA: 0x0001454C File Offset: 0x0001274C
	protected static void InvokeUserCode_RpcWarningFeedback(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcWarningFeedback called on server.");
			return;
		}
		((RouletteButton)obj).UserCode_RpcWarningFeedback();
	}

	// Token: 0x06000481 RID: 1153 RVA: 0x00014570 File Offset: 0x00012770
	protected void UserCode_RpcSetBets__Int64__Int64(long max, long bet)
	{
		float num = Mathf.Clamp01((float)bet / (float)max);
		float endValue = Mathf.Ceil(num * 10f) / 10f;
		this.chips.DOKill(false);
		if ((double)num <= 0.0001)
		{
			this.chips.DOLocalMoveY(0f, 0.2f, false).SetEase(Ease.OutCubic).OnComplete(delegate
			{
				this.chips.gameObject.SetActive(false);
			});
		}
		else
		{
			this.chips.gameObject.SetActive(true);
			this.chips.DOLocalMoveY(endValue, 0.2f, false).SetEase(Ease.OutCubic);
		}
		this.chipText.text = MoneyFormatter.FormatWithDollar(bet);
		SFXManager.SFXOneShot(this.chipSfx, base.transform.position);
	}

	// Token: 0x06000482 RID: 1154 RVA: 0x00014636 File Offset: 0x00012836
	protected static void InvokeUserCode_RpcSetBets__Int64__Int64(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcSetBets called on server.");
			return;
		}
		((RouletteButton)obj).UserCode_RpcSetBets__Int64__Int64(reader.ReadVarLong(), reader.ReadVarLong());
	}

	// Token: 0x06000483 RID: 1155 RVA: 0x00014668 File Offset: 0x00012868
	static RouletteButton()
	{
		RemoteProcedureCalls.RegisterRpc(typeof(RouletteButton), "System.Void RouletteButton::RpcWarningFeedback()", new RemoteCallDelegate(RouletteButton.InvokeUserCode_RpcWarningFeedback));
		RemoteProcedureCalls.RegisterRpc(typeof(RouletteButton), "System.Void RouletteButton::RpcSetBets(System.Int64,System.Int64)", new RemoteCallDelegate(RouletteButton.InvokeUserCode_RpcSetBets__Int64__Int64));
	}

	// Token: 0x04000313 RID: 787
	[Header("References")]
	[SerializeField]
	private Roulette roulette;

	// Token: 0x04000314 RID: 788
	[SerializeField]
	private Transform chips;

	// Token: 0x04000315 RID: 789
	[SerializeField]
	private TextMeshPro chipText;

	// Token: 0x04000316 RID: 790
	[SerializeField]
	private MMF_Player pressFb;

	// Token: 0x04000317 RID: 791
	[SerializeField]
	private MMF_Player warningFb;

	// Token: 0x04000318 RID: 792
	[Header("SFX")]
	[SerializeField]
	private EventReference chipSfx;
}
