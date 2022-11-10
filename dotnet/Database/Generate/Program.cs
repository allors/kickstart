using Allors.Database.Meta;
using Allors.Meta.Generation.Model;

namespace Allors.Meta.Generation
{
    using System;
    using System.IO;


    class Program
    {
        private static readonly MetaBuilder MetaBuilder = new MetaBuilder();

        private static int Main()
        {
            string[,] database =
            {
                { "../allors/dotnet/Core/Database/Templates/domain.cs.stg", "DataBase/Domain/Generated" },
                { "../allors/dotnet/Core/Database/Templates/uml.cs.stg", "DataBase/Diagrams/Generated" },
            };

            string[,] workspace =
            {
                { "../allors/dotnet/Core/Workspace/Templates/meta.cs.stg", "Workspace/Meta/Generated" },
                { "../allors/dotnet/Core/Workspace/Templates/meta.lazy.cs.stg", "Workspace/Meta.Lazy/Generated" },
                { "../allors/dotnet/Core/Workspace/Templates/domain.cs.stg", "Workspace/Domain/Generated" },

                { "../allors/typescript/modules/templates/workspace.meta.ts.stg", "../typescript/modules/libs/intranet/workspace/meta/src/lib/generated" },
                { "../allors/typescript/modules/templates/workspace.meta.json.ts.stg", "../typescript/modules/libs/intranet/workspace/meta-json/src/lib/generated" },
                { "../allors/typescript/modules/templates/workspace.domain.ts.stg", "../typescript/modules/libs/intranet/workspace/domain/src/lib/generated" },

                { "../allors/typescript/modules/templates/workspace.meta.ts.stg", "../typescript/modules/libs/extranet/workspace/meta/src/lib/generated" },
                { "../allors/typescript/modules/templates/workspace.meta.json.ts.stg", "../typescript/modules/libs/extranet/workspace/meta-json/src/lib/generated" },
                { "../allors/typescript/modules/templates/workspace.domain.ts.stg", "../typescript/modules/libs/extranet/workspace/domain/src/lib/generated" },
            };

            var metaPopulation = MetaBuilder.Build();
            var model = new MetaModel(metaPopulation);

            for (var i = 0; i < database.GetLength(0); i++)
            {
                var template = database[i, 0];
                var output = database[i, 1];

                Console.WriteLine("-> " + output);

                RemoveDirectory(output);

                var log = Generate.Execute(model, template, output);
                if (log.ErrorOccured)
                {
                    return 1;
                }
            }

            var workspaceName = "Default";

            for (var i = 0; i < workspace.GetLength(0); i++)
            {
                var template = workspace[i, 0];
                var output = workspace[i, 1];

                Console.WriteLine("-> " + output);

                RemoveDirectory(output);

                var log = Generate.Execute(model, template, output, workspaceName);
                if (log.ErrorOccured)
                {
                    return 1;
                }
            }

            return 0;
        }

        private static void RemoveDirectory(string output)
        {
            var directoryInfo = new DirectoryInfo(output);
            if (directoryInfo.Exists)
            {
                try
                {
                    directoryInfo.Delete(true);
                }
                catch
                {
                }
            }
        }
    }
}
