using System.Threading.Tasks;
using System.Web.Http;
using NLog;
using System.Net;
using System;
using Repository.Repository;
using DemoData.Data;


namespace DemoAPI.Controllers
{

    public class CountryController : BaseController
    {
        public CountryController() : base() { }


        public async Task<IHttpActionResult> Get(int id = 0)
        {
            string methodNameStr = $"CountryController().Get({id})";
            try
            {
                logger.Log(LogLevel.Info, $"{MakeLogStr4Entry(methodNameStr)}.");
                using (var context = new SampDB())
                {
                    var repos = new GenericDataRepository(context);
                    var res = await repos.GetAsync<Country>(id);
                    if (res != null)
                    {
                        logger.Log(LogLevel.Info, $"{MakeLogStr4Exit(methodNameStr)} {res}");
                        return Content(HttpStatusCode.OK, res);
                    }
                    else
                    {
                        logger.Log(LogLevel.Info, $"{MakeLogStr4Exit(methodNameStr)} No content returned by repository.");
                        return Content(HttpStatusCode.NotFound, HttpStatusCode.NotFound);
                    }
                }
            }
            catch (Exception e)
            {
                logger.Log(LogLevel.Error, $"{MakeLogStr4Exit(methodNameStr)}:\r\n{e}");
                return Content(HttpStatusCode.InternalServerError, HttpStatusCode.InternalServerError.ToString());
            }
        }

    }
}

