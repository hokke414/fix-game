using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using MRRobotRepair.Core;
using MRRobotRepair.MR;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEditor.PackageManager.UI;
using UnityEditor.SceneManagement;
using UnityEditor.XR.Management;
using UnityEditor.XR.Management.Metadata;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Locomotion;
using UnityEngine.XR.Management;
using UnityEngine.XR.OpenXR;
using UnityEngine.XR.OpenXR.Features;

namespace MRRobotRepair.Editor
{
    /// <summary>RepairGameシーンと検証用APKを再生成する。</summary>
    public static class RepairGameSceneBuilder
    {
        private const string ScenePath = "Assets/_Project/Scenes/RepairGame.unity";
        private const string MaterialPath = "Assets/_Project/Materials/WorkspaceGuide.mat";
        private const string StarterAssetsRoot = "Assets/Samples/XR Interaction Toolkit/3.6.0/Starter Assets";
        private static readonly HashSet<string> RequiredAndroidFeatureIds = new()
        {
            "com.unity.openxr.feature.metaquest",
            "com.meta.openxr.feature.metaxr",
            "com.meta.openxr.feature.foveation",
            "com.meta.openxr.feature.subsampledLayout",
            "com.unity.openxr.feature.input.oculustouch",
            "com.unity.openxr.feature.input.metaquestplus",
        };

        [MenuItem("MR Robot Repair/Import XRI Starter Assets")]
        public static void ImportXriStarterAssets()
        {
            Sample sample = Sample.FindByPackage("com.unity.xr.interaction.toolkit", "3.6.0")
                .Single(value => value.displayName == "Starter Assets");
            if (!sample.Import(Sample.ImportOptions.HideImportWindow | Sample.ImportOptions.OverridePreviousImports))
                throw new InvalidOperationException("XRI Starter Assets could not be imported.");
            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
        }

        [MenuItem("MR Robot Repair/Build RepairGame Scene")]
        public static void BuildRepairGameScene()
        {
            EnsureFolder("Assets/_Project/Scenes");
            EnsureFolder("Assets/_Project/Materials");
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>($"{StarterAssetsRoot}/Prefabs/XR Origin (XR Rig).prefab");
            if (prefab == null) throw new InvalidOperationException("Import XRI Starter Assets first.");

            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            GameObject xrOrigin = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
            xrOrigin.name = "XR Origin (Controller MR)";
            foreach (LocomotionProvider provider in xrOrigin.GetComponentsInChildren<LocomotionProvider>(true))
                UnityEngine.Object.DestroyImmediate(provider);

            Camera viewer = xrOrigin.GetComponentInChildren<Camera>(true);
            Transform left = FindDescendant(xrOrigin.transform, "Left Controller");
            Transform right = FindDescendant(xrOrigin.transform, "Right Controller");
            if (viewer == null || left == null || right == null) throw new InvalidOperationException("XRI prefab structure changed.");
            viewer.clearFlags = CameraClearFlags.SolidColor;
            viewer.backgroundColor = new Color(0f, 0f, 0f, 0f);
            if (UnityEngine.Object.FindFirstObjectByType<XRInteractionManager>() == null)
                new GameObject("XR Interaction Manager").AddComponent<XRInteractionManager>();

            GameObject workspace = new("Workspace Placement");
            WorkspacePlacementController placement = workspace.AddComponent<WorkspacePlacementController>();
            GameObject guideRoot = new("Semi Transparent Workspace Guide");
            guideRoot.transform.SetParent(workspace.transform, false);
            BoxCollider bounds = guideRoot.AddComponent<BoxCollider>();
            bounds.center = new Vector3(0f, -0.375f, 0f);
            bounds.size = new Vector3(0.8f, 0.75f, 0.5f);
            Renderer[] renderers = CreateGuideFrame(guideRoot.transform, CreateGuideMaterial());
            WorkspaceReachabilityChecker reach = workspace.AddComponent<WorkspaceReachabilityChecker>();
            reach.Configure(bounds, left, right);
            WorkspaceGuideView guide = workspace.AddComponent<WorkspaceGuideView>();
            guide.Configure(placement, guideRoot, renderers);
            guide.SetReachable(false);
            GameObject content = new("Placed Robot And Tools Root");
            content.transform.SetParent(workspace.transform, false);
            content.SetActive(false);
            WorkspacePlacementInputAdapter adapter = workspace.AddComponent<WorkspacePlacementInputAdapter>();
            adapter.Configure(placement, reach, guide, viewer.transform, content);

            EditorSceneManager.SaveScene(scene, ScenePath);
            EditorBuildSettingsScene[] others = EditorBuildSettings.scenes.Where(value => value.path != ScenePath).ToArray();
            EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(ScenePath, true) }.Concat(others).ToArray();
            AssetDatabase.SaveAssets();
        }

        [MenuItem("MR Robot Repair/Configure Android OpenXR")]
        public static void ConfigureAndroidOpenXr()
        {
            XRGeneralSettingsPerBuildTarget settingsStore = GetOrCreateXrSettingsStore();
            if (!settingsStore.HasSettingsForBuildTarget(BuildTargetGroup.Android))
                settingsStore.CreateDefaultSettingsForBuildTarget(BuildTargetGroup.Android);
            if (settingsStore.ManagerSettingsForBuildTarget(BuildTargetGroup.Android) == null)
                settingsStore.CreateDefaultManagerSettingsForBuildTarget(BuildTargetGroup.Android);

            XRGeneralSettings generalSettings = settingsStore.SettingsForBuildTarget(BuildTargetGroup.Android);
            if (generalSettings == null || generalSettings.Manager == null)
                throw new InvalidOperationException("Android XR Management settings could not be created.");
            generalSettings.InitManagerOnStart = true;
            generalSettings.Manager.automaticLoading = true;
            generalSettings.Manager.automaticRunning = true;
            if (!XRPackageMetadataStore.AssignLoader(
                    generalSettings.Manager,
                    "UnityEngine.XR.OpenXR.OpenXRLoader",
                    BuildTargetGroup.Android))
                throw new InvalidOperationException("OpenXR loader could not be assigned for Android.");

            OpenXRSettings openXrSettings = OpenXRSettings.GetSettingsForBuildTargetGroup(BuildTargetGroup.Android);
            if (openXrSettings == null)
                throw new InvalidOperationException("Android OpenXR settings are missing.");
            foreach (OpenXRFeature feature in openXrSettings.GetFeatures())
            {
                var serializedFeature = new SerializedObject(feature);
                SerializedProperty featureId = serializedFeature.FindProperty("featureIdInternal");
                if (featureId != null && RequiredAndroidFeatureIds.Contains(featureId.stringValue))
                {
                    feature.enabled = true;
                    EditorUtility.SetDirty(feature);
                }
            }

            PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64;
            PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel32;
            PlayerSettings.SetScriptingBackend(NamedBuildTarget.Android, ScriptingImplementation.IL2CPP);
            PlayerSettings.SetUseDefaultGraphicsAPIs(BuildTarget.Android, false);
            PlayerSettings.SetGraphicsAPIs(BuildTarget.Android, new[] { GraphicsDeviceType.Vulkan });
            PlayerSettings.Android.applicationEntry = AndroidApplicationEntry.GameActivity;

            OVRProjectConfig projectConfig = OVRProjectConfig.CachedProjectConfig;
            if (projectConfig != null)
            {
                projectConfig.systemLoadingScreenBackground =
                    OVRProjectConfig.SystemLoadingScreenBackground.ContextualPassthrough;
                OVRProjectConfig.CommitProjectConfig(projectConfig);
            }

            EditorUtility.SetDirty(generalSettings);
            EditorUtility.SetDirty(generalSettings.Manager);
            EditorUtility.SetDirty(openXrSettings);
            AssetDatabase.SaveAssets();
        }

        [MenuItem("MR Robot Repair/Build Android Development APK")]
        public static void BuildAndroidDevelopmentApk()
        {
            Directory.CreateDirectory("Builds/Android");
            BuildReport report = BuildPipeline.BuildPlayer(new BuildPlayerOptions
            {
                scenes = new[] { ScenePath },
                locationPathName = "Builds/Android/MRRobotRepair-Development.apk",
                target = BuildTarget.Android,
                options = BuildOptions.Development,
            });
            if (report.summary.result != BuildResult.Succeeded)
                throw new InvalidOperationException($"Android build failed: {report.summary.result}.");
        }

        private static Renderer[] CreateGuideFrame(Transform root, Material material)
        {
            return new[]
            {
                CreatePart(root,"Work Surface",Vector3.zero,new Vector3(0.8f,0.025f,0.5f),material),
                CreatePart(root,"Leg Front Left",new Vector3(-0.375f,-0.375f,-0.225f),new Vector3(0.025f,0.75f,0.025f),material),
                CreatePart(root,"Leg Front Right",new Vector3(0.375f,-0.375f,-0.225f),new Vector3(0.025f,0.75f,0.025f),material),
                CreatePart(root,"Leg Back Left",new Vector3(-0.375f,-0.375f,0.225f),new Vector3(0.025f,0.75f,0.025f),material),
                CreatePart(root,"Leg Back Right",new Vector3(0.375f,-0.375f,0.225f),new Vector3(0.025f,0.75f,0.025f),material),
            };
        }
        private static Renderer CreatePart(Transform parent,string name,Vector3 position,Vector3 scale,Material material)
        {
            GameObject part=GameObject.CreatePrimitive(PrimitiveType.Cube); part.name=name;
            part.transform.SetParent(parent,false); part.transform.localPosition=position; part.transform.localScale=scale;
            UnityEngine.Object.DestroyImmediate(part.GetComponent<Collider>());
            Renderer renderer=part.GetComponent<Renderer>(); renderer.sharedMaterial=material; return renderer;
        }
        private static Material CreateGuideMaterial()
        {
            Material material=AssetDatabase.LoadAssetAtPath<Material>(MaterialPath);
            Shader shader=Shader.Find("Universal Render Pipeline/Unlit");
            if(shader==null) throw new InvalidOperationException("URP Unlit shader missing.");
            if(material==null){material=new Material(shader);AssetDatabase.CreateAsset(material,MaterialPath);}else material.shader=shader;
            material.SetColor("_BaseColor",new Color(1f,0.65f,0.1f,0.3f)); material.SetFloat("_Surface",1f);
            material.SetFloat("_SrcBlend",(float)BlendMode.SrcAlpha); material.SetFloat("_DstBlend",(float)BlendMode.OneMinusSrcAlpha);
            material.SetFloat("_ZWrite",0f); material.EnableKeyword("_SURFACE_TYPE_TRANSPARENT"); material.renderQueue=(int)RenderQueue.Transparent;
            EditorUtility.SetDirty(material); return material;
        }
        private static Transform FindDescendant(Transform root,string name)=>root.GetComponentsInChildren<Transform>(true).FirstOrDefault(value=>value.name==name);
        private static XRGeneralSettingsPerBuildTarget GetOrCreateXrSettingsStore()
        {
            MethodInfo getOrCreate = typeof(XRGeneralSettingsPerBuildTarget).GetMethod(
                "GetOrCreate", BindingFlags.Static | BindingFlags.NonPublic);
            if (getOrCreate == null)
                throw new MissingMethodException(nameof(XRGeneralSettingsPerBuildTarget), "GetOrCreate");
            return (XRGeneralSettingsPerBuildTarget)getOrCreate.Invoke(null, null);
        }
        private static void EnsureFolder(string path)
        {
            string[] segments=path.Split('/'); string current=segments[0];
            for(int index=1;index<segments.Length;index++){string next=$"{current}/{segments[index]}";if(!AssetDatabase.IsValidFolder(next))AssetDatabase.CreateFolder(current,segments[index]);current=next;}
        }
    }
}
