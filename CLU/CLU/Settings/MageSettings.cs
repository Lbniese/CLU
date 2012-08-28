#region Revision Info

// This file was part of Singular - A community driven Honorbuddy CC
// $Author: highvoltz $
// $LastChangedBy: wulf $

#endregion

namespace Clu.Settings
{
    internal class MageSettings : Styx.Helpers.Settings
    {
        public MageSettings()
        : base(CLUSettings.SettingsPath + "_Mage.xml")
        {

        }

        #region Common

        //[SettingAttribute]
        //[DefaultValue(true)]
        //[Category("Common")]
        //[DisplayName("Summon Table If In A Party")]
        //[Description("Summons a food table instead of using conjured food if in a party")]
        //public bool SummonTableIfInParty { get; set; }

        #endregion

        #region Frost
        #endregion

        #region Fire
        #endregion

        #region Arcane
        #endregion
    }
}