using System;
using System.Collections.Generic;
using FMODUnity;
using UnityEngine;
using UnityEngine.EventSystems;

// Token: 0x0200014E RID: 334
public class UICursor : BaseCursor
{
	// Token: 0x1700010B RID: 267
	// (get) Token: 0x06000CBF RID: 3263 RVA: 0x000358A3 File Offset: 0x00033AA3
	public static UICursor Instance
	{
		get
		{
			if (UICursor._instance == null)
			{
				UICursor._instance = Object.FindAnyObjectByType<UICursor>();
				if (UICursor._instance == null)
				{
					UICursor._instance = new GameObject("UICursor").AddComponent<UICursor>();
				}
			}
			return UICursor._instance;
		}
	}

	// Token: 0x06000CC0 RID: 3264 RVA: 0x000358E4 File Offset: 0x00033AE4
	public void SetInputModeEnabled(bool enabled)
	{
		this.isInputModeEnabled = enabled;
		if (enabled)
		{
			this.ShowCursor(true);
			if (this.currentCursorType == CursorType.Default)
			{
				this.currentCursorType = CursorType.Play;
			}
			this.SetCursorType(CursorType.Default);
			return;
		}
		this.ShowCursor(false);
		if (this.useUiCursor && this.uiCursorImage != null)
		{
			this.uiCursorImage.enabled = false;
			return;
		}
		Cursor.visible = false;
	}

	// Token: 0x06000CC1 RID: 3265 RVA: 0x00035949 File Offset: 0x00033B49
	protected override void Awake()
	{
		if (UICursor._instance != null && UICursor._instance != this)
		{
			Object.Destroy(base.gameObject);
			return;
		}
		UICursor._instance = this;
		base.Awake();
		this.BuildTagMap();
	}

	// Token: 0x06000CC2 RID: 3266 RVA: 0x00035983 File Offset: 0x00033B83
	protected override void Start()
	{
		base.Start();
		if (this.mainCamera == null)
		{
			this.mainCamera = Camera.main;
		}
	}

	// Token: 0x06000CC3 RID: 3267 RVA: 0x000359A4 File Offset: 0x00033BA4
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

	// Token: 0x06000CC4 RID: 3268 RVA: 0x00035A20 File Offset: 0x00033C20
	protected override void Update()
	{
		base.Update();
		if (!this.isInputModeEnabled)
		{
			return;
		}
		CursorType cursorType = CursorType.Default;
		GameObject gameObject = null;
		RaycastHit? raycastHit = null;
		if (EventSystem.current != null)
		{
			PointerEventData pointerEventData = new PointerEventData(EventSystem.current);
			pointerEventData.position = CursorPointerInput.ScreenPosition;
			List<RaycastResult> list = new List<RaycastResult>();
			EventSystem.current.RaycastAll(pointerEventData, list);
			if (list.Count > 0)
			{
				gameObject = list[0].gameObject;
				string tagFromHierarchy = this.GetTagFromHierarchy(gameObject);
				CursorType cursorType2;
				if (tagFromHierarchy != null && this.tagMap.TryGetValue(tagFromHierarchy, out cursorType2))
				{
					cursorType = cursorType2;
				}
			}
		}
		RaycastHit value;
		if (gameObject == null && this.mainCamera != null && Physics.Raycast(this.mainCamera.ScreenPointToRay(CursorPointerInput.ScreenPosition3D), out value, this.raycastDistance))
		{
			gameObject = value.collider.gameObject;
			raycastHit = new RaycastHit?(value);
			string tagFromHierarchy2 = this.GetTagFromHierarchy(gameObject);
			CursorType cursorType3;
			if (tagFromHierarchy2 != null && this.tagMap.TryGetValue(tagFromHierarchy2, out cursorType3))
			{
				cursorType = cursorType3;
			}
		}
		this.SetCursorType(cursorType);
		if (CursorPointerInput.LeftClickPressedThisFrame && gameObject != null)
		{
			string tagFromHierarchy3 = this.GetTagFromHierarchy(gameObject);
			if (tagFromHierarchy3 == "menu_play")
			{
				foreach (MonoBehaviour monoBehaviour in gameObject.transform.root.GetComponents<MonoBehaviour>())
				{
					if (monoBehaviour != null && monoBehaviour.GetType().GetMethod("Play") != null)
					{
						SFXManager.SFXOneShot(this.gunSFX, default(Vector3));
						monoBehaviour.GetType().GetMethod("Play").Invoke(monoBehaviour, null);
						return;
					}
				}
				return;
			}
			if (tagFromHierarchy3 == "menu_hit" && raycastHit != null)
			{
				FloatingObjectPhysics floatingObjectPhysics = gameObject.GetComponent<FloatingObjectPhysics>();
				if (floatingObjectPhysics == null)
				{
					floatingObjectPhysics = gameObject.GetComponentInParent<FloatingObjectPhysics>();
				}
				if (floatingObjectPhysics != null)
				{
					SFXManager.SFXOneShot(this.gunSFX, default(Vector3));
					floatingObjectPhysics.HandleClick(raycastHit.Value, this.mainCamera);
					return;
				}
			}
			else if (tagFromHierarchy3 == null && raycastHit != null && raycastHit.Value.collider != null)
			{
				SFXManager.SFXOneShot(this.gunSFX, default(Vector3));
				this.PlayClickParticle(raycastHit.Value.point, raycastHit.Value.normal);
			}
		}
	}

	// Token: 0x06000CC5 RID: 3269 RVA: 0x00035CA8 File Offset: 0x00033EA8
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

	// Token: 0x06000CC6 RID: 3270 RVA: 0x00035CEC File Offset: 0x00033EEC
	private void PlayClickParticle(Vector3 position, Vector3 normal)
	{
		if (this.defaultClickParticle == null)
		{
			return;
		}
		if (!this.instantiateClickParticles)
		{
			if (!this.defaultClickParticle.gameObject.activeInHierarchy)
			{
				this.defaultClickParticle.gameObject.SetActive(true);
			}
			if (this.defaultClickParticle.isPlaying)
			{
				this.defaultClickParticle.Stop();
				this.defaultClickParticle.Clear();
			}
			this.defaultClickParticle.transform.position = position;
			this.defaultClickParticle.transform.rotation = Quaternion.LookRotation(normal);
			this.defaultClickParticle.Play();
			return;
		}
		ParticleSystem particleSystem = Object.Instantiate<ParticleSystem>(this.defaultClickParticle, position, Quaternion.LookRotation(normal));
		particleSystem.gameObject.SetActive(true);
		ParticleSystem.MainModule main = particleSystem.main;
		main.playOnAwake = false;
		particleSystem.Play();
		if (main.duration > 0f)
		{
			float num = (main.startLifetime.constantMax > 0f) ? main.startLifetime.constantMax : main.startLifetime.constant;
			Object.Destroy(particleSystem.gameObject, main.duration + num + 1f);
			return;
		}
		Object.Destroy(particleSystem.gameObject, 5f);
	}

	// Token: 0x04000838 RID: 2104
	private static UICursor _instance;

	// Token: 0x04000839 RID: 2105
	[Header("Tag-Based Hover Detection")]
	[SerializeField]
	private List<TagCursorMapping> tagMappings = new List<TagCursorMapping>();

	// Token: 0x0400083A RID: 2106
	[SerializeField]
	private float raycastDistance = 100f;

	// Token: 0x0400083B RID: 2107
	[SerializeField]
	private Camera mainCamera;

	// Token: 0x0400083C RID: 2108
	[Header("Click Feedback")]
	[SerializeField]
	private ParticleSystem defaultClickParticle;

	// Token: 0x0400083D RID: 2109
	[Tooltip("If true, will instantiate a new particle system for each click. Otherwise, moves the existing one.")]
	[SerializeField]
	private bool instantiateClickParticles = true;

	// Token: 0x0400083E RID: 2110
	[Header("SFX")]
	[SerializeField]
	private EventReference gunSFX;

	// Token: 0x0400083F RID: 2111
	private Dictionary<string, CursorType> tagMap;

	// Token: 0x04000840 RID: 2112
	private bool isInputModeEnabled = true;
}
