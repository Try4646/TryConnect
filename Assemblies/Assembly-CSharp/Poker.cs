using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using Extensions;
using Mirror;
using Mirror.RemoteCalls;
using UnityEngine;

// Token: 0x0200006E RID: 110
public class Poker : GameBase
{
	// Token: 0x060003D1 RID: 977 RVA: 0x00011AD8 File Offset: 0x0000FCD8
	[Server]
	public new void TryStartGame(PlayerInteract playerInteract)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void Poker::TryStartGame(PlayerInteract)' called when server was not active");
			return;
		}
		if (this.gameState == Poker.PokerGameState.PlayerTurn)
		{
			this.ConfirmSelection(playerInteract);
			return;
		}
		if (!this.CanGameStart())
		{
			return;
		}
		if (this.isPlaying)
		{
			return;
		}
		if (this.currentBet <= 0L)
		{
			return;
		}
		PlayerProfile interactingPlayer;
		if (playerInteract.TryGetComponent<PlayerProfile>(out interactingPlayer))
		{
			this.interactingPlayer = interactingPlayer;
		}
		if (this.interactingPlayer == null)
		{
			return;
		}
		if (!this.isGoldenChipApplied)
		{
			if (this.currentBet < base.MinBet || this.currentBet > base.MaxBet)
			{
				return;
			}
			if (!NetworkSingleton<MoneyManager>.Instance.TryChangeBalance(-this.currentBet, this.interactingPlayer, ChangeType.Bet))
			{
				return;
			}
		}
		else
		{
			this.isGoldenBet = true;
		}
		this.isPlaying = true;
		this.canBet = false;
		PlayerEnergy playerEnergy;
		this.interactingPlayer.TryGetComponent<PlayerEnergy>(out playerEnergy);
		if (playerEnergy != null)
		{
			playerEnergy.DecreaseEnergy(6.6f);
		}
		this.StartGame();
		SFXComponent sfxcomponent = this.dealerSfx;
		if (sfxcomponent == null)
		{
			return;
		}
		sfxcomponent.RpcPlayOneShotWith3DPos();
	}

	// Token: 0x060003D2 RID: 978 RVA: 0x00011BD8 File Offset: 0x0000FDD8
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
		this.playerHand.Clear();
		this.cardsToKeep.Clear();
		this.gameState = Poker.PokerGameState.Dealing;
		base.StartCoroutine(this.DealInitialCardsRoutine());
	}

	// Token: 0x060003D3 RID: 979 RVA: 0x00011C93 File Offset: 0x0000FE93
	private IEnumerator DealInitialCardsRoutine()
	{
		WaitForSeconds wfs = new WaitForSeconds(this.cardDealDelay);
		int num;
		for (int i = 0; i < 5; i = num + 1)
		{
			this.DealCardToPlayer(false);
			yield return wfs;
			num = i;
		}
		this.EnableCardInteractions();
		this.gameState = Poker.PokerGameState.PlayerTurn;
		yield break;
	}

	// Token: 0x060003D4 RID: 980 RVA: 0x00011CA4 File Offset: 0x0000FEA4
	[Server]
	public void ToggleCardSelection(PlayerInteract playerInteract, int cardIndex)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void Poker::ToggleCardSelection(PlayerInteract,System.Int32)' called when server was not active");
			return;
		}
		if (this.gameState != Poker.PokerGameState.PlayerTurn)
		{
			return;
		}
		if (cardIndex < 0 || cardIndex >= this.playerHand.Count)
		{
			return;
		}
		Debug.Log(string.Format("[Poker] ToggleCardSelection: {0}", cardIndex));
		CardData cardData = this.playerHand[cardIndex];
		bool flag = this.cardsToKeep.Contains(cardData);
		if (flag)
		{
			this.cardsToKeep.Remove(cardData);
			Debug.Log(string.Format("[Poker] Player deselected card {0} ({1} {2})", cardIndex, cardData.Suit, cardData.Rank));
		}
		else
		{
			this.cardsToKeep.Add(cardData);
			Debug.Log(string.Format("[Poker] Player selected card {0} ({1} {2}) to keep", cardIndex, cardData.Suit, cardData.Rank));
		}
		this.RpcUpdateCardSelection(cardIndex, !flag);
		this.RpcUpdateLockVisual(cardIndex, !flag);
		this.lockCardSfx.RpcPlayOneShotWithCustom3DPos(this.playerCardArea.transform.position);
	}

	// Token: 0x060003D5 RID: 981 RVA: 0x00011DB8 File Offset: 0x0000FFB8
	[Server]
	public void ToggleLockVisual(PlayerInteract playerInteract, int cardIndex)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void Poker::ToggleLockVisual(PlayerInteract,System.Int32)' called when server was not active");
			return;
		}
		if (cardIndex < 0 || cardIndex >= this.playerHand.Count)
		{
			return;
		}
		if (this.cardInteractions == null || cardIndex >= this.cardInteractions.Length)
		{
			return;
		}
		CardData item = this.playerHand[cardIndex];
		bool flag = this.cardsToKeep.Contains(item);
		if (this.cardInteractions[cardIndex] != null)
		{
			this.cardInteractions[cardIndex].IsInteractable = !flag;
		}
		this.SetLockVisualAtIndex(cardIndex, flag);
	}

	// Token: 0x060003D6 RID: 982 RVA: 0x00011E44 File Offset: 0x00010044
	public void ToggleLockVisualAtIndex(int index)
	{
		if (this.lockVisuals == null || index < 0 || index >= this.lockVisuals.Length)
		{
			return;
		}
		if (this.lockVisuals[index] != null)
		{
			this.lockVisuals[index].enabled = !this.lockVisuals[index].enabled;
		}
	}

	// Token: 0x060003D7 RID: 983 RVA: 0x00011E96 File Offset: 0x00010096
	public void SetLockVisualAtIndex(int index, bool enabled)
	{
		if (this.lockVisuals == null || index < 0 || index >= this.lockVisuals.Length)
		{
			return;
		}
		if (this.lockVisuals[index] != null)
		{
			this.lockVisuals[index].enabled = enabled;
		}
	}

	// Token: 0x060003D8 RID: 984 RVA: 0x00011ED0 File Offset: 0x000100D0
	[Command(requiresAuthority = false)]
	public void CmdToggleCardSelection(PlayerInteract playerInteract, int cardIndex)
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		writer.WriteNetworkBehaviour(playerInteract);
		writer.WriteVarInt(cardIndex);
		base.SendCommandInternal("System.Void Poker::CmdToggleCardSelection(PlayerInteract,System.Int32)", 938464306, writer, 0, false);
		NetworkWriterPool.Return(writer);
	}

	// Token: 0x060003D9 RID: 985 RVA: 0x00011F14 File Offset: 0x00010114
	[Command(requiresAuthority = false)]
	public void CmdKeepCard0(PlayerInteract playerInteract)
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		writer.WriteNetworkBehaviour(playerInteract);
		base.SendCommandInternal("System.Void Poker::CmdKeepCard0(PlayerInteract)", 237579634, writer, 0, false);
		NetworkWriterPool.Return(writer);
	}

	// Token: 0x060003DA RID: 986 RVA: 0x00011F50 File Offset: 0x00010150
	[Command(requiresAuthority = false)]
	public void CmdKeepCard1(PlayerInteract playerInteract)
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		writer.WriteNetworkBehaviour(playerInteract);
		base.SendCommandInternal("System.Void Poker::CmdKeepCard1(PlayerInteract)", 1706902393, writer, 0, false);
		NetworkWriterPool.Return(writer);
	}

	// Token: 0x060003DB RID: 987 RVA: 0x00011F8C File Offset: 0x0001018C
	[Command(requiresAuthority = false)]
	public void CmdKeepCard2(PlayerInteract playerInteract)
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		writer.WriteNetworkBehaviour(playerInteract);
		base.SendCommandInternal("System.Void Poker::CmdKeepCard2(PlayerInteract)", 589025896, writer, 0, false);
		NetworkWriterPool.Return(writer);
	}

	// Token: 0x060003DC RID: 988 RVA: 0x00011FC8 File Offset: 0x000101C8
	[Command(requiresAuthority = false)]
	public void CmdKeepCard3(PlayerInteract playerInteract)
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		writer.WriteNetworkBehaviour(playerInteract);
		base.SendCommandInternal("System.Void Poker::CmdKeepCard3(PlayerInteract)", 1748116687, writer, 0, false);
		NetworkWriterPool.Return(writer);
	}

	// Token: 0x060003DD RID: 989 RVA: 0x00012004 File Offset: 0x00010204
	[Command(requiresAuthority = false)]
	public void CmdKeepCard4(PlayerInteract playerInteract)
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		writer.WriteNetworkBehaviour(playerInteract);
		base.SendCommandInternal("System.Void Poker::CmdKeepCard4(PlayerInteract)", 255233166, writer, 0, false);
		NetworkWriterPool.Return(writer);
	}

	// Token: 0x060003DE RID: 990 RVA: 0x00012040 File Offset: 0x00010240
	[Server]
	public void ConfirmSelection(PlayerInteract playerInteract)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void Poker::ConfirmSelection(PlayerInteract)' called when server was not active");
			return;
		}
		if (this.gameState != Poker.PokerGameState.PlayerTurn)
		{
			return;
		}
		Debug.Log(string.Format("[Poker] Player confirmed selection. Keeping {0} cards", this.cardsToKeep.Count));
		this.DisableCardInteractions();
		this.ResetAllLockVisuals();
		this.RpcResetAllLockVisuals();
		this.gameState = Poker.PokerGameState.Replacing;
		base.StartCoroutine(this.ReplaceCardsRoutine());
	}

	// Token: 0x060003DF RID: 991 RVA: 0x000120B4 File Offset: 0x000102B4
	[Command(requiresAuthority = false)]
	public void CmdConfirmSelection(PlayerInteract playerInteract)
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		writer.WriteNetworkBehaviour(playerInteract);
		base.SendCommandInternal("System.Void Poker::CmdConfirmSelection(PlayerInteract)", -50022445, writer, 0, false);
		NetworkWriterPool.Return(writer);
	}

	// Token: 0x060003E0 RID: 992 RVA: 0x000120EE File Offset: 0x000102EE
	private IEnumerator ReplaceCardsRoutine()
	{
		WaitForSeconds wfs = new WaitForSeconds(this.cardDealDelay);
		int num;
		for (int i = this.spawnedPlayerCards.Count - 1; i >= 0; i = num - 1)
		{
			if (i < this.playerHand.Count)
			{
				CardData item = this.playerHand[i];
				if (!this.cardsToKeep.Contains(item))
				{
					this.playerHand.RemoveAt(i);
					GameObject gameObject = this.spawnedPlayerCards[i];
					this.spawnedPlayerCards.RemoveAt(i);
					this.discardPileCards.Add(gameObject);
					this.moveCardSfx.RpcPlayOneShotWithCustom3DPos(this.playerCardArea.transform.position);
					this.RpcDiscardCard(gameObject);
					yield return wfs;
				}
			}
			num = i;
		}
		int cardsNeeded = 5 - this.playerHand.Count;
		for (int i = 0; i < cardsNeeded; i = num + 1)
		{
			this.DealCardToPlayer(false);
			yield return wfs;
			num = i;
		}
		this.gameState = Poker.PokerGameState.Finished;
		yield return base.StartCoroutine(this.EvaluateAndEndGame());
		yield break;
	}

	// Token: 0x060003E1 RID: 993 RVA: 0x000120FD File Offset: 0x000102FD
	private IEnumerator EvaluateAndEndGame()
	{
		yield return new WaitForSeconds(1f);
		int num = this.EvaluatePokerHand(this.playerHand);
		string handDescription = this.GetHandDescription(num);
		Debug.Log(string.Format("[Poker] Final hand: {0} (Rank: {1})", handDescription, num));
		double multiplier = 0.0;
		PokerResult result;
		if (num >= 1)
		{
			result = PokerResult.Win;
			multiplier = (double)this.GetPayoutMultiplier(num) * base.EstimatedValue;
		}
		else
		{
			result = PokerResult.Lose;
		}
		this.EndGame(result, handDescription, num, multiplier);
		yield break;
	}

	// Token: 0x060003E2 RID: 994 RVA: 0x0001210C File Offset: 0x0001030C
	[Server]
	private void EndGame(PokerResult result, string handDescription, int handRank, double multiplier)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void Poker::EndGame(PokerResult,System.String,System.Int32,System.Double)' called when server was not active");
			return;
		}
		this.gameState = Poker.PokerGameState.Finished;
		Dictionary<string, object> gameSpecificData = new Dictionary<string, object>
		{
			{
				"handRank",
				handRank
			},
			{
				"handDescription",
				handDescription
			},
			{
				"lockedCardsCount",
				this.cardsToKeep.Count
			}
		};
		this.Payout(multiplier, ChangeType.GameResult, gameSpecificData, -1L);
		base.StartCoroutine(this.ResetGameRoutine());
		SFXComponent sfxcomponent = this.dealerSfx;
		if (sfxcomponent == null)
		{
			return;
		}
		sfxcomponent.RpcPlayOneShotWith3DPos();
	}

	// Token: 0x060003E3 RID: 995 RVA: 0x0001219C File Offset: 0x0001039C
	private IEnumerator ResetGameRoutine()
	{
		yield return new WaitForSeconds(2f);
		this.ResetGame();
		yield break;
	}

	// Token: 0x060003E4 RID: 996 RVA: 0x000121AC File Offset: 0x000103AC
	[Server]
	protected override void ResetGame()
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void Poker::ResetGame()' called when server was not active");
			return;
		}
		base.ResetGame();
		this.playerHand.Clear();
		this.cardsToKeep.Clear();
		this.gameState = Poker.PokerGameState.Waiting;
		this.CleanupCards(this.spawnedPlayerCards);
		this.CleanupCards(this.discardPileCards);
		this.spawnedPlayerCards.Clear();
		this.discardPileCards.Clear();
		this.ResetAllLockVisuals();
		this.resetCardSfx.RpcPlayOneShotWithCustom3DPos(this.playerCardArea.transform.position);
	}

	// Token: 0x060003E5 RID: 997 RVA: 0x00012240 File Offset: 0x00010440
	public void ResetAllLockVisuals()
	{
		if (this.lockVisuals == null)
		{
			return;
		}
		for (int i = 0; i < this.lockVisuals.Length; i++)
		{
			if (this.lockVisuals[i] != null)
			{
				this.lockVisuals[i].enabled = false;
			}
		}
	}

	// Token: 0x060003E6 RID: 998 RVA: 0x00012288 File Offset: 0x00010488
	[Server]
	private void CleanupCards(List<GameObject> cardList)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void Poker::CleanupCards(System.Collections.Generic.List`1<UnityEngine.GameObject>)' called when server was not active");
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

	// Token: 0x060003E7 RID: 999 RVA: 0x000122F4 File Offset: 0x000104F4
	[Server]
	private void InitializeDeck()
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void Poker::InitializeDeck()' called when server was not active");
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

	// Token: 0x060003E8 RID: 1000 RVA: 0x000123F4 File Offset: 0x000105F4
	[Server]
	private void ShuffleDeck()
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void Poker::ShuffleDeck()' called when server was not active");
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

	// Token: 0x060003E9 RID: 1001 RVA: 0x00012484 File Offset: 0x00010684
	[Server]
	private CardData DrawCardFromDeck()
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'CardData Poker::DrawCardFromDeck()' called when server was not active");
			return default(CardData);
		}
		if (this.deck.Count == 0)
		{
			Debug.LogWarning("[Poker] Deck is empty! Reinitializing...");
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

	// Token: 0x060003EA RID: 1002 RVA: 0x00012578 File Offset: 0x00010778
	private void OnDeckScaleChanged(float oldValue, float newValue)
	{
		if (this.deckOfCardsTransform != null)
		{
			this.deckOfCardsTransform.localScale = new Vector3(this.deckOfCardsTransform.localScale.x, newValue, this.deckOfCardsTransform.localScale.z);
		}
	}

	// Token: 0x060003EB RID: 1003 RVA: 0x000125C4 File Offset: 0x000107C4
	[Server]
	private void DealCardToPlayer(bool isFaceDown = false)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void Poker::DealCardToPlayer(System.Boolean)' called when server was not active");
			return;
		}
		CardData cardData = this.DrawCardFromDeck();
		this.playerHand.Add(cardData);
		this.SpawnCard(cardData, this.spawnedPlayerCards, isFaceDown);
	}

	// Token: 0x060003EC RID: 1004 RVA: 0x00012608 File Offset: 0x00010808
	[Server]
	private void SpawnCard(CardData cardData, List<GameObject> cardList, bool isHidden)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void Poker::SpawnCard(CardData,System.Collections.Generic.List`1<UnityEngine.GameObject>,System.Boolean)' called when server was not active");
			return;
		}
		if (Resources.Load<CardDataSO>(string.Format("Card_{0}_{1}", cardData.Suit, this.GetRankName(cardData.Rank))) == null)
		{
			Debug.LogWarning(string.Format("[Poker] Could not load CardDataSO: Card_{0}_{1}", cardData.Suit, this.GetRankName(cardData.Rank)));
		}
		Vector3 vector = this.CalculateCardPosition(cardList.Count, cardList.Count + 1);
		GameObject gameObject = Object.Instantiate<GameObject>(this.cardPrefab);
		NetworkServer.Spawn(gameObject, null);
		Card component = gameObject.GetComponent<Card>();
		if (component != null)
		{
			component.ServerSetCardData(cardData);
		}
		if (this.playerCardArea != null)
		{
			gameObject.transform.SetParent(this.playerCardArea);
			gameObject.transform.localPosition = vector;
			gameObject.transform.localRotation = Quaternion.identity;
		}
		this.RpcSetCardParentAndPosition(gameObject, vector);
		cardList.Add(gameObject);
		this.RepositionAllCards(cardList.Count);
		this.RpcRepositionAllCards(cardList.Count);
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

	// Token: 0x060003ED RID: 1005 RVA: 0x00012738 File Offset: 0x00010938
	[ClientRpc]
	private void RpcSetCardParentAndPosition(GameObject cardObject, Vector3 cardLocalPosition)
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		writer.WriteGameObject(cardObject);
		writer.WriteVector3(cardLocalPosition);
		this.SendRPCInternal("System.Void Poker::RpcSetCardParentAndPosition(UnityEngine.GameObject,UnityEngine.Vector3)", 1362776838, writer, 0, true);
		NetworkWriterPool.Return(writer);
	}

	// Token: 0x060003EE RID: 1006 RVA: 0x0001277C File Offset: 0x0001097C
	[Server]
	private Vector3 CalculateCardPosition(int cardIndex, int totalCardCount)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'UnityEngine.Vector3 Poker::CalculateCardPosition(System.Int32,System.Int32)' called when server was not active");
			return default(Vector3);
		}
		if (totalCardCount <= 1)
		{
			return Vector3.zero;
		}
		float d = -((float)(totalCardCount - 1) * this.cardSpacing) / 2f + (float)cardIndex * this.cardSpacing;
		return Vector3.right * d;
	}

	// Token: 0x060003EF RID: 1007 RVA: 0x000127DC File Offset: 0x000109DC
	[Server]
	private void RepositionAllCards(int totalCardCount)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void Poker::RepositionAllCards(System.Int32)' called when server was not active");
			return;
		}
		if (this.playerCardArea == null)
		{
			return;
		}
		List<Transform> list = new List<Transform>();
		int num = 0;
		while (num < this.spawnedPlayerCards.Count && num < totalCardCount)
		{
			if (this.spawnedPlayerCards[num] != null)
			{
				list.Add(this.spawnedPlayerCards[num].transform);
			}
			num++;
		}
		base.StartCoroutine(this.RepositionCardsSmoothRoutine(list, this.playerCardArea, totalCardCount));
	}

	// Token: 0x060003F0 RID: 1008 RVA: 0x00012870 File Offset: 0x00010A70
	[ClientRpc]
	private void RpcRepositionAllCards(int totalCardCount)
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		writer.WriteVarInt(totalCardCount);
		this.SendRPCInternal("System.Void Poker::RpcRepositionAllCards(System.Int32)", -1431656758, writer, 0, true);
		NetworkWriterPool.Return(writer);
	}

	// Token: 0x060003F1 RID: 1009 RVA: 0x000128AA File Offset: 0x00010AAA
	private IEnumerator RepositionCardsSmoothRoutine(List<Transform> cardTransforms, Transform cardArea, int totalCardCount)
	{
		int num = 0;
		while (num < cardTransforms.Count && num < totalCardCount)
		{
			if (cardTransforms[num] != null)
			{
				Vector3 targetPosition = this.CalculateCardPositionLocal(cardArea, num, totalCardCount);
				base.StartCoroutine(this.MoveCardSmoothRoutine(cardTransforms[num], targetPosition));
			}
			num++;
		}
		yield return null;
		yield break;
	}

	// Token: 0x060003F2 RID: 1010 RVA: 0x000128D0 File Offset: 0x00010AD0
	private Vector3 CalculateCardPositionLocal(Transform cardArea, int cardIndex, int totalCardCount)
	{
		if (totalCardCount <= 1)
		{
			return Vector3.zero;
		}
		float d = -((float)(totalCardCount - 1) * this.cardSpacing) / 2f + (float)cardIndex * this.cardSpacing;
		return Vector3.right * d;
	}

	// Token: 0x060003F3 RID: 1011 RVA: 0x0001290F File Offset: 0x00010B0F
	private IEnumerator MoveCardSmoothRoutine(Transform cardTransform, Vector3 targetPosition)
	{
		while (cardTransform != null)
		{
			Vector3 localPosition = cardTransform.localPosition;
			if (Vector3.Distance(localPosition, targetPosition) <= 0.01f)
			{
				break;
			}
			cardTransform.localPosition = Vector3.MoveTowards(localPosition, targetPosition, this.cardMoveSpeed * Time.deltaTime);
			yield return null;
		}
		if (cardTransform != null)
		{
			cardTransform.localPosition = targetPosition;
		}
		yield break;
	}

	// Token: 0x060003F4 RID: 1012 RVA: 0x0001292C File Offset: 0x00010B2C
	[ClientRpc]
	private void RpcDiscardCard(GameObject cardObject)
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		writer.WriteGameObject(cardObject);
		this.SendRPCInternal("System.Void Poker::RpcDiscardCard(UnityEngine.GameObject)", -494646251, writer, 0, true);
		NetworkWriterPool.Return(writer);
	}

	// Token: 0x060003F5 RID: 1013 RVA: 0x00012968 File Offset: 0x00010B68
	[ClientRpc]
	private void RpcUpdateCardSelection(int cardIndex, bool isSelected)
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		writer.WriteVarInt(cardIndex);
		writer.WriteBool(isSelected);
		this.SendRPCInternal("System.Void Poker::RpcUpdateCardSelection(System.Int32,System.Boolean)", 1129187464, writer, 0, true);
		NetworkWriterPool.Return(writer);
	}

	// Token: 0x060003F6 RID: 1014 RVA: 0x000129AC File Offset: 0x00010BAC
	[ClientRpc]
	private void RpcUpdateLockVisual(int index, bool enabled)
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		writer.WriteVarInt(index);
		writer.WriteBool(enabled);
		this.SendRPCInternal("System.Void Poker::RpcUpdateLockVisual(System.Int32,System.Boolean)", 1919071727, writer, 0, true);
		NetworkWriterPool.Return(writer);
	}

	// Token: 0x060003F7 RID: 1015 RVA: 0x000129F0 File Offset: 0x00010BF0
	[ClientRpc]
	private void RpcResetAllLockVisuals()
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		this.SendRPCInternal("System.Void Poker::RpcResetAllLockVisuals()", -155673675, writer, 0, true);
		NetworkWriterPool.Return(writer);
	}

	// Token: 0x060003F8 RID: 1016 RVA: 0x00012A20 File Offset: 0x00010C20
	private void EnableCardInteractions()
	{
		if (this.cardInteractions == null || this.cardInteractions.Length == 0)
		{
			return;
		}
		foreach (InteractableBase interactableBase in this.cardInteractions)
		{
			if (interactableBase != null)
			{
				interactableBase.IsInteractable = true;
				interactableBase.TooltipMessage = "Press [E] to select";
			}
		}
	}

	// Token: 0x060003F9 RID: 1017 RVA: 0x00012A74 File Offset: 0x00010C74
	private void DisableCardInteractions()
	{
		if (this.cardInteractions == null || this.cardInteractions.Length == 0)
		{
			return;
		}
		foreach (InteractableBase interactableBase in this.cardInteractions)
		{
			if (interactableBase != null)
			{
				interactableBase.IsInteractable = false;
			}
		}
	}

	// Token: 0x060003FA RID: 1018 RVA: 0x00012ABC File Offset: 0x00010CBC
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

	// Token: 0x060003FB RID: 1019 RVA: 0x00012B10 File Offset: 0x00010D10
	private int EvaluatePokerHand(SyncList<CardData> hand)
	{
		if (hand.Count != 5)
		{
			return 0;
		}
		List<int> values = (from card in hand
		select (int)card.Rank).ToList<int>();
		List<Suit> suits = (from card in hand
		select card.Suit).ToList<Suit>();
		if (this.IsStraight(values) && this.IsFlush(suits))
		{
			return 8;
		}
		if (this.HasFourOfAKind(values))
		{
			return 7;
		}
		if (this.HasFullHouse(values))
		{
			return 6;
		}
		if (this.IsFlush(suits))
		{
			return 5;
		}
		if (this.IsStraight(values))
		{
			return 4;
		}
		if (this.HasThreeOfAKind(values))
		{
			return 3;
		}
		if (this.HasTwoPair(values))
		{
			return 2;
		}
		if (this.HasOnePair(values))
		{
			return 1;
		}
		return 0;
	}

	// Token: 0x060003FC RID: 1020 RVA: 0x00012BE0 File Offset: 0x00010DE0
	private bool IsStraight(List<int> values)
	{
		List<int> list = (from v in values
		orderby v
		select v).ToList<int>();
		for (int i = 1; i < list.Count; i++)
		{
			if (list[i] != list[i - 1] + 1)
			{
				return false;
			}
		}
		return true;
	}

	// Token: 0x060003FD RID: 1021 RVA: 0x00012C40 File Offset: 0x00010E40
	private bool IsFlush(List<Suit> suits)
	{
		return suits.All((Suit s) => s == suits[0]);
	}

	// Token: 0x060003FE RID: 1022 RVA: 0x00012C74 File Offset: 0x00010E74
	private bool HasFourOfAKind(List<int> values)
	{
		return (from v in values
		group v by v).Any((IGrouping<int, int> g) => g.Count<int>() == 4);
	}

	// Token: 0x060003FF RID: 1023 RVA: 0x00012CCC File Offset: 0x00010ECC
	private bool HasFullHouse(List<int> values)
	{
		List<IGrouping<int, int>> list = (from v in values
		group v by v).ToList<IGrouping<int, int>>();
		if (list.Count == 2)
		{
			if (list.Any((IGrouping<int, int> g) => g.Count<int>() == 3))
			{
				return list.Any((IGrouping<int, int> g) => g.Count<int>() == 2);
			}
		}
		return false;
	}

	// Token: 0x06000400 RID: 1024 RVA: 0x00012D5C File Offset: 0x00010F5C
	private bool HasThreeOfAKind(List<int> values)
	{
		return (from v in values
		group v by v).Any((IGrouping<int, int> g) => g.Count<int>() == 3);
	}

	// Token: 0x06000401 RID: 1025 RVA: 0x00012DB4 File Offset: 0x00010FB4
	private bool HasTwoPair(List<int> values)
	{
		return (from v in values
		group v by v into g
		where g.Count<int>() == 2
		select g).Count<IGrouping<int, int>>() == 2;
	}

	// Token: 0x06000402 RID: 1026 RVA: 0x00012E14 File Offset: 0x00011014
	private bool HasOnePair(List<int> values)
	{
		return (from v in values
		group v by v).Any((IGrouping<int, int> g) => g.Count<int>() == 2);
	}

	// Token: 0x06000403 RID: 1027 RVA: 0x00012E6C File Offset: 0x0001106C
	private int GetPayoutMultiplier(int handRank)
	{
		int result;
		switch (handRank)
		{
		case 1:
			result = 1;
			break;
		case 2:
			result = 2;
			break;
		case 3:
			result = 3;
			break;
		case 4:
			result = 4;
			break;
		case 5:
			result = 6;
			break;
		case 6:
			result = 9;
			break;
		case 7:
			result = 25;
			break;
		case 8:
			result = 50;
			break;
		default:
			result = 1;
			break;
		}
		return result;
	}

	// Token: 0x06000404 RID: 1028 RVA: 0x00012ECC File Offset: 0x000110CC
	private string GetHandDescription(int handRank)
	{
		string result;
		switch (handRank)
		{
		case 1:
			result = "One Pair";
			break;
		case 2:
			result = "Two Pair";
			break;
		case 3:
			result = "Three of a Kind";
			break;
		case 4:
			result = "Straight";
			break;
		case 5:
			result = "Flush";
			break;
		case 6:
			result = "Full House";
			break;
		case 7:
			result = "Four of a Kind";
			break;
		case 8:
			result = "Straight Flush";
			break;
		default:
			result = "High Card";
			break;
		}
		return result;
	}

	// Token: 0x06000405 RID: 1029 RVA: 0x00012F4C File Offset: 0x0001114C
	public Poker()
	{
		base.InitSyncObject(this.playerHand);
		base.InitSyncObject(this.cardsToKeep);
		this._Mirror_SyncVarHookDelegate_deckScaleY = new Action<float, float>(this.OnDeckScaleChanged);
	}

	// Token: 0x06000406 RID: 1030 RVA: 0x00002321 File Offset: 0x00000521
	public override bool Weaved()
	{
		return true;
	}

	// Token: 0x17000061 RID: 97
	// (get) Token: 0x06000407 RID: 1031 RVA: 0x00013000 File Offset: 0x00011200
	// (set) Token: 0x06000408 RID: 1032 RVA: 0x00013013 File Offset: 0x00011213
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

	// Token: 0x06000409 RID: 1033 RVA: 0x00013032 File Offset: 0x00011232
	protected void UserCode_CmdToggleCardSelection__PlayerInteract__Int32(PlayerInteract playerInteract, int cardIndex)
	{
		this.ToggleCardSelection(playerInteract, cardIndex);
	}

	// Token: 0x0600040A RID: 1034 RVA: 0x0001303C File Offset: 0x0001123C
	protected static void InvokeUserCode_CmdToggleCardSelection__PlayerInteract__Int32(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdToggleCardSelection called on client.");
			return;
		}
		((Poker)obj).UserCode_CmdToggleCardSelection__PlayerInteract__Int32(reader.ReadNetworkBehaviour<PlayerInteract>(), reader.ReadVarInt());
	}

	// Token: 0x0600040B RID: 1035 RVA: 0x0001306B File Offset: 0x0001126B
	protected void UserCode_CmdKeepCard0__PlayerInteract(PlayerInteract playerInteract)
	{
		this.ToggleCardSelection(playerInteract, 0);
		this.ToggleLockVisual(playerInteract, 0);
	}

	// Token: 0x0600040C RID: 1036 RVA: 0x0001307D File Offset: 0x0001127D
	protected static void InvokeUserCode_CmdKeepCard0__PlayerInteract(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdKeepCard0 called on client.");
			return;
		}
		((Poker)obj).UserCode_CmdKeepCard0__PlayerInteract(reader.ReadNetworkBehaviour<PlayerInteract>());
	}

	// Token: 0x0600040D RID: 1037 RVA: 0x000130A6 File Offset: 0x000112A6
	protected void UserCode_CmdKeepCard1__PlayerInteract(PlayerInteract playerInteract)
	{
		this.ToggleCardSelection(playerInteract, 1);
		this.ToggleLockVisual(playerInteract, 1);
	}

	// Token: 0x0600040E RID: 1038 RVA: 0x000130B8 File Offset: 0x000112B8
	protected static void InvokeUserCode_CmdKeepCard1__PlayerInteract(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdKeepCard1 called on client.");
			return;
		}
		((Poker)obj).UserCode_CmdKeepCard1__PlayerInteract(reader.ReadNetworkBehaviour<PlayerInteract>());
	}

	// Token: 0x0600040F RID: 1039 RVA: 0x000130E1 File Offset: 0x000112E1
	protected void UserCode_CmdKeepCard2__PlayerInteract(PlayerInteract playerInteract)
	{
		this.ToggleCardSelection(playerInteract, 2);
		this.ToggleLockVisual(playerInteract, 2);
	}

	// Token: 0x06000410 RID: 1040 RVA: 0x000130F3 File Offset: 0x000112F3
	protected static void InvokeUserCode_CmdKeepCard2__PlayerInteract(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdKeepCard2 called on client.");
			return;
		}
		((Poker)obj).UserCode_CmdKeepCard2__PlayerInteract(reader.ReadNetworkBehaviour<PlayerInteract>());
	}

	// Token: 0x06000411 RID: 1041 RVA: 0x0001311C File Offset: 0x0001131C
	protected void UserCode_CmdKeepCard3__PlayerInteract(PlayerInteract playerInteract)
	{
		this.ToggleCardSelection(playerInteract, 3);
		this.ToggleLockVisual(playerInteract, 3);
	}

	// Token: 0x06000412 RID: 1042 RVA: 0x0001312E File Offset: 0x0001132E
	protected static void InvokeUserCode_CmdKeepCard3__PlayerInteract(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdKeepCard3 called on client.");
			return;
		}
		((Poker)obj).UserCode_CmdKeepCard3__PlayerInteract(reader.ReadNetworkBehaviour<PlayerInteract>());
	}

	// Token: 0x06000413 RID: 1043 RVA: 0x00013157 File Offset: 0x00011357
	protected void UserCode_CmdKeepCard4__PlayerInteract(PlayerInteract playerInteract)
	{
		this.ToggleCardSelection(playerInteract, 4);
		this.ToggleLockVisual(playerInteract, 4);
	}

	// Token: 0x06000414 RID: 1044 RVA: 0x00013169 File Offset: 0x00011369
	protected static void InvokeUserCode_CmdKeepCard4__PlayerInteract(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdKeepCard4 called on client.");
			return;
		}
		((Poker)obj).UserCode_CmdKeepCard4__PlayerInteract(reader.ReadNetworkBehaviour<PlayerInteract>());
	}

	// Token: 0x06000415 RID: 1045 RVA: 0x00013192 File Offset: 0x00011392
	protected void UserCode_CmdConfirmSelection__PlayerInteract(PlayerInteract playerInteract)
	{
		this.ConfirmSelection(playerInteract);
	}

	// Token: 0x06000416 RID: 1046 RVA: 0x0001319B File Offset: 0x0001139B
	protected static void InvokeUserCode_CmdConfirmSelection__PlayerInteract(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdConfirmSelection called on client.");
			return;
		}
		((Poker)obj).UserCode_CmdConfirmSelection__PlayerInteract(reader.ReadNetworkBehaviour<PlayerInteract>());
	}

	// Token: 0x06000417 RID: 1047 RVA: 0x000131C4 File Offset: 0x000113C4
	protected void UserCode_RpcSetCardParentAndPosition__GameObject__Vector3(GameObject cardObject, Vector3 cardLocalPosition)
	{
		if (this.playerCardArea != null && cardObject != null)
		{
			cardObject.transform.SetParent(this.playerCardArea);
			cardObject.transform.localPosition = cardLocalPosition;
			cardObject.transform.localRotation = Quaternion.identity;
		}
	}

	// Token: 0x06000418 RID: 1048 RVA: 0x00013215 File Offset: 0x00011415
	protected static void InvokeUserCode_RpcSetCardParentAndPosition__GameObject__Vector3(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcSetCardParentAndPosition called on server.");
			return;
		}
		((Poker)obj).UserCode_RpcSetCardParentAndPosition__GameObject__Vector3(reader.ReadGameObject(), reader.ReadVector3());
	}

	// Token: 0x06000419 RID: 1049 RVA: 0x00013244 File Offset: 0x00011444
	protected void UserCode_RpcRepositionAllCards__Int32(int totalCardCount)
	{
		if (this.playerCardArea == null)
		{
			return;
		}
		List<Transform> list = new List<Transform>();
		for (int i = 0; i < this.playerCardArea.childCount; i++)
		{
			Transform child = this.playerCardArea.GetChild(i);
			if (child.GetComponent<Card>() != null)
			{
				list.Add(child);
			}
		}
		list.Sort((Transform a, Transform b) => a.localPosition.x.CompareTo(b.localPosition.x));
		base.StartCoroutine(this.RepositionCardsSmoothRoutine(list, this.playerCardArea, totalCardCount));
	}

	// Token: 0x0600041A RID: 1050 RVA: 0x000132D8 File Offset: 0x000114D8
	protected static void InvokeUserCode_RpcRepositionAllCards__Int32(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcRepositionAllCards called on server.");
			return;
		}
		((Poker)obj).UserCode_RpcRepositionAllCards__Int32(reader.ReadVarInt());
	}

	// Token: 0x0600041B RID: 1051 RVA: 0x00013301 File Offset: 0x00011501
	protected void UserCode_RpcDiscardCard__GameObject(GameObject cardObject)
	{
		if (cardObject != null && this.discardPileArea != null)
		{
			cardObject.transform.SetParent(this.discardPileArea);
			cardObject.transform.localPosition = Vector3.zero;
		}
	}

	// Token: 0x0600041C RID: 1052 RVA: 0x0001333B File Offset: 0x0001153B
	protected static void InvokeUserCode_RpcDiscardCard__GameObject(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcDiscardCard called on server.");
			return;
		}
		((Poker)obj).UserCode_RpcDiscardCard__GameObject(reader.ReadGameObject());
	}

	// Token: 0x0600041D RID: 1053 RVA: 0x00013364 File Offset: 0x00011564
	protected void UserCode_RpcUpdateCardSelection__Int32__Boolean(int cardIndex, bool isSelected)
	{
		if (cardIndex < this.cardInteractions.Length && this.cardInteractions[cardIndex] != null)
		{
			if (isSelected)
			{
				this.cardInteractions[cardIndex].TooltipMessage = "Press [E] to deselect";
				return;
			}
			this.cardInteractions[cardIndex].TooltipMessage = "Press [E] to select";
		}
	}

	// Token: 0x0600041E RID: 1054 RVA: 0x000133B4 File Offset: 0x000115B4
	protected static void InvokeUserCode_RpcUpdateCardSelection__Int32__Boolean(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcUpdateCardSelection called on server.");
			return;
		}
		((Poker)obj).UserCode_RpcUpdateCardSelection__Int32__Boolean(reader.ReadVarInt(), reader.ReadBool());
	}

	// Token: 0x0600041F RID: 1055 RVA: 0x000133E3 File Offset: 0x000115E3
	protected void UserCode_RpcUpdateLockVisual__Int32__Boolean(int index, bool enabled)
	{
		this.SetLockVisualAtIndex(index, enabled);
	}

	// Token: 0x06000420 RID: 1056 RVA: 0x000133ED File Offset: 0x000115ED
	protected static void InvokeUserCode_RpcUpdateLockVisual__Int32__Boolean(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcUpdateLockVisual called on server.");
			return;
		}
		((Poker)obj).UserCode_RpcUpdateLockVisual__Int32__Boolean(reader.ReadVarInt(), reader.ReadBool());
	}

	// Token: 0x06000421 RID: 1057 RVA: 0x0001341C File Offset: 0x0001161C
	protected void UserCode_RpcResetAllLockVisuals()
	{
		this.ResetAllLockVisuals();
	}

	// Token: 0x06000422 RID: 1058 RVA: 0x00013424 File Offset: 0x00011624
	protected static void InvokeUserCode_RpcResetAllLockVisuals(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcResetAllLockVisuals called on server.");
			return;
		}
		((Poker)obj).UserCode_RpcResetAllLockVisuals();
	}

	// Token: 0x06000423 RID: 1059 RVA: 0x00013448 File Offset: 0x00011648
	static Poker()
	{
		RemoteProcedureCalls.RegisterCommand(typeof(Poker), "System.Void Poker::CmdToggleCardSelection(PlayerInteract,System.Int32)", new RemoteCallDelegate(Poker.InvokeUserCode_CmdToggleCardSelection__PlayerInteract__Int32), false);
		RemoteProcedureCalls.RegisterCommand(typeof(Poker), "System.Void Poker::CmdKeepCard0(PlayerInteract)", new RemoteCallDelegate(Poker.InvokeUserCode_CmdKeepCard0__PlayerInteract), false);
		RemoteProcedureCalls.RegisterCommand(typeof(Poker), "System.Void Poker::CmdKeepCard1(PlayerInteract)", new RemoteCallDelegate(Poker.InvokeUserCode_CmdKeepCard1__PlayerInteract), false);
		RemoteProcedureCalls.RegisterCommand(typeof(Poker), "System.Void Poker::CmdKeepCard2(PlayerInteract)", new RemoteCallDelegate(Poker.InvokeUserCode_CmdKeepCard2__PlayerInteract), false);
		RemoteProcedureCalls.RegisterCommand(typeof(Poker), "System.Void Poker::CmdKeepCard3(PlayerInteract)", new RemoteCallDelegate(Poker.InvokeUserCode_CmdKeepCard3__PlayerInteract), false);
		RemoteProcedureCalls.RegisterCommand(typeof(Poker), "System.Void Poker::CmdKeepCard4(PlayerInteract)", new RemoteCallDelegate(Poker.InvokeUserCode_CmdKeepCard4__PlayerInteract), false);
		RemoteProcedureCalls.RegisterCommand(typeof(Poker), "System.Void Poker::CmdConfirmSelection(PlayerInteract)", new RemoteCallDelegate(Poker.InvokeUserCode_CmdConfirmSelection__PlayerInteract), false);
		RemoteProcedureCalls.RegisterRpc(typeof(Poker), "System.Void Poker::RpcSetCardParentAndPosition(UnityEngine.GameObject,UnityEngine.Vector3)", new RemoteCallDelegate(Poker.InvokeUserCode_RpcSetCardParentAndPosition__GameObject__Vector3));
		RemoteProcedureCalls.RegisterRpc(typeof(Poker), "System.Void Poker::RpcRepositionAllCards(System.Int32)", new RemoteCallDelegate(Poker.InvokeUserCode_RpcRepositionAllCards__Int32));
		RemoteProcedureCalls.RegisterRpc(typeof(Poker), "System.Void Poker::RpcDiscardCard(UnityEngine.GameObject)", new RemoteCallDelegate(Poker.InvokeUserCode_RpcDiscardCard__GameObject));
		RemoteProcedureCalls.RegisterRpc(typeof(Poker), "System.Void Poker::RpcUpdateCardSelection(System.Int32,System.Boolean)", new RemoteCallDelegate(Poker.InvokeUserCode_RpcUpdateCardSelection__Int32__Boolean));
		RemoteProcedureCalls.RegisterRpc(typeof(Poker), "System.Void Poker::RpcUpdateLockVisual(System.Int32,System.Boolean)", new RemoteCallDelegate(Poker.InvokeUserCode_RpcUpdateLockVisual__Int32__Boolean));
		RemoteProcedureCalls.RegisterRpc(typeof(Poker), "System.Void Poker::RpcResetAllLockVisuals()", new RemoteCallDelegate(Poker.InvokeUserCode_RpcResetAllLockVisuals));
	}

	// Token: 0x06000424 RID: 1060 RVA: 0x000135FC File Offset: 0x000117FC
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

	// Token: 0x06000425 RID: 1061 RVA: 0x00013654 File Offset: 0x00011854
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

	// Token: 0x040002B7 RID: 695
	[Header("Card Interactions")]
	[SerializeField]
	private InteractableBase[] cardInteractions;

	// Token: 0x040002B8 RID: 696
	[SerializeField]
	private SpriteRenderer[] lockVisuals;

	// Token: 0x040002B9 RID: 697
	[Header("Card System")]
	[SerializeField]
	private GameObject cardPrefab;

	// Token: 0x040002BA RID: 698
	[SerializeField]
	private Transform playerCardArea;

	// Token: 0x040002BB RID: 699
	[SerializeField]
	private Transform discardPileArea;

	// Token: 0x040002BC RID: 700
	[SerializeField]
	private Transform deckOfCardsTransform;

	// Token: 0x040002BD RID: 701
	[SerializeField]
	private int numberOfDecks = 1;

	// Token: 0x040002BE RID: 702
	[SerializeField]
	private float baseScaleForOneDeck = 1f;

	// Token: 0x040002BF RID: 703
	[SerializeField]
	private float cardSpacing = 0.5f;

	// Token: 0x040002C0 RID: 704
	[SerializeField]
	private float cardMoveSpeed = 5f;

	// Token: 0x040002C1 RID: 705
	[SerializeField]
	private float cardDealDelay = 0.3f;

	// Token: 0x040002C2 RID: 706
	[Header("Game State")]
	[SerializeField]
	private readonly SyncList<CardData> playerHand = new SyncList<CardData>();

	// Token: 0x040002C3 RID: 707
	[SerializeField]
	private readonly SyncList<CardData> cardsToKeep = new SyncList<CardData>();

	// Token: 0x040002C4 RID: 708
	private List<CardData> deck = new List<CardData>();

	// Token: 0x040002C5 RID: 709
	private List<GameObject> spawnedPlayerCards = new List<GameObject>();

	// Token: 0x040002C6 RID: 710
	private List<GameObject> discardPileCards = new List<GameObject>();

	// Token: 0x040002C7 RID: 711
	private int initialDeckCount;

	// Token: 0x040002C8 RID: 712
	private bool deckInitialized;

	// Token: 0x040002C9 RID: 713
	[SyncVar(hook = "OnDeckScaleChanged")]
	private float deckScaleY = 1f;

	// Token: 0x040002CA RID: 714
	[Header("SFX")]
	[SerializeField]
	private SFXComponent dealerSfx;

	// Token: 0x040002CB RID: 715
	[SerializeField]
	private SFXComponent lockCardSfx;

	// Token: 0x040002CC RID: 716
	[SerializeField]
	private SFXComponent resetCardSfx;

	// Token: 0x040002CD RID: 717
	[SerializeField]
	private SFXComponent moveCardSfx;

	// Token: 0x040002CE RID: 718
	private Poker.PokerGameState gameState;

	// Token: 0x040002CF RID: 719
	public Action<float, float> _Mirror_SyncVarHookDelegate_deckScaleY;

	// Token: 0x0200006F RID: 111
	private enum PokerGameState
	{
		// Token: 0x040002D1 RID: 721
		Waiting,
		// Token: 0x040002D2 RID: 722
		Dealing,
		// Token: 0x040002D3 RID: 723
		PlayerTurn,
		// Token: 0x040002D4 RID: 724
		Replacing,
		// Token: 0x040002D5 RID: 725
		Finished
	}
}
