using System;using System.Linq;
namespace Touchfish {public partial class PanelWindow {
 void VerifyTargetResolutionTcp(PanelWindow guest,MatchEngine game){
  foreach(int removed in new[]{0,1,2}){
   foreach(var p in game.Players){p.Hand.Clear();p.Board.Clear();p.Mana=p.MaxMana=10;}
   var teacher=game.Summon(0,"VAN_NEW1_026");var targets=new[]{game.Summon(1,"VAN_CS2_182"),game.Summon(1,"VAN_CS2_182")};
   int count=removed,source=teacher.Id;
   game.Rules.RegisterTrigger(new ConditionalTrigger{CardId="NEW1_026",Timing=MatchTriggerTiming.SpellCast,Condition=c=>c.Source.Id==source&&c.Card.BaseId=="CS2_114",Apply=c=>{foreach(var unit in targets.Take(count))c.Game.Damage(new MatchTarget(1,unit.Id),20);}});
   int hand=Inject(game,"CS2_114");PublishLan();PumpLan(()=>guest.match.Players[1].Board.Count==2&&guest.match.Players[0].Board.Any(u=>u.Id==source),"Target fixture synchronized");
   DoMatch("play",hand);
   PumpLan(()=>guest.match.Players[1].Board.Count==2-count&&guest.match.Players[0].Mana==8,"Reduced candidate damage and resources synchronized");
   Check(game.Players[1].Board.All(u=>u.Health==3)&&guest.match.Players[1].Board.All(u=>u.Health==3),"Authority and peer agree with "+count+" vanished candidates");
   Check(guest.match.Players[0].Hand.Count==0&&LanProtocol.View(game,1,"targets",1).Players[0].Hand.Length==0,"Target resolution preserves hand privacy");
  }
 }
}}
