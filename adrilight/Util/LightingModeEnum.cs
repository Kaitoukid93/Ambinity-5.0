namespace adrilight.Util
{
    /// <summary>
    ///     Enum representing the various device types supported.
    /// </summary>
    public enum LightingModeEnum
    {
        /// <summary>
        ///     this LightingMode require screen capture engine
        /// </summary>
        ScreenCapturing = 1,

        /// <summary>
        ///     this LightingMode require Rainbow engine
        /// </summary>
        Rainbow = 2,

        /// <summary>
        ///     this LightingMode require Static Color engine
        /// </summary>
        StaticColor = 3,

        /// <summary>
        ///     this LightingMode require Animation engine
        /// </summary>
        Animation = 4,
        /// <summary>
        ///     this LightingMode require Music Color engine
        /// </summary>
        MusicCapturing = 5,
        /// <summary>
        ///     this LightingMode require Gixelation Color engine
        /// </summary>
        Gifxelation = 6,
        /// <summary>
        ///     this LightingMode Require Custom engine
        /// </summary>
        Custom = 7,

    }
}
