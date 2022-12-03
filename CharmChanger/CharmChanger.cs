using Modding;
using System;
using HutongGames.PlayMaker;
using HutongGames.PlayMaker.Actions;
using Satchel.BetterMenus;
using UnityEngine;
using GlobalEnums;
using System.Collections;

namespace CharmChanger
{
    #region Menu
    public static class ModMenu
    {
        private static Menu? MenuRef;
        public static MenuScreen CreateModMenu(MenuScreen modlistmenu)
        {
            MenuRef ??= new Menu("Charm Changer Options", new Element[]
            {
                #region Grubsong Menu
                new TextPanel("Grubsong Options", 1000, 70),

                new CustomSlider(
                    "Soul",
                    f =>
                    {
                        CharmChangerMod.LS.grubsongDamageSoul = (int)f;
                        CharmChangerMod.LS.grubsongComboBool = true;
                        CharmChangerMod.LS.grubsongDamageSoulCombo = Mathf.Min(100, (int)f + (CharmChangerMod.LS.grubsongComboBool ? 10 : 0));
                        MenuRef?.Update();
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
#endregion
                #region Stalwart Shell Menu
                new TextPanel("Stalwart Shell Options", 1000, 70),

                new TextPanel("Invulnerability Time"),

                new CustomSlider(
                    "(twentieths)",
                    f =>
                    {
                        CharmChangerMod.LS.stalwartShellInvulnerability = f;
                    },
                    () => CharmChangerMod.LS.stalwartShellInvulnerability,
                    0f,
                    100f,
                    true),

                new TextPanel("Inaction Time"),

                new CustomSlider(
                    "(hundreths)",
                    f =>
                    {
                        CharmChangerMod.LS.stalwartShellRecoil = f;
                    },
                    () => CharmChangerMod.LS.stalwartShellRecoil,
                    0f,
                    20f,
                    true),
                #endregion
                #region Baldur Shell Menu
                new TextPanel("Baldur Shell Options", 1000, 70),

                new CustomSlider(
                    "???",
                    f =>
                    {
                        CharmChangerMod.LS.stalwartShellInvulnerability = f;
                    },
                    () => CharmChangerMod.LS.stalwartShellInvulnerability,
                    0f,
                    25f,
                    false,
                    Id:"Baldur???"),

#endregion
            });

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
    #endregion
    public class CharmChangerMod : Mod, ICustomMenuMod, ILocalSettings<LocalSettings>
    {
        #region Biolerplate

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
