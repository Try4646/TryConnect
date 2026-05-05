using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.UIElements;

// Token: 0x02000309 RID: 777
public class GameSettingsManager : MonoBehaviour
{
	// Token: 0x1700026F RID: 623
	// (get) Token: 0x06001A7D RID: 6781 RVA: 0x0006FB07 File Offset: 0x0006DD07
	public bool IsVisible
	{
		get
		{
			return this.isVisible;
		}
	}

	// Token: 0x06001A7E RID: 6782 RVA: 0x0006FB10 File Offset: 0x0006DD10
	private void Start()
	{
		this.settingsDict = new Dictionary<string, ScriptableObject>();
		if (this.gameSettings != null)
		{
			this.settingsDict["Game"] = this.gameSettings;
		}
		if (this.playerSettings != null)
		{
			this.settingsDict["Player"] = this.playerSettings;
		}
		if (this.cameraSettings != null)
		{
			this.settingsDict["Camera"] = this.cameraSettings;
		}
		if (this.soundSettings != null)
		{
			this.settingsDict["Sound"] = this.soundSettings;
		}
		if (this.colorPalette != null)
		{
			this.settingsDict["Colors"] = this.colorPalette;
		}
		if (this.lobbySettings != null)
		{
			this.settingsDict["Lobby"] = this.lobbySettings;
		}
		if (this.spawnableSettings != null)
		{
			this.settingsDict["Spawnable"] = this.spawnableSettings;
		}
		if (this.challengeSettings != null)
		{
			this.settingsDict["Challenge"] = this.challengeSettings;
		}
		if (this.settingsDict.Count > 0)
		{
			this._selectedSettingType = new List<string>(this.settingsDict.Keys)[0];
		}
		else
		{
			this._selectedSettingType = null;
		}
		if (this.uiDocument == null)
		{
			Debug.LogError("UIDocument reference is missing on GameSettingsManager!");
			return;
		}
		VisualTreeAsset visualTreeAsset = Resources.Load<VisualTreeAsset>("GameSettingsManager");
		if (visualTreeAsset == null)
		{
			Debug.LogError("Failed to load GameSettingsManager.uxml from Resources!");
			return;
		}
		StyleSheet styleSheet = Resources.Load<StyleSheet>("GameSettingsManager");
		if (styleSheet == null)
		{
			Debug.LogError("Failed to load GameSettingsManager.uss from Resources!");
			return;
		}
		this.uiDocument.rootVisualElement.Clear();
		this.root = visualTreeAsset.CloneTree();
		this.root.styleSheets.Add(styleSheet);
		this.uiDocument.rootVisualElement.Add(this.root);
		this.root.style.display = DisplayStyle.None;
		this.CreateGUI();
	}

	// Token: 0x06001A7F RID: 6783 RVA: 0x000048A7 File Offset: 0x00002AA7
	private void OnDestroy()
	{
	}

	// Token: 0x06001A80 RID: 6784 RVA: 0x0006FD34 File Offset: 0x0006DF34
	private void CreateGUI()
	{
		VisualElement chipBar = this.root.Q("chipBar", null);
		ScrollView settingsScroll = this.root.Q("settingsScroll", null);
		Font font = Resources.Load<Font>("Fonts/Bungee-Regular");
		if (font != null)
		{
			this.root.style.unityFont = font;
		}
		chipBar.Clear();
		chipBar.style.flexDirection = FlexDirection.Row;
		chipBar.style.flexWrap = Wrap.NoWrap;
		foreach (string text in this.settingsDict.Keys)
		{
			Button chip = null;
			string capturedTab = text;
			chip = new Button(delegate()
			{
				this._selectedSettingType = capturedTab;
				this.UpdateSettingsList(settingsScroll);
				foreach (VisualElement visualElement in chipBar.Children())
				{
					visualElement.RemoveFromClassList("chip--selected");
				}
				chip.AddToClassList("chip--selected");
			})
			{
				text = text
			};
			chip.AddToClassList("chip");
			if (text == this._selectedSettingType)
			{
				chip.AddToClassList("chip--selected");
			}
			chipBar.Add(chip);
		}
		this.UpdateSettingsList(settingsScroll);
	}

	// Token: 0x06001A81 RID: 6785 RVA: 0x0006FEBC File Offset: 0x0006E0BC
	private void UpdateSettingsList(ScrollView scroll)
	{
		scroll.Clear();
		if (this._selectedSettingType == null || !this.settingsDict.ContainsKey(this._selectedSettingType))
		{
			scroll.Add(new Label("No settings assigned for this tab.")
			{
				style = 
				{
					unityFont = Resources.Load<Font>("Fonts/Bungee-Regular")
				}
			});
			return;
		}
		ScriptableObject setting = this.settingsDict[this._selectedSettingType];
		if (setting == null)
		{
			scroll.Add(new Label(this._selectedSettingType + " settings are not assigned!")
			{
				style = 
				{
					unityFont = Resources.Load<Font>("Fonts/Bungee-Regular")
				}
			});
			return;
		}
		VisualElement visualElement = new VisualElement();
		visualElement.style.marginBottom = 8f;
		visualElement.style.paddingBottom = 8f;
		visualElement.style.backgroundColor = new Color(0.15f, 0.15f, 0.15f);
		FieldInfo[] fields = setting.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public);
		for (int i = 0; i < fields.Length; i++)
		{
			FieldInfo field = fields[i];
			if (!(field.Name == "m_Script"))
			{
				VisualElement visualElement2 = new VisualElement();
				visualElement2.style.flexDirection = FlexDirection.Row;
				visualElement2.style.marginBottom = 4f;
				Label child = new Label(field.Name)
				{
					style = 
					{
						width = 150f,
						color = Color.white,
						unityFont = Resources.Load<Font>("Fonts/Bungee-Regular")
					}
				};
				visualElement2.Add(child);
				object value = field.GetValue(setting);
				VisualElement visualElement3 = this.CreateFieldForType(field.FieldType, value, delegate(object newValue)
				{
					field.SetValue(setting, newValue);
					this.NotifySettingsChanged(setting);
				});
				if (visualElement3 != null)
				{
					visualElement3.style.flexGrow = 1f;
					visualElement3.style.unityFont = Resources.Load<Font>("Fonts/Bungee-Regular");
					visualElement2.Add(visualElement3);
					visualElement.Add(visualElement2);
				}
			}
		}
		scroll.Add(visualElement);
	}

	// Token: 0x06001A82 RID: 6786 RVA: 0x00070154 File Offset: 0x0006E354
	private void NotifySettingsChanged(ScriptableObject setting)
	{
		MethodInfo method = setting.GetType().GetMethod("NotifyChanged", BindingFlags.Instance | BindingFlags.Public);
		if (method != null)
		{
			method.Invoke(setting, null);
		}
		if (this.isVisible)
		{
			ScrollView scroll = this.root.Q("settingsScroll", null);
			this.UpdateSettingsList(scroll);
		}
	}

	// Token: 0x06001A83 RID: 6787 RVA: 0x000701A8 File Offset: 0x0006E3A8
	private VisualElement CreateFieldForType(Type type, object value, Action<object> onValueChanged)
	{
		Font v = Resources.Load<Font>("Fonts/Bungee-Regular");
		if (type == typeof(float))
		{
			FloatField floatField = new FloatField();
			floatField.value = (float)value;
			floatField.style.unityFont = v;
			floatField.RegisterValueChangedCallback(delegate(ChangeEvent<float> evt)
			{
				onValueChanged(evt.newValue);
			});
			return floatField;
		}
		if (type == typeof(int))
		{
			IntegerField integerField = new IntegerField();
			integerField.value = (int)value;
			integerField.style.unityFont = v;
			integerField.RegisterValueChangedCallback(delegate(ChangeEvent<int> evt)
			{
				onValueChanged(evt.newValue);
			});
			return integerField;
		}
		if (type == typeof(bool))
		{
			Toggle toggle = new Toggle();
			toggle.value = (bool)value;
			toggle.style.unityFont = v;
			toggle.RegisterValueChangedCallback(delegate(ChangeEvent<bool> evt)
			{
				onValueChanged(evt.newValue);
			});
			return toggle;
		}
		if (type == typeof(string))
		{
			TextField textField = new TextField();
			textField.value = (string)value;
			textField.style.unityFont = v;
			textField.RegisterValueChangedCallback(delegate(ChangeEvent<string> evt)
			{
				onValueChanged(evt.newValue);
			});
			return textField;
		}
		if (type == typeof(Vector2))
		{
			VisualElement visualElement = new VisualElement();
			visualElement.style.flexDirection = FlexDirection.Row;
			FloatField floatField2 = new FloatField
			{
				value = ((Vector2)value).x
			};
			FloatField floatField3 = new FloatField
			{
				value = ((Vector2)value).y
			};
			floatField2.style.unityFont = v;
			floatField3.style.unityFont = v;
			floatField2.RegisterValueChangedCallback(delegate(ChangeEvent<float> evt)
			{
				Vector2 vector = (Vector2)value;
				vector.x = evt.newValue;
				onValueChanged(vector);
			});
			floatField3.RegisterValueChangedCallback(delegate(ChangeEvent<float> evt)
			{
				Vector2 vector = (Vector2)value;
				vector.y = evt.newValue;
				onValueChanged(vector);
			});
			visualElement.Add(new Label("X:")
			{
				style = 
				{
					width = 20f,
					unityFont = v
				}
			});
			visualElement.Add(floatField2);
			visualElement.Add(new Label("Y:")
			{
				style = 
				{
					width = 20f,
					marginLeft = 8f,
					unityFont = v
				}
			});
			visualElement.Add(floatField3);
			return visualElement;
		}
		if (type == typeof(Vector3))
		{
			VisualElement visualElement2 = new VisualElement();
			visualElement2.style.flexDirection = FlexDirection.Row;
			FloatField floatField4 = new FloatField
			{
				value = ((Vector3)value).x
			};
			FloatField floatField5 = new FloatField
			{
				value = ((Vector3)value).y
			};
			FloatField floatField6 = new FloatField
			{
				value = ((Vector3)value).z
			};
			floatField4.style.unityFont = v;
			floatField5.style.unityFont = v;
			floatField6.style.unityFont = v;
			floatField4.RegisterValueChangedCallback(delegate(ChangeEvent<float> evt)
			{
				Vector3 vector = (Vector3)value;
				vector.x = evt.newValue;
				onValueChanged(vector);
			});
			floatField5.RegisterValueChangedCallback(delegate(ChangeEvent<float> evt)
			{
				Vector3 vector = (Vector3)value;
				vector.y = evt.newValue;
				onValueChanged(vector);
			});
			floatField6.RegisterValueChangedCallback(delegate(ChangeEvent<float> evt)
			{
				Vector3 vector = (Vector3)value;
				vector.z = evt.newValue;
				onValueChanged(vector);
			});
			visualElement2.Add(new Label("X:")
			{
				style = 
				{
					width = 20f,
					unityFont = v
				}
			});
			visualElement2.Add(floatField4);
			visualElement2.Add(new Label("Y:")
			{
				style = 
				{
					width = 20f,
					marginLeft = 8f,
					unityFont = v
				}
			});
			visualElement2.Add(floatField5);
			visualElement2.Add(new Label("Z:")
			{
				style = 
				{
					width = 20f,
					marginLeft = 8f,
					unityFont = v
				}
			});
			visualElement2.Add(floatField6);
			return visualElement2;
		}
		if (type == typeof(Color))
		{
			VisualElement visualElement3 = new VisualElement();
			visualElement3.style.flexDirection = FlexDirection.Row;
			FloatField floatField7 = new FloatField
			{
				value = ((Color)value).r
			};
			FloatField floatField8 = new FloatField
			{
				value = ((Color)value).g
			};
			FloatField floatField9 = new FloatField
			{
				value = ((Color)value).b
			};
			FloatField floatField10 = new FloatField
			{
				value = ((Color)value).a
			};
			floatField7.style.unityFont = v;
			floatField8.style.unityFont = v;
			floatField9.style.unityFont = v;
			floatField10.style.unityFont = v;
			floatField7.RegisterValueChangedCallback(delegate(ChangeEvent<float> evt)
			{
				Color color = (Color)value;
				color.r = evt.newValue;
				onValueChanged(color);
			});
			floatField8.RegisterValueChangedCallback(delegate(ChangeEvent<float> evt)
			{
				Color color = (Color)value;
				color.g = evt.newValue;
				onValueChanged(color);
			});
			floatField9.RegisterValueChangedCallback(delegate(ChangeEvent<float> evt)
			{
				Color color = (Color)value;
				color.b = evt.newValue;
				onValueChanged(color);
			});
			floatField10.RegisterValueChangedCallback(delegate(ChangeEvent<float> evt)
			{
				Color color = (Color)value;
				color.a = evt.newValue;
				onValueChanged(color);
			});
			visualElement3.Add(new Label("R:")
			{
				style = 
				{
					width = 20f,
					unityFont = v
				}
			});
			visualElement3.Add(floatField7);
			visualElement3.Add(new Label("G:")
			{
				style = 
				{
					width = 20f,
					marginLeft = 8f,
					unityFont = v
				}
			});
			visualElement3.Add(floatField8);
			visualElement3.Add(new Label("B:")
			{
				style = 
				{
					width = 20f,
					marginLeft = 8f,
					unityFont = v
				}
			});
			visualElement3.Add(floatField9);
			visualElement3.Add(new Label("A:")
			{
				style = 
				{
					width = 20f,
					marginLeft = 8f,
					unityFont = v
				}
			});
			visualElement3.Add(floatField10);
			return visualElement3;
		}
		if (type == typeof(AnimationCurve))
		{
			VisualElement visualElement4 = new VisualElement();
			visualElement4.style.flexDirection = FlexDirection.Row;
			visualElement4.Add(new Label("AnimationCurve (read-only)")
			{
				style = 
				{
					unityFont = v
				}
			});
			return visualElement4;
		}
		if (type.IsEnum)
		{
			VisualElement visualElement5 = new VisualElement();
			visualElement5.style.flexDirection = FlexDirection.Row;
			PopupField<string> popupField = new PopupField<string>(new List<string>(Enum.GetNames(type)), Enum.GetName(type, value), null, null);
			popupField.style.unityFont = v;
			popupField.RegisterValueChangedCallback(delegate(ChangeEvent<string> evt)
			{
				object obj = Enum.Parse(type, evt.newValue);
				onValueChanged(obj);
			});
			visualElement5.Add(popupField);
			return visualElement5;
		}
		if (type == typeof(LayerMask))
		{
			IntegerField integerField2 = new IntegerField();
			integerField2.value = ((LayerMask)value).value;
			integerField2.style.unityFont = v;
			integerField2.RegisterValueChangedCallback(delegate(ChangeEvent<int> evt)
			{
				onValueChanged(new LayerMask
				{
					value = evt.newValue
				});
			});
			return integerField2;
		}
		object value2 = value;
		return new Label(((value2 != null) ? value2.ToString() : null) ?? "null")
		{
			style = 
			{
				unityFont = v
			}
		};
	}

	// Token: 0x06001A84 RID: 6788 RVA: 0x00070A0C File Offset: 0x0006EC0C
	private void ToggleUI()
	{
		this.isVisible = !this.isVisible;
		this.root.style.display = (this.isVisible ? DisplayStyle.Flex : DisplayStyle.None);
	}

	// Token: 0x0400112A RID: 4394
	[SerializeField]
	private UIDocument uiDocument;

	// Token: 0x0400112B RID: 4395
	[Header("Settings")]
	[SerializeField]
	private GameSettings gameSettings;

	// Token: 0x0400112C RID: 4396
	[SerializeField]
	private PlayerSettings playerSettings;

	// Token: 0x0400112D RID: 4397
	[SerializeField]
	private CameraSettings cameraSettings;

	// Token: 0x0400112E RID: 4398
	[SerializeField]
	private SoundSettings soundSettings;

	// Token: 0x0400112F RID: 4399
	[SerializeField]
	private UIColorPalette colorPalette;

	// Token: 0x04001130 RID: 4400
	[SerializeField]
	private LobbySettings lobbySettings;

	// Token: 0x04001131 RID: 4401
	[SerializeField]
	private SpawnableSettings spawnableSettings;

	// Token: 0x04001132 RID: 4402
	[SerializeField]
	private ChallengeSettings challengeSettings;

	// Token: 0x04001133 RID: 4403
	private Dictionary<string, ScriptableObject> settingsDict;

	// Token: 0x04001134 RID: 4404
	private string _selectedSettingType;

	// Token: 0x04001135 RID: 4405
	private VisualElement root;

	// Token: 0x04001136 RID: 4406
	private bool isVisible;
}
