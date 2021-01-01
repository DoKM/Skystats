using UnityEngine;
using System;

namespace AdvancedColorPicker
{
    public static class ColorPickerUtils
    {
        private static Texture2D _checkboard = null;

        /// <summary>
        /// The checkboard background texture used by all ColorPicker components
        /// </summary>
        public static Texture2D Checkboard
        {
            get
            {
                if (_checkboard == null)
                {
                    int checkboardsize = 8;

                    _checkboard = new Texture2D(checkboardsize, checkboardsize);
                    _checkboard.hideFlags = HideFlags.HideAndDontSave;

                    Color32[] pixels = new Color32[checkboardsize * checkboardsize];
                    for (int x = 0; x < checkboardsize; x++)
                    {
                        for (int y = 0; y < checkboardsize; y++)
                        {
                            byte value = ((x < checkboardsize / 2 && y < checkboardsize / 2) || (x >= checkboardsize / 2 && y >= checkboardsize / 2)) ? (byte)255 : (byte)100;
                            pixels[x + (y * checkboardsize)] = new Color32(value, value, value, 255);
                        }
                    }
                    _checkboard.SetPixels32(pixels);
                    _checkboard.Apply();
                }
                return _checkboard;
            }
        }

        /// <summary>
        /// Returns true when the given format is valid, false otherwise
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="target"></param>
        /// <param name="Format"></param>
        /// <returns></returns>
        /// <remarks>
        /// Helper extension method that checks whether the given format is valid for the target.
        /// </remarks>
        public static bool IsFormatValid<T>(this T target, string Format) where T : IFormattable
        {
            try
            {
                target.ToString(Format, null);
            }
            catch
            {
                return false;
            }
            return true;
        }

        public static bool SetClass<T>(ref T currentValue, T newValue) where T : class
        {
            if ((currentValue == null && newValue == null) || (currentValue != null && currentValue.Equals(newValue)))
                return false;

            currentValue = newValue;
            return true;
        }

        public static bool SetStruct<T>(ref T currentValue, T newValue) where T : struct
        {
            if (currentValue.Equals(newValue))
                return false;

            currentValue = newValue;
            return true;
        }
    }
}