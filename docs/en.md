# Dialog Engine — the dialog box and the scenario language

Dialog Engine is what the player reads and clicks: the dialog box with its lines, portraits and answers. Behind it is the **scenario language** — small `.scenario` text files in which a story is written line by line: what is said, which answers there are, where each answer leads, and what happens in the game (a new background, music, an item, a quest step, another scene). Every game in the collection, from the novel template to RPG Plus, tells its story with it.

## What the player sees

A scene's scenario starts by itself when the player enters the scene. Lines appear in the box at the bottom; a click shows the next one:

![The town of My Novel: the evening street, the line "Evening town. The street lamps are already lit and a cool draught runs along the cobbles." in the dialog box.](assets/town-text.jpg)

When the story asks something, the answers appear on the right. The story waits until one is chosen:

![The question "Where to go?" in the dialog box and its two answers on the right: "Go into the bar" and "Go back home".](assets/town-choice.jpg)

"Go into the bar" takes the player to the bar scene, which starts its own scenario:

![The bar of My Novel with its first line in the dialog box.](assets/bar-text.jpg)

## The scenario behind these pictures

This is the whole town scenario of the novel template (`my_novel_game/scenarios/my_novel_town.scenario`):

```scenario
scene: my_novel_town
priority: 10
---
# The town is the hub: every visit shows the same choice.
text "Evening town. The street lamps are already lit and a cool draught runs along the cobbles." key=my_novel/town/intro
select "Where to go?" key=my_novel/town/prompt
option "Go into the bar" key=my_novel/town/opt_bar -> :bar
option "Go back home" key=my_novel/town/opt_home -> :home

:bar
switch_scene my_novel_bar
end

:home
switch_scene my_novel_home
end
```

- **The header** above `---` says when the scenario plays: `scene:` names the scene, `priority:` orders several scenarios of one scene (lower plays first). More header lines are below.
- `text "…"` shows a line and waits for a click. `key=` is the translation key of the line; the quoted text is what is shown when no translation is found.
- `select` asks a question, and each `option` is an answer that jumps to a **label** (`-> :bar`).
- `:bar` is a label; `switch_scene` goes to another scene; `end` finishes the scenario.
- A line starting with `#` is a note for the author.

To add a story to a scene, put a `.scenario` file anywhere in your mod's `scenarios/` folder and name the scene in its header.

## Writing a story

**Lines and portraits.** `text "…" first=marao/sad` shows a portrait in the first place, `second=…` in the second (see Characters Engine). `ptext` is the same as `text`, and also replaces `${name}` with the value of a variable. `title "…"` shows a chapter title.

**Choices and jumps.** An answer can be shown only when a condition holds: `option "Pay" "if=item('money/coin') >= 5" -> :pay` (a condition with spaces is quoted together with `if=`). `goto -> :label` jumps without asking. `when "<condition>" -> :label` jumps if the condition holds at that moment:

```scenario
when "rpg_wolf_dead == 1" -> :lantern
```

**Remembering things.** `flag rpg_quest_taken 1` sets a story flag that is saved with the game; conditions read it as `rpg_quest_taken == 1`. `set name value` sets a variable of the running dialog; `ptext` shows it as `${name}` and actions can read it.

**Conditions** are one shared language: flags, `item('id')` from Items Base, quest checks from Quests Engine, stats and equipment from their mods, joined with `&&`, `||` and `!`.

**The scene.** `background`, `foreground`, `background_color`, `music`, `sound`, `effect`, `delay`, `light_set`, `hide_dialog`/`show_dialog`, `interactive_add`/`interactive_remove`, `notify`.

**Game actions.** `action <name> key=value …` runs an action of another mod, for example `action item.add id=money/coin count=3` or `action quest.set_status quest_id=woodland_ward status=Progress`. The schema files of the mods list the actions, and Editor Tools offers them in its scenario editor and command palette.

## When a scenario plays

The header decides it:

```scenario
scene: prolog_alley
priority: 15
condition: AwakeningQuest.notHasTag("cloak_taken")
is_one_run: true
save_allowed: true
---
```

- `scene:` — the scene where it plays when the player arrives.
- `priority:` — order among scenarios of the same scene; the lowest number that may play goes first, and the next one starts when the dialog ends.
- `condition:` — it plays only while the condition holds; when a flag or quest changes, the scene checks again.
- `is_one_run: true` — it plays only once per game.
- `after:` — it waits until the named scenario has played.
- `save_allowed: true` — the player may save in this scene.

A scenario without a `scene:` header is not started by a scene; it is run by something else, such as a click on an interactive object or a map event.

## Other mods add and replace scenarios

Scenarios of all mods are collected together. A mod adds a story to someone else's scene simply by naming that scene. A mod that depends on another and ships a scenario with the same file name replaces it.

## If something goes wrong

1. **A scenario never plays.** Its `scene:` name is wrong, its `condition` is false, or it has `is_one_run` and has already played.
2. **The line is shown in the wrong language.** The `key=` is missing from the mod's `locale` files for that language; the quoted text is shown instead.
3. **An answer leads nowhere.** The label after `->` must exist in the same file, written with a colon (`:bar`).
4. **An `action` does nothing.** The mod that owns the action is not part of the game (the log says the action "is not registered"), or a parameter name is wrong — compare it with the action in the scenario editor of Editor Tools.
