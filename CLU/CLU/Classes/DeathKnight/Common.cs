
/*
todo:                                                               Done by:
 * add pestilence with glyph                                                   :
 * revisit unholy with rune power management                                   :
 * revisit frost...not happy                                                   :
 * add apply diseases composit, its all the same for any spec                  :
 * add CanPlagueLeech to rotations                                             :
*/
namespace Clu.Classes.DeathKnight
{
    using System;
    using System.Drawing;
    using System.Linq;
    using Clu;
    using Clu.Helpers;
    using Clu.Settings;
    using Styx;
    using Styx.WoWInternals.WoWObjects;
    using TreeSharp;

    /// <summary>
    /// Common Deathnight Functions.
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

        /// <summary>
        /// Checks if the player is weilding a TwoHanded Weapon
        /// </summary>
        /// <returns>returns true if the player is weilding a TwoHanded Weapon</returns>
        public static bool IsWieldingTwoHandedWeapon()
        {
            try
            {
                switch (Me.Inventory.Equipped.MainHand.ItemInfo.WeaponClass)
                {
                    case WoWItemWeaponClass.ExoticTwoHand:
                    case WoWItemWeaponClass.MaceTwoHand:
                    case WoWItemWeaponClass.AxeTwoHand:
                    case WoWItemWeaponClass.SwordTwoHand:
                        return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                CLU.TroubleshootDebugLog(Color.ForestGreen, "IsWieldingBigWeapon : {0}", ex);
            }

            return false;
        }

        /// <summary>
        /// Counts the amount of runes we have
        /// </summary>
        public static double RuneCalculus
        {
            get
            {
                try
                {
                    return Spell.RuneCooldown(1) + Spell.RuneCooldown(2) + Spell.RuneCooldown(3) + Spell.RuneCooldown(4) + Spell.RuneCooldown(5) + Spell.RuneCooldown(6);
                }
                catch (Exception ex)
                {
                    CLU.TroubleshootDebugLog(Color.ForestGreen, "RuneCalculus : {0}", ex);
                }

                return 0;
            }
        }

        /// <summary>
        /// Checks if we can use Plague Leech
        /// We need to use Plague Leech right before Outbreak drops off so we dont 
        /// waste two globals on Icy Touch and Plague strike.
        /// Of course we are checking we have a rune to refresh!
        /// </summary>
        /// <returns>true if we have Blood Plague and Frost Fever up and Outbreak is about to come off cooldown.</returns>
        public static bool CanPlagueLeech()
        {
            try
            {
                return (Spell.RuneCooldown(1) > 1 && Spell.RuneCooldown(2) > 1 && Spell.RuneCooldown(5) > 1
                        && Spell.RuneCooldown(6) > 1 && (Spell.RuneCooldown(3) > 1 && Spell.RuneCooldown(4) == 0 || Spell.RuneCooldown(3) == 0 && Spell.RuneCooldown(4) > 1)) 
                        && (Buff.TargetDebuffTimeLeft("Blood Plague").TotalSeconds < 1
                        && Buff.TargetDebuffTimeLeft("Frost Fever").TotalSeconds < 1)
                        && Spell.SpellCooldown("Outbreak").TotalSeconds < 1 
                        && Buff.TargetHasDebuff("Blood Plague")
                        && Buff.TargetHasDebuff("Frost Fever");
            }
            catch (Exception ex)
            {
                CLU.TroubleshootDebugLog(Color.ForestGreen, "IsWieldingBigWeapon : {0}", ex);
            }

            return false;
        }

        /// <summary>
        /// Spreads your Diseases depending on what Tier One Talent you have
        /// </summary>
        /// <param name="onUnit">the unit to begin spreading Diseases on</param>
        public static Composite SpreadDiseasesBehavior(CLU.UnitSelection onUnit)
        {
            return new PrioritySelector(
                new Decorator(
                    ret => onUnit != null && onUnit(ret) != null && onUnit(ret).DistanceSqr < 40 * 40 
                        && CLUSettings.Instance.DeathKnight.DeathKnightTierOneTalent != DeathKnightTierOneTalent.None,
                    new PrioritySelector(
                        new Sequence(
                                new Switch<DeathKnightTierOneTalent>(ctx => CLUSettings.Instance.DeathKnight.DeathKnightTierOneTalent,
                                    new SwitchArgument<DeathKnightTierOneTalent>(DeathKnightTierOneTalent.PlagueLeech,
                                        Spell.CastAreaSpell("Pestilence", 10, false, CLUSettings.Instance.DeathKnight.BloodPestilenceCount, 0.0, 0.0, ret => Buff.TargetHasDebuff("Blood Plague") && Buff.TargetHasDebuff("Frost Fever") && (from enemy in Unit.EnemyUnits where !enemy.HasAura("Blood Plague") && !enemy.HasAura("Frost Fever") select enemy).Any(), "Pestilence")),
                                    new SwitchArgument<DeathKnightTierOneTalent>(DeathKnightTierOneTalent.UnholyBlight,
                                        Spell.CastAreaSpell("Unholy Blight", 10, false, CLUSettings.Instance.DeathKnight.UnholyBlightCount, 0.0, 0.0, ret => (from enemy in Unit.EnemyUnits where !enemy.HasAura("Blood Plague") && !enemy.HasAura("Frost Fever") select enemy).Any(), "Unholy Blight")),
                                    new SwitchArgument<DeathKnightTierOneTalent>(DeathKnightTierOneTalent.RoilingBlood,
                                        Spell.CastAreaSpell("Blood Boil", 10, false, CLUSettings.Instance.DeathKnight.RoilingBloodCount, 0.0, 0.0, ret => (from enemy in Unit.EnemyUnits where !enemy.HasAura("Blood Plague") && !enemy.HasAura("Frost Fever") select enemy).Any(), "Blood Boil for Roiling Blood")))
                        
                       ))));
        }
    }
}
