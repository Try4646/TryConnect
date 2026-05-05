using System;
using System.Collections.Generic;
using Extensions;
using Mirror;
using Mirror.RemoteCalls;
using TMPro;
using UnityEngine;

// Token: 0x02000108 RID: 264
[RequireComponent(typeof(Collider))]
public class MerchantBuyZone : NetworkBehaviour
{
	// Token: 0x06000AFB RID: 2811 RVA: 0x0002C0BC File Offset: 0x0002A2BC
	private void Awake()
	{
		this.zoneCollider = base.GetComponent<Collider>();
		if (this.zoneCollider != null)
		{
			this.zoneCollider.isTrigger = this.isTrigger;
		}
		if (this.priceSettings == null)
		{
			this.priceSettings = Resources.Load<ItemPriceSettings>("ItemPriceSettings");
		}
		if (base.isServer)
		{
			this.UpdateTotalCostDisplay();
		}
		this.totalCostText.text = "";
		if (!this.gameSettings)
		{
			this.gameSettings = Resources.Load<GameSettings>("GameSettings");
		}
	}

	// Token: 0x06000AFC RID: 2812 RVA: 0x0002C14D File Offset: 0x0002A34D
	private void Update()
	{
		if (base.isServer && Time.time - this.lastUpdateTime >= 0.1f)
		{
			this.UpdateTotalCostDisplay();
			this.lastUpdateTime = Time.time;
		}
	}

	// Token: 0x06000AFD RID: 2813 RVA: 0x0002C17B File Offset: 0x0002A37B
	private bool IsNormalItem(ConsumableItem item)
	{
		return !(item == null) && !(item.GetComponent<GachaSphere>() != null) && item.spawnableSo != null;
	}

	// Token: 0x06000AFE RID: 2814 RVA: 0x0002C1A4 File Offset: 0x0002A3A4
	[Server]
	public List<ConsumableItem> GetItemsInZone()
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Collections.Generic.List`1<ConsumableItem> MerchantBuyZone::GetItemsInZone()' called when server was not active");
			return null;
		}
		List<ConsumableItem> list = new List<ConsumableItem>();
		if (this.zoneCollider == null)
		{
			return list;
		}
		Collider[] array = Physics.OverlapBox(this.zoneCollider.bounds.center, this.zoneCollider.bounds.extents, this.zoneCollider.transform.rotation);
		HashSet<ConsumableItem> hashSet = new HashSet<ConsumableItem>();
		foreach (Collider collider in array)
		{
			if (!(collider == null) && !(collider == this.zoneCollider))
			{
				ConsumableItem component = collider.GetComponent<ConsumableItem>();
				if (component == null && collider.attachedRigidbody != null)
				{
					component = collider.attachedRigidbody.GetComponent<ConsumableItem>();
				}
				if (component != null && !hashSet.Contains(component) && component.IsInteractable)
				{
					hashSet.Add(component);
				}
			}
		}
		list.AddRange(hashSet);
		return list;
	}

	// Token: 0x06000AFF RID: 2815 RVA: 0x0002C2BC File Offset: 0x0002A4BC
	[Server]
	public void ClearItemsInZone()
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void MerchantBuyZone::ClearItemsInZone()' called when server was not active");
			return;
		}
		this.UpdateTotalCostDisplay();
	}

	// Token: 0x06000B00 RID: 2816 RVA: 0x0002C2DC File Offset: 0x0002A4DC
	[Server]
	private void UpdateTotalCostDisplay()
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void MerchantBuyZone::UpdateTotalCostDisplay()' called when server was not active");
			return;
		}
		int totalCost = this.CalculateTotalCost();
		this.RpcUpdateTotalCostDisplay(totalCost);
	}

	// Token: 0x06000B01 RID: 2817 RVA: 0x0002C30C File Offset: 0x0002A50C
	[Server]
	private int CalculateTotalCost()
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Int32 MerchantBuyZone::CalculateTotalCost()' called when server was not active");
			return 0;
		}
		List<ConsumableItem> itemsInZone = this.GetItemsInZone();
		if (itemsInZone == null || itemsInZone.Count == 0)
		{
			return 0;
		}
		if (this.priceSettings == null)
		{
			return 0;
		}
		int num = 0;
		int floorIndex = 0;
		if (NetworkSingleton<GameManager>.Instance != null)
		{
			floorIndex = this.gameSettings.DayToFloor(NetworkSingleton<GameManager>.Instance.daysPassed - 1);
		}
		foreach (ConsumableItem consumableItem in itemsInZone)
		{
			if (!(consumableItem == null))
			{
				GachaSphere component = consumableItem.GetComponent<GachaSphere>();
				if (component != null)
				{
					CosmeticRarity rarity = CosmeticRarity.Common;
					CosmeticData cosmeticById = CosmeticDataManager.GetCosmeticById(component.CosmeticId);
					if (cosmeticById != null)
					{
						rarity = cosmeticById.rarity;
					}
					num += this.priceSettings.CalculateCosmeticPrice(rarity, floorIndex);
				}
				else if (!(consumableItem.spawnableSo == null))
				{
					int num2 = this.priceSettings.CalculatePrice(consumableItem.spawnableSo, floorIndex);
					num += num2;
				}
			}
		}
		return num;
	}

	// Token: 0x06000B02 RID: 2818 RVA: 0x0002C448 File Offset: 0x0002A648
	[ClientRpc]
	private void RpcUpdateTotalCostDisplay(int totalCost)
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		writer.WriteVarInt(totalCost);
		this.SendRPCInternal("System.Void MerchantBuyZone::RpcUpdateTotalCostDisplay(System.Int32)", 1527735829, writer, 0, true);
		NetworkWriterPool.Return(writer);
	}

	// Token: 0x06000B04 RID: 2820 RVA: 0x00002321 File Offset: 0x00000521
	public override bool Weaved()
	{
		return true;
	}

	// Token: 0x06000B05 RID: 2821 RVA: 0x0002C494 File Offset: 0x0002A694
	protected void UserCode_RpcUpdateTotalCostDisplay__Int32(int totalCost)
	{
		if (this.totalCostText != null)
		{
			if (totalCost > 0)
			{
				this.totalCostText.text = string.Format("{0} Tickets", totalCost);
				this.totalCostText.gameObject.SetActive(true);
				return;
			}
			this.totalCostText.text = "";
			this.totalCostText.gameObject.SetActive(false);
		}
	}

	// Token: 0x06000B06 RID: 2822 RVA: 0x0002C501 File Offset: 0x0002A701
	protected static void InvokeUserCode_RpcUpdateTotalCostDisplay__Int32(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcUpdateTotalCostDisplay called on server.");
			return;
		}
		((MerchantBuyZone)obj).UserCode_RpcUpdateTotalCostDisplay__Int32(reader.ReadVarInt());
	}

	// Token: 0x06000B07 RID: 2823 RVA: 0x0002C52A File Offset: 0x0002A72A
	static MerchantBuyZone()
	{
		RemoteProcedureCalls.RegisterRpc(typeof(MerchantBuyZone), "System.Void MerchantBuyZone::RpcUpdateTotalCostDisplay(System.Int32)", new RemoteCallDelegate(MerchantBuyZone.InvokeUserCode_RpcUpdateTotalCostDisplay__Int32));
	}

	// Token: 0x040006DC RID: 1756
	[Header("Settings")]
	[Tooltip("Should the collider be a trigger? (Automatically set on Awake)")]
	[SerializeField]
	private bool isTrigger = true;

	// Token: 0x040006DD RID: 1757
	[Header("Total Cost Display")]
	[Tooltip("TextMeshPro 3D component to display the total cost in tickets.")]
	[SerializeField]
	private TextMeshPro totalCostText;

	// Token: 0x040006DE RID: 1758
	[Tooltip("Item price settings. If null, will load from Resources.")]
	[SerializeField]
	private ItemPriceSettings priceSettings;

	// Token: 0x040006DF RID: 1759
	[SerializeField]
	private GameSettings gameSettings;

	// Token: 0x040006E0 RID: 1760
	private const float UPDATE_INTERVAL = 0.1f;

	// Token: 0x040006E1 RID: 1761
	private float lastUpdateTime;

	// Token: 0x040006E2 RID: 1762
	private Collider zoneCollider;
}
