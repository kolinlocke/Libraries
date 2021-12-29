using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Commons.EntityProps
{
    public class Common_Enums
    {
        public enum ExcelInterop_Operation
        {
            Blank = 0,
            ProcessExcel = 1,
            ProcessPDF = 2,
            ReadExcel = 3,
            ProcessExcelToPDF = 4
        }

        public enum ToUploadConsole_Operation
        {
            Blank = 0,
            ExcelToPDFWithWatermark = 1,
            ExcelToPDf = 2,
            OpenSaveExcel = 3
        }

        public enum EntitySource
        {
            Default = 0,
            AzureBlobStorageTable = 1
        }
    }
}
