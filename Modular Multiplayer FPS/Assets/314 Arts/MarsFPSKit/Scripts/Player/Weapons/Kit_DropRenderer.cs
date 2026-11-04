using System;
using UnityEngine;

namespace MarsFPSKit
{
    namespace Weapons
    {
        public class Kit_DropRenderer : MonoBehaviour
        {
            [Header("Attachments")]
            [Tooltip("Make sure they MATCH the first person attachment slots!")]
            public AttachmentSlot[] attachmentSlots;

            /// <summary>
            /// Enables the given attachments.
            /// </summary>
            /// <param name="enabledAttachments"></param>
            public void SetAttachments(int[] enabledAttachments)
            {
                try
                {
                    //Loop through all slots
                    for (int i = 0; i < enabledAttachments.Length; i++)
                    {
                        if (i < attachmentSlots.Length)
                        {
                            //Loop through all attachments for that slot

                            for (int o = 0; o < attachmentSlots[i].attachments.Length; o++)
                            {
                                //Check if this attachment is enabled
                                if (o == enabledAttachments[i])
                                {
                                    //Tell the behaviours they are active!
                                    for (int p = 0; p < attachmentSlots[i].attachments[o].attachmentBehaviours.Length; p++)
                                    {
                                        attachmentSlots[i].attachments[o].attachmentBehaviours[p].Selected(null, AttachmentUseCase.Drop);
                                    }
                                }
                                else
                                {
                                    //Tell the behaviours they are not active!
                                    for (int p = 0; p < attachmentSlots[i].attachments[o].attachmentBehaviours.Length; p++)
                                    {
                                        attachmentSlots[i].attachments[o].attachmentBehaviours[p].Unselected(null, AttachmentUseCase.Drop);
                                    }
                                }
                            }
                        }
                        else
                        {
                            Debug.LogWarning("Something must have gone wrong with the attachments. Enabled attachments is longer than all slots.");
                        }
                    }
                }
                catch (Exception e)
                {
                    Debug.LogError("There was an error with the attachments: " + e);
                }
            }
        }
    }
}
