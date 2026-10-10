using System;
using System.Collections.Generic;
using System.Linq;

namespace Touchfish {
public static class DeathrattleRules {
 static readonly Dictionary<string,Action<MatchEngine,BattleUnit>> effects=new Dictionary<string,Action<MatchEngine,BattleUnit>>{
  {"EX1_556",(g,u)=>SummonAtDeath(g,u,"VAN_skele21",1)},
  {"EX1_096",(g,u)=>g.Draw(u.Owner,1)},
  {"EX1_012",(g,u)=>g.Draw(u.Owner,1)},
  {"EX1_029",(g,u)=>g.Damage(new MatchTarget(1-u.Owner),2)},
  {"EX1_383",(g,u)=>g.EquipRaw(u.Owner,"GAME_ASHBRINGER",5,3,false)},
  {"EX1_110",(g,u)=>SummonAtDeath(g,u,"VAN_EX1_110t",1)},
  {"EX1_016",(g,u)=>g.StealRandomEnemyMinion(u.Owner)},
  {"EX1_097",(g,u)=>{foreach(var target in g.Characters(0).Concat(g.Characters(1)).ToArray())g.Damage(target,2);}},
  {"EX1_534",(g,u)=>SummonAtDeath(g,u,"GAME_HYENA",2)},
  {"EX1_577",(g,u)=>g.Summon(1-u.Owner,"VAN_EX1_finkle")}
 };
 public static string[] CardIds{get{return effects.Keys.ToArray();}}
 public static void Resolve(MatchEngine game,BattleUnit unit){Action<MatchEngine,BattleUnit> effect;if(!unit.Silenced&&effects.TryGetValue(game.Card(unit.CardId).BaseId,out effect)){game.Log.Add("玩家 "+(unit.Owner+1)+" · 亡语："+game.Card(unit.CardId).Name);effect(game,unit);}if(unit.ForestDeathrattles>0){game.Log.Add("玩家 "+(unit.Owner+1)+" · 丛林之魂亡语："+game.Card(unit.CardId).Name);SummonAtDeath(game,unit,"VAN_EX1_tk9",unit.ForestDeathrattles);}}
 static void SummonAtDeath(MatchEngine game,BattleUnit unit,string id,int count){var board=game.Players[unit.Owner].Board;int position=board.FindIndex(u=>u.Id==unit.DeathAnchor);if(position<0)position=board.Count;for(int i=0;i<count;i++){if(game.Summon(unit.Owner,id,position)==null)break;position++;}}
}
public sealed partial class MatchEngine {
 public void DestroyAllMinions(){foreach(var unit in Players.SelectMany(p=>p.Board).ToArray())unit.DamageTaken=unit.MaxHealth;}
}
}
