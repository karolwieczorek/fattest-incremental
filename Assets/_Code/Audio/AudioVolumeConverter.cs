using UnityEngine;

namespace FattestInc.Audio {
    public static class AudioVolumeConverter
    {
        private const float MinDecibels = -80.0f;
        private const float MaxDecibels = 0.0f;

        /// <summary>
        /// Converts a linear volume setting (0 to 1) to a logarithmic amplitude
        /// suitable for AudioSource.volume, providing more natural control.
        /// </summary>
        /// <param name="linearVolume">Volume setting from your slider (0.0 to 1.0).</param>
        /// <returns>Calculated amplitude (0.0 to 1.0) for AudioSource.volume.</returns>
        public static float ConvertLinearToLogarithmicVolume(float linearVolume)
        {
            // Ensure input is clamped between 0 and 1
            linearVolume = Mathf.Clamp01(linearVolume);

            // Handle silence explicitly to avoid issues with log(0) or tiny values
            if (linearVolume <= 0.0001f) // Use a small threshold for practical silence
            {
                return 0.0f;
            }

            // Map the linear volume (0-1) to the decibel range (MinDecibels to MaxDecibels)
            float targetDecibels = MinDecibels + (MaxDecibels - MinDecibels) * linearVolume;
            // This is equivalent to: Mathf.Lerp(MinDecibels, MaxDecibels, linearVolume);

            // Convert decibels back to linear amplitude (amplitude = 10^(dB / 20))
            return Mathf.Pow(10.0f, targetDecibels / 20.0f);
        }
    }
}