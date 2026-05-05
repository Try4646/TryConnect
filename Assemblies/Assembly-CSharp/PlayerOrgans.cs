using System;
using DG.Tweening;
using Extensions;
using Mirror;
using Mirror.RemoteCalls;
using UnityEngine;

// Token: 0x02000207 RID: 519
public class PlayerOrgans : NetworkBehaviour
{
	// Token: 0x170001B7 RID: 439
	// (get) Token: 0x06001326 RID: 4902 RVA: 0x00052E8E File Offset: 0x0005108E
	public Transform LeftEye
	{
		get
		{
			return this.leftEyeModel.transform.parent;
		}
	}

	// Token: 0x170001B8 RID: 440
	// (get) Token: 0x06001327 RID: 4903 RVA: 0x00052EA0 File Offset: 0x000510A0
	public Transform RightEye
	{
		get
		{
			return this.rightEyeModel.transform.parent;
		}
	}

	// Token: 0x06001328 RID: 4904 RVA: 0x00052EB4 File Offset: 0x000510B4
	private void Awake()
	{
		this._pc = base.GetComponent<PlayerController>();
		this._pp = base.GetComponent<PlayerProfile>();
		this._rb = base.GetComponent<Rigidbody>();
		this._cd = base.GetComponent<CustomDrag>();
		this._ps = Resources.Load<PlayerSettings>("PlayerSettings");
		this._pe = MonoSingleton<LocalManager>.Instance.playerEyesUI;
	}

	// Token: 0x06001329 RID: 4905 RVA: 0x00052F11 File Offset: 0x00051111
	private void OnEnable()
	{
		PlayerProfile pp = this._pp;
		pp.OnPlayerProfileUpdated = (Action)Delegate.Combine(pp.OnPlayerProfileUpdated, new Action(this.OnProfileSync));
	}

	// Token: 0x0600132A RID: 4906 RVA: 0x00052F3A File Offset: 0x0005113A
	private void OnDisable()
	{
		PlayerProfile pp = this._pp;
		pp.OnPlayerProfileUpdated = (Action)Delegate.Remove(pp.OnPlayerProfileUpdated, new Action(this.OnProfileSync));
	}

	// Token: 0x0600132B RID: 4907 RVA: 0x00052F63 File Offset: 0x00051163
	private void OnProfileSync()
	{
		if (base.isServer)
		{
			NetworkSingleton<OrganManager>.Instance.ServerRegisterPlayer(this);
		}
	}

	// Token: 0x0600132C RID: 4908 RVA: 0x00052F78 File Offset: 0x00051178
	[Server]
	public void ServerSetBodyParts(PlayerOrganData data)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void PlayerOrgans::ServerSetBodyParts(PlayerOrganData)' called when server was not active");
			return;
		}
		this.SetEyes(data);
		this.SetBody(data);
		this.SetMouth(data);
	}

	// Token: 0x0600132D RID: 4909 RVA: 0x00052FA4 File Offset: 0x000511A4
	[Server]
	private void SetEyes(PlayerOrganData data)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void PlayerOrgans::SetEyes(PlayerOrganData)' called when server was not active");
			return;
		}
		this.RpcSetEyes(data.leftEye, data.rightEye);
	}

	// Token: 0x0600132E RID: 4910 RVA: 0x00052FD0 File Offset: 0x000511D0
	[ClientRpc]
	private void RpcSetEyes(bool leftEye, bool rightEye)
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		writer.WriteBool(leftEye);
		writer.WriteBool(rightEye);
		this.SendRPCInternal("System.Void PlayerOrgans::RpcSetEyes(System.Boolean,System.Boolean)", 1242183557, writer, 0, true);
		NetworkWriterPool.Return(writer);
	}

	// Token: 0x0600132F RID: 4911 RVA: 0x00053014 File Offset: 0x00051214
	[Server]
	private void SetBody(PlayerOrganData data)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void PlayerOrgans::SetBody(PlayerOrganData)' called when server was not active");
			return;
		}
		this.RpcSetBody(data.body);
	}

	// Token: 0x06001330 RID: 4912 RVA: 0x00053038 File Offset: 0x00051238
	[ClientRpc]
	private void RpcSetBody(bool isEnabled)
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		writer.WriteBool(isEnabled);
		this.SendRPCInternal("System.Void PlayerOrgans::RpcSetBody(System.Boolean)", 876516184, writer, 0, true);
		NetworkWriterPool.Return(writer);
	}

	// Token: 0x06001331 RID: 4913 RVA: 0x00053074 File Offset: 0x00051274
	[Server]
	private void SetMouth(PlayerOrganData data)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void PlayerOrgans::SetMouth(PlayerOrganData)' called when server was not active");
			return;
		}
		this.RpcSetMouth(data.mouth);
		PlayerVoiceFX playerVoiceFX;
		if (base.TryGetComponent<PlayerVoiceFX>(out playerVoiceFX))
		{
			playerVoiceFX.RpcSetNoMouthFX(!data.mouth);
		}
	}

	// Token: 0x06001332 RID: 4914 RVA: 0x000530BC File Offset: 0x000512BC
	[ClientRpc]
	private void RpcSetMouth(bool isEnabled)
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		writer.WriteBool(isEnabled);
		this.SendRPCInternal("System.Void PlayerOrgans::RpcSetMouth(System.Boolean)", 2028909947, writer, 0, true);
		NetworkWriterPool.Return(writer);
	}

	// Token: 0x06001334 RID: 4916 RVA: 0x00002321 File Offset: 0x00000521
	public override bool Weaved()
	{
		return true;
	}

	// Token: 0x06001335 RID: 4917 RVA: 0x0005311C File Offset: 0x0005131C
	protected void UserCode_RpcSetEyes__Boolean__Boolean(bool leftEye, bool rightEye)
	{
		if (this._localLeftEye == leftEye && this._localRightEye == rightEye)
		{
			return;
		}
		this._localLeftEye = leftEye;
		this._localRightEye = rightEye;
		this.leftEyeModel.SetActive(leftEye);
		this.rightEyeModel.SetActive(rightEye);
		if (base.isLocalPlayer)
		{
			this._pe.ToggleEye(false, leftEye);
			this._pe.ToggleEye(true, rightEye);
		}
	}

	// Token: 0x06001336 RID: 4918 RVA: 0x00053184 File Offset: 0x00051384
	protected static void InvokeUserCode_RpcSetEyes__Boolean__Boolean(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcSetEyes called on server.");
			return;
		}
		((PlayerOrgans)obj).UserCode_RpcSetEyes__Boolean__Boolean(reader.ReadBool(), reader.ReadBool());
	}

	// Token: 0x06001337 RID: 4919 RVA: 0x000531B4 File Offset: 0x000513B4
	protected void UserCode_RpcSetBody__Boolean(bool isEnabled)
	{
		if (this._localBody == isEnabled)
		{
			return;
		}
		this._localBody = isEnabled;
		this.bodyModel.SetActive(isEnabled);
		if (base.isLocalPlayer)
		{
			this._pc.NetworkhasBody = isEnabled;
			this._pc.State = (isEnabled ? PlayerController.PlayerState.Free : PlayerController.PlayerState.Ragdoll);
			if (isEnabled)
			{
				base.transform.DOMove(base.transform.position, 0.5f, false);
			}
			this._pc.head.transform.DOLocalMove(isEnabled ? (Vector3.up * this._ps.headHeight) : Vector3.zero, 0.5f, false);
		}
		this._cd.angularDrag = (isEnabled ? new Vector3(0.5f, 20f, 0.5f) : new Vector3(5f, 20f, 5f));
		this._rb.centerOfMass = (isEnabled ? (Vector3.up * 0.9f) : Vector3.zero);
	}

	// Token: 0x06001338 RID: 4920 RVA: 0x000532BB File Offset: 0x000514BB
	protected static void InvokeUserCode_RpcSetBody__Boolean(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcSetBody called on server.");
			return;
		}
		((PlayerOrgans)obj).UserCode_RpcSetBody__Boolean(reader.ReadBool());
	}

	// Token: 0x06001339 RID: 4921 RVA: 0x000532E4 File Offset: 0x000514E4
	protected void UserCode_RpcSetMouth__Boolean(bool isEnabled)
	{
		if (this._localMouth == isEnabled)
		{
			return;
		}
		this._localMouth = isEnabled;
		this.mouthModel.GetComponent<MeshRenderer>().enabled = isEnabled;
		if (base.isLocalPlayer)
		{
			base.GetComponent<PlayerMouth>().enabled = isEnabled;
		}
	}

	// Token: 0x0600133A RID: 4922 RVA: 0x0005331C File Offset: 0x0005151C
	protected static void InvokeUserCode_RpcSetMouth__Boolean(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcSetMouth called on server.");
			return;
		}
		((PlayerOrgans)obj).UserCode_RpcSetMouth__Boolean(reader.ReadBool());
	}

	// Token: 0x0600133B RID: 4923 RVA: 0x00053348 File Offset: 0x00051548
	static PlayerOrgans()
	{
		RemoteProcedureCalls.RegisterRpc(typeof(PlayerOrgans), "System.Void PlayerOrgans::RpcSetEyes(System.Boolean,System.Boolean)", new RemoteCallDelegate(PlayerOrgans.InvokeUserCode_RpcSetEyes__Boolean__Boolean));
		RemoteProcedureCalls.RegisterRpc(typeof(PlayerOrgans), "System.Void PlayerOrgans::RpcSetBody(System.Boolean)", new RemoteCallDelegate(PlayerOrgans.InvokeUserCode_RpcSetBody__Boolean));
		RemoteProcedureCalls.RegisterRpc(typeof(PlayerOrgans), "System.Void PlayerOrgans::RpcSetMouth(System.Boolean)", new RemoteCallDelegate(PlayerOrgans.InvokeUserCode_RpcSetMouth__Boolean));
	}

	// Token: 0x04000C36 RID: 3126
	[Header("References")]
	[SerializeField]
	private GameObject leftEyeModel;

	// Token: 0x04000C37 RID: 3127
	[SerializeField]
	private GameObject rightEyeModel;

	// Token: 0x04000C38 RID: 3128
	[SerializeField]
	private GameObject bodyModel;

	// Token: 0x04000C39 RID: 3129
	[SerializeField]
	private GameObject mouthModel;

	// Token: 0x04000C3A RID: 3130
	private bool _localLeftEye = true;

	// Token: 0x04000C3B RID: 3131
	private bool _localRightEye = true;

	// Token: 0x04000C3C RID: 3132
	private bool _localBody = true;

	// Token: 0x04000C3D RID: 3133
	private bool _localMouth = true;

	// Token: 0x04000C3E RID: 3134
	private PlayerController _pc;

	// Token: 0x04000C3F RID: 3135
	private PlayerProfile _pp;

	// Token: 0x04000C40 RID: 3136
	private Rigidbody _rb;

	// Token: 0x04000C41 RID: 3137
	private CustomDrag _cd;

	// Token: 0x04000C42 RID: 3138
	private PlayerSettings _ps;

	// Token: 0x04000C43 RID: 3139
	private PlayerEyesUI _pe;
}
