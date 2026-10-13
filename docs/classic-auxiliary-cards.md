# 经典发明与强力和弦

现有冻结 VANILLA 可组牌数据保持不变，缺失辅助记录使用 GAME_ ID，不加入 382 张组牌池。

发明分别为 1 费的导航小鸡 0/1、修理机器人 0/3、壮胆机器人 0/4、变鸡器 0/3；变鸡结果为 0 费 1/1 野兽。三种和弦都是 4 费，埃辛诺斯之焰为 1 费 2/1。

部落池固定为 CS2_121、EX1_021、EX1_023、EX1_110、EX1_390、CS2_179；属性从冻结包读取。双方和弦分别由房主随机生成，身份不公开。

核对来源（2026-10-13）：

- [暴雪 2013 修理机器人改动](https://hearthstone.blizzard.com/en-us/news/11144301)：0/3 与回合末治疗六点。
- [暴雪 2013 酋长说明](https://hearthstone.blizzard.com/en-us/news/11514705)：三种和弦。
- [暴雪变鸡器说明](https://hearthstone.blizzard.com/en-us/cards/146/)：回合开始随机随从变鸡。
- [HearthstoneJSON 历史辅助记录 build 25770](https://api.hearthstonejson.com/v1/25770/enUS/cards.json)：辅助牌费用与文本，仅用于核对，不替换现有牌库。
- [Fireplace 经典效果源码](https://github.com/jleclanche/fireplace/blob/master/fireplace/cards/classic/neutral_legendary.py)：发明、和弦触发和固定部落池。
