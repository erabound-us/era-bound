using Photon.Pun;
using UnityEngine;

namespace MarsFPSKit
{
    /// <summary>
    /// Use this class to create interactable objects in the world. These functions are called locally only. Always attach to root!
    /// </summary>
    public abstract class Kit_InteractableObject : MonoBehaviourPunCallbacks
    {
        /// <summary>
        /// Text that will be displayed to player
        /// </summary>
        public string interactionText;

        /// <summary>
        /// Called when looking at trigger and pressing interact button (default = f)
        /// </summary>
        /// <param name="who"></param>
        public abstract void Interact(Kit_PlayerBehaviour who);

        /// <summary>
        /// Can we interact with this object?
        /// </summary>
        /// <param name="who"></param>
        /// <returns></returns>
        public virtual bool CanInteract(Kit_PlayerBehaviour who)
        {
            return true;
        }
    }
}