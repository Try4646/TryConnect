using System;
using System.Collections.Generic;
using Extensions;
using UnityEngine;

// Token: 0x02000143 RID: 323
public abstract class BaseCursorManager : MonoSingleton<BaseCursorManager>
{
	// Token: 0x06000C96 RID: 3222 RVA: 0x0003484F File Offset: 0x00032A4F
	protected override void OnAwake()
	{
		base.OnAwake();
		this.InitializeCursorData();
	}

	// Token: 0x06000C97 RID: 3223 RVA: 0x00034860 File Offset: 0x00032A60
	protected virtual void InitializeCursorData()
	{
		this.cursorDataMap = new Dictionary<BaseCursorManager.CursorType, BaseCursorManager.CursorData>();
		foreach (BaseCursorManager.CursorData cursorData in this.cursorDataList)
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

	// Token: 0x06000C98 RID: 3224
	public abstract void SetCursorType(BaseCursorManager.CursorType type);

	// Token: 0x06000C99 RID: 3225 RVA: 0x000349A0 File Offset: 0x00032BA0
	public virtual void LockCursor(bool isLocked)
	{
		this.isLocked = isLocked;
		Cursor.lockState = (isLocked ? CursorLockMode.Locked : CursorLockMode.None);
	}

	// Token: 0x06000C9A RID: 3226 RVA: 0x000349B5 File Offset: 0x00032BB5
	public virtual void ShowCursor(bool isVisible)
	{
		this.isVisible = isVisible;
		Cursor.visible = isVisible;
	}

	// Token: 0x17000105 RID: 261
	// (get) Token: 0x06000C9B RID: 3227 RVA: 0x000349C4 File Offset: 0x00032BC4
	public bool IsCursorLocked
	{
		get
		{
			return this.isLocked;
		}
	}

	// Token: 0x17000106 RID: 262
	// (get) Token: 0x06000C9C RID: 3228 RVA: 0x000349CC File Offset: 0x00032BCC
	public bool IsCursorVisible
	{
		get
		{
			return this.isVisible;
		}
	}

	// Token: 0x17000107 RID: 263
	// (get) Token: 0x06000C9D RID: 3229 RVA: 0x000349D4 File Offset: 0x00032BD4
	public BaseCursorManager.CursorType CurrentCursorType
	{
		get
		{
			return this.currentCursorType;
		}
	}

	// Token: 0x06000C9E RID: 3230 RVA: 0x000349DC File Offset: 0x00032BDC
	protected BaseCursorManager.CursorData GetCursorData(BaseCursorManager.CursorType type)
	{
		BaseCursorManager.CursorData result;
		this.cursorDataMap.TryGetValue(type, out result);
		return result;
	}

	// Token: 0x040007E6 RID: 2022
	[Header("Cursor Settings")]
	[SerializeField]
	protected List<BaseCursorManager.CursorData> cursorDataList = new List<BaseCursorManager.CursorData>();

	// Token: 0x040007E7 RID: 2023
	protected BaseCursorManager.CursorType currentCursorType;

	// Token: 0x040007E8 RID: 2024
	protected bool isLocked;

	// Token: 0x040007E9 RID: 2025
	protected bool isVisible;

	// Token: 0x040007EA RID: 2026
	protected Dictionary<BaseCursorManager.CursorType, BaseCursorManager.CursorData> cursorDataMap;

	// Token: 0x02000144 RID: 324
	[Serializable]
	public class CursorData
	{
		// Token: 0x040007EB RID: 2027
		public BaseCursorManager.CursorType type;

		// Token: 0x040007EC RID: 2028
		public Texture2D texture;

		// Token: 0x040007ED RID: 2029
		public Sprite sprite;

		// Token: 0x040007EE RID: 2030
		[Tooltip("Leave at 0,0 for top-left alignment. Set to texture dimensions/2 for center alignment")]
		public Vector2 hotspot = Vector2.zero;

		// Token: 0x040007EF RID: 2031
		[Tooltip("If true, hotspot will be set to center of texture")]
		public bool useCenterAlignment;
	}

	// Token: 0x02000145 RID: 325
	public enum CursorType
	{
		// Token: 0x040007F1 RID: 2033
		Default,
		// Token: 0x040007F2 RID: 2034
		Interact,
		// Token: 0x040007F3 RID: 2035
		Forbidden,
		// Token: 0x040007F4 RID: 2036
		Speak,
		// Token: 0x040007F5 RID: 2037
		Play,
		// Token: 0x040007F6 RID: 2038
		Custom,
		// Token: 0x040007F7 RID: 2039
		PointUI,
		// Token: 0x040007F8 RID: 2040
		Lock,
		// Token: 0x040007F9 RID: 2041
		Unlock
	}
}
