using System;
using System.Windows;
using System.Windows.Controls;

namespace Touchfish {
public static class AppRelease {
 public const string Version="v0.3.0",Date="2026-10-09";
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
  "当前范围：174 / 382 张牌可对战，其余标注【暂不可用】。完整经典规则、起手换牌、回合倒计时与断线续局尚未完成。"
 };
}
public partial class PanelWindow {
 Grid releasePanel;Button releaseTab;
 void BuildReleaseNotes(){
  releasePanel=new Grid{Margin=new Thickness(12,8,12,8),Visibility=Visibility.Collapsed};releasePanel.RowDefinitions.Add(new RowDefinition{Height=new GridLength(24)});releasePanel.RowDefinitions.Add(new RowDefinition());Put(shell,releasePanel,4);var heading=new Grid();heading.ColumnDefinitions.Add(new ColumnDefinition());heading.ColumnDefinitions.Add(new ColumnDefinition{Width=new GridLength(64)});heading.Children.Add(T("更新日志 · "+AppRelease.Version,12,Ink));var updates=Btn("版本更新",64);updates.Height=22;updates.FontSize=10;updates.Click+=(s,e)=>OpenUpdates();Grid.SetColumn(updates,1);heading.Children.Add(updates);releasePanel.Children.Add(heading);
  var body=new StackPanel();var date=T(AppRelease.Date+" · 普通模式视觉效果",10,Muted);date.Margin=new Thickness(0,0,0,8);body.Children.Add(date);foreach(string note in new[]{"普通模式新增双方共享的法术提示、伤害飘字；单次伤害大于3震动目标，大于10同时震动整个界面。","视觉事件独立于规则与记录，经局域网同步并去重；极简模式不播放动画。","修复检查更新对 GitHub API 的依赖：优先读取版本订阅，失败时使用 API，改善403错误说明。","运行目录仅保留 Touchfish.exe 和 Touchfish.Update.exe，清理已结束的临时更新器。"}){var text=T(note,10,Ink);text.TextWrapping=TextWrapping.Wrap;text.Margin=new Thickness(0,0,0,8);body.Children.Add(text);}body.Children.Add(T("v0.2.0 · 2026-10-08 · GitHub 更新版",11,Ink));foreach(string note in new[]{"接入 YuziPlus/touchfish 的 GitHub Releases：检查更新、下载校验、安装重启与历史版本回退。","个人卡组、草稿和设置保留；本地不长期保留旧版，历史版本仅存于 GitHub。","增加独立更新器，旧版回退后仍可重新升级；对局中阻止安装。"}){var text=T(note,10,Ink);text.TextWrapping=TextWrapping.Wrap;text.Margin=new Thickness(0,0,0,8);body.Children.Add(text);}body.Children.Add(T("v0.1.1 · 2026-10-08 · 修正版",11,Ink));foreach(string note in AppRelease.PatchNotes){var text=T(note,10,Ink);text.TextWrapping=TextWrapping.Wrap;text.Margin=new Thickness(0,0,0,8);body.Children.Add(text);}var previous=T("v0.1 · 2026-10-08 · 一号版本",11,Ink);previous.Margin=new Thickness(0,6,0,8);body.Children.Add(previous);foreach(string note in AppRelease.Notes){var text=T(note,10,Ink);text.TextWrapping=TextWrapping.Wrap;text.Margin=new Thickness(0,0,0,8);body.Children.Add(text);}Put(releasePanel,new ScrollViewer{Content=body,VerticalScrollBarVisibility=ScrollBarVisibility.Auto,HorizontalScrollBarVisibility=ScrollBarVisibility.Disabled},1);
 }
 void OpenReleaseNotes(){ApplyFonts(releasePanel);ShowPage("release");}
}
}
