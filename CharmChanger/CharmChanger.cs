using UnityEngine;
using Modding;
using System;
using System.Collections;
using System.Reflection;
using GlobalEnums;
using HutongGames.PlayMaker;
using HutongGames.PlayMaker.Actions;
using MonoMod.RuntimeDetour;
using MonoMod.Cil;
using Satchel;
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

        #region Custom Variables
        private bool joniBeamChecker = false;
        #endregion

        #region Init
        public override void Initialize()
        {
            Log("Initializing");

            #region General Init
            On.GameManager.CalculateNotchesUsed += ChangeNotchCosts;
            #endregion

            #region Grubsong Init
            On.HeroController.TakeDamage += GrubsongSoulChanges;
            #endregion
            #region Stalwart Shell Init
            On.HeroController.StartRecoil += StalwartShellChanges;
            #endregion
            #region Baldur Shell Init
            On.BeginRecoil.OnEnter += BaldurShellKnockback;
            On.PlayerData.MaxHealth += BaldurShellBlocks;
            #endregion
            #region Fury of the Fallen Init
            On.HutongGames.PlayMaker.Actions.IntCompare.OnEnter += FotFHPRequirements;
            On.HutongGames.PlayMaker.Actions.BoolAllTrue.OnEnter += FotFJonisRequirements;
            On.HutongGames.PlayMaker.Actions.SetFsmFloat.OnEnter += FotFNailScaling;
            On.HutongGames.PlayMaker.Actions.FloatMultiply.OnEnter += FotFNailArtScaling;
            ilHCAttackFotF = new ILHook(HCAttackFotF, FotFGrubberflysRequirements);
            #endregion
            #region Quick/Deep Focus Init
            On.HutongGames.PlayMaker.Actions.SetFloatValue.OnEnter += FocusSpeed;
            On.HutongGames.PlayMaker.Actions.SetIntValue.OnEnter += HealAmount;
            On.HutongGames.PlayMaker.Actions.FloatMultiply.OnEnter += DeepFocusScaling;
            #endregion
            #region Lifeblood Heart/Core Init
            ilorigUpdateBlueHealth = new ILHook(origUpdateBlueHealth, LifebloodChanges);
            #endregion
            #region Defender's Crest Init
            ilShopItemStatsEnable = new ILHook(shopItemStats, DefendersCrestCostReduction);
            On.HutongGames.PlayMaker.Actions.Wait.OnEnter += DungCloudSettings;
            On.HutongGames.PlayMaker.Actions.SpawnObjectFromGlobalPoolOverTime.OnUpdate += CloudFrequency;
            #endregion
            #region Flukenest Init
            On.HutongGames.PlayMaker.Actions.FlingObjectsFromGlobalPool.OnEnter += FlukeCount;
            On.HutongGames.PlayMaker.Actions.Wait.OnEnter += FlukenestDefendersCrestDurationAndDamage;
            ilflukenestEnable = new ILHook(flukenestEnable, FlukenestEnableHook);
            #endregion
            #region Thorns of Agony Init
            On.HutongGames.PlayMaker.Actions.SetFsmInt.OnEnter += ThornsOfAgonyDamageScale;
            #endregion
            #region Longnail / Mark of Pride Init
            On.PlayMakerFSM.OnEnable += OnFsmEnable;
            On.HutongGames.PlayMaker.Actions.SendMessage.OnEnter += WallSlashSizeScale;
            ilNailSlashStart = new ILHook(nailSlash, NailSlashSizeScale);
            #endregion
            #region Heavy Blow Init
            On.PlayMakerFSM.OnEnable += OnFsmEnableHeavyBlow;
            On.HutongGames.PlayMaker.Actions.SetFsmFloat.OnEnter += HeavyBlowKnockback;
            On.HutongGames.PlayMaker.Actions.IntOperator.OnEnter += HeavyBlowStagger;
            On.HutongGames.PlayMaker.Actions.IntCompare.OnEnter += HeavyBlowComboStaggerFix;
            #endregion
            #region Sharp Shadow Init
            On.HutongGames.PlayMaker.Actions.FloatMultiplyV2.OnEnter += SharpShadowDashMasterDamageScaling;
            On.HutongGames.PlayMaker.Actions.ConvertIntToFloat.OnEnter += SharpShadowBaseDamage;
            ilorigDashVector = new ILHook(origDashVector, SharpShadowSpeed);
            #endregion
            #region Spore Shroom Init
            On.HutongGames.PlayMaker.Actions.Wait.OnEnter += OnWaitAction2;
            On.HutongGames.PlayMaker.Actions.SetBoolValue.OnEnter += SporeShroomDamageReset;
            On.HutongGames.PlayMaker.Actions.ActivateGameObject.OnEnter += SporeShroomVisuals;
            #endregion
            #region Shaman Stone Init
            On.HutongGames.PlayMaker.Actions.SetScale.OnEnter += ShamanStoneVengefulSpiritScaling;
            On.HutongGames.PlayMaker.Actions.FloatMultiply.OnEnter += ShamanStoneShadeSoulScaling;
            On.HutongGames.PlayMaker.Actions.SetFsmInt.OnEnter += ShamanStoneDamage;
            #endregion
            #region Soul Catcher/Eater Init
            ilSoulGain = new ILHook(soulGain, SoulCharmChanges);
            #endregion
            #region Glowing Womb Init
            On.HutongGames.PlayMaker.Actions.Wait.OnEnter += GlowingWombSettings;
            On.HutongGames.PlayMaker.Actions.IntCompare.OnEnter += HatchlingSpawnRequirements;
            On.KnightHatchling.OnEnable += HatchlingDamage;
            ilHatchlingEnable = new ILHook(Hatchling, HatchlingFotFSettings);
            #endregion
            #region Fragile Charms Init
            On.HutongGames.PlayMaker.Actions.PlayerDataBoolTest.OnEnter += FragileCharmsBreak;
            On.HutongGames.PlayMaker.Actions.FloatMultiply.OnEnter += StrengthDamageIncrease;
            ilHMDie = new ILHook(HMDie, GreedGeoIncrease);
            #endregion
            #region Nailmaster's Glory Init
            On.HeroController.CharmUpdate += NailArtChargeTime;
            #endregion
            #region Joni's Blessing Init
            ilOrigCharmUpdate = new ILHook(origCharmUpdate, JonisScaling);
            #endregion
            #region Shape Of Unn Init
            On.HutongGames.PlayMaker.Actions.SetFloatValue.OnEnter += SlugSpeeds;
            #endregion
            #region Hiveblood Init
            On.HutongGames.PlayMaker.Actions.FloatCompare.OnEnter += HivebloodTimers;
            #endregion
            #region Dream Wielder Init
            ilEnemyDreamNailed = new ILHook(EnemyDreamNailed, DreamWielderSoul);
            ilEmitEssence = new ILHook(EmitEssence, DreamWielderEssence);
            #endregion
            #region Dashmaster Init
            ilHeroDash = new ILHook(HeroDash, DashmasterChanges);
            #endregion
            #region Quick Slash Init
            ilHCAttackQuickSlash = new ILHook(HCAttackQuickSlash, QuickSlashAttackDuration);
            ilOrigDoAttack = new ILHook(origDoAttack, QuickSlashAttackCooldown);
            #endregion
            #region Spell Twister Init
            On.HutongGames.PlayMaker.Actions.SetFsmInt.OnEnter += SpellTwisterSpellCost;
            #endregion
            #region Grubberfly's Elegy Init
            On.HeroController.TakeDamage += GrubberflysElegyJoniBeam;
            On.HutongGames.PlayMaker.Actions.FloatMultiply.OnEnter += GrubberflysElegyFotFScaling;
            On.HutongGames.PlayMaker.Actions.FloatOperator.OnEnter += GrubberflysElegyDamage;
            ilHCAttackElegy = new ILHook(HCAttackElegy, GrubberflysSizeScale);
            #endregion
            #region Kingsoul Init
            On.HutongGames.PlayMaker.Actions.Wait.OnEnter += KingsoulTimer;
            On.HutongGames.PlayMaker.Actions.SendMessageV2.DoSendMessage += KingsoulSoul;
            #endregion
            #region Sprintmaster Init
            ilHCMove = new ILHook(HCMove, SprintmasterSpeed);
            #endregion
            #region Dreamshield Init
            On.HutongGames.PlayMaker.Actions.AudioPlayerOneShotSingle.OnEnter += DreamshieldAudio;
            On.HutongGames.PlayMaker.Actions.Wait.OnEnter += DreamshieldReformationTime;
            On.HutongGames.PlayMaker.Actions.GetPlayerDataInt.OnEnter += DreamshieldDamage;
            On.HutongGames.PlayMaker.Actions.SetFsmFloat.OnEnter += DreamshieldSpeeds;
            On.HutongGames.PlayMaker.Actions.SetScale.OnEnter += DreamshieldSizes;
            #endregion
            #region Weaversong Init
            On.HutongGames.PlayMaker.Actions.CallMethodProper.OnEnter += WeaversongDamageAndGrubsongSoul;
            On.HutongGames.PlayMaker.Actions.RandomFloat.OnEnter += WeaversongSpeeds;
            On.HutongGames.PlayMaker.Actions.SetFloatValue.OnEnter += WeaversongSprintmasterSpeed;
            On.PlayMakerFSM.OnEnable += WeaverlingSpawner;
            #endregion
            #region Grimmchild Init
            On.HutongGames.PlayMaker.Actions.SetFloatValue.OnEnter += GrimmchildAttackTimer;
            On.HutongGames.PlayMaker.Actions.RandomFloat.OnEnter += GrimmchildAttackTimer2;
            On.HutongGames.PlayMaker.Actions.SetIntValue.OnEnter += GrimmchildDamage;
            #endregion
            #region Carefree Melody Init
            ilHCTakeDamage = new ILHook(HCTakeDamage, CarefreeMelodyChances);
            #endregion

            Log("Initialized");
        }
        #endregion

        #region Fury of the Fallen IL Hooks
        private static readonly MethodInfo HCAttackFotF = typeof(HeroController).GetMethod("Attack", BindingFlags.Public | BindingFlags.Instance);
        private ILHook? ilHCAttackFotF;
        #endregion
        #region Lifeblood Heart/Core IL Hooks
        private static readonly MethodInfo origUpdateBlueHealth = typeof(PlayerData).GetMethod("orig_UpdateBlueHealth", BindingFlags.Public | BindingFlags.Instance);
        private ILHook? ilorigUpdateBlueHealth;
        #endregion
        #region Defender's Crest IL Hooks
        private static readonly MethodInfo shopItemStats = typeof(ShopItemStats).GetMethod("OnEnable", BindingFlags.NonPublic | BindingFlags.Instance);
        private ILHook? ilShopItemStatsEnable;
        #endregion
        #region Flukenest IL Hooks
        private static readonly MethodInfo flukenestEnable = typeof(SpellFluke).GetMethod("OnEnable", BindingFlags.NonPublic | BindingFlags.Instance);
        private ILHook? ilflukenestEnable;
        #endregion
        #region Longnail / Mark of Pride IL Hooks
        private static readonly MethodInfo nailSlash = typeof(NailSlash).GetMethod("StartSlash", BindingFlags.Public | BindingFlags.Instance);
        private ILHook? ilNailSlashStart;
        #endregion
        #region Sharp Shadow IL Hooks
        private static readonly MethodInfo origDashVector = typeof(HeroController).GetMethod("OrigDashVector", BindingFlags.NonPublic | BindingFlags.Instance);
        private ILHook? ilorigDashVector;
        #endregion
        #region Soul Catcher/Eater IL Hooks
        private static readonly MethodInfo soulGain = typeof(HeroController).GetMethod("SoulGain", BindingFlags.Public | BindingFlags.Instance);
        private ILHook? ilSoulGain;
        #endregion
        #region Glowing Womb IL Hooks
        private static readonly MethodInfo Hatchling = typeof(KnightHatchling).GetMethod("OnEnable", BindingFlags.NonPublic | BindingFlags.Instance);
        private ILHook? ilHatchlingEnable;
        #endregion
        #region Fragile Charms IL Hooks
        private static readonly MethodInfo HMDie = typeof(HealthManager).GetMethod("Die", BindingFlags.Public | BindingFlags.Instance);
        private ILHook? ilHMDie;
        #endregion
        #region Joni's Blessing IL Hooks
        private static readonly MethodInfo origCharmUpdate = typeof(HeroController).GetMethod("orig_CharmUpdate", BindingFlags.Public | BindingFlags.Instance);
        private ILHook? ilOrigCharmUpdate;
        #endregion
        #region Dream Wielder IL Hooks
        private static readonly MethodInfo EnemyDreamNailed = typeof(EnemyDreamnailReaction).GetMethod("RecieveDreamImpact", BindingFlags.Public | BindingFlags.Instance);
        private ILHook? ilEnemyDreamNailed;

        private static readonly MethodInfo EmitEssence = typeof(EnemyDeathEffects).GetMethod("EmitEssence", BindingFlags.NonPublic | BindingFlags.Instance);
        private ILHook? ilEmitEssence;
        #endregion
        #region Dashmaster IL Hooks
        private static readonly MethodInfo HeroDash = typeof(HeroController).GetMethod("HeroDash", BindingFlags.NonPublic | BindingFlags.Instance);
        private ILHook? ilHeroDash;
        #endregion
        #region Quick Slash IL Hooks
        private static readonly MethodInfo HCAttackQuickSlash = typeof(HeroController).GetMethod("Attack", BindingFlags.Public | BindingFlags.Instance);
        private ILHook? ilHCAttackQuickSlash;

        private static readonly MethodInfo origDoAttack = typeof(HeroController).GetMethod("orig_DoAttack", BindingFlags.NonPublic | BindingFlags.Instance);
        private ILHook? ilOrigDoAttack;
        #endregion
        #region Grubberfly's Elegy IL Hooks
        private static readonly MethodInfo HCAttackElegy = typeof(HeroController).GetMethod("Attack", BindingFlags.Public | BindingFlags.Instance);
        private ILHook? ilHCAttackElegy;
        #endregion
        #region Sprintmaster IL Hooks
        private static readonly MethodInfo HCMove = typeof(HeroController).GetMethod("Move", BindingFlags.NonPublic | BindingFlags.Instance);
        private ILHook? ilHCMove;
        #endregion
        #region Carefree Melody IL Hooks
        private static readonly MethodInfo HCTakeDamage = typeof(HeroController).GetMethod("TakeDamage", BindingFlags.Public | BindingFlags.Instance);
        private ILHook? ilHCTakeDamage;
        #endregion

        #region Notch Cost Changes
        private void ChangeNotchCosts(On.GameManager.orig_CalculateNotchesUsed orig, GameManager self)
        {
            PlayerData.instance.SetInt("charmCost_1", LS.charm1NotchCost);
            PlayerData.instance.SetInt("charmCost_2", LS.charm2NotchCost);
            PlayerData.instance.SetInt("charmCost_3", LS.charm3NotchCost);
            PlayerData.instance.SetInt("charmCost_4", LS.charm4NotchCost);
            PlayerData.instance.SetInt("charmCost_5", LS.charm5NotchCost);
            PlayerData.instance.SetInt("charmCost_6", LS.charm6NotchCost);
            PlayerData.instance.SetInt("charmCost_7", LS.charm7NotchCost);
            PlayerData.instance.SetInt("charmCost_8", LS.charm8NotchCost);
            PlayerData.instance.SetInt("charmCost_9", LS.charm9NotchCost);
            PlayerData.instance.SetInt("charmCost_10", LS.charm10NotchCost);
            PlayerData.instance.SetInt("charmCost_11", LS.charm11NotchCost);
            PlayerData.instance.SetInt("charmCost_12", LS.charm12NotchCost);
            PlayerData.instance.SetInt("charmCost_13", LS.charm13NotchCost);
            PlayerData.instance.SetInt("charmCost_14", LS.charm14NotchCost);
            PlayerData.instance.SetInt("charmCost_15", LS.charm15NotchCost);
            PlayerData.instance.SetInt("charmCost_16", LS.charm16NotchCost);
            PlayerData.instance.SetInt("charmCost_17", LS.charm17NotchCost);
            PlayerData.instance.SetInt("charmCost_18", LS.charm18NotchCost);
            PlayerData.instance.SetInt("charmCost_19", LS.charm19NotchCost);
            PlayerData.instance.SetInt("charmCost_20", LS.charm20NotchCost);
            PlayerData.instance.SetInt("charmCost_21", LS.charm21NotchCost);
            PlayerData.instance.SetInt("charmCost_22", LS.charm22NotchCost);
            PlayerData.instance.SetInt("charmCost_23", LS.charm23NotchCost);
            PlayerData.instance.SetInt("charmCost_24", LS.charm24NotchCost);
            PlayerData.instance.SetInt("charmCost_25", LS.charm25NotchCost);
            PlayerData.instance.SetInt("charmCost_26", LS.charm26NotchCost);
            PlayerData.instance.SetInt("charmCost_27", LS.charm27NotchCost);
            PlayerData.instance.SetInt("charmCost_28", LS.charm28NotchCost);
            PlayerData.instance.SetInt("charmCost_29", LS.charm29NotchCost);
            PlayerData.instance.SetInt("charmCost_30", LS.charm30NotchCost);
            PlayerData.instance.SetInt("charmCost_31", LS.charm31NotchCost);
            PlayerData.instance.SetInt("charmCost_32", LS.charm32NotchCost);
            PlayerData.instance.SetInt("charmCost_33", LS.charm33NotchCost);
            PlayerData.instance.SetInt("charmCost_34", LS.charm34NotchCost);
            PlayerData.instance.SetInt("charmCost_35", LS.charm35NotchCost);
            PlayerData.instance.SetInt("charmCost_36", LS.charm36NotchCost);
            PlayerData.instance.SetInt("charmCost_37", LS.charm37NotchCost);
            PlayerData.instance.SetInt("charmCost_38", LS.charm38NotchCost);
            PlayerData.instance.SetInt("charmCost_39", LS.charm39NotchCost);
            PlayerData.instance.SetInt("charmCost_40", LS.charm40NotchCost);
        }
        #endregion

        #region Grubsong Changes
        private void GrubsongSoulChanges(On.HeroController.orig_TakeDamage orig, HeroController self, GameObject go, CollisionSide damageSide, int damageAmount, int hazardType)
        {
            // Grubsong
            self.GRUB_SOUL_MP = LS.grubsongDamageSoul;

            // Grubsong + Grubberfly's
            self.GRUB_SOUL_MP_COMBO = LS.grubsongDamageSoulCombo;

            orig(self, go, damageSide, damageAmount, hazardType);
        }
        #endregion
        #region Stalwart Shell Changes
        // Invulnerability time and inaction time
        private IEnumerator StalwartShellChanges(On.HeroController.orig_StartRecoil orig, HeroController self, CollisionSide impactSide, bool spawnDamageEffect, int damageAmount)
        {
            self.INVUL_TIME_STAL = LS.stalwartShellInvulnerability;
            self.RECOIL_DURATION_STAL = LS.stalwartShellRecoil;

            return orig(self, impactSide, spawnDamageEffect, damageAmount);
        }
        #endregion
        #region Baldur Shell Changes
        private void BaldurShellKnockback(On.BeginRecoil.orig_OnEnter orig, BeginRecoil self)
        {
            if (self.Fsm.GameObject.name.Contains("Hit ") && self.Fsm.Name == "push_enemy" && self.State.Name == "Send Event")
            {
                self.attackMagnitude = 2f * LS.baldurShellKnockbackMult;
            }
            orig(self);
        }
        private void BaldurShellBlocks(On.PlayerData.orig_MaxHealth orig, PlayerData self)
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
        private void FotFHPRequirements(On.HutongGames.PlayMaker.Actions.IntCompare.orig_OnEnter orig, IntCompare self)
        {
            if (self.Fsm.GameObject.name == "Charm Effects" && self.Fsm.Name == "Fury")
            {
                if (self.State.Name == "Check HP")
                {
                    self.integer2 = LS.furyOfTheFallenHealth;
                    self.lessThan = FsmEvent.GetFsmEvent("FURY");
                }
                else if (self.State.Name == "Recheck")
                {
                    self.integer2 = LS.furyOfTheFallenHealth;
                    self.lessThan = FsmEvent.GetFsmEvent("RETURN");
                }
            }

            orig(self);
        }

        // Joni's Requirement
        private void FotFJonisRequirements(On.HutongGames.PlayMaker.Actions.BoolAllTrue.orig_OnEnter orig, BoolAllTrue self)
        {
            if (self.Fsm.GameObject.name == "Charm Effects" && self.Fsm.Name == "Fury" && self.State.Name == "Check HP")
            {
                self.sendEvent = LS.furyOfTheFallenJonis ? null : FsmEvent.GetFsmEvent("CANCEL");
            }

            orig(self);
        }

        // Scaling (regular attacks)
        private void FotFNailScaling(On.HutongGames.PlayMaker.Actions.SetFsmFloat.orig_OnEnter orig, SetFsmFloat self)
        {
            if (self.Fsm.GameObject.name == "Charm Effects" && self.Fsm.Name == "Fury" && self.State.Name == "Activate")
            {
                self.setValue.Value = 1f + (float)(LS.furyOfTheFallenScaling / 100f);
            }

            orig(self);
        }

        private void FotFNailArtScaling(On.HutongGames.PlayMaker.Actions.FloatMultiply.orig_OnEnter orig, FloatMultiply self)
        {
            if (self.Fsm.Name == "nailart_damage" && self.State.Name == "Fury?")
            {
                self.multiplyBy.Value = 1f + (float)(LS.furyOfTheFallenScaling / 100f);
            }

            orig(self);
        }

        // Grubberfly's HP Requirement
        private void FotFGrubberflysRequirements(ILContext il)
        {
            ILCursor cursor = new ILCursor(il).Goto(0);

            while (cursor.TryGotoNext
                (
                i => i.MatchLdstr("health"),
                i => i.MatchCallvirt<PlayerData>("GetInt"),
                i => i.MatchLdcI4(1)
                ))
            {
                cursor.GotoNext();
                cursor.GotoNext();
                cursor.EmitDelegate<Func<int, int>>(health => (PlayerData.instance.health <= LS.furyOfTheFallenHealth) ? 1 : 0);
            }
        }
       #endregion
        #region Quick/Deep Focus Changes
        private void FocusSpeed(On.HutongGames.PlayMaker.Actions.SetFloatValue.orig_OnEnter orig, SetFloatValue self)
        {
            if (self.Fsm.GameObject.name == "Knight" && self.Fsm.Name == "Spell Control")
            {
                if (self.State.Name == "Set Focus Speed" && self.floatValue.Name == "Time Per MP Drain CH")
                {
                    self.floatValue.Value = LS.quickFocusFocusTime / 33f;
                }
            }

            orig(self);
        }

        private void HealAmount(On.HutongGames.PlayMaker.Actions.SetIntValue.orig_OnEnter orig, SetIntValue self)
        {
            if (self.Fsm.GameObject.name == "Knight" && self.Fsm.Name == "Spell Control" && self.State.Name.Contains("Set HP Amount") && self.State.ActiveActionIndex == 2)
            {
                self.intValue.Value = LS.deepFocusHealing;
            }

            orig(self);
        }
        private void DeepFocusScaling(On.HutongGames.PlayMaker.Actions.FloatMultiply.orig_OnEnter orig, FloatMultiply self)
        {
            if (self.Fsm.GameObject.name == "Knight" && self.Fsm.Name == "Spell Control" && self.State.Name == "Deep Focus Speed")
            {
                self.multiplyBy.Value = 1f + (float)(LS.deepFocusHealingTimeScale / 100f);
            }

            orig(self);
        }

        #endregion
        #region Lifeblood Heart/Core Changes
        // Lifeblood Granted
        private void LifebloodChanges(ILContext il)
        {
            ILCursor cursor = new ILCursor(il).Goto(0);

            cursor.TryGotoNext(i => i.MatchLdcI4(2));
            cursor.GotoNext();
            cursor.EmitDelegate<Func<int, int>>(Lifeblood => LS.lifebloodHeartLifeblood);

            cursor.TryGotoNext(i => i.MatchLdcI4(4));
            cursor.GotoNext();
            cursor.EmitDelegate<Func<int, int>>(Lifeblood => LS.lifebloodCoreLifeblood);
        }
        #endregion
        #region Defender's Crest Changes
        // Discount
        private void DefendersCrestCostReduction(ILContext il)
        {
            ILCursor cursor = new ILCursor(il).Goto(0);

            cursor.TryGotoNext(i => i.MatchLdcR4(0.8f));
            cursor.GotoNext();
            cursor.EmitDelegate<Func<float, float>>(Discount => 100f - ((float)(LS.defendersCrestDiscount / 100f)));
        }

        private void DungCloudSettings(On.HutongGames.PlayMaker.Actions.Wait.orig_OnEnter orig, Wait self)
        {
            if (self.Fsm.GameObject.name == "Knight Dung Trail(Clone)" && self.Fsm.Name == "Control" && self.State.Name == "Wait")
            {
                // Duration & Visuals
                //self.time.Value = LS.defendersCrestCloudDuration;
                //self.Fsm.GameObject.transform.Find("Pt Normal").GetComponent<ParticleSystem>().startLifetime = LS.defendersCrestCloudDuration;

                // Damage Rate
                self.Fsm.GameObject.GetComponent<DamageEffectTicker>().damageInterval = LS.defendersCrestDamageRate;
            }

            orig(self);
        }
        private void CloudFrequency(On.HutongGames.PlayMaker.Actions.SpawnObjectFromGlobalPoolOverTime.orig_OnUpdate orig, SpawnObjectFromGlobalPoolOverTime self)
        {
            if (self.Fsm.GameObject.name == "Dung" && self.Fsm.Name == "Control" && self.State.Name == "Equipped")
            {
                self.frequency.Value = LS.defendersCrestCloudFrequency;
            }

            orig(self);
        }
        #endregion
        #region Flukenest Changes
        // Flukes Total
        private void FlukeCount(On.HutongGames.PlayMaker.Actions.FlingObjectsFromGlobalPool.orig_OnEnter orig, FlingObjectsFromGlobalPool self)
        {
            // Vengeful Spirit
            if(self.Fsm.GameObject.name == "Fireball Top(Clone)" && self.Fsm.Name == "Fireball Cast" && self.State.Name == "Flukes")
            {
                self.spawnMin.Value = LS.flukenestVSFlukes;
                self.spawnMax.Value = LS.flukenestVSFlukes;
            }

            // Shade Soul
            else if (self.Fsm.GameObject.name == "Fireball2 Top(Clone)" && self.Fsm.Name == "Fireball Cast" && self.State.Name == "Flukes")
            {
                self.spawnMin.Value = LS.flukenestSSFlukes;
                self.spawnMax.Value = LS.flukenestSSFlukes;
            }

            orig(self);
        }

        private void FlukenestDefendersCrestDurationAndDamage(On.HutongGames.PlayMaker.Actions.Wait.orig_OnEnter orig, Wait self)
        {
            if (self.Fsm.GameObject.name == "Knight Dung Cloud" && self.Fsm.Name == "Control" && self.State.Name == "Collider On")
            {
                // Duration & Visuals
                //self.time.Value = LS.flukenestDefendersCrestDuration;
                //self.Fsm.GameObject.transform.Find("Pt Normal").GetComponent<ParticleSystem>().startLifetime = LS.flukenestDefendersCrestDuration;

                // Damage Rates
                self.Fsm.GameObject.GetComponent<DamageEffectTicker>().SetDamageInterval(PlayerData.instance.equippedCharm_19 ? LS.flukenestDefendersCrestShamanStoneDamageRate : LS.flukenestDefendersCrestDamageRate);
            }

            orig(self);
        }

        private void FlukenestEnableHook(ILContext il)
        {
            ILCursor cursor = new ILCursor(il).Goto(0);

            // Shaman Stone Sizes
            cursor.TryGotoNext(i => i.MatchLdcR4(0.9f));
            cursor.GotoNext();
            cursor.EmitDelegate<Func<float, float>>(minimum => LS.flukenestShamanStoneFlukeSizeMin);

            cursor.TryGotoNext(i => i.MatchLdcR4(1.2f));
            cursor.GotoNext();
            cursor.EmitDelegate<Func<float, float>>(maximum => LS.flukenestShamanStoneFlukeSizeMax);

            // Shaman Stone Damage
            cursor.TryGotoNext(i => i.MatchLdcI4(5));
            cursor.GotoNext();
            cursor.EmitDelegate<Func<int, int>>(damage => LS.flukenestShamanStoneDamage);

            // Regular Sizes
            cursor.TryGotoNext(i => i.MatchLdcR4(0.7f));
            cursor.GotoNext();
            cursor.EmitDelegate<Func<float, float>>(minimum => LS.flukenestFlukeSizeMin);

            cursor.TryGotoNext(i => i.MatchLdcR4(0.9f));
            cursor.GotoNext();
            cursor.EmitDelegate<Func<float, float>>(maximum => LS.flukenestFlukeSizeMax);

            // Regular Damage
            cursor.TryGotoNext(i => i.MatchLdcI4(4));
            cursor.GotoNext();
            cursor.EmitDelegate<Func<int, int>>(damage => LS.flukenestDamage);
        }
        #endregion
        #region Thorns of Agony Changes
        private void ThornsOfAgonyDamageScale(On.HutongGames.PlayMaker.Actions.SetFsmInt.orig_OnEnter orig, SetFsmInt self)
        {
            if (self.Fsm.GameObject.name.Contains("Hit ") && self.Fsm.Name == "set_thorn_damage" && self.State.Name == "Set")
            {
                self.setValue.Value = (int)(self.setValue.Value * LS.thornsOfAgonyDamageMultiplier);
            }

            orig(self);
        }
        #endregion
        #region Longnail / Mark of Pride Changes
        private void NailSlashSizeScale(ILContext il)
        {
            ILCursor cursor = new ILCursor(il).Goto(0);

            // Longnail + Mark of Pride Scaling
            while (cursor.TryGotoNext(i => i.MatchLdcR4(1.4f)))
            {
                cursor.GotoNext();
                cursor.EmitDelegate<Func<float, float>>(scale => (float)(1f + (LS.longnailMarkOfPrideScale / 100f)));
            }

            // Mark of Pride Scaling
            while (cursor.TryGotoNext(i => i.MatchLdcR4(1.25f)))
            {
                cursor.GotoNext();
                cursor.EmitDelegate<Func<float, float>>(scale => (float)(1f + (LS.markOfPrideScale / 100f)));
            }

            // Longnail Scaling
            while (cursor.TryGotoNext(i => i.MatchLdcR4(1.15f)))
            {
                cursor.GotoNext();
                cursor.EmitDelegate<Func<float, float>>(scale => (float)(1f + (LS.longnailScale / 100f)));
            }
        }

        // Wall Slash Application
        private void OnFsmEnable(On.PlayMakerFSM.orig_OnEnable orig, PlayMakerFSM self)
        {
            orig(self);

            if (self.gameObject.name == "Charm Effects" && self.FsmName == "Slash Size Modifiers")
            {
                self.AddFsmAction("Init", new FindChild()
                {
                    gameObject = new FsmOwnerDefault()
                    {
                        OwnerOption = OwnerDefaultOption.SpecifyGameObject,
                        GameObject = self.GetFsmGameObjectVariable("Attacks")
                    },
                    childName = "WallSlash",
                    storeResult = self.GetFsmGameObjectVariable("Wall Slash")
                });

                self.AddFsmAction("Equipped 2", new SendMessage()
                {
                    gameObject = new FsmOwnerDefault()
                    {
                        OwnerOption = OwnerDefaultOption.SpecifyGameObject,
                        GameObject = self.GetGameObjectVariable("Wall Slash")
                    },
                    delivery = 0,
                    options = SendMessageOptions.DontRequireReceiver,
                    functionCall = new FunctionCall()
                    {
                        FunctionName = "SetLongnail",
                        ParameterType = "bool",
                        BoolParameter = new FsmBool(true)
                    }
                });

                self.AddFsmAction("Unequipped 2", new SendMessage()
                {
                    gameObject = new FsmOwnerDefault()
                    {
                        OwnerOption = OwnerDefaultOption.SpecifyGameObject,
                        GameObject = self.GetGameObjectVariable("Wall Slash")
                    },
                    delivery = 0,
                    options = SendMessageOptions.DontRequireReceiver,
                    functionCall = new FunctionCall()
                    {
                        FunctionName = "SetLongnail",
                        ParameterType = "bool",
                        BoolParameter = new FsmBool(false)
                    }
                });

                self.AddFsmAction("Equipped", new SendMessage()
                {
                    gameObject = new FsmOwnerDefault()
                    {
                        OwnerOption = OwnerDefaultOption.SpecifyGameObject,
                        GameObject = self.GetGameObjectVariable("Wall Slash")
                    },
                    delivery = 0,
                    options = SendMessageOptions.DontRequireReceiver,
                    functionCall = new FunctionCall()
                    {
                        FunctionName = "SetMantis",
                        ParameterType = "bool",
                        BoolParameter = new FsmBool(true)
                    }
                });

                self.AddFsmAction("Unequipped", new SendMessage()
                {
                    gameObject = new FsmOwnerDefault()
                    {
                        OwnerOption = OwnerDefaultOption.SpecifyGameObject,
                        GameObject = self.GetGameObjectVariable("Wall Slash")
                    },
                    delivery = 0,
                    options = SendMessageOptions.DontRequireReceiver,
                    functionCall = new FunctionCall()
                    {
                        FunctionName = "SetMantis",
                        ParameterType = "bool",
                        BoolParameter = new FsmBool(false)
                    }
                });
            }
        }
        private void WallSlashSizeScale(On.HutongGames.PlayMaker.Actions.SendMessage.orig_OnEnter orig, SendMessage self)
        {
            if (self.Fsm.GameObject.name == "Charm Effects" && self.Fsm.Name == "Slash Size Modifiers" && self.gameObject.GameObject.Name == "Wall Slash")
            {
                if (self.State.Name == "Equipped 2" || self.State.Name == "Equipped")
                {
                    self.functionCall.BoolParameter.Value = LS.longnailMarkOfPrideWallSlash;
                }
            }

            orig(self);
        }
        #endregion
        #region Heavy Blow Changes
        // Wall Slash
        private void OnFsmEnableHeavyBlow(On.PlayMakerFSM.orig_OnEnable orig, PlayMakerFSM self)
        {
            orig(self);

            if (self.gameObject.name == "Charm Effects" && self.FsmName == "Enemy Recoil Up")
            {
                self.AddFsmAction("Init", new FindChild()
                {
                    gameObject = new FsmOwnerDefault()
                    {
                        OwnerOption = OwnerDefaultOption.SpecifyGameObject,
                        GameObject = self.GetFsmGameObjectVariable("Attacks")
                    },
                    childName = "WallSlash",
                    storeResult = self.GetFsmGameObjectVariable("Wall Slash")
                });

                self.AddFsmAction("Equipped", new SetFsmFloat()
                {
                    gameObject = new FsmOwnerDefault()
                    {
                        OwnerOption = OwnerDefaultOption.SpecifyGameObject,
                        GameObject = self.GetGameObjectVariable("Wall Slash")
                    },
                    fsmName = "damages_enemy",
                    variableName = "magnitudeMult",
                    setValue = 1.75f,
                    everyFrame = false
                });

                self.AddFsmAction("Unequipped", new SetFsmFloat()
                {
                    gameObject = new FsmOwnerDefault()
                    {
                        OwnerOption = OwnerDefaultOption.SpecifyGameObject,
                        GameObject = self.GetGameObjectVariable("Wall Slash")
                    },
                    fsmName = "damages_enemy",
                    variableName = "magnitudeMult",
                    setValue = 1f,
                    everyFrame = false
                });
            }
        }
        private void HeavyBlowKnockback(On.HutongGames.PlayMaker.Actions.SetFsmFloat.orig_OnEnter orig, SetFsmFloat self)
        {
            if (self.Fsm.GameObject.name == "Charm Effects" && self.Fsm.Name == "Enemy Recoil Up" && self.State.Name == "Equipped")
            {
                // Wall Slash
                if (self.gameObject.GameObject.Name == "Wall Slash")
                {
                    self.setValue.Value = (LS.heavyBlowWallSlash) ? 1f + (float)(LS.heavyBlowSlashRecoil / 100f) : 1f;
                }

                // Regular Slashes
                else if (!self.gameObject.GameObject.Name.Contains("Great") && !self.gameObject.GameObject.Name.Contains("Hit "))
                {
                    self.setValue.Value = 1f + (float)(LS.heavyBlowSlashRecoil / 100f);
                }

                // Great Slash
                else
                {
                    self.setValue.Value = 1f + (float)(LS.heavyBlowGreatSlashRecoil / 100f);
                }
            }

            // Cyclone Slash
            else if (self.Fsm.GameObject.name == "Cyclone Slash" && self.Fsm.Name == "Control Collider")
            {
                self.setValue.Value = (LS.heavyBlowCycloneSlash && PlayerData.instance.equippedCharm_15) ? 1f + (float)(LS.heavyBlowCycloneSlashRecoil / 100f) : 1f;
            }

            orig(self);
        }

        private void HeavyBlowStagger(On.HutongGames.PlayMaker.Actions.IntOperator.orig_OnEnter orig, IntOperator self)
        {
            if ((self.Fsm.Name == "Stun" || self.Fsm.Name == "Stun Control") && self.State.Name == "Heavy Blow")
            {
                if (self.integer1.Name == "Stun Hit Max")
                {
                    self.integer2.Value = LS.heavyBlowStagger;
                }

                else
                {
                    self.integer2.Value = LS.heavyBlowStaggerCombo;
                }
            }

            orig(self);
        }

        private void HeavyBlowComboStaggerFix(On.HutongGames.PlayMaker.Actions.IntCompare.orig_OnEnter orig, IntCompare self)
        {
            if ((self.Fsm.Name == "Stun" || self.Fsm.Name == "Stun Control") && self.State.Name == "In Combo")
            {
                self.greaterThan = FsmEvent.GetFsmEvent("STUN");
            }

            orig(self);
        }
        #endregion
        #region Sharp Shadow Changes
        private void SharpShadowBaseDamage(On.HutongGames.PlayMaker.Actions.ConvertIntToFloat.orig_OnEnter orig, ConvertIntToFloat self)
        {
            orig(self);

            if (self.Fsm.GameObject.name == "Attacks" && self.Fsm.Name == "Set Sharp Shadow Damage" && self.State.Name == "Check")
            {
                self.Fsm.GetFsmFloat("Nail Damage Float").Value *= LS.SharpShadowDamageMultiplier;
            }
        }

        private void SharpShadowDashMasterDamageScaling(On.HutongGames.PlayMaker.Actions.FloatMultiplyV2.orig_OnEnter orig, FloatMultiplyV2 self)
        {
            if(self.Fsm.GameObject.name == "Attacks" && self.Fsm.Name == "Set Sharp Shadow Damage" && self.State.Name == "Master")
            {
                self.multiplyBy.Value = 1f + (float)(LS.SharpShadowDashmasterDamageIncrease / 100f);
            }

            orig(self);
        }
        private void SharpShadowSpeed(ILContext il)
        {
            ILCursor cursor = new ILCursor(il).Goto(0);

            cursor.TryGotoNext(i => i.MatchLdfld<HeroController>("DASH_SPEED_SHARP"));
            cursor.GotoNext();
            cursor.EmitDelegate<Func<float, float>>(Speed => LS.SharpShadowDashSpeed);
        }
        #endregion
        #region Spore Shroom Changes
        private void OnWaitAction2(On.HutongGames.PlayMaker.Actions.Wait.orig_OnEnter orig, Wait self)
        {
            // Spore Shroom Cloud
            if (self.Fsm.GameObject.name == "Knight Spore Cloud(Clone)" && self.Fsm.Name == "Control" && self.State.Name == "Wait")
            {
                // Duration
                self.time.Value = LS.sporeShroomCloudDuration;

                // Damage Rate
                self.Fsm.GameObject.GetComponent<DamageEffectTicker>().damageInterval = LS.sporeShroomDamageRate;
            }

            // Spore Shroom + Defender's Crest Cloud
            else if (self.Fsm.GameObject.name == "Knight Dung Cloud(Clone)" && self.Fsm.Name == "Control" && self.State.Name == "Wait")
            {
                // Duration
                //self.time.Value = LS.sporeShroomDefendersCrestCloudDuration;

                // Damage Rate
                self.Fsm.GameObject.GetComponent<DamageEffectTicker>().damageInterval = LS.sporeShroomDefendersCrestDamageRate;
            }

            // Spore Shroom Cooldown
            else if (self.Fsm.GameObject.name == "Knight" && self.Fsm.Name == "Spore Cooldown" && self.State.Name == "Cooldown")
            {
                self.time.Value = LS.sporeShroomCooldown;
            }

            orig(self);
        }
        private void SporeShroomDamageReset(On.HutongGames.PlayMaker.Actions.SetBoolValue.orig_OnEnter orig, SetBoolValue self)
        {
            if (self.Fsm.GameObject.name == "Knight" && self.Fsm.Name == "Spell Control" && self.State.Name == "Cancel All" && self.boolVariable.Name == "Spore Cooldown")
            {
                self.boolValue.Value = self.boolVariable.Value && !LS.sporeShroomDamageResetsCooldown;
            }

            orig(self);
        }
        private void SporeShroomVisuals(On.HutongGames.PlayMaker.Actions.ActivateGameObject.orig_OnEnter orig, ActivateGameObject self)
        {
            orig(self);

            if (self.Fsm.GameObject.name == "Knight Spore Cloud(Clone)" && self.Fsm.Name == "Control")
            {
                if (self.State.Name == "Normal")
                {
                    self.Fsm.GameObject.transform.Find("Pt Normal").gameObject.GetComponent<ParticleSystem>().startLifetime = LS.sporeShroomCloudDuration;
                }

                else
                {
                    self.Fsm.GameObject.transform.Find("Pt Deep").gameObject.GetComponent<ParticleSystem>().startLifetime = LS.sporeShroomCloudDuration;
                }
            }
        }
        #endregion
        #region Shaman Stone Changes
        private void ShamanStoneVengefulSpiritScaling(On.HutongGames.PlayMaker.Actions.SetScale.orig_OnEnter orig, SetScale self)
        {
            if (self.Fsm.GameObject.name == "Fireball(Clone)" && self.Fsm.Name == "Fireball Control" && self.State.Name == "Set Damage" && self.State.ActiveActionIndex == 6)
            {
                self.x.Value = LS.shamanStoneVSSizeScaleX;
                self.y.Value = LS.shamanStoneVSSizeScaleY;
            }

            orig(self);
        }

        private void ShamanStoneShadeSoulScaling(On.HutongGames.PlayMaker.Actions.FloatMultiply.orig_OnEnter orig, FloatMultiply self)
        {
            if (self.Fsm.GameObject.name == "Fireball2 Spiral(Clone)" && self.Fsm.Name == "Fireball Control" && self.State.Name == "Set Damage")
            {
                if (self.floatVariable.Name == "X Scale")
                {
                    self.multiplyBy.Value = LS.shamanStoneSSSizeScaleX;
                }
                else
                {
                    self.multiplyBy.Value = LS.shamanStoneSSSizeScaleY;
                }
            }

            orig(self);
        }

        private void ShamanStoneDamage(On.HutongGames.PlayMaker.Actions.SetFsmInt.orig_OnEnter orig, SetFsmInt self)
        {
            if (self.Fsm.GameObject.name == "Fireball2 Spiral(Clone)" && self.Fsm.Name == "Fireball Control" && self.State.Name == "Set Damage" && self.State.ActiveActionIndex == 5)
            {
                self.setValue.Value = LS.shamanStoneSSDamage;
            }

            else if (self.Fsm.GameObject.name == "Fireball(Clone)" && self.Fsm.Name == "Fireball Control" && self.State.Name == "Set Damage" && self.State.ActiveActionIndex == 4)
            {
                self.setValue.Value = LS.shamanStoneVSDamage;
            }

            else if (self.Fsm.Name == "Set Damage" && self.State.Name == "Set Damage" && self.State.ActiveActionIndex == 2)
            {
                if (self.Fsm.GameObject.transform.parent.gameObject.name == "Scr Heads")
                {
                    self.setValue.Value = LS.shamanStoneHWDamage;
                }

                else if (self.Fsm.GameObject.transform.parent.gameObject.name == "Scr Heads 2")
                {
                    self.setValue.Value = LS.shamanStoneASDamage;
                }

                else if (self.Fsm.GameObject.transform.parent.gameObject.name == "Q Slam")
                {
                    self.setValue.Value = LS.shamanStoneDDiveDamage;
                }

                else if (self.Fsm.GameObject.transform.parent.gameObject.name == "Q Slam 2")
                {
                    self.setValue.Value = LS.shamanStoneDDarkDamage;
                }

                else if (self.Fsm.GameObject.name == "Q Fall Damage")
                {
                    self.setValue.Value = LS.shamanStoneDiveDamage;
                }

            }

            orig(self);
        }
        #endregion
        #region Soul Catcher/Eater Changes
        private void SoulCharmChanges(ILContext il)
        {
            ILCursor cursor = new ILCursor(il).Goto(0);

            // Soul Catcher
            cursor.TryGotoNext(i => i.MatchLdcI4(3));
            cursor.GotoNext();
            cursor.EmitDelegate<Func<int, int>>(soul => LS.soulCatcherSoul);

            // Soul Eater
            cursor.TryGotoNext(i => i.MatchLdcI4(8));
            cursor.GotoNext();
            cursor.EmitDelegate<Func<int, int>>(soul => LS.soulEaterSoul);

            // Soul Catcher Reserves
            cursor.TryGotoNext(i => i.MatchLdcI4(2));
            cursor.GotoNext();
            cursor.EmitDelegate<Func<int, int>>(soul => LS.soulCatcherReservesSoul);

            // Soul Eater Reserves
            cursor.TryGotoNext(i => i.MatchLdcI4(6));
            cursor.GotoNext();
            cursor.EmitDelegate<Func<int, int>>(soul => LS.soulEaterReservesSoul);
        }
        #endregion
        #region Glowing Womb Changes
        private void GlowingWombSettings(On.HutongGames.PlayMaker.Actions.Wait.orig_OnEnter orig, Wait self)
        {
            // Glowing Womb Cloud
            if (self.Fsm.GameObject.name == "Dung Explosion(Clone)" && self.Fsm.Name == "Explosion Control" && self.State.Name == "Explode")
            {
                // Duration & Visuals
                //self.time.Value = LS.glowingWombDefendersCrestDuration;
                //self.Fsm.GameObject.transform.Find("Particle System").GetComponent<ParticleSystem>().startLifetime = LS.glowingWombDefendersCrestDuration;

                // Damage Rate
                self.Fsm.GameObject.GetComponent<DamageEffectTicker>().damageInterval = LS.glowingWombDefendersCrestDamageRate;
            }

            // Hatchling Spawn Rate
            else if (self.Fsm.GameObject.name == "Charm Effects" && self.Fsm.Name == "Hatchling Spawn" && self.State.Name == "Equipped")
            {
                self.time.Value = LS.glowingWombSpawnRate;
            }

            orig(self);
        }
        private void HatchlingDamage(On.KnightHatchling.orig_OnEnable orig, KnightHatchling self)
        {
            self.normalDetails.damage = LS.glowingWombDamage;
            self.dungDetails.damage = LS.glowingWombDefendersCrestDamage;

            orig(self);
        }
        private void HatchlingFotFSettings(ILContext il)
        {
            // Hatchling HP Requirement
            ILCursor cursor = new ILCursor(il).Goto(0);

            cursor.TryGotoNext
                (
                i => i.MatchLdstr("health"),
                i => i.MatchCallvirt<PlayerData>("GetInt"),
                i => i.MatchLdcI4(1)
                );
            cursor.GotoNext();
            cursor.GotoNext();
            cursor.EmitDelegate<Func<int, int>>(health => (PlayerData.instance.health <= LS.furyOfTheFallenHealth) ? 1 : 0);

            // Hatchling Damage Increase
            cursor.Goto(0);
            cursor.TryGotoNext
                (
                i => i.MatchDup(),
                i => i.MatchLdindI4(),
                i => i.MatchLdcI4(5)
                );
            cursor.GotoNext();
            cursor.GotoNext();
            cursor.GotoNext();
            cursor.EmitDelegate<Func<int, int>>(HatchlingBonusDamage => LS.glowingWombFuryOfTheFallenDamage);
        }

        private void HatchlingSpawnRequirements(On.HutongGames.PlayMaker.Actions.IntCompare.orig_OnEnter orig, IntCompare self)
        {

            if (self.Fsm.GameObject.name == "Charm Effects" && self.Fsm.Name == "Hatchling Spawn")
            {
                // Spawn Cost
                if (self.State.Name == "Can Hatch?")
                {
                    self.integer2.Value = LS.glowingWombSpawnCost;
                }

                // Spawn Max
                else if (self.State.Name == "Check Count")
                {
                    self.integer2.Value = LS.glowingWombSpawnTotal;
                }
            }

            orig(self);
        }
        #endregion
        #region Fragile Charms Changes
        private void FragileCharmsBreak(On.HutongGames.PlayMaker.Actions.PlayerDataBoolTest.orig_OnEnter orig, PlayerDataBoolTest self)
        {
            if (self.Fsm.GameObject.name == "Hero Death" && self.Fsm.Name == "Death Anim" && self.State.Name.Contains("Break Glass ") && self.boolName.Value.Contains("_unbreakable"))
            {
                self.isFalse = LS.fragileCharmsBreak ? null : FsmEvent.GetFsmEvent("FINISHED");
            }

            orig(self);
        }

        private void StrengthDamageIncrease(On.HutongGames.PlayMaker.Actions.FloatMultiply.orig_OnEnter orig, FloatMultiply self)
        {
            if (self.Fsm.GameObject.name == "Attacks" && self.Fsm.Name == "Set Slash Damage" && self.State.Name == "Glass Attack Modifier")
            {
                self.multiplyBy.Value = 1f + (float)(LS.strengthDamageIncrease / 100f);
            }

            orig(self);
        }

        private void GreedGeoIncrease(ILContext il)
        {
            ILCursor cursor = new ILCursor(il).Goto(0);

            while(cursor.TryGotoNext(i => i.MatchLdcR4(0.2f)))
            {
                cursor.GotoNext();
                cursor.EmitDelegate<Func<float, float>>(geoScale => LS.greedGeoIncrease / 100f);
            }
        }
        #endregion
        #region Nailmaster's Glory Changes
        private void NailArtChargeTime(On.HeroController.orig_CharmUpdate orig, HeroController self)
        {
            orig(self);

            if (PlayerData.instance.equippedCharm_26)
            {
                ReflectionHelper.SetField<HeroController, float>(self, "nailChargeTime", LS.nailmastersGloryChargeTime);
            }
        }
        #endregion
        #region Joni's Blessing Changes
        private void JonisScaling(ILContext il)
        {
            ILCursor cursor = new ILCursor(il).Goto(0);
            cursor.GotoNext(i => i.MatchLdcR4(1.4f));
            cursor.GotoNext();
            cursor.EmitDelegate<Func<float, float>>(scale => 1f + (float)(LS.jonisBlessingScaling / 100f));
        }
        #endregion
        #region Shape Of Unn Changes
        private void SlugSpeeds(On.HutongGames.PlayMaker.Actions.SetFloatValue.orig_OnEnter orig, SetFloatValue self)
        {
            if (self.Fsm.GameObject.name == "Knight" && self.Fsm.Name == "Spell Control")
            {
                if (self.State.Name == "Slug Speed")
                {
                    if (self.State.ActiveActionIndex == 3)
                    {
                        self.floatValue.Value = -LS.shapeOfUnnSpeed;
                    }
                    else if (self.State.ActiveActionIndex == 4)
                    {
                        self.floatValue.Value = LS.shapeOfUnnSpeed;
                    }

                    else if (self.State.ActiveActionIndex == 6)
                    {
                        self.floatValue.Value = -LS.shapeOfUnnQuickFocusSpeed;
                    }

                    else if (self.State.ActiveActionIndex == 7)
                    {
                        self.floatValue.Value = LS.shapeOfUnnQuickFocusSpeed;
                    }
                }

            }

            orig(self);
        }
        #endregion
        #region Hiveblood Changes
        private void HivebloodTimers(On.HutongGames.PlayMaker.Actions.FloatCompare.orig_OnEnter orig, FloatCompare self)
        {
            if (self.Fsm.GameObject.name == "Health" && self.Fsm.Name == "Hive Health Regen" && self.State.Name.Contains("Recover "))
            {
                self.float2.Value = LS.hivebloodTimer / 2f;
            }

            else if (self.Fsm.GameObject.name == "Blue Health Hive(Clone)" && self.Fsm.Name == "blue_health_display" && self.State.Name.Contains("Regen "))
            {
                self.float2.Value = LS.hivebloodJonisTimer / 2f;
            }

            orig(self);
        }

        #endregion
        #region Dream Wielder Changes
        private void DreamWielderSoul(ILContext il)
        {
            ILCursor cursor = new ILCursor(il).Goto(0);

            cursor.TryGotoNext(i => i.MatchLdcI4(66));
            cursor.GotoNext();
            cursor.EmitDelegate<Func<int, int>>(soul => LS.dreamWielderSoulGain);
        }
        private void DreamWielderEssence(ILContext il)
        {
            ILCursor cursor = new ILCursor(il).Goto(0);

            // Low Value
            cursor.TryGotoNext(i => i.MatchLdcI4(40));
            cursor.GotoNext();
            cursor.EmitDelegate<Func<int, int>>(chanceLow => LS.dreamWielderEssenceChanceLow);

            // High Value
            cursor.TryGotoNext(i => i.MatchLdcI4(200));
            cursor.GotoNext();
            cursor.EmitDelegate<Func<int, int>>(chanceHigh => LS.dreamWielderEssenceChanceHigh);
        }

        #endregion
        #region Dashmaster Changes
        private void DashmasterChanges(ILContext il)
        {
            ILCursor cursor = new ILCursor(il).Goto(0);

            // Downward Dashing
            cursor.TryGotoNext(i => i.MatchLdstr("equippedCharm_31"));
            cursor.GotoNext();
            cursor.GotoNext();
            cursor.EmitDelegate<Func<bool, bool>>(downDash =>  downDash && LS.dashmasterDownwardDash);

            // Dash Cooldown
            cursor.TryGotoNext(i => i.MatchLdfld<HeroController>("DASH_COOLDOWN_CH"));
            cursor.GotoNext();
            cursor.EmitDelegate<Func<float, float>>(cooldown => LS.dashmasterDashCooldown);
        }
        #endregion
        #region Quick Slash Changes
        private void QuickSlashAttackCooldown(ILContext il)
        {
            ILCursor cursor = new ILCursor(il).Goto(0);
            cursor.TryGotoNext(i => i.MatchLdfld<HeroController>("ATTACK_COOLDOWN_TIME_CH"));
            cursor.GotoNext();
            cursor.EmitDelegate<Func<float, float>>(time => LS.quickSlashAttackCooldown);
        }

        private void QuickSlashAttackDuration(ILContext il)
        {
            ILCursor cursor = new ILCursor(il).Goto(0);
            cursor.TryGotoNext(i => i.MatchLdfld<HeroController>("ATTACK_DURATION_CH"));
            cursor.GotoNext();
            cursor.EmitDelegate<Func<float, float>>(time => Mathf.Min(0.35f, LS.quickSlashAttackCooldown + 0.03f));
        }
        #endregion
        #region Spell Twister Changes
        private void SpellTwisterSpellCost(On.HutongGames.PlayMaker.Actions.SetFsmInt.orig_OnEnter orig, SetFsmInt self)
        {
            if (self.Fsm.GameObject.name == "Charm Effects" && self.Fsm.Name == "Set Spell Cost" && self.State.Name == "Mage")
            {
                self.setValue.Value = LS.spellTwisterSpellCost;
            }

            orig(self);
        }

        #endregion
        #region Grubberfly's Elegy Changes
        private void GrubberflysSizeScale(ILContext il)
        {
            ILCursor cursor = new ILCursor(il).Goto(0);

            while(cursor.TryGotoNext(i => i.MatchLdfld<HeroController>("MANTIS_CHARM_SCALE")))
            {
                cursor.GotoNext();
                cursor.EmitDelegate<Func<float, float>>(scale => 1f + (float)(LS.grubberflysElegyMarkOfPrideScale / 100f));
            }
        }
        private void GrubberflysElegyJoniBeam(On.HeroController.orig_TakeDamage orig, HeroController self, GameObject go, CollisionSide damageSide, int damageAmount, int hazardType)
        {
            joniBeamChecker = ReflectionHelper.GetField<HeroController, bool>(self, "joniBeam");

            orig(self, go, damageSide, damageAmount, hazardType);

            ReflectionHelper.SetField<HeroController, bool>(self, "joniBeam", (joniBeamChecker != ReflectionHelper.GetField<HeroController, bool>(self, "joniBeam")) && !LS.grubberflysElegyJoniBeamDamageBool);
        }
        private void GrubberflysElegyFotFScaling(On.HutongGames.PlayMaker.Actions.FloatMultiply.orig_OnEnter orig, FloatMultiply self)
        {
            if (self.Fsm.GameObject.name.Contains("Grubberfly Beam") && self.Fsm.Name == "Control" && self.State.Name == "Fury Multiplier")
            {
                self.multiplyBy.Value = 1f + (float)(LS.grubberflysElegyFuryOfTheFallenScaling / 100f);
            }

            orig(self);
        }
        private void GrubberflysElegyDamage(On.HutongGames.PlayMaker.Actions.FloatOperator.orig_OnEnter orig, FloatOperator self)
        {
            if (self.Fsm.GameObject.name == "Attacks" && self.Fsm.Name == "Set Slash Damage" && self.State.Name == "Set Beam Damage")
            {
                self.float2.Value = 0.5f * LS.grubberflysElegyDamageScale;
            }

            orig(self);
        }

        #endregion
        #region Kingsoul Changes
        private void KingsoulTimer(On.HutongGames.PlayMaker.Actions.Wait.orig_OnEnter orig, Wait self)
        {
            if (self.Fsm.GameObject.name == "Charm Effects" && self.Fsm.Name == "White Charm" && self.State.Name == "Wait")
            {
                self.time.Value = LS.kingsoulSoulTime;
            }

            orig(self);
        }

        private void KingsoulSoul(On.HutongGames.PlayMaker.Actions.SendMessageV2.orig_DoSendMessage orig, SendMessageV2 self)
        {
            if (self.Fsm.GameObject.name == "Charm Effects" && self.Fsm.Name == "White Charm" && self.State.Name == "Soul UP")
            {
                self.functionCall.IntParameter.Value = LS.kingsoulSoulGain;
            }

            orig(self);
        }

        #endregion
        #region Sprintmaster Changes
        private void SprintmasterSpeed(ILContext il)
        {
            ILCursor cursor = new ILCursor(il).Goto(0);
            cursor.TryGotoNext(i => i.MatchLdfld<HeroController>("RUN_SPEED_CH_COMBO"));
            cursor.GotoNext();
            cursor.EmitDelegate<Func<float, float>>(speedCombo => LS.sprintmasterSpeedCombo);

            cursor.TryGotoNext(i => i.MatchLdfld<HeroController>("RUN_SPEED_CH"));
            cursor.GotoNext();
            cursor.EmitDelegate<Func<float, float>>(speed => LS.sprintmasterSpeed);
        }
        #endregion
        #region Dreamshield Changes
        private void DreamshieldAudio(On.HutongGames.PlayMaker.Actions.AudioPlayerOneShotSingle.orig_OnEnter orig, AudioPlayerOneShotSingle self)
        {
            if (self.Fsm.GameObject.name == "Shield" && self.Fsm.Name == "Shield Hit" && self.State.Name == "Slash Anim")
            {
                self.volume = LS.dreamshieldNoise ? 1.0f : 0f;
            }

            orig(self);
        }
        private void DreamshieldSizes(On.HutongGames.PlayMaker.Actions.SetScale.orig_OnEnter orig, SetScale self)
        {
            if (self.Fsm.GameObject.name == "Shield" && self.Fsm.Name == "Shield Hit" && self.State.Name == "Dreamwielder?")
            {
                if (self.State.ActiveActionIndex == 0)
                {
                    self.x.Value = -LS.dreamshieldSizeScale;
                    self.y.Value = LS.dreamshieldSizeScale;
                }

                else if (self.State.ActiveActionIndex == 1)
                {
                    self.x.Value = LS.dreamshieldSizeScale;
                    self.y.Value = LS.dreamshieldSizeScale;
                }

                else if (self.State.ActiveActionIndex == 3)
                {
                    self.x.Value = -LS.dreamshieldDreamWielderSizeScale;
                    self.y.Value = LS.dreamshieldDreamWielderSizeScale;
                }

                else if (self.State.ActiveActionIndex == 4)
                {
                    self.x.Value = LS.dreamshieldDreamWielderSizeScale;
                    self.y.Value = LS.dreamshieldDreamWielderSizeScale;
                }
            }

            orig(self);
        }
        private void DreamshieldSpeeds(On.HutongGames.PlayMaker.Actions.SetFsmFloat.orig_OnEnter orig, SetFsmFloat self)
        {
            if (self.Fsm.GameObject.name == "Orbit Shield(Clone)" && self.Fsm.Name == "Focus Speedup")
            {
                if (self.State.Name == "Idle")
                {
                    self.setValue.Value = LS.dreamshieldSpeed;
                }

                else if (self.State.Name == "Focus")
                {
                    self.setValue.Value = LS.dreamshieldFocusSpeed;
                }
            }

            orig(self);
        }
        private void DreamshieldDamage(On.HutongGames.PlayMaker.Actions.GetPlayerDataInt.orig_OnEnter orig, GetPlayerDataInt self)
        {
            orig(self);

            if (self.Fsm.GameObject.name == "Shield" && self.Fsm.Name == "Shield Hit" && self.State.Name == "Init")
            {
                self.storeValue.Value = (int)(self.storeValue.Value * LS.dreamshieldDamageScale);
            }
        }
        private void DreamshieldReformationTime(On.HutongGames.PlayMaker.Actions.Wait.orig_OnEnter orig, Wait self)
        {
            if (self.Fsm.GameObject.name == "Shield" && self.Fsm.Name == "Shield Hit" && self.State.Name == "Break")
            {
                self.time.Value = LS.dreamshieldReformationTime;
            }

            orig(self);
        }
        #endregion
        #region Weaversong Changes
        private void WeaversongDamageAndGrubsongSoul(On.HutongGames.PlayMaker.Actions.CallMethodProper.orig_OnEnter orig, CallMethodProper self)
        {
            if (self.Fsm.GameObject.name == "Enemy Damager" && self.Fsm.Name == "Attack" && self.State.Name == "Grubsong" && self.methodName.Value == "AddMPCharge")
            {
                self.parameters = new FsmVar[1] { new FsmVar(typeof(int)) { intValue = LS.weaversongGrubsongSoul } };
                self.Fsm.GetFsmInt("Damage").Value = LS.weaversongDamage;
            }

            orig(self);
        }
        private void WeaversongSpeeds(On.HutongGames.PlayMaker.Actions.RandomFloat.orig_OnEnter orig, RandomFloat self)
        {
            if (self.Fsm.GameObject.name == "Weaverling(Clone)" && self.Fsm.Name == "Control")
            {
                // Speeds
                if (self.State.Name == "Run L")
                {
                    self.min.Value = -LS.weaversongSpeedMax;
                    self.max.Value = -LS.weaversongSpeedMin;
                }

                else if (self.State.Name == "Run R")
                {
                    self.min.Value = LS.weaversongSpeedMin;
                    self.max.Value = LS.weaversongSpeedMax;
                }
            }

            orig(self);
        }
        private void WeaversongSprintmasterSpeed(On.HutongGames.PlayMaker.Actions.SetFloatValue.orig_OnEnter orig, SetFloatValue self)
        {
            // Sprintmaster Multiplier
            if (self.Fsm.GameObject.name == "Weaverling(Clone)" && self.Fsm.Name == "Control" && self.State.Name == "Sprintmaster" && self.State.ActiveActionIndex == 2)
            {
                self.floatValue.Value = 1f + (float)(LS.weaversongSpeedSprintmaster / 100f);
            }

            orig(self);
        }
        private void WeaverlingSpawner(On.PlayMakerFSM.orig_OnEnable orig, PlayMakerFSM self)
        {
            orig(self);

            if (self.Fsm.GameObject.name == "Charm Effects" && self.Fsm.Name == "Weaverling Control")
            {
                SpawnObjectFromGlobalPool spawner = self.GetFsmAction<SpawnObjectFromGlobalPool>("Spawn", 0);
                SetAudioClip audio1 = self.GetFsmAction<SetAudioClip>("Spawn", 1);
                SetAudioClip audio2 = self.GetFsmAction<SetAudioClip>("Spawn", 3);
                SetAudioClip audio3 = self.GetFsmAction<SetAudioClip>("Spawn", 5);

                self.GetFsmAction<SpawnObjectFromGlobalPool>("Spawn", 0).Enabled = false;
                self.GetFsmAction<SpawnObjectFromGlobalPool>("Spawn", 2).Enabled = false;
                self.GetFsmAction<SpawnObjectFromGlobalPool>("Spawn", 4).Enabled = false;

                self.GetFsmAction<SetAudioClip>("Spawn", 1).Enabled = false;
                self.GetFsmAction<SetAudioClip>("Spawn", 3).Enabled = false;
                self.GetFsmAction<SetAudioClip>("Spawn", 5).Enabled = false;

                self.AddCustomAction("Spawn", () =>
                {
                    for (int i = 1; i <= LS.weaversongCount; i++)
                    {
                        spawner.OnEnter();

                        if (i % 3 == 0)
                        {
                            audio1.OnEnter();
                        }
                        else if (i % 3 == 1)
                        {
                            audio2.OnEnter();
                        }
                        else
                        {
                            audio3.OnEnter();
                        }
                    }
                });
            }
        }
        #endregion
        #region Grimmchild Changes
        private void GrimmchildDamage(On.HutongGames.PlayMaker.Actions.SetIntValue.orig_OnEnter orig, SetIntValue self)
        {
            if (self.Fsm.GameObject.name == "Grimmchild(Clone)" && self.Fsm.Name == "Control")
            {
                if (self.State.Name == "Level 2")
                {
                    self.intValue.Value = LS.grimmchildDamage2;
                }

                else if (self.State.Name == "Level 3")
                {
                    self.intValue.Value = LS.grimmchildDamage3;
                }

                else if (self.State.Name == "Level 4")
                {
                    self.intValue.Value = LS.grimmchildDamage4;
                }
            }

            orig(self);
        }

        private void GrimmchildAttackTimer(On.HutongGames.PlayMaker.Actions.SetFloatValue.orig_OnEnter orig, SetFloatValue self)
        {
            if (self.Fsm.GameObject.name == "Grimmchild(Clone)" && self.Fsm.Name == "Control" && (self.State.Name == "Pause" || self.State.Name == "Spawn"))
            {
                self.floatValue.Value = LS.grimmchildAttackTimer;
            }

            orig(self);
        }

        private void GrimmchildAttackTimer2(On.HutongGames.PlayMaker.Actions.RandomFloat.orig_OnEnter orig, RandomFloat self)
        {
            if (self.Fsm.GameObject.name == "Grimmchild(Clone)" && self.Fsm.Name == "Control" && self.State.Name == "Antic")
            {
                self.min.Value = LS.grimmchildAttackTimer;
                self.max.Value = LS.grimmchildAttackTimer;
            }

            orig(self);
        }
        #endregion
        #region Carefree Melody Changes
        private void CarefreeMelodyChances(ILContext il)
        {
            ILCursor cursor = new ILCursor(il).Goto(0);

            cursor.TryGotoNext(i => i.MatchLdcR4(10));
            cursor.GotoNext();
            cursor.EmitDelegate<Func<float, float>>(chance => LS.carefreeMelodyChance1);

            cursor.TryGotoNext(i => i.MatchLdcR4(20));
            cursor.GotoNext();
            cursor.EmitDelegate<Func<float, float>>(chance => LS.carefreeMelodyChance2);

            cursor.TryGotoNext(i => i.MatchLdcR4(30));
            cursor.GotoNext();
            cursor.EmitDelegate<Func<float, float>>(chance => LS.carefreeMelodyChance3);

            cursor.TryGotoNext(i => i.MatchLdcR4(50));
            cursor.GotoNext();
            cursor.EmitDelegate<Func<float, float>>(chance => LS.carefreeMelodyChance4);

            cursor.TryGotoNext(i => i.MatchLdcR4(70));
            cursor.GotoNext();
            cursor.EmitDelegate<Func<float, float>>(chance => LS.carefreeMelodyChance5);

            cursor.TryGotoNext(i => i.MatchLdcR4(80));
            cursor.GotoNext();
            cursor.EmitDelegate<Func<float, float>>(chance => LS.carefreeMelodyChance6);

            cursor.TryGotoNext(i => i.MatchLdcR4(90));
            cursor.GotoNext();
            cursor.EmitDelegate<Func<float, float>>(chance => LS.carefreeMelodyChance7);
        }
        #endregion
    }

    public class LocalSettings
    {
        #region Notch Cost Settings
        [SliderIntElement("Notch Cost Options", "Gathering Swarm", 0, 12)]
        public int charm1NotchCost = 1;

        [SliderIntElement("Notch Cost Options", "Wayward Compass", 0, 12)]
        public int charm2NotchCost = 1;

        [SliderIntElement("Notch Cost Options", "Grubsong", 0, 12)]
        public int charm3NotchCost = 1;

        [SliderIntElement("Notch Cost Options", "Stalwart Shell", 0, 12)]
        public int charm4NotchCost = 2;

        [SliderIntElement("Notch Cost Options", "Baldur Shell", 0, 12)]
        public int charm5NotchCost = 2;

        [SliderIntElement("Notch Cost Options", "Fury of the Fallen", 0, 12)]
        public int charm6NotchCost = 2;

        [SliderIntElement("Notch Cost Options", "Quick Focus", 0, 12)]
        public int charm7NotchCost = 3;

        [SliderIntElement("Notch Cost Options", "Lifeblood Heart", 0, 12)]
        public int charm8NotchCost = 2;

        [SliderIntElement("Notch Cost Options", "Lifeblood Core", 0, 12)]
        public int charm9NotchCost = 3;

        [SliderIntElement("Notch Cost Options", "Defender's Crest", 0, 12)]
        public int charm10NotchCost = 1;

        [SliderIntElement("Notch Cost Options", "Flukenest", 0, 12)]
        public int charm11NotchCost = 3;

        [SliderIntElement("Notch Cost Options", "Thorns of Agony", 0, 12)]
        public int charm12NotchCost = 1;

        [SliderIntElement("Notch Cost Options", "Mark of Pride", 0, 12)]
        public int charm13NotchCost = 3;

        [SliderIntElement("Notch Cost Options", "Steady Body", 0, 12)]
        public int charm14NotchCost = 1;

        [SliderIntElement("Notch Cost Options", "Heavy Blow", 0, 12)]
        public int charm15NotchCost = 2;

        [SliderIntElement("Notch Cost Options", "Sharp Shadow", 0, 12)]
        public int charm16NotchCost = 2;

        [SliderIntElement("Notch Cost Options", "Spore Shroom", 0, 12)]
        public int charm17NotchCost = 1;

        [SliderIntElement("Notch Cost Options", "Longnail", 0, 12)]
        public int charm18NotchCost = 2;

        [SliderIntElement("Notch Cost Options", "Shaman Stone", 0, 12)]
        public int charm19NotchCost = 3;

        [SliderIntElement("Notch Cost Options", "Soul Catcher", 0, 12)]
        public int charm20NotchCost = 2;

        [SliderIntElement("Notch Cost Options", "Soul Eater", 0, 12)]
        public int charm21NotchCost = 4;

        [SliderIntElement("Notch Cost Options", "Glowing Womb", 0, 12)]
        public int charm22NotchCost = 2;

        [SliderIntElement("Notch Cost Options", "Fragile Heart", 0, 12)]
        public int charm23NotchCost = 2;

        [SliderIntElement("Notch Cost Options", "Fragile Greed", 0, 12)]
        public int charm24NotchCost = 2;

        [SliderIntElement("Notch Cost Options", "Fragile Strength", 0, 12)]
        public int charm25NotchCost = 3;

        [SliderIntElement("Notch Cost Options", "Nailmaster's Glory", 0, 12)]
        public int charm26NotchCost = 1;

        [SliderIntElement("Notch Cost Options", "Joni's Blessing", 0, 12)]
        public int charm27NotchCost = 4;

        [SliderIntElement("Notch Cost Options", "Shape of Unn", 0, 12)]
        public int charm28NotchCost = 2;

        [SliderIntElement("Notch Cost Options", "Hiveblood", 0, 12)]
        public int charm29NotchCost = 4;

        [SliderIntElement("Notch Cost Options", "Dream Wielder", 0, 12)]
        public int charm30NotchCost = 1;

        [SliderIntElement("Notch Cost Options", "Dashmaster", 0, 12)]
        public int charm31NotchCost = 2;

        [SliderIntElement("Notch Cost Options", "Quick Slash", 0, 12)]
        public int charm32NotchCost = 3;

        [SliderIntElement("Notch Cost Options", "Spell Twister", 0, 12)]
        public int charm33NotchCost = 2;

        [SliderIntElement("Notch Cost Options", "Deep Focus", 0, 12)]
        public int charm34NotchCost = 4;

        [SliderIntElement("Notch Cost Options", "Grubberfly's Elegy", 0, 12)]
        public int charm35NotchCost = 3;

        [SliderIntElement("Notch Cost Options", "Kingsoul / Void Heart", 0, 12)]
        public int charm36NotchCost = 5;

        [SliderIntElement("Notch Cost Options", "Sprintmaster", 0, 12)]
        public int charm37NotchCost = 1;

        [SliderIntElement("Notch Cost Options", "Dreamshield", 0, 12)]
        public int charm38NotchCost = 3;

        [SliderIntElement("Notch Cost Options", "Weaversong", 0, 12)]
        public int charm39NotchCost = 2;

        [SliderIntElement("Notch Cost Options", "Grimmchild / Carefree", 0, 12)]
        public int charm40NotchCost = 2;

        [ButtonElement("Notch Cost Options", "Reset Defaults (remove charms first)", "")]
        public void ResetNotchCosts()
        {
            charm1NotchCost = 1;
            charm2NotchCost = 1;
            charm3NotchCost = 1;
            charm4NotchCost = 2;
            charm5NotchCost = 2;
            charm6NotchCost = 2;
            charm7NotchCost = 3;
            charm8NotchCost = 2;
            charm9NotchCost = 3;
            charm10NotchCost = 1;
            charm11NotchCost = 3;
            charm12NotchCost = 1;
            charm13NotchCost = 3;
            charm14NotchCost = 1;
            charm15NotchCost = 2;
            charm16NotchCost = 2;
            charm17NotchCost = 1;
            charm18NotchCost = 2;
            charm19NotchCost = 3;
            charm20NotchCost = 2;
            charm21NotchCost = 4;
            charm22NotchCost = 2;
            charm23NotchCost = 2;
            charm24NotchCost = 2;
            charm25NotchCost = 3;
            charm26NotchCost = 1;
            charm27NotchCost = 4;
            charm28NotchCost = 2;
            charm29NotchCost = 4;
            charm30NotchCost = 1;
            charm31NotchCost = 2;
            charm32NotchCost = 3;
            charm33NotchCost = 2;
            charm34NotchCost = 4;
            charm35NotchCost = 3;
            charm36NotchCost = 5;
            charm37NotchCost = 1;
            charm38NotchCost = 3;
            charm39NotchCost = 2;
            charm40NotchCost = 2;

            PlayerData.instance.charmSlotsFilled = 0;
        }
        #endregion

        #region Grubsong Settings
        [InputIntElement("Grubsong Options", "Soul", 0, 198, ElementDesc = "description goes here")]
        public int grubsongDamageSoul = 15;

        [InputIntElement("Grubsong Options", "Grubberfly's Soul", 0, 198)]
        public int grubsongDamageSoulCombo = 25;

        [ButtonElement("Grubsong Options", "Reset Defaults", "")]
        public void ResetGrubsong()
        {
            grubsongDamageSoul = 15;
            grubsongDamageSoulCombo = 25;
        }
        #endregion
        #region Stalwart Shell Settings
        [InputFloatElement("Stalwart Shell Options", "Invuln. Time", 0, 10)]
        public float stalwartShellInvulnerability = 1.75f;

        [InputFloatElement("Stalwart Shell Options", "Recoil Time", 0, 2.9f)]
        public float stalwartShellRecoil = 0.08f;

        [ButtonElement("Stalwart Shell Options", "Reset Defaults", "")]
        public void ResetStalwartShell()
        {
            stalwartShellInvulnerability = 1.75f;
            stalwartShellRecoil = 0.08f;
        }
        #endregion
        #region Baldur Shell Settings
        [InputFloatElement("Baldur Shell Options", "Enemy Knockback Mult.", 0f, 5f)]
        public float baldurShellKnockbackMult = 1.0f;

        [SliderIntElement("Baldur Shell Options", "Blocks", 0, 4)]
        public int baldurShellBlocks = 4;

        [ButtonElement("Baldur Shell Options", "Reset Defaults", "")]
        public void ResetBaldurShell()
        {
            baldurShellKnockbackMult = 1.0f;
            baldurShellBlocks = 4;
        }
        #endregion
        #region Fury of the Fallen Settings
        [BoolElement("Fury of the Fallen Options", "Works With Joni's Blessing", "Should FotF work with Joni's Blessing at any Health?")]
        public bool furyOfTheFallenJonis = false;

        [SliderIntElement("Fury of the Fallen Options", "Health Threshold", 0, 13)]
        public int furyOfTheFallenHealth = 1;

        [InputIntElement("Fury of the Fallen Options", "Damage Increase (%)", 0, 500)]
        public int furyOfTheFallenScaling = 75;

        [ButtonElement("Fury of the Fallen Options", "Reset Defaults", "")]
        public void ResetFuryOfTheFallen()
        {
            furyOfTheFallenHealth = 1;
            furyOfTheFallenJonis = false;
            furyOfTheFallenScaling = 75;
        }
        #endregion
        #region Quick/Deep Focus Settings
        [InputFloatElement("Quick/Deep Focus Options", "Quick Focus Time", 0.001f, 2f)]
        public float quickFocusFocusTime = 0.594f;

        [InputIntElement("Quick/Deep Focus Options", "Deep Focus Added Time (%)", 0, 500)]
        public int deepFocusHealingTimeScale = 65;

        [SliderIntElement("Quick/Deep Focus Options", "Deep Focus Healing", 0, 13)]
        public int deepFocusHealing = 2;

        [ButtonElement("Quick/Deep Focus Options", "Reset Defaults", "")]    
        public void ResetQuickAndDeepFocus()
        {
            quickFocusFocusTime = 0.594f;
            deepFocusHealingTimeScale = 65;
            deepFocusHealing = 2;
        }
        #endregion
        #region Lifeblood Heart/Core Settings
        [SliderIntElement("Lifeblood Heart/Core Options", "Lifeblood Heart Masks", 0, 12)]
        public int lifebloodHeartLifeblood = 2;

        [SliderIntElement("Lifeblood Heart/Core Options", "Lifeblood Core Masks", 0, 12)]
        public int lifebloodCoreLifeblood = 4;

        [ButtonElement("Lifeblood Heart/Core Options", "Reset Defaults", "")]
        public void ResetLifebloodHeartAndCore()
        {
            lifebloodHeartLifeblood = 2;
            lifebloodCoreLifeblood = 4;
        }
        #endregion
        #region Defender's Crest Settings
        [InputIntElement("Defender's Crest Options", "Shop Discount (%)", 0, 100)]
        public int defendersCrestDiscount = 20;

        [InputFloatElement("Defender's Crest Options", "Cloud Spawn Timer", 0.2f, 5f)]
        public float defendersCrestCloudFrequency = 0.75f;

        //[InputFloatElement("Defender's Crest Options", "Cloud Duration", 0f, 5f)]
        //public float defendersCrestCloudDuration = 1.1f;

        [InputFloatElement("Defender's Crest Options", "Damage Timer", 0.01f, 1f)]
        public float defendersCrestDamageRate = 0.3f;

        [ButtonElement("Defender's Crest Options", "Reset Defaults", "")]
        public void ResetDefendersCrest()
        {
            defendersCrestDiscount = 20;
            defendersCrestCloudFrequency = 0.75f;
            //defendersCrestCloudDuration = 1.1f;
            defendersCrestDamageRate = 0.3f;
        }
        #endregion
        #region Flukenest Settings
        [InputIntElement("Flukenest Options", "Damage", 0, 100)]
        public int flukenestDamage = 4;

        [InputIntElement("Flukenest Options", "Shaman Stone Damage", 0, 100)]
        public int flukenestShamanStoneDamage = 5;

        [InputIntElement("Flukenest Options", "Vengeful Spirit Fluke #", 0, 36)]
        public int flukenestVSFlukes = 9;

        [InputIntElement("Flukenest Options", "Shade Soul Fluke #", 0, 64)]
        public int flukenestSSFlukes = 16;

        [InputFloatElement("Flukenest Options", "Minimum Size Scale", 0f, 3f)]
        public float flukenestFlukeSizeMin = 0.7f;

        [InputFloatElement("Flukenest Options", "Maximum Size Scale", 0f, 3f)]
        public float flukenestFlukeSizeMax = 0.9f;

        [InputFloatElement("Flukenest Options", "SS Minimum Size Scale", 0f, 3f)]
        public float flukenestShamanStoneFlukeSizeMin = 0.9f;

        [InputFloatElement("Flukenest Options", "SS Maximum Size Scale", 0f, 3f)]
        public float flukenestShamanStoneFlukeSizeMax = 1.2f;

        //[InputFloatElement("Flukenest Options", "DC Cloud Duration", 0f, 10f)]
        //public float flukenestDefendersCrestDuration = 2.2f;

        [InputFloatElement("Flukenest Options", "DC Damage Timer", 0.01f, 1f)]
        public float flukenestDefendersCrestDamageRate = 0.1f;

        [InputFloatElement("Flukenest Options", "DC + SS Damage Timer", 0.01f, 1f)]
        public float flukenestDefendersCrestShamanStoneDamageRate = 0.075f;

        [ButtonElement("Flukenest Options", "Reset Defaults", "")]
        public void ResetFlukenest()
        {
            flukenestDamage = 4;
            flukenestShamanStoneDamage = 5;
            flukenestFlukeSizeMin = 0.7f;
            flukenestFlukeSizeMax = 0.9f;
            flukenestShamanStoneFlukeSizeMin = 0.9f;
            flukenestShamanStoneFlukeSizeMax = 1.2f;
            flukenestVSFlukes = 9;
            flukenestSSFlukes = 16;
            //flukenestDefendersCrestDuration = 2.2f;
            flukenestDefendersCrestDamageRate = 0.1f;
            flukenestDefendersCrestShamanStoneDamageRate = 0.075f;
        }
        #endregion
        #region Thorns of Agony Settings
        [InputFloatElement("Thorns of Agony Options", "Damage Multiplier", 0f, 5f)]
        public float thornsOfAgonyDamageMultiplier = 1.0f;

        [ButtonElement("Thorns of Agony Options", "Reset Defaults", "")]
        public void ResetThornsOfAgony()
        {
            thornsOfAgonyDamageMultiplier = 1.0f;
        }
        #endregion
        #region Longnail / Mark of Pride Settings
        [BoolElement("Longnail / Mark of Pride Options", "Works with Wall Slash", "")]
        public bool longnailMarkOfPrideWallSlash = false;

        [InputIntElement("Longnail / Mark of Pride Options", "Combo Size Increase (%)", 0, 500)]
        public int longnailMarkOfPrideScale = 40;

        [InputIntElement("Longnail / Mark of Pride Options", "MoP Size Increase (%)", 0, 500)]
        public int markOfPrideScale = 25;

        [InputIntElement("Longnail / Mark of Pride Options", "Longnail Size Increase (%)", 0, 500)]
        public int longnailScale = 15;

        [ButtonElement("Longnail / Mark of Pride Options", "Reset Defaults", "")]
        public void ResetLongnailAndMarkOfPride()
        {
            longnailMarkOfPrideWallSlash = false;
            longnailMarkOfPrideScale = 40;
            markOfPrideScale = 25;
            longnailScale = 15;            
        }
        #endregion
        #region Heavy Blow Settings
        [BoolElement("Heavy Blow Options", "Works with Wall Slash", "")]
        public bool heavyBlowWallSlash = false;

        [BoolElement("Heavy Blow Options", "Works with Cyclone Slash", "")]
        public bool heavyBlowCycloneSlash = false;

        [InputIntElement("Heavy Blow Options", "Knockback Increase (%)", 0, 500)]
        public int heavyBlowSlashRecoil = 75;

        [InputIntElement("Heavy Blow Options", "Great Slash Increase (%)", 0, 500)]
        public int heavyBlowGreatSlashRecoil = 33;

        [InputIntElement("Heavy Blow Options", "Cyclone Slash Increase (%)", 0, 500)]
        public int heavyBlowCycloneSlashRecoil = 25;

        [SliderIntElement("Heavy Blow Options", "Stagger Reduction", 0, 20)]
        public int heavyBlowStagger = 1;

        [SliderIntElement("Heavy Blow Options", "Combo Stagger Reduction", 0, 20)]
        public int heavyBlowStaggerCombo = 1;

        [ButtonElement("Heavy Blow Options", "Reset Defaults", "")]
        public void ResetHeavyBlow()
        {
            heavyBlowWallSlash = false;
            heavyBlowCycloneSlash = false;
            heavyBlowSlashRecoil = 75;
            heavyBlowGreatSlashRecoil = 33;
            heavyBlowCycloneSlashRecoil = 25;
            heavyBlowStagger = 1;
            heavyBlowStaggerCombo = 1;
        }
        #endregion
        #region Sharp Shadow Settings
        [InputFloatElement("Sharp Shadow Options", "Damage Multiplier", 0f, 5f)]
        public float SharpShadowDamageMultiplier = 1.0f;

        [InputIntElement("Sharp Shadow Options", "Dashmaster Increase (%)", 0, 500)]
        public int SharpShadowDashmasterDamageIncrease = 50;

        [InputFloatElement("Sharp Shadow Options", "Shadow Dash Speed", 0f, 75f)]
        public float SharpShadowDashSpeed = 28f;

        [ButtonElement("Sharp Shadow Options", "Reset Defaults", "")]
        public void ResetSharpShadow()
        {
            SharpShadowDamageMultiplier = 1.0f;
            SharpShadowDashmasterDamageIncrease = 50;
            SharpShadowDashSpeed = 28f;
        }
        #endregion
        #region Spore Shroom Settings
        [BoolElement("Spore Shroom Options", "Cooldown Resets On Damage", "Should Spore Shroom's cooldown reset when taking damage?")]
        public bool sporeShroomDamageResetsCooldown = true;

        [InputFloatElement("Spore Shroom Options", "Cloud Cooldown", 0f, 20f)]
        public float sporeShroomCooldown = 4.25f;

        [InputFloatElement("Spore Shroom Options", "Spore Cloud Duration", 0f, 10f)]
        public float sporeShroomCloudDuration = 4.1f;

        [InputFloatElement("Spore Shroom Options", "Damage Timer", 0.01f, 1f)]
        public float sporeShroomDamageRate = 0.15f;

        //[InputFloatElement("Spore Shroom Options", "DC Cloud Duration", 0f, 10f)]
        //public float sporeShroomDefendersCrestCloudDuration = 4.1f;

        [InputFloatElement("Spore Shroom Options", "DC Damage Timer", 0.01f, 1f)]
        public float sporeShroomDefendersCrestDamageRate = 0.2f;

        [ButtonElement("Spore Shroom Options", "Reset Defaults", "")]
        public void ResetSporeShroom()
        {
            sporeShroomDamageResetsCooldown = true;
            sporeShroomCooldown = 4.25f;
            sporeShroomCloudDuration = 4.1f;
            //sporeShroomDefendersCrestCloudDuration = 4.1f;
            sporeShroomDamageRate = 0.15f;
            sporeShroomDefendersCrestDamageRate = 0.2f;
        }
        #endregion
        #region Shaman Stone Settings
        [InputFloatElement("Shaman Stone Options", "Vengeful Spirit X Scale", 0f, 5f)]
        public float shamanStoneVSSizeScaleX = 1.3f;

        [InputFloatElement("Shaman Stone Options", "Vengeful Spirit Y Scale", 0f, 5f)]
        public float shamanStoneVSSizeScaleY = 1.6f;

        [InputFloatElement("Shaman Stone Options", "Shade Soul X Scale", 0f, 5f)]
        public float shamanStoneSSSizeScaleX = 1.3f;

        [InputFloatElement("Shaman Stone Options", "Shade Soul Y Scale", 0f, 5f)]
        public float shamanStoneSSSizeScaleY = 1.6f;

        [InputIntElement("Shaman Stone Options", "Vengeful Spirit Damage", 0, 100)]
        public int shamanStoneVSDamage = 20;

        [InputIntElement("Shaman Stone Options", "Shade Soul Damage", 0, 100)]
        public int shamanStoneSSDamage = 40;

        [InputIntElement("Shaman Stone Options", "Howling Wraiths Damage", 0, 100)]
        public int shamanStoneHWDamage = 20;

        [InputIntElement("Shaman Stone Options", "Abyss Shriek Damage", 0, 100)]
        public int shamanStoneASDamage = 40;

        [InputIntElement("Shaman Stone Options", "Dive Contact Damage", 0, 100)]
        public int shamanStoneDiveDamage = 23;

        [InputIntElement("Shaman Stone Options", "Desolate Dive Damage", 0, 100)]
        public int shamanStoneDDiveDamage = 30;

        [InputIntElement("Shaman Stone Options", "Descending Dark Damage", 0, 100)]
        public int shamanStoneDDarkDamage = 50;

        [ButtonElement("Shaman Stone Options", "Reset Defaults", "")]
        public void ResetShamanStone()
        {
            shamanStoneVSSizeScaleX = 1.3f;
            shamanStoneVSSizeScaleY = 1.6f;
            shamanStoneSSSizeScaleX = 1.3f;
            shamanStoneSSSizeScaleY = 1.6f;
            shamanStoneVSDamage = 20;
            shamanStoneSSDamage = 40;
            shamanStoneHWDamage = 20;
            shamanStoneASDamage = 40;
            shamanStoneDiveDamage = 23;
            shamanStoneDDiveDamage = 30;
            shamanStoneDDarkDamage = 50;
        }
        #endregion
        #region Soul Catcher/Eater Settings
        [InputIntElement("Soul Catcher/Eater Options", "Soul Catcher Soul", 0, 198)]
        public int soulCatcherSoul = 3;

        [InputIntElement("Soul Catcher/Eater Options", "Soul Eater Soul", 0, 198)]
        public int soulEaterSoul = 8;

        [InputIntElement("Soul Catcher/Eater Options", "Soul Catcher Vessel Soul", 0, 198)]
        public int soulCatcherReservesSoul = 2;

        [InputIntElement("Soul Catcher/Eater Options", "Soul Eater Vessel Soul", 0, 198)]
        public int soulEaterReservesSoul = 6;

        [ButtonElement("Soul Catcher/Eater Options", "Reset Defaults", "")]
        public void ResetSoulCatcherAndEater()
        {
            soulCatcherSoul = 3;
            soulEaterSoul = 8;
            soulCatcherReservesSoul = 2;
            soulEaterReservesSoul = 6;
        }
        #endregion
        #region Glowing Womb Settings
        [InputFloatElement("Glowing Womb Options", "Spawn Time", 0f, 10f)]
        public float glowingWombSpawnRate = 4f;

        [InputIntElement("Glowing Womb Options", "Spawn Cost", 0, 99)]
        public int glowingWombSpawnCost = 8;

        [InputIntElement("Glowing Womb Options", "Spawn Maximum", 0, 12)]
        public int glowingWombSpawnTotal = 4;

        [InputIntElement("Glowing Womb Options", "Impact Damage", 0, 100)]
        public int glowingWombDamage = 9;

        [InputIntElement("Glowing Womb Options", "FotF Damage Increase", 0, 100)]
        public int glowingWombFuryOfTheFallenDamage = 5;

        [InputIntElement("Glowing Womb Options", "DC Impact Damage", 0, 100)]
        public int glowingWombDefendersCrestDamage = 4;

        //[InputFloatElement("Glowing Womb Options", "DC Cloud Duration", 0f, 10f)]
        //public float glowingWombDefendersCrestDuration = 1f;

        [InputFloatElement("Glowing Womb Options", "DC Damage Timer", 0.01f, 1f)]
        public float glowingWombDefendersCrestDamageRate = 0.2f;

        [ButtonElement("Glowing Womb Options", "Reset Defaults", "")]
        public void ResetGlowingWomb()
        {
            glowingWombSpawnRate = 4f;
            glowingWombSpawnCost = 8;
            glowingWombSpawnTotal = 4;
            glowingWombDamage = 9;
            glowingWombDefendersCrestDamage = 4;
            glowingWombFuryOfTheFallenDamage = 5;
            //glowingWombDefendersCrestDuration = 1f;
            glowingWombDefendersCrestDamageRate = 0.2f;
        }
        #endregion
        #region Fragile Charms Settings
        [BoolElement("Fragile/Unbreakable Charms Options", "Fragiles Break On Death", "")]
        public bool fragileCharmsBreak = true;

        [InputIntElement("Fragile/Unbreakable Charms Options", "Greed Geo Increase (%)", 0, 500)]
        public int greedGeoIncrease = 20;

        [InputIntElement("Fragile/Unbreakable Charms Options", "Strength Damage Increase (%)", 0, 500)]
        public int strengthDamageIncrease = 50;

        [ButtonElement("Fragile/Unbreakable Charms Options", "Reset Defaults", "")]
        public void ResetFragileCharms()
        {
            fragileCharmsBreak = true;
            greedGeoIncrease = 20;
            strengthDamageIncrease = 50;
        }
        #endregion
        #region Nailmaster's Glory Settings
        [InputFloatElement("Nailmaster's Glory Options", "Nail Art Charge Time", 0.01f, 5f)]
        public float nailmastersGloryChargeTime = 0.75f;

        [ButtonElement("Nailmaster's Glory Options", "Reset Defaults", "")]
        public void ResetNailmastersGlory()
        {
            nailmastersGloryChargeTime = 0.75f;
        }
        #endregion
        #region Joni's Blessing Settings
        [InputIntElement("Joni's Blessing Options", "Health Increase (%)", 0, 500)]
        public int jonisBlessingScaling = 50;

        [ButtonElement("Joni's Blessing Options", "Reset Defaults", "")]
        public void ResetJonisBlessing()
        {
            jonisBlessingScaling = 50;
        }
        #endregion
        #region Shape Of Unn Settings
        [InputFloatElement("Shape Of Unn Options", "Slug Speed", 0f, 36f)]
        public float shapeOfUnnSpeed = 6f;

        [InputFloatElement("Shape Of Unn Options", "Quick Focus Slug Speed", 0f, 36f)]
        public float shapeOfUnnQuickFocusSpeed = 12f;

        [ButtonElement("Shape Of Unn Options", "Reset Defaults", "")]
        public void ResetShapeOfUnn()
        {
            shapeOfUnnSpeed = 6f;
            shapeOfUnnQuickFocusSpeed = 12f;
        }
        #endregion
        #region Hiveblood Settings
        [InputFloatElement("Hiveblood Options", "Recovery Time", 0f, 60f)]
        public float hivebloodTimer = 10f;

        [InputFloatElement("Hiveblood Options", "Joni's Blessing Recovery Time", 0f, 60f)]
        public float hivebloodJonisTimer = 20f;

        [ButtonElement("Hiveblood Options", "Reset Defaults", "")]
        public void ResetHiveblood()
        {
            hivebloodTimer = 10f;
            hivebloodJonisTimer = 20f;
        }
        #endregion
        #region Dream Wielder Settings
        [InputIntElement("Dream Wielder Options", "Soul Gain", 0, 198)]
        public int dreamWielderSoulGain = 66;

        [InputIntElement("Dream Wielder Options", "Essence Chance Low (1/X)", 1, 1000)]
        public int dreamWielderEssenceChanceLow = 200;

        [InputIntElement("Dream Wielder Options", "Essence Chance High (1/X)", 1, 1000)]
        public int dreamWielderEssenceChanceHigh = 40;

        [ButtonElement("Dream Wielder Options", "Reset Defaults", "")]
        public void ResetDreamWielder()
        {
            dreamWielderSoulGain = 66;
            dreamWielderEssenceChanceLow = 200;
            dreamWielderEssenceChanceHigh = 40;
        }
        #endregion
        #region Dashmaster Settings
        [BoolElement("Dashmaster Options", "Allows Downward Dash", "")]
        public bool dashmasterDownwardDash = true;

        [InputFloatElement("Dashmaster Options", "Dash Cooldown", 0f, 10f)]
        public float dashmasterDashCooldown = 0.4f;

        [ButtonElement("Dashmaster Options", "Reset Defaults", "")]
        public void ResetDashmaster()
        {
            dashmasterDownwardDash = true;
            dashmasterDashCooldown = 0.4f;
        }
        #endregion
        #region Quick Slash Settings
        [InputFloatElement("Quick Slash Options", "Attack Cooldown", 0f, 2f)]
        public float quickSlashAttackCooldown = 0.25f;

        [ButtonElement("Quick Slash Options", "Reset Defaults", "")]
        public void ResetQuickSlash()
        {
            quickSlashAttackCooldown = 0.25f;
        }
        #endregion
        #region Spell Twister Settings
        [InputIntElement("Spell Twister Options", "Spell Cost", 0, 99)]
        public int spellTwisterSpellCost = 24;

        [ButtonElement("Spell Twister Options", "Reset Defaults", "")]
        public void ResetSpellTwister()
        {
            spellTwisterSpellCost = 24;
        }
        #endregion
        #region Grubberfly's Elegy Settings
        [BoolElement("Grubberfly's Elegy Options", "Ends with Joni's On Damage", "Does taking damage end the effects of Grubberfly's Elegy?")]
        public bool grubberflysElegyJoniBeamDamageBool = true;

        [InputFloatElement("Grubberfly's Elegy Options", "Damage Multiplier", 0f, 5f)]
        public float grubberflysElegyDamageScale = 1.0f;

        [InputIntElement("Grubberfly's Elegy Options", "FotF Increase (%)", 0, 500)]
        public int grubberflysElegyFuryOfTheFallenScaling = 50;

        [InputIntElement("Grubberfly's Elegy Options", "Mark of Pride Size Increase (%)", 0, 500)]
        public int grubberflysElegyMarkOfPrideScale = 35;

        [ButtonElement("Grubberfly's Elegy Options", "Reset Defaults", "")]
        public void ResetGrubberflysElegy()
        {
            grubberflysElegyJoniBeamDamageBool = true;
            grubberflysElegyDamageScale = 1f;
            grubberflysElegyFuryOfTheFallenScaling = 50;
            grubberflysElegyMarkOfPrideScale = 35;
        }
        #endregion
        #region Kingsoul Settings
        [InputIntElement("Kingsoul Options", "Soul Gained", 0, 198)]
        public int kingsoulSoulGain = 4;

        [InputFloatElement("Kingsoul Options", "Soul Timer", 0f, 60f)]
        public float kingsoulSoulTime = 2.0f;

        [ButtonElement("Kingsoul Options", "Reset Defaults", "")]
        public void ResetKingsoul()
        {
            kingsoulSoulGain = 4;
            kingsoulSoulTime = 2.0f;
        }
        #endregion
        #region Sprintmaster Settings
        [InputFloatElement("Sprintmaster Options", "Speed", 0f, 36f)]
        public float sprintmasterSpeed = 10.0f;

        [InputFloatElement("Sprintmaster Options", "Dashmaster Speed", 0f, 36f)]
        public float sprintmasterSpeedCombo = 11.5f;

        [ButtonElement("Sprintmaster Options", "Reset Defaults", "")]
        public void ResetSprintmaster()
        {
            sprintmasterSpeed = 10.0f;
            sprintmasterSpeedCombo = 11.5f;
        }
        #endregion
        #region Dreamshield Settings
        [BoolElement("Dreamshield Options", "Makes Noise On Nail Slash", "")]
        public bool dreamshieldNoise = true;

        [InputFloatElement("Dreamshield Options", "Damage Multiplier", 0, 5f)]
        public float dreamshieldDamageScale = 1.0f;

        [InputFloatElement("Dreamshield Options", "Shield Reformation Time", 0f, 60f)]
        public float dreamshieldReformationTime = 2.0f;

        [InputFloatElement("Dreamshield Options", "Size Scale", 0f, 3f)]
        public float dreamshieldSizeScale = 1.0f;

        [InputFloatElement("Dreamshield Options", "Dream Wielder Size Scale", 0f, 3f)]
        public float dreamshieldDreamWielderSizeScale = 1.15f;

        [InputIntElement("Dreamshield Options", "Rotation Speed", 0, 1000)]
        public int dreamshieldSpeed = 110;

        [InputIntElement("Dreamshield Options", "Focusing Rotation Speed", 0, 1000)]
        public int dreamshieldFocusSpeed = 300;

        [ButtonElement("Dreamshield Options", "Reset Defaults", "")]
        public void ResetDreamshield()
        {
            dreamshieldNoise = true;
            dreamshieldDamageScale = 1.0f;
            dreamshieldReformationTime = 2.0f;
            dreamshieldSizeScale = 1.0f;
            dreamshieldDreamWielderSizeScale = 1.15f;
            dreamshieldSpeed = 110;
            dreamshieldFocusSpeed = 300;
        }
        #endregion
        #region Weaversong Settings
        [SliderIntElement("Weaversong Options", "Weaverling Total", 1, 12)]
        public int weaversongCount = 3;

        [InputIntElement("Weaversong Options", "Damage", 0, 100)]
        public int weaversongDamage = 3;

        [InputFloatElement("Weaversong Options", "Minimum Speed", 0f, 36f)]
        public float weaversongSpeedMin = 6f;

        [InputFloatElement("Weaversong Options", "Maximum Speed", 0f, 36f)]
        public float weaversongSpeedMax = 10f;

        [InputIntElement("Weaversong Options", "Sprintmaster Increase (%)", 0, 500)]
        public int weaversongSpeedSprintmaster = 50;

        [InputIntElement("Weaversong Options", "Grubsong Soul", 0, 198)]
        public int weaversongGrubsongSoul = 3;

        [ButtonElement("Weaversong Options", "Reset Defaults", "")]
        public void ResetWeaversong()
        {
            weaversongCount = 3;
            weaversongSpeedMin = 6f;
            weaversongSpeedMax = 10f;
            weaversongSpeedSprintmaster = 50;
            weaversongDamage = 3;
            weaversongGrubsongSoul = 3;
        }
        #endregion
        #region Grimmchild Settings
        [InputIntElement("Grimmchild Options", "Level 2 Damage", 0, 100)]
        public int grimmchildDamage2 = 5;

        [InputIntElement("Grimmchild Options", "Level 3 Damage", 0, 100)]
        public int grimmchildDamage3 = 8;

        [InputIntElement("Grimmchild Options", "Level 4 Damage", 0, 100)]
        public int grimmchildDamage4 = 11;

        [InputFloatElement("Grimmchild Options", "Attack Cooldown", 0f, 10f)]
        public float grimmchildAttackTimer = 2.0f;

        [ButtonElement("Grimmchild Options", "Reset Defaults", "")]
        public void ResetGrimmchild()
        {
            grimmchildDamage2 = 5;
            grimmchildDamage3 = 8;
            grimmchildDamage4 = 11;
            grimmchildAttackTimer = 2.0f;
        }
        #endregion
        #region Carefree Melody Settings
        [InputIntElement("Carefree Melody Options", "First Chance (X/99)", 0, 99)]
        public int carefreeMelodyChance1 = 10;

        [InputIntElement("Carefree Melody Options", "Second Chance (X/99)", 0, 99)]
        public int carefreeMelodyChance2 = 20;

        [InputIntElement("Carefree Melody Options", "Third Chance (X/99)", 0, 99)]
        public int carefreeMelodyChance3 = 30;

        [InputIntElement("Carefree Melody Options", "Fourth Chance (X/99)", 0, 99)]
        public int carefreeMelodyChance4 = 50;

        [InputIntElement("Carefree Melody Options", "Fifth Chance (X/99)", 0, 99)]
        public int carefreeMelodyChance5 = 70;

        [InputIntElement("Carefree Melody Options", "Sixth Chance (X/99)", 0, 99)]
        public int carefreeMelodyChance6 = 80;

        [InputIntElement("Carefree Melody Options", "Seventh Chance (X/99)", 0, 99)]
        public int carefreeMelodyChance7 = 90;

        [ButtonElement("Carefree Melody Options", "Reset Defaults", "")]
        public void ResetCarefreeMelody()
        {
            carefreeMelodyChance1 = 10;
            carefreeMelodyChance2 = 20;
            carefreeMelodyChance3 = 30;
            carefreeMelodyChance4 = 50;
            carefreeMelodyChance5 = 70;
            carefreeMelodyChance6 = 80;
            carefreeMelodyChance7 = 90;
        }
        #endregion
    }
}
