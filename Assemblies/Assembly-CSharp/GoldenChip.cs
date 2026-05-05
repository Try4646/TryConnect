using System;
using System.Collections;
using System.Runtime.InteropServices;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using Extensions;
using JetBrains.Annotations;
using Mirror;
using Mirror.RemoteCalls;
using UnityEngine;

// Token: 0x020000EB RID: 235
public class GoldenChip : ConsumableItem
{
	// Token: 0x0600097A RID: 2426 RVA: 0x00025F22 File Offset: 0x00024122
	protected override void OnDropped(PlayerInventory playerInventory)
	{
		base.OnDropped(playerInventory);
		if (this._applyChipRoutine != null)
		{
			base.StopCoroutine(this._applyChipRoutine);
		}
		this.RpcOnDropped();
	}

	// Token: 0x0600097B RID: 2427 RVA: 0x00025F48 File Offset: 0x00024148
	[ClientRpc]
	private void RpcOnDropped()
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		this.SendRPCInternal("System.Void GoldenChip::RpcOnDropped()", -1265912124, writer, 0, true);
		NetworkWriterPool.Return(writer);
	}

	// Token: 0x0600097C RID: 2428 RVA: 0x00025F78 File Offset: 0x00024178
	protected override void OnPickedUp(PlayerInventory playerInventory)
	{
		base.OnPickedUp(playerInventory);
		this.RpcOnPickedUp();
	}

	// Token: 0x0600097D RID: 2429 RVA: 0x00025F88 File Offset: 0x00024188
	[ClientRpc]
	private void RpcOnPickedUp()
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		this.SendRPCInternal("System.Void GoldenChip::RpcOnPickedUp()", 358503685, writer, 0, true);
		NetworkWriterPool.Return(writer);
	}

	// Token: 0x0600097E RID: 2430 RVA: 0x00025FB8 File Offset: 0x000241B8
	protected override void OnUseItem(bool isPressed)
	{
		if (!base.NetworkHolder)
		{
			return;
		}
		if (!base.NetworkHolder.isLocalPlayer)
		{
			return;
		}
		if (this._setTargetGameRoutine != null)
		{
			base.StopCoroutine(this._setTargetGameRoutine);
		}
		if (isPressed)
		{
			this._setTargetGameRoutine = base.StartCoroutine(this.SetTargetGameRoutine());
			return;
		}
		this.SetTargetGame(null);
	}

	// Token: 0x0600097F RID: 2431 RVA: 0x00026012 File Offset: 0x00024212
	private IEnumerator SetTargetGameRoutine()
	{
		while (base.NetworkHolder && !this.Network_targetGame)
		{
			this.TryFindTargetGame();
			yield return new WaitForSeconds(0.1f);
		}
		yield break;
	}

	// Token: 0x06000980 RID: 2432 RVA: 0x00026024 File Offset: 0x00024224
	private void TryFindTargetGame()
	{
		Camera mainCamera = MonoSingleton<LocalManager>.Instance.mainCamera;
		Vector3 position = mainCamera.transform.position;
		Vector3 forward = mainCamera.transform.forward;
		int num = Physics.RaycastNonAlloc(new Ray(position, forward), this._raycastHits, this.raycastDistance, this.rayMask, QueryTriggerInteraction.Ignore);
		if (num <= 0)
		{
			return;
		}
		Keypad keypad = null;
		for (int i = 0; i < num; i++)
		{
			RaycastHit raycastHit = this._raycastHits[i];
			Keypad keypad2;
			if (raycastHit.transform.TryGetComponent<Keypad>(out keypad2) && (base.transform.position - keypad2.transform.position).sqrMagnitude <= this.raycastDistance * this.raycastDistance && !keypad2.NetworkcasinoGame.isGoldenChipApplied)
			{
				keypad = keypad2;
				break;
			}
		}
		if (keypad)
		{
			this.SetTargetGame(keypad.NetworkcasinoGame);
		}
	}

	// Token: 0x06000981 RID: 2433 RVA: 0x00026108 File Offset: 0x00024308
	private void SetTargetGame([CanBeNull] GameBase targetGame)
	{
		if (targetGame == this.Network_targetGame)
		{
			return;
		}
		this.Network_targetGame = targetGame;
		bool isApplied = targetGame;
		this.ApplyChip(isApplied);
		this.CmdSetTargetGame(this.Network_targetGame);
	}

	// Token: 0x06000982 RID: 2434 RVA: 0x00026148 File Offset: 0x00024348
	[Command(requiresAuthority = false)]
	private void CmdSetTargetGame([CanBeNull] GameBase targetGame)
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		writer.WriteNetworkBehaviour(targetGame);
		base.SendCommandInternal("System.Void GoldenChip::CmdSetTargetGame(GameBase)", 481031430, writer, 0, false);
		NetworkWriterPool.Return(writer);
	}

	// Token: 0x06000983 RID: 2435 RVA: 0x00026184 File Offset: 0x00024384
	[ClientRpc]
	private void RpcSetTargetGame([CanBeNull] GameBase targetGame)
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		writer.WriteNetworkBehaviour(targetGame);
		this.SendRPCInternal("System.Void GoldenChip::RpcSetTargetGame(GameBase)", 2051719151, writer, 0, true);
		NetworkWriterPool.Return(writer);
	}

	// Token: 0x06000984 RID: 2436 RVA: 0x000261C0 File Offset: 0x000243C0
	[ClientRpc]
	private void RpcApplyChip(bool isApplied)
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		writer.WriteBool(isApplied);
		this.SendRPCInternal("System.Void GoldenChip::RpcApplyChip(System.Boolean)", 1547888810, writer, 0, true);
		NetworkWriterPool.Return(writer);
	}

	// Token: 0x06000985 RID: 2437 RVA: 0x000261FC File Offset: 0x000243FC
	private void ApplyChip(bool isApplied)
	{
		if (base.isServer && this._applyChipRoutine != null)
		{
			base.StopCoroutine(this._applyChipRoutine);
		}
		this.modelTransform.DOKill(false);
		this.onHandFb.StopFeedbacks();
		if (isApplied)
		{
			this.anim.SetBool("IsApplying", true);
			this.activateSfx.LoopSFX(true);
			Vector3 targetPosition = this.Network_targetGame.keypadSpawnPoint.transform.position + this.Network_targetGame.keypadSpawnPoint.transform.forward * 0.4f + this.Network_targetGame.keypadSpawnPoint.transform.up * -0.2f + this.Network_targetGame.keypadSpawnPoint.transform.right * -0.2f;
			Quaternion targetRotation = Quaternion.LookRotation(-this.Network_targetGame.keypadSpawnPoint.transform.forward, this.Network_targetGame.keypadSpawnPoint.transform.up);
			this.modelTransform.DOMove(targetPosition, this.applyTime - 0.2f, false).SetEase(Ease.OutCubic).OnComplete(delegate
			{
				this.modelTransform.DOMove(targetPosition, 0.2f, false);
			});
			this.modelTransform.DORotateQuaternion(targetRotation, this.applyTime - 0.2f).SetEase(Ease.OutCubic).OnComplete(delegate
			{
				this.modelTransform.DORotateQuaternion(targetRotation, 0.2f);
			});
			if (base.isServer)
			{
				this._applyChipRoutine = base.StartCoroutine(this.ApplyChipRoutine());
				return;
			}
		}
		else
		{
			this.anim.SetBool("IsApplying", false);
			this.activateSfx.LoopSFX(false);
			this.modelTransform.DOLocalMove(Vector3.zero, 0.1f, false).SetEase(Ease.OutCubic);
			this.modelTransform.DOLocalRotate(Vector3.zero, 0.1f, RotateMode.Fast).SetEase(Ease.OutCubic);
		}
	}

	// Token: 0x06000986 RID: 2438 RVA: 0x0002640F File Offset: 0x0002460F
	private IEnumerator ApplyChipRoutine()
	{
		yield return new WaitForSeconds(this.applyTime);
		this.Network_targetGame.ApplyGoldenChip(NetworkSingleton<UpgradeManager>.Instance.GetUpgradeData(base.NetworkHolder.GetComponent<PlayerProfile>().steamId, PlayerUpgradeType.Stakeholder));
		base.DestroyItem();
		yield break;
	}

	// Token: 0x06000987 RID: 2439 RVA: 0x00026420 File Offset: 0x00024620
	private void Update()
	{
		if (!base.NetworkHolder)
		{
			return;
		}
		if (!this.Network_targetGame)
		{
			return;
		}
		if (!base.NetworkHolder.isLocalPlayer)
		{
			return;
		}
		if (this.Network_targetGame.isGoldenChipApplied || (base.transform.position - this.Network_targetGame.keypadSpawnPoint.transform.position).sqrMagnitude > this.raycastDistance * this.raycastDistance)
		{
			this.SetTargetGame(null);
			if (this._setTargetGameRoutine != null)
			{
				base.StopCoroutine(this._setTargetGameRoutine);
			}
			this._setTargetGameRoutine = base.StartCoroutine(this.SetTargetGameRoutine());
		}
	}

	// Token: 0x06000989 RID: 2441 RVA: 0x00002321 File Offset: 0x00000521
	public override bool Weaved()
	{
		return true;
	}

	// Token: 0x170000D9 RID: 217
	// (get) Token: 0x0600098A RID: 2442 RVA: 0x000264F8 File Offset: 0x000246F8
	// (set) Token: 0x0600098B RID: 2443 RVA: 0x00026517 File Offset: 0x00024717
	public GameBase Network_targetGame
	{
		get
		{
			return base.GetSyncVarNetworkBehaviour<GameBase>(this.____targetGameNetId, ref this._targetGame);
		}
		[param: In]
		set
		{
			base.GeneratedSyncVarSetter_NetworkBehaviour<GameBase>(value, ref this._targetGame, 2UL, null, ref this.____targetGameNetId);
		}
	}

	// Token: 0x0600098C RID: 2444 RVA: 0x00026538 File Offset: 0x00024738
	protected void UserCode_RpcOnDropped()
	{
		this.Network_targetGame = null;
		this.modelTransform.DOKill(false);
		this.modelTransform.localPosition = Vector3.zero;
		this.modelTransform.localRotation = Quaternion.identity;
		this.anim.SetBool("IsApplying", false);
		this.anim.Play("Default", 0, 0f);
		this.anim.Update(0f);
		this.activateSfx.LoopSFX(false);
	}

	// Token: 0x0600098D RID: 2445 RVA: 0x000265BC File Offset: 0x000247BC
	protected static void InvokeUserCode_RpcOnDropped(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcOnDropped called on server.");
			return;
		}
		((GoldenChip)obj).UserCode_RpcOnDropped();
	}

	// Token: 0x0600098E RID: 2446 RVA: 0x000265DF File Offset: 0x000247DF
	protected void UserCode_RpcOnPickedUp()
	{
		this.anim.SetTrigger("PickUp");
	}

	// Token: 0x0600098F RID: 2447 RVA: 0x000265F1 File Offset: 0x000247F1
	protected static void InvokeUserCode_RpcOnPickedUp(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcOnPickedUp called on server.");
			return;
		}
		((GoldenChip)obj).UserCode_RpcOnPickedUp();
	}

	// Token: 0x06000990 RID: 2448 RVA: 0x00026614 File Offset: 0x00024814
	protected void UserCode_CmdSetTargetGame__GameBase(GameBase targetGame)
	{
		this.Network_targetGame = targetGame;
		this.RpcSetTargetGame(targetGame);
		bool isApplied = targetGame;
		this.RpcApplyChip(isApplied);
	}

	// Token: 0x06000991 RID: 2449 RVA: 0x0002663D File Offset: 0x0002483D
	protected static void InvokeUserCode_CmdSetTargetGame__GameBase(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdSetTargetGame called on client.");
			return;
		}
		((GoldenChip)obj).UserCode_CmdSetTargetGame__GameBase(reader.ReadNetworkBehaviour<GameBase>());
	}

	// Token: 0x06000992 RID: 2450 RVA: 0x00026666 File Offset: 0x00024866
	protected void UserCode_RpcSetTargetGame__GameBase(GameBase targetGame)
	{
		if (!base.NetworkHolder || base.NetworkHolder.isLocalPlayer)
		{
			return;
		}
		this.Network_targetGame = targetGame;
	}

	// Token: 0x06000993 RID: 2451 RVA: 0x0002668A File Offset: 0x0002488A
	protected static void InvokeUserCode_RpcSetTargetGame__GameBase(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcSetTargetGame called on server.");
			return;
		}
		((GoldenChip)obj).UserCode_RpcSetTargetGame__GameBase(reader.ReadNetworkBehaviour<GameBase>());
	}

	// Token: 0x06000994 RID: 2452 RVA: 0x000266B3 File Offset: 0x000248B3
	protected void UserCode_RpcApplyChip__Boolean(bool isApplied)
	{
		if (!base.NetworkHolder || base.NetworkHolder.isLocalPlayer)
		{
			return;
		}
		this.ApplyChip(isApplied);
	}

	// Token: 0x06000995 RID: 2453 RVA: 0x000266D7 File Offset: 0x000248D7
	protected static void InvokeUserCode_RpcApplyChip__Boolean(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcApplyChip called on server.");
			return;
		}
		((GoldenChip)obj).UserCode_RpcApplyChip__Boolean(reader.ReadBool());
	}

	// Token: 0x06000996 RID: 2454 RVA: 0x00026700 File Offset: 0x00024900
	static GoldenChip()
	{
		RemoteProcedureCalls.RegisterCommand(typeof(GoldenChip), "System.Void GoldenChip::CmdSetTargetGame(GameBase)", new RemoteCallDelegate(GoldenChip.InvokeUserCode_CmdSetTargetGame__GameBase), false);
		RemoteProcedureCalls.RegisterRpc(typeof(GoldenChip), "System.Void GoldenChip::RpcOnDropped()", new RemoteCallDelegate(GoldenChip.InvokeUserCode_RpcOnDropped));
		RemoteProcedureCalls.RegisterRpc(typeof(GoldenChip), "System.Void GoldenChip::RpcOnPickedUp()", new RemoteCallDelegate(GoldenChip.InvokeUserCode_RpcOnPickedUp));
		RemoteProcedureCalls.RegisterRpc(typeof(GoldenChip), "System.Void GoldenChip::RpcSetTargetGame(GameBase)", new RemoteCallDelegate(GoldenChip.InvokeUserCode_RpcSetTargetGame__GameBase));
		RemoteProcedureCalls.RegisterRpc(typeof(GoldenChip), "System.Void GoldenChip::RpcApplyChip(System.Boolean)", new RemoteCallDelegate(GoldenChip.InvokeUserCode_RpcApplyChip__Boolean));
	}

	// Token: 0x06000997 RID: 2455 RVA: 0x000267B0 File Offset: 0x000249B0
	public override void SerializeSyncVars(NetworkWriter writer, bool forceAll)
	{
		base.SerializeSyncVars(writer, forceAll);
		if (forceAll)
		{
			writer.WriteNetworkBehaviour(this.Network_targetGame);
			return;
		}
		writer.WriteVarULong(this.syncVarDirtyBits);
		if ((this.syncVarDirtyBits & 2UL) != 0UL)
		{
			writer.WriteNetworkBehaviour(this.Network_targetGame);
		}
	}

	// Token: 0x06000998 RID: 2456 RVA: 0x00026808 File Offset: 0x00024A08
	public override void DeserializeSyncVars(NetworkReader reader, bool initialState)
	{
		base.DeserializeSyncVars(reader, initialState);
		if (initialState)
		{
			base.GeneratedSyncVarDeserialize_NetworkBehaviour<GameBase>(ref this._targetGame, null, reader, ref this.____targetGameNetId);
			return;
		}
		long num = (long)reader.ReadVarULong();
		if ((num & 2L) != 0L)
		{
			base.GeneratedSyncVarDeserialize_NetworkBehaviour<GameBase>(ref this._targetGame, null, reader, ref this.____targetGameNetId);
		}
	}

	// Token: 0x04000606 RID: 1542
	[SerializeField]
	private Animator anim;

	// Token: 0x04000607 RID: 1543
	[Header("Settings")]
	[SerializeField]
	private float applyTime = 1f;

	// Token: 0x04000608 RID: 1544
	[SerializeField]
	private float raycastDistance = 2f;

	// Token: 0x04000609 RID: 1545
	[SerializeField]
	private LayerMask rayMask;

	// Token: 0x0400060A RID: 1546
	[Header("SFX")]
	[SerializeField]
	private SFXLoopComponent activateSfx;

	// Token: 0x0400060B RID: 1547
	[SyncVar]
	private GameBase _targetGame;

	// Token: 0x0400060C RID: 1548
	private Coroutine _setTargetGameRoutine;

	// Token: 0x0400060D RID: 1549
	private Coroutine _applyChipRoutine;

	// Token: 0x0400060E RID: 1550
	private readonly RaycastHit[] _raycastHits = new RaycastHit[8];

	// Token: 0x0400060F RID: 1551
	protected NetworkBehaviourSyncVar ____targetGameNetId;
}
