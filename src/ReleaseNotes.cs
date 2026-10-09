using System;
using System.Windows;
using System.Windows.Controls;

namespace Touchfish {
public static class AppRelease {
 public const string Version="v0.3.6",Date="2026-10-09";
 public static readonly string[] PatchNotes={"修复玛里苟斯法术伤害：原先错误地增加 1，现在正确增加 5；沉默或离场后移除加成。","对方英雄区域显示手牌数量，随抽牌与出牌同步变化；不显示对方手牌内容。"};
 public static readonly string[] Notes={
  "首个正式版本，保留小尺寸、无卡图的办公面板外观。",
  "局域网对战：创建与加入房间，房主地址可一键复制，多网卡可切换；双方固定己方视角，操作自动同步，断线后停止操作。",
  "经典牌库：382 张可组牌，覆盖九职业与中立牌；预留独立卡池扩展接口。",
  "组牌与分享：按职业、费用和类型筛选，搜索名称与效果；卡组可保存，一键导出或导入代码。",
  "基础对战：攻击、法术、战吼、英雄技能、武器、法力与回合流程；先选来源再选目标，支持左右插入随从。",
  "共享记录：双方看到同一份公开对局历史，包含出牌、目标、攻击伤害、抽牌数量、疲劳、阵亡与回合切换。离开后可查看上局记录。",
  "手牌详情：随从显示费用、攻击和生命；武器显示攻击和耐久，完整效果可滚动查看。",
  "外观：WPS 与 Codex 风格，普通 / 极简模式；极简只显示攻、效果、血与详情入口。Codex 深色背景 #0D1117，文字 #E6EDF3。",
  "本地测试：仅从顶栏小 test 按钮进入自对战，正常对战入口为局域网。",
  "联机试运行：用户与朋友已实测，反馈可以运行。",
  "当前范围：198 / 382 张牌可对战，其余标注【暂不可用】。完整经典规则、起手换牌、回合倒计时与断线续局尚未完成。"
 };
}
public partial class PanelWindow {
 Grid releasePanel;Button releaseTab;
 void BuildReleaseNotes(){
  releasePanel=new Grid{Margin=new Thickness(12,8,12,8),Visibility=Visibility.Collapsed};releasePanel.RowDefinitions.Add(new RowDefinition{Height=new GridLength(24)});releasePanel.RowDefinitions.Add(new RowDefinition());Put(shell,releasePanel,4);var heading=new Grid();heading.ColumnDefinitions.Add(new ColumnDefinition());heading.ColumnDefinitions.Add(new ColumnDefinition{Width=new GridLength(64)});heading.Children.Add(T("更新日志 · "+AppRelease.Version,12,Ink));var updates=Btn("版本更新",64);updates.Height=22;updates.FontSize=10;updates.Click+=(s,e)=>OpenUpdates();Grid.SetColumn(updates,1);heading.Children.Add(updates);releasePanel.Children.Add(heading);
  var body=new StackPanel();
  body.Children.Add(ReleaseSection("v0.3.6",AppRelease.Date,new[]{"实现 10 张经典抉择卡：打出后从两张卡牌样式选项中选择对应效果。","抉择随从可先安放位置再选择效果；目标型抉择与战吼均需选目标。","结算前右键当前手牌可撤销待完成的出牌，卡牌仍留在手牌且不消耗法力；场上待出牌预览飞回手牌的动画只在操作者本机显示。","新增霍格回合结束时召唤 2/2 嘲讽豺狼人，以及哈里森·琼斯摧毁敌方武器并按剩余耐久抽牌。","对战支持数量增至 198 / 382。"}));
  body.Children.Add(ReleaseSection("v0.3.5","2026-10-09",new[]{"组牌增加职业与中立筛选，新增鱼人猎潮者、剃刀猎手和白银之手骑士召唤战吼。","普通模式弃牌后显示牌名飘字，持续 2.5 秒，不受“特效全开”开关影响；多张弃牌分行显示。","弃牌飘字通过公开结构化事件同步双方并去重，极简模式不显示。","本地 test 无需满 30 张牌即可开局，职业、数量上限与效果支持限制保留；局域网仍要求完整卡组。"}));
  body.Children.Add(ReleaseSection("v0.3.4","2026-10-09",new[]{"卡组绑定职业，选择已保存卡组或当前编辑卡组后自动切换并锁定职业；基础练习卡组按所选职业生成。","出牌与 test 取牌增加职业限制，只能使用本职业和中立牌；卡组导入、导出和开局继续校验职业。","组牌关键词搜索整个卡池，其他职业牌可查看但不能加入；支持多关键词，无结果时提示检查费用与类型筛选。"}));
  body.Children.Add(ReleaseSection("v0.3.3","2026-10-09",new[]{"增加统一手牌弃牌结算，支持灵魂之火、魔犬、末日守卫和死亡之翼；随机弃牌不重复选择，手牌不足时弃掉现有牌。","房主结算随机结果，双方共享被弃掉的牌名，剩余手牌保密；死亡之翼的亡语抽牌在弃牌完成后结算。","可对战卡牌增加至 183 / 382；追踪术仍待牌库选择机制实现。"}));
  body.Children.Add(ReleaseSection("v0.3.2","2026-10-09",new[]{
   "新增经典六张激怒牌，受伤时获得攻击、风怒或武器加攻；完全治疗、沉默或离场后移除对应加成。对战支持数量为 180 / 382。",
   "设置增加“特效全开”：普通模式可启用金色传说随从名字、生效中的红色激怒标记和震动，默认关闭。",
   "普通模式始终显示法术提示与伤害飘字，伤害飘字延长至 1 秒；极简模式不显示任何特效。",
   "更新日志按版本折叠，点击版本号展开详细内容。"}));
  body.Children.Add(ReleaseSection("v0.3.1","2026-10-09",new[]{"本地 test 双方开局 10 个满水晶；小“取”按钮从己方剩余牌库指定取牌，手牌上限十张。","普通模式伤害飘字延长至 0.8 秒。"}));
  body.Children.Add(ReleaseSection("v0.3.0","2026-10-09",new[]{"普通模式新增双方共享的法术提示、伤害飘字；单次伤害大于3震动目标，大于10同时震动整个界面。","视觉事件独立于规则与记录，经局域网同步并去重；极简模式不播放动画。","检查更新优先读取 GitHub 版本订阅，失败时使用 API，改善403错误说明。","运行目录仅保留 Touchfish.exe 和 Touchfish.Update.exe，清理已结束的临时更新器。"}));
  body.Children.Add(ReleaseSection("v0.2.0","2026-10-08",new[]{"接入 GitHub Releases：检查更新、下载校验、安装重启与历史版本回退。","个人卡组、草稿和设置保留；本地不长期保留旧版，历史版本仅存于 GitHub。","增加独立更新器，旧版回退后仍可重新升级；对局中阻止安装。"}));
  body.Children.Add(ReleaseSection("v0.1.1","2026-10-08",AppRelease.PatchNotes));
  body.Children.Add(ReleaseSection("v0.1","2026-10-08",AppRelease.Notes));
  Put(releasePanel,new ScrollViewer{Content=body,VerticalScrollBarVisibility=ScrollBarVisibility.Auto,HorizontalScrollBarVisibility=ScrollBarVisibility.Disabled},1);

 }
 Expander ReleaseSection(string version,string date,string[] notes){var content=new StackPanel{Margin=new Thickness(18,4,4,6)};content.Children.Add(T(date,9,Muted));foreach(string note in notes){var text=T(note,10,Ink);text.TextWrapping=TextWrapping.Wrap;text.Margin=new Thickness(0,4,0,4);content.Children.Add(text);}return new Expander{Header=T(version,11,Ink),Content=content,IsExpanded=false,FontWeight=FontWeights.Normal,Foreground=Ink,Margin=new Thickness(0,0,0,6),HorizontalContentAlignment=HorizontalAlignment.Stretch};}
 void RenderExpandedRelease(string folder){var scroll=(ScrollViewer)releasePanel.Children[1];var body=(StackPanel)scroll.Content;var first=(Expander)body.Children[0];first.IsExpanded=true;Render(System.IO.Path.Combine(folder,"62-release-expanded.png"));first.IsExpanded=false;}
 void OpenReleaseNotes(){ApplyFonts(releasePanel);ShowPage("release");}
}
}
