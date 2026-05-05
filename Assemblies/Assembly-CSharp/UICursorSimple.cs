using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

// Token: 0x0200014F RID: 335
public class UICursorSimple : BaseCursor
{
	// Token: 0x1700010C RID: 268
	// (get) Token: 0x06000CC8 RID: 3272 RVA: 0x00035E5C File Offset: 0x0003405C
	public static UICursorSimple Instance
	{
		get
		{
			if (UICursorSimple._instance == null)
			{
				UICursorSimple._instance = Object.FindAnyObjectByType<UICursorSimple>();
				if (UICursorSimple._instance == null)
				{
					UICursorSimple._instance = new GameObject("UICursorSimple").AddComponent<UICursorSimple>();
				}
			}
			return UICursorSimple._instance;
		}
	}

	// Token: 0x06000CC9 RID: 3273 RVA: 0x00035E9B File Offset: 0x0003409B
	protected override void Awake()
	{
		if (UICursorSimple._instance != null && UICursorSimple._instance != this)
		{
			Object.Destroy(base.gameObject);
			return;
		}
		UICursorSimple._instance = this;
		base.Awake();
		this.BuildTagMap();
	}

	// Token: 0x06000CCA RID: 3274 RVA: 0x00035ED5 File Offset: 0x000340D5
	protected override void Start()
	{
		base.Start();
		Cursor.lockState = CursorLockMode.Locked;
		Cursor.visible = false;
		this.ShowCursor(false);
	}

	// Token: 0x06000CCB RID: 3275 RVA: 0x00035EF0 File Offset: 0x000340F0
	protected override void Update()
	{
		base.Update();
		if (!this._cursorShouldBeVisible)
		{
			Cursor.lockState = CursorLockMode.Locked;
			Cursor.visible = false;
		}
		CursorType cursorType = CursorType.Default;
		if (EventSystem.current != null)
		{
			PointerEventData pointerEventData = new PointerEventData(EventSystem.current);
			pointerEventData.position = CursorPointerInput.ScreenPosition;
			List<RaycastResult> list = new List<RaycastResult>();
			EventSystem.current.RaycastAll(pointerEventData, list);
			if (list.Count > 0)
			{
				GameObject gameObject = list[0].gameObject;
				string tagFromHierarchy = this.GetTagFromHierarchy(gameObject);
				CursorType cursorType2;
				if (tagFromHierarchy != null && this.tagMap.TryGetValue(tagFromHierarchy, out cursorType2))
				{
					cursorType = cursorType2;
				}
			}
		}
		this.SetCursorType(cursorType);
	}

	// Token: 0x06000CCC RID: 3276 RVA: 0x00035F92 File Offset: 0x00034192
	public void ShowCursor()
	{
		this._cursorShouldBeVisible = true;
		this.ShowCursor(true);
		Cursor.lockState = CursorLockMode.Confined;
		Cursor.visible = false;
	}

	// Token: 0x06000CCD RID: 3277 RVA: 0x00035FAE File Offset: 0x000341AE
	public void HideCursor()
	{
		this._cursorShouldBeVisible = false;
		this.ShowCursor(false);
		Cursor.lockState = CursorLockMode.Locked;
		Cursor.visible = false;
	}

	// Token: 0x06000CCE RID: 3278 RVA: 0x00035FCC File Offset: 0x000341CC
	private void BuildTagMap()
	{
		this.tagMap = new Dictionary<string, CursorType>();
		foreach (TagCursorMapping tagCursorMapping in this.tagMappings)
		{
			if (!string.IsNullOrEmpty(tagCursorMapping.tag))
			{
				this.tagMap[tagCursorMapping.tag] = tagCursorMapping.cursorType;
			}
		}
	}

	// Token: 0x06000CCF RID: 3279 RVA: 0x00036048 File Offset: 0x00034248
	private string GetTagFromHierarchy(GameObject obj)
	{
		Transform transform = obj.transform;
		while (transform != null)
		{
			if (this.tagMap.ContainsKey(transform.tag))
			{
				return transform.tag;
			}
			transform = transform.parent;
		}
		return null;
	}

	// Token: 0x04000841 RID: 2113
	private static UICursorSimple _instance;

	// Token: 0x04000842 RID: 2114
	[Header("Tag-Based Hover Detection")]
	[SerializeField]
	private List<TagCursorMapping> tagMappings = new List<TagCursorMapping>();

	// Token: 0x04000843 RID: 2115
	private Dictionary<string, CursorType> tagMap;

	// Token: 0x04000844 RID: 2116
	private bool _cursorShouldBeVisible;
}
