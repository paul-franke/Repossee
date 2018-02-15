using NLog;
using System.Web.Http;
using System;

namespace DemoAPI.Controllers
{


    public class BaseController : ApiController
    {
        public static Logger logger = LogManager.GetLogger("LoggerInstance");

        protected String MakeLogStr4Entry(String methodNameStr)
        {
            return $"{methodNameStr}: Entry into {methodNameStr}.";
        }
        protected String MakeLogStr4Exit(String methodNameStr)
        {
            return $"{methodNameStr}: Exit from {methodNameStr}. ReturnCode: ";
        }

    }

}