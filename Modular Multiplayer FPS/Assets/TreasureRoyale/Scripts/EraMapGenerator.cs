using Photon.Pun;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;
using System.Collections.Generic;

namespace MarsFPSKit
{
    [DisallowMultipleComponent]
    public class EraMapGenerator : MonoBehaviour
    {
        public Transform structureRoot;
        public EraAssetConfig assetConfig;
        const float VILLAGE = 2000f;
        const float MAP     = 8000f;
        const float PLAY    = 800f;  // actual playable radius
        const float WT      = 0.22f;
        public static float riverWidth;
        public static float riverAngle;
        public static bool hasRiver;
        public static bool mapGenerationComplete;
        public static Color _minimapGroundColor;
        public static bool _minimapGroundColorSet;
        bool _hasRun;

        struct EraTheme
        {
            public Color sky, fog, ambient, ground, groundAlt, groundDetail,
                         wall, wallAlt, accent, water, tree, rock, cloud;
            public float fogDensity;
            public bool hasTrees, hasRocks, hasWater, hasBridge;
            public string style;
        }

        static EraTheme Theme(string era)
        {
            switch (era)
            {
                case "deep_prehistoric":
                    return new EraTheme { sky=C(.32f,.52f,.78f),fog=C(.38f,.55f,.38f),fogDensity=.018f,
                        ambient=C(.26f,.40f,.20f),ground=C(.16f,.26f,.10f),groundAlt=C(.22f,.32f,.14f),groundDetail=C(.30f,.22f,.10f),
                        wall=C(.46f,.38f,.24f),wallAlt=C(.38f,.30f,.18f),accent=C(.28f,.20f,.10f),
                        water=C(.14f,.32f,.52f),tree=C(.12f,.28f,.08f),rock=C(.40f,.36f,.30f),cloud=C(.78f,.82f,.80f),
                        hasTrees=true,hasRocks=true,hasWater=true,hasBridge=false,style="prehistoric" };
                case "prehistoric":
                    return new EraTheme { sky=C(.46f,.60f,.80f),fog=C(.52f,.60f,.42f),fogDensity=.012f,
                        ambient=C(.36f,.42f,.26f),ground=C(.26f,.36f,.16f),groundAlt=C(.32f,.42f,.20f),groundDetail=C(.38f,.28f,.12f),
                        wall=C(.52f,.46f,.30f),wallAlt=C(.44f,.38f,.22f),accent=C(.36f,.26f,.14f),
                        water=C(.16f,.36f,.56f),tree=C(.14f,.30f,.10f),rock=C(.44f,.40f,.32f),cloud=C(.88f,.90f,.88f),
                        hasTrees=true,hasRocks=true,hasWater=true,hasBridge=false,style="prehistoric" };
                case "ancient":
                    return new EraTheme { sky=C(.60f,.74f,.92f),fog=C(.78f,.72f,.54f),fogDensity=.005f,
                        ambient=C(.58f,.52f,.34f),ground=C(.68f,.58f,.36f),groundAlt=C(.74f,.64f,.42f),groundDetail=C(.56f,.44f,.24f),
                        wall=C(.84f,.78f,.58f),wallAlt=C(.76f,.68f,.46f),accent=C(.70f,.50f,.20f),
                        water=C(.20f,.46f,.66f),tree=C(.28f,.42f,.16f),rock=C(.62f,.56f,.40f),cloud=C(.96f,.94f,.88f),
                        hasTrees=false,hasRocks=true,hasWater=true,hasBridge=true,style="ancient" };
                case "medieval":
                    return new EraTheme { sky=C(.38f,.48f,.64f),fog=C(.44f,.44f,.48f),fogDensity=.012f,
                        ambient=C(.28f,.28f,.34f),ground=C(.20f,.24f,.14f),groundAlt=C(.26f,.30f,.18f),groundDetail=C(.32f,.26f,.16f),
                        wall=C(.48f,.46f,.42f),wallAlt=C(.40f,.38f,.34f),accent=C(.34f,.32f,.28f),
                        water=C(.14f,.30f,.48f),tree=C(.16f,.28f,.10f),rock=C(.42f,.40f,.36f),cloud=C(.72f,.74f,.78f),
                        hasTrees=true,hasRocks=true,hasWater=true,hasBridge=true,style="medieval" };
                case "renaissance":
                    return new EraTheme { sky=C(.54f,.68f,.88f),fog=C(.68f,.66f,.58f),fogDensity=.004f,
                        ambient=C(.54f,.50f,.40f),ground=C(.44f,.40f,.28f),groundAlt=C(.50f,.46f,.34f),groundDetail=C(.36f,.30f,.18f),
                        wall=C(.78f,.72f,.54f),wallAlt=C(.68f,.62f,.44f),accent=C(.58f,.48f,.26f),
                        water=C(.18f,.42f,.62f),tree=C(.22f,.36f,.14f),rock=C(.56f,.50f,.38f),cloud=C(.94f,.94f,.96f),
                        hasTrees=true,hasRocks=false,hasWater=true,hasBridge=true,style="renaissance" };
                case "industrial":
                    return new EraTheme { sky=C(.34f,.34f,.38f),fog=C(.38f,.36f,.32f),fogDensity=.022f,
                        ambient=C(.24f,.22f,.18f),ground=C(.24f,.22f,.18f),groundAlt=C(.30f,.28f,.24f),groundDetail=C(.20f,.18f,.14f),
                        wall=C(.32f,.30f,.26f),wallAlt=C(.40f,.36f,.28f),accent=C(.50f,.38f,.16f),
                        water=C(.12f,.24f,.36f),tree=C(.18f,.22f,.12f),rock=C(.36f,.34f,.30f),cloud=C(.52f,.50f,.48f),
                        hasTrees=false,hasRocks=true,hasWater=true,hasBridge=true,style="industrial" };
                case "modern":
                    return new EraTheme { sky=C(.48f,.62f,.82f),fog=C(.54f,.58f,.64f),fogDensity=.003f,
                        ambient=C(.48f,.50f,.54f),ground=C(.28f,.28f,.28f),groundAlt=C(.34f,.34f,.34f),groundDetail=C(.22f,.20f,.18f),
                        wall=C(.54f,.54f,.58f),wallAlt=C(.44f,.44f,.48f),accent=C(.26f,.50f,.76f),
                        water=C(.16f,.38f,.60f),tree=C(.20f,.34f,.14f),rock=C(.38f,.38f,.40f),cloud=C(.90f,.92f,.96f),
                        hasTrees=true,hasRocks=false,hasWater=true,hasBridge=true,style="modern" };
                case "near_future": case "mid_future":
                    return new EraTheme { sky=C(.08f,.14f,.30f),fog=C(.06f,.12f,.28f),fogDensity=.010f,
                        ambient=C(.10f,.18f,.40f),ground=C(.08f,.10f,.18f),groundAlt=C(.12f,.14f,.24f),groundDetail=C(.06f,.08f,.14f),
                        wall=C(.18f,.26f,.48f),wallAlt=C(.24f,.32f,.56f),accent=C(.16f,.76f,.86f),
                        water=C(.06f,.20f,.46f),tree=C(.10f,.28f,.20f),rock=C(.20f,.24f,.40f),cloud=C(.14f,.20f,.40f),
                        hasTrees=false,hasRocks=true,hasWater=false,hasBridge=false,style="future" };
                default:
                    return new EraTheme { sky=C(.03f,.03f,.10f),fog=C(.03f,.05f,.16f),fogDensity=.014f,
                        ambient=C(.06f,.06f,.20f),ground=C(.05f,.05f,.10f),groundAlt=C(.08f,.08f,.16f),groundDetail=C(.04f,.04f,.08f),
                        wall=C(.14f,.18f,.44f),wallAlt=C(.20f,.24f,.52f),accent=C(.56f,.16f,.86f),
                        water=C(.05f,.10f,.36f),tree=C(.08f,.20f,.30f),rock=C(.16f,.18f,.38f),cloud=C(.10f,.12f,.28f),
                        hasTrees=false,hasRocks=true,hasWater=false,hasBridge=false,style="future" };
            }
        }
        static Color C(float r,float g,float b)=>new Color(r,g,b);

        void Start()
        {
            if (_hasRun) return;
            _hasRun = true;
            mapGenerationComplete = false;
            if (!gameObject.activeSelf) return;
            string era = "medieval"; int seed = 0;
            var props = PhotonNetwork.InRoom ? PhotonNetwork.CurrentRoom.CustomProperties : TR_OriginTimelineManager.OfflineMatchProps;
            if (props == null)
            {
                var ht = new ExitGames.Client.Photon.Hashtable();
                ht["gameModeType"] = 1; ht["gameMode"] = 0; ht["era"] = era;
                ht["universeSeed"] = UnityEngine.Random.Range(1, 999999); ht["anchorYear"] = 0;
                TR_OriginTimelineManager.OfflineMatchProps = ht;
                TR_OriginTimelineManager.IsOfflineMatch = true;
                props = ht;
                Debug.Log("[TR] EraMapGenerator: auto-bootstrapped offline medieval match");
            }
            if (props.ContainsKey("era")) era = (string)props["era"];
            if (props.ContainsKey("universeSeed")) seed = (int)props["universeSeed"];
            if (seed == 0) seed = UnityEngine.Random.Range(1, 999999);
            if (assetConfig == null)
            {
                assetConfig = Resources.FindObjectsOfTypeAll<EraAssetConfig>().FirstOrDefault();
                if (assetConfig == null) assetConfig = UnityEngine.Object.FindObjectOfType<EraAssetConfig>();
                if (assetConfig == null) assetConfig = UnityEngine.Resources.Load<EraAssetConfig>("EraAssetConfig");
                if (assetConfig != null) Debug.Log("[TR] EraMapGenerator: auto-wired EraAssetConfig");
                else Debug.LogWarning("[TR] EraMapGenerator: EraAssetConfig not found");
            }
            Apply(era, seed);
            if (GetComponent<TR_ParticleDiagnostic>() == null) gameObject.AddComponent<TR_ParticleDiagnostic>();
            try { DestroyKitPlanes(); } catch (System.Exception e) { Debug.LogError("[TR] DestroyKitPlanes: " + e.Message); }
            try { HideKitRenderers(); } catch (System.Exception e) { Debug.LogError("[TR] HideKitRenderers: " + e.Message); }
            try { DisableKitTriggers(); } catch (System.Exception e) { Debug.LogError("[TR] DisableKitTriggers: " + e.Message); }
            var eraGround = GameObject.Find("ERA_Ground");
            var terrain = Object.FindObjectOfType<Terrain>();
        }

        void DestroyKitPlanes()
        {
            int count = 0;
            foreach (var t in Object.FindObjectsOfType<Transform>())
            {
                var n = t.name;
                if (n == "Plane" || System.Text.RegularExpressions.Regex.IsMatch(n, @"^Plane \(\d+\)$"))
                { Destroy(t.gameObject); count++; }
            }
            Debug.Log($"[TR] Destroyed {count} kit Plane GOs");
        }

        void HideKitRenderers()
        {
            var keepRoots = new System.Collections.Generic.HashSet<string> {
                "Directional Light", "Reflection Probe", "Game Config",
                "_RuntimePinkFixer", "EraMapGenerator", "ERA_Structures", "SUIMONO_Module",
                "Pool", "_Projectiles", "_BulletMarks", "_ImpactEffects"
            };
            int hiddenCount = 0;
            foreach (var r in Object.FindObjectsOfType<Renderer>())
            {
                bool keep = false;
                Transform p = r.transform;
                while (p != null) { if (p.name.StartsWith("ERA_") || keepRoots.Contains(p.name)) { keep = true; break; } p = p.parent; }
                if (!keep) { r.enabled = false; hiddenCount++; }
            }
            // Disable all particle system renderers not under ERA_ — they render as black quads
            foreach (var root2 in UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects())
            {
                if (root2.name.StartsWith("ERA_") || keepRoots.Contains(root2.name)) continue;
                if (root2.activeSelf) continue; // skip active roots — their PSRs may be needed at runtime
                foreach (var psr in root2.GetComponentsInChildren<ParticleSystemRenderer>(true))
                    psr.enabled = false;
            }
            var terrain = Object.FindObjectOfType<Terrain>();
            if (terrain) terrain.enabled = false;
            var terrainCollider = terrain ? terrain.GetComponent<TerrainCollider>() : null;
            if (terrainCollider) terrainCollider.enabled = false;
            foreach (var c in Object.FindObjectsOfType<MeshCollider>())
            {
                bool keep = false;
                Transform p = c.transform;
                while (p != null) { if (p.name.StartsWith("ERA_") || keepRoots.Contains(p.name)) { keep = true; break; } p = p.parent; }
                if (!keep) c.enabled = false;
            }
            Debug.Log($"[TR] Hidden {hiddenCount} original scene renderers");
        }

        void DisableKitTriggers()
        {
            foreach (var name in new[] {
                "Leaving Battlefield", "Kill On Exit", "Kill On Enter", "Damage On Stay", "Damage On Enter",
                "_BulletPenetration", "_Doors", "Domination Flags", "Bot Nav Points", "_WeaponSpawner",
                "_AmmoSpawner", "_HealthSpawner", "ExplosiveBarrels", "_Tech", "Minimap Scene",
                "Deathmatch, Team Deathmatch, Gun Game Spawns", "Backup Spawns", "Audio"
            })
            { var go = GameObject.Find(name); if (go != null) go.SetActive(false); }
        }

        public void Apply(string era, int seed)
        {
            Debug.Log($"[TR] EraMapGenerator.Apply era={era} seed={seed}");
            var rng=new System.Random(seed); var t=Theme(era);
            ApplyEnvironment(t); GenerateMap(era,t,rng,seed);
            _minimapGroundColor = t.ground; _minimapGroundColorSet = true;
            mapGenerationComplete = true;
        }
        void ApplyEnvironment(EraTheme t)
        {
            RenderSettings.skybox=BuildSkyboxMaterial(t);
            RenderSettings.ambientMode=AmbientMode.Flat;
            RenderSettings.ambientLight=new Color(Mathf.Min(1f,t.ambient.r+0.45f),Mathf.Min(1f,t.ambient.g+0.45f),Mathf.Min(1f,t.ambient.b+0.45f));
            RenderSettings.fogColor=t.fog;
            RenderSettings.fogDensity=t.fogDensity;
            RenderSettings.fog=true;
            foreach(var cam in Camera.allCameras)
            {cam.clearFlags=CameraClearFlags.Skybox;cam.backgroundColor=t.sky;}
        }
        // Procedural skybox using Unity built-in "Skybox/Procedural" shader
        static Material BuildSkyboxMaterial(EraTheme t)
        {
            var mat=new Material(Shader.Find("Skybox/Procedural"));
            if(mat.shader.name=="Hidden/InternalErrorShader")
            {
                // Fallback: solid color skybox
                mat=new Material(Shader.Find("Skybox/Panoramic"));
                if(mat.shader.name=="Hidden/InternalErrorShader") return null;
            }
            mat.SetFloat("_SunSize",0.04f);
            mat.SetFloat("_SunSizeConvergence",5f);
            mat.SetFloat("_AtmosphereThickness",1.0f);
            mat.SetColor("_SkyTint",t.sky);
            mat.SetColor("_GroundColor",t.ground*0.6f);
            mat.SetFloat("_Exposure",1.2f);
            return mat;
        }
        static float R(System.Random rng,float a,float b)=>a+(float)rng.NextDouble()*(b-a);
        static int RI(System.Random rng,int a,int b)=>rng.Next(a,b);
        static Color Tint(Color c,System.Random rng,float range=0.05f)
        {float v=R(rng,-range,range);return new Color(Mathf.Clamp01(c.r+v),Mathf.Clamp01(c.g+v),Mathf.Clamp01(c.b+v));}
        static GameObject Prim(PrimitiveType type,Transform parent,string name,Vector3 pos,Vector3 scale,Color col,float yRot=0f)
        {
            var go=GameObject.CreatePrimitive(type); go.name=name;
            go.transform.SetParent(parent,false); go.transform.localPosition=pos; go.transform.localScale=scale;
            if(yRot!=0f)go.transform.localRotation=Quaternion.Euler(0,yRot,0);
            var mat=new Material(Shader.Find("Unlit/Color")); mat.color=col;
            var rend=go.GetComponent<Renderer>(); rend.sharedMaterial=mat;
            var col2=go.GetComponent<Collider>(); if(col2!=null) DestroyImmediate(col2);
            return go;
        }

        static void MakeUnlit(GameObject go)
        {
            var unlitTex = Shader.Find("Unlit/Texture"); var unlitCol = Shader.Find("Unlit/Color");
            if (unlitTex == null || unlitCol == null) return;
            foreach (var r in go.GetComponentsInChildren<Renderer>(true))
            {
                var sharedMats = r.sharedMaterials; var mats = new Material[sharedMats.Length]; bool changed = false;
                for (int i = 0; i < sharedMats.Length; i++)
                {
                    if (sharedMats[i] == null) { mats[i] = sharedMats[i]; continue; }
                    if (sharedMats[i].shader.name == "Unlit/Texture" || sharedMats[i].shader.name == "Unlit/Color") { mats[i] = sharedMats[i]; continue; }
                    if (sharedMats[i].shader.name.StartsWith("Shader Graphs/")) { mats[i] = sharedMats[i]; continue; }
                    Texture tex = null;
                    foreach (var prop in new[]{"_BaseMap","_MainTex","_Albedo","_Diffuse"})
                    { if (sharedMats[i].HasProperty(prop)) { tex = sharedMats[i].GetTexture(prop); if (tex != null) break; } }
                    if (tex == null) { try { tex = sharedMats[i].mainTexture; } catch { tex = null; } }
                    if (tex != null) { var m = new Material(unlitTex); m.SetTexture("_MainTex", tex); mats[i] = m; }
                    else { Color c2 = sharedMats[i].HasProperty("_BaseColor") ? sharedMats[i].GetColor("_BaseColor") : sharedMats[i].HasProperty("_Color") ? sharedMats[i].GetColor("_Color") : Color.grey; var m = new Material(unlitCol); m.color = c2; mats[i] = m; }
                    changed = true;
                }
                for (int i = 0; i < mats.Length; i++) if (mats[i] == null) mats[i] = sharedMats[i];
                if (changed) r.materials = mats;
            }
        }

        static void AddColliders(GameObject go, bool isRock = false)
        {
            if (go.GetComponentInChildren<Collider>() != null) return;
            if (go.GetComponentInChildren<MeshRenderer>() == null) return;
            var bounds = new Bounds(go.transform.position, Vector3.zero);
            foreach (var r in go.GetComponentsInChildren<Renderer>()) bounds.Encapsulate(r.bounds);
            Vector3 localCenter = go.transform.InverseTransformPoint(bounds.center);
            Vector3 s = go.transform.lossyScale;
            Vector3 localSize = new Vector3(
                Mathf.Abs(s.x) > 0.0001f ? bounds.size.x / Mathf.Abs(s.x) : bounds.size.x,
                Mathf.Abs(s.y) > 0.0001f ? bounds.size.y / Mathf.Abs(s.y) : bounds.size.y,
                Mathf.Abs(s.z) > 0.0001f ? bounds.size.z / Mathf.Abs(s.z) : bounds.size.z);
            if (isRock) { var bc = go.AddComponent<BoxCollider>(); bc.center = localCenter; bc.size = localSize; }
            else { var cap = go.AddComponent<CapsuleCollider>(); cap.center = localCenter; cap.height = localSize.y; cap.radius = Mathf.Max(0.3f, Mathf.Min(localSize.x, localSize.z) * 0.5f); }
        }
        static bool Overlaps(List<Rect> list,Rect r,float pad)
        {var p=new Rect(r.x-pad,r.y-pad,r.width+pad*2,r.height+pad*2);foreach(var e in list)if(p.Overlaps(e))return true;return false;}
        static float Noise(int x,int z,int seed){int n=x+z*57+seed*131;n=(n<<13)^n;return 1f-((n*(n*n*15731+789221)+1376312589)&0x7fffffff)/1073741824f;}
        static float SN(float x,float z,int seed){int ix=Mathf.FloorToInt(x),iz=Mathf.FloorToInt(z);float fx=x-ix,fz=z-iz;fx=fx*fx*(3f-2f*fx);fz=fz*fz*(3f-2f*fz);return Mathf.Lerp(Mathf.Lerp(Noise(ix,iz,seed),Noise(ix+1,iz,seed),fx),Mathf.Lerp(Noise(ix,iz+1,seed),Noise(ix+1,iz+1,seed),fx),fz);}
        static float FBM(float x,float z,int seed){float v=0,a=1,f=1,m=0;for(int i=0;i<4;i++){v+=SN(x*f,z*f,seed+i*997)*a;m+=a;a*=.5f;f*=2f;}return v/m;}
        void GenerateMap(string era,EraTheme t,System.Random rng,int seed)
        {
            Debug.Log($"[TR] GenerateMap start era={era} structureRoot={structureRoot}");
            Transform root=structureRoot!=null?structureRoot:transform;
            for(int i=root.childCount-1;i>=0;i--)if(root.GetChild(i).name.StartsWith("ERA_"))DestroyImmediate(root.GetChild(i).gameObject);
            var placed=new List<Rect>();
            // Reserve spawn points so trees/rocks never block them
            foreach (var sp in new[]{new Vector2(80,80),new Vector2(-80,80),new Vector2(80,-80),new Vector2(-80,-80)})
                placed.Add(new Rect(sp.x-6f,sp.y-6f,12f,12f));
            // Reserve center village area so water never spawns under buildings
            placed.Add(new Rect(-120f,-120f,240f,240f));
            BuildGround(root,t,rng,seed);
            BuildClouds(root,t,rng,seed);
            BuildWater(root,t,era,rng,seed,placed);
            BuildVillage(root,t,era,rng,seed,placed);
            BuildNature(root,t,era,rng,seed,placed);
            BuildExitPortal(root,t,rng,seed,placed);
            Debug.Log($"[TR] GenerateMap done, root childCount={root.childCount} instanceID={gameObject.GetInstanceID()}");
            if(Camera.main!=null)Camera.main.farClipPlane=Mathf.Clamp(1f/(t.fogDensity*0.3f),200f,2000f);
        }

        void BuildGround(Transform root,EraTheme t,System.Random rng,int seed)
        {
            var unlitShader = Shader.Find("Unlit/Color");
            var baseGround=GameObject.CreatePrimitive(PrimitiveType.Plane); baseGround.name="ERA_Ground";
            baseGround.transform.SetParent(root,false); baseGround.transform.localPosition=new Vector3(0,-0.05f,0);
            baseGround.transform.localScale=new Vector3(PLAY*2f/10f,1f,PLAY*2f/10f);
            var worldFloor=new GameObject("ERA_WorldFloor"); worldFloor.transform.SetParent(root,false);
            worldFloor.transform.position=new Vector3(0,-6f,0);
            worldFloor.AddComponent<BoxCollider>().size=new Vector3(MAP*2f,0.5f,MAP*2f);
            var grassTex = Resources.Load<Texture2D>("GrassGround");
            Material groundMat;
            var unlitTex = Shader.Find("Unlit/Texture"); var unlitColor = Shader.Find("Unlit/Color");
            if (grassTex != null && unlitTex != null)
            { groundMat = new Material(unlitTex); groundMat.SetTexture("_MainTex", grassTex); groundMat.SetTextureScale("_MainTex", new Vector2(160f, 160f)); }
            else { groundMat = new Material(unlitColor); groundMat.color = t.ground; }
            baseGround.GetComponent<Renderer>().sharedMaterial=groundMat;
            var oceanMat=new Material(Shader.Find("Unlit/Color")); oceanMat.color=new Color(0.04f,0.12f,0.28f);
            float oceanW=(MAP-PLAY)*2f; float oceanOff=PLAY+oceanW*0.5f;
            string[] oceanNames={"ERA_OceanN","ERA_OceanS","ERA_OceanE","ERA_OceanW"};
            Vector3[] oceanPos={new Vector3(0,0f,oceanOff),new Vector3(0,0f,-oceanOff),new Vector3(oceanOff,0f,0),new Vector3(-oceanOff,0f,0)};
            Vector3[] oceanScl={new Vector3(MAP*2f/10f,1f,oceanW/10f),new Vector3(MAP*2f/10f,1f,oceanW/10f),new Vector3(oceanW/10f,1f,PLAY*2f/10f),new Vector3(oceanW/10f,1f,PLAY*2f/10f)};
            for(int i=0;i<4;i++){
                var op=GameObject.CreatePrimitive(PrimitiveType.Plane); op.name=oceanNames[i];
                op.transform.SetParent(root,false); op.transform.localPosition=oceanPos[i]; op.transform.localScale=oceanScl[i];
                op.GetComponent<Renderer>().sharedMaterial=oceanMat; DestroyImmediate(op.GetComponent<Collider>());}
            for(int cx=-1;cx<=1;cx+=2) for(int cz=-1;cz<=1;cz+=2){
                var cp=GameObject.CreatePrimitive(PrimitiveType.Plane); cp.name="ERA_OceanCorner";
                cp.transform.SetParent(root,false); cp.transform.localPosition=new Vector3(cx*oceanOff,0f,cz*oceanOff);
                cp.transform.localScale=new Vector3(oceanW/10f,1f,oceanW/10f);
                cp.GetComponent<Renderer>().sharedMaterial=oceanMat; DestroyImmediate(cp.GetComponent<Collider>());}
            var oceanTrig=new GameObject("ERA_OceanTrigger"); oceanTrig.transform.SetParent(root,false);
            oceanTrig.transform.position=new Vector3(0,-3f,0); oceanTrig.transform.localScale=new Vector3(MAP*2f,4f,MAP*2f); oceanTrig.layer=2;
            var oceanBc=oceanTrig.AddComponent<BoxCollider>(); oceanBc.isTrigger=true;
            var oceanSt=oceanTrig.AddComponent<Kit_SwimmingTrigger>(); oceanSt.col=oceanBc; oceanSt.exitAdjustment=Vector3.zero; oceanSt.enableBouyancyForEnteringRigidbodies=false;
            var oceanDrown=new GameObject("ERA_OceanDrown"); oceanDrown.transform.SetParent(root,false);
            oceanDrown.transform.position=new Vector3(0,-3f,0); oceanDrown.transform.localScale=new Vector3(MAP*2f,4f,MAP*2f); oceanDrown.layer=2;
            var oceanDbc=oceanDrown.AddComponent<BoxCollider>(); oceanDbc.isTrigger=true;
            var oceanDst=oceanDrown.AddComponent<Kit_SwimmingTrigger>(); oceanDst.col=oceanDbc; oceanDst.exitAdjustment=Vector3.zero; oceanDst.enableBouyancyForEnteringRigidbodies=false;
            for(int i=0;i<4;i++){float a=i*90f*Mathf.Deg2Rad,ox=Mathf.Cos(a)*PLAY*1.5f,oz=Mathf.Sin(a)*PLAY*1.5f;
                var ext=GameObject.CreatePrimitive(PrimitiveType.Plane); ext.name="ERA_GroundExt";
                ext.transform.SetParent(root,false); ext.transform.localPosition=new Vector3(ox,0f,oz);
                ext.transform.localScale=new Vector3(PLAY*2f/10f,1f,PLAY*2f/10f);
                ext.GetComponent<Renderer>().sharedMaterial=groundMat;}
            for(int i=0;i<8;i++){
                float a=R(rng,0f,Mathf.PI*2f),d=R(rng,PLAY*2f,PLAY*4f);
                float hx=Mathf.Cos(a)*d,hz=Mathf.Sin(a)*d,hr=R(rng,10f,30f);
                var h2=GameObject.CreatePrimitive(PrimitiveType.Sphere); h2.name="ERA_Hill";
                h2.transform.SetParent(root,false); h2.transform.localPosition=new Vector3(hx,-hr*.62f,hz);
                h2.transform.localScale=new Vector3(hr*2.2f,hr,hr*2.2f);
                h2.GetComponent<Renderer>().sharedMaterial=new Material(unlitShader){color=Tint(t.ground,rng,.05f)};
                DestroyImmediate(h2.GetComponent<Collider>());}
        }

        // ── Procedural cloud layer ───────────────────────────────────────
        void BuildClouds(Transform root,EraTheme t,System.Random rng,int seed)
        {
            int count=RI(rng,18,32);
            float cloudY=R(rng,120f,200f);
            for(int i=0;i<count;i++){
                float cx=R(rng,-MAP*.8f,MAP*.8f),cz=R(rng,-MAP*.8f,MAP*.8f);
                float cw=R(rng,40f,160f),ch=R(rng,8f,22f),cd=R(rng,30f,100f);
                var cloud=new GameObject("ERA_Cloud_"+i);
                cloud.transform.SetParent(root,false);
                cloud.transform.position=new Vector3(cx,cloudY+R(rng,-20f,20f),cz);
                // Main cloud body: 3-5 overlapping spheres
                int blobs=RI(rng,3,6);
                for(int b=0;b<blobs;b++){
                    float bx=R(rng,-cw*.4f,cw*.4f),bz=R(rng,-cd*.3f,cd*.3f);
                    float br=R(rng,ch*.6f,ch*1.4f);
                    Color cc=new Color(Mathf.Clamp01(t.cloud.r+R(rng,-.06f,.06f)),
                                      Mathf.Clamp01(t.cloud.g+R(rng,-.06f,.06f)),
                                      Mathf.Clamp01(t.cloud.b+R(rng,-.06f,.06f)));
                    Prim(PrimitiveType.Sphere,cloud.transform,"Blob",
                        new Vector3(bx,R(rng,-br*.2f,br*.2f),bz),
                        new Vector3(br*2.2f,br,br*1.8f),cc);
                }
            }
        }

        void BuildVillage(Transform root,EraTheme t,string era,System.Random rng,int seed,List<Rect> placed)
        {
            var assets=assetConfig!=null?assetConfig.Get(era):null;
            if(assets?.structurePrefabs?.Length>0)
            {
                int count=RI(rng,1,Mathf.Max(2,Mathf.Min(20,assets.structurePrefabs.Length)+1));
                for(int i=0;i<count;i++){for(int attempt=0;attempt<30;attempt++){
                    float a=R(rng,0f,Mathf.PI*2f),d=R(rng,8f,PLAY*.85f);
                    float x=Mathf.Cos(a)*d,z=Mathf.Sin(a)*d;
                    var rect=new Rect(x-8f,z-8f,16f,16f); if(Overlaps(placed,rect,3f))continue; placed.Add(rect);
                    var pfb=assets.structurePrefabs[RI(rng,0,assets.structurePrefabs.Length)];
                    if(pfb!=null){var go=Instantiate(pfb,new Vector3(x,0f,z),Quaternion.Euler(0,R(rng,0f,360f),0),root);go.name="ERA_Structure_"+i;
                        float s=R(rng,.8f,1.3f); Vector3 bs=pfb.transform.localScale; go.transform.localScale=new Vector3(bs.x*s,bs.y*s,bs.z*s);
                        MakeUnlit(go); AddColliders(go,false);} break;}}
            }
            else
            {
                BuildLandmark(root,t,era,rng,placed);
                int count=RI(rng,8,16);
                for(int i=0;i<count;i++){for(int attempt=0;attempt<30;attempt++){
                    float a=R(rng,0f,Mathf.PI*2f),d=R(rng,10f,PLAY*.8f);
                    float x=Mathf.Cos(a)*d,z=Mathf.Sin(a)*d;
                    float w=R(rng,6f,14f),dep=R(rng,5f,10f);
                    var rect=new Rect(x-w*.5f,z-dep*.5f,w,dep); if(Overlaps(placed,rect,2f))continue; placed.Add(rect);
                    float h=t.style=="modern"?R(rng,6f,18f):t.style=="future"?R(rng,8f,22f):t.style=="medieval"?R(rng,4f,10f):R(rng,3f,8f);
                    BuildHouse(root,t,rng,new Vector3(x,0f,z),w,dep,h,i); break;}}
            }
            for(int i=0;i<6;i++){float a=i*60f*Mathf.Deg2Rad;
                Prim(PrimitiveType.Cube,root,"ERA_Path",new Vector3(Mathf.Cos(a)*VILLAGE*.5f,.01f,Mathf.Sin(a)*VILLAGE*.5f),new Vector3(2f,.02f,VILLAGE*.5f),Tint(t.groundAlt,rng,.02f),a*Mathf.Rad2Deg);}
        }

        void BuildHouse(Transform root,EraTheme t,System.Random rng,Vector3 pos,float w,float d,float h,int idx)
        {
            var p=new GameObject("ERA_House_"+idx); p.transform.SetParent(root,false);
            p.transform.position=pos; p.transform.localRotation=Quaternion.Euler(0,R(rng,0f,360f),0);
            // Floor slab
            Prim(PrimitiveType.Cube,p.transform,"Floor",new Vector3(0,.05f,0),new Vector3(w,.1f,d),Tint(t.wallAlt,rng,.03f));
            // Roof with slight overhang
            Prim(PrimitiveType.Cube,p.transform,"Roof",new Vector3(0,h+.15f,0),new Vector3(w+.4f,.25f,d+.4f),Tint(t.wall,rng,.03f));
            // Roof ridge (pitched look)
            Prim(PrimitiveType.Cube,p.transform,"Ridge",new Vector3(0,h+.4f,0),new Vector3(w*.3f,.3f,d+.5f),Tint(t.wallAlt,rng,.02f));
            // 4 walls with door/window openings
            Wall4(p.transform,t,rng,w,d,h);
            // Window frames on side walls
            AddWindowFrames(p.transform,t,rng,w,d,h);
            // Front step / stoop
            Prim(PrimitiveType.Cube,p.transform,"Step",new Vector3(0,-.05f,d*.5f+.2f),new Vector3(1.6f,.1f,.4f),Tint(t.wallAlt,rng,.02f));
            // Chimney (medieval/renaissance/industrial)
            if(t.style=="medieval"||t.style=="renaissance"||t.style=="industrial"){
                float cx=R(rng,-w*.25f,w*.25f);
                Prim(PrimitiveType.Cube,p.transform,"Chimney",new Vector3(cx,h+.8f,d*.2f),new Vector3(.5f,1.6f,.5f),Tint(t.wallAlt,rng,.03f));
                Prim(PrimitiveType.Cube,p.transform,"ChimneyTop",new Vector3(cx,h+1.7f,d*.2f),new Vector3(.7f,.2f,.7f),Tint(t.rock,rng,.03f));
            }
            // Multi-storey: add second floor balcony if tall enough
            if(h>7f){
                float bh=h*.5f;
                Prim(PrimitiveType.Cube,p.transform,"Balcony",new Vector3(0,bh,d*.5f+.3f),new Vector3(w*.6f,.15f,.8f),Tint(t.wallAlt,rng,.02f));
                // Balcony railing posts
                for(int j=-1;j<=1;j+=2)
                    Prim(PrimitiveType.Cube,p.transform,"Post",new Vector3(j*w*.28f,bh+.4f,d*.5f+.3f),new Vector3(.1f,.8f,.1f),Tint(t.wall,rng,.02f));
                Prim(PrimitiveType.Cube,p.transform,"Rail",new Vector3(0,bh+.8f,d*.5f+.3f),new Vector3(w*.6f,.08f,.08f),Tint(t.wall,rng,.02f));
                // Exterior staircase up to balcony
                BuildStairs(p.transform,t,rng,new Vector3(w*.3f,0f,d*.5f+.5f),bh,1.2f,true);
            }
        }

        void Wall4(Transform p,EraTheme t,System.Random rng,float w,float d,float h)
        {
            float dw=1.2f,dh=Mathf.Min(h*.78f,2.4f);
            // Front: door opening
            WallSeg(p,t,rng,new Vector3(0,h*.5f,d*.5f),w,h,dw,dh,true);
            // Door frame
            AddDoorFrame(p,t,rng,new Vector3(0,0f,d*.5f),dw,dh);
            // Back: window opening
            WallSeg(p,t,rng,new Vector3(0,h*.5f,-d*.5f),w,h,.9f,h*.45f,true);
            // Side walls solid
            Prim(PrimitiveType.Cube,p,"WallL",new Vector3(-w*.5f,h*.5f,0),new Vector3(WT,h,d),Tint(t.wall,rng));
            Prim(PrimitiveType.Cube,p,"WallR",new Vector3(w*.5f,h*.5f,0),new Vector3(WT,h,d),Tint(t.wall,rng));
        }

        void WallSeg(Transform p,EraTheme t,System.Random rng,Vector3 centre,float len,float h,float gapW,float gapH,bool alongX)
        {
            float seg=(len-gapW)*.5f; var dir=alongX?Vector3.right:Vector3.forward;
            var wsz=alongX?new Vector3(seg,h,WT):new Vector3(WT,h,seg);
            Prim(PrimitiveType.Cube,p,"Wall",centre+dir*(-(gapW*.5f+seg*.5f)),wsz,Tint(t.wall,rng));
            Prim(PrimitiveType.Cube,p,"Wall",centre+dir*(gapW*.5f+seg*.5f),wsz,Tint(t.wall,rng));
            float lh=h-gapH; if(lh>.05f){var lsz=alongX?new Vector3(gapW,lh,WT):new Vector3(WT,lh,gapW);
                Prim(PrimitiveType.Cube,p,"Lintel",centre+Vector3.up*(gapH+lh*.5f),lsz,Tint(t.wall,rng));}
        }

        void AddDoorFrame(Transform p,EraTheme t,System.Random rng,Vector3 pos,float dw,float dh)
        {
            Color fc=Tint(t.wallAlt,rng,.04f); float ft=.12f;
            // Left jamb
            Prim(PrimitiveType.Cube,p,"DoorJambL",pos+new Vector3(-dw*.5f-ft*.5f,dh*.5f,0),new Vector3(ft,dh,ft*1.5f),fc);
            // Right jamb
            Prim(PrimitiveType.Cube,p,"DoorJambR",pos+new Vector3(dw*.5f+ft*.5f,dh*.5f,0),new Vector3(ft,dh,ft*1.5f),fc);
            // Top header
            Prim(PrimitiveType.Cube,p,"DoorHeader",pos+new Vector3(0,dh+ft*.5f,0),new Vector3(dw+ft*2f,ft,ft*1.5f),fc);
            // Door panel (slightly recessed, dark wood color)
            Color dc=new Color(Mathf.Clamp01(t.accent.r*.6f),Mathf.Clamp01(t.accent.g*.4f),Mathf.Clamp01(t.accent.b*.2f));
            Prim(PrimitiveType.Cube,p,"Door",pos+new Vector3(dw*.25f,dh*.5f,-.02f),new Vector3(dw*.48f,dh*.97f,.06f),dc);
            Prim(PrimitiveType.Cube,p,"Door2",pos+new Vector3(-dw*.25f,dh*.5f,-.02f),new Vector3(dw*.48f,dh*.97f,.06f),dc);
            // Door knobs
            Prim(PrimitiveType.Sphere,p,"Knob",pos+new Vector3(dw*.02f,dh*.45f,.04f),new Vector3(.06f,.06f,.06f),Tint(t.accent,rng,.02f));
        }

        void AddWindowFrames(Transform p,EraTheme t,System.Random rng,float w,float d,float h)
        {
            Color fc=Tint(t.wallAlt,rng,.04f);
            Color glass=new Color(.3f,.45f,.6f,.7f);
            float ft=.08f,ww=.7f,wh=.8f,wy=h*.58f;
            int wc=Mathf.Max(1,Mathf.FloorToInt(d/3.5f));
            for(int i=0;i<wc;i++){
                float wz=Mathf.Lerp(-d*.32f,d*.32f,wc==1?.5f:(float)i/(wc-1));
                // Left wall window
                AddSingleWindow(p,fc,glass,new Vector3(-w*.5f,wy,wz),ww,wh,ft,true);
                // Right wall window
                AddSingleWindow(p,fc,glass,new Vector3(w*.5f,wy,wz),ww,wh,ft,true);
            }
            // Back wall window
            AddSingleWindow(p,fc,glass,new Vector3(0,h*.55f,-d*.5f),ww*.9f,wh*.9f,ft,false);
        }

        void AddSingleWindow(Transform p,Color fc,Color glass,Vector3 pos,float ww,float wh,float ft,bool onXWall)
        {
            float depth=onXWall?.04f:.04f;
            // Frame: 4 thin bars
            Prim(PrimitiveType.Cube,p,"WFT",pos+Vector3.up*(wh*.5f+ft*.5f),onXWall?new Vector3(depth,ft,ww+ft*2):new Vector3(ww+ft*2,ft,depth),fc);
            Prim(PrimitiveType.Cube,p,"WFB",pos-Vector3.up*(wh*.5f+ft*.5f),onXWall?new Vector3(depth,ft,ww+ft*2):new Vector3(ww+ft*2,ft,depth),fc);
            Prim(PrimitiveType.Cube,p,"WFL",pos+(onXWall?Vector3.forward:Vector3.right)*(ww*.5f+ft*.5f),onXWall?new Vector3(depth,wh+ft*2,ft):new Vector3(ft,wh+ft*2,depth),fc);
            Prim(PrimitiveType.Cube,p,"WFR",pos-(onXWall?Vector3.forward:Vector3.right)*(ww*.5f+ft*.5f),onXWall?new Vector3(depth,wh+ft*2,ft):new Vector3(ft,wh+ft*2,depth),fc);
            // Cross divider
            Prim(PrimitiveType.Cube,p,"WFC",pos,onXWall?new Vector3(depth,ft*.6f,ww):new Vector3(ww,ft*.6f,depth),fc);
            // Glass pane (dark blue-grey)
            Prim(PrimitiveType.Cube,p,"Glass",pos,onXWall?new Vector3(depth*.5f,wh,ww):new Vector3(ww,wh,depth*.5f),glass);
        }

        // Exterior staircase: steps going up to height h, width sw
        void BuildStairs(Transform parent,EraTheme t,System.Random rng,Vector3 basePos,float h,float sw,bool facingZ)
        {
            int steps=Mathf.Max(2,Mathf.RoundToInt(h/.3f));
            float stepH=h/steps, stepD=0.3f;
            Color sc=Tint(t.wallAlt,rng,.03f);
            for(int i=0;i<steps;i++){
                float sy=(i+.5f)*stepH, sz=facingZ?(i+.5f)*stepD:0f, sx=facingZ?0f:(i+.5f)*stepD;
                Prim(PrimitiveType.Cube,parent,"Step",basePos+new Vector3(sx,sy,sz),
                    facingZ?new Vector3(sw,stepH,stepD):new Vector3(stepD,stepH,sw),sc);
            }
        }

        void BuildLandmark(Transform root,EraTheme t,string era,System.Random rng,List<Rect> placed)
        {
            switch(t.style){
                case "prehistoric":BuildStonehenge(root,t,rng,placed);break;
                case "ancient":BuildTemple(root,t,rng,placed);break;
                case "medieval":BuildCastle(root,t,rng,placed);break;
                case "renaissance":BuildCathedral(root,t,rng,placed);break;
                case "industrial":BuildFactory(root,t,rng,placed);break;
                case "modern":BuildCityBlock(root,t,rng,placed);break;
                default:BuildMegastructure(root,t,rng,placed);break;}
        }
        void BuildStonehenge(Transform root,EraTheme t,System.Random rng,List<Rect> placed)
        {
            placed.Add(new Rect(-14f,-14f,28f,28f));
            var p=new GameObject("ERA_Stonehenge"); p.transform.SetParent(root,false);
            // Outer earth bank
            Prim(PrimitiveType.Cylinder,p.transform,"Bank",new Vector3(0,-.1f,0),new Vector3(26f,.4f,26f),Tint(t.groundAlt,rng,.03f));
            for(int i=0;i<12;i++){float a=i*Mathf.PI*2f/12,rx=Mathf.Cos(a)*9f,rz=Mathf.Sin(a)*9f,h=R(rng,2.8f,4.8f);
                Prim(PrimitiveType.Cube,p.transform,"Stone",new Vector3(rx,h*.5f,rz),new Vector3(.9f,h,.75f),Tint(t.rock,rng,.04f),a*Mathf.Rad2Deg);
                if(i%2==0){float a2=(i+1)*Mathf.PI*2f/12;
                    Prim(PrimitiveType.Cube,p.transform,"Lintel",new Vector3((rx+Mathf.Cos(a2)*9f)*.5f,h+.35f,(rz+Mathf.Sin(a2)*9f)*.5f),new Vector3(2.2f,.55f,.75f),Tint(t.rock,rng,.03f),a*Mathf.Rad2Deg);}
            }
            // Inner horseshoe trilithons
            for(int i=0;i<5;i++){float a=i*Mathf.PI/4f+Mathf.PI*.1f,rx=Mathf.Cos(a)*4.5f,rz=Mathf.Sin(a)*4.5f,h=R(rng,4f,6f);
                Prim(PrimitiveType.Cube,p.transform,"IStone",new Vector3(rx-Mathf.Sin(a)*.6f,h*.5f,rz+Mathf.Cos(a)*.6f),new Vector3(.7f,h,.6f),Tint(t.rock,rng,.04f),a*Mathf.Rad2Deg);
                Prim(PrimitiveType.Cube,p.transform,"IStone",new Vector3(rx+Mathf.Sin(a)*.6f,h*.5f,rz-Mathf.Cos(a)*.6f),new Vector3(.7f,h,.6f),Tint(t.rock,rng,.04f),a*Mathf.Rad2Deg);
                Prim(PrimitiveType.Cube,p.transform,"ILintel",new Vector3(rx,h+.3f,rz),new Vector3(1.6f,.5f,.6f),Tint(t.rock,rng,.03f),a*Mathf.Rad2Deg);}
            Prim(PrimitiveType.Cube,p.transform,"Altar",new Vector3(0,.25f,0),new Vector3(2.2f,.5f,1.2f),Tint(t.accent,rng));
        }
        void BuildTemple(Transform root,EraTheme t,System.Random rng,List<Rect> placed)
        {
            placed.Add(new Rect(-16f,-14f,32f,28f));
            var p=new GameObject("ERA_Temple"); p.transform.SetParent(root,false);
            // Raised platform with steps on all 4 sides
            Prim(PrimitiveType.Cube,p.transform,"Platform",new Vector3(0,.5f,0),new Vector3(18f,1f,14f),Tint(t.wall,rng));
            for(int s=0;s<3;s++){float sh=(s+1)*.25f,sw=18f-s*1.5f,sd=14f-s*1.5f;
                Prim(PrimitiveType.Cube,p.transform,"Step",new Vector3(0,sh*.5f,sd*.5f+.4f),new Vector3(sw,sh,.4f),Tint(t.wall,rng,.02f));
                Prim(PrimitiveType.Cube,p.transform,"Step",new Vector3(0,sh*.5f,-sd*.5f-.4f),new Vector3(sw,sh,.4f),Tint(t.wall,rng,.02f));}
            // Columns
            for(int i=-2;i<=2;i++){
                Prim(PrimitiveType.Cylinder,p.transform,"ColF",new Vector3(i*3f,4f,6f),new Vector3(.55f,7f,.55f),Tint(t.wallAlt,rng));
                Prim(PrimitiveType.Cylinder,p.transform,"ColB",new Vector3(i*3f,4f,-6f),new Vector3(.55f,7f,.55f),Tint(t.wallAlt,rng));
                // Column base and capital
                Prim(PrimitiveType.Cube,p.transform,"Base",new Vector3(i*3f,1.1f,6f),new Vector3(.8f,.2f,.8f),Tint(t.wall,rng,.02f));
                Prim(PrimitiveType.Cube,p.transform,"Cap",new Vector3(i*3f,7.6f,6f),new Vector3(.9f,.3f,.9f),Tint(t.wall,rng,.02f));
                Prim(PrimitiveType.Cube,p.transform,"Base",new Vector3(i*3f,1.1f,-6f),new Vector3(.8f,.2f,.8f),Tint(t.wall,rng,.02f));
                Prim(PrimitiveType.Cube,p.transform,"Cap",new Vector3(i*3f,7.6f,-6f),new Vector3(.9f,.3f,.9f),Tint(t.wall,rng,.02f));}
            Prim(PrimitiveType.Cube,p.transform,"Roof",new Vector3(0,8f,0),new Vector3(18f,.5f,14f),Tint(t.wall,rng));
            Prim(PrimitiveType.Cube,p.transform,"Pediment",new Vector3(0,9.5f,0),new Vector3(18f,3f,1f),Tint(t.wallAlt,rng));
            Prim(PrimitiveType.Cube,p.transform,"Ob1",new Vector3(-5f,6f,9f),new Vector3(.7f,12f,.7f),Tint(t.accent,rng));
            Prim(PrimitiveType.Cube,p.transform,"Ob2",new Vector3(5f,6f,9f),new Vector3(.7f,12f,.7f),Tint(t.accent,rng));
        }
        void BuildCastle(Transform root,EraTheme t,System.Random rng,List<Rect> placed)
        {
            placed.Add(new Rect(-20f,-20f,40f,40f));
            var p=new GameObject("ERA_Castle"); p.transform.SetParent(root,false); float wh=6f;
            // Moat (water ring)
            Prim(PrimitiveType.Cylinder,p.transform,"Moat",new Vector3(0,-.15f,0),new Vector3(32f,.3f,32f),t.water);
            Prim(PrimitiveType.Cylinder,p.transform,"MoatFill",new Vector3(0,-.05f,0),new Vector3(26f,.2f,26f),Tint(t.ground,rng,.02f));
            // Curtain walls
            Prim(PrimitiveType.Cube,p.transform,"WN",new Vector3(0,wh*.5f,13f),new Vector3(26f,wh,WT),Tint(t.wall,rng));
            Prim(PrimitiveType.Cube,p.transform,"WS",new Vector3(0,wh*.5f,-13f),new Vector3(26f,wh,WT),Tint(t.wall,rng));
            Prim(PrimitiveType.Cube,p.transform,"WE",new Vector3(13f,wh*.5f,0),new Vector3(WT,wh,26f),Tint(t.wall,rng));
            Prim(PrimitiveType.Cube,p.transform,"WW",new Vector3(-13f,wh*.5f,0),new Vector3(WT,wh,26f),Tint(t.wall,rng));
            // Battlements on walls
            for(int i=-5;i<=5;i+=2){
                Prim(PrimitiveType.Cube,p.transform,"Mrl",new Vector3(i*2.2f,wh+.5f,13f),new Vector3(.8f,1f,.8f),Tint(t.wall,rng));
                Prim(PrimitiveType.Cube,p.transform,"Mrl",new Vector3(i*2.2f,wh+.5f,-13f),new Vector3(.8f,1f,.8f),Tint(t.wall,rng));
                Prim(PrimitiveType.Cube,p.transform,"Mrl",new Vector3(13f,wh+.5f,i*2.2f),new Vector3(.8f,1f,.8f),Tint(t.wall,rng));
                Prim(PrimitiveType.Cube,p.transform,"Mrl",new Vector3(-13f,wh+.5f,i*2.2f),new Vector3(.8f,1f,.8f),Tint(t.wall,rng));}
            // Gate with portcullis
            Prim(PrimitiveType.Cube,p.transform,"GateL",new Vector3(-1.5f,wh*.5f,-13f),new Vector3(WT*3,wh,WT),Tint(t.wall,rng));
            Prim(PrimitiveType.Cube,p.transform,"GateR",new Vector3(1.5f,wh*.5f,-13f),new Vector3(WT*3,wh,WT),Tint(t.wall,rng));
            Prim(PrimitiveType.Cube,p.transform,"GateTop",new Vector3(0,wh-.3f,-13f),new Vector3(3f,.6f,WT),Tint(t.wall,rng));
            // Drawbridge
            Prim(PrimitiveType.Cube,p.transform,"Bridge",new Vector3(0,.05f,-15f),new Vector3(2.8f,.15f,4f),Tint(t.wallAlt,rng,.03f));
            // Corner towers with conical roofs
            float[] cx={-13f,13f,-13f,13f}; float[] cz2={13f,13f,-13f,-13f};
            for(int i=0;i<4;i++){
                Prim(PrimitiveType.Cylinder,p.transform,"Tower",new Vector3(cx[i],wh+2f,cz2[i]),new Vector3(3f,wh+4f,3f),Tint(t.wallAlt,rng));
                // Conical roof
                Prim(PrimitiveType.Cylinder,p.transform,"Cone",new Vector3(cx[i],wh+5.5f,cz2[i]),new Vector3(3.2f,3f,3.2f),Tint(t.accent,rng,.04f));}
            // Keep with interior floors
            Prim(PrimitiveType.Cube,p.transform,"Keep",new Vector3(0,5.5f,2f),new Vector3(7f,11f,7f),Tint(t.wallAlt,rng));
            Prim(PrimitiveType.Cube,p.transform,"KeepFloor2",new Vector3(0,5.5f,2f),new Vector3(6.8f,.2f,6.8f),Tint(t.wall,rng,.02f));
            Prim(PrimitiveType.Cube,p.transform,"KeepRoof",new Vector3(0,11.2f,2f),new Vector3(7.5f,.4f,7.5f),Tint(t.wall,rng));
            // Keep stairs
            BuildStairs(p.transform,t,rng,new Vector3(-3f,0f,-1f),5.5f,1.5f,false);
        }
        void BuildCathedral(Transform root,EraTheme t,System.Random rng,List<Rect> placed)
        {
            placed.Add(new Rect(-12f,-18f,24f,36f));
            var p=new GameObject("ERA_Cathedral"); p.transform.SetParent(root,false);
            // Nave with buttresses
            Prim(PrimitiveType.Cube,p.transform,"Nave",new Vector3(0,5f,-4f),new Vector3(9f,10f,20f),Tint(t.wall,rng));
            for(int i=-2;i<=2;i++){
                Prim(PrimitiveType.Cube,p.transform,"ButtL",new Vector3(-5f,3f,i*4f-4f),new Vector3(2f,6f,1.2f),Tint(t.wallAlt,rng,.02f));
                Prim(PrimitiveType.Cube,p.transform,"ButtR",new Vector3(5f,3f,i*4f-4f),new Vector3(2f,6f,1.2f),Tint(t.wallAlt,rng,.02f));}
            Prim(PrimitiveType.Cube,p.transform,"Transept",new Vector3(0,3.5f,4f),new Vector3(20f,7f,7f),Tint(t.wallAlt,rng));
            Prim(PrimitiveType.Sphere,p.transform,"Dome",new Vector3(0,10f,4f),new Vector3(7f,5f,7f),Tint(t.accent,rng));
            // Bell towers with windows
            Prim(PrimitiveType.Cube,p.transform,"TL",new Vector3(-4.5f,8f,-13f),new Vector3(3.5f,16f,3.5f),Tint(t.wallAlt,rng));
            Prim(PrimitiveType.Cube,p.transform,"TR",new Vector3(4.5f,8f,-13f),new Vector3(3.5f,16f,3.5f),Tint(t.wallAlt,rng));
            // Tower windows
            for(int fl=0;fl<3;fl++){float fy=6f+fl*4f;
                Prim(PrimitiveType.Cube,p.transform,"TWin",new Vector3(-4.5f,fy,-11.2f),new Vector3(.1f,1.2f,.8f),new Color(.2f,.3f,.5f));
                Prim(PrimitiveType.Cube,p.transform,"TWin",new Vector3(4.5f,fy,-11.2f),new Vector3(.1f,1.2f,.8f),new Color(.2f,.3f,.5f));}
            Prim(PrimitiveType.Cylinder,p.transform,"SL",new Vector3(-4.5f,17f,-13f),new Vector3(.7f,4.5f,.7f),t.accent);
            Prim(PrimitiveType.Cylinder,p.transform,"SR",new Vector3(4.5f,17f,-13f),new Vector3(.7f,4.5f,.7f),t.accent);
            // Front steps
            for(int s=0;s<4;s++) Prim(PrimitiveType.Cube,p.transform,"Step",new Vector3(0,(s+1)*.18f,-14.5f-(s*.35f)),new Vector3(8f,.18f,.4f),Tint(t.wall,rng,.02f));
        }
        void BuildFactory(Transform root,EraTheme t,System.Random rng,List<Rect> placed)
        {
            placed.Add(new Rect(-16f,-14f,32f,28f));
            var p=new GameObject("ERA_Factory"); p.transform.SetParent(root,false);
            Prim(PrimitiveType.Cube,p.transform,"Shed",new Vector3(0,4.5f,0),new Vector3(22f,9f,14f),Tint(t.wall,rng));
            Prim(PrimitiveType.Cube,p.transform,"Roof",new Vector3(0,9.2f,0),new Vector3(23f,.4f,15f),Tint(t.wallAlt,rng));
            // Sawtooth roof skylights
            for(int i=-2;i<=2;i++) Prim(PrimitiveType.Cube,p.transform,"Skylight",new Vector3(i*4f,9.5f,0),new Vector3(2f,.5f,14f),new Color(.3f,.4f,.5f));
            // Smokestacks with banding
            for(int i=-1;i<=1;i++){
                Prim(PrimitiveType.Cylinder,p.transform,"Stack",new Vector3(i*7f,12f,-5f),new Vector3(1.4f,24f,1.4f),Tint(t.wallAlt,rng));
                for(int b=0;b<4;b++) Prim(PrimitiveType.Cylinder,p.transform,"Band",new Vector3(i*7f,4f+b*4f,-5f),new Vector3(1.6f,.3f,1.6f),Tint(t.accent,rng,.04f));
                Prim(PrimitiveType.Cylinder,p.transform,"Cap",new Vector3(i*7f,24.5f,-5f),new Vector3(2f,.5f,2f),t.accent);}
            // Rail tracks with ties
            for(int r2=-1;r2<=1;r2+=2) Prim(PrimitiveType.Cube,p.transform,"Rail",new Vector3(r2*.85f,.05f,0),new Vector3(.18f,.1f,35f),t.accent*.8f);
            for(int ti=-6;ti<=6;ti++) Prim(PrimitiveType.Cube,p.transform,"Tie",new Vector3(0,.02f,ti*2.5f),new Vector3(2.2f,.08f,.4f),Tint(t.wallAlt,rng,.04f));
            // Loading dock with stairs
            Prim(PrimitiveType.Cube,p.transform,"Dock",new Vector3(13f,1.2f,0),new Vector3(6f,2.4f,10f),Tint(t.wallAlt,rng));
            BuildStairs(p.transform,t,rng,new Vector3(10f,0f,-3f),1.2f,2f,false);
        }
        void BuildCityBlock(Transform root,EraTheme t,System.Random rng,List<Rect> placed)
        {
            placed.Add(new Rect(-20f,-20f,40f,40f));
            var p=new GameObject("ERA_CityBlock"); p.transform.SetParent(root,false);
            // Roads with lane markings
            Prim(PrimitiveType.Cube,p.transform,"RdH",new Vector3(0,.02f,0),new Vector3(40f,.04f,5f),t.ground*.6f);
            Prim(PrimitiveType.Cube,p.transform,"RdV",new Vector3(0,.02f,0),new Vector3(5f,.04f,40f),t.ground*.6f);
            for(int i=-2;i<=2;i++) Prim(PrimitiveType.Cube,p.transform,"Lane",new Vector3(i*3f,.03f,0),new Vector3(.15f,.02f,4.5f),new Color(.9f,.8f,.1f));
            // Pavement
            Prim(PrimitiveType.Cube,p.transform,"PavN",new Vector3(0,.03f,7f),new Vector3(40f,.06f,4f),Tint(t.groundAlt,rng,.02f));
            Prim(PrimitiveType.Cube,p.transform,"PavS",new Vector3(0,.03f,-7f),new Vector3(40f,.06f,4f),Tint(t.groundAlt,rng,.02f));
            float[] bx={-12f,12f,-12f,12f,0f}; float[] bz={-12f,-12f,12f,12f,0f};
            float[] bh={14f,18f,12f,16f,22f}; float[] bw={6f,5f,7f,5f,4f};
            for(int i=0;i<5;i++){
                Prim(PrimitiveType.Cube,p.transform,"T",new Vector3(bx[i],bh[i]*.5f,bz[i]),new Vector3(bw[i],bh[i],bw[i]),Tint(t.wall,rng));
                // Glass curtain wall
                Prim(PrimitiveType.Cube,p.transform,"G",new Vector3(bx[i],bh[i]*.5f,bz[i]+bw[i]*.5f+.05f),new Vector3(bw[i]*.7f,bh[i]*.85f,.1f),new Color(.3f,.45f,.6f));
                // Floor lines
                int floors=Mathf.RoundToInt(bh[i]/3f);
                for(int fl=1;fl<floors;fl++) Prim(PrimitiveType.Cube,p.transform,"FL",new Vector3(bx[i],fl*3f,bz[i]),new Vector3(bw[i]+.1f,.12f,bw[i]+.1f),Tint(t.wallAlt,rng,.02f));
                // Entrance steps
                for(int s=0;s<2;s++) Prim(PrimitiveType.Cube,p.transform,"Step",new Vector3(bx[i],(s+1)*.15f,bz[i]+bw[i]*.5f+(s*.3f)),new Vector3(bw[i]*.6f,.15f,.3f),Tint(t.wallAlt,rng,.02f));}
        }
        void BuildMegastructure(Transform root,EraTheme t,System.Random rng,List<Rect> placed)
        {
            placed.Add(new Rect(-16f,-16f,32f,32f));
            var p=new GameObject("ERA_Mega"); p.transform.SetParent(root,false);
            // Platform base with steps
            Prim(PrimitiveType.Cylinder,p.transform,"Base",new Vector3(0,.5f,0),new Vector3(20f,1f,20f),Tint(t.wallAlt,rng));
            for(int s=0;s<3;s++) Prim(PrimitiveType.Cylinder,p.transform,"Step",new Vector3(0,(s+1)*.3f,0),new Vector3(20f-s*2f,.3f,20f-s*2f),Tint(t.wall,rng,.02f));
            Prim(PrimitiveType.Cylinder,p.transform,"Core",new Vector3(0,14f,0),new Vector3(4.5f,28f,4.5f),Tint(t.wall,rng));
            // Horizontal rings at intervals
            for(int r=0;r<4;r++) Prim(PrimitiveType.Cylinder,p.transform,"Ring",new Vector3(0,5f+r*5f,0),new Vector3(6f+r*.5f,.4f,6f+r*.5f),Tint(t.wallAlt,rng,.03f));
            Prim(PrimitiveType.Sphere,p.transform,"Top",new Vector3(0,29f,0),new Vector3(6f,6f,6f),t.accent);
            // Energy conduit lines
            for(int i=0;i<4;i++){float a=i*90f*Mathf.Deg2Rad,ax=Mathf.Cos(a)*9f,az=Mathf.Sin(a)*9f;
                Prim(PrimitiveType.Cube,p.transform,"Arm",new Vector3(ax,7f,az),new Vector3(1.5f,1.5f,12f),Tint(t.wallAlt,rng),i*90f);
                Prim(PrimitiveType.Cylinder,p.transform,"Sp",new Vector3(ax*1.6f,10f,az*1.6f),new Vector3(.9f,20f,.9f),Tint(t.accent,rng));
                Prim(PrimitiveType.Sphere,p.transform,"SpTop",new Vector3(ax*1.6f,21f,az*1.6f),new Vector3(1.5f,1.5f,1.5f),t.accent);}
        }

        void BuildNature(Transform root,EraTheme t,string era,System.Random rng,int seed,List<Rect> placed)
        {
            var assets=assetConfig!=null?assetConfig.Get(era):null;
            Debug.Log($"[TR] BuildNature: treePrefabs={(assets?.treePrefabs?.Length ?? 0)} rockPrefabs={(assets?.rockPrefabs?.Length ?? 0)}");
            if(t.hasTrees){for(int i=0;i<800;i++){
                float a=R(rng,0f,Mathf.PI*2f),d=R(rng,5f,PLAY*.95f),x=Mathf.Cos(a)*d,z=Mathf.Sin(a)*d;
                if(Overlaps(placed,new Rect(x-1.5f,z-1.5f,3f,3f),1.5f))continue;
                Vector3 pos=new Vector3(x,0f,z);
                if(assets?.treePrefabs?.Length>0){var pfb=assets.treePrefabs[RI(rng,0,assets.treePrefabs.Length)];if(pfb!=null){var pnf=pfb.name.ToLower();if(pnf.Contains("grass")||pnf.Contains("egrass")||pnf.Contains("bush")||pnf.Contains("flower")||pnf.Contains("fern")||pnf.Contains("heather")||pnf.Contains("plant")||pnf.Contains("mushroom")||pnf.Contains("smalltree")||pnf=="tree_01"){continue;}var go=Instantiate(pfb,pos,Quaternion.Euler(0,R(rng,0f,360f),0),root);go.name="ERA_Tree_"+i;go.transform.localScale=Vector3.one*R(rng,.8f,1.4f);
                    var unlitTex2=Shader.Find("Unlit/Texture");var unlitCol2=Shader.Find("Unlit/Color");if(unlitTex2!=null&&unlitCol2!=null){foreach(var rr in go.GetComponentsInChildren<Renderer>(true)){var mm=rr.sharedMaterials;bool ch=false;for(int mi=0;mi<mm.Length;mi++){if(mm[mi]==null)continue;var sn=mm[mi].shader.name;if(sn=="Unlit/Texture"||sn=="Unlit/Color"||sn.StartsWith("Shader Graphs/"))continue;Texture tex=null;foreach(var prop in new[]{"_BaseMap","_MainTex","_Albedo","_Diffuse"}){if(mm[mi].HasProperty(prop)){tex=mm[mi].GetTexture(prop);if(tex!=null)break;}}if(tex!=null){var m2=new Material(unlitTex2);m2.SetTexture("_MainTex",tex);mm[mi]=m2;ch=true;}else{Color c2=mm[mi].HasProperty("_BaseColor")?mm[mi].GetColor("_BaseColor"):mm[mi].HasProperty("_Color")?mm[mi].GetColor("_Color"):Color.grey;c2.a=1f;var m2=new Material(unlitCol2);m2.color=c2;mm[mi]=m2;ch=true;}}if(ch)rr.sharedMaterials=mm;}}
                    // Boost any near-black Unlit/Color materials to avoid invisible objects
                    foreach(var rr in go.GetComponentsInChildren<Renderer>(true)){var mm=rr.sharedMaterials;bool ch2=false;for(int mi=0;mi<mm.Length;mi++){if(mm[mi]==null)continue;if(mm[mi].shader.name=="Unlit/Color"){Color c=mm[mi].color;float lum=c.r*0.299f+c.g*0.587f+c.b*0.114f;if(lum<0.05f){var m2=new Material(mm[mi]);if(lum<0.001f){m2.color=new Color(0.4f,0.4f,0.4f,1f);}else{float boost=0.15f/lum;m2.color=new Color(Mathf.Min(1f,c.r*boost),Mathf.Min(1f,c.g*boost),Mathf.Min(1f,c.b*boost),1f);}mm[mi]=m2;ch2=true;}}}if(ch2)rr.sharedMaterials=mm;}
                    // Destroy prefabs that have no visible renderers — avoids black quads and white-circle collider ghosts
                    bool hasVisibleRenderer=false;
                    foreach(var rr in go.GetComponentsInChildren<Renderer>(true))
                        if(rr is not ParticleSystemRenderer&&rr.enabled&&rr.sharedMaterial!=null&&!rr.sharedMaterial.shader.name.StartsWith("Hidden/")){{hasVisibleRenderer=true;break;}}
                    if(!hasVisibleRenderer){DestroyImmediate(go);continue;}
                    // Kill any particle systems — black quad emitters on static nature prefabs
                    foreach(var psr in go.GetComponentsInChildren<ParticleSystemRenderer>(true)) psr.enabled=false;
                    foreach(var mc in go.GetComponentsInChildren<MeshCollider>()){
                        mc.convex=true;
                        if(!mc.convex) mc.enabled=false;
                    }
                    bool hasWorkingCollider=false;
                    foreach(var c in go.GetComponentsInChildren<Collider>())
                        if(c.enabled&&!(c is MeshCollider mc2&&!mc2.convex)){hasWorkingCollider=true;break;}
                    if(!hasWorkingCollider){
                        var b2=new Bounds(go.transform.position,Vector3.zero);
                        foreach(var rr in go.GetComponentsInChildren<Renderer>())b2.Encapsulate(rr.bounds);
                        var cap=go.AddComponent<CapsuleCollider>();
                        float treeH=b2.size.y;
                        cap.center=go.transform.InverseTransformPoint(new Vector3(b2.center.x,go.transform.position.y+treeH*0.5f,b2.center.z));
                        cap.height=treeH/Mathf.Max(0.001f,Mathf.Abs(go.transform.lossyScale.y));
                        cap.radius=Mathf.Clamp(treeH*0.06f,0.3f,1.5f);
                    }
                    continue;}}
                BuildDetailTree(root,t,rng,pos,i);
            }}
            if(t.hasRocks){for(int i=0;i<200;i++){
                float a=R(rng,0f,Mathf.PI*2f),d=R(rng,80f,PLAY*.9f),x=Mathf.Cos(a)*d,z=Mathf.Sin(a)*d;
                if(Overlaps(placed,new Rect(x-1f,z-1f,2f,2f),.5f))continue;
                Vector3 pos=new Vector3(x,0f,z);
                if(assets?.rockPrefabs?.Length>0){var pfb=assets.rockPrefabs[RI(rng,0,assets.rockPrefabs.Length)];if(pfb!=null){var go=Instantiate(pfb,pos,Quaternion.Euler(0,R(rng,0f,360f),0),root);go.name="ERA_Rock_"+i;float s=R(rng,.5f,2f);go.transform.localScale=new Vector3(s,s*R(rng,.6f,1f),s*R(rng,.9f,1.2f));
                    var unlitTex2=Shader.Find("Unlit/Texture");var unlitCol2=Shader.Find("Unlit/Color");if(unlitTex2!=null&&unlitCol2!=null){foreach(var rr in go.GetComponentsInChildren<Renderer>(true)){var mm=rr.sharedMaterials;bool ch=false;for(int mi=0;mi<mm.Length;mi++){if(mm[mi]==null)continue;var sn=mm[mi].shader.name;if(sn=="Unlit/Texture"||sn=="Unlit/Color"||sn.StartsWith("Shader Graphs/"))continue;Texture tex=null;foreach(var prop in new[]{"_BaseMap","_MainTex","_Albedo","_Diffuse"}){if(mm[mi].HasProperty(prop)){tex=mm[mi].GetTexture(prop);if(tex!=null)break;}}if(tex!=null){var m2=new Material(unlitTex2);m2.SetTexture("_MainTex",tex);mm[mi]=m2;ch=true;}else{Color c2=mm[mi].HasProperty("_BaseColor")?mm[mi].GetColor("_BaseColor"):mm[mi].HasProperty("_Color")?mm[mi].GetColor("_Color"):Color.grey;c2.a=1f;var m2=new Material(unlitCol2);m2.color=c2;mm[mi]=m2;ch=true;}}if(ch)rr.sharedMaterials=mm;}}
                    // Boost any near-black Unlit/Color materials
                    foreach(var rr in go.GetComponentsInChildren<Renderer>(true)){var mm=rr.sharedMaterials;bool ch2=false;for(int mi=0;mi<mm.Length;mi++){if(mm[mi]==null)continue;if(mm[mi].shader.name=="Unlit/Color"){Color c=mm[mi].color;float lum=c.r*0.299f+c.g*0.587f+c.b*0.114f;if(lum<0.05f){var m2=new Material(mm[mi]);if(lum<0.001f){m2.color=new Color(0.4f,0.4f,0.4f,1f);}else{float boost=0.15f/lum;m2.color=new Color(Mathf.Min(1f,c.r*boost),Mathf.Min(1f,c.g*boost),Mathf.Min(1f,c.b*boost),1f);}mm[mi]=m2;ch2=true;}}}if(ch2)rr.sharedMaterials=mm;}
                    // Destroy prefabs with no visible renderers
                    bool hasVisibleRenderer=false;
                    foreach(var rr in go.GetComponentsInChildren<Renderer>(true))
                        if(rr is not ParticleSystemRenderer&&rr.enabled&&rr.sharedMaterial!=null&&!rr.sharedMaterial.shader.name.StartsWith("Hidden/")){{hasVisibleRenderer=true;break;}}
                    if(!hasVisibleRenderer){DestroyImmediate(go);continue;}
                    // Kill any particle systems — black quad emitters on static nature prefabs
                    foreach(var psr in go.GetComponentsInChildren<ParticleSystemRenderer>(true)) psr.enabled=false;
                    foreach(var mc in go.GetComponentsInChildren<MeshCollider>())mc.convex=true;
                    if(go.GetComponentInChildren<Collider>()==null){var sc=go.AddComponent<SphereCollider>();var b2=new Bounds(go.transform.position,Vector3.zero);foreach(var rr in go.GetComponentsInChildren<Renderer>())b2.Encapsulate(rr.bounds);sc.center=go.transform.InverseTransformPoint(b2.center);Vector3 ls2=go.transform.lossyScale;sc.radius=Mathf.Min(b2.extents.x/Mathf.Max(0.001f,Mathf.Abs(ls2.x)),b2.extents.z/Mathf.Max(0.001f,Mathf.Abs(ls2.z)))*0.7f;}
                    continue;}}
                BuildDetailRock(root,t,rng,pos,i);
            }}
            for(int i=0;i<20;i++){
                float a=R(rng,0f,Mathf.PI*2f),d=R(rng,PLAY*0.3f,PLAY*0.9f),x=Mathf.Cos(a)*d,z=Mathf.Sin(a)*d;
                var rect=new Rect(x-80f,z-80f,160f,160f); if(Overlaps(placed,rect,20f))continue; placed.Add(rect);
                if(assets?.mountainPrefab!=null){
                    var go=Instantiate(assets.mountainPrefab,new Vector3(x,0f,z),Quaternion.Euler(0,R(rng,0f,360f),0),root);
                    go.name="ERA_Mountain_"+i;
                    float s=R(rng,8f,18f); go.transform.localScale=new Vector3(s,s*R(rng,.6f,1f),s);
                    // Kill particle systems
                    foreach(var psr in go.GetComponentsInChildren<ParticleSystemRenderer>(true)) psr.enabled=false;
                    // Make MeshColliders convex, add fallback BoxCollider if none
                    foreach(var mc in go.GetComponentsInChildren<MeshCollider>()) mc.convex=true;
                    if(go.GetComponentInChildren<Collider>()==null){
                        var bc=go.AddComponent<BoxCollider>();
                        var b2=new Bounds(go.transform.position,Vector3.zero);
                        foreach(var rr in go.GetComponentsInChildren<Renderer>()) b2.Encapsulate(rr.bounds);
                        bc.center=go.transform.InverseTransformPoint(b2.center);
                        Vector3 rawSize=go.transform.InverseTransformVector(b2.size);
                        bc.size=new Vector3(Mathf.Abs(rawSize.x),Mathf.Abs(rawSize.y),Mathf.Abs(rawSize.z));
                    }
                    continue;
                }
                BuildDetailMountain(root,t,rng,new Vector3(x,0f,z),i);
            }
        }

        void BuildDetailTree(Transform root,EraTheme t,System.Random rng,Vector3 pos,int idx)
        {
            var tr=new GameObject("ERA_Tree_"+idx); tr.transform.SetParent(root,false); tr.transform.position=pos;
            float th=R(rng,4f,9f),cr=R(rng,2f,4.5f);
            // Trunk: tapered using 2 cylinders
            Prim(PrimitiveType.Cylinder,tr.transform,"Trunk",new Vector3(0,th*.5f,0),new Vector3(.22f,th,.22f),Tint(t.accent,rng,.04f));
            Prim(PrimitiveType.Cylinder,tr.transform,"TrunkBase",new Vector3(0,.4f,0),new Vector3(.35f,.8f,.35f),Tint(t.accent,rng,.05f));
            // Root buttresses
            for(int r=0;r<3;r++){float ra=r*120f*Mathf.Deg2Rad;
                Prim(PrimitiveType.Cube,tr.transform,"Root",new Vector3(Mathf.Cos(ra)*.3f,.1f,Mathf.Sin(ra)*.3f),new Vector3(.12f,.2f,.5f),Tint(t.accent,rng,.05f),ra*Mathf.Rad2Deg);}
            // Canopy: 3 overlapping spheres for volume
            Prim(PrimitiveType.Sphere,tr.transform,"C1",new Vector3(0,th+cr*.5f,0),new Vector3(cr*2f,cr*2.2f,cr*2f),Tint(t.tree,rng,.05f));
            Prim(PrimitiveType.Sphere,tr.transform,"C2",new Vector3(cr*.4f,th+cr*.3f,cr*.3f),new Vector3(cr*1.4f,cr*1.6f,cr*1.4f),Tint(t.tree,rng,.06f));
            Prim(PrimitiveType.Sphere,tr.transform,"C3",new Vector3(-cr*.3f,th+cr*.6f,-cr*.2f),new Vector3(cr*1.2f,cr*1.4f,cr*1.2f),Tint(t.tree,rng,.06f));
        }

        void BuildDetailRock(Transform root,EraTheme t,System.Random rng,Vector3 pos,int idx)
        {
            var rg=new GameObject("ERA_Rock_"+idx); rg.transform.SetParent(root,false); rg.transform.position=pos;
            float rs=R(rng,.5f,2.8f);
            // Main boulder
            Prim(PrimitiveType.Sphere,rg.transform,"Main",new Vector3(0,rs*.4f,0),new Vector3(rs*2f,rs,rs*2.2f),Tint(t.rock,rng,.05f),R(rng,0f,360f));
            // Smaller satellite rocks
            int extras=RI(rng,1,4);
            for(int e=0;e<extras;e++){float ea=R(rng,0f,Mathf.PI*2f),er=R(rng,.3f,.8f)*rs,es=R(rng,.2f,.6f)*rs;
                Prim(PrimitiveType.Sphere,rg.transform,"Sub",new Vector3(Mathf.Cos(ea)*er,es*.3f,Mathf.Sin(ea)*er),new Vector3(es*1.8f,es,es*2f),Tint(t.rock,rng,.06f),R(rng,0f,360f));}
            // Crack lines (dark thin cubes)
            Prim(PrimitiveType.Cube,rg.transform,"Crack",new Vector3(0,rs*.5f,0),new Vector3(.04f,rs*.8f,rs*1.8f),new Color(.08f,.07f,.06f),R(rng,0f,180f));
        }

        void BuildDetailMountain(Transform root,EraTheme t,System.Random rng,Vector3 pos,int idx)
        {
            var assets=assetConfig!=null?assetConfig.Get("medieval"):null;
            if(assets==null) return;
            // Tree scatter runs regardless of rock prefab availability
            // Use era param t to get trees from the actual era config too
            var treeAssets=assets.treePrefabs;
            if(treeAssets==null||treeAssets.Length==0){var eraAssets=assetConfig.Get("medieval");treeAssets=eraAssets?.treePrefabs;}
            if(treeAssets!=null){
                GameObject mtnTree=null;
                foreach(var pfb in treeAssets){if(pfb!=null&&pfb.name=="tree_01"){mtnTree=pfb;break;}}
                if(mtnTree!=null){Debug.Log("[TR] tree_01 found, placing around mountain "+idx);
                    float baseScaleForTrees=R(rng,25f,50f);
                    for(int s=0;s<RI(rng,12,20);s++){
                        float sa=R(rng,0f,Mathf.PI*2f),sd=R(rng,baseScaleForTrees*.3f,baseScaleForTrees*.8f);
                        var go2=Instantiate(mtnTree,new Vector3(pos.x+Mathf.Cos(sa)*sd,1f,pos.z+Mathf.Sin(sa)*sd),Quaternion.Euler(0,R(rng,0f,360f),0),root);
                        go2.name="ERA_MtnTree_"+idx+"_"+s;
                        go2.transform.localScale=Vector3.one*R(rng,20.0f,35.0f);
                        var ut2=Shader.Find("Unlit/Texture");var uc2=Shader.Find("Unlit/Color");
                        if(ut2!=null&&uc2!=null){foreach(var rr in go2.GetComponentsInChildren<Renderer>(true)){var mm=rr.sharedMaterials;bool ch=false;for(int mi=0;mi<mm.Length;mi++){if(mm[mi]==null)continue;var sn=mm[mi].shader.name;if(sn=="Unlit/Texture"||sn=="Unlit/Color"||sn.StartsWith("Shader Graphs/"))continue;Texture tex=null;foreach(var prop in new[]{"_BaseMap","_MainTex","_Albedo","_Diffuse"}){if(mm[mi].HasProperty(prop)){tex=mm[mi].GetTexture(prop);if(tex!=null)break;}}if(tex!=null){var m2=new Material(ut2);m2.SetTexture("_MainTex",tex);mm[mi]=m2;ch=true;}else{Color c2=mm[mi].HasProperty("_BaseColor")?mm[mi].GetColor("_BaseColor"):mm[mi].HasProperty("_Color")?mm[mi].GetColor("_Color"):Color.grey;c2.a=1f;var m2=new Material(uc2);m2.color=c2;mm[mi]=m2;ch=true;}}if(ch)rr.sharedMaterials=mm;}}
                        foreach(var rr in go2.GetComponentsInChildren<Renderer>(true)){var mm=rr.sharedMaterials;bool ch2=false;for(int mi=0;mi<mm.Length;mi++){if(mm[mi]==null)continue;if(mm[mi].shader.name=="Unlit/Color"){Color c=mm[mi].color;if(c.r<0.20f&&c.g<0.35f&&c.b<0.20f){var m2=new Material(mm[mi]);m2.color=new Color(0.15f,0.45f,0.10f,1f);mm[mi]=m2;ch2=true;}}}if(ch2)rr.sharedMaterials=mm;}
                        foreach(var psr in go2.GetComponentsInChildren<ParticleSystemRenderer>(true)) psr.enabled=false;
                        foreach(var col in go2.GetComponentsInChildren<Collider>()) DestroyImmediate(col);
                    }
                }
            }
            if(assets.rockPrefabs==null||assets.rockPrefabs.Length==0) return;
            var boulderPfbs=new List<GameObject>();
            foreach(var pfb in assets.rockPrefabs){if(pfb!=null&&pfb.name.StartsWith("Rock"))boulderPfbs.Add(pfb);}
            if(boulderPfbs.Count==0) foreach(var pfb in assets.rockPrefabs){if(pfb!=null)boulderPfbs.Add(pfb);}
            int clusterCount=RI(rng,3,5);
            float baseScale=R(rng,25f,50f);
            for(int c=0;c<clusterCount;c++){
                float t2=c/(float)(clusterCount-1);
                float scale=Mathf.Lerp(baseScale,baseScale*R(rng,.2f,.4f),t2);
                float ox=R(rng,-baseScale*.35f,baseScale*.35f),oz=R(rng,-baseScale*.35f,baseScale*.35f);
                float oy=Mathf.Lerp(0f,scale*.25f,t2);
                var pfb=boulderPfbs[RI(rng,0,boulderPfbs.Count)];
                if(pfb==null) continue;
                var go=Instantiate(pfb,new Vector3(pos.x+ox,oy,pos.z+oz),Quaternion.Euler(R(rng,-20f,20f),R(rng,0f,360f),R(rng,-20f,20f)),root);
                go.name="ERA_Mountain_"+idx+"_"+c;
                go.transform.localScale=new Vector3(scale,scale*R(rng,.7f,1.2f),scale);
                var ut=Shader.Find("Unlit/Texture");var uc=Shader.Find("Unlit/Color");
                if(ut!=null&&uc!=null){foreach(var rr in go.GetComponentsInChildren<Renderer>(true)){var mm=rr.sharedMaterials;bool ch=false;for(int mi=0;mi<mm.Length;mi++){if(mm[mi]==null)continue;var sn=mm[mi].shader.name;if(sn=="Unlit/Texture"||sn=="Unlit/Color"||sn.StartsWith("Shader Graphs/"))continue;Texture tex=null;foreach(var prop in new[]{"_BaseMap","_MainTex","_Albedo","_Diffuse"}){if(mm[mi].HasProperty(prop)){tex=mm[mi].GetTexture(prop);if(tex!=null)break;}}if(tex!=null){var m2=new Material(ut);m2.SetTexture("_MainTex",tex);mm[mi]=m2;ch=true;}else{Color c2=mm[mi].HasProperty("_BaseColor")?mm[mi].GetColor("_BaseColor"):mm[mi].HasProperty("_Color")?mm[mi].GetColor("_Color"):Color.grey;c2.a=1f;var m2=new Material(uc);m2.color=c2;mm[mi]=m2;ch=true;}}if(ch)rr.sharedMaterials=mm;}}
                foreach(var psr in go.GetComponentsInChildren<ParticleSystemRenderer>(true)) psr.enabled=false;
                foreach(var col in go.GetComponentsInChildren<Collider>()) col.isTrigger=false;
            }
        }

        void BuildDetailCave(Transform root,EraTheme t,System.Random rng,Vector3 pos,int idx)
        {
            var cv=new GameObject("ERA_Cave_"+idx); cv.transform.SetParent(root,false); cv.transform.position=pos;
            float cw=R(rng,5f,9f),ch=R(rng,3.5f,7f),cd=R(rng,4f,8f);
            float rot=R(rng,0f,360f);
            cv.transform.localRotation=Quaternion.Euler(0,rot,0);
            Prim(PrimitiveType.Cube,cv.transform,"Interior",new Vector3(0,ch*.5f,-cd*.5f),new Vector3(cw,ch,cd),Tint(t.rock,rng,.05f));
            Prim(PrimitiveType.Sphere,cv.transform,"RL",new Vector3(-cw*.52f,ch*.45f,0f),new Vector3(cw*.35f,ch,cw*.3f),Tint(t.rock,rng,.04f));
            Prim(PrimitiveType.Sphere,cv.transform,"RR",new Vector3(cw*.52f,ch*.45f,0f),new Vector3(cw*.35f,ch,cw*.3f),Tint(t.rock,rng,.04f));
            Prim(PrimitiveType.Sphere,cv.transform,"OH",new Vector3(0f,ch*1.05f,0f),new Vector3(cw*1.2f,ch*.45f,cw*.7f),Tint(t.rock,rng,.03f));
            for(int s=0;s<5;s++){float sx=R(rng,-cw*.35f,cw*.35f),sz=R(rng,-cd*.3f,0f),sl=R(rng,.3f,1.2f);
                Prim(PrimitiveType.Cylinder,cv.transform,"Stal",new Vector3(sx,ch-sl*.5f,sz),new Vector3(.12f,sl,.12f),Tint(t.rock,rng,.06f));}
            for(int g=0;g<4;g++){float gx=R(rng,-cw*.3f,cw*.3f),gz=R(rng,-cd*.6f,-.2f),gs=R(rng,.15f,.5f);
                Prim(PrimitiveType.Sphere,cv.transform,"GRock",new Vector3(gx,gs*.2f,gz),new Vector3(gs*1.6f,gs*.7f,gs*1.8f),Tint(t.rock,rng,.05f));}
        }

        void BuildWater(Transform root,EraTheme t,string era,System.Random rng,int seed,List<Rect> placed)
        {
            if(!t.hasWater)return;
            var assets=assetConfig!=null?assetConfig.Get(era):null;
            var waterPlanePfb = assets?.waterPrefab;
            var waterPlanePrefab = waterPlanePfb;
            var waterMat=new Material(Shader.Find("Unlit/Color")); waterMat.color=t.water;
            var waterDeepMat=new Material(Shader.Find("Unlit/Color")); waterDeepMat.color=new Color(t.water.r*0.6f,t.water.g*0.6f,t.water.b*0.8f);
            void SpawnWaterSurface(float wx,float wz,float ww,float wd,Quaternion rot){
                if (waterPlanePrefab != null)
                {
                    var wp = Instantiate(waterPlanePrefab, new Vector3(wx, 0.02f, wz), rot, root);
                    wp.name = "ERA_Water";
                    var mc = wp.GetComponent<MeshCollider>(); if (mc != null) mc.enabled = false;
                    var ws = wp.GetComponent<Water_Settings>(); if (ws != null) ws.enabled = false;
                    wp.transform.localScale = new Vector3(ww * 0.1f, 1f, wd * 0.1f);
                    var mr = wp.GetComponent<MeshRenderer>();
                    if (mr != null) { var mat = new Material(Shader.Find("Unlit/Color")); mat.color = t.water; mr.sharedMaterial = mat; }
                }
                else
                {
                    var wp=GameObject.CreatePrimitive(PrimitiveType.Plane); wp.name="ERA_Water";
                    wp.transform.SetParent(root,false); wp.transform.position=new Vector3(wx,0.02f,wz);
                    wp.transform.rotation=rot; wp.transform.localScale=new Vector3(ww*0.1f,1f,wd*0.1f);
                    wp.GetComponent<Renderer>().sharedMaterial=waterMat; DestroyImmediate(wp.GetComponent<Collider>());
                    var wd2=GameObject.CreatePrimitive(PrimitiveType.Plane); wd2.name="ERA_Water";
                    wd2.transform.SetParent(root,false); wd2.transform.position=new Vector3(wx,-0.01f,wz);
                    wd2.transform.rotation=rot; wd2.transform.localScale=new Vector3(ww*0.05f,1f,wd*0.05f);
                    wd2.GetComponent<Renderer>().sharedMaterial=waterDeepMat; DestroyImmediate(wd2.GetComponent<Collider>());
                }
            }
            void SpawnLake(float lx,float lz,float lw,float ld){
                if(new Vector2(lx,lz).magnitude < 80f) return;
                var rect=new Rect(lx-lw*.5f,lz-ld*.5f,lw,ld); if(Overlaps(placed,rect,20f))return; placed.Add(rect);
                SpawnWaterSurface(lx,lz,lw,ld,Quaternion.identity);
                var trig=new GameObject("ERA_WaterTrigger"); trig.transform.SetParent(root,false);
                trig.transform.position=new Vector3(lx,-1.8f,lz); trig.layer=0;
                var tbc=trig.AddComponent<BoxCollider>(); tbc.isTrigger=true; tbc.size=new Vector3(lw,4f,ld);
                var trb=trig.AddComponent<Rigidbody>(); trb.isKinematic=true; trb.useGravity=false;
                var tst=trig.AddComponent<Kit_SwimmingTrigger>(); tst.col=tbc; tst.exitAdjustment=Vector3.zero; tst.enableBouyancyForEnteringRigidbodies=true;
                var fl=new GameObject("ERA_LakeFloor"); fl.transform.SetParent(root,false);
                fl.transform.position=new Vector3(lx,-5f,lz);
                fl.AddComponent<BoxCollider>().size=new Vector3(lw+4f,0.2f,ld+4f);
            }
            if(t.hasBridge){
                float angle=R(rng,-20f,20f),width=R(rng,30f,60f); var q=Quaternion.Euler(0,angle,0);
                riverWidth=width; riverAngle=angle; hasRiver=true;
                SpawnWaterSurface(0f,0f,width,MAP*2f,q);
                var wdeep=GameObject.CreatePrimitive(PrimitiveType.Plane); wdeep.name="ERA_Water";
                wdeep.transform.SetParent(root,false); wdeep.transform.position=q*new Vector3(0,-0.01f,0); wdeep.transform.rotation=q;
                wdeep.transform.localScale=new Vector3(width*0.06f,1f,MAP*2f*0.1f);
                wdeep.GetComponent<Renderer>().sharedMaterial=waterDeepMat; DestroyImmediate(wdeep.GetComponent<Collider>());
                placed.Add(new Rect(-width*.5f-3f,-MAP,width+6f,MAP*2f));
                var riverFloor=new GameObject("ERA_RiverFloor"); riverFloor.transform.SetParent(root,false);
                riverFloor.transform.position=q*new Vector3(0,-5f,0); riverFloor.transform.rotation=q;
                riverFloor.AddComponent<BoxCollider>().size=new Vector3(width+4f,0.2f,MAP*2f);
                float bw=R(rng,3.5f,5.5f);
                float bridgeGap=bw*0.5f+2f; float segLen=Mathf.Min(PLAY*2f,(MAP-bridgeGap)*0.5f); float segOffset=bridgeGap+segLen*0.5f;
                for(int side=-1;side<=1;side+=2){
                    var wtrig=new GameObject("ERA_WaterTrigger"); wtrig.transform.SetParent(root,false);
                    wtrig.transform.position=q*new Vector3(0,-1.8f,side*segOffset); wtrig.transform.rotation=q; wtrig.layer=0;
                    var wbc=wtrig.AddComponent<BoxCollider>(); wbc.isTrigger=true; wbc.size=new Vector3(width,4f,segLen);
                    var wst=wtrig.AddComponent<Kit_SwimmingTrigger>(); wst.col=wbc; wst.exitAdjustment=Vector3.zero; wst.enableBouyancyForEnteringRigidbodies=true;
                    Debug.Log($"[TR] River trigger created: pos={wtrig.transform.position} scale={wtrig.transform.localScale} col={wbc.isTrigger}");
                    var wdrown=new GameObject("ERA_WaterDrown"); wdrown.transform.SetParent(root,false);
                    wdrown.transform.position=q*new Vector3(0,-2.5f,side*segOffset); wdrown.transform.rotation=q;
                    wdrown.transform.localScale=new Vector3(width*0.6f,2f,segLen); wdrown.layer=0;
                    var wdbc=wdrown.AddComponent<BoxCollider>(); wdbc.isTrigger=true;
                    wdrown.AddComponent<Kit_SwimmingTrigger>().col=wdbc;
                }
                placed.Add(new Rect(-(width*.5f+6f),-bw*.5f-4f,width+12f,bw+8f));
                var br=new GameObject("ERA_Bridge"); br.transform.SetParent(root,false);
                br.transform.position=new Vector3(0,0f,0); br.transform.rotation=q;
                Prim(PrimitiveType.Cube,br.transform,"Deck",new Vector3(0,.1f,0),new Vector3(width+1f,.22f,bw),Tint(t.wallAlt,rng));
                int planks=Mathf.RoundToInt((width+1f)/0.6f);
                for(int pl=0;pl<planks;pl++){float px2=-width*.5f+pl*.6f;
                    Prim(PrimitiveType.Cube,br.transform,"Plank",new Vector3(px2,.22f,0),new Vector3(.45f,.08f,bw),Tint(t.wallAlt,rng,.04f));}
                int posts=Mathf.RoundToInt((width+1f)/1.5f);
                for(int po=0;po<=posts;po++){float px2=-width*.5f+po*(width+1f)/posts;
                    Prim(PrimitiveType.Cube,br.transform,"PostL",new Vector3(px2,.7f,-bw*.5f+.1f),new Vector3(.12f,1.2f,.12f),Tint(t.wall,rng));
                    Prim(PrimitiveType.Cube,br.transform,"PostR",new Vector3(px2,.7f,bw*.5f-.1f),new Vector3(.12f,1.2f,.12f),Tint(t.wall,rng));}
                Prim(PrimitiveType.Cube,br.transform,"RailL",new Vector3(0,1.3f,-bw*.5f+.1f),new Vector3(width+1f,.08f,.08f),Tint(t.wall,rng));
                Prim(PrimitiveType.Cube,br.transform,"RailR",new Vector3(0,1.3f,bw*.5f-.1f),new Vector3(width+1f,.08f,.08f),Tint(t.wall,rng));
                var bridgeCol=br.AddComponent<BoxCollider>(); bridgeCol.center=new Vector3(0,0.16f,0); bridgeCol.size=new Vector3(width+1f,0.32f,bw);
                var railColL=br.AddComponent<BoxCollider>(); railColL.center=new Vector3(0,0.8f,-bw*0.5f+0.1f); railColL.size=new Vector3(width+1f,1.6f,0.2f);
                var railColR=br.AddComponent<BoxCollider>(); railColR.center=new Vector3(0,0.8f,bw*0.5f-0.1f); railColR.size=new Vector3(width+1f,1.6f,0.2f);
            }
            int lakeCount=RI(rng,3,6);
            for(int li=0;li<lakeCount;li++){
                float la=R(rng,0f,Mathf.PI*2f),ldist=R(rng,PLAY*0.3f,PLAY*0.85f);
                SpawnLake(Mathf.Cos(la)*ldist,Mathf.Sin(la)*ldist,R(rng,150f,400f),R(rng,120f,350f));
            }
            int pondCount=RI(rng,2,4);
            for(int pi2=0;pi2<pondCount;pi2++){
                float pa=R(rng,0f,Mathf.PI*2f),pdist=R(rng,PLAY*0.2f,PLAY*0.6f);
                SpawnLake(Mathf.Cos(pa)*pdist,Mathf.Sin(pa)*pdist,R(rng,50f,120f),R(rng,40f,100f));
            }
        }

        void BuildExitPortal(Transform root,EraTheme t,System.Random rng,int seed,List<Rect> placed)
        {
            float a=R(rng,0f,360f)*Mathf.Deg2Rad,d=R(rng,20f,PLAY*.7f);
            float px=Mathf.Cos(a)*d,pz=Mathf.Sin(a)*d;
            placed.Add(new Rect(px-6f,pz-6f,12f,12f));
            var portal=new GameObject("ERA_ExitPortal"); portal.transform.SetParent(root,false); portal.transform.position=new Vector3(px,0f,pz);
            Prim(PrimitiveType.Cylinder,portal.transform,"Base",new Vector3(0,.4f,0),new Vector3(5f,.8f,5f),Tint(t.wallAlt,rng));
            for(int s=0;s<3;s++) Prim(PrimitiveType.Cylinder,portal.transform,"Step",new Vector3(0,(s+1)*.2f,0),new Vector3(5f+s*.5f,.2f,5f+s*.5f),Tint(t.wall,rng,.02f));
            Prim(PrimitiveType.Cylinder,portal.transform,"Spire",new Vector3(0,9f,0),new Vector3(1.1f,18f,1.1f),Tint(t.accent,rng));
            var portalOrb=Prim(PrimitiveType.Sphere,portal.transform,"Orb",new Vector3(0,19f,0),new Vector3(2.5f,2.5f,2.5f),t.accent); DestroyImmediate(portalOrb.GetComponent<Collider>());
            for(int r=0;r<3;r++) Prim(PrimitiveType.Cylinder,portal.transform,"Halo",new Vector3(0,5f+r*4f,0),new Vector3(4f+r,.25f,4f+r),t.accent*.7f);
            for(int i=0;i<4;i++){float pa=i*90f*Mathf.Deg2Rad;
                Prim(PrimitiveType.Cylinder,portal.transform,"Pillar",new Vector3(Mathf.Cos(pa)*2.5f,2f,Mathf.Sin(pa)*2.5f),new Vector3(.4f,4f,.4f),Tint(t.wallAlt,rng));
                var pTop=Prim(PrimitiveType.Sphere,portal.transform,"PTop",new Vector3(Mathf.Cos(pa)*2.5f,4.3f,Mathf.Sin(pa)*2.5f),new Vector3(.5f,.5f,.5f),t.accent); DestroyImmediate(pTop.GetComponent<Collider>());}
            var comp=portal.AddComponent<MultiverseExitPortal>(); comp.accentColor=t.accent;
        }
    }
}
