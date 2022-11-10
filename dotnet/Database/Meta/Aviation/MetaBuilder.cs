// <copyright file="MetaBuilder.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

using System;
using System.Linq;

namespace Allors.Database.Meta
{
    public partial class MetaBuilder
    {
        static void AddWorkspace(Class @class, string workspaceName) => @class.AssignedWorkspaceNames = (@class.AssignedWorkspaceNames ?? Array.Empty<string>()).Append(workspaceName).Distinct().ToArray();

        static void AddWorkspace(MethodType methodType, string workspaceName) => methodType.AssignedWorkspaceNames = (methodType.AssignedWorkspaceNames ?? Array.Empty<string>()).Append(workspaceName).Distinct().ToArray();

        static void AddWorkspace(RelationType relationType, string workspaceName) => relationType.AssignedWorkspaceNames = (relationType.AssignedWorkspaceNames ?? Array.Empty<string>()).Append(workspaceName).Distinct().ToArray();

        private void BuildAviation(MetaPopulation meta, Domains domains, RelationTypes relationTypes, MethodTypes methodTypes)
        {
            relationTypes.SerialisedItemSerialNumber.RoleType.IsRequired = false;
            relationTypes.SerialisedItemOwnedBy.RoleType.IsRequired = false;
            relationTypes.SerialisedItemOwnership.RoleType.IsRequired = false;

            relationTypes.WorkRequirementFixedAsset.RoleType.IsRequired = true;
            relationTypes.WorkRequirementLocation.RoleType.IsRequired = true;
            relationTypes.RequirementReason.RoleType.IsRequired = true;

            DefaultWorkspace(meta, relationTypes, methodTypes);
            this.ExtranetWorkspace(meta, relationTypes, methodTypes);
        }

        private static void DefaultWorkspace(MetaPopulation meta, RelationTypes relationTypes, MethodTypes methodTypes)
        {
            // Methods
            AddWorkspace(methodTypes.DeletableDelete, "Default");

            AddWorkspace(methodTypes.PrintablePrint, "Default");

            // Relations
            AddWorkspace(relationTypes.CountryName, "Default");

            AddWorkspace(relationTypes.CurrencyIsoCode, "Default");

            AddWorkspace(relationTypes.EnumerationName, "Default");
            AddWorkspace(relationTypes.EnumerationIsActive, "Default");

            AddWorkspace(relationTypes.LanguageName, "Default");

            AddWorkspace(relationTypes.LocaleName, "Default");
            AddWorkspace(relationTypes.LocaleCountry, "Default");
            AddWorkspace(relationTypes.LocaleLanguage, "Default");

            AddWorkspace(relationTypes.LocalisedLocale, "Default");

            AddWorkspace(relationTypes.ObjectStateName, "Default");

            AddWorkspace(relationTypes.PersonFirstName, "Default");
            AddWorkspace(relationTypes.PersonLastName, "Default");
            AddWorkspace(relationTypes.PersonMiddleName, "Default");

            AddWorkspace(relationTypes.UserUserEmail, "Default");
            AddWorkspace(relationTypes.UserUserName, "Default");

            AddWorkspace(relationTypes.UserGroupName, "Default");

            AddWorkspace(relationTypes.RoleName, "Default");

            // Objects
            AddWorkspace(meta.WorkTask, "Default");

            // Classes
            // TODO: Optimize
            foreach (Class @class in meta.Classes)
            {
                if (@class.RoleTypes.Any(v => v.AssignedWorkspaceNames.Contains("Default")) ||
                    @class.AssociationTypes.Any(v => v.AssignedWorkspaceNames.Contains("Default")) ||
                    @class.MethodTypes.Any(v => v.AssignedWorkspaceNames.Contains("Default")))
                {
                    AddWorkspace(@class, "Default");
                }
            }
        }

        private void ExtranetWorkspace(MetaPopulation meta, RelationTypes relationTypes, MethodTypes methodTypes)
        {
            const string workspaceName = "Extranet";

            // Relations
            AddWorkspace(relationTypes.CountryName, workspaceName);
            AddWorkspace(relationTypes.CurrencyIsoCode, workspaceName);
            AddWorkspace(relationTypes.EnumerationName, workspaceName);
            AddWorkspace(relationTypes.EnumerationIsActive, workspaceName);
            AddWorkspace(relationTypes.LanguageName, workspaceName);
            AddWorkspace(relationTypes.LocaleName, workspaceName);
            AddWorkspace(relationTypes.LocaleCountry, workspaceName);
            AddWorkspace(relationTypes.LocaleLanguage, workspaceName);
            AddWorkspace(relationTypes.LocalisedLocale, workspaceName);
            AddWorkspace(relationTypes.ObjectStateName, workspaceName);
            AddWorkspace(relationTypes.PersonFirstName, workspaceName);
            AddWorkspace(relationTypes.PersonLastName, workspaceName);
            AddWorkspace(relationTypes.PersonMiddleName, workspaceName);
            AddWorkspace(relationTypes.UserUserEmail, workspaceName);
            AddWorkspace(relationTypes.UserUserName, workspaceName);
            AddWorkspace(relationTypes.RoleName, workspaceName);

            // Classes
            AddWorkspace(meta.Country, workspaceName);
            AddWorkspace(meta.Currency, workspaceName);
            AddWorkspace(meta.Language, workspaceName);
            AddWorkspace(meta.Locale, workspaceName);
            AddWorkspace(meta.Person, workspaceName);
            AddWorkspace(meta.Role, workspaceName);
            AddWorkspace(meta.WorkTask, workspaceName);
        }
    }
}