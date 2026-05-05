using System;
using UnityEngine;

// Token: 0x02000341 RID: 833
public class BakedVolumeLight : MonoBehaviour
{
	// Token: 0x06001B6D RID: 7021 RVA: 0x00075354 File Offset: 0x00073554
	private void OnDrawGizmosSelected()
	{
		Gizmos.color = this.color;
		BakedVolumeLight.LightModes lightModes = this.mode;
		if (lightModes == BakedVolumeLight.LightModes.Point)
		{
			Gizmos.DrawWireSphere(base.transform.position, this.radius);
			return;
		}
		if (lightModes != BakedVolumeLight.LightModes.Spot)
		{
			return;
		}
		Vector3 vector = base.transform.position + base.transform.forward * this.radius;
		Gizmos.DrawLine(base.transform.position, vector);
		float d = this.coneSize * 0.034906585f;
		Vector3[] array = new Vector3[]
		{
			vector + base.transform.up * d * this.radius,
			vector + base.transform.right * d * this.radius,
			vector + -base.transform.up * d * this.radius,
			vector + -base.transform.right * d * this.radius
		};
		foreach (Vector3 to in array)
		{
			Gizmos.DrawLine(base.transform.position, to);
		}
		Gizmos.DrawLineStrip(array, true);
	}

	// Token: 0x06001B6E RID: 7022 RVA: 0x000754CB File Offset: 0x000736CB
	public void Rebake()
	{
		Object.FindAnyObjectByType<LightVolume>().Bake();
	}

	// Token: 0x04001247 RID: 4679
	public BakedVolumeLight.LightModes mode;

	// Token: 0x04001248 RID: 4680
	public Color color = Color.white;

	// Token: 0x04001249 RID: 4681
	public float intensity = 1f;

	// Token: 0x0400124A RID: 4682
	public float radius = 10f;

	// Token: 0x0400124B RID: 4683
	[Range(0f, 1f)]
	public float falloff = 0.5f;

	// Token: 0x0400124C RID: 4684
	[Range(0f, 1f)]
	[Tooltip("Percentage width at which the light should be full brightness. 1.0 means the entire cone is full bright, 0.0 means that the fade lerp starts immediately in the center")]
	public float coneFalloff = 0.9f;

	// Token: 0x0400124D RID: 4685
	[Range(0f, 90f)]
	public float coneSize = 30f;

	// Token: 0x02000342 RID: 834
	public enum LightModes
	{
		// Token: 0x0400124F RID: 4687
		Point,
		// Token: 0x04001250 RID: 4688
		Spot
	}
}
