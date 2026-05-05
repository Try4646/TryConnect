using System;
using UnityEngine;

// Token: 0x020002BC RID: 700
public static class InputEvents
{
	// Token: 0x1700024B RID: 587
	// (get) Token: 0x060018A9 RID: 6313 RVA: 0x00068638 File Offset: 0x00066838
	// (set) Token: 0x060018AA RID: 6314 RVA: 0x0006863F File Offset: 0x0006683F
	public static bool IsDev { get; private set; }

	// Token: 0x1700024C RID: 588
	// (get) Token: 0x060018AB RID: 6315 RVA: 0x00068647 File Offset: 0x00066847
	// (set) Token: 0x060018AC RID: 6316 RVA: 0x0006864E File Offset: 0x0006684E
	public static VoiceChatInputMode ProximityVoiceChatMode { get; private set; } = VoiceChatInputMode.VoiceActivation;

	// Token: 0x060018AD RID: 6317 RVA: 0x00068656 File Offset: 0x00066856
	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
	private static void ResetInputLayers()
	{
		InputEvents.ActiveLayer = InputLayer.Default;
		InputEvents.IsDev = false;
		InputEvents.ProximityVoiceChatMode = VoiceChatInputMode.VoiceActivation;
		InputEvents._isPushToTalkInputPressed = false;
	}

	// Token: 0x1700024D RID: 589
	// (get) Token: 0x060018AE RID: 6318 RVA: 0x00068670 File Offset: 0x00066870
	public static bool IsInteractPressed
	{
		get
		{
			return InputEvents._isInteractPressed;
		}
	}

	// Token: 0x1700024E RID: 590
	// (get) Token: 0x060018AF RID: 6319 RVA: 0x00068677 File Offset: 0x00066877
	public static bool IsUseItemPressed
	{
		get
		{
			return InputEvents._isUseItemPressed;
		}
	}

	// Token: 0x1700024F RID: 591
	// (get) Token: 0x060018B0 RID: 6320 RVA: 0x0006867E File Offset: 0x0006687E
	public static bool IsThrowItemPressed
	{
		get
		{
			return InputEvents._isThrowItemPressed;
		}
	}

	// Token: 0x17000250 RID: 592
	// (get) Token: 0x060018B1 RID: 6321 RVA: 0x00068685 File Offset: 0x00066885
	public static bool IsZoomPressed
	{
		get
		{
			return InputEvents._isZoomPressed;
		}
	}

	// Token: 0x17000251 RID: 593
	// (get) Token: 0x060018B2 RID: 6322 RVA: 0x0006868C File Offset: 0x0006688C
	public static bool IsJumpPressed
	{
		get
		{
			return InputEvents._isJumpPressed;
		}
	}

	// Token: 0x17000252 RID: 594
	// (get) Token: 0x060018B3 RID: 6323 RVA: 0x00068693 File Offset: 0x00066893
	public static bool IsCrouchPressed
	{
		get
		{
			return InputEvents._isCrouchPressed;
		}
	}

	// Token: 0x17000253 RID: 595
	// (get) Token: 0x060018B4 RID: 6324 RVA: 0x0006869A File Offset: 0x0006689A
	public static bool IsSprintPressed
	{
		get
		{
			return InputEvents._isSprintPressed;
		}
	}

	// Token: 0x17000254 RID: 596
	// (get) Token: 0x060018B5 RID: 6325 RVA: 0x000686A1 File Offset: 0x000668A1
	public static bool IsEmoteWheelPressed
	{
		get
		{
			return InputEvents._isEmoteWheelPressed;
		}
	}

	// Token: 0x17000255 RID: 597
	// (get) Token: 0x060018B6 RID: 6326 RVA: 0x000686A8 File Offset: 0x000668A8
	public static bool IsPingPressed
	{
		get
		{
			return InputEvents._isPingPressed;
		}
	}

	// Token: 0x17000256 RID: 598
	// (get) Token: 0x060018B7 RID: 6327 RVA: 0x000686AF File Offset: 0x000668AF
	public static bool IsSkipUIPressed
	{
		get
		{
			return InputEvents._isSkipUIPressed;
		}
	}

	// Token: 0x17000257 RID: 599
	// (get) Token: 0x060018B8 RID: 6328 RVA: 0x000686B6 File Offset: 0x000668B6
	public static bool IsPushToTalkPressed
	{
		get
		{
			return InputEvents.ProximityVoiceChatMode != VoiceChatInputMode.PushToTalk || InputEvents._isPushToTalkInputPressed;
		}
	}

	// Token: 0x060018B9 RID: 6329 RVA: 0x000686C6 File Offset: 0x000668C6
	public static void UpdateInteract(bool isPressed)
	{
		InputEvents._isInteractPressed = isPressed;
		Action<bool> onInteractEvent = InputEvents.OnInteractEvent;
		if (onInteractEvent == null)
		{
			return;
		}
		onInteractEvent(isPressed);
	}

	// Token: 0x060018BA RID: 6330 RVA: 0x000686DE File Offset: 0x000668DE
	public static void UpdateUseItem(bool isPressed)
	{
		InputEvents._isUseItemPressed = isPressed;
		Action<bool> onUseItemEvent = InputEvents.OnUseItemEvent;
		if (onUseItemEvent == null)
		{
			return;
		}
		onUseItemEvent(isPressed);
	}

	// Token: 0x060018BB RID: 6331 RVA: 0x000686F6 File Offset: 0x000668F6
	public static void UpdateThrowItem(bool isPressed)
	{
		InputEvents._isThrowItemPressed = isPressed;
		Action<bool> onThrowItemEvent = InputEvents.OnThrowItemEvent;
		if (onThrowItemEvent == null)
		{
			return;
		}
		onThrowItemEvent(isPressed);
	}

	// Token: 0x060018BC RID: 6332 RVA: 0x0006870E File Offset: 0x0006690E
	public static void UpdateZoom(bool isPressed)
	{
		InputEvents._isZoomPressed = isPressed;
		Action<bool> onZoomEvent = InputEvents.OnZoomEvent;
		if (onZoomEvent == null)
		{
			return;
		}
		onZoomEvent(isPressed);
	}

	// Token: 0x060018BD RID: 6333 RVA: 0x00068726 File Offset: 0x00066926
	public static void UpdateJump(bool isPressed)
	{
		InputEvents._isJumpPressed = isPressed;
		Action<bool> onJumpEvent = InputEvents.OnJumpEvent;
		if (onJumpEvent == null)
		{
			return;
		}
		onJumpEvent(isPressed);
	}

	// Token: 0x060018BE RID: 6334 RVA: 0x0006873E File Offset: 0x0006693E
	public static void UpdateCrouch(bool isPressed)
	{
		InputEvents._isCrouchPressed = isPressed;
		Action<bool> onCrouchEvent = InputEvents.OnCrouchEvent;
		if (onCrouchEvent == null)
		{
			return;
		}
		onCrouchEvent(isPressed);
	}

	// Token: 0x060018BF RID: 6335 RVA: 0x00068756 File Offset: 0x00066956
	public static void UpdateSprint(bool isPressed)
	{
		InputEvents._isSprintPressed = isPressed;
		Action<bool> onSprintEvent = InputEvents.OnSprintEvent;
		if (onSprintEvent == null)
		{
			return;
		}
		onSprintEvent(isPressed);
	}

	// Token: 0x060018C0 RID: 6336 RVA: 0x0006876E File Offset: 0x0006696E
	public static void UpdateEmoteWheel(bool isPressed)
	{
		InputEvents._isEmoteWheelPressed = isPressed;
		Action<bool> onEmoteWheelEvent = InputEvents.OnEmoteWheelEvent;
		if (onEmoteWheelEvent == null)
		{
			return;
		}
		onEmoteWheelEvent(isPressed);
	}

	// Token: 0x060018C1 RID: 6337 RVA: 0x00068786 File Offset: 0x00066986
	public static void UpdatePing(bool isPressed)
	{
		InputEvents._isPingPressed = isPressed;
		Action onPingEvent = InputEvents.OnPingEvent;
		if (onPingEvent == null)
		{
			return;
		}
		onPingEvent();
	}

	// Token: 0x060018C2 RID: 6338 RVA: 0x0006879D File Offset: 0x0006699D
	public static void UpdateSkipUI(bool isPressed)
	{
		InputEvents._isSkipUIPressed = isPressed;
		Action<bool> onSkipUIEvent = InputEvents.OnSkipUIEvent;
		if (onSkipUIEvent == null)
		{
			return;
		}
		onSkipUIEvent(isPressed);
	}

	// Token: 0x060018C3 RID: 6339 RVA: 0x000687B5 File Offset: 0x000669B5
	public static void UpdatePushToTalk(bool isPressed)
	{
		InputEvents._isPushToTalkInputPressed = isPressed;
		Action<bool> onPushToTalkEvent = InputEvents.OnPushToTalkEvent;
		if (onPushToTalkEvent == null)
		{
			return;
		}
		onPushToTalkEvent(InputEvents.IsPushToTalkPressed);
	}

	// Token: 0x060018C4 RID: 6340 RVA: 0x000687D1 File Offset: 0x000669D1
	public static void SetProximityVoiceChatMode(VoiceChatInputMode mode)
	{
		if (InputEvents.ProximityVoiceChatMode == mode)
		{
			return;
		}
		InputEvents.ProximityVoiceChatMode = mode;
		Action<bool> onPushToTalkEvent = InputEvents.OnPushToTalkEvent;
		if (onPushToTalkEvent == null)
		{
			return;
		}
		onPushToTalkEvent(InputEvents.IsPushToTalkPressed);
	}

	// Token: 0x060018C5 RID: 6341 RVA: 0x000687F6 File Offset: 0x000669F6
	public static void SetProximityVoiceChatMode(string selectedOption)
	{
		if (string.IsNullOrWhiteSpace(selectedOption))
		{
			return;
		}
		InputEvents.SetProximityVoiceChatMode(selectedOption.Trim().ToLowerInvariant().StartsWith("push") ? VoiceChatInputMode.PushToTalk : VoiceChatInputMode.VoiceActivation);
	}

	// Token: 0x060018C6 RID: 6342 RVA: 0x00068821 File Offset: 0x00066A21
	public static void SetDevMode(bool isDev)
	{
		InputEvents.IsDev = isDev;
	}

	// Token: 0x060018C7 RID: 6343 RVA: 0x00068829 File Offset: 0x00066A29
	public static void TryInvokeConsole()
	{
		if (!InputEvents.IsDev)
		{
			return;
		}
		Action onConsoleEvent = InputEvents.OnConsoleEvent;
		if (onConsoleEvent == null)
		{
			return;
		}
		onConsoleEvent();
	}

	// Token: 0x060018C8 RID: 6344 RVA: 0x00068842 File Offset: 0x00066A42
	public static void TryInvokeF1()
	{
		if (!InputEvents.IsDev)
		{
			return;
		}
		Action onF1Event = InputEvents.OnF1Event;
		if (onF1Event == null)
		{
			return;
		}
		onF1Event();
	}

	// Token: 0x060018C9 RID: 6345 RVA: 0x0006885B File Offset: 0x00066A5B
	public static void TryInvokeF2()
	{
		if (!InputEvents.IsDev)
		{
			return;
		}
		Action onF2Event = InputEvents.OnF2Event;
		if (onF2Event == null)
		{
			return;
		}
		onF2Event();
	}

	// Token: 0x060018CA RID: 6346 RVA: 0x00068874 File Offset: 0x00066A74
	public static void TryInvokeF3()
	{
		if (!InputEvents.IsDev)
		{
			return;
		}
		Action onF3Event = InputEvents.OnF3Event;
		if (onF3Event == null)
		{
			return;
		}
		onF3Event();
	}

	// Token: 0x060018CB RID: 6347 RVA: 0x0006888D File Offset: 0x00066A8D
	public static void TryInvokeF4()
	{
		if (!InputEvents.IsDev)
		{
			return;
		}
		Action onF4Event = InputEvents.OnF4Event;
		if (onF4Event == null)
		{
			return;
		}
		onF4Event();
	}

	// Token: 0x04000FDC RID: 4060
	public static InputLayer ActiveLayer;

	// Token: 0x04000FDF RID: 4063
	public static Action<Vector2> OnMoveEvent;

	// Token: 0x04000FE0 RID: 4064
	public static Action<Vector2> OnAimEvent;

	// Token: 0x04000FE1 RID: 4065
	public static Action<bool> OnJumpEvent;

	// Token: 0x04000FE2 RID: 4066
	public static Action<bool> OnCrouchEvent;

	// Token: 0x04000FE3 RID: 4067
	public static Action<bool> OnSprintEvent;

	// Token: 0x04000FE4 RID: 4068
	public static Action<bool> OnInteractEvent;

	// Token: 0x04000FE5 RID: 4069
	public static Action<bool> OnThrowItemEvent;

	// Token: 0x04000FE6 RID: 4070
	public static Action<bool> OnUseItemEvent;

	// Token: 0x04000FE7 RID: 4071
	public static Action<bool> OnZoomEvent;

	// Token: 0x04000FE8 RID: 4072
	public static Action<int> OnItemSelectEvent;

	// Token: 0x04000FE9 RID: 4073
	public static Action<int> OnScrollEvent;

	// Token: 0x04000FEA RID: 4074
	public static Action OnConsoleEvent;

	// Token: 0x04000FEB RID: 4075
	public static Action OnEscapeMenuEvent;

	// Token: 0x04000FEC RID: 4076
	public static Action<bool> OnEmoteWheelEvent;

	// Token: 0x04000FED RID: 4077
	public static Action OnF1Event;

	// Token: 0x04000FEE RID: 4078
	public static Action OnF2Event;

	// Token: 0x04000FEF RID: 4079
	public static Action OnF3Event;

	// Token: 0x04000FF0 RID: 4080
	public static Action OnF4Event;

	// Token: 0x04000FF1 RID: 4081
	public static Action OnPingEvent;

	// Token: 0x04000FF2 RID: 4082
	public static Action<bool> OnPushToTalkEvent;

	// Token: 0x04000FF3 RID: 4083
	public static Action<bool> OnAnyInputEvent;

	// Token: 0x04000FF4 RID: 4084
	public static Action<bool> OnSkipUIEvent;

	// Token: 0x04000FF5 RID: 4085
	private static bool _isInteractPressed;

	// Token: 0x04000FF6 RID: 4086
	private static bool _isUseItemPressed;

	// Token: 0x04000FF7 RID: 4087
	private static bool _isThrowItemPressed;

	// Token: 0x04000FF8 RID: 4088
	private static bool _isZoomPressed;

	// Token: 0x04000FF9 RID: 4089
	private static bool _isJumpPressed;

	// Token: 0x04000FFA RID: 4090
	private static bool _isCrouchPressed;

	// Token: 0x04000FFB RID: 4091
	private static bool _isSprintPressed;

	// Token: 0x04000FFC RID: 4092
	private static bool _isEmoteWheelPressed;

	// Token: 0x04000FFD RID: 4093
	private static bool _isPingPressed;

	// Token: 0x04000FFE RID: 4094
	private static bool _isSkipUIPressed;

	// Token: 0x04000FFF RID: 4095
	private static bool _isPushToTalkInputPressed;
}
