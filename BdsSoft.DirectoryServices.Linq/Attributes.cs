﻿/*
 * LINQ to Active Directory
 * http://www.codeplex.com/LINQtoAD
 * 
 * Copyright Bart De Smet (C) 2007
 * info@bartdesmet.net - http://blogs.bartdesmet.net/bart
 * 
 * This project is subject to licensing restrictions. Visit http://www.codeplex.com/LINQtoAD/Project/License.aspx for more information.
 */

#region Namespace imports

using System;

#endregion

namespace BdsSoft.DirectoryServices.Linq
{
    /// <summary>
    /// Specifies the directory schema to query.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1019:DefineAccessorsForAttributeArguments"), AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public sealed class DirectorySchemaAttribute : Attribute
    {
        #region Constructors

        /// <summary>
        /// Creates a new schema indicator attribute.
        /// </summary>
        /// <param name="schema">Name of the schema to query for.</param>
        public DirectorySchemaAttribute(string schema)
        {
            Schema = schema;
        }

        /// <summary>
        /// Creates a new schema indicator attribute.
        /// </summary>
        /// <param name="schema">Name of the schema to query for.</param>
        /// <param name="activeDsHelperType">Helper type for Active DS object properties.</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "Ds")]
        public DirectorySchemaAttribute(string schema, Type activeDsHelperType)
        {
            Schema = schema;
            ActiveDsHelperType = activeDsHelperType;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Name of the schema to query for.
        /// </summary>
        public string Schema { get; private set; }

        /// <summary>
        /// Helper type for Active DS object properties.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "Ds")]
        public Type ActiveDsHelperType { get; set; }

        #endregion
    }

    /// <summary>
    /// Specifies the underlying attribute to query for in the directory.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1019:DefineAccessorsForAttributeArguments"), AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public sealed class DirectoryAttributeAttribute : Attribute
    {
        #region Constructors

        /// <summary>
        /// Creates a new attribute binding attribute for a entity class field or property.
        /// </summary>
        /// <param name="attribute">Name of the attribute to query for.</param>
        public DirectoryAttributeAttribute(string attribute)
        {
            Attribute = attribute;
            QuerySource = DirectoryAttributeType.Ldap;
        }

        /// <summary>
        /// Creates a new attribute binding attribute for a entity class field or property.
        /// </summary>
        /// <param name="attribute">Name of the attribute to query for.</param>
        /// <param name="querySource">Type of the underlying query source to get the attribute from.</param>
        public DirectoryAttributeAttribute(string attribute, DirectoryAttributeType querySource)
        {
            Attribute = attribute;
            QuerySource = querySource;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Name of the attribute to query for.
        /// </summary>
        public string Attribute { get; private set; }

        /// <summary>
        /// Type of the underlying query source to get the attribute from.
        /// </summary>
        public DirectoryAttributeType QuerySource { get; set; }

        #endregion
    }

    /// <summary>
    /// Type of the query source to perform queries with.
    /// </summary>
    public enum DirectoryAttributeType
    {
        /// <summary>
        /// Default value. Uses the Properties collection of DirectoryEntry to get data from.
        /// </summary>
        Ldap,

        /// <summary>
        /// Uses Active DS Helper IADs* objects to get data from.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "Ds")]
        ActiveDs
    }
}