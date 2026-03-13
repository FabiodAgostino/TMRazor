import json
import csv

def clean_params(params):
    if params == "N/A" or params == "":
        return ""
    parts = params.split(",")
    cleaned = []
    for p in parts:
        p = p.strip()
        if ":" in p:
            p = p.split(":")[0].strip()
        if "[" in p:
            p = p.split("[")[0].strip()
        cleaned.append(p)
    return ", ".join(cleaned)

def translate_type(t, lang):
    mapping = {
        "Void": {"EN": "None", "IT": "Void", "ES": "Vacío", "DE": "Nichts", "ZH": "无", "RU": "Нет"},
        "N/A": {"EN": "N/A", "IT": "N/A", "ES": "N/A", "DE": "N/A", "ZH": "无", "RU": "Н/Д"},
        "Boolean": {"EN": "Boolean", "IT": "Booleano", "ES": "Booleano", "DE": "Boolesch", "ZH": "布尔值", "RU": "Логическое"},
        "Int32": {"EN": "Integer", "IT": "Intero", "ES": "Entero", "DE": "Ganzzahl", "ZH": "32位整数", "RU": "32-битное целое"},
        "String": {"EN": "String", "IT": "Stringa", "ES": "Cadena", "DE": "Zeichenkette", "ZH": "字符串", "RU": "Строка"},
        "UInt32": {"EN": "Unsigned Integer", "IT": "Intero senza segno", "ES": "Entero sin signo", "DE": "Vorzeichenlose Ganzzahl", "ZH": "32位无符号整数", "RU": "32-битное целое без знака"},
    }
    if t in mapping:
        return mapping[t].get(lang, t)
    return t

translations = {
    "EN": {
        "is_a": "is a",
        "method": "Method",
        "property": "Property",
        "desc": "Description",
        "ret": "Return",
        "params": "Parameters",
        "ex": "Example",
        "prop_comment": "# This is a property, not a method",
        "questions": ["What does {api} do?", "Explain the {api} {type}.", "Tell me about {api}."]
    },
    "IT": {
        "is_a": "è un/una",
        "method": "Metodo",
        "property": "Proprietà",
        "desc": "Descrizione",
        "ret": "Ritorno",
        "params": "Parametri",
        "ex": "Esempio",
        "prop_comment": "# Questa è una proprietà, non un metodo",
        "questions": ["Cosa fa {api}?", "Spiega il {type} {api}.", "Parlami di {api}."]
    },
    "ES": {
        "is_a": "es un/una",
        "method": "Método",
        "property": "Propiedad",
        "desc": "Descripción",
        "ret": "Retorno",
        "params": "Parámetros",
        "ex": "Ejemplo",
        "prop_comment": "# Esta es una propiedad, no un método",
        "questions": ["¿Qué hace {api}?", "Explica el {type} {api}.", "Cuéntame sobre {api}."]
    },
    "DE": {
        "is_a": "ist ein/eine",
        "method": "Methode",
        "property": "Eigenschaft",
        "desc": "Beschreibung",
        "ret": "Rückgabe",
        "params": "Parameter",
        "ex": "Beispiel",
        "prop_comment": "# Dies ist eine Eigenschaft, keine Methode",
        "questions": ["Was macht {api}?", "Erkläre die {type} {api}.", "Erzähl mir von {api}."]
    },
    "ZH": {
        "is_a": "是 Razor Enhanced 的一个",
        "method": "方法",
        "property": "属性",
        "desc": "描述",
        "ret": "返回值",
        "params": "参数",
        "ex": "示例",
        "prop_comment": "# 这是一个属性，不是方法",
        "questions": ["{api} 是做什么用的？", "解释一下 {type} {api}。", "告诉我关于 {api} 的信息。"]
    },
    "RU": {
        "is_a": "является",
        "method": "Метод",
        "property": "Свойство",
        "desc": "Описание",
        "ret": "Возвращаемое значение",
        "params": "Параметры",
        "ex": "Пример",
        "prop_comment": "# Это свойство, а не метод",
        "questions": ["Что делает {api}?", "Объясни {type} {api}.", "Расскажи мне о {api}."]
    }
}

langs = ["EN", "IT", "ES", "DE", "ZH", "RU"]

row_data_translations = {
    "AutoLoot,Metodo,ChangeList,Void,listName: Nome di una lista esistente.,Cambia la lista attiva dell'AutoLoot.": {
        "EN": {"desc": "Changes the active AutoLoot list.", "params": "listName: Name of an existing list."},
    },
    "AutoLoot,Metodo,GetList,List[AutoLoot.AutoLootItem],\"lootListName, wantMinusOnes\",Restituisce una lista di oggetti associati a una specifica lista di AutoLoot.": {
        "IT": {"desc": "Restituisce una lista di oggetti associati a una specifica lista di AutoLoot.", "params": "lootListName, wantMinusOnes"},
    },
    "AutoLoot,Metodo,GetLootBag,UInt32,N/A,Ottiene il seriale del contenitore di destinazione corrente.": {
        "ES": {"desc": "Obtiene el serial del contenedor de destino actual."},
    },
    "AutoLoot,Metodo,ResetIgnore,Void,N/A,Resetta la lista degli oggetti ignorati dall'agente.": {
        "DE": {"desc": "Setzt die Liste der vom Agenten ignorierten Gegenstände zurück."},
    },
    "AutoLoot,Metodo,RunOnce,Void,\"lootListName, millisec [Int32], filter [Items.Filter]\",Avvia l'AutoLoot con parametri personalizzati e filtro ricerca.2": {
        "ZH": {"desc": "使用自定义参数和搜索过滤器启动一次 AutoLoot。", "params": "lootListName, millisec, filter"},
    },
    "AutoLoot,Metodo,SetNoOpenCorpse,Boolean,noOpen: Attiva/disattiva la funzione.,Evita l'apertura grafica del cadavere. Il cambiamento è temporaneo.": {
        "RU": {"desc": "Предотвращает графическое открытие трупа. Изменение временное.", "params": "noOpen: Включает/выключает функцию."},
    },
    "AutoLoot,Metodo,Start,Void,N/A,Avvia l'agente sulla lista attualmente attiva.": {
        "EN": {"desc": "Starts the agent on the currently active list."},
    },
    "AutoLoot,Metodo,Status,Boolean,N/A,Verifica lo stato (True se in esecuzione).": {
        "IT": {"desc": "Verifica lo stato (True se in esecuzione)."},
    },
    "AutoLoot,Metodo,Stop,Void,N/A,Ferma l'agente.": {
        "ES": {"desc": "Detiene el agente."},
    },
    "AutoLoot.AutoLootItem,Proprietà,Color,Int32,N/A,Colore dell'oggetto target.": {
        "DE": {"desc": "Farbe des Zielobjekts."},
    },
    "AutoLoot.AutoLootItem,Proprietà,Graphics,Int32,N/A,ID grafico dell'oggetto da recuperare.": {
        "ZH": {"desc": "要回收物品的图形 ID。"},
    },
    "AutoLoot.AutoLootItem,Proprietà,List,String,N/A,Nome della lista di appartenenza.": {
        "RU": {"desc": "Имя списка, к которому принадлежит предмет."},
    },
    "AutoLoot.AutoLootItem,Proprietà,LootBagOverride,Int32,N/A,Contenitore di destinazione specifico per l'oggetto.": {
        "EN": {"desc": "Specific destination container for the object."},
    },
    "AutoLoot.AutoLootItem,Proprietà,Name,String,N/A,Nome identificativo dell'oggetto.": {
        "IT": {"desc": "Nome identificativo dell'oggetto."},
    },
    "AutoLoot.AutoLootItem,Proprietà,Properties,List[Property],N/A,Lista delle proprietà analizzate.": {
        "ES": {"desc": "Lista de propiedades analizadas."},
    },
    "AutoLoot.AutoLootItem,Proprietà,Selected,Boolean,N/A,Stato di selezione corrente.": {
        "DE": {"desc": "Aktueller Auswahlstatus."},
    },
    "BandageHeal,Metodo,Start,Void,N/A,Avvia l'agente di cura con bende.": {
        "ZH": {"desc": "启动绷带治疗代理。"},
    },
    "BandageHeal,Metodo,Status,Boolean,N/A,Verifica se l'agente è attivo.": {
        "RU": {"desc": "Проверяет, активен ли агент."},
    },
    "BandageHeal,Metodo,Stop,Void,N/A,Ferma l'agente di cura.": {
        "EN": {"desc": "Stops the healing agent."},
    },
    "BuyAgent,Metodo,ChangeList,Void,listName: Nome della lista.,Cambia la lista attiva di acquisto.2": {
        "IT": {"desc": "Cambia la lista attiva di acquisto.", "params": "listName: Nome della lista."},
    },
    "BuyAgent,Metodo,Disable,Void,N/A,Disabilita l'agente di acquisto.": {
        "ES": {"desc": "Deshabilita el agente de compra."},
    },
    "BuyAgent,Metodo,Enable,Void,N/A,Abilita l'agente di acquisto sulla lista corrente.": {
        "DE": {"desc": "Aktiviert den Kauf-Agenten für die aktuelle Liste."},
    },
    "BuyAgent,Metodo,Status,Boolean,N/A,Verifica lo stato operativo dell'agente.": {
        "ZH": {"desc": "验证代理的运行状态。"},
    },
    "DPSMeter,Metodo,GetDamage,Int32,serial [Int32]: Seriale del Mobile.,Ottiene il totale del danno inflitto a un Mobile specifico.2": {
        "RU": {"desc": "Получает общий урон, нанесенный конкретному мобильному объекту.", "params": "serial: Серийный номер мобильного объекта."},
    },
    "DPSMeter,Metodo,Pause,Void,N/A,Sospende la registrazione dei dati di danno.": {
        "EN": {"desc": "Pauses the recording of damage data."},
    },
    "DPSMeter,Metodo,Start,Void,N/A,Avvia il motore di calcolo DPS.": {
        "IT": {"desc": "Avvia il motore di calcolo DPS."},
    },
    "DPSMeter,Metodo,Status,Boolean,N/A,Verifica lo stato del modulo DPS.": {
        "ES": {"desc": "Verifica el estado del módulo DPS."},
    },
    "DPSMeter,Metodo,Stop,Void,N/A,Ferma la misurazione.": {
        "DE": {"desc": "Stoppt die Messung."},
    },
    "Dress,Metodo,ChangeList,Void,dresslist: Nome della lista di vestiario.,Cambia la lista di equipaggiamento attiva.": {
        "ZH": {"desc": "更改当前装备列表。", "params": "dresslist: 装备列表名称。"},
    },
    "Dress,Metodo,DressFStart,Void,N/A,Avvia il processo di vestizione.": {
        "RU": {"desc": "Запускает процесс одевания."},
    },
    "Dress,Metodo,DressFStop,Void,N/A,Ferma il processo di vestizione.": {
        "EN": {"desc": "Stops the dressing process."},
    },
    "Dress,Metodo,DressStatus,Boolean,N/A,Verifica lo stato dell'agente Dress.": {
        "IT": {"desc": "Verifica lo stato dell'agente Dress."},
    },
    "Dress,Metodo,UnDressFStart,Void,N/A,Avvia il processo di rimozione dell'equipaggiamento.": {
        "ES": {"desc": "Inicia el proceso de desvestirse."},
    },
    "Dress,Metodo,UnDressFStop,Void,N/A,Ferma il processo di rimozione.": {
        "DE": {"desc": "Stoppt den Ausziehvorgang."},
    },
    "Dress,Metodo,UnDressStatus,Boolean,N/A,Verifica lo stato dell'agente UnDress.": {
        "ZH": {"desc": "验证卸载装备代理的状态。"},
    },
    "Restock,Metodo,ChangeList,Void,listName: Nome della lista.,Cambia la lista di rifornimento attiva.": {
        "RU": {"desc": "Изменяет активный список пополнения запасов.", "params": "listName: Имя списка."},
    },
    "Restock,Metodo,FStart,Void,N/A,Avvia l'agente Restock.": {
        "EN": {"desc": "Starts the Restock agent."},
    },
    "Restock,Metodo,FStop,Void,N/A,Ferma l'agente Restock.": {
        "IT": {"desc": "Ferma l'agente Restock."},
    },
    "Restock,Metodo,RunOnce,Void,\"restockerName, sourceBag [Int32], destBag [Int32], dragDelay [Int32]\",Esegue un ciclo di rifornimento singolo con parametri specificati.": {
        "ES": {"desc": "Ejecuta un ciclo de reabastecimiento único con parámetros especificados.", "params": "restockerName, sourceBag, destBag, dragDelay"},
    },
    "Restock,Metodo,Status,Boolean,N/A,Verifica lo stato dell'agente Restock.": {
        "DE": {"desc": "Prüft den Status des Restock-Agenten."},
    },
    "Scavenger,Metodo,ChangeList,Void,listName: Nome della lista.,Cambia la lista dello Scavenger.": {
        "ZH": {"desc": "更改捡漏列表。", "params": "listName: 列表名称。"},
    },
    "Scavenger,Metodo,GetScavengerBag,UInt32,N/A,Ottiene il seriale del contenitore di destinazione.2": {
        "RU": {"desc": "Получает серийный номер контейнера назначения."},
    },
    "Scavenger,Metodo,ResetIgnore,Void,N/A,Resetta la lista degli oggetti ignorati in memoria.": {
        "EN": {"desc": "Resets the list of ignored items in memory."},
    },
    "Scavenger,Metodo,RunOnce,Void,\"scavengerList [List], millisec [Int32], filter [Items.Filter]\",Esegue lo Scavenger una singola volta con un filtro custom.": {
        "IT": {"desc": "Esegue lo Scavenger una singola volta con un filtro custom.", "params": "scavengerList, millisec, filter"},
    },
    "Scavenger,Metodo,Start,Void,N/A,Avvia l'agente in background.": {
        "ES": {"desc": "Inicia el agente en segundo plano."},
    },
    "Scavenger,Metodo,Status,Boolean,N/A,Verifica lo stato dell'agente.": {
        "DE": {"desc": "Prüft den Status des Agenten."},
    },
    "Scavenger,Metodo,Stop,Void,N/A,Ferma l'agente.": {
        "ZH": {"desc": "停止代理。"},
    },
    "SellAgent,Metodo,ChangeList,Void,listName: Nome della lista.,Cambia la lista di vendita attiva.": {
        "RU": {"desc": "Изменяет активный список продаж.", "params": "listName: Имя списка."},
    },
    "SellAgent,Metodo,Disable,Void,N/A,Disabilita la vendita automatica.": {
        "EN": {"desc": "Disables automatic selling."},
    },
    "SellAgent,Metodo,Enable,Void,N/A,Abilita la vendita automatica ai vendor NPC.": {
        "IT": {"desc": "Abilita la vendita automatica ai vendor NPC."},
    }
}

system_prompt = "You are a Razor Enhanced scripting assistant for Ultima Online. You write correct IronPython scripts using only the official Razor Enhanced API. Never invent or guess methods, properties, or classes. If a task cannot be done with the available API, say so explicitly. Respond in the same language as the user."

with open('APIS.csv', 'r', encoding='utf-8') as f:
    reader = csv.reader(f)
    header = next(reader)
    rows = list(reader)

with open('dataset_razor_API.jsonl', 'w', encoding='utf-8') as f_out:
    for i in range(50):
        row = rows[i]
        lang = langs[i % 6]
        trans = translations[lang]
        
        modulo = row[0]
        tipo = row[1]
        nome_api = row[2]
        ritorno = row[3]
        parametri = row[4]
        descrizione = row[5]
        
        api_full = f"{modulo}.{nome_api}"
        tipo_label = trans["method"] if tipo == "Metodo" else trans["property"]
        
        # Determine cleaned parameters and code
        cleaned_p = clean_params(parametri)
        if tipo == "Metodo":
            code = f"{api_full}({cleaned_p})"
            if ritorno != "Void":
                code = f"result = {code}"
        else:
            code = f"result = {api_full}"
            code += f"  {trans['prop_comment']}"
        
        # Get translated content
        row_key = ",".join(row)
        row_trans = row_data_translations.get(row_key, {}).get(lang, {})
        t_desc = row_trans.get("desc", descrizione)
        t_ret = translate_type(ritorno, lang)
        t_params = row_trans.get("params", translate_type(parametri, lang))
        
        question = trans["questions"][i % 3].format(api=api_full, type=tipo_label.lower())
        
        assistant_content = f"{api_full} {trans['is_a']} {tipo_label} di Razor Enhanced.\n"
        assistant_content += f"{trans['desc']}: {t_desc}\n"
        assistant_content += f"{trans['ret']}: {t_ret}\n"
        assistant_content += f"{trans['params']}: {t_params}\n"
        assistant_content += f"{trans['ex']}:\n{code}"
        
        message = {
            "messages": [
                {"role": "system", "content": system_prompt},
                {"role": "user", "content": question},
                {"role": "assistant", "content": assistant_content}
            ]
        }
        f_out.write(json.dumps(message, ensure_ascii=False) + "\n")

print("Done generating 50 entries.")
