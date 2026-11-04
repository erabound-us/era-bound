using UnityEngine;

namespace MarsFPSKit
{
    namespace Weapons
    {
        /// <summary>
        /// Player Model Dependent First Person Arms
        /// </summary>
        public class Kit_FirstPersonArms : MonoBehaviour
        {
            /// <summary>
            /// These transforms will get reparented
            /// </summary>
            public Transform[] reparents;
            /// <summary>
            /// Renderers of the arms
            /// </summary>
            public Renderer[] renderers;
        }
    }
}