using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

// Token: 0x0200023B RID: 571
public class ConfirmationDialog : MonoBehaviour
{
	// Token: 0x060014AA RID: 5290 RVA: 0x00058FA4 File Offset: 0x000571A4
	private void Awake()
	{
		if (this.dialogPanel != null)
		{
			this.canvasGroup = this.dialogPanel.GetComponent<CanvasGroup>();
			if (this.canvasGroup == null)
			{
				this.canvasGroup = this.dialogPanel.AddComponent<CanvasGroup>();
			}
		}
		if (this.dialogPanel != null)
		{
			this.dialogPanel.SetActive(false);
		}
	}

	// Token: 0x060014AB RID: 5291 RVA: 0x0005900C File Offset: 0x0005720C
	private void Start()
	{
		if (this.confirmButton != null)
		{
			this.confirmButton.onClick.RemoveAllListeners();
			this.confirmButton.onClick.AddListener(new UnityAction(this.OnConfirmClicked));
		}
		if (this.cancelButton != null)
		{
			this.cancelButton.onClick.RemoveAllListeners();
			this.cancelButton.onClick.AddListener(new UnityAction(this.OnCancelClicked));
		}
	}

	// Token: 0x060014AC RID: 5292 RVA: 0x00059090 File Offset: 0x00057290
	public void Show(string question, Action onConfirm, Action onCancel = null, string confirmLabel = "Yes", string cancelLabel = "No")
	{
		this.onConfirm = onConfirm;
		this.onCancel = onCancel;
		if (this.questionText != null)
		{
			this.questionText.text = question;
		}
		if (this.confirmButtonText != null)
		{
			this.confirmButtonText.text = confirmLabel;
		}
		if (this.cancelButtonText != null)
		{
			this.cancelButtonText.text = cancelLabel;
		}
		if (this.dialogPanel != null)
		{
			this.dialogPanel.SetActive(true);
			base.StartCoroutine(this.AppearCoroutine());
		}
	}

	// Token: 0x060014AD RID: 5293 RVA: 0x00059122 File Offset: 0x00057322
	public void Hide()
	{
		if (this.dialogPanel != null && this.dialogPanel.activeSelf)
		{
			base.StartCoroutine(this.DisappearCoroutine());
		}
	}

	// Token: 0x060014AE RID: 5294 RVA: 0x0005914C File Offset: 0x0005734C
	private void OnConfirmClicked()
	{
		Action action = this.onConfirm;
		if (action != null)
		{
			action();
		}
		this.Hide();
	}

	// Token: 0x060014AF RID: 5295 RVA: 0x00059165 File Offset: 0x00057365
	private void OnCancelClicked()
	{
		Action action = this.onCancel;
		if (action != null)
		{
			action();
		}
		this.Hide();
	}

	// Token: 0x060014B0 RID: 5296 RVA: 0x0005917E File Offset: 0x0005737E
	private IEnumerator AppearCoroutine()
	{
		if (this.canvasGroup == null)
		{
			yield break;
		}
		this.canvasGroup.alpha = 0f;
		float elapsed = 0f;
		while (elapsed < this.appearSpeed)
		{
			elapsed += Time.deltaTime;
			this.canvasGroup.alpha = Mathf.Clamp01(elapsed / this.appearSpeed);
			yield return null;
		}
		this.canvasGroup.alpha = 1f;
		yield break;
	}

	// Token: 0x060014B1 RID: 5297 RVA: 0x0005918D File Offset: 0x0005738D
	private IEnumerator DisappearCoroutine()
	{
		if (this.canvasGroup == null)
		{
			yield break;
		}
		float elapsed = 0f;
		float startAlpha = this.canvasGroup.alpha;
		while (elapsed < this.disappearSpeed)
		{
			elapsed += Time.deltaTime;
			this.canvasGroup.alpha = Mathf.Lerp(startAlpha, 0f, elapsed / this.disappearSpeed);
			yield return null;
		}
		this.canvasGroup.alpha = 0f;
		this.dialogPanel.SetActive(false);
		yield break;
	}

	// Token: 0x04000D2A RID: 3370
	[Header("UI References")]
	[SerializeField]
	private GameObject dialogPanel;

	// Token: 0x04000D2B RID: 3371
	[SerializeField]
	private TextMeshProUGUI questionText;

	// Token: 0x04000D2C RID: 3372
	[SerializeField]
	private Button confirmButton;

	// Token: 0x04000D2D RID: 3373
	[SerializeField]
	private Button cancelButton;

	// Token: 0x04000D2E RID: 3374
	[SerializeField]
	private TextMeshProUGUI confirmButtonText;

	// Token: 0x04000D2F RID: 3375
	[SerializeField]
	private TextMeshProUGUI cancelButtonText;

	// Token: 0x04000D30 RID: 3376
	[Header("Animation Settings")]
	[SerializeField]
	private float appearSpeed = 0.3f;

	// Token: 0x04000D31 RID: 3377
	[SerializeField]
	private float disappearSpeed = 0.3f;

	// Token: 0x04000D32 RID: 3378
	private CanvasGroup canvasGroup;

	// Token: 0x04000D33 RID: 3379
	private Action onConfirm;

	// Token: 0x04000D34 RID: 3380
	private Action onCancel;
}
