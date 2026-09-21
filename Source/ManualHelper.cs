using System;
using System.Reflection;
using Celeste.Mod.Core;
using Microsoft.Xna.Framework;
using Monocle;
using MonoMod.RuntimeDetour;
using ExtendedVariants;
using Celeste.Mod.CommunalHelper;
using Celeste.Mod.CommunalHelper.DashStates;
using Celeste.Mod.UI;
using ExtendedVariants.UI;
using FMOD.Studio;
using MonoMod.ModInterop;

namespace Celeste.Mod.ManualHelper;

public class ManualHelper : EverestModule {
    public static string[] ManualHelperTogglesGrabs = ["LeftClinging","LeftUncrouchedClimbjumping","LeftCrouchedClimbjumping","LeftWalljumps","LeftWallbounces","RightClinging","RightUncrouchedClimbjumping","RightCrouchedClimbjumping","RightWalljumps","RightWallbounces"];
    public static string[] ManualHelperTogglesDashes = ["UsableDashAttack"];
    public static string[] ManualHelperTogglesVanillaEntities = ["HeartDoors","CrumbleBlocks"];
    
    public static ManualHelper Instance { get; private set; }
    public override Type SettingsType => typeof(ManualHelperModuleSettings);
    public static ManualHelperModuleSettings Settings => (ManualHelperModuleSettings) Instance._Settings;
    public override Type SessionType => typeof(ManualHelperModuleSession);
    public static ManualHelperModuleSession Session => (ManualHelperModuleSession) Instance._Session;
    public override Type SaveDataType => typeof(ManualHelperModuleSaveData);
    public static ManualHelperModuleSaveData SaveData => (ManualHelperModuleSaveData) Instance._SaveData;

    public ManualHelper() {
        Instance = this;
//#if DEBUG
        // debug builds use verbose logging
        Logger.SetLogLevel(nameof(ManualHelper), LogLevel.Verbose);
//#else
        // release builds use info logging to reduce spam in log files
        //Logger.SetLogLevel(nameof(ManualHelperModule), LogLevel.Info);
//#endif
    }

    private static Hook dashAttackingHook;
    private static Hook heartDoorHook;
    private static bool isRenderingCode = false;
    public static bool communalHelperLoaded;
    public EverestModuleMetadata communalHelper;
    public override void Load() {
        // apply any hooks that should always be active
        dashAttackingHook = new Hook(
            typeof(Player).GetMethod("get_DashAttacking"),
            typeof(ManualHelper).GetMethod("weirdHookCelestePlayerGetDashAttacking", BindingFlags.NonPublic | BindingFlags.Static)
        );
        heartDoorHook = new Hook(
            typeof(HeartGemDoor).GetMethod("get_Counter"),
            typeof(ManualHelper).GetMethod("weirdHookCelesteHeartGemDoorSetCounter", BindingFlags.NonPublic | BindingFlags.Static)
        );
        On.Celeste.Player.ClimbCheck += OnCelestePlayerClimbCheck;
        On.Celeste.Player.ClimbJump += OnCelestePlayerClimbJump;
        On.Celeste.Player.WallJump += OnCelestePlayerWallJump;
        On.Celeste.Player.SuperWallJump += OnCelestePlayerSuperWallJump;
        On.Celeste.Player.NormalUpdate += OnCelestePlayerNormalUpdate;
        On.Celeste.Player.UpdateSprite += OnCelestePlayerUpdateSprite;
        On.Celeste.HeartGemDoor.Added += OnCelesteHeartGemDoorAdded;
        Everest.Events.Level.OnCreatePauseMenuButtons += EverestEventsLevelOnCreatePauseMenuButtons;
        On.Celeste.Solid.GetPlayerOnTop += OnCelesteSolidGetPlayerOnTop;
        On.Celeste.Solid.GetPlayerClimbing += OnCelesteSolidGetPlayerClimbing;
        
        communalHelper = new() {
            Name = "CommunalHelper",
            Version = new Version(1, 2 ,0)
        };

        communalHelperLoaded = Everest.Loader.DependencyLoaded(communalHelper);
        if (communalHelperLoaded)
        {
            typeof(CommunalHelperImports).ModInterop();
        }
    }

    public override void Unload() {
        // unapply any hooks applied in Load()
        dashAttackingHook.Dispose();
        dashAttackingHook = null;
        heartDoorHook.Dispose();
        heartDoorHook = null;
        On.Celeste.Player.ClimbCheck -= OnCelestePlayerClimbCheck;
        On.Celeste.Player.ClimbJump -= OnCelestePlayerClimbJump;
        On.Celeste.Player.WallJump -= OnCelestePlayerWallJump;
        On.Celeste.Player.SuperWallJump -= OnCelestePlayerSuperWallJump;
        On.Celeste.Player.NormalUpdate -= OnCelestePlayerNormalUpdate;
        On.Celeste.Player.UpdateSprite -= OnCelestePlayerUpdateSprite;
        On.Celeste.HeartGemDoor.Added -= OnCelesteHeartGemDoorAdded;
        Everest.Events.Level.OnCreatePauseMenuButtons -= EverestEventsLevelOnCreatePauseMenuButtons;
        On.Celeste.Solid.GetPlayerOnTop -= OnCelesteSolidGetPlayerOnTop;
        On.Celeste.Solid.GetPlayerClimbing -= OnCelesteSolidGetPlayerClimbing;
    }
    
    [ModImportName("CommunalHelper.DashStates")]
    public static class CommunalHelperImports
    {
        public static Func<bool> HasDreamTunnelDash;
    }

    public static void MakeModMenu(Level level, string text)
    {
        TextMenu modMenu = OuiModOptions.CreateMenu(true, (EventInstance) null!);
        

        Action closeMenu = (() =>
        {
            Audio.Play("event:/ui/main/button_back");
            modMenu.Close();
            level.Paused = false;
        });
        modMenu.OnCancel = closeMenu;
        modMenu.OnESC = closeMenu;
        modMenu.OnPause = closeMenu;

        level.Paused = true;
        modMenu.Selection = modMenu.FirstPossibleSelection;
        for (int a = 0; a < modMenu.Items.Count; a++)
        {
            if (modMenu.Items[a].SearchLabel() == text)
            {
                modMenu.Selection = a;
                break;
            }
        }
        level.Add(modMenu);
        Audio.Play("event:/ui/main/button_select");
        return;
    }

    public override void CreateModMenuSection(TextMenu menu, bool inGame, EventInstance pauseSnapshot)
    {
        CreateModMenuSectionHeader(menu, inGame, pauseSnapshot);
        // subheaders at top
        menu.Add(new TextMenuExt.SubHeaderExt(Dialog.Clean("MODOPTIONS_MANUALHELPER_ExplainLine1")) { TextColor = Color.Goldenrod, HeightExtra = 0 });
        menu.Add(new TextMenuExt.SubHeaderExt(Dialog.Clean("MODOPTIONS_MANUALHELPER_ExplainLine2")) { TextColor = Color.DeepSkyBlue, HeightExtra = 0 });
        menu.Add(new TextMenuExt.SubHeaderExt(Dialog.Clean("MODOPTIONS_MANUALHELPER_ExplainLine3")) { TextColor = Color.Pink, HeightExtra = 0 });
        
        // major submenus
        TextMenuExt.SubMenu myMenu1 = new TextMenuExt.SubMenu(Dialog.Clean("MODOPTIONS_MANUALHELPER_WallTogglesHeader"), false);
        Settings.SettingsMenu1.CreateDummy1Entry(myMenu1,inGame);
        menu.Add(myMenu1);
        TextMenuExt.SubMenu myMenu2 = new TextMenuExt.SubMenu(Dialog.Clean("MODOPTIONS_MANUALHELPER_DashTogglesHeader"), false);
        Settings.SettingsMenu1.CreateDummy1Entry(myMenu2,inGame);
        menu.Add(myMenu2);
        TextMenuExt.SubMenu myMenu3 = new TextMenuExt.SubMenu(Dialog.Clean("MODOPTIONS_MANUALHELPER_VanillaEntityTogglesHeader"), false);
        Settings.SettingsMenu1.CreateDummy1Entry(myMenu3,inGame);
        menu.Add(myMenu3);
        // misc
        menu.Add(new TextMenu.OnOff(Dialog.Clean("MODOPTIONS_MANUALHELPER_PauseMenuButtonEnabled"), Settings.PauseMenuButtonEnabled)
            .Change(v => Settings.PauseMenuButtonEnabled = v));
        // the keybind uh. somehow is still there. idk why i dont need to add it here lmao
        
        CreateModMenuSectionKeyBindings(menu, inGame, pauseSnapshot);
    }

    // commands
    [Command("mh_get_toggle_data", "[from ManualHelper] gets the value of a given manualhelper toggle")]
    public static void CmdGetToggleData(string input)
    {
        int result = GetToggleData(input);
        Engine.Commands.Log("Input "+input+" returned "+result+".");
        switch (result)
        {
            case -1:
            {
                Engine.Commands.Log("This means either an invalid toggle or an error.");
                break;
            }
            case 0:
            {
                Engine.Commands.Log("This means it is set to MapDefault.");
                break;
            }
            case 1:
            {
                Engine.Commands.Log("This means it is set to Off.");
                break;
            }
            case 2:
            {
                Engine.Commands.Log("This means it is set to On.");
                break;
            }
        }
    }
    
    // tools
    public static int GetToggleData(string whichOne)
    {
        // returns 0 if Map Default, 1 if Off, and 2 if On

        if (Settings.DynamicSettings.ContainsKey(whichOne))
        {
            return (Settings.DynamicSettings[whichOne])[0];
        }

        /*if (whichOne == "UsableDashAttack")
        {
            return Settings.UsableDashAttackSlider == ManualHelperModuleSettings.UsableDashAttack.MapDefault ? 0 :
                (Settings.UsableDashAttackSlider == ManualHelperModuleSettings.UsableDashAttack.Off ? 1 : 2);
        }


        if (whichOne == "LeftClinging")
        {
            return (Settings.DynamicSettings["LeftClinging"]) == ManualHelperModuleSettings.LeftClinging.MapDefault ? 0 :
                (Settings.LeftClingingSlider == ManualHelperModuleSettings.LeftClinging.Off ? 1 : 2);
        }
        if (whichOne == "LeftUncrouchedClimbjumping")
        {
            return Settings.LeftUncrouchedClimbjumpingSlider == ManualHelperModuleSettings.LeftUncrouchedClimbjumping.MapDefault ? 0 :
                (Settings.LeftUncrouchedClimbjumpingSlider == ManualHelperModuleSettings.LeftUncrouchedClimbjumping.Off ? 1 : 2);
        }
        if (whichOne == "LeftCrouchedClimbjumping")
        {
            return Settings.LeftCrouchedClimbjumpingSlider == ManualHelperModuleSettings.LeftCrouchedClimbjumping.MapDefault ? 0 :
                (Settings.LeftCrouchedClimbjumpingSlider == ManualHelperModuleSettings.LeftCrouchedClimbjumping.Off ? 1 : 2);
        }
        if (whichOne == "LeftWalljumps")
        {
            return Settings.LeftWalljumpsSlider == ManualHelperModuleSettings.LeftWalljumps.MapDefault ? 0 :
                (Settings.LeftWalljumpsSlider == ManualHelperModuleSettings.LeftWalljumps.Off ? 1 : 2);
        }
        if (whichOne == "LeftWallbounces")
        {
            return Settings.LeftWallbouncesSlider == ManualHelperModuleSettings.LeftWallbounces.MapDefault ? 0 :
                (Settings.LeftWallbouncesSlider == ManualHelperModuleSettings.LeftWallbounces.Off ? 1 : 2);
        }

        if (whichOne == "RightClinging")
        {
            return Settings.RightClingingSlider == ManualHelperModuleSettings.RightClinging.MapDefault ? 0 :
                (Settings.RightClingingSlider == ManualHelperModuleSettings.RightClinging.Off ? 1 : 2);
        }
        if (whichOne == "RightUncrouchedClimbjumping")
        {
            return Settings.RightUncrouchedClimbjumpingSlider == ManualHelperModuleSettings.RightUncrouchedClimbjumping.MapDefault ? 0 :
                (Settings.RightUncrouchedClimbjumpingSlider == ManualHelperModuleSettings.RightUncrouchedClimbjumping.Off ? 1 : 2);
        }
        if (whichOne == "RightCrouchedClimbjumping")
        {
            return Settings.RightCrouchedClimbjumpingSlider == ManualHelperModuleSettings.RightCrouchedClimbjumping.MapDefault ? 0 :
                (Settings.RightCrouchedClimbjumpingSlider == ManualHelperModuleSettings.RightCrouchedClimbjumping.Off ? 1 : 2);
        }
        if (whichOne == "RightWalljumps")
        {
            return Settings.RightWalljumpsSlider == ManualHelperModuleSettings.RightWalljumps.MapDefault ? 0 :
                (Settings.RightWalljumpsSlider == ManualHelperModuleSettings.RightWalljumps.Off ? 1 : 2);
        }
        if (whichOne == "RightWallbounces")
        {
            return Settings.RightWallbouncesSlider == ManualHelperModuleSettings.RightWallbounces.MapDefault ? 0 :
                (Settings.RightWallbouncesSlider == ManualHelperModuleSettings.RightWallbounces.Off ? 1 : 2);
        }


        if (whichOne == "HeartDoors")
        {
            return Settings.HeartDoorsSlider == ManualHelperModuleSettings.HeartDoors.MapDefault ? 0 :
                (Settings.HeartDoorsSlider == ManualHelperModuleSettings.HeartDoors.Off ? 1 : 2);
        }*/


        // but like, return -1 if the option is not found
        Logger.Log(nameof(ManualHelper),"[Error NonIdiot000] Hey twin, option "+whichOne+" isn't a valid ManualHelper Option. Seems like a skill issue.");
        return -1;
    }

    public static bool CanLRInteract(Player self, bool dir, string name)
    {
        return dir ? ReturnFromBoolToggle("Left"+name) : ReturnFromBoolToggle("Right"+name);
    }

    public static bool ReturnFromBoolToggle(string whichOne)
    {
        int myOption = GetToggleData(whichOne);
        // if the given option is not found, return false
        if (myOption == -1)
        {
            Logger.Log(nameof(ManualHelper),"[Error NonIdiot001] Yo nerd, whatever "+whichOne+" is, it ain't a ManualHelper Bool Toggle.");
            return false;
        }
        // if the given option is Map Default, look at map's flags instead of config
        if (myOption == 0)
        {
            // TODO: insert flags stuff here later once that's implemented
            
            return true;
        }
        // otherwise return bool of if the given option is On
        return myOption == 2;
    }
    
    public static int GetFlag(string flagName)
    {
        //thank u snip/tart1998 for the help
        if (Engine.Scene is not Level level)
            // handle case when you're not in a level
            return -1;

        return level.Session.GetFlag(flagName) ? 1 : 0;
    }

    public static void SetFlag(string flagName, bool setTo)
    {
        if (Engine.Scene is not Level level)
            // handle case when you're not in a level
            return;

        level.Session.SetFlag(flagName,setTo);
    }

    // hooks

    // for preventing climbing stuff
    private static bool OnCelestePlayerClimbCheck(On.Celeste.Player.orig_ClimbCheck orig, Player self, int dir, int yAdd)
    {
        if (((!ReturnFromBoolToggle("LeftClinging") && dir == -1) || (!ReturnFromBoolToggle("RightClinging") && dir == 1)) && !self.level.InCredits)
        {
            return false;
        }

        return orig(self, dir, yAdd);
    }

    // for preventing climbjump stuff
    private static void OnCelestePlayerClimbJump(On.Celeste.Player.orig_ClimbJump orig, Player self)
    {
        if ((self.Facing == Facings.Left ?
                (self.Ducking ? ReturnFromBoolToggle("LeftCrouchedClimbjumping") :
                    ReturnFromBoolToggle("LeftUncrouchedClimbjumping")) :
                (self.Ducking ? ReturnFromBoolToggle("RightCrouchedClimbjumping") :
                    ReturnFromBoolToggle("RightUncrouchedClimbjumping")))
            || self.level.InCredits)
        {
            orig(self);
        }
        else //if (CanWallInteract(self, self.Facing == Facings.Right))
        //{
            if (self.DashAttacking && self.SuperWallJumpAngleCheck)
            {
                self.SuperWallJump(-(int)self.Facing);
            }
            else
            {
                self.WallJump(-(int)self.Facing);
            }
        //}
    }

    // for preventing walljump/neutraljump stuff
    private static void OnCelestePlayerWallJump(On.Celeste.Player.orig_WallJump orig, Player self, int dir)
    {
        if (CanLRInteract(self,dir == 1,"Walljumps"))
        {
            orig(self, dir);
        }
        else if (self.StateMachine.State == 1)
        {
            self.moveX = -dir;
            orig(self, dir);
        }
    }

    // for preventing wallbounce stuff
    private static void OnCelestePlayerSuperWallJump(On.Celeste.Player.orig_SuperWallJump orig, Player self, int dir)
    {
        if (CanLRInteract(self,dir == 1,"Wallbounces"))
        {
            orig(self, dir);
        }
        else
        {
            //self.Ducking = false;
            Input.Jump.ConsumePress();
            Input.Jump.ConsumeBuffer();
            self.jumpGraceTimer = 0f;
            //self.varJumpTimer = 0.25f;
            self.varJumpTimer = 0f;
            self.AutoJump = false;
            self.dashAttackTimer = 0f;
            self.wallBoostTimer = 0f;
            self.varJumpSpeed = self.Speed.Y;
            // only do below line if you cant figure out how to prevent it from acting like a regular freakin updash
            //self.Speed.Y = 0;
            self.Speed.Y /= 2;
        }
    }

    // for preventing sliding on walls that aren't enabled
    private static int OnCelestePlayerNormalUpdate(On.Celeste.Player.orig_NormalUpdate orig, Player self)
    {
        if (!CanLRInteract(self, (int)self.Facing == -1,"Walljumps") && (self as Monocle.Entity).CollideCheck<Solid>(self.Position + Vector2.UnitX * (float)self.Facing))
        {
            self.wallSlideTimer = 0;
        }
        return orig(self);
    }

    // for preventing dash attack state. thx to maddie480's Extended Variants for the code!
    // ReSharper disable once UnusedMember.Local
    private static bool weirdHookCelestePlayerGetDashAttacking(Func<Player, bool> orig, Player self) {
        if (!isRenderingCode && !ReturnFromBoolToggle("UsableDashAttack")) {
            return false;
        }

        return orig(self);
    }

    // also used similar code to above for actually making the player render correctly. it WAS funny without but eh
    private static void OnCelestePlayerUpdateSprite(On.Celeste.Player.orig_UpdateSprite orig, Player self)
    {
        isRenderingCode = true;
        orig(self);
        isRenderingCode = false;
    }

    // Prevents heart door from being instantialized as already opened (I think, probably doesn't work for modded Gem Doors)
    private static void OnCelesteHeartGemDoorAdded(On.Celeste.HeartGemDoor.orig_Added orig, HeartGemDoor self, Scene scene)
    {
        (scene as Level).Session.SetFlag("opened_heartgem_door_" + self.Requires, false);
        orig(self, scene);
    }

    // Prevents heart door from opening
    // ReSharper disable once UnusedMember.Local
    private static float weirdHookCelesteHeartGemDoorSetCounter(Func<HeartGemDoor, float> orig, HeartGemDoor self) {
        if (!ReturnFromBoolToggle("HeartDoors")) {
            return 0f;
        }

        return orig(self);
    }

    private void EverestEventsLevelOnCreatePauseMenuButtons(Level level, TextMenu menu, bool minimal)
    {
        if (CoreModule.Settings == null) return;
        if (!Settings.PauseMenuButtonEnabled) return;

        int optionsIndex = menu.Items.FindIndex(item =>
            item.GetType() == typeof(TextMenu.Button) && ((TextMenu.Button) item).Label == Dialog.Clean("menu_pause_resume"));

        menu.Insert(optionsIndex+2, new TextMenu.Button(Dialog.Clean("MODOPTIONS_MANUALHELPER_AwesomeButton")) {
            OnPressed = () => {
                if (communalHelperLoaded)
                {
                    Logger.Log(nameof(ManualHelper),CommunalHelperImports.HasDreamTunnelDash().ToString());
                }
                else
                {
                    Logger.Log(nameof(ManualHelper), "There is no Communal Helper in Ba Sing Se.");
                }
                if (false)
                {
                    //Logger.Log(nameof(ManualHelper),CommunalHelperImports.HasDreamTunnelDash().ToString());
                }
                else
                {
                    //Logger.Log(nameof(ManualHelper), "Whoops! You have to put the <modname> in your computer!");
                }

                menu.OnCancel();
                MakeModMenu(level,Dialog.Clean("MODOPTIONS_MANUALHELPER_WallTogglesHeader"));//Manual Helper//modoptions_ManualHelper_title
                //hintController.ShowHint();
            },
            //Disabled = hintController.SingleUse && hintController.UsedFlagValue,
        });
    }

    private static Player OnCelesteSolidGetPlayerOnTop(On.Celeste.Solid.orig_GetPlayerOnTop orig, Solid self)
    {
        if (self is CrumblePlatform)
        {
            if (!ReturnFromBoolToggle("CrumbleBlocks"))
            {
                if (self.Collidable)
                {
                    if (orig(self) != null)
                    {
                        self.Collidable = false;
                        Player realSelf = orig(self);
                        realSelf.Dashes = 0;
                        return realSelf;
                    }
                }
                else
                {
                    return null;
                }
            }
        }

        return orig(self);
    }

    private static Player OnCelesteSolidGetPlayerClimbing(On.Celeste.Solid.orig_GetPlayerClimbing orig, Solid self)
    {
        if (self is CrumblePlatform)
        {
            if (!ReturnFromBoolToggle("CrumbleBlocks"))
            {
                if (self.Collidable)
                {
                    if (orig(self) != null)
                    {
                        self.Collidable = false;
                        Player realSelf = orig(self);
                        realSelf.Dashes = 0;
                        return realSelf;
                    }
                }
                else
                {
                    return null;
                }
            }
        }

        return orig(self);
    }
}