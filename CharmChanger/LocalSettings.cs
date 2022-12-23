using HKMirror;

namespace CharmChanger
{
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

            PlayerDataAccess.charmSlotsFilled = 0;
        }
        #endregion

        #region Grubsong Settings
        [InputIntElement("Grubsong Options", "Soul", 0, 198)]
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
        public float regularInvulnerability = 1.3f;

        [InputFloatElement("Stalwart Shell Options", "Recoil Time", 0, 1.0f)]
        public float regularRecoil = 0.2f;

        [InputFloatElement("Stalwart Shell Options", "Stalwart Invuln. Time", 0, 10)]
        public float stalwartShellInvulnerability = 1.75f;

        [InputFloatElement("Stalwart Shell Options", "Stalwart Recoil Time", 0, 1.0f)]
        public float stalwartShellRecoil = 0.08f;

        [ButtonElement("Stalwart Shell Options", "Reset Defaults", "")]
        public void ResetStalwartShell()
        {
            regularInvulnerability = 1.3f;
            stalwartShellInvulnerability = 1.75f;
            regularRecoil = 0.2f;
            stalwartShellRecoil = 0.08f;
        }
        #endregion
        #region Baldur Shell Settings
        [InputFloatElement("Baldur Shell Options", "Enemy Knockback Multiplier", 0f, 5f)]
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
        [InputFloatElement("Quick/Deep Focus Options", "Focus Time", 0.033f, 2f)]
        public float regularFocusTime = 0.891f;

        [InputFloatElement("Quick/Deep Focus Options", "Quick Focus Time", 0.033f, 2f)]
        public float quickFocusFocusTime = 0.594f;

        [InputIntElement("Quick/Deep Focus Options", "Deep Focus Added Time (%)", 0, 500)]
        public int deepFocusHealingTimeScale = 65;

        [SliderIntElement("Quick/Deep Focus Options", "Focus Healing", 0, 13)]
        public int regularFocusHealing = 1;

        [SliderIntElement("Quick/Deep Focus Options", "Deep Focus Healing", 0, 13)]
        public int deepFocusHealing = 2;

        [ButtonElement("Quick/Deep Focus Options", "Reset Defaults", "")]
        public void ResetQuickAndDeepFocus()
        {
            regularFocusTime = 0.891f;
            quickFocusFocusTime = 0.594f;
            deepFocusHealingTimeScale = 65;
            regularFocusHealing = 1;
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
        [BoolElement("Heavy Blow Options", "Recoil affects Wall Slash", "")]
        public bool heavyBlowWallSlash = false;

        [BoolElement("Heavy Blow Options", "Recoil affects Cyclone Slash", "")]
        public bool heavyBlowCycloneSlash = false;

        [InputFloatElement("Heavy Blow Options", "Knockback Mult.", 0f, 5f)]
        public float regularSlashRecoil = 1.0f;

        [InputFloatElement("Heavy Blow Options", "Great Slash Knockback Mult.", 0f, 5f)]
        public float regularGreatSlashRecoil = 1.5f;

        [InputFloatElement("Heavy Blow Options", "Cyclone Slash Knockback Mult.", 0f, 5f)]
        public float regularCycloneSlashRecoil = 1.0f;

        [InputFloatElement("Heavy Blow Options", "HB Knockback Mult.", 0f, 5f)]
        public float heavyBlowSlashRecoil = 1.75f;

        [InputFloatElement("Heavy Blow Options", "HB Great Slash Mult.", 0f, 5f)]
        public float heavyBlowGreatSlashRecoil = 2.0f;

        [InputFloatElement("Heavy Blow Options", "HB Cyclone Slash Mult.", 0f, 5f)]
        public float heavyBlowCycloneSlashRecoil = 1.0f;

        [SliderIntElement("Heavy Blow Options", "Stagger Reduction", 0, 20)]
        public int heavyBlowStagger = 1;

        [SliderIntElement("Heavy Blow Options", "Combo Stagger Reduction", 0, 20)]
        public int heavyBlowStaggerCombo = 1;

        [ButtonElement("Heavy Blow Options", "Reset Defaults", "")]
        public void ResetHeavyBlow()
        {
            heavyBlowWallSlash = false;
            heavyBlowCycloneSlash = false;
            regularSlashRecoil = 1.0f;
            regularGreatSlashRecoil = 1.5f;
            regularCycloneSlashRecoil = 1.0f;
            heavyBlowSlashRecoil = 1.75f;
            heavyBlowGreatSlashRecoil = 2.0f;
            heavyBlowCycloneSlashRecoil = 1.0f;
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
        public float regularVSSizeScaleX = 1.0f;

        [InputFloatElement("Shaman Stone Options", "Vengeful Spirit Y Scale", 0f, 5f)]
        public float regularVSSizeScaleY = 1.0f;

        [InputFloatElement("Shaman Stone Options", "Shade Soul X Scale", 0f, 5f)]
        public float regularSSSizeScaleX = 1.0f;

        [InputFloatElement("Shaman Stone Options", "Shade Soul Y Scale", 0f, 5f)]
        public float regularSSSizeScaleY = 1.0f;

        [InputFloatElement("Shaman Stone Options", "SS Vengeful Spirit X Scale", 0f, 5f)]
        public float shamanStoneVSSizeScaleX = 1.3f;

        [InputFloatElement("Shaman Stone Options", "SS Vengeful Spirit Y Scale", 0f, 5f)]
        public float shamanStoneVSSizeScaleY = 1.6f;

        [InputFloatElement("Shaman Stone Options", "SS Shade Soul X Scale", 0f, 5f)]
        public float shamanStoneSSSizeScaleX = 1.3f;

        [InputFloatElement("Shaman Stone Options", "SS Shade Soul Y Scale", 0f, 5f)]
        public float shamanStoneSSSizeScaleY = 1.6f;

        [InputIntElement("Shaman Stone Options", "Vengeful Spirit Damage", 0, 100)]
        public int regularVSDamage = 15;

        [InputIntElement("Shaman Stone Options", "Shade Soul Damage", 0, 100)]
        public int regularSSDamage = 30;

        [InputIntElement("Shaman Stone Options", "Howling Wraiths Damage", 0, 100)]
        public int regularHWDamage = 13;

        [InputIntElement("Shaman Stone Options", "Abyss Shriek Damage", 0, 100)]
        public int regularASDamage = 20;

        [InputIntElement("Shaman Stone Options", "Dive Contact Damage", 0, 100)]
        public int regularDiveDamage = 15;

        [InputIntElement("Shaman Stone Options", "Desolate Dive Damage", 0, 100)]
        public int regularDDiveDamage = 20;

        [InputIntElement("Shaman Stone Options", "Descending Dark Damage L", 0, 100)]
        public int regularDDarkDamageL = 35;

        [InputIntElement("Shaman Stone Options", "Descending Dark Damage R", 0, 100)]
        public int regularDDarkDamageR = 30;

        [InputIntElement("Shaman Stone Options", "Descending Dark Final Damage", 0, 100)]
        public int regularDDarkDamageMega = 15;

        [InputIntElement("Shaman Stone Options", "SS Vengeful Spirit Damage", 0, 100)]
        public int shamanStoneVSDamage = 20;

        [InputIntElement("Shaman Stone Options", "SS Shade Soul Damage", 0, 100)]
        public int shamanStoneSSDamage = 40;

        [InputIntElement("Shaman Stone Options", "SS Howling Wraiths Damage", 0, 100)]
        public int shamanStoneHWDamage = 20;

        [InputIntElement("Shaman Stone Options", "SS Abyss Shriek Damage", 0, 100)]
        public int shamanStoneASDamage = 30;

        [InputIntElement("Shaman Stone Options", "SS Dive Contact Damage", 0, 100)]
        public int shamanStoneDiveDamage = 23;

        [InputIntElement("Shaman Stone Options", "SS Desolate Dive Damage", 0, 100)]
        public int shamanStoneDDiveDamage = 30;

        [InputIntElement("Shaman Stone Options", "SS Descending Dark Damage L", 0, 100)]
        public int shamanStoneDDarkDamageL = 50;

        [InputIntElement("Shaman Stone Options", "SS Descending Dark Damage R", 0, 100)]
        public int shamanStoneDDarkDamageR = 50;

        [InputIntElement("Shaman Stone Options", "SS Descending Dark Final Damage", 0, 100)]
        public int shamanStoneDDarkDamageMega = 15;

        [ButtonElement("Shaman Stone Options", "Reset Defaults", "")]
        public void ResetShamanStone()
        {
            regularVSSizeScaleX = 1.0f;
            regularVSSizeScaleY = 1.0f;
            regularSSSizeScaleX = 1.0f;
            regularSSSizeScaleY = 1.0f;
            shamanStoneVSSizeScaleX = 1.3f;
            shamanStoneVSSizeScaleY = 1.6f;
            shamanStoneSSSizeScaleX = 1.3f;
            shamanStoneSSSizeScaleY = 1.6f;
            regularVSDamage = 15;
            regularSSDamage = 30;
            regularHWDamage = 13;
            regularASDamage = 20;
            regularDiveDamage = 15;
            regularDDiveDamage = 20;
            regularDDarkDamageL = 35;
            regularDDarkDamageR = 30;
            regularDDarkDamageMega = 15;
            shamanStoneVSDamage = 20;
            shamanStoneSSDamage = 40;
            shamanStoneHWDamage = 20;
            shamanStoneASDamage = 30;
            shamanStoneDiveDamage = 23;
            shamanStoneDDiveDamage = 30;
            shamanStoneDDarkDamageL = 50;
            shamanStoneDDarkDamageR = 50;
            shamanStoneDDarkDamageMega = 15;
        }
        #endregion
        #region Soul Catcher/Eater Settings
        [InputIntElement("Soul Catcher/Eater Options", "Soul Gained", 0, 198)]
        public int regularSoul = 11;

        [InputIntElement("Soul Catcher/Eater Options", "Vessel Soul Gained", 0, 198)]
        public int regularReservesSoul = 6;

        [InputIntElement("Soul Catcher/Eater Options", "Soul Catcher Soul", 0, 198)]
        public int soulCatcherSoul = 3;

        [InputIntElement("Soul Catcher/Eater Options", "Soul Catcher Vessel Soul", 0, 198)]
        public int soulCatcherReservesSoul = 2;

        [InputIntElement("Soul Catcher/Eater Options", "Soul Eater Soul", 0, 198)]
        public int soulEaterSoul = 8;

        [InputIntElement("Soul Catcher/Eater Options", "Soul Eater Vessel Soul", 0, 198)]
        public int soulEaterReservesSoul = 6;

        [ButtonElement("Soul Catcher/Eater Options", "Reset Defaults", "")]
        public void ResetSoulCatcherAndEater()
        {
            regularSoul = 11;
            soulCatcherSoul = 3;
            soulEaterSoul = 8;
            regularReservesSoul = 6;
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
        public float regularChargeTime = 1.35f;

        [InputFloatElement("Nailmaster's Glory Options", "Glory NArt Charge Time", 0.01f, 5f)]
        public float nailmastersGloryChargeTime = 0.75f;

        [ButtonElement("Nailmaster's Glory Options", "Reset Defaults", "")]
        public void ResetNailmastersGlory()
        {
            regularChargeTime = 1.35f;
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
        public int regularDreamSoul = 33;

        [InputIntElement("Dream Wielder Options", "Dream Wielder Soul Gain", 0, 198)]
        public int dreamWielderSoulGain = 66;

        [InputIntElement("Dream Wielder Options", "Essence Chance Low (1/X)", 1, 1000)]
        public int dreamWielderEssenceChanceLow = 200;

        [InputIntElement("Dream Wielder Options", "Essence Chance High (1/X)", 1, 1000)]
        public int dreamWielderEssenceChanceHigh = 40;

        [ButtonElement("Dream Wielder Options", "Reset Defaults", "")]
        public void ResetDreamWielder()
        {
            regularDreamSoul = 33;
            dreamWielderSoulGain = 66;
            dreamWielderEssenceChanceLow = 200;
            dreamWielderEssenceChanceHigh = 40;
        }
        #endregion
        #region Dashmaster Settings
        [BoolElement("Dashmaster Options", "Allows Downward Dash", "")]
        public bool dashmasterDownwardDash = true;

        [InputFloatElement("Dashmaster Options", "Dash Cooldown", 0f, 10f)]
        public float regularDashCooldown = 0.6f;

        [InputFloatElement("Dashmaster Options", "Dashmaster Dash Cooldown", 0f, 10f)]
        public float dashmasterDashCooldown = 0.4f;

        [ButtonElement("Dashmaster Options", "Reset Defaults", "")]
        public void ResetDashmaster()
        {
            dashmasterDownwardDash = true;
            regularDashCooldown = 0.6f;
            dashmasterDashCooldown = 0.4f;
        }
        #endregion
        #region Quick Slash Settings
        [InputFloatElement("Quick Slash Options", "Attack Cooldown", 0f, 2f)]
        public float regularAttackCooldown = 0.41f;

        [InputFloatElement("Quick Slash Options", "Quick Slash Attack Cooldown", 0f, 2f)]
        public float quickSlashAttackCooldown = 0.25f;

        [ButtonElement("Quick Slash Options", "Reset Defaults", "")]
        public void ResetQuickSlash()
        {
            regularAttackCooldown = 0.41f;
            quickSlashAttackCooldown = 0.25f;
        }
        #endregion
        #region Spell Twister Settings
        [InputIntElement("Spell Twister Options", "Spell Cost", 0, 99)]
        public int regularSpellCost = 33;

        [InputIntElement("Spell Twister Options", "Spell Twister Spell Cost", 0, 99)]
        public int spellTwisterSpellCost = 24;

        [ButtonElement("Spell Twister Options", "Reset Defaults", "")]
        public void ResetSpellTwister()
        {
            regularSpellCost = 33;
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
        [InputFloatElement("Sprintmaster Options", "Walk Speed", 0f, 36f)]
        public float regularWalkSpeed = 6.0f;

        [InputFloatElement("Sprintmaster Options", "Run Speed", 0f, 36f)]
        public float regularSpeed = 8.3f;

        [InputFloatElement("Sprintmaster Options", "Sprintmaster Speed", 0f, 36f)]
        public float sprintmasterSpeed = 10.0f;

        [InputFloatElement("Sprintmaster Options", "Dashmaster Speed", 0f, 36f)]
        public float sprintmasterSpeedCombo = 11.5f;

        [ButtonElement("Sprintmaster Options", "Reset Defaults", "")]
        public void ResetSprintmaster()
        {
            regularWalkSpeed = 6.0f;
            regularSpeed = 8.3f;
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
