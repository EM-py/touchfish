using System;
using System.Linq;
using Touchfish;
public static partial class RuleTests {
 static AgentMove ZooMove(MatchEngine g){return new ZooWarlockAgent().Choose(AgentBridge.Observe(g,0));}
 static MatchEngine ZooGame(){var g=Game();g.Players[0].ClassId="WARLOCK";return g;}
 static void ZooAi(){
  var zoo=ZooWarlockDeck.Create(catalog);Check(zoo.Count==30&&zoo.ClassId=="WARLOCK"&&zoo.Cards.Keys.All(k=>matchRules.Supports(catalog.Card(k))),"Classic Zoo complete and all supported");Check(DeckCode.Decode(catalog,DeckCode.Encode(catalog,zoo),zoo.PoolId).Cards.Count==16,"Zoo deck roundtrip");Check(MatchAgents.Find("zoo").Create() is ZooWarlockAgent&&MatchAgents.Find("freeze").Create() is FreezeMageAgent,"Opponent registry creates correct strategy");
  var g=ZooGame();g.Players[0].Mana=2;var view=AgentBridge.Observe(g,0);Check(view.Moves.Any(m=>m.Kind=="power"&&m.Target==null),"Warlock power provided by legal bridge");var move=ZooMove(g);Check(move.Kind=="power"&&AgentBridge.Execute(g,0,move).Success&&g.Players[0].Health==28&&g.Players[0].Hand.Count==1,"Tap draws and pays health through real rules");
  g=ZooGame();g.Players[0].Health=3;g.Summon(1,"VAN_CS2_182");Check(ZooMove(g).Kind!="power","No unsafe tap under visible threat");g=ZooGame();g.Players[0].Deck.Clear();Check(ZooMove(g).Kind=="end","No tap into fatigue");
  g=ZooGame();g.Players[0].Health=3;Hand(g,"EX1_319");Check(ZooMove(g).Key!="EX1_319","Do not die to own Flame Imp battlecry");
  g=ZooGame();g.Players[1].Health=4;int soul=Hand(g,"EX1_308");move=ZooMove(g);Check(move.Source==soul&&move.Target.Hero&&AgentBridge.Execute(g,0,move).Success&&g.Winner==0,"Soulfire exact lethal");
  g=ZooGame();g.Players[1].Health=9;g.Players[0].Mana=0;Hand(g,"EX1_308");Hand(g,"EX1_310");Check(ZooMove(g).Key!="EX1_308","Do not sacrifice held Doomguard for nonlethal face Soulfire");
  g=ZooGame();g.Players[0].Mana=7;int guard=Hand(g,"EX1_310"),flame=Hand(g,"EX1_319"),voidwalker=Hand(g,"CS2_065");move=ZooMove(g);Check(move.Source!=guard&&move.Kind=="play","Unload affordable minions before Doomguard discard");for(int n=0;n<3;n++){move=ZooMove(g);Check(AgentBridge.Execute(g,0,move).Success,"Doomguard curve legal step");}Check(g.Players[0].Board.Any(u=>u.CardId=="VAN_EX1_310")&&g.Players[0].Hand.Count==0&&g.Graveyard.All(x=>x.Reason!=GraveyardReason.Discarded),"Doomguard played after hand is empty, no discard");
  g=ZooGame();var doom=g.Summon(1,"VAN_NEW1_021");int owl=Hand(g,"CS2_203");move=ZooMove(g);Check(move.Source==owl&&move.Target.UnitId==doom.Id&&AgentBridge.Execute(g,0,move).Success&&doom.Silenced,"Owl silences doomsayer instead of developing into clear");
  g=ZooGame();var frozen=g.Summon(0,"VAN_EX1_008");frozen.Frozen=true;frozen.SummonTurn=0;Hand(g,"EX1_316");Check(ZooMove(g).Key!="EX1_316","No Power Overwhelming on frozen attacker");
  g=ZooGame();var imp=g.Summon(0,"VAN_EX1_319");imp.SummonTurn=0;g.Players[1].Health=7;int po=Hand(g,"EX1_316");move=ZooMove(g);Check(move.Source==po&&move.Target.UnitId==imp.Id,"Power Overwhelming enables exact face lethal");AgentBridge.Execute(g,0,move);move=ZooMove(g);Check(move.Kind=="attack"&&move.Target.Hero&&AgentBridge.Execute(g,0,move).Success&&g.Winner==0,"Buff then attack closes game");
  g=ZooGame();g.Summon(0,"VAN_EX1_008");g.Summon(0,"VAN_EX1_162");g.Summon(0,"VAN_EX1_310");Hand(g,"EX1_093");move=ZooMove(g);Check(move.Key=="EX1_093"&&move.Position==2,"Argus placed between two valuable adjacent bodies");AgentBridge.Execute(g,0,move);Check(g.Players[0].Board.Count(u=>u.Taunt)==2,"Argus grants both adjacent taunts in actual engine");
  g=ZooGame();Hand(g,"EX1_046");g.Summon(1,"VAN_CS2_182");Check(ZooMove(g).Key!="EX1_046","Do not buff enemy just to deploy Abusive Sergeant");
  g=ZooGame();Hand(g,"EX1_308");Hand(g,"CS2_065");Hand(g,"CS2_024",1);move=ZooMove(g);g.Players[1].Hand.Clear();Hand(g,"EX1_279",1);g.Players[1].Deck.Reverse();var after=ZooMove(g);Check(move.Kind==after.Kind&&move.Source==after.Source,"Zoo decision independent of hidden enemy card identities and order");
  var ai=new ZooWarlockAgent();g=ZooGame();Hand(g,"EX1_319");Hand(g,"NEW1_019");int costly=Hand(g,"EX1_310");int spell=Hand(g,"EX1_316");var replaced=ai.Mulligan(AgentBridge.Observe(g,0));Check(replaced.Contains(costly)&&replaced.Contains(spell)&&replaced.Length==2,"Keep one drop plus knife juggler, replace expensive minion and spell");
  for(int seed=0;seed<4;seed++){
   var enemy=seed%2==0?FreezeMageDeck.Create(catalog):zoo;IMatchAgent opponent=seed%2==0?(IMatchAgent)new FreezeMageAgent():new ZooWarlockAgent();var game=new MatchEngine(catalog,zoo,enemy,seed,startingSeat:seed%2);
   Check(game.Mulligan(0,ai.Mulligan(AgentBridge.Observe(game,0))).Success&&game.Mulligan(1,opponent.Mulligan(AgentBridge.Observe(game,1))).Success,"Zoo normal mulligans");int actions=0;while(!game.Finished&&game.Turn<100&&actions<1800){int seat=game.Active;var action=(seat==0?(IMatchAgent)ai:opponent).Choose(AgentBridge.Observe(game,seat));var result=AgentBridge.Execute(game,seat,action);if(!result.Success)throw new Exception("Zoo integration illegal "+action.Kind+" "+action.Key+": "+result.Message);actions++;}Check(game.Finished&&actions<1800,"Complete Zoo versus Freeze/Zoo without illegal actions or loops");
  }
 }
}
