using System;
using System.Collections;
using Extensions;
using FMODUnity;
using Mirror;
using Mirror.RemoteCalls;
using UnityEngine;

// Token: 0x020000D9 RID: 217
public class Bat : ConsumableItem
{
	// Token: 0x0600087E RID: 2174 RVA: 0x0002246D File Offset: 0x0002066D
	protected override void OnDisable()
	{
		base.OnDisable();
		this._isUsing = false;
	}

	// Token: 0x0600087F RID: 2175 RVA: 0x0002247C File Offset: 0x0002067C
	protected override void OnPickedUp(PlayerInventory playerInventory)
	{
		base.OnPickedUp(playerInventory);
		this.RpcOnPickedUp();
	}

	// Token: 0x06000880 RID: 2176 RVA: 0x0002248C File Offset: 0x0002068C
	[ClientRpc]
	private void RpcOnPickedUp()
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		this.SendRPCInternal("System.Void Bat::RpcOnPickedUp()", -582666199, writer, 0, true);
		NetworkWriterPool.Return(writer);
	}

	// Token: 0x06000881 RID: 2177 RVA: 0x000224BC File Offset: 0x000206BC
	protected override void OnDropped(PlayerInventory playerInventory)
	{
		base.OnDropped(playerInventory);
		this.RpcOnDropped();
	}

	// Token: 0x06000882 RID: 2178 RVA: 0x000224CC File Offset: 0x000206CC
	[ClientRpc]
	private void RpcOnDropped()
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		this.SendRPCInternal("System.Void Bat::RpcOnDropped()", 1719686264, writer, 0, true);
		NetworkWriterPool.Return(writer);
	}

	// Token: 0x06000883 RID: 2179 RVA: 0x000224FC File Offset: 0x000206FC
	protected override void OnUseItem(bool isPressed)
	{
		base.OnUseItem(isPressed);
		if (this._isUsing)
		{
			return;
		}
		this._isUsing = true;
		this.PlayBatSFX(0);
		this.anim.SetTrigger("Hit");
		if (base.NetworkHolder && base.NetworkHolder.isLocalPlayer)
		{
			this.Hit();
		}
		base.StartCoroutine(this.SetIsUsingFalseAfterDelay());
	}

	// Token: 0x06000884 RID: 2180 RVA: 0x00022564 File Offset: 0x00020764
	private IEnumerator SetIsUsingFalseAfterDelay()
	{
		yield return new WaitForSeconds(this.hitDuration);
		this._isUsing = false;
		yield break;
	}

	// Token: 0x06000885 RID: 2181 RVA: 0x00022573 File Offset: 0x00020773
	private void Hit()
	{
		this.hitPoint.enabled = true;
		base.StartCoroutine(this.DisableHitPointAfterDelay());
	}

	// Token: 0x06000886 RID: 2182 RVA: 0x0002258E File Offset: 0x0002078E
	private IEnumerator DisableHitPointAfterDelay()
	{
		yield return new WaitForSeconds(0.15f);
		this.hitPoint.enabled = false;
		yield break;
	}

	// Token: 0x06000887 RID: 2183 RVA: 0x000225A0 File Offset: 0x000207A0
	private void OnTriggerEnter(Collider hit)
	{
		if (!base.NetworkHolder)
		{
			return;
		}
		if (!base.NetworkHolder.isLocalPlayer)
		{
			return;
		}
		if (hit.isTrigger)
		{
			return;
		}
		Vector3 vector = hit.ClosestPointOnBounds(this.hitPoint.transform.position);
		float sqrMagnitude = (vector - this.hitPoint.transform.position).sqrMagnitude;
		float falldown = 1f - Mathf.Clamp(sqrMagnitude, 0f, 0.9f);
		if (!hit.attachedRigidbody)
		{
			this.CmdPlayBatSFX(1);
			this.HitVfx(true, vector);
			return;
		}
		PlayerController playerController;
		NPC npc;
		Item item;
		if (hit.attachedRigidbody.TryGetComponent<PlayerController>(out playerController))
		{
			if (playerController.netId == base.NetworkHolder.netId)
			{
				return;
			}
			this.CmdHitPlayer(playerController, falldown);
			this.CmdPlayBatSFX(2);
		}
		else if (hit.attachedRigidbody.TryGetComponent<NPC>(out npc))
		{
			this.CmdHitNpc(npc, falldown);
			this.CmdPlayBatSFX(2);
		}
		else if (hit.attachedRigidbody.TryGetComponent<Item>(out item))
		{
			this.CmdHitItem(item, falldown);
			this.CmdPlayBatSFX(1);
		}
		this.HitVfx(false, hit.ClosestPointOnBounds(this.hitPoint.transform.position));
	}

	// Token: 0x06000888 RID: 2184 RVA: 0x000226CE File Offset: 0x000208CE
	private void HitVfx(bool isSmall, Vector3 position)
	{
		Object.Destroy(Object.Instantiate<ParticleSystem>(isSmall ? this.hitVfxSmall : this.hitVfx, position, Quaternion.identity).gameObject, 1f);
		this.CmdHitVfx(isSmall, position);
	}

	// Token: 0x06000889 RID: 2185 RVA: 0x00022704 File Offset: 0x00020904
	[Command(requiresAuthority = false)]
	private void CmdHitVfx(bool isSmall, Vector3 position)
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		writer.WriteBool(isSmall);
		writer.WriteVector3(position);
		base.SendCommandInternal("System.Void Bat::CmdHitVfx(System.Boolean,UnityEngine.Vector3)", 1658169067, writer, 0, false);
		NetworkWriterPool.Return(writer);
	}

	// Token: 0x0600088A RID: 2186 RVA: 0x00022748 File Offset: 0x00020948
	[ClientRpc]
	private void RpcHitVfx(bool isSmall, Vector3 position)
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		writer.WriteBool(isSmall);
		writer.WriteVector3(position);
		this.SendRPCInternal("System.Void Bat::RpcHitVfx(System.Boolean,UnityEngine.Vector3)", -1206476476, writer, 0, true);
		NetworkWriterPool.Return(writer);
	}

	// Token: 0x0600088B RID: 2187 RVA: 0x0002278C File Offset: 0x0002098C
	[Command(requiresAuthority = false)]
	private void CmdHitPlayer(PlayerController pc, float falldown)
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		writer.WriteNetworkBehaviour(pc);
		writer.WriteFloat(falldown);
		base.SendCommandInternal("System.Void Bat::CmdHitPlayer(PlayerController,System.Single)", 972876612, writer, 0, false);
		NetworkWriterPool.Return(writer);
	}

	// Token: 0x0600088C RID: 2188 RVA: 0x000227D0 File Offset: 0x000209D0
	[Command(requiresAuthority = false)]
	private void CmdHitNpc(NPC npc, float falldown)
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		writer.WriteNetworkBehaviour(npc);
		writer.WriteFloat(falldown);
		base.SendCommandInternal("System.Void Bat::CmdHitNpc(NPC,System.Single)", 123880106, writer, 0, false);
		NetworkWriterPool.Return(writer);
	}

	// Token: 0x0600088D RID: 2189 RVA: 0x00022814 File Offset: 0x00020A14
	[Command(requiresAuthority = false)]
	private void CmdHitItem(Item item, float falldown)
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		writer.WriteNetworkBehaviour(item);
		writer.WriteFloat(falldown);
		base.SendCommandInternal("System.Void Bat::CmdHitItem(Item,System.Single)", 1971654748, writer, 0, false);
		NetworkWriterPool.Return(writer);
	}

	// Token: 0x0600088E RID: 2190 RVA: 0x00022858 File Offset: 0x00020A58
	private void TryBreak()
	{
		if (NetworkSingleton<GameManager>.Instance.state != GameState.Game)
		{
			return;
		}
		if (this.breakChance <= Random.value)
		{
			return;
		}
		base.DestroyItem();
	}

	// Token: 0x0600088F RID: 2191 RVA: 0x0002287C File Offset: 0x00020A7C
	public void LocalSetBatSpawnPoint(Transform point)
	{
		this.spawnPoint = point;
	}

	// Token: 0x06000890 RID: 2192 RVA: 0x00022888 File Offset: 0x00020A88
	[Server]
	public void ServerResetBat()
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void Bat::ServerResetBat()' called when server was not active");
			return;
		}
		if (base.NetworkHolder)
		{
			return;
		}
		if (!this.spawnPoint)
		{
			return;
		}
		if (NetworkSingleton<ElevatorManager>.Instance && NetworkSingleton<ElevatorManager>.Instance.IsInElevator(base.transform.position))
		{
			return;
		}
		this.RpcResetBat();
	}

	// Token: 0x06000891 RID: 2193 RVA: 0x000228F0 File Offset: 0x00020AF0
	[ClientRpc]
	private void RpcResetBat()
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		this.SendRPCInternal("System.Void Bat::RpcResetBat()", 57258751, writer, 0, true);
		NetworkWriterPool.Return(writer);
	}

	// Token: 0x06000892 RID: 2194 RVA: 0x00022920 File Offset: 0x00020B20
	[Command(requiresAuthority = false)]
	private void CmdPlayBatSFX(int i)
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		writer.WriteVarInt(i);
		base.SendCommandInternal("System.Void Bat::CmdPlayBatSFX(System.Int32)", -1271608753, writer, 0, false);
		NetworkWriterPool.Return(writer);
	}

	// Token: 0x06000893 RID: 2195 RVA: 0x0002295C File Offset: 0x00020B5C
	[ClientRpc]
	private void RpcPlayBatSFX(int i)
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		writer.WriteVarInt(i);
		this.SendRPCInternal("System.Void Bat::RpcPlayBatSFX(System.Int32)", 39244016, writer, 0, true);
		NetworkWriterPool.Return(writer);
	}

	// Token: 0x06000894 RID: 2196 RVA: 0x00022998 File Offset: 0x00020B98
	private void PlayBatSFX(int i)
	{
		EventReference sfxEvent;
		if (i == 0)
		{
			sfxEvent = this.sFXBatSwing;
		}
		else if (i == 1)
		{
			sfxEvent = this.sFXGenericBatHit;
		}
		else
		{
			if (i == 2)
			{
				SFXParams[] sFXParams = new SFXParams[]
				{
					new SFXParams("Magnitude", 1f)
				};
				SFXManager.SFXOneShot3DAttachedWithParameters(this.sFXPlayerHit, sFXParams, base.gameObject, false);
				return;
			}
			sfxEvent = this.sFXBatSwing;
		}
		SFXManager.SFXOneShot3DAttached(sfxEvent, base.gameObject, false);
	}

	// Token: 0x06000896 RID: 2198 RVA: 0x00002321 File Offset: 0x00000521
	public override bool Weaved()
	{
		return true;
	}

	// Token: 0x06000897 RID: 2199 RVA: 0x00022A5D File Offset: 0x00020C5D
	protected void UserCode_RpcOnPickedUp()
	{
		this.anim.SetTrigger("PickUp");
	}

	// Token: 0x06000898 RID: 2200 RVA: 0x00022A6F File Offset: 0x00020C6F
	protected static void InvokeUserCode_RpcOnPickedUp(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcOnPickedUp called on server.");
			return;
		}
		((Bat)obj).UserCode_RpcOnPickedUp();
	}

	// Token: 0x06000899 RID: 2201 RVA: 0x00022A92 File Offset: 0x00020C92
	protected void UserCode_RpcOnDropped()
	{
		this.anim.Play("Default", 0, 0f);
		this.anim.Update(0f);
	}

	// Token: 0x0600089A RID: 2202 RVA: 0x00022ABA File Offset: 0x00020CBA
	protected static void InvokeUserCode_RpcOnDropped(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcOnDropped called on server.");
			return;
		}
		((Bat)obj).UserCode_RpcOnDropped();
	}

	// Token: 0x0600089B RID: 2203 RVA: 0x00022ADD File Offset: 0x00020CDD
	protected void UserCode_CmdHitVfx__Boolean__Vector3(bool isSmall, Vector3 position)
	{
		this.RpcHitVfx(isSmall, position);
	}

	// Token: 0x0600089C RID: 2204 RVA: 0x00022AE7 File Offset: 0x00020CE7
	protected static void InvokeUserCode_CmdHitVfx__Boolean__Vector3(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdHitVfx called on client.");
			return;
		}
		((Bat)obj).UserCode_CmdHitVfx__Boolean__Vector3(reader.ReadBool(), reader.ReadVector3());
	}

	// Token: 0x0600089D RID: 2205 RVA: 0x00022B18 File Offset: 0x00020D18
	protected void UserCode_RpcHitVfx__Boolean__Vector3(bool isSmall, Vector3 position)
	{
		if (base.NetworkHolder && base.NetworkHolder.isLocalPlayer)
		{
			return;
		}
		Object.Destroy(Object.Instantiate<ParticleSystem>(isSmall ? this.hitVfxSmall : this.hitVfx, position, Quaternion.identity).gameObject, 1f);
	}

	// Token: 0x0600089E RID: 2206 RVA: 0x00022B6B File Offset: 0x00020D6B
	protected static void InvokeUserCode_RpcHitVfx__Boolean__Vector3(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcHitVfx called on server.");
			return;
		}
		((Bat)obj).UserCode_RpcHitVfx__Boolean__Vector3(reader.ReadBool(), reader.ReadVector3());
	}

	// Token: 0x0600089F RID: 2207 RVA: 0x00022B9C File Offset: 0x00020D9C
	protected void UserCode_CmdHitPlayer__PlayerController__Single(PlayerController pc, float falldown)
	{
		Vector3 vector = this.hitPoint.transform.forward * this.power + Vector3.up * this.upPower + this.hitPoint.transform.right * Random.Range(-1f, 1f) * this.randomPower;
		vector *= falldown;
		Vector3 torque = Vector3.Cross(Vector3.ClampMagnitude(this.hitPoint.transform.position - pc.GetComponent<Rigidbody>().worldCenterOfMass, 1f), this.hitPoint.transform.forward) * this.torquePower * falldown;
		pc.ServerKnockback(vector, torque);
		this.TryBreak();
	}

	// Token: 0x060008A0 RID: 2208 RVA: 0x00022C75 File Offset: 0x00020E75
	protected static void InvokeUserCode_CmdHitPlayer__PlayerController__Single(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdHitPlayer called on client.");
			return;
		}
		((Bat)obj).UserCode_CmdHitPlayer__PlayerController__Single(reader.ReadNetworkBehaviour<PlayerController>(), reader.ReadFloat());
	}

	// Token: 0x060008A1 RID: 2209 RVA: 0x00022CA8 File Offset: 0x00020EA8
	protected void UserCode_CmdHitNpc__NPC__Single(NPC npc, float falldown)
	{
		Vector3 vector = this.hitPoint.transform.forward * this.power + Vector3.up * this.upPower + this.hitPoint.transform.right * Random.Range(-1f, 1f) * this.randomPower;
		vector *= falldown;
		Vector3 torque = Vector3.Cross(Vector3.ClampMagnitude(this.hitPoint.transform.position - npc.GetComponent<Rigidbody>().worldCenterOfMass, 1f), this.hitPoint.transform.forward) * this.torquePower * falldown;
		npc.ServerKnockback(vector, torque);
	}

	// Token: 0x060008A2 RID: 2210 RVA: 0x00022D7B File Offset: 0x00020F7B
	protected static void InvokeUserCode_CmdHitNpc__NPC__Single(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdHitNpc called on client.");
			return;
		}
		((Bat)obj).UserCode_CmdHitNpc__NPC__Single(reader.ReadNetworkBehaviour<NPC>(), reader.ReadFloat());
	}

	// Token: 0x060008A3 RID: 2211 RVA: 0x00022DAC File Offset: 0x00020FAC
	protected void UserCode_CmdHitItem__Item__Single(Item item, float falldown)
	{
		Rigidbody component = item.GetComponent<Rigidbody>();
		Vector3 vector = this.hitPoint.transform.forward * this.power + Vector3.up * this.upPower + this.hitPoint.transform.right * Random.Range(-1f, 1f) * this.randomPower;
		vector *= falldown;
		Vector3 torque = Vector3.Cross(Vector3.ClampMagnitude(this.hitPoint.transform.position - component.worldCenterOfMass, 1f), this.hitPoint.transform.forward) * this.torquePower * falldown;
		component.AddForce(vector, ForceMode.Impulse);
		component.AddTorque(torque, ForceMode.Impulse);
	}

	// Token: 0x060008A4 RID: 2212 RVA: 0x00022E89 File Offset: 0x00021089
	protected static void InvokeUserCode_CmdHitItem__Item__Single(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdHitItem called on client.");
			return;
		}
		((Bat)obj).UserCode_CmdHitItem__Item__Single(reader.ReadNetworkBehaviour<Item>(), reader.ReadFloat());
	}

	// Token: 0x060008A5 RID: 2213 RVA: 0x00022EB9 File Offset: 0x000210B9
	protected void UserCode_RpcResetBat()
	{
		this.Rb.isKinematic = true;
		base.transform.parent = this.spawnPoint;
		base.transform.localPosition = Vector3.zero;
		base.transform.localRotation = Quaternion.identity;
	}

	// Token: 0x060008A6 RID: 2214 RVA: 0x00022EF8 File Offset: 0x000210F8
	protected static void InvokeUserCode_RpcResetBat(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcResetBat called on server.");
			return;
		}
		((Bat)obj).UserCode_RpcResetBat();
	}

	// Token: 0x060008A7 RID: 2215 RVA: 0x00022F1B File Offset: 0x0002111B
	protected void UserCode_CmdPlayBatSFX__Int32(int i)
	{
		this.RpcPlayBatSFX(i);
	}

	// Token: 0x060008A8 RID: 2216 RVA: 0x00022F24 File Offset: 0x00021124
	protected static void InvokeUserCode_CmdPlayBatSFX__Int32(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdPlayBatSFX called on client.");
			return;
		}
		((Bat)obj).UserCode_CmdPlayBatSFX__Int32(reader.ReadVarInt());
	}

	// Token: 0x060008A9 RID: 2217 RVA: 0x00022F4D File Offset: 0x0002114D
	protected void UserCode_RpcPlayBatSFX__Int32(int i)
	{
		this.PlayBatSFX(i);
	}

	// Token: 0x060008AA RID: 2218 RVA: 0x00022F56 File Offset: 0x00021156
	protected static void InvokeUserCode_RpcPlayBatSFX__Int32(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcPlayBatSFX called on server.");
			return;
		}
		((Bat)obj).UserCode_RpcPlayBatSFX__Int32(reader.ReadVarInt());
	}

	// Token: 0x060008AB RID: 2219 RVA: 0x00022F80 File Offset: 0x00021180
	static Bat()
	{
		RemoteProcedureCalls.RegisterCommand(typeof(Bat), "System.Void Bat::CmdHitVfx(System.Boolean,UnityEngine.Vector3)", new RemoteCallDelegate(Bat.InvokeUserCode_CmdHitVfx__Boolean__Vector3), false);
		RemoteProcedureCalls.RegisterCommand(typeof(Bat), "System.Void Bat::CmdHitPlayer(PlayerController,System.Single)", new RemoteCallDelegate(Bat.InvokeUserCode_CmdHitPlayer__PlayerController__Single), false);
		RemoteProcedureCalls.RegisterCommand(typeof(Bat), "System.Void Bat::CmdHitNpc(NPC,System.Single)", new RemoteCallDelegate(Bat.InvokeUserCode_CmdHitNpc__NPC__Single), false);
		RemoteProcedureCalls.RegisterCommand(typeof(Bat), "System.Void Bat::CmdHitItem(Item,System.Single)", new RemoteCallDelegate(Bat.InvokeUserCode_CmdHitItem__Item__Single), false);
		RemoteProcedureCalls.RegisterCommand(typeof(Bat), "System.Void Bat::CmdPlayBatSFX(System.Int32)", new RemoteCallDelegate(Bat.InvokeUserCode_CmdPlayBatSFX__Int32), false);
		RemoteProcedureCalls.RegisterRpc(typeof(Bat), "System.Void Bat::RpcOnPickedUp()", new RemoteCallDelegate(Bat.InvokeUserCode_RpcOnPickedUp));
		RemoteProcedureCalls.RegisterRpc(typeof(Bat), "System.Void Bat::RpcOnDropped()", new RemoteCallDelegate(Bat.InvokeUserCode_RpcOnDropped));
		RemoteProcedureCalls.RegisterRpc(typeof(Bat), "System.Void Bat::RpcHitVfx(System.Boolean,UnityEngine.Vector3)", new RemoteCallDelegate(Bat.InvokeUserCode_RpcHitVfx__Boolean__Vector3));
		RemoteProcedureCalls.RegisterRpc(typeof(Bat), "System.Void Bat::RpcResetBat()", new RemoteCallDelegate(Bat.InvokeUserCode_RpcResetBat));
		RemoteProcedureCalls.RegisterRpc(typeof(Bat), "System.Void Bat::RpcPlayBatSFX(System.Int32)", new RemoteCallDelegate(Bat.InvokeUserCode_RpcPlayBatSFX__Int32));
	}

	// Token: 0x04000572 RID: 1394
	[Header("References")]
	[SerializeField]
	private Transform spawnPoint;

	// Token: 0x04000573 RID: 1395
	[SerializeField]
	private Collider hitPoint;

	// Token: 0x04000574 RID: 1396
	[SerializeField]
	private Animator anim;

	// Token: 0x04000575 RID: 1397
	[SerializeField]
	private ParticleSystem hitVfx;

	// Token: 0x04000576 RID: 1398
	[SerializeField]
	private ParticleSystem hitVfxSmall;

	// Token: 0x04000577 RID: 1399
	[Header("Settings")]
	[SerializeField]
	private float hitDuration = 0.5f;

	// Token: 0x04000578 RID: 1400
	[SerializeField]
	private float power = 7f;

	// Token: 0x04000579 RID: 1401
	[SerializeField]
	private float upPower = 7f;

	// Token: 0x0400057A RID: 1402
	[SerializeField]
	private float randomPower = 3f;

	// Token: 0x0400057B RID: 1403
	[SerializeField]
	private float torquePower = 10f;

	// Token: 0x0400057C RID: 1404
	[SerializeField]
	private float breakChance = 0.15f;

	// Token: 0x0400057D RID: 1405
	[Header("SFX")]
	[SerializeField]
	private EventReference sFXBatSwing;

	// Token: 0x0400057E RID: 1406
	[SerializeField]
	private EventReference sFXGenericBatHit;

	// Token: 0x0400057F RID: 1407
	[SerializeField]
	private EventReference sFXPlayerHit;

	// Token: 0x04000580 RID: 1408
	private bool _isUsing;
}
