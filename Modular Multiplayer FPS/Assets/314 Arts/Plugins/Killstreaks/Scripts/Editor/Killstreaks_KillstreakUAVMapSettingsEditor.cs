using UnityEngine;
using UnityEditor;

namespace KillstreaksPlugin
{
    [CustomEditor(typeof(Killstreaks_KillstreakUAVMapSettings))]
    public class Killstreaks_KillstreakUAVMapSettingsEditor : Editor
    {
        protected void OnSceneGUI()
        {
            Killstreaks_KillstreakUAVMapSettings trgt = (Killstreaks_KillstreakUAVMapSettings)target;
            //We create these handles so that editing becomes easy
            float size = HandleUtility.GetHandleSize(trgt.startPosition.position) * 0.5f;
            Vector3 snap = Vector3.one * 0.5f;

            EditorGUI.BeginChangeCheck();
            Vector3 newTargetPosition = Handles.FreeMoveHandle(trgt.startPosition.position, Quaternion.identity, size, snap, Handles.RectangleHandleCap);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(trgt, "Change UAV Start Position");
                trgt.startPosition.position = newTargetPosition;
            }

            size = HandleUtility.GetHandleSize(trgt.circlingStartPosition.position) * 0.5f;
            EditorGUI.BeginChangeCheck();
            newTargetPosition = Handles.FreeMoveHandle(trgt.circlingStartPosition.position, Quaternion.identity, size, snap, Handles.RectangleHandleCap);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(trgt, "Change UAV Start Circling Position");
                trgt.circlingStartPosition.position = newTargetPosition;
            }

            size = HandleUtility.GetHandleSize(trgt.circlingEndPosition.position) * 0.5f;
            EditorGUI.BeginChangeCheck();
            newTargetPosition = Handles.FreeMoveHandle(trgt.circlingEndPosition.position, Quaternion.identity, size, snap, Handles.RectangleHandleCap);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(trgt, "Change UAV End Circling Position");
                trgt.circlingEndPosition.position = newTargetPosition;
            }

            size = HandleUtility.GetHandleSize(trgt.endPosition.position) * 0.5f;
            EditorGUI.BeginChangeCheck();
            newTargetPosition = Handles.FreeMoveHandle(trgt.endPosition.position, Quaternion.identity, size, snap, Handles.RectangleHandleCap);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(trgt, "Change UAV End Position");
                trgt.endPosition.position = newTargetPosition;
            }
        }
    }
}