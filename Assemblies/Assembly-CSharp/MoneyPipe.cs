using System;
using Extensions;
using Mirror;
using UnityEngine;

// Token: 0x020000BC RID: 188
public class MoneyPipe : NetworkBehaviour
{
	// Token: 0x0600071D RID: 1821 RVA: 0x0001E11C File Offset: 0x0001C31C
	[Server]
	public void ServerStartSucking()
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void MoneyPipe::ServerStartSucking()' called when server was not active");
			return;
		}
		foreach (DebtBag debtBag in NetworkSingleton<WinSceneManager>.Instance.debtBags)
		{
			debtBag.ServerSuckToPipe(this.pipeSuckTransform.position, this.pipeSuckTransform.up);
		}
	}

	// Token: 0x0600071F RID: 1823 RVA: 0x00002321 File Offset: 0x00000521
	public override bool Weaved()
	{
		return true;
	}

	// Token: 0x040004C1 RID: 1217
	[SerializeField]
	private Transform pipeSuckTransform;
}
