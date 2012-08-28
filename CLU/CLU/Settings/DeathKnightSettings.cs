#region Revision Info

// This file was part of Singular - A community driven Honorbuddy CC
// $Author: raphus $
// $LastChangedBy: wulf $

#endregion

using System.ComponentModel;

using Styx.Helpers;

using DefaultValue = Styx.Helpers.DefaultValueAttribute;

namespace Clu.Settings
{
    internal class DeathKnightSettings : Styx.Helpers.Settings
    {
        public DeathKnightSettings()
        : base(CLUSettings.SettingsPath + "_DeathKnight.xml")
        {
        }

        #region Common

        //[Setting]
        //[DefaultValue(true)]
        //[Category("Common")]
        //[DisplayName("Use Death and Decay")]
        //public bool UseDeathAndDecay { get; set; }



        [Setting]
        [DefaultValue(true)]
        [Category("Common")]
        [DisplayName("Icebound Fortitude")]
        [Description("Will use Icebound Fortitude. (Enable Self Healing must be enabled as well.)")]
        public bool UseIceboundFortitude
        {
            get;
            set;
        }

        [Setting]
        [DefaultValue(true)]
        [Category("Common")]
        [DisplayName("Anti-Magic Shell")]
        [Description("Will use Anti-Magic Shell when the current target is Channeling/Casting a harmful spell. (Enable Self Healing (General Tab) must be enabled as well.)")]
        public bool UseAntiMagicShell
        {
            get;
            set;
        }

        [Setting]
        [DefaultValue(true)]
        [Category("Common")]
        [DisplayName("Horn of Winter")]
        [Description("When true it Will use Use Horn of Winter pre- combat if no one is supplying the Strength Agility Buff")]
        public bool UseHornofWinter
        {
            get;
            set;
        }

        [Setting]
        [DefaultValue(30)]
        [Category("Common")]
        [DisplayName("Death Strike Emergency Percent")]
        [Description("Will use Death Strike for self heal with Frost and Unholy rotations at this healthpercent. (Enable Self Healing (General Tab) must be enabled as well.)")]
        public int DeathStrikeEmergencyPercent
        {
            get;
            set;
        }

        [Setting]
        [DefaultValue(40)]
        [Category("Common")]
        [DisplayName("Healthstone Percent")]
        [Description("Will use a Healthstone for self heal at this healthpercent. (Enable Self Healing (General Tab) must be enabled as well.)")]
        public int HealthstonePercent
        {
            get;
            set;
        }

        [Setting]
        [DefaultValue(true)]
        [Category("Common")]
        [DisplayName("Empower Rune Weapon")]
        [Description("Will use Empower Rune Weapon. Warning! - Shared setting across all specs")]
        public bool UseEmpowerRuneWeapon
        {
            get;
            set;
        }


        #endregion

        #region Blood

        [Setting]
        [DefaultValue(60)]
        [Category("Blood")]
        [DisplayName("Icebound Fortitude Percent")]
        [Description("Will use Icebound Fortitude at the set Health Percent. (Enable Self Healing (General Tab) must be enabled as well.)")]
        public int BloodIceboundFortitudePercent
        {
            get;
            set;
        }

        [Setting]
        [DefaultValue(false)]
        [Category("Blood")]
        [DisplayName("Bone Shield on cooldown")]
        [Description("Will use Bone Shield on cooldown during combat.")]
        public bool UseBoneShieldonCooldown
        {
            get;
            set;
        }

        [Setting]
        [DefaultValue(true)]
        [Category("Blood")]
        [DisplayName("Bone Shield Defensively")]
        [Description("Will use Bone Shield Defensively during combat.")]
        public bool UseBoneShieldDefensively
        {
            get;
            set;
        }

        [Setting]
        [DefaultValue(true)]
        [Category("Blood")]
        [DisplayName("Dancing Rune Weapon")]
        [Description("Will use Dancing Rune Weapon Defensively during combat.")]
        public bool UseDancingRuneWeapon
        {
            get;
            set;
        }

        [Setting]
        [DefaultValue(80)]
        [Category("Blood")]
        [DisplayName("Dancing Rune Weapon Percent")]
        [Description("Will use Dancing Rune Weapon at the set Health Percent. (Enable Self Healing (General Tab) must be enabled as well.)")]
        public int DancingRuneWeaponPercent
        {
            get;
            set;
        }

        [Setting]
        [DefaultValue(true)]
        [Category("Common")]
        [DisplayName("Lichborne")]
        [Description("Will use Lichborne. (Enable Self Healing (General Tab) must be enabled as well.)")]
        public bool UseLichborne
        {
            get;
            set;
        }

        [Setting]
        [DefaultValue(60)]
        [Category("Blood")]
        [DisplayName("Lichborne Percent")]
        [Description("Will use Lichborne at this HealthPercent. (Enable Self Healing (General Tab) must be enabled as well.)")]
        public int LichbornePercent
        {
            get;
            set;
        }

        [Setting]
        [DefaultValue(60)]
        [Category("Blood")]
        [DisplayName("DeathCoil with Lichborne Percent")]
        [Description("Will use DeathCoil at this HealthPercent when Lichborne is active for self heal. (Enable Self Healing (General Tab) must be enabled as well.)")]
        public int DeathCoilHealPercent
        {
            get;
            set;
        }


        [Setting]
        [DefaultValue(true)]
        [Category("Blood")]
        [DisplayName("Pet Sacrifice (Death Pact)")]
        [Description("If set to true CLU will use Death Pact. (Enable Self Healing (General Tab) must be enabled as well.)")]
        public bool BloodUsePetSacrifice
        {
            get;
            set;
        }

        [Setting]
        [DefaultValue(60)]
        [Category("Blood")]
        [DisplayName("Pet Sacrifice (Death Pact) Percent")]
        [Description("Will use Death Pact at the set Healthpercent. (Enable Self Healing (General Tab) must be enabled as well.)")]
        public int BloodPetSacrificePercent
        {
            get;
            set;
        }

        [Setting]
        [DefaultValue(true)]
        [Category("Blood")]
        [DisplayName("Vampiric Blood")]
        [Description("If set to true CLU will use Vampiric Blood. (Enable Self Healing (General Tab) must be enabled as well.)")]
        public bool UseVampiricBlood
        {
            get;
            set;
        }

        [Setting]
        [DefaultValue(60)]
        [Category("Blood")]
        [DisplayName("Vampiric Blood Percent")]
        [Description("Will use Vampiric Blood at the set Healthpercent. (Enable Self Healing (General Tab) must be enabled as well.)")]
        public int VampiricBloodPercent
        {
            get;
            set;
        }

        [Setting]
        [DefaultValue(true)]
        [Category("Blood")]
        [DisplayName("RuneTap & WoTN")]
        [Description("If set to true CLU will use RuneTap when Will of the Necropolis procs. (Enable Self Healing (General Tab) must be enabled as well.)")]
        public bool UseRuneTapWoTN
        {
            get;
            set;
        }

        [Setting]
        [DefaultValue(90)]
        [Category("Blood")]
        [DisplayName("RuneTap Percent")]
        [Description("Will use RuneTap when Will of the Necropolis procs at the set Healthpercent. (Enable Self Healing (General Tab) must be enabled as well.)")]
        public int RuneTapWoTNPercent
        {
            get;
            set;
        }

        [Setting]
        [DefaultValue(true)]
        [Category("Blood")]
        [DisplayName("RuneTap")]
        [Description("If set to true CLU will use RuneTap.")]
        public bool UseRuneTap
        {
            get;
            set;
        }

        [Setting]
        [DefaultValue(80)]
        [Category("Blood")]
        [DisplayName("RuneTap & WoTN Percent")]
        [Description("Will use RuneTap at the set Healthpercent.")]
        public int RuneTapPercent
        {
            get;
            set;
        }

        [Setting]
        [DefaultValue(90)]
        [Category("Blood")]
        [DisplayName("Death Strike for Blood Shield Percent")]
        [Description("Will use Death Strike at the set Healthpercent and at the time [Death Strike Blood Shield Time Remaining] setting specifies. eg: If set to BS health = 101 and BS remaining = 2, Bloodshield will be maintained constantly")]
        public int DeathStrikeBloodShieldPercent
        {
            get;
            set;
        }

        [Setting]
        [DefaultValue(2)]
        [Category("Blood")]
        [DisplayName("Death Strike Blood Shield Time Remaining")]
        [Description("Will use Death Strike if the players bloodshield has less than the set time left and at the Healthpercent [Death Strike for Blood Shield Percent] setting specifies eg: If set to BS health = 101 and BS remaining = 2, Bloodshield will be maintained constantly")]
        public int DeathStrikeBloodShieldTimeRemaining
        {
            get;
            set;
        }

        [Setting]
        [DefaultValue(40)]
        [Category("Blood")]
        [DisplayName("Death Strike Percent")]
        [Description("Will use Death Strike at the set Healthpercent regardless of blood shield")]
        public int DeathStrikePercent
        {
            get;
            set;
        }

        [Setting]
        [DefaultValue(true)]
        [Category("Blood")]
        [DisplayName("BloodTap with RuneTap")]
        [Description("If set to true CLU will use BloodTap for RuneTap.")]
        public bool UseBloodTapforRuneTap
        {
            get;
            set;
        }

        [Setting]
        [DefaultValue(3)]
        [Category("Blood")]
        [DisplayName("Death and Decay Add Count")]
        [Description("Will use Death and Decay when agro mob count is equal to or higher then this value.")]
        public int BloodDeathAndDecayCount
        {
            get;
            set;
        }

        [Setting]
        [DefaultValue(3)]
        [Category("Blood")]
        [DisplayName("Pestilence Add Count")]
        [Description("Will use Pestilence when agro mob count is equal to or higher then this value.")]
        public int BloodPestilenceCount
        {
            get;
            set;
        }

        [Setting]
        [DefaultValue(3)]
        [Category("Blood")]
        [DisplayName("BloodBoil Add Count")]
        [Description("Will use BloodBoil when agro mob count is equal to or higher then this value.")]
        public int BloodBloodBoilCount
        {
            get;
            set;
        }

        [Setting]
        [DefaultValue(60)]
        [Category("Blood")]
        [DisplayName("RuneStrike RunicPower Percent")]
        [Description("Will use RuneStrike when RunicPower Percent is greater thant the set value")]
        public int RuneStrikePercent
        {
            get;
            set;
        }



        #endregion

        #region Frost

        [Setting]
        [DefaultValue(60)]
        [Category("Frost")]
        [DisplayName("Icebound Fortitude Percent")]
        [Description("Will use Icebound Fortitude at the set Health Percent. (Enable Self Healing (General Tab) must be enabled as well.)")]
        public int FrostIceboundFortitudePercent
        {
            get;
            set;
        }

        [Setting]
        [DefaultValue(true)]
        [Category("Frost")]
        [DisplayName("Pet Sacrifice (Death Pact)")]
        [Description("If set to true CLU will use Death Pact. (Enable Self Healing (General Tab) must be enabled as well.)")]
        public bool FrostUsePetSacrifice
        {
            get;
            set;
        }

        [Setting]
        [DefaultValue(60)]
        [Category("Frost")]
        [DisplayName("Pet Sacrifice (Death Pact) Percent")]
        [Description("Will use Death Pact at the set Healthpercent. (Enable Self Healing (General Tab) must be enabled as well.)")]
        public int FrostPetSacrificePercent
        {
            get;
            set;
        }

        //[Setting]
        //[DefaultValue(true)]
        //[Category("Frost")]
        //[DisplayName("Pillar of Frost")]
        //public bool UsePillarOfFrost { get; set; }

        //[Setting]
        //[DefaultValue(true)]
        //[Category("Frost")]
        //[DisplayName("Raise Dead")]
        //public bool UseRaiseDead { get; set; }

        //[Setting]
        //[DefaultValue(true)]
        //[Category("Frost")]
        //[DisplayName("Empower Rune Weapon")]
        //public bool UseEmpowerRuneWeapon { get; set; }

        //[Setting]
        //[DefaultValue(false)]
        //[Category("Frost")]
        //[DisplayName("Use Necrotic Strike - Frost")]
        //public bool UseNecroticStrike { get; set; }

        //#endregion

        //#region Category: Unholy

        //[Setting]
        //[DefaultValue(true)]
        //[Category("Unholy")]
        //[DisplayName("Summon Gargoyle")]
        //public bool UseSummonGargoyle { get; set; }

        #endregion

        #region Unholy

        [Setting]
        [DefaultValue(40)]
        [Category("Unholy")]
        [DisplayName("Icebound Fortitude Percent")]
        [Description("Will use Icebound Fortitude at the set Health Percent. (Enable Self Healing (General Tab) must be enabled as well.)")]
        public int UnholyIceboundFortitudePercent
        {
            get;
            set;
        }

        [Setting]
        [DefaultValue(true)]
        [Category("Unholy")]
        [DisplayName("Pet Sacrifice (Death Pact)")]
        [Description("If set to true CLU will use Death Pact. (Enable Self Healing (General Tab) must be enabled as well.)")]
        public bool UnholyUsePetSacrifice
        {
            get;
            set;
        }

        [Setting]
        [DefaultValue(25)]
        [Category("Unholy")]
        [DisplayName("Pet Sacrifice (Death Pact) Percent")]
        [Description("Will use Death Pact at the set Healthpercent. (Enable Self Healing (General Tab) must be enabled as well.)")]
        public int UnholyPetSacrificePercent
        {
            get;
            set;
        }


        #endregion
    }
}