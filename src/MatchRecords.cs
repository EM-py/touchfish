using System;
using System.Linq;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace Touchfish {
public partial class PanelWindow {
 readonly List<string> localRecords=new List<string>(),lastMatchRecords=new List<string>();StackPanel recordRows;TextBlock recordScope;ScrollViewer recordScroll;string renderedRecords="",lastRecordScope="";
 void BuildRecords(){logPanel.Children.Clear();logPanel.RowDefinitions.Add(new RowDefinition{Height=new GridLength(22)});logPanel.RowDefinitions.Add(new RowDefinition{Height=new GridLength(20)});logPanel.RowDefinitions.Add(new RowDefinition());logPanel.Children.Add(T("记录",12,Ink));recordScope=T("本地操作",9,Muted);Put(logPanel,recordScope,1);recordRows=new StackPanel();recordScroll=new ScrollViewer{Content=recordRows,VerticalScrollBarVisibility=ScrollBarVisibility.Auto,HorizontalScrollBarVisibility=ScrollBarVisibility.Disabled};Put(logPanel,recordScroll,2);RefreshRecords();}
 void Record(string text){localRecords.Add(DateTime.Now.ToString("HH:mm")+"  "+text);if(localRecords.Count>100)localRecords.RemoveAt(0);RefreshRecords();}
 void RefreshRecords(){if(recordRows==null)return;if(match!=null){lastMatchRecords.Clear();lastMatchRecords.AddRange(match.Log);lastRecordScope=lanMode?"双方共享 · 对局记录":"本地 test · 对局记录";}var history=match!=null?match.Log:lastMatchRecords.Count>0?lastMatchRecords:localRecords;string scope=match!=null?lastRecordScope:lastMatchRecords.Count>0?"上局 · "+lastRecordScope:"本地操作";recordScope.Text=scope+" · "+history.Count+" 条";string key=scope+"\n"+String.Join("\n",history);if(key==renderedRecords)return;renderedRecords=key;double offset=recordScroll.VerticalOffset;bool atEnd=offset>=recordScroll.ScrollableHeight-1;recordRows.Children.Clear();if(history.Count==0)recordRows.Children.Add(T("暂无记录。",10,Muted));else for(int i=0;i<history.Count;i++){var text=T((i+1).ToString("D3")+"  "+history[i],10,Ink);text.TextWrapping=TextWrapping.Wrap;text.Margin=new Thickness(0,0,0,6);recordRows.Children.Add(text);}ApplyFonts(logPanel);recordScroll.Dispatcher.BeginInvoke(new Action(()=>{if(atEnd)recordScroll.ScrollToEnd();else recordScroll.ScrollToVerticalOffset(offset);}));}
 string CardDetailMetadata(CardRecord card){return card.Cost+"费 · "+(card.Type=="MINION"?"随从 · 攻击 "+card.Attack+" · 生命 "+card.Health:card.Type=="WEAPON"?"武器 · 攻击 "+card.Attack+" · 耐久 "+card.Durability:"法术")+(String.IsNullOrEmpty(card.Race)?"":" · 种族："+RaceLabel(card.Race));}
 static string RaceLabel(string race){switch(race){case "BEAST":return "野兽";case "DEMON":return "恶魔";case "DRAGON":return "龙";case "MECHANICAL":return "机械";case "MURLOC":return "鱼人";case "PIRATE":return "海盗";case "TOTEM":return "图腾";case "UNDEAD":return "亡灵";case "ELEMENTAL":return "元素";case "NAGA":return "娜迦";case "QUILBOAR":return "野猪人";case "ALL":return "全部";default:return race;}}
}
}
