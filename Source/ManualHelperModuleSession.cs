using System.Collections.Generic;

namespace Celeste.Mod.ManualHelper;

public class ManualHelperModuleSession : EverestModuleSession {
    // flags that havent been set to true yet this session. useful for if someone save/quits a map,
    // updates the mod, and then continues.
    public HashSet<string> flagsAlreadySet = new HashSet<string>();
    public HashSet<string> countersAlreadySet = new HashSet<string>();
}