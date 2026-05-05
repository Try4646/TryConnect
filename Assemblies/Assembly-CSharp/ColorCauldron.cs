using System;
using System.Collections.Generic;
using Extensions;
using FMODUnity;
using Mirror;
using Mirror.RemoteCalls;
using Steamworks;
using UnityEngine;
using UnityEngine.UI;

// Token: 0x020000A8 RID: 168
public class ColorCauldron : NetworkBehaviour
{
	// Token: 0x06000687 RID: 1671 RVA: 0x0001BD42 File Offset: 0x00019F42
	private void Awake()
	{
		this._lobbySettings = Resources.Load<LobbySettings>("LobbySettings");
		this._mpb = new MaterialPropertyBlock();
		this.selectedColorPreviewRenderer.GetPropertyBlock(this._mpb);
		this.ApplyPaletteToTriggers();
	}

	// Token: 0x06000688 RID: 1672 RVA: 0x0001BD76 File Offset: 0x00019F76
	public override void OnStartServer()
	{
		base.OnStartServer();
		this.SubscribeTriggers();
	}

	// Token: 0x06000689 RID: 1673 RVA: 0x0001BD84 File Offset: 0x00019F84
	private void Start()
	{
		this._color = this.colorPalette.playerColors[0];
		this._mpb.SetColor("_BaseColor", this._color);
		this.selectedColorPreviewRenderer.SetPropertyBlock(this._mpb);
	}

	// Token: 0x0600068A RID: 1674 RVA: 0x0001BDC4 File Offset: 0x00019FC4
	private void SubscribeTriggers()
	{
		for (int i = 0; i < this.colorTriggers.Count; i++)
		{
			int num = i;
			UIColorPalette uicolorPalette = this.colorPalette;
			int? num2;
			if (uicolorPalette == null)
			{
				num2 = null;
			}
			else
			{
				Color[] playerColors = uicolorPalette.playerColors;
				num2 = ((playerColors != null) ? new int?(playerColors.Length) : null);
			}
			int? num3 = num2;
			if (num >= num3.GetValueOrDefault())
			{
				break;
			}
			int index = i;
			this.colorTriggers[i].serverOnInteractEvent.AddListener(delegate(PlayerInteract _)
			{
				this.ServerSetSelectedColor(index);
			});
		}
	}

	// Token: 0x0600068B RID: 1675 RVA: 0x0001BE5C File Offset: 0x0001A05C
	private void OnTriggerEnter(Collider other)
	{
		if (!this.IsLocalPlayerCollider(other))
		{
			return;
		}
		PlayerProfile playerProfile;
		if (other.attachedRigidbody && other.attachedRigidbody.TryGetComponent<PlayerProfile>(out playerProfile))
		{
			this.CmdChangePlayerColor(playerProfile.steamId, this._color);
		}
	}

	// Token: 0x0600068C RID: 1676 RVA: 0x0001BEA4 File Offset: 0x0001A0A4
	[Server]
	private void ServerSetSelectedColor(int index)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void ColorCauldron::ServerSetSelectedColor(System.Int32)' called when server was not active");
			return;
		}
		if (!this.colorPalette || this.colorPalette.playerColors == null || this.colorPalette.playerColors.Length == 0)
		{
			return;
		}
		int index2 = Mathf.Clamp(index, 0, this.colorPalette.playerColors.Length - 1);
		this.RpcSetSelectedColor(index2);
	}

	// Token: 0x0600068D RID: 1677 RVA: 0x0001BF10 File Offset: 0x0001A110
	[ClientRpc]
	private void RpcSetSelectedColor(int index)
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		writer.WriteVarInt(index);
		this.SendRPCInternal("System.Void ColorCauldron::RpcSetSelectedColor(System.Int32)", 1659102954, writer, 0, true);
		NetworkWriterPool.Return(writer);
	}

	// Token: 0x0600068E RID: 1678 RVA: 0x0001BF4C File Offset: 0x0001A14C
	private bool IsLocalPlayerCollider(Collider other)
	{
		NetworkIdentity componentInParent = other.GetComponentInParent<NetworkIdentity>();
		return componentInParent && componentInParent.isLocalPlayer;
	}

	// Token: 0x0600068F RID: 1679 RVA: 0x0001BF70 File Offset: 0x0001A170
	[Command(requiresAuthority = false)]
	private void CmdChangePlayerColor(ulong steamId, Color newColor)
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		writer.WriteVarULong(steamId);
		writer.WriteColor(newColor);
		base.SendCommandInternal("System.Void ColorCauldron::CmdChangePlayerColor(System.UInt64,UnityEngine.Color)", 1073795479, writer, 0, false);
		NetworkWriterPool.Return(writer);
	}

	// Token: 0x06000690 RID: 1680 RVA: 0x0001BFB4 File Offset: 0x0001A1B4
	private void ApplyPaletteToTriggers()
	{
		if (this.colorPalette == null || this.colorPalette.playerColors == null)
		{
			return;
		}
		if (this.colorTriggers == null || this.colorTriggers.Count == 0)
		{
			return;
		}
		int num = Mathf.Min(this.colorTriggers.Count, this.colorPalette.playerColors.Length);
		for (int i = 0; i < num; i++)
		{
			InteractableEventTrigger interactableEventTrigger = this.colorTriggers[i];
			if (!(interactableEventTrigger == null))
			{
				Color color = this.colorPalette.playerColors[i];
				Image image = interactableEventTrigger.GetComponent<Image>() ?? interactableEventTrigger.GetComponentInChildren<Image>();
				if (image != null)
				{
					image.color = color;
				}
				SpriteRenderer spriteRenderer = interactableEventTrigger.GetComponent<SpriteRenderer>() ?? interactableEventTrigger.GetComponentInChildren<SpriteRenderer>();
				if (spriteRenderer != null)
				{
					spriteRenderer.color = color;
				}
				foreach (MeshRenderer meshRenderer in interactableEventTrigger.GetComponentsInChildren<MeshRenderer>(true))
				{
					meshRenderer.material = new Material(meshRenderer.sharedMaterial);
					meshRenderer.material.color = color;
				}
			}
		}
	}

	// Token: 0x06000691 RID: 1681 RVA: 0x0001C0D4 File Offset: 0x0001A2D4
	[ClientRpc]
	private void RpcUpdatePlayerColorOnClients(ulong steamId, Color newColor)
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		writer.WriteVarULong(steamId);
		writer.WriteColor(newColor);
		this.SendRPCInternal("System.Void ColorCauldron::RpcUpdatePlayerColorOnClients(System.UInt64,UnityEngine.Color)", 859109838, writer, 0, true);
		NetworkWriterPool.Return(writer);
	}

	// Token: 0x06000692 RID: 1682 RVA: 0x0001C118 File Offset: 0x0001A318
	private void SavePlayerColorToSteamLobby(ulong steamId, Color color)
	{
		if (!SteamManager.Initialized)
		{
			return;
		}
		LobbySettings lobbySettings = Resources.Load<LobbySettings>("LobbySettings");
		if (lobbySettings == null || lobbySettings.steamLobbyID == CSteamID.Nil)
		{
			return;
		}
		ulong steamID = SteamUser.GetSteamID().m_SteamID;
		if (steamId != steamID)
		{
			return;
		}
		string pchValue = ColorHexUtility.ColorToHex(color);
		SteamMatchmaking.SetLobbyMemberData(lobbySettings.steamLobbyID, "PlayerColor", pchValue);
	}

	// Token: 0x06000694 RID: 1684 RVA: 0x00002321 File Offset: 0x00000521
	public override bool Weaved()
	{
		return true;
	}

	// Token: 0x06000695 RID: 1685 RVA: 0x0001C190 File Offset: 0x0001A390
	protected void UserCode_RpcSetSelectedColor__Int32(int index)
	{
		this._color = this.colorPalette.playerColors[index];
		this._mpb.SetColor("_BaseColor", this._color);
		this.selectedColorPreviewRenderer.SetPropertyBlock(this._mpb);
		SFXManager.SFXOneShot(this.colorChangeSFX, base.transform.position);
		PlayerProfile playerProfile;
		if (this.cauldronTrigger.bounds.Contains(NetworkClient.localPlayer.transform.position) && NetworkClient.localPlayer.TryGetComponent<PlayerProfile>(out playerProfile))
		{
			this.CmdChangePlayerColor(playerProfile.steamId, this._color);
		}
	}

	// Token: 0x06000696 RID: 1686 RVA: 0x0001C235 File Offset: 0x0001A435
	protected static void InvokeUserCode_RpcSetSelectedColor__Int32(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcSetSelectedColor called on server.");
			return;
		}
		((ColorCauldron)obj).UserCode_RpcSetSelectedColor__Int32(reader.ReadVarInt());
	}

	// Token: 0x06000697 RID: 1687 RVA: 0x0001C25E File Offset: 0x0001A45E
	protected void UserCode_CmdChangePlayerColor__UInt64__Color(ulong steamId, Color newColor)
	{
		this._lobbySettings.UpdatePlayerColor(steamId, newColor);
		this.RpcUpdatePlayerColorOnClients(steamId, newColor);
	}

	// Token: 0x06000698 RID: 1688 RVA: 0x0001C275 File Offset: 0x0001A475
	protected static void InvokeUserCode_CmdChangePlayerColor__UInt64__Color(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdChangePlayerColor called on client.");
			return;
		}
		((ColorCauldron)obj).UserCode_CmdChangePlayerColor__UInt64__Color(reader.ReadVarULong(), reader.ReadColor());
	}

	// Token: 0x06000699 RID: 1689 RVA: 0x0001C2A4 File Offset: 0x0001A4A4
	protected void UserCode_RpcUpdatePlayerColorOnClients__UInt64__Color(ulong steamId, Color newColor)
	{
		this._lobbySettings.UpdatePlayerColor(steamId, newColor);
		if (SteamManager.Initialized)
		{
			ulong steamID = SteamUser.GetSteamID().m_SteamID;
			if (steamId == steamID && MonoSingleton<CosmeticsUnlockManager>.Instance != null)
			{
				MonoSingleton<CosmeticsUnlockManager>.Instance.SetPlayerColor(newColor);
			}
			this.SavePlayerColorToSteamLobby(steamId, newColor);
		}
	}

	// Token: 0x0600069A RID: 1690 RVA: 0x0001C2F4 File Offset: 0x0001A4F4
	protected static void InvokeUserCode_RpcUpdatePlayerColorOnClients__UInt64__Color(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcUpdatePlayerColorOnClients called on server.");
			return;
		}
		((ColorCauldron)obj).UserCode_RpcUpdatePlayerColorOnClients__UInt64__Color(reader.ReadVarULong(), reader.ReadColor());
	}

	// Token: 0x0600069B RID: 1691 RVA: 0x0001C324 File Offset: 0x0001A524
	static ColorCauldron()
	{
		RemoteProcedureCalls.RegisterCommand(typeof(ColorCauldron), "System.Void ColorCauldron::CmdChangePlayerColor(System.UInt64,UnityEngine.Color)", new RemoteCallDelegate(ColorCauldron.InvokeUserCode_CmdChangePlayerColor__UInt64__Color), false);
		RemoteProcedureCalls.RegisterRpc(typeof(ColorCauldron), "System.Void ColorCauldron::RpcSetSelectedColor(System.Int32)", new RemoteCallDelegate(ColorCauldron.InvokeUserCode_RpcSetSelectedColor__Int32));
		RemoteProcedureCalls.RegisterRpc(typeof(ColorCauldron), "System.Void ColorCauldron::RpcUpdatePlayerColorOnClients(System.UInt64,UnityEngine.Color)", new RemoteCallDelegate(ColorCauldron.InvokeUserCode_RpcUpdatePlayerColorOnClients__UInt64__Color));
	}

	// Token: 0x04000455 RID: 1109
	[Header("Color Selection")]
	[SerializeField]
	private UIColorPalette colorPalette;

	// Token: 0x04000456 RID: 1110
	[SerializeField]
	private List<InteractableEventTrigger> colorTriggers = new List<InteractableEventTrigger>();

	// Token: 0x04000457 RID: 1111
	[SerializeField]
	private MeshRenderer selectedColorPreviewRenderer;

	// Token: 0x04000458 RID: 1112
	[SerializeField]
	private Collider cauldronTrigger;

	// Token: 0x04000459 RID: 1113
	[Header("SFX")]
	[SerializeField]
	private EventReference colorChangeSFX;

	// Token: 0x0400045A RID: 1114
	[SerializeField]
	private Color _color;

	// Token: 0x0400045B RID: 1115
	private LobbySettings _lobbySettings;

	// Token: 0x0400045C RID: 1116
	private MaterialPropertyBlock _mpb;
}
