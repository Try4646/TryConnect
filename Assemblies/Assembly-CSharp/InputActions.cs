using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;

// Token: 0x020002B9 RID: 697
public class InputActions : IInputActionCollection2, IInputActionCollection, IEnumerable<InputAction>, IEnumerable, IDisposable
{
	// Token: 0x1700022F RID: 559
	// (get) Token: 0x06001863 RID: 6243 RVA: 0x00067521 File Offset: 0x00065721
	public InputActionAsset asset { get; }

	// Token: 0x06001864 RID: 6244 RVA: 0x0006752C File Offset: 0x0006572C
	public InputActions()
	{
		this.asset = InputActionAsset.FromJson("{\n    \"version\": 1,\n    \"name\": \"InputActions\",\n    \"maps\": [\n        {\n            \"name\": \"Player\",\n            \"id\": \"5770ebab-5e21-4325-93b0-162d5ad04eab\",\n            \"actions\": [\n                {\n                    \"name\": \"Move\",\n                    \"type\": \"Value\",\n                    \"id\": \"4da15314-2555-43e8-a6f9-09af9a4faa97\",\n                    \"expectedControlType\": \"Vector2\",\n                    \"processors\": \"\",\n                    \"interactions\": \"\",\n                    \"initialStateCheck\": true\n                },\n                {\n                    \"name\": \"Aim\",\n                    \"type\": \"Value\",\n                    \"id\": \"34f98767-7179-4ce9-a1ec-543f5d4db0af\",\n                    \"expectedControlType\": \"Delta\",\n                    \"processors\": \"\",\n                    \"interactions\": \"\",\n                    \"initialStateCheck\": true\n                },\n                {\n                    \"name\": \"Jump\",\n                    \"type\": \"Button\",\n                    \"id\": \"7965c018-f2f3-4a37-a653-ffdd121a9e1f\",\n                    \"expectedControlType\": \"\",\n                    \"processors\": \"\",\n                    \"interactions\": \"\",\n                    \"initialStateCheck\": false\n                },\n                {\n                    \"name\": \"Crouch\",\n                    \"type\": \"Button\",\n                    \"id\": \"88d012bc-c097-414e-9d04-912ab3b0e534\",\n                    \"expectedControlType\": \"\",\n                    \"processors\": \"\",\n                    \"interactions\": \"\",\n                    \"initialStateCheck\": false\n                },\n                {\n                    \"name\": \"Sprint\",\n                    \"type\": \"Button\",\n                    \"id\": \"922e8702-a416-4b83-a438-44f85500e4f0\",\n                    \"expectedControlType\": \"\",\n                    \"processors\": \"\",\n                    \"interactions\": \"\",\n                    \"initialStateCheck\": false\n                },\n                {\n                    \"name\": \"Interact\",\n                    \"type\": \"Button\",\n                    \"id\": \"2ad185b5-6df6-4f44-b25b-9d992357da91\",\n                    \"expectedControlType\": \"\",\n                    \"processors\": \"\",\n                    \"interactions\": \"\",\n                    \"initialStateCheck\": false\n                },\n                {\n                    \"name\": \"SkipUI\",\n                    \"type\": \"Button\",\n                    \"id\": \"cac312c6-6853-4ff1-80ca-f0805bfc5eb3\",\n                    \"expectedControlType\": \"\",\n                    \"processors\": \"\",\n                    \"interactions\": \"\",\n                    \"initialStateCheck\": false\n                },\n                {\n                    \"name\": \"ThrowItem\",\n                    \"type\": \"Button\",\n                    \"id\": \"d529c9ae-50ea-46c8-a403-1d75e2c7d192\",\n                    \"expectedControlType\": \"\",\n                    \"processors\": \"\",\n                    \"interactions\": \"\",\n                    \"initialStateCheck\": false\n                },\n                {\n                    \"name\": \"Zoom\",\n                    \"type\": \"Button\",\n                    \"id\": \"7009ea0d-3c8d-4b64-82d7-bd565f632550\",\n                    \"expectedControlType\": \"\",\n                    \"processors\": \"\",\n                    \"interactions\": \"\",\n                    \"initialStateCheck\": false\n                },\n                {\n                    \"name\": \"ItemSelect\",\n                    \"type\": \"Value\",\n                    \"id\": \"8be36012-d455-4c4a-8dd0-542ca097d813\",\n                    \"expectedControlType\": \"\",\n                    \"processors\": \"\",\n                    \"interactions\": \"\",\n                    \"initialStateCheck\": true\n                },\n                {\n                    \"name\": \"Scroll\",\n                    \"type\": \"Value\",\n                    \"id\": \"90283829-0eae-46de-b689-eb3ee8c99ec6\",\n                    \"expectedControlType\": \"Axis\",\n                    \"processors\": \"\",\n                    \"interactions\": \"\",\n                    \"initialStateCheck\": true\n                },\n                {\n                    \"name\": \"UseItem\",\n                    \"type\": \"Button\",\n                    \"id\": \"950fea2a-a26e-4d20-ab16-da3e264f4722\",\n                    \"expectedControlType\": \"\",\n                    \"processors\": \"\",\n                    \"interactions\": \"\",\n                    \"initialStateCheck\": false\n                },\n                {\n                    \"name\": \"Console\",\n                    \"type\": \"Button\",\n                    \"id\": \"c57aa9eb-e564-45f7-ae36-dfeb9a973802\",\n                    \"expectedControlType\": \"\",\n                    \"processors\": \"\",\n                    \"interactions\": \"\",\n                    \"initialStateCheck\": false\n                },\n                {\n                    \"name\": \"EscapeMenu\",\n                    \"type\": \"Button\",\n                    \"id\": \"459af410-5dd2-42a7-848d-e49bc83470a8\",\n                    \"expectedControlType\": \"\",\n                    \"processors\": \"\",\n                    \"interactions\": \"\",\n                    \"initialStateCheck\": false\n                },\n                {\n                    \"name\": \"EmoteWheel\",\n                    \"type\": \"Button\",\n                    \"id\": \"6641fb91-1537-44dd-b0f7-edbd636c768d\",\n                    \"expectedControlType\": \"\",\n                    \"processors\": \"\",\n                    \"interactions\": \"\",\n                    \"initialStateCheck\": false\n                },\n                {\n                    \"name\": \"F1\",\n                    \"type\": \"Button\",\n                    \"id\": \"fbf13060-94cf-4871-88a5-89a69dd2236c\",\n                    \"expectedControlType\": \"\",\n                    \"processors\": \"\",\n                    \"interactions\": \"\",\n                    \"initialStateCheck\": false\n                },\n                {\n                    \"name\": \"F2\",\n                    \"type\": \"Button\",\n                    \"id\": \"0557d4f4-b887-43f9-b0af-a97cced3c620\",\n                    \"expectedControlType\": \"\",\n                    \"processors\": \"\",\n                    \"interactions\": \"\",\n                    \"initialStateCheck\": false\n                },\n                {\n                    \"name\": \"F3\",\n                    \"type\": \"Button\",\n                    \"id\": \"ed352d11-aaea-4478-a18e-0eb4aa08a9aa\",\n                    \"expectedControlType\": \"\",\n                    \"processors\": \"\",\n                    \"interactions\": \"\",\n                    \"initialStateCheck\": false\n                },\n                {\n                    \"name\": \"F4\",\n                    \"type\": \"Button\",\n                    \"id\": \"b745e2d5-3955-4f0d-817c-6f57ae56bf64\",\n                    \"expectedControlType\": \"\",\n                    \"processors\": \"\",\n                    \"interactions\": \"\",\n                    \"initialStateCheck\": false\n                },\n                {\n                    \"name\": \"Ping\",\n                    \"type\": \"Button\",\n                    \"id\": \"7440d5fa-26ed-45c0-870c-0784d92b3a68\",\n                    \"expectedControlType\": \"\",\n                    \"processors\": \"\",\n                    \"interactions\": \"\",\n                    \"initialStateCheck\": false\n                },\n                {\n                    \"name\": \"PushToTalk\",\n                    \"type\": \"Button\",\n                    \"id\": \"72f3d30d-af8f-4b1c-a4f7-230dc397c5df\",\n                    \"expectedControlType\": \"\",\n                    \"processors\": \"\",\n                    \"interactions\": \"\",\n                    \"initialStateCheck\": false\n                }\n            ],\n            \"bindings\": [\n                {\n                    \"name\": \"2D Vector\",\n                    \"id\": \"d687c5f2-e728-4f77-a1a9-134b479d50e4\",\n                    \"path\": \"2DVector\",\n                    \"interactions\": \"\",\n                    \"processors\": \"\",\n                    \"groups\": \"\",\n                    \"action\": \"Move\",\n                    \"isComposite\": true,\n                    \"isPartOfComposite\": false\n                },\n                {\n                    \"name\": \"up\",\n                    \"id\": \"60e42552-63b5-4f6a-a0a0-b8ffa4ed0820\",\n                    \"path\": \"<Keyboard>/w\",\n                    \"interactions\": \"\",\n                    \"processors\": \"\",\n                    \"groups\": \"\",\n                    \"action\": \"Move\",\n                    \"isComposite\": false,\n                    \"isPartOfComposite\": true\n                },\n                {\n                    \"name\": \"down\",\n                    \"id\": \"fb8ea55c-d97a-4015-bc00-891e84e6aa60\",\n                    \"path\": \"<Keyboard>/s\",\n                    \"interactions\": \"\",\n                    \"processors\": \"\",\n                    \"groups\": \"\",\n                    \"action\": \"Move\",\n                    \"isComposite\": false,\n                    \"isPartOfComposite\": true\n                },\n                {\n                    \"name\": \"left\",\n                    \"id\": \"63d69966-07f3-4e0b-afa9-264a2017d7f0\",\n                    \"path\": \"<Keyboard>/a\",\n                    \"interactions\": \"\",\n                    \"processors\": \"\",\n                    \"groups\": \"\",\n                    \"action\": \"Move\",\n                    \"isComposite\": false,\n                    \"isPartOfComposite\": true\n                },\n                {\n                    \"name\": \"right\",\n                    \"id\": \"da43350f-806c-4b28-9bd0-7951cf3bc076\",\n                    \"path\": \"<Keyboard>/d\",\n                    \"interactions\": \"\",\n                    \"processors\": \"\",\n                    \"groups\": \"\",\n                    \"action\": \"Move\",\n                    \"isComposite\": false,\n                    \"isPartOfComposite\": true\n                },\n                {\n                    \"name\": \"\",\n                    \"id\": \"1c4f0f3e-aede-4444-b598-83be157bf524\",\n                    \"path\": \"<Gamepad>/leftStick\",\n                    \"interactions\": \"\",\n                    \"processors\": \"\",\n                    \"groups\": \"\",\n                    \"action\": \"Move\",\n                    \"isComposite\": false,\n                    \"isPartOfComposite\": false\n                },\n                {\n                    \"name\": \"\",\n                    \"id\": \"aa34ec15-e4ef-43dd-81d6-0cde3ba3dbea\",\n                    \"path\": \"<Keyboard>/space\",\n                    \"interactions\": \"\",\n                    \"processors\": \"\",\n                    \"groups\": \"\",\n                    \"action\": \"Jump\",\n                    \"isComposite\": false,\n                    \"isPartOfComposite\": false\n                },\n                {\n                    \"name\": \"\",\n                    \"id\": \"456a0c4b-71a2-44a0-b3b7-2855ee6b311d\",\n                    \"path\": \"<Gamepad>/buttonSouth\",\n                    \"interactions\": \"\",\n                    \"processors\": \"\",\n                    \"groups\": \"\",\n                    \"action\": \"Jump\",\n                    \"isComposite\": false,\n                    \"isPartOfComposite\": false\n                },\n                {\n                    \"name\": \"\",\n                    \"id\": \"e5e2314c-0520-4816-b79a-1016443095f0\",\n                    \"path\": \"<Keyboard>/ctrl\",\n                    \"interactions\": \"\",\n                    \"processors\": \"\",\n                    \"groups\": \"\",\n                    \"action\": \"Crouch\",\n                    \"isComposite\": false,\n                    \"isPartOfComposite\": false\n                },\n                {\n                    \"name\": \"\",\n                    \"id\": \"05e54a9d-ddac-448f-899b-5dcb55dfabf1\",\n                    \"path\": \"<Gamepad>/buttonEast\",\n                    \"interactions\": \"\",\n                    \"processors\": \"\",\n                    \"groups\": \"\",\n                    \"action\": \"Crouch\",\n                    \"isComposite\": false,\n                    \"isPartOfComposite\": false\n                },\n                {\n                    \"name\": \"\",\n                    \"id\": \"bda22789-09e5-4d96-a611-31e79672c01e\",\n                    \"path\": \"<Keyboard>/leftShift\",\n                    \"interactions\": \"\",\n                    \"processors\": \"\",\n                    \"groups\": \"\",\n                    \"action\": \"Sprint\",\n                    \"isComposite\": false,\n                    \"isPartOfComposite\": false\n                },\n                {\n                    \"name\": \"\",\n                    \"id\": \"6ceabf96-ebf8-4fcd-8905-3c74af189df5\",\n                    \"path\": \"<Gamepad>/leftTrigger\",\n                    \"interactions\": \"\",\n                    \"processors\": \"\",\n                    \"groups\": \"\",\n                    \"action\": \"Sprint\",\n                    \"isComposite\": false,\n                    \"isPartOfComposite\": false\n                },\n                {\n                    \"name\": \"\",\n                    \"id\": \"2b364bda-011f-4862-a237-63310d5569f2\",\n                    \"path\": \"<Keyboard>/e\",\n                    \"interactions\": \"\",\n                    \"processors\": \"\",\n                    \"groups\": \"\",\n                    \"action\": \"Interact\",\n                    \"isComposite\": false,\n                    \"isPartOfComposite\": false\n                },\n                {\n                    \"name\": \"\",\n                    \"id\": \"d2a39044-781b-4207-bcf9-b55fada0f0ff\",\n                    \"path\": \"<Gamepad>/buttonWest\",\n                    \"interactions\": \"\",\n                    \"processors\": \"\",\n                    \"groups\": \"\",\n                    \"action\": \"Interact\",\n                    \"isComposite\": false,\n                    \"isPartOfComposite\": false\n                },\n                {\n                    \"name\": \"\",\n                    \"id\": \"e2fbf232-d586-41f6-9195-ebd3cc7d960f\",\n                    \"path\": \"<Keyboard>/e\",\n                    \"interactions\": \"\",\n                    \"processors\": \"\",\n                    \"groups\": \"\",\n                    \"action\": \"SkipUI\",\n                    \"isComposite\": false,\n                    \"isPartOfComposite\": false\n                },\n                {\n                    \"name\": \"\",\n                    \"id\": \"fb647880-60f3-4c7c-b0ad-42c4bbbb84f7\",\n                    \"path\": \"<Gamepad>/buttonWest\",\n                    \"interactions\": \"\",\n                    \"processors\": \"\",\n                    \"groups\": \"\",\n                    \"action\": \"SkipUI\",\n                    \"isComposite\": false,\n                    \"isPartOfComposite\": false\n                },\n                {\n                    \"name\": \"\",\n                    \"id\": \"cbee88a6-6c48-427c-84eb-493277ccbb56\",\n                    \"path\": \"<Mouse>/rightButton\",\n                    \"interactions\": \"\",\n                    \"processors\": \"\",\n                    \"groups\": \"\",\n                    \"action\": \"Zoom\",\n                    \"isComposite\": false,\n                    \"isPartOfComposite\": false\n                },\n                {\n                    \"name\": \"\",\n                    \"id\": \"c000b279-2eef-4234-9a16-4d3d6ede73a8\",\n                    \"path\": \"<Gamepad>/buttonNorth\",\n                    \"interactions\": \"\",\n                    \"processors\": \"\",\n                    \"groups\": \"\",\n                    \"action\": \"Zoom\",\n                    \"isComposite\": false,\n                    \"isPartOfComposite\": false\n                },\n                {\n                    \"name\": \"\",\n                    \"id\": \"91eaf33e-d060-4882-aa2e-52ffbf656254\",\n                    \"path\": \"<Mouse>/delta\",\n                    \"interactions\": \"\",\n                    \"processors\": \"\",\n                    \"groups\": \"\",\n                    \"action\": \"Aim\",\n                    \"isComposite\": false,\n                    \"isPartOfComposite\": false\n                },\n                {\n                    \"name\": \"2D Vector\",\n                    \"id\": \"6d074707-1344-4083-894b-45653355865a\",\n                    \"path\": \"2DVector(mode=2)\",\n                    \"interactions\": \"\",\n                    \"processors\": \"StickDeadzone(min=0.1,max=0.9)\",\n                    \"groups\": \"\",\n                    \"action\": \"Aim\",\n                    \"isComposite\": true,\n                    \"isPartOfComposite\": false\n                },\n                {\n                    \"name\": \"up\",\n                    \"id\": \"4bce01e8-3a9b-428f-ba94-0e2778b86fcc\",\n                    \"path\": \"<Gamepad>/rightStick/up\",\n                    \"interactions\": \"\",\n                    \"processors\": \"\",\n                    \"groups\": \"\",\n                    \"action\": \"Aim\",\n                    \"isComposite\": false,\n                    \"isPartOfComposite\": true\n                },\n                {\n                    \"name\": \"down\",\n                    \"id\": \"9ddb12f4-8898-48ba-9e71-0ebd423735e4\",\n                    \"path\": \"<Gamepad>/rightStick/down\",\n                    \"interactions\": \"\",\n                    \"processors\": \"\",\n                    \"groups\": \"\",\n                    \"action\": \"Aim\",\n                    \"isComposite\": false,\n                    \"isPartOfComposite\": true\n                },\n                {\n                    \"name\": \"left\",\n                    \"id\": \"d5a3c8d3-4510-4e10-893c-d07e2493b390\",\n                    \"path\": \"<Gamepad>/rightStick/left\",\n                    \"interactions\": \"\",\n                    \"processors\": \"\",\n                    \"groups\": \"\",\n                    \"action\": \"Aim\",\n                    \"isComposite\": false,\n                    \"isPartOfComposite\": true\n                },\n                {\n                    \"name\": \"right\",\n                    \"id\": \"f74febb7-02f7-4fe9-8b48-7dc08cb5cb53\",\n                    \"path\": \"<Gamepad>/rightStick/right\",\n                    \"interactions\": \"\",\n                    \"processors\": \"\",\n                    \"groups\": \"\",\n                    \"action\": \"Aim\",\n                    \"isComposite\": false,\n                    \"isPartOfComposite\": true\n                },\n                {\n                    \"name\": \"\",\n                    \"id\": \"da87e40b-bf9b-4857-a604-e4f7a7f6a164\",\n                    \"path\": \"<Keyboard>/q\",\n                    \"interactions\": \"\",\n                    \"processors\": \"\",\n                    \"groups\": \"\",\n                    \"action\": \"ThrowItem\",\n                    \"isComposite\": false,\n                    \"isPartOfComposite\": false\n                },\n                {\n                    \"name\": \"\",\n                    \"id\": \"e59e7485-7dbf-4a36-96ed-c9892809db07\",\n                    \"path\": \"<Gamepad>/rightTrigger\",\n                    \"interactions\": \"\",\n                    \"processors\": \"\",\n                    \"groups\": \"\",\n                    \"action\": \"ThrowItem\",\n                    \"isComposite\": false,\n                    \"isPartOfComposite\": false\n                },\n                {\n                    \"name\": \"\",\n                    \"id\": \"27cb19bc-fd75-4a6a-9906-608b58df8f0d\",\n                    \"path\": \"<Keyboard>/1\",\n                    \"interactions\": \"\",\n                    \"processors\": \"\",\n                    \"groups\": \"\",\n                    \"action\": \"ItemSelect\",\n                    \"isComposite\": false,\n                    \"isPartOfComposite\": false\n                },\n                {\n                    \"name\": \"\",\n                    \"id\": \"fba0e559-a13f-4969-b130-62543dc0b053\",\n                    \"path\": \"<Keyboard>/2\",\n                    \"interactions\": \"\",\n                    \"processors\": \"\",\n                    \"groups\": \"\",\n                    \"action\": \"ItemSelect\",\n                    \"isComposite\": false,\n                    \"isPartOfComposite\": false\n                },\n                {\n                    \"name\": \"\",\n                    \"id\": \"43ec85f7-51d2-4cc2-93dd-fc2bba7c1b52\",\n                    \"path\": \"<Keyboard>/3\",\n                    \"interactions\": \"\",\n                    \"processors\": \"\",\n                    \"groups\": \"\",\n                    \"action\": \"ItemSelect\",\n                    \"isComposite\": false,\n                    \"isPartOfComposite\": false\n                },\n                {\n        [...string is too long...]");
		this.m_Player = this.asset.FindActionMap("Player", true);
		this.m_Player_Move = this.m_Player.FindAction("Move", true);
		this.m_Player_Aim = this.m_Player.FindAction("Aim", true);
		this.m_Player_Jump = this.m_Player.FindAction("Jump", true);
		this.m_Player_Crouch = this.m_Player.FindAction("Crouch", true);
		this.m_Player_Sprint = this.m_Player.FindAction("Sprint", true);
		this.m_Player_Interact = this.m_Player.FindAction("Interact", true);
		this.m_Player_SkipUI = this.m_Player.FindAction("SkipUI", true);
		this.m_Player_ThrowItem = this.m_Player.FindAction("ThrowItem", true);
		this.m_Player_Zoom = this.m_Player.FindAction("Zoom", true);
		this.m_Player_ItemSelect = this.m_Player.FindAction("ItemSelect", true);
		this.m_Player_Scroll = this.m_Player.FindAction("Scroll", true);
		this.m_Player_UseItem = this.m_Player.FindAction("UseItem", true);
		this.m_Player_Console = this.m_Player.FindAction("Console", true);
		this.m_Player_EscapeMenu = this.m_Player.FindAction("EscapeMenu", true);
		this.m_Player_EmoteWheel = this.m_Player.FindAction("EmoteWheel", true);
		this.m_Player_F1 = this.m_Player.FindAction("F1", true);
		this.m_Player_F2 = this.m_Player.FindAction("F2", true);
		this.m_Player_F3 = this.m_Player.FindAction("F3", true);
		this.m_Player_F4 = this.m_Player.FindAction("F4", true);
		this.m_Player_Ping = this.m_Player.FindAction("Ping", true);
		this.m_Player_PushToTalk = this.m_Player.FindAction("PushToTalk", true);
	}

	// Token: 0x06001865 RID: 6245 RVA: 0x00067754 File Offset: 0x00065954
	~InputActions()
	{
	}

	// Token: 0x06001866 RID: 6246 RVA: 0x0006777C File Offset: 0x0006597C
	public void Dispose()
	{
		Object.Destroy(this.asset);
	}

	// Token: 0x17000230 RID: 560
	// (get) Token: 0x06001867 RID: 6247 RVA: 0x00067789 File Offset: 0x00065989
	// (set) Token: 0x06001868 RID: 6248 RVA: 0x00067796 File Offset: 0x00065996
	public InputBinding? bindingMask
	{
		get
		{
			return this.asset.bindingMask;
		}
		set
		{
			this.asset.bindingMask = value;
		}
	}

	// Token: 0x17000231 RID: 561
	// (get) Token: 0x06001869 RID: 6249 RVA: 0x000677A4 File Offset: 0x000659A4
	// (set) Token: 0x0600186A RID: 6250 RVA: 0x000677B1 File Offset: 0x000659B1
	public ReadOnlyArray<InputDevice>? devices
	{
		get
		{
			return this.asset.devices;
		}
		set
		{
			this.asset.devices = value;
		}
	}

	// Token: 0x17000232 RID: 562
	// (get) Token: 0x0600186B RID: 6251 RVA: 0x000677BF File Offset: 0x000659BF
	public ReadOnlyArray<InputControlScheme> controlSchemes
	{
		get
		{
			return this.asset.controlSchemes;
		}
	}

	// Token: 0x0600186C RID: 6252 RVA: 0x000677CC File Offset: 0x000659CC
	public bool Contains(InputAction action)
	{
		return this.asset.Contains(action);
	}

	// Token: 0x0600186D RID: 6253 RVA: 0x000677DA File Offset: 0x000659DA
	public IEnumerator<InputAction> GetEnumerator()
	{
		return this.asset.GetEnumerator();
	}

	// Token: 0x0600186E RID: 6254 RVA: 0x000677E7 File Offset: 0x000659E7
	IEnumerator IEnumerable.GetEnumerator()
	{
		return this.GetEnumerator();
	}

	// Token: 0x0600186F RID: 6255 RVA: 0x000677EF File Offset: 0x000659EF
	public void Enable()
	{
		this.asset.Enable();
	}

	// Token: 0x06001870 RID: 6256 RVA: 0x000677FC File Offset: 0x000659FC
	public void Disable()
	{
		this.asset.Disable();
	}

	// Token: 0x17000233 RID: 563
	// (get) Token: 0x06001871 RID: 6257 RVA: 0x00067809 File Offset: 0x00065A09
	public IEnumerable<InputBinding> bindings
	{
		get
		{
			return this.asset.bindings;
		}
	}

	// Token: 0x06001872 RID: 6258 RVA: 0x00067816 File Offset: 0x00065A16
	public InputAction FindAction(string actionNameOrId, bool throwIfNotFound = false)
	{
		return this.asset.FindAction(actionNameOrId, throwIfNotFound);
	}

	// Token: 0x06001873 RID: 6259 RVA: 0x00067825 File Offset: 0x00065A25
	public int FindBinding(InputBinding bindingMask, out InputAction action)
	{
		return this.asset.FindBinding(bindingMask, out action);
	}

	// Token: 0x17000234 RID: 564
	// (get) Token: 0x06001874 RID: 6260 RVA: 0x00067834 File Offset: 0x00065A34
	public InputActions.PlayerActions Player
	{
		get
		{
			return new InputActions.PlayerActions(this);
		}
	}

	// Token: 0x04000FC4 RID: 4036
	private readonly InputActionMap m_Player;

	// Token: 0x04000FC5 RID: 4037
	private List<InputActions.IPlayerActions> m_PlayerActionsCallbackInterfaces = new List<InputActions.IPlayerActions>();

	// Token: 0x04000FC6 RID: 4038
	private readonly InputAction m_Player_Move;

	// Token: 0x04000FC7 RID: 4039
	private readonly InputAction m_Player_Aim;

	// Token: 0x04000FC8 RID: 4040
	private readonly InputAction m_Player_Jump;

	// Token: 0x04000FC9 RID: 4041
	private readonly InputAction m_Player_Crouch;

	// Token: 0x04000FCA RID: 4042
	private readonly InputAction m_Player_Sprint;

	// Token: 0x04000FCB RID: 4043
	private readonly InputAction m_Player_Interact;

	// Token: 0x04000FCC RID: 4044
	private readonly InputAction m_Player_SkipUI;

	// Token: 0x04000FCD RID: 4045
	private readonly InputAction m_Player_ThrowItem;

	// Token: 0x04000FCE RID: 4046
	private readonly InputAction m_Player_Zoom;

	// Token: 0x04000FCF RID: 4047
	private readonly InputAction m_Player_ItemSelect;

	// Token: 0x04000FD0 RID: 4048
	private readonly InputAction m_Player_Scroll;

	// Token: 0x04000FD1 RID: 4049
	private readonly InputAction m_Player_UseItem;

	// Token: 0x04000FD2 RID: 4050
	private readonly InputAction m_Player_Console;

	// Token: 0x04000FD3 RID: 4051
	private readonly InputAction m_Player_EscapeMenu;

	// Token: 0x04000FD4 RID: 4052
	private readonly InputAction m_Player_EmoteWheel;

	// Token: 0x04000FD5 RID: 4053
	private readonly InputAction m_Player_F1;

	// Token: 0x04000FD6 RID: 4054
	private readonly InputAction m_Player_F2;

	// Token: 0x04000FD7 RID: 4055
	private readonly InputAction m_Player_F3;

	// Token: 0x04000FD8 RID: 4056
	private readonly InputAction m_Player_F4;

	// Token: 0x04000FD9 RID: 4057
	private readonly InputAction m_Player_Ping;

	// Token: 0x04000FDA RID: 4058
	private readonly InputAction m_Player_PushToTalk;

	// Token: 0x020002BA RID: 698
	public struct PlayerActions
	{
		// Token: 0x06001875 RID: 6261 RVA: 0x0006783C File Offset: 0x00065A3C
		public PlayerActions(InputActions wrapper)
		{
			this.m_Wrapper = wrapper;
		}

		// Token: 0x17000235 RID: 565
		// (get) Token: 0x06001876 RID: 6262 RVA: 0x00067845 File Offset: 0x00065A45
		public InputAction Move
		{
			get
			{
				return this.m_Wrapper.m_Player_Move;
			}
		}

		// Token: 0x17000236 RID: 566
		// (get) Token: 0x06001877 RID: 6263 RVA: 0x00067852 File Offset: 0x00065A52
		public InputAction Aim
		{
			get
			{
				return this.m_Wrapper.m_Player_Aim;
			}
		}

		// Token: 0x17000237 RID: 567
		// (get) Token: 0x06001878 RID: 6264 RVA: 0x0006785F File Offset: 0x00065A5F
		public InputAction Jump
		{
			get
			{
				return this.m_Wrapper.m_Player_Jump;
			}
		}

		// Token: 0x17000238 RID: 568
		// (get) Token: 0x06001879 RID: 6265 RVA: 0x0006786C File Offset: 0x00065A6C
		public InputAction Crouch
		{
			get
			{
				return this.m_Wrapper.m_Player_Crouch;
			}
		}

		// Token: 0x17000239 RID: 569
		// (get) Token: 0x0600187A RID: 6266 RVA: 0x00067879 File Offset: 0x00065A79
		public InputAction Sprint
		{
			get
			{
				return this.m_Wrapper.m_Player_Sprint;
			}
		}

		// Token: 0x1700023A RID: 570
		// (get) Token: 0x0600187B RID: 6267 RVA: 0x00067886 File Offset: 0x00065A86
		public InputAction Interact
		{
			get
			{
				return this.m_Wrapper.m_Player_Interact;
			}
		}

		// Token: 0x1700023B RID: 571
		// (get) Token: 0x0600187C RID: 6268 RVA: 0x00067893 File Offset: 0x00065A93
		public InputAction SkipUI
		{
			get
			{
				return this.m_Wrapper.m_Player_SkipUI;
			}
		}

		// Token: 0x1700023C RID: 572
		// (get) Token: 0x0600187D RID: 6269 RVA: 0x000678A0 File Offset: 0x00065AA0
		public InputAction ThrowItem
		{
			get
			{
				return this.m_Wrapper.m_Player_ThrowItem;
			}
		}

		// Token: 0x1700023D RID: 573
		// (get) Token: 0x0600187E RID: 6270 RVA: 0x000678AD File Offset: 0x00065AAD
		public InputAction Zoom
		{
			get
			{
				return this.m_Wrapper.m_Player_Zoom;
			}
		}

		// Token: 0x1700023E RID: 574
		// (get) Token: 0x0600187F RID: 6271 RVA: 0x000678BA File Offset: 0x00065ABA
		public InputAction ItemSelect
		{
			get
			{
				return this.m_Wrapper.m_Player_ItemSelect;
			}
		}

		// Token: 0x1700023F RID: 575
		// (get) Token: 0x06001880 RID: 6272 RVA: 0x000678C7 File Offset: 0x00065AC7
		public InputAction Scroll
		{
			get
			{
				return this.m_Wrapper.m_Player_Scroll;
			}
		}

		// Token: 0x17000240 RID: 576
		// (get) Token: 0x06001881 RID: 6273 RVA: 0x000678D4 File Offset: 0x00065AD4
		public InputAction UseItem
		{
			get
			{
				return this.m_Wrapper.m_Player_UseItem;
			}
		}

		// Token: 0x17000241 RID: 577
		// (get) Token: 0x06001882 RID: 6274 RVA: 0x000678E1 File Offset: 0x00065AE1
		public InputAction Console
		{
			get
			{
				return this.m_Wrapper.m_Player_Console;
			}
		}

		// Token: 0x17000242 RID: 578
		// (get) Token: 0x06001883 RID: 6275 RVA: 0x000678EE File Offset: 0x00065AEE
		public InputAction EscapeMenu
		{
			get
			{
				return this.m_Wrapper.m_Player_EscapeMenu;
			}
		}

		// Token: 0x17000243 RID: 579
		// (get) Token: 0x06001884 RID: 6276 RVA: 0x000678FB File Offset: 0x00065AFB
		public InputAction EmoteWheel
		{
			get
			{
				return this.m_Wrapper.m_Player_EmoteWheel;
			}
		}

		// Token: 0x17000244 RID: 580
		// (get) Token: 0x06001885 RID: 6277 RVA: 0x00067908 File Offset: 0x00065B08
		public InputAction F1
		{
			get
			{
				return this.m_Wrapper.m_Player_F1;
			}
		}

		// Token: 0x17000245 RID: 581
		// (get) Token: 0x06001886 RID: 6278 RVA: 0x00067915 File Offset: 0x00065B15
		public InputAction F2
		{
			get
			{
				return this.m_Wrapper.m_Player_F2;
			}
		}

		// Token: 0x17000246 RID: 582
		// (get) Token: 0x06001887 RID: 6279 RVA: 0x00067922 File Offset: 0x00065B22
		public InputAction F3
		{
			get
			{
				return this.m_Wrapper.m_Player_F3;
			}
		}

		// Token: 0x17000247 RID: 583
		// (get) Token: 0x06001888 RID: 6280 RVA: 0x0006792F File Offset: 0x00065B2F
		public InputAction F4
		{
			get
			{
				return this.m_Wrapper.m_Player_F4;
			}
		}

		// Token: 0x17000248 RID: 584
		// (get) Token: 0x06001889 RID: 6281 RVA: 0x0006793C File Offset: 0x00065B3C
		public InputAction Ping
		{
			get
			{
				return this.m_Wrapper.m_Player_Ping;
			}
		}

		// Token: 0x17000249 RID: 585
		// (get) Token: 0x0600188A RID: 6282 RVA: 0x00067949 File Offset: 0x00065B49
		public InputAction PushToTalk
		{
			get
			{
				return this.m_Wrapper.m_Player_PushToTalk;
			}
		}

		// Token: 0x0600188B RID: 6283 RVA: 0x00067956 File Offset: 0x00065B56
		public InputActionMap Get()
		{
			return this.m_Wrapper.m_Player;
		}

		// Token: 0x0600188C RID: 6284 RVA: 0x00067963 File Offset: 0x00065B63
		public void Enable()
		{
			this.Get().Enable();
		}

		// Token: 0x0600188D RID: 6285 RVA: 0x00067970 File Offset: 0x00065B70
		public void Disable()
		{
			this.Get().Disable();
		}

		// Token: 0x1700024A RID: 586
		// (get) Token: 0x0600188E RID: 6286 RVA: 0x0006797D File Offset: 0x00065B7D
		public bool enabled
		{
			get
			{
				return this.Get().enabled;
			}
		}

		// Token: 0x0600188F RID: 6287 RVA: 0x0006798A File Offset: 0x00065B8A
		public static implicit operator InputActionMap(InputActions.PlayerActions set)
		{
			return set.Get();
		}

		// Token: 0x06001890 RID: 6288 RVA: 0x00067994 File Offset: 0x00065B94
		public void AddCallbacks(InputActions.IPlayerActions instance)
		{
			if (instance == null || this.m_Wrapper.m_PlayerActionsCallbackInterfaces.Contains(instance))
			{
				return;
			}
			this.m_Wrapper.m_PlayerActionsCallbackInterfaces.Add(instance);
			this.Move.started += instance.OnMove;
			this.Move.performed += instance.OnMove;
			this.Move.canceled += instance.OnMove;
			this.Aim.started += instance.OnAim;
			this.Aim.performed += instance.OnAim;
			this.Aim.canceled += instance.OnAim;
			this.Jump.started += instance.OnJump;
			this.Jump.performed += instance.OnJump;
			this.Jump.canceled += instance.OnJump;
			this.Crouch.started += instance.OnCrouch;
			this.Crouch.performed += instance.OnCrouch;
			this.Crouch.canceled += instance.OnCrouch;
			this.Sprint.started += instance.OnSprint;
			this.Sprint.performed += instance.OnSprint;
			this.Sprint.canceled += instance.OnSprint;
			this.Interact.started += instance.OnInteract;
			this.Interact.performed += instance.OnInteract;
			this.Interact.canceled += instance.OnInteract;
			this.SkipUI.started += instance.OnSkipUI;
			this.SkipUI.performed += instance.OnSkipUI;
			this.SkipUI.canceled += instance.OnSkipUI;
			this.ThrowItem.started += instance.OnThrowItem;
			this.ThrowItem.performed += instance.OnThrowItem;
			this.ThrowItem.canceled += instance.OnThrowItem;
			this.Zoom.started += instance.OnZoom;
			this.Zoom.performed += instance.OnZoom;
			this.Zoom.canceled += instance.OnZoom;
			this.ItemSelect.started += instance.OnItemSelect;
			this.ItemSelect.performed += instance.OnItemSelect;
			this.ItemSelect.canceled += instance.OnItemSelect;
			this.Scroll.started += instance.OnScroll;
			this.Scroll.performed += instance.OnScroll;
			this.Scroll.canceled += instance.OnScroll;
			this.UseItem.started += instance.OnUseItem;
			this.UseItem.performed += instance.OnUseItem;
			this.UseItem.canceled += instance.OnUseItem;
			this.Console.started += instance.OnConsole;
			this.Console.performed += instance.OnConsole;
			this.Console.canceled += instance.OnConsole;
			this.EscapeMenu.started += instance.OnEscapeMenu;
			this.EscapeMenu.performed += instance.OnEscapeMenu;
			this.EscapeMenu.canceled += instance.OnEscapeMenu;
			this.EmoteWheel.started += instance.OnEmoteWheel;
			this.EmoteWheel.performed += instance.OnEmoteWheel;
			this.EmoteWheel.canceled += instance.OnEmoteWheel;
			this.F1.started += instance.OnF1;
			this.F1.performed += instance.OnF1;
			this.F1.canceled += instance.OnF1;
			this.F2.started += instance.OnF2;
			this.F2.performed += instance.OnF2;
			this.F2.canceled += instance.OnF2;
			this.F3.started += instance.OnF3;
			this.F3.performed += instance.OnF3;
			this.F3.canceled += instance.OnF3;
			this.F4.started += instance.OnF4;
			this.F4.performed += instance.OnF4;
			this.F4.canceled += instance.OnF4;
			this.Ping.started += instance.OnPing;
			this.Ping.performed += instance.OnPing;
			this.Ping.canceled += instance.OnPing;
			this.PushToTalk.started += instance.OnPushToTalk;
			this.PushToTalk.performed += instance.OnPushToTalk;
			this.PushToTalk.canceled += instance.OnPushToTalk;
		}

		// Token: 0x06001891 RID: 6289 RVA: 0x00067FB4 File Offset: 0x000661B4
		private void UnregisterCallbacks(InputActions.IPlayerActions instance)
		{
			this.Move.started -= instance.OnMove;
			this.Move.performed -= instance.OnMove;
			this.Move.canceled -= instance.OnMove;
			this.Aim.started -= instance.OnAim;
			this.Aim.performed -= instance.OnAim;
			this.Aim.canceled -= instance.OnAim;
			this.Jump.started -= instance.OnJump;
			this.Jump.performed -= instance.OnJump;
			this.Jump.canceled -= instance.OnJump;
			this.Crouch.started -= instance.OnCrouch;
			this.Crouch.performed -= instance.OnCrouch;
			this.Crouch.canceled -= instance.OnCrouch;
			this.Sprint.started -= instance.OnSprint;
			this.Sprint.performed -= instance.OnSprint;
			this.Sprint.canceled -= instance.OnSprint;
			this.Interact.started -= instance.OnInteract;
			this.Interact.performed -= instance.OnInteract;
			this.Interact.canceled -= instance.OnInteract;
			this.SkipUI.started -= instance.OnSkipUI;
			this.SkipUI.performed -= instance.OnSkipUI;
			this.SkipUI.canceled -= instance.OnSkipUI;
			this.ThrowItem.started -= instance.OnThrowItem;
			this.ThrowItem.performed -= instance.OnThrowItem;
			this.ThrowItem.canceled -= instance.OnThrowItem;
			this.Zoom.started -= instance.OnZoom;
			this.Zoom.performed -= instance.OnZoom;
			this.Zoom.canceled -= instance.OnZoom;
			this.ItemSelect.started -= instance.OnItemSelect;
			this.ItemSelect.performed -= instance.OnItemSelect;
			this.ItemSelect.canceled -= instance.OnItemSelect;
			this.Scroll.started -= instance.OnScroll;
			this.Scroll.performed -= instance.OnScroll;
			this.Scroll.canceled -= instance.OnScroll;
			this.UseItem.started -= instance.OnUseItem;
			this.UseItem.performed -= instance.OnUseItem;
			this.UseItem.canceled -= instance.OnUseItem;
			this.Console.started -= instance.OnConsole;
			this.Console.performed -= instance.OnConsole;
			this.Console.canceled -= instance.OnConsole;
			this.EscapeMenu.started -= instance.OnEscapeMenu;
			this.EscapeMenu.performed -= instance.OnEscapeMenu;
			this.EscapeMenu.canceled -= instance.OnEscapeMenu;
			this.EmoteWheel.started -= instance.OnEmoteWheel;
			this.EmoteWheel.performed -= instance.OnEmoteWheel;
			this.EmoteWheel.canceled -= instance.OnEmoteWheel;
			this.F1.started -= instance.OnF1;
			this.F1.performed -= instance.OnF1;
			this.F1.canceled -= instance.OnF1;
			this.F2.started -= instance.OnF2;
			this.F2.performed -= instance.OnF2;
			this.F2.canceled -= instance.OnF2;
			this.F3.started -= instance.OnF3;
			this.F3.performed -= instance.OnF3;
			this.F3.canceled -= instance.OnF3;
			this.F4.started -= instance.OnF4;
			this.F4.performed -= instance.OnF4;
			this.F4.canceled -= instance.OnF4;
			this.Ping.started -= instance.OnPing;
			this.Ping.performed -= instance.OnPing;
			this.Ping.canceled -= instance.OnPing;
			this.PushToTalk.started -= instance.OnPushToTalk;
			this.PushToTalk.performed -= instance.OnPushToTalk;
			this.PushToTalk.canceled -= instance.OnPushToTalk;
		}

		// Token: 0x06001892 RID: 6290 RVA: 0x000685A9 File Offset: 0x000667A9
		public void RemoveCallbacks(InputActions.IPlayerActions instance)
		{
			if (this.m_Wrapper.m_PlayerActionsCallbackInterfaces.Remove(instance))
			{
				this.UnregisterCallbacks(instance);
			}
		}

		// Token: 0x06001893 RID: 6291 RVA: 0x000685C8 File Offset: 0x000667C8
		public void SetCallbacks(InputActions.IPlayerActions instance)
		{
			foreach (InputActions.IPlayerActions instance2 in this.m_Wrapper.m_PlayerActionsCallbackInterfaces)
			{
				this.UnregisterCallbacks(instance2);
			}
			this.m_Wrapper.m_PlayerActionsCallbackInterfaces.Clear();
			this.AddCallbacks(instance);
		}

		// Token: 0x04000FDB RID: 4059
		private InputActions m_Wrapper;
	}

	// Token: 0x020002BB RID: 699
	public interface IPlayerActions
	{
		// Token: 0x06001894 RID: 6292
		void OnMove(InputAction.CallbackContext context);

		// Token: 0x06001895 RID: 6293
		void OnAim(InputAction.CallbackContext context);

		// Token: 0x06001896 RID: 6294
		void OnJump(InputAction.CallbackContext context);

		// Token: 0x06001897 RID: 6295
		void OnCrouch(InputAction.CallbackContext context);

		// Token: 0x06001898 RID: 6296
		void OnSprint(InputAction.CallbackContext context);

		// Token: 0x06001899 RID: 6297
		void OnInteract(InputAction.CallbackContext context);

		// Token: 0x0600189A RID: 6298
		void OnSkipUI(InputAction.CallbackContext context);

		// Token: 0x0600189B RID: 6299
		void OnThrowItem(InputAction.CallbackContext context);

		// Token: 0x0600189C RID: 6300
		void OnZoom(InputAction.CallbackContext context);

		// Token: 0x0600189D RID: 6301
		void OnItemSelect(InputAction.CallbackContext context);

		// Token: 0x0600189E RID: 6302
		void OnScroll(InputAction.CallbackContext context);

		// Token: 0x0600189F RID: 6303
		void OnUseItem(InputAction.CallbackContext context);

		// Token: 0x060018A0 RID: 6304
		void OnConsole(InputAction.CallbackContext context);

		// Token: 0x060018A1 RID: 6305
		void OnEscapeMenu(InputAction.CallbackContext context);

		// Token: 0x060018A2 RID: 6306
		void OnEmoteWheel(InputAction.CallbackContext context);

		// Token: 0x060018A3 RID: 6307
		void OnF1(InputAction.CallbackContext context);

		// Token: 0x060018A4 RID: 6308
		void OnF2(InputAction.CallbackContext context);

		// Token: 0x060018A5 RID: 6309
		void OnF3(InputAction.CallbackContext context);

		// Token: 0x060018A6 RID: 6310
		void OnF4(InputAction.CallbackContext context);

		// Token: 0x060018A7 RID: 6311
		void OnPing(InputAction.CallbackContext context);

		// Token: 0x060018A8 RID: 6312
		void OnPushToTalk(InputAction.CallbackContext context);
	}
}
