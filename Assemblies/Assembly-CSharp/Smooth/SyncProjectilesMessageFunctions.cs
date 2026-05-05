using System;
using Mirror;
using UnityEngine;

namespace Smooth
{
	// Token: 0x02000367 RID: 871
	public static class SyncProjectilesMessageFunctions
	{
		// Token: 0x06001CCA RID: 7370 RVA: 0x0007BD98 File Offset: 0x00079F98
		public static void Serialize(this NetworkWriter writer, NetworkStateMirror msg)
		{
			SmoothSyncMirror smoothSync = msg.smoothSync;
			StateMirror state = msg.state;
			bool flag;
			bool flag2;
			bool flag3;
			bool flag4;
			bool flag5;
			bool atPositionalRest;
			bool atRotationalRest;
			if (NetworkServer.active && !smoothSync.hasControl)
			{
				flag = state.serverShouldRelayPosition;
				flag2 = state.serverShouldRelayRotation;
				flag3 = state.serverShouldRelayScale;
				flag4 = state.serverShouldRelayVelocity;
				flag5 = state.serverShouldRelayAngularVelocity;
				atPositionalRest = state.atPositionalRest;
				atRotationalRest = state.atRotationalRest;
			}
			else
			{
				flag = smoothSync.sendPosition;
				flag2 = smoothSync.sendRotation;
				flag3 = smoothSync.sendScale;
				flag4 = smoothSync.sendVelocity;
				flag5 = smoothSync.sendAngularVelocity;
				atPositionalRest = smoothSync.sendAtPositionalRestMessage;
				atRotationalRest = smoothSync.sendAtRotationalRestMessage;
			}
			if (!NetworkServer.active)
			{
				if (flag)
				{
					smoothSync.lastPositionWhenStateWasSent = state.position;
				}
				if (flag2)
				{
					smoothSync.lastRotationWhenStateWasSent = state.rotation;
				}
				if (flag3)
				{
					smoothSync.lastScaleWhenStateWasSent = state.scale;
				}
				if (flag4)
				{
					smoothSync.lastVelocityWhenStateWasSent = state.velocity;
				}
				if (flag5)
				{
					smoothSync.lastAngularVelocityWhenStateWasSent = state.angularVelocity;
				}
			}
			byte b = 0;
			b += 1;
			b += 1;
			b += 4;
			b += 4;
			b += 4;
			if (flag)
			{
				byte b2 = 4;
				if (smoothSync.isPositionCompressed)
				{
					b2 = 2;
				}
				if (smoothSync.isSyncingXPosition)
				{
					b += b2;
				}
				if (smoothSync.isSyncingYPosition)
				{
					b += b2;
				}
				if (smoothSync.isSyncingZPosition)
				{
					b += b2;
				}
			}
			if (flag2)
			{
				byte b3 = 4;
				if (smoothSync.isRotationCompressed)
				{
					b3 = 2;
				}
				if (smoothSync.isSyncingXRotation)
				{
					b += b3;
				}
				if (smoothSync.isSyncingYRotation)
				{
					b += b3;
				}
				if (smoothSync.isSyncingZRotation)
				{
					b += b3;
				}
			}
			if (flag3)
			{
				byte b4 = 4;
				if (smoothSync.isScaleCompressed)
				{
					b4 = 2;
				}
				if (smoothSync.isSyncingXScale)
				{
					b += b4;
				}
				if (smoothSync.isSyncingYScale)
				{
					b += b4;
				}
				if (smoothSync.isSyncingZScale)
				{
					b += b4;
				}
			}
			if (flag4)
			{
				byte b5 = 4;
				if (smoothSync.isVelocityCompressed)
				{
					b5 = 2;
				}
				if (smoothSync.isSyncingXVelocity)
				{
					b += b5;
				}
				if (smoothSync.isSyncingYVelocity)
				{
					b += b5;
				}
				if (smoothSync.isSyncingZVelocity)
				{
					b += b5;
				}
			}
			if (flag5)
			{
				byte b6 = 4;
				if (smoothSync.isAngularVelocityCompressed)
				{
					b6 = 2;
				}
				if (smoothSync.isSyncingXAngularVelocity)
				{
					b += b6;
				}
				if (smoothSync.isSyncingYAngularVelocity)
				{
					b += b6;
				}
				if (smoothSync.isSyncingZAngularVelocity)
				{
					b += b6;
				}
			}
			if (smoothSync.isSmoothingAuthorityChanges && NetworkServer.active)
			{
				b += 1;
			}
			if (smoothSync.automaticallyResetTime)
			{
				b += 1;
			}
			writer.WriteByte(b);
			writer.WriteByte(SyncProjectilesMessageFunctions.encodeSyncInformation(flag, flag2, flag3, flag4, flag5, atPositionalRest, atRotationalRest));
			writer.WriteNetworkIdentity(smoothSync.netID);
			writer.WriteUInt((uint)smoothSync.syncIndex);
			writer.WriteFloat(state.ownerTimestamp);
			if (flag)
			{
				if (smoothSync.isPositionCompressed)
				{
					if (smoothSync.isSyncingXPosition)
					{
						writer.WriteUShort(HalfHelper.Compress(state.position.x));
					}
					if (smoothSync.isSyncingYPosition)
					{
						writer.WriteUShort(HalfHelper.Compress(state.position.y));
					}
					if (smoothSync.isSyncingZPosition)
					{
						writer.WriteUShort(HalfHelper.Compress(state.position.z));
					}
				}
				else
				{
					if (smoothSync.isSyncingXPosition)
					{
						writer.WriteFloat(state.position.x);
					}
					if (smoothSync.isSyncingYPosition)
					{
						writer.WriteFloat(state.position.y);
					}
					if (smoothSync.isSyncingZPosition)
					{
						writer.WriteFloat(state.position.z);
					}
				}
			}
			if (flag2)
			{
				Vector3 eulerAngles = state.rotation.eulerAngles;
				if (smoothSync.isRotationCompressed)
				{
					if (smoothSync.isSyncingXRotation)
					{
						writer.WriteUShort(HalfHelper.Compress(eulerAngles.x * 0.017453292f));
					}
					if (smoothSync.isSyncingYRotation)
					{
						writer.WriteUShort(HalfHelper.Compress(eulerAngles.y * 0.017453292f));
					}
					if (smoothSync.isSyncingZRotation)
					{
						writer.WriteUShort(HalfHelper.Compress(eulerAngles.z * 0.017453292f));
					}
				}
				else
				{
					if (smoothSync.isSyncingXRotation)
					{
						writer.WriteFloat(eulerAngles.x);
					}
					if (smoothSync.isSyncingYRotation)
					{
						writer.WriteFloat(eulerAngles.y);
					}
					if (smoothSync.isSyncingZRotation)
					{
						writer.WriteFloat(eulerAngles.z);
					}
				}
			}
			if (flag3)
			{
				if (smoothSync.isScaleCompressed)
				{
					if (smoothSync.isSyncingXScale)
					{
						writer.WriteUShort(HalfHelper.Compress(state.scale.x));
					}
					if (smoothSync.isSyncingYScale)
					{
						writer.WriteUShort(HalfHelper.Compress(state.scale.y));
					}
					if (smoothSync.isSyncingZScale)
					{
						writer.WriteUShort(HalfHelper.Compress(state.scale.z));
					}
				}
				else
				{
					if (smoothSync.isSyncingXScale)
					{
						writer.WriteFloat(state.scale.x);
					}
					if (smoothSync.isSyncingYScale)
					{
						writer.WriteFloat(state.scale.y);
					}
					if (smoothSync.isSyncingZScale)
					{
						writer.WriteFloat(state.scale.z);
					}
				}
			}
			if (flag4)
			{
				if (smoothSync.isVelocityCompressed)
				{
					if (smoothSync.isSyncingXVelocity)
					{
						writer.WriteUShort(HalfHelper.Compress(state.velocity.x));
					}
					if (smoothSync.isSyncingYVelocity)
					{
						writer.WriteUShort(HalfHelper.Compress(state.velocity.y));
					}
					if (smoothSync.isSyncingZVelocity)
					{
						writer.WriteUShort(HalfHelper.Compress(state.velocity.z));
					}
				}
				else
				{
					if (smoothSync.isSyncingXVelocity)
					{
						writer.WriteFloat(state.velocity.x);
					}
					if (smoothSync.isSyncingYVelocity)
					{
						writer.WriteFloat(state.velocity.y);
					}
					if (smoothSync.isSyncingZVelocity)
					{
						writer.WriteFloat(state.velocity.z);
					}
				}
			}
			if (flag5)
			{
				if (smoothSync.isAngularVelocityCompressed)
				{
					if (smoothSync.isSyncingXAngularVelocity)
					{
						writer.WriteUShort(HalfHelper.Compress(state.angularVelocity.x * 0.017453292f));
					}
					if (smoothSync.isSyncingYAngularVelocity)
					{
						writer.WriteUShort(HalfHelper.Compress(state.angularVelocity.y * 0.017453292f));
					}
					if (smoothSync.isSyncingZAngularVelocity)
					{
						writer.WriteUShort(HalfHelper.Compress(state.angularVelocity.z * 0.017453292f));
					}
				}
				else
				{
					if (smoothSync.isSyncingXAngularVelocity)
					{
						writer.WriteFloat(state.angularVelocity.x);
					}
					if (smoothSync.isSyncingYAngularVelocity)
					{
						writer.WriteFloat(state.angularVelocity.y);
					}
					if (smoothSync.isSyncingZAngularVelocity)
					{
						writer.WriteFloat(state.angularVelocity.z);
					}
				}
			}
			if (smoothSync.isSmoothingAuthorityChanges && NetworkServer.active)
			{
				writer.WriteByte((byte)smoothSync.ownerChangeIndicator);
			}
			if (smoothSync.automaticallyResetTime)
			{
				writer.WriteByte((byte)state.localTimeResetIndicator);
			}
		}

		// Token: 0x06001CCB RID: 7371 RVA: 0x0007C49C File Offset: 0x0007A69C
		public static NetworkStateMirror Deserialize(this NetworkReader reader)
		{
			NetworkStateMirror networkStateMirror = new NetworkStateMirror
			{
				state = new StateMirror()
			};
			StateMirror state = networkStateMirror.state;
			byte b = 0;
			byte b2 = reader.ReadByte();
			b += 1;
			byte syncInformation = reader.ReadByte();
			b += 1;
			bool flag = SyncProjectilesMessageFunctions.shouldSyncPosition(syncInformation);
			bool flag2 = SyncProjectilesMessageFunctions.shouldSyncRotation(syncInformation);
			bool flag3 = SyncProjectilesMessageFunctions.shouldSyncScale(syncInformation);
			bool flag4 = SyncProjectilesMessageFunctions.shouldSyncVelocity(syncInformation);
			bool flag5 = SyncProjectilesMessageFunctions.shouldSyncAngularVelocity(syncInformation);
			state.atPositionalRest = SyncProjectilesMessageFunctions.shouldBeAtPositionalRest(syncInformation);
			state.atRotationalRest = SyncProjectilesMessageFunctions.shouldBeAtRotationalRest(syncInformation);
			NetworkIdentity networkIdentity = reader.ReadNetworkIdentity();
			b += 4;
			if (networkIdentity == null)
			{
				reader.ReadBytes((int)(b2 - b));
				return networkStateMirror;
			}
			GameObject gameObject = networkIdentity.gameObject;
			if (!gameObject)
			{
				reader.ReadBytes((int)(b2 - b));
				return networkStateMirror;
			}
			networkStateMirror.smoothSync = gameObject.GetComponent<SmoothSyncMirror>();
			if (!networkStateMirror.smoothSync)
			{
				reader.ReadBytes((int)(b2 - b));
				return networkStateMirror;
			}
			int num = (int)reader.ReadUInt();
			for (int i = 0; i < networkStateMirror.smoothSync.childObjectSmoothSyncs.Length; i++)
			{
				if (networkStateMirror.smoothSync.childObjectSmoothSyncs[i].syncIndex == num)
				{
					networkStateMirror.smoothSync = networkStateMirror.smoothSync.childObjectSmoothSyncs[i];
					break;
				}
			}
			state.ownerTimestamp = reader.ReadFloat();
			SmoothSyncMirror smoothSync = networkStateMirror.smoothSync;
			state.receivedTimestamp = smoothSync.localTime;
			if (NetworkServer.active && !smoothSync.hasControl)
			{
				state.serverShouldRelayPosition = flag;
				state.serverShouldRelayRotation = flag2;
				state.serverShouldRelayScale = flag3;
				state.serverShouldRelayVelocity = flag4;
				state.serverShouldRelayAngularVelocity = flag5;
			}
			if ((float)smoothSync.receivedStatesCounter < smoothSync.sendRate)
			{
				smoothSync.receivedStatesCounter++;
			}
			if (flag)
			{
				if (smoothSync.isPositionCompressed)
				{
					if (smoothSync.isSyncingXPosition)
					{
						state.position.x = HalfHelper.Decompress(reader.ReadUShort());
					}
					if (smoothSync.isSyncingYPosition)
					{
						state.position.y = HalfHelper.Decompress(reader.ReadUShort());
					}
					if (smoothSync.isSyncingZPosition)
					{
						state.position.z = HalfHelper.Decompress(reader.ReadUShort());
					}
				}
				else
				{
					if (smoothSync.isSyncingXPosition)
					{
						state.position.x = reader.ReadFloat();
					}
					if (smoothSync.isSyncingYPosition)
					{
						state.position.y = reader.ReadFloat();
					}
					if (smoothSync.isSyncingZPosition)
					{
						state.position.z = reader.ReadFloat();
					}
				}
			}
			else if (smoothSync.stateCount > 0)
			{
				state.position = smoothSync.stateBuffer[0].position;
			}
			else
			{
				state.position = smoothSync.getPosition();
			}
			if (flag2)
			{
				state.reusableRotationVector = Vector3.zero;
				if (smoothSync.isRotationCompressed)
				{
					if (smoothSync.isSyncingXRotation)
					{
						state.reusableRotationVector.x = HalfHelper.Decompress(reader.ReadUShort());
						StateMirror stateMirror = state;
						stateMirror.reusableRotationVector.x = stateMirror.reusableRotationVector.x * 57.29578f;
					}
					if (smoothSync.isSyncingYRotation)
					{
						state.reusableRotationVector.y = HalfHelper.Decompress(reader.ReadUShort());
						StateMirror stateMirror2 = state;
						stateMirror2.reusableRotationVector.y = stateMirror2.reusableRotationVector.y * 57.29578f;
					}
					if (smoothSync.isSyncingZRotation)
					{
						state.reusableRotationVector.z = HalfHelper.Decompress(reader.ReadUShort());
						StateMirror stateMirror3 = state;
						stateMirror3.reusableRotationVector.z = stateMirror3.reusableRotationVector.z * 57.29578f;
					}
					state.rotation = Quaternion.Euler(state.reusableRotationVector);
				}
				else
				{
					if (smoothSync.isSyncingXRotation)
					{
						state.reusableRotationVector.x = reader.ReadFloat();
					}
					if (smoothSync.isSyncingYRotation)
					{
						state.reusableRotationVector.y = reader.ReadFloat();
					}
					if (smoothSync.isSyncingZRotation)
					{
						state.reusableRotationVector.z = reader.ReadFloat();
					}
					state.rotation = Quaternion.Euler(state.reusableRotationVector);
				}
			}
			else if (smoothSync.stateCount > 0)
			{
				state.rotation = smoothSync.stateBuffer[0].rotation;
			}
			else
			{
				state.rotation = smoothSync.getRotation();
			}
			if (flag3)
			{
				if (smoothSync.isScaleCompressed)
				{
					if (smoothSync.isSyncingXScale)
					{
						state.scale.x = HalfHelper.Decompress(reader.ReadUShort());
					}
					if (smoothSync.isSyncingYScale)
					{
						state.scale.y = HalfHelper.Decompress(reader.ReadUShort());
					}
					if (smoothSync.isSyncingZScale)
					{
						state.scale.z = HalfHelper.Decompress(reader.ReadUShort());
					}
				}
				else
				{
					if (smoothSync.isSyncingXScale)
					{
						state.scale.x = reader.ReadFloat();
					}
					if (smoothSync.isSyncingYScale)
					{
						state.scale.y = reader.ReadFloat();
					}
					if (smoothSync.isSyncingZScale)
					{
						state.scale.z = reader.ReadFloat();
					}
				}
			}
			else if (smoothSync.stateCount > 0)
			{
				state.scale = smoothSync.stateBuffer[0].scale;
			}
			else
			{
				state.scale = smoothSync.getScale();
			}
			if (flag4)
			{
				if (smoothSync.isVelocityCompressed)
				{
					if (smoothSync.isSyncingXVelocity)
					{
						state.velocity.x = HalfHelper.Decompress(reader.ReadUShort());
					}
					if (smoothSync.isSyncingYVelocity)
					{
						state.velocity.y = HalfHelper.Decompress(reader.ReadUShort());
					}
					if (smoothSync.isSyncingZVelocity)
					{
						state.velocity.z = HalfHelper.Decompress(reader.ReadUShort());
					}
				}
				else
				{
					if (smoothSync.isSyncingXVelocity)
					{
						state.velocity.x = reader.ReadFloat();
					}
					if (smoothSync.isSyncingYVelocity)
					{
						state.velocity.y = reader.ReadFloat();
					}
					if (smoothSync.isSyncingZVelocity)
					{
						state.velocity.z = reader.ReadFloat();
					}
				}
				smoothSync.latestReceivedVelocity = state.velocity;
			}
			else
			{
				state.velocity = smoothSync.latestReceivedVelocity;
			}
			if (flag5)
			{
				if (smoothSync.isAngularVelocityCompressed)
				{
					state.reusableRotationVector = Vector3.zero;
					if (smoothSync.isSyncingXAngularVelocity)
					{
						state.reusableRotationVector.x = HalfHelper.Decompress(reader.ReadUShort());
						StateMirror stateMirror4 = state;
						stateMirror4.reusableRotationVector.x = stateMirror4.reusableRotationVector.x * 57.29578f;
					}
					if (smoothSync.isSyncingYAngularVelocity)
					{
						state.reusableRotationVector.y = HalfHelper.Decompress(reader.ReadUShort());
						StateMirror stateMirror5 = state;
						stateMirror5.reusableRotationVector.y = stateMirror5.reusableRotationVector.y * 57.29578f;
					}
					if (smoothSync.isSyncingZAngularVelocity)
					{
						state.reusableRotationVector.z = HalfHelper.Decompress(reader.ReadUShort());
						StateMirror stateMirror6 = state;
						stateMirror6.reusableRotationVector.z = stateMirror6.reusableRotationVector.z * 57.29578f;
					}
					state.angularVelocity = state.reusableRotationVector;
				}
				else
				{
					if (smoothSync.isSyncingXAngularVelocity)
					{
						state.angularVelocity.x = reader.ReadFloat();
					}
					if (smoothSync.isSyncingYAngularVelocity)
					{
						state.angularVelocity.y = reader.ReadFloat();
					}
					if (smoothSync.isSyncingZAngularVelocity)
					{
						state.angularVelocity.z = reader.ReadFloat();
					}
				}
				smoothSync.latestReceivedAngularVelocity = state.angularVelocity;
			}
			else
			{
				state.angularVelocity = smoothSync.latestReceivedAngularVelocity;
			}
			if (smoothSync.isSmoothingAuthorityChanges && !NetworkServer.active)
			{
				smoothSync.ownerChangeIndicator = (int)reader.ReadByte();
			}
			if (smoothSync.automaticallyResetTime)
			{
				state.localTimeResetIndicator = (int)reader.ReadByte();
			}
			return networkStateMirror;
		}

		// Token: 0x06001CCC RID: 7372 RVA: 0x0007CBD0 File Offset: 0x0007ADD0
		private static byte encodeSyncInformation(bool sendPosition, bool sendRotation, bool sendScale, bool sendVelocity, bool sendAngularVelocity, bool atPositionalRest, bool atRotationalRest)
		{
			byte b = 0;
			if (sendPosition)
			{
				b |= 1;
			}
			if (sendRotation)
			{
				b |= 2;
			}
			if (sendScale)
			{
				b |= 4;
			}
			if (sendVelocity)
			{
				b |= 8;
			}
			if (sendAngularVelocity)
			{
				b |= 16;
			}
			if (atPositionalRest)
			{
				b |= 64;
			}
			if (atRotationalRest)
			{
				b |= 128;
			}
			return b;
		}

		// Token: 0x06001CCD RID: 7373 RVA: 0x0007CC21 File Offset: 0x0007AE21
		private static bool shouldSyncPosition(byte syncInformation)
		{
			return (syncInformation & 1) == 1;
		}

		// Token: 0x06001CCE RID: 7374 RVA: 0x0007CC2C File Offset: 0x0007AE2C
		private static bool shouldSyncRotation(byte syncInformation)
		{
			return (syncInformation & 2) == 2;
		}

		// Token: 0x06001CCF RID: 7375 RVA: 0x0007CC37 File Offset: 0x0007AE37
		private static bool shouldSyncScale(byte syncInformation)
		{
			return (syncInformation & 4) == 4;
		}

		// Token: 0x06001CD0 RID: 7376 RVA: 0x0007CC42 File Offset: 0x0007AE42
		private static bool shouldSyncVelocity(byte syncInformation)
		{
			return (syncInformation & 8) == 8;
		}

		// Token: 0x06001CD1 RID: 7377 RVA: 0x0007CC4D File Offset: 0x0007AE4D
		private static bool shouldSyncAngularVelocity(byte syncInformation)
		{
			return (syncInformation & 16) == 16;
		}

		// Token: 0x06001CD2 RID: 7378 RVA: 0x0007CC5A File Offset: 0x0007AE5A
		private static bool shouldBeAtPositionalRest(byte syncInformation)
		{
			return (syncInformation & 64) == 64;
		}

		// Token: 0x06001CD3 RID: 7379 RVA: 0x0007CC67 File Offset: 0x0007AE67
		private static bool shouldBeAtRotationalRest(byte syncInformation)
		{
			return (syncInformation & 128) == 128;
		}

		// Token: 0x04001359 RID: 4953
		private const byte positionMask = 1;

		// Token: 0x0400135A RID: 4954
		private const byte rotationMask = 2;

		// Token: 0x0400135B RID: 4955
		private const byte scaleMask = 4;

		// Token: 0x0400135C RID: 4956
		private const byte velocityMask = 8;

		// Token: 0x0400135D RID: 4957
		private const byte angularVelocityMask = 16;

		// Token: 0x0400135E RID: 4958
		private const byte atPositionalRestMask = 64;

		// Token: 0x0400135F RID: 4959
		private const byte atRotationalRestMask = 128;
	}
}
