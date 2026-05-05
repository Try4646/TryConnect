using System;
using FMOD.Studio;
using FMODUnity;
using Mirror;
using Mirror.RemoteCalls;
using Unity.Mathematics;
using UnityEngine;

// Token: 0x02000270 RID: 624
public class PlayerSFX : NetworkBehaviour
{
	// Token: 0x06001613 RID: 5651 RVA: 0x0005F0E4 File Offset: 0x0005D2E4
	private void Awake()
	{
		this._ps = Resources.Load<PlayerSettings>("PlayerSettings");
		this._pc = base.GetComponent<PlayerController>();
		this._pi = base.GetComponent<PlayerInventory>();
	}

	// Token: 0x06001614 RID: 5652 RVA: 0x0005F110 File Offset: 0x0005D310
	private void OnEnable()
	{
		this._pc.OnClientJumped += this.OnJump;
		this._pc.OnClientLanded += this.OnLand;
		this._pi.OnClientItemThrown += this.OnThrowItem;
		this._pi.OnClientItemPickup += this.OnPickupItem;
		InputEvents.OnZoomEvent = (Action<bool>)Delegate.Combine(InputEvents.OnZoomEvent, new Action<bool>(this.OnZoom));
	}

	// Token: 0x06001615 RID: 5653 RVA: 0x0005F19C File Offset: 0x0005D39C
	private void OnDisable()
	{
		this._pc.OnClientJumped -= this.OnJump;
		this._pc.OnClientLanded -= this.OnLand;
		this._pi.OnClientItemThrown -= this.OnThrowItem;
		this._pi.OnClientItemPickup -= this.OnPickupItem;
		InputEvents.OnZoomEvent = (Action<bool>)Delegate.Remove(InputEvents.OnZoomEvent, new Action<bool>(this.OnZoom));
		if (this._walkInstance.isValid())
		{
			this._walkInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
			this._walkInstance.release();
		}
	}

	// Token: 0x06001616 RID: 5654 RVA: 0x0005F24B File Offset: 0x0005D44B
	private void Update()
	{
		this.SetIsGrounded();
		this.Walking();
	}

	// Token: 0x06001617 RID: 5655 RVA: 0x0005F25C File Offset: 0x0005D45C
	private void SetIsGrounded()
	{
		bool isGrounded = this._pc.isGrounded;
		if (this._isGrounded == isGrounded)
		{
			return;
		}
		this._isGrounded = isGrounded;
		if (this._walkInstance.isValid())
		{
			this._walkInstance.setParameterByName("IsGrounded", (float)(isGrounded ? 1 : 0), false);
		}
	}

	// Token: 0x06001618 RID: 5656 RVA: 0x0005F2B0 File Offset: 0x0005D4B0
	private void Walking()
	{
		bool flag = this._pc.serverVelocity.sqrMagnitude > 0.1f;
		if (this._isWalking == flag)
		{
			return;
		}
		this._isWalking = flag;
		if (flag)
		{
			this._walkInstance = RuntimeManager.CreateInstance(this.footstep);
			this._walkInstance.setParameterByName("IsGrounded", (float)(this._isGrounded ? 1 : 0), false);
			RuntimeManager.AttachInstanceToGameObject(this._walkInstance, base.gameObject, true);
			this._walkInstance.start();
			return;
		}
		this._walkInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
		this._walkInstance.release();
	}

	// Token: 0x06001619 RID: 5657 RVA: 0x0005F354 File Offset: 0x0005D554
	private void OnJump(bool isGrounded)
	{
		SFXParams[] sFXParams = new SFXParams[]
		{
			new SFXParams("IsLocalPlayer", (float)(base.isLocalPlayer ? 1 : 0))
		};
		SFXManager.SFXOneShot3DAttachedWithParameters(this.jump, sFXParams, base.gameObject, false);
	}

	// Token: 0x0600161A RID: 5658 RVA: 0x0005F39C File Offset: 0x0005D59C
	private void OnLand(float fallImpact)
	{
		float v = math.remap(0.05f, 20f, 0f, 1f, fallImpact);
		SFXParams[] sFXParams = new SFXParams[]
		{
			new SFXParams("IsLocalPlayer", (float)(base.isLocalPlayer ? 1 : 0)),
			new SFXParams("Force", v)
		};
		SFXManager.SFXOneShot3DAttachedWithParameters(this.landing, sFXParams, base.gameObject, false);
	}

	// Token: 0x0600161B RID: 5659 RVA: 0x0005F410 File Offset: 0x0005D610
	private void OnThrowItem(float force, Item item)
	{
		float v = math.remap(this._ps.minItemThrowForce, this._ps.maxItemThrowForce, 0f, 1f, force);
		SFXParams[] sFXParams = new SFXParams[]
		{
			new SFXParams("Force", v)
		};
		SFXManager.SFXOneShot3DAttachedWithParameters(this.throwItem, sFXParams, base.gameObject, false);
	}

	// Token: 0x0600161C RID: 5660 RVA: 0x0005F470 File Offset: 0x0005D670
	private void OnPickupItem(Item item)
	{
		SFXManager.SFXOneShot3DAttached(this.pickupItem, base.gameObject, false);
	}

	// Token: 0x0600161D RID: 5661 RVA: 0x0005F484 File Offset: 0x0005D684
	private void OnZoom(bool isPressed)
	{
		if (!base.isLocalPlayer)
		{
			return;
		}
		this.ClientOnZoom(isPressed);
		this.CmdOnZoom(isPressed);
	}

	// Token: 0x0600161E RID: 5662 RVA: 0x0005F4A0 File Offset: 0x0005D6A0
	[Command]
	private void CmdOnZoom(bool isPressed)
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		writer.WriteBool(isPressed);
		base.SendCommandInternal("System.Void PlayerSFX::CmdOnZoom(System.Boolean)", 661333140, writer, 0, true);
		NetworkWriterPool.Return(writer);
	}

	// Token: 0x0600161F RID: 5663 RVA: 0x0005F4DC File Offset: 0x0005D6DC
	[ClientRpc]
	private void RpcOnZoom(bool isPressed)
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		writer.WriteBool(isPressed);
		this.SendRPCInternal("System.Void PlayerSFX::RpcOnZoom(System.Boolean)", -1627116593, writer, 0, true);
		NetworkWriterPool.Return(writer);
	}

	// Token: 0x06001620 RID: 5664 RVA: 0x0005F516 File Offset: 0x0005D716
	private void ClientOnZoom(bool isPressed)
	{
		SFXManager.SFXOneShot3DAttached(this.zoom, base.gameObject, isPressed);
	}

	// Token: 0x06001622 RID: 5666 RVA: 0x00002321 File Offset: 0x00000521
	public override bool Weaved()
	{
		return true;
	}

	// Token: 0x06001623 RID: 5667 RVA: 0x0005F52A File Offset: 0x0005D72A
	protected void UserCode_CmdOnZoom__Boolean(bool isPressed)
	{
		this.RpcOnZoom(isPressed);
	}

	// Token: 0x06001624 RID: 5668 RVA: 0x0005F533 File Offset: 0x0005D733
	protected static void InvokeUserCode_CmdOnZoom__Boolean(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdOnZoom called on client.");
			return;
		}
		((PlayerSFX)obj).UserCode_CmdOnZoom__Boolean(reader.ReadBool());
	}

	// Token: 0x06001625 RID: 5669 RVA: 0x0005F55C File Offset: 0x0005D75C
	protected void UserCode_RpcOnZoom__Boolean(bool isPressed)
	{
		if (base.isLocalPlayer)
		{
			return;
		}
		this.ClientOnZoom(isPressed);
	}

	// Token: 0x06001626 RID: 5670 RVA: 0x0005F56E File Offset: 0x0005D76E
	protected static void InvokeUserCode_RpcOnZoom__Boolean(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcOnZoom called on server.");
			return;
		}
		((PlayerSFX)obj).UserCode_RpcOnZoom__Boolean(reader.ReadBool());
	}

	// Token: 0x06001627 RID: 5671 RVA: 0x0005F598 File Offset: 0x0005D798
	static PlayerSFX()
	{
		RemoteProcedureCalls.RegisterCommand(typeof(PlayerSFX), "System.Void PlayerSFX::CmdOnZoom(System.Boolean)", new RemoteCallDelegate(PlayerSFX.InvokeUserCode_CmdOnZoom__Boolean), true);
		RemoteProcedureCalls.RegisterRpc(typeof(PlayerSFX), "System.Void PlayerSFX::RpcOnZoom(System.Boolean)", new RemoteCallDelegate(PlayerSFX.InvokeUserCode_RpcOnZoom__Boolean));
	}

	// Token: 0x04000E77 RID: 3703
	[Header("SFX References")]
	[SerializeField]
	private EventReference throwItem;

	// Token: 0x04000E78 RID: 3704
	[SerializeField]
	private EventReference pickupItem;

	// Token: 0x04000E79 RID: 3705
	[SerializeField]
	private EventReference pickupChip;

	// Token: 0x04000E7A RID: 3706
	[SerializeField]
	private EventReference jump;

	// Token: 0x04000E7B RID: 3707
	[SerializeField]
	private EventReference landing;

	// Token: 0x04000E7C RID: 3708
	[SerializeField]
	private EventReference footstep;

	// Token: 0x04000E7D RID: 3709
	[SerializeField]
	private EventReference zoom;

	// Token: 0x04000E7E RID: 3710
	private PlayerSettings _ps;

	// Token: 0x04000E7F RID: 3711
	private PlayerInventory _pi;

	// Token: 0x04000E80 RID: 3712
	private PlayerController _pc;

	// Token: 0x04000E81 RID: 3713
	private EventInstance _walkInstance;

	// Token: 0x04000E82 RID: 3714
	private bool _isWalking;

	// Token: 0x04000E83 RID: 3715
	private bool _isGrounded;
}
