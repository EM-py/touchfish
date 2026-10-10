using System;
using System.Collections.Generic;
using System.Linq;

namespace Touchfish {
public static class HeroEmotes {
 static readonly string[] Categories={"开场白","感谢","问候","称赞","惊叹","失误","威胁","输了"};
 static readonly int[] MenuIndices={1,2,3,4,5,6};
 static readonly Dictionary<string,string[]> Lines=new Dictionary<string,string[]> {
  {"PALADIN",new[]{"荣耀将赐予我力量","谢谢你","很高兴见到你","打得不错","哇！","真是失误","接受正义的制裁吧","胜利属于你"}},
  {"MAGE",new[]{"知识就是力量。","谢谢你。","你好，旅人。","你的法术令人钦佩。","哇，真是令人惊讶！","哎呀，出了点差错。","感受奥术的力量！","这场胜利属于你。"}},
  {"WARRIOR",new[]{"战斗现在开始！","多谢。","准备好战斗了吗？","打得漂亮。","真厉害！","那可真是个失误。","来吧，面对我！","你赢得了这场战斗。"}},
  {"HUNTER",new[]{"猎物已经出现。","谢谢。","你好，朋友。","干得不错。","哇！","我走错了一步。","瞄准，射击！","这次你赢了。"}},
  {"ROGUE",new[]{"当心你的背后","欠你个人情。","幸会。","出手漂亮。","漂亮的一手！","计划有变。","你已经无路可逃。","算你技高一筹。"}},
  {"PRIEST",new[]{"光明指引着我们。","谢谢你。","愿你安好。","你的技艺值得赞许。","真令人惊叹。","我犯了个错误。","接受圣光的审判。","愿荣耀归于你。"}},
  {"WARLOCK",new[]{"黑暗在召唤。","这份恩情我记下了。","你好。","干得不错。","真是出乎意料。","出了些小差错。","见识一下真正的力量。","这场胜利归你。"}},
  {"SHAMAN",new[]{"元素已经回应。","感谢你的帮助。","愿元素与你同在。","漂亮的一击。","哇！","我失误了。","元素，听我号令！","你赢得了胜利。"}},
  {"DRUID",new[]{"自然的平衡将得到维护。","谢谢。","你好。","打得很好。","真是令人惊叹。","我犯了个错误。","自然会惩罚你。","这次胜利属于你。"}}
 };
 static readonly Dictionary<string,string> Names=new Dictionary<string,string>{{"PALADIN","乌瑟尔·光明骑士"},{"MAGE","吉安娜·普罗德摩尔"},{"WARRIOR","加尔鲁什·地狱咆哮"},{"HUNTER","雷克萨"},{"ROGUE","瓦莉拉·萨古纳尔"},{"PRIEST","安度因·乌瑞恩"},{"WARLOCK","古尔丹"},{"SHAMAN","萨尔"},{"DRUID","玛法里奥·怒风"}};
 public static string Name(string classId){string name;return Names.TryGetValue(classId??"",out name)?name:"英雄";}
 public static KeyValuePair<string,string>[] Options(string classId){string[] lines;if(!Lines.TryGetValue(classId??"",out lines))lines=Lines["MAGE"];var result=new KeyValuePair<string,string>[MenuIndices.Length];for(int i=0;i<result.Length;i++){int index=MenuIndices[i];result[i]=new KeyValuePair<string,string>(Categories[index],lines[index]);}return result;}
 public static string Opening(string classId){return Line(classId,0);}
 public static string Defeat(string classId){return Line(classId,7);}
 static string Line(string classId,int index){string[] lines;return Lines.TryGetValue(classId??"",out lines)?lines[index]:Lines["MAGE"][index];}
 public static bool Contains(string classId,string phrase){string[] lines;if(!Lines.TryGetValue(classId??"",out lines))lines=Lines["MAGE"];return lines.Contains(phrase);}
}
}
