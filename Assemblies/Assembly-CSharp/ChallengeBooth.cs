using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using Extensions;
using Mirror;
using Mirror.RemoteCalls;
using UnityEngine;
using UnityEngine.Events;

// Token: 0x02000109 RID: 265
public class ChallengeBooth : NetworkBehaviour
{
	// Token: 0x06000B08 RID: 2824 RVA: 0x0002C54C File Offset: 0x0002A74C
	public override void OnStartServer()
	{
		base.OnStartServer();
		this._challengeSettings = Resources.Load<ChallengeSettings>("ChallengeSettings");
		this._gameSettings = Resources.Load<GameSettings>("GameSettings");
		this.dailyChallengeLimit = this._challengeSettings.dailyAvailableChallengeCount;
	}

	// Token: 0x06000B09 RID: 2825 RVA: 0x0002C588 File Offset: 0x0002A788
	[Server]
	public void TryGiveDailyChallenge()
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void ChallengeBooth::TryGiveDailyChallenge()' called when server was not active");
			return;
		}
		if (NetworkSingleton<GameManager>.Instance == null)
		{
			return;
		}
		int daysPassed = NetworkSingleton<GameManager>.Instance.daysPassed;
		if (this.lastDayChallengeGiven == daysPassed)
		{
			return;
		}
		List<ChallengeProgress> activeChallenges = NetworkSingleton<ChallengeManager>.Instance.GetActiveChallenges();
		if (activeChallenges != null && activeChallenges.Count > 0)
		{
			foreach (ChallengeProgress challengeProgress in activeChallenges)
			{
				NetworkSingleton<ChallengeManager>.Instance.DeactivateChallenge(challengeProgress.challenge);
			}
		}
		this.GiveChallengeInternal(null, -1);
		this.NetworklastDayChallengeGiven = daysPassed;
	}

	// Token: 0x06000B0A RID: 2826 RVA: 0x0002C640 File Offset: 0x0002A840
	[Server]
	public void RerollChallenge()
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void ChallengeBooth::RerollChallenge()' called when server was not active");
			return;
		}
		int challengeRerollPrice = this._challengeSettings.challengeRerollPrice;
		if (NetworkSingleton<MoneyManager>.Instance == null)
		{
			Debug.LogWarning("[ChallengeBooth] MoneyManager not found - cannot reroll challenge");
			return;
		}
		if (NetworkSingleton<MoneyManager>.Instance.ticketBalance < (long)challengeRerollPrice)
		{
			Debug.Log(string.Format("[ChallengeBooth] Not enough tickets to reroll! Need {0}, have {1}", challengeRerollPrice, NetworkSingleton<MoneyManager>.Instance.ticketBalance));
			this.RpcNotifyRerollFailed("Not enough tickets");
			return;
		}
		List<ChallengeProgress> activeChallenges = NetworkSingleton<ChallengeManager>.Instance.GetActiveChallenges();
		HashSet<Challenge> hashSet = new HashSet<Challenge>(from p in activeChallenges
		select p.challenge into c
		where c != null
		select c);
		int currentFloor = NetworkSingleton<GameManager>.Instance.currentFloor;
		List<Challenge> challengesByFloorIndex = NetworkSingleton<ChallengeManager>.Instance.GetChallengesByFloorIndex(currentFloor);
		if (challengesByFloorIndex == null)
		{
			return;
		}
		int num = currentFloor + 1;
		HashSet<CasinoGameType> availableGameTypesForFloor = NextCasinoPredicter.GetAvailableGameTypesForFloor(num);
		if (this.debugMode)
		{
			Debug.Log(string.Format("[ChallengeBooth.RerollChallenge] Floor {0} (loot table {1}) - Available game types: {2}", currentFloor, num, string.Join<CasinoGameType>(", ", availableGameTypesForFloor)));
			Debug.Log(string.Format("[ChallengeBooth.RerollChallenge] Total challenges for floor {0}: {1}", currentFloor, challengesByFloorIndex.Count));
		}
		List<ValueTuple<Challenge, HashSet<CasinoGameType>>> list = new List<ValueTuple<Challenge, HashSet<CasinoGameType>>>();
		List<Challenge> list2 = new List<Challenge>();
		foreach (Challenge challenge in challengesByFloorIndex)
		{
			if (!(challenge == null))
			{
				HashSet<CasinoGameType> requiredGameTypes = challenge.GetRequiredGameTypes();
				if (requiredGameTypes == null || requiredGameTypes.Count == 0)
				{
					list2.Add(challenge);
					if (this.debugMode)
					{
						Debug.Log("[ChallengeBooth.RerollChallenge] ✓ '" + challenge.challengeName + "' - Game-agnostic (allowed)");
					}
				}
				else if (requiredGameTypes.Overlaps(availableGameTypesForFloor))
				{
					list2.Add(challenge);
					if (this.debugMode)
					{
						Debug.Log(string.Concat(new string[]
						{
							"[ChallengeBooth.RerollChallenge] ✓ '",
							challenge.challengeName,
							"' - Requires ",
							string.Join<CasinoGameType>(", ", requiredGameTypes),
							" (available)"
						}));
					}
				}
				else
				{
					list.Add(new ValueTuple<Challenge, HashSet<CasinoGameType>>(challenge, requiredGameTypes));
					if (this.debugMode)
					{
						Debug.Log(string.Concat(new string[]
						{
							"[ChallengeBooth.RerollChallenge] ✗ '",
							challenge.challengeName,
							"' - Requires ",
							string.Join<CasinoGameType>(", ", requiredGameTypes),
							" (NOT available - FILTERED OUT)"
						}));
					}
				}
			}
		}
		if (this.debugMode)
		{
			Debug.Log(string.Format("[ChallengeBooth.RerollChallenge] Valid challenges: {0}, Filtered out: {1}", list2.Count, list.Count));
		}
		if (list2.Count == 0)
		{
			Debug.LogWarning(string.Format("[ChallengeBooth] No valid challenges available for floor {0} (all require unavailable games).", currentFloor));
			return;
		}
		int successfulQuota = NetworkSingleton<GameManager>.Instance.successfulQuota;
		int daysPassed = NetworkSingleton<GameManager>.Instance.daysPassed;
		Random random = new Random(this.GetDeterministicChallengeHash(NetworkSingleton<SeededRandomManager>.Instance.CurrentSeed, successfulQuota, daysPassed, 0));
		List<Challenge> list3 = list2.ToList<Challenge>();
		for (int i = list3.Count - 1; i > 0; i--)
		{
			int index = random.Next(0, i + 1);
			Challenge value = list3[i];
			list3[i] = list3[index];
			list3[index] = value;
		}
		int rerollAttempt = 0;
		if (hashSet.Count > 0)
		{
			Challenge item = hashSet.First<Challenge>();
			int num2 = list3.IndexOf(item);
			if (num2 >= 0)
			{
				rerollAttempt = num2 + 1;
			}
			else
			{
				rerollAttempt = hashSet.Count;
			}
		}
		HashSet<Challenge> excludeChallenges = new HashSet<Challenge>(hashSet);
		if (activeChallenges != null && activeChallenges.Count > 0)
		{
			foreach (ChallengeProgress challengeProgress in activeChallenges)
			{
				if (challengeProgress.challenge != null)
				{
					NetworkSingleton<ChallengeManager>.Instance.DeactivateChallenge(challengeProgress.challenge);
				}
			}
		}
		NetworkSingleton<MoneyManager>.Instance.TryChangeTicketBalance((long)(-(long)challengeRerollPrice));
		this.GiveChallengeInternal(excludeChallenges, rerollAttempt);
		this.RpcNotifyChallengeRerolled(challengeRerollPrice);
	}

	// Token: 0x06000B0B RID: 2827 RVA: 0x0002CAB0 File Offset: 0x0002ACB0
	[Server]
	private void GiveChallengeInternal(HashSet<Challenge> excludeChallenges = null, int rerollAttempt = -1)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void ChallengeBooth::GiveChallengeInternal(System.Collections.Generic.HashSet`1<Challenge>,System.Int32)' called when server was not active");
			return;
		}
		int currentFloor = NetworkSingleton<GameManager>.Instance.currentFloor;
		List<Challenge> challengesByFloorIndex = NetworkSingleton<ChallengeManager>.Instance.GetChallengesByFloorIndex(currentFloor);
		if (challengesByFloorIndex == null || challengesByFloorIndex.Count == 0)
		{
			Debug.LogWarning(string.Format("[ChallengeBooth] No challenges available for floor index: {0}", currentFloor));
			return;
		}
		HashSet<Challenge> collection = new HashSet<Challenge>(from p in NetworkSingleton<ChallengeManager>.Instance.GetActiveChallenges()
		select p.challenge into c
		where c != null
		select c);
		HashSet<Challenge> excludedSet = new HashSet<Challenge>(collection);
		if (excludeChallenges != null)
		{
			excludedSet.UnionWith(excludeChallenges);
		}
		int num = currentFloor + 1;
		HashSet<CasinoGameType> availableGameTypesForFloor = NextCasinoPredicter.GetAvailableGameTypesForFloor(num);
		if (this.debugMode)
		{
			Debug.Log(string.Format("[ChallengeBooth] Floor {0} (loot table {1}) - Available game types: {2}", currentFloor, num, string.Join<CasinoGameType>(", ", availableGameTypesForFloor)));
			Debug.Log(string.Format("[ChallengeBooth] Total challenges for floor {0}: {1}", currentFloor, challengesByFloorIndex.Count));
		}
		List<ValueTuple<Challenge, HashSet<CasinoGameType>>> list = new List<ValueTuple<Challenge, HashSet<CasinoGameType>>>();
		List<Challenge> list2 = new List<Challenge>();
		foreach (Challenge challenge in challengesByFloorIndex)
		{
			if (!(challenge == null))
			{
				HashSet<CasinoGameType> requiredGameTypes = challenge.GetRequiredGameTypes();
				if (requiredGameTypes == null || requiredGameTypes.Count == 0)
				{
					list2.Add(challenge);
					if (this.debugMode)
					{
						Debug.Log("[ChallengeBooth] ✓ '" + challenge.challengeName + "' - Game-agnostic (allowed)");
					}
				}
				else if (requiredGameTypes.Overlaps(availableGameTypesForFloor))
				{
					list2.Add(challenge);
					if (this.debugMode)
					{
						Debug.Log(string.Concat(new string[]
						{
							"[ChallengeBooth] ✓ '",
							challenge.challengeName,
							"' - Requires ",
							string.Join<CasinoGameType>(", ", requiredGameTypes),
							" (available)"
						}));
					}
				}
				else
				{
					list.Add(new ValueTuple<Challenge, HashSet<CasinoGameType>>(challenge, requiredGameTypes));
					if (this.debugMode)
					{
						Debug.Log(string.Concat(new string[]
						{
							"[ChallengeBooth] ✗ '",
							challenge.challengeName,
							"' - Requires ",
							string.Join<CasinoGameType>(", ", requiredGameTypes),
							" (NOT available - FILTERED OUT)"
						}));
					}
				}
			}
		}
		if (this.debugMode)
		{
			Debug.Log(string.Format("[ChallengeBooth] Valid challenges: {0}, Filtered out: {1}", list2.Count, list.Count));
		}
		if (list2.Count == 0)
		{
			Debug.LogWarning(string.Format("[ChallengeBooth] No valid challenges available for floor {0} (all require unavailable games).", currentFloor));
			return;
		}
		int successfulQuota = NetworkSingleton<GameManager>.Instance.successfulQuota;
		int daysPassed = NetworkSingleton<GameManager>.Instance.daysPassed;
		if (rerollAttempt < 0)
		{
			rerollAttempt = excludedSet.Count;
		}
		Random random = new Random(this.GetDeterministicChallengeHash(NetworkSingleton<SeededRandomManager>.Instance.CurrentSeed, successfulQuota, daysPassed, 0));
		List<Challenge> list3 = list2.ToList<Challenge>();
		for (int i = list3.Count - 1; i > 0; i--)
		{
			int index = random.Next(0, i + 1);
			Challenge value = list3[i];
			list3[i] = list3[index];
			list3[index] = value;
		}
		Challenge challenge2 = null;
		int num2 = 0;
		while (challenge2 == null && num2 < list3.Count)
		{
			int index2 = (rerollAttempt + num2) % list3.Count;
			Challenge challenge3 = list3[index2];
			if (!excludedSet.Contains(challenge3))
			{
				challenge2 = challenge3;
				break;
			}
			num2++;
		}
		if (challenge2 == null)
		{
			List<Challenge> list4 = (from c in list2
			where !excludedSet.Contains(c)
			select c).ToList<Challenge>();
			if (list4.Count <= 0)
			{
				Debug.LogWarning(string.Format("[ChallengeBooth] Could not find any available challenge for floor {0} after filtering.", currentFloor));
				return;
			}
			challenge2 = list4[0];
		}
		Challenge challenge4 = challenge2;
		Debug.Log(string.Format("[ChallengeBooth] Selected challenge: '{0}' (rerollAttempt: {1}, index in shuffled: {2})", challenge4.challengeName, rerollAttempt, list3.IndexOf(challenge4)));
		NetworkSingleton<ChallengeManager>.Instance.ActivateChallenge(challenge4);
		this.RpcNotifyChallengePurchased(challenge4.challengeName, currentFloor);
	}

	// Token: 0x06000B0C RID: 2828 RVA: 0x0002CF24 File Offset: 0x0002B124
	private int GetDeterministicChallengeHash(int seed, int quotaIndex, int day, int rerollAttempt = 0)
	{
		return ((seed * 31 + quotaIndex) * 31 + day) * 31 + rerollAttempt;
	}

	// Token: 0x06000B0D RID: 2829 RVA: 0x0002CF38 File Offset: 0x0002B138
	[ClientRpc]
	private void RpcNotifyChallengePurchased(string challengeName, int floorIndex)
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		writer.WriteString(challengeName);
		writer.WriteVarInt(floorIndex);
		this.SendRPCInternal("System.Void ChallengeBooth::RpcNotifyChallengePurchased(System.String,System.Int32)", -963902717, writer, 0, true);
		NetworkWriterPool.Return(writer);
	}

	// Token: 0x06000B0E RID: 2830 RVA: 0x0002CF7C File Offset: 0x0002B17C
	[ClientRpc]
	private void RpcNotifyChallengeRerolled(int cost)
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		writer.WriteVarInt(cost);
		this.SendRPCInternal("System.Void ChallengeBooth::RpcNotifyChallengeRerolled(System.Int32)", 345287561, writer, 0, true);
		NetworkWriterPool.Return(writer);
	}

	// Token: 0x06000B0F RID: 2831 RVA: 0x0002CFB8 File Offset: 0x0002B1B8
	[ClientRpc]
	private void RpcNotifyRerollFailed(string reason)
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		writer.WriteString(reason);
		this.SendRPCInternal("System.Void ChallengeBooth::RpcNotifyRerollFailed(System.String)", 1133253287, writer, 0, true);
		NetworkWriterPool.Return(writer);
	}

	// Token: 0x06000B10 RID: 2832 RVA: 0x0002CFF4 File Offset: 0x0002B1F4
	private void OnTriggerEnter(Collider other)
	{
		if (!base.isServer)
		{
			return;
		}
		if (other.attachedRigidbody == null)
		{
			return;
		}
		PlayerController item;
		if (other.attachedRigidbody.TryGetComponent<PlayerController>(out item) && !this.playersInside.Contains(item))
		{
			this.playersInside.Add(item);
			this.CheckAllPlayersInside();
		}
	}

	// Token: 0x06000B11 RID: 2833 RVA: 0x0002D048 File Offset: 0x0002B248
	private void OnTriggerExit(Collider other)
	{
		if (!base.isServer)
		{
			return;
		}
		if (other.attachedRigidbody == null)
		{
			return;
		}
		PlayerController item;
		if (other.attachedRigidbody.TryGetComponent<PlayerController>(out item))
		{
			this.playersInside.Remove(item);
		}
	}

	// Token: 0x06000B12 RID: 2834 RVA: 0x0002D08C File Offset: 0x0002B28C
	[Server]
	private void CheckAllPlayersInside()
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void ChallengeBooth::CheckAllPlayersInside()' called when server was not active");
			return;
		}
		List<PlayerController> list = Object.FindObjectsByType<PlayerController>(FindObjectsSortMode.None).ToList<PlayerController>();
		if (list.Count == 0)
		{
			return;
		}
		if (this.playersInside.Count >= list.Count)
		{
			bool flag = true;
			foreach (PlayerController item in list)
			{
				if (!this.playersInside.Contains(item))
				{
					flag = false;
					break;
				}
			}
			if (flag)
			{
				UnityEvent unityEvent = this.onAllPlayersInside;
				if (unityEvent == null)
				{
					return;
				}
				unityEvent.Invoke();
			}
		}
	}

	// Token: 0x06000B14 RID: 2836 RVA: 0x00002321 File Offset: 0x00000521
	public override bool Weaved()
	{
		return true;
	}

	// Token: 0x170000F5 RID: 245
	// (get) Token: 0x06000B15 RID: 2837 RVA: 0x0002D154 File Offset: 0x0002B354
	// (set) Token: 0x06000B16 RID: 2838 RVA: 0x0002D167 File Offset: 0x0002B367
	public int NetworklastDayChallengeGiven
	{
		get
		{
			return this.lastDayChallengeGiven;
		}
		[param: In]
		set
		{
			base.GeneratedSyncVarSetter<int>(value, ref this.lastDayChallengeGiven, 1UL, null);
		}
	}

	// Token: 0x06000B17 RID: 2839 RVA: 0x0002D181 File Offset: 0x0002B381
	protected void UserCode_RpcNotifyChallengePurchased__String__Int32(string challengeName, int floorIndex)
	{
		Debug.Log(string.Format("[ChallengeBooth] Received challenge: {0} ({1})", challengeName, floorIndex));
	}

	// Token: 0x06000B18 RID: 2840 RVA: 0x0002D199 File Offset: 0x0002B399
	protected static void InvokeUserCode_RpcNotifyChallengePurchased__String__Int32(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcNotifyChallengePurchased called on server.");
			return;
		}
		((ChallengeBooth)obj).UserCode_RpcNotifyChallengePurchased__String__Int32(reader.ReadString(), reader.ReadVarInt());
	}

	// Token: 0x06000B19 RID: 2841 RVA: 0x0002D1C8 File Offset: 0x0002B3C8
	protected void UserCode_RpcNotifyChallengeRerolled__Int32(int cost)
	{
		Debug.Log(string.Format("[ChallengeBooth] Challenge rerolled for {0} ticket(s)", cost));
		if (this.rerollChallengeSfx != null)
		{
			this.rerollChallengeSfx.PlayOneShotWith3DPos();
		}
	}

	// Token: 0x06000B1A RID: 2842 RVA: 0x0002D1F8 File Offset: 0x0002B3F8
	protected static void InvokeUserCode_RpcNotifyChallengeRerolled__Int32(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcNotifyChallengeRerolled called on server.");
			return;
		}
		((ChallengeBooth)obj).UserCode_RpcNotifyChallengeRerolled__Int32(reader.ReadVarInt());
	}

	// Token: 0x06000B1B RID: 2843 RVA: 0x0002D221 File Offset: 0x0002B421
	protected void UserCode_RpcNotifyRerollFailed__String(string reason)
	{
		Debug.Log("[ChallengeBooth] Failed to reroll challenge: " + reason);
		if (this.invalidInteractionSfx != null)
		{
			this.invalidInteractionSfx.PlayOneShotWith3DPos();
		}
	}

	// Token: 0x06000B1C RID: 2844 RVA: 0x0002D24C File Offset: 0x0002B44C
	protected static void InvokeUserCode_RpcNotifyRerollFailed__String(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcNotifyRerollFailed called on server.");
			return;
		}
		((ChallengeBooth)obj).UserCode_RpcNotifyRerollFailed__String(reader.ReadString());
	}

	// Token: 0x06000B1D RID: 2845 RVA: 0x0002D278 File Offset: 0x0002B478
	static ChallengeBooth()
	{
		RemoteProcedureCalls.RegisterRpc(typeof(ChallengeBooth), "System.Void ChallengeBooth::RpcNotifyChallengePurchased(System.String,System.Int32)", new RemoteCallDelegate(ChallengeBooth.InvokeUserCode_RpcNotifyChallengePurchased__String__Int32));
		RemoteProcedureCalls.RegisterRpc(typeof(ChallengeBooth), "System.Void ChallengeBooth::RpcNotifyChallengeRerolled(System.Int32)", new RemoteCallDelegate(ChallengeBooth.InvokeUserCode_RpcNotifyChallengeRerolled__Int32));
		RemoteProcedureCalls.RegisterRpc(typeof(ChallengeBooth), "System.Void ChallengeBooth::RpcNotifyRerollFailed(System.String)", new RemoteCallDelegate(ChallengeBooth.InvokeUserCode_RpcNotifyRerollFailed__String));
	}

	// Token: 0x06000B1E RID: 2846 RVA: 0x0002D2E8 File Offset: 0x0002B4E8
	public override void SerializeSyncVars(NetworkWriter writer, bool forceAll)
	{
		base.SerializeSyncVars(writer, forceAll);
		if (forceAll)
		{
			writer.WriteVarInt(this.lastDayChallengeGiven);
			return;
		}
		writer.WriteVarULong(this.syncVarDirtyBits);
		if ((this.syncVarDirtyBits & 1UL) != 0UL)
		{
			writer.WriteVarInt(this.lastDayChallengeGiven);
		}
	}

	// Token: 0x06000B1F RID: 2847 RVA: 0x0002D340 File Offset: 0x0002B540
	public override void DeserializeSyncVars(NetworkReader reader, bool initialState)
	{
		base.DeserializeSyncVars(reader, initialState);
		if (initialState)
		{
			base.GeneratedSyncVarDeserialize<int>(ref this.lastDayChallengeGiven, null, reader.ReadVarInt());
			return;
		}
		long num = (long)reader.ReadVarULong();
		if ((num & 1L) != 0L)
		{
			base.GeneratedSyncVarDeserialize<int>(ref this.lastDayChallengeGiven, null, reader.ReadVarInt());
		}
	}

	// Token: 0x040006E3 RID: 1763
	private ChallengeSettings _challengeSettings;

	// Token: 0x040006E4 RID: 1764
	private GameSettings _gameSettings;

	// Token: 0x040006E5 RID: 1765
	[Header("Settings")]
	[SerializeField]
	private int dailyChallengeLimit;

	// Token: 0x040006E6 RID: 1766
	[SyncVar]
	private int lastDayChallengeGiven = -1;

	// Token: 0x040006E7 RID: 1767
	[Header("Trigger Events")]
	[SerializeField]
	private UnityEvent onAllPlayersInside;

	// Token: 0x040006E8 RID: 1768
	private List<PlayerController> playersInside = new List<PlayerController>();

	// Token: 0x040006E9 RID: 1769
	[SerializeField]
	private bool debugMode;

	// Token: 0x040006EA RID: 1770
	[Header("SFX")]
	[SerializeField]
	private SFXComponent rerollChallengeSfx;

	// Token: 0x040006EB RID: 1771
	[SerializeField]
	private SFXComponent invalidInteractionSfx;
}
