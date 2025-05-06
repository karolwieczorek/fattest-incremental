using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Hypnagogia.Utils;
using UnityEditor;

namespace FattestInc.Builds {
    public static class ItchUploader {
        static Maybe<string> ItchChannelMaybe => ItchButlerPrefsProvider.GetItchChannel("web");

        [MenuItem(ItchConsts.ItchMenuBase + "Build Development")]
        public static void BuildDevBuild() {
            Build(ItchConsts.BuildPathDev, BuildOptions.Development);
        }
        
        [MenuItem(ItchConsts.ItchMenuBase + "Build Development And Run")]
        public static void BuildAndRunDevBuild() {
            BuildDevBuild();
            WebGLPythonServer.LaunchLastWebGLBuild();
        }

        [MenuItem(ItchConsts.ItchMenuBase + "Build Release")]
        public static void BuildRelease() {
            Build(ItchConsts.BuildPathRelease, BuildOptions.None);
        }

        [MenuItem(ItchConsts.ItchMenuBase + "Build Release And Upload")]
        public static void BuildReleaseAndUpload() {
            BuildRelease();
            UploadLastReleaseBuild();
        }

        [MenuItem(ItchConsts.ItchMenuBase + "Build Release (No Compression)")]
        public static void BuildReleaseNoCompression() {
            using (new WebGLCompressionScope(WebGLCompressionFormat.Disabled)) {
                Build(ItchConsts.BuildPathReleaseNoCompression, BuildOptions.None);
            }
        }

        [MenuItem(ItchConsts.ItchMenuBase + "Build Release (No Compression) And Run")]
        public static void BuildAndRunReleaseNoCompression() {
            BuildReleaseNoCompression();
            WebGLPythonServer.LaunchLastWebGLBuild();
        }

        [MenuItem(ItchConsts.ItchMenuBase + "Upload last Release")]
        public static void UploadLastReleaseBuild() {
            if (ItchButlerPrefsProvider.GetButlerExecutable().TryToGetValue(out var butlerPath) == false) {
                UnityEngine.Debug.LogError("Invalid itch.io butler settings.");
                ItchButlerPrefsProvider.OpenItchPrefs();
                return;
            }

            if (ItchChannelMaybe.TryToGetValue(out var itchChannel) == false) {
                UnityEngine.Debug.LogError("Invalid itch.io butler settings.");
                ItchButlerPrefsProvider.OpenItchPrefs();
                return;
            }

            var currentBuildPath = ItchConsts.BuildPathRelease;
            var version = PlayerSettings.bundleVersion; // TODO get version from build path?

            if (VersionExists(butlerPath, itchChannel, version)) {
                bool proceed = EditorUtility.DisplayDialog(
                    "Version Already Exists",
                    $"Version '{version}' already exists on itch.io.\nDo you want to continue and overwrite it?",
                    "Yes, Upload",
                    "Cancel"
                );

                if (!proceed) {
                    UnityEngine.Debug.Log("Upload canceled by user.");
                    return;
                }
            }

            // Step 3: Upload with version tag
            var startInfo = new ProcessStartInfo(butlerPath) {
                Arguments = $"push \"{currentBuildPath}\" {itchChannel} --userversion \"{version}\"",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };

            var process = new Process {StartInfo = startInfo};
            process.OutputDataReceived += (s, e) => UnityEngine.Debug.Log(e.Data);
            process.ErrorDataReceived += (s, e) => UnityEngine.Debug.LogError(e.Data);

            process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();
        }

        static void Build(string currentBuildPath, BuildOptions buildOptions) {
            var scenes = EditorBuildSettings.scenes
                .Where(s => s.enabled)
                .Select(s => s.path)
                .ToArray();

            BuildPipeline.BuildPlayer(scenes, currentBuildPath, BuildTarget.WebGL, buildOptions);
            UnityEngine.Debug.Log($"Update Build Location [{BuildTarget.WebGL}]: {currentBuildPath}");
            EditorUserBuildSettings.SetBuildLocation(BuildTarget.WebGL, currentBuildPath);
        }

        static bool VersionExists(string butlerPath, string itchChannel, string versionToCheck) {
            var processStartInfo = new ProcessStartInfo(butlerPath) {
                Arguments = $"status {itchChannel}",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };

            var process = new Process {StartInfo = processStartInfo};
            process.Start();

            var output = process.StandardOutput.ReadToEnd();
            process.WaitForExit();

            if (output.Contains("nothing on this channel yet")) {
                UnityEngine.Debug.Log("No previous versions found.");
                return false;
            }

            // Search for the VERSION column row
            using (StringReader reader = new StringReader(output)) {
                string line;
                while ((line = reader.ReadLine()) != null) {
                    // Match lines that contain a version string
                    if (line.Contains("|") && line.Contains(versionToCheck)) {
                        // Split by '|' and trim each column
                        var parts = line.Split('|');
                        if (parts.Length >= 4) {
                            string version = parts[3].Trim();
                            if (version == versionToCheck) {
                                UnityEngine.Debug.LogWarning($"⚠Version {version} already exists on itch.io.");
                                return true;
                            }
                        }
                    }
                }
            }

            // var regex = new Regex(@"\s+\d+\s+\d{4}-\d{2}-\d{2}.*?\s+([^\s]+)");
            // var matches = regex.Matches(output);
            //
            // foreach (Match match in matches) {
            //     string foundVersion = match.Groups[1].Value.Trim();
            //     if (foundVersion == versionToCheck) {
            //         return true;
            //     }
            // }

            return false;
        }

        [MenuItem(ItchConsts.ItchMenuBase + "List Versions")]
        public static void ListButlerVersions() {
            if (ItchButlerPrefsProvider.GetButlerExecutable().TryToGetValue(out var butlerPath) == false) {
                UnityEngine.Debug.LogError("Invalid itch.io butler settings.");
                ItchButlerPrefsProvider.OpenItchPrefs();
                return;
            }

            if (ItchChannelMaybe.TryToGetValue(out var itchChannel) == false) {
                UnityEngine.Debug.LogError("Invalid itch.io butler settings.");
                ItchButlerPrefsProvider.OpenItchPrefs();
                return;
            }

            var psi = new ProcessStartInfo(butlerPath) {
                Arguments = $"status {itchChannel}",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };

            var proc = new Process {StartInfo = psi};
            proc.Start();

            var output = proc.StandardOutput.ReadToEnd();
            var error = proc.StandardError.ReadToEnd();
            proc.WaitForExit();

            if (!string.IsNullOrEmpty(error)) {
                UnityEngine.Debug.LogError("Butler error:\n" + error);
                return;
            }

            UnityEngine.Debug.Log("Butler status output:\n" + output);

            var parsedVersions = new List<string>();
            using (var reader = new StringReader(output)) {
                string line;

                while ((line = reader.ReadLine()) != null) {
                    if (line.StartsWith("|")) {
                        var parts = line.Split('|');
                        if (parts.Length >= 5) {
                            string channel = parts[1].Trim();
                            string upload = parts[2].Trim().TrimStart('#');
                            string build = parts[3].Trim().Replace("√", "").TrimStart('#');
                            string version = parts[4].Trim();

                            parsedVersions.Add(
                                $"🆔 {upload} | 📦 Build: {build} | 📁 Channel: {channel} | 🔖 Version: {version}");
                        }
                    }
                }
            }

            // TODO first element is header, probably should also throw this log
            if (parsedVersions.Count == 0) {
                UnityEngine.Debug.Log("ℹ️ No versions found or output could not be parsed.");
            }
            else {
                foreach (var v in parsedVersions)
                    UnityEngine.Debug.Log(v);
            }
        }
    }
}