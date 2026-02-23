using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace RIGPR.Editor {
    /// <summary>
    /// This script acts as the functionality behind the CI/CD builds, it manages the versioning of the build
    /// and the actual call to unity to build it.
    /// </summary>
    public static class BuildManager {
        /// <summary>
        /// Since we want to use Semantic Versioning, we want to create the reg- 🤢... sorry I mean- Regex 🤮
        /// To ensure that stuff is the correct format, in this case: v1.0.0-hotfix.1 etc
        /// </summary>
        private static readonly Regex _semverRegex = 
            new(@"^(?<major>\d+)\.(?<minor>\d+)\.(?<patch>\d+)(?:-hotfix\.(?<hotfix>\d+))?", RegexOptions.Compiled);
        
        /// <summary>
        /// The list of all the scenes to include within the build
        /// </summary>
        private static readonly List<string> _scenes = new() {
            "Assets/Scenes/Production/MainMenu.unity",
            "Assets/Scenes/Production/MainLevel.unity"
        };

        /// <summary>
        /// Bump the major version component (e.g. 1.X.X -> 2.X.X)
        /// </summary>
        [MenuItem("Build/Bump Major Version")]
        public static void BumpMajor() {
            BumpVersionComponent("major");
        }

        /// <summary>
        /// Bump the minor version component (e.g. X.1.X -> X.2.X)
        /// </summary>
        [MenuItem("Build/Bump Minor Version")]
        public static void BumpMinor() {
            BumpVersionComponent("minor");
        }

        /// <summary>
        /// Bump the patch version component (e.g. X.X.0 -> X.X.1)
        /// </summary>
        [MenuItem("Build/Bump Patch Version")]
        public static void BumpPatch() {
            BumpVersionComponent("patch");
        }

        /// <summary>
        /// Bump the hotfix version (e.g. X.X.X -> X.X.X-hotfix.1 -> X.X.X-hotfix.2)
        /// </summary>
        [MenuItem("Build/Bump Hotfix Version")]
        public static void BumpHotfix() {
            BumpVersionComponent("hotfix");
        }
        
        /// <summary>
        /// Shows the current buildVersion value
        /// </summary>
        [MenuItem("Build/Show Current Version", false, 0)]
        public static void ShowCurrentVersion() {
            Debug.Log($"Current bundleVersion: {PlayerSettings.bundleVersion}");
            EditorUtility.DisplayDialog("Current Version", $"Current bundleVersion:\n\n{PlayerSettings.bundleVersion}",
                "OK");
        }
        
        /// <summary>
        /// Adds the currently open scene to the builds included scenes list
        /// </summary>
        [MenuItem("Build/Add Current Scene To Build Path")]
        public static void AddCurrentSceneToBuildPath() {
            string currScenePath = SceneManager.GetActiveScene().path;

            if (!_scenes.Contains(currScenePath)) {
                _scenes.Add(currScenePath);
                Debug.Log($"Added scene: {currScenePath}");
            }
            else {
                EditorUtility.DisplayDialog("Error",
                    $"Current scene: {currScenePath} is already in the scenes list!", "OK");
            }

            Debug.Log($"Current scene: {currScenePath}");
        }
        
        /// <summary>
        /// Bump the specified component up one value
        /// </summary>
        /// <param name="comp">Version component to bump</param>
        private static void BumpVersionComponent(string comp) {
            string ver = PlayerSettings.bundleVersion;
            Match match = _semverRegex.Match(ver);

            if (!match.Success) {
                Debug.LogError($"Invalid versioning format: {ver}");
                return;
            }

            int major = int.Parse(match.Groups["major"].Value);
            int minor = int.Parse(match.Groups["minor"].Value);
            int patch = int.Parse(match.Groups["patch"].Value);
            int hotfix = match.Groups["hotfix"].Success ? int.Parse(match.Groups["hotfix"].Value) : 0;

            // When we bump a component, all the smaller components reset to their default value
            switch (comp) {
                case "major":
                    major++;
                    minor = 0;
                    patch = 0;
                    hotfix = 0;
                    break;
                case "minor":
                    minor++;
                    patch = 0;
                    hotfix = 0;
                    break;
                case "patch":
                    patch++;
                    hotfix = 0;
                    break;
                case "hotfix":
                    hotfix++;
                    break;
            }

            string newVer = $"{major}.{minor}.{patch}";
            if (hotfix > 0) newVer += $"-hotfix.{hotfix}";

            PlayerSettings.bundleVersion = newVer;
            Debug.Log($"Updated base version to: {newVer}");
        }

        /// <summary>
        /// Clear the hotfix tag from the build version, useful if you are no longer working on a hotfix version
        /// </summary>
        [MenuItem("Build/Clear Hotfix Tag")]
        public static void ClearHotfixTag() {
            string ver = PlayerSettings.bundleVersion;
            Match match = _semverRegex.Match(ver);

            if (!match.Success) {
                Debug.LogError($"Invalid versioning format: {ver}");
                return;
            }

            int major = int.Parse(match.Groups["major"].Value);
            int minor = int.Parse(match.Groups["minor"].Value);
            int patch = int.Parse(match.Groups["patch"].Value);

            string newVer = $"{major}.{minor}.{patch}";

            PlayerSettings.bundleVersion = newVer;
            Debug.Log($"Updated base version to: {newVer}");
        }

        /// <summary>
        /// This function builds the project
        /// It takes in various args to determine the version and targeted platform
        /// </summary>
        public static void BuildProject() {
            string[] args = Environment.GetCommandLineArgs();
            string version = PlayerSettings.bundleVersion;
            BuildTarget target = BuildTarget.StandaloneWindows64;
            string outputPath = "Builds/Windows/Rebellion.exe";

            for (int i = 0; i < args.Length; i++) {
                if (args[i] == "-buildVersion" && i + 1 < args.Length)
                    version = args[i + 1];
                if (args[i] == "-buildTarget" && i + 1 < args.Length) {
                    string t = args[i + 1].ToLower();

                    if (t == "standalonewindows" || t == "standalonewindows64" || t == "win" || t == "win64" ||
                        t == "windows")
                        target = BuildTarget.StandaloneWindows64;
                    else if (t == "linux" || t == "linux64" || t == "standalonelinux64")
                        target = BuildTarget.StandaloneLinux64;
                    else if (t == "osx" || t == "mac" || t == "standaloneosx" || t == "osxuniversal")
                        target = BuildTarget.StandaloneOSX;
                    else
                        Debug.LogError($"Unknown build target: {t}. Defaulting to Linux64.");
                }

                if (args[i] == "-outputPath" && i + 1 < args.Length)
                    outputPath = args[i + 1];
            }

            string platformSuffix = target switch {
                BuildTarget.StandaloneWindows64 => "Windows",
                BuildTarget.StandaloneLinux64 => "Linux",
                BuildTarget.StandaloneOSX => "MacOS",
                _ => "Unknown"
            };

            string platformVersion = version;
            if (!platformVersion.Contains(platformSuffix))
                platformVersion += $".{platformSuffix}";
            PlayerSettings.bundleVersion = platformVersion;

            Debug.Log($"Building version: {platformVersion}");

            ScriptingImplementation backend = ScriptingImplementation.Mono2x;

            for (int i = 0; i < args.Length; i++)
                if (args[i] == "-scriptingBackend" && i + 1 < args.Length) {
                    string b = args[i + 1].ToLower();

                    if (b == "mono")
                        backend = ScriptingImplementation.Mono2x;
                    else if (b == "il2cpp")
                        backend = ScriptingImplementation.IL2CPP;
                    else
                        Debug.LogError($"Unknown script backend: {b}. Defaulting to IL2CPP.");
                }

            if (target == BuildTarget.StandaloneWindows64 ||
                target == BuildTarget.StandaloneLinux64 ||
                target == BuildTarget.StandaloneOSX) {
                PlayerSettings.SetScriptingBackend(BuildTargetGroup.Standalone, backend);
                PlayerSettings.SetArchitecture(BuildTargetGroup.Standalone, 1); // 1 = x86_64
                Debug.Log("Using IL2CPP backend for this build.");
            }

            var buildPlayerOptions = new BuildPlayerOptions {
                scenes = _scenes.ToArray(),
                locationPathName = outputPath,
                target = target,
                options = BuildOptions.None
            };

            BuildReport report = BuildPipeline.BuildPlayer(buildPlayerOptions);
            if (report.summary.result != BuildResult.Succeeded) {
                Debug.LogError($"Build failed: {report.summary.result}");
                EditorApplication.Exit(1);
            }
            else {
                Debug.Log("Build succeeded");
                EditorApplication.Exit(0);
            }
        }
    }
}