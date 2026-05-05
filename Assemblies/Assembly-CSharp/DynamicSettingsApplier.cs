using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using UnityEngine;

// Token: 0x02000010 RID: 16
public static class DynamicSettingsApplier
{
	// Token: 0x0600002C RID: 44 RVA: 0x00002C14 File Offset: 0x00000E14
	public static void ApplySettings(SettingResponse[] settings)
	{
		GameSettings orLoadSettings = DynamicSettingsApplier.GetOrLoadSettings<GameSettings>("GameSettings");
		DynamicSettingsApplier.debugMode = (orLoadSettings != null && orLoadSettings.apiDebugMode);
		foreach (SettingResponse settingResponse in settings)
		{
			try
			{
				DynamicSettingsApplier.ApplySetting(settingResponse);
			}
			catch (Exception ex)
			{
				Debug.LogWarning("[DynamicSettingsApplier] Error processing setting '" + settingResponse.setting_key + "': " + ex.Message);
			}
		}
		foreach (ScriptableObject scriptableObject in DynamicSettingsApplier._settingsCache.Values)
		{
			GameSettings gameSettings = scriptableObject as GameSettings;
			if (gameSettings == null || gameSettings.useAPIUpdates)
			{
				MethodInfo method = scriptableObject.GetType().GetMethod("NotifyChanged", BindingFlags.Instance | BindingFlags.Public);
				if (method != null)
				{
					method.Invoke(scriptableObject, null);
				}
			}
		}
		DynamicSettingsApplier._settingsCache.Clear();
	}

	// Token: 0x0600002D RID: 45 RVA: 0x00002D18 File Offset: 0x00000F18
	private static void ApplySetting(SettingResponse setting)
	{
		string setting_key = setting.setting_key;
		string text = DynamicSettingsApplier.NormalizeKey(setting_key);
		if (text.StartsWith("floor") && setting.setting_type == "json")
		{
			DynamicSettingsApplier.ApplyFloorData(setting_key, setting.setting_value);
			return;
		}
		if (text == "itemprices" && setting.setting_type == "json")
		{
			DynamicSettingsApplier.ApplyItemPrices(setting.setting_value);
			return;
		}
		if ((text == "auxiliarymoneypercentage" || text == "auxillarymoneypercentage") && setting.setting_type == "json")
		{
			DynamicSettingsApplier.ApplyAuxiliaryMoneyPercentage(setting.setting_value);
			return;
		}
		ScriptableObject scriptableObject = null;
		FieldInfo fieldInfo = null;
		GameSettings orLoadSettings = DynamicSettingsApplier.GetOrLoadSettings<GameSettings>("GameSettings");
		if (orLoadSettings != null && orLoadSettings.useAPIUpdates)
		{
			fieldInfo = DynamicSettingsApplier.FindField(orLoadSettings.GetType(), text);
			if (fieldInfo != null)
			{
				scriptableObject = orLoadSettings;
			}
		}
		if (fieldInfo == null)
		{
			LobbySettings orLoadSettings2 = DynamicSettingsApplier.GetOrLoadSettings<LobbySettings>("LobbySettings");
			if (orLoadSettings2 != null)
			{
				fieldInfo = DynamicSettingsApplier.FindField(orLoadSettings2.GetType(), text);
				if (fieldInfo != null)
				{
					scriptableObject = orLoadSettings2;
				}
			}
		}
		if (fieldInfo == null)
		{
			PlayerSettings orLoadSettings3 = DynamicSettingsApplier.GetOrLoadSettings<PlayerSettings>("PlayerSettings");
			if (orLoadSettings3 != null)
			{
				fieldInfo = DynamicSettingsApplier.FindField(orLoadSettings3.GetType(), text);
				if (fieldInfo != null)
				{
					scriptableObject = orLoadSettings3;
				}
			}
		}
		if (fieldInfo == null)
		{
			ChallengeSettings orLoadSettings4 = DynamicSettingsApplier.GetOrLoadSettings<ChallengeSettings>("ChallengeSettings");
			if (orLoadSettings4 != null)
			{
				fieldInfo = DynamicSettingsApplier.FindField(orLoadSettings4.GetType(), text);
				if (fieldInfo != null)
				{
					scriptableObject = orLoadSettings4;
				}
			}
		}
		if (fieldInfo == null)
		{
			ItemPriceSettings orLoadSettings5 = DynamicSettingsApplier.GetOrLoadSettings<ItemPriceSettings>("ItemPriceSettings");
			if (orLoadSettings5 != null)
			{
				fieldInfo = DynamicSettingsApplier.FindField(orLoadSettings5.GetType(), text);
				if (fieldInfo != null)
				{
					scriptableObject = orLoadSettings5;
				}
			}
		}
		if (fieldInfo == null || scriptableObject == null)
		{
			Debug.LogWarning("[DynamicSettingsApplier] No matching field found for setting key: '" + setting_key + "'");
			return;
		}
		DynamicSettingsApplier._settingsCache[scriptableObject.GetType()] = scriptableObject;
		object obj = DynamicSettingsApplier.ParseValue(setting.setting_value, setting.setting_type, fieldInfo.FieldType);
		if (obj != null)
		{
			fieldInfo.SetValue(scriptableObject, obj);
			DynamicSettingsApplier.LogDebug(string.Format("[DynamicSettingsApplier] Applied '{0}' = {1} to {2}.{3}", new object[]
			{
				setting_key,
				obj,
				scriptableObject.GetType().Name,
				fieldInfo.Name
			}));
		}
	}

	// Token: 0x0600002E RID: 46 RVA: 0x00002F77 File Offset: 0x00001177
	private static void LogDebug(string message)
	{
		if (DynamicSettingsApplier.debugMode)
		{
			Debug.Log(message);
		}
	}

	// Token: 0x0600002F RID: 47 RVA: 0x00002F86 File Offset: 0x00001186
	private static T GetOrLoadSettings<T>(string resourceName) where T : ScriptableObject
	{
		return Resources.Load<T>(resourceName);
	}

	// Token: 0x06000030 RID: 48 RVA: 0x00002F90 File Offset: 0x00001190
	private static FieldInfo FindField(Type type, string normalizedKey)
	{
		foreach (FieldInfo fieldInfo in type.GetFields(BindingFlags.Instance | BindingFlags.Public))
		{
			if (DynamicSettingsApplier.NormalizeKey(fieldInfo.Name) == normalizedKey)
			{
				return fieldInfo;
			}
		}
		return null;
	}

	// Token: 0x06000031 RID: 49 RVA: 0x00002FCE File Offset: 0x000011CE
	private static string NormalizeKey(string key)
	{
		return key.ToLower().Replace("_", "").Replace("-", "");
	}

	// Token: 0x06000032 RID: 50 RVA: 0x00002FF4 File Offset: 0x000011F4
	private static object ParseValue(string value, string settingType, Type targetType)
	{
		if (string.IsNullOrEmpty(value))
		{
			return null;
		}
		try
		{
			if (settingType == "json")
			{
				return DynamicSettingsApplier.ParseJsonValue(value, targetType);
			}
			if (targetType == typeof(int))
			{
				int num;
				if (int.TryParse(value, out num))
				{
					return num;
				}
			}
			else if (targetType == typeof(long))
			{
				long num2;
				if (long.TryParse(value, out num2))
				{
					return num2;
				}
			}
			else if (targetType == typeof(float))
			{
				float num3;
				if (float.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out num3))
				{
					return num3;
				}
			}
			else if (targetType == typeof(double))
			{
				double num4;
				if (double.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out num4))
				{
					return num4;
				}
			}
			else if (targetType == typeof(bool))
			{
				bool flag;
				if (bool.TryParse(value, out flag))
				{
					return flag;
				}
			}
			else
			{
				if (targetType == typeof(string))
				{
					return value;
				}
				if (targetType.IsArray)
				{
					return DynamicSettingsApplier.ParseArrayValue(value, settingType, targetType);
				}
				if (targetType.IsGenericType && targetType.GetGenericTypeDefinition() == typeof(List<>))
				{
					return DynamicSettingsApplier.ParseListValue(value, settingType, targetType);
				}
			}
		}
		catch (Exception ex)
		{
			Debug.LogError(string.Concat(new string[]
			{
				"[DynamicSettingsApplier] Error parsing value '",
				value,
				"' to type ",
				targetType.Name,
				": ",
				ex.Message
			}));
		}
		return null;
	}

	// Token: 0x06000033 RID: 51 RVA: 0x000031CC File Offset: 0x000013CC
	private static object ParseJsonValue(string jsonValue, Type targetType)
	{
		if (targetType.IsArray)
		{
			return DynamicSettingsApplier.ParseArrayFromJson(jsonValue, targetType);
		}
		if (targetType.IsGenericType && targetType.GetGenericTypeDefinition() == typeof(List<>))
		{
			return DynamicSettingsApplier.ParseListFromJson(jsonValue, targetType);
		}
		object result;
		try
		{
			result = JsonUtility.FromJson(jsonValue, targetType);
		}
		catch
		{
			Debug.LogWarning("[DynamicSettingsApplier] Could not parse JSON to type " + targetType.Name);
			result = null;
		}
		return result;
	}

	// Token: 0x06000034 RID: 52 RVA: 0x00003248 File Offset: 0x00001448
	private static object ParseArrayValue(string value, string settingType, Type arrayType)
	{
		if (settingType == "json")
		{
			return DynamicSettingsApplier.ParseArrayFromJson(value, arrayType);
		}
		Type elementType = arrayType.GetElementType();
		string[] array = value.Split(',', StringSplitOptions.None);
		Array array2 = Array.CreateInstance(elementType, array.Length);
		for (int i = 0; i < array.Length; i++)
		{
			object obj = DynamicSettingsApplier.ParseValue(array[i].Trim(), "number", elementType);
			if (obj != null)
			{
				array2.SetValue(obj, i);
			}
		}
		return array2;
	}

	// Token: 0x06000035 RID: 53 RVA: 0x000032B8 File Offset: 0x000014B8
	private static object ParseListValue(string value, string settingType, Type listType)
	{
		Type targetType = listType.GetGenericArguments()[0];
		object obj = Activator.CreateInstance(listType);
		if (settingType == "json")
		{
			return DynamicSettingsApplier.ParseListFromJson(value, listType);
		}
		MethodInfo method = listType.GetMethod("Add");
		string[] array = value.Split(',', StringSplitOptions.None);
		for (int i = 0; i < array.Length; i++)
		{
			object obj2 = DynamicSettingsApplier.ParseValue(array[i].Trim(), "number", targetType);
			if (obj2 != null)
			{
				method.Invoke(obj, new object[]
				{
					obj2
				});
			}
		}
		return obj;
	}

	// Token: 0x06000036 RID: 54 RVA: 0x00003340 File Offset: 0x00001540
	private static object ParseArrayFromJson(string jsonValue, Type arrayType)
	{
		Type elementType = arrayType.GetElementType();
		try
		{
			if (elementType == typeof(float) || elementType == typeof(int) || elementType == typeof(long))
			{
				return DynamicSettingsApplier.ParseNumberedFieldsArray(jsonValue, arrayType);
			}
			Type type = typeof(DynamicSettingsApplier.JsonArrayWrapper<>).MakeGenericType(new Type[]
			{
				elementType
			});
			object obj = JsonUtility.FromJson(jsonValue, type);
			FieldInfo field = type.GetField("items", BindingFlags.Instance | BindingFlags.Public);
			if (field != null)
			{
				object value = field.GetValue(obj);
				if (value != null)
				{
					IList list = value as IList;
					if (list != null)
					{
						Array array = Array.CreateInstance(elementType, list.Count);
						list.CopyTo(array, 0);
						return array;
					}
				}
			}
		}
		catch
		{
		}
		return DynamicSettingsApplier.ParseNumberedFieldsArray(jsonValue, arrayType);
	}

	// Token: 0x06000037 RID: 55 RVA: 0x00003428 File Offset: 0x00001628
	private static object ParseListFromJson(string jsonValue, Type listType)
	{
		Type type = listType.GetGenericArguments()[0];
		object obj = Activator.CreateInstance(listType);
		MethodInfo method = listType.GetMethod("Add");
		try
		{
			Type type2 = typeof(DynamicSettingsApplier.JsonArrayWrapper<>).MakeGenericType(new Type[]
			{
				type
			});
			object obj2 = JsonUtility.FromJson(jsonValue, type2);
			FieldInfo field = type2.GetField("items", BindingFlags.Instance | BindingFlags.Public);
			if (field != null)
			{
				IList list = field.GetValue(obj2) as IList;
				if (list != null)
				{
					foreach (object obj3 in list)
					{
						method.Invoke(obj, new object[]
						{
							obj3
						});
					}
					return obj;
				}
			}
		}
		catch
		{
		}
		if (type == typeof(float) || type == typeof(int) || type == typeof(long))
		{
			object obj4 = DynamicSettingsApplier.ParseNumberedFieldsArray(jsonValue, type.MakeArrayType());
			if (obj4 != null)
			{
				foreach (object obj5 in (obj4 as IList))
				{
					method.Invoke(obj, new object[]
					{
						obj5
					});
				}
				return obj;
			}
		}
		return null;
	}

	// Token: 0x06000038 RID: 56 RVA: 0x000035B8 File Offset: 0x000017B8
	private static object ParseNumberedFieldsArray(string jsonValue, Type arrayType)
	{
		Type elementType = arrayType.GetElementType();
		List<object> list = new List<object>();
		bool flag = false;
		for (int i = 0; i < 100; i++)
		{
			string fieldName = string.Format("quota{0}", i);
			float? num = DynamicSettingsApplier.ExtractFieldValue<float>(jsonValue, fieldName);
			if (num != null)
			{
				list.Add(Convert.ChangeType(num.Value, elementType));
				flag = true;
			}
			else if (flag)
			{
				break;
			}
		}
		if (list.Count == 0)
		{
			foreach (string fieldName2 in new string[]
			{
				"firstDay",
				"secondDay",
				"thirdDay"
			})
			{
				float? num2 = DynamicSettingsApplier.ExtractFieldValue<float>(jsonValue, fieldName2);
				if (num2 != null)
				{
					list.Add(Convert.ChangeType(num2.Value, elementType));
				}
			}
		}
		if (list.Count > 0)
		{
			Array array2 = Array.CreateInstance(elementType, list.Count);
			for (int k = 0; k < list.Count; k++)
			{
				array2.SetValue(list[k], k);
			}
			return array2;
		}
		return null;
	}

	// Token: 0x06000039 RID: 57 RVA: 0x000036D4 File Offset: 0x000018D4
	private static T? ExtractFieldValue<T>(string json, string fieldName) where T : struct
	{
		try
		{
			string value = "\"" + fieldName + "\"";
			int num = json.IndexOf(value, StringComparison.Ordinal);
			if (num == -1)
			{
				return null;
			}
			int num2 = json.IndexOf(':', num);
			if (num2 == -1)
			{
				return null;
			}
			int num3 = num2 + 1;
			while (num3 < json.Length && char.IsWhiteSpace(json[num3]))
			{
				num3++;
			}
			if (num3 >= json.Length)
			{
				return null;
			}
			int i = num3;
			bool flag = false;
			while (i < json.Length)
			{
				char c = json[i];
				if (c == '"')
				{
					flag = !flag;
				}
				else if (!flag && (c == ',' || c == '}'))
				{
					break;
				}
				i++;
			}
			if (i > num3)
			{
				string s = json.Substring(num3, i - num3).Trim().TrimEnd(new char[]
				{
					',',
					'}'
				});
				float num4;
				if (typeof(T) == typeof(float) && float.TryParse(s, NumberStyles.Float, CultureInfo.InvariantCulture, out num4))
				{
					return new T?((T)((object)num4));
				}
				int num5;
				if (typeof(T) == typeof(int) && int.TryParse(s, out num5))
				{
					return new T?((T)((object)num5));
				}
				long num6;
				if (typeof(T) == typeof(long) && long.TryParse(s, out num6))
				{
					return new T?((T)((object)num6));
				}
			}
		}
		catch (Exception ex)
		{
			Debug.LogWarning("[DynamicSettingsApplier] Error extracting field '" + fieldName + "': " + ex.Message);
		}
		return null;
	}

	// Token: 0x0600003A RID: 58 RVA: 0x000038E4 File Offset: 0x00001AE4
	private static void ApplyFloorData(string floorKey, string jsonValue)
	{
		GameSettings orLoadSettings = DynamicSettingsApplier.GetOrLoadSettings<GameSettings>("GameSettings");
		if (orLoadSettings == null || !orLoadSettings.useAPIUpdates)
		{
			return;
		}
		try
		{
			int num;
			if (!int.TryParse(floorKey.Substring(5), out num))
			{
				Debug.LogWarning("[DynamicSettingsApplier] Could not parse floor index from '" + floorKey + "'");
			}
			else
			{
				int num2 = num - 1;
				while (orLoadSettings.floorData.Count <= num2)
				{
					orLoadSettings.floorData.Add(new GameSettings.CasinoFloorData());
				}
				GameSettings.CasinoFloorData obj = orLoadSettings.floorData[num2];
				Type typeFromHandle = typeof(GameSettings.CasinoFloorData);
				foreach (FieldInfo fieldInfo in typeFromHandle.GetFields(BindingFlags.Instance | BindingFlags.Public))
				{
					string name = fieldInfo.Name;
					if (fieldInfo.FieldType == typeof(float))
					{
						float? num3 = DynamicSettingsApplier.ExtractFieldValue<float>(jsonValue, name);
						if (num3 != null)
						{
							fieldInfo.SetValue(obj, num3.Value);
						}
					}
					else if (fieldInfo.FieldType == typeof(int))
					{
						int? num4 = DynamicSettingsApplier.ExtractFieldValue<int>(jsonValue, name);
						if (num4 != null)
						{
							fieldInfo.SetValue(obj, num4.Value);
						}
					}
					else if (fieldInfo.FieldType == typeof(long))
					{
						long? num5 = DynamicSettingsApplier.ExtractFieldValue<long>(jsonValue, name);
						if (num5 != null)
						{
							fieldInfo.SetValue(obj, num5.Value);
						}
					}
				}
				FieldInfo field = typeFromHandle.GetField("shreddingBodyPrice");
				if (field != null)
				{
					float? num6 = DynamicSettingsApplier.ExtractFieldValue<float>(jsonValue, "ShreddingBodyPrice");
					float? num7 = DynamicSettingsApplier.ExtractFieldValue<float>(jsonValue, "shreddingBodyPrice");
					if (num6 != null && num6.Value != 0f)
					{
						field.SetValue(obj, num6.Value);
					}
					else if (num7 != null)
					{
						field.SetValue(obj, num7.Value);
					}
				}
				DynamicSettingsApplier._settingsCache[typeof(GameSettings)] = orLoadSettings;
				DynamicSettingsApplier.LogDebug(string.Format("[DynamicSettingsApplier] Updated floor {0} from '{1}'", num2, floorKey));
			}
		}
		catch (Exception ex)
		{
			Debug.LogError("[DynamicSettingsApplier] Error parsing floor data '" + floorKey + "': " + ex.Message);
		}
	}

	// Token: 0x0600003B RID: 59 RVA: 0x00003B58 File Offset: 0x00001D58
	private static void ApplyItemPrices(string jsonValue)
	{
		ItemPriceSettings orLoadSettings = DynamicSettingsApplier.GetOrLoadSettings<ItemPriceSettings>("ItemPriceSettings");
		if (orLoadSettings == null)
		{
			return;
		}
		try
		{
			Type typeFromHandle = typeof(ItemPriceSettings);
			FieldInfo field = typeFromHandle.GetField("itemPrices", BindingFlags.Instance | BindingFlags.NonPublic);
			if (field == null)
			{
				Debug.LogError("[DynamicSettingsApplier] Could not find itemPrices field in ItemPriceSettings");
			}
			else
			{
				IList list = field.GetValue(orLoadSettings) as IList;
				if (list == null)
				{
					Debug.LogError("[DynamicSettingsApplier] itemPrices field is not a list");
				}
				else
				{
					Type nestedType = typeFromHandle.GetNestedType("ItemPriceData", BindingFlags.Public | BindingFlags.NonPublic);
					if (nestedType == null)
					{
						Debug.LogError("[DynamicSettingsApplier] Could not find ItemPriceData nested type");
					}
					else
					{
						FieldInfo field2 = nestedType.GetField("basePrice");
						FieldInfo field3 = nestedType.GetField("priceIncreasePerFloor");
						int num = 0;
						for (int i = 0; i < 100; i++)
						{
							string str = i.ToString();
							string value = "\"" + str + "\"";
							int num2 = jsonValue.IndexOf(value, StringComparison.Ordinal);
							if (num2 == -1)
							{
								break;
							}
							int num3 = jsonValue.IndexOf(':', num2);
							if (num3 != -1)
							{
								int num4 = num3 + 1;
								while (num4 < jsonValue.Length && char.IsWhiteSpace(jsonValue[num4]))
								{
									num4++;
								}
								if (num4 < jsonValue.Length && jsonValue[num4] == '"')
								{
									num4++;
									int num5 = num4;
									while (num5 < jsonValue.Length && (jsonValue[num5] != '"' || (num5 != 0 && jsonValue[num5 - 1] == '\\')))
									{
										num5++;
									}
									if (num5 < jsonValue.Length)
									{
										string text = jsonValue.Substring(num4, num5 - num4);
										text = text.Replace("\\\"", "\"");
										string text2 = "{" + text + "}";
										try
										{
											DynamicSettingsApplier.ItemPriceDataJson itemPriceDataJson = JsonUtility.FromJson<DynamicSettingsApplier.ItemPriceDataJson>(text2);
											if (itemPriceDataJson != null)
											{
												if (i < list.Count)
												{
													object obj = list[i];
													if (obj != null)
													{
														if (field2 != null)
														{
															field2.SetValue(obj, itemPriceDataJson.basePrice);
														}
														if (field3 != null)
														{
															field3.SetValue(obj, itemPriceDataJson.priceIncreasePerFloor);
														}
														num++;
													}
												}
											}
										}
										catch (Exception ex)
										{
											Debug.LogWarning(string.Format("[DynamicSettingsApplier] Failed to parse item price at index {0}: {1}. Error: {2}", i, text2, ex.Message));
										}
									}
								}
							}
						}
						FieldInfo field4 = typeFromHandle.GetField("_priceCache", BindingFlags.Instance | BindingFlags.NonPublic);
						if (field4 != null)
						{
							field4.SetValue(orLoadSettings, null);
						}
						DynamicSettingsApplier._settingsCache[typeof(ItemPriceSettings)] = orLoadSettings;
						DynamicSettingsApplier.LogDebug(string.Format("[DynamicSettingsApplier] Updated {0} item prices", num));
					}
				}
			}
		}
		catch (Exception ex2)
		{
			Debug.LogError("[DynamicSettingsApplier] Error parsing item prices: " + ex2.Message + "\n" + ex2.StackTrace);
		}
	}

	// Token: 0x0600003C RID: 60 RVA: 0x00003E6C File Offset: 0x0000206C
	private static void ApplyAuxiliaryMoneyPercentage(string jsonValue)
	{
		GameSettings orLoadSettings = DynamicSettingsApplier.GetOrLoadSettings<GameSettings>("GameSettings");
		if (orLoadSettings == null || !orLoadSettings.useAPIUpdates)
		{
			return;
		}
		try
		{
			DynamicSettingsApplier.AuxiliaryMoneyData auxiliaryMoneyData = JsonUtility.FromJson<DynamicSettingsApplier.AuxiliaryMoneyData>(jsonValue);
			if (auxiliaryMoneyData == null)
			{
				Debug.LogWarning("[DynamicSettingsApplier] Could not parse auxiliaryMoneyPercentage JSON");
			}
			else
			{
				if (orLoadSettings.auxiliaryMoneyPercentage == null || orLoadSettings.auxiliaryMoneyPercentage.Length < 3)
				{
					orLoadSettings.auxiliaryMoneyPercentage = new float[3];
				}
				orLoadSettings.auxiliaryMoneyPercentage[0] = auxiliaryMoneyData.firstDay;
				orLoadSettings.auxiliaryMoneyPercentage[1] = auxiliaryMoneyData.secondDay;
				orLoadSettings.auxiliaryMoneyPercentage[2] = auxiliaryMoneyData.thirdDay;
				DynamicSettingsApplier._settingsCache[typeof(GameSettings)] = orLoadSettings;
				DynamicSettingsApplier.LogDebug(string.Format("[DynamicSettingsApplier] Updated auxiliaryMoneyPercentage: [{0}, {1}, {2}]", auxiliaryMoneyData.firstDay, auxiliaryMoneyData.secondDay, auxiliaryMoneyData.thirdDay));
			}
		}
		catch (Exception ex)
		{
			Debug.LogError("[DynamicSettingsApplier] Error parsing auxiliaryMoneyPercentage: " + ex.Message + "\n" + ex.StackTrace);
		}
	}

	// Token: 0x04000051 RID: 81
	private static readonly Dictionary<Type, ScriptableObject> _settingsCache = new Dictionary<Type, ScriptableObject>();

	// Token: 0x04000052 RID: 82
	public static bool debugMode = false;

	// Token: 0x02000011 RID: 17
	[Serializable]
	private class AuxiliaryMoneyData
	{
		// Token: 0x04000053 RID: 83
		public float firstDay;

		// Token: 0x04000054 RID: 84
		public float secondDay;

		// Token: 0x04000055 RID: 85
		public float thirdDay;
	}

	// Token: 0x02000012 RID: 18
	[Serializable]
	private class ItemPricesWrapper
	{
		// Token: 0x04000056 RID: 86
		public DynamicSettingsApplier.ItemPriceDataJson[] items;
	}

	// Token: 0x02000013 RID: 19
	[Serializable]
	private class ItemPriceDataJson
	{
		// Token: 0x04000057 RID: 87
		public int basePrice;

		// Token: 0x04000058 RID: 88
		public int priceIncreasePerFloor;
	}

	// Token: 0x02000014 RID: 20
	[Serializable]
	private class JsonArrayWrapper<T>
	{
		// Token: 0x04000059 RID: 89
		public T[] items;
	}
}
