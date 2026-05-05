using System;
using UnityEngine;
using UnityEngine.Rendering.Universal;

// Token: 0x0200031A RID: 794
[CreateAssetMenu(menuName = "Game Settings/Graphics Settings", fileName = "GraphicsSettings")]
public class GraphicsSettings : ScriptableObject
{
	// Token: 0x14000023 RID: 35
	// (add) Token: 0x06001AC7 RID: 6855 RVA: 0x00071878 File Offset: 0x0006FA78
	// (remove) Token: 0x06001AC8 RID: 6856 RVA: 0x000718AC File Offset: 0x0006FAAC
	public static event Action<GraphicsSettings> SettingsChanged;

	// Token: 0x06001AC9 RID: 6857 RVA: 0x000718DF File Offset: 0x0006FADF
	public void NotifyChanged()
	{
		Action<GraphicsSettings> settingsChanged = GraphicsSettings.SettingsChanged;
		if (settingsChanged == null)
		{
			return;
		}
		settingsChanged(this);
	}

	// Token: 0x06001ACA RID: 6858 RVA: 0x000718F4 File Offset: 0x0006FAF4
	public UniversalRenderPipelineAsset GetQualityAsset()
	{
		UniversalRenderPipelineAsset result;
		switch (this.qualityLevel)
		{
		case GraphicsSettings.QualityLevel.High:
			result = this.highQualityAsset;
			break;
		case GraphicsSettings.QualityLevel.Medium:
			result = this.mediumQualityAsset;
			break;
		case GraphicsSettings.QualityLevel.Low:
			result = this.lowQualityAsset;
			break;
		default:
			result = this.mediumQualityAsset;
			break;
		}
		return result;
	}

	// Token: 0x0400118E RID: 4494
	[Header("Quality Settings")]
	[Tooltip("Overall quality level: Low, Medium, or High")]
	public GraphicsSettings.QualityLevel qualityLevel = GraphicsSettings.QualityLevel.Medium;

	// Token: 0x0400118F RID: 4495
	[Header("Quality Level Assets")]
	[Tooltip("URP Asset for Low quality")]
	public UniversalRenderPipelineAsset lowQualityAsset;

	// Token: 0x04001190 RID: 4496
	[Tooltip("URP Asset for Medium quality")]
	public UniversalRenderPipelineAsset mediumQualityAsset;

	// Token: 0x04001191 RID: 4497
	[Tooltip("URP Asset for High quality")]
	public UniversalRenderPipelineAsset highQualityAsset;

	// Token: 0x04001192 RID: 4498
	[Header("Render Settings")]
	[Tooltip("Render scale (0.5 = 50%, 1.0 = 100%, 1.5 = 150%)")]
	[Range(0.5f, 2f)]
	public float renderScale = 1f;

	// Token: 0x04001193 RID: 4499
	[Tooltip("Enable HDR rendering")]
	public bool enableHDR = true;

	// Token: 0x04001194 RID: 4500
	[Header("Display Settings")]
	[Tooltip("Screen brightness adjustment (0.0 = black, 1.0 = normal, 2.0 = double brightness)")]
	[Range(0f, 2f)]
	public float brightness = 1f;

	// Token: 0x04001195 RID: 4501
	[Tooltip("Film grain strength (0 = off, 1 = full)")]
	[Range(0f, 1f)]
	public float filmGrain = 1f;

	// Token: 0x0200031B RID: 795
	public enum QualityLevel
	{
		// Token: 0x04001198 RID: 4504
		High,
		// Token: 0x04001199 RID: 4505
		Medium,
		// Token: 0x0400119A RID: 4506
		Low
	}
}
