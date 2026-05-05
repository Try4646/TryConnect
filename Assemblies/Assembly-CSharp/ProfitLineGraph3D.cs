using System;
using System.Collections.Generic;
using DG.Tweening;
using Extensions;
using Shapes;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Token: 0x0200019C RID: 412
public class ProfitLineGraph3D : MonoBehaviour
{
	// Token: 0x06000F23 RID: 3875 RVA: 0x0003EF64 File Offset: 0x0003D164
	private void OnEnable()
	{
		if (!Application.isPlaying)
		{
			return;
		}
		PayoutTracker instance = NetworkSingleton<PayoutTracker>.Instance;
		if (instance != null)
		{
			instance.OnPayoutRecorded += this.OnPayoutRecorded;
		}
	}

	// Token: 0x06000F24 RID: 3876 RVA: 0x0003EF9C File Offset: 0x0003D19C
	public void ResetAndAnimate()
	{
		DOTween.Kill(this, false);
		foreach (Polyline polyline in this.playerPolylines)
		{
			if (polyline)
			{
				polyline.gameObject.SetActive(false);
			}
		}
		foreach (GameObject gameObject in this.playerEndMarkers)
		{
			if (gameObject)
			{
				gameObject.SetActive(false);
			}
		}
		foreach (GameObject gameObject2 in this.playerEndTexts)
		{
			if (gameObject2)
			{
				gameObject2.SetActive(false);
			}
		}
		foreach (GameObject gameObject3 in this.playerNameTexts)
		{
			if (gameObject3)
			{
				gameObject3.SetActive(false);
			}
		}
		if (this.showCornerMarkers)
		{
			foreach (GameObject gameObject4 in this.cornerMarkerPool)
			{
				if (gameObject4)
				{
					gameObject4.SetActive(false);
				}
			}
		}
		this.AnimatePolylines();
	}

	// Token: 0x06000F25 RID: 3877 RVA: 0x0003F144 File Offset: 0x0003D344
	private void OnDisable()
	{
		if (Application.isPlaying)
		{
			PayoutTracker instance = NetworkSingleton<PayoutTracker>.Instance;
			if (instance != null)
			{
				instance.OnPayoutRecorded -= this.OnPayoutRecorded;
			}
		}
		DOTween.Kill(this, false);
		this.DestroyAllSpheres();
	}

	// Token: 0x06000F26 RID: 3878 RVA: 0x000048A7 File Offset: 0x00002AA7
	private void OnPayoutRecorded(PayoutRecord _)
	{
	}

	// Token: 0x06000F27 RID: 3879 RVA: 0x0003F188 File Offset: 0x0003D388
	private void DestroyAllSpheres()
	{
		for (int i = 0; i < this.playerEndMarkers.Count; i++)
		{
			if (this.playerEndMarkers[i])
			{
				if (Application.isPlaying)
				{
					Object.Destroy(this.playerEndMarkers[i]);
				}
				else
				{
					Object.DestroyImmediate(this.playerEndMarkers[i]);
				}
			}
		}
		this.playerEndMarkers.Clear();
		for (int j = 0; j < this.playerEndTexts.Count; j++)
		{
			if (this.playerEndTexts[j])
			{
				if (Application.isPlaying)
				{
					Object.Destroy(this.playerEndTexts[j]);
				}
				else
				{
					Object.DestroyImmediate(this.playerEndTexts[j]);
				}
			}
		}
		this.playerEndTexts.Clear();
		for (int k = 0; k < this.playerNameTexts.Count; k++)
		{
			if (this.playerNameTexts[k])
			{
				if (Application.isPlaying)
				{
					Object.Destroy(this.playerNameTexts[k]);
				}
				else
				{
					Object.DestroyImmediate(this.playerNameTexts[k]);
				}
			}
		}
		this.playerNameTexts.Clear();
		for (int l = 0; l < this.cornerMarkerPool.Count; l++)
		{
			if (this.cornerMarkerPool[l])
			{
				if (Application.isPlaying)
				{
					Object.Destroy(this.cornerMarkerPool[l]);
				}
				else
				{
					Object.DestroyImmediate(this.cornerMarkerPool[l]);
				}
			}
		}
		this.cornerMarkerPool.Clear();
	}

	// Token: 0x06000F28 RID: 3880 RVA: 0x0003F314 File Offset: 0x0003D514
	private void AnimatePolylines()
	{
		PayoutTracker instance = NetworkSingleton<PayoutTracker>.Instance;
		if (instance == null)
		{
			return;
		}
		List<PayoutRecord> allRecords = instance.GetAllRecords();
		this.playerOrder.Clear();
		Dictionary<PlayerProfile, int> dictionary = new Dictionary<PlayerProfile, int>();
		List<long> list = new List<long>();
		List<List<long>> list2 = new List<List<long>>();
		long num = this.minProfitDisplay;
		long num2 = this.maxProfitDisplay;
		bool flag = this.minProfitDisplay == 0L && this.maxProfitDisplay == 0L;
		if (flag)
		{
			num = 0L;
			num2 = 0L;
		}
		for (int i = 0; i < allRecords.Count; i++)
		{
			PayoutRecord payoutRecord = allRecords[i];
			PlayerProfile playerProfile = payoutRecord.playerProfile;
			if (!(playerProfile == null))
			{
				int count;
				if (!dictionary.TryGetValue(playerProfile, out count))
				{
					count = this.playerOrder.Count;
					dictionary[playerProfile] = count;
					this.playerOrder.Add(playerProfile);
					list.Add(0L);
					list2.Add(new List<long>());
				}
				if (payoutRecord.bet != 0L || payoutRecord.payout != 0L || payoutRecord.profit != 0L)
				{
					long num3 = list[count] + payoutRecord.profit;
					list[count] = num3;
					list2[count].Add(num3);
					if (flag)
					{
						if (num3 < num)
						{
							num = num3;
						}
						if (num3 > num2)
						{
							num2 = num3;
						}
					}
				}
			}
		}
		List<long[]> list3 = new List<long[]>(this.playerOrder.Count);
		for (int j = 0; j < list2.Count; j++)
		{
			list3.Add(ProfitLineGraph3D.DownsamplePoints(list2[j], this.maxPointsPerPlayer));
		}
		if (flag)
		{
			if (num == 9223372036854775807L || num2 == -9223372036854775808L)
			{
				num = 0L;
				num2 = 1L;
			}
			long num4 = Math.Max(100L, (num2 - num) / 10L);
			num -= num4;
			num2 += num4;
		}
		float num5 = Mathf.Max(1f, (float)(num2 - num));
		if (this.lobbySettings == null)
		{
			this.lobbySettings = Resources.Load<LobbySettings>("LobbySettings");
		}
		int count2 = this.playerOrder.Count;
		this.playerColors = new Color[count2];
		int k = 0;
		while (k < count2)
		{
			PlayerProfile playerProfile2 = this.playerOrder[k];
			if (!(this.lobbySettings != null) || !(playerProfile2 != null))
			{
				goto IL_26E;
			}
			PlayerInfo playerBySteamId = this.lobbySettings.GetPlayerBySteamId(playerProfile2.steamId);
			if (playerBySteamId == null)
			{
				goto IL_26E;
			}
			this.playerColors[k] = playerBySteamId.playerColor;
			IL_283:
			k++;
			continue;
			IL_26E:
			this.playerColors[k] = this.GetPlayerColorByIndex(k);
			goto IL_283;
		}
		float duration = Mathf.Max(0.01f, this.animationDuration);
		int num6 = 0;
		float num7 = this.timeAxisLength - this.rightPadding;
		int num8 = 0;
		for (int l = 0; l < count2; l++)
		{
			long[] array = list3[l];
			int num9 = array.Length;
			if (num9 != 0)
			{
				int playerIndex = l;
				int vertexCount = (num9 < 2) ? 2 : num9;
				Vector3[] finalPositions = new Vector3[vertexCount];
				long[] profitValues = new long[vertexCount];
				float num10 = (vertexCount <= 1) ? 0f : (num7 / (float)(vertexCount - 1));
				float z = (float)(playerIndex + 2) * this.playerSpacing;
				for (int m = 0; m < vertexCount; m++)
				{
					long num11 = (m < num9) ? array[m] : array[0];
					float num12 = Mathf.Clamp01((float)(num11 - num) / num5);
					finalPositions[m] = new Vector3(num10 * (float)m, num12 * this.profitAxisHeight, z);
					profitValues[m] = num11;
				}
				int[] cornerIndices = null;
				if (this.showCornerMarkers && num9 >= 3)
				{
					List<int> list4 = new List<int>(2);
					for (int n = 1; n < num9 - 1; n++)
					{
						long num13 = array[n] - array[n - 1];
						long num14 = array[n + 1] - array[n];
						if (num13 != 0L && num14 != 0L && ((num13 > 0L && num14 < 0L) || (num13 < 0L && num14 > 0L)))
						{
							list4.Add(n);
						}
					}
					cornerIndices = ((list4.Count > 0) ? list4.ToArray() : null);
				}
				Color c = this.playerColors[playerIndex];
				Polyline polyline = this.GetOrCreatePolyline(playerIndex);
				polyline.gameObject.SetActive(true);
				polyline.Thickness = this.lineThickness;
				polyline.ThicknessSpace = ThicknessSpace.Meters;
				polyline.Geometry = PolylineGeometry.Billboard;
				polyline.Joins = PolylineJoins.Round;
				polyline.BlendMode = ShapesBlendMode.Opaque;
				polyline.Closed = false;
				polyline.Color = c;
				GameObject[] cornerGOs = null;
				if (cornerIndices != null)
				{
					cornerGOs = new GameObject[cornerIndices.Length];
					for (int num15 = 0; num15 < cornerIndices.Length; num15++)
					{
						int num16 = cornerIndices[num15];
						GameObject gameObject = (num8 < this.cornerMarkerPool.Count) ? this.cornerMarkerPool[num8] : null;
						if (!gameObject)
						{
							gameObject = new GameObject(string.Format("CornerMarker_{0}_{1}", playerIndex, num15));
							gameObject.transform.SetParent(base.transform, false);
							gameObject.AddComponent<Sphere>().RadiusSpace = ThicknessSpace.Meters;
							this.cornerMarkerPool.Add(gameObject);
							this.SetLayerRecursively(gameObject, LayerMask.NameToLayer("Graph"));
						}
						Sphere component = gameObject.GetComponent<Sphere>();
						component.Radius = this.cornerMarkerRadius;
						component.Color = c;
						gameObject.transform.localPosition = finalPositions[num16];
						gameObject.SetActive(false);
						cornerGOs[num15] = gameObject;
						num8++;
					}
				}
				List<Vector3> animPositions = new List<Vector3>(vertexCount);
				List<Color> animColors = new List<Color>(vertexCount);
				Vector3 vector = finalPositions[0];
				for (int num17 = 0; num17 < vertexCount; num17++)
				{
					animPositions.Add(vector);
					animColors.Add(c);
				}
				polyline.SetPoints(animPositions, animColors);
				if (this.showEndpointMarkers)
				{
					this.UpdateEndpointMarker(playerIndex, vector, c, profitValues[0]);
				}
				float delay = (float)num6 * 0.1f;
				num6++;
				DOVirtual.Float(0f, 1f, duration, delegate(float progress)
				{
					float num18 = progress * (float)(vertexCount - 1);
					int num19 = Mathf.Clamp((int)num18, 0, vertexCount - 2);
					float t = Mathf.Clamp01(num18 - (float)num19);
					Vector3 vector2 = Vector3.Lerp(finalPositions[num19], finalPositions[num19 + 1], t);
					long profit = (long)Math.Round((double)Mathf.Lerp((float)profitValues[num19], (float)profitValues[num19 + 1], t));
					for (int num20 = 0; num20 <= num19; num20++)
					{
						animPositions[num20] = finalPositions[num20];
					}
					animPositions[num19 + 1] = vector2;
					for (int num21 = num19 + 2; num21 < vertexCount; num21++)
					{
						animPositions[num21] = vector2;
					}
					polyline.SetPoints(animPositions, animColors);
					if (cornerGOs != null)
					{
						for (int num22 = 0; num22 < cornerGOs.Length; num22++)
						{
							cornerGOs[num22].SetActive(num19 >= cornerIndices[num22]);
						}
					}
					if (this.showEndpointMarkers)
					{
						this.UpdateEndpointMarker(playerIndex, vector2, c, profit);
					}
				}).SetDelay(delay).SetEase(this.animationEase).SetTarget(this);
			}
		}
	}

	// Token: 0x06000F29 RID: 3881 RVA: 0x0003FA58 File Offset: 0x0003DC58
	private static long[] DownsamplePoints(List<long> source, int maxPoints)
	{
		if (source == null || source.Count == 0)
		{
			return Array.Empty<long>();
		}
		if (maxPoints < 2 || source.Count <= maxPoints)
		{
			return source.ToArray();
		}
		int count = source.Count;
		long[] array = new long[maxPoints];
		array[0] = source[0];
		array[maxPoints - 1] = source[count - 1];
		float num = ((float)count - 1f) / ((float)maxPoints - 1f);
		for (int i = 1; i < maxPoints - 1; i++)
		{
			int index = Mathf.Clamp(Mathf.RoundToInt((float)i * num), 0, count - 1);
			array[i] = source[index];
		}
		return array;
	}

	// Token: 0x06000F2A RID: 3882 RVA: 0x0003FAF0 File Offset: 0x0003DCF0
	private Polyline GetOrCreatePolyline(int playerIndex)
	{
		while (this.playerPolylines.Count <= playerIndex)
		{
			this.playerPolylines.Add(null);
		}
		if (this.playerPolylines[playerIndex] == null)
		{
			GameObject gameObject = new GameObject(string.Format("PlayerLine_{0}", playerIndex));
			gameObject.transform.SetParent(base.transform);
			gameObject.transform.localPosition = Vector3.zero;
			gameObject.transform.localRotation = Quaternion.identity;
			gameObject.transform.localScale = Vector3.one;
			this.SetLayerRecursively(gameObject, LayerMask.NameToLayer("Graph"));
			Polyline value = gameObject.AddComponent<Polyline>();
			this.playerPolylines[playerIndex] = value;
		}
		return this.playerPolylines[playerIndex];
	}

	// Token: 0x06000F2B RID: 3883 RVA: 0x0003FBB8 File Offset: 0x0003DDB8
	private void UpdateEndpointMarker(int playerIndex, Vector3 endPosition, Color playerColor, long profit)
	{
		if (this.endpointMarkerPrefab == null)
		{
			Debug.LogWarning("ProfitLineGraph3D: Endpoint marker prefab is not assigned!");
			return;
		}
		while (this.playerEndMarkers.Count <= playerIndex)
		{
			this.playerEndMarkers.Add(null);
		}
		if (this.playerEndMarkers[playerIndex] == null)
		{
			GameObject gameObject = Object.Instantiate<GameObject>(this.endpointMarkerPrefab, base.transform);
			gameObject.name = string.Format("PlayerEndMarker_{0}", playerIndex);
			gameObject.transform.localPosition = Vector3.zero;
			gameObject.transform.localRotation = Quaternion.identity;
			gameObject.transform.localScale = this.markerScale;
			this.SetLayerRecursively(gameObject, LayerMask.NameToLayer("Graph"));
			this.playerEndMarkers[playerIndex] = gameObject;
		}
		this.playerEndMarkers[playerIndex].transform.localPosition = endPosition;
		this.playerEndMarkers[playerIndex].transform.localScale = this.markerScale;
		this.playerEndMarkers[playerIndex].SetActive(true);
		MeshRenderer componentInChildren = this.playerEndMarkers[playerIndex].GetComponentInChildren<MeshRenderer>();
		if (componentInChildren != null && componentInChildren.material != null)
		{
			componentInChildren.material.color = playerColor;
		}
		this.UpdateProfitText(playerIndex, endPosition, profit);
		if (this.showPlayerNameLeft)
		{
			this.UpdatePlayerNameText(playerIndex, endPosition);
			return;
		}
		if (playerIndex < this.playerNameTexts.Count && this.playerNameTexts[playerIndex] != null)
		{
			this.playerNameTexts[playerIndex].SetActive(false);
		}
	}

	// Token: 0x06000F2C RID: 3884 RVA: 0x0003FD4C File Offset: 0x0003DF4C
	private void UpdatePlayerNameText(int playerIndex, Vector3 markerPosition)
	{
		while (this.playerNameTexts.Count <= playerIndex)
		{
			this.playerNameTexts.Add(null);
		}
		if (this.playerNameTexts[playerIndex] == null)
		{
			GameObject gameObject = Object.Instantiate<GameObject>(this.playerNameTextPrefab, base.transform);
			gameObject.name = string.Format("PlayerNameText_{0}", playerIndex);
			gameObject.transform.localPosition = Vector3.zero;
			gameObject.transform.localRotation = Quaternion.identity;
			gameObject.transform.localScale = this.textScale;
			this.SetLayerRecursively(gameObject, LayerMask.NameToLayer("Graph"));
			this.playerNameTexts[playerIndex] = gameObject;
		}
		float z = 2f * this.playerSpacing;
		Vector3 localPosition = new Vector3(markerPosition.x - this.nameOffsetX, markerPosition.y, z);
		this.playerNameTexts[playerIndex].transform.localPosition = localPosition;
		this.playerNameTexts[playerIndex].transform.localScale = this.textScale;
		this.playerNameTexts[playerIndex].SetActive(true);
		string text = (playerIndex < this.playerOrder.Count && this.playerOrder[playerIndex] != null) ? this.playerOrder[playerIndex].playerName : string.Format("Player {0}", playerIndex);
		TextMeshPro component = this.playerNameTexts[playerIndex].GetComponent<TextMeshPro>();
		if (component != null)
		{
			component.text = text;
			return;
		}
		Text component2 = this.playerNameTexts[playerIndex].GetComponent<Text>();
		if (component2 != null)
		{
			component2.text = text;
		}
	}

	// Token: 0x06000F2D RID: 3885 RVA: 0x0003FF04 File Offset: 0x0003E104
	private void UpdateProfitText(int playerIndex, Vector3 markerPosition, long profit)
	{
		while (this.playerEndTexts.Count <= playerIndex)
		{
			this.playerEndTexts.Add(null);
		}
		if (this.playerEndTexts[playerIndex] == null)
		{
			GameObject gameObject = Object.Instantiate<GameObject>(this.profitTextPrefab, base.transform);
			gameObject.name = string.Format("PlayerEndText_{0}", playerIndex);
			gameObject.transform.localPosition = Vector3.zero;
			gameObject.transform.localRotation = Quaternion.identity;
			gameObject.transform.localScale = this.textScale;
			this.SetLayerRecursively(gameObject, LayerMask.NameToLayer("Graph"));
			this.playerEndTexts[playerIndex] = gameObject;
		}
		float z = 2f * this.playerSpacing;
		Vector3 localPosition = new Vector3(markerPosition.x + this.textOffsetX, markerPosition.y, z);
		this.playerEndTexts[playerIndex].transform.localPosition = localPosition;
		this.playerEndTexts[playerIndex].transform.localScale = this.textScale;
		this.playerEndTexts[playerIndex].SetActive(true);
		string text = (profit >= 0L) ? ("+" + MoneyFormatter.FormatWithDollar(profit)) : MoneyFormatter.FormatWithDollar(profit);
		TextMeshPro component = this.playerEndTexts[playerIndex].GetComponent<TextMeshPro>();
		if (component != null)
		{
			component.text = text;
			return;
		}
		Text component2 = this.playerEndTexts[playerIndex].GetComponent<Text>();
		if (component2 != null)
		{
			component2.text = text;
		}
	}

	// Token: 0x06000F2E RID: 3886 RVA: 0x00040094 File Offset: 0x0003E294
	private Color GetPlayerColorByIndex(int playerIndex)
	{
		if (this.colorPalette != null && this.colorPalette.playerColors != null && this.colorPalette.playerColors.Length != 0)
		{
			return this.colorPalette.playerColors[playerIndex % this.colorPalette.playerColors.Length];
		}
		Color[] array = new Color[]
		{
			new Color(1f, 0.3f, 0.3f),
			new Color(0.3f, 1f, 0.3f),
			new Color(0.3f, 0.5f, 1f),
			new Color(1f, 0.8f, 0.2f),
			new Color(0.2f, 1f, 1f),
			new Color(1f, 0.3f, 1f)
		};
		return array[playerIndex % array.Length];
	}

	// Token: 0x06000F2F RID: 3887 RVA: 0x000401A0 File Offset: 0x0003E3A0
	private void SetLayerRecursively(GameObject obj, int layer)
	{
		if (obj == null)
		{
			return;
		}
		obj.layer = layer;
		foreach (object obj2 in obj.transform)
		{
			Transform transform = (Transform)obj2;
			this.SetLayerRecursively(transform.gameObject, layer);
		}
	}

	// Token: 0x040009AA RID: 2474
	[SerializeField]
	private float timeAxisLength;

	// Token: 0x040009AB RID: 2475
	[SerializeField]
	private float profitAxisHeight;

	// Token: 0x040009AC RID: 2476
	[SerializeField]
	private float playerSpacing;

	// Token: 0x040009AD RID: 2477
	[SerializeField]
	private float rightPadding;

	// Token: 0x040009AE RID: 2478
	[Header("Line Settings")]
	[SerializeField]
	private float lineThickness = 0.05f;

	// Token: 0x040009AF RID: 2479
	[SerializeField]
	private UIColorPalette colorPalette;

	// Token: 0x040009B0 RID: 2480
	[Header("Endpoint Marker")]
	[SerializeField]
	private bool showEndpointMarkers = true;

	// Token: 0x040009B1 RID: 2481
	[SerializeField]
	private bool showPlayerNameLeft;

	// Token: 0x040009B2 RID: 2482
	[SerializeField]
	private float nameOffsetX = 1f;

	// Token: 0x040009B3 RID: 2483
	[SerializeField]
	private GameObject endpointMarkerPrefab;

	// Token: 0x040009B4 RID: 2484
	[SerializeField]
	private Vector3 markerScale = Vector3.one;

	// Token: 0x040009B5 RID: 2485
	[SerializeField]
	private GameObject profitTextPrefab;

	// Token: 0x040009B6 RID: 2486
	[SerializeField]
	private GameObject playerNameTextPrefab;

	// Token: 0x040009B7 RID: 2487
	[SerializeField]
	private float textOffsetX = 0.5f;

	// Token: 0x040009B8 RID: 2488
	[SerializeField]
	private Vector3 textScale = Vector3.one;

	// Token: 0x040009B9 RID: 2489
	[Header("Corner Marker")]
	[SerializeField]
	private bool showCornerMarkers = true;

	// Token: 0x040009BA RID: 2490
	[SerializeField]
	private float cornerMarkerRadius = 0.08f;

	// Token: 0x040009BB RID: 2491
	[Header("Graph Display Settings")]
	[SerializeField]
	private long maxProfitDisplay;

	// Token: 0x040009BC RID: 2492
	[Header("Graph Display Settings")]
	[SerializeField]
	private long minProfitDisplay = 1000L;

	// Token: 0x040009BD RID: 2493
	[Header("Animation Settings")]
	[SerializeField]
	private float animationDuration = 5f;

	// Token: 0x040009BE RID: 2494
	[SerializeField]
	private Ease animationEase = Ease.OutQuad;

	// Token: 0x040009BF RID: 2495
	[Header("Performance")]
	[SerializeField]
	private int maxPointsPerPlayer = 120;

	// Token: 0x040009C0 RID: 2496
	private LobbySettings lobbySettings;

	// Token: 0x040009C1 RID: 2497
	private Color[] playerColors;

	// Token: 0x040009C2 RID: 2498
	private List<PlayerProfile> playerOrder = new List<PlayerProfile>();

	// Token: 0x040009C3 RID: 2499
	private List<Polyline> playerPolylines = new List<Polyline>();

	// Token: 0x040009C4 RID: 2500
	private List<GameObject> playerEndMarkers = new List<GameObject>();

	// Token: 0x040009C5 RID: 2501
	private List<GameObject> playerEndTexts = new List<GameObject>();

	// Token: 0x040009C6 RID: 2502
	private List<GameObject> playerNameTexts = new List<GameObject>();

	// Token: 0x040009C7 RID: 2503
	private readonly List<GameObject> cornerMarkerPool = new List<GameObject>();
}
