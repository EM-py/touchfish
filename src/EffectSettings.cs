using System;
using System.IO;
namespace Touchfish {
public enum EffectLevel {Off=0,Low=1,High=2}
public static class EffectSettings {
 public static EffectLevel FromWire(int level){if(level<0||level>2)throw new InvalidDataException("特效等级无效。");return (EffectLevel)level;}
 public static EffectLevel ParsePreferences(string text){foreach(string line in (text??"").Split('\n')){string value=line.Trim();if(!value.StartsWith("effects=",StringComparison.OrdinalIgnoreCase))continue;value=value.Substring(8).Trim().ToLowerInvariant();return value=="high"||value=="all"?EffectLevel.High:value=="low"?EffectLevel.Low:EffectLevel.Off;}return EffectLevel.Off;}
 public static string PreferenceValue(EffectLevel level){FromWire((int)level);return level==EffectLevel.High?"high":level==EffectLevel.Low?"low":"off";}
}
}
