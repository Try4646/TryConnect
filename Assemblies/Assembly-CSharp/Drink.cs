using System;
using System.Collections;
using System.Collections.Generic;
using Extensions;
using Mirror;
using Mirror.RemoteCalls;
using UnityEngine;

// Token: 0x020000E8 RID: 232
public class Drink : ConsumableItem
{
	// Token: 0x0600095B RID: 2395 RVA: 0x00025928 File Offset: 0x00023B28
	protected override void OnUseItem(bool isPressed)
	{
		if (!this._canDrink)
		{
			this.cantDrinkSfx.PlayOneShotAttached();
			return;
		}
		this.anim.SetBool("IsDrinking", isPressed);
		this.drinkSfx.RpcLoopSFX(isPressed);
		if (base.isServer)
		{
			if (this._drinkRoutine != null)
			{
				base.StopCoroutine(this._drinkRoutine);
			}
			if (isPressed)
			{
				this._drinkRoutine = base.StartCoroutine(this.DrinkRoutine());
			}
		}
	}

	// Token: 0x0600095C RID: 2396 RVA: 0x00025997 File Offset: 0x00023B97
	private IEnumerator DrinkRoutine()
	{
		yield return new WaitForSeconds(this.drinkTime);
		this.OnDrank();
		yield break;
	}

	// Token: 0x0600095D RID: 2397 RVA: 0x000259A8 File Offset: 0x00023BA8
	private void OnDrank()
	{
		if (!base.isServer)
		{
			return;
		}
		PlayerInventory networkHolder = base.NetworkHolder;
		if (networkHolder != null)
		{
			networkHolder.GetComponent<PlayerBuff>().ApplyBuff(PlayerBuffType.TipsyFortune, this.upgradeAmount * NetworkSingleton<UpgradeManager>.Instance.GetUpgradeData(this._lastHolder.steamId, PlayerUpgradeType.Stakeholder), this.upgradeDurationDrink);
		}
		PlayerVoiceFX playerVoiceFX;
		if (base.NetworkHolder.TryGetComponent<PlayerVoiceFX>(out playerVoiceFX))
		{
			playerVoiceFX.RpcStartTimedVoiceFX(VoipManipulationManager.VoipFX.Wobble, this.upgradeDurationDrink, true);
		}
		base.DestroyItem();
	}

	// Token: 0x0600095E RID: 2398 RVA: 0x00025A1C File Offset: 0x00023C1C
	private void OnCollisionEnter(Collision other)
	{
		if (!base.isServer)
		{
			return;
		}
		if (base.NetworkHolder)
		{
			return;
		}
		if (!this._isBreakable)
		{
			return;
		}
		if (other.impulse.magnitude < this.shatterThreshold)
		{
			return;
		}
		this.Shatter();
	}

	// Token: 0x0600095F RID: 2399 RVA: 0x00025A66 File Offset: 0x00023C66
	private IEnumerator SetUnbreakableRoutine()
	{
		while (this._isBreakable)
		{
			if (this.Rb.linearVelocity.sqrMagnitude < 0.01f)
			{
				this._isBreakable = false;
			}
			yield return new WaitForFixedUpdate();
		}
		yield break;
	}

	// Token: 0x06000960 RID: 2400 RVA: 0x00025A78 File Offset: 0x00023C78
	private void Shatter()
	{
		List<PlayerBuff> list = new List<PlayerBuff>();
		foreach (Collider collider in Physics.OverlapSphere(base.transform.position, this.radius, LayerMask.GetMask(new string[]
		{
			"Player"
		})))
		{
			PlayerBuff item;
			if (collider.attachedRigidbody && collider.attachedRigidbody.TryGetComponent<PlayerBuff>(out item) && !list.Contains(item))
			{
				list.Add(item);
			}
		}
		foreach (PlayerBuff playerBuff in list)
		{
			playerBuff.ApplyBuff(PlayerBuffType.TipsyFortune, this.upgradeAmount * NetworkSingleton<UpgradeManager>.Instance.GetUpgradeData(this._lastHolder.steamId, PlayerUpgradeType.Stakeholder), this.upgradeDurationThrow);
			PlayerVoiceFX playerVoiceFX;
			if (playerBuff.TryGetComponent<PlayerVoiceFX>(out playerVoiceFX))
			{
				playerVoiceFX.RpcStartTimedVoiceFX(VoipManipulationManager.VoipFX.Wobble, this.upgradeDurationThrow, true);
			}
		}
		base.DestroyItem();
	}

	// Token: 0x06000961 RID: 2401 RVA: 0x00025B78 File Offset: 0x00023D78
	public override void ServerThrow(Vector3 position, Quaternion rotation, Vector3 velocity, Vector3 angularVelocity)
	{
		base.ServerThrow(position, rotation, velocity, angularVelocity);
		if (velocity.magnitude < this.throwThreshold)
		{
			this._isBreakable = false;
			return;
		}
		this._isBreakable = true;
		if (this._setUnbreakableRoutine != null)
		{
			base.StopCoroutine(this._setUnbreakableRoutine);
		}
		this._setUnbreakableRoutine = base.StartCoroutine(this.SetUnbreakableRoutine());
	}

	// Token: 0x06000962 RID: 2402 RVA: 0x00025BD4 File Offset: 0x00023DD4
	[Server]
	public override void ServerTeleport(Vector3 position)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void Drink::ServerTeleport(UnityEngine.Vector3)' called when server was not active");
			return;
		}
		if (base.NetworkHolder)
		{
			return;
		}
		this._isBreakable = false;
		this.Rb.Teleport(position, true);
	}

	// Token: 0x06000963 RID: 2403 RVA: 0x00025C0D File Offset: 0x00023E0D
	protected override void OnPickedUp(PlayerInventory playerInventory)
	{
		base.OnPickedUp(playerInventory);
		this._lastHolder = playerInventory.GetComponent<PlayerProfile>();
		this.RpcSetCanDrink(NetworkSingleton<OrganManager>.Instance.GetOrganData(this._lastHolder.steamId).mouth);
	}

	// Token: 0x06000964 RID: 2404 RVA: 0x00025C42 File Offset: 0x00023E42
	protected override void OnDropped(PlayerInventory playerInventory)
	{
		base.OnDropped(playerInventory);
		if (this._drinkRoutine != null)
		{
			base.StopCoroutine(this._drinkRoutine);
		}
		this.RpcSetCanDrink(false);
		this.RpcOnDropped();
	}

	// Token: 0x06000965 RID: 2405 RVA: 0x00025C6C File Offset: 0x00023E6C
	[ClientRpc]
	private void RpcOnDropped()
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		this.SendRPCInternal("System.Void Drink::RpcOnDropped()", 1556049145, writer, 0, true);
		NetworkWriterPool.Return(writer);
	}

	// Token: 0x06000966 RID: 2406 RVA: 0x00025C9C File Offset: 0x00023E9C
	[ClientRpc]
	private void RpcSetCanDrink(bool canDrink)
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		writer.WriteBool(canDrink);
		this.SendRPCInternal("System.Void Drink::RpcSetCanDrink(System.Boolean)", -1915179511, writer, 0, true);
		NetworkWriterPool.Return(writer);
	}

	// Token: 0x06000968 RID: 2408 RVA: 0x00002321 File Offset: 0x00000521
	public override bool Weaved()
	{
		return true;
	}

	// Token: 0x06000969 RID: 2409 RVA: 0x00025D38 File Offset: 0x00023F38
	protected void UserCode_RpcOnDropped()
	{
		this.anim.SetBool("IsDrinking", false);
		this.anim.Play("Default", 0, 0f);
		this.anim.Update(0f);
		this.drinkSfx.RpcLoopSFX(false);
	}

	// Token: 0x0600096A RID: 2410 RVA: 0x00025D88 File Offset: 0x00023F88
	protected static void InvokeUserCode_RpcOnDropped(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcOnDropped called on server.");
			return;
		}
		((Drink)obj).UserCode_RpcOnDropped();
	}

	// Token: 0x0600096B RID: 2411 RVA: 0x00025DAB File Offset: 0x00023FAB
	protected void UserCode_RpcSetCanDrink__Boolean(bool canDrink)
	{
		this._canDrink = canDrink;
	}

	// Token: 0x0600096C RID: 2412 RVA: 0x00025DB4 File Offset: 0x00023FB4
	protected static void InvokeUserCode_RpcSetCanDrink__Boolean(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcSetCanDrink called on server.");
			return;
		}
		((Drink)obj).UserCode_RpcSetCanDrink__Boolean(reader.ReadBool());
	}

	// Token: 0x0600096D RID: 2413 RVA: 0x00025DE0 File Offset: 0x00023FE0
	static Drink()
	{
		RemoteProcedureCalls.RegisterRpc(typeof(Drink), "System.Void Drink::RpcOnDropped()", new RemoteCallDelegate(Drink.InvokeUserCode_RpcOnDropped));
		RemoteProcedureCalls.RegisterRpc(typeof(Drink), "System.Void Drink::RpcSetCanDrink(System.Boolean)", new RemoteCallDelegate(Drink.InvokeUserCode_RpcSetCanDrink__Boolean));
	}

	// Token: 0x040005F1 RID: 1521
	[SerializeField]
	private Animator anim;

	// Token: 0x040005F2 RID: 1522
	[Header("Settings")]
	[SerializeField]
	private float drinkTime = 1.5f;

	// Token: 0x040005F3 RID: 1523
	[SerializeField]
	private float radius = 4f;

	// Token: 0x040005F4 RID: 1524
	[SerializeField]
	private float upgradeAmount = 1f;

	// Token: 0x040005F5 RID: 1525
	[SerializeField]
	private float upgradeDurationThrow = 10f;

	// Token: 0x040005F6 RID: 1526
	[SerializeField]
	private float upgradeDurationDrink = 30f;

	// Token: 0x040005F7 RID: 1527
	[SerializeField]
	private float throwThreshold = 10f;

	// Token: 0x040005F8 RID: 1528
	[SerializeField]
	private float shatterThreshold = 0.5f;

	// Token: 0x040005F9 RID: 1529
	private bool _isBreakable;

	// Token: 0x040005FA RID: 1530
	private Coroutine _setUnbreakableRoutine;

	// Token: 0x040005FB RID: 1531
	private PlayerProfile _lastHolder;

	// Token: 0x040005FC RID: 1532
	private Coroutine _drinkRoutine;

	// Token: 0x040005FD RID: 1533
	private bool _canDrink;

	// Token: 0x040005FE RID: 1534
	[Header("SFX")]
	[SerializeField]
	private SFXLoopComponent drinkSfx;

	// Token: 0x040005FF RID: 1535
	[SerializeField]
	private SFXComponent cantDrinkSfx;
}
