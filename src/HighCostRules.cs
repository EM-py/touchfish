using System;
using System.Linq;
namespace Touchfish {
public static class HighCostRules {
 public static readonly string[] CardIds={"CS2_226","EX1_002","EX1_095","EX1_112","EX1_283","EX1_399","EX1_564","EX1_614","NEW1_024","NEW1_041","PRO_001"};
 public static readonly string[] Inventions={"GAME_HOMING_CHICKEN","GAME_REPAIR_BOT","GAME_EMBOLDENER","GAME_POULTRYIZER"};
 public static readonly string[] Chords={"GAME_I_AM_MURLOC","GAME_ROGUES_DO_IT","VAN_PRO_001c"};
 public static readonly string[] HordeWarriors={"VAN_CS2_121","VAN_EX1_021","VAN_EX1_023","VAN_EX1_110","VAN_EX1_390","VAN_CS2_179"};
 public static readonly ConditionalTrigger[] Triggers={
  new ConditionalTrigger{CardId="EX1_095",Timing=MatchTriggerTiming.SpellCast,Condition=c=>c.Seat==c.Source.Owner,Apply=c=>c.Game.Draw(c.Source.Owner,1)},
  new ConditionalTrigger{CardId="EX1_399",Timing=MatchTriggerTiming.MinionDamaged,Condition=c=>c.Damaged.Id==c.Source.Id&&c.Amount>0,Apply=c=>c.Source.BuffAttack+=3},
  new ConditionalTrigger{CardId="EX1_614",Timing=MatchTriggerTiming.CardResolved,Condition=c=>c.Seat==c.Source.Owner,Apply=c=>c.Game.Summon(c.Source.Owner,"GAME_AZZINOTH_FLAME")},
  new ConditionalTrigger{CardId="GAME_HOMING_CHICKEN",Timing=MatchTriggerTiming.TurnStart,Condition=c=>c.Seat==c.Source.Owner,Apply=c=>{c.Source.DamageTaken=c.Source.MaxHealth;c.Game.Cleanup();if(!c.Game.Finished)c.Game.Draw(c.Source.Owner,3);}},
  new ConditionalTrigger{CardId="GAME_REPAIR_BOT",Timing=MatchTriggerTiming.TurnEnd,Condition=c=>c.Seat==c.Source.Owner,Apply=Repair},
  new ConditionalTrigger{CardId="GAME_EMBOLDENER",Timing=MatchTriggerTiming.TurnEnd,Condition=c=>c.Seat==c.Source.Owner,Apply=c=>{var target=c.Game.RandomTarget(c.Game.Characters(0,false).Concat(c.Game.Characters(1,false)));if(target!=null)c.Game.Buff(target,1,1);}},
  new ConditionalTrigger{CardId="GAME_POULTRYIZER",Timing=MatchTriggerTiming.TurnStart,Condition=c=>c.Seat==c.Source.Owner,Apply=c=>{var target=c.Game.RandomTarget(c.Game.Characters(0,false).Concat(c.Game.Characters(1,false)));if(target!=null)c.Game.Transform(target,"GAME_CHICKEN");}}
 };
 static void Repair(MatchTriggerContext c){var g=c.Game;var targets=g.Characters(0).Concat(g.Characters(1)).Where(t=>t.Hero?!g.Players[t.Seat].HeroDestroyed&&g.Players[t.Seat].Health<g.HeroHealthLimit(t.Seat):g.Unit(t).Health>0&&g.Unit(t).DamageTaken>0);var target=g.RandomTarget(targets);if(target!=null)g.RestoreHealth(c.Source.Owner,target,6);}
 public static void Apply(EffectContext c){var g=c.Game;switch(c.Card.BaseId){
  case "CS2_226":int count=g.Players[c.Seat].Board.Count(u=>u.Id!=c.Summoned.Id&&u.Health>0);g.Buff(new MatchTarget(c.Seat,c.Summoned.Id),count,count);break;
  case "EX1_002":if(c.Target!=null){var victim=g.Unit(c.Target);victim.DamageTaken=victim.MaxHealth;}break;
  case "EX1_112":g.Summon(c.Seat,Inventions[g.RandomIndex(Inventions.Length)],g.Players[c.Seat].Board.IndexOf(c.Summoned)+1);break;
  case "EX1_283":if(c.Target!=null)g.Freeze(c.Target);break;
  case "EX1_564":if(c.Target!=null)g.CopyFaceless(c.Summoned,g.Unit(c.Target));break;
  case "NEW1_024":var p=g.Players[c.Seat];if(p.WeaponId!=null&&p.WeaponDurability>0){p.WeaponAttack++;p.WeaponDurability++;}break;
  case "NEW1_041":var target=g.RandomTarget(g.Characters(1-c.Seat,false).Where(t=>g.AttackValue(g.Unit(t))<=2));if(target!=null){var unit=g.Unit(target);unit.DamageTaken=unit.MaxHealth;}break;
  case "PRO_001":for(int seat=0;seat<2;seat++)g.GiveMidCostCards(seat,Chords[g.RandomIndex(Chords.Length)],1);break;
  case "GAME_I_AM_MURLOC":int amount=3+g.RandomIndex(3);for(int n=0;n<amount;n++)g.Summon(c.Seat,"GAME_CHORD_MURLOC");break;
  case "GAME_ROGUES_DO_IT":g.Damage(c.Target,g.SpellDamageAmount(c.Seat,4));g.Draw(c.Seat,1);break;
  case "PRO_001c":g.Summon(c.Seat,HordeWarriors[g.RandomIndex(HordeWarriors.Length)]);break;
 }}
 public static CardRecord Auxiliary(string id){switch(id){
  case "GAME_AZZINOTH_FLAME":return Minion(id,"埃辛诺斯之焰",1,2,1,"","");
  case "GAME_CHORD_MURLOC":return Minion(id,"鱼人",0,1,1,"MURLOC","");
  case "GAME_CHICKEN":return Minion(id,"小鸡",0,1,1,"BEAST","");
  case "GAME_HOMING_CHICKEN":return Minion(id,"导航小鸡",1,0,1,"MECHANICAL","在你的回合开始时，消灭本随从并抽三张牌。",true);
  case "GAME_REPAIR_BOT":return Minion(id,"修理机器人",1,0,3,"MECHANICAL","在你的回合结束时，随机为一个受伤的角色恢复6点生命值。",true);
  case "GAME_EMBOLDENER":return Minion(id,"壮胆机器人3000型",1,0,4,"MECHANICAL","在你的回合结束时，随机使一个随从获得+1/+1。",true);
  case "GAME_POULTRYIZER":return Minion(id,"变鸡器",1,0,3,"MECHANICAL","在你的回合开始时，随机将一个随从变形成为1/1的小鸡。",true);
  case "GAME_I_AM_MURLOC":return Spell(id,"我是鱼人",4,"召唤三、四或五个1/1的鱼人。");
  case "GAME_ROGUES_DO_IT":return Spell(id,"潜行者的伎俩",4,"造成4点伤害，抽一张牌。");
  default:return null;
 }}
 static CardRecord Minion(string id,string name,int cost,int attack,int health,string race,string text,bool trigger=false){return new CardRecord{Id=id,BaseId=id,Name=name,CardClass="NEUTRAL",Type="MINION",Cost=cost,Attack=attack,Health=health,Race=race,Text=text,Collectible=false,Mechanics=trigger?new[]{"TRIGGER_VISUAL"}:new string[0]};}
 static CardRecord Spell(string id,string name,int cost,string text){return new CardRecord{Id=id,BaseId=id,Name=name,CardClass="NEUTRAL",Type="SPELL",Cost=cost,Text=text,Collectible=false,Mechanics=new string[0]};}
}
public sealed partial class MatchEngine {
 public void CopyFaceless(BattleUnit body,BattleUnit source){if(body==null||source==null||body==source)return;int instance=body.Id,owner=body.Owner,turn=body.SummonTurn,attacks=body.AttacksUsed;BattleUnitCopies.Copy(source,body);body.Id=instance;body.Owner=body.OriginalOwner=owner;body.SummonTurn=turn;body.AttacksUsed=attacks;body.ReturnSeat=body.ReturnTurn=-1;body.AuraHealth=0;RefreshAuras();}
}
}
