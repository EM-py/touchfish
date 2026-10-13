using System;
using System.Windows.Threading;
namespace Touchfish {
public partial class PanelWindow {
 DispatcherTimer animationUnlockTimer;
 void NotifyPresentation(){if(match!=null&&!lanMode)match.SetAnimationEffects(CurrentEffects);if(!lanMode||(!lanReady&&lanArenaView==null)||lanPeer==null)return;if(lanIsHost&&lanAuthority!=null)lanAuthority.SetPresentation(0,CurrentEffects);else lanPeer.Send(new LanMessage{Kind="presentation",EffectsLevel=(int)CurrentEffects});}
 void RefreshAnimationWait(){if(match==null||!match.AnimationPlaying)return;livePrompt.Text="正在播放动画 · 请稍候";status.Text="正在播放动画 · 暂停操作";if(animationUnlockTimer==null){animationUnlockTimer=new DispatcherTimer{Interval=TimeSpan.FromMilliseconds(50)};animationUnlockTimer.Tick+=(s,e)=>{if(match==null||!match.AnimationPlaying){animationUnlockTimer.Stop();if(match!=null){if(lanIsHost&&lanAuthority!=null)PublishLan();RenderMatch();status.Text="动画结束";}}};}animationUnlockTimer.Start();}
}
}
