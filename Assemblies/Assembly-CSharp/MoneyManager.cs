using System;
using System.Runtime.InteropServices;
using Extensions;
using Mirror;
using Mirror.RemoteCalls;
using UnityEngine;

// Token: 0x02000188 RID: 392
public class MoneyManager : NetworkSingleton<MoneyManager>
{
	// Token: 0x06000EA4 RID: 3748 RVA: 0x0003CC3B File Offset: 0x0003AE3B
	protected override void OnAwake()
	{
		base.OnAwake();
		this._gs = Resources.Load<GameSettings>("GameSettings");
	}

	// Token: 0x06000EA5 RID: 3749 RVA: 0x0003CC54 File Offset: 0x0003AE54
	[Command(requiresAuthority = false)]
	public void CmdTryChangeBalance(long amount, PlayerProfile changer, ChangeType changeType)
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		writer.WriteVarLong(amount);
		writer.WriteNetworkBehaviour(changer);
		Mirror.GeneratedNetworkCode._Write_ChangeType(writer, changeType);
		base.SendCommandInternal("System.Void MoneyManager::CmdTryChangeBalance(System.Int64,PlayerProfile,ChangeType)", 1990209703, writer, 0, false);
		NetworkWriterPool.Return(writer);
	}

	// Token: 0x06000EA6 RID: 3750 RVA: 0x0003CCA4 File Offset: 0x0003AEA4
	[Server]
	public bool TryChangeBalance(long amount, PlayerProfile changer, ChangeType changeType)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Boolean MoneyManager::TryChangeBalance(System.Int64,PlayerProfile,ChangeType)' called when server was not active");
			return default(bool);
		}
		if (amount == 0L)
		{
			return false;
		}
		if (amount > 0L)
		{
			long num = Math.Clamp(this.balance + amount, long.MinValue, long.MaxValue);
			this.AddBalance(num - this.balance, changer, changeType);
		}
		else if (amount < 0L)
		{
			if (this.balance + amount < 0L)
			{
				return false;
			}
			this.RemoveBalance(amount, changer, changeType);
		}
		return true;
	}

	// Token: 0x06000EA7 RID: 3751 RVA: 0x0003CD2C File Offset: 0x0003AF2C
	[Server]
	public void SetBalance(long amount, PlayerProfile changer, ChangeType changeType)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void MoneyManager::SetBalance(System.Int64,PlayerProfile,ChangeType)' called when server was not active");
			return;
		}
		this.Networkbalance = amount;
		Action<BalanceChangeData> onBalanceChanged = this.OnBalanceChanged;
		if (onBalanceChanged != null)
		{
			onBalanceChanged(new BalanceChangeData(amount, changer, changeType));
		}
		this.RpcInvokeBalanceChanged(this.balance, amount, changer, changeType);
	}

	// Token: 0x06000EA8 RID: 3752 RVA: 0x0003CD80 File Offset: 0x0003AF80
	[Server]
	private void AddBalance(long amount, PlayerProfile changer, ChangeType changeType)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void MoneyManager::AddBalance(System.Int64,PlayerProfile,ChangeType)' called when server was not active");
			return;
		}
		this.Networkbalance = this.balance + Math.Abs(amount);
		BalanceChangeData obj = new BalanceChangeData(amount, changer, changeType);
		Action<BalanceChangeData> onBalanceChanged = this.OnBalanceChanged;
		if (onBalanceChanged != null)
		{
			onBalanceChanged(obj);
		}
		this.RpcInvokeBalanceChanged(this.balance, amount, changer, changeType);
	}

	// Token: 0x06000EA9 RID: 3753 RVA: 0x0003CDE0 File Offset: 0x0003AFE0
	[Server]
	private void RemoveBalance(long amount, PlayerProfile changer, ChangeType changeType)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void MoneyManager::RemoveBalance(System.Int64,PlayerProfile,ChangeType)' called when server was not active");
			return;
		}
		this.Networkbalance = this.balance - Math.Abs(amount);
		BalanceChangeData obj = new BalanceChangeData(amount, changer, changeType);
		Action<BalanceChangeData> onBalanceChanged = this.OnBalanceChanged;
		if (onBalanceChanged != null)
		{
			onBalanceChanged(obj);
		}
		this.RpcInvokeBalanceChanged(this.balance, amount, changer, changeType);
	}

	// Token: 0x06000EAA RID: 3754 RVA: 0x0003CE40 File Offset: 0x0003B040
	[ClientRpc]
	private void RpcInvokeBalanceChanged(long finalBalance, long amount, PlayerProfile changer, ChangeType changeType)
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		writer.WriteVarLong(finalBalance);
		writer.WriteVarLong(amount);
		writer.WriteNetworkBehaviour(changer);
		Mirror.GeneratedNetworkCode._Write_ChangeType(writer, changeType);
		this.SendRPCInternal("System.Void MoneyManager::RpcInvokeBalanceChanged(System.Int64,System.Int64,PlayerProfile,ChangeType)", -708839733, writer, 0, true);
		NetworkWriterPool.Return(writer);
	}

	// Token: 0x06000EAB RID: 3755 RVA: 0x0003CE98 File Offset: 0x0003B098
	[Server]
	public void SetDayStartBalance()
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void MoneyManager::SetDayStartBalance()' called when server was not active");
			return;
		}
		this.NetworkdayStartBalance = this.balance;
	}

	// Token: 0x06000EAC RID: 3756 RVA: 0x0003CEBC File Offset: 0x0003B0BC
	[Command(requiresAuthority = false)]
	public void CmdTryChangeTicketBalance(long amount)
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		writer.WriteVarLong(amount);
		base.SendCommandInternal("System.Void MoneyManager::CmdTryChangeTicketBalance(System.Int64)", 63865289, writer, 0, false);
		NetworkWriterPool.Return(writer);
	}

	// Token: 0x06000EAD RID: 3757 RVA: 0x0003CEF8 File Offset: 0x0003B0F8
	[Server]
	public bool TryChangeTicketBalance(long amount)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Boolean MoneyManager::TryChangeTicketBalance(System.Int64)' called when server was not active");
			return default(bool);
		}
		if (amount > 0L)
		{
			long num = Math.Clamp(this.ticketBalance + amount, long.MinValue, long.MaxValue);
			this.AddTicket(num - this.ticketBalance);
		}
		else if (amount < 0L)
		{
			if (this.ticketBalance + amount < 0L)
			{
				return false;
			}
			this.RemoveTicket(amount);
		}
		return true;
	}

	// Token: 0x06000EAE RID: 3758 RVA: 0x0003CF76 File Offset: 0x0003B176
	public bool TrySetTicketBalance(long amount)
	{
		this.NetworkticketBalance = amount;
		Action<long> onTicketBalanceChanged = this.OnTicketBalanceChanged;
		if (onTicketBalanceChanged != null)
		{
			onTicketBalanceChanged(amount);
		}
		this.RpcInvokeTicketChanged(this.ticketBalance, amount);
		return true;
	}

	// Token: 0x06000EAF RID: 3759 RVA: 0x0003CFA0 File Offset: 0x0003B1A0
	[Server]
	private void AddTicket(long amount)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void MoneyManager::AddTicket(System.Int64)' called when server was not active");
			return;
		}
		this.NetworkticketBalance = this.ticketBalance + Math.Abs(amount);
		Action<long> onTicketBalanceChanged = this.OnTicketBalanceChanged;
		if (onTicketBalanceChanged != null)
		{
			onTicketBalanceChanged(amount);
		}
		this.RpcInvokeTicketChanged(this.ticketBalance, amount);
	}

	// Token: 0x06000EB0 RID: 3760 RVA: 0x0003CFF4 File Offset: 0x0003B1F4
	[Server]
	private void RemoveTicket(long amount)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void MoneyManager::RemoveTicket(System.Int64)' called when server was not active");
			return;
		}
		this.NetworkticketBalance = this.ticketBalance - Math.Abs(amount);
		Action<long> onTicketBalanceChanged = this.OnTicketBalanceChanged;
		if (onTicketBalanceChanged != null)
		{
			onTicketBalanceChanged(-Math.Abs(amount));
		}
		this.RpcInvokeTicketChanged(this.ticketBalance, -Math.Abs(amount));
	}

	// Token: 0x06000EB1 RID: 3761 RVA: 0x0003D054 File Offset: 0x0003B254
	[ClientRpc]
	private void RpcInvokeTicketChanged(long finalBalance, long amount)
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		writer.WriteVarLong(finalBalance);
		writer.WriteVarLong(amount);
		this.SendRPCInternal("System.Void MoneyManager::RpcInvokeTicketChanged(System.Int64,System.Int64)", -97559143, writer, 0, true);
		NetworkWriterPool.Return(writer);
	}

	// Token: 0x06000EB2 RID: 3762 RVA: 0x0003D098 File Offset: 0x0003B298
	[Command(requiresAuthority = false)]
	public void CmdResetBalancesToDefault()
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		base.SendCommandInternal("System.Void MoneyManager::CmdResetBalancesToDefault()", -756986718, writer, 0, false);
		NetworkWriterPool.Return(writer);
	}

	// Token: 0x06000EB3 RID: 3763 RVA: 0x0003D0C8 File Offset: 0x0003B2C8
	[Server]
	private void ServerResetBalancesToDefault()
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void MoneyManager::ServerResetBalancesToDefault()' called when server was not active");
			return;
		}
		long num = this.balance;
		this.Networkbalance = this._gs.startingMoney;
		BalanceChangeData obj = new BalanceChangeData(this.balance - num, null, ChangeType.Misc);
		Action<BalanceChangeData> onBalanceChanged = this.OnBalanceChanged;
		if (onBalanceChanged != null)
		{
			onBalanceChanged(obj);
		}
		this.RpcInvokeBalanceChanged(this.balance, this.balance - num, null, ChangeType.Misc);
		long num2 = this.ticketBalance;
		this.NetworkticketBalance = this._gs.startingTicket;
		Action<long> onTicketBalanceChanged = this.OnTicketBalanceChanged;
		if (onTicketBalanceChanged != null)
		{
			onTicketBalanceChanged(this.ticketBalance - num2);
		}
		this.RpcInvokeTicketChanged(this.ticketBalance, this.ticketBalance - num2);
	}

	// Token: 0x06000EB5 RID: 3765 RVA: 0x00002321 File Offset: 0x00000521
	public override bool Weaved()
	{
		return true;
	}

	// Token: 0x17000149 RID: 329
	// (get) Token: 0x06000EB6 RID: 3766 RVA: 0x0003D188 File Offset: 0x0003B388
	// (set) Token: 0x06000EB7 RID: 3767 RVA: 0x0003D19B File Offset: 0x0003B39B
	public long Networkbalance
	{
		get
		{
			return this.balance;
		}
		[param: In]
		set
		{
			base.GeneratedSyncVarSetter<long>(value, ref this.balance, 1UL, null);
		}
	}

	// Token: 0x1700014A RID: 330
	// (get) Token: 0x06000EB8 RID: 3768 RVA: 0x0003D1B8 File Offset: 0x0003B3B8
	// (set) Token: 0x06000EB9 RID: 3769 RVA: 0x0003D1CB File Offset: 0x0003B3CB
	public long NetworkticketBalance
	{
		get
		{
			return this.ticketBalance;
		}
		[param: In]
		set
		{
			base.GeneratedSyncVarSetter<long>(value, ref this.ticketBalance, 2UL, null);
		}
	}

	// Token: 0x1700014B RID: 331
	// (get) Token: 0x06000EBA RID: 3770 RVA: 0x0003D1E8 File Offset: 0x0003B3E8
	// (set) Token: 0x06000EBB RID: 3771 RVA: 0x0003D1FB File Offset: 0x0003B3FB
	public long NetworkdayStartBalance
	{
		get
		{
			return this.dayStartBalance;
		}
		[param: In]
		set
		{
			base.GeneratedSyncVarSetter<long>(value, ref this.dayStartBalance, 4UL, null);
		}
	}

	// Token: 0x06000EBC RID: 3772 RVA: 0x0003D215 File Offset: 0x0003B415
	protected void UserCode_CmdTryChangeBalance__Int64__PlayerProfile__ChangeType(long amount, PlayerProfile changer, ChangeType changeType)
	{
		this.TryChangeBalance(amount, changer, changeType);
	}

	// Token: 0x06000EBD RID: 3773 RVA: 0x0003D221 File Offset: 0x0003B421
	protected static void InvokeUserCode_CmdTryChangeBalance__Int64__PlayerProfile__ChangeType(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdTryChangeBalance called on client.");
			return;
		}
		((MoneyManager)obj).UserCode_CmdTryChangeBalance__Int64__PlayerProfile__ChangeType(reader.ReadVarLong(), reader.ReadNetworkBehaviour<PlayerProfile>(), Mirror.GeneratedNetworkCode._Read_ChangeType(reader));
	}

	// Token: 0x06000EBE RID: 3774 RVA: 0x0003D258 File Offset: 0x0003B458
	protected void UserCode_RpcInvokeBalanceChanged__Int64__Int64__PlayerProfile__ChangeType(long finalBalance, long amount, PlayerProfile changer, ChangeType changeType)
	{
		if (base.isServer)
		{
			return;
		}
		this.Networkbalance = finalBalance;
		BalanceChangeData obj = new BalanceChangeData(amount, changer, changeType);
		Action<BalanceChangeData> onBalanceChanged = this.OnBalanceChanged;
		if (onBalanceChanged == null)
		{
			return;
		}
		onBalanceChanged(obj);
	}

	// Token: 0x06000EBF RID: 3775 RVA: 0x0003D290 File Offset: 0x0003B490
	protected static void InvokeUserCode_RpcInvokeBalanceChanged__Int64__Int64__PlayerProfile__ChangeType(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcInvokeBalanceChanged called on server.");
			return;
		}
		((MoneyManager)obj).UserCode_RpcInvokeBalanceChanged__Int64__Int64__PlayerProfile__ChangeType(reader.ReadVarLong(), reader.ReadVarLong(), reader.ReadNetworkBehaviour<PlayerProfile>(), Mirror.GeneratedNetworkCode._Read_ChangeType(reader));
	}

	// Token: 0x06000EC0 RID: 3776 RVA: 0x0003D2CB File Offset: 0x0003B4CB
	protected void UserCode_CmdTryChangeTicketBalance__Int64(long amount)
	{
		this.TryChangeTicketBalance(amount);
	}

	// Token: 0x06000EC1 RID: 3777 RVA: 0x0003D2D5 File Offset: 0x0003B4D5
	protected static void InvokeUserCode_CmdTryChangeTicketBalance__Int64(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdTryChangeTicketBalance called on client.");
			return;
		}
		((MoneyManager)obj).UserCode_CmdTryChangeTicketBalance__Int64(reader.ReadVarLong());
	}

	// Token: 0x06000EC2 RID: 3778 RVA: 0x0003D2FE File Offset: 0x0003B4FE
	protected void UserCode_RpcInvokeTicketChanged__Int64__Int64(long finalBalance, long amount)
	{
		if (base.isServer)
		{
			return;
		}
		this.NetworkticketBalance = finalBalance;
		Action<long> onTicketBalanceChanged = this.OnTicketBalanceChanged;
		if (onTicketBalanceChanged == null)
		{
			return;
		}
		onTicketBalanceChanged(amount);
	}

	// Token: 0x06000EC3 RID: 3779 RVA: 0x0003D321 File Offset: 0x0003B521
	protected static void InvokeUserCode_RpcInvokeTicketChanged__Int64__Int64(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcInvokeTicketChanged called on server.");
			return;
		}
		((MoneyManager)obj).UserCode_RpcInvokeTicketChanged__Int64__Int64(reader.ReadVarLong(), reader.ReadVarLong());
	}

	// Token: 0x06000EC4 RID: 3780 RVA: 0x0003D350 File Offset: 0x0003B550
	protected void UserCode_CmdResetBalancesToDefault()
	{
		this.ServerResetBalancesToDefault();
	}

	// Token: 0x06000EC5 RID: 3781 RVA: 0x0003D358 File Offset: 0x0003B558
	protected static void InvokeUserCode_CmdResetBalancesToDefault(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdResetBalancesToDefault called on client.");
			return;
		}
		((MoneyManager)obj).UserCode_CmdResetBalancesToDefault();
	}

	// Token: 0x06000EC6 RID: 3782 RVA: 0x0003D37C File Offset: 0x0003B57C
	static MoneyManager()
	{
		RemoteProcedureCalls.RegisterCommand(typeof(MoneyManager), "System.Void MoneyManager::CmdTryChangeBalance(System.Int64,PlayerProfile,ChangeType)", new RemoteCallDelegate(MoneyManager.InvokeUserCode_CmdTryChangeBalance__Int64__PlayerProfile__ChangeType), false);
		RemoteProcedureCalls.RegisterCommand(typeof(MoneyManager), "System.Void MoneyManager::CmdTryChangeTicketBalance(System.Int64)", new RemoteCallDelegate(MoneyManager.InvokeUserCode_CmdTryChangeTicketBalance__Int64), false);
		RemoteProcedureCalls.RegisterCommand(typeof(MoneyManager), "System.Void MoneyManager::CmdResetBalancesToDefault()", new RemoteCallDelegate(MoneyManager.InvokeUserCode_CmdResetBalancesToDefault), false);
		RemoteProcedureCalls.RegisterRpc(typeof(MoneyManager), "System.Void MoneyManager::RpcInvokeBalanceChanged(System.Int64,System.Int64,PlayerProfile,ChangeType)", new RemoteCallDelegate(MoneyManager.InvokeUserCode_RpcInvokeBalanceChanged__Int64__Int64__PlayerProfile__ChangeType));
		RemoteProcedureCalls.RegisterRpc(typeof(MoneyManager), "System.Void MoneyManager::RpcInvokeTicketChanged(System.Int64,System.Int64)", new RemoteCallDelegate(MoneyManager.InvokeUserCode_RpcInvokeTicketChanged__Int64__Int64));
	}

	// Token: 0x06000EC7 RID: 3783 RVA: 0x0003D42C File Offset: 0x0003B62C
	public override void SerializeSyncVars(NetworkWriter writer, bool forceAll)
	{
		base.SerializeSyncVars(writer, forceAll);
		if (forceAll)
		{
			writer.WriteVarLong(this.balance);
			writer.WriteVarLong(this.ticketBalance);
			writer.WriteVarLong(this.dayStartBalance);
			return;
		}
		writer.WriteVarULong(this.syncVarDirtyBits);
		if ((this.syncVarDirtyBits & 1UL) != 0UL)
		{
			writer.WriteVarLong(this.balance);
		}
		if ((this.syncVarDirtyBits & 2UL) != 0UL)
		{
			writer.WriteVarLong(this.ticketBalance);
		}
		if ((this.syncVarDirtyBits & 4UL) != 0UL)
		{
			writer.WriteVarLong(this.dayStartBalance);
		}
	}

	// Token: 0x06000EC8 RID: 3784 RVA: 0x0003D4E0 File Offset: 0x0003B6E0
	public override void DeserializeSyncVars(NetworkReader reader, bool initialState)
	{
		base.DeserializeSyncVars(reader, initialState);
		if (initialState)
		{
			base.GeneratedSyncVarDeserialize<long>(ref this.balance, null, reader.ReadVarLong());
			base.GeneratedSyncVarDeserialize<long>(ref this.ticketBalance, null, reader.ReadVarLong());
			base.GeneratedSyncVarDeserialize<long>(ref this.dayStartBalance, null, reader.ReadVarLong());
			return;
		}
		long num = (long)reader.ReadVarULong();
		if ((num & 1L) != 0L)
		{
			base.GeneratedSyncVarDeserialize<long>(ref this.balance, null, reader.ReadVarLong());
		}
		if ((num & 2L) != 0L)
		{
			base.GeneratedSyncVarDeserialize<long>(ref this.ticketBalance, null, reader.ReadVarLong());
		}
		if ((num & 4L) != 0L)
		{
			base.GeneratedSyncVarDeserialize<long>(ref this.dayStartBalance, null, reader.ReadVarLong());
		}
	}

	// Token: 0x04000969 RID: 2409
	[Header("Logic")]
	[SyncVar]
	public long balance;

	// Token: 0x0400096A RID: 2410
	[SyncVar]
	public long ticketBalance;

	// Token: 0x0400096B RID: 2411
	[SyncVar]
	public long dayStartBalance;

	// Token: 0x0400096C RID: 2412
	public Action<BalanceChangeData> OnBalanceChanged;

	// Token: 0x0400096D RID: 2413
	public Action<long> OnTicketBalanceChanged;

	// Token: 0x0400096E RID: 2414
	private GameSettings _gs;
}
