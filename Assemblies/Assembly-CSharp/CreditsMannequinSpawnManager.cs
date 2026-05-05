using System;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

// Token: 0x02000228 RID: 552
public class CreditsMannequinSpawnManager : NetworkBehaviour
{
	// Token: 0x0600143D RID: 5181 RVA: 0x00056B93 File Offset: 0x00054D93
	public void SetSpawnPoints(Transform[] points)
	{
		this.spawnPoints = points;
	}

	// Token: 0x0600143E RID: 5182 RVA: 0x00056B9C File Offset: 0x00054D9C
	[Server]
	public void SpawnFromSnapshots(IReadOnlyList<PlayerCreditsSnapshot> snapshots)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void CreditsMannequinSpawnManager::SpawnFromSnapshots(System.Collections.Generic.IReadOnlyList`1<PlayerCreditsSnapshot>)' called when server was not active");
			return;
		}
		if (!base.isServer)
		{
			return;
		}
		this.ClearSpawned();
		if (this.mannequinPrefab == null || snapshots == null || this.spawnPoints == null || this.spawnPoints.Length == 0)
		{
			return;
		}
		int num = Mathf.Min(snapshots.Count, this.spawnPoints.Length);
		for (int i = 0; i < num; i++)
		{
			Transform transform = this.spawnPoints[i];
			if (!(transform == null))
			{
				GameObject gameObject = Object.Instantiate<GameObject>(this.mannequinPrefab, transform.position, transform.rotation, transform);
				this.spawnedMannequins.Add(gameObject);
				NetworkServer.Spawn(gameObject, null);
				CreditsMannequinController component = gameObject.GetComponent<CreditsMannequinController>();
				if (component != null)
				{
					component.RpcApplySnapshot(snapshots[i]);
				}
			}
		}
	}

	// Token: 0x0600143F RID: 5183 RVA: 0x00056C70 File Offset: 0x00054E70
	public void ClearSpawned()
	{
		if (!base.isServer)
		{
			return;
		}
		foreach (GameObject gameObject in this.spawnedMannequins)
		{
			if (gameObject != null)
			{
				NetworkServer.Destroy(gameObject);
			}
		}
		this.spawnedMannequins.Clear();
	}

	// Token: 0x06001441 RID: 5185 RVA: 0x00002321 File Offset: 0x00000521
	public override bool Weaved()
	{
		return true;
	}

	// Token: 0x04000CC8 RID: 3272
	[SerializeField]
	private GameObject mannequinPrefab;

	// Token: 0x04000CC9 RID: 3273
	[SerializeField]
	private Transform[] spawnPoints;

	// Token: 0x04000CCA RID: 3274
	private readonly List<GameObject> spawnedMannequins = new List<GameObject>();
}
