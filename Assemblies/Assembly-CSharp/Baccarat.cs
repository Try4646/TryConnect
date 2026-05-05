using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Mirror;
using Mirror.RemoteCalls;
using UnityEngine;

// Token: 0x02000024 RID: 36
public class Baccarat : GameBase
{
	// Token: 0x0600008E RID: 142 RVA: 0x00004EF4 File Offset: 0x000030F4
	protected override void StartGame()
	{
		base.StartGame();
		if (!this.deckInitialized)
		{
			this.InitializeDeck();
			this.initialDeckCount = this.deck.Count;
			this.deckInitialized = true;
			if (this.deckOfCardsTransform != null)
			{
				this.NetworkdeckScaleY = this.baseScaleForOneDeck * (float)this.numberOfDecks;
				this.deckOfCardsTransform.localScale = new Vector3(this.deckOfCardsTransform.localScale.x, this.deckScaleY, this.deckOfCardsTransform.localScale.z);
			}
		}
		base.StartCoroutine(this.DealInitialCardsRoutine());
		SFXComponent sfxcomponent = this.dealerSfx;
		if (sfxcomponent == null)
		{
			return;
		}
		sfxcomponent.PlayOneShotWith3DPos();
	}

	// Token: 0x0600008F RID: 143 RVA: 0x00004FA2 File Offset: 0x000031A2
	private IEnumerator DealInitialCardsRoutine()
	{
		this.gameState = Baccarat.BaccaratGameState.Dealing;
		this.DealCardToPlayer(false);
		yield return new WaitForSeconds(0.5f);
		this.DealCardToDealer(false);
		yield return new WaitForSeconds(0.5f);
		this.DealCardToPlayer(false);
		yield return new WaitForSeconds(0.5f);
		this.DealCardToDealer(false);
		yield return new WaitForSeconds(2f);
		int handValue = this.GetHandValue(this.playerHand);
		int handValue2 = this.GetHandValue(this.dealerHand);
		BaccaratResult result;
		if (handValue > handValue2)
		{
			result = BaccaratResult.Player;
		}
		else if (handValue2 > handValue)
		{
			result = BaccaratResult.Banker;
		}
		else
		{
			result = BaccaratResult.Tie;
		}
		this.EndGame(result);
		yield break;
	}

	// Token: 0x06000090 RID: 144 RVA: 0x00004FB4 File Offset: 0x000031B4
	[Server]
	private void InitializeDeck()
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void Baccarat::InitializeDeck()' called when server was not active");
			return;
		}
		this.deck.Clear();
		for (int i = 0; i < this.numberOfDecks; i++)
		{
			foreach (object obj in Enum.GetValues(typeof(Suit)))
			{
				Suit suit = (Suit)obj;
				foreach (object obj2 in Enum.GetValues(typeof(Rank)))
				{
					Rank rank = (Rank)obj2;
					if (rank != (Rank)0)
					{
						this.deck.Add(new CardData(suit, rank));
					}
				}
			}
		}
		this.ShuffleDeck();
	}

	// Token: 0x06000091 RID: 145 RVA: 0x000050B4 File Offset: 0x000032B4
	[Server]
	private void ShuffleDeck()
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void Baccarat::ShuffleDeck()' called when server was not active");
			return;
		}
		Random seededRandom = base.GetSeededRandom(this.deck.Count * 10000);
		for (int i = this.deck.Count - 1; i > 0; i--)
		{
			int index = seededRandom.Next(0, i + 1);
			CardData value = this.deck[i];
			this.deck[i] = this.deck[index];
			this.deck[index] = value;
		}
	}

	// Token: 0x06000092 RID: 146 RVA: 0x00005144 File Offset: 0x00003344
	[Server]
	private CardData DrawCardFromDeck()
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'CardData Baccarat::DrawCardFromDeck()' called when server was not active");
			return default(CardData);
		}
		if (this.deck.Count == 0)
		{
			Debug.LogWarning("[Blackjack] Deck is empty! Reinitializing...");
			this.InitializeDeck();
			this.initialDeckCount = this.deck.Count;
			if (this.deckOfCardsTransform != null)
			{
				this.NetworkdeckScaleY = this.baseScaleForOneDeck * (float)this.numberOfDecks;
			}
		}
		CardData result = this.deck[0];
		this.deck.RemoveAt(0);
		if (this.deckOfCardsTransform != null)
		{
			if (this.deck.Count == 0)
			{
				this.NetworkdeckScaleY = -0.1f;
				return result;
			}
			if (this.initialDeckCount > 0)
			{
				float num = (float)this.deck.Count / (float)this.initialDeckCount;
				this.NetworkdeckScaleY = this.baseScaleForOneDeck * (float)this.numberOfDecks * num;
			}
		}
		return result;
	}

	// Token: 0x06000093 RID: 147 RVA: 0x00005238 File Offset: 0x00003438
	private void OnDeckScaleChanged(float oldValue, float newValue)
	{
		if (this.deckOfCardsTransform != null)
		{
			this.deckOfCardsTransform.localScale = new Vector3(this.deckOfCardsTransform.localScale.x, newValue, this.deckOfCardsTransform.localScale.z);
		}
	}

	// Token: 0x06000094 RID: 148 RVA: 0x00005284 File Offset: 0x00003484
	[Server]
	private void DealCardToPlayer(bool isFaceDown = false)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void Baccarat::DealCardToPlayer(System.Boolean)' called when server was not active");
			return;
		}
		this.DealCardToHand(this.playerHand, this.spawnedPlayerCards, Baccarat.CardAreaType.Player, isFaceDown);
	}

	// Token: 0x06000095 RID: 149 RVA: 0x000052B0 File Offset: 0x000034B0
	[Server]
	private void DealCardToDealer(bool isFaceDown = false)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void Baccarat::DealCardToDealer(System.Boolean)' called when server was not active");
			return;
		}
		CardData cardData = this.DrawCardFromDeck();
		this.dealerHand.Add(cardData);
		this.SpawnCard(cardData, Baccarat.CardAreaType.Dealer, this.spawnedDealerCards, isFaceDown);
		this.UpdateCasinoHelperCounts();
	}

	// Token: 0x06000096 RID: 150 RVA: 0x000052FC File Offset: 0x000034FC
	[Server]
	private void DealCardToHand(SyncList<CardData> hand, List<GameObject> cardList, Baccarat.CardAreaType areaType, bool isFaceDown = false)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void Baccarat::DealCardToHand(Mirror.SyncList`1<CardData>,System.Collections.Generic.List`1<UnityEngine.GameObject>,Baccarat/CardAreaType,System.Boolean)' called when server was not active");
			return;
		}
		CardData cardData = this.DrawCardFromDeck();
		hand.Add(cardData);
		this.SpawnCard(cardData, areaType, cardList, isFaceDown);
		this.UpdateCasinoHelperCounts();
	}

	// Token: 0x06000097 RID: 151 RVA: 0x00005340 File Offset: 0x00003540
	[Server]
	private void SpawnCard(CardData cardData, Baccarat.CardAreaType areaType, List<GameObject> cardList, bool isHidden)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void Baccarat::SpawnCard(CardData,Baccarat/CardAreaType,System.Collections.Generic.List`1<UnityEngine.GameObject>,System.Boolean)' called when server was not active");
			return;
		}
		if (Resources.Load<CardDataSO>(string.Format("Card_{0}_{1}", cardData.Suit, this.GetRankName(cardData.Rank))) == null)
		{
			Debug.LogWarning(string.Format("[Blackjack] Could not load CardDataSO: Card_{0}_{1}", cardData.Suit, this.GetRankName(cardData.Rank)));
		}
		Vector3 vector = this.CalculateCardPosition(areaType, cardList.Count, cardList.Count + 1);
		GameObject gameObject = Object.Instantiate<GameObject>(this.cardPrefab);
		NetworkServer.Spawn(gameObject, null);
		Card component = gameObject.GetComponent<Card>();
		if (component != null)
		{
			component.ServerSetCardData(cardData);
		}
		Transform cardAreaTransform = this.GetCardAreaTransform(areaType);
		if (cardAreaTransform != null)
		{
			gameObject.transform.SetParent(cardAreaTransform);
			gameObject.transform.localPosition = vector;
			gameObject.transform.localRotation = Quaternion.identity;
		}
		this.RpcSetCardParentAndPosition(gameObject, areaType, vector);
		cardList.Add(gameObject);
		this.RepositionAllCards(areaType, cardList.Count);
		this.RpcRepositionAllCards(areaType, cardList.Count);
		if (isHidden)
		{
			if (component != null)
			{
				component.RpcSetFaceDown(true);
				return;
			}
		}
		else if (component != null)
		{
			component.RpcSetFaceDown(false);
		}
	}

	// Token: 0x06000098 RID: 152 RVA: 0x00005474 File Offset: 0x00003674
	[ClientRpc]
	private void RpcSetCardParentAndPosition(GameObject cardObject, Baccarat.CardAreaType areaType, Vector3 cardLocalPosition)
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		writer.WriteGameObject(cardObject);
		Mirror.GeneratedNetworkCode._Write_Baccarat/CardAreaType(writer, areaType);
		writer.WriteVector3(cardLocalPosition);
		this.SendRPCInternal("System.Void Baccarat::RpcSetCardParentAndPosition(UnityEngine.GameObject,Baccarat/CardAreaType,UnityEngine.Vector3)", -69855219, writer, 0, true);
		NetworkWriterPool.Return(writer);
	}

	// Token: 0x06000099 RID: 153 RVA: 0x000054C4 File Offset: 0x000036C4
	[Server]
	private Vector3 CalculateCardPosition(Baccarat.CardAreaType areaType, int cardIndex, int totalCardCount)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'UnityEngine.Vector3 Baccarat::CalculateCardPosition(Baccarat/CardAreaType,System.Int32,System.Int32)' called when server was not active");
			return default(Vector3);
		}
		if (totalCardCount <= 1)
		{
			return Vector3.zero;
		}
		float d = -((float)(totalCardCount - 1) * this.cardSpacing) / 2f + (float)cardIndex * this.cardSpacing;
		return Vector3.right * d;
	}

	// Token: 0x0600009A RID: 154 RVA: 0x00005524 File Offset: 0x00003724
	[Server]
	private void RepositionAllCards(Baccarat.CardAreaType areaType, int totalCardCount)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void Baccarat::RepositionAllCards(Baccarat/CardAreaType,System.Int32)' called when server was not active");
			return;
		}
		List<GameObject> list = (areaType == Baccarat.CardAreaType.Player) ? this.spawnedPlayerCards : this.spawnedDealerCards;
		Transform cardAreaTransform = this.GetCardAreaTransform(areaType);
		if (cardAreaTransform == null)
		{
			return;
		}
		List<Transform> list2 = new List<Transform>();
		int num = 0;
		while (num < list.Count && num < totalCardCount)
		{
			if (list[num] != null)
			{
				list2.Add(list[num].transform);
			}
			num++;
		}
		base.StartCoroutine(this.RepositionCardsSmoothRoutine(list2, cardAreaTransform, areaType, totalCardCount));
	}

	// Token: 0x0600009B RID: 155 RVA: 0x000055B8 File Offset: 0x000037B8
	[ClientRpc]
	private void RpcRepositionAllCards(Baccarat.CardAreaType areaType, int totalCardCount)
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		Mirror.GeneratedNetworkCode._Write_Baccarat/CardAreaType(writer, areaType);
		writer.WriteVarInt(totalCardCount);
		this.SendRPCInternal("System.Void Baccarat::RpcRepositionAllCards(Baccarat/CardAreaType,System.Int32)", -2109413821, writer, 0, true);
		NetworkWriterPool.Return(writer);
	}

	// Token: 0x0600009C RID: 156 RVA: 0x000055FC File Offset: 0x000037FC
	private IEnumerator RepositionCardsSmoothRoutine(List<Transform> cardTransforms, Transform cardArea, Baccarat.CardAreaType areaType, int totalCardCount)
	{
		int num = 0;
		while (num < cardTransforms.Count && num < totalCardCount)
		{
			if (cardTransforms[num] != null)
			{
				Vector3 targetPosition = this.CalculateCardPositionLocal(cardArea, areaType, num, totalCardCount);
				base.StartCoroutine(this.MoveCardSmoothRoutine(cardTransforms[num], targetPosition));
			}
			num++;
		}
		yield return null;
		yield break;
	}

	// Token: 0x0600009D RID: 157 RVA: 0x00005628 File Offset: 0x00003828
	private Vector3 CalculateCardPositionLocal(Transform cardArea, Baccarat.CardAreaType areaType, int cardIndex, int totalCardCount)
	{
		if (totalCardCount <= 1)
		{
			return Vector3.zero;
		}
		float d = -((float)(totalCardCount - 1) * this.cardSpacing) / 2f + (float)cardIndex * this.cardSpacing;
		return Vector3.right * d;
	}

	// Token: 0x0600009E RID: 158 RVA: 0x00005669 File Offset: 0x00003869
	private IEnumerator MoveCardSmoothRoutine(Transform cardTransform, Vector3 targetPosition)
	{
		while (Vector3.Distance(cardTransform.localPosition, targetPosition) > 0.01f)
		{
			cardTransform.localPosition = Vector3.MoveTowards(cardTransform.localPosition, targetPosition, this.cardMoveSpeed * Time.deltaTime);
			yield return null;
		}
		cardTransform.localPosition = targetPosition;
		yield break;
	}

	// Token: 0x0600009F RID: 159 RVA: 0x00005688 File Offset: 0x00003888
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

	// Token: 0x060000A0 RID: 160 RVA: 0x000056DC File Offset: 0x000038DC
	[Server]
	private void EndGame(BaccaratResult result)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void Baccarat::EndGame(BaccaratResult)' called when server was not active");
			return;
		}
		this.gameState = Baccarat.BaccaratGameState.Finished;
		double multiplier = 0.0;
		switch (this.currentBetType)
		{
		case BaccaratBetType.Player:
			if (result == BaccaratResult.Player)
			{
				multiplier = 2.0 * base.EstimatedValue;
			}
			break;
		case BaccaratBetType.Banker:
			if (result == BaccaratResult.Banker)
			{
				multiplier = 2.0 * base.EstimatedValue;
			}
			break;
		case BaccaratBetType.Tie:
			if (result == BaccaratResult.Tie)
			{
				multiplier = 8.0 * base.EstimatedValue;
			}
			break;
		}
		this.Payout(multiplier, ChangeType.GameResult, null, -1L);
		base.StartCoroutine(this.ResetGameRoutine());
	}

	// Token: 0x060000A1 RID: 161 RVA: 0x00005784 File Offset: 0x00003984
	private IEnumerator ResetGameRoutine()
	{
		yield return new WaitForSeconds(1f);
		this.ResetGame();
		yield break;
	}

	// Token: 0x060000A2 RID: 162 RVA: 0x00005794 File Offset: 0x00003994
	[Server]
	protected override void ResetGame()
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void Baccarat::ResetGame()' called when server was not active");
			return;
		}
		base.ResetGame();
		this.playerHand.Clear();
		this.dealerHand.Clear();
		this.gameState = Baccarat.BaccaratGameState.Waiting;
		this.CleanupCards(this.spawnedPlayerCards);
		this.CleanupCards(this.spawnedDealerCards);
		this.spawnedPlayerCards.Clear();
		this.spawnedDealerCards.Clear();
		this.RpcClearCasinoHelperTexts();
		if (this.resetCardsSfx != null && this.dealerCardArea != null)
		{
			this.resetCardsSfx.RpcPlayOneShotWithCustom3DPos(this.dealerCardArea.position);
		}
	}

	// Token: 0x060000A3 RID: 163 RVA: 0x00005840 File Offset: 0x00003A40
	[Server]
	private void CleanupCards(List<GameObject> cardList)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void Baccarat::CleanupCards(System.Collections.Generic.List`1<UnityEngine.GameObject>)' called when server was not active");
			return;
		}
		foreach (GameObject gameObject in cardList)
		{
			if (gameObject != null)
			{
				NetworkServer.Destroy(gameObject);
			}
		}
	}

	// Token: 0x060000A4 RID: 164 RVA: 0x000058AC File Offset: 0x00003AAC
	private int GetHandValue(SyncList<CardData> hand)
	{
		if (hand.Count == 0)
		{
			return 0;
		}
		int num = 0;
		foreach (CardData cardData in hand)
		{
			num += cardData.GetBaccaratValue();
		}
		return num % 10;
	}

	// Token: 0x060000A5 RID: 165 RVA: 0x00005910 File Offset: 0x00003B10
	[Server]
	private void UpdateCasinoHelperCounts()
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void Baccarat::UpdateCasinoHelperCounts()' called when server was not active");
			return;
		}
		int handValue = this.GetHandValue(this.playerHand);
		int handValue2 = this.GetHandValue(this.dealerHand);
		this.RpcUpdateCasinoHelperCounts(handValue, handValue2);
	}

	// Token: 0x060000A6 RID: 166 RVA: 0x00005954 File Offset: 0x00003B54
	[ClientRpc]
	private void RpcUpdateCasinoHelperCounts(int playerTotal, int dealerTotal)
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		writer.WriteVarInt(playerTotal);
		writer.WriteVarInt(dealerTotal);
		this.SendRPCInternal("System.Void Baccarat::RpcUpdateCasinoHelperCounts(System.Int32,System.Int32)", -2084194017, writer, 0, true);
		NetworkWriterPool.Return(writer);
	}

	// Token: 0x060000A7 RID: 167 RVA: 0x00005998 File Offset: 0x00003B98
	[ClientRpc]
	private void RpcClearCasinoHelperTexts()
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		this.SendRPCInternal("System.Void Baccarat::RpcClearCasinoHelperTexts()", 835280477, writer, 0, true);
		NetworkWriterPool.Return(writer);
	}

	// Token: 0x060000A8 RID: 168 RVA: 0x000059C8 File Offset: 0x00003BC8
	private Transform GetCardAreaTransform(Baccarat.CardAreaType areaType)
	{
		if (areaType == Baccarat.CardAreaType.Player)
		{
			return this.playerCardArea;
		}
		if (areaType != Baccarat.CardAreaType.Dealer)
		{
			return null;
		}
		return this.dealerCardArea;
	}

	// Token: 0x060000A9 RID: 169 RVA: 0x000059E2 File Offset: 0x00003BE2
	[Server]
	public void PlaceBetOnPlayer(PlayerInteract playerInteract)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void Baccarat::PlaceBetOnPlayer(PlayerInteract)' called when server was not active");
			return;
		}
		if (this.gameState != Baccarat.BaccaratGameState.Waiting)
		{
			return;
		}
		this.TryStartGame(playerInteract);
		this.NetworkcurrentBetType = BaccaratBetType.Player;
	}

	// Token: 0x060000AA RID: 170 RVA: 0x00005A10 File Offset: 0x00003C10
	[Server]
	public void PlaceBetOnDealer(PlayerInteract playerInteract)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void Baccarat::PlaceBetOnDealer(PlayerInteract)' called when server was not active");
			return;
		}
		if (this.gameState != Baccarat.BaccaratGameState.Waiting)
		{
			return;
		}
		this.TryStartGame(playerInteract);
		this.NetworkcurrentBetType = BaccaratBetType.Banker;
	}

	// Token: 0x060000AB RID: 171 RVA: 0x00005A3E File Offset: 0x00003C3E
	[Server]
	public void PlaceBetOnTie(PlayerInteract playerInteract)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void Baccarat::PlaceBetOnTie(PlayerInteract)' called when server was not active");
			return;
		}
		if (this.gameState != Baccarat.BaccaratGameState.Waiting)
		{
			return;
		}
		this.TryStartGame(playerInteract);
		this.NetworkcurrentBetType = BaccaratBetType.Tie;
	}

	// Token: 0x060000AC RID: 172 RVA: 0x00005A6C File Offset: 0x00003C6C
	public Baccarat()
	{
		base.InitSyncObject(this.playerHand);
		base.InitSyncObject(this.dealerHand);
		this._Mirror_SyncVarHookDelegate_deckScaleY = new Action<float, float>(this.OnDeckScaleChanged);
	}

	// Token: 0x060000AD RID: 173 RVA: 0x00002321 File Offset: 0x00000521
	public override bool Weaved()
	{
		return true;
	}

	// Token: 0x1700000F RID: 15
	// (get) Token: 0x060000AE RID: 174 RVA: 0x00005B14 File Offset: 0x00003D14
	// (set) Token: 0x060000AF RID: 175 RVA: 0x00005B27 File Offset: 0x00003D27
	public BaccaratBetType NetworkcurrentBetType
	{
		get
		{
			return this.currentBetType;
		}
		[param: In]
		set
		{
			base.GeneratedSyncVarSetter<BaccaratBetType>(value, ref this.currentBetType, 8UL, null);
		}
	}

	// Token: 0x17000010 RID: 16
	// (get) Token: 0x060000B0 RID: 176 RVA: 0x00005B44 File Offset: 0x00003D44
	// (set) Token: 0x060000B1 RID: 177 RVA: 0x00005B57 File Offset: 0x00003D57
	public float NetworkdeckScaleY
	{
		get
		{
			return this.deckScaleY;
		}
		[param: In]
		set
		{
			base.GeneratedSyncVarSetter<float>(value, ref this.deckScaleY, 16UL, this._Mirror_SyncVarHookDelegate_deckScaleY);
		}
	}

	// Token: 0x060000B2 RID: 178 RVA: 0x00005B78 File Offset: 0x00003D78
	protected void UserCode_RpcSetCardParentAndPosition__GameObject__CardAreaType__Vector3(GameObject cardObject, Baccarat.CardAreaType areaType, Vector3 cardLocalPosition)
	{
		Transform cardAreaTransform = this.GetCardAreaTransform(areaType);
		if (cardAreaTransform != null && cardObject != null)
		{
			cardObject.transform.SetParent(cardAreaTransform);
			cardObject.transform.localPosition = cardLocalPosition;
			cardObject.transform.localRotation = Quaternion.identity;
		}
	}

	// Token: 0x060000B3 RID: 179 RVA: 0x00005BC7 File Offset: 0x00003DC7
	protected static void InvokeUserCode_RpcSetCardParentAndPosition__GameObject__CardAreaType__Vector3(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcSetCardParentAndPosition called on server.");
			return;
		}
		((Baccarat)obj).UserCode_RpcSetCardParentAndPosition__GameObject__CardAreaType__Vector3(reader.ReadGameObject(), Mirror.GeneratedNetworkCode._Read_Baccarat/CardAreaType(reader), reader.ReadVector3());
	}

	// Token: 0x060000B4 RID: 180 RVA: 0x00005BFC File Offset: 0x00003DFC
	protected void UserCode_RpcRepositionAllCards__CardAreaType__Int32(Baccarat.CardAreaType areaType, int totalCardCount)
	{
		Transform cardAreaTransform = this.GetCardAreaTransform(areaType);
		if (cardAreaTransform == null)
		{
			return;
		}
		List<Transform> list = new List<Transform>();
		for (int i = 0; i < cardAreaTransform.childCount; i++)
		{
			Transform child = cardAreaTransform.GetChild(i);
			if (child.GetComponent<Card>() != null)
			{
				list.Add(child);
			}
		}
		list.Sort((Transform a, Transform b) => a.localPosition.x.CompareTo(b.localPosition.x));
		base.StartCoroutine(this.RepositionCardsSmoothRoutine(list, cardAreaTransform, areaType, totalCardCount));
	}

	// Token: 0x060000B5 RID: 181 RVA: 0x00005C85 File Offset: 0x00003E85
	protected static void InvokeUserCode_RpcRepositionAllCards__CardAreaType__Int32(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcRepositionAllCards called on server.");
			return;
		}
		((Baccarat)obj).UserCode_RpcRepositionAllCards__CardAreaType__Int32(Mirror.GeneratedNetworkCode._Read_Baccarat/CardAreaType(reader), reader.ReadVarInt());
	}

	// Token: 0x060000B6 RID: 182 RVA: 0x00005CB4 File Offset: 0x00003EB4
	protected void UserCode_RpcUpdateCasinoHelperCounts__Int32__Int32(int playerTotal, int dealerTotal)
	{
		if (!base.IsCasinoHelperEnabled)
		{
			base.ClearCasinoHelperTexts();
			return;
		}
		base.SetCasinoHelperText(0, playerTotal.ToString());
		base.SetCasinoHelperText(1, dealerTotal.ToString());
	}

	// Token: 0x060000B7 RID: 183 RVA: 0x00005CE1 File Offset: 0x00003EE1
	protected static void InvokeUserCode_RpcUpdateCasinoHelperCounts__Int32__Int32(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcUpdateCasinoHelperCounts called on server.");
			return;
		}
		((Baccarat)obj).UserCode_RpcUpdateCasinoHelperCounts__Int32__Int32(reader.ReadVarInt(), reader.ReadVarInt());
	}

	// Token: 0x060000B8 RID: 184 RVA: 0x00005D10 File Offset: 0x00003F10
	protected void UserCode_RpcClearCasinoHelperTexts()
	{
		base.ClearCasinoHelperTexts();
	}

	// Token: 0x060000B9 RID: 185 RVA: 0x00005D18 File Offset: 0x00003F18
	protected static void InvokeUserCode_RpcClearCasinoHelperTexts(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcClearCasinoHelperTexts called on server.");
			return;
		}
		((Baccarat)obj).UserCode_RpcClearCasinoHelperTexts();
	}

	// Token: 0x060000BA RID: 186 RVA: 0x00005D3C File Offset: 0x00003F3C
	static Baccarat()
	{
		RemoteProcedureCalls.RegisterRpc(typeof(Baccarat), "System.Void Baccarat::RpcSetCardParentAndPosition(UnityEngine.GameObject,Baccarat/CardAreaType,UnityEngine.Vector3)", new RemoteCallDelegate(Baccarat.InvokeUserCode_RpcSetCardParentAndPosition__GameObject__CardAreaType__Vector3));
		RemoteProcedureCalls.RegisterRpc(typeof(Baccarat), "System.Void Baccarat::RpcRepositionAllCards(Baccarat/CardAreaType,System.Int32)", new RemoteCallDelegate(Baccarat.InvokeUserCode_RpcRepositionAllCards__CardAreaType__Int32));
		RemoteProcedureCalls.RegisterRpc(typeof(Baccarat), "System.Void Baccarat::RpcUpdateCasinoHelperCounts(System.Int32,System.Int32)", new RemoteCallDelegate(Baccarat.InvokeUserCode_RpcUpdateCasinoHelperCounts__Int32__Int32));
		RemoteProcedureCalls.RegisterRpc(typeof(Baccarat), "System.Void Baccarat::RpcClearCasinoHelperTexts()", new RemoteCallDelegate(Baccarat.InvokeUserCode_RpcClearCasinoHelperTexts));
	}

	// Token: 0x060000BB RID: 187 RVA: 0x00005DCC File Offset: 0x00003FCC
	public override void SerializeSyncVars(NetworkWriter writer, bool forceAll)
	{
		base.SerializeSyncVars(writer, forceAll);
		if (forceAll)
		{
			Mirror.GeneratedNetworkCode._Write_BaccaratBetType(writer, this.currentBetType);
			writer.WriteFloat(this.deckScaleY);
			return;
		}
		writer.WriteVarULong(this.syncVarDirtyBits);
		if ((this.syncVarDirtyBits & 8UL) != 0UL)
		{
			Mirror.GeneratedNetworkCode._Write_BaccaratBetType(writer, this.currentBetType);
		}
		if ((this.syncVarDirtyBits & 16UL) != 0UL)
		{
			writer.WriteFloat(this.deckScaleY);
		}
	}

	// Token: 0x060000BC RID: 188 RVA: 0x00005E54 File Offset: 0x00004054
	public override void DeserializeSyncVars(NetworkReader reader, bool initialState)
	{
		base.DeserializeSyncVars(reader, initialState);
		if (initialState)
		{
			base.GeneratedSyncVarDeserialize<BaccaratBetType>(ref this.currentBetType, null, Mirror.GeneratedNetworkCode._Read_BaccaratBetType(reader));
			base.GeneratedSyncVarDeserialize<float>(ref this.deckScaleY, this._Mirror_SyncVarHookDelegate_deckScaleY, reader.ReadFloat());
			return;
		}
		long num = (long)reader.ReadVarULong();
		if ((num & 8L) != 0L)
		{
			base.GeneratedSyncVarDeserialize<BaccaratBetType>(ref this.currentBetType, null, Mirror.GeneratedNetworkCode._Read_BaccaratBetType(reader));
		}
		if ((num & 16L) != 0L)
		{
			base.GeneratedSyncVarDeserialize<float>(ref this.deckScaleY, this._Mirror_SyncVarHookDelegate_deckScaleY, reader.ReadFloat());
		}
	}

	// Token: 0x04000086 RID: 134
	[Header("UI References")]
	[Header("Card System")]
	[SerializeField]
	private GameObject cardPrefab;

	// Token: 0x04000087 RID: 135
	[SerializeField]
	private Transform playerCardArea;

	// Token: 0x04000088 RID: 136
	[SerializeField]
	private Transform dealerCardArea;

	// Token: 0x04000089 RID: 137
	[SerializeField]
	private Transform deckOfCardsTransform;

	// Token: 0x0400008A RID: 138
	[SerializeField]
	private int numberOfDecks = 1;

	// Token: 0x0400008B RID: 139
	[SerializeField]
	private float baseScaleForOneDeck = 1f;

	// Token: 0x0400008C RID: 140
	[SerializeField]
	private float cardSpacing = 0.5f;

	// Token: 0x0400008D RID: 141
	[SerializeField]
	private float cardMoveSpeed = 5f;

	// Token: 0x0400008E RID: 142
	[Header("Game State")]
	[SerializeField]
	private readonly SyncList<CardData> playerHand = new SyncList<CardData>();

	// Token: 0x0400008F RID: 143
	[SerializeField]
	private readonly SyncList<CardData> dealerHand = new SyncList<CardData>();

	// Token: 0x04000090 RID: 144
	[SerializeField]
	private Baccarat.BaccaratGameState gameState;

	// Token: 0x04000091 RID: 145
	[SyncVar]
	private BaccaratBetType currentBetType;

	// Token: 0x04000092 RID: 146
	[Header("SFX")]
	[SerializeField]
	private SFXComponent resetCardsSfx;

	// Token: 0x04000093 RID: 147
	[SerializeField]
	private SFXComponent dealerSfx;

	// Token: 0x04000094 RID: 148
	private List<CardData> deck = new List<CardData>();

	// Token: 0x04000095 RID: 149
	private List<GameObject> spawnedPlayerCards = new List<GameObject>();

	// Token: 0x04000096 RID: 150
	private List<GameObject> spawnedDealerCards = new List<GameObject>();

	// Token: 0x04000097 RID: 151
	private int initialDeckCount;

	// Token: 0x04000098 RID: 152
	private bool deckInitialized;

	// Token: 0x04000099 RID: 153
	[SyncVar(hook = "OnDeckScaleChanged")]
	private float deckScaleY = 1f;

	// Token: 0x0400009A RID: 154
	public Action<float, float> _Mirror_SyncVarHookDelegate_deckScaleY;

	// Token: 0x02000025 RID: 37
	private enum CardAreaType
	{
		// Token: 0x0400009C RID: 156
		Player,
		// Token: 0x0400009D RID: 157
		Dealer
	}

	// Token: 0x02000026 RID: 38
	private enum BaccaratGameState
	{
		// Token: 0x0400009F RID: 159
		Waiting,
		// Token: 0x040000A0 RID: 160
		Dealing,
		// Token: 0x040000A1 RID: 161
		Finished
	}
}
