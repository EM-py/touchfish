using System;using System.Linq;using System.IO;using System.Diagnostics;using System.Net;using System.Net.Sockets;using System.Windows;
namespace Touchfish {public partial class PanelWindow {
 public void PreviewAttackScope(string folder){
  previewRun=true;Directory.CreateDirectory(folder);LeaveLan();ApplyTheme(true,true);ApplyMode(false);SetEffectsLevel(EffectLevel.Low);localTest=true;
  match=MatchFixture();var attacker=match.Summon(0,"VAN_CS2_182");attacker.SummonTurn=0;var target=match.Summon(1,"VAN_CS2_182");matchViewSeat=0;awaitHandoff=false;ShowPage("match");RenderMatch();UpdateLayout();
  var from=ResolveAttackAnchor(0,attacker.Id,previousVisualTargets);var to=ResolveAttackAnchor(1,target.Id,previousVisualTargets);var renderer=(OfficeMatchVisualEffects)matchEffects;int count=renderer.AttackCount;
  var item=new MatchVisualEvent{Kind="attack",Seat=0,UnitId=attacker.Id,TargetSeat=1,TargetUnitId=target.Id};
  renderer.Attack(item,from,to);Check(renderer.AttackCount==count+1&&renderer.ActiveAttacks==1&&from.Element.Opacity==0,"Low plays lunge and hides stationary source");
  var clock=Stopwatch.StartNew();PumpLan(()=>clock.ElapsedMilliseconds>=100,"Attack advances toward target");Render(Path.Combine(folder,"attack-lunge-normal.png"));
  PumpLan(()=>renderer.ActiveAttacks==0,"Attack completes return");Check(from.Element.Opacity==1,"Source opacity restored after attack");
  renderer.Attack(item,from,to);renderer.PrepareRedraw();Check(renderer.ActiveAttacks==0&&from.Element.Opacity==1,"Redraw safely cancels ghost and restores source");
  SetEffectsLevel(EffectLevel.Off);count=renderer.AttackCount;renderer.Attack(item,from,to);Check(renderer.AttackCount==count,"Off hides attack animation");
  SetEffectsLevel(EffectLevel.High);ApplyMode(true);renderer.Attack(item,from,to);Check(renderer.AttackCount==count&&renderer.ActiveAttacks==0,"Minimal suppresses high-tier attack animation");
  ApplyMode(false);SetEffectsLevel(EffectLevel.Low);match.Attack(0,attacker.Id,new MatchTarget(1,target.Id));RenderMatch();clock.Restart();PumpLan(()=>clock.ElapsedMilliseconds>=30,"Structured event reaches renderer");Check(renderer.AttackCount>count,"Combat event is rendered through public event pipeline");
  if(VerificationRun.Network){var listener=new TcpListener(IPAddress.Loopback,0);listener.Start();try{using(var client=new TcpClient()){client.Connect((IPEndPoint)listener.LocalEndpoint);using(var server=listener.AcceptTcpClient()){LanProtocol.WriteFrame(client.GetStream(),JsonData.Serializer().Serialize(LanProtocol.View(match,1,"attack-tcp",1)));var state=JsonData.Serializer().Deserialize<LanState>(LanProtocol.ReadFrame(server.GetStream()));var attack=state.VisualEvents.Single(e=>e.Kind=="attack");Check(attack.UnitId==attacker.Id&&attack.TargetUnitId==target.Id&&attack.TargetSeat==1&&state.Players[0].Hand.Length==0,"Attack endpoints survive TCP without hand leakage");var cursor=new MatchVisualCursor();Check(cursor.Take(state.VisualSession,state.VisualEvents,true).Count(e=>e.Kind=="attack")==1&&cursor.Take(state.VisualSession,state.VisualEvents,true).Length==0,"Repeated snapshots deduplicate attack animation");}}}finally{listener.Stop();}}
  renderer.Clear();Check(renderer.ActiveAttacks==0,"Clear releases attack animation");File.WriteAllText(Path.Combine(folder,"attack-verification.txt"),"PASS: lunge/return, tier and minimal gating, redraw cleanup, public events and optional loopback TCP. No physical mouse verification.");
 }
}}
