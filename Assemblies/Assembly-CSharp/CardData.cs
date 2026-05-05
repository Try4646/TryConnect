using System;

// Token: 0x02000093 RID: 147
[Serializable]
public struct CardData
{
	// Token: 0x06000543 RID: 1347 RVA: 0x00017785 File Offset: 0x00015985
	public CardData(Suit suit, Rank rank)
	{
		this.Suit = suit;
		this.Rank = rank;
	}

	// Token: 0x06000544 RID: 1348 RVA: 0x00017798 File Offset: 0x00015998
	public int GetBlackjackValue()
	{
		Rank rank = this.Rank;
		if (rank == Rank.Ace)
		{
			return 11;
		}
		if (rank - Rank.Jack <= 2)
		{
			return 10;
		}
		return (int)this.Rank;
	}

	// Token: 0x06000545 RID: 1349 RVA: 0x000177C4 File Offset: 0x000159C4
	public int GetBaccaratValue()
	{
		Rank rank = this.Rank;
		if (rank - Rank.Jack <= 2)
		{
			return 0;
		}
		return (int)this.Rank;
	}

	// Token: 0x06000546 RID: 1350 RVA: 0x000177E8 File Offset: 0x000159E8
	public string GetDisplayString()
	{
		Rank rank = this.Rank;
		string str;
		if (rank != Rank.Ace)
		{
			switch (rank)
			{
			case Rank.Jack:
				str = "J";
				break;
			case Rank.Queen:
				str = "Q";
				break;
			case Rank.King:
				str = "K";
				break;
			default:
			{
				int rank2 = (int)this.Rank;
				str = rank2.ToString();
				break;
			}
			}
		}
		else
		{
			str = "A";
		}
		string text;
		switch (this.Suit)
		{
		case Suit.Hearts:
			text = "♥";
			break;
		case Suit.Diamonds:
			text = "♦";
			break;
		case Suit.Clubs:
			text = "♣";
			break;
		case Suit.Spades:
			text = "♠";
			break;
		default:
			text = "?";
			break;
		}
		string str2 = text;
		return str + str2;
	}

	// Token: 0x040003BA RID: 954
	public Suit Suit;

	// Token: 0x040003BB RID: 955
	public Rank Rank;
}
