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

using System;
using System.Windows.Forms;

namespace CLU.GUI
{
    using System.Globalization;

    using Styx;
    using Styx.CommonBot;
    using Styx.WoWInternals;

    public partial class Spellchecker : Form
    {
         private static Spellchecker instance = new Spellchecker();

        public static void Display()
        {
            if (instance == null || instance.IsDisposed)
                instance = new Spellchecker();
            if (!instance.Visible)
                instance.Show();
        }

        private Spellchecker()
        {
            this.InitializeComponent();
            GUIHelpers.SetDoubleBuffered(this.panel1);
        }

        private void update()
        {
            var spellId = spellid_input.Value;
            var spellname = spellname_txt.Text;
            CurrentEclipse_lbl.Text = StyxWoW.Me.CurrentEclipse.ToString(CultureInfo.InvariantCulture);
            ChanneledSpellID_lbl.Text = StyxWoW.Me.ChanneledCastingSpellId.ToString(CultureInfo.InvariantCulture);

            WoWSpell spell;
            SpellManager.Spells.TryGetValue(spellname, out spell);
            // Fishing for KeyNotFoundException's yay!
            if (spell != null)
            {
                spellId = spell.Id;
            }
                
                WoWSpell test = WoWSpell.FromId((int)spellId);
                
                // populate values
                if (test != null)
                {
                    spellid_lbl.Text = test.Id.ToString(CultureInfo.InvariantCulture);
                    smHasSpell_lbl.Text  = SpellManager.HasSpell(test).ToString(CultureInfo.InvariantCulture);
                    smCanCast_lbl.Text = SpellManager.CanCast(test).ToString(CultureInfo.InvariantCulture);
                    spellname_lbl.Text = test.Name;
                    spellCancast_lbl.Text = test.CanCast.ToString(CultureInfo.InvariantCulture);
                    smIsValid_lbl.Text = test.IsValid.ToString(CultureInfo.InvariantCulture);
                    smCooldownTimeLeftid_lbl.Text = test.CooldownTimeLeft.ToString();
                    smCooldownid_lbl.Text = test.Cooldown.ToString(CultureInfo.InvariantCulture);
                    smCooldownTimeLeft_lbl.Text = SpellManager.Spells[spellname].CooldownTimeLeft.ToString();
                }


        }

        

        void Timer1Tick(object sender, EventArgs e)
        {
            try
            {
                this.update();
            }
            catch { }
        }
    }
}
