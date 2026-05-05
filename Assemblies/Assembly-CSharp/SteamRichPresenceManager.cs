using System;
using Extensions;
using Steamworks;
using UnityEngine;
using UnityEngine.SceneManagement;

// Token: 0x020002EE RID: 750
public class SteamRichPresenceManager : MonoSingleton<SteamRichPresenceManager>
{
	// Token: 0x06001A02 RID: 6658 RVA: 0x0006D680 File Offset: 0x0006B880
	protected override void OnAwake()
	{
		base.OnAwake();
		this.lobbySettings = Resources.Load<LobbySettings>("LobbySettings");
		SceneManager.sceneLoaded += this.OnSceneLoaded;
	}

	// Token: 0x06001A03 RID: 6659 RVA: 0x0006D6A9 File Offset: 0x0006B8A9
	private bool HasMoneyManager()
	{
		return Object.FindFirstObjectByType<MoneyManager>() != null;
	}

	// Token: 0x06001A04 RID: 6660 RVA: 0x0006D6B6 File Offset: 0x0006B8B6
	private bool HasGameManager()
	{
		return Object.FindFirstObjectByType<GameManager>() != null;
	}

	// Token: 0x06001A05 RID: 6661 RVA: 0x0006D6C3 File Offset: 0x0006B8C3
	private void SubscribeToMoneyChanges()
	{
		if (this.HasMoneyManager())
		{
			MoneyManager instance = NetworkSingleton<MoneyManager>.Instance;
			instance.OnBalanceChanged = (Action<BalanceChangeData>)Delegate.Combine(instance.OnBalanceChanged, new Action<BalanceChangeData>(this.OnMoneyChanged));
		}
	}

	// Token: 0x06001A06 RID: 6662 RVA: 0x0006D6F3 File Offset: 0x0006B8F3
	private void UnsubscribeFromMoneyChanges()
	{
		if (this.HasMoneyManager())
		{
			MoneyManager instance = NetworkSingleton<MoneyManager>.Instance;
			instance.OnBalanceChanged = (Action<BalanceChangeData>)Delegate.Remove(instance.OnBalanceChanged, new Action<BalanceChangeData>(this.OnMoneyChanged));
		}
	}

	// Token: 0x06001A07 RID: 6663 RVA: 0x0006D723 File Offset: 0x0006B923
	private void OnDestroy()
	{
		SceneManager.sceneLoaded -= this.OnSceneLoaded;
		this.UnsubscribeFromMoneyChanges();
	}

	// Token: 0x06001A08 RID: 6664 RVA: 0x0006D73C File Offset: 0x0006B93C
	private void UpdateGroupPresence(int playerCount)
	{
		if (this.lobbySettings != null && this.lobbySettings.steamLobbyID != CSteamID.Nil)
		{
			SteamFriends.SetRichPresence("steam_player_group", this.lobbySettings.steamLobbyID.m_SteamID.ToString());
			SteamFriends.SetRichPresence("steam_player_group_size", playerCount.ToString());
		}
	}

	// Token: 0x06001A09 RID: 6665 RVA: 0x0006D7A0 File Offset: 0x0006B9A0
	private string BuildQuotaBar(long balance, long quota)
	{
		if (quota <= 0L)
		{
			return "";
		}
		int num = Mathf.RoundToInt(Mathf.Clamp01((float)balance / (float)quota) * 10f);
		return "[" + new string('#', num) + new string('-', 10 - num) + "]";
	}

	// Token: 0x06001A0A RID: 6666 RVA: 0x0006D7F0 File Offset: 0x0006B9F0
	private void OnMoneyChanged(BalanceChangeData data)
	{
		if (this.isMoneyPresenceUpdateOnCooldown)
		{
			return;
		}
		this.pendingMoneyPresenceUpdate = true;
	}

	// Token: 0x06001A0B RID: 6667 RVA: 0x0006D802 File Offset: 0x0006BA02
	public void Update()
	{
		if (this.isMoneyPresenceUpdateOnCooldown && Time.unscaledTime >= this.moneyPresenceCooldownEndTime)
		{
			this.isMoneyPresenceUpdateOnCooldown = false;
		}
		if (!this.pendingMoneyPresenceUpdate)
		{
			return;
		}
		if (this.isMoneyPresenceUpdateOnCooldown)
		{
			return;
		}
		this.TryUpdateMoneyDrivenPresence();
	}

	// Token: 0x06001A0C RID: 6668 RVA: 0x0006D838 File Offset: 0x0006BA38
	private void TryUpdateMoneyDrivenPresence()
	{
		if (!this.HasGameManager())
		{
			return;
		}
		if (NetworkSingleton<GameManager>.Instance.state == GameState.Lobby)
		{
			this.SetInHomePresence();
		}
		else
		{
			if (NetworkSingleton<GameManager>.Instance.state != GameState.Game)
			{
				return;
			}
			this.SetInGamePresence();
		}
		this.pendingMoneyPresenceUpdate = false;
		this.isMoneyPresenceUpdateOnCooldown = true;
		this.moneyPresenceCooldownEndTime = Time.unscaledTime + this.moneyPresenceCooldownDuration;
	}

	// Token: 0x06001A0D RID: 6669 RVA: 0x0006D898 File Offset: 0x0006BA98
	private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
	{
		if (!SteamManager.Initialized)
		{
			return;
		}
		string name = scene.name;
		if (name == "MainMenuScene")
		{
			this.SetMainMenuPresence();
			return;
		}
		if (name == "CasinoScene")
		{
			this.UnsubscribeFromMoneyChanges();
			this.SubscribeToMoneyChanges();
			return;
		}
		if (name == "LoseStateScene")
		{
			this.SetLoseScenePresence();
			return;
		}
		if (name == "WinStateScene")
		{
			this.SetWinScenePresence();
		}
	}

	// Token: 0x06001A0E RID: 6670 RVA: 0x0006D90C File Offset: 0x0006BB0C
	public void SetLoseScenePresence()
	{
		try
		{
			SteamFriends.SetRichPresence("status", "At the end of the line.");
			SteamFriends.SetRichPresence("steam_display", "#Status_Lose");
			this.SetConnectString(false);
			Debug.Log("Rich presence set to: Lose Scene");
		}
		catch (Exception ex)
		{
			Debug.LogWarning("Failed to set lose scene rich presence: " + ex.Message);
		}
	}

	// Token: 0x06001A0F RID: 6671 RVA: 0x0006D974 File Offset: 0x0006BB74
	public void SetWinScenePresence()
	{
		try
		{
			SteamFriends.SetRichPresence("status", "Deciding on something very important...");
			SteamFriends.SetRichPresence("steam_display", "#Status_Win");
			this.SetConnectString(false);
			Debug.Log("Rich presence set to: Win Scene");
		}
		catch (Exception ex)
		{
			Debug.LogWarning("Failed to set win scene rich presence: " + ex.Message);
		}
	}

	// Token: 0x06001A10 RID: 6672 RVA: 0x0006D9DC File Offset: 0x0006BBDC
	public void SetMainMenuPresence()
	{
		try
		{
			int playerCount = 0;
			int num = 0;
			if (this.lobbySettings != null && this.lobbySettings.steamLobbyID != CSteamID.Nil)
			{
				playerCount = SteamMatchmaking.GetNumLobbyMembers(this.lobbySettings.steamLobbyID);
				num = SteamMatchmaking.GetLobbyMemberLimit(this.lobbySettings.steamLobbyID);
			}
			else if (this.lobbySettings != null)
			{
				playerCount = this.lobbySettings.currentPlayerCount;
				num = this.lobbySettings.maxPlayers;
			}
			this.SetConnectString(true);
			this.UpdateGroupPresence(playerCount);
			SteamFriends.SetRichPresence("status", "Getting the gang together for an adventure");
			SteamFriends.SetRichPresence("steam_display", "#Status_MainMenu");
			SteamFriends.SetRichPresence("player_count", playerCount.ToString());
			SteamFriends.SetRichPresence("max_players", num.ToString());
			Debug.Log("Rich presence set to: Main Menu");
		}
		catch (Exception ex)
		{
			Debug.LogWarning("Failed to set main menu rich presence: " + ex.Message);
		}
	}

	// Token: 0x06001A11 RID: 6673 RVA: 0x0006DAE4 File Offset: 0x0006BCE4
	public void SetInHomePresence()
	{
		try
		{
			long amount = this.HasMoneyManager() ? NetworkSingleton<MoneyManager>.Instance.balance : 0L;
			int num = this.HasGameManager() ? NetworkSingleton<GameManager>.Instance.successfulQuota : 0;
			if (this.HasGameManager())
			{
				long currentQuota = NetworkSingleton<GameManager>.Instance.currentQuota;
			}
			string text = MoneyFormatter.FormatWithDollar(amount);
			int playerCount = 0;
			if (this.lobbySettings != null && this.lobbySettings.steamLobbyID != CSteamID.Nil)
			{
				playerCount = SteamMatchmaking.GetNumLobbyMembers(this.lobbySettings.steamLobbyID);
			}
			this.SetConnectString(true);
			this.UpdateGroupPresence(playerCount);
			int num2 = NetworkSingleton<GameManager>.Instance.daysPassed + 1;
			SteamFriends.SetRichPresence("status", string.Format("Home - Balance: {0} | Day {1}", text, num2));
			SteamFriends.SetRichPresence("steam_display", "#Status_InLobby");
			SteamFriends.SetRichPresence("money", text);
			SteamFriends.SetRichPresence("quota_number", num.ToString());
			SteamFriends.SetRichPresence("day_number", num2.ToString());
			this.UpdatePlayerCount();
			Debug.Log("Rich presence set to: In Lobby");
		}
		catch (Exception ex)
		{
			Debug.LogWarning("Failed to set lobby rich presence: " + ex.Message);
		}
	}

	// Token: 0x06001A12 RID: 6674 RVA: 0x0006DC30 File Offset: 0x0006BE30
	public void SetInGamePresence()
	{
		if (!SteamManager.Initialized)
		{
			return;
		}
		if (!this.HasGameManager() || NetworkSingleton<GameManager>.Instance.state != GameState.Game)
		{
			return;
		}
		try
		{
			long num = this.HasMoneyManager() ? NetworkSingleton<MoneyManager>.Instance.balance : 0L;
			long num2 = this.HasGameManager() ? NetworkSingleton<GameManager>.Instance.currentQuota : 0L;
			string text = MoneyFormatter.FormatWithDollar(num);
			string text2 = MoneyFormatter.FormatWithDollar(num2);
			int playerCount = 0;
			if (this.lobbySettings != null && this.lobbySettings.steamLobbyID != CSteamID.Nil)
			{
				playerCount = SteamMatchmaking.GetNumLobbyMembers(this.lobbySettings.steamLobbyID);
			}
			this.UpdateGroupPresence(playerCount);
			int num3 = Mathf.RoundToInt((num2 > 0L) ? (Mathf.Clamp01((float)num / (float)num2) * 100f) : 0f);
			string text3 = this.BuildQuotaBar(num, num2);
			SteamFriends.SetRichPresence("status", string.Format("Letting it ride - {0}/{1} ({2}%) {3}", new object[]
			{
				text,
				text2,
				num3,
				text3
			}));
			SteamFriends.SetRichPresence("steam_display", "#Status_Casino");
			SteamFriends.SetRichPresence("money", text);
			SteamFriends.SetRichPresence("quota_amount", text2);
			SteamFriends.SetRichPresence("quota_percent", num3.ToString());
			SteamFriends.SetRichPresence("quota_progress_bar", text3);
			SteamFriends.SetRichPresence("player_count", "");
			SteamFriends.SetRichPresence("max_players", "");
			this.SetConnectString(false);
			if (this.HasGameManager())
			{
				bool flag = NetworkSingleton<GameManager>.Instance.state == GameState.Game;
			}
			SceneManager.GetActiveScene().name == "CasinoScene";
			Debug.Log("Rich presence set to: In Game");
		}
		catch (Exception ex)
		{
			Debug.LogWarning("Failed to set in-game rich presence: " + ex.Message);
		}
	}

	// Token: 0x06001A13 RID: 6675 RVA: 0x0006DE1C File Offset: 0x0006C01C
	public void UpdatePlayerCount()
	{
		if (!SteamManager.Initialized)
		{
			return;
		}
		if (this.lobbySettings == null || this.lobbySettings.steamLobbyID == CSteamID.Nil)
		{
			return;
		}
		try
		{
			int numLobbyMembers = SteamMatchmaking.GetNumLobbyMembers(this.lobbySettings.steamLobbyID);
			int lobbyMemberLimit = SteamMatchmaking.GetLobbyMemberLimit(this.lobbySettings.steamLobbyID);
			SteamFriends.SetRichPresence("player_count", numLobbyMembers.ToString());
			SteamFriends.SetRichPresence("max_players", lobbyMemberLimit.ToString());
		}
		catch (Exception ex)
		{
			Debug.LogWarning("Failed to update player count in rich presence: " + ex.Message);
		}
	}

	// Token: 0x06001A14 RID: 6676 RVA: 0x0006DEC8 File Offset: 0x0006C0C8
	public void SetConnectString(bool connectable)
	{
		if (!connectable || this.lobbySettings == null || this.lobbySettings.steamLobbyID == CSteamID.Nil)
		{
			SteamFriends.SetRichPresence("connect", null);
			return;
		}
		SteamFriends.SetRichPresence("connect", string.Format("connect_lobby {0}", this.lobbySettings.steamLobbyID));
	}

	// Token: 0x06001A15 RID: 6677 RVA: 0x0006DF30 File Offset: 0x0006C130
	public void ClearRichPresence()
	{
		if (!SteamManager.Initialized)
		{
			return;
		}
		try
		{
			SteamFriends.ClearRichPresence();
			Debug.Log("Rich presence cleared");
		}
		catch (Exception ex)
		{
			Debug.LogWarning("Failed to clear rich presence: " + ex.Message);
		}
	}

	// Token: 0x040010AB RID: 4267
	private LobbySettings lobbySettings;

	// Token: 0x040010AC RID: 4268
	[SerializeField]
	private bool pendingMoneyPresenceUpdate;

	// Token: 0x040010AD RID: 4269
	[SerializeField]
	private float moneyPresenceCooldownDuration = 60f;

	// Token: 0x040010AE RID: 4270
	[SerializeField]
	private bool isMoneyPresenceUpdateOnCooldown;

	// Token: 0x040010AF RID: 4271
	private float moneyPresenceCooldownEndTime;
}
