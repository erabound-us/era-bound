using UnityEngine;

namespace MarsFPSKit
{
    namespace Weapons
    {
        public class Kit_AttachmentAimOverride : Kit_AttachmentBehaviour
        {
            /// <summary>
            /// New Aiming position
            /// </summary>
            public Vector3 aimPos;
            /// <summary>
            /// New Aiming Rotation
            /// </summary>
            public Vector3 aimRot;
            /// <summary>
            /// New Aiming FOV
            /// </summary>
            public float aimFov = 40f;
            /// <summary>
            /// Defines if this should use fullscreen aiming
            /// </summary>
            public bool useFullscreenScope;

            public override void Selected(Kit_PlayerBehaviour pb, AttachmentUseCase auc)
            {

            }

            public override void Unselected(Kit_PlayerBehaviour pb, AttachmentUseCase auc)
            {

            }
        }
    }
}
