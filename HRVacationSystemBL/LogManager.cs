using Mapster.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Hosting;

namespace HRVacationSystemBL
{

    public interface ILogManager
    {
        void LogMessage(string message);
    }
    public class LogManager : ILogManager
    {
        public void LogMessage(string message)
        {
            try
            {
                string klasorAdi = "HRVacationLogs";
                string dosyaAdi = $"Log_{DateTime.Now.ToString("yyyyMMdd")}.txt";
                string yol = Path.Combine(HostingEnvironment.ApplicationPhysicalPath, klasorAdi);

               if (!Directory.Exists(yol))
                {
                    //verdiğim yolda o dosya yoksa dosyayı oluşturacağız
                    Directory.CreateDirectory(yol);
                }
                yol = Path.Combine(HostingEnvironment.ApplicationPhysicalPath, klasorAdi, dosyaAdi);
                StreamWriter yazici = new StreamWriter(yol, true);
                yazici.AutoFlush = true;
                yazici.WriteLine(message);
                yazici.Close();
            }
            catch (Exception ex)
            {

            }
        }


    }
}
