import json
import random

system_prompt = "You are a Razor Enhanced scripting assistant for Ultima Online. You write correct IronPython scripts using only the official Razor Enhanced API. Never invent or guess methods, properties, or classes. If a task cannot be done with the available API, say so explicitly. Respond in the same language as the user."

data = [
    {"lang": "EN", "mod": "Player", "type": "Proprietà", "name": "StaminaIncrease", "ret": "Int32", "params": "N/A", "desc": "Quantitative expansion derived from magical clothing."},
    {"lang": "IT", "mod": "Player", "type": "Proprietà", "name": "StaminaRegeneration", "ret": "Int32", "params": "N/A", "desc": "Tasso di compensazione passivo dello stress fisico da corsa e swing."},
    {"lang": "ES", "mod": "Player", "type": "Proprietà", "name": "StatCap", "ret": "Int32", "params": "N/A", "desc": "Límite fisiológico (a menudo 225-255) más allá del cual las estadísticas en bruto no aumentan."},
    {"lang": "DE", "mod": "Player", "type": "Proprietà", "name": "StaticMount", "ret": "Int32", "params": "N/A", "desc": "Gespeicherte ID im Grafikfilter des Programms, falls vorhanden."},
    {"lang": "ZH", "mod": "Player", "type": "Proprietà", "name": "Str", "ret": "Int32", "params": "N/A", "desc": "决定生命值和重量基础的原始物理力量。"},
    {"lang": "RU", "mod": "Player", "type": "Proprietà", "name": "StrengthIncrease", "ret": "Int32", "params": "N/A", "desc": "Сумма дополнительных статистических данных для силы."},
    {"lang": "EN", "mod": "Player", "type": "Proprietà", "name": "SwingSpeedIncrease", "ret": "Int32", "params": "N/A", "desc": "Accelerated modulation of weapon animation frequency (SSI)."},
    {"lang": "IT", "mod": "Player", "type": "Proprietà", "name": "Visible", "ret": "Boolean", "params": "N/A", "desc": "Valutazione booleana dello stato nascosto al piano server globale."},
    {"lang": "ES", "mod": "Player", "type": "Proprietà", "name": "WarMode", "ret": "Boolean", "params": "N/A", "desc": "Bandera que habilita el seguimiento agresivo de colisiones y la autodefensa."},
    {"lang": "DE", "mod": "Player", "type": "Proprietà", "name": "Weight", "ret": "Int32", "params": "N/A", "desc": "Aktuelle logistische Berechnung der transportierten Masse."},
    {"lang": "ZH", "mod": "Player", "type": "Proprietà", "name": "YellowHits", "ret": "Boolean", "params": "N/A", "desc": "通过修改界面调色板分析治疗阻断信号。"},
    {"lang": "RU", "mod": "Player", "type": "Metodo", "name": "Area", "ret": "String", "params": "N/A", "desc": "Извлекает местный географический топоним, сравнивая координаты в regions.json."},
    {"lang": "EN", "mod": "Player", "type": "Metodo", "name": "Attack", "ret": "Void", "params": "serial", "desc": "Forces the start of the melee routine by directing the combat packet to the serial."},
    {"lang": "IT", "mod": "Player", "type": "Metodo", "name": "AttackLast", "ret": "Void", "params": "N/A", "desc": "Replica istantaneamente l'intento offensivo sull'ultimo avversario cacheato."},
    {"lang": "ES", "mod": "Player", "type": "Metodo", "name": "AttackType", "ret": "Boolean", "params": "graphic, rangemax, selector, color, notoriety", "desc": "Detección compleja y auto-enfrentamiento inmediato omitiendo la configuración de filtros."},
    {"lang": "DE", "mod": "Player", "type": "Metodo", "name": "BuffTime", "ret": "Int32", "params": "buffname", "desc": "Berechnet und drückt die verbleibenden Millisekunden vor Ablauf des angegebenen Buffs aus."},
    {"lang": "ZH", "mod": "Player", "type": "Metodo", "name": "BuffsExist", "ret": "Boolean", "params": "buffname, okayToGuess", "desc": "对活动增益的哈希集进行超快速迭代，用于条件语句。"},
    {"lang": "RU", "mod": "Player", "type": "Metodo", "name": "ChatAlliance", "ret": "Void", "params": "msg", "desc": "Текстовое вещание в ограниченную сеть альянса гильдий."},
    {"lang": "EN", "mod": "Player", "type": "Metodo", "name": "ChatChannel", "ret": "Void", "params": "msg", "desc": "Writes in global or custom chat channels instantiated by the client."},
    {"lang": "IT", "mod": "Player", "type": "Metodo", "name": "ChatEmote", "ret": "Void", "params": "color, msg", "desc": "Produce l'effetto di interpretazione ruolo in terza persona con colore scelto."},
    {"lang": "ES", "mod": "Player", "type": "Metodo", "name": "ChatGuild", "ret": "Void", "params": "msg", "desc": "Aislamiento textual a la jerarquía restringida de la hermandad."},
    {"lang": "DE", "mod": "Player", "type": "Metodo", "name": "ChatParty", "ret": "Void", "params": "msg, recepient_serial", "desc": "Flüstert direkte Nachrichten an ein entferntes Mitglied und umgeht Bereichs-Listener."},
    {"lang": "ZH", "mod": "Player", "type": "Metodo", "name": "ChatSay", "ret": "Void", "params": "color, msg", "desc": "通过视觉上操作字符颜色的十六进制来以纯文本进行径向传播。"},
    {"lang": "RU", "mod": "Player", "type": "Metodo", "name": "ChatWhisper", "ret": "Void", "params": "color, msg", "desc": "Имитирует ограниченную фонетику, ограничивая радиус отправки текста до 1-2 плиток."},
    {"lang": "EN", "mod": "Player", "type": "Metodo", "name": "ChatYell", "ret": "Void", "params": "color, msg", "desc": "Extension of the radial transmission range for town or group alerts."},
    {"lang": "IT", "mod": "Player", "type": "Metodo", "name": "CheckLayer", "ret": "Boolean", "params": "layer", "desc": "Identifica collisioni d'uso prima di eseguire cambi abito fallimentari."},
    {"lang": "ES", "mod": "Player", "type": "Metodo", "name": "ClearCorpseList", "ret": "Void", "params": "N/A", "desc": "Vacía las entradas de la matriz del navegador de guardado de muertes."},
    {"lang": "DE", "mod": "Player", "type": "Metodo", "name": "DistanceTo", "ret": "Int32", "params": "target", "desc": "Abstraktion für pythagoreische Messung ohne direkten Zugriff auf Raumvektoren."},
    {"lang": "ZH", "mod": "Player", "type": "Metodo", "name": "EmoteAction", "ret": "Void", "params": "action", "desc": "强制触发化身的物理动画（鞠躬，敬礼）。"},
    {"lang": "RU", "mod": "Player", "type": "Metodo", "name": "EquipItem", "ret": "Void", "params": "serial", "desc": "Отправляет пакет привязки, принудительно устанавливая соответствующее анатомическое место."},
    {"lang": "EN", "mod": "Player", "type": "Metodo", "name": "EquipLastWeapon", "ret": "Void", "params": "N/A", "desc": "Executes the logical macro toggle for the most recently held weapon in the RightHand node."},
    {"lang": "IT", "mod": "Player", "type": "Metodo", "name": "EquipUO3D", "ret": "Void", "params": "serials", "desc": "Aggrega istruzioni UO3D incapsulandole in cluster massivi minimizzando le conferme di rete."},
    {"lang": "ES", "mod": "Player", "type": "Metodo", "name": "Fly", "ret": "Void", "params": "status", "desc": "Genera la señal de levitación que permite caminar sobre obstáculos terrestres predefinidos."},
    {"lang": "DE", "mod": "Player", "type": "Metodo", "name": "GetBuffInfo", "ret": "BuffInfo", "params": "buffName, okayToGuess", "desc": "Fordert die vollständige strukturierte Untersuchung der Dauer und Intensität einer Änderung an."},
    {"lang": "ZH", "mod": "Player", "type": "Metodo", "name": "GetItemOnLayer", "ret": "Item", "params": "layer", "desc": "提取并具体化受保护实体以进行随后的库存操作。"},
    {"lang": "RU", "mod": "Player", "type": "Metodo", "name": "GetPropStringByIndex", "ret": "String", "params": "index", "desc": "Позволяет читать базовые значения (часто встроенные в строки клиентом), извлекая их позиционно."},
    {"lang": "EN", "mod": "Player", "type": "Metodo", "name": "GetPropStringList", "ret": "List", "params": "N/A", "desc": "Makes accessible the mass of passively associated tooltip data."},
    {"lang": "IT", "mod": "Player", "type": "Metodo", "name": "GetPropValue", "ret": "Int32", "params": "name", "desc": "Automatizza la decodifica delle diciture di abilità trasformandole in int32 sicuri."},
    {"lang": "ES", "mod": "Player", "type": "Metodo", "name": "GetRealSkillValue", "ret": "Double", "params": "skillname", "desc": "Aislamiento quirúrgico de la habilidad verdadera descartando artificios mágicos."},
    {"lang": "DE", "mod": "Player", "type": "Metodo", "name": "GetSkillCap", "ret": "Double", "params": "skillname", "desc": "Gibt die theoretische Lerngrenze zurück, um zu bewerten, ob das Training der Fähigkeit nützlich ist."},
    {"lang": "ZH", "mod": "Player", "type": "Metodo", "name": "GetSkillStatus", "ret": "Int32", "params": "skillname", "desc": "检测增长障碍以防止意外损失经验。"},
    {"lang": "RU", "mod": "Player", "type": "Metodo", "name": "GetSkillValue", "ret": "Double", "params": "skillname", "desc": "Вызывает компетенцию, искаженную текущими улучшениями, для проверки эффективности."},
    {"lang": "EN", "mod": "Player", "type": "Metodo", "name": "GetStatStatus", "ret": "Int32", "params": "statname", "desc": "Accesses the player's macro preferences for the increment of STR, DEX, INT."},
    {"lang": "IT", "mod": "Player", "type": "Metodo", "name": "GuildButton", "ret": "Void", "params": "N/A", "desc": "Forza il pacchetto UI bypassando il caricamento dell'interfaccia cartacea Paperdoll."},
    {"lang": "ES", "mod": "Player", "type": "Metodo", "name": "HeadMessage", "ret": "Void", "params": "color, msg", "desc": "Emite un aviso invisible a otros jugadores sobre el avatar, ideal para la depuración."},
    {"lang": "DE", "mod": "Player", "type": "Metodo", "name": "InRange", "ret": "Boolean", "params": "entity, range", "desc": "Hocheffizienter interner Prozess zur geometrischen Validierung von Angriffsbedingungen."},
    {"lang": "ZH", "mod": "Player", "type": "Metodo", "name": "InRangeItem", "ret": "Boolean", "params": "item, range", "desc": "空间扩展，用于预防性检查掉落距离。"},
    {"lang": "RU", "mod": "Player", "type": "Metodo", "name": "InRangeMobile", "ret": "Boolean", "params": "mobile, range", "desc": "Аналогичная проверка для определения возможности лечения/рукопашного боя."},
    {"lang": "EN", "mod": "Player", "type": "Metodo", "name": "InvokeVirtue", "ret": "Void", "params": "virtue", "desc": "Activation of abilities linked to the ethical and honorific points of the latest expansion."},
    {"lang": "IT", "mod": "Player", "type": "Metodo", "name": "KickMember", "ret": "Void", "params": "serial", "desc": "Espulsione comandata di nodi dal gruppo logico se in possesso dei diritti di leader."},
    {"lang": "ES", "mod": "Player", "type": "Metodo", "name": "LeaveParty", "ret": "Void", "params": "force", "desc": "Genera el comando asíncrono para el abandono voluntario sin pasar por Gump."}
]

def get_prompts(lang, mod, type_api, name, ret, params, desc):
    if lang == "EN":
        tipo_trad = "Property" if type_api == "Proprietà" else "Method"
        desc_trad = "Description"
        ret_trad = "Return"
        params_trad = "Parameters"
        ex_trad = "Example"
        q1 = f"What is the function of `{mod}.{name}`?"
        q2 = f"Could you explain what `{mod}.{name}` is used for?"
        if type_api == "Proprietà":
            code = f"result = {mod}.{name}\\n# This is a property, not a method"
        else:
            args = "" if params == "N/A" else params
            code = f"{mod}.{name}({args})" if ret == "Void" else f"result = {mod}.{name}({args})"
    elif lang == "IT":
        tipo_trad = "Proprietà" if type_api == "Proprietà" else "Metodo"
        desc_trad = "Descrizione"
        ret_trad = "Ritorno"
        params_trad = "Parametri"
        ex_trad = "Esempio"
        q1 = f"Qual è la funzione di `{mod}.{name}`?"
        q2 = f"Spiega lo scopo di `{mod}.{name}`"
        if type_api == "Proprietà":
            code = f"result = {mod}.{name}\\n# Questa è una proprietà, non un metodo"
        else:
            args = "" if params == "N/A" else params
            code = f"{mod}.{name}({args})" if ret == "Void" else f"result = {mod}.{name}({args})"
    elif lang == "ES":
        tipo_trad = "Propiedad" if type_api == "Proprietà" else "Método"
        desc_trad = "Descripción"
        ret_trad = "Retorno"
        params_trad = "Parámetros"
        ex_trad = "Ejemplo"
        q1 = f"¿Cuál es la función de `{mod}.{name}`?"
        q2 = f"Explica el uso de `{mod}.{name}`."
        if type_api == "Proprietà":
            code = f"result = {mod}.{name}\\n# Esta es una propiedad, no un método"
        else:
            args = "" if params == "N/A" else params
            code = f"{mod}.{name}({args})" if ret == "Void" else f"result = {mod}.{name}({args})"
    elif lang == "DE":
        tipo_trad = "Eigenschaft" if type_api == "Proprietà" else "Methode"
        desc_trad = "Beschreibung"
        ret_trad = "Rückgabe"
        params_trad = "Parameter"
        ex_trad = "Beispiel"
        q1 = f"Was ist die Funktion von `{mod}.{name}`?"
        q2 = f"Erkläre den Zweck von `{mod}.{name}`."
        if type_api == "Proprietà":
            code = f"result = {mod}.{name}\\n# Dies ist eine Eigenschaft, keine Methode"
        else:
            args = "" if params == "N/A" else params
            code = f"{mod}.{name}({args})" if ret == "Void" else f"result = {mod}.{name}({args})"
    elif lang == "ZH":
        tipo_trad = "属性" if type_api == "Proprietà" else "方法"
        desc_trad = "描述"
        ret_trad = "返回"
        params_trad = "参数"
        ex_trad = "示例"
        q1 = f"`{mod}.{name}` 的功能是什么？"
        q2 = f"请解释 `{mod}.{name}` 的用途。"
        if type_api == "Proprietà":
            code = f"result = {mod}.{name}\\n# 这是一个属性，不是一个方法"
        else:
            args = "" if params == "N/A" else params
            code = f"{mod}.{name}({args})" if ret == "Void" else f"result = {mod}.{name}({args})"
    elif lang == "RU":
        tipo_trad = "Свойство" if type_api == "Proprietà" else "Метод"
        desc_trad = "Описание"
        ret_trad = "Возвращаемое значение"
        params_trad = "Параметры"
        ex_trad = "Пример"
        q1 = f"Какова функция `{mod}.{name}`?"
        q2 = f"Объясните назначение `{mod}.{name}`."
        if type_api == "Proprietà":
            code = f"result = {mod}.{name}\\n# Это свойство, а не метод"
        else:
            args = "" if params == "N/A" else params
            code = f"{mod}.{name}({args})" if ret == "Void" else f"result = {mod}.{name}({args})"

    assistant_content = f"{mod}.{name} {('is a' if lang=='EN' else 'è un' if lang=='IT' and type_api=='Metodo' else 'è una' if lang=='IT' else 'es un' if lang=='ES' and type_api=='Metodo' else 'es una' if lang=='ES' else 'ist eine' if lang=='DE' and type_api=='Proprietà' else 'ist ein' if lang=='DE' else '是一个' if lang=='ZH' else 'это')} {tipo_trad} {('of' if lang=='EN' else 'di' if lang=='IT' else 'de' if lang=='ES' else 'von' if lang=='DE' else 'Razor Enhanced 的' if lang=='ZH' else 'из')} Razor Enhanced.\\n[{desc_trad}]: {desc}\\n[{ret_trad}]: {ret}\\n[{params_trad}]: {params}\\n[{ex_trad}]:\\n```python\\n{code}\\n```"

    if lang in ["ZH", "RU"]:
        assistant_content = f"`{mod}.{name}` {('是一个' if lang=='ZH' else 'это')} {tipo_trad} {('Razor Enhanced 的' if lang=='ZH' else 'в Razor Enhanced')}.\\n[{desc_trad}]: {desc}\\n[{ret_trad}]: {ret}\\n[{params_trad}]: {params}\\n[{ex_trad}]:\\n```python\\n{code}\\n```"

    return q1, q2, assistant_content

with open("dataset_razor_API.jsonl", "a", encoding="utf-8") as f:
    for d in data:
        q1, q2, ast = get_prompts(d['lang'], d['mod'], d['type'], d['name'], d['ret'], d['params'], d['desc'])
        question = random.choice([q1, q2])
        obj = {
            "messages": [
                {"role": "system", "content": system_prompt},
                {"role": "user", "content": question},
                {"role": "assistant", "content": ast}
            ]
        }
        f.write(json.dumps(obj, ensure_ascii=False) + "\\n")
