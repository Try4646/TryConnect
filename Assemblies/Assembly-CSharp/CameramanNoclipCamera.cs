using System;
using Extensions;
using UnityEngine;

// Token: 0x020001EC RID: 492
public class CameramanNoclipCamera : MonoBehaviour
{
	// Token: 0x060011A2 RID: 4514 RVA: 0x0004C268 File Offset: 0x0004A468
	private void Awake()
	{
		this._camera = base.GetComponent<Camera>();
		if (this._camera == null)
		{
			this._camera = base.GetComponentInChildren<Camera>();
		}
		this._canvas = base.GetComponentInChildren<Canvas>();
		this._cs = Resources.Load<CameraSettings>("CameraSettings");
	}

	// Token: 0x060011A3 RID: 4515 RVA: 0x0004C2B8 File Offset: 0x0004A4B8
	private void Start()
	{
		if (this._camera != null)
		{
			Camera main = Camera.main;
			if (main != null && main != this._camera)
			{
				main.gameObject.SetActive(false);
			}
			this._camera.tag = "MainCamera";
		}
		Cursor.lockState = CursorLockMode.Locked;
		Cursor.visible = false;
	}

	// Token: 0x060011A4 RID: 4516 RVA: 0x0004C318 File Offset: 0x0004A518
	private void OnEnable()
	{
		InputEvents.OnMoveEvent = (Action<Vector2>)Delegate.Combine(InputEvents.OnMoveEvent, new Action<Vector2>(this.OnMove));
		InputEvents.OnAimEvent = (Action<Vector2>)Delegate.Combine(InputEvents.OnAimEvent, new Action<Vector2>(this.OnLook));
		InputEvents.OnScrollEvent = (Action<int>)Delegate.Combine(InputEvents.OnScrollEvent, new Action<int>(this.OnScroll));
	}

	// Token: 0x060011A5 RID: 4517 RVA: 0x0004C388 File Offset: 0x0004A588
	private void OnDisable()
	{
		InputEvents.OnMoveEvent = (Action<Vector2>)Delegate.Remove(InputEvents.OnMoveEvent, new Action<Vector2>(this.OnMove));
		InputEvents.OnAimEvent = (Action<Vector2>)Delegate.Remove(InputEvents.OnAimEvent, new Action<Vector2>(this.OnLook));
		InputEvents.OnScrollEvent = (Action<int>)Delegate.Remove(InputEvents.OnScrollEvent, new Action<int>(this.OnScroll));
	}

	// Token: 0x060011A6 RID: 4518 RVA: 0x0004C3F5 File Offset: 0x0004A5F5
	private void OnDestroy()
	{
		if (MonoSingleton<LocalManager>.Instance != null && MonoSingleton<LocalManager>.Instance.mainCamera != null)
		{
			MonoSingleton<LocalManager>.Instance.mainCamera.gameObject.SetActive(true);
		}
	}

	// Token: 0x060011A7 RID: 4519 RVA: 0x0004C42B File Offset: 0x0004A62B
	private void OnMove(Vector2 input)
	{
		this._horizontalInput = input;
	}

	// Token: 0x060011A8 RID: 4520 RVA: 0x0004C434 File Offset: 0x0004A634
	private void OnLook(Vector2 input)
	{
		this._lookInput = input;
	}

	// Token: 0x060011A9 RID: 4521 RVA: 0x0004C440 File Offset: 0x0004A640
	private void OnScroll(int scrollValue)
	{
		if (this._canvas != null && this._canvas.gameObject.activeSelf)
		{
			float value = this._canvas.planeDistance + (float)scrollValue * this.planeDistanceStep;
			this._canvas.planeDistance = Mathf.Clamp(value, this.minPlaneDistance, this.maxPlaneDistance);
		}
	}

	// Token: 0x060011AA RID: 4522 RVA: 0x0004C4A0 File Offset: 0x0004A6A0
	public void ToggleCanvas()
	{
		if (this._canvas != null)
		{
			this._canvas.gameObject.SetActive(!this._canvas.gameObject.activeSelf);
		}
	}

	// Token: 0x060011AB RID: 4523 RVA: 0x0004C4D3 File Offset: 0x0004A6D3
	public bool IsCanvasActive()
	{
		return this._canvas != null && this._canvas.gameObject.activeSelf;
	}

	// Token: 0x060011AC RID: 4524 RVA: 0x0004C4F5 File Offset: 0x0004A6F5
	private void Update()
	{
		if (this._camera == null)
		{
			return;
		}
		this.HandleRotation();
		this.HandleMovement();
	}

	// Token: 0x060011AD RID: 4525 RVA: 0x0004C514 File Offset: 0x0004A714
	private void HandleRotation()
	{
		float num = InputEvents.IsZoomPressed ? this._cs.zoomSensitivity : this._cs.sensitivity.value;
		this._yaw += this._lookInput.x * num * Time.deltaTime;
		this._pitch -= this._lookInput.y * num * Time.deltaTime;
		this._pitch = Mathf.Clamp(this._pitch, -89f, 89f);
		Quaternion b = Quaternion.Euler(this._pitch, this._yaw, 0f);
		float t = this._cs.cameraLerp * Time.deltaTime;
		base.transform.rotation = Quaternion.Slerp(base.transform.rotation, b, t);
	}

	// Token: 0x060011AE RID: 4526 RVA: 0x0004C5E8 File Offset: 0x0004A7E8
	private void HandleMovement()
	{
		int num = 0;
		if (InputEvents.IsJumpPressed)
		{
			num++;
		}
		if (InputEvents.IsCrouchPressed)
		{
			num--;
		}
		Vector3 normalized = Vector3.ProjectOnPlane(base.transform.forward, Vector3.up).normalized;
		Vector3 normalized2 = Vector3.ProjectOnPlane(base.transform.right, Vector3.up).normalized;
		Vector3 normalized3 = (normalized * this._horizontalInput.y + normalized2 * this._horizontalInput.x + Vector3.up * (float)num).normalized;
		float num2 = InputEvents.IsSprintPressed ? this.sprintSpeed : this.speed;
		Vector3 a = Vector3.SmoothDamp(Vector3.zero, normalized3, ref this._currentVelocity, this.smoothness);
		base.transform.position += a * (num2 * Time.unscaledDeltaTime);
	}

	// Token: 0x060011AF RID: 4527 RVA: 0x0004C6E0 File Offset: 0x0004A8E0
	public void InitializeRotation(Vector3 position, Quaternion rotation)
	{
		base.transform.position = position;
		base.transform.rotation = rotation;
		Vector3 eulerAngles = rotation.eulerAngles;
		this._yaw = eulerAngles.y;
		this._pitch = eulerAngles.x;
		if (this._pitch > 180f)
		{
			this._pitch -= 360f;
		}
	}

	// Token: 0x04000B58 RID: 2904
	[Header("Movement Settings")]
	[SerializeField]
	private float speed = 600f;

	// Token: 0x04000B59 RID: 2905
	[SerializeField]
	private float sprintSpeed = 1000f;

	// Token: 0x04000B5A RID: 2906
	[SerializeField]
	private float smoothness = 0.3f;

	// Token: 0x04000B5B RID: 2907
	private Camera _camera;

	// Token: 0x04000B5C RID: 2908
	private CameraSettings _cs;

	// Token: 0x04000B5D RID: 2909
	private Canvas _canvas;

	// Token: 0x04000B5E RID: 2910
	private Vector2 _horizontalInput;

	// Token: 0x04000B5F RID: 2911
	private Vector2 _lookInput;

	// Token: 0x04000B60 RID: 2912
	private Vector3 _currentVelocity;

	// Token: 0x04000B61 RID: 2913
	private float _yaw;

	// Token: 0x04000B62 RID: 2914
	private float _pitch;

	// Token: 0x04000B63 RID: 2915
	[Header("Canvas Settings")]
	[SerializeField]
	private float planeDistanceStep = 10f;

	// Token: 0x04000B64 RID: 2916
	[SerializeField]
	private float minPlaneDistance = 10f;

	// Token: 0x04000B65 RID: 2917
	[SerializeField]
	private float maxPlaneDistance = 1000f;
}
