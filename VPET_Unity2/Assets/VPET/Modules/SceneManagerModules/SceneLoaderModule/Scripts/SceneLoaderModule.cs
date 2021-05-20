/*
-------------------------------------------------------------------------------
VPET - Virtual Production Editing Tools
vpet.research.animationsinstitut.de
https://github.com/FilmakademieRnd/VPET
 
Copyright (c) 2021 Filmakademie Baden-Wuerttemberg, Animationsinstitut R&D Lab
 
This project has been initiated in the scope of the EU funded project 
Dreamspace (http://dreamspaceproject.eu/) under grant agreement no 610005 2014-2016.
 
Post Dreamspace the project has been further developed on behalf of the 
research and development activities of Animationsinstitut.
 
In 2018 some features (Character Animation Interface and USD support) were
addressed in the scope of the EU funded project  SAUCE (https://www.sauceproject.eu/) 
under grant agreement no 780470, 2018-2020
 
VPET consists of 3 core components: VPET Unity Client, Scene Distribution and
Syncronisation Server. They are licensed under the following terms:
-------------------------------------------------------------------------------
*/

//! @file "SceneLoaderModule.cs"
//! @brief implementation of VPET scene loader module
//! @author Simon Spielmann
//! @author Jonas Trottnow
//! @version 0
//! @date 23.04.2021

using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace vpet
{
    //!
    //! implementation of unity scene loader module
    //!
    public class SceneLoaderModule : Module
    {
        //! The list storing Unity materials in scene.
        public static List<Material> SceneMaterialList = new List<Material>();
        
        //! The list storing Unity textures in scene.
        public static List<Texture2D> SceneTextureList = new List<Texture2D>();
       
        //! The list storing Unity meshes in scene.
        public static List<Mesh> SceneMeshList = new List<Mesh>();
        
        // [REVIEW]
        //! The list storing editable Unity game objects in scene.
        public static List<GameObject> SceneEditableObjects = new List<GameObject>();
        
        //! The list storing selectable Unity lights in scene.
        public static List<GameObject> SelectableLights = new List<GameObject>();
        
        //! The list storing Unity cameras in scene.
        public static List<GameObject> SceneCameraList = new List<GameObject>();
       
        //! The list storing all Unity gameObjects in scene.
        public static List<GameObject> gameObjectList = new List<GameObject>();

        public static GameObject scnRoot;

        //!
        //! constructor
        //! @param   name    Name of this module
        //!
        public SceneLoaderModule(string name) : base(name) => name = base.name;

        // [REVIEW]
        //! to be replaced
        public void Test()
        {
            UnitySceneLoaderModule m = (UnitySceneLoaderModule)manager.getModule(typeof(UnitySceneLoaderModule));        

        }

        //!
        //! Function that creates the Unity scene content.
        //!
        public void LoadScene()
        {
            SceneDataHandler sceneDataHandler = new SceneDataHandler();

            // create scene parent if not there
            GameObject scnPrtGO = GameObject.Find("Scene");
            if (scnPrtGO == null)
            {
                scnPrtGO = new GameObject("Scene");
            }

            GameObject scnRoot = scnPrtGO.transform.Find("root").gameObject;
            if (scnRoot == null)
            {
                scnRoot = new GameObject("root");
                scnRoot.transform.parent = scnPrtGO.transform;
            }

            Helpers.Log(string.Format("Build scene from: {0} objects, {1} textures, {2} materials, {3} nodes", sceneDataHandler.ObjectList.Count, sceneDataHandler.TextureList.Count, sceneDataHandler.MaterialList.Count, sceneDataHandler.NodeList.Count));

            createMaterials(ref sceneDataHandler);
            
            if (SceneManager.Settings.loadTextures)
                createTextures(ref sceneDataHandler);

            createMeshes(ref sceneDataHandler);

            createSceneGraphIter(ref sceneDataHandler, scnRoot.transform);

            createSkinnedMeshes(ref sceneDataHandler, scnRoot.transform);
        }

        //!
        //! Function that creates the materials in the Unity scene.
        //!
        //! @param sceneDataHandler A reference to the actual VPET sceneDataHandler.
        //!
        private void createMaterials(ref SceneDataHandler sceneDataHandler)
        {
            foreach (MaterialPackage matPack in sceneDataHandler.MaterialList)
            {
                if (matPack.type == 1)
                {
                    Material mat = Resources.Load(string.Format("VPET/Materials/{0}", matPack.src), typeof(Material)) as Material;
                    if (mat)
                        SceneMaterialList.Add(mat);
                    else
                    {
                        Debug.LogWarning(string.Format("[{0} createMaterials]: Cant find Resource: {1}. Create Standard.", this.GetType(), matPack.src));
                        Material _mat = new Material(Shader.Find("Standard"));
                        _mat.name = matPack.name;
                        SceneMaterialList.Add(_mat);
                    }
                }
                else if (matPack.type == 2)
                {
                    Debug.Log(matPack.src);
                    Material mat = new Material(Shader.Find(matPack.src));
                    mat.name = matPack.name;
                    SceneMaterialList.Add(mat);
                }
            }
        }

        //!
        //! Function that creates the textures in the Unity scene.
        //!
        //! @param sceneDataHandler A reference to the actual VPET sceneDataHandler.
        //!
        private void createTextures(ref SceneDataHandler sceneDataHandler)
        {
            foreach (TexturePackage texPack in sceneDataHandler.TextureList)
            {
                if (sceneDataHandler.TextureBinaryType == 1)
                {
                    Texture2D tex_2d = new Texture2D(texPack.width, texPack.height, texPack.format, false);
                    tex_2d.LoadRawTextureData(texPack.colorMapData);
                    tex_2d.Apply();
                    SceneTextureList.Add(tex_2d);
                }
                else
                {
#if UNITY_IPHONE
                    Texture2D tex_2d = new Texture2D(16, 16, TextureFormat.PVRTC_RGBA4, false);
#else
                    Texture2D tex_2d = new Texture2D(16, 16, TextureFormat.DXT5Crunched, false);
#endif
                    tex_2d.LoadImage(texPack.colorMapData);
                    SceneTextureList.Add(tex_2d);
                }

            }
        }

        //!
        //! Function that creates the meshes in the Unity scene.
        //!
        //! @param sceneDataHandler A reference to the actual VPET sceneDataHandler.
        //!
        private void createMeshes(ref SceneDataHandler sceneDataHandler)
        {
            foreach (ObjectPackage objPack in sceneDataHandler.ObjectList)
            {
                Vector3[] vertices = new Vector3[objPack.vSize];
                Vector3[] normals = new Vector3[objPack.nSize];
                Vector2[] uv = new Vector2[objPack.uvSize];
                BoneWeight[] weights = new BoneWeight[objPack.bWSize];

                for (int i = 0; i < objPack.bWSize; i++)
                {
                    BoneWeight b = new BoneWeight();
                    b.weight0 = objPack.boneWeights[i * 4 + 0];
                    b.weight1 = objPack.boneWeights[i * 4 + 1];
                    b.weight2 = objPack.boneWeights[i * 4 + 2];
                    b.weight3 = objPack.boneWeights[i * 4 + 3];
                    b.boneIndex0 = objPack.boneIndices[i * 4 + 0];
                    b.boneIndex1 = objPack.boneIndices[i * 4 + 1];
                    b.boneIndex2 = objPack.boneIndices[i * 4 + 2];
                    b.boneIndex3 = objPack.boneIndices[i * 4 + 3];
                    weights[i] = b;
                }

                for (int i = 0; i < objPack.vSize; i++)
                {
                    Vector3 v = new Vector3(objPack.vertices[i * 3 + 0], objPack.vertices[i * 3 + 1], objPack.vertices[i * 3 + 2]);
                    vertices[i] = v;
                }

                for (int i = 0; i < objPack.nSize; i++)
                {
                    Vector3 v = new Vector3(objPack.normals[i * 3 + 0], objPack.normals[i * 3 + 1], objPack.normals[i * 3 + 2]);
                    normals[i] = v;
                }

                for (int i = 0; i < objPack.uvSize; i++)
                {
                    Vector2 v2 = new Vector2(objPack.uvs[i * 2 + 0], objPack.uvs[i * 2 + 1]);
                    uv[i] = v2;
                }

                Mesh mesh = new Mesh();
                mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
                mesh.vertices = vertices;
                mesh.normals = normals;
                mesh.uv = uv;
                mesh.triangles = objPack.indices;
                mesh.boneWeights = weights;

                SceneMeshList.Add(mesh);
            }
        }

        // [REVIEW]
        //!
        //! Function that recusively creates the gameObjects in the Unity scene.
        //!
        //! @param sceneDataHandler A reference to the actual VPET sceneDataHandler.
        //! @param parent the parent Unity transform.
        //! @param idx The index for referencing into the node list.
        //!
        private int createSceneGraphIter(ref SceneDataHandler sceneDataHandler, Transform parent, int idx = 0)
        {
            SceneNode node = sceneDataHandler.NodeList[idx];

            // process all registered build callbacks
            GameObject obj = CreateObject(node, parent);

            gameObjectList.Add(obj);

            // add scene object to editable 
            if (node.editable)
            {
                SceneEditableObjects.Add(obj);
            }

            // recursive call
            int idxChild = idx;
            for (int k = 1; k <= node.childCount; k++)
            {
                idxChild = createSceneGraphIter(ref sceneDataHandler, obj.transform, idxChild + 1);
            }

            return idxChild;
        }

        //!
        //! Function that recusively creates the gameObjects in the Unity scene.
        //!
        //! @param sceneDataHandler A reference to the scene data handler.
        //! @param sceneDataHandler A reference to the actual VPET sceneDataHandler.
        //!
        private void createSkinnedMeshes(ref SceneDataHandler sceneDataHandler, Transform root)
        {
            List<CharacterPackage> characterList = sceneDataHandler.CharacterList;

            createSkinnedRendererIter(ref sceneDataHandler, root);

            //setup characters
            foreach (CharacterPackage cp in characterList)
            {
                GameObject obj = gameObjectList[cp.rootId];
                Transform parentBackup = obj.transform.parent;
                obj.transform.parent = GameObject.Find("Scene").transform.parent;
                HumanBone[] human = new HumanBone[cp.bMSize];
                for (int i = 0; i < human.Length; i++)
                {
                    int boneMapping = cp.boneMapping[i];
                    if (boneMapping == -1)
                        continue;
                    GameObject boneObj = gameObjectList[boneMapping];
                    human[i].boneName = boneObj.name;
                    human[i].humanName = ((HumanBodyBones)i).ToString();
                    human[i].limit.useDefaultValues = true;
                }
                SkeletonBone[] skeleton = new SkeletonBone[cp.sSize];
                skeleton[0].name = obj.name;
                skeleton[0].position = new Vector3(cp.bonePosition[0], cp.bonePosition[1], cp.bonePosition[2]);
                skeleton[0].rotation = new Quaternion(cp.boneRotation[0], cp.boneRotation[1], cp.boneRotation[2], cp.boneRotation[3]);
                skeleton[0].scale = new Vector3(cp.boneScale[0], cp.boneScale[1], cp.boneScale[2]);

                for (int i = 1; i < cp.skeletonMapping.Length; i++)
                {
                    if (cp.skeletonMapping[i] != -1)
                    {
                        skeleton[i].name = gameObjectList[cp.skeletonMapping[i]].name;
                        skeleton[i].position = new Vector3(cp.bonePosition[i * 3], cp.bonePosition[i * 3 + 1], cp.bonePosition[i * 3 + 2]);
                        skeleton[i].rotation = new Quaternion(cp.boneRotation[i * 4], cp.boneRotation[i * 4 + 1], cp.boneRotation[i * 4 + 2], cp.boneRotation[i * 4 + 3]);
                        skeleton[i].scale = new Vector3(cp.boneScale[i * 3], cp.boneScale[i * 3 + 1], cp.boneScale[i * 3 + 2]);
                    }
                }
                HumanDescription humanDescription = new HumanDescription();
                humanDescription.human = human;
                humanDescription.skeleton = skeleton;
                humanDescription.upperArmTwist = 0.5f;
                humanDescription.lowerArmTwist = 0.5f;
                humanDescription.upperLegTwist = 0.5f;
                humanDescription.lowerLegTwist = 0.5f;
                humanDescription.armStretch = 0.05f;
                humanDescription.legStretch = 0.05f;
                humanDescription.feetSpacing = 0.0f;
                humanDescription.hasTranslationDoF = false;

                Avatar avatar = AvatarBuilder.BuildHumanAvatar(obj, humanDescription);
                if (avatar.isValid == false || avatar.isHuman == false)
                {
                    Helpers.Log(GetType().FullName + ": Unable to create source Avatar for retargeting. Check that your Skeleton Asset Name and Bone Naming Convention are configured correctly.", Helpers.logMsgType.ERROR);
                    return;
                }
                avatar.name = obj.name;
                Animator animator = obj.AddComponent<Animator>();
                animator.avatar = avatar;
                animator.applyRootMotion = true;

                animator.runtimeAnimatorController = (RuntimeAnimatorController)Instantiate(Resources.Load("VPET/Prefabs/AnimatorController"));
                obj.AddComponent<CharacterAnimationController>();

                obj.transform.parent = parentBackup;
            }

        }

        //!
        //! Creates an GameObject from an VPET SceneNode beneath the parent transform.
        //! @param node VPET SceneNode to be parsed.
        //! @param parentTransform Unity parent transform of the GameObject to-be created.
        //! @return The created GameObject.
        //!
        private GameObject CreateObject(SceneNode node, Transform parentTransform)
        {
            GameObject objMain;

            // Transform / convert handiness
            Vector3 pos = new Vector3(node.position[0], node.position[1], node.position[2]);

            // Rotation / convert handiness
            Quaternion rot = new Quaternion(node.rotation[0], node.rotation[1], node.rotation[2], node.rotation[3]);

            // Scale
            Vector3 scl = new Vector3(node.scale[0], node.scale[1], node.scale[2]);

            if (!parentTransform.Find(Encoding.ASCII.GetString(node.name)))
            {
                // set up object basics
                objMain = new GameObject();
                objMain.name = Encoding.ASCII.GetString(node.name);

                if (node.GetType() == typeof(SceneNodeGeo) || node.GetType() == typeof(SceneNodeSkinnedGeo))
                {
                    SceneNodeGeo nodeGeo = (SceneNodeGeo)node;
                    // Material Properties and Textures
                    Material mat;
                    // assign material from material list
                    if (nodeGeo.materialId > -1 && nodeGeo.materialId < SceneMaterialList.Count)
                    {
                        mat = SceneMaterialList[nodeGeo.materialId];
                    }
                    else // or set standard
                    {
                        mat = new Material(Shader.Find("Standard"));
                    }

                    // map properties
                    if (SceneManager.Settings.loadSampleScene)
                    {
                        MapMaterialProperties(mat, nodeGeo);
                    }

                    // Add Material
                    Renderer renderer;
                    if (nodeGeo.GetType() == typeof(SceneNodeSkinnedGeo))
                        renderer = objMain.AddComponent<SkinnedMeshRenderer>();
                    else
                        renderer = objMain.AddComponent<MeshRenderer>();

                    renderer.material = mat;

                    // Add Mesh
                    if (nodeGeo.geoId > -1 && nodeGeo.geoId < SceneMeshList.Count)
                    {
                        Mesh mesh = SceneMeshList[nodeGeo.geoId];

                        SceneManager.Settings.sceneBoundsMax = Vector3.Max(SceneManager.Settings.sceneBoundsMax, renderer.bounds.max);
                        SceneManager.Settings.sceneBoundsMin = Vector3.Min(SceneManager.Settings.sceneBoundsMin, renderer.bounds.min);

                        if (node.GetType() == typeof(SceneNodeSkinnedGeo))
                        {
                            SkinnedMeshRenderer sRenderer = (SkinnedMeshRenderer)renderer;
                            SceneNodeSkinnedGeo sNodeGeo = (SceneNodeSkinnedGeo)node;
                            Bounds bounds = new Bounds(new Vector3(sNodeGeo.boundCenter[0], sNodeGeo.boundCenter[1], sNodeGeo.boundCenter[2]),
                                                   new Vector3(sNodeGeo.boundExtents[0] * 2f, sNodeGeo.boundExtents[1] * 2f, sNodeGeo.boundExtents[2] * 2f));
                            sRenderer.localBounds = bounds;
                            Matrix4x4[] bindposes = new Matrix4x4[sNodeGeo.bindPoseLength];
                            for (int i = 0; i < sNodeGeo.bindPoseLength; i++)
                            {
                                bindposes[i] = new Matrix4x4();
                                bindposes[i][0, 0] = sNodeGeo.bindPoses[i * 16];
                                bindposes[i][0, 1] = sNodeGeo.bindPoses[i * 16 + 1];
                                bindposes[i][0, 2] = sNodeGeo.bindPoses[i * 16 + 2];
                                bindposes[i][0, 3] = sNodeGeo.bindPoses[i * 16 + 3];
                                bindposes[i][1, 0] = sNodeGeo.bindPoses[i * 16 + 4];
                                bindposes[i][1, 1] = sNodeGeo.bindPoses[i * 16 + 5];
                                bindposes[i][1, 2] = sNodeGeo.bindPoses[i * 16 + 6];
                                bindposes[i][1, 3] = sNodeGeo.bindPoses[i * 16 + 7];
                                bindposes[i][2, 0] = sNodeGeo.bindPoses[i * 16 + 8];
                                bindposes[i][2, 1] = sNodeGeo.bindPoses[i * 16 + 9];
                                bindposes[i][2, 2] = sNodeGeo.bindPoses[i * 16 + 10];
                                bindposes[i][2, 3] = sNodeGeo.bindPoses[i * 16 + 11];
                                bindposes[i][3, 0] = sNodeGeo.bindPoses[i * 16 + 12];
                                bindposes[i][3, 1] = sNodeGeo.bindPoses[i * 16 + 13];
                                bindposes[i][3, 2] = sNodeGeo.bindPoses[i * 16 + 14];
                                bindposes[i][3, 3] = sNodeGeo.bindPoses[i * 16 + 15];
                            }
                            mesh.bindposes = bindposes;
                            sRenderer.sharedMesh = mesh;

                        }
                        else
                        {
                            objMain.AddComponent<MeshFilter>();
                            objMain.GetComponent<MeshFilter>().mesh = mesh;
                        }
                    }

                    if (nodeGeo.editable)
                    {
                        SceneObject sceneObject = objMain.AddComponent<SceneObject>();
                    }
                }
                else if (node.GetType() == typeof(SceneNodeLight))
                {
                    SceneNodeLight nodeLight = (SceneNodeLight)node;

                    // Add light prefab
                    GameObject lightUber = Resources.Load<GameObject>("VPET/Prefabs/UberLight");
                    GameObject _lightUberInstance = GameObject.Instantiate(lightUber);
                    _lightUberInstance.name = lightUber.name;
                    lightUber.transform.GetChild(0).gameObject.layer = 8;


                    Light lightComponent = _lightUberInstance.GetComponent<Light>();
                    // instert type here!!!
                    lightComponent.type = nodeLight.lightType;
                    lightComponent.color = new Color(nodeLight.color[0], nodeLight.color[1], nodeLight.color[2]);
                    lightComponent.intensity = nodeLight.intensity * SceneManager.Settings.lightIntensityFactor;
                    lightComponent.spotAngle = nodeLight.angle;
                    if (lightComponent.type == LightType.Directional)
                    {
                        lightComponent.shadows = LightShadows.Soft;
                        lightComponent.shadowStrength = 0.8f;
                    }
                    else
                        lightComponent.shadows = LightShadows.None;
                    lightComponent.shadowBias = 0f;
                    lightComponent.shadowNormalBias = 1f;
                    lightComponent.range = nodeLight.range * SceneManager.Settings.sceneScale;

                    Debug.Log("Create Light: " + nodeLight.name + " of type: " + nodeLight.lightType.ToString() + " Intensity: " + nodeLight.intensity + " Pos: " + pos);

                    // Add light specific settings
                    if (nodeLight.lightType == LightType.Directional)
                    {
                    }
                    else if (nodeLight.lightType == LightType.Spot)
                    {
                        lightComponent.range *= 2;
                        //objMain.transform.Rotate(new Vector3(0, 180f, 0), Space.Self);
                    }
                    else if (nodeLight.lightType == LightType.Area)
                    {
                        // TODO: use are lights when supported in unity
                        lightComponent.spotAngle = 170;
                        lightComponent.range *= 4;
                    }
                    else
                    {
                        Debug.Log("Unknown Light Type in NodeBuilderBasic::CreateLight");
                    }

                    // parent 
                    _lightUberInstance.transform.SetParent(objMain.transform, false);

                    LightObject sco = objMain.AddComponent<LightObject>();

                    SelectableLights.Add(objMain);

                }
                else if (node.GetType() == typeof(SceneNodeCam))
                {
                    SceneNodeCam nodeCam = (SceneNodeCam)node;

                    // add camera dummy mesh
                    GameObject cameraObject = Resources.Load<GameObject>("VPET/Prefabs/cameraObject");
                    GameObject cameraInstance = GameObject.Instantiate(cameraObject);
                    cameraInstance.SetActive(false);
                    cameraInstance.name = cameraObject.name;
                    cameraInstance.transform.SetParent(objMain.transform, false);
                    cameraInstance.transform.localScale = new Vector3(1, 1, 1) * SceneManager.Settings.sceneScale;
                    cameraInstance.transform.localPosition = new Vector3(0, 0, 0);
                    cameraInstance.transform.localRotation = Quaternion.AngleAxis(180, Vector3.up);

                    // add camera data script and set values
                    //if (nodeCam.editable)
                    //{
                    CameraObject sco = objMain.AddComponent<CameraObject>();
                    sco.fov.value = nodeCam.fov;
                    sco.near.value = nodeCam.near;
                    sco.far.value = nodeCam.far;

                    SceneCameraList.Add(objMain);
                }

                Vector3 sceneExtends = SceneManager.Settings.sceneBoundsMax - SceneManager.Settings.sceneBoundsMin;
                SceneManager.Settings.maxExtend = Mathf.Max(Mathf.Max(sceneExtends.x, sceneExtends.y), sceneExtends.z);

                //place object
                objMain.transform.parent = parentTransform; // GameObject.Find( "Scene" ).transform;
            }
            else
            {
                objMain = parentTransform.Find(Encoding.ASCII.GetString(node.name)).gameObject;
            }

            objMain.transform.localPosition = pos;
            objMain.transform.localRotation = rot;
            objMain.transform.localScale = scl;

            return objMain;

        }

        //! 
        //! [REVIEW]
        //!
        public static void MapMaterialProperties(Material material, SceneNodeGeo nodeGeo)
        {
            /*
            //available parameters in this physically based standard shader:
            // _Color                   diffuse color (color including alpha)
            // _MainTex                 diffuse texture (2D texture)
            // _MainTex_ST
            // _Cutoff                  alpha cutoff
            // _Glossiness              smoothness of surface
            // _Metallic                matallic look of the material
            // _MetallicGlossMap        metallic texture (2D texture)
            // _BumpScale               scale of the bump map (float)
            // _BumpMap                 bumpmap (2D texture)
            // _Parallax                scale of height map
            // _ParallaxMap             height map (2D texture)
            // _OcclusionStrength       scale of occlusion
            // _OcclusionMap            occlusionMap (2D texture)
            // _EmissionColor           color of emission (color without alpha)
            // _EmissionMap             emission strength map (2D texture)
            // _DetailMask              detail mask (2D texture)
            // _DetailAlbedoMap         detail diffuse texture (2D texture)
            // _DetailAlbedoMap_ST
            // _DetailNormalMap
            // _DetailNormalMapScale    scale of detail normal map (float)
            // _DetailAlbedoMap         detail normal map (2D texture)
            // _UVSec                   UV Set for secondary textures (float)
            // _Mode                    rendering mode (float) 0 -> Opaque , 1 -> Cutout , 2 -> Transparent
            // _SrcBlend                source blend mode (enum is UnityEngine.Rendering.BlendMode)
            // _DstBlend                destination blend mode (enum is UnityEngine.Rendering.BlendMode)
            // test texture
            // WWW www = new WWW("file://F:/XML3D_Examples/tex/casual08a.jpg");
            // Texture2D texture = www.texture;
            foreach (KeyValuePair<string, KeyValuePair<string, Type>> pair in VPETSettings.ShaderPropertyMap)
            {
                FieldInfo fieldInfo = nodeGeo.GetType().GetField(pair.Value.Key, BindingFlags.Instance | BindingFlags.Public);
                Type propertyType = pair.Value.Value;

                if (material.HasProperty(pair.Key) && fieldInfo != null)
                {
                    if (propertyType == typeof(int))
                    {
                        material.SetInt(pair.Key, (int)Convert.ChangeType(fieldInfo.GetValue(nodeGeo), propertyType));
                    }
                    else if (propertyType == typeof(float))
                    {
                        material.SetFloat(pair.Key, (float)Convert.ChangeType(fieldInfo.GetValue(nodeGeo), propertyType));
                    }
                    else if (propertyType == typeof(Color))
                    {
                        float[] v = (float[])fieldInfo.GetValue(nodeGeo);
                        float a = v.Length > 3 ? v[3] : 1.0f;
                        Color c = new Color(v[0], v[1], v[2], a);
                        string name = Encoding.UTF8.GetString(nodeGeo.name, 0, nodeGeo.name.Length);
                        material.SetColor(pair.Key, c);

                        if (a < 1.0f)
                        {
                            // set rendering mode
                            material.SetFloat("_Mode", 1);
                            material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
                            material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                            material.SetInt("_ZWrite", 0);
                            material.DisableKeyword("_ALPHATEST_ON");
                            material.DisableKeyword("_ALPHABLEND_ON");
                            material.EnableKeyword("_ALPHAPREMULTIPLY_ON");
                            material.renderQueue = 3000;
                        }
                    }
                    else if (propertyType == typeof(Texture))
                    {
                        int id = (int)Convert.ChangeType(fieldInfo.GetValue(nodeGeo), typeof(int));

                        if (id > -1 && id < SceneLoader.SceneTextureList.Count)
                        {
                            Texture2D texRef = SceneLoader.SceneTextureList[nodeGeo.textureId];

                            material.SetTexture(pair.Key, texRef);

                            // set materials render mode to fate to senable alpha blending
                            // TODO these values should be part of the geo node or material package !?
                            if (Textures.hasAlpha(texRef))
                            {
                                // set rendering mode
                                material.SetFloat("_Mode", 1);
                                material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
                                material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                                material.SetInt("_ZWrite", 0);
                                material.DisableKeyword("_ALPHATEST_ON");
                                material.DisableKeyword("_ALPHABLEND_ON");
                                material.EnableKeyword("_ALPHAPREMULTIPLY_ON");
                                material.renderQueue = 3000;
                            }
                        }

                    }
                    else
                    {
                        Debug.LogWarning("Can not map material property " + pair.Key);
                    }


                    // TODO implement the rest
                    // .
                    // .
                    // .
                }

            }*/

        }

        //!
        //! Function that recusively adds bone transforms to renderers of SkinnedMesh objects.
        //!
        //! @param sceneDataHandler A reference to the actual VPET sceneDataHandler.
        //! @param parent the parent Unity transform.
        //! @param idx The index for referencing into the node list.
        //!
        private int createSkinnedRendererIter(ref SceneDataHandler sceneDataHandler, Transform parent, int idx = 0)
        {

            SceneNodeSkinnedGeo node = (SceneNodeSkinnedGeo)sceneDataHandler.NodeList[idx];
            Transform trans = parent.Find(Encoding.ASCII.GetString(node.name));

            SkinnedMeshRenderer renderer = trans.gameObject.GetComponent<SkinnedMeshRenderer>();

            if (renderer)
            {
                renderer.rootBone = trans;
                Transform[] meshBones = new Transform[node.skinnedMeshBoneIDs.Length];
                for (int i = 0; i < node.skinnedMeshBoneIDs.Length; i++)
                {
                    if (node.skinnedMeshBoneIDs[i] != -1)
                        meshBones[i] = gameObjectList[node.skinnedMeshBoneIDs[i]].transform;
                }
                renderer.bones = meshBones;
            }

            // recursive call
            int idxChild = idx;
            for (int k = 1; k <= node.childCount; k++)
            {
                idxChild = createSkinnedRendererIter(ref sceneDataHandler, trans, idxChild + 1);
            }

            return idxChild;
        }

        //!
        //! Function that deletes all Unity scene content and clears the VPET scene object lists.
        //!
        public void ResetScene()
        {
            SceneEditableObjects.Clear();
            SceneMaterialList.Clear();
            SceneTextureList.Clear();
            SceneMeshList.Clear();
            SceneCameraList.Clear();
            SelectableLights.Clear();
            gameObjectList.Clear();

            if (scnRoot != null)
            {
                foreach (Transform child in scnRoot.transform)
                {
                    GameObject.Destroy(child.gameObject);
                }
            }
        }

    }
}
