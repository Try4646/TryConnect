using System;
using System.Collections;
using System.Runtime.InteropServices;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using Mirror;
using Mirror.RemoteCalls;
using MoreMountains.Feedbacks;
using UnityEngine;

// Token: 0x020000E5 RID: 229
public class Dice : Item
{
	// Token: 0x14000007 RID: 7
	// (add) Token: 0x06000930 RID: 2352 RVA: 0x00024F5C File Offset: 0x0002315C
	// (remove) Token: 0x06000931 RID: 2353 RVA: 0x00024F94 File Offset: 0x00023194
	public event Action<Dice, int> OnDiceStopped;

	// Token: 0x06000932 RID: 2354 RVA: 0x00024FC9 File Offset: 0x000231C9
	public override void OnStartServer()
	{
		base.OnStartServer();
		this.Network_randomIndex = Random.Range(int.MinValue, int.MaxValue);
	}

	// Token: 0x06000933 RID: 2355 RVA: 0x00024FE8 File Offset: 0x000231E8
	private Random GetRandom()
	{
		int randomIndex = this._randomIndex;
		this.Network_randomIndex = this._randomIndex + 1;
		long num = ((long)randomIndex * (long)((ulong)-1640531535) + (long)randomIndex) * (long)((ulong)-1640531535) + (long)randomIndex;
		long num2 = (num ^ num >> 32) * (long)((ulong)-2048144789);
		long num3 = (num2 ^ num2 >> 16) * (long)((ulong)-1028477379);
		return new Random((int)(num3 ^ num3 >> 13));
	}

	// Token: 0x06000934 RID: 2356 RVA: 0x00025044 File Offset: 0x00023244
	protected override void OnDropped(PlayerInventory playerInventory)
	{
		base.OnDropped(playerInventory);
		this.diceModel.DOKill(false);
		this.diceModel.localPosition = Vector3.zero;
		this.diceModel.localRotation = this._currentRotation;
		this._isRolling = true;
		this.IsInteractable = false;
		this.CursorType = CursorManager.CursorType.Default;
		this.trail.emitting = true;
		this.RpcSetInteractable(false);
		base.StartCoroutine(this.CheckIfStopped());
	}

	// Token: 0x06000935 RID: 2357 RVA: 0x000250BB File Offset: 0x000232BB
	protected override void OnPickedUp(PlayerInventory playerInventory)
	{
		base.OnPickedUp(playerInventory);
		this.RpcSetDiceRotation();
	}

	// Token: 0x06000936 RID: 2358 RVA: 0x000250CC File Offset: 0x000232CC
	private void OnCollisionEnter(Collision other)
	{
		if (!base.isServer)
		{
			return;
		}
		if (other.impulse.sqrMagnitude <= 0.01f)
		{
			return;
		}
		this.RpcOnHitVFX();
	}

	// Token: 0x06000937 RID: 2359 RVA: 0x00025100 File Offset: 0x00023300
	[ClientRpc]
	private void RpcOnHitVFX()
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		this.SendRPCInternal("System.Void Dice::RpcOnHitVFX()", -267509617, writer, 0, true);
		NetworkWriterPool.Return(writer);
	}

	// Token: 0x06000938 RID: 2360 RVA: 0x00025130 File Offset: 0x00023330
	protected override void OnUseItem(bool isPressed)
	{
		base.OnUseItem(isPressed);
		if (this._isShaking)
		{
			return;
		}
		this._isShaking = true;
		int rotationIndex = this.GetRandom().Next(0, Dice.DiceRotations.Length);
		base.StartCoroutine(this.ShakeDice(rotationIndex));
		this.shakeSfx.PlayOneShotAttached();
	}

	// Token: 0x06000939 RID: 2361 RVA: 0x00025181 File Offset: 0x00023381
	private IEnumerator ShakeDice(int rotationIndex)
	{
		this.anim.SetTrigger("Shake");
		this.diceModel.DOLocalJump(Vector3.zero, 0.5f, 1, 0.5f, false).SetEase(this.jumpCurve);
		this._currentRotation = Dice.DiceRotations[rotationIndex];
		this.diceModel.DOLocalRotate(this._currentRotation.eulerAngles, 0.5f, RotateMode.Fast).SetEase(Ease.OutQuad);
		yield return new WaitForSeconds(0.5f);
		this._isShaking = false;
		yield break;
	}

	// Token: 0x0600093A RID: 2362 RVA: 0x00025198 File Offset: 0x00023398
	[ClientRpc]
	private void RpcSetDiceRotation()
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		this.SendRPCInternal("System.Void Dice::RpcSetDiceRotation()", 518578692, writer, 0, true);
		NetworkWriterPool.Return(writer);
	}

	// Token: 0x0600093B RID: 2363 RVA: 0x000251C8 File Offset: 0x000233C8
	public override void ServerThrow(Vector3 position, Quaternion rotation, Vector3 velocity, Vector3 angularVelocity)
	{
		if (base.NetworkHolder)
		{
			base.ServerDrop();
		}
		this.Rb.Teleport(position, false);
		this.Rb.Rotate(rotation, false);
		this.Rb.linearVelocity = velocity;
		this.Rb.angularVelocity = angularVelocity * 10f;
	}

	// Token: 0x0600093C RID: 2364 RVA: 0x00025225 File Offset: 0x00023425
	private IEnumerator CheckIfStopped()
	{
		float timer = 0f;
		while (this._isRolling)
		{
			timer += Time.deltaTime;
			if (timer >= 10f)
			{
				this.StopDice();
				yield break;
			}
			if (this.Rb.linearVelocity.magnitude < this.stopThreshold && this.Rb.angularVelocity.magnitude < this.stopThreshold)
			{
				yield return new WaitForSeconds(this.checkDelay);
				if (this.Rb.linearVelocity.magnitude < this.stopThreshold && this.Rb.angularVelocity.magnitude < this.stopThreshold)
				{
					this.StopDice();
					yield break;
				}
			}
			yield return null;
		}
		yield break;
	}

	// Token: 0x0600093D RID: 2365 RVA: 0x00025234 File Offset: 0x00023434
	private void StopDice()
	{
		this._isRolling = false;
		this.IsInteractable = true;
		this.CursorType = CursorManager.CursorType.Interact;
		this.trail.emitting = false;
		this.RpcSetInteractable(true);
		this.Rb.linearVelocity = Vector3.zero;
		this.Rb.angularVelocity = Vector3.zero;
		Action<Dice, int> onDiceStopped = this.OnDiceStopped;
		if (onDiceStopped == null)
		{
			return;
		}
		onDiceStopped(this, this.GetTopFaceIndex());
	}

	// Token: 0x0600093E RID: 2366 RVA: 0x000252A0 File Offset: 0x000234A0
	private int GetTopFaceIndex()
	{
		int result = 0;
		float num = float.NegativeInfinity;
		for (int i = 0; i < this.faces.Length; i++)
		{
			if (this.faces[i].position.y > num)
			{
				num = this.faces[i].position.y;
				result = i + 1;
			}
		}
		return result;
	}

	// Token: 0x0600093F RID: 2367 RVA: 0x000252F8 File Offset: 0x000234F8
	[Server]
	public void LockDice(bool isLocked)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void Dice::LockDice(System.Boolean)' called when server was not active");
			return;
		}
		this.IsInteractable = !isLocked;
		this.CursorType = ((!isLocked) ? CursorManager.CursorType.Interact : CursorManager.CursorType.Default);
		this.RpcSetInteractable(!isLocked);
		this.Rb.isKinematic = isLocked;
	}

	// Token: 0x06000940 RID: 2368 RVA: 0x00025348 File Offset: 0x00023548
	[ClientRpc]
	private void RpcSetInteractable(bool isInteractable)
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		writer.WriteBool(isInteractable);
		this.SendRPCInternal("System.Void Dice::RpcSetInteractable(System.Boolean)", -1328582806, writer, 0, true);
		NetworkWriterPool.Return(writer);
	}

	// Token: 0x06000941 RID: 2369 RVA: 0x00025382 File Offset: 0x00023582
	[Server]
	public void ServerResetDice(Vector3 pos)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void Dice::ServerResetDice(UnityEngine.Vector3)' called when server was not active");
			return;
		}
		this.Rb.Teleport(pos, false);
		this.Rb.Rotate(Quaternion.identity, false);
	}

	// Token: 0x06000943 RID: 2371 RVA: 0x000253E0 File Offset: 0x000235E0
	static Dice()
	{
		RemoteProcedureCalls.RegisterRpc(typeof(Dice), "System.Void Dice::RpcOnHitVFX()", new RemoteCallDelegate(Dice.InvokeUserCode_RpcOnHitVFX));
		RemoteProcedureCalls.RegisterRpc(typeof(Dice), "System.Void Dice::RpcSetDiceRotation()", new RemoteCallDelegate(Dice.InvokeUserCode_RpcSetDiceRotation));
		RemoteProcedureCalls.RegisterRpc(typeof(Dice), "System.Void Dice::RpcSetInteractable(System.Boolean)", new RemoteCallDelegate(Dice.InvokeUserCode_RpcSetInteractable__Boolean));
	}

	// Token: 0x06000944 RID: 2372 RVA: 0x00002321 File Offset: 0x00000521
	public override bool Weaved()
	{
		return true;
	}

	// Token: 0x170000D0 RID: 208
	// (get) Token: 0x06000945 RID: 2373 RVA: 0x000254FC File Offset: 0x000236FC
	// (set) Token: 0x06000946 RID: 2374 RVA: 0x0002550F File Offset: 0x0002370F
	public int Network_randomIndex
	{
		get
		{
			return this._randomIndex;
		}
		[param: In]
		set
		{
			base.GeneratedSyncVarSetter<int>(value, ref this._randomIndex, 2UL, null);
		}
	}

	// Token: 0x06000947 RID: 2375 RVA: 0x00025529 File Offset: 0x00023729
	protected void UserCode_RpcOnHitVFX()
	{
		this.onHitFb.PlayFeedbacks();
	}

	// Token: 0x06000948 RID: 2376 RVA: 0x00025536 File Offset: 0x00023736
	protected static void InvokeUserCode_RpcOnHitVFX(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcOnHitVFX called on server.");
			return;
		}
		((Dice)obj).UserCode_RpcOnHitVFX();
	}

	// Token: 0x06000949 RID: 2377 RVA: 0x0002555C File Offset: 0x0002375C
	protected void UserCode_RpcSetDiceRotation()
	{
		int num = this.GetRandom().Next(0, Dice.DiceRotations.Length);
		this._currentRotation = Dice.DiceRotations[num];
		this.diceModel.localRotation = this._currentRotation;
	}

	// Token: 0x0600094A RID: 2378 RVA: 0x0002559F File Offset: 0x0002379F
	protected static void InvokeUserCode_RpcSetDiceRotation(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcSetDiceRotation called on server.");
			return;
		}
		((Dice)obj).UserCode_RpcSetDiceRotation();
	}

	// Token: 0x0600094B RID: 2379 RVA: 0x000255C4 File Offset: 0x000237C4
	protected void UserCode_RpcSetInteractable__Boolean(bool isInteractable)
	{
		if (base.isServer)
		{
			return;
		}
		this.IsInteractable = isInteractable;
		this.CursorType = (isInteractable ? CursorManager.CursorType.Interact : CursorManager.CursorType.Default);
		this.trail.emitting = !isInteractable;
		if (!isInteractable)
		{
			this.diceModel.DOKill(false);
			this.diceModel.localPosition = Vector3.zero;
			this.diceModel.localRotation = this._currentRotation;
		}
	}

	// Token: 0x0600094C RID: 2380 RVA: 0x0002562E File Offset: 0x0002382E
	protected static void InvokeUserCode_RpcSetInteractable__Boolean(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcSetInteractable called on server.");
			return;
		}
		((Dice)obj).UserCode_RpcSetInteractable__Boolean(reader.ReadBool());
	}

	// Token: 0x0600094D RID: 2381 RVA: 0x00025658 File Offset: 0x00023858
	public override void SerializeSyncVars(NetworkWriter writer, bool forceAll)
	{
		base.SerializeSyncVars(writer, forceAll);
		if (forceAll)
		{
			writer.WriteVarInt(this._randomIndex);
			return;
		}
		writer.WriteVarULong(this.syncVarDirtyBits);
		if ((this.syncVarDirtyBits & 2UL) != 0UL)
		{
			writer.WriteVarInt(this._randomIndex);
		}
	}

	// Token: 0x0600094E RID: 2382 RVA: 0x000256B0 File Offset: 0x000238B0
	public override void DeserializeSyncVars(NetworkReader reader, bool initialState)
	{
		base.DeserializeSyncVars(reader, initialState);
		if (initialState)
		{
			base.GeneratedSyncVarDeserialize<int>(ref this._randomIndex, null, reader.ReadVarInt());
			return;
		}
		long num = (long)reader.ReadVarULong();
		if ((num & 2L) != 0L)
		{
			base.GeneratedSyncVarDeserialize<int>(ref this._randomIndex, null, reader.ReadVarInt());
		}
	}

	// Token: 0x040005DA RID: 1498
	[Header("Dice Settings")]
	[SerializeField]
	private float stopThreshold = 0.1f;

	// Token: 0x040005DB RID: 1499
	[SerializeField]
	private float checkDelay = 0.5f;

	// Token: 0x040005DC RID: 1500
	[SerializeField]
	private AnimationCurve jumpCurve;

	// Token: 0x040005DD RID: 1501
	[Header("References")]
	[SerializeField]
	private Transform[] faces;

	// Token: 0x040005DE RID: 1502
	[SerializeField]
	private Transform diceModel;

	// Token: 0x040005DF RID: 1503
	[SerializeField]
	private Animator anim;

	// Token: 0x040005E0 RID: 1504
	[SerializeField]
	private MMF_Player onHitFb;

	// Token: 0x040005E1 RID: 1505
	[SerializeField]
	private TrailRenderer trail;

	// Token: 0x040005E2 RID: 1506
	[SyncVar]
	private int _randomIndex;

	// Token: 0x040005E3 RID: 1507
	[Header("SFX")]
	[SerializeField]
	private SFXComponent shakeSfx;

	// Token: 0x040005E4 RID: 1508
	private static readonly Quaternion[] DiceRotations = new Quaternion[]
	{
		Quaternion.Euler(0f, 0f, 0f),
		Quaternion.Euler(0f, 0f, 90f),
		Quaternion.Euler(90f, 0f, 0f),
		Quaternion.Euler(-90f, 0f, 0f),
		Quaternion.Euler(0f, 0f, -90f),
		Quaternion.Euler(180f, 0f, 0f)
	};

	// Token: 0x040005E6 RID: 1510
	private bool _isRolling;

	// Token: 0x040005E7 RID: 1511
	private bool _isShaking;

	// Token: 0x040005E8 RID: 1512
	private Quaternion _currentRotation = Quaternion.identity;
}
