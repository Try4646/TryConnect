using System;
using System.Collections.Generic;
using Extensions;
using FMODUnity;
using Mirror;
using Mirror.RemoteCalls;
using MoreMountains.Feedbacks;
using UnityEngine;

// Token: 0x02000107 RID: 263
public class MerchantBuyButton : InteractableBase
{
	// Token: 0x06000AE8 RID: 2792 RVA: 0x0002B8A8 File Offset: 0x00029AA8
	private void Awake()
	{
		if (this.buyZone == null)
		{
			this.buyZone = base.GetComponentInParent<MerchantBuyZone>();
		}
		if (this.priceSettings == null)
		{
			this.priceSettings = Resources.Load<ItemPriceSettings>("ItemPriceSettings");
		}
		if (string.IsNullOrEmpty(this.TooltipMessage))
		{
			this.TooltipMessage = "Press [E] to Buy Items";
		}
		if (!this.gameSettings)
		{
			this.gameSettings = Resources.Load<GameSettings>("GameSettings");
		}
	}

	// Token: 0x06000AE9 RID: 2793 RVA: 0x0002B924 File Offset: 0x00029B24
	public override void ServerOnInteract(PlayerInteract playerInteract)
	{
		base.ServerOnInteract(playerInteract);
		if (this.buyZone == null)
		{
			Debug.LogWarning("[MerchantBuyButton] Buy zone not assigned!");
			this.RpcPlayFailFeedback();
			return;
		}
		List<ConsumableItem> itemsInZone = this.buyZone.GetItemsInZone();
		if (itemsInZone == null || itemsInZone.Count == 0)
		{
			Debug.Log("[MerchantBuyButton] No items in buy zone to purchase.");
			this.RpcPlayFailFeedback();
			return;
		}
		int num = this.CalculateTotalCost(itemsInZone);
		if (num <= 0)
		{
			Debug.LogWarning("[MerchantBuyButton] Could not calculate price for items. Price settings may be missing.");
			this.RpcPlayFailFeedback();
			return;
		}
		if (NetworkSingleton<MoneyManager>.Instance == null)
		{
			Debug.LogError("[MerchantBuyButton] MoneyManager instance not found!");
			this.RpcPlayFailFeedback();
			return;
		}
		if (!NetworkSingleton<MoneyManager>.Instance.TryChangeTicketBalance((long)(-(long)num)))
		{
			Debug.Log(string.Format("[MerchantBuyButton] Not enough tickets! Need {0}, have {1}.", num, NetworkSingleton<MoneyManager>.Instance.ticketBalance));
			this.RpcPlayFailFeedback();
			return;
		}
		List<ConsumableItem> list = new List<ConsumableItem>();
		List<GachaSphere> list2 = new List<GachaSphere>();
		foreach (ConsumableItem consumableItem in itemsInZone)
		{
			if (!(consumableItem == null) && !(consumableItem.gameObject == null))
			{
				GachaSphere component = consumableItem.GetComponent<GachaSphere>();
				if (component != null)
				{
					list2.Add(component);
				}
				else
				{
					list.Add(consumableItem);
				}
			}
		}
		foreach (ConsumableItem consumableItem2 in list)
		{
			if (!(consumableItem2 == null) && !(consumableItem2.gameObject == null))
			{
				if (NetworkSingleton<ItemStampManager>.Instance != null)
				{
					NetworkSingleton<ItemStampManager>.Instance.MarkInstancePurchased(consumableItem2.gameObject);
				}
				if (consumableItem2.spawnableSo != null && NetworkSingleton<ItemManager>.Instance != null)
				{
					NetworkSingleton<ItemManager>.Instance.ServerAddItem(consumableItem2.spawnableSo);
				}
				NetworkIdentity networkIdentity;
				if (consumableItem2.TryGetComponent<NetworkIdentity>(out networkIdentity))
				{
					NetworkServer.Destroy(networkIdentity.gameObject);
				}
			}
		}
		foreach (GachaSphere gachaSphere in list2)
		{
			if (!(gachaSphere == null) && !(gachaSphere.gameObject == null))
			{
				int cosmeticId = gachaSphere.CosmeticId;
				if (cosmeticId > 0)
				{
					this.RpcUnlockCosmetic(cosmeticId);
					Debug.Log(string.Format("[MerchantBuyButton] Unlocking cosmetic {0} for all clients.", cosmeticId));
				}
				NetworkIdentity networkIdentity2;
				if (gachaSphere.TryGetComponent<NetworkIdentity>(out networkIdentity2))
				{
					NetworkServer.Destroy(networkIdentity2.gameObject);
				}
			}
		}
		this.buyZone.ClearItemsInZone();
		Debug.Log(string.Format("[MerchantBuyButton] Successfully purchased {0} item(s) for {1} tickets.", itemsInZone.Count, num));
		this.RpcPlaySuccessFeedback(itemsInZone.Count, num);
	}

	// Token: 0x06000AEA RID: 2794 RVA: 0x0002BC00 File Offset: 0x00029E00
	[ClientRpc]
	private void RpcPlaySuccessFeedback(int itemCount, int totalCost)
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		writer.WriteVarInt(itemCount);
		writer.WriteVarInt(totalCost);
		this.SendRPCInternal("System.Void MerchantBuyButton::RpcPlaySuccessFeedback(System.Int32,System.Int32)", 2025179644, writer, 0, true);
		NetworkWriterPool.Return(writer);
	}

	// Token: 0x06000AEB RID: 2795 RVA: 0x0002BC44 File Offset: 0x00029E44
	[ClientRpc]
	private void RpcPlayFailFeedback()
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		this.SendRPCInternal("System.Void MerchantBuyButton::RpcPlayFailFeedback()", 633224113, writer, 0, true);
		NetworkWriterPool.Return(writer);
	}

	// Token: 0x06000AEC RID: 2796 RVA: 0x0002BC74 File Offset: 0x00029E74
	public override void ServerOnHover(PlayerInteract playerInteract)
	{
		base.ServerOnHover(playerInteract);
		if (this.buyZone != null && base.isServer)
		{
			List<ConsumableItem> itemsInZone = this.buyZone.GetItemsInZone();
			int count = itemsInZone.Count;
			if (count > 0)
			{
				int num = this.CalculateTotalCost(itemsInZone);
				string message = string.Format("[E] Buy {0} Item(s) ({1} tickets)", count, num);
				this.RpcUpdateTooltip(message);
				return;
			}
			this.RpcUpdateTooltip("[E] Buy Items");
		}
	}

	// Token: 0x06000AED RID: 2797 RVA: 0x0002BCE8 File Offset: 0x00029EE8
	[Server]
	private int CalculateTotalCost(List<ConsumableItem> items)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Int32 MerchantBuyButton::CalculateTotalCost(System.Collections.Generic.List`1<ConsumableItem>)' called when server was not active");
			return 0;
		}
		if (items == null || items.Count == 0)
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
		foreach (ConsumableItem consumableItem in items)
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

	// Token: 0x06000AEE RID: 2798 RVA: 0x0002BE18 File Offset: 0x0002A018
	[ClientRpc]
	private void RpcUpdateTooltip(string message)
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		writer.WriteString(message);
		this.SendRPCInternal("System.Void MerchantBuyButton::RpcUpdateTooltip(System.String)", -1417971140, writer, 0, true);
		NetworkWriterPool.Return(writer);
	}

	// Token: 0x06000AEF RID: 2799 RVA: 0x0002BE54 File Offset: 0x0002A054
	[ClientRpc]
	private void RpcUnlockCosmetic(int cosmeticId)
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		writer.WriteVarInt(cosmeticId);
		this.SendRPCInternal("System.Void MerchantBuyButton::RpcUnlockCosmetic(System.Int32)", -1621149138, writer, 0, true);
		NetworkWriterPool.Return(writer);
	}

	// Token: 0x06000AF1 RID: 2801 RVA: 0x00002321 File Offset: 0x00000521
	public override bool Weaved()
	{
		return true;
	}

	// Token: 0x06000AF2 RID: 2802 RVA: 0x0002BE8E File Offset: 0x0002A08E
	protected void UserCode_RpcPlaySuccessFeedback__Int32__Int32(int itemCount, int totalCost)
	{
		if (this.purchaseSuccessFeedback != null)
		{
			this.purchaseSuccessFeedback.PlayFeedbacks();
		}
		if (!this.purchaseSuccessSFX.IsNull)
		{
			SFXManager.SFXOneShot(this.purchaseSuccessSFX, base.transform.position);
		}
	}

	// Token: 0x06000AF3 RID: 2803 RVA: 0x0002BECC File Offset: 0x0002A0CC
	protected static void InvokeUserCode_RpcPlaySuccessFeedback__Int32__Int32(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcPlaySuccessFeedback called on server.");
			return;
		}
		((MerchantBuyButton)obj).UserCode_RpcPlaySuccessFeedback__Int32__Int32(reader.ReadVarInt(), reader.ReadVarInt());
	}

	// Token: 0x06000AF4 RID: 2804 RVA: 0x0002BEFB File Offset: 0x0002A0FB
	protected void UserCode_RpcPlayFailFeedback()
	{
		if (this.purchaseFailFeedback != null)
		{
			this.purchaseFailFeedback.PlayFeedbacks();
		}
		if (!this.purchaseFailSFX.IsNull)
		{
			SFXManager.SFXOneShot(this.purchaseFailSFX, base.transform.position);
		}
	}

	// Token: 0x06000AF5 RID: 2805 RVA: 0x0002BF39 File Offset: 0x0002A139
	protected static void InvokeUserCode_RpcPlayFailFeedback(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcPlayFailFeedback called on server.");
			return;
		}
		((MerchantBuyButton)obj).UserCode_RpcPlayFailFeedback();
	}

	// Token: 0x06000AF6 RID: 2806 RVA: 0x0002BF5C File Offset: 0x0002A15C
	protected void UserCode_RpcUpdateTooltip__String(string message)
	{
		this.TooltipMessage = message;
	}

	// Token: 0x06000AF7 RID: 2807 RVA: 0x0002BF65 File Offset: 0x0002A165
	protected static void InvokeUserCode_RpcUpdateTooltip__String(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcUpdateTooltip called on server.");
			return;
		}
		((MerchantBuyButton)obj).UserCode_RpcUpdateTooltip__String(reader.ReadString());
	}

	// Token: 0x06000AF8 RID: 2808 RVA: 0x0002BF90 File Offset: 0x0002A190
	protected void UserCode_RpcUnlockCosmetic__Int32(int cosmeticId)
	{
		if (MonoSingleton<CosmeticsUnlockManager>.Instance != null)
		{
			if (MonoSingleton<CosmeticsUnlockManager>.Instance.UnlockCosmetic(cosmeticId))
			{
				CosmeticData cosmeticById = CosmeticDataManager.GetCosmeticById(cosmeticId);
				string arg = (cosmeticById != null) ? cosmeticById.cosmeticName : string.Format("ID {0}", cosmeticId);
				Debug.Log(string.Format("[MerchantBuyButton] Successfully unlocked cosmetic: {0} (ID: {1})", arg, cosmeticId));
				return;
			}
		}
		else
		{
			Debug.LogError("[MerchantBuyButton] CosmeticsUnlockManager.Instance is null! Cannot unlock cosmetic.");
		}
	}

	// Token: 0x06000AF9 RID: 2809 RVA: 0x0002C001 File Offset: 0x0002A201
	protected static void InvokeUserCode_RpcUnlockCosmetic__Int32(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcUnlockCosmetic called on server.");
			return;
		}
		((MerchantBuyButton)obj).UserCode_RpcUnlockCosmetic__Int32(reader.ReadVarInt());
	}

	// Token: 0x06000AFA RID: 2810 RVA: 0x0002C02C File Offset: 0x0002A22C
	static MerchantBuyButton()
	{
		RemoteProcedureCalls.RegisterRpc(typeof(MerchantBuyButton), "System.Void MerchantBuyButton::RpcPlaySuccessFeedback(System.Int32,System.Int32)", new RemoteCallDelegate(MerchantBuyButton.InvokeUserCode_RpcPlaySuccessFeedback__Int32__Int32));
		RemoteProcedureCalls.RegisterRpc(typeof(MerchantBuyButton), "System.Void MerchantBuyButton::RpcPlayFailFeedback()", new RemoteCallDelegate(MerchantBuyButton.InvokeUserCode_RpcPlayFailFeedback));
		RemoteProcedureCalls.RegisterRpc(typeof(MerchantBuyButton), "System.Void MerchantBuyButton::RpcUpdateTooltip(System.String)", new RemoteCallDelegate(MerchantBuyButton.InvokeUserCode_RpcUpdateTooltip__String));
		RemoteProcedureCalls.RegisterRpc(typeof(MerchantBuyButton), "System.Void MerchantBuyButton::RpcUnlockCosmetic(System.Int32)", new RemoteCallDelegate(MerchantBuyButton.InvokeUserCode_RpcUnlockCosmetic__Int32));
	}

	// Token: 0x040006D5 RID: 1749
	[Header("References")]
	[Tooltip("The buy zone that tracks items for purchase.")]
	[SerializeField]
	private MerchantBuyZone buyZone;

	// Token: 0x040006D6 RID: 1750
	[Tooltip("Item price settings. If null, will load from Resources.")]
	[SerializeField]
	private ItemPriceSettings priceSettings;

	// Token: 0x040006D7 RID: 1751
	[SerializeField]
	private GameSettings gameSettings;

	// Token: 0x040006D8 RID: 1752
	[Header("Feedbacks")]
	[Tooltip("Feedback to play when purchase is successful.")]
	[SerializeField]
	private MMF_Player purchaseSuccessFeedback;

	// Token: 0x040006D9 RID: 1753
	[Tooltip("Feedback to play when purchase fails (not enough tickets or no items).")]
	[SerializeField]
	private MMF_Player purchaseFailFeedback;

	// Token: 0x040006DA RID: 1754
	[Header("SFX")]
	[SerializeField]
	private EventReference purchaseSuccessSFX;

	// Token: 0x040006DB RID: 1755
	[SerializeField]
	private EventReference purchaseFailSFX;
}
