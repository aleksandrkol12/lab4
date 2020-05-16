using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Text.Json;
using System.IO;
using System.Text;

namespace lab4
{
    public class Startup
    {

        public void ConfigureServices(IServiceCollection services)
        {
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapGet("/", async context =>
                {
                    await context.Response.WriteAsync("Hello World!");
                });

                endpoints.MapGet("/students", async context =>
                {
                    await context.Response.WriteAsync(await PrintStudentsAsync());
                });

                endpoints.MapPost("/students", async context =>
                {
                    await context.Response.WriteAsync(await PostStudentAsync(context));
                });

                endpoints.MapGet("/students/{id}", async context =>
                {
                    context.Response.ContentType = "text/html; charset=utf-8";
                    await context.Response.WriteAsync(await PrintStudentAsync(context));
                });

                endpoints.MapDelete("/students/{id}", async context =>
                {
                    context.Response.ContentType = "text/html; charset=utf-8";
                    await context.Response.WriteAsync(await DeleteStudentAsync(context));
                });

                endpoints.MapPut("/students/{id}", async context =>
                {
                    context.Response.ContentType = "text/html; charset=utf-8";
                    await context.Response.WriteAsync(await PutStudentAsync(context));
                });
            });
        }

        public async Task<string> PutStudentAsync(HttpContext context)
        {
            string res = "";

            string json = null;

            int id = Int32.Parse(context.Request.Path.ToString().Substring(context.Request.Path.ToString().LastIndexOf('/') + 1));

            List<Student> students;

            using (FileStream fs = new FileStream("students.json", FileMode.Open))
            {
                students = await JsonSerializer.DeserializeAsync<List<Student>>(fs);
            }

            int lastId = students.Count > 0 ? students[students.Count - 1].Id.Value : 0;

            if (id <= lastId && id > 0 && lastId != 0)
            {
                using (StreamReader rdr = new StreamReader(context.Request.Body))
                {
                    json = await rdr.ReadToEndAsync();
                }

                Student putStudent = JsonSerializer.Deserialize<Student>(json);

                if (putStudent.FirstName != null)
                {
                    students.Where(p => p.Id == id).First().FirstName = putStudent.FirstName;
                    students.Where(p => p.Id == id).First().UpdatedAt = DateTime.UtcNow;
                    res += "The 'FirstName' field has been edited\n";
                }

                if (putStudent.LastName != null)
                {
                    students.Where(p => p.Id == id).First().LastName = putStudent.LastName;
                    students.Where(p => p.Id == id).First().UpdatedAt = DateTime.UtcNow;
                    res += "The 'LastName' field has been edited\n";
                }

                if (putStudent.Group != null)
                {
                    students.Where(p => p.Id == id).First().Group = putStudent.Group;
                    students.Where(p => p.Id == id).First().UpdatedAt = DateTime.UtcNow;
                    res += "The 'Group' field has been edited\n";
                }

                using (FileStream fs = new FileStream("students.json", FileMode.Create))
                {
                    await JsonSerializer.SerializeAsync<List<Student>>(fs, students);
                }
            }
            else
                res = "error";

            return await Task.FromResult(res);
        }

        public async Task<string> PostStudentAsync(HttpContext context)
        {
            string json = "";

            using (StreamReader rdr = new StreamReader(context.Request.Body))
            {
                json = await rdr.ReadToEndAsync();
            }

            Student student = JsonSerializer.Deserialize<Student>(json);
            student.CreatedAt = DateTime.UtcNow;

            FileInfo fileInfo = new FileInfo("students.json");

            if (fileInfo.Length == 0)
            {
                student.Id = 1;
                List<Student> students = new List<Student>();
                students.Add(student);

                using (FileStream fs = new FileStream("students.json", FileMode.Create))
                {
                    await JsonSerializer.SerializeAsync<List<Student>>(fs, students);
                }
            }
            else
            {
                List<Student> students;

                using (FileStream fs = new FileStream("students.json", FileMode.Open))
                {
                    students = await JsonSerializer.DeserializeAsync<List<Student>>(fs);
                }

                student.Id = students[students.Count - 1].Id + 1;

                students.Add(student);

                using (FileStream fs = new FileStream("students.json", FileMode.Create))
                {
                    await JsonSerializer.SerializeAsync<List<Student>>(fs, students);
                }
            }

            return await Task.FromResult("ok");
        }

        public async Task<string> PrintStudentsAsync()
        {
            string res = "";

            using (FileStream fs = new FileStream("students.json", FileMode.Open))
            {
                using (StreamReader rdr = new StreamReader(fs))
                {
                    res = await rdr.ReadToEndAsync();
                }
            }

            return await Task.FromResult(JsonPrettyPrint(res));
        }

        public async Task<string> PrintStudentAsync(HttpContext context)
        {
            int id = Int32.Parse(context.Request.Path.ToString().Substring(context.Request.Path.ToString().LastIndexOf('/') + 1));

            string res = "The student is not find";

            List<Student> students;

            using (FileStream fs = new FileStream("students.json", FileMode.Open))
            {
                students = await JsonSerializer.DeserializeAsync<List<Student>>(fs);
            }

            int lastId = students.Count > 0 ? students[students.Count - 1].Id.Value : 0;

            if (id <= lastId && id > 0 && lastId != 0)
            {
                Student student = students.Where(p => p.Id == id).First();
                res = JsonPrettyPrint(JsonSerializer.Serialize<Student>(student, new JsonSerializerOptions
                {
                    IgnoreNullValues = false,
                    WriteIndented = true
                }));
            }

            return await Task.FromResult(res);
        }

        public async Task<string> DeleteStudentAsync(HttpContext context)
        {
            int id = Int32.Parse(context.Request.Path.ToString().Substring(context.Request.Path.ToString().LastIndexOf('/') + 1));

            string res = null;

            List<Student> students;

            using (FileStream fs = new FileStream("students.json", FileMode.Open))
            {
                students = await JsonSerializer.DeserializeAsync<List<Student>>(fs);
            }

            int lastId = students[students.Count - 1].Id.Value;

            if (id <= lastId && id > 0)
            {
                students.Remove(students.Where(p => p.Id == id).First());

                using (FileStream fs = new FileStream("students.json", FileMode.Create))
                {
                    await JsonSerializer.SerializeAsync<List<Student>>(fs, students);
                }

                res = "The student was removed";
            }
            else
                res = "The student is not find";

            return await Task.FromResult(res);
        }

        public string JsonPrettyPrint(string json)
        {
            if (string.IsNullOrEmpty(json))
                return string.Empty;

            json = json.Replace(Environment.NewLine, "").Replace("\t", "");

            StringBuilder sb = new StringBuilder();
            bool quote = false;
            bool ignore = false;
            int offset = 0;
            int indentLength = 3;

            foreach (char ch in json)
            {
                switch (ch)
                {
                    case '"':
                        if (!ignore) quote = !quote;
                        break;
                    case '\'':
                        if (quote) ignore = !ignore;
                        break;
                }

                if (quote)
                    sb.Append(ch);
                else
                {
                    switch (ch)
                    {
                        case '{':
                        case '[':
                            sb.Append(ch);
                            sb.Append(Environment.NewLine);
                            sb.Append(new string(' ', ++offset * indentLength));
                            break;
                        case '}':
                        case ']':
                            sb.Append(Environment.NewLine);
                            sb.Append(new string(' ', --offset * indentLength));
                            sb.Append(ch);
                            break;
                        case ',':
                            sb.Append(ch);
                            sb.Append(Environment.NewLine);
                            sb.Append(new string(' ', offset * indentLength));
                            break;
                        case ':':
                            sb.Append(ch);
                            sb.Append(' ');
                            break;
                        default:
                            if (ch != ' ') sb.Append(ch);
                            break;
                    }
                }
            }

            return sb.ToString().Trim();
        }
    }
}