using System;
using System.Text.RegularExpressions;
using UnityEngine;

public static class VersionUtils {
    public static readonly Version NullVersion = new(0, 0, 0);
    public static Version Current => TryParseVersion(Application.version, out var v) ? v : NullVersion;

    public static bool TryParseVersion(string s, out Version v) {
        // Allows Unity versions like "1.2.3f1", "1.2.3-beta", etc.
        var m = Regex.Match(s, @"\d+(\.\d+){1,2}");
        if (!m.Success) {
            v = NullVersion;
            return false;
        }

        return Version.TryParse(m.Value, out v);
    }
}