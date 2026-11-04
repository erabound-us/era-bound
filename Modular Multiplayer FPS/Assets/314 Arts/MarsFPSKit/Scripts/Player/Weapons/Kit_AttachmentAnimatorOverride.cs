using UnityEngine;

namespace MarsFPSKit
{
    namespace Weapons
    {
        /// <summary>
        /// Changes the animator at runtime (e.g. useful for a grip)
        /// Thanks to Ciulama for sponsoring this!
        /// </summary>
        public class Kit_AttachmentAnimatorOverride : Kit_AttachmentBehaviour
        {
            /// <summary>
            /// Animator that will override the default one
            /// </summary>
            public RuntimeAnimatorController animatorOverride;

            public override void Selected(Kit_PlayerBehaviour pb, AttachmentUseCase auc)
            {

            }

            public override void Unselected(Kit_PlayerBehaviour pb, AttachmentUseCase auc)
            {

            }
        }
    }
}