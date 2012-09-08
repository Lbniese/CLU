using CLU.Helpers;
using Styx.TreeSharp;
using CommonBehaviors.Actions;
using CLU.Settings;
using CLU.Base;

namespace CLU.Classes.Warrior
{
    class Protection : RotationBase
    {        

        public override string Name
        {
            get { return "Protection Warrior"; }
        }

        public override string KeySpell
        {
            get { return "Shield Slam"; }
        }

        public override float CombatMaxDistance
        {
            get { return 3.2f; }
        }

        // adding some help
        public override string Help
        {
            get
            {
                return "----------------------------------------------------------------------\n" +
                      "This Rotation will:\n" +
                      "1. Heal using Victory Rush\n" +
                      "==>             Rallying Cry if it detects player below 20%, Healthstone \n" +
                      "==>             Last Stand with Enraged Regeneration (or without)\n" +
                      "2. AutomaticCooldowns has: \n" +
                      "==> UseTrinkets \n" +
                      "==> UseRacials \n" +
                      "==> UseEngineerGloves \n" +
                      "==> Earthen Potion & Recklessness & BattleShout & Demoralizing Shout\n" +
                      "4. Best Suited for end game raiding\n" +
                      "NOTE: PvP uses single target rotation - It's not designed for PvP use. \n" +
                      "Credits to Jamjar0207\n" +
                      "----------------------------------------------------------------------\n";
            }
        }

        public override Composite SingleRotation
        {
            get
            {
               return new PrioritySelector(
                    // Pause Rotation
                    new Decorator(ret => CLUSettings.Instance.PauseRotation, new ActionAlwaysSucceed()),

                    // For DS Encounters.
                    EncounterSpecific.ExtraActionButton(),

                    // Trinkets & Cooldowns
                        new Decorator(
                            ret => Me.CurrentTarget != null && Unit.IsTargetWorthy(Me.CurrentTarget), 
                                new PrioritySelector(
                                        Item.UseTrinkets(), 
                                        Spell.UseRacials(), 
                                        Item.UseEngineerGloves(), 
                                        Buff.CastBuff("Lifeblood", ret => true, "Lifeblood"), 
                                        Item.UseBagItem("Earthen Potion",      ret => Me.HealthPercent < 35, "Earthen Potion"),
                                        Spell.CastSelfSpell("Recklessness",    ret => Me.CurrentTarget != null && (Me.CurrentTarget.HealthPercent < 20 || Buff.UnitHasHasteBuff(Me)) && Me.HealthPercent > 80, "Recklessness"))), 
                    // Interupts
                    Spell.CastInterupt("Pummel",                   ret => true, "Pummel"), 
                    Spell.CastInterupt("Shockwave",                ret => true, "Shockwave"),
                    Spell.CastInterupt("Spell Reflection",         ret => Me.CurrentTarget != null && Me.CurrentTarget.CurrentTarget == Me, "Spell Reflection"),                    
                    Spell.CastInterupt("Concussion Blow",          ret => true, "Concussion Blow"),
                    // Spell.CastInterupt("Heroic Throw",          ret => true, "Heroic Throw"), //if you are gag order talented
                    // START AoE
                    new Decorator(
                        ret => Unit.CountEnnemiesInRange(Me.Location, 12) >= 3, 
                        new PrioritySelector(
                            // Spell.CastAreaSpell("Intimidating Shout", 8, false, 3, 0.0, 0.0, ret => true, "Intimidating Shout"), //only use if glyphed
                            Spell.CastSelfSpell("Retaliation",     ret => true, "Retaliation"),
                            Spell.CastSelfSpell("Inner Rage",      ret => Me.RagePercent >= 85 && !Buff.PlayerHasBuff("Berserker Rage"), "Inner Rage"),
                            Spell.CastAreaSpell("Thunder Clap", 10, false, 3, 0.0, 0.0, ret => Buff.TargetHasDebuff("Rend"), "Thunder Clap"),
                            Spell.CastConicSpell("Shockwave", 11f, 33f, ret => true, "Shockwave"),
                            Spell.CastAreaSpell("Cleave", 10, true, 3, 0.0, 0.0, ret => Me.RagePercent > 50, "Cleave"), 
                            Buff.CastDebuff("Rend",                 ret => true, "Rend"),
                            Spell.CastSpell("Revenge",             ret => true, "Revenge"),
                            Spell.CastSpell("Shield Slam",         ret => Buff.PlayerHasActiveBuff("Sword and Board"), "Shield Slam (Sword and Board)"),
                            Spell.CastSelfSpell("Shield Block",    ret => true, "Shield Block"),
                            Buff.CastDebuff("Demoralizing Shout",   ret => Me.CurrentTarget != null && !Buff.UnitHasWeakenedBlows(Me.CurrentTarget), "Demoralizing Shout"),
                            Spell.CastSpell("Shield Slam",         ret => true, "Shield Slam")
                            )),
                    // START Main Rotation
                    Spell.CastSpell("Shield Slam",             ret => Buff.PlayerHasActiveBuff("Sword and Board"), "Shield Slam (Sword and Board)"),
                    Spell.CastSpell("Heroic Strike",           ret => Me.RagePercent >= 60, "Heroic Strike Rage Dump"),
                    Spell.CastSelfSpell("Inner Rage",          ret => Me.RagePercent >= 85 && !Buff.PlayerHasBuff("Berserker Rage"), "Inner Rage"), 
                    Spell.CastSelfSpell("Berserker Rage",      ret => !Buff.PlayerHasBuff("Inner Rage"), "Berserker Rage"), 
                    Spell.CastSelfSpell("Shield Block",        ret => true, "Shield Block"),
                    Spell.CastSpell("Shield Slam",             ret => true, "Shield Slam"),
                    Spell.CastSpell("Devastate", ret => Me.CurrentTarget != null && Spell.SpellCooldown("Shield Slam").TotalSeconds > 1.5 && Me.RagePercent < 60 && Buff.TargetHasDebuff("Rend") && Buff.UnitHasWeakenedBlows(Me.CurrentTarget), "Devastate waiting for Shield Slam"), //removed UnitHasAttackSpeedDebuff no longer ingame.
                    Spell.CastSpell("Revenge",                 ret => true, "Revenge"),
                    Spell.CastSpell("Devastate",               ret => Me.CurrentTarget != null && !Buff.UnitHasWeakenedArmor(Me.CurrentTarget) && Buff.TargetCountDebuff("Sunder Armor") < 3, "Devastate (Sunder Armor < 3)"),
                    Buff.CastDebuff("Demoralizing Shout",       ret => Me.CurrentTarget != null && !Buff.UnitHasWeakenedBlows(Me.CurrentTarget), "Demoralizing Shout"),
                    Buff.CastDebuff("Rend",                     ret => true, "Rend"),
                      //removed thunderclap with UnitHasAttackSpeedDebuff no longer in the game TODO: Change this rotation to suit --wulf
                    Spell.CastSpell("Devastate",               ret => true, "Devastate"),
                    Spell.CastConicSpell("Shockwave", 11f, 33f, ret => true, "Shockwave"),
                    Spell.CastSpell("Commanding Shout",        ret => Me.RagePercent < 40 && CLUSettings.Instance.Warrior.ShoutSelection == WarriorShout.Commanding, "Commanding Shout for Rage"),
                    Spell.CastSpell("Battle Shout",            ret => Me.RagePercent < 40 && CLUSettings.Instance.Warrior.ShoutSelection == WarriorShout.Battle, "Battle Shout for Rage"));
            }
        }

        public override Composite Medic
        {
            get
            {
                return new Decorator(
                    ret => Me.HealthPercent < 100 && CLUSettings.Instance.EnableSelfHealing, 
                    new PrioritySelector(
                        Spell.CastSpell("Victory Rush",                ret => Me.HealthPercent < 80, "Victory Rush"),
                        Spell.CastSelfSpell("Last Stand",              ret => Me.HealthPercent < 40 && !Buff.PlayerHasBuff("Shield Wall") && !Buff.PlayerHasBuff("Rallying Cry") && !Buff.PlayerHasBuff("Enraged Regeneration"), "Last Stand"),
                        Spell.CastSelfSpell("Shield Wall",             ret => Me.HealthPercent < 40 && !Buff.PlayerHasBuff("Last Stand") && !Buff.PlayerHasBuff("Rallying Cry") && !Buff.PlayerHasBuff("Enraged Regeneration"), "Shield Wall"),
                        Spell.CastSelfSpell("Rallying Cry",            ret => Me.HealthPercent > 60 && !Buff.PlayerHasBuff("Last Stand") && !Buff.PlayerHasBuff("Shield Wall") && !Buff.PlayerHasBuff("Enraged Regeneration") && Unit.WarriorRallyingCryPlayers, "Rallying Cry - Somebody needs me!"),
                        Spell.CastSelfSpell("Enraged Regeneration",    ret => Me.HealthPercent < 40 && (Buff.PlayerHasBuff("Rallying Cry") || Buff.PlayerHasBuff("Last Stand")) && !Buff.PlayerHasBuff("Shield Wall"), "Enraged Regeneration"),
                        Spell.CastSelfSpell("Enraged Regeneration",    ret => Me.HealthPercent < 40 && !Buff.PlayerHasBuff("Rallying Cry") && !Buff.PlayerHasBuff("Last Stand") && !Buff.PlayerHasBuff("Shield Wall"), "Enraged Regeneration"),
                        Item.UseBagItem("Healthstone",                 ret => Me.HealthPercent < 40, "Healthstone")));                                                            
            }
        }

        public override Composite PreCombat
        {
            get
            {
                return new PrioritySelector(
                        new Decorator(
                            ret => !Me.Mounted && !Me.IsDead && !Me.Combat && !Me.IsFlying && !Me.IsOnTransport && !Me.HasAura("Food") && !Me.HasAura("Drink"),
                            new PrioritySelector(
                                Buff.CastRaidBuff("Commanding Shout",   ret => true, "Commanding Shout"),
                                Buff.CastRaidBuff("Battle Shout",       ret => true, "Battle Shout"))));
            }
        }

        public override Composite Resting
        {
            get { return Rest.CreateDefaultRestBehaviour(); }
        }

        public override Composite PVPRotation
        {
            get { return this.SingleRotation; }
        }

        public override Composite PVERotation
        {
            get { return this.SingleRotation; }
        }
    }
}