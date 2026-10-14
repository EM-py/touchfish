using System;using System.Linq;using Touchfish;
public static partial class RuleTests {
 static void TargetResolution(){
  var g=Game();var living=g.Summon(1,"VAN_CS2_182");var dead=g.Summon(1,"VAN_CS2_231");g.Damage(new MatchTarget(1,dead.Id),1);
  var pool=new[]{null,new MatchTarget(-1),new MatchTarget(2),new MatchTarget(1,dead.Id),new MatchTarget(1,999999),new MatchTarget(1,living.Id),new MatchTarget(1,living.Id)};
  Check(g.SelectRandomTargets(pool,5).Length==1,"Candidates reject stale, invalid and dead targets and deduplicate identity");
  Check(g.RandomTarget(new MatchTarget[0])==null&&g.Damage(null,3)==0&&g.Damage(new MatchTarget(1,999999),3)==0,"Empty random target and damage safely fizzle");
  Check(g.Characters(1,false).Length==1,"Public living character query excludes pending death");
  foreach(string spell in new[]{"CS2_114","DS1_183","EX1_251"}){
   g=InteractionSeedFixture();g.Summon(0,"VAN_NEW1_026");g.Summon(0,"VAN_NEW1_019");g.Summon(1,"VAN_CS2_231");var yeti=g.Summon(1,"VAN_CS2_182");
   Check(g.Play(0,Hand(g,spell)).Success,"Multi-target spell survives cast-trigger target loss: "+spell);
   Check(g.Players[1].Board.Count==1&&yeti.Health==(spell=="DS1_183"?2:3),"Remaining target hit once without null damage: "+spell);
   Check(g.Players[0].Hand.Count==0&&g.CardsPlayed==1&&g.Players[0].Overload==(spell=="EX1_251"?2:0),"Target loss preserves paid spell bookkeeping: "+spell);
  }
  foreach(int removed in new[]{0,1,2}){
   g=Game();var first=g.Summon(1,"VAN_CS2_182");var second=g.Summon(1,"VAN_CS2_182");
   // Registered expansion trigger models an arbitrary cast-time battlefield change.
   g.Rules.RegisterTrigger(new ConditionalTrigger{CardId="NEW1_026",Timing=MatchTriggerTiming.SpellCast,Condition=c=>true,Apply=c=>{if(removed>0)c.Game.Damage(new MatchTarget(1,first.Id),20);if(removed>1)c.Game.Damage(new MatchTarget(1,second.Id),20);}});
   g.Summon(0,"VAN_NEW1_026");Check(g.Play(0,Hand(g,"CS2_114")).Success,"Cast-time candidate reduction completes safely: "+removed);
   Check(g.Players[1].Board.Count==2-removed&&g.Players[1].Board.All(u=>u.Health==3),"Batch handles zero, one or two remaining candidates: "+removed);
  }
  g=Game();g.Summon(1,"VAN_CS2_182");int hand=Hand(g,"CS2_114");Check(!g.Play(0,hand).Success&&g.Players[0].Mana==10&&g.Players[0].Hand.Any(h=>h.Id==hand),"Insufficient targets at command time remain illegal before payment");
  g=Game();g.Summon(1,"VAN_CS2_182");g.Summon(1,"VAN_CS2_182");g.ArmSecret(1,new HandCard{Id=id++,CardId="VAN_EX1_287"});Check(g.Play(0,Hand(g,"CS2_114")).Success&&g.Players[1].Board.All(u=>u.Health==5),"Countered multi-target spell does no damage");
  g=Game();var golem=g.Summon(1,"VAN_EX1_556");g.Damage(new MatchTarget(1,golem.Id),1);var body=g.Summon(1,"VAN_CS2_182");g.DamageRandomMinions(1,2,2);g.Cleanup();
  Check(body.Health==3&&g.Players[1].Board.Any(u=>u.CardId=="VAN_skele21"&&u.Health==1),"Batch snapshot does not chase deathrattle replacement");
  g=Game();body=g.Summon(1,"VAN_CS2_182");body.Shield=true;g.DamageRandomMinions(1,2,2);
  Check(body.Health==5&&!body.Shield,"One candidate consumes shield once without repeated targeting");
  var view=LanProtocol.ReadView(catalog,LanProtocol.Copy(LanProtocol.View(g,0,"targets",1)));
  Check(view.Players[1].Board.Single().Health==5&&!view.Players[1].Board.Single().Shield,"Result is preserved by explicit public snapshot");
  g=Game();var attacker=g.Summon(0,"VAN_CS2_182");attacker.SummonTurn=0;var target=g.Summon(1,"VAN_CS2_182");g.TakePermanentControl(0,new MatchTarget(1,target.Id));
  Check(!g.IsPresentTarget(new MatchTarget(1,target.Id))&&g.IsPresentTarget(new MatchTarget(0,target.Id)),"Control change invalidates old-seat target identity");
  Check(g.Damage(new MatchTarget(1,target.Id),2)==0&&target.Health==5,"Stale seat target cannot damage transferred minion");
 }
}
