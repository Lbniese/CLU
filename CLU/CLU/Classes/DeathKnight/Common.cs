
/*
todo:                                                               Done by:
 * add pestilence with glyph                                                   : by Weischbier 15:46h GMT 31.08.2012
 * revisit unholy with rune power management                                   : Pending
 * revisit frost...not happy                                                   : Pending
 * add apply diseases composit, its all the same for any spec                  : by Weischbier 15:34h GMT 31.08.2012
 * add CanPlagueLeech to rotations                                             : by Weischbier 15:34h GMT 31.08.2012
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
                //CLU.TroubleshootDebugLog(Color.ForestGreen, "IsWieldingBigWeapon : {0}", ex);//Yeah yeah...copy and paste :D haha
                CLU.TroubleshootDebugLog(Color.ForestGreen, "CanPlagueLeech : {0}", ex);
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
                                        new PrioritySelector(
                                        Spell.CastAreaSpell("Pestilence", 10, false, CLUSettings.Instance.DeathKnight.BloodPestilenceCount, 0.0, 0.0, ret => !TalentManager.HasGlyph("Pestilence") && Buff.TargetHasDebuff("Blood Plague") && Buff.TargetHasDebuff("Frost Fever") && (from enemy in Unit.EnemyUnits where !enemy.HasAura("Blood Plague") && !enemy.HasAura("Frost Fever") select enemy).Any(), "Pestilence"),
                                        Spell.CastAreaSpell("Pestilence", 14.5/*Give it a tolerance, it's never perfect*/, false, CLUSettings.Instance.DeathKnight.BloodPestilenceCount, 0.0, 0.0, ret => TalentManager.HasGlyph("Pestilence") && Buff.TargetHasDebuff("Blood Plague") && Buff.TargetHasDebuff("Frost Fever") && (from enemy in Unit.EnemyUnits where !enemy.HasAura("Blood Plague") && !enemy.HasAura("Frost Fever") select enemy).Any(), "Pestilence"))),
                                    new SwitchArgument<DeathKnightTierOneTalent>(DeathKnightTierOneTalent.UnholyBlight,
                                        Spell.CastAreaSpell("Unholy Blight", 10, false, CLUSettings.Instance.DeathKnight.UnholyBlightCount, 0.0, 0.0, ret => (from enemy in Unit.EnemyUnits where !enemy.HasAura("Blood Plague") && !enemy.HasAura("Frost Fever") select enemy).Any(), "Unholy Blight")),
                                    new SwitchArgument<DeathKnightTierOneTalent>(DeathKnightTierOneTalent.RoilingBlood,
                                        Spell.CastAreaSpell("Blood Boil", 10, false, CLUSettings.Instance.DeathKnight.RoilingBloodCount, 0.0, 0.0, ret => (from enemy in Unit.EnemyUnits where !enemy.HasAura("Blood Plague") && !enemy.HasAura("Frost Fever") select enemy).Any(), "Blood Boil for Roiling Blood")))
                        
                       ))));
        }

        /// <summary>
        /// Applys your Diseases depending on what disease is falling off or utilizes Plague Leech when Outbreak comes off cooldown.
        /// </summary>
        /// <param name="onUnit">the unit to begin spreading Diseases on</param>
        public static Composite ApplyDiseases(CLU.UnitSelection onUnit)
        {
            return
                new Decorator(ret => onUnit != null && onUnit(ret) != null,
                    new PrioritySelector(
                    //Diseases -- The most important stuff for playing a Death Knight is keeping them up at all times
                    Spell.CastSpell("Plague Leech" , ret => CanPlagueLeech(), "Plague Leech"), // should be used just as your diseases are about to expire, and each time that you can refresh them right away with Outbreak
                    Spell.CastSpell("Outbreak"     , ret => Buff.TargetDebuffTimeLeft("Blood Plague").TotalSeconds < 0.5 ||Buff.TargetDebuffTimeLeft("Frost Fever").TotalSeconds < 0.5, "Outbreak"),
                    Spell.CastSpell("Howling Blast", ret => Spell.SpellOnCooldown("Outbreak") && Buff.TargetDebuffTimeLeft("Frost Fever").TotalSeconds < 0.5,"Howling Blast (Frost Fever)"),
                    Spell.CastSpell("Plague Strike", ret => Spell.SpellOnCooldown("Outbreak") && Buff.TargetDebuffTimeLeft("Blood Plague").TotalSeconds < 0.5, "Plague Strike"))
                    );
        }
    }
}
