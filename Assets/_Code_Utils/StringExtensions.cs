namespace Hypnagogia.Utils {
    public static class StringExtensions {
        public static bool IsNullOrWhitespace(this string str) {
            if (string.IsNullOrEmpty(str))
                return true;
            foreach (var t in str) {
                if (!char.IsWhiteSpace(t))
                    return false;
            }
            return true;
        }
    }
}