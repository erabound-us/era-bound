using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MarsFPSKit
{
    namespace Weapons
    {
        /// <summary>
        /// Contains information for attachment slots
        /// </summary>
        [System.Serializable]
        public class AttachmentSlot
        {
            /// <summary>
            /// Name of this slot
            /// </summary>
            public string name;

            /// <summary>
            /// The position for the UI dropdown for this slot
            /// </summary>
            public Transform uiPosition;

            /// <summary>
            /// All attachments in this slot
            /// </summary>
            public Attachment[] attachments;
        }

        /// <summary>
        /// An attachment that can be put in a slot
        /// </summary>
        [System.Serializable]
        public class Attachment
        {
            /// <summary>
            /// Name of this attachment
            /// </summary>
            public string name;

            /*
            [Tooltip("At which level should this attachment be unlocked at?")]
            /// <summary>
            /// At which level should this attachment be unlocked at?
            /// </summary>
            public int levelToUnlockAt = -1;
            [Tooltip("This image will be displayed when this is unlocked")]
            /// <summary>
            /// This image will be displayed when this is unlocked
            /// </summary>
            public Sprite unlockImage;
            */

            public Kit_AttachmentBehaviour[] attachmentBehaviours;
        }

        /// <summary>
        /// Enum used to describe attachment case
        /// </summary>
        public enum AttachmentUseCase { FirstPerson, ThirdPerson, Drop }

        public abstract class Kit_AttachmentBehaviour : MonoBehaviour
        {
            /// <summary>
            /// If this attachment requires syncing, the third person equivalent (where "SyncFromFirstPerson" will be called), will be assigned here by the system.
            /// </summary>
            public Kit_AttachmentBehaviour thirdPersonEquivalent;

            /// <summary>
            /// Does this attachment require syncing?
            /// </summary>
            /// <returns></returns>
            public virtual bool RequiresSyncing()
            {
                return false;
            }

            /// <summary>
            /// Call for sync
            /// </summary>
            /// <param name="stream"></param>
            /// <param name="info"></param>
            public virtual void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info, Kit_PlayerBehaviour pb, WeaponControllerRuntimeData data, int index)
            {

            }

            /// <summary>
            /// Does this behaviour require interaction ?
            /// </summary>
            /// <returns></returns>
            public virtual bool RequiresInteraction()
            {
                return false;
            }

            /// <summary>
            /// Interaction function
            /// </summary>
            /// <param name="pb"></param>
            public virtual void Interaction(Kit_PlayerBehaviour pb)
            {

            }

            /// <summary>
            /// For local sync in third person (and bots as master client), this is called from first person to sync.
            /// </summary>
            /// <param name="obj"></param>
            public virtual void SyncFromFirstPerson(object obj)
            {

            }

            /// <summary>
            /// Called when this attachment is selected
            /// </summary>
            public abstract void Selected(Kit_PlayerBehaviour pb, AttachmentUseCase auc);

            /// <summary>
            /// Called when this attachment is not selected
            /// </summary>
            public abstract void Unselected(Kit_PlayerBehaviour pb, AttachmentUseCase auc);

            /// <summary>
            /// Sets visibility if its selected
            /// </summary>
            /// <param name="visible"></param>
            public virtual void SetVisibility(Kit_PlayerBehaviour pb, AttachmentUseCase auc, bool visible)
            {

            }
        }
    }
}