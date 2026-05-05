using System;
using Mirror;
using UnityEngine;

// Token: 0x020002EC RID: 748
public class SpawnArea : NetworkBehaviour
{
	// Token: 0x060019F4 RID: 6644 RVA: 0x0006D159 File Offset: 0x0006B359
	private void Start()
	{
		if (!base.isServer)
		{
			return;
		}
		base.InvokeRepeating("SpawnObject", 0f, 0.1f);
	}

	// Token: 0x060019F5 RID: 6645 RVA: 0x0006D179 File Offset: 0x0006B379
	private void Update()
	{
		if (!base.isServer)
		{
			return;
		}
		this.currentObjectCount = Object.FindObjectsByType<ValuableItem>(FindObjectsInactive.Exclude, FindObjectsSortMode.None).Length;
	}

	// Token: 0x060019F6 RID: 6646 RVA: 0x0006D194 File Offset: 0x0006B394
	private Vector3 CalculateRandomPositionWithinBoxColliderBounds()
	{
		Vector3 vector = this.spawnAreaCollider.center + base.transform.position;
		Vector3 size = this.spawnAreaCollider.size;
		float x = Random.Range(vector.x - size.x / 2f, vector.x + size.x / 2f);
		float y = Random.Range(vector.y - size.y / 2f, vector.y + size.y / 2f);
		float z = Random.Range(vector.z - size.z / 2f, vector.z + size.z / 2f);
		return new Vector3(x, y, z);
	}

	// Token: 0x060019F7 RID: 6647 RVA: 0x0006D254 File Offset: 0x0006B454
	[Server]
	public void SpawnObject()
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void SpawnArea::SpawnObject()' called when server was not active");
			return;
		}
		if (this._spawnedObjectCount >= this.maxObjectsInArea)
		{
			return;
		}
		this._spawnedObjectCount++;
		Vector3 position = this.CalculateRandomPositionWithinBoxColliderBounds();
		ValuableItem valuableItem = Object.Instantiate<ValuableItem>(this.objectToSpawn, position, Quaternion.identity);
		NetworkServer.Spawn(valuableItem.gameObject, null);
		ValuableItem valuableItem2;
		this.RandomizeValuableItemValue(valuableItem.TryGetComponent<ValuableItem>(out valuableItem2) ? valuableItem2 : null);
	}

	// Token: 0x060019F8 RID: 6648 RVA: 0x0006D2CC File Offset: 0x0006B4CC
	[Server]
	public void RandomizeValuableItemValue(ValuableItem item)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void SpawnArea::RandomizeValuableItemValue(ValuableItem)' called when server was not active");
			return;
		}
		if (!item)
		{
			return;
		}
		float num = 0.15f;
		if (Random.value <= num)
		{
			item.ServerChangeValue(item.value * 20);
		}
	}

	// Token: 0x060019FA RID: 6650 RVA: 0x00002321 File Offset: 0x00000521
	public override bool Weaved()
	{
		return true;
	}

	// Token: 0x040010A4 RID: 4260
	[SerializeField]
	private ValuableItem objectToSpawn;

	// Token: 0x040010A5 RID: 4261
	[SerializeField]
	private BoxCollider spawnAreaCollider;

	// Token: 0x040010A6 RID: 4262
	[SerializeField]
	private int maxObjectsInArea = 30;

	// Token: 0x040010A7 RID: 4263
	[SerializeField]
	private int currentObjectCount;

	// Token: 0x040010A8 RID: 4264
	private int _spawnedObjectCount;
}
