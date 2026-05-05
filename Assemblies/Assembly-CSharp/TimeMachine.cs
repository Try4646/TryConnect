using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Extensions;
using Mirror;
using TMPro;
using UnityEngine;

// Token: 0x020000FD RID: 253
public class TimeMachine : ConsumableItem
{
	// Token: 0x06000A71 RID: 2673 RVA: 0x00029D70 File Offset: 0x00027F70
	protected override void OnUseItem(bool isPressed)
	{
		base.OnUseItem(isPressed);
		if (!isPressed)
		{
			return;
		}
		if (this._isActivated)
		{
			return;
		}
		if (NetworkSingleton<GameManager>.Instance.state == GameState.Game && !NetworkSingleton<GameManager>.Instance.HasDayStarted)
		{
			return;
		}
		this._isActivated = true;
		this.rewindSfx.PlayOneShotWith3DPos();
		this.anim.SetTrigger("Rewind");
		DOVirtual.Float(0f, 1f, 0.5f, delegate(float t)
		{
			float f = this.rollbackSeconds - this.rollbackSeconds * t;
			this.screenText.text = Mathf.RoundToInt(f).ToString() + "s";
		}).SetEase(Ease.OutCubic);
		if (base.isServer)
		{
			base.StartCoroutine(this.RollbackRoutine());
		}
	}

	// Token: 0x06000A72 RID: 2674 RVA: 0x00029E0C File Offset: 0x0002800C
	[Server]
	private IEnumerator RollbackRoutine()
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Collections.IEnumerator TimeMachine::RollbackRoutine()' called when server was not active");
			return null;
		}
		TimeMachine.<RollbackRoutine>d__7 <RollbackRoutine>d__ = new TimeMachine.<RollbackRoutine>d__7(0);
		<RollbackRoutine>d__.<>4__this = this;
		return <RollbackRoutine>d__;
	}

	// Token: 0x06000A73 RID: 2675 RVA: 0x00029E48 File Offset: 0x00028048
	[Server]
	private void ServerRewindTime()
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void TimeMachine::ServerRewindTime()' called when server was not active");
			return;
		}
		List<PayoutRecord> list = null;
		if (NetworkSingleton<PayoutTracker>.Instance)
		{
			list = NetworkSingleton<PayoutTracker>.Instance.RollbackLastSeconds(this.rollbackSeconds);
		}
		if (list != null && list.Count > 0)
		{
			foreach (PayoutRecord payoutRecord in list)
			{
				if (payoutRecord != null && payoutRecord.profit != 0L)
				{
					NetworkSingleton<MoneyManager>.Instance.TryChangeBalance(-payoutRecord.profit, payoutRecord.playerProfile, ChangeType.GameResult);
				}
			}
			NetworkSingleton<GameResultsManager>.Instance.RollbackResults(list);
		}
		NetworkSingleton<GameManager>.Instance.ServerAdjustTimer(-this.rollbackSeconds);
	}

	// Token: 0x06000A76 RID: 2678 RVA: 0x00002321 File Offset: 0x00000521
	public override bool Weaved()
	{
		return true;
	}

	// Token: 0x0400068C RID: 1676
	[Header("Settings")]
	[SerializeField]
	private float rollbackSeconds = 60f;

	// Token: 0x0400068D RID: 1677
	[Header("References")]
	[SerializeField]
	private TextMeshPro screenText;

	// Token: 0x0400068E RID: 1678
	[SerializeField]
	private Animator anim;

	// Token: 0x0400068F RID: 1679
	[Header("SFX")]
	[SerializeField]
	private SFXComponent rewindSfx;

	// Token: 0x04000690 RID: 1680
	[SerializeField]
	private SFXComponent destroySfx;

	// Token: 0x04000691 RID: 1681
	private bool _isActivated;
}
