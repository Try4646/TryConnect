using System;
using System.Collections.Generic;
using Extensions;
using Mirror;
using Mirror.RemoteCalls;
using Steamworks;
using UnityEngine;

// Token: 0x020001B3 RID: 435
public class UpgradeManager : NetworkSingleton<UpgradeManager>
{
	// Token: 0x06000FC4 RID: 4036 RVA: 0x000433AA File Offset: 0x000415AA
	public override void OnStartClient()
	{
		this.CmdUpdateUI();
	}

	// Token: 0x06000FC5 RID: 4037 RVA: 0x000433B4 File Offset: 0x000415B4
	[Command(requiresAuthority = false)]
	private void CmdUpdateUI()
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		base.SendCommandInternal("System.Void UpgradeManager::CmdUpdateUI()", -834980475, writer, 0, false);
		NetworkWriterPool.Return(writer);
	}

	// Token: 0x06000FC6 RID: 4038 RVA: 0x000433E4 File Offset: 0x000415E4
	[Server]
	public void ChangeUpgradeData(ulong steamId, PlayerUpgradeType type, float amount)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void UpgradeManager::ChangeUpgradeData(System.UInt64,PlayerUpgradeType,System.Single)' called when server was not active");
			return;
		}
		PlayerUpgradeData playerUpgradeData;
		if (!this._upgradeDataBySteamId.TryGetValue(steamId, out playerUpgradeData))
		{
			playerUpgradeData = new PlayerUpgradeData();
			this._upgradeDataBySteamId[steamId] = playerUpgradeData;
		}
		if (type == PlayerUpgradeType.Insurance)
		{
			float num = 1f - playerUpgradeData.Upgrades[type];
			float value = 1f - num * (1f - amount);
			playerUpgradeData.Upgrades[type] = value;
		}
		else
		{
			Dictionary<PlayerUpgradeType, float> upgrades = playerUpgradeData.Upgrades;
			upgrades[type] += amount;
		}
		this.RpcOnDataChanged(steamId, type, this._upgradeDataBySteamId[steamId].Upgrades[type], amount);
	}

	// Token: 0x06000FC7 RID: 4039 RVA: 0x0004349C File Offset: 0x0004169C
	[Server]
	public void SetUpgradeData(ulong steamId, PlayerUpgradeType type, float value)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void UpgradeManager::SetUpgradeData(System.UInt64,PlayerUpgradeType,System.Single)' called when server was not active");
			return;
		}
		PlayerUpgradeData playerUpgradeData;
		if (this._upgradeDataBySteamId.TryGetValue(steamId, out playerUpgradeData))
		{
			playerUpgradeData.Upgrades[type] = value;
		}
		else
		{
			this._upgradeDataBySteamId[steamId] = new PlayerUpgradeData();
			this._upgradeDataBySteamId[steamId].Upgrades[type] = value;
		}
		float num = new PlayerUpgradeData().Upgrades[type];
		if (value != num)
		{
			this.RpcOnDataChanged(steamId, type, this._upgradeDataBySteamId[steamId].Upgrades[type], 0f);
		}
	}

	// Token: 0x06000FC8 RID: 4040 RVA: 0x00043540 File Offset: 0x00041740
	[Server]
	public float GetUpgradeData(ulong steamId, PlayerUpgradeType type)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Single UpgradeManager::GetUpgradeData(System.UInt64,PlayerUpgradeType)' called when server was not active");
			return 0f;
		}
		PlayerUpgradeData playerUpgradeData;
		if (this._upgradeDataBySteamId.TryGetValue(steamId, out playerUpgradeData))
		{
			return playerUpgradeData.Upgrades[type];
		}
		this._upgradeDataBySteamId[steamId] = new PlayerUpgradeData();
		return this._upgradeDataBySteamId[steamId].Upgrades[type];
	}

	// Token: 0x06000FC9 RID: 4041 RVA: 0x000435B4 File Offset: 0x000417B4
	[Server]
	public IReadOnlyDictionary<ulong, PlayerUpgradeData> GetAllUpgradeDataBySteamId()
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Collections.Generic.IReadOnlyDictionary`2<System.UInt64,PlayerUpgradeData> UpgradeManager::GetAllUpgradeDataBySteamId()' called when server was not active");
			return null;
		}
		return this._upgradeDataBySteamId;
	}

	// Token: 0x06000FCA RID: 4042 RVA: 0x000435E8 File Offset: 0x000417E8
	[Server]
	public void ServerResetAllUpgradesToDefaults()
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void UpgradeManager::ServerResetAllUpgradesToDefaults()' called when server was not active");
			return;
		}
		this._upgradeDataBySteamId.Clear();
		this.RpcClearUpgradeUI();
	}

	// Token: 0x06000FCB RID: 4043 RVA: 0x00043610 File Offset: 0x00041810
	[ClientRpc]
	private void RpcOnDataChanged(ulong steamId, PlayerUpgradeType type, float value, float amount)
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		writer.WriteVarULong(steamId);
		Mirror.GeneratedNetworkCode._Write_PlayerUpgradeType(writer, type);
		writer.WriteFloat(value);
		writer.WriteFloat(amount);
		this.SendRPCInternal("System.Void UpgradeManager::RpcOnDataChanged(System.UInt64,PlayerUpgradeType,System.Single,System.Single)", 1329345160, writer, 0, true);
		NetworkWriterPool.Return(writer);
	}

	// Token: 0x06000FCC RID: 4044 RVA: 0x00043668 File Offset: 0x00041868
	[ClientRpc]
	private void RpcClearUpgradeUI()
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		this.SendRPCInternal("System.Void UpgradeManager::RpcClearUpgradeUI()", -1500172496, writer, 0, true);
		NetworkWriterPool.Return(writer);
	}

	// Token: 0x06000FCE RID: 4046 RVA: 0x00002321 File Offset: 0x00000521
	public override bool Weaved()
	{
		return true;
	}

	// Token: 0x06000FCF RID: 4047 RVA: 0x000436AC File Offset: 0x000418AC
	protected void UserCode_CmdUpdateUI()
	{
		foreach (KeyValuePair<ulong, PlayerUpgradeData> keyValuePair in this._upgradeDataBySteamId)
		{
			foreach (KeyValuePair<PlayerUpgradeType, float> keyValuePair2 in keyValuePair.Value.Upgrades)
			{
				float num = new PlayerUpgradeData().Upgrades[keyValuePair2.Key];
				if (keyValuePair2.Value != num)
				{
					this.RpcOnDataChanged(keyValuePair.Key, keyValuePair2.Key, keyValuePair2.Value, 0f);
				}
			}
		}
	}

	// Token: 0x06000FD0 RID: 4048 RVA: 0x00043784 File Offset: 0x00041984
	protected static void InvokeUserCode_CmdUpdateUI(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdUpdateUI called on client.");
			return;
		}
		((UpgradeManager)obj).UserCode_CmdUpdateUI();
	}

	// Token: 0x06000FD1 RID: 4049 RVA: 0x000437A7 File Offset: 0x000419A7
	protected void UserCode_RpcOnDataChanged__UInt64__PlayerUpgradeType__Single__Single(ulong steamId, PlayerUpgradeType type, float value, float amount)
	{
		if (SteamUser.GetSteamID().m_SteamID != steamId)
		{
			return;
		}
		MonoSingleton<UpgradeUI>.Instance.UpdateUpgradeUI(type, value, amount);
	}

	// Token: 0x06000FD2 RID: 4050 RVA: 0x000437C5 File Offset: 0x000419C5
	protected static void InvokeUserCode_RpcOnDataChanged__UInt64__PlayerUpgradeType__Single__Single(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcOnDataChanged called on server.");
			return;
		}
		((UpgradeManager)obj).UserCode_RpcOnDataChanged__UInt64__PlayerUpgradeType__Single__Single(reader.ReadVarULong(), Mirror.GeneratedNetworkCode._Read_PlayerUpgradeType(reader), reader.ReadFloat(), reader.ReadFloat());
	}

	// Token: 0x06000FD3 RID: 4051 RVA: 0x00043802 File Offset: 0x00041A02
	protected void UserCode_RpcClearUpgradeUI()
	{
		MonoSingleton<UpgradeUI>.Instance.ClearUpgradeUI();
	}

	// Token: 0x06000FD4 RID: 4052 RVA: 0x0004380E File Offset: 0x00041A0E
	protected static void InvokeUserCode_RpcClearUpgradeUI(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcClearUpgradeUI called on server.");
			return;
		}
		((UpgradeManager)obj).UserCode_RpcClearUpgradeUI();
	}

	// Token: 0x06000FD5 RID: 4053 RVA: 0x00043834 File Offset: 0x00041A34
	static UpgradeManager()
	{
		RemoteProcedureCalls.RegisterCommand(typeof(UpgradeManager), "System.Void UpgradeManager::CmdUpdateUI()", new RemoteCallDelegate(UpgradeManager.InvokeUserCode_CmdUpdateUI), false);
		RemoteProcedureCalls.RegisterRpc(typeof(UpgradeManager), "System.Void UpgradeManager::RpcOnDataChanged(System.UInt64,PlayerUpgradeType,System.Single,System.Single)", new RemoteCallDelegate(UpgradeManager.InvokeUserCode_RpcOnDataChanged__UInt64__PlayerUpgradeType__Single__Single));
		RemoteProcedureCalls.RegisterRpc(typeof(UpgradeManager), "System.Void UpgradeManager::RpcClearUpgradeUI()", new RemoteCallDelegate(UpgradeManager.InvokeUserCode_RpcClearUpgradeUI));
	}

	// Token: 0x04000A3E RID: 2622
	private readonly Dictionary<ulong, PlayerUpgradeData> _upgradeDataBySteamId = new Dictionary<ulong, PlayerUpgradeData>();
}
