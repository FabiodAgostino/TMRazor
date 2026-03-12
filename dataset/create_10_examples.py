import json

examples = [
    {
        "messages": [
            {"role": "system", "content": "You are a Razor Enhanced (IronPython) expert. You MUST ALWAYS reason inside <think></think> tags before writing the code."},
            {"role": "user", "content": "Write an IronPython script for Razor Enhanced that repeats the last spell I cast on the last target I had, creating a simple one-button repeat macro for farming."},
            {"role": "assistant", "content": "<think>User speaks English. APIs: Spells.CastLast, Target.WaitForTarget, Target.Last. IDs: None.</think>\nimport Spells, Target\n\nSpells.CastLast()\nTarget.WaitForTarget(1000, False)\nTarget.Last()"}
        ]
    },
    {
        "messages": [
            {"role": "system", "content": "You are a Razor Enhanced (IronPython) expert. You MUST ALWAYS reason inside <think></think> tags before writing the code."},
            {"role": "user", "content": "Scrivi uno script in IronPython per Razor Enhanced che lanci la spell Earthquake e poi attenda un tempo fisso prima di contare quanti nemici sono ancora vivi nel raggio, stampando il conteggio."},
            {"role": "assistant", "content": "<think>User speaks Italian. APIs: Spells.Cast, Misc.Pause, Mobiles.Filter, Mobiles.ApplyFilter, Misc.SendMessage. IDs: None.</think>\nimport Spells, Misc, Mobiles\n\nSpells.Cast(\"Earthquake\")\nMisc.Pause(2000)\n\nenemy_filter = Mobiles.Filter()\nenemy_filter.Enabled = True\nenemy_filter.RangeMax = 10\nenemy_filter.Notorieties = [3, 4, 5, 6]\n\nenemies = Mobiles.ApplyFilter(enemy_filter)\nMisc.SendMessage(\"Enemies still alive: \" + str(len(enemies)))"}
        ]
    },
    {
        "messages": [
            {"role": "system", "content": "You are a Razor Enhanced (IronPython) expert. You MUST ALWAYS reason inside <think></think> tags before writing the code."},
            {"role": "user", "content": "Escribe un script en IronPython para Razor Enhanced que use el hechizo Remove Curse en mí mismo siempre que el diario detecte la palabra 'cursed', e imprima cuántas veces se activó."},
            {"role": "assistant", "content": "<think>User speaks Spanish. APIs: Journal.SearchByType, Spells.Cast, Target.WaitForTarget, Target.Self, Misc.SendMessage, Journal.Clear. IDs: None.</think>\nimport Journal, Spells, Target, Misc\n\ntrigger_count = 0\nwhile True:\n    if Journal.SearchByType(\"cursed\", \"Regular\"):\n        Spells.Cast(\"Remove Curse\")\n        Target.WaitForTarget(1500, False)\n        Target.Self()\n        Journal.Clear()\n        trigger_count += 1\n        Misc.SendMessage(\"Remove Curse triggered: \" + str(trigger_count))\n    Misc.Pause(100)"}
        ]
    },
    {
        "messages": [
            {"role": "system", "content": "You are a Razor Enhanced (IronPython) expert. You MUST ALWAYS reason inside <think></think> tags before writing the code."},
            {"role": "user", "content": "Schreibe ein IronPython-Skript für Razor Enhanced, das Animal Form wirkt, um sich in einen Wolf zu verwandeln, und die Form automatisch abbricht, wenn ich innerhalb von 10 Kacheln zu meinem Ziel bin."},
            {"role": "assistant", "content": "<think>User speaks German. APIs: Spells.Cast, Misc.Pause, Player.Position, Misc.Distance. IDs: None.</think>\nimport Spells, Player, Misc\n\nSpells.Cast(\"Animal Form\")\nMisc.Pause(1000)\n\ndest_x = 1000\ndest_y = 1000\n\nwhile True:\n    if Misc.Distance(Player.Position.X, Player.Position.Y, dest_x, dest_y) <= 10:\n        Spells.Cast(\"Animal Form\")\n        break\n    Misc.Pause(500)"}
        ]
    },
    {
        "messages": [
            {"role": "system", "content": "You are a Razor Enhanced (IronPython) expert. You MUST ALWAYS reason inside <think></think> tags before writing the code."},
            {"role": "user", "content": "Напиши скрипт IronPython для Razor Enhanced, который вызывает Воздушного Элементаля, а затем сразу же использует систему целей, чтобы направить его атаковать ближайшего врага."},
            {"role": "assistant", "content": "<think>User speaks Russian. APIs: Spells.Cast, Target.WaitForTarget, Mobiles.Filter, Mobiles.ApplyFilter, Target.TargetExecute. IDs: None.</think>\nimport Spells, Target, Mobiles\n\nSpells.Cast(\"Summon Air Elemental\")\nTarget.WaitForTarget(2000, False)\n\nenemy_filter = Mobiles.Filter()\nenemy_filter.Enabled = True\nenemy_filter.RangeMax = 12\nenemy_filter.Notorieties = [3, 4, 5, 6]\n\nenemies = Mobiles.ApplyFilter(enemy_filter)\nif len(enemies) > 0:\n    Target.TargetExecute(enemies[0].Serial)"}
        ]
    },
    {
        "messages": [
            {"role": "system", "content": "You are a Razor Enhanced (IronPython) expert. You MUST ALWAYS reason inside <think></think> tags before writing the code."},
            {"role": "user", "content": "Write an IronPython script for Razor Enhanced that casts a Mysticism combo: cast Sleep on the target first, wait for confirmation, then cast Eagle Strike while the target is sleeping."},
            {"role": "assistant", "content": "<think>User speaks English. APIs: Spells.Cast, Target.WaitForTarget, Target.Last, Misc.Pause. IDs: None.</think>\nimport Spells, Target, Misc\n\nSpells.Cast(\"Sleep\")\nTarget.WaitForTarget(1500, False)\nTarget.Last()\n\nMisc.Pause(1500)\n\nSpells.Cast(\"Eagle Strike\")\nTarget.WaitForTarget(1500, False)\nTarget.Last()"}
        ]
    },
    {
        "messages": [
            {"role": "system", "content": "You are a Razor Enhanced (IronPython) expert. You MUST ALWAYS reason inside <think></think> tags before writing the code."},
            {"role": "user", "content": "Scrivi uno script IronPython per Razor Enhanced che implementa un ciclo di combattimento Paladino: lancia Divine Fury, poi alterna attacchi corpo a corpo e Consecrate Weapon."},
            {"role": "assistant", "content": "<think>User speaks Italian. APIs: Spells.Cast, Misc.Pause, Player.HasBuff. IDs: None.</think>\nimport Spells, Misc, Player\n\nSpells.Cast(\"Divine Fury\")\nMisc.Pause(1000)\n\nwhile True:\n    if not Player.HasBuff(\"Consecrate Weapon\"):\n        Spells.Cast(\"Consecrate Weapon\")\n        Misc.Pause(1000)\n    \n    Misc.Pause(500)"}
        ]
    },
    {
        "messages": [
            {"role": "system", "content": "You are a Razor Enhanced (IronPython) expert. You MUST ALWAYS reason inside <think></think> tags before writing the code."},
            {"role": "user", "content": "Escribe un script IronPython para Razor Enhanced que empiece el medidor de DPS cuando entre en modo de guerra y lo detenga al salir."},
            {"role": "assistant", "content": "<think>User speaks Spanish. APIs: Player.WarMode, Misc.SendMessage, Misc.Pause. IDs: None.</think>\nimport Player, Misc\n\nin_war_mode = False\n\nwhile True:\n    if Player.WarMode and not in_war_mode:\n        Misc.SendMessage(\"Started DPS meter\")\n        in_war_mode = True\n    elif not Player.WarMode and in_war_mode:\n        Misc.SendMessage(\"Stopped DPS meter\")\n        in_war_mode = False\n        break\n    Misc.Pause(100)"}
        ]
    },
    {
        "messages": [
            {"role": "system", "content": "You are a Razor Enhanced (IronPython) expert. You MUST ALWAYS reason inside <think></think> tags before writing the code."},
            {"role": "user", "content": "Schreibe ein IronPython-Skript für Razor Enhanced, das überwacht, ob mein aktuelles Angriffsziel noch am Leben ist. Wenn es stirbt, automatisch das nächste Ziel finden."},
            {"role": "assistant", "content": "<think>User speaks German. APIs: Target.GetLastAttack, Mobiles.FindBySerial, Mobiles.Filter, Mobiles.ApplyFilter, Player.Attack, Misc.Pause. IDs: None.</think>\nimport Target, Mobiles, Player, Misc\n\nwhile True:\n    target_serial = Target.GetLastAttack()\n    mob = Mobiles.FindBySerial(target_serial)\n    \n    if mob is None:\n        enemy_filter = Mobiles.Filter()\n        enemy_filter.Enabled = True\n        enemy_filter.RangeMax = 10\n        enemy_filter.Notorieties = [3, 4, 5, 6]\n        \n        enemies = Mobiles.ApplyFilter(enemy_filter)\n        if len(enemies) > 0:\n            Player.Attack(enemies[0].Serial)\n            \n    Misc.Pause(500)"}
        ]
    },
    {
        "messages": [
            {"role": "system", "content": "You are a Razor Enhanced (IronPython) expert. You MUST ALWAYS reason inside <think></think> tags before writing the code."},
            {"role": "user", "content": "编写一个 Razor Enhanced 的 IronPython 脚本，检测我何时受到攻击，通过跟踪日志来计算，并每 5 秒打印一次计数。"},
            {"role": "assistant", "content": "<think>User speaks Chinese. APIs: Journal.SearchByType, Misc.SendMessage, Misc.Pause, Journal.Clear. IDs: None.</think>\nimport Journal, Misc\n\nattacks = 0\n\nwhile True:\n    if Journal.SearchByType(\"attacking you\", \"System\"):\n        attacks += 1\n        Journal.Clear()\n    \n    Misc.SendMessage(\"Attacks received: \" + str(attacks))\n    Misc.Pause(5000)"}
        ]
    }
]

with open('dataset.jsonl', 'a', encoding='utf-8') as f:
    # Adding a newline first in case the file didn't end with one
    f.write('\n')
    for ex in examples:
        f.write(json.dumps(ex, ensure_ascii=False) + '\n')
