using System;
using System.Text.RegularExpressions;
using FattestInc.Windows;
using FattestInc.Windows.General;
using Hypnagogia.Utils;
using UnityEngine;
using Utils.PlayerPrefUtils;

namespace FattestInc {
    public class ChangelogGateSystem : HSystem {
        [HInject] WindowManager windowManager;
        
        const string PrefKey = "last_seen_version";
        static readonly Version DefaultVersion = new(0, 0, 0);
        readonly PlayerPrefString lastSeenVersionPref = new(PrefKey, "");

        static Version Current => TryParseVersion(Application.version, out var v) ? v : DefaultVersion;
        static Version LastSeen
        {
            get {
                var raw = PlayerPrefs.GetString(PrefKey, "");
                return TryParseVersion(raw, out var v) ? v : DefaultVersion;
            }
        }

        protected override void SystemStart() {
            if (ShouldShowOnLaunch()) {
                // TODO show changelog window
                windowManager.OpenOnTop<ChangelogWindow>();
                MarkSeen();
            }
        }

        static bool TryParseVersion(string s, out Version v) {
            // Allows Unity versions like "1.2.3f1", "1.2.3-beta", etc.
            var m = Regex.Match(s, @"\d+(\.\d+){1,2}");
            if (!m.Success) {
                v = DefaultVersion;
                return false;
            }

            return Version.TryParse(m.Value, out v);
        }
        
        static bool ShouldShowOnLaunch() {
            var cur = Current;
            var seen = LastSeen;
            // If nothing saved yet, treat as not seen → show
            if (seen.Major == 0 && seen.Minor == 0 && seen.Build == 0)
                return true;

            return cur > seen;
        }

        void MarkSeen() {
            lastSeenVersionPref.Value = Application.version;
        }
    }
}