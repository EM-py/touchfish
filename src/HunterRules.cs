using System;
using System.Linq;
using System.Collections.Generic;

namespace Touchfish {
public static class HunterRules {
 public const string TrackingCardId="DS1_184";
 public static string[] TrackingPreview(MatchEngine game,int seat){
  if(game==null||seat<0||seat>1)return new string[0];var deck=game.Players[seat].Deck;
  if(deck.Count>0&&deck[0]!="HIDDEN")return deck.Take(3).ToArray();
  return game.PrivateDeckPreview[seat]??new string[0];
 }
 public static MatchChoiceOption[] TrackingOptions(MatchEngine game,int seat){return TrackingPreview(game,seat).Select((id,index)=>new MatchChoiceOption("track:"+index,game.Card(id).Name,game.Card(id).Cost+"费 · "+game.Card(id).Text)).ToArray();}
 public static bool IsTrackingChoiceValid(MatchEngine game,int seat,string choice){int index=TrackingIndex(choice);return index>=0&&index<TrackingPreview(game,seat).Length;}
 static int TrackingIndex(string choice){if(String.IsNullOrEmpty(choice)||!choice.StartsWith("track:"))return -1;int index;return Int32.TryParse(choice.Substring(6),out index)?index:-1;}
 public static string Requirement(MatchEngine game,int seat,string cardId){if(cardId=="DS1_183"&&game.Players[1-seat].Board.Count<2)return "对方至少需要两个随从。";return null;}
 public static void Apply(EffectContext c){switch(c.Card.BaseId){
  case "DS1_183":MultiShot(c);break;
  case "DS1_184":Track(c);break;
  case "DS1_188":break;
  case "DS1_070":if(c.Target!=null){var unit=c.Game.Unit(c.Target);if(unit!=null){c.Game.Buff(c.Target,2,2);unit.Taunt=true;}}break;
  case "EX1_531":case "CS2_237":break;
  case "EX1_536":break;
  case "EX1_537":ExplosiveShot(c);break;
  case "EX1_538":Unleash(c);break;
  case "EX1_544":Flare(c);break;
  case "EX1_549":if(c.Target!=null){var beast=c.Game.Unit(c.Target);if(beast!=null){c.Game.Buff(c.Target,2,0);beast.ImmuneUntilTurn=c.Game.Turn;}}break;
 }}
 static void Track(EffectContext c){var p=c.Game.Players[c.Seat];var preview=TrackingPreview(c.Game,c.Seat);int chosen=TrackingIndex(c.Choice);if(preview.Length==0)return;if(chosen<0)chosen=0;if(chosen>=preview.Length)return;
  for(int i=preview.Length-1;i>=0;i--)if(i!=chosen){string discarded=p.Deck[i];p.Deck.RemoveAt(i);c.Game.RecordGraveyard(c.Seat,c.Seat,discarded,c.Game.NewEffectInstanceId(),GraveyardReason.Discarded);}
  if(p.Deck.Count==0)return;string selected=p.Deck[0];p.Deck.RemoveAt(0);if(p.Hand.Count<10)p.Hand.Add(new HandCard{Id=c.Game.NewEffectInstanceId(),CardId=selected});else c.Game.RecordGraveyard(c.Seat,c.Seat,selected,c.Game.NewEffectInstanceId(),GraveyardReason.Burned);
 }
 static void MultiShot(EffectContext c){var targets=c.Game.Players[1-c.Seat].Board.ToList();if(targets.Count<2)return;for(int i=0;i<2&&targets.Count>0;i++){var target=c.Game.RandomTarget(targets.Select(u=>new MatchTarget(1-c.Seat,u.Id)));var unit=c.Game.Unit(target);targets.Remove(unit);c.Game.Damage(target,3+c.Game.SpellPower(c.Seat));}}
 static void ExplosiveShot(EffectContext c){var target=c.Game.Unit(c.Target);if(target==null)return;var board=c.Game.Players[target.Owner].Board;int index=board.IndexOf(target);var adjacent=new[]{index>0?board[index-1]:null,index+1<board.Count?board[index+1]:null}.Where(u=>u!=null).ToArray();int power=c.Game.SpellPower(c.Seat);c.Game.Damage(c.Target,5+power);foreach(var unit in adjacent)c.Game.Damage(new MatchTarget(unit.Owner,unit.Id),2+power);}
 static void Unleash(EffectContext c){int count=c.Game.Players[1-c.Seat].Board.Count;for(int i=0;i<count&&c.Game.Players[c.Seat].Board.Count<7;i++)c.Game.Summon(c.Seat,"GAME_HOUND");}
 static void Flare(EffectContext c){foreach(var unit in c.Game.Players.SelectMany(p=>p.Board))unit.Stealth=false;SecretRules.DestroyAll(c.Game,1-c.Seat);c.Game.Draw(c.Seat,1);}
 public static void MinionSummoned(MatchEngine game,BattleUnit unit){if(unit==null||unit.Race!="BEAST")return;foreach(var buzzard in game.Players[unit.Owner].Board.Where(u=>!u.Silenced&&game.Card(u.CardId).BaseId=="CS2_237").ToArray())game.Draw(buzzard.Owner,1);}
 public static void MinionDied(MatchEngine game,BattleUnit dead){if(dead==null||dead.Race!="BEAST")return;foreach(var hyena in game.Players[dead.Owner].Board.Where(u=>!u.Silenced&&game.Card(u.CardId).BaseId=="EX1_531").ToArray())game.Buff(new MatchTarget(hyena.Owner,hyena.Id),2,1);}
 public static void SecretRevealed(MatchEngine game,int seat){var p=game.Players[seat];if(p.WeaponId!=null&&p.WeaponDurability>0&&game.Card(p.WeaponId).BaseId=="EX1_536")p.WeaponDurability++;}
}
}
