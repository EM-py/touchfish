using System;
using System.Linq;
using System.Collections.Generic;
namespace Touchfish {
public static class WarriorRules {
 public static void Apply(EffectContext c){var g=c.Game;var p=g.Players[c.Seat];switch(c.Card.BaseId){
  case "CS2_104":g.Buff(c.Target,3,3);break;
  case "CS2_114":var targets=g.Characters(1-c.Seat,false).ToList();var first=g.RandomTarget(targets);targets.Remove(first);var second=g.RandomTarget(targets);int damage=2+g.SpellPower(c.Seat);g.Damage(first,damage);g.Damage(second,damage);break;
  case "EX1_392":g.Draw(c.Seat,(p.Health<g.Mode.StartingHealth?1:0)+p.Board.Count(u=>u.Health>0&&u.DamageTaken>0));break;
  case "EX1_398":g.EquipRaw(c.Seat,"GAME_BATTLE_AXE",2,2,false);break;
  case "EX1_407":var units=g.Players.SelectMany(player=>player.Board).ToArray();var survivor=g.RandomTarget(units.Select(u=>new MatchTarget(u.Owner,u.Id)));foreach(var u in units)if(u.Id!=survivor.UnitId)u.DamageTaken=u.MaxHealth;break;
  case "EX1_409":if(p.WeaponDurability>0){p.WeaponAttack++;p.WeaponDurability++;}else g.EquipRaw(c.Seat,"GAME_HEAVY_AXE",1,3,false);break;
  case "NEW1_036":p.CommandingShoutTurn=g.Turn;foreach(var u in p.Board)u.CommandingShoutTurn=g.Turn;g.Draw(c.Seat,1);break;
 }}
 public static void Summoned(MatchEngine g,BattleUnit unit,BattleUnit[] commanders){if(g.Players[unit.Owner].CommandingShoutTurn==g.Turn)unit.CommandingShoutTurn=g.Turn;if(commanders.Length>0&&g.AttackValue(unit)<=3)unit.Charge=true;}
 public static void UseWeapon(MatchEngine g,int seat,MatchTarget target){var p=g.Players[seat];if(p.WeaponDurability<=0)return;if(!target.Hero&&g.Card(p.WeaponId).BaseId=="EX1_411")p.WeaponAttack--;else if(--p.WeaponDurability==0)g.BreakWeapon(seat);}
}
}
