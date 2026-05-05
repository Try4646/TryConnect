using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;

// Token: 0x02000347 RID: 839
[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class MeshCombiner : MonoBehaviour
{
	// Token: 0x17000286 RID: 646
	// (get) Token: 0x06001B8A RID: 7050 RVA: 0x0007632A File Offset: 0x0007452A
	// (set) Token: 0x06001B8B RID: 7051 RVA: 0x00076332 File Offset: 0x00074532
	public bool CreateMultiMaterialMesh
	{
		get
		{
			return this.createMultiMaterialMesh;
		}
		set
		{
			this.createMultiMaterialMesh = value;
		}
	}

	// Token: 0x17000287 RID: 647
	// (get) Token: 0x06001B8C RID: 7052 RVA: 0x0007633B File Offset: 0x0007453B
	// (set) Token: 0x06001B8D RID: 7053 RVA: 0x00076343 File Offset: 0x00074543
	public bool CombineInactiveChildren
	{
		get
		{
			return this.combineInactiveChildren;
		}
		set
		{
			this.combineInactiveChildren = value;
		}
	}

	// Token: 0x17000288 RID: 648
	// (get) Token: 0x06001B8E RID: 7054 RVA: 0x0007634C File Offset: 0x0007454C
	// (set) Token: 0x06001B8F RID: 7055 RVA: 0x00076354 File Offset: 0x00074554
	public bool AddMeshCollider
	{
		get
		{
			return this.addMeshCollider;
		}
		set
		{
			this.addMeshCollider = value;
		}
	}

	// Token: 0x17000289 RID: 649
	// (get) Token: 0x06001B90 RID: 7056 RVA: 0x0007635D File Offset: 0x0007455D
	// (set) Token: 0x06001B91 RID: 7057 RVA: 0x00076365 File Offset: 0x00074565
	public bool DeactivateCombinedChildren
	{
		get
		{
			return this.deactivateCombinedChildren;
		}
		set
		{
			this.deactivateCombinedChildren = value;
			this.CheckDeactivateCombinedChildren();
		}
	}

	// Token: 0x1700028A RID: 650
	// (get) Token: 0x06001B92 RID: 7058 RVA: 0x00076374 File Offset: 0x00074574
	// (set) Token: 0x06001B93 RID: 7059 RVA: 0x0007637C File Offset: 0x0007457C
	public bool DeactivateCombinedChildrenMeshRenderers
	{
		get
		{
			return this.deactivateCombinedChildrenMeshRenderers;
		}
		set
		{
			this.deactivateCombinedChildrenMeshRenderers = value;
			this.CheckDeactivateCombinedChildren();
		}
	}

	// Token: 0x1700028B RID: 651
	// (get) Token: 0x06001B94 RID: 7060 RVA: 0x0007638B File Offset: 0x0007458B
	// (set) Token: 0x06001B95 RID: 7061 RVA: 0x00076393 File Offset: 0x00074593
	public bool GenerateUVMap
	{
		get
		{
			return this.generateUVMap;
		}
		set
		{
			this.generateUVMap = value;
		}
	}

	// Token: 0x1700028C RID: 652
	// (get) Token: 0x06001B96 RID: 7062 RVA: 0x0007639C File Offset: 0x0007459C
	// (set) Token: 0x06001B97 RID: 7063 RVA: 0x000763A4 File Offset: 0x000745A4
	public bool DestroyCombinedChildren
	{
		get
		{
			return this.destroyCombinedChildren;
		}
		set
		{
			this.destroyCombinedChildren = value;
			this.CheckDestroyCombinedChildren();
		}
	}

	// Token: 0x1700028D RID: 653
	// (get) Token: 0x06001B98 RID: 7064 RVA: 0x000763B3 File Offset: 0x000745B3
	// (set) Token: 0x06001B99 RID: 7065 RVA: 0x000763BB File Offset: 0x000745BB
	public string FolderPath
	{
		get
		{
			return this.folderPath;
		}
		set
		{
			this.folderPath = value;
		}
	}

	// Token: 0x1700028E RID: 654
	// (get) Token: 0x06001B9A RID: 7066 RVA: 0x000763C4 File Offset: 0x000745C4
	// (set) Token: 0x06001B9B RID: 7067 RVA: 0x000763CC File Offset: 0x000745CC
	public bool SetStaticAfterCombine
	{
		get
		{
			return this.setStaticAfterCombine;
		}
		set
		{
			this.setStaticAfterCombine = value;
		}
	}

	// Token: 0x06001B9C RID: 7068 RVA: 0x000763D5 File Offset: 0x000745D5
	private void CheckDeactivateCombinedChildren()
	{
		if (this.deactivateCombinedChildren || this.deactivateCombinedChildrenMeshRenderers)
		{
			this.destroyCombinedChildren = false;
		}
	}

	// Token: 0x06001B9D RID: 7069 RVA: 0x000763EE File Offset: 0x000745EE
	private void CheckDestroyCombinedChildren()
	{
		if (this.destroyCombinedChildren)
		{
			this.deactivateCombinedChildren = false;
			this.deactivateCombinedChildrenMeshRenderers = false;
		}
	}

	// Token: 0x06001B9E RID: 7070 RVA: 0x00076406 File Offset: 0x00074606
	private void Start()
	{
		if (this.combineOnStart)
		{
			if (this.onlyAffectChildren)
			{
				this.CombineMeshes(false);
				return;
			}
			this.FindAndCombineAllActiveMeshes(false);
		}
	}

	// Token: 0x06001B9F RID: 7071 RVA: 0x00076428 File Offset: 0x00074628
	public void FindAndCombineAllActiveMeshes(bool showCreatedMeshInfo)
	{
		if (this.onlyAffectChildren)
		{
			this.CombineMeshes(showCreatedMeshInfo);
			return;
		}
		(from mf in Object.FindObjectsByType<MeshFilter>(FindObjectsSortMode.None)
		where mf.gameObject.activeInHierarchy && mf.transform.parent == null && !this.meshFiltersToSkip.Contains(mf) && !this.ShouldSkipMeshFilter(mf) && !this.ShouldSkipGameObject(mf.gameObject)
		select mf).ToArray<MeshFilter>();
		MeshFilter[] array = this.meshFiltersToSkip;
		this.meshFiltersToSkip = new MeshFilter[0];
		this.CombineMeshes(showCreatedMeshInfo);
		this.meshFiltersToSkip = array;
		if (this.setStaticAfterCombine)
		{
			base.gameObject.isStatic = true;
		}
	}

	// Token: 0x06001BA0 RID: 7072 RVA: 0x00076498 File Offset: 0x00074698
	public void CombineMeshes(bool showCreatedMeshInfo)
	{
		Vector3 localScale = base.transform.localScale;
		int siblingIndex = base.transform.GetSiblingIndex();
		Transform parent = base.transform.parent;
		base.transform.parent = null;
		Quaternion rotation = base.transform.rotation;
		Vector3 position = base.transform.position;
		Vector3 localScale2 = base.transform.localScale;
		base.transform.rotation = Quaternion.identity;
		base.transform.position = Vector3.zero;
		base.transform.localScale = Vector3.one;
		if (!this.createMultiMaterialMesh)
		{
			this.CombineMeshesWithSingleMaterial(showCreatedMeshInfo);
		}
		else
		{
			this.CombineMeshesWithMutliMaterial(showCreatedMeshInfo);
		}
		base.transform.rotation = rotation;
		base.transform.position = position;
		base.transform.localScale = localScale2;
		base.transform.parent = parent;
		base.transform.SetSiblingIndex(siblingIndex);
		base.transform.localScale = localScale;
		MeshFilter component = base.GetComponent<MeshFilter>();
		if (component != null && component.sharedMesh != null && this.addMeshCollider)
		{
			MeshCollider meshCollider = base.gameObject.GetComponent<MeshCollider>();
			if (meshCollider == null)
			{
				meshCollider = base.gameObject.AddComponent<MeshCollider>();
			}
			meshCollider.sharedMesh = component.sharedMesh;
		}
		if (this.setStaticAfterCombine)
		{
			base.gameObject.isStatic = true;
		}
	}

	// Token: 0x06001BA1 RID: 7073 RVA: 0x000765FC File Offset: 0x000747FC
	public void UndoCombine()
	{
		foreach (MeshRenderer meshRenderer in base.GetComponentsInChildren<MeshRenderer>(true))
		{
			if (!(meshRenderer == null) && !(meshRenderer.gameObject == base.gameObject))
			{
				meshRenderer.enabled = true;
				if (!meshRenderer.gameObject.activeSelf)
				{
					meshRenderer.gameObject.SetActive(true);
				}
			}
		}
		MeshFilter component = base.GetComponent<MeshFilter>();
		if (component != null)
		{
			component.sharedMesh = null;
		}
		MeshRenderer component2 = base.GetComponent<MeshRenderer>();
		if (component2 != null)
		{
			component2.sharedMaterials = new Material[0];
			component2.enabled = false;
		}
	}

	// Token: 0x06001BA2 RID: 7074 RVA: 0x000766A0 File Offset: 0x000748A0
	private MeshFilter[] GetMeshFiltersToCombine()
	{
		MeshCombiner.<>c__DisplayClass48_0 CS$<>8__locals1 = new MeshCombiner.<>c__DisplayClass48_0();
		CS$<>8__locals1.<>4__this = this;
		CS$<>8__locals1.meshFilters = base.GetComponentsInChildren<MeshFilter>(this.combineInactiveChildren);
		if (CS$<>8__locals1.meshFilters == null || CS$<>8__locals1.meshFilters.Length == 0)
		{
			return new MeshFilter[0];
		}
		if (CS$<>8__locals1.meshFilters.Length != 0 && CS$<>8__locals1.meshFilters[0] != null)
		{
			this.meshFiltersToSkip = (from meshFilter in this.meshFiltersToSkip
			where meshFilter != CS$<>8__locals1.meshFilters[0]
			select meshFilter).ToArray<MeshFilter>();
		}
		this.meshFiltersToSkip = (from meshFilter in this.meshFiltersToSkip
		where meshFilter != null
		select meshFilter).ToArray<MeshFilter>();
		this.gameObjectsToIgnore = (from go in this.gameObjectsToIgnore
		where go != null
		select go).ToArray<GameObject>();
		CS$<>8__locals1.meshFilters = (from meshFilter in CS$<>8__locals1.meshFilters
		where meshFilter != null && !CS$<>8__locals1.<>4__this.ShouldSkipMeshFilter(meshFilter)
		select meshFilter).ToArray<MeshFilter>();
		CS$<>8__locals1.meshFilters = (from meshFilter in CS$<>8__locals1.meshFilters
		where !CS$<>8__locals1.<>4__this.ShouldSkipGameObject(meshFilter.gameObject)
		select meshFilter).ToArray<MeshFilter>();
		int i;
		int j;
		for (i = 0; i < this.meshFiltersToSkip.Length; i = j + 1)
		{
			CS$<>8__locals1.meshFilters = (from meshFilter in CS$<>8__locals1.meshFilters
			where meshFilter != CS$<>8__locals1.<>4__this.meshFiltersToSkip[i]
			select meshFilter).ToArray<MeshFilter>();
			j = i;
		}
		return CS$<>8__locals1.meshFilters;
	}

	// Token: 0x06001BA3 RID: 7075 RVA: 0x00076838 File Offset: 0x00074A38
	private bool ShouldSkipMeshFilter(MeshFilter meshFilter)
	{
		if (meshFilter == null)
		{
			return false;
		}
		Transform transform = meshFilter.transform;
		while (transform != null)
		{
			if ((1 << transform.gameObject.layer & this.layersToIgnore.value) != 0)
			{
				return true;
			}
			transform = transform.parent;
		}
		return false;
	}

	// Token: 0x06001BA4 RID: 7076 RVA: 0x0007688C File Offset: 0x00074A8C
	private bool ShouldSkipGameObject(GameObject gameObject)
	{
		if (gameObject == null)
		{
			return false;
		}
		Transform transform = gameObject.transform;
		while (transform != null)
		{
			if (this.gameObjectsToIgnore.Contains(transform.gameObject))
			{
				return true;
			}
			transform = transform.parent;
		}
		return false;
	}

	// Token: 0x06001BA5 RID: 7077 RVA: 0x000768D4 File Offset: 0x00074AD4
	private void CombineMeshesWithSingleMaterial(bool showCreatedMeshInfo)
	{
		MeshFilter[] meshFiltersToCombine = this.GetMeshFiltersToCombine();
		if (meshFiltersToCombine.Length <= 1)
		{
			if (showCreatedMeshInfo)
			{
				Debug.LogWarning("No children meshes found to combine.");
			}
			return;
		}
		List<CombineInstance> list = new List<CombineInstance>();
		long num = 0L;
		for (int i = 1; i < meshFiltersToCombine.Length; i++)
		{
			if (meshFiltersToCombine[i] != null && meshFiltersToCombine[i].sharedMesh != null)
			{
				CombineInstance item = new CombineInstance
				{
					subMeshIndex = 0,
					mesh = meshFiltersToCombine[i].sharedMesh,
					transform = meshFiltersToCombine[i].transform.localToWorldMatrix
				};
				list.Add(item);
				num += (long)item.mesh.vertices.Length;
			}
		}
		if (list.Count == 0)
		{
			if (showCreatedMeshInfo)
			{
				Debug.LogWarning("No valid meshes found to combine.");
			}
			return;
		}
		CombineInstance[] array = list.ToArray();
		MeshRenderer[] componentsInChildren = base.GetComponentsInChildren<MeshRenderer>(this.combineInactiveChildren);
		if (componentsInChildren.Length >= 2 && componentsInChildren[0] != null)
		{
			if (componentsInChildren[1] != null && componentsInChildren[1].sharedMaterial != null)
			{
				componentsInChildren[0].sharedMaterials = new Material[1];
				componentsInChildren[0].sharedMaterial = componentsInChildren[1].sharedMaterial;
			}
			else
			{
				componentsInChildren[0].sharedMaterials = new Material[0];
			}
		}
		else if (componentsInChildren.Length != 0 && componentsInChildren[0] != null)
		{
			componentsInChildren[0].sharedMaterials = new Material[0];
		}
		Mesh mesh = new Mesh();
		mesh.name = base.name;
		if (num > 65535L)
		{
			mesh.indexFormat = IndexFormat.UInt32;
		}
		mesh.CombineMeshes(array);
		this.GenerateUV(mesh);
		meshFiltersToCombine[0].sharedMesh = mesh;
		this.DeactivateCombinedGameObjects(meshFiltersToCombine);
		if (showCreatedMeshInfo)
		{
			if (num <= 65535L)
			{
				Debug.Log(string.Concat(new string[]
				{
					"<color=#00cc00><b>Mesh \"",
					base.name,
					"\" was created from ",
					array.Length.ToString(),
					" children meshes and has ",
					num.ToString(),
					" vertices.</b></color>"
				}));
				return;
			}
			Debug.Log(string.Concat(new string[]
			{
				"<color=#ff3300><b>Mesh \"",
				base.name,
				"\" was created from ",
				array.Length.ToString(),
				" children meshes and has ",
				num.ToString(),
				" vertices. Some old devices, like Android with Mali-400 GPU, do not support over 65535 vertices.</b></color>"
			}));
		}
	}

	// Token: 0x06001BA6 RID: 7078 RVA: 0x00076B2C File Offset: 0x00074D2C
	private void CombineMeshesWithMutliMaterial(bool showCreatedMeshInfo)
	{
		MeshFilter[] meshFiltersToCombine = this.GetMeshFiltersToCombine();
		if (meshFiltersToCombine.Length <= 1)
		{
			if (showCreatedMeshInfo)
			{
				Debug.LogWarning("No children meshes found to combine.");
			}
			return;
		}
		MeshRenderer[] array = new MeshRenderer[meshFiltersToCombine.Length];
		array[0] = base.GetComponent<MeshRenderer>();
		List<Material> list = new List<Material>();
		for (int i = 1; i < meshFiltersToCombine.Length; i++)
		{
			if (meshFiltersToCombine[i] != null && meshFiltersToCombine[i].sharedMesh != null)
			{
				array[i] = meshFiltersToCombine[i].GetComponent<MeshRenderer>();
				if (array[i] != null)
				{
					Material[] sharedMaterials = array[i].sharedMaterials;
					for (int j = 0; j < sharedMaterials.Length; j++)
					{
						if (sharedMaterials[j] != null && !list.Contains(sharedMaterials[j]))
						{
							list.Add(sharedMaterials[j]);
						}
					}
				}
			}
		}
		if (list.Count == 0)
		{
			if (showCreatedMeshInfo)
			{
				Debug.LogWarning("No materials found in children meshes to combine.");
			}
			return;
		}
		List<CombineInstance> list2 = new List<CombineInstance>();
		long num = 0L;
		for (int k = 0; k < list.Count; k++)
		{
			List<CombineInstance> list3 = new List<CombineInstance>();
			for (int l = 1; l < meshFiltersToCombine.Length; l++)
			{
				if (meshFiltersToCombine[l] != null && meshFiltersToCombine[l].sharedMesh != null && array[l] != null)
				{
					Material[] sharedMaterials2 = array[l].sharedMaterials;
					for (int m = 0; m < sharedMaterials2.Length; m++)
					{
						if (sharedMaterials2[m] != null && list[k] == sharedMaterials2[m])
						{
							CombineInstance item = new CombineInstance
							{
								subMeshIndex = m,
								mesh = meshFiltersToCombine[l].sharedMesh,
								transform = meshFiltersToCombine[l].transform.localToWorldMatrix
							};
							list3.Add(item);
							num += (long)item.mesh.vertices.Length;
						}
					}
				}
			}
			Mesh mesh = new Mesh();
			if (num > 65535L)
			{
				mesh.indexFormat = IndexFormat.UInt32;
			}
			mesh.CombineMeshes(list3.ToArray(), true);
			list2.Add(new CombineInstance
			{
				subMeshIndex = 0,
				mesh = mesh,
				transform = Matrix4x4.identity
			});
		}
		if (list2.Count == 0)
		{
			if (showCreatedMeshInfo)
			{
				Debug.LogWarning("No valid meshes found to combine.");
			}
			return;
		}
		if (array[0] != null)
		{
			array[0].sharedMaterials = list.ToArray();
		}
		Mesh mesh2 = new Mesh();
		mesh2.name = base.name;
		if (num > 65535L)
		{
			mesh2.indexFormat = IndexFormat.UInt32;
		}
		mesh2.CombineMeshes(list2.ToArray(), false);
		this.GenerateUV(mesh2);
		meshFiltersToCombine[0].sharedMesh = mesh2;
		this.DeactivateCombinedGameObjects(meshFiltersToCombine);
		if (showCreatedMeshInfo)
		{
			int num2 = meshFiltersToCombine.Length - 1;
			if (num <= 65535L)
			{
				Debug.Log(string.Concat(new string[]
				{
					"<color=#00cc00><b>Mesh \"",
					base.name,
					"\" was created from ",
					num2.ToString(),
					" children meshes and has ",
					list2.Count.ToString(),
					" submeshes, and ",
					num.ToString(),
					" vertices.</b></color>"
				}));
				return;
			}
			Debug.Log(string.Concat(new string[]
			{
				"<color=#ff3300><b>Mesh \"",
				base.name,
				"\" was created from ",
				num2.ToString(),
				" children meshes and has ",
				list2.Count.ToString(),
				" submeshes, and ",
				num.ToString(),
				" vertices. Some old devices, like Android with Mali-400 GPU, do not support over 65535 vertices.</b></color>"
			}));
		}
	}

	// Token: 0x06001BA7 RID: 7079 RVA: 0x00076ED4 File Offset: 0x000750D4
	private void DeactivateCombinedGameObjects(MeshFilter[] meshFilters)
	{
		for (int i = 1; i < meshFilters.Length; i++)
		{
			if (!(meshFilters[i] == null))
			{
				if (!this.destroyCombinedChildren)
				{
					if (this.deactivateCombinedChildren)
					{
						meshFilters[i].gameObject.SetActive(false);
					}
					if (this.deactivateCombinedChildrenMeshRenderers)
					{
						MeshRenderer component = meshFilters[i].gameObject.GetComponent<MeshRenderer>();
						if (component != null)
						{
							component.enabled = false;
						}
					}
				}
				else
				{
					Object.Destroy(meshFilters[i].gameObject);
				}
			}
		}
	}

	// Token: 0x06001BA8 RID: 7080 RVA: 0x000048A7 File Offset: 0x00002AA7
	private void GenerateUV(Mesh combinedMesh)
	{
	}

	// Token: 0x04001275 RID: 4725
	private const int Mesh16BitBufferVertexLimit = 65535;

	// Token: 0x04001276 RID: 4726
	[SerializeField]
	private bool createMultiMaterialMesh;

	// Token: 0x04001277 RID: 4727
	[SerializeField]
	private bool combineInactiveChildren;

	// Token: 0x04001278 RID: 4728
	[SerializeField]
	private bool deactivateCombinedChildren = true;

	// Token: 0x04001279 RID: 4729
	[SerializeField]
	private bool deactivateCombinedChildrenMeshRenderers;

	// Token: 0x0400127A RID: 4730
	[SerializeField]
	private bool generateUVMap;

	// Token: 0x0400127B RID: 4731
	[SerializeField]
	private bool destroyCombinedChildren;

	// Token: 0x0400127C RID: 4732
	[SerializeField]
	private bool addMeshCollider;

	// Token: 0x0400127D RID: 4733
	[SerializeField]
	private bool onlyAffectChildren;

	// Token: 0x0400127E RID: 4734
	[SerializeField]
	private bool combineOnStart;

	// Token: 0x0400127F RID: 4735
	[SerializeField]
	private string folderPath = "Content/Baking/CombinedMeshes";

	// Token: 0x04001280 RID: 4736
	[SerializeField]
	private bool setStaticAfterCombine;

	// Token: 0x04001281 RID: 4737
	[SerializeField]
	[Tooltip("MeshFilters with Meshes which we don't want to combine into one Mesh.")]
	private MeshFilter[] meshFiltersToSkip = new MeshFilter[0];

	// Token: 0x04001282 RID: 4738
	[SerializeField]
	[Tooltip("GameObjects to ignore when combining meshes. These GameObjects and their children will be skipped.")]
	private GameObject[] gameObjectsToIgnore = new GameObject[0];

	// Token: 0x04001283 RID: 4739
	[Tooltip("Layers to ignore when combining meshes. Objects on these layers and their children will be skipped.")]
	public LayerMask layersToIgnore = 0;
}
