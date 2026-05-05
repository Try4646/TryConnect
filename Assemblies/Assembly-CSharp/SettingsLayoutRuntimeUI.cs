using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// Token: 0x0200032C RID: 812
public class SettingsLayoutRuntimeUI : MonoBehaviour
{
	// Token: 0x06001B0F RID: 6927 RVA: 0x00072A8F File Offset: 0x00070C8F
	private void Awake()
	{
		this.BuildTabLookup();
	}

	// Token: 0x06001B10 RID: 6928 RVA: 0x00072A97 File Offset: 0x00070C97
	private void OnEnable()
	{
		SettingItemBase.SettingsChanged += this.OnSettingChanged;
		this.ShowInitialOrCurrentTab();
	}

	// Token: 0x06001B11 RID: 6929 RVA: 0x00072AB0 File Offset: 0x00070CB0
	private void OnDisable()
	{
		SettingItemBase.SettingsChanged -= this.OnSettingChanged;
	}

	// Token: 0x06001B12 RID: 6930 RVA: 0x00072AC4 File Offset: 0x00070CC4
	private void ShowInitialOrCurrentTab()
	{
		if (this.layout == null)
		{
			Debug.LogError("SettingsLayoutRuntimeUI is missing a SettingsLayout reference.");
			return;
		}
		string text = (!string.IsNullOrWhiteSpace(this._currentTabName)) ? this._currentTabName : (string.IsNullOrWhiteSpace(this.defaultTabName) ? ((this.layout.tabs.Count > 0) ? this.layout.tabs[0].tabName : "") : this.defaultTabName);
		if (!string.IsNullOrWhiteSpace(text))
		{
			this.ShowTab(text);
		}
	}

	// Token: 0x06001B13 RID: 6931 RVA: 0x00072B54 File Offset: 0x00070D54
	public void ShowTab(string tabName)
	{
		if (this.layout == null)
		{
			Debug.LogError("SettingsLayoutRuntimeUI has no SettingsLayout assigned.");
			return;
		}
		SettingsLayout.Tab tab = this.layout.tabs.Find((SettingsLayout.Tab t) => string.Equals(t.tabName, tabName, StringComparison.OrdinalIgnoreCase));
		if (tab == null)
		{
			Debug.LogWarning("SettingsLayoutRuntimeUI could not find tab '" + tabName + "' in layout.");
			return;
		}
		RectTransform rectTransform;
		if (!this._tabLookup.TryGetValue(tabName, out rectTransform) || rectTransform == null)
		{
			Debug.LogWarning("SettingsLayoutRuntimeUI could not find content root for tab '" + tabName + "'.");
			return;
		}
		SettingsLayoutRuntimeUI.ClearChildren(rectTransform);
		this._currentTabName = tab.tabName;
		DropdownSettingItem dropdownSettingItem = this.FindDropdownSetting("display");
		bool flag = SettingsLayoutRuntimeUI.IsWindowed((dropdownSettingItem != null) ? dropdownSettingItem.CurrentOption : null);
		ToggleSettingItem toggleSettingItem = this.FindToggleSetting("camSmoothingToggle");
		bool flag2 = toggleSettingItem != null && toggleSettingItem.value;
		foreach (SettingItemBase settingItemBase in tab.entries)
		{
			if (!(settingItemBase == null) && (!SettingsLayoutRuntimeUI.IsKey(settingItemBase, "aspectratio") || flag) && (!SettingsLayoutRuntimeUI.IsKey(settingItemBase, "camSmoothingSlider") || flag2))
			{
				switch (settingItemBase.Kind)
				{
				case SettingKind.Slider:
					this.CreateSliderEntry(rectTransform, settingItemBase as SliderSettingItem);
					break;
				case SettingKind.Dropdown:
					this.CreateDropdownEntry(rectTransform, settingItemBase);
					break;
				case SettingKind.Reset:
					this.CreateResetEntry(rectTransform, settingItemBase as ResetSettingItem, tab);
					break;
				case SettingKind.Title:
					this.CreateTitleEntry(rectTransform, settingItemBase as TitleSettingItem);
					break;
				case SettingKind.Rebind:
					this.CreateRebindEntry(rectTransform, settingItemBase as RebindSettingItem);
					break;
				}
			}
		}
	}

	// Token: 0x06001B14 RID: 6932 RVA: 0x00072D30 File Offset: 0x00070F30
	private void BuildTabLookup()
	{
		this._tabLookup.Clear();
		foreach (SettingsLayoutRuntimeUI.TabContent tabContent in this.tabContents)
		{
			if (tabContent != null && !string.IsNullOrWhiteSpace(tabContent.tabName) && !(tabContent.contentRoot == null))
			{
				this._tabLookup[tabContent.tabName] = tabContent.contentRoot;
			}
		}
	}

	// Token: 0x06001B15 RID: 6933 RVA: 0x00072DBC File Offset: 0x00070FBC
	private static void ClearChildren(RectTransform root)
	{
		for (int i = root.childCount - 1; i >= 0; i--)
		{
			Object.Destroy(root.GetChild(i).gameObject);
		}
	}

	// Token: 0x06001B16 RID: 6934 RVA: 0x00072DF0 File Offset: 0x00070FF0
	private void CreateSliderEntry(RectTransform parent, SliderSettingItem entry)
	{
		SettingsLayoutRuntimeUI.<>c__DisplayClass18_0 CS$<>8__locals1 = new SettingsLayoutRuntimeUI.<>c__DisplayClass18_0();
		CS$<>8__locals1.entry = entry;
		CS$<>8__locals1.<>4__this = this;
		if (this.sliderEntryPrefab == null || CS$<>8__locals1.entry == null)
		{
			return;
		}
		Transform child = Object.Instantiate<GameObject>(this.sliderEntryPrefab, parent).transform.GetChild(0);
		SettingsLayoutRuntimeUI.SetLabel(child.transform, CS$<>8__locals1.entry);
		CS$<>8__locals1.slider = child.GetComponentInChildren<Slider>(true);
		if (CS$<>8__locals1.slider == null)
		{
			return;
		}
		CS$<>8__locals1.slider.minValue = CS$<>8__locals1.entry.min;
		CS$<>8__locals1.slider.maxValue = CS$<>8__locals1.entry.max;
		CS$<>8__locals1.slider.wholeNumbers = CS$<>8__locals1.entry.wholeNumbers;
		CS$<>8__locals1.slider.value = CS$<>8__locals1.entry.value;
		CS$<>8__locals1.valueInput = child.GetComponentInChildren<TMP_InputField>(true);
		Transform transform = child.transform.Find("ValueText");
		if (CS$<>8__locals1.valueInput == null && transform != null)
		{
			CS$<>8__locals1.valueInput = transform.GetComponent<TMP_InputField>();
		}
		CS$<>8__locals1.valueText = ((CS$<>8__locals1.valueInput != null) ? CS$<>8__locals1.valueInput.textComponent : child.GetComponentInChildren<TMP_Text>(true));
		if (CS$<>8__locals1.valueText == null && transform != null)
		{
			CS$<>8__locals1.valueText = transform.GetComponent<TMP_Text>();
		}
		if (CS$<>8__locals1.valueText == null)
		{
			TMP_Text[] componentsInChildren = child.GetComponentsInChildren<TMP_Text>(true);
			Transform transform2 = child.transform.Find("SettingName");
			foreach (TMP_Text tmp_Text in componentsInChildren)
			{
				if (transform2 == null || tmp_Text.transform != transform2)
				{
					CS$<>8__locals1.valueText = tmp_Text;
					break;
				}
			}
		}
		CS$<>8__locals1.<CreateSliderEntry>g__UpdateValueDisplay|0(CS$<>8__locals1.entry.value);
		CS$<>8__locals1.slider.onValueChanged.AddListener(delegate(float newValue)
		{
			CS$<>8__locals1.entry.value = (CS$<>8__locals1.entry.wholeNumbers ? Mathf.Round(newValue) : newValue);
			base.<CreateSliderEntry>g__UpdateValueDisplay|0(CS$<>8__locals1.entry.value);
			CS$<>8__locals1.<>4__this.NotifySettingsChanged(CS$<>8__locals1.entry);
		});
		if (CS$<>8__locals1.valueInput != null)
		{
			CS$<>8__locals1.valueInput.contentType = (CS$<>8__locals1.entry.wholeNumbers ? TMP_InputField.ContentType.IntegerNumber : TMP_InputField.ContentType.DecimalNumber);
			CS$<>8__locals1.valueInput.onEndEdit.AddListener(delegate(string str)
			{
				float value;
				if (!float.TryParse(str, out value))
				{
					return;
				}
				float num = Mathf.Clamp(value, CS$<>8__locals1.entry.min, CS$<>8__locals1.entry.max);
				if (CS$<>8__locals1.entry.wholeNumbers)
				{
					num = Mathf.Round(num);
				}
				CS$<>8__locals1.entry.value = num;
				CS$<>8__locals1.slider.SetValueWithoutNotify(num);
				base.<CreateSliderEntry>g__UpdateValueDisplay|0(num);
				CS$<>8__locals1.<>4__this.NotifySettingsChanged(CS$<>8__locals1.entry);
			});
		}
	}

	// Token: 0x06001B17 RID: 6935 RVA: 0x00073030 File Offset: 0x00071230
	private void CreateDropdownEntry(RectTransform parent, SettingItemBase entry)
	{
		if (this.dropdownEntryPrefab == null)
		{
			return;
		}
		Transform child = Object.Instantiate<GameObject>(this.dropdownEntryPrefab, parent).transform.GetChild(0);
		SettingsLayoutRuntimeUI.SetLabel(child.transform, entry);
		ValueTuple<List<string>, int, List<object>> options = this.BuildDropdownOptions(entry);
		TMP_Dropdown componentInChildren = child.GetComponentInChildren<TMP_Dropdown>(true);
		if (componentInChildren != null)
		{
			componentInChildren.ClearOptions();
			componentInChildren.AddOptions(options.Item1);
			componentInChildren.value = options.Item2;
			componentInChildren.RefreshShownValue();
			componentInChildren.onValueChanged.AddListener(delegate(int index)
			{
				this.ApplyDropdownSelection(entry, index, options);
				this.NotifySettingsChanged(entry);
			});
			return;
		}
		Dropdown componentInChildren2 = child.GetComponentInChildren<Dropdown>(true);
		if (componentInChildren2 == null)
		{
			return;
		}
		componentInChildren2.ClearOptions();
		componentInChildren2.AddOptions(options.Item1);
		componentInChildren2.value = options.Item2;
		componentInChildren2.RefreshShownValue();
		componentInChildren2.onValueChanged.AddListener(delegate(int index)
		{
			this.ApplyDropdownSelection(entry, index, options);
			this.NotifySettingsChanged(entry);
		});
	}

	// Token: 0x06001B18 RID: 6936 RVA: 0x0007314C File Offset: 0x0007134C
	private void CreateResetEntry(RectTransform parent, ResetSettingItem entry, SettingsLayout.Tab tab)
	{
		if (this.resetEntryPrefab == null || entry == null)
		{
			return;
		}
		GameObject gameObject = Object.Instantiate<GameObject>(this.resetEntryPrefab, parent);
		SettingsLayoutRuntimeUI.SetLabel(gameObject.transform, entry);
		Button componentInChildren = gameObject.GetComponentInChildren<Button>(true);
		if (componentInChildren == null)
		{
			return;
		}
		componentInChildren.onClick.AddListener(delegate()
		{
			this.ResetTabToDefaults(tab);
		});
	}

	// Token: 0x06001B19 RID: 6937 RVA: 0x000731C5 File Offset: 0x000713C5
	private void CreateTitleEntry(RectTransform parent, TitleSettingItem entry)
	{
		if (this.titleEntryPrefab == null || entry == null)
		{
			return;
		}
		SettingsLayoutRuntimeUI.SetLabel(Object.Instantiate<GameObject>(this.titleEntryPrefab, parent).transform, entry);
	}

	// Token: 0x06001B1A RID: 6938 RVA: 0x000731F8 File Offset: 0x000713F8
	private void CreateRebindEntry(RectTransform parent, RebindSettingItem entry)
	{
		if (this.dropdownEntryPrefab == null || entry == null)
		{
			return;
		}
		Transform child = Object.Instantiate<GameObject>(this.dropdownEntryPrefab, parent).transform.GetChild(0);
		SettingsLayoutRuntimeUI.SetLabel(child.transform, entry);
		InputReader instance = InputReader.Instance;
		string originalLabel = (instance != null) ? instance.GetBindingDisplayName(entry.actionName, entry.bindingIndex) : "Unassigned";
		TMP_Dropdown tmpDropdown = child.GetComponentInChildren<TMP_Dropdown>(true);
		if (tmpDropdown != null)
		{
			tmpDropdown.ClearOptions();
			tmpDropdown.AddOptions(new List<string>
			{
				originalLabel
			});
			tmpDropdown.SetValueWithoutNotify(0);
			tmpDropdown.RefreshShownValue();
			Action<string> <>9__2;
			Action <>9__3;
			SettingsLayoutRuntimeUI.AddPointerClickHandler(tmpDropdown.gameObject, delegate
			{
				string item = "Listening...";
				tmpDropdown.ClearOptions();
				tmpDropdown.AddOptions(new List<string>
				{
					item
				});
				tmpDropdown.SetValueWithoutNotify(0);
				tmpDropdown.RefreshShownValue();
				if (!(InputReader.Instance == null))
				{
					InputReader instance2 = InputReader.Instance;
					string actionName = entry.actionName;
					int bindingIndex = entry.bindingIndex;
					Action<string> onComplete;
					if ((onComplete = <>9__2) == null)
					{
						onComplete = (<>9__2 = delegate(string _)
						{
							entry.overridePath = InputReader.Instance.GetBindingEffectivePath(entry.actionName, entry.bindingIndex);
							string bindingDisplayName = InputReader.Instance.GetBindingDisplayName(entry.actionName, entry.bindingIndex);
							tmpDropdown.ClearOptions();
							tmpDropdown.AddOptions(new List<string>
							{
								bindingDisplayName
							});
							tmpDropdown.SetValueWithoutNotify(0);
							tmpDropdown.RefreshShownValue();
							this.NotifySettingsChanged(entry);
						});
					}
					Action onCancelled;
					if ((onCancelled = <>9__3) == null)
					{
						onCancelled = (<>9__3 = delegate()
						{
							string item2 = (InputReader.Instance != null) ? InputReader.Instance.GetBindingDisplayName(entry.actionName, entry.bindingIndex) : originalLabel;
							tmpDropdown.ClearOptions();
							tmpDropdown.AddOptions(new List<string>
							{
								item2
							});
							tmpDropdown.SetValueWithoutNotify(0);
							tmpDropdown.RefreshShownValue();
						});
					}
					if (instance2.StartInteractiveRebind(actionName, bindingIndex, onComplete, onCancelled))
					{
						return;
					}
				}
				tmpDropdown.ClearOptions();
				tmpDropdown.AddOptions(new List<string>
				{
					originalLabel
				});
				tmpDropdown.SetValueWithoutNotify(0);
				tmpDropdown.RefreshShownValue();
			});
			return;
		}
		Dropdown dropdown = child.GetComponentInChildren<Dropdown>(true);
		if (dropdown == null)
		{
			return;
		}
		dropdown.ClearOptions();
		dropdown.AddOptions(new List<string>
		{
			originalLabel
		});
		dropdown.SetValueWithoutNotify(0);
		dropdown.RefreshShownValue();
		Action<string> <>9__4;
		Action <>9__5;
		SettingsLayoutRuntimeUI.AddPointerClickHandler(dropdown.gameObject, delegate
		{
			string item = "Listening...";
			dropdown.ClearOptions();
			dropdown.AddOptions(new List<string>
			{
				item
			});
			dropdown.SetValueWithoutNotify(0);
			dropdown.RefreshShownValue();
			if (!(InputReader.Instance == null))
			{
				InputReader instance2 = InputReader.Instance;
				string actionName = entry.actionName;
				int bindingIndex = entry.bindingIndex;
				Action<string> onComplete;
				if ((onComplete = <>9__4) == null)
				{
					onComplete = (<>9__4 = delegate(string _)
					{
						entry.overridePath = InputReader.Instance.GetBindingEffectivePath(entry.actionName, entry.bindingIndex);
						string bindingDisplayName = InputReader.Instance.GetBindingDisplayName(entry.actionName, entry.bindingIndex);
						dropdown.ClearOptions();
						dropdown.AddOptions(new List<string>
						{
							bindingDisplayName
						});
						dropdown.SetValueWithoutNotify(0);
						dropdown.RefreshShownValue();
						this.NotifySettingsChanged(entry);
					});
				}
				Action onCancelled;
				if ((onCancelled = <>9__5) == null)
				{
					onCancelled = (<>9__5 = delegate()
					{
						string item2 = (InputReader.Instance != null) ? InputReader.Instance.GetBindingDisplayName(entry.actionName, entry.bindingIndex) : originalLabel;
						dropdown.ClearOptions();
						dropdown.AddOptions(new List<string>
						{
							item2
						});
						dropdown.SetValueWithoutNotify(0);
						dropdown.RefreshShownValue();
					});
				}
				if (instance2.StartInteractiveRebind(actionName, bindingIndex, onComplete, onCancelled))
				{
					return;
				}
			}
			dropdown.ClearOptions();
			dropdown.AddOptions(new List<string>
			{
				originalLabel
			});
			dropdown.SetValueWithoutNotify(0);
			dropdown.RefreshShownValue();
		});
	}

	// Token: 0x06001B1B RID: 6939 RVA: 0x00073384 File Offset: 0x00071584
	private static void AddPointerClickHandler(GameObject target, Action onClick)
	{
		if (target == null || onClick == null)
		{
			return;
		}
		EventTrigger eventTrigger = target.GetComponent<EventTrigger>();
		if (eventTrigger == null)
		{
			eventTrigger = target.AddComponent<EventTrigger>();
		}
		if (eventTrigger.triggers == null)
		{
			eventTrigger.triggers = new List<EventTrigger.Entry>();
		}
		EventTrigger.Entry entry = new EventTrigger.Entry
		{
			eventID = EventTriggerType.PointerClick
		};
		entry.callback.AddListener(delegate(BaseEventData _)
		{
			onClick();
		});
		eventTrigger.triggers.Add(entry);
	}

	// Token: 0x06001B1C RID: 6940 RVA: 0x0007340C File Offset: 0x0007160C
	private ToggleSettingItem FindToggleSetting(string key)
	{
		if (this.layout == null)
		{
			return null;
		}
		foreach (SettingsLayout.Tab tab in this.layout.tabs)
		{
			if (tab != null)
			{
				foreach (SettingItemBase settingItemBase in tab.entries)
				{
					ToggleSettingItem toggleSettingItem = settingItemBase as ToggleSettingItem;
					if (toggleSettingItem != null && SettingsLayoutRuntimeUI.IsKey(toggleSettingItem, key))
					{
						return toggleSettingItem;
					}
				}
			}
		}
		return null;
	}

	// Token: 0x06001B1D RID: 6941 RVA: 0x000734C8 File Offset: 0x000716C8
	[return: TupleElementNames(new string[]
	{
		"labels",
		"selectedIndex",
		"values"
	})]
	private ValueTuple<List<string>, int, List<object>> BuildDropdownOptions(SettingItemBase entry)
	{
		List<string> list = new List<string>();
		List<object> list2 = new List<object>();
		int item = 0;
		ToggleSettingItem toggleSettingItem = entry as ToggleSettingItem;
		if (toggleSettingItem != null)
		{
			list.Add("No");
			list.Add("Yes");
			list2.Add(false);
			list2.Add(true);
			item = (toggleSettingItem.value ? 1 : 0);
		}
		else
		{
			ResolutionSettingItem resolutionSettingItem = entry as ResolutionSettingItem;
			if (resolutionSettingItem != null)
			{
				Resolution[] availableResolutions = SettingsLayoutRuntimeUI.GetAvailableResolutions();
				for (int i = 0; i < availableResolutions.Length; i++)
				{
					string item2 = string.Format("{0}x{1}", availableResolutions[i].width, availableResolutions[i].height);
					list.Add(item2);
					list2.Add(availableResolutions[i]);
					if (resolutionSettingItem.width == availableResolutions[i].width && resolutionSettingItem.height == availableResolutions[i].height)
					{
						item = i;
					}
				}
			}
			else
			{
				DropdownSettingItem dropdownSettingItem = entry as DropdownSettingItem;
				if (dropdownSettingItem != null)
				{
					List<string> list3 = dropdownSettingItem.options;
					if (SettingsLayoutRuntimeUI.IsKey(dropdownSettingItem, "resolution"))
					{
						string currentSelection = dropdownSettingItem.CurrentOption;
						list3 = this.BuildResolutionOptions();
						int num = list3.FindIndex((string option) => string.Equals(option, currentSelection, StringComparison.OrdinalIgnoreCase));
						if (num >= 0)
						{
							dropdownSettingItem.index = num;
						}
						else
						{
							dropdownSettingItem.index = Mathf.Clamp(dropdownSettingItem.index, 0, Mathf.Max(0, list3.Count - 1));
						}
					}
					if (dropdownSettingItem.useDynamicOptions)
					{
						IDropdownOptionsProvider dropdownOptionsProvider = dropdownSettingItem.optionsProvider as IDropdownOptionsProvider;
						if (dropdownOptionsProvider != null)
						{
							list3 = (dropdownOptionsProvider.GetOptions() ?? new List<string>());
							dropdownSettingItem.index = dropdownOptionsProvider.GetDefaultIndex(list3);
						}
					}
					dropdownSettingItem.options = (list3 ?? new List<string>());
					for (int j = 0; j < list3.Count; j++)
					{
						list.Add(list3[j]);
						list2.Add(list3[j]);
						if (dropdownSettingItem.index == j)
						{
							item = j;
						}
					}
				}
			}
		}
		return new ValueTuple<List<string>, int, List<object>>(list, item, list2);
	}

	// Token: 0x06001B1E RID: 6942 RVA: 0x00073708 File Offset: 0x00071908
	private void ApplyDropdownSelection(SettingItemBase entry, int index, [TupleElementNames(new string[]
	{
		"labels",
		"selectedIndex",
		"values"
	})] ValueTuple<List<string>, int, List<object>> options)
	{
		if (index < 0 || index >= options.Item3.Count)
		{
			return;
		}
		object obj = options.Item3[index];
		ToggleSettingItem toggleSettingItem = entry as ToggleSettingItem;
		if (toggleSettingItem != null)
		{
			toggleSettingItem.value = (index == 1);
			return;
		}
		ResolutionSettingItem resolutionSettingItem = entry as ResolutionSettingItem;
		if (resolutionSettingItem != null && obj is Resolution)
		{
			Resolution resolution = (Resolution)obj;
			resolutionSettingItem.width = resolution.width;
			resolutionSettingItem.height = resolution.height;
			return;
		}
		DropdownSettingItem dropdownSettingItem = entry as DropdownSettingItem;
		if (dropdownSettingItem != null && obj is string)
		{
			dropdownSettingItem.index = index;
		}
	}

	// Token: 0x06001B1F RID: 6943 RVA: 0x0007379C File Offset: 0x0007199C
	private void ResetTabToDefaults(SettingsLayout.Tab tab)
	{
		if (this.layout == null || tab == null)
		{
			return;
		}
		List<SettingItemBase> list = new List<SettingItemBase>();
		foreach (SettingItemBase settingItemBase in tab.entries)
		{
			if (!(settingItemBase == null) && !(settingItemBase is ResetSettingItem))
			{
				ToggleSettingItem toggleSettingItem = settingItemBase as ToggleSettingItem;
				if (toggleSettingItem != null)
				{
					if (toggleSettingItem.value != toggleSettingItem.defaultValue)
					{
						toggleSettingItem.value = toggleSettingItem.defaultValue;
						list.Add(toggleSettingItem);
					}
				}
				else
				{
					SliderSettingItem sliderSettingItem = settingItemBase as SliderSettingItem;
					if (sliderSettingItem != null)
					{
						float num = Mathf.Clamp(sliderSettingItem.defaultValue, sliderSettingItem.min, sliderSettingItem.max);
						if (sliderSettingItem.wholeNumbers)
						{
							num = Mathf.Round(num);
						}
						if (!Mathf.Approximately(sliderSettingItem.value, num))
						{
							sliderSettingItem.value = num;
							list.Add(sliderSettingItem);
						}
					}
					else
					{
						DropdownSettingItem dropdownSettingItem = settingItemBase as DropdownSettingItem;
						if (dropdownSettingItem != null && SettingsLayoutRuntimeUI.IsKey(dropdownSettingItem, "microphonedevice"))
						{
							if (dropdownSettingItem.useDynamicOptions)
							{
								IDropdownOptionsProvider dropdownOptionsProvider = dropdownSettingItem.optionsProvider as IDropdownOptionsProvider;
								if (dropdownOptionsProvider != null)
								{
									dropdownSettingItem.options = (dropdownOptionsProvider.GetOptions() ?? dropdownSettingItem.options);
								}
							}
							List<string> options = dropdownSettingItem.options;
							if (options != null && options.Count != 0)
							{
								if (!dropdownSettingItem.useDynamicOptions)
								{
									goto IL_163;
								}
								IDropdownOptionsProvider dropdownOptionsProvider2 = dropdownSettingItem.optionsProvider as IDropdownOptionsProvider;
								if (dropdownOptionsProvider2 == null)
								{
									goto IL_163;
								}
								int num2 = dropdownOptionsProvider2.GetDefaultIndex(options);
								IL_16F:
								int num3 = num2;
								num3 = Mathf.Clamp(num3, 0, options.Count - 1);
								if (dropdownSettingItem.index != num3)
								{
									dropdownSettingItem.index = num3;
									list.Add(dropdownSettingItem);
									continue;
								}
								continue;
								IL_163:
								num2 = 0;
								goto IL_16F;
							}
						}
					}
				}
			}
		}
		foreach (SettingItemBase entry in list)
		{
			this.NotifySettingsChanged(entry);
		}
		if (!string.IsNullOrWhiteSpace(tab.tabName))
		{
			this.ShowTab(tab.tabName);
		}
	}

	// Token: 0x06001B20 RID: 6944 RVA: 0x000739E8 File Offset: 0x00071BE8
	private static void SetLabel(Transform root, SettingItemBase entry)
	{
		string text = (entry != null) ? entry.DisplayLabel : string.Empty;
		if (string.IsNullOrWhiteSpace(text))
		{
			return;
		}
		Transform transform = root.Find("SettingName");
		if (transform == null)
		{
			return;
		}
		TMP_Text component = transform.GetComponent<TMP_Text>();
		if (component != null)
		{
			component.text = text;
		}
	}

	// Token: 0x06001B21 RID: 6945 RVA: 0x00073A42 File Offset: 0x00071C42
	private void NotifySettingsChanged(SettingItemBase entry)
	{
		if (this.layout == null || entry == null)
		{
			return;
		}
		this.layout.NotifyChanged(entry);
		entry.NotifyChanged();
	}

	// Token: 0x06001B22 RID: 6946 RVA: 0x00073A70 File Offset: 0x00071C70
	private void OnSettingChanged(SettingItemBase entry)
	{
		if (this.layout == null || entry == null)
		{
			return;
		}
		if ((SettingsLayoutRuntimeUI.IsKey(entry, "display") || SettingsLayoutRuntimeUI.IsKey(entry, "aspectratio") || SettingsLayoutRuntimeUI.IsKey(entry, "camSmoothingToggle")) && !string.IsNullOrWhiteSpace(this._currentTabName))
		{
			this.ShowTab(this._currentTabName);
		}
	}

	// Token: 0x06001B23 RID: 6947 RVA: 0x00073AD8 File Offset: 0x00071CD8
	private DropdownSettingItem FindDropdownSetting(string key)
	{
		if (this.layout == null)
		{
			return null;
		}
		foreach (SettingsLayout.Tab tab in this.layout.tabs)
		{
			if (tab != null)
			{
				foreach (SettingItemBase settingItemBase in tab.entries)
				{
					DropdownSettingItem dropdownSettingItem = settingItemBase as DropdownSettingItem;
					if (dropdownSettingItem != null && SettingsLayoutRuntimeUI.IsKey(dropdownSettingItem, key))
					{
						return dropdownSettingItem;
					}
				}
			}
		}
		return null;
	}

	// Token: 0x06001B24 RID: 6948 RVA: 0x0006CEFF File Offset: 0x0006B0FF
	private static bool IsKey(SettingItemBase entry, string key)
	{
		return !(entry == null) && !string.IsNullOrWhiteSpace(entry.key) && string.Equals(entry.key.Trim(), key, StringComparison.OrdinalIgnoreCase);
	}

	// Token: 0x06001B25 RID: 6949 RVA: 0x00073B94 File Offset: 0x00071D94
	private static bool IsFullscreenExclusive(string displayMode)
	{
		if (string.IsNullOrWhiteSpace(displayMode))
		{
			return false;
		}
		string a = displayMode.Trim().ToLowerInvariant();
		return a == "fullscreen" || a == "exclusive fullscreen";
	}

	// Token: 0x06001B26 RID: 6950 RVA: 0x00073BD4 File Offset: 0x00071DD4
	private static bool IsFullscreenWindowed(string displayMode)
	{
		if (string.IsNullOrWhiteSpace(displayMode))
		{
			return false;
		}
		string a = displayMode.Trim().ToLowerInvariant();
		return a == "windowed fullscreen" || a == "borderless" || a == "borderless fullscreen" || a == "fullscreen windowed";
	}

	// Token: 0x06001B27 RID: 6951 RVA: 0x00073C2B File Offset: 0x00071E2B
	private static bool IsWindowed(string displayMode)
	{
		return !string.IsNullOrWhiteSpace(displayMode) && displayMode.Trim().ToLowerInvariant() == "windowed";
	}

	// Token: 0x06001B28 RID: 6952 RVA: 0x00073C4C File Offset: 0x00071E4C
	private List<string> BuildResolutionOptions()
	{
		List<string> list = new List<string>();
		Resolution[] resolutions = Screen.resolutions;
		if (resolutions == null || resolutions.Length == 0)
		{
			list.Add(string.Format("{0}x{1}", Screen.currentResolution.width, Screen.currentResolution.height));
			return list;
		}
		float targetAspectRatio = this.GetTargetAspectRatio();
		HashSet<string> hashSet = new HashSet<string>();
		List<Resolution> list2 = new List<Resolution>();
		foreach (Resolution item in resolutions)
		{
			if (Mathf.Abs((float)item.width / (float)item.height - targetAspectRatio) <= 0.01f)
			{
				string item2 = string.Format("{0}x{1}", item.width, item.height);
				if (hashSet.Add(item2))
				{
					list2.Add(item);
				}
			}
		}
		if (list2.Count == 0)
		{
			list.Add(string.Format("{0}x{1}", Screen.currentResolution.width, Screen.currentResolution.height));
			return list;
		}
		list2.Sort(delegate(Resolution a, Resolution b)
		{
			int value = a.width * a.height;
			return (b.width * b.height).CompareTo(value);
		});
		for (int j = 0; j < list2.Count; j++)
		{
			list.Add(string.Format("{0}x{1}", list2[j].width, list2[j].height));
		}
		return list;
	}

	// Token: 0x06001B29 RID: 6953 RVA: 0x00073DE8 File Offset: 0x00071FE8
	private float GetTargetAspectRatio()
	{
		DropdownSettingItem dropdownSettingItem = this.FindDropdownSetting("display");
		if (SettingsLayoutRuntimeUI.IsFullscreenExclusive((dropdownSettingItem != null) ? dropdownSettingItem.CurrentOption : null) || SettingsLayoutRuntimeUI.IsFullscreenWindowed((dropdownSettingItem != null) ? dropdownSettingItem.CurrentOption : null))
		{
			return (float)Screen.currentResolution.width / (float)Screen.currentResolution.height;
		}
		DropdownSettingItem dropdownSettingItem2 = this.FindDropdownSetting("aspectratio");
		float result;
		if (SettingsLayoutRuntimeUI.TryParseAspectRatio((dropdownSettingItem2 != null) ? dropdownSettingItem2.CurrentOption : null, out result))
		{
			return result;
		}
		return (float)Screen.width / (float)Screen.height;
	}

	// Token: 0x06001B2A RID: 6954 RVA: 0x00073E74 File Offset: 0x00072074
	private static bool TryParseAspectRatio(string value, out float ratio)
	{
		ratio = 0f;
		if (string.IsNullOrWhiteSpace(value))
		{
			return false;
		}
		string[] array = value.Split(':', StringSplitOptions.None);
		if (array.Length != 2)
		{
			return false;
		}
		float num;
		if (!float.TryParse(array[0].Trim(), out num))
		{
			return false;
		}
		float num2;
		if (!float.TryParse(array[1].Trim(), out num2))
		{
			return false;
		}
		if (num2 <= 0f)
		{
			return false;
		}
		ratio = num / num2;
		return true;
	}

	// Token: 0x06001B2B RID: 6955 RVA: 0x00073EDC File Offset: 0x000720DC
	private static Resolution[] GetAvailableResolutions()
	{
		Resolution[] resolutions = Screen.resolutions;
		if (resolutions == null || resolutions.Length == 0)
		{
			return new Resolution[]
			{
				Screen.currentResolution
			};
		}
		List<Resolution> list = new List<Resolution>();
		foreach (Resolution item in resolutions)
		{
			bool flag = false;
			for (int j = 0; j < list.Count; j++)
			{
				if (list[j].width == item.width && list[j].height == item.height)
				{
					flag = true;
					break;
				}
			}
			if (!flag)
			{
				list.Add(item);
			}
		}
		list.Sort(delegate(Resolution a, Resolution b)
		{
			int value = a.width * a.height;
			return (b.width * b.height).CompareTo(value);
		});
		return list.ToArray();
	}

	// Token: 0x040011E7 RID: 4583
	[Header("Layout Source")]
	[SerializeField]
	private SettingsLayout layout;

	// Token: 0x040011E8 RID: 4584
	[Header("Prefabs")]
	[SerializeField]
	private GameObject toggleEntryPrefab;

	// Token: 0x040011E9 RID: 4585
	[SerializeField]
	private GameObject sliderEntryPrefab;

	// Token: 0x040011EA RID: 4586
	[SerializeField]
	private GameObject dropdownEntryPrefab;

	// Token: 0x040011EB RID: 4587
	[SerializeField]
	private GameObject resetEntryPrefab;

	// Token: 0x040011EC RID: 4588
	[SerializeField]
	private GameObject titleEntryPrefab;

	// Token: 0x040011ED RID: 4589
	[Header("Tabs")]
	[SerializeField]
	private List<SettingsLayoutRuntimeUI.TabContent> tabContents = new List<SettingsLayoutRuntimeUI.TabContent>();

	// Token: 0x040011EE RID: 4590
	[SerializeField]
	private string defaultTabName = "";

	// Token: 0x040011EF RID: 4591
	private readonly Dictionary<string, RectTransform> _tabLookup = new Dictionary<string, RectTransform>(StringComparer.OrdinalIgnoreCase);

	// Token: 0x040011F0 RID: 4592
	private string _currentTabName;

	// Token: 0x0200032D RID: 813
	[Serializable]
	public class TabContent
	{
		// Token: 0x040011F1 RID: 4593
		public string tabName;

		// Token: 0x040011F2 RID: 4594
		public RectTransform contentRoot;
	}
}
