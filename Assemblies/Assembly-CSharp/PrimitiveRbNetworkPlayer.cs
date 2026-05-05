using System;
using Mirror;
using UnityEngine;

// Token: 0x020002D5 RID: 725
public class PrimitiveRbNetworkPlayer : NetworkBehaviour
{
	// Token: 0x0600197E RID: 6526 RVA: 0x0006AF39 File Offset: 0x00069139
	private void Awake()
	{
		this._rb = base.GetComponent<Rigidbody>();
	}

	// Token: 0x0600197F RID: 6527 RVA: 0x0006AF47 File Offset: 0x00069147
	private void Start()
	{
		if (!base.isLocalPlayer)
		{
			return;
		}
		Cursor.lockState = CursorLockMode.Locked;
	}

	// Token: 0x06001980 RID: 6528 RVA: 0x0006AF58 File Offset: 0x00069158
	private void Update()
	{
		if (!base.isLocalPlayer)
		{
			return;
		}
		this.HandleMovement();
		this.HandleRotation();
	}

	// Token: 0x06001981 RID: 6529 RVA: 0x0006AF70 File Offset: 0x00069170
	private void HandleMovement()
	{
		float axis = Input.GetAxis("Horizontal");
		float axis2 = Input.GetAxis("Vertical");
		Vector3 vector = new Vector3(axis, 0f, axis2);
		vector = base.transform.TransformDirection(vector);
		this._rb.AddForce(vector * 5f);
	}

	// Token: 0x06001982 RID: 6530 RVA: 0x0006AFC4 File Offset: 0x000691C4
	private void HandleRotation()
	{
		float axis = Input.GetAxis("Mouse X");
		float axis2 = Input.GetAxis("Mouse Y");
		Vector3 localEulerAngles = base.transform.localEulerAngles;
		localEulerAngles.y += axis * 2f;
		localEulerAngles.x -= axis2 * 2f;
		localEulerAngles.z = 0f;
		base.transform.localEulerAngles = localEulerAngles;
	}

	// Token: 0x06001984 RID: 6532 RVA: 0x00002321 File Offset: 0x00000521
	public override bool Weaved()
	{
		return true;
	}

	// Token: 0x04001061 RID: 4193
	private Rigidbody _rb;

	// Token: 0x04001062 RID: 4194
	[SerializeField]
	private int blueChips;

	// Token: 0x04001063 RID: 4195
	[SerializeField]
	private int redChips;

	// Token: 0x04001064 RID: 4196
	[SerializeField]
	private int greenChips;
}
