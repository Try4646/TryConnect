using System;
using System.Collections.Generic;
using Extensions;
using TMPro;
using UnityEngine;

// Token: 0x020000D5 RID: 213
public class ItemPriceDisplayManager : MonoBehaviour
{
	// Token: 0x06000859 RID: 2137 RVA: 0x0002180C File Offset: 0x0001FA0C
	private void Awake()
	{
		if (this.priceSettings == null)
		{
			this.priceSettings = Resources.Load<ItemPriceSettings>("ItemPriceSettings");
		}
		if (!this.gameSettings)
		{
			this.gameSettings = Resources.Load<GameSettings>("GameSettings");
		}
	}

	// Token: 0x0600085A RID: 2138 RVA: 0x0002184C File Offset: 0x0001FA4C
	private void Update()
	{
		List<Item> list = new List<Item>();
		foreach (KeyValuePair<Item, GameObject> keyValuePair in this._activePriceDisplays)
		{
			Item key = keyValuePair.Key;
			GameObject value = keyValuePair.Value;
			if (key == null || key.gameObject == null || value == null)
			{
				list.Add(key);
			}
			else
			{
				Vector3 position = this.CalculateCanvasPosition(key);
				value.transform.position = position;
			}
		}
		foreach (Item key2 in list)
		{
			GameObject gameObject;
			if (this._activePriceDisplays.TryGetValue(key2, out gameObject) && gameObject != null)
			{
				Object.Destroy(gameObject);
			}
			this._activePriceDisplays.Remove(key2);
		}
	}

	// Token: 0x0600085B RID: 2139 RVA: 0x00021958 File Offset: 0x0001FB58
	public void ShowPriceForItem(Item item)
	{
		if (item == null)
		{
			return;
		}
		if (item.GetComponent<GachaSphere>() == null && item.spawnableSo == null)
		{
			return;
		}
		if (this.priceCanvasPrefab == null)
		{
			return;
		}
		if (this._activePriceDisplays.ContainsKey(item))
		{
			return;
		}
		if (this.CalculatePrice(item) == 0)
		{
			return;
		}
		Vector3 position = this.CalculateCanvasPosition(item);
		GameObject gameObject = Object.Instantiate<GameObject>(this.priceCanvasPrefab, position, Quaternion.identity);
		gameObject.transform.localScale = this.canvasScale;
		this.UpdatePriceText(gameObject, item);
		this._activePriceDisplays[item] = gameObject;
	}

	// Token: 0x0600085C RID: 2140 RVA: 0x000219F4 File Offset: 0x0001FBF4
	public void HidePriceForItem(Item item)
	{
		if (item == null)
		{
			return;
		}
		GameObject gameObject;
		if (this._activePriceDisplays.TryGetValue(item, out gameObject))
		{
			if (gameObject != null)
			{
				Object.Destroy(gameObject);
			}
			this._activePriceDisplays.Remove(item);
		}
	}

	// Token: 0x0600085D RID: 2141 RVA: 0x00021A38 File Offset: 0x0001FC38
	private Vector3 CalculateCanvasPosition(Item item)
	{
		if (item == null)
		{
			return Vector3.zero;
		}
		Bounds bounds = default(Bounds);
		bool flag = false;
		Renderer[] components = item.GetComponents<Renderer>();
		Renderer[] componentsInChildren = item.GetComponentsInChildren<Renderer>();
		foreach (Renderer renderer in components)
		{
			if (!(renderer == null) && renderer.enabled && !(renderer is ParticleSystemRenderer) && !(renderer is LineRenderer))
			{
				if (!flag)
				{
					bounds = renderer.bounds;
					flag = true;
				}
				else
				{
					bounds.Encapsulate(renderer.bounds);
				}
			}
		}
		foreach (Renderer renderer2 in componentsInChildren)
		{
			if (!(renderer2 == null) && renderer2.enabled && !(renderer2 is ParticleSystemRenderer) && !(renderer2 is LineRenderer))
			{
				if (!flag)
				{
					bounds = renderer2.bounds;
					flag = true;
				}
				else
				{
					bounds.Encapsulate(renderer2.bounds);
				}
			}
		}
		Collider[] components2 = item.GetComponents<Collider>();
		Collider[] componentsInChildren2 = item.GetComponentsInChildren<Collider>();
		foreach (Collider collider in components2)
		{
			if (!(collider == null) && collider.enabled)
			{
				if (!flag)
				{
					bounds = collider.bounds;
					flag = true;
				}
				else
				{
					bounds.Encapsulate(collider.bounds);
				}
			}
		}
		foreach (Collider collider2 in componentsInChildren2)
		{
			if (!(collider2 == null) && collider2.enabled)
			{
				if (!flag)
				{
					bounds = collider2.bounds;
					flag = true;
				}
				else
				{
					bounds.Encapsulate(collider2.bounds);
				}
			}
		}
		if (!flag)
		{
			return item.transform.position + Vector3.up * this.heightOffset;
		}
		return new Vector3(bounds.center.x, bounds.min.y, bounds.center.z) + Vector3.up * this.heightOffset;
	}

	// Token: 0x0600085E RID: 2142 RVA: 0x00021C30 File Offset: 0x0001FE30
	private void UpdatePriceText(GameObject canvas, Item item)
	{
		if (canvas == null || item == null)
		{
			return;
		}
		if (this.priceSettings == null)
		{
			return;
		}
		TextMeshProUGUI componentInChildren = canvas.GetComponentInChildren<TextMeshProUGUI>(true);
		if (componentInChildren != null)
		{
			componentInChildren.text = this.CalculatePrice(item).ToString();
		}
	}

	// Token: 0x0600085F RID: 2143 RVA: 0x00021C88 File Offset: 0x0001FE88
	private int CalculatePrice(Item item)
	{
		if (this.priceSettings == null || item == null)
		{
			return 0;
		}
		int floorIndex = 0;
		if (NetworkSingleton<GameManager>.Instance != null)
		{
			floorIndex = this.gameSettings.DayToFloor(NetworkSingleton<GameManager>.Instance.daysPassed - 1);
		}
		GachaSphere component = item.GetComponent<GachaSphere>();
		if (component != null)
		{
			CosmeticRarity rarity = CosmeticRarity.Common;
			CosmeticData cosmeticById = CosmeticDataManager.GetCosmeticById(component.CosmeticId);
			if (cosmeticById != null)
			{
				rarity = cosmeticById.rarity;
			}
			return this.priceSettings.CalculateCosmeticPrice(rarity, floorIndex);
		}
		if (item.spawnableSo == null)
		{
			return 0;
		}
		return this.priceSettings.CalculatePrice(item.spawnableSo, floorIndex);
	}

	// Token: 0x06000860 RID: 2144 RVA: 0x00021D34 File Offset: 0x0001FF34
	private void OnDestroy()
	{
		foreach (GameObject gameObject in this._activePriceDisplays.Values)
		{
			if (gameObject != null)
			{
				Object.Destroy(gameObject);
			}
		}
		this._activePriceDisplays.Clear();
	}

	// Token: 0x04000556 RID: 1366
	[Header("References")]
	[Tooltip("The canvas prefab to instantiate above items. Should be a world space canvas.")]
	[SerializeField]
	private GameObject priceCanvasPrefab;

	// Token: 0x04000557 RID: 1367
	[Tooltip("Item price settings. If null, will load from Resources.")]
	[SerializeField]
	private ItemPriceSettings priceSettings;

	// Token: 0x04000558 RID: 1368
	[SerializeField]
	private GameSettings gameSettings;

	// Token: 0x04000559 RID: 1369
	[Header("Display Settings")]
	[Tooltip("Height offset above the item's bounding box where the canvas should be positioned.")]
	[SerializeField]
	private float heightOffset = 0.15f;

	// Token: 0x0400055A RID: 1370
	[Tooltip("Scale for the canvas (typically 0.01 for world space).")]
	[SerializeField]
	private Vector3 canvasScale = new Vector3(0.01f, 0.01f, 0.01f);

	// Token: 0x0400055B RID: 1371
	private readonly Dictionary<Item, GameObject> _activePriceDisplays = new Dictionary<Item, GameObject>();
}
