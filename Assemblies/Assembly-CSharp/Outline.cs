using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

// Token: 0x0200034B RID: 843
public class Outline : MonoBehaviour
{
	// Token: 0x1700028F RID: 655
	// (get) Token: 0x06001BB5 RID: 7093 RVA: 0x0007704E File Offset: 0x0007524E
	// (set) Token: 0x06001BB6 RID: 7094 RVA: 0x00077056 File Offset: 0x00075256
	public Outline.Mode OutlineMode
	{
		get
		{
			return this.outlineMode;
		}
		set
		{
			this.outlineMode = value;
			this.needsUpdate = true;
		}
	}

	// Token: 0x17000290 RID: 656
	// (get) Token: 0x06001BB7 RID: 7095 RVA: 0x00077066 File Offset: 0x00075266
	// (set) Token: 0x06001BB8 RID: 7096 RVA: 0x0007706E File Offset: 0x0007526E
	public Color OutlineColor
	{
		get
		{
			return this.outlineColor;
		}
		set
		{
			this.outlineColor = value;
			this.needsUpdate = true;
		}
	}

	// Token: 0x17000291 RID: 657
	// (get) Token: 0x06001BB9 RID: 7097 RVA: 0x0007707E File Offset: 0x0007527E
	// (set) Token: 0x06001BBA RID: 7098 RVA: 0x00077086 File Offset: 0x00075286
	public float OutlineWidth
	{
		get
		{
			return this.outlineWidth;
		}
		set
		{
			this.outlineWidth = value;
			this.needsUpdate = true;
		}
	}

	// Token: 0x06001BBB RID: 7099 RVA: 0x00077098 File Offset: 0x00075298
	private void Awake()
	{
		this.CacheRenderers();
		this.outlineMaskMaterial = Object.Instantiate<Material>(Resources.Load<Material>("Materials/OutlineMask"));
		this.outlineFillMaterial = Object.Instantiate<Material>(Resources.Load<Material>("Materials/OutlineFill"));
		this.outlineMaskMaterial.name = "OutlineMask (Instance)";
		this.outlineFillMaterial.name = "OutlineFill (Instance)";
		this.LoadSmoothNormals();
		this.needsUpdate = true;
	}

	// Token: 0x06001BBC RID: 7100 RVA: 0x00077102 File Offset: 0x00075302
	public void CacheRenderers()
	{
		this.renderers = (from r in base.GetComponentsInChildren<Renderer>()
		where r.gameObject.layer != LayerMask.NameToLayer("DisableOutline")
		select r).ToArray<Renderer>();
	}

	// Token: 0x06001BBD RID: 7101 RVA: 0x0007713C File Offset: 0x0007533C
	private void OnEnable()
	{
		foreach (Renderer renderer in this.renderers)
		{
			if (!(renderer == null))
			{
				List<Material> list = renderer.sharedMaterials.ToList<Material>();
				list.Add(this.outlineMaskMaterial);
				list.Add(this.outlineFillMaterial);
				renderer.materials = list.ToArray();
			}
		}
	}

	// Token: 0x06001BBE RID: 7102 RVA: 0x0007719C File Offset: 0x0007539C
	private void OnValidate()
	{
		this.needsUpdate = true;
		if ((!this.precomputeOutline && this.bakeKeys.Count != 0) || this.bakeKeys.Count != this.bakeValues.Count)
		{
			this.bakeKeys.Clear();
			this.bakeValues.Clear();
		}
		if (this.precomputeOutline && this.bakeKeys.Count == 0)
		{
			this.Bake();
		}
	}

	// Token: 0x06001BBF RID: 7103 RVA: 0x0007720E File Offset: 0x0007540E
	private void Update()
	{
		if (this.needsUpdate)
		{
			this.needsUpdate = false;
			this.UpdateMaterialProperties();
		}
	}

	// Token: 0x06001BC0 RID: 7104 RVA: 0x00077228 File Offset: 0x00075428
	private void OnDisable()
	{
		foreach (Renderer renderer in this.renderers)
		{
			if (!(renderer == null))
			{
				List<Material> list = renderer.sharedMaterials.ToList<Material>();
				list.Remove(this.outlineMaskMaterial);
				list.Remove(this.outlineFillMaterial);
				renderer.materials = list.ToArray();
			}
		}
	}

	// Token: 0x06001BC1 RID: 7105 RVA: 0x00077289 File Offset: 0x00075489
	private void OnDestroy()
	{
		Object.Destroy(this.outlineMaskMaterial);
		Object.Destroy(this.outlineFillMaterial);
	}

	// Token: 0x06001BC2 RID: 7106 RVA: 0x000772A4 File Offset: 0x000754A4
	private void Bake()
	{
		HashSet<Mesh> hashSet = new HashSet<Mesh>();
		foreach (MeshFilter meshFilter in base.GetComponentsInChildren<MeshFilter>())
		{
			if (hashSet.Add(meshFilter.sharedMesh))
			{
				List<Vector3> data = this.SmoothNormals(meshFilter.sharedMesh);
				this.bakeKeys.Add(meshFilter.sharedMesh);
				this.bakeValues.Add(new Outline.ListVector3
				{
					data = data
				});
			}
		}
	}

	// Token: 0x06001BC3 RID: 7107 RVA: 0x00077318 File Offset: 0x00075518
	private void LoadSmoothNormals()
	{
		foreach (MeshFilter meshFilter in base.GetComponentsInChildren<MeshFilter>())
		{
			if (Outline.registeredMeshes.Add(meshFilter.sharedMesh))
			{
				int num = this.bakeKeys.IndexOf(meshFilter.sharedMesh);
				List<Vector3> uvs = (num >= 0) ? this.bakeValues[num].data : this.SmoothNormals(meshFilter.sharedMesh);
				meshFilter.sharedMesh.SetUVs(3, uvs);
				Renderer component = meshFilter.GetComponent<Renderer>();
				if (component != null)
				{
					this.CombineSubmeshes(meshFilter.sharedMesh, component.sharedMaterials);
				}
			}
		}
		foreach (SkinnedMeshRenderer skinnedMeshRenderer in base.GetComponentsInChildren<SkinnedMeshRenderer>())
		{
			if (Outline.registeredMeshes.Add(skinnedMeshRenderer.sharedMesh))
			{
				skinnedMeshRenderer.sharedMesh.uv4 = new Vector2[skinnedMeshRenderer.sharedMesh.vertexCount];
				this.CombineSubmeshes(skinnedMeshRenderer.sharedMesh, skinnedMeshRenderer.sharedMaterials);
			}
		}
	}

	// Token: 0x06001BC4 RID: 7108 RVA: 0x00077424 File Offset: 0x00075624
	private List<Vector3> SmoothNormals(Mesh mesh)
	{
		IEnumerable<IGrouping<Vector3, KeyValuePair<Vector3, int>>> enumerable = from pair in mesh.vertices.Select((Vector3 vertex, int index) => new KeyValuePair<Vector3, int>(vertex, index))
		group pair by pair.Key;
		List<Vector3> list = new List<Vector3>(mesh.normals);
		foreach (IGrouping<Vector3, KeyValuePair<Vector3, int>> grouping in enumerable)
		{
			if (grouping.Count<KeyValuePair<Vector3, int>>() != 1)
			{
				Vector3 vector = Vector3.zero;
				foreach (KeyValuePair<Vector3, int> keyValuePair in grouping)
				{
					vector += list[keyValuePair.Value];
				}
				vector.Normalize();
				foreach (KeyValuePair<Vector3, int> keyValuePair2 in grouping)
				{
					list[keyValuePair2.Value] = vector;
				}
			}
		}
		return list;
	}

	// Token: 0x06001BC5 RID: 7109 RVA: 0x0007756C File Offset: 0x0007576C
	private void CombineSubmeshes(Mesh mesh, Material[] materials)
	{
		if (mesh.subMeshCount == 1)
		{
			return;
		}
		if (mesh.subMeshCount > materials.Length)
		{
			return;
		}
		int subMeshCount = mesh.subMeshCount;
		mesh.subMeshCount = subMeshCount + 1;
		mesh.SetTriangles(mesh.triangles, mesh.subMeshCount - 1);
	}

	// Token: 0x06001BC6 RID: 7110 RVA: 0x000775B4 File Offset: 0x000757B4
	private void UpdateMaterialProperties()
	{
		this.outlineFillMaterial.SetColor("_OutlineColor", this.outlineColor);
		switch (this.outlineMode)
		{
		case Outline.Mode.OutlineAll:
			this.outlineMaskMaterial.SetFloat("_ZTest", 8f);
			this.outlineFillMaterial.SetFloat("_ZTest", 8f);
			this.outlineFillMaterial.SetFloat("_OutlineWidth", this.outlineWidth);
			return;
		case Outline.Mode.OutlineVisible:
			this.outlineMaskMaterial.SetFloat("_ZTest", 8f);
			this.outlineFillMaterial.SetFloat("_ZTest", 4f);
			this.outlineFillMaterial.SetFloat("_OutlineWidth", this.outlineWidth);
			return;
		case Outline.Mode.OutlineHidden:
			this.outlineMaskMaterial.SetFloat("_ZTest", 8f);
			this.outlineFillMaterial.SetFloat("_ZTest", 5f);
			this.outlineFillMaterial.SetFloat("_OutlineWidth", this.outlineWidth);
			return;
		case Outline.Mode.OutlineAndSilhouette:
			this.outlineMaskMaterial.SetFloat("_ZTest", 4f);
			this.outlineFillMaterial.SetFloat("_ZTest", 8f);
			this.outlineFillMaterial.SetFloat("_OutlineWidth", this.outlineWidth);
			return;
		case Outline.Mode.SilhouetteOnly:
			this.outlineMaskMaterial.SetFloat("_ZTest", 4f);
			this.outlineFillMaterial.SetFloat("_ZTest", 5f);
			this.outlineFillMaterial.SetFloat("_OutlineWidth", 0f);
			return;
		default:
			return;
		}
	}

	// Token: 0x0400128B RID: 4747
	private static HashSet<Mesh> registeredMeshes = new HashSet<Mesh>();

	// Token: 0x0400128C RID: 4748
	[SerializeField]
	private Outline.Mode outlineMode;

	// Token: 0x0400128D RID: 4749
	[SerializeField]
	private Color outlineColor = Color.white;

	// Token: 0x0400128E RID: 4750
	[SerializeField]
	[Range(0f, 10f)]
	private float outlineWidth = 2f;

	// Token: 0x0400128F RID: 4751
	[Header("Optional")]
	[SerializeField]
	[Tooltip("Precompute enabled: Per-vertex calculations are performed in the editor and serialized with the object. Precompute disabled: Per-vertex calculations are performed at runtime in Awake(). This may cause a pause for large meshes.")]
	private bool precomputeOutline;

	// Token: 0x04001290 RID: 4752
	[SerializeField]
	[HideInInspector]
	private List<Mesh> bakeKeys = new List<Mesh>();

	// Token: 0x04001291 RID: 4753
	[SerializeField]
	[HideInInspector]
	private List<Outline.ListVector3> bakeValues = new List<Outline.ListVector3>();

	// Token: 0x04001292 RID: 4754
	private Renderer[] renderers;

	// Token: 0x04001293 RID: 4755
	private Material outlineMaskMaterial;

	// Token: 0x04001294 RID: 4756
	private Material outlineFillMaterial;

	// Token: 0x04001295 RID: 4757
	private bool needsUpdate;

	// Token: 0x0200034C RID: 844
	public enum Mode
	{
		// Token: 0x04001297 RID: 4759
		OutlineAll,
		// Token: 0x04001298 RID: 4760
		OutlineVisible,
		// Token: 0x04001299 RID: 4761
		OutlineHidden,
		// Token: 0x0400129A RID: 4762
		OutlineAndSilhouette,
		// Token: 0x0400129B RID: 4763
		SilhouetteOnly
	}

	// Token: 0x0200034D RID: 845
	[Serializable]
	private class ListVector3
	{
		// Token: 0x0400129C RID: 4764
		public List<Vector3> data;
	}
}
