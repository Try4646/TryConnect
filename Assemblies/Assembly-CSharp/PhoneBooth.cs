using System;
using System.Collections;
using DG.Tweening;
using FMODUnity;
using Mirror;
using Mirror.RemoteCalls;
using MoreMountains.Feedbacks;
using TMPro;
using UnityEngine;

// Token: 0x0200010C RID: 268
public class PhoneBooth : InteractableBase
{
	// Token: 0x06000B28 RID: 2856 RVA: 0x0002D3C9 File Offset: 0x0002B5C9
	private void Update()
	{
		if (!base.isServer)
		{
			return;
		}
		if (this._interactionCooldown <= 0f)
		{
			return;
		}
		this._interactionCooldown -= Time.deltaTime;
	}

	// Token: 0x06000B29 RID: 2857 RVA: 0x0002D3F4 File Offset: 0x0002B5F4
	public override void ServerOnInteract(PlayerInteract playerInteract)
	{
		base.ServerOnInteract(playerInteract);
		if (this._interactionCooldown > 0f)
		{
			return;
		}
		this._interactionCooldown += this.interactionCooldown;
		if (!this._isVehicleCalled)
		{
			this.ServerFirstInteraction();
			return;
		}
		this.ServerRerollChallenge();
	}

	// Token: 0x06000B2A RID: 2858 RVA: 0x0002D433 File Offset: 0x0002B633
	[Server]
	private void ServerFirstInteraction()
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void PhoneBooth::ServerFirstInteraction()' called when server was not active");
			return;
		}
		this._isVehicleCalled = true;
		base.StartCoroutine(this.FirstInteractionRoutine());
	}

	// Token: 0x06000B2B RID: 2859 RVA: 0x0002D45E File Offset: 0x0002B65E
	private IEnumerator FirstInteractionRoutine()
	{
		this.RpcOnPhoneAnswered();
		this._interactionCooldown = this.doorOpenDelay + 2f;
		yield return new WaitForSeconds(this.doorOpenDelay);
		this.vehicleDoors.ServerOpenDoors();
		this.RpcOnDoorsOpened();
		yield return new WaitForSeconds(0.5f);
		this.challengeBooth.TryGiveDailyChallenge();
		this.RpcOnCallEnded();
		yield return new WaitForSeconds(1.5f);
		this.RpcOnRerollActive();
		yield break;
	}

	// Token: 0x06000B2C RID: 2860 RVA: 0x0002D46D File Offset: 0x0002B66D
	[Server]
	private void ServerRerollChallenge()
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void PhoneBooth::ServerRerollChallenge()' called when server was not active");
			return;
		}
		this.challengeBooth.RerollChallenge();
		this.RpcOnChallengeRerolled();
	}

	// Token: 0x06000B2D RID: 2861 RVA: 0x0002D498 File Offset: 0x0002B698
	[ClientRpc]
	private void RpcOnPhoneAnswered()
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		this.SendRPCInternal("System.Void PhoneBooth::RpcOnPhoneAnswered()", -2075849234, writer, 0, true);
		NetworkWriterPool.Return(writer);
	}

	// Token: 0x06000B2E RID: 2862 RVA: 0x0002D4C8 File Offset: 0x0002B6C8
	[ClientRpc]
	private void RpcOnDoorsOpened()
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		this.SendRPCInternal("System.Void PhoneBooth::RpcOnDoorsOpened()", -648298079, writer, 0, true);
		NetworkWriterPool.Return(writer);
	}

	// Token: 0x06000B2F RID: 2863 RVA: 0x0002D4F8 File Offset: 0x0002B6F8
	[ClientRpc]
	private void RpcOnCallEnded()
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		this.SendRPCInternal("System.Void PhoneBooth::RpcOnCallEnded()", -1064754735, writer, 0, true);
		NetworkWriterPool.Return(writer);
	}

	// Token: 0x06000B30 RID: 2864 RVA: 0x0002D528 File Offset: 0x0002B728
	[ClientRpc]
	private void RpcOnChallengeRerolled()
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		this.SendRPCInternal("System.Void PhoneBooth::RpcOnChallengeRerolled()", 1290531531, writer, 0, true);
		NetworkWriterPool.Return(writer);
	}

	// Token: 0x06000B31 RID: 2865 RVA: 0x0002D558 File Offset: 0x0002B758
	[ClientRpc]
	private void RpcOnRerollActive()
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		this.SendRPCInternal("System.Void PhoneBooth::RpcOnRerollActive()", 721189153, writer, 0, true);
		NetworkWriterPool.Return(writer);
	}

	// Token: 0x06000B33 RID: 2867 RVA: 0x00002321 File Offset: 0x00000521
	public override bool Weaved()
	{
		return true;
	}

	// Token: 0x06000B34 RID: 2868 RVA: 0x0002D588 File Offset: 0x0002B788
	protected void UserCode_RpcOnPhoneAnswered()
	{
		this.IsInteractable = false;
		this.phoneWiggle.enabled = false;
		this.phoneWiggle.RestoreInitialValues();
		this.phoneRingEmitter.Stop();
		this.screenText.text = "On call...";
	}

	// Token: 0x06000B35 RID: 2869 RVA: 0x0002D5C3 File Offset: 0x0002B7C3
	protected static void InvokeUserCode_RpcOnPhoneAnswered(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcOnPhoneAnswered called on server.");
			return;
		}
		((PhoneBooth)obj).UserCode_RpcOnPhoneAnswered();
	}

	// Token: 0x06000B36 RID: 2870 RVA: 0x000048A7 File Offset: 0x00002AA7
	protected void UserCode_RpcOnDoorsOpened()
	{
	}

	// Token: 0x06000B37 RID: 2871 RVA: 0x0002D5E6 File Offset: 0x0002B7E6
	protected static void InvokeUserCode_RpcOnDoorsOpened(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcOnDoorsOpened called on server.");
			return;
		}
		((PhoneBooth)obj).UserCode_RpcOnDoorsOpened();
	}

	// Token: 0x06000B38 RID: 2872 RVA: 0x0002D609 File Offset: 0x0002B809
	protected void UserCode_RpcOnCallEnded()
	{
		this.screenText.text = "Doors Opened!";
	}

	// Token: 0x06000B39 RID: 2873 RVA: 0x0002D61B File Offset: 0x0002B81B
	protected static void InvokeUserCode_RpcOnCallEnded(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcOnCallEnded called on server.");
			return;
		}
		((PhoneBooth)obj).UserCode_RpcOnCallEnded();
	}

	// Token: 0x06000B3A RID: 2874 RVA: 0x0002D63E File Offset: 0x0002B83E
	protected void UserCode_RpcOnChallengeRerolled()
	{
		this.screenText.transform.DOPunchScale(this.screenText.transform.localScale * 0.2f, 0.3f, 1, 1f);
	}

	// Token: 0x06000B3B RID: 2875 RVA: 0x0002D676 File Offset: 0x0002B876
	protected static void InvokeUserCode_RpcOnChallengeRerolled(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcOnChallengeRerolled called on server.");
			return;
		}
		((PhoneBooth)obj).UserCode_RpcOnChallengeRerolled();
	}

	// Token: 0x06000B3C RID: 2876 RVA: 0x0002D699 File Offset: 0x0002B899
	protected void UserCode_RpcOnRerollActive()
	{
		this.screenText.text = "Reroll Challenge \n(1 Ticket)";
		this.screenText.color = Color.crimson;
		this.InteractableName = "Reroll Challenge (1 Ticket)";
		this.TooltipMessage = "[E] Reroll";
		this.IsInteractable = true;
	}

	// Token: 0x06000B3D RID: 2877 RVA: 0x0002D6D8 File Offset: 0x0002B8D8
	protected static void InvokeUserCode_RpcOnRerollActive(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcOnRerollActive called on server.");
			return;
		}
		((PhoneBooth)obj).UserCode_RpcOnRerollActive();
	}

	// Token: 0x06000B3E RID: 2878 RVA: 0x0002D6FC File Offset: 0x0002B8FC
	static PhoneBooth()
	{
		RemoteProcedureCalls.RegisterRpc(typeof(PhoneBooth), "System.Void PhoneBooth::RpcOnPhoneAnswered()", new RemoteCallDelegate(PhoneBooth.InvokeUserCode_RpcOnPhoneAnswered));
		RemoteProcedureCalls.RegisterRpc(typeof(PhoneBooth), "System.Void PhoneBooth::RpcOnDoorsOpened()", new RemoteCallDelegate(PhoneBooth.InvokeUserCode_RpcOnDoorsOpened));
		RemoteProcedureCalls.RegisterRpc(typeof(PhoneBooth), "System.Void PhoneBooth::RpcOnCallEnded()", new RemoteCallDelegate(PhoneBooth.InvokeUserCode_RpcOnCallEnded));
		RemoteProcedureCalls.RegisterRpc(typeof(PhoneBooth), "System.Void PhoneBooth::RpcOnChallengeRerolled()", new RemoteCallDelegate(PhoneBooth.InvokeUserCode_RpcOnChallengeRerolled));
		RemoteProcedureCalls.RegisterRpc(typeof(PhoneBooth), "System.Void PhoneBooth::RpcOnRerollActive()", new RemoteCallDelegate(PhoneBooth.InvokeUserCode_RpcOnRerollActive));
	}

	// Token: 0x040006F2 RID: 1778
	[Header("Settings")]
	[SerializeField]
	private float interactionCooldown;

	// Token: 0x040006F3 RID: 1779
	[SerializeField]
	private float doorOpenDelay;

	// Token: 0x040006F4 RID: 1780
	[Header("References")]
	[SerializeField]
	private VehicleDoors vehicleDoors;

	// Token: 0x040006F5 RID: 1781
	[SerializeField]
	private ChallengeBooth challengeBooth;

	// Token: 0x040006F6 RID: 1782
	[SerializeField]
	private StudioEventEmitter phoneRingEmitter;

	// Token: 0x040006F7 RID: 1783
	[SerializeField]
	private TextMeshPro screenText;

	// Token: 0x040006F8 RID: 1784
	[SerializeField]
	private MMWiggle phoneWiggle;

	// Token: 0x040006F9 RID: 1785
	private float _interactionCooldown;

	// Token: 0x040006FA RID: 1786
	private bool _isVehicleCalled;
}
