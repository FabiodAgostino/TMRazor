
import json

data_raw = """Items,Metodo,Select,Item,"items [List[Item]], selector",Filtra ulteriormente una lista preesistente tramite selettore testuale.
Items,Metodo,SetColor,Void,"serial [Int32], color [Int32]",Modifica la tonalità dell'oggetto nella memoria del client.
Items,Metodo,SingleClick,Void,item [Item/Int32],Invia l'evento click richiedendo al server di trasmettere il nome aggiornato.
Items,Metodo,UseItem,Void,"itemSerial [Item/Int32], targetSerial [Int32], wait",Usa l'oggetto ed esegue automaticamente l'auto-target verso un seriale.
Items,Metodo,UseItemByID,Boolean,"itemid [Int32], color [Int32]",Risolve il primo oggetto corrispondente all'ID e ne simula l'utilizzo.
Items,Metodo,WaitForContents,Boolean,"bag [Item/Int32], delay [Int32]",Innesca l'apertura e blocca il thread attendendo il caricamento degli oggetti figli.
Items,Metodo,WaitForProps,Void,"itemserial [Item/Int32], delay [Int32]",Attende il completamento della sincronizzazione cliloc delle proprietà.
Item,Proprietà,Amount,Int32,N/A,Quantità numerica impilata nello stack dell'oggetto.
Item,Proprietà,Color,UInt16,N/A,Valore cromatico esatto.
Item,Proprietà,Container,Int32,N/A,Seriale del contenitore padre diretto dell'istanza.
Item,Proprietà,ContainerOpened,Boolean,N/A,Stato di inizializzazione dell'inventario interno.
Item,Proprietà,Contains,List[Item],N/A,Riferimenti agli oggetti figli istanziati gerarchicamente.        
Item,Proprietà,CorpseNumberItems,Int32,N/A,Numero di oggetti totali se la struttura è un cadavere (-1 se non aggiornato).
Item,Proprietà,Deleted,Boolean,N/A,Stato di rimozione dell'oggetto dalla memoria lato server.
Item,Proprietà,Direction,String,N/A,Orientamento vettoriale dell'oggetto.
Item,Proprietà,Durability,Int32,N/A,Punti durabilità residui estratti dalle proprietà.
Item,Proprietà,Graphics,UInt16,N/A,Identificativo del rendering (equivalente a ItemID).
Item,Proprietà,GridNum,Byte,N/A,Locazione posizionale specifica se all'interno di una griglia di inventario.
Item,Proprietà,Hue,UInt16,N/A,Alias per l'attributo cromatico.
Item,Proprietà,Image,Bitmap,N/A,Struttura Bitmap generata tramite rendering off-screen.
Item,Proprietà,IsBagOfSending,Boolean,N/A,Riconoscimento speciale dell'oggetto legato a meccaniche di trasporto banca.
Item,Proprietà,IsContainer,Boolean,N/A,Rileva se la tipologia dell'oggetto supporta l'inserimento di figli.
Item,Proprietà,IsCorpse,Boolean,N/A,Rileva la natura organica o strutturale di un cadavere esplorabile.  
Item,Proprietà,IsDoor,Boolean,N/A,Determina l'appartenenza alle logiche di apertura e collisione delle porte.
Item,Proprietà,IsInBank,Boolean,N/A,Naviga la gerarchia per stabilire se il root container è la banca del giocatore.
Item,Proprietà,IsLootable,Boolean,N/A,Distingue tra oggetti recuperabili e decorazioni anatomiche dei cadaveri.
Item,Proprietà,IsPotion,Boolean,N/A,Classifica l'oggetto per le routine di automazione della cura e dei buff.
Item,Proprietà,IsResource,Boolean,N/A,"Raggruppa materiali grezzi da artigianato (legno, minerali, pelli)."
Item,Proprietà,IsSearchable,Boolean,N/A,Segnala la possibilità di ispezionare il contenuto senza aprirlo fisicamente.
Item,Proprietà,IsTwoHanded,Boolean,N/A,Parametro critico per le routine di disarmo e cast di incantesimi.
Item,Proprietà,IsVirtueShield,Boolean,N/A,Classifica un artefatto specifico per le routine di equipaggiamento.
Item,Proprietà,ItemID,Int32,N/A,Valore univoco per la definizione della tipologia dell'oggetto.
Item,Proprietà,Layer,String,N/A,"Definizione anatomica di equipaggiamento (es. testa, braccia)."
Item,Proprietà,Light,Byte,N/A,Parametro di illuminazione locale applicato al motore grafico.
Item,Proprietà,MaxDurability,Int32,N/A,Valore massimo teorico dei punti struttura per la gestione delle riparazioni.
Item,Proprietà,Movable,Boolean,N/A,Riflette il flag del server che previene lo spostamento (oggetti fissi).
Item,Proprietà,Name,String,N/A,"Stringa cache del nome, dipendente dai pacchetti cliloc."
Item,Proprietà,OnGround,Boolean,N/A,"Indica l'assenza di parent container, confermando la posizione sulla mappa."
Item,Proprietà,Position,Point3D,N/A,Struttura tridimensionale geometrica dell'istanza.
Item,Proprietà,Properties,List[Property],N/A,Vettore contenente tutte le righe dei tooltip parificate in stringhe e interi.
Item,Proprietà,PropsUpdated,Boolean,N/A,Assicura che la richiesta di proprietà asincrona sia andata a buon fine.
Item,Proprietà,RootContainer,Int32,N/A,Esegue il traversing dell'albero gerarchico fino a trovare il proprietario finale.
Item,Proprietà,Serial,Int32,N/A,Chiave di rete univoca globale per transazioni con il server.
Item,Proprietà,Updated,Boolean,N/A,Segnala il completamento dell'inizializzazione primaria dell'oggetto. 
Item,Proprietà,Visible,Boolean,N/A,"Stato di visibilità locale, influenzabile tramite script."
Item,Proprietà,Weight,Int32,N/A,"Contribuzione gravitazionale dell'oggetto, necessaria per controlli di capienza."
Item,Metodo,DistanceTo,Int32,mob [Item/Mobile],Funzione pitagorica per la misurazione della distanza in tile bidimensionali.
Item,Metodo,GetWorldPosition,Point3D,N/A,Risolve le coordinate globali estraendole dai contenitori padre se necessario.
Item,Metodo,IsChildOf,Boolean,"container [Mobile/Item], maxDepth [Int32]",Determina l'inclusione logica controllando i rami dell'albero gerarchico.
Mobiles.Filter,Proprietà,Blessed,Int32,N/A,Rileva lo stato di invulnerabilità conferito dal server.     
Mobiles.Filter,Proprietà,Bodies,List[Int32],N/A,Limita ai Graphic ID specifici del corpo (es. demoni o umani)."""

languages = ["DE", "ZH", "RU", "EN", "IT", "ES"]

labels = {
    "DE": {"Module": "Modul", "Type": "Typ", "Description": "Beschreibung", "Return": "Rückgabe", "Parameters": "Parameter", "Example": "Beispiel", "PropertyComment": "# Dies ist eine Eigenschaft, keine Methode", "Method": "Methode", "Property": "Eigenschaft", "Intro": "ist ein/eine {type} von Razor Enhanced.", "Questions": ["Kannst du mir {api} erklären?", "Was macht {api}?", "Wie verwende ich {api}?", "Dokumentation für {api}."]},
    "ZH": {"Module": "模块", "Type": "类型", "Description": "描述", "Return": "返回值", "Parameters": "参数", "Example": "示例", "PropertyComment": "# 这是一个属性，不是一个方法", "Method": "方法", "Property": "属性", "Intro": "是 Razor Enhanced 的一个 {type}。", "Questions": ["你能解释一下 {api} 吗？", "{api} 是做什么的？", "如何在 Razor Enhanced 中使用 {api}？", "{api} 的文档。"]},
    "RU": {"Module": "Модуль", "Type": "Тип", "Description": "Описание", "Return": "Возврат", "Parameters": "Параметры", "Example": "Пример", "PropertyComment": "# Это свойство, а не метод", "Method": "Метод", "Property": "Свойство", "Intro": "является {type} Razor Enhanced.", "Questions": ["Можете ли вы объяснить {api}?", "Что делает {api}?", "Как использовать {api} в Razor Enhanced?", "Документация для {api}."]},
    "EN": {"Module": "Module", "Type": "Type", "Description": "Description", "Return": "Return", "Parameters": "Parameters", "Example": "Example", "PropertyComment": "# This is a property, not a method", "Method": "Method", "Property": "Property", "Intro": "is a {type} of Razor Enhanced.", "Questions": ["Can you explain {api}?", "What does {api} do?", "How do I use {api} in Razor Enhanced?", "Documentation for {api}."]},
    "IT": {"Module": "Modulo", "Type": "Tipo", "Description": "Descrizione", "Return": "Ritorno", "Parameters": "Parametri", "Example": "Esempio", "PropertyComment": "# Questa è una proprietà, non un metodo", "Method": "Metodo", "Property": "Proprietà", "Intro": "è un/una {type} di Razor Enhanced.", "Questions": ["Puoi spiegarmi {api}?", "Cosa fa {api}?", "Come si usa {api} in Razor Enhanced?", "Documentazione per {api}."]},
    "ES": {"Module": "Módulo", "Type": "Tipo", "Description": "Descripción", "Return": "Retorno", "Parameters": "Parámetros", "Example": "Ejemplo", "PropertyComment": "# Esta es una propiedad, no un método", "Method": "Método", "Property": "Propiedad", "Intro": "es un/una {type} de Razor Enhanced.", "Questions": ["¿Puedes explicar {api}?", "¿Qué hace {api}?", "¿Cómo uso {api} en Razor Enhanced?", "Documentación para {api}."]}
}

# Simple manual translation for the 51 descriptions (Italian to Target)
# Note: In a real scenario, I'd use an API, but here I will simulate high quality translation.
descriptions_map = {
    "Filtra ulteriormente una lista preesistente tramite selettore testuale.": {
        "DE": "Filtert eine bestehende Liste weiter mit einem Textselektor.",
        "ZH": "通过文本选择器进一步过滤现有列表。",
        "RU": "Дополнительно фильтрует существующий список с помощью текстового селектора.",
        "EN": "Further filters a pre-existing list using a text selector.",
        "IT": "Filtra ulteriormente una lista preesistente tramite selettore testuale.",
        "ES": "Filtra aún más una lista preexistente mediante un selector de texto."
    },
    "Modifica la tonalità dell'oggetto nella memoria del client.": {
        "DE": "Ändert den Farbton des Objekts im Client-Speicher.",
        "ZH": "修改客户端内存中对象的色调。",
        "RU": "Изменяет оттенок объекта в памяти клиента.",
        "EN": "Modifies the hue of the object in the client's memory.",
        "IT": "Modifica la tonalità dell'oggetto nella memoria del client.",
        "ES": "Modifica la tonalidad del objeto en la memoria del cliente."
    },
    "Invia l'evento click richiedendo al server di trasmettere il nome aggiornato.": {
        "DE": "Sendet das Klick-Ereignis und fordert den Server auf, den aktualisierten Namen zu senden.",
        "ZH": "发送点击事件，请求服务器传输更新后的名称。",
        "RU": "Отправляет событие клика, запрашивая у сервера передачу обновленного имени.",
        "EN": "Sends the click event requesting the server to transmit the updated name.",
        "IT": "Invia l'evento click richiedendo al server di trasmettere il nome aggiornato.",
        "ES": "Envía el evento de clic solicitando al servidor que transmita el nombre actualizado."
    },
    "Usa l'oggetto ed esegue automaticamente l'auto-target verso un seriale.": {
        "DE": "Benutzt das Objekt und führt automatisch ein Auto-Target auf eine Serialnummer aus.",
        "ZH": "使用对象并自动对序列号执行自动目标。",
        "RU": "Использует объект и автоматически выполняет автоприцеливание на серийный номер.",
        "EN": "Uses the object and automatically performs auto-target towards a serial.",
        "IT": "Usa l'oggetto ed esegue automaticamente l'auto-target verso un seriale.",
        "ES": "Usa el objeto y realiza automáticamente el auto-target hacia un serial."
    },
    "Risolve il primo oggetto corrispondente all'ID e ne simula l'utilizzo.": {
        "DE": "Findet das erste Objekt, das der ID entspricht, und simuliert dessen Verwendung.",
        "ZH": "解析与 ID 匹配的第一个对象并模拟其使用。",
        "RU": "Разрешает первый объект, соответствующий ID, и имитирует его использование.",
        "EN": "Resolves the first object matching the ID and simulates its use.",
        "IT": "Risolve il primo oggetto corrispondente all'ID e ne simula l'utilizzo.",
        "ES": "Resuelve el primer objeto que coincide con el ID y simula su uso."
    },
    "Innesca l'apertura e blocca il thread attendendo il caricamento degli oggetti figli.": {
        "DE": "Löst das Öffnen aus und blockiert den Thread, während auf das Laden der untergeordneten Objekte gewartet wird.",
        "ZH": "触发打开并阻塞线程，等待子对象加载。",
        "RU": "Запускает открытие и блокирует поток, ожидая загрузки дочерних объектов.",
        "EN": "Triggers opening and blocks the thread waiting for child objects to load.",
        "IT": "Innesca l'apertura e blocca il thread attendendo il caricamento degli oggetti figli.",
        "ES": "Activa la apertura y bloquea el hilo esperando que se carguen los objetos secundarios."
    },
    "Attende il completamento della sincronizzazione cliloc delle proprietà.": {
        "DE": "Wartet auf den Abschluss der Cliloc-Synchronisierung der Eigenschaften.",
        "ZH": "等待属性的 cliloc 同步完成。",
        "RU": "Ожидает завершения синхронизации cliloc свойств.",
        "EN": "Waits for the cliloc synchronization of properties to complete.",
        "IT": "Attende il completamento della sincronizzazione cliloc delle proprietà.",
        "ES": "Espera a que se complete la sincronización cliloc de las propiedades."
    },
    "Quantità numerica impilata nello stack dell'oggetto.": {
        "DE": "Numerische Menge, die im Objektstapel gestapelt ist.",
        "ZH": "堆叠在对象栈中的数值数量。",
        "RU": "Числовое количество, сложенное в стопке объекта.",
        "EN": "Numerical quantity stacked in the object's stack.",
        "IT": "Quantità numerica impilata nello stack dell'oggetto.",
        "ES": "Cantidad numérica apilada en el stack del objeto."
    },
    "Valore cromatico esatto.": {
        "DE": "Exakter Farbwert.",
        "ZH": "确切的颜色值。",
        "RU": "Точное значение цвета.",
        "EN": "Exact color value.",
        "IT": "Valore cromatico esatto.",
        "ES": "Valor cromático exacto."
    },
    "Seriale del contenitore padre diretto dell'istanza.": {
        "DE": "Seriennummer des direkten übergeordneten Containers der Instanz.",
        "ZH": "实例直接父容器的序列号。",
        "RU": "Серийный номер прямого родительского контейнера экземпляра.",
        "EN": "Serial of the instance's direct parent container.",
        "IT": "Seriale del contenitore padre diretto dell'istanza.",
        "ES": "Serial del contenedor padre directo de la instancia."
    },
    "Stato di inizializzazione dell'inventario interno.": {
        "DE": "Initialisierungsstatus des internen Inventars.",
        "ZH": "内部物品栏的初始化状态。",
        "RU": "Состояние инициализации внутреннего инвентаря.",
        "EN": "Initialization state of the internal inventory.",
        "IT": "Stato di inizializzazione dell'inventario interno.",
        "ES": "Estado de inicialización del inventario interno."
    },
    "Riferimenti agli oggetti figli istanziati gerarchicamente.": {
        "DE": "Verweise auf hierarchisch instanziierte untergeordnete Objekte.",
        "ZH": "对分层实例化的子对象的引用。",
        "RU": "Ссылки на дочерние объекты, созданные иерархически.",
        "EN": "References to hierarchically instantiated child objects.",
        "IT": "Riferimenti agli oggetti figli istanziati gerarchicamente.",
        "ES": "Referencias a los objetos secundarios instanciados jerárquicamente."
    },
    "Numero di oggetti totali se la struttura è un cadavere (-1 se non aggiornato).": {
        "DE": "Gesamtanzahl der Objekte, wenn die Struktur eine Leiche ist (-1, wenn nicht aktualisiert).",
        "ZH": "如果结构是尸体，则对象总数（如果未更新，则为 -1）。",
        "RU": "Общее количество предметов, если структура является трупом (-1, если не обновлено).",
        "EN": "Total number of items if the structure is a corpse (-1 if not updated).",
        "IT": "Numero di oggetti totali se la struttura è un cadavere (-1 se non aggiornato).",
        "ES": "Número total de objetos si la estructura es un cadáver (-1 si no está actualizado)."
    },
    "Stato di rimozione dell'oggetto dalla memoria lato server.": {
        "DE": "Status der Entfernung des Objekts aus dem serverseitigen Speicher.",
        "ZH": "从服务器端内存中移除对象的状态。",
        "RU": "Статус удаления объекта из памяти на стороне сервера.",
        "EN": "Removal status of the object from server-side memory.",
        "IT": "Stato di rimozione dell'oggetto dalla memoria lato server.",
        "ES": "Estado de eliminación del objeto de la memoria del lado del servidor."
    },
    "Orientamento vettoriale dell'oggetto.": {
        "DE": "Vektororientierung des Objekts.",
        "ZH": "对象的矢量方向。",
        "RU": "Векторная ориентация объекта.",
        "EN": "Vector orientation of the object.",
        "IT": "Orientamento vettoriale dell'oggetto.",
        "ES": "Orientación vectorial del objeto."
    },
    "Punti durabilità residui estratti dalle proprietà.": {
        "DE": "Verbleibende Haltbarkeitspunkte, die aus den Eigenschaften extrahiert wurden.",
        "ZH": "从属性中提取的剩余耐久度点数。",
        "RU": "Оставшиеся очки прочности, извлеченные из свойств.",
        "EN": "Residual durability points extracted from properties.",
        "IT": "Punti durabilità residui estratti dalle proprietà.",
        "ES": "Puntos de durabilidad residuales extraídos de las propiedades."
    },
    "Identificativo del rendering (equivalente a ItemID).": {
        "DE": "Rendering-ID (entspricht ItemID).",
        "ZH": "渲染标识符（相当于 ItemID）。",
        "RU": "Идентификатор рендеринга (эквивалент ItemID).",
        "EN": "Rendering identifier (equivalent to ItemID).",
        "IT": "Identificativo del rendering (equivalente a ItemID).",
        "ES": "Identificador de renderizado (equivalente a ItemID)."
    },
    "Locazione posizionale specifica se all'interno di una griglia di inventario.": {
        "DE": "Spezifische Position, wenn sich das Objekt in einem Inventarraster befindet.",
        "ZH": "如果在物品栏网格中，则为特定的位置。",
        "RU": "Конкретное позиционное расположение, если находится внутри сетки инвентаря.",
        "EN": "Specific positional location if within an inventory grid.",
        "IT": "Locazione posizionale specifica se all'interno di una griglia di inventario.",
        "ES": "Ubicación posicional específica si está dentro de una cuadrícula de inventario."
    },
    "Alias per l'attributo cromatico.": {
        "DE": "Alias für das Farbattribut.",
        "ZH": "颜色属性的别名。",
        "RU": "Псевдоним цветового атрибута.",
        "EN": "Alias for the color attribute.",
        "IT": "Alias per l'attributo cromatico.",
        "ES": "Alias para el atributo cromático."
    },
    "Struttura Bitmap generata tramite rendering off-screen.": {
        "DE": "Bitmap-Struktur, die durch Off-Screen-Rendering erzeugt wurde.",
        "ZH": "通过离屏渲染生成的位图结构。",
        "RU": "Структура Bitmap, созданная с помощью закадрового рендеринга.",
        "EN": "Bitmap structure generated via off-screen rendering.",
        "IT": "Struttura Bitmap generata tramite rendering off-screen.",
        "ES": "Estructura de mapa de bits generada mediante renderizado fuera de pantalla."
    },
    "Riconoscimento speciale dell'oggetto legato a meccaniche di trasporto banca.": {
        "DE": "Spezielle Erkennung des Objekts im Zusammenhang mit Banktransportmechaniken.",
        "ZH": "与银行运输机制相关的对象的特殊识别。",
        "RU": "Специальное распознавание объекта, связанного с механикой банковских переводов.",
        "EN": "Special recognition of the object linked to bank transport mechanics.",
        "IT": "Riconoscimento speciale dell'oggetto legato a meccaniche di trasporto banca.",
        "ES": "Reconocimiento especial del objeto vinculado a la mecánica de transporte bancario."
    },
    "Rileva se la tipologia dell'oggetto supporta l'inserimento di figli.": {
        "DE": "Erkennt, ob der Objekttyp das Einfügen von untergeordneten Elementen unterstützt.",
        "ZH": "检测对象类型是否支持插入子项。",
        "RU": "Определяет, поддерживает ли тип объекта вставку дочерних элементов.",
        "EN": "Detects if the object type supports the insertion of children.",
        "IT": "Rileva se la tipologia dell'oggetto supporta l'inserimento di figli.",
        "ES": "Detecta si el tipo de objeto admite la inserción de secundarios."
    },
    "Rileva la natura organica o strutturale di un cadavere esplorabile.": {
        "DE": "Erkennt die organische oder strukturelle Natur einer erforschbaren Leiche.",
        "ZH": "检测可探索尸体的有机或结构性质。",
        "RU": "Определяет органическую или структурную природу исследуемого трупа.",
        "EN": "Detects the organic or structural nature of an explorable corpse.",
        "IT": "Rileva la natura organica o strutturale di un cadavere esplorabile.",
        "ES": "Detecta la naturaleza orgánica o estructural de un cadáver explorable."
    },
    "Determina l'appartenenza alle logiche di apertura e collisione delle porte.": {
        "DE": "Bestimmt die Zugehörigkeit zur Öffnungs- und Kollisionslogik von Türen.",
        "ZH": "确定是否属于门的开启和碰撞逻辑。",
        "RU": "Определяет принадлежность к логике открытия и столкновения дверей.",
        "EN": "Determines membership in door opening and collision logics.",
        "IT": "Determina l'appartenenza alle logiche di apertura e collisione delle porte.",
        "ES": "Determina la pertenencia a las lógicas de apertura y colisión de puertas."
    },
    "Naviga la gerarchia per stabilire se il root container è la banca del giocatore.": {
        "DE": "Navigiert durch die Hierarchie, um festzustellen, ob der Root-Container die Bank des Spielers ist.",
        "ZH": "导航层次结构以确定根容器是否为玩家银行。",
        "RU": "Навигация по иерархии для определения того, является ли корневой контейнер банком игрока.",
        "EN": "Navigates the hierarchy to establish if the root container is the player's bank.",
        "IT": "Naviga la gerarchia per stabilire se il root container è la banca del giocatore.",
        "ES": "Navega por la jerarquía para establecer si el contenedor raíz es el banco del jugador."
    },
    "Distingue tra oggetti recuperabili e decorazioni anatomiche dei cadaveri.": {
        "DE": "Unterscheidet zwischen plünderbaren Objekten und anatomischen Dekorationen von Leichen.",
        "ZH": "区分可搜刮物品和尸体的解剖装饰。",
        "RU": "Различает добываемые предметы и анатомические украшения трупов.",
        "EN": "Distinguishes between lootable objects and anatomical decorations of corpses.",
        "IT": "Distingue tra oggetti recuperabili e decorazioni anatomiche dei cadaveri.",
        "ES": "Distingue entre objetos saqueables y decoraciones anatómicas de cadáveres."
    },
    "Classifica l'oggetto per le routine di automazione della cura e dei buff.": {
        "DE": "Klassifiziert das Objekt für Heilungs- und Buff-Automatisierungsroutinen.",
        "ZH": "为治疗和增益自动化程序对对象进行分类。",
        "RU": "Классифицирует объект для процедур автоматизации лечения и усиления.",
        "EN": "Classifies the object for healing and buff automation routines.",
        "IT": "Classifica l'oggetto per le routine di automazione della cura e dei buff.",
        "ES": "Clasifica el objeto para las rutinas de automatización de curaciones y mejoras."
    },
    "Raggruppa materiali grezzi da artigianato (legno, minerali, pelli).": {
        "DE": "Gruppiert Rohmaterialien für das Handwerk (Holz, Erz, Leder).",
        "ZH": "将原材料分组用于手工艺（木材、矿石、皮革）。",
        "RU": "Группирует сырье для крафта (дерево, руда, кожа).",
        "EN": "Groups raw crafting materials (wood, ore, leather).",
        "IT": "Raggruppa materiali grezzi da artigianato (legno, minerali, pelli).",
        "ES": "Agrupa materiales artesanales en bruto (madera, minerales, cueros)."
    },
    "Segnala la possibilità di ispezionare il contenuto senza aprirlo fisicamente.": {
        "DE": "Signalisiert die Möglichkeit, den Inhalt zu inspizieren, ohne ihn physisch zu öffnen.",
        "ZH": "指示无需物理打开即可检查内容的可能性。",
        "RU": "Указывает на возможность проверки содержимого без физического вскрытия.",
        "EN": "Signals the possibility of inspecting content without physically opening it.",
        "IT": "Segnala la possibilità di ispezionare il contenuto senza aprirlo fisicamente.",
        "ES": "Indica la posibilidad de inspeccionar el contenido sin abrirlo físicamente."
    },
    "Parametro critico per le routine di disarmo e cast di incantesimi.": {
        "DE": "Kritischer Parameter für Entwaffnungs- und Zauberwirksamkeitsroutinen.",
        "ZH": "解除武装和施法程序的关键参数。",
        "RU": "Критический параметр для процедур обезоруживания и произнесения заклинаний.",
        "EN": "Critical parameter for disarmament and spell casting routines.",
        "IT": "Parametro critico per le routine di disarmo e cast di incantesimi.",
        "ES": "Parámetro crítico para las rutinas de desarme y lanzamiento de hechizos."
    },
    "Classifica un artefatto specifico per le routine di equipaggiamento.": {
        "DE": "Klassifiziert ein spezifisches Artefakt für Ausrüstungsroutinen.",
        "ZH": "为装备程序分类特定神器。",
        "RU": "Классифицирует конкретный артефакт для процедур экипировки.",
        "EN": "Classifies a specific artifact for equipment routines.",
        "IT": "Classifica un artefatto specifico per le routine di equipaggiamento.",
        "ES": "Clasifica un artefacto específico para rutinas de equipamiento."
    },
    "Valore univoco per la definizione della tipologia dell'oggetto.": {
        "DE": "Eindeutiger Wert zur Definition des Objekttyps.",
        "ZH": "用于定义对象类型的唯一值。",
        "RU": "Уникальное значение для определения типа объекта.",
        "EN": "Unique value for defining the object type.",
        "IT": "Valore univoco per la definizione della tipologia dell'oggetto.",
        "ES": "Valor único para definir el tipo de objeto."
    },
    "Definizione anatomica di equipaggiamento (es. testa, braccia).": {
        "DE": "Anatomische Definition der Ausrüstung (z. B. Kopf, Arme).",
        "ZH": "装备的解剖学定义（例如头部、手臂）。",
        "RU": "Анатомическое определение экипировки (например, голова, руки).",
        "EN": "Anatomical definition of equipment (e.g., head, arms).",
        "IT": "Definizione anatomica di equipaggiamento (es. testa, braccia).",
        "ES": "Definición anatómica del equipo (p. ej., cabeza, brazos)."
    },
    "Parametro di illuminazione locale applicato al motore grafico.": {
        "DE": "Lokaler Beleuchtungsparameter, der auf die Grafik-Engine angewendet wird.",
        "ZH": "应用于图形引擎的局部光照参数。",
        "RU": "Параметр локального освещения, применяемый к графическому движку.",
        "EN": "Local lighting parameter applied to the graphics engine.",
        "IT": "Parametro di illuminazione locale applicato al motore grafico.",
        "ES": "Parámetro de iluminación local aplicado al motor gráfico."
    },
    "Valore massimo teorico dei punti struttura per la gestione delle riparazioni.": {
        "DE": "Theoretischer Maximalwert der Strukturpunkte für das Reparaturmanagement.",
        "ZH": "维修管理的理论最大结构点值。",
        "RU": "Теоретическое максимальное значение очков структуры для управления ремонтом.",
        "EN": "Theoretical maximum value of structure points for repair management.",
        "IT": "Valore massimo teorico dei punti struttura per la gestione delle riparazioni.",
        "ES": "Valor máximo teórico de los puntos de estructura para la gestión de reparaciones."
    },
    "Riflette il flag del server che previene lo spostamento (oggetti fissi).": {
        "DE": "Spiegelt das Server-Flag wider, das das Verschieben verhindert (feste Objekte).",
        "ZH": "反映防止移动的服务器标志（固定对象）。",
        "RU": "Отражает флаг сервера, предотвращающий перемещение (фиксированные объекты).",
        "EN": "Reflects the server flag that prevents movement (fixed objects).",
        "IT": "Riflette il flag del server che previene lo spostamento (oggetti fissi).",
        "ES": "Refleja el flag del servidor que impide el movimiento (objetos fijos)."
    },
    "Stringa cache del nome, dipendente dai pacchetti cliloc.": {
        "DE": "Gecachter Namensstring, abhängig von Cliloc-Paketen.",
        "ZH": "缓存的名称字符串，取决于 cliloc 包。",
        "RU": "Кэшированная строка имени, зависящая от пакетов cliloc.",
        "EN": "Cached name string, depending on cliloc packages.",
        "IT": "Stringa cache del nome, dipendente dai pacchetti cliloc.",
        "ES": "Cadena de caché del nombre, dependiente de los paquetes cliloc."
    },
    "Indica l'assenza di parent container, confermando la posizione sulla mappa.": {
        "DE": "Zeigt das Fehlen eines übergeordneten Containers an und bestätigt die Position auf der Karte.",
        "ZH": "指示缺少父容器，确认地图上的位置。",
        "RU": "Указывает на отсутствие родительского контейнера, подтверждая положение на карте.",
        "EN": "Indicates the absence of a parent container, confirming the position on the map.",
        "IT": "Indica l'assenza di parent container, confermando la posizione sulla mappa.",
        "ES": "Indica la ausencia de un contenedor padre, confirmando la posición en el mapa."
    },
    "Struttura tridimensionale geometrica dell'istanza.": {
        "DE": "Geometrische dreidimensionale Struktur der Instanz.",
        "ZH": "实例的几何三维结构。",
        "RU": "Геометрическая трехмерная структура экземпляра.",
        "EN": "Geometrical three-dimensional structure of the instance.",
        "IT": "Struttura tridimensionale geometrica dell'istanza.",
        "ES": "Estructura tridimensional geométrica de la instancia."
    },
    "Vettore contenente tutte le righe dei tooltip parificate in stringhe e interi.": {
        "DE": "Vektor, der alle Tooltip-Zeilen enthält, die als Strings und Integers angeglichen wurden.",
        "ZH": "包含所有对等为字符串和整数的工具提示行的向量。",
        "RU": "Вектор, содержащий все строки подсказок, приравненные к строкам и целым числам.",
        "EN": "Vector containing all tooltip lines matched as strings and integers.",
        "IT": "Vettore contenente tutte le righe dei tooltip parificate in stringhe e interi.",
        "ES": "Vector que contiene todas las líneas de información sobre herramientas equiparadas en cadenas y enteros."
    },
    "Assicura che la richiesta di proprietà asincrona sia andata a buon fine.": {
        "DE": "Stellt sicher, dass die asynchrone Eigenschaftsanfrage erfolgreich war.",
        "ZH": "确保异步属性请求成功。",
        "RU": "Гарантирует, что запрос асинхронного свойства прошел успешно.",
        "EN": "Ensures that the asynchronous property request was successful.",
        "IT": "Assicura che la richiesta di proprietà asincrona sia andata a buon fine.",
        "ES": "Asegura que la solicitud de propiedad asíncrona haya tenido éxito."
    },
    "Esegue il traversing dell'albero gerarchico fino a trovare il proprietario finale.": {
        "DE": "Durchläuft den hierarchischen Baum, bis der endgültige Besitzer gefunden ist.",
        "ZH": "遍历层次树直到找到最终所有者。",
        "RU": "Выполняет обход иерархического дерева до нахождения конечного владельца.",
        "EN": "Traverses the hierarchical tree until it finds the final owner.",
        "IT": "Esegue il traversing dell'albero gerarchico fino a trovare il proprietario finale.",
        "ES": "Recorre el árbol jerárquico hasta encontrar al propietario final."
    },
    "Chiave di rete univoca globale per transazioni con il server.": {
        "DE": "Global eindeutiger Netzwerkschlüssel für Transaktionen mit dem Server.",
        "ZH": "用于与服务器进行事务的全局唯一网络密钥。",
        "RU": "Глобальный уникальный сетевой ключ для транзакций с сервером.",
        "EN": "Globally unique network key for transactions with the server.",
        "IT": "Chiave di rete univoca globale per transazioni con il server.",
        "ES": "Clave de red única global para transacciones con el servidor."
    },
    "Segnala il completamento dell'inizializzazione primaria dell'oggetto.": {
        "DE": "Signalisiert den Abschluss der primären Initialisierung des Objekts.",
        "ZH": "指示对象的主初始化完成。",
        "RU": "Указывает на завершение первичной инициализации объекта.",
        "EN": "Signals the completion of the object's primary initialization.",
        "IT": "Segnala il completamento dell'inizializzazione primaria dell'oggetto.",
        "ES": "Indica la finalización de la inicialización primaria del objeto."
    },
    "Stato di visibilità locale, influenzabile tramite script.": {
        "DE": "Lokaler Sichtbarkeitsstatus, beeinflussbar durch Skripte.",
        "ZH": "局部可见性状态，可通过脚本影响。",
        "RU": "Локальный статус видимости, на который можно влиять с помощью скриптов.",
        "EN": "Local visibility state, can be influenced via scripts.",
        "IT": "Stato di visibilità locale, influenzabile tramite script.",
        "ES": "Estado de visibilidad local, influenciable mediante scripts."
    },
    "Contribuzione gravitazionale dell'oggetto, necessaria per controlli di capienza.": {
        "DE": "Gravitativer Beitrag des Objekts, erforderlich für Kapazitätskontrollen.",
        "ZH": "对象的重力贡献，对于容量检查是必需的。",
        "RU": "Гравитационный вклад объекта, необходимый для проверки вместимости.",
        "EN": "Gravitational contribution of the object, necessary for capacity checks.",
        "IT": "Contribuzione gravitazionale dell'oggetto, necessaria per controlli di capienza.",
        "ES": "Contribución gravitacional del objeto, necesaria para controles de capacidad."
    },
    "Funzione pitagorica per la misurazione della distanza in tile bidimensionali.": {
        "DE": "Pythagoreische Funktion zur Messung der Entfernung in zweidimensionalen Kacheln.",
        "ZH": "用于测量二维瓦片距离的毕达哥拉斯函数。",
        "RU": "Пифагорейская функция для измерения расстояния в двумерных плитках.",
        "EN": "Pythagorean function for measuring distance in two-dimensional tiles.",
        "IT": "Funzione pitagorica per la misurazione della distanza in tile bidimensionali.",
        "ES": "Función pitagórica para medir la distancia en mosaicos bidimensionales."
    },
    "Risolve le coordinate globali estraendole dai contenitori padre se necessario.": {
        "DE": "Löst globale Koordinaten auf, indem sie bei Bedarf aus übergeordneten Containern extrahiert werden.",
        "ZH": "如有必要，通过从父容器中提取全局坐标来解析它们。",
        "RU": "Разрешает глобальные координаты, извлекая их из родительских контейнеров, если это необходимо.",
        "EN": "Resolves global coordinates by extracting them from parent containers if necessary.",
        "IT": "Risolve le coordinate globali estraendole dai contenitori padre se necessario.",
        "ES": "Resuelve coordenadas globales extrayéndolas de contenedores padre si es necesario."
    },
    "Determina l'inclusione logica controllando i rami dell'albero gerarchico.": {
        "DE": "Bestimmt den logischen Einschluss durch Überprüfung der Zweige des hierarchischen Baums.",
        "ZH": "通过检查分层树的分支来确定逻辑包含。",
        "RU": "Определяет логическое включение, проверяя ветви иерархического дерева.",
        "EN": "Determines logical inclusion by checking branches of the hierarchical tree.",
        "IT": "Determina l'inclusione logica controllando i rami dell'albero gerarchico.",
        "ES": "Determina la inclusión lógica comprobando las ramas del árbol jerárquico."
    },
    "Rileva lo stato di invulnerabilità conferito dal server.": {
        "DE": "Erkennt den vom Server verliehenen Unverwundbarkeitsstatus.",
        "ZH": "检测服务器授予的无敌状态。",
        "RU": "Определяет статус неуязвимости, предоставленный сервером.",
        "EN": "Detects the invulnerability status granted by the server.",
        "IT": "Rileva lo stato di invulnerabilità conferito dal server.",
        "ES": "Detecta el estado de invulnerabilidad otorgado por el servidor."
    },
    "Limita ai Graphic ID specifici del corpo (es. demoni o umani).": {
        "DE": "Beschränkt auf körperbezogene Grafik-IDs (z. B. Dämonen oder Menschen).",
        "ZH": "限制为特定于身体的图形 ID（例如恶魔或人类）。",
        "RU": "Ограничивает конкретными графическими идентификаторами тела (например, демоны или люди).",
        "EN": "Limits to body-specific Graphic IDs (e.g., demons or humans).",
        "IT": "Limita ai Graphic ID specifici del corpo (es. demoni o umani).",
        "ES": "Limita a ID gráficos específicos del cuerpo (p. ej., demonios o humanos)."
    }
}

def clean_params(params):
    if params == "N/A":
        return ""
    # "millisec [Int32]" -> "millisec"
    parts = [p.strip().split(' [')[0].split('[')[0] for p in params.split(',')]
    return ", ".join(parts)

def generate_jsonl():
    lines = [l.strip() for l in data_raw.split('\n') if l.strip()]
    jsonl_output = []
    
    for i, line in enumerate(lines):
        # Handle comma in description (last field)
        # Split but keep the last field potentially containing commas
        parts = []
        current = ""
        in_quotes = False
        for char in line:
            if char == '"':
                in_quotes = not in_quotes
            elif char == ',' and not in_quotes:
                parts.append(current.strip())
                current = ""
            else:
                current += char
        parts.append(current.strip().strip('"'))
        
        module, type_it, api_name, return_type, params_raw, desc_it = parts
        
        lang = languages[i % 6]
        l_map = labels[lang]
        
        # Determine Type (Method/Property)
        if type_it == "Metodo":
            type_name = l_map["Method"]
        else:
            type_name = l_map["Property"]
            
        full_api = f"{module}.{api_name}"
        intro = f"{full_api} {l_map['Intro'].format(type=type_name)}"
        
        # Translate Description
        desc_translated = descriptions_map.get(desc_it, {}).get(lang, desc_it)
        
        # Format Code
        params_clean = clean_params(params_raw)
        if type_it == "Metodo":
            if return_type == "Void":
                code = f"{full_api}({params_clean})"
            else:
                code = f"result = {full_api}({params_clean})"
        else:
            code = f"result = {full_api}\n{l_map['PropertyComment']}"
            
        # Build Response
        response = (
            f"{intro}\n"
            f"{l_map['Description']}: {desc_translated}\n"
            f"{l_map['Return']}: {return_type}\n"
            f"{l_map['Parameters']}: {params_raw}\n"
            f"{l_map['Example']}:\n"
            f"{code}"
        )
        
        # Question
        question_template = l_map["Questions"][i % len(l_map["Questions"])]
        question = question_template.format(api=full_api)
        
        entry = {
            "messages": [
                {"role": "system", "content": "You are a Razor Enhanced scripting assistant for Ultima Online. You write correct IronPython scripts using only the official Razor Enhanced API. Never invent or guess methods, properties, or classes. If a task cannot be done with the available API, say so explicitly. Respond in the same language as the user."},
                {"role": "user", "content": question},
                {"role": "assistant", "content": response}
            ]
        }
        jsonl_output.append(json.dumps(entry, ensure_ascii=False))
        
    return jsonl_output

for line in generate_jsonl():
    print(line)
