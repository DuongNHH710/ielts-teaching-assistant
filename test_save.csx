#r "d:\Project\ielts-teaching-assistant\src\IeltsTeachingAssistant\bin\x64\Debug\net8.0-windows10.0.22621.0\IeltsTeachingAssistant.dll"
using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using IeltsTeachingAssistant.Data;
using IeltsTeachingAssistant.ViewModels;
using IeltsTeachingAssistant.Models;
using Microsoft.EntityFrameworkCore;

var services = new ServiceCollection();
services.AddDbContext<AppDbContext>(options => options.UseSqlite("Data Source=test.db"));
services.AddSingleton<IeltsTeachingAssistant.Services.VertexAIService>();
services.AddTransient<WritingEvaluationViewModel>();
var sp = services.BuildServiceProvider();

var db = sp.GetRequiredService<AppDbContext>();
db.Database.EnsureCreated();
db.Students.Add(new Student { Name = "Test", Class = new ClassEntity { Name = "Test Class" } });
db.SaveChanges();

var vm = sp.GetRequiredService<WritingEvaluationViewModel>();
vm.LoadData();
vm.Evaluation.Student = vm.Students[0];

try {
    Console.WriteLine("Saving...");
    await vm.SaveSessionAsync();
    Console.WriteLine("Done saving. Error: " + vm.ErrorMessage);
} catch (Exception ex) {
    Console.WriteLine("CRASH: " + ex.ToString());
}
