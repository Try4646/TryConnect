using System;
using Mirror;
using Mirror.RemoteCalls;
using TMPro;
using UnityEngine;

// Token: 0x02000101 RID: 257
public class VoiceManipulatorItem : Item
{
	// Token: 0x06000A88 RID: 2696 RVA: 0x0002A163 File Offset: 0x00028363
	public override void OnStartClient()
	{
		if (!base.isServer)
		{
			return;
		}
		this.RpcUpdateText(this.fXType.ToString());
	}

	// Token: 0x06000A89 RID: 2697 RVA: 0x0002A188 File Offset: 0x00028388
	protected override void OnUseItem(bool isPressed)
	{
		if (!base.isServer)
		{
			return;
		}
		if (isPressed && this.playerVoiceFX != null)
		{
			this.fXType = this.fXType.Next<VoipManipulationManager.VoipFX>();
			this.playerVoiceFX.RpcStartVoiceFX(this.fXType);
			this.RpcUpdateText(this.fXType.ToString());
		}
	}

	// Token: 0x06000A8A RID: 2698 RVA: 0x0002A1E8 File Offset: 0x000283E8
	[ClientRpc]
	private void RpcUpdateText(string fxTypeText)
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		writer.WriteString(fxTypeText);
		this.SendRPCInternal("System.Void VoiceManipulatorItem::RpcUpdateText(System.String)", -1004062755, writer, 0, true);
		NetworkWriterPool.Return(writer);
	}

	// Token: 0x06000A8B RID: 2699 RVA: 0x0002A222 File Offset: 0x00028422
	protected override void OnPickedUp(PlayerInventory playerInventory)
	{
		base.OnPickedUp(playerInventory);
		playerInventory.TryGetComponent<PlayerVoiceFX>(out this.playerVoiceFX);
	}

	// Token: 0x06000A8C RID: 2700 RVA: 0x0002A238 File Offset: 0x00028438
	protected override void OnDropped(PlayerInventory playerInventory)
	{
		this.playerVoiceFX.CmdResetVoiceFX();
		this.playerVoiceFX = null;
		base.OnDropped(playerInventory);
	}

	// Token: 0x06000A8E RID: 2702 RVA: 0x00002321 File Offset: 0x00000521
	public override bool Weaved()
	{
		return true;
	}

	// Token: 0x06000A8F RID: 2703 RVA: 0x0002A253 File Offset: 0x00028453
	protected void UserCode_RpcUpdateText__String(string fxTypeText)
	{
		this.fXText.text = fxTypeText;
	}

	// Token: 0x06000A90 RID: 2704 RVA: 0x0002A261 File Offset: 0x00028461
	protected static void InvokeUserCode_RpcUpdateText__String(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcUpdateText called on server.");
			return;
		}
		((VoiceManipulatorItem)obj).UserCode_RpcUpdateText__String(reader.ReadString());
	}

	// Token: 0x06000A91 RID: 2705 RVA: 0x0002A28A File Offset: 0x0002848A
	static VoiceManipulatorItem()
	{
		RemoteProcedureCalls.RegisterRpc(typeof(VoiceManipulatorItem), "System.Void VoiceManipulatorItem::RpcUpdateText(System.String)", new RemoteCallDelegate(VoiceManipulatorItem.InvokeUserCode_RpcUpdateText__String));
	}

	// Token: 0x040006A0 RID: 1696
	[Header("Settings")]
	[SerializeField]
	private VoipManipulationManager.VoipFX fXType;

	// Token: 0x040006A1 RID: 1697
	[SerializeField]
	private TextMeshPro fXText;

	// Token: 0x040006A2 RID: 1698
	private PlayerVoiceFX playerVoiceFX;
}
