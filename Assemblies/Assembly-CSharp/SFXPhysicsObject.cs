using System;
using FMOD.Studio;
using FMODUnity;
using Mirror;
using Mirror.RemoteCalls;
using UnityEngine;

// Token: 0x02000282 RID: 642
public class SFXPhysicsObject : NetworkBehaviour
{
	// Token: 0x060016D0 RID: 5840 RVA: 0x000613D2 File Offset: 0x0005F5D2
	private void Awake()
	{
		this.rb = base.GetComponent<Rigidbody>();
	}

	// Token: 0x060016D1 RID: 5841 RVA: 0x000613E0 File Offset: 0x0005F5E0
	private void Start()
	{
		this.hitCooldownTimer = Time.time + this.startSleepTime;
		this.playerHitCooldownTimer = Time.time + this.startSleepTime;
	}

	// Token: 0x060016D2 RID: 5842 RVA: 0x00061408 File Offset: 0x0005F608
	private void OnCollisionEnter(Collision other)
	{
		if (!base.enabled)
		{
			return;
		}
		if (this.eventRef.IsNull)
		{
			return;
		}
		if (this.hitCooldownTimer >= Time.time)
		{
			return;
		}
		if (other.gameObject.layer == this.playerLayer)
		{
			if (this.canHitPlayer)
			{
				this.OnPlayerCollision(other);
			}
			return;
		}
		Vector3 relativeVelocity = other.relativeVelocity;
		if (relativeVelocity.magnitude < this.SensitivityThreshold)
		{
			return;
		}
		float num = Mathf.Max(0f, relativeVelocity.magnitude - this.SensitivityThreshold);
		num = Mathf.Clamp01(num * 0.07f);
		this.HandleHit(num);
	}

	// Token: 0x060016D3 RID: 5843 RVA: 0x000614A4 File Offset: 0x0005F6A4
	private void OnPlayerCollision(Collision other)
	{
		if (this.playerHitReference.IsNull)
		{
			return;
		}
		if (this.wasSleeping)
		{
			return;
		}
		if (other.relativeVelocity.magnitude < 6.5f)
		{
			return;
		}
		if (this.playerHitCooldownTimer >= Time.time)
		{
			return;
		}
		Vector3 relativeVelocity = other.relativeVelocity;
		if (relativeVelocity.magnitude < this.SensitivityThreshold * this.playerHitThresholdMultiplier)
		{
			return;
		}
		float num = Mathf.Max(0f, relativeVelocity.magnitude - this.SensitivityThreshold);
		num = Mathf.Clamp01(num * 0.07f);
		this.HandlePlayerHit(num);
	}

	// Token: 0x060016D4 RID: 5844 RVA: 0x00061538 File Offset: 0x0005F738
	private void OnCollisionStay(Collision other)
	{
		if (!base.enabled)
		{
			return;
		}
		if (!this.stayCollision)
		{
			return;
		}
		if (this.eventRef.IsNull)
		{
			return;
		}
		if (this.wasSleeping)
		{
			return;
		}
		if (this.hitCooldownTimer >= Time.time)
		{
			return;
		}
		if (other.gameObject.layer == this.playerLayer)
		{
			return;
		}
		Vector3 impulse = other.impulse;
		if (impulse.magnitude < this.SensitivityThreshold * this.staySensitivityMultiplier)
		{
			return;
		}
		float magnitude = Mathf.Max(0f, impulse.magnitude - this.SensitivityThreshold);
		this.HandleHit(magnitude);
	}

	// Token: 0x060016D5 RID: 5845 RVA: 0x000615CE File Offset: 0x0005F7CE
	private void LateUpdate()
	{
		if (!base.enabled)
		{
			return;
		}
		this.wasSleeping = this.rb.IsSleeping();
	}

	// Token: 0x060016D6 RID: 5846 RVA: 0x000615EA File Offset: 0x0005F7EA
	private void HandleHit(float magnitude)
	{
		this.CmdPlayHit(magnitude);
		this.hitCooldownTimer = Time.time + this.hitCooldownTime * Random.Range(0.9f, 1f);
	}

	// Token: 0x060016D7 RID: 5847 RVA: 0x00061615 File Offset: 0x0005F815
	[Server]
	private void CmdPlayHit(float magnitude)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void SFXPhysicsObject::CmdPlayHit(System.Single)' called when server was not active");
			return;
		}
		this.RpcPlayHit(magnitude);
	}

	// Token: 0x060016D8 RID: 5848 RVA: 0x00061634 File Offset: 0x0005F834
	[ClientRpc]
	private void RpcPlayHit(float magnitude)
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		writer.WriteFloat(magnitude);
		this.SendRPCInternal("System.Void SFXPhysicsObject::RpcPlayHit(System.Single)", 1505502791, writer, 0, true);
		NetworkWriterPool.Return(writer);
	}

	// Token: 0x060016D9 RID: 5849 RVA: 0x0006166E File Offset: 0x0005F86E
	private void HandlePlayerHit(float magnitude)
	{
		this.CmdPlayPlayerHit(magnitude);
		this.playerHitCooldownTimer = Time.time + this.playerHitCooldownTime;
	}

	// Token: 0x060016DA RID: 5850 RVA: 0x00061689 File Offset: 0x0005F889
	[Server]
	private void CmdPlayPlayerHit(float magnitude)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void SFXPhysicsObject::CmdPlayPlayerHit(System.Single)' called when server was not active");
			return;
		}
		this.RpcPlayPlayerHit(magnitude);
	}

	// Token: 0x060016DB RID: 5851 RVA: 0x000616A8 File Offset: 0x0005F8A8
	[ClientRpc]
	private void RpcPlayPlayerHit(float magnitude)
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		writer.WriteFloat(magnitude);
		this.SendRPCInternal("System.Void SFXPhysicsObject::RpcPlayPlayerHit(System.Single)", 270337566, writer, 0, true);
		NetworkWriterPool.Return(writer);
	}

	// Token: 0x060016DC RID: 5852 RVA: 0x000616E2 File Offset: 0x0005F8E2
	public void SetPlayerThrowCooldown()
	{
		this.playerHitCooldownTimer = Time.time + this.playerThrowCooldown;
	}

	// Token: 0x060016DE RID: 5854 RVA: 0x00002321 File Offset: 0x00000521
	public override bool Weaved()
	{
		return true;
	}

	// Token: 0x060016DF RID: 5855 RVA: 0x00061778 File Offset: 0x0005F978
	protected void UserCode_RpcPlayHit__Single(float magnitude)
	{
		SFXParams[] sFXParams = new SFXParams[]
		{
			new SFXParams("PhysicsObjectType", 0f),
			new SFXParams("Magnitude", magnitude)
		};
		SFXManager.SFXOneShotWithParameters(this.eventRef, sFXParams, base.gameObject.transform.position, this.pitchMod);
	}

	// Token: 0x060016E0 RID: 5856 RVA: 0x000617D6 File Offset: 0x0005F9D6
	protected static void InvokeUserCode_RpcPlayHit__Single(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcPlayHit called on server.");
			return;
		}
		((SFXPhysicsObject)obj).UserCode_RpcPlayHit__Single(reader.ReadFloat());
	}

	// Token: 0x060016E1 RID: 5857 RVA: 0x00061800 File Offset: 0x0005FA00
	protected void UserCode_RpcPlayPlayerHit__Single(float magnitude)
	{
		SFXParams[] sFXParams = new SFXParams[]
		{
			new SFXParams("Magnitude", magnitude)
		};
		SFXManager.SFXOneShotWithParameters(this.playerHitReference, sFXParams, base.gameObject.transform.position, 1f);
	}

	// Token: 0x060016E2 RID: 5858 RVA: 0x00061847 File Offset: 0x0005FA47
	protected static void InvokeUserCode_RpcPlayPlayerHit__Single(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcPlayPlayerHit called on server.");
			return;
		}
		((SFXPhysicsObject)obj).UserCode_RpcPlayPlayerHit__Single(reader.ReadFloat());
	}

	// Token: 0x060016E3 RID: 5859 RVA: 0x00061874 File Offset: 0x0005FA74
	static SFXPhysicsObject()
	{
		RemoteProcedureCalls.RegisterRpc(typeof(SFXPhysicsObject), "System.Void SFXPhysicsObject::RpcPlayHit(System.Single)", new RemoteCallDelegate(SFXPhysicsObject.InvokeUserCode_RpcPlayHit__Single));
		RemoteProcedureCalls.RegisterRpc(typeof(SFXPhysicsObject), "System.Void SFXPhysicsObject::RpcPlayPlayerHit(System.Single)", new RemoteCallDelegate(SFXPhysicsObject.InvokeUserCode_RpcPlayPlayerHit__Single));
	}

	// Token: 0x04000ED8 RID: 3800
	[SerializeField]
	private EventReference eventRef;

	// Token: 0x04000ED9 RID: 3801
	[SerializeField]
	private float SensitivityThreshold = 3f;

	// Token: 0x04000EDA RID: 3802
	[Header("Stay Collision")]
	[SerializeField]
	private bool stayCollision = true;

	// Token: 0x04000EDB RID: 3803
	[SerializeField]
	private float staySensitivityMultiplier = 0.2f;

	// Token: 0x04000EDC RID: 3804
	[Header("Other")]
	[SerializeField]
	private float hitCooldownTime = 0.3f;

	// Token: 0x04000EDD RID: 3805
	private float hitCooldownTimer;

	// Token: 0x04000EDE RID: 3806
	[SerializeField]
	private float pitchMod = 1f;

	// Token: 0x04000EDF RID: 3807
	private EventInstance movementInstance;

	// Token: 0x04000EE0 RID: 3808
	private Rigidbody rb;

	// Token: 0x04000EE1 RID: 3809
	private int playerLayer = 6;

	// Token: 0x04000EE2 RID: 3810
	private bool wasSleeping;

	// Token: 0x04000EE3 RID: 3811
	[SerializeField]
	private EventReference playerHitReference;

	// Token: 0x04000EE4 RID: 3812
	private float playerHitCooldownTime = 0.8f;

	// Token: 0x04000EE5 RID: 3813
	private float playerHitCooldownTimer;

	// Token: 0x04000EE6 RID: 3814
	private float playerHitThresholdMultiplier = 3f;

	// Token: 0x04000EE7 RID: 3815
	private float playerThrowCooldown = 0.3f;

	// Token: 0x04000EE8 RID: 3816
	private float startSleepTime = 0.3f;

	// Token: 0x04000EE9 RID: 3817
	[SerializeField]
	private bool canHitPlayer = true;
}
