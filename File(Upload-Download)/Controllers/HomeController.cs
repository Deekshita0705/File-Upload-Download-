using File_Upload_Download_.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Collections.Generic;
using System.Data.SqlClient;


namespace File_Upload_Download_.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        static string constr = @"Data Source=DKOTHA-L-5509\SQLEXPRESS;Initial Catalog=Files;User ID=sa;Password=Welcome2evoke@1234";
        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View(PopulateFiles());
        }
        [HttpPost]
        public IActionResult Index(List<IFormFile> postedFiles)
        {
           
            foreach (IFormFile postedFile in postedFiles)

            {
                var extension = postedFile.ContentType;
                if (postedFile != null)
                {
                    
                    if (extension.ToLower().Equals("application/pdf") || extension.ToLower().Equals("image/png") || extension.ToLower().Equals("image/jpeg") || extension.ToLower().Equals("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"))
                    {
                        string fileName = Path.GetFileName(postedFile.FileName);
                        string type = postedFile.ContentType;
                        byte[] bytes = null;
                        using (MemoryStream ms = new MemoryStream())
                        {
                            postedFile.CopyTo(ms);
                            bytes = ms.ToArray();
                        }
                        using (SqlConnection con = new SqlConnection(constr))
                        {
                            using (SqlCommand cmd = new SqlCommand())
                            {
                                cmd.CommandText = "INSERT INTO TableFiles(Name, ContentType, Data) VALUES (@Name, @ContentType, @Data)";
                                cmd.Parameters.AddWithValue("@Name", fileName);
                                cmd.Parameters.AddWithValue("@ContentType", type);
                                cmd.Parameters.AddWithValue("@Data", bytes);
                                cmd.Connection = con;
                                con.Open();
                                cmd.ExecuteNonQuery();
                                con.Close();
                            }
                        }
                    }
                    else
                    {
                        ViewBag.Message = $"This file{postedFile.FileName} is not allowed to upload";

                    }
                }
                
            }
            return View(PopulateFiles());
        }        

        public FileResult DownloadFile(int fileId)
        {
            FileModel model = PopulateFiles().Find(x => x.Id == Convert.ToInt32(fileId));
            string fileName = model.Name;
            string contentType = model.ContentType;
            byte[] bytes = model.Data;
            return File(bytes, contentType, fileName);
        }
        public ActionResult DeleteFile(int fileId)
        {
            FileModel filetoremove = PopulateFiles().Find(x => x.Id == Convert.ToInt32(fileId));
            if (filetoremove != null)
            {
                using (SqlConnection con = new SqlConnection(constr))
                {
                    using (SqlCommand cmd = new SqlCommand())
                    {
                        cmd.CommandText = "Delete from TableFiles where Id = @Id";
                        cmd.Parameters.AddWithValue("@Id", fileId);
                        cmd.Connection=con;
                        con.Open();
                        cmd.ExecuteNonQuery();
                        con.Close();

                    }
                }
            }
            return RedirectToAction("Index");
        }

        private static List<FileModel> PopulateFiles()
        {
            List<FileModel> files = new List<FileModel>();
            using (SqlConnection con = new SqlConnection(constr))
            {
                string query = "SELECT * FROM TableFiles";
                using (SqlCommand cmd = new SqlCommand(query))
                {
                    cmd.Connection = con;
                    con.Open();
                    using (SqlDataReader sdr = cmd.ExecuteReader())
                    {
                        while (sdr.Read())
                        {
                            files.Add(new FileModel
                            {
                                Id = Convert.ToInt32(sdr["Id"]),
                                Name = sdr["Name"].ToString(),
                                ContentType = sdr["ContentType"].ToString(),
                                Data = (byte[])sdr["Data"]
                            });
                        }
                    }
                    con.Close();
                }
            }

            return files;
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}