const { GoogleGenerativeAI } = require("@google/generative-ai");
const fs = require("fs");
const path = require("path");

async function run() {
  try {
    // 1. Inizializza l'SDK con la chiave API e imposta Gemini 3.1 Pro
    const genAI = new GoogleGenerativeAI(process.env.GEMINI_API_KEY);
    const model = genAI.getGenerativeModel({ model: "gemini-3.1-pro" });

    // 2. Recupera il commento dall'Issue di GitHub
    const commentBody = process.env.COMMENT_BODY;
    if (!commentBody || !commentBody.startsWith("/gemini")) {
      console.log("Nessun comando /gemini rilevato. Esco.");
      return;
    }

    const userRequest = commentBody.replace("/gemini", "").trim();
    console.log(`Richiesta ricevuta: ${userRequest}`);

    // 3. Istruzioni ferree per il modello (Prompt Engineering)
    const prompt = `
      Sei un agente sviluppatore autonomo. Il tuo compito è scrivere o modificare codice in base alla richiesta dell'utente.
      Richiesta dell'utente: "${userRequest}"
      
      DEVI rispondere SOLO ed ESCLUSIVAMENTE con un oggetto JSON valido, senza testo aggiuntivo o formattazione Markdown (niente \`\`\`json).
      Il JSON deve avere questa struttura:
      {
        "file": "percorso/del/file/da/creare_o_modificare.js",
        "content": "il codice sorgente completo e aggiornato del file"
      }
    `;

    // 4. Chiama Gemini 3.1 Pro
    console.log("Elaborazione con Gemini 3.1 Pro in corso...");
    const result = await model.generateContent(prompt);
    let responseText = result.response.text();

    // Pulizia di eventuale formattazione residua
    responseText = responseText.replace(/```json/g, "").replace(/```/g, "").trim();

    // 5. Applica le modifiche al file system locale del server di GitHub
    const aiResponse = JSON.parse(responseText);
    const targetPath = path.resolve(process.cwd(), aiResponse.file);
    
    // Assicurati che la cartella di destinazione esista
    fs.mkdirSync(path.dirname(targetPath), { recursive: true });
    
    // Scrivi il nuovo codice nel file
    fs.writeFileSync(targetPath, aiResponse.content, "utf8");
    
    console.log(`✅ File ${aiResponse.file} aggiornato con successo!`);

  } catch (error) {
    console.error("❌ Errore durante l'esecuzione dell'agente:", error);
    process.exit(1); // Fai fallire la GitHub Action in caso di errore
  }
}

run();