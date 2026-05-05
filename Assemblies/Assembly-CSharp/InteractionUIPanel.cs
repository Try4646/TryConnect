using System;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// Token: 0x02000230 RID: 560
public class InteractionUIPanel : MonoBehaviour
{
	// Token: 0x06001462 RID: 5218 RVA: 0x0005771C File Offset: 0x0005591C
	private void OnEnable()
	{
		SceneManager.sceneLoaded += this.OnSceneLoaded;
		InputEvents.OnThrowItemEvent = (Action<bool>)Delegate.Combine(InputEvents.OnThrowItemEvent, new Action<bool>(this.OnThrowItemEvent));
		if (this.playerInventory)
		{
			this.playerInventory.OnThrowChargeChanged += this.OnThrowChargeChanged;
		}
	}

	// Token: 0x06001463 RID: 5219 RVA: 0x00057780 File Offset: 0x00055980
	private void OnDisable()
	{
		SceneManager.sceneLoaded -= this.OnSceneLoaded;
		InputEvents.OnThrowItemEvent = (Action<bool>)Delegate.Remove(InputEvents.OnThrowItemEvent, new Action<bool>(this.OnThrowItemEvent));
		if (this.playerInventory)
		{
			this.playerInventory.OnThrowChargeChanged -= this.OnThrowChargeChanged;
		}
	}

	// Token: 0x06001464 RID: 5220 RVA: 0x000577E2 File Offset: 0x000559E2
	private void Start()
	{
		this.ResetUI();
	}

	// Token: 0x06001465 RID: 5221 RVA: 0x000577E2 File Offset: 0x000559E2
	private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
	{
		this.ResetUI();
	}

	// Token: 0x06001466 RID: 5222 RVA: 0x000577EC File Offset: 0x000559EC
	public void SetPlayerInventory(PlayerInventory inventory)
	{
		if (this.playerInventory)
		{
			this.playerInventory.OnThrowChargeChanged -= this.OnThrowChargeChanged;
		}
		this.playerInventory = inventory;
		this.playerInventory.OnThrowChargeChanged += this.OnThrowChargeChanged;
	}

	// Token: 0x06001467 RID: 5223 RVA: 0x0005783B File Offset: 0x00055A3B
	public void SetItemNameText(string itemName)
	{
		this.itemNameText.SetText(itemName);
	}

	// Token: 0x06001468 RID: 5224 RVA: 0x0005784C File Offset: 0x00055A4C
	public void SetTooltip(string tooltip)
	{
		if (this.keyButtonManager != null)
		{
			this.keyButtonManager.ClearKeyButtons();
		}
		foreach (TooltipElement tooltipElement in TooltipKeyParser.ParseTooltip(tooltip))
		{
			if (tooltipElement.Type == TooltipElementType.Text)
			{
				this.keyButtonManager.CreateTextElement(tooltipElement.Content);
			}
			else if (tooltipElement.Type == TooltipElementType.Key)
			{
				this.keyButtonManager.CreateKeyButton(tooltipElement.Content);
			}
		}
		this.keyButtonManager.SetupLayout();
	}

	// Token: 0x06001469 RID: 5225 RVA: 0x000578F4 File Offset: 0x00055AF4
	public void UpdateProgressBar(float fillAmount)
	{
		this.progressBar.fillAmount = fillAmount;
	}

	// Token: 0x0600146A RID: 5226 RVA: 0x00057904 File Offset: 0x00055B04
	public void ResetUI()
	{
		this.progressBar.fillAmount = 0f;
		this.itemNameText.SetText("");
		if (this.keyButtonManager != null)
		{
			this.keyButtonManager.ClearKeyButtons();
		}
		if (this.throwForceIndicator != null)
		{
			this.throwForceIndicator.fillAmount = 0f;
		}
		if (this.throwForceIndicatorPanel != null)
		{
			this.throwForceIndicatorPanel.SetActive(false);
		}
	}

	// Token: 0x0600146B RID: 5227 RVA: 0x00057982 File Offset: 0x00055B82
	private void OnThrowItemEvent(bool isPressed)
	{
		if (!isPressed)
		{
			if (this.throwForceIndicatorPanel != null)
			{
				this.throwForceIndicatorPanel.SetActive(false);
			}
			if (this.throwForceIndicator != null)
			{
				this.throwForceIndicator.fillAmount = 0f;
			}
		}
	}

	// Token: 0x0600146C RID: 5228 RVA: 0x000579C0 File Offset: 0x00055BC0
	private void OnThrowChargeChanged(float chargePercentage)
	{
		if (chargePercentage <= 0f)
		{
			if (this.throwForceIndicatorPanel != null)
			{
				this.throwForceIndicatorPanel.SetActive(false);
			}
			if (this.throwForceIndicator != null)
			{
				this.throwForceIndicator.fillAmount = 0f;
			}
			return;
		}
		bool active = chargePercentage > 0f;
		if (this.throwForceIndicatorPanel != null)
		{
			this.throwForceIndicatorPanel.SetActive(active);
		}
		if (this.throwForceIndicator != null)
		{
			this.throwForceIndicator.fillAmount = Mathf.Lerp(0.33f, 0.67f, chargePercentage);
		}
	}

	// Token: 0x04000CDF RID: 3295
	[SerializeField]
	private TextMeshProUGUI itemNameText;

	// Token: 0x04000CE0 RID: 3296
	[SerializeField]
	private Image progressBar;

	// Token: 0x04000CE1 RID: 3297
	[SerializeField]
	private KeyButtonManager keyButtonManager;

	// Token: 0x04000CE2 RID: 3298
	[SerializeField]
	private Image throwForceIndicator;

	// Token: 0x04000CE3 RID: 3299
	[SerializeField]
	private GameObject throwForceIndicatorPanel;

	// Token: 0x04000CE4 RID: 3300
	[SerializeField]
	private PlayerInventory playerInventory;
}
