using System;
using System.Runtime.InteropServices;
using Extensions;
using Mirror;
using SkyBrave_Toolkit.Scripts.Scriptable_Game_Events;
using Steamworks;
using TMPro;
using UnityEngine;

// Token: 0x02000320 RID: 800
public class PlayerProfile : NetworkBehaviour
{
	// Token: 0x06001ADC RID: 6876 RVA: 0x00071D05 File Offset: 0x0006FF05
	private void Start()
	{
		if (this.hasSynced)
		{
			this.SetPlayerNameTag();
		}
	}

	// Token: 0x06001ADD RID: 6877 RVA: 0x00071D18 File Offset: 0x0006FF18
	public override void OnStartClient()
	{
		base.OnStartClient();
		MonoSingleton<LocalManager>.Instance.RegisterPlayer(base.netIdentity);
		if (!base.isLocalPlayer)
		{
			base.enabled = false;
			return;
		}
		if (SteamManager.Initialized)
		{
			string personaName = SteamFriends.GetPersonaName();
			ulong steamID = SteamUser.GetSteamID().m_SteamID;
			Texture2D steamImageAsTexture = this.GetSteamImageAsTexture(SteamFriends.GetLargeFriendAvatar(new CSteamID(SteamUser.GetSteamID().m_SteamID)));
			this.SetVariables(personaName, steamID, steamImageAsTexture);
			return;
		}
		this.SetVariables("Guest", 0UL, null);
	}

	// Token: 0x06001ADE RID: 6878 RVA: 0x00071D96 File Offset: 0x0006FF96
	public override void OnStopClient()
	{
		base.OnStopClient();
		MonoSingleton<LocalManager>.Instance.UnregisterPlayer(base.netIdentity);
	}

	// Token: 0x06001ADF RID: 6879 RVA: 0x00071DB0 File Offset: 0x0006FFB0
	private void SetVariables(string profileName, ulong id, Texture2D profilePicture)
	{
		this.NetworkplayerName = profileName;
		this.NetworksteamId = id;
		this.NetworksteamProfilePicture = profilePicture;
		bool oldValue = this.hasSynced;
		this.NetworkhasSynced = true;
		if (!base.isServer)
		{
			this.OnSync(oldValue, this.hasSynced);
		}
	}

	// Token: 0x06001AE0 RID: 6880 RVA: 0x00071DF8 File Offset: 0x0006FFF8
	private void OnSync(bool oldValue, bool newValue)
	{
		if (oldValue == newValue)
		{
			return;
		}
		if (!newValue)
		{
			return;
		}
		if (this.clientOnPlayerProfileUpdated)
		{
			GameEvent gameEvent = this.clientOnPlayerProfileUpdated;
			if (gameEvent != null)
			{
				gameEvent.Raise();
			}
		}
		Action onPlayerProfileUpdated = this.OnPlayerProfileUpdated;
		if (onPlayerProfileUpdated != null)
		{
			onPlayerProfileUpdated();
		}
		this.SetPlayerNameTag();
		base.GetComponent<SteamIdComponent>().SetSteamID(this.steamId);
	}

	// Token: 0x06001AE1 RID: 6881 RVA: 0x00071E55 File Offset: 0x00070055
	private void SetPlayerNameTag()
	{
		if (this.playerNameLabel != null)
		{
			this.playerNameLabel.text = this.playerName;
		}
	}

	// Token: 0x06001AE2 RID: 6882 RVA: 0x00071E78 File Offset: 0x00070078
	private Texture2D GetSteamImageAsTexture(int imageHandle)
	{
		if (imageHandle == 0)
		{
			return null;
		}
		uint num;
		uint num2;
		if (SteamUtils.GetImageSize(imageHandle, out num, out num2))
		{
			byte[] array = new byte[num * num2 * 4U];
			if (SteamUtils.GetImageRGBA(imageHandle, array, (int)(num * num2 * 4U)))
			{
				Texture2D texture2D = new Texture2D((int)num, (int)num2, TextureFormat.RGBA32, false, true);
				texture2D.LoadRawTextureData(array);
				texture2D.Apply();
				return texture2D;
			}
		}
		return null;
	}

	// Token: 0x06001AE3 RID: 6883 RVA: 0x00071EC8 File Offset: 0x000700C8
	public PlayerProfile()
	{
		this._Mirror_SyncVarHookDelegate_hasSynced = new Action<bool, bool>(this.OnSync);
	}

	// Token: 0x06001AE4 RID: 6884 RVA: 0x00002321 File Offset: 0x00000521
	public override bool Weaved()
	{
		return true;
	}

	// Token: 0x17000276 RID: 630
	// (get) Token: 0x06001AE5 RID: 6885 RVA: 0x00071EE4 File Offset: 0x000700E4
	// (set) Token: 0x06001AE6 RID: 6886 RVA: 0x00071EF7 File Offset: 0x000700F7
	public string NetworkplayerName
	{
		get
		{
			return this.playerName;
		}
		[param: In]
		set
		{
			base.GeneratedSyncVarSetter<string>(value, ref this.playerName, 1UL, null);
		}
	}

	// Token: 0x17000277 RID: 631
	// (get) Token: 0x06001AE7 RID: 6887 RVA: 0x00071F14 File Offset: 0x00070114
	// (set) Token: 0x06001AE8 RID: 6888 RVA: 0x00071F27 File Offset: 0x00070127
	public ulong NetworksteamId
	{
		get
		{
			return this.steamId;
		}
		[param: In]
		set
		{
			base.GeneratedSyncVarSetter<ulong>(value, ref this.steamId, 2UL, null);
		}
	}

	// Token: 0x17000278 RID: 632
	// (get) Token: 0x06001AE9 RID: 6889 RVA: 0x00071F44 File Offset: 0x00070144
	// (set) Token: 0x06001AEA RID: 6890 RVA: 0x00071F57 File Offset: 0x00070157
	public Texture2D NetworksteamProfilePicture
	{
		get
		{
			return this.steamProfilePicture;
		}
		[param: In]
		set
		{
			base.GeneratedSyncVarSetter<Texture2D>(value, ref this.steamProfilePicture, 4UL, null);
		}
	}

	// Token: 0x17000279 RID: 633
	// (get) Token: 0x06001AEB RID: 6891 RVA: 0x00071F74 File Offset: 0x00070174
	// (set) Token: 0x06001AEC RID: 6892 RVA: 0x00071F87 File Offset: 0x00070187
	public bool NetworkhasSynced
	{
		get
		{
			return this.hasSynced;
		}
		[param: In]
		set
		{
			base.GeneratedSyncVarSetter<bool>(value, ref this.hasSynced, 8UL, this._Mirror_SyncVarHookDelegate_hasSynced);
		}
	}

	// Token: 0x06001AED RID: 6893 RVA: 0x00071FA8 File Offset: 0x000701A8
	public override void SerializeSyncVars(NetworkWriter writer, bool forceAll)
	{
		base.SerializeSyncVars(writer, forceAll);
		if (forceAll)
		{
			writer.WriteString(this.playerName);
			writer.WriteVarULong(this.steamId);
			writer.WriteTexture2D(this.steamProfilePicture);
			writer.WriteBool(this.hasSynced);
			return;
		}
		writer.WriteVarULong(this.syncVarDirtyBits);
		if ((this.syncVarDirtyBits & 1UL) != 0UL)
		{
			writer.WriteString(this.playerName);
		}
		if ((this.syncVarDirtyBits & 2UL) != 0UL)
		{
			writer.WriteVarULong(this.steamId);
		}
		if ((this.syncVarDirtyBits & 4UL) != 0UL)
		{
			writer.WriteTexture2D(this.steamProfilePicture);
		}
		if ((this.syncVarDirtyBits & 8UL) != 0UL)
		{
			writer.WriteBool(this.hasSynced);
		}
	}

	// Token: 0x06001AEE RID: 6894 RVA: 0x0007208C File Offset: 0x0007028C
	public override void DeserializeSyncVars(NetworkReader reader, bool initialState)
	{
		base.DeserializeSyncVars(reader, initialState);
		if (initialState)
		{
			base.GeneratedSyncVarDeserialize<string>(ref this.playerName, null, reader.ReadString());
			base.GeneratedSyncVarDeserialize<ulong>(ref this.steamId, null, reader.ReadVarULong());
			base.GeneratedSyncVarDeserialize<Texture2D>(ref this.steamProfilePicture, null, reader.ReadTexture2D());
			base.GeneratedSyncVarDeserialize<bool>(ref this.hasSynced, this._Mirror_SyncVarHookDelegate_hasSynced, reader.ReadBool());
			return;
		}
		long num = (long)reader.ReadVarULong();
		if ((num & 1L) != 0L)
		{
			base.GeneratedSyncVarDeserialize<string>(ref this.playerName, null, reader.ReadString());
		}
		if ((num & 2L) != 0L)
		{
			base.GeneratedSyncVarDeserialize<ulong>(ref this.steamId, null, reader.ReadVarULong());
		}
		if ((num & 4L) != 0L)
		{
			base.GeneratedSyncVarDeserialize<Texture2D>(ref this.steamProfilePicture, null, reader.ReadTexture2D());
		}
		if ((num & 8L) != 0L)
		{
			base.GeneratedSyncVarDeserialize<bool>(ref this.hasSynced, this._Mirror_SyncVarHookDelegate_hasSynced, reader.ReadBool());
		}
	}

	// Token: 0x040011AA RID: 4522
	[SyncVar]
	public string playerName;

	// Token: 0x040011AB RID: 4523
	[SyncVar]
	public ulong steamId;

	// Token: 0x040011AC RID: 4524
	[SyncVar]
	public Texture2D steamProfilePicture;

	// Token: 0x040011AD RID: 4525
	[SyncVar(hook = "OnSync")]
	public bool hasSynced;

	// Token: 0x040011AE RID: 4526
	[Space(10f)]
	public GameEvent clientOnPlayerProfileUpdated;

	// Token: 0x040011AF RID: 4527
	public Action OnPlayerProfileUpdated;

	// Token: 0x040011B0 RID: 4528
	[SerializeField]
	private TextMeshProUGUI playerNameLabel;

	// Token: 0x040011B1 RID: 4529
	public Action<bool, bool> _Mirror_SyncVarHookDelegate_hasSynced;
}
