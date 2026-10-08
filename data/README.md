# 卡牌数据与扩展接口

内置卡池 `classic-2014` 对应经典模式的 2014 年 6 月 / 1.0.0.5832 基准。使用 HearthstoneJSON 冻结的 `VANILLA` 版本，而不是当前 `LEGACY` 版本。暴雪规则依据：https://hearthstone.blizzard.com/en-us/news/23620129/introducing-the-core-set-and-classic-format

数据固定于 HearthstoneJSON build 253932：
https://api.hearthstonejson.com/v1/253932/zhCN/cards.json

- 382 张可组牌：每个经典职业 25 张，九职业共 225 张；中立 157 张。
- 518 条导入记录：509 条 VANILLA 记录及 9 个基础英雄。136 条非组牌记录用于保留来源上下文，包含衍生牌、附魔和辅助英雄技能；不宣称这些辅助记录全部是 2014 年可用内容，不允许将它们加入卡组。
- 中文名称、原文、去标记的显示文本、费用、攻击、生命、耐久、职业、稀有度、机制、随从种族、关联牌组、过载与法强均独立存储。
- `sources/hearthstonejson-classic-zhCN.json` 保留来源原始记录；`sources/provenance.json` 记录固定 URL、build、完整下载 SHA256、导入文件 SHA256 和数量。
- 卡牌名称和数值属于 Blizzard；HearthstoneJSON 数据说明：https://hearthstonejson.com/docs/cards.html
- 冻结 VANILLA 武器记录可能显式给出 `durability: 0` 并把实际耐久放在 `health`。导入器将该历史字段映射为正常的 Durability，已检查十二把武器的耐久为正数；炽炎战斧为 2，毁灭之锤为 8。
- 冻结记录的 `spellDamage` 可能只表示法强机制存在，例如玛里苟斯原始为 1，但牌面为“法术伤害+5”。导入器按 SPELLPOWER 牌面中的明确数字归一化加成，玛里苟斯为 5；原始来源记录保持不变。

## 添加新牌库

1. 在 `cardsets/` 放入一个新的 JSON 文件，根对象必须是 `SchemaVersion: 1`、唯一 `Id`、`Name`、`Version`、`Cards`。
2. 每张卡必须有唯一稳定 `Id`、唯一正整数 `DbfId`、`SetId`（等于包 Id）、`Name`、`CardClass`、`Type`、`Rarity`、`Collectible` 和对应数值。`LegacyDbfId` 仅是旧代码导入别名，保留 0 表示没有别名。
3. 在 `pools.json` 的 `Pools` 中添加一个独立卡池，设置 `CardSetIds`、`DeckSize`、`CopyLimit`、`LegendaryLimit`、`DeckstringFormat`、`HeroDbfIds`。不要往经典池直接混入新牌。
4. 重启程序。新卡池自动出现在组牌选择器，不需要修改已有经典牌库，也不需要重新编译。

`ICardSetSource` 是牌库来源接口；默认实现 `JsonDirectoryCardSetSource`。`CardCatalog` 加载与索引，`DeckRules` 负责组牌数量和职业限制，`DeckCode` 独立负责代码格式。当前组牌类型为随从、法术、武器。实际效果另由 `MatchRules` 显式注册；未知复杂效果不能默认当作白板，卡牌进入牌库不等于其对战行为已实现。

预览验证用隔离的 `previews/extension-fixture/` 测试额外一张牌及一个新卡池，证明扩展池为 383 张而经典池仍为 382 张。测试牌没有放入内置运行数据。

## 卡组代码

使用 HearthSim/Blizzard Deckstring v1：Base64 + varint + 英雄 DBF ID + 按数量分组的卡牌 DBF ID。经典格式编号为 3；导出使用冻结版本 DBF ID。导入支持该版本及对应旧卡 ID 的别名，始终映射到冻结的经典数值。

完成卡组按代码保存在 `decks/library.json`（名称、代码、卡池和内部标识）；未完成编辑状态在 `decks/draft.json` 自动恢复。分享时只需一串代码，不需要分享这些文件。

编码协议：https://hearthsim.info/docs/deckstrings/

九职业代码已由独立的 `python-hearthstone` 实现解码并逐字节重新编码验证。尚未进行炉石客户端的实际互操作测试。

## 重新导入与验证

`powershell -ExecutionPolicy Bypass -File scripts/import_classic.ps1` 下载固定快照并重新生成内置经典牌库（会覆盖内置数据文件）。Python 3 为导入脚本运行条件，运行游戏本身不需要 Python。

`Touchfish.exe --preview previews` 检查界面、卡池、组牌限制、代码、保存、失败导入不修改当前卡组，以及扩展接口。

`powershell -ExecutionPolicy Bypass -File scripts/fetch_reference.ps1` 获取 HearthSim 参考实现，然后 `python scripts/verify_reference.py` 执行独立代码核对。
