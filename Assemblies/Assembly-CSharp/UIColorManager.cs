using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Token: 0x0200033D RID: 829
public class UIColorManager : MonoBehaviour
{
	// Token: 0x06001B5B RID: 7003 RVA: 0x000747A8 File Offset: 0x000729A8
	private void Awake()
	{
		if (this.lobbySettings == null)
		{
			this.lobbySettings = Resources.Load<LobbySettings>("LobbySettings");
		}
		if (this.palette == null)
		{
			this.palette = Resources.Load<UIColorPalette>("ColorSettings");
			if (this.palette == null)
			{
				Debug.LogError("UIColorManager: Failed to load UIColorPalette from Resources/ColorSettings. Please ensure the ColorSettings asset exists in a Resources folder.");
			}
		}
	}

	// Token: 0x06001B5C RID: 7004 RVA: 0x00074809 File Offset: 0x00072A09
	private void OnEnable()
	{
		UIColorPalette.PaletteChanged += this.OnPaletteChanged;
		LobbySettings.SettingsChanged += this.OnLobbySettingsChanged;
		this.ApplyColors();
	}

	// Token: 0x06001B5D RID: 7005 RVA: 0x00074833 File Offset: 0x00072A33
	private void OnDisable()
	{
		UIColorPalette.PaletteChanged -= this.OnPaletteChanged;
		LobbySettings.SettingsChanged -= this.OnLobbySettingsChanged;
	}

	// Token: 0x06001B5E RID: 7006 RVA: 0x00074857 File Offset: 0x00072A57
	private void OnPaletteChanged(UIColorPalette changed)
	{
		if (changed == this.palette)
		{
			this.ApplyColors();
		}
	}

	// Token: 0x06001B5F RID: 7007 RVA: 0x00074870 File Offset: 0x00072A70
	private void OnLobbySettingsChanged(LobbySettings settings)
	{
		PlayerProfile component = base.gameObject.GetComponent<PlayerProfile>();
		if (component != null && settings != null && this.palette != null)
		{
			settings.GetPlayerBySteamId(component.steamId);
		}
		Color playerColor = this.GetPlayerColor();
		if (this._lastAppliedPlayerColor != playerColor)
		{
			this._lastAppliedPlayerColor = playerColor;
			this.ApplyColors();
		}
	}

	// Token: 0x06001B60 RID: 7008 RVA: 0x000748D8 File Offset: 0x00072AD8
	public void ApplyColors()
	{
		if (this.palette == null)
		{
			return;
		}
		foreach (UIColorManager.ColorGroup colorGroup in this.colorGroups)
		{
			Color color;
			switch (colorGroup.tone)
			{
			case UIColorManager.Tone.ProfitGreen:
				color = this.palette.profitGreen;
				break;
			case UIColorManager.Tone.LossRed:
				color = this.palette.lossRed;
				break;
			case UIColorManager.Tone.TicketYellow:
				color = this.palette.ticketYellow;
				break;
			case UIColorManager.Tone.White:
				color = this.palette.white;
				break;
			case UIColorManager.Tone.Black:
				color = this.palette.black;
				break;
			case UIColorManager.Tone.PlayerColor:
				color = this.GetPlayerColor();
				break;
			case UIColorManager.Tone.GwyfMainColor:
				color = this.palette.gwyfMainColor;
				break;
			case UIColorManager.Tone.GwyfSecondaryColor:
				color = this.palette.gwyfSecondaryColor;
				break;
			case UIColorManager.Tone.NPCColor:
				color = this.palette.NPCColor;
				break;
			default:
				color = this.palette.profitGreen;
				break;
			}
			Color color2 = color;
			if (colorGroup.useTransparency)
			{
				color2.a = 0.5f;
			}
			foreach (Image image in colorGroup.images)
			{
				if (image)
				{
					image.color = color2;
				}
			}
			foreach (TextMeshProUGUI textMeshProUGUI in colorGroup.texts)
			{
				if (textMeshProUGUI)
				{
					textMeshProUGUI.color = color2;
				}
			}
			foreach (TextMeshPro textMeshPro in colorGroup.textMeshPros)
			{
				if (textMeshPro)
				{
					textMeshPro.color = color2;
				}
			}
			foreach (MeshRenderer meshRenderer in colorGroup.meshRenderers)
			{
				if (meshRenderer)
				{
					if (Application.isPlaying)
					{
						Material sharedMaterial = meshRenderer.sharedMaterial;
						if (!(sharedMaterial == null))
						{
							if (!this._runtimeMaterials.ContainsKey(meshRenderer))
							{
								this._runtimeMaterials[meshRenderer] = new Material(sharedMaterial);
								meshRenderer.material = this._runtimeMaterials[meshRenderer];
							}
							Material material = this._runtimeMaterials[meshRenderer];
							material.color = new Color(color2.r, color2.g, color2.b, material.color.a);
							if (colorGroup.useEmission && material.HasProperty("_EmissionColor"))
							{
								Color playerColor = this.GetPlayerColor();
								material.SetColor("_EmissionColor", playerColor);
								material.EnableKeyword("_EMISSION");
							}
							else if (material.HasProperty("_EmissionColor"))
							{
								material.DisableKeyword("_EMISSION");
							}
						}
					}
					else
					{
						Material sharedMaterial2 = meshRenderer.sharedMaterial;
						if (!(sharedMaterial2 == null))
						{
							sharedMaterial2.color = new Color(color2.r, color2.g, color2.b, sharedMaterial2.color.a);
						}
					}
				}
			}
			foreach (SkinnedMeshRenderer skinnedMeshRenderer in colorGroup.skinnedMeshRenderers)
			{
				if (skinnedMeshRenderer)
				{
					if (Application.isPlaying)
					{
						Material sharedMaterial3 = skinnedMeshRenderer.sharedMaterial;
						if (!(sharedMaterial3 == null))
						{
							if (!this._runtimeMaterials.ContainsKey(skinnedMeshRenderer))
							{
								this._runtimeMaterials[skinnedMeshRenderer] = new Material(sharedMaterial3);
								skinnedMeshRenderer.material = this._runtimeMaterials[skinnedMeshRenderer];
							}
							Material material2 = this._runtimeMaterials[skinnedMeshRenderer];
							material2.color = new Color(color2.r, color2.g, color2.b, material2.color.a);
						}
					}
					else
					{
						Material sharedMaterial4 = skinnedMeshRenderer.sharedMaterial;
						if (!(sharedMaterial4 == null))
						{
							sharedMaterial4.color = new Color(color2.r, color2.g, color2.b, sharedMaterial4.color.a);
						}
					}
				}
			}
			foreach (Material material3 in colorGroup.decalMaterials)
			{
				if (material3)
				{
					material3.SetColor("_Tint", color2);
					if (colorGroup.useEmission && material3.HasProperty("_EmissionColor"))
					{
						Color playerColor2 = this.GetPlayerColor();
						material3.SetColor("_EmissionColor", playerColor2);
						material3.EnableKeyword("_EMISSION");
					}
					else if (material3.HasProperty("_EmissionColor"))
					{
						material3.DisableKeyword("_EMISSION");
					}
				}
			}
			foreach (LineRenderer lineRenderer in colorGroup.lineRenderers)
			{
				if (lineRenderer)
				{
					if (Application.isPlaying)
					{
						if (!this._runtimeLineMaterials.ContainsKey(lineRenderer))
						{
							Material sharedMaterial5 = lineRenderer.sharedMaterial;
							if (sharedMaterial5 == null)
							{
								continue;
							}
							this._runtimeLineMaterials[lineRenderer] = new Material(sharedMaterial5);
							lineRenderer.material = this._runtimeLineMaterials[lineRenderer];
						}
						Material material4 = this._runtimeLineMaterials[lineRenderer];
						material4.color = new Color(color2.r, color2.g, color2.b, material4.color.a);
						if (colorGroup.useEmission && material4.HasProperty("_EmissionColor"))
						{
							Color playerColor3 = this.GetPlayerColor();
							material4.SetColor("_EmissionColor", playerColor3);
							material4.EnableKeyword("_EMISSION");
						}
						else if (material4.HasProperty("_EmissionColor"))
						{
							material4.DisableKeyword("_EMISSION");
						}
					}
					else
					{
						lineRenderer.sharedMaterial.color = new Color(color2.r, color2.g, color2.b, lineRenderer.sharedMaterial.color.a);
					}
				}
			}
		}
	}

	// Token: 0x06001B61 RID: 7009 RVA: 0x00074FF4 File Offset: 0x000731F4
	private Color GetPlayerColor()
	{
		if (this.lobbySettings == null)
		{
			return this.palette.playerColor;
		}
		SteamIdComponent steamIdComponent;
		if (base.gameObject.TryGetComponent<SteamIdComponent>(out steamIdComponent))
		{
			PlayerInfo playerBySteamId = this.lobbySettings.GetPlayerBySteamId(steamIdComponent.SteamId);
			if (playerBySteamId == null)
			{
				return this.palette.playerColor;
			}
			return playerBySteamId.playerColor;
		}
		else
		{
			PlayerProfile playerProfile;
			if (!base.gameObject.TryGetComponent<PlayerProfile>(out playerProfile))
			{
				return this.palette.playerColor;
			}
			PlayerInfo playerBySteamId2 = this.lobbySettings.GetPlayerBySteamId(playerProfile.steamId);
			if (playerBySteamId2 == null)
			{
				return this.palette.playerColor;
			}
			return playerBySteamId2.playerColor;
		}
	}

	// Token: 0x04001222 RID: 4642
	[SerializeField]
	private UIColorPalette palette;

	// Token: 0x04001223 RID: 4643
	[SerializeField]
	private List<UIColorManager.ColorGroup> colorGroups = new List<UIColorManager.ColorGroup>();

	// Token: 0x04001224 RID: 4644
	private LobbySettings lobbySettings;

	// Token: 0x04001225 RID: 4645
	private Color _lastAppliedPlayerColor = new Color(0f, 0f, 0f, 0f);

	// Token: 0x04001226 RID: 4646
	private Dictionary<Renderer, Material> _runtimeMaterials = new Dictionary<Renderer, Material>();

	// Token: 0x04001227 RID: 4647
	private Dictionary<LineRenderer, Material> _runtimeLineMaterials = new Dictionary<LineRenderer, Material>();

	// Token: 0x0200033E RID: 830
	public enum Tone
	{
		// Token: 0x04001229 RID: 4649
		ProfitGreen,
		// Token: 0x0400122A RID: 4650
		LossRed,
		// Token: 0x0400122B RID: 4651
		TicketYellow,
		// Token: 0x0400122C RID: 4652
		White,
		// Token: 0x0400122D RID: 4653
		Black,
		// Token: 0x0400122E RID: 4654
		PlayerColor,
		// Token: 0x0400122F RID: 4655
		GwyfMainColor,
		// Token: 0x04001230 RID: 4656
		GwyfSecondaryColor,
		// Token: 0x04001231 RID: 4657
		NPCColor
	}

	// Token: 0x0200033F RID: 831
	[Serializable]
	public class ColorGroup
	{
		// Token: 0x04001232 RID: 4658
		public UIColorManager.Tone tone;

		// Token: 0x04001233 RID: 4659
		public bool useTransparency;

		// Token: 0x04001234 RID: 4660
		public bool useEmission;

		// Token: 0x04001235 RID: 4661
		public List<Image> images = new List<Image>();

		// Token: 0x04001236 RID: 4662
		public List<TextMeshProUGUI> texts = new List<TextMeshProUGUI>();

		// Token: 0x04001237 RID: 4663
		public List<TextMeshPro> textMeshPros = new List<TextMeshPro>();

		// Token: 0x04001238 RID: 4664
		public List<MeshRenderer> meshRenderers = new List<MeshRenderer>();

		// Token: 0x04001239 RID: 4665
		public List<SkinnedMeshRenderer> skinnedMeshRenderers = new List<SkinnedMeshRenderer>();

		// Token: 0x0400123A RID: 4666
		public List<Material> decalMaterials = new List<Material>();

		// Token: 0x0400123B RID: 4667
		public List<LineRenderer> lineRenderers = new List<LineRenderer>();
	}
}
