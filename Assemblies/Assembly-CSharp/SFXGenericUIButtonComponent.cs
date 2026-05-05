using System;
using FMODUnity;
using MoreMountains.Tools;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

// Token: 0x0200027B RID: 635
public class SFXGenericUIButtonComponent : MonoBehaviour
{
	// Token: 0x060016A0 RID: 5792 RVA: 0x000609B1 File Offset: 0x0005EBB1
	private void Awake()
	{
		if (this.playClick)
		{
			this._button = base.GetComponent<Button>();
		}
		if (this.playHover)
		{
			this._mmOnPointer = base.GetComponent<MMOnPointer>();
		}
	}

	// Token: 0x060016A1 RID: 5793 RVA: 0x000609DC File Offset: 0x0005EBDC
	private void OnEnable()
	{
		if (this._button != null)
		{
			this._button.onClick.AddListener(new UnityAction(this.PlayClick));
		}
		if (this._mmOnPointer != null)
		{
			UnityEvent pointerEnter = this._mmOnPointer.PointerEnter;
			if (pointerEnter == null)
			{
				return;
			}
			pointerEnter.AddListener(new UnityAction(this.PlayHover));
		}
	}

	// Token: 0x060016A2 RID: 5794 RVA: 0x00060A44 File Offset: 0x0005EC44
	private void OnDisable()
	{
		if (this._button != null)
		{
			this._button.onClick.RemoveListener(new UnityAction(this.PlayClick));
		}
		if (this._mmOnPointer != null)
		{
			UnityEvent pointerEnter = this._mmOnPointer.PointerEnter;
			if (pointerEnter == null)
			{
				return;
			}
			pointerEnter.RemoveListener(new UnityAction(this.PlayHover));
		}
	}

	// Token: 0x060016A3 RID: 5795 RVA: 0x00060AAA File Offset: 0x0005ECAA
	public void PlayClick()
	{
		if (this.onClickEventReference.IsNull)
		{
			return;
		}
		SFXManager.SFXOneShot(this.onClickEventReference, base.gameObject.transform.position);
	}

	// Token: 0x060016A4 RID: 5796 RVA: 0x00060AD5 File Offset: 0x0005ECD5
	public void PlayHover()
	{
		if (this.onHoverEventReference.IsNull)
		{
			return;
		}
		SFXManager.SFXOneShot(this.onHoverEventReference, base.gameObject.transform.position);
	}

	// Token: 0x04000EB5 RID: 3765
	[SerializeField]
	private EventReference onClickEventReference;

	// Token: 0x04000EB6 RID: 3766
	[SerializeField]
	private EventReference onHoverEventReference;

	// Token: 0x04000EB7 RID: 3767
	[SerializeField]
	private bool playClick = true;

	// Token: 0x04000EB8 RID: 3768
	[SerializeField]
	private bool playHover = true;

	// Token: 0x04000EB9 RID: 3769
	private Button _button;

	// Token: 0x04000EBA RID: 3770
	private MMOnPointer _mmOnPointer;
}
