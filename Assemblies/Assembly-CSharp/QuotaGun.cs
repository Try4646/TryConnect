using System;
using System.Collections.Generic;
using Extensions;
using FMODUnity;
using Mirror;
using Mirror.RemoteCalls;
using UnityEngine;

// Token: 0x020000F9 RID: 249
public class QuotaGun : ConsumableItem
{
	// Token: 0x06000A0F RID: 2575 RVA: 0x0002830E File Offset: 0x0002650E
	private void Start()
	{
		this._defaultIndicatorColor = this.organIndicator[0].material.GetColor("_EmissionColor");
	}

	// Token: 0x06000A10 RID: 2576 RVA: 0x00028334 File Offset: 0x00026534
	protected override void OnUseItem(bool isPressed)
	{
		base.OnUseItem(isPressed);
		if (!isPressed)
		{
			return;
		}
		if (!base.NetworkHolder || !base.NetworkHolder.isLocalPlayer)
		{
			return;
		}
		if (Time.time - this._lastShootTime < this.shootCooldown)
		{
			return;
		}
		this._lastShootTime = Time.time;
		this.Shoot();
	}

	// Token: 0x06000A11 RID: 2577 RVA: 0x0002838D File Offset: 0x0002658D
	protected override void OnDropped(PlayerInventory playerInventory)
	{
		base.OnDropped(playerInventory);
		this.RpcOnDropped();
	}

	// Token: 0x06000A12 RID: 2578 RVA: 0x0002839C File Offset: 0x0002659C
	[ClientRpc]
	private void RpcOnDropped()
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		this.SendRPCInternal("System.Void QuotaGun::RpcOnDropped()", -12772959, writer, 0, true);
		NetworkWriterPool.Return(writer);
	}

	// Token: 0x06000A13 RID: 2579 RVA: 0x000283CC File Offset: 0x000265CC
	private void Shoot()
	{
		Camera mainCamera = MonoSingleton<LocalManager>.Instance.mainCamera;
		Vector3 position = mainCamera.transform.position;
		Vector3 forward = mainCamera.transform.forward;
		int num = Physics.RaycastNonAlloc(new Ray(position, forward), this._raycastHits, this.raycastDistance, this.rayMask, QueryTriggerInteraction.Ignore);
		Vector3 vector = position + forward * this.raycastDistance;
		if (num > 0)
		{
			RaycastHit raycastHit = this._raycastHits[0];
			for (int i = 1; i < num; i++)
			{
				if (this._raycastHits[i].distance < raycastHit.distance)
				{
					raycastHit = this._raycastHits[i];
				}
			}
			bool hitPlayer = false;
			vector = raycastHit.point;
			Rigidbody attachedRigidbody = raycastHit.collider.attachedRigidbody;
			if (attachedRigidbody)
			{
				PlayerController pc;
				NPC npc;
				Item item;
				if (attachedRigidbody.TryGetComponent<PlayerController>(out pc))
				{
					this.CmdShootPlayer(pc, forward, vector);
					hitPlayer = true;
				}
				else if (attachedRigidbody.TryGetComponent<NPC>(out npc))
				{
					this.CmdShootNpc(npc, forward, vector);
				}
				else if (attachedRigidbody.TryGetComponent<Item>(out item))
				{
					this.CmdShootItem(item, forward, vector);
				}
			}
			this.PlayHitEffects(hitPlayer, vector);
			this.CmdPlayHitEffects(hitPlayer, vector);
		}
		this.PlayShootEffects(vector);
		this.CmdPlayShootEffects(vector);
	}

	// Token: 0x06000A14 RID: 2580 RVA: 0x0002850C File Offset: 0x0002670C
	private Vector3 CalculateKnockbackVector(Vector3 direction)
	{
		return direction * this.power + Vector3.up * this.upPower + Vector3.Cross(direction, Vector3.up) * Random.Range(-1f, 1f) * this.randomPower;
	}

	// Token: 0x06000A15 RID: 2581 RVA: 0x00028569 File Offset: 0x00026769
	private Vector3 CalculateTorque(Vector3 position, Vector3 centerOfMass, Vector3 direction)
	{
		return Vector3.Cross(Vector3.ClampMagnitude(position - centerOfMass, 1f), direction) * this.torquePower;
	}

	// Token: 0x06000A16 RID: 2582 RVA: 0x00028590 File Offset: 0x00026790
	[Command(requiresAuthority = false)]
	private void CmdShootPlayer(PlayerController pc, Vector3 dir, Vector3 pos)
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		writer.WriteNetworkBehaviour(pc);
		writer.WriteVector3(dir);
		writer.WriteVector3(pos);
		base.SendCommandInternal("System.Void QuotaGun::CmdShootPlayer(PlayerController,UnityEngine.Vector3,UnityEngine.Vector3)", 182095588, writer, 0, false);
		NetworkWriterPool.Return(writer);
	}

	// Token: 0x06000A17 RID: 2583 RVA: 0x000285E0 File Offset: 0x000267E0
	[Command(requiresAuthority = false)]
	private void CmdShootNpc(NPC npc, Vector3 dir, Vector3 pos)
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		writer.WriteNetworkBehaviour(npc);
		writer.WriteVector3(dir);
		writer.WriteVector3(pos);
		base.SendCommandInternal("System.Void QuotaGun::CmdShootNpc(NPC,UnityEngine.Vector3,UnityEngine.Vector3)", -1272076522, writer, 0, false);
		NetworkWriterPool.Return(writer);
	}

	// Token: 0x06000A18 RID: 2584 RVA: 0x00028630 File Offset: 0x00026830
	[Command(requiresAuthority = false)]
	private void CmdShootItem(Item item, Vector3 dir, Vector3 pos)
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		writer.WriteNetworkBehaviour(item);
		writer.WriteVector3(dir);
		writer.WriteVector3(pos);
		base.SendCommandInternal("System.Void QuotaGun::CmdShootItem(Item,UnityEngine.Vector3,UnityEngine.Vector3)", 534524124, writer, 0, false);
		NetworkWriterPool.Return(writer);
	}

	// Token: 0x06000A19 RID: 2585 RVA: 0x00028680 File Offset: 0x00026880
	private void TryRemoveRandomOrgan(PlayerController pc)
	{
		if (this._removedOrganCount >= this.removableOrganCount)
		{
			return;
		}
		if (NetworkSingleton<GameManager>.Instance.state != GameState.Game)
		{
			return;
		}
		if (NetworkSingleton<MoneyManager>.Instance.balance >= NetworkSingleton<GameManager>.Instance.currentQuota)
		{
			return;
		}
		PlayerOrgans component = pc.GetComponent<PlayerOrgans>();
		PlayerOrganData organData = NetworkSingleton<OrganManager>.Instance.GetOrganData(component);
		if (organData == null)
		{
			return;
		}
		List<OrganType> list = new List<OrganType>();
		if (organData.body)
		{
			list.Add(OrganType.Body);
		}
		if (organData.mouth)
		{
			list.Add(OrganType.Mouth);
		}
		if (organData.leftEye && organData.rightEye)
		{
			list.Add((Random.Range(0f, 1f) > 0.5f) ? OrganType.LeftEye : OrganType.RightEye);
		}
		if (list.Count <= 0)
		{
			return;
		}
		NetworkSingleton<OrganManager>.Instance.ServerToggleOrgan(component, list.GetRandomElement<OrganType>(), false);
		this._removedOrganCount++;
		this.RpcSetRemovedOrganIndicator(this._removedOrganCount);
		this.AwardQuotaFraction();
	}

	// Token: 0x06000A1A RID: 2586 RVA: 0x00028768 File Offset: 0x00026968
	[Server]
	private void AwardQuotaFraction()
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void QuotaGun::AwardQuotaFraction()' called when server was not active");
			return;
		}
		long currentQuota = NetworkSingleton<GameManager>.Instance.currentQuota;
		long num = (long)Math.Ceiling((double)currentQuota / (double)this.removableOrganCount);
		long num2 = currentQuota - NetworkSingleton<MoneyManager>.Instance.balance;
		if (num > num2)
		{
			num = num2;
		}
		if (num <= 0L)
		{
			return;
		}
		NetworkSingleton<MoneyManager>.Instance.TryChangeBalance(num, null, ChangeType.Item);
	}

	// Token: 0x06000A1B RID: 2587 RVA: 0x000287CC File Offset: 0x000269CC
	[ClientRpc]
	private void RpcSetRemovedOrganIndicator(int removedOrganCount)
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		writer.WriteVarInt(removedOrganCount);
		this.SendRPCInternal("System.Void QuotaGun::RpcSetRemovedOrganIndicator(System.Int32)", 1515992013, writer, 0, true);
		NetworkWriterPool.Return(writer);
	}

	// Token: 0x06000A1C RID: 2588 RVA: 0x00028808 File Offset: 0x00026A08
	[Command(requiresAuthority = false)]
	private void CmdPlayShootEffects(Vector3 hitPos)
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		writer.WriteVector3(hitPos);
		base.SendCommandInternal("System.Void QuotaGun::CmdPlayShootEffects(UnityEngine.Vector3)", -562226375, writer, 0, false);
		NetworkWriterPool.Return(writer);
	}

	// Token: 0x06000A1D RID: 2589 RVA: 0x00028844 File Offset: 0x00026A44
	[ClientRpc]
	private void RpcPlayShootEffects(Vector3 hitPos)
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		writer.WriteVector3(hitPos);
		this.SendRPCInternal("System.Void QuotaGun::RpcPlayShootEffects(UnityEngine.Vector3)", 1557493596, writer, 0, true);
		NetworkWriterPool.Return(writer);
	}

	// Token: 0x06000A1E RID: 2590 RVA: 0x00028880 File Offset: 0x00026A80
	private void PlayShootEffects(Vector3 hitPos)
	{
		this.anim.Play("Shoot", 0, 0f);
		this.muzzleVFX.Play();
		LineRenderer lineRenderer = Object.Instantiate<LineRenderer>(this.lineRenderer);
		lineRenderer.SetPosition(0, this.muzzleVFX.transform.position);
		lineRenderer.SetPosition(1, hitPos);
		Object.Destroy(lineRenderer.gameObject, 0.05f);
		SFXManager.SFXOneShot3DAttached(this.shootSfx, base.gameObject, false);
	}

	// Token: 0x06000A1F RID: 2591 RVA: 0x000288FC File Offset: 0x00026AFC
	[Command(requiresAuthority = false)]
	private void CmdPlayHitEffects(bool hitPlayer, Vector3 hitPos)
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		writer.WriteBool(hitPlayer);
		writer.WriteVector3(hitPos);
		base.SendCommandInternal("System.Void QuotaGun::CmdPlayHitEffects(System.Boolean,UnityEngine.Vector3)", 960346420, writer, 0, false);
		NetworkWriterPool.Return(writer);
	}

	// Token: 0x06000A20 RID: 2592 RVA: 0x00028940 File Offset: 0x00026B40
	[ClientRpc]
	private void RpcPlayHitEffects(bool hitPlayer, Vector3 hitPos)
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		writer.WriteBool(hitPlayer);
		writer.WriteVector3(hitPos);
		this.SendRPCInternal("System.Void QuotaGun::RpcPlayHitEffects(System.Boolean,UnityEngine.Vector3)", -584027577, writer, 0, true);
		NetworkWriterPool.Return(writer);
	}

	// Token: 0x06000A21 RID: 2593 RVA: 0x00028984 File Offset: 0x00026B84
	private void PlayHitEffects(bool hitPlayer, Vector3 hitPos)
	{
		Object.Destroy(Object.Instantiate<GameObject>(this.hitVFX, hitPos, Quaternion.identity), 1f);
		SFXManager.SFXOneShot(hitPlayer ? this.hitPlayerSfx : this.hitRandomSfx, hitPos);
	}

	// Token: 0x06000A23 RID: 2595 RVA: 0x00002321 File Offset: 0x00000521
	public override bool Weaved()
	{
		return true;
	}

	// Token: 0x06000A24 RID: 2596 RVA: 0x00028A20 File Offset: 0x00026C20
	protected void UserCode_RpcOnDropped()
	{
		this.anim.Play("Default", 0, 0f);
		this.anim.Update(0f);
	}

	// Token: 0x06000A25 RID: 2597 RVA: 0x00028A48 File Offset: 0x00026C48
	protected static void InvokeUserCode_RpcOnDropped(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcOnDropped called on server.");
			return;
		}
		((QuotaGun)obj).UserCode_RpcOnDropped();
	}

	// Token: 0x06000A26 RID: 2598 RVA: 0x00028A6C File Offset: 0x00026C6C
	protected void UserCode_CmdShootPlayer__PlayerController__Vector3__Vector3(PlayerController pc, Vector3 dir, Vector3 pos)
	{
		this.TryRemoveRandomOrgan(pc);
		Rigidbody component = pc.GetComponent<Rigidbody>();
		Vector3 force = this.CalculateKnockbackVector(dir);
		Vector3 torque = this.CalculateTorque(pos, component.worldCenterOfMass, dir);
		pc.ServerKnockback(force, torque);
	}

	// Token: 0x06000A27 RID: 2599 RVA: 0x00028AA6 File Offset: 0x00026CA6
	protected static void InvokeUserCode_CmdShootPlayer__PlayerController__Vector3__Vector3(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdShootPlayer called on client.");
			return;
		}
		((QuotaGun)obj).UserCode_CmdShootPlayer__PlayerController__Vector3__Vector3(reader.ReadNetworkBehaviour<PlayerController>(), reader.ReadVector3(), reader.ReadVector3());
	}

	// Token: 0x06000A28 RID: 2600 RVA: 0x00028ADC File Offset: 0x00026CDC
	protected void UserCode_CmdShootNpc__NPC__Vector3__Vector3(NPC npc, Vector3 dir, Vector3 pos)
	{
		Rigidbody component = npc.GetComponent<Rigidbody>();
		Vector3 force = this.CalculateKnockbackVector(dir);
		Vector3 torque = this.CalculateTorque(pos, component.worldCenterOfMass, dir);
		npc.ServerKnockback(force, torque);
	}

	// Token: 0x06000A29 RID: 2601 RVA: 0x00028B0F File Offset: 0x00026D0F
	protected static void InvokeUserCode_CmdShootNpc__NPC__Vector3__Vector3(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdShootNpc called on client.");
			return;
		}
		((QuotaGun)obj).UserCode_CmdShootNpc__NPC__Vector3__Vector3(reader.ReadNetworkBehaviour<NPC>(), reader.ReadVector3(), reader.ReadVector3());
	}

	// Token: 0x06000A2A RID: 2602 RVA: 0x00028B44 File Offset: 0x00026D44
	protected void UserCode_CmdShootItem__Item__Vector3__Vector3(Item item, Vector3 dir, Vector3 pos)
	{
		Rigidbody component = item.GetComponent<Rigidbody>();
		Vector3 force = this.CalculateKnockbackVector(dir);
		Vector3 torque = this.CalculateTorque(pos, component.worldCenterOfMass, dir);
		component.AddForce(force, ForceMode.Impulse);
		component.AddTorque(torque, ForceMode.Impulse);
	}

	// Token: 0x06000A2B RID: 2603 RVA: 0x00028B7F File Offset: 0x00026D7F
	protected static void InvokeUserCode_CmdShootItem__Item__Vector3__Vector3(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdShootItem called on client.");
			return;
		}
		((QuotaGun)obj).UserCode_CmdShootItem__Item__Vector3__Vector3(reader.ReadNetworkBehaviour<Item>(), reader.ReadVector3(), reader.ReadVector3());
	}

	// Token: 0x06000A2C RID: 2604 RVA: 0x00028BB4 File Offset: 0x00026DB4
	protected void UserCode_RpcSetRemovedOrganIndicator__Int32(int removedOrganCount)
	{
		for (int i = 0; i < this.organIndicator.Count; i++)
		{
			MeshRenderer meshRenderer = this.organIndicator[i];
			if (i < removedOrganCount)
			{
				meshRenderer.material.SetColor("_EmissionColor", Color.gold * 2f);
			}
			else
			{
				meshRenderer.material.SetColor("_EmissionColor", this._defaultIndicatorColor);
			}
		}
	}

	// Token: 0x06000A2D RID: 2605 RVA: 0x00028C1F File Offset: 0x00026E1F
	protected static void InvokeUserCode_RpcSetRemovedOrganIndicator__Int32(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcSetRemovedOrganIndicator called on server.");
			return;
		}
		((QuotaGun)obj).UserCode_RpcSetRemovedOrganIndicator__Int32(reader.ReadVarInt());
	}

	// Token: 0x06000A2E RID: 2606 RVA: 0x00028C48 File Offset: 0x00026E48
	protected void UserCode_CmdPlayShootEffects__Vector3(Vector3 hitPos)
	{
		this.RpcPlayShootEffects(hitPos);
	}

	// Token: 0x06000A2F RID: 2607 RVA: 0x00028C51 File Offset: 0x00026E51
	protected static void InvokeUserCode_CmdPlayShootEffects__Vector3(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdPlayShootEffects called on client.");
			return;
		}
		((QuotaGun)obj).UserCode_CmdPlayShootEffects__Vector3(reader.ReadVector3());
	}

	// Token: 0x06000A30 RID: 2608 RVA: 0x00028C7A File Offset: 0x00026E7A
	protected void UserCode_RpcPlayShootEffects__Vector3(Vector3 hitPos)
	{
		if (!base.NetworkHolder || base.NetworkHolder.isLocalPlayer)
		{
			return;
		}
		this.PlayShootEffects(hitPos);
	}

	// Token: 0x06000A31 RID: 2609 RVA: 0x00028C9E File Offset: 0x00026E9E
	protected static void InvokeUserCode_RpcPlayShootEffects__Vector3(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcPlayShootEffects called on server.");
			return;
		}
		((QuotaGun)obj).UserCode_RpcPlayShootEffects__Vector3(reader.ReadVector3());
	}

	// Token: 0x06000A32 RID: 2610 RVA: 0x00028CC7 File Offset: 0x00026EC7
	protected void UserCode_CmdPlayHitEffects__Boolean__Vector3(bool hitPlayer, Vector3 hitPos)
	{
		this.RpcPlayHitEffects(hitPlayer, hitPos);
	}

	// Token: 0x06000A33 RID: 2611 RVA: 0x00028CD1 File Offset: 0x00026ED1
	protected static void InvokeUserCode_CmdPlayHitEffects__Boolean__Vector3(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdPlayHitEffects called on client.");
			return;
		}
		((QuotaGun)obj).UserCode_CmdPlayHitEffects__Boolean__Vector3(reader.ReadBool(), reader.ReadVector3());
	}

	// Token: 0x06000A34 RID: 2612 RVA: 0x00028D00 File Offset: 0x00026F00
	protected void UserCode_RpcPlayHitEffects__Boolean__Vector3(bool hitPlayer, Vector3 hitPos)
	{
		if (!base.NetworkHolder || base.NetworkHolder.isLocalPlayer)
		{
			return;
		}
		this.PlayHitEffects(hitPlayer, hitPos);
	}

	// Token: 0x06000A35 RID: 2613 RVA: 0x00028D25 File Offset: 0x00026F25
	protected static void InvokeUserCode_RpcPlayHitEffects__Boolean__Vector3(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcPlayHitEffects called on server.");
			return;
		}
		((QuotaGun)obj).UserCode_RpcPlayHitEffects__Boolean__Vector3(reader.ReadBool(), reader.ReadVector3());
	}

	// Token: 0x06000A36 RID: 2614 RVA: 0x00028D54 File Offset: 0x00026F54
	static QuotaGun()
	{
		RemoteProcedureCalls.RegisterCommand(typeof(QuotaGun), "System.Void QuotaGun::CmdShootPlayer(PlayerController,UnityEngine.Vector3,UnityEngine.Vector3)", new RemoteCallDelegate(QuotaGun.InvokeUserCode_CmdShootPlayer__PlayerController__Vector3__Vector3), false);
		RemoteProcedureCalls.RegisterCommand(typeof(QuotaGun), "System.Void QuotaGun::CmdShootNpc(NPC,UnityEngine.Vector3,UnityEngine.Vector3)", new RemoteCallDelegate(QuotaGun.InvokeUserCode_CmdShootNpc__NPC__Vector3__Vector3), false);
		RemoteProcedureCalls.RegisterCommand(typeof(QuotaGun), "System.Void QuotaGun::CmdShootItem(Item,UnityEngine.Vector3,UnityEngine.Vector3)", new RemoteCallDelegate(QuotaGun.InvokeUserCode_CmdShootItem__Item__Vector3__Vector3), false);
		RemoteProcedureCalls.RegisterCommand(typeof(QuotaGun), "System.Void QuotaGun::CmdPlayShootEffects(UnityEngine.Vector3)", new RemoteCallDelegate(QuotaGun.InvokeUserCode_CmdPlayShootEffects__Vector3), false);
		RemoteProcedureCalls.RegisterCommand(typeof(QuotaGun), "System.Void QuotaGun::CmdPlayHitEffects(System.Boolean,UnityEngine.Vector3)", new RemoteCallDelegate(QuotaGun.InvokeUserCode_CmdPlayHitEffects__Boolean__Vector3), false);
		RemoteProcedureCalls.RegisterRpc(typeof(QuotaGun), "System.Void QuotaGun::RpcOnDropped()", new RemoteCallDelegate(QuotaGun.InvokeUserCode_RpcOnDropped));
		RemoteProcedureCalls.RegisterRpc(typeof(QuotaGun), "System.Void QuotaGun::RpcSetRemovedOrganIndicator(System.Int32)", new RemoteCallDelegate(QuotaGun.InvokeUserCode_RpcSetRemovedOrganIndicator__Int32));
		RemoteProcedureCalls.RegisterRpc(typeof(QuotaGun), "System.Void QuotaGun::RpcPlayShootEffects(UnityEngine.Vector3)", new RemoteCallDelegate(QuotaGun.InvokeUserCode_RpcPlayShootEffects__Vector3));
		RemoteProcedureCalls.RegisterRpc(typeof(QuotaGun), "System.Void QuotaGun::RpcPlayHitEffects(System.Boolean,UnityEngine.Vector3)", new RemoteCallDelegate(QuotaGun.InvokeUserCode_RpcPlayHitEffects__Boolean__Vector3));
	}

	// Token: 0x0400065F RID: 1631
	[Header("References")]
	[SerializeField]
	private GameObject hitVFX;

	// Token: 0x04000660 RID: 1632
	[SerializeField]
	private ParticleSystem muzzleVFX;

	// Token: 0x04000661 RID: 1633
	[SerializeField]
	private LineRenderer lineRenderer;

	// Token: 0x04000662 RID: 1634
	[SerializeField]
	private Animator anim;

	// Token: 0x04000663 RID: 1635
	[SerializeField]
	private List<MeshRenderer> organIndicator;

	// Token: 0x04000664 RID: 1636
	[Header("Settings")]
	[SerializeField]
	private float power = 7f;

	// Token: 0x04000665 RID: 1637
	[SerializeField]
	private float upPower = 7f;

	// Token: 0x04000666 RID: 1638
	[SerializeField]
	private float randomPower = 3f;

	// Token: 0x04000667 RID: 1639
	[SerializeField]
	private float torquePower = 10f;

	// Token: 0x04000668 RID: 1640
	[SerializeField]
	private float shootCooldown = 0.5f;

	// Token: 0x04000669 RID: 1641
	[SerializeField]
	private int removableOrganCount = 3;

	// Token: 0x0400066A RID: 1642
	[SerializeField]
	private float raycastDistance = 50f;

	// Token: 0x0400066B RID: 1643
	[SerializeField]
	private LayerMask rayMask;

	// Token: 0x0400066C RID: 1644
	[Header("SFX")]
	[SerializeField]
	private EventReference shootSfx;

	// Token: 0x0400066D RID: 1645
	[SerializeField]
	private EventReference hitPlayerSfx;

	// Token: 0x0400066E RID: 1646
	[SerializeField]
	private EventReference hitRandomSfx;

	// Token: 0x0400066F RID: 1647
	private readonly RaycastHit[] _raycastHits = new RaycastHit[8];

	// Token: 0x04000670 RID: 1648
	private float _lastShootTime;

	// Token: 0x04000671 RID: 1649
	private int _removedOrganCount;

	// Token: 0x04000672 RID: 1650
	private Color _defaultIndicatorColor;
}
