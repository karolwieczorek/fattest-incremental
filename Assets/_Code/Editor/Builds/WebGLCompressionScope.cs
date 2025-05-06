using System;
using UnityEditor;
using UnityEngine;

namespace FattestInc.Builds {
    public class WebGLCompressionScope : IDisposable {
        readonly WebGLCompressionFormat originalFormat;

        public WebGLCompressionScope(WebGLCompressionFormat temporaryFormat) {
            originalFormat = PlayerSettings.WebGL.compressionFormat;
            PlayerSettings.WebGL.compressionFormat = temporaryFormat;
            Debug.Log($"🌐 WebGL compression temporarily set to: {temporaryFormat}");
        }

        public void Dispose() {
            PlayerSettings.WebGL.compressionFormat = originalFormat;
            Debug.Log($"🔁 WebGL compression reverted to: {originalFormat}");
        }
    }
}