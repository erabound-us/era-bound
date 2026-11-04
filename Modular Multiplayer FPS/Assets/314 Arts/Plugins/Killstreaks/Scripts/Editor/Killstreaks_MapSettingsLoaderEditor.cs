using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using System.Collections.Generic;

namespace KillstreaksPlugin
{
    [CustomEditor(typeof(Killstreaks_MapSettingsLoader))]
    public class Killstreaks_MapSettingsLoaderEditor : Editor
    {
        public bool checkedForData;


        public override void OnInspectorGUI()
        {
            Killstreaks_MapSettingsLoader trgt = (Killstreaks_MapSettingsLoader)target;

            DrawDefaultInspector();

            if (trgt.gameObject.scene == EditorSceneManager.GetActiveScene())
            {
                if (!checkedForData)
                {
                    if (!PlayerPrefs.HasKey(EditorSceneManager.GetActiveScene() + "klspsaved"))
                    {
                        Killstreaks_MapSettingsLoaderData[] data = Resources.LoadAll<Killstreaks_MapSettingsLoaderData>("SetupData");

                        for (int i = 0; i < data.Length; i++)
                        {
                            if (data[i].sceneName == EditorSceneManager.GetActiveScene().name)
                            {
                                if (EditorUtility.DisplayDialog("Map Data Found", "Killstreak map data for this map was found, do you want to load it?", "Yes", "no"))
                                {
                                    SerializedObject uavSerialized = new SerializedObject(trgt.uav);
                                    SerializedObject airstrikeSerialized = new SerializedObject(trgt.airstrike);
                                    SerializedObject helicopterSerialized = new SerializedObject(trgt.helicopter);

                                    Undo.RecordObject(trgt, "Map Data Load");
                                    Undo.RecordObject(trgt.uav, "Map Data Load UAV");

                                    //Copy Data
                                    trgt.uav.startPosition = data[i].uav.startPosition;
                                    trgt.uav.circlingStartPosition = data[i].uav.circlingStartPosition;
                                    trgt.uav.circlingEndPosition = data[i].uav.circlingEndPosition;
                                    trgt.uav.endPosition = data[i].uav.endPosition;
                                    trgt.uav.uavArrivalTime = data[i].uav.uavArrivalTime;
                                    trgt.uav.uavLifeTime = data[i].uav.uavLifeTime;
                                    trgt.uav.uavDepartureTime = data[i].uav.uavDepartureTime;
                                    trgt.uav.xpGainedUponDestruction = data[i].uav.xpGainedUponDestruction;
                                    trgt.uav.scannerStartPos = data[i].uav.scannerStartPos;
                                    trgt.uav.scannerEndPos = data[i].uav.scannerEndPos;
                                    trgt.uav.scannerTime = data[i].uav.scannerTime;

                                    Undo.RecordObject(trgt.helicopter, "Map Data Load Helicopter");
                                    trgt.helicopter.helicopterLiveTime = data[i].helicopter.helicopterLiveTime;
                                    trgt.helicopter.helicopterHoveringTime = data[i].helicopter.helicopterHoveringTime;
                                    trgt.helicopter.helicopterSmooth = data[i].helicopter.helicopterSmooth;
                                    trgt.helicopter.hoveringPoints = new Vector3[data[i].helicopter.hoveringPoints.Length];
                                    for (int o = 0; o < trgt.helicopter.hoveringPoints.Length; o++)
                                    {
                                        trgt.helicopter.hoveringPoints[o] = data[i].helicopter.hoveringPoints[o];
                                    }
                                    trgt.helicopter.xpGainedUponDestruction = data[i].helicopter.xpGainedUponDestruction;

                                    Undo.RecordObject(trgt.airstrike, "Map Data Load Airstrike");
                                    trgt.airstrike.planePrefab = data[i].airstrike.planePrefab;
                                    trgt.airstrike.amountOfPlanes = data[i].airstrike.amountOfPlanes;
                                    trgt.airstrike.planeSpawnTime = data[i].airstrike.planeSpawnTime;
                                    trgt.airstrike.planeAltitude = data[i].airstrike.planeAltitude;
                                    trgt.airstrike.planeSpeed = data[i].airstrike.planeSpeed;
                                    trgt.airstrike.spawnPointDistance = data[i].airstrike.spawnPointDistance;
                                    trgt.airstrike.distanceAtWhichDropStats = data[i].airstrike.distanceAtWhichDropStats;
                                    trgt.airstrike.amountOfBombs = data[i].airstrike.amountOfBombs;
                                    trgt.airstrike.timeBetweenBombDrops = data[i].airstrike.timeBetweenBombDrops;
                                    trgt.airstrike.bombPrefab = data[i].airstrike.bombPrefab;
                                    trgt.airstrike.bombDropStartVelocity = data[i].airstrike.bombDropStartVelocity;
                                    trgt.airstrike.xpGainedUponDestruction = data[i].airstrike.xpGainedUponDestruction;

                                    uavSerialized.ApplyModifiedProperties();
                                    airstrikeSerialized.ApplyModifiedProperties();
                                    helicopterSerialized.ApplyModifiedProperties();
                                    serializedObject.ApplyModifiedProperties();

                                    EditorUtility.SetDirty(trgt.helicopter);

                                    checkedForData = true;
                                    PlayerPrefs.SetString(EditorSceneManager.GetActiveScene() + "klspsaved", EditorSceneManager.GetActiveScene() + "ksl");

                                    EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo();
                                    break;
                                }
                                else
                                {
                                    checkedForData = true;
                                    PlayerPrefs.SetString(EditorSceneManager.GetActiveScene() + "klspsaved", EditorSceneManager.GetActiveScene() + "ksl");
                                }
                            }
                        }
                    }
                }
            }
            if (trgt.gameObject.scene == EditorSceneManager.GetActiveScene())
            {
                if (GUILayout.Button("Search for Map Data"))
                {
                    Killstreaks_MapSettingsLoaderData[] data = Resources.LoadAll<Killstreaks_MapSettingsLoaderData>("SetupData");

                    for (int i = 0; i < data.Length; i++)
                    {
                        if (data[i].sceneName == EditorSceneManager.GetActiveScene().name)
                        {
                            if (EditorUtility.DisplayDialog("Map Data Found", "Killstreak map data for this map was found, do you want to load it?", "Yes", "no"))
                            {
                                SerializedObject uavSerialized = new SerializedObject(trgt.uav);
                                SerializedObject airstrikeSerialized = new SerializedObject(trgt.airstrike);
                                SerializedObject helicopterSerialized = new SerializedObject(trgt.helicopter);

                                Undo.RecordObject(trgt, "Map Data Load");
                                Undo.RecordObject(trgt.uav, "Map Data Load UAV");

                                //Copy Data
                                trgt.uav.startPosition = data[i].uav.startPosition;
                                trgt.uav.circlingStartPosition = data[i].uav.circlingStartPosition;
                                trgt.uav.circlingEndPosition = data[i].uav.circlingEndPosition;
                                trgt.uav.endPosition = data[i].uav.endPosition;
                                trgt.uav.uavArrivalTime = data[i].uav.uavArrivalTime;
                                trgt.uav.uavLifeTime = data[i].uav.uavLifeTime;
                                trgt.uav.uavDepartureTime = data[i].uav.uavDepartureTime;
                                trgt.uav.xpGainedUponDestruction = data[i].uav.xpGainedUponDestruction;
                                trgt.uav.scannerStartPos = data[i].uav.scannerStartPos;
                                trgt.uav.scannerEndPos = data[i].uav.scannerEndPos;
                                trgt.uav.scannerTime = data[i].uav.scannerTime;

                                Undo.RecordObject(trgt.helicopter, "Map Data Load Helicopter");
                                trgt.helicopter.helicopterLiveTime = data[i].helicopter.helicopterLiveTime;
                                trgt.helicopter.helicopterHoveringTime = data[i].helicopter.helicopterHoveringTime;
                                trgt.helicopter.helicopterSmooth = data[i].helicopter.helicopterSmooth;
                                trgt.helicopter.hoveringPoints = new Vector3[data[i].helicopter.hoveringPoints.Length];
                                for (int o = 0; o < trgt.helicopter.hoveringPoints.Length; o++)
                                {
                                    trgt.helicopter.hoveringPoints[o] = data[i].helicopter.hoveringPoints[o];
                                }
                                trgt.helicopter.xpGainedUponDestruction = data[i].helicopter.xpGainedUponDestruction;

                                Undo.RecordObject(trgt.airstrike, "Map Data Load Airstrike");
                                trgt.airstrike.planePrefab = data[i].airstrike.planePrefab;
                                trgt.airstrike.amountOfPlanes = data[i].airstrike.amountOfPlanes;
                                trgt.airstrike.planeSpawnTime = data[i].airstrike.planeSpawnTime;
                                trgt.airstrike.planeAltitude = data[i].airstrike.planeAltitude;
                                trgt.airstrike.planeSpeed = data[i].airstrike.planeSpeed;
                                trgt.airstrike.spawnPointDistance = data[i].airstrike.spawnPointDistance;
                                trgt.airstrike.distanceAtWhichDropStats = data[i].airstrike.distanceAtWhichDropStats;
                                trgt.airstrike.amountOfBombs = data[i].airstrike.amountOfBombs;
                                trgt.airstrike.timeBetweenBombDrops = data[i].airstrike.timeBetweenBombDrops;
                                trgt.airstrike.bombPrefab = data[i].airstrike.bombPrefab;
                                trgt.airstrike.bombDropStartVelocity = data[i].airstrike.bombDropStartVelocity;
                                trgt.airstrike.xpGainedUponDestruction = data[i].airstrike.xpGainedUponDestruction;

                                uavSerialized.ApplyModifiedProperties();
                                airstrikeSerialized.ApplyModifiedProperties();
                                helicopterSerialized.ApplyModifiedProperties();
                                serializedObject.ApplyModifiedProperties();

                                EditorUtility.SetDirty(trgt.helicopter);

                                EditorSceneManager.MarkAllScenesDirty();
                                checkedForData = true;

                                EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo();

                                break;
                            }
                        }
                    }
                }

                if (GUILayout.Button("Save Map Settings"))
                {
                    Killstreaks_MapSettingsLoaderData asset = ScriptableObject.CreateInstance<Killstreaks_MapSettingsLoaderData>();

                    string path = EditorSceneManager.GetActiveScene().path;
                    path = path.Replace(EditorSceneManager.GetActiveScene().name + ".unity", "");
                    path = path + EditorSceneManager.GetActiveScene().name + "_MapSettings.asset";


                    AssetDatabase.CreateAsset(asset, path);
                    AssetDatabase.SaveAssets();

                    EditorUtility.FocusProjectWindow();

                    Selection.activeObject = asset;

                    asset.sceneName = EditorSceneManager.GetActiveScene().name;

                    //Copy Data
                    asset.uav.startPosition = trgt.uav.startPosition;
                    asset.uav.circlingStartPosition = trgt.uav.circlingStartPosition;
                    asset.uav.circlingEndPosition = trgt.uav.circlingEndPosition;
                    asset.uav.endPosition = trgt.uav.endPosition;
                    asset.uav.uavArrivalTime = trgt.uav.uavArrivalTime;
                    asset.uav.uavLifeTime = trgt.uav.uavLifeTime;
                    asset.uav.uavDepartureTime = trgt.uav.uavDepartureTime;
                    asset.uav.xpGainedUponDestruction = trgt.uav.xpGainedUponDestruction;
                    asset.uav.scannerStartPos = trgt.uav.scannerStartPos;
                    asset.uav.scannerEndPos = trgt.uav.scannerEndPos;
                    asset.uav.scannerTime = trgt.uav.scannerTime;


                    asset.helicopter.helicopterLiveTime = trgt.helicopter.helicopterLiveTime;
                    asset.helicopter.helicopterHoveringTime = trgt.helicopter.helicopterHoveringTime;
                    asset.helicopter.helicopterSmooth = trgt.helicopter.helicopterSmooth;
                    asset.helicopter.hoveringPoints = trgt.helicopter.hoveringPoints;
                    asset.helicopter.xpGainedUponDestruction = trgt.helicopter.xpGainedUponDestruction;

                    asset.airstrike.planePrefab = trgt.airstrike.planePrefab;
                    asset.airstrike.amountOfPlanes = trgt.airstrike.amountOfPlanes;
                    asset.airstrike.planeSpawnTime = trgt.airstrike.planeSpawnTime;
                    asset.airstrike.planeAltitude = trgt.airstrike.planeAltitude;
                    asset.airstrike.planeSpeed = trgt.airstrike.planeSpeed;
                    asset.airstrike.spawnPointDistance = trgt.airstrike.spawnPointDistance;
                    asset.airstrike.distanceAtWhichDropStats = trgt.airstrike.distanceAtWhichDropStats;
                    asset.airstrike.amountOfBombs = trgt.airstrike.amountOfBombs;
                    asset.airstrike.timeBetweenBombDrops = trgt.airstrike.timeBetweenBombDrops;
                    asset.airstrike.bombPrefab = trgt.airstrike.bombPrefab;
                    asset.airstrike.bombDropStartVelocity = trgt.airstrike.bombDropStartVelocity;
                    asset.airstrike.xpGainedUponDestruction = trgt.airstrike.xpGainedUponDestruction;
                }
            }
        }
    }
}