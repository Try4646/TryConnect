using System;
using System.Collections.Generic;
using System.Linq;
using Extensions;
using UnityEngine;

// Token: 0x02000311 RID: 785
[CreateAssetMenu(menuName = "Game Settings/Challenge Settings", fileName = "ChallengeSettings")]
public class ChallengeSettings : ScriptableObject
{
	// Token: 0x14000021 RID: 33
	// (add) Token: 0x06001AA5 RID: 6821 RVA: 0x00070EE4 File Offset: 0x0006F0E4
	// (remove) Token: 0x06001AA6 RID: 6822 RVA: 0x00070F18 File Offset: 0x0006F118
	public static event Action<ChallengeSettings> SettingsChanged;

	// Token: 0x06001AA7 RID: 6823 RVA: 0x00070F4B File Offset: 0x0006F14B
	public void NotifyChanged()
	{
		Action<ChallengeSettings> settingsChanged = ChallengeSettings.SettingsChanged;
		if (settingsChanged == null)
		{
			return;
		}
		settingsChanged(this);
	}

	// Token: 0x06001AA8 RID: 6824 RVA: 0x00070F60 File Offset: 0x0006F160
	public void UpdateActiveChallenges()
	{
		this.activeChallenges.Clear();
		if (NetworkSingleton<ChallengeManager>.Instance == null)
		{
			return;
		}
		foreach (ChallengeProgress challengeProgress in NetworkSingleton<ChallengeManager>.Instance.GetActiveChallenges())
		{
			if (challengeProgress != null && !(challengeProgress.challenge == null) && !challengeProgress.isClaimed)
			{
				this.activeChallenges.Add(new ActiveChallengeInfo(challengeProgress.challenge, "Server", challengeProgress.progress, challengeProgress.progressText, challengeProgress.isCompleted, challengeProgress.isClaimed));
			}
		}
	}

	// Token: 0x06001AA9 RID: 6825 RVA: 0x00071018 File Offset: 0x0006F218
	public List<Challenge> GetChallengesByFloorIndex(int floorIndex)
	{
		List<Challenge> result;
		switch (floorIndex)
		{
		case 0:
			result = this.firstFloorChallenges;
			break;
		case 1:
			result = this.secondFloorChallenges;
			break;
		case 2:
			result = this.thirdFloorChallenges;
			break;
		case 3:
			result = this.fourthFloorChallenges;
			break;
		default:
			result = new List<Challenge>();
			break;
		}
		return result;
	}

	// Token: 0x06001AAA RID: 6826 RVA: 0x00071068 File Offset: 0x0006F268
	public Challenge GetRandomChallenge(int floorIndex)
	{
		List<Challenge> challengesByFloorIndex = this.GetChallengesByFloorIndex(floorIndex);
		if (challengesByFloorIndex == null || challengesByFloorIndex.Count == 0)
		{
			return null;
		}
		int floorIndex2 = floorIndex + 1;
		HashSet<CasinoGameType> availableGameTypes = NextCasinoPredicter.GetAvailableGameTypesForFloor(floorIndex2);
		List<Challenge> list = (from c in challengesByFloorIndex
		where c != null
		select c).Where(delegate(Challenge c)
		{
			HashSet<CasinoGameType> requiredGameTypes = c.GetRequiredGameTypes();
			return requiredGameTypes == null || requiredGameTypes.Count == 0 || requiredGameTypes.Overlaps(availableGameTypes);
		}).ToList<Challenge>();
		if (list.Count == 0)
		{
			return null;
		}
		return list.GetRandomElement<Challenge>();
	}

	// Token: 0x06001AAB RID: 6827 RVA: 0x000710EC File Offset: 0x0006F2EC
	public int GetTicketReward(int floorIndex)
	{
		return this.ticketRewards[floorIndex];
	}

	// Token: 0x04001160 RID: 4448
	public int dailyAvailableChallengeCount = 1;

	// Token: 0x04001161 RID: 4449
	[Tooltip("Cost in tickets to reroll a challenge")]
	[Min(0f)]
	public int challengeRerollPrice = 1;

	// Token: 0x04001162 RID: 4450
	public int[] ticketRewards = new int[4];

	// Token: 0x04001163 RID: 4451
	[Header("Challenges")]
	[Tooltip("List of all challenges in the game. These will be available in the ChallengeBooth.")]
	public List<Challenge> challenges = new List<Challenge>();

	// Token: 0x04001164 RID: 4452
	[Header("Challenge Lists by Difficulty")]
	[Tooltip("First floor challenges")]
	public List<Challenge> firstFloorChallenges = new List<Challenge>();

	// Token: 0x04001165 RID: 4453
	[Tooltip("Medium difficulty challenges")]
	public List<Challenge> secondFloorChallenges = new List<Challenge>();

	// Token: 0x04001166 RID: 4454
	[Tooltip("Third floor challenges")]
	public List<Challenge> thirdFloorChallenges = new List<Challenge>();

	// Token: 0x04001167 RID: 4455
	[Tooltip("Fourth floor challenges")]
	public List<Challenge> fourthFloorChallenges = new List<Challenge>();

	// Token: 0x04001168 RID: 4456
	[Header("Runtime: Currently Active Challenges")]
	[Tooltip("Challenges that players have purchased and are currently working on (runtime only - updates automatically)")]
	public List<ActiveChallengeInfo> activeChallenges = new List<ActiveChallengeInfo>();
}
