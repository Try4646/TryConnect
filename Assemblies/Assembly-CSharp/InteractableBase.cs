using System;
using System.Collections.Generic;
using Mirror;
using Mirror.RemoteCalls;
using UnityEngine;
using UnityEngine.Serialization;

// Token: 0x020000C8 RID: 200
public class InteractableBase : NetworkBehaviour, IInteractable
{
	// Token: 0x14000006 RID: 6
	// (add) Token: 0x06000786 RID: 1926 RVA: 0x0001EE10 File Offset: 0x0001D010
	// (remove) Token: 0x06000787 RID: 1927 RVA: 0x0001EE48 File Offset: 0x0001D048
	public event Action<IInteractable> OnInteractableChanged;

	// Token: 0x170000AD RID: 173
	// (get) Token: 0x06000788 RID: 1928 RVA: 0x0001EE7D File Offset: 0x0001D07D
	// (set) Token: 0x06000789 RID: 1929 RVA: 0x0001EE85 File Offset: 0x0001D085
	public virtual bool HoldInteract { get; set; }

	// Token: 0x170000AE RID: 174
	// (get) Token: 0x0600078A RID: 1930 RVA: 0x0001EE8E File Offset: 0x0001D08E
	// (set) Token: 0x0600078B RID: 1931 RVA: 0x0001EE96 File Offset: 0x0001D096
	public virtual float HoldDuration { get; set; } = 0.25f;

	// Token: 0x170000AF RID: 175
	// (get) Token: 0x0600078C RID: 1932 RVA: 0x0001EE9F File Offset: 0x0001D09F
	// (set) Token: 0x0600078D RID: 1933 RVA: 0x0001EEA7 File Offset: 0x0001D0A7
	public virtual bool IsInteractable
	{
		get
		{
			return this.isInteractable;
		}
		set
		{
			this.SetField<bool>(ref this.isInteractable, value);
		}
	}

	// Token: 0x170000B0 RID: 176
	// (get) Token: 0x0600078E RID: 1934 RVA: 0x0001EEB7 File Offset: 0x0001D0B7
	// (set) Token: 0x0600078F RID: 1935 RVA: 0x0001EEBF File Offset: 0x0001D0BF
	public virtual bool MeetRequirements
	{
		get
		{
			return this.meetRequirements;
		}
		set
		{
			this.SetField<bool>(ref this.meetRequirements, value);
		}
	}

	// Token: 0x170000B1 RID: 177
	// (get) Token: 0x06000790 RID: 1936 RVA: 0x0001EECF File Offset: 0x0001D0CF
	// (set) Token: 0x06000791 RID: 1937 RVA: 0x0001EED7 File Offset: 0x0001D0D7
	public virtual bool IsBeingHovered { get; set; }

	// Token: 0x170000B2 RID: 178
	// (get) Token: 0x06000792 RID: 1938 RVA: 0x0001EEE0 File Offset: 0x0001D0E0
	// (set) Token: 0x06000793 RID: 1939 RVA: 0x0001EEE8 File Offset: 0x0001D0E8
	public virtual bool IsBeingHold { get; set; }

	// Token: 0x170000B3 RID: 179
	// (get) Token: 0x06000794 RID: 1940 RVA: 0x0001EEF1 File Offset: 0x0001D0F1
	// (set) Token: 0x06000795 RID: 1941 RVA: 0x0001EEF9 File Offset: 0x0001D0F9
	public float HoldProgress { get; set; }

	// Token: 0x170000B4 RID: 180
	// (get) Token: 0x06000796 RID: 1942 RVA: 0x0001EF02 File Offset: 0x0001D102
	// (set) Token: 0x06000797 RID: 1943 RVA: 0x0001EF0A File Offset: 0x0001D10A
	public virtual string TooltipMessage
	{
		get
		{
			return this.tooltipMessage;
		}
		set
		{
			this.SetField<string>(ref this.tooltipMessage, value);
		}
	}

	// Token: 0x170000B5 RID: 181
	// (get) Token: 0x06000798 RID: 1944 RVA: 0x0001EF1A File Offset: 0x0001D11A
	// (set) Token: 0x06000799 RID: 1945 RVA: 0x0001EF22 File Offset: 0x0001D122
	public virtual string InteractableName
	{
		get
		{
			return this.interactableName;
		}
		set
		{
			this.SetField<string>(ref this.interactableName, value);
		}
	}

	// Token: 0x170000B6 RID: 182
	// (get) Token: 0x0600079A RID: 1946 RVA: 0x0001EF32 File Offset: 0x0001D132
	// (set) Token: 0x0600079B RID: 1947 RVA: 0x0001EF3A File Offset: 0x0001D13A
	public virtual CursorManager.CursorType CursorType
	{
		get
		{
			return this.cursorType;
		}
		set
		{
			this.SetField<CursorManager.CursorType>(ref this.cursorType, value);
		}
	}

	// Token: 0x0600079C RID: 1948 RVA: 0x0001EF4C File Offset: 0x0001D14C
	public override void OnStartClient()
	{
		base.OnStartClient();
		if (!base.isLocalPlayer)
		{
			this._outline = base.gameObject.AddComponent<Outline>();
			this._outline.OutlineMode = Outline.Mode.OutlineAll;
			this._outline.OutlineColor = Color.yellow;
			this._outline.OutlineWidth = 5f;
			this._outline.enabled = false;
		}
	}

	// Token: 0x0600079D RID: 1949 RVA: 0x0001EFB0 File Offset: 0x0001D1B0
	private void Awake()
	{
		this.OnAwake();
	}

	// Token: 0x0600079E RID: 1950 RVA: 0x000048A7 File Offset: 0x00002AA7
	protected virtual void OnAwake()
	{
	}

	// Token: 0x0600079F RID: 1951 RVA: 0x0001EFB8 File Offset: 0x0001D1B8
	public virtual void OnHover(PlayerInteract playerInteract)
	{
		if (this.IsBeingHovered)
		{
			return;
		}
		if (!this.IsInteractable)
		{
			return;
		}
		if (!this.MeetRequirements)
		{
			return;
		}
		this.IsBeingHovered = true;
		if (this._outline)
		{
			this._outline.enabled = true;
		}
		this.IsBeingHold = false;
		this.HoldProgress = 0f;
		this.CmdOnHover(playerInteract);
	}

	// Token: 0x060007A0 RID: 1952 RVA: 0x0001F01C File Offset: 0x0001D21C
	[Command(requiresAuthority = false)]
	private void CmdOnHover(PlayerInteract playerInteract)
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		writer.WriteNetworkBehaviour(playerInteract);
		base.SendCommandInternal("System.Void InteractableBase::CmdOnHover(PlayerInteract)", 1096060196, writer, 0, false);
		NetworkWriterPool.Return(writer);
	}

	// Token: 0x060007A1 RID: 1953 RVA: 0x0001F056 File Offset: 0x0001D256
	[Server]
	public virtual void ServerOnHover(PlayerInteract playerInteract)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void InteractableBase::ServerOnHover(PlayerInteract)' called when server was not active");
			return;
		}
	}

	// Token: 0x060007A2 RID: 1954 RVA: 0x0001F070 File Offset: 0x0001D270
	public virtual void OnHoverExit(PlayerInteract playerInteract)
	{
		if (!this.IsBeingHovered)
		{
			return;
		}
		this.IsBeingHovered = false;
		if (this._outline)
		{
			this._outline.enabled = false;
		}
		this.IsBeingHold = false;
		this.HoldProgress = 0f;
		this.CmdOnHoverExit(playerInteract);
	}

	// Token: 0x060007A3 RID: 1955 RVA: 0x0001F0C0 File Offset: 0x0001D2C0
	[Command(requiresAuthority = false)]
	private void CmdOnHoverExit(PlayerInteract playerInteract)
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		writer.WriteNetworkBehaviour(playerInteract);
		base.SendCommandInternal("System.Void InteractableBase::CmdOnHoverExit(PlayerInteract)", 58381884, writer, 0, false);
		NetworkWriterPool.Return(writer);
	}

	// Token: 0x060007A4 RID: 1956 RVA: 0x0001F0FA File Offset: 0x0001D2FA
	[Server]
	public virtual void ServerOnHoverExit(PlayerInteract playerInteract)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void InteractableBase::ServerOnHoverExit(PlayerInteract)' called when server was not active");
			return;
		}
	}

	// Token: 0x060007A5 RID: 1957 RVA: 0x0001F111 File Offset: 0x0001D311
	public virtual void OnHold(PlayerInteract playerInteract)
	{
		if (this.IsBeingHold)
		{
			return;
		}
		this.IsBeingHold = true;
		this.HoldProgress = 0f;
		this.CmdOnHold(playerInteract);
	}

	// Token: 0x060007A6 RID: 1958 RVA: 0x0001F138 File Offset: 0x0001D338
	[Command(requiresAuthority = false)]
	private void CmdOnHold(PlayerInteract playerInteract)
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		writer.WriteNetworkBehaviour(playerInteract);
		base.SendCommandInternal("System.Void InteractableBase::CmdOnHold(PlayerInteract)", 1136293411, writer, 0, false);
		NetworkWriterPool.Return(writer);
	}

	// Token: 0x060007A7 RID: 1959 RVA: 0x0001F172 File Offset: 0x0001D372
	[Server]
	public virtual void ServerOnHold(PlayerInteract playerInteract)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void InteractableBase::ServerOnHold(PlayerInteract)' called when server was not active");
			return;
		}
	}

	// Token: 0x060007A8 RID: 1960 RVA: 0x0001F189 File Offset: 0x0001D389
	public virtual void OnHoldExit(PlayerInteract playerInteract)
	{
		if (!this.IsBeingHold)
		{
			return;
		}
		this.IsBeingHold = false;
		this.HoldProgress = 0f;
		this.CmdOnHoldExit(playerInteract);
	}

	// Token: 0x060007A9 RID: 1961 RVA: 0x0001F1B0 File Offset: 0x0001D3B0
	[Command(requiresAuthority = false)]
	private void CmdOnHoldExit(PlayerInteract playerInteract)
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		writer.WriteNetworkBehaviour(playerInteract);
		base.SendCommandInternal("System.Void InteractableBase::CmdOnHoldExit(PlayerInteract)", 1949110787, writer, 0, false);
		NetworkWriterPool.Return(writer);
	}

	// Token: 0x060007AA RID: 1962 RVA: 0x0001F1EA File Offset: 0x0001D3EA
	[Server]
	public virtual void ServerOnHoldExit(PlayerInteract playerInteract)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void InteractableBase::ServerOnHoldExit(PlayerInteract)' called when server was not active");
			return;
		}
	}

	// Token: 0x060007AB RID: 1963 RVA: 0x0001F201 File Offset: 0x0001D401
	public virtual void OnInteract(PlayerInteract playerInteract)
	{
		this.CmdOnInteract(playerInteract);
	}

	// Token: 0x060007AC RID: 1964 RVA: 0x0001F20C File Offset: 0x0001D40C
	[Command(requiresAuthority = false)]
	private void CmdOnInteract(PlayerInteract playerInteract)
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		writer.WriteNetworkBehaviour(playerInteract);
		base.SendCommandInternal("System.Void InteractableBase::CmdOnInteract(PlayerInteract)", -1367533928, writer, 0, false);
		NetworkWriterPool.Return(writer);
	}

	// Token: 0x060007AD RID: 1965 RVA: 0x0001F246 File Offset: 0x0001D446
	[Server]
	public virtual void ServerOnInteract(PlayerInteract playerInteract)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void InteractableBase::ServerOnInteract(PlayerInteract)' called when server was not active");
			return;
		}
		this.ClientRpcOnInteract(playerInteract);
	}

	// Token: 0x060007AE RID: 1966 RVA: 0x0001F264 File Offset: 0x0001D464
	[ClientRpc]
	private void ClientRpcOnInteract(PlayerInteract playerInteract)
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		writer.WriteNetworkBehaviour(playerInteract);
		this.SendRPCInternal("System.Void InteractableBase::ClientRpcOnInteract(PlayerInteract)", 1920928442, writer, 0, true);
		NetworkWriterPool.Return(writer);
	}

	// Token: 0x060007AF RID: 1967 RVA: 0x000048A7 File Offset: 0x00002AA7
	public virtual void RpcOnInteract(PlayerInteract playerInteract)
	{
	}

	// Token: 0x060007B0 RID: 1968 RVA: 0x0001F29E File Offset: 0x0001D49E
	protected virtual void OnDestroy()
	{
		this.OnInteractableChanged = null;
	}

	// Token: 0x060007B1 RID: 1969 RVA: 0x0001F2A7 File Offset: 0x0001D4A7
	private bool SetField<T>(ref T field, T value)
	{
		if (EqualityComparer<T>.Default.Equals(field, value))
		{
			return false;
		}
		field = value;
		Action<IInteractable> onInteractableChanged = this.OnInteractableChanged;
		if (onInteractableChanged != null)
		{
			onInteractableChanged(this);
		}
		return true;
	}

	// Token: 0x060007B3 RID: 1971 RVA: 0x00002321 File Offset: 0x00000521
	public override bool Weaved()
	{
		return true;
	}

	// Token: 0x060007B4 RID: 1972 RVA: 0x0001F316 File Offset: 0x0001D516
	protected void UserCode_CmdOnHover__PlayerInteract(PlayerInteract playerInteract)
	{
		this.ServerOnHover(playerInteract);
	}

	// Token: 0x060007B5 RID: 1973 RVA: 0x0001F31F File Offset: 0x0001D51F
	protected static void InvokeUserCode_CmdOnHover__PlayerInteract(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdOnHover called on client.");
			return;
		}
		((InteractableBase)obj).UserCode_CmdOnHover__PlayerInteract(reader.ReadNetworkBehaviour<PlayerInteract>());
	}

	// Token: 0x060007B6 RID: 1974 RVA: 0x0001F348 File Offset: 0x0001D548
	protected void UserCode_CmdOnHoverExit__PlayerInteract(PlayerInteract playerInteract)
	{
		this.ServerOnHoverExit(playerInteract);
	}

	// Token: 0x060007B7 RID: 1975 RVA: 0x0001F351 File Offset: 0x0001D551
	protected static void InvokeUserCode_CmdOnHoverExit__PlayerInteract(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdOnHoverExit called on client.");
			return;
		}
		((InteractableBase)obj).UserCode_CmdOnHoverExit__PlayerInteract(reader.ReadNetworkBehaviour<PlayerInteract>());
	}

	// Token: 0x060007B8 RID: 1976 RVA: 0x0001F37A File Offset: 0x0001D57A
	protected void UserCode_CmdOnHold__PlayerInteract(PlayerInteract playerInteract)
	{
		this.ServerOnHold(playerInteract);
	}

	// Token: 0x060007B9 RID: 1977 RVA: 0x0001F383 File Offset: 0x0001D583
	protected static void InvokeUserCode_CmdOnHold__PlayerInteract(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdOnHold called on client.");
			return;
		}
		((InteractableBase)obj).UserCode_CmdOnHold__PlayerInteract(reader.ReadNetworkBehaviour<PlayerInteract>());
	}

	// Token: 0x060007BA RID: 1978 RVA: 0x0001F3AC File Offset: 0x0001D5AC
	protected void UserCode_CmdOnHoldExit__PlayerInteract(PlayerInteract playerInteract)
	{
		this.ServerOnHoldExit(playerInteract);
	}

	// Token: 0x060007BB RID: 1979 RVA: 0x0001F3B5 File Offset: 0x0001D5B5
	protected static void InvokeUserCode_CmdOnHoldExit__PlayerInteract(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdOnHoldExit called on client.");
			return;
		}
		((InteractableBase)obj).UserCode_CmdOnHoldExit__PlayerInteract(reader.ReadNetworkBehaviour<PlayerInteract>());
	}

	// Token: 0x060007BC RID: 1980 RVA: 0x0001F3DE File Offset: 0x0001D5DE
	protected void UserCode_CmdOnInteract__PlayerInteract(PlayerInteract playerInteract)
	{
		this.ServerOnInteract(playerInteract);
	}

	// Token: 0x060007BD RID: 1981 RVA: 0x0001F3E7 File Offset: 0x0001D5E7
	protected static void InvokeUserCode_CmdOnInteract__PlayerInteract(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdOnInteract called on client.");
			return;
		}
		((InteractableBase)obj).UserCode_CmdOnInteract__PlayerInteract(reader.ReadNetworkBehaviour<PlayerInteract>());
	}

	// Token: 0x060007BE RID: 1982 RVA: 0x0001F410 File Offset: 0x0001D610
	protected void UserCode_ClientRpcOnInteract__PlayerInteract(PlayerInteract playerInteract)
	{
		this.RpcOnInteract(playerInteract);
	}

	// Token: 0x060007BF RID: 1983 RVA: 0x0001F419 File Offset: 0x0001D619
	protected static void InvokeUserCode_ClientRpcOnInteract__PlayerInteract(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC ClientRpcOnInteract called on server.");
			return;
		}
		((InteractableBase)obj).UserCode_ClientRpcOnInteract__PlayerInteract(reader.ReadNetworkBehaviour<PlayerInteract>());
	}

	// Token: 0x060007C0 RID: 1984 RVA: 0x0001F444 File Offset: 0x0001D644
	static InteractableBase()
	{
		RemoteProcedureCalls.RegisterCommand(typeof(InteractableBase), "System.Void InteractableBase::CmdOnHover(PlayerInteract)", new RemoteCallDelegate(InteractableBase.InvokeUserCode_CmdOnHover__PlayerInteract), false);
		RemoteProcedureCalls.RegisterCommand(typeof(InteractableBase), "System.Void InteractableBase::CmdOnHoverExit(PlayerInteract)", new RemoteCallDelegate(InteractableBase.InvokeUserCode_CmdOnHoverExit__PlayerInteract), false);
		RemoteProcedureCalls.RegisterCommand(typeof(InteractableBase), "System.Void InteractableBase::CmdOnHold(PlayerInteract)", new RemoteCallDelegate(InteractableBase.InvokeUserCode_CmdOnHold__PlayerInteract), false);
		RemoteProcedureCalls.RegisterCommand(typeof(InteractableBase), "System.Void InteractableBase::CmdOnHoldExit(PlayerInteract)", new RemoteCallDelegate(InteractableBase.InvokeUserCode_CmdOnHoldExit__PlayerInteract), false);
		RemoteProcedureCalls.RegisterCommand(typeof(InteractableBase), "System.Void InteractableBase::CmdOnInteract(PlayerInteract)", new RemoteCallDelegate(InteractableBase.InvokeUserCode_CmdOnInteract__PlayerInteract), false);
		RemoteProcedureCalls.RegisterRpc(typeof(InteractableBase), "System.Void InteractableBase::ClientRpcOnInteract(PlayerInteract)", new RemoteCallDelegate(InteractableBase.InvokeUserCode_ClientRpcOnInteract__PlayerInteract));
	}

	// Token: 0x040004FC RID: 1276
	[FormerlySerializedAs("<IsInteractable>k__BackingField")]
	[SerializeField]
	private bool isInteractable = true;

	// Token: 0x040004FD RID: 1277
	[FormerlySerializedAs("<MeetRequirements>k__BackingField")]
	[SerializeField]
	private bool meetRequirements = true;

	// Token: 0x04000501 RID: 1281
	[FormerlySerializedAs("<TooltipMessage>k__BackingField")]
	[SerializeField]
	private string tooltipMessage = "Press [E] to Interact";

	// Token: 0x04000502 RID: 1282
	[FormerlySerializedAs("<Name>k__BackingField")]
	[SerializeField]
	private string interactableName = "Item";

	// Token: 0x04000503 RID: 1283
	[FormerlySerializedAs("<CursorType>k__BackingField")]
	[SerializeField]
	private CursorManager.CursorType cursorType = CursorManager.CursorType.Interact;

	// Token: 0x04000504 RID: 1284
	private Outline _outline;
}
