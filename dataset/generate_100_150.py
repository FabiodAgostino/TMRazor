import json

system_prompt = "You are a Razor Enhanced scripting assistant for Ultima Online. You write correct IronPython scripts using only the official Razor Enhanced API. Never invent or guess methods, properties, or classes. If a task cannot be done with the available API, say so explicitly. Respond in the same language as the user."

data = [
    {"lang": "EN", "mod": "Items", "type": "Metodo", "name": "OpenAt", "ret": "Void", "params": "serial, x, y", "desc": "Forces graphical opening at absolute screen coordinates."},
    {"lang": "IT", "mod": "Items", "type": "Metodo", "name": "OpenContainerAt", "ret": "Void", "params": "bag, x, y", "desc": "Apre l'inventario in coordinate specifiche per favorire macro grafiche."},
    {"lang": "ES", "mod": "Items", "type": "Metodo", "name": "Select", "ret": "Item", "params": "items, selector", "desc": "Filtra aún más una lista preexistente a través de un selector textual."},
    {"lang": "DE", "mod": "Items", "type": "Metodo", "name": "SetColor", "ret": "Void", "params": "serial, color", "desc": "Ändert den Farbton des Objekts im Speicher des Clients."},
    {"lang": "ZH", "mod": "Items", "type": "Metodo", "name": "SingleClick", "ret": "Void", "params": "item", "desc": "发送点击事件，请求服务器传输更新后的名称。"},
    {"lang": "RU", "mod": "Items", "type": "Metodo", "name": "UseItem", "ret": "Void", "params": "itemSerial, targetSerial, wait", "desc": "Использует предмет и автоматически выполняет авто-таргет на серийный номер."},
    {"lang": "EN", "mod": "Items", "type": "Metodo", "name": "UseItemByID", "ret": "Boolean", "params": "itemid, color", "desc": "Resolves the first item matching the ID and simulates its usage."},
    {"lang": "IT", "mod": "Items", "type": "Metodo", "name": "WaitForContents", "ret": "Boolean", "params": "bag, delay", "desc": "Innesca l'apertura e blocca il thread attendendo il caricamento degli oggetti figli."},
    {"lang": "ES", "mod": "Items", "type": "Metodo", "name": "WaitForProps", "ret": "Void", "params": "itemserial, delay", "desc": "Espera a que se complete la sincronización de las propiedades cliloc."},
    {"lang": "DE", "mod": "Item", "type": "Proprietà", "name": "Amount", "ret": "Int32", "params": "N/A", "desc": "Numerische Menge, die im Stack des Objekts gestapelt ist."},
    {"lang": "ZH", "mod": "Item", "type": "Proprietà", "name": "Color", "ret": "UInt16", "params": "N/A", "desc": "精确的颜色值。"},
    {"lang": "RU", "mod": "Item", "type": "Proprietà", "name": "Container", "ret": "Int32", "params": "N/A", "desc": "Серийный номер прямого родительского контейнера экземпляра."},
    {"lang": "EN", "mod": "Item", "type": "Proprietà", "name": "ContainerOpened", "ret": "Boolean", "params": "N/A", "desc": "Initialization state of the internal inventory."},
    {"lang": "IT", "mod": "Item", "type": "Proprietà", "name": "Contains", "ret": "List[Item]", "params": "N/A", "desc": "Riferimenti agli oggetti figli istanziati gerarchicamente."},
    {"lang": "ES", "mod": "Item", "type": "Proprietà", "name": "CorpseNumberItems", "ret": "Int32", "params": "N/A", "desc": "Número total de objetos si la estructura es un cadáver (-1 si no está actualizado)."},
    {"lang": "DE", "mod": "Item", "type": "Proprietà", "name": "Deleted", "ret": "Boolean", "params": "N/A", "desc": "Entfernungsstatus des Objekts aus dem serverseitigen Speicher."},
    {"lang": "ZH", "mod": "Item", "type": "Proprietà", "name": "Direction", "ret": "String", "params": "N/A", "desc": "物体的矢量方向。"},
    {"lang": "RU", "mod": "Item", "type": "Proprietà", "name": "Durability", "ret": "Int32", "params": "N/A", "desc": "Оставшиеся очки прочности, извлеченные из свойств."},
    {"lang": "EN", "mod": "Item", "type": "Proprietà", "name": "Graphics", "ret": "UInt16", "params": "N/A", "desc": "Rendering identifier (equivalent to ItemID)."},
    {"lang": "IT", "mod": "Item", "type": "Proprietà", "name": "GridNum", "ret": "Byte", "params": "N/A", "desc": "Locazione posizionale specifica se all'interno di una griglia di inventario."},
    {"lang": "ES", "mod": "Item", "type": "Proprietà", "name": "Hue", "ret": "UInt16", "params": "N/A", "desc": "Alias para el atributo cromático."},
    {"lang": "DE", "mod": "Item", "type": "Proprietà", "name": "Image", "ret": "Bitmap", "params": "N/A", "desc": "Bitmap-Struktur, die durch Off-Screen-Rendering erzeugt wird."},
    {"lang": "ZH", "mod": "Item", "type": "Proprietà", "name": "IsBagOfSending", "ret": "Boolean", "params": "N/A", "desc": "与银行运输机制相关的物品特殊识别。"},
    {"lang": "RU", "mod": "Item", "type": "Proprietà", "name": "IsContainer", "ret": "Boolean", "params": "N/A", "desc": "Определяет, поддерживает ли тип объекта вставку дочерних элементов."},
    {"lang": "EN", "mod": "Item", "type": "Proprietà", "name": "IsCorpse", "ret": "Boolean", "params": "N/A", "desc": "Detects the organic or structural nature of an explorable corpse."},
    {"lang": "IT", "mod": "Item", "type": "Proprietà", "name": "IsDoor", "ret": "Boolean", "params": "N/A", "desc": "Determina l'appartenenza alle logiche di apertura e collisione delle porte."},
    {"lang": "ES", "mod": "Item", "type": "Proprietà", "name": "IsInBank", "ret": "Boolean", "params": "N/A", "desc": "Navega por la jerarquía para establecer si el contenedor raíz es el banco del jugador."},
    {"lang": "DE", "mod": "Item", "type": "Proprietà", "name": "IsLootable", "ret": "Boolean", "params": "N/A", "desc": "Unterscheidet zwischen plünderbaren Objekten und anatomischen Dekorationen von Leichen."},
    {"lang": "ZH", "mod": "Item", "type": "Proprietà", "name": "IsPotion", "ret": "Boolean", "params": "N/A", "desc": "对物品进行分类以实现治疗和增益的自动化例程。"},
    {"lang": "RU", "mod": "Item", "type": "Proprietà", "name": "IsResource", "ret": "Boolean", "params": "N/A", "desc": "Группирует сырье для крафта (дерево, минералы, кожа)."},
    {"lang": "EN", "mod": "Item", "type": "Proprietà", "name": "IsSearchable", "ret": "Boolean", "params": "N/A", "desc": "Indicates the possibility to inspect the content without physically opening it."},
    {"lang": "IT", "mod": "Item", "type": "Proprietà", "name": "IsTwoHanded", "ret": "Boolean", "params": "N/A", "desc": "Parametro critico per le routine di disarmo e cast di incantesimi."},
    {"lang": "ES", "mod": "Item", "type": "Proprietà", "name": "IsVirtueShield", "ret": "Boolean", "params": "N/A", "desc": "Clasifica un artefacto específico para rutinas de equipamiento."},
    {"lang": "DE", "mod": "Item", "type": "Proprietà", "name": "ItemID", "ret": "Int32", "params": "N/A", "desc": "Eindeutiger Wert zur Definition des Objekttyps."},
    {"lang": "ZH", "mod": "Item", "type": "Proprietà", "name": "Layer", "ret": "String", "params": "N/A", "desc": "装备的解剖学定义（例如头部，手臂）。"},
    {"lang": "RU", "mod": "Item", "type": "Proprietà", "name": "Light", "ret": "Byte", "params": "N/A", "desc": "Параметр локального освещения, применяемый к графическому движку."},
    {"lang": "EN", "mod": "Item", "type": "Proprietà", "name": "MaxDurability", "ret": "Int32", "params": "N/A", "desc": "Theoretical maximum value of structural points for repair management."},
    {"lang": "IT", "mod": "Item", "type": "Proprietà", "name": "Movable", "ret": "Boolean", "params": "N/A", "desc": "Riflette il flag del server che previene lo spostamento (oggetti fissi)."},
    {"lang": "ES", "mod": "Item", "type": "Proprietà", "name": "Name", "ret": "String", "params": "N/A", "desc": "Cadena de caché del nombre, dependiente de los paquetes cliloc."},
    {"lang": "DE", "mod": "Item", "type": "Proprietà", "name": "OnGround", "ret": "Boolean", "params": "N/A", "desc": "Zeigt das Fehlen eines übergeordneten Containers an und bestätigt die Position auf der Karte."},
    {"lang": "ZH", "mod": "Item", "type": "Proprietà", "name": "Position", "ret": "Point3D", "params": "N/A", "desc": "实例的几何三维结构。"},
    {"lang": "RU", "mod": "Item", "type": "Proprietà", "name": "Properties", "ret": "List[Property]", "params": "N/A", "desc": "Вектор, содержащий все строки всплывающих подсказок, преобразованные в строки и целые числа."},
    {"lang": "EN", "mod": "Item", "type": "Proprietà", "name": "PropsUpdated", "ret": "Boolean", "params": "N/A", "desc": "Ensures that the asynchronous properties request was successful."},
    {"lang": "IT", "mod": "Item", "type": "Proprietà", "name": "RootContainer", "ret": "Int32", "params": "N/A", "desc": "Esegue il traversing dell'albero gerarchico fino a trovare il proprietario finale."},
    {"lang": "ES", "mod": "Item", "type": "Proprietà", "name": "Serial", "ret": "Int32", "params": "N/A", "desc": "Clave de red global única para transacciones con el servidor."},
    {"lang": "DE", "mod": "Item", "type": "Proprietà", "name": "Updated", "ret": "Boolean", "params": "N/A", "desc": "Signalisiert den Abschluss der primären Initialisierung des Objekts."},
    {"lang": "ZH", "mod": "Item", "type": "Proprietà", "name": "Visible", "ret": "Boolean", "params": "N/A", "desc": "局部可见性状态，可通过脚本影响。"},
    {"lang": "RU", "mod": "Item", "type": "Proprietà", "name": "Weight", "ret": "Int32", "params": "N/A", "desc": "Гравитационный вклад объекта, необходимый для проверки вместимости."},
    {"lang": "EN", "mod": "Item", "type": "Metodo", "name": "DistanceTo", "ret": "Int32", "params": "mob", "desc": "Pythagorean function for measuring distance in two-dimensional tiles."},
    {"lang": "IT", "mod": "Item", "type": "Metodo", "name": "GetWorldPosition", "ret": "Point3D", "params": "N/A", "desc": "Risolve le coordinate globali estraendole dai contenitori padre se necessario."},
    {"lang": "ES", "mod": "Item", "type": "Metodo", "name": "IsChildOf", "ret": "Boolean", "params": "container, maxDepth", "desc": "Determina la inclusión lógica comprobando las ramas del árbol jerárquico."}
]

type_trans = {
    "EN": {"Metodo": "Method", "Proprietà": "Property"},
    "IT": {"Metodo": "Metodo", "Proprietà": "Proprietà"},
    "ES": {"Metodo": "Método", "Proprietà": "Propiedad"},
    "DE": {"Metodo": "Methode", "Proprietà": "Eigenschaft"},
    "ZH": {"Metodo": "方法", "Proprietà": "属性"},
    "RU": {"Metodo": "Метод", "Proprietà": "Свойство"}
}

first_sentences = {
    "EN": "{api} is a {type} in Razor Enhanced.",
    "IT": "{api} è un/una {type} di Razor Enhanced.",
    "ES": "{api} es un/una {type} de Razor Enhanced.",
    "DE": "{api} ist eine {type} in Razor Enhanced.",
    "ZH": "{api} 是 Razor Enhanced 的一个 {type}。",
    "RU": "{api} — это {type} в Razor Enhanced."
}

labels = {
    "EN": {"desc": "Description", "ret": "Returns", "params": "Parameters", "ex": "Example", "prop_comment": "This is a property, not a method"},
    "IT": {"desc": "Descrizione", "ret": "Ritorno", "params": "Parametri", "ex": "Esempio", "prop_comment": "Questa è una proprietà, non un metodo"},
    "ES": {"desc": "Descripción", "ret": "Retorno", "params": "Parámetros", "ex": "Ejemplo", "prop_comment": "Esta es una propiedad, no un método"},
    "DE": {"desc": "Beschreibung", "ret": "Rückgabe", "params": "Parameter", "ex": "Beispiel", "prop_comment": "Dies ist eine Eigenschaft, keine Methode"},
    "ZH": {"desc": "描述", "ret": "返回值", "params": "参数", "ex": "示例", "prop_comment": "这是一个属性，不是方法"},
    "RU": {"desc": "Описание", "ret": "Возвращаемое значение", "params": "Параметры", "ex": "Пример", "prop_comment": "Это свойство, а не метод"}
}

questions = {
    "EN": ["Can you explain what {api} does?", "What is the purpose of {api}?", "Describe {api}."],
    "IT": ["Cosa fa {api}?", "A cosa serve {api}?", "Spiega {api}."],
    "ES": ["¿Qué hace {api}?", "¿Para qué sirve {api}?", "Explica {api}."],
    "DE": ["Was macht {api}?", "Wozu dient {api}?", "Erkläre {api}."],
    "ZH": ["{api} 有什么作用？", "{api} 的目的是什么？", "请解释 {api}。"],
    "RU": ["Что делает {api}?", "Для чего нужен {api}?", "Объясни {api}."]
}

with open("dataset_razor_API.jsonl", "a", encoding="utf-8") as f:
    for i, d in enumerate(data):
        lang = d["lang"]
        api_full = f"{d['mod']}.{d['name']}"
        q = questions[lang][i % len(questions[lang])].format(api=api_full)
        
        t_type = type_trans[lang][d["type"]]
        l_desc = labels[lang]["desc"]
        l_ret = labels[lang]["ret"]
        l_params = labels[lang]["params"]
        l_ex = labels[lang]["ex"]
        
        first_line = first_sentences[lang].format(api=api_full, type=t_type)
        
        if d["type"] == "Proprietà":
            ex_code = f"result = {api_full}\n# {labels[lang]['prop_comment']}"
        else:
            args = d["params"] if d["params"] != "N/A" else ""
            if d["ret"] == "Void":
                ex_code = f"{api_full}({args})"
            else:
                ex_code = f"result = {api_full}({args})"
                
        ans = f"{first_line}\n{l_desc}: {d['desc']}\n{l_ret}: {d['ret']}\n{l_params}: {d['params']}\n{l_ex}:\n{ex_code}"
        
        entry = {
            "messages": [
                {"role": "system", "content": system_prompt},
                {"role": "user", "content": q},
                {"role": "assistant", "content": ans}
            ]
        }
        f.write(json.dumps(entry, ensure_ascii=False) + "\n")

print("Generazione completata con successo!")