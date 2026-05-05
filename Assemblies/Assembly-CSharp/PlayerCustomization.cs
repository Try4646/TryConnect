using System;
using System.Collections.Generic;
using Extensions;
using Mirror;
using Mirror.RemoteCalls;
using Steamworks;
using UnityEngine;
using UnityEngine.Rendering;

// Token: 0x020001FB RID: 507
public class PlayerCustomization : NetworkBehaviour
{
	// Token: 0x0600126B RID: 4715 RVA: 0x0004F941 File Offset: 0x0004DB41
	private void Awake()
	{
		this.lobbySettings = Resources.Load<LobbySettings>("LobbySettings");
	}

	// Token: 0x0600126C RID: 4716 RVA: 0x0004F954 File Offset: 0x0004DB54
	public void LoadCosmetics()
	{
		if (!base.isLocalPlayer || MonoSingleton<CosmeticsUnlockManager>.Instance == null)
		{
			return;
		}
		MonoSingleton<CosmeticsUnlockManager>.Instance.LoadFromFile();
		foreach (KeyValuePair<CosmeticType, int> keyValuePair in MonoSingleton<CosmeticsUnlockManager>.Instance.GetEquippedCosmetics())
		{
			int value = keyValuePair.Value;
			if (MonoSingleton<CosmeticsUnlockManager>.Instance.IsCosmeticUnlocked(value))
			{
				this.CmdChangeCustomization(value, false);
			}
		}
	}

	// Token: 0x0600126D RID: 4717 RVA: 0x0004F9E4 File Offset: 0x0004DBE4
	public void SaveCosmetics()
	{
		if (!base.isLocalPlayer || MonoSingleton<CosmeticsUnlockManager>.Instance == null)
		{
			return;
		}
		MonoSingleton<CosmeticsUnlockManager>.Instance.SetEquippedCosmetics(this.equippedCosmetics, false);
	}

	// Token: 0x0600126E RID: 4718 RVA: 0x0004FA10 File Offset: 0x0004DC10
	[Command]
	public void CmdChangeCustomization(int cosmeticId, bool shouldSave)
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		writer.WriteVarInt(cosmeticId);
		writer.WriteBool(shouldSave);
		base.SendCommandInternal("System.Void PlayerCustomization::CmdChangeCustomization(System.Int32,System.Boolean)", 1528149408, writer, 0, true);
		NetworkWriterPool.Return(writer);
	}

	// Token: 0x0600126F RID: 4719 RVA: 0x0004FA54 File Offset: 0x0004DC54
	[ClientRpc]
	private void RpcChangeCustomization(int cosmeticId, bool shouldSave)
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		writer.WriteVarInt(cosmeticId);
		writer.WriteBool(shouldSave);
		this.SendRPCInternal("System.Void PlayerCustomization::RpcChangeCustomization(System.Int32,System.Boolean)", -207402373, writer, 0, true);
		NetworkWriterPool.Return(writer);
	}

	// Token: 0x06001270 RID: 4720 RVA: 0x0004FA98 File Offset: 0x0004DC98
	private void ApplyCosmetic(int cosmeticId, bool shouldSave)
	{
		CosmeticData cosmeticById = CosmeticDataManager.GetCosmeticById(cosmeticId);
		if (cosmeticById == null)
		{
			Debug.LogError(string.Format("[PlayerCustomization] Cosmetic {0} not found", cosmeticId));
			return;
		}
		MeshFilter meshFilterForType = this.GetMeshFilterForType(cosmeticById.cosmeticType);
		Material material;
		if (!(cosmeticById.cosmeticMaterial != null))
		{
			MeshRenderer componentInChildren = cosmeticById.cosmeticModel.GetComponentInChildren<MeshRenderer>();
			material = ((componentInChildren != null) ? componentInChildren.sharedMaterial : null);
		}
		else
		{
			material = cosmeticById.cosmeticMaterial;
		}
		Material fallbackMaterial = material;
		if (meshFilterForType == null)
		{
			return;
		}
		MeshFilter meshFilter;
		if (!cosmeticById.cosmeticModel.TryGetComponent<MeshFilter>(out meshFilter))
		{
			return;
		}
		Mesh sharedMesh = meshFilter.sharedMesh;
		if (sharedMesh == null)
		{
			return;
		}
		meshFilterForType.mesh = sharedMesh;
		this.CreateShadowOnlyDuplicate(cosmeticById.cosmeticType, meshFilterForType, sharedMesh, fallbackMaterial);
		this.equippedCosmetics[cosmeticById.cosmeticType] = cosmeticId;
		if (base.isLocalPlayer && shouldSave)
		{
			this.SaveCosmetics();
		}
	}

	// Token: 0x06001271 RID: 4721 RVA: 0x0004FB6D File Offset: 0x0004DD6D
	public void ResetCustomization()
	{
		this.CmdResetCustomization();
	}

	// Token: 0x06001272 RID: 4722 RVA: 0x0004FB78 File Offset: 0x0004DD78
	[Command]
	private void CmdResetCustomization()
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		base.SendCommandInternal("System.Void PlayerCustomization::CmdResetCustomization()", -1499202751, writer, 0, true);
		NetworkWriterPool.Return(writer);
	}

	// Token: 0x06001273 RID: 4723 RVA: 0x0004FBA8 File Offset: 0x0004DDA8
	[ClientRpc]
	private void RpcResetCustomization()
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		this.SendRPCInternal("System.Void PlayerCustomization::RpcResetCustomization()", 1758572570, writer, 0, true);
		NetworkWriterPool.Return(writer);
	}

	// Token: 0x06001274 RID: 4724 RVA: 0x0004FBD8 File Offset: 0x0004DDD8
	private void ClearAllCosmetics(bool shouldSave = true)
	{
		foreach (GameObject gameObject in this.shadowOnlyClones.Values)
		{
			if (gameObject != null)
			{
				Object.Destroy(gameObject);
			}
		}
		this.shadowOnlyClones.Clear();
		this.hat.mesh = null;
		this.hair.mesh = null;
		this.mustache.mesh = null;
		this.beard.mesh = null;
		this.neckwear.mesh = null;
		this.equippedCosmetics.Clear();
		if (base.isLocalPlayer && shouldSave)
		{
			this.SaveCosmetics();
		}
	}

	// Token: 0x06001275 RID: 4725 RVA: 0x0004FC9C File Offset: 0x0004DE9C
	public void ClearCategory(CosmeticType category)
	{
		this.CmdClearCategory(category);
	}

	// Token: 0x06001276 RID: 4726 RVA: 0x0004FCA8 File Offset: 0x0004DEA8
	[Command]
	private void CmdClearCategory(CosmeticType category)
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		Mirror.GeneratedNetworkCode._Write_CosmeticType(writer, category);
		base.SendCommandInternal("System.Void PlayerCustomization::CmdClearCategory(CosmeticType)", 1714529541, writer, 0, true);
		NetworkWriterPool.Return(writer);
	}

	// Token: 0x06001277 RID: 4727 RVA: 0x0004FCE4 File Offset: 0x0004DEE4
	[ClientRpc]
	private void RpcClearCategory(CosmeticType category)
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		Mirror.GeneratedNetworkCode._Write_CosmeticType(writer, category);
		this.SendRPCInternal("System.Void PlayerCustomization::RpcClearCategory(CosmeticType)", -866626640, writer, 0, true);
		NetworkWriterPool.Return(writer);
	}

	// Token: 0x06001278 RID: 4728 RVA: 0x0004FD20 File Offset: 0x0004DF20
	public void LoadSavedPlayerColor()
	{
		if (!base.isLocalPlayer || MonoSingleton<CosmeticsUnlockManager>.Instance == null)
		{
			return;
		}
		Color? playerColor = MonoSingleton<CosmeticsUnlockManager>.Instance.GetPlayerColor();
		if (playerColor == null)
		{
			return;
		}
		PlayerProfile component = base.GetComponent<PlayerProfile>();
		if (component == null)
		{
			return;
		}
		this.CmdChangePlayerColor(component.steamId, playerColor.Value);
		this.SavePlayerColorToSteamLobby(component.steamId, playerColor.Value);
	}

	// Token: 0x06001279 RID: 4729 RVA: 0x0004FD90 File Offset: 0x0004DF90
	[Command]
	private void CmdChangePlayerColor(ulong steamId, Color newColor)
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		writer.WriteVarULong(steamId);
		writer.WriteColor(newColor);
		base.SendCommandInternal("System.Void PlayerCustomization::CmdChangePlayerColor(System.UInt64,UnityEngine.Color)", 615657454, writer, 0, true);
		NetworkWriterPool.Return(writer);
	}

	// Token: 0x0600127A RID: 4730 RVA: 0x0004FDD4 File Offset: 0x0004DFD4
	[ClientRpc]
	private void RpcUpdatePlayerColorOnClients(ulong steamId, Color newColor)
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		writer.WriteVarULong(steamId);
		writer.WriteColor(newColor);
		this.SendRPCInternal("System.Void PlayerCustomization::RpcUpdatePlayerColorOnClients(System.UInt64,UnityEngine.Color)", 2115703653, writer, 0, true);
		NetworkWriterPool.Return(writer);
	}

	// Token: 0x0600127B RID: 4731 RVA: 0x0004FE18 File Offset: 0x0004E018
	private void SavePlayerColorToSteamLobby(ulong steamId, Color color)
	{
		if (!SteamManager.Initialized)
		{
			return;
		}
		LobbySettings lobbySettings = this.lobbySettings;
		CSteamID? csteamID = (lobbySettings != null) ? new CSteamID?(lobbySettings.steamLobbyID) : null;
		CSteamID nil = CSteamID.Nil;
		if (csteamID != null && (csteamID == null || csteamID.GetValueOrDefault() == nil))
		{
			return;
		}
		if (steamId != SteamUser.GetSteamID().m_SteamID)
		{
			return;
		}
		string pchValue = ColorHexUtility.ColorToHex(color);
		SteamMatchmaking.SetLobbyMemberData(this.lobbySettings.steamLobbyID, "PlayerColor", pchValue);
	}

	// Token: 0x0600127C RID: 4732 RVA: 0x0004FEA6 File Offset: 0x0004E0A6
	public Dictionary<CosmeticType, int> GetEquippedCosmetics()
	{
		return new Dictionary<CosmeticType, int>(this.equippedCosmetics);
	}

	// Token: 0x0600127D RID: 4733 RVA: 0x0004FEB4 File Offset: 0x0004E0B4
	private MeshFilter GetMeshFilterForType(CosmeticType type)
	{
		MeshFilter result;
		switch (type)
		{
		case CosmeticType.Hat:
			result = this.hat;
			break;
		case CosmeticType.Hair:
			result = this.hair;
			break;
		case CosmeticType.Mustache:
			result = this.mustache;
			break;
		case CosmeticType.Beard:
			result = this.beard;
			break;
		case CosmeticType.Neckwear:
			result = this.neckwear;
			break;
		case CosmeticType.Clothing:
			result = this.clothing;
			break;
		case CosmeticType.Facewear:
			result = this.facewear;
			break;
		default:
			result = null;
			break;
		}
		return result;
	}

	// Token: 0x0600127E RID: 4734 RVA: 0x0004FF28 File Offset: 0x0004E128
	private void CreateShadowOnlyDuplicate(CosmeticType type, MeshFilter parentFilter, Mesh mesh, Material fallbackMaterial)
	{
		GameObject gameObject;
		if (this.shadowOnlyClones.TryGetValue(type, out gameObject) && gameObject != null)
		{
			Object.Destroy(gameObject);
			this.shadowOnlyClones.Remove(type);
		}
		Transform transform = parentFilter.transform;
		GameObject gameObject2 = new GameObject("ShadowOnly_" + type.ToString());
		gameObject2.transform.SetParent(transform, false);
		gameObject2.transform.localPosition = Vector3.zero;
		gameObject2.transform.localRotation = Quaternion.identity;
		gameObject2.transform.localScale = Vector3.one;
		gameObject2.layer = 0;
		gameObject2.AddComponent<MeshFilter>().sharedMesh = mesh;
		MeshRenderer meshRenderer = gameObject2.AddComponent<MeshRenderer>();
		MeshRenderer component = parentFilter.GetComponent<MeshRenderer>();
		Material material = (component != null) ? component.sharedMaterial : null;
		meshRenderer.sharedMaterial = ((material != null) ? material : fallbackMaterial);
		meshRenderer.shadowCastingMode = ShadowCastingMode.ShadowsOnly;
		meshRenderer.receiveShadows = false;
		this.shadowOnlyClones[type] = gameObject2;
	}

	// Token: 0x06001280 RID: 4736 RVA: 0x00002321 File Offset: 0x00000521
	public override bool Weaved()
	{
		return true;
	}

	// Token: 0x06001281 RID: 4737 RVA: 0x0005003A File Offset: 0x0004E23A
	protected void UserCode_CmdChangeCustomization__Int32__Boolean(int cosmeticId, bool shouldSave)
	{
		this.RpcChangeCustomization(cosmeticId, shouldSave);
	}

	// Token: 0x06001282 RID: 4738 RVA: 0x00050044 File Offset: 0x0004E244
	protected static void InvokeUserCode_CmdChangeCustomization__Int32__Boolean(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdChangeCustomization called on client.");
			return;
		}
		((PlayerCustomization)obj).UserCode_CmdChangeCustomization__Int32__Boolean(reader.ReadVarInt(), reader.ReadBool());
	}

	// Token: 0x06001283 RID: 4739 RVA: 0x00050073 File Offset: 0x0004E273
	protected void UserCode_RpcChangeCustomization__Int32__Boolean(int cosmeticId, bool shouldSave)
	{
		this.ApplyCosmetic(cosmeticId, shouldSave);
	}

	// Token: 0x06001284 RID: 4740 RVA: 0x0005007D File Offset: 0x0004E27D
	protected static void InvokeUserCode_RpcChangeCustomization__Int32__Boolean(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcChangeCustomization called on server.");
			return;
		}
		((PlayerCustomization)obj).UserCode_RpcChangeCustomization__Int32__Boolean(reader.ReadVarInt(), reader.ReadBool());
	}

	// Token: 0x06001285 RID: 4741 RVA: 0x000500AC File Offset: 0x0004E2AC
	protected void UserCode_CmdResetCustomization()
	{
		this.RpcResetCustomization();
	}

	// Token: 0x06001286 RID: 4742 RVA: 0x000500B4 File Offset: 0x0004E2B4
	protected static void InvokeUserCode_CmdResetCustomization(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdResetCustomization called on client.");
			return;
		}
		((PlayerCustomization)obj).UserCode_CmdResetCustomization();
	}

	// Token: 0x06001287 RID: 4743 RVA: 0x000500D8 File Offset: 0x0004E2D8
	protected void UserCode_RpcResetCustomization()
	{
		this.ClearAllCosmetics(false);
		int num = (MonoSingleton<CosmeticsUnlockManager>.Instance != null) ? MonoSingleton<CosmeticsUnlockManager>.Instance.GetDefaultClothingCosmeticId() : -1;
		if (num > 0 && CosmeticDataManager.HasCosmetic(num))
		{
			this.ApplyCosmetic(num, true);
			return;
		}
		if (base.isLocalPlayer)
		{
			this.SaveCosmetics();
		}
	}

	// Token: 0x06001288 RID: 4744 RVA: 0x0005012A File Offset: 0x0004E32A
	protected static void InvokeUserCode_RpcResetCustomization(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcResetCustomization called on server.");
			return;
		}
		((PlayerCustomization)obj).UserCode_RpcResetCustomization();
	}

	// Token: 0x06001289 RID: 4745 RVA: 0x0005014D File Offset: 0x0004E34D
	protected void UserCode_CmdClearCategory__CosmeticType(CosmeticType category)
	{
		this.RpcClearCategory(category);
	}

	// Token: 0x0600128A RID: 4746 RVA: 0x00050156 File Offset: 0x0004E356
	protected static void InvokeUserCode_CmdClearCategory__CosmeticType(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdClearCategory called on client.");
			return;
		}
		((PlayerCustomization)obj).UserCode_CmdClearCategory__CosmeticType(Mirror.GeneratedNetworkCode._Read_CosmeticType(reader));
	}

	// Token: 0x0600128B RID: 4747 RVA: 0x00050180 File Offset: 0x0004E380
	protected void UserCode_RpcClearCategory__CosmeticType(CosmeticType category)
	{
		GameObject gameObject;
		if (this.shadowOnlyClones.TryGetValue(category, out gameObject))
		{
			if (gameObject != null)
			{
				Object.Destroy(gameObject);
			}
			this.shadowOnlyClones.Remove(category);
		}
		MeshFilter meshFilterForType = this.GetMeshFilterForType(category);
		if (meshFilterForType != null)
		{
			meshFilterForType.mesh = null;
		}
		this.equippedCosmetics.Remove(category);
		if (base.isLocalPlayer)
		{
			this.SaveCosmetics();
		}
	}

	// Token: 0x0600128C RID: 4748 RVA: 0x000501EC File Offset: 0x0004E3EC
	protected static void InvokeUserCode_RpcClearCategory__CosmeticType(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcClearCategory called on server.");
			return;
		}
		((PlayerCustomization)obj).UserCode_RpcClearCategory__CosmeticType(Mirror.GeneratedNetworkCode._Read_CosmeticType(reader));
	}

	// Token: 0x0600128D RID: 4749 RVA: 0x00050215 File Offset: 0x0004E415
	protected void UserCode_CmdChangePlayerColor__UInt64__Color(ulong steamId, Color newColor)
	{
		LobbySettings lobbySettings = this.lobbySettings;
		if (lobbySettings != null)
		{
			lobbySettings.UpdatePlayerColor(steamId, newColor);
		}
		this.RpcUpdatePlayerColorOnClients(steamId, newColor);
	}

	// Token: 0x0600128E RID: 4750 RVA: 0x00050232 File Offset: 0x0004E432
	protected static void InvokeUserCode_CmdChangePlayerColor__UInt64__Color(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdChangePlayerColor called on client.");
			return;
		}
		((PlayerCustomization)obj).UserCode_CmdChangePlayerColor__UInt64__Color(reader.ReadVarULong(), reader.ReadColor());
	}

	// Token: 0x0600128F RID: 4751 RVA: 0x00050261 File Offset: 0x0004E461
	protected void UserCode_RpcUpdatePlayerColorOnClients__UInt64__Color(ulong steamId, Color newColor)
	{
		LobbySettings lobbySettings = this.lobbySettings;
		if (lobbySettings != null)
		{
			lobbySettings.UpdatePlayerColor(steamId, newColor);
		}
		if (SteamManager.Initialized)
		{
			this.SavePlayerColorToSteamLobby(steamId, newColor);
		}
	}

	// Token: 0x06001290 RID: 4752 RVA: 0x00050285 File Offset: 0x0004E485
	protected static void InvokeUserCode_RpcUpdatePlayerColorOnClients__UInt64__Color(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcUpdatePlayerColorOnClients called on server.");
			return;
		}
		((PlayerCustomization)obj).UserCode_RpcUpdatePlayerColorOnClients__UInt64__Color(reader.ReadVarULong(), reader.ReadColor());
	}

	// Token: 0x06001291 RID: 4753 RVA: 0x000502B4 File Offset: 0x0004E4B4
	static PlayerCustomization()
	{
		RemoteProcedureCalls.RegisterCommand(typeof(PlayerCustomization), "System.Void PlayerCustomization::CmdChangeCustomization(System.Int32,System.Boolean)", new RemoteCallDelegate(PlayerCustomization.InvokeUserCode_CmdChangeCustomization__Int32__Boolean), true);
		RemoteProcedureCalls.RegisterCommand(typeof(PlayerCustomization), "System.Void PlayerCustomization::CmdResetCustomization()", new RemoteCallDelegate(PlayerCustomization.InvokeUserCode_CmdResetCustomization), true);
		RemoteProcedureCalls.RegisterCommand(typeof(PlayerCustomization), "System.Void PlayerCustomization::CmdClearCategory(CosmeticType)", new RemoteCallDelegate(PlayerCustomization.InvokeUserCode_CmdClearCategory__CosmeticType), true);
		RemoteProcedureCalls.RegisterCommand(typeof(PlayerCustomization), "System.Void PlayerCustomization::CmdChangePlayerColor(System.UInt64,UnityEngine.Color)", new RemoteCallDelegate(PlayerCustomization.InvokeUserCode_CmdChangePlayerColor__UInt64__Color), true);
		RemoteProcedureCalls.RegisterRpc(typeof(PlayerCustomization), "System.Void PlayerCustomization::RpcChangeCustomization(System.Int32,System.Boolean)", new RemoteCallDelegate(PlayerCustomization.InvokeUserCode_RpcChangeCustomization__Int32__Boolean));
		RemoteProcedureCalls.RegisterRpc(typeof(PlayerCustomization), "System.Void PlayerCustomization::RpcResetCustomization()", new RemoteCallDelegate(PlayerCustomization.InvokeUserCode_RpcResetCustomization));
		RemoteProcedureCalls.RegisterRpc(typeof(PlayerCustomization), "System.Void PlayerCustomization::RpcClearCategory(CosmeticType)", new RemoteCallDelegate(PlayerCustomization.InvokeUserCode_RpcClearCategory__CosmeticType));
		RemoteProcedureCalls.RegisterRpc(typeof(PlayerCustomization), "System.Void PlayerCustomization::RpcUpdatePlayerColorOnClients(System.UInt64,UnityEngine.Color)", new RemoteCallDelegate(PlayerCustomization.InvokeUserCode_RpcUpdatePlayerColorOnClients__UInt64__Color));
	}

	// Token: 0x04000BC8 RID: 3016
	[Header("Cosmetic Mesh Filters")]
	[SerializeField]
	private MeshFilter hat;

	// Token: 0x04000BC9 RID: 3017
	[SerializeField]
	private MeshFilter hair;

	// Token: 0x04000BCA RID: 3018
	[SerializeField]
	private MeshFilter mustache;

	// Token: 0x04000BCB RID: 3019
	[SerializeField]
	private MeshFilter beard;

	// Token: 0x04000BCC RID: 3020
	[SerializeField]
	private MeshFilter neckwear;

	// Token: 0x04000BCD RID: 3021
	[SerializeField]
	private MeshFilter clothing;

	// Token: 0x04000BCE RID: 3022
	[SerializeField]
	private MeshFilter facewear;

	// Token: 0x04000BCF RID: 3023
	private Dictionary<CosmeticType, int> equippedCosmetics = new Dictionary<CosmeticType, int>();

	// Token: 0x04000BD0 RID: 3024
	private Dictionary<CosmeticType, GameObject> shadowOnlyClones = new Dictionary<CosmeticType, GameObject>();

	// Token: 0x04000BD1 RID: 3025
	private LobbySettings lobbySettings;

	// Token: 0x04000BD2 RID: 3026
	private const int ShadowOnlyLayer = 0;
}
