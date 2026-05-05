using System;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using Extensions;
using FMODUnity;
using Mirror;
using Mirror.RemoteCalls;
using MoreMountains.Feedbacks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Token: 0x02000180 RID: 384
public class MoneyDisplayAndFeedbacks : NetworkSingleton<MoneyDisplayAndFeedbacks>
{
	// Token: 0x06000E5D RID: 3677 RVA: 0x0003B6CC File Offset: 0x000398CC
	public override void OnStartClient()
	{
		base.OnStartClient();
		SyncDictionary<string, long> playerProfitHistory = this.PlayerProfitHistory;
		playerProfitHistory.OnChange = (Action<SyncIDictionary<string, long>.Operation, string, long>)Delegate.Remove(playerProfitHistory.OnChange, new Action<SyncIDictionary<string, long>.Operation, string, long>(this.OnProfitHistoryChanged));
		SyncDictionary<string, long> playerProfitHistory2 = this.PlayerProfitHistory;
		playerProfitHistory2.OnChange = (Action<SyncIDictionary<string, long>.Operation, string, long>)Delegate.Combine(playerProfitHistory2.OnChange, new Action<SyncIDictionary<string, long>.Operation, string, long>(this.OnProfitHistoryChanged));
		this.UpdatePlayerHistoryLabel();
	}

	// Token: 0x06000E5E RID: 3678 RVA: 0x0003B733 File Offset: 0x00039933
	private void OnEnable()
	{
		this.SubscribeEvents();
		this.SubscribeLobbyEvents();
	}

	// Token: 0x06000E5F RID: 3679 RVA: 0x0003B741 File Offset: 0x00039941
	private void OnDisable()
	{
		this.UnsubscribeEvents();
		this.UnsubscribeLobbyEvents();
	}

	// Token: 0x06000E60 RID: 3680 RVA: 0x0003B750 File Offset: 0x00039950
	private void Start()
	{
		this._displayedTicketBalance = (double)NetworkSingleton<MoneyManager>.Instance.ticketBalance;
		this.ticketBalanceLabel.text = ((long)this._displayedTicketBalance).ToString("N0");
		this._displayedBalance = (double)NetworkSingleton<MoneyManager>.Instance.balance;
		this._displayedQuota = (double)((NetworkSingleton<GameManager>.Instance != null) ? NetworkSingleton<GameManager>.Instance.currentQuota : 0L);
		if (this.balanceLabel != null)
		{
			this.balanceLabel.text = MoneyFormatter.FormatWithDollar((long)this._displayedBalance);
		}
		if (this.quotaLabel != null)
		{
			this.quotaLabel.text = "/ " + MoneyFormatter.FormatWithDollar((long)this._displayedQuota);
		}
	}

	// Token: 0x06000E61 RID: 3681 RVA: 0x0003B814 File Offset: 0x00039A14
	protected override void OnDestroy()
	{
		base.OnDestroy();
		DOTween.Kill(this, false);
	}

	// Token: 0x06000E62 RID: 3682 RVA: 0x0003B824 File Offset: 0x00039A24
	private static void SetProfitEntryText(MoneyDisplayAndFeedbacks.HistoryEntry entry)
	{
		long num = (long)entry.displayedValue;
		entry.profitText.text = ((num >= 0L) ? ("+" + MoneyFormatter.FormatWithDollar(num)) : MoneyFormatter.FormatWithDollar(num));
		entry.profitText.color = ((num >= 0L) ? Color.green : Color.red);
	}

	// Token: 0x06000E63 RID: 3683 RVA: 0x0003B87D File Offset: 0x00039A7D
	private void PlayGoalReachedScale(Transform target)
	{
		if (target == null)
		{
			return;
		}
		target.DOPunchScale(Vector3.one * this.goalReachedPunchScale, this.goalReachedPunchDuration, 10, 1f).SetTarget(this);
	}

	// Token: 0x06000E64 RID: 3684 RVA: 0x0003B8B4 File Offset: 0x00039AB4
	private void SubscribeEvents()
	{
		if (NetworkSingleton<MoneyManager>.Instance != null)
		{
			MoneyManager instance = NetworkSingleton<MoneyManager>.Instance;
			instance.OnBalanceChanged = (Action<BalanceChangeData>)Delegate.Combine(instance.OnBalanceChanged, new Action<BalanceChangeData>(this.UpdateDisplay));
			MoneyManager instance2 = NetworkSingleton<MoneyManager>.Instance;
			instance2.OnTicketBalanceChanged = (Action<long>)Delegate.Combine(instance2.OnTicketBalanceChanged, new Action<long>(this.UpdateTicketDisplay));
		}
		if (NetworkSingleton<GameManager>.Instance != null)
		{
			NetworkSingleton<GameManager>.Instance.OnQuotaChangedEvent += this.OnQuotaChanged;
		}
	}

	// Token: 0x06000E65 RID: 3685 RVA: 0x0003B940 File Offset: 0x00039B40
	private void UnsubscribeEvents()
	{
		if (NetworkSingleton<MoneyManager>.Instance != null)
		{
			MoneyManager instance = NetworkSingleton<MoneyManager>.Instance;
			instance.OnBalanceChanged = (Action<BalanceChangeData>)Delegate.Remove(instance.OnBalanceChanged, new Action<BalanceChangeData>(this.UpdateDisplay));
			MoneyManager instance2 = NetworkSingleton<MoneyManager>.Instance;
			instance2.OnTicketBalanceChanged = (Action<long>)Delegate.Remove(instance2.OnTicketBalanceChanged, new Action<long>(this.UpdateTicketDisplay));
		}
		if (NetworkSingleton<GameManager>.Instance != null)
		{
			NetworkSingleton<GameManager>.Instance.OnQuotaChangedEvent -= this.OnQuotaChanged;
		}
	}

	// Token: 0x06000E66 RID: 3686 RVA: 0x0003B9CC File Offset: 0x00039BCC
	private void SubscribeLobbyEvents()
	{
		if (this._lobbySettings == null)
		{
			this._lobbySettings = Resources.Load<LobbySettings>("LobbySettings");
		}
		LobbySettings.SettingsChanged -= this.OnLobbySettingsChanged;
		LobbySettings.SettingsChanged += this.OnLobbySettingsChanged;
	}

	// Token: 0x06000E67 RID: 3687 RVA: 0x0003BA19 File Offset: 0x00039C19
	private void UnsubscribeLobbyEvents()
	{
		LobbySettings.SettingsChanged -= this.OnLobbySettingsChanged;
	}

	// Token: 0x06000E68 RID: 3688 RVA: 0x0003BA2C File Offset: 0x00039C2C
	private void OnLobbySettingsChanged(LobbySettings _)
	{
		this.UpdatePlayerHistoryLabel();
	}

	// Token: 0x06000E69 RID: 3689 RVA: 0x0003BA2C File Offset: 0x00039C2C
	private void OnProfitHistoryChanged(SyncIDictionary<string, long>.Operation op, string key, long value)
	{
		this.UpdatePlayerHistoryLabel();
	}

	// Token: 0x06000E6A RID: 3690 RVA: 0x0003BA34 File Offset: 0x00039C34
	private void UpdateDisplay(BalanceChangeData balanceChangeData)
	{
		this.AddEntryToPlayerHistory(balanceChangeData);
		this.PlayFeedbacks(balanceChangeData);
		this.LerpBalance();
	}

	// Token: 0x06000E6B RID: 3691 RVA: 0x0003BA4C File Offset: 0x00039C4C
	private void LerpBalance()
	{
		Tween balanceTween = this._balanceTween;
		if (balanceTween != null)
		{
			balanceTween.Kill(false);
		}
		long balance = NetworkSingleton<MoneyManager>.Instance.balance;
		this._balanceTween = DOTween.To(() => this._displayedBalance, delegate(double x)
		{
			this._displayedBalance = x;
			if (this.balanceLabel != null)
			{
				this.balanceLabel.text = MoneyFormatter.FormatWithDollar((long)x);
			}
		}, (double)balance, this.moneyLerpDuration).SetTarget(this).OnComplete(delegate
		{
			this.PlayGoalReachedScale((this.balanceLabel != null) ? this.balanceLabel.transform : null);
		});
	}

	// Token: 0x06000E6C RID: 3692 RVA: 0x0003BAB8 File Offset: 0x00039CB8
	private void OnQuotaChanged(long _, long newQuota)
	{
		Tween quotaTween = this._quotaTween;
		if (quotaTween != null)
		{
			quotaTween.Kill(false);
		}
		this._quotaTween = DOTween.To(() => this._displayedQuota, delegate(double x)
		{
			this._displayedQuota = x;
			if (this.quotaLabel != null)
			{
				this.quotaLabel.text = "/ " + MoneyFormatter.FormatWithDollar((long)x);
			}
		}, (double)newQuota, this.moneyLerpDuration).SetTarget(this).OnComplete(delegate
		{
			this.PlayGoalReachedScale((this.quotaLabel != null) ? this.quotaLabel.transform : null);
		});
	}

	// Token: 0x06000E6D RID: 3693 RVA: 0x0003BB1C File Offset: 0x00039D1C
	private void UpdateTicketDisplay(long change)
	{
		Tween ticketTween = this._ticketTween;
		if (ticketTween != null)
		{
			ticketTween.Kill(false);
		}
		long ticketBalance = NetworkSingleton<MoneyManager>.Instance.ticketBalance;
		this._ticketTween = DOTween.To(() => this._displayedTicketBalance, delegate(double x)
		{
			this._displayedTicketBalance = x;
			this.ticketBalanceLabel.text = ((long)x).ToString("N0");
		}, (double)ticketBalance, this.moneyLerpDuration).SetTarget(this).OnComplete(delegate
		{
			this.PlayGoalReachedScale(this.ticketBalanceLabel.transform);
		});
	}

	// Token: 0x06000E6E RID: 3694 RVA: 0x0003BB88 File Offset: 0x00039D88
	private void UpdatePlayerHistoryLabel()
	{
		HashSet<string> currentPlayerNames = this.GetCurrentPlayerNames();
		HashSet<string> hashSet = new HashSet<string>(currentPlayerNames);
		foreach (string text in this.PlayerProfitHistory.Keys)
		{
			if (!string.IsNullOrWhiteSpace(text) && currentPlayerNames.Contains(text))
			{
				hashSet.Add(text);
			}
		}
		List<KeyValuePair<string, long>> list = (from e in hashSet.Select(delegate(string name)
		{
			long value2 = 0L;
			this.PlayerProfitHistory.TryGetValue(name, out value2);
			return new KeyValuePair<string, long>(name, value2);
		})
		orderby e.Value descending
		select e).ToList<KeyValuePair<string, long>>();
		foreach (KeyValuePair<string, long> keyValuePair in list)
		{
			string key = keyValuePair.Key;
			long value = keyValuePair.Value;
			MoneyDisplayAndFeedbacks.HistoryEntry entry;
			if (!this._historyEntries.TryGetValue(key, out entry))
			{
				Transform transform = Object.Instantiate<Transform>(this.playerHistoryEntryPrefab, this.playerHistoryContainer);
				TextMeshProUGUI component = transform.GetChild(0).GetComponent<TextMeshProUGUI>();
				TextMeshProUGUI component2 = transform.GetChild(2).GetComponent<TextMeshProUGUI>();
				entry = new MoneyDisplayAndFeedbacks.HistoryEntry
				{
					root = transform,
					profitText = component,
					nameText = component2,
					displayedValue = (double)value
				};
				this._historyEntries[key] = entry;
				MoneyDisplayAndFeedbacks.SetProfitEntryText(entry);
			}
			else
			{
				Tween tween = entry.tween;
				if (tween != null)
				{
					tween.Kill(false);
				}
				MoneyDisplayAndFeedbacks.HistoryEntry entryCopy = entry;
				entry.tween = DOTween.To(() => entry.displayedValue, delegate(double x)
				{
					entry.displayedValue = x;
					MoneyDisplayAndFeedbacks.SetProfitEntryText(entry);
				}, (double)value, this.moneyLerpDuration).SetTarget(this).OnComplete(delegate
				{
					this.PlayGoalReachedScale(entryCopy.profitText.transform);
				});
			}
			entry.nameText.text = key;
		}
		HashSet<string> hashSet2 = new HashSet<string>(from e in list
		select e.Key);
		foreach (string text2 in this._historyEntries.Keys.ToList<string>())
		{
			if (!hashSet2.Contains(text2))
			{
				Tween tween2 = this._historyEntries[text2].tween;
				if (tween2 != null)
				{
					tween2.Kill(false);
				}
				Object.Destroy(this._historyEntries[text2].root.gameObject);
				this._historyEntries.Remove(text2);
			}
		}
		for (int i = 0; i < list.Count; i++)
		{
			this._historyEntries[list[i].Key].root.SetSiblingIndex(i);
		}
	}

	// Token: 0x06000E6F RID: 3695 RVA: 0x0003BF10 File Offset: 0x0003A110
	private HashSet<string> GetCurrentPlayerNames()
	{
		if (this._lobbySettings == null)
		{
			this._lobbySettings = Resources.Load<LobbySettings>("LobbySettings");
		}
		if (this._lobbySettings == null || this._lobbySettings.players == null)
		{
			return new HashSet<string>();
		}
		return (from p in this._lobbySettings.players
		where !string.IsNullOrWhiteSpace(p.playerName)
		select p.playerName).ToHashSet<string>();
	}

	// Token: 0x06000E70 RID: 3696 RVA: 0x0003BFB4 File Offset: 0x0003A1B4
	private void AddEntryToPlayerHistory(BalanceChangeData balanceChangeData)
	{
		if (!base.isServer)
		{
			return;
		}
		if (!balanceChangeData.changer)
		{
			return;
		}
		if (string.IsNullOrWhiteSpace(balanceChangeData.changer.playerName))
		{
			return;
		}
		if (balanceChangeData.changeType == ChangeType.Misc)
		{
			return;
		}
		if (this.PlayerProfitHistory.ContainsKey(balanceChangeData.changer.playerName))
		{
			SyncDictionary<string, long> playerProfitHistory = this.PlayerProfitHistory;
			string playerName = balanceChangeData.changer.playerName;
			playerProfitHistory[playerName] += balanceChangeData.changeAmount;
			return;
		}
		this.PlayerProfitHistory.TryAdd(balanceChangeData.changer.playerName, balanceChangeData.changeAmount);
	}

	// Token: 0x06000E71 RID: 3697 RVA: 0x0003C054 File Offset: 0x0003A254
	private void PlayFeedbacks(BalanceChangeData balanceChangeData)
	{
		long num = (NetworkSingleton<GameManager>.Instance != null) ? NetworkSingleton<GameManager>.Instance.currentQuota : 0L;
		if (num <= 0L)
		{
			return;
		}
		long num2 = (long)((float)num * this.minQuotaFractionToShow);
		if (Math.Abs(balanceChangeData.changeAmount) < num2)
		{
			return;
		}
		this.SpawnMoneyChangeText(balanceChangeData);
	}

	// Token: 0x06000E72 RID: 3698 RVA: 0x0003C0A4 File Offset: 0x0003A2A4
	private void SpawnMoneyChangeText(BalanceChangeData balanceChangeData)
	{
		if (!balanceChangeData.changer)
		{
			return;
		}
		if (string.IsNullOrWhiteSpace(balanceChangeData.changer.playerName))
		{
			return;
		}
		string key = string.IsNullOrEmpty(balanceChangeData.changer.playerName) ? string.Empty : balanceChangeData.changer.playerName;
		MoneyDisplayAndFeedbacks.ActiveMoneyChange active;
		if (!this._activeMoneyChanges.TryGetValue(key, out active) || active.instance == null)
		{
			MoneyChangeText instance = Object.Instantiate<MoneyChangeText>(this.moneyChangePrefab, this.feedbackParent);
			active = new MoneyDisplayAndFeedbacks.ActiveMoneyChange
			{
				instance = instance,
				displayedValue = 0.0,
				targetValue = 0.0
			};
			this._activeMoneyChanges[key] = active;
		}
		else if (active.hideTween != null && active.hideTween.IsActive())
		{
			active.hideTween.Kill(false);
			active.hideTween = null;
		}
		active.instance.playerNameText.text = balanceChangeData.changer.playerName;
		active.instance.playerNameText.color = new Color(1f, 1f, 1f, 1f);
		if (!string.IsNullOrEmpty(balanceChangeData.changer.playerName) && active.instance.playerColorImage != null)
		{
			LobbySettings lobbySettings = Resources.Load<LobbySettings>("LobbySettings");
			if (lobbySettings != null)
			{
				PlayerInfo playerInfo = lobbySettings.players.Find((PlayerInfo p) => p.playerName == balanceChangeData.changer.playerName);
				if (playerInfo != null)
				{
					active.instance.playerColorImage.color = playerInfo.playerColor.PastelizeColor(0.6f, 0.3f);
				}
			}
		}
		active.targetValue += (double)balanceChangeData.changeAmount;
		double newTotal = active.targetValue;
		if (active.valueTween != null && active.valueTween.IsActive())
		{
			active.valueTween.Kill(false);
		}
		SFXManager.SFXOneShot((newTotal > 0.0) ? this.sfxMoneyCountUp : this.sfxMoneyCountDown, default(Vector3));
		TweenCallback <>9__4;
		active.valueTween = DOTween.To(() => active.displayedValue, delegate(double x)
		{
			active.displayedValue = x;
			long num = (long)x;
			active.instance.changeAmountText.text = ((num >= 0L) ? ("+" + MoneyFormatter.FormatWithDollar(num)) : MoneyFormatter.FormatWithDollar(num));
			active.instance.changeAmountText.color = ((num > 0L) ? Color.green : ((num < 0L) ? Color.red : Color.white));
			active.instance.GetComponent<Image>().color = active.instance.changeAmountText.color;
		}, newTotal, this.moneyLerpDuration).SetTarget(this).OnComplete(delegate
		{
			SFXManager.SFXOneShot((newTotal > 0.0) ? this.sfxMoneyUp : this.sfxMoneyDown, default(Vector3));
			MoneyDisplayAndFeedbacks.ActiveMoneyChange active = active;
			float delay = this.disappearTime;
			TweenCallback callback;
			if ((callback = <>9__4) == null)
			{
				callback = (<>9__4 = delegate()
				{
					this._activeMoneyChanges.Remove(key);
					if (active.instance != null)
					{
						Object.Destroy(active.instance.gameObject);
					}
				});
			}
			active.hideTween = DOVirtual.DelayedCall(delay, callback, true).SetTarget(this);
		});
		this.onBalanceChangedFeedbacks.PlayFeedbacks();
	}

	// Token: 0x06000E73 RID: 3699 RVA: 0x0003C3A5 File Offset: 0x0003A5A5
	public Dictionary<string, long> GetProfitHistorySnapshot()
	{
		return new Dictionary<string, long>(this.PlayerProfitHistory);
	}

	// Token: 0x06000E74 RID: 3700 RVA: 0x0003C3B4 File Offset: 0x0003A5B4
	[Server]
	public void SetProfitHistory(Dictionary<string, long> history)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void MoneyDisplayAndFeedbacks::SetProfitHistory(System.Collections.Generic.Dictionary`2<System.String,System.Int64>)' called when server was not active");
			return;
		}
		this.RpcBeginProfitHistoryBulkSync();
		this.PlayerProfitHistory.Clear();
		if (history == null)
		{
			this.RpcEndProfitHistoryBulkSync();
			return;
		}
		foreach (KeyValuePair<string, long> keyValuePair in history)
		{
			this.PlayerProfitHistory[keyValuePair.Key] = keyValuePair.Value;
		}
		this.RpcEndProfitHistoryBulkSync();
	}

	// Token: 0x06000E75 RID: 3701 RVA: 0x0003C44C File Offset: 0x0003A64C
	[ClientRpc]
	private void RpcBeginProfitHistoryBulkSync()
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		this.SendRPCInternal("System.Void MoneyDisplayAndFeedbacks::RpcBeginProfitHistoryBulkSync()", -1089434775, writer, 0, true);
		NetworkWriterPool.Return(writer);
	}

	// Token: 0x06000E76 RID: 3702 RVA: 0x0003C47C File Offset: 0x0003A67C
	[ClientRpc]
	private void RpcEndProfitHistoryBulkSync()
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		this.SendRPCInternal("System.Void MoneyDisplayAndFeedbacks::RpcEndProfitHistoryBulkSync()", 1866625689, writer, 0, true);
		NetworkWriterPool.Return(writer);
	}

	// Token: 0x06000E77 RID: 3703 RVA: 0x0003C4AC File Offset: 0x0003A6AC
	[Server]
	public void ServerResetProfitHistory()
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void MoneyDisplayAndFeedbacks::ServerResetProfitHistory()' called when server was not active");
			return;
		}
		this.PlayerProfitHistory.Clear();
		this.RpcRefreshProfitHistoryLabel();
	}

	// Token: 0x06000E78 RID: 3704 RVA: 0x0003C4D4 File Offset: 0x0003A6D4
	[ClientRpc]
	private void RpcRefreshProfitHistoryLabel()
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		this.SendRPCInternal("System.Void MoneyDisplayAndFeedbacks::RpcRefreshProfitHistoryLabel()", -2118369112, writer, 0, true);
		NetworkWriterPool.Return(writer);
	}

	// Token: 0x06000E79 RID: 3705 RVA: 0x0003C504 File Offset: 0x0003A704
	public MoneyDisplayAndFeedbacks()
	{
		base.InitSyncObject(this.PlayerProfitHistory);
	}

	// Token: 0x06000E84 RID: 3716 RVA: 0x00002321 File Offset: 0x00000521
	public override bool Weaved()
	{
		return true;
	}

	// Token: 0x06000E85 RID: 3717 RVA: 0x0003C6A2 File Offset: 0x0003A8A2
	protected void UserCode_RpcBeginProfitHistoryBulkSync()
	{
		this._suppressProfitHistoryUi = true;
	}

	// Token: 0x06000E86 RID: 3718 RVA: 0x0003C6AB File Offset: 0x0003A8AB
	protected static void InvokeUserCode_RpcBeginProfitHistoryBulkSync(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcBeginProfitHistoryBulkSync called on server.");
			return;
		}
		((MoneyDisplayAndFeedbacks)obj).UserCode_RpcBeginProfitHistoryBulkSync();
	}

	// Token: 0x06000E87 RID: 3719 RVA: 0x0003C6CE File Offset: 0x0003A8CE
	protected void UserCode_RpcEndProfitHistoryBulkSync()
	{
		this._suppressProfitHistoryUi = false;
		this.UpdatePlayerHistoryLabel();
	}

	// Token: 0x06000E88 RID: 3720 RVA: 0x0003C6DD File Offset: 0x0003A8DD
	protected static void InvokeUserCode_RpcEndProfitHistoryBulkSync(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcEndProfitHistoryBulkSync called on server.");
			return;
		}
		((MoneyDisplayAndFeedbacks)obj).UserCode_RpcEndProfitHistoryBulkSync();
	}

	// Token: 0x06000E89 RID: 3721 RVA: 0x0003BA2C File Offset: 0x00039C2C
	protected void UserCode_RpcRefreshProfitHistoryLabel()
	{
		this.UpdatePlayerHistoryLabel();
	}

	// Token: 0x06000E8A RID: 3722 RVA: 0x0003C700 File Offset: 0x0003A900
	protected static void InvokeUserCode_RpcRefreshProfitHistoryLabel(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcRefreshProfitHistoryLabel called on server.");
			return;
		}
		((MoneyDisplayAndFeedbacks)obj).UserCode_RpcRefreshProfitHistoryLabel();
	}

	// Token: 0x06000E8B RID: 3723 RVA: 0x0003C724 File Offset: 0x0003A924
	static MoneyDisplayAndFeedbacks()
	{
		RemoteProcedureCalls.RegisterRpc(typeof(MoneyDisplayAndFeedbacks), "System.Void MoneyDisplayAndFeedbacks::RpcBeginProfitHistoryBulkSync()", new RemoteCallDelegate(MoneyDisplayAndFeedbacks.InvokeUserCode_RpcBeginProfitHistoryBulkSync));
		RemoteProcedureCalls.RegisterRpc(typeof(MoneyDisplayAndFeedbacks), "System.Void MoneyDisplayAndFeedbacks::RpcEndProfitHistoryBulkSync()", new RemoteCallDelegate(MoneyDisplayAndFeedbacks.InvokeUserCode_RpcEndProfitHistoryBulkSync));
		RemoteProcedureCalls.RegisterRpc(typeof(MoneyDisplayAndFeedbacks), "System.Void MoneyDisplayAndFeedbacks::RpcRefreshProfitHistoryLabel()", new RemoteCallDelegate(MoneyDisplayAndFeedbacks.InvokeUserCode_RpcRefreshProfitHistoryLabel));
	}

	// Token: 0x04000933 RID: 2355
	[Header("References")]
	[SerializeField]
	private TextMeshProUGUI ticketBalanceLabel;

	// Token: 0x04000934 RID: 2356
	[SerializeField]
	private TextMeshProUGUI balanceLabel;

	// Token: 0x04000935 RID: 2357
	[SerializeField]
	private TextMeshProUGUI quotaLabel;

	// Token: 0x04000936 RID: 2358
	[Header("Feedbacks (Visual)")]
	[Tooltip("Prefabs are instantiated under this transform for the display duration, then destroyed.")]
	[SerializeField]
	private Transform feedbackParent;

	// Token: 0x04000937 RID: 2359
	[SerializeField]
	private MMF_Player onBalanceChangedFeedbacks;

	// Token: 0x04000938 RID: 2360
	[SerializeField]
	private MoneyChangeText moneyChangePrefab;

	// Token: 0x04000939 RID: 2361
	[SerializeField]
	private float disappearTime = 1f;

	// Token: 0x0400093A RID: 2362
	[Tooltip("Only show money change popup when |change| is at least this fraction of current quota (e.g. 0.05 = 5%). Big wins/losses only.")]
	[SerializeField]
	[Range(0f, 1f)]
	private float minQuotaFractionToShow = 0.05f;

	// Token: 0x0400093B RID: 2363
	[Header("Feedbacks (Audio)")]
	[SerializeField]
	private EventReference sfxMoneyUp;

	// Token: 0x0400093C RID: 2364
	[SerializeField]
	private EventReference sfxMoneyDown;

	// Token: 0x0400093D RID: 2365
	[SerializeField]
	private EventReference sfxMoneyCountUp;

	// Token: 0x0400093E RID: 2366
	[SerializeField]
	private EventReference sfxMoneyCountDown;

	// Token: 0x0400093F RID: 2367
	[Header("Profit History")]
	[SerializeField]
	private Transform playerHistoryContainer;

	// Token: 0x04000940 RID: 2368
	[SerializeField]
	private Transform playerHistoryEntryPrefab;

	// Token: 0x04000941 RID: 2369
	private readonly Dictionary<string, MoneyDisplayAndFeedbacks.HistoryEntry> _historyEntries = new Dictionary<string, MoneyDisplayAndFeedbacks.HistoryEntry>();

	// Token: 0x04000942 RID: 2370
	private LobbySettings _lobbySettings;

	// Token: 0x04000943 RID: 2371
	[Header("Money lerp")]
	[SerializeField]
	private float moneyLerpDuration = 0.5f;

	// Token: 0x04000944 RID: 2372
	[SerializeField]
	private float goalReachedPunchScale = 0.15f;

	// Token: 0x04000945 RID: 2373
	[SerializeField]
	private float goalReachedPunchDuration = 0.25f;

	// Token: 0x04000946 RID: 2374
	private double _displayedTicketBalance;

	// Token: 0x04000947 RID: 2375
	private Tween _ticketTween;

	// Token: 0x04000948 RID: 2376
	private double _displayedBalance;

	// Token: 0x04000949 RID: 2377
	private double _displayedQuota;

	// Token: 0x0400094A RID: 2378
	private Tween _balanceTween;

	// Token: 0x0400094B RID: 2379
	private Tween _quotaTween;

	// Token: 0x0400094C RID: 2380
	public readonly SyncDictionary<string, long> PlayerProfitHistory = new SyncDictionary<string, long>();

	// Token: 0x0400094D RID: 2381
	private bool _suppressProfitHistoryUi;

	// Token: 0x0400094E RID: 2382
	private readonly Dictionary<string, MoneyDisplayAndFeedbacks.ActiveMoneyChange> _activeMoneyChanges = new Dictionary<string, MoneyDisplayAndFeedbacks.ActiveMoneyChange>();

	// Token: 0x02000181 RID: 385
	private class HistoryEntry
	{
		// Token: 0x0400094F RID: 2383
		public Transform root;

		// Token: 0x04000950 RID: 2384
		public TextMeshProUGUI profitText;

		// Token: 0x04000951 RID: 2385
		public TextMeshProUGUI nameText;

		// Token: 0x04000952 RID: 2386
		public double displayedValue;

		// Token: 0x04000953 RID: 2387
		public Tween tween;
	}

	// Token: 0x02000182 RID: 386
	private class ActiveMoneyChange
	{
		// Token: 0x04000954 RID: 2388
		public MoneyChangeText instance;

		// Token: 0x04000955 RID: 2389
		public double displayedValue;

		// Token: 0x04000956 RID: 2390
		public double targetValue;

		// Token: 0x04000957 RID: 2391
		public Tween valueTween;

		// Token: 0x04000958 RID: 2392
		public Tween hideTween;
	}
}
