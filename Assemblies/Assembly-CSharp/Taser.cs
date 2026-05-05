using System;
using System.Collections;
using System.Runtime.InteropServices;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using Extensions;
using FMOD.Studio;
using FMODUnity;
using JetBrains.Annotations;
using Mirror;
using Mirror.RemoteCalls;
using UnityEngine;

// Token: 0x020000FA RID: 250
public class Taser : ConsumableItem
{
	// Token: 0x06000A37 RID: 2615 RVA: 0x00028E86 File Offset: 0x00027086
	protected override void OnPickedUp(PlayerInventory playerInventory)
	{
		base.OnPickedUp(playerInventory);
		this._holderProfile = playerInventory.GetComponent<PlayerProfile>();
	}

	// Token: 0x06000A38 RID: 2616 RVA: 0x00028E9B File Offset: 0x0002709B
	protected override void OnDropped(PlayerInventory playerInventory)
	{
		base.OnDropped(playerInventory);
		this.RpcOnDropped();
	}

	// Token: 0x06000A39 RID: 2617 RVA: 0x00028EAC File Offset: 0x000270AC
	[ClientRpc]
	private void RpcOnDropped()
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		this.SendRPCInternal("System.Void Taser::RpcOnDropped()", 519732880, writer, 0, true);
		NetworkWriterPool.Return(writer);
	}

	// Token: 0x06000A3A RID: 2618 RVA: 0x00028EDC File Offset: 0x000270DC
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
		if (this._currentCharge <= 0f)
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
		}
		else
		{
			this.SetTargetGame(null);
		}
		this.TaserFeedbacks(isPressed);
		this.CmdTaserFeedbacks(isPressed);
	}

	// Token: 0x06000A3B RID: 2619 RVA: 0x00028F53 File Offset: 0x00027153
	private IEnumerator SetTargetGameRoutine()
	{
		while (base.NetworkHolder && !this.Network_targetGame)
		{
			this.TryFindTargetGame();
			yield return new WaitForSeconds(0.1f);
		}
		yield break;
	}

	// Token: 0x06000A3C RID: 2620 RVA: 0x00028F64 File Offset: 0x00027164
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
			if (raycastHit.transform.TryGetComponent<Keypad>(out keypad2) && (base.transform.position - keypad2.transform.position).sqrMagnitude <= this.raycastDistance * this.raycastDistance)
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

	// Token: 0x06000A3D RID: 2621 RVA: 0x0002903C File Offset: 0x0002723C
	private void SetTargetGame([CanBeNull] GameBase targetGame)
	{
		if (targetGame == this.Network_targetGame)
		{
			return;
		}
		this.Network_targetGame = targetGame;
		bool flag = targetGame;
		this.UseTaser(flag);
		this.TaserLoopSfxParams(this._currentCharge, flag);
		this.CmdSetTargetGame(this.Network_targetGame);
	}

	// Token: 0x06000A3E RID: 2622 RVA: 0x00029088 File Offset: 0x00027288
	[Command(requiresAuthority = false)]
	private void CmdSetTargetGame([CanBeNull] GameBase targetGame)
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		writer.WriteNetworkBehaviour(targetGame);
		base.SendCommandInternal("System.Void Taser::CmdSetTargetGame(GameBase)", 1565143834, writer, 0, false);
		NetworkWriterPool.Return(writer);
	}

	// Token: 0x06000A3F RID: 2623 RVA: 0x000290C4 File Offset: 0x000272C4
	[ClientRpc]
	private void RpcSetTargetGame([CanBeNull] GameBase targetGame)
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		writer.WriteNetworkBehaviour(targetGame);
		this.SendRPCInternal("System.Void Taser::RpcSetTargetGame(GameBase)", -370932413, writer, 0, true);
		NetworkWriterPool.Return(writer);
	}

	// Token: 0x06000A40 RID: 2624 RVA: 0x00029100 File Offset: 0x00027300
	[ClientRpc]
	private void RpcUseTaser(bool isApplied)
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		writer.WriteBool(isApplied);
		this.SendRPCInternal("System.Void Taser::RpcUseTaser(System.Boolean)", -758977358, writer, 0, true);
		NetworkWriterPool.Return(writer);
	}

	// Token: 0x06000A41 RID: 2625 RVA: 0x0002913C File Offset: 0x0002733C
	private void UseTaser(bool isApplied)
	{
		this.modelTransform.DOKill(false);
		this.onHandFb.StopFeedbacks();
		if (isApplied)
		{
			Vector3 targetPosition = this.Network_targetGame.keypadSpawnPoint.transform.position + this.Network_targetGame.keypadSpawnPoint.transform.forward * 0.4f + this.Network_targetGame.keypadSpawnPoint.transform.up * -0.2f + this.Network_targetGame.keypadSpawnPoint.transform.right * -0.2f;
			Quaternion targetRotation = Quaternion.LookRotation(-this.Network_targetGame.keypadSpawnPoint.transform.up, -this.Network_targetGame.keypadSpawnPoint.transform.forward);
			this.modelTransform.DOMove(targetPosition, 0.3f, false).SetEase(Ease.OutCubic).OnComplete(delegate
			{
				this.modelTransform.DOMove(targetPosition, 10f, false);
			});
			this.modelTransform.DORotateQuaternion(targetRotation, 0.3f).SetEase(Ease.OutCubic).OnComplete(delegate
			{
				this.modelTransform.DORotateQuaternion(targetRotation, 10f);
			});
			return;
		}
		this.modelTransform.DOLocalMove(Vector3.zero, 0.1f, false).SetEase(Ease.OutCubic);
		this.modelTransform.DOLocalRotate(Vector3.zero, 0.1f, RotateMode.Fast).SetEase(Ease.OutCubic);
	}

	// Token: 0x06000A42 RID: 2626 RVA: 0x000292D8 File Offset: 0x000274D8
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
		if (base.NetworkHolder.isLocalPlayer && (base.transform.position - this.Network_targetGame.keypadSpawnPoint.transform.position).sqrMagnitude > this.raycastDistance * this.raycastDistance)
		{
			this.SetTargetGame(null);
			if (this._setTargetGameRoutine != null)
			{
				base.StopCoroutine(this._setTargetGameRoutine);
			}
			this._setTargetGameRoutine = base.StartCoroutine(this.SetTargetGameRoutine());
			return;
		}
		if (base.isServer)
		{
			this.UseCharge();
		}
	}

	// Token: 0x06000A43 RID: 2627 RVA: 0x00029388 File Offset: 0x00027588
	private void UseCharge()
	{
		if (Time.time - this._lastChargeTime < this.chargeTick)
		{
			return;
		}
		this._lastChargeTime = Time.time;
		this.SetCurrentCharge(this._currentCharge - this.chargeUsePerTick);
		this.RpcSetCurrentCharge(this._currentCharge);
		this.Network_targetGame.MaxBetOverrideMultiplier += (double)(this.totalMultiplierIncrease * this.chargeUsePerTick * NetworkSingleton<UpgradeManager>.Instance.GetUpgradeData(this._holderProfile.steamId, PlayerUpgradeType.Stakeholder));
		this.RpcTaserLoopSfxParams(this._currentCharge, true, true);
		if (this._currentCharge <= 0f)
		{
			this.Network_targetGame = null;
			this.RpcDestroySfx();
			base.DestroyItem();
		}
	}

	// Token: 0x06000A44 RID: 2628 RVA: 0x0002943C File Offset: 0x0002763C
	[ClientRpc]
	private void RpcSetCurrentCharge(float charge)
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		writer.WriteFloat(charge);
		this.SendRPCInternal("System.Void Taser::RpcSetCurrentCharge(System.Single)", -678428923, writer, 0, true);
		NetworkWriterPool.Return(writer);
	}

	// Token: 0x06000A45 RID: 2629 RVA: 0x00029476 File Offset: 0x00027676
	private void SetCurrentCharge(float charge)
	{
		this._currentCharge = charge;
		this.chargeBar.transform.DOScaleY(charge, 0.2f).SetEase(Ease.OutCubic);
	}

	// Token: 0x06000A46 RID: 2630 RVA: 0x000294A0 File Offset: 0x000276A0
	[Command(requiresAuthority = false)]
	private void CmdTaserFeedbacks(bool isPressed)
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		writer.WriteBool(isPressed);
		base.SendCommandInternal("System.Void Taser::CmdTaserFeedbacks(System.Boolean)", -1693549802, writer, 0, false);
		NetworkWriterPool.Return(writer);
	}

	// Token: 0x06000A47 RID: 2631 RVA: 0x000294DC File Offset: 0x000276DC
	[ClientRpc]
	private void RpcTaserFeedbacks(bool isPressed)
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		writer.WriteBool(isPressed);
		this.SendRPCInternal("System.Void Taser::RpcTaserFeedbacks(System.Boolean)", 1518651113, writer, 0, true);
		NetworkWriterPool.Return(writer);
	}

	// Token: 0x06000A48 RID: 2632 RVA: 0x00029518 File Offset: 0x00027718
	private void TaserFeedbacks(bool isPressed)
	{
		float endValue = isPressed ? 1f : 0f;
		DOTween.To(() => this.anim.GetFloat("Blend"), delegate(float x)
		{
			this.anim.SetFloat("Blend", x);
		}, endValue, 0.25f).SetEase(Ease.OutCubic);
		if (isPressed)
		{
			this.lightningParticles.Play();
		}
		else
		{
			this.lightningParticles.Stop();
		}
		if (isPressed)
		{
			SFXManager.SFXOneShot(this.taserInvalidSfx, base.transform.position);
		}
		this.TaserLoopSfx(isPressed);
	}

	// Token: 0x06000A49 RID: 2633 RVA: 0x0002959C File Offset: 0x0002779C
	private void TaserLoopSfx(bool play)
	{
		if (play)
		{
			if (this._taserLoopInstance.isValid())
			{
				PLAYBACK_STATE playback_STATE;
				this._taserLoopInstance.getPlaybackState(out playback_STATE);
				if (playback_STATE == PLAYBACK_STATE.PLAYING)
				{
					return;
				}
			}
			this._taserLoopInstance = RuntimeManager.CreateInstance(this.taserShootSfx);
			this._taserLoopInstance.set3DAttributes(base.transform.position.To3DAttributes());
			RuntimeManager.AttachInstanceToGameObject(this._taserLoopInstance, base.gameObject, true);
			this._taserLoopInstance.start();
			return;
		}
		if (!this._taserLoopInstance.isValid())
		{
			return;
		}
		this._taserLoopInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
		this._taserLoopInstance.release();
	}

	// Token: 0x06000A4A RID: 2634 RVA: 0x00029640 File Offset: 0x00027840
	[ClientRpc]
	private void RpcDestroySfx()
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		this.SendRPCInternal("System.Void Taser::RpcDestroySfx()", -1855344908, writer, 0, true);
		NetworkWriterPool.Return(writer);
	}

	// Token: 0x06000A4B RID: 2635 RVA: 0x00029670 File Offset: 0x00027870
	[ClientRpc]
	private void RpcTaserLoopSfxParams(float charge, bool targetGame, bool callOnHolder)
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		writer.WriteFloat(charge);
		writer.WriteBool(targetGame);
		writer.WriteBool(callOnHolder);
		this.SendRPCInternal("System.Void Taser::RpcTaserLoopSfxParams(System.Single,System.Boolean,System.Boolean)", -365983546, writer, 0, true);
		NetworkWriterPool.Return(writer);
	}

	// Token: 0x06000A4C RID: 2636 RVA: 0x000296C0 File Offset: 0x000278C0
	private void TaserLoopSfxParams(float charge, bool targetGame)
	{
		if (!this._taserLoopInstance.isValid())
		{
			return;
		}
		PLAYBACK_STATE playback_STATE;
		this._taserLoopInstance.getPlaybackState(out playback_STATE);
		if (playback_STATE == PLAYBACK_STATE.PLAYING)
		{
			this._taserLoopInstance.setPitch(1f + charge * 0.5f);
			this._taserLoopInstance.setParameterByName("bool", targetGame ? 1f : 0f, false);
		}
	}

	// Token: 0x06000A50 RID: 2640 RVA: 0x00002321 File Offset: 0x00000521
	public override bool Weaved()
	{
		return true;
	}

	// Token: 0x170000E9 RID: 233
	// (get) Token: 0x06000A51 RID: 2641 RVA: 0x000297A4 File Offset: 0x000279A4
	// (set) Token: 0x06000A52 RID: 2642 RVA: 0x000297C3 File Offset: 0x000279C3
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

	// Token: 0x06000A53 RID: 2643 RVA: 0x000297E4 File Offset: 0x000279E4
	protected void UserCode_RpcOnDropped()
	{
		this.Network_targetGame = null;
		this.modelTransform.DOKill(false);
		this.modelTransform.localPosition = Vector3.zero;
		this.modelTransform.localRotation = Quaternion.identity;
		this.TaserLoopSfx(false);
		this.anim.SetFloat("Blend", 0f);
		this.anim.Update(0f);
	}

	// Token: 0x06000A54 RID: 2644 RVA: 0x00029851 File Offset: 0x00027A51
	protected static void InvokeUserCode_RpcOnDropped(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcOnDropped called on server.");
			return;
		}
		((Taser)obj).UserCode_RpcOnDropped();
	}

	// Token: 0x06000A55 RID: 2645 RVA: 0x00029874 File Offset: 0x00027A74
	protected void UserCode_CmdSetTargetGame__GameBase(GameBase targetGame)
	{
		this.Network_targetGame = targetGame;
		this.RpcSetTargetGame(targetGame);
		bool flag = targetGame;
		this.RpcUseTaser(flag);
		this.RpcTaserLoopSfxParams(this._currentCharge, flag, false);
	}

	// Token: 0x06000A56 RID: 2646 RVA: 0x000298AB File Offset: 0x00027AAB
	protected static void InvokeUserCode_CmdSetTargetGame__GameBase(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdSetTargetGame called on client.");
			return;
		}
		((Taser)obj).UserCode_CmdSetTargetGame__GameBase(reader.ReadNetworkBehaviour<GameBase>());
	}

	// Token: 0x06000A57 RID: 2647 RVA: 0x000298D4 File Offset: 0x00027AD4
	protected void UserCode_RpcSetTargetGame__GameBase(GameBase targetGame)
	{
		if (!base.NetworkHolder || base.NetworkHolder.isLocalPlayer)
		{
			return;
		}
		this.Network_targetGame = targetGame;
	}

	// Token: 0x06000A58 RID: 2648 RVA: 0x000298F8 File Offset: 0x00027AF8
	protected static void InvokeUserCode_RpcSetTargetGame__GameBase(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcSetTargetGame called on server.");
			return;
		}
		((Taser)obj).UserCode_RpcSetTargetGame__GameBase(reader.ReadNetworkBehaviour<GameBase>());
	}

	// Token: 0x06000A59 RID: 2649 RVA: 0x00029921 File Offset: 0x00027B21
	protected void UserCode_RpcUseTaser__Boolean(bool isApplied)
	{
		if (!base.NetworkHolder || base.NetworkHolder.isLocalPlayer)
		{
			return;
		}
		this.UseTaser(isApplied);
	}

	// Token: 0x06000A5A RID: 2650 RVA: 0x00029945 File Offset: 0x00027B45
	protected static void InvokeUserCode_RpcUseTaser__Boolean(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcUseTaser called on server.");
			return;
		}
		((Taser)obj).UserCode_RpcUseTaser__Boolean(reader.ReadBool());
	}

	// Token: 0x06000A5B RID: 2651 RVA: 0x0002996E File Offset: 0x00027B6E
	protected void UserCode_RpcSetCurrentCharge__Single(float charge)
	{
		if (base.isServer)
		{
			return;
		}
		this.SetCurrentCharge(charge);
	}

	// Token: 0x06000A5C RID: 2652 RVA: 0x00029980 File Offset: 0x00027B80
	protected static void InvokeUserCode_RpcSetCurrentCharge__Single(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcSetCurrentCharge called on server.");
			return;
		}
		((Taser)obj).UserCode_RpcSetCurrentCharge__Single(reader.ReadFloat());
	}

	// Token: 0x06000A5D RID: 2653 RVA: 0x000299AA File Offset: 0x00027BAA
	protected void UserCode_CmdTaserFeedbacks__Boolean(bool isPressed)
	{
		this.RpcTaserFeedbacks(isPressed);
	}

	// Token: 0x06000A5E RID: 2654 RVA: 0x000299B3 File Offset: 0x00027BB3
	protected static void InvokeUserCode_CmdTaserFeedbacks__Boolean(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdTaserFeedbacks called on client.");
			return;
		}
		((Taser)obj).UserCode_CmdTaserFeedbacks__Boolean(reader.ReadBool());
	}

	// Token: 0x06000A5F RID: 2655 RVA: 0x000299DC File Offset: 0x00027BDC
	protected void UserCode_RpcTaserFeedbacks__Boolean(bool isPressed)
	{
		if (!base.NetworkHolder || base.NetworkHolder.isLocalPlayer)
		{
			return;
		}
		this.TaserFeedbacks(isPressed);
	}

	// Token: 0x06000A60 RID: 2656 RVA: 0x00029A00 File Offset: 0x00027C00
	protected static void InvokeUserCode_RpcTaserFeedbacks__Boolean(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcTaserFeedbacks called on server.");
			return;
		}
		((Taser)obj).UserCode_RpcTaserFeedbacks__Boolean(reader.ReadBool());
	}

	// Token: 0x06000A61 RID: 2657 RVA: 0x00029A29 File Offset: 0x00027C29
	protected void UserCode_RpcDestroySfx()
	{
		SFXManager.SFXOneShot(this.destroySfx, base.transform.position);
	}

	// Token: 0x06000A62 RID: 2658 RVA: 0x00029A41 File Offset: 0x00027C41
	protected static void InvokeUserCode_RpcDestroySfx(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcDestroySfx called on server.");
			return;
		}
		((Taser)obj).UserCode_RpcDestroySfx();
	}

	// Token: 0x06000A63 RID: 2659 RVA: 0x00029A64 File Offset: 0x00027C64
	protected void UserCode_RpcTaserLoopSfxParams__Single__Boolean__Boolean(float charge, bool targetGame, bool callOnHolder)
	{
		if (!base.NetworkHolder || (!callOnHolder && base.NetworkHolder.isLocalPlayer))
		{
			return;
		}
		this.TaserLoopSfxParams(charge, targetGame);
	}

	// Token: 0x06000A64 RID: 2660 RVA: 0x00029A8C File Offset: 0x00027C8C
	protected static void InvokeUserCode_RpcTaserLoopSfxParams__Single__Boolean__Boolean(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcTaserLoopSfxParams called on server.");
			return;
		}
		((Taser)obj).UserCode_RpcTaserLoopSfxParams__Single__Boolean__Boolean(reader.ReadFloat(), reader.ReadBool(), reader.ReadBool());
	}

	// Token: 0x06000A65 RID: 2661 RVA: 0x00029AC4 File Offset: 0x00027CC4
	static Taser()
	{
		RemoteProcedureCalls.RegisterCommand(typeof(Taser), "System.Void Taser::CmdSetTargetGame(GameBase)", new RemoteCallDelegate(Taser.InvokeUserCode_CmdSetTargetGame__GameBase), false);
		RemoteProcedureCalls.RegisterCommand(typeof(Taser), "System.Void Taser::CmdTaserFeedbacks(System.Boolean)", new RemoteCallDelegate(Taser.InvokeUserCode_CmdTaserFeedbacks__Boolean), false);
		RemoteProcedureCalls.RegisterRpc(typeof(Taser), "System.Void Taser::RpcOnDropped()", new RemoteCallDelegate(Taser.InvokeUserCode_RpcOnDropped));
		RemoteProcedureCalls.RegisterRpc(typeof(Taser), "System.Void Taser::RpcSetTargetGame(GameBase)", new RemoteCallDelegate(Taser.InvokeUserCode_RpcSetTargetGame__GameBase));
		RemoteProcedureCalls.RegisterRpc(typeof(Taser), "System.Void Taser::RpcUseTaser(System.Boolean)", new RemoteCallDelegate(Taser.InvokeUserCode_RpcUseTaser__Boolean));
		RemoteProcedureCalls.RegisterRpc(typeof(Taser), "System.Void Taser::RpcSetCurrentCharge(System.Single)", new RemoteCallDelegate(Taser.InvokeUserCode_RpcSetCurrentCharge__Single));
		RemoteProcedureCalls.RegisterRpc(typeof(Taser), "System.Void Taser::RpcTaserFeedbacks(System.Boolean)", new RemoteCallDelegate(Taser.InvokeUserCode_RpcTaserFeedbacks__Boolean));
		RemoteProcedureCalls.RegisterRpc(typeof(Taser), "System.Void Taser::RpcDestroySfx()", new RemoteCallDelegate(Taser.InvokeUserCode_RpcDestroySfx));
		RemoteProcedureCalls.RegisterRpc(typeof(Taser), "System.Void Taser::RpcTaserLoopSfxParams(System.Single,System.Boolean,System.Boolean)", new RemoteCallDelegate(Taser.InvokeUserCode_RpcTaserLoopSfxParams__Single__Boolean__Boolean));
	}

	// Token: 0x06000A66 RID: 2662 RVA: 0x00029BF4 File Offset: 0x00027DF4
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

	// Token: 0x06000A67 RID: 2663 RVA: 0x00029C4C File Offset: 0x00027E4C
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

	// Token: 0x04000673 RID: 1651
	[Header("References")]
	[SerializeField]
	private Transform chargeBar;

	// Token: 0x04000674 RID: 1652
	[SerializeField]
	private ParticleSystem lightningParticles;

	// Token: 0x04000675 RID: 1653
	[SerializeField]
	private Animator anim;

	// Token: 0x04000676 RID: 1654
	[Header("Settings")]
	[SerializeField]
	private float chargeTick = 0.5f;

	// Token: 0x04000677 RID: 1655
	[SerializeField]
	private float chargeUsePerTick = 0.05f;

	// Token: 0x04000678 RID: 1656
	[SerializeField]
	private float totalMultiplierIncrease = 4f;

	// Token: 0x04000679 RID: 1657
	[SerializeField]
	private float raycastDistance = 2f;

	// Token: 0x0400067A RID: 1658
	[SerializeField]
	private LayerMask rayMask;

	// Token: 0x0400067B RID: 1659
	[Header("SFX")]
	[SerializeField]
	private EventReference taserShootSfx;

	// Token: 0x0400067C RID: 1660
	[SerializeField]
	private EventReference taserInvalidSfx;

	// Token: 0x0400067D RID: 1661
	private EventInstance _taserLoopInstance;

	// Token: 0x0400067E RID: 1662
	[SerializeField]
	private EventReference destroySfx;

	// Token: 0x0400067F RID: 1663
	private PlayerProfile _holderProfile;

	// Token: 0x04000680 RID: 1664
	[SyncVar]
	private GameBase _targetGame;

	// Token: 0x04000681 RID: 1665
	private Coroutine _setTargetGameRoutine;

	// Token: 0x04000682 RID: 1666
	private readonly RaycastHit[] _raycastHits = new RaycastHit[8];

	// Token: 0x04000683 RID: 1667
	private float _currentCharge = 1f;

	// Token: 0x04000684 RID: 1668
	private float _lastChargeTime;

	// Token: 0x04000685 RID: 1669
	protected NetworkBehaviourSyncVar ____targetGameNetId;
}
