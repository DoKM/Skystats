using System.Collections.Generic;

/// <summary>
/// Modified Gradient effect script from http://answers.unity3d.com/questions/1086415/gradient-text-in-unity-522-basevertexeffect-is-obs.html
/// -Uses Unity's Gradient class to define the color
/// -Offset is now limited to -1,1
/// -Multiple color blend modes
/// 
/// Remember that the colors are applied per-vertex so if you have multiple points on your gradient where the color changes and there aren't enough vertices, you won't see all of the colors.
/// </summary>
namespace UnityEngine.UI
{
    [AddComponentMenu("UI/Effects/Gradient")]
    public class GradientText : BaseMeshEffect
    {
        [SerializeField]
        private Type _gradientType;

        [SerializeField]
        private Blend _blendMode = Blend.Multiply;

        [SerializeField]
        [Range(-1, 1)]
        private float _offset = 0f;

        [SerializeField]
        private UnityEngine.Gradient _effectGradient = new UnityEngine.Gradient() { colorKeys = new GradientColorKey[] { new GradientColorKey(Color.black, 0), new GradientColorKey(Color.white, 1) } };

        [SerializeField]
        private string hexToReplace = "#301C46";

        [SerializeField]
        private GradientType enabledGradient;

        private Dictionary<string, Gradient> gradients = new Dictionary<string, Gradient>();

        #region Properties
        public Blend BlendMode
        {
            get { return _blendMode; }
            set { _blendMode = value; }
        }

        public UnityEngine.Gradient EffectGradient
        {
            get { return _effectGradient; }
            set { _effectGradient = value; }
        }

        public Type GradientType
        {
            get { return _gradientType; }
            set { _gradientType = value; }
        }

        public float Offset
        {
            get { return _offset; }
            set { _offset = value; }
        }
        #endregion

        public new void OnEnable()
        {
            if (GradientManager.Instance != null)
            {
                UpdateGradient();
            }

            base.OnEnable();
        }

        private void Update()
        {
            UpdateGradient();
        }

        private void UpdateGradient()
        {
            if (Application.isPlaying)
            {
                if (GradientManager.Instance.currentTheme.ThemeColors.FindIndex((x) => x.Type == enabledGradient) < 0)
                    Debug.LogError("THIS GRADIENT NAME WAS NOT FOUND IN GRADIENT MANAGER");
                if (_effectGradient != GetOffsetGradient(GradientManager.Instance.GetCurrentThemeColor(enabledGradient).Color))
                    _effectGradient = GetOffsetGradient(GradientManager.Instance.GetCurrentThemeColor(enabledGradient).Color);
            }
        }

        public Gradient GetOffsetGradient(Gradient gradient)
        {
            var newGradient = gradient;

            var newColorKeys = new GradientColorKey[2];
            var newAlphaKeys = new GradientAlphaKey[2];

            GradientColorKey firstKey = newGradient.colorKeys[0];
            firstKey.time = 0.35f;
            newColorKeys[0] = firstKey;
            newAlphaKeys[0] = newGradient.alphaKeys[0];

            GradientColorKey secondKey = newGradient.colorKeys[1];
            secondKey.time = 0.65f;
            newColorKeys[1] = secondKey;
            newAlphaKeys[1] = newGradient.alphaKeys[1];

            newGradient.SetKeys(newColorKeys, newAlphaKeys);
            return newGradient;
        }

        public override void ModifyMesh(VertexHelper helper)
        {
            if (!IsActive() || helper.currentVertCount == 0)
                return;

            List<UIVertex> _vertexList = new List<UIVertex>();

            helper.GetUIVertexStream(_vertexList);

            int nCount = _vertexList.Count;
            switch (GradientType)
            {
                case Type.Horizontal:
                    {
                        float left = _vertexList[0].position.x;
                        float right = _vertexList[0].position.x;
                        float x = 0f;

                        for (int i = nCount - 1; i >= 1; --i)
                        {
                            x = _vertexList[i].position.x;

                            if (x > right) right = x;
                            else if (x < left) left = x;
                        }

                        float width = 1f / (right - left);
                        UIVertex vertex = new UIVertex();

                        for (int i = 0; i < helper.currentVertCount; i++)
                        {
                            helper.PopulateUIVertex(ref vertex, i);

                            //#301C46
                            if (vertex.color == HexToColor(hexToReplace))
                            {
                                vertex.color = BlendColor(vertex.color, EffectGradient.Evaluate((vertex.position.x - left) * width - Offset));
                                helper.SetUIVertex(vertex, i);
                            }
                        }
                    }
                    break;

                case Type.Vertical:
                    {
                        float bottom = _vertexList[0].position.y;
                        float top = _vertexList[0].position.y;
                        float y = 0f;

                        for (int i = nCount - 1; i >= 1; --i)
                        {
                            y = _vertexList[i].position.y;

                            if (y > top) top = y;
                            else if (y < bottom) bottom = y;
                        }

                        float height = 1f / (top - bottom);
                        UIVertex vertex = new UIVertex();

                        for (int i = 0; i < helper.currentVertCount; i++)
                        {
                            helper.PopulateUIVertex(ref vertex, i);

                            //#301C46
                            if (vertex.color == HexToColor(hexToReplace))
                            {
                                vertex.color = BlendColor(vertex.color, EffectGradient.Evaluate((vertex.position.y - bottom) * height - Offset));
                                helper.SetUIVertex(vertex, i);
                            }
                        }
                    }
                    break;
            }
        }

        Color BlendColor(Color colorA, Color colorB)
        {
            switch (BlendMode)
            {
                default: return colorB;
                case Blend.Add: return colorA + colorB;
                case Blend.Multiply: return colorA * colorB;
            }
        }

        public enum Type
        {
            Horizontal,
            Vertical
        }

        public enum Blend
        {
            Override,
            Add,
            Multiply
        }

        public static Color HexToColor(string hex)
        {
            hex = hex.Replace("0x", "");//in case the string is formatted 0xFFFFFF
            hex = hex.Replace("#", "");//in case the string is formatted #FFFFFF
            byte a = 255;//assume fully visible unless specified in hex
            byte r = byte.Parse(hex.Substring(0, 2), System.Globalization.NumberStyles.HexNumber);
            byte g = byte.Parse(hex.Substring(2, 2), System.Globalization.NumberStyles.HexNumber);
            byte b = byte.Parse(hex.Substring(4, 2), System.Globalization.NumberStyles.HexNumber);
            //Only use alpha if the string has enough characters
            if (hex.Length == 8)
            {
                a = byte.Parse(hex.Substring(6, 2), System.Globalization.NumberStyles.HexNumber);
            }
            return new Color32(r, g, b, a);
        }

    }
}

/*// #301C46
            */