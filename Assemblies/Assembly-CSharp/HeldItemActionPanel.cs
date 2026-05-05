using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// Token: 0x0200022E RID: 558
public class HeldItemActionPanel : MonoBehaviour
{
	// Token: 0x06001452 RID: 5202 RVA: 0x000571F5 File Offset: 0x000553F5
	private void OnEnable()
	{
		SceneManager.sceneLoaded += this.OnSceneLoaded;
		if (this.playerInventory)
		{
			this.playerInventory.OnLocalInventoryUpdated += this.OnInventoryUpdated;
		}
	}

	// Token: 0x06001453 RID: 5203 RVA: 0x0005722C File Offset: 0x0005542C
	private void OnDisable()
	{
		SceneManager.sceneLoaded -= this.OnSceneLoaded;
		if (this.playerInventory)
		{
			this.playerInventory.OnLocalInventoryUpdated -= this.OnInventoryUpdated;
		}
	}

	// Token: 0x06001454 RID: 5204 RVA: 0x00057263 File Offset: 0x00055463
	private void Start()
	{
		this.ClearActionParent();
	}

	// Token: 0x06001455 RID: 5205 RVA: 0x0005726C File Offset: 0x0005546C
	public void SetPlayerInventory(PlayerInventory inventory)
	{
		if (this.playerInventory)
		{
			this.playerInventory.OnLocalInventoryUpdated -= this.OnInventoryUpdated;
		}
		this.playerInventory = inventory;
		this.playerInventory.OnLocalInventoryUpdated += this.OnInventoryUpdated;
		this.RefreshActions();
	}

	// Token: 0x06001456 RID: 5206 RVA: 0x000572C4 File Offset: 0x000554C4
	private void ClearActionParent()
	{
		if (this.actionParent != null)
		{
			for (int i = this.actionParent.childCount - 1; i >= 0; i--)
			{
				Object.Destroy(this.actionParent.GetChild(i).gameObject);
			}
			this._activeActionElements.Clear();
		}
	}

	// Token: 0x06001457 RID: 5207 RVA: 0x00057318 File Offset: 0x00055518
	private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
	{
		if (this.playerInventory)
		{
			this.playerInventory.OnLocalInventoryUpdated -= this.OnInventoryUpdated;
		}
		this.playerInventory = null;
		this._currentHeldItem = null;
		this.ClearActionParent();
	}

	// Token: 0x06001458 RID: 5208 RVA: 0x00057352 File Offset: 0x00055552
	private void OnInventoryUpdated()
	{
		this.RefreshActions();
	}

	// Token: 0x06001459 RID: 5209 RVA: 0x0005735C File Offset: 0x0005555C
	private void RefreshActions()
	{
		if (this.playerInventory == null)
		{
			return;
		}
		Item networkholdingItem = this.playerInventory.NetworkholdingItem;
		if (networkholdingItem != null && !networkholdingItem.isInPocket)
		{
			if (networkholdingItem != this._currentHeldItem)
			{
				this._currentHeldItem = networkholdingItem;
				this.UpdateHeldItemActions();
			}
			return;
		}
		this._currentHeldItem = null;
		this.ShowDefaultActions();
	}

	// Token: 0x0600145A RID: 5210 RVA: 0x000573C0 File Offset: 0x000555C0
	private void UpdateHeldItemActions()
	{
		this.ClearActionParent();
		if (this.playerInventory == null || this.playerInventory.NetworkholdingItem == null)
		{
			return;
		}
		Item networkholdingItem = this.playerInventory.NetworkholdingItem;
		if (networkholdingItem == null || networkholdingItem.isInPocket)
		{
			return;
		}
		this.CreateActionElements(networkholdingItem);
	}

	// Token: 0x0600145B RID: 5211 RVA: 0x0005741A File Offset: 0x0005561A
	private void ShowDefaultActions()
	{
		this.ClearActionParent();
		this.CreateActionElement("Middle Click", "Ping", false);
		this.CreateActionElement("R", "Emote Wheel", true);
	}

	// Token: 0x0600145C RID: 5212 RVA: 0x00057444 File Offset: 0x00055644
	private void CreateActionElements(Item item)
	{
		if (this.itemActionPrefab == null)
		{
			return;
		}
		List<ItemAction> itemActions = item.itemActions;
		if (itemActions == null || itemActions.Count == 0)
		{
			return;
		}
		foreach (ItemAction itemAction in itemActions)
		{
			if (!string.IsNullOrEmpty(itemAction.actionName) && !string.IsNullOrEmpty(itemAction.key))
			{
				this.CreateActionElement(itemAction.key, itemAction.actionName, itemAction.isHold);
			}
		}
	}

	// Token: 0x0600145D RID: 5213 RVA: 0x000574E0 File Offset: 0x000556E0
	private void CreateActionElement(string key, string actionText, bool isHold)
	{
		if (this.itemActionPrefab == null)
		{
			return;
		}
		GameObject gameObject = Object.Instantiate<GameObject>(this.itemActionPrefab, this.actionParent);
		gameObject.name = "ItemAction_" + actionText;
		TextMeshProUGUI componentInChildren = gameObject.GetComponentInChildren<TextMeshProUGUI>();
		if (componentInChildren != null && componentInChildren.transform.parent == gameObject.transform)
		{
			componentInChildren.text = " " + actionText;
		}
		Transform transform = gameObject.transform.Find("KeyButton");
		if (transform != null)
		{
			this.SetupKeyButton(transform.gameObject, key, isHold);
		}
		this._activeActionElements.Add(gameObject);
	}

	// Token: 0x0600145E RID: 5214 RVA: 0x0005758C File Offset: 0x0005578C
	private void SetupKeyButton(GameObject keyButton, string key, bool isHold)
	{
		bool flag = key.Length == 1 && char.IsLetter(key[0]);
		TextMeshProUGUI[] componentsInChildren = keyButton.GetComponentsInChildren<TextMeshProUGUI>(true);
		TextMeshProUGUI textMeshProUGUI = null;
		TextMeshProUGUI textMeshProUGUI2 = null;
		foreach (TextMeshProUGUI textMeshProUGUI3 in componentsInChildren)
		{
			if (textMeshProUGUI3.gameObject.name.ToLower().Contains("hold"))
			{
				textMeshProUGUI2 = textMeshProUGUI3;
			}
			else
			{
				textMeshProUGUI = textMeshProUGUI3;
			}
		}
		Image component = keyButton.GetComponent<Image>();
		if (textMeshProUGUI2 != null)
		{
			textMeshProUGUI2.gameObject.SetActive(isHold);
		}
		if (flag)
		{
			if (textMeshProUGUI != null)
			{
				textMeshProUGUI.text = key.ToUpper();
				textMeshProUGUI.gameObject.SetActive(true);
				return;
			}
		}
		else
		{
			if (textMeshProUGUI != null)
			{
				textMeshProUGUI.gameObject.SetActive(false);
			}
			if (component != null)
			{
				component.gameObject.SetActive(true);
				Sprite keySprite = this.GetKeySprite(key);
				if (keySprite != null)
				{
					component.sprite = keySprite;
				}
				component.type = Image.Type.Simple;
				component.preserveAspect = true;
			}
		}
	}

	// Token: 0x0600145F RID: 5215 RVA: 0x00057694 File Offset: 0x00055894
	private Sprite GetKeySprite(string keyName)
	{
		foreach (HeldItemActionPanel.KeyButtonImageData keyButtonImageData in this.keyButtonImages)
		{
			if (keyButtonImageData.keyName.Equals(keyName, StringComparison.OrdinalIgnoreCase))
			{
				return keyButtonImageData.sprite;
			}
		}
		return null;
	}

	// Token: 0x04000CD7 RID: 3287
	[Header("References")]
	[Tooltip("Transform where action buttons will be created")]
	[SerializeField]
	private Transform actionParent;

	// Token: 0x04000CD8 RID: 3288
	[Header("Item Action Prefab")]
	[Tooltip("Prefab that contains both a key button and text element as children")]
	[SerializeField]
	private GameObject itemActionPrefab;

	// Token: 0x04000CD9 RID: 3289
	[Header("Key Button Images")]
	[Tooltip("Dictionary of key names to their corresponding sprites (e.g., 'Left Click' -> sprite)")]
	[SerializeField]
	private List<HeldItemActionPanel.KeyButtonImageData> keyButtonImages = new List<HeldItemActionPanel.KeyButtonImageData>();

	// Token: 0x04000CDA RID: 3290
	[SerializeField]
	private PlayerInventory playerInventory;

	// Token: 0x04000CDB RID: 3291
	[SerializeField]
	private Item _currentHeldItem;

	// Token: 0x04000CDC RID: 3292
	[SerializeField]
	private List<GameObject> _activeActionElements = new List<GameObject>();

	// Token: 0x0200022F RID: 559
	[Serializable]
	private class KeyButtonImageData
	{
		// Token: 0x04000CDD RID: 3293
		public string keyName;

		// Token: 0x04000CDE RID: 3294
		public Sprite sprite;
	}
}
