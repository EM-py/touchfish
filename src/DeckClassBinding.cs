using System;
using System.Linq;
using System.Windows.Controls;
namespace Touchfish {
public partial class PanelWindow {
 bool syncingDeckClass;
 void BindDeckClass(ComboBox profession,ComboBox decks){decks.SelectionChanged+=(s,e)=>SyncChosenDeckClass(profession,decks);profession.SelectionChanged+=(s,e)=>{if(!syncingDeckClass)SyncChosenDeckClass(profession,decks);};}
 DeckDocument SelectedBoundDeck(string key){if(key=="draft")return draft.Copy();var saved=savedDecks.First(d=>d.Id==key);return DeckCode.Decode(catalog,saved.Code,saved.PoolId??"classic-2014");}
 void SyncChosenDeckClass(ComboBox profession,ComboBox decks){if(syncingDeckClass||profession==null||decks==null||decks.SelectedItem==null)return;string key=((Choice)decks.SelectedItem).Key;bool busy=decks==lanDeck&&lanMode;if(key=="training"){profession.IsEnabled=!busy;profession.ToolTip="选择基础练习卡组的职业";return;}try{var deck=SelectedBoundDeck(key);syncingDeckClass=true;profession.SelectedItem=Classes.First(c=>c.Key==deck.ClassId);profession.ToolTip="职业由卡组绑定";}catch(Exception ex){if(!(ex is System.IO.InvalidDataException||ex is InvalidOperationException))throw;status.Text="卡组无效："+ex.Message;}finally{syncingDeckClass=false;profession.IsEnabled=false;}}
}
}
