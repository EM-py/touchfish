using System;
using System.IO;
using System.Linq;
namespace Touchfish {
public partial class PanelWindow {
 public void PreviewOpeningTurnScope(string folder){
  previewRun=true;Directory.CreateDirectory(folder);int before=checks;
  foreach(int firstSeat in new[]{0,1})foreach(bool replace in new[]{false,true}){
   var game=new MatchEngine(catalog,matchRules.TrainingDeck("MAGE"),matchRules.TrainingDeck("WARRIOR"),31);
   Check(game.Players[0].Hand.Count==3&&game.Players[1].Hand.Count==4,"Before mulligan first has three and second has four");
   var ids=replace?game.Players[firstSeat].Hand.Select(h=>h.Id).ToArray():new int[0];Check(game.Mulligan(firstSeat,ids).Success,"First confirmation succeeds in either order");
   Check(!game.MulliganComplete&&game.Players[0].Hand.Count==3&&game.Players[1].Hand.Count==4,"One confirmation never triggers turn draw or coin");
   int next=1-firstSeat;ids=replace?game.Players[next].Hand.Select(h=>h.Id).Take(1).ToArray():new int[0];Check(game.Mulligan(next,ids).Success,"Second confirmation starts turn");
   Check(game.Players[0].Hand.Count==4&&game.Players[0].Deck.Count==26&&game.Players[0].Mana==1,"First player draws once after mulligan with one crystal");
   Check(game.Players[1].Hand.Count==5&&game.Players[1].Deck.Count==26&&game.Players[1].Hand.Count(h=>h.CardId=="GAME_COIN")==1,"Second player has four plus coin without premature turn draw");
   int log=game.Log.Count;Check(!game.Mulligan(0,new int[0]).Success&&game.Players[0].Hand.Count==4&&game.Players[0].Deck.Count==26&&game.Log.Count==log,"Repeated confirmation never draws again");
   Check(game.EndTurn().Success&&game.Players[1].Hand.Count==6&&game.Players[1].Deck.Count==25,"Second player draws once on first own turn");
   Check(game.EndTurn().Success&&game.Players[0].Hand.Count==5&&game.Players[0].Deck.Count==25,"Next first-player turn still draws once");
  }
  var first=matchRules.TrainingDeck("MAGE");var second=matchRules.TrainingDeck("WARRIOR");first.Cards.Clear();second.Cards.Clear();first.Cards["VAN_CS2_231"]=2;first.Cards["VAN_CS2_182"]=1;second.Cards["VAN_CS2_231"]=2;second.Cards["VAN_CS2_182"]=2;
  var shortGame=new MatchEngine(catalog,first,second,12,true);shortGame.Mulligan(1,new int[0]);shortGame.Mulligan(0,new int[0]);
  Check(shortGame.Players[0].Fatigue==1&&shortGame.Players[0].Health==29&&shortGame.Players[0].Hand.Count==3,"Empty test deck applies first-turn fatigue instead of skipping draw");
  Check(shortGame.Players.All(p=>p.Mana==10&&p.MaxMana==10),"Test first-turn draw preserves ten full crystals");
  var normalTest=new MatchEngine(catalog,matchRules.TrainingDeck("MAGE"),matchRules.TrainingDeck("WARRIOR"),12,true);normalTest.Mulligan(0,new int[0]);normalTest.Mulligan(1,new int[0]);Check(normalTest.Players[0].Hand.Count==4&&normalTest.Players[0].Mana==10,"Test full deck also draws first turn");
  VerifyLan(folder);
  LeaveLan();ApplyTheme(true,true);ApplyMode(false);SetFullEffects(false);match=new MatchEngine(catalog,matchRules.TrainingDeck("MAGE"),matchRules.TrainingDeck("WARRIOR"),31);match.Mulligan(0,new int[0]);match.Mulligan(1,new int[0]);matchViewSeat=0;awaitHandoff=false;ClearPending();RenderMatch();ShowPage("match");Render(Path.Combine(folder,"105-opening-first-turn.png"));Check(match.Players[0].Hand.Count==4&&Width==548&&Height==474,"First-turn hand appears in fixed WPF window");
  File.WriteAllText(Path.Combine(folder,"opening-turn-verification.txt"),"PASS: "+(checks-before)+" focused opening and real loopback TCP assertions. Confirmation order, replacements, first draw, coin, repeated confirmation, following turns, fatigue, test crystals and opponent hand privacy.");
 }
}
}
