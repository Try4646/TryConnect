using System;
using System.Collections.Generic;
using Extensions;
using UnityEngine;

// Token: 0x020002C1 RID: 705
public class LightbakerManager : MonoSingleton<LightbakerManager>
{
	// Token: 0x060018F5 RID: 6389 RVA: 0x00069115 File Offset: 0x00067315
	public void UpdateActiveLightVolume()
	{
		this.SetShaderVars();
	}

	// Token: 0x060018F6 RID: 6390 RVA: 0x0006911D File Offset: 0x0006731D
	public void SetLightVolume(int index)
	{
		this.targetLightVolume = this.lightVolumes[index];
		this.SetShaderVars();
	}

	// Token: 0x060018F7 RID: 6391 RVA: 0x00069138 File Offset: 0x00067338
	private void SetShaderVars()
	{
		Texture3D value = Resources.Load<Texture3D>(this.targetLightVolume.fileName);
		Shader.SetGlobalTexture("_LightMap", value);
		Shader.SetGlobalFloat("brightness", this.targetLightVolume.brightness);
		Shader.SetGlobalFloat("ambienceStrength", this.targetLightVolume.ambienceStrength);
		Shader.SetGlobalFloat("ambienceMin", this.targetLightVolume.ambienceMin);
		Shader.SetGlobalVector("gridRes", this.targetLightVolume.gridRes);
		Shader.SetGlobalFloat("raySpacing", this.targetLightVolume.raySpacing);
		Shader.SetGlobalVector("gridOffset", this.targetLightVolume.gridOffset);
	}

	// Token: 0x04001010 RID: 4112
	public List<LightVolume> lightVolumes;

	// Token: 0x04001011 RID: 4113
	public LightVolume targetLightVolume;
}
