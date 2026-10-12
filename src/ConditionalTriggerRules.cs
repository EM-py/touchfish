using System;
using System.Linq;

namespace Touchfish {
public enum MatchTriggerTiming { CardPlayed, SpellCast, TurnStart, MinionDamaged, TurnEnd, SourceDied }
public sealed class MatchTriggerContext {
 public MatchEngine Game;public int Seat,Amount;public CardRecord Card;public BattleUnit Source,Damaged;
}
// Conditions and effects are separate; new conditional cards register their timing here.
public sealed class ConditionalTrigger {
 public string CardId;public MatchTriggerTiming Timing;
 public Func<MatchTriggerContext,bool> Condition;public Action<MatchTriggerContext> Apply;
}
public static class ConditionalTriggerRules {
 static readonly ConditionalTrigger[] rules={
  new ConditionalTrigger{CardId="EX1_402",Timing=MatchTriggerTiming.MinionDamaged,Condition=c=>c.Source.Owner==c.Damaged.Owner,Apply=c=>c.Game.Players[c.Source.Owner].Armor++},
  new ConditionalTrigger{CardId="EX1_604",Timing=MatchTriggerTiming.MinionDamaged,Condition=c=>true,Apply=c=>c.Source.BuffAttack++},
  new ConditionalTrigger{CardId="EX1_044",Timing=MatchTriggerTiming.CardPlayed,Condition=c=>c.Source.Owner==c.Seat,Apply=c=>c.Game.Buff(new MatchTarget(c.Source.Owner,c.Source.Id),1,1)},
  new ConditionalTrigger{CardId="EX1_100",Timing=MatchTriggerTiming.SpellCast,Condition=c=>c.Card!=null&&c.Card.Type=="SPELL",Apply=c=>c.Game.CopySpellToHand(1-c.Seat,c.Card)},
  new ConditionalTrigger{CardId="EX1_557",Timing=MatchTriggerTiming.TurnStart,Condition=c=>c.Source.Owner==c.Seat,Apply=c=>{if(c.Game.TriggerCoinFlip())c.Game.FishingDraw(c.Source);}},
  new ConditionalTrigger{CardId="NEW1_021",Timing=MatchTriggerTiming.TurnStart,Condition=c=>c.Source.Owner==c.Seat,Apply=c=>c.Game.DestroyAllMinions()}
 };
 public static void MinionDamaged(MatchEngine game,BattleUnit damaged,int amount,BattleUnit[] listeners){foreach(var source in listeners){if(source.Silenced||!game.Players[source.Owner].Board.Contains(source))continue;foreach(var rule in game.Rules.Triggers.Where(r=>r.Timing==MatchTriggerTiming.MinionDamaged&&r.CardId==MatchRules.EffectKey(game.Card(source.CardId)))){var context=new MatchTriggerContext{Game=game,Seat=damaged.Owner,Source=source,Damaged=damaged,Amount=amount};if(rule.Condition(context))rule.Apply(context);}}}
 public static ConditionalTrigger[] Definitions{get{return rules.Concat(TurnEndRules.TriggerIds.Select(id=>new ConditionalTrigger{CardId=id,Timing=MatchTriggerTiming.TurnEnd,Condition=c=>(c.Source.Owner==c.Seat||id=="NEW1_038"||id=="EX1_tk9b")&&(id!="EX1_274"||c.Game.Players[c.Source.Owner].SecretCount>0),Apply=c=>c.Game.ApplyTurnEndTrigger(c.Source)})).Concat(DeathrattleRules.CardIds.Select(id=>new ConditionalTrigger{CardId=id,Timing=MatchTriggerTiming.SourceDied,Condition=c=>true,Apply=c=>DeathrattleRules.Resolve(c.Game,c.Source)})).ToArray();}}
 public static void SourceDied(MatchEngine game,BattleUnit source){bool native=DeathrattleRules.CardIds.Contains(game.Card(source.CardId).BaseId);foreach(var rule in game.Rules.Triggers.Where(r=>r.Timing==MatchTriggerTiming.SourceDied&&r.CardId==MatchRules.EffectKey(game.Card(source.CardId)))){var context=new MatchTriggerContext{Game=game,Seat=source.Owner,Source=source};if(rule.Condition(context))rule.Apply(context);}if(!native&&source.ForestDeathrattles>0)DeathrattleRules.Resolve(game,source);}
 public static string[] CardIds{get{return rules.Select(r=>r.CardId).Distinct().ToArray();}}
 public static void Resolve(MatchEngine game,MatchTriggerTiming timing,int seat,CardRecord card=null){
  var queue=(timing==MatchTriggerTiming.TurnEnd?game.Players[seat].Board.OrderBy(u=>u.Id).Concat(game.Players[1-seat].Board.OrderBy(u=>u.Id)):game.Players.SelectMany(p=>p.Board).OrderBy(u=>u.Id)).ToArray();
  foreach(var source in queue){if(game.Finished)break;if(source.Silenced||source.Health<=0||!game.Players[source.Owner].Board.Contains(source))continue;
   foreach(var rule in game.Rules.Triggers.Where(r=>r.Timing==timing&&r.CardId==MatchRules.EffectKey(game.Card(source.CardId)))){
    var context=new MatchTriggerContext{Game=game,Seat=seat,Card=card,Source=source};if(!rule.Condition(context))continue;
    game.Log.Add("玩家 "+(source.Owner+1)+" · 触发："+game.Card(source.CardId).Name);rule.Apply(context);game.Cleanup();if(game.Finished)break;
   }
  }
 }
}
public static class TurnStartRules {
 public static void Resolve(MatchEngine game,int seat){ConditionalTriggerRules.Resolve(game,MatchTriggerTiming.TurnStart,seat);}
}
public static class TimedCostRules {
 public static void GrantFreeSpells(MatchEngine game,int seat,int throughTurn){game.Players[seat].FreeSpellsUntilTurn=Math.Max(game.Players[seat].FreeSpellsUntilTurn,throughTurn);}
 public static int Apply(MatchEngine game,int seat,CardRecord card,int cost){if(card.Type=="SPELL")cost=Math.Max(0,cost-game.Players[seat].PreparationDiscount);return card.Type=="SPELL"&&game.Players[seat].FreeSpellsUntilTurn>=game.Turn?0:cost;}
 public static void Expire(MatchEngine game){foreach(var player in game.Players){player.PreparationDiscount=0;if(player.FreeSpellsUntilTurn<=game.Turn)player.FreeSpellsUntilTurn=-1;}}
}
public sealed partial class MatchEngine {
 internal bool TriggerCoinFlip(){return random.Next(2)==0;}
 public void CopySpellToHand(int seat,CardRecord card){var player=Players[seat];if(player.Hand.Count>=Mode.HandLimit){Log.Add("玩家 "+(seat+1)+" · 手牌已满，未获得法术复制。");return;}player.Hand.Add(new HandCard{Id=nextId++,CardId=card.Id,Generated=true});Log.Add("玩家 "+(seat+1)+" · 获得法术复制 1 张。");}
}
}
