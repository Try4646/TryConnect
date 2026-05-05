using System;
using UnityEngine;

// Token: 0x020002F0 RID: 752
public class Trampoline : MonoBehaviour
{
	// Token: 0x06001A20 RID: 6688 RVA: 0x0006E146 File Offset: 0x0006C346
	private void OnCollisionEnter(Collision collision)
	{
		this.animator.SetTrigger("bounce");
	}

	// Token: 0x040010B4 RID: 4276
	[SerializeField]
	private Animator animator;
}
