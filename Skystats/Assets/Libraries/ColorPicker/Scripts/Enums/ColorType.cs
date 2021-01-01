using UnityEngine;

namespace AdvancedColorPicker
{
    [System.Flags]
    public enum ColorType
    {
        RGB_R = 1 << 1,
        RGB_G = 1 << 2,
        RGB_B = 1 << 3,

        HSV_H = 1 << 4,
        HSV_S = 1 << 5,
        HSV_V = 1 << 6,

        HSL_H = 1 << 7,
        HSL_S = 1 << 8,
        HSL_L = 1 << 9
    }

    [System.Flags]
    public enum ColorValueType
    {
        Alpha = 1 << 0,

        RGB_R = 1 << 1,
        RGB_G = 1 << 2,
        RGB_B = 1 << 3,

        HSV_H = 1 << 4,
        HSV_S = 1 << 5,
        HSV_V = 1 << 6,

        HSL_H = 1 << 7,
        HSL_S = 1 << 8,
        HSL_L = 1 << 9
    }
}