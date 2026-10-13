using System;
using System.IO;
using System.Linq;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Windows;
namespace Touchfish {
public partial class PanelWindow {
 public void PreviewDeathShatterScope(string folder){
  previewRun=true;Directory.CreateDirectory(folder);int before=checks;
  var game=MatchFixture();var exact=game.Summon(1,"VAN_CS2_182");var over=game.Summon(1,"VAN_CS2_182");game.SetHealth(new MatchTarget(1,exact.Id),3);game.SetHealth(new MatchTarget(1,over.Id),3);
  game.Damage(new MatchTarget(1,exact.Id),3);game.Damage(new MatchTarget(1,over.Id),10);game.Cleanup();var deaths=game.VisualEvents.Where(e=>e.Kind=="death").ToArray();
  Check(deaths.Length==2&&deaths[0].Amount==3&&deaths[0].HealthBefore==3&&deaths[0].Overkill==0&&deaths[1].Amount==10&&deaths[1].Overkill==7,"Death event captures actual lethal hit and excess damage");
  Check(deaths[0].Slot==0&&deaths[1].Slot==1,"Simultaneous deaths retain original slots");
  var remote=LanProtocol.ReadView(catalog,LanProtocol.Copy(LanProtocol.View(game,0,"shatter",1)));Check(remote.VisualEvents.Last().Kind=="death"&&remote.VisualEvents.Last().Overkill==7&&remote.Players[1].Hand.Count==0,"Death metadata survives private LAN snapshot serialization");
  var cursor=new MatchVisualCursor();Check(cursor.Take(remote.VisualSession,remote.VisualEvents,true).Count(e=>e.Kind=="death")==2&&cursor.Take(remote.VisualSession,remote.VisualEvents,true).Length==0,"Repeated snapshots do not repeat death effects");
  if(VerificationRun.Network){var listener=new TcpListener(IPAddress.Loopback,0);listener.Start();try{using(var client=new TcpClient()){client.Connect((IPEndPoint)listener.LocalEndpoint);using(var server=listener.AcceptTcpClient()){LanProtocol.WriteFrame(client.GetStream(),JsonData.Serializer().Serialize(LanProtocol.View(game,0,"death-tcp",1)));var received=JsonData.Serializer().Deserialize<LanState>(LanProtocol.ReadFrame(server.GetStream()));Check(received.VisualEvents.Count(e=>e.Kind=="death")==2&&received.VisualEvents.Last().Overkill==7&&received.Players[1].Hand.Length==0,"Death metadata and hidden-hand privacy survive loopback TCP framing");}}}finally{listener.Stop();}}
  game=MatchFixture();var shield=game.Summon(1,"VAN_EX1_008");game.Damage(new MatchTarget(1,shield.Id),10);game.Cleanup();Check(!game.VisualEvents.Any(e=>e.Kind=="death"),"Shield prevents death and shatter");shield.DamageTaken=shield.MaxHealth;game.Cleanup();Check(game.VisualEvents.Last().Kind=="death"&&game.VisualEvents.Last().Amount==0&&game.VisualEvents.Last().Overkill==0,"Direct destruction uses default light fracture");
  game=MatchFixture();var living=game.Summon(1,"VAN_CS2_182");game.Damage(new MatchTarget(1,living.Id),2);game.Cleanup();Check(!game.VisualEvents.Any(e=>e.Kind=="death"),"Nonlethal hit never shatters");living.DamageTaken=living.MaxHealth;game.Cleanup();Check(game.VisualEvents.Last().Amount==0,"Old nonlethal hit does not amplify later destruction");
  Check(OfficeMatchVisualEffects.DeathSpread(7)-OfficeMatchVisualEffects.DeathSpread(0)==56&&OfficeMatchVisualEffects.DeathSpread(1000)==100,"Linear excess-damage spread has a finite cap");
  var layout=new MatchPresentationLayout();var heldDeath=new MatchVisualEvent{Sequence=1,Kind="death",Seat=1,UnitId=1};layout.Prepare("layout",new System.Collections.Generic.Dictionary<int,int[]>{{1,new[]{1,2,3,4,5,6,7}}},new[]{heldDeath},true);var heldSlots=layout.Slots(1,new[]{8,2,3,4,5,6,7});Check(heldSlots[0]==0&&heldSlots[1]==2&&heldSlots[6]==7&&!heldSlots.Contains(8),"Full-board death reserves its slot and queues arrivals without moving survivors");layout.Complete("old-session",1);Check(layout.Reserved(1,0),"Old animation completion cannot release current slots");layout.Complete("layout",1);Check(layout.Slots(1,new[]{8,2,3,4,5,6,7})[0]==8,"Layout follows authoritative order only after completion");layout.Clear();
  if(VerificationRun.Visual){
   LeaveLan();ApplyTheme(true,true);ApplyMode(false);SetFullEffects(true);localTest=true;match=MatchFixture();matchViewSeat=0;awaitHandoff=false;ClearPending();
   var first=match.Summon(1,"VAN_CS2_182");var second=match.Summon(1,"VAN_CS2_182");match.SetHealth(new MatchTarget(1,first.Id),3);match.SetHealth(new MatchTarget(1,second.Id),3);var survivor=match.Summon(1,"VAN_CS2_200");match.Summon(0,"VAN_CS2_182");RenderMatch();ShowPage("match");UpdateLayout();Render(Path.Combine(folder,"death-before.png"));
   var renderer=(OfficeMatchVisualEffects)matchEffects;renderer.Clear();int initial=renderer.DeathCount;match.Damage(new MatchTarget(1,first.Id),3);match.Damage(new MatchTarget(1,second.Id),10);match.Cleanup();RenderMatch();PumpLan(()=>renderer.DeathCount==initial+2,"Both lethal hits render their own fractures");
   Check(renderer.DeathFragmentCount==48&&!matchEffectsCanvas.IsHitTestVisible&&!match.AnimationPlaying,"Fragments are bounded, noninteractive and do not lock actions");
   Check(System.Windows.Controls.Grid.GetColumn(visualTargets[VisualKey(1,survivor.Id)])==2&&presentationLayout.Reserved(1,0)&&presentationLayout.Reserved(1,1),"Survivor stays in its original slot while neighboring deaths animate");RenderMatch();Check(System.Windows.Controls.Grid.GetColumn(visualTargets[VisualKey(1,survivor.Id)])==2,"Selection or snapshot redraw cannot compact held slots");
   var watch=Stopwatch.StartNew();for(int frame=0;frame<18;frame++){int target=frame*90;PumpLan(()=>watch.ElapsedMilliseconds>=target,"Advance real WPF shatter frame");Render(Path.Combine(folder,"frame-"+frame.ToString("D2")+".png"));}
   PumpLan(()=>renderer.DeathFragmentCount==0,"Fragments disappear after their animation lifetime");Render(Path.Combine(folder,"frame-18.png"));
   Check(System.Windows.Controls.Grid.GetColumn(visualTargets[VisualKey(1,survivor.Id)])==0,"Survivor compacts after both death animations complete");
   SetFullEffects(false);renderer.Death(deaths[0],new MatchVisualAnchor(),TimeSpan.Zero);Check(renderer.DeathCount==initial+2,"Disabled extras do not shatter");ApplyMode(true);SetFullEffects(true);renderer.Death(deaths[0],new MatchVisualAnchor(),TimeSpan.Zero);Check(renderer.DeathCount==initial+2&&matchEffectsCanvas.Children.Count==0,"Minimal mode remains silent with extras enabled");ApplyMode(false);SetFullEffects(false);
  }
  File.WriteAllText(Path.Combine(folder,"death-shatter-verification.txt"),"PASS: "+(checks-before)+" checks; actual WPF frames, lethal metadata, original death slots, LAN serialization and duplicate suppression. No user data writes or physical mouse claims.");
 }
}
}
