using System;


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
}
