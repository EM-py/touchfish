using System;
using System.Windows.Threading;

namespace Touchfish {
public sealed partial class MatchEngine {
 public const int EntranceAnimationMs=2000;
 public bool EntranceAnimationsEnabled;
 public int AnimationSequence;
 DateTime animationUntil=DateTime.MinValue;
 public int AnimationRemainingMs{get{return Math.Max(0,(int)Math.Ceiling((animationUntil-DateTime.UtcNow).TotalMilliseconds));}}
 public bool AnimationPlaying{get{return AnimationRemainingMs>0;}}
 internal void StartEntranceWait(){if(!EntranceAnimationsEnabled)return;AnimationSequence++;animationUntil=DateTime.UtcNow.AddMilliseconds(EntranceAnimationMs);}
 internal void ReceiveAnimationWait(int sequence,int remaining){if(sequence!=AnimationSequence){AnimationSequence=sequence;animationUntil=DateTime.UtcNow.AddMilliseconds(remaining);}else if(remaining==0)animationUntil=DateTime.MinValue;}
 internal void LimitAnimationWait(int remaining){animationUntil=DateTime.UtcNow.AddMilliseconds(Math.Min(AnimationRemainingMs,remaining));}
}
public partial class PanelWindow {
 DispatcherTimer animationUnlockTimer;
 void NotifyPresentation(){if(match!=null&&!lanMode)match.EntranceAnimationsEnabled=fullEffects&&!minimal;if(!lanMode||!lanReady||lanPeer==null)return;if(lanIsHost&&lanAuthority!=null)lanAuthority.SetPresentation(0,fullEffects&&!minimal);else lanPeer.Send(new LanMessage{Kind="presentation",FullEffectsEnabled=fullEffects&&!minimal});}
 void RefreshAnimationWait(){if(match==null||!match.AnimationPlaying)return;livePrompt.Text="正在播放动画 · 请稍候";status.Text="正在播放动画 · 暂停操作";if(animationUnlockTimer==null){animationUnlockTimer=new DispatcherTimer{Interval=TimeSpan.FromMilliseconds(50)};animationUnlockTimer.Tick+=(s,e)=>{if(match==null||!match.AnimationPlaying){animationUnlockTimer.Stop();if(match!=null){if(lanIsHost&&lanAuthority!=null)PublishLan();RenderMatch();status.Text="动画结束";}}};}animationUnlockTimer.Start();}
}
}
