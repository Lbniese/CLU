#region Revision Info

// This file was part of Singular - A community driven Honorbuddy CC
// $Author: highvoltz $
// $LastChangedBy: wulf $
#endregion

using System.ComponentModel;

using Styx.Helpers;

using DefaultValue = Styx.Helpers.DefaultValueAttribute;

namespace Clu.Settings
{
    internal class WarriorSettings : Styx.Helpers.Settings
    {
        public WarriorSettings()
        : base(CLUSettings.SettingsPath + "_Warrior.xml")
        {
        }

        #region Common

        [Setting]
        [DefaultValue(WarriorShout.Battle)]
        [Category("Common")]
        [DisplayName("Warrior Shout Selector")]
        [Description("Choose a Warrior Shout (Default is Battle)")]
        public WarriorShout WarriorShoutSelection
        {
            get;
            set;
        }

        #endregion

        #region Protection

        //[Setting]
        //[DefaultValue(50)]
        //[Category("Protection")]
        //[DisplayName("Enraged Regeneration Health")]
        //[Description("Enrage Regeneration will be used when your health drops below this value")]
        //public int WarriorEnragedRegenerationHealth { get; set; }

        //[Setting]
        //[DefaultValue(40)]
        //[Category("Protection")]
        //[DisplayName("Shield Wall Health")]
        //[Description("Shield Wall will be used when your health drops below this value")]
        //public int WarriorProtShieldWallHealth { get; set; }


        #endregion

        #region DPS

        //[Setting]
        //[DefaultValue(true)]
        //[Category("DPS")]
        //[DisplayName("Use Damage Cooldowns")]
        //[Description("True / False if you would like the cc to use damage cooldowns")]
        //public bool UseWarriorDpsCooldowns { get; set; }

        //[Setting]
        //[DefaultValue(true)]
        //[Category("DPS")]
        //[DisplayName("Use Interupts")]
        //[Description("True / False if you would like the cc to use Interupts")]
        //public bool UseWarriorInterupts { get; set; }

        //[Setting]
        //[DefaultValue(true)]
        //[Category("DPS")]
        //[DisplayName("true for Battle Shout, false for Commanding")]
        //[Description("True / False if you would like the cc to use Battleshout/Commanding")]
        //public bool UseWarriorShouts { get; set; }

        //[Setting]
        //[DefaultValue(true)]
        //[Category("DPS")]
        //[DisplayName("Slows")]
        //[Description("True / False if you would like the cc to use slows ie. Hammstring, Piercing Howl")]
        //public bool UseWarriorSlows { get; set; }

        //[Setting]
        //[DefaultValue(false)]
        //[Category("DPS")]
        //[DisplayName("Basic Rotation Only")]
        //[Description("True / False if you would like the cc to use just the basic DPS rotation only")]
        //public bool UseWarriorBasicRotation { get; set; }

        //[Setting]
        //[DefaultValue(true)]
        //[Category("DPS")]
        //[DisplayName("Use AOE")]
        //[Description("True / False if you would like the cc to use AOE with more than 3 mobs")]
        //public bool UseWarriorAOE { get; set; }

        //[Setting]
        //[DefaultValue(false)]
        //[Category("DPS")]
        //[DisplayName("T12 2-Piece")]
        //[Description("True / False if you have the T12 2-piece set bonus")]
        //public bool UseWarriorT12 { get; set; }

        //[Setting]
        //[DefaultValue(true)]
        //[Category("DPS")]
        //[DisplayName("Force proper stance?")]
        //[Description("True / False on whether you would like the cc to keep the toon in the proper stance for the spec. Arms:Battle, Fury:Berserker")]
        //public bool UseWarriorKeepStance { get; set; }

        //[Setting]
        //[DefaultValue(true)]
        //[Category("DPS")]
        //[DisplayName("Use Charge/Intercept/Heroic Leap?")]
        //[Description("True / False if you would like the cc to use any gap closers")]
        //public bool UseWarriorCloser { get; set; }

        #endregion

        #region Arms


        [Setting]
        [DefaultValue(true)]
        [Category("Arms Spec")]
        [DisplayName("Enable Stance Dance")]
        [Description("When this is set to true, CLU will automaticly switch between Stances")]
        public bool EnableStanceDance
        {
            get;
            set;
        }

        //[Setting]
        //[DefaultValue(true)]
        //[Category("Arms")]
        //[DisplayName("Improved Slam Talented?")]
        //[Description("True / False if you have Improved Slam Talented")]
        //public bool UseWarriorSlamTalent { get; set; }

        //[Setting]
        //[DefaultValue(true)]
        //[Category("Arms")]
        //[DisplayName("Bladestorm?")]
        //[Description("True / False if you would like the cc to use bladestorm")]
        //public bool UseWarriorBladestorm { get; set; }

        //[Setting]
        //[DefaultValue(true)]
        //[Category("Arms")]
        //[DisplayName("Throwdown?")]
        //[Description("True / False if you would like the cc to use Throwdown")]
        //public bool UseWarriorThrowdown { get; set; }

        #endregion

        #region Fury

        //[Setting]
        //[DefaultValue(false)]
        //[Category("Fury")]
        //[DisplayName("Use SMF Rotation")]
        //[Description("True / False if you would like the cc to use a SMF rotation")]
        //public bool UseWarriorSMF { get; set; }

        #endregion

    }
}