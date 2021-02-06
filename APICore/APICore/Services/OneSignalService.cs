using Newtonsoft.Json.Linq;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;

namespace APICore.Services
{
    public interface IOneSignalService
    {
        void SendNotification(int[] userIds, string title, string content);
    }

    public class OneSignalService : IOneSignalService
    {
        public void SendNotification(int[] userIds, string title, string content)
        {
            var request = WebRequest.Create("https://onesignal.com/api/v1/notifications") as HttpWebRequest;

            request.KeepAlive = true;
            request.Method = "POST";
            request.ContentType = "application/json; charset=utf-8";

            request.Headers.Add("authorization", "Basic Y2U4ODdmYzUtMTRkNS00MGQzLWIyOWItMDc0YmM0MTliNGNl");

            var param = JObject.FromObject(new
            {
                app_id = "e87a63aa-3beb-4c15-abbd-71e324625722",
                headings = new { en = title },
                contents = new { en = content },
                channel_for_external_user_ids = "push",
                include_external_user_ids = userIds.Select(x => x.ToString()),
            });

            byte[] byteArray = Encoding.UTF8.GetBytes(param.ToString());

            string responseContent = null;

            try
            {
                using (var writer = request.GetRequestStream())
                {
                    writer.Write(byteArray, 0, byteArray.Length);
                }

                using (var response = request.GetResponse() as HttpWebResponse)
                {
                    using (var reader = new StreamReader(response.GetResponseStream()))
                    {
                        responseContent = reader.ReadToEnd();
                        Vars.Logger.Info(responseContent);
                    }
                }
            }
            catch (WebException ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
                System.Diagnostics.Debug.WriteLine(new StreamReader(ex.Response.GetResponseStream()).ReadToEnd());
            }

            System.Diagnostics.Debug.WriteLine(responseContent);
        }
    }
}
