using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Token: 0x020002B0 RID: 688
public class FlexibleCreditsScreen : MonoBehaviour
{
	// Token: 0x0600183A RID: 6202 RVA: 0x00066914 File Offset: 0x00064B14
	private void Reset()
	{
		if (this.scrollRect == null)
		{
			this.scrollRect = base.GetComponent<ScrollRect>();
		}
		if (this.scrollRect != null && this.content == null)
		{
			this.content = this.scrollRect.content;
		}
	}

	// Token: 0x0600183B RID: 6203 RVA: 0x00066968 File Offset: 0x00064B68
	private void OnEnable()
	{
		if (this.scrollRect == null || this.content == null)
		{
			Debug.LogWarning("[FlexibleCreditsScreen] Assign ScrollRect and Content.");
			return;
		}
		this._scrollRoutine = base.StartCoroutine(this.PlayCreditsRoutine());
	}

	// Token: 0x0600183C RID: 6204 RVA: 0x000669A3 File Offset: 0x00064BA3
	private void OnDisable()
	{
		if (this._scrollRoutine != null)
		{
			base.StopCoroutine(this._scrollRoutine);
			this._scrollRoutine = null;
		}
	}

	// Token: 0x0600183D RID: 6205 RVA: 0x000669C0 File Offset: 0x00064BC0
	private IEnumerator PlayCreditsRoutine()
	{
		this.ClearContent();
		FlexibleCreditsScreen.CreditsData creditsData = this.LoadCreditsData();
		if (creditsData.HasEntries)
		{
			yield return base.StartCoroutine(this.BuildFromEntries(creditsData.entries));
		}
		else
		{
			if (this.linePrefab == null)
			{
				Debug.LogWarning("[FlexibleCreditsScreen] Assign Line Prefab for text-only credits.");
				yield break;
			}
			string[] lines = creditsData.ResolveLines();
			this.SpawnTextLines(lines);
		}
		this.scrollRect.StopMovement();
		this.scrollRect.velocity = Vector2.zero;
		this.scrollRect.verticalNormalizedPosition = 1f;
		Canvas.ForceUpdateCanvases();
		LayoutRebuilder.ForceRebuildLayoutImmediate(this.content);
		yield return null;
		Canvas.ForceUpdateCanvases();
		LayoutRebuilder.ForceRebuildLayoutImmediate(this.content);
		this.scrollRect.verticalNormalizedPosition = 1f;
		float height = ((this.scrollRect.viewport != null) ? this.scrollRect.viewport : this.scrollRect.GetComponent<RectTransform>()).rect.height;
		float height2 = this.content.rect.height;
		float num = Mathf.Max(0f, height2 - height);
		float duration = (num > 0f) ? (num / Mathf.Max(0.01f, this.scrollSpeedPixelsPerSecond)) : this.minScrollDuration;
		duration = Mathf.Max(duration, this.minScrollDuration);
		float elapsed = 0f;
		while (elapsed < duration)
		{
			elapsed += Time.unscaledDeltaTime;
			float num2 = Mathf.Clamp01(elapsed / duration);
			num2 = num2 * num2 * (3f - 2f * num2);
			this.scrollRect.verticalNormalizedPosition = Mathf.Lerp(1f, 0f, num2);
			yield return null;
		}
		this.scrollRect.verticalNormalizedPosition = 0f;
		yield break;
	}

	// Token: 0x0600183E RID: 6206 RVA: 0x000669CF File Offset: 0x00064BCF
	private IEnumerator BuildFromEntries(FlexibleCreditsScreen.CreditEntry[] entries)
	{
		foreach (FlexibleCreditsScreen.CreditEntry creditEntry in entries)
		{
			if (creditEntry != null)
			{
				if ((string.IsNullOrEmpty(creditEntry.type) ? "text" : creditEntry.type.Trim().ToLowerInvariant()) == "image")
				{
					FlexibleCreditsScreen.<>c__DisplayClass15_0 CS$<>8__locals1 = new FlexibleCreditsScreen.<>c__DisplayClass15_0();
					if (this.imagePrefab == null)
					{
						Debug.LogWarning("[FlexibleCreditsScreen] Image entry in JSON but Image Prefab is not assigned.");
					}
					else if (string.IsNullOrWhiteSpace(creditEntry.path))
					{
						Debug.LogWarning("[FlexibleCreditsScreen] Image entry missing path.");
					}
					else
					{
						CS$<>8__locals1.tex = null;
						yield return this.LoadTextureAsync(creditEntry.path.Trim(), delegate(Texture2D t)
						{
							CS$<>8__locals1.tex = t;
						});
						if (!(CS$<>8__locals1.tex == null))
						{
							GameObject instance = Object.Instantiate<GameObject>(this.imagePrefab, this.content);
							this.ApplyTextureToPrefab(instance, CS$<>8__locals1.tex);
							CS$<>8__locals1 = null;
						}
					}
				}
				else if (this.linePrefab == null)
				{
					Debug.LogWarning("[FlexibleCreditsScreen] Text entry skipped: Line Prefab not assigned.");
				}
				else if (this.linePrefab.GetComponentInChildren<TMP_Text>(true) == null)
				{
					Debug.LogWarning("[FlexibleCreditsScreen] Line prefab needs a TMP_Text in children.");
				}
				else
				{
					Object.Instantiate<GameObject>(this.linePrefab, this.content).GetComponentInChildren<TMP_Text>(true).text = (creditEntry.text ?? "");
				}
			}
		}
		FlexibleCreditsScreen.CreditEntry[] array = null;
		yield break;
	}

	// Token: 0x0600183F RID: 6207 RVA: 0x000669E8 File Offset: 0x00064BE8
	private void SpawnTextLines(string[] lines)
	{
		foreach (string text in lines)
		{
			Object.Instantiate<GameObject>(this.linePrefab, this.content).GetComponentInChildren<TMP_Text>(true).text = text;
		}
	}

	// Token: 0x06001840 RID: 6208 RVA: 0x00066A28 File Offset: 0x00064C28
	private void ApplyTextureToPrefab(GameObject instance, Texture2D tex)
	{
		RawImage componentInChildren = instance.GetComponentInChildren<RawImage>(true);
		if (componentInChildren != null)
		{
			componentInChildren.texture = tex;
			FlexibleCreditsScreen.ApplyAspectRatio(componentInChildren.rectTransform, tex);
			this._loadedTextures.Add(tex);
			return;
		}
		Image componentInChildren2 = instance.GetComponentInChildren<Image>(true);
		if (componentInChildren2 != null)
		{
			Sprite sprite = Sprite.Create(tex, new Rect(0f, 0f, (float)tex.width, (float)tex.height), new Vector2(0.5f, 0.5f), 100f);
			componentInChildren2.sprite = sprite;
			componentInChildren2.preserveAspect = true;
			FlexibleCreditsScreen.ApplyAspectRatio(componentInChildren2.rectTransform, tex);
			this._loadedTextures.Add(tex);
			this._loadedSprites.Add(sprite);
			return;
		}
		Debug.LogWarning("[FlexibleCreditsScreen] Image prefab needs RawImage or Image.");
		Object.Destroy(tex);
	}

	// Token: 0x06001841 RID: 6209 RVA: 0x00066AF4 File Offset: 0x00064CF4
	private static void ApplyAspectRatio(RectTransform target, Texture2D tex)
	{
		if (target == null || tex == null || tex.height == 0)
		{
			return;
		}
		float aspectRatio = (float)tex.width / (float)tex.height;
		AspectRatioFitter aspectRatioFitter = target.GetComponent<AspectRatioFitter>();
		if (aspectRatioFitter == null)
		{
			aspectRatioFitter = target.gameObject.AddComponent<AspectRatioFitter>();
		}
		aspectRatioFitter.aspectMode = AspectRatioFitter.AspectMode.WidthControlsHeight;
		aspectRatioFitter.aspectRatio = aspectRatio;
	}

	// Token: 0x06001842 RID: 6210 RVA: 0x00066B56 File Offset: 0x00064D56
	private IEnumerator LoadTextureAsync(string relativePathOrUrl, Action<Texture2D> done)
	{
		FlexibleCreditsScreen.<LoadTextureAsync>d__19 <LoadTextureAsync>d__ = new FlexibleCreditsScreen.<LoadTextureAsync>d__19(0);
		<LoadTextureAsync>d__.relativePathOrUrl = relativePathOrUrl;
		<LoadTextureAsync>d__.done = done;
		return <LoadTextureAsync>d__;
	}

	// Token: 0x06001843 RID: 6211 RVA: 0x00066B6C File Offset: 0x00064D6C
	private static string BuildTextureRequestUrl(string relativePathOrUrl)
	{
		string text = relativePathOrUrl.Trim();
		if (text.StartsWith("http://", StringComparison.OrdinalIgnoreCase) || text.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
		{
			return text;
		}
		string text2 = Path.Combine(Application.streamingAssetsPath, text).Replace('\\', '/');
		if (!text2.StartsWith("file://", StringComparison.OrdinalIgnoreCase))
		{
			return "file://" + text2;
		}
		return text2;
	}

	// Token: 0x06001844 RID: 6212 RVA: 0x00066BD0 File Offset: 0x00064DD0
	private void ClearContent()
	{
		foreach (Sprite sprite in this._loadedSprites)
		{
			if (sprite != null)
			{
				Object.Destroy(sprite);
			}
		}
		this._loadedSprites.Clear();
		foreach (Texture2D texture2D in this._loadedTextures)
		{
			if (texture2D != null)
			{
				Object.Destroy(texture2D);
			}
		}
		this._loadedTextures.Clear();
		for (int i = this.content.childCount - 1; i >= 0; i--)
		{
			Object.Destroy(this.content.GetChild(i).gameObject);
		}
	}

	// Token: 0x06001845 RID: 6213 RVA: 0x00066CC0 File Offset: 0x00064EC0
	private FlexibleCreditsScreen.CreditsData LoadCreditsData()
	{
		string text = null;
		if (this.creditsJson != null)
		{
			text = this.creditsJson.text;
		}
		else if (!string.IsNullOrWhiteSpace(this.streamingAssetsFileName))
		{
			string text2 = Path.Combine(Application.streamingAssetsPath, this.streamingAssetsFileName);
			if (File.Exists(text2))
			{
				text = File.ReadAllText(text2);
			}
			else
			{
				Debug.LogWarning("[FlexibleCreditsScreen] Missing file: " + text2);
			}
		}
		if (string.IsNullOrWhiteSpace(text))
		{
			Debug.LogWarning("[FlexibleCreditsScreen] No credits JSON; assign TextAsset or StreamingAssets file.");
			return new FlexibleCreditsScreen.CreditsData();
		}
		text = FlexibleCreditsScreen.StripUtf8Bom(text);
		FlexibleCreditsScreen.CreditsData result;
		try
		{
			CreditsFileJson creditsFileJson = JsonConvert.DeserializeObject<CreditsFileJson>(text);
			if (creditsFileJson == null)
			{
				result = new FlexibleCreditsScreen.CreditsData();
			}
			else
			{
				FlexibleCreditsScreen.CreditsData creditsData = new FlexibleCreditsScreen.CreditsData
				{
					body = creditsFileJson.body,
					lines = creditsFileJson.lines
				};
				if (creditsFileJson.entries != null && creditsFileJson.entries.Length != 0)
				{
					creditsData.entries = new FlexibleCreditsScreen.CreditEntry[creditsFileJson.entries.Length];
					for (int i = 0; i < creditsFileJson.entries.Length; i++)
					{
						CreditEntryJson creditEntryJson = creditsFileJson.entries[i];
						creditsData.entries[i] = new FlexibleCreditsScreen.CreditEntry
						{
							type = ((creditEntryJson != null) ? creditEntryJson.type : null),
							text = ((creditEntryJson != null) ? creditEntryJson.text : null),
							path = ((creditEntryJson != null) ? creditEntryJson.path : null)
						};
					}
				}
				result = creditsData;
			}
		}
		catch (Exception ex)
		{
			Debug.LogWarning("[FlexibleCreditsScreen] JSON parse failed: " + ex.Message);
			result = new FlexibleCreditsScreen.CreditsData();
		}
		return result;
	}

	// Token: 0x06001846 RID: 6214 RVA: 0x00066E48 File Offset: 0x00065048
	private static string StripUtf8Bom(string s)
	{
		if (string.IsNullOrEmpty(s) || s[0] != '﻿')
		{
			return s;
		}
		return s.Substring(1);
	}

	// Token: 0x04000F99 RID: 3993
	[Header("Scroll")]
	[SerializeField]
	private ScrollRect scrollRect;

	// Token: 0x04000F9A RID: 3994
	[Tooltip("Pixels per second. Scroll duration follows content height.")]
	[SerializeField]
	private float scrollSpeedPixelsPerSecond = 48f;

	// Token: 0x04000F9B RID: 3995
	[Tooltip("Minimum time to scroll when content is shorter than the viewport.")]
	[SerializeField]
	private float minScrollDuration = 4f;

	// Token: 0x04000F9C RID: 3996
	[Header("Layout")]
	[SerializeField]
	private RectTransform content;

	// Token: 0x04000F9D RID: 3997
	[SerializeField]
	private GameObject linePrefab;

	// Token: 0x04000F9E RID: 3998
	[SerializeField]
	private GameObject imagePrefab;

	// Token: 0x04000F9F RID: 3999
	[Header("Credits JSON")]
	[Tooltip("If set, this text is parsed (Editor and builds).")]
	[SerializeField]
	private TextAsset creditsJson;

	// Token: 0x04000FA0 RID: 4000
	[Tooltip("Optional: file under StreamingAssets, e.g. credits.json. Used when creditsJson is null.")]
	[SerializeField]
	private string streamingAssetsFileName = "credits.json";

	// Token: 0x04000FA1 RID: 4001
	private Coroutine _scrollRoutine;

	// Token: 0x04000FA2 RID: 4002
	private readonly List<Texture2D> _loadedTextures = new List<Texture2D>();

	// Token: 0x04000FA3 RID: 4003
	private readonly List<Sprite> _loadedSprites = new List<Sprite>();

	// Token: 0x020002B1 RID: 689
	[Serializable]
	private class CreditEntry
	{
		// Token: 0x04000FA4 RID: 4004
		public string type;

		// Token: 0x04000FA5 RID: 4005
		public string text;

		// Token: 0x04000FA6 RID: 4006
		public string path;
	}

	// Token: 0x020002B2 RID: 690
	[Serializable]
	private class CreditsData
	{
		// Token: 0x17000228 RID: 552
		// (get) Token: 0x06001849 RID: 6217 RVA: 0x00066EA8 File Offset: 0x000650A8
		public bool HasEntries
		{
			get
			{
				return this.entries != null && this.entries.Length != 0;
			}
		}

		// Token: 0x0600184A RID: 6218 RVA: 0x00066EC0 File Offset: 0x000650C0
		public string[] ResolveLines()
		{
			if (this.lines != null && this.lines.Length != 0)
			{
				return this.lines;
			}
			if (!string.IsNullOrEmpty(this.body))
			{
				return this.body.Split(new string[]
				{
					"\r\n",
					"\r",
					"\n"
				}, StringSplitOptions.None);
			}
			return Array.Empty<string>();
		}

		// Token: 0x04000FA7 RID: 4007
		public string body;

		// Token: 0x04000FA8 RID: 4008
		public string[] lines;

		// Token: 0x04000FA9 RID: 4009
		public FlexibleCreditsScreen.CreditEntry[] entries;
	}
}
