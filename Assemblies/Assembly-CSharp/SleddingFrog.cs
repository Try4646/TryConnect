using System;
using System.Collections;
using UnityEngine;

// Token: 0x020000F7 RID: 247
public class SleddingFrog : Plush
{
	// Token: 0x06000A01 RID: 2561 RVA: 0x000280F7 File Offset: 0x000262F7
	public override void OnStartServer()
	{
		base.OnStartServer();
		this.Rb.isKinematic = true;
	}

	// Token: 0x06000A02 RID: 2562 RVA: 0x0002810B File Offset: 0x0002630B
	private void OnTriggerEnter(Collider other)
	{
		if (!base.isServer)
		{
			return;
		}
		if (!this.Rb.isKinematic)
		{
			return;
		}
		if (base.NetworkHolder)
		{
			return;
		}
		this.Rb.isKinematic = false;
	}

	// Token: 0x06000A03 RID: 2563 RVA: 0x0002810B File Offset: 0x0002630B
	private void OnCollisionEnter(Collision other)
	{
		if (!base.isServer)
		{
			return;
		}
		if (!this.Rb.isKinematic)
		{
			return;
		}
		if (base.NetworkHolder)
		{
			return;
		}
		this.Rb.isKinematic = false;
	}

	// Token: 0x06000A04 RID: 2564 RVA: 0x0002813E File Offset: 0x0002633E
	protected override void OnDropped(PlayerInventory playerInventory)
	{
		base.OnDropped(playerInventory);
		base.StartCoroutine(this.FixedUpdateDelay());
	}

	// Token: 0x06000A05 RID: 2565 RVA: 0x00028154 File Offset: 0x00026354
	private IEnumerator FixedUpdateDelay()
	{
		yield return new WaitForFixedUpdate();
		Vector3 normalized = (Vector3.down + Random.insideUnitSphere * Mathf.Tan(0.2617994f)).normalized;
		this.Rb.Rotate(Quaternion.LookRotation(normalized), false);
		this.Rb.angularVelocity = Vector3.Project(this.Rb.angularVelocity, base.transform.forward);
		yield break;
	}

	// Token: 0x06000A06 RID: 2566 RVA: 0x00028164 File Offset: 0x00026364
	private void FixedUpdate()
	{
		if (!base.isServer)
		{
			return;
		}
		if (base.NetworkHolder)
		{
			return;
		}
		if (this.Rb.isKinematic)
		{
			return;
		}
		Vector3 vector = -base.transform.forward;
		Vector3 up = Vector3.up;
		Vector3 vector2 = Vector3.Cross(vector, up);
		if (vector2.sqrMagnitude < 0.001f)
		{
			return;
		}
		vector2.Normalize();
		float num = Vector3.Angle(vector, up) * 0.017453292f;
		this.Rb.AddTorque(vector2 * (num * this.strength));
		float num2 = Vector3.Dot(this.Rb.angularVelocity, vector2);
		this.Rb.angularVelocity -= vector2 * (num2 * this.angularDamping * Time.fixedDeltaTime);
	}

	// Token: 0x06000A08 RID: 2568 RVA: 0x00002321 File Offset: 0x00000521
	public override bool Weaved()
	{
		return true;
	}

	// Token: 0x0400065A RID: 1626
	[SerializeField]
	private float strength = 5f;

	// Token: 0x0400065B RID: 1627
	[SerializeField]
	private float angularDamping = 2f;
}
