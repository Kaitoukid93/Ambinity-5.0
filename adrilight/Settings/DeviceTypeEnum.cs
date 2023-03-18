using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace adrilight.Settings
{
    public enum  DeviceTypeEnum
    {
        /// <summary>
        /// Ambino Device Type
        /// </summary>
        AmbinoBasic,

        AmbinoEDGE,

        AmbinoFanHub,

        AmbinoHUBV3,

        AmbinoHUBV2,

        AmbinoRainPowPro,

        /// <summary>
        /// OpenRGB DeviceType
        /// </summary>

        Ram,
        /// <summary>
        ///     Motherboard device type.
        /// </summary>
        Motherboard,

        /// <summary>
        ///     RAM device type.
        /// </summary>
        Dram,

        /// <summary>
        ///     GPU device type.
        /// </summary>
        Gpu,

        /// <summary>
        ///     CPU device type.
        /// </summary>
        Cooler,

        /// <summary>
        ///     LED strip device type.
        /// </summary>
        Ledstrip,

        /// <summary>
        ///     Keyboard device type.
        /// </summary>
        Keyboard,

        /// <summary>
        ///     Mouse device type.
        /// </summary>
        Mouse,

        /// <summary>
        ///     Mousemat device type.
        /// </summary>
        Mousemat,

        /// <summary>
        ///     Headset device type.
        /// </summary>
        Headset,

        /// <summary>
        ///     Headset stand device type.
        /// </summary>
        HeadsetStand,

        /// <summary>
        ///     Gamepad device type.
        /// </summary>
        Gamepad,

        /// <summary>
        ///     Light device type.
        /// </summary>
        Light,

        /// <summary>
        ///     Speaker device type.
        /// </summary>
        Speaker,

        /// <summary>
        ///     Virtual device type.
        /// </summary>
        Virtual,

        /// <summary>
        ///     Unknown device type.
        /// </summary>
        Unknown

    }
}
