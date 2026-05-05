using System;
using System.Collections;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using Extensions;
using Mirror;
using Mirror.RemoteCalls;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

// Token: 0x020000BD RID: 189
public class EndingSequenceManager : NetworkSingleton<EndingSequenceManager>
{
	// Token: 0x06000720 RID: 1824 RVA: 0x0001E19C File Offset: 0x0001C39C
	private void Start()
	{
		this.InitBaseValues();
		this.achievementToUnlock.UnlockAchievement();
		MonoSingleton<LocalManager>.Instance.mainCamera.cullingMask |= 1 << LayerMask.NameToLayer("SelfMeshPlayer");
		this.skipUI.Reset();
	}

	// Token: 0x06000721 RID: 1825 RVA: 0x0001E1EC File Offset: 0x0001C3EC
	private void InitBaseValues()
	{
		foreach (Image image in this.bars)
		{
			Color color = image.color;
			color.a = 0f;
			image.color = color;
		}
		CanvasGroup[] array2 = this.creditSteps;
		for (int i = 0; i < array2.Length; i++)
		{
			array2[i].alpha = 0f;
		}
	}

	// Token: 0x06000722 RID: 1826 RVA: 0x0001E24C File Offset: 0x0001C44C
	private void CrossFadeBetweenTwoCanvasGroups(CanvasGroup from, CanvasGroup to, float duration)
	{
		if (this._isBeenFading)
		{
			return;
		}
		this._isBeenFading = true;
		from.DOFade(0f, duration).SetEase(Ease.InOutSine);
		to.DOFade(1f, duration).SetEase(Ease.InOutSine).OnComplete(delegate
		{
			this._isBeenFading = false;
		});
	}

	// Token: 0x06000723 RID: 1827 RVA: 0x0001E2A0 File Offset: 0x0001C4A0
	private void OnEnable()
	{
		SkipUI skipUI = this.skipUI;
		skipUI.OnSkipped = (UnityAction)Delegate.Combine(skipUI.OnSkipped, new UnityAction(this.OnSkip));
	}

	// Token: 0x06000724 RID: 1828 RVA: 0x0001E2C9 File Offset: 0x0001C4C9
	private void OnDisable()
	{
		SkipUI skipUI = this.skipUI;
		skipUI.OnSkipped = (UnityAction)Delegate.Remove(skipUI.OnSkipped, new UnityAction(this.OnSkip));
	}

	// Token: 0x06000725 RID: 1829 RVA: 0x000048A7 File Offset: 0x00002AA7
	private void OnSkip()
	{
	}

	// Token: 0x06000726 RID: 1830 RVA: 0x0001E2F2 File Offset: 0x0001C4F2
	[Server]
	public void ServerStartSequence()
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void EndingSequenceManager::ServerStartSequence()' called when server was not active");
			return;
		}
		this.RpcStartSequence();
	}

	// Token: 0x06000727 RID: 1831 RVA: 0x0001E310 File Offset: 0x0001C510
	[ClientRpc]
	private void RpcStartSequence()
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		this.SendRPCInternal("System.Void EndingSequenceManager::RpcStartSequence()", -2144402586, writer, 0, true);
		NetworkWriterPool.Return(writer);
	}

	// Token: 0x06000728 RID: 1832 RVA: 0x0001E340 File Offset: 0x0001C540
	public void TestLocal()
	{
		base.StartCoroutine(this.SequenceRoutine(this.thingsToLookAtInOrder));
	}

	// Token: 0x06000729 RID: 1833 RVA: 0x0001E358 File Offset: 0x0001C558
	private void Update()
	{
		float cameraPosition = this.splineDolly.CameraPosition;
		while (this.currentStepIndex < this.sequence.Length && cameraPosition >= this.sequence[this.currentStepIndex].threshold)
		{
			this.ExecuteStep(this.sequence[this.currentStepIndex], this.currentStepIndex);
			this.currentStepIndex++;
		}
	}

	// Token: 0x0600072A RID: 1834 RVA: 0x0001E3C0 File Offset: 0x0001C5C0
	private void ExecuteStep(EndingSequenceManager.CreditStep step, int stepIndex)
	{
		if (this.creditSteps == null || this.creditSteps.Length == 0)
		{
			Debug.LogWarning("Credit steps array is empty.");
			return;
		}
		if (step.fromCanvasIndex < 0 || step.fromCanvasIndex >= this.creditSteps.Length || step.toCanvasIndex < 0 || step.toCanvasIndex >= this.creditSteps.Length)
		{
			Debug.LogWarning(string.Format("Invalid canvas index at sequence step {0}.", stepIndex));
			return;
		}
		Debug.Log(string.IsNullOrWhiteSpace(step.debugLabel) ? string.Format("step {0}", stepIndex + 1) : step.debugLabel);
		this.CrossFadeBetweenTwoCanvasGroups(this.creditSteps[step.fromCanvasIndex], this.creditSteps[step.toCanvasIndex], step.fadeDuration);
	}

	// Token: 0x0600072B RID: 1835 RVA: 0x0001E483 File Offset: 0x0001C683
	public void ResetSequence()
	{
		this.currentStepIndex = 0;
	}

	// Token: 0x0600072C RID: 1836 RVA: 0x0001E48C File Offset: 0x0001C68C
	private IEnumerator SequenceRoutine(Transform[] lookAtTargets)
	{
		this.skipUI.SetSkippableServer();
		this.splineDolly.CameraPosition = 0f;
		float camPositionAlongSpline = this.splineDolly.CameraPosition;
		DOTween.To(() => camPositionAlongSpline, delegate(float x)
		{
			this.splineDolly.CameraPosition = x;
		}, 1f, this.splineDuration).SetEase(Ease.Linear);
		yield return new WaitForSeconds(this.initialDelay);
		this.skipUI.SetSkippableForLocal();
		Image[] array = this.bars;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].DOColor(Color.black, this.barFadeDuration);
		}
		yield return new WaitForSeconds(this.barFadeDuration);
		if (this.thingsToLookAtInOrder[0])
		{
			this.splineDolly.VirtualCamera.Follow = this.thingsToLookAtInOrder[0];
		}
		if (this.creditSteps.Length != 0 && this.creditSteps[0])
		{
			this.creditSteps[0].DOFade(1f, this.barFadeDuration);
		}
		yield return new WaitForSeconds(this.splineDuration + 0.5f);
		this.background.DOColor(new Color(0f, 0f, 0f, 0.5f), this.backgroundFadeDuration);
		yield return new WaitForSeconds(this.backgroundFadeDuration);
		yield return new WaitForSeconds(this.delayBeforeSkip);
		yield break;
	}

	// Token: 0x0600072F RID: 1839 RVA: 0x00002321 File Offset: 0x00000521
	public override bool Weaved()
	{
		return true;
	}

	// Token: 0x06000730 RID: 1840 RVA: 0x0001E340 File Offset: 0x0001C540
	protected void UserCode_RpcStartSequence()
	{
		base.StartCoroutine(this.SequenceRoutine(this.thingsToLookAtInOrder));
	}

	// Token: 0x06000731 RID: 1841 RVA: 0x0001E510 File Offset: 0x0001C710
	protected static void InvokeUserCode_RpcStartSequence(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcStartSequence called on server.");
			return;
		}
		((EndingSequenceManager)obj).UserCode_RpcStartSequence();
	}

	// Token: 0x06000732 RID: 1842 RVA: 0x0001E533 File Offset: 0x0001C733
	static EndingSequenceManager()
	{
		RemoteProcedureCalls.RegisterRpc(typeof(EndingSequenceManager), "System.Void EndingSequenceManager::RpcStartSequence()", new RemoteCallDelegate(EndingSequenceManager.InvokeUserCode_RpcStartSequence));
	}

	// Token: 0x040004C2 RID: 1218
	[Header("Settings")]
	[SerializeField]
	private float initialDelay = 2f;

	// Token: 0x040004C3 RID: 1219
	[SerializeField]
	private float barFadeDuration = 1f;

	// Token: 0x040004C4 RID: 1220
	[SerializeField]
	private float splineDuration = 15f;

	// Token: 0x040004C5 RID: 1221
	[SerializeField]
	private float cameraOffsetDuration = 2f;

	// Token: 0x040004C6 RID: 1222
	[SerializeField]
	private float creditsDuration = 20f;

	// Token: 0x040004C7 RID: 1223
	[SerializeField]
	private float skipTime = 2f;

	// Token: 0x040004C8 RID: 1224
	[SerializeField]
	private float backgroundFadeDuration = 1f;

	// Token: 0x040004C9 RID: 1225
	[SerializeField]
	private float delayBeforeSkip = 2f;

	// Token: 0x040004CA RID: 1226
	[Header("References")]
	[SerializeField]
	private SkipUI skipUI;

	// Token: 0x040004CB RID: 1227
	[SerializeField]
	private CinemachineSplineDolly splineDolly;

	// Token: 0x040004CC RID: 1228
	[SerializeField]
	private CinemachineCameraOffset cameraOffset;

	// Token: 0x040004CD RID: 1229
	[SerializeField]
	private SteamAchievement_SteamworksNET achievementToUnlock;

	// Token: 0x040004CE RID: 1230
	[SerializeField]
	private Image[] bars;

	// Token: 0x040004CF RID: 1231
	[SerializeField]
	private Image background;

	// Token: 0x040004D0 RID: 1232
	[SerializeField]
	private CanvasGroup[] creditSteps;

	// Token: 0x040004D1 RID: 1233
	[SerializeField]
	private EndingSequenceManager.CreditStep[] sequence;

	// Token: 0x040004D2 RID: 1234
	private bool _isBeenFading;

	// Token: 0x040004D3 RID: 1235
	private int currentStepIndex;

	// Token: 0x040004D4 RID: 1236
	[Header("CutsceneSpecific")]
	[SerializeField]
	private Transform[] thingsToLookAtInOrder;

	// Token: 0x020000BE RID: 190
	[Serializable]
	public class CreditStep
	{
		// Token: 0x040004D5 RID: 1237
		[Range(0f, 1f)]
		public float threshold;

		// Token: 0x040004D6 RID: 1238
		public int fromCanvasIndex;

		// Token: 0x040004D7 RID: 1239
		public int toCanvasIndex;

		// Token: 0x040004D8 RID: 1240
		public float fadeDuration = 2f;

		// Token: 0x040004D9 RID: 1241
		public string debugLabel;
	}
}
