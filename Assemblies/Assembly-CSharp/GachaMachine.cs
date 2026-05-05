using System;
using Extensions;
using Mirror;
using MoreMountains.Tools;
using UnityEngine;

// Token: 0x02000105 RID: 261
public class GachaMachine : NetworkBehaviour
{
	// Token: 0x06000AD0 RID: 2768 RVA: 0x0002B22F File Offset: 0x0002942F
	[Server]
	public void ServerBuyGacha(PlayerInteract playerInteract)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void GachaMachine::ServerBuyGacha(PlayerInteract)' called when server was not active");
			return;
		}
		if (!NetworkSingleton<MoneyManager>.Instance.TryChangeTicketBalance((long)(-(long)this.cost)))
		{
			return;
		}
		this.ProcessGacha();
	}

	// Token: 0x06000AD1 RID: 2769 RVA: 0x0002B264 File Offset: 0x00029464
	[Server]
	private void ProcessGacha()
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void GachaMachine::ProcessGacha()' called when server was not active");
			return;
		}
		ScriptableObject loot = this.lootTable.GetLoot();
		Vector3 position = (this.spawnPoint != null) ? this.spawnPoint.position : (base.transform.position + Vector3.up);
		GameObject gameObject = Object.Instantiate<GameObject>(this.spherePrefab.gameObject, position, Quaternion.identity);
		NetworkServer.Spawn(gameObject, null);
		GachaSphere component = gameObject.GetComponent<GachaSphere>();
		if (component != null && loot != null)
		{
			CosmeticData cosmeticData = loot as CosmeticData;
			if (cosmeticData != null)
			{
				component.SetCosmeticId(cosmeticData.cosmeticId);
			}
		}
	}

	// Token: 0x06000AD3 RID: 2771 RVA: 0x00002321 File Offset: 0x00000521
	public override bool Weaved()
	{
		return true;
	}

	// Token: 0x040006C8 RID: 1736
	[Header("References")]
	[SerializeField]
	private Transform spawnPoint;

	// Token: 0x040006C9 RID: 1737
	[SerializeField]
	private GachaSphere spherePrefab;

	// Token: 0x040006CA RID: 1738
	[SerializeField]
	private MMLootTableScriptableObjectSO lootTable;

	// Token: 0x040006CB RID: 1739
	[Header("Settings")]
	[SerializeField]
	private int cost = 1;
}
