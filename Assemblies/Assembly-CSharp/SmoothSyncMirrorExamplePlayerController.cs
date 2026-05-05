using System;
using Mirror;
using Smooth;
using UnityEngine;

// Token: 0x02000350 RID: 848
public class SmoothSyncMirrorExamplePlayerController : NetworkBehaviour
{
	// Token: 0x06001BDB RID: 7131 RVA: 0x00077AAC File Offset: 0x00075CAC
	private void Start()
	{
		this.rb = base.GetComponent<Rigidbody>();
		this.rb2D = base.GetComponent<Rigidbody2D>();
		this.smoothSync = base.GetComponent<SmoothSyncMirror>();
		if (this.smoothSync)
		{
			this.smoothSync.validateStateMethod = new SmoothSyncMirror.validateStateDelegate(SmoothSyncMirrorExamplePlayerController.validateStateOfPlayer);
		}
	}

	// Token: 0x06001BDC RID: 7132 RVA: 0x00077B04 File Offset: 0x00075D04
	private void Update()
	{
		if (Input.GetKeyDown(KeyCode.T))
		{
			if (base.isOwned)
			{
				base.transform.position = base.transform.position + Vector3.right * 18f;
				this.smoothSync.teleportOwnedObjectFromOwner();
			}
			else if (NetworkServer.active)
			{
				this.smoothSync.teleportAnyObjectFromServer(base.transform.position + Vector3.right * 18f, base.transform.rotation, base.transform.localScale);
			}
		}
		if (!base.isOwned && (!NetworkServer.active || base.netIdentity.connectionToClient != null))
		{
			return;
		}
		if (Input.GetKeyDown(KeyCode.F))
		{
			this.smoothSync.forceStateSendNextFixedUpdate();
		}
		Input.GetKeyDown(KeyCode.C);
		float d = this.transformMovementSpeed * Time.deltaTime;
		if (Input.GetKey(KeyCode.LeftShift) && Input.GetKey(KeyCode.Equals))
		{
			base.transform.localScale = base.transform.localScale + new Vector3(1f, 1f, 1f) * d * 0.2f;
		}
		if (Input.GetKey(KeyCode.LeftShift) && Input.GetKey(KeyCode.Minus))
		{
			base.transform.localScale = base.transform.localScale - new Vector3(1f, 1f, 1f) * d * 0.2f;
		}
		if (this.childObjectToControl)
		{
			if (Input.GetKey(KeyCode.RightShift) && Input.GetKey(KeyCode.Equals))
			{
				this.childObjectToControl.transform.localScale = this.childObjectToControl.transform.localScale + new Vector3(1f, 1f, 1f) * d * 0.2f;
			}
			if (Input.GetKey(KeyCode.RightShift) && Input.GetKey(KeyCode.Minus))
			{
				this.childObjectToControl.transform.localScale = this.childObjectToControl.transform.localScale - new Vector3(1f, 1f, 1f) * d * 0.2f;
			}
		}
		if (this.childObjectToControl)
		{
			if (Input.GetKey(KeyCode.S))
			{
				this.childObjectToControl.transform.position = this.childObjectToControl.transform.position + new Vector3(0f, -1.5f, -1f) * d;
			}
			if (Input.GetKey(KeyCode.W))
			{
				this.childObjectToControl.transform.position = this.childObjectToControl.transform.position + new Vector3(0f, 1.5f, 1f) * d;
			}
			if (Input.GetKey(KeyCode.A))
			{
				this.childObjectToControl.transform.position = this.childObjectToControl.transform.position + new Vector3(-1f, 0f, 0f) * d;
			}
			if (Input.GetKey(KeyCode.D))
			{
				this.childObjectToControl.transform.position = this.childObjectToControl.transform.position + new Vector3(1f, 0f, 0f) * d;
			}
		}
		if (this.rb)
		{
			if (Input.GetKeyDown(KeyCode.Alpha0))
			{
				this.rb.linearVelocity = Vector3.zero;
				this.rb.angularVelocity = Vector3.zero;
			}
			if (Input.GetKeyDown(KeyCode.DownArrow))
			{
				this.rb.AddForce(new Vector3(0f, -1.5f, -1f) * this.rigidbodyMovementForce);
			}
			if (Input.GetKeyDown(KeyCode.UpArrow))
			{
				this.rb.AddForce(new Vector3(0f, 1.5f, 1f) * this.rigidbodyMovementForce);
			}
			if (Input.GetKeyDown(KeyCode.LeftArrow))
			{
				this.rb.AddForce(new Vector3(-1f, 0f, 0f) * this.rigidbodyMovementForce);
			}
			if (Input.GetKeyDown(KeyCode.RightArrow))
			{
				this.rb.AddForce(new Vector3(1f, 0f, 0f) * this.rigidbodyMovementForce);
				return;
			}
		}
		else if (this.rb2D)
		{
			if (Input.GetKeyDown(KeyCode.Alpha0))
			{
				this.rb2D.linearVelocity = Vector3.zero;
				this.rb2D.angularVelocity = 0f;
			}
			if (Input.GetKeyDown(KeyCode.DownArrow))
			{
				this.rb2D.AddForce(new Vector3(0f, -1.5f, -1f) * this.rigidbodyMovementForce);
			}
			if (Input.GetKeyDown(KeyCode.UpArrow))
			{
				this.rb2D.AddForce(new Vector3(0f, 1.5f, 1f) * this.rigidbodyMovementForce);
			}
			if (Input.GetKeyDown(KeyCode.LeftArrow))
			{
				this.rb2D.AddForce(new Vector3(-1f, 0f, 0f) * this.rigidbodyMovementForce);
			}
			if (Input.GetKeyDown(KeyCode.RightArrow))
			{
				this.rb2D.AddForce(new Vector3(1f, 0f, 0f) * this.rigidbodyMovementForce);
				return;
			}
		}
		else
		{
			if (Input.GetKey(KeyCode.DownArrow))
			{
				base.transform.position = base.transform.position + new Vector3(0f, 0f, -1f) * d;
			}
			if (Input.GetKey(KeyCode.UpArrow))
			{
				base.transform.position = base.transform.position + new Vector3(0f, 0f, 1f) * d;
			}
			if (Input.GetKey(KeyCode.LeftArrow))
			{
				base.transform.position = base.transform.position + new Vector3(-1f, 0f, 0f) * d;
			}
			if (Input.GetKey(KeyCode.RightArrow))
			{
				base.transform.position = base.transform.position + new Vector3(1f, 0f, 0f) * d;
			}
		}
	}

	// Token: 0x06001BDD RID: 7133 RVA: 0x000781D4 File Offset: 0x000763D4
	public static bool validateStateOfPlayer(StateMirror latestReceivedState, StateMirror latestValidatedState)
	{
		return Vector3.Distance(latestReceivedState.position, latestValidatedState.position) <= 9000f || latestReceivedState.ownerTimestamp - latestValidatedState.receivedOnServerTimestamp >= 0.5f;
	}

	// Token: 0x06001BDF RID: 7135 RVA: 0x00002321 File Offset: 0x00000521
	public override bool Weaved()
	{
		return true;
	}

	// Token: 0x040012A5 RID: 4773
	private Rigidbody rb;

	// Token: 0x040012A6 RID: 4774
	private Rigidbody2D rb2D;

	// Token: 0x040012A7 RID: 4775
	private SmoothSyncMirror smoothSync;

	// Token: 0x040012A8 RID: 4776
	public float transformMovementSpeed = 30f;

	// Token: 0x040012A9 RID: 4777
	public float rigidbodyMovementForce = 500f;

	// Token: 0x040012AA RID: 4778
	public GameObject childObjectToControl;
}
