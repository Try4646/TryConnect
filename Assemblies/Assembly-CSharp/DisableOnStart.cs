using System;
using UnityEngine;

// Token: 0x02000295 RID: 661
public class DisableOnStart : MonoBehaviour
{
	// Token: 0x0600177D RID: 6013 RVA: 0x00059C47 File Offset: 0x00057E47
	private void Awake()
	{
		base.gameObject.SetActive(false);
	}
}
