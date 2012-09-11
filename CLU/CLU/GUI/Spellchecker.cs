using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace CLU.GUI
{
    using System.Globalization;

    using Styx;
    using Styx.CommonBot;
    using Styx.WoWInternals;

    using global::CLU.Base;

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
                var spellname = spellname_txt.Text;
                var spellId = spellid_input.Value;

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
