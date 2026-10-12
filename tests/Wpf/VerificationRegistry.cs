using System;
using System.Collections.Generic;
namespace Touchfish {
public static class VerificationRun {public static int? FirstSeat=0;public static bool Network=true,Visual=true;}
public partial class PanelWindow {
 public void RunVerification(string scope,string folder){
  VerificationRun.Network=Environment.GetEnvironmentVariable("TOUCHFISH_VERIFY_NETWORK")!="0";VerificationRun.Visual=Environment.GetEnvironmentVariable("TOUCHFISH_VERIFY_VISUAL")!="0";
  var suites=new Dictionary<string,Action<string>>{
   {"priest",PreviewPriestScope},{"full",PreviewSuite},{"summon",PreviewSummonScope},{"turn-end",PreviewTurnEndScope},{"hero-visuals",PreviewHeroVisualScope},{"triggers",PreviewTriggerScope},{"deathrattle",PreviewDeathrattleScope},{"druid-spells",PreviewDruidSpellScope},{"emote-selection",PreviewEmoteSelectionScope},{"overload",PreviewOverloadScope},{"spell-cards",PreviewSpellCardsScope},{"opening-turn",PreviewOpeningTurnScope},{"deck-picker",PreviewDeckPickerScope},{"hero-freeze",PreviewHeroFreezeScope},{"combo",PreviewComboScope},{"warlock",PreviewWarlockScope},{"visual-polish",PreviewVisualPolishScope},{"graveyard",PreviewGraveyardScope},{"rogue",PreviewRogueScope},{"warrior",PreviewWarriorScope},{"secret-presentation",PreviewSecretPresentationScope},{"secrets",PreviewSecretScope}};
  Action<string> run;if(!suites.TryGetValue(scope,out run))throw new InvalidOperationException("未知检查模块："+scope);run(folder);
 }
}
}
