using System;
using FattestInc.Windows;
using FattestInc.Windows.General;
using Hypnagogia.Utils;
using UnityEngine;
using Utils.PlayerPrefUtils;

namespace FattestInc {
    public class ChangelogGateSystem : HSystem {
        [HInject] WindowManager windowManager;
        
        const string PrefKey = "last_seen_version";
        
        readonly PlayerPrefString lastSeenVersionPref = new(PrefKey, "");
        Version LastSeen {
            get {
                var raw = lastSeenVersionPref.Value;
                return VersionUtils.TryParseVersion(raw, out var v) ? v : VersionUtils.NullVersion;
            }
        }

        protected override void SystemStart() {
            if (ShouldShowOnLaunch()) {
                // TODO show changelog window
                windowManager.OpenOnTop<ChangelogWindow>();
                MarkSeen();
            }
        }

        bool ShouldShowOnLaunch() {
            var cur = VersionUtils.Current;
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