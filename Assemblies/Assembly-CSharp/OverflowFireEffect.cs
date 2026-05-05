using System;
using UnityEngine;
using UnityEngine.UI;

// Token: 0x02000249 RID: 585
[RequireComponent(typeof(RawImage))]
public class OverflowFireEffect : MonoBehaviour
{
	// Token: 0x06001507 RID: 5383 RVA: 0x0005A428 File Offset: 0x00058628
	private void Awake()
	{
		this._rawImage = base.GetComponent<RawImage>();
	}

	// Token: 0x06001508 RID: 5384 RVA: 0x0005A438 File Offset: 0x00058638
	private void OnEnable()
	{
		if (this._rawImage.material != null && this._instancedMat == null)
		{
			this._instancedMat = new Material(this._rawImage.material);
		}
		if (this._instancedMat != null)
		{
			this._rawImage.material = this._instancedMat;
			this._instancedMat.mainTextureScale = new Vector2(0.33333334f, 0.33333334f);
		}
	}

	// Token: 0x06001509 RID: 5385 RVA: 0x0005A4B5 File Offset: 0x000586B5
	private void OnDisable()
	{
		if (this._instancedMat != null)
		{
			this._rawImage.material = null;
		}
	}

	// Token: 0x0600150A RID: 5386 RVA: 0x0005A4D1 File Offset: 0x000586D1
	private void OnDestroy()
	{
		if (this._instancedMat != null)
		{
			Object.Destroy(this._instancedMat);
		}
	}

	// Token: 0x0600150B RID: 5387 RVA: 0x0005A4EC File Offset: 0x000586EC
	private void Update()
	{
		if (this._instancedMat == null)
		{
			return;
		}
		this._timer += Time.deltaTime;
		if (this._timer < this.frameDuration)
		{
			return;
		}
		this._timer = 0f;
		this._index = (this._index + 1) % 9;
		int num = this._index / 3;
		int num2 = this._index % 3;
		this._instancedMat.mainTextureOffset = new Vector2((float)num2 / 3f, 1f - (float)(num + 1) / 3f);
	}

	// Token: 0x04000D6E RID: 3438
	[SerializeField]
	private float frameDuration = 0.08f;

	// Token: 0x04000D6F RID: 3439
	private RawImage _rawImage;

	// Token: 0x04000D70 RID: 3440
	private Material _instancedMat;

	// Token: 0x04000D71 RID: 3441
	private float _timer;

	// Token: 0x04000D72 RID: 3442
	private int _index;
}
