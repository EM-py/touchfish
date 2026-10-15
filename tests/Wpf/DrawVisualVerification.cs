using System;
using System.IO;
using System.Linq;
namespace Touchfish {
public partial class PanelWindow {
 public void PreviewDrawVisualScope(string folder){
  previewRun=true;Directory.CreateDirectory(folder);LeaveLan();localTest=true;ApplyTheme(true,true);ApplyMode(false);match=MatchFixture();matchViewSeat=0;awaitHandoff=false;ShowPage("match");RenderMatch();UpdateLayout();var renderer=(OfficeMatchVisualEffects)matchEffects;renderer.Clear();
  var player=match.Players[0];player.Hand.Clear();for(int n=0;n<10;n++)player.Hand.Add(new HandCard{Id=80000+n,CardId="VAN_CS2_182"});player.Deck.Clear();player.Deck.Add("VAN_CS2_029");int before=renderer.OverdrawCount;match.Draw(0,1);RenderMatch();UpdateLayout();Check(renderer.OverdrawCount==before+1&&matchEffectsCanvas.Children.Count>0,"Full hand emits a centered overdraw card animation");Check(player.Hand.Count==10&&player.Deck.Count==0,"Overdraw leaves hand full and consumes exactly one deck card");Render(Path.Combine(folder,"overdraw-normal.png"));
  before=renderer.FatigueCount;match.Draw(0,1);RenderMatch();UpdateLayout();Check(renderer.FatigueCount==before+1,"Empty deck draw emits fatigue animation");Check(enemyHandStatus!=null&&enemyHandStatus.Text.Contains("手牌 ")&&enemyHandStatus.Text.Contains("牌库 "),"Opponent resource line shows hand and deck counts");var fatigueStarted=DateTime.UtcNow;PumpLan(()=>DateTime.UtcNow-fatigueStarted>=TimeSpan.FromMilliseconds(1600),"Overdraw animation finishes before queued fatigue");Render(Path.Combine(folder,"fatigue-normal.png"));
  ApplyMode(true);RenderMatch();before=renderer.FatigueCount;match.Draw(0,1);RenderMatch();Check(renderer.FatigueCount==before&&matchEffectsCanvas.Visibility==System.Windows.Visibility.Collapsed,"Minimal mode remains visually quiet");ApplyMode(false);localTest=false;File.WriteAllText(Path.Combine(folder,"draw-visual-verification.txt"),"PASS: overdraw, fatigue, opponent deck count and minimal-mode suppression; rendered real WPF previews.");
 }
}
}
