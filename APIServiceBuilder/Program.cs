using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using cpModel.Models;
using cpDataServices.Models;
using HandlebarsDotNet;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata;
using cpDataORM.Models;

namespace APIServiceBuilder
{
    class Program
    {
        static void Main(string[] args)
        {

            RunMethods();
        }

        private static void RunMethods()
        {
            List<string> lstLines = new List<string>() { "Press", "- l to list build classes", "- a to list all classes",
                "- x to list excluded classes", "-b to build", "-d to build DataSource class for Dto", "-s to build service register class"
                , "- p to build project delete method", "-q to quit" };
            Console.WriteLine(String.Join(Environment.NewLine, lstLines));

            //Necessary to load assembly
            var dummy = new Approval();
            var dummy2 = new NcrStatusEOMStats();

            var modelClassNames = AppDomain.CurrentDomain.GetAssemblies()
                       .SelectMany(t => t.GetTypes())
                       .Where(t => t.IsClass && t.Namespace == "cpModel.Models")
                       .Select(x => x.Name).Distinct();

            var template = Handlebars.Compile(ServiceTemplate.TemplateSource);

            List<string> lstNotToListClasses = new List<string>() { "cpContext", "Entity" };
            List<string> lstExplicitIncludeClasses = ServiceTemplate.LstExplicitIncludeClasses;
            var baseClassList = modelClassNames.Where(x => !lstNotToListClasses.Any(y => x.Contains(y)) && !x.StartsWith("<")).ToList();

            List<string> lstExcludeClasses = baseClassList.Except(lstExplicitIncludeClasses).Where(x => !x.Contains("Configuration")).ToList();

            string result = Console.ReadLine();

            if (result.Equals("a", StringComparison.OrdinalIgnoreCase))
                baseClassList.ForEach(x => Console.WriteLine(x));

            if (result.Equals("l", StringComparison.OrdinalIgnoreCase))
                lstExplicitIncludeClasses.ForEach(x => Console.WriteLine(x));

            if (result.Equals("x", StringComparison.OrdinalIgnoreCase))
                Console.WriteLine(string.Join(",\r\n", lstExcludeClasses.Select(x => $"\"{x}\"")));

            if (result.Equals("b", StringComparison.OrdinalIgnoreCase))
            {
                foreach (var item in lstExplicitIncludeClasses)
                {
                    Type T = AppDomain.CurrentDomain.GetAssemblies()
                       .SelectMany(t => t.GetTypes())
                       .FirstOrDefault(t => t.IsClass && t.Namespace == "cpModel.Models" && t.Name == item);

                    ContextSetting.ConnectionString = "Data Source=dgbox\\sqlexpress;Initial Catalog=b101;Integrated Security=True;MultipleActiveResultSets=False";
                    var _context = new cpContext();
                    string entityNameWithNS = "cpModel.Models." + item;
                    var PrimaryKeyId = _context.Model.FindEntityType(entityNameWithNS).FindPrimaryKey().Properties[0].Name;


                    var fkSet = GetFKSet(item);
                    string IsProjectValidAsyncString = GetIsProjectValidAsync(item, fkSet);
                    string IsEntityUniqueAsyncString = GetIsEntityUniqueAsync(item, fkSet);
                    string CheckIdsInCurrentProjectAsync = GetCheckIdsInCurrentProjectAsync(item, fkSet);
                    string EntitiesForProject = GetEntitiesForProject(item, fkSet);
                    string renderedData = null;
                    var data = new
                    {
                        EntityName = item,
                        PrimaryKeyId,
                        IsProjectValidAsyncString,
                        IsEntityUniqueAsyncString,
                        CheckIdsInCurrentProjectAsync,
                        EntitiesForProject
                    };
                    renderedData = template(data).Replace("&amp;", "&").Replace("&gt;", ">");
                    string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "ServiceBase", $"{item}Service.cs");
                    File.WriteAllText(path, renderedData);
                }
            }

            if (result.Equals("s", StringComparison.OrdinalIgnoreCase))
            {

                var serviceClassNames = AppDomain.CurrentDomain.GetAssemblies()
                           .SelectMany(t => t.GetTypes())
                           .Where(t => t.IsClass && t.Namespace == "cpDataServices.Services")
                           .Select(x => x.Name).Distinct();
                List<string> lstScopedClasses = ServiceTemplate.LstScopedServices;
                List<string> lstServiceReg = new List<string>();
                foreach (var serviceName in serviceClassNames)
                {
                    if (!serviceName.Contains("Service")) continue;
                    if (lstScopedClasses.Contains(serviceName))
                        lstServiceReg.Add($"services.AddScoped<I{serviceName}, {serviceName}>();");
                    else lstServiceReg.Add($"services.AddTransient<I{serviceName}, {serviceName}>();");
                }

                var serviceRegTemplate = Handlebars.Compile(ServiceTemplate.serviceRegister);
                var data = new
                {
                    lstService = String.Join(Environment.NewLine, lstServiceReg)
                };

                string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "ServiceBase", $"listServices.cs");
                File.WriteAllText(path, serviceRegTemplate(data).Replace("&lt;", "<").Replace("&gt;", ">"));
            }

            if (result.Equals("p", StringComparison.OrdinalIgnoreCase))
            {
                var modelDtoClassNames = AppDomain.CurrentDomain.GetAssemblies()
                           .SelectMany(t => t.GetTypes())
                           .Where(t => t.IsClass && t.Namespace == "cpModel.Models")
                           .Select(x => x.Name).Distinct();

                List<DeleteEntityInfo> lstDeleters = new List<DeleteEntityInfo>();
                foreach (var modelDtoClassName in modelDtoClassNames)
                {
                    if (ServiceTemplate.LstExplicitExcludeProjectDeleteClasses.Contains(modelDtoClassName)) continue;
                    var explicitMethodCount = ServiceTemplate.LstExplicitOverrideProjectDeleteClasses.Where(x => x.EntityName == modelDtoClassName).Count();
                    if (explicitMethodCount > 0)
                    {
                        var explicitMethod = ServiceTemplate.LstExplicitOverrideProjectDeleteClasses.FirstOrDefault(x => x.EntityName == modelDtoClassName);
                        lstDeleters.Add(explicitMethod);
                        continue;
                    }
                    var fkSet = GetFKSet(modelDtoClassName);
                    if (fkSet == null) continue;
                    else lstDeleters.Add(GetProjectDeleteAsync(modelDtoClassName, fkSet));
                }
                foreach (DeleteEntityInfo deleteEntityInfo in lstDeleters)
                {
                    var priority = ServiceTemplate.LstExplicitPriorityProjectDeleteClasses.FirstOrDefault(x => x.EntityName == deleteEntityInfo.EntityName);
                    if (priority != null) deleteEntityInfo.Priority = priority.Priority;
                }
                lstDeleters = lstDeleters.OrderByDescending(x => x.Priority).ToList();

                var data = new
                {
                    lstSetNull = String.Join(Environment.NewLine, lstDeleters.Select(x => x.UpdateString)),
                    lstDelete = String.Join(Environment.NewLine, lstDeleters.Select(x => x.DeleteString)),
                    lstSetNullForAsync = String.Join(Environment.NewLine, lstDeleters.Select(x => x.UpdateStringForAsync)),
                    lstDeleteForAsync = String.Join(Environment.NewLine, lstDeleters.Select(x => x.DeleteStringForAsync))
                };
                var serviceRegTemplateforAsync = Handlebars.Compile(ServiceTemplate.projectDeleteMethodforAsync);
                var serviceRegTemplate = Handlebars.Compile(ServiceTemplate.projectDeleteMethod);

                string pathAsync = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "ServiceBase", $"projectMethodAsync.txt");
                File.WriteAllText(pathAsync, serviceRegTemplateforAsync(data).Replace("&lt;", "<").Replace("&gt;", ">"));
                string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "ServiceBase", $"projectMethod.txt");
                File.WriteAllText(path, serviceRegTemplate(data).Replace("&lt;", "<").Replace("&gt;", ">"));
            }

            if (result.Equals("d", StringComparison.OrdinalIgnoreCase))
            {
                var modelDtoClassNames = AppDomain.CurrentDomain.GetAssemblies()
                           .SelectMany(t => t.GetTypes())
                           .Where(t => t.IsClass && t.Namespace == "cpModel.Dtos")
                           .Select(x => x.Name).Distinct();

                foreach (var item in modelDtoClassNames)
                {
                    if (!item.Contains("Dto")) continue;
                    var dtoTemplate = Handlebars.Compile(ServiceTemplate.dataSourceModel);
                    var data = new
                    {
                        EntityDto = item
                    };
                    var renderedData = dtoTemplate(data).Replace("&amp;", "&").Replace("&gt;", ">");
                    string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "DtoLoadResultModels", $"{item}LoadResult.cs");
                    File.WriteAllText(path, renderedData);
                }
            }

            if (!result.Equals("q", StringComparison.OrdinalIgnoreCase)) RunMethods();

        }

        private static List<FKSet> GetFKSet(string item)
        {
            var _context = new cpContext();
            string entityNameWithNS = "cpModel.Models." + item;
            if (_context.Model.FindEntityType(entityNameWithNS) == null) return null;
            var projectProperty = _context.Model.FindEntityType(entityNameWithNS).GetProperties()?.FirstOrDefault(x => x.GetContainingForeignKeys().Any(y => y.PrincipalEntityType.ClrType.Name == "Project"))?.Name;
            if (!string.IsNullOrEmpty(projectProperty)) return new List<FKSet>();
            else
            {
                List<(IProperty property, string ProjectProperty)> lstLinkProjectRefs = new List<(IProperty property, string ProjectProperty)>();
                var FKProps = _context.Model.FindEntityType(entityNameWithNS).GetProperties()?.Where(x => x.GetContainingForeignKeys().Count() > 0);

                List<FKSet> lstFKSet = new List<FKSet>();
                foreach (var prop in FKProps)
                {
                    foreach (IForeignKey iForeignKey in prop.GetContainingForeignKeys())
                    {
                        try
                        {
                            var navigationProjectPropertyName = iForeignKey.PrincipalToDependent.DeclaringEntityType.GetProperties().FirstOrDefault(x => x.GetContainingForeignKeys().Any(y => y.PrincipalEntityType.ClrType.Name == "Project"))?.Name;
                            if (string.IsNullOrEmpty(navigationProjectPropertyName)) continue;
                            var navigationPropertyName = iForeignKey.DependentToPrincipal.Name;
                            var FKidName = iForeignKey.Properties[0].Name;
                            var FKPKidName = iForeignKey.DependentToPrincipal.ForeignKey.PrincipalEntityType.FindPrimaryKey().Properties[0].Name;
                            var FKEntityTypeName = iForeignKey.PrincipalEntityType.ClrType.Name;
                            FKSet fks = new FKSet()
                            {
                                navigationPropertyName = navigationPropertyName,
                                navigationProjectPropertyName = navigationProjectPropertyName,
                                FKidName = FKidName,
                                FKPKidName = FKPKidName,
                                FKEntityTypeName = FKEntityTypeName
                            };
                            lstFKSet.Add(fks);
                        }
                        catch (Exception ex)
                        {

                        }
                    }
                }

                return lstFKSet;
            }
        }
        private static string GetEntitiesForProject(string item, List<FKSet> lstFKSet)
        {
            var _context = new cpContext();
            string entityNameWithNS = "cpModel.Models." + item;
            var projectProperty = _context.Model.FindEntityType(entityNameWithNS).GetProperties()?.FirstOrDefault(x => x.GetContainingForeignKeys().Any(y => y.PrincipalEntityType.ClrType.Name == "Project"))?.Name;
            if (!string.IsNullOrEmpty(projectProperty)) return $"_context.{item}s.Where(x => x.ProjectId == ProjectId)";
            else
            {
                if (lstFKSet.Count == 0) return "";

                var lstIsUqString = new List<string>();

                foreach (FKSet fKSet in lstFKSet)
                {
                    lstIsUqString.Add($@"x.{fKSet.navigationPropertyName}.{fKSet.navigationProjectPropertyName} == ProjectId");
                }

                return $"_context.{item}s.Where(x => {String.Join(" && ", lstIsUqString)})";
            }
        }

        private static string GetIsProjectValidAsync(string item, List<FKSet> lstFKSet)
        {
            var _context = new cpContext();
            string entityNameWithNS = "cpModel.Models." + item;
            var projectProperty = _context.Model.FindEntityType(entityNameWithNS).GetProperties()?.FirstOrDefault(x => x.GetContainingForeignKeys().Any(y => y.PrincipalEntityType.ClrType.Name == "Project"))?.Name;
            if (!string.IsNullOrEmpty(projectProperty)) return "return ProjectId == entity.ProjectId;";
            else
            {
                if (lstFKSet.Count == 0) return "";

                var lstIfString = new List<string>();
                var lstProjectIdPrelimString = new List<string>();
                var lstIsProjectValidAsyncString = new List<string>();

                foreach (FKSet fKSet in lstFKSet)
                {
                    lstIfString.Add($@"entity.{fKSet.navigationPropertyName} != null");
                    lstProjectIdPrelimString.Add($@"entity.{fKSet.navigationPropertyName}.{fKSet.navigationProjectPropertyName} == ProjectId");
                    lstIsProjectValidAsyncString.Add($@"                if ((await _context.{fKSet.FKEntityTypeName}s.Where(x => x.{fKSet.FKPKidName} == entity.{fKSet.FKidName} " +
                        $@"&& x.{fKSet.navigationProjectPropertyName} == ProjectId).CountAsync()) != 1) return false;");
                }
                var prelimString = $@"if ({String.Join(" && ", lstIfString)}) return {String.Join(" && ", lstProjectIdPrelimString)};";

                return prelimString + Environment.NewLine + String.Join(Environment.NewLine, lstIsProjectValidAsyncString) + Environment.NewLine + $@"                return true;";
            }
        }

        private static string GetIsEntityUniqueAsync(string item, List<FKSet> lstFKSet)
        {
            var _context = new cpContext();
            string entityNameWithNS = "cpModel.Models." + item;
            var projectProperty = _context.Model.FindEntityType(entityNameWithNS).GetProperties()?.FirstOrDefault(x => x.GetContainingForeignKeys().Any(y => y.PrincipalEntityType.ClrType.Name == "Project"))?.Name;
            if (!string.IsNullOrEmpty(projectProperty)) return
                    $@"x.UqName == entity.UqName
                    && x.ProjectId == entity.ProjectId
                    && x.UniqueId != entity.UniqueId";
            else
            {
                if (lstFKSet.Count == 0) return "";

                var lstIsUqString = new List<string>();

                foreach (FKSet fKSet in lstFKSet)
                {
                    lstIsUqString.Add($@"x.{fKSet.FKidName} == entity.{fKSet.FKidName}");
                }
                lstIsUqString.Add($@"x.UniqueId != entity.UniqueId");

                return String.Join(" &&" + Environment.NewLine + @"                  ", lstIsUqString);
            }
        }

        private static string GetCheckIdsInCurrentProjectAsync(string item, List<FKSet> lstFKSet)
        {
            var _context = new cpContext();
            string entityNameWithNS = "cpModel.Models." + item;
            var projectProperty = _context.Model.FindEntityType(entityNameWithNS).GetProperties()?.FirstOrDefault(x => x.GetContainingForeignKeys().Any(y => y.PrincipalEntityType.ClrType.Name == "Project"))?.Name;
            if (!string.IsNullOrEmpty(projectProperty)) return
                    $@"x.ProjectId != ProjectId";
            else
            {
                if (lstFKSet.Count == 0) return "";

                var lstIsUqString = new List<string>();

                foreach (FKSet fKSet in lstFKSet)
                {
                    lstIsUqString.Add($@"x.{fKSet.navigationPropertyName}.{fKSet.navigationProjectPropertyName} != ProjectId");
                }

                return String.Join(" || ", lstIsUqString);
            }
        }

        private static DeleteEntityInfo GetProjectDeleteAsync(string item, List<FKSet> lstFKSet)
        {
            var _context = new cpContext();
            string entityNameWithNS = "cpModel.Models." + item;
            var projectProperty = _context.Model.FindEntityType(entityNameWithNS).GetProperties()?.FirstOrDefault(x => x.GetContainingForeignKeys().Any(y => y.PrincipalEntityType.ClrType.Name == "Project"))?.Name;
            if (!string.IsNullOrEmpty(projectProperty))
                return new DeleteEntityInfo(item, $@"x=>x.ProjectId == ProjectId", 1);

            else
            {
                if (lstFKSet.Count == 0) return new DeleteEntityInfo(item, $@"x=> x.#.ProjectId == ProjectId", 3);

                var lstIsUqString = new List<string>();

                foreach (FKSet fKSet in lstFKSet)
                {
                    lstIsUqString.Add($@"(x.{fKSet.navigationPropertyName}.{fKSet.navigationProjectPropertyName} == ProjectId)");
                }
                if (lstIsUqString.Count == 0) return new DeleteEntityInfo(item, $@"x=>{lstIsUqString[0]}", 2);
                var joinedString = String.Join(" || ", lstIsUqString);
                return new DeleteEntityInfo(item, $@"x=>{joinedString}", 2);
            }
        }
    }

    public class FKSet
    {
        public string navigationPropertyName { get; set; }
        public string navigationProjectPropertyName { get; set; }
        public string FKidName { get; set; }
        public string FKPKidName { get; set; }
        public string FKEntityTypeName { get; set; }
        public FKSet()
        {

        }
    }

}
