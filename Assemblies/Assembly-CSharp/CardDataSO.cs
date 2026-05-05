using System;
using UnityEngine;

// Token: 0x02000094 RID: 148
[CreateAssetMenu(fileName = "CardData", menuName = "Gamble With Your Friends/Card Data", order = 0)]
public class CardDataSO : ScriptableObject
{
	// Token: 0x040003BC RID: 956
	[Header("Card Visual")]
	[SerializeField]
	public Sprite cardSprite;
}
