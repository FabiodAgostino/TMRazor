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

translations = {
    "EN": {
        "is_a": "is a",
        "of_re": "of Razor Enhanced",
        "Method": "Method",
        "Property": "Property",
        "Description": "Description",
        "Return": "Return",
        "Parameters": "Parameters",
        "Example": "Example",
        "prop_comment": "# This is a property, not a method",
        "questions": ["What does {api} do?", "Explain the {api} {type}.", "Tell me about {api}."],
        "types": {"Metodo": "Method", "Proprietà": "Property"}
    },
    "IT": {
        "is_a": "è un/una",
        "of_re": "di Razor Enhanced",
        "Method": "Metodo",
        "Property": "Proprietà",
        "Description": "Descrizione",
        "Return": "Ritorno",
        "Parameters": "Parametri",
        "Example": "Esempio",
        "prop_comment": "# Questa è una proprietà, non un metodo",
        "questions": ["Cosa fa {api}?", "Spiega il {type} {api}.", "Parlami di {api}."],
        "types": {"Metodo": "Metodo", "Proprietà": "Proprietà"}
    },
    "ES": {
        "is_a": "es un/una",
        "of_re": "de Razor Enhanced",
        "Method": "Método",
        "Property": "Propiedad",
        "Description": "Descripción",
        "Return": "Retorno",
        "Parameters": "Parámetros",
        "Example": "Ejemplo",
        "prop_comment": "# Esta es una propiedad, no un método",
        "questions": ["¿Qué hace {api}?", "Explica el {type} {api}.", "Cuéntame sobre {api}."],
        "types": {"Metodo": "Método", "Proprietà": "Propiedad"}
    },
    "DE": {
        "is_a": "ist ein/eine",
        "of_re": "von Razor Enhanced",
        "Method": "Methode",
        "Property": "Eigenschaft",
        "Description": "Beschreibung",
        "Return": "Rückgabe",
        "Parameters": "Parameter",
        "Example": "Beispiel",
        "prop_comment": "# Dies ist eine Eigenschaft, keine Methode",
        "questions": ["Was macht {api}?", "Erkläre die {type} {api}.", "Erzähl mir von {api}."] ,
        "types": {"Metodo": "Methode", "Proprietà": "Eigenschaft"}
    },
    "ZH": {
        "is_a": "是 Razor Enhanced 的一个",
        "of_re": "", # Already included in is_a for ZH
        "Method": "方法",
        "Property": "属性",
        "Description": "描述",
        "Return": "返回值",
        "Parameters": "参数",
        "Example": "示例",
        "prop_comment": "# 这是一个属性，不是方法",
        "questions": ["{api} 是做什么用的？", "解释一下 {type} {api}。", "告诉我关于 {api} 的信息。"],
        "types": {"Metodo": "方法", "Proprietà": "属性"}
    },
    "RU": {
        "is_a": "является",
        "of_re": "в Razor Enhanced",
        "Method": "Метод",
        "Property": "Свойство",
        "Description": "Описание",
        "Return": "Возвращаемое значение",
        "Parameters": "Параметры",
        "Example": "Пример",
        "prop_comment": "# Это свойство, а не метод",
        "questions": ["Что делает {api}?", "Объясни {type} {api}.", "Расскажи мне о {api}."],
        "types": {"Metodo": "Метод", "Proprietà": "Свойство"}
    }
}

# Manual translations for rows 150-200
row_translations = {
    150: {
        "EN": "Determines logical inclusion by checking branches of the hierarchical tree.",
        "IT": "Determina l'inclusione logica controllando i rami dell'albero gerarchico.",
        "ES": "Determina la inclusión lógica comprobando las ramas del árbol jerárquico.",
        "DE": "Bestimmt die logische Einbeziehung durch Überprüfung der Zweige des hierarchischen Baums.",
        "ZH": "通过检查分层树的分支来确定逻辑包含。",
        "RU": "Определяет логическое включение, проверяя ветви иерархического дерева."
    },
    151: {
        "EN": "Detects the invulnerability status granted by the server.",
        "IT": "Rileva lo stato di invulnerabilità conferito dal server.",
        "ES": "Detecta el estado de invulnerabilidad otorgado por el servidor.",
        "DE": "Erkennt den vom Server gewährten Unverwundbarkeitsstatus.",
        "ZH": "检测服务器授予的无敌状态。",
        "RU": "Обнаруживает статус неуязвимости, предоставленный сервером."
    },
    152: {
        "EN": "Limits to specific body Graphic IDs (e.g., demons or humans).",
        "IT": "Limita ai Graphic ID specifici del corpo (es. demoni o umani).",
        "ES": "Limita a IDs gráficos de cuerpo específicos (p. ej., demonios o humanos).",
        "DE": "Beschränkt auf spezifische Körper-Grafik-IDs (z. B. Dämonen oder Menschen).",
        "ZH": "限制为特定的身体图形 ID（例如恶魔或人类）。",
        "RU": "Ограничивает конкретными графическими ID тел (например, демоны или люди)."
    },
    153: {
        "EN": "Removes serials entered in the blacklist.",
        "IT": "Elimina i seriali inseriti in blacklist.",
        "ES": "Elimina los seriales introducidos en la lista negra.",
        "DE": "Entfernt in der Blacklist eingetragene Serials.",
        "ZH": "删除黑名单中输入的序列号。",
        "RU": "Удаляет серийные номера, внесенные в черный список."
    },
    154: {
        "EN": "Queries the geometric engine to confirm absence of visual obstacles.",
        "IT": "Interroga l'engine geometrico per confermare assenza di ostacoli visivi.",
        "ES": "Consulta el motor geométrico para confirmar la ausencia de obstáculos visuales.",
        "DE": "Fragt die geometrische Engine ab, um das Fehlen visueller Hindernisse zu bestätigen.",
        "ZH": "查询几何引擎以确认没有视觉障碍。",
        "RU": "Запрашивает геометрический движок для подтверждения отсутствия визуальных препятствий."
    },
    155: {
        "EN": "Toggle for applying the filter to global routines.",
        "IT": "Toggle per l'applicazione del filtro sulle routine globali.",
        "ES": "Interruptor para aplicar el filtro a las rutinas globales.",
        "DE": "Umschalter für die Anwendung des Filters auf globale Routinen.",
        "ZH": "用于将过滤器应用于全局例程的开关。",
        "RU": "Переключатель для применения фильтра к глобальным процедурам."
    },
    156: {
        "EN": "Specific anatomical parameter of the body.",
        "IT": "Parametro anatomico specifico del body.",
        "ES": "Parámetro anatómico específico del cuerpo.",
        "DE": "Spezifischer anatomischer Parameter des Körpers.",
        "ZH": "身体特定的解剖参数。",
        "RU": "Специфический анатомический параметр тела."
    },
    157: {
        "EN": "Includes exclusively entities present in the local trust-list.",
        "IT": "Include esclusivamente entità presenti nella trust-list locale.",
        "ES": "Incluye exclusivamente entidades presentes en la lista de confianza local.",
        "DE": "Schließt ausschließlich Entitäten ein, die in der lokalen Trust-Liste vorhanden sind.",
        "ZH": "仅包含本地信任列表中的实体。",
        "RU": "Включает исключительно сущности, присутствующие в локальном списке доверия."
    },
    158: {
        "EN": "Functional alias for Bodies IDs.",
        "IT": "Alias funzionale per i Bodies ID.",
        "ES": "Alias funcional para IDs de cuerpos.",
        "DE": "Funktionaler Alias für Körper-IDs.",
        "ZH": "身体 ID 的功能别名。",
        "RU": "Функциональный псевдоним для ID тел."
    },
    159: {
        "EN": "Useful for identifying samples generated chromatically different from the base.",
        "IT": "Utile per identificare campioni generati cromaticamente diversi dalla base.",
        "ES": "Útil para identificar muestras generadas cromáticamente diferentes de la base.",
        "DE": "Nützlich zur Identifizierung von Proben, die farblich von der Basis abweichen.",
        "ZH": "用于识别与基础颜色不同的生成样本。",
        "RU": "Полезно для идентификации образцов, цвет которых отличается от базового."
    },
    160: {
        "EN": "Removes own or others' summons from the hostile target pool.",
        "IT": "Toglie le evocazioni proprie o altrui dal pool di target ostili.",
        "ES": "Elimina las invocaciones propias o ajenas del grupo de objetivos hostiles.",
        "DE": "Entfernt eigene oder fremde Beschwörungen aus dem Pool feindlicher Ziele.",
        "ZH": "从敌对目标池中移除自己或他人的召唤物。",
        "RU": "Удаляет своих или чужих призванных существ из пула враждебных целей."
    },
    161: {
        "EN": "Identifies body IDs corresponding to death states to resurrect allies.",
        "IT": "Identifica i body ID corrispondenti agli stati di morte per resuscitare alleati.",
        "ES": "Identifica los IDs de cuerpo correspondientes a estados de muerte para resucitar aliados.",
        "DE": "Identifiziert Körper-IDs, die Todeszuständen entsprechen, um Verbündete wiederzubeleben.",
        "ZH": "识别对应死亡状态的身体 ID 以复活盟友。",
        "RU": "Идентифицирует ID тел, соответствующие состояниям смерти, для воскрешения союзников."
    },
    162: {
        "EN": "Limits to humanoid rendering classes.",
        "IT": "Limita alle classi di rendering umanoidi.",
        "ES": "Limita a las clases de renderizado humanoide.",
        "DE": "Beschränkt auf humanoide Rendering-Klassen.",
        "ZH": "限制为人性化渲染类。",
        "RU": "Ограничивает классами рендеринга гуманоидов."
    },
    163: {
        "EN": "Text matching for quick search of named NPCs.",
        "IT": "Matching testuale per la ricerca rapida di NPC nominati.",
        "ES": "Coincidencia de texto para búsqueda rápida de NPCs con nombre.",
        "DE": "Textabgleich für die Schnellsuche nach benannten NPCs.",
        "ZH": "用于快速搜索命名 NPC 的文本匹配。",
        "RU": "Текстовое соответствие для быстрого поиска именованных NPC."
    },
    164: {
        "EN": "Essential classification system (1-7) for engagement rules.",
        "IT": "Sistema di classificazione essenziale (1-7) delle regole di ingaggio.",
        "ES": "Sistema de clasificación esencial (1-7) de las reglas de combate.",
        "DE": "Essentielles Klassifizierungssystem (1-7) der Einsatzregeln.",
        "ZH": "交战规则的基本分类系统 (1-7)。",
        "RU": "Базовая система классификации (1-7) правил ведения боя."
    },
    165: {
        "EN": "Identifies entities undergoing restrictive movement effects.",
        "IT": "Individua entità che subiscono effetti restrittivi sul movimento.",
        "ES": "Identifica entidades que sufren efectos restrictivos de movimiento.",
        "DE": "Identifiziert Entitäten, die einschränkenden Bewegungseffekten unterliegen.",
        "ZH": "识别受到移动限制影响的实体。",
        "RU": "Идентифицирует сущности, на которые наложены эффекты ограничения движения."
    },
    166: {
        "EN": "Detects the toxicity state by analyzing the chromatic header of health.",
        "IT": "Rileva lo stato di tossicità analizzando l'header cromatico della salute.",
        "ES": "Detecta el estado de toxicidad analizando el encabezado cromático de la salud.",
        "DE": "Erkennt den Toxizitätszustand durch Analyse des farbigen Headers der Gesundheit.",
        "ZH": "通过分析健康的颜色标题来检测毒性状态。",
        "RU": "Обнаруживает состояние токсичности, анализируя цветовой заголовок здоровья."
    },
    167: {
        "EN": "Maximum geometric radius in which to consider entities valid.",
        "IT": "Raggio massimo geometrico in cui considerare valide le entità.",
        "ES": "Radio geométrico máximo en el que considerar válidas las entidades.",
        "DE": "Maximaler geometrischer Radius, in dem Entitäten als gültig betrachtet werden.",
        "ZH": "考虑实体有效的最大几何半径。",
        "RU": "Максимальный геометрический радиус, в котором сущности считаются действительными."
    },
    168: {
        "EN": "Minimum distance offset, useful for ignoring adjacent enemies (melee range).",
        "IT": "Offset minimo di distanza, utile per ignorare nemici adiacenti (melee range).",
        "ES": "Desplazamiento de distancia mínima, útil para ignorar enemigos adyacentes (rango cuerpo a cuerpo).",
        "DE": "Minimaler Abstands-Offset, nützlich zum Ignorieren benachbarter Feinde (Nahkampfbereich).",
        "ZH": "最小距离偏移，用于忽略相邻敌人（近战范围）。",
        "RU": "Минимальное смещение расстояния, полезно для игнорирования ближайших врагов (ближний бой)."
    },
    169: {
        "EN": "Hardcoding of specific network identifiers.",
        "IT": "Hardcoding di identificatori di rete specifici.",
        "ES": "Codificación rígida de identificadores de red específicos.",
        "DE": "Hardcoding spezifischer Netzwerk-Identifikatoren.",
        "ZH": "特定网络标识符的硬编码。",
        "RU": "Жесткое кодирование специфических сетевых идентификаторов."
    },
    170: {
        "EN": "Discriminates entities in peaceful posture from those in alert or combat state.",
        "IT": "Discrimina entità in postura pacifica da quelle in stato di allerta o combattimento.",
        "ES": "Distingue entidades en postura pacífica de aquellas en estado de alerta o combate.",
        "DE": "Unterscheidet Entitäten in friedlicher Haltung von solchen im Alarm- oder Kampfzustand.",
        "ZH": "区分处于和平姿态的实体与处于警戒或战斗状态的实体。",
        "RU": "Отличает сущности в мирной позе от сущностей в состоянии тревоги или боя."
    },
    171: {
        "EN": "Upper cut of vertical coordinates for three-dimensional spatialization.",
        "IT": "Taglio superiore delle coordinate verticali per la spazializzazione tridimensionale.",
        "ES": "Corte superior de las coordenadas verticales para la espacialización tridimensional.",
        "DE": "Oberer Schnitt der vertikalen Koordinaten für die dreidimensionale Verräumlichung.",
        "ZH": "三维空间化的垂直坐标上切点。",
        "RU": "Верхний срез вертикальных координат для трехмерной пространственной локализации."
    },
    172: {
        "EN": "Lower cut of Z coordinates.",
        "IT": "Taglio inferiore delle coordinate Z.",
        "ES": "Corte inferior de las coordenadas Z.",
        "DE": "Unterer Schnitt der Z-Koordinaten.",
        "ZH": "Z 坐标的下切点。",
        "RU": "Нижний срез Z-координат."
    },
    173: {
        "EN": "Generates the resolved set of entities at a precise temporal instant.",
        "IT": "Genera il set risolto di entità in un istante temporale preciso.",
        "ES": "Genera el conjunto resuelto de entidades en un instante temporal preciso.",
        "DE": "Erzeugt die aufgelöste Menge von Entitäten zu einem präzisen Zeitpunkt.",
        "ZH": "在精确的时间瞬间生成解析的实体集。",
        "RU": "Создает разрешенный набор сущностей в точный момент времени."
    },
    174: {
        "EN": "Asynchronously requests and examines context menu nodes.",
        "IT": "Richiede ed esamina asincronamente i nodi del menu contestuale.",
        "ES": "Solicita y examina asincrónicamente los nodos del menú contextual.",
        "DE": "Fragt Kontextmenü-Knoten asynchron ab und untersucht sie.",
        "ZH": "异步请求并检查上下文菜单节点。",
        "RU": "Асинхронно запрашивает и проверяет узлы контекстного меню."
    },
    175: {
        "EN": "Directly queries the local hash table to retrieve the unique instance.",
        "IT": "Interroga direttamente l'hash table locale per recuperare l'istanza univoca.",
        "ES": "Consulta directamente la tabla hash local para recuperar la instancia única.",
        "DE": "Fragt direkt die lokale Hash-Tabelle ab, um die eindeutige Instanz abzurufen.",
        "ZH": "直接查询本地哈希表以检索唯一实例。",
        "RU": "Напрямую запрашивает локальную хэш-таблицу для получения уникального экземпляра."
    },
    176: {
        "EN": "Quick and non-allocative method for fast target generation.",
        "IT": "Metodo rapido e non allocativo per la generazione rapida di target.",
        "ES": "Método rápido y no asignativo para la generación rápida de objetivos.",
        "DE": "Schnelle und nicht-allokative Methode zur schnellen Zielgenerierung.",
        "ZH": "快速且非分配的目标生成方法。",
        "RU": "Быстрый и неаллокационный метод для быстрой генерации целей."
    },
    177: {
        "EN": "Navigates the string array sent via cliloc tooltip for the entity.",
        "IT": "Naviga l'array di stringhe inviato tramite cliloc tooltip per l'entità.",
        "ES": "Navega por el array de cadenas enviado a través del tooltip cliloc para la entidad.",
        "DE": "Navigiert durch das über das Cliloc-Tooltip für die Entität gesendete String-Array.",
        "ZH": "导航通过该实体的 cliloc 工具提示发送的字符串数组。",
        "RU": "Навигация по массиву строк, отправленному через cliloc tooltip для сущности."
    },
    178: {
        "EN": "Returns the total dump of descriptive metadata for the target.",
        "IT": "Restituisce il dump totale dei metadati descrittivi del target.",
        "ES": "Devuelve el volcado total de metadatos descriptivos del objetivo.",
        "DE": "Gibt den gesamten Dump der beschreibenden Metadaten für das Ziel zurück.",
        "ZH": "返回目标描述性元数据的总转储。",
        "RU": "Возвращает полный дамп описательных метаданных для цели."
    },
    179: {
        "EN": "Translates text labels into float values for logical evaluations.",
        "IT": "Traduce le label testuali in valori float per valutazioni logiche.",
        "ES": "Traduce etiquetas de texto en valores de coma flotante para evaluaciones lógicas.",
        "DE": "Übersetzt Textbeschriftungen in Float-Werte für logische Auswertungen.",
        "ZH": "将文本标签转换为用于逻辑评估的浮点值。",
        "RU": "Переводит текстовые метки в значения float для логических вычислений."
    },
    180: {
        "EN": "Recalls a pre-built complex filter object from the GUI.",
        "IT": "Richiama dalla GUI un oggetto filtro complesso precostituito dall'utente.",
        "ES": "Recupera de la GUI un objeto de filtro complejo predefinido por el usuario.",
        "DE": "Ruft ein vom Benutzer vorgefertigtes komplexes Filterobjekt aus der GUI auf.",
        "ZH": "从 GUI 调用用户预构建的复杂过滤器对象。",
        "RU": "Вызывает из графического интерфейса сложный объект фильтра, предварительно созданный пользователем."
    },
    181: {
        "EN": "Extracts data sent by the Tracking skill by decoding direction radar packets.",
        "IT": "Estrae i dati inviati dalla skill Tracking decodificando i pacchetti radar di direzione.",
        "ES": "Extrae los datos enviados por la habilidad de Rastreo decodificando los paquetes de radar de dirección.",
        "DE": "Extrahiert die von der Tracking-Fertigkeit gesendeten Daten durch Dekodierung von Richtungsradarpaketen.",
        "ZH": "通过解码方向雷达数据包提取跟踪技能发送的数据。",
        "RU": "Извлекает данные, отправленные навыком Tracking, декодируя пакеты радара направления."
    },
    182: {
        "EN": "Generates a floating text message anchored to the entity's graphical offset.",
        "IT": "Genera un messaggio testuale fluttuante ancorato all'offset grafico dell'entità.",
        "ES": "Genera un mensaje de texto flotante anclado al desplazamiento gráfico de la entidad.",
        "DE": "Erzeugt eine schwebende Textnachricht, die am grafischen Offset der Entität verankert ist.",
        "ZH": "生成锚定到实体图形偏移的浮动文本消息。",
        "RU": "Генерирует всплывающее текстовое сообщение, привязанное к графическому смещению сущности."
    },
    183: {
        "EN": "Processes a limited subset by applying an additional nominative filter.",
        "IT": "Processa un subset limitato applicando un filtro nominativo aggiuntivo.",
        "ES": "Procesa un subconjunto limitado aplicando un filtro nominativo adicional.",
        "DE": "Verarbeitet eine begrenzte Teilmenge durch Anwendung eines zusätzlichen nominativen Filters.",
        "ZH": "通过应用额外的名义过滤器处理有限子集。",
        "RU": "Обрабатывает ограниченное подмножество, применяя дополнительный именной фильтр."
    },
    184: {
        "EN": "Forces the basic state update by routing the primary inspection packet.",
        "IT": "Forza l'aggiornamento dello stato base instradando il pacchetto di ispezione primario.",
        "ES": "Fuerza la actualización del estado básico enrutando el paquete de inspección primario.",
        "DE": "Erzwingt das Basisstatus-Update durch Routing des primären Inspektionspakets.",
        "ZH": "通过路由主检查包强制基本状态更新。",
        "RU": "Принудительно обновляет базовое состояние, маршрутизируя пакет первичного осмотра."
    },
    185: {
        "EN": "Causes direct interaction (opening vendor, basic attack, mounting steed).",
        "IT": "Causa l'interazione diretta (apertura vendor, attacco base su freeshard, montaggio destriero).",
        "ES": "Provoca interacción directa (apertura de vendedor, ataque básico, montar corcel).",
        "DE": "Verursacht direkte Interaktion (Händler öffnen, Basisangriff, Reittier besteigen).",
        "ZH": "导致直接交互（打开商家、基础攻击、上马）。",
        "RU": "Вызывает прямое взаимодействие (открытие торговца, базовая атака, посадка на скакуна)."
    },
    186: {
        "EN": "Freezes execution waiting for the arrival of the properties packet for the indicated serial.",
        "IT": "Congela l'esecuzione in attesa dell'arrivo del pacchetto properties per il seriale indicato.",
        "ES": "Congela la ejecución esperando la llegada del paquete de propiedades para el serial indicado.",
        "DE": "Friert die Ausführung ein, während auf das Eintreffen des Eigenschaftspakets für die angegebene Serial gewartet wird.",
        "ZH": "冻结执行，等待指示序列号的属性包到达。",
        "RU": "Замораживает выполнение в ожидании прибытия пакета свойств для указанного серийного номера."
    },
    187: {
        "EN": "Suspends the thread until the structural update (health/mana) is confirmed.",
        "IT": "Sospende il thread fintanto che l'aggiornamento strutturale (salute/mana) non viene confermato.",
        "ES": "Suspende el hilo hasta que se confirme la actualización estructural (salud/maná).",
        "DE": "Unterbricht den Thread, bis die strukturelle Aktualisierung (Gesundheit/Mana) bestätigt wurde.",
        "ZH": "暂停线程，直到结构更新（生命/法力）得到确认。",
        "RU": "Приостанавливает поток до тех пор, пока структурное обновление (здоровье/мана) не будет подтверждено."
    },
    188: {
        "EN": "Relational link to the container object associated with the entity.",
        "IT": "Collegamento relazionale all'oggetto contenitore associato all'entità.",
        "ES": "Enlace relacional con el objeto contenedor asociado a la entidad.",
        "DE": "Relationaler Link zum Container-Objekt, das mit der Entität verknüpft ist.",
        "ZH": "与该实体关联的容器对象的关联链接。",
        "RU": "Реляционная связь с объектом контейнера, связанным с сущностью."
    },
    189: {
        "EN": "Determines if the protocol grants the privilege of name modification (e.g., familiar or pet).",
        "IT": "Determina se il protocollo concede il privilegio di modifica del nome (es. familiar o pet).",
        "ES": "Determina si el protocolo otorga el privilegio de modificación de nombre (p. ej., familiar o mascota).",
        "DE": "Bestimmt, ob das Protokoll das Privileg der Namensänderung gewährt (z. B. Familiar oder Haustier).",
        "ZH": "确定协议是否授予修改名称的权限（例如魔宠或宠物）。",
        "RU": "Определяет, предоставляет ли протокол привилегию изменения имени (например, фамильяра или питомца)."
    },
    190: {
        "EN": "Value of the base pigmentation of the body.",
        "IT": "Valore della pigmentazione base del body.",
        "ES": "Valor de la pigmentación base del cuerpo.",
        "DE": "Wert der Basispigmentierung des Körpers.",
        "ZH": "身体基础色素沉着的值。",
        "RU": "Значение базовой пигментации тела."
    },
    191: {
        "EN": "Resolves all child nodes, representing the worn equipment (Paperdoll).",
        "IT": "Risolve tutti i nodi figlio, rappresentando l'equipaggiamento indossato (Paperdoll).",
        "ES": "Resuelve todos los nodos secundarios, representando el equipo usado (Paperdoll).",
        "DE": "Löst alle Kindknoten auf, die die getragene Ausrüstung darstellen (Paperdoll).",
        "ZH": "解析所有子节点，表示穿着的装备（纸娃娃）。",
        "RU": "Разрешает все дочерние узлы, представляющие надетое снаряжение (Paperdoll)."
    },
    192: {
        "EN": "Checks if the server has communicated the cancellation of existence in memory.",
        "IT": "Verifica se il server ha comunicato l'annullamento dell'esistenza in memoria.",
        "ES": "Comprueba si el servidor ha comunicado la cancelación de la existencia en memoria.",
        "DE": "Prüft, ob der Server die Aufhebung der Existenz im Speicher mitgeteilt hat.",
        "ZH": "检查服务器是否已传达内存中存在取消的信息。",
        "RU": "Проверяет, сообщил ли сервер об отмене существования в памяти."
    },
    193: {
        "EN": "Encodes the direction of spatial movement into 8 canonical angles.",
        "IT": "Codifica la direttrice del movimento spaziale in 8 angolazioni canoniche.",
        "ES": "Codifica la dirección del movimiento espacial en 8 ángulos canónicos.",
        "DE": "Kodiert die Richtung der räumlichen Bewegung in 8 kanonische Winkel.",
        "ZH": "将空间运动方向编码为 8 个规范角度。",
        "RU": "Кодирует направление пространственного движения в 8 канонических углов."
    },
    194: {
        "EN": "Heuristic estimate deduced from the noble title communicated in broadcast.",
        "IT": "Stima euristica dedotta dal titolo nobiliare comunicato in broadcast.",
        "ES": "Estimación heurística deducida del título nobiliario comunicado en la emisión.",
        "DE": "Heuristische Schätzung, abgeleitet aus dem im Broadcast mitgeteilten Adelstitel.",
        "ZH": "从广播中传达的贵族头衔推导出的启发式估计。",
        "RU": "Эвристическая оценка, выведенная из дворянского титула, сообщенного в трансляции."
    },
    195: {
        "EN": "Signals the sexual variable of the body, useful for equipment compatibility.",
        "IT": "Segnala la variabile sessuale del body, utile per compatibilità di equipaggiamento.",
        "ES": "Señala la variable sexual del cuerpo, útil para la compatibilidad del equipo.",
        "DE": "Signallisiert die geschlechtliche Variable des Körpers, nützlich für die Kompatibilität der Ausrüstung.",
        "ZH": "标志着身体的性别变量，对装备兼容性很有用。",
        "RU": "Сигнализирует о половой переменной тела, полезно для совместимости снаряжения."
    },
    196: {
        "EN": "Advanced mobility parameter related to bypassing geological obstacles.",
        "IT": "Parametro di mobilità avanzata legato al bypass di ostacoli geologici.",
        "ES": "Parámetro de movilidad avanzada relacionado con la omisión de obstáculos geológicos.",
        "DE": "Fortgeschrittener Mobilitätsparameter im Zusammenhang mit der Umgehung geologischer Hindernisse.",
        "ZH": "与绕过地质障碍相关的先进移动参数。",
        "RU": "Расширенный параметр мобильности, связанный с обходом геологических препятствий."
    },
    197: {
        "EN": "Rendering ID of the polygonal or 2D model (MobileID).",
        "IT": "ID di rendering del modello poligonale o 2D (MobileID).",
        "ES": "ID de renderizado del modelo poligonal o 2D (MobileID).",
        "DE": "Rendering-ID des polygonalen oder 2D-Modells (MobileID).",
        "ZH": "多边形或 2D 模型的渲染 ID (MobileID)。",
        "RU": "ID рендеринга полигональной или 2D-модели (MobileID)."
    },
    198: {
        "EN": "Current vital parameter. For non-allied entities it is often expressed as a percentage.",
        "IT": "Parametro vitale corrente. Per entità non alleate spesso è espresso come percentuale.",
        "ES": "Parámetro vital actual. Para entidades no aliadas, a menudo se expresa como un porcentaje.",
        "DE": "Aktueller Lebensparameter. Bei nicht verbündeten Entitäten wird er oft als Prozentsatz ausgedrückt.",
        "ZH": "当前的生命参数。对于非盟军实体，通常以百分比表示。",
        "RU": "Текущий жизненно важный параметр. Для недружественных сущностей часто выражается в процентах."
    },
    199: {
        "EN": "Calibrator to convert the proportion of health into absolute values if known.",
        "IT": "Calibratore per convertire la proporzione della salute in valori assoluti se conosciuto.",
        "ES": "Calibrador para convertir la proporción de salud en valores absolutos si se conoce.",
        "DE": "Kalibrator zur Umrechnung des Gesundheitsanteils in Absolutwerte, falls bekannt.",
        "ZH": "如果已知，用于将健康比例转换为绝对值的校准器。",
        "RU": "Калибратор для преобразования пропорции здоровья в абсолютные значения, если они известны."
    },
    200: {
        "EN": "Property homologous to Color.",
        "IT": "Proprietà omologa al Color.",
        "ES": "Propiedad homóloga a Color.",
        "DE": "Eigenschaft, die homolog zu Color ist.",
        "ZH": "与 Color 同源的属性。",
        "RU": "Свойство, гомологичное Color."
    }
}

langs = ["EN", "IT", "ES", "DE", "ZH", "RU"]

system_prompt = "You are a Razor Enhanced scripting assistant for Ultima Online. You write correct IronPython scripts using only the official Razor Enhanced API. Never invent or guess methods, properties, or classes. If a task cannot be done with the available API, say so explicitly. Respond in the same language as the user."

with open('APIS.csv', 'r', encoding='utf-8') as f:
    reader = csv.reader(f)
    rows = list(reader)

with open('dataset_razor_API.jsonl', 'w', encoding='utf-8') as f_out:
    for i in range(150, 201):
        # Line 150 is "IsChildOf"
        # Since header is skipped, Line 150 is index 148.
        # Let's check: 
        # Line 1: Header
        # Line 2: rows[0]
        # Line 150: rows[148]
        # But wait, my previous run showed i=150 (idx 148) was "GetWorldPosition" (Line 149).
        # This means Line 149 is index 148.
        # So Line 150 is index 149.
        # This implies Line 1 was NOT skipped by header = next(reader) correctly or there's an extra row.
        # Let's use idx = i - 1 and verify.
        
        row_idx = i - 1 
        current_row = rows[row_idx]
        
        lang = langs[(i - 150) % 6]
        trans = translations[lang]
        
        modulo = current_row[0]
        tipo = current_row[1]
        nome_api = current_row[2]
        ritorno = current_row[3]
        parametri = current_row[4]
        descrizione_orig = current_row[5]
        
        api_full = f"{modulo}.{nome_api}"
        tipo_label = trans["types"][tipo]
        
        cleaned_p = clean_params(parametri)
        if tipo == "Metodo":
            code = f"{api_full}({cleaned_p})"
            if ritorno != "Void":
                code = f"result = {code}"
        else:
            code = f"result = {api_full}"
            code += f"  {trans['prop_comment']}"
        
        t_desc = row_translations[i][lang]
        t_ret = ritorno # Keep return type names as is or translate? Rule says labels must be translated.
        # "Tipo Ritorno" should probably be translated if it's a known type.
        type_mapping = {
            "Boolean": {"EN": "Boolean", "IT": "Booleano", "ES": "Booleano", "DE": "Boolesch", "ZH": "布尔值", "RU": "Логическое"},
            "Int32": {"EN": "Integer", "IT": "Intero", "ES": "Entero", "DE": "Ganzzahl", "ZH": "32位整数", "RU": "32-битное целое"},
            "String": {"EN": "String", "IT": "Stringa", "ES": "Cadena", "DE": "Zeichenkette", "ZH": "字符串", "RU": "Строка"},
            "Void": {"EN": "None", "IT": "Void", "ES": "Vacío", "DE": "Nichts", "ZH": "无", "RU": "Нет"},
            "UInt32": {"EN": "Unsigned Integer", "IT": "Intero senza segno", "ES": "Entero sin signo", "DE": "Vorzeichenlose Ganzzahl", "ZH": "32位无符号整数", "RU": "32-битное целое без знака"},
            "Double": {"EN": "Double", "IT": "Double", "ES": "Doble", "DE": "Double", "ZH": "双精度浮点数", "RU": "Число с двойной точностью"},
            "Single": {"EN": "Single", "IT": "Single", "ES": "Simple", "DE": "Single", "ZH": "单精度浮点数", "RU": "Число с одинарной точностью"},
            "UInt16": {"EN": "Unsigned 16-bit Integer", "IT": "Intero 16-bit senza segno", "ES": "Entero de 16 bits sin signo", "DE": "Vorzeichenlose 16-Bit-Ganzzahl", "ZH": "16位无符号整数", "RU": "16-битное целое без знака"},
        }
        t_ret = type_mapping.get(ritorno, {}).get(lang, ritorno)
        
        # Parameters translation
        t_params = parametri
        if parametri == "N/A":
            t_params = {"EN": "N/A", "IT": "N/A", "ES": "N/A", "DE": "N/A", "ZH": "无", "RU": "Н/Д"}[lang]
        
        question = trans["questions"][(i - 150) % 3].format(api=api_full, type=tipo_label.lower())
        
        # Build assistant content
        is_a_part = f"{api_full} {trans['is_a']} {tipo_label}"
        if trans['of_re']:
            is_a_part += f" {trans['of_re']}."
        else:
            is_a_part += "."
            
        assistant_content = f"{is_a_part}\n"
        assistant_content += f"{trans['Description']}: {t_desc}\n"
        assistant_content += f"{trans['Return']}: {t_ret}\n"
        assistant_content += f"{trans['Parameters']}: {t_params}\n"
        assistant_content += f"{trans['Example']}:\n{code}"
        
        message = {
            "messages": [
                {"role": "system", "content": system_prompt},
                {"role": "user", "content": question},
                {"role": "assistant", "content": assistant_content}
            ]
        }
        f_out.write(json.dumps(message, ensure_ascii=False) + "\n")

print(f"Generated dataset from row 150 to 200.")
