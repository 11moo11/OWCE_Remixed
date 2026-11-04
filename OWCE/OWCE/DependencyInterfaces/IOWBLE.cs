using System;
using System.Threading;
using System.Threading.Tasks;

namespace OWCE.DependencyInterfaces
{
    public interface IOWBLE
    {
        public bool IsScanning { get; }
        void StartScanning();
        void StopScanning();
        Task<bool> ReadyToScan();
        void Shutdown();
        void RequestRSSIUpdate();

        Task<bool> Connect(OWBaseBoard board, CancellationToken cancellationToken);
        Task Disconnect();

        Action<String> ErrorOccurred { get; set; }

        Action<OWBaseBoard> BoardDiscovered { get; set; }
        Action<BluetoothState> BLEStateChanged { get; set; }
        Action<string, byte[]> BoardValueChanged { get; set; }
        Action<int> RSSIUpdated { get; set; }
        Action BoardDisconnected { get; set; }
        Action BoardReconnecting { get; set; }
        Action BoardReconnected { get; set; }


        Task<byte[]> ReadValue(string characteristicGuid, bool important = false);
        /// <summary>
        /// Writes a value. When <paramref name="throwOnFailure"/> is true the returned task faults if the board rejects the write
        /// (by default a rejected write is ignored, as it always has been).
        /// </summary>
        Task<byte[]> WriteValue(string characteristicGuid, byte[] data, bool overrideExistingQueue = false, bool throwOnFailure = false);
        Task SubscribeValue(string characteristicGuid, bool important = false);
        Task UnsubscribeValue(string characteristicGuid, bool important = false);
    }
}
