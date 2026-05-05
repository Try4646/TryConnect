using System;
using System.Collections;
using System.Collections.Generic;
using Extensions;
using UnityEngine;

// Token: 0x0200024F RID: 591
public class SaveSlotManager : MonoBehaviour
{
	// Token: 0x0600152E RID: 5422 RVA: 0x0005ADAB File Offset: 0x00058FAB
	private void Awake()
	{
		if (this.saveSlotPanel != null)
		{
			this.saveSlotPanel.SetActive(false);
		}
	}

	// Token: 0x0600152F RID: 5423 RVA: 0x0005ADC8 File Offset: 0x00058FC8
	public void OpenSaveSelection()
	{
		if (MonoSingleton<ConfirmationDialogManager>.Instance != null)
		{
			MonoSingleton<ConfirmationDialogManager>.Instance.ShowConfirmation("Are you the friend with the best computer and network connection?", delegate
			{
				if (this.saveSlotPanel != null)
				{
					this.saveSlotPanel.SetActive(true);
				}
				this.PopulateSlots();
			}, delegate
			{
				Debug.Log("[SaveSlotManager] User cancelled hosting game.");
			}, "Yes, I am", "No, cancel");
			return;
		}
		Debug.LogWarning("[SaveSlotManager] ConfirmationDialogManager not found. Opening save selection without confirmation.");
		if (this.saveSlotPanel != null)
		{
			this.saveSlotPanel.SetActive(true);
		}
		this.PopulateSlots();
	}

	// Token: 0x06001530 RID: 5424 RVA: 0x0005ADAB File Offset: 0x00058FAB
	public void CloseSaveSelection()
	{
		if (this.saveSlotPanel != null)
		{
			this.saveSlotPanel.SetActive(false);
		}
	}

	// Token: 0x06001531 RID: 5425 RVA: 0x0005AE54 File Offset: 0x00059054
	private void PopulateSlots()
	{
		if (this.saveSlots == null || this.saveSlots.Length == 0)
		{
			Debug.LogWarning("[SaveSlotManager] No save slots assigned!");
			return;
		}
		List<string> list = (MonoSingleton<LocalSaveManager>.Instance != null) ? MonoSingleton<LocalSaveManager>.Instance.GetAvailableSaves() : new List<string>();
		int num = 0;
		while (num < this.saveSlots.Length && num < this.maxSlots)
		{
			if (!(this.saveSlots[num] == null))
			{
				if (num < list.Count)
				{
					string saveName = list[num];
					SaveData saveData = MonoSingleton<LocalSaveManager>.Instance.LoadSaveData(saveName);
					this.saveSlots[num].OnSlotSelected = new Action<string>(this.OnSlotSelected);
					this.saveSlots[num].PopulateSlot(saveName, saveData);
				}
				else
				{
					string saveName2 = string.Format("Slot_{0}", num + 1);
					this.saveSlots[num].OnSlotSelected = new Action<string>(this.OnSlotSelected);
					this.saveSlots[num].PopulateSlot(saveName2, null);
				}
			}
			num++;
		}
	}

	// Token: 0x06001532 RID: 5426 RVA: 0x0005AF58 File Offset: 0x00059158
	private void OnSlotSelected(string saveName)
	{
		if (MonoSingleton<LocalSaveManager>.Instance != null)
		{
			MonoSingleton<LocalSaveManager>.Instance.SelectSave(saveName);
		}
		this.CloseSaveSelection();
		base.StartCoroutine(this.StartGameCoroutine());
		if (MonoSingleton<LobbyManager>.Instance != null)
		{
			MonoSingleton<LobbyManager>.Instance.StartGame();
			return;
		}
		Debug.LogError("[SaveSlotManager] LobbyManager not found! Cannot start game.");
	}

	// Token: 0x06001533 RID: 5427 RVA: 0x0005AFB2 File Offset: 0x000591B2
	private IEnumerator StartGameCoroutine()
	{
		MonoSingleton<SceneTransitioner>.Instance.SetLoadingScreen(false, 0.5f, true);
		yield return new WaitForSeconds(1f);
		if (MonoSingleton<LobbyManager>.Instance != null)
		{
			MonoSingleton<LobbyManager>.Instance.StartGame();
		}
		yield break;
	}

	// Token: 0x04000D85 RID: 3461
	[Header("UI References")]
	[SerializeField]
	private GameObject saveSlotPanel;

	// Token: 0x04000D86 RID: 3462
	[SerializeField]
	private SaveSlotUI[] saveSlots;

	// Token: 0x04000D87 RID: 3463
	[SerializeField]
	private int maxSlots = 6;
}
