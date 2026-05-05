using System;
using FMODUnity;
using Mirror;
using Mirror.RemoteCalls;
using UnityEngine;

// Token: 0x020000DC RID: 220
public class Bongo : Item
{
	// Token: 0x060008B8 RID: 2232 RVA: 0x000231B0 File Offset: 0x000213B0
	protected override void OnUseItem(bool isPressed)
	{
		if (!base.isServer)
		{
			return;
		}
		if (isPressed && this.playerController != null)
		{
			float x = this.playerController.head.transform.rotation.x;
			SFXParams[] sFXParams = new SFXParams[]
			{
				new SFXParams("Force", Mathf.Clamp01(Mathf.Abs(x * 1.5f)))
			};
			if (x > 0f)
			{
				SFXManager.SFXOneShotWithParameters(this.bongoLo, sFXParams, base.transform.position, 1f);
				return;
			}
			SFXManager.SFXOneShotWithParameters(this.bongoHi, sFXParams, base.transform.position, 1f);
		}
	}

	// Token: 0x060008B9 RID: 2233 RVA: 0x00023264 File Offset: 0x00021464
	[ClientRpc]
	private void PlayBongLo(SFXParams[] sFXParams)
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		Mirror.GeneratedNetworkCode._Write_SFXParams[](writer, sFXParams);
		this.SendRPCInternal("System.Void Bongo::PlayBongLo(SFXParams[])", 985448762, writer, 0, true);
		NetworkWriterPool.Return(writer);
	}

	// Token: 0x060008BA RID: 2234 RVA: 0x000232A0 File Offset: 0x000214A0
	[ClientRpc]
	private void PlayBongHi(SFXParams[] sFXParams)
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		Mirror.GeneratedNetworkCode._Write_SFXParams[](writer, sFXParams);
		this.SendRPCInternal("System.Void Bongo::PlayBongHi(SFXParams[])", -2025937380, writer, 0, true);
		NetworkWriterPool.Return(writer);
	}

	// Token: 0x060008BB RID: 2235 RVA: 0x000232DA File Offset: 0x000214DA
	protected override void OnPickedUp(PlayerInventory playerInventory)
	{
		base.OnPickedUp(playerInventory);
		playerInventory.TryGetComponent<PlayerController>(out this.playerController);
	}

	// Token: 0x060008BC RID: 2236 RVA: 0x000232F0 File Offset: 0x000214F0
	protected override void OnDropped(PlayerInventory playerInventory)
	{
		this.playerController = null;
		base.OnDropped(playerInventory);
	}

	// Token: 0x060008BE RID: 2238 RVA: 0x00002321 File Offset: 0x00000521
	public override bool Weaved()
	{
		return true;
	}

	// Token: 0x060008BF RID: 2239 RVA: 0x00023300 File Offset: 0x00021500
	protected void UserCode_PlayBongLo__SFXParams[](SFXParams[] sFXParams)
	{
		SFXManager.SFXOneShotWithParameters(this.bongoLo, sFXParams, base.transform.position, 1f);
	}

	// Token: 0x060008C0 RID: 2240 RVA: 0x0002331E File Offset: 0x0002151E
	protected static void InvokeUserCode_PlayBongLo__SFXParams[](NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC PlayBongLo called on server.");
			return;
		}
		((Bongo)obj).UserCode_PlayBongLo__SFXParams[](Mirror.GeneratedNetworkCode._Read_SFXParams[](reader));
	}

	// Token: 0x060008C1 RID: 2241 RVA: 0x00023347 File Offset: 0x00021547
	protected void UserCode_PlayBongHi__SFXParams[](SFXParams[] sFXParams)
	{
		SFXManager.SFXOneShotWithParameters(this.bongoHi, sFXParams, base.transform.position, 1f);
	}

	// Token: 0x060008C2 RID: 2242 RVA: 0x00023365 File Offset: 0x00021565
	protected static void InvokeUserCode_PlayBongHi__SFXParams[](NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC PlayBongHi called on server.");
			return;
		}
		((Bongo)obj).UserCode_PlayBongHi__SFXParams[](Mirror.GeneratedNetworkCode._Read_SFXParams[](reader));
	}

	// Token: 0x060008C3 RID: 2243 RVA: 0x00023390 File Offset: 0x00021590
	static Bongo()
	{
		RemoteProcedureCalls.RegisterRpc(typeof(Bongo), "System.Void Bongo::PlayBongLo(SFXParams[])", new RemoteCallDelegate(Bongo.InvokeUserCode_PlayBongLo__SFXParams[]));
		RemoteProcedureCalls.RegisterRpc(typeof(Bongo), "System.Void Bongo::PlayBongHi(SFXParams[])", new RemoteCallDelegate(Bongo.InvokeUserCode_PlayBongHi__SFXParams[]));
	}

	// Token: 0x04000587 RID: 1415
	[SerializeField]
	private EventReference bongoHi;

	// Token: 0x04000588 RID: 1416
	[SerializeField]
	private EventReference bongoLo;

	// Token: 0x04000589 RID: 1417
	private PlayerController playerController;
}
