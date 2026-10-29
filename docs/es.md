# Dialog Engine — la caja de diálogo y el lenguaje de escenarios

Dialog Engine es lo que el jugador lee y pulsa: la caja de diálogo con sus líneas, retratos y respuestas. Detrás está el **lenguaje de escenarios** — pequeños archivos de texto `.scenario` en los que la historia se escribe línea a línea: quién dice qué, qué respuestas hay, adónde conduce cada una y qué ocurre en el juego (un fondo nuevo, música, un objeto, un paso de misión, otra escena). Con él cuentan su historia todos los juegos de la colección, desde la plantilla de novela hasta RPG Plus.

## Lo que ve el jugador

El escenario de una escena arranca solo cuando el jugador entra en ella. Las líneas aparecen en la caja inferior; un clic muestra la siguiente:

![El pueblo de My Novel: la calle de tarde, y en la caja de diálogo la línea «El pueblo, al anochecer. Las farolas ya están encendidas y un aire fresco corre entre los adoquines».](assets/town-text.jpg)

Cuando la historia hace una pregunta, las respuestas aparecen a la derecha, y espera a que se elija una:

![La pregunta «¿A dónde ir?» en la caja de diálogo y sus dos respuestas a la derecha: «Entrar al bar» y «Volver a casa».](assets/town-choice.jpg)

«Entrar al bar» lleva al jugador a la escena del bar, que arranca su propio escenario:

![El bar de My Novel con su primera línea en la caja de diálogo.](assets/bar-text.jpg)

## El escenario detrás de estas imágenes

Este es el escenario completo del pueblo de la plantilla de novela (`my_novel_game/scenarios/my_novel_town.scenario`):

```scenario
scene: my_novel_town
priority: 10
---
# El pueblo es el centro: en cada visita aparece la misma elección.
text "El pueblo, al anochecer. Las farolas ya están encendidas y un aire fresco corre entre los adoquines." key=my_novel/town/intro
select "¿A dónde ir?" key=my_novel/town/prompt
option "Entrar al bar" key=my_novel/town/opt_bar -> :bar
option "Volver a casa" key=my_novel/town/opt_home -> :home

:bar
switch_scene my_novel_bar
end

:home
switch_scene my_novel_home
end
```

- **La cabecera**, sobre `---`, dice cuándo se reproduce el escenario: `scene:` nombra la escena y `priority:` ordena varios escenarios de una misma escena (se reproduce antes el número más bajo). Más adelante hay más líneas de cabecera.
- `text "…"` muestra una línea y espera un clic. `key=` es la clave de traducción de la línea; el texto entrecomillado es lo que se muestra cuando no hay traducción.
- `select` plantea una pregunta, y cada `option` es una respuesta que salta a una **etiqueta** (`-> :bar`).
- `:bar` es una etiqueta; `switch_scene` pasa a otra escena; `end` termina el escenario.
- Una línea que empieza por `#` es una nota para el autor.

Para añadir una historia a una escena, coloca un archivo `.scenario` en cualquier punto de la carpeta `scenarios/` de tu mod y nombra la escena en la cabecera.

## Escribir una historia

**Líneas y retratos.** `text "…" first=marao/sad` muestra un retrato en el primer puesto, `second=…` en el segundo (véase Characters Engine). `ptext` hace lo mismo que `text` y además sustituye `${name}` por el valor de una variable. `title "…"` muestra el título de un capítulo.

**Opciones y saltos.** Una respuesta puede mostrarse solo mientras se cumpla una condición: `option "Pay" "if=item('money/coin') >= 5" -> :pay` (una condición con espacios se entrecomilla junto con `if=`). `goto -> :label` salta sin preguntar. `when "<condition>" -> :label` salta si la condición se cumple en ese momento:

```scenario
when "rpg_wolf_dead == 1" -> :lantern
```

**Recordar cosas.** `flag rpg_quest_taken 1` activa un marcador argumental que se guarda con la partida; las condiciones lo leen como `rpg_quest_taken == 1`. `set name value` asigna una variable del diálogo en curso: `ptext` la muestra como `${name}` y las acciones pueden leerla.

**Las condiciones** son un único lenguaje común: marcadores, `item('id')` de Items Base, comprobaciones de misiones de Quests Engine, atributos y equipo de sus respectivos mods, unidos con `&&`, `||` y `!`.

**La escena.** `background`, `foreground`, `background_color`, `music`, `sound`, `effect`, `delay`, `light_set`, `hide_dialog`/`show_dialog`, `interactive_add`/`interactive_remove`, `notify`.

**Acciones de juego.** `action <name> key=value …` ejecuta una acción de otro mod, por ejemplo `action item.add id=money/coin count=3` o `action quest.set_status quest_id=woodland_ward status=Progress`. Los archivos schema de los mods enumeran las acciones, y Editor Tools las ofrece en su editor de escenarios y en la paleta de comandos.

## Cuándo se reproduce un escenario

Lo decide la cabecera:

```scenario
scene: prolog_alley
priority: 15
condition: AwakeningQuest.notHasTag("cloak_taken")
is_one_run: true
save_allowed: true
---
```

- `scene:` — la escena donde se reproduce cuando el jugador llega.
- `priority:` — el orden entre los escenarios de una misma escena; va primero el número más bajo que pueda reproducirse, y el siguiente empieza cuando termina el diálogo.
- `condition:` — solo se reproduce mientras se cumple la condición; cuando cambia un marcador o una misión, la escena vuelve a comprobar.
- `is_one_run: true` — se reproduce una sola vez por partida.
- `after:` — espera a que se haya reproducido el escenario indicado.
- `save_allowed: true` — el jugador puede guardar en esta escena.

Un escenario sin cabecera `scene:` no lo arranca ninguna escena: lo ejecuta otra cosa, como un clic sobre un objeto interactivo o un evento del mapa.

## Los demás mods añaden y reemplazan escenarios

Los escenarios de todos los mods se reúnen en un mismo conjunto. Un mod añade una historia a la escena de otro con solo nombrarla. Y un mod que depende de otro e incluye un escenario con el mismo nombre de archivo lo reemplaza.

## Si algo falla

1. **El escenario nunca se reproduce.** El nombre de `scene:` es incorrecto, la `condition` es falsa, o tiene `is_one_run` y ya se reprodujo.
2. **La línea aparece en otro idioma.** Falta la clave `key=` en los archivos `locale` del mod para ese idioma, y se muestra el texto entrecomillado.
3. **Una respuesta no lleva a ninguna parte.** La etiqueta tras `->` debe existir en el mismo archivo, escrita con dos puntos (`:bar`).
4. **Una `action` no hace nada.** El mod dueño de la acción no forma parte del juego (el registro dice que la acción "is not registered") o hay un nombre de parámetro equivocado: compáralo con la acción en el editor de escenarios de Editor Tools.
