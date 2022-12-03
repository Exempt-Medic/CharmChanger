using Modding;
using System;
using HutongGames.PlayMaker;
using HutongGames.PlayMaker.Actions;
using UnityEngine;
using GlobalEnums;
using System.Collections;
using Satchel.BetterMenus;
using Modding.Menu;

namespace CharmChanger
{
    public static class ModMenu
    {
        private static Menu? MenuRef;
        private static Menu? GrubsongMenuRef;
        private static MenuScreen? GrubsongMenu;
        private static Menu? StalwartShellMenuRef;
        private static MenuScreen? StalwartShellMenu;
        private static Menu? BaldurShellMenuRef;
        private static MenuScreen? BaldurShellMenu;
        public static MenuScreen CreateModMenu(MenuScreen modlistmenu)
        {
            #region Main Menu
            MenuRef ??= new Menu("Charm Changer Options", new Element[]
            {
                Blueprints.NavigateToMenu("Grubsong Options", "", () => GrubsongMenu),
                Blueprints.NavigateToMenu("Stalwart Shell Options", "", () => StalwartShellMenu),
                Blueprints.NavigateToMenu("Baldur Shell Options", "", () => BaldurShellMenu),
            });

            MenuScreen MainMenuScreen = MenuRef.GetMenuScreen(modlistmenu);
            #endregion
            #region Grubsong Menu
            GrubsongMenuRef ??= new Menu("Grubsong Options", new Element[]
            {
                new CustomSlider(
                    "Soul",
                    f =>
                    {
                        CharmChangerMod.LS.grubsongDamageSoul = (int)f;
                        CharmChangerMod.LS.grubsongComboBool = true;
                        CharmChangerMod.LS.grubsongDamageSoulCombo = Mathf.Min(100, (int)f + (CharmChangerMod.LS.grubsongComboBool ? 10 : 0));
                        GrubsongMenuRef?.Update();
                    },
                    () => CharmChangerMod.LS.grubsongDamageSoul,
                    0f,
                    100f,
                    true),

                new CustomSlider(
                    "Grubberfly's Soul",
                    f =>
                    {
                        CharmChangerMod.LS.grubsongDamageSoulCombo = (int)f;
                        CharmChangerMod.LS.grubsongComboBool = false;
                    },
                    () => CharmChangerMod.LS.grubsongDamageSoulCombo,
                    0f,
                    100f,
                    true),

                new CustomSlider(
                    "Weaversong Soul",
                    f =>
                    {
                        CharmChangerMod.LS.grubsongWeaversongSoul = (int)f;
                    },
                    () => CharmChangerMod.LS.grubsongWeaversongSoul,
                    0f,
                    100f,
                    true),
            });
            GrubsongMenu = GrubsongMenuRef.GetMenuScreen(MainMenuScreen);
            #endregion
            #region Stalwart Shell Menu
            StalwartShellMenuRef ??= new Menu("Stalwart Shell Options", new Element[]
            {
                new TextPanel("Invulnerability Time", 1000, 50),

                new CustomSlider(
                    "(twentieths of seconds)",
                    f =>
                    {
                        CharmChangerMod.LS.stalwartShellInvulnerability = f;
                    },
                    () => CharmChangerMod.LS.stalwartShellInvulnerability,
                    0f,
                    100f,
                    true),

                new TextPanel("Inaction Time", 1000, 50),

                new CustomSlider(
                    "(hundreths of seconds)",
                    f =>
                    {
                        CharmChangerMod.LS.stalwartShellRecoil = f;
                    },
                    () => CharmChangerMod.LS.stalwartShellRecoil,
                    0f,
                    20f,
                    true)
            });
            StalwartShellMenu = StalwartShellMenuRef.GetMenuScreen(MainMenuScreen);
            #endregion
            #region Baldur Shell Menu
            BaldurShellMenuRef ??= new Menu("Baldur Shell Options", new Element[]
            {
                new CustomSlider(
                    "???",
                    f =>
                    {
                        CharmChangerMod.LS.stalwartShellInvulnerability = f;
                    },
                    () => CharmChangerMod.LS.stalwartShellInvulnerability,
                    0f,
                    25f,
                    false)
            });
            BaldurShellMenu = BaldurShellMenuRef.GetMenuScreen(MainMenuScreen);
            #endregion
            return MenuRef.GetMenuScreen(modlistmenu);
        }
        #region Bool Option Definition
        public static HorizontalOption BoolOption(
            string name,
            string description,
            Action<bool> applySetting,
            Func<bool> loadSetting,
            string _true = "True",
            string _false = "False",
            string Id = "__UseName")
        {
            if (Id == "__UseName")
            {
                Id = name;
            }

            return new HorizontalOption(
                name,
                description,
                new[] { _true, _false },
                (i) => applySetting(i == 0),
                () => loadSetting() ? 0 : 1,
                Id
            );
        }
#endregion
    }
    public class CharmChangerMod : Mod, ICustomMenuMod, ILocalSettings<LocalSettings>
    {
        #region Boilerplate

        private static CharmChangerMod? _instance;

        internal static CharmChangerMod Instance
        {
            get
            {
                if (_instance == null)
                {
                    throw new InvalidOperationException($"An instance of {nameof(CharmChangerMod)} was never constructed");
                }
                return _instance;
            }
        }

        public static LocalSettings LS { get; private set; } = new();
        public void OnLoadLocal(LocalSettings s) => LS = s;
        public LocalSettings OnSaveLocal() => LS;
        public override string GetVersion() => GetType().Assembly.GetName().Version.ToString();
        public MenuScreen GetMenuScreen(MenuScreen modListMenu, ModToggleDelegates? toggleDelegates) => ModMenu.CreateModMenu(modListMenu);
        public bool ToggleButtonInsideMenu => false;

        public CharmChangerMod() : base("CharmChanger")
        {
            _instance = this;
        }
        #endregion
        #region Init
        public override void Initialize()
        {
            Log("Initializing");

            On.HeroController.TakeDamage += OnHCTakeDamage;
            On.HeroController.StartRecoil += OnHCStartRecoil;
            On.HutongGames.PlayMaker.Actions.CallMethodProper.OnEnter += OnCallMethodProperAction;

            Log("Initialized");
        }

        #endregion

        #region Grubsong
        private void OnHCTakeDamage(On.HeroController.orig_TakeDamage orig, HeroController self, GameObject go, CollisionSide damageSide, int damageAmount, int hazardType)
        {
            // Grubsong
            self.GRUB_SOUL_MP = LS.grubsongDamageSoul;

            // Grubsong + Grubberfly's
            self.GRUB_SOUL_MP_COMBO = LS.grubsongDamageSoulCombo;

            orig(self, go, damageSide, damageAmount, hazardType);
        }

        private void OnCallMethodProperAction(On.HutongGames.PlayMaker.Actions.CallMethodProper.orig_OnEnter orig, CallMethodProper self)
        {
            // Grubsong + Weaversong
            if (self.Fsm.GameObject.name == "Enemy Damager" && self.Fsm.Name == "Attack" && self.State.Name == "Grubsong" && self.methodName.Value == "AddMPCharge")
            {
                self.parameters = new FsmVar[1] { new FsmVar(typeof(int)) { intValue = LS.grubsongWeaversongSoul } };
            }

            orig(self);
        }
        #endregion
        #region Stalwart Shell
        private IEnumerator OnHCStartRecoil(On.HeroController.orig_StartRecoil orig, HeroController self, CollisionSide impactSide, bool spawnDamageEffect, int damageAmount)
        {
            self.INVUL_TIME_STAL = LS.stalwartShellInvulnerability / 20f;
            self.RECOIL_DURATION_STAL = LS.stalwartShellRecoil / 100f;

            return orig(self, impactSide, spawnDamageEffect, damageAmount);
        }
        #endregion
        #region Baldur Shell

        #endregion
    }
    public class LocalSettings
    {
        #region Grubsong Settings
        public int grubsongDamageSoul = 15;
        public int grubsongDamageSoulCombo = 25;
        public int grubsongWeaversongSoul = 3;
        public bool grubsongComboBool = true;
        #endregion
        #region Stalwart Shell Settings
        public float stalwartShellInvulnerability = 35f;
        public float stalwartShellRecoil = 20f;
        #endregion
        #region Baldur Shell Settings

        #endregion
    }
}
