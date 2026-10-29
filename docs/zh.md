# Dialog Engine — 对话框与剧本语言

Dialog Engine 负责玩家阅读和点击的一切：带台词、肖像和选项的对话框。它背后是**剧本语言**——一些简短的 `.scenario` 文本文件，故事在其中一行一行地写下来：谁说了什么、有哪些选项、每个选项通向哪里、游戏里会发生什么（换背景、放音乐、给物品、推进任务、切换场景）。合集里的每一款游戏，从小说模板到 RPG Plus，都用它讲述自己的故事。

## 玩家看到什么

进入场景时，场景的剧本会自动开始。台词出现在屏幕下方的对话框里，点击一下就显示下一句：

![My Novel 的小镇：傍晚的街道，对话框中显示台词"傍晚的小镇。路灯已经亮起，石板路上吹过一阵凉风。"。](assets/town-text.jpg)

当故事向玩家提问时，选项会出现在右侧，并等待玩家做出选择：

![对话框中的问题"接下来去哪儿？"以及右侧的两个选项："去酒吧"和"回家"。](assets/town-choice.jpg)

选择"去酒吧"后，玩家来到酒吧场景，那里开始播放自己的剧本：

![My Novel 的酒吧，对话框中是它的第一句台词。](assets/bar-text.jpg)

## 这些截图背后的剧本

这就是小说模板小镇场景的完整剧本（`my_novel_game/scenarios/my_novel_town.scenario`）：

```scenario
scene: my_novel_town
priority: 10
---
# 小镇是枢纽：每次到访都显示同样的选择。
text "傍晚的小镇。路灯已经亮起，石板路上吹过一阵凉风。" key=my_novel/town/intro
select "接下来去哪儿？" key=my_novel/town/prompt
option "去酒吧" key=my_novel/town/opt_bar -> :bar
option "回家" key=my_novel/town/opt_home -> :home

:bar
switch_scene my_novel_bar
end

:home
switch_scene my_novel_home
end
```

- `---` 之上的**头部**说明剧本何时播放：`scene:` 指明场景，`priority:` 排定同一场景多个剧本的先后（数字小的先播）。后文还会介绍其他头部行。
- `text "…"` 显示一句台词并等待点击。`key=` 是这句台词的翻译键；引号里的文本在找不到翻译时显示。
- `select` 提出一个问题，每个 `option` 都是一个选项，跳转到某个**标签**（`-> :bar`）。
- `:bar` 是标签；`switch_scene` 切换到另一场景；`end` 结束剧本。
- 以 `#` 开头的行是给作者看的注释。

要给场景添加故事，只需把 `.scenario` 文件放进自己 mod 的 `scenarios/` 文件夹（位置不限），并在头部写明场景名。

## 编写故事

**台词与肖像。** `text "…" first=marao/sad` 在第一个位置显示肖像，`second=…` 在第二个位置（参见 Characters Engine）。`ptext` 与 `text` 相同，还会把 `${name}` 替换为变量的值。`title "…"` 显示章节标题。

**选项与跳转。** 选项可以只在条件成立时显示：`option "Pay" "if=item('money/coin') >= 5" -> :pay`（条件里有空格时，连 `if=` 一起加引号）。`goto -> :label` 不作询问直接跳转。`when "<condition>" -> :label` 在条件此刻成立时跳转：

```scenario
when "rpg_wolf_dead == 1" -> :lantern
```

**记住状态。** `flag rpg_quest_taken 1` 设置一个随游戏存档保存的剧情标记；条件写作 `rpg_quest_taken == 1` 即可读取。`set name value` 设置当前对话的变量：`ptext` 把它显示为 `${name}`，动作也能读取它。

**条件**是一门统一的语言：标记、来自 Items Base 的 `item('id')`、来自 Quests Engine 的任务检查、各 mod 自带的属性与装备，用 `&&`、`||` 和 `!` 组合起来。

**场景演出。** `background`、`foreground`、`background_color`、`music`、`sound`、`effect`、`delay`、`light_set`、`hide_dialog`/`show_dialog`、`interactive_add`/`interactive_remove`、`notify`。

**游戏动作。** `action <name> key=value …` 执行另一个 mod 的动作，例如 `action item.add id=money/coin count=3` 或 `action quest.set_status quest_id=woodland_ward status=Progress`。各 mod 的 schema 文件列出了这些动作，Editor Tools 会在剧本编辑器和命令面板中提供它们。

## 剧本何时播放

这由头部决定：

```scenario
scene: prolog_alley
priority: 15
condition: AwakeningQuest.notHasTag("cloak_taken")
is_one_run: true
save_allowed: true
---
```

- `scene:` — 玩家到达时播放剧本的场景。
- `priority:` — 同一场景中各剧本的先后顺序；可播放的编号最小者先播，对话结束后下一个才开始。
- `condition:` — 仅在条件成立时播放；标记或任务发生变化时，场景会重新检查。
- `is_one_run: true` — 每局游戏只播放一次。
- `after:` — 等待指定剧本播放完毕。
- `save_allowed: true` — 玩家可以在该场景存档。

没有 `scene:` 头部的剧本不会由场景启动，而是由其他东西触发，比如点击场景中的交互物或地图事件。

## 其他 mod 的添加与替换

所有 mod 的剧本汇总在一起。mod 只要在头部写明场景名，就能把自己的故事加进别人的场景。声明了依赖并附带同名剧本文件的 mod 会替换原剧本。

## 出问题时

1. **剧本从不播放。** `scene:` 的场景名写错了、`condition` 为假，或者剧本带 `is_one_run` 且已经播放过。
2. **台词显示成了别的语言。** 该语言的翻译键 `key=` 在 mod 的 `locale` 文件里缺失，于是显示引号里的原文。
3. **选项跳了个空。** `->` 后面的标签必须存在于同一文件中，并带冒号（`:bar`）。
4. **`action` 没有反应。** 动作所属的 mod 不在游戏内（日志提示动作 "is not registered"），或者参数名写错——与 Editor Tools 剧本编辑器中的动作对照一下。
