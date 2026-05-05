using System;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using Mirror;
using Mirror.RemoteCalls;
using UnityEngine;

// Token: 0x020001FF RID: 511
public class PlayerHandButtonAnimation : NetworkBehaviour
{
	// Token: 0x060012A7 RID: 4775 RVA: 0x00050A8C File Offset: 0x0004EC8C
	private void Awake()
	{
		this._originalLocalPositions = new Vector3[this.handBones.Length];
		for (int i = 0; i < this.handBones.Length; i++)
		{
			if (this.handBones[i] != null)
			{
				this._originalLocalPositions[i] = this.handBones[i].localPosition;
			}
		}
		this._originalLocalRotations = new Vector3[this.handBones.Length];
		for (int j = 0; j < this.handBones.Length; j++)
		{
			if (this.handBones[j] != null)
			{
				this._originalLocalRotations[j] = this.handBones[j].localEulerAngles;
			}
		}
	}

	// Token: 0x060012A8 RID: 4776 RVA: 0x00050B38 File Offset: 0x0004ED38
	public void PressButton(Transform buttonTransform)
	{
		if (this.handBones == null || this.handBones.Length == 0)
		{
			return;
		}
		if (!buttonTransform)
		{
			return;
		}
		int num = Random.Range(0, this.handBones.Length);
		bool flag = num == 0;
		int num2 = flag ? 1 : -1;
		Vector3 buttonPos = buttonTransform.position + buttonTransform.forward * this.pressDistance;
		Quaternion rhs = Quaternion.Euler(this.pressRotation.x, this.pressRotation.y * (float)num2, this.pressRotation.z * (float)num2);
		Quaternion buttonRot = buttonTransform.rotation * rhs;
		string trigger = flag ? "PressRight" : "PressLeft";
		this.networkAnimator.SetTrigger(trigger);
		this.HandBoneAnimation(num, buttonPos, buttonRot);
		this.CmdPressButton(num, buttonPos, buttonRot);
	}

	// Token: 0x060012A9 RID: 4777 RVA: 0x00050C04 File Offset: 0x0004EE04
	[Command]
	private void CmdPressButton(int handIndex, Vector3 buttonPos, Quaternion buttonRot)
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		writer.WriteVarInt(handIndex);
		writer.WriteVector3(buttonPos);
		writer.WriteQuaternion(buttonRot);
		base.SendCommandInternal("System.Void PlayerHandButtonAnimation::CmdPressButton(System.Int32,UnityEngine.Vector3,UnityEngine.Quaternion)", 434843827, writer, 0, true);
		NetworkWriterPool.Return(writer);
	}

	// Token: 0x060012AA RID: 4778 RVA: 0x00050C54 File Offset: 0x0004EE54
	[ClientRpc]
	private void RpcPressButton(int handIndex, Vector3 buttonPos, Quaternion buttonRot)
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		writer.WriteVarInt(handIndex);
		writer.WriteVector3(buttonPos);
		writer.WriteQuaternion(buttonRot);
		this.SendRPCInternal("System.Void PlayerHandButtonAnimation::RpcPressButton(System.Int32,UnityEngine.Vector3,UnityEngine.Quaternion)", -483286476, writer, 0, true);
		NetworkWriterPool.Return(writer);
	}

	// Token: 0x060012AB RID: 4779 RVA: 0x00050CA4 File Offset: 0x0004EEA4
	private void HandBoneAnimation(int handIndex, Vector3 buttonPos, Quaternion buttonRot)
	{
		Transform transform = this.handBones[handIndex];
		if (!transform)
		{
			return;
		}
		transform.DOKill(true);
		Sequence s = DOTween.Sequence();
		s.Append(transform.DOMove(buttonPos, this.moveDuration, false).SetEase(Ease.OutCirc));
		s.Join(transform.DORotateQuaternion(buttonRot, this.moveDuration).SetEase(Ease.OutCirc));
		s.Append(transform.DOLocalMove(this._originalLocalPositions[handIndex], this.moveDuration, false).SetEase(Ease.InOutCirc));
		s.Join(transform.DOLocalRotate(this._originalLocalRotations[handIndex], this.moveDuration, RotateMode.Fast).SetEase(Ease.InOutCirc));
	}

	// Token: 0x060012AC RID: 4780 RVA: 0x00050D54 File Offset: 0x0004EF54
	public void LocalResetHands()
	{
		for (int i = 0; i < this.handBones.Length; i++)
		{
			Transform transform = this.handBones[i];
			transform.DOKill(false);
			transform.localPosition = this._originalLocalPositions[i];
			transform.localEulerAngles = this._originalLocalRotations[i];
		}
	}

	// Token: 0x060012AE RID: 4782 RVA: 0x00002321 File Offset: 0x00000521
	public override bool Weaved()
	{
		return true;
	}

	// Token: 0x060012AF RID: 4783 RVA: 0x00050DD1 File Offset: 0x0004EFD1
	protected void UserCode_CmdPressButton__Int32__Vector3__Quaternion(int handIndex, Vector3 buttonPos, Quaternion buttonRot)
	{
		this.RpcPressButton(handIndex, buttonPos, buttonRot);
	}

	// Token: 0x060012B0 RID: 4784 RVA: 0x00050DDC File Offset: 0x0004EFDC
	protected static void InvokeUserCode_CmdPressButton__Int32__Vector3__Quaternion(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdPressButton called on client.");
			return;
		}
		((PlayerHandButtonAnimation)obj).UserCode_CmdPressButton__Int32__Vector3__Quaternion(reader.ReadVarInt(), reader.ReadVector3(), reader.ReadQuaternion());
	}

	// Token: 0x060012B1 RID: 4785 RVA: 0x00050E11 File Offset: 0x0004F011
	protected void UserCode_RpcPressButton__Int32__Vector3__Quaternion(int handIndex, Vector3 buttonPos, Quaternion buttonRot)
	{
		if (base.isLocalPlayer)
		{
			return;
		}
		this.HandBoneAnimation(handIndex, buttonPos, buttonRot);
	}

	// Token: 0x060012B2 RID: 4786 RVA: 0x00050E25 File Offset: 0x0004F025
	protected static void InvokeUserCode_RpcPressButton__Int32__Vector3__Quaternion(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcPressButton called on server.");
			return;
		}
		((PlayerHandButtonAnimation)obj).UserCode_RpcPressButton__Int32__Vector3__Quaternion(reader.ReadVarInt(), reader.ReadVector3(), reader.ReadQuaternion());
	}

	// Token: 0x060012B3 RID: 4787 RVA: 0x00050E5C File Offset: 0x0004F05C
	static PlayerHandButtonAnimation()
	{
		RemoteProcedureCalls.RegisterCommand(typeof(PlayerHandButtonAnimation), "System.Void PlayerHandButtonAnimation::CmdPressButton(System.Int32,UnityEngine.Vector3,UnityEngine.Quaternion)", new RemoteCallDelegate(PlayerHandButtonAnimation.InvokeUserCode_CmdPressButton__Int32__Vector3__Quaternion), true);
		RemoteProcedureCalls.RegisterRpc(typeof(PlayerHandButtonAnimation), "System.Void PlayerHandButtonAnimation::RpcPressButton(System.Int32,UnityEngine.Vector3,UnityEngine.Quaternion)", new RemoteCallDelegate(PlayerHandButtonAnimation.InvokeUserCode_RpcPressButton__Int32__Vector3__Quaternion));
	}

	// Token: 0x04000BE9 RID: 3049
	[Header("References")]
	[SerializeField]
	private Transform[] handBones = new Transform[2];

	// Token: 0x04000BEA RID: 3050
	[SerializeField]
	private NetworkAnimator networkAnimator;

	// Token: 0x04000BEB RID: 3051
	[Header("Animation Settings")]
	[SerializeField]
	private float moveDuration = 0.1f;

	// Token: 0x04000BEC RID: 3052
	[SerializeField]
	private float pressDistance = 0.1f;

	// Token: 0x04000BED RID: 3053
	[SerializeField]
	private Vector3 pressRotation;

	// Token: 0x04000BEE RID: 3054
	private Vector3[] _originalLocalPositions;

	// Token: 0x04000BEF RID: 3055
	private Vector3[] _originalLocalRotations;
}
