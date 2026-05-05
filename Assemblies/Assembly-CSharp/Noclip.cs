using System;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

// Token: 0x020001EF RID: 495
public class Noclip : NetworkBehaviour
{
	// Token: 0x060011BC RID: 4540 RVA: 0x0004CA88 File Offset: 0x0004AC88
	private void Awake()
	{
		this._pc = base.GetComponent<PlayerController>();
		this._rb = base.GetComponent<Rigidbody>();
		this._head = this._pc.head.transform;
		foreach (Collider item in base.GetComponentsInChildren<Collider>())
		{
			this._colliders.Add(item);
		}
	}

	// Token: 0x060011BD RID: 4541 RVA: 0x0004CAE8 File Offset: 0x0004ACE8
	public override void OnStartClient()
	{
		base.OnStartClient();
		if (!base.isLocalPlayer)
		{
			base.enabled = false;
			return;
		}
	}

	// Token: 0x060011BE RID: 4542 RVA: 0x0004CB00 File Offset: 0x0004AD00
	private void OnEnable()
	{
		InputEvents.OnMoveEvent = (Action<Vector2>)Delegate.Combine(InputEvents.OnMoveEvent, new Action<Vector2>(this.OnMove));
	}

	// Token: 0x060011BF RID: 4543 RVA: 0x0004CB22 File Offset: 0x0004AD22
	private void OnDisable()
	{
		InputEvents.OnMoveEvent = (Action<Vector2>)Delegate.Remove(InputEvents.OnMoveEvent, new Action<Vector2>(this.OnMove));
	}

	// Token: 0x060011C0 RID: 4544 RVA: 0x0004CB44 File Offset: 0x0004AD44
	private void OnMove(Vector2 input)
	{
		this._horizontalInput = input;
	}

	// Token: 0x060011C1 RID: 4545 RVA: 0x0004CB4D File Offset: 0x0004AD4D
	private void Update()
	{
		this.Move();
	}

	// Token: 0x060011C2 RID: 4546 RVA: 0x0004CB58 File Offset: 0x0004AD58
	private void Move()
	{
		if (!this._isNoclipActive)
		{
			return;
		}
		int num = 0;
		if (InputEvents.IsJumpPressed)
		{
			num++;
		}
		if (InputEvents.IsCrouchPressed)
		{
			num--;
		}
		Vector3 normalized = Vector3.ProjectOnPlane(this._head.transform.forward, Vector3.up).normalized;
		Vector3 normalized2 = Vector3.ProjectOnPlane(this._head.transform.right, Vector3.up).normalized;
		Vector3 normalized3 = (normalized * this._horizontalInput.y + normalized2 * this._horizontalInput.x + Vector3.up * (float)num).normalized;
		float num2 = InputEvents.IsSprintPressed ? this.noclipSprintSpeed : this.noclipSpeed;
		Vector3 a = Vector3.SmoothDamp(Vector3.zero, normalized3, ref this._currentVelocity, this.smoothness);
		base.transform.position += a * (num2 * Time.unscaledDeltaTime);
	}

	// Token: 0x060011C3 RID: 4547 RVA: 0x0004CC61 File Offset: 0x0004AE61
	public void ToggleNoclip()
	{
		this.SetNoclipActive(!this._isNoclipActive);
	}

	// Token: 0x060011C4 RID: 4548 RVA: 0x0004CC72 File Offset: 0x0004AE72
	private void SetNoclipActive(bool active)
	{
		this._pc.enabled = !active;
		this._rb.isKinematic = active;
		this._rb.interpolation = ((!active && base.isLocalPlayer) ? RigidbodyInterpolation.Interpolate : RigidbodyInterpolation.None);
		this._isNoclipActive = active;
	}

	// Token: 0x060011C5 RID: 4549 RVA: 0x0004CCB0 File Offset: 0x0004AEB0
	private void SetCollidersActive(bool active)
	{
		foreach (Collider collider in this._colliders)
		{
			collider.enabled = active;
		}
	}

	// Token: 0x060011C7 RID: 4551 RVA: 0x00002321 File Offset: 0x00000521
	public override bool Weaved()
	{
		return true;
	}

	// Token: 0x04000B74 RID: 2932
	[Header("Noclip Settings")]
	[SerializeField]
	private float noclipSpeed = 600f;

	// Token: 0x04000B75 RID: 2933
	[SerializeField]
	private float noclipSprintSpeed = 1000f;

	// Token: 0x04000B76 RID: 2934
	[SerializeField]
	private float smoothness = 0.3f;

	// Token: 0x04000B77 RID: 2935
	private bool _isNoclipActive;

	// Token: 0x04000B78 RID: 2936
	private PlayerController _pc;

	// Token: 0x04000B79 RID: 2937
	private Rigidbody _rb;

	// Token: 0x04000B7A RID: 2938
	private Transform _head;

	// Token: 0x04000B7B RID: 2939
	private readonly List<Collider> _colliders = new List<Collider>();

	// Token: 0x04000B7C RID: 2940
	private Vector2 _horizontalInput;

	// Token: 0x04000B7D RID: 2941
	private Vector3 _currentVelocity;
}
