using System;
using System.IO;
using System.Text;

namespace TMRazorImproved.Core.Services.Scripting
{
    /// <summary>
    /// TextWriter che reindirizza l'output dello script Python (stdout/stderr)
    /// verso un callback C# linea per linea.
    /// Usato con <c>engine.Runtime.IO.SetOutput / SetErrorOutput</c>.
    /// </summary>
    internal sealed class ScriptOutputWriter : TextWriter
    {
        private readonly Action<string> _lineCallback;
        private readonly StringBuilder _buffer = new();

        public override Encoding Encoding => Encoding.UTF8;

        public ScriptOutputWriter(Action<string> lineCallback)
        {
            _lineCallback = lineCallback;
        }

        public override void Write(char value)
        {
            if (value == '\n')
            {
                Flush();
            }
            else if (value != '\r')
            {
                _buffer.Append(value);
            }
        }

        public override void Write(string? value)
        {
            if (value is null) return;
            foreach (char c in value)
                Write(c);
        }

        public override void Flush()
        {
            if (_buffer.Length > 0)
            {
                _lineCallback(_buffer.ToString());
                _buffer.Clear();
            }
        }

        protected override void Dispose(bool disposing)
        {
            // Svuota eventuali dati senza newline finale
            Flush();
            base.Dispose(disposing);
        }
    }
}
