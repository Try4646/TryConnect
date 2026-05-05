using System;
using System.Collections;
using System.Collections.Generic;
using Extensions;
using Mirror;
using UnityEngine;
using UnityEngine.Events;

// Token: 0x02000152 RID: 338
public class ElevatorGuy : InteractableBase
{
	// Token: 0x06000CD5 RID: 3285 RVA: 0x0003614C File Offset: 0x0003434C
	public override void ServerOnInteract(PlayerInteract playerInteract)
	{
		base.ServerOnInteract(playerInteract);
		if (this._isSpawning)
		{
			return;
		}
		if (NetworkSingleton<ElevatorManager>.Instance.isTeleporting)
		{
			return;
		}
		this._isSpawning = true;
		UnityEvent unityEvent = this.serverOnInteractElevatorGuy;
		if (unityEvent != null)
		{
			unityEvent.Invoke();
		}
		if (!this._hasSpawned)
		{
			base.StartCoroutine(this.ItemInitializeRoutine());
			return;
		}
		base.StartCoroutine(this.ItemSpawnRoutine());
	}

	// Token: 0x06000CD6 RID: 3286 RVA: 0x000361B1 File Offset: 0x000343B1
	private IEnumerator ItemInitializeRoutine()
	{
		List<SpawnableSO> currentItems = NetworkSingleton<ItemManager>.Instance.currentItems;
		foreach (SpawnableSO spawnableSO in currentItems)
		{
			GameObject gameObject = Object.Instantiate<GameObject>(spawnableSO.prefab, this.itemSpawnTransform.position, this.itemSpawnTransform.rotation);
			ConsumableItem it = gameObject.GetComponent<ConsumableItem>();
			NetworkSingleton<ItemManager>.Instance.spawnedItemInstances.Add(it);
			NetworkServer.Spawn(gameObject, null);
			yield return null;
			it.ServerThrow(this.itemSpawnTransform.position, this.itemSpawnTransform.rotation, this.itemSpawnTransform.forward * this.throwForce, Random.insideUnitSphere * this.throwTorque);
			UnityEvent unityEvent = this.serverOnSpawnItem;
			if (unityEvent != null)
			{
				unityEvent.Invoke();
			}
			yield return new WaitForSeconds(this.itemSpawnInterval);
			it = null;
		}
		List<SpawnableSO>.Enumerator enumerator = default(List<SpawnableSO>.Enumerator);
		yield return new WaitForSeconds(0.5f);
		this._hasSpawned = true;
		this._isSpawning = false;
		yield break;
		yield break;
	}

	// Token: 0x06000CD7 RID: 3287 RVA: 0x000361C0 File Offset: 0x000343C0
	private IEnumerator ItemSpawnRoutine()
	{
		List<ConsumableItem> spawnedItemInstances = NetworkSingleton<ItemManager>.Instance.spawnedItemInstances;
		foreach (ConsumableItem item in spawnedItemInstances)
		{
			if (!item.GetIsBeingHeld() && !NetworkSingleton<ElevatorManager>.Instance.IsInElevator(item.transform.position))
			{
				item.ServerSetEnabled(true);
				yield return new WaitForFixedUpdate();
				item.ServerThrow(this.itemSpawnTransform.position, this.itemSpawnTransform.rotation, this.itemSpawnTransform.forward * this.throwForce, Random.insideUnitSphere * this.throwTorque);
				UnityEvent unityEvent = this.serverOnSpawnItem;
				if (unityEvent != null)
				{
					unityEvent.Invoke();
				}
				yield return new WaitForSeconds(this.itemSpawnInterval);
				item = null;
			}
		}
		List<ConsumableItem>.Enumerator enumerator = default(List<ConsumableItem>.Enumerator);
		yield return new WaitForSeconds(0.5f);
		this._isSpawning = false;
		yield break;
		yield break;
	}

	// Token: 0x06000CD9 RID: 3289 RVA: 0x00002321 File Offset: 0x00000521
	public override bool Weaved()
	{
		return true;
	}

	// Token: 0x04000848 RID: 2120
	[SerializeField]
	private Transform itemSpawnTransform;

	// Token: 0x04000849 RID: 2121
	[SerializeField]
	private float itemSpawnInterval;

	// Token: 0x0400084A RID: 2122
	[SerializeField]
	private float throwForce;

	// Token: 0x0400084B RID: 2123
	[SerializeField]
	private float throwTorque;

	// Token: 0x0400084C RID: 2124
	[SerializeField]
	private UnityEvent serverOnSpawnItem;

	// Token: 0x0400084D RID: 2125
	[SerializeField]
	private UnityEvent serverOnInteractElevatorGuy;

	// Token: 0x0400084E RID: 2126
	private bool _isSpawning;

	// Token: 0x0400084F RID: 2127
	private bool _hasSpawned;
}
