#region Revision Info

// This file is part of Singular - A community driven Honorbuddy CC
// $Author: raphus $
// $LastChangedBy: wulf $

#endregion

namespace Clu.Settings
{
    internal class HunterSettings : Styx.Helpers.Settings
    {
        public HunterSettings()
        : base(CLUSettings.SettingsPath + "_Hunter.xml")
        {
        }

        #region Pet

        //[Setting]
        //[DefaultValue("1")]
        //[Category("Pet")]
        //[DisplayName("Pet Slot")]
        //public string PetSlot { get; set; }

        //[Setting]
        //[DefaultValue(70)]
        //[Category("Pet")]
        //[DisplayName("Mend Pet Percent")]
        //public double MendPetPercent { get; set; }

        #endregion

        #region Common

        //[Setting]
        //[DefaultValue(false)]
        //[Category("Common")]
        //[DisplayName("Use Disengage")]
        //[Description("Will be used in battlegrounds no matter what this is set")]
        //public bool UseDisengage { get; set; }

        #endregion

        #region Marksmanship
        #endregion

        #region BeastMastery
        #endregion

        #region Survival
        #endregion
    }
}