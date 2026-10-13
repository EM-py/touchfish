using System;
using System.Linq;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Controls;

namespace Touchfish {
// One bounded vector surface, drawn by WPF's render clock; no external textures.
sealed class PyroblastSurface : FrameworkElement {
 readonly Point source,target;readonly Stopwatch watch=Stopwatch.StartNew();readonly int elapsedOffset;
 readonly Brush[] flames={B(255,246,169),B(255,169,42),B(245,76,15),B(155,28,8)};
 readonly RadialGradientBrush glow=new RadialGradientBrush(Color.FromArgb(210,255,151,30),Color.FromArgb(0,230,45,8));
 readonly RadialGradientBrush core=new RadialGradientBrush(Color.FromRgb(255,252,205),Color.FromRgb(244,69,8));
 bool hit,disposed;double hitAt;readonly Action finished;
 public PyroblastSurface(Point start,Point end,int elapsed,Action complete){source=start;target=end;elapsedOffset=elapsed;finished=complete;IsHitTestVisible=false;glow.Freeze();core.Freeze();CompositionTarget.Rendering+=Frame;}
 static Brush B(byte r,byte g,byte b){var brush=new SolidColorBrush(Color.FromRgb(r,g,b));brush.Freeze();return brush;}
 void Frame(object sender,EventArgs args){if(disposed)return;if(hit&&watch.Elapsed.TotalMilliseconds-hitAt>600||!hit&&watch.Elapsed.TotalMilliseconds+elapsedOffset>2600){Dispose();finished();return;}InvalidateVisual();}
 public void Hit(){if(hit)return;hit=true;hitAt=watch.Elapsed.TotalMilliseconds;InvalidateVisual();}
 public void Dispose(){if(disposed)return;disposed=true;CompositionTarget.Rendering-=Frame;}
 Point Position(double phase){double p=phase*phase;return new Point(source.X+(target.X-source.X)*p,source.Y+(target.Y-source.Y)*p-Math.Sin(phase*Math.PI)*28);}
 protected override void OnRender(DrawingContext d){base.OnRender(d);double ms=watch.Elapsed.TotalMilliseconds+elapsedOffset,t=ms/1000;
  if(hit){double q=Math.Min(1,(watch.Elapsed.TotalMilliseconds-hitAt)/600);d.PushOpacity(1-q);d.DrawEllipse(glow,null,target,28+q*90,24+q*70);var ring=new Pen(flames[1],Math.Max(.5,5*(1-q)));d.DrawEllipse(null,ring,target,12+q*70,10+q*54);for(int i=0;i<54;i++){double angle=i*2.399963,r=(18+q*(45+i%13*4));Point p=new Point(target.X+Math.Cos(angle)*r,target.Y+Math.Sin(angle)*r*.8);Flame(d,p,3+i%3,angle,6+q*15,t+i*.13,i%3);}d.Pop();return;}
  double phase=Math.Max(0,Math.Min(.998,(ms-260)/(MatchEngine.PyroblastImpactMs-260.0)));Point ball=Position(phase);double charge=Math.Min(1,ms/260),radius=10+charge*15;
  if(phase>0){for(int i=32;i>=0;i--){double back=i/180.0;Point p=Position(Math.Max(0,phase-back));double fade=1-i/34.0;d.PushOpacity(fade*.75);d.DrawEllipse(glow,null,p,8+fade*23,7+fade*22);Flame(d,p,3+fade*8,Math.Atan2(source.Y-target.Y,source.X-target.X),14+fade*20,t+i*.22,i%3);d.Pop();}}
  d.DrawEllipse(glow,null,ball,radius*2.4,radius*2.4);
  for(int i=0;i<43;i++){double angle=i*2.399963+Math.Sin(t*7+i)*.3;double wave=Math.Sin(t*19+i*2.7);Point p=new Point(ball.X+Math.Cos(angle)*radius*.65,ball.Y+Math.Sin(angle)*radius*.65);double burnDirection=(phase>.03?Math.Atan2(source.Y-target.Y,source.X-target.X):-Math.PI/2)+Math.Sin(t*9+i*1.7)*.9;Flame(d,p,2+i%7,burnDirection,radius*(.4+(i*13%17)/17.0)+wave*7+7,t+i*.11,i%3);}
  d.DrawEllipse(core,null,ball,radius*(.61+Math.Sin(t*21)*.04),radius*(.66+Math.Cos(t*17)*.04));
 }
 void Flame(DrawingContext d,Point p,double width,double angle,double length,double time,int color){double ax=Math.Cos(angle),ay=Math.Sin(angle),nx=-ay,ny=ax;double curl=Math.Sin(time*15)*width;
  var shape=new StreamGeometry();using(var c=shape.Open()){c.BeginFigure(new Point(p.X+nx*width,p.Y+ny*width),true,true);c.BezierTo(new Point(p.X+ax*length*.4+nx*width,p.Y+ay*length*.4+ny*width),new Point(p.X+ax*length+nx*curl,p.Y+ay*length+ny*curl),new Point(p.X+ax*length,p.Y+ay*length),true,false);c.BezierTo(new Point(p.X+ax*length*.4-nx*width,p.Y+ay*length*.4-ny*width),new Point(p.X-nx*width,p.Y-ny*width),p,true,false);}d.DrawGeometry(flames[color],null,shape);
 }
}
public sealed partial class OfficeMatchVisualEffects {
 readonly Dictionary<int,PyroblastSurface> pyroblasts=new Dictionary<int,PyroblastSurface>();
 public int PyroblastCount,PyroblastImpactCount;public int ActivePyroblasts{get{return pyroblasts.Count;}}
 public void Pyroblast(MatchVisualEvent item,MatchVisualAnchor from,MatchVisualAnchor to,int elapsed){if(!enabled()||!highEffects()||from==null||to==null)return;if(pyroblasts.ContainsKey(item.Action))return;
  PyroblastSurface surface=null;surface=new PyroblastSurface(new Point(from.Bounds.X+from.Bounds.Width/2,from.Bounds.Y+from.Bounds.Height/2),new Point(to.Bounds.X+to.Bounds.Width/2,to.Bounds.Y+to.Bounds.Height/2),elapsed,()=>{overlay.Children.Remove(surface);if(pyroblasts.ContainsKey(item.Action)&&pyroblasts[item.Action]==surface)pyroblasts.Remove(item.Action);});surface.Width=overlay.ActualWidth;surface.Height=overlay.ActualHeight;Panel.SetZIndex(surface,50);pyroblasts[item.Action]=surface;overlay.Children.Add(surface);PyroblastCount++;
 }
 public void PyroblastImpact(MatchVisualEvent item){PyroblastSurface surface;if(!pyroblasts.TryGetValue(item.Action,out surface))return;surface.Hit();PyroblastImpactCount++;}
 public bool IsPyroblast(int action){return pyroblasts.ContainsKey(action);}
 void ClearPyroblasts(){foreach(var surface in pyroblasts.Values){surface.Dispose();overlay.Children.Remove(surface);}pyroblasts.Clear();}
 void HeavyImpact(){var move=new TranslateTransform();var group=new TransformGroup();group.Children.Add(move);screen.RenderTransform=group;cinematicUntil=DateTime.UtcNow.AddMilliseconds(450);
  var x=new DoubleAnimationUsingKeyFrames{Duration=TimeSpan.FromMilliseconds(420),FillBehavior=FillBehavior.Stop};var y=new DoubleAnimationUsingKeyFrames{Duration=TimeSpan.FromMilliseconds(420),FillBehavior=FillBehavior.Stop};double[] offsets={0,8,-6,4,-3,1.5,0};for(int i=0;i<offsets.Length;i++){x.KeyFrames.Add(new LinearDoubleKeyFrame(offsets[i],KeyTime.FromPercent(i/6.0)));y.KeyFrames.Add(new LinearDoubleKeyFrame(offsets[i]*.65,KeyTime.FromPercent(i/6.0)));}move.BeginAnimation(TranslateTransform.XProperty,x);move.BeginAnimation(TranslateTransform.YProperty,y);
 }
}
}
