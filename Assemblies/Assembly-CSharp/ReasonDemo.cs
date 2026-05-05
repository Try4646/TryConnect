using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

// Token: 0x020002D6 RID: 726
public class ReasonDemo : MonoBehaviour
{
	// Token: 0x06001985 RID: 6533 RVA: 0x0006B030 File Offset: 0x00069230
	private void Awake()
	{
		this._patterns = new List<SentencePattern>
		{
			new SentencePattern("1: we + Conseq", new PatternToken[]
			{
				PatternToken.Lit("we"),
				PatternToken.Slot(SlotType.Conseq)
			}),
			new SentencePattern("2: Verb Object", new PatternToken[]
			{
				PatternToken.Slot(SlotType.Verb),
				PatternToken.Slot(SlotType.Object)
			}),
			new SentencePattern("2: Verb Opponent", new PatternToken[]
			{
				PatternToken.Slot(SlotType.Verb),
				PatternToken.Slot(SlotType.Opponent)
			}),
			new SentencePattern("2: Verb at Location", new PatternToken[]
			{
				PatternToken.Slot(SlotType.Verb),
				PatternToken.Lit("at"),
				PatternToken.Slot(SlotType.Location)
			}),
			new SentencePattern("3: Verb Object and Conseq", new PatternToken[]
			{
				PatternToken.Slot(SlotType.Verb),
				PatternToken.Slot(SlotType.Object),
				PatternToken.Lit("and"),
				PatternToken.Slot(SlotType.Conseq)
			}),
			new SentencePattern("3: Verb Object at Location", new PatternToken[]
			{
				PatternToken.Slot(SlotType.Verb),
				PatternToken.Slot(SlotType.Object),
				PatternToken.Lit("at"),
				PatternToken.Slot(SlotType.Location)
			}),
			new SentencePattern("3: Verb Object with Opponent", new PatternToken[]
			{
				PatternToken.Slot(SlotType.Verb),
				PatternToken.Slot(SlotType.Object),
				PatternToken.Lit("with"),
				PatternToken.Slot(SlotType.Opponent)
			}),
			new SentencePattern("3: we + Conseq at Location", new PatternToken[]
			{
				PatternToken.Lit("we"),
				PatternToken.Slot(SlotType.Conseq),
				PatternToken.Lit("at"),
				PatternToken.Slot(SlotType.Location)
			}),
			new SentencePattern("3: Verb Opponent and Conseq", new PatternToken[]
			{
				PatternToken.Slot(SlotType.Verb),
				PatternToken.Slot(SlotType.Opponent),
				PatternToken.Lit("and"),
				PatternToken.Slot(SlotType.Conseq)
			})
		};
	}

	// Token: 0x06001986 RID: 6534 RVA: 0x0006B2A9 File Offset: 0x000694A9
	private void Update()
	{
		if (Input.GetKeyDown(KeyCode.R))
		{
			this.GenerateExampleOutput();
		}
	}

	// Token: 0x06001987 RID: 6535 RVA: 0x0006B2BC File Offset: 0x000694BC
	private void GenerateExampleOutput()
	{
		SentencePattern pattern = this.PickPatternByCap(this.maxSlots);
		string text = SentenceGenerator.BuildLine(this.wordBank, pattern, this.maxSlots);
		this.exampleOutput.text = text;
	}

	// Token: 0x06001988 RID: 6536 RVA: 0x0006B2F8 File Offset: 0x000694F8
	private static int CountSlots(SentencePattern p)
	{
		int num = 0;
		using (List<PatternToken>.Enumerator enumerator = p.tokens.GetEnumerator())
		{
			while (enumerator.MoveNext())
			{
				if (!enumerator.Current.isLiteral)
				{
					num++;
				}
			}
		}
		return num;
	}

	// Token: 0x06001989 RID: 6537 RVA: 0x0006B354 File Offset: 0x00069554
	private SentencePattern PickPatternByCap(int maxSlots)
	{
		List<SentencePattern> list = this._patterns.FindAll((SentencePattern p) => ReasonDemo.CountSlots(p) <= maxSlots);
		if (list.Count == 0)
		{
			return this._patterns[0];
		}
		return list[Random.Range(0, list.Count)];
	}

	// Token: 0x04001065 RID: 4197
	[SerializeField]
	private WordBankSO wordBank;

	// Token: 0x04001066 RID: 4198
	[Range(1f, 3f)]
	public int maxSlots = 3;

	// Token: 0x04001067 RID: 4199
	[SerializeField]
	private TextMeshProUGUI exampleOutput;

	// Token: 0x04001068 RID: 4200
	private List<SentencePattern> _patterns;
}
