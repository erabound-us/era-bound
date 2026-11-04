using UnityEngine;
using MarsFPSKit.Weapons;

namespace MarsFPSKit
{
    public class TR_FPArmHider : MonoBehaviour
    {
        void Awake()
        {
            foreach (var fpa in GetComponentsInChildren<Kit_FirstPersonArms>(true))
            {
                foreach (var r in fpa.renderers)
                    if (r) r.enabled = false;
                foreach (var r in fpa.GetComponentsInChildren<Renderer>(true))
                    r.enabled = false;
            }
        }
    }
}
