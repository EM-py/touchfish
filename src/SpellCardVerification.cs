using System;
using System.IO;
using System.Linq;
using System.Diagnostics;
namespace Touchfish {
public partial class PanelWindow {
 public void PreviewSpellCardsScope(string folder){
  previewRun=true;Directory.CreateDirectory(folder);int before=checks;
  var game=MatchFixture("MAGE");int spell=Inject(game,"CS2_029");game.Play(0,spell,new MatchTarget(1));var item=game.VisualEvents.First(e=>e.Kind=="spell");
  Check(item.CardName=="火球术"&&item.CardId==game.Card("VAN_CS2_029").Id&&item.CardCost==4&&item.PaidCost==4&&item.CardText==game.Card(item.CardId).Text,"Spell event includes public identity, printed and paid cost, full effect");
  var copy=LanProtocol.ReadView(catalog,LanProtocol.Copy(LanProtocol.View(game,1,"spell-card",1)));
  Check(copy.VisualEvents.First(e=>e.Kind=="spell").CardText==item.CardText&&copy.Players[0].Hand.Count==0,"Masked snapshot carries played spell text while hiding opponent hand");
  spell=Inject(game,"CS2_029");game.Players[0].Mana=0;int count=game.VisualEvents.Count;game.Play(0,spell,new MatchTarget(1));Check(game.VisualEvents.Count==count,"Rejected spell has no card display event");
  TimedCostRules.GrantFreeSpells(game,0,game.Turn);game.Play(0,spell,new MatchTarget(1));item=game.VisualEvents.Last(e=>e.Kind=="spell");Check(item.PaidCost==0&&item.CardCost==4,"Free spell preserves printed cost and actual payment");
  LeaveLan();ApplyTheme(true,true);ApplyMode(false);SetFullEffects(true);ShowPage("match");match=MatchFixture("MAGE");matchViewSeat=0;awaitHandoff=false;ClearPending();RenderMatch();UpdateLayout();var renderer=(OfficeMatchVisualEffects)matchEffects;renderer.Clear();
  var watch=Stopwatch.StartNew();renderer.SpellCard(item);renderer.SpellCard(item);renderer.SpellCard(item);Check(renderer.ActiveSpellCards==3,"Rapid spells coexist in ordered separate panels");Render(Path.Combine(folder,"101-spell-cards-codex.png"));
  PumpLan(()=>watch.ElapsedMilliseconds>=2050,"Spell card clock passes two second hold");Check(renderer.ActiveSpellCards==3,"Panels remain present during fade after two seconds");PumpLan(()=>renderer.ActiveSpellCards==0,"Panels expire independently after hold and fade");
  SetFullEffects(false);renderer.SpellCard(item);Check(renderer.ActiveSpellCards==0,"Extras disabled prevents complete spell panels");int hints=renderer.SpellCount;renderer.Spell(item);Check(renderer.SpellCount==hints+1,"Base spell hints remain active without extras");SetFullEffects(true);renderer.SpellCard(item);ApplyMode(true);Check(renderer.ActiveSpellCards==0,"Minimal transition clears active spell panels");renderer.SpellCard(item);Check(renderer.ActiveSpellCards==0,"Minimal blocks spell panels even when extras enabled");ApplyMode(false);RenderMatch();
  int shown=renderer.SpellCardCount;spell=Inject(match,"CS2_024");match.Play(0,spell,new MatchTarget(1));RenderMatch();PumpLan(()=>renderer.SpellCount>hints+1,"Own spell is presented");Check(renderer.SpellCardCount==shown,"Caster does not see own opponent presentation panel");
  VerifySpellCardsTcp(folder);
  File.WriteAllText(Path.Combine(folder,"spell-card-verification.txt"),"PASS · "+(checks-before)+" targeted assertions\r\n");
 }
 void VerifySpellCardsTcp(string folder){
  LeaveLan();ApplyMode(false);ApplyTheme(true,true);SetFullEffects(true);var guest=new PanelWindow(true){previewRun=true,ShowInTaskbar=false,Opacity=0};guest.Show();guest.ApplyMode(false);guest.SetFullEffects(false);
  try{
   lanPort.Text=FreeLanPort().ToString();lanDeck.SelectedIndex=0;lanClass.SelectedItem=Classes.First(c=>c.Key=="MAGE");BeginLan(true);guest.lanDeck.SelectedIndex=0;guest.lanClass.SelectedItem=Classes.First(c=>c.Key=="MAGE");guest.lanAddress.Text="127.0.0.1:"+lanPort.Text;guest.BeginLan(false);PumpLan(()=>lanReady&&guest.lanReady,"Real TCP spell-card peers connect");DoMatch("mulligan",selectedIds:new int[0]);PumpLan(()=>guest.match.Players[0].MulliganDone,"Host mulligan synchronizes");guest.DoMatch("mulligan",selectedIds:new int[0]);PumpLan(()=>match.MulliganComplete&&!guest.lanWaiting,"Guest mulligan synchronizes");DoMatch("end");PumpLan(()=>guest.match.Active==1,"Guest casting turn begins");
   var game=lanAuthority.Game;game.Players[1].Mana=game.Players[1].MaxMana=10;game.Players[1].Hand.Clear();int first=Inject(game,"CS2_024",1),second=Inject(game,"CS2_024",1);PublishLan();PumpLan(()=>guest.match.Players[1].Hand.Count==2,"Guest receives spell fixture");var receiver=(OfficeMatchVisualEffects)matchEffects;receiver.Clear();int shown=receiver.SpellCardCount;guest.BeginCard(first);guest.ClickMatchTarget(new MatchTarget(0));PumpLan(()=>!guest.lanWaiting&&receiver.SpellCardCount==shown+1,"Opponent with extras receives played spell card over TCP");guest.BeginCard(second);guest.ClickMatchTarget(new MatchTarget(0));PumpLan(()=>!guest.lanWaiting&&receiver.SpellCardCount==shown+2,"Second rapid spell appends rather than replaces");Check(receiver.ActiveSpellCards==2,"Both spells remain displayed concurrently");Render(Path.Combine(folder,"102-spell-cards-tcp.png"));PublishLan();PumpLan(()=>guest.match.Players[1].Hand.Count==0,"Repeated snapshot synchronizes");Check(receiver.SpellCardCount==shown+2,"Snapshot repeat never replays panels");
   ApplyTheme(false,false);Render(Path.Combine(folder,"103-spell-cards-wps.png"));Check(Width==548&&Height==474,"Spell cards preserve fixed normal window size");ApplyMode(true);Check(receiver.ActiveSpellCards==0,"Minimal receiver clears synchronized spell display");
  }finally{guest.LeaveLan();guest.Close();LeaveLan();}
 }
}
}
