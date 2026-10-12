# 扩展接口与开发检查

正式程序仍使用现有 Windows Framework 编译器，不需要新增 SDK 或游戏引擎。开发测试程序保存在 `work/test-bin`，不会进入发布包。

## 选择检查范围

```powershell
# 默认：根据未提交的 Git 改动选择规则模块
.\scripts\test.ps1
# 只看执行计划
.\scripts\test.ps1 -PlanOnly
# 单一模块，不启动窗口或网络
.\scripts\test.ps1 -Scope secrets
# 两个模块
.\scripts\test.ps1 -Scope 'warrior,rogue'
# 对应模块的 TCP 与 WPF 交互检查；不写界面 PNG、不统计字体遍历
.\scripts\test.ps1 -Scope secrets -Layer network
# 法术展示的界面检查，不连接 TCP
.\scripts\test.ps1 -Scope spell-cards -Layer ui
# 发布与核心结构重构后的完整回归
.\scripts\test.ps1 -Scope full -Layer all
```

自动选择对无法识别的核心改动采取保守策略，运行全部纯规则模块。界面或联机改动仍应明确调用对应层，纯规则检查不能证明鼠标界面或 TCP 可用。`tests/LegacyRuleChecks.cs` 保留迁移前的机制断言；`tests/RuleTests.cs` 补充扩展边界契约，不把几千项字体检查混入日常规则检查。

`Touchfish.exe --preview previews` 只生成真实 WPF 外观预览。原有完整验证和专项入口迁至测试 EXE，由脚本编译与调用。发布脚本先调用完整检查，再打包。

## 模块职责

| 模块 | 扩展入口与约束 |
|---|---|
| CardCatalog | JSON 卡包与卡池；冻结经典数据保持独立 |
| MatchRules / RuleModules | 显式 EffectId，例如 `my-pack:damage@1`；模块注册效果与触发，不覆盖内置实现 |
| ConditionalTriggerRules | 出牌、施法、开始、结束、受伤、实际随从治疗、自身死亡的公共时机；旧效果通过适配保持结算顺序 |
| ResolutionQueue | 受伤与治疗批次 FIFO，嵌套排队不重复进入；结算与视觉事件分离 |
| GameModes | 起手、生命、容量、初始法力，以及回合、胜负、卡组校验钩子 |
| EnchantmentCollection | 属性、临时攻击、命令怒吼、亡语及延迟效果的统一容器；旧属性通过兼容访问器操作集合 |
| BattleUnitCopies | 明确字段与附加效果深拷贝，新增语义需要主动声明复制策略 |
| PublicPlayerState / LanProtocol | 明确公开字段白名单，私有手牌与牌库单独遮蔽；已确认日志增量同步 |
| MatchVisualEvents / IMatchVisualEffects | 公开展示事件和渲染，不能从日志解析效果或发送隐藏身份 |

模式配置当前仍在两人、最多七槽、十张手牌、十点水晶的办公界面范围内。新增了配置与注册入口，没有开放新的产品模式。多人、战棋、新职业、可保存回放、断线续局仍需要独立实现和验证。

注册模块只使用编译进程序的可信 C# 代码；JSON 引用效果定义，不执行任意脚本。内置规则保留经典 BaseId；扩展卡可显式引用版本化 EffectId，未注册的效果继续标记不可用。

局域网协议目前为 2，仍要求双方同一完整程序包。日志确认不推进对局 Revision；重复日志增量重新拼接完整历史，不增加重复记录。对局公开视图不是可恢复的完整存档，墓地、奥秘实例和随机状态不借此暴露。

## 牧师结算接口

`PriestRules` 承接复制与控制；`RestoreHealth` 显式传入治疗来源和法术/技能资格，基础 `Heal` 只执行实际治疗与触发。法术开始结算时快照法强、维伦倍数与奥金尼转换，保证同一群体效果不会因来源中途死亡改变后续目标。法术伤害使用 `SpellDamageAmount`，英雄技能只乘维伦，不附加法强；战吼和回合治疗不受维伦放大。

`MinionHealed` 提供被治疗随从和实际恢复量，过量治疗和英雄治疗不触发北郡。`Shadowform` 是明确公开状态；复制手牌由房主标记 `Generated`，保持身份私有，生成牌可在对局跨职业使用，卡组验证仍按职业。

日常调用 `scripts/test.ps1 -Scope priest`（纯规则），需要联机时追加 `-Layer network`。牧师核心断言位于 `tests/PriestRuleChecks.cs`，真实 TCP 交互位于 `tests/Wpf/PriestVerification.cs`。
