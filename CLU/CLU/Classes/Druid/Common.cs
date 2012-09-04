using Clu.Helpers;
using Clu.Settings;
using Styx;
using Styx.WoWInternals.WoWObjects;
using TreeSharp;

namespace Clu.Classes.Druid
{
    using global::CLU.Base;

    /// <summary>
    /// Common Druid Functions.
    /// </summary>
    public static class Common
    {

        /// <summary>
        /// erm..me
        /// </summary>
        private static LocalPlayer Me
        {
            get
            {
                return StyxWoW.Me;
            }
        }

        public static bool IsBehind
        {
            get
            {
                return Me.CurrentTarget != null && Me.CurrentTarget.MeIsBehind;
            }
        }

        /// <summary>
        /// Handles Shapeshift form.
        /// </summary>
        public static Composite HandleShapeshiftForm
        {
            get
            {
                return new Decorator(
                         ret => CLUSettings.Instance.EnableMovement,
                               new PrioritySelector(
                                   Spell.CastSelfSpell("Bear Form", ret => Me.HealthPercent <= 20 && !Buff.PlayerHasBuff("Bear Form"), "Bear Form"),
                                   Spell.CastSelfSpell("Cat Form", ret => Me.HealthPercent > 20 && !Buff.PlayerHasBuff("Cat Form"), "Cat Form")));
            }
        } 
    }
}
