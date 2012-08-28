CLU (Codified Likeness Utility) the Rotation CC for Combat/Raid/Lazyraider bots Powered by Felmaster technology

Latest Version: 3.0.11 (From SVN).

Current Issues Updated: 26/August/2012
=================================
* [Movement] No Pull Logic (Will cast ranged spells to pull)
* [Movement] PvP Tweaking needed, Use at your own peri

Description
==========
This CC is mainly intended for PVE users of honorbuddy, however it can still be used for PVP it will simply use the PVE Rotation at the moment. (Class Specific PVP rotations may be added in the future).
It will perform its rotation according to online resources such as Elitist Jerks & SimulationCraft, leaving you to monitor more important things such as fight mechanics.

Author's Note
============
This CC was formed as a hobby.
Donations are not permitted.
Support is provided, however I do have a RL job and a wife and children therefore support may be a little slow (Wife rage!).
Constructive criticism is welcome.

How do I use CLU
================
* Follow the "How to get the latest version" in this post
* Copy the CLU folder into %Your Honorbuddy Folder%\CustomClasses
* Load Honorbuddy
* Select "Combat" as your bot base (Lazyraider and RaidBot tested OK)
* Select "Load Profile" and load the BlankProfile.xml (Provided at the end of this post.)
* Start Honorbuddy.
* Stay out of the fire and point to kill.

How to get the latest version
========================
* See latest and greatest attached to the bottom of this post.
* Unz** the files into your %HB FOLDER%\CustomClasses\
* SVN link: http://wulfdev.googlecode.com/svn/trunk/
* Google: How to use tortoisesvn. (Please do not PM HB moderators if you do not know how to use a SVN, PM me instead)


Features
========

Current Rotations
------------------------------

Three Rotation Modes - Single, PVE, PVP
- Key: Tested[T], Untested[U], Partially checked[PC], NotImplemented[N/A]

* DeathKnight
- Blood...................[T]
- Frost...................[T]
- Unholy................[T]

* Druid
- Balance.................[T]
- Feral (bear/Cat)....[T]
- Restoration..........[T]

* Hunter
- BeastMastery.......[T]
- Marksmanship.....[T]
- Survival...............[T]

* Mage
- Arcane.................[T]
- Fire......................[T]
- Frost....................[T]

* Monk
- Brewmaster.........[N/A]
- Mistweaver..........[N/A]
- Windwalker..........[N/A]

* Paladin
- Holy....................[T]
- Protection...........[T]
- Retribution.........[T]

* Priest
- Shadow................[T]
- Discipline............[T]
- Holy....................[T]

* Rogue
- Assassination......[T]
- Combat	...............[T]
- Subtlety...............[T]

* Shaman
- Elemental............[T]		
- Restoration..........[T]
- Enhancement.......[T]

* Warlock
- Affliction.............[T]
- Demonology........[T]
- Destruction.........[T]

* Warrior
- Arms...................[T]
- Protection...........[T]
- Fury....................[T]
	
Utility
-------------

* Trinkets
- Will Use trinkets on cooldown if the target is a boss or a PvP player
- All level 85-90 Trinkets with a "USE" affect supported.

* Racials
- Will Use *most* Racials on cooldown if the target is a boss or a PvP player 

* Totems
- Handles Earth, Water, Air totems (Fire totems are specified within the specific rotation)

* Poisons
- Handles the application of poisons for rogues.

Miscellaneous functionality
-----------------------------------------------

* Multi-Dotting
- When enabled it will cycle threw all targets and apply appropriate DoT's

* Cooldown Management
- Intelligent use of cooldowns by confirming if your target is worthy.
- Combat resurrection (Resto Druid only atm)
- Engineering gloves

* Extra Action button Click
- Whilst in DeathWing (Cataclysm) CLU will click the Extra Action Button during the encounter for you.

* Interrupts
- It will not try to interrupt non-interruptible abilities

* Buffing
- Apply a buff or debuff depending on other players providing that buff or debuff.

* AoE
- Clustered pack detection (for AoE)
- Intelligent AoE - checking for controlled mobs.

* Tier piece detection
- Detection of how many Tier pieces a player is wearing to dynamically change rotation priorities (Only built with feral druid and retribution paladin and Shadow Priest)

* Bag Items Support
- Using a bag item (Healthstone, Flask, Potion)

* Resting/ Self Healing
- Emergency healing via class specific spells/abilities.
- Will Drink and Eat food

Managers
------------------

* Pet Manager
- Handles the calling of pets, pet spells, pet coooldowns

* SpellImunity Manager
- Handles tracking of target immunities to Nature, Fire, Frost, etc

* Bot Management
- Handles checking what the current botbase is and how CLU should set its configuration

* CombatLogEvents Manager
- Handles events that happen within the WoW combat Log, Spell Missed, Target Evading, etc

* EncounterSpecific
- Handles specific functions for encounters 
	
	
Movement
------------------

CLU will take over movement when the botbase (questing, bgbuddy,gatherbuddy,etc) gives control to CLU.

 * Overview:
- Acquire Target
- Face Target
- Move to Location (LoS)
- Blacklist evading targets
- Move behind Target (If enabled in the UI Settings)
- Move towards target if it exceeds minimum/maximum combat distance
- Stop moving if it meets minimum/maximum combat distance
- Considers both melee and ranged characters (i.e. melee will move within melee range, and ranged will stay at maximum range.)

Targeting
------------------

CLU will take over targeting when the botbase (questing, bgbuddy,gatherbuddy,etc) gives control to CLU.

* Target Priority:	
- If we have a RaF leader, then use its target.
- Healers first (Battlegrounds/arenas)
- Enemy’s  Attacking Us  (Battlegrounds/arenas)
- Flag Carrier units (Battlegrounds/arenas)
- Low Health units (Battlegrounds/arenas)
- bot poi.
- Target list (Honorbuddy Targeting)
- MostFocusedUnit (Targets the unit that everyone else in your party is focused on, or targets the closest target to you.)
		

Information
---------------------

* Target
- Information on the current target includes Location, Distance, Name, GUID, isBehind.

* Spells
- Dumps the targets spells (including ID's, Powercost, etc) to the honorbuddy debug log

* Auras
- Dumps the targets Auras to the honorbuddy debug log

Keybinds
------------------

* With Sound Notification
- PauseRotation
- UseCooldowns
- ClickExtraActionButton
- UseAoEAbilities
- EnableRaidPartyBuffing
- HealEnableSelfHealing
- EnableMultiDotting
- EnableInterupts
- ChakraStanceSelection

Statistics
------------------

* Spells cast
* Average APM
* Spell Cast intervals
* Healing report (Spell casts that succeeded)

User Interface
------------------------

* Settings
- General
- Class Specific

* Healing Helper
- Blacklisting Players
- Urgent Dispel selector
- MainTank/Offtank Selection
- Beacon of Light Selection
- Earth Shield Selection
- LifeBloom Selection
- Role Selection (Tanks/Healers/Damage)
- Add/Remove Members

* Debugging
- Target information
- Healing Target Information
- Reports
- Aura/Spell Dumps

* Spell Lock Watcher
- Watchs debuffs on your target and counts down when time remaining is less than 3 seconds.

* Realtime Target Information
- Information on the current target includes Location, Distance, Name, GUID, isBehind.
		

How to report a bug
==================
* Make sure you are using latest version
* Verify that you have followed all the steps listed under the 'How do I use CLU' of this post.
* Please enable debug log within CLU settings before you attatch your log..the more information the better.
* Attach a log (See How to by Kickazz006) http://www.thebuddyforum.com/honorbuddy-forum/guides/35945-guide-how-attach-your-log-kick.html


Credits
=======
* cowdude for his initial work with Felmaster and giving me inspiration to create this CC.
* All the Singular Developers For there tiresome work and supporting code
* bobby53 For his Disclaimer (Couldn't have said it better)
* Kickazz006 for his BlankProfile.xml
* Shaddar & Mentally for thier initial detection code of HourofTwilight, FadingLight and Shrapnel
* bennyquest for his continued efforts in reporting issues!
* Jamjar0207 for his Protection Warrior Rotation and Fire Mage
* gniegsch for his Arms Warrior Rotation and improvements with blood deathknight
* Stormchasing for his help with warlocks and patchs
* Obliv For his Boomkin, Frost Mage & Assassination rotations and Arms warrior improvement
* ShinobiAoshi for his initial Affliction warlock rotation
* fluffyhusky for his initial Enhancement Shaman rotation
* Digitalmocking for his initial Elemental Shaman rotation
* Toney001 for his improvements to the Unholy Deathknight rotation
* kbrebel04  for his improvements to Subtlety Rogue


Disclaimer
=========
Use of 3rd party programs are against Blizzard's terms of use and license.
This CC is not intended for use on live Blizzard Entertainment World of Warcraft realms.
Any use on Blizzard realms is at your own risk and not supported per this statement.


F.A.Q
======

Q. Can CLU tank raids and heroic dungeons ?
A. All Tank/Healing specs provided have been tested for Heroics/Raids.

Q. Will CLU taunt targets for me when tanking?
A. No CLU does not taunt for you.

Q. What Botbase should I use for Raiding/ Heroic Dungeons?
A. Depends on what you want to do. CLU works best with Raidbot but Lazyraider and Combat can be used as well. When Healing Use Lazyraider and turn of Tank selection (aka: Solo Mode)

Q. Where is XYZ setting?
A. If you do not see it in the UI settings its not there yet!

Q. Is there any way for me to have control over blade flurry as a rogue? 
A. Turn off AoE, this is true for all class AoE abilities.


