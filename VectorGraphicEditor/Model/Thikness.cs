using System.ComponentModel;

namespace VectorGraphicEditor.Model
{
    public enum Thikness
    {
        [Description("Тонкая")]
        Thin = 1,

        [Description("Двойная")]
        Double = 2,

        [Description("Тройная")]
        Triple = 3,

        [Description("Четверная")]
        Fourth = 4
    }

}
