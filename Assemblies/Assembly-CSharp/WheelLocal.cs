using System;
using System.Collections;
using System.Linq;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using FMOD.Studio;
using FMODUnity;
using UnityEngine;

// Token: 0x02000099 RID: 153
public class WheelLocal : MonoBehaviour
{
	// Token: 0x1700008F RID: 143
	// (get) Token: 0x060005A2 RID: 1442 RVA: 0x00018EA7 File Offset: 0x000170A7
	private WheelResult[] Results
	{
		get
		{
			if (this._results == null || this._results.Length == 0)
			{
				this._results = this.resultsParent.GetComponentsInChildren<WheelResult>();
			}
			return this._results;
		}
	}

	// Token: 0x060005A3 RID: 1443 RVA: 0x00018ED4 File Offset: 0x000170D4
	public void SpinTheWheel()
	{
		if (this._isSpinning)
		{
			return;
		}
		this._isSpinning = true;
		float num = Random.Range(0f, 360f);
		float num2 = (float)this.minTurnAmount * 360f + num;
		if (this.spinDirection)
		{
			num2 *= -1f;
		}
		this.SpinWheel(num2, this.spinDuration);
		base.StartCoroutine(this.WaitAndStop());
	}

	// Token: 0x060005A4 RID: 1444 RVA: 0x00018F3B File Offset: 0x0001713B
	private IEnumerator WaitAndStop()
	{
		yield return new WaitForSeconds(this.spinDuration);
		this._isSpinning = false;
		this.FindResult();
		this.sfxSpinInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
		this.sfxSpinInstance.release();
		yield break;
	}

	// Token: 0x060005A5 RID: 1445 RVA: 0x00018F4C File Offset: 0x0001714C
	private void FindResult()
	{
		int index = (from x in this.Results.Select((WheelResult r, int i) => new
		{
			Result = r,
			Index = i
		})
		orderby x.Result.transform.position.y descending
		select x).First().Index;
		this.ResultFeedback(index);
	}

	// Token: 0x060005A6 RID: 1446 RVA: 0x00018FBC File Offset: 0x000171BC
	private void SpinWheel(float finalAngle, float duration)
	{
		this.wheelTransform.DOLocalRotate(new Vector3(0f, 0f, -finalAngle), duration, RotateMode.FastBeyond360).SetEase(this.easing);
		this.sfxSpinInstance = RuntimeManager.CreateInstance(this.sfxSpinEvent);
		this.sfxSpinInstance.set3DAttributes(base.transform.position.To3DAttributes());
		this.sfxSpinInstance.setParameterByName("spinDuration", duration * 1000f, false);
		this.sfxSpinInstance.start();
	}

	// Token: 0x060005A7 RID: 1447 RVA: 0x00019045 File Offset: 0x00017245
	private void ResultFeedback(int index)
	{
		this.Results[index].SelectedResultFeedback();
	}

	// Token: 0x040003EA RID: 1002
	[Header("Wheel Settings")]
	[SerializeField]
	private float spinDuration = 3f;

	// Token: 0x040003EB RID: 1003
	[SerializeField]
	private int minTurnAmount = 3;

	// Token: 0x040003EC RID: 1004
	[SerializeField]
	private bool spinDirection;

	// Token: 0x040003ED RID: 1005
	[SerializeField]
	private Ease easing = Ease.OutCubic;

	// Token: 0x040003EE RID: 1006
	[Header("References")]
	[SerializeField]
	private Transform wheelTransform;

	// Token: 0x040003EF RID: 1007
	[SerializeField]
	private Transform resultsParent;

	// Token: 0x040003F0 RID: 1008
	private WheelResult[] _results;

	// Token: 0x040003F1 RID: 1009
	[Header("SFX")]
	[SerializeField]
	private EventReference sfxSpinEvent;

	// Token: 0x040003F2 RID: 1010
	private EventInstance sfxSpinInstance;

	// Token: 0x040003F3 RID: 1011
	private bool _isSpinning;
}
