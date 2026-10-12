using System;
using System.Linq;

namespace Touchfish {
public static class SecretRules {
 public static readonly string[] CardIds={"EX1_130","EX1_132","EX1_136","EX1_379","EX1_287","EX1_289","EX1_294","EX1_295","EX1_594","tt_010","EX1_533","EX1_554","EX1_609","EX1_610","EX1_611"};
 public static bool IsSecret(CardRecord card){return card!=null&&card.Mechanics!=null&&card.Mechanics.Contains("SECRET");}
 public static string Requirement(MatchEngine game,int seat,CardRecord card){if(!IsSecret(card))return null;var p=game.Players[seat];if(p.ActiveSecrets.Contains(card.BaseId))return "己方已经有这个奥秘。";if(p.ActiveSecrets.Count>=5)return "奥秘栏已满（最多五个）。";return null;}
 static bool Has(MatchPlayer p,string id){return p.ActiveSecrets.Contains(id);}
 static void Reveal(MatchEngine g,int seat,string id){g.Players[seat].ActiveSecrets.Remove(id);g.Players[seat].SecretCount=g.Players[seat].ActiveSecrets.Count;g.Log.Add("玩家 "+(seat+1)+" · 奥秘触发："+g.Card("VAN_"+id).Name);HunterRules.SecretRevealed(g,seat);}
 public static void DestroyAll(MatchEngine g,int seat){var p=g.Players[seat];if(p.ActiveSecrets.Count==0)return;p.ActiveSecrets.Clear();p.SecretCount=0;g.Log.Add("玩家 "+(seat+1)+" · 敌方奥秘被照明弹摧毁");}
 public static bool ResolveSpell(MatchEngine g,int caster,CardRecord card,ref MatchTarget target){var owner=g.Players[1-caster];foreach(string id in owner.ActiveSecrets.ToArray()){
  if(id=="EX1_287"&&card.Type=="SPELL"){Reveal(g,1-caster,id);g.Log.Add("法术被法术反制抵消。");return true;}
  if(id=="tt_010"&&!IsSecret(card)&&card.Type=="SPELL"&&target!=null&&!target.Hero&&g.Unit(target)!=null){if(owner.Board.Count>=7)continue;Reveal(g,1-caster,id);var ward=g.Summon(1-caster,"VAN_tt_010a");target=new MatchTarget(1-caster,ward.Id);g.Log.Add("扰咒术将法术目标转移到守护者。");}
 }
 return false;
 }
 public static void MinionPlayed(MatchEngine g,int caster,BattleUnit minion){var owner=g.Players[1-caster];foreach(string id in owner.ActiveSecrets.ToArray()){
  if(id=="EX1_294"&&owner.Board.Count<7){Reveal(g,1-caster,id);var copy=g.Summon(1-caster,minion.CardId);copy.DamageTaken=0;g.Log.Add("镜像实体召唤了一个复制。");}
  else if(id=="EX1_379"){Reveal(g,1-caster,id);g.SetHealth(new MatchTarget(caster,minion.Id),1);}
  else if(id=="EX1_609"){Reveal(g,1-caster,id);g.Damage(new MatchTarget(caster,minion.Id),4);}
 }
 }
 // Returns true when the attack is cancelled. Target may be changed by Misdirection.
 public static bool BeforeAttack(MatchEngine g,int attackerSeat,BattleUnit attacker,ref MatchTarget target){var defender=g.Players[1-attackerSeat];var victim=g.Unit(target);bool hero=target.Hero;
  foreach(string id in defender.ActiveSecrets.ToArray()){
   if(id=="EX1_130"&&hero){if(defender.Board.Count<7){Reveal(g,1-attackerSeat,id);var protector=g.Summon(1-attackerSeat,"VAN_EX1_130a");target=new MatchTarget(1-attackerSeat,protector.Id);victim=protector;hero=false;}else continue;}
   else if(id=="EX1_554"&&!hero&&attacker!=null&&defender.Board.Count<7){Reveal(g,1-attackerSeat,id);for(int n=0;n<3&&defender.Board.Count<7;n++)g.Summon(1-attackerSeat,"VAN_EX1_554t");}
   else if(id=="EX1_611"&&attacker!=null){Reveal(g,1-attackerSeat,id);g.BounceRogueUnits(new[]{attacker},2);return true;}
   else if(id=="EX1_289"&&hero){Reveal(g,1-attackerSeat,id);defender.Armor+=8;}
   else if(id=="EX1_610"&&hero){Reveal(g,1-attackerSeat,id);foreach(var t in g.Characters(attackerSeat).ToArray())g.Damage(t,2);g.Cleanup();return true;}
   else if(id=="EX1_594"&&hero&&attacker!=null){Reveal(g,1-attackerSeat,id);attacker.DamageTaken=attacker.MaxHealth;g.Cleanup();return true;}
   else if(id=="EX1_533"&&hero){var options=g.Characters(attackerSeat).Where(t=>attacker==null?t.UnitId!=0:t.Hero||t.UnitId!=attacker.Id).ToArray();if(options.Length>0){Reveal(g,1-attackerSeat,id);target=g.RandomTarget(options);victim=g.Unit(target);hero=victim==null;}}
  }
  return false;
 }
 public static void MinionDied(MatchEngine g,BattleUnit unit){var owner=g.Players[unit.Owner];if(!Has(owner,"EX1_136")||owner.Board.Count>=7)return;Reveal(g,unit.Owner,"EX1_136");var reborn=g.Summon(unit.Owner,unit.CardId,unit.DeathAnchor==0?owner.Board.Count:Math.Max(0,owner.Board.FindIndex(u=>u.Id==unit.DeathAnchor)));reborn.DamageTaken=reborn.MaxHealth-1;g.Log.Add("救赎使随从以 1 点生命值复生。");}
 public static int PreventLethal(MatchEngine g,int seat,int postArmorHealth){var p=g.Players[seat];if(postArmorHealth>0||!Has(p,"EX1_295"))return postArmorHealth;Reveal(g,seat,"EX1_295");p.ImmuneUntilTurn=g.Turn;p.Health=1;g.Log.Add("寒冰屏障使英雄免疫至本回合结束。");return 1;}
 public static void HeroDamaged(MatchEngine g,int seat,int amount){if(amount<=0)return;var p=g.Players[seat];if(Has(p,"EX1_132")){Reveal(g,seat,"EX1_132");g.Damage(new MatchTarget(1-seat),amount);}}
}
}
