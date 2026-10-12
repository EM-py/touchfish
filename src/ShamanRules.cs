using System;
using System.Linq;
namespace Touchfish {
public static class ShamanRules {
 public static void Apply(EffectContext c){var g=c.Game;switch(c.Card.BaseId){
  case "CS2_038":g.Unit(c.Target).Enchantments.Items.Add(new Enchantment{Kind="ancestral-spirit",Amount=1});break;
  case "CS2_042":g.Damage(c.Target,3);break;
  case "CS2_053":g.Draw(c.Seat,1,-3);break;
  case "EX1_244":foreach(var unit in g.Players[c.Seat].Board.Where(u=>u.Race=="TOTEM").ToArray())g.Buff(new MatchTarget(c.Seat,unit.Id),0,2);break;
  case "EX1_246":g.Transform(c.Target,"GAME_FROG");break;
  case "EX1_587":g.Unit(c.Target).Windfury=true;break;
 }}
}
}
