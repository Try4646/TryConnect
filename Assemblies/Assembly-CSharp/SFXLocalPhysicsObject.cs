using System;
using FMOD.Studio;
using FMODUnity;
using UnityEngine;

// Token: 0x0200027D RID: 637
public class SFXLocalPhysicsObject : MonoBehaviour
{
	// Token: 0x060016AA RID: 5802 RVA: 0x00060BFA File Offset: 0x0005EDFA
	private void Awake()
	{
		this.rb = base.GetComponent<Rigidbody>();
	}

	// Token: 0x060016AB RID: 5803 RVA: 0x00060C08 File Offset: 0x0005EE08
	private void Start()
	{
		this.hitCooldownTimer = Time.time + this.startSleepTime;
		this.playerHitCooldownTimer = Time.time + this.startSleepTime;
	}

	// Token: 0x060016AC RID: 5804 RVA: 0x00060C30 File Offset: 0x0005EE30
	private void OnCollisionEnter(Collision other)
	{
		if (!base.enabled)
		{
			return;
		}
		if (this.eventRef.IsNull)
		{
			return;
		}
		if (this.hitCooldownTimer >= Time.time)
		{
			return;
		}
		if (other.gameObject.layer == this.playerLayer)
		{
			if (this.canHitPlayer)
			{
				this.OnPlayerCollision(other);
			}
			return;
		}
		Vector3 relativeVelocity = other.relativeVelocity;
		if (relativeVelocity.magnitude < this.SensitivityThreshold)
		{
			return;
		}
		float num = Mathf.Max(0f, relativeVelocity.magnitude - this.SensitivityThreshold);
		num = Mathf.Clamp01(num * 0.07f);
		this.HandleHit(num);
	}

	// Token: 0x060016AD RID: 5805 RVA: 0x00060CCC File Offset: 0x0005EECC
	private void OnPlayerCollision(Collision other)
	{
		if (this.playerHitReference.IsNull)
		{
			return;
		}
		if (this.wasSleeping)
		{
			return;
		}
		if (other.relativeVelocity.magnitude < 6.5f)
		{
			return;
		}
		if (this.playerHitCooldownTimer >= Time.time)
		{
			return;
		}
		Vector3 relativeVelocity = other.relativeVelocity;
		if (relativeVelocity.magnitude < this.SensitivityThreshold * this.playerHitThresholdMultiplier)
		{
			return;
		}
		float num = Mathf.Max(0f, relativeVelocity.magnitude - this.SensitivityThreshold);
		num = Mathf.Clamp01(num * 0.07f);
		this.HandlePlayerHit(num);
	}

	// Token: 0x060016AE RID: 5806 RVA: 0x00060D60 File Offset: 0x0005EF60
	private void OnCollisionStay(Collision other)
	{
		if (!base.enabled)
		{
			return;
		}
		if (!this.stayCollision)
		{
			return;
		}
		if (this.eventRef.IsNull)
		{
			return;
		}
		if (this.wasSleeping)
		{
			return;
		}
		if (this.hitCooldownTimer >= Time.time)
		{
			return;
		}
		if (other.gameObject.layer == this.playerLayer)
		{
			return;
		}
		Vector3 impulse = other.impulse;
		if (impulse.magnitude < this.SensitivityThreshold * this.staySensitivityMultiplier)
		{
			return;
		}
		float magnitude = Mathf.Max(0f, impulse.magnitude - this.SensitivityThreshold);
		this.HandleHit(magnitude);
	}

	// Token: 0x060016AF RID: 5807 RVA: 0x00060DF6 File Offset: 0x0005EFF6
	private void LateUpdate()
	{
		if (!base.enabled)
		{
			return;
		}
		this.wasSleeping = this.rb.IsSleeping();
	}

	// Token: 0x060016B0 RID: 5808 RVA: 0x00060E12 File Offset: 0x0005F012
	private void HandleHit(float magnitude)
	{
		this.PlayHit(magnitude);
		this.hitCooldownTimer = Time.time + this.hitCooldownTime * Random.Range(0.9f, 1f);
	}

	// Token: 0x060016B1 RID: 5809 RVA: 0x00060E40 File Offset: 0x0005F040
	private void PlayHit(float magnitude)
	{
		SFXParams[] sFXParams = new SFXParams[]
		{
			new SFXParams("PhysicsObjectType", 0f),
			new SFXParams("Magnitude", magnitude)
		};
		SFXManager.SFXOneShotWithParameters(this.eventRef, sFXParams, base.gameObject.transform.position, this.pitchMod);
	}

	// Token: 0x060016B2 RID: 5810 RVA: 0x00060E9E File Offset: 0x0005F09E
	private void HandlePlayerHit(float magnitude)
	{
		this.PlayPlayerHit(magnitude);
		this.playerHitCooldownTimer = Time.time + this.playerHitCooldownTime;
	}

	// Token: 0x060016B3 RID: 5811 RVA: 0x00060EBC File Offset: 0x0005F0BC
	private void PlayPlayerHit(float magnitude)
	{
		SFXParams[] sFXParams = new SFXParams[]
		{
			new SFXParams("Magnitude", magnitude)
		};
		SFXManager.SFXOneShotWithParameters(this.playerHitReference, sFXParams, base.gameObject.transform.position, 1f);
	}

	// Token: 0x060016B4 RID: 5812 RVA: 0x00060F03 File Offset: 0x0005F103
	public void SetPlayerThrowCooldown()
	{
		this.playerHitCooldownTimer = Time.time + this.playerThrowCooldown;
	}

	// Token: 0x04000EBE RID: 3774
	[SerializeField]
	private EventReference eventRef;

	// Token: 0x04000EBF RID: 3775
	[SerializeField]
	private float SensitivityThreshold = 3f;

	// Token: 0x04000EC0 RID: 3776
	[Header("Stay Collision")]
	[SerializeField]
	private bool stayCollision = true;

	// Token: 0x04000EC1 RID: 3777
	[SerializeField]
	private float staySensitivityMultiplier = 0.2f;

	// Token: 0x04000EC2 RID: 3778
	[Header("Other")]
	[SerializeField]
	private float hitCooldownTime = 0.3f;

	// Token: 0x04000EC3 RID: 3779
	private float hitCooldownTimer;

	// Token: 0x04000EC4 RID: 3780
	[SerializeField]
	private float pitchMod = 1f;

	// Token: 0x04000EC5 RID: 3781
	private EventInstance movementInstance;

	// Token: 0x04000EC6 RID: 3782
	private Rigidbody rb;

	// Token: 0x04000EC7 RID: 3783
	private int playerLayer = 6;

	// Token: 0x04000EC8 RID: 3784
	private bool wasSleeping;

	// Token: 0x04000EC9 RID: 3785
	[SerializeField]
	private EventReference playerHitReference;

	// Token: 0x04000ECA RID: 3786
	private float playerHitCooldownTime = 0.8f;

	// Token: 0x04000ECB RID: 3787
	private float playerHitCooldownTimer;

	// Token: 0x04000ECC RID: 3788
	private float playerHitThresholdMultiplier = 3f;

	// Token: 0x04000ECD RID: 3789
	private float playerThrowCooldown = 0.3f;

	// Token: 0x04000ECE RID: 3790
	private float startSleepTime = 0.3f;

	// Token: 0x04000ECF RID: 3791
	[SerializeField]
	private bool canHitPlayer = true;
}
