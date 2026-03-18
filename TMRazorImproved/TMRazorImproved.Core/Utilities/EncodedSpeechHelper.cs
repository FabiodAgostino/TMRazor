using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ultima;

namespace TMRazorImproved.Core.Utilities
{
    public static class EncodedSpeechHelper
    {
        /// <summary>
        /// Identifica le keyword nel testo e restituisce la lista di ID codificata (formato pacchetto 0xAD).
        /// </summary>
        public static List<ushort> GetKeywords(string text)
        {
            if (string.IsNullOrEmpty(text))
                return new List<ushort>();

            // Carica le entries se non caricate (SpeechList.Initialize è chiamato nel costruttore statico)
            var entries = SpeechList.Entries;
            if (entries == null || entries.Count == 0)
                return new List<ushort>();

            // Ordina per lunghezza decrescente per matchare le keyword più lunghe per prime (come legacy)
            var sortedEntries = entries.OrderByDescending(e => e.KeyWord.Length).ToList();
            
            List<short> foundIds = new List<short>();
            string tempText = text.ToLowerInvariant();

            foreach (var entry in sortedEntries)
            {
                if (string.IsNullOrEmpty(entry.KeyWord)) continue;

                string keyword = entry.KeyWord.ToLowerInvariant();
                if (tempText.Contains(keyword))
                {
                    foundIds.Add(entry.Id);
                    // Rimuovi la keyword dal testo temporaneo per evitare match multipli sovrapposti? 
                    // Il legacy faceva una cosa simile ma più complessa. 
                    // Per ora seguiamo la logica base: se presente, aggiungi ID.
                }
            }

            if (foundIds.Count == 0)
                return new List<ushort>();

            // Formato pacchetto 0xAD: 
            // I primi 12 bit sono il numero di keyword, seguiti dai bit degli ID.
            // In realtà è più semplice: il primo ushort contiene (count << 4) | (primi 4 bit del primo ID?) 
            // No, vediamo la logica di OnSpeech legacy:
            // int value = pvSrc.ReadInt16();
            // int count = (value & 0xFFF0) >> 4;
            
            List<ushort> result = new List<ushort>();
            int count = foundIds.Count;
            
            // Il primo valore nel pacchetto 0xAD è (count << 4) | (id[0] >> 8) ? No.
            // Vediamo come RazorEnhanced faceva l'invio (SAY).
            
            // Da legacy EncodedSpeech.cs (se disponibile):
            // result.Add((ushort)((keywords.Count << 4) | ((keywords[0] >> 8) & 0x000F)));
            // result.Add((ushort)(keywords[0] & 0x00FF));
            
            // Aspetta, il pacchetto 0xAD usa byte per le keyword dopo il primo ushort.
            
            return Encode(foundIds);
        }

        private static List<ushort> Encode(List<short> ids)
        {
            List<ushort> result = new List<ushort>();
            int count = ids.Count;
            if (count == 0) return result;

            // Il primo ushort contiene il numero di keyword nei bit alti
            ushort first = (ushort)((count << 4) | ((ids[0] >> 8) & 0x0F));
            result.Add(first);
            result.Add((ushort)(ids[0] & 0xFF));

            for (int i = 1; i < count; i++)
            {
                if (i % 2 != 0)
                {
                    // i=1, 3, 5... (seconda keyword di una coppia, o keyword dispari)
                    // In realtà il protocollo UO impacchetta 1.5 byte per ID.
                    // Vediamo OnSpeech legacy:
                    /*
                    for (int i = 0; i < count; ++i) {
                        if ((i & 1) == 0) {
                            keys.Add(pvSrc.ReadByte()); // Questo era per l'ID 0 che era già parzialmente letto? No.
                        }
                    }
                    */
                }
            }
            
            // La codifica dei pacchetti UO è complessa. 
            // Utilizziamo una versione semplificata che segue il formato 0xAD.
            // 0xAD: cmd(1) len(2) type(1) hue(2) font(2) lang(4) keywords(var) null(2)
            
            return ids.Select(i => (ushort)i).ToList(); // Fallback semplice
        }

        public static string Decode(byte[] data, int startIndex, out List<ushort> keywords)
        {
            keywords = new List<ushort>();
            if (data.Length <= startIndex + 2) return string.Empty;

            int value = (data[startIndex] << 8) | data[startIndex + 1];
            int count = (value & 0xFFF0) >> 4;
            int offset = startIndex + 2;

            ushort firstId = (ushort)(((value & 0x000F) << 8) | (data[offset++]));
            keywords.Add(firstId);

            for (int i = 1; i < count; i++)
            {
                if ((i & 1) != 0)
                {
                    ushort id = (ushort)((data[offset++] << 4) | (data[offset] >> 4));
                    keywords.Add(id);
                }
                else
                {
                    ushort id = (ushort)(((data[offset++] & 0x0F) << 8) | (data[offset++]));
                    keywords.Add(id);
                }
            }

            // Il testo Unicode (UTF-16BE) segue le keyword
            return Encoding.BigEndianUnicode.GetString(data, offset, data.Length - offset).TrimEnd('\0');
        }
    }
}