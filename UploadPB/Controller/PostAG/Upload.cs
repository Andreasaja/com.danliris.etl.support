using System;
using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using OfficeOpenXml;
using UploadPB.Models;
using UploadPB.Services.Class;
using UploadPB.Tools;
using Microsoft.Extensions.DependencyInjection;
using UploadPB.Services.Interfaces.IPostBC30Service.PostAG;

namespace UploadPB.Controller.PostBC.PostAG
{
    public class UploadAG
    {
        public IUploadExcel30AG _uploadExcel30;

        ConverterChecker converterChecker = new ConverterChecker();

        public UploadAG(IUploadExcel30AG uploadExcel30)
        {
            _uploadExcel30 = uploadExcel30;
        }

        [FunctionName("UploadBC-AG")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            req.HttpContext.Response.Headers.Add("Access-Control-Allow-Origin", "*");

            const string EXTENSION = ".xlsx";

            try
            {
                var formdata = await req.ReadFormAsync();
                IFormFile file = req.Form.Files["file"];
                req.Form.TryGetValue("type", out Microsoft.Extensions.Primitives.StringValues type);


                ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

                if (System.IO.Path.GetExtension(file.FileName) == EXTENSION)
                {

                    using (var excelPack = new ExcelPackage())
                    {
                        using (var stream = file.OpenReadStream())
                        {
                            excelPack.Load(stream);

                        }
                        var sheet = excelPack.Workbook.Worksheets;
                        var sheetData = sheet[0];
                        var typeFromSheet = converterChecker.GenerateValueString(sheetData.Cells[2, 2]);
                        try
                        {
                        
                            
                            if (type == "30" && typeFromSheet == "30")
                            {
                                await _uploadExcel30.Upload(sheet);
                            }
                         
                            else if (type != typeFromSheet)
                            {
                                throw new Exception("Harap Upload File Sesuai Dengan Tipe BC yang Dipilih");
                            }
                            return new OkObjectResult(new ResponseSuccess("success"));
                        }
                        catch (Exception ex)
                        {
                            return new BadRequestObjectResult(new ResponseFailed(ex.Message, ex.Data));
                        }
                    }
                }             
            }
            catch (Exception ex)
            {
                return new BadRequestObjectResult(new ResponseFailed("Gagal menyimpan data", ex));
            }

            return new OkObjectResult(new ResponseSuccess("success"));
        }
    }
}
