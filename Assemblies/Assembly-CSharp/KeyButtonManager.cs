using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

// Token: 0x02000231 RID: 561
public class KeyButtonManager : MonoBehaviour
{
	// Token: 0x0600146E RID: 5230 RVA: 0x00057A5A File Offset: 0x00055C5A
	private void Awake()
	{
		if (this.tooltipParent == null)
		{
			this.tooltipParent = base.transform;
		}
	}

	// Token: 0x0600146F RID: 5231 RVA: 0x00057A78 File Offset: 0x00055C78
	public GameObject CreateKeyButton(string keyText)
	{
		if (this.keyButtonPrefab == null)
		{
			Debug.LogError("KeyButtonManager: Key button prefab is not assigned!");
			return null;
		}
		GameObject gameObject = Object.Instantiate<GameObject>(this.keyButtonPrefab, this.tooltipParent);
		gameObject.name = "KeyButton_" + keyText;
		TextMeshProUGUI componentInChildren = gameObject.GetComponentInChildren<TextMeshProUGUI>();
		if (componentInChildren != null)
		{
			componentInChildren.text = keyText;
		}
		else
		{
			Debug.LogError("KeyButtonManager: No TextMeshProUGUI component found in key button prefab for key '" + keyText + "'");
		}
		gameObject.transform.SetAsLastSibling();
		this.activeKeyButtons.Add(gameObject);
		return gameObject;
	}

	// Token: 0x06001470 RID: 5232 RVA: 0x00057B08 File Offset: 0x00055D08
	public GameObject CreateTextElement(string text)
	{
		if (string.IsNullOrEmpty(text))
		{
			return null;
		}
		if (this.textElementPrefab == null)
		{
			Debug.LogError("KeyButtonManager: Text element prefab is not assigned!");
			return null;
		}
		GameObject gameObject = Object.Instantiate<GameObject>(this.textElementPrefab, this.tooltipParent);
		gameObject.name = "TextElement_" + text;
		TextMeshProUGUI component = gameObject.GetComponent<TextMeshProUGUI>();
		if (component != null)
		{
			component.text = text;
		}
		else
		{
			Debug.LogError("KeyButtonManager: No TextMeshProUGUI component found in text element prefab for text '" + text + "'");
		}
		gameObject.transform.SetAsLastSibling();
		this.activeTextElements.Add(gameObject);
		return gameObject;
	}

	// Token: 0x06001471 RID: 5233 RVA: 0x00057BA4 File Offset: 0x00055DA4
	public void ClearKeyButtons()
	{
		foreach (GameObject gameObject in this.activeKeyButtons)
		{
			if (gameObject != null)
			{
				Object.Destroy(gameObject);
			}
		}
		this.activeKeyButtons.Clear();
		foreach (GameObject gameObject2 in this.activeTextElements)
		{
			if (gameObject2 != null)
			{
				Object.Destroy(gameObject2);
			}
		}
		this.activeTextElements.Clear();
	}

	// Token: 0x06001472 RID: 5234 RVA: 0x000048A7 File Offset: 0x00002AA7
	public void SetupLayout()
	{
	}

	// Token: 0x06001473 RID: 5235 RVA: 0x00057C60 File Offset: 0x00055E60
	public float GetTotalWidth()
	{
		float num = 0f;
		foreach (GameObject gameObject in this.activeKeyButtons)
		{
			if (gameObject != null)
			{
				RectTransform component = gameObject.GetComponent<RectTransform>();
				if (component != null)
				{
					num += component.sizeDelta.x;
				}
			}
		}
		foreach (GameObject gameObject2 in this.activeTextElements)
		{
			if (gameObject2 != null)
			{
				TextMeshProUGUI componentInChildren = gameObject2.GetComponentInChildren<TextMeshProUGUI>();
				if (componentInChildren != null)
				{
					num += componentInChildren.preferredWidth;
				}
			}
		}
		return num;
	}

	// Token: 0x06001474 RID: 5236 RVA: 0x00057D40 File Offset: 0x00055F40
	private void OnDestroy()
	{
		this.ClearKeyButtons();
	}

	// Token: 0x04000CE5 RID: 3301
	[Header("Key Button Settings")]
	[SerializeField]
	private GameObject keyButtonPrefab;

	// Token: 0x04000CE6 RID: 3302
	[SerializeField]
	private GameObject textElementPrefab;

	// Token: 0x04000CE7 RID: 3303
	[SerializeField]
	private Transform tooltipParent;

	// Token: 0x04000CE8 RID: 3304
	private List<GameObject> activeKeyButtons = new List<GameObject>();

	// Token: 0x04000CE9 RID: 3305
	private List<GameObject> activeTextElements = new List<GameObject>();
}
