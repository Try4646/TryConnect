using System;
using System.Collections;
using System.Collections.Generic;
using Extensions;
using Mirror;
using UnityEngine;

// Token: 0x02000229 RID: 553
public class CreditsRollManager : NetworkSingleton<CreditsRollManager>
{
	// Token: 0x06001442 RID: 5186 RVA: 0x00056CF4 File Offset: 0x00054EF4
	public void BeginCredits(IReadOnlyList<PlayerCreditsSnapshot> snapshots)
	{
		if (!base.isServer)
		{
			return;
		}
		if (this.hasSpawnedCredits)
		{
			return;
		}
		if (snapshots == null || snapshots.Count == 0)
		{
			Debug.LogWarning("[CreditsRollManager] BeginCredits called with no snapshots.");
			return;
		}
		if (this.mannequinSpawnManager == null)
		{
			this.mannequinSpawnManager = Object.FindFirstObjectByType<CreditsMannequinSpawnManager>();
		}
		if (this.mannequinSpawnManager == null)
		{
			Debug.LogWarning("[CreditsRollManager] CreditsMannequinSpawnManager not found.");
			return;
		}
		Debug.Log(string.Format("[CreditsRollManager] Spawning mannequins from {0} snapshots.", snapshots.Count));
		this.mannequinSpawnManager.SpawnFromSnapshots(snapshots);
		this.hasSpawnedCredits = true;
	}

	// Token: 0x06001443 RID: 5187 RVA: 0x00056D88 File Offset: 0x00054F88
	[Server]
	public void BeginCreditsFromScenePlayers()
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void CreditsRollManager::BeginCreditsFromScenePlayers()' called when server was not active");
			return;
		}
		if (this.hasSpawnedCredits)
		{
			return;
		}
		base.StartCoroutine(this.BeginCreditsFromScenePlayersRoutine());
	}

	// Token: 0x06001444 RID: 5188 RVA: 0x00056DB5 File Offset: 0x00054FB5
	private IEnumerator BeginCreditsFromScenePlayersRoutine()
	{
		yield return new WaitForSeconds(2f);
		int num2;
		for (int attempt = 1; attempt <= 5; attempt = num2 + 1)
		{
			List<PlayerCreditsSnapshot> list = CreditsRollManager.BuildSnapshotsFromScenePlayers();
			int num = CreditsRollManager.CountTotalCosmetics(list);
			Debug.Log(string.Format("[CreditsRollManager] Snapshot attempt {0}/{1}: players={2}, cosmetics={3}", new object[]
			{
				attempt,
				5,
				list.Count,
				num
			}));
			if (list.Count > 0 && (num > 0 || attempt == 5))
			{
				if (num == 0)
				{
					Debug.LogWarning("[CreditsRollManager] Proceeding after max attempts with 0 cosmetics.");
				}
				this.BeginCredits(list);
				yield break;
			}
			yield return null;
			num2 = attempt;
		}
		yield break;
	}

	// Token: 0x06001445 RID: 5189 RVA: 0x00056DC4 File Offset: 0x00054FC4
	private static List<PlayerCreditsSnapshot> BuildSnapshotsFromScenePlayers()
	{
		List<PlayerCreditsSnapshot> list = new List<PlayerCreditsSnapshot>();
		PlayerCustomization[] array = Object.FindObjectsByType<PlayerCustomization>(FindObjectsSortMode.None);
		Debug.Log(string.Format("[CreditsRollManager] Found {0} PlayerCustomization components in scene.", array.Length));
		foreach (PlayerCustomization playerCustomization in array)
		{
			if (playerCustomization == null)
			{
				Debug.LogWarning("[CreditsRollManager] Skipping null PlayerCustomization.");
			}
			else
			{
				PlayerProfile component = playerCustomization.GetComponent<PlayerProfile>();
				if (component == null || component.steamId == 0UL)
				{
					Debug.LogWarning("[CreditsRollManager] Skipping player with missing PlayerProfile or steamId == 0.");
				}
				else
				{
					PlayerCreditsSnapshot playerCreditsSnapshot = new PlayerCreditsSnapshot
					{
						steamId = component.steamId,
						displayName = component.playerName
					};
					foreach (KeyValuePair<CosmeticType, int> keyValuePair in playerCustomization.GetEquippedCosmetics())
					{
						playerCreditsSnapshot.cosmetics.Add(new PlayerCreditsSnapshot.CosmeticEntry
						{
							type = keyValuePair.Key,
							cosmeticId = keyValuePair.Value
						});
					}
					list.Add(playerCreditsSnapshot);
					Debug.Log(string.Format("[CreditsRollManager] Snapshot added for '{0}' ({1}) with {2} cosmetics.", playerCreditsSnapshot.displayName, playerCreditsSnapshot.steamId, playerCreditsSnapshot.cosmetics.Count));
				}
			}
		}
		return list;
	}

	// Token: 0x06001446 RID: 5190 RVA: 0x00056F28 File Offset: 0x00055128
	private static int CountTotalCosmetics(List<PlayerCreditsSnapshot> snapshots)
	{
		int num = 0;
		foreach (PlayerCreditsSnapshot playerCreditsSnapshot in snapshots)
		{
			num += playerCreditsSnapshot.cosmetics.Count;
		}
		return num;
	}

	// Token: 0x06001448 RID: 5192 RVA: 0x00002321 File Offset: 0x00000521
	public override bool Weaved()
	{
		return true;
	}

	// Token: 0x04000CCB RID: 3275
	[SerializeField]
	private CreditsMannequinSpawnManager mannequinSpawnManager;

	// Token: 0x04000CCC RID: 3276
	[SerializeField]
	private List<PlayerCreditsSnapshot> debugSnapshots = new List<PlayerCreditsSnapshot>();

	// Token: 0x04000CCD RID: 3277
	private bool hasSpawnedCredits;
}
