using System;
using System.Collections.Generic;
namespace Touchfish {
public interface ICardEffectModule {string Id{get;}void Register(MatchRules rules);}
public static class RuleModules {
 static readonly Dictionary<string,ICardEffectModule> modules=new Dictionary<string,ICardEffectModule>();
 public static void Register(ICardEffectModule module){if(module==null||String.IsNullOrWhiteSpace(module.Id)||modules.ContainsKey(module.Id))throw new ArgumentException("效果模块 ID 无效或重复。");modules.Add(module.Id,module);}
 internal static void Configure(MatchRules rules){foreach(var module in modules.Values)module.Register(rules);}
}
}
