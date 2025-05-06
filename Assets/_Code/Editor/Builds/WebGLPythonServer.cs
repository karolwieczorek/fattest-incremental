using System;
using System.Diagnostics;
using System.IO;
using UnityEditor;
using Debug = UnityEngine.Debug;

namespace FattestInc.Builds {
    [InitializeOnLoad]
    public class WebGLPythonServer {
        static Process serverProcess = null;

        static WebGLPythonServer() {
            EditorApplication.quitting += StopServer;
            AssemblyReloadEvents.beforeAssemblyReload += StopServer;
        }

        [MenuItem("Tools/WebGL/Launch Last build (Python Server)")]
        public static void LaunchLastWebGLBuild() {
            string buildPath = EditorUserBuildSettings.GetBuildLocation(EditorUserBuildSettings.activeBuildTarget);

            if (string.IsNullOrEmpty(buildPath) || !Directory.Exists(buildPath)) {
                Debug.LogError("WebGL build folder not found. Build your project first.");
                return;
            }

            if (!File.Exists(Path.Combine(buildPath, "index.html"))) {
                Debug.LogError("WebGL index.html not found in build folder.");
                return;
            }

            StopServer();

            try {
                serverProcess = new Process {
                    StartInfo = new ProcessStartInfo {
                        FileName = "python",
                        Arguments = "-m http.server 8080",
                        WorkingDirectory = buildPath,
                        UseShellExecute = false,
                        CreateNoWindow = true, // set to true to hide terminal
                        RedirectStandardOutput = true,
                        RedirectStandardError = true
                    }
                };

                serverProcess.OutputDataReceived += (s, e) => {
                    if (!string.IsNullOrEmpty(e.Data))
                        Debug.Log("[PythonServer] " + e.Data);
                };
                serverProcess.ErrorDataReceived += (s, e) => {
                    if (!string.IsNullOrEmpty(e.Data))
                        Debug.LogError("[PythonServer ERROR] " + e.Data);
                };

                serverProcess.Start();
                serverProcess.BeginOutputReadLine();
                serverProcess.BeginErrorReadLine();

                Debug.Log($"Python server started at http://localhost:8080 buildPath: {buildPath}");

                // Open in browser
                Process.Start(new ProcessStartInfo {FileName = "http://localhost:8080", UseShellExecute = true});
            }
            catch (Exception ex) {
                Debug.LogError("Failed to start Python server: " + ex.Message);
            }
        }

        [MenuItem("Tools/WebGL/Stop Server")]
        public static void StopServer() {
            try {
                if (serverProcess != null && !serverProcess.HasExited) {
                    serverProcess.Kill();
                    serverProcess.Dispose();
                    serverProcess = null;
                    Debug.Log("Python WebGL server stopped.");
                }
            }
            catch (Exception e) {
                Debug.LogWarning("Error while stopping server: " + e.Message);
            }
        }
    }
}