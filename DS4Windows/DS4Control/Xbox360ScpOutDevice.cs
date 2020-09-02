using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DS4Windows
{
    class Xbox360ScpOutDevice : OutputDevice
    {
        private const int inputResolution = 127 - (-128);
        private const float reciprocalInputResolution = 1 / (float)inputResolution;
        private const int outputResolution = 32767 - (-32768);
        public const string devType = "X360";
        
        private byte[] report = new byte[28];
        private byte[] rumble = new byte[8];

        private X360BusDevice x360Bus;
        private int slotIdx = 0;

        public delegate void Xbox360FeedbackReceivedEventHandler(Xbox360ScpOutDevice sender, byte large, byte small, int idx);
        public event Xbox360FeedbackReceivedEventHandler FeedbackReceived;
        public Xbox360FeedbackReceivedEventHandler forceFeedbackCall;

        public Xbox360ScpOutDevice(X360BusDevice client, int idx)
        {
            this.x360Bus = client;
            slotIdx = idx;
        }

        public override void Connect()
        {
            x360Bus.Plugin(slotIdx);
        }

        public override void ConvertandSendReport(DS4State state, int device)
        {
            x360Bus.Parse(state, report, slotIdx);
            if (x360Bus.Report(report, rumble))
            {
                byte Big = rumble[3];
                byte Small = rumble[4];

                if (rumble[1] == 0x08)
                {
                    FeedbackReceived?.Invoke(this, Big, Small, slotIdx);
                }
            }
        }

        public override void Disconnect()
        {
            FeedbackReceived = null;
            x360Bus.Unplug(slotIdx);
        }

        public override string GetDeviceType() => devType;

        private DS4State emptyState = new DS4State();
        public override void ResetState(bool submit = true)
        {
            x360Bus.Parse(emptyState, report, slotIdx);
            if (submit)
            {
                x360Bus.Report(report, rumble);
            }
        }
    }
}
