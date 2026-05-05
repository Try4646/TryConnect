using System;
using UnityEngine.Events;

// Token: 0x020000C9 RID: 201
public class InteractableEventTrigger : InteractableBase
{
	// Token: 0x060007C1 RID: 1985 RVA: 0x0001F516 File Offset: 0x0001D716
	public override void OnInteract(PlayerInteract playerInteract)
	{
		base.OnInteract(playerInteract);
		UnityEvent<PlayerInteract> unityEvent = this.onInteractEvent;
		if (unityEvent == null)
		{
			return;
		}
		unityEvent.Invoke(playerInteract);
	}

	// Token: 0x060007C2 RID: 1986 RVA: 0x0001F530 File Offset: 0x0001D730
	public override void ServerOnInteract(PlayerInteract playerInteract)
	{
		base.ServerOnInteract(playerInteract);
		UnityEvent<PlayerInteract> unityEvent = this.serverOnInteractEvent;
		if (unityEvent == null)
		{
			return;
		}
		unityEvent.Invoke(playerInteract);
	}

	// Token: 0x060007C3 RID: 1987 RVA: 0x0001F54A File Offset: 0x0001D74A
	public override void RpcOnInteract(PlayerInteract playerInteract)
	{
		base.RpcOnInteract(playerInteract);
		UnityEvent<PlayerInteract> unityEvent = this.rpcOnInteractEvent;
		if (unityEvent == null)
		{
			return;
		}
		unityEvent.Invoke(playerInteract);
	}

	// Token: 0x060007C4 RID: 1988 RVA: 0x0001F564 File Offset: 0x0001D764
	public override void OnHover(PlayerInteract playerInteract)
	{
		base.OnHover(playerInteract);
		UnityEvent<PlayerInteract> unityEvent = this.onHoverEvent;
		if (unityEvent == null)
		{
			return;
		}
		unityEvent.Invoke(playerInteract);
	}

	// Token: 0x060007C5 RID: 1989 RVA: 0x0001F57E File Offset: 0x0001D77E
	public override void OnHoverExit(PlayerInteract playerInteract)
	{
		base.OnHoverExit(playerInteract);
		UnityEvent<PlayerInteract> unityEvent = this.onHoverExitEvent;
		if (unityEvent == null)
		{
			return;
		}
		unityEvent.Invoke(playerInteract);
	}

	// Token: 0x060007C6 RID: 1990 RVA: 0x0001F598 File Offset: 0x0001D798
	public override void OnHold(PlayerInteract playerInteract)
	{
		base.OnHold(playerInteract);
		UnityEvent<PlayerInteract> unityEvent = this.onHoldEvent;
		if (unityEvent == null)
		{
			return;
		}
		unityEvent.Invoke(playerInteract);
	}

	// Token: 0x060007C7 RID: 1991 RVA: 0x0001F5B2 File Offset: 0x0001D7B2
	public override void OnHoldExit(PlayerInteract playerInteract)
	{
		base.OnHoldExit(playerInteract);
		UnityEvent<PlayerInteract> unityEvent = this.onHoldExitEvent;
		if (unityEvent == null)
		{
			return;
		}
		unityEvent.Invoke(playerInteract);
	}

	// Token: 0x060007C9 RID: 1993 RVA: 0x00002321 File Offset: 0x00000521
	public override bool Weaved()
	{
		return true;
	}

	// Token: 0x04000505 RID: 1285
	public UnityEvent<PlayerInteract> onInteractEvent;

	// Token: 0x04000506 RID: 1286
	public UnityEvent<PlayerInteract> serverOnInteractEvent;

	// Token: 0x04000507 RID: 1287
	public UnityEvent<PlayerInteract> rpcOnInteractEvent;

	// Token: 0x04000508 RID: 1288
	public UnityEvent<PlayerInteract> onHoverEvent;

	// Token: 0x04000509 RID: 1289
	public UnityEvent<PlayerInteract> onHoverExitEvent;

	// Token: 0x0400050A RID: 1290
	public UnityEvent<PlayerInteract> onHoldEvent;

	// Token: 0x0400050B RID: 1291
	public UnityEvent<PlayerInteract> onHoldExitEvent;
}
