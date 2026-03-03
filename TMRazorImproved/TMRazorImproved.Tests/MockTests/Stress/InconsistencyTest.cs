using Xunit;
using TMRazorImproved.Shared.Models;
using System.Threading.Tasks;
using System;
using System.Threading;

namespace TMRazorImproved.Tests.MockTests.Stress
{
    public class InconsistencyTest
    {
        [Fact]
        public async Task Demonstrate_TemporalInconsistency_X_vs_Y()
        {
            // Arrange
            var mobile = new Mobile(0x1);
            bool inconsistencyDetected = false;
            int iterations = 1_000_000;
            var cts = new CancellationTokenSource();

            // Thread Scrittore (Simula il thread di rete)
            // Imposta X e Y sempre allo stesso valore 'i'
            var writerTask = Task.Run(() =>
            {
                for (int i = 0; i < iterations; i++)
                {
                    mobile.X = i;
                    // Qui c'è un micro-istante in cui X è 'i' ma Y è ancora 'i-1'
                    mobile.Y = i;
                }
            });

            // Thread Lettore (Simula uno script Python)
            var readerTask = Task.Run(() =>
            {
                while (!writerTask.IsCompleted)
                {
                    int x = mobile.X;
                    int y = mobile.Y;

                    // Se X e Y sono diversi, abbiamo "beccato" l'inconsistenza
                    if (x != y)
                    {
                        inconsistencyDetected = true;
                        break; 
                    }
                }
            });

            await Task.WhenAll(writerTask, readerTask);

            // Assert
            // Se inconsistencyDetected è true, abbiamo dimostrato che i dati non sono thread-safe
            Assert.True(inconsistencyDetected, "Inconsistenza rilevata: lo script ha letto X e Y diversi nonostante lo scrittore li imposti sempre uguali.");
        }
    }
}
