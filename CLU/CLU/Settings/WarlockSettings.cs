﻿#region Revision info
/*
 * $Author$
 * $Date$
 * $ID$
 * $Revision$
 * $URL$
 * $LastChangedBy$
 * $ChangesMade$
 */
#endregion

// This file was part of Singular - A community driven Honorbuddy CC
using System.IO;
using CLU.Base;
using System.ComponentModel;
using Styx.Helpers;
using DefaultValue = Styx.Helpers.DefaultValueAttribute;


namespace CLU.Settings
{

    internal class WarlockSettings : Styx.Helpers.Settings
    {
        public WarlockSettings()
            : base(Path.Combine(CLUSettings.SettingsPath, "Warlock.xml"))
        {
        }

        #region Common
        [Setting]
        [DefaultValue(PetType.None)]
        [Category("Pet Selection")]
        [DisplayName("Select Pet to Summon")]
        [Description("Choose your Pet to Summon. Default Felhunter, if not learned Summon Imp")]
        public PetType CurrentPetType
        {
            get;
            set;
        }

        #endregion

        #region Affliction
        [Setting]
        [DefaultValue(4)]
        [Category("Affliction")]
        [DisplayName("Seed of Corruption Adds")]
        [Description("Choose a Number to start with Seed of Corruption instead of Multidotting (Soulburn + Soulswap), set to 1 or 0 to disable it")]
        public int SeedOfCorruptionCount
        {
            get;
            set;
        }
        #endregion

        #region Demonology


        [Setting]
        [DefaultValue(false)]
        [Category("Demonology Spec")]
        [DisplayName("Dark Apotheosis Tanking")]
        [Description("When this is set to true, CLU will tank using Dark Apotheosis")]
        public bool ImTheFuckingBoss
        {
            get;
            set;
        }


        #endregion

        #region Destruction

        [Setting]
        [DefaultValue(true)]
        [Category("Destruction Spec")]
        [DisplayName("Automaticly Apply Bane Of Havoc")]
        [Description("When this is set to true, CLU will always apply Bane Of Havoc to the most appropriate target. If the player has a focus target it will apply bane of havoc on that target otherwise it will choose the closest target that is not crowdcontrolled")] // TODO: Describe how CLU selects an appropriate target.
        public bool ApplyBaneOfHavoc
        {
            get;
            set;
        }

        #endregion

    }
}