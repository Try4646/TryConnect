using System;
using Extensions;
using Mirror;
using UnityEngine;

// Token: 0x02000200 RID: 512
public class PlayerHead : NetworkBehaviour
{
	// Token: 0x170001B0 RID: 432
	// (get) Token: 0x060012B4 RID: 4788 RVA: 0x00050EAA File Offset: 0x0004F0AA
	private bool IsFree
	{
		get
		{
			return !this._pc.hasBody && this._pc.State == PlayerController.PlayerState.Ragdoll;
		}
	}

	// Token: 0x060012B5 RID: 4789 RVA: 0x00050EC9 File Offset: 0x0004F0C9
	private void Awake()
	{
		this._cs = Resources.Load<CameraSettings>("CameraSettings");
		this._settingsLayout = Resources.Load<SettingsLayout>("SettingsLayout");
		this._pc = base.GetComponentInParent<PlayerController>();
		this._targetRotation = Quaternion.identity;
	}

	// Token: 0x060012B6 RID: 4790 RVA: 0x00050F04 File Offset: 0x0004F104
	public override void OnStartClient()
	{
		base.OnStartClient();
		if (!base.isLocalPlayer)
		{
			base.enabled = false;
			return;
		}
		InputEvents.OnAimEvent = (Action<Vector2>)Delegate.Combine(InputEvents.OnAimEvent, new Action<Vector2>(this.OnLook));
		SettingItemBase.SettingsChanged += this.OnSettingChanged;
		this.CacheInvertSettings();
	}

	// Token: 0x060012B7 RID: 4791 RVA: 0x00050F60 File Offset: 0x0004F160
	public override void OnStopClient()
	{
		base.OnStopClient();
		if (!base.isLocalPlayer)
		{
			return;
		}
		InputEvents.OnAimEvent = (Action<Vector2>)Delegate.Remove(InputEvents.OnAimEvent, new Action<Vector2>(this.OnLook));
		SettingItemBase.SettingsChanged -= this.OnSettingChanged;
	}

	// Token: 0x060012B8 RID: 4792 RVA: 0x00050FB0 File Offset: 0x0004F1B0
	private void OnSettingChanged(SettingItemBase setting)
	{
		ToggleSettingItem toggleSettingItem = setting as ToggleSettingItem;
		if (toggleSettingItem != null && !string.IsNullOrWhiteSpace(toggleSettingItem.key))
		{
			string a = toggleSettingItem.key.Trim();
			if (string.Equals(a, "invertX", StringComparison.OrdinalIgnoreCase))
			{
				this._invertX = toggleSettingItem.value;
				return;
			}
			if (string.Equals(a, "invertY", StringComparison.OrdinalIgnoreCase))
			{
				this._invertY = toggleSettingItem.value;
			}
		}
	}

	// Token: 0x060012B9 RID: 4793 RVA: 0x00051015 File Offset: 0x0004F215
	private void OnLook(Vector2 input)
	{
		this._lookInput = input;
	}

	// Token: 0x060012BA RID: 4794 RVA: 0x0005101E File Offset: 0x0004F21E
	private void Update()
	{
		this.GetInput();
		this.RotateHead();
	}

	// Token: 0x060012BB RID: 4795 RVA: 0x0005102C File Offset: 0x0004F22C
	private void GetInput()
	{
		if (this.isLocked)
		{
			return;
		}
		object obj = MonoSingleton<InputModeManager>.Instance && MonoSingleton<InputModeManager>.Instance.IsControllerActive();
		float num = Mathf.Min(Time.deltaTime, 0.033f);
		object obj2 = obj;
		float num2;
		if (obj2 != null)
		{
			num2 = (InputEvents.IsZoomPressed ? (this._cs.controllerSensitivity ? (this._cs.controllerSensitivity.value * 0.6f) : this._cs.zoomSensitivity) : (this._cs.controllerSensitivity ? this._cs.controllerSensitivity.value : this._cs.sensitivity.value));
		}
		else
		{
			num2 = (InputEvents.IsZoomPressed ? (this._cs.zoomSensitivity / 30f) : (this._cs.sensitivity.value / 30f));
		}
		float num3 = num2;
		float num4 = this._lookInput.x * (this._invertX ? -1f : 1f);
		float num5 = this._lookInput.y * (this._invertY ? -1f : 1f);
		float num6 = num4 * num3;
		float num7 = -num5 * num3;
		if (obj2 != null)
		{
			num6 *= num;
			num7 *= num;
		}
		if (!this.IsFree)
		{
			this.SetRotation(this._yaw + num6, this._pitch + num7);
			return;
		}
		Quaternion lhs = Quaternion.AngleAxis(num6, this._targetRotation * Vector3.up) * Quaternion.AngleAxis(num7, this._targetRotation * Vector3.right);
		this.SetRotationFree(lhs * this._targetRotation);
	}

	// Token: 0x060012BC RID: 4796 RVA: 0x000511D8 File Offset: 0x0004F3D8
	private void CacheInvertSettings()
	{
		this._invertX = this.GetInvertSetting("invertX");
		this._invertY = this.GetInvertSetting("invertY");
	}

	// Token: 0x060012BD RID: 4797 RVA: 0x000511FC File Offset: 0x0004F3FC
	private bool GetInvertSetting(string key)
	{
		if (this._settingsLayout == null)
		{
			return false;
		}
		foreach (SettingsLayout.Tab tab in this._settingsLayout.tabs)
		{
			if (tab != null)
			{
				foreach (SettingItemBase settingItemBase in tab.entries)
				{
					ToggleSettingItem toggleSettingItem = settingItemBase as ToggleSettingItem;
					if (toggleSettingItem != null && !string.IsNullOrWhiteSpace(toggleSettingItem.key) && string.Equals(toggleSettingItem.key.Trim(), key, StringComparison.OrdinalIgnoreCase))
					{
						return toggleSettingItem.value;
					}
				}
			}
		}
		return false;
	}

	// Token: 0x060012BE RID: 4798 RVA: 0x000512D4 File Offset: 0x0004F4D4
	private void RotateHead()
	{
		float cameraLerp = this._cs.cameraLerp;
		float t = 1f - Mathf.Exp(-cameraLerp * Time.deltaTime);
		base.transform.localRotation = Quaternion.Slerp(base.transform.localRotation, this._targetRotation, t);
	}

	// Token: 0x060012BF RID: 4799 RVA: 0x00051324 File Offset: 0x0004F524
	public void SetRotation(float yaw, float pitch)
	{
		this._yaw = Mathf.DeltaAngle(0f, yaw);
		this._pitch = Mathf.Clamp(pitch, -89f, 89f);
		this._targetRotation = Quaternion.Euler(this._pitch, this._yaw, 0f);
	}

	// Token: 0x060012C0 RID: 4800 RVA: 0x00051374 File Offset: 0x0004F574
	public void SetRotationFree(Quaternion rotation)
	{
		this._targetRotation = rotation;
	}

	// Token: 0x060012C1 RID: 4801 RVA: 0x0005137D File Offset: 0x0004F57D
	public Vector2 GetRotation()
	{
		return new Vector2(this._yaw, this._pitch);
	}

	// Token: 0x060012C3 RID: 4803 RVA: 0x00002321 File Offset: 0x00000521
	public override bool Weaved()
	{
		return true;
	}

	// Token: 0x04000BF0 RID: 3056
	public bool isLocked;

	// Token: 0x04000BF1 RID: 3057
	private PlayerController _pc;

	// Token: 0x04000BF2 RID: 3058
	private CameraSettings _cs;

	// Token: 0x04000BF3 RID: 3059
	private SettingsLayout _settingsLayout;

	// Token: 0x04000BF4 RID: 3060
	private Vector2 _lookInput;

	// Token: 0x04000BF5 RID: 3061
	private float _yaw;

	// Token: 0x04000BF6 RID: 3062
	private float _pitch;

	// Token: 0x04000BF7 RID: 3063
	private Quaternion _targetRotation = Quaternion.identity;

	// Token: 0x04000BF8 RID: 3064
	private bool _invertX;

	// Token: 0x04000BF9 RID: 3065
	private bool _invertY;
}
