using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using Extensions;
using JetBrains.Annotations;
using Mirror;
using Mirror.RemoteCalls;
using MoreMountains.Feedbacks;
using Smooth;
using UnityEngine;

// Token: 0x020000D1 RID: 209
public class Item : InteractableBase
{
	// Token: 0x170000BC RID: 188
	// (get) Token: 0x06000806 RID: 2054 RVA: 0x00002321 File Offset: 0x00000521
	[Header("Settings")]
	public virtual bool ShouldShowHoverDescription
	{
		get
		{
			return true;
		}
	}

	// Token: 0x170000BD RID: 189
	// (get) Token: 0x06000807 RID: 2055 RVA: 0x0002061E File Offset: 0x0001E81E
	// (set) Token: 0x06000808 RID: 2056 RVA: 0x00020626 File Offset: 0x0001E826
	public float Mass { get; private set; }

	// Token: 0x06000809 RID: 2057 RVA: 0x00020630 File Offset: 0x0001E830
	protected override void OnAwake()
	{
		this.Rb = base.GetComponent<Rigidbody>();
		this.Mass = this.Rb.mass;
		this._nrb = base.GetComponent<NetworkRigidbodyUnreliable>();
		this._ssm = base.GetComponent<SmoothSyncMirror>();
		this._sfxPhysicsObject = base.GetComponent<SFXPhysicsObject>();
		this._ls = Resources.Load<LobbySettings>("LobbySettings");
		if (!this.modelTransform)
		{
			this.modelTransform = base.transform.Find("Model");
		}
		if (this.modelTransform && !this.handRig)
		{
			this.handRig = this.modelTransform.Find("HandRig").gameObject;
		}
		if (this.handRig && !this.handMesh)
		{
			this.handMesh = this.handRig.GetComponentInChildren<SkinnedMeshRenderer>();
		}
		if (!this.onHandFb)
		{
			Transform transform = base.transform.Find("Feedbacks");
			MMF_Player mmf_Player;
			if (transform == null)
			{
				mmf_Player = null;
			}
			else
			{
				Transform transform2 = transform.Find("OnHand");
				mmf_Player = ((transform2 != null) ? transform2.GetComponent<MMF_Player>() : null);
			}
			this.onHandFb = mmf_Player;
		}
		if (!this.onDropFb)
		{
			Transform transform3 = base.transform.Find("Feedbacks");
			MMF_Player mmf_Player2;
			if (transform3 == null)
			{
				mmf_Player2 = null;
			}
			else
			{
				Transform transform4 = transform3.Find("OnDrop");
				mmf_Player2 = ((transform4 != null) ? transform4.GetComponent<MMF_Player>() : null);
			}
			this.onDropFb = mmf_Player2;
		}
		if (!this.onThrowVfx)
		{
			Transform transform5 = base.transform.Find("Feedbacks");
			ParticleSystem particleSystem;
			if (transform5 == null)
			{
				particleSystem = null;
			}
			else
			{
				Transform transform6 = transform5.Find("OnThrowVFX");
				particleSystem = ((transform6 != null) ? transform6.GetComponent<ParticleSystem>() : null);
			}
			this.onThrowVfx = particleSystem;
		}
		foreach (Collider collider in base.GetComponentsInChildren<Collider>(true))
		{
			if (!collider.isTrigger)
			{
				this._allColliders.Add(collider);
			}
		}
	}

	// Token: 0x0600080A RID: 2058 RVA: 0x00020800 File Offset: 0x0001EA00
	protected void SetEnableColliders(bool isEnabled)
	{
		foreach (Collider collider in this._allColliders)
		{
			collider.enabled = isEnabled;
		}
	}

	// Token: 0x0600080B RID: 2059 RVA: 0x00020854 File Offset: 0x0001EA54
	protected virtual void OnEnable()
	{
		this.SubscribeToEvents(true);
		this.modelTransform.DOKill(false);
		if (!(this is PlayerCarry))
		{
			this.modelTransform.DOScale(Vector3.one, 1f).From(0f, true, false).SetEase(Ease.OutElastic, 0f, 0f).SetUpdate(true);
		}
	}

	// Token: 0x0600080C RID: 2060 RVA: 0x000208B6 File Offset: 0x0001EAB6
	protected virtual void OnDisable()
	{
		this.SubscribeToEvents(false);
	}

	// Token: 0x0600080D RID: 2061 RVA: 0x000208C0 File Offset: 0x0001EAC0
	protected virtual void SubscribeToEvents(bool isSubscribed)
	{
		if (isSubscribed)
		{
			InputEvents.OnUseItemEvent = (Action<bool>)Delegate.Combine(InputEvents.OnUseItemEvent, new Action<bool>(this.TryUseItem));
			return;
		}
		InputEvents.OnUseItemEvent = (Action<bool>)Delegate.Remove(InputEvents.OnUseItemEvent, new Action<bool>(this.TryUseItem));
	}

	// Token: 0x0600080E RID: 2062 RVA: 0x00020911 File Offset: 0x0001EB11
	public override void OnStartServer()
	{
		base.OnStartServer();
		NetworkSingleton<PlayerSpawnManager>.Instance.OnPlayerLateJoined += this.ServerOnPlayerLateJoined;
	}

	// Token: 0x0600080F RID: 2063 RVA: 0x0002092F File Offset: 0x0001EB2F
	public override void OnStopServer()
	{
		base.OnStopServer();
		NetworkSingleton<PlayerSpawnManager>.Instance.OnPlayerLateJoined -= this.ServerOnPlayerLateJoined;
	}

	// Token: 0x06000810 RID: 2064 RVA: 0x00020950 File Offset: 0x0001EB50
	[Server]
	private void ServerOnPlayerLateJoined()
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void Item::ServerOnPlayerLateJoined()' called when server was not active");
			return;
		}
		if (this.NetworkHolder)
		{
			this.SetPickedUp(this.NetworkHolder);
			this.RpcPickup(this.NetworkHolder);
			this.OnPickedUp(this.NetworkHolder);
			this.ServerHandEnter(this.NetworkHolder);
		}
	}

	// Token: 0x06000811 RID: 2065 RVA: 0x000209B0 File Offset: 0x0001EBB0
	public override void OnStartClient()
	{
		base.OnStartClient();
		if (base.isOwned || (!base.isOwned && base.isServer && base.connectionToClient == null))
		{
			this.Rb.interpolation = RigidbodyInterpolation.Interpolate;
			return;
		}
		this.Rb.interpolation = RigidbodyInterpolation.None;
	}

	// Token: 0x06000812 RID: 2066 RVA: 0x000209FC File Offset: 0x0001EBFC
	protected override void OnDestroy()
	{
		base.OnDestroy();
		this.modelTransform.DOKill(false);
	}

	// Token: 0x06000813 RID: 2067 RVA: 0x000048A7 File Offset: 0x00002AA7
	protected virtual void OnHolderChanged(PlayerInventory oldHolder, PlayerInventory newHolder)
	{
	}

	// Token: 0x06000814 RID: 2068 RVA: 0x00020A11 File Offset: 0x0001EC11
	private void TryUseItem(bool isPressed)
	{
		if (!this.NetworkHolder)
		{
			return;
		}
		if (!this.NetworkHolder.isLocalPlayer)
		{
			return;
		}
		if (this.isInPocket)
		{
			return;
		}
		this.OnUseItem(isPressed);
		this.CmdUseItem(isPressed);
	}

	// Token: 0x06000815 RID: 2069 RVA: 0x00020A48 File Offset: 0x0001EC48
	public override void OnInteract(PlayerInteract playerInteract)
	{
		base.OnInteract(playerInteract);
		if (this.NetworkHolder)
		{
			return;
		}
		PlayerInventory playerInventory;
		if (!playerInteract.TryGetComponent<PlayerInventory>(out playerInventory))
		{
			return;
		}
		this.OnLocalPickUp();
	}

	// Token: 0x06000816 RID: 2070 RVA: 0x00020A7C File Offset: 0x0001EC7C
	[Server]
	public override void ServerOnInteract(PlayerInteract playerInteract)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void Item::ServerOnInteract(PlayerInteract)' called when server was not active");
			return;
		}
		base.ServerOnInteract(playerInteract);
		if (this.NetworkHolder)
		{
			return;
		}
		PlayerInventory playerInventory;
		if (!playerInteract.TryGetComponent<PlayerInventory>(out playerInventory))
		{
			return;
		}
		this.ServerPickup(playerInventory);
	}

	// Token: 0x06000817 RID: 2071 RVA: 0x00020AC8 File Offset: 0x0001ECC8
	[Server]
	public virtual void ServerThrow(Vector3 position, Quaternion rotation, Vector3 velocity, Vector3 angularVelocity)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void Item::ServerThrow(UnityEngine.Vector3,UnityEngine.Quaternion,UnityEngine.Vector3,UnityEngine.Vector3)' called when server was not active");
			return;
		}
		if (this.NetworkHolder)
		{
			this.ServerDrop();
		}
		this.Rb.Teleport(position, false);
		this.Rb.Rotate(rotation, false);
		this.Rb.linearVelocity = velocity;
		this.Rb.angularVelocity = angularVelocity;
		base.StartCoroutine(this.DelayFixedUpdate(velocity));
	}

	// Token: 0x06000818 RID: 2072 RVA: 0x00020B3E File Offset: 0x0001ED3E
	private IEnumerator DelayFixedUpdate(Vector3 velocity)
	{
		yield return new WaitForFixedUpdate();
		this.RpcOnThrow(velocity);
		yield break;
	}

	// Token: 0x06000819 RID: 2073 RVA: 0x00020B54 File Offset: 0x0001ED54
	[ClientRpc]
	private void RpcOnThrow(Vector3 velocity)
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		writer.WriteVector3(velocity);
		this.SendRPCInternal("System.Void Item::RpcOnThrow(UnityEngine.Vector3)", -368834569, writer, 0, true);
		NetworkWriterPool.Return(writer);
	}

	// Token: 0x0600081A RID: 2074 RVA: 0x00020B8E File Offset: 0x0001ED8E
	[Server]
	public virtual void ServerTeleport(Vector3 position)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void Item::ServerTeleport(UnityEngine.Vector3)' called when server was not active");
			return;
		}
		if (this.NetworkHolder)
		{
			return;
		}
		this.Rb.Teleport(position, true);
	}

	// Token: 0x0600081B RID: 2075 RVA: 0x00020BC0 File Offset: 0x0001EDC0
	[Server]
	public virtual void ServerRotate(Quaternion rotation)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void Item::ServerRotate(UnityEngine.Quaternion)' called when server was not active");
			return;
		}
		if (this.NetworkHolder)
		{
			return;
		}
		this.Rb.Rotate(rotation, true);
	}

	// Token: 0x0600081C RID: 2076 RVA: 0x00020BF2 File Offset: 0x0001EDF2
	[Server]
	public void ServerSetEnabled(bool isEnabled)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void Item::ServerSetEnabled(System.Boolean)' called when server was not active");
			return;
		}
		if (!isEnabled)
		{
			this.ServerDrop();
		}
		this.RpcSetEnabled(isEnabled);
	}

	// Token: 0x0600081D RID: 2077 RVA: 0x00020C1C File Offset: 0x0001EE1C
	[ClientRpc]
	private void RpcSetEnabled(bool isEnabled)
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		writer.WriteBool(isEnabled);
		this.SendRPCInternal("System.Void Item::RpcSetEnabled(System.Boolean)", 1610715325, writer, 0, true);
		NetworkWriterPool.Return(writer);
	}

	// Token: 0x0600081E RID: 2078 RVA: 0x00020C56 File Offset: 0x0001EE56
	public bool GetIsBeingHeld()
	{
		return this.NetworkHolder;
	}

	// Token: 0x0600081F RID: 2079 RVA: 0x00020C64 File Offset: 0x0001EE64
	[Server]
	private void ServerPickup(PlayerInventory playerInventory)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void Item::ServerPickup(PlayerInventory)' called when server was not active");
			return;
		}
		if (this.NetworkHolder)
		{
			return;
		}
		this.NetworkHolder = playerInventory;
		if (playerInventory != null)
		{
			playerInventory.ServerAddItem(this);
		}
		this.SetPickedUp(playerInventory);
		this.RpcPickup(playerInventory);
		this.OnPickedUp(playerInventory);
		this.ServerHandEnter(playerInventory);
	}

	// Token: 0x06000820 RID: 2080 RVA: 0x00020CC4 File Offset: 0x0001EEC4
	[ClientRpc]
	private void RpcPickup(PlayerInventory playerInventory)
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		writer.WriteNetworkBehaviour(playerInventory);
		this.SendRPCInternal("System.Void Item::RpcPickup(PlayerInventory)", 1550294340, writer, 0, true);
		NetworkWriterPool.Return(writer);
	}

	// Token: 0x06000821 RID: 2081 RVA: 0x00020D00 File Offset: 0x0001EF00
	private void SetPickedUp(PlayerInventory playerInventory)
	{
		this.Rb.interpolation = RigidbodyInterpolation.None;
		this.Rb.isKinematic = true;
		if (this._nrb)
		{
			this._nrb.enabled = false;
		}
		if (this._ssm)
		{
			this._ssm.enabled = false;
		}
		this.SetEnableColliders(false);
		this.IsInteractable = false;
		base.transform.SetParent(playerInventory.handTransform);
		Color playerColor = this._ls.GetPlayerBySteamId(playerInventory.GetComponent<PlayerProfile>().steamId).playerColor;
		this.handMesh.material.SetColor("_Color", playerColor);
	}

	// Token: 0x06000822 RID: 2082 RVA: 0x00020DA8 File Offset: 0x0001EFA8
	private void OnLocalPickUp()
	{
		this.modelTransform.gameObject.SetActive(false);
	}

	// Token: 0x06000823 RID: 2083 RVA: 0x00020DBC File Offset: 0x0001EFBC
	[Server]
	public void ServerDrop()
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void Item::ServerDrop()' called when server was not active");
			return;
		}
		if (!this.NetworkHolder)
		{
			return;
		}
		PlayerInventory networkHolder = this.NetworkHolder;
		this.NetworkHolder = null;
		networkHolder.ServerRemoveItem(this);
		this.SetDropped(networkHolder);
		this.RpcDrop(networkHolder);
		this.OnDropped(networkHolder);
	}

	// Token: 0x06000824 RID: 2084 RVA: 0x00020E18 File Offset: 0x0001F018
	[ClientRpc]
	private void RpcDrop(PlayerInventory previousHolder)
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		writer.WriteNetworkBehaviour(previousHolder);
		this.SendRPCInternal("System.Void Item::RpcDrop(PlayerInventory)", 251131031, writer, 0, true);
		NetworkWriterPool.Return(writer);
	}

	// Token: 0x06000825 RID: 2085 RVA: 0x00020E54 File Offset: 0x0001F054
	private void SetDropped(PlayerInventory previousHolder)
	{
		this.isInPocket = false;
		base.transform.SetParent(null);
		this.Rb.isKinematic = false;
		if (base.isOwned || (!base.isOwned && base.isServer && base.connectionToClient == null))
		{
			this.Rb.interpolation = RigidbodyInterpolation.Interpolate;
		}
		base.StartCoroutine(this.MassTween());
		if (this._nrb)
		{
			this._nrb.enabled = true;
		}
		if (this._ssm)
		{
			this._ssm.enabled = true;
		}
		if (this._sfxPhysicsObject != null)
		{
			this._sfxPhysicsObject.SetPlayerThrowCooldown();
		}
		this.SetEnableColliders(true);
		this.IsInteractable = true;
		this.modelTransform.localPosition = Vector3.zero;
		this.modelTransform.localRotation = Quaternion.identity;
		this.modelTransform.gameObject.SetActive(true);
		this.handRig.SetActive(false);
		previousHolder.SetPlayerHandsVisible(true);
		if (this.onDropFb)
		{
			this.onDropFb.PlayFeedbacks();
		}
	}

	// Token: 0x06000826 RID: 2086 RVA: 0x00020F6E File Offset: 0x0001F16E
	private IEnumerator MassTween()
	{
		float duration = 0.1f;
		float t = 0f;
		this.Rb.mass = this.Mass / 100f;
		while (t < duration)
		{
			float mass = Mathf.Lerp(this.Mass / 100f, this.Mass, t / duration);
			this.Rb.mass = mass;
			yield return new WaitForFixedUpdate();
			t += Time.fixedDeltaTime;
		}
		this.Rb.mass = this.Mass;
		yield break;
	}

	// Token: 0x06000827 RID: 2087 RVA: 0x00020F7D File Offset: 0x0001F17D
	public void OnLocalDrop()
	{
		if (this.modelTransform)
		{
			this.modelTransform.gameObject.SetActive(false);
		}
	}

	// Token: 0x06000828 RID: 2088 RVA: 0x00020FA0 File Offset: 0x0001F1A0
	[Server]
	public void ServerHandEnter(PlayerInventory playerInventory)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void Item::ServerHandEnter(PlayerInventory)' called when server was not active");
			return;
		}
		if (!this.NetworkHolder)
		{
			return;
		}
		if (this.NetworkHolder != playerInventory)
		{
			return;
		}
		this.SetHandEnter(playerInventory);
		this.RpcHandEnter(playerInventory);
		this.OnHandEnter(playerInventory);
	}

	// Token: 0x06000829 RID: 2089 RVA: 0x00020FF4 File Offset: 0x0001F1F4
	[ClientRpc]
	private void RpcHandEnter(PlayerInventory playerInventory)
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		writer.WriteNetworkBehaviour(playerInventory);
		this.SendRPCInternal("System.Void Item::RpcHandEnter(PlayerInventory)", 275819659, writer, 0, true);
		NetworkWriterPool.Return(writer);
	}

	// Token: 0x0600082A RID: 2090 RVA: 0x00021030 File Offset: 0x0001F230
	private void SetHandEnter(PlayerInventory playerInventory)
	{
		this.isInPocket = false;
		base.transform.localPosition = Vector3.zero;
		base.transform.localRotation = Quaternion.identity;
		this.modelTransform.localPosition = Vector3.zero;
		this.modelTransform.localRotation = Quaternion.identity;
		this.modelTransform.gameObject.SetActive(true);
		playerInventory.SetPlayerHandsVisible(false);
		this.handRig.SetActive(true);
		if (this.onHandFb)
		{
			this.onHandFb.PlayFeedbacks();
		}
	}

	// Token: 0x0600082B RID: 2091 RVA: 0x000210C0 File Offset: 0x0001F2C0
	[Server]
	public void ServerHandExit(PlayerInventory playerInventory)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void Item::ServerHandExit(PlayerInventory)' called when server was not active");
			return;
		}
		if (!this.NetworkHolder)
		{
			return;
		}
		if (this.NetworkHolder != playerInventory)
		{
			return;
		}
		this.SetHandExit(playerInventory);
		this.RpcHandExit(playerInventory);
		this.OnHandExit(playerInventory);
	}

	// Token: 0x0600082C RID: 2092 RVA: 0x00021114 File Offset: 0x0001F314
	[ClientRpc]
	private void RpcHandExit(PlayerInventory playerInventory)
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		writer.WriteNetworkBehaviour(playerInventory);
		this.SendRPCInternal("System.Void Item::RpcHandExit(PlayerInventory)", -558030313, writer, 0, true);
		NetworkWriterPool.Return(writer);
	}

	// Token: 0x0600082D RID: 2093 RVA: 0x0002114E File Offset: 0x0001F34E
	private void SetHandExit(PlayerInventory playerInventory)
	{
		this.isInPocket = true;
		this.modelTransform.gameObject.SetActive(false);
		base.transform.SetParent(playerInventory.pocketTransform);
		this.handRig.SetActive(false);
		playerInventory.SetPlayerHandsVisible(true);
	}

	// Token: 0x0600082E RID: 2094 RVA: 0x000048A7 File Offset: 0x00002AA7
	protected virtual void OnPickedUp(PlayerInventory playerInventory)
	{
	}

	// Token: 0x0600082F RID: 2095 RVA: 0x0002118C File Offset: 0x0001F38C
	[Server]
	protected virtual void OnDropped(PlayerInventory playerInventory)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void Item::OnDropped(PlayerInventory)' called when server was not active");
			return;
		}
	}

	// Token: 0x06000830 RID: 2096 RVA: 0x000211A3 File Offset: 0x0001F3A3
	[Server]
	protected virtual void OnHandEnter(PlayerInventory holder)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void Item::OnHandEnter(PlayerInventory)' called when server was not active");
			return;
		}
	}

	// Token: 0x06000831 RID: 2097 RVA: 0x000211BA File Offset: 0x0001F3BA
	[Server]
	protected virtual void OnHandExit(PlayerInventory holder)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void Item::OnHandExit(PlayerInventory)' called when server was not active");
			return;
		}
	}

	// Token: 0x06000832 RID: 2098 RVA: 0x000211D4 File Offset: 0x0001F3D4
	[Command(requiresAuthority = false)]
	private void CmdUseItem(bool isPressed)
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		writer.WriteBool(isPressed);
		base.SendCommandInternal("System.Void Item::CmdUseItem(System.Boolean)", 738706521, writer, 0, false);
		NetworkWriterPool.Return(writer);
	}

	// Token: 0x06000833 RID: 2099 RVA: 0x00021210 File Offset: 0x0001F410
	[ClientRpc]
	private void RpcUseItem(bool isPressed)
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		writer.WriteBool(isPressed);
		this.SendRPCInternal("System.Void Item::RpcUseItem(System.Boolean)", -234203220, writer, 0, true);
		NetworkWriterPool.Return(writer);
	}

	// Token: 0x06000834 RID: 2100 RVA: 0x000048A7 File Offset: 0x00002AA7
	protected virtual void OnUseItem(bool isPressed)
	{
	}

	// Token: 0x06000835 RID: 2101 RVA: 0x0002124A File Offset: 0x0001F44A
	public Item()
	{
		this._Mirror_SyncVarHookDelegate_Holder = new Action<PlayerInventory, PlayerInventory>(this.OnHolderChanged);
	}

	// Token: 0x06000836 RID: 2102 RVA: 0x00002321 File Offset: 0x00000521
	public override bool Weaved()
	{
		return true;
	}

	// Token: 0x170000BE RID: 190
	// (get) Token: 0x06000837 RID: 2103 RVA: 0x00021270 File Offset: 0x0001F470
	// (set) Token: 0x06000838 RID: 2104 RVA: 0x0002128F File Offset: 0x0001F48F
	public PlayerInventory NetworkHolder
	{
		get
		{
			return base.GetSyncVarNetworkBehaviour<PlayerInventory>(this.___HolderNetId, ref this.Holder);
		}
		[param: In]
		set
		{
			base.GeneratedSyncVarSetter_NetworkBehaviour<PlayerInventory>(value, ref this.Holder, 1UL, this._Mirror_SyncVarHookDelegate_Holder, ref this.___HolderNetId);
		}
	}

	// Token: 0x06000839 RID: 2105 RVA: 0x000212B4 File Offset: 0x0001F4B4
	protected void UserCode_RpcOnThrow__Vector3(Vector3 velocity)
	{
		ParticleSystem.MainModule main = this.onThrowVfx.main;
		ParticleSystem.MinMaxCurve startSpeed = main.startSpeed;
		startSpeed.constantMax = Mathf.Max(velocity.magnitude, startSpeed.constantMin);
		main.startSpeed = startSpeed;
		this.onThrowVfx.Play();
	}

	// Token: 0x0600083A RID: 2106 RVA: 0x00021302 File Offset: 0x0001F502
	protected static void InvokeUserCode_RpcOnThrow__Vector3(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcOnThrow called on server.");
			return;
		}
		((Item)obj).UserCode_RpcOnThrow__Vector3(reader.ReadVector3());
	}

	// Token: 0x0600083B RID: 2107 RVA: 0x0002132B File Offset: 0x0001F52B
	protected void UserCode_RpcSetEnabled__Boolean(bool isEnabled)
	{
		if (base.gameObject.activeSelf == isEnabled)
		{
			return;
		}
		base.gameObject.SetActive(isEnabled);
	}

	// Token: 0x0600083C RID: 2108 RVA: 0x00021348 File Offset: 0x0001F548
	protected static void InvokeUserCode_RpcSetEnabled__Boolean(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcSetEnabled called on server.");
			return;
		}
		((Item)obj).UserCode_RpcSetEnabled__Boolean(reader.ReadBool());
	}

	// Token: 0x0600083D RID: 2109 RVA: 0x00021371 File Offset: 0x0001F571
	protected void UserCode_RpcPickup__PlayerInventory(PlayerInventory playerInventory)
	{
		if (base.isServer)
		{
			return;
		}
		this.SetPickedUp(playerInventory);
	}

	// Token: 0x0600083E RID: 2110 RVA: 0x00021383 File Offset: 0x0001F583
	protected static void InvokeUserCode_RpcPickup__PlayerInventory(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcPickup called on server.");
			return;
		}
		((Item)obj).UserCode_RpcPickup__PlayerInventory(reader.ReadNetworkBehaviour<PlayerInventory>());
	}

	// Token: 0x0600083F RID: 2111 RVA: 0x000213AC File Offset: 0x0001F5AC
	protected void UserCode_RpcDrop__PlayerInventory(PlayerInventory previousHolder)
	{
		if (base.isServer)
		{
			return;
		}
		this.SetDropped(previousHolder);
	}

	// Token: 0x06000840 RID: 2112 RVA: 0x000213BE File Offset: 0x0001F5BE
	protected static void InvokeUserCode_RpcDrop__PlayerInventory(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcDrop called on server.");
			return;
		}
		((Item)obj).UserCode_RpcDrop__PlayerInventory(reader.ReadNetworkBehaviour<PlayerInventory>());
	}

	// Token: 0x06000841 RID: 2113 RVA: 0x000213E7 File Offset: 0x0001F5E7
	protected void UserCode_RpcHandEnter__PlayerInventory(PlayerInventory playerInventory)
	{
		if (base.isServer)
		{
			return;
		}
		this.SetHandEnter(playerInventory);
	}

	// Token: 0x06000842 RID: 2114 RVA: 0x000213F9 File Offset: 0x0001F5F9
	protected static void InvokeUserCode_RpcHandEnter__PlayerInventory(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcHandEnter called on server.");
			return;
		}
		((Item)obj).UserCode_RpcHandEnter__PlayerInventory(reader.ReadNetworkBehaviour<PlayerInventory>());
	}

	// Token: 0x06000843 RID: 2115 RVA: 0x00021422 File Offset: 0x0001F622
	protected void UserCode_RpcHandExit__PlayerInventory(PlayerInventory playerInventory)
	{
		if (base.isServer)
		{
			return;
		}
		this.SetHandExit(playerInventory);
	}

	// Token: 0x06000844 RID: 2116 RVA: 0x00021434 File Offset: 0x0001F634
	protected static void InvokeUserCode_RpcHandExit__PlayerInventory(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcHandExit called on server.");
			return;
		}
		((Item)obj).UserCode_RpcHandExit__PlayerInventory(reader.ReadNetworkBehaviour<PlayerInventory>());
	}

	// Token: 0x06000845 RID: 2117 RVA: 0x0002145D File Offset: 0x0001F65D
	protected void UserCode_CmdUseItem__Boolean(bool isPressed)
	{
		this.RpcUseItem(isPressed);
	}

	// Token: 0x06000846 RID: 2118 RVA: 0x00021466 File Offset: 0x0001F666
	protected static void InvokeUserCode_CmdUseItem__Boolean(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdUseItem called on client.");
			return;
		}
		((Item)obj).UserCode_CmdUseItem__Boolean(reader.ReadBool());
	}

	// Token: 0x06000847 RID: 2119 RVA: 0x0002148F File Offset: 0x0001F68F
	protected void UserCode_RpcUseItem__Boolean(bool isPressed)
	{
		if (!this.NetworkHolder)
		{
			return;
		}
		if (this.NetworkHolder.isLocalPlayer)
		{
			return;
		}
		this.OnUseItem(isPressed);
	}

	// Token: 0x06000848 RID: 2120 RVA: 0x000214B4 File Offset: 0x0001F6B4
	protected static void InvokeUserCode_RpcUseItem__Boolean(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcUseItem called on server.");
			return;
		}
		((Item)obj).UserCode_RpcUseItem__Boolean(reader.ReadBool());
	}

	// Token: 0x06000849 RID: 2121 RVA: 0x000214E0 File Offset: 0x0001F6E0
	static Item()
	{
		RemoteProcedureCalls.RegisterCommand(typeof(Item), "System.Void Item::CmdUseItem(System.Boolean)", new RemoteCallDelegate(Item.InvokeUserCode_CmdUseItem__Boolean), false);
		RemoteProcedureCalls.RegisterRpc(typeof(Item), "System.Void Item::RpcOnThrow(UnityEngine.Vector3)", new RemoteCallDelegate(Item.InvokeUserCode_RpcOnThrow__Vector3));
		RemoteProcedureCalls.RegisterRpc(typeof(Item), "System.Void Item::RpcSetEnabled(System.Boolean)", new RemoteCallDelegate(Item.InvokeUserCode_RpcSetEnabled__Boolean));
		RemoteProcedureCalls.RegisterRpc(typeof(Item), "System.Void Item::RpcPickup(PlayerInventory)", new RemoteCallDelegate(Item.InvokeUserCode_RpcPickup__PlayerInventory));
		RemoteProcedureCalls.RegisterRpc(typeof(Item), "System.Void Item::RpcDrop(PlayerInventory)", new RemoteCallDelegate(Item.InvokeUserCode_RpcDrop__PlayerInventory));
		RemoteProcedureCalls.RegisterRpc(typeof(Item), "System.Void Item::RpcHandEnter(PlayerInventory)", new RemoteCallDelegate(Item.InvokeUserCode_RpcHandEnter__PlayerInventory));
		RemoteProcedureCalls.RegisterRpc(typeof(Item), "System.Void Item::RpcHandExit(PlayerInventory)", new RemoteCallDelegate(Item.InvokeUserCode_RpcHandExit__PlayerInventory));
		RemoteProcedureCalls.RegisterRpc(typeof(Item), "System.Void Item::RpcUseItem(System.Boolean)", new RemoteCallDelegate(Item.InvokeUserCode_RpcUseItem__Boolean));
	}

	// Token: 0x0600084A RID: 2122 RVA: 0x000215F0 File Offset: 0x0001F7F0
	public override void SerializeSyncVars(NetworkWriter writer, bool forceAll)
	{
		base.SerializeSyncVars(writer, forceAll);
		if (forceAll)
		{
			writer.WriteNetworkBehaviour(this.NetworkHolder);
			return;
		}
		writer.WriteVarULong(this.syncVarDirtyBits);
		if ((this.syncVarDirtyBits & 1UL) != 0UL)
		{
			writer.WriteNetworkBehaviour(this.NetworkHolder);
		}
	}

	// Token: 0x0600084B RID: 2123 RVA: 0x00021648 File Offset: 0x0001F848
	public override void DeserializeSyncVars(NetworkReader reader, bool initialState)
	{
		base.DeserializeSyncVars(reader, initialState);
		if (initialState)
		{
			base.GeneratedSyncVarDeserialize_NetworkBehaviour<PlayerInventory>(ref this.Holder, this._Mirror_SyncVarHookDelegate_Holder, reader, ref this.___HolderNetId);
			return;
		}
		long num = (long)reader.ReadVarULong();
		if ((num & 1L) != 0L)
		{
			base.GeneratedSyncVarDeserialize_NetworkBehaviour<PlayerInventory>(ref this.Holder, this._Mirror_SyncVarHookDelegate_Holder, reader, ref this.___HolderNetId);
		}
	}

	// Token: 0x04000536 RID: 1334
	[Header("References")]
	public SpawnableSO spawnableSo;

	// Token: 0x04000537 RID: 1335
	public Transform modelTransform;

	// Token: 0x04000538 RID: 1336
	public GameObject handRig;

	// Token: 0x04000539 RID: 1337
	public SkinnedMeshRenderer handMesh;

	// Token: 0x0400053A RID: 1338
	public MMF_Player onHandFb;

	// Token: 0x0400053B RID: 1339
	public MMF_Player onDropFb;

	// Token: 0x0400053C RID: 1340
	public ParticleSystem onThrowVfx;

	// Token: 0x0400053D RID: 1341
	[SyncVar(hook = "OnHolderChanged")]
	[CanBeNull]
	protected PlayerInventory Holder;

	// Token: 0x0400053E RID: 1342
	[HideInInspector]
	public bool isInPocket;

	// Token: 0x0400053F RID: 1343
	[Header("Item Actions")]
	public List<ItemAction> itemActions;

	// Token: 0x04000540 RID: 1344
	protected Rigidbody Rb;

	// Token: 0x04000541 RID: 1345
	private List<Collider> _allColliders = new List<Collider>();

	// Token: 0x04000542 RID: 1346
	private NetworkRigidbodyUnreliable _nrb;

	// Token: 0x04000543 RID: 1347
	private SmoothSyncMirror _ssm;

	// Token: 0x04000544 RID: 1348
	private SFXPhysicsObject _sfxPhysicsObject;

	// Token: 0x04000545 RID: 1349
	private LobbySettings _ls;

	// Token: 0x04000547 RID: 1351
	[Range(0f, 1f)]
	public float slowPercent;

	// Token: 0x04000548 RID: 1352
	protected NetworkBehaviourSyncVar ___HolderNetId;

	// Token: 0x04000549 RID: 1353
	public Action<PlayerInventory, PlayerInventory> _Mirror_SyncVarHookDelegate_Holder;
}
