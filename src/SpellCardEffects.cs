using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace Touchfish {
public sealed partial class OfficeMatchVisualEffects {
 public const int SpellCardHoldMs=2000,SpellCardFadeMs=400;
 public int SpellCardCount;
 readonly List<Border> spellCards=new List<Border>();WrapPanel spellCardRow;
 public int ActiveSpellCards {get{return spellCards.Count;}}
 public void SpellCard(MatchVisualEvent item){
  if(!enabled()||!extraEffects()||String.IsNullOrEmpty(item.CardId))return;
  if(spellCardRow==null){spellCardRow=new WrapPanel{Width=Math.Max(100,overlay.ActualWidth-16),IsHitTestVisible=false};overlay.Children.Add(spellCardRow);Canvas.SetLeft(spellCardRow,8);Canvas.SetTop(spellCardRow,Math.Max(26,overlay.ActualHeight/2-55));}
  var body=new StackPanel();body.Children.Add(SpellCardText(item.CardName,11));
  body.Children.Add(SpellCardText("法术 · "+item.CardCost+"费"+(item.PaidCost==item.CardCost?"":" · 实付"+item.PaidCost),9));
  body.Children.Add(SpellCardText(item.CardText??"",10));
  var card=new Border{Width=120,Margin=new Thickness(2),Padding=new Thickness(6),Background=surface,BorderBrush=ink,BorderThickness=new Thickness(.5),Child=body};
  spellCards.Add(card);spellCardRow.Children.Add(card);SpellCardCount++;LayoutSpellCards();
  var fade=new DoubleAnimation(1,0,TimeSpan.FromMilliseconds(SpellCardFadeMs)){BeginTime=TimeSpan.FromMilliseconds(SpellCardHoldMs)};
  fade.Completed+=(s,e)=>{spellCards.Remove(card);if(spellCardRow!=null){spellCardRow.Children.Remove(card);LayoutSpellCards();}};card.BeginAnimation(UIElement.OpacityProperty,fade);
 }
 TextBlock SpellCardText(string text,double size){return new TextBlock{Text=text,FontSize=size,FontWeight=FontWeights.Normal,Foreground=ink,TextWrapping=TextWrapping.Wrap,Margin=new Thickness(0,0,0,3)};}
 void LayoutSpellCards(){if(spellCardRow==null)return;spellCardRow.Measure(new Size(spellCardRow.Width,Double.PositiveInfinity));Canvas.SetTop(spellCardRow,Math.Max(8,Math.Min(overlay.ActualHeight/2-55,overlay.ActualHeight-spellCardRow.DesiredSize.Height-8)));if(spellCards.Count==0){overlay.Children.Remove(spellCardRow);spellCardRow=null;}}
 void ClearSpellCards(){foreach(var card in spellCards)card.BeginAnimation(UIElement.OpacityProperty,null);spellCards.Clear();if(spellCardRow!=null)overlay.Children.Remove(spellCardRow);spellCardRow=null;}
}
}
