using System.Collections.Generic;

namespace Touchfish {
public static class MinionSpeechRules {
 static readonly Dictionary<string,string> entrances=new Dictionary<string,string>{
  {"EX1_557","我能在河边钓上一整天的鱼。"},
  {"NEW1_029","你等着，等我有十点法力值的时候！"},
  {"EX1_100","愿青龙指引你。"}
 };
 public static void Entrance(MatchEngine game,BattleUnit source){string text;if(entrances.TryGetValue(game.Card(source.CardId).BaseId,out text))game.EmitVisual("minion-speech",source.Owner,source.Id,name:text);}
}
public sealed partial class MatchEngine {
 internal void FishingDraw(BattleUnit source){bool hasCard=Players[source.Owner].Deck.Count>0;Draw(source.Owner,1);if(hasCard)EmitVisual("fishing",source.Owner,source.Id,name:"上钩了！");}
}
}
