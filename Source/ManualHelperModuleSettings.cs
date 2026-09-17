namespace Celeste.Mod.ManualHelper;

public class ManualHelperModuleSettings : EverestModuleSettings {
    
    // settings
    public enum LeftClinging { MapDefault, Off, On }

    [SettingSubText("MODOPTIONS_MANUALHELPER_LeftClingingDesc")]
    [SettingName("MODOPTIONS_MANUALHELPER_LeftClinging")]
    public LeftClinging LeftClingingSlider { get; set; } = LeftClinging.MapDefault;
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
    
    
    public enum UsableDashAttack { MapDefault, Off, On }
    [SettingSubText("MODOPTIONS_MANUALHELPER_UsableDashAttackDesc")]
    [SettingName("MODOPTIONS_MANUALHELPER_UsableDashAttack")]
    public UsableDashAttack UsableDashAttackSlider { get; set; } = UsableDashAttack.MapDefault;
}