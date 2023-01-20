using UnityEngine;
using Modding;
using System;
using GlobalEnums;
using HutongGames.PlayMaker;
using HutongGames.PlayMaker.Actions;
using MonoMod.Cil;
using Satchel;
using SFCore.Utils;
using HKMirror;
using HKMirror.Reflection.SingletonClasses;
using HKMirror.Hooks.ILHooks;

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

            #region Notch Costs Init
            On.GameManager.CalculateNotchesUsed += ChangeNotchCosts;
            #endregion

            #region Grubsong Init
            IL.HeroController.TakeDamage += GrubsongSoulChanges;
            #endregion
            #region Stalwart Shell Init
            ILHeroController.orig_Update += (il) =>
            {
                ILCursor cursor = new ILCursor(il).Goto(0);
                cursor.TryGotoNext(i => i.MatchLdfld<HeroController>("RECOIL_DURATION"));
                cursor.GotoNext();
                cursor.EmitDelegate<Func<float, float>>(time => LS.regularRecoil);

                cursor.TryGotoNext(i => i.MatchLdfld<HeroController>("RECOIL_DURATION_STAL"));
                cursor.GotoNext();
                cursor.EmitDelegate<Func<float, float>>(time => LS.stalwartShellRecoil);
            };
            ILHeroController.StartRecoil += (il) =>
            {
                ILCursor cursor = new ILCursor(il).Goto(0);
                cursor.TryGotoNext(i => i.MatchLdfld<HeroController>("INVUL_TIME_STAL"));
                cursor.GotoNext();
                cursor.EmitDelegate<Func<float, float>>(time => LS.stalwartShellInvulnerability);

                cursor.TryGotoNext(i => i.MatchLdfld<HeroController>("INVUL_TIME"));
                cursor.GotoNext();
                cursor.EmitDelegate<Func<float, float>>(time => LS.regularInvulnerability);
            };
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
            #endregion
            #region Quick/Deep Focus Init
            On.HutongGames.PlayMaker.Actions.SetFloatValue.OnEnter += FocusSpeed;
            On.HutongGames.PlayMaker.Actions.SetIntValue.OnEnter += HealAmount;
            On.HutongGames.PlayMaker.Actions.FloatMultiply.OnEnter += DeepFocusScaling;
            #endregion
            #region Lifeblood Heart/Core Init
            ILPlayerData.orig_UpdateBlueHealth += (il) =>
            {
                ILCursor cursor = new ILCursor(il).Goto(0);
                cursor.TryGotoNext(i => i.MatchLdcI4(2));
                cursor.GotoNext();
                cursor.EmitDelegate<Func<int, int>>(Lifeblood => LS.lifebloodHeartLifeblood);

                cursor.TryGotoNext(i => i.MatchLdcI4(4));
                cursor.GotoNext();
                cursor.EmitDelegate<Func<int, int>>(Lifeblood => LS.lifebloodCoreLifeblood);
            };
            #endregion
            #region Defender's Crest Init
            On.HutongGames.PlayMaker.Actions.Wait.OnEnter += DungCloudSettings;
            On.HutongGames.PlayMaker.Actions.SpawnObjectFromGlobalPoolOverTime.OnUpdate += CloudFrequency;
            IL.ShopItemStats.OnEnable += DefendersCrestCostReduction;
            #endregion
            #region Flukenest Init
            On.HutongGames.PlayMaker.Actions.FlingObjectsFromGlobalPool.OnEnter += FlukeCount;
            On.HutongGames.PlayMaker.Actions.Wait.OnEnter += FlukenestDefendersCrestDurationAndDamage;
            IL.SpellFluke.OnEnable += FlukenestEnableHook;
            #endregion
            #region Thorns of Agony Init
            On.HutongGames.PlayMaker.Actions.SetFsmInt.OnEnter += ThornsOfAgonyDamageScale;
            #endregion
            #region Longnail / Mark of Pride Init
            On.PlayMakerFSM.OnEnable += OnFsmEnable;
            On.HutongGames.PlayMaker.Actions.SendMessage.OnEnter += WallSlashSizeScale;
            IL.NailSlash.StartSlash += NailSlashSizeScale;
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
            ILHeroController.OrigDashVector += (il) =>
            {
                ILCursor cursor = new ILCursor(il).Goto(0);

                cursor.TryGotoNext(i => i.MatchLdfld<HeroController>("DASH_SPEED_SHARP"));
                cursor.GotoNext();
                cursor.EmitDelegate<Func<float, float>>(Speed => LS.SharpShadowDashSpeed);
            };
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
            On.HutongGames.PlayMaker.Actions.FloatCompare.OnEnter += ShamanStoneQMegaDamage;
            #endregion
            #region Soul Catcher/Eater Init
            IL.HeroController.SoulGain += SoulCharmChanges;
            #endregion
            #region Glowing Womb Init
            On.HutongGames.PlayMaker.Actions.Wait.OnEnter += GlowingWombSettings;
            On.HutongGames.PlayMaker.Actions.IntCompare.OnEnter += HatchlingSpawnRequirements;
            On.KnightHatchling.OnEnable += HatchlingDamage;
            IL.KnightHatchling.OnEnable += HatchlingFotFSettings;
            #endregion
            #region Fragile Charms Init
            On.HutongGames.PlayMaker.Actions.PlayerDataBoolTest.OnEnter += FragileCharmsBreak;
            On.HutongGames.PlayMaker.Actions.FloatMultiply.OnEnter += StrengthDamageIncrease;
            IL.HealthManager.Die += GreedGeoIncrease;
            #endregion
            #region Nailmaster's Glory Init
            ILHeroController.orig_CharmUpdate += (il) =>
            {
                ILCursor cursor = new ILCursor(il).Goto(0);
                cursor.TryGotoNext(i => i.MatchLdfld<HeroController>("NAIL_CHARGE_TIME_CHARM"));
                cursor.GotoNext();
                cursor.EmitDelegate<Func<float, float>>(time => LS.nailmastersGloryChargeTime);

                cursor.TryGotoNext(i => i.MatchLdfld<HeroController>("NAIL_CHARGE_TIME_DEFAULT"));
                cursor.GotoNext();
                cursor.EmitDelegate<Func<float, float>>(time => LS.regularChargeTime);
            };
            #endregion
            #region Joni's Blessing Init
            ILHeroController.orig_CharmUpdate += (il) =>
            {
                ILCursor cursor = new ILCursor(il).Goto(0);
                cursor.GotoNext(i => i.MatchLdcR4(1.4f));
                cursor.GotoNext();
                cursor.EmitDelegate<Func<float, float>>(scale => 1f + (float)(LS.jonisBlessingScaling / 100f));
            };
            #endregion
            #region Shape Of Unn Init
            On.HutongGames.PlayMaker.Actions.SetFloatValue.OnEnter += SlugSpeeds;
            #endregion
            #region Hiveblood Init
            On.HutongGames.PlayMaker.Actions.FloatCompare.OnEnter += HivebloodTimers;
            #endregion
            #region Dream Wielder Init
            IL.EnemyDreamnailReaction.RecieveDreamImpact += DreamWielderSoul;
            IL.EnemyDeathEffects.EmitEssence += DreamWielderEssence;
            #endregion
            #region Dashmaster Init
            IL.HeroController.HeroDash += DashmasterChanges;
            #endregion
            #region Quick Slash Init
            IL.HeroController.Attack += QuickSlashAttackDuration;
            ILHeroController.orig_DoAttack += (il) =>
            {
                ILCursor cursor = new ILCursor(il).Goto(0);
                cursor.TryGotoNext(i => i.MatchLdfld<HeroController>("ATTACK_COOLDOWN_TIME_CH"));
                cursor.GotoNext();
                cursor.EmitDelegate<Func<float, float>>(time => LS.quickSlashAttackCooldown);

                cursor.TryGotoNext(i => i.MatchLdfld<HeroController>("ATTACK_COOLDOWN_TIME"));
                cursor.GotoNext();
                cursor.EmitDelegate<Func<float, float>>(time => LS.regularAttackCooldown);
            };
            #endregion
            #region Spell Twister Init
            On.HutongGames.PlayMaker.Actions.IntCompare.OnEnter += SpellTwisterSpellCosts;
            #endregion
            #region Grubberfly's Elegy Init
            On.HeroController.TakeDamage += GrubberflysElegyJoniBeam;
            On.HutongGames.PlayMaker.Actions.FloatMultiply.OnEnter += GrubberflysElegyFotFScaling;
            ModHooks.GetPlayerIntHook += GrubberflysElegyDamage;
            IL.HeroController.Attack += GrubberflysSizeScale;
            IL.HeroController.Attack += GrubberflysFotFRequirements;
            #endregion
            #region Kingsoul Init
            On.HutongGames.PlayMaker.Actions.Wait.OnEnter += KingsoulTimer;
            On.HutongGames.PlayMaker.Actions.SendMessageV2.DoSendMessage += KingsoulSoul;
            #endregion
            #region Sprintmaster Init
            IL.HeroController.Move += SprintmasterSpeed;
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
            IL.HeroController.TakeDamage += CarefreeMelodyChances;
            #endregion

            Log("Initialized");
        }
        #endregion

        #region Changes
        #region Notch Cost Changes
        private void ChangeNotchCosts(On.GameManager.orig_CalculateNotchesUsed orig, GameManager self)
        {
            PlayerDataAccess.charmCost_1 = LS.charm1NotchCost;
            PlayerDataAccess.charmCost_2 = LS.charm2NotchCost;
            PlayerDataAccess.charmCost_3 = LS.charm3NotchCost;
            PlayerDataAccess.charmCost_4 = LS.charm4NotchCost;
            PlayerDataAccess.charmCost_5 = LS.charm5NotchCost;
            PlayerDataAccess.charmCost_6 = LS.charm6NotchCost;
            PlayerDataAccess.charmCost_7 = LS.charm7NotchCost;
            PlayerDataAccess.charmCost_8 = LS.charm8NotchCost;
            PlayerDataAccess.charmCost_9 = LS.charm9NotchCost;
            PlayerDataAccess.charmCost_10 = LS.charm10NotchCost;
            PlayerDataAccess.charmCost_11 = LS.charm11NotchCost;
            PlayerDataAccess.charmCost_12 = LS.charm12NotchCost;
            PlayerDataAccess.charmCost_13 = LS.charm13NotchCost;
            PlayerDataAccess.charmCost_14 = LS.charm14NotchCost;
            PlayerDataAccess.charmCost_15 = LS.charm15NotchCost;
            PlayerDataAccess.charmCost_16 = LS.charm16NotchCost;
            PlayerDataAccess.charmCost_17 = LS.charm17NotchCost;
            PlayerDataAccess.charmCost_18 = LS.charm18NotchCost;
            PlayerDataAccess.charmCost_19 = LS.charm19NotchCost;
            PlayerDataAccess.charmCost_20 = LS.charm20NotchCost;
            PlayerDataAccess.charmCost_21 = LS.charm21NotchCost;
            PlayerDataAccess.charmCost_22 = LS.charm22NotchCost;
            PlayerDataAccess.charmCost_23 = LS.charm23NotchCost;
            PlayerDataAccess.charmCost_24 = LS.charm24NotchCost;
            PlayerDataAccess.charmCost_25 = LS.charm25NotchCost;
            PlayerDataAccess.charmCost_26 = LS.charm26NotchCost;
            PlayerDataAccess.charmCost_27 = LS.charm27NotchCost;
            PlayerDataAccess.charmCost_28 = LS.charm28NotchCost;
            PlayerDataAccess.charmCost_29 = LS.charm29NotchCost;
            PlayerDataAccess.charmCost_30 = LS.charm30NotchCost;
            PlayerDataAccess.charmCost_31 = LS.charm31NotchCost;
            PlayerDataAccess.charmCost_32 = LS.charm32NotchCost;
            PlayerDataAccess.charmCost_33 = LS.charm33NotchCost;
            PlayerDataAccess.charmCost_34 = LS.charm34NotchCost;
            PlayerDataAccess.charmCost_35 = LS.charm35NotchCost;
            PlayerDataAccess.charmCost_36 = LS.charm36NotchCost;
            PlayerDataAccess.charmCost_37 = LS.charm37NotchCost;
            PlayerDataAccess.charmCost_38 = LS.charm38NotchCost;
            PlayerDataAccess.charmCost_39 = LS.charm39NotchCost;
            PlayerDataAccess.charmCost_40 = LS.charm40NotchCost;
        }
        #endregion

        #region Grubsong Changes
        private void GrubsongSoulChanges(ILContext il)
        {
            ILCursor cursor = new ILCursor(il).Goto(0);
            cursor.TryGotoNext(i => i.MatchLdfld<HeroController>("GRUB_SOUL_MP_COMBO"));
            cursor.GotoNext();
            cursor.EmitDelegate<Func<int, int>>(soul => LS.grubsongDamageSoulCombo);

            cursor.TryGotoNext(i => i.MatchLdfld<HeroController>("GRUB_SOUL_MP"));
            cursor.GotoNext();
            cursor.EmitDelegate<Func<int, int>>(soul => LS.grubsongDamageSoul);
        }
        #endregion
        #region Stalwart Shell Changes
        #endregion
        #region Baldur Shell Changes
        private void BaldurShellKnockback(On.BeginRecoil.orig_OnEnter orig, BeginRecoil self)
        {
            if (self.Fsm.GameObject.name.StartsWith("Hit ") && self.Fsm.Name == "push_enemy" && self.State.Name == "Send Event")
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
        private void FotFJonisRequirements(On.HutongGames.PlayMaker.Actions.BoolAllTrue.orig_OnEnter orig, BoolAllTrue self)
        {
            if (self.Fsm.GameObject.name == "Charm Effects" && self.Fsm.Name == "Fury" && self.State.Name == "Check HP")
            {
                self.sendEvent = LS.furyOfTheFallenJonis ? null : FsmEvent.GetFsmEvent("CANCEL");
            }

            orig(self);
        }
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
        #endregion
        #region Quick/Deep Focus Changes
        private void FocusSpeed(On.HutongGames.PlayMaker.Actions.SetFloatValue.orig_OnEnter orig, SetFloatValue self)
        {
            if (self.Fsm.GameObject.name == "Knight" && self.Fsm.Name == "Spell Control" && self.State.Name == "Set Focus Speed")
            {
                if (self.floatValue.Name == "Time Per MP Drain UnCH")
                {
                    self.floatValue.Value = LS.regularFocusTime / 33f;
                }
                else if (self.floatValue.Name == "Time Per MP Drain CH")
                {
                    self.floatValue.Value = LS.quickFocusFocusTime / 33f;
                }
            }

            orig(self);
        }
        private void HealAmount(On.HutongGames.PlayMaker.Actions.SetIntValue.orig_OnEnter orig, SetIntValue self)
        {
            if (self.Fsm.GameObject.name == "Knight" && self.Fsm.Name == "Spell Control" && self.State.Name.StartsWith("Set HP Amount"))
            {
                if (self.State.ActiveActionIndex == 0)
                {
                    self.intValue.Value = LS.regularFocusHealing;
                }
                else if (self.State.ActiveActionIndex == 2)
                {
                    self.intValue.Value = LS.deepFocusHealing;
                }
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
        #endregion
        #region Defender's Crest Changes
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
                self.Fsm.GameObject.GetComponent<DamageEffectTicker>().SetDamageInterval(PlayerDataAccess.equippedCharm_19 ? LS.flukenestDefendersCrestShamanStoneDamageRate : LS.flukenestDefendersCrestDamageRate);
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
            if (self.Fsm.GameObject.name.StartsWith("Hit ") && self.Fsm.Name == "set_thorn_damage" && self.State.Name == "Set")
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
            if (self.Fsm.GameObject.name == "Charm Effects" && self.Fsm.Name == "Slash Size Modifiers" && self.gameObject.GameObject.Name == "Wall Slash" && self.State.Name.StartsWith("Equipped"))
            {
                self.functionCall.BoolParameter.Value = LS.longnailMarkOfPrideWallSlash;
            }

            orig(self);
        }
        #endregion
        #region Steady Body Changes
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
            if (self.Fsm.GameObject.name == "Charm Effects" && self.Fsm.Name == "Enemy Recoil Up")
            {
                if (self.State.Name == "Equipped")
                {
                    // Wall Slash
                    if (self.gameObject.GameObject.Name == "Wall Slash")
                    {
                        self.setValue.Value = LS.heavyBlowWallSlash ? LS.heavyBlowSlashRecoil : 1.0f;
                    }

                    // Regular Slashes
                    else if (!self.gameObject.GameObject.Name.StartsWith("Great") && !self.gameObject.GameObject.Name.StartsWith("Hit "))
                    {
                        self.setValue.Value = LS.heavyBlowSlashRecoil;
                    }

                    // Great Slash
                    else
                    {
                        self.setValue.Value = LS.heavyBlowGreatSlashRecoil;
                    }
                }
                else if (self.State.Name == "Unequipped")
                {
                    // Wall Slash
                    if (self.gameObject.GameObject.Name == "Wall Slash")
                    {
                        self.setValue.Value = LS.heavyBlowWallSlash ? LS.regularSlashRecoil : 1.0f;
                    }

                    // Regular Slashes
                    else if (!self.gameObject.GameObject.Name.StartsWith("Great") && !self.gameObject.GameObject.Name.StartsWith("Hit "))
                    {
                        self.setValue.Value = LS.regularSlashRecoil;
                    }

                    // Great Slash
                    else
                    {
                        self.setValue.Value = LS.regularGreatSlashRecoil;
                    }
                }
            }

            // Cyclone Slash
            else if (self.Fsm.GameObject.name == "Cyclone Slash" && self.Fsm.Name == "Control Collider" && self.State.Name == "Init")
            {
                self.setValue.Value = LS.heavyBlowCycloneSlash ? (PlayerDataAccess.equippedCharm_15 ? LS.heavyBlowCycloneSlashRecoil : LS.regularCycloneSlashRecoil) : 1.0f;
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
                else if (self.State.Name == "Deep")
                {
                    self.Fsm.GameObject.transform.Find("Pt Deep").gameObject.GetComponent<ParticleSystem>().startLifetime = LS.sporeShroomCloudDuration;
                }
            }
        }
        #endregion
        #region Shaman Stone Changes
        private void ShamanStoneVengefulSpiritScaling(On.HutongGames.PlayMaker.Actions.SetScale.orig_OnEnter orig, SetScale self)
        {
            if (self.Fsm.GameObject.name == "Fireball(Clone)" && self.Fsm.Name == "Fireball Control" && self.State.Name == "Set Damage")
            {
                if (self.State.ActiveActionIndex == 0)
                {
                    self.x.Value = LS.regularVSSizeScaleX;
                    self.y.Value = LS.regularVSSizeScaleY;
                }
                else if (self.State.ActiveActionIndex == 6)
                {
                    self.x.Value = LS.shamanStoneVSSizeScaleX;
                    self.y.Value = LS.shamanStoneVSSizeScaleY;
                }
            }

            // Shade Soul
            else if (self.Fsm.GameObject.name == "Fireball2 Spiral(Clone)" && self.Fsm.Name == "Fireball Control" && self.State.Name == "Set Damage" && self.State.ActiveActionIndex == 0)
            {
                if (self.State.ActiveActionIndex == 0)
                {
                    self.x.Value = PlayerDataAccess.equippedCharm_19 ? 1.8f : LS.regularSSSizeScaleX * 1.8f;
                    self.y.Value = PlayerDataAccess.equippedCharm_19 ? 1.8f : LS.regularSSSizeScaleY * 1.8f;
                }
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
                else if (self.floatVariable.Name == "Y Scale")
                {
                    self.multiplyBy.Value = LS.shamanStoneSSSizeScaleY;
                }
            }

            orig(self);
        }
        private void ShamanStoneDamage(On.HutongGames.PlayMaker.Actions.SetFsmInt.orig_OnEnter orig, SetFsmInt self)
        {
            if (self.Fsm.GameObject.name == "Fireball(Clone)" && self.Fsm.Name == "Fireball Control" && self.State.Name == "Set Damage")
            {
                if (self.State.ActiveActionIndex == 2)
                {
                    self.setValue.Value = LS.regularVSDamage;
                }
                else if (self.State.ActiveActionIndex == 4)
                {
                    self.setValue.Value = LS.shamanStoneVSDamage;
                }
            }
            else if (self.Fsm.GameObject.name == "Fireball2 Spiral(Clone)" && self.Fsm.Name == "Fireball Control" && self.State.Name == "Set Damage")
            {
                if (self.State.ActiveActionIndex == 3)
                {
                    self.setValue.Value = LS.regularSSDamage;
                }
                else if (self.State.ActiveActionIndex == 5)
                {
                    self.setValue.Value = LS.shamanStoneSSDamage;
                }
            }
            else if (self.Fsm.Name == "Set Damage" && self.State.Name == "Set Damage")
            {
                if (self.Fsm.GameObject.transform.parent.gameObject.name == "Scr Heads")
                {
                    if (self.State.ActiveActionIndex == 0)
                    {
                        self.setValue.Value = LS.regularHWDamage;
                    }
                    else if (self.State.ActiveActionIndex == 2)
                    {
                        self.setValue.Value = LS.shamanStoneHWDamage;
                    }
                }
                else if (self.Fsm.GameObject.transform.parent.gameObject.name == "Scr Heads 2")
                {
                    if (self.State.ActiveActionIndex == 0)
                    {
                        self.setValue.Value = LS.regularASDamage;
                    }
                    else if (self.State.ActiveActionIndex == 2)
                    {
                        self.setValue.Value = LS.shamanStoneASDamage;
                    }
                }
                else if (self.Fsm.GameObject.transform.parent.gameObject.name == "Q Slam")
                {
                    if (self.State.ActiveActionIndex == 0)
                    {
                        self.setValue.Value = LS.regularDDiveDamage;
                    }
                    else if (self.State.ActiveActionIndex == 2)
                    {
                        self.setValue.Value = LS.shamanStoneDDiveDamage;
                    }
                }
                else if (self.Fsm.GameObject.transform.parent.gameObject.name == "Q Slam 2")
                {
                    if (self.Fsm.GameObject.name == "Hit L")
                    {
                        if (self.State.ActiveActionIndex == 0)
                        {
                            self.setValue.Value = LS.regularDDarkDamageL;
                        }
                        else if (self.State.ActiveActionIndex == 2)
                        {
                            self.setValue.Value = LS.shamanStoneDDarkDamageL;
                        }
                    }
                    else if (self.Fsm.GameObject.name == "Hit R")
                    {
                        if (self.State.ActiveActionIndex == 0)
                        {
                            self.setValue.Value = LS.regularDDarkDamageR;
                        }
                        else if (self.State.ActiveActionIndex == 2)
                        {
                            self.setValue.Value = LS.shamanStoneDDarkDamageR;
                        }
                    }
                }
                else if (self.Fsm.GameObject.name == "Q Fall Damage")
                {
                    if (self.State.ActiveActionIndex == 0)
                    {
                        self.setValue.Value = LS.regularDiveDamage;
                    }
                    else if (self.State.ActiveActionIndex == 2)
                    {
                        self.setValue.Value = LS.shamanStoneDiveDamage;
                    }
                }
            }

            orig(self);
        }
        private void ShamanStoneQMegaDamage(On.HutongGames.PlayMaker.Actions.FloatCompare.orig_OnEnter orig, FloatCompare self)
        {
            if (self.Fsm.GameObject.name == "Q Mega" && self.Fsm.Name == "Hit Box Control" && self.State.Name == "Check Scale")
            {
                self.Fsm.GameObject.transform.Find("Hit L").gameObject.LocateMyFSM("damages_enemy").GetFsmIntVariable("damageDealt").Value = PlayerDataAccess.equippedCharm_19 ? LS.shamanStoneDDarkDamageMega : LS.regularDDarkDamageMega;
                self.Fsm.GameObject.transform.Find("Hit R").gameObject.LocateMyFSM("damages_enemy").GetFsmIntVariable("damageDealt").Value = PlayerDataAccess.equippedCharm_19 ? LS.shamanStoneDDarkDamageMega : LS.regularDDarkDamageMega;
            }

            orig(self);
        }
        #endregion
        #region Soul Catcher/Eater Changes
        private void SoulCharmChanges(ILContext il)
        {
            ILCursor cursor = new ILCursor(il).Goto(0);

            // Regular
            cursor.TryGotoNext(i => i.MatchLdcI4(11));
            cursor.GotoNext();
            cursor.EmitDelegate<Func<int, int>>(soul => LS.regularSoul);

            // Soul Catcher
            cursor.TryGotoNext(i => i.MatchLdcI4(3));
            cursor.GotoNext();
            cursor.EmitDelegate<Func<int, int>>(soul => LS.soulCatcherSoul);

            // Soul Eater
            cursor.TryGotoNext(i => i.MatchLdcI4(8));
            cursor.GotoNext();
            cursor.EmitDelegate<Func<int, int>>(soul => LS.soulEaterSoul);

            // Regular Reserves
            cursor.TryGotoNext(i => i.MatchLdcI4(6));
            cursor.GotoNext();
            cursor.EmitDelegate<Func<int, int>>(soul => LS.regularReservesSoul);

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
            cursor.EmitDelegate<Func<int, int>>(health => (PlayerDataAccess.health <= LS.furyOfTheFallenHealth) ? 1 : 0);

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
            if (self.Fsm.GameObject.name == "Hero Death" && self.Fsm.Name == "Hero Death Anim" && self.State.Name.StartsWith("Break Glass ") && self.boolName.Value.EndsWith("_unbreakable"))
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
        #endregion
        #region Joni's Blessing Changes
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
            if (self.Fsm.GameObject.name == "Health" && self.Fsm.Name == "Hive Health Regen" && self.State.Name.StartsWith("Recover "))
            {
                self.float2.Value = LS.hivebloodTimer / 2f;
            }
            else if (self.Fsm.GameObject.name == "Blue Health Hive(Clone)" && self.Fsm.Name == "blue_health_display" && self.State.Name.StartsWith("Regen "))
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

            // Regular Soul
            cursor.TryGotoNext(i => i.MatchLdcI4(33));
            cursor.GotoNext();
            cursor.EmitDelegate<Func<int, int>>(soul => LS.regularDreamSoul);

            // Charm Soul
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

            // Dashmaster Dash Cooldown
            cursor.TryGotoNext(i => i.MatchLdfld<HeroController>("DASH_COOLDOWN_CH"));
            cursor.GotoNext();
            cursor.EmitDelegate<Func<float, float>>(cooldown => LS.dashmasterDashCooldown);

            // Regular Dash Cooldown
            cursor.TryGotoNext(i => i.MatchLdfld<HeroController>("DASH_COOLDOWN"));
            cursor.GotoNext();
            cursor.EmitDelegate<Func<float, float>>(cooldown => LS.regularDashCooldown);
        }
        #endregion
        #region Quick Slash Changes
        private void QuickSlashAttackCooldown(ILContext il)
        {
            ILCursor cursor = new ILCursor(il).Goto(0);
            cursor.TryGotoNext(i => i.MatchLdfld<HeroController>("ATTACK_COOLDOWN_TIME_CH"));
            cursor.GotoNext();
            cursor.EmitDelegate<Func<float, float>>(time => LS.quickSlashAttackCooldown);

            cursor.TryGotoNext(i => i.MatchLdfld<HeroController>("ATTACK_COOLDOWN_TIME"));
            cursor.GotoNext();
            cursor.EmitDelegate<Func<float, float>>(time => LS.regularAttackCooldown);
        }
        private void QuickSlashAttackDuration(ILContext il)
        {
            ILCursor cursor = new ILCursor(il).Goto(0);
            cursor.TryGotoNext(i => i.MatchLdfld<HeroController>("ATTACK_DURATION_CH"));
            cursor.GotoNext();
            cursor.EmitDelegate<Func<float, float>>(time => Mathf.Min(0.35f, LS.quickSlashAttackCooldown + 0.03f));

            cursor.TryGotoNext(i => i.MatchLdfld<HeroController>("ATTACK_DURATION"));
            cursor.GotoNext();
            cursor.EmitDelegate<Func<float, float>>(time => Mathf.Min(0.41f, LS.regularAttackCooldown - 0.06f));
        }
        #endregion
        #region Spell Twister Changes
        private void SpellTwisterSpellCosts(On.HutongGames.PlayMaker.Actions.IntCompare.orig_OnEnter orig, IntCompare self)
        {
            if (self.Fsm.GameObject.name == "Knight" && self.Fsm.Name == "Spell Control" && self.State.Name.StartsWith("Can Cast?"))
            {
                self.integer2.Value = PlayerDataAccess.equippedCharm_33 ? LS.spellTwisterSpellCost : LS.regularSpellCost;
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
            joniBeamChecker = HeroControllerR.joniBeam;

            orig(self, go, damageSide, damageAmount, hazardType);

            HeroControllerR.joniBeam = (joniBeamChecker == HeroControllerR.joniBeam) ? joniBeamChecker : !LS.grubberflysElegyJoniBeamDamageBool;
        }
        private void GrubberflysElegyFotFScaling(On.HutongGames.PlayMaker.Actions.FloatMultiply.orig_OnEnter orig, FloatMultiply self)
        {
            if (self.Fsm.GameObject.name.StartsWith("Grubberfly Beam") && self.Fsm.Name == "Control" && self.State.Name == "Fury Multiplier")
            {
                self.multiplyBy.Value = 1f + (float)(LS.grubberflysElegyFuryOfTheFallenScaling / 100f);
            }

            orig(self);
        }
        private int GrubberflysElegyDamage(string name, int orig)
        {
            if (name == "beamDamage")
            {
                return (int)(orig * LS.grubberflysElegyDamageScale);
            }
            return orig;
        }
        private void GrubberflysFotFRequirements(ILContext il)
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
                cursor.EmitDelegate<Func<int, int>>(health => (PlayerDataAccess.health <= LS.furyOfTheFallenHealth) ? 1 : 0);
            }
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
            cursor.TryGotoNext(i => i.MatchLdfld<HeroController>("WALK_SPEED"));
            cursor.GotoNext();
            cursor.EmitDelegate<Func<float, float>>(speed => LS.regularWalkSpeed);

            cursor.TryGotoNext(i => i.MatchLdfld<HeroController>("RUN_SPEED_CH_COMBO"));
            cursor.GotoNext();
            cursor.EmitDelegate<Func<float, float>>(speed => LS.sprintmasterSpeedCombo);

            cursor.TryGotoNext(i => i.MatchLdfld<HeroController>("RUN_SPEED_CH"));
            cursor.GotoNext();
            cursor.EmitDelegate<Func<float, float>>(speed => LS.sprintmasterSpeed);

            cursor.TryGotoNext(i => i.MatchLdfld<HeroController>("RUN_SPEED"));
            cursor.GotoNext();
            cursor.EmitDelegate<Func<float, float>>(speed => LS.regularSpeed);
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
        #endregion
    }
}
