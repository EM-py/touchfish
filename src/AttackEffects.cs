using System;using System.Collections.Generic;using System.Windows;using System.Windows.Controls;using System.Windows.Media;using System.Windows.Media.Animation;using System.Windows.Media.Imaging;
namespace Touchfish {
public sealed partial class OfficeMatchVisualEffects {
 public const int AttackImpactMs=140,AttackDurationMs=360;public int AttackCount;public int ActiveAttacks{get{return attacks.Count;}}
 sealed class AttackFlight {public FrameworkElement Ghost,Source;public double Opacity;}
 readonly List<AttackFlight> attacks=new List<AttackFlight>();
 public void PrepareRedraw(){ClearAttacks();}
 void ClearAttacks(){foreach(var flight in attacks.ToArray())FinishAttack(flight);}
 void FinishAttack(AttackFlight flight){if(!attacks.Remove(flight))return;if(flight.Source!=null)flight.Source.Opacity=flight.Opacity;overlay.Children.Remove(flight.Ghost);}
 public void Attack(MatchVisualEvent item,MatchVisualAnchor from,MatchVisualAnchor to){
  if(!enabled()||!extraEffects()||from==null||to==null||from.Bounds.Width<=0||from.Bounds.Height<=0)return;
  Image ghost=new Image{Width=from.Bounds.Width,Height=from.Bounds.Height,IsHitTestVisible=false};
  if(from.Snapshot!=null)ghost.Source=from.Snapshot;
  else if(from.Element!=null){var bitmap=new RenderTargetBitmap(Math.Max(1,(int)Math.Ceiling(ghost.Width)),Math.Max(1,(int)Math.Ceiling(ghost.Height)),96,96,PixelFormats.Pbgra32);var drawing=new DrawingVisual();using(var context=drawing.RenderOpen())context.DrawRectangle(new VisualBrush(from.Element){Stretch=Stretch.Fill},null,new Rect(0,0,ghost.Width,ghost.Height));bitmap.Render(drawing);ghost.Source=bitmap;}
  else return;
  var flight=new AttackFlight{Ghost=ghost,Source=from.Element,Opacity=from.Element==null?1:from.Element.Opacity};
  // The snapshot is taken before the source is hidden; originals are restored on redraw or clear.
  if(from.Element!=null)from.Element.Opacity=0;
  AttackCount++;attacks.Add(flight);overlay.Children.Add(ghost);Canvas.SetLeft(ghost,from.Bounds.X);Canvas.SetTop(ghost,from.Bounds.Y);
  double dx=to.Bounds.X+to.Bounds.Width/2-from.Bounds.X-from.Bounds.Width/2,dy=to.Bounds.Y+to.Bounds.Height/2-from.Bounds.Y-from.Bounds.Height/2;
  var move=new TranslateTransform();ghost.RenderTransform=move;bool returns=from.Element!=null;int duration=returns?AttackDurationMs:AttackImpactMs+20;
  var x=AttackAxis(dx,duration,returns);var y=AttackAxis(dy,duration,returns);y.Completed+=(s,e)=>FinishAttack(flight);
  move.BeginAnimation(TranslateTransform.XProperty,x);move.BeginAnimation(TranslateTransform.YProperty,y);
 }
 static DoubleAnimationUsingKeyFrames AttackAxis(double distance,int duration,bool returns){
  var animation=new DoubleAnimationUsingKeyFrames{Duration=TimeSpan.FromMilliseconds(duration),FillBehavior=FillBehavior.Stop};
  animation.KeyFrames.Add(new LinearDoubleKeyFrame(0,KeyTime.FromTimeSpan(TimeSpan.Zero)));
  animation.KeyFrames.Add(new EasingDoubleKeyFrame(distance*.86,KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(AttackImpactMs)),new CubicEase{EasingMode=EasingMode.EaseIn}));
  animation.KeyFrames.Add(new LinearDoubleKeyFrame(distance*.86,KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(AttackImpactMs+20))));
  if(returns)animation.KeyFrames.Add(new EasingDoubleKeyFrame(0,KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(duration)),new CubicEase{EasingMode=EasingMode.EaseOut}));
  return animation;
 }
}
}
