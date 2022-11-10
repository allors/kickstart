// <copyright file="Expression.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    using Meta;
    using Allors.Database.Domain.Derivations.Rules;

    public class Expression
    {
        private static readonly object locker = new object();

        private readonly NCalc.Expression expression;

        public Expression(string calculation)
        {
            lock (locker)
            {
                this.expression = new NCalc.Expression(calculation);
            }
        }

        public string Error
        {
            get
            {
                lock (locker)
                {
                    return this.expression.Error;
                }
            }
        }

        public bool HasErrors()
        {
            lock (locker)
            {
                return this.expression.HasErrors();
            }
        }

        public void AddParameter(string key, decimal value)
        {
            lock (locker)
            {
                this.expression.Parameters.Add(key, value);
            }
        }

        public object Evaluate()
        {
            lock (locker)
            {
                return this.expression.Evaluate();
            }
        }
    }
}
