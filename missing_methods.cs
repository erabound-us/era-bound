        void BuildDetailCave(Transform root,EraTheme t,System.Random rng,Vector3 pos,int idx)
        {
            var cv=new GameObject("ERA_Cave_"+idx); cv.transform.SetParent(root,false); cv.transform.position=pos;
            float cw=R(rng,5f,9f),ch=R(rng,3.5f,7f),cd=R(rng,4f,8f);
            float rot=R(rng,0f,360f);
            cv.transform.localRotation=Quaternion.Euler(0,rot,0);
            Prim(PrimitiveType.Cube,cv.transform,"Interior",new Vector3(0,ch*.5f,-cd*.5f),new Vector3(cw,ch,cd),new Color(.03f,.03f,.03f));
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
