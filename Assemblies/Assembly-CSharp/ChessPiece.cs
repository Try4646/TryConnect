using System;
using System.Collections;
using UnityEngine;

// Token: 0x020000DD RID: 221
public class ChessPiece : Item
{
	// Token: 0x060008C4 RID: 2244 RVA: 0x000233DD File Offset: 0x000215DD
	private void Start()
	{
		this._excludeLayers = LayerMask.GetMask(new string[]
		{
			"Player",
			"SelfMeshPlayer"
		});
	}

	// Token: 0x060008C5 RID: 2245 RVA: 0x00023408 File Offset: 0x00021608
	private void FixedUpdate()
	{
		if (this.Rb.isKinematic)
		{
			return;
		}
		Vector3 up = base.transform.up;
		Vector3 up2 = Vector3.up;
		Vector3 vector = Vector3.Cross(up, up2);
		if (vector.sqrMagnitude < 0.001f)
		{
			return;
		}
		vector.Normalize();
		float num = Vector3.Angle(up, up2) * 0.017453292f;
		this.Rb.AddTorque(vector * (num * this.strength));
		float num2 = Vector3.Dot(this.Rb.angularVelocity, vector);
		this.Rb.angularVelocity -= vector * (num2 * this.angularDamping * Time.fixedDeltaTime);
	}

	// Token: 0x060008C6 RID: 2246 RVA: 0x000234B8 File Offset: 0x000216B8
	protected override void OnPickedUp(PlayerInventory playerInventory)
	{
		base.OnPickedUp(playerInventory);
		if (this._colRoutine != null)
		{
			base.StopCoroutine(this._colRoutine);
		}
		this.Rb.excludeLayers = this._excludeLayers;
	}

	// Token: 0x060008C7 RID: 2247 RVA: 0x000234E6 File Offset: 0x000216E6
	protected override void OnDropped(PlayerInventory playerInventory)
	{
		base.OnDropped(playerInventory);
		this._colRoutine = base.StartCoroutine(this.DelayedEnableColliders());
	}

	// Token: 0x060008C8 RID: 2248 RVA: 0x00023501 File Offset: 0x00021701
	private IEnumerator DelayedEnableColliders()
	{
		yield return new WaitForSeconds(0.5f);
		this.Rb.excludeLayers = 0;
		yield break;
	}

	// Token: 0x060008CA RID: 2250 RVA: 0x00002321 File Offset: 0x00000521
	public override bool Weaved()
	{
		return true;
	}

	// Token: 0x0400058A RID: 1418
	[Header("Settings")]
	[SerializeField]
	private float strength = 15f;

	// Token: 0x0400058B RID: 1419
	[SerializeField]
	private float angularDamping = 5f;

	// Token: 0x0400058C RID: 1420
	private Coroutine _colRoutine;

	// Token: 0x0400058D RID: 1421
	private LayerMask _excludeLayers;
}
