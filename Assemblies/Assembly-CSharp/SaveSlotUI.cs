using System;
using Extensions;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

// Token: 0x02000252 RID: 594
public class SaveSlotUI : MonoBehaviour
{
	// Token: 0x0600153F RID: 5439 RVA: 0x0005B08C File Offset: 0x0005928C
	private void Awake()
	{
		if (this.slotButton != null)
		{
			this.slotButton.onClick.AddListener(new UnityAction(this.OnSlotClicked));
		}
		if (this.deleteButton != null)
		{
			this.deleteButton.onClick.AddListener(new UnityAction(this.OnDeleteClicked));
		}
	}

	// Token: 0x06001540 RID: 5440 RVA: 0x0005B0F0 File Offset: 0x000592F0
	public void PopulateSlot(string saveName, SaveData saveData = null)
	{
		this.saveName = saveName;
		this.isEmpty = (saveData == null);
		this.currentSaveData = saveData;
		if (this.isEmpty)
		{
			if (this.titleText != null)
			{
				this.titleText.text = this.emptySlotTitle;
			}
			if (this.buttonText != null)
			{
				this.buttonText.text = this.emptySlotButtonText;
			}
			if (this.moneyText != null)
			{
				this.moneyText.text = "";
			}
			if (this.ticketsText != null)
			{
				this.ticketsText.text = "";
			}
			if (this.quotaText != null)
			{
				this.quotaText.text = "";
			}
			if (this.seedText != null)
			{
				this.seedText.text = "";
			}
			if (this.daysText != null)
			{
				this.daysText.text = "";
			}
			if (this.deleteButton != null)
			{
				this.deleteButton.gameObject.SetActive(false);
				return;
			}
		}
		else
		{
			if (this.titleText != null)
			{
				string text = this.FormatSaveDate(saveData.saveTime);
				this.titleText.text = text;
			}
			if (this.buttonText != null)
			{
				this.buttonText.text = "SELECT";
			}
			if (this.moneyText != null)
			{
				this.moneyText.text = MoneyFormatter.FormatWithDollar(saveData.money);
			}
			if (this.ticketsText != null)
			{
				this.ticketsText.text = saveData.tickets.ToString();
			}
			if (this.quotaText != null)
			{
				this.quotaText.text = MoneyFormatter.FormatWithDollar(saveData.currentQuota);
			}
			if (this.seedText != null)
			{
				this.seedText.text = saveData.seed.ToString();
			}
			if (this.daysText != null)
			{
				this.daysText.text = string.Format("{0}", saveData.daysPassed + 1);
			}
			if (this.deleteButton != null)
			{
				this.deleteButton.gameObject.SetActive(true);
			}
		}
	}

	// Token: 0x06001541 RID: 5441 RVA: 0x0005B33C File Offset: 0x0005953C
	private void OnSlotClicked()
	{
		if (this.isEmpty)
		{
			if (MonoSingleton<LocalSaveManager>.Instance != null)
			{
				string obj = string.Format("Save_{0}", DateTimeOffset.UtcNow.ToUnixTimeSeconds());
				MonoSingleton<LocalSaveManager>.Instance.CreateNewSave(obj);
				Action<string> onSlotSelected = this.OnSlotSelected;
				if (onSlotSelected == null)
				{
					return;
				}
				onSlotSelected(obj);
				return;
			}
		}
		else
		{
			Action<string> onSlotSelected2 = this.OnSlotSelected;
			if (onSlotSelected2 == null)
			{
				return;
			}
			onSlotSelected2(this.saveName);
		}
	}

	// Token: 0x06001542 RID: 5442 RVA: 0x0005B3B0 File Offset: 0x000595B0
	private void OnDeleteClicked()
	{
		if (this.isEmpty || string.IsNullOrEmpty(this.saveName))
		{
			return;
		}
		string saveToDelete = this.saveName;
		string str = "this save";
		if (this.currentSaveData != null)
		{
			str = this.FormatSaveDate(this.currentSaveData.saveTime);
		}
		if (MonoSingleton<ConfirmationDialogManager>.Instance != null)
		{
			MonoSingleton<ConfirmationDialogManager>.Instance.ShowConfirmation("Are you sure you want to delete the save from " + str + "? This action cannot be undone.", delegate
			{
				if (MonoSingleton<LocalSaveManager>.Instance != null)
				{
					MonoSingleton<LocalSaveManager>.Instance.DeleteSave(saveToDelete);
				}
				this.PopulateSlot(saveToDelete, null);
				Action<string> onSaveDeleted2 = this.OnSaveDeleted;
				if (onSaveDeleted2 == null)
				{
					return;
				}
				onSaveDeleted2(saveToDelete);
			}, delegate
			{
			}, "Yes, delete", "Cancel");
			return;
		}
		Debug.LogWarning("[SaveSlotUI] ConfirmationDialogManager not found. Deleting save without confirmation.");
		if (MonoSingleton<LocalSaveManager>.Instance != null)
		{
			MonoSingleton<LocalSaveManager>.Instance.DeleteSave(saveToDelete);
		}
		this.PopulateSlot(saveToDelete, null);
		Action<string> onSaveDeleted = this.OnSaveDeleted;
		if (onSaveDeleted == null)
		{
			return;
		}
		onSaveDeleted(saveToDelete);
	}

	// Token: 0x06001543 RID: 5443 RVA: 0x0005B4B4 File Offset: 0x000596B4
	private string FormatSaveDate(long unixTimestamp)
	{
		if (unixTimestamp <= 0L)
		{
			return "Unknown Date";
		}
		string result;
		try
		{
			result = DateTimeOffset.FromUnixTimeSeconds(unixTimestamp).ToString("dd/MM/yyyy");
		}
		catch
		{
			result = "Invalid Date";
		}
		return result;
	}

	// Token: 0x04000D8C RID: 3468
	[Header("UI References")]
	[SerializeField]
	private TextMeshProUGUI titleText;

	// Token: 0x04000D8D RID: 3469
	[SerializeField]
	private Button slotButton;

	// Token: 0x04000D8E RID: 3470
	[SerializeField]
	private TextMeshProUGUI buttonText;

	// Token: 0x04000D8F RID: 3471
	[SerializeField]
	private Button deleteButton;

	// Token: 0x04000D90 RID: 3472
	[Header("Save Data Display")]
	[SerializeField]
	private TextMeshProUGUI moneyText;

	// Token: 0x04000D91 RID: 3473
	[SerializeField]
	private TextMeshProUGUI ticketsText;

	// Token: 0x04000D92 RID: 3474
	[SerializeField]
	private TextMeshProUGUI quotaText;

	// Token: 0x04000D93 RID: 3475
	[SerializeField]
	private TextMeshProUGUI seedText;

	// Token: 0x04000D94 RID: 3476
	[SerializeField]
	private TextMeshProUGUI daysText;

	// Token: 0x04000D95 RID: 3477
	[Header("Empty Slot Settings")]
	[SerializeField]
	private string emptySlotTitle = "Empty Slot";

	// Token: 0x04000D96 RID: 3478
	[SerializeField]
	private string emptySlotButtonText = "New Game";

	// Token: 0x04000D97 RID: 3479
	private string saveName;

	// Token: 0x04000D98 RID: 3480
	private bool isEmpty;

	// Token: 0x04000D99 RID: 3481
	private SaveData currentSaveData;

	// Token: 0x04000D9A RID: 3482
	public Action<string> OnSlotSelected;

	// Token: 0x04000D9B RID: 3483
	public Action<string> OnSaveDeleted;
}
