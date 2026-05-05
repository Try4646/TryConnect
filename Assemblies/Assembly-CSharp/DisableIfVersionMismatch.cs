using System;
using Extensions;
using UnityEngine;
using UnityEngine.UI;

// Token: 0x02000240 RID: 576
public class DisableIfVersionMismatch : MonoBehaviour
{
	// Token: 0x060014CF RID: 5327 RVA: 0x00059724 File Offset: 0x00057924
	private void Awake()
	{
		this.button = base.GetComponent<Button>();
		if (this.button == null)
		{
			Debug.LogWarning("[DisableIfVersionMismatch] No Button component found on " + base.gameObject.name);
		}
		this.canvasGroup = base.GetComponent<CanvasGroup>();
		if (this.canvasGroup == null)
		{
			Debug.LogWarning("[DisableIfVersionMismatch] No CanvasGroup component found on " + base.gameObject.name);
		}
	}

	// Token: 0x060014D0 RID: 5328 RVA: 0x00059799 File Offset: 0x00057999
	private void OnEnable()
	{
		VersionMismatchManager.OnVersionMismatchChanged += this.OnVersionMismatchChanged;
		if (MonoSingleton<VersionMismatchManager>.Instance != null)
		{
			this.hasVersionMismatch = MonoSingleton<VersionMismatchManager>.Instance.HasVersionMismatch();
		}
		this.ApplyDisableState();
	}

	// Token: 0x060014D1 RID: 5329 RVA: 0x000597CF File Offset: 0x000579CF
	private void OnDisable()
	{
		VersionMismatchManager.OnVersionMismatchChanged -= this.OnVersionMismatchChanged;
	}

	// Token: 0x060014D2 RID: 5330 RVA: 0x000597E2 File Offset: 0x000579E2
	private void OnVersionMismatchChanged(bool hasMismatch)
	{
		this.hasVersionMismatch = hasMismatch;
		this.ApplyDisableState();
	}

	// Token: 0x060014D3 RID: 5331 RVA: 0x000597F4 File Offset: 0x000579F4
	private void ApplyDisableState()
	{
		if (this.hasVersionMismatch)
		{
			if (this.canvasGroup != null)
			{
				this.canvasGroup.alpha = this.disabledAlpha;
				this.canvasGroup.blocksRaycasts = false;
				this.canvasGroup.interactable = false;
			}
			if (this.button != null)
			{
				this.button.interactable = false;
				return;
			}
		}
		else
		{
			if (this.canvasGroup != null)
			{
				this.canvasGroup.alpha = 1f;
				this.canvasGroup.blocksRaycasts = true;
				this.canvasGroup.interactable = true;
			}
			if (this.button != null)
			{
				this.button.interactable = true;
			}
		}
	}

	// Token: 0x04000D44 RID: 3396
	[Header("Settings")]
	[Tooltip("Alpha value to set when there is a version mismatch (0-1)")]
	[SerializeField]
	private float disabledAlpha = 0.5f;

	// Token: 0x04000D45 RID: 3397
	private Button button;

	// Token: 0x04000D46 RID: 3398
	private CanvasGroup canvasGroup;

	// Token: 0x04000D47 RID: 3399
	private bool hasVersionMismatch;
}
