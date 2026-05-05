using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using Mirror;
using Mirror.RemoteCalls;
using UnityEngine;

// Token: 0x020001F7 RID: 503
public class PlayerController : NetworkBehaviour
{
	// Token: 0x170001A4 RID: 420
	// (get) Token: 0x06001205 RID: 4613 RVA: 0x0004DC4A File Offset: 0x0004BE4A
	// (set) Token: 0x06001206 RID: 4614 RVA: 0x0004DC54 File Offset: 0x0004BE54
	[ReadOnly]
	public PlayerController.PlayerState State
	{
		get
		{
			return this._state;
		}
		set
		{
			if (this._ragdollRoutine != null)
			{
				base.StopCoroutine(this._ragdollRoutine);
			}
			if (value == PlayerController.PlayerState.Ragdoll)
			{
				this._ragdollRoutine = base.StartCoroutine(this.DelayedDisableRagdoll());
			}
			if (value == this._state)
			{
				return;
			}
			this.Network_state = value;
			this.SetPlayerState(value);
		}
	}

	// Token: 0x170001A5 RID: 421
	// (get) Token: 0x06001207 RID: 4615 RVA: 0x0004DCA3 File Offset: 0x0004BEA3
	// (set) Token: 0x06001208 RID: 4616 RVA: 0x0004DCAB File Offset: 0x0004BEAB
	public bool IsLocked
	{
		get
		{
			return this._isLocked;
		}
		set
		{
			if (value == this._isLocked)
			{
				return;
			}
			this._isLocked = value;
			if (this._isLocked)
			{
				this.Crouch(false);
				this._moveInput = Vector3.zero;
				this._horizontalMoveDirection = Vector3.zero;
			}
		}
	}

	// Token: 0x14000010 RID: 16
	// (add) Token: 0x06001209 RID: 4617 RVA: 0x0004DCE8 File Offset: 0x0004BEE8
	// (remove) Token: 0x0600120A RID: 4618 RVA: 0x0004DD20 File Offset: 0x0004BF20
	public event Action<bool> OnClientJumped;

	// Token: 0x14000011 RID: 17
	// (add) Token: 0x0600120B RID: 4619 RVA: 0x0004DD58 File Offset: 0x0004BF58
	// (remove) Token: 0x0600120C RID: 4620 RVA: 0x0004DD90 File Offset: 0x0004BF90
	public event Action<float> OnClientLanded;

	// Token: 0x14000012 RID: 18
	// (add) Token: 0x0600120D RID: 4621 RVA: 0x0004DDC8 File Offset: 0x0004BFC8
	// (remove) Token: 0x0600120E RID: 4622 RVA: 0x0004DE00 File Offset: 0x0004C000
	public event Action<bool> OnClientCrouched;

	// Token: 0x0600120F RID: 4623 RVA: 0x0004DE38 File Offset: 0x0004C038
	private void Awake()
	{
		this._ps = Resources.Load<PlayerSettings>("PlayerSettings");
		this._rb = base.GetComponent<Rigidbody>();
		this._pc = base.GetComponent<PlayerCarry>();
		this._pi = base.GetComponent<PlayerInventory>();
		if (!this.head)
		{
			this.head = base.GetComponentInChildren<PlayerHead>();
		}
		this._rb.freezeRotation = true;
	}

	// Token: 0x06001210 RID: 4624 RVA: 0x0004DEA0 File Offset: 0x0004C0A0
	private void OnEnable()
	{
		InputEvents.OnMoveEvent = (Action<Vector2>)Delegate.Combine(InputEvents.OnMoveEvent, new Action<Vector2>(this.OnMove));
		InputEvents.OnJumpEvent = (Action<bool>)Delegate.Combine(InputEvents.OnJumpEvent, new Action<bool>(this.OnJump));
		InputEvents.OnCrouchEvent = (Action<bool>)Delegate.Combine(InputEvents.OnCrouchEvent, new Action<bool>(this.OnCrouch));
	}

	// Token: 0x06001211 RID: 4625 RVA: 0x0004DF10 File Offset: 0x0004C110
	private void OnDisable()
	{
		InputEvents.OnMoveEvent = (Action<Vector2>)Delegate.Remove(InputEvents.OnMoveEvent, new Action<Vector2>(this.OnMove));
		InputEvents.OnJumpEvent = (Action<bool>)Delegate.Remove(InputEvents.OnJumpEvent, new Action<bool>(this.OnJump));
		InputEvents.OnCrouchEvent = (Action<bool>)Delegate.Remove(InputEvents.OnCrouchEvent, new Action<bool>(this.OnCrouch));
	}

	// Token: 0x06001212 RID: 4626 RVA: 0x0004DF7D File Offset: 0x0004C17D
	private void FixedUpdate()
	{
		this.CheckGround();
		this.MovePlayer();
		this.StepClimb();
		this.ApplyGravity();
		this.HandleVelocityChange();
		this.SendVelocity();
	}

	// Token: 0x06001213 RID: 4627 RVA: 0x0004DFA4 File Offset: 0x0004C1A4
	public override void OnStartClient()
	{
		base.OnStartClient();
		this.head.transform.DOLocalMoveY(this._ps.headHeight, this._ps.headMoveDuration, false).SetEase(Ease.InOutSine);
		if (!base.isLocalPlayer)
		{
			base.enabled = false;
			return;
		}
		this._rb.interpolation = RigidbodyInterpolation.Interpolate;
	}

	// Token: 0x06001214 RID: 4628 RVA: 0x0004E001 File Offset: 0x0004C201
	private void OnMove(Vector2 input)
	{
		this._moveInput = input;
	}

	// Token: 0x06001215 RID: 4629 RVA: 0x0004E00A File Offset: 0x0004C20A
	private void OnJump(bool isPressed)
	{
		if (!isPressed)
		{
			return;
		}
		if (Time.time - this._lastJumpTime < 0.05f)
		{
			return;
		}
		this._lastJumpTime = Time.time;
		this.CheckJump();
		this.Jump();
	}

	// Token: 0x06001216 RID: 4630 RVA: 0x0004E03B File Offset: 0x0004C23B
	private void OnCrouch(bool isPressed)
	{
		this.Crouch(isPressed);
	}

	// Token: 0x06001217 RID: 4631 RVA: 0x0004E044 File Offset: 0x0004C244
	private void CheckGround()
	{
		Vector3 vector = base.transform.position + base.transform.up * (this._ps.playerRadius - this._ps.playerHeadRadius);
		Vector3 origin = vector + Vector3.up * 0.04f;
		float maxDistance = this._ps.groundCheckDistance + 0.05f;
		float radius = this._ps.playerRadius - 0.01f;
		if (!this.hasBody)
		{
			vector = base.transform.position;
			origin = vector + Vector3.up * 0.04f;
			maxDistance = this._ps.groundCheckDistance + 0.05f;
			radius = this._ps.playerHeadRadius - 0.01f;
		}
		int num = Physics.SphereCastNonAlloc(new Ray(origin, Vector3.down), radius, this._groundCheckHits, maxDistance, this._ps.groundMask, QueryTriggerInteraction.Ignore);
		List<Vector3> list = new List<Vector3>();
		for (int i = 0; i < num; i++)
		{
			RaycastHit raycastHit = this._groundCheckHits[i];
			if (raycastHit.collider && (!raycastHit.collider.attachedRigidbody || !(raycastHit.collider.attachedRigidbody == this._rb)) && Vector3.Angle(raycastHit.point - vector, Vector3.down) <= this._ps.maxSlopeAngle)
			{
				list.Add(raycastHit.point);
			}
		}
		if (list.Count > 0)
		{
			Vector3 a = list[0];
			foreach (Vector3 vector2 in list)
			{
				if ((vector2 - vector).sqrMagnitude < (a - vector).sqrMagnitude)
				{
					a = vector2;
				}
			}
			this.NetworkisGrounded = true;
			this._groundVector = (a - vector).normalized;
			return;
		}
		this.NetworkisGrounded = false;
		this._groundVector = Vector3.down;
	}

	// Token: 0x06001218 RID: 4632 RVA: 0x0004E280 File Offset: 0x0004C480
	private void CheckJump()
	{
		Vector3 origin = this.head.transform.position + Vector3.up * 0.04f;
		float maxDistance = this.head.transform.localPosition.y + this._ps.groundCheckDistance + 0.05f;
		float radius = this._ps.playerHeadRadius - 0.01f;
		if (this.hasBody && this._state == PlayerController.PlayerState.Ragdoll)
		{
			origin = base.transform.position + base.transform.up * (this._ps.playerRadius - this._ps.playerHeadRadius) + Vector3.up * 0.04f;
			maxDistance = this._ps.groundCheckDistance + 0.05f;
			radius = this._ps.playerRadius - 0.01f;
		}
		int num = Physics.SphereCastNonAlloc(new Ray(origin, Vector3.down), radius, this._jumpCheckHits, maxDistance, this._ps.groundMask, QueryTriggerInteraction.Ignore);
		bool canJump = false;
		for (int i = 0; i < num; i++)
		{
			RaycastHit raycastHit = this._jumpCheckHits[i];
			if (raycastHit.collider && (!raycastHit.collider.attachedRigidbody || !(raycastHit.collider.attachedRigidbody == this._rb)))
			{
				canJump = true;
				break;
			}
		}
		this._canJump = canJump;
	}

	// Token: 0x06001219 RID: 4633 RVA: 0x0004E400 File Offset: 0x0004C600
	private void MovePlayer()
	{
		if (this.IsLocked)
		{
			return;
		}
		if (this.State == PlayerController.PlayerState.Free && this.hasBody)
		{
			this.MoveFree();
			return;
		}
		if (this.State == PlayerController.PlayerState.Ragdoll && !this.hasBody)
		{
			this.MoveRoll();
		}
	}

	// Token: 0x0600121A RID: 4634 RVA: 0x0004E43C File Offset: 0x0004C63C
	private void MoveFree()
	{
		Vector3 a = Vector3.ProjectOnPlane(this.head.transform.forward, Vector3.up);
		Vector3 a2 = Vector3.ProjectOnPlane(this.head.transform.right, Vector3.up);
		this._horizontalMoveDirection = (a * this._moveInput.y + a2 * this._moveInput.x).normalized;
		Vector3 normalized = Vector3.ProjectOnPlane(this._horizontalMoveDirection, this._groundVector).normalized;
		float num = this._ps.maxSpeed;
		if (InputEvents.IsCrouchPressed)
		{
			num = this._ps.crouchMaxSpeed;
		}
		else if (InputEvents.IsSprintPressed)
		{
			num = this._ps.sprintMaxSpeed;
		}
		if (this._pi.NetworkholdingItem)
		{
			num *= 1f - this._pi.NetworkholdingItem.slowPercent;
		}
		Vector3 a3 = normalized * num;
		Vector3 b = new Vector3(this._rb.linearVelocity.x, 0f, this._rb.linearVelocity.z);
		Vector3 force = (a3 - b) * this._ps.acceleration;
		this._rb.AddForce(force, ForceMode.Acceleration);
	}

	// Token: 0x0600121B RID: 4635 RVA: 0x0004E588 File Offset: 0x0004C788
	private void MoveRoll()
	{
		Vector3 forward = this.head.transform.forward;
		Vector3 right = this.head.transform.right;
		Vector3 normalized = (forward * this._moveInput.y + right * this._moveInput.x).normalized;
		if (normalized.sqrMagnitude < 0.01f)
		{
			return;
		}
		float rollMaxSpeed = this._ps.rollMaxSpeed;
		Vector3 vector = -Vector3.Cross(normalized, this.head.transform.up).normalized;
		float num = Vector3.Dot(this._rb.angularVelocity, vector);
		float num2 = rollMaxSpeed - num;
		Vector3 torque = vector * (num2 * this._ps.rollAcceleration);
		this._rb.AddTorque(torque, ForceMode.Acceleration);
	}

	// Token: 0x0600121C RID: 4636 RVA: 0x0004E660 File Offset: 0x0004C860
	private void Jump()
	{
		if (this.IsLocked)
		{
			return;
		}
		if (this.State == PlayerController.PlayerState.Locked)
		{
			return;
		}
		if (!this._canJump)
		{
			return;
		}
		if (this.hasBody)
		{
			this._rb.linearVelocity = new Vector3(this._rb.linearVelocity.x, 0f, this._rb.linearVelocity.z);
			this._rb.AddForce(Vector3.up * this._ps.jumpForce, ForceMode.VelocityChange);
		}
		else
		{
			Vector3 forward = this.head.transform.forward;
			Vector3 a = Vector3.ProjectOnPlane(forward, Vector3.up);
			float d = Mathf.Clamp01(Vector3.Dot(forward, Vector3.up));
			Vector3 normalized = (a + Vector3.up * d).normalized;
			Vector3 normalized2 = Vector3.Cross(Vector3.up, normalized).normalized;
			Vector3 normalized3 = (Quaternion.AngleAxis(Vector3.Angle(Vector3.up, normalized) / 3f, normalized2) * Vector3.up).normalized;
			this._rb.linearVelocity = Vector3.zero;
			this._rb.AddForce(normalized3 * this._ps.jumpForce * 3f / 4f, ForceMode.VelocityChange);
		}
		this.OnJumpFeedback(this.isGrounded);
	}

	// Token: 0x0600121D RID: 4637 RVA: 0x0004E7C4 File Offset: 0x0004C9C4
	private void Crouch(bool isPressed)
	{
		if (this.IsLocked)
		{
			return;
		}
		if (this.State == PlayerController.PlayerState.Locked)
		{
			return;
		}
		if (!this.hasBody)
		{
			return;
		}
		this.head.transform.DOLocalMoveY(isPressed ? this._ps.headHeightCrouch : this._ps.headHeight, this._ps.headMoveDuration, false).SetEase(Ease.InOutSine);
		Action<bool> onClientCrouched = this.OnClientCrouched;
		if (onClientCrouched == null)
		{
			return;
		}
		onClientCrouched(isPressed);
	}

	// Token: 0x0600121E RID: 4638 RVA: 0x0004E83C File Offset: 0x0004CA3C
	private void StepClimb()
	{
		if (this.IsLocked)
		{
			return;
		}
		if (this.State != PlayerController.PlayerState.Free)
		{
			return;
		}
		Vector3 horizontalMoveDirection = this._horizontalMoveDirection;
		Vector3 a = base.transform.position + base.transform.up * -this._ps.playerRadius;
		Vector3 vector = a + base.transform.up * 0.01f;
		RaycastHit raycastHit;
		if (Physics.Raycast(vector, horizontalMoveDirection, out raycastHit, this._ps.stepCheckDistance, this._ps.groundMask))
		{
			Vector3 vector2 = a + base.transform.up * this._ps.maxStepHeight;
			float magnitude = (raycastHit.point - vector).magnitude;
			RaycastHit raycastHit2;
			RaycastHit raycastHit3;
			if (!Physics.Raycast(vector2, horizontalMoveDirection, out raycastHit2, magnitude, this._ps.groundMask) && Physics.Raycast(vector2 + horizontalMoveDirection * (magnitude + 0.01f), Vector3.down, out raycastHit3, this._ps.maxStepHeight + 0.01f, this._ps.groundMask))
			{
				if (Vector3.Angle(raycastHit3.normal, Vector3.up) > 5f)
				{
					return;
				}
				Vector3 a2 = Vector3.up * (raycastHit3.point.y - raycastHit.point.y + 0.1f);
				this._rb.MovePosition(base.transform.position + a2 * this._ps.stepUpDistance);
			}
		}
	}

	// Token: 0x0600121F RID: 4639 RVA: 0x0004E9E8 File Offset: 0x0004CBE8
	private void ApplyGravity()
	{
		if (this.State == PlayerController.PlayerState.Locked)
		{
			return;
		}
		if (!this._rb)
		{
			return;
		}
		if (this._rb.isKinematic)
		{
			return;
		}
		if (this._ps.gravity == 0f)
		{
			return;
		}
		this._rb.AddForce(this._groundVector * this._ps.gravity, ForceMode.Acceleration);
	}

	// Token: 0x06001220 RID: 4640 RVA: 0x0004EA50 File Offset: 0x0004CC50
	private void HandleVelocityChange()
	{
		float num = this._rb.linearVelocity.y - this._previousVelocity.y;
		if (this.isGrounded && num >= this._ps.landThreshold && this._rb.linearVelocity.y < 1f)
		{
			this.OnLandFeedback(num);
		}
		this._previousVelocity = this._rb.linearVelocity;
	}

	// Token: 0x06001221 RID: 4641 RVA: 0x0004EAC0 File Offset: 0x0004CCC0
	private void SendVelocity()
	{
		Vector3 linearVelocity = this._rb.linearVelocity;
		if ((linearVelocity - this.serverVelocity).sqrMagnitude > 0.005f)
		{
			this.NetworkserverVelocity = linearVelocity;
		}
	}

	// Token: 0x06001222 RID: 4642 RVA: 0x0004EAFC File Offset: 0x0004CCFC
	private void SetPlayerState(PlayerController.PlayerState newState)
	{
		if (!this._rb.isKinematic)
		{
			this._rb.linearVelocity = Vector3.zero;
		}
		this._rb.isKinematic = (newState == PlayerController.PlayerState.Locked);
		this._rb.constraints = ((newState == PlayerController.PlayerState.Ragdoll) ? RigidbodyConstraints.None : RigidbodyConstraints.FreezeRotation);
		if (newState == PlayerController.PlayerState.Free)
		{
			this._rb.DORotate(Vector3.zero, 0.5f, RotateMode.Fast).SetEase(Ease.OutCubic);
		}
		this._pc.LocalSetInteractable(newState == PlayerController.PlayerState.Ragdoll);
		if (this.sfxPhysicsObject)
		{
			this.sfxPhysicsObject.enabled = (newState == PlayerController.PlayerState.Ragdoll);
		}
	}

	// Token: 0x06001223 RID: 4643 RVA: 0x0004EB95 File Offset: 0x0004CD95
	private IEnumerator DelayedDisableRagdoll()
	{
		if (!this.hasBody)
		{
			yield break;
		}
		yield return new WaitForSeconds(this._ps.ragdollDuration);
		if (!this.hasBody)
		{
			yield break;
		}
		this.State = PlayerController.PlayerState.Free;
		yield break;
	}

	// Token: 0x06001224 RID: 4644 RVA: 0x0004EBA4 File Offset: 0x0004CDA4
	[Server]
	public void ServerTeleport(Vector3 position)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void PlayerController::ServerTeleport(UnityEngine.Vector3)' called when server was not active");
			return;
		}
		this.TargetTeleport(base.netIdentity.connectionToClient, position);
	}

	// Token: 0x06001225 RID: 4645 RVA: 0x0004EBD0 File Offset: 0x0004CDD0
	[TargetRpc]
	private void TargetTeleport(NetworkConnection conn, Vector3 position)
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		writer.WriteVector3(position);
		this.SendTargetRPCInternal(conn, "System.Void PlayerController::TargetTeleport(Mirror.NetworkConnection,UnityEngine.Vector3)", -1471364446, writer, 0);
		NetworkWriterPool.Return(writer);
	}

	// Token: 0x06001226 RID: 4646 RVA: 0x0004EC0A File Offset: 0x0004CE0A
	public void LocalTeleport(Vector3 position)
	{
		base.transform.SetParent(null);
		this._rb.Teleport(position, true);
		this._rb.Rotate(Quaternion.identity, true);
	}

	// Token: 0x06001227 RID: 4647 RVA: 0x0004EC36 File Offset: 0x0004CE36
	[Server]
	public void ServerRotate(Vector2 rotation)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void PlayerController::ServerRotate(UnityEngine.Vector2)' called when server was not active");
			return;
		}
		this.TargetRotate(base.netIdentity.connectionToClient, rotation);
	}

	// Token: 0x06001228 RID: 4648 RVA: 0x0004EC60 File Offset: 0x0004CE60
	[TargetRpc]
	private void TargetRotate(NetworkConnection conn, Vector2 rotation)
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		writer.WriteVector2(rotation);
		this.SendTargetRPCInternal(conn, "System.Void PlayerController::TargetRotate(Mirror.NetworkConnection,UnityEngine.Vector2)", 889457579, writer, 0);
		NetworkWriterPool.Return(writer);
	}

	// Token: 0x06001229 RID: 4649 RVA: 0x0004EC9A File Offset: 0x0004CE9A
	public void LocalRotate(Vector2 rotation)
	{
		base.transform.SetParent(null);
		this.head.SetRotation(rotation.x, rotation.y);
	}

	// Token: 0x0600122A RID: 4650 RVA: 0x0004ECBF File Offset: 0x0004CEBF
	[Server]
	public void ServerKnockback(Vector3 force, Vector3 torque)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void PlayerController::ServerKnockback(UnityEngine.Vector3,UnityEngine.Vector3)' called when server was not active");
			return;
		}
		this._pi.ServerThrowItemRandomly();
		this.TargetKnockback(base.netIdentity.connectionToClient, force, torque);
	}

	// Token: 0x0600122B RID: 4651 RVA: 0x0004ECF4 File Offset: 0x0004CEF4
	[TargetRpc]
	private void TargetKnockback(NetworkConnection conn, Vector3 force, Vector3 torque)
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		writer.WriteVector3(force);
		writer.WriteVector3(torque);
		this.SendTargetRPCInternal(conn, "System.Void PlayerController::TargetKnockback(Mirror.NetworkConnection,UnityEngine.Vector3,UnityEngine.Vector3)", -557766891, writer, 0);
		NetworkWriterPool.Return(writer);
	}

	// Token: 0x0600122C RID: 4652 RVA: 0x0004ED38 File Offset: 0x0004CF38
	public void LocalKnockback(Vector3 force, Vector3 torque)
	{
		if (this.IsLocked)
		{
			return;
		}
		if (this._state == PlayerController.PlayerState.Locked)
		{
			return;
		}
		this.State = PlayerController.PlayerState.Ragdoll;
		this._rb.AddForce(force, ForceMode.VelocityChange);
		this._rb.AddTorque(torque, ForceMode.VelocityChange);
	}

	// Token: 0x0600122D RID: 4653 RVA: 0x0004ED6E File Offset: 0x0004CF6E
	[Server]
	public void ServerLock(bool isLocked)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void PlayerController::ServerLock(System.Boolean)' called when server was not active");
			return;
		}
		this.TargetLock(base.netIdentity.connectionToClient, isLocked);
	}

	// Token: 0x0600122E RID: 4654 RVA: 0x0004ED98 File Offset: 0x0004CF98
	[TargetRpc]
	private void TargetLock(NetworkConnection conn, bool isLocked)
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		writer.WriteBool(isLocked);
		this.SendTargetRPCInternal(conn, "System.Void PlayerController::TargetLock(Mirror.NetworkConnection,System.Boolean)", -1289107260, writer, 0);
		NetworkWriterPool.Return(writer);
	}

	// Token: 0x0600122F RID: 4655 RVA: 0x0004EDD4 File Offset: 0x0004CFD4
	public void LocalLock(bool isLocked)
	{
		base.transform.SetParent(null);
		this.IsLocked = isLocked;
		if (isLocked && !this._rb.isKinematic)
		{
			this._rb.linearVelocity = Vector3.zero;
		}
		this._rb.isKinematic = isLocked;
	}

	// Token: 0x06001230 RID: 4656 RVA: 0x0004EE20 File Offset: 0x0004D020
	[Server]
	public void ServerLockHead(bool isLocked)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void PlayerController::ServerLockHead(System.Boolean)' called when server was not active");
			return;
		}
		this.TargetLockHead(base.netIdentity.connectionToClient, isLocked);
	}

	// Token: 0x06001231 RID: 4657 RVA: 0x0004EE4C File Offset: 0x0004D04C
	[TargetRpc]
	private void TargetLockHead(NetworkConnection conn, bool isLocked)
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		writer.WriteBool(isLocked);
		this.SendTargetRPCInternal(conn, "System.Void PlayerController::TargetLockHead(Mirror.NetworkConnection,System.Boolean)", -1308872738, writer, 0);
		NetworkWriterPool.Return(writer);
	}

	// Token: 0x06001232 RID: 4658 RVA: 0x0004EE86 File Offset: 0x0004D086
	public void LocalLockHead(bool isLocked)
	{
		this.head.isLocked = isLocked;
	}

	// Token: 0x06001233 RID: 4659 RVA: 0x0004EE94 File Offset: 0x0004D094
	[Server]
	public void ServerWakeUp()
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void PlayerController::ServerWakeUp()' called when server was not active");
			return;
		}
		this.TargetWakeUp(base.netIdentity.connectionToClient);
	}

	// Token: 0x06001234 RID: 4660 RVA: 0x0004EEBC File Offset: 0x0004D0BC
	[TargetRpc]
	private void TargetWakeUp(NetworkConnection conn)
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		this.SendTargetRPCInternal(conn, "System.Void PlayerController::TargetWakeUp(Mirror.NetworkConnection)", 1791146995, writer, 0);
		NetworkWriterPool.Return(writer);
	}

	// Token: 0x06001235 RID: 4661 RVA: 0x0004EEEC File Offset: 0x0004D0EC
	private void WakeUp()
	{
		InputEvents.ActiveLayer = InputLayer.Default;
		this.IsLocked = false;
		bool flag = this.State == PlayerController.PlayerState.Ragdoll;
		if (flag)
		{
			this._rb.isKinematic = false;
			this._rb.constraints = RigidbodyConstraints.None;
			if (this.sfxPhysicsObject)
			{
				this.sfxPhysicsObject.enabled = true;
			}
		}
		this.State = PlayerController.PlayerState.Ragdoll;
		if (flag)
		{
			this._rb.AddTorque(base.transform.right * Random.Range(-5f, -5f), ForceMode.VelocityChange);
			Vector3 insideUnitSphere = Random.insideUnitSphere;
			insideUnitSphere.y = 0f;
			insideUnitSphere.Normalize();
			Vector3 force = Vector3.up * 20f + insideUnitSphere * 10f;
			this._rb.AddForce(force, ForceMode.VelocityChange);
			return;
		}
		this._rb.AddTorque(base.transform.right * Random.Range(-5f, -5f), ForceMode.VelocityChange);
		this._rb.AddForce(Vector3.up * 15f + -base.transform.up * 10f, ForceMode.VelocityChange);
	}

	// Token: 0x06001236 RID: 4662 RVA: 0x0004F023 File Offset: 0x0004D223
	private IEnumerator DelayedWakeUp()
	{
		yield return new WaitForSeconds(2.5f);
		this.IsLocked = false;
		InputEvents.ActiveLayer = InputLayer.Default;
		yield break;
	}

	// Token: 0x06001237 RID: 4663 RVA: 0x0004F034 File Offset: 0x0004D234
	public void TriggerRagdoll()
	{
		if (this.IsLocked)
		{
			return;
		}
		if (this.State != PlayerController.PlayerState.Free)
		{
			return;
		}
		this.State = PlayerController.PlayerState.Ragdoll;
		this._rb.AddTorque(new Vector3(Random.Range(-5f, 5f), Random.Range(-5f, 5f), Random.Range(-5f, 5f)), ForceMode.VelocityChange);
		this._rb.AddForce(Vector3.up * 10f + this.head.transform.forward * 10f, ForceMode.VelocityChange);
	}

	// Token: 0x06001238 RID: 4664 RVA: 0x0004F0D2 File Offset: 0x0004D2D2
	private void OnLandFeedback(float fallImpact)
	{
		Action<float> onClientLanded = this.OnClientLanded;
		if (onClientLanded != null)
		{
			onClientLanded(fallImpact);
		}
		this.CmdOnLand(fallImpact);
	}

	// Token: 0x06001239 RID: 4665 RVA: 0x0004F0F0 File Offset: 0x0004D2F0
	[Command]
	private void CmdOnLand(float fallImpact)
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		writer.WriteFloat(fallImpact);
		base.SendCommandInternal("System.Void PlayerController::CmdOnLand(System.Single)", 711888585, writer, 0, true);
		NetworkWriterPool.Return(writer);
	}

	// Token: 0x0600123A RID: 4666 RVA: 0x0004F12C File Offset: 0x0004D32C
	[ClientRpc]
	private void RpcOnLand(float fallImpact)
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		writer.WriteFloat(fallImpact);
		this.SendRPCInternal("System.Void PlayerController::RpcOnLand(System.Single)", -204472168, writer, 0, true);
		NetworkWriterPool.Return(writer);
	}

	// Token: 0x0600123B RID: 4667 RVA: 0x0004F166 File Offset: 0x0004D366
	private void OnJumpFeedback(bool wasGrounded)
	{
		Action<bool> onClientJumped = this.OnClientJumped;
		if (onClientJumped != null)
		{
			onClientJumped(wasGrounded);
		}
		this.CmdOnJump(wasGrounded);
	}

	// Token: 0x0600123C RID: 4668 RVA: 0x0004F184 File Offset: 0x0004D384
	[Command]
	private void CmdOnJump(bool wasGrounded)
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		writer.WriteBool(wasGrounded);
		base.SendCommandInternal("System.Void PlayerController::CmdOnJump(System.Boolean)", 2066950688, writer, 0, true);
		NetworkWriterPool.Return(writer);
	}

	// Token: 0x0600123D RID: 4669 RVA: 0x0004F1C0 File Offset: 0x0004D3C0
	[ClientRpc]
	private void RpcOnJump(bool wasGrounded)
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		writer.WriteBool(wasGrounded);
		this.SendRPCInternal("System.Void PlayerController::RpcOnJump(System.Boolean)", -1493115925, writer, 0, true);
		NetworkWriterPool.Return(writer);
	}

	// Token: 0x0600123F RID: 4671 RVA: 0x00002321 File Offset: 0x00000521
	public override bool Weaved()
	{
		return true;
	}

	// Token: 0x170001A6 RID: 422
	// (get) Token: 0x06001240 RID: 4672 RVA: 0x0004F22C File Offset: 0x0004D42C
	// (set) Token: 0x06001241 RID: 4673 RVA: 0x0004F23F File Offset: 0x0004D43F
	public PlayerController.PlayerState Network_state
	{
		get
		{
			return this._state;
		}
		[param: In]
		set
		{
			base.GeneratedSyncVarSetter<PlayerController.PlayerState>(value, ref this._state, 1UL, null);
		}
	}

	// Token: 0x170001A7 RID: 423
	// (get) Token: 0x06001242 RID: 4674 RVA: 0x0004F25C File Offset: 0x0004D45C
	// (set) Token: 0x06001243 RID: 4675 RVA: 0x0004F26F File Offset: 0x0004D46F
	public bool NetworkhasBody
	{
		get
		{
			return this.hasBody;
		}
		[param: In]
		set
		{
			base.GeneratedSyncVarSetter<bool>(value, ref this.hasBody, 2UL, null);
		}
	}

	// Token: 0x170001A8 RID: 424
	// (get) Token: 0x06001244 RID: 4676 RVA: 0x0004F28C File Offset: 0x0004D48C
	// (set) Token: 0x06001245 RID: 4677 RVA: 0x0004F29F File Offset: 0x0004D49F
	public bool NetworkisGrounded
	{
		get
		{
			return this.isGrounded;
		}
		[param: In]
		set
		{
			base.GeneratedSyncVarSetter<bool>(value, ref this.isGrounded, 4UL, null);
		}
	}

	// Token: 0x170001A9 RID: 425
	// (get) Token: 0x06001246 RID: 4678 RVA: 0x0004F2BC File Offset: 0x0004D4BC
	// (set) Token: 0x06001247 RID: 4679 RVA: 0x0004F2CF File Offset: 0x0004D4CF
	public Vector3 NetworkserverVelocity
	{
		get
		{
			return this.serverVelocity;
		}
		[param: In]
		set
		{
			base.GeneratedSyncVarSetter<Vector3>(value, ref this.serverVelocity, 8UL, null);
		}
	}

	// Token: 0x06001248 RID: 4680 RVA: 0x0004F2E9 File Offset: 0x0004D4E9
	protected void UserCode_TargetTeleport__NetworkConnection__Vector3(NetworkConnection conn, Vector3 position)
	{
		this.LocalTeleport(position);
	}

	// Token: 0x06001249 RID: 4681 RVA: 0x0004F2F2 File Offset: 0x0004D4F2
	protected static void InvokeUserCode_TargetTeleport__NetworkConnection__Vector3(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("TargetRPC TargetTeleport called on server.");
			return;
		}
		((PlayerController)obj).UserCode_TargetTeleport__NetworkConnection__Vector3(null, reader.ReadVector3());
	}

	// Token: 0x0600124A RID: 4682 RVA: 0x0004F31C File Offset: 0x0004D51C
	protected void UserCode_TargetRotate__NetworkConnection__Vector2(NetworkConnection conn, Vector2 rotation)
	{
		this.LocalRotate(rotation);
	}

	// Token: 0x0600124B RID: 4683 RVA: 0x0004F325 File Offset: 0x0004D525
	protected static void InvokeUserCode_TargetRotate__NetworkConnection__Vector2(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("TargetRPC TargetRotate called on server.");
			return;
		}
		((PlayerController)obj).UserCode_TargetRotate__NetworkConnection__Vector2(null, reader.ReadVector2());
	}

	// Token: 0x0600124C RID: 4684 RVA: 0x0004F34F File Offset: 0x0004D54F
	protected void UserCode_TargetKnockback__NetworkConnection__Vector3__Vector3(NetworkConnection conn, Vector3 force, Vector3 torque)
	{
		this.LocalKnockback(force, torque);
	}

	// Token: 0x0600124D RID: 4685 RVA: 0x0004F359 File Offset: 0x0004D559
	protected static void InvokeUserCode_TargetKnockback__NetworkConnection__Vector3__Vector3(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("TargetRPC TargetKnockback called on server.");
			return;
		}
		((PlayerController)obj).UserCode_TargetKnockback__NetworkConnection__Vector3__Vector3(null, reader.ReadVector3(), reader.ReadVector3());
	}

	// Token: 0x0600124E RID: 4686 RVA: 0x0004F389 File Offset: 0x0004D589
	protected void UserCode_TargetLock__NetworkConnection__Boolean(NetworkConnection conn, bool isLocked)
	{
		this.LocalLock(isLocked);
	}

	// Token: 0x0600124F RID: 4687 RVA: 0x0004F392 File Offset: 0x0004D592
	protected static void InvokeUserCode_TargetLock__NetworkConnection__Boolean(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("TargetRPC TargetLock called on server.");
			return;
		}
		((PlayerController)obj).UserCode_TargetLock__NetworkConnection__Boolean(null, reader.ReadBool());
	}

	// Token: 0x06001250 RID: 4688 RVA: 0x0004F3BC File Offset: 0x0004D5BC
	protected void UserCode_TargetLockHead__NetworkConnection__Boolean(NetworkConnection conn, bool isLocked)
	{
		this.LocalLockHead(isLocked);
	}

	// Token: 0x06001251 RID: 4689 RVA: 0x0004F3C5 File Offset: 0x0004D5C5
	protected static void InvokeUserCode_TargetLockHead__NetworkConnection__Boolean(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("TargetRPC TargetLockHead called on server.");
			return;
		}
		((PlayerController)obj).UserCode_TargetLockHead__NetworkConnection__Boolean(null, reader.ReadBool());
	}

	// Token: 0x06001252 RID: 4690 RVA: 0x0004F3EF File Offset: 0x0004D5EF
	protected void UserCode_TargetWakeUp__NetworkConnection(NetworkConnection conn)
	{
		this.WakeUp();
		base.StartCoroutine(this.DelayedWakeUp());
	}

	// Token: 0x06001253 RID: 4691 RVA: 0x0004F404 File Offset: 0x0004D604
	protected static void InvokeUserCode_TargetWakeUp__NetworkConnection(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("TargetRPC TargetWakeUp called on server.");
			return;
		}
		((PlayerController)obj).UserCode_TargetWakeUp__NetworkConnection(null);
	}

	// Token: 0x06001254 RID: 4692 RVA: 0x0004F428 File Offset: 0x0004D628
	protected void UserCode_CmdOnLand__Single(float fallImpact)
	{
		this.RpcOnLand(fallImpact);
	}

	// Token: 0x06001255 RID: 4693 RVA: 0x0004F431 File Offset: 0x0004D631
	protected static void InvokeUserCode_CmdOnLand__Single(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdOnLand called on client.");
			return;
		}
		((PlayerController)obj).UserCode_CmdOnLand__Single(reader.ReadFloat());
	}

	// Token: 0x06001256 RID: 4694 RVA: 0x0004F45B File Offset: 0x0004D65B
	protected void UserCode_RpcOnLand__Single(float fallImpact)
	{
		if (base.isLocalPlayer)
		{
			return;
		}
		Action<float> onClientLanded = this.OnClientLanded;
		if (onClientLanded == null)
		{
			return;
		}
		onClientLanded(fallImpact);
	}

	// Token: 0x06001257 RID: 4695 RVA: 0x0004F477 File Offset: 0x0004D677
	protected static void InvokeUserCode_RpcOnLand__Single(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcOnLand called on server.");
			return;
		}
		((PlayerController)obj).UserCode_RpcOnLand__Single(reader.ReadFloat());
	}

	// Token: 0x06001258 RID: 4696 RVA: 0x0004F4A1 File Offset: 0x0004D6A1
	protected void UserCode_CmdOnJump__Boolean(bool wasGrounded)
	{
		this.RpcOnJump(wasGrounded);
	}

	// Token: 0x06001259 RID: 4697 RVA: 0x0004F4AA File Offset: 0x0004D6AA
	protected static void InvokeUserCode_CmdOnJump__Boolean(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdOnJump called on client.");
			return;
		}
		((PlayerController)obj).UserCode_CmdOnJump__Boolean(reader.ReadBool());
	}

	// Token: 0x0600125A RID: 4698 RVA: 0x0004F4D3 File Offset: 0x0004D6D3
	protected void UserCode_RpcOnJump__Boolean(bool wasGrounded)
	{
		if (base.isLocalPlayer)
		{
			return;
		}
		Action<bool> onClientJumped = this.OnClientJumped;
		if (onClientJumped == null)
		{
			return;
		}
		onClientJumped(wasGrounded);
	}

	// Token: 0x0600125B RID: 4699 RVA: 0x0004F4EF File Offset: 0x0004D6EF
	protected static void InvokeUserCode_RpcOnJump__Boolean(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcOnJump called on server.");
			return;
		}
		((PlayerController)obj).UserCode_RpcOnJump__Boolean(reader.ReadBool());
	}

	// Token: 0x0600125C RID: 4700 RVA: 0x0004F518 File Offset: 0x0004D718
	static PlayerController()
	{
		RemoteProcedureCalls.RegisterCommand(typeof(PlayerController), "System.Void PlayerController::CmdOnLand(System.Single)", new RemoteCallDelegate(PlayerController.InvokeUserCode_CmdOnLand__Single), true);
		RemoteProcedureCalls.RegisterCommand(typeof(PlayerController), "System.Void PlayerController::CmdOnJump(System.Boolean)", new RemoteCallDelegate(PlayerController.InvokeUserCode_CmdOnJump__Boolean), true);
		RemoteProcedureCalls.RegisterRpc(typeof(PlayerController), "System.Void PlayerController::RpcOnLand(System.Single)", new RemoteCallDelegate(PlayerController.InvokeUserCode_RpcOnLand__Single));
		RemoteProcedureCalls.RegisterRpc(typeof(PlayerController), "System.Void PlayerController::RpcOnJump(System.Boolean)", new RemoteCallDelegate(PlayerController.InvokeUserCode_RpcOnJump__Boolean));
		RemoteProcedureCalls.RegisterRpc(typeof(PlayerController), "System.Void PlayerController::TargetTeleport(Mirror.NetworkConnection,UnityEngine.Vector3)", new RemoteCallDelegate(PlayerController.InvokeUserCode_TargetTeleport__NetworkConnection__Vector3));
		RemoteProcedureCalls.RegisterRpc(typeof(PlayerController), "System.Void PlayerController::TargetRotate(Mirror.NetworkConnection,UnityEngine.Vector2)", new RemoteCallDelegate(PlayerController.InvokeUserCode_TargetRotate__NetworkConnection__Vector2));
		RemoteProcedureCalls.RegisterRpc(typeof(PlayerController), "System.Void PlayerController::TargetKnockback(Mirror.NetworkConnection,UnityEngine.Vector3,UnityEngine.Vector3)", new RemoteCallDelegate(PlayerController.InvokeUserCode_TargetKnockback__NetworkConnection__Vector3__Vector3));
		RemoteProcedureCalls.RegisterRpc(typeof(PlayerController), "System.Void PlayerController::TargetLock(Mirror.NetworkConnection,System.Boolean)", new RemoteCallDelegate(PlayerController.InvokeUserCode_TargetLock__NetworkConnection__Boolean));
		RemoteProcedureCalls.RegisterRpc(typeof(PlayerController), "System.Void PlayerController::TargetLockHead(Mirror.NetworkConnection,System.Boolean)", new RemoteCallDelegate(PlayerController.InvokeUserCode_TargetLockHead__NetworkConnection__Boolean));
		RemoteProcedureCalls.RegisterRpc(typeof(PlayerController), "System.Void PlayerController::TargetWakeUp(Mirror.NetworkConnection)", new RemoteCallDelegate(PlayerController.InvokeUserCode_TargetWakeUp__NetworkConnection));
	}

	// Token: 0x0600125D RID: 4701 RVA: 0x0004F668 File Offset: 0x0004D868
	public override void SerializeSyncVars(NetworkWriter writer, bool forceAll)
	{
		base.SerializeSyncVars(writer, forceAll);
		if (forceAll)
		{
			Mirror.GeneratedNetworkCode._Write_PlayerController/PlayerState(writer, this._state);
			writer.WriteBool(this.hasBody);
			writer.WriteBool(this.isGrounded);
			writer.WriteVector3(this.serverVelocity);
			return;
		}
		writer.WriteVarULong(this.syncVarDirtyBits);
		if ((this.syncVarDirtyBits & 1UL) != 0UL)
		{
			Mirror.GeneratedNetworkCode._Write_PlayerController/PlayerState(writer, this._state);
		}
		if ((this.syncVarDirtyBits & 2UL) != 0UL)
		{
			writer.WriteBool(this.hasBody);
		}
		if ((this.syncVarDirtyBits & 4UL) != 0UL)
		{
			writer.WriteBool(this.isGrounded);
		}
		if ((this.syncVarDirtyBits & 8UL) != 0UL)
		{
			writer.WriteVector3(this.serverVelocity);
		}
	}

	// Token: 0x0600125E RID: 4702 RVA: 0x0004F74C File Offset: 0x0004D94C
	public override void DeserializeSyncVars(NetworkReader reader, bool initialState)
	{
		base.DeserializeSyncVars(reader, initialState);
		if (initialState)
		{
			base.GeneratedSyncVarDeserialize<PlayerController.PlayerState>(ref this._state, null, Mirror.GeneratedNetworkCode._Read_PlayerController/PlayerState(reader));
			base.GeneratedSyncVarDeserialize<bool>(ref this.hasBody, null, reader.ReadBool());
			base.GeneratedSyncVarDeserialize<bool>(ref this.isGrounded, null, reader.ReadBool());
			base.GeneratedSyncVarDeserialize<Vector3>(ref this.serverVelocity, null, reader.ReadVector3());
			return;
		}
		long num = (long)reader.ReadVarULong();
		if ((num & 1L) != 0L)
		{
			base.GeneratedSyncVarDeserialize<PlayerController.PlayerState>(ref this._state, null, Mirror.GeneratedNetworkCode._Read_PlayerController/PlayerState(reader));
		}
		if ((num & 2L) != 0L)
		{
			base.GeneratedSyncVarDeserialize<bool>(ref this.hasBody, null, reader.ReadBool());
		}
		if ((num & 4L) != 0L)
		{
			base.GeneratedSyncVarDeserialize<bool>(ref this.isGrounded, null, reader.ReadBool());
		}
		if ((num & 8L) != 0L)
		{
			base.GeneratedSyncVarDeserialize<Vector3>(ref this.serverVelocity, null, reader.ReadVector3());
		}
	}

	// Token: 0x04000BA7 RID: 2983
	[Header("References")]
	public PlayerHead head;

	// Token: 0x04000BA8 RID: 2984
	[SerializeField]
	private SFXPhysicsObject sfxPhysicsObject;

	// Token: 0x04000BA9 RID: 2985
	[SyncVar]
	private PlayerController.PlayerState _state;

	// Token: 0x04000BAA RID: 2986
	[SyncVar]
	public bool hasBody = true;

	// Token: 0x04000BAB RID: 2987
	[SyncVar]
	public bool isGrounded;

	// Token: 0x04000BAC RID: 2988
	private bool _canJump;

	// Token: 0x04000BAD RID: 2989
	private bool _isLocked;

	// Token: 0x04000BAE RID: 2990
	private Rigidbody _rb;

	// Token: 0x04000BAF RID: 2991
	private PlayerSettings _ps;

	// Token: 0x04000BB0 RID: 2992
	private PlayerInventory _pi;

	// Token: 0x04000BB1 RID: 2993
	private PlayerCarry _pc;

	// Token: 0x04000BB2 RID: 2994
	private Vector3 _groundVector = Vector3.down;

	// Token: 0x04000BB3 RID: 2995
	private Vector3 _horizontalMoveDirection;

	// Token: 0x04000BB4 RID: 2996
	private Vector2 _moveInput;

	// Token: 0x04000BB5 RID: 2997
	private float _lastJumpTime;

	// Token: 0x04000BB6 RID: 2998
	private Vector3 _previousVelocity;

	// Token: 0x04000BB7 RID: 2999
	[SyncVar]
	public Vector3 serverVelocity;

	// Token: 0x04000BB8 RID: 3000
	private RaycastHit[] _groundCheckHits = new RaycastHit[8];

	// Token: 0x04000BB9 RID: 3001
	private RaycastHit[] _jumpCheckHits = new RaycastHit[8];

	// Token: 0x04000BBA RID: 3002
	private Coroutine _ragdollRoutine;

	// Token: 0x020001F8 RID: 504
	public enum PlayerState
	{
		// Token: 0x04000BBF RID: 3007
		Free,
		// Token: 0x04000BC0 RID: 3008
		Ragdoll,
		// Token: 0x04000BC1 RID: 3009
		Locked
	}
}
