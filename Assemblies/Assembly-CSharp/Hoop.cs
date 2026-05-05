using System;
using System.Collections.Generic;
using Mirror;
using Mirror.RemoteCalls;
using MoreMountains.Feedbacks;
using UnityEngine;
using UnityEngine.Events;

// Token: 0x020000C5 RID: 197
public class Hoop : NetworkBehaviour
{
	// Token: 0x0600075C RID: 1884 RVA: 0x0001EC18 File Offset: 0x0001CE18
	private void OnTriggerEnter(Collider other)
	{
		if (!base.isServer)
		{
			return;
		}
		if (!other.attachedRigidbody)
		{
			return;
		}
		Basketball basketball;
		if (!other.attachedRigidbody.TryGetComponent<Basketball>(out basketball))
		{
			return;
		}
		if (basketball.transform.position.y < this.hoopCenterPosition.position.y)
		{
			other.attachedRigidbody.linearVelocity *= 0.3f;
			other.attachedRigidbody.AddForce(Vector3.down * 100f, ForceMode.Acceleration);
			return;
		}
		if (!this._entryPositions.ContainsKey(basketball))
		{
			this._entryPositions[basketball] = basketball.transform.position;
		}
	}

	// Token: 0x0600075D RID: 1885 RVA: 0x0001ECCC File Offset: 0x0001CECC
	private void OnTriggerExit(Collider other)
	{
		if (!base.isServer)
		{
			return;
		}
		if (!other.attachedRigidbody)
		{
			return;
		}
		Basketball basketball;
		if (!other.attachedRigidbody.TryGetComponent<Basketball>(out basketball))
		{
			return;
		}
		Vector3 vector;
		if (this._entryPositions.TryGetValue(basketball, out vector))
		{
			if (vector.y > this.hoopCenterPosition.position.y && basketball.transform.position.y < this.hoopCenterPosition.position.y)
			{
				this.RegisterHoop(basketball);
			}
			this._entryPositions.Remove(basketball);
		}
	}

	// Token: 0x0600075E RID: 1886 RVA: 0x0001ED5E File Offset: 0x0001CF5E
	private void RegisterHoop(Basketball ball)
	{
		this.RpcRegisterHoop();
		UnityEvent unityEvent = this.onHoopRegistered;
		if (unityEvent == null)
		{
			return;
		}
		unityEvent.Invoke();
	}

	// Token: 0x0600075F RID: 1887 RVA: 0x0001ED78 File Offset: 0x0001CF78
	[ClientRpc]
	private void RpcRegisterHoop()
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		this.SendRPCInternal("System.Void Hoop::RpcRegisterHoop()", -1963564451, writer, 0, true);
		NetworkWriterPool.Return(writer);
	}

	// Token: 0x06000761 RID: 1889 RVA: 0x00002321 File Offset: 0x00000521
	public override bool Weaved()
	{
		return true;
	}

	// Token: 0x06000762 RID: 1890 RVA: 0x0001EDBB File Offset: 0x0001CFBB
	protected void UserCode_RpcRegisterHoop()
	{
		this.registerHoopFb.PlayFeedbacks();
	}

	// Token: 0x06000763 RID: 1891 RVA: 0x0001EDC8 File Offset: 0x0001CFC8
	protected static void InvokeUserCode_RpcRegisterHoop(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcRegisterHoop called on server.");
			return;
		}
		((Hoop)obj).UserCode_RpcRegisterHoop();
	}

	// Token: 0x06000764 RID: 1892 RVA: 0x0001EDEB File Offset: 0x0001CFEB
	static Hoop()
	{
		RemoteProcedureCalls.RegisterRpc(typeof(Hoop), "System.Void Hoop::RpcRegisterHoop()", new RemoteCallDelegate(Hoop.InvokeUserCode_RpcRegisterHoop));
	}

	// Token: 0x040004F0 RID: 1264
	[SerializeField]
	private Transform hoopCenterPosition;

	// Token: 0x040004F1 RID: 1265
	[SerializeField]
	private MMF_Player registerHoopFb;

	// Token: 0x040004F2 RID: 1266
	public UnityEvent onHoopRegistered;

	// Token: 0x040004F3 RID: 1267
	private readonly Dictionary<Item, Vector3> _entryPositions = new Dictionary<Item, Vector3>();
}
