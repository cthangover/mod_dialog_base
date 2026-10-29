# Dialog Engine — 대화 상자와 시나리오 언어

Dialog Engine은 플레이어가 읽고 클릭하는 모든 것을 담당합니다. 대사·초상화·선택지가 표시되는 대화 상자이며, 그 뒤에는 **시나리오 언어**가 있습니다. `.scenario`라는 짧은 텍스트 파일에 이야기를 한 줄씩 적어 나가는 언어로, 누가 무엇을 말하는지, 어떤 선택지가 있는지, 각 선택지가 어디로 이어지는지, 게임 안에서 무슨 일이 일어나는지(배경 전환, 음악, 아이템, 퀘스트 진행, 다른 장면으로 이동)를 모두 적습니다. 컬렉션의 모든 게임이 소설 템플릿부터 RPG Plus까지 이 언어로 이야기를 전합니다.

## 플레이어가 보는 것

장면에 들어가면 그 장면의 시나리오가 저절로 시작됩니다. 대사는 화면 아래 상자에 나타나고, 클릭하면 다음 대사로 넘어갑니다.

![My Novel의 마을: 저녁 거리. 대화 상자에 "저녁 마을. 가로등이 이미 켜졌고, 돌바닥을 따라 차가운 바람이 불어 옵니다."라는 대사가 표시되어 있습니다.](assets/town-text.jpg)

이야기가 무언가를 물으면 선택지가 오른쪽에 나타나고, 플레이어가 고를 때까지 기다립니다.

![대화 상자의 질문 "어디로 갈까요?"와 오른쪽의 두 선택지 "술집에 들어가기", "집으로 돌아가기".](assets/town-choice.jpg)

"술집에 들어가기"를 고르면 플레이어는 술집 장면으로 이동하고, 그곳의 시나리오가 시작됩니다.

![My Novel의 술집. 대화 상자에는 첫 대사가 표시되어 있습니다.](assets/bar-text.jpg)

## 이 화면 뒤의 시나리오

소설 템플릿 마을 시나리오의 전문입니다(`my_novel_game/scenarios/my_novel_town.scenario`).

```scenario
scene: my_novel_town
priority: 10
---
# 마을은 허브:방문할 때마다 같은 선택지가 표시된다.
text "저녁 마을. 가로등은 이미 켜졌고, 돌바닥을 따라 차가운 바람이 불어 옵니다." key=my_novel/town/intro
select "어디로 갈까요?" key=my_novel/town/prompt
option "술집에 들어가기" key=my_novel/town/opt_bar -> :bar
option "집으로 돌아가기" key=my_novel/town/opt_home -> :home

:bar
switch_scene my_novel_bar
end

:home
switch_scene my_novel_home
end
```

- `---` 위의 **헤더**는 시나리오가 언제 재생되는지를 정합니다. `scene:`은 장면 이름, `priority:`는 같은 장면의 여러 시나리오 순서(작은 쪽이 먼저)입니다. 헤더 행은 아래에 더 있습니다.
- `text "…"`는 대사를 보여 주고 클릭을 기다립니다. `key=`는 대사의 번역 키이며, 따옴표 안의 텍스트는 번역이 없을 때 표시됩니다.
- `select`는 질문을 던지고, 각 `option`은 **레이블**( `-> :bar` )로 이동하는 선택지입니다.
- `:bar`는 레이블, `switch_scene`는 다른 장면으로 이동, `end`는 시나리오 종료입니다.
- `#`으로 시작하는 줄은 작성자를 위한 메모입니다.

장면에 이야기를 추가하려면 `.scenario` 파일을 자신의 mod `scenarios/` 폴더 어디에나 두고 헤더에 장면 이름을 적으면 됩니다.

## 이야기 쓰기

**대사와 초상화.** `text "…" first=marao/sad`는 첫 번째 자리에 초상화를, `second=…`는 두 번째 자리에 표시합니다(Characters Engine 참조). `ptext`는 `text`와 같으며 `${name}`을 변수 값으로 바꾸는 기능이 하나 더 있습니다. `title "…"`는 장 제목을 표시합니다.

**선택지와 점프.** 조건이 맞을 때만 선택지를 보이게 할 수 있습니다. `option "Pay" "if=item('money/coin') >= 5" -> :pay` (공백이 있는 조건은 `if=`까지 따옴표로 묶습니다). `goto -> :label`은 묻지 않고 건너뜁니다. `when "<condition>" -> :label`은 그 순간 조건이 맞으면 이동합니다.

```scenario
when "rpg_wolf_dead == 1" -> :lantern
```

**상태 기억.** `flag rpg_quest_taken 1`은 게임 저장에 함께 저장되는 스토리 플래그를 설정합니다. 조건에서는 `rpg_quest_taken == 1`로 읽습니다. `set name value`는 실행 중인 대화의 변수를 정합니다. `ptext`는 이를 `${name}`으로 표시하고 액션에서도 읽을 수 있습니다.

**조건**은 하나의 공용 언어입니다. 플래그, Items Base의 `item('id')`, Quests Engine의 퀘스트 확인, 각 mod의 스탯과 장비를 `&&`, `||`, `!`로 조합합니다.

**장면 연출.** `background`, `foreground`, `background_color`, `music`, `sound`, `effect`, `delay`, `light_set`, `hide_dialog`/`show_dialog`, `interactive_add`/`interactive_remove`, `notify`.

**게임 액션.** `action <name> key=value …`는 다른 mod의 액션을 실행합니다. 예를 들어 `action item.add id=money/coin count=3`, `action quest.set_status quest_id=woodland_ward status=Progress`입니다. 액션 목록은 각 mod의 스키마 파일에 있으며, Editor Tools의 시나리오 편집기와 커맨드 팔레트에서 바로 쓸 수 있습니다.

## 시나리오가 재생되는 시점

헤더가 결정합니다.

```scenario
scene: prolog_alley
priority: 15
condition: AwakeningQuest.notHasTag("cloak_taken")
is_one_run: true
save_allowed: true
---
```

- `scene:` — 플레이어가 도착했을 때 시나리오가 재생되는 장면.
- `priority:` — 같은 장면 시나리오끼리의 순서. 재생 가능한 가장 작은 번호가 먼저이며, 대화가 끝나면 다음이 시작됩니다.
- `condition:` — 조건이 유지되는 동안에만 재생. 플래그나 퀘스트가 바뀌면 장면이 다시 확인합니다.
- `is_one_run: true` — 게임당 한 번만 재생.
- `after:` — 지정한 시나리오가 재생될 때까지 기다립니다.
- `save_allowed: true` — 이 장면에서 저장할 수 있습니다.

`scene:` 헤더가 없는 시나리오는 장면이 시작하지 않습니다. 인터랙티브 오브젝트 클릭이나 맵 이벤트 같은 다른 계기가 실행합니다.

## 다른 mod의 추가와 교체

모든 mod의 시나리오는 한데 모입니다. mod는 장면 이름만 적으면 다른 mod의 장면에 이야기를 추가할 수 있습니다. 다른 mod에 의존하면서 같은 파일 이름의 시나리오를 포함한 mod는 그 시나리오를 교체합니다.

## 문제가 생겼을 때

1. **시나리오가 재생되지 않습니다.** `scene:` 이름이 틀렸거나, `condition`이 거짓이거나, `is_one_run`이 붙어 있어 이미 재생된 경우입니다.
2. **대사가 다른 언어로 표시됩니다.** 해당 언어의 번역 키 `key=`가 mod의 `locale` 파일에 없어서 따옴표 안의 텍스트가 그대로 보입니다.
3. **선택지가 어디로도 이어지지 않습니다.** `->` 뒤의 레이블은 같은 파일 안에 콜론을 붙여(`:bar`) 존재해야 합니다.
4. **`action`이 아무것도 하지 않습니다.** 액션을 소유한 mod가 게임에 없거나(로그에 "is not registered"라고 기록됨), 매개변수 이름이 틀렸습니다. Editor Tools의 시나리오 편집기에서 해당 액션과 비교해 보세요.
