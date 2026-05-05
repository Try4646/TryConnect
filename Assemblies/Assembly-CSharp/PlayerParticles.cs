using System;
using UnityEngine;

// Token: 0x02000208 RID: 520
public class PlayerParticles : MonoBehaviour
{
	// Token: 0x0600133C RID: 4924 RVA: 0x000533B5 File Offset: 0x000515B5
	private void OnEnable()
	{
		this.playerController.OnClientJumped += this.OnJump;
		this.playerController.OnClientLanded += this.OnLand;
	}

	// Token: 0x0600133D RID: 4925 RVA: 0x000533E5 File Offset: 0x000515E5
	private void OnDisable()
	{
		this.playerController.OnClientJumped -= this.OnJump;
		this.playerController.OnClientLanded -= this.OnLand;
	}

	// Token: 0x0600133E RID: 4926 RVA: 0x00053415 File Offset: 0x00051615
	private void OnJump(bool wasGrounded)
	{
		if (!wasGrounded)
		{
			return;
		}
		this.jumpParticles.Play();
	}

	// Token: 0x0600133F RID: 4927 RVA: 0x00053426 File Offset: 0x00051626
	private void OnLand(float fallImpact)
	{
		this.landParticles.Play();
	}

	// Token: 0x06001340 RID: 4928 RVA: 0x00053433 File Offset: 0x00051633
	private void Update()
	{
		this.SetHasBody();
		this.SetIsGrounded();
		this.SetMoveParticles();
	}

	// Token: 0x06001341 RID: 4929 RVA: 0x00053448 File Offset: 0x00051648
	private void SetHasBody()
	{
		bool hasBody = this.playerController.hasBody;
		if (hasBody != this._hasBody)
		{
			this._hasBody = hasBody;
			this.OnHasBodyChanged();
		}
	}

	// Token: 0x06001342 RID: 4930 RVA: 0x00053477 File Offset: 0x00051677
	private void OnHasBodyChanged()
	{
		this.moveParticles.gameObject.SetActive(this._hasBody);
		this.moveTrail.gameObject.SetActive(this._hasBody);
	}

	// Token: 0x06001343 RID: 4931 RVA: 0x000534A8 File Offset: 0x000516A8
	private void SetIsGrounded()
	{
		bool isGrounded = this.playerController.isGrounded;
		if (isGrounded != this._isGrounded)
		{
			this._isGrounded = isGrounded;
			this.OnGroundedChange();
		}
	}

	// Token: 0x06001344 RID: 4932 RVA: 0x000534D7 File Offset: 0x000516D7
	private void OnGroundedChange()
	{
		if (this._isGrounded)
		{
			this.moveParticles.Play();
			this.moveTrail.Play();
			return;
		}
		this.moveParticles.Stop();
		this.moveTrail.Stop();
	}

	// Token: 0x06001345 RID: 4933 RVA: 0x00053510 File Offset: 0x00051710
	private void SetMoveParticles()
	{
		if (!this._isGrounded)
		{
			return;
		}
		Vector3 serverVelocity = this.playerController.serverVelocity;
		if (serverVelocity.sqrMagnitude < 0.01f)
		{
			return;
		}
		this.moveParticles.transform.rotation = FathF.LookRotationUpPriority(-serverVelocity.normalized, base.transform.up);
	}

	// Token: 0x04000C44 RID: 3140
	[SerializeField]
	private PlayerController playerController;

	// Token: 0x04000C45 RID: 3141
	[SerializeField]
	private ParticleSystem moveTrail;

	// Token: 0x04000C46 RID: 3142
	[SerializeField]
	private ParticleSystem moveParticles;

	// Token: 0x04000C47 RID: 3143
	[SerializeField]
	private ParticleSystem jumpParticles;

	// Token: 0x04000C48 RID: 3144
	[SerializeField]
	private ParticleSystem landParticles;

	// Token: 0x04000C49 RID: 3145
	private bool _isGrounded;

	// Token: 0x04000C4A RID: 3146
	private bool _hasBody;
}
