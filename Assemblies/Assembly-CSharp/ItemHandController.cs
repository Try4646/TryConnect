using System;
using UnityEngine;

// Token: 0x020001EE RID: 494
public class ItemHandController : MonoBehaviour
{
	// Token: 0x060011B4 RID: 4532 RVA: 0x0004C7AB File Offset: 0x0004A9AB
	private void Awake()
	{
		this.animator = base.GetComponent<Animator>();
		if (this.handTransform == null)
		{
			Debug.LogError("ItemHandController: Hand Transform reference is missing!");
			return;
		}
		if (this.handWatcher == null)
		{
			Debug.LogError("ItemHandController: Hand Transform Watcher reference is missing! Add the HandTransformWatcher component to your hand transform GameObject.");
			return;
		}
	}

	// Token: 0x060011B5 RID: 4533 RVA: 0x0004C7EB File Offset: 0x0004A9EB
	private void OnEnable()
	{
		if (this.handWatcher != null)
		{
			HandTransformWatcher handTransformWatcher = this.handWatcher;
			handTransformWatcher.OnChildrenChanged = (Action)Delegate.Combine(handTransformWatcher.OnChildrenChanged, new Action(this.OnHandChildrenChanged));
		}
	}

	// Token: 0x060011B6 RID: 4534 RVA: 0x0004C822 File Offset: 0x0004AA22
	private void OnDisable()
	{
		if (this.handWatcher != null)
		{
			HandTransformWatcher handTransformWatcher = this.handWatcher;
			handTransformWatcher.OnChildrenChanged = (Action)Delegate.Remove(handTransformWatcher.OnChildrenChanged, new Action(this.OnHandChildrenChanged));
		}
	}

	// Token: 0x060011B7 RID: 4535 RVA: 0x0004C85C File Offset: 0x0004AA5C
	private void OnHandChildrenChanged()
	{
		if (this.handTransform.childCount > 0)
		{
			Item component = this.handTransform.GetChild(0).GetComponent<Item>();
			if (component != null)
			{
				this.OnItemPickup(component);
				return;
			}
		}
		else
		{
			this.OnItemDropped();
		}
	}

	// Token: 0x060011B8 RID: 4536 RVA: 0x0004C8A0 File Offset: 0x0004AAA0
	private void OnItemPickup(Item item)
	{
		if (this.animator == null || item == null)
		{
			return;
		}
		float num = 1f;
		float num2 = Mathf.InverseLerp(this.minItemWidth, this.maxItemWidth, num);
		num2 = Mathf.Clamp01(num2 * this.widthMultiplier);
		this.animator.SetFloat(this.handsWidthParam, num2);
		float value;
		if (num < this.clutchingThreshold)
		{
			value = 0f;
		}
		else
		{
			value = Mathf.InverseLerp(this.maxItemWidthForMinClutch, this.minItemWidthForMaxClutch, num);
			value = Mathf.Clamp01(value) * this.maxClutching;
		}
		this.animator.SetFloat(this.handsClutchingParam, value);
		this.animator.SetBool(this.isHoldingItemParam, true);
	}

	// Token: 0x060011B9 RID: 4537 RVA: 0x0004C95C File Offset: 0x0004AB5C
	public void RefreshCurrentItem()
	{
		if (this.handTransform != null && this.handTransform.childCount > 0)
		{
			Item component = this.handTransform.GetChild(0).GetComponent<Item>();
			if (component != null)
			{
				this.OnItemPickup(component);
			}
		}
	}

	// Token: 0x060011BA RID: 4538 RVA: 0x0004C9A8 File Offset: 0x0004ABA8
	private void OnItemDropped()
	{
		if (this.animator == null)
		{
			return;
		}
		this.animator.SetFloat(this.handsWidthParam, 0f);
		this.animator.SetFloat(this.handsClutchingParam, 0f);
		this.animator.SetBool(this.isHoldingItemParam, false);
	}

	// Token: 0x04000B67 RID: 2919
	[Header("Hand Transform Reference")]
	[SerializeField]
	private Transform handTransform;

	// Token: 0x04000B68 RID: 2920
	[Header("Hand Transform Watcher")]
	[SerializeField]
	private HandTransformWatcher handWatcher;

	// Token: 0x04000B69 RID: 2921
	[Header("Animation Parameters")]
	[SerializeField]
	private string handsWidthParam = "handsWidth";

	// Token: 0x04000B6A RID: 2922
	[SerializeField]
	private string handsClutchingParam = "handsClutching";

	// Token: 0x04000B6B RID: 2923
	[SerializeField]
	private string isHoldingItemParam = "isHoldingItem";

	// Token: 0x04000B6C RID: 2924
	[Header("Width Settings")]
	[SerializeField]
	private float minItemWidth = 0.1f;

	// Token: 0x04000B6D RID: 2925
	[SerializeField]
	private float maxItemWidth = 2f;

	// Token: 0x04000B6E RID: 2926
	[SerializeField]
	private float widthMultiplier = 1.5f;

	// Token: 0x04000B6F RID: 2927
	[Header("Clutching Settings")]
	[SerializeField]
	private float clutchingThreshold = 0.5f;

	// Token: 0x04000B70 RID: 2928
	[SerializeField]
	private float maxClutching = 0.8f;

	// Token: 0x04000B71 RID: 2929
	[SerializeField]
	private float minItemWidthForMaxClutch = 0.2f;

	// Token: 0x04000B72 RID: 2930
	[SerializeField]
	private float maxItemWidthForMinClutch = 0.8f;

	// Token: 0x04000B73 RID: 2931
	private Animator animator;
}
