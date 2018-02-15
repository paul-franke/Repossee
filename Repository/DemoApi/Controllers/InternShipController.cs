using System;
using System.Net;
using System.Threading.Tasks;
using System.Web.Http;
using DemoData.Data;
using NLog;
using Repository.Repository;

namespace DemoAPI.Controllers
{
    public class InternShipController : BaseController
    {
        public InternShipController() : base()
        {
        }

        public async Task<IHttpActionResult> Get(int id = 0)
        {
            string methodNameStr = $"InternShipController().Get({id})";
            try
            {
                logger.Log(LogLevel.Info, $"{MakeLogStr4Entry(methodNameStr)}.");
                using (var context = new SampDB())
                {
                    var repos = new GenericDataRepository(context);
                    var res = await repos.GetAsync<InternShip>(id);
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

        public async Task<IHttpActionResult> Delete(int id)
        {
            string methodNameStr = $"InternShipController().Delete({id})";
            try
            {

                logger.Log(LogLevel.Info, $"{MakeLogStr4Entry(methodNameStr)}.");
                using (var context = new SampDB())
                {
                    var repos = new GenericDataRepository(context);
                    var res = await repos.DeleteAsync<InternShip>(id);
                    if (res == HttpStatusCode.OK)
                        await context.SaveChangesAsync();
                    logger.Log(LogLevel.Info, $"{MakeLogStr4Exit(methodNameStr)} {res}");
                    return Content(res, res.ToString());
                }
            }
            catch (Exception e)
            {
                logger.Log(LogLevel.Error, $"{MakeLogStr4Exit(methodNameStr)}:\r\n{e}");
                return Content(HttpStatusCode.InternalServerError, HttpStatusCode.InternalServerError.ToString());
            }
        }
        public async Task<IHttpActionResult> Put(InternShip data)
        {
            string methodNameStr = $"InternShipController().Put({data})";
            try
            {

                logger.Log(LogLevel.Info, $"{MakeLogStr4Entry(methodNameStr)}.");
                using (var context = new SampDB())
                {
                    var repos = new GenericDataRepository(context);
                    var res = await repos.UpdateAsync<InternShip>(data);
                    if (res == HttpStatusCode.OK)
                        await context.SaveChangesAsync();
                    logger.Log(LogLevel.Info, $"{MakeLogStr4Exit(methodNameStr)} {res}");
                    return Content(res, res.ToString());
                }
            }
            catch (Exception e)
            {
                logger.Log(LogLevel.Error, $"{MakeLogStr4Exit(methodNameStr)}:\r\n{e}");
                return Content(HttpStatusCode.InternalServerError, HttpStatusCode.InternalServerError.ToString());
            }
        }
        public async Task<IHttpActionResult> Post(InternShip InternShipData)
        {
            string methodNameStr = $"InternShipController().Post({InternShipData})";
            try
            {

                logger.Log(LogLevel.Info, $"{MakeLogStr4Entry(methodNameStr)}.");
                using (var context = new SampDB())
                {
                    var repos = new GenericDataRepository(context);
                    var httpStatusCode = await repos.InsertAsync<InternShip>(InternShipData);
                    if (httpStatusCode == HttpStatusCode.OK)
                        await context.SaveChangesAsync();
                    logger.Log(LogLevel.Info, $"{MakeLogStr4Exit(methodNameStr)} {httpStatusCode}");
                    if (httpStatusCode == HttpStatusCode.OK)
                        return Content(httpStatusCode, InternShipData.Id.ToString());
                    else
                        return Content(httpStatusCode, httpStatusCode.ToString());

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
