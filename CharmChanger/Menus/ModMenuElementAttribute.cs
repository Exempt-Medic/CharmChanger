using System;

namespace CharmChanger;

//a base attribute class for defining meta data
public abstract class ModMenuElementAttribute : Attribute
{
    public string MenuName;
    public string ElementName;
    public string ElementDesc; //you can remove this if you want

    public ModMenuElementAttribute(string menuName, string elementName, string elementDesc)
    {
        MenuName = menuName;
        ElementName = elementName;
        ElementDesc = elementDesc;
    }
}

public class BoolElementAttribute : ModMenuElementAttribute
{
    public BoolElementAttribute(string menuName, string elementName, string elementDesc) : base(menuName, elementName, elementDesc) { }
}
public class ButtonElementAttribute : ModMenuElementAttribute
{
    public ButtonElementAttribute(string menuName, string elementName, string elementDesc) : base(menuName, elementName, elementDesc) { }
}
public class SliderFloatElementAttribute : ModMenuElementAttribute
{
    public float MinValue;
    public float MaxValue;

    public SliderFloatElementAttribute(string menuName, string elementName, float minValue, float maxValue) :
        base(menuName, elementName, "")
    {
        MinValue = minValue;
        MaxValue = maxValue;
    }
}
public class SliderIntElementAttribute : ModMenuElementAttribute
{
    public int MinValue;
    public int MaxValue;

    public SliderIntElementAttribute(string menuName, string elementName, int minValue, int maxValue) :
        base(menuName, elementName, "")
    {
        MinValue = minValue;
        MaxValue = maxValue;
    }
}