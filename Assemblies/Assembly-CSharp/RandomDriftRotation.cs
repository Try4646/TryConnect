using System;
using UnityEngine;

// Token: 0x020002F1 RID: 753
public class RandomDriftRotation : MonoBehaviour
{
	// Token: 0x06001A22 RID: 6690 RVA: 0x0006E158 File Offset: 0x0006C358
	private void Start()
	{
		this.noiseOffset = new Vector3(Random.Range(0f, 1000f), Random.Range(0f, 1000f), Random.Range(0f, 1000f));
	}

	// Token: 0x06001A23 RID: 6691 RVA: 0x0006E194 File Offset: 0x0006C394
	private void Update()
	{
		float num = Time.time * this.velocityChangeSpeed;
		this.angularVelocity.x = (Mathf.PerlinNoise(num + this.noiseOffset.x, 0f) - 0.5f) * 2f * this.maxAngularVelocity;
		this.angularVelocity.y = (Mathf.PerlinNoise(num + this.noiseOffset.y, 0f) - 0.5f) * 2f * this.maxAngularVelocity;
		this.angularVelocity.z = (Mathf.PerlinNoise(num + this.noiseOffset.z, 0f) - 0.5f) * 2f * this.maxAngularVelocity;
		base.transform.Rotate(this.angularVelocity * Time.deltaTime, Space.Self);
	}

	// Token: 0x040010B5 RID: 4277
	public float velocityChangeSpeed = 1f;

	// Token: 0x040010B6 RID: 4278
	public float maxAngularVelocity = 90f;

	// Token: 0x040010B7 RID: 4279
	private Vector3 angularVelocity;

	// Token: 0x040010B8 RID: 4280
	private Vector3 noiseOffset;
}
