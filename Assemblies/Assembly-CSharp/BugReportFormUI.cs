using System;
using System.Collections.Generic;
using Extensions;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

// Token: 0x0200023A RID: 570
public class BugReportFormUI : MonoBehaviour
{
	// Token: 0x060014A0 RID: 5280 RVA: 0x0005899C File Offset: 0x00056B9C
	private void Awake()
	{
		if (this.apiClient == null)
		{
			this.apiClient = MonoSingleton<BugReportAPIClient>.Instance;
		}
		if (this.formPanel != null)
		{
			this.formPanel.SetActive(false);
		}
		if (this.severityDropdown != null)
		{
			this.severityDropdown.ClearOptions();
			this.severityDropdown.AddOptions(new List<string>(BugReportFormUI.Severities));
		}
		if (this.categoryDropdown != null)
		{
			this.categoryDropdown.ClearOptions();
			this.categoryDropdown.AddOptions(new List<string>(BugReportFormUI.Categories));
		}
		if (this.frequencyDropdown != null)
		{
			this.frequencyDropdown.ClearOptions();
			this.frequencyDropdown.AddOptions(new List<string>(BugReportFormUI.Frequencies));
		}
		if (this.submitButton != null)
		{
			this.submitButton.onClick.AddListener(new UnityAction(this.OnSubmit));
		}
		if (this.cancelButton != null)
		{
			this.cancelButton.onClick.AddListener(new UnityAction(this.CloseForm));
		}
	}

	// Token: 0x060014A1 RID: 5281 RVA: 0x00058ABC File Offset: 0x00056CBC
	public void OpenForm()
	{
		if (this.formPanel != null)
		{
			this.formPanel.SetActive(true);
		}
		this.SetMessage("");
		if (this.loadingIndicator != null)
		{
			this.loadingIndicator.SetActive(false);
		}
	}

	// Token: 0x060014A2 RID: 5282 RVA: 0x00058B08 File Offset: 0x00056D08
	public void CloseForm()
	{
		if (this.formPanel != null)
		{
			this.formPanel.SetActive(false);
		}
	}

	// Token: 0x060014A3 RID: 5283 RVA: 0x00058B24 File Offset: 0x00056D24
	private void SetMessage(string msg)
	{
		if (this.messageText != null)
		{
			this.messageText.text = msg;
		}
	}

	// Token: 0x060014A4 RID: 5284 RVA: 0x00058B40 File Offset: 0x00056D40
	private void OnSubmit()
	{
		string text = (this.titleField != null) ? this.titleField.text.Trim() : "";
		if (string.IsNullOrEmpty(text))
		{
			this.SetMessage("Please enter a title.");
			return;
		}
		BugReportPayload bugReportPayload = new BugReportPayload
		{
			title = text,
			severity = ((this.severityDropdown != null && this.severityDropdown.options.Count > 0) ? BugReportFormUI.Severities[Mathf.Clamp(this.severityDropdown.value, 0, BugReportFormUI.Severities.Length - 1)] : "Minor"),
			category = ((this.categoryDropdown != null && this.categoryDropdown.options.Count > 0) ? BugReportFormUI.Categories[Mathf.Clamp(this.categoryDropdown.value, 0, BugReportFormUI.Categories.Length - 1)] : "Other"),
			whatHappened = ((this.whatHappenedField != null) ? this.whatHappenedField.text.Trim() : ""),
			expected = ((this.expectedField != null) ? this.expectedField.text.Trim() : ""),
			reproSteps = BugReportFormUI.ParseReproSteps((this.reproStepsField != null) ? this.reproStepsField.text : ""),
			frequency = ((this.frequencyDropdown != null && this.frequencyDropdown.options.Count > 0) ? BugReportFormUI.Frequencies[Mathf.Clamp(this.frequencyDropdown.value, 0, BugReportFormUI.Frequencies.Length - 1)] : "Once"),
			canReproduceNow = (this.canReproduceToggle != null && this.canReproduceToggle.isOn)
		};
		if (this.apiClient == null)
		{
			this.SetMessage("Bug report client not configured.");
			return;
		}
		this.apiClient.FillContext(bugReportPayload);
		if (this.loadingIndicator != null)
		{
			this.loadingIndicator.SetActive(true);
		}
		if (this.submitButton != null)
		{
			this.submitButton.interactable = false;
		}
		this.SetMessage("Sending…");
		this.apiClient.SendReport(bugReportPayload, delegate(bool success, string errorMsg, string trelloUrl)
		{
			if (this.loadingIndicator != null)
			{
				this.loadingIndicator.SetActive(false);
			}
			if (this.submitButton != null)
			{
				this.submitButton.interactable = true;
			}
			if (success)
			{
				this.SetMessage(string.IsNullOrEmpty(trelloUrl) ? "Report submitted." : ("Report submitted. Card: " + trelloUrl));
				this.ClearForm();
				return;
			}
			this.SetMessage("Failed: " + (string.IsNullOrEmpty(errorMsg) ? "Unknown error" : errorMsg));
		});
	}

	// Token: 0x060014A5 RID: 5285 RVA: 0x00058DA0 File Offset: 0x00056FA0
	private static string[] ParseReproSteps(string text)
	{
		if (string.IsNullOrWhiteSpace(text))
		{
			return null;
		}
		string[] array = text.Split(new char[]
		{
			'\r',
			'\n'
		}, StringSplitOptions.RemoveEmptyEntries);
		if (array.Length == 0)
		{
			return null;
		}
		return array;
	}

	// Token: 0x060014A6 RID: 5286 RVA: 0x00058DD8 File Offset: 0x00056FD8
	private void ClearForm()
	{
		if (this.titleField != null)
		{
			this.titleField.text = "";
		}
		if (this.whatHappenedField != null)
		{
			this.whatHappenedField.text = "";
		}
		if (this.expectedField != null)
		{
			this.expectedField.text = "";
		}
		if (this.reproStepsField != null)
		{
			this.reproStepsField.text = "";
		}
		if (this.canReproduceToggle != null)
		{
			this.canReproduceToggle.isOn = false;
		}
	}

	// Token: 0x04000D19 RID: 3353
	[Header("Panel")]
	[SerializeField]
	private GameObject formPanel;

	// Token: 0x04000D1A RID: 3354
	[Header("Required fields")]
	[SerializeField]
	private TMP_InputField titleField;

	// Token: 0x04000D1B RID: 3355
	[SerializeField]
	private TMP_Dropdown severityDropdown;

	// Token: 0x04000D1C RID: 3356
	[Header("Optional fields")]
	[SerializeField]
	private TMP_Dropdown categoryDropdown;

	// Token: 0x04000D1D RID: 3357
	[SerializeField]
	private TMP_InputField whatHappenedField;

	// Token: 0x04000D1E RID: 3358
	[SerializeField]
	private TMP_InputField expectedField;

	// Token: 0x04000D1F RID: 3359
	[SerializeField]
	private TMP_InputField reproStepsField;

	// Token: 0x04000D20 RID: 3360
	[SerializeField]
	private TMP_Dropdown frequencyDropdown;

	// Token: 0x04000D21 RID: 3361
	[SerializeField]
	private Toggle canReproduceToggle;

	// Token: 0x04000D22 RID: 3362
	[Header("Actions")]
	[SerializeField]
	private Button submitButton;

	// Token: 0x04000D23 RID: 3363
	[SerializeField]
	private Button cancelButton;

	// Token: 0x04000D24 RID: 3364
	[Header("Feedback")]
	[SerializeField]
	private GameObject loadingIndicator;

	// Token: 0x04000D25 RID: 3365
	[SerializeField]
	private TextMeshProUGUI messageText;

	// Token: 0x04000D26 RID: 3366
	[Header("API")]
	[SerializeField]
	private BugReportAPIClient apiClient;

	// Token: 0x04000D27 RID: 3367
	private static readonly string[] Categories = new string[]
	{
		"Other",
		"Crash",
		"Multiplayer",
		"UI",
		"Performance",
		"Gameplay",
		"Visual",
		"Audio"
	};

	// Token: 0x04000D28 RID: 3368
	private static readonly string[] Severities = new string[]
	{
		"Minor",
		"Major",
		"Blocker"
	};

	// Token: 0x04000D29 RID: 3369
	private static readonly string[] Frequencies = new string[]
	{
		"Once",
		"Sometimes",
		"Always"
	};
}
