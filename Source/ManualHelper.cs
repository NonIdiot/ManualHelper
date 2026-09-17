using System;
using System.Reflection;
using Microsoft.Xna.Framework;
using MonoMod.RuntimeDetour;

namespace Celeste.Mod.ManualHelper;

public class ManualHelper : EverestModule {
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
    private static bool isRenderingCode = false;
    public override void Load() {
        // apply any hooks that should always be active
        dashAttackingHook = new Hook(
            typeof(Player).GetMethod("get_DashAttacking"),
            typeof(ManualHelper).GetMethod("weirdHookCelestePlayerDashAttacking", BindingFlags.NonPublic | BindingFlags.Static)
        );
        On.Celeste.Player.ClimbCheck += OnCelestePlayerClimbCheck;
        On.Celeste.Player.ClimbJump += OnCelestePlayerClimbJump;
        On.Celeste.Player.WallJump += OnCelestePlayerWallJump;
        On.Celeste.Player.SuperWallJump += OnCelestePlayerSuperWallJump;
        On.Celeste.Player.NormalUpdate += OnCelestePlayerNormalUpdate;
        On.Celeste.Player.UpdateSprite += OnCelestePlayerUpdateSprite;
    }

    public override void Unload() {
        // unapply any hooks applied in Load()
        dashAttackingHook.Dispose();
        dashAttackingHook = null;
        On.Celeste.Player.ClimbCheck -= OnCelestePlayerClimbCheck;
        On.Celeste.Player.ClimbJump -= OnCelestePlayerClimbJump;
        On.Celeste.Player.WallJump -= OnCelestePlayerWallJump;
        On.Celeste.Player.SuperWallJump -= OnCelestePlayerSuperWallJump;
        On.Celeste.Player.NormalUpdate -= OnCelestePlayerNormalUpdate;
        On.Celeste.Player.UpdateSprite -= OnCelestePlayerUpdateSprite;
    }
    
    // tools
    public static int GetOptionData(string whichOne)
    {
        // returns 0 if Map Default, 1 if Off, and 2 if On
        if (whichOne == "LeftClinging")
        {
            return Settings.LeftClingingSlider == ManualHelperModuleSettings.LeftClinging.MapDefault ? 0 : 
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
        
        
        if (whichOne == "UsableDashAttack")
        {
            return Settings.UsableDashAttackSlider == ManualHelperModuleSettings.UsableDashAttack.MapDefault ? 0 : 
                (Settings.UsableDashAttackSlider == ManualHelperModuleSettings.UsableDashAttack.Off ? 1 : 2);
        }
        // but like, return -1 if the option is not found
        Logger.Log(nameof(ManualHelper),"[Error NonIdiot000] Hey twin, option "+whichOne+" isn't a valid ManualHelper Option. Seems like a skill issue.");
        return -1;
    }

    public static bool CanLRInteract(Player self, bool dir, string name)
    {
        return dir ? ReturnFromBoolToggle(self, "Left"+name) : ReturnFromBoolToggle(self, "Right"+name);
    }

    public static bool ReturnFromBoolToggle(Player self, string whichOne)
    {
        int myOption = GetOptionData(whichOne);
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

    // hooks
    
    // for preventing climbing stuff
    private static bool OnCelestePlayerClimbCheck(On.Celeste.Player.orig_ClimbCheck orig, Player self, int dir, int yAdd)
    {
        if (((!ReturnFromBoolToggle(self,"LeftClinging") && dir == -1) || (!ReturnFromBoolToggle(self,"RightClinging") && dir == 1)) && !self.level.InCredits)
        {
            return false;
        }

        return orig(self, dir, yAdd);
    }
    
    // for preventing climbjump stuff
    private static void OnCelestePlayerClimbJump(On.Celeste.Player.orig_ClimbJump orig, Player self)
    {
        if ((self.Facing == Facings.Left ?
                (self.Ducking ? ReturnFromBoolToggle(self,"LeftCrouchedClimbjumping") : 
                    ReturnFromBoolToggle(self,"LeftUncrouchedClimbjumping")) : 
                (self.Ducking ? ReturnFromBoolToggle(self,"RightCrouchedClimbjumping") : 
                    ReturnFromBoolToggle(self,"RightUncrouchedClimbjumping"))) 
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
    private static bool weirdHookCelestePlayerDashAttacking(Func<Player, bool> orig, Player self) {
        if (!isRenderingCode && !ReturnFromBoolToggle(self, "UsableDashAttack")) {
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
}