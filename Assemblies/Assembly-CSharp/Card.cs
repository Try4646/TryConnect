using System;
using System.Runtime.InteropServices;
using FMODUnity;
using Mirror;
using Mirror.RemoteCalls;
using UnityEngine;

// Token: 0x02000090 RID: 144
public class Card : NetworkBehaviour
{
	// Token: 0x06000532 RID: 1330 RVA: 0x00017423 File Offset: 0x00015623
	private void OnCardDataChanged(CardData oldValue, CardData newValue)
	{
		this.NetworkcardData = newValue;
		this.LoadCardDataSO();
		this.UpdateCardSprite();
	}

	// Token: 0x06000533 RID: 1331 RVA: 0x00017438 File Offset: 0x00015638
	private void LoadCardDataSO()
	{
		if (this.cardData.Suit == Suit.Hearts && this.cardData.Rank == (Rank)0)
		{
			return;
		}
		string rankName = this.GetRankName(this.cardData.Rank);
		string text = string.Format("Card_{0}_{1}", this.cardData.Suit, rankName);
		this.cardDataSO = Resources.Load<CardDataSO>(text);
		if (this.cardDataSO == null)
		{
			Debug.LogWarning("[Card] Could not load CardDataSO: " + text);
		}
	}

	// Token: 0x06000534 RID: 1332 RVA: 0x000174B8 File Offset: 0x000156B8
	private string GetRankName(Rank rank)
	{
		string result;
		if (rank != Rank.Ace)
		{
			switch (rank)
			{
			case Rank.Jack:
				result = "Jack";
				break;
			case Rank.Queen:
				result = "Queen";
				break;
			case Rank.King:
				result = "King";
				break;
			default:
			{
				int num = (int)rank;
				result = num.ToString();
				break;
			}
			}
		}
		else
		{
			result = "Ace";
		}
		return result;
	}

	// Token: 0x06000535 RID: 1333 RVA: 0x0001750C File Offset: 0x0001570C
	private void UpdateCardSprite()
	{
		if (this.spriteRenderer == null)
		{
			return;
		}
		if (this.isFaceDown || this.cardDataSO == null || this.cardDataSO.cardSprite == null)
		{
			this.spriteRenderer.sprite = null;
			return;
		}
		this.spriteRenderer.sprite = this.cardDataSO.cardSprite;
	}

	// Token: 0x06000536 RID: 1334 RVA: 0x00017574 File Offset: 0x00015774
	protected override void OnValidate()
	{
		base.OnValidate();
		if (this.cardData.Suit != Suit.Hearts || this.cardData.Rank != (Rank)0)
		{
			this.LoadCardDataSO();
			this.UpdateCardSprite();
		}
	}

	// Token: 0x06000537 RID: 1335 RVA: 0x000175A2 File Offset: 0x000157A2
	[Server]
	public void ServerSetCardData(CardData newCardData)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void Card::ServerSetCardData(CardData)' called when server was not active");
			return;
		}
		this.NetworkcardData = newCardData;
	}

	// Token: 0x06000538 RID: 1336 RVA: 0x000175C0 File Offset: 0x000157C0
	[ClientRpc]
	public void RpcSetFaceDown(bool faceDown)
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		writer.WriteBool(faceDown);
		this.SendRPCInternal("System.Void Card::RpcSetFaceDown(System.Boolean)", -690581388, writer, 0, true);
		NetworkWriterPool.Return(writer);
	}

	// Token: 0x06000539 RID: 1337 RVA: 0x000175FA File Offset: 0x000157FA
	public void SetFaceDownDirect(bool faceDown)
	{
		this.isFaceDown = faceDown;
		this.UpdateCardSprite();
	}

	// Token: 0x0600053A RID: 1338 RVA: 0x00017609 File Offset: 0x00015809
	public Card()
	{
		this._Mirror_SyncVarHookDelegate_cardData = new Action<CardData, CardData>(this.OnCardDataChanged);
	}

	// Token: 0x0600053B RID: 1339 RVA: 0x00002321 File Offset: 0x00000521
	public override bool Weaved()
	{
		return true;
	}

	// Token: 0x17000088 RID: 136
	// (get) Token: 0x0600053C RID: 1340 RVA: 0x00017624 File Offset: 0x00015824
	// (set) Token: 0x0600053D RID: 1341 RVA: 0x00017637 File Offset: 0x00015837
	public CardData NetworkcardData
	{
		get
		{
			return this.cardData;
		}
		[param: In]
		set
		{
			base.GeneratedSyncVarSetter<CardData>(value, ref this.cardData, 1UL, this._Mirror_SyncVarHookDelegate_cardData);
		}
	}

	// Token: 0x0600053E RID: 1342 RVA: 0x00017656 File Offset: 0x00015856
	protected void UserCode_RpcSetFaceDown__Boolean(bool faceDown)
	{
		SFXManager.SFXOneShot(this.sFXEventGenericSwipe, base.transform.position);
		this.isFaceDown = faceDown;
		this.UpdateCardSprite();
	}

	// Token: 0x0600053F RID: 1343 RVA: 0x0001767B File Offset: 0x0001587B
	protected static void InvokeUserCode_RpcSetFaceDown__Boolean(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcSetFaceDown called on server.");
			return;
		}
		((Card)obj).UserCode_RpcSetFaceDown__Boolean(reader.ReadBool());
	}

	// Token: 0x06000540 RID: 1344 RVA: 0x000176A4 File Offset: 0x000158A4
	static Card()
	{
		RemoteProcedureCalls.RegisterRpc(typeof(Card), "System.Void Card::RpcSetFaceDown(System.Boolean)", new RemoteCallDelegate(Card.InvokeUserCode_RpcSetFaceDown__Boolean));
	}

	// Token: 0x06000541 RID: 1345 RVA: 0x000176C8 File Offset: 0x000158C8
	public override void SerializeSyncVars(NetworkWriter writer, bool forceAll)
	{
		base.SerializeSyncVars(writer, forceAll);
		if (forceAll)
		{
			Mirror.GeneratedNetworkCode._Write_CardData(writer, this.cardData);
			return;
		}
		writer.WriteVarULong(this.syncVarDirtyBits);
		if ((this.syncVarDirtyBits & 1UL) != 0UL)
		{
			Mirror.GeneratedNetworkCode._Write_CardData(writer, this.cardData);
		}
	}

	// Token: 0x06000542 RID: 1346 RVA: 0x00017720 File Offset: 0x00015920
	public override void DeserializeSyncVars(NetworkReader reader, bool initialState)
	{
		base.DeserializeSyncVars(reader, initialState);
		if (initialState)
		{
			base.GeneratedSyncVarDeserialize<CardData>(ref this.cardData, this._Mirror_SyncVarHookDelegate_cardData, Mirror.GeneratedNetworkCode._Read_CardData(reader));
			return;
		}
		long num = (long)reader.ReadVarULong();
		if ((num & 1L) != 0L)
		{
			base.GeneratedSyncVarDeserialize<CardData>(ref this.cardData, this._Mirror_SyncVarHookDelegate_cardData, Mirror.GeneratedNetworkCode._Read_CardData(reader));
		}
	}

	// Token: 0x040003A1 RID: 929
	[Header("Card Data")]
	[SerializeField]
	[SyncVar(hook = "OnCardDataChanged")]
	private CardData cardData;

	// Token: 0x040003A2 RID: 930
	[SerializeField]
	private SpriteRenderer spriteRenderer;

	// Token: 0x040003A3 RID: 931
	[Header("SFX")]
	[SerializeField]
	private EventReference sFXEventGenericSwipe;

	// Token: 0x040003A4 RID: 932
	private CardDataSO cardDataSO;

	// Token: 0x040003A5 RID: 933
	private bool isFaceDown;

	// Token: 0x040003A6 RID: 934
	public Action<CardData, CardData> _Mirror_SyncVarHookDelegate_cardData;
}
