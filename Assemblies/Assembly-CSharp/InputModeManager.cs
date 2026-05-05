using System;
using System.Collections;
using System.Collections.Generic;
using Extensions;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

// Token: 0x02000174 RID: 372
public class InputModeManager : MonoSingleton<InputModeManager>
{
	// Token: 0x17000142 RID: 322
	// (get) Token: 0x06000E1B RID: 3611 RVA: 0x0003A7A6 File Offset: 0x000389A6
	public InputMode CurrentInputMode
	{
		get
		{
			return this.currentInputMode;
		}
	}

	// Token: 0x06000E1C RID: 3612 RVA: 0x0003A7AE File Offset: 0x000389AE
	protected override void OnAwake()
	{
		base.OnAwake();
		this.EnsureUIInputModule();
	}

	// Token: 0x06000E1D RID: 3613 RVA: 0x0003A7BC File Offset: 0x000389BC
	private void Start()
	{
		if (UICursor.Instance != null)
		{
			UICursor.Instance.SetInputModeEnabled(this.currentInputMode == InputMode.KeyboardMouse);
		}
	}

	// Token: 0x06000E1E RID: 3614 RVA: 0x0003A7DE File Offset: 0x000389DE
	private void Update()
	{
		this.CheckInputDevice();
	}

	// Token: 0x06000E1F RID: 3615 RVA: 0x0003A7E8 File Offset: 0x000389E8
	private void EnsureUIInputModule()
	{
		if (EventSystem.current == null)
		{
			new GameObject("EventSystem").AddComponent<EventSystem>();
		}
		this.uiInputModule = EventSystem.current.GetComponent<InputSystemUIInputModule>();
		if (this.uiInputModule == null)
		{
			this.uiInputModule = EventSystem.current.gameObject.AddComponent<InputSystemUIInputModule>();
		}
		this.legacyInputModule = EventSystem.current.GetComponent<StandaloneInputModule>();
		if (this.legacyInputModule != null)
		{
			this.legacyInputModule.enabled = false;
		}
		this.uiInputModule.enabled = true;
	}

	// Token: 0x06000E20 RID: 3616 RVA: 0x0003A87C File Offset: 0x00038A7C
	private void CheckInputDevice()
	{
		bool flag = false;
		bool flag2 = false;
		Keyboard current = Keyboard.current;
		Mouse current2 = Mouse.current;
		Gamepad current3 = Gamepad.current;
		if (current != null && current.anyKey.wasPressedThisFrame)
		{
			flag = true;
		}
		if (current2 != null)
		{
			if (current2.leftButton.wasPressedThisFrame || current2.rightButton.wasPressedThisFrame || current2.middleButton.wasPressedThisFrame || current2.scroll.ReadValue().magnitude > this.mouseScrollSwitchThreshold)
			{
				flag = true;
			}
			else if (current2.delta.ReadValue().magnitude > this.mouseDeltaSwitchThreshold)
			{
				flag = true;
			}
		}
		if (current3 != null)
		{
			Vector2 vector = current3.leftStick.ReadValue();
			Vector2 vector2 = current3.rightStick.ReadValue();
			float magnitude = vector.magnitude;
			float magnitude2 = vector2.magnitude;
			if ((magnitude > this.joystickDeadzone && magnitude >= this.minJoystickMagnitude) || (magnitude2 > this.joystickDeadzone && magnitude2 >= this.minJoystickMagnitude))
			{
				flag2 = true;
			}
			if (current3.buttonSouth.wasPressedThisFrame || current3.buttonNorth.wasPressedThisFrame || current3.buttonEast.wasPressedThisFrame || current3.buttonWest.wasPressedThisFrame || current3.dpad.ReadValue().magnitude > this.joystickDeadzone || current3.leftShoulder.wasPressedThisFrame || current3.rightShoulder.wasPressedThisFrame || current3.leftTrigger.ReadValue() > this.joystickThreshold || current3.rightTrigger.ReadValue() > this.joystickThreshold || current3.startButton.wasPressedThisFrame || current3.selectButton.wasPressedThisFrame)
			{
				flag2 = true;
			}
		}
		InputMode inputMode = this.currentInputMode;
		if (flag)
		{
			inputMode = InputMode.KeyboardMouse;
		}
		else if (flag2)
		{
			inputMode = InputMode.Controller;
		}
		if (inputMode != this.currentInputMode)
		{
			this.SetInputMode(inputMode);
		}
	}

	// Token: 0x06000E21 RID: 3617 RVA: 0x0003AA64 File Offset: 0x00038C64
	private void SetInputMode(InputMode newMode)
	{
		this.currentInputMode = newMode;
		Action<InputMode> onInputModeChanged = InputModeManager.OnInputModeChanged;
		if (onInputModeChanged != null)
		{
			onInputModeChanged(newMode);
		}
		if (UICursor.Instance != null)
		{
			UICursor.Instance.SetInputModeEnabled(newMode == InputMode.KeyboardMouse);
		}
		if (this.uiInputModule != null)
		{
			this.uiInputModule.enabled = true;
		}
		if (newMode == InputMode.Controller)
		{
			base.StartCoroutine(this.SelectBestSelectableNextFrame());
			return;
		}
		if (EventSystem.current != null)
		{
			EventSystem.current.SetSelectedGameObject(null);
		}
	}

	// Token: 0x06000E22 RID: 3618 RVA: 0x0003AAE8 File Offset: 0x00038CE8
	private IEnumerator SelectBestSelectableNextFrame()
	{
		yield return null;
		if (EventSystem.current == null)
		{
			yield break;
		}
		UIImportance mostImportantAvailable = this.GetMostImportantAvailable();
		if (mostImportantAvailable != null)
		{
			this.ForceSelect(mostImportantAvailable);
			yield break;
		}
		GameObject firstSelectedGameObject = EventSystem.current.firstSelectedGameObject;
		if (firstSelectedGameObject != null && firstSelectedGameObject.activeInHierarchy)
		{
			Selectable component = firstSelectedGameObject.GetComponent<Selectable>();
			if (component != null && component.IsInteractable())
			{
				this.ForceSelect(component);
				yield break;
			}
		}
		Selectable selectable = this.FindFirstAvailableSelectable();
		if (selectable != null)
		{
			this.ForceSelect(selectable);
		}
		yield break;
	}

	// Token: 0x06000E23 RID: 3619 RVA: 0x0003AAF8 File Offset: 0x00038CF8
	private UIImportance GetMostImportantAvailable()
	{
		UIImportance result = null;
		int num = int.MinValue;
		foreach (UIImportance uiimportance in this.trackedUIImportance)
		{
			if (!(uiimportance == null) && uiimportance.IsVisibleAndEnabled() && uiimportance.Importance > num)
			{
				num = uiimportance.Importance;
				result = uiimportance;
			}
		}
		return result;
	}

	// Token: 0x06000E24 RID: 3620 RVA: 0x0003AB70 File Offset: 0x00038D70
	private Selectable FindFirstAvailableSelectable()
	{
		foreach (Selectable selectable in Object.FindObjectsByType<Selectable>(FindObjectsInactive.Include, FindObjectsSortMode.None))
		{
			if (!(selectable == null) && selectable.IsInteractable() && selectable.gameObject.activeInHierarchy && selectable.navigation.mode != Navigation.Mode.None)
			{
				return selectable;
			}
		}
		return null;
	}

	// Token: 0x06000E25 RID: 3621 RVA: 0x0003ABC8 File Offset: 0x00038DC8
	private void ForceSelect(UIImportance ui)
	{
		if (ui == null || ui.Selectable == null)
		{
			return;
		}
		this.ForceSelect(ui.Selectable);
	}

	// Token: 0x06000E26 RID: 3622 RVA: 0x0003ABF0 File Offset: 0x00038DF0
	private void ForceSelect(Selectable selectable)
	{
		if (selectable == null || EventSystem.current == null)
		{
			return;
		}
		if (!selectable.IsInteractable() || !selectable.gameObject.activeInHierarchy)
		{
			return;
		}
		if (selectable.navigation.mode == Navigation.Mode.None)
		{
			Navigation navigation = selectable.navigation;
			navigation.mode = Navigation.Mode.Automatic;
			selectable.navigation = navigation;
		}
		EventSystem.current.SetSelectedGameObject(null);
		selectable.Select();
	}

	// Token: 0x06000E27 RID: 3623 RVA: 0x0003AC61 File Offset: 0x00038E61
	public bool IsControllerActive()
	{
		return this.currentInputMode == InputMode.Controller;
	}

	// Token: 0x06000E28 RID: 3624 RVA: 0x0003AC6C File Offset: 0x00038E6C
	public bool IsKeyboardMouseActive()
	{
		return this.currentInputMode == InputMode.KeyboardMouse;
	}

	// Token: 0x06000E29 RID: 3625 RVA: 0x0003AC77 File Offset: 0x00038E77
	public void OnUIImportanceEnabled(UIImportance uiImportance)
	{
		if (uiImportance == null)
		{
			return;
		}
		this.trackedUIImportance.Add(uiImportance);
		if (this.currentInputMode == InputMode.Controller)
		{
			base.StartCoroutine(this.TrySelectIfBetter(uiImportance));
		}
	}

	// Token: 0x06000E2A RID: 3626 RVA: 0x0003ACA7 File Offset: 0x00038EA7
	private IEnumerator TrySelectIfBetter(UIImportance uiImportance)
	{
		yield return null;
		if (uiImportance == null || this.currentInputMode != InputMode.Controller)
		{
			yield break;
		}
		if (EventSystem.current == null)
		{
			yield break;
		}
		if (!uiImportance.IsVisibleAndEnabled())
		{
			yield break;
		}
		GameObject currentSelectedGameObject = EventSystem.current.currentSelectedGameObject;
		UIImportance uiimportance = (currentSelectedGameObject != null) ? currentSelectedGameObject.GetComponent<UIImportance>() : null;
		if (!(uiimportance == null) && uiImportance.Importance <= uiimportance.Importance)
		{
			yield break;
		}
		this.ForceSelect(uiImportance);
		yield break;
	}

	// Token: 0x06000E2B RID: 3627 RVA: 0x0003ACC0 File Offset: 0x00038EC0
	public void OnUIImportanceDisabled(UIImportance uiImportance)
	{
		if (uiImportance == null)
		{
			return;
		}
		this.trackedUIImportance.Remove(uiImportance);
		if (this.currentInputMode != InputMode.Controller || EventSystem.current == null)
		{
			return;
		}
		if (EventSystem.current.currentSelectedGameObject == uiImportance.gameObject)
		{
			base.StartCoroutine(this.SelectBestSelectableNextFrame());
		}
	}

	// Token: 0x040008F7 RID: 2295
	[Header("Settings")]
	[SerializeField]
	private float joystickDeadzone = 0.3f;

	// Token: 0x040008F8 RID: 2296
	[SerializeField]
	private float joystickThreshold = 0.5f;

	// Token: 0x040008F9 RID: 2297
	[Tooltip("Minimum magnitude of joystick movement to switch to controller mode")]
	[SerializeField]
	private float minJoystickMagnitude = 0.4f;

	// Token: 0x040008FA RID: 2298
	[Header("Mouse Noise Filtering")]
	[SerializeField]
	private float mouseDeltaSwitchThreshold = 2f;

	// Token: 0x040008FB RID: 2299
	[SerializeField]
	private float mouseScrollSwitchThreshold = 0.01f;

	// Token: 0x040008FC RID: 2300
	[SerializeField]
	private InputMode currentInputMode;

	// Token: 0x040008FD RID: 2301
	private InputSystemUIInputModule uiInputModule;

	// Token: 0x040008FE RID: 2302
	private StandaloneInputModule legacyInputModule;

	// Token: 0x040008FF RID: 2303
	private readonly HashSet<UIImportance> trackedUIImportance = new HashSet<UIImportance>();

	// Token: 0x04000900 RID: 2304
	public static Action<InputMode> OnInputModeChanged;
}
