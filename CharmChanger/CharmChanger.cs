using GlobalEnums;
using Modding;
using System;
using HutongGames.PlayMaker.Actions;
using HutongGames.PlayMaker;
using UnityEngine;
using System.Collections;
using SFCore.Utils;

namespace CharmChanger
{
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
        public MenuScreen GetMenuScreen(MenuScreen modListMenu, ModToggleDelegates? _) => ModMenu.CreateMenuScreen(modListMenu);
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

            #region Grubsong Init
            On.HeroController.TakeDamage += OnHCTakeDamage;
            On.HutongGames.PlayMaker.Actions.CallMethodProper.OnEnter += OnCallMethodProperAction;
            #endregion
            #region Stalwart Shell Init
            On.HeroController.StartRecoil += OnHCStartRecoil;
            #endregion
            #region Baldur Shell Init
            On.BeginRecoil.OnEnter += OnBeginRecoilAction;
            On.PlayerData.MaxHealth += OnPDMaxHealth;
            #endregion
            #region Fury of the Fallen Init
            On.HutongGames.PlayMaker.Actions.IntCompare.OnEnter += OnIntCompareAction;
            On.HutongGames.PlayMaker.Actions.BoolAllTrue.OnEnter += OnBoolAllTrueAction;
            #endregion

            Log("Initialized");
        }
        #endregion

        #region Grubsong Changes
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
        #region Stalwart Shell Changes
        private IEnumerator OnHCStartRecoil(On.HeroController.orig_StartRecoil orig, HeroController self, CollisionSide impactSide, bool spawnDamageEffect, int damageAmount)
        {
            self.INVUL_TIME_STAL = LS.stalwartShellInvulnerability / 100f;
            self.RECOIL_DURATION_STAL = LS.stalwartShellRecoil / 100f;

            return orig(self, impactSide, spawnDamageEffect, damageAmount);
        }
        #endregion
        #region Baldur Shell Changes
        private void OnBeginRecoilAction(On.BeginRecoil.orig_OnEnter orig, BeginRecoil self)
        {
            if (self.Fsm.GameObject.name.Contains("Hit ") && self.Fsm.Name == "push_enemy" && self.State.Name == "Send Event")
            {
                self.attackMagnitude = LS.baldurShellKnockback;
            }
            orig(self);
        }
        private void OnPDMaxHealth(On.PlayerData.orig_MaxHealth orig, PlayerData self)
        {
            orig(self);

            self.blockerHits = LS.baldurShellBlocks;
            var BaldurShellFSM = HeroController.instance.gameObject.transform.Find("Charm Effects/Blocker Shield").gameObject.LocateMyFSM("Control");
            BaldurShellFSM.ChangeFsmTransition("HUD Icon Up", "FINISHED",
                (LS.baldurShellBlocks == 4) ? "Equipped" :
                (LS.baldurShellBlocks == 3) ? "HUD 3" :
                (LS.baldurShellBlocks == 2) ? "HUD 2" :
                (LS.baldurShellBlocks == 1) ? "HUD 1" :
                "Break");
        }
        #endregion
        #region Fury of the Fallen Changes
        private void OnIntCompareAction(On.HutongGames.PlayMaker.Actions.IntCompare.orig_OnEnter orig, IntCompare self)
        {
            if (self.Fsm.GameObject.name == "Charm Effects" && self.Fsm.Name == "Fury" && self.State.Name == "Check HP")
            {
                self.integer2 = LS.furyOfTheFallenHealth;
                self.lessThan = FsmEvent.GetFsmEvent("FURY");
            }

            orig(self);
        }
        private void OnBoolAllTrueAction(On.HutongGames.PlayMaker.Actions.BoolAllTrue.orig_OnEnter orig, BoolAllTrue self)
        {
            if (self.Fsm.GameObject.name == "Charm Effects" && self.Fsm.Name == "Fury" && self.State.Name == "Check HP")
            {
                self.sendEvent = (LS.furyOfTheFallenJonis) ? null : FsmEvent.GetFsmEvent("CANCEL");
            }

            orig(self);
        }

        #endregion
    }

    public class LocalSettings
    {
        #region Grubsong Settings
        [SliderIntElement("Grubsong Options", "Soul", 0, 100)]
        public int grubsongDamageSoul = 15;

        [SliderIntElement("Grubsong Options", "Grubberfly's Soul", 0, 100)]
        public int grubsongDamageSoulCombo = 25;

        [SliderIntElement("Grubsong Options", "Weaversong Song", 0, 100)]
        public int grubsongWeaversongSoul = 3;

        [ButtonElement("Grubsong Options", "Reset Defaults", "")]
        public void ResetGrubsong()
        {
            grubsongDamageSoul = 15;
            grubsongDamageSoulCombo = 25;
            grubsongWeaversongSoul = 3;
        }
        #endregion
        #region Stalwart Shell Settings
        [SliderIntElement("Stalwart Shell Options", "Invul Time (hundreths)", 0, 200)]
        public int stalwartShellInvulnerability = 175;

        [SliderIntElement("Stalwart Shell Options", "Recoil Time (hundreths)", 0, 100)]
        public int stalwartShellRecoil = 8;

        [ButtonElement("Stalwart Shell Options", "Reset Defaults", "")]
        public void ResetStalwartShell()
        {
            stalwartShellInvulnerability = 175;
            stalwartShellRecoil = 8;
        }

        #endregion
        #region Baldur Shell Settings
        [SliderFloatElement("Baldur Shell Options", "Enemy Knockback", 0f, 10f)]
        public float baldurShellKnockback = 2f;

        [SliderIntElement("Baldur Shell Options", "Blocks", 0, 4)]
        public int baldurShellBlocks = 4;

        [ButtonElement("Baldur Shell Options", "Reset Defaults", "")]
        public void ResetBaldurShell()
        {
            baldurShellKnockback = 2f;
            baldurShellBlocks = 4;
        }
        #endregion
        #region Fury of the Fallen Settings
        [SliderIntElement("Fury of the Fallen Options", "Maximum Health", 0, 13)]
        public int furyOfTheFallenHealth = 1;

        [BoolElement("Fury of the Fallen Options", "Works with Joni's", "")]
        public bool furyOfTheFallenJonis = false;

        [ButtonElement("Fury of the Fallen Options", "Reset Defaults", "")]
        public void ResetFuryOfTheFallen()
        {
            furyOfTheFallenHealth = 1;
            furyOfTheFallenJonis = false;
        }
        #endregion
    }
}
