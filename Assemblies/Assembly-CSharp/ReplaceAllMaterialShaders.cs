using System;
using System.Collections.Generic;
using UnityEngine;

// Token: 0x02000346 RID: 838
public class ReplaceAllMaterialShaders : MonoBehaviour
{
	// Token: 0x06001B88 RID: 7048 RVA: 0x000762A4 File Offset: 0x000744A4
	private void Update()
	{
		MeshRenderer[] componentsInChildren = base.GetComponentsInChildren<MeshRenderer>();
		List<Material> list = new List<Material>();
		MeshRenderer[] array = componentsInChildren;
		for (int i = 0; i < array.Length; i++)
		{
			foreach (Material material in array[i].sharedMaterials)
			{
				if (!list.Contains(material) && material != null && material.shader != this.targetShader)
				{
					material.shader = this.targetShader;
					list.Add(material);
				}
			}
		}
	}

	// Token: 0x04001274 RID: 4724
	public Shader targetShader;
}
