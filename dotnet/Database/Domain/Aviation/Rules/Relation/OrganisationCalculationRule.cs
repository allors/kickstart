// <copyright file="OrganisationCalculationRule.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Allors.Database.Meta;
using Allors.Database.Derivations;
using Allors.Database.Domain.Derivations.Rules;

namespace Allors.Database.Domain
{
    public class OrganisationCalculationRule : Rule
    {
        public OrganisationCalculationRule(MetaPopulation m) : base(m, new Guid("c47e4675-83fd-4c47-97dc-651d9a636bd1")) =>
            this.Patterns = new Pattern[]
            {
                m.Organisation.RolePattern(v => v.CleaningCalculation),
                m.Organisation.RolePattern(v => v.SundriesCalculation),
            };

        public override void Derive(ICycle cycle, IEnumerable<IObject> matches)
        {
            var validation = cycle.Validation;

            foreach (var @this in matches.Cast<Organisation>())
            {
                if (@this.ExistCleaningCalculation)
                {
                    this.DeriveCleaningCalculation(@this, validation);
                }

                if (@this.ExistSundriesCalculation)
                {
                    this.DeriveSundriesCalculation(@this, validation);
                }
            }
        }

        private void DeriveCleaningCalculation(Organisation @this, IValidation validation)
        {
            var expression = @this.CleaningExpression;
            if (expression.HasErrors())
            {
                validation.AddError(@this, @this.Meta.CleaningCalculation, expression.Error);
            }
            else
            {
                var matches = Settings.ParameterExtractionRegex.Matches(@this.CleaningCalculation);
                foreach (Match match in matches)
                {
                    var parameter = match.Groups[1].Value;
                    int parameterNumber;
                    if (!int.TryParse(parameter, out parameterNumber))
                    {
                        validation.AddError(@this, @this.Meta.CleaningCalculation, "illegal parameter: " + parameter);
                    }
                    else
                    {
                        if (parameterNumber != 1)
                        {
                            validation.AddError(@this, @this.Meta.CleaningCalculation, "not implemented: " + parameterNumber);
                        }
                    }
                }
            }
        }

        private void DeriveSundriesCalculation(Organisation @this, IValidation validation)
        {
            var expression = @this.SundriesExpression;
            if (expression.HasErrors())
            {
                validation.AddError(@this, @this.Meta.SundriesCalculation, expression.Error);
            }
            else
            {
                var matches = Settings.ParameterExtractionRegex.Matches(@this.SundriesCalculation);
                foreach (Match match in matches)
                {
                    var parameter = match.Groups[1].Value;
                    int parameterNumber;
                    if (!int.TryParse(parameter, out parameterNumber))
                    {
                        validation.AddError(@this, @this.Meta.SundriesCalculation, "illegal parameter: " + parameter);
                    }
                    else
                    {
                        if (parameterNumber != 1)
                        {
                            validation.AddError(@this, @this.Meta.SundriesCalculation, "not implemented: " + parameterNumber);
                        }
                    }
                }
            }
        }
    }
}
