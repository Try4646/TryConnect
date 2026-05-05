using System;
using System.Collections.Generic;
using Extensions;
using Mirror;
using UnityEngine;

// Token: 0x0200018E RID: 398
public class OrganManager : NetworkSingleton<OrganManager>
{
	// Token: 0x06000EE2 RID: 3810 RVA: 0x0003DD03 File Offset: 0x0003BF03
	public override void OnStartServer()
	{
		base.OnStartServer();
		NetworkSingleton<PlayerSpawnManager>.Instance.OnPlayerLateJoined += this.ServerApplyAllOrganSettings;
	}

	// Token: 0x06000EE3 RID: 3811 RVA: 0x0003DD21 File Offset: 0x0003BF21
	public override void OnStopServer()
	{
		base.OnStopServer();
		NetworkSingleton<PlayerSpawnManager>.Instance.OnPlayerLateJoined -= this.ServerApplyAllOrganSettings;
	}

	// Token: 0x06000EE4 RID: 3812 RVA: 0x0003DD40 File Offset: 0x0003BF40
	[Server]
	public void ServerRegisterPlayer(PlayerOrgans po)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void OrganManager::ServerRegisterPlayer(PlayerOrgans)' called when server was not active");
			return;
		}
		ulong num = 0UL;
		PlayerProfile component = po.GetComponent<PlayerProfile>();
		if (component != null)
		{
			num = component.steamId;
		}
		int connectionId = po.connectionToClient.connectionId;
		PlayerOrganData playerOrganData;
		PlayerOrganData playerOrganData2;
		if (num != 0UL && this.OrganDataBySteamId.TryGetValue(num, out playerOrganData))
		{
			playerOrganData2 = new PlayerOrganData
			{
				organsReference = po,
				leftEye = playerOrganData.leftEye,
				rightEye = playerOrganData.rightEye,
				body = playerOrganData.body,
				mouth = playerOrganData.mouth
			};
			playerOrganData.organsReference = po;
			this.OrganDataBySteamId[num] = playerOrganData;
		}
		else
		{
			playerOrganData2 = new PlayerOrganData
			{
				organsReference = po,
				leftEye = true,
				rightEye = true,
				body = true,
				mouth = true
			};
			if (num != 0UL)
			{
				this.OrganDataBySteamId[num] = playerOrganData2;
			}
		}
		this.OrganData[connectionId] = playerOrganData2;
		po.ServerSetBodyParts(playerOrganData2);
	}

	// Token: 0x06000EE5 RID: 3813 RVA: 0x0003DE44 File Offset: 0x0003C044
	[Server]
	public void ServerToggleOrgan(PlayerOrgans organs, OrganType organType, bool isEnabled)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void OrganManager::ServerToggleOrgan(PlayerOrgans,OrganType,System.Boolean)' called when server was not active");
			return;
		}
		int connectionId = organs.connectionToClient.connectionId;
		PlayerOrganData playerOrganData;
		if (!this.OrganData.TryGetValue(connectionId, out playerOrganData))
		{
			return;
		}
		switch (organType)
		{
		case OrganType.LeftEye:
			playerOrganData.leftEye = isEnabled;
			break;
		case OrganType.RightEye:
			playerOrganData.rightEye = isEnabled;
			break;
		case OrganType.Body:
			playerOrganData.body = isEnabled;
			break;
		case OrganType.Mouth:
			playerOrganData.mouth = isEnabled;
			break;
		}
		PlayerProfile component = organs.GetComponent<PlayerProfile>();
		if (component != null && component.steamId != 0UL)
		{
			PlayerOrganData playerOrganData2;
			if (this.OrganDataBySteamId.TryGetValue(component.steamId, out playerOrganData2))
			{
				playerOrganData2.leftEye = playerOrganData.leftEye;
				playerOrganData2.rightEye = playerOrganData.rightEye;
				playerOrganData2.body = playerOrganData.body;
				playerOrganData2.mouth = playerOrganData.mouth;
			}
			else
			{
				this.OrganDataBySteamId[component.steamId] = playerOrganData;
			}
		}
		organs.ServerSetBodyParts(playerOrganData);
	}

	// Token: 0x06000EE6 RID: 3814 RVA: 0x0003DF38 File Offset: 0x0003C138
	[Server]
	public void ServerApplyAllOrganSettings()
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void OrganManager::ServerApplyAllOrganSettings()' called when server was not active");
			return;
		}
		foreach (KeyValuePair<int, PlayerOrganData> keyValuePair in this.OrganData)
		{
			PlayerOrganData value = keyValuePair.Value;
			if (value.organsReference != null)
			{
				value.organsReference.ServerSetBodyParts(value);
			}
		}
	}

	// Token: 0x06000EE7 RID: 3815 RVA: 0x0003DFBC File Offset: 0x0003C1BC
	[Server]
	public void ServerResetAllOrgansToDefaults()
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void OrganManager::ServerResetAllOrgansToDefaults()' called when server was not active");
			return;
		}
		this.OrganDataBySteamId.Clear();
		foreach (KeyValuePair<int, PlayerOrganData> keyValuePair in this.OrganData)
		{
			PlayerOrganData value = keyValuePair.Value;
			value.leftEye = true;
			value.rightEye = true;
			value.body = true;
			value.mouth = true;
			if (value.organsReference != null)
			{
				value.organsReference.ServerSetBodyParts(value);
				PlayerProfile component = value.organsReference.GetComponent<PlayerProfile>();
				if (component != null && component.steamId != 0UL)
				{
					this.OrganDataBySteamId[component.steamId] = value;
				}
			}
		}
	}

	// Token: 0x06000EE8 RID: 3816 RVA: 0x0003E098 File Offset: 0x0003C298
	[Server]
	public Dictionary<ulong, PlayerOrganData> GetAllOrganDataBySteamId()
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Collections.Generic.Dictionary`2<System.UInt64,PlayerOrganData> OrganManager::GetAllOrganDataBySteamId()' called when server was not active");
			return null;
		}
		return new Dictionary<ulong, PlayerOrganData>(this.OrganDataBySteamId);
	}

	// Token: 0x06000EE9 RID: 3817 RVA: 0x0003E0D4 File Offset: 0x0003C2D4
	[Server]
	public PlayerOrganData GetOrganData(PlayerOrgans organs)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'PlayerOrganData OrganManager::GetOrganData(PlayerOrgans)' called when server was not active");
			return null;
		}
		int connectionId = organs.connectionToClient.connectionId;
		PlayerOrganData result;
		if (!this.OrganData.TryGetValue(connectionId, out result))
		{
			return null;
		}
		return result;
	}

	// Token: 0x06000EEA RID: 3818 RVA: 0x0003E124 File Offset: 0x0003C324
	[Server]
	public PlayerOrganData GetOrganData(ulong steamId)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'PlayerOrganData OrganManager::GetOrganData(System.UInt64)' called when server was not active");
			return null;
		}
		return this.OrganDataBySteamId[steamId];
	}

	// Token: 0x06000EEB RID: 3819 RVA: 0x0003E160 File Offset: 0x0003C360
	[Server]
	public void SetOrganDataBySteamId(ulong steamId, bool leftEye, bool rightEye, bool body, bool mouth)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void OrganManager::SetOrganDataBySteamId(System.UInt64,System.Boolean,System.Boolean,System.Boolean,System.Boolean)' called when server was not active");
			return;
		}
		PlayerOrganData playerOrganData;
		if (this.OrganDataBySteamId.TryGetValue(steamId, out playerOrganData))
		{
			playerOrganData.leftEye = leftEye;
			playerOrganData.rightEye = rightEye;
			playerOrganData.body = body;
			playerOrganData.mouth = mouth;
			if (playerOrganData.organsReference != null)
			{
				int connectionId = playerOrganData.organsReference.connectionToClient.connectionId;
				PlayerOrganData playerOrganData2;
				if (this.OrganData.TryGetValue(connectionId, out playerOrganData2))
				{
					playerOrganData2.leftEye = leftEye;
					playerOrganData2.rightEye = rightEye;
					playerOrganData2.body = body;
					playerOrganData2.mouth = mouth;
				}
				playerOrganData.organsReference.ServerSetBodyParts(playerOrganData);
				return;
			}
		}
		else
		{
			this.OrganDataBySteamId[steamId] = new PlayerOrganData
			{
				organsReference = null,
				leftEye = leftEye,
				rightEye = rightEye,
				body = body,
				mouth = mouth
			};
			foreach (KeyValuePair<int, PlayerOrganData> keyValuePair in this.OrganData)
			{
				PlayerOrganData value = keyValuePair.Value;
				if (value.organsReference != null)
				{
					PlayerProfile component = value.organsReference.GetComponent<PlayerProfile>();
					if (component != null && component.steamId == steamId)
					{
						value.leftEye = leftEye;
						value.rightEye = rightEye;
						value.body = body;
						value.mouth = mouth;
						this.OrganDataBySteamId[steamId].organsReference = value.organsReference;
						value.organsReference.ServerSetBodyParts(value);
						break;
					}
				}
			}
		}
	}

	// Token: 0x06000EED RID: 3821 RVA: 0x00002321 File Offset: 0x00000521
	public override bool Weaved()
	{
		return true;
	}

	// Token: 0x0400097E RID: 2430
	public Dictionary<int, PlayerOrganData> OrganData = new Dictionary<int, PlayerOrganData>();

	// Token: 0x0400097F RID: 2431
	private Dictionary<ulong, PlayerOrganData> OrganDataBySteamId = new Dictionary<ulong, PlayerOrganData>();
}
