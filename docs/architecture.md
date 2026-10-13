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

## 法师扩展

冰锥术由 `MageRules` 在施法时固定目标及邻居 ID，再冻结并结算同批伤害。安东尼达斯和法力浮龙复用 `SpellCast`，生成火球只由房主写入私有手牌。肯瑞托的 `FreeSecretThisTurn` 是公开的本回合费用状态，`TimedCostRules` 应用与到期清理，成功使用奥秘后消费；静态卡牌费用不修改。

使用 `scripts/test.ps1 -Scope mage` 检查法师规则，追加 `-Layer network` 检查双端权威结算与手牌隐私。

## 圣骑士与萨满扩展

`PaladinRules.Summoned` 处理公正之剑的加成与耐久，`Summon` 的召唤事件开关使变形不重复触发入场效果。变形保留攻击使用、入场回合及临时控制返还信息，其他附加效果清除。`ShamanRules` 处理战吼、视界术实例减费、图腾之力与妖术；无羁元素注册公共 `CardPlayed` 过载条件。

先祖之魂和丛林之魂在 `EnchantmentCollection` 中各次授予分别记录，由公共 `DeathrattleRules` 按授予顺序在死亡位置结算。先祖之魂召唤基础复制，不重复战吼且不继承旧附加亡语；新复制仍经过真实召唤接口。沉默清除旧授予，后续新授予仍有效。

纯规则使用 `scripts/test.ps1 -Scope "paladin,shaman"`，实际 TCP 交互使用 `-Scope paladin-shaman -Layer network`。

## 英雄替换与技能注册

`HeroReplacementRules.RegisterHero` 注册 `HeroReplacementDefinition`（稳定 ID、名称、生命策略、护甲策略、种族及技能 ID）；`RegisterAbility` 注册 `HeroAbilityDefinition`（费用、目标、要求和结算）。替换战吼的扩展效果 ID 可用 `RegisterReplacementMinion` 登记，以免替换前重复触发普通随从入场反应。`ReplaceHero` 只改英雄状态，不修改手牌、牌库、卡组职业或法力，武器由效果独立装备。

`HeroFormId` 与 `HeroAbilityId` 独立；替换英雄可刷新技能，后续暗影形态可替换技能而保留英雄形态。普通职业技能仍沿用现有默认入口，已注册技能使用同一选择目标与原子校验流程，界面按实际技能费用和名称显示。`HeroHealthLimit` 统一治疗上限，`HeroDestroyed` 区分直接消灭与普通受伤，阻止治疗复活已消灭的英雄。

英雄字段使用 `PublicPlayerState` 白名单同步，快照拒绝未注册的替换定义。大王特效采用 `hero-speech` 公开事件，锚定英雄气泡，受普通模式及特效开关约束并去重；不解析日志，不修改真实用户设置。

纯规则用 `scripts/test.ps1 -Scope heroes`，双端与特效开关交互用 `-Scope heroes -Layer network`。

## 中立低费牌与触发阶段

`LowCostRules` 独立注册本批 12 张中立牌的效果和监听器，不实现猎人职业牌。`CharacterHealed` 给出目标与实际恢复量，英雄和随从均可触发，过量治疗不触发；保留北郡的 `MinionHealed` 时机。

`MinionSummoned` 在普通打牌时延迟到战吼之后，监听者在召唤时快照，新增随从不响应自身召唤，战吼回手的监听者不再触发。战吼子召唤按 FIFO 排队；法术、技能和亡语产生的真实召唤同样走接口，变形不走。此公共接口可供后续职业模块登记召唤监听器，本轮不加入猎人效果。

`SpellCast` 表示已成功支付的施法，反制前仍可响应；`SpellResolved` 是未被反制且效果已完成后的时机，炎术师使用后者。临时攻击沿用回合末清理，海盗匹配抽牌保持牌库剩余顺序且只公开数量。检查使用 `scripts/test.ps1 -Scope low-cost`，需要实际双端则追加 `-Layer network`。

猎人独立模块 `HunterRules` 复用召唤后公共触发；`hunter` 可单独运行规则和 TCP 检查。追踪候选在 `LanPlayerView.DeckPreview` 中按接收席位和持牌条件筛选，公开玩家白名单不包含牌库身份；追踪选牌不写公开抉择记录。随从免疫使用附加效果的到期字段，复制深拷贝、沉默和变形沿用集合规则。

## 中立 3 / 4 费牌

`MidCostRules` 独立注册规则与监听器，`mid-cost` 分别运行纯规则或 TCP 范围。新增 `MinionDied` 及 `MatchTriggerContext.Died`，死亡批次先移除所有死者并刷新光环，再取生存监听者快照逐个通知，随后执行既有亡语；通知时 `cleanup:false` 避免递归打乱死亡批次。报警机器人只在当前回合开始交换，复用实际召唤而不触发打牌战吼；工匠使用现有变形，技师使用永久控制，年迈酒仙使用回手。动态武器减费在费用计算末段处理，不计英雄临时攻击；牌库冻结数据不变，香蕉与机械幼龙为辅助记录。

## 中立 5 / 6 费牌

HighCostRules 注册 11 张中立牌、四种发明和三种和弦，规则与 TCP 范围为 high-cost。CardResolved 在出牌效果与清场后通知出牌前的生存监听者，含反制法术；非法出牌无事件，新登场伊利丹不响应自身。CopyFaceless 显式深复制后恢复新实体、控制者、入场元数据，清除临时控制返还。辅助牌来源见 docs/classic-auxiliary-cards.md，未改动冻结组牌数据。
