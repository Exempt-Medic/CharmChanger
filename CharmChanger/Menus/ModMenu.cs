using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Satchel.BetterMenus;
using Modding;
using UnityEngine;
using UnityEngine.UI;
using MenuButton = Satchel.BetterMenus.MenuButton;
using InputField = Satchel.BetterMenus.InputField;

namespace CharmChanger;
public static class ModMenu
{
    private static Menu? MenuRef;
    private static MenuScreen? MainMenu;

    public static Dictionary<string, MenuScreen>? ExtraMenuScreens;

    //gets all the fields that have our custom atribute (that also means if you dont add the attribute, it wont be in the menu
    public static FieldInfo[] GSFields = typeof(LocalSettings).GetFields().Where(f => f.GetCustomAttribute<ModMenuElementAttribute>() != null).ToArray();
    public static MethodInfo[] GSMethods = typeof(LocalSettings).GetMethods().Where(f => f.GetCustomAttribute<ModMenuElementAttribute>() != null).ToArray();

    //make it use your mods settings
    public static LocalSettings Settings => CharmChangerMod.LS;

    public static MenuScreen CreateMenuScreen(MenuScreen modListMenu)
    {
        //create a dict for for the buttons in main menu to use to get to sub menus
        ExtraMenuScreens = new Dictionary<string, MenuScreen>();

        //create the main menu
        MenuRef = new Menu("Charm Changer Options", new Element[]
        {
            new MenuButton(
                "Reset Defaults (remove charms first)",
                "",
                submitAction =>
                {
                    CharmChangerMod.LS.ResetGrubsong();
                    CharmChangerMod.LS.ResetStalwartShell();
                    CharmChangerMod.LS.ResetBaldurShell();
                    CharmChangerMod.LS.ResetFuryOfTheFallen();
                    CharmChangerMod.LS.ResetQuickAndDeepFocus();
                    CharmChangerMod.LS.ResetLifebloodHeartAndCore();
                    CharmChangerMod.LS.ResetDefendersCrest();
                    CharmChangerMod.LS.ResetFlukenest();
                    CharmChangerMod.LS.ResetThornsOfAgony();
                    CharmChangerMod.LS.ResetLongnailAndMarkOfPride();
                    CharmChangerMod.LS.ResetHeavyBlow();
                    CharmChangerMod.LS.ResetSharpShadow();
                    CharmChangerMod.LS.ResetSporeShroom();
                    CharmChangerMod.LS.ResetShamanStone();
                    CharmChangerMod.LS.ResetSoulCatcherAndEater();
                    CharmChangerMod.LS.ResetGlowingWomb();
                    CharmChangerMod.LS.ResetFragileCharms();
                    CharmChangerMod.LS.ResetNailmastersGlory();
                    CharmChangerMod.LS.ResetJonisBlessing();
                    CharmChangerMod.LS.ResetShapeOfUnn();
                    CharmChangerMod.LS.ResetHiveblood();
                    CharmChangerMod.LS.ResetDreamWielder();
                    CharmChangerMod.LS.ResetDashmaster();
                    CharmChangerMod.LS.ResetQuickSlash();
                    CharmChangerMod.LS.ResetSpellTwister();
                    CharmChangerMod.LS.ResetGrubberflysElegy();
                    CharmChangerMod.LS.ResetKingsoul();
                    CharmChangerMod.LS.ResetSprintmaster();
                    CharmChangerMod.LS.ResetDreamshield();
                    CharmChangerMod.LS.ResetWeaversong();
                    CharmChangerMod.LS.ResetGrimmchild();
                    CharmChangerMod.LS.ResetCarefreeMelody();

                    CharmChangerMod.LS.ResetNotchCosts();
                })
        });

        // key: menu name, value: list of fields 
        Dictionary<string, List<MemberInfo>> menuScreenNameList = new();

        //im not too sure about the order on which they are so im gonna loop through myself
        foreach (var field in GSFields)
        {
            //garunteed to be there from the where above
            var menuName = field.GetCustomAttribute<ModMenuElementAttribute>().MenuName;

            if (menuScreenNameList.TryGetValue(menuName, out var list))
            {
                list.Add(field);
            }
            else
            {
                menuScreenNameList[menuName] = new List<MemberInfo>() { field };
            }
        }
        foreach (var method in GSMethods)
        {
            //garunteed to be there from the where above
            var menuName = method.GetCustomAttribute<ModMenuElementAttribute>().MenuName;

            if (menuScreenNameList.TryGetValue(menuName, out var list))
            {
                list.Add(method);
            }
            else
            {
                menuScreenNameList[menuName] = new List<MemberInfo>() { method };
            }
        }

        //we first create the buttons in main menu
        foreach (var menuScreenName in menuScreenNameList.Keys)
        {
            MenuRef.AddElement(Blueprints.NavigateToMenu(menuScreenName,
                "", //you can add desc as a parameter in the atribute if you want
                () => ExtraMenuScreens[menuScreenName]));
        }

        //we create the menu screen
        MainMenu = MenuRef.GetMenuScreen(modListMenu);

        foreach (var (menuScreenName, members) in menuScreenNameList)
        {
            var extraMenu = new Menu(menuScreenName);

            foreach (var member in members)
            {
                if (member is FieldInfo field)
                {
                    if (field.FieldType == typeof(float))
                    {
                        var info = field.GetCustomAttribute<ModMenuElementAttribute>();
                        if (info is SliderFloatElementAttribute sliderInfo)
                        {
                            var slider = new CustomSlider(
                                sliderInfo.ElementName,
                                f => { field.SetValue(Settings, f); },
                                () => (float)field.GetValue(Settings),
                                sliderInfo.MinValue,
                                sliderInfo.MaxValue,
                                false);

                            extraMenu.AddElement(slider);

                            slider.OnBuilt += () =>
                            {
                                slider.gameObject.transform.GetChild(0).Find("Label").GetComponent<Text>().fontSize = 36;
                            };
                        }
                        else if (info is InputFloatElementAttribute inputInfo)
                        {
                            var input = Blueprints.FloatInputField(
                                inputInfo.ElementName,
                                f =>
                                {
                                    f = Mathf.Clamp(f, inputInfo.MinValue, inputInfo.MaxValue);
                                    field.SetValue(Settings, f);
                                    var elem = (InputField)extraMenu.Find(inputInfo.ElementName);
                                    if (elem != null)
                                    {
                                        elem.userInput = f.ToString();
                                    }
                                },
                                () => (float)field.GetValue(Settings),
                                inputInfo.DefaultValue,
                                inputInfo.PlaceHolder,
                                inputInfo.CharacterLimit,
                                Id: inputInfo.ElementName
                            );

                            extraMenu.AddElement(input);

                            input.OnBuilt += () =>
                            {
                                input.gameObject.transform.GetChild(0).Find("Label").GetComponent<Text>().fontSize = 38;
                            };
                        }
                        else
                        {
                            Modding.Logger.LogError($"Wrong attribute assigned to {field.Name}");
                        }
                    }
                    else if (field.FieldType == typeof(int))
                    {
                        var info = field.GetCustomAttribute<ModMenuElementAttribute>();
                        if (info is SliderIntElementAttribute sliderInfo)
                        {
                            var slider = new CustomSlider(
                                sliderInfo.ElementName,
                                f => { field.SetValue(Settings, (int)f); },
                                () => (int)field.GetValue(Settings),
                                sliderInfo.MinValue,
                                sliderInfo.MaxValue,
                                true);

                            extraMenu.AddElement(slider);

                            slider.OnBuilt += () =>
                            {
                                slider.gameObject.transform.GetChild(0).Find("Label").GetComponent<Text>().fontSize = 38;
                            };
                        }
                        else if (info is InputIntElementAttribute inputInfo)
                        {
                            var input = Blueprints.IntInputField(
                                inputInfo.ElementName,
                                f =>
                                {
                                    f = Mathf.Clamp(f, inputInfo.MinValue, inputInfo.MaxValue);
                                    field.SetValue(Settings, f);
                                    var elem = (InputField)extraMenu.Find(inputInfo.ElementName);
                                    if (elem != null)
                                    {
                                        elem.userInput = f.ToString();
                                    }
                                },
                                () => (int)field.GetValue(Settings),
                                inputInfo.DefaultValue,
                                inputInfo.PlaceHolder,
                                inputInfo.CharacterLimit,
                                Id: inputInfo.ElementName
                            );

                            extraMenu.AddElement(input);

                            input.OnBuilt += () =>
                            {
                                input.gameObject.transform.GetChild(0).Find("Label").GetComponent<Text>().fontSize = 38;
                            };
                        }
                        else
                        {
                            Modding.Logger.LogError($"Wrong attribute assigned to {field.Name}");
                        }
                    }
                    else if (field.FieldType == typeof(bool))
                    {
                        var info = field.GetCustomAttribute<BoolElementAttribute>();
                        if (info == null)
                        {
                            Modding.Logger.LogError($"Wrong attribute assigned to {field.Name}");
                        }
                        else
                        {
                            extraMenu.AddElement(Blueprints.HorizontalBoolOption(
                                info.ElementName,
                                info.ElementDesc,
                                (b) => field.SetValue(Settings, b),
                                () => (bool)field.GetValue(Settings)));
                        }
                    }
                }
                else if (member is MethodInfo method)
                {
                    var info = method.GetCustomAttribute<ButtonElementAttribute>();
                    if (info == null)
                    {
                        Modding.Logger.LogError($"Wrong attribute assigned to {method.Name}");
                    }
                    else
                    {
                        extraMenu.AddElement(new MenuButton(
                            info.ElementName,
                            info.ElementDesc,
                            _ =>
                            {
                                method.Invoke(Settings, null);
                                extraMenu.Update();
                            }));
                    }
                }
            }

            ExtraMenuScreens[menuScreenName] = extraMenu.GetMenuScreen(MainMenu);
        }

        return MainMenu;
    }
}