using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Text;
using System.Globalization;
using System;

namespace AdvancedColorPicker
{
    [RequireComponent(typeof(InputField)), ExecuteInEditMode]
    public class ColorHexField : ColorComponent
    {
        public enum HexfieldType
        {
            RGB = 1 << 0,
            RGBA = 1 << 1,
            RRGGBB = 1 << 2,
            RRGGBBAA = 1 << 3,
        }

        [SerializeField]
        private bool displayAlpha = true;

        [SerializeField]
        private bool displayHashtag = true;

        [SerializeField]
        private HexfieldType acceptedInput = HexfieldType.RGB | HexfieldType.RGBA | HexfieldType.RRGGBB | HexfieldType.RRGGBBAA;

        private InputField hexInputField;

        private const string hexRegex = "^#?(?:[0-9a-fA-F]{3,4}){1,2}$";

        private StringBuilder builder = new StringBuilder();

        public bool DisplayAlpha
        {
            get
            {
                return displayAlpha;
            }
            set
            {
                if (displayAlpha == value)
                    return;

                displayAlpha = value;
                DisplayNewColor();
            }
        }

        public bool DisplayHashtag
        {
            get
            {
                return displayHashtag;
            }
            set
            {
                if (displayHashtag == value)
                    return;

                displayHashtag = value;
                DisplayNewColor();
            }
        }

        public HexfieldType AcceptedInput
        {
            get
            {
                return acceptedInput;
            }
            set
            {
                if (acceptedInput == value)
                    return;

                acceptedInput = value;
                DisplayNewColor();
            }
        }

        protected override void Awake()
        {
            base.Awake();
            hexInputField = GetComponent<InputField>();
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            DisplayNewColor();
            hexInputField.onEndEdit.AddListener(HexChanged);
            hexInputField.onValidateInput = ValidateHexField;
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            hexInputField.onEndEdit.RemoveListener(HexChanged);
        }

        private char ValidateHexField(string text, int charIndex, char addedChar)
        {
            switch (addedChar)
            {
                case '#': // Checking for first doesn't work sadly as when typing over selected text, and thus replacing it. The text and charIndex are wrong, as they are still that of the selected text in which the # would be invalid at given index
                case 'A':
                case 'B':
                case 'C':
                case 'D':
                case 'E':
                case 'F':
                case '0':
                case '1':
                case '2':
                case '3':
                case '4':
                case '5':
                case '6':
                case '7':
                case '8':
                case '9':
                    return addedChar;
                default:
                    return '\0';
            }
        }

        private void HexChanged(string newHex)
        {
            Color32 color;
            if (HexToColor(newHex, out color))
            {
                Picker.CurrentColor = color;
            }

            hexInputField.text = ColorToHex(Picker != null ? Picker.CurrentColor : new Color(0, 0, 0, 0));
        }

        private string ColorToHex(Color32 color)
        {
            builder.Length = 0;
            if (displayHashtag)
                builder.Append("#");
            builder.AppendFormat("{0:X2}{1:X2}{2:X2}", color.r, color.g, color.b);
            if (displayAlpha)
                builder.AppendFormat("{0:X2}", color.a);
            return builder.ToString();
        }

        private bool HexToColor(string hex, out Color32 color)
        {
            // Check if this could be a valid hex string (# is optional)
            if (System.Text.RegularExpressions.Regex.IsMatch(hex, hexRegex))
            {
                int startIndex = hex.StartsWith("#") ? 1 : 0;

                if ((acceptedInput & HexfieldType.RRGGBBAA) != 0 && hex.Length == startIndex + 8) //#RRGGBBAA
                {
                    color = new Color32(byte.Parse(hex.Substring(startIndex, 2), NumberStyles.AllowHexSpecifier),
                        byte.Parse(hex.Substring(startIndex + 2, 2), NumberStyles.AllowHexSpecifier),
                        byte.Parse(hex.Substring(startIndex + 4, 2), NumberStyles.AllowHexSpecifier),
                        byte.Parse(hex.Substring(startIndex + 6, 2), NumberStyles.AllowHexSpecifier));
                }
                else if ((acceptedInput & HexfieldType.RRGGBB) != 0 && hex.Length == startIndex + 6)  //#RRGGBB
                {
                    color = new Color32(byte.Parse(hex.Substring(startIndex, 2), NumberStyles.AllowHexSpecifier),
                        byte.Parse(hex.Substring(startIndex + 2, 2), NumberStyles.AllowHexSpecifier),
                        byte.Parse(hex.Substring(startIndex + 4, 2), NumberStyles.AllowHexSpecifier),
                        Picker == null ? (byte)255 : Picker.Alpha);
                }
                else if ((acceptedInput & HexfieldType.RGBA) != 0 && hex.Length == startIndex + 4) //#RGBA
                {
                    color = new Color32(byte.Parse("" + hex[startIndex] + hex[startIndex], NumberStyles.AllowHexSpecifier),
                        byte.Parse("" + hex[startIndex + 1] + hex[startIndex + 1], NumberStyles.AllowHexSpecifier),
                        byte.Parse("" + hex[startIndex + 2] + hex[startIndex + 2], NumberStyles.AllowHexSpecifier),
                        byte.Parse("" + hex[startIndex + 3] + hex[startIndex + 3], NumberStyles.AllowHexSpecifier));
                }
                else if ((acceptedInput & HexfieldType.RGB) != 0 && hex.Length == startIndex + 3)  //#RGB
                {
                    color = new Color32(byte.Parse("" + hex[startIndex] + hex[startIndex], NumberStyles.AllowHexSpecifier),
                        byte.Parse("" + hex[startIndex + 1] + hex[startIndex + 1], NumberStyles.AllowHexSpecifier),
                        byte.Parse("" + hex[startIndex + 2] + hex[startIndex + 2], NumberStyles.AllowHexSpecifier),
                        Picker == null ? (byte)255 : Picker.Alpha);
                }
                else
                {
                    color = new Color32(); // no valid hex
                    return false;
                }
                return true;
            }
            else
            {
                color = new Color32(); // no valid hex
                return false;
            }
        }

        protected override void DisplayNewColor()
        {
            if (!isActiveAndEnabled)
                return;

            if (Picker != null)
                hexInputField.text = ColorToHex(Picker.CurrentColor);
            else
                hexInputField.text = ColorToHex(Color.clear);
        }
    }
}