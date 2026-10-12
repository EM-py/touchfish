using System;
namespace Touchfish {
public static class PaladinRules {
 public static void Apply(EffectContext c){if(c.Card.BaseId=="CS2_088")c.Game.RestoreHealth(c.Seat,new MatchTarget(c.Seat),6);}
 public static void Summoned(MatchEngine game,BattleUnit unit){var p=game.Players[unit.Owner];if(p.WeaponDurability<=0||p.WeaponId==null||game.Card(p.WeaponId).BaseId!="EX1_366")return;game.Buff(new MatchTarget(unit.Owner,unit.Id),1,1);p.WeaponDurability--;if(p.WeaponDurability==0)game.BreakWeapon(unit.Owner);}
}
}
