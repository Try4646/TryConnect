using System;
using System.Collections;
using Extensions;
using TMPro;
using UnityEngine;

// Token: 0x020000FF RID: 255
public class Upgrade : ConsumableItem
{
	// Token: 0x06000A7D RID: 2685 RVA: 0x0002A004 File Offset: 0x00028204
	public void Start()
	{
		this.typeText.text = this.upgradeType.ToString();
		this.valueText.text = (this.value * 100f).ToString("0.#") + "%";
	}

	// Token: 0x06000A7E RID: 2686 RVA: 0x0002A05C File Offset: 0x0002825C
	protected override void OnUseItem(bool isPressed)
	{
		if (this._hasBeenUsed)
		{
			return;
		}
		this._hasBeenUsed = true;
		this._holderProfile = base.NetworkHolder.GetComponent<PlayerProfile>();
		this.anim.SetTrigger("Use");
		if (base.isServer)
		{
			base.StartCoroutine(this.UseRoutine());
		}
	}

	// Token: 0x06000A7F RID: 2687 RVA: 0x0002A0AF File Offset: 0x000282AF
	private IEnumerator UseRoutine()
	{
		yield return new WaitForSecondsRealtime(0.5f);
		if (NetworkSingleton<GameManager>.Instance.state == GameState.Game)
		{
			NetworkSingleton<UpgradeManager>.Instance.ChangeUpgradeData(this._holderProfile.steamId, this.upgradeType, this.value);
		}
		this.upgradeSfx.RpcPlayOneShotWith3DPos();
		base.DestroyItem();
		yield break;
	}

	// Token: 0x06000A81 RID: 2689 RVA: 0x00002321 File Offset: 0x00000521
	public override bool Weaved()
	{
		return true;
	}

	// Token: 0x04000695 RID: 1685
	[SerializeField]
	private Animator anim;

	// Token: 0x04000696 RID: 1686
	[SerializeField]
	private TextMeshPro typeText;

	// Token: 0x04000697 RID: 1687
	[SerializeField]
	private TextMeshPro valueText;

	// Token: 0x04000698 RID: 1688
	[SerializeField]
	private PlayerUpgradeType upgradeType;

	// Token: 0x04000699 RID: 1689
	[SerializeField]
	private float value;

	// Token: 0x0400069A RID: 1690
	[Header("SFX")]
	[SerializeField]
	private SFXComponent upgradeSfx;

	// Token: 0x0400069B RID: 1691
	private bool _hasBeenUsed;

	// Token: 0x0400069C RID: 1692
	private PlayerProfile _holderProfile;
}
