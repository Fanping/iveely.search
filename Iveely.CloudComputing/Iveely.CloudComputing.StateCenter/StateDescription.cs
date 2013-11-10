using System;
using Iveely.CloudComputing.StateCommon;

namespace Iveely.CloudComputing.StateCenter
{
    [Serializable]
    public class StateDescription
    {
        public string AppName { get; set; }

        public ExcuteType ExcuteType { get; set; }

        public ExcuteState ExcuteState { get; set; }

        public double StatePercentage { get; set; }

        public Exception Exception { get; set; }

        private readonly DateTime _dateTime;

        public StateDescription(string appName, ExcuteType excuteType, ExcuteState excuteState, double statePercentage, Exception exception)
        {
            AppName = appName;
            ExcuteType = excuteType;
            ExcuteState = excuteState;
            StatePercentage = statePercentage;
            Exception = exception;
            _dateTime = DateTime.Now;
        }

        public override string ToString()
        {
            string info = _dateTime + "  " + ExcuteType + " " + ExcuteState + "    " + StatePercentage;
            if (Exception != null)
            {
                info += Exception.ToString();
            }
            return info;
        }

    }
}
