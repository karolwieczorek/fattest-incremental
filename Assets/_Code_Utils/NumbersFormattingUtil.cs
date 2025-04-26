namespace Hypnagogia.Utils {
    public static class NumbersFormattingUtil {
        public static string FormatNumber(ulong number) {
            if (number >= 100_000_000_000)
                return (number / 1_000_000_000D).ToString("0") + "B";
            if (number >= 10_000_000_000)
                return (number / 1_000_000_000D).ToString("0.#") + "B";
            if (number >= 1_000_000_000)
                return (number / 1_000_000_000D).ToString("0.##") + "B";

            if (number >= 100_000_000)
                return (number / 1_000_000D).ToString("0") + "M";
            if (number >= 10_000_000)
                return (number / 1_000_000D).ToString("0.#") + "M";
            if (number >= 1_000_000)
                return (number / 1_000_000D).ToString("0.##") + "M";
            
            if (number >= 100_000)
                return (number / 1_000D).ToString("0") + "K";
            if (number >= 10_000)
                return (number / 1_000D).ToString("0.#") + "K";
            if (number >= 1_000)
                return (number / 1_000D).ToString("0.##") + "K";
            
            return number.ToString("0");
        }
    }
}