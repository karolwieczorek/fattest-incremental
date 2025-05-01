namespace Hypnagogia.Utils {
    public static class NumbersFormattingUtil {
        public static string FormatNumber(ulong number) {
            string formattedNumber;
            if (TryReturn(out formattedNumber, 1_000_000_000_000, "T")) return formattedNumber;
            if (TryReturn(out formattedNumber, 1_000_000_000, "B")) return formattedNumber;
            if (TryReturn(out formattedNumber, 1_000_000, "M")) return formattedNumber;
            if (TryReturn(out formattedNumber, 1_000, "K")) return formattedNumber;
            
            return number.ToString("0");

            bool TryReturn(out string localFormattedNumber, ulong value, string suffix) {
                if (number >= value * 100) {
                    localFormattedNumber = (number / (double)value).ToString("0") + suffix;
                    return true;
                }
                if (number >= value * 10) {
                    localFormattedNumber = (number / (double)value).ToString("0.#") + suffix;
                    return true;
                }
                if (number >= value) {
                    localFormattedNumber = (number / (double)value).ToString("0.##") + suffix;
                    return true;
                }

                localFormattedNumber = "";
                return false;
            }
        }
        
        
    }
}