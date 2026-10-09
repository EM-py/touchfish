using System;
using System.Linq;
using System.IO;
using System.Windows;
using System.Windows.Controls;
namespace Touchfish {
public partial class PanelWindow {
 ComboBox testTakePicker;Button testTakeButton;
 FrameworkElement LiveTestTake(){
  var grid=new Grid();grid.RowDefinitions.Add(new RowDefinition{Height=new GridLength(13)});grid.RowDefinitions.Add(new RowDefinition());grid.Children.Add(T("test · 从剩余牌库取到手牌（最多十张）",9,Muted));
  var row=new Grid();row.ColumnDefinitions.Add(new ColumnDefinition());row.ColumnDefinitions.Add(new ColumnDefinition{Width=new GridLength(38)});row.ColumnDefinitions.Add(new ColumnDefinition{Width=new GridLength(34)});
  var p=match.Players[matchViewSeat];var choices=p.Deck.GroupBy(id=>id).OrderBy(g=>match.Card(g.Key).Cost).ThenBy(g=>match.Card(g.Key).Name).Select(g=>new Choice(g.Key,match.Card(g.Key).Cost+"费 · "+match.Card(g.Key).Name+" ×"+g.Count())).ToArray();
  testTakePicker=Picker(choices);testTakePicker.Height=21;testTakePicker.SelectedIndex=choices.Length>0?0:-1;row.Children.Add(testTakePicker);
  testTakeButton=Btn("取牌",38);testTakeButton.Height=21;testTakeButton.FontSize=9;testTakeButton.IsEnabled=CanMatchInput()&&choices.Length>0&&p.Hand.Count<10;testTakeButton.Click+=(s,e)=>TakeTestCard();Grid.SetColumn(testTakeButton,1);row.Children.Add(testTakeButton);
  var back=Btn("返回",34);back.Height=21;back.FontSize=9;back.Click+=(s,e)=>{testTakeOpen=false;RenderMatch();};Grid.SetColumn(back,2);row.Children.Add(back);Put(grid,row,1);return grid;
 }
 void TakeTestCard(){if(!localTest||lanMode||match==null||!match.TestMode||!CanMatchInput())return;var choice=testTakePicker.SelectedItem as Choice;if(choice!=null)ApplyMatchAction(match.TestTakeCard(matchViewSeat,choice.Key));}
 void VerifyLocalTestTools(string folder){
  var game=new MatchEngine(catalog,matchRules.TrainingDeck("MAGE"),matchRules.TrainingDeck("WARRIOR"),42,true);Check(game.Players.All(player=>player.MaxMana==10&&player.Mana==10),"Test starts both players at ten full crystals");var p=game.Players[0];string id=p.Deck.Last();var remaining=p.Deck.ToArray();int count=p.Hand.Count;Check(game.TestTakeCard(0,id).Success&&p.Hand.Count==count+1&&p.Hand.Last().CardId==id&&p.Deck.Count==remaining.Length-1,"Test selects requested remaining card");var expected=remaining.ToList();expected.Remove(id);Check(p.Deck.SequenceEqual(expected),"Test take preserves remaining deck order");Check(!game.TestTakeCard(1,game.Players[1].Deck[0]).Success,"Test take requires active seat");Check(!game.TestTakeCard(0,"GAME_COIN").Success,"Test cannot take absent card");while(p.Hand.Count<10)game.TestTakeCard(0,p.Deck[0]);int size=p.Deck.Count;Check(!game.TestTakeCard(0,p.Deck[0]).Success&&p.Deck.Count==size&&p.Hand.Count==10,"Full test hand rejects without consuming card");game.EndTurn();Check(game.Players[1].Mana==10,"Test next player retains ten crystals");var regular=new MatchEngine(catalog,matchRules.TrainingDeck("MAGE"),matchRules.TrainingDeck("WARRIOR"),42);size=regular.Players[0].Deck.Count;Check(!regular.TestTakeCard(0,regular.Players[0].Deck[0]).Success&&regular.Players[0].Deck.Count==size&&regular.Players[0].Mana==1,"Normal match rejects test operations and keeps normal mana");
  match=game;matchViewSeat=game.Active;awaitHandoff=false;localTest=true;lanMode=false;ClearPending();inspectMatch=false;testTakeOpen=true;ApplyMode(true);RenderMatch();ShowPage("match");Render(Path.Combine(folder,"55-test-take-minimal.png"));Check(testTakePicker.Items.Count>0&&testTakeButton.IsEnabled&&Width==392&&Height==300,"Test picker fits minimal window");id=((Choice)testTakePicker.SelectedItem).Key;TakeTestCard();Check(match.Players[matchViewSeat].Hand.Last().CardId==id&&!testTakeOpen,"Test UI takes selection and returns to hand");ApplyMode(false);testTakeOpen=true;RenderMatch();Render(Path.Combine(folder,"56-test-take-normal.png"));CheckNormalWeights(livePanel);testTakeOpen=false;localTest=false;
 }
}
}
