using System;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

// Token: 0x02000095 RID: 149
public class DiceManager : NetworkBehaviour
{
	// Token: 0x14000001 RID: 1
	// (add) Token: 0x06000548 RID: 1352 RVA: 0x000178A4 File Offset: 0x00015AA4
	// (remove) Token: 0x06000549 RID: 1353 RVA: 0x000178DC File Offset: 0x00015ADC
	public event Action<int> OnDiceRolled;

	// Token: 0x0600054A RID: 1354 RVA: 0x00017914 File Offset: 0x00015B14
	public override void OnStartServer()
	{
		base.OnStartServer();
		for (int i = 0; i < this.diceCount; i++)
		{
			Dice dice = Object.Instantiate<Dice>(this.dicePrefab, this.diceSpawnPoint.position, this.diceSpawnPoint.rotation);
			NetworkServer.Spawn(dice.gameObject, null);
			this._diceList.Add(dice);
		}
		foreach (Dice dice2 in this._diceList)
		{
			dice2.OnDiceStopped += this.HandleDiceStopped;
		}
	}

	// Token: 0x0600054B RID: 1355 RVA: 0x000179C4 File Offset: 0x00015BC4
	public override void OnStopServer()
	{
		base.OnStopServer();
		foreach (Dice dice in this._diceList.ToArray())
		{
			if (!(dice == null))
			{
				dice.OnDiceStopped -= this.HandleDiceStopped;
				if (dice.gameObject != null)
				{
					NetworkServer.Destroy(dice.gameObject);
				}
			}
		}
		this._diceList.Clear();
	}

	// Token: 0x0600054C RID: 1356 RVA: 0x00017A34 File Offset: 0x00015C34
	private void OnEnable()
	{
		if (base.isServer)
		{
			foreach (Dice dice in this._diceList)
			{
				dice.ServerSetEnabled(true);
			}
		}
	}

	// Token: 0x0600054D RID: 1357 RVA: 0x00017A90 File Offset: 0x00015C90
	private void OnDisable()
	{
		if (base.isServer)
		{
			foreach (Dice dice in this._diceList)
			{
				dice.ServerSetEnabled(false);
			}
		}
	}

	// Token: 0x0600054E RID: 1358 RVA: 0x00017AEC File Offset: 0x00015CEC
	private void HandleDiceStopped(Dice dice, int result)
	{
		if (this._rolledDice.Contains(dice))
		{
			return;
		}
		if (!this.IsInZone(dice.transform.position))
		{
			this.ResetDice(dice);
			return;
		}
		this._rolledDice.Add(dice);
		dice.LockDice(true);
		this._currentResult += result;
		this.CheckAllDices();
	}

	// Token: 0x0600054F RID: 1359 RVA: 0x00017B4A File Offset: 0x00015D4A
	private void CheckAllDices()
	{
		if (this._rolledDice.Count < this._diceList.Count)
		{
			return;
		}
		this.diceStoppedSfx.RpcPlayOneShotWith3DPos();
		Action<int> onDiceRolled = this.OnDiceRolled;
		if (onDiceRolled == null)
		{
			return;
		}
		onDiceRolled(this._currentResult);
	}

	// Token: 0x06000550 RID: 1360 RVA: 0x00017B88 File Offset: 0x00015D88
	public void ResetRound()
	{
		foreach (Dice dice in this._diceList)
		{
			this.ResetDice(dice);
		}
		this._currentResult = 0;
		this._rolledDice.Clear();
	}

	// Token: 0x06000551 RID: 1361 RVA: 0x00017BF0 File Offset: 0x00015DF0
	private bool IsInZone(Vector3 position)
	{
		Vector3 vector = this.diceZone.InverseTransformPoint(position);
		return Mathf.Abs(vector.x) <= 0.5f && Mathf.Abs(vector.y) <= 0.5f && Mathf.Abs(vector.z) <= 0.5f;
	}

	// Token: 0x06000552 RID: 1362 RVA: 0x00017C45 File Offset: 0x00015E45
	private void ResetDice(Dice dice)
	{
		dice.ServerResetDice(this.diceSpawnPoint.position);
		dice.LockDice(false);
		this.resetSfx.RpcPlayOneShotWith3DPos();
	}

	// Token: 0x06000553 RID: 1363 RVA: 0x00017C6C File Offset: 0x00015E6C
	public void LockDices(bool isLocked)
	{
		foreach (Dice dice in this._diceList)
		{
			dice.LockDice(isLocked);
		}
	}

	// Token: 0x06000555 RID: 1365 RVA: 0x00002321 File Offset: 0x00000521
	public override bool Weaved()
	{
		return true;
	}

	// Token: 0x040003BD RID: 957
	[SerializeField]
	private int diceCount;

	// Token: 0x040003BE RID: 958
	[SerializeField]
	private Dice dicePrefab;

	// Token: 0x040003BF RID: 959
	[SerializeField]
	private Transform diceZone;

	// Token: 0x040003C0 RID: 960
	[SerializeField]
	private Transform diceSpawnPoint;

	// Token: 0x040003C2 RID: 962
	private int _currentResult;

	// Token: 0x040003C3 RID: 963
	private List<Dice> _diceList = new List<Dice>();

	// Token: 0x040003C4 RID: 964
	private List<Dice> _rolledDice = new List<Dice>();

	// Token: 0x040003C5 RID: 965
	[SerializeField]
	private SFXComponent resetSfx;

	// Token: 0x040003C6 RID: 966
	[SerializeField]
	private SFXComponent diceStoppedSfx;
}
