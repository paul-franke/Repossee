using NLog;
using System;
using System.Net;
using System.Threading.Tasks;
using System.Web.Http;
using DemoData.Data;
using Repository.Repository;


namespace DemoAPI.Controllers
{
    public class CourseController : BaseController
    {
        public CourseController() : base()
        {
        }

        public async Task<IHttpActionResult> Get(int id = 0)
        {
            string methodNameStr = $"CourseController().Get({id})";
            try
            {
                logger.Log(LogLevel.Info, $"{MakeLogStr4Entry(methodNameStr)}.");
                using (var context = new SampDB())
                {
                    var repos = new GenericDataRepository(context);
                    var res = await repos.GetAsync<Course>(id);
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
        public async Task<IHttpActionResult> Post(Course data)
        {
            string methodNameStr = $"CourseController().Post({data})";
            try
            {

                logger.Log(LogLevel.Info, $"{MakeLogStr4Entry(methodNameStr)}.");
                using (var context = new SampDB())
                {
                    var repos = new GenericDataRepository(context);
                    var httpStatusCode = await repos.InsertAsync<Course>(data);
                    if (httpStatusCode == HttpStatusCode.OK)
                        await context.SaveChangesAsync();
                    logger.Log(LogLevel.Info, $"{MakeLogStr4Exit(methodNameStr)} {httpStatusCode}");
                    if (httpStatusCode == HttpStatusCode.OK)
                        return Content(httpStatusCode, data.Id.ToString());
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