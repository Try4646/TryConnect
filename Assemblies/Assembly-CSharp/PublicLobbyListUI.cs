using System;
using System.Collections.Generic;
using Extensions;
using Steamworks;
using UnityEngine;

// Token: 0x02000248 RID: 584
public class PublicLobbyListUI : MonoBehaviour
{
	// Token: 0x060014FE RID: 5374 RVA: 0x0005A0D8 File Offset: 0x000582D8
	private void Awake()
	{
		if (!SteamManager.Initialized)
		{
			if (this.rootPanel != null)
			{
				this.rootPanel.SetActive(false);
			}
			base.enabled = false;
			return;
		}
		this._lobbyMatchListResult = CallResult<LobbyMatchList_t>.Create(new CallResult<LobbyMatchList_t>.APIDispatchDelegate(this.OnLobbyMatchList));
	}

	// Token: 0x060014FF RID: 5375 RVA: 0x0005A128 File Offset: 0x00058328
	public void Show()
	{
		if (!SteamManager.Initialized)
		{
			Debug.LogWarning("[PublicLobbyListUI] Show: Steam not initialized.");
			return;
		}
		if (this.rootPanel != null && !this.rootPanel.activeSelf)
		{
			this.rootPanel.SetActive(true);
		}
		this.Refresh();
	}

	// Token: 0x06001500 RID: 5376 RVA: 0x0005A174 File Offset: 0x00058374
	public void Hide()
	{
		if (this.rootPanel != null && this.rootPanel.activeSelf)
		{
			this.rootPanel.SetActive(false);
		}
	}

	// Token: 0x06001501 RID: 5377 RVA: 0x0005A1A0 File Offset: 0x000583A0
	public void Refresh()
	{
		if (!SteamManager.Initialized)
		{
			return;
		}
		this.ClearEntries();
		SteamMatchmaking.AddRequestLobbyListStringFilter("GameStarted", "1", ELobbyComparison.k_ELobbyComparisonEqual);
		SteamMatchmaking.AddRequestLobbyListResultCountFilter(50);
		SteamAPICall_t steamAPICall_t = SteamMatchmaking.RequestLobbyList();
		if (steamAPICall_t == SteamAPICall_t.Invalid)
		{
			Debug.LogWarning("[PublicLobbyListUI] RequestLobbyList returned invalid handle.");
			return;
		}
		this._lobbyMatchListResult.Set(steamAPICall_t, null);
		Debug.Log("[PublicLobbyListUI] Lobby list requested, waiting for Steam callback...");
	}

	// Token: 0x06001502 RID: 5378 RVA: 0x0005A208 File Offset: 0x00058408
	private void OnLobbyMatchList(LobbyMatchList_t result, bool failure)
	{
		if (failure)
		{
			Debug.Log("[PublicLobbyListUI] Lobby list request failed (IO failure).");
			return;
		}
		if (result.m_nLobbiesMatching <= 0U)
		{
			Debug.Log("[PublicLobbyListUI] Lobby list: 0 public lobbies for this game.");
			return;
		}
		Debug.Log(string.Format("[PublicLobbyListUI] Lobby list: {0} lobbies.", result.m_nLobbiesMatching));
		if (this.contentRoot == null || this.lobbyEntryPrefab == null)
		{
			Debug.LogWarning("[PublicLobbyListUI] contentRoot or lobbyEntryPrefab is not set in the inspector - no entries will be created.");
			return;
		}
		int num = 0;
		while ((long)num < (long)((ulong)result.m_nLobbiesMatching))
		{
			CSteamID lobbyByIndex = SteamMatchmaking.GetLobbyByIndex(num);
			this.AddEntry(lobbyByIndex);
			num++;
		}
	}

	// Token: 0x06001503 RID: 5379 RVA: 0x0005A29C File Offset: 0x0005849C
	private void AddEntry(CSteamID lobbyId)
	{
		if (this.contentRoot == null || this.lobbyEntryPrefab == null)
		{
			return;
		}
		GameObject gameObject = Object.Instantiate<GameObject>(this.lobbyEntryPrefab, this.contentRoot);
		this._entries.Add(gameObject);
		string text = SteamMatchmaking.GetLobbyData(lobbyId, "name");
		if (string.IsNullOrEmpty(text))
		{
			text = "Lobby " + lobbyId.m_SteamID.ToString();
		}
		int numLobbyMembers = SteamMatchmaking.GetNumLobbyMembers(lobbyId);
		int lobbyMemberLimit = SteamMatchmaking.GetLobbyMemberLimit(lobbyId);
		string labelText = (lobbyMemberLimit > 0) ? string.Format("{0} ({1}/{2})", text, numLobbyMembers, lobbyMemberLimit) : string.Format("{0} ({1})", text, numLobbyMembers);
		PublicLobbyListEntry component = gameObject.GetComponent<PublicLobbyListEntry>();
		if (component != null)
		{
			component.Initialize(this, lobbyId, labelText);
			return;
		}
	}

	// Token: 0x06001504 RID: 5380 RVA: 0x0005A36C File Offset: 0x0005856C
	private void ClearEntries()
	{
		for (int i = 0; i < this._entries.Count; i++)
		{
			if (this._entries[i] != null)
			{
				Object.Destroy(this._entries[i]);
			}
		}
		this._entries.Clear();
	}

	// Token: 0x06001505 RID: 5381 RVA: 0x0005A3C0 File Offset: 0x000585C0
	public void JoinLobby(CSteamID lobbyId)
	{
		if (!SteamManager.Initialized)
		{
			Debug.LogWarning("[PublicLobbyListUI] JoinLobby: Steam not initialized.");
			return;
		}
		Debug.Log(string.Format("[PublicLobbyListUI] JoinLobby: joining lobby {0}", lobbyId.m_SteamID));
		LobbyManager instance = MonoSingleton<LobbyManager>.Instance;
		if (instance != null)
		{
			instance.CleanupCurrentLobby();
		}
		SteamMatchmaking.JoinLobby(lobbyId);
	}

	// Token: 0x04000D69 RID: 3433
	[Header("UI")]
	[SerializeField]
	private GameObject rootPanel;

	// Token: 0x04000D6A RID: 3434
	[SerializeField]
	private Transform contentRoot;

	// Token: 0x04000D6B RID: 3435
	[SerializeField]
	private GameObject lobbyEntryPrefab;

	// Token: 0x04000D6C RID: 3436
	private readonly List<GameObject> _entries = new List<GameObject>();

	// Token: 0x04000D6D RID: 3437
	private CallResult<LobbyMatchList_t> _lobbyMatchListResult;
}
