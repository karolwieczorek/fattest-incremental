using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using FattestInc.Windows.General;
using TMPro;
using UnityEngine;

namespace FattestInc.Windows {
    public class ChangelogWindow : SimpleWindow {
        [SerializeField] TMP_Text descriptionLabel;

        const string Separator = "\n\n__________________________________________\n\n";

        void Awake() {
            descriptionLabel.text = "";
            var changelogList = LoadAll();
            foreach (var changelogDoc in changelogList) {
                descriptionLabel.text += changelogDoc.Body + Separator;
            }
        }

        static readonly Regex FileRx = new(@"^v(?<ver>\d+(\.\d+){1,2})_(?<date>\d{4}-\d{2}-\d{2})_(?<loc>[a-z]{2}(-[A-Z]{2})?)$",
            RegexOptions.Compiled);
        
        [Serializable]
        public class ChangelogDoc
        {
            public Version Version;
            public DateTime Date;
            public string Locale;
            public string Body;
        }

        public static List<ChangelogDoc> LoadAll(string preferredLocale = "en")
        {
            var assets = Resources.LoadAll<TextAsset>("Changelog");
            var docs = new List<ChangelogDoc>();

            foreach (var asset in assets)
            {
                var match = FileRx.Match(asset.name);
                if (!match.Success)
                {
                    Debug.LogWarning($"Changelog file name invalid: {asset.name}");
                    continue;
                }

                var ver = new Version(match.Groups["ver"].Value);
                var date = DateTime.Parse(match.Groups["date"].Value);
                var loc = match.Groups["loc"].Value;

                docs.Add(new ChangelogDoc
                {
                    Version = ver,
                    Date = date,
                    Locale = loc,
                    Body = asset.text
                });
            }

            // Filter by preferred locale, fallback to en
            var primary = docs.Where(d => d.Locale == preferredLocale).ToList();
            var fallback = docs.Where(d => d.Locale.StartsWith("en")).ToList();
            var merged = primary.Any() ? primary : fallback;

            return merged
                .OrderByDescending(d => d.Version)
                .ThenByDescending(d => d.Date)
                .ToList();
        }
    }
}