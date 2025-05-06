using Hypnagogia.Utils;
using UnityEditor;

namespace FattestInc.Builds {
    internal static class ItchButlerPrefsProvider {
        const string UserKey = "Itch_UserName";
        const string ProjectKey = "Itch_ProjectName";
        const string UseSystemPathKey = "Itch_UseSystemButlerPath";
        const string ButlerPathKey = "Itch_ButlerPath";

        const string ProviderName = "Itch.io butler";

        [MenuItem(ItchConsts.ItchMenuBase + "Butler Settings", priority = 0)]
        public static void OpenItchPrefs() {
            SettingsService.OpenUserPreferences($"Preferences/{ProviderName}");
        }

        [SettingsProvider]
        public static SettingsProvider CreateItchPrefsProvider() {
            var provider = new SettingsProvider($"Preferences/{ProviderName}", SettingsScope.User) {
                label = ProviderName,
                guiHandler = (searchContext) => {
                    EditorGUI.BeginChangeCheck();

                    string user = EditorPrefs.GetString(UserKey, "");
                    string project = EditorPrefs.GetString(ProjectKey, "");
                    bool useSystemPath = EditorPrefs.GetBool(UseSystemPathKey, true);
                    string butlerPath = EditorPrefs.GetString(ButlerPathKey, "");

                    user = EditorGUILayout.TextField("User Name", user);
                    project = EditorGUILayout.TextField("Project Name", project);
                    useSystemPath = EditorGUILayout.Toggle("Use system PATH", useSystemPath);
                    butlerPath = EditorGUILayout.TextField("Custom Butler Path", butlerPath);

                    if (EditorGUI.EndChangeCheck()) {
                        EditorPrefs.SetString(UserKey, user);
                        EditorPrefs.SetString(ProjectKey, project);
                        EditorPrefs.SetBool(UseSystemPathKey, useSystemPath);
                        EditorPrefs.SetString(ButlerPathKey, butlerPath);
                    }
                }
            };

            return provider;
        }

        public static Maybe<string> GetButlerExecutable() {
            bool useSystem = EditorPrefs.GetBool(UseSystemPathKey, true);
            if (useSystem)
                return "butler"; // rely on system PATH

            if (EditorPrefs.HasKey(ButlerPathKey) == false)
                return Maybe<string>.Empty;

            var customPath = EditorPrefs.GetString(ButlerPathKey, "");
            return string.IsNullOrEmpty(customPath) ? "butler" : customPath;
        }

        public static Maybe<string> GetItchChannel(string channel = "web") {
            if (EditorPrefs.HasKey(UserKey) == false)
                return Maybe<string>.Empty;
            if (EditorPrefs.HasKey(ProjectKey) == false)
                return Maybe<string>.Empty;

            string user = EditorPrefs.GetString(UserKey, "");
            string project = EditorPrefs.GetString(ProjectKey, "");
            if (user.IsNullOrWhitespace() || project.IsNullOrWhitespace())
                return Maybe<string>.Empty;
            return $"{user}/{project}:{channel}";
        }
    }
}