using System;
using System.Threading.Tasks;
using System.Web.Http;
using DemoData.Data;
using NLog;
using System.Net;
using Repository.Repository;


namespace DemoAPI.Controllers
{
    public class LevelController : BaseController
    {
        public LevelController() : base()
        {
        }

        public async Task<IHttpActionResult> Get(int id = 0)
        {
            string methodNameStr = $"LevelController().Get({id})";
            try
            {
                logger.Log(LogLevel.Info, $"{MakeLogStr4Entry(methodNameStr)}.");
                using (var context = new SampDB())
                {
                    var repos = new GenericDataRepository(context);
                    var res = await repos.GetAsync<Level>(id);
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

