using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// Token: 0x02000142 RID: 322
public abstract class BaseCursor : MonoBehaviour
{
	// Token: 0x06000C8A RID: 3210 RVA: 0x00034370 File Offset: 0x00032570
	protected virtual void Awake()
	{
		this.InitializeCursorData();
	}

	// Token: 0x06000C8B RID: 3211 RVA: 0x00034378 File Offset: 0x00032578
	protected virtual void Start()
	{
		if (this.useUiCursor && this.uiCursorCanvas != null)
		{
			this.uiCursorCanvasRect = this.uiCursorCanvas.GetComponent<RectTransform>();
		}
		if (this.useUiCursor)
		{
			Cursor.visible = false;
			Cursor.lockState = CursorLockMode.None;
		}
	}

	// Token: 0x06000C8C RID: 3212 RVA: 0x000343B5 File Offset: 0x000325B5
	protected virtual void Update()
	{
		if (this.useUiCursor)
		{
			this.UpdateUiCursorPosition();
		}
	}

	// Token: 0x06000C8D RID: 3213 RVA: 0x000343C8 File Offset: 0x000325C8
	protected virtual void InitializeCursorData()
	{
		this.cursorDataMap = new Dictionary<CursorType, CursorData>();
		this.cursorSpriteCache = new Dictionary<CursorType, Sprite>();
		foreach (CursorData cursorData in this.cursorDataList)
		{
			if (!this.cursorDataMap.ContainsKey(cursorData.type))
			{
				if (cursorData.useCenterAlignment && cursorData.texture != null)
				{
					cursorData.hotspot = new Vector2((float)cursorData.texture.width / 2f, (float)cursorData.texture.height / 2f);
				}
				this.cursorDataMap.Add(cursorData.type, cursorData);
			}
		}
	}

	// Token: 0x06000C8E RID: 3214 RVA: 0x00034494 File Offset: 0x00032694
	public virtual void SetCursorType(CursorType type)
	{
		if (this.currentCursorType == type)
		{
			return;
		}
		this.currentCursorType = type;
		CursorData cursorData;
		if (this.cursorDataMap.TryGetValue(type, out cursorData))
		{
			if (cursorData.texture != null)
			{
				if (this.useUiCursor && this.uiCursorImage != null && this.uiCursorCanvas != null)
				{
					this.ApplyUiCursor(cursorData);
					return;
				}
				Vector2 vector = cursorData.hotspot;
				if (vector.x < 0f || vector.x > (float)cursorData.texture.width || vector.y < 0f || vector.y > (float)cursorData.texture.height)
				{
					vector = Vector2.zero;
				}
				Cursor.SetCursor(cursorData.texture, vector, CursorMode.Auto);
				return;
			}
			else
			{
				if (this.useUiCursor && this.uiCursorImage != null)
				{
					this.uiCursorImage.enabled = false;
					return;
				}
				Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
			}
		}
	}

	// Token: 0x06000C8F RID: 3215 RVA: 0x0003458E File Offset: 0x0003278E
	public virtual void LockCursor(bool isLocked)
	{
		Cursor.lockState = (isLocked ? CursorLockMode.Locked : CursorLockMode.None);
	}

	// Token: 0x06000C90 RID: 3216 RVA: 0x0003459C File Offset: 0x0003279C
	public virtual void ShowCursor(bool isVisible)
	{
		if (this.useUiCursor && this.uiCursorImage != null)
		{
			this.uiCursorImage.enabled = isVisible;
			Cursor.visible = false;
			return;
		}
		Cursor.visible = isVisible;
	}

	// Token: 0x06000C91 RID: 3217 RVA: 0x000345D0 File Offset: 0x000327D0
	protected virtual void ApplyUiCursor(CursorData data)
	{
		if (this.uiCursorImage == null || this.uiCursorCanvas == null)
		{
			return;
		}
		this.uiCursorImage.enabled = true;
		Cursor.visible = false;
		Sprite cursorSprite = this.GetCursorSprite(data);
		this.uiCursorImage.sprite = cursorSprite;
		Vector2 sizeDelta = new Vector2((float)data.texture.width, (float)data.texture.height) * this.uiCursorScale;
		RectTransform rectTransform = this.uiCursorImage.rectTransform;
		rectTransform.sizeDelta = sizeDelta;
		Vector2 vector = data.hotspot;
		if (vector.x < 0f || vector.x > (float)data.texture.width || vector.y < 0f || vector.y > (float)data.texture.height)
		{
			vector = Vector2.zero;
		}
		rectTransform.pivot = new Vector2((data.texture.width > 0) ? (vector.x / (float)data.texture.width) : 0f, (data.texture.height > 0) ? (1f - vector.y / (float)data.texture.height) : 1f);
	}

	// Token: 0x06000C92 RID: 3218 RVA: 0x0003470C File Offset: 0x0003290C
	protected virtual Sprite GetCursorSprite(CursorData data)
	{
		Sprite sprite;
		if (this.cursorSpriteCache.TryGetValue(data.type, out sprite) && sprite != null)
		{
			return sprite;
		}
		Rect rect = new Rect(0f, 0f, (float)data.texture.width, (float)data.texture.height);
		Sprite sprite2 = Sprite.Create(data.texture, rect, new Vector2(0.5f, 0.5f), 100f);
		this.cursorSpriteCache[data.type] = sprite2;
		return sprite2;
	}

	// Token: 0x06000C93 RID: 3219 RVA: 0x00034798 File Offset: 0x00032998
	protected virtual void UpdateUiCursorPosition()
	{
		if (this.uiCursorImage == null || this.uiCursorCanvas == null)
		{
			return;
		}
		if (this.uiCursorCanvasRect == null)
		{
			this.uiCursorCanvasRect = this.uiCursorCanvas.GetComponent<RectTransform>();
		}
		Camera cam = (this.uiCursorCanvas.renderMode == RenderMode.ScreenSpaceCamera) ? this.uiCursorCanvas.worldCamera : null;
		Vector2 anchoredPosition;
		if (RectTransformUtility.ScreenPointToLocalPointInRectangle(this.uiCursorCanvasRect, CursorPointerInput.ScreenPosition3D, cam, out anchoredPosition))
		{
			this.uiCursorImage.rectTransform.anchoredPosition = anchoredPosition;
		}
	}

	// Token: 0x17000104 RID: 260
	// (get) Token: 0x06000C94 RID: 3220 RVA: 0x00034829 File Offset: 0x00032A29
	public CursorType CurrentCursorType
	{
		get
		{
			return this.currentCursorType;
		}
	}

	// Token: 0x040007DD RID: 2013
	[Header("Cursor Visuals")]
	[SerializeField]
	protected List<CursorData> cursorDataList = new List<CursorData>();

	// Token: 0x040007DE RID: 2014
	[Header("UI Cursor (Software)")]
	[SerializeField]
	protected bool useUiCursor;

	// Token: 0x040007DF RID: 2015
	[Tooltip("Canvas hosting the cursor image. Must be Screen Space - Camera.")]
	[SerializeField]
	protected Canvas uiCursorCanvas;

	// Token: 0x040007E0 RID: 2016
	[Tooltip("UI Image used as the cursor.")]
	[SerializeField]
	protected Image uiCursorImage;

	// Token: 0x040007E1 RID: 2017
	[SerializeField]
	protected float uiCursorScale = 1f;

	// Token: 0x040007E2 RID: 2018
	protected CursorType currentCursorType;

	// Token: 0x040007E3 RID: 2019
	protected Dictionary<CursorType, CursorData> cursorDataMap;

	// Token: 0x040007E4 RID: 2020
	protected Dictionary<CursorType, Sprite> cursorSpriteCache;

	// Token: 0x040007E5 RID: 2021
	protected RectTransform uiCursorCanvasRect;
}
