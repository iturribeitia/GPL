using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GPL
{
    /// <summary>
    /// enum to handle Http Verbs.
    /// </summary>
    public enum HttpVerb
    {
        GET,
        POST,
        PUT,
        DELETE
    }

    /// <summary>
    /// enum to handle autentication type.
    /// </summary>
    public enum AuthenticationType
    {
        Basic,
        NTLM
    }
}

/// <summary>
/// enum to handle subscriber status.
/// </summary>
public enum SubscriberStatus
{
    ACTIVE,
    INACTIVE,
    OPT_OUT
}

/// <summary>
/// enum to handle subscriber genders.
/// </summary>
public enum SubscriberGender
{
    Male,
    Female,
}

/// <summary>
/// A list of data providers
/// </summary>
public enum Providers
{
    /// <summary>
    /// SQL server
    /// </summary>
    SqlServer,
    /// <summary>
    /// OLE database
    /// </summary>
    OleDB,
    /// <summary>
    /// ODBC
    /// </summary>
    ODBC,
    /// <summary>
    /// Oracle
    /// </summary>
    Oracle,
}