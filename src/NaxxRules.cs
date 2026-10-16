using System;
using System.Linq;

namespace Touchfish {
// August 2014 Naxx effects use the same summon, secret and death queues as Classic.
public static class NaxxRules {
 public static readonly string[] CardIds={"FP1_020","FP1_011","FP1_025","FP1_018","FP1_023","FP1_019","FP1_021","FP1_022","FP1_026","FP1_004","FP1_030","FP1_009"};
 // Webspinner's full era beast pool also needs Haunted Creeper and native Poisonous on Maexxna.
 public static void Register(MatchRules rules){foreach(string id in CardIds)rules.Register(id,"NONE",c=>{});rules.Register("FP1_010","NONE",c=>{});rules.Register("FP1_020","NONE",SecretRules.Apply,(g,s)=>g.SecretRequirement(s,g.Card("FP1_020")));rules.Register("FP1_018","NONE",SecretRules.Apply,(g,s)=>g.SecretRequirement(s,g.Card("FP1_018")));rules.Register("FP1_025","MINION",Reincarnate);rules.Register("FP1_019","NONE",PoisonSeeds);rules.Register("FP1_030","NONE",c=>{var p=c.Game.Players[1-c.Seat];if(p.SpellCostPenaltyUntilTurn<c.Game.Turn)p.SpellCostPenalty=0;p.SpellCostPenalty+=5;p.SpellCostPenaltyUntilTurn=c.Game.Turn+1;});}
 static void Reincarnate(EffectContext c){var g=c.Game;var unit=g.LivingUnit(c.Target);if(unit==null)return;int seat=unit.Owner,position=g.Players[seat].Board.IndexOf(unit);string id=unit.CardId;unit.DamageTaken=unit.MaxHealth;g.Cleanup();g.Summon(seat,id,position);}
 static void PoisonSeeds(EffectContext c){var g=c.Game;var units=g.Players.SelectMany(p=>p.Board).Where(u=>u.Health>0).ToArray();var counts=new[]{units.Count(u=>u.Owner==0),units.Count(u=>u.Owner==1)};foreach(var unit in units)unit.DamageTaken=unit.MaxHealth;g.Cleanup();for(int seat=0;seat<2;seat++)for(int n=0;n<counts[seat]&&g.Players[seat].Board.Count<g.Mode.BoardSlots;n++)g.Summon(seat,"VAN_EX1_tk9");}
 public static void Death(MatchEngine g,BattleUnit unit){int seat=unit.Owner;switch(g.Card(unit.CardId).BaseId){
  case "FP1_011":var pool=g.Catalog.Pool("naxxramas-2014");var beasts=g.Catalog.Deckable(pool).Where(c=>c.Race=="BEAST"&&c.Type=="MINION").ToArray();if(beasts.Length>0)g.GiveNaxxCard(seat,beasts[g.RandomIndex(beasts.Length)].Id);break;
  case "FP1_023":var ally=g.RandomTarget(g.Characters(seat,false));if(ally!=null)g.Buff(ally,0,3);break;
  case "FP1_022":var hand=g.Players[seat].Hand;if(g.Players[seat].Board.Count>=g.Mode.BoardSlots)break;var demons=hand.Where(h=>g.Card(h.CardId).Type=="MINION"&&g.Card(h.CardId).Race=="DEMON").ToArray();if(demons.Length==0)break;var demon=demons[g.RandomIndex(demons.Length)];hand.Remove(demon);g.Summon(seat,demon.CardId,DeathPosition(g,unit));break;
  case "FP1_026":var target=g.RandomTarget(g.Characters(seat,false));if(target!=null)g.ReturnToHand(target);break;
  case "FP1_004":g.PutSecretFromDeck(seat);break;
  case "FP1_009":g.SummonMinionFromDeck(1-seat);break;
 }}
 static int DeathPosition(MatchEngine g,BattleUnit unit){var board=g.Players[unit.Owner].Board;int index=board.FindIndex(u=>u.Id==unit.DeathAnchor);return index<0?board.Count:index;}
 public static void WeaponRemoved(MatchEngine g,int seat,string cardId){if(cardId==null||g.Card(cardId).BaseId!="FP1_021")return;g.Log.Add(g.PlayerLabel(seat)+" · 武器亡语：死亡之咬");g.DamageBatch(()=>{foreach(var target in g.Characters(0,false).Concat(g.Characters(1,false)).ToArray())g.Damage(target,1);});g.Cleanup();}
}
public sealed partial class MatchEngine {
 public void GiveNaxxCard(int seat,string cardId){var p=Players[seat];if(p.Hand.Count>=Mode.HandLimit){RecordGraveyard(seat,seat,cardId,nextId++,GraveyardReason.Burned);Log.Add(PlayerLabel(seat)+" · 手牌已满，未获得生成牌。");return;}p.Hand.Add(new HandCard{Id=nextId++,CardId=cardId,Generated=true});Log.Add(PlayerLabel(seat)+" · 获得生成牌 1 张。");}
 public void SummonMinionFromDeck(int seat){var p=Players[seat];if(p.Board.Count>=Mode.BoardSlots)return;var indices=p.Deck.Select((id,index)=>new{Id=id,Index=index}).Where(c=>Card(c.Id).Type=="MINION").ToArray();if(indices.Length==0)return;var chosen=indices[random.Next(indices.Length)];p.Deck.RemoveAt(chosen.Index);Summon(seat,chosen.Id);Log.Add(PlayerLabel(seat)+" · 从牌库召唤："+Card(chosen.Id).Name);}
 public void PutSecretFromDeck(int seat){var p=Players[seat];if(p.SecretCount>=5)return;var indices=p.Deck.Select((id,index)=>new{Id=id,Index=index}).Where(c=>Has(Card(c.Id),"SECRET")&&Rules.Supports(Card(c.Id))&&SecretRequirement(seat,Card(c.Id))==null).ToArray();if(indices.Length==0)return;var chosen=indices[random.Next(indices.Length)];p.Deck.RemoveAt(chosen.Index);ArmSecret(seat,new HandCard{Id=nextId++,CardId=chosen.Id});Log.Add(PlayerLabel(seat)+" · 从牌库置入一张奥秘");}
}
}
