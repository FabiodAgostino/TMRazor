import json

examples = [
    {
        "messages": [
            {"role": "system", "content": "You are a Razor Enhanced (IronPython) expert. You MUST ALWAYS reason inside <think></think> tags before writing the code."},
            {"role": "user", "content": "Scrivi uno script IronPython per Razor Enhanced che controlli se ho tutti i reagenti necessari per una specifica pergamena prima di tentare di craftarla. Se manca un reagente, stampa quale e quanti ne mancano."},
            {"role": "assistant", "content": "<think>User speaks Italian. APIs: Items.BackpackCount, Misc.SendMessage. IDs: 0x0F7B, 0x0F7A.</think>\nimport Items, Misc\n\nbm_needed = 2\nbp_needed = 1\n\nbm_count = Items.BackpackCount(0x0F7B, -1)\nbp_count = Items.BackpackCount(0x0F7A, -1)\n\nmissing = False\nif bm_count < bm_needed:\n    Misc.SendMessage(\"Mancano \" + str(bm_needed - bm_count) + \" Blood Moss.\")\n    missing = True\nif bp_count < bp_needed:\n    Misc.SendMessage(\"Mancano \" + str(bp_needed - bp_count) + \" Black Pearl.\")\n    missing = True\n\nif not missing:\n    Misc.SendMessage(\"Reagenti sufficienti. Pronto per il crafting.\")"}
        ]
    },
    {
        "messages": [
            {"role": "system", "content": "You are a Razor Enhanced (IronPython) expert. You MUST ALWAYS reason inside <think></think> tags before writing the code."},
            {"role": "user", "content": "Schreibe ein IronPython-Skript für Razor Enhanced, das das Tinkering-Handwerksmenü öffnet, zur Kategorie 'Tools' navigiert, ein bestimmtes Werkzeug herstellt und zählt, wie viele ich am Ende der Sitzung produziert habe."},
            {"role": "assistant", "content": "<think>User speaks German. APIs: Items.UseItemByID, Gumps.WaitForGump, Gumps.SendAction, Items.BackpackCount. IDs: 0x1EB8, 0x0FBC.</think>\nimport Items, Gumps, Misc\n\nstart_count = Items.BackpackCount(0x0FBC, -1)\nItems.UseItemByID(0x1EB8, -1)\nGumps.WaitForGump(949095101, 2000)\nGumps.SendAction(949095101, 15)\nGumps.WaitForGump(949095101, 2000)\nGumps.SendAction(949095101, 23)\n\nMisc.Pause(3000)\nend_count = Items.BackpackCount(0x0FBC, -1)\nMisc.SendMessage(\"Produzierte Werkzeuge: \" + str(end_count - start_count))"}
        ]
    },
    {
        "messages": [
            {"role": "system", "content": "You are a Razor Enhanced (IronPython) expert. You MUST ALWAYS reason inside <think></think> tags before writing the code."},
            {"role": "user", "content": "Escribe un script en IronPython para Razor Enhanced que fabrique objetos con Blacksmithing y registre el número de éxitos y fracasos por separado. Cuando los fracasos superen los 5 seguidos, detente e imprime una advertencia."},
            {"role": "assistant", "content": "<think>User speaks Spanish. APIs: Items.UseItemByID, Gumps.WaitForGump, Gumps.SendAction, Journal.SearchByType, Journal.Clear, Misc.SendMessage. IDs: 0x13E3.</think>\nimport Items, Gumps, Journal, Misc\n\nsuccess = 0\nfailures = 0\nconsecutive_fails = 0\n\nwhile consecutive_fails <= 5:\n    Journal.Clear()\n    Items.UseItemByID(0x13E3, -1)\n    Gumps.WaitForGump(949095101, 2000)\n    Gumps.SendAction(949095101, 21)\n    Misc.Pause(3000)\n    \n    if Journal.SearchByType(\"You fail\", \"Regular\"):\n        failures += 1\n        consecutive_fails += 1\n    elif Journal.SearchByType(\"You create\", \"Regular\"):\n        success += 1\n        consecutive_fails = 0\n\nMisc.SendMessage(\"Demasiados fracasos consecutivos. Habilidad baja.\")"}
        ]
    },
    {
        "messages": [
            {"role": "system", "content": "You are a Razor Enhanced (IronPython) expert. You MUST ALWAYS reason inside <think></think> tags before writing the code."},
            {"role": "user", "content": "Напиши скрипт IronPython для Razor Enhanced, который автоматизирует Fletching: сначала разрежьте бревна на древки, затем соедините древки с перьями, чтобы сделать стрелы, и остановитесь, когда закончится любой из материалов."},
            {"role": "assistant", "content": "<think>User speaks Russian. APIs: Items.BackpackCount, Items.UseItemByID, Target.WaitForTarget, Target.TargetExecute, Gumps.WaitForGump, Gumps.SendAction. IDs: 0x1BDD, 0x1BD1, 0x1BD4, 0x0F3F.</think>\nimport Items, Target, Gumps, Misc\n\nwhile True:\n    logs = Items.BackpackCount(0x1BDD, -1)\n    feathers = Items.BackpackCount(0x1BD1, -1)\n    shafts = Items.BackpackCount(0x1BD4, -1)\n    \n    if logs == 0 and shafts == 0 or feathers == 0:\n        Misc.SendMessage(\"Материалы закончились!\")\n        break\n    \n    if shafts < 10 and logs > 0:\n        Items.UseItemByID(0x0F51, -1)\n        Target.WaitForTarget(1500, False)\n        Target.TargetExecute(0x1BDD)\n        Gumps.WaitForGump(949095101, 2000)\n        Gumps.SendAction(949095101, 14)\n        Misc.Pause(2000)\n    elif shafts > 0 and feathers > 0:\n        Items.UseItemByID(0x1BD4, -1)\n        Gumps.WaitForGump(949095101, 2000)\n        Gumps.SendAction(949095101, 21)\n        Misc.Pause(2000)"}
        ]
    },
    {
        "messages": [
            {"role": "system", "content": "You are a Razor Enhanced (IronPython) expert. You MUST ALWAYS reason inside <think></think> tags before writing the code."},
            {"role": "user", "content": "Write an IronPython script for Razor Enhanced that uses Carpentry to craft furniture items and, after each attempt, checks the journal for whether the item was created or the attempt failed, keeping a running total of both."},
            {"role": "assistant", "content": "<think>User speaks English. APIs: Items.UseItemByID, Gumps.WaitForGump, Gumps.SendAction, Journal.SearchByType, Journal.Clear, Misc.SendMessage. IDs: 0x1034.</think>\nimport Items, Gumps, Journal, Misc\n\nsuccess = 0\nfailures = 0\n\nfor i in range(10):\n    Journal.Clear()\n    Items.UseItemByID(0x1034, -1)\n    Gumps.WaitForGump(949095101, 2000)\n    Gumps.SendAction(949095101, 21)\n    Misc.Pause(3000)\n    \n    if Journal.SearchByType(\"You fail to create\", \"Regular\"):\n        failures += 1\n    elif Journal.SearchByType(\"You create the item\", \"Regular\"):\n        success += 1\n        \n    Misc.SendMessage(\"Success: \" + str(success) + \", Failures: \" + str(failures))"}
        ]
    },
    {
        "messages": [
            {"role": "system", "content": "You are a Razor Enhanced (IronPython) expert. You MUST ALWAYS reason inside <think></think> tags before writing the code."},
            {"role": "user", "content": "编写一个 Razor Enhanced 脚本，运行完整的炼金术会话：检查拥有最多成分的药水，酿造该类型直到成分不足，然后自动切换。"},
            {"role": "assistant", "content": "<think>User speaks Chinese. APIs: Items.BackpackCount, Items.UseItemByID, Gumps.WaitForGump, Gumps.SendAction. IDs: 0x0E9B, 0x0F7B, 0x0F88.</think>\nimport Items, Gumps, Misc\n\nwhile True:\n    bm_count = Items.BackpackCount(0x0F7B, -1)\n    ns_count = Items.BackpackCount(0x0F88, -1)\n    \n    if bm_count == 0 and ns_count == 0:\n        break\n        \n    Items.UseItemByID(0x0E9B, -1)\n    Gumps.WaitForGump(949095101, 2000)\n    \n    if bm_count > ns_count:\n        Gumps.SendAction(949095101, 21)\n    else:\n        Gumps.SendAction(949095101, 28)\n        \n    Misc.Pause(3000)"}
        ]
    },
    {
        "messages": [
            {"role": "system", "content": "You are a Razor Enhanced (IronPython) expert. You MUST ALWAYS reason inside <think></think> tags before writing the code."},
            {"role": "user", "content": "Scrivi uno script IronPython per Razor Enhanced che usi la skill Tinkering per creare parti di orologio e assemblarle in un orologio. Se l'assemblaggio fallisce, riprova fino a 3 volte prima di arrenderti."},
            {"role": "assistant", "content": "<think>User speaks Italian. APIs: Items.UseItemByID, Gumps.WaitForGump, Gumps.SendAction, Journal.Clear, Journal.SearchByType. IDs: 0x1EB8.</think>\nimport Items, Gumps, Journal, Misc\n\nretries = 0\nwhile retries < 3:\n    Items.UseItemByID(0x1EB8, -1)\n    Gumps.WaitForGump(949095101, 2000)\n    Gumps.SendAction(949095101, 42)\n    Misc.Pause(3000)\n    \n    Items.UseItemByID(0x1EB8, -1)\n    Gumps.WaitForGump(949095101, 2000)\n    Gumps.SendAction(949095101, 49)\n    Misc.Pause(3000)\n    \n    if Journal.SearchByType(\"You fail\", \"Regular\"):\n        retries += 1\n        Misc.SendMessage(\"Assemblaggio fallito, tentativo \" + str(retries))\n    else:\n        Misc.SendMessage(\"Orologio completato!\")\n        break"}
        ]
    },
    {
        "messages": [
            {"role": "system", "content": "You are a Razor Enhanced (IronPython) expert. You MUST ALWAYS reason inside <think></think> tags before writing the code."},
            {"role": "user", "content": "Write an IronPython script for Razor Enhanced that automates Inscription by casting a specific mage spell first to consume mana, then inscribing a corresponding scroll. It should track the current Inscription skill value and stop when capped."},
            {"role": "assistant", "content": "<think>User speaks English. APIs: Spells.Cast, Player.Mana, Items.UseItemByID, Gumps.WaitForGump, Gumps.SendAction, Player.GetSkillValue. IDs: 0x0FBF.</think>\nimport Spells, Player, Items, Gumps, Misc\n\nwhile True:\n    if Player.GetSkillValue(\"Inscription\") >= 100.0:\n        Misc.SendMessage(\"Inscription capped!\")\n        break\n        \n    if Player.Mana > 20:\n        Spells.Cast(\"Lightning\")\n        Misc.Pause(2000)\n        \n    Items.UseItemByID(0x0FBF, -1)\n    Gumps.WaitForGump(949095101, 2000)\n    Gumps.SendAction(949095101, 35)\n    Misc.Pause(4000)"}
        ]
    },
    {
        "messages": [
            {"role": "system", "content": "You are a Razor Enhanced (IronPython) expert. You MUST ALWAYS reason inside <think></think> tags before writing the code."},
            {"role": "user", "content": "Schreibe ein IronPython-Skript für Razor Enhanced, das Stärketränke mit Alchemie herstellt und, wenn ein Trankfass in der Nähe ist, jeden produzierten Trank automatisch in das Fass gießt."},
            {"role": "assistant", "content": "<think>User speaks German. APIs: Items.UseItemByID, Gumps.WaitForGump, Gumps.SendAction, Items.FindByID, Items.Move. IDs: 0x0E9B, 0x0F09, 0x1940.</think>\nimport Items, Gumps, Misc, Player\n\nkeg = Items.FindByID(0x1940, -1, Player.Backpack.Serial)\n\nfor i in range(5):\n    Items.UseItemByID(0x0E9B, -1)\n    Gumps.WaitForGump(949095101, 2000)\n    Gumps.SendAction(949095101, 28)\n    Misc.Pause(3000)\n    \n    potion = Items.FindByID(0x0F09, -1, Player.Backpack.Serial)\n    if potion and keg:\n        Items.Move(potion, keg.Serial, 1)\n        Misc.Pause(1000)"}
        ]
    },
    {
        "messages": [
            {"role": "system", "content": "You are a Razor Enhanced (IronPython) expert. You MUST ALWAYS reason inside <think></think> tags before writing the code."},
            {"role": "user", "content": "Escribe un script IronPython para Razor Enhanced que monitoree el peso de mi mochila durante una sesión de artesanía. Cuando el peso supere los 350, mueve automáticamente los objetos fabricados a un cofre cercano."},
            {"role": "assistant", "content": "<think>User speaks Spanish. APIs: Player.Weight, Items.FindByID, Items.Move, Items.UseItemByID, Gumps.WaitForGump, Gumps.SendAction. IDs: 0x1034, 0x0B22.</think>\nimport Player, Items, Gumps, Misc\n\nchest_serial = 0x40001234\n\nwhile True:\n    if Player.Weight > 350:\n        Misc.SendMessage(\"Mochila llena. Moviendo objetos...\")\n        crafted_item = Items.FindByID(0x0B22, -1, Player.Backpack.Serial)\n        if crafted_item:\n            Items.Move(crafted_item, chest_serial, 0)\n            Misc.Pause(1000)\n    else:\n        Items.UseItemByID(0x1034, -1)\n        Gumps.WaitForGump(949095101, 2000)\n        Gumps.SendAction(949095101, 21)\n        Misc.Pause(3000)"}
        ]
    }
]

with open('dataset.jsonl', 'a', encoding='utf-8') as f:
    for ex in examples:
        f.write(json.dumps(ex, ensure_ascii=False) + '\n')
