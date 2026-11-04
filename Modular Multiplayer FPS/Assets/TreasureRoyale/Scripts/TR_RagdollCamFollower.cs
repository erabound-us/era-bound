using UnityEngine;

namespace MarsFPSKit
{
    public class TR_RagdollCamFollower : MonoBehaviour
    {
        public Transform target;
        public Vector3 offset = new Vector3(0f, 3f, -2f);

        void LateUpdate()
        {
            if (target == null) return;
            transform.position = target.position + offset;
            transform.LookAt(target.position);
        }

        void OnDestroy()
        {
            // Unparent the camera before this anchor is destroyed so the camera survives
            var main = FindObjectOfType<Kit_IngameMain>();
            if (main && main.mainCamera && main.mainCamera.transform.parent == transform)
                main.mainCamera.transform.parent = null;
        }
    }
}
