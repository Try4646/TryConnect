using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Extensions;
using Mirror;
using Mirror.RemoteCalls;
using UnityEngine;
using UnityEngine.Events;

// Token: 0x0200002E RID: 46
public class Blackjack : GameBase
{
	// Token: 0x060000D8 RID: 216 RVA: 0x00006240 File Offset: 0x00004440
	protected override void StartGame()
	{
		base.StartGame();
		UnityEvent unityEvent = this.rpcOnStartEvent;
		if (unityEvent != null)
		{
			unityEvent.Invoke();
		}
		this.hasSplitThisRound = false;
		this.activeHandIndex = 0;
		this.handCompleted[0] = false;
		this.handCompleted[1] = false;
		this.handDoubled[0] = false;
		this.handDoubled[1] = false;
		this.handBets[0] = this.currentBet;
		this.handBets[1] = 0L;
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
	}

	// Token: 0x060000D9 RID: 217 RVA: 0x00006339 File Offset: 0x00004539
	private IEnumerator DealInitialCardsRoutine()
	{
		this.DealCardToPlayer(false);
		yield return new WaitForSeconds(0.5f);
		this.DealCardToDealer(false);
		yield return new WaitForSeconds(0.5f);
		this.DealCardToPlayer(false);
		yield return new WaitForSeconds(0.5f);
		this.DealCardToDealer(true);
		if (this.GetHandValue(this.playerHand) == 21)
		{
			this.EndGame(BlackjackResult.PlayerBlackjackWin);
			yield break;
		}
		this.gameState = Blackjack.BlackjackGameState.PlayerTurn;
		yield break;
	}

	// Token: 0x060000DA RID: 218 RVA: 0x00006348 File Offset: 0x00004548
	[Server]
	public void PlayerHit(PlayerInteract playerInteract)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void Blackjack::PlayerHit(PlayerInteract)' called when server was not active");
			return;
		}
		if (this.gameState != Blackjack.BlackjackGameState.PlayerTurn)
		{
			return;
		}
		this.DealCardToCurrentHand();
		int handValue = this.GetHandValue(this.GetCurrentHand());
		if (handValue <= 21)
		{
			if (handValue == 21)
			{
				this.CompleteCurrentHandAndAdvance();
			}
			return;
		}
		if (!this.hasSplitThisRound)
		{
			this.EndGame(BlackjackResult.PlayerLose);
			return;
		}
		this.CompleteCurrentHandAndAdvance();
	}

	// Token: 0x060000DB RID: 219 RVA: 0x000063AE File Offset: 0x000045AE
	[Server]
	public void PlayerStand(PlayerInteract playerInteract)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void Blackjack::PlayerStand(PlayerInteract)' called when server was not active");
			return;
		}
		if (this.gameState != Blackjack.BlackjackGameState.PlayerTurn)
		{
			return;
		}
		this.CompleteCurrentHandAndAdvance();
	}

	// Token: 0x060000DC RID: 220 RVA: 0x000063D8 File Offset: 0x000045D8
	[Server]
	public void PlayerDouble(PlayerInteract playerInteract)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void Blackjack::PlayerDouble(PlayerInteract)' called when server was not active");
			return;
		}
		if (this.gameState != Blackjack.BlackjackGameState.PlayerTurn)
		{
			return;
		}
		if (this.handDoubled[this.activeHandIndex])
		{
			return;
		}
		SyncList<CardData> currentHand = this.GetCurrentHand();
		if (currentHand.Count != 2)
		{
			return;
		}
		if (this.interactingPlayer == null)
		{
			return;
		}
		if (!NetworkSingleton<MoneyManager>.Instance.TryChangeBalance(-this.handBets[this.activeHandIndex], this.interactingPlayer, ChangeType.Bet))
		{
			return;
		}
		this.handBets[this.activeHandIndex] *= 2L;
		this.handDoubled[this.activeHandIndex] = true;
		base.NetworkcurrentBet = this.handBets[0] + this.handBets[1];
		this.DealCardToCurrentHand();
		if (this.GetHandValue(currentHand) > 21 && !this.hasSplitThisRound)
		{
			this.EndGame(BlackjackResult.PlayerLose);
			return;
		}
		this.CompleteCurrentHandAndAdvance();
	}

	// Token: 0x060000DD RID: 221 RVA: 0x000064BC File Offset: 0x000046BC
	[Server]
	public void PlayerSplit(PlayerInteract playerInteract)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void Blackjack::PlayerSplit(PlayerInteract)' called when server was not active");
			return;
		}
		if (this.gameState != Blackjack.BlackjackGameState.PlayerTurn)
		{
			return;
		}
		if (this.hasSplitThisRound)
		{
			return;
		}
		if (this.playerHand.Count != 2)
		{
			return;
		}
		if (this.playerHand[0].Rank != this.playerHand[1].Rank)
		{
			return;
		}
		if (this.interactingPlayer == null)
		{
			return;
		}
		if (!NetworkSingleton<MoneyManager>.Instance.TryChangeBalance(-this.handBets[0], this.interactingPlayer, ChangeType.Bet))
		{
			return;
		}
		this.hasSplitThisRound = true;
		this.handBets[1] = this.handBets[0];
		base.NetworkcurrentBet = this.handBets[0] + this.handBets[1];
		this.EnsureSplitCardArea();
		CardData item = this.playerHand[1];
		this.playerHand.RemoveAt(1);
		this.splitHand.Add(item);
		if (this.spawnedPlayerCards.Count > 1)
		{
			GameObject gameObject = this.spawnedPlayerCards[1];
			this.spawnedPlayerCards.RemoveAt(1);
			this.spawnedSplitCards.Add(gameObject);
			Transform cardAreaTransform = this.GetCardAreaTransform(Blackjack.CardAreaType.PlayerSplit);
			if (cardAreaTransform != null && gameObject != null)
			{
				gameObject.transform.SetParent(cardAreaTransform);
				gameObject.transform.localPosition = this.CalculateCardPosition(Blackjack.CardAreaType.PlayerSplit, this.spawnedSplitCards.Count - 1, this.spawnedSplitCards.Count);
				gameObject.transform.localRotation = Quaternion.identity;
				this.RpcSetCardParentAndPosition(gameObject, Blackjack.CardAreaType.PlayerSplit, gameObject.transform.localPosition);
			}
		}
		this.DealCardToHand(this.playerHand, this.spawnedPlayerCards, Blackjack.CardAreaType.Player, false);
		this.DealCardToHand(this.splitHand, this.spawnedSplitCards, Blackjack.CardAreaType.PlayerSplit, false);
		this.RepositionAllCards(Blackjack.CardAreaType.Player, this.spawnedPlayerCards.Count);
		this.RpcRepositionAllCards(Blackjack.CardAreaType.Player, this.spawnedPlayerCards.Count);
		this.RepositionAllCards(Blackjack.CardAreaType.PlayerSplit, this.spawnedSplitCards.Count);
		this.RpcRepositionAllCards(Blackjack.CardAreaType.PlayerSplit, this.spawnedSplitCards.Count);
		this.activeHandIndex = 0;
		this.handCompleted[0] = false;
		this.handCompleted[1] = false;
		this.UpdateCasinoHelperCounts();
	}

	// Token: 0x060000DE RID: 222 RVA: 0x000066E4 File Offset: 0x000048E4
	[Server]
	private void InitializeDeck()
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void Blackjack::InitializeDeck()' called when server was not active");
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

	// Token: 0x060000DF RID: 223 RVA: 0x000067E4 File Offset: 0x000049E4
	[Server]
	private void ShuffleDeck()
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void Blackjack::ShuffleDeck()' called when server was not active");
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

	// Token: 0x060000E0 RID: 224 RVA: 0x00006874 File Offset: 0x00004A74
	[Server]
	private CardData DrawCardFromDeck()
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'CardData Blackjack::DrawCardFromDeck()' called when server was not active");
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

	// Token: 0x060000E1 RID: 225 RVA: 0x00006968 File Offset: 0x00004B68
	private void OnDeckScaleChanged(float oldValue, float newValue)
	{
		if (this.deckOfCardsTransform != null)
		{
			this.deckOfCardsTransform.localScale = new Vector3(this.deckOfCardsTransform.localScale.x, newValue, this.deckOfCardsTransform.localScale.z);
		}
	}

	// Token: 0x060000E2 RID: 226 RVA: 0x000069B4 File Offset: 0x00004BB4
	[Server]
	private void DealCardToPlayer(bool isFaceDown = false)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void Blackjack::DealCardToPlayer(System.Boolean)' called when server was not active");
			return;
		}
		this.DealCardToHand(this.playerHand, this.spawnedPlayerCards, Blackjack.CardAreaType.Player, isFaceDown);
	}

	// Token: 0x060000E3 RID: 227 RVA: 0x000069E0 File Offset: 0x00004BE0
	[Server]
	private void DealCardToDealer(bool isFaceDown = false)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void Blackjack::DealCardToDealer(System.Boolean)' called when server was not active");
			return;
		}
		CardData cardData = this.DrawCardFromDeck();
		this.dealerHand.Add(cardData);
		this.SpawnCard(cardData, Blackjack.CardAreaType.Dealer, this.spawnedDealerCards, isFaceDown);
		if (isFaceDown)
		{
			this.hiddenDealerCardIndex = this.dealerHand.Count - 1;
		}
		this.UpdateCasinoHelperCounts();
	}

	// Token: 0x060000E4 RID: 228 RVA: 0x00006A40 File Offset: 0x00004C40
	[Server]
	private void DealCardToHand(SyncList<CardData> hand, List<GameObject> cardList, Blackjack.CardAreaType areaType, bool isFaceDown = false)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void Blackjack::DealCardToHand(Mirror.SyncList`1<CardData>,System.Collections.Generic.List`1<UnityEngine.GameObject>,Blackjack/CardAreaType,System.Boolean)' called when server was not active");
			return;
		}
		CardData cardData = this.DrawCardFromDeck();
		hand.Add(cardData);
		this.SpawnCard(cardData, areaType, cardList, isFaceDown);
		this.UpdateCasinoHelperCounts();
	}

	// Token: 0x060000E5 RID: 229 RVA: 0x00006A84 File Offset: 0x00004C84
	[Server]
	private void SpawnCard(CardData cardData, Blackjack.CardAreaType areaType, List<GameObject> cardList, bool isHidden)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void Blackjack::SpawnCard(CardData,Blackjack/CardAreaType,System.Collections.Generic.List`1<UnityEngine.GameObject>,System.Boolean)' called when server was not active");
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

	// Token: 0x060000E6 RID: 230 RVA: 0x00006BB8 File Offset: 0x00004DB8
	[ClientRpc]
	private void RpcSetCardParentAndPosition(GameObject cardObject, Blackjack.CardAreaType areaType, Vector3 cardLocalPosition)
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		writer.WriteGameObject(cardObject);
		Mirror.GeneratedNetworkCode._Write_Blackjack/CardAreaType(writer, areaType);
		writer.WriteVector3(cardLocalPosition);
		this.SendRPCInternal("System.Void Blackjack::RpcSetCardParentAndPosition(UnityEngine.GameObject,Blackjack/CardAreaType,UnityEngine.Vector3)", 691033185, writer, 0, true);
		NetworkWriterPool.Return(writer);
	}

	// Token: 0x060000E7 RID: 231 RVA: 0x00006C08 File Offset: 0x00004E08
	[Server]
	private Vector3 CalculateCardPosition(Blackjack.CardAreaType areaType, int cardIndex, int totalCardCount)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'UnityEngine.Vector3 Blackjack::CalculateCardPosition(Blackjack/CardAreaType,System.Int32,System.Int32)' called when server was not active");
			return default(Vector3);
		}
		if (totalCardCount <= 1)
		{
			return Vector3.zero;
		}
		float d = -((float)(totalCardCount - 1) * this.cardSpacing) / 2f + (float)cardIndex * this.cardSpacing;
		return Vector3.right * d;
	}

	// Token: 0x060000E8 RID: 232 RVA: 0x00006C68 File Offset: 0x00004E68
	[Server]
	private void RepositionAllCards(Blackjack.CardAreaType areaType, int totalCardCount)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void Blackjack::RepositionAllCards(Blackjack/CardAreaType,System.Int32)' called when server was not active");
			return;
		}
		List<GameObject> list = (areaType == Blackjack.CardAreaType.Player) ? this.spawnedPlayerCards : this.spawnedDealerCards;
		if (areaType == Blackjack.CardAreaType.PlayerSplit)
		{
			list = this.spawnedSplitCards;
		}
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

	// Token: 0x060000E9 RID: 233 RVA: 0x00006D08 File Offset: 0x00004F08
	[ClientRpc]
	private void RpcRepositionAllCards(Blackjack.CardAreaType areaType, int totalCardCount)
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		Mirror.GeneratedNetworkCode._Write_Blackjack/CardAreaType(writer, areaType);
		writer.WriteVarInt(totalCardCount);
		this.SendRPCInternal("System.Void Blackjack::RpcRepositionAllCards(Blackjack/CardAreaType,System.Int32)", 1910415189, writer, 0, true);
		NetworkWriterPool.Return(writer);
	}

	// Token: 0x060000EA RID: 234 RVA: 0x00006D4C File Offset: 0x00004F4C
	private IEnumerator RepositionCardsSmoothRoutine(List<Transform> cardTransforms, Transform cardArea, Blackjack.CardAreaType areaType, int totalCardCount)
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

	// Token: 0x060000EB RID: 235 RVA: 0x00006D78 File Offset: 0x00004F78
	private Vector3 CalculateCardPositionLocal(Transform cardArea, Blackjack.CardAreaType areaType, int cardIndex, int totalCardCount)
	{
		if (totalCardCount <= 1)
		{
			return Vector3.zero;
		}
		float d = -((float)(totalCardCount - 1) * this.cardSpacing) / 2f + (float)cardIndex * this.cardSpacing;
		return Vector3.right * d;
	}

	// Token: 0x060000EC RID: 236 RVA: 0x00006DB9 File Offset: 0x00004FB9
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

	// Token: 0x060000ED RID: 237 RVA: 0x00006DD8 File Offset: 0x00004FD8
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

	// Token: 0x060000EE RID: 238 RVA: 0x00006E2B File Offset: 0x0000502B
	private IEnumerator DealerPlay()
	{
		yield return new WaitForSeconds(1f);
		if (this.spawnedDealerCards.Count > 1 && this.spawnedDealerCards[1] != null)
		{
			Card component = this.spawnedDealerCards[1].GetComponent<Card>();
			if (component != null)
			{
				component.RpcSetFaceDown(false);
			}
		}
		this.hiddenDealerCardIndex = -1;
		this.UpdateCasinoHelperCounts();
		yield return new WaitForSeconds(1f);
		while (this.GetHandValue(this.dealerHand) < 17)
		{
			this.DealCardToDealer(false);
			yield return new WaitForSeconds(1f);
		}
		int handValue = this.GetHandValue(this.dealerHand);
		int handValue2 = this.GetHandValue(this.playerHand);
		if (this.hasSplitThisRound)
		{
			long num = 0L;
			num += this.CalculateHandPayout(this.playerHand, handValue, this.handBets[0], false);
			num += this.CalculateHandPayout(this.splitHand, handValue, this.handBets[1], false);
			this.EndGameWithPayout(num);
			yield break;
		}
		if (handValue2 > 21)
		{
		}
		BlackjackResult result;
		if (handValue > 21)
		{
			result = BlackjackResult.PlayerWin;
		}
		else if (handValue > handValue2)
		{
			result = BlackjackResult.PlayerLose;
		}
		else if (handValue2 > handValue)
		{
			result = BlackjackResult.PlayerWin;
		}
		else
		{
			result = BlackjackResult.Push;
		}
		this.EndGame(result);
		yield break;
	}

	// Token: 0x060000EF RID: 239 RVA: 0x00006E3C File Offset: 0x0000503C
	[Server]
	private void EndGame(BlackjackResult result)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void Blackjack::EndGame(BlackjackResult)' called when server was not active");
			return;
		}
		this.gameState = Blackjack.BlackjackGameState.Finished;
		int handValue = this.GetHandValue(this.playerHand);
		int handValue2 = this.GetHandValue(this.dealerHand);
		Dictionary<string, object> gameSpecificData = new Dictionary<string, object>
		{
			{
				"playerHandValue",
				handValue
			},
			{
				"dealerHandValue",
				handValue2
			}
		};
		switch (result)
		{
		case BlackjackResult.PlayerWin:
			this.Payout(2.0 * base.EstimatedValue, ChangeType.GameResult, gameSpecificData, -1L);
			break;
		case BlackjackResult.PlayerLose:
			this.Payout(0.0, ChangeType.GameResult, gameSpecificData, -1L);
			break;
		case BlackjackResult.PlayerBlackjackWin:
			this.Payout(3.0 * base.EstimatedValue, ChangeType.GameResult, gameSpecificData, -1L);
			break;
		case BlackjackResult.Push:
			this.Payout(1.0, ChangeType.Misc, gameSpecificData, -1L);
			break;
		}
		base.StartCoroutine(this.ResetGameRoutine());
	}

	// Token: 0x060000F0 RID: 240 RVA: 0x00006F2C File Offset: 0x0000512C
	private IEnumerator ResetGameRoutine()
	{
		yield return new WaitForSeconds(1f);
		this.ResetGame();
		yield break;
	}

	// Token: 0x060000F1 RID: 241 RVA: 0x00006F3C File Offset: 0x0000513C
	[Server]
	protected override void ResetGame()
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void Blackjack::ResetGame()' called when server was not active");
			return;
		}
		base.ResetGame();
		this.playerHand.Clear();
		this.splitHand.Clear();
		this.dealerHand.Clear();
		this.gameState = Blackjack.BlackjackGameState.Waiting;
		this.hiddenDealerCardIndex = -1;
		this.hasSplitThisRound = false;
		this.activeHandIndex = 0;
		this.handCompleted[0] = false;
		this.handCompleted[1] = false;
		this.handDoubled[0] = false;
		this.handDoubled[1] = false;
		this.handBets[0] = 0L;
		this.handBets[1] = 0L;
		this.CleanupCards(this.spawnedPlayerCards);
		this.CleanupCards(this.spawnedSplitCards);
		this.CleanupCards(this.spawnedDealerCards);
		this.spawnedPlayerCards.Clear();
		this.spawnedSplitCards.Clear();
		this.spawnedDealerCards.Clear();
		this.RpcClearCasinoHelperTexts();
		this.resetCardsSfx.RpcPlayOneShotWithCustom3DPos(this.dealerCardArea.position);
	}

	// Token: 0x060000F2 RID: 242 RVA: 0x0000703C File Offset: 0x0000523C
	[Server]
	private void CleanupCards(List<GameObject> cardList)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void Blackjack::CleanupCards(System.Collections.Generic.List`1<UnityEngine.GameObject>)' called when server was not active");
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

	// Token: 0x060000F3 RID: 243 RVA: 0x000070A8 File Offset: 0x000052A8
	private int GetHandValue(SyncList<CardData> hand)
	{
		int num = 0;
		int num2 = 0;
		using (SyncList<CardData>.Enumerator enumerator = hand.GetEnumerator())
		{
			while (enumerator.MoveNext())
			{
				CardData cardData = enumerator.Current;
				if (cardData.Rank == Rank.Ace)
				{
					num2++;
					num += 11;
				}
				else
				{
					num += cardData.GetBlackjackValue();
				}
			}
			goto IL_55;
		}
		IL_4C:
		num -= 10;
		num2--;
		IL_55:
		if (num <= 21 || num2 <= 0)
		{
			return num;
		}
		goto IL_4C;
	}

	// Token: 0x060000F4 RID: 244 RVA: 0x00007124 File Offset: 0x00005324
	[Server]
	private void UpdateCasinoHelperCounts()
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void Blackjack::UpdateCasinoHelperCounts()' called when server was not active");
			return;
		}
		int handValue = this.GetHandValue(this.GetCurrentHand());
		int dealerTotal = (this.hiddenDealerCardIndex >= 0) ? this.GetHandValueExcludingIndex(this.dealerHand, this.hiddenDealerCardIndex) : this.GetHandValue(this.dealerHand);
		this.RpcUpdateCasinoHelperCounts(handValue, dealerTotal);
	}

	// Token: 0x060000F5 RID: 245 RVA: 0x00007188 File Offset: 0x00005388
	[ClientRpc]
	private void RpcUpdateCasinoHelperCounts(int playerTotal, int dealerTotal)
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		writer.WriteVarInt(playerTotal);
		writer.WriteVarInt(dealerTotal);
		this.SendRPCInternal("System.Void Blackjack::RpcUpdateCasinoHelperCounts(System.Int32,System.Int32)", 1511639462, writer, 0, true);
		NetworkWriterPool.Return(writer);
	}

	// Token: 0x060000F6 RID: 246 RVA: 0x000071CC File Offset: 0x000053CC
	[ClientRpc]
	private void RpcClearCasinoHelperTexts()
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		this.SendRPCInternal("System.Void Blackjack::RpcClearCasinoHelperTexts()", -1603687594, writer, 0, true);
		NetworkWriterPool.Return(writer);
	}

	// Token: 0x060000F7 RID: 247 RVA: 0x000071FC File Offset: 0x000053FC
	private int GetHandValueExcludingIndex(SyncList<CardData> hand, int excludedIndex)
	{
		int num = 0;
		int num2 = 0;
		for (int i = 0; i < hand.Count; i++)
		{
			if (i != excludedIndex)
			{
				CardData cardData = hand[i];
				if (cardData.Rank == Rank.Ace)
				{
					num2++;
					num += 11;
				}
				else
				{
					num += cardData.GetBlackjackValue();
				}
			}
		}
		while (num > 21 && num2 > 0)
		{
			num -= 10;
			num2--;
		}
		return num;
	}

	// Token: 0x060000F8 RID: 248 RVA: 0x0000725D File Offset: 0x0000545D
	private SyncList<CardData> GetCurrentHand()
	{
		if (this.activeHandIndex != 0)
		{
			return this.splitHand;
		}
		return this.playerHand;
	}

	// Token: 0x060000F9 RID: 249 RVA: 0x00007274 File Offset: 0x00005474
	private List<GameObject> GetCurrentHandCards()
	{
		if (this.activeHandIndex != 0)
		{
			return this.spawnedSplitCards;
		}
		return this.spawnedPlayerCards;
	}

	// Token: 0x060000FA RID: 250 RVA: 0x0000728B File Offset: 0x0000548B
	private Blackjack.CardAreaType GetCurrentHandAreaType()
	{
		if (this.activeHandIndex != 0)
		{
			return Blackjack.CardAreaType.PlayerSplit;
		}
		return Blackjack.CardAreaType.Player;
	}

	// Token: 0x060000FB RID: 251 RVA: 0x00007298 File Offset: 0x00005498
	[Server]
	private void DealCardToCurrentHand()
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void Blackjack::DealCardToCurrentHand()' called when server was not active");
			return;
		}
		this.DealCardToHand(this.GetCurrentHand(), this.GetCurrentHandCards(), this.GetCurrentHandAreaType(), false);
	}

	// Token: 0x060000FC RID: 252 RVA: 0x000072C8 File Offset: 0x000054C8
	[Server]
	private void CompleteCurrentHandAndAdvance()
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void Blackjack::CompleteCurrentHandAndAdvance()' called when server was not active");
			return;
		}
		this.handCompleted[this.activeHandIndex] = true;
		if (this.hasSplitThisRound && this.activeHandIndex == 0)
		{
			this.activeHandIndex = 1;
			this.UpdateCasinoHelperCounts();
			return;
		}
		this.gameState = Blackjack.BlackjackGameState.DealerTurn;
		base.StartCoroutine(this.DealerPlay());
	}

	// Token: 0x060000FD RID: 253 RVA: 0x0000732C File Offset: 0x0000552C
	private long CalculateHandPayout(SyncList<CardData> hand, int dealerValue, long handBet, bool allowBlackjackPayout)
	{
		if (handBet <= 0L)
		{
			return 0L;
		}
		int handValue = this.GetHandValue(hand);
		if (handValue > 21)
		{
			return 0L;
		}
		bool flag = dealerValue > 21;
		if (allowBlackjackPayout && hand.Count == 2 && handValue == 21)
		{
			return (long)Math.Round((double)(handBet * 3L) * base.EstimatedValue);
		}
		if (flag || handValue > dealerValue)
		{
			return (long)Math.Round((double)(handBet * 2L) * base.EstimatedValue);
		}
		if (handValue < dealerValue)
		{
			return 0L;
		}
		return handBet;
	}

	// Token: 0x060000FE RID: 254 RVA: 0x000073A0 File Offset: 0x000055A0
	[Server]
	private void EndGameWithPayout(long payout)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void Blackjack::EndGameWithPayout(System.Int64)' called when server was not active");
			return;
		}
		this.gameState = Blackjack.BlackjackGameState.Finished;
		int handValue = this.GetHandValue(this.playerHand);
		int handValue2 = this.GetHandValue(this.splitHand);
		int handValue3 = this.GetHandValue(this.dealerHand);
		Dictionary<string, object> gameSpecificData = new Dictionary<string, object>
		{
			{
				"playerHandValue",
				handValue
			},
			{
				"splitHandValue",
				handValue2
			},
			{
				"dealerHandValue",
				handValue3
			}
		};
		this.Payout((double)payout, ChangeType.GameResult, gameSpecificData, -1L);
		base.StartCoroutine(this.ResetGameRoutine());
	}

	// Token: 0x060000FF RID: 255 RVA: 0x00007444 File Offset: 0x00005644
	private Transform GetCardAreaTransform(Blackjack.CardAreaType areaType)
	{
		switch (areaType)
		{
		case Blackjack.CardAreaType.Player:
			return this.playerCardArea;
		case Blackjack.CardAreaType.PlayerSplit:
			if (!(this.playerSplitCardArea != null))
			{
				return this.playerCardArea;
			}
			return this.playerSplitCardArea;
		case Blackjack.CardAreaType.Dealer:
			return this.dealerCardArea;
		default:
			return null;
		}
	}

	// Token: 0x06000100 RID: 256 RVA: 0x00007490 File Offset: 0x00005690
	[Server]
	private void EnsureSplitCardArea()
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void Blackjack::EnsureSplitCardArea()' called when server was not active");
			return;
		}
		if (this.playerSplitCardArea != null)
		{
			return;
		}
		if (this.playerCardArea == null)
		{
			return;
		}
		Transform parent = this.playerCardArea.parent;
		GameObject gameObject = new GameObject(this.splitHandAreaName);
		gameObject.transform.SetParent(parent);
		gameObject.transform.localPosition = this.playerCardArea.localPosition + this.splitHandAreaOffset;
		gameObject.transform.localRotation = this.playerCardArea.localRotation;
		gameObject.transform.localScale = this.playerCardArea.localScale;
		this.playerSplitCardArea = gameObject.transform;
		this.RpcEnsureSplitCardArea(this.splitHandAreaName, this.playerSplitCardArea.localPosition, this.playerSplitCardArea.localRotation, this.playerSplitCardArea.localScale);
	}

	// Token: 0x06000101 RID: 257 RVA: 0x0000757C File Offset: 0x0000577C
	[ClientRpc]
	private void RpcEnsureSplitCardArea(string areaName, Vector3 localPosition, Quaternion localRotation, Vector3 localScale)
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		writer.WriteString(areaName);
		writer.WriteVector3(localPosition);
		writer.WriteQuaternion(localRotation);
		writer.WriteVector3(localScale);
		this.SendRPCInternal("System.Void Blackjack::RpcEnsureSplitCardArea(System.String,UnityEngine.Vector3,UnityEngine.Quaternion,UnityEngine.Vector3)", 1104917024, writer, 0, true);
		NetworkWriterPool.Return(writer);
	}

	// Token: 0x06000102 RID: 258 RVA: 0x000075D4 File Offset: 0x000057D4
	public Blackjack()
	{
		base.InitSyncObject(this.playerHand);
		base.InitSyncObject(this.splitHand);
		base.InitSyncObject(this.dealerHand);
		this._Mirror_SyncVarHookDelegate_deckScaleY = new Action<float, float>(this.OnDeckScaleChanged);
	}

	// Token: 0x06000103 RID: 259 RVA: 0x00002321 File Offset: 0x00000521
	public override bool Weaved()
	{
		return true;
	}

	// Token: 0x17000019 RID: 25
	// (get) Token: 0x06000104 RID: 260 RVA: 0x000076F0 File Offset: 0x000058F0
	// (set) Token: 0x06000105 RID: 261 RVA: 0x00007703 File Offset: 0x00005903
	public float NetworkdeckScaleY
	{
		get
		{
			return this.deckScaleY;
		}
		[param: In]
		set
		{
			base.GeneratedSyncVarSetter<float>(value, ref this.deckScaleY, 8UL, this._Mirror_SyncVarHookDelegate_deckScaleY);
		}
	}

	// Token: 0x06000106 RID: 262 RVA: 0x00007724 File Offset: 0x00005924
	protected void UserCode_RpcSetCardParentAndPosition__GameObject__CardAreaType__Vector3(GameObject cardObject, Blackjack.CardAreaType areaType, Vector3 cardLocalPosition)
	{
		Transform cardAreaTransform = this.GetCardAreaTransform(areaType);
		if (cardAreaTransform != null && cardObject != null)
		{
			cardObject.transform.SetParent(cardAreaTransform);
			cardObject.transform.localPosition = cardLocalPosition;
			cardObject.transform.localRotation = Quaternion.identity;
		}
	}

	// Token: 0x06000107 RID: 263 RVA: 0x00007773 File Offset: 0x00005973
	protected static void InvokeUserCode_RpcSetCardParentAndPosition__GameObject__CardAreaType__Vector3(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcSetCardParentAndPosition called on server.");
			return;
		}
		((Blackjack)obj).UserCode_RpcSetCardParentAndPosition__GameObject__CardAreaType__Vector3(reader.ReadGameObject(), Mirror.GeneratedNetworkCode._Read_Blackjack/CardAreaType(reader), reader.ReadVector3());
	}

	// Token: 0x06000108 RID: 264 RVA: 0x000077A8 File Offset: 0x000059A8
	protected void UserCode_RpcRepositionAllCards__CardAreaType__Int32(Blackjack.CardAreaType areaType, int totalCardCount)
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

	// Token: 0x06000109 RID: 265 RVA: 0x00007831 File Offset: 0x00005A31
	protected static void InvokeUserCode_RpcRepositionAllCards__CardAreaType__Int32(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcRepositionAllCards called on server.");
			return;
		}
		((Blackjack)obj).UserCode_RpcRepositionAllCards__CardAreaType__Int32(Mirror.GeneratedNetworkCode._Read_Blackjack/CardAreaType(reader), reader.ReadVarInt());
	}

	// Token: 0x0600010A RID: 266 RVA: 0x00005CB4 File Offset: 0x00003EB4
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

	// Token: 0x0600010B RID: 267 RVA: 0x00007860 File Offset: 0x00005A60
	protected static void InvokeUserCode_RpcUpdateCasinoHelperCounts__Int32__Int32(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcUpdateCasinoHelperCounts called on server.");
			return;
		}
		((Blackjack)obj).UserCode_RpcUpdateCasinoHelperCounts__Int32__Int32(reader.ReadVarInt(), reader.ReadVarInt());
	}

	// Token: 0x0600010C RID: 268 RVA: 0x00005D10 File Offset: 0x00003F10
	protected void UserCode_RpcClearCasinoHelperTexts()
	{
		base.ClearCasinoHelperTexts();
	}

	// Token: 0x0600010D RID: 269 RVA: 0x0000788F File Offset: 0x00005A8F
	protected static void InvokeUserCode_RpcClearCasinoHelperTexts(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcClearCasinoHelperTexts called on server.");
			return;
		}
		((Blackjack)obj).UserCode_RpcClearCasinoHelperTexts();
	}

	// Token: 0x0600010E RID: 270 RVA: 0x000078B4 File Offset: 0x00005AB4
	protected void UserCode_RpcEnsureSplitCardArea__String__Vector3__Quaternion__Vector3(string areaName, Vector3 localPosition, Quaternion localRotation, Vector3 localScale)
	{
		if (this.playerSplitCardArea != null)
		{
			return;
		}
		if (this.playerCardArea == null)
		{
			return;
		}
		Transform parent = this.playerCardArea.parent;
		Transform x = (parent != null) ? parent.Find(areaName) : null;
		if (x != null)
		{
			this.playerSplitCardArea = x;
			return;
		}
		GameObject gameObject = new GameObject(areaName);
		gameObject.transform.SetParent(parent);
		gameObject.transform.localPosition = localPosition;
		gameObject.transform.localRotation = localRotation;
		gameObject.transform.localScale = localScale;
		this.playerSplitCardArea = gameObject.transform;
	}

	// Token: 0x0600010F RID: 271 RVA: 0x00007954 File Offset: 0x00005B54
	protected static void InvokeUserCode_RpcEnsureSplitCardArea__String__Vector3__Quaternion__Vector3(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcEnsureSplitCardArea called on server.");
			return;
		}
		((Blackjack)obj).UserCode_RpcEnsureSplitCardArea__String__Vector3__Quaternion__Vector3(reader.ReadString(), reader.ReadVector3(), reader.ReadQuaternion(), reader.ReadVector3());
	}

	// Token: 0x06000110 RID: 272 RVA: 0x00007990 File Offset: 0x00005B90
	static Blackjack()
	{
		RemoteProcedureCalls.RegisterRpc(typeof(Blackjack), "System.Void Blackjack::RpcSetCardParentAndPosition(UnityEngine.GameObject,Blackjack/CardAreaType,UnityEngine.Vector3)", new RemoteCallDelegate(Blackjack.InvokeUserCode_RpcSetCardParentAndPosition__GameObject__CardAreaType__Vector3));
		RemoteProcedureCalls.RegisterRpc(typeof(Blackjack), "System.Void Blackjack::RpcRepositionAllCards(Blackjack/CardAreaType,System.Int32)", new RemoteCallDelegate(Blackjack.InvokeUserCode_RpcRepositionAllCards__CardAreaType__Int32));
		RemoteProcedureCalls.RegisterRpc(typeof(Blackjack), "System.Void Blackjack::RpcUpdateCasinoHelperCounts(System.Int32,System.Int32)", new RemoteCallDelegate(Blackjack.InvokeUserCode_RpcUpdateCasinoHelperCounts__Int32__Int32));
		RemoteProcedureCalls.RegisterRpc(typeof(Blackjack), "System.Void Blackjack::RpcClearCasinoHelperTexts()", new RemoteCallDelegate(Blackjack.InvokeUserCode_RpcClearCasinoHelperTexts));
		RemoteProcedureCalls.RegisterRpc(typeof(Blackjack), "System.Void Blackjack::RpcEnsureSplitCardArea(System.String,UnityEngine.Vector3,UnityEngine.Quaternion,UnityEngine.Vector3)", new RemoteCallDelegate(Blackjack.InvokeUserCode_RpcEnsureSplitCardArea__String__Vector3__Quaternion__Vector3));
	}

	// Token: 0x06000111 RID: 273 RVA: 0x00007A40 File Offset: 0x00005C40
	public override void SerializeSyncVars(NetworkWriter writer, bool forceAll)
	{
		base.SerializeSyncVars(writer, forceAll);
		if (forceAll)
		{
			writer.WriteFloat(this.deckScaleY);
			return;
		}
		writer.WriteVarULong(this.syncVarDirtyBits);
		if ((this.syncVarDirtyBits & 8UL) != 0UL)
		{
			writer.WriteFloat(this.deckScaleY);
		}
	}

	// Token: 0x06000112 RID: 274 RVA: 0x00007A98 File Offset: 0x00005C98
	public override void DeserializeSyncVars(NetworkReader reader, bool initialState)
	{
		base.DeserializeSyncVars(reader, initialState);
		if (initialState)
		{
			base.GeneratedSyncVarDeserialize<float>(ref this.deckScaleY, this._Mirror_SyncVarHookDelegate_deckScaleY, reader.ReadFloat());
			return;
		}
		long num = (long)reader.ReadVarULong();
		if ((num & 8L) != 0L)
		{
			base.GeneratedSyncVarDeserialize<float>(ref this.deckScaleY, this._Mirror_SyncVarHookDelegate_deckScaleY, reader.ReadFloat());
		}
	}

	// Token: 0x040000BE RID: 190
	[Header("UI References")]
	[Header("Card System")]
	[SerializeField]
	private GameObject cardPrefab;

	// Token: 0x040000BF RID: 191
	[SerializeField]
	private Transform playerCardArea;

	// Token: 0x040000C0 RID: 192
	[SerializeField]
	private Transform playerSplitCardArea;

	// Token: 0x040000C1 RID: 193
	[SerializeField]
	private Transform dealerCardArea;

	// Token: 0x040000C2 RID: 194
	[SerializeField]
	private Transform deckOfCardsTransform;

	// Token: 0x040000C3 RID: 195
	[SerializeField]
	private int numberOfDecks = 1;

	// Token: 0x040000C4 RID: 196
	[SerializeField]
	private float baseScaleForOneDeck = 1f;

	// Token: 0x040000C5 RID: 197
	[SerializeField]
	private float cardSpacing = 0.5f;

	// Token: 0x040000C6 RID: 198
	[SerializeField]
	private float cardMoveSpeed = 5f;

	// Token: 0x040000C7 RID: 199
	[SerializeField]
	private Vector3 splitHandAreaOffset = new Vector3(2f, 0f, 0f);

	// Token: 0x040000C8 RID: 200
	[SerializeField]
	private string splitHandAreaName = "PlayerSplitHand";

	// Token: 0x040000C9 RID: 201
	[Header("Game State")]
	[SerializeField]
	private readonly SyncList<CardData> playerHand = new SyncList<CardData>();

	// Token: 0x040000CA RID: 202
	[SerializeField]
	private readonly SyncList<CardData> splitHand = new SyncList<CardData>();

	// Token: 0x040000CB RID: 203
	[SerializeField]
	private readonly SyncList<CardData> dealerHand = new SyncList<CardData>();

	// Token: 0x040000CC RID: 204
	[SerializeField]
	private Blackjack.BlackjackGameState gameState;

	// Token: 0x040000CD RID: 205
	[Header("SFX")]
	[SerializeField]
	private SFXComponent resetCardsSfx;

	// Token: 0x040000CE RID: 206
	private List<CardData> deck = new List<CardData>();

	// Token: 0x040000CF RID: 207
	private List<GameObject> spawnedPlayerCards = new List<GameObject>();

	// Token: 0x040000D0 RID: 208
	private List<GameObject> spawnedSplitCards = new List<GameObject>();

	// Token: 0x040000D1 RID: 209
	private List<GameObject> spawnedDealerCards = new List<GameObject>();

	// Token: 0x040000D2 RID: 210
	private int initialDeckCount;

	// Token: 0x040000D3 RID: 211
	private bool deckInitialized;

	// Token: 0x040000D4 RID: 212
	[SyncVar(hook = "OnDeckScaleChanged")]
	private float deckScaleY = 1f;

	// Token: 0x040000D5 RID: 213
	private int hiddenDealerCardIndex = -1;

	// Token: 0x040000D6 RID: 214
	private bool hasSplitThisRound;

	// Token: 0x040000D7 RID: 215
	private int activeHandIndex;

	// Token: 0x040000D8 RID: 216
	private readonly bool[] handCompleted = new bool[2];

	// Token: 0x040000D9 RID: 217
	private readonly bool[] handDoubled = new bool[2];

	// Token: 0x040000DA RID: 218
	private readonly long[] handBets = new long[2];

	// Token: 0x040000DB RID: 219
	[SerializeField]
	private UnityEvent rpcOnStartEvent;

	// Token: 0x040000DC RID: 220
	public Action<float, float> _Mirror_SyncVarHookDelegate_deckScaleY;

	// Token: 0x0200002F RID: 47
	private enum CardAreaType
	{
		// Token: 0x040000DE RID: 222
		Player,
		// Token: 0x040000DF RID: 223
		PlayerSplit,
		// Token: 0x040000E0 RID: 224
		Dealer
	}

	// Token: 0x02000030 RID: 48
	private enum BlackjackGameState
	{
		// Token: 0x040000E2 RID: 226
		Waiting,
		// Token: 0x040000E3 RID: 227
		PlayerTurn,
		// Token: 0x040000E4 RID: 228
		DealerTurn,
		// Token: 0x040000E5 RID: 229
		Finished
	}
}
