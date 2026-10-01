using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Celeste.Mod.Core;
using Microsoft.Xna.Framework;
using Monocle;
using MonoMod.RuntimeDetour;
using ExtendedVariants;
using Celeste.Mod.CommunalHelper;
using Celeste.Mod.CommunalHelper.DashStates;
using Celeste.Mod.CommunalHelper.States;
using Celeste.Mod.UI;
using ExtendedVariants.UI;
using FMOD;
using FMOD.Studio;
using MonoMod.ModInterop;
// ReSharper disable StringIndexOfIsCultureSpecific.1
// ReSharper disable StringLastIndexOfIsCultureSpecific.1

namespace Celeste.Mod.ManualHelper;

public class ManualHelper : EverestModule {
    
    // do NOT let mods touch this.
    //public static Dictionary<string,string[]> typesOfToggles = new Dictionary<string, string[]>();
    // mods can touch this. string is the header, and the string[x] is the order in it
    public static Dictionary<string,string[]> ManualHelperToggles = new Dictionary<string, string[]>();
    public static void resetManualHelperToggleGroups()
    {
        ManualHelperToggles = new Dictionary<string, string[]>();
        
        // the key has two sections. the type, and the name of the group.
        // the type is Bools, HarshLenientOn, or .
        // the name of the group should be /ManualHelper/<nameOfGroup>, or /<YourModID>/<nameOfGroup> if adding via Interop.
        // note that if the type is different but the name is the same, they are still grouped together in the same subsection.
        ManualHelperToggles.Add("Bools/ManualHelper/WallToggles",[
            "LeftClinging","LeftUncrouchedClimbjumping","LeftCrouchedClimbjumping","LeftWallInteractions","LeftWallbounces",
            "RightClinging","RightUncrouchedClimbjumping","RightCrouchedClimbjumping","RightWallInteractions","RightWallbounces"]);
        //ManualHelperToggles.Add("IntsMultiplier/ManualHelper/WallToggles",[
        //    "LeftWalljumpStaminaCost","RightWalljumpStaminaCost"]);
        ManualHelperToggles.Add("Bools/ManualHelper/DashToggles",[
            "LeftDashlessClinging","LeftDashlessClimbjumping","LeftClimbjumpingDoesntCostDashes","LeftDashlessWalljumps","LeftWalljumpsDontCostDashes",
            "RightDashlessClinging","RightDashlessClimbjumping","RightClimbjumpingDoesntCostDashes","RightDashlessWalljumps","RightWalljumpsDontCostDashes",
            "UsableDashAttack"]);
        ManualHelperToggles.Add("Bools/ManualHelper/StaminaToggles",[
            "MadelineHasAllergyMedication"]);
        ManualHelperToggles.Add("Bools/ManualHelper/VanillaEntityToggles",[
            "HeartDoors","NonWoodenDoors","WoodenDoors"]);
        ManualHelperToggles.Add("HarshLenientOn/ManualHelper/VanillaEntityToggles",[
            "CrumbleBlocks","Snowballs","OshiroBosses"]);
        // if ANY mod with entity toggles is enabled
        if (communalHelperLoaded || false)
        {
            ManualHelperToggles.Add("Bools/ManualHelper/ModdedEntityToggles",[
                "FakeMod/Awewa"]);//,
        }

        if (communalHelperLoaded)
        {
            //ManualHelperToggles["Bools/ManualHelper/ModdedEntityToggles"]=ManualHelperToggles["Bools/ManualHelper/ModdedEntityToggles"].Append("ForceDisableElytra").ToArray();
            appendify("Bools/ManualHelper/ModdedEntityToggles","AllowElytra");
        }
        //ManualHelperToggles.Add("Bools/FakeMod/CoolThing",[
        //    "Wawa"]);
        //ManualHelperToggles.Add("HarshLenientOn/FakeMod/CoolThing",[
        //    "Awawa"]);
        
        
        //typesOfToggles = new Dictionary<string, string[]>();
        //typesOfToggles.Add("Bools",["Bool"]);
    }
    public static void appendify(string input, string putIn)
    {
        ManualHelperToggles[input] = ManualHelperToggles[input].Append(putIn).ToArray();
    }

    // ManualHelperNonBoolToggleToInt is used for what the "default" and "disabled" options should be, while
    // ManualHelperNonBoolToggleToIntAllValues is used for what values are present (which is ignored for HarshLenientOn).
    // both of the below are formatted with the key being ManualHelperToggles.Keys[#]+"/"+ManualHelperToggles[ManualHelperToggles.Keys[#]]. so just key+"/"+ManualHelperToggles[key].
    
    // ManualHelperNonBoolToggleToInt is formatted with the int[] being [<default value>, <value that should be "disabled">].
    // to note, 0 isnt available for these, as that is what the "error code" and/or MapDefault is. See level.Session.GetCounter for why its an "error code".
    public static Dictionary<string, int[]> ManualHelperNonBoolToggleToInt = new Dictionary<string, int[]>();
    // ManualHelperNonBoolToggleToIntAllValues is formatted with the int[] being value/10.
    public static Dictionary<string, int[]> ManualHelperNonBoolToggleToIntAllValues = new Dictionary<string, int[]>();
    public static void resetNonBoolToggleToInt()
    {
        ManualHelperNonBoolToggleToInt = new Dictionary<string, int[]>();
        ManualHelperNonBoolToggleToIntAllValues = new Dictionary<string, int[]>();
        // the HarshLenientOn should always be [3,1], as Option 1 (Harsh) is the "disabled" option, and Option 3 (On) is the "on" option.
        //foreach (string harshLenientOnString in ManualHelperTogglesHarshLenientOnVanillaEntities)
        foreach (string key in ManualHelperToggles.Keys)
        {
            int whichOptionWas = -1;
            if (key.StartsWith("HarshLenientOn/"))
            {
                whichOptionWas = 0;
            }
            if (key.StartsWith("IntsCustom/"))
            {
                whichOptionWas = 1;
            }
            if (key.StartsWith("IntsMultiplier/"))
            {
                whichOptionWas = 2;
            }

            if (whichOptionWas != -1) {
                if (!ManualHelperNonBoolToggleToInt.ContainsKey(key+"/"+ManualHelperToggles[key]))
                {
                    string[] Lees = ManualHelperToggles[key];
                    foreach (string lee in Lees)
                    {
                        ManualHelperNonBoolToggleToInt.Add(key+"/"+lee, whichOptionWas == 0 ? [3, 1] : (whichOptionWas == 2 ? [10,1] : [1,1]));
                        ManualHelperNonBoolToggleToIntAllValues.Add(key+"/"+lee, whichOptionWas == 0 ? [1,2,3] : (whichOptionWas == 2 ? [0,1,2,3,4,5,6,7,8,9,10,11,12,13,14,15,16,17,18,19,20,25,30,35,40,45,50,60,70,80,90,100] : []));
                        //Logger.Log(LogLevel.Info,"ManualHelper_resetNonBoolToggleToInt",key+"/"+lee+" "+whichOptionWas+" added huehl");
                    }
                }
            }
        }
        //ManualHelperNonBoolToggleToInt.Add("Snowballs", [3,1]);
        //ManualHelperNonBoolToggleToInt.Add("OshiroBosses", [3,1]);
    }

    // in order: Changed By Player color, Changed By Map color, Changed By Player/Map color, Not Changed By Either color, Disabled color, Error color
    public static Color[] ManualHelperAllMenuColors = [Color.Goldenrod,Color.DeepSkyBlue,Color.DeepPink,Color.White,Color.DarkSlateGray,Color.Red];
    
    public static ManualHelper Instance { get; private set; }
    public override Type SettingsType => typeof(ManualHelperModuleSettings);
    public static ManualHelperModuleSettings Settings => (ManualHelperModuleSettings) Instance._Settings;
    public static bool DynamicSettingsSet = false;
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
    private static Hook communalHelperElytraHook;
    private static bool isRenderingCode = false;
    public static bool communalHelperLoaded;
    public static bool gravityHelperLoaded;
    public EverestModuleMetadata communalHelper;
    public EverestModuleMetadata gravityHelper;
    public override void Load()
    {
        // dependencies and such
        communalHelper = new() {Name = "CommunalHelper",Version = new Version(1, 25 ,8)};
        gravityHelper = new() {Name = "GravityHelper",Version = new Version(1, 2 ,28)};
        communalHelperLoaded = Everest.Loader.DependencyLoaded(communalHelper);
        gravityHelperLoaded = Everest.Loader.DependencyLoaded(gravityHelper);
        if (communalHelperLoaded) {typeof(CommunalHelperImports).ModInterop();Logger.Log("ManualHelper_Load","haii CommunalHelper :3");}
        //if (gravityHelperLoaded) {typeof(GravityHelperImports).ModInterop();}
        
        // reset all da stuff
        resetManualHelperToggleGroups();
        resetNonBoolToggleToInt();
        
        // apply any hooks that should always be active
        dashAttackingHook = new Hook(
            typeof(Player).GetMethod("get_DashAttacking"),
            typeof(ManualHelper).GetMethod("weirdHookCelestePlayerGetDashAttacking", BindingFlags.NonPublic | BindingFlags.Static)
        );
        heartDoorHook = new Hook(
            typeof(HeartGemDoor).GetMethod("get_HeartGems"),
            typeof(ManualHelper).GetMethod("weirdHookCelesteHeartGemDoorSetHeartGems", BindingFlags.NonPublic | BindingFlags.Static)
        );
        if (communalHelperLoaded)
        {
            /*communalHelperElytraHook = new Hook(
                typeof(StateMachine).GetMethod("get_State"),
                typeof(ManualHelper).GetMethod("weirdHookCommunalHelperGetElytraCooldown", BindingFlags.NonPublic | BindingFlags.Static)
            );*/
        }
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
        Everest.Events.Level.OnLoadLevel += EverestEventsLevelLoaderOnLoadLevel;
        On.Celeste.Player.Render += OnCelestePlayerRender;
        On.Celeste.Player.UpdateHair += OnCelestePlayerUpdateHair;
        On.Celeste.Snowball.OnPlayerBounce += OnCelesteSnowballOnPlayerBounce;
        On.Celeste.AngryOshiro.OnPlayerBounce += OnCelesteAngryOshiroOnPlayerBounce;
        On.Celeste.TextMenuExt.SubHeaderExt.Render += OnCelesteTextMenuExtSubHeaderExtRender;
        On.Celeste.Player.Die += OnCelestePlayerDie;
        On.Celeste.HeartGemDoor.DrawEdges += OnCelesteHeartGemDoorDrawEdges;
        On.Celeste.Door.Open += OnCelesteDoorOpen;
        On.Celeste.Door.Update += OnCelesteDoorUpdate;
        On.Celeste.LevelLoader.ctor += OnCelesteLevelLoaderCtor;
    }

    public override void Unload() {
        // unapply any hooks applied in Load()
        dashAttackingHook.Dispose();
        dashAttackingHook = null;
        heartDoorHook.Dispose();
        heartDoorHook = null;
        if (communalHelperElytraHook != null)
        {
            communalHelperElytraHook.Dispose();
            communalHelperElytraHook = null;
        }
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
        Everest.Events.Level.OnLoadLevel -= EverestEventsLevelLoaderOnLoadLevel;
        On.Celeste.Player.Render -= OnCelestePlayerRender;
        On.Celeste.Player.UpdateHair -= OnCelestePlayerUpdateHair;
        On.Celeste.Snowball.OnPlayerBounce -= OnCelesteSnowballOnPlayerBounce;
        On.Celeste.AngryOshiro.OnPlayerBounce -= OnCelesteAngryOshiroOnPlayerBounce;
        On.Celeste.TextMenuExt.SubHeaderExt.Render -= OnCelesteTextMenuExtSubHeaderExtRender;
        On.Celeste.Player.Die -= OnCelestePlayerDie;
        On.Celeste.HeartGemDoor.DrawEdges -= OnCelesteHeartGemDoorDrawEdges;
        On.Celeste.Door.Open -= OnCelesteDoorOpen;
        On.Celeste.Door.Update -= OnCelesteDoorUpdate;
        On.Celeste.LevelLoader.ctor -= OnCelesteLevelLoaderCtor;
    }

    [ModImportName("CommunalHelper.Elytra")]
    public static class CommunalHelperImports
    {
        //public static Func<bool> HasDreamTunnelDash;

        //public static Func<int> GetDreamTunnelDashState;
        
        //public static Func<Player, bool> SetInfiniteElytra;
        //public static Func<Player, bool> SetElytraEnabled;
        
        public delegate void SetElytraEnabledDelegate(
            bool isTrue);

        public static SetElytraEnabledDelegate SetElytraEnabled;
    }

    public static void MakeModMenu(Level level, string text)
    {
        TextMenu modMenu = OuiModOptions.CreateMenu(true, (EventInstance) null!);

        Action startSearching = OuiModOptions.AddSearchBox(modMenu);

        modMenu.OnUpdate = () => {
            if (modMenu.Focused) {
                //if (CoreModule.Settings.MenuSearch.Pressed) {
                if (Input.QuickRestart.Pressed) {//CoreModule.Settings.MenuPageDown.Pressed
                    startSearching?.Invoke();
                }
            }
        };
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

    public void DescriptionFixes(TextMenuExt.SubMenu fixedMenu)
    {
        foreach (TextMenu.Item item in fixedMenu.Items)
        {
            if (item is TextMenuExt.EaseInSubHeaderExt)
            {
                //(item as TextMenuExt.EaseInSubHeaderExt).Offset.X = 20;
            }
        }
    }

    public override void CreateModMenuSection(TextMenu menu, bool inGame, EventInstance pauseSnapshot)
    {
        Settings.InitializeDynamicSettings();
        CreateModMenuSectionHeader(menu, inGame, pauseSnapshot);
        // subheaders at top
        menu.Add(new TextMenuExt.SubHeaderExt(Dialog.Clean("MODOPTIONS_MANUALHELPER_ExplainLine1")) { TextColor = ManualHelper.ManualHelperAllMenuColors[0], HeightExtra = 0 });
        menu.Add(new TextMenuExt.SubHeaderExt(Dialog.Clean("MODOPTIONS_MANUALHELPER_ExplainLine2")) { TextColor = ManualHelper.ManualHelperAllMenuColors[1], HeightExtra = 0 });
        menu.Add(new TextMenuExt.SubHeaderExt(Dialog.Clean("MODOPTIONS_MANUALHELPER_ExplainLine3")) { TextColor = ManualHelper.ManualHelperAllMenuColors[2], HeightExtra = 0 });
        
        // major submenus
        string[] allCategories = ReturnAllCategories();
        foreach (string category in allCategories)
        {
            string theCool = category.StartsWith("ManualHelper") ? category.Substring(category.IndexOf("/")+1) : category;
            TextMenuExt.SubMenu myMenu = new TextMenuExt.SubMenu("MODOPTIONS_MANUALHELPER_"+theCool.Replace("/","__")+"Header", false);
            Settings.SettingsMenu.CreateDummy1Entry(myMenu,inGame,menu);
            menu.Add(myMenu);
        }

        /*TextMenuExt.SubMenu myMenu1 = new TextMenuExt.SubMenu(Dialog.Clean("MODOPTIONS_MANUALHELPER_WallTogglesHeader"), false);
        Settings.SettingsMenu.CreateDummy1Entry(myMenu1,inGame,menu);
        menu.Add(myMenu1);
        TextMenuExt.SubMenu myMenu2 = new TextMenuExt.SubMenu(Dialog.Clean("MODOPTIONS_MANUALHELPER_DashTogglesHeader"), false);
        Settings.SettingsMenu.CreateDummy1Entry(myMenu2,inGame,menu);
        menu.Add(myMenu2);
        TextMenuExt.SubMenu myMenu3 = new TextMenuExt.SubMenu(Dialog.Clean("MODOPTIONS_MANUALHELPER_VanillaEntityTogglesHeader"), false);
        Settings.SettingsMenu.CreateDummy1Entry(myMenu3,inGame,menu);
        menu.Add(myMenu3);
        TextMenuExt.SubMenu myMenu4 = new TextMenuExt.SubMenu(Dialog.Clean("MODOPTIONS_MANUALHELPER_ModdedEntityTogglesHeader"), false);
        Settings.SettingsMenu.CreateDummy1Entry(myMenu4,inGame,menu);
        menu.Add(myMenu4);*/
        
        // misc
        TextMenu.Option<bool> myButton1 = new TextMenu.OnOff(Dialog.Clean("MODOPTIONS_MANUALHELPER_PauseMenuButtonEnabled"), Settings.PauseMenuButtonEnabled)
            .Change(v => Settings.PauseMenuButtonEnabled = v);
        menu.Add(myButton1);
        myButton1.AddDescription(menu, Dialog.Clean("MODOPTIONS_MANUALHELPER_PauseMenuButtonEnabledDesc"));
        TextMenu.Option<bool> myButton2 = new TextMenu.OnOff(Dialog.Clean("MODOPTIONS_MANUALHELPER_PauseOnDeath"), Settings.PauseOnDeath)
            .Change(v => Settings.PauseOnDeath = v);
        menu.Add(myButton2);
        myButton2.AddDescription(menu, Dialog.Clean("MODOPTIONS_MANUALHELPER_PauseOnDeathDesc"));
        // the keybind uh. somehow is still there. idk why i dont need to add it here lmao
        
        CreateModMenuSectionKeyBindings(menu, inGame, pauseSnapshot);
    }

    // commands
    [Command("manuhelp_toggledata", "[from ManualHelper] gets the value of a given manualhelper toggle")]
    public static void CmdGetToggleData(string input)
    {
        if (input == null)
        {
            Engine.Commands.Log("No toggle detected! Please enter the name of a toggle.");
            return;
        }

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

    [Command("counters", "[from ManualHelper] gets the value of every non-zero counter")]
    public static void CmdCounters(string input)
    {
        if (!(Engine.Scene is Level))
        {
            Engine.Commands.Log("Not currently ingame!");
        }
        else if ((Engine.Scene as Level).Session is null)
        {
            Engine.Commands.Log("Session is currently null!");
        }
        else
        {
            bool becomeEvil = input != null;
            Engine.Commands.Log("Active "+(becomeEvil ? "non-zero " : "")+"counters:");
            foreach (Session.Counter myCount in (Engine.Scene as Level).Session.Counters)
            {
                if (!becomeEvil || myCount.Value != 0)
                {
                    Engine.Commands.Log(myCount.Key+": "+myCount.Value);
                }
            }
        }
    }

    // tools
    public static int GetToggleData(string whichOne)
    {
        // returns whatever the number is for that settings entry in DynamicSettings.
        // for bool toggles this is 0 if Map Default, 1 if Off, and 2 if On
        // otherwise, 0 is Map Default and 1+ is whatever the value is.

        Settings.InitializeDynamicSettings();
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
        if (whichOne == "LeftWallInteractions")
        {
            return Settings.LeftWallInteractionsSlider == ManualHelperModuleSettings.LeftWallInteractions.MapDefault ? 0 :
                (Settings.LeftWallInteractionsSlider == ManualHelperModuleSettings.LeftWallInteractions.Off ? 1 : 2);
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
        if (whichOne == "RightWallInteractions")
        {
            return Settings.RightWallInteractionsSlider == ManualHelperModuleSettings.RightWallInteractions.MapDefault ? 0 :
                (Settings.RightWallInteractionsSlider == ManualHelperModuleSettings.RightWallInteractions.Off ? 1 : 2);
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
        Logger.Log(LogLevel.Error,nameof(ManualHelper),"[Error NonIdiot000] Hey twin, option "+whichOne+" isn't a valid ManualHelper Option. Seems like a skill issue.");
        return -1;
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
            return GetFlag(whichOne) == 1;
        }
        // otherwise return bool of if the given option is On
        return myOption == 2;
    }

    public static int ReturnFromCounterToggle(string whichOne)
    {
        int myOption = GetToggleData(whichOne);
        // if the given option is not found, return false
        if (myOption == -1)
        {
            Logger.Log(LogLevel.Error,nameof(ManualHelper),"[Error NonIdiot002] Yo nerd, whatever "+whichOne+" is, it ain't a ManualHelper Counter Toggle.");
            return -1;
        }
        // if the given option is Map Default, look at map's counters instead of config
        if (myOption == 0)
        {
            return GetCounter(whichOne);
        }
        // otherwise return whatever the counter says
        return myOption;
    }

    // true is left
    public static bool CanLRInteract(Player self, bool dir, string name, string prefix)
    {
        return dir ? ReturnFromBoolToggle("Bools/"+prefix+"/Left"+name) : ReturnFromBoolToggle("Bools/"+prefix+"/Right"+name);
    }
    
    // note that for flags, they are only used for 2-answer entries. the base format is "ManualHelper/ToggleBool_<togglename>", and 1 means unset by map and 0 means set by map.
    // if added via another mod, it will be closer to "ManualHelper/<ModName>/ToggleBool_<togglename>".
    public static int GetFlag(string flagName)
    {
        //thank u snip/tart1998 for the help
        if (Engine.Scene is not Level level)
            // handle case when you're not in a level
            return -1;

        return level.Session.GetFlag(ReturnOutputFromName(flagName,0)) ? 1 : 0;
    }
    
    // note that for counters, they are used everything but 2-answer entries. the base format is "ManualHelper/ToggleCounter_<togglename>", and 0 means unset by map and any other positive option means set by map.
    public static int GetCounter(string counterName)
    {
        //thank u snip/tart1998 for the help
        if (Engine.Scene is not Level level)
            // handle case when you're not in a level
            return -1;

        return level.Session.GetCounter(ReturnOutputFromName(counterName,0));
    }

    public static void SetFlag(string flagName, bool setTo)
    {
        if (Engine.Scene is not Level level)
            // handle case when you're not in a level
            return;

        level.Session.SetFlag(ReturnOutputFromName(flagName,0),setTo);
    }
    
    public static string ReturnFlagFromName(string myString)
    {
        //Logger.Log(LogLevel.Info, "ManualHelper_ReturnFlagFromName1", "uh um i thw "+myString);
        string flagName = myString.Substring(myString.IndexOf("/") + 1);
        //Logger.Log(LogLevel.Info, "ManualHelper_ReturnFlagFromName2", flagName+" was "+myString);
        string subStringString = flagName.Substring(flagName.IndexOf("/") + 1);
        subStringString = subStringString.Substring(subStringString.IndexOf("/") + 1);
        flagName = (flagName.StartsWith("ManualHelper/") ? subStringString : "/" + flagName.Remove(flagName.IndexOf("/")) + "/" + subStringString);
        //Logger.Log(LogLevel.Info, "ManualHelper_ReturnFlagFromName3", flagName+" was "+myString);
        string realFlagName = "ManualHelper/Toggle"+(myString.StartsWith("Bools") ? "Bool" : "Counter")+"_" + flagName;
        if (flagName.StartsWith("/"))
        {
            // for if added to a new category by a mod
            string myFlagName = flagName.Substring(1);
            realFlagName = "ManualHelper/" + myFlagName.Remove(myFlagName.LastIndexOf("/")) + "/Toggle"+(myString.StartsWith("Bools") ? "Bool" : "Counter")+"__" + myFlagName.Substring(myFlagName.LastIndexOf("/")+1);
        }
        else if (flagName.Contains("/"))
        {
            // for if added to an existing category by a mod
            realFlagName = "ManualHelper/Toggle"+(myString.StartsWith("Bools") ? "Bool" : "Counter")+"_" + flagName.Replace("/","__");
        }
        string theUselessThingSoICanTestTheFunction = ReturnOutputFromName(myString, 0);
        //Logger.Log(LogLevel.Info, "ManualHelper_ReturnFlagFromName5", realFlagName+" was "+myString+" funnyBool: "+flagName.StartsWith("/")+" alsoFunnyBool: "+flagName.Contains("__"));
        return realFlagName;
    }

    // This function is an all-in-one converter for converting the (key + ManualHelperToggles[key][#]) into one of many outputs.
    // An example of an input would be "Bools/ManualHelper/WallToggles/LeftClinging". This will be used below.
    // If type=0, it returns the flag/counter name. For example, "ManualHelper/ToggleBool_LeftClinging".
    // If type=1, it returns the translation name. For example, "MODOPTIONS_MANUALHELPER_LeftClinging".
    public static string ReturnOutputFromName(string input, int type)
    {
        // To note, there are four types of Toggles.
        // Firstly is a Toggle by ManualHelper, added to a ManualHelper menu.   This is found by if splitInputArray[1] is "ManualHelper" and if splitInputArray is 4-long.
        // Secondly is a Toggle by a second mod, added to a ManualHelper menu.  This is found by if splitInputArray[1] is "ManualHelper" and if splitInputArray is more than 4-long.
        // Thirdly is a Toggle by a second mod, added to a second mod's menu.   This is found by if splitInputArray[1] is not "ManualHelper" and if splitInputArray is 4-long.
        // Fourthly is a Toggle by a third mod, added to a second mod's menu.   This is found by if splitInputArray[1] is not "ManualHelper" and if splitInputArray is more than 4-long.
        string[] splitInputArray = input.Split('/');
        Logger.Log(LogLevel.Info, "ManualHelper_ReturnOutputFromName1", input+" was "+splitInputArray.Length+" long.");
        if (splitInputArray.Length < 4)
        {
            Logger.Log(LogLevel.Error, nameof(ManualHelper),"[Error NonIdiot004] Uh oh! ReturnOutputFromName was fed something too small.");
            return "error";
        }
        bool inManualHelperMenu = splitInputArray[1] == "ManualHelper";
        bool addedByMenuCreator = splitInputArray.Length == 4;
        string whichStarter = (splitInputArray[0] == "Bools" ? "Bool" : "Counter");
        string endOfLine = addedByMenuCreator ? "" : "_";
        string namespacee = inManualHelperMenu ? "" : splitInputArray[1];

        for (int a = 3; a < splitInputArray.Length; a++)
        {
            if (endOfLine != "" && endOfLine != "_")
            {
                endOfLine += "/";
            }
            endOfLine+=splitInputArray[a];
        }

        string output = "";
        switch (type)
        {
            case 0:
            {
                output = "ManualHelper/"+namespacee+"Toggle"+whichStarter+"_"+endOfLine;
                break;
            }
            case 1:
            {
                output = "MODOPTIONS_MANUALHELPER_"+endOfLine.Replace("/","__");
                break;
            }
        }
        if (output != "")
        {
            Logger.Log(LogLevel.Info, "ManualHelper_ReturnOutputFromName2", input+" outputted "+output+" from type "+type);
            return output;
        }
        Logger.Log(LogLevel.Error, nameof(ManualHelper),"[Error NonIdiot005] So it seems ReturnOutputFromName didn't return anything. HOW.");
        return "error";
    }

    public static void InitFlags(Level level)
    {
        // this breaks things sometimes. for some reason. idk
        /*if (Engine.Scene is not Level level)
        {
            // handle case when you're not in a level
            Logger.Log(LogLevel.Info,"ManualHelper_InitFlags","Hey, looks like you tried to InitFlags() while not in a level. For shame.");
            return;
        }*/
        Logger.Log(LogLevel.Info,"ManualHelper_InitFlags","Initializing all flags.");
        int flagsBeenSet = 0;
        int countersBeenSet = 0;
        
        /*Func<char,bool> myFunc = (i) =>
        {
            return "/".ToCharArray()[0] == i;
        };

        string[] setFlagsToTrue = [];
        string[] setCountersToDefault = [];
        string[] setCountersToDefaultButTheirKeys = [];
        foreach (string key in ManualHelperToggles.Keys)
        {
            string evilKey = "";
            if (key.StartsWith("Bools/"))
            {
                evilKey = "Bools/";
                foreach (string toggle in ManualHelperToggles[key])
                {
                    string splitThang = key.Substring(evilKey.Length);
                    //Logger.Log(LogLevel.Info, "ManualHelper_InitFlags1", key);
                    //Logger.Log(LogLevel.Info, "ManualHelper_InitFlags2", splitThang);
                    if (splitThang.StartsWith("ManualHelper/"))
                    {
                        string splitErThang = toggle.Count(myFunc) > 1 ? toggle.Remove(toggle.IndexOf("/"))+"/" : "";
                        //Logger.Log(LogLevel.Info, "ManualHelper_InitFlags11", splitThang+" m "+toggle+" ohohoh "+splitErThang+" juan "+toggle.Count(myFunc));
                        // results in the appended string being something like "LeftClinging" or "UsableDashAttack",
                        // which translates to the flag being named "ManualHelper/ToggleBool_LeftClinging".
                        setFlagsToTrue = setFlagsToTrue.Append(splitErThang+toggle).ToArray();
                    }
                    else
                    {
                        // results in the appended string being something like "/ModName/CustomToggle1",
                        // which translates to the flag being named "ManualHelper/ModName/ToggleBool__CustomToggle1".
                        //Logger.Log(LogLevel.Info, "ManualHelper_InitFlags3", splitThang.Remove(splitThang.LastIndexOf("/")));
                        // ReSharper disable once StringLastIndexOfIsCultureSpecific.1
                        setFlagsToTrue = setFlagsToTrue.Append("/"+splitThang.Remove(splitThang.LastIndexOf("/"))+"/"+toggle).ToArray();
                        //Logger.Log(LogLevel.Info, "ManualHelper_InitFlags8","/"+splitThang.Remove(splitThang.LastIndexOf("/"))+"/"+toggle);
                    }
                }
            }
            if (key.StartsWith("HarshLenientOn/") || false)
            {
                evilKey = "HarshLenientOn/";
                foreach (string toggle in ManualHelperToggles[key])
                {
                    string splitThang = key.Substring(evilKey.Length);
                    //Logger.Log(LogLevel.Info, "ManualHelper_InitFlags7", splitThang);
                    if (splitThang.StartsWith("ManualHelper/"))
                    {
                        string splitErThang = toggle.Count(myFunc) > 1 ? toggle.Substring(toggle.LastIndexOf("/"))+"/" : "";
                        Logger.Log(LogLevel.Info, "ManualHelper_InitFlags12", splitThang+" m "+toggle+" ohohoh "+splitErThang+" juan "+toggle.Count(myFunc)+" or "+"123/456/7/".Count(myFunc));
                        // results in the appended string being something like "CrumbleBlocks" or "Snowballs",
                        // which translates to the counter being named "ManualHelper/ToggleCounter_CrumbleBlocks".
                        setCountersToDefault = setCountersToDefault.Append(splitErThang+toggle).ToArray();
                    }
                    else
                    {
                        // results in the appended string being something like "/ModName/CustomToggle2",
                        // which translates to the counter being named "ManualHelper/ModName/ToggleCounter__CustomToggle2".
                        setCountersToDefault = setCountersToDefault.Append("/"+splitThang.Remove(splitThang.LastIndexOf("/"))+"/"+toggle).ToArray();
                        Logger.Log(LogLevel.Info, "ManualHelper_InitFlags6","/"+splitThang.Remove(splitThang.LastIndexOf("/"))+"/"+toggle);
                    }
                    // is added for later.
                    setCountersToDefaultButTheirKeys = setCountersToDefaultButTheirKeys.Append(key+"/"+toggle).ToArray();
                }
            }
            //setFlagsToTrue = setFlagsToTrue.Concat(ManualHelperToggles[key]).ToArray();
        }
        //setFlagsToTrue = setFlagsToTrue.Concat(ManualHelperTogglesBoolGrabs).ToArray();
        //setFlagsToTrue = setFlagsToTrue.Concat(ManualHelperTogglesBoolDashes).ToArray();
        //setFlagsToTrue = setFlagsToTrue.Concat(ManualHelperTogglesBoolVanillaEntities).ToArray();
        //setFlagsToTrue = setFlagsToTrue.Concat(ManualHelperTogglesBoolModdedEntities).ToArray();
        foreach (string flagName in setFlagsToTrue)
        {
            string realFlagName = "ManualHelper/ToggleBool_" + flagName;
            if (flagName.StartsWith("/"))
            {
                // for if added to a new category by a mod
                string myFlagName = flagName.Substring(1);
                //Logger.Log(LogLevel.Info, "ManualHelper_InitFlags10",myFlagName+" "+flagName);
                realFlagName = "ManualHelper/" + myFlagName.Remove(myFlagName.LastIndexOf("/")) + "/ToggleBool__" + myFlagName.Substring(myFlagName.LastIndexOf("/")+1);
            }
            else if (flagName.Contains("/"))
            {
                // for if added to an existing category by a mod
                realFlagName = "ManualHelper/ToggleBool_" + flagName.Replace("/","__");
            }
            //Logger.Log(LogLevel.Info, "ManualHelper_InitFlags4",realFlagName);
            if (!Session.flagsAlreadySet.Contains(realFlagName))
            {
                level.Session.SetFlag(realFlagName,true);
                Session.flagsAlreadySet.Add(realFlagName);
                flagsBeenSet++;
            }
        }

        //string[] setCountersToDefault = [];
        //setCountersToDefault=setFlagsToTrue.Concat(ManualHelperTogglesHarshLenientOnVanillaEntities).ToArray();
        foreach (string counterName in setCountersToDefault)
        {
            string realCounterName = "ManualHelper/ToggleCounter_"+counterName;
            if (counterName.StartsWith("/"))
            {
                // for if added to a new category by a mod
                string myFlagName = counterName.Substring(1);
                //Logger.Log(LogLevel.Info, "ManualHelper_InitFlags9",myFlagName.Remove(myFlagName.LastIndexOf("/"))+"   seminary   "+myFlagName.Substring(myFlagName.LastIndexOf("/")+1)+"   bp   ");
                realCounterName = "ManualHelper/" + myFlagName.Remove(myFlagName.LastIndexOf("/")) + "/ToggleCounter__" + myFlagName.Substring(myFlagName.LastIndexOf("/")+1);
            }
            else if (realCounterName.Contains("/"))
            {
                // for if added to an existing category by a mod
                realCounterName = "ManualHelper/ToggleCounter_" + realCounterName.Replace("/","__");
            }
            //Logger.Log(LogLevel.Info, "ManualHelper_InitFlags5",realCounterName+"   ao   "+counterName+"   bp   ");
            if (!Session.countersAlreadySet.Contains(realCounterName))
            {
                if (ManualHelperNonBoolToggleToInt.TryGetValue(setCountersToDefaultButTheirKeys[Array.IndexOf(setCountersToDefault,counterName)],out int[] toIntIfied))
                {
                    level.Session.SetCounter(realCounterName,toIntIfied[0]);
                    Session.countersAlreadySet.Add(realCounterName);
                    countersBeenSet++;
                }
                else
                {
                    Logger.Log(LogLevel.Error,"ManualHelper_InitFlags","wait ok huh "+realCounterName);
                }
            }
        }*/
        foreach (string key in ManualHelperToggles.Keys)
        {
            foreach (string toggle in ManualHelperToggles[key])
            {
                string result = ReturnOutputFromName(key+"/"+toggle, 0);
                if (key.StartsWith("Bools"))
                {
                    if (!Session.flagsAlreadySet.Contains(result))
                    {
                        level.Session.SetFlag(result,true);
                        Session.flagsAlreadySet.Add(result);
                        flagsBeenSet++;
                    }
                }
                else
                {
                    if (!Session.countersAlreadySet.Contains(result))
                    {
                        if (ManualHelperNonBoolToggleToInt.ContainsKey(key+"/"+toggle))
                        {
                            // sets the counter to its default, which is found in ManualHelperNonBoolToggleToInt.
                            level.Session.SetCounter(result,ManualHelperNonBoolToggleToInt[key+"/"+toggle][0]);
                            Session.countersAlreadySet.Add(result);
                            countersBeenSet++;
                        }
                        else
                        {
                            Logger.Log(LogLevel.Error,"ManualHelper_InitFlags","wait ok huh "+result);
                        }
                    }
                }
            }
        }

        Logger.Log(LogLevel.Info,"ManualHelper_InitFlags","Flags initialized! Flags set: "+flagsBeenSet+" Counters set: "+countersBeenSet);
    }

    public static string[] ReturnAllCategories()
    {
        string[] allCategories = [];
        foreach (string key in ManualHelperToggles.Keys)
        {
            if (key.Contains("/"))
            {
                if (!allCategories.Contains(key.Substring(key.IndexOf("/")+1)))
                {
                    allCategories = allCategories.Append(key.Substring(key.IndexOf("/")+1)).ToArray();
                }
            }
            else
            {
                // in case of issues, report here
                Logger.Log(LogLevel.Error, "ManualHelper_ReturnAllCategories","Well it sure seems that "+key+" doesn't have a \"/\" in it. Odd.");
            }
        }
        if (1 == 0)
#pragma warning disable CS0162 // Unreachable code detected
        {
            Logger.Log(LogLevel.Info, "ManualHelper_ReturnAllCategories2","Sending all categories!");
            foreach (string category in allCategories)
            {
                Logger.Log(LogLevel.Info, "ManualHelper_ReturnAllCategories2",category);
            }
            Logger.Log(LogLevel.Info, "ManualHelper_ReturnAllCategories2","All categories sent!");
        }
#pragma warning restore CS0162 // Unreachable code detected
        return allCategories;
    }

    public static void RetractDash(Player self)
    {
        if (self.Dashes > 0)
        {
            self.Dashes -= 1;
        }
    }

    public static bool HasDoorOfType(Door myDoor)
    {
        return (ReturnFromBoolToggle("Bools/ManualHelper/VanillaEntityToggles/NonWoodenDoors") && myDoor.openSfx != "event:/game/03_resort/door_wood_open") ||
               (ReturnFromBoolToggle("Bools/ManualHelper/VanillaEntityToggles/WoodenDoors") && myDoor.openSfx == "event:/game/03_resort/door_wood_open");
    }

    // hooks

    // for preventing climbing stuff
    private static bool OnCelestePlayerClimbCheck(On.Celeste.Player.orig_ClimbCheck orig, Player self, int dir, int yAdd)
    {
        if ((((!ReturnFromBoolToggle("Bools/ManualHelper/WallToggles/LeftClinging") || (!ReturnFromBoolToggle("Bools/ManualHelper/DashToggles/LeftDashlessClinging") && self.Dashes < 1)) && dir == -1) || ((!ReturnFromBoolToggle("Bools/ManualHelper/WallToggles/RightClinging") || (!ReturnFromBoolToggle("Bools/ManualHelper/DashToggles/RightDashlessClinging") && self.Dashes < 1)) && dir == 1)) && !self.level.InCredits)
        {
            return false;
        }

        return orig(self, dir, yAdd);
    }

    // for preventing climbjump stuff
    private static void OnCelestePlayerClimbJump(On.Celeste.Player.orig_ClimbJump orig, Player self)
    {
        if (((self.Facing == Facings.Left ?
                (self.Ducking ? ReturnFromBoolToggle("Bools/ManualHelper/WallToggles/LeftCrouchedClimbjumping") :
                    ReturnFromBoolToggle("Bools/ManualHelper/WallToggles/LeftUncrouchedClimbjumping")) :
                (self.Ducking ? ReturnFromBoolToggle("Bools/ManualHelper/WallToggles/RightCrouchedClimbjumping") :
                    ReturnFromBoolToggle("Bools/ManualHelper/WallToggles/RightUncrouchedClimbjumping"))) &&
                (self.Dashes > 0 || CanLRInteract(self, self.Facing == Facings.Left, "DashlessClimbjumping","ManualHelper/DashToggles")))
            || self.level.InCredits)
        {
            orig(self);
            if (!CanLRInteract(self, self.Facing == Facings.Left, "ClimbjumpingDoesntCostDashes","ManualHelper/DashToggles"))
            {
                if (self.Dashes > 0)
                {
                    Audio.Play("event:/char/madeline/jump_dreamblock",self.Position);
                }
                //self.Dashes -= 1;
                RetractDash(self);
            }
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
        bool doOrig = false;
        if (self.Dashes > 0 || CanLRInteract(self, dir == 1, "DashlessWalljumps","ManualHelper/DashToggles"))
        {
            if (CanLRInteract(self,dir == 1,"WallInteractions","ManualHelper/WallToggles"))
            {
                doOrig = true;
            }
            else if (self.StateMachine.State == 1)
            {
                self.moveX = -dir;
                doOrig = true;
            }
        }
        else if (self.StateMachine.State == 1)
        {
            self.moveX = -dir;
            doOrig = true;
        }
        if (doOrig)
        {
            orig(self, dir);
            if (!CanLRInteract(self, dir == 1, "WalljumpsDontCostDashes","ManualHelper/DashToggles"))
            {
                if (self.Dashes > 0)
                {
                    Audio.Play("event:/char/madeline/jump_dreamblock",self.Position);
                }
                //self.Dashes -= 1;
                RetractDash(self);
            }
        }
    }

    // for preventing wallbounce stuff
    private static void OnCelestePlayerSuperWallJump(On.Celeste.Player.orig_SuperWallJump orig, Player self, int dir)
    {
        if (CanLRInteract(self,dir == 1,"Wallbounces","ManualHelper/WallToggles"))
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
            Audio.Play("event:/char/madeline/core_hair_charged",self.Position);//,"volume",2
        }
    }

    // for preventing sliding on walls that aren't enabled, and elytra disabling
    private static int OnCelestePlayerNormalUpdate(On.Celeste.Player.orig_NormalUpdate orig, Player self)
    {
        if (!CanLRInteract(self, (int)self.Facing == -1,"WallInteractions","ManualHelper/WallToggles") && (self as Monocle.Entity).CollideCheck<Solid>(self.Position + Vector2.UnitX * (float)self.Facing))
        {
            self.wallSlideTimer = 0;
        }

        if (communalHelperLoaded && !ReturnFromBoolToggle("Bools/ManualHelper/ModdedEntityToggles/AllowElytra"))
        {
            CommunalHelperImports.SetElytraEnabled.Invoke(false);
            //CommunalHelperImports.SetInfiniteElytra.Invoke(self, false);
        }

        return orig(self);
    }

    // for preventing dash attack state. thx to maddie480's Extended Variants for the code!
    // ReSharper disable once UnusedMember.Local
    private static bool weirdHookCelestePlayerGetDashAttacking(Func<Player, bool> orig, Player self) {
        if (!isRenderingCode && !ReturnFromBoolToggle("Bools/ManualHelper/DashToggles/UsableDashAttack")) {
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
        if (!ReturnFromBoolToggle("Bools/ManualHelper/VanillaEntityToggles/HeartDoors"))
        {
            (scene as Level).Session.SetFlag("opened_heartgem_door_" + self.Requires, false);
            self.Opened = false;
            self.Visible = true;
            self.openPercent = 1f;
            self.startHidden = false;
        }

        orig(self, scene);
    }

    // Prevents heart door from opening
    // ReSharper disable once UnusedMember.Local
    private static int weirdHookCelesteHeartGemDoorSetHeartGems(Func<HeartGemDoor, int> orig, HeartGemDoor self) {
        if (!ReturnFromBoolToggle("Bools/ManualHelper/VanillaEntityToggles/HeartDoors")) {
            return 0;
        }

        return orig(self);
    }
    
    // pause menu button function.
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
                    //Logger.Log(nameof(ManualHelper),CommunalHelperImports.HasDreamTunnelDash().ToString());
                }
                else
                {
                    //Logger.Log(nameof(ManualHelper), "There is no Communal Helper in Ba Sing Se.");
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

    // disables CrumbleBlocks (part 1)
    private static Player OnCelesteSolidGetPlayerOnTop(On.Celeste.Solid.orig_GetPlayerOnTop orig, Solid self)
    {
        if (self is CrumblePlatform)
        {
            if (ReturnFromCounterToggle("HarshLenientOn/ManualHelper/VanillaEntityToggles/CrumbleBlocks") != 3)
            {
                if (self.Collidable)
                {
                    if (orig(self) != null)
                    {
                        Audio.Play("event:/game/general/assist_nonsolid_out",self.Position);
                        self.Collidable = false;
                        Player realSelf = orig(self);
                        if (ReturnFromCounterToggle("HarshLenientOn/ManualHelper/VanillaEntityToggles/CrumbleBlocks") == 1)
                        {
                            realSelf.Dashes = 0;
                        }
                        realSelf.Speed.Y = 0;
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

    // disables CrumbleBlocks (part 2)
    private static Player OnCelesteSolidGetPlayerClimbing(On.Celeste.Solid.orig_GetPlayerClimbing orig, Solid self)
    {
        if (self is CrumblePlatform)
        {
            if (ReturnFromCounterToggle("HarshLenientOn/ManualHelper/VanillaEntityToggles/CrumbleBlocks") != 3)
            {
                if (self.Collidable)
                {
                    if (orig(self) != null)
                    {
                        Audio.Play("event:/game/general/assist_nonsolid_out",self.Position);
                        self.Collidable = false;
                        Player realSelf = orig(self);
                        if (ReturnFromCounterToggle("HarshLenientOn/ManualHelper/VanillaEntityToggles/CrumbleBlocks") == 1)
                        {
                            realSelf.Dashes = 0;
                        }
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

    // flag initialization hook. probably the least stable thing in this mod for some reason?????
    private static void EverestEventsLevelLoaderOnLoadLevel(Level level, Player.IntroTypes introType, bool isFromData)
    {
        //Logger.Log(LogLevel.Info,"ManualHelper_OnLoadLevel","OnLoadLevel called! isFromData: "+isFromData);
        if (!isFromData)
        {
            Session.flagsAlreadySet = new HashSet<string>();
        }

        InitFlags(level);
    }

    public static float shakeAmount = 1;
    private static void OnCelestePlayerRender(On.Celeste.Player.orig_Render orig, Player self)
    {
        
        //shakeAmount = ReturnFromCounterToggle("HarshLenientOn/ManualHelper/VanillaEntityToggles/CrumbleBlocks") == 3 ? 0 : (self.Ducking ? 0.5f : 1);
        shakeAmount = ReturnFromBoolToggle("Bools/ManualHelper/ModdedEntityToggles/FakeMod/Awewa") == true ? 0 : (self.Ducking ? 0.5f : 1);
        
        Vector2 coolOffset = Vector2.Zero;
        float tempShakeAmount = shakeAmount + 0;
        if (tempShakeAmount > 0)
        {
            coolOffset = Calc.Random.ShakeVector();
            self.Position += coolOffset*tempShakeAmount;
        }
        orig(self);
        if (coolOffset != Vector2.Zero)
        {
            self.Position -= coolOffset*tempShakeAmount;
        }
    }

    private static void OnCelestePlayerUpdateHair(On.Celeste.Player.orig_UpdateHair orig, Player self, bool applyGravity)
    {
        Vector2 coolOffset = Vector2.Zero;
        float tempShakeAmount = shakeAmount + 0;
        if (tempShakeAmount > 0)
        {
            coolOffset = Calc.Random.ShakeVector();
            self.Hair.MoveHairBy(coolOffset*tempShakeAmount);
        }
        orig(self,applyGravity);
        if (coolOffset != Vector2.Zero)
        {
            self.Hair.MoveHairBy(-1*coolOffset*tempShakeAmount);
        }
    }

    // for preventing bounce on Snowballs.
    private static void OnCelesteSnowballOnPlayerBounce(On.Celeste.Snowball.orig_OnPlayerBounce orig, Snowball self, Player player)
    {
        if (ReturnFromCounterToggle("HarshLenientOn/ManualHelper/VanillaEntityToggles/Snowballs") == 3)
        {
            orig(self, player);
        }
        else if (ReturnFromCounterToggle("HarshLenientOn/ManualHelper/VanillaEntityToggles/Snowballs") == 1)
        {
            self.OnPlayer(player);
        }
    }
    
    // for preventing bounce on Oshiro Bosses.
    private static void OnCelesteAngryOshiroOnPlayerBounce(On.Celeste.AngryOshiro.orig_OnPlayerBounce orig, AngryOshiro self, Player player)
    {
        if (ReturnFromCounterToggle("HarshLenientOn/ManualHelper/VanillaEntityToggles/OshiroBosses") == 3)
        {
            orig(self, player);
        }
        else if (ReturnFromCounterToggle("HarshLenientOn/ManualHelper/VanillaEntityToggles/OshiroBosses") == 1)
        {
            self.OnPlayer(player);
        }
    }

    // makes the submenu subheaders move correctly. if you see "nonidiotplaceholder_" at the start of a title, thats really bad.
    private static void OnCelesteTextMenuExtSubHeaderExtRender(On.Celeste.TextMenuExt.SubHeaderExt.orig_Render orig, TextMenuExt.SubHeaderExt self, Vector2 position, bool highlighted)
    {
        if (self.Title.StartsWith("nonidiotplaceholder_"))
        {
            self.Title=self.Title.Substring("nonidiotplaceholder_".Length);
            orig(self, new Vector2(position.X+50,position.Y), highlighted);
            self.Title = "nonidiotplaceholder_" + self.Title;
        }
        else
        {
            orig(self, position, highlighted);    
        }
    }

    // pause on death
    private static PlayerDeadBody OnCelestePlayerDie(On.Celeste.Player.orig_Die orig, Player self, Vector2 direction, bool evenIfInvincible, bool registerDeathInStats)
    {
        if (self.level != null && Settings.PauseOnDeath)
        {
            self.level.Pause();
        }

        return orig(self, direction,evenIfInvincible, registerDeathInStats);
    }

    // drawing the heart gem door but evil
    private static void OnCelesteHeartGemDoorDrawEdges(On.Celeste.HeartGemDoor.orig_DrawEdges orig, HeartGemDoor self, Rectangle bounds, Color color)
    {
        if (!ReturnFromBoolToggle("Bools/ManualHelper/VanillaEntityToggles/HeartDoors")) {
            orig(self, bounds, new Color((int)(Math.Sin((float)Engine.FrameCounter/10f)*127)+127, 0, 0));
        }
        else
        {
            orig(self, bounds, color);
        }
    }

    // making the doors not open lmao
    private static void OnCelesteDoorOpen(On.Celeste.Door.orig_Open orig, Door self, float myFloat)
    {
        if (HasDoorOfType(self))
        {
            orig(self,myFloat);
            //self.disabled = false;
        }
        else
        {
            self.disabled = true;
        }
    }

    // also more door stuff
    private static void OnCelesteDoorUpdate(On.Celeste.Door.orig_Update orig, Door self)
    {
        orig(self);
        // only adds to scene if doors is not enabled, to save on computation when Doors hasnt been modified yet
        if (!HasDoorOfType(self) && Engine.Scene is Level level)
        {
            bool wasBroken = false;
            foreach (DoorCollider entity in Engine.Scene.Tracker.GetEntities<DoorCollider>())//Entity entity in level.Entities
            {
                if (((DoorCollider)entity).myDoor == self)//entity is DoorCollider && 
                {
                    wasBroken = true;
                    break;
                }
            }
            if (!wasBroken)
            {
                level.Add(new DoorCollider(self,self.Position-new Vector2(0,24),2,24));
            }
        }
    }
    
    // door collider stuff, not technically a hook but eh
    [Tracked]
    public class DoorCollider : Solid
    {
        public DoorCollider(Door myDoor, Vector2 position, float width, float height)
            : base(position, width, height, true)
        {
            this.myDoor = myDoor;
            //position = myDoor.Position;
            //width = 2;
            //height = 24;
            //Engine.Commands.Log("Door position: "+myDoor.Position+", my position: "+position);
            wasEvil = true;
        }

        public override void Update()
        {
            if (myDoor == null)
            {
                base.Remove();
            }
            else
            {
                base.Update();
                if (HasDoorOfType(myDoor))
                {
                    this.Collidable = false;
                    if (wasEvil)
                    {
                        wasEvil = false;
                        myDoor.disabled = false;
                    }
                }
                else
                {
                    this.Collidable = true;
                    wasEvil = true;
                }
            }
        }

        public Door myDoor;

        public bool wasEvil = false;
    }
    
    
    // Prevents heart door from opening
    // ReSharper disable once UnusedMember.Local
    /*private static int weirdHookCommunalHelperGetElytraCooldown(Func<StateMachine, int> orig, StateMachine self) {
        int dreamTunnelState = CommunalHelperImports.GetDreamTunnelDashState?.Invoke() ?? -999;
        if (self.Entity is Player && !ReturnFromBoolToggle("Bools/ManualHelper/WallToggles/ForceDisableElytra") && dreamTunnelState+1 == self.State) {
            return 0;
        }
        else
        {
            return orig(self);
        }
    }*/

    private static void OnCelesteLevelLoaderCtor(On.Celeste.LevelLoader.orig_ctor orig, LevelLoader self, Session session, Vector2? startPosition)
    {
        if (gravityHelperLoaded)
        {
            //EntityData myEnt = new EntityData();
            //myEnt.Values.Add("_gravityHelper",new Object());
            //session.MapData.Levels[0].Entities.Add(myEnt);
        }

        orig(self, session, startPosition);
    }
}