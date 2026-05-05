using System;
using UnityEngine;

// Token: 0x020002D2 RID: 722
[RequireComponent(typeof(Rigidbody))]
public class CustomGravity : MonoBehaviour
{
	// Token: 0x0600196F RID: 6511 RVA: 0x0006ACC1 File Offset: 0x00068EC1
	private void Awake()
	{
		if (!this.rb)
		{
			this.rb = base.GetComponent<Rigidbody>();
		}
		this.rb.useGravity = false;
	}

	// Token: 0x06001970 RID: 6512 RVA: 0x0006ACE8 File Offset: 0x00068EE8
	private void FixedUpdate()
	{
		if (this.gravityCenter)
		{
			Vector3 normalized = (this.gravityCenter.position - base.transform.position).normalized;
			this.rb.AddForce(normalized * this.gravityStrength, ForceMode.Acceleration);
			return;
		}
		this.rb.AddForce(this.gravityDirection.normalized * this.gravityStrength, ForceMode.Acceleration);
	}

	// Token: 0x04001059 RID: 4185
	[Header("References")]
	[SerializeField]
	private Rigidbody rb;

	// Token: 0x0400105A RID: 4186
	[Header("Optional References")]
	[SerializeField]
	private Transform gravityCenter;

	// Token: 0x0400105B RID: 4187
	[Header("Settings")]
	[SerializeField]
	private Vector3 gravityDirection = Vector3.down;

	// Token: 0x0400105C RID: 4188
	[SerializeField]
	private float gravityStrength = 9.81f;
}
