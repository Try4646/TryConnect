using System;
using Mirror;

// Token: 0x020000C6 RID: 198
public interface IInteractable
{
	// Token: 0x14000005 RID: 5
	// (add) Token: 0x06000765 RID: 1893
	// (remove) Token: 0x06000766 RID: 1894
	event Action<IInteractable> OnInteractableChanged;

	// Token: 0x170000A3 RID: 163
	// (get) Token: 0x06000767 RID: 1895
	// (set) Token: 0x06000768 RID: 1896
	float HoldDuration { get; set; }

	// Token: 0x170000A4 RID: 164
	// (get) Token: 0x06000769 RID: 1897
	// (set) Token: 0x0600076A RID: 1898
	bool HoldInteract { get; set; }

	// Token: 0x170000A5 RID: 165
	// (get) Token: 0x0600076B RID: 1899
	// (set) Token: 0x0600076C RID: 1900
	bool IsInteractable { get; set; }

	// Token: 0x170000A6 RID: 166
	// (get) Token: 0x0600076D RID: 1901
	// (set) Token: 0x0600076E RID: 1902
	bool MeetRequirements { get; set; }

	// Token: 0x170000A7 RID: 167
	// (get) Token: 0x0600076F RID: 1903
	// (set) Token: 0x06000770 RID: 1904
	bool IsBeingHovered { get; set; }

	// Token: 0x170000A8 RID: 168
	// (get) Token: 0x06000771 RID: 1905
	// (set) Token: 0x06000772 RID: 1906
	bool IsBeingHold { get; set; }

	// Token: 0x170000A9 RID: 169
	// (get) Token: 0x06000773 RID: 1907
	// (set) Token: 0x06000774 RID: 1908
	float HoldProgress { get; set; }

	// Token: 0x170000AA RID: 170
	// (get) Token: 0x06000775 RID: 1909
	// (set) Token: 0x06000776 RID: 1910
	string TooltipMessage { get; set; }

	// Token: 0x170000AB RID: 171
	// (get) Token: 0x06000777 RID: 1911
	// (set) Token: 0x06000778 RID: 1912
	string InteractableName { get; set; }

	// Token: 0x170000AC RID: 172
	// (get) Token: 0x06000779 RID: 1913
	// (set) Token: 0x0600077A RID: 1914
	CursorManager.CursorType CursorType { get; set; }

	// Token: 0x0600077B RID: 1915
	void OnHover(PlayerInteract playerInteract);

	// Token: 0x0600077C RID: 1916
	[Server]
	void ServerOnHover(PlayerInteract playerInteract);

	// Token: 0x0600077D RID: 1917
	void OnHoverExit(PlayerInteract playerInteract);

	// Token: 0x0600077E RID: 1918
	[Server]
	void ServerOnHoverExit(PlayerInteract playerInteract);

	// Token: 0x0600077F RID: 1919
	void OnHold(PlayerInteract playerInteract);

	// Token: 0x06000780 RID: 1920
	[Server]
	void ServerOnHold(PlayerInteract playerInteract);

	// Token: 0x06000781 RID: 1921
	void OnHoldExit(PlayerInteract playerInteract);

	// Token: 0x06000782 RID: 1922
	[Server]
	void ServerOnHoldExit(PlayerInteract playerInteract);

	// Token: 0x06000783 RID: 1923
	void OnInteract(PlayerInteract playerInteract);

	// Token: 0x06000784 RID: 1924
	[Server]
	void ServerOnInteract(PlayerInteract playerInteract);

	// Token: 0x06000785 RID: 1925
	void RpcOnInteract(PlayerInteract playerInteract);
}
