using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UI.Extensions.ColorPicker;

namespace Visualization.UI
{
    public class SelectColorFromPallete : MonoBehaviour
    { 
        public InputField selectedColorCode;
        public GameObject classPreset;
        public GameObject methodPreset;
        public GameObject relationPreset;
        public InputField colorInput;
        string SelectedPreset = "class";
        int i = 0;
        public void SetColor()
        {
            ToolManager.Instance.SelectColor(selectedColorCode.text);
            if (SelectedPreset.Equals("class"))
            {
                Animation.Animation.Instance.classColor = ToolManager.Instance.SelectedColor;
                SetButtonColor(classPreset, ToolManager.Instance.SelectedColor);
            }
            if (SelectedPreset.Equals("method"))
            {
                Animation.Animation.Instance.methodColor = ToolManager.Instance.SelectedColor;
                SetButtonColor(methodPreset, ToolManager.Instance.SelectedColor);
            }
            if (SelectedPreset.Equals("relation"))
            {
                Animation.Animation.Instance.relationColor = ToolManager.Instance.SelectedColor;
                SetButtonColor(relationPreset, ToolManager.Instance.SelectedColor);
            }
        }
        public void SetColor(Color c,Color m, Color r)
        {
                SetButtonColor(classPreset, c);
                Animation.Animation.Instance.classColor = c;
                SetButtonColor(methodPreset, m);
                Animation.Animation.Instance.methodColor = m;
                SetButtonColor(relationPreset, r); ;
                Animation.Animation.Instance.relationColor = r;
        }
        public class ColorGenerator : MonoBehaviour
        {
            public static Color GetRandomColor()
            {
                float red = Random.Range(0f, 1f);
                float green = Random.Range(0f, 1f);
                float blue = Random.Range(0f, 1f);

                return new Color(red, green, blue);
            }
        }
        public void RandomPallete()
        {

            Color bg = ColorGenerator.GetRandomColor();
            Color c = ColorGenerator.GetRandomColor();
            Color m = ColorGenerator.GetRandomColor();
            Color r = ColorGenerator.GetRandomColor();
            if (i == 0)
            {
                ColorUtility.TryParseHtmlString("#2d334a", out bg);
                ColorUtility.TryParseHtmlString("#0c9463", out c);
                ColorUtility.TryParseHtmlString("#fbe3b9", out m);
                ColorUtility.TryParseHtmlString("#fab696", out r);
            }
            if (i == 3)
            {
                ColorUtility.TryParseHtmlString("#522d5b", out bg);
                ColorUtility.TryParseHtmlString("#e7d39f", out c);
                ColorUtility.TryParseHtmlString("#d7385e", out m);
                ColorUtility.TryParseHtmlString("#fb7b6b", out r);
            }
            if (i == 2)
            {
                ColorUtility.TryParseHtmlString("#434e52", out bg);
                ColorUtility.TryParseHtmlString("#5b8c85", out c);
                ColorUtility.TryParseHtmlString("#ecce6d", out m);
                ColorUtility.TryParseHtmlString("#ecce6d", out r);
            }
            if (i == 1)
            {
                ColorUtility.TryParseHtmlString("#596157", out bg);
                ColorUtility.TryParseHtmlString("#5b8c5a", out c);
                ColorUtility.TryParseHtmlString("#cfd186", out m);
                ColorUtility.TryParseHtmlString("#cfd186", out r);
            }
            i++;
            if (i > 3) i = 0;
            SetColor(c, m, r);
            GameObject.Find("Main Camera").GetComponent<Camera>().backgroundColor = bg;
        }
        public void SetPreset(string type)
        {
            SelectedPreset = type;
            colorInput.text = "#" + Animation.Animation.Instance.GetColorCode(type);
            colorInput.GetComponent<HexColorField>().UpdateColor(colorInput.text);
        }
        public void SetButtonColor(GameObject b, Color c)
        {
            Button button = b.GetComponent<Button>();
            ColorBlock cb = button.colors;
            cb.normalColor = c;
            button.colors = cb;
        }
    }
}