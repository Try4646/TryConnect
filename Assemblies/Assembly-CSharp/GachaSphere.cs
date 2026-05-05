using System;
using System.Runtime.InteropServices;
using Extensions;
using Mirror;
using SkyBrave_Toolkit.Scripts.Scriptable_Game_Events;
using UnityEngine;

// Token: 0x02000106 RID: 262
public class GachaSphere : ConsumableItem
{
	// Token: 0x170000F2 RID: 242
	// (get) Token: 0x06000AD4 RID: 2772 RVA: 0x0002B323 File Offset: 0x00029523
	public int CosmeticId
	{
		get
		{
			return this.cosmeticId;
		}
	}

	// Token: 0x170000F3 RID: 243
	// (get) Token: 0x06000AD5 RID: 2773 RVA: 0x0002B32B File Offset: 0x0002952B
	public override bool ShouldShowHoverDescription
	{
		get
		{
			return false;
		}
	}

	// Token: 0x06000AD6 RID: 2774 RVA: 0x0002B330 File Offset: 0x00029530
	public override void OnStartServer()
	{
		base.OnStartServer();
		if (this.randomCosmeticIdOnStart)
		{
			int successfulQuota = NetworkSingleton<GameManager>.Instance.successfulQuota;
			Random random = new Random(this.GetDeterministicCosmeticHash(base.transform.position, NetworkSingleton<SeededRandomManager>.Instance.CurrentSeed, successfulQuota));
			int[] validCosmeticIdsSorted = CosmeticDataManager.GetValidCosmeticIdsSorted();
			if (validCosmeticIdsSorted.Length == 0)
			{
				Debug.LogWarning("[GachaSphere] No cosmetic data loaded; cannot assign random cosmetic.");
				this.NetworkcosmeticId = -1;
				return;
			}
			int num = random.Next(0, validCosmeticIdsSorted.Length);
			this.NetworkcosmeticId = validCosmeticIdsSorted[num];
		}
	}

	// Token: 0x06000AD7 RID: 2775 RVA: 0x0002B3A8 File Offset: 0x000295A8
	private int GetDeterministicCosmeticHash(Vector3 position, int seed, int quotaIndex)
	{
		int num = seed * 31 + quotaIndex;
		int num2 = Mathf.RoundToInt(position.x * 100f);
		int num3 = Mathf.RoundToInt(position.y * 100f);
		int num4 = Mathf.RoundToInt(position.z * 100f);
		return ((num * 31 + num2) * 31 + num3) * 31 + num4;
	}

	// Token: 0x06000AD8 RID: 2776 RVA: 0x0002B400 File Offset: 0x00029600
	[Server]
	public void SetCosmeticData(CosmeticData data)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void GachaSphere::SetCosmeticData(CosmeticData)' called when server was not active");
			return;
		}
		if (data != null)
		{
			this.NetworkcosmeticId = data.cosmeticId;
		}
	}

	// Token: 0x06000AD9 RID: 2777 RVA: 0x0002B42C File Offset: 0x0002962C
	[Server]
	public void SetCosmeticId(int id)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void GachaSphere::SetCosmeticId(System.Int32)' called when server was not active");
			return;
		}
		if (id > 0)
		{
			this.NetworkcosmeticId = id;
		}
	}

	// Token: 0x06000ADA RID: 2778 RVA: 0x0002B44E File Offset: 0x0002964E
	private void OnCosmeticIdChanged(int oldValue, int newValue)
	{
		this.NetworkcosmeticId = newValue;
		this.InteractableName = CosmeticDataManager.GetCosmeticById(this.cosmeticId).cosmeticName;
		this.LoadAndSpawnCosmetic();
	}

	// Token: 0x06000ADB RID: 2779 RVA: 0x0002B474 File Offset: 0x00029674
	private void LoadAndSpawnCosmetic()
	{
		this.ClearCosmeticModel();
		if (this.cosmeticId <= 0)
		{
			return;
		}
		this._currentCosmeticData = CosmeticDataManager.GetCosmeticById(this.cosmeticId);
		if (this._currentCosmeticData == null)
		{
			Debug.LogError(string.Format("[GachaSphere] Failed to load CosmeticData with ID {0}. Make sure CosmeticDataManager is initialized.", this.cosmeticId));
			return;
		}
		if (this._currentCosmeticData.cosmeticModel == null)
		{
			Debug.LogError(string.Format("[GachaSphere] CosmeticData with ID {0} ({1}) has no cosmeticModel assigned!", this.cosmeticId, this._currentCosmeticData.cosmeticName));
			return;
		}
		this.SpawnCosmeticModel();
	}

	// Token: 0x06000ADC RID: 2780 RVA: 0x0002B50C File Offset: 0x0002970C
	private void SpawnCosmeticModel()
	{
		if (this._currentCosmeticData == null || this._currentCosmeticData.cosmeticModel == null)
		{
			return;
		}
		bool flag = this._currentCosmeticData.cosmeticType == CosmeticType.Clothing;
		this._spawnedCosmeticModel = Object.Instantiate<GameObject>(this._currentCosmeticData.cosmeticModel, this.cosmeticModelTransform.position, this.cosmeticModelTransform.rotation, this.cosmeticModelTransform);
		this.RefreshCosmeticParentOutline();
		Renderer component = this._spawnedCosmeticModel.GetComponent<Renderer>();
		if (component != null)
		{
			component.material = this._currentCosmeticData.cosmeticMaterial;
		}
		if (flag)
		{
			GachaSphere.SetRendererEnabled(this.clothingRendererToDisable, false);
			this._spawnedCosmeticModel.transform.localPosition = new Vector3(0f, 1.1f, 0f);
		}
		else
		{
			GachaSphere.SetRendererEnabled(this.clothingRendererToDisable, true);
			this._spawnedCosmeticModel.transform.localPosition = Vector3.zero;
		}
		if (this.centerUsingBounds)
		{
			this.CenterCosmeticUsingBounds();
			return;
		}
	}

	// Token: 0x06000ADD RID: 2781 RVA: 0x0002B60C File Offset: 0x0002980C
	private void RefreshCosmeticParentOutline()
	{
		if (this.cosmeticModelTransform == null)
		{
			return;
		}
		Outline componentInParent = this.cosmeticModelTransform.GetComponentInParent<Outline>();
		if (componentInParent == null)
		{
			return;
		}
		bool enabled = componentInParent.enabled;
		componentInParent.CacheRenderers();
		if (!enabled)
		{
			return;
		}
		componentInParent.enabled = false;
		componentInParent.enabled = true;
	}

	// Token: 0x06000ADE RID: 2782 RVA: 0x0002B660 File Offset: 0x00029860
	private static void SetRendererEnabled(Transform transform, bool enabled)
	{
		if (transform == null)
		{
			return;
		}
		MeshRenderer component = transform.GetComponent<MeshRenderer>();
		if (component != null)
		{
			component.enabled = enabled;
		}
		SkinnedMeshRenderer component2 = transform.GetComponent<SkinnedMeshRenderer>();
		if (component2 != null)
		{
			component2.enabled = enabled;
		}
	}

	// Token: 0x06000ADF RID: 2783 RVA: 0x0002B6A8 File Offset: 0x000298A8
	private void CenterCosmeticUsingBounds()
	{
		if (this._spawnedCosmeticModel == null)
		{
			return;
		}
		Renderer[] componentsInChildren = this._spawnedCosmeticModel.GetComponentsInChildren<Renderer>();
		if (componentsInChildren.Length == 0)
		{
			return;
		}
		Bounds bounds = componentsInChildren[0].bounds;
		for (int i = 1; i < componentsInChildren.Length; i++)
		{
			bounds.Encapsulate(componentsInChildren[i].bounds);
		}
		Vector3 b = new Vector3(bounds.center.x, bounds.min.y, bounds.center.z);
		Vector3 b2 = this.cosmeticModelTransform.position - b;
		this._spawnedCosmeticModel.transform.position += b2;
	}

	// Token: 0x06000AE0 RID: 2784 RVA: 0x0002B759 File Offset: 0x00029959
	private void ClearCosmeticModel()
	{
		if (this._spawnedCosmeticModel != null)
		{
			Object.Destroy(this._spawnedCosmeticModel);
			this._spawnedCosmeticModel = null;
		}
	}

	// Token: 0x06000AE1 RID: 2785 RVA: 0x0002B77B File Offset: 0x0002997B
	protected override void OnDestroy()
	{
		base.OnDestroy();
		this.ClearCosmeticModel();
	}

	// Token: 0x06000AE2 RID: 2786 RVA: 0x0002B789 File Offset: 0x00029989
	public GachaSphere()
	{
		this._Mirror_SyncVarHookDelegate_cosmeticId = new Action<int, int>(this.OnCosmeticIdChanged);
	}

	// Token: 0x06000AE3 RID: 2787 RVA: 0x00002321 File Offset: 0x00000521
	public override bool Weaved()
	{
		return true;
	}

	// Token: 0x170000F4 RID: 244
	// (get) Token: 0x06000AE4 RID: 2788 RVA: 0x0002B7B4 File Offset: 0x000299B4
	// (set) Token: 0x06000AE5 RID: 2789 RVA: 0x0002B7C7 File Offset: 0x000299C7
	public int NetworkcosmeticId
	{
		get
		{
			return this.cosmeticId;
		}
		[param: In]
		set
		{
			base.GeneratedSyncVarSetter<int>(value, ref this.cosmeticId, 2UL, this._Mirror_SyncVarHookDelegate_cosmeticId);
		}
	}

	// Token: 0x06000AE6 RID: 2790 RVA: 0x0002B7E8 File Offset: 0x000299E8
	public override void SerializeSyncVars(NetworkWriter writer, bool forceAll)
	{
		base.SerializeSyncVars(writer, forceAll);
		if (forceAll)
		{
			writer.WriteVarInt(this.cosmeticId);
			return;
		}
		writer.WriteVarULong(this.syncVarDirtyBits);
		if ((this.syncVarDirtyBits & 2UL) != 0UL)
		{
			writer.WriteVarInt(this.cosmeticId);
		}
	}

	// Token: 0x06000AE7 RID: 2791 RVA: 0x0002B840 File Offset: 0x00029A40
	public override void DeserializeSyncVars(NetworkReader reader, bool initialState)
	{
		base.DeserializeSyncVars(reader, initialState);
		if (initialState)
		{
			base.GeneratedSyncVarDeserialize<int>(ref this.cosmeticId, this._Mirror_SyncVarHookDelegate_cosmeticId, reader.ReadVarInt());
			return;
		}
		long num = (long)reader.ReadVarULong();
		if ((num & 2L) != 0L)
		{
			base.GeneratedSyncVarDeserialize<int>(ref this.cosmeticId, this._Mirror_SyncVarHookDelegate_cosmeticId, reader.ReadVarInt());
		}
	}

	// Token: 0x040006CC RID: 1740
	[Header("Cosmetic Data")]
	[SerializeField]
	[SyncVar(hook = "OnCosmeticIdChanged")]
	private int cosmeticId = -1;

	// Token: 0x040006CD RID: 1741
	[SerializeField]
	private Transform cosmeticModelTransform;

	// Token: 0x040006CE RID: 1742
	[SerializeField]
	private bool centerUsingBounds = true;

	// Token: 0x040006CF RID: 1743
	[Header("Clothing Overrides")]
	[Tooltip("Only for clothing cosmetics: disables this transform's MeshRenderer/SkinnedMeshRenderer.")]
	[SerializeField]
	private Transform clothingRendererToDisable;

	// Token: 0x040006D0 RID: 1744
	[SerializeField]
	private GameEvent onCosmeticUnlocked;

	// Token: 0x040006D1 RID: 1745
	private CosmeticData _currentCosmeticData;

	// Token: 0x040006D2 RID: 1746
	private GameObject _spawnedCosmeticModel;

	// Token: 0x040006D3 RID: 1747
	[SerializeField]
	private bool randomCosmeticIdOnStart;

	// Token: 0x040006D4 RID: 1748
	public Action<int, int> _Mirror_SyncVarHookDelegate_cosmeticId;
}
