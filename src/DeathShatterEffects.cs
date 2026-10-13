using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;

namespace Touchfish {
public sealed partial class OfficeMatchVisualEffects {
 public const int DeathDurationMs=1500,MaxDeathFragments=240;
 public int DeathCount;public int DeathFragmentCount{get{return deathFragments.Count;}}
 readonly List<Image> deathFragments=new List<Image>();
 public static double DeathSpread(int overkill){return Math.Min(100,12+Math.Max(0,overkill)*8.0);}
 void ClearDeathFragments(){deathFragments.Clear();}
 public void Death(MatchVisualEvent item,MatchVisualAnchor anchor,TimeSpan delay,Action completed=null){
  if(!enabled()||!extraEffects()||item.UnitId<=0||anchor==null||anchor.Snapshot==null)return;
  var bitmap=anchor.Snapshot;if(bitmap.PixelWidth<3||bitmap.PixelHeight<4)return;DeathCount++;
  double spread=DeathSpread(item.Overkill);int start=Math.Max(0,(int)delay.TotalMilliseconds);int remaining=24;
  // Cut the existing office panel into triangles. No card art or game decoration.
  for(int row=0;row<4;row++)for(int col=0;col<3;col++)for(int triangle=0;triangle<2;triangle++){
   int left=col*bitmap.PixelWidth/3,top=row*bitmap.PixelHeight/4;
   int width=(col+1)*bitmap.PixelWidth/3-left,height=(row+1)*bitmap.PixelHeight/4-top;
   var crop=new CroppedBitmap(bitmap,new Int32Rect(left,top,width,height));crop.Freeze();
   double w=anchor.Bounds.Width*width/bitmap.PixelWidth,h=anchor.Bounds.Height*height/bitmap.PixelHeight;
   var points=triangle==0?new PointCollection{new Point(0,0),new Point(w,0),new Point(0,h)}:new PointCollection{new Point(w,0),new Point(w,h),new Point(0,h)};
   var shape=new StreamGeometry();using(var ctx=shape.Open()){ctx.BeginFigure(points[0],true,true);ctx.LineTo(points[1],true,false);ctx.LineTo(points[2],true,false);}shape.Freeze();
   var fragment=new Image{Source=crop,Width=w,Height=h,Clip=shape,IsHitTestVisible=false,Opacity=0};
   Canvas.SetLeft(fragment,anchor.Bounds.X+anchor.Bounds.Width*left/bitmap.PixelWidth);Canvas.SetTop(fragment,anchor.Bounds.Y+anchor.Bounds.Height*top/bitmap.PixelHeight);
   int index=(row*3+col)*2+triangle;double jitter=((item.Sequence*17+index*29)%23-11)*.022;
   double angle=Math.Atan2(row-1.5+(triangle==0?-.16:.16),col-1+(triangle==0?-.16:.16))+jitter;
   double distance=spread*(.72+(index*7%11)*.035),dx=Math.Cos(angle)*distance,dy=Math.Sin(angle)*distance;
   var rotate=new RotateTransform(0,w/2,h/2);var move=new TranslateTransform();var transforms=new TransformGroup();transforms.Children.Add(rotate);transforms.Children.Add(move);fragment.RenderTransform=transforms;
   while(deathFragments.Count>=MaxDeathFragments){overlay.Children.Remove(deathFragments[0]);deathFragments.RemoveAt(0);}
   deathFragments.Add(fragment);overlay.Children.Add(fragment);
   move.BeginAnimation(TranslateTransform.XProperty,FragmentMotion(dx,dx*1.07,start));
   move.BeginAnimation(TranslateTransform.YProperty,FragmentMotion(dy,dy+9,start));
   rotate.BeginAnimation(RotateTransform.AngleProperty,new DoubleAnimation(0,(index%2==0?-1:1)*(9+spread*.28),TimeSpan.FromMilliseconds(650)){BeginTime=TimeSpan.FromMilliseconds(start)});
   var opacity=new DoubleAnimationUsingKeyFrames{Duration=TimeSpan.FromMilliseconds(DeathDurationMs),BeginTime=TimeSpan.FromMilliseconds(start)};
   opacity.KeyFrames.Add(new DiscreteDoubleKeyFrame(1,KeyTime.FromTimeSpan(TimeSpan.Zero)));
   opacity.KeyFrames.Add(new LinearDoubleKeyFrame(.85,KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(900))));
   opacity.KeyFrames.Add(new LinearDoubleKeyFrame(0,KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(DeathDurationMs))));
   opacity.Completed+=(s,e)=>{overlay.Children.Remove(fragment);deathFragments.Remove(fragment);if(--remaining==0&&completed!=null)completed();};fragment.BeginAnimation(UIElement.OpacityProperty,opacity);
  }
 }
 static DoubleAnimationUsingKeyFrames FragmentMotion(double burst,double finish,int delay){var a=new DoubleAnimationUsingKeyFrames{Duration=TimeSpan.FromMilliseconds(DeathDurationMs),BeginTime=TimeSpan.FromMilliseconds(delay)};a.KeyFrames.Add(new LinearDoubleKeyFrame(0,KeyTime.FromTimeSpan(TimeSpan.Zero)));a.KeyFrames.Add(new EasingDoubleKeyFrame(burst,KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(650)),new CubicEase{EasingMode=EasingMode.EaseOut}));a.KeyFrames.Add(new LinearDoubleKeyFrame(finish,KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(DeathDurationMs))));return a;}
}
}
