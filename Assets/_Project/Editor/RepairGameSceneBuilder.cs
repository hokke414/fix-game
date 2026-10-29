using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using MRRobotRepair.Core;
using MRRobotRepair.Gameplay;
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
using UnityEngine.XR.Interaction.Toolkit.Interactables;
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
        private const string FontPath = "Assets/_Project/Fonts/NotoSansJP-VF.ttf";
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

            Font font = AssetDatabase.LoadAssetAtPath<Font>(FontPath);
            if (font == null) throw new InvalidOperationException($"Japanese font is missing: {FontPath}");

            Material dark = GetOrCreateOpaqueMaterial("MvpDark", new Color(0.04f, 0.10f, 0.16f));
            Material body = GetOrCreateOpaqueMaterial("RobotBody", new Color(0.72f, 0.78f, 0.82f));
            Material red = GetOrCreateOpaqueMaterial("ToolRed", new Color(0.9f, 0.12f, 0.1f));
            Material yellow = GetOrCreateOpaqueMaterial("ToolYellow", new Color(1f, 0.82f, 0.08f));
            Material orange = GetOrCreateOpaqueMaterial("ToolOrange", new Color(1f, 0.4f, 0.05f));
            Material cyan = GetOrCreateOpaqueMaterial("ToolCyan", new Color(0.1f, 0.8f, 0.85f));
            Material green = GetOrCreateOpaqueMaterial("ToolGreen", new Color(0.15f, 0.75f, 0.3f));
            Material white = GetOrCreateOpaqueMaterial("ToolWhite", new Color(0.9f, 0.92f, 0.94f));
            Material metal = GetOrCreateOpaqueMaterial("ToolMetal", new Color(0.55f, 0.62f, 0.68f));

            CreateVisual(workspace.transform, "Instruction Board", PrimitiveType.Cube,
                new Vector3(0f, 0.62f, 0.2f), new Vector3(0.82f, 0.36f, 0.02f), Vector3.zero, dark, false);
            TextMesh screen = CreateText(workspace.transform, "Instruction And Status Text", font,
                new Vector3(0f, 0.63f, 0.186f), 0.0135f, Color.white,
                "左スティック: 移動 / 右スティック: 高さ・回転\n手を作業台へ近づけ、AまたはXで確定\n\n周囲の安全を確認してください");
            screen.lineSpacing = 0.9f;

            CreateVisual(content.transform, "Solid Work Surface", PrimitiveType.Cube,
                new Vector3(0f, -0.015f, 0f), new Vector3(0.8f, 0.03f, 0.5f), Vector3.zero, dark, true);
            CreateVisual(content.transform, "Robot Body", PrimitiveType.Cube,
                new Vector3(0f, 0.14f, 0.06f), new Vector3(0.36f, 0.25f, 0.22f), Vector3.zero, body, false);
            CreateVisual(content.transform, "Robot Head", PrimitiveType.Cube,
                new Vector3(0f, 0.31f, 0.06f), new Vector3(0.18f, 0.11f, 0.13f), Vector3.zero, body, false);
            CreateVisual(content.transform, "Dark Front Control Panel", PrimitiveType.Cube,
                new Vector3(0f, 0.145f, -0.061f), new Vector3(0.34f, 0.2f, 0.015f), Vector3.zero, dark, false);
            CreateVisual(content.transform, "Left Eye", PrimitiveType.Sphere,
                new Vector3(-0.045f, 0.32f, -0.01f), Vector3.one * 0.025f, Vector3.zero, dark, false);
            CreateVisual(content.transform, "Right Eye", PrimitiveType.Sphere,
                new Vector3(0.045f, 0.32f, -0.01f), Vector3.one * 0.025f, Vector3.zero, dark, false);

            RepairGameMvpController repairGame = workspace.AddComponent<RepairGameMvpController>();
            RepairSelectStep powerSwitch = CreateSelectStep(content.transform, repairGame, 0, "Power Switch",
                PrimitiveType.Cylinder, new Vector3(-0.14f, 0.21f, -0.078f), new Vector3(0.045f, 0.018f, 0.045f),
                new Vector3(90f, 0f, 0f), red);
            AddToolPart(powerSwitch.transform, "Power Icon White Disc", PrimitiveType.Cylinder,
                new Vector3(0f, -0.019f, 0f), new Vector3(0.028f, 0.003f, 0.028f), Vector3.zero, white);
            AddToolPart(powerSwitch.transform, "Power Icon Red Center", PrimitiveType.Cylinder,
                new Vector3(0f, -0.023f, 0f), new Vector3(0.02f, 0.003f, 0.02f), Vector3.zero, red);
            AddToolPart(powerSwitch.transform, "Power Icon Line", PrimitiveType.Cube,
                new Vector3(0f, -0.027f, 0.021f), new Vector3(0.007f, 0.004f, 0.028f), Vector3.zero, white);
            RepairTarget wireDiagnostic = CreateTarget(content.transform, repairGame, 1, "tool-wire-tester", "Wire Diagnostic",
                PrimitiveType.Sphere, new Vector3(-0.12f, 0.14f, -0.078f), new Vector3(0.045f, 0.045f, 0.025f), Vector3.zero, cyan);
            RepairTarget wireReplacement = CreateTarget(content.transform, repairGame, 2, "part-cable", "Cable Socket",
                PrimitiveType.Cube, new Vector3(-0.12f, 0.08f, -0.078f), new Vector3(0.09f, 0.032f, 0.025f), Vector3.zero, cyan);
            AddToolPart(wireReplacement.transform, "Cable Socket Left Pin", PrimitiveType.Cube,
                new Vector3(-0.027f, 0f, -0.016f), new Vector3(0.012f, 0.014f, 0.008f), Vector3.zero, dark);
            AddToolPart(wireReplacement.transform, "Cable Socket Right Pin", PrimitiveType.Cube,
                new Vector3(0.027f, 0f, -0.016f), new Vector3(0.012f, 0.014f, 0.008f), Vector3.zero, dark);
            RepairTarget fuseDiagnostic = CreateTarget(content.transform, repairGame, 3, "tool-fuse-tester", "Fuse Diagnostic",
                PrimitiveType.Cylinder, new Vector3(0f, 0.14f, -0.078f), new Vector3(0.04f, 0.018f, 0.04f), new Vector3(90f, 0f, 0f), orange);
            AddToolPart(fuseDiagnostic.transform, "Fuse Test Center", PrimitiveType.Cylinder,
                new Vector3(0f, -0.019f, 0f), new Vector3(0.018f, 0.003f, 0.018f), Vector3.zero, white);
            RepairTarget fuseReplacement = CreateTarget(content.transform, repairGame, 4, "part-fuse", "Fuse Slot",
                PrimitiveType.Cylinder, new Vector3(0f, 0.08f, -0.078f), new Vector3(0.02f, 0.05f, 0.02f), new Vector3(0f, 0f, 90f), white);
            AddToolPart(fuseReplacement.transform, "Fuse Slot Left Cap", PrimitiveType.Cylinder,
                new Vector3(0f, -0.04f, 0f), new Vector3(0.025f, 0.012f, 0.025f), Vector3.zero, metal);
            AddToolPart(fuseReplacement.transform, "Fuse Slot Right Cap", PrimitiveType.Cylinder,
                new Vector3(0f, 0.04f, 0f), new Vector3(0.025f, 0.012f, 0.025f), Vector3.zero, metal);
            RepairTarget boardOpen = CreateTarget(content.transform, repairGame, 5, "tool-screwdriver", "Board Screws",
                PrimitiveType.Cylinder, new Vector3(0.12f, 0.14f, -0.078f), new Vector3(0.038f, 0.014f, 0.038f), new Vector3(90f, 0f, 0f), metal);
            AddToolPart(boardOpen.transform, "Screw Slot Horizontal", PrimitiveType.Cube,
                new Vector3(0f, -0.016f, 0f), new Vector3(0.045f, 0.005f, 0.008f), Vector3.zero, dark);
            AddToolPart(boardOpen.transform, "Screw Slot Vertical", PrimitiveType.Cube,
                new Vector3(0f, -0.017f, 0f), new Vector3(0.008f, 0.005f, 0.045f), Vector3.zero, dark);
            RepairTarget boardReplacement = CreateTarget(content.transform, repairGame, 6, "part-board", "Board Slot",
                PrimitiveType.Cube, new Vector3(0.12f, 0.08f, -0.078f), new Vector3(0.1f, 0.055f, 0.025f), Vector3.zero, green);
            AddToolPart(boardReplacement.transform, "Board Slot Chip", PrimitiveType.Cube,
                new Vector3(0f, 0f, -0.017f), new Vector3(0.035f, 0.026f, 0.008f), Vector3.zero, dark);
            RepairSelectStep finalTest = CreateSelectStep(content.transform, repairGame, 7, "Final Test Switch",
                PrimitiveType.Cube, new Vector3(0.14f, 0.21f, -0.078f), new Vector3(0.075f, 0.04f, 0.025f), Vector3.zero, green);
            AddToolPart(finalTest.transform, "Final Test Lever", PrimitiveType.Cube,
                new Vector3(0f, 0.025f, -0.02f), new Vector3(0.02f, 0.055f, 0.02f), new Vector3(0f, 0f, -20f), metal);

            RepairTool wireTester = CreateTool(content.transform, "tool-wire-tester", "Yellow Continuity Tester",
                PrimitiveType.Cube, new Vector3(-0.31f, 0.055f, -0.18f), new Vector3(0.065f, 0.08f, 0.045f),
                Vector3.zero, new Vector3(0.09f, 0.14f, 0.065f), yellow);
            AddToolPart(wireTester.transform, "Display", PrimitiveType.Cube,
                new Vector3(0f, 0.005f, -0.026f), new Vector3(0.04f, 0.025f, 0.008f), Vector3.zero, dark);
            AddToolPart(wireTester.transform, "Left Probe", PrimitiveType.Cylinder,
                new Vector3(-0.022f, 0.065f, 0f), new Vector3(0.006f, 0.035f, 0.006f), new Vector3(0f, 0f, -12f), metal);
            AddToolPart(wireTester.transform, "Right Probe", PrimitiveType.Cylinder,
                new Vector3(0.022f, 0.065f, 0f), new Vector3(0.006f, 0.035f, 0.006f), new Vector3(0f, 0f, 12f), metal);

            RepairTool cable = CreateTool(content.transform, "part-cable", "Cyan Replacement Cable",
                PrimitiveType.Cylinder, new Vector3(-0.19f, 0.04f, -0.18f), new Vector3(0.018f, 0.065f, 0.018f),
                new Vector3(0f, 0f, 90f), new Vector3(0.18f, 0.055f, 0.055f), cyan);
            AddToolPart(cable.transform, "Left Connector", PrimitiveType.Cube,
                new Vector3(-0.075f, 0f, 0f), new Vector3(0.032f, 0.04f, 0.04f), Vector3.zero, metal);
            AddToolPart(cable.transform, "Right Connector", PrimitiveType.Cube,
                new Vector3(0.075f, 0f, 0f), new Vector3(0.032f, 0.04f, 0.04f), Vector3.zero, metal);

            RepairTool fuseTester = CreateTool(content.transform, "tool-fuse-tester", "Orange Fuse Tester",
                PrimitiveType.Cube, new Vector3(-0.065f, 0.055f, -0.18f), new Vector3(0.065f, 0.08f, 0.045f),
                Vector3.zero, new Vector3(0.09f, 0.14f, 0.065f), orange);
            AddToolPart(fuseTester.transform, "Fuse Window", PrimitiveType.Cube,
                new Vector3(0f, 0.01f, -0.026f), new Vector3(0.035f, 0.035f, 0.008f), Vector3.zero, white);
            AddToolPart(fuseTester.transform, "Fork Left", PrimitiveType.Cylinder,
                new Vector3(-0.018f, 0.065f, 0f), new Vector3(0.005f, 0.03f, 0.005f), Vector3.zero, metal);
            AddToolPart(fuseTester.transform, "Fork Right", PrimitiveType.Cylinder,
                new Vector3(0.018f, 0.065f, 0f), new Vector3(0.005f, 0.03f, 0.005f), Vector3.zero, metal);

            RepairTool fuse = CreateTool(content.transform, "part-fuse", "White Replacement Fuse",
                PrimitiveType.Cylinder, new Vector3(0.07f, 0.035f, -0.18f), new Vector3(0.016f, 0.05f, 0.016f),
                new Vector3(0f, 0f, 90f), new Vector3(0.13f, 0.045f, 0.045f), white);
            AddToolPart(fuse.transform, "Left Metal Cap", PrimitiveType.Cylinder,
                new Vector3(-0.058f, 0f, 0f), new Vector3(0.02f, 0.014f, 0.02f), new Vector3(0f, 0f, 90f), metal);
            AddToolPart(fuse.transform, "Right Metal Cap", PrimitiveType.Cylinder,
                new Vector3(0.058f, 0f, 0f), new Vector3(0.02f, 0.014f, 0.02f), new Vector3(0f, 0f, 90f), metal);

            RepairTool screwdriver = CreateTool(content.transform, "tool-screwdriver", "Red Screwdriver",
                PrimitiveType.Cylinder, new Vector3(0.2f, 0.035f, -0.18f), new Vector3(0.03f, 0.05f, 0.03f),
                new Vector3(0f, 0f, 90f), new Vector3(0.24f, 0.065f, 0.065f), red);
            AddToolPart(screwdriver.transform, "Metal Shaft", PrimitiveType.Cylinder,
                new Vector3(0.1f, 0f, 0f), new Vector3(0.009f, 0.055f, 0.009f), new Vector3(0f, 0f, 90f), metal);
            AddToolPart(screwdriver.transform, "Driver Tip", PrimitiveType.Cube,
                new Vector3(0.16f, 0f, 0f), new Vector3(0.025f, 0.008f, 0.018f), Vector3.zero, dark);

            RepairTool boardPart = CreateTool(content.transform, "part-board", "Green Replacement Board",
                PrimitiveType.Cube, new Vector3(0.33f, 0.045f, -0.18f), new Vector3(0.1f, 0.018f, 0.075f),
                Vector3.zero, new Vector3(0.14f, 0.05f, 0.11f), green);
            AddToolPart(boardPart.transform, "Main Chip", PrimitiveType.Cube,
                new Vector3(0f, 0.014f, 0f), new Vector3(0.038f, 0.012f, 0.035f), Vector3.zero, dark);
            AddToolPart(boardPart.transform, "Left Chip", PrimitiveType.Cube,
                new Vector3(-0.035f, 0.014f, 0.025f), new Vector3(0.018f, 0.012f, 0.018f), Vector3.zero, metal);
            AddToolPart(boardPart.transform, "Right Chip", PrimitiveType.Cube,
                new Vector3(0.04f, 0.014f, -0.02f), new Vector3(0.018f, 0.012f, 0.018f), Vector3.zero, metal);

            RepairTool[] tools = { wireTester, cable, fuseTester, fuse, screwdriver, boardPart };

            string[] instructions =
            {
                "赤い丸型の電源ボタンを掴んでOFFにする",
                "黄色の導通チェッカーを左上の水色丸端子へ当てる",
                "水色の交換ケーブルを左下の横長ソケットへ当てる",
                "橙のヒューズテスターを中央上の丸端子へ当てる",
                "白い交換ヒューズを中央下の横長スロットへ当てる",
                "赤いドライバーを右上の銀色ネジへ当てる",
                "緑の交換基板を右下の基板スロットへ当てる",
                "右上の緑レバーを掴んで最終テストする",
            };
            string[] hints =
            {
                "電源記号の付いた赤い丸ボタンです",
                "表示窓と2本のプローブがある黄色い工具です",
                "両端にコネクターがある水色のケーブルです",
                "白い窓と2本の端子がある橙色の工具です",
                "両端が銀色の白い円筒部品です",
                "赤い柄と銀色の軸がある工具です",
                "黒いチップが載った緑の板です",
                "銀色の持ち手が付いた緑レバーです",
            };
            repairGame.Configure(
                placement,
                screen,
                new[] { wireDiagnostic, wireReplacement, fuseDiagnostic, fuseReplacement, boardOpen, boardReplacement },
                new[] { powerSwitch, finalTest },
                tools,
                instructions,
                hints);

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
            PlayerSettings.companyName = "hokke414";
            PlayerSettings.productName = "MR Robot Repair";
            PlayerSettings.SetApplicationIdentifier(NamedBuildTarget.Android, "com.hokke414.mrrobotrepair");
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
            ConfigureAndroidOpenXr();
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

        private static GameObject CreateVisual(
            Transform parent,
            string name,
            PrimitiveType primitive,
            Vector3 position,
            Vector3 scale,
            Vector3 eulerAngles,
            Material material,
            bool keepCollider)
        {
            GameObject visual = GameObject.CreatePrimitive(primitive);
            visual.name = name;
            visual.transform.SetParent(parent, false);
            visual.transform.localPosition = position;
            visual.transform.localEulerAngles = eulerAngles;
            visual.transform.localScale = scale;
            visual.GetComponent<Renderer>().sharedMaterial = material;
            if (!keepCollider) UnityEngine.Object.DestroyImmediate(visual.GetComponent<Collider>());
            return visual;
        }

        private static TextMesh CreateText(
            Transform parent,
            string name,
            Font font,
            Vector3 position,
            float characterSize,
            Color color,
            string value)
        {
            GameObject textObject = new(name);
            textObject.transform.SetParent(parent, false);
            textObject.transform.localPosition = position;
            TextMesh text = textObject.AddComponent<TextMesh>();
            text.font = font;
            text.fontSize = 64;
            text.characterSize = characterSize;
            text.anchor = TextAnchor.MiddleCenter;
            text.alignment = TextAlignment.Center;
            text.color = color;
            text.text = value;
            textObject.GetComponent<MeshRenderer>().sharedMaterial = font.material;
            return text;
        }

        private static RepairTool CreateTool(
            Transform parent,
            string toolId,
            string name,
            PrimitiveType primitive,
            Vector3 position,
            Vector3 scale,
            Vector3 eulerAngles,
            Vector3 colliderSize,
            Material material)
        {
            GameObject tool = new(name);
            tool.transform.SetParent(parent, false);
            tool.transform.localPosition = position;
            AddToolPart(tool.transform, "Body", primitive, Vector3.zero, scale, eulerAngles, material);
            BoxCollider collider = tool.AddComponent<BoxCollider>();
            collider.size = colliderSize;
            Rigidbody rigidbody = tool.AddComponent<Rigidbody>();
            rigidbody.useGravity = false;
            rigidbody.linearDamping = 8f;
            rigidbody.angularDamping = 8f;
            rigidbody.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;
            XRGrabInteractable grab = tool.AddComponent<XRGrabInteractable>();
            grab.movementType = XRBaseInteractable.MovementType.Kinematic;
            grab.throwOnDetach = false;
            RepairTool repairTool = tool.AddComponent<RepairTool>();
            repairTool.Configure(toolId);
            return repairTool;
        }

        private static void AddToolPart(
            Transform parent,
            string name,
            PrimitiveType primitive,
            Vector3 localPosition,
            Vector3 localScale,
            Vector3 localEulerAngles,
            Material material)
        {
            CreateVisual(parent, name, primitive, localPosition, localScale, localEulerAngles, material, false);
        }

        private static RepairTarget CreateTarget(
            Transform parent,
            RepairGameMvpController controller,
            int stepIndex,
            string toolId,
            string name,
            PrimitiveType primitive,
            Vector3 position,
            Vector3 scale,
            Vector3 eulerAngles,
            Material material)
        {
            GameObject target = CreateVisual(parent, name, primitive, position, scale, eulerAngles, material, true);
            target.GetComponent<Collider>().isTrigger = true;
            RepairTarget repairTarget = target.AddComponent<RepairTarget>();
            repairTarget.Configure(controller, stepIndex, toolId, target.GetComponent<Renderer>());
            return repairTarget;
        }

        private static RepairSelectStep CreateSelectStep(
            Transform parent,
            RepairGameMvpController controller,
            int stepIndex,
            string name,
            PrimitiveType primitive,
            Vector3 position,
            Vector3 scale,
            Vector3 eulerAngles,
            Material material)
        {
            GameObject selectable = CreateVisual(parent, name, primitive, position, scale,
                eulerAngles, material, true);
            XRSimpleInteractable interactable = selectable.AddComponent<XRSimpleInteractable>();
            RepairSelectStep step = selectable.AddComponent<RepairSelectStep>();
            step.Configure(controller, stepIndex, interactable, selectable.GetComponent<Renderer>());
            return step;
        }

        private static Material GetOrCreateOpaqueMaterial(string fileName, Color color)
        {
            string path = $"Assets/_Project/Materials/{fileName}.mat";
            Material material = AssetDatabase.LoadAssetAtPath<Material>(path);
            Shader shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null) throw new InvalidOperationException("URP Lit shader missing.");
            if (material == null)
            {
                material = new Material(shader);
                AssetDatabase.CreateAsset(material, path);
            }
            else
            {
                material.shader = shader;
            }
            material.color = color;
            if (material.HasProperty("_BaseColor")) material.SetColor("_BaseColor", color);
            EditorUtility.SetDirty(material);
            return material;
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
