using System;
using System.IO;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using System.Windows.Documents;
using System.ComponentModel;
using System.Windows.Data;
using Polygon = System.Windows.Shapes.Polygon;

namespace Touchfish {public partial class PanelWindow {
 int checks;
 public void PreviewSuite(string folder){
  previewRun=true;ApplyTheme(false,false);ApplyMode(false);ShowPage("game");if(details)ToggleDetails();Directory.CreateDirectory(folder);Render(Path.Combine(folder,"01-standard.png"));Render(Path.Combine(folder,"03-compact.png"));
  Check(Width==548&&Height==474&&!details,"Default compact size");Check(battleCards.Count==14,"Seven minions per side");
  foreach(var card in battleCards){var bounds=card.TransformToAncestor(main).TransformBounds(new Rect(0,0,card.ActualWidth,card.ActualHeight));Check(bounds.Left>=-1&&bounds.Right<=main.ActualWidth+1,"Battle card fits width");var label=(StackPanel)((Grid)card.Child).Children[1];var name=(TextBlock)label.Children[0];Check(name.TextWrapping==TextWrapping.Wrap,"Name wraps");Check(label.DesiredSize.Height<=53,"Name and keywords fit vertically");}
  Select((Entry)battleCards[3].Tag);UpdateLayout();Render(Path.Combine(folder,"02-details.png"));Render(Path.Combine(folder,"04-compact-details.png"));Check(Height==474&&Width==548,"Inspection preserves size");Check(handLane.Visibility==Visibility.Collapsed,"Inspector replaces hand area");
  foreach(var row in rows){Select((Entry)row.Tag);Check(detailTitle.Text==((Entry)row.Tag).Name,"Selection");}
  ToggleDetails();Check(handLane.Visibility==Visibility.Visible,"Hand restored");ToggleBoard();UpdateLayout();Render(Path.Combine(folder,"06-three-minions.png"));Check(battleCards.Count==6,"Three minions with reserved slots");ToggleBoard();
  SwitchTab(true);UpdateLayout();Render(Path.Combine(folder,"05-log.png"));SwitchTab(false);Check(main.Visibility==Visibility.Visible,"Tab restoration");
  Check(heroSlots.Count==6&&crystals.Count==20,"Hero and mana slots");Check(Grid.GetRow(battleLanes[0])==2&&Grid.GetRow(battleLanes[1])==4,"Battlefield order");CheckNormalWeights(this);
  ApplyTheme(true,false);Render(Path.Combine(folder,"08-codex-normal-light.png"));Check(!dark&&codex,"Codex light theme");
  ApplyTheme(true,true);Render(Path.Combine(folder,"09-codex-normal-dark.png"));Check(dark&&codex,"Codex dark theme");
  ApplyMode(true);Render(Path.Combine(folder,"10-codex-minimal-dark.png"));Check(Width==392&&Height==300,"Minimal dimensions");CheckBoardFit();foreach(var card in battleCards){var label=(StackPanel)((Grid)card.Child).Children[1];Check(((TextBlock)label.Children[0]).Text!=((Entry)card.Tag).Name,"Minimal hides names");Check(label.Children[1] is Button,"Small detail entry");}foreach(Border slot in heroSlots){var entry=(Entry)slot.Tag;Check(((TextBlock)((StackPanel)slot.Child).Children[0]).Text==(entry.Kind=="英雄"?entry.Health.ToString():entry.Kind=="武器"?"武":"技"),"Minimal hero text");}
  ShowPage("settings");Render(Path.Combine(folder,"11-settings-dark.png"));Check(settingsPanel.Visibility==Visibility.Visible&&main.Visibility==Visibility.Collapsed,"Settings page");
  ShowPage("game");Select((Entry)battleCards[6].Tag);Render(Path.Combine(folder,"12-codex-minimal-details.png"));Check(Width==392&&Height==300,"Minimal inspector size");if(details)ToggleDetails();
  ApplyTheme(true,false);Render(Path.Combine(folder,"13-codex-minimal-light.png"));ApplyTheme(false,false);Render(Path.Combine(folder,"07-wps-minimal.png"));CheckBoardFit();
  ShowPage("settings");Render(Path.Combine(folder,"14-settings-light.png"));Check(!lightOption.IsEnabled&&!darkOption.IsEnabled,"WPS appearance controls disabled");ShowPage("game");
  ApplyMode(false);Check(Width==548&&Height==474,"Normal restoration");CheckBoardFit();CheckNormalWeights(this);
  VerifyLibrary(folder);
  VerifyMatch(folder);VerifyLocalTestTools(folder);
  VerifyLan(folder);VerifyUpdates(folder);VerifyVisualEffects(folder);ApplyTheme(true,true);ApplyMode(true);OpenReleaseNotes();Render(Path.Combine(folder,"47-release-notes-codex.png"));ApplyTheme(false,false);ApplyMode(false);OpenReleaseNotes();Render(Path.Combine(folder,"48-release-notes-wps.png"));RenderExpandedRelease(folder);ShowPage("lan");
  File.WriteAllText(Path.Combine(folder,"verification.txt"),"PASS: "+checks+" assertions. Classic collection and deckcodes; local test engine, LAN host authority and two WPF peers over real TCP; attacks, spells, powers, placement, privacy, revision rejection, disconnects; themes and layouts.\r\nActual WPF visual-tree renders at 96 DPI. Rule coverage is partial. Automated network checks use loopback sockets. User reported a working LAN session with a friend; original game client comparisons were not performed.\r\n");
 }
 void CheckBoardFit(){UpdateLayout();Check(battleCards.Count==14,"Fourteen visible minions");foreach(var card in battleCards){var bounds=card.TransformToAncestor(main).TransformBounds(new Rect(0,0,card.ActualWidth,card.ActualHeight));Check(bounds.Left>=-1&&bounds.Right<=main.ActualWidth+1&&bounds.Bottom<=main.ActualHeight+1,"Card in viewport");var label=(StackPanel)((Grid)card.Child).Children[1];Check(label.DesiredSize.Height<=card.ActualHeight-4,"Wrapping fits card");}foreach(var slot in heroSlots){var bounds=slot.TransformToAncestor(main).TransformBounds(new Rect(0,0,slot.ActualWidth,slot.ActualHeight));Check(bounds.Bottom<=main.ActualHeight+1,"Hero component in viewport");}Check(crystals.Count==20,"Twenty crystals");CheckNormalWeights(this);}
 void Check(bool condition,string message){if(!condition)throw new Exception(message);checks++;}
 void CheckNormalWeights(DependencyObject node){if(!VerificationRun.Visual)return;var text=node as TextBlock;if(text!=null)Check(text.FontWeight==FontWeights.Normal,"Text weight");var control=node as Control;if(control!=null)Check(control.FontWeight==FontWeights.Normal,"Control weight");for(int i=0;i<VisualTreeHelper.GetChildrenCount(node);i++)CheckNormalWeights(VisualTreeHelper.GetChild(node,i));}
}}