using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

// Token: 0x02000239 RID: 569
[RequireComponent(typeof(Button))]
public class BugReportButton : MonoBehaviour
{
	// Token: 0x0600149D RID: 5277 RVA: 0x00058920 File Offset: 0x00056B20
	private void Awake()
	{
		Button component = base.GetComponent<Button>();
		if (component != null)
		{
			component.onClick.RemoveAllListeners();
			component.onClick.AddListener(new UnityAction(this.OpenForm));
		}
	}

	// Token: 0x0600149E RID: 5278 RVA: 0x00058960 File Offset: 0x00056B60
	private void OpenForm()
	{
		if (this.formUI != null)
		{
			this.formUI.OpenForm();
			return;
		}
		BugReportFormUI bugReportFormUI = Object.FindFirstObjectByType<BugReportFormUI>();
		if (bugReportFormUI != null)
		{
			bugReportFormUI.OpenForm();
		}
	}

	// Token: 0x04000D18 RID: 3352
	[SerializeField]
	private BugReportFormUI formUI;
}
