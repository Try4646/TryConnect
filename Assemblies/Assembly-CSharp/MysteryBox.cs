using System;
using System.Collections;
using System.Collections.Generic;
using Extensions;
using FMODUnity;
using Mirror;
using Mirror.RemoteCalls;
using UnityEngine;

// Token: 0x020000F3 RID: 243
public class MysteryBox : ConsumableItem
{
	// Token: 0x060009EB RID: 2539 RVA: 0x00027C40 File Offset: 0x00025E40
	protected override void OnUseItem(bool isPressed)
	{
		if (this._hasBeenUsed)
		{
			return;
		}
		this._hasBeenUsed = true;
		this.anim.SetTrigger("Unbox");
		if (base.isServer)
		{
			base.StartCoroutine(this.UnboxRoutine());
		}
	}

	// Token: 0x060009EC RID: 2540 RVA: 0x00027C77 File Offset: 0x00025E77
	private IEnumerator UnboxRoutine()
	{
		yield return new WaitForSeconds(1.5f);
		if (NetworkSingleton<GameManager>.Instance.state == GameState.Game)
		{
			SpawnableSO randomSpawnableByWeight = this.GetRandomSpawnableByWeight();
			GameObject gameObject = Object.Instantiate<GameObject>(randomSpawnableByWeight.prefab, base.transform.position, Quaternion.identity);
			NetworkServer.Spawn(gameObject, null);
			NetworkSingleton<ItemManager>.Instance.ServerAddItem(randomSpawnableByWeight);
			NetworkSingleton<ItemManager>.Instance.spawnedItemInstances.Add(gameObject.GetComponent<ConsumableItem>());
		}
		else
		{
			GameObject gameObject2 = Object.Instantiate<GameObject>(this.lobbySpawnable.prefab, base.transform.position, Quaternion.identity);
			NetworkServer.Spawn(gameObject2, null);
			if (NetworkSingleton<ItemStampManager>.Instance)
			{
				ItemStamp stampFromInstance = NetworkSingleton<ItemStampManager>.Instance.GetStampFromInstance(base.gameObject);
				NetworkSingleton<ItemStampManager>.Instance.UnregisterSpawnedInstance(base.gameObject);
				NetworkSingleton<ItemStampManager>.Instance.RegisterSpawnedInstance(gameObject2, stampFromInstance);
			}
		}
		this.sfxComponent.RpcPlayOneShotWith3DPos();
		base.DestroyItem();
		yield break;
	}

	// Token: 0x060009ED RID: 2541 RVA: 0x00027C86 File Offset: 0x00025E86
	protected override void OnDropped(PlayerInventory playerInventory)
	{
		base.OnDropped(playerInventory);
		this.RpcOnDropped();
	}

	// Token: 0x060009EE RID: 2542 RVA: 0x00027C98 File Offset: 0x00025E98
	[ClientRpc]
	private void RpcOnDropped()
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		this.SendRPCInternal("System.Void MysteryBox::RpcOnDropped()", -335798415, writer, 0, true);
		NetworkWriterPool.Return(writer);
	}

	// Token: 0x060009EF RID: 2543 RVA: 0x00027CC8 File Offset: 0x00025EC8
	private SpawnableSO GetRandomSpawnableByWeight()
	{
		if (this.spawnableList == null || this.spawnableList.Count == 0)
		{
			return null;
		}
		float num = 0f;
		for (int i = 0; i < this.spawnableList.Count; i++)
		{
			num += this.spawnableList[i].chanceWeight;
		}
		float num2 = (float)this.GetSeededRandom().NextDouble() * num;
		for (int j = 0; j < this.spawnableList.Count; j++)
		{
			num2 -= this.spawnableList[j].chanceWeight;
			if (num2 <= 0f)
			{
				return this.spawnableList[j].spawnable;
			}
		}
		List<SpawnableEntry> list = this.spawnableList;
		return list[list.Count - 1].spawnable;
	}

	// Token: 0x060009F0 RID: 2544 RVA: 0x00027D88 File Offset: 0x00025F88
	private Random GetSeededRandom()
	{
		if (!NetworkSingleton<SeededRandomManager>.Instance || !NetworkSingleton<GameManager>.Instance)
		{
			return new Random(Random.Range(int.MinValue, int.MaxValue));
		}
		long currentSeed = (long)NetworkSingleton<SeededRandomManager>.Instance.CurrentSeed;
		int daysPassed = NetworkSingleton<GameManager>.Instance.daysPassed;
		int mysteryBoxCounter = NetworkSingleton<SeededRandomManager>.Instance.MysteryBoxCounter;
		long num = ((currentSeed * (long)((ulong)-1640531535) + (long)daysPassed) * (long)((ulong)-1640531535) + (long)mysteryBoxCounter) * (long)((ulong)-1640531535) ^ (long)((long)mysteryBoxCounter << 13) ^ (long)(mysteryBoxCounter >> 7);
		long num2 = (num ^ num >> 32) * (long)((ulong)-2048144789);
		long num3 = (num2 ^ num2 >> 16) * (long)((ulong)-1028477379);
		return new Random((int)(num3 ^ num3 >> 13));
	}

	// Token: 0x060009F1 RID: 2545 RVA: 0x00027E2C File Offset: 0x0002602C
	private void PopOpenSfx()
	{
		SFXManager.SFXOneShot(this.popOpenSfx, base.transform.position);
	}

	// Token: 0x060009F3 RID: 2547 RVA: 0x00002321 File Offset: 0x00000521
	public override bool Weaved()
	{
		return true;
	}

	// Token: 0x060009F4 RID: 2548 RVA: 0x00027E44 File Offset: 0x00026044
	protected void UserCode_RpcOnDropped()
	{
		this.anim.Play("Default", 0, 0f);
		this.anim.Update(0f);
	}

	// Token: 0x060009F5 RID: 2549 RVA: 0x00027E6C File Offset: 0x0002606C
	protected static void InvokeUserCode_RpcOnDropped(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcOnDropped called on server.");
			return;
		}
		((MysteryBox)obj).UserCode_RpcOnDropped();
	}

	// Token: 0x060009F6 RID: 2550 RVA: 0x00027E8F File Offset: 0x0002608F
	static MysteryBox()
	{
		RemoteProcedureCalls.RegisterRpc(typeof(MysteryBox), "System.Void MysteryBox::RpcOnDropped()", new RemoteCallDelegate(MysteryBox.InvokeUserCode_RpcOnDropped));
	}

	// Token: 0x0400064C RID: 1612
	[SerializeField]
	private Animator anim;

	// Token: 0x0400064D RID: 1613
	[SerializeField]
	private SpawnableSO lobbySpawnable;

	// Token: 0x0400064E RID: 1614
	[SerializeField]
	private List<SpawnableEntry> spawnableList;

	// Token: 0x0400064F RID: 1615
	[SerializeField]
	private SFXComponent sfxComponent;

	// Token: 0x04000650 RID: 1616
	[SerializeField]
	private EventReference popOpenSfx;

	// Token: 0x04000651 RID: 1617
	private bool _hasBeenUsed;
}
