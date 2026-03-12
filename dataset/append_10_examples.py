import json
import io

examples = [
    {
        "messages": [
            {"role": "system", "content": "You are a Razor Enhanced (IronPython) expert. You MUST ALWAYS reason inside <think></think> tags before writing the code."},
            {"role": "user", "content": "Write an IronPython script for Razor Enhanced that implements a mana-efficient combat sequence: cast Energy Bolt when mana is above 50, cast Magic Arrow when mana is between 20 and 50, and stop casting entirely when below 20."},
            {"role": "assistant", "content": "<think>User speaks English. APIs: Player.Mana, Spells.Cast, Target.WaitForTarget, Target.Last, Misc.Pause. IDs: None.</think>\nimport Player, Spells, Target, Misc\n\nwhile True:\n    if Player.Mana > 50:\n        Spells.Cast(\"Energy Bolt\")\n        Target.WaitForTarget(2000, False)\n        Target.Last()\n    elif Player.Mana >= 20:\n        Spells.Cast(\"Magic Arrow\")\n        Target.WaitForTarget(2000, False)\n        Target.Last()\n    else:\n        Misc.Pause(500)\n    Misc.Pause(1500)"}
        ]
    },
    {
        "messages": [
            {"role": "system", "content": "You are a Razor Enhanced (IronPython) expert. You MUST ALWAYS reason inside <think></think> tags before writing the code."},
            {"role": "user", "content": "Scrivi uno script in IronPython per Razor Enhanced che usa Carpentry per craftare oggetti finché la skill non raggiunge 100. Controlla il livello della skill dopo ogni tentativo."},
            {"role": "assistant", "content": "<think>User speaks Italian. APIs: Player.GetSkillValue, Items.UseItemByID, Gumps.WaitForGump, Gumps.SendAction, Misc.Pause, Misc.SendMessage. IDs: 0x1030 (Carpentry tools).</think>\nimport Items, Gumps, Player, Misc\n\ncarpentry_tool = 0x1030\n\nwhile Player.GetSkillValue('Carpentry') < 100:\n    Items.UseItemByID(carpentry_tool, -1)\n    Gumps.WaitForGump(949095101, 2000)\n    Gumps.SendAction(949095101, 21)\n    Misc.Pause(2000)\n    \nMisc.SendMessage(\"Skill reached 100!\")"}
        ]
    },
    {
        "messages": [
            {"role": "system", "content": "You are a Razor Enhanced (IronPython) expert. You MUST ALWAYS reason inside <think></think> tags before writing the code."},
            {"role": "user", "content": "为Razor Enhanced编写一个IronPython脚本：实施死灵法师的开局连招。先对目标施放 Corpse Skin，等待1秒钟，然后再施放 Poison Strike。"},
            {"role": "assistant", "content": "<think>User speaks Chinese. APIs: Spells.Cast, Target.WaitForTarget, Target.Last, Misc.Pause. IDs: None.</think>\nimport Spells, Target, Misc\n\nSpells.Cast(\"Corpse Skin\")\nTarget.WaitForTarget(2000, False)\nTarget.Last()\n\nMisc.Pause(1000)\n\nSpells.Cast(\"Poison Strike\")\nTarget.WaitForTarget(2000, False)\nTarget.Last()"}
        ]
    },
    {
        "messages": [
            {"role": "system", "content": "You are a Razor Enhanced (IronPython) expert. You MUST ALWAYS reason inside <think></think> tags before writing the code."},
            {"role": "user", "content": "Напишите скрипт на IronPython для Razor Enhanced, который использует Cooking для постоянной выпечки хлеба. Каждый раз, когда в журнале появляется сообщение об успехе ('You put the'), перемещайте созданную еду в отдельную сумку."},
            {"role": "assistant", "content": "<think>User speaks Russian. APIs: Journal.Clear, Items.UseItemByID, Gumps.WaitForGump, Gumps.SendAction, Misc.Pause, Journal.Search, Items.FindByID, Items.Move. IDs: 0x1039 (Flour), 0x103B (Bread), 0x41234567 (Bag).</think>\nimport Items, Gumps, Journal, Misc\n\nflour_id = 0x1039\nbread_id = 0x103B\nbag_serial = 0x41234567\n\nwhile True:\n    Journal.Clear()\n    Items.UseItemByID(flour_id, -1)\n    Gumps.WaitForGump(949095101, 2000)\n    Gumps.SendAction(949095101, 10)\n    Misc.Pause(2000)\n    \n    if Journal.Search(\"You put the\"):\n        bread = Items.FindByID(bread_id, -1, -1)\n        if bread:\n            Items.Move(bread, bag_serial, 0)"}
        ]
    },
    {
        "messages": [
            {"role": "system", "content": "You are a Razor Enhanced (IronPython) expert. You MUST ALWAYS reason inside <think></think> tags before writing the code."},
            {"role": "user", "content": "Schreibe ein IronPython-Skript für Razor Enhanced, das eine defensive Kampfrotation ausführt: Wenn ein Feind innerhalb von 2 Kacheln ist, aktiviere Evasion. Wenn die Gesundheit unter 50% fällt, aktiviere Confidence. Verwende Bandagen (0x0E21) unter 40%."},
            {"role": "assistant", "content": "<think>User speaks German. APIs: Mobiles.Filter, Mobiles.ApplyFilter, Spells.Cast, Misc.Pause, Player.Hits, Player.HitsMax, Items.UseItemByID, Target.WaitForTarget, Target.Self. IDs: 0x0E21 (Bandages).</think>\nimport Mobiles, Spells, Player, Items, Target, Misc\n\nwhile True:\n    filtro = Mobiles.Filter()\n    filtro.Enabled = True\n    filtro.RangeMax = 2\n    filtro.Notorieties = [3, 4, 5, 6]\n    enemies = Mobiles.ApplyFilter(filtro)\n    \n    if enemies:\n        Spells.Cast(\"Evasion\")\n        Misc.Pause(1000)\n        \n    hp_percent = (Player.Hits / Player.HitsMax) * 100\n    if hp_percent < 40:\n        Items.UseItemByID(0x0E21, -1)\n        Target.WaitForTarget(1000, False)\n        Target.Self()\n        Misc.Pause(1000)\n    elif hp_percent < 50:\n        Spells.Cast(\"Confidence\")\n        Misc.Pause(1000)\n        \n    Misc.Pause(500)"}
        ]
    },
    {
        "messages": [
            {"role": "system", "content": "You are a Razor Enhanced (IronPython) expert. You MUST ALWAYS reason inside <think></think> tags before writing the code."},
            {"role": "user", "content": "Escribe un script en IronPython para Razor Enhanced que equipe el arma y active automáticamente la habilidad especial primaria antes de atacar, golpee una vez, espere 2 segundos, y luego la desactive para recuperar stamina."},
            {"role": "assistant", "content": "<think>User speaks Spanish. APIs: Player.WeaponPrimarySA, Player.AttackLast, Misc.Pause. IDs: None.</think>\nimport Player, Misc\n\nwhile True:\n    if not Player.WeaponPrimarySA:\n        Player.WeaponPrimarySA = True\n        \n    Player.AttackLast()\n    Misc.Pause(2000)\n    \n    if Player.WeaponPrimarySA:\n        Player.WeaponPrimarySA = False\n        \n    Misc.Pause(2000)"}
        ]
    },
    {
        "messages": [
            {"role": "system", "content": "You are a Razor Enhanced (IronPython) expert. You MUST ALWAYS reason inside <think></think> tags before writing the code."},
            {"role": "user", "content": "Write an IronPython script for Razor Enhanced that tracks how long each combat encounter lasts from when I enter war mode to when the current enemy target dies, and prints the duration in seconds."},
            {"role": "assistant", "content": "<think>User speaks English. APIs: Player.WarMode, Player.CurrentTarget, Mobiles.FindBySerial, Misc.Pause, Misc.SendMessage. IDs: None.</think>\nimport Player, Mobiles, Misc\n\nwhile True:\n    if Player.WarMode and Player.CurrentTarget:\n        target_serial = Player.CurrentTarget\n        target = Mobiles.FindBySerial(target_serial)\n        if target:\n            ticks = 0\n            while target and target.Hits > 0:\n                Misc.Pause(100)\n                ticks += 1\n                target = Mobiles.FindBySerial(target_serial)\n            seconds = ticks / 10.0\n            Misc.SendMessage(\"Combat lasted: \" + str(seconds) + \" seconds.\")\n    Misc.Pause(500)"}
        ]
    },
    {
        "messages": [
            {"role": "system", "content": "You are a Razor Enhanced (IronPython) expert. You MUST ALWAYS reason inside <think></think> tags before writing the code."},
            {"role": "user", "content": "Scrivi uno script in IronPython per Razor Enhanced che monitora i nemici entro 10 tile. Se uno di loro inizia a lanciare una magia (rilevato dal messaggio 'begins to cast' nel journal), stampa un allarme."},
            {"role": "assistant", "content": "<think>User speaks Italian. APIs: Journal.Clear, Journal.Search, Misc.SendMessage, Misc.Pause. IDs: None.</think>\nimport Journal, Misc\n\nJournal.Clear()\nwhile True:\n    if Journal.Search(\"begins to cast\"):\n        Misc.SendMessage(\"Allarme: Nemico sta lanciando una magia!\", 33)\n        Journal.Clear()\n    Misc.Pause(200)"}
        ]
    },
    {
        "messages": [
            {"role": "system", "content": "You are a Razor Enhanced (IronPython) expert. You MUST ALWAYS reason inside <think></think> tags before writing the code."},
            {"role": "user", "content": "编写一个IronPython脚本，如果目标敌人的生命值低于10%，则自动施放 Word of Death，并通过读取日志检查是否成功击杀。"},
            {"role": "assistant", "content": "<think>User speaks Chinese. APIs: Player.CurrentTarget, Mobiles.FindBySerial, Journal.Clear, Spells.Cast, Target.WaitForTarget, Target.TargetExecute, Misc.Pause, Journal.Search, Misc.SendMessage. IDs: None.</think>\nimport Player, Mobiles, Spells, Target, Journal, Misc\n\nwhile True:\n    target_serial = Player.CurrentTarget\n    if target_serial:\n        enemy = Mobiles.FindBySerial(target_serial)\n        if enemy and enemy.HitsMax > 0:\n            hp_percent = (enemy.Hits / enemy.HitsMax) * 100\n            if hp_percent < 10:\n                Journal.Clear()\n                Spells.Cast(\"Word of Death\")\n                Target.WaitForTarget(2000, False)\n                Target.TargetExecute(target_serial)\n                Misc.Pause(1000)\n                if Journal.Search(\"body of\"):\n                    Misc.SendMessage(\"Kill confirmed!\")\n    Misc.Pause(500)"}
        ]
    },
    {
        "messages": [
            {"role": "system", "content": "You are a Razor Enhanced (IronPython) expert. You MUST ALWAYS reason inside <think></think> tags before writing the code."},
            {"role": "user", "content": "Напишите скрипт, который автоматически находит и атакует врага с наивысшим уровнем известности (Notoriety) в пределах 12 тайлов при переходе в боевой режим."},
            {"role": "assistant", "content": "<think>User speaks Russian. APIs: Player.WarMode, Player.CurrentTarget, Mobiles.Filter, Mobiles.ApplyFilter, Player.Attack, Misc.Pause. IDs: None.</think>\nimport Player, Mobiles, Misc\n\nwhile True:\n    if Player.WarMode and not Player.CurrentTarget:\n        filtro = Mobiles.Filter()\n        filtro.Enabled = True\n        filtro.RangeMax = 12\n        filtro.Notorieties = [3, 4, 5, 6]\n        enemies = Mobiles.ApplyFilter(filtro)\n        \n        if enemies:\n            sorted_enemies = sorted(enemies, key=lambda m: m.Notoriety, reverse=True)\n            target = sorted_enemies[0]\n            Player.Attack(target.Serial)\n    Misc.Pause(500)"}
        ]
    }
]

with io.open('dataset.jsonl', 'a', encoding='utf-8') as f:
    for ex in examples:
        f.write(json.dumps(ex, ensure_ascii=False) + '\n')

print(f"Successfully appended {len(examples)} examples to dataset.jsonl")
