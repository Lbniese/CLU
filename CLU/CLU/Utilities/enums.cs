namespace Clu
{
    using System;

    // stop pollution of the namespace in random classes

    public enum TrinketUsage {
        Never,
        OnCooldown,
        OnCooldownInCombat,
        LowPower,
        LowHealth,
        CrowdControlled,
        CrowdControlledSilenced
    }

    public enum PetSlot
    {
        FirstSlot = 1,
        SecondSlot,
        ThirdSlot,
        FourthSlot,
        FifthSlot
    }

     public enum Burst {
        onBoss,
        onMob
    }

     public enum DeathKnightTierOneTalent
     {
         PlagueLeech,
         UnholyBlight,
         RoilingBlood,
         None
     }

    public enum HealingAquisitionMethod
    {
        Proximity,
        RaidParty
    }


    public enum PaladinAura {
        Auto,
        Devotion,
        Retribution,
        Resistance,
        Concentration,
    }

    public enum DruidForm {
        None,
        Bear,
        Cat,
        Moonkin
    }


    public enum ShadowPriestRotation {
        Leveling,
        Default,
        MindSpike
    }

    public enum SubtletyRogueRotation {
        Default,
        ImprovedTestVersion
    }

    public enum WarriorShout {
        None,
        Battle,
        Commanding
    }

    public enum ChakraStance {
        None,
        Serenity,
        Sanctuary
    }

    public enum PoisonType {
        Instant,
        Crippling,
        MindNumbing,
        Deadly,
        Wound
    }

    public enum PetType {
        // These are CreatureFamily IDs. See 'CurrentPet' for usage.
        None = 0,
        Imp = 23,
        Felguard = 29,
        Voidwalker = 16,
        Felhunter = 15,
        Succubus = 17,
    }

    public enum OracleWatchMode {
        Healer,
        DPS,
        Tank
    }

    public enum GroupType {
        Single,
        Party,
        Raid
    }

    public enum GroupLogic {
        PVE,
        Battleground,
        Arena
    }

    [Flags]
    public enum TargetFilter {
        None = 0,
        Tanks = 1,
        Healers = 2,
        Damage = 4,
        FlagCarrier = 5,
        EnemyHealers = 6,
        Threats = 7,
        LowHealth = 8,
        MostFocused = 9
    }

    public enum Keyboardfunctions {
        Nothing, // - default
        IsAltKeyDown, // - Returns whether an Alt key on the keyboard is held down.
        IsControlKeyDown, // - Returns whether a Control key on the keyboard is held down
        IsLeftAltKeyDown, // - Returns whether the left Alt key is currently held down
        IsLeftControlKeyDown, // - Returns whether the left Control key is held down
        IsLeftShiftKeyDown, // - Returns whether the left Shift key on the keyboard is held down
        IsModifierKeyDown, // - Returns whether a modifier key is held down
        IsRightAltKeyDown, // - Returns whether the right Alt key is currently held down
        IsRightControlKeyDown, // - Returns whether the right Control key on the keyboard is held down
        IsRightShiftKeyDown, // - Returns whether the right shift key on the keyboard is held down
        IsShiftKeyDown, // - Returns whether a Shift key on the keyboard is held down
    }
}
