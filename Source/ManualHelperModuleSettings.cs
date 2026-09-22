using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using YamlDotNet.Serialization;

namespace Celeste.Mod.ManualHelper;

public class ManualHelperModuleSettings : EverestModuleSettings {
    
    // settings
    
    // TODO: fix this not actually saving due to the new custom Grandmaster settings
    public Dictionary<string, int[]> DynamicSettings { get; set; } = new();
    public bool DynamicSettingsSet = false;

    public void InitializeDynamicSettings()
    {
        if (!ManualHelper.Settings.DynamicSettingsSet)
        {
            ManualHelper.Settings.DynamicSettingsSet = true;
            
            ManualHelper.Settings.DynamicSettings = new Dictionary<string, int[]>();
            // for the buttons at the top of each subsection
            for (int a = 1; a <= 3; a++)
            {
                // for buttons, the <current value> is instead what it should set non-buttons to.
                ManualHelper.Settings.DynamicSettings.Add("AllMapDefault_"+a,[0,a,2]);
                ManualHelper.Settings.DynamicSettings.Add("AllOff_"+a,[1,a,2]);
                ManualHelper.Settings.DynamicSettings.Add("AllOn_"+a,[2,a,2]);
                // logging
                ManualHelper.Settings.DynamicSettings.TryGetValue("AllOn_" + a, out int[] valuu);
                Logger.Log(LogLevel.Info,"ManualHelper_Dummy","a"+valuu[0]+" "+valuu[1]+" "+valuu.Length+" "+valuu);
            }

            string[] myList1 = ManualHelper.ManualHelperTogglesGrabs;
            foreach (string item1 in myList1)
            {
                //if (item1 != "LeftClinging")
                //{
                    // format is <current value>, <submenu it should go in>, <type of setting>.
                    // <current value> is default 0, and set to a new value when changed in Slider form.
                    // <submenu it should go in> is self-explanatory and easy to figure out.
                    // <type of setting> is 0 if MapSetting/Off/On, 1 if MapSetting/0/, 2 if button,
                    ManualHelper.Settings.DynamicSettings.Add(item1,[0,1,0]);
                //}
            }
            string[] myList2 = ManualHelper.ManualHelperTogglesDashes;
            foreach (string item2 in myList2)
            {
                ManualHelper.Settings.DynamicSettings.Add(item2,[0,2,0]);
            }
            string[] myList3 = ManualHelper.ManualHelperTogglesVanillaEntities;
            foreach (string item3 in myList3)
            {
                ManualHelper.Settings.DynamicSettings.Add(item3,[0,3,0]);
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

        public static Dictionary<string, TextMenu.Item> DynamicSettingItems = new();

        Func<int, string> MapDefaultOrElse = (i) =>
        {
            return
                i == 0 ? Dialog.Clean("MODOPTIONS_MANUALHELPER_MapDefault") : (i == 1 ? Dialog.Clean("MODOPTIONS_MANUALHELPER_Off") : Dialog.Clean("MODOPTIONS_MANUALHELPER_On"));
        };

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
                        // be sure that once more-than-2 options are added, that they are accounted for here too
                        ((TextMenu.Slider)myItem).Index = TurnAllVarResulte;
                        ((TextMenu.Slider)myItem).OnValueChange(((TextMenu.Slider)myItem).Values[((TextMenu.Slider)myItem).Index].Item2);
                    }
                }
            }
            TurnAllVarSection = -1;
            TurnAllVarResulte = -1;
        }

        public Action CreateAction(int varSection, int varResulte)
        {
            return () =>
            {
                TurnAllVarSection = varSection;
                TurnAllVarResulte = varResulte;
                TurnAllOfSectionInto();
            };
        }

        public Color returnColorFromInt(int myInt, bool isDisabled, string settingName)
        {
            if (myInt < 0 || myInt > 2)
            {
                // error color.
                return ManualHelper.ManualHelperAllMenuColors[5];
            }
            bool changedByPlayer = myInt != 0;
            // be sure to change this when non-bool toggles are added! 
            bool changedByMap = ManualHelper.GetFlag("ManualHelper/ToggleBool_"+settingName) == 0;
            return isDisabled ? ManualHelper.ManualHelperAllMenuColors[4] : (changedByMap ? (changedByPlayer ? ManualHelper.ManualHelperAllMenuColors[2] : ManualHelper.ManualHelperAllMenuColors[1]) : (changedByPlayer ? ManualHelper.ManualHelperAllMenuColors[0] : ManualHelper.ManualHelperAllMenuColors[3]));
            //return isDisabled ? Color.DarkSlateGray: (myInt == 0 ? (Color.Red) : (myInt == 1 ? Color.Goldenrod : (myInt == 2 ? Color.Goldenrod : Color.Purple)));
        }

        public void CreateDummy1Entry(TextMenuExt.SubMenu menu, bool inGame)
        {
            int myInt = menu.Label == Dialog.Clean("MODOPTIONS_MANUALHELPER_WallTogglesHeader") ? 1 : (menu.Label == Dialog.Clean("MODOPTIONS_MANUALHELPER_DashTogglesHeader") ? 2 : (menu.Label == Dialog.Clean("MODOPTIONS_MANUALHELPER_VanillaEntityTogglesHeader") ? 3 : 4));
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
                        switch (settingValue[2])
                        {
                            case 0:
                            {
                                myItem = new TextMenu.Slider(
                                    label: "  "+Dialog.Clean("MODOPTIONS_MANUALHELPER_" + settingName),
                                    values: MapDefaultOrElse,
                                    min: 0,
                                    max: 2,
                                    value: settingValue[0]
                                );
                                ((TextMenu.Slider)myItem).Change(newValue =>
                                {
                                    dynamicSettings[settingName][0] = newValue;
                                    ((TextMenu.Slider)myItem).UnselectedColor = returnColorFromInt(newValue,myItem.Disabled,settingName);
                                });
                                ((TextMenu.Slider)myItem).UnselectedColor = returnColorFromInt(settingValue[0],myItem.Disabled,settingName);
                                
                                isValid = true;
                                break;
                            }
                            case 1:
                            {
                                break;
                            }
                            case 2:
                            {
                                myItem = new TextMenu.Button(
                                    label: Dialog.Clean("MODOPTIONS_MANUALHELPER_" + settingName.Remove(settingName.Length-2))
                                );
                                // INCREDIBLY scuffed method of getting this to work how i want. dont do this at home
                                ((TextMenu.Button)myItem).Pressed(CreateAction(myInt, settingValue[0]));
                                
                                isValid = true;
                                break;
                            }
                            default:
                            {
                                break;
                            }
                        }
                    }

                    if (isValid)
                    {
                        if (Dialog.Has("MODOPTIONS_MANUALHELPER_" + settingName + "Desc", Dialog.Language))
                        {
                            myItem.AddDescription(menu, menu.Container, Dialog.Clean("MODOPTIONS_MANUALHELPER_" + settingName + "Desc"));
                        }

                        DynamicSettingItems[settingName] = myItem;
                        menu.Add(myItem);
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
    [SettingSubText("MODOPTIONS_MANUALHELPER_PauseMenuButtonEnabledDesc")]
    [SettingName("MODOPTIONS_MANUALHELPER_PauseMenuButtonEnabled")]
    public bool PauseMenuButtonEnabled { get; set; } = false;
    [SettingSubText("MODOPTIONS_MANUALHELPER_OpenManualHelperMenuDesc")]
    [SettingName("MODOPTIONS_MANUALHELPER_OpenManualHelperMenu")]
    public ButtonBinding OpenManualHelperMenu { get; set; }
}