﻿using Styx.WoWInternals;
using System.Drawing;
using CLU.Settings;
using CLU.Base;
using CLU.Sound;

namespace CLU.Helpers
{

    public static class Keybinds
    {
        /* putting all the Keybind logic here */

        // Sounds from http://www2.research.att.com/~ttsweb/tts/demo.php

        internal static void Pulse()
        {

            // KeybindUseCooldowns \\
            if (IsKeyDown(CLUSettings.Instance.KeybindUseCooldowns)) {
                switch (CLUSettings.Instance.UseCooldowns) {
                case true:
                    CLUSettings.Instance.UseCooldowns = !CLUSettings.Instance.UseCooldowns;
                    CLU.Log(" UseCooldowns= {0}", CLUSettings.Instance.UseCooldowns);
                    if (CLUSettings.Instance.EnableKeybindSounds) {
                        try {
                            SoundManager.LoadSoundFilePath(@"\CustomClasses\CLU\Sound\cooldownsdisabled.wav");
                            SoundManager.SoundPlay();
                        } catch { }
                    }
                    break;
                case false:
                    CLUSettings.Instance.UseCooldowns = !CLUSettings.Instance.UseCooldowns;
                    CLU.Log(" UseCooldowns= {0}", CLUSettings.Instance.UseCooldowns);
                    if (CLUSettings.Instance.EnableKeybindSounds) {
                        try {
                            SoundManager.LoadSoundFilePath(@"\CustomClasses\CLU\Sound\cooldownsenabled.wav");
                            SoundManager.SoundPlay();
                        } catch { }
                    }
                    break;
                default:
                    return;
                }
            }

            // KeybindClickExtraActionButton \\
            if (IsKeyDown(CLUSettings.Instance.KeybindClickExtraActionButton)) {
                switch (CLUSettings.Instance.ClickExtraActionButton) {
                case true:
                    CLUSettings.Instance.ClickExtraActionButton = !CLUSettings.Instance.ClickExtraActionButton;
                    CLU.Log(" ClickExtraActionButton= {0}", CLUSettings.Instance.ClickExtraActionButton);
                    if (CLUSettings.Instance.EnableKeybindSounds) {
                        try {
                            SoundManager.LoadSoundFilePath(@"\CustomClasses\CLU\Sound\dsextraactionbuttondisabled.wav");
                            SoundManager.SoundPlay();
                        } catch { }
                    }
                    break;
                case false:
                    CLUSettings.Instance.ClickExtraActionButton = !CLUSettings.Instance.ClickExtraActionButton;
                    CLU.Log(" ClickExtraActionButton= {0}", CLUSettings.Instance.ClickExtraActionButton);
                    if (CLUSettings.Instance.EnableKeybindSounds) {
                        try {
                            SoundManager.LoadSoundFilePath(@"\CustomClasses\CLU\Sound\dsextraactionbuttonenabled.wav");
                            SoundManager.SoundPlay();
                        } catch { }
                    }
                    break;
                default:
                    return;
                }
            }

            // KeybindUseAoEAbilities \\
            if (IsKeyDown(CLUSettings.Instance.KeybindUseAoEAbilities)) {
                switch (CLUSettings.Instance.UseAoEAbilities) {
                case true:
                    CLUSettings.Instance.UseAoEAbilities = !CLUSettings.Instance.UseAoEAbilities;
                    CLU.Log(" UseAoEAbilities= {0}", CLUSettings.Instance.UseAoEAbilities);
                    if (CLUSettings.Instance.EnableKeybindSounds) {
                        try {
                            SoundManager.LoadSoundFilePath(@"\CustomClasses\CLU\Sound\aoeabilitiesdisabled.wav");
                            SoundManager.SoundPlay();
                        } catch { }
                    }
                    break;
                case false:
                    CLUSettings.Instance.UseAoEAbilities = !CLUSettings.Instance.UseAoEAbilities;
                    CLU.Log(" UseAoEAbilities= {0}", CLUSettings.Instance.UseAoEAbilities);
                    if (CLUSettings.Instance.EnableKeybindSounds) {
                        try {
                            SoundManager.LoadSoundFilePath(@"\CustomClasses\CLU\Sound\aoeabilitiesenabled.wav");
                            SoundManager.SoundPlay();
                        } catch { }
                    }
                    break;
                default:
                    return;
                }
            }

            // KeybindEnableRaidPartyBuffing \\
            if (IsKeyDown(CLUSettings.Instance.KeybindEnableRaidPartyBuffing)) {
                switch (CLUSettings.Instance.EnableRaidPartyBuffing) {
                case true:
                    CLUSettings.Instance.EnableRaidPartyBuffing = !CLUSettings.Instance.EnableRaidPartyBuffing;
                    CLU.Log(" EnableRaidPartyBuffing= {0}", CLUSettings.Instance.EnableRaidPartyBuffing);
                    if (CLUSettings.Instance.EnableKeybindSounds) {
                        try {
                            SoundManager.LoadSoundFilePath(@"\CustomClasses\CLU\Sound\raidandpartybuffingdisabled.wav");
                            SoundManager.SoundPlay();
                        } catch { }
                    }
                    break;
                case false:
                    CLUSettings.Instance.EnableRaidPartyBuffing = !CLUSettings.Instance.EnableRaidPartyBuffing;
                    CLU.Log(" EnableRaidPartyBuffing= {0}", CLUSettings.Instance.EnableRaidPartyBuffing);
                    if (CLUSettings.Instance.EnableKeybindSounds) {
                        try {
                            SoundManager.LoadSoundFilePath(@"\CustomClasses\CLU\Sound\raidandpartybuffingenabled.wav");
                            SoundManager.SoundPlay();
                        } catch { }
                    }
                    break;
                default:
                    return;
                }
            }

            // KeybindEnableInterupts \\
            if (IsKeyDown(CLUSettings.Instance.KeybindEnableInterupts)) {
                switch (CLUSettings.Instance.EnableInterupts) {
                case true:
                    CLUSettings.Instance.EnableInterupts = !CLUSettings.Instance.EnableInterupts;
                    CLU.Log(" EnableInterupts= {0}", CLUSettings.Instance.EnableInterupts);
                    if (CLUSettings.Instance.EnableKeybindSounds) {
                        try {
                            SoundManager.LoadSoundFilePath(@"\CustomClasses\CLU\Sound\interuptsdisabled.wav");
                            SoundManager.SoundPlay();
                        } catch { }
                    }
                    break;
                case false:
                    CLUSettings.Instance.EnableInterupts = !CLUSettings.Instance.EnableInterupts;
                    CLU.Log(" EnableInterupts= {0}", CLUSettings.Instance.EnableInterupts);
                    if (CLUSettings.Instance.EnableKeybindSounds) {
                        try {
                            SoundManager.LoadSoundFilePath(@"\CustomClasses\CLU\Sound\interuptsenabled.wav");
                            SoundManager.SoundPlay();
                        } catch { }
                    }
                    break;
                default:
                    return;
                }
            }

            // KeybindHealDefensiveManagement \\
            if (IsKeyDown(CLUSettings.Instance.KeybindHealEnableSelfHealing)) {
                switch (CLUSettings.Instance.EnableSelfHealing) {
                case true:
                    CLUSettings.Instance.EnableSelfHealing = !CLUSettings.Instance.EnableSelfHealing;
                    CLU.Log(" EnableSelfHealing= {0}", CLUSettings.Instance.EnableSelfHealing);
                    if (CLUSettings.Instance.EnableKeybindSounds) {
                        try {
                            SoundManager.LoadSoundFilePath(@"\CustomClasses\CLU\Sound\selfhealingdisabled.wav");
                            SoundManager.SoundPlay();
                        } catch { }
                    }
                    break;
                case false:
                    CLUSettings.Instance.EnableSelfHealing = !CLUSettings.Instance.EnableSelfHealing;
                    CLU.Log(" EnableSelfHealing= {0}", CLUSettings.Instance.EnableSelfHealing);
                    if (CLUSettings.Instance.EnableKeybindSounds) {
                        try {
                            SoundManager.LoadSoundFilePath(@"\CustomClasses\CLU\Sound\selfhealingenabled.wav");
                            SoundManager.SoundPlay();
                        } catch { }
                    }
                    break;
                default:
                    return;
                }
            }

            // KeybindEnableMultiDotting \\
            if (IsKeyDown(CLUSettings.Instance.KeybindEnableMultiDotting)) {
                switch (CLUSettings.Instance.EnableMultiDotting) {
                case true:
                    CLUSettings.Instance.EnableMultiDotting = !CLUSettings.Instance.EnableMultiDotting;
                    CLU.Log(" EnableMultiDotting= {0}", CLUSettings.Instance.EnableMultiDotting);
                    if (CLUSettings.Instance.EnableKeybindSounds) {
                        try {
                            SoundManager.LoadSoundFilePath(@"\CustomClasses\CLU\Sound\multidottingdisabled.wav");
                            SoundManager.SoundPlay();
                        } catch { }
                    }
                    break;
                case false:
                    CLUSettings.Instance.EnableMultiDotting = !CLUSettings.Instance.EnableMultiDotting;
                    CLU.Log(" EnableMultiDotting= {0}", CLUSettings.Instance.EnableMultiDotting);
                    if (CLUSettings.Instance.EnableKeybindSounds) {
                        try {
                            SoundManager.LoadSoundFilePath(@"\CustomClasses\CLU\Sound\multidottingenabled.wav");
                            SoundManager.SoundPlay();
                        } catch { }
                    }
                    break;
                default:
                    return;
                }
            }

            // KeybindPauseRotation \\
            if (IsKeyDown(CLUSettings.Instance.KeybindPauseRotation)) {
                switch (CLUSettings.Instance.PauseRotation) {
                case true:
                    CLUSettings.Instance.PauseRotation = !CLUSettings.Instance.PauseRotation;
                    CLU.Log(" PauseRotation= {0}", CLUSettings.Instance.PauseRotation);
                    if (CLUSettings.Instance.EnableKeybindSounds) {
                        try {
                            SoundManager.LoadSoundFilePath(@"\CustomClasses\CLU\Sound\rotationenabled.wav");
                            SoundManager.SoundPlay();
                        } catch {}

                    }
                    break;
                case false:
                    CLUSettings.Instance.PauseRotation = !CLUSettings.Instance.PauseRotation;
                    CLU.Log(" PauseRotation= {0}", CLUSettings.Instance.PauseRotation);
                    if (CLUSettings.Instance.EnableKeybindSounds) {
                        try {
                            SoundManager.LoadSoundFilePath(@"\CustomClasses\CLU\Sound\rotationpaused.wav");
                            SoundManager.SoundPlay();
                        } catch { }
                    }
                    break;
                default:
                    return;
                }
            }

            // KeybindChakraStanceSelection \\
            if (IsKeyDown(CLUSettings.Instance.KeybindChakraStanceSelection)) {
                switch (CLUSettings.Instance.Priest.ChakraStanceSelection) {
                case ChakraStance.Sanctuary:
                    CLUSettings.Instance.Priest.ChakraStanceSelection = ChakraStance.Serenity;
                    CLU.Log(" ChakraStanceSelection= {0}", CLUSettings.Instance.Priest.ChakraStanceSelection);
                    if (CLUSettings.Instance.EnableKeybindSounds) {
                        try {
                            SoundManager.LoadSoundFilePath(@"\CustomClasses\CLU\Sound\chakraserenity.wav");
                            SoundManager.SoundPlay();
                        } catch {}
                    }
                    break;
                case ChakraStance.Serenity:
                    CLUSettings.Instance.Priest.ChakraStanceSelection = ChakraStance.Sanctuary;
                    CLU.Log(" ChakraStanceSelection= {0}", CLUSettings.Instance.Priest.ChakraStanceSelection);
                    if (CLUSettings.Instance.EnableKeybindSounds) {
                        try {
                            SoundManager.LoadSoundFilePath(@"\CustomClasses\CLU\Sound\chakrasanctuary.wav");
                            SoundManager.SoundPlay();
                        } catch { }
                    }

                    break;
                default:
                    return;
                }
            }
        }

        /// <summary>
        /// checks to see if the specified key has been pressed within wow.
        /// </summary>
        /// <param name="key">The key to check for (see Keyboardfunctions)</param>
        /// <returns>true if the player has pressed the key</returns>
        private static bool IsKeyDown(Keyboardfunctions key)
        {
            try {
                if (key == Keyboardfunctions.Nothing) return false;
                var raw = Lua.GetReturnValues("if " + key.ToString("g") + "() then return 1 else return 0 end");
                return raw[0] == "1";
            } catch {
                CLU.DiagnosticLog("Lua failed in IsKeyDown" + key.ToString("g"));
                return false;
            }
        }
    }
}