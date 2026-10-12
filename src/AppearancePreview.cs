using System.IO;
namespace Touchfish {
public partial class PanelWindow {
 public void PreviewAppearance(string folder){previewRun=true;Directory.CreateDirectory(folder);ApplyMode(false);ApplyTheme(false,false);ShowPage("game");Render(Path.Combine(folder,"appearance-wps.png"));ApplyTheme(true,true);Render(Path.Combine(folder,"appearance-codex.png"));ApplyMode(true);Render(Path.Combine(folder,"appearance-minimal.png"));}
}
}
