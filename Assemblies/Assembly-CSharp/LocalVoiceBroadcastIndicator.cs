using System;
using Dissonance;
using UnityEngine;

// Token: 0x020002CF RID: 719
public class LocalVoiceBroadcastIndicator : MonoBehaviour
{
	// Token: 0x0600195D RID: 6493 RVA: 0x0006A943 File Offset: 0x00068B43
	private void Awake()
	{
		if (this.canvasGroup == null)
		{
			this.canvasGroup = base.GetComponent<CanvasGroup>();
		}
	}

	// Token: 0x0600195E RID: 6494 RVA: 0x0006A95F File Offset: 0x00068B5F
	private void Start()
	{
		this.RefreshTriggers();
	}

	// Token: 0x0600195F RID: 6495 RVA: 0x0006A967 File Offset: 0x00068B67
	private void OnEnable()
	{
		if (this.canvasGroup != null)
		{
			this.canvasGroup.alpha = 0f;
		}
	}

	// Token: 0x06001960 RID: 6496 RVA: 0x0006A988 File Offset: 0x00068B88
	private void Update()
	{
		if (this.canvasGroup == null)
		{
			return;
		}
		if (this._triggerBehaviours == null || this._triggerBehaviours.Length == 0)
		{
			this.RefreshTriggers();
		}
		bool flag = false;
		for (int i = 0; i < this._triggerBehaviours.Length; i++)
		{
			IVoiceBroadcastTrigger voiceBroadcastTrigger = this._triggerBehaviours[i] as IVoiceBroadcastTrigger;
			if (voiceBroadcastTrigger != null && voiceBroadcastTrigger.IsTransmitting)
			{
				flag = true;
				break;
			}
		}
		float b = flag ? 1f : 0f;
		this.canvasGroup.alpha = Mathf.Lerp(this.canvasGroup.alpha, b, this.fadeSpeed * Time.unscaledDeltaTime);
	}

	// Token: 0x06001961 RID: 6497 RVA: 0x0006AA25 File Offset: 0x00068C25
	private void RefreshTriggers()
	{
		this._triggerBehaviours = Object.FindObjectsByType<MonoBehaviour>(FindObjectsInactive.Include, FindObjectsSortMode.None);
	}

	// Token: 0x0400104F RID: 4175
	[SerializeField]
	private CanvasGroup canvasGroup;

	// Token: 0x04001050 RID: 4176
	[SerializeField]
	private float fadeSpeed = 12f;

	// Token: 0x04001051 RID: 4177
	private MonoBehaviour[] _triggerBehaviours;
}
