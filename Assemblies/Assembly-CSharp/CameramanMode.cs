using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Extensions;
using Mirror;
using Mirror.RemoteCalls;
using UnityEngine;

// Token: 0x020001EB RID: 491
public class CameramanMode : NetworkBehaviour
{
	// Token: 0x06001182 RID: 4482 RVA: 0x0004B8E4 File Offset: 0x00049AE4
	private void Awake()
	{
		this._renderers.AddRange(base.GetComponentsInChildren<Renderer>());
		this._canvases.AddRange(base.GetComponentsInChildren<Canvas>());
		this._pc = base.GetComponent<PlayerController>();
		this._rb = base.GetComponent<Rigidbody>();
		this._head = base.GetComponentInChildren<PlayerHead>();
	}

	// Token: 0x06001183 RID: 4483 RVA: 0x0004B937 File Offset: 0x00049B37
	public override void OnStartClient()
	{
		base.OnStartClient();
		if (base.isLocalPlayer && MonoSingleton<LocalManager>.Instance != null)
		{
			this._mainCamera = MonoSingleton<LocalManager>.Instance.mainCamera;
		}
		this.UpdateUI();
		this.UpdateVisibility();
	}

	// Token: 0x06001184 RID: 4484 RVA: 0x0004B970 File Offset: 0x00049B70
	private void UpdateUI()
	{
		foreach (Canvas canvas in this._canvases)
		{
			if (canvas != null)
			{
				canvas.enabled = !this._isActive;
			}
		}
		if (base.isLocalPlayer && MonoSingleton<LocalManager>.Instance != null && MonoSingleton<LocalManager>.Instance.playerEyesUI != null)
		{
			MonoSingleton<LocalManager>.Instance.playerEyesUI.gameObject.SetActive(!this._isActive);
		}
	}

	// Token: 0x06001185 RID: 4485 RVA: 0x0004BA18 File Offset: 0x00049C18
	private void UpdateVisibility()
	{
		foreach (Renderer renderer in this._renderers)
		{
			if (renderer != null)
			{
				renderer.enabled = this._isVisible;
			}
		}
	}

	// Token: 0x06001186 RID: 4486 RVA: 0x0004BA7C File Offset: 0x00049C7C
	public void ToggleCameramanMode()
	{
		if (!base.isLocalPlayer)
		{
			return;
		}
		this.CmdSetCameramanMode(!this._isActive);
	}

	// Token: 0x06001187 RID: 4487 RVA: 0x0004BA96 File Offset: 0x00049C96
	public void ToggleVisibility()
	{
		if (!base.isLocalPlayer)
		{
			return;
		}
		this.CmdSetVisibility(!this._isVisible);
	}

	// Token: 0x06001188 RID: 4488 RVA: 0x0004BAB0 File Offset: 0x00049CB0
	[Command(requiresAuthority = false)]
	private void CmdSetCameramanMode(bool active)
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		writer.WriteBool(active);
		base.SendCommandInternal("System.Void CameramanMode::CmdSetCameramanMode(System.Boolean)", 1531626262, writer, 0, false);
		NetworkWriterPool.Return(writer);
	}

	// Token: 0x06001189 RID: 4489 RVA: 0x0004BAEC File Offset: 0x00049CEC
	[Command(requiresAuthority = false)]
	private void CmdSetVisibility(bool visible)
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		writer.WriteBool(visible);
		base.SendCommandInternal("System.Void CameramanMode::CmdSetVisibility(System.Boolean)", -1250602304, writer, 0, false);
		NetworkWriterPool.Return(writer);
	}

	// Token: 0x0600118A RID: 4490 RVA: 0x0004BB26 File Offset: 0x00049D26
	private void OnCameramanModeChanged(bool oldValue, bool newValue)
	{
		if (base.isLocalPlayer)
		{
			if (newValue)
			{
				this.EnableCameramanMode();
			}
			else
			{
				this.DisableCameramanMode();
			}
		}
		this.UpdateUI();
	}

	// Token: 0x0600118B RID: 4491 RVA: 0x0004BB47 File Offset: 0x00049D47
	private void OnVisibilityChanged(bool oldValue, bool newValue)
	{
		this.UpdateVisibility();
	}

	// Token: 0x0600118C RID: 4492 RVA: 0x0004BB50 File Offset: 0x00049D50
	private void EnableCameramanMode()
	{
		if (!base.isLocalPlayer)
		{
			return;
		}
		this._savedBodyPosition = base.transform.position;
		this._savedBodyRotation = base.transform.rotation;
		this._wasBodyKinematic = this._rb.isKinematic;
		this._originalConstraints = this._rb.constraints;
		this.ToggleAllSelfMeshDisablers(true);
		this._rb.isKinematic = true;
		this._rb.constraints = RigidbodyConstraints.FreezeAll;
		this._rb.linearVelocity = Vector3.zero;
		this._rb.angularVelocity = Vector3.zero;
		if (this._pc != null)
		{
			this._pc.enabled = false;
		}
		if (this._head != null)
		{
			this._wasHeadEnabled = this._head.enabled;
			this._head.enabled = false;
			this._head.isLocked = true;
		}
		if (this._mainCamera != null)
		{
			this._mainCamera.gameObject.SetActive(false);
		}
		this.DisableIdentifiedCanvases();
		if (this.cameramanCameraPrefab != null)
		{
			Vector3 position = (this._head != null) ? this._head.transform.position : base.transform.position;
			Quaternion rotation = (this._head != null) ? this._head.transform.rotation : base.transform.rotation;
			this._cameramanCameraInstance = Object.Instantiate<GameObject>(this.cameramanCameraPrefab, position, rotation);
			CameramanNoclipCamera component = this._cameramanCameraInstance.GetComponent<CameramanNoclipCamera>();
			if (component != null)
			{
				component.InitializeRotation(position, rotation);
				return;
			}
		}
		else
		{
			Debug.LogWarning("CameramanCameraPrefab is not assigned in CameramanMode!");
		}
	}

	// Token: 0x0600118D RID: 4493 RVA: 0x0004BD04 File Offset: 0x00049F04
	private void DisableCameramanMode()
	{
		if (!base.isLocalPlayer)
		{
			return;
		}
		if (this._cameramanCameraInstance != null)
		{
			Object.Destroy(this._cameramanCameraInstance);
			this._cameramanCameraInstance = null;
		}
		if (this._mainCamera != null)
		{
			this._mainCamera.gameObject.SetActive(true);
		}
		this.EnableIdentifiedCanvases();
		base.transform.position = this._savedBodyPosition;
		base.transform.rotation = this._savedBodyRotation;
		this._rb.isKinematic = this._wasBodyKinematic;
		this._rb.constraints = this._originalConstraints;
		this.ToggleAllSelfMeshDisablers(false);
		if (this._pc != null)
		{
			this._pc.enabled = true;
		}
		if (this._head != null)
		{
			this._head.enabled = this._wasHeadEnabled;
			this._head.isLocked = false;
		}
	}

	// Token: 0x0600118E RID: 4494 RVA: 0x0004BDF0 File Offset: 0x00049FF0
	private void ToggleAllSelfMeshDisablers(bool enabled)
	{
		this._selfMeshDisablers.Clear();
		foreach (SelfMeshDisabler selfMeshDisabler in Object.FindObjectsByType<SelfMeshDisabler>(FindObjectsInactive.Include, FindObjectsSortMode.None))
		{
			if (selfMeshDisabler != null)
			{
				this._selfMeshDisablers.Add(selfMeshDisabler);
				selfMeshDisabler.ToggleMesh(enabled);
			}
		}
	}

	// Token: 0x0600118F RID: 4495 RVA: 0x0004BE40 File Offset: 0x0004A040
	private void DisableIdentifiedCanvases()
	{
		this._identifiedCanvases.Clear();
		foreach (Canvas canvas in Object.FindObjectsByType<Canvas>(FindObjectsInactive.Include, FindObjectsSortMode.None))
		{
			if (canvas != null && canvas.GetComponent<CanvasIdentifier>() != null)
			{
				this._identifiedCanvases.Add(new ValueTuple<Canvas, bool>(canvas, canvas.enabled));
				canvas.enabled = false;
			}
		}
	}

	// Token: 0x06001190 RID: 4496 RVA: 0x0004BEA8 File Offset: 0x0004A0A8
	private void EnableIdentifiedCanvases()
	{
		foreach (ValueTuple<Canvas, bool> valueTuple in this._identifiedCanvases)
		{
			Canvas item = valueTuple.Item1;
			bool item2 = valueTuple.Item2;
			if (item != null)
			{
				item.enabled = item2;
			}
		}
		this._identifiedCanvases.Clear();
	}

	// Token: 0x06001191 RID: 4497 RVA: 0x0004BF1C File Offset: 0x0004A11C
	public void ToggleCameramanCanvas()
	{
		if (!base.isLocalPlayer)
		{
			return;
		}
		if (this._cameramanCameraInstance != null)
		{
			CameramanNoclipCamera component = this._cameramanCameraInstance.GetComponent<CameramanNoclipCamera>();
			if (component != null)
			{
				component.ToggleCanvas();
			}
		}
	}

	// Token: 0x06001192 RID: 4498 RVA: 0x0004BF5C File Offset: 0x0004A15C
	public bool IsCameramanCanvasActive()
	{
		if (!base.isLocalPlayer || this._cameramanCameraInstance == null)
		{
			return false;
		}
		CameramanNoclipCamera component = this._cameramanCameraInstance.GetComponent<CameramanNoclipCamera>();
		return component != null && component.IsCanvasActive();
	}

	// Token: 0x1700019D RID: 413
	// (get) Token: 0x06001193 RID: 4499 RVA: 0x0004BF9E File Offset: 0x0004A19E
	public bool IsActive
	{
		get
		{
			return this._isActive;
		}
	}

	// Token: 0x1700019E RID: 414
	// (get) Token: 0x06001194 RID: 4500 RVA: 0x0004BFA6 File Offset: 0x0004A1A6
	public bool IsVisible
	{
		get
		{
			return this._isVisible;
		}
	}

	// Token: 0x06001195 RID: 4501 RVA: 0x0004BFB0 File Offset: 0x0004A1B0
	public CameramanMode()
	{
		this._Mirror_SyncVarHookDelegate__isActive = new Action<bool, bool>(this.OnCameramanModeChanged);
		this._Mirror_SyncVarHookDelegate__isVisible = new Action<bool, bool>(this.OnVisibilityChanged);
	}

	// Token: 0x06001196 RID: 4502 RVA: 0x00002321 File Offset: 0x00000521
	public override bool Weaved()
	{
		return true;
	}

	// Token: 0x1700019F RID: 415
	// (get) Token: 0x06001197 RID: 4503 RVA: 0x0004C01C File Offset: 0x0004A21C
	// (set) Token: 0x06001198 RID: 4504 RVA: 0x0004C02F File Offset: 0x0004A22F
	public bool Network_isActive
	{
		get
		{
			return this._isActive;
		}
		[param: In]
		set
		{
			base.GeneratedSyncVarSetter<bool>(value, ref this._isActive, 1UL, this._Mirror_SyncVarHookDelegate__isActive);
		}
	}

	// Token: 0x170001A0 RID: 416
	// (get) Token: 0x06001199 RID: 4505 RVA: 0x0004C050 File Offset: 0x0004A250
	// (set) Token: 0x0600119A RID: 4506 RVA: 0x0004C063 File Offset: 0x0004A263
	public bool Network_isVisible
	{
		get
		{
			return this._isVisible;
		}
		[param: In]
		set
		{
			base.GeneratedSyncVarSetter<bool>(value, ref this._isVisible, 2UL, this._Mirror_SyncVarHookDelegate__isVisible);
		}
	}

	// Token: 0x0600119B RID: 4507 RVA: 0x0004C082 File Offset: 0x0004A282
	protected void UserCode_CmdSetCameramanMode__Boolean(bool active)
	{
		this.Network_isActive = active;
	}

	// Token: 0x0600119C RID: 4508 RVA: 0x0004C08B File Offset: 0x0004A28B
	protected static void InvokeUserCode_CmdSetCameramanMode__Boolean(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdSetCameramanMode called on client.");
			return;
		}
		((CameramanMode)obj).UserCode_CmdSetCameramanMode__Boolean(reader.ReadBool());
	}

	// Token: 0x0600119D RID: 4509 RVA: 0x0004C0B4 File Offset: 0x0004A2B4
	protected void UserCode_CmdSetVisibility__Boolean(bool visible)
	{
		this.Network_isVisible = visible;
	}

	// Token: 0x0600119E RID: 4510 RVA: 0x0004C0BD File Offset: 0x0004A2BD
	protected static void InvokeUserCode_CmdSetVisibility__Boolean(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdSetVisibility called on client.");
			return;
		}
		((CameramanMode)obj).UserCode_CmdSetVisibility__Boolean(reader.ReadBool());
	}

	// Token: 0x0600119F RID: 4511 RVA: 0x0004C0E8 File Offset: 0x0004A2E8
	static CameramanMode()
	{
		RemoteProcedureCalls.RegisterCommand(typeof(CameramanMode), "System.Void CameramanMode::CmdSetCameramanMode(System.Boolean)", new RemoteCallDelegate(CameramanMode.InvokeUserCode_CmdSetCameramanMode__Boolean), false);
		RemoteProcedureCalls.RegisterCommand(typeof(CameramanMode), "System.Void CameramanMode::CmdSetVisibility(System.Boolean)", new RemoteCallDelegate(CameramanMode.InvokeUserCode_CmdSetVisibility__Boolean), false);
	}

	// Token: 0x060011A0 RID: 4512 RVA: 0x0004C138 File Offset: 0x0004A338
	public override void SerializeSyncVars(NetworkWriter writer, bool forceAll)
	{
		base.SerializeSyncVars(writer, forceAll);
		if (forceAll)
		{
			writer.WriteBool(this._isActive);
			writer.WriteBool(this._isVisible);
			return;
		}
		writer.WriteVarULong(this.syncVarDirtyBits);
		if ((this.syncVarDirtyBits & 1UL) != 0UL)
		{
			writer.WriteBool(this._isActive);
		}
		if ((this.syncVarDirtyBits & 2UL) != 0UL)
		{
			writer.WriteBool(this._isVisible);
		}
	}

	// Token: 0x060011A1 RID: 4513 RVA: 0x0004C1C0 File Offset: 0x0004A3C0
	public override void DeserializeSyncVars(NetworkReader reader, bool initialState)
	{
		base.DeserializeSyncVars(reader, initialState);
		if (initialState)
		{
			base.GeneratedSyncVarDeserialize<bool>(ref this._isActive, this._Mirror_SyncVarHookDelegate__isActive, reader.ReadBool());
			base.GeneratedSyncVarDeserialize<bool>(ref this._isVisible, this._Mirror_SyncVarHookDelegate__isVisible, reader.ReadBool());
			return;
		}
		long num = (long)reader.ReadVarULong();
		if ((num & 1L) != 0L)
		{
			base.GeneratedSyncVarDeserialize<bool>(ref this._isActive, this._Mirror_SyncVarHookDelegate__isActive, reader.ReadBool());
		}
		if ((num & 2L) != 0L)
		{
			base.GeneratedSyncVarDeserialize<bool>(ref this._isVisible, this._Mirror_SyncVarHookDelegate__isVisible, reader.ReadBool());
		}
	}

	// Token: 0x04000B45 RID: 2885
	[SyncVar(hook = "OnCameramanModeChanged")]
	private bool _isActive;

	// Token: 0x04000B46 RID: 2886
	[SyncVar(hook = "OnVisibilityChanged")]
	private bool _isVisible = true;

	// Token: 0x04000B47 RID: 2887
	private List<Renderer> _renderers = new List<Renderer>();

	// Token: 0x04000B48 RID: 2888
	private List<Canvas> _canvases = new List<Canvas>();

	// Token: 0x04000B49 RID: 2889
	[Header("Cameraman Settings")]
	[SerializeField]
	private GameObject cameramanCameraPrefab;

	// Token: 0x04000B4A RID: 2890
	private PlayerController _pc;

	// Token: 0x04000B4B RID: 2891
	private Rigidbody _rb;

	// Token: 0x04000B4C RID: 2892
	private PlayerHead _head;

	// Token: 0x04000B4D RID: 2893
	private Camera _mainCamera;

	// Token: 0x04000B4E RID: 2894
	private GameObject _cameramanCameraInstance;

	// Token: 0x04000B4F RID: 2895
	private Vector3 _savedBodyPosition;

	// Token: 0x04000B50 RID: 2896
	private Quaternion _savedBodyRotation;

	// Token: 0x04000B51 RID: 2897
	private bool _wasBodyKinematic;

	// Token: 0x04000B52 RID: 2898
	private RigidbodyConstraints _originalConstraints;

	// Token: 0x04000B53 RID: 2899
	private bool _wasHeadEnabled;

	// Token: 0x04000B54 RID: 2900
	[TupleElementNames(new string[]
	{
		"canvas",
		"wasEnabled"
	})]
	private List<ValueTuple<Canvas, bool>> _identifiedCanvases = new List<ValueTuple<Canvas, bool>>();

	// Token: 0x04000B55 RID: 2901
	private List<SelfMeshDisabler> _selfMeshDisablers = new List<SelfMeshDisabler>();

	// Token: 0x04000B56 RID: 2902
	public Action<bool, bool> _Mirror_SyncVarHookDelegate__isActive;

	// Token: 0x04000B57 RID: 2903
	public Action<bool, bool> _Mirror_SyncVarHookDelegate__isVisible;
}
