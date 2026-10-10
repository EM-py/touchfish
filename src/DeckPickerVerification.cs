using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
namespace Touchfish {
public partial class PanelWindow {
 public void PreviewDeckPickerScope(string folder){
  previewRun=true;Directory.CreateDirectory(folder);int before=checks;var mage=ExampleDeck("MAGE");var warrior=ExampleDeck("WARRIOR");
  savedDecks=new List<SavedDeck>{new SavedDeck{Id="picker-mage",Name="经典法师",PoolId=mage.PoolId,Code=DeckCode.Encode(catalog,mage)},new SavedDeck{Id="picker-warrior",Name="经典战士",PoolId=warrior.PoolId,Code=DeckCode.Encode(catalog,warrior)},new SavedDeck{Id="picker-invalid",Name="损坏代码",PoolId=mage.PoolId,Code="invalid"}};
  draft=new DeckDocument{PoolId=mage.PoolId};SyncDeckControls();RefreshDeck();ShowPage("deck");
  savedDeckPicker.SelectedItem=((Choice[])savedDeckPicker.ItemsSource).First(c=>c.Key=="picker-mage");Check(draft.Id=="picker-mage"&&draft.ClassId=="MAGE"&&SameDeck(draft,mage)&&codePanel==null,"Direct selector loads exact saved deck and keeps builder visible");
  deckNameBox.Text="编辑后的法师";SaveFinishedDeck();Check(savedDecks.Count==3&&savedDecks[0].Name=="编辑后的法师"&&((Choice)savedDeckPicker.SelectedItem).Key==draft.Id,"Save edits existing identity and updates selector name");
  savedDeckPicker.SelectedItem=((Choice[])savedDeckPicker.ItemsSource).First(c=>c.Key=="picker-warrior");Check(draft.Id=="picker-warrior"&&draft.ClassId=="WARRIOR"&&SameDeck(draft,warrior),"Selecting another class loads correct profession and cards");
  var prior=draft.Copy();savedDeckPicker.SelectedItem=((Choice[])savedDeckPicker.ItemsSource).First(c=>c.Key=="picker-invalid");Check(draft.Id==prior.Id&&SameDeck(draft,prior)&&((Choice)savedDeckPicker.SelectedItem).Key==prior.Id,"Invalid saved code preserves editor and restores selector");
  ApplyTheme(false,false);ApplyMode(false);Render(Path.Combine(folder,"106-deck-direct-select-wps.png"));ApplyTheme(true,true);ApplyMode(true);Render(Path.Combine(folder,"107-deck-direct-select-minimal.png"));Check(Width==392&&Height==300&&savedDeckPicker.ActualWidth>100,"Selector fits unchanged minimal window");
  RemoveSavedDeck("picker-warrior");Check(!((Choice[])savedDeckPicker.ItemsSource).Any(c=>c.Key=="picker-warrior")&&((Choice)savedDeckPicker.SelectedItem).Key=="","Delete updates selector and detaches editor");
  savedDeckPicker.SelectedItem=((Choice[])savedDeckPicker.ItemsSource).First(c=>c.Key=="picker-mage");classPicker.SelectedItem=Classes.First(c=>c.Key=="WARRIOR");Check(draft.Id!="picker-mage"&&((Choice)savedDeckPicker.SelectedItem).Key==""&&savedDecks[0].Code==DeckCode.Encode(catalog,mage),"Class change leaves original saved deck intact and selects current draft");
  File.WriteAllText(Path.Combine(folder,"deck-picker-verification.txt"),"PASS: "+(checks-before)+" targeted deck selector assertions. Real WPF previews; no user data or clipboard writes.");
 }
}
}
