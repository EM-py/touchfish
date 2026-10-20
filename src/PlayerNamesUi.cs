using System.Windows;
using System.Windows.Controls;
namespace Touchfish {
public partial class PanelWindow {
 TextBox lanName,myName,opponentName;
 FrameworkElement NamedHeroContent(int seat,bool own){var stack=new StackPanel{VerticalAlignment=VerticalAlignment.Center};var name=T(match.PlayerLabel(seat),9,Ink);name.TextAlignment=TextAlignment.Center;name.TextTrimming=TextTrimming.CharacterEllipsis;stack.Children.Add(name);string label=HeroStatusLabel(seat,own);var stats=T(label.Substring(label.IndexOf('\n')+1),9,Ink);stats.TextAlignment=TextAlignment.Center;stats.TextTrimming=TextTrimming.CharacterEllipsis;AddSecretMarkerToolTips(stats,seat);stack.Children.Add(stats);return stack;}
 TextBox NameInput(){var input=Input("");input.MaxLength=PlayerNames.MaxLength;input.Height=23;input.ToolTip="本局名称，最多12个字符；留空使用默认名称。";return input;}
 FrameworkElement NameField(string caption,TextBox input){var grid=new Grid();grid.ColumnDefinitions.Add(new ColumnDefinition{Width=GridLength.Auto});grid.ColumnDefinitions.Add(new ColumnDefinition());var label=T(caption,9,Muted);label.Margin=new Thickness(0,0,6,0);grid.Children.Add(label);Grid.SetColumn(input,1);grid.Children.Add(input);return grid;}
 FrameworkElement LocalNames(){var grid=new Grid();grid.ColumnDefinitions.Add(new ColumnDefinition());grid.ColumnDefinitions.Add(new ColumnDefinition{Width=new GridLength(8)});grid.ColumnDefinitions.Add(new ColumnDefinition());myName=NameInput();opponentName=NameInput();grid.Children.Add(NameField("玩家1名称",myName));var other=NameField("玩家2名称",opponentName);Grid.SetColumn(other,2);grid.Children.Add(other);return grid;}
}
}
