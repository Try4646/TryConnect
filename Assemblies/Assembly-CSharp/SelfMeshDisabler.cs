using System;
using Mirror;
using UnityEngine;

// Token: 0x0200020D RID: 525
public class SelfMeshDisabler : MonoBehaviour
{
	// Token: 0x06001367 RID: 4967 RVA: 0x00053B3D File Offset: 0x00051D3D
	private void Start()
	{
		this._networkIdentity = base.transform.root.GetComponent<NetworkIdentity>();
		if (this.IsLocalPlayer())
		{
			base.gameObject.layer = LayerMask.NameToLayer("SelfMeshPlayer");
		}
	}

	// Token: 0x06001368 RID: 4968 RVA: 0x00053B72 File Offset: 0x00051D72
	private bool IsLocalPlayer()
	{
		return this._networkIdentity != null && NetworkClient.localPlayer == this._networkIdentity;
	}

	// Token: 0x06001369 RID: 4969 RVA: 0x00053B94 File Offset: 0x00051D94
	public void ToggleMesh(bool enabled)
	{
		if (this.IsLocalPlayer())
		{
			base.gameObject.layer = (enabled ? LayerMask.NameToLayer("Player") : LayerMask.NameToLayer("SelfMeshPlayer"));
		}
	}

	// Token: 0x04000C60 RID: 3168
	private NetworkIdentity _networkIdentity;
}
