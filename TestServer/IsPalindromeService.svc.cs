using System;
using System.Configuration;
using System.Threading;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Activation;
using System.ServiceModel.Web;

namespace TestServer
{
    [ServiceContract(Namespace = "")]
    [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Allowed)]
    public class IsPalindromeService
    {
        private static int maxClients = Convert.ToInt32(ConfigurationManager.AppSettings["N"]);  //Максимальное количество операций в момент времени
                                                                                                 //Число задается в конфиге
        private static Semaphore sem = new Semaphore(maxClients, maxClients);  //Семафор для ограничения операций

        [WebGet(UriTemplate = "/Info")]  //Get-запрос на получение информации о максимальном количестве операций в момент времени

        private string GetInfo() => $"Current number of Max Clients is {maxClients}";

        [WebInvoke(Method = "POST", RequestFormat = WebMessageFormat.Json,  //Post-запрос на проверку строки на палиндром
            UriTemplate = "/Check", ResponseFormat = WebMessageFormat.Json,
            BodyStyle = WebMessageBodyStyle.Wrapped)]
        private string CheckPalindrome(string text)
        {
            if (sem.WaitOne(1))  //Если есть ожидание, т.е. семафор занят - сообщение об ошибке, иначе - проверка строки на палиндром
            {
                bool isPalindrome = text == new string(text.Reverse().ToArray()) ? true : false;
                Thread.Sleep(5000);
                sem.Release();
                return isPalindrome.ToString();
            }
            else
                return "The server is busy. Try again later.";
        }
    }
}
