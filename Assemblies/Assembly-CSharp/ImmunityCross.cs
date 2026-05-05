using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using Extensions;
using Mirror;
using Mirror.RemoteCalls;
using UnityEngine;

// Token: 0x020000EF RID: 239
public class ImmunityCross : ConsumableItem
{
	// Token: 0x060009A8 RID: 2472 RVA: 0x000269BC File Offset: 0x00024BBC
	private void Update()
	{
		if (!this._isActive)
		{
			return;
		}
		if (!base.NetworkHolder)
		{
			return;
		}
		if (base.isServer)
		{
			this.Network_usedDuration = this._usedDuration + Time.deltaTime / NetworkSingleton<UpgradeManager>.Instance.GetUpgradeData(this._holderProfile.steamId, PlayerUpgradeType.Stakeholder);
			if (this._usedDuration >= this.duration)
			{
				this._hasEnded = true;
				foreach (PlayerBuff playerBuff in this._buffs)
				{
					playerBuff.ResetBuffArea(PlayerBuffType.Immunity, this);
				}
				this._buffs.Clear();
				this.loopComponent.RpcLoopSFX(false);
				this.onDestroySfx.RpcPlayOneShotWith3DPos();
				base.DestroyItem();
				return;
			}
		}
		float num = Mathf.InverseLerp(this.duration, 0f, this._usedDuration);
		if (!DOTween.IsTweening(this.statueTransform, false))
		{
			this.statueTransform.localPosition = new Vector3(0f, 1f * num, 0.4f * num);
		}
		Color value = Color.Lerp(Color.black, Color.white, Mathf.SmoothStep(0f, 1f, num));
		MeshRenderer[] array = this.meshRenderers;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].material.SetColor("_Color", value);
		}
	}

	// Token: 0x060009A9 RID: 2473 RVA: 0x00026B2C File Offset: 0x00024D2C
	protected override void OnUseItem(bool isPressed)
	{
		if (this._hasEnded)
		{
			return;
		}
		this._isActive = isPressed;
		if (base.isServer)
		{
			if (this._buffs.Count != MonoSingleton<LocalManager>.Instance.players.Count)
			{
				this._buffs.Clear();
				foreach (PlayerReferences playerReferences in MonoSingleton<LocalManager>.Instance.players)
				{
					this._buffs.Add(playerReferences.buff);
				}
			}
			foreach (PlayerBuff playerBuff in this._buffs)
			{
				BuffArea area = new BuffArea
				{
					Source = base.transform,
					Range = this.radius,
					Amount = 1f,
					IsActive = this._isActive
				};
				playerBuff.SetBuffArea(PlayerBuffType.Immunity, this, area);
			}
		}
		this.loopComponent.LoopSFX(this._isActive);
		float endValue = this._isActive ? 1f : 0f;
		DOTween.To(() => this.anim.GetFloat("Blend"), delegate(float x)
		{
			this.anim.SetFloat("Blend", x);
		}, endValue, 0.25f).SetEase(Ease.OutCubic);
		float num = Mathf.InverseLerp(this.duration, 0f, this._usedDuration);
		this.statueTransform.DOLocalMove(this._isActive ? new Vector3(0f, 1f * num, 0.4f * num) : Vector3.zero, 0.2f, false).SetEase(Ease.OutQuad);
		this.areaTransform.DOScale(isPressed ? (Vector3.one * this.radius) : Vector3.zero, 0.1f).SetEase(Ease.OutQuad);
	}

	// Token: 0x060009AA RID: 2474 RVA: 0x00026D24 File Offset: 0x00024F24
	protected override void OnPickedUp(PlayerInventory playerInventory)
	{
		base.OnPickedUp(playerInventory);
		if (this._hasEnded)
		{
			return;
		}
		this._holderProfile = playerInventory.GetComponent<PlayerProfile>();
		this._buffs.Clear();
		foreach (PlayerReferences playerReferences in MonoSingleton<LocalManager>.Instance.players)
		{
			this._buffs.Add(playerReferences.buff);
		}
		playerInventory.TryGetComponent<PlayerVoiceFX>(out this._playerVoiceFX);
	}

	// Token: 0x060009AB RID: 2475 RVA: 0x00026DBC File Offset: 0x00024FBC
	protected override void OnDropped(PlayerInventory playerInventory)
	{
		base.OnDropped(playerInventory);
		this._isActive = false;
		foreach (PlayerBuff playerBuff in this._buffs)
		{
			playerBuff.ResetBuffArea(PlayerBuffType.Immunity, this);
		}
		this._buffs.Clear();
		this.ResetLocal();
		this.RpcReset();
		if (this._playerVoiceFX)
		{
			this._playerVoiceFX.RpcResetVoiceFX();
			this._playerVoiceFX = null;
		}
	}

	// Token: 0x060009AC RID: 2476 RVA: 0x00026E54 File Offset: 0x00025054
	protected override void OnHolderChanged(PlayerInventory oldHolder, PlayerInventory newHolder)
	{
		base.OnHolderChanged(oldHolder, newHolder);
		if (!newHolder)
		{
			this.ResetLocal();
		}
	}

	// Token: 0x060009AD RID: 2477 RVA: 0x00026E6C File Offset: 0x0002506C
	[ClientRpc]
	private void RpcReset()
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		this.SendRPCInternal("System.Void ImmunityCross::RpcReset()", 665502891, writer, 0, true);
		NetworkWriterPool.Return(writer);
	}

	// Token: 0x060009AE RID: 2478 RVA: 0x00026E9C File Offset: 0x0002509C
	private void ResetLocal()
	{
		this.anim.SetFloat("Blend", 0f);
		this.anim.Update(0f);
		this.statueTransform.DOKill(false);
		this.statueTransform.localPosition = Vector3.zero;
		this.areaTransform.DOScale(0f, 0.1f).SetEase(Ease.OutQuad);
		this.loopComponent.LoopSFX(false);
	}

	// Token: 0x060009B2 RID: 2482 RVA: 0x00002321 File Offset: 0x00000521
	public override bool Weaved()
	{
		return true;
	}

	// Token: 0x170000DE RID: 222
	// (get) Token: 0x060009B3 RID: 2483 RVA: 0x00026F4C File Offset: 0x0002514C
	// (set) Token: 0x060009B4 RID: 2484 RVA: 0x00026F5F File Offset: 0x0002515F
	public float Network_usedDuration
	{
		get
		{
			return this._usedDuration;
		}
		[param: In]
		set
		{
			base.GeneratedSyncVarSetter<float>(value, ref this._usedDuration, 2UL, null);
		}
	}

	// Token: 0x060009B5 RID: 2485 RVA: 0x00026F79 File Offset: 0x00025179
	protected void UserCode_RpcReset()
	{
		if (base.isServer)
		{
			return;
		}
		this.ResetLocal();
	}

	// Token: 0x060009B6 RID: 2486 RVA: 0x00026F8A File Offset: 0x0002518A
	protected static void InvokeUserCode_RpcReset(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcReset called on server.");
			return;
		}
		((ImmunityCross)obj).UserCode_RpcReset();
	}

	// Token: 0x060009B7 RID: 2487 RVA: 0x00026FAD File Offset: 0x000251AD
	static ImmunityCross()
	{
		RemoteProcedureCalls.RegisterRpc(typeof(ImmunityCross), "System.Void ImmunityCross::RpcReset()", new RemoteCallDelegate(ImmunityCross.InvokeUserCode_RpcReset));
	}

	// Token: 0x060009B8 RID: 2488 RVA: 0x00026FD0 File Offset: 0x000251D0
	public override void SerializeSyncVars(NetworkWriter writer, bool forceAll)
	{
		base.SerializeSyncVars(writer, forceAll);
		if (forceAll)
		{
			writer.WriteFloat(this._usedDuration);
			return;
		}
		writer.WriteVarULong(this.syncVarDirtyBits);
		if ((this.syncVarDirtyBits & 2UL) != 0UL)
		{
			writer.WriteFloat(this._usedDuration);
		}
	}

	// Token: 0x060009B9 RID: 2489 RVA: 0x00027028 File Offset: 0x00025228
	public override void DeserializeSyncVars(NetworkReader reader, bool initialState)
	{
		base.DeserializeSyncVars(reader, initialState);
		if (initialState)
		{
			base.GeneratedSyncVarDeserialize<float>(ref this._usedDuration, null, reader.ReadFloat());
			return;
		}
		long num = (long)reader.ReadVarULong();
		if ((num & 2L) != 0L)
		{
			base.GeneratedSyncVarDeserialize<float>(ref this._usedDuration, null, reader.ReadFloat());
		}
	}

	// Token: 0x04000619 RID: 1561
	[Header("Settings")]
	[SerializeField]
	private float radius;

	// Token: 0x0400061A RID: 1562
	[SerializeField]
	private float duration;

	// Token: 0x0400061B RID: 1563
	[Header("References")]
	[SerializeField]
	private Transform statueTransform;

	// Token: 0x0400061C RID: 1564
	[SerializeField]
	private Transform areaTransform;

	// Token: 0x0400061D RID: 1565
	[SerializeField]
	private Animator anim;

	// Token: 0x0400061E RID: 1566
	[Header("SFX")]
	[SerializeField]
	private SFXComponent onDestroySfx;

	// Token: 0x0400061F RID: 1567
	private PlayerVoiceFX _playerVoiceFX;

	// Token: 0x04000620 RID: 1568
	[SerializeField]
	private SFXLoopComponent loopComponent;

	// Token: 0x04000621 RID: 1569
	private bool _isActive;

	// Token: 0x04000622 RID: 1570
	private bool _hasEnded;

	// Token: 0x04000623 RID: 1571
	[SyncVar]
	private float _usedDuration;

	// Token: 0x04000624 RID: 1572
	private PlayerProfile _holderProfile;

	// Token: 0x04000625 RID: 1573
	private readonly List<PlayerBuff> _buffs = new List<PlayerBuff>();

	// Token: 0x04000626 RID: 1574
	[SerializeField]
	private MeshRenderer[] meshRenderers;
}
