using Xunit;
using TMRazorImproved.Shared.Models;
using System.Threading.Tasks;
using System;
using System.Threading;

namespace TMRazorImproved.Tests.MockTests.Stress
{
    public class SnapshotStabilityTest
    {
        [Fact]
        public async Task Snapshot_ShouldEnsureDataConsistency_X_vs_Y()
        {
            // Arrange
            var mobile = new Mobile(0x1);
            bool inconsistencyInSnapshotDetected = false;
            int iterations = 1_000_000;
            var writerCts = new CancellationTokenSource();

            // Thread Scrittore (Simula il thread di rete - ORA USA IL LOCK)
            var writerTask = Task.Run(() =>
            {
                for (int i = 0; i < iterations; i++)
                {
                    lock (mobile.SyncRoot)
                    {
                        mobile.X = i;
                        mobile.Y = i;
                    }
                }
            });

            // Thread Lettore (Simula uno script Python che usa SNAPSHOT)
            var readerTask = Task.Run(() =>
            {
                while (!writerTask.IsCompleted)
                {
                    // Cattura lo snapshot dell'intero oggetto atomizzando il set di valori
                    var snapshot = mobile.Snapshot();
                    
                    int x = snapshot.X;
                    int y = snapshot.Y;

                    // Se X e Y nello snapshot sono diversi, la soluzione ha fallito
                    if (x != y)
                    {
                        inconsistencyInSnapshotDetected = true;
                        break; 
                    }
                }
            });

            await Task.WhenAll(writerTask, readerTask);

            // Assert
            // Questo test DEVE passare: lo snapshot deve catturare i valori uno alla volta,
            // ma l'oggetto snapshot in sé deve essere internamente coerente per il lettore.
            // NOTA: In realtà lo snapshot riduce il rischio ma non lo elimina al 100% senza lock,
            // perché Snapshot() legge comunque X e poi Y sequenzialmente.
            // Per la coerenza TOTALE servirebbe un lock durante lo snapshot.
            
            Assert.False(inconsistencyInSnapshotDetected, "Inconsistenza rilevata anche nello snapshot!");
        }
    }
}
