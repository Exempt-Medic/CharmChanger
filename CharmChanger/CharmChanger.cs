using GlobalEnums;
using Modding;
using System;
using HutongGames.PlayMaker.Actions;
using HutongGames.PlayMaker;
using UnityEngine;
using System.Collections;
using SFCore.Utils;
using MonoMod.RuntimeDetour;
using System.Reflection;
using MonoMod.Cil;

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
            On.HutongGames.PlayMaker.Actions.SetFsmFloat.OnEnter += OnSetFsmFloatAction;
            On.HutongGames.PlayMaker.Actions.FloatMultiply.OnEnter += OnFloatMultiplyAction;
            ilHCAttack = new ILHook(HCAttack, HCAttackHook);
            ilHatchlingEnable = new ILHook(Hatchling, HatchlingEnableHook);
            #endregion
            #region Quick Focus Init
            On.HutongGames.PlayMaker.Actions.SetFloatValue.OnEnter += OnSetFloatValueAction;
            #endregion
            #region Lifeblood Heart/Core Init
            ilorigUpdateBlueHealth = new ILHook(origUpdateBlueHealth, OrigUpdateBlueHealthHook);
            #endregion
            #region Defender's Crest Init
            ilShopItemStatsEnable = new ILHook(shopItemStats, ShopItemStatsHook);
            On.HutongGames.PlayMaker.Actions.Wait.OnEnter += OnWaitAction;
            On.HutongGames.PlayMaker.Actions.SpawnObjectFromGlobalPoolOverTime.OnUpdate += OnSpawnObjectFromGlobalPoolOverTimeAction;
            On.HutongGames.PlayMaker.Actions.ActivateGameObject.OnEnter += OnActivateGameObjectAction;
            On.HutongGames.PlayMaker.Actions.SetScale.OnEnter += OnSetScaleAction;
            On.KnightHatchling.OnEnable += OnHatchlingEnable;
            On.HutongGames.PlayMaker.Actions.CallMethodProper.OnEnter += OnCallMethodProperActionDC;
            #endregion
            #region Flukenest Init
            On.HutongGames.PlayMaker.Actions.FlingObjectsFromGlobalPool.OnEnter += OnFlingObjectsFromGlobalPoolAction;
            ilflukenestEnable = new ILHook(flukenestEnable, FlukenestEnableHook);
            #endregion
            #region Thorns of Agony Init
            On.HutongGames.PlayMaker.Actions.SetFsmInt.OnEnter += OnSetFsmIntAction;
            #endregion
            #region Longnail / Mark of Pride Init
            On.HeroController.Attack += OnHCAttack;
            On.PlayMakerFSM.OnEnable += OnFsmEnable;
            On.HutongGames.PlayMaker.Actions.SendMessage.OnEnter += OnSendMessageAction;
            ilNailSlashStart = new ILHook(nailSlash, NailSlashStartHook);
            #endregion
            #region Heavy Blow Init
            On.HutongGames.PlayMaker.Actions.SetFsmFloat.OnEnter += OnSetFsmFloatActionHeavyBlow;
            On.PlayMakerFSM.OnEnable += OnFsmEnableHeavyBlow;
            On.HutongGames.PlayMaker.Actions.IntOperator.OnEnter += OnIntOperatorAction;
            On.HutongGames.PlayMaker.Actions.IntCompare.OnEnter += OnIntCompareActionHeavyBlow;
            #endregion
            #region Sharp Shadow Init

            #endregion

            Log("Initialized");
        }
        #endregion

        #region Fury of the Fallen IL Hooks
        private static readonly MethodInfo HCAttack = typeof(HeroController).GetMethod("Attack", BindingFlags.Public | BindingFlags.Instance);
        private ILHook? ilHCAttack;

        private static readonly MethodInfo Hatchling = typeof(KnightHatchling).GetMethod("OnEnable", BindingFlags.NonPublic | BindingFlags.Instance);
        private ILHook? ilHatchlingEnable;
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
        // Invulnerability time and inaction time
        private IEnumerator OnHCStartRecoil(On.HeroController.orig_StartRecoil orig, HeroController self, CollisionSide impactSide, bool spawnDamageEffect, int damageAmount)
        {
            self.INVUL_TIME_STAL = LS.stalwartShellInvulnerability / 100f;
            self.RECOIL_DURATION_STAL = LS.stalwartShellRecoil / 100f;

            return orig(self, impactSide, spawnDamageEffect, damageAmount);
        }
        #endregion
        #region Baldur Shell Changes
        // Knockback
        private void OnBeginRecoilAction(On.BeginRecoil.orig_OnEnter orig, BeginRecoil self)
        {
            if (self.Fsm.GameObject.name.Contains("Hit ") && self.Fsm.Name == "push_enemy" && self.State.Name == "Send Event")
            {
                self.attackMagnitude = LS.baldurShellKnockback;
            }
            orig(self);
        }
        // Blocks
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
        // HP Requirement
        private void OnIntCompareAction(On.HutongGames.PlayMaker.Actions.IntCompare.orig_OnEnter orig, IntCompare self)
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
        private void OnBoolAllTrueAction(On.HutongGames.PlayMaker.Actions.BoolAllTrue.orig_OnEnter orig, BoolAllTrue self)
        {
            if (self.Fsm.GameObject.name == "Charm Effects" && self.Fsm.Name == "Fury" && self.State.Name == "Check HP")
            {
                self.sendEvent = (LS.furyOfTheFallenJonis) ? null : FsmEvent.GetFsmEvent("CANCEL");
            }

            orig(self);
        }

        // Scaling (regular attacks)
        private void OnSetFsmFloatAction(On.HutongGames.PlayMaker.Actions.SetFsmFloat.orig_OnEnter orig, SetFsmFloat self)
        {
            if (self.Fsm.GameObject.name == "Charm Effects" && self.Fsm.Name == "Fury" && self.State.Name == "Activate")
            {
                self.setValue.Value = 1f + (LS.furyOfTheFallenScaling / 100f);
            }

            orig(self);
        }

        private void OnFloatMultiplyAction(On.HutongGames.PlayMaker.Actions.FloatMultiply.orig_OnEnter orig, FloatMultiply self)
        {
            // Scaling (Nail Arts)
            if (self.Fsm.Name == "nailart_damage" && self.State.Name == "Fury?")
            {
                self.multiplyBy.Value = 1f + (LS.furyOfTheFallenScaling / 100f);
            }

            // Scaling (Grubberfly's Elegy)
            else if (self.Fsm.GameObject.name.Contains("Grubberfly Beam") && self.Fsm.Name == "Control" && self.State.Name == "Fury Multiplier")
            {
                self.multiplyBy.Value = 1f + (LS.furyOfTheFallenGrubberflysScaling / 100f);
            }

            orig(self);
        }

        // Grubberfly's HP Requirement
        private void HCAttackHook(ILContext il)
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
        private void HatchlingEnableHook(ILContext il)
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
            cursor.EmitDelegate<Func<int, int>>(HatchlingBonusDamage => LS.furyOfTheFallenHatchlingDamage);
        }
        #endregion
        #region Quick Focus Changes
        private void OnSetFloatValueAction(On.HutongGames.PlayMaker.Actions.SetFloatValue.orig_OnEnter orig, SetFloatValue self)
        {
            if (self.Fsm.GameObject.name == "Knight" && self.Fsm.Name == "Spell Control")
            {
                // Shape of Unn Speed
                if (self.State.Name == "Slug Speed")
                {
                    if (self.State.ActiveActionIndex == 6)
                    {
                        self.floatValue.Value = -LS.quickFocusShapeOfUnnSpeed;
                    }
                    else if (self.State.ActiveActionIndex == 7)
                    {
                        self.floatValue.Value = LS.quickFocusShapeOfUnnSpeed;
                    }
                }

                // Focus Time
                else if (self.State.Name == "Set Focus Speed" && self.floatValue.Name == "Time Per MP Drain CH")
                {
                    self.floatValue.Value = LS.quickFocusFocusTime / 33000f;
                }
            }

            orig(self);
        }

        #endregion
        #region Lifeblood Heart/Core Changes
        // Lifeblood Granted
        private void OrigUpdateBlueHealthHook(ILContext il)
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
        // Defender's Crest Needs More Testing
        #region Defender's Crest Changes
        // Discount
        private void ShopItemStatsHook(ILContext il)
        {
            ILCursor cursor = new ILCursor(il).Goto(0);

            cursor.TryGotoNext(i => i.MatchLdcR4(0.8f));
            cursor.GotoNext();
            cursor.EmitDelegate<Func<float, float>>(Discount => (100 - (float)LS.defendersCrestDiscount) / 100f);
        }

        private void OnWaitAction(On.HutongGames.PlayMaker.Actions.Wait.orig_OnEnter orig, Wait self)
        {
            // Defender's Crest Cloud
            if (self.Fsm.GameObject.name == "Knight Dung Trail(Clone)" && self.Fsm.Name == "Control" && self.State.Name == "Wait")
            {
                // Duration
                self.time.Value = LS.defendersCrestCloudDuration;

                // Damage Rate
                self.Fsm.GameObject.GetComponent<DamageEffectTicker>().damageInterval = LS.defendersCrestDamageRate / 1000f;
            }

            // Spore Shroom Cloud
            else if (self.Fsm.GameObject.name == "Knight Dung Cloud(Clone)" && self.Fsm.Name == "Control" && self.State.Name == "Wait")
            {
                // Duration
                self.time.Value = LS.defendersCrestSporeShroomCloudDuration;

                // Damage Rate
                self.Fsm.GameObject.GetComponent<DamageEffectTicker>().damageInterval = LS.defendersCrestSporeShroomDamageRate / 1000f;
            }

            // Glowing Womb Cloud
            else if (self.Fsm.GameObject.name == "Dung Explosion(Clone)" && self.Fsm.Name == "Explosion Control" && self.State.Name == "Explode")
            {
                // Duration
                self.time.Value = LS.defendersCrestGlowingWombDuration;

                // Damage Rate
                self.Fsm.GameObject.GetComponent<DamageEffectTicker>().damageInterval = LS.defendersCrestGlowingWombDamageRate / 1000f;
            }

            // Flukenest Cloud
            else if (self.Fsm.GameObject.name == "Knight Dung Cloud(Clone)" && self.Fsm.Name == "Control" && self.State.Name == "Collider On")
            {
                // Duration
                self.time.Value = LS.defendersCrestFlukenestDuration;
            }

            orig(self);
        }
        // Cloud Frequency
        private void OnSpawnObjectFromGlobalPoolOverTimeAction(On.HutongGames.PlayMaker.Actions.SpawnObjectFromGlobalPoolOverTime.orig_OnUpdate orig, SpawnObjectFromGlobalPoolOverTime self)
        {
            if (self.Fsm.GameObject.name == "Dung" && self.Fsm.Name == "Control" && self.State.Name == "Equipped")
            {
                self.frequency.Value = LS.defendersCrestCloudFrequency / 100f;
            }

            orig(self);
        }

        // Hatchling Base Damage
        private void OnHatchlingEnable(On.KnightHatchling.orig_OnEnable orig, KnightHatchling self)
        {
            self.dungDetails.damage = LS.defendersCrestGlowingWombDamage;

            orig(self);
        }

        // DC + Flukenest Damage Interval
        private void OnCallMethodProperActionDC(On.HutongGames.PlayMaker.Actions.CallMethodProper.orig_OnEnter orig, CallMethodProper self)
        {
            if (self.Fsm.Name.Contains("Spell Fluke Dung Lv") && self.Fsm.Name == "Control")
            {
                // Regular
                if (self.State.Name == "Normal")
                {
                    self.parameters = new FsmVar[1] { new FsmVar(typeof(float)) { floatValue = LS.defendersCrestFlukenestDamageRate / 1000f } };
                }

                else if (self.State.Name == "Spell Up")
                {
                    self.parameters = new FsmVar[1] { new FsmVar(typeof(float)) { floatValue = LS.defendersCrestFlukenestShamanStoneDamageRate / 1000f } };
                }
            }
            orig(self);
        }

        // Defender's Crest + Spore Shroom Cloud Visuals Size (math is hard)
        private void OnActivateGameObjectAction(On.HutongGames.PlayMaker.Actions.ActivateGameObject.orig_OnEnter orig, ActivateGameObject self)
        {
            orig(self);

            //if (self.Fsm.GameObject.name == "Knight Dung Cloud(Clone)" && self.Fsm.Name == "Control")
            //{
            //    var deactive = self.gameObject.GameObject.Value.name;
            //    if (deactive == "Pt Normal")
            //    {
            //        self.Fsm.GameObject.transform.Find("Pt Deep").gameObject.GetComponent<ParticleSystem>().startSpeed = LS.defendersCrestSporeShroomDeepFocusCloudSize;
            //    }

            //    else
            //    {
            //        self.Fsm.GameObject.transform.Find("Pt Normal").gameObject.GetComponent<ParticleSystem>().startSpeed = LS.defendersCrestSporeShroomCloudSize;
            //    }
            //}
        }

        // Defender's Crest + Spore Shroom Cloud Hurtbox Size (math is hard)
        private void OnSetScaleAction(On.HutongGames.PlayMaker.Actions.SetScale.orig_OnEnter orig, SetScale self)
        {
        //    if (self.Fsm.GameObject.name == "Knight Dung Cloud(Clone)" && self.Fsm.Name == "Control")
        //    {
        //        if (self.State.Name == "Pt Normal")
        //        {
        //            self.x.Value = Mathf.Max(26f, LS.defendersCrestSporeShroomCloudSize) * 0.04375f - 0.75f;
        //            self.y.Value = Mathf.Max(26f, LS.defendersCrestSporeShroomCloudSize) * 0.04375f - 0.75f;
        //        }

        //        else
        //        {
        //            self.x.Value = Mathf.Max(26f, LS.defendersCrestSporeShroomDeepFocusCloudSize) * 0.04375f - 0.75f;
        //            self.y.Value = Mathf.Max(26f, LS.defendersCrestSporeShroomDeepFocusCloudSize) * 0.04375f - 0.75f;
        //        }
        //    }

            orig(self);
        }

        #endregion
        #region Flukenest Changes
        // Flukes Total
        private void OnFlingObjectsFromGlobalPoolAction(On.HutongGames.PlayMaker.Actions.FlingObjectsFromGlobalPool.orig_OnEnter orig, FlingObjectsFromGlobalPool self)
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
        private void OnSetFsmIntAction(On.HutongGames.PlayMaker.Actions.SetFsmInt.orig_OnEnter orig, SetFsmInt self)
        {
            if (self.Fsm.GameObject.name.Contains("Hit ") && self.Fsm.Name == "set_thorn_damage" && self.State.Name == "Set")
            {
                self.setValue.Value = (int)(self.setValue.Value * LS.thornsOfAgonyDamageMultiplier);
            }

            orig(self);
        }
        #endregion
        #region Longnail / Mark of Pride Changes
        private void NailSlashStartHook(ILContext il)
        {
            ILCursor cursor = new ILCursor(il).Goto(0);

            // Longnail + Mark of Pride Scaling
            while (cursor.TryGotoNext(i => i.MatchLdcR4(1.4f)))
            {
                cursor.GotoNext();
                cursor.EmitDelegate<Func<float, float>>(scale => LS.longnailMarkOfPrideScale / 100f);
            }

            // Mark of Pride Scaling
            while (cursor.TryGotoNext(i => i.MatchLdcR4(1.25f)))
            {
                cursor.GotoNext();
                cursor.EmitDelegate<Func<float, float>>(scale => LS.markOfPrideScale / 100f);
            }

            // Longnail Scaling
            while (cursor.TryGotoNext(i => i.MatchLdcR4(1.15f)))
            {
                cursor.GotoNext();
                cursor.EmitDelegate<Func<float, float>>(scale => LS.longnailScale / 100f);
            }
        }

        // Grubberfly's Size Scale
        private void OnHCAttack(On.HeroController.orig_Attack orig, HeroController self, AttackDirection attackDir)
        {
            ReflectionHelper.SetField<HeroController, float>(self, "MANTIS_CHARM_SCALE", LS.markOfPrideGrubberflysScale / 100f);

            orig(self, attackDir);
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

        // Wall Slash Execution
        private void OnSendMessageAction(On.HutongGames.PlayMaker.Actions.SendMessage.orig_OnEnter orig, SendMessage self)
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
        // Heavy Blow Needs Wall Slash Fixes
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

        // Knockback
        private void OnSetFsmFloatActionHeavyBlow(On.HutongGames.PlayMaker.Actions.SetFsmFloat.orig_OnEnter orig, SetFsmFloat self)
        {
            if (self.Fsm.GameObject.name == "Charm Effects" && self.Fsm.Name == "Enemy Recoil Up" && self.State.Name == "Equipped")
            {
                // Regular Slashes
                if (!self.gameObject.GameObject.Name.Contains("Great") && !self.gameObject.GameObject.Name.Contains("Hit "))
                {
                    self.setValue.Value = LS.heavyBlowSlashRecoil;
                }

                // Wall Slash
                else if (self.gameObject.GameObject.Name == "Wall Slash")
                {
                    self.setValue.Value = (LS.heavyBlowWallSlash) ? LS.heavyBlowSlashRecoil : 1f;
                }

                // Great Slash
                else
                {
                    self.setValue.Value = LS.heavyBlowGreatSlashRecoil;
                }
            }

            // Cyclone Slash
            else if (self.Fsm.GameObject.name == "Cyclone Slash" && self.Fsm.Name == "Control Collider")
            {
                self.setValue.Value = (LS.heavyBlowCycloneSlash && PlayerData.instance.equippedCharm_15) ? LS.heavyBlowCycloneSlashRecoil : 1f;
            }

            orig(self);
        }

        // Stagger Changes
        private void OnIntOperatorAction(On.HutongGames.PlayMaker.Actions.IntOperator.orig_OnEnter orig, IntOperator self)
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

        // Fixing Combo Stagger
        private void OnIntCompareActionHeavyBlow(On.HutongGames.PlayMaker.Actions.IntCompare.orig_OnEnter orig, IntCompare self)
        {
            if ((self.Fsm.Name == "Stun" || self.Fsm.Name == "Stun Control") && self.State.Name == "In Combo")
            {
                self.greaterThan = FsmEvent.GetFsmEvent("STUN");
            }

            orig(self);
        }
        #endregion
        #region Sharp Shadow Changes

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
        [BoolElement("Fury of the Fallen Options", "Works with Joni's", "")]
        public bool furyOfTheFallenJonis = false;

        [SliderIntElement("Fury of the Fallen Options", "Health Threshold", 0, 13)]
        public int furyOfTheFallenHealth = 1;

        [SliderIntElement("Fury of the Fallen Options", "Damage Increase (%)", 0, 150)]
        public int furyOfTheFallenScaling = 75;

        [SliderIntElement("Fury of the Fallen Options", "Grubberfly's Increase (%)", 0, 150)]
        public int furyOfTheFallenGrubberflysScaling = 50;

        [SliderIntElement("Fury of the Fallen Options", "Glowing Womb Increase", 0, 15)]
        public int furyOfTheFallenHatchlingDamage = 5;

        [ButtonElement("Fury of the Fallen Options", "Reset Defaults", "")]
        public void ResetFuryOfTheFallen()
        {
            furyOfTheFallenHealth = 1;
            furyOfTheFallenJonis = false;
            furyOfTheFallenScaling = 75;
            furyOfTheFallenGrubberflysScaling = 50;
            furyOfTheFallenHatchlingDamage = 5;
        }
        #endregion
        #region Quick Focus Settings
        [SliderFloatElement("Quick Focus Options", "Shape of Unn Fast Speed", 0f, 36f)]
        public float quickFocusShapeOfUnnSpeed = 12f;

        [SliderIntElement("Quick Focus Options", "Focus Time (thousandths)", 45, 891)]
        public int quickFocusFocusTime = 594;

        [ButtonElement("Quick Focus Options", "Reset Defaults", "")]    
        public void ResetQuickFocus()
        {
            quickFocusShapeOfUnnSpeed = 12f;
            quickFocusFocusTime = 594;
        }
        #endregion
        #region Lifeblood Heart/Core Settings
        [SliderIntElement("Lifeblood Heart/Core Options", "Heart Lifeblood", 0, 6)]
        public int lifebloodHeartLifeblood = 2;

        [SliderIntElement("Lifeblood Heart/Core Options", "Core Lifeblood", 0, 12)]
        public int lifebloodCoreLifeblood = 4;

        [ButtonElement("Lifeblood Heart/Core Options", "Reset Defaults", "")]
        public void ResetLifebloodHeart()
        {
            lifebloodHeartLifeblood = 2;
            lifebloodCoreLifeblood = 4;
        }
        #endregion
        #region Defender's Crest Settings
        [SliderIntElement("Defender's Crest Options", "Shop Discount (%)", 0, 100)]
        public int defendersCrestDiscount = 20;

        [SliderFloatElement("Defender's Crest Options", "DC Cloud Duration", 0f, 5f)]
        public float defendersCrestCloudDuration = 1.1f;

        [SliderFloatElement("Defender's Crest Options", "DC + SS Cloud Duration", 0f, 10f)]
        public float defendersCrestSporeShroomCloudDuration = 4.1f;

        [SliderFloatElement("Defender's Crest Options", "DC + GW Cloud Duration", 0f, 10f)]
        public float defendersCrestGlowingWombDuration = 1f;

        [SliderFloatElement("Defender's Crest Options", "DC + Fluke Cloud Duration", 0f, 10f)]
        public float defendersCrestFlukenestDuration = 2.2f;

        [SliderIntElement("Defender's Crest Options", "DC Tick Rate (1000ths)", 10, 300)]
        public int defendersCrestDamageRate = 300;

        [SliderIntElement("Defender's Crest Options", "DC + SS Tick Rate (1000ths)", 10, 200)]
        public int defendersCrestSporeShroomDamageRate = 200;

        [SliderIntElement("Defender's Crest Options", "DC + GW Tick Rate (1000ths)", 10, 200)]
        public int defendersCrestGlowingWombDamageRate = 200;

        [SliderIntElement("Defender's Crest Options", "DC + Fluke Tick Rate (1000ths)", 10, 100)]
        public int defendersCrestFlukenestDamageRate = 100;

        [SliderIntElement("Defender's Crest Options", "DC + Fluke + SS Tick Rate (1000ths)", 0, 100)]
        public int defendersCrestFlukenestShamanStoneDamageRate = 75;

        [SliderIntElement("Defender's Crest Options", "Cloud Frequency (100ths)", 20, 150)]
        public int defendersCrestCloudFrequency = 75;

        [SliderIntElement("Defender's Crest Options", "DC + GW Impact Damage", 0, 12)]
        public int defendersCrestGlowingWombDamage = 4;

        //[SliderFloatElement("Defender's Crest Options", "DC + SS Cloud Size", 0f, 75f)]
        //public float defendersCrestSporeShroomCloudSize = 40f;

        //[SliderFloatElement("Defender's Crest Options", "DC + SS + DF Cloud Size", 0f, 75f)]
        //public float defendersCrestSporeShroomDeepFocusCloudSize = 48f;

        [ButtonElement("Defender's Crest Options", "Reset Defaults", "")]
        public void ResetDefendersCrest()
        {
            defendersCrestDiscount = 20;
            defendersCrestCloudDuration = 1.1f;
            defendersCrestSporeShroomCloudDuration = 4.1f;
            defendersCrestCloudFrequency = 75;
            defendersCrestGlowingWombDamage = 4;
            defendersCrestGlowingWombDuration = 1f;
            defendersCrestFlukenestDuration = 2.2f;
            defendersCrestFlukenestDamageRate = 100;
            defendersCrestFlukenestShamanStoneDamageRate = 75;
            defendersCrestDamageRate = 300;
            defendersCrestSporeShroomDamageRate = 200;
            //defendersCrestSporeShroomCloudSize = 40f;
            //defendersCrestSporeShroomDeepFocusCloudSize = 48f;
        }
        #endregion
        #region Flukenest Settings

        [SliderIntElement("Flukenest Options", "Fluke Damage", 0, 12)]
        public int flukenestDamage = 4;

        [SliderIntElement("Flukenest Options", "SS Fluke Damage", 0, 15)]
        public int flukenestShamanStoneDamage = 5;

        [SliderFloatElement("Flukenest Options", "Fluke Minimum Size", 0f, 3f)]
        public float flukenestFlukeSizeMin = 0.7f;

        [SliderFloatElement("Flukenest Options", "Fluke Maximum Size", 0f, 3f)]
        public float flukenestFlukeSizeMax = 0.9f;

        [SliderFloatElement("Flukenest Options", "SS Fluke Minimum Size", 0f, 2.7f)]
        public float flukenestShamanStoneFlukeSizeMin = 0.9f;

        [SliderFloatElement("Flukenest Options", "SS Fluke Maximum Size", 0f, 3.6f)]
        public float flukenestShamanStoneFlukeSizeMax = 1.2f;

        [SliderIntElement("Flukenest Options", "Vengeful Spirit Flukes", 0, 18)]
        public int flukenestVSFlukes = 9;

        [SliderIntElement("Flukenest Options", "Shade Soul Flukes", 0, 32)]
        public int flukenestSSFlukes = 16;

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
        }

        #endregion
        #region Thorns of Agony Settings
        [SliderFloatElement("Thorns of Agony Options", "Nail Damage Multiplier", 0f, 5f)]
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

        [SliderIntElement("Longnail / Mark of Pride Options", "Longnail + MoP Size Scale (100ths)", 0, 500)]
        public int longnailMarkOfPrideScale = 140;

        [SliderIntElement("Longnail / Mark of Pride Options", "MoP Size Scale (100ths)", 0, 500)]
        public int markOfPrideScale = 125;

        [SliderIntElement("Longnail / Mark of Pride Options", "Longnail Size Scale (100ths)", 0, 500)]
        public int longnailScale = 115;

        [SliderIntElement("Longnail / Mark of Pride Options", "Grubberfly's Size Scale (100ths)", 0, 500)]
        public int markOfPrideGrubberflysScale = 135;

        [ButtonElement("Longnail / Mark of Pride Options", "Reset Defaults", "")]
        public void ResetLongnailAndMarkOfPride()
        {
            longnailMarkOfPrideWallSlash = false;
            markOfPrideGrubberflysScale = 135;
            longnailMarkOfPrideScale = 140;
            markOfPrideScale = 125;
            longnailScale = 115;            
        }

        #endregion
        #region Heavy Blow Settings
        [BoolElement("Heavy Blow Options", "Works with Wall Slash", "")]
        public bool heavyBlowWallSlash = false;

        [BoolElement("Heavy Blow Options", "Works with Cyclone Slash", "")]
        public bool heavyBlowCycloneSlash = false;

        [SliderIntElement("Heavy Blow Options", "Knockback Increase (%)", 0, 300)]
        public int heavyBlowSlashRecoil = 75;

        [SliderIntElement("Heavy Blow Options", "Great Slash Increase (%)", 0, 300)]
        public int heavyBlowGreatSlashRecoil = 33;

        [SliderIntElement("Heavy Blow Options", "Cyclone Slash Increase (%)", 0, 300)]
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

        #endregion
    }
}
