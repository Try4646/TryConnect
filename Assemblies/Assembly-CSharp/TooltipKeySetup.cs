using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

// Token: 0x02000235 RID: 565
[Serializable]
public class TooltipKeySetup : MonoBehaviour
{
	// Token: 0x0600147A RID: 5242 RVA: 0x00057F5F File Offset: 0x0005615F
	private void Start()
	{
		if (this.autoSetupOnStart)
		{
			this.SetupTooltipKeySystem();
		}
	}

	// Token: 0x0600147B RID: 5243 RVA: 0x00057F70 File Offset: 0x00056170
	[ContextMenu("Setup Tooltip Key System")]
	public void SetupTooltipKeySystem()
	{
		if (this.keyButtonManager == null)
		{
			this.keyButtonManager = base.GetComponent<KeyButtonManager>();
			if (this.keyButtonManager == null)
			{
				this.keyButtonManager = base.gameObject.AddComponent<KeyButtonManager>();
			}
		}
		if (this.keyButtonPrefab == null)
		{
			this.keyButtonPrefab = Resources.Load<GameObject>("KeyButton");
			if (this.keyButtonPrefab == null)
			{
				this.keyButtonPrefab = Resources.Load<GameObject>("UI/GameplayUI/Prefabs/KeyButton");
			}
		}
		if (this.keyButtonManager != null && this.keyButtonPrefab != null)
		{
			FieldInfo field = typeof(KeyButtonManager).GetField("keyButtonPrefab", BindingFlags.Instance | BindingFlags.NonPublic);
			if (field != null)
			{
				field.SetValue(this.keyButtonManager, this.keyButtonPrefab);
			}
			Debug.Log("TooltipKeySetup: Successfully assigned KeyButton prefab to KeyButtonManager");
			return;
		}
		Debug.LogWarning("TooltipKeySetup: Could not find KeyButton prefab. Please assign it manually in the inspector.");
	}

	// Token: 0x0600147C RID: 5244 RVA: 0x00058055 File Offset: 0x00056255
	public void SetKeyButtonPrefab(GameObject prefab)
	{
		this.keyButtonPrefab = prefab;
		this.SetupTooltipKeySystem();
	}

	// Token: 0x0600147D RID: 5245 RVA: 0x00058064 File Offset: 0x00056264
	[ContextMenu("Test Tooltip System")]
	public void TestTooltipSystem()
	{
		if (this.keyButtonManager == null)
		{
			Debug.LogError("TooltipKeySetup: KeyButtonManager not found. Run Setup first.");
			return;
		}
		string text = "hold [E] to pick up";
		List<TooltipElement> list = TooltipKeyParser.ParseTooltip(text);
		Debug.Log("Test Tooltip: '" + text + "'");
		Debug.Log(string.Format("Has Keys: {0}", TooltipKeyParser.HasKeys(text)));
		Debug.Log(string.Format("Elements Count: {0}", list.Count));
		for (int i = 0; i < list.Count; i++)
		{
			Debug.Log(string.Format("Element {0}: Type={1}, Content='{2}'", i, list[i].Type, list[i].Content));
		}
	}

	// Token: 0x04000CF1 RID: 3313
	[Header("Key Button Configuration")]
	[SerializeField]
	private GameObject keyButtonPrefab;

	// Token: 0x04000CF2 RID: 3314
	[SerializeField]
	private KeyButtonManager keyButtonManager;

	// Token: 0x04000CF3 RID: 3315
	[Header("Auto Setup")]
	[SerializeField]
	private bool autoSetupOnStart = true;
}
