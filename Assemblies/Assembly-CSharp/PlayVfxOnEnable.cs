using System;
using UnityEngine;

// Token: 0x020002D3 RID: 723
public class PlayVfxOnEnable : MonoBehaviour
{
	// Token: 0x06001972 RID: 6514 RVA: 0x0006AD7F File Offset: 0x00068F7F
	private void OnEnable()
	{
		this.targetParticles.Play();
	}

	// Token: 0x06001973 RID: 6515 RVA: 0x0006AD8C File Offset: 0x00068F8C
	private void OnDisable()
	{
		this.targetParticles.Stop();
	}

	// Token: 0x0400105D RID: 4189
	[SerializeField]
	private ParticleSystem targetParticles;
}
