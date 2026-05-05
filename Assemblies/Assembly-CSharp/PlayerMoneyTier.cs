using System;
using UnityEngine;

// Token: 0x0200019E RID: 414
[CreateAssetMenu(fileName = "PlayerMoneyTier", menuName = "Player Money/Player Money Tier")]
public class PlayerMoneyTier : ScriptableObject
{
	// Token: 0x040009D3 RID: 2515
	[Header("Tier Settings")]
	[Tooltip("The tier number (1-4)")]
	public int tierNumber = 1;

	// Token: 0x040009D4 RID: 2516
	[Header("Money Limits")]
	[Tooltip("Maximum amount of money the player can hold in this tier")]
	public int maxMoney = 1000;
}
