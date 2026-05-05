using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Extensions;
using Mirror;
using Mirror.RemoteCalls;
using Steamworks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Token: 0x0200009E RID: 158
public class NewConsole : NetworkSingleton<NewConsole>, IUIManager
{
	// Token: 0x17000092 RID: 146
	// (get) Token: 0x060005BA RID: 1466 RVA: 0x00019224 File Offset: 0x00017424
	public bool IsActive
	{
		get
		{
			return this.isVisible;
		}
	}

	// Token: 0x17000093 RID: 147
	// (get) Token: 0x060005BB RID: 1467 RVA: 0x0001922C File Offset: 0x0001742C
	public int Priority
	{
		get
		{
			return 10;
		}
	}

	// Token: 0x060005BC RID: 1468 RVA: 0x00019230 File Offset: 0x00017430
	protected override void OnAwake()
	{
		base.OnAwake();
		this._gs = Resources.Load<GameSettings>("GameSettings");
	}

	// Token: 0x060005BD RID: 1469 RVA: 0x00019248 File Offset: 0x00017448
	private void OnEnable()
	{
		InputEvents.OnConsoleEvent = (Action)Delegate.Combine(InputEvents.OnConsoleEvent, new Action(this.ToggleConsole));
		if (NetworkSingleton<MoneyManager>.Instance != null)
		{
			InputEvents.OnF1Event = (Action)Delegate.Combine(InputEvents.OnF1Event, new Action(this.HandleF1MoneyChange));
			InputEvents.OnF2Event = (Action)Delegate.Combine(InputEvents.OnF2Event, new Action(this.HandleF2MoneyChange));
		}
		if (UIManager.Instance != null)
		{
			UIManager.Instance.RegisterUI(this);
		}
		InputEvents.OnF3Event = (Action)Delegate.Combine(InputEvents.OnF3Event, new Action(this.ProgressGame));
	}

	// Token: 0x060005BE RID: 1470 RVA: 0x000192FC File Offset: 0x000174FC
	private void OnDisable()
	{
		InputEvents.OnConsoleEvent = (Action)Delegate.Remove(InputEvents.OnConsoleEvent, new Action(this.ToggleConsole));
		if (NetworkSingleton<MoneyManager>.Instance != null)
		{
			InputEvents.OnF1Event = (Action)Delegate.Remove(InputEvents.OnF1Event, new Action(this.HandleF1MoneyChange));
			InputEvents.OnF2Event = (Action)Delegate.Remove(InputEvents.OnF2Event, new Action(this.HandleF2MoneyChange));
		}
		if (UIManager.Instance != null)
		{
			UIManager.Instance.UnregisterUI(this);
		}
		InputEvents.OnF3Event = (Action)Delegate.Remove(InputEvents.OnF3Event, new Action(this.ProgressGame));
	}

	// Token: 0x060005BF RID: 1471 RVA: 0x000193AE File Offset: 0x000175AE
	private void HandleF1MoneyChange()
	{
		if (NetworkSingleton<MoneyManager>.Instance != null)
		{
			NetworkSingleton<MoneyManager>.Instance.CmdTryChangeBalance(100L, null, ChangeType.Misc);
		}
	}

	// Token: 0x060005C0 RID: 1472 RVA: 0x000193CC File Offset: 0x000175CC
	private void HandleF2MoneyChange()
	{
		if (NetworkSingleton<MoneyManager>.Instance != null)
		{
			NetworkSingleton<MoneyManager>.Instance.CmdTryChangeBalance(-100L, null, ChangeType.Misc);
		}
	}

	// Token: 0x060005C1 RID: 1473 RVA: 0x000193EA File Offset: 0x000175EA
	private void Start()
	{
		if (this.consolePanel != null)
		{
			this.consolePanel.SetActive(false);
		}
		this.GetAvailablePrefabs();
	}

	// Token: 0x060005C2 RID: 1474 RVA: 0x0001940C File Offset: 0x0001760C
	private void GetAvailablePrefabs()
	{
		this.availablePrefabs.Clear();
		if (this.spawnableSettings == null)
		{
			this.spawnableSettings = Resources.Load<SpawnableSettings>("SpawnableSettings");
		}
		if (this.spawnableSettings != null && this.spawnableSettings.isEnabled)
		{
			foreach (SpawnableSO spawnableSO in this.spawnableSettings.spawnables)
			{
				if (spawnableSO != null)
				{
					this.availablePrefabs.Add(spawnableSO);
				}
			}
			Debug.Log(string.Format("Found {0} spawnable prefabs from SpawnableSettings", this.availablePrefabs.Count));
			return;
		}
		Debug.LogWarning("SpawnableSettings not found or disabled. Please create a SpawnableSettings asset in Resources folder.");
	}

	// Token: 0x060005C3 RID: 1475 RVA: 0x000194E0 File Offset: 0x000176E0
	public void CloseUI()
	{
		if (this.isVisible)
		{
			this.isVisible = false;
			UICursorSimple instance = UICursorSimple.Instance;
			if (instance != null)
			{
				instance.HideCursor();
			}
			if (this.consolePanel != null)
			{
				this.consolePanel.SetActive(false);
			}
			if (UIManager.Instance != null)
			{
				UIManager.Instance.ClearActiveUI(this);
			}
		}
	}

	// Token: 0x060005C4 RID: 1476 RVA: 0x00019540 File Offset: 0x00017740
	public void OpenUI()
	{
		if (!this.isVisible)
		{
			this.isVisible = true;
			NetworkClient.localPlayer.GetComponent<PlayerController>().head.isLocked = true;
			UICursorSimple instance = UICursorSimple.Instance;
			if (instance != null)
			{
				instance.ShowCursor();
			}
			if (this.consolePanel != null)
			{
				this.consolePanel.SetActive(true);
			}
			this.ShowMainMenu();
			this.ShowLastUsedTab();
			if (UIManager.Instance != null)
			{
				UIManager.Instance.SetActiveUI(this);
			}
		}
	}

	// Token: 0x060005C5 RID: 1477 RVA: 0x000195C0 File Offset: 0x000177C0
	private void ToggleConsole()
	{
		if (this.IsActive)
		{
			this.CloseUI();
			Cursor.lockState = CursorLockMode.Locked;
			UICursorSimple instance = UICursorSimple.Instance;
			if (instance != null)
			{
				instance.HideCursor();
			}
			NetworkIdentity localPlayer = NetworkClient.localPlayer;
			if (localPlayer != null)
			{
				PlayerController component = localPlayer.GetComponent<PlayerController>();
				if (component != null && component.head != null)
				{
					component.head.isLocked = false;
					return;
				}
			}
		}
		else
		{
			this.OpenUI();
		}
	}

	// Token: 0x060005C6 RID: 1478 RVA: 0x00019634 File Offset: 0x00017834
	private void ShowMainMenu()
	{
		this.ClearButtons();
		if (!this._sectionsInitialized && this.sectionContainer != null && this.sectionContainer.childCount == 0)
		{
			this.CreateSectionButton("Spawn", delegate
			{
				this.ShowSpawnMenu();
			});
			this.CreateSectionButton("Teleport", delegate
			{
				this.ShowTeleportMenu();
			});
			this.CreateSectionButton(this.GetNoclipButtonText(), delegate
			{
				this.ToggleNoclip();
			});
			this.CreateSectionButton("Money", delegate
			{
				this.ShowMoneyMenu();
			});
			this.CreateSectionButton("Simulate", delegate
			{
				this.ShowSimulateMenu();
			});
			this.CreateSectionButton("Cosmetics", delegate
			{
				this.ShowCosmeticsMenu();
			});
			this.CreateSectionButton(this.GetCameramanButtonText(), delegate
			{
				this.ShowCameramanMenu();
			});
			this.CreateSectionButton("Challenges", delegate
			{
				this.ShowChallengesMenu();
			});
			this._sectionsInitialized = true;
		}
	}

	// Token: 0x060005C7 RID: 1479 RVA: 0x00019734 File Offset: 0x00017934
	private void ShowLastUsedTab()
	{
		switch (this.lastUsedState)
		{
		case NewConsole.ConsoleState.Spawn:
			this.ShowSpawnMenu();
			return;
		case NewConsole.ConsoleState.Count:
			this.ShowCountMenu();
			return;
		case NewConsole.ConsoleState.Teleport:
			this.ShowTeleportMenu();
			return;
		case NewConsole.ConsoleState.Money:
			this.ShowMoneyMenu();
			return;
		case NewConsole.ConsoleState.Simulate:
			this.ShowSimulateMenu();
			return;
		case NewConsole.ConsoleState.Cosmetics:
			this.ShowCosmeticsMenu();
			return;
		case NewConsole.ConsoleState.Cameraman:
			this.ShowCameramanMenu();
			return;
		case NewConsole.ConsoleState.Challenges:
			this.ShowChallengesMenu();
			return;
		}
		this.ShowSpawnMenu();
	}

	// Token: 0x060005C8 RID: 1480 RVA: 0x000197BC File Offset: 0x000179BC
	private void ShowMoneyMenu()
	{
		this.lastUsedState = NewConsole.ConsoleState.Money;
		this.ClearButtons();
		this.CreateButton("Back", delegate()
		{
			this.ShowMainMenu();
		});
		this.CreateButton("Reset To Default", delegate()
		{
			NetworkSingleton<MoneyManager>.Instance.CmdResetBalancesToDefault();
		});
		this.CreateButton("+ 1 Ticket", delegate()
		{
			NetworkSingleton<MoneyManager>.Instance.CmdTryChangeTicketBalance(1L);
		});
		this.CreateButton("+ 5 Ticket", delegate()
		{
			NetworkSingleton<MoneyManager>.Instance.CmdTryChangeTicketBalance(5L);
		});
		this.CreateButton("+ 10 Ticket", delegate()
		{
			NetworkSingleton<MoneyManager>.Instance.CmdTryChangeTicketBalance(10L);
		});
		this.CreateButton("- 1 Ticket", delegate()
		{
			NetworkSingleton<MoneyManager>.Instance.CmdTryChangeTicketBalance(-1L);
		});
		this.CreateButton("- 5 Ticket", delegate()
		{
			NetworkSingleton<MoneyManager>.Instance.CmdTryChangeTicketBalance(-5L);
		});
		this.CreateButton("- 10 Ticket", delegate()
		{
			NetworkSingleton<MoneyManager>.Instance.CmdTryChangeTicketBalance(-10L);
		});
		this.CreateButton("+ $10", delegate()
		{
			NetworkSingleton<MoneyManager>.Instance.CmdTryChangeBalance(10L, null, ChangeType.Misc);
		});
		this.CreateButton("+ $100", delegate()
		{
			NetworkSingleton<MoneyManager>.Instance.CmdTryChangeBalance(100L, null, ChangeType.Misc);
		});
		this.CreateButton("+ $1K", delegate()
		{
			NetworkSingleton<MoneyManager>.Instance.CmdTryChangeBalance(1000L, null, ChangeType.Misc);
		});
		this.CreateButton("+ $10K", delegate()
		{
			NetworkSingleton<MoneyManager>.Instance.CmdTryChangeBalance(10000L, null, ChangeType.Misc);
		});
		this.CreateButton("+ $100K", delegate()
		{
			NetworkSingleton<MoneyManager>.Instance.CmdTryChangeBalance(100000L, null, ChangeType.Misc);
		});
		this.CreateButton("+ $1M", delegate()
		{
			NetworkSingleton<MoneyManager>.Instance.CmdTryChangeBalance(1000000L, null, ChangeType.Misc);
		});
		this.CreateButton("+ $10M", delegate()
		{
			NetworkSingleton<MoneyManager>.Instance.CmdTryChangeBalance(10000000L, null, ChangeType.Misc);
		});
		this.CreateButton("+ $100M", delegate()
		{
			NetworkSingleton<MoneyManager>.Instance.CmdTryChangeBalance(100000000L, null, ChangeType.Misc);
		});
		this.CreateButton("+ $1B", delegate()
		{
			NetworkSingleton<MoneyManager>.Instance.CmdTryChangeBalance(1000000000L, null, ChangeType.Misc);
		});
		this.CreateButton("+ $10B", delegate()
		{
			NetworkSingleton<MoneyManager>.Instance.CmdTryChangeBalance(10000000000L, null, ChangeType.Misc);
		});
		this.CreateButton("+ $100B", delegate()
		{
			NetworkSingleton<MoneyManager>.Instance.CmdTryChangeBalance(100000000000L, null, ChangeType.Misc);
		});
		this.CreateButton("+ $1T", delegate()
		{
			NetworkSingleton<MoneyManager>.Instance.CmdTryChangeBalance(1000000000000L, null, ChangeType.Misc);
		});
		this.CreateButton("- 10", delegate()
		{
			NetworkSingleton<MoneyManager>.Instance.CmdTryChangeBalance(-10L, null, ChangeType.Misc);
		});
		this.CreateButton("- $100", delegate()
		{
			NetworkSingleton<MoneyManager>.Instance.CmdTryChangeBalance(-100L, null, ChangeType.Misc);
		});
		this.CreateButton("- $1K", delegate()
		{
			NetworkSingleton<MoneyManager>.Instance.CmdTryChangeBalance(-1000L, null, ChangeType.Misc);
		});
		this.CreateButton("- $10K", delegate()
		{
			NetworkSingleton<MoneyManager>.Instance.CmdTryChangeBalance(-10000L, null, ChangeType.Misc);
		});
		this.CreateButton("- $100K", delegate()
		{
			NetworkSingleton<MoneyManager>.Instance.CmdTryChangeBalance(-100000L, null, ChangeType.Misc);
		});
		this.CreateButton("- $1M", delegate()
		{
			NetworkSingleton<MoneyManager>.Instance.CmdTryChangeBalance(-1000000L, null, ChangeType.Misc);
		});
		this.CreateButton("- $10M", delegate()
		{
			NetworkSingleton<MoneyManager>.Instance.CmdTryChangeBalance(-10000000L, null, ChangeType.Misc);
		});
		this.CreateButton("- $100M", delegate()
		{
			NetworkSingleton<MoneyManager>.Instance.CmdTryChangeBalance(-100000000L, null, ChangeType.Misc);
		});
		this.CreateButton("- $1B", delegate()
		{
			NetworkSingleton<MoneyManager>.Instance.CmdTryChangeBalance(-1000000000L, null, ChangeType.Misc);
		});
		this.CreateButton("- $10B", delegate()
		{
			NetworkSingleton<MoneyManager>.Instance.CmdTryChangeBalance(-10000000000L, null, ChangeType.Misc);
		});
		this.CreateButton("- $100B", delegate()
		{
			NetworkSingleton<MoneyManager>.Instance.CmdTryChangeBalance(-100000000000L, null, ChangeType.Misc);
		});
		this.CreateButton("- $1T", delegate()
		{
			NetworkSingleton<MoneyManager>.Instance.CmdTryChangeBalance(-1000000000000L, null, ChangeType.Misc);
		});
	}

	// Token: 0x060005C9 RID: 1481 RVA: 0x00019D04 File Offset: 0x00017F04
	private void ShowSimulateMenu()
	{
		this.lastUsedState = NewConsole.ConsoleState.Simulate;
		this.ClearButtons();
		this.CreateButton("Back", delegate()
		{
			this.ShowMainMenu();
		});
		this.CreateButton("Win", delegate()
		{
			this.SimulateWinLose(true);
		});
		this.CreateButton("Lose", delegate()
		{
			this.SimulateWinLose(false);
		});
		this.CreateButton("EnableFloors", delegate()
		{
			this.CmdEnableAllFloors();
		});
		this.CreateButton("Customization", delegate()
		{
			this.SimulateCustomization();
		});
		this.CreateButton("Reset Customization", delegate()
		{
			this.ResetCustomization();
		});
		this.CreateButton("Graph", delegate()
		{
			this.ToggleGraph();
		});
		this.CreateButton("Stats", delegate()
		{
			this.ShowStats();
		});
		this.CreateButton("GameOver", delegate()
		{
			this.ShowGameOver();
		});
	}

	// Token: 0x060005CA RID: 1482 RVA: 0x00019DF0 File Offset: 0x00017FF0
	private void ToggleGraph()
	{
		EndOfRoundData endOfRoundData = Object.FindFirstObjectByType<EndOfRoundData>();
		if (endOfRoundData == null || endOfRoundData.canvasGroup == null)
		{
			Debug.LogWarning("[Console] EndOfRoundData or canvasGroup not found!");
			return;
		}
		bool flag = endOfRoundData.canvasGroup.alpha > 0.5f;
		endOfRoundData.canvasGroup.alpha = (float)(flag ? 0 : 1);
		if (!flag)
		{
			ProfitLineGraph3D profitLineGraph3D = Object.FindFirstObjectByType<ProfitLineGraph3D>();
			if (profitLineGraph3D != null)
			{
				profitLineGraph3D.ResetAndAnimate();
				return;
			}
			Debug.LogWarning("[Console] ProfitLineGraph3D not found!");
		}
	}

	// Token: 0x060005CB RID: 1483 RVA: 0x00019E6E File Offset: 0x0001806E
	private void ShowStats()
	{
		this.ToggleConsole();
		NetworkSingleton<GameManager>.Instance.ShowDayStats();
	}

	// Token: 0x060005CC RID: 1484 RVA: 0x00019E80 File Offset: 0x00018080
	private void ShowGameOver()
	{
		this.ToggleConsole();
		NetworkSingleton<GameManager>.Instance.ShowGameOverStats();
	}

	// Token: 0x060005CD RID: 1485 RVA: 0x00019E94 File Offset: 0x00018094
	private void ShowCosmeticsMenu()
	{
		this.lastUsedState = NewConsole.ConsoleState.Cosmetics;
		this.ClearButtons();
		this.CreateButton("Back", delegate()
		{
			this.ShowMainMenu();
		});
		if (MonoSingleton<CosmeticsUnlockManager>.Instance == null)
		{
			this.CreateButton("ERROR: CosmeticsUnlockManager not found!", delegate()
			{
			});
			return;
		}
		CosmeticsUnlockManager instance = MonoSingleton<CosmeticsUnlockManager>.Instance;
		this.CreateButton(string.Format("Unlocked: {0}/{1}", instance.GetUnlockedCount(), instance.GetTotalCosmeticsCount()), delegate()
		{
		});
		this.CreateButton("Unlock Random Cosmetic", delegate()
		{
			this.UnlockRandomCosmetic();
		});
		this.CreateButton("Unlock All Cosmetics", delegate()
		{
			this.UnlockAllCosmetics();
		});
		this.CreateButton("Reset All Unlocks", delegate()
		{
			this.ResetAllCosmetics();
		});
		this.CreateButton("Apply Random Cosmetic", delegate()
		{
			this.ApplyRandomUnlockedCosmetic();
		});
		this.CreateButton("Reset Player Appearance", delegate()
		{
			this.ResetPlayerAppearance();
		});
		bool initialized = SteamManager.Initialized;
		string text = initialized ? "Steam: Connected" : "Steam: Offline";
		this.CreateButton(text, delegate()
		{
		});
		if (initialized)
		{
			string text2 = SteamRemoteStorage.FileExists("cosmetics_unlocks.json") ? "Cloud Save: Available" : "Cloud Save: None";
			this.CreateButton(text2, delegate()
			{
			});
		}
	}

	// Token: 0x060005CE RID: 1486 RVA: 0x0001A038 File Offset: 0x00018238
	private void ShowChallengesMenu()
	{
		this.lastUsedState = NewConsole.ConsoleState.Challenges;
		this.ClearButtons();
		this.CreateButton("Back", delegate()
		{
			this.ShowMainMenu();
		});
		this.CreateButton("Complete All Active", delegate()
		{
			this.CmdCompleteAllChallenges();
		});
		this.CreateButton("Clear All Challenges", delegate()
		{
			this.CmdClearAllChallenges();
		});
		if (Resources.Load<ChallengeSettings>("ChallengeSettings") == null)
		{
			this.CreateButton("(No challenges in settings)", delegate()
			{
			});
			return;
		}
		int num = (NetworkSingleton<ChallengeManager>.Instance != null) ? NetworkSingleton<ChallengeManager>.Instance.GetActiveChallenges().Count : 0;
		this.CreateButton(string.Format("Active: {0}", num), delegate()
		{
		});
		this.CreateButton("Floor 1", delegate()
		{
			this.ShowChallengesForFloor(0);
		});
		this.CreateButton("Floor 2", delegate()
		{
			this.ShowChallengesForFloor(1);
		});
		this.CreateButton("Floor 3", delegate()
		{
			this.ShowChallengesForFloor(2);
		});
		this.CreateButton("Floor 4", delegate()
		{
			this.ShowChallengesForFloor(3);
		});
	}

	// Token: 0x060005CF RID: 1487 RVA: 0x0001A188 File Offset: 0x00018388
	private void ShowChallengesForFloor(int floorIndex)
	{
		this.ClearButtons();
		this.CreateButton("Back", delegate()
		{
			this.ShowChallengesMenu();
		});
		ChallengeSettings challengeSettings = Resources.Load<ChallengeSettings>("ChallengeSettings");
		if (challengeSettings == null)
		{
			return;
		}
		List<Challenge> challengesByFloorIndex = challengeSettings.GetChallengesByFloorIndex(floorIndex);
		if (challengesByFloorIndex == null || challengesByFloorIndex.Count == 0)
		{
			this.CreateButton(string.Format("(No challenges for floor {0})", floorIndex + 1), delegate()
			{
			});
			return;
		}
		foreach (Challenge challenge in challengesByFloorIndex.OrderBy(delegate(Challenge x)
		{
			if (!(x != null))
			{
				return "";
			}
			return x.challengeName;
		}))
		{
			if (!(challenge == null))
			{
				int id = challenge.challengeID;
				string challengeName = challenge.challengeName;
				this.CreateButton(challengeName ?? "", delegate()
				{
					this.CmdGiveChallengeById(id);
				});
			}
		}
	}

	// Token: 0x060005D0 RID: 1488 RVA: 0x0001A2B8 File Offset: 0x000184B8
	[Command(requiresAuthority = false)]
	private void CmdGiveChallengeById(int challengeId)
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		writer.WriteVarInt(challengeId);
		base.SendCommandInternal("System.Void NewConsole::CmdGiveChallengeById(System.Int32)", 67421307, writer, 0, false);
		NetworkWriterPool.Return(writer);
	}

	// Token: 0x060005D1 RID: 1489 RVA: 0x0001A2F4 File Offset: 0x000184F4
	[Command(requiresAuthority = false)]
	private void CmdCompleteAllChallenges()
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		base.SendCommandInternal("System.Void NewConsole::CmdCompleteAllChallenges()", -1411737432, writer, 0, false);
		NetworkWriterPool.Return(writer);
	}

	// Token: 0x060005D2 RID: 1490 RVA: 0x0001A324 File Offset: 0x00018524
	[Command(requiresAuthority = false)]
	private void CmdClearAllChallenges()
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		base.SendCommandInternal("System.Void NewConsole::CmdClearAllChallenges()", 1966610104, writer, 0, false);
		NetworkWriterPool.Return(writer);
	}

	// Token: 0x060005D3 RID: 1491 RVA: 0x0001A354 File Offset: 0x00018554
	private void SimulateCustomization()
	{
		PlayerCustomization component = NetworkClient.localPlayer.GetComponent<PlayerCustomization>();
		CosmeticData[] array = Resources.LoadAll<CosmeticData>("Cosmetics");
		CosmeticData cosmeticData = array[Random.Range(0, array.Length)];
		component.CmdChangeCustomization(cosmeticData.cosmeticId, true);
	}

	// Token: 0x060005D4 RID: 1492 RVA: 0x0001A38E File Offset: 0x0001858E
	private void ResetCustomization()
	{
		NetworkClient.localPlayer.GetComponent<PlayerCustomization>().ResetCustomization();
	}

	// Token: 0x060005D5 RID: 1493 RVA: 0x0001A3A0 File Offset: 0x000185A0
	private void UnlockRandomCosmetic()
	{
		if (MonoSingleton<CosmeticsUnlockManager>.Instance == null)
		{
			Debug.LogError("[Console] CosmeticsUnlockManager not found!");
			return;
		}
		int num = MonoSingleton<CosmeticsUnlockManager>.Instance.UnlockRandomCosmetic();
		if (num != -1)
		{
			Debug.Log(string.Format("[Console] Unlocked cosmetic ID: {0}", num));
			this.ShowCosmeticsMenu();
			return;
		}
		Debug.Log("[Console] All cosmetics are already unlocked!");
	}

	// Token: 0x060005D6 RID: 1494 RVA: 0x0001A3FC File Offset: 0x000185FC
	private void UnlockAllCosmetics()
	{
		if (MonoSingleton<CosmeticsUnlockManager>.Instance == null)
		{
			Debug.LogError("[Console] CosmeticsUnlockManager not found!");
			return;
		}
		CosmeticData[] array = Resources.LoadAll<CosmeticData>("Cosmetics");
		int num = 0;
		foreach (CosmeticData cosmeticData in array)
		{
			if (MonoSingleton<CosmeticsUnlockManager>.Instance.UnlockCosmetic(cosmeticData.cosmeticId))
			{
				num++;
			}
		}
		Debug.Log(string.Format("[Console] Unlocked {0} cosmetics!", num));
		this.ShowCosmeticsMenu();
	}

	// Token: 0x060005D7 RID: 1495 RVA: 0x0001A474 File Offset: 0x00018674
	private void ResetAllCosmetics()
	{
		if (MonoSingleton<CosmeticsUnlockManager>.Instance == null)
		{
			Debug.LogError("[Console] CosmeticsUnlockManager not found!");
			return;
		}
		MonoSingleton<CosmeticsUnlockManager>.Instance.ResetAllUnlocks();
		NetworkClient.localPlayer.GetComponent<PlayerCustomization>().ResetCustomization();
		Debug.Log("[Console] All cosmetic unlocks have been reset!");
		this.ShowCosmeticsMenu();
	}

	// Token: 0x060005D8 RID: 1496 RVA: 0x0001A4C4 File Offset: 0x000186C4
	private void ApplyRandomUnlockedCosmetic()
	{
		if (MonoSingleton<CosmeticsUnlockManager>.Instance == null)
		{
			Debug.LogError("[Console] CosmeticsUnlockManager not found!");
			return;
		}
		int[] unlockedCosmetics = MonoSingleton<CosmeticsUnlockManager>.Instance.GetUnlockedCosmetics();
		if (unlockedCosmetics.Length == 0)
		{
			Debug.LogWarning("[Console] No cosmetics are unlocked yet!");
			return;
		}
		int num = unlockedCosmetics[Random.Range(0, unlockedCosmetics.Length)];
		NetworkClient.localPlayer.GetComponent<PlayerCustomization>().CmdChangeCustomization(num, true);
		Debug.Log(string.Format("[Console] Applied random unlocked cosmetic ID: {0}", num));
	}

	// Token: 0x060005D9 RID: 1497 RVA: 0x0001A535 File Offset: 0x00018735
	private void ResetPlayerAppearance()
	{
		NetworkClient.localPlayer.GetComponent<PlayerCustomization>().ResetCustomization();
		Debug.Log("[Console] Player appearance has been reset!");
	}

	// Token: 0x060005DA RID: 1498 RVA: 0x0001A550 File Offset: 0x00018750
	private void SimulateWinLose(bool isWin)
	{
		NetworkIdentity localPlayer = NetworkClient.localPlayer;
		if (localPlayer == null)
		{
			Debug.LogWarning("[Console] Cannot simulate win/loss: Local player not found");
			return;
		}
		PlayerProfile component = localPlayer.GetComponent<PlayerProfile>();
		if (component == null)
		{
			Debug.LogWarning("[Console] Cannot simulate win/loss: PlayerProfile not found on local player");
			return;
		}
		Vector3 position = localPlayer.transform.position;
		Debug.Log(string.Format("[Console] Requesting simulate {0} for player at {1}", isWin ? "WIN" : "LOSS", position));
		this.CmdSimulateWinLose(component.netId, position, isWin);
	}

	// Token: 0x060005DB RID: 1499 RVA: 0x0001A5D0 File Offset: 0x000187D0
	[Command(requiresAuthority = false)]
	private void CmdSimulateWinLose(uint playerNetId, Vector3 playerPosition, bool isWin)
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		writer.WriteVarUInt(playerNetId);
		writer.WriteVector3(playerPosition);
		writer.WriteBool(isWin);
		base.SendCommandInternal("System.Void NewConsole::CmdSimulateWinLose(System.UInt32,UnityEngine.Vector3,System.Boolean)", 1061694545, writer, 0, false);
		NetworkWriterPool.Return(writer);
	}

	// Token: 0x060005DC RID: 1500 RVA: 0x0001A620 File Offset: 0x00018820
	[Command(requiresAuthority = false)]
	private void CmdEnableAllFloors()
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		base.SendCommandInternal("System.Void NewConsole::CmdEnableAllFloors()", -145919939, writer, 0, false);
		NetworkWriterPool.Return(writer);
	}

	// Token: 0x060005DD RID: 1501 RVA: 0x0001A650 File Offset: 0x00018850
	[Server]
	private GameBase FindNearestGameServer(Vector3 position)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'GameBase NewConsole::FindNearestGameServer(UnityEngine.Vector3)' called when server was not active");
			return null;
		}
		GameBase[] array = Object.FindObjectsByType<GameBase>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
		if (array == null || array.Length == 0)
		{
			return null;
		}
		GameBase result = null;
		float num = float.MaxValue;
		foreach (GameBase gameBase in array)
		{
			if (!(gameBase == null))
			{
				float num2 = Vector3.Distance(position, gameBase.transform.position);
				if (num2 < num)
				{
					num = num2;
					result = gameBase;
				}
			}
		}
		return result;
	}

	// Token: 0x060005DE RID: 1502 RVA: 0x0001A6DC File Offset: 0x000188DC
	private void ShowSpawnMenu()
	{
		this.lastUsedState = NewConsole.ConsoleState.Spawn;
		this.ClearButtons();
		using (IEnumerator<SpawnableSO> enumerator = (from p in this.availablePrefabs
		orderby p.name
		select p).GetEnumerator())
		{
			while (enumerator.MoveNext())
			{
				SpawnableSO prefab = enumerator.Current;
				this.CreateButton(prefab.name, delegate()
				{
					this.OnSpawnableSelected(prefab.spawnableID);
				}, prefab.prefab);
			}
		}
	}

	// Token: 0x060005DF RID: 1503 RVA: 0x0001A790 File Offset: 0x00018990
	private void ShowCountMenu()
	{
		this.lastUsedState = NewConsole.ConsoleState.Count;
		this.ClearButtons();
		this.CreateButton("Back", delegate()
		{
			this.ResetSelection();
			this.ShowSpawnMenu();
		});
		int[] array = new int[]
		{
			1,
			5,
			10,
			25,
			50,
			100
		};
		for (int i = 0; i < array.Length; i++)
		{
			int count = array[i];
			this.CreateButton("x" + count.ToString(), delegate()
			{
				this.OnCountSelected(count);
			});
		}
	}

	// Token: 0x060005E0 RID: 1504 RVA: 0x0001A81E File Offset: 0x00018A1E
	private void CreateButton(string text, Action onClick)
	{
		this.CreateButton(text, onClick, null);
	}

	// Token: 0x060005E1 RID: 1505 RVA: 0x0001A82C File Offset: 0x00018A2C
	private void CreateButton(string text, Action onClick, GameObject prefab)
	{
		if (this.buttonPrefab == null || this.buttonContainer == null)
		{
			return;
		}
		GameObject gameObject = Object.Instantiate<GameObject>(this.buttonPrefab, this.buttonContainer);
		Button component = gameObject.GetComponent<Button>();
		TextMeshProUGUI componentInChildren = gameObject.GetComponentInChildren<TextMeshProUGUI>();
		if (componentInChildren != null)
		{
			componentInChildren.text = text;
		}
		if (component != null)
		{
			component.onClick.AddListener(delegate()
			{
				onClick();
			});
		}
	}

	// Token: 0x060005E2 RID: 1506 RVA: 0x0001A8B4 File Offset: 0x00018AB4
	private void CreateSectionButton(string text, Action onClick)
	{
		if (this.sectionButtonPrefab == null || this.sectionContainer == null)
		{
			return;
		}
		GameObject gameObject = Object.Instantiate<GameObject>(this.sectionButtonPrefab, this.sectionContainer);
		Button component = gameObject.GetComponent<Button>();
		TextMeshProUGUI componentInChildren = gameObject.GetComponentInChildren<TextMeshProUGUI>();
		if (componentInChildren != null)
		{
			componentInChildren.text = text;
		}
		if (component != null)
		{
			component.onClick.AddListener(delegate()
			{
				onClick();
			});
		}
	}

	// Token: 0x060005E3 RID: 1507 RVA: 0x0001A939 File Offset: 0x00018B39
	private Texture2D GetPrefabPreview(GameObject prefab)
	{
		prefab == null;
		return null;
	}

	// Token: 0x060005E4 RID: 1508 RVA: 0x0001A944 File Offset: 0x00018B44
	private void ClearButtons()
	{
		if (this.buttonContainer != null)
		{
			for (int i = this.buttonContainer.childCount - 1; i >= 0; i--)
			{
				Object.DestroyImmediate(this.buttonContainer.GetChild(i).gameObject);
			}
		}
	}

	// Token: 0x060005E5 RID: 1509 RVA: 0x0001A98D File Offset: 0x00018B8D
	private void OnSpawnableSelected(int id)
	{
		this._spawnableId = id;
		this.ShowCountMenu();
	}

	// Token: 0x060005E6 RID: 1510 RVA: 0x0001A99C File Offset: 0x00018B9C
	private void OnCountSelected(int count)
	{
		this.selectedCount = count;
		this.SpawnSelectedPrefab();
	}

	// Token: 0x060005E7 RID: 1511 RVA: 0x0001A9AC File Offset: 0x00018BAC
	private bool IsPhysicObject(int id)
	{
		SpawnableSO spawnableSoById = SpawnableSettings.GetSpawnableSoById(id);
		Rigidbody rigidbody;
		return !(spawnableSoById == null) && !(spawnableSoById.prefab == null) && (spawnableSoById.prefab.TryGetComponent<Rigidbody>(out rigidbody) && !rigidbody.isKinematic);
	}

	// Token: 0x060005E8 RID: 1512 RVA: 0x0001A9F4 File Offset: 0x00018BF4
	private void SpawnSelectedPrefab()
	{
		if (this._spawnableId < 0)
		{
			Debug.LogError(string.Format("Cannot spawn: Invalid spawnable ID {0}. Please select a spawnable first.", this._spawnableId));
			return;
		}
		if (this.selectedCount <= 0)
		{
			Debug.LogWarning(string.Format("Cannot spawn: Invalid count {0}. Setting to 1.", this.selectedCount));
			this.selectedCount = 1;
		}
		Vector3 playerPosition = this.GetPlayerPosition(!this.IsPhysicObject(this._spawnableId));
		for (int i = 0; i < this.selectedCount; i++)
		{
			this.SpawnPrefab(this._spawnableId, playerPosition, 1);
		}
	}

	// Token: 0x060005E9 RID: 1513 RVA: 0x0001AA84 File Offset: 0x00018C84
	private void ResetSelection()
	{
		this._spawnableId = -1;
		this.selectedCount = 1;
		this.selectedValue = 1;
	}

	// Token: 0x060005EA RID: 1514 RVA: 0x0001AA9C File Offset: 0x00018C9C
	private void ShowTeleportMenu()
	{
		this.lastUsedState = NewConsole.ConsoleState.Teleport;
		this.ClearButtons();
		this.CreateButton("Back", delegate()
		{
			this.ShowMainMenu();
		});
		this.CreateButton("Progress", delegate()
		{
			this.ProgressGame();
		});
		this.CreateButton("Lobby", delegate()
		{
			this.TeleportToScene(GameState.Lobby);
		});
		this.CreateButton("Game", delegate()
		{
			this.TeleportToScene(GameState.Game);
		});
		this.CreateButton("Lose", delegate()
		{
			this.TeleportToScene(GameState.Lose);
		});
		this.CreateButton("Win", delegate()
		{
			this.TeleportToScene(GameState.Win);
		});
		this.CreateButton("CutsceneCoinflipWon", delegate()
		{
			this.TeleportToCutscene(0);
		});
		this.CreateButton("CutsceneCoinflipLost", delegate()
		{
			this.TeleportToCutscene(1);
		});
		this.CreateButton("CutsceneDebtPaid", delegate()
		{
			this.TeleportToCutscene(2);
		});
		this.CreateButton("Summary", delegate()
		{
			this.TeleportToScene(GameState.Summary);
		});
		this.CreateButton("FollowUs", delegate()
		{
			this.TeleportToScene(GameState.FollowUs);
		});
		this.CreateButton("Test", delegate()
		{
			this.TeleportToScene(GameState.Test);
		});
		using (List<PlayerProfile>.Enumerator enumerator = this.GetAllPlayerProfiles().GetEnumerator())
		{
			while (enumerator.MoveNext())
			{
				PlayerProfile player = enumerator.Current;
				if (player != null && !string.IsNullOrEmpty(player.playerName))
				{
					this.CreateButton("Teleport to " + player.playerName, delegate()
					{
						this.TeleportToPlayer(player);
					});
				}
			}
		}
	}

	// Token: 0x060005EB RID: 1515 RVA: 0x0001AC64 File Offset: 0x00018E64
	private void TeleportToScene(GameState state)
	{
		this.ToggleConsole();
		this.CmdTeleportPlayers(state);
	}

	// Token: 0x060005EC RID: 1516 RVA: 0x0001AC73 File Offset: 0x00018E73
	private void TeleportToCutscene(int cutsceneIndex)
	{
		this.ToggleConsole();
		this.CmdTeleportToCutscene(cutsceneIndex);
	}

	// Token: 0x060005ED RID: 1517 RVA: 0x0001AC82 File Offset: 0x00018E82
	private void ProgressGame()
	{
		if (this.isVisible)
		{
			this.ToggleConsole();
		}
		this.CmdProgressGame();
	}

	// Token: 0x060005EE RID: 1518 RVA: 0x0001AC98 File Offset: 0x00018E98
	[Command(requiresAuthority = false)]
	private void CmdTeleportPlayers(GameState sceneState)
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		Mirror.GeneratedNetworkCode._Write_GameState(writer, sceneState);
		base.SendCommandInternal("System.Void NewConsole::CmdTeleportPlayers(GameState)", -1457229490, writer, 0, false);
		NetworkWriterPool.Return(writer);
	}

	// Token: 0x060005EF RID: 1519 RVA: 0x0001ACD4 File Offset: 0x00018ED4
	[Command(requiresAuthority = false)]
	private void CmdTeleportToCutscene(int cutsceneIndex)
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		writer.WriteVarInt(cutsceneIndex);
		base.SendCommandInternal("System.Void NewConsole::CmdTeleportToCutscene(System.Int32)", 1143405009, writer, 0, false);
		NetworkWriterPool.Return(writer);
	}

	// Token: 0x060005F0 RID: 1520 RVA: 0x0001AD10 File Offset: 0x00018F10
	[Command(requiresAuthority = false)]
	private void CmdProgressGame()
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		base.SendCommandInternal("System.Void NewConsole::CmdProgressGame()", 28004343, writer, 0, false);
		NetworkWriterPool.Return(writer);
	}

	// Token: 0x060005F1 RID: 1521 RVA: 0x0001AD40 File Offset: 0x00018F40
	private Vector3 GetNextSpawnPos()
	{
		int num = NetworkServer.connections.Count;
		if (num <= 0)
		{
			num = 1;
		}
		float num2 = 360f / (float)num;
		float f = (float)this._spawnIndex * 0.017453292f * num2;
		float x = Mathf.Sin(f) * this.spawnRadius;
		float z = Mathf.Cos(f) * this.spawnRadius;
		this._spawnIndex = (this._spawnIndex + 1) % num;
		return new Vector3(x, 0f, z) + this.spawnOffset;
	}

	// Token: 0x060005F2 RID: 1522 RVA: 0x0001ADB8 File Offset: 0x00018FB8
	private void ToggleNoclip()
	{
		NetworkIdentity localPlayer = NetworkClient.localPlayer;
		if (!(localPlayer != null))
		{
			Debug.LogWarning("Local player not found. Cannot toggle noclip mode.");
			return;
		}
		Noclip component = localPlayer.GetComponent<Noclip>();
		if (component != null)
		{
			component.ToggleNoclip();
			this.UpdateNoclipButtonText();
			return;
		}
		Debug.LogWarning("Noclip component not found on player. Please add the Noclip component to the player prefab.");
	}

	// Token: 0x060005F3 RID: 1523 RVA: 0x0001AE06 File Offset: 0x00019006
	private string GetNoclipButtonText()
	{
		if (!this.IsNoclipActive())
		{
			return "Noclip: OFF";
		}
		return "Noclip: ON";
	}

	// Token: 0x060005F4 RID: 1524 RVA: 0x0001AE1C File Offset: 0x0001901C
	private bool IsNoclipActive()
	{
		NetworkIdentity localPlayer = NetworkClient.localPlayer;
		if (localPlayer != null)
		{
			Noclip component = localPlayer.GetComponent<Noclip>();
			if (component != null)
			{
				FieldInfo field = typeof(Noclip).GetField("_isNoclipActive", BindingFlags.Instance | BindingFlags.NonPublic);
				if (field != null)
				{
					return (bool)field.GetValue(component);
				}
			}
		}
		return false;
	}

	// Token: 0x060005F5 RID: 1525 RVA: 0x0001AE78 File Offset: 0x00019078
	private void UpdateNoclipButtonText()
	{
		if (this.sectionContainer == null)
		{
			return;
		}
		for (int i = 0; i < this.sectionContainer.childCount; i++)
		{
			TextMeshProUGUI componentInChildren = this.sectionContainer.GetChild(i).gameObject.GetComponentInChildren<TextMeshProUGUI>();
			if (componentInChildren != null && (componentInChildren.text == "Noclip: ON" || componentInChildren.text == "Noclip: OFF"))
			{
				componentInChildren.text = this.GetNoclipButtonText();
				return;
			}
		}
	}

	// Token: 0x060005F6 RID: 1526 RVA: 0x0001AEFB File Offset: 0x000190FB
	private void SpawnPrefab(int id, Vector3 position, int chipValue)
	{
		if (!NetworkManager.singleton.isNetworkActive)
		{
			Debug.LogWarning("Network not active. Cannot spawn prefabs.");
			return;
		}
		this.RequestSpawnPrefab(id, position, chipValue);
	}

	// Token: 0x060005F7 RID: 1527 RVA: 0x0001AF20 File Offset: 0x00019120
	[Server]
	private void SpawnPrefabServer(GameObject prefab, Vector3 position, int chipValue)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void NewConsole::SpawnPrefabServer(UnityEngine.GameObject,UnityEngine.Vector3,System.Int32)' called when server was not active");
			return;
		}
		NetworkServer.Spawn(Object.Instantiate<GameObject>(prefab, position, Quaternion.identity), null);
		Debug.Log(string.Format("Spawned networked prefab '{0}' at {1}", prefab.name, position));
	}

	// Token: 0x060005F8 RID: 1528 RVA: 0x0001AF70 File Offset: 0x00019170
	[Command(requiresAuthority = false)]
	private void RequestSpawnPrefab(int id, Vector3 position, int chipValue)
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		writer.WriteVarInt(id);
		writer.WriteVector3(position);
		writer.WriteVarInt(chipValue);
		base.SendCommandInternal("System.Void NewConsole::RequestSpawnPrefab(System.Int32,UnityEngine.Vector3,System.Int32)", -169869193, writer, 0, false);
		NetworkWriterPool.Return(writer);
	}

	// Token: 0x060005F9 RID: 1529 RVA: 0x0001AFC0 File Offset: 0x000191C0
	private Vector3 GetPlayerPosition(bool shouldBeGrounded)
	{
		NetworkIdentity localPlayer = NetworkClient.localPlayer;
		PlayerController playerController;
		if (!localPlayer || !localPlayer.TryGetComponent<PlayerController>(out playerController))
		{
			return Vector3.zero;
		}
		if (shouldBeGrounded)
		{
			Vector3 normalized = Vector3.ProjectOnPlane(playerController.head.transform.forward, Vector3.up).normalized;
			return playerController.transform.position + normalized * 3f;
		}
		return playerController.head.transform.position + playerController.head.transform.forward * 2f;
	}

	// Token: 0x060005FA RID: 1530 RVA: 0x0001B05C File Offset: 0x0001925C
	private List<PlayerProfile> GetAllPlayerProfiles()
	{
		List<PlayerProfile> list = new List<PlayerProfile>();
		foreach (PlayerProfile playerProfile in Object.FindObjectsByType<PlayerProfile>(FindObjectsInactive.Exclude, FindObjectsSortMode.None))
		{
			if (playerProfile != null && playerProfile.hasSynced && !string.IsNullOrEmpty(playerProfile.playerName))
			{
				list.Add(playerProfile);
			}
		}
		return list;
	}

	// Token: 0x060005FB RID: 1531 RVA: 0x0001B0B0 File Offset: 0x000192B0
	private void TeleportToPlayer(PlayerProfile targetPlayer)
	{
		if (targetPlayer == null)
		{
			return;
		}
		Vector3 position = targetPlayer.transform.position + Vector3.up * 2f;
		this.CmdTeleportToPlayer(targetPlayer.netId, position, null);
	}

	// Token: 0x060005FC RID: 1532 RVA: 0x0001B0F8 File Offset: 0x000192F8
	[Command(requiresAuthority = false)]
	private void CmdTeleportToPlayer(uint targetPlayerNetId, Vector3 position, NetworkConnectionToClient conn = null)
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		writer.WriteVarUInt(targetPlayerNetId);
		writer.WriteVector3(position);
		base.SendCommandInternal("System.Void NewConsole::CmdTeleportToPlayer(System.UInt32,UnityEngine.Vector3,Mirror.NetworkConnectionToClient)", 302698187, writer, 0, false);
		NetworkWriterPool.Return(writer);
	}

	// Token: 0x060005FD RID: 1533 RVA: 0x0001B13C File Offset: 0x0001933C
	private void ShowCameramanMenu()
	{
		this.lastUsedState = NewConsole.ConsoleState.Cameraman;
		this.ClearButtons();
		this.CreateButton("Back", delegate()
		{
			this.ShowMainMenu();
		});
		this.CreateButton(this.GetCameramanToggleButtonText(), delegate()
		{
			this.ToggleCameramanMode();
		});
		this.CreateButton(this.GetVisibilityToggleButtonText(), delegate()
		{
			this.ToggleCameramanVisibility();
		});
		this.CreateButton(this.GetCanvasToggleButtonText(), delegate()
		{
			this.ToggleCameramanCanvas();
		});
	}

	// Token: 0x060005FE RID: 1534 RVA: 0x0001B1B8 File Offset: 0x000193B8
	private void ToggleCameramanMode()
	{
		NetworkIdentity localPlayer = NetworkClient.localPlayer;
		if (localPlayer != null)
		{
			CameramanMode component = localPlayer.GetComponent<CameramanMode>();
			if (component != null)
			{
				component.ToggleCameramanMode();
				this.UpdateCameramanButtonText();
				return;
			}
			Debug.LogWarning("CameramanMode component not found on player.");
		}
	}

	// Token: 0x060005FF RID: 1535 RVA: 0x0001B1FB File Offset: 0x000193FB
	private string GetCameramanButtonText()
	{
		if (!this.IsCameramanActive())
		{
			return "Cameraman: OFF";
		}
		return "Cameraman: ON";
	}

	// Token: 0x06000600 RID: 1536 RVA: 0x0001B210 File Offset: 0x00019410
	private string GetCameramanToggleButtonText()
	{
		if (!this.IsCameramanActive())
		{
			return "Enable Cameraman Mode";
		}
		return "Disable Cameraman Mode";
	}

	// Token: 0x06000601 RID: 1537 RVA: 0x0001B228 File Offset: 0x00019428
	private bool IsCameramanActive()
	{
		NetworkIdentity localPlayer = NetworkClient.localPlayer;
		if (localPlayer != null)
		{
			CameramanMode component = localPlayer.GetComponent<CameramanMode>();
			if (component != null)
			{
				return component.IsActive;
			}
		}
		return false;
	}

	// Token: 0x06000602 RID: 1538 RVA: 0x0001B25C File Offset: 0x0001945C
	private void ToggleCameramanVisibility()
	{
		NetworkIdentity localPlayer = NetworkClient.localPlayer;
		if (localPlayer != null)
		{
			CameramanMode component = localPlayer.GetComponent<CameramanMode>();
			if (component != null)
			{
				component.ToggleVisibility();
				this.UpdateCameramanButtonText();
			}
		}
	}

	// Token: 0x06000603 RID: 1539 RVA: 0x0001B294 File Offset: 0x00019494
	private string GetVisibilityToggleButtonText()
	{
		if (!this.IsCameramanVisible())
		{
			return "Show Player";
		}
		return "Hide Player";
	}

	// Token: 0x06000604 RID: 1540 RVA: 0x0001B2AC File Offset: 0x000194AC
	private bool IsCameramanVisible()
	{
		NetworkIdentity localPlayer = NetworkClient.localPlayer;
		if (localPlayer != null)
		{
			CameramanMode component = localPlayer.GetComponent<CameramanMode>();
			if (component != null)
			{
				return component.IsVisible;
			}
		}
		return true;
	}

	// Token: 0x06000605 RID: 1541 RVA: 0x0001B2E0 File Offset: 0x000194E0
	private void ToggleCameramanCanvas()
	{
		NetworkIdentity localPlayer = NetworkClient.localPlayer;
		if (localPlayer != null)
		{
			CameramanMode component = localPlayer.GetComponent<CameramanMode>();
			if (component != null)
			{
				component.ToggleCameramanCanvas();
				this.UpdateCameramanButtonText();
			}
		}
	}

	// Token: 0x06000606 RID: 1542 RVA: 0x0001B318 File Offset: 0x00019518
	private string GetCanvasToggleButtonText()
	{
		if (!this.IsCameramanCanvasActive())
		{
			return "Show Camera Canvas";
		}
		return "Hide Camera Canvas";
	}

	// Token: 0x06000607 RID: 1543 RVA: 0x0001B330 File Offset: 0x00019530
	private bool IsCameramanCanvasActive()
	{
		NetworkIdentity localPlayer = NetworkClient.localPlayer;
		if (localPlayer != null)
		{
			CameramanMode component = localPlayer.GetComponent<CameramanMode>();
			if (component != null)
			{
				return component.IsCameramanCanvasActive();
			}
		}
		return false;
	}

	// Token: 0x06000608 RID: 1544 RVA: 0x0001B364 File Offset: 0x00019564
	private void UpdateCameramanButtonText()
	{
		if (this.sectionContainer == null)
		{
			return;
		}
		for (int i = 0; i < this.sectionContainer.childCount; i++)
		{
			TextMeshProUGUI componentInChildren = this.sectionContainer.GetChild(i).gameObject.GetComponentInChildren<TextMeshProUGUI>();
			if (componentInChildren != null && (componentInChildren.text == "Cameraman: ON" || componentInChildren.text == "Cameraman: OFF"))
			{
				componentInChildren.text = this.GetCameramanButtonText();
				break;
			}
		}
		if (this.lastUsedState == NewConsole.ConsoleState.Cameraman)
		{
			this.ShowCameramanMenu();
		}
	}

	// Token: 0x0600063B RID: 1595 RVA: 0x00002321 File Offset: 0x00000521
	public override bool Weaved()
	{
		return true;
	}

	// Token: 0x0600063C RID: 1596 RVA: 0x0001B5C5 File Offset: 0x000197C5
	protected void UserCode_CmdGiveChallengeById__Int32(int challengeId)
	{
		if (NetworkSingleton<ChallengeManager>.Instance != null)
		{
			NetworkSingleton<ChallengeManager>.Instance.ServerActivateChallengeById(challengeId);
		}
	}

	// Token: 0x0600063D RID: 1597 RVA: 0x0001B5E0 File Offset: 0x000197E0
	protected static void InvokeUserCode_CmdGiveChallengeById__Int32(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdGiveChallengeById called on client.");
			return;
		}
		((NewConsole)obj).UserCode_CmdGiveChallengeById__Int32(reader.ReadVarInt());
	}

	// Token: 0x0600063E RID: 1598 RVA: 0x0001B609 File Offset: 0x00019809
	protected void UserCode_CmdCompleteAllChallenges()
	{
		ChallengeManager instance = NetworkSingleton<ChallengeManager>.Instance;
		if (instance == null)
		{
			return;
		}
		instance.ServerCompleteAllActiveChallenges();
	}

	// Token: 0x0600063F RID: 1599 RVA: 0x0001B61A File Offset: 0x0001981A
	protected static void InvokeUserCode_CmdCompleteAllChallenges(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdCompleteAllChallenges called on client.");
			return;
		}
		((NewConsole)obj).UserCode_CmdCompleteAllChallenges();
	}

	// Token: 0x06000640 RID: 1600 RVA: 0x0001B63D File Offset: 0x0001983D
	protected void UserCode_CmdClearAllChallenges()
	{
		ChallengeManager instance = NetworkSingleton<ChallengeManager>.Instance;
		if (instance == null)
		{
			return;
		}
		instance.ServerResetAllChallenges();
	}

	// Token: 0x06000641 RID: 1601 RVA: 0x0001B64E File Offset: 0x0001984E
	protected static void InvokeUserCode_CmdClearAllChallenges(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdClearAllChallenges called on client.");
			return;
		}
		((NewConsole)obj).UserCode_CmdClearAllChallenges();
	}

	// Token: 0x06000642 RID: 1602 RVA: 0x0001B674 File Offset: 0x00019874
	protected void UserCode_CmdSimulateWinLose__UInt32__Vector3__Boolean(uint playerNetId, Vector3 playerPosition, bool isWin)
	{
		NetworkIdentity networkIdentity;
		if (!NetworkServer.spawned.TryGetValue(playerNetId, out networkIdentity))
		{
			Debug.LogWarning(string.Format("[Console] Cannot simulate payout: Player with netId {0} not found", playerNetId));
			return;
		}
		PlayerProfile component = networkIdentity.GetComponent<PlayerProfile>();
		if (component == null)
		{
			Debug.LogWarning("[Console] Cannot simulate payout: PlayerProfile component not found on " + networkIdentity.name);
			return;
		}
		GameBase gameBase = this.FindNearestGameServer(playerPosition);
		if (gameBase == null)
		{
			Debug.LogWarning(string.Format("[Console] Cannot simulate payout: No game found near player at {0}", playerPosition));
			return;
		}
		Debug.Log(string.Format("[Console] Simulating {0} on game: {1} at {2}", isWin ? "WIN" : "LOSS", gameBase.GameName, gameBase.transform.position));
		gameBase.ServerSimulatePayout(component, isWin);
	}

	// Token: 0x06000643 RID: 1603 RVA: 0x0001B72F File Offset: 0x0001992F
	protected static void InvokeUserCode_CmdSimulateWinLose__UInt32__Vector3__Boolean(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdSimulateWinLose called on client.");
			return;
		}
		((NewConsole)obj).UserCode_CmdSimulateWinLose__UInt32__Vector3__Boolean(reader.ReadVarUInt(), reader.ReadVector3(), reader.ReadBool());
	}

	// Token: 0x06000644 RID: 1604 RVA: 0x0001B764 File Offset: 0x00019964
	protected void UserCode_CmdEnableAllFloors()
	{
		ElevatorManager instance = NetworkSingleton<ElevatorManager>.Instance;
		if (instance == null)
		{
			return;
		}
		instance.RpcEnableAllButtons();
	}

	// Token: 0x06000645 RID: 1605 RVA: 0x0001B775 File Offset: 0x00019975
	protected static void InvokeUserCode_CmdEnableAllFloors(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdEnableAllFloors called on client.");
			return;
		}
		((NewConsole)obj).UserCode_CmdEnableAllFloors();
	}

	// Token: 0x06000646 RID: 1606 RVA: 0x0001B798 File Offset: 0x00019998
	protected void UserCode_CmdTeleportPlayers__GameState(GameState sceneState)
	{
		NetworkSingleton<GameManager>.Instance.ServerSetScene(sceneState);
	}

	// Token: 0x06000647 RID: 1607 RVA: 0x0001B7A5 File Offset: 0x000199A5
	protected static void InvokeUserCode_CmdTeleportPlayers__GameState(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdTeleportPlayers called on client.");
			return;
		}
		((NewConsole)obj).UserCode_CmdTeleportPlayers__GameState(Mirror.GeneratedNetworkCode._Read_GameState(reader));
	}

	// Token: 0x06000648 RID: 1608 RVA: 0x0001B7CE File Offset: 0x000199CE
	protected void UserCode_CmdTeleportToCutscene__Int32(int cutsceneIndex)
	{
		NetworkSingleton<GameManager>.Instance.ServerSetCutscene(cutsceneIndex);
	}

	// Token: 0x06000649 RID: 1609 RVA: 0x0001B7DB File Offset: 0x000199DB
	protected static void InvokeUserCode_CmdTeleportToCutscene__Int32(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdTeleportToCutscene called on client.");
			return;
		}
		((NewConsole)obj).UserCode_CmdTeleportToCutscene__Int32(reader.ReadVarInt());
	}

	// Token: 0x0600064A RID: 1610 RVA: 0x0001B804 File Offset: 0x00019A04
	protected void UserCode_CmdProgressGame()
	{
		NetworkSingleton<GameManager>.Instance.ProgressGame();
	}

	// Token: 0x0600064B RID: 1611 RVA: 0x0001B810 File Offset: 0x00019A10
	protected static void InvokeUserCode_CmdProgressGame(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdProgressGame called on client.");
			return;
		}
		((NewConsole)obj).UserCode_CmdProgressGame();
	}

	// Token: 0x0600064C RID: 1612 RVA: 0x0001B834 File Offset: 0x00019A34
	protected void UserCode_RequestSpawnPrefab__Int32__Vector3__Int32(int id, Vector3 position, int chipValue)
	{
		SpawnableSO spawnableSoById = SpawnableSettings.GetSpawnableSoById(id);
		if (spawnableSoById == null || spawnableSoById.prefab == null)
		{
			Debug.LogError(string.Format("Cannot spawn prefab: Spawnable with ID {0} not found or prefab is null.", id));
			return;
		}
		this.SpawnPrefabServer(spawnableSoById.prefab, position, chipValue);
	}

	// Token: 0x0600064D RID: 1613 RVA: 0x0001B883 File Offset: 0x00019A83
	protected static void InvokeUserCode_RequestSpawnPrefab__Int32__Vector3__Int32(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command RequestSpawnPrefab called on client.");
			return;
		}
		((NewConsole)obj).UserCode_RequestSpawnPrefab__Int32__Vector3__Int32(reader.ReadVarInt(), reader.ReadVector3(), reader.ReadVarInt());
	}

	// Token: 0x0600064E RID: 1614 RVA: 0x0001B8B8 File Offset: 0x00019AB8
	protected void UserCode_CmdTeleportToPlayer__UInt32__Vector3__NetworkConnectionToClient(uint targetPlayerNetId, Vector3 position, NetworkConnectionToClient conn)
	{
		if (NetworkServer.spawned[targetPlayerNetId] == null)
		{
			return;
		}
		if (conn == null)
		{
			return;
		}
		conn.identity.GetComponent<PlayerController>().ServerTeleport(position);
	}

	// Token: 0x0600064F RID: 1615 RVA: 0x0001B8E3 File Offset: 0x00019AE3
	protected static void InvokeUserCode_CmdTeleportToPlayer__UInt32__Vector3__NetworkConnectionToClient(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdTeleportToPlayer called on client.");
			return;
		}
		((NewConsole)obj).UserCode_CmdTeleportToPlayer__UInt32__Vector3__NetworkConnectionToClient(reader.ReadVarUInt(), reader.ReadVector3(), senderConnection);
	}

	// Token: 0x06000650 RID: 1616 RVA: 0x0001B914 File Offset: 0x00019B14
	static NewConsole()
	{
		RemoteProcedureCalls.RegisterCommand(typeof(NewConsole), "System.Void NewConsole::CmdGiveChallengeById(System.Int32)", new RemoteCallDelegate(NewConsole.InvokeUserCode_CmdGiveChallengeById__Int32), false);
		RemoteProcedureCalls.RegisterCommand(typeof(NewConsole), "System.Void NewConsole::CmdCompleteAllChallenges()", new RemoteCallDelegate(NewConsole.InvokeUserCode_CmdCompleteAllChallenges), false);
		RemoteProcedureCalls.RegisterCommand(typeof(NewConsole), "System.Void NewConsole::CmdClearAllChallenges()", new RemoteCallDelegate(NewConsole.InvokeUserCode_CmdClearAllChallenges), false);
		RemoteProcedureCalls.RegisterCommand(typeof(NewConsole), "System.Void NewConsole::CmdSimulateWinLose(System.UInt32,UnityEngine.Vector3,System.Boolean)", new RemoteCallDelegate(NewConsole.InvokeUserCode_CmdSimulateWinLose__UInt32__Vector3__Boolean), false);
		RemoteProcedureCalls.RegisterCommand(typeof(NewConsole), "System.Void NewConsole::CmdEnableAllFloors()", new RemoteCallDelegate(NewConsole.InvokeUserCode_CmdEnableAllFloors), false);
		RemoteProcedureCalls.RegisterCommand(typeof(NewConsole), "System.Void NewConsole::CmdTeleportPlayers(GameState)", new RemoteCallDelegate(NewConsole.InvokeUserCode_CmdTeleportPlayers__GameState), false);
		RemoteProcedureCalls.RegisterCommand(typeof(NewConsole), "System.Void NewConsole::CmdTeleportToCutscene(System.Int32)", new RemoteCallDelegate(NewConsole.InvokeUserCode_CmdTeleportToCutscene__Int32), false);
		RemoteProcedureCalls.RegisterCommand(typeof(NewConsole), "System.Void NewConsole::CmdProgressGame()", new RemoteCallDelegate(NewConsole.InvokeUserCode_CmdProgressGame), false);
		RemoteProcedureCalls.RegisterCommand(typeof(NewConsole), "System.Void NewConsole::RequestSpawnPrefab(System.Int32,UnityEngine.Vector3,System.Int32)", new RemoteCallDelegate(NewConsole.InvokeUserCode_RequestSpawnPrefab__Int32__Vector3__Int32), false);
		RemoteProcedureCalls.RegisterCommand(typeof(NewConsole), "System.Void NewConsole::CmdTeleportToPlayer(System.UInt32,UnityEngine.Vector3,Mirror.NetworkConnectionToClient)", new RemoteCallDelegate(NewConsole.InvokeUserCode_CmdTeleportToPlayer__UInt32__Vector3__NetworkConnectionToClient), false);
	}

	// Token: 0x040003FF RID: 1023
	public ToggleSettingItem _devConsoleSetting;

	// Token: 0x04000400 RID: 1024
	[Header("UI References")]
	[SerializeField]
	private GameObject consolePanel;

	// Token: 0x04000401 RID: 1025
	[SerializeField]
	private Transform sectionContainer;

	// Token: 0x04000402 RID: 1026
	[SerializeField]
	private Transform buttonContainer;

	// Token: 0x04000403 RID: 1027
	[SerializeField]
	private SpawnableSettings spawnableSettings;

	// Token: 0x04000404 RID: 1028
	[Header("Button Prefab")]
	[SerializeField]
	private GameObject sectionButtonPrefab;

	// Token: 0x04000405 RID: 1029
	[SerializeField]
	private GameObject buttonPrefab;

	// Token: 0x04000406 RID: 1030
	[Header("Settings")]
	[SerializeField]
	private float spawnRadius = 3f;

	// Token: 0x04000407 RID: 1031
	[SerializeField]
	private Vector3 spawnOffset = new Vector3(5f, 1f, 5f);

	// Token: 0x04000408 RID: 1032
	private bool isVisible;

	// Token: 0x04000409 RID: 1033
	private NewConsole.ConsoleState lastUsedState = NewConsole.ConsoleState.Spawn;

	// Token: 0x0400040A RID: 1034
	private bool _sectionsInitialized;

	// Token: 0x0400040B RID: 1035
	private int _spawnableId = -1;

	// Token: 0x0400040C RID: 1036
	private int _spawnIndex;

	// Token: 0x0400040D RID: 1037
	private int selectedCount = 1;

	// Token: 0x0400040E RID: 1038
	private int selectedValue = 1;

	// Token: 0x0400040F RID: 1039
	private List<SpawnableSO> availablePrefabs = new List<SpawnableSO>();

	// Token: 0x04000410 RID: 1040
	private GameSettings _gs;

	// Token: 0x0200009F RID: 159
	private enum ConsoleState
	{
		// Token: 0x04000412 RID: 1042
		Main,
		// Token: 0x04000413 RID: 1043
		Spawn,
		// Token: 0x04000414 RID: 1044
		Count,
		// Token: 0x04000415 RID: 1045
		Value,
		// Token: 0x04000416 RID: 1046
		Teleport,
		// Token: 0x04000417 RID: 1047
		Money,
		// Token: 0x04000418 RID: 1048
		Simulate,
		// Token: 0x04000419 RID: 1049
		Settings,
		// Token: 0x0400041A RID: 1050
		CasinoLevel,
		// Token: 0x0400041B RID: 1051
		Cosmetics,
		// Token: 0x0400041C RID: 1052
		Cameraman,
		// Token: 0x0400041D RID: 1053
		Challenges
	}

	// Token: 0x020000A0 RID: 160
	private enum TeleportLocation
	{
		// Token: 0x0400041F RID: 1055
		Lobby,
		// Token: 0x04000420 RID: 1056
		TestScene,
		// Token: 0x04000421 RID: 1057
		MainScene
	}
}
