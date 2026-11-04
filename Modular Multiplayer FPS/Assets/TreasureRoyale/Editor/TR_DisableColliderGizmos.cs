using UnityEngine;
using UnityEditor;
using System;
using System.Reflection;

namespace MarsFPSKit
{
    /// <summary>
    /// Disables physics collider gizmos in the Game view on editor load.
    /// Prevents black objects with white circles from appearing.
    /// </summary>
    [InitializeOnLoad]
    public static class TR_DisableColliderGizmos
    {
        static TR_DisableColliderGizmos()
        {
            EditorApplication.delayCall += DisableGizmos;
        }

        static void DisableGizmos()
        {
            try
            {
                var annotationUtility = Type.GetType("UnityEditor.AnnotationUtility, UnityEditor");
                if (annotationUtility == null) return;

                var getAnnotations = annotationUtility.GetMethod("GetAnnotations",
                    BindingFlags.Static | BindingFlags.NonPublic);
                var setGizmoEnabled = annotationUtility.GetMethod("SetGizmoEnabled",
                    BindingFlags.Static | BindingFlags.NonPublic);

                if (getAnnotations == null || setGizmoEnabled == null) return;

                var annotations = getAnnotations.Invoke(null, null);
                if (annotations == null) return;
                var annotationArray = (Array)annotations;

                foreach (var annotation in annotationArray)
                {
                    var annType = annotation.GetType();
                    var classIdField = annType.GetField("classID", BindingFlags.Public | BindingFlags.Instance);
                    var scriptClassField = annType.GetField("scriptClass", BindingFlags.Public | BindingFlags.Instance);
                    var gizmoEnabledField = annType.GetField("gizmoEnabled", BindingFlags.Public | BindingFlags.Instance);

                    if (classIdField == null) continue;
                    int classId = (int)classIdField.GetValue(annotation);
                    string scriptClass = scriptClassField != null ? (string)scriptClassField.GetValue(annotation) ?? "" : "";

                    // Unity class IDs: SphereCollider=135, CapsuleCollider=136, BoxCollider=65, MeshCollider=64
                    // CharacterController=111
                    bool isPhysicsCollider = classId == 135 || classId == 136 || classId == 65 || classId == 64;

                    if (isPhysicsCollider)
                    {
                        // Try the 4-parameter version first (Unity 2019+)
                        try
                        {
                            setGizmoEnabled.Invoke(null, new object[] { classId, scriptClass, 0, false });
                        }
                        catch
                        {
                            // Try 3-parameter version (older Unity)
                            try { setGizmoEnabled.Invoke(null, new object[] { classId, scriptClass, 0 }); }
                            catch { }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                // Log only in development — this is cosmetic only
                Debug.Log("[TR] DisableColliderGizmos: " + e.Message);
            }
        }
    }
}
