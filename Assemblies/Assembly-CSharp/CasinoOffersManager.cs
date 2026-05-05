using System;
using System.Runtime.InteropServices;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using Extensions;
using Mirror;
using TMPro;
using UnityEngine;

// Token: 0x02000294 RID: 660
public class CasinoOffersManager : NetworkSingleton<CasinoOffersManager>
{
	// Token: 0x0600176E RID: 5998 RVA: 0x0006306F File Offset: 0x0006126F
	private void Start()
	{
		this.UpdateText(0, 0);
	}

	// Token: 0x0600176F RID: 5999 RVA: 0x0006307C File Offset: 0x0006127C
	[Server]
	public void ServerCheckBlackjackOffer(bool isWin, int playerHand, int dealerHand)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void CasinoOffersManager::ServerCheckBlackjackOffer(System.Boolean,System.Int32,System.Int32)' called when server was not active");
			return;
		}
		if (!isWin && playerHand >= this.blackjackMinHandRequirement && playerHand < 21)
		{
			this.NetworkcurrentBlackjackGoalCount = this.currentBlackjackGoalCount + 1;
			if (this.currentBlackjackGoalCount >= this.blackjackGoalCountMax)
			{
				this.NetworkcurrentBlackjackGoalCount = 0;
			}
		}
	}

	// Token: 0x06001770 RID: 6000 RVA: 0x000630D4 File Offset: 0x000612D4
	[Server]
	public void ServerCheckSlotsJackpotOffer(bool isWin, int betAmount)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void CasinoOffersManager::ServerCheckSlotsJackpotOffer(System.Boolean,System.Int32)' called when server was not active");
			return;
		}
		if (isWin)
		{
			int num = this.currentLoseForJackpot;
			int num2 = this.slotsMaxLoseForJackpot;
			this.NetworkcurrentLoseForJackpot = 0;
			this.NetworkslotsJackpotReward = 0;
			return;
		}
		this.NetworkcurrentLoseForJackpot = this.currentLoseForJackpot + 1;
		if (this.currentLoseForJackpot <= this.slotsMaxLoseForJackpot)
		{
			this.NetworkslotsJackpotReward = this.slotsJackpotReward + (betAmount + betAmount * this.currentLoseForJackpot);
			return;
		}
		this.NetworkcurrentLoseForJackpot = 0;
		this.NetworkslotsJackpotReward = 0;
	}

	// Token: 0x06001771 RID: 6001 RVA: 0x00063158 File Offset: 0x00061358
	private void UpdateText(int oldValue = 0, int newValue = 0)
	{
		this.goalText.text = string.Format("- [Blackjack] Lose {0} times ({1}/{2}) with a hand of {3}+, earn ${4}", new object[]
		{
			this.blackjackGoalCountMax,
			this.currentBlackjackGoalCount,
			this.blackjackGoalCountMax,
			this.blackjackMinHandRequirement,
			this.blackjackOfferReward
		});
		TextMeshProUGUI textMeshProUGUI = this.goalText;
		textMeshProUGUI.text += string.Format("\n- [Slots] Lose {0} times consecutively ({1}/{2}) to increase the jackpot (${3})", new object[]
		{
			this.slotsMaxLoseForJackpot,
			this.currentLoseForJackpot,
			this.slotsMaxLoseForJackpot,
			this.slotsJackpotReward
		});
		this.goalText.DOFade(1f, 0.5f).SetEase(Ease.OutCubic).OnComplete(delegate
		{
			this.goalText.DOFade(0.1f, 2.5f).SetEase(Ease.OutCubic);
		});
	}

	// Token: 0x06001772 RID: 6002 RVA: 0x00063254 File Offset: 0x00061454
	public CasinoOffersManager()
	{
		this._Mirror_SyncVarHookDelegate_currentBlackjackGoalCount = new Action<int, int>(this.UpdateText);
		this._Mirror_SyncVarHookDelegate_currentLoseForJackpot = new Action<int, int>(this.UpdateText);
		this._Mirror_SyncVarHookDelegate_slotsJackpotReward = new Action<int, int>(this.UpdateText);
	}

	// Token: 0x06001774 RID: 6004 RVA: 0x00002321 File Offset: 0x00000521
	public override bool Weaved()
	{
		return true;
	}

	// Token: 0x17000215 RID: 533
	// (get) Token: 0x06001775 RID: 6005 RVA: 0x000632E0 File Offset: 0x000614E0
	// (set) Token: 0x06001776 RID: 6006 RVA: 0x000632F3 File Offset: 0x000614F3
	public int NetworkcurrentBlackjackGoalCount
	{
		get
		{
			return this.currentBlackjackGoalCount;
		}
		[param: In]
		set
		{
			base.GeneratedSyncVarSetter<int>(value, ref this.currentBlackjackGoalCount, 1UL, this._Mirror_SyncVarHookDelegate_currentBlackjackGoalCount);
		}
	}

	// Token: 0x17000216 RID: 534
	// (get) Token: 0x06001777 RID: 6007 RVA: 0x00063314 File Offset: 0x00061514
	// (set) Token: 0x06001778 RID: 6008 RVA: 0x00063327 File Offset: 0x00061527
	public int NetworkcurrentLoseForJackpot
	{
		get
		{
			return this.currentLoseForJackpot;
		}
		[param: In]
		set
		{
			base.GeneratedSyncVarSetter<int>(value, ref this.currentLoseForJackpot, 2UL, this._Mirror_SyncVarHookDelegate_currentLoseForJackpot);
		}
	}

	// Token: 0x17000217 RID: 535
	// (get) Token: 0x06001779 RID: 6009 RVA: 0x00063348 File Offset: 0x00061548
	// (set) Token: 0x0600177A RID: 6010 RVA: 0x0006335B File Offset: 0x0006155B
	public int NetworkslotsJackpotReward
	{
		get
		{
			return this.slotsJackpotReward;
		}
		[param: In]
		set
		{
			base.GeneratedSyncVarSetter<int>(value, ref this.slotsJackpotReward, 4UL, this._Mirror_SyncVarHookDelegate_slotsJackpotReward);
		}
	}

	// Token: 0x0600177B RID: 6011 RVA: 0x0006337C File Offset: 0x0006157C
	public override void SerializeSyncVars(NetworkWriter writer, bool forceAll)
	{
		base.SerializeSyncVars(writer, forceAll);
		if (forceAll)
		{
			writer.WriteVarInt(this.currentBlackjackGoalCount);
			writer.WriteVarInt(this.currentLoseForJackpot);
			writer.WriteVarInt(this.slotsJackpotReward);
			return;
		}
		writer.WriteVarULong(this.syncVarDirtyBits);
		if ((this.syncVarDirtyBits & 1UL) != 0UL)
		{
			writer.WriteVarInt(this.currentBlackjackGoalCount);
		}
		if ((this.syncVarDirtyBits & 2UL) != 0UL)
		{
			writer.WriteVarInt(this.currentLoseForJackpot);
		}
		if ((this.syncVarDirtyBits & 4UL) != 0UL)
		{
			writer.WriteVarInt(this.slotsJackpotReward);
		}
	}

	// Token: 0x0600177C RID: 6012 RVA: 0x00063430 File Offset: 0x00061630
	public override void DeserializeSyncVars(NetworkReader reader, bool initialState)
	{
		base.DeserializeSyncVars(reader, initialState);
		if (initialState)
		{
			base.GeneratedSyncVarDeserialize<int>(ref this.currentBlackjackGoalCount, this._Mirror_SyncVarHookDelegate_currentBlackjackGoalCount, reader.ReadVarInt());
			base.GeneratedSyncVarDeserialize<int>(ref this.currentLoseForJackpot, this._Mirror_SyncVarHookDelegate_currentLoseForJackpot, reader.ReadVarInt());
			base.GeneratedSyncVarDeserialize<int>(ref this.slotsJackpotReward, this._Mirror_SyncVarHookDelegate_slotsJackpotReward, reader.ReadVarInt());
			return;
		}
		long num = (long)reader.ReadVarULong();
		if ((num & 1L) != 0L)
		{
			base.GeneratedSyncVarDeserialize<int>(ref this.currentBlackjackGoalCount, this._Mirror_SyncVarHookDelegate_currentBlackjackGoalCount, reader.ReadVarInt());
		}
		if ((num & 2L) != 0L)
		{
			base.GeneratedSyncVarDeserialize<int>(ref this.currentLoseForJackpot, this._Mirror_SyncVarHookDelegate_currentLoseForJackpot, reader.ReadVarInt());
		}
		if ((num & 4L) != 0L)
		{
			base.GeneratedSyncVarDeserialize<int>(ref this.slotsJackpotReward, this._Mirror_SyncVarHookDelegate_slotsJackpotReward, reader.ReadVarInt());
		}
	}

	// Token: 0x04000F30 RID: 3888
	[Header("Blackjack Offer Settings")]
	[SerializeField]
	public int blackjackGoalCountMax = 3;

	// Token: 0x04000F31 RID: 3889
	[SerializeField]
	[SyncVar(hook = "UpdateText")]
	public int currentBlackjackGoalCount;

	// Token: 0x04000F32 RID: 3890
	[SerializeField]
	public int blackjackMinHandRequirement = 18;

	// Token: 0x04000F33 RID: 3891
	[SerializeField]
	public int blackjackOfferReward = 1000;

	// Token: 0x04000F34 RID: 3892
	[SerializeField]
	private TextMeshProUGUI goalText;

	// Token: 0x04000F35 RID: 3893
	[Header("Slots Offer Settings")]
	[SerializeField]
	public int slotsMaxLoseForJackpot = 5;

	// Token: 0x04000F36 RID: 3894
	[SerializeField]
	[SyncVar(hook = "UpdateText")]
	public int currentLoseForJackpot;

	// Token: 0x04000F37 RID: 3895
	[SerializeField]
	[SyncVar(hook = "UpdateText")]
	public int slotsJackpotReward;

	// Token: 0x04000F38 RID: 3896
	public Action<int, int> _Mirror_SyncVarHookDelegate_currentBlackjackGoalCount;

	// Token: 0x04000F39 RID: 3897
	public Action<int, int> _Mirror_SyncVarHookDelegate_currentLoseForJackpot;

	// Token: 0x04000F3A RID: 3898
	public Action<int, int> _Mirror_SyncVarHookDelegate_slotsJackpotReward;
}
