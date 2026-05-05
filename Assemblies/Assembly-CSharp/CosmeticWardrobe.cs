using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using Extensions;
using FMODUnity;
using Mirror;
using UnityEngine;

// Token: 0x020000AF RID: 175
[RequireComponent(typeof(BoxCollider))]
public class CosmeticWardrobe : NetworkBehaviour
{
	// Token: 0x17000094 RID: 148
	// (get) Token: 0x060006AA RID: 1706 RVA: 0x0001C6DA File Offset: 0x0001A8DA
	private bool IsClothingTab
	{
		get
		{
			return this.currentCategory == CosmeticType.Clothing;
		}
	}

	// Token: 0x060006AB RID: 1707 RVA: 0x0001C6E5 File Offset: 0x0001A8E5
	private void Awake()
	{
		this.ConfigureRangeTrigger();
	}

	// Token: 0x060006AC RID: 1708 RVA: 0x0001C6ED File Offset: 0x0001A8ED
	public override void OnStartClient()
	{
		base.OnStartClient();
		base.StartCoroutine(this.CheckInitialRangeOnceReady());
	}

	// Token: 0x060006AD RID: 1709 RVA: 0x0001C704 File Offset: 0x0001A904
	public void CloseDoors()
	{
		if (!this.isWardrobeOpen)
		{
			return;
		}
		Tween tween;
		Tween tween2;
		this.BeginClose(delegate
		{
			if (NetworkClient.localPlayer != null && !this.isWardrobeOpen)
			{
				this.ClearSpawnedCosmetics();
				Camera[] array = this.mirrorCameras;
				for (int i = 0; i < array.Length; i++)
				{
					array[i].enabled = false;
				}
			}
		}, out tween, out tween2);
	}

	// Token: 0x060006AE RID: 1710 RVA: 0x0001C730 File Offset: 0x0001A930
	public void OpenDoors()
	{
		if (this.isWardrobeOpen)
		{
			return;
		}
		SFXManager.SFXOneShot3DAttached(this.openDoorsSFX, this.sFXEmitter, false);
		this.leftWardrobeDoor.DOLocalRotate(new Vector3(0f, this.openDoorAngle, 0f), 1f, RotateMode.Fast).SetEase(Ease.OutBounce);
		this.rightWardrobeDoor.DOLocalRotate(new Vector3(0f, -this.openDoorAngle, 0f), 1f, RotateMode.Fast).SetEase(Ease.OutBounce);
		this.isWardrobeOpen = true;
		this.ApplyOpenEffectsIfReady();
	}

	// Token: 0x060006AF RID: 1711 RVA: 0x0001C7C4 File Offset: 0x0001A9C4
	private void ApplyOpenEffectsIfReady()
	{
		if (!this.isWardrobeOpen)
		{
			return;
		}
		if (this.openEffectsApplied)
		{
			return;
		}
		if (NetworkClient.localPlayer == null)
		{
			return;
		}
		Camera[] array = this.mirrorCameras;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].enabled = true;
		}
		this.LoadAndDisplayCosmetics();
		this.openEffectsApplied = true;
	}

	// Token: 0x060006B0 RID: 1712 RVA: 0x0001C81C File Offset: 0x0001AA1C
	private void OnTriggerEnter(Collider other)
	{
		if (!this.IsLocalPlayerCollider(other))
		{
			return;
		}
		if (this.playerInRange)
		{
			return;
		}
		this.playerInRange = true;
		this.OpenDoors();
	}

	// Token: 0x060006B1 RID: 1713 RVA: 0x0001C83E File Offset: 0x0001AA3E
	private void OnTriggerExit(Collider other)
	{
		if (!this.IsLocalPlayerCollider(other))
		{
			return;
		}
		if (!this.playerInRange)
		{
			return;
		}
		this.playerInRange = false;
		this.CloseDoors();
	}

	// Token: 0x060006B2 RID: 1714 RVA: 0x0001C860 File Offset: 0x0001AA60
	private void BeginClose(Action onLeftComplete, out Tween leftDoorTween, out Tween rightDoorTween)
	{
		SFXManager.SFXOneShot3DAttached(this.closeDoorsSFX, this.sFXEmitter, false);
		leftDoorTween = this.leftWardrobeDoor.DOLocalRotate(new Vector3(0f, 0f, 0f), 1f, RotateMode.Fast).SetEase(Ease.OutBounce).OnComplete(delegate
		{
			Action onLeftComplete2 = onLeftComplete;
			if (onLeftComplete2 == null)
			{
				return;
			}
			onLeftComplete2();
		});
		rightDoorTween = this.rightWardrobeDoor.DOLocalRotate(new Vector3(0f, 0f, 0f), 1f, RotateMode.Fast).SetEase(Ease.OutBounce);
		this.isWardrobeOpen = false;
		this.openEffectsApplied = false;
	}

	// Token: 0x060006B3 RID: 1715 RVA: 0x0001C908 File Offset: 0x0001AB08
	private void ConfigureRangeTrigger()
	{
		if (this.rangeTrigger == null)
		{
			this.rangeTrigger = base.GetComponent<BoxCollider>();
		}
		this.rangeTrigger.isTrigger = true;
		this.rangeTrigger.size = new Vector3(this.interactionRange, this.interactionRange, this.interactionRange);
	}

	// Token: 0x060006B4 RID: 1716 RVA: 0x0001C95D File Offset: 0x0001AB5D
	private IEnumerator CheckInitialRangeOnceReady()
	{
		while (NetworkClient.localPlayer == null)
		{
			yield return null;
		}
		if (Physics.OverlapSphere(base.transform.position, this.interactionRange).Any((Collider collider) => this.IsLocalPlayerCollider(collider)) && !this.playerInRange)
		{
			this.playerInRange = true;
			this.OpenDoors();
		}
		this.ApplyOpenEffectsIfReady();
		yield break;
	}

	// Token: 0x060006B5 RID: 1717 RVA: 0x0001C96C File Offset: 0x0001AB6C
	private bool IsLocalPlayerCollider(Collider other)
	{
		if (other == null)
		{
			return false;
		}
		NetworkIdentity componentInParent = other.GetComponentInParent<NetworkIdentity>();
		return componentInParent != null && componentInParent.isLocalPlayer;
	}

	// Token: 0x060006B6 RID: 1718 RVA: 0x0001C99C File Offset: 0x0001AB9C
	public void NextPage()
	{
		if (NetworkClient.localPlayer == null)
		{
			return;
		}
		if (this.GetTotalPages() <= 1)
		{
			return;
		}
		base.StartCoroutine(this.ChangePageCoroutine(true));
	}

	// Token: 0x060006B7 RID: 1719 RVA: 0x0001C9C4 File Offset: 0x0001ABC4
	public void PreviousPage()
	{
		if (NetworkClient.localPlayer == null)
		{
			return;
		}
		if (this.GetTotalPages() <= 1)
		{
			return;
		}
		base.StartCoroutine(this.ChangePageCoroutine(false));
	}

	// Token: 0x060006B8 RID: 1720 RVA: 0x0001C9EC File Offset: 0x0001ABEC
	public void SetCategoryByIndex(int categoryIndex)
	{
		if (NetworkClient.localPlayer == null)
		{
			return;
		}
		CosmeticType[] array = (CosmeticType[])Enum.GetValues(typeof(CosmeticType));
		if (categoryIndex < 0 || categoryIndex >= array.Length)
		{
			Debug.LogWarning(string.Format("[CosmeticWardrobe] Invalid category index: {0}. Valid range: 0-{1}", categoryIndex, array.Length - 1));
			return;
		}
		CosmeticType cosmeticType = array[categoryIndex];
		if (this.currentCategory == cosmeticType)
		{
			return;
		}
		base.StartCoroutine(this.ChangeCategoryCoroutine(cosmeticType));
	}

	// Token: 0x060006B9 RID: 1721 RVA: 0x0001CA62 File Offset: 0x0001AC62
	public void RefreshPage()
	{
		if (!this.playerInRange)
		{
			return;
		}
		if (NetworkClient.localPlayer == null)
		{
			return;
		}
		base.StartCoroutine(this.RefreshPageCoroutine());
	}

	// Token: 0x060006BA RID: 1722 RVA: 0x0001CA88 File Offset: 0x0001AC88
	private IEnumerator ChangePageCoroutine(bool next)
	{
		if (!this.isWardrobeOpen)
		{
			yield break;
		}
		Tween t;
		Tween rightDoorTween;
		this.BeginClose(delegate
		{
			if (NetworkClient.localPlayer != null)
			{
				this.ClearSpawnedCosmetics();
			}
		}, out t, out rightDoorTween);
		yield return t.WaitForCompletion();
		yield return rightDoorTween.WaitForCompletion();
		if (next)
		{
			this.currentPage = (this.currentPage + 1) % this.GetTotalPages();
		}
		else
		{
			this.currentPage = (this.currentPage - 1 + this.GetTotalPages()) % this.GetTotalPages();
		}
		this.OpenDoors();
		yield break;
	}

	// Token: 0x060006BB RID: 1723 RVA: 0x0001CA9E File Offset: 0x0001AC9E
	private IEnumerator ChangeCategoryCoroutine(CosmeticType newCategory)
	{
		if (!this.isWardrobeOpen)
		{
			yield break;
		}
		Tween t;
		Tween rightDoorTween;
		this.BeginClose(delegate
		{
			if (NetworkClient.localPlayer != null)
			{
				this.ClearSpawnedCosmetics();
			}
		}, out t, out rightDoorTween);
		yield return t.WaitForCompletion();
		yield return rightDoorTween.WaitForCompletion();
		this.currentCategory = newCategory;
		this.currentPage = 0;
		this.OpenDoors();
		yield break;
	}

	// Token: 0x060006BC RID: 1724 RVA: 0x0001CAB4 File Offset: 0x0001ACB4
	private IEnumerator RefreshPageCoroutine()
	{
		if (!this.isWardrobeOpen)
		{
			yield break;
		}
		Tween t;
		Tween rightDoorTween;
		this.BeginClose(delegate
		{
			if (NetworkClient.localPlayer != null)
			{
				this.ClearSpawnedCosmetics();
			}
		}, out t, out rightDoorTween);
		yield return t.WaitForCompletion();
		yield return rightDoorTween.WaitForCompletion();
		this.OpenDoors();
		yield break;
	}

	// Token: 0x060006BD RID: 1725 RVA: 0x0001CAC4 File Offset: 0x0001ACC4
	private void LoadAndDisplayCosmetics()
	{
		this.ClearSpawnedCosmetics();
		this.LoadCategoryCosmetics();
		for (int i = 1; i < this.spawnTransforms.Length; i++)
		{
			Transform transform = this.spawnTransforms[i];
			if (transform != null && transform.parent != null)
			{
				transform.parent.gameObject.SetActive(false);
				CosmeticWardrobe.SetSlotParentName(transform, string.Empty);
				CosmeticWardrobe.SetLockSpriteVisible(transform, false);
			}
		}
		int usableSlotsPerPage = this.GetUsableSlotsPerPage();
		int num = this.currentPage * usableSlotsPerPage;
		int num2 = Mathf.Min(num + usableSlotsPerPage, this.currentCategoryCosmetics.Count);
		for (int j = num; j < num2; j++)
		{
			int num3 = j - num + 1;
			if (num3 >= this.spawnTransforms.Length || this.spawnTransforms[num3] == null)
			{
				Debug.LogWarning(string.Format("[CosmeticWardrobe] Spawn transform at index {0} is null or out of bounds", num3));
			}
			else
			{
				CosmeticData cosmeticData = this.currentCategoryCosmetics[j];
				if (cosmeticData == null || cosmeticData.cosmeticModel == null)
				{
					Debug.LogWarning(string.Format("[CosmeticWardrobe] Cosmetic {0} has no model", j));
				}
				else
				{
					Transform transform2 = this.spawnTransforms[num3];
					this.SpawnCosmetic(cosmeticData, transform2);
					CosmeticWardrobe.SetSlotParentName(transform2, cosmeticData.cosmeticName);
					if (transform2.parent != null)
					{
						transform2.parent.gameObject.SetActive(true);
					}
				}
			}
		}
		bool enabled = !this.IsClothingTab;
		for (int k = 0; k < this.spawnTransforms.Length; k++)
		{
			Transform transform3 = this.spawnTransforms[k];
			if (!(transform3 == null) && !(transform3.parent == null))
			{
				foreach (MeshRenderer meshRenderer in transform3.parent.GetComponents<MeshRenderer>())
				{
					if (meshRenderer != null)
					{
						meshRenderer.enabled = enabled;
					}
				}
				foreach (SkinnedMeshRenderer skinnedMeshRenderer in transform3.parent.GetComponents<SkinnedMeshRenderer>())
				{
					if (skinnedMeshRenderer != null)
					{
						skinnedMeshRenderer.enabled = enabled;
					}
				}
			}
		}
		this.RefreshSlotParentOutlines();
	}

	// Token: 0x060006BE RID: 1726 RVA: 0x0001CD04 File Offset: 0x0001AF04
	private void LoadCategoryCosmetics()
	{
		this.currentCategoryCosmetics.Clear();
		IEnumerable<CosmeticData> allCosmetics = CosmeticDataManager.GetAllCosmetics();
		this.currentCategoryCosmetics = (from c in allCosmetics
		where c != null && c.cosmeticType == this.currentCategory
		orderby c.cosmeticId
		select c).ToList<CosmeticData>();
		Debug.Log(string.Format("[CosmeticWardrobe] Loaded {0} cosmetics for category {1}", this.currentCategoryCosmetics.Count, this.currentCategory));
	}

	// Token: 0x060006BF RID: 1727 RVA: 0x0001CD90 File Offset: 0x0001AF90
	private void SpawnCosmetic(CosmeticData cosmetic, Transform spawnTransform)
	{
		if (cosmetic == null || cosmetic.cosmeticModel == null || spawnTransform == null)
		{
			return;
		}
		GameObject gameObject = Object.Instantiate<GameObject>(cosmetic.cosmeticModel, spawnTransform.position, spawnTransform.rotation, spawnTransform);
		if (this.IsClothingTab)
		{
			gameObject.transform.localPosition += this.clothingSpawnLocalOffset;
		}
		this.spawnedCosmetics.Add(gameObject);
		bool flag = MonoSingleton<CosmeticsUnlockManager>.Instance != null && MonoSingleton<CosmeticsUnlockManager>.Instance.IsCosmeticUnlocked(cosmetic.cosmeticId);
		CosmeticWardrobe.SetLockSpriteVisible(spawnTransform, !flag);
		Material material = flag ? cosmetic.cosmeticMaterial : this.lockedCosmeticMaterial;
		if (material != null)
		{
			foreach (Renderer renderer in gameObject.GetComponentsInChildren<Renderer>())
			{
				if (renderer != null)
				{
					renderer.material = material;
				}
			}
		}
	}

	// Token: 0x060006C0 RID: 1728 RVA: 0x0001CE80 File Offset: 0x0001B080
	private static void SetLockSpriteVisible(Transform spawnTransform, bool isVisible)
	{
		if (spawnTransform == null || spawnTransform.parent == null)
		{
			return;
		}
		SpriteRenderer spriteRenderer = spawnTransform.parent.GetComponentsInChildren<SpriteRenderer>(true).FirstOrDefault((SpriteRenderer sr) => sr.transform.parent == spawnTransform.parent && sr.transform != spawnTransform);
		if (spriteRenderer != null)
		{
			spriteRenderer.enabled = isVisible;
		}
	}

	// Token: 0x060006C1 RID: 1729 RVA: 0x0001CEF0 File Offset: 0x0001B0F0
	private void ClearSpawnedCosmetics()
	{
		foreach (GameObject gameObject in this.spawnedCosmetics)
		{
			if (gameObject != null)
			{
				Object.Destroy(gameObject);
			}
		}
		this.spawnedCosmetics.Clear();
		for (int i = 0; i < this.spawnTransforms.Length; i++)
		{
			CosmeticWardrobe.SetLockSpriteVisible(this.spawnTransforms[i], false);
		}
	}

	// Token: 0x060006C2 RID: 1730 RVA: 0x0001CF78 File Offset: 0x0001B178
	private void RefreshSlotParentOutlines()
	{
		for (int i = 0; i < this.spawnTransforms.Length; i++)
		{
			Transform transform = this.spawnTransforms[i];
			Outline outline;
			if (!(transform == null) && !(transform.parent == null) && transform.parent.TryGetComponent<Outline>(out outline))
			{
				bool enabled = outline.enabled;
				outline.CacheRenderers();
				if (enabled)
				{
					outline.enabled = false;
					outline.enabled = true;
				}
			}
		}
	}

	// Token: 0x060006C3 RID: 1731 RVA: 0x0001CFE4 File Offset: 0x0001B1E4
	private static void SetSlotParentName(Transform spawnTransform, string tooltip)
	{
		if (spawnTransform == null || spawnTransform.parent == null)
		{
			return;
		}
		InteractableBase interactableBase;
		if (!spawnTransform.parent.TryGetComponent<InteractableBase>(out interactableBase))
		{
			return;
		}
		interactableBase.InteractableName = tooltip;
	}

	// Token: 0x060006C4 RID: 1732 RVA: 0x0001D020 File Offset: 0x0001B220
	private int GetTotalPages()
	{
		if (this.currentCategoryCosmetics.Count == 0)
		{
			this.LoadCategoryCosmetics();
		}
		int usableSlotsPerPage = this.GetUsableSlotsPerPage();
		return Mathf.Max(1, Mathf.CeilToInt((float)this.currentCategoryCosmetics.Count / (float)usableSlotsPerPage));
	}

	// Token: 0x060006C5 RID: 1733 RVA: 0x0001D061 File Offset: 0x0001B261
	private int GetUsableSlotsPerPage()
	{
		return Mathf.Max(1, this.itemsPerPage - 1);
	}

	// Token: 0x060006C6 RID: 1734 RVA: 0x0001D074 File Offset: 0x0001B274
	public void TryEquipCosmetic(int spawnIndex)
	{
		if (NetworkClient.localPlayer == null)
		{
			return;
		}
		if (spawnIndex < 0 || spawnIndex >= this.spawnTransforms.Length)
		{
			Debug.LogWarning(string.Format("[CosmeticWardrobe] Invalid spawn index: {0}", spawnIndex));
			return;
		}
		if (spawnIndex == 0)
		{
			PlayerCustomization component = NetworkClient.localPlayer.GetComponent<PlayerCustomization>();
			if (component == null)
			{
				Debug.LogWarning("[CosmeticWardrobe] PlayerCustomization component not found on local player");
				return;
			}
			component.ClearCategory(this.currentCategory);
			return;
		}
		else
		{
			int usableSlotsPerPage = this.GetUsableSlotsPerPage();
			int num = this.currentPage * usableSlotsPerPage + (spawnIndex - 1);
			if (num < 0 || num >= this.currentCategoryCosmetics.Count)
			{
				Debug.LogWarning(string.Format("[CosmeticWardrobe] Cosmetic index {0} is out of bounds", num));
				return;
			}
			CosmeticData cosmeticData = this.currentCategoryCosmetics[num];
			if (cosmeticData == null)
			{
				Debug.LogWarning(string.Format("[CosmeticWardrobe] Cosmetic at index {0} is null", num));
				return;
			}
			int cosmeticId = cosmeticData.cosmeticId;
			if (MonoSingleton<CosmeticsUnlockManager>.Instance == null)
			{
				Debug.LogError("[CosmeticWardrobe] CosmeticsUnlockManager.Instance is null!");
				return;
			}
			if (!MonoSingleton<CosmeticsUnlockManager>.Instance.IsCosmeticUnlocked(cosmeticId))
			{
				Debug.LogWarning(string.Format("[CosmeticWardrobe] Cosmetic {0} ({1}) is not unlocked!", cosmeticId, cosmeticData.cosmeticName));
				return;
			}
			PlayerCustomization component2 = NetworkClient.localPlayer.GetComponent<PlayerCustomization>();
			if (component2 == null)
			{
				Debug.LogWarning("[CosmeticWardrobe] PlayerCustomization component not found on local player");
				return;
			}
			component2.CmdChangeCustomization(cosmeticId, true);
			Debug.Log(string.Format("[CosmeticWardrobe] Equipped cosmetic: {0} (ID: {1})", cosmeticData.cosmeticName, cosmeticId));
			return;
		}
	}

	// Token: 0x060006C7 RID: 1735 RVA: 0x0001D1DC File Offset: 0x0001B3DC
	private void OnDrawGizmos()
	{
		Gizmos.color = this.gizmoColor;
		Gizmos.DrawWireSphere(base.transform.position, this.interactionRange);
		Gizmos.color = new Color(this.gizmoColor.r, this.gizmoColor.g, this.gizmoColor.b, 1f);
		Gizmos.DrawWireSphere(base.transform.position, this.interactionRange);
	}

	// Token: 0x060006C8 RID: 1736 RVA: 0x0001D250 File Offset: 0x0001B450
	private void OnDrawGizmosSelected()
	{
		Gizmos.color = new Color(1f, 1f, 0f, 0.5f);
		Gizmos.DrawSphere(base.transform.position, this.interactionRange);
		Gizmos.color = Color.yellow;
		Gizmos.DrawWireSphere(base.transform.position, this.interactionRange);
	}

	// Token: 0x060006D0 RID: 1744 RVA: 0x00002321 File Offset: 0x00000521
	public override bool Weaved()
	{
		return true;
	}

	// Token: 0x04000477 RID: 1143
	[SerializeField]
	private Transform leftWardrobeDoor;

	// Token: 0x04000478 RID: 1144
	[SerializeField]
	private Transform rightWardrobeDoor;

	// Token: 0x04000479 RID: 1145
	[SerializeField]
	private float openDoorAngle = 120f;

	// Token: 0x0400047A RID: 1146
	[SerializeField]
	private bool isWardrobeOpen;

	// Token: 0x0400047B RID: 1147
	[Header("Range Settings")]
	[SerializeField]
	private float interactionRange = 3f;

	// Token: 0x0400047C RID: 1148
	[SerializeField]
	private Color gizmoColor = new Color(0f, 1f, 0f, 0.3f);

	// Token: 0x0400047D RID: 1149
	[Header("Cosmetic Display")]
	[SerializeField]
	private CosmeticType currentCategory;

	// Token: 0x0400047E RID: 1150
	[SerializeField]
	private Transform[] spawnTransforms = new Transform[12];

	// Token: 0x0400047F RID: 1151
	[SerializeField]
	private int itemsPerPage = 12;

	// Token: 0x04000480 RID: 1152
	[SerializeField]
	private Material lockedCosmeticMaterial;

	// Token: 0x04000481 RID: 1153
	[SerializeField]
	private Material unlockedCosmeticMaterial;

	// Token: 0x04000482 RID: 1154
	[Header("Clothing tab display")]
	[SerializeField]
	private Vector3 clothingSpawnLocalOffset = new Vector3(0f, 1.1f, 0f);

	// Token: 0x04000483 RID: 1155
	[SerializeField]
	private Camera[] mirrorCameras;

	// Token: 0x04000484 RID: 1156
	[Header("SFX")]
	[SerializeField]
	private EventReference openDoorsSFX;

	// Token: 0x04000485 RID: 1157
	[SerializeField]
	private EventReference closeDoorsSFX;

	// Token: 0x04000486 RID: 1158
	[SerializeField]
	private EventReference pickItemSFX;

	// Token: 0x04000487 RID: 1159
	[SerializeField]
	private GameObject sFXEmitter;

	// Token: 0x04000488 RID: 1160
	private bool playerInRange;

	// Token: 0x04000489 RID: 1161
	private int currentPage;

	// Token: 0x0400048A RID: 1162
	private List<CosmeticData> currentCategoryCosmetics = new List<CosmeticData>();

	// Token: 0x0400048B RID: 1163
	private List<GameObject> spawnedCosmetics = new List<GameObject>();

	// Token: 0x0400048C RID: 1164
	private bool openEffectsApplied;

	// Token: 0x0400048D RID: 1165
	private BoxCollider rangeTrigger;
}
