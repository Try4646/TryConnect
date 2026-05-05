using System;
using System.Runtime.InteropServices;
using Extensions;
using Mirror;
using TMPro;
using UnityEngine;

// Token: 0x0200005A RID: 90
public class Bank : NetworkBehaviour
{
	// Token: 0x060002B4 RID: 692 RVA: 0x0000DF41 File Offset: 0x0000C141
	private void Awake()
	{
		if (this.knob != null)
		{
			BankKnob bankKnob = this.knob;
			bankKnob.OnKnobValueChanged = (Action<float>)Delegate.Combine(bankKnob.OnKnobValueChanged, new Action<float>(this.HandleKnobValueChanged));
		}
	}

	// Token: 0x060002B5 RID: 693 RVA: 0x0000DF78 File Offset: 0x0000C178
	private void Start()
	{
		this.UpdateLastModificationLabel(this.lastModificationPercent);
		if (this.bankTier == null && this.bankTierNumber > 0)
		{
			this.LoadBankTier(this.bankTierNumber);
		}
	}

	// Token: 0x060002B6 RID: 694 RVA: 0x0000DFA9 File Offset: 0x0000C1A9
	private void OnBankTierNumberChanged(int oldValue, int newValue)
	{
		this.LoadBankTier(newValue);
	}

	// Token: 0x060002B7 RID: 695 RVA: 0x0000DFB4 File Offset: 0x0000C1B4
	private void LoadBankTier(int tierNumber)
	{
		BankTier x = Resources.Load<BankTier>(string.Format("Tier_{0}", tierNumber));
		if (x != null)
		{
			this.bankTier = x;
		}
	}

	// Token: 0x060002B8 RID: 696 RVA: 0x0000DFE7 File Offset: 0x0000C1E7
	private void Update()
	{
		if (base.isServer && this.bankBalance > 0L)
		{
			this.ProcessMarketModification();
		}
		this.UpdateMarketStatusLabel();
	}

	// Token: 0x060002B9 RID: 697 RVA: 0x0000E008 File Offset: 0x0000C208
	private void ProcessMarketModification()
	{
		double time = NetworkTime.time;
		if (this.nextModificationTime == 0f || this.bankBalance == 0L)
		{
			return;
		}
		if (time >= (double)this.nextModificationTime)
		{
			this.ApplyMarketModification((float)time);
		}
	}

	// Token: 0x060002BA RID: 698 RVA: 0x0000E044 File Offset: 0x0000C244
	private void ApplyMarketModification(float currentTime)
	{
		float randomModification = this.bankTier.GetRandomModification();
		double d = (double)((float)this.bankBalance * randomModification);
		this.NetworkbankBalance = Math.Max(0L, (long)Math.Floor(d));
		this.NetworklastModificationPercent = (randomModification - 1f) * 100f;
		this.NetworklastModificationTime = currentTime;
		this.NetworknextModificationTime = currentTime + ((this.bankTier != null) ? this.bankTier.modificationInterval : 60f);
		this.NotifyBankBalanceChanged();
	}

	// Token: 0x060002BB RID: 699 RVA: 0x0000E0C4 File Offset: 0x0000C2C4
	private void OnBankBalanceChanged(long oldValue, long newValue)
	{
		this.UpdateBankBalanceLabel(newValue);
		Action<long> onBankBalanceChangedEvent = this.OnBankBalanceChangedEvent;
		if (onBankBalanceChangedEvent != null)
		{
			onBankBalanceChangedEvent(newValue);
		}
		this.UpdateMaxKnobValue();
	}

	// Token: 0x060002BC RID: 700 RVA: 0x0000E0E5 File Offset: 0x0000C2E5
	private void OnModeChanged(BankMode oldMode, BankMode newMode)
	{
		this.UpdateModeLabel(newMode);
		Action<BankMode> onModeChangedEvent = this.OnModeChangedEvent;
		if (onModeChangedEvent != null)
		{
			onModeChangedEvent(newMode);
		}
		this.UpdateMaxKnobValue();
		this.NetworkselectedAmount = 0L;
	}

	// Token: 0x060002BD RID: 701 RVA: 0x0000E10E File Offset: 0x0000C30E
	private void OnSelectedAmountChanged(long oldValue, long newValue)
	{
		this.UpdateSelectedAmountLabel(newValue);
		Action<long> onSelectedAmountChangedEvent = this.OnSelectedAmountChangedEvent;
		if (onSelectedAmountChangedEvent == null)
		{
			return;
		}
		onSelectedAmountChangedEvent(newValue);
	}

	// Token: 0x060002BE RID: 702 RVA: 0x000048A7 File Offset: 0x00002AA7
	private void OnLastModificationTimeChanged(float oldValue, float newValue)
	{
	}

	// Token: 0x060002BF RID: 703 RVA: 0x000048A7 File Offset: 0x00002AA7
	private void OnNextModificationTimeChanged(float oldValue, float newValue)
	{
	}

	// Token: 0x060002C0 RID: 704 RVA: 0x0000E128 File Offset: 0x0000C328
	private void OnLastModificationPercentChanged(float oldValue, float newValue)
	{
		this.UpdateLastModificationLabel(newValue);
	}

	// Token: 0x060002C1 RID: 705 RVA: 0x000048A7 File Offset: 0x00002AA7
	private void OnLastDepositTimeChanged(float oldValue, float newValue)
	{
	}

	// Token: 0x060002C2 RID: 706 RVA: 0x0000E134 File Offset: 0x0000C334
	private void HandleKnobValueChanged(float normalizedValue)
	{
		if (!base.isServer)
		{
			return;
		}
		long maxAmount = this.GetMaxAmount();
		long minAmount = this.GetMinAmount();
		long num = maxAmount - minAmount;
		if (num <= 0L)
		{
			this.NetworkselectedAmount = minAmount;
			return;
		}
		long num2 = minAmount + (long)Math.Round((double)(normalizedValue * (float)num));
		num2 = (long)Math.Round((double)num2 / (double)this.stepAmount) * (long)this.stepAmount;
		num2 = Math.Max(minAmount, Math.Min(num2, maxAmount));
		this.NetworkselectedAmount = num2;
	}

	// Token: 0x060002C3 RID: 707 RVA: 0x0000E1A5 File Offset: 0x0000C3A5
	private long GetMinAmount()
	{
		if (this.currentMode != BankMode.Put)
		{
			return 1L;
		}
		if (this.bankTier != null)
		{
			return this.bankTier.minDepositAmount;
		}
		return 1L;
	}

	// Token: 0x060002C4 RID: 708 RVA: 0x0000E1D0 File Offset: 0x0000C3D0
	private long GetMaxAmount()
	{
		if (this.currentMode == BankMode.Put)
		{
			long val = 0L;
			if (NetworkSingleton<MoneyManager>.Instance != null)
			{
				val = NetworkSingleton<MoneyManager>.Instance.balance;
			}
			long val2 = long.MaxValue;
			if (this.bankTier != null)
			{
				val2 = this.bankTier.maxDepositAmount;
			}
			return Math.Min(val, val2);
		}
		return this.bankBalance;
	}

	// Token: 0x060002C5 RID: 709 RVA: 0x0000E234 File Offset: 0x0000C434
	private void UpdateMaxKnobValue()
	{
		if (this.knob != null)
		{
			long maxAmount = this.GetMaxAmount();
			this.knob.SetMaxValue(maxAmount);
		}
	}

	// Token: 0x060002C6 RID: 710 RVA: 0x0000E264 File Offset: 0x0000C464
	[Server]
	private void SetMode(BankMode mode)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void Bank::SetMode(BankMode)' called when server was not active");
			return;
		}
		this.NetworkcurrentMode = mode;
		this.NetworkselectedAmount = 0L;
		if (this.knob != null)
		{
			this.knob.SetNormalizedValue(0f);
		}
	}

	// Token: 0x060002C7 RID: 711 RVA: 0x0000E2B3 File Offset: 0x0000C4B3
	[Server]
	public void SetModePut(PlayerInteract playerInteract)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void Bank::SetModePut(PlayerInteract)' called when server was not active");
			return;
		}
		this.SetMode(BankMode.Put);
	}

	// Token: 0x060002C8 RID: 712 RVA: 0x0000E2D1 File Offset: 0x0000C4D1
	[Server]
	public void SetModePull(PlayerInteract playerInteract)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void Bank::SetModePull(PlayerInteract)' called when server was not active");
			return;
		}
		this.SetMode(BankMode.Pull);
	}

	// Token: 0x060002C9 RID: 713 RVA: 0x0000E2F0 File Offset: 0x0000C4F0
	[Server]
	public void ConfirmTransaction(PlayerInteract playerInteract)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void Bank::ConfirmTransaction(PlayerInteract)' called when server was not active");
			return;
		}
		if (this.selectedAmount <= 0L)
		{
			return;
		}
		if (this.currentMode == BankMode.Put)
		{
			long minAmount = this.GetMinAmount();
			long maxAmount = this.GetMaxAmount();
			this.NetworkselectedAmount = Math.Max(minAmount, Math.Min(this.selectedAmount, maxAmount));
			if (this.selectedAmount < minAmount)
			{
				return;
			}
		}
		else
		{
			if (!this.CanWithdraw())
			{
				return;
			}
			if (this.bankBalance >= this.selectedAmount)
			{
				this.NetworkbankBalance = this.bankBalance - this.selectedAmount;
				this.NotifyBankBalanceChanged();
				if (this.bankBalance == 0L)
				{
					this.NetworknextModificationTime = 0f;
					this.NetworklastModificationTime = 0f;
					this.NetworklastDepositTime = 0f;
				}
				else
				{
					this.NetworklastDepositTime = 0f;
				}
			}
		}
		this.NetworkselectedAmount = 0L;
		if (this.knob != null)
		{
			this.knob.SetNormalizedValue(0f);
		}
	}

	// Token: 0x060002CA RID: 714 RVA: 0x0000E3E1 File Offset: 0x0000C5E1
	private bool CanWithdraw()
	{
		return this.lastDepositTime == 0f || (this.lastModificationTime > this.lastDepositTime && this.lastModificationTime > 0f);
	}

	// Token: 0x060002CB RID: 715 RVA: 0x0000E410 File Offset: 0x0000C610
	private void NotifyBankBalanceChanged()
	{
		Action<long> onBankBalanceChangedEvent = this.OnBankBalanceChangedEvent;
		if (onBankBalanceChangedEvent == null)
		{
			return;
		}
		onBankBalanceChangedEvent(this.bankBalance);
	}

	// Token: 0x060002CC RID: 716 RVA: 0x0000E428 File Offset: 0x0000C628
	private void UpdateBankBalanceLabel(long value)
	{
		if (this.bankBalanceLabel != null)
		{
			this.bankBalanceLabel.text = string.Format(this.currencyFormat, value);
		}
	}

	// Token: 0x060002CD RID: 717 RVA: 0x0000E454 File Offset: 0x0000C654
	private void UpdateSelectedAmountLabel(long value)
	{
		if (this.selectedAmountLabel != null)
		{
			this.selectedAmountLabel.text = string.Format(this.currencyFormat, value);
		}
	}

	// Token: 0x060002CE RID: 718 RVA: 0x0000E480 File Offset: 0x0000C680
	private void UpdateModeLabel(BankMode mode)
	{
		if (this.modeLabel != null)
		{
			this.modeLabel.text = ((mode == BankMode.Put) ? "Deposit" : "Withdraw");
		}
	}

	// Token: 0x060002CF RID: 719 RVA: 0x0000E4AC File Offset: 0x0000C6AC
	private void UpdateMarketStatusLabel()
	{
		if (this.currentInterestLabel == null)
		{
			return;
		}
		if (this.bankBalance > 0L && this.nextModificationTime > 0f)
		{
			double time = NetworkTime.time;
			float num = (float)((double)this.nextModificationTime - time);
			if (num <= 0f)
			{
				this.currentInterestLabel.text = "Modifying...";
				return;
			}
			int num2 = Mathf.CeilToInt(num);
			int num3 = num2 / 60;
			num2 %= 60;
			if (num3 > 0)
			{
				this.currentInterestLabel.text = string.Format("{0}m {1}s", num3, num2);
				return;
			}
			this.currentInterestLabel.text = string.Format("{0}s", num2);
			return;
		}
		else
		{
			if (this.bankBalance > 0L)
			{
				this.currentInterestLabel.text = "Waiting...";
				return;
			}
			string text = (this.bankTier != null) ? this.bankTier.GetFluctuationDisplay() : "+0% / -0%";
			this.currentInterestLabel.text = text;
			return;
		}
	}

	// Token: 0x060002D0 RID: 720 RVA: 0x0000E5AC File Offset: 0x0000C7AC
	private void UpdateLastModificationLabel(float percent)
	{
		if (this.lastModificationLabel == null)
		{
			return;
		}
		if (this.lastModificationTime > 0f && percent != 0f)
		{
			string arg = (percent >= 0f) ? "+" : "";
			this.lastModificationLabel.text = string.Format("{0}{1:F1}%", arg, percent);
			return;
		}
		this.lastModificationLabel.text = "";
	}

	// Token: 0x060002D1 RID: 721 RVA: 0x0000E61F File Offset: 0x0000C81F
	public long GetBankBalance()
	{
		return this.bankBalance;
	}

	// Token: 0x060002D2 RID: 722 RVA: 0x0000E627 File Offset: 0x0000C827
	public BankMode GetCurrentMode()
	{
		return this.currentMode;
	}

	// Token: 0x060002D3 RID: 723 RVA: 0x0000E62F File Offset: 0x0000C82F
	public long GetSelectedAmount()
	{
		return this.selectedAmount;
	}

	// Token: 0x060002D4 RID: 724 RVA: 0x0000E637 File Offset: 0x0000C837
	public float GetTimeUntilNextModification()
	{
		if (this.nextModificationTime == 0f)
		{
			return 0f;
		}
		return Mathf.Max(0f, (float)((double)this.nextModificationTime - NetworkTime.time));
	}

	// Token: 0x060002D5 RID: 725 RVA: 0x0000E664 File Offset: 0x0000C864
	public bool CanWithdrawMoney()
	{
		return this.CanWithdraw();
	}

	// Token: 0x060002D6 RID: 726 RVA: 0x0000E66C File Offset: 0x0000C86C
	public float GetLastModificationPercent()
	{
		return this.lastModificationPercent;
	}

	// Token: 0x060002D7 RID: 727 RVA: 0x0000E674 File Offset: 0x0000C874
	public int GetCurrentTier()
	{
		if (!(this.bankTier != null))
		{
			return 1;
		}
		return this.bankTier.tierNumber;
	}

	// Token: 0x060002D8 RID: 728 RVA: 0x0000E691 File Offset: 0x0000C891
	[Server]
	public void SetBankTier(BankTier newTier)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void Bank::SetBankTier(BankTier)' called when server was not active");
			return;
		}
		if (newTier == null)
		{
			return;
		}
		this.bankTier = newTier;
		this.NetworkbankTierNumber = newTier.tierNumber;
	}

	// Token: 0x060002D9 RID: 729 RVA: 0x0000E6C8 File Offset: 0x0000C8C8
	public Bank()
	{
		this._Mirror_SyncVarHookDelegate_bankTierNumber = new Action<int, int>(this.OnBankTierNumberChanged);
		this._Mirror_SyncVarHookDelegate_bankBalance = new Action<long, long>(this.OnBankBalanceChanged);
		this._Mirror_SyncVarHookDelegate_currentMode = new Action<BankMode, BankMode>(this.OnModeChanged);
		this._Mirror_SyncVarHookDelegate_selectedAmount = new Action<long, long>(this.OnSelectedAmountChanged);
		this._Mirror_SyncVarHookDelegate_lastModificationTime = new Action<float, float>(this.OnLastModificationTimeChanged);
		this._Mirror_SyncVarHookDelegate_nextModificationTime = new Action<float, float>(this.OnNextModificationTimeChanged);
		this._Mirror_SyncVarHookDelegate_lastModificationPercent = new Action<float, float>(this.OnLastModificationPercentChanged);
		this._Mirror_SyncVarHookDelegate_lastDepositTime = new Action<float, float>(this.OnLastDepositTimeChanged);
	}

	// Token: 0x060002DA RID: 730 RVA: 0x00002321 File Offset: 0x00000521
	public override bool Weaved()
	{
		return true;
	}

	// Token: 0x1700004B RID: 75
	// (get) Token: 0x060002DB RID: 731 RVA: 0x0000E784 File Offset: 0x0000C984
	// (set) Token: 0x060002DC RID: 732 RVA: 0x0000E797 File Offset: 0x0000C997
	public int NetworkbankTierNumber
	{
		get
		{
			return this.bankTierNumber;
		}
		[param: In]
		set
		{
			base.GeneratedSyncVarSetter<int>(value, ref this.bankTierNumber, 1UL, this._Mirror_SyncVarHookDelegate_bankTierNumber);
		}
	}

	// Token: 0x1700004C RID: 76
	// (get) Token: 0x060002DD RID: 733 RVA: 0x0000E7B8 File Offset: 0x0000C9B8
	// (set) Token: 0x060002DE RID: 734 RVA: 0x0000E7CB File Offset: 0x0000C9CB
	public long NetworkbankBalance
	{
		get
		{
			return this.bankBalance;
		}
		[param: In]
		set
		{
			base.GeneratedSyncVarSetter<long>(value, ref this.bankBalance, 2UL, this._Mirror_SyncVarHookDelegate_bankBalance);
		}
	}

	// Token: 0x1700004D RID: 77
	// (get) Token: 0x060002DF RID: 735 RVA: 0x0000E7EC File Offset: 0x0000C9EC
	// (set) Token: 0x060002E0 RID: 736 RVA: 0x0000E7FF File Offset: 0x0000C9FF
	public BankMode NetworkcurrentMode
	{
		get
		{
			return this.currentMode;
		}
		[param: In]
		set
		{
			base.GeneratedSyncVarSetter<BankMode>(value, ref this.currentMode, 4UL, this._Mirror_SyncVarHookDelegate_currentMode);
		}
	}

	// Token: 0x1700004E RID: 78
	// (get) Token: 0x060002E1 RID: 737 RVA: 0x0000E820 File Offset: 0x0000CA20
	// (set) Token: 0x060002E2 RID: 738 RVA: 0x0000E833 File Offset: 0x0000CA33
	public long NetworkselectedAmount
	{
		get
		{
			return this.selectedAmount;
		}
		[param: In]
		set
		{
			base.GeneratedSyncVarSetter<long>(value, ref this.selectedAmount, 8UL, this._Mirror_SyncVarHookDelegate_selectedAmount);
		}
	}

	// Token: 0x1700004F RID: 79
	// (get) Token: 0x060002E3 RID: 739 RVA: 0x0000E854 File Offset: 0x0000CA54
	// (set) Token: 0x060002E4 RID: 740 RVA: 0x0000E867 File Offset: 0x0000CA67
	public float NetworklastModificationTime
	{
		get
		{
			return this.lastModificationTime;
		}
		[param: In]
		set
		{
			base.GeneratedSyncVarSetter<float>(value, ref this.lastModificationTime, 16UL, this._Mirror_SyncVarHookDelegate_lastModificationTime);
		}
	}

	// Token: 0x17000050 RID: 80
	// (get) Token: 0x060002E5 RID: 741 RVA: 0x0000E888 File Offset: 0x0000CA88
	// (set) Token: 0x060002E6 RID: 742 RVA: 0x0000E89B File Offset: 0x0000CA9B
	public float NetworknextModificationTime
	{
		get
		{
			return this.nextModificationTime;
		}
		[param: In]
		set
		{
			base.GeneratedSyncVarSetter<float>(value, ref this.nextModificationTime, 32UL, this._Mirror_SyncVarHookDelegate_nextModificationTime);
		}
	}

	// Token: 0x17000051 RID: 81
	// (get) Token: 0x060002E7 RID: 743 RVA: 0x0000E8BC File Offset: 0x0000CABC
	// (set) Token: 0x060002E8 RID: 744 RVA: 0x0000E8CF File Offset: 0x0000CACF
	public float NetworklastModificationPercent
	{
		get
		{
			return this.lastModificationPercent;
		}
		[param: In]
		set
		{
			base.GeneratedSyncVarSetter<float>(value, ref this.lastModificationPercent, 64UL, this._Mirror_SyncVarHookDelegate_lastModificationPercent);
		}
	}

	// Token: 0x17000052 RID: 82
	// (get) Token: 0x060002E9 RID: 745 RVA: 0x0000E8F0 File Offset: 0x0000CAF0
	// (set) Token: 0x060002EA RID: 746 RVA: 0x0000E903 File Offset: 0x0000CB03
	public float NetworklastDepositTime
	{
		get
		{
			return this.lastDepositTime;
		}
		[param: In]
		set
		{
			base.GeneratedSyncVarSetter<float>(value, ref this.lastDepositTime, 128UL, this._Mirror_SyncVarHookDelegate_lastDepositTime);
		}
	}

	// Token: 0x060002EB RID: 747 RVA: 0x0000E924 File Offset: 0x0000CB24
	public override void SerializeSyncVars(NetworkWriter writer, bool forceAll)
	{
		base.SerializeSyncVars(writer, forceAll);
		if (forceAll)
		{
			writer.WriteVarInt(this.bankTierNumber);
			writer.WriteVarLong(this.bankBalance);
			Mirror.GeneratedNetworkCode._Write_BankMode(writer, this.currentMode);
			writer.WriteVarLong(this.selectedAmount);
			writer.WriteFloat(this.lastModificationTime);
			writer.WriteFloat(this.nextModificationTime);
			writer.WriteFloat(this.lastModificationPercent);
			writer.WriteFloat(this.lastDepositTime);
			return;
		}
		writer.WriteVarULong(this.syncVarDirtyBits);
		if ((this.syncVarDirtyBits & 1UL) != 0UL)
		{
			writer.WriteVarInt(this.bankTierNumber);
		}
		if ((this.syncVarDirtyBits & 2UL) != 0UL)
		{
			writer.WriteVarLong(this.bankBalance);
		}
		if ((this.syncVarDirtyBits & 4UL) != 0UL)
		{
			Mirror.GeneratedNetworkCode._Write_BankMode(writer, this.currentMode);
		}
		if ((this.syncVarDirtyBits & 8UL) != 0UL)
		{
			writer.WriteVarLong(this.selectedAmount);
		}
		if ((this.syncVarDirtyBits & 16UL) != 0UL)
		{
			writer.WriteFloat(this.lastModificationTime);
		}
		if ((this.syncVarDirtyBits & 32UL) != 0UL)
		{
			writer.WriteFloat(this.nextModificationTime);
		}
		if ((this.syncVarDirtyBits & 64UL) != 0UL)
		{
			writer.WriteFloat(this.lastModificationPercent);
		}
		if ((this.syncVarDirtyBits & 128UL) != 0UL)
		{
			writer.WriteFloat(this.lastDepositTime);
		}
	}

	// Token: 0x060002EC RID: 748 RVA: 0x0000EAC0 File Offset: 0x0000CCC0
	public override void DeserializeSyncVars(NetworkReader reader, bool initialState)
	{
		base.DeserializeSyncVars(reader, initialState);
		if (initialState)
		{
			base.GeneratedSyncVarDeserialize<int>(ref this.bankTierNumber, this._Mirror_SyncVarHookDelegate_bankTierNumber, reader.ReadVarInt());
			base.GeneratedSyncVarDeserialize<long>(ref this.bankBalance, this._Mirror_SyncVarHookDelegate_bankBalance, reader.ReadVarLong());
			base.GeneratedSyncVarDeserialize<BankMode>(ref this.currentMode, this._Mirror_SyncVarHookDelegate_currentMode, Mirror.GeneratedNetworkCode._Read_BankMode(reader));
			base.GeneratedSyncVarDeserialize<long>(ref this.selectedAmount, this._Mirror_SyncVarHookDelegate_selectedAmount, reader.ReadVarLong());
			base.GeneratedSyncVarDeserialize<float>(ref this.lastModificationTime, this._Mirror_SyncVarHookDelegate_lastModificationTime, reader.ReadFloat());
			base.GeneratedSyncVarDeserialize<float>(ref this.nextModificationTime, this._Mirror_SyncVarHookDelegate_nextModificationTime, reader.ReadFloat());
			base.GeneratedSyncVarDeserialize<float>(ref this.lastModificationPercent, this._Mirror_SyncVarHookDelegate_lastModificationPercent, reader.ReadFloat());
			base.GeneratedSyncVarDeserialize<float>(ref this.lastDepositTime, this._Mirror_SyncVarHookDelegate_lastDepositTime, reader.ReadFloat());
			return;
		}
		long num = (long)reader.ReadVarULong();
		if ((num & 1L) != 0L)
		{
			base.GeneratedSyncVarDeserialize<int>(ref this.bankTierNumber, this._Mirror_SyncVarHookDelegate_bankTierNumber, reader.ReadVarInt());
		}
		if ((num & 2L) != 0L)
		{
			base.GeneratedSyncVarDeserialize<long>(ref this.bankBalance, this._Mirror_SyncVarHookDelegate_bankBalance, reader.ReadVarLong());
		}
		if ((num & 4L) != 0L)
		{
			base.GeneratedSyncVarDeserialize<BankMode>(ref this.currentMode, this._Mirror_SyncVarHookDelegate_currentMode, Mirror.GeneratedNetworkCode._Read_BankMode(reader));
		}
		if ((num & 8L) != 0L)
		{
			base.GeneratedSyncVarDeserialize<long>(ref this.selectedAmount, this._Mirror_SyncVarHookDelegate_selectedAmount, reader.ReadVarLong());
		}
		if ((num & 16L) != 0L)
		{
			base.GeneratedSyncVarDeserialize<float>(ref this.lastModificationTime, this._Mirror_SyncVarHookDelegate_lastModificationTime, reader.ReadFloat());
		}
		if ((num & 32L) != 0L)
		{
			base.GeneratedSyncVarDeserialize<float>(ref this.nextModificationTime, this._Mirror_SyncVarHookDelegate_nextModificationTime, reader.ReadFloat());
		}
		if ((num & 64L) != 0L)
		{
			base.GeneratedSyncVarDeserialize<float>(ref this.lastModificationPercent, this._Mirror_SyncVarHookDelegate_lastModificationPercent, reader.ReadFloat());
		}
		if ((num & 128L) != 0L)
		{
			base.GeneratedSyncVarDeserialize<float>(ref this.lastDepositTime, this._Mirror_SyncVarHookDelegate_lastDepositTime, reader.ReadFloat());
		}
	}

	// Token: 0x04000217 RID: 535
	[Header("References")]
	[SerializeField]
	private BankKnob knob;

	// Token: 0x04000218 RID: 536
	[SerializeField]
	private TextMeshPro bankBalanceLabel;

	// Token: 0x04000219 RID: 537
	[SerializeField]
	private TextMeshPro selectedAmountLabel;

	// Token: 0x0400021A RID: 538
	[SerializeField]
	private TextMeshPro modeLabel;

	// Token: 0x0400021B RID: 539
	[SerializeField]
	private TextMeshPro currentInterestLabel;

	// Token: 0x0400021C RID: 540
	[SerializeField]
	private TextMeshPro lastModificationLabel;

	// Token: 0x0400021D RID: 541
	[SerializeField]
	private string currencyFormat = "${0}";

	// Token: 0x0400021E RID: 542
	[Header("Bank Settings")]
	[SerializeField]
	private int stepAmount = 1;

	// Token: 0x0400021F RID: 543
	[Header("Stock Market Settings")]
	[Tooltip("The tier configuration for this bank")]
	[SerializeField]
	private BankTier bankTier;

	// Token: 0x04000220 RID: 544
	[SyncVar(hook = "OnBankTierNumberChanged")]
	private int bankTierNumber = 1;

	// Token: 0x04000221 RID: 545
	[SyncVar(hook = "OnBankBalanceChanged")]
	private long bankBalance;

	// Token: 0x04000222 RID: 546
	[SyncVar(hook = "OnModeChanged")]
	private BankMode currentMode;

	// Token: 0x04000223 RID: 547
	[SyncVar(hook = "OnSelectedAmountChanged")]
	private long selectedAmount;

	// Token: 0x04000224 RID: 548
	[SyncVar(hook = "OnLastModificationTimeChanged")]
	private float lastModificationTime;

	// Token: 0x04000225 RID: 549
	[SyncVar(hook = "OnNextModificationTimeChanged")]
	private float nextModificationTime;

	// Token: 0x04000226 RID: 550
	[SyncVar(hook = "OnLastModificationPercentChanged")]
	private float lastModificationPercent;

	// Token: 0x04000227 RID: 551
	[SyncVar(hook = "OnLastDepositTimeChanged")]
	private float lastDepositTime;

	// Token: 0x04000228 RID: 552
	public Action<long> OnBankBalanceChangedEvent;

	// Token: 0x04000229 RID: 553
	public Action<BankMode> OnModeChangedEvent;

	// Token: 0x0400022A RID: 554
	public Action<long> OnSelectedAmountChangedEvent;

	// Token: 0x0400022B RID: 555
	public Action<int, int> _Mirror_SyncVarHookDelegate_bankTierNumber;

	// Token: 0x0400022C RID: 556
	public Action<long, long> _Mirror_SyncVarHookDelegate_bankBalance;

	// Token: 0x0400022D RID: 557
	public Action<BankMode, BankMode> _Mirror_SyncVarHookDelegate_currentMode;

	// Token: 0x0400022E RID: 558
	public Action<long, long> _Mirror_SyncVarHookDelegate_selectedAmount;

	// Token: 0x0400022F RID: 559
	public Action<float, float> _Mirror_SyncVarHookDelegate_lastModificationTime;

	// Token: 0x04000230 RID: 560
	public Action<float, float> _Mirror_SyncVarHookDelegate_nextModificationTime;

	// Token: 0x04000231 RID: 561
	public Action<float, float> _Mirror_SyncVarHookDelegate_lastModificationPercent;

	// Token: 0x04000232 RID: 562
	public Action<float, float> _Mirror_SyncVarHookDelegate_lastDepositTime;
}
