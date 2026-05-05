using System;
using System.Collections.Generic;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using Extensions;
using UnityEngine;
using UnityEngine.UI;

// Token: 0x02000146 RID: 326
public class CursorManager : MonoSingleton<CursorManager>
{
	// Token: 0x06000CA1 RID: 3233 RVA: 0x00034A1F File Offset: 0x00032C1F
	protected override void OnAwake()
	{
		base.OnAwake();
		this.InitializeCursorData();
	}

	// Token: 0x06000CA2 RID: 3234 RVA: 0x00034A30 File Offset: 0x00032C30
	private void Start()
	{
		this.cursorImage = base.GetComponent<Image>();
		this.currentCursorType = CursorManager.CursorType.Default;
		CursorManager.CursorData cursorData;
		if (!this.cursorDataMap.TryGetValue(CursorManager.CursorType.Default, out cursorData))
		{
			Debug.LogWarning("No default cursor data found in cursorDataMap!");
			return;
		}
		if (cursorData.sprite != null && this.cursorImage != null)
		{
			this.cursorImage.sprite = cursorData.sprite;
			return;
		}
		if (cursorData.texture != null && this.cursorImage != null)
		{
			cursorData.sprite = Sprite.Create(cursorData.texture, new Rect(0f, 0f, (float)cursorData.texture.width, (float)cursorData.texture.height), cursorData.hotspot);
			this.cursorImage.sprite = cursorData.sprite;
			return;
		}
		Debug.LogWarning("Default cursor sprite/texture is null or SpriteRenderer not assigned!");
	}

	// Token: 0x06000CA3 RID: 3235 RVA: 0x00034B14 File Offset: 0x00032D14
	private void InitializeCursorData()
	{
		this.cursorDataMap = new Dictionary<CursorManager.CursorType, CursorManager.CursorData>();
		foreach (CursorManager.CursorData cursorData in this.cursorDataList)
		{
			if (!this.cursorDataMap.ContainsKey(cursorData.type))
			{
				if (cursorData.useCenterAlignment && cursorData.texture != null)
				{
					cursorData.hotspot = new Vector2((float)cursorData.texture.width / 2f, (float)cursorData.texture.height / 2f);
				}
				if (cursorData.sprite == null && cursorData.texture != null)
				{
					cursorData.sprite = Sprite.Create(cursorData.texture, new Rect(0f, 0f, (float)cursorData.texture.width, (float)cursorData.texture.height), cursorData.hotspot);
				}
				this.cursorDataMap.Add(cursorData.type, cursorData);
			}
			else
			{
				Debug.LogWarning(string.Format("Duplicate cursor type {0} found in CursorManager. Only the first entry will be used.", cursorData.type));
			}
		}
	}

	// Token: 0x06000CA4 RID: 3236 RVA: 0x00034C54 File Offset: 0x00032E54
	public void SetCursorType(CursorManager.CursorType type)
	{
		if (this.currentCursorType == type)
		{
			return;
		}
		this.currentCursorType = type;
		CursorManager.CursorData cursorData;
		if (!this.cursorDataMap.TryGetValue(type, out cursorData))
		{
			Debug.LogWarning(string.Format("Cursor type {0} not found in CursorManager. Using default cursor.", type));
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
		if (this.cursorImage != null)
		{
			if (cursorData.sprite != null)
			{
				this.cursorImage.sprite = cursorData.sprite;
			}
			else if (cursorData.texture != null)
			{
				Vector2 vector = cursorData.hotspot;
				if (vector.x < 0f || vector.x > (float)cursorData.texture.width || vector.y < 0f || vector.y > (float)cursorData.texture.height)
				{
					Debug.LogWarning(string.Format("Hotspot for cursor type {0} is outside texture bounds. Using top-left alignment.", type));
					vector = Vector2.zero;
				}
				cursorData.sprite = Sprite.Create(cursorData.texture, new Rect(0f, 0f, (float)cursorData.texture.width, (float)cursorData.texture.height), vector);
				this.cursorImage.sprite = cursorData.sprite;
			}
			this.cursorImage.transform.DOScale(1.2f, 0.1f).OnComplete(delegate
			{
				this.cursorImage.transform.DOScale(1f, 0.1f);
			});
			return;
		}
		Debug.LogWarning("SpriteRenderer not assigned to CursorManager!");
	}

	// Token: 0x06000CA5 RID: 3237 RVA: 0x00034E2C File Offset: 0x0003302C
	public void LockCursor(bool isLocked)
	{
		this.isLocked = isLocked;
		Cursor.lockState = (isLocked ? CursorLockMode.Locked : CursorLockMode.None);
	}

	// Token: 0x06000CA6 RID: 3238 RVA: 0x00034E41 File Offset: 0x00033041
	public void ShowCursor(bool isVisible)
	{
		this.isVisible = isVisible;
		Cursor.visible = isVisible;
	}

	// Token: 0x17000108 RID: 264
	// (get) Token: 0x06000CA7 RID: 3239 RVA: 0x00034E50 File Offset: 0x00033050
	public bool IsCursorLocked
	{
		get
		{
			return this.isLocked;
		}
	}

	// Token: 0x17000109 RID: 265
	// (get) Token: 0x06000CA8 RID: 3240 RVA: 0x00034E58 File Offset: 0x00033058
	public bool IsCursorVisible
	{
		get
		{
			return this.isVisible;
		}
	}

	// Token: 0x1700010A RID: 266
	// (get) Token: 0x06000CA9 RID: 3241 RVA: 0x00034E60 File Offset: 0x00033060
	public CursorManager.CursorType CurrentCursorType
	{
		get
		{
			return this.currentCursorType;
		}
	}

	// Token: 0x040007FA RID: 2042
	[Header("Cursor Settings")]
	[SerializeField]
	private List<CursorManager.CursorData> cursorDataList = new List<CursorManager.CursorData>();

	// Token: 0x040007FB RID: 2043
	[Header("Sprite Cursor Settings")]
	[SerializeField]
	private Image cursorImage;

	// Token: 0x040007FC RID: 2044
	private CursorManager.CursorType currentCursorType;

	// Token: 0x040007FD RID: 2045
	private bool isLocked;

	// Token: 0x040007FE RID: 2046
	private bool isVisible;

	// Token: 0x040007FF RID: 2047
	private Dictionary<CursorManager.CursorType, CursorManager.CursorData> cursorDataMap;

	// Token: 0x02000147 RID: 327
	[Serializable]
	public class CursorData
	{
		// Token: 0x04000800 RID: 2048
		public CursorManager.CursorType type;

		// Token: 0x04000801 RID: 2049
		public Texture2D texture;

		// Token: 0x04000802 RID: 2050
		public Sprite sprite;

		// Token: 0x04000803 RID: 2051
		[Tooltip("Leave at 0,0 for top-left alignment. Set to texture dimensions/2 for center alignment")]
		public Vector2 hotspot = Vector2.zero;

		// Token: 0x04000804 RID: 2052
		[Tooltip("If true, hotspot will be set to center of texture")]
		public bool useCenterAlignment;
	}

	// Token: 0x02000148 RID: 328
	public enum CursorType
	{
		// Token: 0x04000806 RID: 2054
		Default,
		// Token: 0x04000807 RID: 2055
		Interact,
		// Token: 0x04000808 RID: 2056
		Forbidden,
		// Token: 0x04000809 RID: 2057
		Speak,
		// Token: 0x0400080A RID: 2058
		Play,
		// Token: 0x0400080B RID: 2059
		Custom,
		// Token: 0x0400080C RID: 2060
		PointUI,
		// Token: 0x0400080D RID: 2061
		Lock,
		// Token: 0x0400080E RID: 2062
		Unlock
	}
}
