#region Revision Info

// This file was part of Singular - A community driven Honorbuddy CC
// $Author: Laria $
// $LastChangedBy: Laria$
#endregion
using System.ComponentModel;

using Styx.Helpers;

using DefaultValue = Styx.Helpers.DefaultValueAttribute;

namespace Clu.Settings
{
    using global::CLU.Base;

    internal class MonkSettings : Styx.Helpers.Settings
    {
        public MonkSettings()
        : base(CLUSettings.SettingsPath + "_Monk.xml")
        {
        }

        #region Common

        [Setting]
        [DefaultValue(MonkLegacy.Emperor)]
        [Category("Common")]
        [DisplayName("Monk Legacy Selector")]
        [Description("Choose a Monk Legacy. This is on applicable to solo play (ie: not in party or raid.)")]
        public MonkLegacy LegacySelection
        {
            get;
            set;
        }

        #endregion
    }
}