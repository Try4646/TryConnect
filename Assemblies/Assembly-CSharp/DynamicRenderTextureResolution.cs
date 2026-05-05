using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

// Token: 0x02000299 RID: 665
public class DynamicRenderTextureResolution : MonoBehaviour
{
	// Token: 0x060017A7 RID: 6055 RVA: 0x00064131 File Offset: 0x00062331
	private void OnEnable()
	{
		SettingsLayout.SettingsChanged += this.OnSettingsChanged;
		base.StartCoroutine(this.ApplyNextFrame());
	}

	// Token: 0x060017A8 RID: 6056 RVA: 0x00064154 File Offset: 0x00062354
	private void OnDisable()
	{
		SettingsLayout.SettingsChanged -= this.OnSettingsChanged;
		if (this.sourceCamera != null && this.sourceCamera.targetTexture == this.renderTextureAsset)
		{
			this.sourceCamera.targetTexture = null;
		}
		if (this.refreshRoutine != null)
		{
			base.StopCoroutine(this.refreshRoutine);
			this.refreshRoutine = null;
		}
	}

	// Token: 0x060017A9 RID: 6057 RVA: 0x000641BF File Offset: 0x000623BF
	private void OnValidate()
	{
		if (Application.isPlaying)
		{
			this.CheckAndApply();
		}
	}

	// Token: 0x060017AA RID: 6058 RVA: 0x000641D0 File Offset: 0x000623D0
	private void OnSettingsChanged(SettingsLayout source, SettingItemBase entry)
	{
		if (entry == null || string.IsNullOrWhiteSpace(entry.key))
		{
			return;
		}
		string a = entry.key.Trim().ToLowerInvariant();
		if (a == "resolution" || a == "display")
		{
			base.StartCoroutine(this.ApplyNextFrame());
		}
	}

	// Token: 0x060017AB RID: 6059 RVA: 0x0006422C File Offset: 0x0006242C
	private IEnumerator ApplyNextFrame()
	{
		yield return null;
		this.CheckAndApply();
		yield break;
	}

	// Token: 0x060017AC RID: 6060 RVA: 0x0006423C File Offset: 0x0006243C
	private void CheckAndApply()
	{
		if (this.sourceCamera == null || this.renderTextureAsset == null)
		{
			return;
		}
		int num = Mathf.Max(1, Mathf.RoundToInt((float)Screen.width * this.scale));
		int num2 = Mathf.Max(1, Mathf.RoundToInt((float)Screen.height * this.scale));
		if (this.useEvenDimensions)
		{
			num &= -2;
			num2 &= -2;
			num = Mathf.Max(2, num);
			num2 = Mathf.Max(2, num2);
		}
		if (num != this.lastW || num2 != this.lastH || !Mathf.Approximately(this.scale, this.lastScale))
		{
			this.Apply(num, num2);
		}
	}

	// Token: 0x060017AD RID: 6061 RVA: 0x000642E8 File Offset: 0x000624E8
	private void Apply(int targetW, int targetH)
	{
		if (this.sourceCamera == null || this.renderTextureAsset == null)
		{
			return;
		}
		this.lastW = targetW;
		this.lastH = targetH;
		this.lastScale = this.scale;
		DynamicRenderTextureResolution.RebuildRenderTexture(this.renderTextureAsset, targetW, targetH);
		Debug.Log(string.Format("[DynamicRenderTextureResolution] Render texture resolution changed to {0}x{1}", targetW, targetH));
		if (this.previewImage != null && this.previewImage.texture != this.renderTextureAsset)
		{
			this.previewImage.texture = this.renderTextureAsset;
		}
		if (this.refreshRoutine != null)
		{
			base.StopCoroutine(this.refreshRoutine);
		}
		this.refreshRoutine = base.StartCoroutine(this.RefreshCameraNextFrame(targetW, targetH));
	}

	// Token: 0x060017AE RID: 6062 RVA: 0x000643B3 File Offset: 0x000625B3
	private IEnumerator RefreshCameraNextFrame(int w, int h)
	{
		RenderTexture rt = this.renderTextureAsset;
		if (rt == null || !rt.IsCreated())
		{
			yield break;
		}
		if (this.sourceCamera.targetTexture == rt)
		{
			this.sourceCamera.targetTexture = null;
		}
		yield return null;
		if (rt == null || !rt.IsCreated())
		{
			Debug.LogError(string.Format("[DynamicRenderTextureResolution] RenderTexture is invalid after creation. Resolution: {0}x{1}", w, h));
			yield break;
		}
		float aspect = (float)w / (float)h;
		this.sourceCamera.aspect = aspect;
		this.sourceCamera.targetTexture = rt;
		if (this.sourceCamera.orthographic)
		{
			float orthographicSize = this.sourceCamera.orthographicSize;
			this.sourceCamera.orthographicSize = orthographicSize + 0.0001f;
			this.sourceCamera.orthographicSize = orthographicSize;
		}
		else
		{
			float fieldOfView = this.sourceCamera.fieldOfView;
			this.sourceCamera.fieldOfView = fieldOfView + 0.0001f;
			this.sourceCamera.fieldOfView = fieldOfView;
		}
		this.sourceCamera.ResetProjectionMatrix();
		this.sourceCamera.Render();
		yield break;
	}

	// Token: 0x060017AF RID: 6063 RVA: 0x000643D0 File Offset: 0x000625D0
	private static void RebuildRenderTexture(RenderTexture rt, int w, int h)
	{
		if (rt == null)
		{
			return;
		}
		try
		{
			if (rt.IsCreated())
			{
				rt.Release();
			}
			rt.width = w;
			rt.height = h;
			rt.Create();
			if (!rt.IsCreated())
			{
				Debug.LogError(string.Format("[DynamicRenderTextureResolution] Failed to create RenderTexture {0}x{1}. GPU may be out of memory. Error: 0x8007000e", w, h));
				int num = Mathf.Max(256, w / 2);
				int num2 = Mathf.Max(256, h / 2);
				rt.width = num;
				rt.height = num2;
				rt.Create();
				if (!rt.IsCreated())
				{
					Debug.LogError(string.Format("[DynamicRenderTextureResolution] Failed to create fallback RenderTexture {0}x{1}. Disabling component.", num, num2));
				}
				else
				{
					Debug.LogWarning(string.Format("[DynamicRenderTextureResolution] Using fallback resolution: {0}x{1} instead of {2}x{3}", new object[]
					{
						num,
						num2,
						w,
						h
					}));
				}
			}
		}
		catch (Exception ex)
		{
			Debug.LogError(string.Format("[DynamicRenderTextureResolution] Exception creating RenderTexture {0}x{1}: {2}", w, h, ex.Message));
		}
	}

	// Token: 0x04000F48 RID: 3912
	[SerializeField]
	private Camera sourceCamera;

	// Token: 0x04000F49 RID: 3913
	[SerializeField]
	private RenderTexture renderTextureAsset;

	// Token: 0x04000F4A RID: 3914
	[SerializeField]
	private RawImage previewImage;

	// Token: 0x04000F4B RID: 3915
	[Range(0.25f, 2f)]
	[SerializeField]
	private float scale = 1f;

	// Token: 0x04000F4C RID: 3916
	[SerializeField]
	private bool useEvenDimensions = true;

	// Token: 0x04000F4D RID: 3917
	private int lastW;

	// Token: 0x04000F4E RID: 3918
	private int lastH;

	// Token: 0x04000F4F RID: 3919
	private float lastScale;

	// Token: 0x04000F50 RID: 3920
	private Coroutine refreshRoutine;
}
