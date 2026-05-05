using System;
using System.Collections;
using System.Collections.Generic;
using Extensions;
using Steamworks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Token: 0x020002C5 RID: 709
public class LobbyMemberList : MonoBehaviour
{
	// Token: 0x06001916 RID: 6422 RVA: 0x00069954 File Offset: 0x00067B54
	private void Awake()
	{
		if (!SteamManager.Initialized)
		{
			base.enabled = false;
			return;
		}
		this.lobbySettings = Resources.Load<LobbySettings>("LobbySettings");
		this.colorPalette = Resources.Load<UIColorPalette>("ColorSettings");
		this.lobbyChatUpdateCallback = Callback<LobbyChatUpdate_t>.Create(new Callback<LobbyChatUpdate_t>.DispatchDelegate(this.OnLobbyMemberUpdate));
		this.personaStateCallback = Callback<PersonaStateChange_t>.Create(new Callback<PersonaStateChange_t>.DispatchDelegate(this.OnPersonaStateChange));
		this.avatarLoadedCallback = Callback<AvatarImageLoaded_t>.Create(new Callback<AvatarImageLoaded_t>.DispatchDelegate(this.OnAvatarImageLoaded));
		this.lobbyDataUpdateCallback = Callback<LobbyDataUpdate_t>.Create(new Callback<LobbyDataUpdate_t>.DispatchDelegate(this.OnLobbyDataUpdate));
	}

	// Token: 0x06001917 RID: 6423 RVA: 0x000699EC File Offset: 0x00067BEC
	private void OnEnable()
	{
		VersionMismatchManager.OnVersionMismatchChanged += this.OnVersionMismatchChanged;
		LobbyManager.OnLobbyEnteredEvent += this.OnLobbyEntered;
	}

	// Token: 0x06001918 RID: 6424 RVA: 0x00069A10 File Offset: 0x00067C10
	private void OnDisable()
	{
		VersionMismatchManager.OnVersionMismatchChanged -= this.OnVersionMismatchChanged;
		LobbyManager.OnLobbyEnteredEvent -= this.OnLobbyEntered;
	}

	// Token: 0x06001919 RID: 6425 RVA: 0x00069A34 File Offset: 0x00067C34
	private void OnVersionMismatchChanged(bool hasMismatch)
	{
		this.RefreshAllVersionMismatches();
	}

	// Token: 0x0600191A RID: 6426 RVA: 0x00069A3C File Offset: 0x00067C3C
	private void RefreshAllVersionMismatches()
	{
		if (this.lobbySettings == null || this.lobbySettings.steamLobbyID == CSteamID.Nil)
		{
			return;
		}
		CSteamID steamLobbyID = this.lobbySettings.steamLobbyID;
		CSteamID lobbyOwner = SteamMatchmaking.GetLobbyOwner(steamLobbyID);
		if (lobbyOwner == CSteamID.Nil)
		{
			return;
		}
		if (string.IsNullOrEmpty(SteamMatchmaking.GetLobbyMemberData(steamLobbyID, lobbyOwner, "GameVersion")))
		{
			return;
		}
		foreach (KeyValuePair<CSteamID, GameObject> keyValuePair in this.memberEntries)
		{
			this.CheckAndUpdateVersionMismatch(keyValuePair.Key, keyValuePair.Value);
		}
	}

	// Token: 0x0600191B RID: 6427 RVA: 0x00069AF8 File Offset: 0x00067CF8
	private void Start()
	{
		if (this.lobbySettings != null && this.lobbySettings.inALobby && this.lobbySettings.steamLobbyID != CSteamID.Nil)
		{
			this.SyncLocalPlayerColorToSteamLobby();
			base.StartCoroutine(this.DelayedInitialRefresh());
		}
	}

	// Token: 0x0600191C RID: 6428 RVA: 0x00069B4A File Offset: 0x00067D4A
	private IEnumerator DelayedInitialRefresh()
	{
		yield return new WaitForSeconds(0.5f);
		this.RefreshLobbyMembers();
		yield break;
	}

	// Token: 0x0600191D RID: 6429 RVA: 0x00069B5C File Offset: 0x00067D5C
	private void OnDestroy()
	{
		Callback<LobbyChatUpdate_t> callback = this.lobbyChatUpdateCallback;
		if (callback != null)
		{
			callback.Dispose();
		}
		Callback<PersonaStateChange_t> callback2 = this.personaStateCallback;
		if (callback2 != null)
		{
			callback2.Dispose();
		}
		Callback<AvatarImageLoaded_t> callback3 = this.avatarLoadedCallback;
		if (callback3 != null)
		{
			callback3.Dispose();
		}
		Callback<LobbyDataUpdate_t> callback4 = this.lobbyDataUpdateCallback;
		if (callback4 != null)
		{
			callback4.Dispose();
		}
		foreach (Texture2D obj in this.avatarTextures.Values)
		{
			Object.Destroy(obj);
		}
		this.avatarTextures.Clear();
	}

	// Token: 0x0600191E RID: 6430 RVA: 0x00069C00 File Offset: 0x00067E00
	private void RefreshLobbyMembers()
	{
		CSteamID currentLobbyID = MonoSingleton<LobbyManager>.Instance.GetCurrentLobbyID();
		if (currentLobbyID == CSteamID.Nil)
		{
			this.ClearAllEntries();
			return;
		}
		this.EnsureLocalPlayerVersionSet(currentLobbyID);
		int numLobbyMembers = SteamMatchmaking.GetNumLobbyMembers(currentLobbyID);
		HashSet<CSteamID> hashSet = new HashSet<CSteamID>();
		for (int i = 0; i < numLobbyMembers; i++)
		{
			CSteamID lobbyMemberByIndex = SteamMatchmaking.GetLobbyMemberByIndex(currentLobbyID, i);
			if (!(lobbyMemberByIndex == CSteamID.Nil))
			{
				hashSet.Add(lobbyMemberByIndex);
				string lobbyMemberData = SteamMatchmaking.GetLobbyMemberData(currentLobbyID, lobbyMemberByIndex, "PlayerColor");
				string lobbyMemberData2 = SteamMatchmaking.GetLobbyMemberData(currentLobbyID, lobbyMemberByIndex, "GameVersion");
				if (!string.IsNullOrEmpty(lobbyMemberData) && !string.IsNullOrEmpty(lobbyMemberData2))
				{
					if (!this.memberEntries.ContainsKey(lobbyMemberByIndex))
					{
						this.AddMemberEntry(lobbyMemberByIndex);
					}
					else
					{
						this.UpdateMemberEntry(lobbyMemberByIndex, null);
					}
				}
			}
		}
		List<CSteamID> list = new List<CSteamID>();
		foreach (CSteamID item in this.memberEntries.Keys)
		{
			if (!hashSet.Contains(item))
			{
				list.Add(item);
			}
		}
		foreach (CSteamID id in list)
		{
			this.RemoveMemberEntry(id);
		}
	}

	// Token: 0x0600191F RID: 6431 RVA: 0x00069D60 File Offset: 0x00067F60
	private void ClearAllEntries()
	{
		foreach (CSteamID id in new List<CSteamID>(this.memberEntries.Keys))
		{
			this.RemoveMemberEntry(id);
		}
	}

	// Token: 0x06001920 RID: 6432 RVA: 0x00069DC0 File Offset: 0x00067FC0
	private Transform FindChildRecursively(Transform parent, string name)
	{
		foreach (object obj in parent)
		{
			Transform transform = (Transform)obj;
			if (transform.name == name)
			{
				return transform;
			}
			Transform transform2 = this.FindChildRecursively(transform, name);
			if (transform2 != null)
			{
				return transform2;
			}
		}
		return null;
	}

	// Token: 0x06001921 RID: 6433 RVA: 0x00069E3C File Offset: 0x0006803C
	private void AddMemberEntry(CSteamID id)
	{
		if (this.memberEntries.ContainsKey(id))
		{
			return;
		}
		if (this.lobbySettings == null || this.lobbySettings.steamLobbyID == CSteamID.Nil)
		{
			return;
		}
		CSteamID steamLobbyID = this.lobbySettings.steamLobbyID;
		string lobbyMemberData = SteamMatchmaking.GetLobbyMemberData(steamLobbyID, id, "PlayerColor");
		string lobbyMemberData2 = SteamMatchmaking.GetLobbyMemberData(steamLobbyID, id, "GameVersion");
		if (string.IsNullOrEmpty(lobbyMemberData) || string.IsNullOrEmpty(lobbyMemberData2))
		{
			return;
		}
		GameObject gameObject = Object.Instantiate<GameObject>(this.memberEntryPrefab, this.memberListContainer);
		this.memberEntries[id] = gameObject;
		this.UpdateMemberEntry(id, gameObject);
	}

	// Token: 0x06001922 RID: 6434 RVA: 0x00069EDC File Offset: 0x000680DC
	private void UpdateMemberEntry(CSteamID id, GameObject entry = null)
	{
		if (entry == null && !this.memberEntries.TryGetValue(id, out entry))
		{
			return;
		}
		TextMeshProUGUI componentInChildren = entry.GetComponentInChildren<TextMeshProUGUI>();
		if (componentInChildren != null)
		{
			componentInChildren.text = SteamFriends.GetFriendPersonaName(id);
		}
		Transform transform = this.FindChildRecursively(entry.transform, "Avatar");
		RawImage rawImage = (transform != null) ? transform.GetComponent<RawImage>() : null;
		if (rawImage == null)
		{
			rawImage = entry.GetComponentInChildren<RawImage>();
		}
		if (rawImage != null)
		{
			this.RequestOrAssignAvatar(id, rawImage);
		}
		SteamIdComponent component = entry.GetComponent<SteamIdComponent>();
		if (component != null)
		{
			component.SetSteamID(id.m_SteamID);
		}
		this.CheckAndUpdateVersionMismatch(id, entry);
	}

	// Token: 0x06001923 RID: 6435 RVA: 0x00069F84 File Offset: 0x00068184
	private void CheckAndUpdateVersionMismatch(CSteamID memberID, GameObject entry)
	{
		if (this.lobbySettings == null || this.lobbySettings.steamLobbyID == CSteamID.Nil)
		{
			return;
		}
		CSteamID steamLobbyID = this.lobbySettings.steamLobbyID;
		CSteamID lobbyOwner = SteamMatchmaking.GetLobbyOwner(steamLobbyID);
		if (lobbyOwner == CSteamID.Nil)
		{
			return;
		}
		string lobbyMemberData = SteamMatchmaking.GetLobbyMemberData(steamLobbyID, lobbyOwner, "GameVersion");
		if (string.IsNullOrEmpty(lobbyMemberData))
		{
			return;
		}
		string lobbyMemberData2 = SteamMatchmaking.GetLobbyMemberData(steamLobbyID, memberID, "GameVersion");
		if (memberID == lobbyOwner)
		{
			return;
		}
		VersionMismatchDisplay component = entry.GetComponent<VersionMismatchDisplay>();
		if (component != null)
		{
			if (string.IsNullOrEmpty(lobbyMemberData2) || lobbyMemberData2 != lobbyMemberData)
			{
				component.ShowVersionMismatch(lobbyMemberData2 ?? "unknown");
				return;
			}
			component.HideVersionMismatch();
		}
	}

	// Token: 0x06001924 RID: 6436 RVA: 0x0006A044 File Offset: 0x00068244
	public void UpdatePlayerVersionMismatch(CSteamID playerID, string playerVersion, bool hasMismatch)
	{
		GameObject gameObject;
		if (!this.memberEntries.TryGetValue(playerID, out gameObject))
		{
			return;
		}
		VersionMismatchDisplay component = gameObject.GetComponent<VersionMismatchDisplay>();
		if (component != null)
		{
			if (hasMismatch)
			{
				component.ShowVersionMismatch(playerVersion);
				return;
			}
			component.HideVersionMismatch();
		}
	}

	// Token: 0x06001925 RID: 6437 RVA: 0x0006A084 File Offset: 0x00068284
	private void RemoveMemberEntry(CSteamID id)
	{
		GameObject obj;
		if (!this.memberEntries.TryGetValue(id, out obj))
		{
			return;
		}
		Object.Destroy(obj);
		this.memberEntries.Remove(id);
		this.pendingAvatars.Remove(id);
	}

	// Token: 0x06001926 RID: 6438 RVA: 0x0006A0C4 File Offset: 0x000682C4
	private void RequestOrAssignAvatar(CSteamID id, RawImage target)
	{
		Texture2D texture;
		if (this.avatarTextures.TryGetValue(id, out texture))
		{
			target.texture = texture;
			return;
		}
		int largeFriendAvatar = SteamFriends.GetLargeFriendAvatar(id);
		if (largeFriendAvatar == -1)
		{
			this.pendingAvatars[id] = target;
			return;
		}
		this.LoadTextureFromHandle(largeFriendAvatar, id, target);
	}

	// Token: 0x06001927 RID: 6439 RVA: 0x0006A10C File Offset: 0x0006830C
	private void OnAvatarImageLoaded(AvatarImageLoaded_t cb)
	{
		CSteamID steamID = cb.m_steamID;
		RawImage target;
		if (!this.pendingAvatars.TryGetValue(steamID, out target))
		{
			return;
		}
		this.LoadTextureFromHandle(cb.m_iImage, steamID, target);
		this.pendingAvatars.Remove(steamID);
	}

	// Token: 0x06001928 RID: 6440 RVA: 0x0006A14C File Offset: 0x0006834C
	private void LoadTextureFromHandle(int handle, CSteamID id, RawImage target)
	{
		if (handle <= 0)
		{
			return;
		}
		uint num;
		uint num2;
		if (!SteamUtils.GetImageSize(handle, out num, out num2))
		{
			return;
		}
		byte[] array = new byte[num * num2 * 4U];
		if (!SteamUtils.GetImageRGBA(handle, array, array.Length))
		{
			return;
		}
		Texture2D texture2D = new Texture2D((int)num, (int)num2, TextureFormat.RGBA32, false);
		texture2D.LoadRawTextureData(array);
		texture2D.Apply();
		this.avatarTextures[id] = texture2D;
		target.texture = texture2D;
	}

	// Token: 0x06001929 RID: 6441 RVA: 0x0006A1B0 File Offset: 0x000683B0
	private void OnLobbyMemberUpdate(LobbyChatUpdate_t cb)
	{
		if ((CSteamID)cb.m_ulSteamIDLobby != this.lobbySettings.steamLobbyID)
		{
			return;
		}
		CSteamID id = new CSteamID(cb.m_ulSteamIDUserChanged);
		uint rgfChatMemberStateChange = cb.m_rgfChatMemberStateChange;
		if ((rgfChatMemberStateChange & 1U) != 0U)
		{
			base.StartCoroutine(this.DelayedRefresh());
			return;
		}
		if ((rgfChatMemberStateChange & 30U) != 0U)
		{
			this.RemoveMemberEntry(id);
		}
	}

	// Token: 0x0600192A RID: 6442 RVA: 0x0006A20F File Offset: 0x0006840F
	private IEnumerator DelayedRefresh()
	{
		yield return new WaitForSeconds(0.5f);
		this.RefreshLobbyMembers();
		yield break;
	}

	// Token: 0x0600192B RID: 6443 RVA: 0x0006A220 File Offset: 0x00068420
	private void OnPersonaStateChange(PersonaStateChange_t cb)
	{
		CSteamID csteamID = new CSteamID(cb.m_ulSteamID);
		GameObject gameObject;
		if (this.memberEntries.TryGetValue(csteamID, out gameObject))
		{
			Transform transform = this.FindChildRecursively(gameObject.transform, "Avatar");
			RawImage rawImage = (transform != null) ? transform.GetComponent<RawImage>() : null;
			if (rawImage == null)
			{
				rawImage = gameObject.GetComponentInChildren<RawImage>();
			}
			if (rawImage != null)
			{
				this.RequestOrAssignAvatar(csteamID, rawImage);
			}
		}
	}

	// Token: 0x0600192C RID: 6444 RVA: 0x0006A289 File Offset: 0x00068489
	public void OnLobbyEntered()
	{
		this.SyncLocalPlayerColorToSteamLobby();
		base.StartCoroutine(this.DelayedInitialRefresh());
	}

	// Token: 0x0600192D RID: 6445 RVA: 0x0006A2A0 File Offset: 0x000684A0
	private void EnsureLocalPlayerVersionSet(CSteamID lobbyId)
	{
		if (!SteamManager.Initialized || lobbyId == CSteamID.Nil)
		{
			return;
		}
		CSteamID steamID = SteamUser.GetSteamID();
		if (string.IsNullOrEmpty(SteamMatchmaking.GetLobbyMemberData(lobbyId, steamID, "GameVersion")))
		{
			SteamMatchmaking.SetLobbyMemberData(lobbyId, "GameVersion", Application.version);
		}
	}

	// Token: 0x0600192E RID: 6446 RVA: 0x0006A2EC File Offset: 0x000684EC
	private void SyncLocalPlayerColorToSteamLobby()
	{
		if (!SteamManager.Initialized)
		{
			return;
		}
		if (this.lobbySettings == null || this.lobbySettings.steamLobbyID == CSteamID.Nil)
		{
			return;
		}
		if (MonoSingleton<CosmeticsUnlockManager>.Instance == null)
		{
			return;
		}
		Color? playerColor = MonoSingleton<CosmeticsUnlockManager>.Instance.GetPlayerColor();
		Color color = (playerColor != null) ? playerColor.Value : this.colorPalette.playerColors[Random.Range(0, this.colorPalette.playerColors.Length)];
		if (playerColor == null)
		{
			MonoSingleton<CosmeticsUnlockManager>.Instance.SetPlayerColor(color);
		}
		string pchValue = ColorHexUtility.ColorToHex(color);
		SteamMatchmaking.SetLobbyMemberData(this.lobbySettings.steamLobbyID, "PlayerColor", pchValue);
		SteamMatchmaking.SetLobbyMemberData(this.lobbySettings.steamLobbyID, "GameVersion", Application.version);
	}

	// Token: 0x0600192F RID: 6447 RVA: 0x0006A3C4 File Offset: 0x000685C4
	private void OnLobbyDataUpdate(LobbyDataUpdate_t cb)
	{
		if (cb.m_ulSteamIDMember == 0UL)
		{
			return;
		}
		if (this.lobbySettings == null || this.lobbySettings.steamLobbyID == CSteamID.Nil)
		{
			return;
		}
		if ((CSteamID)cb.m_ulSteamIDLobby != this.lobbySettings.steamLobbyID)
		{
			return;
		}
		CSteamID csteamID = new CSteamID(cb.m_ulSteamIDMember);
		CSteamID steamLobbyID = this.lobbySettings.steamLobbyID;
		string lobbyMemberData = SteamMatchmaking.GetLobbyMemberData(steamLobbyID, csteamID, "PlayerColor");
		string lobbyMemberData2 = SteamMatchmaking.GetLobbyMemberData(steamLobbyID, csteamID, "GameVersion");
		if (string.IsNullOrEmpty(lobbyMemberData) || string.IsNullOrEmpty(lobbyMemberData2))
		{
			return;
		}
		if (!this.memberEntries.ContainsKey(csteamID))
		{
			this.AddMemberEntry(csteamID);
			return;
		}
		this.UpdateMemberEntry(csteamID, null);
	}

	// Token: 0x06001930 RID: 6448 RVA: 0x0006A47E File Offset: 0x0006867E
	private void OnLobbyEnteredEvent()
	{
		this.OnLobbyEntered();
	}

	// Token: 0x04001029 RID: 4137
	[Header("UI")]
	[SerializeField]
	private Transform memberListContainer;

	// Token: 0x0400102A RID: 4138
	[SerializeField]
	private GameObject memberEntryPrefab;

	// Token: 0x0400102B RID: 4139
	private LobbySettings lobbySettings;

	// Token: 0x0400102C RID: 4140
	private UIColorPalette colorPalette;

	// Token: 0x0400102D RID: 4141
	private readonly Dictionary<CSteamID, GameObject> memberEntries = new Dictionary<CSteamID, GameObject>();

	// Token: 0x0400102E RID: 4142
	private readonly Dictionary<CSteamID, Texture2D> avatarTextures = new Dictionary<CSteamID, Texture2D>();

	// Token: 0x0400102F RID: 4143
	private readonly Dictionary<CSteamID, RawImage> pendingAvatars = new Dictionary<CSteamID, RawImage>();

	// Token: 0x04001030 RID: 4144
	private Callback<LobbyChatUpdate_t> lobbyChatUpdateCallback;

	// Token: 0x04001031 RID: 4145
	private Callback<PersonaStateChange_t> personaStateCallback;

	// Token: 0x04001032 RID: 4146
	private Callback<AvatarImageLoaded_t> avatarLoadedCallback;

	// Token: 0x04001033 RID: 4147
	private Callback<LobbyDataUpdate_t> lobbyDataUpdateCallback;
}
