using System;
using UnityEngine;
using UnityEngine.UI;

// Token: 0x0200014A RID: 330
public class SimpleCursorManager : BaseCursorManager
{
	// Token: 0x06000CB7 RID: 3255 RVA: 0x000355E0 File Offset: 0x000337E0
	private void Start()
	{
		if (this.cursorImage == null)
		{
			this.cursorImage = base.GetComponent<Image>();
		}
		if (this.cursorRectTransform == null && this.cursorImage != null)
		{
			this.cursorRectTransform = this.cursorImage.rectTransform;
		}
		this.currentCursorType = BaseCursorManager.CursorType.Default;
		BaseCursorManager.CursorData cursorData = base.GetCursorData(BaseCursorManager.CursorType.Default);
		if (cursorData != null)
		{
			this.ApplyCursorData(cursorData);
			return;
		}
		Debug.LogWarning("No default cursor data found in cursorDataMap!");
	}

	// Token: 0x06000CB8 RID: 3256 RVA: 0x00035658 File Offset: 0x00033858
	private void Update()
	{
		this.UpdateCursorPosition();
	}

	// Token: 0x06000CB9 RID: 3257 RVA: 0x00035660 File Offset: 0x00033860
	private void UpdateCursorPosition()
	{
		if (this.cursorRectTransform == null)
		{
			return;
		}
		Canvas componentInParent = this.cursorRectTransform.GetComponentInParent<Canvas>();
		if (componentInParent == null)
		{
			return;
		}
		Camera cam = (componentInParent.renderMode == RenderMode.ScreenSpaceCamera) ? componentInParent.worldCamera : null;
		Vector2 anchoredPosition;
		if (RectTransformUtility.ScreenPointToLocalPointInRectangle(componentInParent.GetComponent<RectTransform>(), CursorPointerInput.ScreenPosition3D, cam, out anchoredPosition))
		{
			this.cursorRectTransform.anchoredPosition = anchoredPosition;
		}
	}

	// Token: 0x06000CBA RID: 3258 RVA: 0x000356CC File Offset: 0x000338CC
	public override void SetCursorType(BaseCursorManager.CursorType type)
	{
		if (this.currentCursorType == type)
		{
			return;
		}
		this.currentCursorType = type;
		BaseCursorManager.CursorData cursorData = base.GetCursorData(type);
		if (cursorData == null)
		{
			Debug.LogWarning(string.Format("Cursor type {0} not found in SimpleCursorManager. Using default cursor.", type));
			if (this.cursorImage != null)
			{
				this.cursorImage.sprite = null;
			}
			return;
		}
		if (cursorData.sprite == null && cursorData.texture == null)
		{
			Debug.LogWarning(string.Format("Cursor sprite/texture for type {0} is null. Using default cursor.", type));
			if (this.cursorImage != null)
			{
				this.cursorImage.sprite = null;
			}
			return;
		}
		this.ApplyCursorData(cursorData);
	}

	// Token: 0x06000CBB RID: 3259 RVA: 0x00035778 File Offset: 0x00033978
	private void ApplyCursorData(BaseCursorManager.CursorData data)
	{
		if (this.cursorImage == null)
		{
			return;
		}
		if (data.sprite != null)
		{
			this.cursorImage.sprite = data.sprite;
		}
		else if (data.texture != null)
		{
			Vector2 vector = data.hotspot;
			if (vector.x < 0f || vector.x > (float)data.texture.width || vector.y < 0f || vector.y > (float)data.texture.height)
			{
				Debug.LogWarning(string.Format("Hotspot for cursor type {0} is outside texture bounds. Using top-left alignment.", data.type));
				vector = Vector2.zero;
			}
			data.sprite = Sprite.Create(data.texture, new Rect(0f, 0f, (float)data.texture.width, (float)data.texture.height), vector);
			this.cursorImage.sprite = data.sprite;
		}
		this.cursorImage.enabled = true;
	}

	// Token: 0x04000826 RID: 2086
	[Header("Sprite Cursor Settings")]
	[SerializeField]
	private Image cursorImage;

	// Token: 0x04000827 RID: 2087
	[SerializeField]
	private RectTransform cursorRectTransform;
}
