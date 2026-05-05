using System;
using System.Collections.Generic;
using Mirror;
using Mirror.RemoteCalls;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Smooth
{
	// Token: 0x0200035F RID: 863
	public class SmoothSyncMirror : NetworkBehaviour
	{
		// Token: 0x17000293 RID: 659
		// (get) Token: 0x06001C6D RID: 7277 RVA: 0x0007900D File Offset: 0x0007720D
		// (set) Token: 0x06001C6E RID: 7278 RVA: 0x00079015 File Offset: 0x00077215
		public float localTime { get; private set; }

		// Token: 0x06001C6F RID: 7279 RVA: 0x00002321 File Offset: 0x00000521
		public static bool validateState(StateMirror latestReceivedState, StateMirror latestValidatedState)
		{
			return true;
		}

		// Token: 0x17000294 RID: 660
		// (get) Token: 0x06001C70 RID: 7280 RVA: 0x0007901E File Offset: 0x0007721E
		public new NetworkIdentity netIdentity
		{
			get
			{
				if (!this.hasCachedNetID)
				{
					this.cachedNetIdentity = base.GetComponent<NetworkIdentity>();
					this.hasCachedNetID = true;
				}
				return this.cachedNetIdentity;
			}
		}

		// Token: 0x17000295 RID: 661
		// (get) Token: 0x06001C71 RID: 7281 RVA: 0x00079041 File Offset: 0x00077241
		public bool hasAuthorityOrUnownedOnServer
		{
			get
			{
				return this.netIdentity.isOwned || (NetworkServer.active && this.netIdentity.connectionToClient == null);
			}
		}

		// Token: 0x17000296 RID: 662
		// (get) Token: 0x06001C72 RID: 7282 RVA: 0x00079069 File Offset: 0x00077269
		public bool hasControl
		{
			get
			{
				return (this.transformSource == SmoothSyncMirror.TransformSource.Owner && this.hasAuthorityOrUnownedOnServer) || (this.transformSource == SmoothSyncMirror.TransformSource.Server && NetworkServer.active);
			}
		}

		// Token: 0x06001C73 RID: 7283 RVA: 0x00079090 File Offset: 0x00077290
		public void Awake()
		{
			int a = ((int)(this.sendRate * this.interpolationBackTime) + 1) * 2;
			this.stateBuffer = new StateMirror[Mathf.Max(a, 30)];
			this.SetObjectToSync(this.childObjectToSync);
			if (this.extrapolationMode == SmoothSyncMirror.ExtrapolationMode.Unlimited)
			{
				this.useExtrapolationDistanceLimit = false;
				this.useExtrapolationTimeLimit = false;
			}
			this.targetTempState = new StateMirror();
			this.sendingTempState = default(NetworkStateMirror);
			this.sendingTempState.state = new StateMirror();
			NetworkIdentity.clientAuthorityCallback += new NetworkIdentity.ClientAuthorityCallback(this.AssignAuthorityCallback);
		}

		// Token: 0x06001C74 RID: 7284 RVA: 0x0007911E File Offset: 0x0007731E
		public void OnDestroy()
		{
			NetworkIdentity.clientAuthorityCallback -= new NetworkIdentity.ClientAuthorityCallback(this.AssignAuthorityCallback);
		}

		// Token: 0x06001C75 RID: 7285 RVA: 0x00079134 File Offset: 0x00077334
		public void SetObjectToSync(GameObject childObjectToSync)
		{
			this.childObjectToSync = childObjectToSync;
			if (childObjectToSync)
			{
				this.realObjectToSync = childObjectToSync;
				this.isSyncingChild = true;
				bool flag = false;
				this.childObjectSmoothSyncs = base.GetComponents<SmoothSyncMirror>();
				for (int i = 0; i < this.childObjectSmoothSyncs.Length; i++)
				{
					if (!this.childObjectSmoothSyncs[i].childObjectToSync)
					{
						flag = true;
					}
				}
				if (!flag)
				{
					Debug.LogError("You must have one SmoothSyncMirror script with unassigned childObjectToSync in order to sync the parent object");
				}
			}
			else
			{
				this.realObjectToSync = base.gameObject;
				this.childObjectSmoothSyncs = base.GetComponents<SmoothSyncMirror>();
				int num = 0;
				while (num < this.childObjectSmoothSyncs.Length && !(this.childObjectSmoothSyncs[num] == this))
				{
					if (this.childObjectSmoothSyncs[num].childObjectToSync == null)
					{
						string str = "More than one SmoothSync instance with no childObjectToSync on ";
						GameObject gameObject = base.gameObject;
						Debug.LogWarning(str + ((gameObject != null) ? gameObject.ToString() : null) + ". Disabling all but one.");
						base.enabled = false;
						return;
					}
					num++;
				}
				int num2 = 0;
				for (int j = 0; j < this.childObjectSmoothSyncs.Length; j++)
				{
					this.childObjectSmoothSyncs[j].syncIndex = num2;
					num2++;
				}
			}
			this.netID = base.GetComponent<NetworkIdentity>();
			this.rb = this.realObjectToSync.GetComponent<Rigidbody>();
			this.rb2D = this.realObjectToSync.GetComponent<Rigidbody2D>();
			if (this.rb)
			{
				this.hasRigidbody = true;
			}
			else if (this.rb2D)
			{
				this.hasRigidbody2D = true;
				if (this.syncVelocity != SyncMode.NONE)
				{
					this.syncVelocity = SyncMode.XY;
				}
				if (this.syncAngularVelocity != SyncMode.NONE)
				{
					this.syncAngularVelocity = SyncMode.Z;
				}
			}
			if (!this.rb && !this.rb2D)
			{
				this.syncVelocity = SyncMode.NONE;
				this.syncAngularVelocity = SyncMode.NONE;
			}
		}

		// Token: 0x06001C76 RID: 7286 RVA: 0x000792F4 File Offset: 0x000774F4
		private void Update()
		{
			if (this.whenToUpdateTransform == SmoothSyncMirror.WhenToUpdateTransform.Update)
			{
				this.SmoothSyncUpdate();
			}
			if (this.isSmoothingAuthorityChanges)
			{
				this.authorityChangeUpdate();
			}
		}

		// Token: 0x06001C77 RID: 7287 RVA: 0x00079312 File Offset: 0x00077512
		private void FixedUpdate()
		{
			if (this.whenToUpdateTransform == SmoothSyncMirror.WhenToUpdateTransform.FixedUpdate)
			{
				this.SmoothSyncUpdate();
			}
			this.sendState();
			this.positionLastFrame = this.getPosition();
			this.rotationLastFrame = this.getRotation();
			this.resetFlags();
		}

		// Token: 0x06001C78 RID: 7288 RVA: 0x00079348 File Offset: 0x00077548
		private void SmoothSyncUpdate()
		{
			this.localTime += Time.deltaTime;
			if (this.automaticallyResetTime && this.localTime > this.maxLocalTime)
			{
				this.ResetLocalTime();
			}
			if (!this.hasControl)
			{
				this.adjustOwnerTime();
				this.applyInterpolationOrExtrapolation();
			}
		}

		// Token: 0x06001C79 RID: 7289 RVA: 0x00079397 File Offset: 0x00077597
		public void OnEnable()
		{
			SceneManager.sceneLoaded += this.OnSceneLoaded;
			if (!NetworkServer.active)
			{
				this.registerClientHandlers();
			}
			this.clearBuffer();
		}

		// Token: 0x06001C7A RID: 7290 RVA: 0x000793BD File Offset: 0x000775BD
		public void OnDisable()
		{
			SceneManager.sceneLoaded -= this.OnSceneLoaded;
		}

		// Token: 0x06001C7B RID: 7291 RVA: 0x000793D0 File Offset: 0x000775D0
		public void OnSceneLoaded(Scene scene, LoadSceneMode mode)
		{
			if (this.automaticallyResetTime)
			{
				this.ResetLocalTime();
			}
		}

		// Token: 0x06001C7C RID: 7292 RVA: 0x000793E0 File Offset: 0x000775E0
		public override void OnStartAuthority()
		{
			base.OnStartAuthority();
			this.teleportOwnedObjectFromOwner();
		}

		// Token: 0x06001C7D RID: 7293 RVA: 0x000793F0 File Offset: 0x000775F0
		public void ResetLocalTime()
		{
			this.localTimeResetIndicator++;
			if (this.localTimeResetIndicator >= 128)
			{
				this.localTimeResetIndicator = 0;
			}
			this.lastTimeStateWasSent -= this.localTime;
			this.lastTimeOwnerTimeWasSet -= this.localTime;
			this.latestAuthorityChangeZeroTime -= this.localTime;
			for (int i = 0; i < this.stateCount; i++)
			{
				this.stateBuffer[i].receivedTimestamp -= this.localTime;
			}
			this.localTime = 0f;
			this.forceStateSendNextFixedUpdate();
		}

		// Token: 0x06001C7E RID: 7294 RVA: 0x00079494 File Offset: 0x00077694
		public void OnRemoteTimeReset()
		{
			this.approximateNetworkTimeOnOwner -= this.maxLocalTime;
			this.targetTempState.ownerTimestamp -= this.maxLocalTime;
			for (int i = this.stateCount - 1; i >= 0; i--)
			{
				this.stateBuffer[i].ownerTimestamp -= this.maxLocalTime;
			}
		}

		// Token: 0x06001C7F RID: 7295 RVA: 0x000794FC File Offset: 0x000776FC
		private void sendState()
		{
			if (NetworkServer.active)
			{
				if (this.netIdentity.observers == null || this.netIdentity.observers.Count == 0)
				{
					return;
				}
				if (this.netIdentity.observers.Count == 1 && NetworkServer.localConnection != null && this.netIdentity.observers.ContainsKey(NetworkServer.localConnection.connectionId))
				{
					return;
				}
			}
			if (!this.hasControl || (!NetworkServer.active && !NetworkClient.ready) || this.sendRate == 0f)
			{
				return;
			}
			if (this.syncPosition != SyncMode.NONE)
			{
				if (this.positionLastFrame == this.getPosition())
				{
					if (this.restStatePosition != SmoothSyncMirror.RestState.AT_REST)
					{
						this.samePositionCount++;
					}
					if (this.samePositionCount == this.atRestThresholdCount)
					{
						this.samePositionCount = 0;
						this.restStatePosition = SmoothSyncMirror.RestState.AT_REST;
						this.forceStateSendNextFixedUpdate();
					}
				}
				else if (this.restStatePosition == SmoothSyncMirror.RestState.AT_REST && this.getPosition() != this.latestTeleportedFromPosition)
				{
					this.restStatePosition = SmoothSyncMirror.RestState.JUST_STARTED_MOVING;
					this.forceStateSendNextFixedUpdate();
				}
				else if (this.restStatePosition == SmoothSyncMirror.RestState.JUST_STARTED_MOVING)
				{
					this.restStatePosition = SmoothSyncMirror.RestState.MOVING;
				}
				else
				{
					this.samePositionCount = 0;
				}
			}
			else
			{
				this.restStatePosition = SmoothSyncMirror.RestState.AT_REST;
			}
			if (this.syncRotation != SyncMode.NONE)
			{
				if (this.rotationLastFrame == this.getRotation())
				{
					if (this.restStateRotation != SmoothSyncMirror.RestState.AT_REST)
					{
						this.sameRotationCount++;
					}
					if (this.sameRotationCount == this.atRestThresholdCount)
					{
						this.sameRotationCount = 0;
						this.restStateRotation = SmoothSyncMirror.RestState.AT_REST;
						this.forceStateSendNextFixedUpdate();
					}
				}
				else if (this.restStateRotation == SmoothSyncMirror.RestState.AT_REST && this.getRotation() != this.latestTeleportedFromRotation)
				{
					this.restStateRotation = SmoothSyncMirror.RestState.JUST_STARTED_MOVING;
					this.forceStateSendNextFixedUpdate();
				}
				else if (this.restStateRotation == SmoothSyncMirror.RestState.JUST_STARTED_MOVING)
				{
					this.restStateRotation = SmoothSyncMirror.RestState.MOVING;
				}
				else
				{
					this.sameRotationCount = 0;
				}
			}
			else
			{
				this.restStateRotation = SmoothSyncMirror.RestState.AT_REST;
			}
			if (this.localTime - this.lastTimeStateWasSent < this.GetNetworkSendInterval() && !this.forceStateSend)
			{
				return;
			}
			this.sendPosition = this.shouldSendPosition();
			this.sendRotation = this.shouldSendRotation();
			this.sendScale = this.shouldSendScale();
			this.sendVelocity = this.shouldSendVelocity();
			this.sendAngularVelocity = this.shouldSendAngularVelocity();
			if (!this.sendPosition && !this.sendRotation && !this.sendScale && !this.sendVelocity && !this.sendAngularVelocity)
			{
				return;
			}
			this.sendingTempState.copyFromSmoothSync(this);
			if (this.restStatePosition == SmoothSyncMirror.RestState.AT_REST)
			{
				this.sendAtPositionalRestMessage = true;
			}
			if (this.restStateRotation == SmoothSyncMirror.RestState.AT_REST)
			{
				this.sendAtRotationalRestMessage = true;
			}
			if (this.restStatePosition == SmoothSyncMirror.RestState.JUST_STARTED_MOVING)
			{
				this.sendingTempState.state.position = this.lastPositionWhenStateWasSent;
			}
			if (this.restStateRotation == SmoothSyncMirror.RestState.JUST_STARTED_MOVING)
			{
				this.sendingTempState.state.rotation = this.lastRotationWhenStateWasSent;
			}
			if (this.restStatePosition == SmoothSyncMirror.RestState.JUST_STARTED_MOVING || this.restStateRotation == SmoothSyncMirror.RestState.JUST_STARTED_MOVING)
			{
				this.sendingTempState.state.ownerTimestamp = this.localTime - Time.deltaTime;
				if (this.restStatePosition != SmoothSyncMirror.RestState.JUST_STARTED_MOVING)
				{
					this.sendingTempState.state.position = this.positionLastFrame;
				}
				if (this.restStateRotation != SmoothSyncMirror.RestState.JUST_STARTED_MOVING)
				{
					this.sendingTempState.state.rotation = this.rotationLastFrame;
				}
			}
			this.lastTimeStateWasSent = this.localTime;
			if (NetworkServer.active)
			{
				this.SendStateToNonOwners(this.sendingTempState);
				if (this.sendPosition)
				{
					this.lastPositionWhenStateWasSent = this.sendingTempState.state.position;
				}
				if (this.sendRotation)
				{
					this.lastRotationWhenStateWasSent = this.sendingTempState.state.rotation;
				}
				if (this.sendScale)
				{
					this.lastScaleWhenStateWasSent = this.sendingTempState.state.scale;
				}
				if (this.sendVelocity)
				{
					this.lastVelocityWhenStateWasSent = this.sendingTempState.state.velocity;
				}
				if (this.sendAngularVelocity)
				{
					this.lastAngularVelocityWhenStateWasSent = this.sendingTempState.state.angularVelocity;
					return;
				}
			}
			else if (NetworkClient.active)
			{
				NetworkClient.Send<NetworkStateMirror>(this.sendingTempState, this.networkChannel);
			}
		}

		// Token: 0x06001C80 RID: 7296 RVA: 0x00079900 File Offset: 0x00077B00
		private void authorityChangeUpdate()
		{
			if (this.hasAuthorityOrUnownedOnServer && !this.hadAuthorityLastFrame && this.stateBuffer[0] != null)
			{
				if (this.hasRigidbody)
				{
					this.rb.linearVelocity = this.stateBuffer[0].velocity;
					this.rb.angularVelocity = this.stateBuffer[0].angularVelocity * 0.017453292f;
				}
				else if (this.hasRigidbody2D)
				{
					this.rb2D.linearVelocity = this.stateBuffer[0].velocity;
					this.rb2D.angularVelocity = this.stateBuffer[0].angularVelocity.z * 0.017453292f;
				}
				this.clearBuffer();
			}
			this.hadAuthorityLastFrame = this.hasAuthorityOrUnownedOnServer;
		}

		// Token: 0x06001C81 RID: 7297 RVA: 0x000799D0 File Offset: 0x00077BD0
		private void applyInterpolationOrExtrapolation()
		{
			if (this.stateCount == 0)
			{
				return;
			}
			if (!this.extrapolatedLastFrame)
			{
				this.targetTempState.resetTheVariables();
			}
			this.triedToExtrapolateTooFar = false;
			float num = this.approximateNetworkTimeOnOwner - this.interpolationBackTime;
			if (this.stateCount > 1 && this.stateBuffer[0].ownerTimestamp > num)
			{
				this.interpolate(num);
				this.extrapolatedLastFrame = false;
			}
			else if (this.stateBuffer[0].atPositionalRest && this.stateBuffer[0].atRotationalRest)
			{
				this.targetTempState.copyFromState(this.stateBuffer[0]);
				this.extrapolatedLastFrame = false;
				if (this.setVelocityInsteadOfPositionOnNonOwners)
				{
					this.triedToExtrapolateTooFar = true;
				}
			}
			else
			{
				if ((!this.isSmoothingAuthorityChanges || this.localTime - this.latestAuthorityChangeZeroTime <= this.interpolationBackTime * 2f) && this.isSmoothingAuthorityChanges)
				{
					return;
				}
				bool flag = this.extrapolate(num);
				this.extrapolatedLastFrame = true;
				this.triedToExtrapolateTooFar = !flag;
				if (this.setVelocityInsteadOfPositionOnNonOwners)
				{
					float d = num - this.stateBuffer[0].ownerTimestamp;
					this.targetTempState.velocity = this.stateBuffer[0].velocity;
					this.targetTempState.position = this.stateBuffer[0].position + this.targetTempState.velocity * d;
					Vector3 b = base.transform.position + this.targetTempState.velocity * Time.deltaTime;
					float t = (this.targetTempState.position - b).sqrMagnitude / (this.maxPositionDifferenceForVelocitySyncing * this.maxPositionDifferenceForVelocitySyncing);
					this.targetTempState.velocity = Vector3.Lerp(this.targetTempState.velocity, (this.targetTempState.position - base.transform.position) / Time.deltaTime, t);
				}
			}
			float t2 = this.positionLerpSpeed;
			float t3 = this.rotationLerpSpeed;
			float t4 = this.scaleLerpSpeed;
			bool flag2 = false;
			bool isTeleporting = false;
			if (this.dontEasePosition)
			{
				t2 = 1f;
				flag2 = true;
				this.dontEasePosition = false;
			}
			if (this.dontEaseRotation)
			{
				t3 = 1f;
				isTeleporting = true;
				this.dontEaseRotation = false;
			}
			if (this.dontEaseScale)
			{
				t4 = 1f;
				this.dontEaseScale = false;
			}
			if (!this.triedToExtrapolateTooFar)
			{
				bool flag3 = false;
				float num2 = 0f;
				if (this.getPosition() != this.targetTempState.position && this.receivedPositionThreshold != 0f)
				{
					num2 = Vector3.Distance(this.getPosition(), this.targetTempState.position);
				}
				if (this.receivedPositionThreshold != 0f)
				{
					if (num2 > this.receivedPositionThreshold)
					{
						flag3 = true;
					}
				}
				else
				{
					flag3 = true;
				}
				bool flag4 = false;
				float num3 = 0f;
				if (this.getRotation() != this.targetTempState.rotation && this.receivedRotationThreshold != 0f)
				{
					num3 = Quaternion.Angle(this.getRotation(), this.targetTempState.rotation);
				}
				if (this.receivedRotationThreshold != 0f)
				{
					if (num3 > this.receivedRotationThreshold)
					{
						flag4 = true;
					}
				}
				else
				{
					flag4 = true;
				}
				bool flag5 = false;
				if (this.getScale() != this.targetTempState.scale)
				{
					flag5 = true;
				}
				if (this.syncPosition != SyncMode.NONE && flag3)
				{
					Vector3 position = this.getPosition();
					if (this.isSyncingXPosition)
					{
						position.x = this.targetTempState.position.x;
					}
					if (this.isSyncingYPosition)
					{
						position.y = this.targetTempState.position.y;
					}
					if (this.isSyncingZPosition)
					{
						position.z = this.targetTempState.position.z;
					}
					if (this.setVelocityInsteadOfPositionOnNonOwners && !flag2)
					{
						if (this.hasRigidbody)
						{
							this.rb.linearVelocity = this.targetTempState.velocity;
						}
						if (this.hasRigidbody2D)
						{
							this.rb2D.linearVelocity = this.targetTempState.velocity;
						}
					}
					else
					{
						this.setPosition(Vector3.Lerp(this.getPosition(), position, t2), flag2);
					}
				}
				if (this.syncRotation != SyncMode.NONE && flag4)
				{
					Vector3 eulerAngles = this.getRotation().eulerAngles;
					if (this.isSyncingXRotation)
					{
						eulerAngles.x = this.targetTempState.rotation.eulerAngles.x;
					}
					if (this.isSyncingYRotation)
					{
						eulerAngles.y = this.targetTempState.rotation.eulerAngles.y;
					}
					if (this.isSyncingZRotation)
					{
						eulerAngles.z = this.targetTempState.rotation.eulerAngles.z;
					}
					Quaternion b2 = Quaternion.Euler(eulerAngles);
					this.setRotation(Quaternion.Lerp(this.getRotation(), b2, t3), isTeleporting);
				}
				if (this.syncScale != SyncMode.NONE && flag5)
				{
					Vector3 scale = this.getScale();
					if (this.isSyncingXScale)
					{
						scale.x = this.targetTempState.scale.x;
					}
					if (this.isSyncingYScale)
					{
						scale.y = this.targetTempState.scale.y;
					}
					if (this.isSyncingZScale)
					{
						scale.z = this.targetTempState.scale.z;
					}
					this.setScale(Vector3.Lerp(this.getScale(), scale, t4));
					return;
				}
			}
			else if (this.triedToExtrapolateTooFar)
			{
				if (this.hasRigidbody)
				{
					this.rb.linearVelocity = Vector3.zero;
					this.rb.angularVelocity = Vector3.zero;
				}
				if (this.hasRigidbody2D)
				{
					this.rb2D.linearVelocity = Vector2.zero;
					this.rb2D.angularVelocity = 0f;
				}
			}
		}

		// Token: 0x06001C82 RID: 7298 RVA: 0x00079F94 File Offset: 0x00078194
		private void interpolate(float interpolationTime)
		{
			int num = 0;
			while (num < this.stateCount && this.stateBuffer[num].ownerTimestamp > interpolationTime)
			{
				num++;
			}
			if (num == this.stateCount)
			{
				num--;
			}
			StateMirror stateMirror = this.stateBuffer[Mathf.Max(num - 1, 0)];
			StateMirror stateMirror2 = this.stateBuffer[num];
			float t = (interpolationTime - stateMirror2.ownerTimestamp) / (stateMirror.ownerTimestamp - stateMirror2.ownerTimestamp);
			this.shouldTeleport(stateMirror2, ref stateMirror, interpolationTime, ref t);
			this.targetTempState = StateMirror.Lerp(this.targetTempState, stateMirror2, stateMirror, t);
			if (this.snapPositionThreshold != 0f)
			{
				if ((stateMirror.position - stateMirror2.position).magnitude > this.snapPositionThreshold)
				{
					this.targetTempState.position = stateMirror.position;
				}
				this.dontEasePosition = true;
			}
			if (this.snapScaleThreshold != 0f)
			{
				if ((stateMirror.scale - stateMirror2.scale).magnitude > this.snapScaleThreshold)
				{
					this.targetTempState.scale = stateMirror.scale;
				}
				this.dontEaseScale = true;
			}
			if (this.snapRotationThreshold != 0f)
			{
				if (Quaternion.Angle(stateMirror.rotation, stateMirror2.rotation) > this.snapRotationThreshold)
				{
					this.targetTempState.rotation = stateMirror.rotation;
				}
				this.dontEaseRotation = true;
			}
			if (this.setVelocityInsteadOfPositionOnNonOwners)
			{
				Vector3 b = base.transform.position + this.targetTempState.velocity * Time.deltaTime;
				float t2 = (this.targetTempState.position - b).sqrMagnitude / (this.maxPositionDifferenceForVelocitySyncing * this.maxPositionDifferenceForVelocitySyncing);
				this.targetTempState.velocity = Vector3.Lerp(this.targetTempState.velocity, (this.targetTempState.position - base.transform.position) / Time.deltaTime, t2);
			}
		}

		// Token: 0x06001C83 RID: 7299 RVA: 0x0007A18C File Offset: 0x0007838C
		private bool extrapolate(float interpolationTime)
		{
			if (!this.extrapolatedLastFrame || this.targetTempState.ownerTimestamp < this.stateBuffer[0].ownerTimestamp)
			{
				this.targetTempState.copyFromState(this.stateBuffer[0]);
				this.timeSpentExtrapolating = 0f;
			}
			if (this.extrapolationMode != SmoothSyncMirror.ExtrapolationMode.None && this.stateCount >= 2)
			{
				if (this.syncVelocity == SyncMode.NONE && !this.stateBuffer[0].atPositionalRest)
				{
					bool flag = false;
					for (int i = 1; i < this.stateCount; i++)
					{
						if (this.stateBuffer[0].ownerTimestamp != this.stateBuffer[i].ownerTimestamp)
						{
							this.targetTempState.velocity = (this.stateBuffer[0].position - this.stateBuffer[i].position) / (this.stateBuffer[0].ownerTimestamp - this.stateBuffer[i].ownerTimestamp);
							flag = true;
							break;
						}
					}
					if (!flag)
					{
						this.targetTempState.velocity = Vector3.zero;
					}
				}
				if (this.syncAngularVelocity == SyncMode.NONE && !this.stateBuffer[0].atRotationalRest)
				{
					bool flag2 = false;
					for (int j = 1; j < this.stateCount; j++)
					{
						if (this.stateBuffer[0].ownerTimestamp != this.stateBuffer[j].ownerTimestamp)
						{
							Quaternion quaternion = this.stateBuffer[0].rotation * Quaternion.Inverse(this.stateBuffer[j].rotation);
							Vector3 angularVelocity = new Vector3(Mathf.DeltaAngle(0f, quaternion.eulerAngles.x), Mathf.DeltaAngle(0f, quaternion.eulerAngles.y), Mathf.DeltaAngle(0f, quaternion.eulerAngles.z)) / (this.stateBuffer[0].ownerTimestamp - this.stateBuffer[j].ownerTimestamp);
							this.targetTempState.angularVelocity = angularVelocity;
							flag2 = true;
							break;
						}
					}
					if (!flag2)
					{
						this.targetTempState.angularVelocity = Vector3.zero;
					}
				}
			}
			if (this.extrapolationMode == SmoothSyncMirror.ExtrapolationMode.None)
			{
				return false;
			}
			if (this.useExtrapolationTimeLimit && this.timeSpentExtrapolating > this.extrapolationTimeLimit)
			{
				return false;
			}
			bool flag3 = Mathf.Abs(this.targetTempState.velocity.x) >= 0.01f || Mathf.Abs(this.targetTempState.velocity.y) >= 0.01f || Mathf.Abs(this.targetTempState.velocity.z) >= 0.01f;
			bool flag4 = Mathf.Abs(this.targetTempState.angularVelocity.x) >= 0.01f || Mathf.Abs(this.targetTempState.angularVelocity.y) >= 0.01f || Mathf.Abs(this.targetTempState.angularVelocity.z) >= 0.01f;
			if (!flag3 && !flag4)
			{
				return false;
			}
			float num;
			if (this.timeSpentExtrapolating == 0f)
			{
				num = interpolationTime - this.targetTempState.ownerTimestamp;
			}
			else
			{
				num = Time.deltaTime;
			}
			this.timeSpentExtrapolating += num;
			if (flag3)
			{
				if (!this.rb)
				{
					this.targetTempState.position += this.targetTempState.velocity * num;
				}
				if (Mathf.Abs(this.targetTempState.velocity.y) >= 0.01f)
				{
					if (this.hasRigidbody && this.rb.useGravity)
					{
						this.targetTempState.velocity += Physics.gravity * num;
					}
					else if (this.hasRigidbody2D)
					{
						this.targetTempState.velocity += Physics.gravity * this.rb2D.gravityScale * num;
					}
				}
				if (this.hasRigidbody)
				{
					this.targetTempState.velocity -= this.targetTempState.velocity * num * this.rb.linearDamping;
				}
				else if (this.hasRigidbody2D)
				{
					this.targetTempState.velocity -= this.targetTempState.velocity * num * this.rb2D.linearDamping;
				}
			}
			if (flag4)
			{
				Quaternion lhs = Quaternion.AngleAxis(num * this.targetTempState.angularVelocity.magnitude, this.targetTempState.angularVelocity);
				this.targetTempState.rotation = lhs * this.targetTempState.rotation;
				float num2 = 0f;
				if (this.hasRigidbody)
				{
					num2 = this.rb.angularDamping;
				}
				if (this.hasRigidbody2D)
				{
					num2 = this.rb2D.angularDamping;
				}
				if ((this.hasRigidbody || this.hasRigidbody2D) && num2 > 0f)
				{
					this.targetTempState.angularVelocity -= this.targetTempState.angularVelocity * num * num2;
				}
			}
			return !this.useExtrapolationDistanceLimit || Vector3.Distance(this.stateBuffer[0].position, this.targetTempState.position) < this.extrapolationDistanceLimit;
		}

		// Token: 0x06001C84 RID: 7300 RVA: 0x0007A708 File Offset: 0x00078908
		private void shouldTeleport(StateMirror start, ref StateMirror end, float interpolationTime, ref float t)
		{
			if (start.ownerTimestamp > interpolationTime && start.teleport && this.stateCount == 2)
			{
				end = start;
				t = 1f;
				this.stopEasing();
			}
			for (int i = 0; i < this.stateCount; i++)
			{
				if (this.stateBuffer[i] == this.latestEndStateUsed && this.latestEndStateUsed != end && this.latestEndStateUsed != start)
				{
					for (int j = i - 1; j >= 0; j--)
					{
						if (this.stateBuffer[j].teleport)
						{
							t = 1f;
							this.stopEasing();
						}
						if (this.stateBuffer[j] == start)
						{
							break;
						}
					}
					break;
				}
			}
			this.latestEndStateUsed = end;
			if (end.teleport)
			{
				t = 1f;
				this.stopEasing();
			}
		}

		// Token: 0x06001C85 RID: 7301 RVA: 0x0007A7CB File Offset: 0x000789CB
		public Vector3 getPosition()
		{
			if (this.isSyncingChild || this.useLocalTransformOnly)
			{
				return this.realObjectToSync.transform.localPosition;
			}
			return this.realObjectToSync.transform.position;
		}

		// Token: 0x06001C86 RID: 7302 RVA: 0x0007A7FE File Offset: 0x000789FE
		public Quaternion getRotation()
		{
			if (this.isSyncingChild || this.useLocalTransformOnly)
			{
				return this.realObjectToSync.transform.localRotation;
			}
			return this.realObjectToSync.transform.rotation;
		}

		// Token: 0x06001C87 RID: 7303 RVA: 0x0007A831 File Offset: 0x00078A31
		public Vector3 getScale()
		{
			return this.realObjectToSync.transform.localScale;
		}

		// Token: 0x06001C88 RID: 7304 RVA: 0x0007A844 File Offset: 0x00078A44
		public void setPosition(Vector3 position, bool isTeleporting)
		{
			if (position.x == float.NaN || position.y == float.NaN || position.z == float.NaN)
			{
				return;
			}
			if (float.IsInfinity(position.x) || float.IsInfinity(position.y) || float.IsInfinity(position.z))
			{
				return;
			}
			if (this.isSyncingChild || this.useLocalTransformOnly)
			{
				this.realObjectToSync.transform.localPosition = position;
				return;
			}
			if (this.hasRigidbody && !isTeleporting && this.whenToUpdateTransform == SmoothSyncMirror.WhenToUpdateTransform.FixedUpdate)
			{
				this.rb.MovePosition(position);
				return;
			}
			if (this.hasRigidbody2D && !isTeleporting && this.whenToUpdateTransform == SmoothSyncMirror.WhenToUpdateTransform.FixedUpdate)
			{
				this.rb2D.MovePosition(position);
				return;
			}
			this.realObjectToSync.transform.position = position;
		}

		// Token: 0x06001C89 RID: 7305 RVA: 0x0007A91C File Offset: 0x00078B1C
		public void setRotation(Quaternion rotation, bool isTeleporting)
		{
			if (rotation.x == float.NaN || rotation.y == float.NaN || rotation.z == float.NaN || rotation.w == float.NaN)
			{
				return;
			}
			if (float.IsInfinity(rotation.x) || float.IsInfinity(rotation.y) || float.IsInfinity(rotation.z) || float.IsInfinity(rotation.w))
			{
				return;
			}
			if (this.isSyncingChild || this.useLocalTransformOnly)
			{
				this.realObjectToSync.transform.localRotation = rotation;
				return;
			}
			if (this.hasRigidbody && !isTeleporting && this.whenToUpdateTransform == SmoothSyncMirror.WhenToUpdateTransform.FixedUpdate)
			{
				this.rb.MoveRotation(rotation);
				return;
			}
			if (this.hasRigidbody2D && !isTeleporting && this.whenToUpdateTransform == SmoothSyncMirror.WhenToUpdateTransform.FixedUpdate)
			{
				this.rb2D.MoveRotation(rotation.eulerAngles.z);
				return;
			}
			this.realObjectToSync.transform.rotation = rotation;
		}

		// Token: 0x06001C8A RID: 7306 RVA: 0x0007AA13 File Offset: 0x00078C13
		public void setScale(Vector3 scale)
		{
			this.realObjectToSync.transform.localScale = scale;
		}

		// Token: 0x06001C8B RID: 7307 RVA: 0x0007AA26 File Offset: 0x00078C26
		private void resetFlags()
		{
			this.forceStateSend = false;
			this.sendAtPositionalRestMessage = false;
			this.sendAtRotationalRestMessage = false;
		}

		// Token: 0x06001C8C RID: 7308 RVA: 0x0007AA40 File Offset: 0x00078C40
		public void addState(StateMirror state)
		{
			if (this.stateCount > 1)
			{
				bool flag = state.ownerTimestamp - this.stateBuffer[0].ownerTimestamp <= 0f;
				bool flag2 = state.localTimeResetIndicator != this.stateBuffer[0].localTimeResetIndicator;
				if (flag && !flag2)
				{
					return;
				}
				if (flag2)
				{
					this.OnRemoteTimeReset();
				}
			}
			for (int i = this.stateBuffer.Length - 1; i >= 1; i--)
			{
				this.stateBuffer[i] = this.stateBuffer[i - 1];
			}
			this.stateBuffer[0] = state;
			this.stateCount = Mathf.Min(this.stateCount + 1, this.stateBuffer.Length);
		}

		// Token: 0x06001C8D RID: 7309 RVA: 0x0007AAE7 File Offset: 0x00078CE7
		public void stopEasing()
		{
			this.dontEasePosition = true;
			this.dontEaseRotation = true;
			this.dontEaseScale = true;
		}

		// Token: 0x06001C8E RID: 7310 RVA: 0x0007AAFE File Offset: 0x00078CFE
		public void clearBuffer()
		{
			this.stateCount = 0;
			this.firstReceivedMessageZeroTime = 0f;
			this.restStatePosition = SmoothSyncMirror.RestState.MOVING;
			this.restStateRotation = SmoothSyncMirror.RestState.MOVING;
		}

		// Token: 0x06001C8F RID: 7311 RVA: 0x0007AB20 File Offset: 0x00078D20
		public void teleport()
		{
			this.teleportOwnedObjectFromOwner();
		}

		// Token: 0x06001C90 RID: 7312 RVA: 0x0007AB28 File Offset: 0x00078D28
		public void teleportOwnedObjectFromOwner()
		{
			if (!this.hasControl)
			{
				if (NetworkServer.active)
				{
					Debug.LogWarning("Use teleportAnyObjectFromServer() since you are the server, do not own the object, and you will need to choose the new transform.");
					return;
				}
				Debug.LogWarning("Only owners of objects or the server can send messages out. Teleport from the owner or the server instead.");
				return;
			}
			else
			{
				this.latestTeleportedFromPosition = this.getPosition();
				this.latestTeleportedFromRotation = this.getRotation();
				if (NetworkServer.active)
				{
					this.RpcTeleport(this.getPosition(), this.getRotation().eulerAngles, this.getScale(), this.localTime);
					return;
				}
				if (base.isOwned)
				{
					this.CmdTeleport(this.getPosition(), this.getRotation().eulerAngles, this.getScale(), this.localTime);
				}
				return;
			}
		}

		// Token: 0x06001C91 RID: 7313 RVA: 0x0007ABD0 File Offset: 0x00078DD0
		public void teleportAnyObjectFromServer(Vector3 newPosition, Quaternion newRotation, Vector3 newScale)
		{
			if (this.hasControl)
			{
				this.setPosition(newPosition, true);
				this.setRotation(newRotation, true);
				this.setScale(newScale);
				this.teleportOwnedObjectFromOwner();
				return;
			}
			if (NetworkServer.active)
			{
				this.RpcNonServerOwnedTeleportFromServer(newPosition, newRotation.eulerAngles, newScale);
				return;
			}
			Debug.LogWarning("Call this from the server.");
		}

		// Token: 0x06001C92 RID: 7314 RVA: 0x0007AC24 File Offset: 0x00078E24
		[ClientRpc]
		public void RpcNonServerOwnedTeleportFromServer(Vector3 newPosition, Vector3 newRotation, Vector3 newScale)
		{
			NetworkWriterPooled writer = NetworkWriterPool.Get();
			writer.WriteVector3(newPosition);
			writer.WriteVector3(newRotation);
			writer.WriteVector3(newScale);
			this.SendRPCInternal("System.Void Smooth.SmoothSyncMirror::RpcNonServerOwnedTeleportFromServer(UnityEngine.Vector3,UnityEngine.Vector3,UnityEngine.Vector3)", -16266588, writer, 0, true);
			NetworkWriterPool.Return(writer);
		}

		// Token: 0x06001C93 RID: 7315 RVA: 0x0007AC74 File Offset: 0x00078E74
		[Command]
		public void CmdTeleport(Vector3 position, Vector3 rotation, Vector3 scale, float tempOwnerTime)
		{
			NetworkWriterPooled writer = NetworkWriterPool.Get();
			writer.WriteVector3(position);
			writer.WriteVector3(rotation);
			writer.WriteVector3(scale);
			writer.WriteFloat(tempOwnerTime);
			base.SendCommandInternal("System.Void Smooth.SmoothSyncMirror::CmdTeleport(UnityEngine.Vector3,UnityEngine.Vector3,UnityEngine.Vector3,System.Single)", 1505460848, writer, 0, true);
			NetworkWriterPool.Return(writer);
		}

		// Token: 0x06001C94 RID: 7316 RVA: 0x0007ACCC File Offset: 0x00078ECC
		[ClientRpc]
		public void RpcTeleport(Vector3 position, Vector3 rotation, Vector3 scale, float tempOwnerTime)
		{
			NetworkWriterPooled writer = NetworkWriterPool.Get();
			writer.WriteVector3(position);
			writer.WriteVector3(rotation);
			writer.WriteVector3(scale);
			writer.WriteFloat(tempOwnerTime);
			this.SendRPCInternal("System.Void Smooth.SmoothSyncMirror::RpcTeleport(UnityEngine.Vector3,UnityEngine.Vector3,UnityEngine.Vector3,System.Single)", -386256399, writer, 0, true);
			NetworkWriterPool.Return(writer);
		}

		// Token: 0x06001C95 RID: 7317 RVA: 0x0007AD24 File Offset: 0x00078F24
		private void addTeleportState(StateMirror teleportState)
		{
			if (teleportState != null)
			{
				teleportState.atPositionalRest = true;
				teleportState.atRotationalRest = true;
			}
			if (this.stateCount == 0)
			{
				this.approximateNetworkTimeOnOwner = teleportState.ownerTimestamp;
			}
			if (this.stateCount == 0 || teleportState.ownerTimestamp >= this.stateBuffer[0].ownerTimestamp)
			{
				for (int i = this.stateBuffer.Length - 1; i >= 1; i--)
				{
					this.stateBuffer[i] = this.stateBuffer[i - 1];
				}
				this.stateBuffer[0] = teleportState;
			}
			else
			{
				if (this.stateCount == this.stateBuffer.Length && this.stateBuffer[this.stateCount - 1].ownerTimestamp > teleportState.ownerTimestamp)
				{
					return;
				}
				for (int j = this.stateCount - 1; j >= 0; j--)
				{
					if (this.stateBuffer[j].ownerTimestamp > teleportState.ownerTimestamp)
					{
						for (int k = this.stateBuffer.Length - 1; k > j + 1; k--)
						{
							this.stateBuffer[k] = this.stateBuffer[k - 1];
						}
						this.stateBuffer[j + 1] = teleportState;
						break;
					}
				}
			}
			this.stateCount = Mathf.Min(this.stateCount + 1, this.stateBuffer.Length);
		}

		// Token: 0x06001C96 RID: 7318 RVA: 0x0007AE4E File Offset: 0x0007904E
		public void forceStateSendNextFixedUpdate()
		{
			this.forceStateSend = true;
		}

		// Token: 0x06001C97 RID: 7319 RVA: 0x0007AE58 File Offset: 0x00079058
		public void AssignAuthorityCallback(NetworkConnection conn, NetworkIdentity theNetID, bool authorityState)
		{
			NetworkIdentity networkIdentity = NetworkServer.spawned[theNetID.netId];
			if (networkIdentity == null)
			{
				Debug.LogWarning("Smooth Sync: Cannot find target for authority change.");
				return;
			}
			SmoothSyncMirror component = networkIdentity.GetComponent<SmoothSyncMirror>();
			if (component != null && component == this)
			{
				SmoothSyncMirror[] array = component.childObjectSmoothSyncs;
				for (int i = 0; i < array.Length; i++)
				{
					if (authorityState)
					{
						array[i].ownerChangeIndicator++;
						if (array[i].ownerChangeIndicator > 127)
						{
							array[i].ownerChangeIndicator = 1;
						}
					}
				}
			}
		}

		// Token: 0x06001C98 RID: 7320 RVA: 0x0007AEE0 File Offset: 0x000790E0
		public override void OnStartServer()
		{
			NetworkServer.ReplaceHandler<NetworkStateMirror>(new Action<NetworkConnectionToClient, NetworkStateMirror>(SmoothSyncMirror.HandleSyncServer), true);
		}

		// Token: 0x06001C99 RID: 7321 RVA: 0x0007AEF4 File Offset: 0x000790F4
		public override void OnStartClient()
		{
			this.registerClientHandlers();
		}

		// Token: 0x06001C9A RID: 7322 RVA: 0x0007AEFC File Offset: 0x000790FC
		public void registerClientHandlers()
		{
			if (!NetworkServer.active)
			{
				NetworkClient.ReplaceHandler<NetworkStateMirror>(new Action<NetworkStateMirror>(SmoothSyncMirror.HandleSyncClient), true);
			}
		}

		// Token: 0x06001C9B RID: 7323 RVA: 0x0007AF18 File Offset: 0x00079118
		public bool shouldSendPosition()
		{
			return this.syncPosition != SyncMode.NONE && (this.forceStateSend || (this.getPosition() != this.lastPositionWhenStateWasSent && (this.sendPositionThreshold == 0f || Vector3.Distance(this.lastPositionWhenStateWasSent, this.getPosition()) > this.sendPositionThreshold)));
		}

		// Token: 0x06001C9C RID: 7324 RVA: 0x0007AF74 File Offset: 0x00079174
		public bool shouldSendRotation()
		{
			return this.syncRotation != SyncMode.NONE && (this.forceStateSend || (this.getRotation() != this.lastRotationWhenStateWasSent && (this.sendRotationThreshold == 0f || Quaternion.Angle(this.lastRotationWhenStateWasSent, this.getRotation()) > this.sendRotationThreshold)));
		}

		// Token: 0x06001C9D RID: 7325 RVA: 0x0007AFD0 File Offset: 0x000791D0
		public bool shouldSendScale()
		{
			return this.syncScale != SyncMode.NONE && (this.forceStateSend || (this.getScale() != this.lastScaleWhenStateWasSent && (this.sendScaleThreshold == 0f || Vector3.Distance(this.lastScaleWhenStateWasSent, this.getScale()) > this.sendScaleThreshold)));
		}

		// Token: 0x06001C9E RID: 7326 RVA: 0x0007B02C File Offset: 0x0007922C
		public bool shouldSendVelocity()
		{
			if (this.hasRigidbody)
			{
				return this.syncVelocity != SyncMode.NONE && (this.forceStateSend || (this.rb.linearVelocity != this.lastVelocityWhenStateWasSent && (this.sendVelocityThreshold == 0f || Vector3.Distance(this.lastVelocityWhenStateWasSent, this.rb.linearVelocity) > this.sendVelocityThreshold)));
			}
			return this.hasRigidbody2D && (this.syncVelocity != SyncMode.NONE && (this.forceStateSend || ((this.rb2D.linearVelocity.x != this.lastVelocityWhenStateWasSent.x || this.rb2D.linearVelocity.y != this.lastVelocityWhenStateWasSent.y) && (this.sendVelocityThreshold == 0f || Vector2.Distance(this.lastVelocityWhenStateWasSent, this.rb2D.linearVelocity) > this.sendVelocityThreshold))));
		}

		// Token: 0x06001C9F RID: 7327 RVA: 0x0007B124 File Offset: 0x00079324
		public bool shouldSendAngularVelocity()
		{
			if (this.hasRigidbody)
			{
				return this.syncAngularVelocity != SyncMode.NONE && (this.forceStateSend || (this.rb.angularVelocity != this.lastAngularVelocityWhenStateWasSent && (this.sendAngularVelocityThreshold == 0f || Vector3.Distance(this.lastAngularVelocityWhenStateWasSent, this.rb.angularVelocity * 57.29578f) > this.sendAngularVelocityThreshold)));
			}
			return this.hasRigidbody2D && (this.syncAngularVelocity != SyncMode.NONE && (this.forceStateSend || (this.rb2D.angularVelocity != this.lastAngularVelocityWhenStateWasSent.z && (this.sendAngularVelocityThreshold == 0f || Mathf.Abs(this.lastAngularVelocityWhenStateWasSent.z - this.rb2D.angularVelocity) > this.sendAngularVelocityThreshold))));
		}

		// Token: 0x17000297 RID: 663
		// (get) Token: 0x06001CA0 RID: 7328 RVA: 0x0007B202 File Offset: 0x00079402
		public bool isSyncingXPosition
		{
			get
			{
				return this.syncPosition == SyncMode.XYZ || this.syncPosition == SyncMode.XY || this.syncPosition == SyncMode.XZ || this.syncPosition == SyncMode.X;
			}
		}

		// Token: 0x17000298 RID: 664
		// (get) Token: 0x06001CA1 RID: 7329 RVA: 0x0007B229 File Offset: 0x00079429
		public bool isSyncingYPosition
		{
			get
			{
				return this.syncPosition == SyncMode.XYZ || this.syncPosition == SyncMode.XY || this.syncPosition == SyncMode.YZ || this.syncPosition == SyncMode.Y;
			}
		}

		// Token: 0x17000299 RID: 665
		// (get) Token: 0x06001CA2 RID: 7330 RVA: 0x0007B250 File Offset: 0x00079450
		public bool isSyncingZPosition
		{
			get
			{
				return this.syncPosition == SyncMode.XYZ || this.syncPosition == SyncMode.XZ || this.syncPosition == SyncMode.YZ || this.syncPosition == SyncMode.Z;
			}
		}

		// Token: 0x1700029A RID: 666
		// (get) Token: 0x06001CA3 RID: 7331 RVA: 0x0007B277 File Offset: 0x00079477
		public bool isSyncingXRotation
		{
			get
			{
				return this.syncRotation == SyncMode.XYZ || this.syncRotation == SyncMode.XY || this.syncRotation == SyncMode.XZ || this.syncRotation == SyncMode.X;
			}
		}

		// Token: 0x1700029B RID: 667
		// (get) Token: 0x06001CA4 RID: 7332 RVA: 0x0007B29E File Offset: 0x0007949E
		public bool isSyncingYRotation
		{
			get
			{
				return this.syncRotation == SyncMode.XYZ || this.syncRotation == SyncMode.XY || this.syncRotation == SyncMode.YZ || this.syncRotation == SyncMode.Y;
			}
		}

		// Token: 0x1700029C RID: 668
		// (get) Token: 0x06001CA5 RID: 7333 RVA: 0x0007B2C5 File Offset: 0x000794C5
		public bool isSyncingZRotation
		{
			get
			{
				return this.syncRotation == SyncMode.XYZ || this.syncRotation == SyncMode.XZ || this.syncRotation == SyncMode.YZ || this.syncRotation == SyncMode.Z;
			}
		}

		// Token: 0x1700029D RID: 669
		// (get) Token: 0x06001CA6 RID: 7334 RVA: 0x0007B2EC File Offset: 0x000794EC
		public bool isSyncingXScale
		{
			get
			{
				return this.syncScale == SyncMode.XYZ || this.syncScale == SyncMode.XY || this.syncScale == SyncMode.XZ || this.syncScale == SyncMode.X;
			}
		}

		// Token: 0x1700029E RID: 670
		// (get) Token: 0x06001CA7 RID: 7335 RVA: 0x0007B313 File Offset: 0x00079513
		public bool isSyncingYScale
		{
			get
			{
				return this.syncScale == SyncMode.XYZ || this.syncScale == SyncMode.XY || this.syncScale == SyncMode.YZ || this.syncScale == SyncMode.Y;
			}
		}

		// Token: 0x1700029F RID: 671
		// (get) Token: 0x06001CA8 RID: 7336 RVA: 0x0007B33A File Offset: 0x0007953A
		public bool isSyncingZScale
		{
			get
			{
				return this.syncScale == SyncMode.XYZ || this.syncScale == SyncMode.XZ || this.syncScale == SyncMode.YZ || this.syncScale == SyncMode.Z;
			}
		}

		// Token: 0x170002A0 RID: 672
		// (get) Token: 0x06001CA9 RID: 7337 RVA: 0x0007B361 File Offset: 0x00079561
		public bool isSyncingXVelocity
		{
			get
			{
				return this.syncVelocity == SyncMode.XYZ || this.syncVelocity == SyncMode.XY || this.syncVelocity == SyncMode.XZ || this.syncVelocity == SyncMode.X;
			}
		}

		// Token: 0x170002A1 RID: 673
		// (get) Token: 0x06001CAA RID: 7338 RVA: 0x0007B388 File Offset: 0x00079588
		public bool isSyncingYVelocity
		{
			get
			{
				return this.syncVelocity == SyncMode.XYZ || this.syncVelocity == SyncMode.XY || this.syncVelocity == SyncMode.YZ || this.syncVelocity == SyncMode.Y;
			}
		}

		// Token: 0x170002A2 RID: 674
		// (get) Token: 0x06001CAB RID: 7339 RVA: 0x0007B3AF File Offset: 0x000795AF
		public bool isSyncingZVelocity
		{
			get
			{
				return this.syncVelocity == SyncMode.XYZ || this.syncVelocity == SyncMode.XZ || this.syncVelocity == SyncMode.YZ || this.syncVelocity == SyncMode.Z;
			}
		}

		// Token: 0x170002A3 RID: 675
		// (get) Token: 0x06001CAC RID: 7340 RVA: 0x0007B3D6 File Offset: 0x000795D6
		public bool isSyncingXAngularVelocity
		{
			get
			{
				return this.syncAngularVelocity == SyncMode.XYZ || this.syncAngularVelocity == SyncMode.XY || this.syncAngularVelocity == SyncMode.XZ || this.syncAngularVelocity == SyncMode.X;
			}
		}

		// Token: 0x170002A4 RID: 676
		// (get) Token: 0x06001CAD RID: 7341 RVA: 0x0007B3FD File Offset: 0x000795FD
		public bool isSyncingYAngularVelocity
		{
			get
			{
				return this.syncAngularVelocity == SyncMode.XYZ || this.syncAngularVelocity == SyncMode.XY || this.syncAngularVelocity == SyncMode.YZ || this.syncAngularVelocity == SyncMode.Y;
			}
		}

		// Token: 0x170002A5 RID: 677
		// (get) Token: 0x06001CAE RID: 7342 RVA: 0x0007B424 File Offset: 0x00079624
		public bool isSyncingZAngularVelocity
		{
			get
			{
				return this.syncAngularVelocity == SyncMode.XYZ || this.syncAngularVelocity == SyncMode.XZ || this.syncAngularVelocity == SyncMode.YZ || this.syncAngularVelocity == SyncMode.Z;
			}
		}

		// Token: 0x06001CAF RID: 7343 RVA: 0x0007B44C File Offset: 0x0007964C
		[Server]
		private void SendStateToNonOwners(NetworkStateMirror state)
		{
			if (!NetworkServer.active)
			{
				Debug.LogWarning("[Server] function 'System.Void Smooth.SmoothSyncMirror::SendStateToNonOwners(Smooth.NetworkStateMirror)' called when server was not active");
				return;
			}
			if (this.netID.observers == null)
			{
				return;
			}
			foreach (KeyValuePair<int, NetworkConnectionToClient> keyValuePair in this.netID.observers)
			{
				NetworkConnection value = keyValuePair.Value;
				if (value != null && (this.transformSource == SmoothSyncMirror.TransformSource.Server || value != this.netID.connectionToClient) && value.GetType() == typeof(NetworkConnectionToClient) && value.isReady)
				{
					value.Send<NetworkStateMirror>(state, this.networkChannel);
				}
			}
		}

		// Token: 0x06001CB0 RID: 7344 RVA: 0x0007B510 File Offset: 0x00079710
		public static void HandleSyncServer(NetworkConnectionToClient conn, NetworkStateMirror networkState)
		{
			if (networkState.smoothSync == null || networkState.smoothSync.netID.connectionToClient != conn)
			{
				return;
			}
			if (networkState.smoothSync.latestValidatedState == null || networkState.smoothSync.validateStateMethod(networkState.state, networkState.smoothSync.latestValidatedState))
			{
				networkState.smoothSync.latestValidatedState = networkState.state;
				networkState.smoothSync.latestValidatedState.receivedOnServerTimestamp = networkState.smoothSync.localTime;
				networkState.smoothSync.SendStateToNonOwners(networkState);
				networkState.smoothSync.addState(networkState.state);
				networkState.smoothSync.checkIfOwnerHasChanged(networkState.state);
			}
		}

		// Token: 0x06001CB1 RID: 7345 RVA: 0x0007B5C9 File Offset: 0x000797C9
		public static void HandleSyncClient(NetworkStateMirror networkState)
		{
			if (networkState.smoothSync != null && !networkState.smoothSync.hasControl)
			{
				networkState.smoothSync.addState(networkState.state);
				networkState.smoothSync.checkIfOwnerHasChanged(networkState.state);
			}
		}

		// Token: 0x06001CB2 RID: 7346 RVA: 0x0007B608 File Offset: 0x00079808
		public void checkIfOwnerHasChanged(StateMirror newState)
		{
			if (this.isSmoothingAuthorityChanges && this.ownerChangeIndicator != this.previousReceivedOwnerInt)
			{
				this.approximateNetworkTimeOnOwner = newState.ownerTimestamp;
				this.latestAuthorityChangeZeroTime = this.localTime;
				this.stateCount = 0;
				this.firstReceivedMessageZeroTime = 1f;
				this.restStatePosition = SmoothSyncMirror.RestState.MOVING;
				this.restStateRotation = SmoothSyncMirror.RestState.MOVING;
				this.addState(new StateMirror
				{
					position = this.getPosition(),
					rotation = this.getRotation(),
					scale = this.getScale(),
					ownerTimestamp = newState.ownerTimestamp - this.interpolationBackTime,
					receivedTimestamp = newState.receivedTimestamp
				});
				this.previousReceivedOwnerInt = this.ownerChangeIndicator;
			}
		}

		// Token: 0x06001CB3 RID: 7347 RVA: 0x0007B6C5 File Offset: 0x000798C5
		public float GetNetworkSendInterval()
		{
			if (this.sendRate == 0f)
			{
				return 0f;
			}
			return 1f / this.sendRate;
		}

		// Token: 0x170002A6 RID: 678
		// (get) Token: 0x06001CB4 RID: 7348 RVA: 0x0007B6E6 File Offset: 0x000798E6
		// (set) Token: 0x06001CB5 RID: 7349 RVA: 0x0007B6FC File Offset: 0x000798FC
		public float approximateNetworkTimeOnOwner
		{
			get
			{
				return this._ownerTime + (this.localTime - this.lastTimeOwnerTimeWasSet);
			}
			set
			{
				this._ownerTime = value;
				this.lastTimeOwnerTimeWasSet = this.localTime;
			}
		}

		// Token: 0x06001CB6 RID: 7350 RVA: 0x0007B714 File Offset: 0x00079914
		private void adjustOwnerTime()
		{
			if (this.stateBuffer[0] == null || (this.stateBuffer[0].atPositionalRest && this.stateBuffer[0].atRotationalRest))
			{
				return;
			}
			float num = this.stateBuffer[0].ownerTimestamp + (this.localTime - this.stateBuffer[0].receivedTimestamp);
			float num2 = Mathf.Max(this.timeCorrectionSpeed * Time.deltaTime, this.minTimePrecision);
			if (this.firstReceivedMessageZeroTime == 0f)
			{
				this.firstReceivedMessageZeroTime = this.localTime;
			}
			float num3 = Mathf.Abs(this.approximateNetworkTimeOnOwner - num);
			if ((float)this.receivedStatesCounter < this.sendRate || num3 < num2 || num3 > this.snapTimeThreshold)
			{
				this.approximateNetworkTimeOnOwner = num;
				return;
			}
			if (this.approximateNetworkTimeOnOwner < num)
			{
				this.approximateNetworkTimeOnOwner += num2;
				return;
			}
			this.approximateNetworkTimeOnOwner -= num2;
		}

		// Token: 0x06001CB8 RID: 7352 RVA: 0x00002321 File Offset: 0x00000521
		public override bool Weaved()
		{
			return true;
		}

		// Token: 0x06001CB9 RID: 7353 RVA: 0x0007B90B File Offset: 0x00079B0B
		protected void UserCode_RpcNonServerOwnedTeleportFromServer__Vector3__Vector3__Vector3(Vector3 newPosition, Vector3 newRotation, Vector3 newScale)
		{
			if (this.hasAuthorityOrUnownedOnServer)
			{
				this.setPosition(newPosition, true);
				this.setRotation(Quaternion.Euler(newRotation), true);
				this.setScale(newScale);
				this.teleportOwnedObjectFromOwner();
			}
		}

		// Token: 0x06001CBA RID: 7354 RVA: 0x0007B937 File Offset: 0x00079B37
		protected static void InvokeUserCode_RpcNonServerOwnedTeleportFromServer__Vector3__Vector3__Vector3(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
		{
			if (!NetworkClient.active)
			{
				Debug.LogError("RPC RpcNonServerOwnedTeleportFromServer called on server.");
				return;
			}
			((SmoothSyncMirror)obj).UserCode_RpcNonServerOwnedTeleportFromServer__Vector3__Vector3__Vector3(reader.ReadVector3(), reader.ReadVector3(), reader.ReadVector3());
		}

		// Token: 0x06001CBB RID: 7355 RVA: 0x0007B96C File Offset: 0x00079B6C
		protected void UserCode_CmdTeleport__Vector3__Vector3__Vector3__Single(Vector3 position, Vector3 rotation, Vector3 scale, float tempOwnerTime)
		{
			this.RpcTeleport(position, rotation, scale, tempOwnerTime);
			StateMirror stateMirror = new StateMirror();
			stateMirror.copyFromSmoothSync(this);
			stateMirror.position = position;
			stateMirror.rotation = Quaternion.Euler(rotation);
			stateMirror.ownerTimestamp = tempOwnerTime;
			stateMirror.receivedTimestamp = this.localTime;
			stateMirror.teleport = true;
			this.addTeleportState(stateMirror);
		}

		// Token: 0x06001CBC RID: 7356 RVA: 0x0007B9C6 File Offset: 0x00079BC6
		protected static void InvokeUserCode_CmdTeleport__Vector3__Vector3__Vector3__Single(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
		{
			if (!NetworkServer.active)
			{
				Debug.LogError("Command CmdTeleport called on client.");
				return;
			}
			((SmoothSyncMirror)obj).UserCode_CmdTeleport__Vector3__Vector3__Vector3__Single(reader.ReadVector3(), reader.ReadVector3(), reader.ReadVector3(), reader.ReadFloat());
		}

		// Token: 0x06001CBD RID: 7357 RVA: 0x0007BA04 File Offset: 0x00079C04
		protected void UserCode_RpcTeleport__Vector3__Vector3__Vector3__Single(Vector3 position, Vector3 rotation, Vector3 scale, float tempOwnerTime)
		{
			if (this.hasAuthorityOrUnownedOnServer || NetworkServer.active)
			{
				return;
			}
			StateMirror stateMirror = new StateMirror();
			stateMirror.copyFromSmoothSync(this);
			stateMirror.position = position;
			stateMirror.rotation = Quaternion.Euler(rotation);
			stateMirror.ownerTimestamp = tempOwnerTime;
			stateMirror.receivedTimestamp = this.localTime;
			stateMirror.teleport = true;
			this.addTeleportState(stateMirror);
		}

		// Token: 0x06001CBE RID: 7358 RVA: 0x0007BA63 File Offset: 0x00079C63
		protected static void InvokeUserCode_RpcTeleport__Vector3__Vector3__Vector3__Single(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
		{
			if (!NetworkClient.active)
			{
				Debug.LogError("RPC RpcTeleport called on server.");
				return;
			}
			((SmoothSyncMirror)obj).UserCode_RpcTeleport__Vector3__Vector3__Vector3__Single(reader.ReadVector3(), reader.ReadVector3(), reader.ReadVector3(), reader.ReadFloat());
		}

		// Token: 0x06001CBF RID: 7359 RVA: 0x0007BAA0 File Offset: 0x00079CA0
		static SmoothSyncMirror()
		{
			RemoteProcedureCalls.RegisterCommand(typeof(SmoothSyncMirror), "System.Void Smooth.SmoothSyncMirror::CmdTeleport(UnityEngine.Vector3,UnityEngine.Vector3,UnityEngine.Vector3,System.Single)", new RemoteCallDelegate(SmoothSyncMirror.InvokeUserCode_CmdTeleport__Vector3__Vector3__Vector3__Single), true);
			RemoteProcedureCalls.RegisterRpc(typeof(SmoothSyncMirror), "System.Void Smooth.SmoothSyncMirror::RpcNonServerOwnedTeleportFromServer(UnityEngine.Vector3,UnityEngine.Vector3,UnityEngine.Vector3)", new RemoteCallDelegate(SmoothSyncMirror.InvokeUserCode_RpcNonServerOwnedTeleportFromServer__Vector3__Vector3__Vector3));
			RemoteProcedureCalls.RegisterRpc(typeof(SmoothSyncMirror), "System.Void Smooth.SmoothSyncMirror::RpcTeleport(UnityEngine.Vector3,UnityEngine.Vector3,UnityEngine.Vector3,System.Single)", new RemoteCallDelegate(SmoothSyncMirror.InvokeUserCode_RpcTeleport__Vector3__Vector3__Vector3__Single));
		}

		// Token: 0x040012CE RID: 4814
		public float interpolationBackTime = 0.1f;

		// Token: 0x040012CF RID: 4815
		public SmoothSyncMirror.ExtrapolationMode extrapolationMode = SmoothSyncMirror.ExtrapolationMode.Limited;

		// Token: 0x040012D0 RID: 4816
		public bool useExtrapolationTimeLimit = true;

		// Token: 0x040012D1 RID: 4817
		public float extrapolationTimeLimit = 5f;

		// Token: 0x040012D2 RID: 4818
		public bool useExtrapolationDistanceLimit;

		// Token: 0x040012D3 RID: 4819
		public float extrapolationDistanceLimit = 20f;

		// Token: 0x040012D4 RID: 4820
		public float sendPositionThreshold;

		// Token: 0x040012D5 RID: 4821
		public float sendRotationThreshold;

		// Token: 0x040012D6 RID: 4822
		public float sendScaleThreshold;

		// Token: 0x040012D7 RID: 4823
		public float sendVelocityThreshold;

		// Token: 0x040012D8 RID: 4824
		public float sendAngularVelocityThreshold;

		// Token: 0x040012D9 RID: 4825
		public float receivedPositionThreshold;

		// Token: 0x040012DA RID: 4826
		public float receivedRotationThreshold;

		// Token: 0x040012DB RID: 4827
		public float snapPositionThreshold;

		// Token: 0x040012DC RID: 4828
		public float snapRotationThreshold;

		// Token: 0x040012DD RID: 4829
		public float snapScaleThreshold;

		// Token: 0x040012DE RID: 4830
		[Range(0f, 1f)]
		public float positionLerpSpeed = 0.85f;

		// Token: 0x040012DF RID: 4831
		[Range(0f, 1f)]
		public float rotationLerpSpeed = 0.85f;

		// Token: 0x040012E0 RID: 4832
		[Range(0f, 1f)]
		public float scaleLerpSpeed = 0.85f;

		// Token: 0x040012E1 RID: 4833
		[Range(0f, 5f)]
		public float timeCorrectionSpeed = 0.1f;

		// Token: 0x040012E2 RID: 4834
		public float snapTimeThreshold = 0.3f;

		// Token: 0x040012E3 RID: 4835
		public SyncMode syncPosition;

		// Token: 0x040012E4 RID: 4836
		public SyncMode syncRotation;

		// Token: 0x040012E5 RID: 4837
		public SyncMode syncScale;

		// Token: 0x040012E6 RID: 4838
		public SyncMode syncVelocity;

		// Token: 0x040012E7 RID: 4839
		public SyncMode syncAngularVelocity;

		// Token: 0x040012E8 RID: 4840
		public bool isPositionCompressed;

		// Token: 0x040012E9 RID: 4841
		public bool isRotationCompressed;

		// Token: 0x040012EA RID: 4842
		public bool isScaleCompressed;

		// Token: 0x040012EB RID: 4843
		public bool isVelocityCompressed;

		// Token: 0x040012EC RID: 4844
		public bool isAngularVelocityCompressed;

		// Token: 0x040012ED RID: 4845
		public bool automaticallyResetTime = true;

		// Token: 0x040012EF RID: 4847
		private const int maxTimePower = 12;

		// Token: 0x040012F0 RID: 4848
		private readonly float maxLocalTime = Mathf.Pow(2f, 12f);

		// Token: 0x040012F1 RID: 4849
		private readonly float minTimePrecision = Mathf.Pow(2f, -12f);

		// Token: 0x040012F2 RID: 4850
		[NonSerialized]
		public int localTimeResetIndicator;

		// Token: 0x040012F3 RID: 4851
		public bool isSmoothingAuthorityChanges;

		// Token: 0x040012F4 RID: 4852
		public SmoothSyncMirror.TransformSource transformSource;

		// Token: 0x040012F5 RID: 4853
		public SmoothSyncMirror.WhenToUpdateTransform whenToUpdateTransform;

		// Token: 0x040012F6 RID: 4854
		public float sendRate = 30f;

		// Token: 0x040012F7 RID: 4855
		public int networkChannel = 1;

		// Token: 0x040012F8 RID: 4856
		public GameObject childObjectToSync;

		// Token: 0x040012F9 RID: 4857
		[NonSerialized]
		public bool isSyncingChild;

		// Token: 0x040012FA RID: 4858
		[NonSerialized]
		public SmoothSyncMirror.validateStateDelegate validateStateMethod = new SmoothSyncMirror.validateStateDelegate(SmoothSyncMirror.validateState);

		// Token: 0x040012FB RID: 4859
		private StateMirror latestValidatedState;

		// Token: 0x040012FC RID: 4860
		public bool setVelocityInsteadOfPositionOnNonOwners;

		// Token: 0x040012FD RID: 4861
		public float maxPositionDifferenceForVelocitySyncing = 10f;

		// Token: 0x040012FE RID: 4862
		public bool useLocalTransformOnly;

		// Token: 0x040012FF RID: 4863
		[NonSerialized]
		public StateMirror[] stateBuffer;

		// Token: 0x04001300 RID: 4864
		[NonSerialized]
		public int stateCount;

		// Token: 0x04001301 RID: 4865
		[NonSerialized]
		public Rigidbody rb;

		// Token: 0x04001302 RID: 4866
		[NonSerialized]
		public bool hasRigidbody;

		// Token: 0x04001303 RID: 4867
		[NonSerialized]
		public Rigidbody2D rb2D;

		// Token: 0x04001304 RID: 4868
		[NonSerialized]
		public bool hasRigidbody2D;

		// Token: 0x04001305 RID: 4869
		private bool dontEasePosition;

		// Token: 0x04001306 RID: 4870
		private bool dontEaseScale;

		// Token: 0x04001307 RID: 4871
		private bool dontEaseRotation;

		// Token: 0x04001308 RID: 4872
		private float firstReceivedMessageZeroTime;

		// Token: 0x04001309 RID: 4873
		[NonSerialized]
		public float lastTimeStateWasSent;

		// Token: 0x0400130A RID: 4874
		[NonSerialized]
		public Vector3 lastPositionWhenStateWasSent;

		// Token: 0x0400130B RID: 4875
		[NonSerialized]
		public Quaternion lastRotationWhenStateWasSent = Quaternion.identity;

		// Token: 0x0400130C RID: 4876
		[NonSerialized]
		public Vector3 lastScaleWhenStateWasSent;

		// Token: 0x0400130D RID: 4877
		[NonSerialized]
		public Vector3 lastVelocityWhenStateWasSent;

		// Token: 0x0400130E RID: 4878
		[NonSerialized]
		public Vector3 lastAngularVelocityWhenStateWasSent;

		// Token: 0x0400130F RID: 4879
		[NonSerialized]
		public NetworkIdentity netID;

		// Token: 0x04001310 RID: 4880
		[NonSerialized]
		public GameObject realObjectToSync;

		// Token: 0x04001311 RID: 4881
		[NonSerialized]
		public int syncIndex;

		// Token: 0x04001312 RID: 4882
		[NonSerialized]
		public SmoothSyncMirror[] childObjectSmoothSyncs = new SmoothSyncMirror[0];

		// Token: 0x04001313 RID: 4883
		[NonSerialized]
		public bool forceStateSend;

		// Token: 0x04001314 RID: 4884
		[NonSerialized]
		public bool sendAtPositionalRestMessage;

		// Token: 0x04001315 RID: 4885
		[NonSerialized]
		public bool sendAtRotationalRestMessage;

		// Token: 0x04001316 RID: 4886
		[NonSerialized]
		public bool sendPosition;

		// Token: 0x04001317 RID: 4887
		[NonSerialized]
		public bool sendRotation;

		// Token: 0x04001318 RID: 4888
		[NonSerialized]
		public bool sendScale;

		// Token: 0x04001319 RID: 4889
		[NonSerialized]
		public bool sendVelocity;

		// Token: 0x0400131A RID: 4890
		[NonSerialized]
		public bool sendAngularVelocity;

		// Token: 0x0400131B RID: 4891
		private StateMirror targetTempState;

		// Token: 0x0400131C RID: 4892
		private NetworkStateMirror sendingTempState;

		// Token: 0x0400131D RID: 4893
		[NonSerialized]
		public Vector3 latestReceivedVelocity;

		// Token: 0x0400131E RID: 4894
		[NonSerialized]
		public Vector3 latestReceivedAngularVelocity;

		// Token: 0x0400131F RID: 4895
		private float timeSpentExtrapolating;

		// Token: 0x04001320 RID: 4896
		private bool extrapolatedLastFrame;

		// Token: 0x04001321 RID: 4897
		private Vector3 positionLastFrame;

		// Token: 0x04001322 RID: 4898
		private bool changedPositionLastFrame;

		// Token: 0x04001323 RID: 4899
		private Quaternion rotationLastFrame;

		// Token: 0x04001324 RID: 4900
		private bool changedRotationLastFrame;

		// Token: 0x04001325 RID: 4901
		private int atRestThresholdCount = 3;

		// Token: 0x04001326 RID: 4902
		private int samePositionCount;

		// Token: 0x04001327 RID: 4903
		private int sameRotationCount;

		// Token: 0x04001328 RID: 4904
		private SmoothSyncMirror.RestState restStatePosition = SmoothSyncMirror.RestState.MOVING;

		// Token: 0x04001329 RID: 4905
		private SmoothSyncMirror.RestState restStateRotation = SmoothSyncMirror.RestState.MOVING;

		// Token: 0x0400132A RID: 4906
		private bool hadAuthorityLastFrame;

		// Token: 0x0400132B RID: 4907
		private StateMirror latestEndStateUsed;

		// Token: 0x0400132C RID: 4908
		private Vector3 latestTeleportedFromPosition;

		// Token: 0x0400132D RID: 4909
		private Quaternion latestTeleportedFromRotation;

		// Token: 0x0400132E RID: 4910
		private bool hasCachedNetID;

		// Token: 0x0400132F RID: 4911
		private NetworkIdentity cachedNetIdentity;

		// Token: 0x04001330 RID: 4912
		private bool triedToExtrapolateTooFar;

		// Token: 0x04001331 RID: 4913
		private float _ownerTime;

		// Token: 0x04001332 RID: 4914
		private float lastTimeOwnerTimeWasSet;

		// Token: 0x04001333 RID: 4915
		private float latestAuthorityChangeZeroTime;

		// Token: 0x04001334 RID: 4916
		private int previousReceivedOwnerInt = 1;

		// Token: 0x04001335 RID: 4917
		public int ownerChangeIndicator = 1;

		// Token: 0x04001336 RID: 4918
		public int receivedStatesCounter;

		// Token: 0x02000360 RID: 864
		public enum ExtrapolationMode
		{
			// Token: 0x04001338 RID: 4920
			None,
			// Token: 0x04001339 RID: 4921
			Limited,
			// Token: 0x0400133A RID: 4922
			Unlimited
		}

		// Token: 0x02000361 RID: 865
		public enum TransformSource
		{
			// Token: 0x0400133C RID: 4924
			Owner,
			// Token: 0x0400133D RID: 4925
			Server
		}

		// Token: 0x02000362 RID: 866
		public enum WhenToUpdateTransform
		{
			// Token: 0x0400133F RID: 4927
			Update,
			// Token: 0x04001340 RID: 4928
			FixedUpdate
		}

		// Token: 0x02000363 RID: 867
		// (Invoke) Token: 0x06001CC1 RID: 7361
		public delegate bool validateStateDelegate(StateMirror receivedState, StateMirror latestVerifiedState);

		// Token: 0x02000364 RID: 868
		private enum RestState
		{
			// Token: 0x04001342 RID: 4930
			AT_REST,
			// Token: 0x04001343 RID: 4931
			JUST_STARTED_MOVING,
			// Token: 0x04001344 RID: 4932
			MOVING
		}
	}
}
