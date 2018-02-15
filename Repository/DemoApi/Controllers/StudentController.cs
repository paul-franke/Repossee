using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using DemoData.Data;
using DemoData.Models;
using NLog;
using System.Web.Http;
using Repository.Repository;


namespace DemoAPI.Controllers
{

    public class StudentController : BaseController
    {
        public StudentController() : base() { }

        private Student GetData(StudentDTO source, Student dest)
        {
            dest.Id = source.Id;
            dest.Description = source.Description;
            dest.PassportNumber = source.PassportNumber;
            dest.Name = source.Name;
            dest.Mentor_Id = source.Mentor_Id;
            dest.Level_Id = source.Level_Id;
            return dest;
        }
        private StudentDTO GetData(Student source, StudentDTO dest)
        {
            dest.Id = source.Id;
            dest.Description = source.Description;
            dest.PassportNumber = source.PassportNumber;
            dest.Name = source.Name;
            dest.Mentor_Id = source.Mentor_Id;
            dest.Level_Id = source.Level_Id;
            return dest;
        }
        private StudentDTO MakeStudentDTO(Student chemRecord, List<int> CourseIds)
        {
            StudentDTO result = GetData(chemRecord, new StudentDTO());
            result.CourseIds = CourseIds;
            return result;
        }

        private List<dynamic[]> Convert1DTo2D(List<int> oneDList)
        {
            var lstOfArray = new List<dynamic[]>();
            for (var x = 0; x < oneDList.Count; x++)
            {
                dynamic[] y = new dynamic[1];
                y[0] = oneDList[x];
                lstOfArray.Add(y);
            }
            return lstOfArray;
        }
        private List<int> Convert2DTo1D(List<dynamic[]> twoDList)
        {
            var lstOfInt = new List<int>();
            foreach (var entry in twoDList)
                lstOfInt.Add(entry[0]);
            return lstOfInt;
        }

        private void Put_MKeysInDTO(StudentDTO dto, List<int> keys)
        {
            dto.CourseIds = keys;
        }

        private List<int> Get_MKeysFromDTO(StudentDTO dto)
        {
            return dto.CourseIds;
        }

        public async Task<IHttpActionResult> Get(int id = 0)
        {
            string methodNameStr = $"StudentController().Get({id})";
            try
            {
                logger.Log(LogLevel.Info, $"{MakeLogStr4Entry(methodNameStr)}.");
                using (var context = new SampDB())
                {
                    var repos = new GenericDataRepository(context);
                    var res = await repos.GetAsync<Student>(id);
                    if (res != null)
                    {   var reslist = new List<StudentDTO>();
                        foreach (var record in res)
                        {
                            List<dynamic[]> tmpCourseIds = (List < dynamic[] > )await repos.GetPKs4EntityManyAsync<Student, Course>(record);
                            var CourseIds = Convert2DTo1D(tmpCourseIds);
                            reslist.Add(MakeStudentDTO(record, CourseIds));

                        }
                        logger.Log(LogLevel.Info, $"{MakeLogStr4Exit(methodNameStr)} {reslist}");
                        return Content(HttpStatusCode.OK, reslist);
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
            string methodNameStr = $"StudentController().Delete({id})";
          try
            {

                logger.Log(LogLevel.Info, $"{MakeLogStr4Entry(methodNameStr)}.");
                using (var context = new SampDB())
                {
                    var repos = new GenericDataRepository(context);
                    var res = await repos.DeleteAsync<Student>(id);
                    if (res == HttpStatusCode.OK)
                        await context.SaveChangesAsync();
                    logger.Log(LogLevel.Info, $"{MakeLogStr4Exit(methodNameStr)} {res}");
                    return  Content(res,res.ToString());
                }
            }
            catch (Exception e)
            {
                logger.Log(LogLevel.Error, $"{MakeLogStr4Exit(methodNameStr)}:\r\n{e}");
                return  Content(HttpStatusCode.InternalServerError, HttpStatusCode.InternalServerError.ToString());
            }
        }

         public async Task<IHttpActionResult> Post([FromBody] StudentDTO StudentDTO)
        {
            string methodNameStr = $"StudentController().post()";
            logger.Log(LogLevel.Info, $"{MakeLogStr4Entry(methodNameStr)} {StudentDTO}.");

            if (!ModelState.IsValid)
            {
                logger.Log(LogLevel.Info, $"{MakeLogStr4Exit(methodNameStr)} {HttpStatusCode.BadRequest}: Modelstate is not valid.");
                return Content(HttpStatusCode.BadRequest, "The validation of the Data Transfer Object fails.");
            }
            using (var context = new SampDB())
            {
                using (var transaction = context.Database.BeginTransaction())
                {
                    try
                    {
                        List<int> CourseIds = Get_MKeysFromDTO(StudentDTO);
                        var StudentData = GetData(StudentDTO, new Student());
                        var repos = new GenericDataRepository(context);

                        var httpStatusCode = await repos.InsertAsync<Student>(StudentData);
                        if (httpStatusCode == HttpStatusCode.OK)
                        {
                            context.SaveChanges();
                            httpStatusCode = await repos.UpdateJoinEntityAsync<Student, Course>(StudentData, Convert1DTo2D(CourseIds));
                            if (httpStatusCode == HttpStatusCode.OK)
                            {
                                context.SaveChanges();
                                transaction.Commit();
                            }
                            else
                            {
                                transaction.Rollback();
                            }
                        }

                        logger.Log(LogLevel.Info, $"{MakeLogStr4Exit(methodNameStr)} {httpStatusCode}.");

                        if (httpStatusCode == HttpStatusCode.OK)
                            return Content(httpStatusCode, StudentData.Id.ToString());
                        else
                           return Content(httpStatusCode, httpStatusCode.ToString());
                    }
                    catch (Exception e)
                    {
                        transaction.Rollback();
                        logger.Log(LogLevel.Error, $"{MakeLogStr4Exit(methodNameStr)}:\r\n{e}");
                        return Content(HttpStatusCode.InternalServerError, HttpStatusCode.InternalServerError.ToString());
                    }
                }
            }
        }

        public async Task<IHttpActionResult> Put([FromBody] StudentDTO StudentDTO)
        {
            string methodNameStr = $"StudentController().put()";
            logger.Log(LogLevel.Error, $"{MakeLogStr4Entry(methodNameStr)} {StudentDTO}.");

            if (!ModelState.IsValid)
            {
                logger.Log(LogLevel.Info, $"{MakeLogStr4Exit(methodNameStr)} {HttpStatusCode.BadRequest}: Modelstate is not valid.");
                return Content(HttpStatusCode.BadRequest, "Modelstate is not valid.");
            }

            using (var context = new SampDB())
            {
                try
                {
                    List<int> CourseIds = Get_MKeysFromDTO(StudentDTO);
                    var StudentData = GetData(StudentDTO, new Student());
                    var repos = new GenericDataRepository(context);

                    var httpStatusCode = await repos.UpdateJoinEntityAsync<Student, Course>(StudentData, Convert1DTo2D(CourseIds));

                    if (httpStatusCode == HttpStatusCode.OK)
                    {
                        context.SaveChanges();
                        StudentData = GetData(StudentDTO, StudentData);
                        httpStatusCode = await repos.UpdateAsync<Student>(StudentData);
                        if (httpStatusCode == HttpStatusCode.OK)
                            context.SaveChanges();
                    }

                    logger.Log(LogLevel.Info, $"{MakeLogStr4Exit(methodNameStr)} {httpStatusCode}.");

                    if (httpStatusCode == HttpStatusCode.OK)
                        return Content(httpStatusCode, httpStatusCode.ToString());
                    else
                        return Content(httpStatusCode, httpStatusCode.ToString());
                }
                catch (Exception e)
                {
                    logger.Log(LogLevel.Error, $"{MakeLogStr4Exit(methodNameStr)}:\r\n{e}");
                    return Content(HttpStatusCode.InternalServerError, HttpStatusCode.InternalServerError.ToString());
                }
            }


        }

    }
}
