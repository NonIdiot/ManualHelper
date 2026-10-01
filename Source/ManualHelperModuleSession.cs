using System.Collections.Generic;

namespace Celeste.Mod.ManualHelper;

public class ManualHelperModuleSession : EverestModuleSession {
    // flags/toggles that havent been set to true yet this session. useful for if someone save/quits a map,
    // updates the mod (or adds/updates a mod that modifies this mod), and then continues.
    public HashSet<string> flagsAlreadySet = new HashSet<string>();
    public HashSet<string> countersAlreadySet = new HashSet<string>();
    
    // for preventing SO much log spam. hopefully.
    public Dictionary<string,int> warnedCooldown = new Dictionary<string,int>();
}