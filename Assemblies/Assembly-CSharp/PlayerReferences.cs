using System;
using Dissonance.Integrations.MirrorIgnorance;
using Mirror;
using UnityEngine;

// Token: 0x0200017D RID: 381
[Serializable]
public class PlayerReferences
{
	// Token: 0x06000E55 RID: 3669 RVA: 0x0003B5E0 File Offset: 0x000397E0
	public PlayerReferences(NetworkIdentity netIdentity)
	{
		this.identity = netIdentity;
		this.transform = netIdentity.transform;
		this.profile = netIdentity.GetComponent<PlayerProfile>();
		this.controller = netIdentity.GetComponent<PlayerController>();
		this.headTransform = this.controller.head.transform;
		this.buff = netIdentity.GetComponent<PlayerBuff>();
		this.organs = netIdentity.GetComponent<PlayerOrgans>();
		this.mirrorIgnorance = netIdentity.GetComponent<MirrorIgnorancePlayer>();
		this.carry = netIdentity.GetComponent<PlayerCarry>();
		this.mouth = netIdentity.GetComponent<PlayerMouth>();
	}

	// Token: 0x04000926 RID: 2342
	public NetworkIdentity identity;

	// Token: 0x04000927 RID: 2343
	public Transform transform;

	// Token: 0x04000928 RID: 2344
	public Transform headTransform;

	// Token: 0x04000929 RID: 2345
	public PlayerProfile profile;

	// Token: 0x0400092A RID: 2346
	public PlayerController controller;

	// Token: 0x0400092B RID: 2347
	public PlayerBuff buff;

	// Token: 0x0400092C RID: 2348
	public PlayerOrgans organs;

	// Token: 0x0400092D RID: 2349
	public MirrorIgnorancePlayer mirrorIgnorance;

	// Token: 0x0400092E RID: 2350
	public PlayerCarry carry;

	// Token: 0x0400092F RID: 2351
	public PlayerMouth mouth;
}
