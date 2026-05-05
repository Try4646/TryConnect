using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Sirenix.Serialization;
using UnityEngine;

// Token: 0x020002DD RID: 733
[ExecuteAlways]
public class SceneMaterialAuditor : MonoBehaviour
{
	// Token: 0x17000267 RID: 615
	// (get) Token: 0x060019AE RID: 6574 RVA: 0x0006B882 File Offset: 0x00069A82
	public IReadOnlyList<SceneMaterialAuditor.MaterialRecord> Materials
	{
		get
		{
			return this.materials;
		}
	}

	// Token: 0x060019AF RID: 6575 RVA: 0x0006B88C File Offset: 0x00069A8C
	[ContextMenu("Scan Scene Materials")]
	public void Scan()
	{
		this.materials.Clear();
		this.grouped.Clear();
		Dictionary<Material, SceneMaterialAuditor.MaterialRecord> dictionary = new Dictionary<Material, SceneMaterialAuditor.MaterialRecord>(256);
		Renderer[] array = Object.FindObjectsByType<Renderer>(this.includeInactive ? FindObjectsInactive.Include : FindObjectsInactive.Exclude, FindObjectsSortMode.None);
		this.totalRendererCount = array.Length;
		foreach (Renderer renderer in array)
		{
			if (!(renderer == null) && (this.includeAllRenderers || renderer is MeshRenderer || renderer is SkinnedMeshRenderer) && (this.includeSpriteRenderers || !(renderer is SpriteRenderer)))
			{
				Material[] sharedMaterials = renderer.sharedMaterials;
				if (sharedMaterials != null && sharedMaterials.Length != 0)
				{
					string transformPath = SceneMaterialAuditor.GetTransformPath(renderer.transform);
					foreach (Material material in sharedMaterials)
					{
						if (!(material == null))
						{
							string empty = string.Empty;
							bool flag = !string.IsNullOrEmpty(empty);
							if (this.includeNonAssetMaterials || flag)
							{
								SceneMaterialAuditor.MaterialRecord materialRecord;
								if (!dictionary.TryGetValue(material, out materialRecord))
								{
									materialRecord = new SceneMaterialAuditor.MaterialRecord
									{
										material = material,
										materialName = material.name,
										shaderName = (material.shader ? material.shader.name : "(No Shader)"),
										assetPath = (flag ? empty : "(Non-Asset / Built-in / Runtime)"),
										folder = (flag ? SceneMaterialAuditor.NormalizeFolder(Path.GetDirectoryName(empty)) : "(No Asset Folder)"),
										users = new List<string>(4)
									};
									dictionary.Add(material, materialRecord);
								}
								if (!materialRecord.users.Contains(transformPath))
								{
									materialRecord.users.Add(transformPath);
								}
							}
						}
					}
				}
			}
		}
		this.materials = (from r in dictionary.Values
		orderby r.UserCount descending
		select r).ThenBy((SceneMaterialAuditor.MaterialRecord r) => r.shaderName, StringComparer.OrdinalIgnoreCase).ThenBy((SceneMaterialAuditor.MaterialRecord r) => r.folder, StringComparer.OrdinalIgnoreCase).ThenBy((SceneMaterialAuditor.MaterialRecord r) => r.materialName, StringComparer.OrdinalIgnoreCase).ToList<SceneMaterialAuditor.MaterialRecord>();
		this.uniqueMaterialCount = this.materials.Count;
		this.BuildGroupedView();
	}

	// Token: 0x060019B0 RID: 6576 RVA: 0x0006BB18 File Offset: 0x00069D18
	[ContextMenu("Clear Results")]
	public void Clear()
	{
		this.materials.Clear();
		this.uniqueMaterialCount = 0;
		this.totalRendererCount = 0;
		this.grouped.Clear();
	}

	// Token: 0x060019B1 RID: 6577 RVA: 0x000048A7 File Offset: 0x00002AA7
	private void OnValidate()
	{
	}

	// Token: 0x060019B2 RID: 6578 RVA: 0x0006BB40 File Offset: 0x00069D40
	private void BuildGroupedView()
	{
		this.grouped = new Dictionary<string, Dictionary<string, List<SceneMaterialAuditor.MaterialRecord>>>(StringComparer.OrdinalIgnoreCase);
		foreach (SceneMaterialAuditor.MaterialRecord materialRecord in this.materials)
		{
			string key = string.IsNullOrEmpty(materialRecord.shaderName) ? "(No Shader)" : materialRecord.shaderName;
			string key2 = string.IsNullOrEmpty(materialRecord.folder) ? "(No Folder)" : materialRecord.folder;
			Dictionary<string, List<SceneMaterialAuditor.MaterialRecord>> dictionary;
			if (!this.grouped.TryGetValue(key, out dictionary))
			{
				dictionary = new Dictionary<string, List<SceneMaterialAuditor.MaterialRecord>>(StringComparer.OrdinalIgnoreCase);
				this.grouped.Add(key, dictionary);
			}
			List<SceneMaterialAuditor.MaterialRecord> list;
			if (!dictionary.TryGetValue(key2, out list))
			{
				list = new List<SceneMaterialAuditor.MaterialRecord>();
				dictionary.Add(key2, list);
			}
			list.Add(materialRecord);
		}
		foreach (Dictionary<string, List<SceneMaterialAuditor.MaterialRecord>> dictionary2 in this.grouped.Values)
		{
			using (Dictionary<string, List<SceneMaterialAuditor.MaterialRecord>>.ValueCollection.Enumerator enumerator3 = dictionary2.Values.GetEnumerator())
			{
				while (enumerator3.MoveNext())
				{
					enumerator3.Current.Sort(delegate(SceneMaterialAuditor.MaterialRecord a, SceneMaterialAuditor.MaterialRecord b)
					{
						int num = b.UserCount.CompareTo(a.UserCount);
						if (num != 0)
						{
							return num;
						}
						return string.Compare(a.materialName, b.materialName, StringComparison.OrdinalIgnoreCase);
					});
				}
			}
		}
	}

	// Token: 0x060019B3 RID: 6579 RVA: 0x0006BCC8 File Offset: 0x00069EC8
	private static string GetTransformPath(Transform t)
	{
		if (t == null)
		{
			return "(null)";
		}
		Stack<string> stack = new Stack<string>(16);
		Transform transform = t;
		while (transform != null)
		{
			stack.Push(transform.name);
			transform = transform.parent;
		}
		return string.Join("/", stack);
	}

	// Token: 0x060019B4 RID: 6580 RVA: 0x0006BD17 File Offset: 0x00069F17
	private static string NormalizeFolder(string folder)
	{
		if (string.IsNullOrEmpty(folder))
		{
			return "(Root)";
		}
		folder = folder.Replace("\\", "/");
		return folder;
	}

	// Token: 0x04001070 RID: 4208
	[Header("Scan Options")]
	[Tooltip("Include inactive GameObjects in the scan.")]
	public bool includeInactive = true;

	// Token: 0x04001071 RID: 4209
	[Tooltip("Include SpriteRenderers in the scan.")]
	public bool includeSpriteRenderers = true;

	// Token: 0x04001072 RID: 4210
	[Tooltip("Include all Renderer types (ParticleSystemRenderer, LineRenderer, etc.). If false, only MeshRenderer + SkinnedMeshRenderer.")]
	public bool includeAllRenderers = true;

	// Token: 0x04001073 RID: 4211
	[Tooltip("If true, scans will include materials that are not project assets (e.g., built-in / runtime-created).")]
	public bool includeNonAssetMaterials = true;

	// Token: 0x04001074 RID: 4212
	[Header("Results Summary")]
	[SerializeField]
	private string lastScanInfo;

	// Token: 0x04001075 RID: 4213
	[SerializeField]
	private int uniqueMaterialCount;

	// Token: 0x04001076 RID: 4214
	[SerializeField]
	private int totalRendererCount;

	// Token: 0x04001077 RID: 4215
	[SerializeField]
	private List<SceneMaterialAuditor.MaterialRecord> materials = new List<SceneMaterialAuditor.MaterialRecord>();

	// Token: 0x04001078 RID: 4216
	[OdinSerialize]
	private Dictionary<string, Dictionary<string, List<SceneMaterialAuditor.MaterialRecord>>> grouped = new Dictionary<string, Dictionary<string, List<SceneMaterialAuditor.MaterialRecord>>>();

	// Token: 0x04001079 RID: 4217
	[Header("Optional Auto Refresh")]
	[Tooltip("If enabled, will rescan when options change in Inspector (Editor only).")]
	public bool rescanOnValidate;

	// Token: 0x020002DE RID: 734
	[Serializable]
	public class MaterialRecord
	{
		// Token: 0x17000268 RID: 616
		// (get) Token: 0x060019B6 RID: 6582 RVA: 0x0006BD74 File Offset: 0x00069F74
		public int UserCount
		{
			get
			{
				if (this.users == null)
				{
					return 0;
				}
				return this.users.Count;
			}
		}

		// Token: 0x0400107A RID: 4218
		public Material material;

		// Token: 0x0400107B RID: 4219
		public string materialName;

		// Token: 0x0400107C RID: 4220
		public string shaderName;

		// Token: 0x0400107D RID: 4221
		public string assetPath;

		// Token: 0x0400107E RID: 4222
		public string folder;

		// Token: 0x0400107F RID: 4223
		public List<string> users;
	}

	// Token: 0x020002DF RID: 735
	[Serializable]
	private class MaterialExport
	{
		// Token: 0x04001080 RID: 4224
		public List<SceneMaterialAuditor.MaterialRecord> records;
	}

	// Token: 0x020002E0 RID: 736
	[Serializable]
	private class ShaderGroup
	{
		// Token: 0x04001081 RID: 4225
		public string shaderName;

		// Token: 0x04001082 RID: 4226
		public List<SceneMaterialAuditor.FolderGroup> folders;
	}

	// Token: 0x020002E1 RID: 737
	[Serializable]
	private class FolderGroup
	{
		// Token: 0x04001083 RID: 4227
		public string folder;

		// Token: 0x04001084 RID: 4228
		public List<SceneMaterialAuditor.MaterialRecord> records;
	}
}
