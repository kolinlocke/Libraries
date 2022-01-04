using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Commons
{
    public class Logger
    {
        #region _Variables

        String Log_Name;
        DateTime Log_Start;
        StringBuilder Log_String = new StringBuilder();

        Action<String> mDs_Log;
        Action<String> mDs_EndLog;

        delegate void Ds_Sample(String MarvinPogi);

        #endregion

        #region _Constructor
                
        public Logger(
            String Log_Name
            , Action<String> Ds_Log = null
            , Action<String> Ds_EndLog = null)
        {
            this.mDs_Log = Ds_Log;
            this.mDs_EndLog = Ds_EndLog;

            this.Log_Name = Log_Name;
            this.Log_Start = DateTime.Now;
            String Log_Msg = $"({this.Log_Start:yyyy/MM/dd HH:mm:ss}) Log {this.Log_Name} started.";

            if (this.mDs_Log != null)
            { this.mDs_Log(Log_Msg); }
            
            this.Log_String.AppendLine(Log_Msg);
        }

        #endregion

        #region _Methods
                
        public void Log(String Msg)
        {
            DateTime Current = DateTime.Now;
            var Duration = Current.Subtract(this.Log_Start);

            String Log_Date = $"{Current:yyyy/MM/dd HH:mm:ss}";
            String Log_Duration = $"Elapsed {Duration.Hours:D2}:{Duration.Minutes:D2}:{Duration.Seconds:D2}";
            String Log_Msg = $"({Log_Date}) {Log_Duration} {Msg}";

            if (this.mDs_Log != null)
            { this.mDs_Log(Log_Msg); }

            this.Log_String.AppendLine(Log_Msg);
        }

        public void EndLog()
        {
            DateTime Current = DateTime.Now;
            var Duration = Current.Subtract(this.Log_Start);

            String Log_Date = $"{Current:yyyy/MM/dd HH:mm:ss}";
            String Log_Duration = $"Elapsed {Duration.Hours:D2}:{Duration.Minutes:D2}:{Duration.Seconds:D2}";
            String Log_Msg = $"({Log_Date}) {Log_Duration} Log {this.Log_Name} ended.";

            if (this.mDs_Log != null)
            { this.mDs_Log(Log_Msg); }

            this.Log_String.AppendLine(Log_Msg);

            if (this.mDs_EndLog != null)
            { this.mDs_EndLog(this.GetLog()); }
        }

        public String GetLog()
        {
            return this.Log_String.ToString();
        }

        #endregion
    }
}
