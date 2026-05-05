using System;
using System.Collections.Generic;
using Mirror;
using Mirror.RemoteCalls;
using UnityEngine;

// Token: 0x02000227 RID: 551
public class CreditsMannequinController : NetworkBehaviour
{
	// Token: 0x06001434 RID: 5172 RVA: 0x00056988 File Offset: 0x00054B88
	[ClientRpc]
	public void RpcApplySnapshot(PlayerCreditsSnapshot snapshot)
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		writer.WritePlayerCreditsSnapshot(snapshot);
		this.SendRPCInternal("System.Void CreditsMannequinController::RpcApplySnapshot(PlayerCreditsSnapshot)", 156465597, writer, 0, true);
		NetworkWriterPool.Return(writer);
	}

	// Token: 0x06001435 RID: 5173 RVA: 0x000569C4 File Offset: 0x00054BC4
	public void ApplySnapshot(PlayerCreditsSnapshot snapshot)
	{
		if (snapshot == null)
		{
			return;
		}
		if (this.steamIdComponent != null && snapshot.steamId != 0UL)
		{
			this.steamIdComponent.SetSteamID(snapshot.steamId);
		}
		if (snapshot.cosmetics == null || snapshot.cosmetics.Count == 0)
		{
			return;
		}
		this.ApplyCosmetics(snapshot.cosmetics);
	}

	// Token: 0x06001436 RID: 5174 RVA: 0x00056A20 File Offset: 0x00054C20
	private void ApplyCosmetics(List<PlayerCreditsSnapshot.CosmeticEntry> cosmetics)
	{
		foreach (PlayerCreditsSnapshot.CosmeticEntry cosmeticEntry in cosmetics)
		{
			if (CosmeticDataManager.HasCosmetic(cosmeticEntry.cosmeticId))
			{
				CosmeticData cosmeticById = CosmeticDataManager.GetCosmeticById(cosmeticEntry.cosmeticId);
				MeshFilter meshFilter;
				if (!(cosmeticById == null) && !(cosmeticById.cosmeticModel == null) && cosmeticById.cosmeticModel.TryGetComponent<MeshFilter>(out meshFilter))
				{
					MeshFilter meshFilterForType = this.GetMeshFilterForType(cosmeticById.cosmeticType);
					if (!(meshFilterForType == null))
					{
						meshFilterForType.mesh = meshFilter.sharedMesh;
					}
				}
			}
		}
	}

	// Token: 0x06001437 RID: 5175 RVA: 0x00056ACC File Offset: 0x00054CCC
	private MeshFilter GetMeshFilterForType(CosmeticType type)
	{
		MeshFilter result;
		switch (type)
		{
		case CosmeticType.Hat:
			result = this.hat;
			break;
		case CosmeticType.Hair:
			result = this.hair;
			break;
		case CosmeticType.Mustache:
			result = this.mustache;
			break;
		case CosmeticType.Beard:
			result = this.beard;
			break;
		case CosmeticType.Neckwear:
			result = this.neckwear;
			break;
		case CosmeticType.Clothing:
			result = this.clothing;
			break;
		case CosmeticType.Facewear:
			result = this.facewear;
			break;
		default:
			result = null;
			break;
		}
		return result;
	}

	// Token: 0x06001439 RID: 5177 RVA: 0x00002321 File Offset: 0x00000521
	public override bool Weaved()
	{
		return true;
	}

	// Token: 0x0600143A RID: 5178 RVA: 0x00056B3F File Offset: 0x00054D3F
	protected void UserCode_RpcApplySnapshot__PlayerCreditsSnapshot(PlayerCreditsSnapshot snapshot)
	{
		this.ApplySnapshot(snapshot);
	}

	// Token: 0x0600143B RID: 5179 RVA: 0x00056B48 File Offset: 0x00054D48
	protected static void InvokeUserCode_RpcApplySnapshot__PlayerCreditsSnapshot(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcApplySnapshot called on server.");
			return;
		}
		((CreditsMannequinController)obj).UserCode_RpcApplySnapshot__PlayerCreditsSnapshot(reader.ReadPlayerCreditsSnapshot());
	}

	// Token: 0x0600143C RID: 5180 RVA: 0x00056B71 File Offset: 0x00054D71
	static CreditsMannequinController()
	{
		RemoteProcedureCalls.RegisterRpc(typeof(CreditsMannequinController), "System.Void CreditsMannequinController::RpcApplySnapshot(PlayerCreditsSnapshot)", new RemoteCallDelegate(CreditsMannequinController.InvokeUserCode_RpcApplySnapshot__PlayerCreditsSnapshot));
	}

	// Token: 0x04000CC0 RID: 3264
	[Header("Components")]
	[SerializeField]
	private SteamIdComponent steamIdComponent;

	// Token: 0x04000CC1 RID: 3265
	[Header("Cosmetic Mesh Targets")]
	[SerializeField]
	private MeshFilter hat;

	// Token: 0x04000CC2 RID: 3266
	[SerializeField]
	private MeshFilter hair;

	// Token: 0x04000CC3 RID: 3267
	[SerializeField]
	private MeshFilter mustache;

	// Token: 0x04000CC4 RID: 3268
	[SerializeField]
	private MeshFilter beard;

	// Token: 0x04000CC5 RID: 3269
	[SerializeField]
	private MeshFilter neckwear;

	// Token: 0x04000CC6 RID: 3270
	[SerializeField]
	private MeshFilter clothing;

	// Token: 0x04000CC7 RID: 3271
	[SerializeField]
	private MeshFilter facewear;
}
