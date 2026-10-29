# Dialog Engine — hộp hội thoại và ngôn ngữ kịch bản

Dialog Engine là mọi thứ người chơi đọc và nhấp vào: hộp hội thoại với các câu thoại, chân dung và phương án trả lời. Đằng sau đó là **ngôn ngữ kịch bản** — những tệp văn bản `.scenario` nhỏ, nơi câu chuyện được viết từng dòng: ai nói gì, có những lựa chọn nào, mỗi lựa chọn dẫn đi đâu, và chuyện gì xảy ra trong game (đổi nền, bật nhạc, thêm vật phẩm, tiến nhiệm vụ, sang cảnh khác). Mọi trò chơi trong bộ sưu tập, từ mẫu truyện đến RPG Plus, đều kể câu chuyện của mình bằng ngôn ngữ này.

## Những gì người chơi nhìn thấy

Kịch bản của một cảnh tự bắt đầu khi người chơi bước vào. Các câu thoại hiện trong hộp ở đáy màn hình; mỗi cú nhấp hiện câu tiếp theo.

![Thị trấn của My Novel: con phố buổi chiều, trong hộp hội thoại là câu "Buổi chiều ở thị trấn. Đèn đường đã sáng, một cơn gió mát thổi qua đường lát đá".](assets/town-text.jpg)

Khi câu chuyện hỏi điều gì đó, các lựa chọn hiện ra bên phải, và câu chuyện chờ cho đến khi người chơi chọn xong.

![Câu hỏi "Đi đâu đây?" trong hộp hội thoại cùng hai lựa chọn bên phải: "Ghé vào quán bar" và "Về nhà".](assets/town-choice.jpg)

"Ghé vào quán bar" đưa người chơi sang cảnh quán bar, nơi bắt đầu kịch bản riêng của nó.

![Quán bar của My Novel với câu thoại đầu tiên trong hộp hội thoại.](assets/bar-text.jpg)

## Kịch bản đứng sau những hình này

Đây là toàn bộ kịch bản thị trấn của mẫu truyện (`my_novel_game/scenarios/my_novel_town.scenario`).

```scenario
scene: my_novel_town
priority: 10
---
# Thị trấn là điểm tụ: lần nào ghé thăm cũng thấy cùng một lựa chọn.
text "Buổi chiều ở thị trấn. Đèn đường đã sáng, một cơn gió mát thổi qua đường lát đá." key=my_novel/town/intro
select "Đi đâu đây?" key=my_novel/town/prompt
option "Ghé vào quán bar" key=my_novel/town/opt_bar -> :bar
option "Về nhà" key=my_novel/town/opt_home -> :home

:bar
switch_scene my_novel_bar
end

:home
switch_scene my_novel_home
end
```

- **Phần đầu** phía trên `---` cho biết kịch bản chạy khi nào: `scene:` nêu tên cảnh, `priority:` sắp thứ tự cho nhiều kịch bản của cùng một cảnh (số nhỏ chạy trước). Bên dưới còn vài dòng đầu khác sẽ được giới thiệu.
- `text "…"` hiện một câu thoại và chờ cú nhấp. `key=` là khóa dịch của câu thoại; văn bản trong ngoặc kép được hiển thị khi không tìm thấy bản dịch.
- `select` đặt một câu hỏi, mỗi `option` là một lựa chọn nhảy tới một **nhãn** (`-> :bar`).
- `:bar` là nhãn; `switch_scene` chuyển sang cảnh khác; `end` kết thúc kịch bản.
- Dòng bắt đầu bằng `#` là ghi chú dành cho tác giả.

Muốn thêm câu chuyện vào một cảnh, bạn chỉ cần đặt tệp `.scenario` vào bất kỳ đâu trong thư mục `scenarios/` của mod và nêu tên cảnh trong phần đầu.

## Viết câu chuyện

**Câu thoại và chân dung.** `text "…" first=marao/sad` hiện chân dung ở vị trí thứ nhất, `second=…` ở vị trí thứ hai (xem Characters Engine). `ptext` giống `text` và thay thêm `${name}` bằng giá trị của biến. `title "…"` hiện tiêu đề chương.

**Lựa chọn và bước nhảy.** Có thể chỉ hiện một lựa chọn khi điều kiện thỏa mãn: `option "Pay" "if=item('money/coin') >= 5" -> :pay` (điều kiện có dấu cách thì bọc cả `if=` trong ngoặc kép). `goto -> :label` nhảy luôn không hỏi. `when "<condition>" -> :label` nhảy nếu điều kiện thỏa tại thời điểm đó.

```scenario
when "rpg_wolf_dead == 1" -> :lantern
```

**Ghi nhớ thông tin.** `flag rpg_quest_taken 1` đặt một cờ cốt truyện được lưu cùng game; điều kiện đọc nó dưới dạng `rpg_quest_taken == 1`. `set name value` đặt biến của phiên hội thoại đang chạy: `ptext` hiển thị nó dưới dạng `${name}` và các hành động cũng đọc được.

**Điều kiện** là một ngôn ngữ chung: cờ, `item('id')` từ Items Base, phép kiểm tra nhiệm vụ từ Quests Engine, chỉ số và trang bị từ mod tương ứng, nối với nhau bằng `&&`, `||` và `!`.

**Tổ chức cảnh.** `background`, `foreground`, `background_color`, `music`, `sound`, `effect`, `delay`, `light_set`, `hide_dialog`/`show_dialog`, `interactive_add`/`interactive_remove`, `notify`.

**Hành động của game.** `action <name> key=value …` chạy một hành động của mod khác, ví dụ `action item.add id=money/coin count=3` hay `action quest.set_status quest_id=woodland_ward status=Progress`. Các tệp schema của mod liệt kê hành động, còn Editor Tools đưa chúng vào trình soạn thảo kịch bản và bảng lệnh.

## Khi nào kịch bản chạy

Phần đầu quyết định điều đó.

```scenario
scene: prolog_alley
priority: 15
condition: AwakeningQuest.notHasTag("cloak_taken")
is_one_run: true
save_allowed: true
---
```

- `scene:` — cảnh mà kịch bản chạy khi người chơi tới.
- `priority:` — thứ tự giữa các kịch bản của cùng một cảnh; số nhỏ nhất được phép chạy sẽ chạy trước, kịch bản kế tiếp bắt đầu khi hội thoại kết thúc.
- `condition:` — chỉ chạy khi điều kiện còn thỏa mãn; khi cờ hoặc nhiệm vụ đổi, cảnh sẽ kiểm tra lại.
- `is_one_run: true` — chỉ chạy một lần mỗi ván chơi.
- `after:` — chờ cho đến khi kịch bản được nêu đã chạy xong.
- `save_allowed: true` — người chơi được lưu trong cảnh này.

Kịch bản không có dòng `scene:` thì không do cảnh khởi chạy; nó do thứ khác chạy, chẳng hạn cú nhấp vào vật thể tương tác hay sự kiện bản đồ.

## Mod khác thêm và thay thế kịch bản

Kịch bản của mọi mod được gom chung với nhau. Một mod thêm câu chuyện vào cảnh của mod khác đơn giản chỉ bằng cách nêu tên cảnh đó. Mod phụ thuộc vào mod khác và kèm theo một tệp kịch bản trùng tên sẽ thay thế tệp ấy.

## Khi có trục trặc

1. **Kịch bản không bao giờ chạy.** Tên trong `scene:` sai, `condition` sai, hoặc kịch bản có `is_one_run` và đã chạy rồi.
2. **Câu thoại hiện sai ngôn ngữ.** Thiếu khóa `key=` trong các tệp `locale` của mod cho ngôn ngữ đó; văn bản trong ngoặc kép được hiển thị thay thế.
3. **Lựa chọn dẫn đến hư không.** Nhãn sau `->` phải tồn tại trong cùng tệp, viết kèm dấu hai chấm (`:bar`).
4. **`action` không làm gì cả.** Mod sở hữu hành động không có trong game (log ghi hành động "is not registered"), hoặc tên tham số sai — hãy so với hành động trong trình soạn thảo kịch bản của Editor Tools.
