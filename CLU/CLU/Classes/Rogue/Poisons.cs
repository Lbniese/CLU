// Full Credit to singular for this class (why re-invent the wheel?)
// edited for MoP (8/09/2012)

namespace CLU.Classes.Rogue
{
    using CommonBehaviors.Actions;
    using Styx;
    using Styx.CommonBot;
    using Styx.Pathing;
    using Styx.TreeSharp;

    using global::CLU.Base;
    using global::CLU.Settings;

    public static class Poisons
    {

        public static bool NeedsPoison
        {
            get {
                return StyxWoW.Me.Inventory.Equipped.MainHand != null && 
                    StyxWoW.Me.Inventory.Equipped.MainHand.TemporaryEnchantment.Id == 0 && 
                    StyxWoW.Me.Inventory.Equipped.MainHand.ItemInfo.WeaponClass != WoWItemWeaponClass.FishingPole;
            }
        }

        public static int WantedPoison
        {
            get {
                switch (CLUSettings.Instance.Rogue.WantedPoison)
                {
                case PoisonType.Crippling:
                        return 3408;
                case PoisonType.MindNumbing:
                        return 571;
                case PoisonType.Deadly:
                        return 2823;
                case PoisonType.Wound:
                        return 8679;
                default:
                    return 0;
                }
            }
        }   

        public static Composite CreateApplyPoisons()
        {
            return new PrioritySelector(
                       new Decorator(
                           ret => NeedsPoison && !StyxWoW.Me.HasAura(WantedPoison),
                           new Sequence(
                               new Action(ret => CLU.TroubleshootLog("Applying {0} to main hand", CLUSettings.Instance.Rogue.WantedPoison)),
                               new Action(ret => Navigator.PlayerMover.MoveStop()),
                               Spell.CreateWaitForLagDuration(),
                               new Action(ret => SpellManager.CastSpellById(WantedPoison)),
                               Spell.CreateWaitForLagDuration(),
                               new WaitContinue(2, ret => StyxWoW.Me.IsCasting, new ActionAlwaysSucceed()),
                               new WaitContinue(10, ret => !StyxWoW.Me.IsCasting, new ActionAlwaysSucceed()),
                               new WaitContinue(1, ret => false, new ActionAlwaysSucceed())))
                   );
        }
    }
}
