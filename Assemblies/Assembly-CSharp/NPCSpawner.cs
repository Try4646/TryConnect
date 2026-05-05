using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Extensions;
using Mirror;
using Mirror.RemoteCalls;
using UnityEngine;
using UnityEngine.AI;

// Token: 0x020001CF RID: 463
public class NPCSpawner : NetworkSingleton<NPCSpawner>
{
	// Token: 0x06001099 RID: 4249 RVA: 0x000472E8 File Offset: 0x000454E8
	[Server]
	private int GetCosmeticPresetIndexForFloor(int floorIndex)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Int32 NPCSpawner::GetCosmeticPresetIndexForFloor(System.Int32)' called when server was not active");
			return 0;
		}
		int result;
		switch ((floorIndex >= 1 && floorIndex <= 4) ? floorIndex : (floorIndex + 1))
		{
		case 1:
			result = Random.Range(6, 14);
			break;
		case 2:
			result = Random.Range(10, 17);
			break;
		case 3:
			result = Random.Range(18, 23);
			break;
		case 4:
			result = Random.Range(24, 28);
			break;
		default:
			result = Random.Range(6, 14);
			break;
		}
		return result;
	}

	// Token: 0x0600109A RID: 4250 RVA: 0x00047378 File Offset: 0x00045578
	[Server]
	private void SetupNPC(NPC npc, Transform npcHolder, int floorIndex, int presetIndex, bool delayClientSync)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void NPCSpawner::SetupNPC(NPC,UnityEngine.Transform,System.Int32,System.Int32,System.Boolean)' called when server was not active");
			return;
		}
		npc.transform.SetParent(npcHolder);
		NPCCosmeticSelector component = npc.GetComponent<NPCCosmeticSelector>();
		if (component != null)
		{
			component.SetSelectedPresetIndex(presetIndex);
		}
		if (delayClientSync)
		{
			base.StartCoroutine(this.SyncNPCSetupAfterDelay(npc.netId, floorIndex, presetIndex));
			return;
		}
		this.RpcSetupNPC(npc.netId, floorIndex, presetIndex);
	}

	// Token: 0x0600109B RID: 4251 RVA: 0x000473E8 File Offset: 0x000455E8
	[Server]
	private bool TryGetRandomNavMeshPosition(out Vector3 position, Vector3? fallbackCenter = null)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Boolean NPCSpawner::TryGetRandomNavMeshPosition(UnityEngine.Vector3&,System.Nullable`1<UnityEngine.Vector3>)' called when server was not active");
			position = default(Vector3);
			return default(bool);
		}
		position = Vector3.zero;
		NavMeshTriangulation navMeshTriangulation = NavMesh.CalculateTriangulation();
		if (navMeshTriangulation.vertices != null && navMeshTriangulation.vertices.Length >= 3 && navMeshTriangulation.indices != null && navMeshTriangulation.indices.Length >= 3)
		{
			int maxExclusive = navMeshTriangulation.indices.Length / 3;
			int num = Random.Range(0, maxExclusive);
			int num2 = navMeshTriangulation.indices[num * 3];
			int num3 = navMeshTriangulation.indices[num * 3 + 1];
			int num4 = navMeshTriangulation.indices[num * 3 + 2];
			float num5 = Random.value;
			float num6 = Random.value;
			if (num5 + num6 > 1f)
			{
				num5 = 1f - num5;
				num6 = 1f - num6;
			}
			position = num5 * navMeshTriangulation.vertices[num2] + num6 * navMeshTriangulation.vertices[num3] + (1f - num5 - num6) * navMeshTriangulation.vertices[num4];
			return true;
		}
		Vector3 a = fallbackCenter ?? Vector3.zero;
		for (int i = 0; i < 30; i++)
		{
			Vector2 vector = Random.insideUnitCircle * 25f;
			NavMeshHit navMeshHit;
			if (NavMesh.SamplePosition(a + new Vector3(vector.x, 0f, vector.y), out navMeshHit, 15f, -1))
			{
				position = navMeshHit.position;
				return true;
			}
		}
		return false;
	}

	// Token: 0x0600109C RID: 4252 RVA: 0x000475A8 File Offset: 0x000457A8
	[Server]
	public void DestroyAllNPCs()
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void NPCSpawner::DestroyAllNPCs()' called when server was not active");
			return;
		}
		NPCHolder[] array = Object.FindObjectsByType<NPCHolder>(FindObjectsInactive.Include, FindObjectsSortMode.None);
		List<NPC> list = new List<NPC>();
		foreach (NPCHolder npcholder in array)
		{
			list.AddRange(npcholder.GetComponentsInChildren<NPC>(true));
		}
		foreach (NPC npc in list)
		{
			if (NetworkSingleton<NPCController>.Instance != null)
			{
				NetworkSingleton<NPCController>.Instance.UnregisterNPC(npc);
			}
			this.NPCs.Remove(npc);
			NetworkServer.Destroy(npc.gameObject);
		}
	}

	// Token: 0x0600109D RID: 4253 RVA: 0x00047668 File Offset: 0x00045868
	[Server]
	public IEnumerator SpawnNPCsForFloor(int floorIndex)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Collections.IEnumerator NPCSpawner::SpawnNPCsForFloor(System.Int32)' called when server was not active");
			return null;
		}
		NPCSpawner.<SpawnNPCsForFloor>d__9 <SpawnNPCsForFloor>d__ = new NPCSpawner.<SpawnNPCsForFloor>d__9(0);
		<SpawnNPCsForFloor>d__.<>4__this = this;
		<SpawnNPCsForFloor>d__.floorIndex = floorIndex;
		return <SpawnNPCsForFloor>d__;
	}

	// Token: 0x0600109E RID: 4254 RVA: 0x000476AC File Offset: 0x000458AC
	[Server]
	public IEnumerator SpawnAllCoroutine()
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Collections.IEnumerator NPCSpawner::SpawnAllCoroutine()' called when server was not active");
			return null;
		}
		NPCSpawner.<SpawnAllCoroutine>d__10 <SpawnAllCoroutine>d__ = new NPCSpawner.<SpawnAllCoroutine>d__10(0);
		<SpawnAllCoroutine>d__.<>4__this = this;
		return <SpawnAllCoroutine>d__;
	}

	// Token: 0x0600109F RID: 4255 RVA: 0x000476E7 File Offset: 0x000458E7
	private IEnumerator SyncNPCSetupAfterDelay(uint npcNetId, int floorIndex, int presetIndex)
	{
		yield return new WaitForSeconds(0.1f);
		this.RpcSetupNPC(npcNetId, floorIndex, presetIndex);
		yield break;
	}

	// Token: 0x060010A0 RID: 4256 RVA: 0x0004770C File Offset: 0x0004590C
	[ClientRpc]
	private void RpcSetupNPC(uint npcNetId, int floorIndex, int presetIndex)
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		writer.WriteVarUInt(npcNetId);
		writer.WriteVarInt(floorIndex);
		writer.WriteVarInt(presetIndex);
		this.SendRPCInternal("System.Void NPCSpawner::RpcSetupNPC(System.UInt32,System.Int32,System.Int32)", 646469297, writer, 0, true);
		NetworkWriterPool.Return(writer);
	}

	// Token: 0x060010A1 RID: 4257 RVA: 0x0004775A File Offset: 0x0004595A
	public NPCSpawner()
	{
		base.InitSyncObject(this.NPCs);
	}

	// Token: 0x060010A2 RID: 4258 RVA: 0x00002321 File Offset: 0x00000521
	public override bool Weaved()
	{
		return true;
	}

	// Token: 0x060010A3 RID: 4259 RVA: 0x0004778C File Offset: 0x0004598C
	protected void UserCode_RpcSetupNPC__UInt32__Int32__Int32(uint npcNetId, int floorIndex, int presetIndex)
	{
		NetworkIdentity networkIdentity;
		if (NetworkClient.spawned.TryGetValue(npcNetId, out networkIdentity))
		{
			NPC component = networkIdentity.GetComponent<NPC>();
			if (component != null)
			{
				foreach (CasinoFloor casinoFloor in from f in Object.FindObjectsByType<CasinoFloor>(FindObjectsSortMode.None)
				where f.floorIndex == floorIndex
				select f)
				{
					NPCHolder componentInChildren = casinoFloor.GetComponentInChildren<NPCHolder>();
					if (componentInChildren != null)
					{
						component.transform.SetParent(componentInChildren.transform);
						NPCCosmeticSelector component2 = component.GetComponent<NPCCosmeticSelector>();
						if (component2 != null)
						{
							component2.SetSelectedPresetIndex(presetIndex);
							break;
						}
						break;
					}
				}
			}
		}
	}

	// Token: 0x060010A4 RID: 4260 RVA: 0x00047850 File Offset: 0x00045A50
	protected static void InvokeUserCode_RpcSetupNPC__UInt32__Int32__Int32(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcSetupNPC called on server.");
			return;
		}
		((NPCSpawner)obj).UserCode_RpcSetupNPC__UInt32__Int32__Int32(reader.ReadVarUInt(), reader.ReadVarInt(), reader.ReadVarInt());
	}

	// Token: 0x060010A5 RID: 4261 RVA: 0x00047885 File Offset: 0x00045A85
	static NPCSpawner()
	{
		RemoteProcedureCalls.RegisterRpc(typeof(NPCSpawner), "System.Void NPCSpawner::RpcSetupNPC(System.UInt32,System.Int32,System.Int32)", new RemoteCallDelegate(NPCSpawner.InvokeUserCode_RpcSetupNPC__UInt32__Int32__Int32));
	}

	// Token: 0x04000ABE RID: 2750
	private GameSettings gameSettings;

	// Token: 0x04000ABF RID: 2751
	[Header("Spawn Settings")]
	public GameObject npcPrefab;

	// Token: 0x04000AC0 RID: 2752
	[SerializeField]
	private readonly SyncList<NPC> NPCs = new SyncList<NPC>();

	// Token: 0x04000AC1 RID: 2753
	[Header("Performance Settings")]
	[SerializeField]
	private float maxFrameTime = 16f;

	// Token: 0x04000AC2 RID: 2754
	[SerializeField]
	private int maxSpawnsPerFrame = 3;
}
