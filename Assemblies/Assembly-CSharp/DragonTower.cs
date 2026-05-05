using System;
using System.Collections;
using System.Collections.Generic;
using FMODUnity;
using Mirror;
using Mirror.RemoteCalls;
using TMPro;
using UnityEngine;

// Token: 0x0200004B RID: 75
public class DragonTower : GameBase
{
	// Token: 0x0600020B RID: 523 RVA: 0x0000BA38 File Offset: 0x00009C38
	[Server]
	protected override void StartGame()
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void DragonTower::StartGame()' called when server was not active");
			return;
		}
		base.StartGame();
		this.sfxLoopComponent.RpcLoopSFX(true);
		this.SetEggs();
		this.RpcSetInteractableButtons(0);
		this.RpcSetMultiplierText(1.0);
	}

	// Token: 0x0600020C RID: 524 RVA: 0x0000BA88 File Offset: 0x00009C88
	private void SetEggs()
	{
		Random seededRandom = base.GetSeededRandom(0);
		foreach (DragonTower.Floor floor in this.floors)
		{
			floor.eggIndex = seededRandom.Next(0, 4);
		}
	}

	// Token: 0x0600020D RID: 525 RVA: 0x0000BAE8 File Offset: 0x00009CE8
	[Server]
	public void OnPressButton(int floorIndex, int buttonIndex, DragonTowerButton button)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void DragonTower::OnPressButton(System.Int32,System.Int32,DragonTowerButton)' called when server was not active");
			return;
		}
		if (!this.isPlaying)
		{
			return;
		}
		if (this._currentFloor != floorIndex)
		{
			return;
		}
		if (this._hasEnded)
		{
			return;
		}
		if (buttonIndex == this.floors[floorIndex].eggIndex)
		{
			button.ServerSetButtonState(DragonTowerButton.ButtonState.Red);
			this.Lose();
			return;
		}
		button.ServerSetButtonState(DragonTowerButton.ButtonState.Green);
		this.ProgressGame();
	}

	// Token: 0x0600020E RID: 526 RVA: 0x0000BB58 File Offset: 0x00009D58
	private void ProgressGame()
	{
		this._currentFloor++;
		this.RpcSetMultiplierText(this.GetMultiplier(this._currentFloor));
		if (this._currentFloor > this.floors.Count - 1)
		{
			this.Win();
			return;
		}
		this.RpcStepSfx();
		this.RpcSetInteractableButtons(this._currentFloor);
	}

	// Token: 0x0600020F RID: 527 RVA: 0x0000BBB3 File Offset: 0x00009DB3
	[Server]
	public void Cashout(PlayerInteract playerInteract)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void DragonTower::Cashout(PlayerInteract)' called when server was not active");
			return;
		}
		if (!this.isPlaying)
		{
			return;
		}
		if (this._hasEnded)
		{
			return;
		}
		if (this._currentFloor <= 0)
		{
			return;
		}
		this.Win();
	}

	// Token: 0x06000210 RID: 528 RVA: 0x0000BBEC File Offset: 0x00009DEC
	private void Win()
	{
		this._hasEnded = true;
		this.sfxLoopComponent.RpcLoopSFX(false);
		double multiplier = this.GetMultiplier(this._currentFloor);
		this.RpcSetAnimator(true);
		this.RevealEggs();
		this.Payout(multiplier, ChangeType.GameResult, null, -1L);
		this.winSfx.PlayOneShotWith3DPos();
		base.StartCoroutine(this.ResetGameRoutine());
	}

	// Token: 0x06000211 RID: 529 RVA: 0x0000BC4C File Offset: 0x00009E4C
	private void Lose()
	{
		this._hasEnded = true;
		this.sfxLoopComponent.RpcLoopSFX(false);
		this.RpcSetAnimator(false);
		this.RevealEggs();
		this.Payout(0.0, ChangeType.GameResult, null, -1L);
		this.loseSfx.PlayOneShotWith3DPos();
		base.StartCoroutine(this.ResetGameRoutine());
	}

	// Token: 0x06000212 RID: 530 RVA: 0x0000BCA4 File Offset: 0x00009EA4
	private void RevealEggs()
	{
		for (int i = 0; i < this._currentFloor; i++)
		{
			DragonTower.Floor floor = this.floors[i];
			floor.buttons[floor.eggIndex].ServerSetButtonState(DragonTowerButton.ButtonState.RevealEgg);
		}
	}

	// Token: 0x06000213 RID: 531 RVA: 0x0000BCE8 File Offset: 0x00009EE8
	private double GetMultiplier(int stage)
	{
		if (stage <= 0)
		{
			return 1.0;
		}
		double num = Math.Pow(0.75, (double)stage);
		return 1.0 / num * base.EstimatedValue;
	}

	// Token: 0x06000214 RID: 532 RVA: 0x0000BD26 File Offset: 0x00009F26
	private IEnumerator ResetGameRoutine()
	{
		yield return new WaitForSeconds(0.8f);
		this.RpcSetInteractableButtons(-1);
		yield return new WaitForSeconds(0.2f);
		this.ResetGame();
		yield break;
	}

	// Token: 0x06000215 RID: 533 RVA: 0x0000BD35 File Offset: 0x00009F35
	protected override void ResetGame()
	{
		this.RpcSetInteractableButtons(-1);
		this.RpcSetMultiplierText(0.0);
		base.ResetGame();
		this._hasEnded = false;
		this._currentFloor = 0;
	}

	// Token: 0x06000216 RID: 534 RVA: 0x0000BD64 File Offset: 0x00009F64
	[ClientRpc]
	private void RpcSetInteractableButtons(int floorIndex)
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		writer.WriteVarInt(floorIndex);
		this.SendRPCInternal("System.Void DragonTower::RpcSetInteractableButtons(System.Int32)", 2121373762, writer, 0, true);
		NetworkWriterPool.Return(writer);
	}

	// Token: 0x06000217 RID: 535 RVA: 0x0000BDA0 File Offset: 0x00009FA0
	[ClientRpc]
	private void RpcSetMultiplierText(double multiplier)
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		writer.WriteDouble(multiplier);
		this.SendRPCInternal("System.Void DragonTower::RpcSetMultiplierText(System.Double)", -1489341436, writer, 0, true);
		NetworkWriterPool.Return(writer);
	}

	// Token: 0x06000218 RID: 536 RVA: 0x0000BDDC File Offset: 0x00009FDC
	[ClientRpc]
	private void RpcSetAnimator(bool isWin)
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		writer.WriteBool(isWin);
		this.SendRPCInternal("System.Void DragonTower::RpcSetAnimator(System.Boolean)", 751021246, writer, 0, true);
		NetworkWriterPool.Return(writer);
	}

	// Token: 0x06000219 RID: 537 RVA: 0x0000BE16 File Offset: 0x0000A016
	private void PlayFireVfx()
	{
		this.fireVfx.Play();
	}

	// Token: 0x0600021A RID: 538 RVA: 0x0000BE24 File Offset: 0x0000A024
	[ClientRpc]
	private void RpcStepSfx()
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		this.SendRPCInternal("System.Void DragonTower::RpcStepSfx()", -1454015945, writer, 0, true);
		NetworkWriterPool.Return(writer);
	}

	// Token: 0x0600021C RID: 540 RVA: 0x00002321 File Offset: 0x00000521
	public override bool Weaved()
	{
		return true;
	}

	// Token: 0x0600021D RID: 541 RVA: 0x0000BE68 File Offset: 0x0000A068
	protected void UserCode_RpcSetInteractableButtons__Int32(int floorIndex)
	{
		for (int i = 0; i < this.floors.Count; i++)
		{
			foreach (DragonTowerButton dragonTowerButton in this.floors[i].buttons)
			{
				if (i < floorIndex)
				{
					if (dragonTowerButton.buttonState == DragonTowerButton.ButtonState.Clickable)
					{
						dragonTowerButton.ServerSetButtonState(DragonTowerButton.ButtonState.Inactive);
					}
				}
				else if (i == floorIndex)
				{
					dragonTowerButton.ServerSetButtonState(DragonTowerButton.ButtonState.Clickable);
				}
				else
				{
					dragonTowerButton.ServerSetButtonState(DragonTowerButton.ButtonState.Inactive);
				}
			}
		}
	}

	// Token: 0x0600021E RID: 542 RVA: 0x0000BF00 File Offset: 0x0000A100
	protected static void InvokeUserCode_RpcSetInteractableButtons__Int32(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcSetInteractableButtons called on server.");
			return;
		}
		((DragonTower)obj).UserCode_RpcSetInteractableButtons__Int32(reader.ReadVarInt());
	}

	// Token: 0x0600021F RID: 543 RVA: 0x0000BF2C File Offset: 0x0000A12C
	protected void UserCode_RpcSetMultiplierText__Double(double multiplier)
	{
		this.multiplierText.text = multiplier.ToString("0.##") + "x";
		this.potentialWinningText.text = "$" + ((long)Math.Round((double)this.currentBet * multiplier)).ToString("N0");
	}

	// Token: 0x06000220 RID: 544 RVA: 0x0000BF8B File Offset: 0x0000A18B
	protected static void InvokeUserCode_RpcSetMultiplierText__Double(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcSetMultiplierText called on server.");
			return;
		}
		((DragonTower)obj).UserCode_RpcSetMultiplierText__Double(reader.ReadDouble());
	}

	// Token: 0x06000221 RID: 545 RVA: 0x0000BFB5 File Offset: 0x0000A1B5
	protected void UserCode_RpcSetAnimator__Boolean(bool isWin)
	{
		if (isWin)
		{
			this.animator.SetTrigger("Win");
			return;
		}
		this.animator.SetTrigger("Lose");
	}

	// Token: 0x06000222 RID: 546 RVA: 0x0000BFDB File Offset: 0x0000A1DB
	protected static void InvokeUserCode_RpcSetAnimator__Boolean(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcSetAnimator called on server.");
			return;
		}
		((DragonTower)obj).UserCode_RpcSetAnimator__Boolean(reader.ReadBool());
	}

	// Token: 0x06000223 RID: 547 RVA: 0x0000C004 File Offset: 0x0000A204
	protected void UserCode_RpcStepSfx()
	{
		SFXManager.SFXOneShotWithParameters(this.stepSfx, null, base.transform.position, 1f + (float)this._currentFloor / (float)this.floors.Count);
	}

	// Token: 0x06000224 RID: 548 RVA: 0x0000C037 File Offset: 0x0000A237
	protected static void InvokeUserCode_RpcStepSfx(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcStepSfx called on server.");
			return;
		}
		((DragonTower)obj).UserCode_RpcStepSfx();
	}

	// Token: 0x06000225 RID: 549 RVA: 0x0000C05C File Offset: 0x0000A25C
	static DragonTower()
	{
		RemoteProcedureCalls.RegisterRpc(typeof(DragonTower), "System.Void DragonTower::RpcSetInteractableButtons(System.Int32)", new RemoteCallDelegate(DragonTower.InvokeUserCode_RpcSetInteractableButtons__Int32));
		RemoteProcedureCalls.RegisterRpc(typeof(DragonTower), "System.Void DragonTower::RpcSetMultiplierText(System.Double)", new RemoteCallDelegate(DragonTower.InvokeUserCode_RpcSetMultiplierText__Double));
		RemoteProcedureCalls.RegisterRpc(typeof(DragonTower), "System.Void DragonTower::RpcSetAnimator(System.Boolean)", new RemoteCallDelegate(DragonTower.InvokeUserCode_RpcSetAnimator__Boolean));
		RemoteProcedureCalls.RegisterRpc(typeof(DragonTower), "System.Void DragonTower::RpcStepSfx()", new RemoteCallDelegate(DragonTower.InvokeUserCode_RpcStepSfx));
	}

	// Token: 0x040001B1 RID: 433
	[Header("References")]
	[SerializeField]
	private Animator animator;

	// Token: 0x040001B2 RID: 434
	[SerializeField]
	private ParticleSystem fireVfx;

	// Token: 0x040001B3 RID: 435
	[SerializeField]
	private TextMeshPro multiplierText;

	// Token: 0x040001B4 RID: 436
	[SerializeField]
	private TextMeshPro potentialWinningText;

	// Token: 0x040001B5 RID: 437
	[SerializeField]
	private List<DragonTower.Floor> floors = new List<DragonTower.Floor>();

	// Token: 0x040001B6 RID: 438
	private bool _hasEnded;

	// Token: 0x040001B7 RID: 439
	private int _currentFloor;

	// Token: 0x040001B8 RID: 440
	[Header("SFX")]
	[SerializeField]
	private SFXLoopComponent sfxLoopComponent;

	// Token: 0x040001B9 RID: 441
	[SerializeField]
	private SFXComponent loseSfx;

	// Token: 0x040001BA RID: 442
	[SerializeField]
	private SFXComponent winSfx;

	// Token: 0x040001BB RID: 443
	[SerializeField]
	private EventReference stepSfx;

	// Token: 0x0200004C RID: 76
	[Serializable]
	private class Floor
	{
		// Token: 0x040001BC RID: 444
		public List<DragonTowerButton> buttons = new List<DragonTowerButton>();

		// Token: 0x040001BD RID: 445
		public int eggIndex;
	}
}
