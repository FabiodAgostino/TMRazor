import json

examples = [
    {
        "role_user": "Spiele einen lokalen Piepton ab, wenn meine Gesundheit unter 30% fällt, um mich zu warnen, auch wenn ich nicht auf den Bildschirm schaue.",
        "think": "User speaks German. APIs: Player.Hits, Player.HitsMax, Misc.Beep, Misc.Pause. IDs: .",
        "code": "import Player, Misc\n\nwhile True:\n    if Player.HitsMax > 0:\n        hp_percent = (Player.Hits / Player.HitsMax) * 100\n        if hp_percent < 30:\n            Misc.Beep()\n    Misc.Pause(1000)"
    },
    {
        "role_user": "检查我的 Bless 增益在过期前还剩多少时间，并打印剩余的秒数。",
        "think": "User speaks Chinese. APIs: Player.GetBuffInfo, Misc.SendMessage. IDs: .",
        "code": "import Player, Misc\n\nbuff = Player.GetBuffInfo(\"Bless\")\nif buff:\n    Misc.SendMessage(\"Bless time left: \" + str(buff.Duration) + \"s\", 68)\nelse:\n    Misc.SendMessage(\"Bless is not active.\", 33)"
    },
    {
        "role_user": "Controlla se il mio personaggio ha la mossa speciale primaria dell'arma attuale pronta all'uso, e stampa il risultato.",
        "think": "User speaks Italian. APIs: Player.HasPrimarySpecial, Misc.SendMessage. IDs: .",
        "code": "import Player, Misc\n\nif Player.HasPrimarySpecial:\n    Misc.SendMessage(\"Mossa speciale primaria pronta!\", 68)\nelse:\n    Misc.SendMessage(\"Mossa speciale primaria non pronta.\", 33)"
    },
    {
        "role_user": "Нарисуйте стрелку отслеживания на моем экране, указывающую на определенный набор координат, например, как маркер путевой точки.",
        "think": "User speaks Russian. APIs: Player.TrackingArrow. IDs: .",
        "code": "import Player\n\n# Установить стрелку отслеживания на координаты X: 1500, Y: 1500\nPlayer.TrackingArrow(True, 1500, 1500)"
    },
    {
        "role_user": "Busca todos los objetos con un nombre específico en cualquier parte de mi mochila, e imprime cada coincidencia encontrada.",
        "think": "User speaks Spanish. APIs: Items.Filter, Items.ApplyFilter, Misc.SendMessage, Player.Backpack. IDs: .",
        "code": "import Items, Player, Misc\n\nf = Items.Filter()\nf.Enabled = True\nf.Name = \"specific name\"\nf.IsCorpse = -1\n\nfound = Items.ApplyFilter(f)\nfor item in found:\n    if item.RootContainer == Player.Backpack.Serial:\n        Misc.SendMessage(\"Encontrado: \" + item.Name + \" [\" + hex(item.Serial) + \"]\", 68)"
    },
    {
        "role_user": "Add all arrows (0x0F3F) and bolts (0x1BFB) in my backpack to a temporary ignore list so they are excluded from any item filter searches.",
        "think": "User speaks English. APIs: Player.Backpack, Misc.IgnoreObject, Misc.SendMessage. IDs: 0x0F3F, 0x1BFB.",
        "code": "import Player, Misc\n\narrows_and_bolts = [0x0F3F, 0x1BFB]\n\nif Player.Backpack:\n    for item in Player.Backpack.Contains:\n        if item.ItemID in arrows_and_bolts:\n            Misc.IgnoreObject(item.Serial)\n            Misc.SendMessage(\"Ignored: \" + item.Name, 33)"
    },
    {
        "role_user": "Berechnen Sie die genaue Entfernung in Kacheln zwischen zwei beliebigen Kartenkoordinaten und geben Sie das Ergebnis aus.",
        "think": "User speaks German. APIs: Misc.Distance, Misc.SendMessage. IDs: .",
        "code": "import Misc\n\ndist = Misc.Distance(1200, 1500, 1250, 1530)\nMisc.SendMessage(\"Entfernung: \" + str(dist) + \" Kacheln\", 99)"
    },
    {
        "role_user": "检查给定的序列号是属于物品还是生物，并打印结果。",
        "think": "User speaks Chinese. APIs: Items.FindBySerial, Mobiles.FindBySerial, Misc.SendMessage. IDs: .",
        "code": "import Items, Mobiles, Misc\n\ntest_serial = 0x40001234\nitem = Items.FindBySerial(test_serial)\nmob = Mobiles.FindBySerial(test_serial)\n\nif item:\n    Misc.SendMessage(\"这是一个物品: \" + item.Name, 68)\nelif mob:\n    Misc.SendMessage(\"这是一个生物: \" + mob.Name, 68)\nelse:\n    Misc.SendMessage(\"未找到序列号\", 33)"
    },
    {
        "role_user": "Usa una ricerca rapida per trovare il nemico più vicino di un tipo specifico di mostro entro 10 tile ed evidenzialo.",
        "think": "User speaks Italian. APIs: Mobiles.Filter, Mobiles.ApplyFilter, Misc.SendMessage. IDs: 0x000D.",
        "code": "import Mobiles, Misc\n\nf = Mobiles.Filter()\nf.Enabled = True\nf.RangeMax = 10\nf.Bodies = [0x000D] # Orc\nf.Notorieties = [3, 4, 5, 6]\n\nenemies = Mobiles.ApplyFilter(f)\nif enemies:\n    nearest = enemies[0]\n    Misc.SendMessage(\"Trovato mostro: \" + nearest.Name, 68)\nelse:\n    Misc.SendMessage(\"Nessun mostro trovato.\", 33)"
    },
    {
        "role_user": "Прочитайте значения славы и кармы последнего существа, на которое я нажал, и выведите их вместе с титулом кармы.",
        "think": "User speaks Russian. APIs: Target.GetLast, Mobiles.FindBySerial, Misc.SendMessage. IDs: .",
        "code": "import Target, Mobiles, Misc\n\nlast_target = Target.GetLast()\nmob = Mobiles.FindBySerial(last_target)\n\nif mob:\n    Misc.SendMessage(\"Имя: \" + mob.Name, 99)\n    Misc.SendMessage(\"Слава: \" + str(mob.Fame) + \" Карма: \" + str(mob.Karma), 99)\n    Misc.SendMessage(\"Титул: \" + str(mob.KarmaTitle), 99)\nelse:\n    Misc.SendMessage(\"Существо не найдено.\", 33)"
    }
]

lines = []
for ex in examples:
    obj = {
        "messages": [
            {"role": "system", "content": "You are a Razor Enhanced (IronPython) expert. You MUST ALWAYS reason inside <think></think> tags before writing the code."},
            {"role": "user", "content": ex["role_user"]},
            {"role": "assistant", "content": f"<think>{ex['think']}</think>\\n{ex['code']}"}
        ]
    }
    lines.append(json.dumps(obj, ensure_ascii=False))

with open("dataset.jsonl", "a", encoding="utf-8") as f:
    for line in lines:
        f.write(line + "\\n")

print("Added 10 examples to dataset.jsonl successfully.")
