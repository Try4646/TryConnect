using System;
using UnityEngine;

// Token: 0x02000321 RID: 801
[CreateAssetMenu(menuName = "Game Settings/Player Settings", fileName = "PlayerSettings")]
public class PlayerSettings : ScriptableObject
{
	// Token: 0x14000025 RID: 37
	// (add) Token: 0x06001AEF RID: 6895 RVA: 0x00072198 File Offset: 0x00070398
	// (remove) Token: 0x06001AF0 RID: 6896 RVA: 0x000721CC File Offset: 0x000703CC
	public static event Action<PlayerSettings> SettingsChanged;

	// Token: 0x06001AF1 RID: 6897 RVA: 0x000721FF File Offset: 0x000703FF
	public void NotifyChanged()
	{
		Action<PlayerSettings> settingsChanged = PlayerSettings.SettingsChanged;
		if (settingsChanged == null)
		{
			return;
		}
		settingsChanged(this);
	}

	// Token: 0x040011B2 RID: 4530
	public float playerRadius;

	// Token: 0x040011B3 RID: 4531
	public float playerHeadRadius;

	// Token: 0x040011B4 RID: 4532
	public float headHeight;

	// Token: 0x040011B5 RID: 4533
	public float headHeightCrouch;

	// Token: 0x040011B6 RID: 4534
	[Header("Player Movement")]
	public float maxSpeed;

	// Token: 0x040011B7 RID: 4535
	public float acceleration;

	// Token: 0x040011B8 RID: 4536
	public float sprintMaxSpeed;

	// Token: 0x040011B9 RID: 4537
	public float crouchMaxSpeed;

	// Token: 0x040011BA RID: 4538
	public float rollMaxSpeed;

	// Token: 0x040011BB RID: 4539
	public float rollAcceleration;

	// Token: 0x040011BC RID: 4540
	public float jumpForce;

	// Token: 0x040011BD RID: 4541
	public float gravity;

	// Token: 0x040011BE RID: 4542
	public float groundCheckDistance;

	// Token: 0x040011BF RID: 4543
	public float maxSlopeAngle;

	// Token: 0x040011C0 RID: 4544
	public float maxStepHeight;

	// Token: 0x040011C1 RID: 4545
	public float stepCheckDistance;

	// Token: 0x040011C2 RID: 4546
	public float stepUpDistance;

	// Token: 0x040011C3 RID: 4547
	public LayerMask groundMask;

	// Token: 0x040011C4 RID: 4548
	public float landThreshold;

	// Token: 0x040011C5 RID: 4549
	public float ragdollDuration;

	// Token: 0x040011C6 RID: 4550
	public float headMoveDuration;

	// Token: 0x040011C7 RID: 4551
	[Header("Item Settings")]
	public float minItemThrowForce;

	// Token: 0x040011C8 RID: 4552
	public float maxItemThrowForce;

	// Token: 0x040011C9 RID: 4553
	public float minItemThrowTorque;

	// Token: 0x040011CA RID: 4554
	public float maxItemThrowTorque;

	// Token: 0x040011CB RID: 4555
	public float itemThrowDuration;

	// Token: 0x040011CC RID: 4556
	public float throwThreshold;

	// Token: 0x040011CD RID: 4557
	public float constantMass;
}
