using System;
using Extensions;
using UnityEngine;

// Token: 0x0200023E RID: 574
public class ConfirmationDialogManager : MonoSingleton<ConfirmationDialogManager>
{
	// Token: 0x060014BF RID: 5311 RVA: 0x00059380 File Offset: 0x00057580
	protected override void OnAwake()
	{
		base.OnAwake();
		if (this.confirmationDialog == null)
		{
			this.confirmationDialog = Object.FindFirstObjectByType<ConfirmationDialog>();
			if (this.confirmationDialog == null)
			{
				Debug.LogWarning("[ConfirmationDialogManager] No ConfirmationDialog found in scene. Please assign one in the inspector or add it to the scene.");
			}
		}
	}

	// Token: 0x060014C0 RID: 5312 RVA: 0x000593B9 File Offset: 0x000575B9
	public void ShowConfirmation(string question, Action onConfirm, Action onCancel = null, string confirmLabel = "Yes", string cancelLabel = "No")
	{
		if (this.confirmationDialog == null)
		{
			Debug.LogError("[ConfirmationDialogManager] Cannot show confirmation dialog - ConfirmationDialog is not assigned!");
			return;
		}
		this.confirmationDialog.Show(question, onConfirm, onCancel, confirmLabel, cancelLabel);
	}

	// Token: 0x060014C1 RID: 5313 RVA: 0x000593E6 File Offset: 0x000575E6
	public void HideConfirmation()
	{
		if (this.confirmationDialog != null)
		{
			this.confirmationDialog.Hide();
		}
	}

	// Token: 0x060014C2 RID: 5314 RVA: 0x00059401 File Offset: 0x00057601
	public bool IsDialogVisible()
	{
		return this.confirmationDialog != null && this.confirmationDialog.gameObject.activeSelf;
	}

	// Token: 0x04000D3E RID: 3390
	[Header("Dialog Reference")]
	[SerializeField]
	private ConfirmationDialog confirmationDialog;
}
