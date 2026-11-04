using UnityEngine;
using UnityEditor;

namespace KillstreaksPlugin
{
    [CustomEditor(typeof(Killstreaks_KillstreakHelicopterMapSettings))]
    public class Killstreaks_KillstreakHelicopterMapSettingsEditor : Editor
    {
        protected void OnSceneGUI()
        {
            Killstreaks_KillstreakHelicopterMapSettings trgt = (Killstreaks_KillstreakHelicopterMapSettings)target;
            //We create these handles so that editing becomes easy
            float size = 0f;
            Vector3 snap = Vector3.one * 0.5f;

            for (int i = 0; i < trgt.hoveringPoints.Length; i++)
            {
                EditorGUI.BeginChangeCheck();
                size = HandleUtility.GetHandleSize(trgt.hoveringPoints[i]) * 0.5f;
                Vector3 newTargetPosition = Handles.FreeMoveHandle(trgt.hoveringPoints[i], Quaternion.identity, size, snap, Handles.RectangleHandleCap);
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(trgt, "Change Helicopter Hovering Position " + i);
                    trgt.hoveringPoints[i] = newTargetPosition;
                }
            }
        }
    }
}