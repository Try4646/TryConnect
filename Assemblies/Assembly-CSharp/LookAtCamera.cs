using System;
using Gilzoide.UpdateManager;
using TMPro;
using UnityEngine;

// Token: 0x02000236 RID: 566
public class LookAtCamera : MonoBehaviour, ILateUpdatable, IManagedObject
{
	// Token: 0x0600147F RID: 5247 RVA: 0x00058132 File Offset: 0x00056332
	private void OnEnable()
	{
		this.CacheReferencesIfNeeded();
		this.RegisterInManager();
	}

	// Token: 0x06001480 RID: 5248 RVA: 0x00052C15 File Offset: 0x00050E15
	private void OnDisable()
	{
		this.UnregisterInManager();
	}

	// Token: 0x06001481 RID: 5249 RVA: 0x00058140 File Offset: 0x00056340
	public void ManagedLateUpdate()
	{
		if (this.lookAtOnUpdate)
		{
			this.LookAtCameraTransform();
		}
		if (this.fadeTextByDistance)
		{
			this.UpdateTextFade();
		}
	}

	// Token: 0x06001482 RID: 5250 RVA: 0x00058160 File Offset: 0x00056360
	private void LookAtCameraTransform()
	{
		Camera mainCamera = this.GetMainCamera();
		if (mainCamera == null)
		{
			return;
		}
		Vector3 vector = mainCamera.transform.position - base.transform.position + this.lookOffset;
		if (this.lockXAxis)
		{
			vector.x = 0f;
		}
		if (this.lockYAxis)
		{
			vector.y = 0f;
		}
		if (this.lockZAxis)
		{
			vector.z = 0f;
		}
		if (vector != Vector3.zero)
		{
			Quaternion quaternion = Quaternion.LookRotation(vector);
			if (this.flipY)
			{
				quaternion *= Quaternion.Euler(0f, 180f, 0f);
			}
			if (this.flipX)
			{
				quaternion *= Quaternion.Euler(180f, 0f, 0f);
			}
			if (this.flipZ)
			{
				quaternion *= Quaternion.Euler(0f, 0f, 180f);
			}
			if (this.useRotationClamps)
			{
				Vector3 eulerAngles = quaternion.eulerAngles;
				eulerAngles.x = this.ClampAngle(eulerAngles.x, this.minRotationX, this.maxRotationX);
				eulerAngles.y = this.ClampAngle(eulerAngles.y, this.minRotationY, this.maxRotationY);
				eulerAngles.z = this.ClampAngle(eulerAngles.z, this.minRotationZ, this.maxRotationZ);
				quaternion = Quaternion.Euler(eulerAngles);
			}
			if (this.useSmoothRotation)
			{
				base.transform.rotation = Quaternion.Slerp(base.transform.rotation, quaternion, this.rotationSpeed * Time.deltaTime);
				return;
			}
			base.transform.rotation = quaternion;
		}
	}

	// Token: 0x06001483 RID: 5251 RVA: 0x00058314 File Offset: 0x00056514
	private void UpdateTextFade()
	{
		this.CacheReferencesIfNeeded();
		if (this.textToFade == null)
		{
			return;
		}
		Camera mainCamera = this.GetMainCamera();
		if (mainCamera == null)
		{
			return;
		}
		float num = Vector3.Distance(mainCamera.transform.position, base.transform.position);
		float a;
		if (this.fadeEndDistance <= this.fadeStartDistance)
		{
			a = ((num <= this.fadeStartDistance) ? this.nearAlpha : this.farAlpha);
		}
		else
		{
			float t = Mathf.InverseLerp(this.fadeStartDistance, this.fadeEndDistance, num);
			a = Mathf.Lerp(this.nearAlpha, this.farAlpha, t);
		}
		Color color = this.textToFade.color;
		color.a = a;
		this.textToFade.color = color;
	}

	// Token: 0x06001484 RID: 5252 RVA: 0x000583D4 File Offset: 0x000565D4
	private void CacheReferencesIfNeeded()
	{
		if (this._cachedMainCamera == null)
		{
			this._cachedMainCamera = Camera.main;
		}
		if (this.textToFade == null)
		{
			this.textToFade = base.GetComponent<TMP_Text>();
			if (this.textToFade == null)
			{
				this.textToFade = base.GetComponentInChildren<TMP_Text>();
			}
		}
	}

	// Token: 0x06001485 RID: 5253 RVA: 0x0005842E File Offset: 0x0005662E
	private Camera GetMainCamera()
	{
		if (this._cachedMainCamera == null)
		{
			this._cachedMainCamera = Camera.main;
		}
		return this._cachedMainCamera;
	}

	// Token: 0x06001486 RID: 5254 RVA: 0x0005844F File Offset: 0x0005664F
	private float ClampAngle(float angle, float min, float max)
	{
		if (angle > 180f)
		{
			angle -= 360f;
		}
		return Mathf.Clamp(angle, min, max);
	}

	// Token: 0x04000CF4 RID: 3316
	[Header("Look At Settings")]
	[Tooltip("Whether to look at the camera on every frame")]
	public bool lookAtOnUpdate = true;

	// Token: 0x04000CF5 RID: 3317
	[Tooltip("Whether to use smooth rotation")]
	public bool useSmoothRotation;

	// Token: 0x04000CF6 RID: 3318
	[Tooltip("Rotation speed when using smooth rotation")]
	[Range(1f, 20f)]
	public float rotationSpeed = 5f;

	// Token: 0x04000CF7 RID: 3319
	[Header("Axis Locking")]
	[Tooltip("Lock rotation on specific axes")]
	public bool lockXAxis;

	// Token: 0x04000CF8 RID: 3320
	public bool lockYAxis;

	// Token: 0x04000CF9 RID: 3321
	public bool lockZAxis;

	// Token: 0x04000CFA RID: 3322
	[Header("Offset")]
	[Tooltip("Optional offset to add to the look direction")]
	public Vector3 lookOffset = Vector3.zero;

	// Token: 0x04000CFB RID: 3323
	[Header("Flip Options")]
	[Tooltip("Flip the object 180 degrees around Y-axis")]
	public bool flipY;

	// Token: 0x04000CFC RID: 3324
	[Tooltip("Flip the object 180 degrees around X-axis")]
	public bool flipX;

	// Token: 0x04000CFD RID: 3325
	[Tooltip("Flip the object 180 degrees around Z-axis")]
	public bool flipZ;

	// Token: 0x04000CFE RID: 3326
	[Header("Rotation Clamps")]
	[Tooltip("Enable rotation clamping to limit rotation angles")]
	public bool useRotationClamps;

	// Token: 0x04000CFF RID: 3327
	[Tooltip("Minimum rotation angle for X-axis (degrees)")]
	public float minRotationX = -180f;

	// Token: 0x04000D00 RID: 3328
	[Tooltip("Maximum rotation angle for X-axis (degrees)")]
	public float maxRotationX = 180f;

	// Token: 0x04000D01 RID: 3329
	[Tooltip("Minimum rotation angle for Y-axis (degrees)")]
	public float minRotationY = -180f;

	// Token: 0x04000D02 RID: 3330
	[Tooltip("Maximum rotation angle for Y-axis (degrees)")]
	public float maxRotationY = 180f;

	// Token: 0x04000D03 RID: 3331
	[Tooltip("Minimum rotation angle for Z-axis (degrees)")]
	public float minRotationZ = -180f;

	// Token: 0x04000D04 RID: 3332
	[Tooltip("Maximum rotation angle for Z-axis (degrees)")]
	public float maxRotationZ = 180f;

	// Token: 0x04000D05 RID: 3333
	[Header("Fade (TextMeshPro)")]
	[Tooltip("If enabled, fades a TextMeshPro component based on distance to the main camera")]
	public bool fadeTextByDistance;

	// Token: 0x04000D06 RID: 3334
	[Tooltip("TextMeshPro component to fade. If left null, will try to find one on this object (or its children).")]
	public TMP_Text textToFade;

	// Token: 0x04000D07 RID: 3335
	[Tooltip("Distance at which fading starts (near).")]
	[Min(0f)]
	public float fadeStartDistance = 2f;

	// Token: 0x04000D08 RID: 3336
	[Tooltip("Distance at which fading ends (far).")]
	[Min(0f)]
	public float fadeEndDistance = 10f;

	// Token: 0x04000D09 RID: 3337
	[Tooltip("Alpha at (or closer than) Fade Start Distance.")]
	[Range(0f, 1f)]
	public float nearAlpha = 1f;

	// Token: 0x04000D0A RID: 3338
	[Tooltip("Alpha at (or farther than) Fade End Distance.")]
	[Range(0f, 1f)]
	public float farAlpha;

	// Token: 0x04000D0B RID: 3339
	private Camera _cachedMainCamera;
}
