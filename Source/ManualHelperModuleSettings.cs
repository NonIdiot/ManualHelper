using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Monocle;
using YamlDotNet.Serialization;

namespace Celeste.Mod.ManualHelper;

public class ManualHelperModuleSettings : EverestModuleSettings {
    
    // settings
    
    public Dictionary<string, int[]> DynamicSettings { get; set; } = new();

    public void InitializeDynamicSettings()
    {
        if (!ManualHelper.DynamicSettingsSet)
        {
            Logger.Log(LogLevel.Info, "ManualHelper_InitializeDynamicSettings","Initializing Dynamic Settings!");
            ManualHelper.DynamicSettingsSet = true;
            
            ManualHelper.Settings.DynamicSettings = new Dictionary<string, int[]>();
            // for the buttons at the top of each subsection
            for (int a = 1; a <= 4; a++)
            {
                // for buttons, the <current value> is instead what it should set non-buttons to.
                ManualHelper.Settings.DynamicSettings.Add("AllMapDefault_"+a,[0,a,2]);
                ManualHelper.Settings.DynamicSettings.Add("AllOff_"+a,[1,a,2]);
                ManualHelper.Settings.DynamicSettings.Add("AllOn_"+a,[2,a,2]);
                // logging
                //ManualHelper.Settings.DynamicSettings.TryGetValue("AllOn_" + a, out int[] valuu);
                //Logger.Log(LogLevel.Info,"ManualHelper_Dummy","a"+valuu[0]+" "+valuu[1]+" "+valuu.Length+" "+valuu);
            }

            string[] myList1 = ManualHelper.ManualHelperTogglesBoolGrabs;
            foreach (string item1 in myList1)
            {
                //if (item1 != "LeftClinging")
                //{
                    // format is <current value>, <submenu it should go in>, <type of setting>.
                    // <current value> is default 0, and set to a new value when changed in Slider form. dont set this to anything other than 0 when doing .Add().
                    // <submenu it should go in> is self-explanatory and easy to figure out. search "int myInt = " if ur not sure tho
                    // <type of setting> is 0 if MapSetting/Off/On, 1 if MapSetting/OffHarsh/OffLenient/On, 2 if button,
                    ManualHelper.Settings.DynamicSettings.Add(item1,[0,1,0]);
                //}
            }
            string[] myList2 = ManualHelper.ManualHelperTogglesBoolDashes;
            foreach (string item2 in myList2)
            {
                ManualHelper.Settings.DynamicSettings.Add(item2,[0,2,0]);
            }
            string[] myList3 = ManualHelper.ManualHelperTogglesBoolVanillaEntities;
            foreach (string item3 in myList3)
            {
                ManualHelper.Settings.DynamicSettings.Add(item3,[0,3,0]);
            }
            string[] myList4 = ManualHelper.ManualHelperTogglesHarshLenientOnVanillaEntities;
            foreach (string item4 in myList4)
            {
                // second value is cause its still in the Vanilla Entities tab.
                // third value is cause its the Harsh/Lenient one instead of regular On/Off.
                ManualHelper.Settings.DynamicSettings.Add(item4,[0,3,1]);
            }
            string[] myList5 = ManualHelper.ManualHelperTogglesBoolModdedEntities;
            foreach (string item5 in myList5)
            {
                ManualHelper.Settings.DynamicSettings.Add(item5,[0,4,0]);
            }

            if (1 == 1) // set to 1==0 if unused. this adds a LOT of entries to the log...
            {
                foreach (string item1 in myList1)
                {
                    Logger.Log(LogLevel.Info, "ManualHelper_Dummy","| "+Dialog.Clean("MODOPTIONS_MANUALHELPER_" + item1)+" | "+Dialog.Clean("MODOPTIONS_MANUALHELPER_" + item1 + "Desc").ReplaceLineEndings("<br/>")+" | Bool | "+item1+" | "+Dialog.Clean("MODOPTIONS_MANUALHELPER_WallTogglesHeader")+" |");
                }
                Logger.Log(LogLevel.Info, "ManualHelper_Dummy", "|.|.|.|.|.|");
                foreach (string item2 in myList2)
                {
                    Logger.Log(LogLevel.Info, "ManualHelper_Dummy","| "+Dialog.Clean("MODOPTIONS_MANUALHELPER_" + item2)+" | "+Dialog.Clean("MODOPTIONS_MANUALHELPER_" + item2 + "Desc").ReplaceLineEndings("<br/>")+" | Bool | "+item2+" | "+Dialog.Clean("MODOPTIONS_MANUALHELPER_DashTogglesHeader")+" |");
                }
                Logger.Log(LogLevel.Info, "ManualHelper_Dummy", "|.|.|.|.|.|");
                foreach (string item3 in myList3)
                {
                    Logger.Log(LogLevel.Info, "ManualHelper_Dummy","| "+Dialog.Clean("MODOPTIONS_MANUALHELPER_" + item3)+" | "+Dialog.Clean("MODOPTIONS_MANUALHELPER_" + item3 + "Desc").ReplaceLineEndings("<br/>")+" | Bool | "+item3+" | "+Dialog.Clean("MODOPTIONS_MANUALHELPER_VanillaEntityTogglesHeader")+" |");
                }
                foreach (string item4 in myList4)
                {
                    Logger.Log(LogLevel.Info, "ManualHelper_Dummy","| "+Dialog.Clean("MODOPTIONS_MANUALHELPER_" + item4)+" | "+Dialog.Clean("MODOPTIONS_MANUALHELPER_" + item4 + "Desc").ReplaceLineEndings("<br/>")+" | Harsh/Lenient/On | "+item4+" | "+Dialog.Clean("MODOPTIONS_MANUALHELPER_VanillaEntityTogglesHeader")+" |");
                }
            }
        }
    }

    [YamlIgnore]
    [SettingName("MODOPTIONS_MANUALHELPER_WallTogglesHeader")]
    public DynamicSettingsMenu1 SettingsMenu1 { get; set; } = new();
    [YamlIgnore]
    [SettingName("MODOPTIONS_MANUALHELPER_DashTogglesHeader")]
    public DynamicSettingsMenu1 SettingsMenu2 { get; set; } = new();
    [YamlIgnore]
    [SettingName("MODOPTIONS_MANUALHELPER_VanillaEntityTogglesHeader")]
    public DynamicSettingsMenu1 SettingsMenu3 { get; set; } = new();
    
    [SettingSubMenu]
    public class DynamicSettingsMenu1
    {
        [YamlIgnore]
        public bool Dummy1 { get; set; }

        // this is for the items that dynamicsettings uses. its literally only used for the TurnAllOfSectionInto function
        public static Dictionary<string, TextMenu.Item> DynamicSettingItems = new();

        Func<int, string> MapDefaultOrElseBool = (i) =>
        {
            return
                i == 0 ? Dialog.Clean("MODOPTIONS_MANUALHELPER_MapDefault") : (i == 1 ? Dialog.Clean("MODOPTIONS_MANUALHELPER_Off") : Dialog.Clean("MODOPTIONS_MANUALHELPER_On"));
        };
        Func<int, string> MapDefaultOrElseHarshLenientOn = (i) =>
        {
            return
                i == 0 ? Dialog.Clean("MODOPTIONS_MANUALHELPER_MapDefault") : (i == 1 ? Dialog.Clean("MODOPTIONS_MANUALHELPER_OffHarsh") : (i == 2 ? Dialog.Clean("MODOPTIONS_MANUALHELPER_OffLenient") : Dialog.Clean("MODOPTIONS_MANUALHELPER_On")));
        };

        // these two ints and two functions are for the buttons that set all of a submenu to a single value.
        public int TurnAllVarSection = -1;
        public int TurnAllVarResulte = -1;
        public void TurnAllOfSectionInto()
        {
            foreach ((string settingName, TextMenu.Item myItem) in DynamicSettingItems)
            {
                if (ManualHelper.Settings.DynamicSettings.TryGetValue(settingName, out int[] value) && value[1] == TurnAllVarSection)
                {
                    if (myItem is TextMenu.Slider)
                    {
                        // be sure that once more-than-3 options are added, that they are accounted for here too
                        // this if statement is basically "if its an on/off, OR this is the "Map Default" button, set it all to the given button value."
                        if (value[2] == 0 || TurnAllVarResulte == 0)
                        {
                            ((TextMenu.Slider)myItem).Index = TurnAllVarResulte;
                            ((TextMenu.Slider)myItem).OnValueChange(((TextMenu.Slider)myItem).Values[((TextMenu.Slider)myItem).Index].Item2);
                        }
                        else if (value[2] == 1 && ManualHelper.ManualHelperNonBoolToggleToInt.TryGetValue(settingName, out int[] toIntIfied))
                        {
                            int realResulte = TurnAllVarResulte;
                            // the "off" button, which resets the value to the "disabled".
                            if (TurnAllVarResulte == 1)
                            {
                                realResulte = toIntIfied[1];
                            }
                            // the "on" button, which resets the value to the "on", aka vanilla.
                            else if (TurnAllVarResulte == 2)
                            {
                                realResulte = toIntIfied[0];
                            }
                            ((TextMenu.Slider)myItem).Index = realResulte;
                            ((TextMenu.Slider)myItem).OnValueChange(((TextMenu.Slider)myItem).Values[((TextMenu.Slider)myItem).Index].Item2);
                        }
                    }
                }
            }
            TurnAllVarSection = -1;
            TurnAllVarResulte = -1;
        }
        public Action CreateAction(int varSection, int varResulte)
        {
            // INCREDIBLY scuffed method of getting this to work how i want. dont do this at home
            return () =>
            {
                TurnAllVarSection = varSection;
                TurnAllVarResulte = varResulte;
                TurnAllOfSectionInto();
            };
        }

        public Color errorColor(int errorCode)
        {
            Logger.Log(LogLevel.Error,nameof(ManualHelper),"[Error NonIdiot003] errorColor detected. deploy the error code "+errorCode);
            // error color.
            return ManualHelper.ManualHelperAllMenuColors[5];
        }

        // this function gets a value from myInt, and returns:
        // gray if isDisabled is true
        // white if 0 and the flag is default (or not ingame)
        // blue if 0 and flag is non-default
        // orange if above 0 and flag is default (or not ingame)
        // pink if above 0 and flag is non-default
        public Color returnColorFromInt(int myInt, bool isDisabled, string settingName, int typeOfSetting)
        {
            // add support for typeOfSetting once it is implemented
            if (typeOfSetting < 0 || typeOfSetting > 1)
            {
                return errorColor(0);
            }
            if (typeOfSetting == 0 || typeOfSetting == 1)
            {
                if (myInt < 0 || myInt > 2 + typeOfSetting)
                {
                    return errorColor(1);
                }
            }
            bool isCounterGood = ManualHelper.ManualHelperNonBoolToggleToInt.TryGetValue(settingName, out int[] toIntIfied);
            if (typeOfSetting == 1 && !isCounterGood)
            {
                return errorColor(2);
            }
            bool changedByPlayer = myInt != 0;
            int returnedFlag = ManualHelper.GetFlag("ManualHelper/ToggleBool_" + settingName);
            int returnedCounter = ManualHelper.GetCounter("ManualHelper/ToggleCounter_" + settingName);
            // be sure to change this when more non-bool toggles are added! 
            bool changedByMap = Engine.Scene is Level level &&
                (typeOfSetting == 0 ? (returnedFlag == 0) : 
                (typeOfSetting == 1 && toIntIfied != null ? (returnedCounter != toIntIfied[0]) :
                false));
            // dont touch the below line. its perfect the way it is methinks
            return isDisabled ? ManualHelper.ManualHelperAllMenuColors[4] : (changedByMap ? (changedByPlayer ? ManualHelper.ManualHelperAllMenuColors[2] : ManualHelper.ManualHelperAllMenuColors[1]) : (changedByPlayer ? ManualHelper.ManualHelperAllMenuColors[0] : ManualHelper.ManualHelperAllMenuColors[3]));
            //return isDisabled ? Color.DarkSlateGray: (myInt == 0 ? (Color.Red) : (myInt == 1 ? Color.Goldenrod : (myInt == 2 ? Color.Goldenrod : Color.Purple)));
        }

        public void CreateDummy1Entry(TextMenuExt.SubMenu menu, bool inGame, TextMenu bigMenu)
        {
            int myInt = menu.Label == Dialog.Clean("MODOPTIONS_MANUALHELPER_WallTogglesHeader") ? 1 : (menu.Label == Dialog.Clean("MODOPTIONS_MANUALHELPER_DashTogglesHeader") ? 2 : (menu.Label == Dialog.Clean("MODOPTIONS_MANUALHELPER_VanillaEntityTogglesHeader") ? 3 : (menu.Label == Dialog.Clean("MODOPTIONS_MANUALHELPER_ModdedEntityTogglesHeader") ? 4 : 5)));
            //Logger.Log(LogLevel.Info,"ManualHelper",menu.Label+" "+myInt.ToString());
            //Logger.Log(LogLevel.Info, "ManualHelper_Dummy", "bbb"+ManualHelper.Settings.DynamicSettings.Count);
            // dunno why it adds random things to DynamicSettings before this happens, but oh well. not my fault (probably)
            // anyways this should only run once per time the menu opens.

            Dictionary<string, int[]> dynamicSettings = ManualHelper.Settings.DynamicSettings;

            foreach ((string settingName, int[] settingValue) in dynamicSettings)
            {
                if (settingValue[1] == myInt)
                {
                    bool isValid = false;
                    TextMenu.Item myItem = new TextMenu.Button(label: "");
                    if (settingValue.Length >= 3)
                    {
                        if (settingValue[2] == 0 || settingValue[2] == 1)
                        {
                            myItem = new TextMenu.Slider(
                                label: "  "+Dialog.Clean("MODOPTIONS_MANUALHELPER_" + settingName),
                                values: settingValue[2] == 0 ? MapDefaultOrElseBool : MapDefaultOrElseHarshLenientOn,
                                min: 0,
                                max: 2 + settingValue[2],
                                value: settingValue[0]
                            );
                            ((TextMenu.Slider)myItem).Change(newValue =>
                            {
                                dynamicSettings[settingName][0] = newValue;
                                ((TextMenu.Slider)myItem).UnselectedColor = returnColorFromInt(newValue,myItem.Disabled,settingName,settingValue[2]);
                            });
                            ((TextMenu.Slider)myItem).UnselectedColor = returnColorFromInt(settingValue[0],myItem.Disabled,settingName,settingValue[2]);
                            
                            isValid = true;
                            //Logger.Log(LogLevel.Info,"ManualHelper","lalalae "+settingName+" "+settingValue[0]+" "+settingValue[1]+" "+settingValue[2]);
                        }
                        else if (settingValue[2] == 2)
                        {
                            myItem = new TextMenu.Button(
                                label: Dialog.Clean("MODOPTIONS_MANUALHELPER_" + settingName.Remove(settingName.Length-2))
                            );
                            ((TextMenu.Button)myItem).Pressed(CreateAction(myInt, settingValue[0]));
                            
                            isValid = true;
                        }
                    }
                    else
                    {
                        Logger.Log(LogLevel.Warn,"ManualHelper","Woah, woah, woah there buster, looks like you got a case of improperly made DynamicSettings.Add() code. It's a "+menu.Label+" problem, specifically.");
                    }

                    if (isValid)
                    {
                        DynamicSettingItems[settingName] = myItem;
                        menu.Add(myItem);
                        if (Dialog.Has("MODOPTIONS_MANUALHELPER_" + settingName + "Desc", Dialog.Language))
                        {
                            myItem.AddDescription(menu, bigMenu, "nonidiotplaceholder_"+Dialog.Clean("MODOPTIONS_MANUALHELPER_" + settingName + "Desc"));
                        }
                    }
                }
            }
        }
    }
    
    
    //public bool WallTogglesSubmenu { get; set; } = true;
    //public TextMenuExt.SubMenu WallTogglesSubmenuEntry;
    /*Func<int, string> MapDefaultOrElse = (i) =>
    {
        return
            i == 0 ? Dialog.Clean("MODOPTIONS_MANUALHELPER_MapDefault") : (i == 1 ? Dialog.Clean("MODOPTIONS_MANUALHELPER_Off") : Dialog.Clean("MODOPTIONS_MANUALHELPER_On"));
    };
    public TextMenu.Slider MakeNewEntry(string name)
    {
        return (new TextMenu.Slider(
            label: Dialog.Clean("MODOPTIONS_MANUALHELPER_"+name),
            values: MapDefaultOrElse,
            min: 0,
            max: 2,
            value: 0
        ));
    }

    public enum UsableDashAttack { MapDefault, Off, On }
    [SettingSubText("MODOPTIONS_MANUALHELPER_UsableDashAttackDesc")]
    [SettingName("MODOPTIONS_MANUALHELPER_UsableDashAttack")]
    public UsableDashAttack UsableDashAttackSlider { get; set; } = UsableDashAttack.MapDefault;

    public void CreateWallTogglesSubmenuEntry(TextMenu menu, bool inGame)
    {
        menu.Add(WallTogglesSubmenuEntry = new TextMenuExt.SubMenu(
            label: Dialog.Clean("MODOPTIONS_MANUALHELPER_WallTogglesHeader"),
            enterOnSelect: true
        ));

        WallTogglesSubmenuEntry.Add(MakeNewEntry("LeftClinging"));
    }*/

    //[YamlIgnore]
    //public bool SubmenuExample { get; set; }
    /*public enum LeftClinging { MapDefault, Off, On }
    //[SettingSubHeader("MODOPTIONS_MANUALHELPER_WallTogglesHeader")]

    //[SettingSubText("MODOPTIONS_MANUALHELPER_LeftClingingDesc")]
    //[SettingName("MODOPTIONS_MANUALHELPER_LeftClinging")]
    //public LeftClinging LeftClingingSlider { get; set; } = LeftClinging.MapDefault;
    public enum LeftUncrouchedClimbjumping { MapDefault, Off, On }
    [SettingSubText("MODOPTIONS_MANUALHELPER_LeftUncrouchedClimbjumpingDesc")]
    [SettingName("MODOPTIONS_MANUALHELPER_LeftUncrouchedClimbjumping")]
    public LeftUncrouchedClimbjumping LeftUncrouchedClimbjumpingSlider { get; set; } = LeftUncrouchedClimbjumping.MapDefault;
    public enum LeftCrouchedClimbjumping { MapDefault, Off, On }
    [SettingSubText("MODOPTIONS_MANUALHELPER_LeftCrouchedClimbjumpingDesc")]
    [SettingName("MODOPTIONS_MANUALHELPER_LeftCrouchedClimbjumping")]
    public LeftCrouchedClimbjumping LeftCrouchedClimbjumpingSlider { get; set; } = LeftCrouchedClimbjumping.MapDefault;
    public enum LeftWalljumps { MapDefault, Off, On }
    [SettingSubText("MODOPTIONS_MANUALHELPER_LeftWalljumpsDesc")]
    [SettingName("MODOPTIONS_MANUALHELPER_LeftWalljumps")]
    public LeftWalljumps LeftWalljumpsSlider { get; set; } = LeftWalljumps.MapDefault;
    public enum LeftWallbounces { MapDefault, Off, On }
    [SettingSubText("MODOPTIONS_MANUALHELPER_LeftWallbouncesDesc")]
    [SettingName("MODOPTIONS_MANUALHELPER_LeftWallbounces")]
    public LeftWallbounces LeftWallbouncesSlider { get; set; } = LeftWallbounces.MapDefault;
    public enum RightClinging { MapDefault, Off, On }
    [SettingSubText("MODOPTIONS_MANUALHELPER_RightClingingDesc")]
    [SettingName("MODOPTIONS_MANUALHELPER_RightClinging")]
    
    public RightClinging RightClingingSlider { get; set; } = RightClinging.MapDefault;
    public enum RightUncrouchedClimbjumping { MapDefault, Off, On }
    [SettingSubText("MODOPTIONS_MANUALHELPER_RightUncrouchedClimbjumpingDesc")]
    [SettingName("MODOPTIONS_MANUALHELPER_RightUncrouchedClimbjumping")]
    public RightUncrouchedClimbjumping RightUncrouchedClimbjumpingSlider { get; set; } = RightUncrouchedClimbjumping.MapDefault;
    public enum RightCrouchedClimbjumping { MapDefault, Off, On }
    [SettingSubText("MODOPTIONS_MANUALHELPER_RightCrouchedClimbjumpingDesc")]
    [SettingName("MODOPTIONS_MANUALHELPER_RightCrouchedClimbjumping")]
    public RightCrouchedClimbjumping RightCrouchedClimbjumpingSlider { get; set; } = RightCrouchedClimbjumping.MapDefault;
    public enum RightWalljumps { MapDefault, Off, On }
    [SettingSubText("MODOPTIONS_MANUALHELPER_RightWalljumpsDesc")]
    [SettingName("MODOPTIONS_MANUALHELPER_RightWalljumps")]
    public RightWalljumps RightWalljumpsSlider { get; set; } = RightWalljumps.MapDefault;
    public enum RightWallbounces { MapDefault, Off, On }
    [SettingSubText("MODOPTIONS_MANUALHELPER_RightWallbouncesDesc")]
    [SettingName("MODOPTIONS_MANUALHELPER_RightWallbounces")]
    public RightWallbounces RightWallbouncesSlider { get; set; } = RightWallbounces.MapDefault;
    
    
    public enum HeartDoors { MapDefault, Off, On }
    [SettingSubHeader("MODOPTIONS_MANUALHELPER_VanillaEntityTogglesHeader")]
    [SettingSubText("MODOPTIONS_MANUALHELPER_HeartDoorsDesc")]
    [SettingName("MODOPTIONS_MANUALHELPER_HeartDoors")]
    public HeartDoors HeartDoorsSlider { get; set; } = HeartDoors.MapDefault;*/


    //[SettingSubHeader("MODOPTIONS_MANUALHELPER_MiscellaneousHeader")]
    //[SettingSubText("MODOPTIONS_MANUALHELPER_PauseMenuButtonEnabledDesc")]
    //[SettingName("MODOPTIONS_MANUALHELPER_PauseMenuButtonEnabled")]
    public bool PauseMenuButtonEnabled { get; set; } = false;
    [SettingSubText("MODOPTIONS_MANUALHELPER_OpenManualHelperMenuDesc")]
    [SettingName("MODOPTIONS_MANUALHELPER_OpenManualHelperMenu")]
    public ButtonBinding OpenManualHelperMenu { get; set; }
    //[SettingSubText("MODOPTIONS_MANUALHELPER_PauseOnDeathDesc")]
    //[SettingName("MODOPTIONS_MANUALHELPER_PauseOnDeath")]
    public bool PauseOnDeath { get; set; } = false;
}