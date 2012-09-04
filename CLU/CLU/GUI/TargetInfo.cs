using System;
using System.Windows.Forms;

using Styx.WoWInternals;

namespace CLU.GUI
{
    using System.Drawing;
    using System.Globalization;

    using global::CLU.Base;

    using Styx;

    public partial class TargetInfo : Form
    {
        private static TargetInfo instance = new TargetInfo();
        // private static readonly HealableUnit Myself = new HealableUnit(StyxWoW.Me).TimeToLive(2);

        public static void Display()
        {
            if (instance == null || instance.IsDisposed)
                instance = new TargetInfo();
            if (!instance.Visible)
                instance.Show();
        }

        private TargetInfo()
        {
            this.InitializeComponent();
        }

        private void update()
        {

            var target = ObjectManager.Me.CurrentTarget;
            var color = StyxWoW.Me.IsBehind(target) ? Color.DarkGreen : Color.Red;


            tname.Text = target == null ? "<No target>" : target.Name;
            tguid.Text = target == null ? string.Empty : target.Guid.ToString(CultureInfo.InvariantCulture);
            distance.Text = target == null ? string.Empty : Math.Round(target.Distance, 3).ToString(CultureInfo.InvariantCulture);
            realDistance.Text = target == null ? string.Empty : Math.Round(Unit.DistanceToTargetBoundingBox(target), 3).ToString(CultureInfo.InvariantCulture);
            issafelybehindtarget.ForeColor = color;
            issafelybehindtarget.Text = (target != null && StyxWoW.Me.IsBehind(target)).ToString(CultureInfo.InvariantCulture);
            // ttl_label.Text = new HealableUnit(StyxWoW.Me.CurrentTarget).TimeToLive(4).ToString(CultureInfo.InvariantCulture);


            if (target != null) {
                // facing
                var me = ObjectManager.Me.Location;
                var ta = target.Location;
                facing.Text = Math.Round(Unit.FacingTowardsUnitDegrees(me, ta), 2) + "°";
            }
        }

        void Timer1Tick(object sender, EventArgs e)
        {
            try {
                this.update();
            } catch { }
        }
    }
}
