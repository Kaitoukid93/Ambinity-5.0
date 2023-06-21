﻿namespace adrilight.Util
{
    /// <summary>
    ///     Enum representing the various device types supported.
    /// </summary>
    public enum LightingModeEnum
    {
        /// <summary>
        ///     this LightingMode require screen capture engine
        /// </summary>
        ScreenCapturing,

        /// <summary>
        ///     this LightingMode require Rainbow engine
        /// </summary>
        Rainbow,

        /// <summary>
        ///     this LightingMode require Static Color engine
        /// </summary>
        StaticColor,

        /// <summary>
        ///     this LightingMode require Animation engine
        /// </summary>
        Animation,
        /// <summary>
        ///     this LightingMode require Music Color engine
        /// </summary>
        MusicCapturing,
        /// <summary>
        ///     this LightingMode require Gixelation Color engine
        /// </summary>
        Gifxelation,
        /// <summary>
        ///     this LightingMode Require Custom engine
        /// </summary>
        Custom,

    }
}
