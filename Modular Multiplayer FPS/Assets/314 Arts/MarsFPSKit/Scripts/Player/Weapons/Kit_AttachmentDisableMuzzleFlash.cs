using UnityEngine;

namespace MarsFPSKit
{
    namespace Weapons
    {
        /// <summary>
        /// This behaviour disabled the muzzle flash
        /// </summary>
        public class Kit_AttachmentDisableMuzzleFlash : Kit_AttachmentBehaviour
        {
            public override void Selected(Kit_PlayerBehaviour pb, AttachmentUseCase auc)
            {

            }

            public override void Unselected(Kit_PlayerBehaviour pb, AttachmentUseCase auc)
            {

            }
        }
    }
}
