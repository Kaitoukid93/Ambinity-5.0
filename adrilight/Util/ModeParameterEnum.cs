namespace adrilight.Util
{
    public enum ModeParameterEnum
    {
          /// <summary>
        ///    this is Brightness Type, contains brightness value of current Mode
        /// </summary>
        Brightness,

        /// <summary>
        ///    this is Color Type, contains Color value of current Mode
        /// </summary>
        Color,
        /// <summary>
        ///    this is Speed Type, contains Speed of current Mode
        /// </summary>
        Speed,
        /// <summary>
        ///    this is Direction Type, contains Direction value of current Mode
        /// </summary>
        Direction,
        /// <summary>
        ///    this is Pattern Type, specific use for rainbow mode
        /// </summary>
        ChasingPattern,
        /// <summary>
        ///    this is ColorMode, specific use for rainbow mode
        /// </summary>
        ColorMode,
        /// <summary>
        ///    this is In Sync param type, specific use for rainbow mode
        /// </summary>
        IsSystemSync,
        /// <summary>
        ///    this is In SereencIndex param type, specific use for rainbow mode
        /// </summary>
        ScreenIndex,

        LinearLighting,

        StaticColorMode,
        Breathing,
    }
}