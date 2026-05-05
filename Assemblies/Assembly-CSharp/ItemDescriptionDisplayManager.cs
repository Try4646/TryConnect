using System;
using System.Collections.Generic;
using Extensions;
using TMPro;
using UnityEngine;

// Token: 0x020000CE RID: 206
public class ItemDescriptionDisplayManager : MonoBehaviour
{
	// Token: 0x060007F8 RID: 2040 RVA: 0x000200DA File Offset: 0x0001E2DA
	private void Awake()
	{
		if (this.descriptionSettings == null)
		{
			this.descriptionSettings = Resources.Load<ItemDescriptionSettings>("ItemDescriptionSettings");
		}
	}

	// Token: 0x060007F9 RID: 2041 RVA: 0x000200FA File Offset: 0x0001E2FA
	private void Start()
	{
		if (MonoSingleton<LocalManager>.Instance != null)
		{
			this._camera = MonoSingleton<LocalManager>.Instance.mainCamera;
		}
	}

	// Token: 0x060007FA RID: 2042 RVA: 0x0002011C File Offset: 0x0001E31C
	private void LateUpdate()
	{
		List<Item> list = new List<Item>();
		foreach (KeyValuePair<Item, GameObject> keyValuePair in this._activeDescriptionDisplays)
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
			if (this._activeDescriptionDisplays.TryGetValue(key2, out gameObject) && gameObject != null)
			{
				Object.Destroy(gameObject);
			}
			this._activeDescriptionDisplays.Remove(key2);
		}
	}

	// Token: 0x060007FB RID: 2043 RVA: 0x00020228 File Offset: 0x0001E428
	public void ShowDescriptionForItem(Item item)
	{
		if (item == null || item.spawnableSo == null)
		{
			return;
		}
		if (this.descriptionCanvasPrefab == null)
		{
			return;
		}
		if (this._activeDescriptionDisplays.ContainsKey(item))
		{
			return;
		}
		Vector3 position = this.CalculateCanvasPosition(item);
		GameObject gameObject = Object.Instantiate<GameObject>(this.descriptionCanvasPrefab, position, Quaternion.identity);
		gameObject.transform.localScale = this.canvasScale;
		this.UpdateDescriptionText(gameObject, item);
		this._activeDescriptionDisplays[item] = gameObject;
	}

	// Token: 0x060007FC RID: 2044 RVA: 0x000202AC File Offset: 0x0001E4AC
	public void HideDescriptionForItem(Item item)
	{
		if (item == null)
		{
			return;
		}
		GameObject gameObject;
		if (this._activeDescriptionDisplays.TryGetValue(item, out gameObject))
		{
			if (gameObject != null)
			{
				Object.Destroy(gameObject);
			}
			this._activeDescriptionDisplays.Remove(item);
		}
	}

	// Token: 0x060007FD RID: 2045 RVA: 0x000202F0 File Offset: 0x0001E4F0
	private Vector3 CalculateCanvasPosition(Item item)
	{
		if (item == null)
		{
			return Vector3.zero;
		}
		Camera camera = this._camera;
		if (camera == null)
		{
			if (MonoSingleton<LocalManager>.Instance != null)
			{
				camera = MonoSingleton<LocalManager>.Instance.mainCamera;
			}
			if (camera == null)
			{
				camera = Camera.main;
			}
		}
		Vector3 a = Vector3.right;
		Vector3 a2 = Vector3.forward;
		if (camera != null)
		{
			Vector3 normalized = (item.transform.position - camera.transform.position).normalized;
			a = Vector3.Cross(Vector3.up, normalized).normalized;
			if (a.sqrMagnitude < 0.1f)
			{
				a = camera.transform.right;
			}
			a2 = -normalized;
		}
		return item.transform.position + a * this.horizontalOffset + Vector3.up * this.verticalOffset + a2 * this.forwardOffset;
	}

	// Token: 0x060007FE RID: 2046 RVA: 0x000203F8 File Offset: 0x0001E5F8
	private void UpdateDescriptionText(GameObject canvas, Item item)
	{
		if (canvas == null || item == null || item.spawnableSo == null)
		{
			return;
		}
		if (this.descriptionSettings == null)
		{
			return;
		}
		TextMeshProUGUI componentInChildren = canvas.GetComponentInChildren<TextMeshProUGUI>(true);
		if (componentInChildren != null)
		{
			string description = this.descriptionSettings.GetDescription(item.spawnableSo);
			componentInChildren.text = description;
		}
	}

	// Token: 0x060007FF RID: 2047 RVA: 0x00020460 File Offset: 0x0001E660
	private void OnDestroy()
	{
		foreach (GameObject gameObject in this._activeDescriptionDisplays.Values)
		{
			if (gameObject != null)
			{
				Object.Destroy(gameObject);
			}
		}
		this._activeDescriptionDisplays.Clear();
	}

	// Token: 0x04000529 RID: 1321
	[Header("References")]
	[Tooltip("The canvas prefab to instantiate next to items. Should be a world space canvas.")]
	[SerializeField]
	private GameObject descriptionCanvasPrefab;

	// Token: 0x0400052A RID: 1322
	[Tooltip("Item description settings. If null, will load from Resources.")]
	[SerializeField]
	private ItemDescriptionSettings descriptionSettings;

	// Token: 0x0400052B RID: 1323
	[Header("Display Settings")]
	[Tooltip("Horizontal offset to the right of the item where the canvas should be positioned (camera-relative).")]
	[SerializeField]
	private float horizontalOffset = 0.2f;

	// Token: 0x0400052C RID: 1324
	[Tooltip("Vertical offset from the item's transform position.")]
	[SerializeField]
	private float verticalOffset;

	// Token: 0x0400052D RID: 1325
	[Tooltip("Forward/backward offset along the camera's view direction. Positive values move toward camera, negative away.")]
	[SerializeField]
	private float forwardOffset;

	// Token: 0x0400052E RID: 1326
	[Tooltip("Scale for the canvas (typically 0.01 for world space).")]
	[SerializeField]
	private Vector3 canvasScale = new Vector3(0.01f, 0.01f, 0.01f);

	// Token: 0x0400052F RID: 1327
	private readonly Dictionary<Item, GameObject> _activeDescriptionDisplays = new Dictionary<Item, GameObject>();

	// Token: 0x04000530 RID: 1328
	private Camera _camera;
}
