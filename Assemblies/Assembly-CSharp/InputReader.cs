using System;
using UnityEngine;
using UnityEngine.InputSystem;

// Token: 0x020002BF RID: 703
[CreateAssetMenu(fileName = "New InputReader", menuName = "InputReader")]
public class InputReader : ScriptableObject, InputActions.IPlayerActions
{
	// Token: 0x17000258 RID: 600
	// (get) Token: 0x060018CD RID: 6349 RVA: 0x000688AE File Offset: 0x00066AAE
	// (set) Token: 0x060018CE RID: 6350 RVA: 0x000688B5 File Offset: 0x00066AB5
	public static InputReader Instance { get; private set; }

	// Token: 0x060018CF RID: 6351 RVA: 0x000688BD File Offset: 0x00066ABD
	[RuntimeInitializeOnLoadMethod]
	private static void InitializeInputReader()
	{
		Resources.Load<InputReader>("InputReader");
	}

	// Token: 0x060018D0 RID: 6352 RVA: 0x000688CC File Offset: 0x00066ACC
	private void OnEnable()
	{
		InputReader.Instance = this;
		if (this._inputActions == null)
		{
			this._inputActions = new InputActions();
			this.LoadBindingOverrides();
			this._inputActions.Player.SetCallbacks(this);
			this._inputActions.Player.Enable();
		}
	}

	// Token: 0x060018D1 RID: 6353 RVA: 0x00068920 File Offset: 0x00066B20
	private void OnDisable()
	{
		this.CancelCurrentRebind();
		InputActions inputActions = this._inputActions;
		if (inputActions != null)
		{
			inputActions.Player.Disable();
		}
		if (InputReader.Instance == this)
		{
			InputReader.Instance = null;
		}
	}

	// Token: 0x060018D2 RID: 6354 RVA: 0x0006895C File Offset: 0x00066B5C
	protected override void Finalize()
	{
		try
		{
			this.CancelCurrentRebind();
			InputActions inputActions = this._inputActions;
			if (inputActions != null)
			{
				inputActions.Dispose();
			}
		}
		finally
		{
			base.Finalize();
		}
	}

	// Token: 0x060018D3 RID: 6355 RVA: 0x0006899C File Offset: 0x00066B9C
	public string GetBindingDisplayName(string actionName, int bindingIndex)
	{
		InputAction inputAction;
		if (!this.TryGetAction(actionName, out inputAction))
		{
			return "Unassigned";
		}
		if (bindingIndex < 0 || bindingIndex >= inputAction.bindings.Count)
		{
			return "Unassigned";
		}
		return inputAction.GetBindingDisplayString(bindingIndex, InputBinding.DisplayStringOptions.DontUseShortDisplayNames);
	}

	// Token: 0x060018D4 RID: 6356 RVA: 0x000689E0 File Offset: 0x00066BE0
	public string GetBindingEffectivePath(string actionName, int bindingIndex)
	{
		InputAction inputAction;
		if (!this.TryGetAction(actionName, out inputAction))
		{
			return string.Empty;
		}
		if (bindingIndex < 0 || bindingIndex >= inputAction.bindings.Count)
		{
			return string.Empty;
		}
		return inputAction.bindings[bindingIndex].effectivePath;
	}

	// Token: 0x060018D5 RID: 6357 RVA: 0x00068A30 File Offset: 0x00066C30
	public bool ApplyBindingOverride(string actionName, int bindingIndex, string overridePath)
	{
		InputAction inputAction;
		if (!this.TryGetAction(actionName, out inputAction))
		{
			return false;
		}
		if (bindingIndex < 0 || bindingIndex >= inputAction.bindings.Count)
		{
			return false;
		}
		inputAction.ApplyBindingOverride(bindingIndex, overridePath);
		this.SaveBindingOverrides();
		return true;
	}

	// Token: 0x060018D6 RID: 6358 RVA: 0x00068A70 File Offset: 0x00066C70
	public bool StartInteractiveRebind(string actionName, int bindingIndex, Action<string> onComplete, Action onCancelled = null)
	{
		InputAction inputAction;
		if (!this.TryGetAction(actionName, out inputAction))
		{
			return false;
		}
		if (bindingIndex < 0 || bindingIndex >= inputAction.bindings.Count)
		{
			return false;
		}
		InputBinding inputBinding = inputAction.bindings[bindingIndex];
		if (inputBinding.isComposite || inputBinding.isPartOfComposite)
		{
			return false;
		}
		this.CancelCurrentRebind();
		this._inputActions.Player.Disable();
		this._rebindOperation = inputAction.PerformInteractiveRebinding(bindingIndex).WithCancelingThrough("<Keyboard>/escape").OnComplete(delegate(InputActionRebindingExtensions.RebindingOperation op)
		{
			this.SaveBindingOverrides();
			string bindingDisplayName = this.GetBindingDisplayName(actionName, bindingIndex);
			this.FinishRebindOperation();
			Action<string> onComplete2 = onComplete;
			if (onComplete2 == null)
			{
				return;
			}
			onComplete2(bindingDisplayName);
		}).OnCancel(delegate(InputActionRebindingExtensions.RebindingOperation op)
		{
			this.FinishRebindOperation();
			Action onCancelled2 = onCancelled;
			if (onCancelled2 == null)
			{
				return;
			}
			onCancelled2();
		});
		this._rebindOperation.Start();
		return true;
	}

	// Token: 0x060018D7 RID: 6359 RVA: 0x00068B68 File Offset: 0x00066D68
	private void FinishRebindOperation()
	{
		InputActionRebindingExtensions.RebindingOperation rebindOperation = this._rebindOperation;
		if (rebindOperation != null)
		{
			rebindOperation.Dispose();
		}
		this._rebindOperation = null;
		InputActions inputActions = this._inputActions;
		if (inputActions == null)
		{
			return;
		}
		inputActions.Player.Enable();
	}

	// Token: 0x060018D8 RID: 6360 RVA: 0x00068BA5 File Offset: 0x00066DA5
	private void CancelCurrentRebind()
	{
		if (this._rebindOperation == null)
		{
			return;
		}
		this._rebindOperation.Cancel();
		this._rebindOperation.Dispose();
		this._rebindOperation = null;
	}

	// Token: 0x060018D9 RID: 6361 RVA: 0x00068BCD File Offset: 0x00066DCD
	private bool TryGetAction(string actionName, out InputAction action)
	{
		action = null;
		if (this._inputActions == null || string.IsNullOrWhiteSpace(actionName))
		{
			return false;
		}
		action = this._inputActions.asset.FindAction(actionName, false);
		return action != null;
	}

	// Token: 0x060018DA RID: 6362 RVA: 0x00068C00 File Offset: 0x00066E00
	private void LoadBindingOverrides()
	{
		string @string = PlayerPrefs.GetString("input.bindingOverrides", string.Empty);
		if (string.IsNullOrWhiteSpace(@string))
		{
			return;
		}
		this._inputActions.asset.LoadBindingOverridesFromJson(@string, true);
	}

	// Token: 0x060018DB RID: 6363 RVA: 0x00068C38 File Offset: 0x00066E38
	private void SaveBindingOverrides()
	{
		string value = this._inputActions.asset.SaveBindingOverridesAsJson();
		PlayerPrefs.SetString("input.bindingOverrides", value);
		PlayerPrefs.Save();
	}

	// Token: 0x060018DC RID: 6364 RVA: 0x00068C68 File Offset: 0x00066E68
	public void OnMove(InputAction.CallbackContext context)
	{
		if (InputEvents.ActiveLayer != InputLayer.Default)
		{
			return;
		}
		if (!context.performed)
		{
			if (context.canceled)
			{
				Action<Vector2> onMoveEvent = InputEvents.OnMoveEvent;
				if (onMoveEvent == null)
				{
					return;
				}
				onMoveEvent(Vector2.zero);
			}
			return;
		}
		Action<Vector2> onMoveEvent2 = InputEvents.OnMoveEvent;
		if (onMoveEvent2 == null)
		{
			return;
		}
		onMoveEvent2(context.ReadValue<Vector2>());
	}

	// Token: 0x060018DD RID: 6365 RVA: 0x00068CBC File Offset: 0x00066EBC
	public void OnAim(InputAction.CallbackContext context)
	{
		if (InputEvents.ActiveLayer != InputLayer.Default && InputEvents.ActiveLayer != InputLayer.SpawnBox)
		{
			return;
		}
		if (!context.performed)
		{
			if (context.canceled)
			{
				Action<Vector2> onAimEvent = InputEvents.OnAimEvent;
				if (onAimEvent == null)
				{
					return;
				}
				onAimEvent(Vector2.zero);
			}
			return;
		}
		Action<Vector2> onAimEvent2 = InputEvents.OnAimEvent;
		if (onAimEvent2 == null)
		{
			return;
		}
		onAimEvent2(context.ReadValue<Vector2>());
	}

	// Token: 0x060018DE RID: 6366 RVA: 0x00068D16 File Offset: 0x00066F16
	public void OnJump(InputAction.CallbackContext context)
	{
		if (InputEvents.ActiveLayer == InputLayer.Default && (context.performed || context.canceled))
		{
			InputEvents.UpdateJump(context.ReadValueAsButton());
		}
		Action<bool> onAnyInputEvent = InputEvents.OnAnyInputEvent;
		if (onAnyInputEvent == null)
		{
			return;
		}
		onAnyInputEvent(context.ReadValueAsButton());
	}

	// Token: 0x060018DF RID: 6367 RVA: 0x00068D53 File Offset: 0x00066F53
	public void OnCrouch(InputAction.CallbackContext context)
	{
		if (InputEvents.ActiveLayer != InputLayer.Default)
		{
			return;
		}
		if (context.performed || context.canceled)
		{
			InputEvents.UpdateCrouch(context.ReadValueAsButton());
		}
	}

	// Token: 0x060018E0 RID: 6368 RVA: 0x00068D7B File Offset: 0x00066F7B
	public void OnSprint(InputAction.CallbackContext context)
	{
		if (InputEvents.ActiveLayer != InputLayer.Default)
		{
			return;
		}
		if (context.performed || context.canceled)
		{
			InputEvents.UpdateSprint(context.ReadValueAsButton());
		}
	}

	// Token: 0x060018E1 RID: 6369 RVA: 0x00068DA4 File Offset: 0x00066FA4
	public void OnInteract(InputAction.CallbackContext context)
	{
		if ((InputEvents.ActiveLayer == InputLayer.Default || InputEvents.ActiveLayer == InputLayer.SpawnBox) && (context.performed || context.canceled))
		{
			InputEvents.UpdateInteract(context.ReadValueAsButton());
		}
		Action<bool> onAnyInputEvent = InputEvents.OnAnyInputEvent;
		if (onAnyInputEvent == null)
		{
			return;
		}
		onAnyInputEvent(context.ReadValueAsButton());
	}

	// Token: 0x060018E2 RID: 6370 RVA: 0x00068DF4 File Offset: 0x00066FF4
	public void OnZoom(InputAction.CallbackContext context)
	{
		if (InputEvents.ActiveLayer == InputLayer.Default && (context.performed || context.canceled))
		{
			InputEvents.UpdateZoom(context.ReadValueAsButton());
		}
		Action<bool> onAnyInputEvent = InputEvents.OnAnyInputEvent;
		if (onAnyInputEvent == null)
		{
			return;
		}
		onAnyInputEvent(context.ReadValueAsButton());
	}

	// Token: 0x060018E3 RID: 6371 RVA: 0x00068E34 File Offset: 0x00067034
	public void OnItemSelect(InputAction.CallbackContext context)
	{
		if (InputEvents.ActiveLayer != InputLayer.Default)
		{
			return;
		}
		if (!context.performed)
		{
			return;
		}
		string displayName = context.control.displayName;
		if (!(displayName == "1"))
		{
			if (!(displayName == "2"))
			{
				if (!(displayName == "3"))
				{
					return;
				}
				Action<int> onItemSelectEvent = InputEvents.OnItemSelectEvent;
				if (onItemSelectEvent == null)
				{
					return;
				}
				onItemSelectEvent(3);
				return;
			}
			else
			{
				Action<int> onItemSelectEvent2 = InputEvents.OnItemSelectEvent;
				if (onItemSelectEvent2 == null)
				{
					return;
				}
				onItemSelectEvent2(2);
				return;
			}
		}
		else
		{
			Action<int> onItemSelectEvent3 = InputEvents.OnItemSelectEvent;
			if (onItemSelectEvent3 == null)
			{
				return;
			}
			onItemSelectEvent3(1);
			return;
		}
	}

	// Token: 0x060018E4 RID: 6372 RVA: 0x00068EBC File Offset: 0x000670BC
	public void OnScroll(InputAction.CallbackContext context)
	{
		if (InputEvents.ActiveLayer != InputLayer.Default)
		{
			return;
		}
		if (context.performed)
		{
			float num = context.ReadValue<float>();
			if (num > 0f)
			{
				num = 1f;
			}
			else if (num < 0f)
			{
				num = -1f;
			}
			else
			{
				num = 0f;
			}
			Action<int> onScrollEvent = InputEvents.OnScrollEvent;
			if (onScrollEvent == null)
			{
				return;
			}
			onScrollEvent((int)num);
		}
	}

	// Token: 0x060018E5 RID: 6373 RVA: 0x00068F19 File Offset: 0x00067119
	public void OnUseItem(InputAction.CallbackContext context)
	{
		if (InputEvents.ActiveLayer == InputLayer.Default && (context.performed || context.canceled))
		{
			InputEvents.UpdateUseItem(context.ReadValueAsButton());
		}
		Action<bool> onAnyInputEvent = InputEvents.OnAnyInputEvent;
		if (onAnyInputEvent == null)
		{
			return;
		}
		onAnyInputEvent(context.ReadValueAsButton());
	}

	// Token: 0x060018E6 RID: 6374 RVA: 0x00068F56 File Offset: 0x00067156
	public void OnThrowItem(InputAction.CallbackContext context)
	{
		if (InputEvents.ActiveLayer != InputLayer.Default)
		{
			return;
		}
		if (context.performed || context.canceled)
		{
			InputEvents.UpdateThrowItem(context.ReadValueAsButton());
		}
	}

	// Token: 0x060018E7 RID: 6375 RVA: 0x00068F7E File Offset: 0x0006717E
	public void OnConsole(InputAction.CallbackContext context)
	{
		if (context.performed)
		{
			InputEvents.TryInvokeConsole();
		}
	}

	// Token: 0x060018E8 RID: 6376 RVA: 0x00068F8E File Offset: 0x0006718E
	public void OnEscapeMenu(InputAction.CallbackContext context)
	{
		if (context.performed)
		{
			Action onEscapeMenuEvent = InputEvents.OnEscapeMenuEvent;
			if (onEscapeMenuEvent == null)
			{
				return;
			}
			onEscapeMenuEvent();
		}
	}

	// Token: 0x060018E9 RID: 6377 RVA: 0x00068FA8 File Offset: 0x000671A8
	public void OnEmoteWheel(InputAction.CallbackContext context)
	{
		if (InputEvents.ActiveLayer != InputLayer.Default)
		{
			return;
		}
		if (context.performed || context.canceled)
		{
			InputEvents.UpdateEmoteWheel(context.ReadValueAsButton());
		}
	}

	// Token: 0x060018EA RID: 6378 RVA: 0x00068FD0 File Offset: 0x000671D0
	public void OnF1(InputAction.CallbackContext context)
	{
		if (context.performed)
		{
			InputEvents.TryInvokeF1();
		}
	}

	// Token: 0x060018EB RID: 6379 RVA: 0x00068FE0 File Offset: 0x000671E0
	public void OnF2(InputAction.CallbackContext context)
	{
		if (context.performed)
		{
			InputEvents.TryInvokeF2();
		}
	}

	// Token: 0x060018EC RID: 6380 RVA: 0x00068FF0 File Offset: 0x000671F0
	public void OnF3(InputAction.CallbackContext context)
	{
		if (context.performed)
		{
			InputEvents.TryInvokeF3();
		}
	}

	// Token: 0x060018ED RID: 6381 RVA: 0x00069000 File Offset: 0x00067200
	public void OnF4(InputAction.CallbackContext context)
	{
		if (context.performed)
		{
			InputEvents.TryInvokeF4();
		}
	}

	// Token: 0x060018EE RID: 6382 RVA: 0x00069010 File Offset: 0x00067210
	public void OnPing(InputAction.CallbackContext context)
	{
		if (InputEvents.ActiveLayer != InputLayer.Default)
		{
			return;
		}
		if (context.performed)
		{
			InputEvents.UpdatePing(context.ReadValueAsButton());
		}
	}

	// Token: 0x060018EF RID: 6383 RVA: 0x0006902F File Offset: 0x0006722F
	public void OnSkipUI(InputAction.CallbackContext context)
	{
		if (InputEvents.ActiveLayer == InputLayer.Cutscene && (context.performed || context.canceled))
		{
			InputEvents.UpdateSkipUI(context.ReadValueAsButton());
		}
		Action<bool> onAnyInputEvent = InputEvents.OnAnyInputEvent;
		if (onAnyInputEvent == null)
		{
			return;
		}
		onAnyInputEvent(context.ReadValueAsButton());
	}

	// Token: 0x060018F0 RID: 6384 RVA: 0x0006906D File Offset: 0x0006726D
	public void OnPushToTalk(InputAction.CallbackContext context)
	{
		if (InputEvents.ActiveLayer != InputLayer.Default)
		{
			return;
		}
		if (context.performed || context.canceled)
		{
			InputEvents.UpdatePushToTalk(context.ReadValueAsButton());
		}
		Action<bool> onAnyInputEvent = InputEvents.OnAnyInputEvent;
		if (onAnyInputEvent == null)
		{
			return;
		}
		onAnyInputEvent(context.ReadValueAsButton());
	}

	// Token: 0x04001007 RID: 4103
	private const string BindingOverridesPlayerPrefsKey = "input.bindingOverrides";

	// Token: 0x04001008 RID: 4104
	private InputActions _inputActions;

	// Token: 0x04001009 RID: 4105
	private InputActionRebindingExtensions.RebindingOperation _rebindOperation;
}
