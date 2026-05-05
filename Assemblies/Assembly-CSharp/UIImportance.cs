using System;
using Extensions;
using UnityEngine;
using UnityEngine.UI;

// Token: 0x02000177 RID: 375
[RequireComponent(typeof(Selectable))]
public class UIImportance : MonoBehaviour
{
	// Token: 0x17000147 RID: 327
	// (get) Token: 0x06000E39 RID: 3641 RVA: 0x0003AF33 File Offset: 0x00039133
	public int Importance
	{
		get
		{
			return this.importance;
		}
	}

	// Token: 0x17000148 RID: 328
	// (get) Token: 0x06000E3A RID: 3642 RVA: 0x0003AF3B File Offset: 0x0003913B
	public Selectable Selectable
	{
		get
		{
			return this.selectable;
		}
	}

	// Token: 0x06000E3B RID: 3643 RVA: 0x0003AF43 File Offset: 0x00039143
	private void Awake()
	{
		this.selectable = base.GetComponent<Selectable>();
	}

	// Token: 0x06000E3C RID: 3644 RVA: 0x0003AF51 File Offset: 0x00039151
	private void OnEnable()
	{
		if (MonoSingleton<InputModeManager>.Instance != null)
		{
			MonoSingleton<InputModeManager>.Instance.OnUIImportanceEnabled(this);
		}
	}

	// Token: 0x06000E3D RID: 3645 RVA: 0x0003AF6B File Offset: 0x0003916B
	private void OnDisable()
	{
		if (MonoSingleton<InputModeManager>.Instance != null)
		{
			MonoSingleton<InputModeManager>.Instance.OnUIImportanceDisabled(this);
		}
	}

	// Token: 0x06000E3E RID: 3646 RVA: 0x0003AF88 File Offset: 0x00039188
	public bool IsVisibleAndEnabled()
	{
		if (this.selectable == null)
		{
			return false;
		}
		if (!this.selectable.IsInteractable())
		{
			return false;
		}
		if (!base.gameObject.activeInHierarchy)
		{
			return false;
		}
		if (this.canvasGroup != null)
		{
			if (!this.canvasGroup.interactable || this.canvasGroup.alpha <= 0f)
			{
				return false;
			}
		}
		else
		{
			Transform transform = base.transform;
			while (transform != null)
			{
				CanvasGroup component = transform.GetComponent<CanvasGroup>();
				if (component != null && (!component.interactable || component.alpha <= 0f))
				{
					return false;
				}
				transform = transform.parent;
			}
		}
		return true;
	}

	// Token: 0x04000908 RID: 2312
	[Header("Importance Settings")]
	[Tooltip("Higher value = more important. The most important active button will be auto-selected.")]
	[SerializeField]
	private int importance;

	// Token: 0x04000909 RID: 2313
	[Header("Visibility Check")]
	[Tooltip("Optional: Directly assign CanvasGroup to check. If not assigned, will search parent hierarchy.")]
	[SerializeField]
	private CanvasGroup canvasGroup;

	// Token: 0x0400090A RID: 2314
	private Selectable selectable;
}
