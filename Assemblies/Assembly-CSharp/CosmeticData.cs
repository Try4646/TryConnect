using System;
using UnityEngine;

// Token: 0x020000AB RID: 171
[CreateAssetMenu(fileName = "New Cosmetic", menuName = "Gacha/Cosmetic Data")]
public class CosmeticData : ScriptableObject
{
	// Token: 0x0400045F RID: 1119
	[Header("Cosmetic Info")]
	[UniqueID("cosmetics")]
	public int cosmeticId;

	// Token: 0x04000460 RID: 1120
	public string cosmeticName;

	// Token: 0x04000461 RID: 1121
	public string description;

	// Token: 0x04000462 RID: 1122
	public GameObject cosmeticModel;

	// Token: 0x04000463 RID: 1123
	public Material cosmeticMaterial;

	// Token: 0x04000464 RID: 1124
	public CosmeticType cosmeticType;

	// Token: 0x04000465 RID: 1125
	[Header("Rarity")]
	public CosmeticRarity rarity;
}
