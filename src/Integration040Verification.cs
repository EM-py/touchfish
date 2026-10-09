using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace Touchfish {
public partial class PanelWindow {
 void Verify040Integration(string folder){
  int before=checks;
  var g=new MatchEngine(catalog,matchRules.TrainingDeck("MAGE"),matchRules.TrainingDeck("WARRIOR"),42);
  Check(!g.MulliganComplete&&g.Players[0].Hand.Count==3&&g.Players[1].Hand.Count==4,"Opening hand sizes before coin");
  var hand=g.Players[0].Hand.ToArray();int mana=g.Players[0].Mana;int log=g.Log.Count;
  Check(!g.Mulligan(0,new[]{hand[0].Id,hand[0].Id}).Success&&g.Players[0].Hand.SequenceEqual(hand)&&g.Log.Count==log,"Duplicate mulligan selection is atomic");
  Check(!g.Mulligan(0,new[]{g.Players[1].Hand[0].Id}).Success&&g.Players[0].Hand.SequenceEqual(hand),"Mulligan cannot replace an opponent card");
  Check(!g.Play(0,hand[0].Id).Success&&g.Players[0].Mana==mana&&!g.EndTurn().Success,"Turn actions stay blocked during mulligan");
  string original=hand[0].CardId;g.Players[0].Deck.Clear();g.Players[0].Deck.Add("VAN_EX1_105");
  Check(g.Mulligan(0,new[]{hand[0].Id}).Success&&g.Players[0].Hand.Last().CardId=="VAN_EX1_105"&&g.Players[0].Deck.SequenceEqual(new[]{original}),"Replaced cards return only after replacements are drawn");
  Check(!g.Mulligan(0,new int[0]).Success&&!g.MulliganComplete,"Only one confirmation per seat");
  Check(g.Mulligan(1,new int[0]).Success&&g.MulliganComplete&&g.Players[1].Hand.Last().CardId=="GAME_COIN"&&g.Players[0].Mana==1,"Both confirmations enable turns and add one coin");
  Check(g.Emote(1,HeroEmotes.Options("WARRIOR")[3].Value).Success,"Non-active player may send a validated hero phrase");
  log=g.Log.Count;Check(!g.Emote(1,"arbitrary input").Success&&g.Log.Count==log,"Arbitrary emote strings are rejected");
  var view=LanProtocol.View(g,0,"integration",1);Check(view.Players[1].Hand.Length==0&&view.Players[1].Public.Hand.Count==0&&view.Players[1].Public.Deck.Count==0&&view.Players[1].HandCount==5,"Mulligan and emote snapshots preserve opponent privacy");
  g=MatchFixture("PALADIN");g.Players[0].Deck.Clear();g.Players[0].Deck.Add("VAN_EX1_105");int wrath=Inject(g,"EX1_365");g.Summon(0,"VAN_EX1_563");Check(g.Play(0,wrath,new MatchTarget(1)).Success&&g.Players[1].Health==13,"Holy Wrath targets heroes and adds spell damage");
  g=MatchFixture("PALADIN");var body=g.Summon(0,"VAN_CS2_182");int wisdom=Inject(g,"EX1_363");g.Play(0,wisdom,new MatchTarget(0,body.Id));g.Silence(new MatchTarget(0,body.Id));Check(body.WisdomBlessings0==0&&body.WisdomBlessings1==0,"Silence removes Wisdom counters while retaining aura rules");
  g=MatchFixture();body=g.Summon(1,"VAN_CS2_182");g.StealRandomEnemyMinion(0);g.ReturnToHand(new MatchTarget(0,body.Id));Check(g.Players[0].Hand.Any(h=>h.CardId==body.CardId)&&g.Players[1].Hand.Count==0,"Permanent steal and Dream bounce return to the new owner");
  g=MatchFixture();var hogger=g.Summon(0,"VAN_NEW1_040");g.EndTurn();Check(g.Players[0].Board.Count(u=>u.CardId=="GAME_GNOLL")==1,"Integrated Hogger has exactly one end trigger");
  localTest=true;lanMode=false;awaitHandoff=false;matchViewSeat=0;testTakeOpen=false;inspectMatch=false;ClearPending();
  match=new MatchEngine(catalog,matchRules.TrainingDeck("MAGE"),matchRules.TrainingDeck("WARRIOR"),42,true);ApplyMode(false);ApplyTheme(true,true);RenderMatch();ShowPage("match");Render(Path.Combine(folder,"80-v040-mulligan-normal.png"));
  Check(Width==548&&Height==474&&liveUnits.Count==0&&livePanel.Children.OfType<ScrollViewer>().Any(v=>Grid.GetRow(v)==6),"Mulligan stays in the bottom normal hand region");
  ApplyMode(true);Render(Path.Combine(folder,"81-v040-mulligan-minimal.png"));Check(Width==392&&Height==300,"Mulligan preserves minimal window size");
  match=MatchFixture();RenderMatch();emoteOpen=true;RenderMatch();Render(Path.Combine(folder,"82-v040-emotes-minimal.png"));Check(livePanel.Children.OfType<ScrollViewer>().Any(v=>Grid.GetRow(v)==6),"Hero dialogue stays in the bottom hand region");CheckNormalWeights(livePanel);emoteOpen=false;localTest=false;RenderMatch();
  JsonData.Write(Path.Combine(folder,"v040-integration-verification.json"),new{Assertions=checks-before,Mulligan=true,SingleHoggerTrigger=true,OpponentPrivateDataHidden=true,Version="v0.4.0"});
 }
}
}
