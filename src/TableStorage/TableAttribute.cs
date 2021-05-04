﻿using System;
using System.Text.RegularExpressions;

namespace Devlooped
{
    /// <summary>
    /// Overrides the default table name to use when persisting an entity to 
    /// table storage without providing an explicit table name.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    partial class TableAttribute : Attribute
    {
        static readonly Regex validator = new Regex("^[A-Za-z][A-Za-z0-9]{2,62}$", RegexOptions.Compiled);

        /// <summary>
        /// Initializes the attribute with the table name to use by default.
        /// </summary>
        public TableAttribute(string name)
        {
            if (!validator.IsMatch(name))
                throw new ArgumentException($"Table name '{name}' contains invalid characters.", nameof(name));

            Name = name;
        }

        /// <summary>
        /// The default table name to use.
        /// </summary>
        public string Name { get; }
    }
}
