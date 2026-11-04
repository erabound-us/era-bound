using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways]
public class Water_Settings : MonoBehaviour
{
    Material waterVolume;
    Material waterMaterial;

    void Update()
    {
        if(waterVolume == null)
        {
            waterVolume = (Material)Resources.Load("Water_Volume");
        }

        if (waterMaterial == null)
        {
            waterMaterial = GetComponent<MeshRenderer>().sharedMaterial;
        }

        waterVolume.SetVector("pos", new Vector4(0, (waterVolume.HasProperty("bounds") ? waterVolume.GetVector("bounds").y / -2 : 0) + transform.position.y + (waterMaterial.HasProperty("_Displacement_Amount") ? waterMaterial.GetFloat("_Displacement_Amount") / 3 : 0), 0, 0));
    }
}
