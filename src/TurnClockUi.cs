using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
namespace Touchfish {
public partial class PanelWindow {
 DispatcherTimer turnClockTimer;TextBlock turnClockLabel;
 void BuildTurnClock(){turnClockTimer=new DispatcherTimer{Interval=TimeSpan.FromMilliseconds(100)};turnClockTimer.Tick+=(s,e)=>TickTurnClock();turnClockTimer.Start();Closed+=(s,e)=>turnClockTimer.Stop();}
 void TickTurnClock(){if(match==null)return;bool changed=false;if(lanMode){if(lanIsHost&&lanReady&&lanAuthority!=null&&lanPeer!=null&&lanPeer.IsConnected){changed=lanAuthority.PollTimeout();if(changed){lanRevision=lanAuthority.Revision;PublishLan(new ActionResult{Success=true,Message="倒计时结束 · 自动换边"});}}}else if(!match.Finished){changed=match.TryEndTimedTurn();if(changed){matchViewSeat=match.Active;handoffSeat=match.Active;awaitHandoff=false;}}
  if(changed){ClearPending();inspectMatch=testTakeOpen=emoteOpen=false;RenderMatch();status.Text="倒计时结束 · 自动换边";}UpdateTurnClockLabel();}
 void UpdateTurnClockLabel(){if(turnClockLabel==null||match==null)return;int remaining=match.TurnClockRemainingMs;turnClockLabel.Text=remaining<0?"":Math.Ceiling(remaining/1000.0)+"s";turnClockLabel.Visibility=remaining<0?Visibility.Collapsed:Visibility.Visible;}
}
}
